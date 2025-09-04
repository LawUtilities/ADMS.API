using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Data Transfer Object for updating existing matters with comprehensive validation and business rule enforcement.
/// </summary>
/// <remarks>
/// This DTO serves as the contract for updating existing matters within the ADMS legal document management system,
/// providing only the properties that can be safely modified while maintaining data integrity and business rule
/// compliance. It mirrors updatable properties from <see cref="ADMS.API.Entities.Matter"/> while enforcing
/// professional standards and validation rules appropriate for matter update operations.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Update-Focused Design:</strong> Contains only properties that can be safely updated in existing matters</item>
/// <item><strong>Professional Validation:</strong> Uses ADMS.API.Common.MatterValidationHelper for comprehensive data integrity</item>
/// <item><strong>Business Rule Enforcement:</strong> Enforces matter lifecycle and update-specific business rules</item>
/// <item><strong>Audit Trail Support:</strong> Designed to work with matter activity tracking and audit systems</item>
/// <item><strong>Immutable Design:</strong> Record type with init-only properties for thread-safe operations</item>
/// </list>
/// 
/// <para><strong>Updatable Properties:</strong></para>
/// This DTO includes only properties from ADMS.API.Entities.Matter that are appropriate for updates:
/// <list type="bullet">
/// <item><strong>Description:</strong> Matter description (with uniqueness validation considerations)</item>
/// <item><strong>IsArchived:</strong> Archival status (with proper state transition validation)</item>
/// <item><strong>CreationDate:</strong> Allowed for data correction scenarios under strict validation</item>
/// </list>
/// 
/// <para><strong>Excluded Properties:</strong></para>
/// Properties excluded for data integrity and business rule compliance:
/// <list type="bullet">
/// <item><strong>Id:</strong> Matter ID is immutable and set during creation</item>
/// <item><strong>IsDeleted:</strong> Deletion requires separate operations with proper audit trails</item>
/// <item><strong>Document Collections:</strong> Managed through separate document operations</item>
/// <item><strong>Activity Collections:</strong> Managed through activity tracking systems</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter Metadata Updates:</strong> Updating matter descriptions and basic properties</item>
/// <item><strong>Matter State Management:</strong> Archiving and unarchiving matters through proper workflows</item>
/// <item><strong>Data Correction:</strong> Correcting matter information while maintaining audit trails</item>
/// <item><strong>Batch Updates:</strong> Bulk matter updates with consistent validation and business rules</item>
/// <item><strong>API Operations:</strong> REST API endpoints for matter update operations</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter Lifecycle Management:</strong> Proper matter state transitions for legal practice workflows</item>
/// <item><strong>Professional Standards:</strong> Maintains professional naming conventions and validation rules</item>
/// <item><strong>Audit Compliance:</strong> Designed to work with audit trail and activity tracking systems</item>
/// <item><strong>Data Integrity:</strong> Comprehensive validation ensures matter data integrity and consistency</item>
/// </list>
/// 
/// <para><strong>Business Rules for Updates:</strong></para>
/// <list type="bullet">
/// <item><strong>Description Updates:</strong> Must maintain uniqueness across the system</item>
/// <item><strong>Archive State Changes:</strong> Must follow proper matter lifecycle transitions</item>
/// <item><strong>Creation Date Updates:</strong> Allowed only for data correction with validation</item>
/// <item><strong>Audit Trail Integration:</strong> All updates should be tracked through matter activity systems</item>
/// </list>
/// 
/// <para><strong>Update Validation Strategy:</strong></para>
/// <list type="bullet">
/// <item><strong>Property Validation:</strong> Each property validated against professional standards</item>
/// <item><strong>Business Rule Validation:</strong> Cross-property validation for business rule compliance</item>
/// <item><strong>State Transition Validation:</strong> Ensures valid state transitions during updates</item>
/// <item><strong>Uniqueness Validation:</strong> Description uniqueness validation in update context</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a matter update DTO
/// var updateDto = new MatterForUpdateDto
/// {
///     Description = "Updated Matter Description - Smith Family Trust",
///     IsArchived = true,
///     CreationDate = originalMatter.CreationDate // Typically preserved
/// };
/// 
/// // Validating the update DTO
/// var validationResults = MatterForUpdateDto.ValidateModel(updateDto);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Using for matter updates
/// if (updateDto.IsValid)
/// {
///     // Apply updates to existing matter
///     await matterService.UpdateMatterAsync(existingMatterId, updateDto);
/// }
/// </code>
/// </example>
public record MatterForUpdateDto : IValidatableObject, IEquatable<MatterForUpdateDto>
{
    #region Updatable Properties

