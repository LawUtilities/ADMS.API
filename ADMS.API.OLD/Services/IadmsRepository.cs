using ADMS.API.Entities;
using ADMS.API.Helpers;
using ADMS.API.Models;
using ADMS.API.ResourceParameters;
using Microsoft.AspNetCore.Mvc;

namespace ADMS.API.Services
{
    /// <summary>
    ///     ADMS Repository identifying actions used.
    /// </summary>
    public interface IAdmsRepository
    {
        #region HELPERS

        /// <summary>
        ///   Retrieves a list of extended audits for a specified matter and document.
        /// </summary>
        /// <param name="matterId">Matter to be retrieved</param>
        /// <param name="documentId">Document to be retrieved</param>
        /// <param name="direction">FROM or TO</param>
        /// <returns>A queryable collection of audit records.</returns>
        Task<IQueryable<MatterDocumentActivityUserMinimalDto>> GetExtendedAuditsAsync(Guid matterId, Guid documentId,
            AuditEnums.AuditDirection direction);

        #endregion HELPERS

        #region MatterActivity

        /// <summary>
        ///     Retrieves a MatterActivity by its name.
        /// </summary>
        /// <param name="activityName">The name of the activity to retrieve.</param>
        /// <returns>The requested MatterActivity, or null if not found.</returns>
        Task<MatterActivity?> GetMatterActivityByActivityNameAsync(string activityName);

        #endregion MatterActivity

        #region User Actions

        /// <summary>
        ///     Retrieves a user by their username.
        /// </summary>
        /// <param name="username">The username of the user to retrieve.</param>
        /// <returns>The requested user, or null if not found.</returns>
        Task<User?> GetUserByUsernameAsync(string username);

        #endregion User Actions

        #region General Actions

        /// <summary>
        ///     Persists changes to the database.
        /// </summary>
        /// <returns>True if the changes were successfully saved, false otherwise.</returns>
        Task<bool> SaveChangesAsync();

        #endregion General Actions

        #region Documents

        /// <summary>
        ///     Adds a document to the specified matter.
        /// </summary>
        /// <param name="matterId">The ID of the matter to add the document to.</param>
        /// <param name="document">The document to add.</param>
        /// <returns>The created document, or null if the operation fails.</returns>
        Task<Document?> AddDocumentAsync(Guid matterId, DocumentForCreationDto? document);

        /// <summary>
        ///     Checks if a document exists.
        /// </summary>
        /// <param name="documentId">The ID of the document to check.</param>
        /// <returns>True if the document exists, false otherwise.</returns>
        Task<bool> DocumentExistsAsync(Guid documentId);

        /// <summary>
        /// Checks if a document with the specified file name already exists.
        /// </summary>
        /// <param name="matterId">Matter ID containing the document.</param>
        /// <param name="fileName">The file name to check.</param>
        /// <returns>True if the file name exists, false otherwise.</returns>
        Task<bool> FileNameExists(Guid matterId, string fileName);

        /// <summary>
        ///     Retrieves a document by its ID, optionally including revisions and history.
        /// </summary>
        /// <param name="documentId">The ID of the document to retrieve.</param>
        /// <param name="includeRevisions">Whether to include revisions in the result.</param>
        /// <param name="includeHistory">Whether to include history in the result.</param>
        /// <returns>The requested document, or null if not found.</returns>
        Task<Document?> GetDocumentAsync(Guid documentId, bool includeRevisions, bool includeHistory);

        /// <summary>
        ///     Retrieves a paginated list of documents for a specified matter.
        /// </summary>
        /// <param name="matterId">The ID of the matter containing the documents.</param>
        /// <param name="parameters">The parameters for pagination and filtering.</param>
        /// <returns>A paginated list of documents.</returns>
        Task<Helpers.PagedList<Document>> GetPaginatedDocumentsAsync(Guid matterId, DocumentsResourceParameters parameters);

        /// <summary>
        ///     Deletes a document.
        /// </summary>
        /// <param name="document">The document to delete.</param>
        /// <returns>True if the document was successfully deleted, false otherwise.</returns>
        Task<bool> DeleteDocumentAsync(DocumentDto document);

        /// <summary>
        /// Sets the checked-out state of a document.
        /// </summary>
        /// <param name="documentId">Document Id to be marked as checked out or checked in.</param>
        /// <param name="isCheckedOut">True to check out, false to check in.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> SetDocumentCheckStateAsync(Guid documentId, bool isCheckedOut);

        /// <summary>
        ///     Updates a document with new details using a DTO.
        /// </summary>
        /// <param name="documentId">The ID of the document to update.</param>
        /// <param name="documentForUpdate">The DTO containing updated document data.</param>
        /// <returns>The updated document, or null if the operation fails.</returns>
        Task<Document?> UpdateDocumentAsync(Guid documentId, DocumentForUpdateDto documentForUpdate);

