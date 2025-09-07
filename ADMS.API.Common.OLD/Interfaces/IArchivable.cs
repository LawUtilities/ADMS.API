namespace ADMS.API.Common.Interfaces;

/// <summary>
/// Marker interface for entities that support archival operations for inactive state management.
/// </summary>
/// <remarks>
/// Archival functionality supports matter lifecycle management by moving completed
/// or inactive entities to an archived state while preserving accessibility.
/// </remarks>
public interface IArchivable
{
    /// <summary>
    /// Gets a value indicating whether the entity is archived.
    /// </summary>
    bool IsArchived { get; }

    /// <summary>
    /// Gets a value indicating whether the entity is currently active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Determines whether the entity can be archived.
    /// </summary>
    bool CanBeArchived();
}