using ADMS.API.Common;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Models;

/// <summary>
/// Minimal Data Transfer Object representing essential matter document activity information for efficient activity classification and audit trail operations.
/// </summary>
/// <remarks>
/// This DTO serves as a lightweight representation of matter document activities within the ADMS legal 
/// document management system, corresponding to <see cref="ADMS.API.Entities.MatterDocumentActivity"/>. 
/// It provides only essential properties required for activity classification, audit trail display, and 
/// document operation categorization while excluding detailed relationships for optimal performance.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Activity Representation:</strong> Essential properties for document activity classification without relationship overhead</item>
/// <item><strong>Performance Optimized:</strong> Excludes detailed relationships for fast activity enumeration and classification</item>
/// <item><strong>Professional Validation:</strong> Uses ADMS.API.Common.MatterDocumentActivityValidationHelper for comprehensive data integrity</item>
/// <item><strong>Immutable Design:</strong> Record type with init-only properties for thread-safe audit operations</item>
/// <item><strong>Display Ready:</strong> Pre-validated for immediate use in activity classification and audit displays</item>
/// </list>
/// 
/// <para><strong>Entity Alignment:</strong></para>
/// This DTO mirrors essential properties from ADMS.API.Entities.MatterDocumentActivity:
/// <list type="bullet">
/// <item><strong>Id:</strong> Unique identifier for the matter document activity</item>
/// <item><strong>Activity:</strong> Activity classification string for operation type identification</item>
/// <item><strong>Minimal Footprint:</strong> Excludes usage collections and detailed metadata for performance</item>
/// </list>
/// 
/// <para><strong>Supported Document Activity Types:</strong></para>
/// <list type="bullet">
/// <item><strong>SAVED:</strong> Document saved within matter context</item>
/// <item><strong>MOVED:</strong> Document moved between matters</item>
/// <item><strong>COPIED:</strong> Document copied between matters</item>
/// <item><strong>DELETED:</strong> Document deleted from matter</item>
/// <item><strong>RESTORED:</strong> Document restored to matter</item>
/// <item><strong>Extensible Framework:</strong> Support for additional activity types as practice requirements evolve</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Activity Classification:</strong> Document activity type identification and categorization</item>
/// <item><strong>Audit Trail Display:</strong> Lightweight activity information for audit trail presentations</item>
/// <item><strong>API Responses:</strong> Efficient activity data transfer in REST API responses</item>
/// <item><strong>Activity Selection:</strong> Activity type selection interfaces and dropdowns</item>
/// <item><strong>Performance-Critical Operations:</strong> Bulk activity processing with minimal memory footprint</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Activity Classification:</strong> Essential activity type classification for legal document management</item>
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
/// <item><strong>Use MatterDocumentActivityMinimalDto:</strong> For activity classification, selection lists, and performance-critical scenarios</item>
/// <item><strong>Use MatterDocumentActivityDto:</strong> For complete activity information with usage statistics and metadata</item>
/// <item><strong>Use MatterDocumentActivityUserDto:</strong> For activity tracking with user attribution and matter context</item>
/// </list>
/// 
/// <para><strong>Audit Trail Integration:</strong></para>
/// <list type="bullet">
/// <item><strong>Activity Identification:</strong> Provides essential activity type identification for audit trails</item>
/// <item><strong>Operation Classification:</strong> Enables clear categorization of document operations</item>
/// <item><strong>Professional Presentation:</strong> Supports professional presentation of activity information</item>
/// <item><strong>Compliance Documentation:</strong> Minimal data required for compliance and audit reporting</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a minimal document activity for classification
/// var activityMinimal = new MatterDocumentActivityMinimalDto
/// {
///     Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
///     Activity = "SAVED"
/// };
/// 
/// // Validation example
/// var validationResults = MatterDocumentActivityMinimalDto.ValidateModel(activityMinimal);
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
/// Console.WriteLine($"Is Transfer Operation: {activityMinimal.IsTransferActivity}");
/// </code>
/// </example>
public record MatterDocumentActivityMinimalDto : IValidatableObject, IEquatable<MatterDocumentActivityMinimalDto>
{
    #region Core Properties

