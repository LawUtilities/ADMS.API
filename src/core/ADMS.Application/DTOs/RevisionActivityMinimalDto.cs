using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Minimal Data Transfer Object representing a revision activity with essential information for audit trail and activity classification.
/// </summary>
/// <remarks>
/// This DTO serves as a lightweight representation of the revision activity lookup entity 
/// <see cref="ADMS.API.Entities.RevisionActivity"/>, providing essential information for activity classification,
/// audit trail display, and workflow operations within the ADMS legal document management system.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Entity Representation:</strong> Contains only essential properties from ADMS.API.Entities.RevisionActivity</item>
/// <item><strong>Activity Classification Focus:</strong> Optimized for activity type identification and classification</item>
/// <item><strong>Audit Trail Integration:</strong> Designed for audit log displays and activity tracking</item>
/// <item><strong>Professional Validation:</strong> Uses ADMS.API.Common.RevisionActivityValidationHelper for data integrity</item>
/// <item><strong>Immutable Design:</strong> Record type with init-only properties for thread-safe operations</item>
/// </list>
/// 
/// <para><strong>Entity Alignment:</strong></para>
/// This DTO mirrors essential properties from ADMS.API.Entities.RevisionActivity:
/// <list type="bullet">
/// <item><strong>Id:</strong> Unique identifier matching seeded activity GUIDs</item>
/// <item><strong>Activity:</strong> Standardized activity name (CREATED, SAVED, DELETED, RESTORED)</item>
/// </list>
/// 
/// <para><strong>Standard Activities Supported:</strong></para>
/// Based on <see cref="ADMS.API.Entities.RevisionActivity"/> seeded data:
/// <list type="bullet">
/// <item><strong>CREATED:</strong> Initial creation of a document revision (ID: 10000000-0000-0000-0000-000000000001)</item>
/// <item><strong>SAVED:</strong> Saving changes to an existing revision (ID: 10000000-0000-0000-0000-000000000004)</item>
/// <item><strong>DELETED:</strong> Soft deletion of a revision (ID: 10000000-0000-0000-0000-000000000002)</item>
/// <item><strong>RESTORED:</strong> Restoration of a deleted revision (ID: 10000000-0000-0000-0000-000000000003)</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Audit Trail Display:</strong> Activity type identification in revision activity logs</item>
/// <item><strong>Activity Classification:</strong> Categorizing and filtering revision activities</item>
/// <item><strong>API Responses:</strong> Lightweight activity data in REST API responses</item>
/// <item><strong>UI Controls:</strong> Activity selection dropdowns and filtering controls</item>
/// <item><strong>Workflow Operations:</strong> Activity type validation and business rule enforcement</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Standardized Activities:</strong> Consistent activity classification for legal compliance</item>
/// <item><strong>Audit Compliance:</strong> Proper activity categorization for audit trail requirements</item>
/// <item><strong>Professional Documentation:</strong> Clear activity descriptions for legal documentation</item>
/// <item><strong>System Integration:</strong> Compatible with ADMS entity relationships and workflows</item>
/// </list>
/// 
/// <para><strong>Data Integrity:</strong></para>
/// <list type="bullet">
/// <item><strong>Validation Integration:</strong> Uses RevisionActivityValidationHelper for consistent validation</item>
/// <item><strong>Standardized Format:</strong> Activity names follow uppercase convention</item>
/// <item><strong>Reserved Name Protection:</strong> Prevents use of system-reserved activity names</item>
/// <item><strong>Business Rule Compliance:</strong> Ensures activities conform to legal document management standards</item>
/// </list>
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Properties:</strong> Only essential properties to minimize memory footprint</item>
/// <item><strong>Immutable Design:</strong> Thread-safe for concurrent audit operations</item>
/// <item><strong>Validation Optimization:</strong> Efficient validation using centralized helpers</item>
/// <item><strong>Display Ready:</strong> Pre-validated for immediate use in UI components</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a revision activity for audit trails
/// var createdActivity = new RevisionActivityMinimalDto
/// {
///     Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
///     Activity = "CREATED"
/// };
/// 
/// // Using in audit trail scenarios
/// var auditMessage = $"User performed {createdActivity.Activity} operation";
/// 
/// // Activity classification for workflow
/// if (createdActivity.IsCreationActivity)
/// {
///     // Apply creation-specific business rules
///     ProcessCreationActivity(createdActivity);
/// }
/// 
/// // Validating activity DTO
/// var validationResults = RevisionActivityMinimalDto.ValidateModel(createdActivity);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Validation Error: {result.ErrorMessage}");
///     }
/// }
/// </code>
/// </example>
public record RevisionActivityMinimalDto : IValidatableObject, IEquatable<RevisionActivityMinimalDto>
{
    #region Core Properties