    /// <summary>
    /// Gets the matter description for update operations.
    /// </summary>
    /// <remarks>
    /// The matter description serves as the primary human-readable identifier and can be updated to reflect
    /// changes in matter scope, client requirements, or organizational needs. This property corresponds to
    /// <see cref="ADMS.API.Entities.Matter.Description"/> and must maintain uniqueness across the system
    /// while adhering to professional naming conventions.
    /// 
    /// <para><strong>Update Validation Rules (via ADMS.API.Common.MatterValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null, empty, or whitespace</item>
    /// <item><strong>Length:</strong> 3-128 characters (matching database constraint)</item>
    /// <item><strong>Uniqueness:</strong> Must be unique across all matters in the system (excluding current matter)</item>
    /// <item><strong>Format:</strong> Must contain letters and start/end with alphanumeric characters</item>
    /// <item><strong>Reserved Words:</strong> Cannot contain system-reserved terms</item>
    /// <item><strong>Professional Standards:</strong> Must follow professional legal practice naming conventions</item>
    /// </list>
    /// 
    /// <para><strong>Update Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Audit Trail:</strong> Description changes should be tracked through matter activity systems</item>
    /// <item><strong>Client Communication:</strong> Description changes may require client notification</item>
    /// <item><strong>System Impact:</strong> Description changes affect all matter references and displays</item>
    /// <item><strong>Professional Standards:</strong> Updates must maintain professional legal practice standards</item>
    /// </list>
    /// 
    /// <para><strong>Professional Naming Examples for Updates:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Scope Changes:</strong> "Smith Family Trust" → "Smith Family Trust - Estate Planning"</item>
    /// <item><strong>Status Updates:</strong> "ABC Corp Merger" → "ABC Corp Merger - Phase II"</item>
    /// <item><strong>Clarifications:</strong> "Johnson Matter" → "Johnson Estate Probate"</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Matter.Description"/> with identical validation
    /// rules and professional standards, ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional matter description updates
    /// var updateDto = new MatterForUpdateDto 
    /// { 
    ///     Description = "Smith Family Trust - Updated Scope",
    ///     IsArchived = false,
    ///     CreationDate = originalMatter.CreationDate
    /// };
    /// 
    /// // Validation before update
    /// var isValid = MatterValidationHelper.IsValidDescription(updateDto.Description);
    /// 
    /// // Audit trail integration
    /// await auditService.LogMatterDescriptionChange(matterId, oldDescription, updateDto.Description);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter description is required.")]
    [StringLength(
        128, // Use constant value directly
        MinimumLength = 3, // Use constant value directly
        ErrorMessage = "Matter description must be between 3 and 128 characters.")]
    public required string Description { get; init; }

