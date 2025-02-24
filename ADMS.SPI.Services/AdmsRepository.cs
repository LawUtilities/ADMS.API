using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.ResourceParameters;

using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

//using System.Reflection.Metadata;

namespace ADMS.API.Services
{
    /// <summary>
    /// Adms Repository containing implementation details.
    /// </summary>
    /// <remarks>
    /// Adms Context constructor
    /// </remarks>
    /// <param name="logger">logging mechanism</param>
    /// <param name="context">context to use</param>
    /// <param name="mapper">Data Mapper to convert from Dto to entity class</param>
    /// <param name="propertyMappingService">property mapping service</param>
    public class AdmsRepository(
        ILogger<AdmsRepository> logger,
        AdmsContext context,
        IMapper mapper,
        IPropertyMappingService propertyMappingService) : IAdmsRepository
    {
        private readonly ILogger<AdmsRepository> _logger = logger;
        private readonly AdmsContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly IPropertyMappingService _propertyMappingService = propertyMappingService;

        // TODO: Add config for these parameters
        private const string ServerFilesPath = @"C:\Dev\Repos\ADMSServerFiles";

        #region Documents

        /// <summary>
        /// Add document to existing task.
        /// </summary>
        /// <param name="matterId">matter to add document to</param>
        /// <param name="document">document to add</param>
        /// <returns></returns>
        public async Task<Document?> AddDocumentAsync(
            Guid matterId,
            DocumentForCreationDto document)
        {
            try
            {
                var newDocument = _mapper.Map<Document>(document);
                // TODO: Add appropriate username code after authentication
                User? user = _context
                    .Users
                    .FirstOrDefault(u => u.Name == "rbrown");
                DocumentActivity? documentActivity = _context
                    .DocumentActivities
                    .FirstOrDefault(da => da.Activity == "CREATED");

                if (documentActivity == null || user == null)
                {
                    return await Task.FromResult<Document?>(null);
                }

                Matter matterToAddTo = await _context
                    .Matters
                    .Where(m => m.Id == matterId)
                    .SingleAsync();

                newDocument.MatterId = matterToAddTo.Id;
                newDocument.Matter = matterToAddTo;

                foreach (var revision in newDocument.Revisions)
                {
                    revision.Id = Guid.NewGuid();
                }

                var createdDocument = await _context
                    .Documents
                    .AddAsync(newDocument);

                DocumentActivityUser dau = new()
                {
                    Document = createdDocument.Entity,
                    DocumentId = createdDocument.Entity.Id,
                    DocumentActivity = documentActivity,
                    DocumentActivityId = documentActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                };

                if (_context.Entry(dau.DocumentActivity).State != EntityState.Detached)
                {
                    _context.Entry(dau.DocumentActivity).State = EntityState.Unchanged;
                }
                if (_context.Entry(dau.User).State != EntityState.Detached)
                {
                    _context.Entry(dau.User).State = EntityState.Unchanged;
                }

                await _context.DocumentActivityUsers.AddAsync(dau);

                return (await SaveChangesAsync()) 
                    ? createdDocument.Entity
                    : await Task.FromResult<Document?>(null);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error adding document: {document} to matter: {matterId}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Add document
        /// </summary>
        /// <param name="document">document to be added</param>
        /// <exception cref="ArgumentNullException">thrown if document is null</exception>
        public void AddDocument(Document document)
        {
            ArgumentNullException.ThrowIfNull(document);

            // the repository fills the id (instead of using identity columns)
            document.Id = Guid.NewGuid();

            _context.Documents.Add(document);
        }

        /// <summary>
        /// Checks in document after editing
        /// </summary>
        /// <param name="documentId">document to be checked in</param>
        /// <returns>true if document checked in, false otherwise</returns>
        public async Task<bool> CheckinDocumentAsync(
            Guid documentId)
        {
            try
            {
                // TODO: Add appropriate username code after authentication
                Document? document = await _context
                    .Documents
                    .FirstOrDefaultAsync(d => d.Id == documentId);
                User? user = _context
                    .Users
                    .FirstOrDefault(u => u.Name == "rbrown");
                DocumentActivity? documentActivity = _context
                    .DocumentActivities
                    .FirstOrDefault(da => da.Activity == "CHECKED IN");

                if (document == null || 
                    documentActivity == null || 
                    user == null)
                {
                    return false;
                }

                document.IsCheckedOut = false;

                DocumentActivityUser dau = new()
                {
                    Document = document,
                    DocumentId = document.Id,
                    DocumentActivity = documentActivity,
                    DocumentActivityId = documentActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                };

                if (_context.Entry(dau.DocumentActivity).State != EntityState.Detached)
                {
                    _context.Entry(dau.DocumentActivity).State = EntityState.Unchanged;
                }
                if (_context.Entry(dau.User).State != EntityState.Detached)
                {
                    _context.Entry(dau.User).State = EntityState.Unchanged;
                }

                await _context.DocumentActivityUsers.AddAsync(dau);

                return (await SaveChangesAsync());
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error checking in document with id: {documentId}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Check out document for editing
        /// </summary>
        /// <param name="documentId">document to be checked out</param>
        /// <returns>true if checked out, false otherwise</returns>
        public async Task<bool> CheckoutDocumentAsync(
            Guid documentId)
        {
            try
            {
                // TODO: Add appropriate username code after authentication
                Document? document = await _context
                    .Documents
                    .FirstOrDefaultAsync(d => d.Id == documentId);
                User? user = _context
                    .Users
                    .FirstOrDefault(u => u.Name == "rbrown");
                DocumentActivity? documentActivity = _context
                    .DocumentActivities
                    .FirstOrDefault(da => da.Activity == "CHECKED OUT");

                if (document == null || 
                    documentActivity == null || 
                    user == null)
                {
                    return false;
                }

                document.IsCheckedOut = true;

                DocumentActivityUser dau = new()
                {
                    Document = document,
                    DocumentId = document.Id,
                    DocumentActivity = documentActivity,
                    DocumentActivityId = documentActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                };

                if (_context.Entry(dau.DocumentActivity).State != EntityState.Detached)
                {
                    _context.Entry(dau.DocumentActivity).State = EntityState.Unchanged;
                }
                if (_context.Entry(dau.User).State != EntityState.Detached)
                {
                    _context.Entry(dau.User).State = EntityState.Unchanged;
                }

                await _context.DocumentActivityUsers.AddAsync(dau);

                return (await SaveChangesAsync());
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error checking out document in with id: {documentId}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Deletes a specified document
        /// </summary>
        /// <param name="document">document to be deleted</param>
        public async Task<bool> DeleteDocumentAsync(
            DocumentDto document)
        {
            try
            {
                Document dbDocument = await _context
                    .Documents
                    .Where(d => d.Id == document.Id)
                    .SingleAsync();

                if (dbDocument == null)
                {
                    return false;
                }
                dbDocument.IsDeleted = true;
                EntityEntry<Document> updateResult = _context
                    .Documents
                    .Update(dbDocument);

                // TODO: Add appropriate username code after authentication
                User? user = _context
                    .Users
                    .FirstOrDefault(u => u.Name == "rbrown");
                DocumentActivity? documentActivity = _context
                    .DocumentActivities
                    .FirstOrDefault(da => da.Activity == "DELETED");

                if (documentActivity == null || user == null)
                {
                    return await Task.FromResult(false);
                }

                DocumentActivityUser dau = new()
                {
                    Document = dbDocument,
                    DocumentId = dbDocument.Id,
                    DocumentActivity = documentActivity,
                    DocumentActivityId = documentActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                };

                if (_context.Entry(dau.DocumentActivity).State != EntityState.Detached)
                {
                    _context.Entry(dau.DocumentActivity).State = EntityState.Unchanged;
                }
                if (_context.Entry(dau.User).State != EntityState.Detached)
                {
                    _context.Entry(dau.User).State = EntityState.Unchanged;
                }

                await _context.DocumentActivityUsers.AddAsync(dau);

                return (await SaveChangesAsync());
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error deleting document: {document}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Document existence check
        /// </summary>
        /// <param name="documentId">document to check existence of</param>
        /// <returns>true if document exists, false otherwise</returns>
        public async Task<bool> CheckDocumentExistsAsync(
            Guid documentId)
        {
            try
            {
                return await _context
                    .Documents
                    .AsNoTracking()
                    .AnyAsync(d => d.Id == documentId);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error checking for document with id: {documentId}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Gets documents and conditionally revisions based on a selected document id
        /// </summary>
        /// <param name="documentId">docuent to retrieve</param>
        /// <param name="includeRevisions">include revisions with return data</param>
        /// <returns>Document</returns>
        public async Task<Document?> GetDocumentAsync(
            Guid documentId,
            bool includeRevisions)
        {
            try
            {
                if (includeRevisions)
                {
                    return await _context
                        .Documents
                        .AsNoTracking()
                        .Include(d => d.Revisions)
                        .Where(d => d.Id == documentId)
                        .OrderBy(d => d.FileName)
                        .AsSplitQuery()
                        .SingleAsync();
                }

                return await _context
                    .Documents
                    .AsNoTracking()
                    .Where(d => d.Id == documentId)
                    .OrderBy(d => d.FileName)
                    .SingleAsync();
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error getting document with id: {documentId}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Get Document
        /// </summary>
        /// <param name="documentId">document to retrieve</param>
        /// <returns>Document</returns>
        /// <exception cref="ArgumentNullException">thrown if documentId is null</exception>
        public async Task<Document> GetDocumentAsync(
            Guid documentId)
        {
            if (documentId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(documentId));
            }

#pragma warning disable CS8603 // possible null reference return
            return await _context.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
        }

        /// <summary>
        /// Gets a documnt by the filename entered
        /// </summary>
        /// <param name="matterId">matter containing document(s)</param>
        /// <param name="fileName">filename required</param>
        /// <returns>Diocument matching file name</returns>
        public async Task<Document?> GetDocumentByFileNameAsync(
            Guid matterId,
            string fileName)
        {
            try
            {
                if (!await _context
                    .Documents
                    .Select(d => d.MatterId == matterId && d.FileName == fileName)
                    .AnyAsync())
                {
                    return await Task.FromResult<Document?>(null);
                }

                IQueryable<Document> collection = _context.Documents;
                collection = collection
                    .Where(d => d.MatterId == matterId);
                collection = collection
                    .Where(d => d.FileName == fileName);

                return await collection
                    .AsNoTracking()
                    .SingleAsync();
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error getting document with filename: {fileName}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Gets a llist of documents without filter
        /// </summary>
        /// <param name="matterId">matter containing document(s)</param>
        /// <param name="includeDeleted">includes deleted documents</param>
        /// <returns>list of documents</returns>
        public async Task<IEnumerable<Document?>> GetDocumentsAsync(
            Guid matterId,
            bool includeDeleted = false)
        {
            try
            {
                IQueryable<Document> collection = _context.Documents;
                collection = collection
                    .Where(d => d.MatterId == matterId);

                if (includeDeleted)
                {
                    return await collection
                        .OrderBy(d => d.FileName)
                        .ToListAsync();
                }

                collection = collection
                    .Where(d => d.IsDeleted == includeDeleted);
                return await collection
                    .AsNoTracking()
                    .OrderBy(d => d.FileName)
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error getting document list with matterId: {matterId}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Get list of documents
        /// </summary>
        /// <param name="documentsResourceParameters">search parameters to locate</param>
        /// <returns>paged list of documents</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<PagedList<Document>> GetDocumentsAsync(
            DocumentsResourceParameters documentsResourceParameters)
        {
            ArgumentNullException.ThrowIfNull(documentsResourceParameters);

            //if (string.IsNullOrWhiteSpace(authorsResourceParameters.MainCategory)
            //    && string.IsNullOrWhiteSpace(authorsResourceParameters.SearchQuery))
            //{
            //    return await GetAuthorsAsync();
            //}

            // collection to start from
            var collection = _context.Documents as IQueryable<Document>;

            if (!string.IsNullOrWhiteSpace(documentsResourceParameters.FileName))
            {
                var fileName = documentsResourceParameters.FileName.Trim();
                collection = collection.Where(d => d.FileName == fileName);
            }

            if (!string.IsNullOrWhiteSpace(documentsResourceParameters.SearchQuery))
            {
                var searchQuery = documentsResourceParameters.SearchQuery.Trim();
                collection = collection.Where(d => d.FileName.Contains(searchQuery));
            }

            if (!string.IsNullOrWhiteSpace(documentsResourceParameters.OrderBy))
            {
                // get property mapping dictionary
                var documentPropertyMappingDictionary = _propertyMappingService
                    .GetPropertyMapping<DocumentDto, Document>();

                collection = collection.ApplySort(documentsResourceParameters.OrderBy,
                    documentPropertyMappingDictionary);
            }

            return await PagedList<Document>.CreateAsync(collection,
                 documentsResourceParameters.PageNumber,
                 documentsResourceParameters.PageSize);
        }

        /// <summary>
        /// Gets document
        /// </summary>
        /// <param name="matterId">matter containing document(s)</param>
        /// <param name="fileName">filename to search for</param>
        /// <param name="searchQuery">additional parameters to search for</param>
        /// <param name="includeDeleted">include deleted documents</param>
        /// <param name="pageNumber">page number in use</param>
        /// <param name="pageSize">page size in use</param>
        /// <returns>List of documents and pagination metadata</returns>
        public async Task<(IEnumerable<Document>, PaginationMetadata)> GetDocumentsAsync(
                    Guid matterId,
                    string? fileName,
                    string? searchQuery,
                    bool includeDeleted,
                    int pageNumber,
                    int pageSize)
        {
            try
            {
                IQueryable<Document> collection = _context.Documents;

                collection.Include(d => d.Revisions);

                collection = collection
                    .Where(d => d.MatterId == matterId);
                if (!includeDeleted)
                {
                    collection = collection
                        .Where(d => d.IsDeleted == includeDeleted);
                }

                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = fileName.Trim();
                    collection = collection
                        .Where(d => d.FileName == fileName);
                }

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    searchQuery = searchQuery.Trim();
                    collection = collection
                        .Where(d => d.FileName.Contains(searchQuery)
                    || (d.Extension != null && d.FileName.Contains(searchQuery)));
                }

                int totalItemCount = await collection.CountAsync();

                PaginationMetadata paginationMetadata = new(
                    totalItemCount, pageSize, pageNumber
                    );

                List<Document> collectionToReturn = await collection
                    .AsNoTracking()
                    .Include(c => c.Revisions)
                    .OrderBy(c => c.FileName)
                    .Skip(pageSize * (pageNumber - 1))
                    .Take(pageSize)
                    .AsSplitQuery()
                    .ToListAsync();

                return (collectionToReturn, paginationMetadata);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error getting paginated document list with matterId: {matterId}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Retrieves document history by specified Id
        /// </summary>
        /// <param name="id">document history to retrieve</param>
        public async Task<IEnumerable<DocumentActivityUser>> GetDocumentWithHistoryByIdAsync(
            Guid id)
        {
            try
            {
                IQueryable<DocumentActivityUser> documentWithActivityAndHistory = _context.DocumentActivityUsers;

                documentWithActivityAndHistory = documentWithActivityAndHistory.Where(dau => dau.DocumentId == id);
                documentWithActivityAndHistory = documentWithActivityAndHistory.Include(dau => dau.User);
                documentWithActivityAndHistory = documentWithActivityAndHistory.Include(dau => dau.Document);
                documentWithActivityAndHistory = documentWithActivityAndHistory.Include(dau => dau.DocumentActivity);

                return await documentWithActivityAndHistory
                    .AsNoTracking()
                    .OrderBy(dau => dau.CreatedAt)
                    .AsSplitQuery()
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error getting document with historical data with id: {id}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Updates a specified document with new data
        /// </summary>
        /// <param name="document">Document to be updated</param>
        /// <returns>true if updated, false otherwise</returns>
        public async Task<Document?> UpdateDocumentAsync(
            Document document)
        {
            try
            {
                EntityEntry<Document> updatedDocument = _context.Documents.Update(document);

                User? user = _context
                    .Users
                    .FirstOrDefault(u => u.Name == "rbrown");
                DocumentActivity? documentActivity = _context
                    .DocumentActivities
                    .FirstOrDefault(ra => ra.Activity == "SAVED");

                if (user == null || documentActivity == null)
                {
                    return await Task.FromResult<Document?>(null);
                }

                DocumentActivityUser dau = new()
                {
                    DocumentId = document.Id,
                    Document = document,
                    DocumentActivityId = documentActivity.Id,
                    DocumentActivity = documentActivity,
                    CreatedAt = DateTime.UtcNow,
                    UserId = user.Id,
                    User = user,
                };

                if (_context.Entry(dau.DocumentActivity).State != EntityState.Detached)
                {
                    _context.Entry(dau.DocumentActivity).State = EntityState.Unchanged;
                }
                if (_context.Entry(dau.User).State != EntityState.Detached)
                {
                    _context.Entry(dau.User).State = EntityState.Unchanged;
                }

                _context.DocumentActivityUsers.Add(dau);

                return (await SaveChangesAsync())
                    ? dau.Document
                    : await Task.FromResult<Document?>(null);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error updating document with id: {document.Id}",
                                    args: [exception.StackTrace]);
                throw;
            }

        }

        #endregion Documents

        #region Document Activity

        /// <summary>
        /// Retrieves document activity by activity
        /// </summary>
        /// <param name="activity">activity to retrieve</param>
        /// <returns>DocumentActivity</returns>
        public async Task<DocumentActivity?> GetDcumentActivityByActivity(
            string activity)
        {
            try
            {
                return await _context
                    .DocumentActivities
                    .Where(da => da.Activity == activity)
                    .SingleAsync();
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error getting document activity with activity: {activity}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Add Document Audit record
        /// </summary>
        /// <param name="audit">audit record to be added</param>
        /// <exception cref="ArgumentNullException">audit is null</exception>
        public void AddDocumentAudit(DocumentActivityUser audit)
        {
            ArgumentNullException.ThrowIfNull(audit);

            _context.DocumentActivityUsers.Add(audit);
        }

        /// <summary>
        /// Get document audit history
        /// </summary>
        /// <param name="documentId">document to retrieve audit history for</param>
        /// <returns>list of document activity user records</returns>
        public async Task<IEnumerable<DocumentActivityUser>> GetDocumentAuditsAsync(Guid documentId)
        {
            ArgumentNullException.ThrowIfNull(documentId);

            IQueryable<DocumentActivityUser> collection = _context.DocumentActivityUsers;

            collection = collection.Where(dau => dau.DocumentId == documentId);

            collection = collection.OrderBy(dau => dau.CreatedAt);

            List<DocumentActivityUser> collectionToReturn = await collection
                                    .AsNoTracking()
                                    .Include(dau => dau.User)
                                    .Include(dau => dau.DocumentActivity)
                                        .IgnoreAutoIncludes()
                                    .AsSplitQuery()
                                    .ToListAsync();

            return collectionToReturn;
        }

        #endregion Document Activity

        #region Matters

        /// <summary>
        /// Add a matter to the matters collection
        /// </summary>
        /// <param name="matter">matter to be added</param>
        /// <returns>matter created</returns>
        public async Task<Matter?> AddMatterAsync(
            MatterDto matter)
        {
            try
            {
                if (matter == null ||
                    await CheckMatterExists(matter.Description))
                {
                    return await Task.FromResult<Matter?>(null);
                }

                // TODO: Add appropriate username code after authentication
                User? user = _context
                    .Users
                    .FirstOrDefault(u => u.Name == "rbrown");
                MatterActivity? matterActivity = _context
                    .MatterActivities
                    .FirstOrDefault(ma => ma.Activity == "CREATED");

                if (matterActivity == null || 
                    user == null)
                {
                    return await Task.FromResult<Matter?>(null);
                }

                EntityEntry<Matter> newMatter = await _context
                    .Matters
                    .AddAsync(_mapper.Map<Matter>(matter));

                MatterActivityUser mau = new()
                {
                    Matter = newMatter.Entity,
                    MatterId = newMatter.Entity.Id,
                    MatterActivity = matterActivity,
                    MatterActivityId = matterActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                };

                if (_context.Entry(mau.MatterActivity).State != EntityState.Detached)
                {
                    _context.Entry(mau.MatterActivity).State = EntityState.Unchanged;
                }
                if (_context.Entry(mau.User).State != EntityState.Detached)
                {
                    _context.Entry(mau.User).State = EntityState.Unchanged;
                }

                await _context.MatterActivityUsers.AddAsync(mau);

                if (mau.Matter.Id == Guid.Empty)
                {
                    return await Task.FromResult<Matter?>(null);
                }

                if (await SaveChangesAsync())
                {
                    Matter matterCreated = await _context
                        .Matters
                        .AsNoTracking()
                        .Where(m => m.Id == mau.MatterId)
                        .SingleAsync();

                    return matterCreated;
                }
                else
                {
                    return await Task.FromResult<Matter?>(null);
                }
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error adding matter: {matter}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Check if a matter exists
        /// </summary>
        /// <param name="matterId">matter to check</param>
        /// <returns>true if exists, false otherwise</returns>
        public async Task<bool> CheckMatterExists(
            Guid matterId)
        {
            try
            {
                return await _context
                    .Matters
                    .AsNoTracking()
                    .AnyAsync(m => m.Id == matterId);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error checking if matter exists with id: {matterId}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Check if a matter exists
        /// </summary>
        /// <param name="description">description of matter to identify</param>
        /// <returns>true if exists, false otherwise</returns>
        public async Task<bool> CheckMatterExists(
            string description)
        {
            try
            {
                return await _context
                    .Matters
                    .AsNoTracking()
                    .AnyAsync(m => m.Description == description);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error checking if matter exists with description: {description}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Deletes a metter
        /// </summary>
        /// <param name="matter">matter to be deleted</param>
        public async Task<bool> DeleteMatterAsync(
            MatterDto matter)
        {
            try
            {
                Matter dbMatter = await _context
                    .Matters
                    .Where(m => m.Id == matter.Id)
                    .FirstAsync();

                if (dbMatter == null)
                {
                    return await Task.FromResult(false);
                }

                dbMatter.IsDeleted = true;

                EntityEntry<Matter> updateResult = _context.Matters.Update(dbMatter);

                // TODO: Add appropriate username code after authentication
                User? user = _context
                    .Users
                    .FirstOrDefault(u => u.Name == "rbrown");
                MatterActivity? matterActivity = _context
                    .MatterActivities
                    .FirstOrDefault(ma => ma.Activity == "DELETED");

                if (matterActivity == null || 
                    user == null)
                {
                    return await Task.FromResult(false);
                }

                MatterActivityUser mau = new()
                {
                    Matter = dbMatter,
                    MatterId = dbMatter.Id,
                    MatterActivity = matterActivity,
                    MatterActivityId = matterActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                };

                if (_context.Entry(mau.MatterActivity).State != EntityState.Detached)
                {
                    _context.Entry(mau.MatterActivity).State = EntityState.Unchanged;
                }
                if (_context.Entry(mau.User).State != EntityState.Detached)
                {
                    _context.Entry(mau.User).State = EntityState.Unchanged;
                }
                await _context.MatterActivityUsers.AddAsync(mau);

                return await SaveChangesAsync();
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error deleting matter: {matter}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Gets list of matters ordered by description
        /// </summary>
        /// <param name="description">matter description being searched for</param>
        /// <param name="includeArchived">include archived matters in returned result</param>
        /// <param name="includeDeleted">include deleted matters in returned data</param>
        /// <returns>IEnumerable Matter</returns>
        public async Task<IEnumerable<Matter>> GetMattersAsync(
            string description,
            bool includeArchived = false,
            bool includeDeleted = false)
        {
            try
            
            {
                if (_context.Matters is not IQueryable<Matter> collection)
                {
                    return [];
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        if (includeArchived)
                        {
                            if (includeDeleted)
                            {
                                collection = collection
                                    .Where(m => m.Description.Contains(description.Trim()) &&
                                    (m.IsArchived == true || m.IsArchived == false) &&
                                    (m.IsDeleted == true || m.IsDeleted == false));
                            }
                            else
                            {
                                collection = collection
                                    .Where(m => m.Description.Contains(description.Trim()) &&
                                    (m.IsArchived == true || m.IsArchived == false) &&
                                    m.IsDeleted == false);
                            }
                        }
                        else
                        {
                            if (includeDeleted)
                            {
                                collection = collection
                                    .Where(m => m.Description.Contains(description.Trim()) &&
                                    m.IsArchived == false &&
                                    (m.IsDeleted == true || m.IsDeleted == false));
                            }
                            else
                            {
                                collection = collection
                                    .Where(m => m.Description.Contains(description.Trim()) &&
                                    m.IsArchived == false &&
                                    m.IsDeleted == false);
                            }
                        }
                    }
                    else
                    {
                        if (includeArchived)
                        {
                            if (includeDeleted)
                            {
                                collection = collection
                                    .Where(m => (m.IsArchived == true || m.IsArchived == false) &&
                                    (m.IsDeleted == true || m.IsDeleted == false));
                            }
                            else
                            {
                                collection = collection
                                    .Where(m => (m.IsArchived == true || m.IsArchived == false) &&
                                    m.IsDeleted == false);
                            }
                        }
                        else
                        {
                            if (includeDeleted)
                            {
                                collection = collection
                                    .Where(m => m.IsArchived == false &&
                                    (m.IsDeleted == true || m.IsDeleted == false));
                            }
                            else
                            {
                                collection = collection
                                    .Where(m => m.IsArchived == false &&
                                    m.IsDeleted == false);
                            }
                        }
                    }
                    var results = await collection
                        .OrderBy(m => m.Description)
                        .Include(m => m.MatterActivityUsers)
                        .AsSplitQuery()
                        .ToListAsync();

                    return results;
                }
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error getting matter list.",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Get matter by id
        /// </summary>
        /// <param name="matterId">matter id to retrieve</param>
        /// <param name="includeDocuments">include documents in returned matter</param>
        /// <returns>matter</returns>
        public async Task<Matter?> GetMatterAsync(
            Guid matterId,
            bool includeDocuments)
        {
            try
            {
                if (!await _context
                    .Matters
                    .Where(m => m.Id == matterId)
                    .AnyAsync())
                {
                    return await Task.FromResult<Matter?>(null);
                }

                if (includeDocuments)
                {
                    return await _context
                        .Matters
                        .AsNoTracking()
                        .Include(m => m.Documents.OrderBy(d => d.FileName))
                        .Where(m => m.Id == matterId)
                        .AsSplitQuery()
                        .SingleAsync();
                }
                return await _context
                    .Matters
                    .AsNoTracking()
                    .Where(m => m.Id == matterId)
                    .SingleAsync();
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error getting matter with id: {matterId}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Restore Matter by id
        /// </summary>
        /// <param name="matterId">matter id to restore</param>
        /// <returns>True if matter restored, false otherwise</returns>
        public async Task<bool> RestoreMatterAsync(
            Guid matterId)
        {
            string matterDescription = string.Empty;
            try
            {
                Matter dbMatter = await _context
                    .Matters
                    .Where(m => m.Id == matterId)
                    .SingleAsync();

                if (dbMatter == null)
                {
                    return await Task.FromResult(false);
                }

                matterDescription = dbMatter.Description;
                dbMatter.IsDeleted = false;

                EntityEntry<Matter> updateResult = _context
                    .Matters
                    .Update(dbMatter);

                // TODO: Add appropriate username code after authentication
                User? user = _context
                    .Users
                    .FirstOrDefault(u => u.Name == "rbrown");
                MatterActivity? matterActivity = _context
                    .MatterActivities
                    .FirstOrDefault(ma => ma.Activity == "RESTORED");

                if (matterActivity == null ||
                    user == null)
                {
                    return await Task.FromResult(false);
                }

                MatterActivityUser mau = new()
                {
                    Matter = dbMatter,
                    MatterId = dbMatter.Id,
                    MatterActivity = matterActivity,
                    MatterActivityId = matterActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                };

                if (_context.Entry(mau.MatterActivity).State != EntityState.Detached)
                {
                    _context.Entry(mau.MatterActivity).State = EntityState.Unchanged;
                }
                if (_context.Entry(mau.User).State != EntityState.Detached)
                {
                    _context.Entry(mau.User).State = EntityState.Unchanged;
                }
                await _context
                    .MatterActivityUsers
                    .AddAsync(mau);

                return await SaveChangesAsync();
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error restoring deleted matter with id: {matterId}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Retrieves matter history by specified Id
        /// </summary>
        /// <param name="id">matter history to retrieve</param>
        public async Task<Matter> GetMatterWithHistoryByIdAsync(
            Guid id)
        {
            try
            {
                IQueryable<Matter> MatterReturned = _context.Matters;

                MatterReturned = MatterReturned.Where(m => m.Id == id);
                MatterReturned = MatterReturned
                    .Include(m => m.MatterActivityUsers)
                        .ThenInclude(m => m.User)
                    .Include(m => m.MatterActivityUsers)
                        .ThenInclude(m => m.MatterActivity)
                    .AsSplitQuery();

                return await MatterReturned
                    .AsNoTracking()
                    .OrderBy(m => m.CreationDate)
                    .SingleAsync();
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error getting matter with historical data with id: {matterId}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Get matter document activity user from list
        /// </summary>
        /// <param name="matterId">matter to retrieve data for</param>
        /// <param name="documentId">document to retrieve data for</param>
        /// <returns>collection of matter document activity user histories</returns>
        public async Task<IEnumerable<MatterDocumentActivityUserFrom>> GetMDAUFromHistoryAsync(
            Guid matterId,
            Guid documentId)
        {
            try
            {
                IEnumerable<MatterDocumentActivityUserFrom> histories = [];
                if (matterId == Guid.Empty && documentId == Guid.Empty)
                {
                    return [];
                }
                if (matterId != Guid.Empty)
                {
                    if (documentId != Guid.Empty)
                    {
                        histories = await _context
                            .MatterDocumentActivityUsersFrom
                            .Where(mdauf => mdauf.DocumentId == documentId)
                            .Include(md => md.Matter)
                            .Include(md => md.MatterDocumentActivity)
                            .Include(md => md.Document)
                            .Include(md => md.User)
                            .OrderBy(mdauf => mdauf.CreatedAt)
                            .AsSplitQuery()
                            .ToListAsync();
                    }
                    else
                    {
                        histories = await _context
                            .MatterDocumentActivityUsersFrom
                            .Where(mdau => mdau.MatterId == matterId)
                            .Include(md => md.Matter)
                            .Include(md => md.MatterDocumentActivity)
                            .Include(md => md.Document)
                            .Include(md => md.User)
                            .OrderBy(mdau => mdau.CreatedAt)
                            .AsSplitQuery()
                            .ToListAsync();
                    }
                }
                else
                {
                    if (documentId != Guid.Empty)
                    {
                        histories = await _context
                            .MatterDocumentActivityUsersFrom
                            .Where(mdau => mdau.DocumentId == documentId)
                            .Include(md => md.Matter)
                            .Include(md => md.MatterDocumentActivity)
                            .Include(md => md.Document)
                            .Include(md => md.User)
                            .OrderBy(mdau => mdau.CreatedAt)
                            .AsSplitQuery()
                            .ToListAsync();
                    }
                }

                return histories;
            }
            catch (Exception exception)
            {
                string errorMessage = string.Empty;
                if (matterId != Guid.Empty)
                {
                    if (documentId != Guid.Empty)
                    {
                        errorMessage = "Error getting matter and document history with id's: {matterId}, {documentId}";
                    }
                    else
                    {
                        errorMessage = "Error getting matter history with id's: {matterId}";
                    }
                }
                else
                {
                    if (documentId != Guid.Empty)
                    {
                        errorMessage = "Error getting document history with id's: {documentId}";
                    }
                    else
                    {
                        errorMessage = "An error occured while retrieving data...";
                    }
                }

                _logger.LogCritical(exception: exception,
                                    message: "{errorMessage}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Get matter document activity user from list
        /// </summary>
        /// <param name="matterId">matter to retrieve data for</param>
        /// <param name="documentId">document to retrieve data for</param>
        /// <returns>collection of matter document activity user histories</returns>
        public async Task<IEnumerable<MatterDocumentActivityUserTo>> GetMDAUToHistoryAsync(
            Guid matterId,
            Guid documentId)
        {
            try
            {
                IEnumerable<MatterDocumentActivityUserTo> histories = [];
                if (matterId == Guid.Empty && documentId == Guid.Empty)
                {
                    return [];
                }
                if (matterId != Guid.Empty)
                {
                    if (documentId != Guid.Empty)
                    {
                        histories = await _context
                            .MatterDocumentActivityUsersTo
                            .Where(mdaut => mdaut.DocumentId == documentId)
                            .Include(md => md.Matter)
                            .Include(md => md.MatterDocumentActivity)
                            .Include(md => md.Document)
                            .Include(md => md.User)
                            .OrderBy(mdauf => mdauf.CreatedAt)
                            .AsSplitQuery()
                            .ToListAsync();
                    }
                    else
                    {
                        histories = await _context
                            .MatterDocumentActivityUsersTo
                            .Where(mdau => mdau.MatterId == matterId)
                            .Include(md => md.Matter)
                            .Include(md => md.MatterDocumentActivity)
                            .Include(md => md.Document)
                            .Include(md => md.User)
                            .OrderBy(mdau => mdau.CreatedAt)
                            .AsSplitQuery()
                            .ToListAsync();
                    }
                }
                else
                {
                    if (documentId != Guid.Empty)
                    {
                        histories = await _context
                            .MatterDocumentActivityUsersTo
                            .Where(mdau => mdau.DocumentId == documentId)
                            .Include(md => md.Matter)
                            .Include(md => md.MatterDocumentActivity)
                            .Include(md => md.Document)
                            .Include(md => md.User)
                            .OrderBy(mdau => mdau.CreatedAt)
                            .AsSplitQuery()
                            .ToListAsync();
                    }
                }

                return histories;
            }
            catch (Exception exception)
            {
                string errorMessage = string.Empty;
                if (matterId != Guid.Empty)
                {
                    if (documentId != Guid.Empty)
                    {
                        errorMessage = "Error getting matter and document history with id's: {matterId}, {documentId}";
                    }
                    else
                    {
                        errorMessage = "Error getting matter history with id's: {matterId}";
                    }
                }
                else
                {
                    if (documentId != Guid.Empty)
                    {
                        errorMessage = "Error getting document history with id's: {documentId}";
                    }
                    else
                    {
                        errorMessage = "An error occured while retrieving data...";
                    }
                }

                _logger.LogCritical(exception: exception,
                                    message: "{errorMessage}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Identifies if matter history exists
        /// </summary>
        /// <param name="id">Matter to check</param>
        /// <returns>true if Matter History exists, false otherwise</returns>
        public async Task<bool> DoesMatterHistoryExists(
            Guid id)
        {
            try
            {
                return await _context
                    .MatterActivityUsers
                    .AnyAsync(mau => mau.MatterId == id);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error getting matter historical data with id: {id}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        #endregion Matters

        #region MatterActivity

        /// <summary>
        /// Gets MatterActivity by activity
        /// </summary>
        /// <param name="activity">activity to retrieve</param>
        /// <returns>MatterActivity</returns>
        public async Task<MatterActivity?> GetMatterActivityByActivity(
                    string activity)
        {
            try
            {
                return await _context
                    .MatterActivities
                    .AsNoTracking()
                    .SingleAsync(m => m.Activity == activity);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error getting matter activity with activity: {activity}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        #endregion MatterActivity

        #region RevisionActivities

        /// <summary>
        /// checks if a revision activity already exists
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public async Task<bool> CheckRevisionActivityExists(
            string activity)
        {
            try
            {
                return await _context
                    .RevisionActivities
                    .AsNoTracking()
                    .AnyAsync(ra => ra.Activity == activity);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error getting revision activity with activity: {activity}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Gets revision activity for given activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public async Task<RevisionActivity?> GetRevisionActivityByActivityAsync(
            string activity)
        {
            try
            {
                return await _context
                    .RevisionActivities
                    .AsNoTracking()
                    .SingleAsync(ra => ra.Activity == activity);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error getting revision activity with activity: {activity}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Update specified revision
        /// </summary>
        /// <param name="matterId">Matter containing revision to update</param>
        /// <param name="documentId">Document containing revision to be updated</param>
        /// <param name="revisionId">Revision to be updated</param>
        /// <param name="revision">Revision containing data to be updated</param>
        /// <returns>Updated revision</returns>
        public async Task<Revision?> UpdateRevisionsAsync(
            Guid matterId, 
            Guid documentId, 
            Guid revisionId, 
            RevisionDto revision)
        {
            try
            {
                EntityEntry<Revision> newRevision = _context.Revisions.Update(_mapper.Map<Revision>(revision));

                User? user = _context
                    .Users
                    .FirstOrDefault(u => u.Name == "rbrown");
                RevisionActivity? revisionActivity = _context
                    .RevisionActivities
                    .FirstOrDefault(ra => ra.Activity == "SAVED");

                if (user == null || revisionActivity == null)
                {
                    return await Task.FromResult<Revision?>(null);
                }

                RevisionActivityUser rau = new()
                {
                    CreatedAt = DateTime.UtcNow,
                    Revision = newRevision.Entity,
                    RevisionId = newRevision.Entity.Id,
                    UserId = user.Id,
                    User = user,
                    RevisionActivityId = revisionActivity.Id,
                    RevisionActivity = revisionActivity
                };

                if (_context.Entry(rau.RevisionActivity).State != EntityState.Detached)
                {
                    _context.Entry(rau.RevisionActivity).State = EntityState.Unchanged;
                }
                if (_context.Entry(rau.User).State != EntityState.Detached)
                {
                    _context.Entry(rau.User).State = EntityState.Unchanged;
                }
                await _context
                    .RevisionActivityUsers
                    .AddAsync(rau);

                return (await SaveChangesAsync()) 
                    ? rau.Revision 
                    : await Task.FromResult<Revision?>(null);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    throw;
                }
                _logger.LogCritical(exception: exception,
                                    message: "Error updating revision: {revision}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        #endregion RevisionActivities

        #region Revisions

        /// <summary>
        /// Adds revision to document
        /// </summary>
        /// <param name="documentId">document to be added to </param>
        /// <param name="revision">revision to add</param>
        /// <returns></returns>
        public async Task<Revision?> AddRevisionAsync(
                    Guid documentId,
                    RevisionDto revision)
        {
            try
            {
                // TODO: Add appropriate username code after authentication
                Document document = await _context
                    .Documents
                    .SingleAsync(d => d.Id == documentId);
                User? user = _context
                    .Users
                    .FirstOrDefault(u => u.Name == "rbrown");
                RevisionActivity? revisionActivity = _context
                    .RevisionActivities
                    .FirstOrDefault(ra => ra.Activity == "CREATED");

                if (document == null || 
                    user == null || 
                    revisionActivity == null)
                {
                    return await Task.FromResult<Revision?>(null);
                }

                Revision dbRevision = _mapper.Map<Revision>(revision);
                dbRevision.DocumentId = documentId;
                dbRevision.Document = document;

                EntityEntry<Revision> updatedRevision = await _context
                    .Revisions
                    .AddAsync(dbRevision);

                RevisionActivityUser rau = new()
                {
                    Revision = updatedRevision.Entity,
                    RevisionId = updatedRevision.Entity.Id,
                    RevisionActivity = revisionActivity,
                    RevisionActivityId = revisionActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                };

                if (_context.Entry(rau.RevisionActivity).State != EntityState.Detached)
                {
                    _context.Entry(rau.RevisionActivity).State = EntityState.Unchanged;
                }
                if (_context.Entry(rau.User).State != EntityState.Detached)
                {
                    _context.Entry(rau.User).State = EntityState.Unchanged;
                }

                await _context.RevisionActivityUsers.AddAsync(rau);

                return (await SaveChangesAsync()) ? rau.Revision : await Task.FromResult<Revision?>(null);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    throw;
                }
                _logger.LogCritical(exception: exception,
                                    message: "Error adding revision to document with id: {documentId}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Checks if revision Id exists
        /// </summary>
        /// <param name="id">revision to check</param>
        /// <returns>true if exists, false otherwise</returns>
        public async Task<bool> CheckRevisionExistsAsync(
            Guid id)
        {
            try
            {
                return await _context
                    .Revisions
                    .AsNoTracking()
                    .AnyAsync(r => r.Id == id);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    throw;
                }
                _logger.LogCritical(exception: exception,
                                    message: "Error finding revision with id: {id}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Deletes a specified revision
        /// </summary>
        /// <param name="revision">The revision to be deleted</param>
        public async Task<bool> DeleteRevisionAsync(
            RevisionDto revision)
        {
            try
            {
                Revision dbRevision = await _context
                    .Revisions
                    .Where(r => r.Id == revision.Id)
                    .SingleAsync();

                if (dbRevision == null)
                {
                    return false;
                }

                dbRevision.IsDeleted = true;
                EntityEntry<Revision> updateResult = _context
                    .Revisions
                    .Update(dbRevision);

                // TODO: Add appropriate username code after authentication
                User? user = _context
                    .Users
                    .FirstOrDefault(u => u.Name == "rbrown");
                RevisionActivity? revisionActivity = _context
                    .RevisionActivities
                    .FirstOrDefault(ma => ma.Activity == "DELETED");

                if (user == null || 
                    revisionActivity == null)
                {
                    return await Task.FromResult(false);
                }

                RevisionActivityUser rau = new()
                {
                    Revision = dbRevision,
                    RevisionId = dbRevision.Id,
                    RevisionActivity = revisionActivity,
                    RevisionActivityId = revisionActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                };

                if (_context.Entry(rau.RevisionActivity).State != EntityState.Detached)
                {
                    _context.Entry(rau.RevisionActivity).State = EntityState.Unchanged;
                }
                if (_context.Entry(rau.User).State != EntityState.Detached)
                {
                    _context.Entry(rau.User).State = EntityState.Unchanged;
                }

                await _context
                    .RevisionActivityUsers
                    .AddAsync(rau);

                return await Task.FromResult(await SaveChangesAsync());
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    throw;
                }
                _logger.LogCritical(exception: exception,
                                    message: "Error deleting revision with id: {id}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Retrieves revision by specified Id
        /// </summary>
        /// <param name="id">revision to retrieve</param>
        public async Task<Revision?> GetRevisionByIdAsync(
            Guid id)
        {
            try
            {
                return await _context
                    .Revisions
                    .AsNoTracking()
                    .Where(r => r.Id == id)
                    .SingleAsync();
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    throw;
                }
                _logger.LogCritical(exception: exception,
                                    message: "Error getting revision with id: {id}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Retrieves revision history by specified Id
        /// </summary>
        /// <param name="id">revision to retrieve</param>
        public async Task<IEnumerable<RevisionActivityUser>> GetRevisionWithHistoryByIdAsync(
            Guid id)
        {
            try
            {
                IQueryable<RevisionActivityUser> revisionWithActivityAndHistory = _context.RevisionActivityUsers;

                revisionWithActivityAndHistory = revisionWithActivityAndHistory.Where(rau => rau.RevisionId == id);
                revisionWithActivityAndHistory = revisionWithActivityAndHistory.Include(rau => rau.User);
                revisionWithActivityAndHistory = revisionWithActivityAndHistory.Include(rau => rau.Revision);
                revisionWithActivityAndHistory = revisionWithActivityAndHistory.Include(rau => rau.RevisionActivity);

                return await revisionWithActivityAndHistory
                    .AsNoTracking()
                    .OrderBy(rau => rau.CreatedAt)
                    .AsSplitQuery()
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    throw;
                }
                _logger.LogCritical(exception: exception,
                                    message: "Error getting revision history with id: {id}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Get list of revisions
        /// </summary>
        /// <param name="documentId">document to retrieve revisions for</param>
        /// <param name="includeDeleted">include deleted revisions</param>
        /// <returns>list of revisions</returns>
        public async Task<IEnumerable<Revision>> GetRevisionsAsync(
            Guid documentId,
            bool includeDeleted = false)
        {
            try
            {
                IQueryable<Revision> collection = _context
                    .Revisions;
                collection = collection
                    .Where(r => r.DocumentId == documentId);

                if (!includeDeleted)
                {
                    collection = collection
                        .Where(r => r.IsDeleted == includeDeleted);
                }

                return await collection
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    throw;
                }
                _logger.LogCritical(exception: exception,
                                    message: "Error getting revisions with document id: {documentId}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        #endregion Revisions

        #region User Actions

        /// <summary>
        /// Gets a user by username
        /// </summary>
        /// <param name="username">username being requested</param>
        /// <returns>User</returns>
        public async Task<User?> GetUserByUsername(
            string username)
        {
            try
            {
                // TODO: Add appropriate username code after authentication
                return await _context
                    .Users
                    .AsNoTracking()
                    .SingleAsync(u => u.Name == username);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    throw;
                }
                _logger.LogCritical(exception: exception,
                                    message: "Error getting username with value: {username}",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        #endregion User Actions

        /// <summary>
        /// persists the saved data to the database
        /// </summary>
        /// <returns>true if data could be saved to the database, false otherwise</returns>
        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync() >= 0;
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    throw;
                }
                _logger.LogCritical(exception: exception,
                                    message: "Error saving changes to the database",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Copy document from one matter to another as a new document
        /// </summary>
        /// <param name="document">document to be copied</param>
        /// <param name="matterId">new matter to copy to</param>
        /// <returns>true if copied, false otherwise</returns>
        public async Task<bool> CopyDocumentAsync(
            Guid matterId,
            DocumentWithoutRevisionsDto document)
        {
            try
            {
                Document oldDocument = await _context
                    .Documents
                    .Where(d => d.Id == document.Id)
                    .SingleAsync();

                Revision lastOldRevision = await _context
                    .Revisions
                    .Where(rev => rev.DocumentId == oldDocument.Id)
                    .OrderBy(rev => rev.RevisionId)
                    .LastAsync();

                Matter oldMatter = await _context
                    .Matters
                    .Where(m => m.Id == oldDocument.MatterId)
                    .SingleAsync();

                Matter newMatter = await _context
                    .Matters
                    .Where(m => m.Id == matterId)
                    .SingleAsync();

                DocumentForCreationDto newDocument = new()
                {
                    Extension = document.Extension,
                    FileName = document.FileName,
                    IsCheckedOut = false
                };

                Document? createdDocument = await AddDocumentAsync(newMatter.Id, newDocument);

                if (createdDocument == null)
                {
                    return false;
                }

                RevisionDto revisionToAdd = new()
                {
                    RevisionId = 1,
                    CreationDate = DateTime.Now.ToUniversalTime(),
                    ModificationDate = DateTime.Now.ToUniversalTime(),
                };

                Revision? revisionCreation = await AddRevisionAsync(createdDocument.Id, revisionToAdd);

                if (revisionCreation == null)
                {
                    return false;
                }

                var originalFolderPath = Path.Combine(ServerFilesPath, "matters", $"{oldMatter.Id}");
                var newFolderPath = Path.Combine(ServerFilesPath, "matters", $"{newMatter.Id}");

                if (!Directory.Exists(originalFolderPath))
                {
                    Directory.CreateDirectory(originalFolderPath);
                }

                if (!Directory.Exists(newFolderPath))
                {
                    Directory.CreateDirectory(newFolderPath);
                }

                var originalPath = Path.Combine(originalFolderPath, $"{oldDocument.Id}R{lastOldRevision.RevisionId}{oldDocument.Extension}");
                var newPath = Path.Combine(newFolderPath, $"{createdDocument.Id}R{lastOldRevision.RevisionId}{oldDocument.Extension}");
                
                File.Copy( originalPath, newPath );

                MatterDocumentActivity matterDocumentActivity = await _context
                .MatterDocumentActivities
                .Where(mda => mda.Activity == "COPIED")
                .SingleAsync();

                User user = await _context
                    .Users
                    .Where(u => u.Name == "rbrown")
                    .SingleAsync();

                MatterDocumentActivityUserFrom newMatterDocumentActivityUserFrom = new()
                {
                    CreatedAt = DateTime.Now.ToUniversalTime(),
                    MatterDocumentActivity = matterDocumentActivity,
                    MatterDocumentActivityId = matterDocumentActivity.Id,
                    User = user,
                    UserId = user.Id,
                    Matter = oldMatter,
                    MatterId = oldMatter.Id,
                    Document = oldDocument,
                    DocumentId = oldDocument.Id
                };

                MatterDocumentActivityUserTo newMatterDocumentActivityUserTo = new()
                {
                    CreatedAt = DateTime.Now.ToUniversalTime(),
                    MatterDocumentActivity = matterDocumentActivity,
                    MatterDocumentActivityId = matterDocumentActivity.Id,
                    User = user,
                    UserId = user.Id,
                    Matter = newMatter,
                    MatterId = newMatter.Id,
                    Document = createdDocument,
                    DocumentId = createdDocument.Id
                };

                _context.Entry(newMatterDocumentActivityUserFrom.MatterDocumentActivity).State = EntityState.Unchanged;
                _context.Entry(newMatterDocumentActivityUserFrom.User).State = EntityState.Unchanged;
                _context.Entry(newMatterDocumentActivityUserTo.MatterDocumentActivity).State = EntityState.Unchanged;
                _context.Entry(newMatterDocumentActivityUserTo.User).State = EntityState.Unchanged;

                ValueTask<EntityEntry<MatterDocumentActivityUserFrom>> mdauFromSaved = _context
                    .MatterDocumentActivityUsersFrom
                    .AddAsync(newMatterDocumentActivityUserFrom);
                ValueTask<EntityEntry<MatterDocumentActivityUserTo>> mdauToSaved = _context
                    .MatterDocumentActivityUsersTo
                    .AddAsync(newMatterDocumentActivityUserTo);

                if (mdauFromSaved.IsCompleted && mdauToSaved.IsCompleted)
                {
                    return await SaveChangesAsync();
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error saving changes to the database",
                                    args: [exception.StackTrace]);
                throw;
            }
        }

        /// <summary>
        /// Move document from onw matter to another
        /// </summary>
        /// <param name="document">document to be moved</param>
        /// <param name="matterId">new matter to move document to</param>
        /// <returns>true if moved, false otherwise</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> MoveDocumentAsync(
            Guid matterId,
            DocumentWithoutRevisionsDto document)
        {
            try
            {
                Matter newMatter = await _context
                    .Matters
                    .Where(m => m.Id == matterId)
                    .SingleAsync();

                Document oldDocument = await _context
                    .Documents
                    .Where(d => d.Id == document.Id)
                    .SingleAsync();

                Revision lastOldRevision = await _context
                    .Revisions
                    .Where(rev => rev.DocumentId == oldDocument.Id)
                    .OrderBy(rev => rev.RevisionId)
                    .LastAsync();

                Matter oldMatter = await _context
                    .Matters
                    .Where(m => m.Id == oldDocument.MatterId)
                    .SingleAsync();

                if (oldDocument == null)
                {
                    return false;
                }

                oldDocument.Matter = newMatter;
                oldDocument.MatterId = newMatter.Id;

                EntityEntry<Document> updatedDocument = _context
                    .Documents
                    .Update(oldDocument);

                if (updatedDocument.State == EntityState.Modified)
                {
                    if (!await SaveChangesAsync() )
                    {
                        return false;
                    }
                }

                MatterDocumentActivity matterDocumentActivity = await _context
                    .MatterDocumentActivities
                    .AsNoTracking()
                    .Where(mda => mda.Activity == "MOVED")
                    .SingleAsync();

                User user = await _context
                    .Users
                    .AsNoTracking()
                    .Where(u => u.Name == "rbrown")
                    .SingleAsync();

                MatterDocumentActivityUserFrom newMatterDocumentActivityUserFrom = new()
                {
                    CreatedAt = DateTime.Now.ToUniversalTime(),
                    MatterDocumentActivity = matterDocumentActivity,
                    MatterDocumentActivityId = matterDocumentActivity.Id,
                    User = user,
                    UserId = user.Id,
                    Matter = oldMatter,
                    MatterId = oldMatter.Id,
                    Document = oldDocument,
                    DocumentId = oldDocument.Id
                };

                MatterDocumentActivityUserTo newMatterDocumentActivityUserTo = new()
                {
                    CreatedAt = DateTime.Now.ToUniversalTime(),
                    MatterDocumentActivity = matterDocumentActivity,
                    MatterDocumentActivityId = matterDocumentActivity.Id,
                    User = user,
                    UserId = user.Id,
                    Matter = newMatter,
                    MatterId = newMatter.Id,
                    Document = oldDocument,
                    DocumentId = oldDocument.Id
                };

                _context.Entry(newMatterDocumentActivityUserFrom.MatterDocumentActivity).State = EntityState.Unchanged;
                _context.Entry(newMatterDocumentActivityUserFrom.User).State = EntityState.Unchanged;
                _context.Entry(newMatterDocumentActivityUserTo.MatterDocumentActivity).State = EntityState.Unchanged;
                _context.Entry(newMatterDocumentActivityUserTo.User).State = EntityState.Unchanged;

                ValueTask<EntityEntry<MatterDocumentActivityUserFrom>> mdauFromSaved = _context
                    .MatterDocumentActivityUsersFrom
                    .AddAsync(newMatterDocumentActivityUserFrom);
                ValueTask<EntityEntry<MatterDocumentActivityUserTo>> mdauToSaved = _context
                    .MatterDocumentActivityUsersTo
                    .AddAsync(newMatterDocumentActivityUserTo);

                if (mdauFromSaved.IsCompleted && mdauFromSaved.IsCompleted)
                {

                    var originalFolderPath = Path.Combine(ServerFilesPath, "matters", $"{oldMatter.Id}");
                    var newFolderPath = Path.Combine(ServerFilesPath, "matters", $"{newMatter.Id}");

                    if (!Directory.Exists(originalFolderPath))
                    {
                        Directory.CreateDirectory(originalFolderPath);
                    }

                    if (!Directory.Exists(newFolderPath))
                    {
                        Directory.CreateDirectory(newFolderPath);
                    }

                    var originalPath = Path.Combine(originalFolderPath, $"{oldDocument.Id}R{lastOldRevision.RevisionId}{oldDocument.Extension}");
                    var newPath = Path.Combine(newFolderPath, $"{oldDocument.Id}R{lastOldRevision.RevisionId}{oldDocument.Extension}");

                    File.Move(originalPath, newPath);

                    return await SaveChangesAsync();
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                if (exception == null)
                    throw;
                if (string.IsNullOrEmpty(exception.StackTrace))
                    throw;
                _logger.LogCritical(exception: exception,
                                    message: "Error saving changes to the database",
                                    args: [exception.StackTrace]);
                throw;
            }
        }
    }
}