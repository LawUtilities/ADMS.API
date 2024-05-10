using ADMS.API.Entities;
using ADMS.API.Models;

namespace ADMS.API.Services
{
    /// <summary>
    /// ADMS Repository identifying actions used.
    /// </summary>
    public interface IAdmsRepository
    {

        #region Documents

        /// <summary>
        /// Adds document to selected matter
        /// </summary>
        /// <param name="matterId">matter Id to add to</param>
        /// <param name="document">document to be added</param>
        /// <returns></returns>
        Task<Document?> AddDocumentAsync(
            Guid matterId, 
            DocumentForCreationDto document);

        /// <summary>
        /// Checks to see if a specified document exists
        /// </summary>
        /// <param name="documentId">Document ID to be checked</param>
        /// <returns>True if the document exists, false otherwise</returns>
        Task<bool> CheckDocumentExistsAsync(
            Guid documentId);

        /// <summary>
        /// Checkin document
        /// </summary>
        /// <param name="documentId">document to be checked out</param>
        /// <returns>true if checked in, false otherwise</returns>
        Task<bool> CheckinDocumentAsync(
            Guid documentId);

        /// <summary>
        /// Checkout document for editing
        /// </summary>
        /// <param name="documentId">document to be checked out</param>
        /// <returns>true if checked out, false otherwise</returns>
        Task<bool> CheckoutDocumentAsync(
            Guid documentId);

        /// <summary>
        /// Deletes a document
        /// </summary>
        /// <param name="document">document to be deleted</param>
        Task<bool> DeleteDocumentAsync(
            DocumentDto document);

        /// <summary>
        /// Gets a list of Documents.
        /// </summary>
        /// <param name="matterId">matter containing document(s)</param>
        /// <param name="includeDeleted">include deleted documents</param>
        /// <returns>A list of documents</returns>
        Task<IEnumerable<Document?>> GetDocumentsAsync(
            Guid matterId, 
            bool includeDeleted = false);

        /// <summary>
        /// Gets a filtered set of Documents
        /// </summary>
        /// <param name="matterId">matter containing document(s)</param>
        /// <param name="fileName">Filename to search for</param>
        /// <param name="searchQuery">Additional search terms to find</param>
        /// <param name="includeDeleted">include deleted documents</param>
        /// <param name="pageNumber">Page Number being sought.</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <returns>Document and pagination information.</returns>
        Task<(IEnumerable<Document>, PaginationMetadata)> GetDocumentsAsync(
            Guid matterId, 
            string? fileName, 
            string? searchQuery, 
            bool includeDeleted, 
            int pageNumber, 
            int pageSize);

        /// <summary>
        /// Gets a single document details
        /// </summary>
        /// <param name="documentId">Document Id</param>
        /// <param name="includeRevisions">Should the document return with revision information</param>
        /// <returns>Document and associated revisions</returns>
        Task<Document?> GetDocumentAsync(
            Guid documentId, 
            bool includeRevisions);

        /// <summary>
        /// Gets a document by entered filename
        /// </summary>
        /// <param name="matterId">matter containing document(s)</param>
        /// <param name="fileName">File name of the document being searched.</param>
        /// <returns>Document</returns>
        Task<Document?> GetDocumentByFileNameAsync(
            Guid matterId, 
            string fileName);

        /// <summary>
        /// retrieve document history by id
        /// </summary>
        /// <param name="id">document to retrieve history from</param>
        /// <returns>Document history to be retrieved</returns>
        Task<IEnumerable<DocumentActivityUser>> GetDocumentWithHistoryByIdAsync(
            Guid id);

        /// <summary>
        /// Copy document from one matter to another
        /// </summary>
        /// <param name="document">document to copy</param>
        /// <param name="matterId">matter to copy document to</param>
        /// <returns>true if copied, false otherwise</returns>
        Task<bool> CopyDocumentAsync(
            Guid matterId,
            DocumentWithoutRevisionsDto document);

        /// <summary>
        /// Move document from one matter to another
        /// </summary>
        /// <param name="document">document to be moved</param>
        /// <param name="matterId">matter to be moved to</param>
        /// <returns>true if moved, false if not</returns>
        Task<bool> MoveDocumentAsync(
            Guid matterId,
            DocumentWithoutRevisionsDto document);

        #endregion Documents

        #region DocumentActivity

        /// <summary>
        /// Gets a DocumentActivity by activity
        /// </summary>
        /// <param name="activity">activity to be retrieved</param>
        /// <returns>DocumentActivity</returns>
        Task<DocumentActivity?> GetDcumentActivityByActivity(
            string activity);

        #endregion DocumentActivity

        #region Matters

        /// <summary>
        /// Adds a matter to the repository
        /// </summary>
        /// <param name="matter">matter to be added.</param>
        /// <returns>matter that has been created</returns>
        Task<Matter?> AddMatterAsync(
            MatterDto matter);

        /// <summary>
        /// Checks if matter exists
        /// </summary>
        /// <param name="matterId">Matter to check</param>
        /// <returns>true if exists, false otherwise</returns>
        Task<bool> CheckMatterExists(
            Guid matterId);

        /// <summary>
        /// Checks if matter exists
        /// </summary>
        /// <param name="description">description of matter to identify</param>
        /// <returns>true if exists, false otherwise</returns>
        Task<bool> CheckMatterExists(
            string description);

        /// <summary>
        /// Delete matter
        /// </summary>
        /// <param name="matter">matter to be deleted</param>
        Task<bool> DeleteMatterAsync(
            MatterDto matter);

        /// <summary>
        /// Gets list of matters not including documents
        /// </summary>
        /// <param name="description">matter description</param>
        /// <param name="includeArchived">include archived matters</param>
        /// <param name="includeDeleted">include deleted matters</param>
        /// <returns>list of matters</returns>
        Task<IEnumerable<Matter>> GetMattersAsync(
            string description, 
            bool includeArchived = false, 
            bool includeDeleted = false);

