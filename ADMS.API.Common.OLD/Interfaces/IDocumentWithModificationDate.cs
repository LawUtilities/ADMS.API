namespace ADMS.API.Common.Interfaces;

/// <summary>
/// Interface for documents that include modification date information.
/// </summary>
/// <remarks>
/// This interface provides a contract for document types that include modification date tracking,
/// enabling consistent temporal operations across different document representations in the
/// ADMS legal document management system.
/// 
/// <para><strong>Implementation Guidelines:</strong></para>
/// <list type="bullet">
/// <item><strong>UTC Storage:</strong> All modification dates should be stored in UTC format</item>
/// <item><strong>Update Tracking:</strong> Modification dates should be updated whenever document content changes</item>
/// <item><strong>Validation:</strong> Implementations should validate modification dates using common validation helpers</item>
/// <item><strong>Audit Trail:</strong> Modification dates support audit trail temporal tracking and document lifecycle management</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Change Tracking:</strong> Identifying recently modified documents within specific time ranges</item>
/// <item><strong>Audit Operations:</strong> Chronological analysis of document modification patterns</item>
/// <item><strong>Reporting:</strong> Document modification statistics and timeline analysis</item>
/// <item><strong>Filtering:</strong> Date-based document filtering for recent changes and updates</item>
/// </list>
/// </remarks>
public interface IDocumentWithModificationDate
{
    /// <summary>
    /// Gets the modification date of the document (in UTC).
    /// </summary>
    /// <remarks>
    /// The modification date represents when the document was last modified in the system,
    /// stored in UTC format for consistency across different time zones and supporting
    /// accurate temporal tracking for legal compliance and audit requirements.
    /// </remarks>
    DateTime ModificationDate { get; }
}