        /// <summary>
        ///     Performs an operation (Move or Copy) on a document.
        /// </summary>
        /// <param name="sourceMatterId">The ID of the source matter containing the document.</param>
        /// <param name="targetMatterId">The ID of the target matter to move or copy the document to.</param>
        /// <param name="document">The document to move or copy.</param>
        /// <param name="operationType">The type of operation to perform ("Move" or "Copy").</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> PerformDocumentOperationAsync(
            Guid sourceMatterId,
            Guid targetMatterId,
            DocumentWithoutRevisionsDto? document,
            string operationType);

        #endregion Documents

        #region DocumentActivity

        /// <summary>
        ///     Retrieves a DocumentActivity by its name.
        /// </summary>
        /// <param name="activityName">The name of the activity to retrieve.</param>
        /// <returns>The requested DocumentActivity, or null if not found.</returns>
        Task<DocumentActivity?> GetDocumentActivityByActivityNameAsync(string activityName);

        /// <summary>
        /// Retrieves document audit records for a specified document, returning appropriate error responses.
        /// </summary>
        /// <param name="documentId">The ID of the document to retrieve audits for.</param>
        /// <returns>
        /// <para><see cref="OkObjectResult"/> with a list of document audit records if successful.</para>
        /// <para><see cref="BadRequestObjectResult"/> if the documentId is invalid.</para>
        /// <para><see cref="NotFoundObjectResult"/> if the document does not exist.</para>
        /// <para><see cref="ObjectResult"/> with status code 500 if an unexpected error occurs.</para>
        /// </returns>
        Task<ActionResult<IEnumerable<DocumentActivityUserMinimalDto>>> GetDocumentAuditsAsync(Guid documentId);

        /// <summary>
        /// Retrieves a paginated list of document activity audit records (create, save, delete, restore, etc.) for a specified document.
        /// </summary>
        /// <param name="documentId">The ID of the document to retrieve audits for.</param>
        /// <param name="resourceParameters">Pagination and sorting information.</param>
        /// <returns>Paged list of document activity audit records.</returns>
        Task<ActionResult<Helpers.PagedList<DocumentActivityUserMinimalDto>>> GetDocumentActivityAuditsAsync(
        Guid documentId, DocumentAuditsResourceParameters resourceParameters);

        /// <summary>
        /// Retrieves a paginated list of "move/copy FROM" audit records for a specified document.
        /// </summary>
        /// <param name="documentId">The ID of the document to retrieve move/copy FROM audits for.</param>
        /// <param name="resourceParameters">Pagination and sorting information.</param>
        /// <returns>Paged list of move/copy FROM audit records.</returns>
        Task<Helpers.PagedList<MatterDocumentActivityUserMinimalDto>> GetPaginatedDocumentMoveFromAuditsAsync(
        Guid documentId, DocumentAuditsResourceParameters resourceParameters);

        /// <summary>
        /// Retrieves a paginated list of "move/copy TO" audit records for a specified document.
        /// </summary>
        /// <param name="documentId">The ID of the document to retrieve move/copy TO audits for.</param>
        /// <param name="resourceParameters">Pagination and sorting information.</param>
        /// <returns>Paged list of move/copy TO audit records.</returns>
        Task<Helpers.PagedList<MatterDocumentActivityUserMinimalDto>> GetPaginatedDocumentMoveToAuditsAsync(
        Guid documentId, DocumentAuditsResourceParameters resourceParameters);

        #endregion DocumentActivity

        #region Matters

        /// <summary>
        ///     Adds a matter to the repository.
        /// </summary>
        /// <param name="matter">The matter to add.</param>
        /// <returns>The created matter, or null if the operation fails.</returns>
        Task<ActionResult<Matter>> AddMatterAsync(MatterForCreationDto matter);

        /// <summary>
        ///     Checks if a matter exists.
        /// </summary>
        /// <param name="matterId">The ID of the matter to check.</param>
        /// <returns>True if the matter exists, false otherwise.</returns>
        Task<ActionResult<bool>> MatterExistsAsync(Guid matterId);

        /// <summary>
        ///    Checks if a matter name already exists.
        /// </summary>
        /// <param name="matterName">Matter name to check.</param>
        /// <returns>True if name exists, false otherwise</returns>
        Task<ActionResult<bool>> MatterNameExistsAsync(string matterName);

        /// <summary>
        ///    Deletes the specified matter.
        /// </summary>
        /// <param name="matterToDelete">Matter to be deleted</param>
        /// <returns>true if deleted, false otherwise</returns>
        Task<bool> DeleteMatterAsync(MatterDto matterToDelete);