    /// <summary>
    /// Gets the unique identifier for the revision activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the revision activity within the ADMS system.
    /// The ID corresponds directly to the <see cref="ADMS.API.Entities.RevisionActivity.Id"/> property and uses
    /// specific seeded values for standard activities to ensure consistency across deployments.
    /// 
    /// <para><strong>Seeded Activity IDs:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> 10000000-0000-0000-0000-000000000001</item>
    /// <item><strong>DELETED:</strong> 10000000-0000-0000-0000-000000000002</item>
    /// <item><strong>RESTORED:</strong> 10000000-0000-0000-0000-000000000003</item>
    /// <item><strong>SAVED:</strong> 10000000-0000-0000-0000-000000000004</item>
    /// </list>
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Foreign Key Reference:</strong> Used in RevisionActivityUser junction entities</item>
    /// <item><strong>Business Logic:</strong> Enables activity-specific business rule implementation</item>
    /// <item><strong>API Operations:</strong> Activity identification in REST API operations</item>
    /// <item><strong>Database Queries:</strong> Primary key for activity-related database operations</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.RevisionActivity.Id"/> with identical seeded values,
    /// ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using seeded activity IDs
    /// var createdActivityId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    /// var createdActivity = new RevisionActivityMinimalDto 
    /// { 
    ///     Id = createdActivityId,
    ///     Activity = "CREATED" 
    /// };
    /// 
    /// // Business logic based on activity ID
    /// if (activity.Id == RevisionActivity.GetSeededActivityId("CREATED"))
    /// {
    ///     // Handle creation-specific logic
    /// }
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Activity ID is required.")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the standardized activity name describing the revision operation.
    /// </summary>
    /// <remarks>
    /// The activity name serves as the primary classification for revision operations within the ADMS system.
    /// This property corresponds to <see cref="ADMS.API.Entities.RevisionActivity.Activity"/> and must conform 
    /// to the standardized activity names defined in ADMS.API.Common.RevisionActivityValidationHelper.
    /// 
    /// <para><strong>Standard Activity Types:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Initial creation of a document revision</item>
    /// <item><strong>SAVED:</strong> Saving changes to an existing revision</item>
    /// <item><strong>DELETED:</strong> Soft deletion of a revision (preserves audit trail)</item>
    /// <item><strong>RESTORED:</strong> Restoration of a previously deleted revision</item>
    /// </list>
    /// 
    /// <para><strong>Validation Rules (via ADMS.API.Common.RevisionActivityValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null, empty, or whitespace</item>
    /// <item><strong>Length:</strong> 1-50 characters (matching database constraint)</item>
    /// <item><strong>Format:</strong> Letters, numbers, underscores only; must contain at least one letter</item>
    /// <item><strong>Allowed Values:</strong> Must be one of the standardized activities</item>
    /// <item><strong>Reserved Names:</strong> Cannot use system-reserved activity names</item>
    /// </list>
    /// 
    /// <para><strong>Business Context:</strong></para>
    /// Activity names are used throughout the system for:
    /// <list type="bullet">
    /// <item><strong>Audit Trail Classification:</strong> Categorizing revision activities for compliance reporting</item>
    /// <item><strong>Business Rule Enforcement:</strong> Activity-specific workflow and validation rules</item>
    /// <item><strong>User Interface Display:</strong> Human-readable activity descriptions in audit logs</item>
    /// <item><strong>Legal Compliance:</strong> Standardized activity tracking for legal document management</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.RevisionActivity.Activity"/> with identical validation
    /// rules and business logic to ensure consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard activity examples
    /// var createdActivity = new RevisionActivityMinimalDto { Activity = "CREATED" };
    /// var savedActivity = new RevisionActivityMinimalDto { Activity = "SAVED" };
    /// var deletedActivity = new RevisionActivityMinimalDto { Activity = "DELETED" };
    /// var restoredActivity = new RevisionActivityMinimalDto { Activity = "RESTORED" };
    /// 
    /// // Activity-based business logic
    /// var actionDescription = activity.Activity switch
    /// {
    ///     "CREATED" => "created a new revision",
    ///     "SAVED" => "saved changes to revision",
    ///     "DELETED" => "deleted revision",
    ///     "RESTORED" => "restored deleted revision",
    ///     _ => "performed unknown action"
    /// };
    /// 
    /// // Validation example
    /// bool isValid = RevisionActivityValidationHelper.IsActivityAllowed(activity.Activity);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Activity name is required.")]
    [StringLength(50, MinimumLength = 1,
        ErrorMessage = "Activity name must be between 1 and 50 characters.")]
    public required string Activity { get; init; }

