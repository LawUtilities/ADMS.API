using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing a matter with complete document collection and activity relationships for comprehensive matter operations.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of a matter within the ADMS legal document management system,
/// providing all properties and relationships from <see cref="ADMS.API.Entities.Matter"/> including the complete 
/// document collection and activity audit trails. It is designed for scenarios requiring full matter context 
/// with comprehensive document management capabilities.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Entity Mirror:</strong> Mirrors all properties and relationships from ADMS.API.Entities.Matter</item>
/// <item><strong>Document Collection Included:</strong> Contains complete document collection with full document information</item>
/// <item><strong>Professional Validation:</strong> Uses ADMS.API.Common.MatterValidationHelper for comprehensive data integrity</item>
/// <item><strong>Computed Properties:</strong> Client-optimized properties for UI display and business logic</item>
/// <item><strong>Collection Validation:</strong> Deep validation of all collections using DtoValidationHelper</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO maintains the complete relationship structure as ADMS.API.Entities.Matter:
/// <list type="bullet">
/// <item><strong>Documents:</strong> Complete document collection with DocumentWithoutRevisionsDto instances</item>
/// <item><strong>MatterActivityUsers:</strong> Complete audit trail of matter lifecycle activities</item>
/// <item><strong>MatterDocumentActivityUsersFrom:</strong> Source-side document transfer operations</item>
/// <item><strong>MatterDocumentActivityUsersTo:</strong> Destination-side document transfer operations</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Matter Operations:</strong> Operations requiring full matter context including documents</item>
/// <item><strong>Document Management:</strong> Document listing, organization, and management within matter context</item>
/// <item><strong>Comprehensive API Responses:</strong> Full matter data including document inventory for client applications</item>
/// <item><strong>Administrative Operations:</strong> Complete matter administration including document oversight</item>
/// <item><strong>Client Presentations:</strong> Complete matter summaries for client communication and reporting</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Matter Management:</strong> Full matter-based document organization and oversight</item>
/// <item><strong>Document Inventory:</strong> Complete document accounting and organization within matters</item>
/// <item><strong>Comprehensive Auditing:</strong> Complete audit trail relationships for legal compliance</item>
/// <item><strong>Client Reporting:</strong> Detailed matter summaries including document status for client communication</item>
/// </list>
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Collection Impact:</strong> May include large document collections; consider selective loading</item>
/// <item><strong>Memory Usage:</strong> Higher memory footprint due to complete document collection and activity relationships</item>
/// <item><strong>Lazy Loading Support:</strong> Collections can be populated on-demand for optimal performance</item>
/// <item><strong>Selective Usage:</strong> Use MatterWithoutDocumentsDto or MatterMinimalDto for performance-critical scenarios</item>
/// </list>
/// 
/// <para><strong>When to Use vs Other Matter DTOs:</strong></para>
/// <list type="bullet">
/// <item><strong>Use MatterDto:</strong> When complete matter and document information is required</item>
/// <item><strong>Use MatterWithoutDocumentsDto:</strong> For matter operations without document collection requirements</item>
/// <item><strong>Use MatterMinimalDto:</strong> For matter selection and identification scenarios</item>
/// <item><strong>Use MatterWithDocumentsDto:</strong> Alternative with full DocumentDto instances (with revisions)</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a complete matter DTO
/// var matterDto = new MatterDto
/// {
///     Id = Guid.NewGuid(),
///     Description = "Smith Family Trust - Complete Matter",
///     IsArchived = false,
///     IsDeleted = false,
///     CreationDate = DateTime.UtcNow,
///     Documents = new List&lt;DocumentWithoutRevisionsDto&gt;()
/// };
/// 
/// // Validating the complete matter DTO
/// var validationResults = MatterDto.ValidateModel(matterDto);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Using computed properties
/// var documentCount = matterDto.DocumentCount;
/// var hasDocuments = matterDto.HasDocuments;
/// var status = matterDto.Status;
/// var isActive = matterDto.IsActive;
/// </code>
/// </example>
public record MatterDto : IValidatableObject, IEquatable<MatterDto>
{
    #region Core Properties