    /// <summary>
    /// Gets a value indicating whether the matter should be archived.
    /// </summary>
    /// <remarks>
    /// The archived status controls the matter's lifecycle state and can be updated to reflect changes in
    /// matter activity and client requirements. This property corresponds to <see cref="ADMS.API.Entities.Matter.IsArchived"/>
    /// and must follow proper state transition rules during updates.
    /// 
    /// <para><strong>Archive State Transition Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Active to Archived:</strong> Valid transition for completed or inactive matters</item>
    /// <item><strong>Archived to Active:</strong> Valid transition when matter becomes active again</item>
    /// <item><strong>Audit Requirements:</strong> All state changes must be tracked in matter activity logs</item>
    /// <item><strong>Document Impact:</strong> Archival status affects document accessibility and operations</item>
    /// </list>
    /// 
    /// <para><strong>Professional Practice Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Matter Lifecycle:</strong> Supports professional matter lifecycle management workflows</item>
    /// <item><strong>Client Communication:</strong> Archival status changes may require client notification</item>
    /// <item><strong>Practice Organization:</strong> Helps organize active vs completed work for efficiency</item>
    /// <item><strong>Compliance Support:</strong> Maintains matter records for professional and legal compliance</item>
    /// </list>
    /// 
    /// <para><strong>Update Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Active Documents Check:</strong> Consider document checkout status before archiving</item>
    /// <item><strong>Activity Validation:</strong> Ensure no active operations are in progress</item>
    /// <item><strong>Client Approval:</strong> May require client approval for archival status changes</item>
    /// <item><strong>Restoration Rules:</strong> Archived matters can be reactivated when needed</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Matter.IsArchived"/> with identical business logic
    /// and state transition rules, ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Archive a completed matter
    /// var archiveUpdate = new MatterForUpdateDto 
    /// { 
    ///     Description = existingMatter.Description,
    ///     IsArchived = true, // Archiving the matter
    ///     CreationDate = existingMatter.CreationDate
    /// };
    /// 
    /// // Reactivate an archived matter
    /// var reactivateUpdate = new MatterForUpdateDto 
    /// { 
    ///     Description = existingMatter.Description,
    ///     IsArchived = false, // Reactivating the matter
    ///     CreationDate = existingMatter.CreationDate
    /// };
    /// 
    /// // Validate state transition
    /// var canArchive = !existingMatter.IsArchived && !existingMatter.IsDeleted;
    /// </code>
    /// </example>
    public bool IsArchived { get; init; }

    /// <summary>
    /// Gets the UTC creation date of the matter for update operations.
    /// </summary>
    /// <remarks>
    /// The creation date can be updated in specific scenarios such as data correction, migration, or administrative
    /// adjustments. This property corresponds to <see cref="ADMS.API.Entities.Matter.CreationDate"/> and is subject
    /// to strict validation to maintain temporal consistency and audit trail integrity.
    /// 
    /// <para><strong>Update Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Data Correction:</strong> Correcting incorrectly recorded creation dates</item>
    /// <item><strong>System Migration:</strong> Preserving historical creation dates during system transitions</item>
    /// <item><strong>Administrative Adjustment:</strong> Administrative corrections with proper authorization</item>
    /// <item><strong>Audit Trail Correction:</strong> Correcting temporal inconsistencies in audit records</item>
    /// </list>
    /// 
    /// <para><strong>Strict Validation Requirements (via ADMS.API.Common.MatterValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Valid Range:</strong> Between January 1, 1980, and current time (with tolerance)</item>
    /// <item><strong>Not Future:</strong> Cannot be set to future dates to prevent temporal inconsistencies</item>
    /// <item><strong>Not Default:</strong> Must be a valid DateTime, not DateTime.MinValue or default values</item>
    /// <item><strong>UTC Requirement:</strong> Must be provided in UTC for global consistency</item>
    /// <item><strong>Audit Trail Impact:</strong> Changes must not break audit trail chronology</item>
    /// </list>
    /// 
    /// <para><strong>Professional Practice Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Legal Implications:</strong> Creation date changes may have legal implications for case timelines</item>
    /// <item><strong>Client Communication:</strong> Date changes may require client notification and explanation</item>
    /// <item><strong>Audit Requirements:</strong> All creation date changes must be thoroughly documented</item>
    /// <item><strong>Authorization Required:</strong> Typically requires elevated permissions and audit approval</item>
    /// </list>
    /// 
    /// <para><strong>Update Best Practices:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Preserve Original:</strong> In most cases, preserve the original creation date</item>
    /// <item><strong>Document Changes:</strong> Thoroughly document reasons for creation date changes</item>
    /// <item><strong>Audit Trail:</strong> Maintain complete audit trail of all temporal changes</item>
    /// <item><strong>Authorization:</strong> Ensure proper authorization for creation date modifications</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Matter.CreationDate"/> with identical validation
    /// rules and temporal constraints, ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Typical update preserving original creation date
    /// var standardUpdate = new MatterForUpdateDto 
    /// { 
    ///     Description = "Updated Description",
    ///     IsArchived = false,
    ///     CreationDate = existingMatter.CreationDate // Preserve original
    /// };
    /// 
    /// // Data correction scenario (requires special authorization)
    /// var correctionUpdate = new MatterForUpdateDto 
    /// { 
    ///     Description = existingMatter.Description,
    ///     IsArchived = existingMatter.IsArchived,
    ///     CreationDate = correctedCreationDate // Administrative correction
    /// };
    /// 
    /// // Validation for creation date updates
    /// var isValidDate = MatterValidationHelper.IsValidDate(correctionUpdate.CreationDate);
    /// if (correctionUpdate.CreationDate != existingMatter.CreationDate)
    /// {
    ///     await auditService.LogCreationDateChange(matterId, existingMatter.CreationDate, correctionUpdate.CreationDate);
    /// }
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Creation date is required.")]
    public required DateTime CreationDate { get; init; } = DateTime.UtcNow;

