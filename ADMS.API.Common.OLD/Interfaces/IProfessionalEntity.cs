namespace ADMS.API.Common.Interfaces;

/// <summary>
/// Marker interface for entities that support professional legal practice standards.
/// </summary>
/// <remarks>
/// Professional entity functionality ensures compliance with legal practice standards,
/// professional responsibility requirements, and business process integration.
/// </remarks>
public interface IProfessionalEntity
{
    /// <summary>
    /// Gets the professional status of the entity.
    /// </summary>
    string Status { get; }

    /// <summary>
    /// Gets professional metrics for reporting and analysis.
    /// </summary>
    object GetComprehensiveStatistics();

    /// <summary>
    /// Gets audit information suitable for professional compliance reporting.
    /// </summary>
    IReadOnlyDictionary<string, object> GetAuditInformation();
}