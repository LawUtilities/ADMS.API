using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using ADMS.API.Common;

namespace ADMS.API.Models;

/// <summary>
/// Minimal Document Revision Data Transfer Object providing essential revision information for UI display and system operations.
/// </summary>
/// <remarks>
/// This immutable record represents a lightweight revision DTO optimized for UI scenarios where only essential 
/// revision information is needed. It provides the minimal set of properties required to identify, display, and 
/// manage document revisions throughout the ADMS legal document management system while maintaining data integrity 
/// through comprehensive validation.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Entity Representation:</strong> Contains only essential properties from ADMS.API.Entities.Revision</item>
/// <item><strong>Version Control Support:</strong> Provides revision numbering and temporal tracking for document versions</item>
/// <item><strong>UI Optimized:</strong> Designed for revision display, selection, and basic management operations</item>
/// <item><strong>Comprehensive Validation:</strong> Uses ADMS.API.Common.RevisionValidationHelper for data integrity</item>
/// <item><strong>Immutable Design:</strong> Record type with init-only properties for thread-safe operations</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO mirrors essential properties from ADMS.API.Entities.Revision:
/// <list type="bullet">
/// <item><strong>Id:</strong> Unique identifier for the revision (optional for new revisions)</item>
/// <item><strong>RevisionNumber:</strong> Sequential version number within document scope</item>
/// <item><strong>CreationDate/ModificationDate:</strong> Temporal tracking with UTC storage</item>
/// <item><strong>IsDeleted:</strong> Soft deletion state for audit trail preservation</item>
/// <item><strong>DocumentId:</strong> Association with parent document (when needed)</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Revision Lists:</strong> Document version history displays and revision selection controls</item>
/// <item><strong>Version Control UI:</strong> Revision comparison, rollback, and management interfaces</item>
/// <item><strong>Audit Trail Display:</strong> Revision information in activity logs and audit records</item>
/// <item><strong>API Responses:</strong> Lightweight revision data in document and activity DTOs</item>
/// <item><strong>Search Results:</strong> Revision search and filtering operations</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Version Control:</strong> Sequential revision numbering for legal document management</item>
/// <item><strong>Temporal Tracking:</strong> Creation and modification dates for legal compliance</item>
/// <item><strong>Audit Trail Compatibility:</strong> Soft deletion and change tracking for legal requirements</item>
/// <item><strong>System Integration:</strong> Compatible with ADMS entity relationships and workflows</item>
/// </list>
/// 
/// <para><strong>Data Integrity:</strong></para>
/// <list type="bullet">
/// <item><strong>Revision Numbering:</strong> Sequential validation with business rule enforcement</item>
/// <item><strong>Date Validation:</strong> UTC storage with temporal consistency checking</item>
/// <item><strong>GUID Validation:</strong> Proper identifier validation for database operations</item>
/// <item><strong>Business Rules:</strong> Comprehensive validation using RevisionValidationHelper</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a minimal revision DTO
/// var revisionMinimal = new RevisionMinimalDto
/// {
///     Id = Guid.NewGuid(),
///     RevisionNumber = 1,
///     CreationDate = DateTime.UtcNow,
///     ModificationDate = DateTime.UtcNow,
///     IsDeleted = false
/// };
/// 
/// // Using in version control scenarios
/// var revisionHistory = revisions.Select(r => new RevisionMinimalDto 
/// { 
///     Id = r.Id,
///     RevisionNumber = r.RevisionNumber,
///     CreationDate = r.CreationDate,
///     ModificationDate = r.ModificationDate,
///     IsDeleted = r.IsDeleted
/// }).OrderByDescending(r => r.RevisionNumber).ToList();
/// 
/// // Validation example
/// var validationResults = RevisionMinimalDto.ValidateModel(revisionMinimal);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Validation Error: {result.ErrorMessage}");
///     }
/// }
/// </code>
/// </example>
public record RevisionMinimalDto : IValidatableObject, IEquatable<RevisionMinimalDto>
{
    #region Core Properties

