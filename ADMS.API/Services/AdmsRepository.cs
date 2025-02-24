using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Helpers;
using ADMS.API.Models;
using ADMS.API.ResourceParameters;

using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
                // TODO: Replace with dynamic username after authentication
                User? user = _context.Users.FirstOrDefault(u => u.Name == "rbrown");
                DocumentActivity? documentActivity = _context.DocumentActivities.FirstOrDefault(da => da.Activity == "CREATED");

                if (documentActivity == null || user == null)
                {
                    _logger.LogWarning("DocumentActivity or User not found.");
                    return null;
                }

                Matter matterToAddTo = await _context.Matters.SingleAsync(m => m.Id == matterId);

                newDocument.MatterId = matterToAddTo.Id;
                newDocument.Matter = matterToAddTo;

                var createdDocument = await _context
                    .Documents
                    .AddAsync(newDocument);

                createdDocument.Entity.Revisions.Add(new Revision() 
                { 
                    Id = Guid.NewGuid(),
                    CreationDate = DateTime.UtcNow,
                    ModificationDate = DateTime.UtcNow,
                    IsDeleted = false,
                    RevisionId = 1,
                    Document = createdDocument.Entity,
                    DocumentId = createdDocument.Entity.Id
                });

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

                createdDocument.Entity.DocumentActivityUsers.Add(dau);

                return await SaveChangesAsync() ? createdDocument.Entity : null;
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error adding document: {document} to matter: {matterId}", document, matterId);
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
                Document oldDocument = await _context.Documents.SingleAsync(d => d.Id == document.Id);
                Revision lastOldRevision = await _context.Revisions.Where(rev => rev.DocumentId == oldDocument.Id).OrderBy(rev => rev.RevisionId).LastAsync();
                Matter oldMatter = await _context.Matters.SingleAsync(m => m.Id == oldDocument.MatterId);
                Matter newMatter = await _context.Matters.SingleAsync(m => m.Id == matterId);

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

                if (!File.Exists(originalPath))
                {
                    return false;
                }

                File.Copy(originalPath, newPath);

                MatterDocumentActivity matterDocumentActivity = await _context.MatterDocumentActivities.SingleAsync(mda => mda.Activity == "COPIED");
                User user = await _context.Users.SingleAsync(u => u.Name == "rbrown");

                MatterDocumentActivityUserFrom newMatterDocumentActivityUserFrom = new()
                {
                    CreatedAt = DateTime.UtcNow,
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
                    CreatedAt = DateTime.UtcNow,
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

                await _context.MatterDocumentActivityUsersFrom.AddAsync(newMatterDocumentActivityUserFrom);
                await _context.MatterDocumentActivityUsersTo.AddAsync(newMatterDocumentActivityUserTo);

                return await SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error saving changes to the database");
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
                Matter newMatter = await _context.Matters.SingleAsync(m => m.Id == matterId);
                Document oldDocument = await _context.Documents.SingleAsync(d => d.Id == document.Id);
                Revision lastOldRevision = await _context.Revisions.Where(rev => rev.DocumentId == oldDocument.Id).OrderBy(rev => rev.RevisionId).LastAsync();
                Matter oldMatter = await _context.Matters.SingleAsync(m => m.Id == oldDocument.MatterId);

                if (oldDocument == null)
                {
                    return false;
                }

                oldDocument.Matter = newMatter;
                oldDocument.MatterId = newMatter.Id;

                EntityEntry<Document> updatedDocument = _context.Documents.Update(oldDocument);

                if (updatedDocument.State == EntityState.Modified)
                {
                    if (!await SaveChangesAsync())
                    {
                        return false;
                    }
                }

                MatterDocumentActivity matterDocumentActivity = await _context.MatterDocumentActivities.AsNoTracking().SingleAsync(mda => mda.Activity == "MOVED");
                User user = await _context.Users.AsNoTracking().SingleAsync(u => u.Name == "rbrown");

                MatterDocumentActivityUserFrom newMatterDocumentActivityUserFrom = new()
                {
                    CreatedAt = DateTime.UtcNow,
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
                    CreatedAt = DateTime.UtcNow,
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

                await _context.MatterDocumentActivityUsersFrom.AddAsync(newMatterDocumentActivityUserFrom);
                await _context.MatterDocumentActivityUsersTo.AddAsync(newMatterDocumentActivityUserTo);

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
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error saving changes to the database");
                throw;
            }
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
                // TODO: Replace with dynamic username after authentication
                Document? document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
                User? user = _context.Users.FirstOrDefault(u => u.Name == "rbrown");
                DocumentActivity? documentActivity = _context.DocumentActivities.FirstOrDefault(da => da.Activity == "CHECKED IN");

                if (document == null || documentActivity == null || user == null)
                {
                    _logger.LogWarning("Document, DocumentActivity, or User not found.");
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
                    CreatedAt = DateTime.UtcNow
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

                return await SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error checking in document with id: {documentId}", documentId);
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
                // TODO: Replace with dynamic username after authentication
                Document? document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
                User? user = _context.Users.FirstOrDefault(u => u.Name == "rbrown");
                DocumentActivity? documentActivity = _context.DocumentActivities.FirstOrDefault(da => da.Activity == "CHECKED OUT");

                if (document == null || documentActivity == null || user == null)
                {
                    _logger.LogWarning("Document, DocumentActivity, or User not found.");
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
                    CreatedAt = DateTime.UtcNow
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

                return await SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error checking out document with id: {documentId}", documentId);
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
                Document dbDocument = await _context.Documents.SingleAsync(d => d.Id == document.Id);

                if (dbDocument == null)
                {
                    _logger.LogWarning("Document not found.");
                    return false;
                }

                dbDocument.IsDeleted = true;
                _context.Documents.Update(dbDocument);

                // TODO: Replace with dynamic username after authentication
                User? user = _context.Users.FirstOrDefault(u => u.Name == "rbrown");
                DocumentActivity? documentActivity = _context.DocumentActivities.FirstOrDefault(da => da.Activity == "DELETED");

                if (documentActivity == null || user == null)
                {
                    _logger.LogWarning("DocumentActivity or User not found.");
                    return false;
                }

                DocumentActivityUser dau = new()
                {
                    Document = dbDocument,
                    DocumentId = dbDocument.Id,
                    DocumentActivity = documentActivity,
                    DocumentActivityId = documentActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
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

                return await SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error deleting document: {document}", document);
                throw;
            }
        }

        /// <summary>
        /// Document existence check
        /// </summary>
        /// <param name="documentId">document to check existence of</param>
        /// <returns>true if document exists, false otherwise</returns>
        public async Task<bool> DocumentExistsAsync(
            Guid documentId)
        {
            try
            {
                return await _context.Documents.AsNoTracking().AnyAsync(d => d.Id == documentId);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error checking for document with id: {documentId}", documentId);
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
                        .AsSplitQuery()
                        .SingleAsync();
                }

                return await _context
                    .Documents
                    .AsNoTracking()
                    .Where(d => d.Id == documentId)
                    .SingleAsync();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting document with id: {documentId}", documentId);
                throw;
            }
        }

        /// <summary>
        /// Get Document
        /// </summary>
        /// <param name="documentId">document to retrieve</param>
        /// <returns>Document or null if document doesn't exist</returns>
        /// <exception cref="ArgumentNullException">thrown if documentId is null</exception>
        public async Task<Document?> GetDocumentAsync(
            Guid documentId)
        {
            if (documentId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(documentId));
            }

            try
            {
                return await _context.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting document with id: {documentId}", documentId);
                throw;
            }
        }

        /// <summary>
        /// Gets a list of documnts by the filename entered
        /// </summary>
        /// <param name="matterId">matter containing document(s)</param>
        /// <param name="fileName">filename required</param>
        /// <returns>Diocument matching file name</returns>
        public async Task<IEnumerable<Document>> GetDocumentsByFileNameAsync(
            Guid matterId,
            string fileName)
        {
            try
            {
                return await _context.Documents
                    .AsNoTracking()
                    .Where(d => d.MatterId == matterId && d.FileName == fileName)
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting documents with filename: {fileName}", fileName);
                throw;
            }
        }

        /// <summary>
        /// Gets a list of documents without filter
        /// </summary>
        /// <param name="matterId">matter containing document(s)</param>
        /// <param name="includeDeleted">includes deleted documents</param>
        /// <returns>list of documents</returns>
        public async Task<IEnumerable<Document>> GetDocumentsAsync(Guid matterId, bool includeDeleted = false)
        {
            try
            {
                IQueryable<Document> collection = _context.Documents
                    .Where(d => d.MatterId == matterId);

                if (!includeDeleted)
                {
                    collection = collection.Where(d => !d.IsDeleted);
                }

                return await collection
                    .AsNoTracking()
                    .OrderBy(d => d.FileName)
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting document list with matterId: {matterId}", matterId);
                throw;
            }
        }

        /// <summary>
        /// Get list of documents
        /// </summary>
        /// <param name="matterId">matter containing document(s)</param>
        /// <param name="documentsResourceParameters">search parameters to locate</param>
        /// <returns>paged list of documents</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<PagedList<Document>> GetDocumentsAsync(
            Guid matterId,
            DocumentsResourceParameters documentsResourceParameters)
        {
            ArgumentNullException.ThrowIfNull(documentsResourceParameters);

            try
            {
                var collection = _context.Documents
                    .Where(d => d.MatterId == matterId)
                    .Include(d => d.Revisions)
                    .AsQueryable();

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
                    var documentPropertyMappingDictionary = _propertyMappingService
                        .GetPropertyMapping<DocumentDto, Document>();

                    collection = collection.ApplySort(documentsResourceParameters.OrderBy, documentPropertyMappingDictionary);
                }

                return await PagedList<Document>.CreateAsync(collection,
                    documentsResourceParameters.PageNumber,
                    documentsResourceParameters.PageSize);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting document list with matterId: {matterId}", matterId);
                throw;
            }
        }

        /// <summary>
        /// Gets a list of documents
        /// </summary>
        /// <param name="matterId">matter containing document(s)</param>
        /// <param name="documentsResourceParameters">search parameters to locate</param>
        /// <param name="includeDeleted">include deleted documents</param>
        /// <returns>List of documents and pagination metadata</returns>
        public async Task<(IEnumerable<Document>, PaginationMetadata)> GetPagedDocumentsAsync(
                    Guid matterId,
                    DocumentsResourceParameters documentsResourceParameters,
                    bool includeDeleted)
        {
            try
            {
                IQueryable<Document> collection = _context.Documents
                    .Where(d => d.MatterId == matterId)
                    .Include(d => d.Revisions);

                if (!includeDeleted)
                {
                    collection = collection.Where(d => !d.IsDeleted);
                }

                if (!string.IsNullOrWhiteSpace(documentsResourceParameters.FileName))
                {
                    var fileName = documentsResourceParameters.FileName.Trim();
                    collection = collection.Where(d => d.FileName == fileName);
                }

                if (!string.IsNullOrWhiteSpace(documentsResourceParameters.SearchQuery))
                {
                    var searchQuery = documentsResourceParameters.SearchQuery.Trim();
                    collection = collection.Where(d => d.FileName.Contains(searchQuery) ||
                                                       (d.Extension != null && d.FileName.Contains(searchQuery)));
                }

                int totalItemCount = await collection.CountAsync();

                PaginationMetadata paginationMetadata = new(
                    totalItemCount, documentsResourceParameters.PageSize, documentsResourceParameters.PageNumber);

                List<Document> collectionToReturn = await collection
                    .AsNoTracking()
                    .OrderBy(c => c.FileName)
                    .Skip(documentsResourceParameters.PageSize * (documentsResourceParameters.PageNumber - 1))
                    .Take(documentsResourceParameters.PageSize)
                    .AsSplitQuery()
                    .ToListAsync();

                return (collectionToReturn, paginationMetadata);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting paginated document list with matterId: {matterId}", matterId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves document history by specified Id
        /// </summary>
        /// <param name="documentId">document history to retrieve</param>
        public async Task<IEnumerable<DocumentActivityUser>> GetDocumentWithHistoryByIdAsync(
            Guid documentId)
        {
            try
            {
                IQueryable<DocumentActivityUser> documentWithActivityAndHistory = _context.DocumentActivityUsers
                    .Where(dau => dau.DocumentId == documentId)
                    .Include(dau => dau.User)
                    .Include(dau => dau.Document)
                    .Include(dau => dau.DocumentActivity);

                return await documentWithActivityAndHistory
                    .AsNoTracking()
                    .OrderBy(dau => dau.CreatedAt)
                    .AsSplitQuery()
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting document with historical data with id: {documentId}", documentId);
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
                _context.Documents.Update(document);

                // TODO: Replace with dynamic username after authentication
                User? user = _context.Users.FirstOrDefault(u => u.Name == "rbrown");
                DocumentActivity? documentActivity = _context.DocumentActivities.FirstOrDefault(ra => ra.Activity == "SAVED");

                if (user == null || documentActivity == null)
                {
                    _logger.LogWarning("User or DocumentActivity not found.");
                    return null;
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

                return await SaveChangesAsync() ? dau.Document : null;
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error updating document with id: {document.Id}", document.Id);
                throw;
            }
        }

        #endregion Documents

        #region Document Activity

        /// <summary>
        /// Retrieves document activity by activity name
        /// </summary>
        /// <param name="activityName">activity to retrieve</param>
        /// <returns>DocumentActivity</returns>
        public async Task<DocumentActivity> GetDocumentActivityByActivityNameAsync(
            string activityName)
        {
            try
            {
                return await _context.DocumentActivities
                    .Where(da => da.Activity == activityName)
                    .SingleAsync();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting document activity with activity: {activityName}", activityName);
                throw;
            }
        }

        /// <summary>
        /// Add Document Audit record
        /// </summary>
        /// <param name="audit">audit record to be added</param>
        /// <exception cref="ArgumentNullException">audit is null</exception>
        public async Task AddDocumentAuditAsync(DocumentActivityUser audit)
        {
            ArgumentNullException.ThrowIfNull(audit);

            await _context.DocumentActivityUsers.AddAsync(audit);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get document audit history
        /// </summary>
        /// <param name="documentId">document to retrieve audit history for</param>
        /// <returns>list of document activity user records</returns>
        public async Task<IEnumerable<DocumentActivityUserMinimalDto>> GetDocumentAuditsAsync(Guid documentId)
        {
            try
            {
                IQueryable<DocumentActivityUser> collection = _context.DocumentActivityUsers
                    .Where(dau => dau.DocumentId == documentId)
                    .OrderBy(dau => dau.CreatedAt)
                    .AsNoTracking()
                    .Include(dau => dau.User)
                    .Include(dau => dau.DocumentActivity)
                    .IgnoreAutoIncludes()
                    .AsSplitQuery();

                return _mapper.Map<IEnumerable<DocumentActivityUserMinimalDto>>(await collection.ToListAsync());
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting document audit history for document with id: {documentId}", documentId);
                throw;
            }
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
                if (matter == null || await MatterNameExistsAsync(matter.Description))
                {
                    return null;
                }

                // TODO: Replace with dynamic username after authentication
                User? user = _context.Users.FirstOrDefault(u => u.Name == "rbrown");
                MatterActivity? matterActivity = _context.MatterActivities.FirstOrDefault(ma => ma.Activity == "CREATED");

                if (matterActivity == null || user == null)
                {
                    _logger.LogWarning("User or MatterActivity not found.");
                    return null;
                }

                EntityEntry<Matter> newMatter = await _context.Matters.AddAsync(_mapper.Map<Matter>(matter));

                MatterActivityUser mau = new()
                {
                    Matter = newMatter.Entity,
                    MatterId = newMatter.Entity.Id,
                    MatterActivity = matterActivity,
                    MatterActivityId = matterActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
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
                    return null;
                }

                if (await SaveChangesAsync())
                {
                    return await _context.Matters.AsNoTracking().SingleAsync(m => m.Id == mau.MatterId);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error adding matter: {matter}", matter);
                throw;
            }
        }

        /// <summary>
        /// Check if a matter exists
        /// </summary>
        /// <param name="matterId">matter to check</param>
        /// <returns>true if exists, false otherwise</returns>
        public async Task<bool> MatterExistsAsync(
            Guid matterId)
        {
            try
            {
                return await _context.Matters
                    .AsNoTracking()
                    .AnyAsync(m => m.Id == matterId);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error checking if matter exists with id: {matterId}", matterId);
                throw;
            }
        }

        /// <summary>
        /// Check if a matter name exists
        /// </summary>
        /// <param name="matterName">name of matter to identify</param>
        /// <returns>true if exists, false otherwise</returns>
        public async Task<bool> MatterNameExistsAsync(
            string matterName)
        {
            try
            {
                return await _context.Matters
                    .AsNoTracking()
                    .AnyAsync(m => m.Description == matterName);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error checking if matter exists with id: {matterName}", matterName);
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
                if (matter == null)
                {
                    _logger.LogWarning("Matter is null.");
                    return false;
                }

                Matter? dbMatter = await _context.Matters
                    .Where(m => m.Id == matter.Id)
                    .FirstOrDefaultAsync();

                if (dbMatter == null)
                {
                    _logger.LogWarning("Matter not found.");
                    return false;
                }

                dbMatter.IsDeleted = true;
                _context.Matters.Update(dbMatter);

                // TODO: Replace with dynamic username after authentication
                User? user = _context.Users.FirstOrDefault(u => u.Name == "rbrown");
                MatterActivity? matterActivity = _context.MatterActivities.FirstOrDefault(ma => ma.Activity == "DELETED");

                if (matterActivity == null || user == null)
                {
                    _logger.LogWarning("User or MatterActivity not found.");
                    return false;
                }

                MatterActivityUser mau = new()
                {
                    Matter = dbMatter,
                    MatterId = dbMatter.Id,
                    MatterActivity = matterActivity,
                    MatterActivityId = matterActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
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
                _logger.LogCritical(exception, "Error deleting matter: {matter}", matter);
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
                IQueryable<Matter> collection = _context.Matters;

                if (!string.IsNullOrWhiteSpace(description))
                {
                    description = description.Trim();
                    collection = collection.Where(m => m.Description.Contains(description));
                }

                if (!includeArchived)
                {
                    collection = collection.Where(m => !m.IsArchived);
                }

                if (!includeDeleted)
                {
                    collection = collection.Where(m => !m.IsDeleted);
                }

                var results = await collection
                    .OrderBy(m => m.Description)
                    .Include(m => m.MatterActivityUsers)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .ToListAsync();

                return results;
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting matter list.");
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
                IQueryable<Matter> query = _context.Matters.AsNoTracking();

                if (includeDocuments)
                {
                    query = query.Include(m => m.Documents.OrderBy(d => d.FileName));
                }

                return await query
                    .AsSplitQuery()
                    .SingleOrDefaultAsync(m => m.Id == matterId);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting matter with id: {matterId}", matterId);
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
            try
            {
                Matter? dbMatter = await _context.Matters
                    .Where(m => m.Id == matterId)
                    .SingleOrDefaultAsync();

                if (dbMatter == null)
                {
                    _logger.LogWarning("Matter not found.");
                    return false;
                }

                dbMatter.IsDeleted = false;
                _context.Matters.Update(dbMatter);

                // TODO: Replace with dynamic username after authentication
                User? user = _context.Users.FirstOrDefault(u => u.Name == "rbrown");
                MatterActivity? matterActivity = _context.MatterActivities.FirstOrDefault(ma => ma.Activity == "RESTORED");

                if (matterActivity == null || user == null)
                {
                    _logger.LogWarning("User or MatterActivity not found.");
                    return false;
                }

                MatterActivityUser mau = new()
                {
                    Matter = dbMatter,
                    MatterId = dbMatter.Id,
                    MatterActivity = matterActivity,
                    MatterActivityId = matterActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
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
                _logger.LogCritical(exception, "Error restoring deleted matter with id: {matterId}", matterId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves matter history by specified Id
        /// </summary>
        /// <param name="matterId">matter history to retrieve</param>
        public async Task<Matter> GetMatterWithHistoryByIdAsync(
            Guid matterId)
        {
            try
            {
                IQueryable<Matter> matterQuery = _context.Matters
                    .Where(m => m.Id == matterId)
                    .Include(m => m.MatterActivityUsers)
                        .ThenInclude(mau => mau.User)
                    .Include(m => m.MatterActivityUsers)
                        .ThenInclude(mau => mau.MatterActivity)
                    .AsSplitQuery()
                    .AsNoTracking();

                return await matterQuery
                    .OrderBy(m => m.CreationDate)
                    .SingleOrDefaultAsync();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting matter with historical data with id: {matterId}", matterId);
                throw;
            }
        }

        /*
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
                if (matterId == Guid.Empty && documentId == Guid.Empty)
                {
                    return [];
                }

                IQueryable<MatterDocumentActivityUserFrom> query = _context.MatterDocumentActivityUsersFrom
                    .Include(md => md.Matter)
                    .Include(md => md.MatterDocumentActivity)
                    .Include(md => md.Document)
                    .Include(md => md.User)
                    .AsSplitQuery()
                    .AsNoTracking();

                if (matterId != Guid.Empty)
                {
                    query = query.Where(mdau => mdau.MatterId == matterId);
                }

                if (documentId != Guid.Empty)
                {
                    query = query.Where(mdau => mdau.DocumentId == documentId);
                }

                return await query.OrderBy(mdau => mdau.CreatedAt).ToListAsync();
            }
            catch (Exception exception)
            {
                string errorMessage = matterId != Guid.Empty && documentId != Guid.Empty
                    ? $"Error getting matter and document history with id's: {matterId}, {documentId}"
                    : matterId != Guid.Empty
                        ? $"Error getting matter history with id: {matterId}"
                        : documentId != Guid.Empty
                            ? $"Error getting document history with id: {documentId}"
                            : "An error occurred while retrieving data.";

                _logger.LogCritical(exception, errorMessage);
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
                if (matterId == Guid.Empty && documentId == Guid.Empty)
                {
                    return [];
                }

                IQueryable<MatterDocumentActivityUserTo> query = _context.MatterDocumentActivityUsersTo
                    .Include(md => md.Matter)
                    .Include(md => md.MatterDocumentActivity)
                    .Include(md => md.Document)
                    .Include(md => md.User)
                    .AsSplitQuery()
                    .AsNoTracking();

                if (matterId != Guid.Empty)
                {
                    query = query.Where(mdau => mdau.MatterId == matterId);
                }

                if (documentId != Guid.Empty)
                {
                    query = query.Where(mdau => mdau.DocumentId == documentId);
                }

                return await query.OrderBy(mdau => mdau.CreatedAt).ToListAsync();
            }
            catch (Exception exception)
            {
                string errorMessage = matterId != Guid.Empty && documentId != Guid.Empty
                    ? $"Error getting matter and document history with id's: {matterId}, {documentId}"
                    : matterId != Guid.Empty
                        ? $"Error getting matter history with id: {matterId}"
                        : documentId != Guid.Empty
                            ? $"Error getting document history with id: {documentId}"
                            : "An error occurred while retrieving data.";

                _logger.LogCritical(exception, errorMessage);
                throw;
            }
        }
        */

        /// <summary>
        /// Identifies if matter history exists
        /// </summary>
        /// <param name="matterId">Matter to check</param>
        /// <returns>true if Matter History exists, false otherwise</returns>
        public async Task<bool> DoesMatterHistoryExistAsync(
            Guid matterId)
        {
            try
            {
                return await _context.MatterActivityUsers
                    .AnyAsync(mau => mau.MatterId == matterId);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting matter historical data with id: {matterId}", matterId);
                throw;
            }
        }

        #endregion Matters

        #region MatterActivity

        /// <summary>
        /// Gets MatterActivity by activity
        /// </summary>
        /// <param name="activityName">activity to retrieve</param>
        /// <returns>MatterActivity</returns>
        public async Task<MatterActivity?> GetMatterActivityByActivityNameAsync(
                    string activityName)
        {
            if (string.IsNullOrWhiteSpace(activityName))
            {
                throw new ArgumentException("Activity name cannot be null or empty", nameof(activityName));
            }

            try
            {
                return await _context
                    .MatterActivities
                    .AsNoTracking()
                    .SingleOrDefaultAsync(m => m.Activity == activityName);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting matter activity with activity: {ActivityName}", activityName);
                throw;
            }
        }

        #endregion MatterActivity

        #region RevisionActivities

        /// <summary>
        /// checks if a revision activity already exists
        /// </summary>
        /// <param name="activityName"></param>
        /// <returns></returns>
        public async Task<bool> RevisionActivityExistsAsync(
            string activityName)
        {
            if (string.IsNullOrWhiteSpace(activityName))
            {
                throw new ArgumentException("Activity name cannot be null or empty", nameof(activityName));
            }

            try
            {
                return await _context
                    .RevisionActivities
                    .AsNoTracking()
                    .AnyAsync(ra => ra.Activity == activityName);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error checking revision activity with activity: {ActivityName}", activityName);
                throw;
            }
        }

        /// <summary>
        /// Gets revision activity for given activity
        /// </summary>
        /// <param name="activityName">activity to retrieve</param>
        /// <returns></returns>
        public async Task<RevisionActivity?> GetRevisionActivityByActivityNameAsync(
            string activityName)
        {
            if (string.IsNullOrWhiteSpace(activityName))
            {
                throw new ArgumentException("Activity name cannot be null or empty", nameof(activityName));
            }

            try
            {
                return await _context
                    .RevisionActivities
                    .AsNoTracking()
                    .SingleOrDefaultAsync(ra => ra.Activity == activityName);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting revision activity with activity: {activityName}", activityName);
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
        public async Task<Revision?> UpdateRevisionAsync(
            Guid matterId, 
            Guid documentId, 
            Guid revisionId, 
            RevisionDto revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision), "Revision cannot be null");
            }

            try
            {
                var newRevision = _context.Revisions.Update(_mapper.Map<Revision>(revision));

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == "rbrown");
                var revisionActivity = await _context.RevisionActivities.FirstOrDefaultAsync(ra => ra.Activity == "SAVED");

                if (user == null || revisionActivity == null)
                {
                    return null;
                }

                var rau = new RevisionActivityUser
                {
                    CreatedAt = DateTime.UtcNow,
                    Revision = newRevision.Entity,
                    RevisionId = newRevision.Entity.Id,
                    UserId = user.Id,
                    User = user,
                    RevisionActivityId = revisionActivity.Id,
                    RevisionActivity = revisionActivity
                };

                _context.Entry(rau.RevisionActivity).State = EntityState.Unchanged;
                _context.Entry(rau.User).State = EntityState.Unchanged;

                await _context.RevisionActivityUsers.AddAsync(rau);

                return await SaveChangesAsync() ? rau.Revision : null;
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error updating revision with ID: {RevisionId}", revisionId);
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
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision), "Revision cannot be null");
            }

            try
            {
                // TODO: Add appropriate username code after authentication
                var document = await _context.Documents.SingleOrDefaultAsync(d => d.Id == documentId);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == "rbrown");
                var revisionActivity = await _context.RevisionActivities.FirstOrDefaultAsync(ra => ra.Activity == "CREATED");

                if (document == null || user == null || revisionActivity == null)
                {
                    return null;
                }

                var dbRevision = _mapper.Map<Revision>(revision);
                dbRevision.DocumentId = documentId;
                dbRevision.Document = document;

                var updatedRevision = await _context.Revisions.AddAsync(dbRevision);

                var rau = new RevisionActivityUser
                {
                    Revision = updatedRevision.Entity,
                    RevisionId = updatedRevision.Entity.Id,
                    RevisionActivity = revisionActivity,
                    RevisionActivityId = revisionActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Entry(rau.RevisionActivity).State = EntityState.Unchanged;
                _context.Entry(rau.User).State = EntityState.Unchanged;

                await _context.RevisionActivityUsers.AddAsync(rau);

                return await SaveChangesAsync() ? rau.Revision : null;
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error adding revision to document with ID: {DocumentId}", documentId);
                throw;
            }
        }

        /// <summary>
        /// Checks if revision Id exists
        /// </summary>
        /// <param name="revisionId">revision to check</param>
        /// <returns>true if exists, false otherwise</returns>
        public async Task<bool> RevisionExistsAsync(
            Guid revisionId)
        {
            try
            {
                return await _context
                    .Revisions
                    .AsNoTracking()
                    .AnyAsync(r => r.Id == revisionId);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error finding revision with ID: {RevisionId}", revisionId);
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
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision), "Revision cannot be null");
            }

            try
            {
                var dbRevision = await _context
                    .Revisions
                    .SingleOrDefaultAsync(r => r.Id == revision.Id);

                if (dbRevision == null)
                {
                    return false;
                }

                dbRevision.IsDeleted = true;
                _context.Revisions.Update(dbRevision);

                // TODO: Add appropriate username code after authentication
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == "rbrown");
                var revisionActivity = await _context.RevisionActivities.FirstOrDefaultAsync(ma => ma.Activity == "DELETED");

                if (user == null || revisionActivity == null)
                {
                    return false;
                }

                var rau = new RevisionActivityUser
                {
                    Revision = dbRevision,
                    RevisionId = dbRevision.Id,
                    RevisionActivity = revisionActivity,
                    RevisionActivityId = revisionActivity.Id,
                    User = user,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Entry(rau.RevisionActivity).State = EntityState.Unchanged;
                _context.Entry(rau.User).State = EntityState.Unchanged;

                await _context.RevisionActivityUsers.AddAsync(rau);

                return await SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error deleting revision with ID: {RevisionId}", revision.Id);
                throw;
            }
        }

        /// <summary>
        /// Retrieves revision by specified Id
        /// </summary>
        /// <param name="revisionId">revision to retrieve</param>
        public async Task<Revision?> GetRevisionByIdAsync(
            Guid revisionId)
        {
            try
            {
                return await _context
                    .Revisions
                    .AsNoTracking()
                    .SingleOrDefaultAsync(r => r.Id == revisionId);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting revision with ID: {RevisionId}", revisionId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves revision history by specified Id
        /// </summary>
        /// <param name="revisionId">revision to retrieve</param>
        public async Task<IEnumerable<RevisionActivityUser>> GetRevisionWithHistoryByIdAsync(
            Guid revisionId)
        {
            try
            {
                var revisionWithActivityAndHistory = _context.RevisionActivityUsers
                    .Where(rau => rau.RevisionId == revisionId)
                    .Include(rau => rau.User)
                    .Include(rau => rau.Revision)
                    .Include(rau => rau.RevisionActivity)
                    .AsNoTracking()
                    .OrderBy(rau => rau.CreatedAt)
                    .AsSplitQuery();

                return await revisionWithActivityAndHistory.ToListAsync();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting revision history with ID: {RevisionId}", revisionId);
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
                var collection = _context.Revisions
                    .Where(r => r.DocumentId == documentId);

                if (!includeDeleted)
                {
                    collection = collection.Where(r => !r.IsDeleted);
                }

                return await collection
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting revisions with document ID: {DocumentId}", documentId);
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
        public async Task<User?> GetUserByUsernameAsync(
            string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            try
            {
                // TODO: Add appropriate username code after authentication
                return await _context
                    .Users
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.Name == username);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error getting user with username: {Username}", username);
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
                _logger.LogCritical(exception, "Error saving changes to the database");
                throw;
            }
        }

        /// <summary>
        /// retrieves an extended audit history
        /// </summary>
        /// <param name="matterId">matter to retrieve data for</param>
        /// <param name="documentId">document to retrieve data for</param>
        /// <param name="direction">operation:  From / To</param>
        /// <returns></returns>
        public async Task<IEnumerable<MatterDocumentActivityUserMinimalDto>> GetExtendedAuditsAsync(
            Guid matterId, 
            Guid documentId, 
            string direction)
        {
            try
            {
                if (matterId == Guid.Empty && documentId == Guid.Empty)
                {
                    return [];
                }

                if (direction.Equals("from", StringComparison.OrdinalIgnoreCase))
                {
                    IQueryable<MatterDocumentActivityUserFrom> query = _context.MatterDocumentActivityUsersFrom
                        .Include(md => md.Matter)
                        .Include(md => md.MatterDocumentActivity)
                        .Include(md => md.Document)
                        .Include(md => md.User)
                        .AsSplitQuery()
                        .AsNoTracking();

                    if (matterId != Guid.Empty)
                    {
                        query = query.Where(mdau => mdau.MatterId == matterId);
                    }

                    if (documentId != Guid.Empty)
                    {
                        query = query.Where(mdau => mdau.DocumentId == documentId);
                    }

                    return _mapper.Map<IEnumerable<MatterDocumentActivityUserMinimalDto>>(await query.OrderBy(mdau => mdau.CreatedAt).ToListAsync());
                }
                else if (direction.Equals("to", StringComparison.OrdinalIgnoreCase))
                {
                    IQueryable<MatterDocumentActivityUserTo> query = _context.MatterDocumentActivityUsersTo
                        .Include(md => md.Matter)
                        .Include(md => md.MatterDocumentActivity)
                        .Include(md => md.Document)
                        .Include(md => md.User)
                        .AsSplitQuery()
                        .AsNoTracking();

                    if (matterId != Guid.Empty)
                    {
                        query = query.Where(mdau => mdau.MatterId == matterId);
                    }

                    if (documentId != Guid.Empty)
                    {
                        query = query.Where(mdau => mdau.DocumentId == documentId);
                    }

                    return _mapper.Map<IEnumerable<MatterDocumentActivityUserMinimalDto>>(await query.OrderBy(mdau => mdau.CreatedAt).ToListAsync());
                }
                else
                {
                    return [];
                }
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Error saving changes to the database");
                throw;
            }
        }
    }
}