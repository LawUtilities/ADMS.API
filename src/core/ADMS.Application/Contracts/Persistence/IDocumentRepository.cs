using ADMS.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMS.Application.Contracts.Persistence
{
    internal interface IDocumentRepository
    {
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
    }
}
