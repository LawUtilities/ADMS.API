using ADMS.API.Common;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Models;

/// <summary>
/// Minimal Data Transfer Object representing essential matter activity information for efficient activity classification and matter operation identification.
/// </summary>
/// <remarks>
/// This DTO serves as a lightweight representation of matter activities within the ADMS legal 
/// document management system, corresponding to <see cref="ADMS.API.Entities.MatterActivity"/>. 
/// It provides only essential properties required for activity identification, operation classification, 
/// and matter activity selection while excluding detailed relationships for optimal performance.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Activity Representation:</strong> Essential properties for matter activity identification without relationship overhead</item>
/// <item><strong>Performance Optimized:</strong> Excludes detailed relationships for fast activity enumeration and selection</item>
/// <item><strong>Professional Validation:</strong> Uses ADMS.API.Common.MatterActivityValidationHelper for comprehensive data integrity</item>
/// <item><strong>Immutable Design:</strong> Record type with init-only properties for thread-safe operations</item>
/// <item><strong>Display Ready:</strong> Pre-validated for immediate use in activity selection interfaces and operation classification</item>
/// </list>
/// 
/// <para><strong>Entity Alignment:</strong></para>
/// This DTO mirrors essential properties from ADMS.API.Entities.MatterActivity:
/// <list type="bullet">
/// <item><strong>Id:</strong> Unique identifier for the matter activity</item>
/// <item><strong>Activity:</strong> Activity classification string for operation type identification</item>
/// <item><strong>Minimal Footprint:</strong> Excludes user collections and detailed metadata for performance</item>
/// </list>
/// 
/// <para><strong>Supported Matter Activity Types:</strong></para>
/// <list type="bullet">
/// <item><strong>CREATED:</strong> Matter created by user - foundational activity for new matters</item>
/// <item><strong>ARCHIVED:</strong> Matter archived by user - lifecycle management for inactive matters</item>
/// <item><strong>DELETED:</strong> Matter deleted by user - removal activity with audit preservation</item>
/// <item><strong>RESTORED:</strong> Matter restored by user - recovery activity for deleted matters</item>
/// <item><strong>UNARCHIVED:</strong> Matter unarchived by user - reactivation of archived matters</item>
/// <item><strong>VIEWED:</strong> Matter viewed by user - access tracking for audit and analytics</item>
/// <item><strong>Extensible Framework:</strong> Support for additional activity types as practice requirements evolve</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Activity Selection:</strong> Matter activity type identification and selection interfaces</item>
/// <item><strong>Operation Classification:</strong> Activity type classification for matter operations</item>
/// <item><strong>API Responses:</strong> Efficient activity data transfer in REST API responses</item>
/// <item><strong>Activity Dropdowns:</strong> Activity type selection dropdowns and UI controls</item>
/// <item><strong>Performance-Critical Operations:</strong> Bulk activity processing with minimal memory footprint</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter Activity Classification:</strong> Essential activity type classification for legal matter management</item>
/// <item><strong>Professional Standards:</strong> Maintains professional activity naming conventions and validation</item>
/// <item><strong>Compliance Support:</strong> Minimal data for regulatory compliance and audit requirements</item>
/// <item><strong>Practice Efficiency:</strong> Optimized for rapid activity classification and professional workflows</item>
/// </list>
/// 
/// <para><strong>Performance Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Memory Footprint:</strong> Only essential properties for activity identification</item>
/// <item><strong>Fast Serialization:</strong> Quick JSON serialization/deserialization for API operations</item>
/// <item><strong>Database Efficiency:</strong> Optimal for database projections and bulk activity classification</item>
/// <item><strong>UI Responsiveness:</strong> Fast loading in activity selection and classification interfaces</item>
/// </list>
/// 
/// <para><strong>When to Use vs Other Activity DTOs:</strong></para>
/// <list type="bullet">
/// <item><strong>Use MatterActivityMinimalDto:</strong> For activity selection, classification lists, and performance-critical scenarios</item>
/// <item><strong>Use MatterActivityDto:</strong> For complete activity information with usage statistics and user collections</item>
/// <item><strong>Use MatterActivityUserDto:</strong> For activity tracking with user attribution and matter context</item>
/// </list>
/// 
/// <para><strong>Activity Classification Integration:</strong></para>
/// <list type="bullet">
/// <item><strong>Activity Identification:</strong> Provides essential activity type identification for matter operations</item>
/// <item><strong>Operation Classification:</strong> Enables clear categorization of matter operations</item>
/// <item><strong>Professional Presentation:</strong> Supports professional presentation of activity information</item>
/// <item><strong>System Integration:</strong> Integrates with matter management and audit systems</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a minimal matter activity for classification
/// var activityMinimal = new MatterActivityMinimalDto
/// {
///     Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
///     Activity = "CREATED"
/// };
/// 
/// // Validation example
/// var validationResults = MatterActivityMinimalDto.ValidateModel(activityMinimal);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Activity Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Professional activity classification
/// Console.WriteLine($"Activity Classification: {activityMinimal.DisplayActivity}");
/// Console.WriteLine($"Activity Type: {activityMinimal.ActivityType}");
/// Console.WriteLine($"Is Lifecycle Operation: {activityMinimal.IsLifecycleActivity}");
/// </code>
/// </example>
public record MatterActivityMinimalDto : IValidatableObject, IEquatable<MatterActivityMinimalDto>
{
    #region Core Properties