    /// <summary>
    /// Gets the unique identifier for the matter.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the matter within the ADMS system.
    /// The ID corresponds directly to the <see cref="ADMS.API.Entities.Matter.Id"/> property and is
    /// used for establishing relationships, audit trail associations, and all system operations
    /// requiring precise matter identification.
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required Property:</strong> Always required for existing matter operations</item>
    /// <item><strong>Foreign Key Reference:</strong> Used as foreign key in all matter-related junction entities</item>
    /// <item><strong>Document Association:</strong> Links documents to this matter through foreign key relationships</item>
    /// <item><strong>API Operations:</strong> Primary identifier for REST API operations and matter references</item>
    /// <item><strong>Database Queries:</strong> Primary key for all matter-specific database operations</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Matter.Id"/> exactly, ensuring consistency
    /// between entity and DTO representations for reliable matter identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing matter by ID
    /// var matterId = Guid.Parse("60000000-0000-0000-0000-000000000001");
    /// var matter = matters.FirstOrDefault(m => m.Id == matterId);
    /// 
    /// // Using ID for document associations
    /// var matterDocuments = documents.Where(d => d.MatterId == matter.Id);
    /// 
    /// // API response with ID
    /// return Ok(new { Id = matter.Id, Description = matter.Description });
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter ID is required.")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the matter description.
    /// </summary>
    /// <remarks>
    /// The matter description serves as the primary human-readable identifier and must be unique
    /// across the entire system. This field supports both client-based and matter-specific
    /// naming conventions to accommodate various legal practice organization strategies.
    /// 
    /// <para><strong>Validation Rules (via ADMS.API.Common.MatterValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null, empty, or whitespace</item>
    /// <item><strong>Length:</strong> 3-128 characters (matching database constraint)</item>
    /// <item><strong>Uniqueness:</strong> Must be unique across all matters in the system</item>
    /// <item><strong>Format:</strong> Must contain letters and start/end with alphanumeric characters</item>
    /// <item><strong>Reserved Words:</strong> Cannot contain system-reserved terms</item>
    /// <item><strong>Professional Standards:</strong> Must follow professional legal practice naming conventions</item>
    /// </list>
    /// 
    /// <para><strong>Professional Naming Examples:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Client-Based:</strong> "Smith Family Trust", "ABC Corporation Legal Services", "Johnson Estate Planning"</item>
    /// <item><strong>Matter-Specific:</strong> "Smith v. Jones Contract Dispute", "ABC Corp IPO Legal Support", "Johnson Probate Case"</item>
    /// <item><strong>Project-Based:</strong> "Downtown Development Legal Review", "Patent Portfolio Management - XYZ Tech"</item>
    /// <item><strong>Practice Area:</strong> "Corporate Law - Merger Advisory", "Estate Planning - Trust Administration"</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Matter.Description"/> with identical validation
    /// rules and professional standards, ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional matter descriptions
    /// var trustMatter = new MatterDto { Description = "Smith Family Revocable Living Trust" };
    /// var corporateMatter = new MatterDto { Description = "ABC Corporation - Merger Advisory" };
    /// var litigationMatter = new MatterDto { Description = "Johnson v. Smith Commercial Dispute" };
    /// 
    /// // Validation example
    /// var isValid = MatterValidationHelper.IsValidDescription(matter.Description);
    /// 
    /// // UI display
    /// matterLabel.Text = matter.Description;
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter description is required.")]
    [StringLength(128, MinimumLength = 3,
        ErrorMessage = "Matter description must be between 3 and 128 characters.")]
    public required string Description { get; init; }

    /// <summary>
    /// Gets a value indicating whether the matter is archived.
    /// </summary>
    /// <remarks>
    /// The archived status indicates whether the matter has been moved to an inactive state for long-term
    /// retention. This property corresponds to <see cref="ADMS.API.Entities.Matter.IsArchived"/> and affects
    /// matter visibility and accessibility in user interfaces and business operations.
    /// 
    /// <para><strong>Business State Implications:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Active State:</strong> IsArchived = false, IsDeleted = false (normal operations)</item>
    /// <item><strong>Archived State:</strong> IsArchived = true, IsDeleted = false (inactive but accessible)</item>
    /// <item><strong>Deleted State:</strong> IsArchived = true, IsDeleted = true (soft deleted, must be archived)</item>
    /// </list>
    /// 
    /// <para><strong>Document Collection Impact:</strong></para>
    /// When a matter is archived, all associated documents are typically preserved but may
    /// have restricted access or display in normal operations. The complete document collection
    /// remains available for historical reference and compliance requirements.
    /// 
    /// <para><strong>Professional Practice Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Matter Lifecycle:</strong> Supports professional matter lifecycle management workflows</item>
    /// <item><strong>Client Communication:</strong> Clear indication of matter status for client reporting</item>
    /// <item><strong>Practice Organization:</strong> Helps organize active vs completed work for efficiency</item>
    /// <item><strong>Compliance Support:</strong> Maintains matter records for professional and legal compliance</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Matter.IsArchived"/> exactly, ensuring consistency
    /// between entity and DTO representations for reliable state management.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check archive status for UI display
    /// var displayText = matter.IsArchived ? $"{matter.Description} (Archived)" : matter.Description;
    /// 
    /// // Filter active matters
    /// var activeMatters = matters.Where(m => !m.IsArchived && !m.IsDeleted).ToList();
    /// 
    /// // Archive status with document count
    /// if (matter.IsArchived)
    /// {
    ///     Console.WriteLine($"Archived matter '{matter.Description}' contains {matter.DocumentCount} documents");
    /// }
    /// </code>
    /// </example>
    public bool IsArchived { get; init; }

    /// <summary>
    /// Gets a value indicating whether the matter is deleted.
    /// </summary>
    /// <remarks>
    /// The deletion status indicates whether the matter has been soft-deleted while preserving audit trails
    /// and referential integrity. This property corresponds to <see cref="ADMS.API.Entities.Matter.IsDeleted"/>
    /// and is crucial for proper matter filtering and access control.
    /// 
    /// <para><strong>Soft Deletion Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Audit Trail Preservation:</strong> Maintains complete audit trails for legal compliance</item>
    /// <item><strong>Referential Integrity:</strong> Preserves relationships with documents and activities</item>
    /// <item><strong>Recovery Operations:</strong> Enables matter restoration through administrative functions</item>
    /// <item><strong>Historical Reporting:</strong> Supports comprehensive historical analysis and reporting</item>
    /// </list>
    /// 
    /// <para><strong>Document Collection Impact:</strong></para>
    /// When a matter is deleted, the document collection is preserved for audit trail integrity.
    /// However, documents may have their own deletion status and should be handled according to
    /// document retention policies and legal requirements.
    /// 
    /// <para><strong>UI and Access Control:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Normal Operations:</strong> Deleted matters are hidden from standard user interfaces</item>
    /// <item><strong>Administrative Access:</strong> Deleted matters visible in administrative and audit views</item>
    /// <item><strong>Document Access:</strong> Document collections remain intact for audit trail preservation</item>
    /// <item><strong>Restoration Interface:</strong> Available in matter restoration and recovery interfaces</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Matter.IsDeleted"/> exactly, ensuring consistency
    /// between entity and DTO representations for reliable state management.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard filtering excludes deleted matters
    /// var availableMatters = matters.Where(m => !m.IsDeleted).ToList();
    /// 
    /// // Administrative view including deleted matters with indicators
    /// var adminDisplayText = matter.IsDeleted ? $"{matter.Description} (Deleted)" : matter.Description;
    /// 
    /// // Check deletion status with document preservation
    /// if (matter.IsDeleted)
    /// {
    ///     // Documents preserved but access may be restricted
    ///     var preservedDocCount = matter.Documents.Count;
    ///     Console.WriteLine($"Deleted matter has {preservedDocCount} preserved documents");
    /// }
    /// </code>
    /// </example>
    public bool IsDeleted { get; init; }