    #endregion Updatable Properties

    #region Computed Properties

    /// <summary>
    /// Gets the normalized description for consistent comparison and uniqueness validation.
    /// </summary>
    /// <remarks>
    /// This computed property provides a normalized version of the matter description using
    /// ADMS.API.Common.MatterValidationHelper normalization rules for consistent comparison
    /// and uniqueness validation during update operations.
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
    /// var update1 = new MatterForUpdateDto { Description = "  Contract   Review  " };
    /// var update2 = new MatterForUpdateDto { Description = "Contract Review" };
    /// 
    /// // Both will have the same normalized description: "Contract Review"
    /// bool areEquivalent = update1.NormalizedDescription == update2.NormalizedDescription; // true
    /// </code>
    /// </example>
    public string? NormalizedDescription => MatterValidationHelper.NormalizeDescription(Description);

    /// <summary>
    /// Gets the creation date formatted as a localized string for UI display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a user-friendly formatted representation of the creation date
    /// converted to local time, optimized for update interface display and temporal validation feedback.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Display in update interface
    /// var displayText = $"Creation Date: {updateDto.LocalCreationDateString}";
    /// </code>
    /// </example>
    public string LocalCreationDateString => CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the age of the matter in days based on the creation date being updated.
    /// </summary>
    /// <remarks>
    /// This computed property calculates the number of days since the matter creation date,
    /// useful for validating the reasonableness of creation date updates and providing
    /// temporal context during update operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Matter will show as {updateDto.AgeDays} days old after update");
    /// 
    /// // Validation for reasonable age
    /// if (updateDto.AgeDays > 365 * 10) // More than 10 years
    /// {
    ///     Console.WriteLine("Warning: Matter creation date is very old");
    /// }
    /// </code>
    /// </example>
    public double AgeDays => (DateTime.UtcNow - CreationDate).TotalDays;

