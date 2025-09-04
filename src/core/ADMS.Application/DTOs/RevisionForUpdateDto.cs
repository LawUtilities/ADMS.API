using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Data Transfer Object for updating existing document revisions in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// This DTO provides a structured approach to updating revision properties while maintaining data integrity
/// and business rule compliance. It mirrors the updatable properties of <see cref="ADMS.API.Entities.Revision"/>
/// and enforces the same validation rules to ensure consistency between API operations and entity state.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Update-Focused Design:</strong> Contains only properties that can be modified for existing revisions</item>
/// <item><strong>Entity Synchronization:</strong> Mirrors updatable properties from ADMS.API.Entities.Revision</item>
/// <item><strong>Comprehensive Validation:</strong> Uses ADMS.API.Common.RevisionValidationHelper for data integrity</item>
/// <item><strong>Business Rule Enforcement:</strong> Ensures revision updates comply with legal document management standards</item>
/// <item><strong>Audit Trail Support:</strong> Designed to work with revision activity tracking and audit systems</item>
/// </list>
/// 
/// <para><strong>Update Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Metadata Updates:</strong> Updating revision timestamps and properties</item>
/// <item><strong>Correction Operations:</strong> Correcting revision information after creation</item>
/// <item><strong>Administrative Updates:</strong> System-level revision property modifications</item>
/// <item><strong>Bulk Operations:</strong> Batch revision updates with validation</item>
/// </list>
/// 
/// <para><strong>Entity Alignment:</strong></para>
/// This DTO maps to updatable properties of ADMS.API.Entities.Revision:
/// <list type="bullet">
/// <item><strong>RevisionNumber:</strong> Sequential version number (with business rule constraints)</item>
/// <item><strong>CreationDate:</strong> Original creation timestamp (limited update scenarios)</item>
/// <item><strong>ModificationDate:</strong> Last modification timestamp (automatically updated)</item>
/// <item><strong>IsDeleted:</strong> Soft deletion state for audit trail preservation</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Version Control Integrity:</strong> Maintains proper revision sequencing and numbering</item>
/// <item><strong>Temporal Consistency:</strong> Ensures chronological order of revision modifications</item>
/// <item><strong>Audit Compliance:</strong> Supports legal document audit trail requirements</item>
/// <item><strong>Data Integrity:</strong> Prevents updates that would corrupt document version history</item>
/// </list>
/// 
/// <para><strong>Validation Strategy:</strong></para>
/// <list type="bullet">
/// <item><strong>Individual Property Validation:</strong> Each property validated using RevisionValidationHelper</item>
/// <item><strong>Cross-Property Validation:</strong> Ensures relationships between properties remain valid</item>
/// <item><strong>Business Rule Validation:</strong> Enforces legal document management business rules</item>
/// <item><strong>Entity Consistency:</strong> Maintains consistency with entity validation logic</item>
/// </list>
/// 
/// <para><strong>Usage Guidelines:</strong></para>
/// <list type="bullet">
/// <item>Use for updating existing revision metadata and properties</item>
/// <item>Validate thoroughly before applying updates to preserve data integrity</item>
/// <item>Consider audit trail implications when updating revision properties</item>
/// <item>Ensure proper authorization before allowing revision updates</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Updating revision modification date
/// var updateDto = new RevisionForUpdateDto
/// {
///     RevisionNumber = 2,
///     CreationDate = originalRevision.CreationDate, // Keep original
///     ModificationDate = DateTime.UtcNow,           // Update to current time
///     IsDeleted = false
/// };
/// 
/// // Validating before update
/// var validationResults = RevisionForUpdateDto.ValidateModel(updateDto);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Validation Error: {result.ErrorMessage}");
///     }
/// }
/// else
/// {
///     // Apply update to entity
///     await revisionService.UpdateRevisionAsync(revisionId, updateDto);
/// }
/// 
/// // Soft delete operation
/// var deleteDto = updateDto with { IsDeleted = true, ModificationDate = DateTime.UtcNow };
/// await revisionService.UpdateRevisionAsync(revisionId, deleteDto);
/// </code>
/// </example>
public class RevisionForUpdateDto : IValidatableObject, IEquatable<RevisionForUpdateDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the revision number for the document.
    /// </summary>
    /// <remarks>
    /// The revision number represents the sequential version of the document, corresponding to
    /// <see cref="ADMS.API.Entities.Revision.RevisionNumber"/>. While typically immutable after creation,
    /// certain administrative scenarios may require revision number updates with proper authorization.
    /// 
    /// <para><strong>Update Constraints (via ADMS.API.Common.RevisionValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Range:</strong> 1 to 999,999 (following legal document versioning standards)</item>
    /// <item><strong>Sequential Integrity:</strong> Updates must maintain proper sequencing within document</item>
    /// <item><strong>Business Rules:</strong> Changes must not disrupt existing version control flow</item>
    /// <item><strong>Authorization:</strong> Typically requires administrative privileges to modify</item>
    /// </list>
    /// 
    /// <para><strong>Update Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Administrative Correction:</strong> Fixing incorrectly assigned revision numbers</item>
    /// <item><strong>Data Migration:</strong> Updating revision numbers during system migrations</item>
    /// <item><strong>Sequence Repair:</strong> Repairing broken revision sequences after data issues</item>
    /// <item><strong>Compliance Updates:</strong> Adjusting numbering for legal compliance requirements</item>
    /// </list>
    /// 
    /// <para><strong>Legal Implications:</strong></para>
    /// Revision number changes can affect legal document version tracking and should be performed
    /// with careful consideration of audit trail implications and legal compliance requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Administrative revision number correction
    /// var updateDto = new RevisionForUpdateDto
    /// {
    ///     RevisionNumber = 3, // Correcting from incorrect number
    ///     CreationDate = originalRevision.CreationDate,
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// 
    /// // Note: This operation typically requires admin privileges and audit logging
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Revision number is required.")]
    [Range(RevisionValidationHelper.MinRevisionNumber, RevisionValidationHelper.MaxRevisionNumber,
        ErrorMessage = "Revision number must be between 1 and 999999.")]
    public required int RevisionNumber { get; set; }

    /// <summary>
    /// Gets or sets the creation date for the revision (in UTC).
    /// </summary>
    /// <remarks>
    /// The creation date represents when the revision was initially created, corresponding to
    /// <see cref="ADMS.API.Entities.Revision.CreationDate"/>. While typically immutable after creation,
    /// certain correction scenarios may require creation date updates with proper authorization.
    /// 
    /// <para><strong>Update Constraints (via ADMS.API.Common.RevisionValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>UTC Storage:</strong> Must be stored in UTC format for consistency</item>
    /// <item><strong>Valid Range:</strong> Between January 1, 1980 and current time (with tolerance)</item>
    /// <item><strong>Temporal Consistency:</strong> Must precede or equal the ModificationDate</item>
    /// <item><strong>Business Rules:</strong> Changes must maintain chronological integrity</item>
    /// </list>
    /// 
    /// <para><strong>Update Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Data Correction:</strong> Fixing incorrectly recorded creation timestamps</item>
    /// <item><strong>Migration Updates:</strong> Adjusting timestamps during system migrations</item>
    /// <item><strong>Administrative Correction:</strong> Correcting creation dates for compliance</item>
    /// <item><strong>Audit Trail Repair:</strong> Repairing chronological inconsistencies</item>
    /// </list>
    /// 
    /// <para><strong>Legal Significance:</strong></para>
    /// Creation date changes can impact legal document timelines and audit trails. Such changes
    /// should be made only with proper authorization and comprehensive audit logging.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Correcting creation date for data integrity
    /// var updateDto = new RevisionForUpdateDto
    /// {
    ///     RevisionNumber = revision.RevisionNumber,
    ///     CreationDate = correctedCreationDate.ToUniversalTime(), // Ensure UTC
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Creation date is required.")]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the modification date for the revision (in UTC).
    /// </summary>
    /// <remarks>
    /// The modification date represents when the revision was last modified, corresponding to
    /// <see cref="ADMS.API.Entities.Revision.ModificationDate"/>. This property is typically
    /// updated automatically when revision changes occur, ensuring accurate change tracking.
    /// 
    /// <para><strong>Update Behavior:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Automatic Updates:</strong> Usually updated automatically by business logic</item>
    /// <item><strong>Manual Override:</strong> Can be set explicitly for specific scenarios</item>
    /// <item><strong>UTC Consistency:</strong> Always stored in UTC for cross-timezone consistency</item>
    /// <item><strong>Audit Trail Integration:</strong> Changes trigger audit trail entries</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Chronological Order:</strong> Must be >= CreationDate</item>
    /// <item><strong>Valid Range:</strong> Between January 1, 1980 and current time (with tolerance)</item>
    /// <item><strong>Reasonable Span:</strong> Time span from creation must be within reasonable limits</item>
    /// <item><strong>UTC Format:</strong> Must be provided in UTC for consistency</item>
    /// </list>
    /// 
    /// <para><strong>Business Logic Integration:</strong></para>
    /// The modification date is typically updated automatically when revision content changes,
    /// but can be set explicitly in this DTO for administrative or correction scenarios.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard update with automatic timestamp
    /// var updateDto = new RevisionForUpdateDto
    /// {
    ///     RevisionNumber = revision.RevisionNumber,
    ///     CreationDate = revision.CreationDate,
    ///     ModificationDate = DateTime.UtcNow // Update to current time
    /// };
    /// 
    /// // Specific timestamp for administrative scenarios
    /// var adminUpdate = new RevisionForUpdateDto
    /// {
    ///     RevisionNumber = revision.RevisionNumber,
    ///     CreationDate = revision.CreationDate,
    ///     ModificationDate = specificTimestamp.ToUniversalTime()
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Modification date is required.")]
    public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether the revision is deleted.
    /// </summary>
    /// <remarks>
    /// This property implements soft deletion for the revision, corresponding to
    /// <see cref="ADMS.API.Entities.Revision.IsDeleted"/>. Soft deletion allows the revision to be
    /// marked as deleted while preserving all data for audit trail and legal compliance purposes.
    /// 
    /// <para><strong>Soft Deletion Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Audit Trail Preservation:</strong> Maintains complete history for legal compliance</item>
    /// <item><strong>Referential Integrity:</strong> Preserves relationships with audit and activity records</item>
    /// <item><strong>Recovery Operations:</strong> Enables restoration if deletion was accidental</item>
    /// <item><strong>Legal Compliance:</strong> Supports legal document retention requirements</item>
    /// </list>
    /// 
    /// <para><strong>Update Operations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Soft Delete:</strong> Set to true to mark revision as deleted</item>
    /// <item><strong>Restore:</strong> Set to false to restore previously deleted revision</item>
    /// <item><strong>Status Toggle:</strong> Administrative operations to change deletion state</item>
    /// <item><strong>Bulk Operations:</strong> Mass deletion/restoration operations</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Deletion state changes typically require administrative authorization</item>
    /// <item>Deleted revisions are filtered from normal operations but preserved for audit</item>
    /// <item>Restoration operations should verify business rule compliance</item>
    /// <item>All deletion state changes should be logged in audit trail</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance:</strong></para>
    /// Soft deletion supports legal document management requirements by maintaining complete
    /// version history while allowing logical removal of revisions from active use.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Soft delete a revision
    /// var deleteDto = new RevisionForUpdateDto
    /// {
    ///     RevisionNumber = revision.RevisionNumber,
    ///     CreationDate = revision.CreationDate,
    ///     ModificationDate = DateTime.UtcNow,
    ///     IsDeleted = true // Mark as deleted
    /// };
    /// 
    /// // Restore a deleted revision
    /// var restoreDto = new RevisionForUpdateDto
    /// {
    ///     RevisionNumber = revision.RevisionNumber,
    ///     CreationDate = revision.CreationDate,
    ///     ModificationDate = DateTime.UtcNow,
    ///     IsDeleted = false // Restore from deletion
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
    /// var updateDto = new RevisionForUpdateDto 
    /// { 
    ///     CreationDate = new DateTime(2024, 1, 15, 14, 30, 45, DateTimeKind.Utc) 
    /// };
    /// 
    /// // Display in UI
    /// label.Text = $"Originally Created: {updateDto.LocalCreationDateString}";
    /// // Output: "Originally Created: Monday, 15 January 2024 16:30:45" (if local time is UTC+2)
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
    /// var updateDto = new RevisionForUpdateDto 
    /// { 
    ///     ModificationDate = new DateTime(2024, 1, 15, 16, 45, 30, DateTimeKind.Utc) 
    /// };
    /// 
    /// // Display in UI
    /// label.Text = $"Last Modified: {updateDto.LocalModificationDateString}";
    /// // Output: "Last Modified: Monday, 15 January 2024 18:45:30" (if local time is UTC+2)
    /// </code>
    /// </example>
    public string LocalModificationDateString => ModificationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the time span between creation and modification dates.
    /// </summary>
    /// <remarks>
    /// This computed property calculates the duration between when the revision was created
    /// and when it was last modified, providing insight into revision development time and
    /// modification patterns. Useful for update validation and audit analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (updateDto.ModificationTimeSpan.TotalDays > 30)
    /// {
    ///     Console.WriteLine($"Revision has been modified {updateDto.ModificationTimeSpan.TotalDays:F1} days after creation");
    /// }
    /// </code>
    /// </example>
    public TimeSpan ModificationTimeSpan => ModificationDate - CreationDate;

    /// <summary>
    /// Gets a value indicating whether this update DTO has valid data for system operations.
    /// </summary>
    /// <remarks>
    /// This property provides a quick validation check without running full validation logic,
    /// useful for UI scenarios where immediate feedback is needed before submitting updates.
    /// 
    /// <para><strong>Validation Checks:</strong></para>
    /// <list type="bullet">
    /// <item>Revision number is within valid bounds</item>
    /// <item>Both creation and modification dates are valid</item>
    /// <item>Date sequence is chronologically correct</item>
    /// <item>Time span between dates is reasonable</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (updateDto.IsValid)
    /// {
    ///     // Proceed with update operation
    ///     await revisionService.UpdateAsync(revisionId, updateDto);
    /// }
    /// else
    /// {
    ///     // Show validation errors to user
    ///     DisplayValidationErrors(updateDto);
    /// }
    /// </code>
    /// </example>
    public bool IsValid =>
        RevisionValidationHelper.IsValidRevisionNumber(RevisionNumber) &&
        RevisionValidationHelper.IsValidDate(CreationDate) &&
        RevisionValidationHelper.IsValidDate(ModificationDate) &&
        RevisionValidationHelper.IsValidDateSequence(CreationDate, ModificationDate) &&
        RevisionValidationHelper.IsValidDateTimeSpan(CreationDate, ModificationDate);

    /// <summary>
    /// Gets a value indicating whether this update represents a soft deletion operation.
    /// </summary>
    /// <remarks>
    /// This property helps identify when the update operation is specifically for soft deletion,
    /// useful for conditional logic and audit trail categorization.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (updateDto.IsDeletionUpdate)
    /// {
    ///     // Log as deletion operation
    ///     logger.LogInformation($"Soft deleting revision {updateDto.RevisionNumber}");
    /// }
    /// </code>
    /// </example>
    public bool IsDeletionUpdate => IsDeleted;

    /// <summary>
    /// Gets a value indicating whether this update represents a restoration operation.
    /// </summary>
    /// <remarks>
    /// This property is useful when the original revision state is known to be deleted,
    /// indicating the update is a restoration operation.
    /// </remarks>
    public bool IsRestorationUpdate => !IsDeleted;

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="RevisionForUpdateDto"/> for data integrity and business rules compliance.
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
    /// <item><strong>Cross-Property Validation:</strong> Ensures relationships between properties remain valid</item>
    /// <item><strong>Business Rules:</strong> Enforces legal document management business rules</item>
    /// <item><strong>Update Constraints:</strong> Validates update-specific business rules</item>
    /// </list>
    /// 
    /// <para><strong>Integration with RevisionValidationHelper:</strong></para>
    /// Uses the centralized validation helper to ensure consistency across all revision-related
    /// validation in the system, including entity validation, creation DTOs, and update DTOs.
    /// </remarks>
    /// <example>
    /// <code>
    /// var updateDto = new RevisionForUpdateDto 
    /// { 
    ///     RevisionNumber = 0, // Invalid
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow.AddHours(-1) // Invalid: before creation
    /// };
    /// 
    /// var context = new ValidationContext(updateDto);
    /// var results = updateDto.Validate(context);
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

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="RevisionForUpdateDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The RevisionForUpdateDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate RevisionForUpdateDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>API Validation:</strong> Validating incoming update DTOs in API controllers</item>
    /// <item><strong>Service Layer:</strong> Validation before processing revision update operations</item>
    /// <item><strong>Unit Testing:</strong> Simplified validation testing without ValidationContext</item>
    /// <item><strong>UI Validation:</strong> Client-side validation feedback</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Validate an update DTO instance
    /// var updateDto = new RevisionForUpdateDto 
    /// { 
    ///     RevisionNumber = 2,
    ///     CreationDate = DateTime.UtcNow.AddHours(-1),
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// 
    /// var results = RevisionForUpdateDto.ValidateModel(updateDto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Revision update validation failed: {errorMessages}");
    /// }
    /// 
    /// // Validate null DTO
    /// var nullResults = RevisionForUpdateDto.ValidateModel(null);
    /// // Returns validation error for null DTO
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] RevisionForUpdateDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("RevisionForUpdateDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a RevisionForUpdateDto from an ADMS.API.Entities.Revision entity with validation.
    /// </summary>
    /// <param name="revision">The Revision entity to convert. Cannot be null.</param>
    /// <param name="updateModificationDate">Whether to update the modification date to current time.</param>
    /// <returns>A valid RevisionForUpdateDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when revision is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create RevisionForUpdateDto instances from
    /// ADMS.API.Entities.Revision entities with automatic validation. It ensures the resulting
    /// DTO is valid before returning it.
    /// 
    /// <para><strong>Modification Date Handling:</strong></para>
    /// When updateModificationDate is true, the ModificationDate is set to current UTC time,
    /// useful for update operations. When false, preserves the original modification date.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create update DTO from entity with updated modification time
    /// var revision = await revisionRepository.GetByIdAsync(revisionId);
    /// var updateDto = RevisionForUpdateDto.FromEntity(revision, updateModificationDate: true);
    /// 
    /// // Modify properties as needed
    /// updateDto.IsDeleted = true; // Soft delete
    /// 
    /// // Apply update
    /// await revisionService.UpdateAsync(revisionId, updateDto);
    /// </code>
    /// </example>
    public static RevisionForUpdateDto FromEntity([NotNull] Entities.Revision revision, bool updateModificationDate = true)
    {
        ArgumentNullException.ThrowIfNull(revision, nameof(revision));

        var dto = new RevisionForUpdateDto
        {
            RevisionNumber = revision.RevisionNumber,
            CreationDate = revision.CreationDate,
            ModificationDate = updateModificationDate ? DateTime.UtcNow : revision.ModificationDate,
            IsDeleted = revision.IsDeleted
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid RevisionForUpdateDto from entity: {errorMessages}");

    }

    /// <summary>
    /// Creates a RevisionForUpdateDto for soft deletion operations.
    /// </summary>
    /// <param name="revision">The Revision entity to soft delete. Cannot be null.</param>
    /// <returns>A RevisionForUpdateDto configured for soft deletion.</returns>
    /// <exception cref="ArgumentNullException">Thrown when revision is null.</exception>
    /// <remarks>
    /// This factory method creates a properly configured update DTO for soft deletion operations,
    /// ensuring the modification date is updated and the IsDeleted flag is set correctly.
    /// </remarks>
    /// <example>
    /// <code>
    /// var revision = await revisionRepository.GetByIdAsync(revisionId);
    /// var deleteDto = RevisionForUpdateDto.ForSoftDeletion(revision);
    /// 
    /// await revisionService.UpdateAsync(revisionId, deleteDto);
    /// </code>
    /// </example>
    public static RevisionForUpdateDto ForSoftDeletion([NotNull] Entities.Revision revision)
    {
        ArgumentNullException.ThrowIfNull(revision, nameof(revision));

        return new RevisionForUpdateDto
        {
            RevisionNumber = revision.RevisionNumber,
            CreationDate = revision.CreationDate,
            ModificationDate = DateTime.UtcNow,
            IsDeleted = true
        };
    }

    /// <summary>
    /// Creates a RevisionForUpdateDto for restoration operations.
    /// </summary>
    /// <param name="revision">The Revision entity to restore. Cannot be null.</param>
    /// <returns>A RevisionForUpdateDto configured for restoration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when revision is null.</exception>
    /// <remarks>
    /// This factory method creates a properly configured update DTO for restoration operations,
    /// ensuring the modification date is updated and the IsDeleted flag is cleared.
    /// </remarks>
    /// <example>
    /// <code>
    /// var deletedRevision = await revisionRepository.GetByIdAsync(revisionId);
    /// var restoreDto = RevisionForUpdateDto.ForRestoration(deletedRevision);
    /// 
    /// await revisionService.UpdateAsync(revisionId, restoreDto);
    /// </code>
    /// </example>
    public static RevisionForUpdateDto ForRestoration([NotNull] Entities.Revision revision)
    {
        ArgumentNullException.ThrowIfNull(revision, nameof(revision));

        return new RevisionForUpdateDto
        {
            RevisionNumber = revision.RevisionNumber,
            CreationDate = revision.CreationDate,
            ModificationDate = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    #endregion Static Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified RevisionForUpdateDto is equal to the current RevisionForUpdateDto.
    /// </summary>
    /// <param name="other">The RevisionForUpdateDto to compare with the current RevisionForUpdateDto.</param>
    /// <returns>true if the specified RevisionForUpdateDto is equal to the current RevisionForUpdateDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all properties, as update DTOs represent specific
    /// sets of changes and should be compared by their content.
    /// </remarks>
    public bool Equals(RevisionForUpdateDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return RevisionNumber == other.RevisionNumber &&
               CreationDate == other.CreationDate &&
               ModificationDate == other.ModificationDate &&
               IsDeleted == other.IsDeleted;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current RevisionForUpdateDto.
    /// </summary>
    /// <param name="obj">The object to compare with the current RevisionForUpdateDto.</param>
    /// <returns>true if the specified object is equal to the current RevisionForUpdateDto; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as RevisionForUpdateDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current RevisionForUpdateDto.</returns>
    /// <remarks>
    /// The hash code is based on all properties to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode()
    {
        return HashCode.Combine(RevisionNumber, CreationDate, ModificationDate, IsDeleted);
    }

    /// <summary>
    /// Determines whether two RevisionForUpdateDto instances are equal.
    /// </summary>
    /// <param name="left">The first RevisionForUpdateDto to compare.</param>
    /// <param name="right">The second RevisionForUpdateDto to compare.</param>
    /// <returns>true if the RevisionForUpdateDtos are equal; otherwise, false.</returns>
    public static bool operator ==(RevisionForUpdateDto? left, RevisionForUpdateDto? right) =>
        EqualityComparer<RevisionForUpdateDto>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two RevisionForUpdateDto instances are not equal.
    /// </summary>
    /// <param name="left">The first RevisionForUpdateDto to compare.</param>
    /// <param name="right">The second RevisionForUpdateDto to compare.</param>
    /// <returns>true if the RevisionForUpdateDtos are not equal; otherwise, false.</returns>
    public static bool operator !=(RevisionForUpdateDto? left, RevisionForUpdateDto? right) => !(left == right);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the RevisionForUpdateDto.
    /// </summary>
    /// <returns>A string that represents the current RevisionForUpdateDto.</returns>
    /// <remarks>
    /// The string representation includes key information about the update operation,
    /// useful for logging, debugging, and audit trail purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var updateDto = new RevisionForUpdateDto 
    /// { 
    ///     RevisionNumber = 3,
    ///     CreationDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
    ///     ModificationDate = new DateTime(2024, 1, 15, 14, 45, 0, DateTimeKind.Utc),
    ///     IsDeleted = false
    /// };
    /// 
    /// Console.WriteLine(updateDto);
    /// // Output: "Update Revision 3 - Created: 2024-01-15 10:30:00, Modified: 2024-01-15 14:45:00, Deleted: False"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Update Revision {RevisionNumber} - Created: {CreationDate:yyyy-MM-dd HH:mm:ss}, " +
        $"Modified: {ModificationDate:yyyy-MM-dd HH:mm:ss}, Deleted: {IsDeleted}";

    #endregion String Representation
}