    /// <summary>
    /// Gets the UTC creation date of the matter.
    /// </summary>
    /// <remarks>
    /// The creation date establishes the temporal foundation for the matter and all associated
    /// activities and documents. This property corresponds to <see cref="ADMS.API.Entities.Matter.CreationDate"/>
    /// and provides chronological context for matter identification and sorting.
    /// 
    /// <para><strong>Temporal Standards:</strong></para>
    /// <list type="bullet">
    /// <item><strong>UTC Storage:</strong> All dates stored in UTC for global consistency and timezone independence</item>
    /// <item><strong>Immutable Value:</strong> Creation date remains constant throughout matter lifecycle</item>
    /// <item><strong>Chronological Foundation:</strong> Provides baseline for all matter-related temporal operations</item>
    /// <item><strong>Professional Standards:</strong> Maintains professional practice temporal consistency</item>
    /// </list>
    /// 
    /// <para><strong>Document Timeline Context:</strong></para>
    /// The matter creation date provides the temporal context for all associated documents
    /// and serves as the baseline for document creation timeline analysis and reporting.
    /// 
    /// <para><strong>Professional Practice Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Matter Timeline:</strong> Establishes matter timeline for billing and case management</item>
    /// <item><strong>Professional Reporting:</strong> Foundation for professional practice analytics and reporting</item>
    /// <item><strong>Client Communication:</strong> Provides matter creation context for client communications</item>
    /// <item><strong>Compliance Documentation:</strong> Supports compliance and professional responsibility requirements</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Matter.CreationDate"/> exactly, ensuring consistency
    /// between entity and DTO representations for reliable temporal tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Display matter age
    /// var matterAge = DateTime.UtcNow - matter.CreationDate;
    /// Console.WriteLine($"Matter is {matterAge.Days} days old with {matter.DocumentCount} documents");
    /// 
    /// // Chronological sorting
    /// var sortedMatters = matters.OrderByDescending(m => m.CreationDate);
    /// 
    /// // Date-based filtering
    /// var recentMatters = matters.Where(m => 
    ///     m.CreationDate >= DateTime.UtcNow.AddMonths(-6) && !m.IsDeleted);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Creation date is required.")]
    public required DateTime CreationDate { get; init; }

    #endregion Core Properties

    #region Navigation Collections

    /// <summary>
    /// Gets the collection of documents associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.Matter.Documents"/> and represents the core
    /// document management functionality of the matter. It contains DocumentWithoutRevisionsDto instances
    /// that provide comprehensive document information without the overhead of revision history.
    /// 
    /// <para><strong>Document Collection Features:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Complete Document Data:</strong> Contains DocumentWithoutRevisionsDto instances with comprehensive document information</item>
    /// <item><strong>Document Management:</strong> Support for various document types, formats, and operations</item>
    /// <item><strong>Activity Integration:</strong> Documents include activity tracking and user attribution</item>
    /// <item><strong>Audit Trail Support:</strong> Complete document activity tracking without revision overhead</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Usage:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Organization:</strong> Complete client or matter-based document organization</item>
    /// <item><strong>Document Inventory:</strong> Comprehensive document accounting and tracking</item>
    /// <item><strong>Client Communication:</strong> Complete document status for client reporting</item>
    /// <item><strong>Compliance Reporting:</strong> Complete document collections for regulatory compliance</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Collection Size:</strong> Can be large for active matters; consider pagination or selective loading</item>
    /// <item><strong>Memory Usage:</strong> DocumentWithoutRevisionsDto instances have moderate memory footprint</item>
    /// <item><strong>Loading Strategy:</strong> Consider explicit loading or projections for large document collections</item>
    /// <item><strong>Filtering Options:</strong> Apply filtering criteria before loading full collections</item>
    /// </list>
    /// 
    /// <para><strong>DTO Composition:</strong></para>
    /// Contains <see cref="DocumentWithoutRevisionsDto"/> instances that include complete document information
    /// and activity relationships for comprehensive document management without revision history overhead.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing complete document collection
    /// foreach (var document in matter.Documents)
    /// {
    ///     Console.WriteLine($"Document: {document.FileName} " +
    ///                      $"(Size: {document.FileSize} bytes, " +
    ///                      $"Checked Out: {document.IsCheckedOut})");
    /// }
    /// 
    /// // Document filtering and analysis
    /// var activeDocuments = matter.Documents.Where(d => !d.IsDeleted).ToList();
    /// var checkedOutDocuments = matter.Documents.Where(d => d.IsCheckedOut).ToList();
    /// 
    /// // Document statistics
    /// var documentStats = new
    /// {
    ///     TotalDocuments = matter.Documents.Count,
    ///     ActiveDocuments = matter.Documents.Count(d => !d.IsDeleted),
    ///     CheckedOutDocuments = matter.Documents.Count(d => d.IsCheckedOut),
    ///     TotalSize = matter.Documents.Sum(d => d.FileSize)
    /// };
    /// </code>
    /// </example>
    public ICollection<DocumentWithoutRevisionsDto> Documents { get; init; } = new List<DocumentWithoutRevisionsDto>();