    /// <summary>
    /// Gets the unique identifier for the matter activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the matter activity within the ADMS system.
    /// The ID corresponds directly to the <see cref="ADMS.API.Entities.MatterActivity.Id"/> property and is
    /// used for establishing relationships, activity references, and all system operations requiring precise activity
    /// identification.
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required Property:</strong> Always required for existing activity operations</item>
    /// <item><strong>Foreign Key Reference:</strong> Used as foreign key in all activity-related junction entities</item>
    /// <item><strong>Activity Classification:</strong> Links to specific activity types for matter operations</item>
    /// <item><strong>API Operations:</strong> Primary identifier for REST API operations and activity references</item>
    /// <item><strong>Database Queries:</strong> Primary key for all activity-specific database operations</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterActivity.Id"/> exactly, ensuring consistency
    /// between entity and DTO representations for reliable activity identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard seeded activity IDs
    /// var createdActivityId = Guid.Parse("20000000-0000-0000-0000-000000000001"); // CREATED
    /// var archivedActivityId = Guid.Parse("20000000-0000-0000-0000-000000000002"); // ARCHIVED
    /// 
    /// // Using ID for activity references
    /// var userActivities = auditTrail.Where(a => a.MatterActivityId == createdActivityId);
    /// 
    /// // API response with ID
    /// return Ok(new { Id = activity.Id, Activity = activity.Activity });
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter activity ID is required.")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the activity classification string describing the type of matter operation.
    /// </summary>
    /// <remarks>
    /// The activity string serves as the primary classification for matter operations and must conform to 
    /// professional standards and validation rules. This field corresponds to 
    /// <see cref="ADMS.API.Entities.MatterActivity.Activity"/> and provides essential operation 
    /// type identification for audit trails and professional compliance.
    /// 
    /// <para><strong>Validation Rules (via ADMS.API.Common.MatterActivityValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null, empty, or whitespace</item>
    /// <item><strong>Length:</strong> Must not exceed maximum activity length constraint</item>
    /// <item><strong>Allowed Values:</strong> Must be one of the predefined allowed activity types</item>
    /// <item><strong>Format:</strong> Must contain at least one letter for professional readability</item>
    /// <item><strong>Professional Standards:</strong> Must follow professional activity naming conventions</item>
    /// </list>
    /// 
    /// <para><strong>Supported Activity Classifications:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Matter created by user - foundational activity for new matters</item>
    /// <item><strong>ARCHIVED:</strong> Matter archived by user - lifecycle management for inactive matters</item>
    /// <item><strong>DELETED:</strong> Matter deleted by user - removal activity with audit preservation</item>
    /// <item><strong>RESTORED:</strong> Matter restored by user - recovery activity for deleted matters</item>
    /// <item><strong>UNARCHIVED:</strong> Matter unarchived by user - reactivation of archived matters</item>
    /// <item><strong>VIEWED:</strong> Matter viewed by user - access tracking for audit and analytics</item>
    /// </list>
    /// 
    /// <para><strong>Professional Activity Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Matter Lifecycle:</strong> Represents different stages of matter lifecycle management</item>
    /// <item><strong>Professional Operations:</strong> Aligns with professional legal matter handling practices</item>
    /// <item><strong>Audit Classification:</strong> Provides clear categorization for audit trail analysis</item>
    /// <item><strong>Client Communication:</strong> Supports professional client communication about matter operations</item>
    /// </list>
    /// 
    /// <para><strong>Business Rule Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Operation Validation:</strong> Activity must be valid for specific matter states</item>
    /// <item><strong>Professional Standards:</strong> Must align with professional matter management standards</item>
    /// <item><strong>System Integration:</strong> Must integrate properly with audit trail and reporting systems</item>
    /// <item><strong>Future Extensibility:</strong> Framework supports addition of new activity types as needed</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterActivity.Activity"/> with identical validation
    /// rules and professional standards, ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional activity classifications
    /// var createdActivity = new MatterActivityMinimalDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     Activity = "CREATED" 
    /// };
    /// 
    /// var archivedActivity = new MatterActivityMinimalDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     Activity = "ARCHIVED" 
    /// };
    /// 
    /// // Activity validation
    /// var isValidActivity = MatterActivityValidationHelper.IsActivityAllowed(createdActivity.Activity);
    /// 
    /// // Professional display
    /// Console.WriteLine($"Matter Operation: {createdActivity.DisplayActivity}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Activity classification is required.")]
    [StringLength(50, MinimumLength = 1,
        ErrorMessage = "Activity must be between 1 and 50 characters.")]
    public required string Activity { get; init; }