    /// <summary>
    /// Gets the unique identifier for the matter document activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the matter document activity within the ADMS system.
    /// The ID corresponds directly to the <see cref="ADMS.API.Entities.MatterDocumentActivity.Id"/> property and is
    /// used for establishing relationships, activity references, and all system operations requiring precise activity
    /// identification.
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required Property:</strong> Always required for existing activity operations</item>
    /// <item><strong>Foreign Key Reference:</strong> Used as foreign key in all activity-related junction entities</item>
    /// <item><strong>Activity Classification:</strong> Links to specific activity types for document operations</item>
    /// <item><strong>API Operations:</strong> Primary identifier for REST API operations and activity references</item>
    /// <item><strong>Database Queries:</strong> Primary key for all activity-specific database operations</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivity.Id"/> exactly, ensuring consistency
    /// between entity and DTO representations for reliable activity identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing activity by ID
    /// var activityId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    /// var activity = activities.FirstOrDefault(a => a.Id == activityId);
    /// 
    /// // Using ID for activity references
    /// var userActivities = auditTrail.Where(a => a.MatterDocumentActivityId == activity.Id);
    /// 
    /// // API response with ID
    /// return Ok(new { Id = activity.Id, Activity = activity.Activity });
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter document activity ID is required.")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the activity classification string describing the type of document operation.
    /// </summary>
    /// <remarks>
    /// The activity string serves as the primary classification for document operations and must conform to 
    /// professional standards and validation rules. This field corresponds to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivity.Activity"/> and provides essential operation 
    /// type identification for audit trails and professional compliance.
    /// 
    /// <para><strong>Validation Rules (via ADMS.API.Common.MatterDocumentActivityValidationHelper):</strong></para>
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
    /// <item><strong>SAVED:</strong> Document saved within matter context - most common operation</item>
    /// <item><strong>MOVED:</strong> Document moved between matters - custody transfer operation</item>
    /// <item><strong>COPIED:</strong> Document copied between matters - duplication operation</item>
    /// <item><strong>DELETED:</strong> Document deleted from matter - removal operation</item>
    /// <item><strong>RESTORED:</strong> Document restored to matter - recovery operation</item>
    /// </list>
    /// 
    /// <para><strong>Professional Activity Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Lifecycle:</strong> Represents different stages of document lifecycle management</item>
    /// <item><strong>Professional Operations:</strong> Aligns with professional legal document handling practices</item>
    /// <item><strong>Audit Classification:</strong> Provides clear categorization for audit trail analysis</item>
    /// <item><strong>Client Communication:</strong> Supports professional client communication about document operations</item>
    /// </list>
    /// 
    /// <para><strong>Business Rule Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Operation Validation:</strong> Activity must be valid for specific document states</item>
    /// <item><strong>Professional Standards:</strong> Must align with professional document management standards</item>
    /// <item><strong>System Integration:</strong> Must integrate properly with audit trail and reporting systems</item>
    /// <item><strong>Future Extensibility:</strong> Framework supports addition of new activity types as needed</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivity.Activity"/> with identical validation
    /// rules and professional standards, ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional activity classifications
    /// var saveActivity = new MatterDocumentActivityMinimalDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     Activity = "SAVED" 
    /// };
    /// 
    /// var moveActivity = new MatterDocumentActivityMinimalDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     Activity = "MOVED" 
    /// };
    /// 
    /// // Activity validation
    /// var isValidActivity = MatterDocumentActivityValidationHelper.IsActivityAllowed(saveActivity.Activity);
    /// 
    /// // Professional display
    /// Console.WriteLine($"Document Operation: {saveActivity.DisplayActivity}");
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
    /// var activity1 = new MatterDocumentActivityMinimalDto { Activity = "saved" };
    /// var activity2 = new MatterDocumentActivityMinimalDto { Activity = "SAVED" };
    /// 
    /// // Both will have the same normalized activity: "SAVED"
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
    /// var activity = new MatterDocumentActivityMinimalDto { Activity = "SAVED" };
    /// 
    /// // Professional display
    /// Console.WriteLine($"Document Operation: {activity.DisplayActivity}");
    /// // Output: "Document Operation: Saved"
    /// </code>
    /// </example>
    public string DisplayActivity => Activity switch
    {
        "SAVED" => "Saved",
        "MOVED" => "Moved",
        "COPIED" => "Copied",
        "DELETED" => "Deleted",
        "RESTORED" => "Restored",
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
    /// var activity = new MatterDocumentActivityMinimalDto { Activity = "MOVED" };
    /// 
    /// Console.WriteLine($"Activity Category: {activity.ActivityType}");
    /// // Output: "Activity Category: Transfer"
    /// </code>
    /// </example>
    public string ActivityType => NormalizedActivity switch
    {
        "SAVED" => "Storage",
        "MOVED" or "COPIED" => "Transfer",
        "DELETED" => "Removal",
        "RESTORED" => "Recovery",
        _ => "Other"
    };

    /// <summary>
    /// Gets a value indicating whether this activity represents a transfer operation (MOVE or COPY).
    /// </summary>
    /// <remarks>
    /// This computed property helps identify transfer operations, which have different
    /// implications for document custody and availability tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsTransferActivity)
    /// {
    ///     Console.WriteLine("This activity involves document transfer between matters");
    /// }
    /// </code>
    /// </example>
    public bool IsTransferActivity => ActivityType == "Transfer";

    /// <summary>
    /// Gets a value indicating whether this activity represents a destructive operation (DELETE).
    /// </summary>
    /// <remarks>
    /// This computed property helps identify destructive operations that affect document
    /// availability and require special handling in audit trails.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsDestructiveActivity)
    /// {
    ///     Console.WriteLine("Warning: This activity affects document availability");
    /// }
    /// </code>
    /// </example>
    public bool IsDestructiveActivity => ActivityType == "Removal";

    /// <summary>
    /// Gets a value indicating whether this activity represents a recovery operation (RESTORE).
    /// </summary>
    /// <remarks>
    /// This computed property helps identify recovery operations that restore document
    /// availability and typically require administrative privileges.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsRecoveryActivity)
    /// {
    ///     Console.WriteLine("This activity restores document availability");
    /// }
    /// </code>
    /// </example>
    public bool IsRecoveryActivity => ActivityType == "Recovery";

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
    ///     ProcessActivity(activity);
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
        MatterDocumentActivityValidationHelper.IsActivityAllowed(Activity);

    /// <summary>
    /// Gets the display text suitable for UI controls and activity identification.
    /// </summary>
    /// <remarks>
    /// Provides a consistent format for displaying activity information in UI elements,
    /// optimized for professional presentation and user comprehension.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new MatterDocumentActivityMinimalDto 
    /// { 
    ///     Id = activityGuid,
    ///     Activity = "SAVED" 
    /// };
    /// var displayText = activity.DisplayText; // Returns "Saved"
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
            IsTransferActivity,
            IsDestructiveActivity,
            IsRecoveryActivity,
            IsValid
        },
        ValidationInfo = new
        {
            IsValid,
            HasValidId = Id != Guid.Empty,
            HasValidActivity = !string.IsNullOrWhiteSpace(Activity),
            IsAllowedActivity = MatterDocumentActivityValidationHelper.IsActivityAllowed(Activity ?? string.Empty)
        }
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="MatterDocumentActivityMinimalDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using the ADMS.API.Common.MatterDocumentActivityValidationHelper for consistency 
    /// with entity validation rules. This ensures the DTO maintains the same validation standards as the corresponding 
    /// ADMS.API.Entities.MatterDocumentActivity entity while enforcing minimal DTO-specific requirements.
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
    /// Uses centralized MatterDocumentActivityValidationHelper to ensure consistency across all activity
    /// validation in the system, maintaining professional standards and business rule compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityMinimalDto 
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
    /// Uses ADMS.API.Common.MatterDocumentActivityValidationHelper for comprehensive activity validation
    /// including allowed values, format requirements, and professional standards.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateActivity()
    {
        // Basic null/empty validation
        if (string.IsNullOrWhiteSpace(Activity))
        {
            yield return new ValidationResult(
                "Activity classification is required for document operation identification.",
                [nameof(Activity)]);
            yield break;
        }

        // Length validation
        if (Activity.Length > MatterDocumentActivityValidationHelper.MaxActivityLength)
            yield return new ValidationResult(
                $"Activity classification cannot exceed {MatterDocumentActivityValidationHelper.MaxActivityLength} characters.",
                [nameof(Activity)]);

        // Allowed activity validation
        if (!MatterDocumentActivityValidationHelper.IsActivityAllowed(Activity))
            yield return new ValidationResult(
                $"Activity '{Activity}' is not allowed. Allowed activities: {string.Join(", ", MatterDocumentActivityValidationHelper.AllowedActivitiesList)}",
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
    /// Validates a <see cref="MatterDocumentActivityMinimalDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The MatterDocumentActivityMinimalDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate MatterDocumentActivityMinimalDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance Validate method 
    /// but with null-safety and simplified usage.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityMinimalDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     Activity = "SAVED"
    /// };
    /// 
    /// var results = MatterDocumentActivityMinimalDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Activity validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterDocumentActivityMinimalDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterDocumentActivityMinimalDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a MatterDocumentActivityMinimalDto from ADMS.API.Entities.MatterDocumentActivity entity with validation.
    /// </summary>
    /// <param name="entity">The MatterDocumentActivity entity to convert. Cannot be null.</param>
    /// <returns>A valid MatterDocumentActivityMinimalDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create MatterDocumentActivityMinimalDto instances from
    /// ADMS.API.Entities.MatterDocumentActivity entities with automatic validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity
    /// var entity = new ADMS.API.Entities.MatterDocumentActivity 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     Activity = "SAVED"
    /// };
    /// 
    /// var dto = MatterDocumentActivityMinimalDto.FromEntity(entity);
    /// </code>
    /// </example>
    public static MatterDocumentActivityMinimalDto FromEntity([NotNull] Entities.MatterDocumentActivity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var dto = new MatterDocumentActivityMinimalDto
        {
            Id = entity.Id,
            Activity = entity.Activity
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterDocumentActivityMinimalDto from entity: {errorMessages}");

    }

    /// <summary>
    /// Creates multiple MatterDocumentActivityMinimalDto instances from a collection of entities.
    /// </summary>
    /// <param name="entities">The collection of MatterDocumentActivity entities to convert. Cannot be null.</param>
    /// <returns>A collection of valid MatterDocumentActivityMinimalDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method is optimized for creating multiple minimal activity DTOs efficiently.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convert collection of activity entities
    /// var entities = await context.MatterDocumentActivities.ToListAsync();
    /// var activityDtos = MatterDocumentActivityMinimalDto.FromEntities(entities);
    /// 
    /// // Use in dropdown or selection list
    /// foreach (var activityDto in activityDtos)
    /// {
    ///     dropdown.Items.Add(new ListItem(activityDto.DisplayText, activityDto.Id.ToString()));
    /// }
    /// </code>
    /// </example>
    public static IList<MatterDocumentActivityMinimalDto> FromEntities([NotNull] IEnumerable<Entities.MatterDocumentActivity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        var result = new List<MatterDocumentActivityMinimalDto>();

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
    /// <returns>A valid MatterDocumentActivityMinimalDto instance.</returns>
    /// <exception cref="ArgumentException">Thrown when activity is invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a convenient way to create activity DTOs with validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create new activity
    /// var saveActivity = MatterDocumentActivityMinimalDto.CreateActivity("SAVED");
    /// var moveActivity = MatterDocumentActivityMinimalDto.CreateActivity("MOVED", specificGuid);
    /// 
    /// // Use in system operations
    /// await activityService.RegisterActivityAsync(saveActivity);
    /// </code>
    /// </example>
    public static MatterDocumentActivityMinimalDto CreateActivity([NotNull] string activity, Guid? id = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activity, nameof(activity));

        var dto = new MatterDocumentActivityMinimalDto
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
    /// Gets all allowed activities as MatterDocumentActivityMinimalDto instances.
    /// </summary>
    /// <returns>A collection of MatterDocumentActivityMinimalDto instances for all allowed activities.</returns>
    /// <remarks>
    /// This method provides a convenient way to get all allowed activities as DTOs for
    /// UI population, validation, and system operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Populate activity selection dropdown
    /// var allowedActivities = MatterDocumentActivityMinimalDto.GetAllowedActivities();
    /// foreach (var activity in allowedActivities)
    /// {
    ///     activityDropdown.Items.Add(new ListItem(activity.DisplayText, activity.Id.ToString()));
    /// }
    /// </code>
    /// </example>
    public static IList<MatterDocumentActivityMinimalDto> GetAllowedActivities()
    {
        var result = new List<MatterDocumentActivityMinimalDto>();

        foreach (var activity in MatterDocumentActivityValidationHelper.AllowedActivitiesList)
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
    /// Determines whether this activity can be applied to documents in the specified state.
    /// </summary>
    /// <param name="isDeleted">Whether the document is currently deleted.</param>
    /// <param name="isCheckedOut">Whether the document is currently checked out.</param>
    /// <returns>true if the activity can be applied; otherwise, false.</returns>
    /// <remarks>
    /// This method validates business rules for activity application based on document state.
    /// </remarks>
    /// <example>
    /// <code>
    /// var saveActivity = MatterDocumentActivityMinimalDto.CreateActivity("SAVED");
    /// var deleteActivity = MatterDocumentActivityMinimalDto.CreateActivity("DELETED");
    /// 
    /// // Check if activities can be applied
    /// var canSaveDeleted = saveActivity.CanApplyToDocument(isDeleted: true, isCheckedOut: false); // false
    /// var canDeleteActive = deleteActivity.CanApplyToDocument(isDeleted: false, isCheckedOut: false); // true
    /// </code>
    /// </example>
    public bool CanApplyToDocument(bool isDeleted, bool isCheckedOut)
    {
        return NormalizedActivity switch
        {
            "SAVED" => !isDeleted, // Can't save deleted documents
            "MOVED" or "COPIED" => !isDeleted && !isCheckedOut, // Can't transfer deleted or checked out documents
            "DELETED" => !isDeleted, // Can't delete already deleted documents
            "RESTORED" => isDeleted, // Can only restore deleted documents
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
    /// var activity = MatterDocumentActivityMinimalDto.CreateActivity("MOVED");
    /// var description = activity.GetActivityDescription();
    /// // Returns: "Transfers document custody between matters, removing it from the source matter"
    /// </code>
    /// </example>
    public string GetActivityDescription()
    {
        return NormalizedActivity switch
        {
            "SAVED" => "Saves document within the matter context, creating or updating the document record",
            "MOVED" => "Transfers document custody between matters, removing it from the source matter",
            "COPIED" => "Creates a duplicate of the document in another matter while retaining the original",
            "DELETED" => "Removes the document from the matter, marking it as deleted but preserving audit history",
            "RESTORED" => "Restores a previously deleted document, making it available again in the matter",
            _ => $"Performs {DisplayActivity.ToLowerInvariant()} operation on the document"
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
    /// var saveActivity = MatterDocumentActivityMinimalDto.CreateActivity("SAVED");
    /// var compatibleActivities = saveActivity.GetCompatibleFollowUpActivities();
    /// // Returns: ["MOVED", "COPIED", "DELETED"]
    /// </code>
    /// </example>
    public IReadOnlyList<string> GetCompatibleFollowUpActivities()
    {
        return NormalizedActivity switch
        {
            "SAVED" => new[] { "MOVED", "COPIED", "DELETED" },
            "MOVED" => new[] { "RESTORED" }, // Can restore to original location
            "COPIED" => new[] { "DELETED" }, // Can delete the copy
            "DELETED" => new[] { "RESTORED" },
            "RESTORED" => new[] { "SAVED", "MOVED", "COPIED", "DELETED" },
            _ => Array.Empty<string>()
        };
    }

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterDocumentActivityMinimalDto is equal to the current MatterDocumentActivityMinimalDto.
    /// </summary>
    /// <param name="other">The MatterDocumentActivityMinimalDto to compare with the current MatterDocumentActivityMinimalDto.</param>
    /// <returns>true if the specified MatterDocumentActivityMinimalDto is equal to the current MatterDocumentActivityMinimalDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property when available, or Activity if IDs match.
    /// </remarks>
    public virtual bool Equals(MatterDocumentActivityMinimalDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterDocumentActivityMinimalDto.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterDocumentActivityMinimalDto.
    /// </summary>
    /// <returns>A string that represents the current MatterDocumentActivityMinimalDto.</returns>
    /// <remarks>
    /// The string representation includes key identifying information for logging and debugging purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityMinimalDto 
    /// { 
    ///     Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
    ///     Activity = "SAVED"
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "Activity: SAVED (30000000-0000-0000-0000-000000000001) - Storage"
    /// </code>
    /// </example>
    public override string ToString() => $"Activity: {Activity} ({Id}) - {ActivityType}";

    #endregion String Representation
}