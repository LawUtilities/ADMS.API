using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using ADMS.API.Common;

namespace ADMS.API.Models;

/// <summary>
/// Comprehensive Data Transfer Object representing a matter without its associated documents for efficient matter management operations.
/// </summary>
/// <remarks>
/// This DTO serves as a complete representation of a matter within the ADMS legal document management system,
/// excluding document collections for performance optimization while maintaining all activity relationships and
/// audit trail capabilities. It mirrors the structure of <see cref="ADMS.API.Entities.Matter"/> while providing
/// comprehensive validation and computed properties for client-side operations.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Document-Free Representation:</strong> Excludes document collections for optimal performance in matter-focused operations</item>
/// <item><strong>Complete Activity Integration:</strong> Includes all matter and document transfer activity collections</item>
/// <item><strong>Professional Validation:</strong> Uses ADMS.API.Common.MatterValidationHelper for comprehensive data integrity</item>
/// <item><strong>Computed Properties:</strong> Client-optimized properties for UI display and business logic</item>
/// <item><strong>Collection Validation:</strong> Deep validation of all activity collections using DtoValidationHelper</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO maintains the same relationship structure as ADMS.API.Entities.Matter, excluding Documents:
/// <list type="bullet">
/// <item><strong>MatterActivityUsers:</strong> Complete audit trail of matter lifecycle activities</item>
/// <item><strong>MatterDocumentActivityUsersFrom:</strong> Source-side document transfer operations</item>
/// <item><strong>MatterDocumentActivityUsersTo:</strong> Destination-side document transfer operations</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter Management:</strong> Primary matter administration and lifecycle operations</item>
/// <item><strong>API Responses:</strong> Matter data without document overhead for performance-critical scenarios</item>
/// <item><strong>Activity Analysis:</strong> Matter activity tracking and audit trail analysis</item>
/// <item><strong>UI Operations:</strong> Matter selection, listing, and basic operations</item>
/// <item><strong>Transfer Operations:</strong> Document transfer between matters with complete audit trails</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter Organization:</strong> Flexible matter-based or client-based document organization</item>
/// <item><strong>Activity Attribution:</strong> Complete user accountability for all matter operations</item>
/// <item><strong>Audit Compliance:</strong> Comprehensive audit trail relationships for legal compliance</item>
/// <item><strong>Professional Standards:</strong> Maintains professional naming conventions and validation rules</item>
/// </list>
/// 
/// <para><strong>Business Rules:</strong></para>
/// <list type="bullet">
/// <item><strong>Unique Descriptions:</strong> Matter descriptions must be unique system-wide</item>
/// <item><strong>Archive State Consistency:</strong> Deleted matters must be archived to maintain audit trails</item>
/// <item><strong>Creation Date Immutability:</strong> Creation dates establish temporal foundation and cannot be modified</item>
/// <item><strong>Reserved Word Protection:</strong> Prevents use of system-reserved terms in descriptions</item>
/// </list>
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Exclusion:</strong> Excludes potentially large document collections for optimal performance</item>
/// <item><strong>Lazy Loading Support:</strong> Activity collections can be populated on-demand</item>
/// <item><strong>Selective Loading:</strong> Individual activity collections can be loaded independently</item>
/// <item><strong>Validation Optimization:</strong> Efficient validation using centralized helpers</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a matter without documents DTO
/// var matterDto = new MatterWithoutDocumentsDto
/// {
///     Id = Guid.NewGuid(),
///     Description = "Smith Family Trust",
///     IsArchived = false,
///     IsDeleted = false,
///     CreationDate = DateTime.UtcNow
/// };
/// 
/// // Validating the matter DTO
/// var validationResults = MatterWithoutDocumentsDto.ValidateModel(matterDto);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Using computed properties
/// var status = matterDto.Status;
/// var isActive = matterDto.IsActive;
/// var displayText = matterDto.DisplayText;
/// </code>
/// </example>
public class MatterWithoutDocumentsDto : IValidatableObject, IEquatable<MatterWithoutDocumentsDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the matter.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the matter within the ADMS system.
    /// The ID corresponds directly to the <see cref="ADMS.API.Entities.Matter.Id"/> property and is
    /// used for establishing relationships, audit trail associations, and all system operations
    /// requiring precise matter identification.
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Optional for Creation:</strong> Can be empty for new matter creation operations</item>
    /// <item><strong>Required for Updates:</strong> Must be provided when updating existing matters</item>
    /// <item><strong>Foreign Key Reference:</strong> Referenced in all matter-related junction entities</item>
    /// <item><strong>Database Operations:</strong> Primary key for matter-related database queries</item>
    /// <item><strong>API Operations:</strong> Matter identification in REST API operations</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Matter.Id"/> with identical behavior,
    /// ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // For existing matters
    /// var existingMatter = new MatterWithoutDocumentsDto 
    /// { 
    ///     Id = Guid.Parse("60000000-0000-0000-0000-000000000001"),
    ///     Description = "Smith Family Trust"
    /// };
    /// 
    /// // For new matters (ID will be generated by database)
    /// var newMatter = new MatterWithoutDocumentsDto 
    /// { 
    ///     Id = Guid.Empty,
    ///     Description = "New Client Matter"
    /// };
    /// </code>
    /// </example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the matter description.
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
    /// </list>
    /// 
    /// <para><strong>Professional Naming Examples:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Client-Based:</strong> "Smith Family Trust", "ABC Corporation", "Johnson Estate"</item>
    /// <item><strong>Matter-Specific:</strong> "Smith v. Jones Litigation", "ABC Corp Merger", "Johnson Will Contest"</item>
    /// <item><strong>Project-Based:</strong> "Downtown Development Project", "Patent Application - XYZ Technology"</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Matter.Description"/> with identical validation
    /// rules and business logic to ensure consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Client-based matter descriptions
    /// var clientMatter = new MatterWithoutDocumentsDto { Description = "Smith Family Trust" };
    /// var corporateMatter = new MatterWithoutDocumentsDto { Description = "ABC Corporation" };
    /// 
    /// // Matter-specific descriptions
    /// var litigationMatter = new MatterWithoutDocumentsDto { Description = "Smith v. Jones Contract Dispute" };
    /// var realEstateMatter = new MatterWithoutDocumentsDto { Description = "123 Main St Property Purchase" };
    /// 
    /// // Validation example
    /// bool isValidDescription = MatterValidationHelper.IsValidDescription(matter.Description);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter description is required.")]
    [StringLength(MatterValidationHelper.MaxDescriptionLength, MinimumLength = MatterValidationHelper.MinDescriptionLength,
        ErrorMessage = "Matter description must be between 3 and 128 characters.")]
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the matter is archived.
    /// </summary>
    /// <remarks>
    /// The archived status indicates that the matter has been moved to an inactive state,
    /// typically when legal work is complete but the matter and its documents must be retained
    /// for legal, professional, or client requirements. This property corresponds to
    /// <see cref="ADMS.API.Entities.Matter.IsArchived"/>.
    /// 
    /// <para><strong>Archival Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Active State:</strong> IsArchived = false, IsDeleted = false</item>
    /// <item><strong>Archived State:</strong> IsArchived = true, IsDeleted = false</item>
    /// <item><strong>Deleted State:</strong> IsArchived = true, IsDeleted = true (deleted matters must be archived)</item>
    /// </list>
    /// 
    /// <para><strong>Professional Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Practice Management:</strong> Distinguishes between active and completed work</item>
    /// <item><strong>Document Retention:</strong> Supports compliance with document retention policies</item>
    /// <item><strong>Client Communication:</strong> Clear indication of matter status for client reporting</item>
    /// <item><strong>Legal Compliance:</strong> Maintains professional practice standards</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Archive a completed matter
    /// var completedMatter = new MatterWithoutDocumentsDto 
    /// { 
    ///     Description = "Johnson Estate - Probate Complete",
    ///     IsArchived = true,
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// 
    /// // Check archival status
    /// if (matter.IsArchived && !matter.IsDeleted)
    /// {
    ///     Console.WriteLine($"Matter '{matter.Description}' is archived but not deleted");
    /// }
    /// </code>
    /// </example>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the matter is deleted.
    /// </summary>
    /// <remarks>
    /// The deletion status indicates that the matter has been marked for removal while preserving
    /// the data for audit trail integrity and potential restoration. This implements soft deletion
    /// to maintain legal compliance and professional standards, corresponding to
    /// <see cref="ADMS.API.Entities.Matter.IsDeleted"/>.
    /// 
    /// <para><strong>Soft Deletion Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Audit Trail Preservation:</strong> Maintains complete audit trails for legal compliance</item>
    /// <item><strong>Referential Integrity:</strong> Preserves relationships with documents and activities</item>
    /// <item><strong>Recovery Operations:</strong> Enables matter restoration if deletion was in error</item>
    /// <item><strong>Historical Reporting:</strong> Supports historical reporting and analysis</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Deleted matters are typically filtered from normal operations</item>
    /// <item>Deletion status is tracked through matter activity audit entries</item>
    /// <item>Restoration operations can reverse soft deletion</item>
    /// <item>Administrative users may view and manage deleted matters</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Active matter
    /// var activeMatter = new MatterWithoutDocumentsDto 
    /// { 
    ///     Description = "Active Client Matter", 
    ///     IsDeleted = false 
    /// };
    /// 
    /// // Filtering active matters
    /// var activeMatters = matters.Where(m => !m.IsDeleted).ToList();
    /// 
    /// // Deleted matter for administrative view
    /// var deletedMatter = new MatterWithoutDocumentsDto 
    /// { 
    ///     Description = "Old Completed Matter", 
    ///     IsArchived = true,
    ///     IsDeleted = true 
    /// };
    /// </code>
    /// </example>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the UTC creation date of the matter.
    /// </summary>
    /// <remarks>
    /// The creation date establishes the temporal foundation for the matter and all associated
    /// activities. It corresponds to <see cref="ADMS.API.Entities.Matter.CreationDate"/> and should
    /// be set when the matter is created and remain immutable thereafter to maintain audit trail
    /// integrity and professional compliance standards.
    /// 
    /// <para><strong>Temporal Standards:</strong></para>
    /// <list type="bullet">
    /// <item><strong>UTC Storage:</strong> All dates stored in UTC for global consistency</item>
    /// <item><strong>Automatic Setting:</strong> Typically set during matter creation</item>
    /// <item><strong>Immutability:</strong> Should not be modified after creation</item>
    /// <item><strong>Audit Foundation:</strong> Establishes chronological basis for all activities</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements (via ADMS.API.Common.MatterValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Valid Range:</strong> Between January 1, 1980, and current time (with tolerance)</item>
    /// <item><strong>Not Future:</strong> Cannot be in the future to prevent temporal inconsistencies</item>
    /// <item><strong>Not Default:</strong> Must be a valid DateTime, not DateTime.MinValue</item>
    /// </list>
    /// 
    /// <para><strong>Professional Usage:</strong></para>
    /// <list type="bullet">
    /// <item>Establishes matter timeline for billing and case management</item>
    /// <item>Supports professional reporting and practice analytics</item>
    /// <item>Provides foundation for document retention and compliance policies</item>
    /// <item>Enables historical analysis and matter lifecycle tracking</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Matter creation with UTC timestamp
    /// var newMatter = new MatterWithoutDocumentsDto
    /// {
    ///     Description = "New Client Intake - XYZ Corp",
    ///     CreationDate = DateTime.UtcNow,  // Always use UTC
    ///     IsArchived = false,
    ///     IsDeleted = false
    /// };
    /// 
    /// // Calculate matter age
    /// var matterAge = DateTime.UtcNow - matter.CreationDate;
    /// Console.WriteLine($"Matter is {matterAge.Days} days old");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Creation date is required.")]
    public required DateTime CreationDate { get; set; } = DateTime.UtcNow;

    #endregion Core Properties

    #region Navigation Collections

    /// <summary>
    /// Gets or sets the collection of matter activity users associated with this matter.
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
    /// <para><strong>Audit Trail Significance:</strong></para>
    /// Each association provides:
    /// <list type="bullet">
    /// <item><strong>User Attribution:</strong> Complete user accountability for matter operations</item>
    /// <item><strong>Activity Classification:</strong> Categorization of operations for audit analysis</item>
    /// <item><strong>Temporal Tracking:</strong> Precise temporal tracking for audit chronology</item>
    /// <item><strong>Legal Compliance:</strong> Complete audit trail for legal and regulatory requirements</item>
    /// </list>
    /// 
    /// <para><strong>DTO Composition:</strong></para>
    /// Contains <see cref="MatterActivityUserDto"/> instances that include complete matter, activity,
    /// and user information for comprehensive audit trail presentation and analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing matter audit trail
    /// foreach (var activity in matter.MatterActivityUsers.OrderBy(a => a.CreatedAt))
    /// {
    ///     Console.WriteLine($"User {activity.User?.Name} performed {activity.MatterActivity?.Activity} " +
    ///                      $"on matter at {activity.CreatedAt}");
    /// }
    /// 
    /// // Finding who created this matter
    /// var creator = matter.MatterActivityUsers
    ///     .FirstOrDefault(ma => ma.MatterActivity?.Activity == "CREATED")?.User;
    /// </code>
    /// </example>
    public ICollection<MatterActivityUserDto> MatterActivityUsers { get; set; } = new List<MatterActivityUserDto>();

    /// <summary>
    /// Gets or sets the collection of "from" matter document activity users associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.Matter.MatterDocumentActivityUsersFrom"/> and
    /// tracks all document transfer activities where documents were moved or copied FROM this matter to
    /// other matters. This provides the source-side audit trail for document transfer operations,
    /// essential for legal compliance and document custody tracking.
    /// 
    /// <para><strong>Transfer Operations Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> Documents moved from this matter to another matter</item>
    /// <item><strong>COPIED:</strong> Documents copied from this matter to another matter</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Custody Chain:</strong> Maintains complete document custody tracking</item>
    /// <item><strong>Professional Responsibility:</strong> Detailed user attribution for document handling</item>
    /// <item><strong>Client Confidentiality:</strong> Document security and confidentiality monitoring</item>
    /// <item><strong>Audit Trail Preservation:</strong> Complete audit trail for compliance requirements</item>
    /// </list>
    /// 
    /// <para><strong>DTO Composition:</strong></para>
    /// Contains <see cref="MatterDocumentActivityUserFromDto"/> instances that include complete matter,
    /// document, activity, and user information for comprehensive transfer audit trails.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing documents transferred FROM this matter
    /// foreach (var transferFrom in matter.MatterDocumentActivityUsersFrom)
    /// {
    ///     Console.WriteLine($"Document {transferFrom.Document?.FileName} " +
    ///                      $"{transferFrom.MatterDocumentActivity?.Activity} from this matter " +
    ///                      $"by {transferFrom.User?.Name} at {transferFrom.CreatedAt}");
    /// }
    /// 
    /// // Counting transfer activities by type
    /// var moveCount = matter.MatterDocumentActivityUsersFrom
    ///     .Count(t => t.MatterDocumentActivity?.Activity == "MOVED");
    /// </code>
    /// </example>
    public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; set; } = new List<MatterDocumentActivityUserFromDto>();

    /// <summary>
    /// Gets or sets the collection of "to" matter document activity users associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.Matter.MatterDocumentActivityUsersTo"/> and
    /// tracks all document transfer activities where documents were moved or copied TO this matter from
    /// other matters. This provides the destination-side audit trail for document transfer operations,
    /// completing the bidirectional tracking system for legal compliance.
    /// 
    /// <para><strong>Transfer Operations Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> Documents moved to this matter from another matter</item>
    /// <item><strong>COPIED:</strong> Documents copied to this matter from another matter</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Provenance:</strong> Complete tracking of document sources and origins</item>
    /// <item><strong>Client Notification:</strong> Ability to notify clients about document additions</item>
    /// <item><strong>Matter Completeness:</strong> Validation and quality assurance for matter organization</item>
    /// <item><strong>Professional Responsibility:</strong> Complete user accountability for document custody</item>
    /// </list>
    /// 
    /// <para><strong>Bidirectional Audit System:</strong></para>
    /// This collection complements MatterDocumentActivityUsersFrom to provide complete bidirectional
    /// tracking of document transfers, ensuring every document movement between matters is fully
    /// documented and traceable for legal and professional compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing documents transferred TO this matter
    /// foreach (var transferTo in matter.MatterDocumentActivityUsersTo)
    /// {
    ///     Console.WriteLine($"Document {transferTo.Document?.FileName} " +
    ///                      $"{transferTo.MatterDocumentActivity?.Activity} to this matter " +
    ///                      $"by {transferTo.User?.Name} at {transferTo.CreatedAt}");
    /// }
    /// 
    /// // Recent inbound transfers analysis
    /// var recentTransfersIn = matter.MatterDocumentActivityUsersTo
    ///     .Where(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-7))
    ///     .OrderByDescending(t => t.CreatedAt);
    /// </code>
    /// </example>
    public ICollection<MatterDocumentActivityUserToDto> MatterDocumentActivityUsersTo { get; set; } = new List<MatterDocumentActivityUserToDto>();

    #endregion Navigation Collections

    #region Computed Properties

    /// <summary>
    /// Gets the creation date formatted as a localized string for UI display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a user-friendly formatted representation of the creation date
    /// converted to local time, optimized for matter management interface and professional presentation.
    /// 
    /// <para><strong>Format:</strong></para>
    /// Uses "dddd, dd MMMM yyyy HH:mm:ss" format for complete temporal information suitable for
    /// professional legal practice documentation and client communication.
    /// </remarks>
    /// <example>
    /// <code>
    /// var matter = new MatterWithoutDocumentsDto 
    /// { 
    ///     CreationDate = new DateTime(2024, 1, 15, 14, 30, 45, DateTimeKind.Utc) 
    /// };
    /// 
    /// // Display in UI
    /// label.Text = $"Created: {matter.LocalCreationDateString}";
    /// // Output: "Created: Monday, 15 January 2024 16:30:45" (if local time is UTC+2)
    /// </code>
    /// </example>
    public string LocalCreationDateString => CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the normalized description for consistent comparison and search operations.
    /// </summary>
    /// <remarks>
    /// This computed property provides a normalized version of the matter description using
    /// ADMS.API.Common.MatterValidationHelper normalization rules for consistent comparison,
    /// uniqueness validation, and search operations.
    /// 
    /// <para><strong>Normalization Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Trims leading and trailing whitespace</item>
    /// <item>Normalizes multiple consecutive spaces to single spaces</item>
    /// <item>Preserves internal punctuation and formatting</item>
    /// <item>Returns null for invalid descriptions</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var matter1 = new MatterWithoutDocumentsDto { Description = "  Contract   Review  " };
    /// var matter2 = new MatterWithoutDocumentsDto { Description = "Contract Review" };
    /// 
    /// // Both will have the same normalized description: "Contract Review"
    /// bool areEquivalent = matter1.NormalizedDescription == matter2.NormalizedDescription; // true
    /// </code>
    /// </example>
    public string? NormalizedDescription => MatterValidationHelper.NormalizeDescription(Description);

    /// <summary>
    /// Gets the current status of the matter as a descriptive string.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.Matter.Status"/> and provides
    /// a human-readable status description based on the matter's current state flags,
    /// useful for user interface display and professional reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Matter status: {matter.Status}");
    /// // Possible outputs: "Active", "Archived", "Deleted", "Archived and Deleted"
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
    /// the number of days since the matter was created, useful for practice management,
    /// billing analysis, and matter lifecycle tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Matter is {matter.AgeDays} days old");
    /// 
    /// // Finding old matters for review
    /// var oldMatters = matters.Where(m => m.AgeDays > 365);
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
    ///     // Show matter in active matters list
    ///     Console.WriteLine($"Active matter: {matter.Description}");
    /// }
    /// 
    /// // Filter for active matters
    /// var activeMatters = matters.Where(m => m.IsActive).ToList();
    /// </code>
    /// </example>
    public bool IsActive => !IsArchived && !IsDeleted;

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
    ///     // Display activity history in user interface
    ///     DisplayActivityHistory(matter);
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
    /// Console.WriteLine($"Total activities: {matter.TotalActivityCount}");
    /// 
    /// // Finding highly active matters
    /// var activeMatters = matters
    ///     .Where(m => m.TotalActivityCount > 10)
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
    /// 
    /// <para><strong>Validation Checks:</strong></para>
    /// <list type="bullet">
    /// <item>Matter ID is valid (if provided)</item>
    /// <item>Description passes comprehensive validation</item>
    /// <item>Creation date is within valid temporal bounds</item>
    /// <item>Archive state is consistent with business rules</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.IsValid)
    /// {
    ///     // Proceed with business operations
    ///     ProcessMatter(matter);
    /// }
    /// else
    /// {
    ///     // Show validation errors to user
    ///     DisplayValidationErrors(matter);
    /// }
    /// </code>
    /// </example>
    public bool IsValid =>
        (Id == Guid.Empty || MatterValidationHelper.IsValidMatterId(Id)) &&
        MatterValidationHelper.IsValidDescription(Description) &&
        MatterValidationHelper.IsValidDate(CreationDate) &&
        MatterValidationHelper.IsValidArchiveState(IsArchived, IsDeleted);

    /// <summary>
    /// Gets the display text suitable for UI controls and matter identification.
    /// </summary>
    /// <remarks>
    /// Provides a consistent format for displaying matter information in UI elements,
    /// using the description for clear professional identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// var matter = new MatterWithoutDocumentsDto { Description = "Smith Family Trust" };
    /// var displayText = matter.DisplayText; // Returns "Smith Family Trust"
    /// 
    /// // UI usage
    /// matterDropdown.Items.Add(new ListItem(matter.DisplayText, matter.Id.ToString()));
    /// </code>
    /// </example>
    public string DisplayText => Description;

    /// <summary>
    /// Gets comprehensive matter metrics for reporting and analysis.
    /// </summary>
    /// <remarks>
    /// This property provides a structured object containing key metrics and information
    /// for comprehensive matter analysis and professional reporting purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var metrics = matter.MatterMetrics;
    /// // Access comprehensive metrics for analysis and reporting
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
            LocalCreationDateString
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
        }
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="MatterWithoutDocumentsDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using the ADMS.API.Common.MatterValidationHelper for consistency 
    /// with entity validation rules. This ensures the DTO maintains the same validation standards as 
    /// the corresponding ADMS.API.Entities.Matter entity.
    /// 
    /// <para><strong>Validation Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>ID Validation:</strong> Ensures matter ID is valid when provided</item>
    /// <item><strong>Description Validation:</strong> Comprehensive description validation including format, length, and uniqueness</item>
    /// <item><strong>Date Validation:</strong> Creation date temporal consistency and bounds checking</item>
    /// <item><strong>State Validation:</strong> Archive and delete state consistency validation</item>
    /// <item><strong>Collection Validation:</strong> Deep validation of all activity collections</item>
    /// </list>
    /// 
    /// <para><strong>Integration with Validation Helpers:</strong></para>
    /// Uses centralized validation helpers (MatterValidationHelper, DtoValidationHelper) to ensure
    /// consistency across all matter-related validation in the system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterWithoutDocumentsDto 
    /// { 
    ///     Id = Guid.Empty,
    ///     Description = "",
    ///     CreationDate = DateTime.MinValue
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
        // Validate matter ID if provided
        foreach (var result in ValidateMatterId())
            yield return result;

        // Validate matter description
        foreach (var result in ValidateDescription())
            yield return result;

        // Validate creation date
        foreach (var result in ValidateCreationDate())
            yield return result;

        // Validate state consistency
        foreach (var result in ValidateStateConsistency())
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
    /// Uses ADMS.API.Common.MatterValidationHelper.ValidateMatterId for consistent validation when ID is provided.
    /// Empty IDs are allowed for new matter creation scenarios.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateMatterId()
    {
        return Id != Guid.Empty ? MatterValidationHelper.ValidateMatterId(Id, nameof(Id)) : [];
    }

    /// <summary>
    /// Validates the <see cref="Description"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the matter description.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.MatterValidationHelper.ValidateDescription for comprehensive validation
    /// including format, length, reserved words, and professional naming standards.
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
    /// delete states follow business rules (deleted matters must be archived).
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateStateConsistency()
    {
        return MatterValidationHelper.ValidateStates(IsArchived, IsDeleted, nameof(IsArchived), nameof(IsDeleted));
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="MatterWithoutDocumentsDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The MatterWithoutDocumentsDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate MatterWithoutDocumentsDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterWithoutDocumentsDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     Description = "Smith Family Trust",
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// 
    /// var results = MatterWithoutDocumentsDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Matter validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterWithoutDocumentsDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterWithoutDocumentsDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a MatterWithoutDocumentsDto from an ADMS.API.Entities.Matter entity with validation.
    /// </summary>
    /// <param name="matter">The Matter entity to convert. Cannot be null.</param>
    /// <param name="includeActivities">Whether to include activity collections in the conversion.</param>
    /// <returns>A valid MatterWithoutDocumentsDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when matter is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create MatterWithoutDocumentsDto instances from
    /// ADMS.API.Entities.Matter entities with automatic validation. It ensures the resulting
    /// DTO is valid before returning it, excluding document collections for performance.
    /// 
    /// <para><strong>Activity Collection Handling:</strong></para>
    /// When includeActivities is true, the method will attempt to map activity collections.
    /// For performance reasons, activity collections should typically be loaded separately
    /// using projection or explicit loading strategies.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity without activities for better performance
    /// var entity = new ADMS.API.Entities.Matter 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     Description = "Smith Family Trust",
    ///     CreationDate = DateTime.UtcNow,
    ///     IsArchived = false,
    ///     IsDeleted = false
    /// };
    /// 
    /// var dto = MatterWithoutDocumentsDto.FromEntity(entity, includeActivities: false);
    /// // Returns validated MatterWithoutDocumentsDto instance
    /// </code>
    /// </example>
    public static MatterWithoutDocumentsDto FromEntity([NotNull] Entities.Matter matter, bool includeActivities = false)
    {
        ArgumentNullException.ThrowIfNull(matter, nameof(matter));

        var dto = new MatterWithoutDocumentsDto
        {
            Id = matter.Id,
            Description = matter.Description,
            IsArchived = matter.IsArchived,
            IsDeleted = matter.IsDeleted,
            CreationDate = matter.CreationDate
        };

        // Optionally include activity collections
        if (includeActivities)
        {
            // Note: In practice, these would typically be mapped using a mapping framework
            // like AutoMapper or Mapster for better performance and maintainability
            // This is a placeholder for actual mapping logic
        }

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterWithoutDocumentsDto from entity: {errorMessages}");

    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this matter can be archived based on business rules.
    /// </summary>
    /// <returns>true if the matter can be archived; otherwise, false.</returns>
    /// <remarks>
    /// This method mirrors the business logic from <see cref="ADMS.API.Entities.Matter.CanBeArchived"/> and
    /// checks business rules to determine if archival is allowed, considering the current state and any associated constraints.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.CanBeArchived())
    /// {
    ///     matter.IsArchived = true;
    ///     // Log archival activity...
    /// }
    /// </code>
    /// </example>
    public bool CanBeArchived() => !IsArchived && !IsDeleted;

    /// <summary>
    /// Determines whether this matter can be restored from deleted state.
    /// </summary>
    /// <returns>true if the matter can be restored; otherwise, false.</returns>
    /// <remarks>
    /// This method mirrors the business logic from <see cref="ADMS.API.Entities.Matter.CanBeRestored"/> and
    /// determines if a deleted matter can be restored based on business rules and system constraints.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.CanBeRestored())
    /// {
    ///     matter.IsDeleted = false;
    ///     // Log restoration activity...
    /// }
    /// </code>
    /// </example>
    public bool CanBeRestored() => IsDeleted;

    /// <summary>
    /// Gets activities of a specific type performed on this matter.
    /// </summary>
    /// <param name="activityType">The activity type to filter by.</param>
    /// <returns>A collection of matching activities.</returns>
    /// <remarks>
    /// This method enables filtering activities by type for specific analysis and reporting purposes,
    /// mirroring the functionality from ADMS.API.Entities.Matter.
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
    /// This method provides access to the most recent activity for user interface display
    /// and business logic purposes, mirroring ADMS.API.Entities.Matter functionality.
    /// </remarks>
    /// <example>
    /// <code>
    /// var lastActivity = matter.GetMostRecentActivity();
    /// if (lastActivity != null)
    /// {
    ///     Console.WriteLine($"Last activity: {lastActivity.MatterActivity?.Activity} " +
    ///                      $"by {lastActivity.User?.Name} at {lastActivity.CreatedAt}");
    /// }
    /// </code>
    /// </example>
    public MatterActivityUserDto? GetMostRecentActivity()
    {
        return MatterActivityUsers
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();
    }

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterWithoutDocumentsDto is equal to the current MatterWithoutDocumentsDto.
    /// </summary>
    /// <param name="other">The MatterWithoutDocumentsDto to compare with the current MatterWithoutDocumentsDto.</param>
    /// <returns>true if the specified MatterWithoutDocumentsDto is equal to the current MatterWithoutDocumentsDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property when both have values, or by comparing
    /// description and key properties when IDs are not available. This follows the same equality 
    /// pattern as ADMS.API.Entities.Matter for consistency.
    /// </remarks>
    public bool Equals(MatterWithoutDocumentsDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // If both have IDs, compare by ID
        if (Id != Guid.Empty && other.Id != Guid.Empty)
        {
            return Id.Equals(other.Id);
        }

        // If neither has ID or one is missing, compare by content
        return string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase) &&
               IsArchived == other.IsArchived &&
               IsDeleted == other.IsDeleted &&
               CreationDate == other.CreationDate;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current MatterWithoutDocumentsDto.
    /// </summary>
    /// <param name="obj">The object to compare with the current MatterWithoutDocumentsDto.</param>
    /// <returns>true if the specified object is equal to the current MatterWithoutDocumentsDto; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as MatterWithoutDocumentsDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterWithoutDocumentsDto.</returns>
    /// <remarks>
    /// The hash code is based on the Id property when available, or on content properties
    /// when ID is not available, ensuring consistent hashing behavior.
    /// </remarks>
    public override int GetHashCode()
    {
        return Id != Guid.Empty ? Id.GetHashCode() : HashCode.Combine(Description, IsArchived, IsDeleted, CreationDate);
    }

    /// <summary>
    /// Determines whether two MatterWithoutDocumentsDto instances are equal.
    /// </summary>
    /// <param name="left">The first MatterWithoutDocumentsDto to compare.</param>
    /// <param name="right">The second MatterWithoutDocumentsDto to compare.</param>
    /// <returns>true if the MatterWithoutDocumentsDtos are equal; otherwise, false.</returns>
    public static bool operator ==(MatterWithoutDocumentsDto? left, MatterWithoutDocumentsDto? right) =>
        EqualityComparer<MatterWithoutDocumentsDto>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two MatterWithoutDocumentsDto instances are not equal.
    /// </summary>
    /// <param name="left">The first MatterWithoutDocumentsDto to compare.</param>
    /// <param name="right">The second MatterWithoutDocumentsDto to compare.</param>
    /// <returns>true if the MatterWithoutDocumentsDtos are not equal; otherwise, false.</returns>
    public static bool operator !=(MatterWithoutDocumentsDto? left, MatterWithoutDocumentsDto? right) => !(left == right);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterWithoutDocumentsDto.
    /// </summary>
    /// <returns>A string that represents the current MatterWithoutDocumentsDto.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the matter,
    /// following the same pattern as ADMS.API.Entities.Matter.ToString() for consistency.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterWithoutDocumentsDto 
    /// { 
    ///     Id = Guid.Parse("60000000-0000-0000-0000-000000000001"),
    ///     Description = "Smith Family Trust",
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "Matter: Smith Family Trust (60000000-0000-0000-0000-000000000001) - Active"
    /// </code>
    /// </example>
    public override string ToString() =>
        Id != Guid.Empty
            ? $"Matter: {Description} ({Id}) - {Status}"
            : $"Matter: {Description} - {Status}";

    #endregion String Representation
}