    #endregion Core Properties

    #region Computed Properties

    /// <summary>
    /// Gets the normalized activity string for consistent comparison and analysis.
    /// </summary>
    /// <remarks>
    /// This computed property provides a normalized version of the activity string using
    /// consistent formatting rules for comparison, analysis, and professional presentation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity1 = new MatterActivityMinimalDto { Activity = "created" };
    /// var activity2 = new MatterActivityMinimalDto { Activity = "CREATED" };
    /// 
    /// // Both will have the same normalized activity: "CREATED"
    /// bool areEquivalent = activity1.NormalizedActivity == activity2.NormalizedActivity; // true
    /// </code>
    /// </example>
    public string NormalizedActivity => Activity.Trim().ToUpperInvariant();

    /// <summary>
    /// Gets the professional display representation of the activity for client communication and UI display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a user-friendly formatted representation of the activity
    /// optimized for professional interfaces, client communications, and audit trail presentation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new MatterActivityMinimalDto { Activity = "CREATED" };
    /// 
    /// // Professional display
    /// Console.WriteLine($"Matter Operation: {activity.DisplayActivity}");
    /// // Output: "Matter Operation: Created"
    /// </code>
    /// </example>
    public string DisplayActivity => Activity switch
    {
        "CREATED" => "Created",
        "ARCHIVED" => "Archived",
        "DELETED" => "Deleted",
        "RESTORED" => "Restored",
        "UNARCHIVED" => "Unarchived",
        "VIEWED" => "Viewed",
        _ => Activity.ToLowerInvariant().Trim() switch
        {
            var act when !string.IsNullOrEmpty(act) => char.ToUpperInvariant(act[0]) + act[1..],
            _ => Activity
        }
    };

    /// <summary>
    /// Gets the activity type category for classification and analysis purposes.
    /// </summary>
    /// <remarks>
    /// This computed property categorizes activities into logical groups for analysis,
    /// reporting, and business rule application.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new MatterActivityMinimalDto { Activity = "ARCHIVED" };
    /// 
    /// Console.WriteLine($"Activity Category: {activity.ActivityType}");
    /// // Output: "Activity Category: Lifecycle"
    /// </code>
    /// </example>
    public string ActivityType => NormalizedActivity switch
    {
        "CREATED" => "Creation",
        "ARCHIVED" or "DELETED" or "RESTORED" or "UNARCHIVED" => "Lifecycle",
        "VIEWED" => "Access",
        _ => "Other"
    };

    /// <summary>
    /// Gets a value indicating whether this activity represents a lifecycle operation.
    /// </summary>
    /// <remarks>
    /// This computed property helps identify lifecycle operations, which affect matter state
    /// and require special handling in audit trails and business logic.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsLifecycleActivity)
    /// {
    ///     Console.WriteLine("This activity affects matter lifecycle state");
    /// }
    /// </code>
    /// </example>
    public bool IsLifecycleActivity => ActivityType == "Lifecycle";

