using ADMS.API.Common;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Models;

/// <summary>
/// Comprehensive Data Transfer Object representing a matter activity with complete user associations and validation logic.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of matter activities within the ADMS legal document management system,
/// corresponding to <see cref="ADMS.API.Entities.MatterActivity"/>. It provides comprehensive activity data including
/// complete user association collections for audit trail management and professional accountability.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Activity Representation:</strong> Full representation of matter activity with all relationships</item>
/// <item><strong>User Association Management:</strong> Comprehensive user association collection for audit trails</item>
/// <item><strong>Professional Validation:</strong> Uses centralized MatterActivityValidationHelper for data integrity</item>
/// <item><strong>Entity Synchronization:</strong> Mirrors all properties and relationships from ADMS.API.Entities.MatterActivity</item>
/// <item><strong>Legal Compliance Support:</strong> Designed for comprehensive audit reporting and legal compliance</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO represents the complete structure from ADMS.API.Entities.MatterActivity:
/// <list type="bullet">
/// <item><strong>Activity Classification:</strong> ID and Activity description for operation identification</item>
/// <item><strong>User Associations:</strong> Complete MatterActivityUsers collection for audit trail integrity</item>
/// <item><strong>Business Logic:</strong> Computed properties for activity analysis and categorization</item>
/// </list>
/// 
/// <para><strong>Supported Matter Activities:</strong></para>
/// <list type="bullet">
/// <item><strong>CREATED:</strong> Matter creation activity - foundational activity for new matters</item>
/// <item><strong>ARCHIVED:</strong> Matter archival activity - lifecycle management for inactive matters</item>
/// <item><strong>DELETED:</strong> Matter deletion activity - removal activity with audit preservation</item>
/// <item><strong>RESTORED:</strong> Matter restoration activity - recovery activity for deleted matters</item>
/// <item><strong>UNARCHIVED:</strong> Matter unarchival activity - reactivation of archived matters</item>
/// <item><strong>VIEWED:</strong> Matter viewing activity - access tracking for audit and analytics</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>Activity Classification:</strong> Clear categorization of matter operations for professional management</item>
/// <item><strong>Professional Accountability:</strong> Complete user attribution for matter activity operations</item>
/// <item><strong>Client Communication:</strong> Enables precise client communication about matter operations</item>
/// <item><strong>Legal Discovery:</strong> Important for understanding matter lifecycle and status changes</item>
/// <item><strong>Compliance Reporting:</strong> Supports detailed compliance reporting requirements</item>
/// </list>
/// 
/// <para><strong>Data Integrity and Validation:</strong></para>
/// <list type="bullet">
/// <item><strong>Activity Validation:</strong> Comprehensive activity name validation using centralized helpers</item>
/// <item><strong>User Association Validation:</strong> Deep validation of user association collections</item>
/// <item><strong>Business Rule Compliance:</strong> Enforces professional standards and business rule compliance</item>
/// <item><strong>Entity Completeness:</strong> Ensures all critical activity properties are validated</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Activity Management:</strong> Primary use for managing matter activity types and classifications</item>
/// <item><strong>Audit Trail Configuration:</strong> Configuration of available activities for audit trail systems</item>
/// <item><strong>Professional Reporting:</strong> Activity-based reporting and analysis for practice management</item>
/// <item><strong>User Interface:</strong> Activity selection and display in matter management interfaces</item>
/// <item><strong>API Operations:</strong> Complete activity data transfer in REST API operations</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive matter activity DTO
/// var activityDto = new MatterActivityDto
/// {
///     Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
///     Activity = "CREATED",
///     MatterActivityUsers = new List&lt;MatterActivityUserDto&gt;
///     {
///         new MatterActivityUserDto 
///         { 
///             MatterId = matterGuid,
///             MatterActivityId = activityGuid,
///             UserId = userGuid,
///             CreatedAt = DateTime.UtcNow
///         }
///     }
/// };
/// 
/// // Comprehensive validation
/// var validationResults = MatterActivityDto.ValidateModel(activityDto);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Activity Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Professional activity analysis
/// Console.WriteLine($"Activity '{activityDto.Activity}' has {activityDto.UsageCount} user associations");
/// Console.WriteLine($"Activity category: {activityDto.ActivityCategory}");
/// </code>
/// </example>
public class MatterActivityDto : IValidatableObject, IEquatable<MatterActivityDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the matter activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the matter activity within the ADMS system.
    /// It corresponds directly to <see cref="ADMS.API.Entities.MatterActivity.Id"/> and is used for 
    /// establishing relationships, audit trail associations, and all system operations requiring 
    /// precise activity identification.
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required for Updates:</strong> Must be provided when updating existing activities</item>
    /// <item><strong>Foreign Key Reference:</strong> Referenced in MatterActivityUser entities</item>
    /// <item><strong>Seeded Values:</strong> Standard activities use specific seeded GUIDs for consistency</item>
    /// <item><strong>API Operations:</strong> Activity identification in REST API operations</item>
    /// </list>
    /// 
    /// <para><strong>Standard Activity IDs:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> 20000000-0000-0000-0000-000000000001</item>
    /// <item><strong>ARCHIVED:</strong> 20000000-0000-0000-0000-000000000002</item>
    /// <item><strong>DELETED:</strong> 20000000-0000-0000-0000-000000000003</item>
    /// <item><strong>RESTORED:</strong> 20000000-0000-0000-0000-000000000004</item>
    /// <item><strong>UNARCHIVED:</strong> 20000000-0000-0000-0000-000000000005</item>
    /// <item><strong>VIEWED:</strong> 20000000-0000-0000-0000-000000000006</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using seeded activity ID
    /// var createdActivity = new MatterActivityDto 
    /// { 
    ///     Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
    ///     Activity = "CREATED"
    /// };
    /// 
    /// // For new custom activities, leave empty for auto-generation
    /// var customActivity = new MatterActivityDto 
    /// { 
    ///     Id = Guid.Empty, // Will be generated by system
    ///     Activity = "CUSTOM_OPERATION"
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Activity ID is required for activity identification.")]
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the activity being undertaken.
    /// </summary>
    /// <remarks>
    /// The activity name serves as the primary classifier and human-readable identifier for matter operations.
    /// This field must conform to the standardized activity names defined in MatterActivityValidationHelper.AllowedActivities
    /// to ensure consistency across the ADMS system and proper integration with audit trail systems.
    /// 
    /// <para><strong>Validation Rules (via ADMS.API.Common.MatterActivityValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null, empty, or whitespace</item>
    /// <item><strong>Length:</strong> 2-50 characters (matching database constraint)</item>
    /// <item><strong>Format:</strong> Must contain letters and use only letters, numbers, and underscores</item>
    /// <item><strong>Allowed Values:</strong> Must be one of the predefined allowed activities</item>
    /// <item><strong>Reserved Names:</strong> Cannot use system-reserved activity names</item>
    /// </list>
    /// 
    /// <para><strong>Standard Activity Names:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Matter creation activity for new matter establishment</item>
    /// <item><strong>ARCHIVED:</strong> Matter archival activity for lifecycle management</item>
    /// <item><strong>DELETED:</strong> Matter deletion activity with audit trail preservation</item>
    /// <item><strong>RESTORED:</strong> Matter restoration activity for deleted matter recovery</item>
    /// <item><strong>UNARCHIVED:</strong> Matter unarchival activity for reactivation</item>
    /// <item><strong>VIEWED:</strong> Matter viewing activity for access tracking</item>
    /// </list>
    /// 
    /// <para><strong>Professional Significance:</strong></para>
    /// Activity names are used throughout the system for matter lifecycle classification, audit trail generation,
    /// legal compliance reporting, and professional practice management.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard activity examples
    /// var creationActivity = new MatterActivityDto { Activity = "CREATED" };
    /// var archivalActivity = new MatterActivityDto { Activity = "ARCHIVED" };
    /// var viewingActivity = new MatterActivityDto { Activity = "VIEWED" };
    /// 
    /// // Validation example
    /// bool isValid = MatterActivityValidationHelper.IsActivityAllowed(creationActivity.Activity); // true
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Activity description is required and cannot be empty.")]
    [MaxLength(MatterActivityValidationHelper.MaxActivityLength,
        ErrorMessage = "Activity description cannot exceed {1} characters.")]
    public required string Activity { get; set; }

    /// <summary>
    /// Gets or sets the collection of user associations for this activity.
    /// </summary>
    /// <remarks>
    /// This collection maintains the comprehensive audit trail for all users who have performed this matter activity.
    /// Each association represents a specific instance of this activity being performed by a user on a matter,
    /// providing complete professional accountability and legal compliance support.
    /// 
    /// <para><strong>Audit Trail Functionality:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Attribution:</strong> Complete user accountability for activity operations</item>
    /// <item><strong>Matter Context:</strong> Links activities to specific matters for comprehensive organization</item>
    /// <item><strong>Temporal Tracking:</strong> Precise timestamps for chronological audit trail construction</item>
    /// <item><strong>Legal Compliance:</strong> Supports comprehensive audit trail requirements</item>
    /// </list>
    /// 
    /// <para><strong>Professional Practice Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Analysis:</strong> Enables activity-based reporting and analysis</item>
    /// <item><strong>User Monitoring:</strong> Tracks user engagement with specific activity types</item>
    /// <item><strong>Matter Lifecycle:</strong> Comprehensive matter lifecycle activity tracking</item>
    /// <item><strong>Quality Assurance:</strong> Supports professional quality assurance and oversight</item>
    /// </list>
    /// 
    /// <para><strong>Collection Validation:</strong></para>
    /// The collection is validated using DtoValidationHelper.ValidateCollection for comprehensive validation
    /// including null checking, individual item validation, and business rule compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Activity with user associations
    /// var activityWithUsers = new MatterActivityDto
    /// {
    ///     Id = activityGuid,
    ///     Activity = "CREATED",
    ///     MatterActivityUsers = new List&lt;MatterActivityUserDto&gt;
    ///     {
    ///         new MatterActivityUserDto 
    ///         { 
    ///             MatterId = matterGuid,
    ///             MatterActivityId = activityGuid,
    ///             UserId = userGuid,
    ///             CreatedAt = DateTime.UtcNow
    ///         }
    ///     }
    /// };
    /// 
    /// // Accessing activity audit trail
    /// foreach (var userAssociation in activityWithUsers.MatterActivityUsers)
    /// {
    ///     Console.WriteLine($"User performed {activityWithUsers.Activity} at {userAssociation.CreatedAt}");
    /// }
    /// </code>
    /// </example>
    public ICollection<MatterActivityUserDto> MatterActivityUsers { get; set; } = [];

    #endregion Core Properties

    #region Computed Properties

    /// <summary>
    /// Gets a value indicating whether this activity has any recorded user associations.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.MatterActivity.HasUserAssociations"/> and
    /// determines if the activity has been used in the system, useful for activity analysis and validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityDto.HasUserAssociations)
    /// {
    ///     Console.WriteLine($"Activity {activityDto.Activity} is actively being used");
    /// }
    /// </code>
    /// </example>
    public bool HasUserAssociations => MatterActivityUsers.Count > 0;

    /// <summary>
    /// Gets the total count of user associations for this activity.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.MatterActivity.UsageCount"/> and provides
    /// insight into activity usage frequency for analysis and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Activity {activityDto.Activity} has been performed {activityDto.UsageCount} times");
    /// </code>
    /// </example>
    public int UsageCount => MatterActivityUsers.Count;

    /// <summary>
    /// Gets the count of unique users who have performed this activity.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.MatterActivity.UniqueUserCount"/> and
    /// provides insight into user engagement breadth for analysis and training needs assessment.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Activity {activityDto.Activity} has been used by {activityDto.UniqueUserCount} different users");
    /// </code>
    /// </example>
    public int UniqueUserCount => MatterActivityUsers
        .Select(au => au.UserId)
        .Distinct()
        .Count();

    /// <summary>
    /// Gets the count of unique matters this activity has been performed on.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.MatterActivity.UniqueMatterCount"/> and
    /// provides insight into matter engagement breadth for analysis and system usage patterns.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Activity {activityDto.Activity} has been performed on {activityDto.UniqueMatterCount} different matters");
    /// </code>
    /// </example>
    public int UniqueMatterCount => MatterActivityUsers
        .Select(au => au.MatterId)
        .Distinct()
        .Count();

    /// <summary>
    /// Gets a value indicating whether this activity is one of the standard seeded activities.
    /// </summary>
    /// <remarks>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterActivity.IsStandardActivity"/> and
    /// identifies whether the activity is system-defined or custom, useful for business logic and validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityDto.IsStandardActivity)
    /// {
    ///     // Apply standard business rules
    ///     Console.WriteLine("This is a standard system activity");
    /// }
    /// </code>
    /// </example>
    public bool IsStandardActivity => MatterActivityValidationHelper.IsActivityAllowed(Activity);

    /// <summary>
    /// Gets the normalized version of the activity name.
    /// </summary>
    /// <remarks>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterActivity.NormalizedActivity"/> and provides
    /// a normalized version using MatterActivityValidationHelper for consistent comparison and storage.
    /// </remarks>
    /// <example>
    /// <code>
    /// var normalized = activityDto.NormalizedActivity; // Always uppercase, trimmed
    /// bool areEqual = activityDto.NormalizedActivity == otherActivity.NormalizedActivity;
    /// </code>
    /// </example>
    public string NormalizedActivity => MatterActivityValidationHelper.NormalizeActivity(Activity) ?? Activity.ToUpperInvariant();

    /// <summary>
    /// Gets the activity category for classification and reporting purposes.
    /// </summary>
    /// <remarks>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterActivity.ActivityCategory"/> and categorizes
    /// the activity based on its type for professional reporting and analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// var category = activityDto.ActivityCategory; // "Lifecycle", "Archive", "Access", or "Unknown"
    /// 
    /// // Category-based processing
    /// switch (activityDto.ActivityCategory)
    /// {
    ///     case "Lifecycle":
    ///         ProcessLifecycleActivity(activityDto);
    ///         break;
    ///     case "Archive":
    ///         ProcessArchiveActivity(activityDto);
    ///         break;
    /// }
    /// </code>
    /// </example>
    public string ActivityCategory => Activity.ToUpperInvariant() switch
    {
        "CREATED" or "DELETED" or "RESTORED" => "Lifecycle",
        "ARCHIVED" or "UNARCHIVED" => "Archive",
        "VIEWED" => "Access",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets the display text suitable for UI controls and activity identification.
    /// </summary>
    /// <remarks>
    /// Provides a consistent format for displaying activity information in UI elements,
    /// using the activity name for clear professional identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// var displayText = activityDto.DisplayText; // Returns the activity name
    /// activityDropdown.Items.Add(new ListItem(activityDto.DisplayText, activityDto.Id.ToString()));
    /// </code>
    /// </example>
    public string DisplayText => Activity;

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="MatterActivityDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using centralized validation helpers for consistency with entity
    /// validation rules. This ensures the DTO maintains the same validation standards as the corresponding
    /// ADMS.API.Entities.MatterActivity entity while enforcing professional standards.
    /// 
    /// <para><strong>Comprehensive Validation Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>ID Validation:</strong> Activity ID validated for proper GUID structure</item>
    /// <item><strong>Activity Name Validation:</strong> Comprehensive activity name validation using centralized helpers</item>
    /// <item><strong>User Association Validation:</strong> Deep validation of user association collection</item>
    /// <item><strong>Business Rule Validation:</strong> Activity-specific business rule compliance</item>
    /// </list>
    /// 
    /// <para><strong>Professional Standards Integration:</strong></para>
    /// Uses centralized validation helpers (MatterActivityValidationHelper, DtoValidationHelper) to ensure
    /// consistency across all activity validation in the system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterActivityDto 
    /// { 
    ///     Id = Guid.Empty, // Invalid
    ///     Activity = "", // Invalid
    ///     MatterActivityUsers = new List&lt;MatterActivityUserDto&gt; { null } // Invalid
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
        foreach (var result in ValidateActivityId())
            yield return result;

        // Validate activity name using centralized helper
        foreach (var result in ValidateActivityName())
            yield return result;

        // Validate user associations collection
        foreach (var result in ValidateUserAssociations())
            yield return result;

        // Validate business rules
        foreach (var result in ValidateBusinessRules())
            yield return result;
    }

    /// <summary>
    /// Validates the <see cref="Id"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the activity ID.</returns>
    /// <remarks>
    /// Uses MatterActivityValidationHelper.ValidateActivityId for consistent validation
    /// across all activity-related DTOs and entities.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateActivityId()
    {
        return MatterActivityValidationHelper.ValidateActivityId(Id, nameof(Id));
    }

    /// <summary>
    /// Validates the <see cref="Activity"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the activity name.</returns>
    /// <remarks>
    /// Uses MatterActivityValidationHelper.ValidateActivity for comprehensive validation
    /// including format, length, allowed values, and reserved name checking.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateActivityName()
    {
        return MatterActivityValidationHelper.ValidateActivity(Activity, nameof(Activity));
    }

    /// <summary>
    /// Validates the <see cref="MatterActivityUsers"/> collection using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the user associations collection.</returns>
    /// <remarks>
    /// Uses DtoValidationHelper.ValidateCollection for comprehensive validation of the user association
    /// collection, including null checking and deep validation of individual items.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateUserAssociations()
    {
        return DtoValidationHelper.ValidateCollection(MatterActivityUsers, nameof(MatterActivityUsers));
    }

    /// <summary>
    /// Validates business rules specific to matter activities.
    /// </summary>
    /// <returns>A collection of validation results for business rule compliance.</returns>
    /// <remarks>
    /// Validates activity-specific business rules such as user association requirements
    /// and activity appropriateness for professional practice standards.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Validate user association requirements
        if (!MatterActivityValidationHelper.HasRequiredUserAssociations(MatterActivityUsers.Count, allowEmptyUsers: false))
        {
            yield return new ValidationResult(
                "At least one user association is required for professional accountability and audit trail integrity.",
                [nameof(MatterActivityUsers)]);
        }

        // Validate for duplicate activities if applicable
        var existingActivities = MatterActivityUsers
            .Where(au => au.MatterActivity != null)
            .Select(au => au.MatterActivity!.Activity)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .ToArray();

        if (existingActivities.Length > 0 &&
            !MatterActivityValidationHelper.IsValidActivityDuplication(Activity, existingActivities, allowDuplicates: true))
        {
            yield return new ValidationResult(
                $"Activity '{Activity}' has inappropriate duplication patterns in user associations.",
                [nameof(Activity)]);
        }
    }

    #endregion Validation Implementation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this activity represents a creation operation.
    /// </summary>
    /// <returns>true if this is a creation activity; otherwise, false.</returns>
    /// <remarks>
    /// This method mirrors <see cref="ADMS.API.Entities.MatterActivity.IsCreationActivity"/> and provides
    /// a convenient way to identify creation activities for business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityDto.IsCreationActivity())
    /// {
    ///     Console.WriteLine("This activity creates new matters");
    /// }
    /// </code>
    /// </example>
    public bool IsCreationActivity() => string.Equals(Activity, "CREATED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents an archival operation.
    /// </summary>
    /// <returns>true if this is an archival activity; otherwise, false.</returns>
    /// <remarks>
    /// This method mirrors <see cref="ADMS.API.Entities.MatterActivity.IsArchivalActivity"/> and provides
    /// archival activity identification for business logic.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityDto.IsArchivalActivity())
    /// {
    ///     Console.WriteLine("This activity archives matters");
    /// }
    /// </code>
    /// </example>
    public bool IsArchivalActivity() => string.Equals(Activity, "ARCHIVED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a deletion operation.
    /// </summary>
    /// <returns>true if this is a deletion activity; otherwise, false.</returns>
    /// <remarks>
    /// This method mirrors <see cref="ADMS.API.Entities.MatterActivity.IsDeletionActivity"/> and provides
    /// deletion activity identification for audit trail analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityDto.IsDeletionActivity())
    /// {
    ///     Console.WriteLine("This activity deletes matters");
    /// }
    /// </code>
    /// </example>
    public bool IsDeletionActivity() => string.Equals(Activity, "DELETED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a restoration operation.
    /// </summary>
    /// <returns>true if this is a restoration activity; otherwise, false.</returns>
    /// <remarks>
    /// This method mirrors <see cref="ADMS.API.Entities.MatterActivity.IsRestorationActivity"/> and provides
    /// restoration activity identification for recovery operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityDto.IsRestorationActivity())
    /// {
    ///     Console.WriteLine("This activity restores deleted matters");
    /// }
    /// </code>
    /// </example>
    public bool IsRestorationActivity() => string.Equals(Activity, "RESTORED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents an unarchival operation.
    /// </summary>
    /// <returns>true if this is an unarchival activity; otherwise, false.</returns>
    /// <remarks>
    /// This method mirrors <see cref="ADMS.API.Entities.MatterActivity.IsUnarchivalActivity"/> and provides
    /// unarchival activity identification for business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityDto.IsUnarchivalActivity())
    /// {
    ///     Console.WriteLine("This activity unarchives matters");
    /// }
    /// </code>
    /// </example>
    public bool IsUnarchivalActivity() => string.Equals(Activity, "UNARCHIVED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a viewing operation.
    /// </summary>
    /// <returns>true if this is a viewing activity; otherwise, false.</returns>
    /// <remarks>
    /// This method mirrors <see cref="ADMS.API.Entities.MatterActivity.IsViewingActivity"/> and provides
    /// viewing activity identification for access tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityDto.IsViewingActivity())
    /// {
    ///     Console.WriteLine("This activity tracks matter access");
    /// }
    /// </code>
    /// </example>
    public bool IsViewingActivity() => string.Equals(Activity, "VIEWED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity is appropriate for the given matter context.
    /// </summary>
    /// <param name="isArchived">Whether the matter is currently archived.</param>
    /// <param name="isDeleted">Whether the matter is currently deleted.</param>
    /// <returns>true if the activity is appropriate for the context; otherwise, false.</returns>
    /// <remarks>
    /// This method mirrors <see cref="ADMS.API.Entities.MatterActivity.IsAppropriateForMatterStatus"/> and
    /// uses MatterActivityValidationHelper to validate business rules for activity context appropriateness.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool canApply = activityDto.IsAppropriateForMatterStatus(false, false);  // true for normal matter
    /// bool cannotApply = activityDto.IsAppropriateForMatterStatus(true, false); // false if trying to archive already archived
    /// </code>
    /// </example>
    public bool IsAppropriateForMatterStatus(bool isArchived, bool isDeleted) =>
        MatterActivityValidationHelper.IsActivityAppropriateForMatterStatus(Activity, isArchived, isDeleted);

    /// <summary>
    /// Gets usage statistics for this activity.
    /// </summary>
    /// <returns>A dictionary containing usage statistics and analysis data.</returns>
    /// <remarks>
    /// This method mirrors <see cref="ADMS.API.Entities.MatterActivity.GetUsageStatistics"/> and provides
    /// insights into usage patterns and statistics for reporting and analysis purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = activityDto.GetUsageStatistics();
    /// Console.WriteLine($"Total usage: {stats["TotalUsage"]}");
    /// Console.WriteLine($"Activity category: {stats["ActivityCategory"]}");
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetUsageStatistics()
    {
        return new Dictionary<string, object>
        {
            ["ActivityType"] = Activity,
            ["ActivityCategory"] = ActivityCategory,
            ["TotalUsage"] = UsageCount,
            ["UniqueUsers"] = UniqueUserCount,
            ["UniqueMatters"] = UniqueMatterCount,
            ["IsStandardActivity"] = IsStandardActivity,
            ["HasUserAssociations"] = HasUserAssociations,
            ["NormalizedActivity"] = NormalizedActivity
        };
    }

    #endregion Business Logic Methods

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="MatterActivityDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The MatterActivityDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate MatterActivityDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterActivityDto 
    /// { 
    ///     Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
    ///     Activity = "CREATED"
    /// };
    /// 
    /// var results = MatterActivityDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Activity validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterActivityDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterActivityDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto);
        results.AddRange(dto.Validate(context));

        return results;
    }

    /// <summary>
    /// Creates a MatterActivityDto from ADMS.API.Entities.MatterActivity entity with validation.
    /// </summary>
    /// <param name="entity">The MatterActivity entity to convert. Cannot be null.</param>
    /// <param name="includeUserAssociations">Whether to include user associations in the conversion.</param>
    /// <returns>A valid MatterActivityDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create MatterActivityDto instances from
    /// ADMS.API.Entities.MatterActivity entities with automatic validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity with user associations
    /// var entity = new ADMS.API.Entities.MatterActivity 
    /// { 
    ///     Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
    ///     Activity = "CREATED"
    /// };
    /// 
    /// var dto = MatterActivityDto.FromEntity(entity, includeUserAssociations: true);
    /// </code>
    /// </example>
    public static MatterActivityDto FromEntity([NotNull] Entities.MatterActivity entity, bool includeUserAssociations = false)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new MatterActivityDto
        {
            Id = entity.Id,
            Activity = entity.Activity,
            MatterActivityUsers = includeUserAssociations
                ? entity.MatterActivityUsers.Select(au => MatterActivityUserDto.FromEntity(au)).ToList()
                : []
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Created MatterActivityDto failed validation: {errorMessages}");

    }

    /// <summary>
    /// Creates multiple MatterActivityDto instances from a collection of entities.
    /// </summary>
    /// <param name="entities">The collection of MatterActivity entities to convert. Cannot be null.</param>
    /// <param name="includeUserAssociations">Whether to include user associations in the conversion.</param>
    /// <returns>A collection of valid MatterActivityDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method is optimized for creating multiple activity DTOs efficiently.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convert collection of activity entities
    /// var entities = await context.MatterActivities.ToListAsync();
    /// var activityDtos = MatterActivityDto.FromEntities(entities, includeUserAssociations: false);
    /// 
    /// // Process activities
    /// foreach (var activityDto in activityDtos)
    /// {
    ///     ProcessActivity(activityDto);
    /// }
    /// </code>
    /// </example>
    public static IList<MatterActivityDto> FromEntities([NotNull] IEnumerable<Entities.MatterActivity> entities, bool includeUserAssociations = false)
    {
        ArgumentNullException.ThrowIfNull(entities);

        return entities.Select(entity => FromEntity(entity, includeUserAssociations)).ToList();
    }

    #endregion Static Methods

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified MatterActivityDto is equal to the current MatterActivityDto.
    /// </summary>
    /// <param name="other">The MatterActivityDto to compare with the current MatterActivityDto.</param>
    /// <returns>true if the specified MatterActivityDto is equal to the current MatterActivityDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each activity has a unique identifier.
    /// This follows the same equality pattern as ADMS.API.Entities.MatterActivity for consistency.
    /// </remarks>
    public bool Equals(MatterActivityDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current MatterActivityDto.
    /// </summary>
    /// <param name="obj">The object to compare with the current MatterActivityDto.</param>
    /// <returns>true if the specified object is equal to the current MatterActivityDto; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as MatterActivityDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterActivityDto.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterActivityDto.
    /// </summary>
    /// <returns>A string that represents the current MatterActivityDto.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the activity,
    /// which is useful for debugging, logging, and display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterActivityDto 
    /// { 
    ///     Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
    ///     Activity = "CREATED"
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "MatterActivity: CREATED (20000000-0000-0000-0000-000000000001) - 0 associations"
    /// </code>
    /// </example>
    public override string ToString() => $"MatterActivity: {Activity} ({Id}) - {UsageCount} associations";

    #endregion String Representation
}