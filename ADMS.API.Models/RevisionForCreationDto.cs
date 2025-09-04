using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using ADMS.API.Common;

namespace ADMS.API.Models;

/// <summary>
/// Data Transfer Object for creating new document revisions in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// This DTO provides a structured approach to creating new revisions while maintaining data integrity
/// and business rule compliance. It mirrors the creatable properties of <see cref="ADMS.API.Entities.Revision"/>
/// and enforces the same validation rules to ensure consistency between API operations and entity creation.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Creation-Focused Design:</strong> Contains only properties needed for creating new revisions</item>
/// <item><strong>Entity Synchronization:</strong> Mirrors creatable properties from ADMS.API.Entities.Revision</item>
/// <item><strong>Comprehensive Validation:</strong> Uses ADMS.API.Common.RevisionValidationHelper for data integrity</item>
/// <item><strong>Business Rule Enforcement:</strong> Ensures revision creation complies with legal document management standards</item>
/// <item><strong>Version Control Support:</strong> Proper sequential revision numbering and temporal consistency</item>
/// </list>
/// 
/// <para><strong>Creation Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>New Document Versions:</strong> Creating subsequent revisions for existing documents</item>
/// <item><strong>Document Creation:</strong> Creating initial revision (number 1) for new documents</item>
/// <item><strong>Import Operations:</strong> Creating revisions during document import processes</item>
/// <item><strong>Migration Operations:</strong> Creating revisions during system migrations</item>
/// <item><strong>Administrative Creation:</strong> Creating revisions for administrative purposes</item>
/// </list>
/// 
/// <para><strong>Entity Alignment:</strong></para>
/// This DTO maps to creatable properties of ADMS.API.Entities.Revision:
/// <list type="bullet">
/// <item><strong>RevisionNumber:</strong> Sequential version number (typically calculated automatically)</item>
/// <item><strong>CreationDate:</strong> Timestamp when revision is created (typically current UTC time)</item>
/// <item><strong>ModificationDate:</strong> Initial modification timestamp (typically same as creation)</item>
/// <item><strong>DocumentId:</strong> Required association with parent document</item>
/// <item><strong>IsDeleted:</strong> Initial deletion state (typically false for new revisions)</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Version Control Integrity:</strong> Maintains proper revision sequencing and numbering</item>
/// <item><strong>Temporal Consistency:</strong> Ensures chronological order of revision creation</item>
/// <item><strong>Audit Compliance:</strong> Supports legal document audit trail requirements</item>
/// <item><strong>Data Integrity:</strong> Prevents creation of revisions that would corrupt document version history</item>
/// </list>
/// 
/// <para><strong>Validation Strategy:</strong></para>
/// <list type="bullet">
/// <item><strong>Individual Property Validation:</strong> Each property validated using RevisionValidationHelper</item>
/// <item><strong>Cross-Property Validation:</strong> Ensures relationships between properties are valid</item>
/// <item><strong>Business Rule Validation:</strong> Enforces legal document management business rules</item>
/// <item><strong>Sequential Validation:</strong> Can validate revision number sequencing when context is available</item>
/// </list>
/// 
/// <para><strong>Usage Guidelines:</strong></para>
/// <list type="bullet">
/// <item>Revision numbers should typically be calculated automatically by business logic</item>
/// <item>Creation and modification dates are usually set to current UTC time</item>
/// <item>Document ID must reference a valid existing document</item>
/// <item>IsDeleted should typically be false for new revisions</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating the first revision for a new document
/// var firstRevisionDto = new RevisionForCreationDto
/// {
///     RevisionNumber = 1,
///     DocumentId = documentId,
///     CreationDate = DateTime.UtcNow,
///     ModificationDate = DateTime.UtcNow,
///     IsDeleted = false
/// };
/// 
/// // Creating a subsequent revision
/// var nextRevisionDto = new RevisionForCreationDto
/// {
///     RevisionNumber = previousRevision.RevisionNumber + 1,
///     DocumentId = documentId,
///     CreationDate = DateTime.UtcNow,
///     ModificationDate = DateTime.UtcNow,
///     IsDeleted = false
/// };
/// 
/// // Validating before creation
/// var validationResults = RevisionForCreationDto.ValidateModel(firstRevisionDto);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Validation Error: {result.ErrorMessage}");
///     }
/// }
/// else
/// {
///     // Create the revision
///     var newRevision = await revisionService.CreateRevisionAsync(firstRevisionDto);
/// }
/// </code>
/// </example>
public class RevisionForCreationDto : IValidatableObject, IEquatable<RevisionForCreationDto>
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="RevisionForCreationDto"/> class.
    /// </summary>
    /// <remarks>
    /// Default constructor for standard revision creation scenarios. Sets IsDeleted to false
    /// as most new revisions are created in active state.
    /// </remarks>
    public RevisionForCreationDto()
    {
        IsDeleted = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RevisionForCreationDto"/> class with specified deletion state.
    /// </summary>
    /// <param name="isDeleted">The initial deletion state for the revision.</param>
    /// <remarks>
    /// This constructor allows creating revisions with a specific deletion state, useful for
    /// migration scenarios or administrative operations where revisions may need to be created
    /// in deleted state.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Creating an active revision (typical scenario)
    /// var activeRevision = new RevisionForCreationDto(isDeleted: false);
    /// 
    /// // Creating a revision in deleted state (migration/admin scenario)
    /// var deletedRevision = new RevisionForCreationDto(isDeleted: true);
    /// </code>
    /// </example>
    public RevisionForCreationDto(bool isDeleted)
    {
        IsDeleted = isDeleted;
    }

    #endregion Constructors

    #region Core Properties

    /// <summary>
    /// Gets or sets the revision number for the document.
    /// </summary>
    /// <remarks>
    /// The revision number represents the sequential version of the document, corresponding to
    /// <see cref="ADMS.API.Entities.Revision.RevisionNumber"/>. For new revisions, this should
    /// typically be calculated automatically by business logic to ensure proper sequencing.
    /// 
    /// <para><strong>Creation Guidelines (via ADMS.API.Common.RevisionValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Range:</strong> 1 to 999,999 (following legal document versioning standards)</item>
    /// <item><strong>Sequential:</strong> Must be next in sequence for the document</item>
    /// <item><strong>First Revision:</strong> Should be 1 for new documents</item>
    /// <item><strong>Business Logic:</strong> Typically calculated by service layer, not manually set</item>
    /// </list>
    /// 
    /// <para><strong>Creation Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>New Document:</strong> First revision should be number 1</item>
    /// <item><strong>Existing Document:</strong> Next sequential number (max existing + 1)</item>
    /// <item><strong>Migration:</strong> May need specific numbers to preserve version history</item>
    /// <item><strong>Administrative:</strong> May require specific numbering for compliance</item>
    /// </list>
    /// 
    /// <para><strong>Best Practices:</strong></para>
    /// <list type="bullet">
    /// <item>Allow business logic to calculate revision numbers automatically when possible</item>
    /// <item>Validate sequential integrity when creating multiple revisions</item>
    /// <item>Consider document context when setting revision numbers</item>
    /// <item>Ensure proper authorization for manual revision number assignment</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Automatic revision number calculation (recommended)
    /// var nextRevisionNumber = await revisionService.GetNextRevisionNumberAsync(documentId);
    /// var revisionDto = new RevisionForCreationDto
    /// {
    ///     RevisionNumber = nextRevisionNumber,
    ///     DocumentId = documentId
    /// };
    /// 
    /// // Manual revision number for first revision
    /// var firstRevisionDto = new RevisionForCreationDto
    /// {
    ///     RevisionNumber = 1, // First revision
    ///     DocumentId = newDocumentId
    /// };
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
    /// The creation date represents when the revision is created in the system, corresponding to
    /// <see cref="ADMS.API.Entities.Revision.CreationDate"/>. For new revisions, this should
    /// typically be set to the current UTC time to ensure accurate temporal tracking.
    /// 
    /// <para><strong>Creation Guidelines (via ADMS.API.Common.RevisionValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>UTC Storage:</strong> Must be stored in UTC format for consistency</item>
    /// <item><strong>Valid Range:</strong> Between January 1, 1980 and current time (with tolerance)</item>
    /// <item><strong>Temporal Consistency:</strong> Must precede or equal the ModificationDate</item>
    /// <item><strong>Automatic Setting:</strong> Typically set automatically to DateTime.UtcNow</item>
    /// </list>
    /// 
    /// <para><strong>Creation Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Standard Creation:</strong> Set to current UTC time for accurate tracking</item>
    /// <item><strong>Migration:</strong> May need specific timestamps to preserve history</item>
    /// <item><strong>Import Operations:</strong> May use original document creation timestamps</item>
    /// <item><strong>Administrative:</strong> May require specific timestamps for compliance</item>
    /// </list>
    /// 
    /// <para><strong>Legal Significance:</strong></para>
    /// The creation date establishes when document versions were created for legal timelines,
    /// chronological order for audit trails, and evidence for document development history.
    /// Accuracy is critical for legal compliance and audit requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard creation with current UTC time (recommended)
    /// var revisionDto = new RevisionForCreationDto
    /// {
    ///     RevisionNumber = 1,
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow,
    ///     DocumentId = documentId
    /// };
    /// 
    /// // Migration with specific timestamp
    /// var migrationRevision = new RevisionForCreationDto
    /// {
    ///     RevisionNumber = 1,
    ///     CreationDate = originalTimestamp.ToUniversalTime(),
    ///     ModificationDate = originalTimestamp.ToUniversalTime(),
    ///     DocumentId = documentId
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Creation date is required.")]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the initial modification date for the revision (in UTC).
    /// </summary>
    /// <remarks>
    /// The modification date represents the initial modification timestamp for the revision,
    /// corresponding to <see cref="ADMS.API.Entities.Revision.ModificationDate"/>. For new
    /// revisions, this is typically set to the same value as CreationDate initially.
    /// 
    /// <para><strong>Initial Setting Guidelines:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Same as Creation:</strong> Initially set to same value as CreationDate</item>
    /// <item><strong>UTC Consistency:</strong> Always stored in UTC for cross-timezone consistency</item>
    /// <item><strong>Future Updates:</strong> Will be updated when revision content changes</item>
    /// <item><strong>Chronological Order:</strong> Must be >= CreationDate</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Valid Range:</strong> Between January 1, 1980 and current time (with tolerance)</item>
    /// <item><strong>Temporal Consistency:</strong> Must be >= CreationDate</item>
    /// <item><strong>Reasonable Span:</strong> Time span from creation must be within reasonable limits</item>
    /// <item><strong>UTC Format:</strong> Must be provided in UTC for consistency</item>
    /// </list>
    /// 
    /// <para><strong>Business Logic Integration:</strong></para>
    /// While initially set during creation, the modification date will be automatically updated
    /// by business logic whenever the revision content or metadata changes after creation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard creation with same initial timestamps
    /// var currentTime = DateTime.UtcNow;
    /// var revisionDto = new RevisionForCreationDto
    /// {
    ///     RevisionNumber = 1,
    ///     CreationDate = currentTime,
    ///     ModificationDate = currentTime, // Same as creation initially
    ///     DocumentId = documentId
    /// };
    /// 
    /// // Creation with slight delay (edge case)
    /// var creationTime = DateTime.UtcNow;
    /// var revisionDto2 = new RevisionForCreationDto
    /// {
    ///     RevisionNumber = 2,
    ///     CreationDate = creationTime,
    ///     ModificationDate = creationTime.AddSeconds(1), // Slightly later
    ///     DocumentId = documentId
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Modification date is required.")]
    public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the document ID associated with this revision.
    /// </summary>
    /// <remarks>
    /// This property establishes the required relationship between the revision and its parent document,
    /// corresponding to <see cref="ADMS.API.Entities.Revision.DocumentId"/>. Every revision must be
    /// associated with exactly one document, and this relationship is enforced at creation time.
    /// 
    /// <para><strong>Relationship Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required Association:</strong> Every revision must belong to a document</item>
    /// <item><strong>Valid Document:</strong> Must reference an existing, valid document</item>
    /// <item><strong>Non-Empty GUID:</strong> Cannot be Guid.Empty or null</item>
    /// <item><strong>Authorization:</strong> User must have permission to create revisions for the document</item>
    /// </list>
    /// 
    /// <para><strong>Creation Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>New Documents:</strong> Associate revision with newly created document</item>
    /// <item><strong>Existing Documents:</strong> Associate revision with existing document for versioning</item>
    /// <item><strong>Document Validation:</strong> Ensure document exists and is accessible</item>
    /// <item><strong>Business Rules:</strong> Verify document state allows new revision creation</item>
    /// </list>
    /// 
    /// <para><strong>Version Control Integration:</strong></para>
    /// The document association enables proper version control functionality including revision
    /// sequencing, document history, and proper audit trail maintenance within document scope.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Creating revision for existing document
    /// var existingDocumentId = Guid.Parse("87654321-4321-8765-2109-876543210987");
    /// var revisionDto = new RevisionForCreationDto
    /// {
    ///     RevisionNumber = 2,
    ///     DocumentId = existingDocumentId,
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// 
    /// // Creating first revision for new document
    /// var newDocumentId = await documentService.CreateDocumentAsync(documentDto);
    /// var firstRevision = new RevisionForCreationDto
    /// {
    ///     RevisionNumber = 1,
    ///     DocumentId = newDocumentId,
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Document ID is required.")]
    public required Guid DocumentId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the revision is created in deleted state.
    /// </summary>
    /// <remarks>
    /// This property sets the initial deletion state for the revision, corresponding to
    /// <see cref="ADMS.API.Entities.Revision.IsDeleted"/>. Most new revisions are created
    /// in active state (false), but certain scenarios may require creating revisions in
    /// deleted state for migration or administrative purposes.
    /// 
    /// <para><strong>Creation State Guidelines:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Standard Creation:</strong> Typically false for new active revisions</item>
    /// <item><strong>Migration Scenarios:</strong> May be true when importing deleted revisions</item>
    /// <item><strong>Administrative:</strong> May be true for special administrative scenarios</item>
    /// <item><strong>Audit Preservation:</strong> Maintains audit trail integrity during creation</item>
    /// </list>
    /// 
    /// <para><strong>Legal Implications:</strong></para>
    /// <list type="bullet">
    /// <item>Creating revisions in deleted state should be rare and well-documented</item>
    /// <item>Such operations typically require administrative authorization</item>
    /// <item>All creation operations should be logged in audit trail</item>
    /// <item>Consider business justification for creating deleted revisions</item>
    /// </list>
    /// 
    /// <para><strong>Common Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Active Creation (false):</strong> Standard revision creation for active use</item>
    /// <item><strong>Migration (true):</strong> Preserving deleted revisions during data migration</item>
    /// <item><strong>Import (varies):</strong> May match original deletion state during import</item>
    /// <item><strong>Administrative (varies):</strong> Special administrative requirements</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard active revision creation
    /// var activeRevision = new RevisionForCreationDto
    /// {
    ///     RevisionNumber = 1,
    ///     DocumentId = documentId,
    ///     IsDeleted = false // Standard active creation
    /// };
    /// 
    /// // Migration scenario with deleted revision
    /// var migratedRevision = new RevisionForCreationDto(isDeleted: true)
    /// {
    ///     RevisionNumber = 2,
    ///     DocumentId = documentId,
    ///     CreationDate = historicalTimestamp
    /// };
    /// </code>
    /// </example>
    public bool IsDeleted { get; set; }

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
    /// var creationDto = new RevisionForCreationDto 
    /// { 
    ///     CreationDate = new DateTime(2024, 1, 15, 14, 30, 45, DateTimeKind.Utc) 
    /// };
    /// 
    /// // Display in UI
    /// label.Text = $"Will be created: {creationDto.LocalCreationDateString}";
    /// // Output: "Will be created: Monday, 15 January 2024 16:30:45" (if local time is UTC+2)
    /// </code>
    /// </example>
    public string LocalCreationDateString => CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the modification date formatted as a localized string for UI display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a user-friendly formatted representation of the initial
    /// modification date converted to local time, suitable for display in user interfaces and reports.
    /// 
    /// <para><strong>Format:</strong></para>
    /// Uses "dddd, dd MMMM yyyy HH:mm:ss" format (e.g., "Monday, 15 January 2024 14:30:45")
    /// </remarks>
    /// <example>
    /// <code>
    /// var creationDto = new RevisionForCreationDto 
    /// { 
    ///     ModificationDate = new DateTime(2024, 1, 15, 14, 30, 45, DateTimeKind.Utc) 
    /// };
    /// 
    /// // Display in UI
    /// label.Text = $"Initial modification date: {creationDto.LocalModificationDateString}";
    /// // Output: "Initial modification date: Monday, 15 January 2024 16:30:45" (if local time is UTC+2)
    /// </code>
    /// </example>
    public string LocalModificationDateString => ModificationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the time span between creation and modification dates.
    /// </summary>
    /// <remarks>
    /// This computed property calculates the duration between the creation and initial modification
    /// dates. For new revisions, this is typically zero or a very small value since both dates
    /// are usually set to the same time initially.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (creationDto.ModificationTimeSpan.TotalSeconds > 0)
    /// {
    ///     Console.WriteLine($"Revision will have initial modification delay of {creationDto.ModificationTimeSpan.TotalSeconds} seconds");
    /// }
    /// </code>
    /// </example>
    public TimeSpan ModificationTimeSpan => ModificationDate - CreationDate;

    /// <summary>
    /// Gets a value indicating whether this creation DTO has valid data for system operations.
    /// </summary>
    /// <remarks>
    /// This property provides a quick validation check without running full validation logic,
    /// useful for UI scenarios where immediate feedback is needed before submitting creation requests.
    /// 
    /// <para><strong>Validation Checks:</strong></para>
    /// <list type="bullet">
    /// <item>Revision number is within valid bounds</item>
    /// <item>Both creation and modification dates are valid</item>
    /// <item>Date sequence is chronologically correct</item>
    /// <item>Document ID is valid and non-empty</item>
    /// <item>Time span between dates is reasonable</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (creationDto.IsValid)
    /// {
    ///     // Proceed with creation operation
    ///     await revisionService.CreateAsync(creationDto);
    /// }
    /// else
    /// {
    ///     // Show validation errors to user
    ///     DisplayValidationErrors(creationDto);
    /// }
    /// </code>
    /// </example>
    public bool IsValid =>
        RevisionValidationHelper.IsValidRevisionNumber(RevisionNumber) &&
        RevisionValidationHelper.IsValidDate(CreationDate) &&
        RevisionValidationHelper.IsValidDate(ModificationDate) &&
        RevisionValidationHelper.IsValidDateSequence(CreationDate, ModificationDate) &&
        RevisionValidationHelper.IsValidDocumentId(DocumentId) &&
        RevisionValidationHelper.IsValidDateTimeSpan(CreationDate, ModificationDate);

    /// <summary>
    /// Gets a value indicating whether this creation represents the first revision of a document.
    /// </summary>
    /// <remarks>
    /// This property helps identify when the creation operation is for the first revision of a document,
    /// useful for conditional logic and business rule application.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (creationDto.IsFirstRevision)
    /// {
    ///     // Apply first revision business rules
    ///     logger.LogInformation($"Creating first revision for document {creationDto.DocumentId}");
    /// }
    /// </code>
    /// </example>
    public bool IsFirstRevision => RevisionNumber == RevisionValidationHelper.MinRevisionNumber;

    /// <summary>
    /// Gets a value indicating whether this creation is for a deleted revision.
    /// </summary>
    /// <remarks>
    /// This property helps identify when the creation operation is specifically for a revision
    /// in deleted state, useful for audit logging and administrative operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (creationDto.IsDeletedCreation)
    /// {
    ///     // Log as special creation operation
    ///     logger.LogWarning($"Creating revision {creationDto.RevisionNumber} in deleted state");
    /// }
    /// </code>
    /// </example>
    public bool IsDeletedCreation => IsDeleted;

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="RevisionForCreationDto"/> for data integrity and business rules compliance.
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
    /// <item><strong>Revision Number Validation:</strong> Range checking and business rule compliance</item>
    /// <item><strong>Date Validation:</strong> Individual date validation and temporal consistency</item>
    /// <item><strong>Document Association:</strong> Valid document ID for proper relationship</item>
    /// <item><strong>Cross-Property Validation:</strong> Ensures relationships between properties are valid</item>
    /// <item><strong>Creation Constraints:</strong> Validates creation-specific business rules</item>
    /// </list>
    /// 
    /// <para><strong>Integration with RevisionValidationHelper:</strong></para>
    /// Uses the centralized validation helper to ensure consistency across all revision-related
    /// validation in the system, including entity validation, update DTOs, and creation DTOs.
    /// </remarks>
    /// <example>
    /// <code>
    /// var creationDto = new RevisionForCreationDto 
    /// { 
    ///     RevisionNumber = 0, // Invalid
    ///     DocumentId = Guid.Empty, // Invalid
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow.AddHours(-1) // Invalid: before creation
    /// };
    /// 
    /// var context = new ValidationContext(creationDto);
    /// var results = creationDto.Validate(context);
    /// 
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"Validation Error: {result.ErrorMessage}");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate revision number
        foreach (var result in ValidateRevisionNumber())
            yield return result;

        // Validate document association
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
    /// Validates the <see cref="DocumentId"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the document ID.</returns>
    /// <remarks>
    /// Validates that the document ID is a valid non-empty GUID suitable for establishing
    /// the required relationship with the parent document.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateDocumentId()
    {
        if (DocumentId == Guid.Empty)
        {
            yield return new ValidationResult(
                "DocumentId must be a valid non-empty GUID.",
                [nameof(DocumentId)]);
        }
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
    /// modification date is not before creation date and that the time span is reasonable.
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

    #region Static Factory Methods

    /// <summary>
    /// Validates a <see cref="RevisionForCreationDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The RevisionForCreationDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate RevisionForCreationDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Validate a creation DTO instance
    /// var creationDto = new RevisionForCreationDto 
    /// { 
    ///     RevisionNumber = 1,
    ///     DocumentId = documentId,
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// 
    /// var results = RevisionForCreationDto.ValidateModel(creationDto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Revision creation validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] RevisionForCreationDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("RevisionForCreationDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a RevisionForCreationDto for the first revision of a new document.
    /// </summary>
    /// <param name="documentId">The document ID for the new revision. Cannot be empty.</param>
    /// <param name="creationTime">Optional creation time. If not provided, uses current UTC time.</param>
    /// <returns>A valid RevisionForCreationDto configured for first revision creation.</returns>
    /// <exception cref="ArgumentException">Thrown when documentId is empty.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method creates a properly configured creation DTO for the first revision of a document,
    /// ensuring revision number is set to 1 and timestamps are properly configured.
    /// </remarks>
    /// <example>
    /// <code>
    /// var newDocumentId = await documentService.CreateDocumentAsync(documentDto);
    /// var firstRevisionDto = RevisionForCreationDto.ForFirstRevision(newDocumentId);
    /// 
    /// var revision = await revisionService.CreateAsync(firstRevisionDto);
    /// </code>
    /// </example>
    public static RevisionForCreationDto ForFirstRevision(Guid documentId, DateTime? creationTime = null)
    {
        if (documentId == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty.", nameof(documentId));

        var currentTime = creationTime?.ToUniversalTime() ?? DateTime.UtcNow;

        var dto = new RevisionForCreationDto
        {
            RevisionNumber = RevisionValidationHelper.MinRevisionNumber, // Always 1 for first revision
            DocumentId = documentId,
            CreationDate = currentTime,
            ModificationDate = currentTime,
            IsDeleted = false
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid first revision DTO: {errorMessages}");

    }

    /// <summary>
    /// Creates a RevisionForCreationDto for the next revision of an existing document.
    /// </summary>
    /// <param name="documentId">The document ID for the new revision. Cannot be empty.</param>
    /// <param name="currentMaxRevisionNumber">The current maximum revision number for the document.</param>
    /// <param name="creationTime">Optional creation time. If not provided, uses current UTC time.</param>
    /// <returns>A valid RevisionForCreationDto configured for next revision creation.</returns>
    /// <exception cref="ArgumentException">Thrown when documentId is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when currentMaxRevisionNumber is invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method creates a properly configured creation DTO for the next sequential revision,
    /// calculating the proper revision number and setting appropriate timestamps.
    /// </remarks>
    /// <example>
    /// <code>
    /// var maxRevisionNumber = await revisionService.GetMaxRevisionNumberAsync(documentId);
    /// var nextRevisionDto = RevisionForCreationDto.ForNextRevision(documentId, maxRevisionNumber);
    /// 
    /// var revision = await revisionService.CreateAsync(nextRevisionDto);
    /// </code>
    /// </example>
    public static RevisionForCreationDto ForNextRevision(Guid documentId, int currentMaxRevisionNumber, DateTime? creationTime = null)
    {
        if (documentId == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty.", nameof(documentId));

        if (!RevisionValidationHelper.IsValidRevisionNumber(currentMaxRevisionNumber))
            throw new ArgumentOutOfRangeException(nameof(currentMaxRevisionNumber),
                "Current maximum revision number must be valid.");

        var nextRevisionNumber = currentMaxRevisionNumber + 1;
        if (!RevisionValidationHelper.IsValidRevisionNumber(nextRevisionNumber))
            throw new ArgumentOutOfRangeException(nameof(currentMaxRevisionNumber),
                "Next revision number would exceed maximum allowed value.");

        var currentTime = creationTime?.ToUniversalTime() ?? DateTime.UtcNow;

        var dto = new RevisionForCreationDto
        {
            RevisionNumber = nextRevisionNumber,
            DocumentId = documentId,
            CreationDate = currentTime,
            ModificationDate = currentTime,
            IsDeleted = false
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid next revision DTO: {errorMessages}");

    }

    /// <summary>
    /// Creates a RevisionForCreationDto for migration scenarios with specific parameters.
    /// </summary>
    /// <param name="documentId">The document ID for the new revision. Cannot be empty.</param>
    /// <param name="revisionNumber">The specific revision number for migration.</param>
    /// <param name="creationDate">The original creation date for the revision.</param>
    /// <param name="modificationDate">The original modification date for the revision.</param>
    /// <param name="isDeleted">Whether the revision was deleted in the original system.</param>
    /// <returns>A valid RevisionForCreationDto configured for migration.</returns>
    /// <exception cref="ArgumentException">Thrown when documentId is empty.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method creates a creation DTO for data migration scenarios where specific
    /// revision numbers, timestamps, and deletion states need to be preserved from the source system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var migrationDto = RevisionForCreationDto.ForMigration(
    ///     documentId, 
    ///     revisionNumber: 3,
    ///     creationDate: originalCreationDate,
    ///     modificationDate: originalModificationDate,
    ///     isDeleted: false);
    /// 
    /// var revision = await revisionService.CreateAsync(migrationDto);
    /// </code>
    /// </example>
    public static RevisionForCreationDto ForMigration(
        Guid documentId,
        int revisionNumber,
        DateTime creationDate,
        DateTime modificationDate,
        bool isDeleted = false)
    {
        if (documentId == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty.", nameof(documentId));

        var dto = new RevisionForCreationDto(isDeleted)
        {
            RevisionNumber = revisionNumber,
            DocumentId = documentId,
            CreationDate = creationDate.Kind == DateTimeKind.Utc ? creationDate : creationDate.ToUniversalTime(),
            ModificationDate = modificationDate.Kind == DateTimeKind.Utc ? modificationDate : modificationDate.ToUniversalTime()
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid migration revision DTO: {errorMessages}");

    }

    #endregion Static Factory Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified RevisionForCreationDto is equal to the current RevisionForCreationDto.
    /// </summary>
    /// <param name="other">The RevisionForCreationDto to compare with the current RevisionForCreationDto.</param>
    /// <returns>true if the specified RevisionForCreationDto is equal to the current RevisionForCreationDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all properties, as creation DTOs represent specific
    /// sets of creation parameters and should be compared by their content.
    /// </remarks>
    public bool Equals(RevisionForCreationDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return RevisionNumber == other.RevisionNumber &&
               CreationDate == other.CreationDate &&
               ModificationDate == other.ModificationDate &&
               DocumentId.Equals(other.DocumentId) &&
               IsDeleted == other.IsDeleted;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current RevisionForCreationDto.
    /// </summary>
    /// <param name="obj">The object to compare with the current RevisionForCreationDto.</param>
    /// <returns>true if the specified object is equal to the current RevisionForCreationDto; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as RevisionForCreationDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current RevisionForCreationDto.</returns>
    /// <remarks>
    /// The hash code is based on all properties to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode()
    {
        return HashCode.Combine(RevisionNumber, CreationDate, ModificationDate, DocumentId, IsDeleted);
    }

    /// <summary>
    /// Determines whether two RevisionForCreationDto instances are equal.
    /// </summary>
    /// <param name="left">The first RevisionForCreationDto to compare.</param>
    /// <param name="right">The second RevisionForCreationDto to compare.</param>
    /// <returns>true if the RevisionForCreationDtos are equal; otherwise, false.</returns>
    public static bool operator ==(RevisionForCreationDto? left, RevisionForCreationDto? right) =>
        EqualityComparer<RevisionForCreationDto>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two RevisionForCreationDto instances are not equal.
    /// </summary>
    /// <param name="left">The first RevisionForCreationDto to compare.</param>
    /// <param name="right">The second RevisionForCreationDto to compare.</param>
    /// <returns>true if the RevisionForCreationDtos are not equal; otherwise, false.</returns>
    public static bool operator !=(RevisionForCreationDto? left, RevisionForCreationDto? right) => !(left == right);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the RevisionForCreationDto.
    /// </summary>
    /// <returns>A string that represents the current RevisionForCreationDto.</returns>
    /// <remarks>
    /// The string representation includes key information about the creation operation,
    /// useful for logging, debugging, and audit trail purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var creationDto = new RevisionForCreationDto 
    /// { 
    ///     RevisionNumber = 1,
    ///     DocumentId = Guid.Parse("87654321-4321-8765-2109-876543210987"),
    ///     CreationDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
    ///     ModificationDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
    ///     IsDeleted = false
    /// };
    /// 
    /// Console.WriteLine(creationDto);
    /// // Output: "Create Revision 1 for Document 87654321-4321-8765-2109-876543210987 - Created: 2024-01-15 10:30:00, Deleted: False"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Create Revision {RevisionNumber} for Document {DocumentId} - " +
        $"Created: {CreationDate:yyyy-MM-dd HH:mm:ss}, Deleted: {IsDeleted}";

    #endregion String Representation
}