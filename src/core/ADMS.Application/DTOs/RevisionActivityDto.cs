using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing a revision activity with complete audit trail and user associations.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of a revision activity within the ADMS legal document management system,
/// including all user associations and audit trail relationships. It mirrors the structure of 
/// <see cref="ADMS.API.Entities.RevisionActivity"/> while providing comprehensive validation and computed properties
/// for client-side operations and audit trail management.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Entity Representation:</strong> Mirrors all properties and relationships from ADMS.API.Entities.RevisionActivity</item>
/// <item><strong>User Association Integration:</strong> Includes comprehensive user activity collections for complete audit trail support</item>
/// <item><strong>Professional Validation:</strong> Uses ADMS.API.Common.RevisionActivityValidationHelper for data integrity</item>
/// <item><strong>Computed Properties:</strong> Client-optimized properties for UI display and business logic</item>
/// <item><strong>Collection Validation:</strong> Deep validation of user activity collections using DtoValidationHelper</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO maintains the same relationship structure as ADMS.API.Entities.RevisionActivity:
/// <list type="bullet">
/// <item><strong>RevisionActivityUsers:</strong> Complete collection of users who have performed this activity type</item>
/// <item><strong>Activity Classification:</strong> Standardized activity names for consistent operation classification</item>
/// <item><strong>Usage Statistics:</strong> Comprehensive metrics about activity usage and patterns</item>
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
/// <item><strong>API Responses:</strong> Complete activity data including all user associations and audit trails</item>
/// <item><strong>Activity Management:</strong> Comprehensive activity administration and analytics</item>
/// <item><strong>Audit Trail Analysis:</strong> Complete activity usage patterns and user attribution</item>
/// <item><strong>Reporting:</strong> Activity-based reporting and analytics with complete context</item>
/// <item><strong>System Administration:</strong> Complete activity lifecycle management and monitoring</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Standardized Activities:</strong> Consistent activity classification for legal compliance</item>
/// <item><strong>Comprehensive Audit Compliance:</strong> Complete activity usage tracking for legal compliance</item>
/// <item><strong>Professional Documentation:</strong> Clear activity descriptions for legal documentation</item>
/// <item><strong>User Accountability:</strong> Complete user attribution for all activity instances</item>
/// </list>
/// 
/// <para><strong>Data Integrity:</strong></para>
/// <list type="bullet">
/// <item><strong>Validation Integration:</strong> Uses RevisionActivityValidationHelper for consistent validation</item>
/// <item><strong>Standardized Format:</strong> Activity names follow uppercase convention</item>
/// <item><strong>Reserved Name Protection:</strong> Prevents use of system-reserved activity names</item>
/// <item><strong>Collection Integrity:</strong> Comprehensive validation of user association collections</item>
/// </list>
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// <list type="bullet">
/// <item><strong>Lazy Loading Support:</strong> User collections can be populated on-demand</item>
/// <item><strong>Selective Loading:</strong> Individual user collections can be loaded independently</item>
/// <item><strong>Computed Properties:</strong> Cached computed values for frequently accessed calculations</item>
/// <item><strong>Validation Optimization:</strong> Efficient validation using centralized helpers</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive revision activity DTO
/// var activityDto = new RevisionActivityDto
/// {
///     Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
///     Activity = "CREATED"
/// };
/// 
/// // Validating the complete activity DTO
/// var validationResults = RevisionActivityDto.ValidateModel(activityDto);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Using computed properties
/// var usageCount = activityDto.UsageCount;
/// var hasUsers = activityDto.HasUserAssociations;
/// var displayText = activityDto.DisplayText;
/// </code>
/// </example>
public class RevisionActivityDto : IValidatableObject, IEquatable<RevisionActivityDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the revision activity.
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
    /// <item><strong>Database Operations:</strong> Primary key for activity-related database queries</item>
    /// <item><strong>Collection Management:</strong> Key for user association collections</item>
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
    /// var createdActivity = new RevisionActivityDto 
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
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the standardized activity name describing the revision operation.
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
    /// var createdActivity = new RevisionActivityDto { Activity = "CREATED" };
    /// var savedActivity = new RevisionActivityDto { Activity = "SAVED" };
    /// var deletedActivity = new RevisionActivityDto { Activity = "DELETED" };
    /// var restoredActivity = new RevisionActivityDto { Activity = "RESTORED" };
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
    public required string Activity { get; set; }

    #endregion Core Properties

    #region Navigation Collections

    /// <summary>
    /// Gets or sets the collection of revision activity user associations for this activity.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.RevisionActivity.RevisionActivityUsers"/> and maintains
    /// the many-to-many relationship between activities, users, and revisions through the RevisionActivityUser
    /// junction entity. Each association represents a specific instance of this activity being performed.
    /// 
    /// <para><strong>Comprehensive Audit Trail Functionality:</strong></para>
    /// Each association in this collection provides:
    /// <list type="bullet">
    /// <item><strong>User Attribution:</strong> Complete user accountability for activity instances</item>
    /// <item><strong>Revision Context:</strong> Specific revision the activity was performed on</item>
    /// <item><strong>Temporal Tracking:</strong> Precise timestamps for when activities occurred</item>
    /// <item><strong>Unique Identification:</strong> Composite key preventing duplicate entries</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Integration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured in ADMS.API.Entities DbContext.ConfigureRevisionActivityUser</item>
    /// <item>Many-to-many-to-many relationship through junction entity</item>
    /// <item>Composite primary key includes ActivityId, RevisionId, UserId, CreatedAt</item>
    /// <item>Cascade behavior: Restricted to preserve comprehensive audit trail integrity</item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Usage Analysis:</strong> Understanding how frequently activities are used</item>
    /// <item><strong>User Activity Reporting:</strong> Comprehensive user activity monitoring and reporting</item>
    /// <item><strong>Activity Lifecycle Tracking:</strong> Complete activity instance history</item>
    /// <item><strong>Legal Compliance:</strong> Comprehensive audit trail support for legal requirements</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// This collection can be large for frequently used activities. Consider using explicit loading,
    /// projections, or pagination when working with comprehensive activity data to avoid performance issues.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing comprehensive activity usage
    /// foreach (var activityUser in revisionActivity.RevisionActivityUsers)
    /// {
    ///     Console.WriteLine($"User {activityUser.User.DisplayName} performed {revisionActivity.Activity} " +
    ///                      $"on revision {activityUser.Revision.RevisionNumber} at {activityUser.CreatedAt}");
    /// }
    /// 
    /// // Finding all users who performed this activity
    /// var activityUsers = revisionActivity.RevisionActivityUsers
    ///     .Select(au => au.User)
    ///     .Distinct();
    /// 
    /// // Activity usage statistics
    /// var usageCount = revisionActivity.RevisionActivityUsers.Count;
    /// var uniqueUsers = revisionActivity.RevisionActivityUsers
    ///     .Select(au => au.UserId)
    ///     .Distinct()
    ///     .Count();
    /// 
    /// // Recent activity instances
    /// var recentUsage = revisionActivity.RevisionActivityUsers
    ///     .Where(au => au.CreatedAt >= DateTime.UtcNow.AddDays(-30))
    ///     .OrderByDescending(au => au.CreatedAt);
    /// </code>
    /// </example>
    public ICollection<RevisionActivityUserDto> RevisionActivityUsers { get; set; } = new List<RevisionActivityUserDto>();

    #endregion Navigation Collections

    #region Computed Properties

    /// <summary>
    /// Gets the normalized version of the activity name for consistent comparison and storage.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.RevisionActivity.NormalizedActivity"/> and
    /// provides a normalized version of the activity name following the standardization rules from
    /// ADMS.API.Common.RevisionActivityValidationHelper. Normalization includes trimming whitespace
    /// and converting to uppercase for consistency.
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
    /// var activity1 = new RevisionActivityDto { Activity = "  created  " };
    /// var activity2 = new RevisionActivityDto { Activity = "CREATED" };
    /// 
    /// // Both will have the same normalized activity: "CREATED"
    /// bool areEquivalent = activity1.NormalizedActivity == activity2.NormalizedActivity; // true
    /// </code>
    /// </example>
    public string NormalizedActivity =>
        RevisionActivityValidationHelper.NormalizeActivity(Activity);

    /// <summary>
    /// Gets a value indicating whether this activity has any recorded user associations.
    /// </summary>
    /// <remarks>
    /// This property mirrors <see cref="ADMS.API.Entities.RevisionActivity.HasUserAssociations"/> and is useful 
    /// for determining activity usage and ensuring that activities are actually being used in the system.
    /// Activities without associations may indicate unused system features or configuration issues.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Usage Analysis:</strong> Understanding which activities are actively used</item>
    /// <item><strong>System Configuration Validation:</strong> Identifying potentially unused activities</item>
    /// <item><strong>Feature Utilization Monitoring:</strong> Tracking system feature adoption</item>
    /// <item><strong>Data Cleanup Operations:</strong> Identifying activities that may need attention</item>
    /// </list>
    /// 
    /// <para><strong>Performance Note:</strong></para>
    /// This property evaluates the count of the loaded RevisionActivityUsers collection. For accurate results
    /// when the collection is not loaded, consider using database-level queries.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!revisionActivity.HasUserAssociations)
    /// {
    ///     logger.LogInformation($"Activity {revisionActivity.Activity} has no user associations");
    /// }
    /// 
    /// // Finding unused activities for cleanup or analysis
    /// var unusedActivities = activities.Where(a => !a.HasUserAssociations).ToList();
    /// 
    /// // Activity usage summary
    /// var usageSummary = activities.Select(a => new {
    ///     Activity = a.Activity,
    ///     HasUsage = a.HasUserAssociations,
    ///     UsageCount = a.UsageCount
    /// });
    /// </code>
    /// </example>
    public bool HasUserAssociations => RevisionActivityUsers.Count > 0;

    /// <summary>
    /// Gets the total count of user associations for this activity.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.RevisionActivity.UsageCount"/> and provides
    /// insight into the frequency of use for this activity type, useful for activity monitoring, 
    /// usage analytics, and system optimization.
    /// 
    /// <para><strong>Analytics Applications:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Usage Frequency Analysis:</strong> Understanding which activities are most common</item>
    /// <item><strong>System Performance Planning:</strong> Optimizing for frequently used activities</item>
    /// <item><strong>User Behavior Analysis:</strong> Understanding user activity patterns</item>
    /// <item><strong>Feature Adoption Metrics:</strong> Measuring feature usage and adoption rates</item>
    /// </list>
    /// 
    /// <para><strong>Performance Note:</strong></para>
    /// This property counts the loaded RevisionActivityUsers collection. For large datasets or
    /// when the collection is not loaded, consider using database-level aggregation for better performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Activity {revisionActivity.Activity} has been used {revisionActivity.UsageCount} times");
    /// 
    /// // Finding most used activities
    /// var mostUsedActivities = activities
    ///     .OrderByDescending(a => a.UsageCount)
    ///     .Take(10)
    ///     .Select(a => new { a.Activity, a.UsageCount });
    /// 
    /// // Usage analytics
    /// var totalUsage = activities.Sum(a => a.UsageCount);
    /// var usagePercentages = activities.Select(a => new {
    ///     Activity = a.Activity,
    ///     Count = a.UsageCount,
    ///     Percentage = (double)a.UsageCount / totalUsage * 100
    /// });
    /// </code>
    /// </example>
    public int UsageCount => RevisionActivityUsers.Count;

    /// <summary>
    /// Gets the count of unique users who have performed this activity.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.RevisionActivity.UniqueUserCount"/> and 
    /// provides insight into the breadth of user engagement with this activity type, useful for user 
    /// adoption analysis and training needs assessment.
    /// 
    /// <para><strong>User Engagement Metrics:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Adoption Analysis:</strong> Understanding how many users utilize each activity</item>
    /// <item><strong>Training Needs Assessment:</strong> Identifying activities that may need more user training</item>
    /// <item><strong>Feature Penetration:</strong> Measuring how broadly features are adopted across users</item>
    /// <item><strong>User Behavior Patterns:</strong> Analyzing user engagement with different activity types</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Activity {revisionActivity.Activity} has been performed by {revisionActivity.UniqueUserCount} different users");
    /// 
    /// // User engagement analysis
    /// var engagementMetric = revisionActivity.UniqueUserCount / (double)revisionActivity.UsageCount;
    /// Console.WriteLine($"Average usage per user: {1/engagementMetric:F2}");
    /// 
    /// // Finding activities with broad user adoption
    /// var broadlyAdoptedActivities = activities
    ///     .Where(a => a.UniqueUserCount > 10)
    ///     .OrderByDescending(a => a.UniqueUserCount);
    /// </code>
    /// </example>
    public int UniqueUserCount => RevisionActivityUsers
        .Select(au => au.UserId)
        .Distinct()
        .Count();

    /// <summary>
    /// Gets a value indicating whether this activity is one of the standard seeded activities.
    /// </summary>
    /// <remarks>
    /// This property mirrors <see cref="ADMS.API.Entities.RevisionActivity.IsStandardActivity"/> and
    /// identifies whether the activity is one of the standard system-defined activities from the 
    /// ADMS.API.Entities.RevisionActivity seeded data or a potentially custom activity.
    /// 
    /// <para><strong>Standard vs Custom Activities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Standard Activities:</strong> CREATED, SAVED, DELETED, RESTORED (system-defined)</item>
    /// <item><strong>Custom Activities:</strong> Activities that may be added for specific business needs</item>
    /// <item><strong>Business Rules:</strong> Standard activities have predefined business logic and validation</item>
    /// <item><strong>System Integration:</strong> Standard activities are fully integrated with all system features</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsStandardActivity)
    /// {
    ///     // Apply standard business rules and validation
    ///     ProcessStandardActivity(activity);
    /// }
    /// else
    /// {
    ///     // Handle custom activity logic
    ///     ProcessCustomActivity(activity);
    /// }
    /// 
    /// // System configuration validation
    /// var customActivities = activities.Where(a => !a.IsStandardActivity).ToList();
    /// if (customActivities.Any())
    /// {
    ///     logger.LogInformation($"Found {customActivities.Count} custom activities");
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
    /// <item>All required properties are properly set</item>
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
    /// 
    /// // Batch filtering of valid activities
    /// var validActivities = activities.Where(a => a.IsValid).ToList();
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
    /// var activity = new RevisionActivityDto { Activity = "CREATED" };
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

    /// <summary>
    /// Gets comprehensive activity metrics for reporting and analysis.
    /// </summary>
    /// <remarks>
    /// This property provides a structured object containing key metrics and information
    /// for comprehensive activity analysis and reporting purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var metrics = activity.ActivityMetrics;
    /// // Access comprehensive metrics for analysis
    /// </code>
    /// </example>
    public object ActivityMetrics => new
    {
        ActivityInfo = new
        {
            Activity,
            NormalizedActivity,
            IsStandardActivity,
            UserFriendlyDescription
        },
        UsageMetrics = new
        {
            UsageCount,
            UniqueUserCount,
            HasUserAssociations,
            AverageUsagePerUser = UniqueUserCount > 0 ? (double)UsageCount / UniqueUserCount : 0
        },
        ActivityClassification = new
        {
            IsCreationActivity,
            IsDeletionActivity,
            IsRestorationActivity,
            IsSaveActivity
        }
    };

    #endregion Computed Properties

    #region Activity Classification Properties

    /// <summary>
    /// Gets a value indicating whether this activity represents a creation operation.
    /// </summary>
    /// <remarks>
    /// This property mirrors the business logic from <see cref="ADMS.API.Entities.RevisionActivity.IsCreationActivity"/> and
    /// provides a convenient way to identify creation activities for business rule enforcement and audit trail analysis.
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
    /// This property mirrors the business logic from <see cref="ADMS.API.Entities.RevisionActivity.IsDeletionActivity"/> and
    /// provides a convenient way to identify deletion activities for audit trail analysis and business rule enforcement.
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
    /// This property mirrors the business logic from <see cref="ADMS.API.Entities.RevisionActivity.IsRestorationActivity"/> and
    /// provides a convenient way to identify restoration activities for audit trail analysis and recovery operations.
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
    /// This property mirrors the business logic from <see cref="ADMS.API.Entities.RevisionActivity.IsSaveActivity"/> and
    /// provides a convenient way to identify save activities for business rule enforcement and workflow analysis.
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

    #endregion Activity Classification Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="RevisionActivityDto"/> for data integrity and business rules compliance.
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
    /// <item><strong>Collection Validation:</strong> Deep validation of user association collections</item>
    /// <item><strong>Business Rules:</strong> Activity-specific business rule compliance</item>
    /// <item><strong>Format Validation:</strong> Character format and length constraints</item>
    /// </list>
    /// 
    /// <para><strong>Integration with ValidationHelper:</strong></para>
    /// Uses the centralized RevisionActivityValidationHelper and DtoValidationHelper to ensure consistency 
    /// across all activity-related validation in the system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new RevisionActivityDto 
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

        // Validate user associations collection
        foreach (var result in ValidateRevisionActivityUsers())
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

    /// <summary>
    /// Validates the <see cref="RevisionActivityUsers"/> collection using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the user associations collection.</returns>
    /// <remarks>
    /// Uses DtoValidationHelper.ValidateCollection for comprehensive validation of the user association
    /// collection, including null checking and deep validation of individual items.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateRevisionActivityUsers()
    {
        return DtoValidationHelper.ValidateCollection(RevisionActivityUsers, nameof(RevisionActivityUsers));
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="RevisionActivityDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The RevisionActivityDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate RevisionActivityDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new RevisionActivityDto 
    /// { 
    ///     Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
    ///     Activity = "CREATED"
    /// };
    /// 
    /// var results = RevisionActivityDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Activity validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] RevisionActivityDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("RevisionActivityDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a RevisionActivityDto from an ADMS.API.Entities.RevisionActivity entity with validation.
    /// </summary>
    /// <param name="revisionActivity">The RevisionActivity entity to convert. Cannot be null.</param>
    /// <param name="includeUsers">Whether to include user association collections in the conversion.</param>
    /// <returns>A valid RevisionActivityDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when revisionActivity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create RevisionActivityDto instances from
    /// ADMS.API.Entities.RevisionActivity entities with automatic validation. It ensures the resulting
    /// DTO is valid before returning it.
    /// 
    /// <para><strong>User Collection Handling:</strong></para>
    /// When includeUsers is true, the method will attempt to map user association collections.
    /// For performance reasons, user collections should typically be loaded separately using
    /// projection or explicit loading strategies.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity without user collections for better performance
    /// var entity = new ADMS.API.Entities.RevisionActivity 
    /// { 
    ///     Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
    ///     Activity = "CREATED" 
    /// };
    /// 
    /// var dto = RevisionActivityDto.FromEntity(entity, includeUsers: false);
    /// // Returns validated RevisionActivityDto instance
    /// </code>
    /// </example>
    public static RevisionActivityDto FromEntity([NotNull] Entities.RevisionActivity revisionActivity, bool includeUsers = false)
    {
        ArgumentNullException.ThrowIfNull(revisionActivity, nameof(revisionActivity));

        var dto = new RevisionActivityDto
        {
            Id = revisionActivity.Id,
            Activity = revisionActivity.Activity
        };

        // Optionally include user association collections
        if (includeUsers)
        {
            // Note: In practice, these would typically be mapped using a mapping framework
            // like AutoMapper or Mapster for better performance and maintainability
            // This is a placeholder for actual mapping logic
            dto.RevisionActivityUsers = new List<RevisionActivityUserDto>();
        }

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid RevisionActivityDto from entity: {errorMessages}");

    }

    /// <summary>
    /// Creates a RevisionActivityDto for a specific standard activity type.
    /// </summary>
    /// <param name="activityType">The standard activity type (CREATED, SAVED, DELETED, RESTORED).</param>
    /// <returns>A RevisionActivityDto with the appropriate seeded ID and activity name.</returns>
    /// <exception cref="ArgumentException">Thrown when activityType is not a recognized standard activity.</exception>
    /// <remarks>
    /// This factory method creates RevisionActivityDto instances for standard activities
    /// using the correct seeded GUIDs from ADMS.API.Entities.RevisionActivity.
    /// </remarks>
    /// <example>
    /// <code>
    /// var createdActivity = RevisionActivityDto.ForStandardActivity("CREATED");
    /// var savedActivity = RevisionActivityDto.ForStandardActivity("SAVED");
    /// var deletedActivity = RevisionActivityDto.ForStandardActivity("DELETED");
    /// var restoredActivity = RevisionActivityDto.ForStandardActivity("RESTORED");
    /// </code>
    /// </example>
    public static RevisionActivityDto ForStandardActivity(string activityType)
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

        return new RevisionActivityDto
        {
            Id = activityId,
            Activity = normalizedActivity
        };
    }

    /// <summary>
    /// Gets all standard revision activities as RevisionActivityDto instances.
    /// </summary>
    /// <returns>A collection of all standard revision activities.</returns>
    /// <remarks>
    /// This method returns all four standard revision activities (CREATED, SAVED, DELETED, RESTORED)
    /// with their correct seeded GUIDs, useful for populating dropdown lists and other UI controls.
    /// </remarks>
    /// <example>
    /// <code>
    /// var standardActivities = RevisionActivityDto.GetStandardActivities();
    /// 
    /// // Populate dropdown
    /// foreach (var activity in standardActivities)
    /// {
    ///     activityDropdown.Items.Add(new ListItem(activity.UserFriendlyDescription, activity.Id.ToString()));
    /// }
    /// </code>
    /// </example>
    public static IReadOnlyList<RevisionActivityDto> GetStandardActivities()
    {
        return new List<RevisionActivityDto>
        {
            ForStandardActivity("CREATED"),
            ForStandardActivity("DELETED"),
            ForStandardActivity("RESTORED"),
            ForStandardActivity("SAVED")
        }.AsReadOnly();
    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this activity is appropriate for the given revision context.
    /// </summary>
    /// <param name="revisionExists">Whether the revision already exists.</param>
    /// <param name="isDeleted">Whether the revision is currently deleted.</param>
    /// <returns>true if the activity is appropriate for the context; otherwise, false.</returns>
    /// <remarks>
    /// This method mirrors the business logic from <see cref="ADMS.API.Entities.RevisionActivity.IsAppropriateForContext"/> and
    /// uses the ADMS.API.Common.RevisionActivityValidationHelper to validate business rules for activity context appropriateness.
    /// </remarks>
    /// <example>
    /// <code>
    /// var createdActivity = RevisionActivityDto.ForStandardActivity("CREATED");
    /// 
    /// bool canApply = createdActivity.IsAppropriateForContext(false, false);  // true for new revision
    /// bool cannotApply = createdActivity.IsAppropriateForContext(true, false); // false for existing revision
    /// </code>
    /// </example>
    public bool IsAppropriateForContext(bool revisionExists, bool isDeleted) =>
        RevisionActivityValidationHelper.IsActivityAppropriateForContext(Activity, revisionExists, isDeleted);

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified RevisionActivityDto is equal to the current RevisionActivityDto.
    /// </summary>
    /// <param name="other">The RevisionActivityDto to compare with the current RevisionActivityDto.</param>
    /// <returns>true if the specified RevisionActivityDto is equal to the current RevisionActivityDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each activity has a unique identifier.
    /// This follows the same equality pattern as ADMS.API.Entities.RevisionActivity for consistency.
    /// </remarks>
    public bool Equals(RevisionActivityDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current RevisionActivityDto.
    /// </summary>
    /// <param name="obj">The object to compare with the current RevisionActivityDto.</param>
    /// <returns>true if the specified object is equal to the current RevisionActivityDto; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as RevisionActivityDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current RevisionActivityDto.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Determines whether two RevisionActivityDto instances are equal.
    /// </summary>
    /// <param name="left">The first RevisionActivityDto to compare.</param>
    /// <param name="right">The second RevisionActivityDto to compare.</param>
    /// <returns>true if the RevisionActivityDtos are equal; otherwise, false.</returns>
    public static bool operator ==(RevisionActivityDto? left, RevisionActivityDto? right) =>
        EqualityComparer<RevisionActivityDto>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two RevisionActivityDto instances are not equal.
    /// </summary>
    /// <param name="left">The first RevisionActivityDto to compare.</param>
    /// <param name="right">The second RevisionActivityDto to compare.</param>
    /// <returns>true if the RevisionActivityDtos are not equal; otherwise, false.</returns>
    public static bool operator !=(RevisionActivityDto? left, RevisionActivityDto? right) => !(left == right);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the RevisionActivityDto.
    /// </summary>
    /// <returns>A string that represents the current RevisionActivityDto.</returns>
    /// <remarks>
    /// The string representation includes both the activity name and ID for identification
    /// purposes, following the same pattern as ADMS.API.Entities.RevisionActivity.ToString() for consistency.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new RevisionActivityDto 
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
}