    #endregion Core Properties

    #region Computed Properties

    /// <summary>
    /// Gets the normalized version of the activity name for consistent comparison and storage.
    /// </summary>
    /// <remarks>
    /// This computed property provides a normalized version of the activity name following
    /// the standardization rules from ADMS.API.Common.RevisionActivityValidationHelper.
    /// Normalization includes trimming whitespace and converting to uppercase for consistency.
    /// 
    /// <para><strong>Normalization Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Trims leading and trailing whitespace</item>
    /// <item>Converts to uppercase using invariant culture</item>
    /// <item>Returns original value if normalization fails</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity1 = new RevisionActivityMinimalDto { Activity = "  created  " };
    /// var activity2 = new RevisionActivityMinimalDto { Activity = "CREATED" };
    /// 
    /// // Both will have the same normalized activity: "CREATED"
    /// bool areEquivalent = activity1.NormalizedActivity == activity2.NormalizedActivity; // true
    /// </code>
    /// </example>
    public string NormalizedActivity =>
        RevisionActivityValidationHelper.NormalizeActivity(Activity) ?? Activity.ToUpperInvariant();

    /// <summary>
    /// Gets a value indicating whether this activity represents a creation operation.
    /// </summary>
    /// <remarks>
    /// This property provides a convenient way to identify creation activities for
    /// business rule enforcement and audit trail analysis, mirroring the business logic
    /// from ADMS.API.Entities.RevisionActivity.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsCreationActivity)
    /// {
    ///     // Apply creation-specific business rules
    ///     ProcessRevisionCreation(activity);
    /// }
    /// </code>
    /// </example>
    public bool IsCreationActivity =>
        string.Equals(Activity, "CREATED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this activity represents a deletion operation.
    /// </summary>
    /// <remarks>
    /// This property provides a convenient way to identify deletion activities for
    /// audit trail analysis and business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsDeletionActivity)
    /// {
    ///     // Apply deletion-specific business rules
    ///     ProcessRevisionDeletion(activity);
    /// }
    /// </code>
    /// </example>
    public bool IsDeletionActivity =>
        string.Equals(Activity, "DELETED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this activity represents a restoration operation.
    /// </summary>
    /// <remarks>
    /// This property provides a convenient way to identify restoration activities for
    /// audit trail analysis and recovery operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsRestorationActivity)
    /// {
    ///     // Apply restoration-specific business rules
    ///     ProcessRevisionRestoration(activity);
    /// }
    /// </code>
    /// </example>
    public bool IsRestorationActivity =>
        string.Equals(Activity, "RESTORED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this activity represents a save operation.
    /// </summary>
    /// <remarks>
    /// This property provides a convenient way to identify save activities for
    /// business rule enforcement and workflow analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsSaveActivity)
    /// {
    ///     // Apply save-specific business rules
    ///     ProcessRevisionSave(activity);
    /// }
    /// </code>
    /// </example>
    public bool IsSaveActivity =>
        string.Equals(Activity, "SAVED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this activity is one of the standard seeded activities.
    /// </summary>
    /// <remarks>
    /// This property identifies whether the activity is one of the standard system-defined
    /// activities from the ADMS.API.Entities.RevisionActivity seeded data or a potentially custom activity.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsStandardActivity)
    /// {
    ///     // Apply standard business rules
    ///     ProcessStandardActivity(activity);
    /// }
    /// </code>
    /// </example>
    public bool IsStandardActivity => RevisionActivityValidationHelper.IsActivityAllowed(Activity);

    /// <summary>
    /// Gets a value indicating whether this activity DTO has valid data for system operations.
    /// </summary>
    /// <remarks>
    /// This property provides a quick validation check without running full validation logic,
    /// useful for UI scenarios where immediate feedback is needed.
    /// 
    /// <para><strong>Validation Checks:</strong></para>
    /// <list type="bullet">
    /// <item>Activity ID is not empty</item>
    /// <item>Activity name passes comprehensive validation</item>
    /// <item>Activity is in the allowed activities list</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsValid)
    /// {
    ///     // Proceed with business operations
    ///     ProcessActivity(activity);
    /// }
    /// else
    /// {
    ///     // Show validation errors to user
    ///     DisplayValidationErrors(activity);
    /// }
    /// </code>
    /// </example>
    public bool IsValid =>
        Id != Guid.Empty &&
        RevisionActivityValidationHelper.IsValidActivity(Activity);

    /// <summary>
    /// Gets the display text suitable for UI controls and activity identification.
    /// </summary>
    /// <remarks>
    /// Provides a consistent format for displaying activity information in UI elements,
    /// using the activity name for clear identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new RevisionActivityMinimalDto { Activity = "CREATED" };
    /// var displayText = activity.DisplayText; // Returns "CREATED"
    /// 
    /// // UI usage
    /// activityDropdown.Items.Add(new ListItem(activity.DisplayText, activity.Id.ToString()));
    /// </code>
    /// </example>
    public string DisplayText => Activity;

    /// <summary>
    /// Gets a user-friendly description of the activity for display purposes.
    /// </summary>
    /// <remarks>
    /// Converts the technical activity name to a more user-friendly description
    /// suitable for audit logs and user interface display.
    /// </remarks>
    /// <example>
    /// <code>
    /// var description = activity.UserFriendlyDescription;
    /// // Returns: "Created", "Saved", "Deleted", "Restored"
    /// </code>
    /// </example>
    public string UserFriendlyDescription => Activity switch
    {
        "CREATED" => "Created",
        "SAVED" => "Saved",
        "DELETED" => "Deleted",
        "RESTORED" => "Restored",
        _ => Activity
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="RevisionActivityMinimalDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using the ADMS.API.Common.RevisionActivityValidationHelper for consistency 
    /// with entity validation rules. This ensures the DTO maintains the same validation standards as 
    /// the corresponding ADMS.API.Entities.RevisionActivity entity.
    /// 
    /// <para><strong>Validation Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>ID Validation:</strong> Ensures activity ID is a valid non-empty GUID</item>
    /// <item><strong>Activity Name Validation:</strong> Comprehensive activity name validation</item>
    /// <item><strong>Business Rules:</strong> Activity-specific business rule compliance</item>
    /// <item><strong>Format Validation:</strong> Character format and length constraints</item>
    /// </list>
    /// 
    /// <para><strong>Integration with ValidationHelper:</strong></para>
    /// Uses the centralized RevisionActivityValidationHelper to ensure consistency across all
    /// activity-related validation in the system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new RevisionActivityMinimalDto 
    /// { 
    ///     Id = Guid.Empty, // Invalid
    ///     Activity = "INVALID" // Invalid
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
        // Validate activity ID
        foreach (var result in ValidateActivityId())
            yield return result;

        // Validate activity name using centralized helper
        foreach (var result in ValidateActivityName())
            yield return result;
    }

    /// <summary>
    /// Validates the <see cref="Id"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the activity ID.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.RevisionActivityValidationHelper.ValidateActivityId for consistent validation
    /// across all activity-related DTOs and entities.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateActivityId()
    {
        return RevisionActivityValidationHelper.ValidateActivityId(Id, nameof(Id));
    }

    /// <summary>
    /// Validates the <see cref="Activity"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the activity name.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.RevisionActivityValidationHelper.ValidateActivity for comprehensive validation
    /// including format, length, allowed values, and reserved name checking.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateActivityName()
    {
        return RevisionActivityValidationHelper.ValidateActivity(Activity, nameof(Activity));
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="RevisionActivityMinimalDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The RevisionActivityMinimalDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate RevisionActivityMinimalDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new RevisionActivityMinimalDto 
    /// { 
    ///     Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
    ///     Activity = "CREATED"
    /// };
    /// 
    /// var results = RevisionActivityMinimalDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Activity validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] RevisionActivityMinimalDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("RevisionActivityMinimalDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a RevisionActivityMinimalDto from an ADMS.API.Entities.RevisionActivity entity with validation.
    /// </summary>
    /// <param name="revisionActivity">The RevisionActivity entity to convert. Cannot be null.</param>
    /// <returns>A valid RevisionActivityMinimalDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when revisionActivity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create RevisionActivityMinimalDto instances from
    /// ADMS.API.Entities.RevisionActivity entities with automatic validation. It ensures the resulting
    /// DTO is valid before returning it.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity
    /// var entity = new ADMS.API.Entities.RevisionActivity 
    /// { 
    ///     Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
    ///     Activity = "CREATED" 
    /// };
    /// 
    /// var dto = RevisionActivityMinimalDto.FromEntity(entity);
    /// // Returns validated RevisionActivityMinimalDto instance
    /// </code>
    /// </example>
    public static RevisionActivityMinimalDto FromEntity([NotNull] Entities.RevisionActivity revisionActivity)
    {
        ArgumentNullException.ThrowIfNull(revisionActivity, nameof(revisionActivity));

        var dto = new RevisionActivityMinimalDto
        {
            Id = revisionActivity.Id,
            Activity = revisionActivity.Activity
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid RevisionActivityMinimalDto from entity: {errorMessages}");

    }

    /// <summary>
    /// Creates a RevisionActivityMinimalDto for a specific standard activity type.
    /// </summary>
    /// <param name="activityType">The standard activity type (CREATED, SAVED, DELETED, RESTORED).</param>
    /// <returns>A RevisionActivityMinimalDto with the appropriate seeded ID and activity name.</returns>
    /// <exception cref="ArgumentException">Thrown when activityType is not a recognized standard activity.</exception>
    /// <remarks>
    /// This factory method creates RevisionActivityMinimalDto instances for standard activities
    /// using the correct seeded GUIDs from ADMS.API.Entities.RevisionActivity.
    /// </remarks>
    /// <example>
    /// <code>
    /// var createdActivity = RevisionActivityMinimalDto.ForStandardActivity("CREATED");
    /// var savedActivity = RevisionActivityMinimalDto.ForStandardActivity("SAVED");
    /// var deletedActivity = RevisionActivityMinimalDto.ForStandardActivity("DELETED");
    /// var restoredActivity = RevisionActivityMinimalDto.ForStandardActivity("RESTORED");
    /// </code>
    /// </example>
    public static RevisionActivityMinimalDto ForStandardActivity(string activityType)
    {
        if (string.IsNullOrWhiteSpace(activityType))
            throw new ArgumentException("Activity type cannot be null or empty.", nameof(activityType));

        var normalizedActivity = activityType.Trim().ToUpperInvariant();
        var activityId = normalizedActivity switch
        {
            "CREATED" => Guid.Parse("10000000-0000-0000-0000-000000000001"),
            "DELETED" => Guid.Parse("10000000-0000-0000-0000-000000000002"),
            "RESTORED" => Guid.Parse("10000000-0000-0000-0000-000000000003"),
            "SAVED" => Guid.Parse("10000000-0000-0000-0000-000000000004"),
            _ => throw new ArgumentException($"Unknown standard activity type: {activityType}", nameof(activityType))
        };

        return new RevisionActivityMinimalDto
        {
            Id = activityId,
            Activity = normalizedActivity
        };
    }

    /// <summary>
    /// Gets all standard revision activities as RevisionActivityMinimalDto instances.
    /// </summary>
    /// <returns>A collection of all standard revision activities.</returns>
    /// <remarks>
    /// This method returns all four standard revision activities (CREATED, SAVED, DELETED, RESTORED)
    /// with their correct seeded GUIDs, useful for populating dropdown lists and other UI controls.
    /// </remarks>
    /// <example>
    /// <code>
    /// var standardActivities = RevisionActivityMinimalDto.GetStandardActivities();
    /// 
    /// // Populate dropdown
    /// foreach (var activity in standardActivities)
    /// {
    ///     activityDropdown.Items.Add(new ListItem(activity.UserFriendlyDescription, activity.Id.ToString()));
    /// }
    /// </code>
    /// </example>
    public static IReadOnlyList<RevisionActivityMinimalDto> GetStandardActivities()
    {
        return new List<RevisionActivityMinimalDto>
        {
            ForStandardActivity("CREATED"),
            ForStandardActivity("DELETED"),
            ForStandardActivity("RESTORED"),
            ForStandardActivity("SAVED")
        }.AsReadOnly();
    }

    #endregion Static Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified RevisionActivityMinimalDto is equal to the current RevisionActivityMinimalDto.
    /// </summary>
    /// <param name="other">The RevisionActivityMinimalDto to compare with the current RevisionActivityMinimalDto.</param>
    /// <returns>true if the specified RevisionActivityMinimalDto is equal to the current RevisionActivityMinimalDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each activity has a unique identifier.
    /// This follows the same equality pattern as ADMS.API.Entities.RevisionActivity for consistency.
    /// </remarks>
    public virtual bool Equals(RevisionActivityMinimalDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current RevisionActivityMinimalDto.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the RevisionActivityMinimalDto.
    /// </summary>
    /// <returns>A string that represents the current RevisionActivityMinimalDto.</returns>
    /// <remarks>
    /// The string representation includes both the activity name and ID for identification
    /// purposes, following the same pattern as ADMS.API.Entities.RevisionActivity.ToString().
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new RevisionActivityMinimalDto 
    /// { 
    ///     Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
    ///     Activity = "CREATED" 
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "RevisionActivity: CREATED (10000000-0000-0000-0000-000000000001)"
    /// </code>
    /// </example>
    public override string ToString() => $"RevisionActivity: {Activity} ({Id})";

    #endregion String Representation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this activity is appropriate for the given revision context.
    /// </summary>
    /// <param name="revisionExists">Whether the revision already exists.</param>
    /// <param name="isDeleted">Whether the revision is currently deleted.</param>
    /// <returns>true if the activity is appropriate for the context; otherwise, false.</returns>
    /// <remarks>
    /// This method uses the ADMS.API.Common.RevisionActivityValidationHelper to validate business rules
    /// for activity context appropriateness, ensuring proper workflow enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// var createdActivity = RevisionActivityMinimalDto.ForStandardActivity("CREATED");
    /// 
    /// bool canApply = createdActivity.IsAppropriateForContext(false, false);  // true for new revision
    /// bool cannotApply = createdActivity.IsAppropriateForContext(true, false); // false for existing revision
    /// </code>
    /// </example>
    public bool IsAppropriateForContext(bool revisionExists, bool isDeleted) =>
        RevisionActivityValidationHelper.IsActivityAppropriateForContext(Activity, revisionExists, isDeleted);

    #endregion Business Logic Methods
}