    /// <summary>
    /// Gets the collection of matter activity users associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.Matter.MatterActivityUsers"/> and maintains
    /// the comprehensive audit trail for all matter-related activities, tracking who performed what
    /// actions on the matter and when. This supports legal compliance, professional accountability,
    /// and practice management requirements.
    /// 
    /// <para><strong>Activity Types Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Matter creation activities</item>
    /// <item><strong>ARCHIVED:</strong> Matter archival activities</item>
    /// <item><strong>DELETED:</strong> Matter deletion activities</item>
    /// <item><strong>RESTORED:</strong> Matter restoration activities</item>
    /// <item><strong>UNARCHIVED:</strong> Matter unarchival activities</item>
    /// <item><strong>VIEWED:</strong> Matter viewing activities</item>
    /// </list>
    /// 
    /// <para><strong>Document Context Integration:</strong></para>
    /// When combined with the document collection, this audit trail provides complete
    /// visibility into both matter-level and document-level activities for comprehensive
    /// legal practice management and compliance reporting.
    /// 
    /// <para><strong>DTO Composition:</strong></para>
    /// Contains <see cref="MatterActivityUserDto"/> instances that include complete matter, activity,
    /// and user information for comprehensive audit trail presentation and analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing matter audit trail with document context
    /// foreach (var activity in matter.MatterActivityUsers.OrderBy(a => a.CreatedAt))
    /// {
    ///     var documentCount = matter.Documents.Count;
    ///     Console.WriteLine($"User {activity.User?.Name} performed {activity.MatterActivity?.Activity} " +
    ///                      $"on matter with {documentCount} documents at {activity.CreatedAt}");
    /// }
    /// 
    /// // Activity analysis with document correlation
    /// var activitySummary = matter.MatterActivityUsers
    ///     .GroupBy(a => a.MatterActivity?.Activity)
    ///     .Select(g => new { Activity = g.Key, Count = g.Count() });
    /// </code>
    /// </example>
    public ICollection<MatterActivityUserDto> MatterActivityUsers { get; init; } = new List<MatterActivityUserDto>();