    /// <summary>
    /// Gets the unique identifier for the revision.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the revision within the ADMS system.
    /// The ID corresponds directly to the <see cref="ADMS.API.Entities.Revision.Id"/> property and is
    /// used for database operations, API calls, and system references.
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Optional for New Revisions:</strong> Can be null when creating new revisions</item>
    /// <item><strong>Required for Updates:</strong> Must be provided when updating existing revisions</item>
    /// <item><strong>Database Operations:</strong> Primary key for revision-related database queries</item>
    /// <item><strong>API Operations:</strong> Revision identification in REST API operations</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Revision.Id"/> with identical behavior,
    /// ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // For existing revisions
    /// var existingRevision = new RevisionMinimalDto 
    /// { 
    ///     Id = Guid.Parse("12345678-1234-5678-9012-123456789012"),
    ///     RevisionNumber = 3 
    /// };
    /// 
    /// // For new revisions (ID will be generated by database)
    /// var newRevision = new RevisionMinimalDto 
    /// { 
    ///     Id = null,
    ///     RevisionNumber = 1 
    /// };
    /// </code>
    /// </example>
    public Guid? Id { get; init; }

    /// <summary>
    /// Gets the revision number for the document.
    /// </summary>
    /// <remarks>
    /// The revision number represents the sequential version of the document, starting from 1 for
    /// the first revision and incrementing by 1 for each subsequent revision. This numbering
    /// system mirrors <see cref="ADMS.API.Entities.Revision.RevisionNumber"/> and ensures clear 
    /// version identification with chronological order.
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
    /// </remarks>
    /// <example>
    /// <code>
    /// // Sequential revision examples
    /// var revision1 = new RevisionMinimalDto { RevisionNumber = 1 }; // First revision
    /// var revision2 = new RevisionMinimalDto { RevisionNumber = 2 }; // Second revision
    /// var revision3 = new RevisionMinimalDto { RevisionNumber = 3 }; // Third revision
    /// 
    /// // Version comparison
    /// if (revision3.RevisionNumber > revision1.RevisionNumber)
    /// {
    ///     Console.WriteLine($"Revision {revision3.RevisionNumber} is newer than {revision1.RevisionNumber}");
    /// }
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Revision number is required.")]
    [Range(1, 999999,
        ErrorMessage = "Revision number must be between 1 and 999999.")]
    public required int RevisionNumber { get; init; }