    /// <summary>
    /// Gets a value indicating whether this update DTO has valid data for system operations.
    /// </summary>
    /// <remarks>
    /// This property provides a quick validation check without running full validation logic,
    /// useful for UI scenarios where immediate feedback is needed before processing update operations.
    /// 
    /// <para><strong>Validation Checks:</strong></para>
    /// <list type="bullet">
    /// <item>Description passes comprehensive validation</item>
    /// <item>Creation date is within valid temporal bounds</item>
    /// <item>All required properties are properly set</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (updateDto.IsValid)
    /// {
    ///     // Proceed with matter update
    ///     await ProcessMatterUpdate(matterId, updateDto);
    /// }
    /// else
    /// {
    ///     // Show validation errors to user
    ///     DisplayUpdateValidationErrors(updateDto);
    /// }
    /// </code>
    /// </example>
    public bool IsValid =>
        MatterValidationHelper.IsValidDescription(Description) &&
        MatterValidationHelper.IsValidDate(CreationDate);

    /// <summary>
    /// Gets the display text suitable for update confirmation and UI display.
    /// </summary>
    /// <remarks>
    /// Provides a consistent format for displaying matter update information in UI elements,
    /// using the description for clear identification during update operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Update confirmation display
    /// var confirmationText = $"Update matter to: {updateDto.DisplayText}";
    /// </code>
    /// </example>
    public string DisplayText => Description;

    /// <summary>
    /// Gets comprehensive update metrics for validation and analysis.
    /// </summary>
    /// <remarks>
    /// This property provides a structured object containing key metrics and information
    /// for update validation and professional compliance analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// var metrics = updateDto.UpdateMetrics;
    /// // Access comprehensive update validation metrics
    /// </code>
    /// </example>
    public object UpdateMetrics => new
    {
        UpdateInfo = new
        {
            Description,
            NormalizedDescription,
            IsArchived,
            LocalCreationDateString,
            DisplayText
        },
        ValidationInfo = new
        {
            IsValid,
            AgeDays
        },
        TemporalInfo = new
        {
            CreationDate,
            AgeDays,
            IsReasonableAge = AgeDays is >= 0 and <= 365 * 20 // Within 20 years
        }
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="MatterForUpdateDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using the ADMS.API.Common.MatterValidationHelper for consistency 
    /// with entity validation rules. This ensures the DTO maintains the same validation standards as 
    /// the corresponding ADMS.API.Entities.Matter entity while enforcing update-specific business rules.
    /// 
    /// <para><strong>Update-Specific Validation Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Description Validation:</strong> Comprehensive description validation including format, length, and standards</item>
    /// <item><strong>Date Validation:</strong> Creation date temporal consistency and update appropriateness</item>
    /// <item><strong>State Validation:</strong> Archive state transition validation</item>
    /// <item><strong>Business Rules:</strong> Update-specific business rule compliance</item>
    /// <item><strong>Professional Standards:</strong> Legal practice professional standard compliance</item>
    /// </list>
    /// 
    /// <para><strong>Integration with Validation Helpers:</strong></para>
    /// Uses the centralized MatterValidationHelper to ensure consistency across all matter-related
    /// validation in the system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterForUpdateDto 
    /// { 
    ///     Description = "", // Invalid
    ///     IsArchived = false,
    ///     CreationDate = DateTime.MinValue // Invalid
    /// };
    /// 
    /// var context = new ValidationContext(dto);
    /// var results = dto.Validate(context);
    /// 
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"Update Validation Error: {result.ErrorMessage}");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate matter description using centralized helper
        foreach (var result in ValidateDescription())
            yield return result;

        // Validate creation date for update appropriateness
        foreach (var result in ValidateCreationDate())
            yield return result;

