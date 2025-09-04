namespace ADMS.API.Common.Interfaces;

/// <summary>
/// Marker interface for entities that maintain comprehensive audit trails for legal compliance.
/// </summary>
/// <remarks>
/// Audit trail functionality is essential for legal document management systems,
/// providing accountability, compliance tracking, and professional responsibility support.
/// </remarks>
public interface IAuditable
{
    /// <summary>
    /// Gets a value indicating whether the entity has recorded activities.
    /// </summary>
    bool HasActivities { get; }

    /// <summary>
    /// Gets the total count of all activities associated with the entity.
    /// </summary>
    int TotalActivityCount { get; }

    /// <summary>
    /// Gets a value indicating whether the entity has a comprehensive audit trail.
    /// </summary>
    bool HasActivityHistory { get; }
}