    /// <summary>
    /// Gets a value indicating whether this activity represents a creation operation.
    /// </summary>
    /// <remarks>
    /// This computed property helps identify creation operations that establish new matters
    /// and require special handling for matter initialization.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsCreationActivity)
    /// {
    ///     Console.WriteLine("This activity creates a new matter");
    /// }
    /// </code>
    /// </example>
    public bool IsCreationActivity => ActivityType == "Creation";

    /// <summary>
    /// Gets a value indicating whether this activity represents an access operation.
    /// </summary>
    /// <remarks>
    /// This computed property helps identify access operations used for tracking
    /// matter viewing and access analytics.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsAccessActivity)
    /// {
    ///     Console.WriteLine("This activity tracks matter access");
    /// }
    /// </code>
    /// </example>
    public bool IsAccessActivity => ActivityType == "Access";

    /// <summary>
    /// Gets a value indicating whether this activity DTO has valid data for system operations.
    /// </summary>
    /// <remarks>
    /// This property provides a quick validation check without running full validation logic,
    /// useful for UI scenarios where immediate feedback is needed before processing activity operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsValid)
    /// {
    ///     // Proceed with activity processing
    ///     ProcessMatterActivity(activity);
    /// }
    /// else
    /// {
    ///     // Show validation errors to user
    ///     DisplayActivityValidationErrors(activity);
    /// }
    /// </code>
    /// </example>
    public bool IsValid =>
        Id != Guid.Empty &&
        !string.IsNullOrWhiteSpace(Activity) &&
        MatterActivityValidationHelper.IsActivityAllowed(Activity);

    /// <summary>
    /// Gets the display text suitable for UI controls and activity identification.
    /// </summary>
    /// <remarks>
    /// Provides a consistent format for displaying activity information in UI elements,
    /// optimized for professional presentation and user comprehension.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new MatterActivityMinimalDto 
    /// { 
    ///     Id = activityGuid,
    ///     Activity = "CREATED" 
    /// };
    /// var displayText = activity.DisplayText; // Returns "Created"
    /// 
    /// // UI usage
    /// activityDropdown.Items.Add(new ListItem(activity.DisplayText, activity.Id.ToString()));
    /// </code>
    /// </example>
    public string DisplayText => DisplayActivity;

    /// <summary>
    /// Gets comprehensive activity metrics for analysis and reporting.
    /// </summary>
    /// <remarks>
    /// This property provides a structured object containing key metrics and information
    /// for comprehensive activity analysis and professional reporting purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var metrics = activity.ActivityMetrics;
    /// // Access comprehensive activity metrics for analysis
    /// </code>
    /// </example>
    public object ActivityMetrics => new
    {
        ActivityInfo = new
        {
            Id,
            Activity,
            NormalizedActivity,
            DisplayActivity,
            ActivityType,
            DisplayText
        },
        ClassificationInfo = new
        {
            IsLifecycleActivity,
            IsCreationActivity,
            IsAccessActivity,
            IsValid
        },
        ValidationInfo = new
        {
            IsValid,
            HasValidId = Id != Guid.Empty,
            HasValidActivity = !string.IsNullOrWhiteSpace(Activity),
            IsAllowedActivity = MatterActivityValidationHelper.IsActivityAllowed(Activity ?? string.Empty)
        }
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="MatterActivityMinimalDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using the ADMS.API.Common.MatterActivityValidationHelper for consistency 
    /// with entity validation rules. This ensures the DTO maintains the same validation standards as the corresponding 
    /// ADMS.API.Entities.MatterActivity entity while enforcing minimal DTO-specific requirements.
    /// 
    /// <para><strong>Minimal Validation Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>ID Validation:</strong> Ensures GUID is valid and non-empty</item>
    /// <item><strong>Activity Validation:</strong> Comprehensive activity string validation using centralized helper</item>
    /// <item><strong>Professional Standards:</strong> Enforces professional activity naming and classification standards</item>
    /// <item><strong>Business Rule Compliance:</strong> Validates against allowed activity types and constraints</item>
    /// </list>
    /// 
    /// <para><strong>Professional Standards Integration:</strong></para>
    /// Uses centralized MatterActivityValidationHelper to ensure consistency across all activity
    /// validation in the system, maintaining professional standards and business rule compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterActivityMinimalDto 
    /// { 
    ///     Id = Guid.Empty, // Invalid
    ///     Activity = "INVALID_ACTIVITY" // Invalid
    /// };
    /// 
    /// var context = new ValidationContext(dto);
    /// var results = dto.Validate(context);
    /// 
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"Activity Validation Error: {result.ErrorMessage}");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate activity ID
        foreach (var result in ValidateId())
            yield return result;