        /// <summary>
        ///     Retrieves a paginated list of matters based on the specified resource parameters.
        /// </summary>
        /// <param name="resourceParameters">The parameters for pagination, filtering, and sorting.</param>
        /// <returns>A paginated list of matters.</returns>
        Task<Helpers.PagedList<Matter>> GetPaginatedMattersAsync(MattersResourceParameters? resourceParameters);

        /// <summary>
        ///     Retrieves a matter by its ID, optionally including documents and history.
        /// </summary>
        /// <param name="matterId">The ID of the matter to retrieve.</param>
        /// <param name="includeDocuments">Whether to include documents in the result.</param>
        /// <param name="includeHistory">Whether to include history in the result.</param>
        /// <returns>The requested matter, or null if not found.</returns>
        Task<Matter?> GetMatterAsync(Guid matterId, bool includeDocuments, bool includeHistory = false);

        /// <summary>
        ///     Restores a deleted matter.
        /// </summary>
        /// <param name="matterId">The ID of the matter to restore.</param>
        /// <returns>True if the matter was successfully restored, false otherwise.</returns>
        Task<bool> RestoreMatterAsync(Guid matterId);

        /// <summary>
        /// Updates a specified matter with new data and logs the update as an audit activity.
        /// </summary>
        /// <param name="matterId">The ID of the matter to update.</param>
        /// <param name="matterToUpdate">The updated matter data.</param>
        /// <returns>The updated matter, or null if the operation fails.</returns>
        Task<Matter?> UpdateMatterAsync(Guid matterId, MatterForUpdateDto? matterToUpdate);

        #endregion Matters

        #region Revisions

        /// <summary>
        ///     Adds a revision to the specified document.
        /// </summary>
        /// <param name="documentId">The ID of the document to add the revision to.</param>
        /// <param name="revision">The revision to add.</param>
        /// <returns>The created revision, or null if the operation fails.</returns>
        Task<Revision?> AddRevisionAsync(Guid documentId, RevisionDto revision);

        /// <summary>
        ///     Checks if a revision exists.
        /// </summary>
        /// <param name="revisionId">The ID of the revision to check.</param>
        /// <returns>True if the revision exists, false otherwise.</returns>
        Task<bool> RevisionExistsAsync(Guid revisionId);

        /// <summary>
        ///     Deletes a revision from a document.
        /// </summary>
        /// <param name="revision">The revision to be deleted.</param>
        /// <returns>True if the revision was successfully deleted, false otherwise.</returns>
        Task<bool> DeleteRevisionAsync(RevisionDto revision);

        /// <summary>
        ///     Retrieves a revision by its ID.
        /// </summary>
        /// <param name="revisionId">The ID of the revision to retrieve.</param>
        /// <param name="includeHistory">Include revision history in retrieved data.</param>
        /// <returns>The requested revision, or null if not found.</returns>
        Task<Revision?> GetRevisionByIdAsync(Guid revisionId, bool includeHistory = false);

        /// <summary>
        ///     Retrieves a list of revisions for a specified document.
        /// </summary>
        /// <param name="documentId">The ID of the document to retrieve revisions for.</param>
        /// <param name="includeDeleted">Whether to include deleted revisions.</param>
        /// <param name="orderBy">Optional order by clause.</param>
        /// <returns>A list of revisions.</returns>
        Task<ActionResult<IQueryable<Revision>>> GetRevisionsAsync(
            Guid documentId,
            bool includeDeleted,
            string? orderBy = null);

        /// <summary>
        ///    Retrieves a paginated list of revisions for a specified document.
        /// </summary>
        /// <param name="documentId">Document ID containing the Revisions.</param>
        /// <param name="resourceParameters">Parameters to be sent to perform pagination / sorting, etc.</param>
        /// <returns>Paged list of revisions</returns>
        Task<ActionResult<Helpers.PagedList<Revision>>> GetPaginatedRevisionsAsync(
            Guid documentId,
            RevisionsResourceParameters resourceParameters);

        /// <summary>
        ///     Updates a revision with new details.
        /// </summary>
        /// <param name="matterId">The ID of the matter containing the revision.</param>
        /// <param name="documentId">The ID of the document containing the revision.</param>
        /// <param name="revisionId">The ID of the revision to update.</param>
        /// <param name="revision">The revision details to update.</param>
        /// <returns>The updated revision, or null if the operation fails.</returns>
        Task<Revision?> UpdateRevisionAsync(Guid matterId, Guid documentId, Guid revisionId, Revision revision);

        #endregion Revisions
    }
}