    /// <summary>
    /// Gets the collection of "from" matter document activity users associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.Matter.MatterDocumentActivityUsersFrom"/> and
    /// tracks all document transfer activities where documents were moved or copied FROM this matter to
    /// other matters. When combined with the complete document collection, this provides comprehensive
    /// tracking of document provenance and transfer operations.
    /// 
    /// <para><strong>Enhanced Context with Documents:</strong></para>
    /// The complete document collection enables correlation between current documents and
    /// historical transfer activities, providing complete visibility into document movement
    /// patterns and matter organization strategies.
    /// 
    /// <para><strong>DTO Composition:</strong></para>
    /// Contains <see cref="MatterDocumentActivityUserFromDto"/> instances that include complete matter,
    /// document, activity, and user information for comprehensive transfer audit trails.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Analyzing document transfers with complete context
    /// var transferredDocuments = matter.MatterDocumentActivityUsersFrom
    ///     .Select(t => t.Document?.FileName)
    ///     .Distinct()
    ///     .ToList();
    /// 
    /// var currentDocuments = matter.Documents.Select(d => d.FileName).ToList();
    /// var documentsStillInMatter = transferredDocuments
    ///     .Where(td => currentDocuments.Contains(td))
    ///     .ToList();
    /// 
    /// Console.WriteLine($"Transferred {transferredDocuments.Count} documents, " +
    ///                  $"{documentsStillInMatter.Count} still in matter");
    /// </code>
    /// </example>
    public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; init; } = new List<MatterDocumentActivityUserFromDto>();

    /// <summary>
    /// Gets the collection of "to" matter document activity users associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.Matter.MatterDocumentActivityUsersTo"/> and
    /// tracks all document transfer activities where documents were moved or copied TO this matter from
    /// other matters. The complete document collection enables full analysis of how documents
    /// arrived in the matter and their current status.
    /// 
    /// <para><strong>Document Provenance with Complete Context:</strong></para>
    /// The combination of transfer audit trails and current document collection provides
    /// complete document provenance tracking, enabling analysis of document sources,
    /// transfer patterns, and matter consolidation strategies.
    /// 
    /// <para><strong>DTO Composition:</strong></para>
    /// Contains <see cref="MatterDocumentActivityUserToDto"/> instances that include complete matter,
    /// document, activity, and user information for comprehensive transfer audit trails.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Complete document provenance analysis
    /// var documentsReceivedFromTransfers = matter.MatterDocumentActivityUsersTo
    ///     .Where(t => t.MatterDocumentActivity?.Activity == "MOVED")
    ///     .Select(t => t.Document?.FileName)
    ///     .ToList();
    /// 
    /// var currentlyAvailableTransferredDocs = matter.Documents
    ///     .Where(d => documentsReceivedFromTransfers.Contains(d.FileName) && !d.IsDeleted)
    ///     .ToList();
    /// 
    /// // Analysis of matter growth through transfers
    /// var growthAnalysis = new
    /// {
    ///     TotalDocumentsReceived = documentsReceivedFromTransfers.Count,
    ///     DocumentsCurrentlyAvailable = currentlyAvailableTransferredDocs.Count,
    ///     MatterGrowthFromTransfers = (double)documentsReceivedFromTransfers.Count / matter.Documents.Count * 100
    /// };
    /// </code>
    /// </example>
    public ICollection<MatterDocumentActivityUserToDto> MatterDocumentActivityUsersTo { get; init; } = new List<MatterDocumentActivityUserToDto>();

    #endregion Navigation Collections

    #region Computed Properties

    /// <summary>
    /// Gets the normalized description for consistent comparison and search operations.
    /// </summary>
    /// <remarks>
    /// This computed property provides a normalized version of the matter description using
    /// ADMS.API.Common.MatterValidationHelper normalization rules for consistent comparison,
    /// uniqueness validation, and search operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var matter1 = new MatterDto { Description = "  Contract   Review  " };
    /// var matter2 = new MatterDto { Description = "Contract Review" };
    /// 
    /// // Both will have the same normalized description: "Contract Review"
    /// bool areEquivalent = matter1.NormalizedDescription == matter2.NormalizedDescription; // true
    /// </code>
    /// </example>
    public string? NormalizedDescription => MatterValidationHelper.NormalizeDescription(Description);

    /// <summary>
    /// Gets the creation date formatted as a localized string for UI display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a user-friendly formatted representation of the creation date
    /// converted to local time, optimized for matter management interface and professional presentation
    /// with document context awareness.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Display matter creation with document context
    /// var displayText = $"Matter '{matter.Description}' created {matter.LocalCreationDateString} " +
    ///                  $"({matter.DocumentCount} documents)";
    /// </code>
    /// </example>
    public string LocalCreationDateString => CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the current status of the matter as a descriptive string.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.Matter.Status"/> and provides
    /// a human-readable status description based on the matter's current state flags.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Matter status: {matter.Status} ({matter.DocumentCount} documents)");
    /// // Output: "Matter status: Active (15 documents)"
    /// </code>
    /// </example>
    public string Status
    {
        get
        {
            return IsDeleted switch
            {
                true when IsArchived => "Archived and Deleted",
                true => "Deleted",
                _ => IsArchived ? "Archived" : "Active"
            };
        }
    }

    /// <summary>
    /// Gets the age of this matter in days since creation.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.Matter.AgeDays"/> and calculates
    /// the number of days since the matter was created, useful for document timeline analysis
    /// and matter lifecycle tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Matter is {matter.AgeDays} days old with {matter.DocumentCount} documents");
    /// 
    /// // Age-based analysis with document correlation
    /// var recentDocumentActivity = matter.Documents
    ///     .Where(d => (DateTime.UtcNow - d.ModificationDate).TotalDays < 7);
    /// </code>
    /// </example>
    public double AgeDays => (DateTime.UtcNow - CreationDate).TotalDays;

    /// <summary>
    /// Gets a value indicating whether this matter is currently active (not archived and not deleted).
    /// </summary>
    /// <remarks>
    /// This computed property mirrors the business logic from <see cref="ADMS.API.Entities.Matter.IsActive"/> and
    /// provides a convenient way to identify active matters for user interface display and business logic enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.IsActive)
    /// {
    ///     // Show matter in active matters list with document count
    ///     Console.WriteLine($"Active matter: {matter.Description} ({matter.DocumentCount} documents)");
    /// }
    /// </code>
    /// </example>
    public bool IsActive => !IsArchived && !IsDeleted;

    /// <summary>
    /// Gets a value indicating whether this matter contains any documents.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.Matter.HasDocuments"/> and provides
    /// a quick way to determine if the matter has any associated documents, useful for validation,
    /// user interface logic, and business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.HasDocuments)
    /// {
    ///     Console.WriteLine($"Matter '{matter.Description}' contains {matter.DocumentCount} documents");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Matter '{matter.Description}' is empty - ready for document upload");
    /// }
    /// </code>
    /// </example>
    public bool HasDocuments => Documents.Count > 0;

    /// <summary>
    /// Gets the total number of documents in this matter.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.Matter.DocumentCount"/> and provides
    /// the total count of documents associated with the matter, useful for reporting, user interface
    /// display, and practice management analytics.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Matter contains {matter.DocumentCount} total documents");
    /// 
    /// // Finding matters with many documents
    /// var largeMatters = matters.Where(m => m.DocumentCount > 100);
    /// </code>
    /// </example>
    public int DocumentCount => Documents.Count;

    /// <summary>
    /// Gets the number of active (non-deleted) documents in this matter.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.Matter.ActiveDocumentCount"/> and provides
    /// the count of active documents, excluding those that have been soft-deleted, useful for active
    /// document management and user interface display.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Matter has {matter.ActiveDocumentCount} active documents " +
    ///                  $"out of {matter.DocumentCount} total");
    /// 
    /// // Document status analysis
    /// var deletedDocuments = matter.DocumentCount - matter.ActiveDocumentCount;
    /// if (deletedDocuments > 0)
    /// {
    ///     Console.WriteLine($"Warning: {deletedDocuments} documents are marked as deleted");
    /// }
    /// </code>
    /// </example>
    public int ActiveDocumentCount => Documents.Count(d => !d.IsDeleted);

    /// <summary>
    /// Gets the number of documents that are currently checked out.
    /// </summary>
    /// <remarks>
    /// This computed property provides the count of documents that are currently checked out
    /// for editing, useful for understanding document availability and workflow status.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.CheckedOutDocumentCount > 0)
    /// {
    ///     Console.WriteLine($"Warning: {matter.CheckedOutDocumentCount} documents are currently checked out");
    /// }
    /// 
    /// // Workflow status check
    /// var workflowStatus = matter.CheckedOutDocumentCount > 0 ? "In Progress" : "Available";
    /// </code>
    /// </example>
    public int CheckedOutDocumentCount => Documents.Count(d => d.IsCheckedOut);

    /// <summary>
    /// Gets the total file size of all documents in this matter.
    /// </summary>
    /// <remarks>
    /// This computed property calculates the total storage space used by all documents
    /// in the matter, useful for storage management and billing analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// var totalSizeMB = matter.TotalDocumentSize / (1024.0 * 1024.0);
    /// Console.WriteLine($"Matter uses {totalSizeMB:F2} MB of storage");
    /// 
    /// // Storage analysis
    /// var storageStats = new
    /// {
    ///     TotalSizeBytes = matter.TotalDocumentSize,
    ///     TotalSizeMB = totalSizeMB,
    ///     AverageDocumentSize = matter.DocumentCount > 0 ? matter.TotalDocumentSize / matter.DocumentCount : 0
    /// };
    /// </code>
    /// </example>
    public long TotalDocumentSize => Documents.Sum(d => d.FileSize);

    /// <summary>
    /// Gets a value indicating whether this matter has any activity history.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.Matter.HasActivityHistory"/> and
    /// determines if the matter has any recorded activities, useful for audit trail validation and user interface logic.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.HasActivityHistory)
    /// {
    ///     // Display activity history with document context
    ///     DisplayActivityHistory(matter, includeDocumentActivities: true);
    /// }
    /// </code>
    /// </example>
    public bool HasActivityHistory => MatterActivityUsers.Count > 0;

    /// <summary>
    /// Gets the total count of all activities (matter + document transfer) for this matter.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.Matter.TotalActivityCount"/> and provides
    /// a comprehensive count of all activities associated with the matter, including general matter activities
    /// and document transfer activities.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Total activities: {matter.TotalActivityCount} " +
    ///                  $"across {matter.DocumentCount} documents");
    /// 
    /// // Finding highly active matters
    /// var activeMatters = matters
    ///     .Where(m => m.TotalActivityCount > 10 && m.DocumentCount > 5)
    ///     .OrderByDescending(m => m.TotalActivityCount);
    /// </code>
    /// </example>
    public int TotalActivityCount =>
        MatterActivityUsers.Count +
        MatterDocumentActivityUsersFrom.Count +
        MatterDocumentActivityUsersTo.Count;

    /// <summary>
    /// Gets a value indicating whether this matter DTO has valid data for system operations.
    /// </summary>
    /// <remarks>
    /// This property provides a quick validation check without running full validation logic,
    /// useful for UI scenarios where immediate feedback is needed before processing matter operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.IsValid)
    /// {
    ///     // Proceed with business operations including document processing
    ///     ProcessMatterWithDocuments(matter);
    /// }
    /// else
    /// {
    ///     // Show validation errors to user
    ///     DisplayValidationErrors(matter);
    /// }
    /// </code>
    /// </example>
    public bool IsValid =>
        Id != Guid.Empty &&
        MatterValidationHelper.IsValidDescription(Description) &&
        MatterValidationHelper.IsValidDate(CreationDate) &&
        MatterValidationHelper.IsValidArchiveState(IsArchived, IsDeleted);

    /// <summary>
    /// Gets the display text suitable for UI controls and matter identification.
    /// </summary>
    /// <remarks>
    /// Provides a consistent format for displaying matter information in UI elements,
    /// including document count for additional context.
    /// </remarks>
    /// <example>
    /// <code>
    /// var matter = new MatterDto 
    /// { 
    ///     Description = "Smith Family Trust", 
    ///     Documents = documentList 
    /// };
    /// var displayText = matter.DisplayText; // Returns "Smith Family Trust"
    /// 
    /// // UI usage with document context
    /// var listItem = $"{matter.DisplayText} ({matter.DocumentCount} documents)";
    /// </code>
    /// </example>
    public string DisplayText => Description;

    /// <summary>
    /// Gets comprehensive matter metrics including document statistics for reporting and analysis.
    /// </summary>
    /// <remarks>
    /// This property provides a structured object containing key metrics and information
    /// for comprehensive matter analysis including document inventory and professional reporting purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var metrics = matter.MatterMetrics;
    /// // Access comprehensive metrics including document statistics
    /// </code>
    /// </example>
    public object MatterMetrics => new
    {
        MatterInfo = new
        {
            Id,
            Description,
            NormalizedDescription,
            Status,
            IsActive,
            LocalCreationDateString,
            DisplayText
        },
        StateInfo = new
        {
            IsArchived,
            IsDeleted,
            CreationDate,
            AgeDays
        },
        ActivityMetrics = new
        {
            TotalActivityCount,
            HasActivityHistory,
            MatterActivityCount = MatterActivityUsers.Count,
            TransferFromCount = MatterDocumentActivityUsersFrom.Count,
            TransferToCount = MatterDocumentActivityUsersTo.Count
        },
        DocumentMetrics = new
        {
            DocumentCount,
            ActiveDocumentCount,
            CheckedOutDocumentCount,
            HasDocuments,
            TotalDocumentSize,
            DeletedDocumentCount = DocumentCount - ActiveDocumentCount,
            DocumentTypes = Documents.GroupBy(d => d.Extension).Select(g => new { Type = g.Key, Count = g.Count() }).ToList(),
            RecentDocuments = Documents.OfType<IDocumentWithCreationDate>().Where(d => (DateTime.UtcNow - d.CreationDate).TotalDays <= 7).Count(),
            AverageDocumentSize = DocumentCount > 0 ? TotalDocumentSize / DocumentCount : 0
        }
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="MatterDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using the ADMS.API.Common.MatterValidationHelper for consistency 
    /// with entity validation rules, including validation of the complete document collection.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDto 
    /// { 
    ///     Id = Guid.Empty, // Invalid
    ///     Description = "",
    ///     CreationDate = DateTime.MinValue,
    ///     Documents = new List&lt;DocumentWithoutRevisionsDto&gt; { null } // Invalid document
    /// };
    /// 
    /// var context = new ValidationContext(dto);
    /// var results = dto.Validate(context);
    /// 
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"Validation Error: {result.ErrorMessage}");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate matter ID
        foreach (var result in ValidateMatterId())
            yield return result;

        // Validate matter description using centralized helper
        foreach (var result in ValidateDescription())
            yield return result;

        // Validate creation date
        foreach (var result in ValidateCreationDate())
            yield return result;

        // Validate state consistency
        foreach (var result in ValidateStateConsistency())
            yield return result;

        // Validate document collection using centralized helper
        foreach (var result in DtoValidationHelper.ValidateCollection(Documents, nameof(Documents)))
            yield return result;

        // Validate activity collections using centralized helper
        foreach (var result in DtoValidationHelper.ValidateCollection(MatterActivityUsers, nameof(MatterActivityUsers)))
            yield return result;

        foreach (var result in DtoValidationHelper.ValidateCollection(MatterDocumentActivityUsersFrom, nameof(MatterDocumentActivityUsersFrom)))
            yield return result;

        foreach (var result in DtoValidationHelper.ValidateCollection(MatterDocumentActivityUsersTo, nameof(MatterDocumentActivityUsersTo)))
            yield return result;
    }

    /// <summary>
    /// Validates the <see cref="Id"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the matter ID.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.MatterValidationHelper.ValidateMatterId for consistent validation.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateMatterId()
    {
        return MatterValidationHelper.ValidateMatterId(Id, nameof(Id));
    }

    /// <summary>
    /// Validates the <see cref="Description"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the matter description.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.MatterValidationHelper.ValidateDescription for comprehensive validation.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateDescription()
    {
        return MatterValidationHelper.ValidateDescription(Description, nameof(Description));
    }

    /// <summary>
    /// Validates the <see cref="CreationDate"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the creation date.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.MatterValidationHelper.ValidateDate to ensure the creation date
    /// meets professional standards and temporal consistency requirements.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateCreationDate()
    {
        return MatterValidationHelper.ValidateDate(CreationDate, nameof(CreationDate));
    }

    /// <summary>
    /// Validates the consistency of archive and delete states using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for state consistency.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.MatterValidationHelper.ValidateStates to ensure that archive and
    /// delete states follow business rules.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateStateConsistency()
    {
        return MatterValidationHelper.ValidateStates(IsArchived, IsDeleted, nameof(IsArchived), nameof(IsDeleted));
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="MatterDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The MatterDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate MatterDto instances
    /// without requiring a ValidationContext.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     Description = "Smith Family Trust",
    ///     CreationDate = DateTime.UtcNow,
    ///     Documents = documentList
    /// };
    /// 
    /// var results = MatterDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Matter validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a MatterDto from an ADMS.API.Entities.Matter entity with validation.
    /// </summary>
    /// <param name="matter">The Matter entity to convert. Cannot be null.</param>
    /// <param name="includeActivities">Whether to include activity collections in the conversion.</param>
    /// <returns>A valid MatterDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when matter is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create MatterDto instances from
    /// ADMS.API.Entities.Matter entities with automatic validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity with complete document collection
    /// var entity = new ADMS.API.Entities.Matter 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     Description = "Smith Family Trust",
    ///     CreationDate = DateTime.UtcNow,
    ///     IsArchived = false,
    ///     IsDeleted = false,
    ///     Documents = documentEntityCollection
    /// };
    /// 
    /// var dto = MatterDto.FromEntity(entity, includeActivities: false);
    /// </code>
    /// </example>
    public static MatterDto FromEntity([NotNull] Entities.Matter matter, bool includeActivities = false)
    {
        ArgumentNullException.ThrowIfNull(matter, nameof(matter));

        var dto = new MatterDto
        {
            Id = matter.Id,
            Description = matter.Description,
            IsArchived = matter.IsArchived,
            IsDeleted = matter.IsDeleted,
            CreationDate = matter.CreationDate
        };

        // Note: In practice, document and activity collections would be mapped
        // using a mapping framework like AutoMapper or Mapster

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterDto from entity: {errorMessages}");

    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this matter can be archived based on business rules including document status.
    /// </summary>
    /// <returns>true if the matter can be archived; otherwise, false.</returns>
    /// <remarks>
    /// This method mirrors the business logic from <see cref="ADMS.API.Entities.Matter.CanBeArchived"/> and
    /// considers both matter state and document collection status when determining archival eligibility.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.CanBeArchived())
    /// {
    ///     // Check if all documents are in appropriate state for archival
    ///     var checkedOutDocs = matter.Documents.Where(d => d.IsCheckedOut).ToList();
    ///     if (checkedOutDocs.Any())
    ///     {
    ///         Console.WriteLine($"Warning: {checkedOutDocs.Count} documents are still checked out");
    ///     }
    /// }
    /// </code>
    /// </example>
    public bool CanBeArchived() => !IsArchived && !IsDeleted;

    /// <summary>
    /// Determines whether this matter can be restored from deleted state.
    /// </summary>
    /// <returns>true if the matter can be restored; otherwise, false.</returns>
    /// <remarks>
    /// This method mirrors the business logic from <see cref="ADMS.API.Entities.Matter.CanBeRestored"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.CanBeRestored())
    /// {
    ///     Console.WriteLine($"Matter can be restored with {matter.DocumentCount} documents");
    /// }
    /// </code>
    /// </example>
    public bool CanBeRestored() => IsDeleted;

    /// <summary>
    /// Determines whether this matter can be safely deleted considering document status.
    /// </summary>
    /// <returns>true if the matter can be deleted; otherwise, false.</returns>
    /// <remarks>
    /// This method mirrors the business logic from <see cref="ADMS.API.Entities.Matter.CanBeDeleted"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!matter.CanBeDeleted())
    /// {
    ///     var checkedOutDocs = matter.Documents.Where(d => d.IsCheckedOut).ToList();
    ///     Console.WriteLine($"Cannot delete: {checkedOutDocs.Count} documents are checked out");
    /// }
    /// </code>
    /// </example>
    public bool CanBeDeleted() => !IsDeleted && Documents.All(d => d is { IsDeleted: true, IsCheckedOut: false });

    /// <summary>
    /// Gets activities of a specific type performed on this matter.
    /// </summary>
    /// <param name="activityType">The activity type to filter by.</param>
    /// <returns>A collection of matching activities.</returns>
    /// <remarks>
    /// This method enables filtering activities by type for specific analysis and reporting purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var creationActivities = matter.GetActivitiesByType("CREATED");
    /// var archivalActivities = matter.GetActivitiesByType("ARCHIVED");
    /// </code>
    /// </example>
    public IEnumerable<MatterActivityUserDto> GetActivitiesByType(string activityType)
    {
        if (string.IsNullOrWhiteSpace(activityType))
            return [];

        return MatterActivityUsers
            .Where(a => string.Equals(a.MatterActivity?.Activity, activityType, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(a => a.CreatedAt);
    }

    /// <summary>
    /// Gets the most recent activity performed on this matter.
    /// </summary>
    /// <returns>The most recent MatterActivityUserDto, or null if no activities exist.</returns>
    /// <remarks>
    /// This method provides access to the most recent activity with document context awareness.
    /// </remarks>
    /// <example>
    /// <code>
    /// var lastActivity = matter.GetMostRecentActivity();
    /// if (lastActivity != null)
    /// {
    ///     Console.WriteLine($"Last activity: {lastActivity.MatterActivity?.Activity} " +
    ///                      $"by {lastActivity.User?.Name}. Matter has {matter.DocumentCount} documents.");
    /// }
    /// </code>
    /// </example>
    public MatterActivityUserDto? GetMostRecentActivity()
    {
        return MatterActivityUsers
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets documents that match specific criteria within the matter.
    /// </summary>
    /// <param name="predicate">The filtering criteria for documents.</param>
    /// <returns>A collection of matching documents.</returns>
    /// <remarks>
    /// This method provides flexible document filtering within the matter context.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Find recent documents
    /// var recentDocs = matter.GetDocuments(d => d.CreationDate >= DateTime.UtcNow.AddDays(-30));
    /// 
    /// // Find checked out documents
    /// var checkedOutDocs = matter.GetDocuments(d => d.IsCheckedOut);
    /// </code>
    /// </example>
    public IEnumerable<DocumentWithoutRevisionsDto> GetDocuments(Func<DocumentWithoutRevisionsDto, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return Documents.Where(predicate);
    }

    /// <summary>
    /// Gets comprehensive document statistics for this matter.
    /// </summary>
    /// <returns>A dictionary containing detailed document statistics.</returns>
    /// <remarks>
    /// This method provides detailed document analysis for reporting and management purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = matter.GetDocumentStatistics();
    /// Console.WriteLine($"Document Analysis for {matter.Description}:");
    /// Console.WriteLine($"  Total Documents: {stats["TotalDocuments"]}");
    /// Console.WriteLine($"  Active Documents: {stats["ActiveDocuments"]}");
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetDocumentStatistics()
    {
        var documentsByType = Documents
            .Where(d => !d.IsDeleted)
            .GroupBy(d => d.Extension)
            .ToDictionary(g => g.Key, g => g.Count());

        return new Dictionary<string, object>
        {
            ["TotalDocuments"] = DocumentCount,
            ["ActiveDocuments"] = ActiveDocumentCount,
            ["DeletedDocuments"] = DocumentCount - ActiveDocumentCount,
            ["CheckedOutDocuments"] = CheckedOutDocumentCount,
            ["TotalSizeBytes"] = TotalDocumentSize,
            ["TotalSizeMB"] = TotalDocumentSize / (1024.0 * 1024.0),
            ["DocumentsByType"] = documentsByType,
            ["AverageFileSize"] = Documents.Any() ? Documents.Average(d => d.FileSize) : 0,
            ["LargestDocument"] = Documents.Any() ? Documents.Max(d => d.FileSize) : 0,
            ["RecentDocuments"] = Documents.OfType<IDocumentWithCreationDate>().Count(d => (DateTime.UtcNow - d.CreationDate).TotalDays <= 30),
            ["HasDocuments"] = HasDocuments
        }.ToImmutableDictionary();
    }

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterDto is equal to the current MatterDto.
    /// </summary>
    /// <param name="other">The MatterDto to compare with the current MatterDto.</param>
    /// <returns>true if the specified MatterDto is equal to the current MatterDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property when available.
    /// </remarks>
    public virtual bool Equals(MatterDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterDto.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterDto.
    /// </summary>
    /// <returns>A string that represents the current MatterDto.</returns>
    /// <remarks>
    /// The string representation includes key identifying information including document count.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDto 
    /// { 
    ///     Id = Guid.Parse("60000000-0000-0000-0000-000000000001"),
    ///     Description = "Smith Family Trust",
    ///     Documents = documentList
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "Matter: Smith Family Trust (60000000-0000-0000-0000-000000000001) - Active (15 documents)"
    /// </code>
    /// </example>
    public override string ToString() => $"Matter: {Description} ({Id}) - {Status} ({DocumentCount} documents)";

    #endregion String Representation
}