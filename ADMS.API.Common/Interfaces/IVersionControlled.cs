namespace ADMS.API.Common.Interfaces;

/// <summary>
/// Marker interface for entities that support version control with check-in/check-out operations.
/// </summary>
/// <remarks>
/// Version control functionality supports professional document management practices
/// by providing exclusive editing control and revision tracking capabilities.
/// </remarks>
public interface IVersionControlled
{
    /// <summary>
    /// Gets a value indicating whether the entity is currently checked out for editing.
    /// </summary>
    bool IsCheckedOut { get; }

    /// <summary>
    /// Gets a value indicating whether the entity is available for editing.
    /// </summary>
    bool IsAvailableForEdit { get; }

    /// <summary>
    /// Determines whether the entity can be checked out for editing.
    /// </summary>
    bool CanBeCheckedOut();

    /// <summary>
    /// Determines whether the entity can be checked in from its current state.
    /// </summary>
    bool CanBeCheckedIn();
}