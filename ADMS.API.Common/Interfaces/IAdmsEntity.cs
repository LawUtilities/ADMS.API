namespace ADMS.API.Common.Interfaces;

/// <summary>
/// Marker interface for core ADMS entity DTOs providing unified entity identification and behavior.
/// </summary>
/// <remarks>
/// This interface serves as the base marker for all primary entity DTOs in the ADMS system,
/// providing consistent identification patterns and enabling polymorphic operations across
/// different entity types.
/// </remarks>
public interface IAdmsEntity
{
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the display text suitable for UI presentation.
    /// </summary>
    string DisplayText { get; }

    /// <summary>
    /// Gets a value indicating whether this entity is in a valid state for operations.
    /// </summary>
    bool IsValid { get; }
}