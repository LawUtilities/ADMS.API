namespace ADMS.API.Common.Interfaces;

/// <summary>
/// Marker interface for entities that support soft deletion with audit trail preservation.
/// </summary>
/// <remarks>
/// Soft deletion preserves audit trails and referential integrity while marking entities
/// as deleted for business operations and user interface filtering.
/// </remarks>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets a value indicating whether the entity is soft-deleted.
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// Determines whether the entity can be safely deleted.
    /// </summary>
    bool CanBeDeleted();

    /// <summary>
    /// Determines whether the entity can be restored from deleted state.
    /// </summary>
    bool CanBeRestored();
}