        /// <summary>
        /// Gets a matter by Id
        /// </summary>
        /// <param name="matterId">matter to return</param>
        /// <param name="includeDocuments">include documents with returned matter</param>
        /// <returns>Matter</returns>
        Task<Matter?> GetMatterAsync(
            Guid matterId, 
            bool includeDocuments);

        /// <summary>
        /// Restore Matter by id
        /// </summary>
        /// <param name="matterId">matter id to restore</param>
        /// <returns>True if matter restored, false otherwise</returns>
        Task<bool> RestoreMatterAsync(
            Guid matterId);

        /// <summary>
        /// retrieve matter history by id
        /// </summary>
        /// <param name="id">matter to retrieve history from</param>
        /// <returns>Matter with history to be retrieved</returns>
        Task<Matter> GetMatterWithHistoryByIdAsync(
            Guid id);

        /// <summary>
        /// Get matter document activity user from list
        /// </summary>
        /// <param name="matterId">matter to retrieve data for</param>
        /// <param name="documentId">document to retrieve data for</param>
        /// <returns>collection of matter document activity user histories</returns>
        Task<ICollection<MatterDocumentActivityUserFromDto>> GetMDAUFromHistoryAsync(
            Guid matterId,
            Guid documentId);

        /// <summary>
        /// Get matter document activity user to list
        /// </summary>
        /// <param name="matterId">matter to retrieve data for</param>
        /// <param name="documentId">document to retrieve data for</param>
        /// <returns>collection of matter document activity user histories</returns>
        Task<ICollection<MatterDocumentActivityUserToDto>> GetMDAUToHistoryAsync(
            Guid matterId,
            Guid documentId);

        /// <summary>
        /// Identifies if matter history exists
        /// </summary>
        /// <param name="id">Matter to check</param>
        /// <returns>true if Matter History exists, false otherwise</returns>
        Task<bool> DoesMatterHistoryExists(
            Guid id);

        #endregion Matters

        #region MatterActivity

        /// <summary>
        /// Gets a MatterActivity by activity
        /// </summary>
        /// <param name="activity">activity to be retrieved</param>
        /// <returns>MatterActivity</returns>
        Task<MatterActivity?> GetMatterActivityByActivity(
            string activity);

        #endregion MatterActivity

        #region Revisions

        /// <summary>
        /// Ad revision to selected document
        /// </summary>
        /// <param name="documentId">Document to add the revision to</param>
        /// <param name="revision">revision to add</param>
        /// <returns>IActionResult</returns>
        Task<Revision?> AddRevisionAsync(
            Guid documentId, 
            RevisionDto revision);

        /// <summary>
        /// Checks to see if a specified revision exists
        /// </summary>
        /// <param name="id">Revision ID to be checked</param>
        /// <returns>True if the document exists, false otherwise</returns>
        Task<bool> CheckRevisionExistsAsync(
            Guid id);

        /// <summary>
        /// Deletes a revision from a document
        /// </summary>
        /// <param name="revision">The revision to be deleted</param>
        Task<bool> DeleteRevisionAsync(
            RevisionDto revision);

        /// <summary>
        /// retrieve revision by id
        /// </summary>
        /// <param name="id">revision to be deleted</param>
        /// <returns>Revision to be retrieved</returns>
        Task<Revision?> GetRevisionByIdAsync(
            Guid id);

        /// <summary>
        /// retrieve revision history by revision id
        /// </summary>
        /// <param name="id">revision to be deleted</param>
        /// <returns>Revision to be retrieved</returns>
        Task<IEnumerable<RevisionActivityUser>> GetRevisionWithHistoryByIdAsync(
            Guid id);

        /// <summary>
        /// Get list of revisions
        /// </summary>
        /// <param name="documentId">Document Id to retrieve revisions for</param>
        /// <param name="includeDeleted">include deleted revisions</param>
        /// <returns>list of revisions</returns>
        Task<IEnumerable<Revision>> GetRevisionsAsync(
            Guid documentId, 
            bool includeDeleted = false);

        /// <summary>
        /// update revision
        /// </summary>
        /// <param name="matterId">Matter containing revision</param>
        /// <param name="documentId">Document being updated</param>
        /// <param name="revisionId">Revision to be updated</param>
        /// <param name="revision">Revision details to update</param>
        /// <returns>list of revisions</returns>
        Task<Revision?> UpdateRevisionsAsync(
            Guid matterId, 
            Guid documentId, 
            Guid revisionId, 
            RevisionDto revision);

        #endregion Revisions

        #region Revision Activities

        /// <summary>
        /// Gets a revision activity by activity name
        /// </summary>
        /// <param name="activity">activity name being sourced</param>
        /// <returns>RevisionActivity</returns>
        Task<RevisionActivity?> GetRevisionActivityByActivityAsync(
            string activity);

        /// <summary>
        /// Identifies if a revision activity exists by activity description
        /// </summary>
        /// <param name="activity">activity being checked</param>
        /// <returns>true if activity exists, false otherwise</returns>
        Task<bool> CheckRevisionActivityExists(
            string activity);

        #endregion Revision Activities

        #region User Actions

        /// <summary>
        /// Get a user by enterted username
        /// </summary>
        /// <param name="username">username being requested</param>
        /// <returns>User</returns>
        Task<User?> GetUserByUsername(
            string username);

        #endregion User Actions

        #region General Actions

        /// <summary>
        /// persists data to database
        /// </summary>
        /// <returns>true if successfull, false otherwise</returns>
        Task<bool> SaveChangesAsync();

        #endregion General Actions
    }
}
