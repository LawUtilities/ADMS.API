using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Entities;

/// <summary>
/// Represents the association between a matter, a matter activity, and a user in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// The MatterActivityUser entity serves as a junction table implementing the many-to-many relationship
/// between matters, matter activities, and users. This entity is critical for maintaining comprehensive
/// audit trails of all matter-related operations within the legal document management system.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Audit Trail Foundation:</strong> Central component of matter activity tracking</item>
/// <item><strong>Composite Primary Key:</strong> Ensures uniqueness while allowing temporal tracking</item>
/// <item><strong>User Attribution:</strong> Links every matter activity to a responsible user</item>
/// <item><strong>Temporal Tracking:</strong> Maintains precise timestamps for all activities</item>
/// <item><strong>Legal Compliance:</strong> Supports comprehensive audit requirements</item>
/// </list>
/// 
/// <para><strong>Database Configuration:</strong></para>
/// <list type="bullet">
/// <item>Composite primary key: MatterId + MatterActivityId + UserId + CreatedAt</item>
/// <item>All foreign key relationships are required</item>
/// <item>NoAction cascade delete to preserve audit trail integrity</item>
/// <item>Indexed on CreatedAt for temporal queries</item>
/// </list>
/// 
/// <para><strong>Matter Activities Supported:</strong></para>
/// This entity tracks various matter lifecycle activities:
/// <list type="bullet">
/// <item><strong>CREATED:</strong> Matter creation activity</item>
/// <item><strong>ARCHIVED:</strong> Matter archival activity</item>
/// <item><strong>DELETED:</strong> Matter deletion activity</item>
/// <item><strong>RESTORED:</strong> Matter restoration activity</item>
/// <item><strong>UNARCHIVED:</strong> Matter unarchival activity</item>
/// <item><strong>VIEWED:</strong> Matter access activity</item>
/// </list>
/// 
/// <para><strong>Audit Trail Functionality:</strong></para>
/// This entity enables comprehensive tracking of matter activities:
/// <list type="bullet">
/// <item><strong>Who:</strong> Which user performed the matter activity</item>
/// <item><strong>What:</strong> What type of activity was performed (CREATED/ARCHIVED/etc.)</item>
/// <item><strong>Where:</strong> Which matter the activity was performed on</item>
/// <item><strong>When:</strong> Precise timestamp of the activity</item>
/// </list>
/// 
/// <para><strong>Legal Compliance Support:</strong></para>
/// <list type="bullet">
/// <item>Complete user attribution for all matter operations</item>
/// <item>Temporal audit trails for legal discovery and compliance</item>
/// <item>Immutable record of matter lifecycle activities</item>
/// <item>Support for regulatory reporting and audit requirements</item>
/// </list>
/// 
/// <para><strong>Business Rules:</strong></para>
/// <list type="bullet">
/// <item>Every matter activity must be attributed to a user</item>
/// <item>Multiple activities of the same type can occur with different timestamps</item>
/// <item>CreatedAt timestamp must be within reasonable bounds</item>
/// <item>All foreign key relationships must reference valid entities</item>
/// </list>
/// 
/// <para><strong>Entity Framework Configuration:</strong></para>
/// The entity is configured in AdmsContext.ConfigureMatterActivityUser with:
/// <list type="bullet">
/// <item>Composite key including all four core properties</item>
/// <item>Required relationships to Matter, MatterActivity, and User</item>
/// <item>NoAction cascade behaviors to maintain audit trail integrity</item>
/// <item>Performance indexes on commonly queried fields</item>
/// </list>
/// </remarks>
public class MatterActivityUser : IEquatable<MatterActivityUser>, IComparable<MatterActivityUser>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier of the matter.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the matter that this activity
    /// is associated with. It forms part of the composite primary key and is required for all
    /// matter activity user associations.
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to Matter entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>NoAction cascade delete to preserve audit trail integrity</item>
    /// </list>
    /// 
    /// <para><strong>Business Context:</strong></para>
    /// <list type="bullet">
    /// <item>Represents the matter on which the activity was performed</item>
    /// <item>Links activity to specific legal case or project</item>
    /// <item>Enables matter-scoped activity queries</item>
    /// <item>Supports matter-level audit trail aggregation</item>
    /// </list>
    /// 
    /// <para><strong>Legal Significance:</strong></para>
    /// The matter ID is crucial for legal document management as it establishes the context
    /// within which activities occur, supporting case management and legal compliance
    /// requirements for matter operation tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityUser = new MatterActivityUser
    /// {
    ///     MatterId = legalMatter.Id,  // Must reference valid matter
    ///     MatterActivityId = activityId,
    ///     UserId = performingUser.Id,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter ID is required for matter association.")]
    public Guid MatterId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the matter activity.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the matter activity type
    /// that was performed. It forms part of the composite primary key and must reference
    /// a valid matter activity from the seeded data.
    /// 
    /// <para><strong>Activity Types:</strong></para>
    /// Must reference one of the seeded matter activities:
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Matter creation activity</item>
    /// <item><strong>ARCHIVED:</strong> Matter archival activity</item>
    /// <item><strong>DELETED:</strong> Matter deletion activity</item>
    /// <item><strong>RESTORED:</strong> Matter restoration activity</item>
    /// <item><strong>UNARCHIVED:</strong> Matter unarchival activity</item>
    /// <item><strong>VIEWED:</strong> Matter access activity</item>
    /// </list>
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to MatterActivity entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>NoAction cascade delete to preserve audit integrity</item>
    /// </list>
    /// 
    /// <para><strong>Validation Integration:</strong></para>
    /// The MatterActivityId is validated using MatterActivityValidationHelper
    /// to ensure it references a valid, allowed activity type.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Linking to a CREATED activity (seeded ID: 20000000-0000-0000-0000-000000000001)
    /// var createdActivityId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    /// var activityUser = new MatterActivityUser
    /// {
    ///     MatterActivityId = createdActivityId,  // Must be valid activity
    ///     // ... other properties
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter Activity ID is required for activity classification.")]
    public Guid MatterActivityId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who performed the activity.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the user who performed
    /// the matter activity. It forms part of the composite primary key and provides
    /// essential user attribution for audit trail purposes.
    /// 
    /// <para><strong>User Attribution:</strong></para>
    /// <list type="bullet">
    /// <item>Links activity to responsible user for accountability</item>
    /// <item>Enables user-scoped matter activity reporting</item>
    /// <item>Supports legal compliance and audit requirements</item>
    /// <item>Facilitates user activity analytics and monitoring</item>
    /// </list>
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to User entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>NoAction cascade delete to preserve audit integrity</item>
    /// </list>
    /// 
    /// <para><strong>Legal Significance:</strong></para>
    /// User attribution is critical for:
    /// <list type="bullet">
    /// <item>Legal discovery and compliance audits</item>
    /// <item>Professional responsibility tracking</item>
    /// <item>Accountability in legal matter management</item>
    /// <item>Evidence of who performed specific matter activities</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Attributing matter activity to a specific user
    /// var activityUser = new MatterActivityUser
    /// {
    ///     UserId = currentUser.Id,  // Must reference valid user
    ///     MatterId = matter.Id,
    ///     MatterActivityId = activityId,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User ID is required for user attribution.")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entry was created (in UTC).
    /// </summary>
    /// <remarks>
    /// This property maintains the precise timestamp of when the matter activity occurred.
    /// It forms part of the composite primary key, enabling multiple activities of the same
    /// type with different timestamps while maintaining uniqueness.
    /// 
    /// <para><strong>Temporal Tracking:</strong></para>
    /// <list type="bullet">
    /// <item>Provides precise timing for matter activity audit trails</item>
    /// <item>Enables temporal analysis of matter lifecycle patterns</item>
    /// <item>Supports legal timeline reconstruction for case management</item>
    /// <item>Facilitates workflow and process analysis</item>
    /// </list>
    /// 
    /// <para><strong>Date Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Must be stored in UTC format for consistency</item>
    /// <item>Cannot be earlier than system minimum date</item>
    /// <item>Cannot be in the future (with tolerance for clock skew)</item>
    /// <item>Forms part of composite primary key for uniqueness</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Automatically set to current UTC time when not specified</item>
    /// <item>Should reflect the actual time the matter activity occurred</item>
    /// <item>Used for chronological ordering in audit reports</item>
    /// <item>Critical for legal compliance and matter activity tracking</item>
    /// </list>
    /// 
    /// <para><strong>Validation:</strong></para>
    /// The CreatedAt timestamp is validated using RevisionValidationHelper date validation
    /// methods to ensure it falls within acceptable ranges for legal document management.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityUser = new MatterActivityUser
    /// {
    ///     CreatedAt = DateTime.UtcNow,  // Always use UTC
    ///     // ... other properties
    /// };
    /// 
    /// // For historical data import
    /// var historicalActivity = new MatterActivityUser
    /// {
    ///     CreatedAt = specificUtcDateTime,  // Specific historical timestamp
    ///     // ... other properties
    /// };
    /// </code>
    /// </example>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    #endregion Core Properties

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the matter associated with this activity.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the matter on which the activity was
    /// performed. The relationship is established through the MatterId foreign key and
    /// enables rich querying and navigation within Entity Framework.
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses MatterId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>NoAction cascade delete behavior preserves audit trail integrity</item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Accessing matter details from activity context</item>
    /// <item>Matter-level activity operations and reporting</item>
    /// <item>Audit trail reporting with matter information</item>
    /// <item>Cross-matter activity analysis</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// The virtual modifier enables lazy loading, but consider explicit loading or
    /// projections when working with multiple activities to avoid N+1 query issues.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing matter information through activity
    /// var matterDescription = activityUser.Matter?.Description;
    /// var matterCreationDate = activityUser.Matter?.CreationDate;
    /// 
    /// // Using explicit loading to avoid N+1 queries
    /// await context.Entry(activityUser)
    ///     .Reference(a => a.Matter)
    ///     .LoadAsync();
    /// </code>
    /// </example>
    [ForeignKey(nameof(MatterId))]
    public virtual required Matter Matter { get; set; }

    /// <summary>
    /// Gets or sets the matter activity associated with this user.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the matter activity type that was performed.
    /// The relationship is established through the MatterActivityId foreign key and enables
    /// access to activity metadata such as the activity name and description.
    /// 
    /// <para><strong>Activity Information Access:</strong></para>
    /// Provides access to:
    /// <list type="bullet">
    /// <item>Activity name (CREATED, ARCHIVED, DELETED, etc.)</item>
    /// <item>Activity metadata and configuration</item>
    /// <item>Activity validation rules and constraints</item>
    /// <item>Cross-activity analysis and reporting</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses MatterActivityId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>NoAction cascade delete to preserve audit integrity</item>
    /// </list>
    /// 
    /// <para><strong>Business Logic Integration:</strong></para>
    /// <list type="bullet">
    /// <item>Activity-specific business rule enforcement</item>
    /// <item>Activity categorization and reporting</item>
    /// <item>Workflow and process analysis</item>
    /// <item>Activity-based authorization and permissions</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing activity type information
    /// var activityName = activityUser.MatterActivity?.Activity;
    /// var isCreationActivity = activityName == "CREATED";
    /// 
    /// // Activity-based filtering
    /// var creationActivities = activities
    ///     .Where(a => a.MatterActivity?.Activity == "CREATED");
    /// </code>
    /// </example>
    [ForeignKey(nameof(MatterActivityId))]
    public virtual required MatterActivity MatterActivity { get; set; }

    /// <summary>
    /// Gets or sets the user associated with this matter activity.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the user who performed the matter activity.
    /// The relationship is established through the UserId foreign key and enables comprehensive
    /// user-based reporting and analysis of matter activities.
    /// 
    /// <para><strong>User Information Access:</strong></para>
    /// Provides access to:
    /// <list type="bullet">
    /// <item>User identification and contact information</item>
    /// <item>User activity patterns and behavior</item>
    /// <item>Professional attribution for legal compliance</item>
    /// <item>User-based reporting and analytics</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses UserId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>NoAction cascade delete to preserve audit integrity</item>
    /// </list>
    /// 
    /// <para><strong>Legal and Compliance Support:</strong></para>
    /// <list type="bullet">
    /// <item>User accountability for matter activities</item>
    /// <item>Professional responsibility tracking</item>
    /// <item>Legal discovery and audit trail support</item>
    /// <item>Compliance reporting and analysis</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing user information through activity
    /// var userName = activityUser.User?.Name;
    /// var userActivities = activityUser.User?.MatterActivityUsers;
    /// 
    /// // User-based activity analysis
    /// var userCreationCount = user.MatterActivityUsers
    ///     .Count(au => au.MatterActivity?.Activity == "CREATED");
    /// </code>
    /// </example>
    [ForeignKey(nameof(UserId))]
    public virtual required User User { get; set; }

    #endregion Navigation Properties

    #region Computed Properties

    /// <summary>
    /// Gets a value indicating whether this activity record has valid foreign key references.
    /// </summary>
    /// <remarks>
    /// This computed property validates that all required foreign key properties contain
    /// valid (non-empty) GUID values, ensuring referential integrity.
    /// 
    /// <para><strong>Validation Checks:</strong></para>
    /// <list type="bullet">
    /// <item>MatterId is not Guid.Empty</item>
    /// <item>MatterActivityId is not Guid.Empty</item>
    /// <item>UserId is not Guid.Empty</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!activityUser.HasValidReferences)
    /// {
    ///     throw new InvalidOperationException("Activity user has invalid foreign key references");
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasValidReferences =>
        MatterId != Guid.Empty &&
        MatterActivityId != Guid.Empty &&
        UserId != Guid.Empty;

    /// <summary>
    /// Gets a value indicating whether the CreatedAt timestamp is within reasonable bounds.
    /// </summary>
    /// <remarks>
    /// This computed property uses the RevisionValidationHelper to validate that the
    /// CreatedAt timestamp falls within acceptable date ranges for the system.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!activityUser.HasValidTimestamp)
    /// {
    ///     logger.LogWarning($"Activity user {activityUser} has invalid timestamp: {activityUser.CreatedAt}");
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasValidTimestamp =>
        Common.RevisionValidationHelper.IsValidDate(CreatedAt);

    /// <summary>
    /// Gets the age of this activity record in days.
    /// </summary>
    /// <remarks>
    /// This computed property calculates the number of days between the activity creation
    /// time and the current UTC time, providing insight into the age of audit trail records.
    /// </remarks>
    /// <example>
    /// <code>
    /// var recentActivities = activities.Where(a => a.AgeDays <= 30);
    /// Console.WriteLine($"Activity is {activityUser.AgeDays} days old");
    /// </code>
    /// </example>
    [NotMapped]
    public double AgeDays => (DateTime.UtcNow - CreatedAt).TotalDays;

    /// <summary>
    /// Gets the formatted activity description for display purposes.
    /// </summary>
    /// <remarks>
    /// This computed property provides a human-readable description of the matter activity
    /// including user attribution, activity type, and timestamp information.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine(activityUser.ActivityDescription);
    /// // Output: "rbrown performed CREATED on matter 'Corporate Case' at 2024-01-15 10:30 UTC"
    /// </code>
    /// </example>
    [NotMapped]
    public string ActivityDescription =>
        $"{User?.Name ?? "Unknown User"} performed {MatterActivity?.Activity ?? "Unknown Activity"} " +
        $"on matter '{Matter?.Description ?? "Unknown"}' at {CreatedAt:yyyy-MM-dd HH:mm} UTC";

    #endregion Computed Properties

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified MatterActivityUser is equal to the current MatterActivityUser.
    /// </summary>
    /// <param name="other">The MatterActivityUser to compare with the current MatterActivityUser.</param>
    /// <returns>true if the specified MatterActivityUser is equal to the current MatterActivityUser; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all four components of the composite primary key:
    /// MatterId, MatterActivityId, UserId, and CreatedAt. This follows Entity Framework
    /// best practices for entities with composite keys.
    /// </remarks>
    public bool Equals(MatterActivityUser? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return MatterId.Equals(other.MatterId) &&
               MatterActivityId.Equals(other.MatterActivityId) &&
               UserId.Equals(other.UserId) &&
               CreatedAt.Equals(other.CreatedAt);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current MatterActivityUser.
    /// </summary>
    /// <param name="obj">The object to compare with the current MatterActivityUser.</param>
    /// <returns>true if the specified object is equal to the current MatterActivityUser; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as MatterActivityUser);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterActivityUser.</returns>
    /// <remarks>
    /// The hash code is computed from all four components of the composite primary key
    /// to ensure consistent hashing behavior that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => HashCode.Combine(MatterId, MatterActivityId, UserId, CreatedAt);

    /// <summary>
    /// Compares the current MatterActivityUser with another MatterActivityUser for ordering purposes.
    /// </summary>
    /// <param name="other">The MatterActivityUser to compare with the current MatterActivityUser.</param>
    /// <returns>
    /// A value that indicates the relative order of the activity records being compared.
    /// Less than zero: This activity precedes the other activity.
    /// Zero: This activity occurs in the same position as the other activity.
    /// Greater than zero: This activity follows the other activity.
    /// </returns>
    /// <remarks>
    /// Comparison is performed based on CreatedAt timestamp for chronological ordering,
    /// which is most useful for audit trail analysis and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Sort activities chronologically
    /// var sortedActivities = activities.OrderBy(a => a).ToList();
    /// 
    /// // Compare specific activities
    /// if (activity1.CompareTo(activity2) < 0)
    /// {
    ///     Console.WriteLine($"Activity1 occurred before Activity2");
    /// }
    /// </code>
    /// </example>
    public int CompareTo(MatterActivityUser? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by timestamp for chronological ordering
        var timeComparison = CreatedAt.CompareTo(other.CreatedAt);
        if (timeComparison != 0) return timeComparison;

        // Secondary sort by MatterId for consistency
        var matterComparison = MatterId.CompareTo(other.MatterId);
        if (matterComparison != 0) return matterComparison;

        // Tertiary sort by ActivityId
        var activityComparison = MatterActivityId.CompareTo(other.MatterActivityId);
        return activityComparison != 0 ? activityComparison :
            // Final sort by UserId
            UserId.CompareTo(other.UserId);
    }

    /// <summary>
    /// Determines whether two MatterActivityUser instances are equal.
    /// </summary>
    /// <param name="left">The first MatterActivityUser to compare.</param>
    /// <param name="right">The second MatterActivityUser to compare.</param>
    /// <returns>true if the MatterActivityUsers are equal; otherwise, false.</returns>
    public static bool operator ==(MatterActivityUser? left, MatterActivityUser? right) =>
        EqualityComparer<MatterActivityUser>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two MatterActivityUser instances are not equal.
    /// </summary>
    /// <param name="left">The first MatterActivityUser to compare.</param>
    /// <param name="right">The second MatterActivityUser to compare.</param>
    /// <returns>true if the MatterActivityUsers are not equal; otherwise, false.</returns>
    public static bool operator !=(MatterActivityUser? left, MatterActivityUser? right) => !(left == right);

    /// <summary>
    /// Determines whether one MatterActivityUser precedes another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterActivityUser to compare.</param>
    /// <param name="right">The second MatterActivityUser to compare.</param>
    /// <returns>true if the left MatterActivityUser precedes the right MatterActivityUser; otherwise, false.</returns>
    public static bool operator <(MatterActivityUser? left, MatterActivityUser? right) =>
        left is not null && (right is null || left.CompareTo(right) < 0);

    /// <summary>
    /// Determines whether one MatterActivityUser precedes or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterActivityUser to compare.</param>
    /// <param name="right">The second MatterActivityUser to compare.</param>
    /// <returns>true if the left MatterActivityUser precedes or equals the right MatterActivityUser; otherwise, false.</returns>
    public static bool operator <=(MatterActivityUser? left, MatterActivityUser? right) =>
        left is null || (right is not null && left.CompareTo(right) <= 0);

    /// <summary>
    /// Determines whether one MatterActivityUser follows another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterActivityUser to compare.</param>
    /// <param name="right">The second MatterActivityUser to compare.</param>
    /// <returns>true if the left MatterActivityUser follows the right MatterActivityUser; otherwise, false.</returns>
    public static bool operator >(MatterActivityUser? left, MatterActivityUser? right) =>
        left is not null && (right is null || left.CompareTo(right) > 0);

    /// <summary>
    /// Determines whether one MatterActivityUser follows or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterActivityUser to compare.</param>
    /// <param name="right">The second MatterActivityUser to compare.</param>
    /// <returns>true if the left MatterActivityUser follows or equals the right MatterActivityUser; otherwise, false.</returns>
    public static bool operator >=(MatterActivityUser? left, MatterActivityUser? right) =>
        left is null || (right is not null && left.CompareTo(right) >= 0);

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterActivityUser.
    /// </summary>
    /// <returns>A string that represents the current MatterActivityUser.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the activity,
    /// which is useful for debugging, logging, and audit trail display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityUser = new MatterActivityUser
    /// {
    ///     MatterId = matterGuid,
    ///     MatterActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// Console.WriteLine(activityUser);
    /// // Output: "User rbrown performed CREATED on Matter 'Corporate Case' at 2024-01-15 10:30:00 UTC"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"User {User?.Name ?? UserId.ToString()[..8]} performed {MatterActivity?.Activity ?? "Activity"} " +
        $"on Matter '{Matter?.Description ?? MatterId.ToString()[..8]}' at {CreatedAt:yyyy-MM-dd HH:mm:ss} UTC";

    #endregion String Representation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this activity record represents a creation activity.
    /// </summary>
    /// <returns>true if this is a creation activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify creation activities without
    /// directly accessing the navigation property, which can help avoid lazy loading
    /// in performance-sensitive scenarios.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityUser.IsCreationActivity())
    /// {
    ///     // Handle creation-specific logic
    ///     Console.WriteLine("This matter was created by: " + activityUser.User?.Name);
    /// }
    /// </code>
    /// </example>
    public bool IsCreationActivity() => MatterActivity?.Activity == "CREATED";

    /// <summary>
    /// Determines whether this activity record represents an archival activity.
    /// </summary>
    /// <returns>true if this is an archival activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify archival activities for
    /// audit trail analysis and business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityUser.IsArchivalActivity())
    /// {
    ///     // Handle archival-specific logic
    ///     archiveLogger.LogArchival(activityUser);
    /// }
    /// </code>
    /// </example>
    public bool IsArchivalActivity() => MatterActivity?.Activity == "ARCHIVED";

    /// <summary>
    /// Determines whether this activity record represents a deletion activity.
    /// </summary>
    /// <returns>true if this is a deletion activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify deletion activities for
    /// audit trail analysis and business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityUser.IsDeletionActivity())
    /// {
    ///     // Handle deletion-specific logic
    ///     auditLogger.LogDeletion(activityUser);
    /// }
    /// </code>
    /// </example>
    public bool IsDeletionActivity() => MatterActivity?.Activity == "DELETED";

    /// <summary>
    /// Determines whether this activity record represents a restoration activity.
    /// </summary>
    /// <returns>true if this is a restoration activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify restoration activities for
    /// audit trail analysis and recovery operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityUser.IsRestorationActivity())
    /// {
    ///     // Handle restoration-specific logic
    ///     recoveryLogger.LogRestoration(activityUser);
    /// }
    /// </code>
    /// </example>
    public bool IsRestorationActivity() => MatterActivity?.Activity == "RESTORED";

    /// <summary>
    /// Determines whether this activity record represents an unarchival activity.
    /// </summary>
    /// <returns>true if this is an unarchival activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify unarchival activities for
    /// audit trail analysis and business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityUser.IsUnarchivalActivity())
    /// {
    ///     // Handle unarchival-specific logic
    ///     unarchiveLogger.LogUnarchival(activityUser);
    /// }
    /// </code>
    /// </example>
    public bool IsUnarchivalActivity() => MatterActivity?.Activity == "UNARCHIVED";

    /// <summary>
    /// Determines whether this activity record represents a viewing activity.
    /// </summary>
    /// <returns>true if this is a viewing activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify viewing activities for
    /// access tracking and analytics purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityUser.IsViewingActivity())
    /// {
    ///     // Handle viewing-specific logic
    ///     accessTracker.LogAccess(activityUser);
    /// }
    /// </code>
    /// </example>
    public bool IsViewingActivity() => MatterActivity?.Activity == "VIEWED";

    /// <summary>
    /// Determines whether this activity occurred recently within the specified timeframe.
    /// </summary>
    /// <param name="withinHours">The number of hours to consider as "recent".</param>
    /// <returns>true if the activity occurred within the specified timeframe; otherwise, false.</returns>
    /// <remarks>
    /// This method is useful for identifying recent matter activities for notifications,
    /// reporting, and real-time monitoring purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check for activities in the last 24 hours
    /// if (activityUser.IsRecent(24))
    /// {
    ///     notificationService.NotifyRecentActivity(activityUser);
    /// }
    /// </code>
    /// </example>
    public bool IsRecent(double withinHours = 24)
    {
        return (DateTime.UtcNow - CreatedAt).TotalHours <= withinHours;
    }

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
    /// bool canApply = activityUser.IsAppropriateForMatterStatus(false, false);  // true for normal matter
    /// bool cannotApply = activityUser.IsAppropriateForMatterStatus(true, false); // false for archived if trying to archive again
    /// </code>
    /// </example>
    public bool IsAppropriateForMatterStatus(bool isArchived, bool isDeleted) =>
        Common.MatterActivityValidationHelper.IsActivityAppropriateForMatterStatus(
            MatterActivity?.Activity, isArchived, isDeleted);

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
    /// var createdActivityId = MatterActivityUser.GetSeededActivityId("CREATED");
    /// // Returns: 20000000-0000-0000-0000-000000000001
    /// </code>
    /// </example>
    public static Guid GetSeededActivityId(string activityName)
    {
        if (string.IsNullOrWhiteSpace(activityName))
            return Guid.Empty;

        return activityName.Trim().ToUpperInvariant() switch
        {
            "ARCHIVED" => Guid.Parse("20000000-0000-0000-0000-000000000002"),
            "CREATED" => Guid.Parse("20000000-0000-0000-0000-000000000001"),
            "DELETED" => Guid.Parse("20000000-0000-0000-0000-000000000003"),
            "RESTORED" => Guid.Parse("20000000-0000-0000-0000-000000000004"),
            "UNARCHIVED" => Guid.Parse("20000000-0000-0000-0000-000000000005"),
            "VIEWED" => Guid.Parse("20000000-0000-0000-0000-000000000006"),
            _ => Guid.Empty
        };
    }

    #endregion Business Logic Methods

    #region Validation Methods

    /// <summary>
    /// Validates the current MatterActivityUser instance against business rules.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive validation using MatterActivityValidationHelper
    /// including foreign key validation, timestamp validation, and activity appropriateness checking.
    /// </remarks>
    /// <example>
    /// <code>
    /// var validationResults = activityUser.Validate();
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

        // Validate foreign key references
        if (MatterId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Matter ID cannot be empty.",
                [nameof(MatterId)]));
        }

        if (MatterActivityId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Matter Activity ID cannot be empty.",
                [nameof(MatterActivityId)]));
        }

        if (UserId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "User ID cannot be empty.",
                [nameof(UserId)]));
        }

        // Validate timestamp using RevisionValidationHelper
        if (!Common.RevisionValidationHelper.IsValidDate(CreatedAt))
        {
            results.Add(new ValidationResult(
                "Created At timestamp is not within valid date range.",
                [nameof(CreatedAt)]));
        }

        // Validate activity appropriateness (general validation)
        if (MatterActivity == null) return results;
        if (!Common.MatterActivityValidationHelper.IsActivityAllowed(MatterActivity.Activity))
        {
            results.Add(new ValidationResult(
                $"Activity '{MatterActivity.Activity}' is not allowed for matter operations.",
                [nameof(MatterActivityId)]));
        }

        return results;
    }

    #endregion Validation Methods
}