        // Validate update-specific business rules
        foreach (var result in ValidateUpdateBusinessRules())
            yield return result;
    }

    /// <summary>
    /// Validates the <see cref="Description"/> property using ADMS validation standards for updates.
    /// </summary>
    /// <returns>A collection of validation results for the matter description.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.MatterValidationHelper.ValidateDescription for comprehensive validation
    /// including format, length, reserved words, and professional naming standards appropriate for updates.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateDescription()
    {
        return MatterValidationHelper.ValidateDescription(Description, nameof(Description));
    }

    /// <summary>
    /// Validates the <see cref="CreationDate"/> property using ADMS validation standards for updates.
    /// </summary>
    /// <returns>A collection of validation results for the creation date.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.MatterValidationHelper.ValidateDate with additional checks for update
    /// appropriateness and temporal consistency requirements.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateCreationDate()
    {
        foreach (var result in MatterValidationHelper.ValidateDate(CreationDate, nameof(CreationDate)))
            yield return result;

        // Additional validation for update scenarios
        if (AgeDays > 365 * 20) // More than 20 years old
        {
            yield return new ValidationResult(
                "Creation date results in unreasonably old matter age. Please verify the date is correct.",
                [nameof(CreationDate)]);
        }
    }

    /// <summary>
    /// Validates update-specific business rules and constraints.
    /// </summary>
    /// <returns>A collection of validation results for update business rules.</returns>
    /// <remarks>
    /// Validates business rules specific to matter update operations, including professional
    /// practice standards and update-specific constraints.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateUpdateBusinessRules()
    {
        // Validate description normalization for updates
        if (string.IsNullOrEmpty(NormalizedDescription))
        {
            yield return new ValidationResult(
                "Description cannot be normalized properly. Please ensure it contains valid characters and content.",
                [nameof(Description)]);
        }

        // Additional update-specific validations can be added here
        // For example, validating against current matter state, checking permissions, etc.
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="MatterForUpdateDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The MatterForUpdateDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate MatterForUpdateDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterForUpdateDto 
    /// { 
    ///     Description = "Updated Smith Family Trust",
    ///     IsArchived = true,
    ///     CreationDate = DateTime.UtcNow.AddDays(-30)
    /// };
    /// 
    /// var results = MatterForUpdateDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Matter update validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterForUpdateDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterForUpdateDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a MatterForUpdateDto from an existing ADMS.API.Entities.Matter entity with validation.
    /// </summary>
    /// <param name="matter">The Matter entity to create update DTO from. Cannot be null.</param>
    /// <returns>A valid MatterForUpdateDto instance representing the current matter state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when matter is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create MatterForUpdateDto instances from
    /// existing ADMS.API.Entities.Matter entities, preserving current values for update operations.
    /// This is useful for pre-populating update forms or creating baseline update DTOs.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create update DTO from existing matter
    /// var existingMatter = await context.Matters.FindAsync(matterId);
    /// var updateDto = MatterForUpdateDto.FromEntity(existingMatter);
    /// 
    /// // Modify properties as needed
    /// updateDto = updateDto with { Description = "Updated Description" };
    /// 
    /// // Apply updates
    /// await matterService.UpdateMatterAsync(matterId, updateDto);
    /// </code>
    /// </example>
    public static MatterForUpdateDto FromEntity([NotNull] Entities.Matter matter)
    {
        ArgumentNullException.ThrowIfNull(matter, nameof(matter));

        var dto = new MatterForUpdateDto
        {
            Description = matter.Description,
            IsArchived = matter.IsArchived,
            CreationDate = matter.CreationDate
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterForUpdateDto from entity: {errorMessages}");

    }

    /// <summary>
    /// Creates a MatterForUpdateDto with only the description updated, preserving other properties.
    /// </summary>
    /// <param name="baseMatter">The base matter to preserve properties from. Cannot be null.</param>
    /// <param name="newDescription">The new description to set. Cannot be null or empty.</param>
    /// <returns>A valid MatterForUpdateDto instance with updated description.</returns>
    /// <exception cref="ArgumentNullException">Thrown when baseMatter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when newDescription is null or empty.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a convenient way to create update DTOs for description-only changes,
    /// which is a common update scenario in matter management.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Update only the description
    /// var updateDto = MatterForUpdateDto.WithUpdatedDescription(
    ///     existingMatter, 
    ///     "Updated Matter Description - Phase II");
    /// 
    /// await matterService.UpdateMatterAsync(matterId, updateDto);
    /// </code>
    /// </example>
    public static MatterForUpdateDto WithUpdatedDescription([NotNull] Entities.Matter baseMatter, [NotNull] string newDescription)
    {
        ArgumentNullException.ThrowIfNull(baseMatter, nameof(baseMatter));
        ArgumentException.ThrowIfNullOrWhiteSpace(newDescription, nameof(newDescription));

        var dto = new MatterForUpdateDto
        {
            Description = newDescription.Trim(),
            IsArchived = baseMatter.IsArchived,
            CreationDate = baseMatter.CreationDate
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterForUpdateDto with updated description: {errorMessages}");

    }

    /// <summary>
    /// Creates a MatterForUpdateDto with only the archive status updated, preserving other properties.
    /// </summary>
    /// <param name="baseMatter">The base matter to preserve properties from. Cannot be null.</param>
    /// <param name="newIsArchived">The new archive status to set.</param>
    /// <returns>A valid MatterForUpdateDto instance with updated archive status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when baseMatter is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a convenient way to create update DTOs for archive status changes,
    /// which is a common update scenario in matter lifecycle management.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Archive a matter
    /// var archiveDto = MatterForUpdateDto.WithUpdatedArchiveStatus(existingMatter, true);
    /// await matterService.UpdateMatterAsync(matterId, archiveDto);
    /// 
    /// // Unarchive a matter
    /// var unarchiveDto = MatterForUpdateDto.WithUpdatedArchiveStatus(existingMatter, false);
    /// await matterService.UpdateMatterAsync(matterId, unarchiveDto);
    /// </code>
    /// </example>
    public static MatterForUpdateDto WithUpdatedArchiveStatus([NotNull] Entities.Matter baseMatter, bool newIsArchived)
    {
        ArgumentNullException.ThrowIfNull(baseMatter, nameof(baseMatter));

        var dto = new MatterForUpdateDto
        {
            Description = baseMatter.Description,
            IsArchived = newIsArchived,
            CreationDate = baseMatter.CreationDate
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterForUpdateDto with updated archive status: {errorMessages}");

    }

    #endregion Static Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterForUpdateDto is equal to the current MatterForUpdateDto.
    /// </summary>
    /// <param name="other">The MatterForUpdateDto to compare with the current MatterForUpdateDto.</param>
    /// <returns>true if the specified MatterForUpdateDto is equal to the current MatterForUpdateDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all updatable properties to identify identical update operations.
    /// This is useful for detecting duplicate update requests and optimizing update operations.
    /// </remarks>
    public virtual bool Equals(MatterForUpdateDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase) &&
               IsArchived == other.IsArchived &&
               CreationDate == other.CreationDate;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterForUpdateDto.</returns>
    /// <remarks>
    /// The hash code is based on all updatable properties to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Description.GetHashCode(StringComparison.OrdinalIgnoreCase),
            IsArchived,
            CreationDate);
    }

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterForUpdateDto.
    /// </summary>
    /// <returns>A string that represents the current MatterForUpdateDto.</returns>
    /// <remarks>
    /// The string representation includes key update information for identification and logging purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterForUpdateDto 
    /// { 
    ///     Description = "Updated Smith Family Trust",
    ///     IsArchived = true,
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "Update Matter: Updated Smith Family Trust - Archive: True"
    /// </code>
    /// </example>
    public override string ToString() => $"Update Matter: {Description} - Archive: {IsArchived}";

    #endregion String Representation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this update represents an archive status change.
    /// </summary>
    /// <param name="currentMatter">The current matter state to compare against. Cannot be null.</param>
    /// <returns>true if the archive status is being changed; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify when an update operation includes an archive status change,
    /// which may require additional processing, notifications, or audit trail entries.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (updateDto.IsArchiveStatusChange(currentMatter))
    /// {
    ///     // Handle archive status change
    ///     await notificationService.NotifyArchiveStatusChange(matterId, updateDto.IsArchived);
    ///     await auditService.LogArchiveStatusChange(matterId, currentMatter.IsArchived, updateDto.IsArchived);
    /// }
    /// </code>
    /// </example>
    public bool IsArchiveStatusChange([NotNull] Entities.Matter currentMatter)
    {
        ArgumentNullException.ThrowIfNull(currentMatter, nameof(currentMatter));
        return IsArchived != currentMatter.IsArchived;
    }

    /// <summary>
    /// Determines whether this update represents a description change.
    /// </summary>
    /// <param name="currentMatter">The current matter state to compare against. Cannot be null.</param>
    /// <returns>true if the description is being changed; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify when an update operation includes a description change,
    /// which may require uniqueness validation, client notification, or system-wide reference updates.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (updateDto.IsDescriptionChange(currentMatter))
    /// {
    ///     // Validate uniqueness and handle description change
    ///     await ValidateDescriptionUniqueness(updateDto.Description, currentMatter.Id);
    ///     await auditService.LogDescriptionChange(matterId, currentMatter.Description, updateDto.Description);
    /// }
    /// </code>
    /// </example>
    public bool IsDescriptionChange([NotNull] Entities.Matter currentMatter)
    {
        ArgumentNullException.ThrowIfNull(currentMatter, nameof(currentMatter));
        return !string.Equals(Description, currentMatter.Description, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether this update represents a creation date change.
    /// </summary>
    /// <param name="currentMatter">The current matter state to compare against. Cannot be null.</param>
    /// <returns>true if the creation date is being changed; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify when an update operation includes a creation date change,
    /// which typically requires special authorization and audit trail considerations.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (updateDto.IsCreationDateChange(currentMatter))
    /// {
    ///     // Handle creation date change with special authorization
    ///     await authorizationService.ValidateCreationDateChangePermission(userId, matterId);
    ///     await auditService.LogCreationDateChange(matterId, currentMatter.CreationDate, updateDto.CreationDate);
    /// }
    /// </code>
    /// </example>
    public bool IsCreationDateChange([NotNull] Entities.Matter currentMatter)
    {
        ArgumentNullException.ThrowIfNull(currentMatter, nameof(currentMatter));
        return CreationDate != currentMatter.CreationDate;
    }

    /// <summary>
    /// Gets a summary of all changes represented by this update DTO.
    /// </summary>
    /// <param name="currentMatter">The current matter state to compare against. Cannot be null.</param>
    /// <returns>A dictionary containing information about all changes in this update.</returns>
    /// <remarks>
    /// This method provides a comprehensive analysis of what changes are represented by the update DTO,
    /// useful for audit logging, user notifications, and update processing decisions.
    /// </remarks>
    /// <example>
    /// <code>
    /// var changeSummary = updateDto.GetChangeSummary(currentMatter);
    /// foreach (var change in changeSummary)
    /// {
    ///     Console.WriteLine($"{change.Key}: {change.Value}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetChangeSummary([NotNull] Entities.Matter currentMatter)
    {
        ArgumentNullException.ThrowIfNull(currentMatter, nameof(currentMatter));

        return new Dictionary<string, object>
        {
            ["HasChanges"] = IsDescriptionChange(currentMatter) || IsArchiveStatusChange(currentMatter) || IsCreationDateChange(currentMatter),
            ["DescriptionChange"] = new
            {
                IsChanged = IsDescriptionChange(currentMatter),
                OldValue = currentMatter.Description,
                NewValue = Description
            },
            ["ArchiveStatusChange"] = new
            {
                IsChanged = IsArchiveStatusChange(currentMatter),
                OldValue = currentMatter.IsArchived,
                NewValue = IsArchived
            },
            ["CreationDateChange"] = new
            {
                IsChanged = IsCreationDateChange(currentMatter),
                OldValue = currentMatter.CreationDate,
                NewValue = CreationDate
            }
        }.ToImmutableDictionary();
    }

    #endregion Business Logic Methods
}