    /// <summary>
    /// Gets the creation date for the revision (in UTC).
    /// </summary>
    /// <remarks>
    /// The creation date represents when the revision was initially created in the system,
    /// mirroring <see cref="ADMS.API.Entities.Revision.CreationDate"/>. All dates are stored 
    /// in UTC to ensure consistency across different time zones and support accurate temporal 
    /// tracking for legal compliance.
    /// 
    /// <para><strong>Date Requirements (via ADMS.API.Common.RevisionValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>UTC Storage:</strong> Must be stored in UTC format for consistency</item>
    /// <item><strong>Valid Range:</strong> Between January 1, 1980 and current time (with tolerance)</item>
    /// <item><strong>Temporal Consistency:</strong> Must precede or equal the ModificationDate</item>
    /// <item><strong>Legal Compliance:</strong> Supports legal document chronology requirements</item>
    /// </list>
    /// 
    /// <para><strong>UI Display Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Version History:</strong> "Created on January 15, 2024"</item>
    /// <item><strong>Audit Trails:</strong> Chronological ordering of revision activities</item>
    /// <item><strong>Timeline Views:</strong> Document development timeline visualization</item>
    /// <item><strong>Comparison Views:</strong> Temporal context for revision comparisons</item>
    /// </list>
    /// 
    /// <para><strong>Legal Significance:</strong></para>
    /// The creation date establishes when document versions were created for legal timelines,
    /// chronological order for audit trails, and evidence for document development history.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Creating revision with UTC timestamps
    /// var revision = new RevisionMinimalDto
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
    public required DateTime CreationDate { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the modification date for the revision (in UTC).
    /// </summary>
    /// <remarks>
    /// The modification date represents when the revision was last modified or updated,
    /// mirroring <see cref="ADMS.API.Entities.Revision.ModificationDate"/>. This date must be 
    /// greater than or equal to the creation date and follows the same UTC storage requirements.
    /// 
    /// <para><strong>Date Requirements (via ADMS.API.Common.RevisionValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>UTC Storage:</strong> Must be stored in UTC format for consistency</item>
    /// <item><strong>Chronological Order:</strong> Must be >= CreationDate</item>
    /// <item><strong>Valid Range:</strong> Between January 1, 1980 and current time (with tolerance)</item>
    /// <item><strong>Update Tracking:</strong> Updated whenever revision content or metadata changes</item>
    /// </list>
    /// 
    /// <para><strong>Business Logic:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Change Tracking:</strong> Tracks when changes were made to revision content</item>
    /// <item><strong>Temporal Analysis:</strong> Establishes chronological order for audit purposes</item>
    /// <item><strong>Version Comparison:</strong> Supports version comparison and change analysis</item>
    /// <item><strong>Audit Compliance:</strong> Provides timestamps for legal document modification tracking</item>
    /// </list>
    /// 
    /// <para><strong>Update Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Initially set to same value as CreationDate for new revisions</item>
    /// <item>Updated whenever revision content or metadata changes</item>
    /// <item>Preserved during soft deletion for audit trail integrity</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Initial revision with same creation and modification dates
    /// var newRevision = new RevisionMinimalDto
    /// {
    ///     RevisionNumber = 1,
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// 
    /// // Later revision update (would be done through business logic)
    /// var updatedRevision = newRevision with 
    /// { 
    ///     ModificationDate = DateTime.UtcNow 
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Modification date is required.")]
    public required DateTime ModificationDate { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a value indicating whether the revision is deleted.
    /// </summary>
    /// <remarks>
    /// This property implements soft deletion for the revision, mirroring <see cref="ADMS.API.Entities.Revision.IsDeleted"/>.
    /// Soft deletion allows the revision to be marked as deleted while preserving all data for audit trail 
    /// and legal compliance purposes, which is critical in legal document management.
    /// 
    /// <para><strong>Soft Deletion Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Audit Trail Preservation:</strong> Maintains complete audit trail for legal compliance</item>
    /// <item><strong>Referential Integrity:</strong> Preserves relationships with audit and activity records</item>
    /// <item><strong>Recovery Operations:</strong> Enables restoration if deletion was accidental</item>
    /// <item><strong>Historical Reporting:</strong> Supports historical reporting and analysis</item>
    /// </list>
    /// 
    /// <para><strong>UI Display Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Revision Lists:</strong> Filtering active vs. deleted revisions</item>
    /// <item><strong>Version History:</strong> Indicating deleted revisions with special formatting</item>
    /// <item><strong>Administrative Views:</strong> Managing soft-deleted revisions</item>
    /// <item><strong>Audit Trails:</strong> Showing deletion status in activity logs</item>
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
    /// var activeRevision = new RevisionMinimalDto 
    /// { 
    ///     RevisionNumber = 1, 
    ///     IsDeleted = false 
    /// };
    /// 
    /// // Filtering active revisions
    /// var activeRevisions = revisions.Where(r => !r.IsDeleted).ToList();
    /// 
    /// // Soft delete a revision (would be done through business logic)
    /// var deletedRevision = activeRevision with { IsDeleted = true };
    /// </code>
    /// </example>
    public bool IsDeleted { get; init; }

    /// <summary>
    /// Gets the document ID associated with this revision.
    /// </summary>
    /// <remarks>
    /// This optional property establishes the relationship between the revision and its parent document,
    /// mirroring <see cref="ADMS.API.Entities.Revision.DocumentId"/>. While not always needed in minimal
    /// scenarios, it provides context when revisions are displayed outside of their document context.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Cross-Document Views:</strong> When displaying revisions from multiple documents</item>
    /// <item><strong>Search Results:</strong> Revision search results showing parent document association</item>
    /// <item><strong>API Operations:</strong> When document context is needed for revision operations</item>
    /// <item><strong>Navigation:</strong> Enabling navigation from revision back to parent document</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// When provided, this property must reference a valid existing document ID and follows
    /// the same validation rules as the entity property.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Revision with document association for cross-document scenarios
    /// var revisionWithDocument = new RevisionMinimalDto
    /// {
    ///     RevisionNumber = 2,
    ///     DocumentId = Guid.Parse("87654321-4321-8765-2109-876543210987"),
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// 
    /// // Revision without document association for document-scoped scenarios
    /// var revisionWithoutDocument = new RevisionMinimalDto
    /// {
    ///     RevisionNumber = 1,
    ///     DocumentId = null,
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    public Guid? DocumentId { get; init; }

    #endregion Core Properties

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
    /// var revision = new RevisionMinimalDto 
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
    /// var revision = new RevisionMinimalDto 
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
        (DocumentId == null || RevisionValidationHelper.IsValidDocumentId(DocumentId.Value));

    /// <summary>
    /// Gets the display text suitable for UI controls and revision identification.
    /// </summary>
    /// <remarks>
    /// Provides a consistent format for displaying revision information in UI elements,
    /// combining revision number with creation date for clear identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// var revision = new RevisionMinimalDto { RevisionNumber = 3, CreationDate = DateTime.UtcNow };
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
    /// Validates the <see cref="RevisionMinimalDto"/> for data integrity and business rules compliance.
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
    /// <item><strong>Document Association:</strong> Valid document ID when provided</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new RevisionMinimalDto 
    /// { 
    ///     RevisionNumber = 0, 
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
        // Validate revision ID if provided
        foreach (var result in ValidateRevisionId())
            yield return result;

        // Validate revision number
        foreach (var result in ValidateRevisionNumber())
            yield return result;

        // Validate individual dates
        foreach (var result in ValidateCreationDate())
            yield return result;

        foreach (var result in ValidateModificationDate())
            yield return result;

        // Validate date sequence and business rules
        foreach (var result in ValidateDateSequence())
            yield return result;

        // Validate document association if provided
        foreach (var result in ValidateDocumentId())
            yield return result;
    }

    /// <summary>
    /// Validates the <see cref="Id"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the revision ID.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.RevisionValidationHelper.ValidateRevisionId for consistent validation
    /// across all revision-related DTOs and entities.
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
    /// Validates the <see cref="DocumentId"/> property if provided.
    /// </summary>
    /// <returns>A collection of validation results for the document ID.</returns>
    /// <remarks>
    /// When DocumentId is provided, validates it using RevisionValidationHelper to ensure
    /// it represents a valid document identifier.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateDocumentId()
    {
        if (!DocumentId.HasValue) yield break;
        if (DocumentId.Value != Guid.Empty) yield break;
        yield return new ValidationResult(
            "DocumentId must be a valid non-empty GUID when provided.",
            [nameof(DocumentId)]);
    }

    #endregion Validation Implementation

    #region Static Validation Methods

    /// <summary>
    /// Validates a <see cref="RevisionMinimalDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The RevisionMinimalDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate RevisionMinimalDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new RevisionMinimalDto 
    /// { 
    ///     RevisionNumber = 1, 
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// 
    /// var results = RevisionMinimalDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Revision validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] RevisionMinimalDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("RevisionMinimalDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a RevisionMinimalDto from an ADMS.API.Entities.Revision entity with validation.
    /// </summary>
    /// <param name="revision">The Revision entity to convert. Cannot be null.</param>
    /// <param name="includeDocumentId">Whether to include the document ID in the conversion.</param>
    /// <returns>A valid RevisionMinimalDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when revision is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create RevisionMinimalDto instances from
    /// ADMS.API.Entities.Revision entities with automatic validation. It ensures the resulting
    /// DTO is valid before returning it.
    /// 
    /// <para><strong>Document ID Inclusion:</strong></para>
    /// The includeDocumentId parameter controls whether the document association is included,
    /// useful for scenarios where document context is or isn't needed.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity without document association
    /// var revision = new ADMS.API.Entities.Revision 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     RevisionNumber = 1,
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// 
    /// var dto = RevisionMinimalDto.FromEntity(revision, includeDocumentId: false);
    /// // Returns validated RevisionMinimalDto instance
    /// </code>
    /// </example>
    public static RevisionMinimalDto FromEntity([NotNull] Entities.Revision revision, bool includeDocumentId = false)
    {
        ArgumentNullException.ThrowIfNull(revision, nameof(revision));

        var dto = new RevisionMinimalDto
        {
            Id = revision.Id,
            RevisionNumber = revision.RevisionNumber,
            CreationDate = revision.CreationDate,
            ModificationDate = revision.ModificationDate,
            IsDeleted = revision.IsDeleted,
            DocumentId = includeDocumentId ? revision.DocumentId : null
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid RevisionMinimalDto from entity: {errorMessages}");

    }

    #endregion Static Validation Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified RevisionMinimalDto is equal to the current RevisionMinimalDto.
    /// </summary>
    /// <param name="other">The RevisionMinimalDto to compare with the current RevisionMinimalDto.</param>
    /// <returns>true if the specified RevisionMinimalDto is equal to the current RevisionMinimalDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property when both have values, or by comparing
    /// revision number and dates when IDs are not available. This follows the same equality pattern 
    /// as ADMS.API.Entities.Revision for consistency.
    /// </remarks>
    public virtual bool Equals(RevisionMinimalDto? other)
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
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current RevisionMinimalDto.</returns>
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

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the RevisionMinimalDto.
    /// </summary>
    /// <returns>A string that represents the current RevisionMinimalDto.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the revision,
    /// following the same pattern as ADMS.API.Entities.Revision.ToString() for consistency.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new RevisionMinimalDto 
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