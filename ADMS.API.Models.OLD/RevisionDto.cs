using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using ADMS.API.Common;

namespace ADMS.API.Models;

/// <summary>
/// Comprehensive Document Revision Data Transfer Object representing a complete revision with all associated audit trail data.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of a document revision within the ADMS legal document management system,
/// including all audit trail associations and activity relationships. It mirrors the structure of 
/// <see cref="ADMS.API.Entities.Revision"/> while providing comprehensive validation and computed properties
/// for client-side operations and audit trail display.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Entity Representation:</strong> Mirrors all properties and relationships from ADMS.API.Entities.Revision</item>
/// <item><strong>Audit Trail Integration:</strong> Includes comprehensive audit trail associations for legal compliance</item>
/// <item><strong>Professional Validation:</strong> Uses ADMS.API.Common.RevisionValidationHelper for data integrity</item>
/// <item><strong>Computed Properties:</strong> Client-optimized properties for UI display and business logic</item>
/// <item><strong>Collection Validation:</strong> Deep validation of audit trail collections using DtoValidationHelper</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO maintains the same relationship structure as ADMS.API.Entities.Revision:
/// <list type="bullet">
/// <item><strong>RevisionActivityUsers:</strong> Complete audit trail of user activities on this revision</item>
/// <item><strong>Document Association:</strong> Optional document context when needed for cross-document scenarios</item>
/// <item><strong>Version Control:</strong> Sequential revision numbering and temporal tracking</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>API Responses:</strong> Complete revision data including all audit trail relationships</item>
/// <item><strong>Revision Management:</strong> Comprehensive revision administration and lifecycle operations</item>
/// <item><strong>Audit Trail Display:</strong> Full revision activity history and user attribution</item>
/// <item><strong>Reporting:</strong> Revision-based reporting and analytics with complete context</item>
/// <item><strong>Version Control:</strong> Complete version control operations and history display</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Version Control Integrity:</strong> Complete revision history with sequential numbering</item>
/// <item><strong>Audit Compliance:</strong> Comprehensive audit trail relationships for legal compliance</item>
/// <item><strong>Temporal Tracking:</strong> Precise temporal data for legal document chronology</item>
/// <item><strong>User Attribution:</strong> Complete user accountability for all revision operations</item>
/// </list>
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// <list type="bullet">
/// <item><strong>Lazy Loading Support:</strong> Audit collections can be populated on-demand</item>
/// <item><strong>Selective Loading:</strong> Individual audit collections can be loaded independently</item>
/// <item><strong>Computed Properties:</strong> Cached computed values for frequently accessed calculations</item>
/// <item><strong>Validation Optimization:</strong> Efficient validation using centralized helpers</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive revision DTO
/// var revisionDto = new RevisionDto
/// {
///     Id = Guid.NewGuid(),
///     RevisionNumber = 2,
///     DocumentId = documentId,
///     CreationDate = DateTime.UtcNow,
///     ModificationDate = DateTime.UtcNow,
///     IsDeleted = false
/// };
/// 
/// // Validating the complete revision DTO
/// var validationResults = RevisionDto.ValidateModel(revisionDto);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Using computed properties
/// var activityCount = revisionDto.ActivityCount;
/// var hasAuditTrail = revisionDto.HasActivities;
/// var displayText = revisionDto.DisplayText;
/// </code>
/// </example>
public class RevisionDto : IValidatableObject, IEquatable<RevisionDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the revision.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the revision within the ADMS system.
    /// The ID corresponds directly to the <see cref="ADMS.API.Entities.Revision.Id"/> property and is
    /// used for database operations, API calls, and system references.
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Optional for Creation:</strong> Can be null when creating new revisions</item>
    /// <item><strong>Required for Updates:</strong> Must be provided when updating existing revisions</item>
    /// <item><strong>Database Operations:</strong> Primary key for revision-related database queries</item>
    /// <item><strong>API Operations:</strong> Revision identification in REST API operations</item>
    /// <item><strong>Audit Trail References:</strong> Used in audit trail and activity records</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Revision.Id"/> with identical behavior,
    /// ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // For existing revisions
    /// var existingRevision = new RevisionDto 
    /// { 
    ///     Id = Guid.Parse("12345678-1234-5678-9012-123456789012"),
    ///     RevisionNumber = 3 
    /// };
    /// 
    /// // For new revisions (ID will be generated by database)
    /// var newRevision = new RevisionDto 
    /// { 
    ///     Id = null,
    ///     RevisionNumber = 1 
    /// };
    /// </code>
    /// </example>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the revision number for the document.
    /// </summary>
    /// <remarks>
    /// The revision number represents the sequential version of the document, corresponding to
    /// <see cref="ADMS.API.Entities.Revision.RevisionNumber"/>. This numbering system ensures 
    /// clear version identification with chronological order and proper sequencing.
    /// 
    /// <para><strong>Versioning Rules (via ADMS.API.Common.RevisionValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Range:</strong> 1 to 999,999 (following legal document versioning standards)</item>
    /// <item><strong>Sequential:</strong> Must be sequential within document scope</item>
    /// <item><strong>No Gaps:</strong> Version control maintains continuous numbering</item>
    /// <item><strong>Unique:</strong> Revision numbers are unique within each document</item>
    /// </list>
    /// 
    /// <para><strong>UI Display Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Version History:</strong> "Revision 3" in document version lists</item>
    /// <item><strong>Comparison Views:</strong> "Compare Revision 2 to Revision 3"</item>
    /// <item><strong>Rollback Operations:</strong> "Rollback to Revision 2"</item>
    /// <item><strong>Audit Trails:</strong> "User created Revision 1 at [timestamp]"</item>
    /// </list>
    /// 
    /// <para><strong>Business Logic Integration:</strong></para>
    /// Used for sorting, ordering, and version comparison operations throughout the system.
    /// Essential for maintaining proper document version control integrity.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Sequential revision examples
    /// var revision1 = new RevisionDto { RevisionNumber = 1 }; // First revision
    /// var revision2 = new RevisionDto { RevisionNumber = 2 }; // Second revision
    /// var revision3 = new RevisionDto { RevisionNumber = 3 }; // Third revision
    /// 
    /// // Version comparison and sorting
    /// var sortedRevisions = revisions.OrderByDescending(r => r.RevisionNumber);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Revision number is required.")]
    [Range(1, 999999,
        ErrorMessage = "Revision number must be between 1 and 999999.")]
    public required int RevisionNumber { get; set; }

    /// <summary>
    /// Gets or sets the creation date for the revision (in UTC).
    /// </summary>
    /// <remarks>
    /// The creation date represents when the revision was initially created in the system,
    /// corresponding to <see cref="ADMS.API.Entities.Revision.CreationDate"/>. All dates are 
    /// stored in UTC to ensure consistency across different time zones and support accurate 
    /// temporal tracking for legal compliance.
    /// 
    /// <para><strong>Date Requirements (via ADMS.API.Common.RevisionValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>UTC Storage:</strong> Must be stored in UTC format for consistency</item>
    /// <item><strong>Valid Range:</strong> Between January 1, 1980 and current time (with tolerance)</item>
    /// <item><strong>Temporal Consistency:</strong> Must precede or equal the ModificationDate</item>
    /// <item><strong>Legal Compliance:</strong> Supports legal document chronology requirements</item>
    /// </list>
    /// 
    /// <para><strong>Legal Significance:</strong></para>
    /// The creation date establishes when document versions were created for legal timelines,
    /// chronological order for audit trails, and evidence for document development history.
    /// Critical for legal compliance and audit requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Creating revision with UTC timestamps
    /// var revision = new RevisionDto
    /// {
    ///     RevisionNumber = 1,
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// 
    /// // Display formatting for UI
    /// var displayText = $"Created: {revision.LocalCreationDateString}";
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Creation date is required.")]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the modification date for the revision (in UTC).
    /// </summary>
    /// <remarks>
    /// The modification date represents when the revision was last modified or updated,
    /// corresponding to <see cref="ADMS.API.Entities.Revision.ModificationDate"/>. This date 
    /// must be greater than or equal to the creation date and follows UTC storage requirements.
    /// 
    /// <para><strong>Date Requirements (via ADMS.API.Common.RevisionValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>UTC Storage:</strong> Must be stored in UTC format for consistency</item>
    /// <item><strong>Chronological Order:</strong> Must be >= CreationDate</item>
    /// <item><strong>Valid Range:</strong> Between January 1, 1980 and current time (with tolerance)</item>
    /// <item><strong>Update Tracking:</strong> Updated whenever revision content or metadata changes</item>
    /// </list>
    /// 
    /// <para><strong>Business Logic Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Change Tracking:</strong> Tracks when changes were made to revision content</item>
    /// <item><strong>Temporal Analysis:</strong> Establishes chronological order for audit purposes</item>
    /// <item><strong>Version Comparison:</strong> Supports version comparison and change analysis</item>
    /// <item><strong>Audit Compliance:</strong> Provides timestamps for legal document modification tracking</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Revision with modification tracking
    /// var revision = new RevisionDto
    /// {
    ///     RevisionNumber = 2,
    ///     CreationDate = DateTime.UtcNow.AddHours(-1),
    ///     ModificationDate = DateTime.UtcNow // Updated later
    /// };
    /// 
    /// // Time span analysis
    /// var developmentTime = revision.ModificationTimeSpan;
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Modification date is required.")]
    public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the document ID associated with this revision.
    /// </summary>
    /// <remarks>
    /// This optional property establishes the relationship between the revision and its parent document,
    /// corresponding to <see cref="ADMS.API.Entities.Revision.DocumentId"/>. While not always needed 
    /// in all scenarios, it provides essential context when revisions are displayed outside of their 
    /// document context or when document association is required for operations.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Cross-Document Views:</strong> When displaying revisions from multiple documents</item>
    /// <item><strong>Search Results:</strong> Revision search results showing parent document association</item>
    /// <item><strong>API Operations:</strong> When document context is needed for revision operations</item>
    /// <item><strong>Navigation:</strong> Enabling navigation from revision back to parent document</item>
    /// <item><strong>Audit Reporting:</strong> Document-centric audit reports including revision data</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// When provided, this property must reference a valid existing document ID and follows
    /// the same validation rules as the corresponding entity property.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Revision with document association for cross-document scenarios
    /// var revisionWithDocument = new RevisionDto
    /// {
    ///     Id = Guid.NewGuid(),
    ///     RevisionNumber = 2,
    ///     DocumentId = Guid.Parse("87654321-4321-8765-2109-876543210987"),
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// 
    /// // Navigation usage
    /// if (revision.DocumentId.HasValue)
    /// {
    ///     var document = await documentService.GetByIdAsync(revision.DocumentId.Value);
    /// }
    /// </code>
    /// </example>
    public Guid? DocumentId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the revision is deleted.
    /// </summary>
    /// <remarks>
    /// This property implements soft deletion for the revision, corresponding to
    /// <see cref="ADMS.API.Entities.Revision.IsDeleted"/>. Soft deletion allows the revision 
    /// to be marked as deleted while preserving all data for audit trail and legal compliance 
    /// purposes, which is critical in legal document management.
    /// 
    /// <para><strong>Soft Deletion Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Audit Trail Preservation:</strong> Maintains complete audit trail for legal compliance</item>
    /// <item><strong>Referential Integrity:</strong> Preserves relationships with audit and activity records</item>
    /// <item><strong>Recovery Operations:</strong> Enables restoration if deletion was accidental</item>
    /// <item><strong>Historical Reporting:</strong> Supports historical reporting and analysis</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Deleted revisions are typically filtered from normal operations</item>
    /// <item>Deletion status is tracked through revision activity audit entries</item>
    /// <item>Restoration operations can reverse soft deletion</item>
    /// <item>Administrative users may view and manage deleted revisions</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Active revision
    /// var activeRevision = new RevisionDto 
    /// { 
    ///     RevisionNumber = 1, 
    ///     IsDeleted = false 
    /// };
    /// 
    /// // Filtering active revisions
    /// var activeRevisions = revisions.Where(r => !r.IsDeleted).ToList();
    /// 
    /// // Deleted revision for administrative view
    /// var deletedRevision = new RevisionDto 
    /// { 
    ///     RevisionNumber = 2, 
    ///     IsDeleted = true 
    /// };
    /// </code>
    /// </example>
    public bool IsDeleted { get; set; }

    #endregion Core Properties

    #region Audit Trail Collections

    /// <summary>
    /// Gets or sets the collection of revision activity user associations for this revision.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.Revision.RevisionActivityUsers"/> and maintains
    /// the many-to-many relationship between revisions, activities, and users, providing a comprehensive 
    /// audit trail of all actions performed on this revision.
    /// 
    /// <para><strong>Activity Types Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> User created this revision</item>
    /// <item><strong>SAVED:</strong> User saved changes to this revision</item>
    /// <item><strong>DELETED:</strong> User deleted this revision</item>
    /// <item><strong>RESTORED:</strong> User restored this deleted revision</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Significance:</strong></para>
    /// Each association provides:
    /// <list type="bullet">
    /// <item><strong>User Attribution:</strong> Complete user accountability for revision operations</item>
    /// <item><strong>Timestamp Tracking:</strong> Precise temporal tracking for audit chronology</item>
    /// <item><strong>Activity Classification:</strong> Categorization of operations for audit analysis</item>
    /// <item><strong>Legal Compliance:</strong> Complete audit trail for legal and regulatory requirements</item>
    /// </list>
    /// 
    /// <para><strong>DTO Composition:</strong></para>
    /// Contains <see cref="RevisionActivityUserDto"/> instances that include complete revision, activity,
    /// and user information for comprehensive audit trail presentation and analysis.
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// This collection can be populated on-demand or selectively loaded based on specific audit 
    /// trail display requirements to optimize performance for large revision histories.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing revision audit trail
    /// foreach (var activity in revision.RevisionActivityUsers)
    /// {
    ///     Console.WriteLine($"User {activity.User?.Name} performed {activity.RevisionActivity?.Activity} " +
    ///                      $"on revision {revision.RevisionNumber} at {activity.CreatedAt}");
    /// }
    /// 
    /// // Finding who created this revision
    /// var creator = revision.RevisionActivityUsers
    ///     .FirstOrDefault(ra => ra.RevisionActivity?.Activity == "CREATED")?.User;
    /// 
    /// // Getting revision activity timeline
    /// var timeline = revision.RevisionActivityUsers
    ///     .OrderBy(ra => ra.CreatedAt)
    ///     .Select(ra => new { 
    ///         Timestamp = ra.CreatedAt, 
    ///         Activity = ra.RevisionActivity?.Activity, 
    ///         User = ra.User?.Name 
    ///     });
    /// </code>
    /// </example>
    public ICollection<RevisionActivityUserDto> RevisionActivityUsers { get; set; } = new List<RevisionActivityUserDto>();

    #endregion Audit Trail Collections

    #region Computed Properties

    /// <summary>
    /// Gets the creation date formatted as a localized string for UI display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a user-friendly formatted representation of the creation date
    /// converted to local time, suitable for display in user interfaces and reports.
    /// 
    /// <para><strong>Format:</strong></para>
    /// Uses "dddd, dd MMMM yyyy HH:mm:ss" format (e.g., "Monday, 15 January 2024 14:30:45")
    /// </remarks>
    /// <example>
    /// <code>
    /// var revision = new RevisionDto 
    /// { 
    ///     CreationDate = new DateTime(2024, 1, 15, 14, 30, 45, DateTimeKind.Utc) 
    /// };
    /// 
    /// // Display in UI
    /// label.Text = $"Created: {revision.LocalCreationDateString}";
    /// // Output: "Created: Monday, 15 January 2024 16:30:45" (if local time is UTC+2)
    /// </code>
    /// </example>
    public string LocalCreationDateString => CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the modification date formatted as a localized string for UI display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a user-friendly formatted representation of the modification date
    /// converted to local time, suitable for display in user interfaces and reports.
    /// 
    /// <para><strong>Format:</strong></para>
    /// Uses "dddd, dd MMMM yyyy HH:mm:ss" format (e.g., "Monday, 15 January 2024 14:30:45")
    /// </remarks>
    /// <example>
    /// <code>
    /// var revision = new RevisionDto 
    /// { 
    ///     ModificationDate = new DateTime(2024, 1, 15, 16, 45, 30, DateTimeKind.Utc) 
    /// };
    /// 
    /// // Display in UI
    /// label.Text = $"Last Modified: {revision.LocalModificationDateString}";
    /// // Output: "Last Modified: Monday, 15 January 2024 18:45:30" (if local time is UTC+2)
    /// </code>
    /// </example>
    public string LocalModificationDateString => ModificationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the time span between creation and modification dates.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.Revision.ModificationTimeSpan"/> and
    /// calculates the duration between when the revision was created and when it was last modified,
    /// providing insight into revision development time and modification patterns.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (revision.ModificationTimeSpan.TotalMinutes > 30)
    /// {
    ///     Console.WriteLine($"Revision {revision.RevisionNumber} was modified " +
    ///                      $"{revision.ModificationTimeSpan.TotalMinutes:F0} minutes after creation");
    /// }
    /// </code>
    /// </example>
    public TimeSpan ModificationTimeSpan => ModificationDate - CreationDate;

    /// <summary>
    /// Gets a value indicating whether this revision has any recorded activities.
    /// </summary>
    /// <remarks>
    /// This property mirrors <see cref="ADMS.API.Entities.Revision.HasActivities"/> and is useful 
    /// for determining audit trail completeness and ensuring that revisions have proper activity tracking.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Audit Trail Validation:</strong> Ensuring complete audit trail coverage</item>
    /// <item><strong>Data Integrity Verification:</strong> Identifying revisions missing activity records</item>
    /// <item><strong>Reporting:</strong> Activity coverage analysis for compliance reporting</item>
    /// <item><strong>Administrative Views:</strong> Highlighting revisions requiring audit attention</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!revision.HasActivities)
    /// {
    ///     // Log potential audit trail gap
    ///     logger.LogWarning($"Revision {revision.Id} has no activity records");
    /// }
    /// 
    /// // Filter revisions with complete audit trails
    /// var auditedRevisions = revisions.Where(r => r.HasActivities).ToList();
    /// </code>
    /// </example>
    public bool HasActivities => RevisionActivityUsers.Count > 0;

    /// <summary>
    /// Gets the total count of activities recorded for this revision.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.Revision.ActivityCount"/> and provides
    /// insight into the level of activity associated with this revision, useful for activity monitoring,
    /// audit analysis, and understanding revision lifecycle complexity.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Revision {revision.RevisionNumber} has {revision.ActivityCount} recorded activities");
    /// 
    /// // Finding most active revisions
    /// var mostActiveRevisions = revisions
    ///     .OrderByDescending(r => r.ActivityCount)
    ///     .Take(10);
    /// </code>
    /// </example>
    public int ActivityCount => RevisionActivityUsers.Count;

    /// <summary>
    /// Gets a value indicating whether this revision DTO has valid data for system operations.
    /// </summary>
    /// <remarks>
    /// This property provides a quick validation check without running full validation logic,
    /// useful for UI scenarios where immediate feedback is needed.
    /// 
    /// <para><strong>Validation Checks:</strong></para>
    /// <list type="bullet">
    /// <item>Revision number is within valid bounds</item>
    /// <item>Both creation and modification dates are valid</item>
    /// <item>Date sequence is chronologically correct</item>
    /// <item>Document ID is valid (if provided)</item>
    /// <item>Time span between dates is reasonable</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (revision.IsValid)
    /// {
    ///     // Proceed with business operations
    ///     ProcessRevision(revision);
    /// }
    /// else
    /// {
    ///     // Show validation errors to user
    ///     DisplayValidationErrors(revision);
    /// }
    /// </code>
    /// </example>
    public bool IsValid =>
        RevisionValidationHelper.IsValidRevisionNumber(RevisionNumber) &&
        RevisionValidationHelper.IsValidDate(CreationDate) &&
        RevisionValidationHelper.IsValidDate(ModificationDate) &&
        RevisionValidationHelper.IsValidDateSequence(CreationDate, ModificationDate) &&
        (DocumentId == null || RevisionValidationHelper.IsValidDocumentId(DocumentId.Value)) &&
        RevisionValidationHelper.IsValidDateTimeSpan(CreationDate, ModificationDate);

    /// <summary>
    /// Gets the display text suitable for UI controls and revision identification.
    /// </summary>
    /// <remarks>
    /// Provides a consistent format for displaying revision information in UI elements,
    /// combining revision number with creation date for clear identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// var revision = new RevisionDto { RevisionNumber = 3, CreationDate = DateTime.UtcNow };
    /// var displayText = revision.DisplayText; // Returns "Revision 3 (Created: [date])"
    /// 
    /// // UI usage
    /// revisionDropdown.Items.Add(new ListItem(revision.DisplayText, revision.Id.ToString()));
    /// </code>
    /// </example>
    public string DisplayText => $"Revision {RevisionNumber} (Created: {LocalCreationDateString})";

    /// <summary>
    /// Gets a shorter display text for compact UI scenarios.
    /// </summary>
    /// <remarks>
    /// Provides a compact representation for scenarios where space is limited,
    /// such as dropdown lists or table cells.
    /// </remarks>
    /// <example>
    /// <code>
    /// var compactText = revision.CompactDisplayText; // Returns "Rev. 3"
    /// tableCell.Text = compactText;
    /// </code>
    /// </example>
    public string CompactDisplayText => $"Rev. {RevisionNumber}";

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="RevisionDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using the ADMS.API.Common.RevisionValidationHelper for consistency 
    /// with entity validation rules. This ensures the DTO maintains the same validation standards as 
    /// the corresponding ADMS.API.Entities.Revision entity.
    /// 
    /// <para><strong>Validation Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>ID Validation:</strong> Ensures valid revision ID when provided</item>
    /// <item><strong>Revision Number Validation:</strong> Sequential numbering and bounds checking</item>
    /// <item><strong>Date Validation:</strong> Individual date validation and temporal consistency</item>
    /// <item><strong>Business Rules:</strong> Chronological order and business logic compliance</item>
    /// <item><strong>Collection Validation:</strong> Deep validation of audit trail collections</item>
    /// </list>
    /// 
    /// <para><strong>Integration with Validation Helpers:</strong></para>
    /// Uses centralized validation helpers (RevisionValidationHelper, DtoValidationHelper) to ensure
    /// consistency across all revision-related validation in the system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new RevisionDto 
    /// { 
    ///     RevisionNumber = 0, // Invalid
    ///     CreationDate = DateTime.MinValue // Invalid
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
        // Validate revision ID if provided
        foreach (var result in ValidateRevisionId())
            yield return result;

        // Validate revision number
        foreach (var result in ValidateRevisionNumber())
            yield return result;

        // Validate document association if provided
        foreach (var result in ValidateDocumentId())
            yield return result;

        // Validate individual dates
        foreach (var result in ValidateCreationDate())
            yield return result;

        foreach (var result in ValidateModificationDate())
            yield return result;

        // Validate date sequence and business rules
        foreach (var result in ValidateDateSequence())
            yield return result;

        // Validate time span reasonableness
        foreach (var result in ValidateTimeSpan())
            yield return result;

        // Validate audit trail collections using centralized helper
        foreach (var result in DtoValidationHelper.ValidateCollection(RevisionActivityUsers, nameof(RevisionActivityUsers)))
            yield return result;
    }

    /// <summary>
    /// Validates the <see cref="Id"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the revision ID.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.RevisionValidationHelper for consistent validation when ID is provided.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateRevisionId()
    {
        return Id.HasValue ? RevisionValidationHelper.ValidateRevisionId(Id.Value, nameof(Id)) : [];
    }

    /// <summary>
    /// Validates the <see cref="RevisionNumber"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the revision number.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.RevisionValidationHelper.ValidateRevisionNumber for consistent validation
    /// across all revision-related DTOs and entities.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateRevisionNumber()
    {
        return RevisionValidationHelper.ValidateRevisionNumber(RevisionNumber, nameof(RevisionNumber));
    }

    /// <summary>
    /// Validates the <see cref="DocumentId"/> property if provided.
    /// </summary>
    /// <returns>A collection of validation results for the document ID.</returns>
    /// <remarks>
    /// When DocumentId is provided, validates it to ensure it represents a valid document identifier.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateDocumentId()
    {
        if (!DocumentId.HasValue) yield break;
        if (DocumentId.Value != Guid.Empty) yield break;
        yield return new ValidationResult(
            "DocumentId must be a valid non-empty GUID when provided.",
            [nameof(DocumentId)]);
    }

    /// <summary>
    /// Validates the <see cref="CreationDate"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the creation date.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.RevisionValidationHelper.ValidateDate for consistent validation
    /// across all revision-related DTOs and entities.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateCreationDate()
    {
        return RevisionValidationHelper.ValidateDate(CreationDate, nameof(CreationDate));
    }

    /// <summary>
    /// Validates the <see cref="ModificationDate"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the modification date.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.RevisionValidationHelper.ValidateDate for consistent validation
    /// across all revision-related DTOs and entities.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateModificationDate()
    {
        return RevisionValidationHelper.ValidateDate(ModificationDate, nameof(ModificationDate));
    }

    /// <summary>
    /// Validates the chronological sequence of creation and modification dates.
    /// </summary>
    /// <returns>A collection of validation results for date sequence validation.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.RevisionValidationHelper.ValidateDateSequence to ensure
    /// modification date is not before creation date.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateDateSequence()
    {
        return RevisionValidationHelper.ValidateDateSequence(
            CreationDate, ModificationDate, nameof(CreationDate), nameof(ModificationDate));
    }

    /// <summary>
    /// Validates that the time span between creation and modification dates is reasonable.
    /// </summary>
    /// <returns>A collection of validation results for time span validation.</returns>
    /// <remarks>
    /// Ensures the time span between creation and modification dates is within reasonable
    /// bounds to detect potential data corruption or incorrect timestamps.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateTimeSpan()
    {
        if (RevisionValidationHelper.IsValidDateTimeSpan(CreationDate, ModificationDate)) yield break;
        var span = ModificationDate - CreationDate;
        if (span < TimeSpan.Zero)
        {
            yield return new ValidationResult(
                "Modification date cannot be earlier than creation date.",
                [nameof(ModificationDate), nameof(CreationDate)]);
        }
        else if (span > RevisionValidationHelper.MaxDateTimeSpan)
        {
            yield return new ValidationResult(
                $"Time span between creation and modification dates is too large (maximum allowed: {RevisionValidationHelper.MaxDateTimeSpan.TotalDays:F0} days).",
                [nameof(CreationDate), nameof(ModificationDate)]);
        }
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="RevisionDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The RevisionDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate RevisionDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new RevisionDto 
    /// { 
    ///     RevisionNumber = 1, 
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// 
    /// var results = RevisionDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Revision validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] RevisionDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("RevisionDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a RevisionDto from an ADMS.API.Entities.Revision entity with validation.
    /// </summary>
    /// <param name="revision">The Revision entity to convert. Cannot be null.</param>
    /// <param name="includeDocumentId">Whether to include the document ID in the conversion.</param>
    /// <param name="includeActivities">Whether to include activity collections in the conversion.</param>
    /// <returns>A valid RevisionDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when revision is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create RevisionDto instances from
    /// ADMS.API.Entities.Revision entities with automatic validation. It ensures the resulting
    /// DTO is valid before returning it.
    /// 
    /// <para><strong>Activity Collection Handling:</strong></para>
    /// When includeActivities is true, the method will attempt to map activity collections.
    /// For performance reasons, activity collections should typically be loaded separately
    /// using projection or explicit loading strategies.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity with document association but without activities for performance
    /// var revision = new ADMS.API.Entities.Revision 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     RevisionNumber = 1,
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow,
    ///     DocumentId = documentId,
    ///     IsDeleted = false
    /// };
    /// 
    /// var dto = RevisionDto.FromEntity(revision, includeDocumentId: true, includeActivities: false);
    /// // Returns validated RevisionDto instance
    /// </code>
    /// </example>
    public static RevisionDto FromEntity([NotNull] Entities.Revision revision, bool includeDocumentId = false, bool includeActivities = false)
    {
        ArgumentNullException.ThrowIfNull(revision, nameof(revision));

        var dto = new RevisionDto
        {
            Id = revision.Id,
            RevisionNumber = revision.RevisionNumber,
            CreationDate = revision.CreationDate,
            ModificationDate = revision.ModificationDate,
            IsDeleted = revision.IsDeleted,
            DocumentId = includeDocumentId ? revision.DocumentId : null
        };

        // Optionally include activity collections
        // Note: In practice, these would typically be mapped using a mapping framework
        // like AutoMapper or Mapster for better performance and maintainability
        if (includeActivities)
        {
            // Activity collections would be mapped here if needed
            // This is a placeholder for actual mapping logic
        }

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid RevisionDto from entity: {errorMessages}");

    }

    #endregion Static Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified RevisionDto is equal to the current RevisionDto.
    /// </summary>
    /// <param name="other">The RevisionDto to compare with the current RevisionDto.</param>
    /// <returns>true if the specified RevisionDto is equal to the current RevisionDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property when both have values, or by comparing
    /// revision number and key properties when IDs are not available. This follows the same equality 
    /// pattern as ADMS.API.Entities.Revision for consistency.
    /// </remarks>
    public bool Equals(RevisionDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // If both have IDs, compare by ID
        if (Id.HasValue && other.Id.HasValue)
        {
            return Id.Value.Equals(other.Id.Value) && Id.Value != Guid.Empty;
        }

        // If neither has ID or one is missing, compare by content
        return RevisionNumber == other.RevisionNumber &&
               CreationDate == other.CreationDate &&
               ModificationDate == other.ModificationDate &&
               IsDeleted == other.IsDeleted &&
               DocumentId == other.DocumentId;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current RevisionDto.
    /// </summary>
    /// <param name="obj">The object to compare with the current RevisionDto.</param>
    /// <returns>true if the specified object is equal to the current RevisionDto; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as RevisionDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current RevisionDto.</returns>
    /// <remarks>
    /// The hash code is based on the Id property when available, or on content properties
    /// when ID is not available, ensuring consistent hashing behavior.
    /// </remarks>
    public override int GetHashCode()
    {
        if (Id.HasValue && Id.Value != Guid.Empty)
        {
            return Id.Value.GetHashCode();
        }

        return HashCode.Combine(RevisionNumber, CreationDate, ModificationDate, IsDeleted, DocumentId);
    }

    /// <summary>
    /// Determines whether two RevisionDto instances are equal.
    /// </summary>
    /// <param name="left">The first RevisionDto to compare.</param>
    /// <param name="right">The second RevisionDto to compare.</param>
    /// <returns>true if the RevisionDtos are equal; otherwise, false.</returns>
    public static bool operator ==(RevisionDto? left, RevisionDto? right) =>
        EqualityComparer<RevisionDto>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two RevisionDto instances are not equal.
    /// </summary>
    /// <param name="left">The first RevisionDto to compare.</param>
    /// <param name="right">The second RevisionDto to compare.</param>
    /// <returns>true if the RevisionDtos are not equal; otherwise, false.</returns>
    public static bool operator !=(RevisionDto? left, RevisionDto? right) => !(left == right);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the RevisionDto.
    /// </summary>
    /// <returns>A string that represents the current RevisionDto.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the revision,
    /// following the same pattern as ADMS.API.Entities.Revision.ToString() for consistency.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new RevisionDto 
    /// { 
    ///     Id = Guid.Parse("12345678-1234-5678-9012-123456789012"),
    ///     RevisionNumber = 3,
    ///     CreationDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "Revision 3 (12345678-1234-5678-9012-123456789012) - Created: 2024-01-15 10:30:00 UTC"
    /// </code>
    /// </example>
    public override string ToString() =>
        Id.HasValue
            ? $"Revision {RevisionNumber} ({Id}) - Created: {CreationDate:yyyy-MM-dd HH:mm:ss} UTC"
            : $"Revision {RevisionNumber} - Created: {CreationDate:yyyy-MM-dd HH:mm:ss} UTC";

    #endregion String Representation
}