using ADMS.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMS.Application.Contracts.Persistence
{
    internal interface IMatterRepository
    {
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

        #region MatterActivity

        /// <summary>
        ///     Retrieves a MatterActivity by its name.
        /// </summary>
        /// <param name="activityName">The name of the activity to retrieve.</param>
        /// <returns>The requested MatterActivity, or null if not found.</returns>
        Task<MatterActivity?> GetMatterActivityByActivityNameAsync(string activityName);

        #endregion MatterActivity

    }
}
