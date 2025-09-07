using System;

namespace ADMS.API.Common.Interfaces;

/// <summary>
/// Interface for documents that include creation date information.
/// </summary>
/// <remarks>
/// This interface provides a contract for document types that include creation date tracking,
/// enabling consistent temporal operations across different document representations in the
/// ADMS legal document management system.
/// 
/// <para><strong>Implementation Guidelines:</strong></para>
/// <list type="bullet">
/// <item><strong>UTC Storage:</strong> All creation dates should be stored in UTC format</item>
/// <item><strong>Immutability:</strong> Creation dates should be set once and not modified</item>
/// <item><strong>Validation:</strong> Implementations should validate creation dates using common validation helpers</item>
/// <item><strong>Audit Trail:</strong> Creation dates form the foundation of audit trail temporal tracking</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Temporal Queries:</strong> Finding recent documents within specific time ranges</item>
/// <item><strong>Audit Operations:</strong> Chronological analysis of document creation patterns</item>
/// <item><strong>Reporting:</strong> Document creation statistics and timeline analysis</item>
/// <item><strong>Filtering:</strong> Date-based document filtering in collections and queries</item>
/// </list>
/// </remarks>
public interface IDocumentWithCreationDate
{
    /// <summary>
    /// Gets the creation date of the document (in UTC).
    /// </summary>
    /// <remarks>
    /// The creation date represents when the document was initially created in the system,
    /// stored in UTC format for consistency across different time zones and supporting
    /// accurate temporal tracking for legal compliance and audit requirements.
    /// </remarks>
    DateTime CreationDate { get; }
}