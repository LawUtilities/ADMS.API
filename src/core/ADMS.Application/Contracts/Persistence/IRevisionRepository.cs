using ADMS.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMS.Application.Contracts.Persistence
{
    internal interface IRevisionRepository
    {

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
    }
}