        // Validate activity classification using centralized helper
        foreach (var result in ValidateActivity())
            yield return result;
    }

    /// <summary>
    /// Validates the <see cref="Id"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the activity ID.</returns>
    /// <remarks>
    /// Ensures the activity ID is a valid, non-empty GUID which is essential for activity identification
    /// and referential integrity.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateId()
    {
        if (Id == Guid.Empty)
            yield return new ValidationResult(
                "Activity ID must be a valid non-empty GUID for activity identification.",
                [nameof(Id)]);
    }

    /// <summary>
    /// Validates the <see cref="Activity"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the activity classification.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.MatterActivityValidationHelper for comprehensive activity validation
    /// including allowed values, format requirements, and professional standards.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateActivity()
    {
        // Basic null/empty validation
        if (string.IsNullOrWhiteSpace(Activity))
        {
            yield return new ValidationResult(
                "Activity classification is required for matter operation identification.",
                [nameof(Activity)]);
            yield break;
        }

        // Length validation
        if (Activity.Length > MatterActivityValidationHelper.MaxActivityLength)
            yield return new ValidationResult(
                $"Activity classification cannot exceed {MatterActivityValidationHelper.MaxActivityLength} characters.",
                [nameof(Activity)]);

        // Allowed activity validation
        if (!MatterActivityValidationHelper.IsActivityAllowed(Activity))
            yield return new ValidationResult(
                $"Activity '{Activity}' is not allowed. Allowed activities: {string.Join(", ", MatterActivityValidationHelper.AllowedActivitiesList)}",
                [nameof(Activity)]);

        // Professional format validation
        if (!Activity.Any(char.IsLetter))
            yield return new ValidationResult(
                "Activity classification must contain at least one letter for professional readability.",
                [nameof(Activity)]);
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="MatterActivityMinimalDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The MatterActivityMinimalDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate MatterActivityMinimalDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance Validate method 
    /// but with null-safety and simplified usage.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterActivityMinimalDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     Activity = "CREATED"
    /// };
    /// 
    /// var results = MatterActivityMinimalDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Activity validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterActivityMinimalDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterActivityMinimalDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a MatterActivityMinimalDto from ADMS.API.Entities.MatterActivity entity with validation.
    /// </summary>
    /// <param name="entity">The MatterActivity entity to convert. Cannot be null.</param>
    /// <returns>A valid MatterActivityMinimalDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create MatterActivityMinimalDto instances from
    /// ADMS.API.Entities.MatterActivity entities with automatic validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity
    /// var entity = new ADMS.API.Entities.MatterActivity 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     Activity = "CREATED"
    /// };
    /// 
    /// var dto = MatterActivityMinimalDto.FromEntity(entity);
    /// </code>
    /// </example>
    public static MatterActivityMinimalDto FromEntity([NotNull] Entities.MatterActivity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var dto = new MatterActivityMinimalDto
        {
            Id = entity.Id,
            Activity = entity.Activity
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterActivityMinimalDto from entity: {errorMessages}");

    }

    /// <summary>
    /// Creates multiple MatterActivityMinimalDto instances from a collection of entities.
    /// </summary>
    /// <param name="entities">The collection of MatterActivity entities to convert. Cannot be null.</param>
    /// <returns>A collection of valid MatterActivityMinimalDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method is optimized for creating multiple minimal activity DTOs efficiently.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convert collection of activity entities
    /// var entities = await context.MatterActivities.ToListAsync();
    /// var activityDtos = MatterActivityMinimalDto.FromEntities(entities);
    /// 
    /// // Use in dropdown or selection list
    /// foreach (var activityDto in activityDtos)
    /// {
    ///     dropdown.Items.Add(new ListItem(activityDto.DisplayText, activityDto.Id.ToString()));
    /// }
    /// </code>
    /// </example>
    public static IList<MatterActivityMinimalDto> FromEntities([NotNull] IEnumerable<Entities.MatterActivity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        var result = new List<MatterActivityMinimalDto>();

        foreach (var entity in entities)
        {
            try
            {
                var dto = FromEntity(entity);
                result.Add(dto);
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                // Log invalid entity but continue processing others
                Console.WriteLine($"Warning: Skipped invalid activity entity: {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a new activity with specified parameters and comprehensive validation.
    /// </summary>
    /// <param name="activity">The activity classification string.</param>
    /// <param name="id">Optional ID (generates new GUID if not provided).</param>
    /// <returns>A valid MatterActivityMinimalDto instance.</returns>
    /// <exception cref="ArgumentException">Thrown when activity is invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a convenient way to create activity DTOs with validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create new activity
    /// var createdActivity = MatterActivityMinimalDto.CreateActivity("CREATED");
    /// var archivedActivity = MatterActivityMinimalDto.CreateActivity("ARCHIVED", specificGuid);
    /// 
    /// // Use in system operations
    /// await activityService.RegisterActivityAsync(createdActivity);
    /// </code>
    /// </example>
    public static MatterActivityMinimalDto CreateActivity([NotNull] string activity, Guid? id = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activity, nameof(activity));

        var dto = new MatterActivityMinimalDto
        {
            Id = id ?? Guid.NewGuid(),
            Activity = activity.Trim().ToUpperInvariant()
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid activity: {errorMessages}");

    }

    /// <summary>
    /// Gets all allowed activities as MatterActivityMinimalDto instances.
    /// </summary>
    /// <returns>A collection of MatterActivityMinimalDto instances for all allowed activities.</returns>
    /// <remarks>
    /// This method provides a convenient way to get all allowed activities as DTOs for
    /// UI population, validation, and system operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Populate activity selection dropdown
    /// var allowedActivities = MatterActivityMinimalDto.GetAllowedActivities();
    /// foreach (var activity in allowedActivities)
    /// {
    ///     activityDropdown.Items.Add(new ListItem(activity.DisplayText, activity.Id.ToString()));
    /// }
    /// </code>
    /// </example>
    public static IList<MatterActivityMinimalDto> GetAllowedActivities()
    {
        var result = new List<MatterActivityMinimalDto>();

        foreach (var activity in MatterActivityValidationHelper.AllowedActivitiesList)
        {
            try
            {
                var dto = CreateActivity(activity.ToString());
                result.Add(dto);
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                // Log invalid activity but continue processing others
                Console.WriteLine($"Warning: Skipped invalid allowed activity '{activity}': {ex.Message}");
            }
        }

        return result;
    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this activity can be applied to matters in the specified state.
    /// </summary>
    /// <param name="isArchived">Whether the matter is currently archived.</param>
    /// <param name="isDeleted">Whether the matter is currently deleted.</param>
    /// <returns>true if the activity can be applied; otherwise, false.</returns>
    /// <remarks>
    /// This method validates business rules for activity application based on matter state.
    /// </remarks>
    /// <example>
    /// <code>
    /// var archivedActivity = MatterActivityMinimalDto.CreateActivity("ARCHIVED");
    /// var deleteActivity = MatterActivityMinimalDto.CreateActivity("DELETED");
    /// 
    /// // Check if activities can be applied
    /// var canArchiveDeleted = archivedActivity.CanApplyToMatter(isArchived: false, isDeleted: true); // false
    /// var canDeleteActive = deleteActivity.CanApplyToMatter(isArchived: false, isDeleted: false); // true
    /// </code>
    /// </example>
    public bool CanApplyToMatter(bool isArchived, bool isDeleted)
    {
        return NormalizedActivity switch
        {
            "CREATED" => !isDeleted, // Can't create if already exists and not deleted
            "ARCHIVED" => !isArchived && !isDeleted, // Can't archive if already archived or deleted
            "UNARCHIVED" => isArchived && !isDeleted, // Can only unarchive archived matters that aren't deleted
            "DELETED" => !isDeleted, // Can't delete already deleted matters
            "RESTORED" => isDeleted, // Can only restore deleted matters
            "VIEWED" => !isDeleted, // Can't view deleted matters
            _ => true // Unknown activities allowed by default
        };
    }

    /// <summary>
    /// Gets the professional description of what this activity does.
    /// </summary>
    /// <returns>A professional description of the activity operation.</returns>
    /// <remarks>
    /// This method provides human-readable descriptions for client communication and documentation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = MatterActivityMinimalDto.CreateActivity("ARCHIVED");
    /// var description = activity.GetActivityDescription();
    /// // Returns: "Archives the matter, making it inactive while preserving all data and audit history"
    /// </code>
    /// </example>
    public string GetActivityDescription()
    {
        return NormalizedActivity switch
        {
            "CREATED" => "Creates a new matter, establishing it in the system for client work and document management",
            "ARCHIVED" => "Archives the matter, making it inactive while preserving all data and audit history",
            "DELETED" => "Deletes the matter, marking it as removed but preserving audit history for compliance",
            "RESTORED" => "Restores a previously deleted matter, making it available again for active use",
            "UNARCHIVED" => "Unarchives the matter, reactivating it for continued client work and operations",
            "VIEWED" => "Records matter access for analytics, compliance, and professional accountability tracking",
            _ => $"Performs {DisplayActivity.ToLowerInvariant()} operation on the matter"
        };
    }

    /// <summary>
    /// Gets activities that are compatible with this activity for sequential operations.
    /// </summary>
    /// <returns>A collection of activity names that can follow this activity.</returns>
    /// <remarks>
    /// This method helps identify valid activity sequences for workflow validation and UI logic.
    /// </remarks>
    /// <example>
    /// <code>
    /// var createdActivity = MatterActivityMinimalDto.CreateActivity("CREATED");
    /// var compatibleActivities = createdActivity.GetCompatibleFollowUpActivities();
    /// // Returns: ["ARCHIVED", "DELETED", "VIEWED"]
    /// </code>
    /// </example>
    public IReadOnlyList<string> GetCompatibleFollowUpActivities()
    {
        return NormalizedActivity switch
        {
            "CREATED" => new[] { "ARCHIVED", "DELETED", "VIEWED" },
            "ARCHIVED" => new[] { "UNARCHIVED", "DELETED", "VIEWED" },
            "UNARCHIVED" => new[] { "ARCHIVED", "DELETED", "VIEWED" },
            "DELETED" => new[] { "RESTORED" },
            "RESTORED" => new[] { "ARCHIVED", "DELETED", "VIEWED" },
            "VIEWED" => new[] { "ARCHIVED", "DELETED" }, // Most operations possible after viewing
            _ => Array.Empty<string>()
        };
    }

    /// <summary>
    /// Gets the matter state implications of performing this activity.
    /// </summary>
    /// <returns>A description of how this activity affects matter state.</returns>
    /// <remarks>
    /// This method describes the state changes that occur when this activity is performed,
    /// useful for UI feedback and workflow management.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = MatterActivityMinimalDto.CreateActivity("ARCHIVED");
    /// var stateImplication = activity.GetStateImplication();
    /// // Returns: "Matter becomes inactive (archived)"
    /// </code>
    /// </example>
    public string GetStateImplication()
    {
        return NormalizedActivity switch
        {
            "CREATED" => "Matter becomes active and available for use",
            "ARCHIVED" => "Matter becomes inactive (archived)",
            "DELETED" => "Matter becomes deleted (audit preserved)",
            "RESTORED" => "Matter becomes active again",
            "UNARCHIVED" => "Matter becomes active (unarchived)",
            "VIEWED" => "No state change (access tracked)",
            _ => "State change depends on specific activity implementation"
        };
    }

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterActivityMinimalDto is equal to the current MatterActivityMinimalDto.
    /// </summary>
    /// <param name="other">The MatterActivityMinimalDto to compare with the current MatterActivityMinimalDto.</param>
    /// <returns>true if the specified MatterActivityMinimalDto is equal to the current MatterActivityMinimalDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property when available, or Activity if IDs match.
    /// </remarks>
    public virtual bool Equals(MatterActivityMinimalDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterActivityMinimalDto.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterActivityMinimalDto.
    /// </summary>
    /// <returns>A string that represents the current MatterActivityMinimalDto.</returns>
    /// <remarks>
    /// The string representation includes key identifying information for logging and debugging purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterActivityMinimalDto 
    /// { 
    ///     Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
    ///     Activity = "CREATED"
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "Matter Activity: CREATED (20000000-0000-0000-0000-000000000001) - Creation"
    /// </code>
    /// </example>
    public override string ToString() => $"Matter Activity: {Activity} ({Id}) - {ActivityType}";

    #endregion String Representation
}