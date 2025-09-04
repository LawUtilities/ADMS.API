using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Domain.Entities;

/// <summary>
/// Represents an activity that can be performed on a matter in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// The MatterActivity entity serves as a lookup table for the standardized activities that can be performed
/// on legal matters within the document management system. This entity is critical for maintaining
/// comprehensive audit trails and ensuring consistent activity classification across all matter operations.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Static Reference Data:</strong> Contains predefined activity types seeded from database</item>
/// <item><strong>Audit Trail Foundation:</strong> Central classification system for matter activities</item>
/// <item><strong>Business Rule Enforcement:</strong> Ensures only valid activities are recorded</item>
/// <item><strong>Legal Compliance:</strong> Supports comprehensive audit requirements</item>
/// <item><strong>Immutable Activities:</strong> Activity types are system-defined and rarely change</item>
/// </list>
/// 
/// <para><strong>Seeded Activities:</strong></para>
/// The following activities are seeded in AdmsContext.SeedMatterActivities:
/// <list type="bullet">
/// <item><strong>CREATED:</strong> Matter creation activity</item>
/// <item><strong>ARCHIVED:</strong> Matter archival activity</item>
/// <item><strong>DELETED:</strong> Matter deletion activity</item>
/// <item><strong>RESTORED:</strong> Matter restoration activity</item>
/// <item><strong>UNARCHIVED:</strong> Matter unarchival activity</item>
/// <item><strong>VIEWED:</strong> Matter viewing/access activity</item>
/// </list>
/// 
/// <para><strong>Database Configuration:</strong></para>
/// <list type="bullet">
/// <item>Primary key: GUID with identity generation</item>
/// <item>Activity constraint: StringLength(50) with required validation</item>
/// <item>Seeded data: Six standard matter lifecycle activities</item>
/// <item>Relationships: One-to-many with MatterActivityUser for audit trails</item>
/// </list>
/// 
/// <para><strong>Matter Lifecycle Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Creation Operations:</strong> CREATED for new matter establishment</item>
/// <item><strong>Archive Operations:</strong> ARCHIVED/UNARCHIVED for matter lifecycle management</item>
/// <item><strong>Deletion Operations:</strong> DELETED/RESTORED for matter disposal and recovery</item>
/// <item><strong>Access Operations:</strong> VIEWED for matter access tracking</item>
/// </list>
/// 
/// <para><strong>Legal Compliance Support:</strong></para>
/// <list type="bullet">
/// <item>Standardized activity classification for legal audit requirements</item>
/// <item>Immutable reference data preserving audit trail integrity</item>
/// <item>Complete user attribution through MatterActivityUser associations</item>
/// <item>Support for regulatory reporting and compliance audits</item>
/// </list>
/// 
/// <para><strong>Business Rules:</strong></para>
/// <list type="bullet">
/// <item>Activity names must match MatterActivityValidationHelper.AllowedActivities</item>
/// <item>Activities are standardized and system-defined (not user-created)</item>
/// <item>Each activity can be associated with multiple users and matters</item>
/// <item>Activity names are case-insensitive but stored in uppercase</item>
/// </list>
/// 
/// <para><strong>Entity Framework Integration:</strong></para>
/// The entity is configured in AdmsContext with:
/// <list type="bullet">
/// <item>Seeded data for all standard matter lifecycle activities</item>
/// <item>Required relationships to MatterActivityUser junction entity</item>
/// <item>NoAction cascade delete to preserve audit trail integrity</item>
/// <item>Performance optimization for frequent lookup operations</item>
/// </list>
/// </remarks>
public class MatterActivity : IEquatable<MatterActivity>, IComparable<MatterActivity>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the matter activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and is automatically generated when the activity is created.
    /// For seeded activities, specific GUIDs are used to ensure consistency across deployments and
    /// enable reliable reference in business logic and reporting.
    /// 
    /// <para><strong>Database Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Primary key with identity generation</item>
    /// <item>Non-nullable and required for all operations</item>
    /// <item>Used as foreign key in MatterActivityUser relationship table</item>
    /// <item>Seeded with specific GUIDs for standard activities</item>
    /// </list>
    /// 
    /// <para><strong>Seeded Activity IDs:</strong></para>
    /// <list type="bullet">
    /// <item>CREATED: 20000000-0000-0000-0000-000000000001</item>
    /// <item>ARCHIVED: 20000000-0000-0000-0000-000000000002</item>
    /// <item>DELETED: 20000000-0000-0000-0000-000000000003</item>
    /// <item>RESTORED: 20000000-0000-0000-0000-000000000004</item>
    /// <item>UNARCHIVED: 20000000-0000-0000-0000-000000000005</item>
    /// <item>VIEWED: 20000000-0000-0000-0000-000000000006</item>
    /// </list>
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// The ID remains constant throughout the activity's lifecycle and is used for all
    /// audit trail associations, business logic references, and reporting operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new MatterActivity 
    /// { 
    ///     Activity = "CUSTOM_ACTIVITY"
    /// };
    /// // ID will be automatically generated when saved to database
    /// 
    /// // Accessing seeded activity
    /// var createdActivityId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    /// </code>
    /// </example>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name or description of the activity being undertaken.
    /// </summary>
    /// <remarks>
    /// The activity name serves as the primary identifier and classifier for matter operations.
    /// This field must conform to the standardized activity names defined in 
    /// MatterActivityValidationHelper.AllowedActivities to ensure consistency across the system.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Required field - cannot be null or empty</item>
    /// <item>Maximum length: 50 characters (database constraint)</item>
    /// <item>Minimum length: 2 characters (business rule)</item>
    /// <item>Must be one of the allowed activities from MatterActivityValidationHelper</item>
    /// <item>Must contain only letters, numbers, and underscores</item>
    /// <item>Must contain at least one letter</item>
    /// <item>Cannot use reserved activity names</item>
    /// </list>
    /// 
    /// <para><strong>Standard Activities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Matter creation activity</item>
    /// <item><strong>ARCHIVED:</strong> Matter archival activity</item>
    /// <item><strong>DELETED:</strong> Matter deletion activity</item>
    /// <item><strong>RESTORED:</strong> Matter restoration activity</item>
    /// <item><strong>UNARCHIVED:</strong> Matter unarchival activity</item>
    /// <item><strong>VIEWED:</strong> Matter viewing/access activity</item>
    /// </list>
    /// 
    /// <para><strong>Business Context:</strong></para>
    /// Activity names are used throughout the system for:
    /// <list type="bullet">
    /// <item>Matter lifecycle classification and reporting</item>
    /// <item>Business rule enforcement and workflow control</item>
    /// <item>User interface display and activity filtering</item>
    /// <item>Legal compliance reporting and analysis</item>
    /// <item>Audit trail generation and matter management</item>
    /// </list>
    /// 
    /// <para><strong>Validation Integration:</strong></para>
    /// Activity names are validated using MatterActivityValidationHelper to ensure
    /// they conform to business rules and legal compliance requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard seeded activities
    /// var createdActivity = new MatterActivity { Activity = "CREATED" };
    /// var archivedActivity = new MatterActivity { Activity = "ARCHIVED" };
    /// var deletedActivity = new MatterActivity { Activity = "DELETED" };
    /// var restoredActivity = new MatterActivity { Activity = "RESTORED" };
    /// var unarchivedActivity = new MatterActivity { Activity = "UNARCHIVED" };
    /// var viewedActivity = new MatterActivity { Activity = "VIEWED" };
    /// 
    /// // Validation example
    /// bool isValid = Common.MatterActivityValidationHelper.IsActivityAllowed(createdActivity.Activity);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Activity description is required and cannot be empty.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Activity description must be between 2 and 50 characters.")]
    public required string Activity { get; set; }

    #endregion Core Properties

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the collection of matter activity user associations for this activity.
    /// </summary>
    /// <remarks>
    /// This collection maintains the one-to-many relationship between this matter activity and
    /// the user associations. Each association represents a specific instance of this activity
    /// being performed by a user on a matter.
    /// 
    /// <para><strong>Audit Trail Functionality:</strong></para>
    /// Each association in this collection provides:
    /// <list type="bullet">
    /// <item>User attribution - which user performed this activity</item>
    /// <item>Matter context - which matter the activity was performed on</item>
    /// <item>Temporal tracking - when the activity occurred</item>
    /// <item>Unique identification - composite key preventing duplicates</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured in AdmsContext.ConfigureMatterActivityUser</item>
    /// <item>One-to-many relationship from activity to user associations</item>
    /// <item>Composite primary key includes ActivityId in the user association entity</item>
    /// <item>NoAction cascade behavior preserves audit trail integrity</item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Activity-based audit trail analysis</item>
    /// <item>User activity reporting and monitoring</item>
    /// <item>Matter lifecycle tracking</item>
    /// <item>Legal compliance and discovery support</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// The virtual modifier enables lazy loading, but consider explicit loading or
    /// projections when working with multiple activities to avoid N+1 query issues.
    /// For frequently accessed audit data, consider including related entities in queries.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing users who performed this activity
    /// foreach (var activityUser in matterActivity.MatterActivityUsers)
    /// {
    ///     Console.WriteLine($"User {activityUser.User?.Name} performed {matterActivity.Activity} " +
    ///                      $"on matter {activityUser.Matter?.Description} at {activityUser.CreatedAt}");
    /// }
    /// 
    /// // Finding all users who performed CREATED activity
    /// var createdActivity = activities.FirstOrDefault(a => a.Activity == "CREATED");
    /// var creators = createdActivity?.MatterActivityUsers
    ///     .Select(au => au.User)
    ///     .Distinct();
    /// 
    /// // Activity usage statistics
    /// var usageCount = matterActivity.MatterActivityUsers.Count;
    /// var uniqueUsers = matterActivity.MatterActivityUsers
    ///     .Select(au => au.UserId)
    ///     .Distinct()
    ///     .Count();
    /// </code>
    /// </example>
    public virtual ICollection<MatterActivityUser> MatterActivityUsers { get; set; } = new HashSet<MatterActivityUser>();

    #endregion Navigation Properties

    #region Computed Properties

    /// <summary>
    /// Gets a value indicating whether this activity has any recorded user associations.
    /// </summary>
    /// <remarks>
    /// This property is useful for determining activity usage and ensuring that
    /// activities are actually being used in the system. Activities without associations
    /// may indicate unused system features or configuration issues.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Activity usage analysis and reporting</item>
    /// <item>System configuration validation</item>
    /// <item>Feature utilization monitoring</item>
    /// <item>Data cleanup and maintenance operations</item>
    /// </list>
    /// 
    /// <para><strong>Performance Note:</strong></para>
    /// This property triggers database queries to count related entities. Consider using
    /// explicit loading or projections when working with multiple activities to avoid N+1 queries.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!matterActivity.HasUserAssociations)
    /// {
    ///     logger.LogInformation($"Activity {matterActivity.Activity} has no user associations");
    /// }
    /// 
    /// // Finding unused activities
    /// var unusedActivities = activities.Where(a => !a.HasUserAssociations).ToList();
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasUserAssociations => MatterActivityUsers.Count > 0;

    /// <summary>
    /// Gets the total count of user associations for this activity.
    /// </summary>
    /// <remarks>
    /// This computed property provides insight into the frequency of use for this activity
    /// type, useful for activity monitoring, usage analytics, and system optimization.
    /// 
    /// <para><strong>Performance Note:</strong></para>
    /// This property triggers database queries to count related entities. Consider using
    /// explicit loading or projections when working with multiple activities to avoid N+1 queries.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Activity {matterActivity.Activity} has been used {matterActivity.UsageCount} times");
    /// 
    /// // Finding most used activities
    /// var mostUsedActivities = activities
    ///     .OrderByDescending(a => a.UsageCount)
    ///     .Take(10);
    /// </code>
    /// </example>
    [NotMapped]
    public int UsageCount => MatterActivityUsers.Count;

    /// <summary>
    /// Gets the count of unique users who have performed this activity.
    /// </summary>
    /// <remarks>
    /// This computed property provides insight into the breadth of user engagement
    /// with this activity type, useful for user adoption analysis and training needs assessment.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Activity {matterActivity.Activity} has been performed by {matterActivity.UniqueUserCount} different users");
    /// 
    /// // Analyzing user engagement
    /// var engagementMetric = matterActivity.UniqueUserCount / (double)matterActivity.UsageCount;
    /// </code>
    /// </example>
    [NotMapped]
    public int UniqueUserCount => MatterActivityUsers
        .Select(au => au.UserId)
        .Distinct()
        .Count();

    /// <summary>
    /// Gets the count of unique matters this activity has been performed on.
    /// </summary>
    /// <remarks>
    /// This computed property provides insight into the breadth of matter engagement
    /// with this activity type, useful for matter lifecycle analysis and system usage patterns.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Activity {matterActivity.Activity} has been performed on {matterActivity.UniqueMatterCount} different matters");
    /// </code>
    /// </example>
    [NotMapped]
    public int UniqueMatterCount => MatterActivityUsers
        .Select(au => au.MatterId)
        .Distinct()
        .Count();

    /// <summary>
    /// Gets a value indicating whether this activity is one of the standard seeded activities.
    /// </summary>
    /// <remarks>
    /// This property identifies whether the activity is one of the standard system-defined
    /// activities or a custom activity. Standard activities are those seeded in the database
    /// and validated by MatterActivityValidationHelper.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matterActivity.IsStandardActivity)
    /// {
    ///     // Apply standard business rules
    /// }
    /// else
    /// {
    ///     // Handle custom activity logic
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool IsStandardActivity =>
        Common.MatterActivityValidationHelper.IsActivityAllowed(Activity);

    /// <summary>
    /// Gets the normalized version of the activity name.
    /// </summary>
    /// <remarks>
    /// This property provides a normalized version of the activity name following
    /// the normalization rules from MatterActivityValidationHelper for consistent
    /// comparison and storage operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var normalized = matterActivity.NormalizedActivity; // Always uppercase, trimmed
    /// bool areEqual = matterActivity.NormalizedActivity == other.NormalizedActivity;
    /// </code>
    /// </example>
    [NotMapped]
    public string NormalizedActivity =>
        Common.MatterActivityValidationHelper.NormalizeActivity(Activity) ?? Activity.ToUpperInvariant();

    /// <summary>
    /// Gets the activity category for classification purposes.
    /// </summary>
    /// <remarks>
    /// This property categorizes the activity based on its type for reporting and analysis purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var category = matterActivity.ActivityCategory;
    /// // Returns: "Lifecycle", "Archive", "Access", or "Unknown"
    /// </code>
    /// </example>
    [NotMapped]
    public string ActivityCategory => Activity.ToUpperInvariant() switch
    {
        "CREATED" or "DELETED" or "RESTORED" => "Lifecycle",
        "ARCHIVED" or "UNARCHIVED" => "Archive",
        "VIEWED" => "Access",
        _ => "Unknown"
    };

    #endregion Computed Properties

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified MatterActivity is equal to the current MatterActivity.
    /// </summary>
    /// <param name="other">The MatterActivity to compare with the current MatterActivity.</param>
    /// <returns>true if the specified MatterActivity is equal to the current MatterActivity; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each activity has a unique identifier.
    /// This follows Entity Framework best practices for entity equality comparison.
    /// </remarks>
    public bool Equals(MatterActivity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current MatterActivity.
    /// </summary>
    /// <param name="obj">The object to compare with the current MatterActivity.</param>
    /// <returns>true if the specified object is equal to the current MatterActivity; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as MatterActivity);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterActivity.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Compares the current MatterActivity with another MatterActivity for ordering purposes.
    /// </summary>
    /// <param name="other">The MatterActivity to compare with the current MatterActivity.</param>
    /// <returns>
    /// A value that indicates the relative order of the activities being compared.
    /// Less than zero: This activity precedes the other activity.
    /// Zero: This activity occurs in the same position as the other activity.
    /// Greater than zero: This activity follows the other activity.
    /// </returns>
    /// <remarks>
    /// Comparison is performed based on activity name for alphabetical ordering,
    /// which is most useful for display and reporting purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Sort activities alphabetically
    /// var sortedActivities = activities.OrderBy(a => a).ToList();
    /// 
    /// // Compare specific activities
    /// if (activity1.CompareTo(activity2) < 0)
    /// {
    ///     Console.WriteLine($"Activity {activity1.Activity} comes before {activity2.Activity}");
    /// }
    /// </code>
    /// </example>
    public int CompareTo(MatterActivity? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by activity name for alphabetical ordering
        var activityComparison = string.Compare(Activity, other.Activity, StringComparison.OrdinalIgnoreCase);
        if (activityComparison != 0) return activityComparison;

        // Secondary sort by ID for consistency when activities have same name
        return Id.CompareTo(other.Id);
    }

    /// <summary>
    /// Determines whether two MatterActivity instances are equal.
    /// </summary>
    /// <param name="left">The first MatterActivity to compare.</param>
    /// <param name="right">The second MatterActivity to compare.</param>
    /// <returns>true if the MatterActivities are equal; otherwise, false.</returns>
    public static bool operator ==(MatterActivity? left, MatterActivity? right) =>
        EqualityComparer<MatterActivity>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two MatterActivity instances are not equal.
    /// </summary>
    /// <param name="left">The first MatterActivity to compare.</param>
    /// <param name="right">The second MatterActivity to compare.</param>
    /// <returns>true if the MatterActivities are not equal; otherwise, false.</returns>
    public static bool operator !=(MatterActivity? left, MatterActivity? right) => !(left == right);

    /// <summary>
    /// Determines whether one MatterActivity precedes another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterActivity to compare.</param>
    /// <param name="right">The second MatterActivity to compare.</param>
    /// <returns>true if the left MatterActivity precedes the right MatterActivity; otherwise, false.</returns>
    public static bool operator <(MatterActivity? left, MatterActivity? right) =>
        left is not null && (right is null || left.CompareTo(right) < 0);

    /// <summary>
    /// Determines whether one MatterActivity precedes or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterActivity to compare.</param>
    /// <param name="right">The second MatterActivity to compare.</param>
    /// <returns>true if the left MatterActivity precedes or equals the right MatterActivity; otherwise, false.</returns>
    public static bool operator <=(MatterActivity? left, MatterActivity? right) =>
        left is null || (right is not null && left.CompareTo(right) <= 0);

    /// <summary>
    /// Determines whether one MatterActivity follows another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterActivity to compare.</param>
    /// <param name="right">The second MatterActivity to compare.</param>
    /// <returns>true if the left MatterActivity follows the right MatterActivity; otherwise, false.</returns>
    public static bool operator >(MatterActivity? left, MatterActivity? right) =>
        left is not null && (right is null || left.CompareTo(right) > 0);

    /// <summary>
    /// Determines whether one MatterActivity follows or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterActivity to compare.</param>
    /// <param name="right">The second MatterActivity to compare.</param>
    /// <returns>true if the left MatterActivity follows or equals the right MatterActivity; otherwise, false.</returns>
    public static bool operator >=(MatterActivity? left, MatterActivity? right) =>
        left is null || (right is not null && left.CompareTo(right) >= 0);

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterActivity.
    /// </summary>
    /// <returns>A string that represents the current MatterActivity.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the activity,
    /// which is useful for debugging, logging, and display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new MatterActivity 
    /// { 
    ///     Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), 
    ///     Activity = "CREATED"
    /// };
    /// 
    /// Console.WriteLine(activity);
    /// // Output: "MatterActivity: CREATED (20000000-0000-0000-0000-000000000001)"
    /// </code>
    /// </example>
    public override string ToString() => $"MatterActivity: {Activity} ({Id})";

    #endregion String Representation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this activity represents a creation operation.
    /// </summary>
    /// <returns>true if this is a creation activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify creation activities for
    /// business rule enforcement and audit trail analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matterActivity.IsCreationActivity())
    /// {
    ///     // Apply creation-specific business rules
    ///     Console.WriteLine("This is a matter creation activity");
    /// }
    /// </code>
    /// </example>
    public bool IsCreationActivity() =>
        string.Equals(Activity, "CREATED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents an archival operation.
    /// </summary>
    /// <returns>true if this is an archival activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify archival activities for
    /// audit trail analysis and business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matterActivity.IsArchivalActivity())
    /// {
    ///     // Apply archival-specific business rules
    ///     archiveLogger.LogArchival(matterActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsArchivalActivity() =>
        string.Equals(Activity, "ARCHIVED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a deletion operation.
    /// </summary>
    /// <returns>true if this is a deletion activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify deletion activities for
    /// audit trail analysis and business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matterActivity.IsDeletionActivity())
    /// {
    ///     // Apply deletion-specific business rules
    ///     auditLogger.LogDeletion(matterActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsDeletionActivity() =>
        string.Equals(Activity, "DELETED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a restoration operation.
    /// </summary>
    /// <returns>true if this is a restoration activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify restoration activities for
    /// audit trail analysis and recovery operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matterActivity.IsRestorationActivity())
    /// {
    ///     // Apply restoration-specific business rules
    ///     recoveryLogger.LogRestoration(matterActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsRestorationActivity() =>
        string.Equals(Activity, "RESTORED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents an unarchival operation.
    /// </summary>
    /// <returns>true if this is an unarchival activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify unarchival activities for
    /// audit trail analysis and business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matterActivity.IsUnarchivalActivity())
    /// {
    ///     // Apply unarchival-specific business rules
    ///     unarchiveLogger.LogUnarchival(matterActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsUnarchivalActivity() =>
        string.Equals(Activity, "UNARCHIVED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a viewing operation.
    /// </summary>
    /// <returns>true if this is a viewing activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify viewing activities for
    /// access tracking and analytics purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matterActivity.IsViewingActivity())
    /// {
    ///     // Apply viewing-specific business rules
    ///     accessTracker.LogAccess(matterActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsViewingActivity() =>
        string.Equals(Activity, "VIEWED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity is appropriate for the given matter context.
    /// </summary>
    /// <param name="isArchived">Whether the matter is currently archived.</param>
    /// <param name="isDeleted">Whether the matter is currently deleted.</param>
    /// <returns>true if the activity is appropriate for the context; otherwise, false.</returns>
    /// <remarks>
    /// This method uses the MatterActivityValidationHelper to validate business rules
    /// for activity context appropriateness.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool canApply = activity.IsAppropriateForMatterStatus(false, false);  // true for normal matter
    /// bool cannotApply = activity.IsAppropriateForMatterStatus(true, false); // false for archived if trying to archive again
    /// </code>
    /// </example>
    public bool IsAppropriateForMatterStatus(bool isArchived, bool isDeleted) =>
        Common.MatterActivityValidationHelper.IsActivityAppropriateForMatterStatus(Activity, isArchived, isDeleted);

    /// <summary>
    /// Gets the seeded GUID for a specific activity name.
    /// </summary>
    /// <param name="activityName">The activity name to get the GUID for.</param>
    /// <returns>The seeded GUID if found; otherwise, Guid.Empty.</returns>
    /// <remarks>
    /// This method returns the specific GUIDs used in database seeding for standard activities,
    /// useful for business logic that needs to reference specific activity types.
    /// </remarks>
    /// <example>
    /// <code>
    /// var createdActivityId = MatterActivity.GetSeededActivityId("CREATED");
    /// // Returns: 20000000-0000-0000-0000-000000000001
    /// 
    /// var archivedActivityId = MatterActivity.GetSeededActivityId("ARCHIVED");
    /// // Returns: 20000000-0000-0000-0000-000000000002
    /// </code>
    /// </example>
    public static Guid GetSeededActivityId(string activityName)
    {
        if (string.IsNullOrWhiteSpace(activityName))
            return Guid.Empty;

        return activityName.Trim().ToUpperInvariant() switch
        {
            "CREATED" => Guid.Parse("20000000-0000-0000-0000-000000000001"),
            "ARCHIVED" => Guid.Parse("20000000-0000-0000-0000-000000000002"),
            "DELETED" => Guid.Parse("20000000-0000-0000-0000-000000000003"),
            "RESTORED" => Guid.Parse("20000000-0000-0000-0000-000000000004"),
            "UNARCHIVED" => Guid.Parse("20000000-0000-0000-0000-000000000005"),
            "VIEWED" => Guid.Parse("20000000-0000-0000-0000-000000000006"),
            _ => Guid.Empty
        };
    }

    /// <summary>
    /// Gets usage statistics for this activity.
    /// </summary>
    /// <returns>A dictionary containing usage statistics.</returns>
    /// <remarks>
    /// This method provides insights into the usage patterns and statistics of this activity
    /// for reporting and analysis purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = matterActivity.GetUsageStatistics();
    /// Console.WriteLine($"Total usage: {stats["TotalUsage"]}");
    /// Console.WriteLine($"Unique users: {stats["UniqueUsers"]}");
    /// Console.WriteLine($"Unique matters: {stats["UniqueMatters"]}");
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
            ["HasUserAssociations"] = HasUserAssociations
        };
    }

    #endregion Business Logic Methods

    #region Validation Methods

    /// <summary>
    /// Validates the current MatterActivity instance against business rules.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive validation using MatterActivityValidationHelper
    /// including activity name validation, length constraints, and business rule compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// var validationResults = matterActivity.Validate();
    /// if (validationResults.Any())
    /// {
    ///     foreach (var error in validationResults)
    ///     {
    ///         Console.WriteLine($"Validation Error: {error.ErrorMessage}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public IEnumerable<ValidationResult> Validate()
    {
        var results = new List<ValidationResult>();

        // Validate using MatterActivityValidationHelper
        results.AddRange(Common.MatterActivityValidationHelper.ValidateActivity(Activity, nameof(Activity)));

        // Additional entity-specific validations
        if (Id == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Activity ID cannot be empty.",
                [nameof(Id)]));
        }

        return results;
    }

    #endregion Validation Methods
}