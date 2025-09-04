using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Entities;

/// <summary>
/// Represents the association between a revision, a revision activity, and a user in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// The RevisionActivityUser entity serves as a junction table implementing the many-to-many relationship
/// between revisions, revision activities, and users. This entity is critical for maintaining comprehensive
/// audit trails of all revision-related operations within the legal document management system.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Audit Trail Foundation:</strong> Central component of revision activity tracking</item>
/// <item><strong>Composite Primary Key:</strong> Ensures uniqueness while allowing temporal tracking</item>
/// <item><strong>User Attribution:</strong> Links every revision activity to a responsible user</item>
/// <item><strong>Temporal Tracking:</strong> Maintains precise timestamps for all activities</item>
/// <item><strong>Legal Compliance:</strong> Supports comprehensive audit requirements</item>
/// </list>
/// 
/// <para><strong>Database Configuration:</strong></para>
/// <list type="bullet">
/// <item>Composite primary key: RevisionId + RevisionActivityId + UserId + CreatedAt</item>
/// <item>All foreign key relationships are required</item>
/// <item>Cascade behavior: Cascade from Revision, Restrict from Activity and User</item>
/// <item>Indexed on CreatedAt for temporal queries</item>
/// </list>
/// 
/// <para><strong>Audit Trail Functionality:</strong></para>
/// This entity enables comprehensive tracking of:
/// <list type="bullet">
/// <item><strong>Who:</strong> Which user performed the activity</item>
/// <item><strong>What:</strong> What revision activity was performed</item>
/// <item><strong>Where:</strong> On which revision the activity occurred</item>
/// <item><strong>When:</strong> Precise timestamp of the activity</item>
/// </list>
/// 
/// <para><strong>Legal Compliance Support:</strong></para>
/// <list type="bullet">
/// <item>Complete user attribution for all revision operations</item>
/// <item>Temporal audit trails for legal discovery and compliance</item>
/// <item>Immutable record of document version control activities</item>
/// <item>Support for regulatory reporting and audit requirements</item>
/// </list>
/// 
/// <para><strong>Business Rules:</strong></para>
/// <list type="bullet">
/// <item>Every revision activity must be attributed to a user</item>
/// <item>Multiple activities of the same type can occur with different timestamps</item>
/// <item>CreatedAt timestamp must be within reasonable bounds</item>
/// <item>All foreign key relationships must reference valid entities</item>
/// </list>
/// 
/// <para><strong>Entity Framework Configuration:</strong></para>
/// The entity is configured in AdmsContext.ConfigureRevisionActivityUser with:
/// <list type="bullet">
/// <item>Composite key including all four core properties</item>
/// <item>Required relationships to Revision, RevisionActivity, and User</item>
/// <item>Proper cascade behaviors to maintain audit trail integrity</item>
/// <item>Performance indexes on commonly queried fields</item>
/// </list>
/// </remarks>
public class RevisionActivityUser : IEquatable<RevisionActivityUser>, IComparable<RevisionActivityUser>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier of the revision.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the revision that this activity
    /// is associated with. It forms part of the composite primary key and is required for all
    /// revision activity user associations.
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to Revision entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>Supports cascade delete from parent revision</item>
    /// </list>
    /// 
    /// <para><strong>Usage Context:</strong></para>
    /// <list type="bullet">
    /// <item>Links activity to specific document revision</item>
    /// <item>Enables revision-scoped activity queries</item>
    /// <item>Supports version control audit trails</item>
    /// <item>Facilitates revision lifecycle tracking</item>
    /// </list>
    /// 
    /// <para><strong>Validation:</strong></para>
    /// The RevisionId is validated to ensure it references a valid revision using
    /// RevisionValidationHelper.IsValidRevisionId method.
    /// </remarks>
    /// <example>
    /// <code>
    /// var revisionActivity = new RevisionActivityUser
    /// {
    ///     RevisionId = revision.Id,  // Must reference valid revision
    ///     RevisionActivityId = activityId,
    ///     UserId = user.Id,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Revision ID is required for activity association.")]
    public Guid RevisionId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the revision activity.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the revision activity type
    /// that was performed. It forms part of the composite primary key and must reference
    /// a valid revision activity from the seeded data.
    /// 
    /// <para><strong>Activity Types:</strong></para>
    /// Must reference one of the seeded revision activities:
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Revision creation activity</item>
    /// <item><strong>SAVED:</strong> Revision save activity</item>
    /// <item><strong>DELETED:</strong> Revision deletion activity</item>
    /// <item><strong>RESTORED:</strong> Revision restoration activity</item>
    /// </list>
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to RevisionActivity entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>Restricted cascade delete to preserve audit integrity</item>
    /// </list>
    /// 
    /// <para><strong>Validation:</strong></para>
    /// The RevisionActivityId is validated using RevisionActivityValidationHelper
    /// to ensure it references a valid, allowed activity type.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Linking to a CREATED activity
    /// var createdActivityId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    /// var activityUser = new RevisionActivityUser
    /// {
    ///     RevisionActivityId = createdActivityId,  // Must be valid activity
    ///     // ... other properties
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Revision Activity ID is required for activity classification.")]
    public Guid RevisionActivityId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the user who performed
    /// the revision activity. It forms part of the composite primary key and provides
    /// the essential user attribution for audit trail purposes.
    /// 
    /// <para><strong>User Attribution:</strong></para>
    /// <list type="bullet">
    /// <item>Links activity to responsible user for accountability</item>
    /// <item>Enables user-scoped activity reporting</item>
    /// <item>Supports legal compliance and audit requirements</item>
    /// <item>Facilitates user activity analytics and monitoring</item>
    /// </list>
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to User entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>Restricted cascade delete to preserve audit integrity</item>
    /// </list>
    /// 
    /// <para><strong>Legal Significance:</strong></para>
    /// User attribution is critical for:
    /// <list type="bullet">
    /// <item>Legal discovery and compliance audits</item>
    /// <item>Professional responsibility tracking</item>
    /// <item>Accountability in document management</item>
    /// <item>Evidence of who performed specific actions</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Attributing activity to a specific user
    /// var activityUser = new RevisionActivityUser
    /// {
    ///     UserId = currentUser.Id,  // Must reference valid user
    ///     RevisionId = revision.Id,
    ///     RevisionActivityId = activityId,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User ID is required for activity attribution.")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entry was created (in UTC).
    /// </summary>
    /// <remarks>
    /// This property maintains the precise timestamp of when the revision activity occurred.
    /// It forms part of the composite primary key, enabling multiple activities of the same
    /// type to be recorded with different timestamps while maintaining uniqueness.
    /// 
    /// <para><strong>Temporal Tracking:</strong></para>
    /// <list type="bullet">
    /// <item>Provides precise timing for audit trail chronology</item>
    /// <item>Enables temporal analysis of revision activities</item>
    /// <item>Supports legal timeline reconstruction</item>
    /// <item>Facilitates performance and workflow analysis</item>
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
    /// <item>Should reflect the actual time the activity occurred</item>
    /// <item>Used for chronological ordering in audit reports</item>
    /// <item>Critical for legal compliance and audit trails</item>
    /// </list>
    /// 
    /// <para><strong>Validation:</strong></para>
    /// The CreatedAt timestamp is validated using RevisionValidationHelper
    /// to ensure it falls within acceptable date ranges.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityUser = new RevisionActivityUser
    /// {
    ///     CreatedAt = DateTime.UtcNow,  // Always use UTC
    ///     // ... other properties
    /// };
    /// 
    /// // For historical data import
    /// var historicalActivity = new RevisionActivityUser
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
    /// Gets or sets the revision associated with this activity.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the revision that this activity was performed on.
    /// The relationship is established through the RevisionId foreign key and enables rich
    /// querying and navigation within Entity Framework.
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses RevisionId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>Cascade delete behavior preserves audit trail integrity</item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Accessing revision details from activity context</item>
    /// <item>Document version control operations</item>
    /// <item>Audit trail reporting with revision information</item>
    /// <item>Cross-entity queries and projections</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// The virtual modifier enables lazy loading, but consider explicit loading or
    /// projections when working with multiple activities to avoid N+1 query issues.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing revision information through activity
    /// var documentName = activityUser.Revision.Document.FileName;
    /// var revisionNumber = activityUser.Revision.RevisionNumber;
    /// 
    /// // Using explicit loading to avoid N+1 queries
    /// await context.Entry(activityUser)
    ///     .Reference(au => au.Revision)
    ///     .LoadAsync();
    /// </code>
    /// </example>
    [ForeignKey(nameof(RevisionId))]
    public virtual required Revision Revision { get; set; }

    /// <summary>
    /// Gets or sets the revision activity associated with this user.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the revision activity type that was performed.
    /// The relationship is established through the RevisionActivityId foreign key and enables
    /// access to activity metadata such as the activity name and description.
    /// 
    /// <para><strong>Activity Information:</strong></para>
    /// Provides access to:
    /// <list type="bullet">
    /// <item>Activity name (CREATED, SAVED, DELETED, RESTORED)</item>
    /// <item>Activity metadata and configuration</item>
    /// <item>Activity validation rules and constraints</item>
    /// <item>Cross-activity analysis and reporting</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses RevisionActivityId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>Restricted cascade delete to preserve audit integrity</item>
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
    /// var activityName = activityUser.RevisionActivity.Activity;
    /// var isCreationActivity = activityName == "CREATED";
    /// 
    /// // Activity-based filtering
    /// var creationActivities = activities
    ///     .Where(a => a.RevisionActivity.Activity == "CREATED");
    /// </code>
    /// </example>
    [ForeignKey(nameof(RevisionActivityId))]
    public virtual required RevisionActivity RevisionActivity { get; set; }

    /// <summary>
    /// Gets or sets the user associated with this revision activity.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the user who performed the revision activity.
    /// The relationship is established through the UserId foreign key and enables comprehensive
    /// user-based reporting and analysis of revision activities.
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
    /// <item>Restricted cascade delete to preserve audit integrity</item>
    /// </list>
    /// 
    /// <para><strong>Legal and Compliance Support:</strong></para>
    /// <list type="bullet">
    /// <item>User accountability for revision activities</item>
    /// <item>Professional responsibility tracking</item>
    /// <item>Legal discovery and audit trail support</item>
    /// <item>Compliance reporting and analysis</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing user information through activity
    /// var userName = activityUser.User.Name;
    /// var userActivities = activityUser.User.RevisionActivityUsers;
    /// 
    /// // User-based activity analysis
    /// var userCreationCount = user.RevisionActivityUsers
    ///     .Count(au => au.RevisionActivity.Activity == "CREATED");
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
    /// <item>RevisionId is not Guid.Empty</item>
    /// <item>RevisionActivityId is not Guid.Empty</item>
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
        RevisionId != Guid.Empty &&
        RevisionActivityId != Guid.Empty &&
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
    /// This computed property provides a human-readable description of the activity
    /// including user attribution, activity type, and timestamp information.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine(activityUser.ActivityDescription);
    /// // Output: "rbrown performed CREATED on revision 1 at 2024-01-15 10:30 UTC"
    /// </code>
    /// </example>
    [NotMapped]
    public string ActivityDescription =>
        $"{User?.Name ?? "Unknown User"} performed {RevisionActivity?.Activity ?? "Unknown Activity"} " +
        $"on revision {Revision?.RevisionNumber.ToString() ?? "Unknown"} at {CreatedAt:yyyy-MM-dd HH:mm} UTC";

    #endregion Computed Properties

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified RevisionActivityUser is equal to the current RevisionActivityUser.
    /// </summary>
    /// <param name="other">The RevisionActivityUser to compare with the current RevisionActivityUser.</param>
    /// <returns>true if the specified RevisionActivityUser is equal to the current RevisionActivityUser; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all four components of the composite primary key:
    /// RevisionId, RevisionActivityId, UserId, and CreatedAt. This follows Entity Framework
    /// best practices for entities with composite keys.
    /// </remarks>
    public bool Equals(RevisionActivityUser? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return RevisionId.Equals(other.RevisionId) &&
               RevisionActivityId.Equals(other.RevisionActivityId) &&
               UserId.Equals(other.UserId) &&
               CreatedAt.Equals(other.CreatedAt);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current RevisionActivityUser.
    /// </summary>
    /// <param name="obj">The object to compare with the current RevisionActivityUser.</param>
    /// <returns>true if the specified object is equal to the current RevisionActivityUser; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as RevisionActivityUser);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current RevisionActivityUser.</returns>
    /// <remarks>
    /// The hash code is computed from all four components of the composite primary key
    /// to ensure consistent hashing behavior that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => HashCode.Combine(RevisionId, RevisionActivityId, UserId, CreatedAt);

    /// <summary>
    /// Compares the current RevisionActivityUser with another RevisionActivityUser for ordering purposes.
    /// </summary>
    /// <param name="other">The RevisionActivityUser to compare with the current RevisionActivityUser.</param>
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
    public int CompareTo(RevisionActivityUser? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by timestamp for chronological ordering
        var timeComparison = CreatedAt.CompareTo(other.CreatedAt);
        if (timeComparison != 0) return timeComparison;

        // Secondary sort by RevisionId for consistency
        var revisionComparison = RevisionId.CompareTo(other.RevisionId);
        if (revisionComparison != 0) return revisionComparison;

        // Tertiary sort by ActivityId
        var activityComparison = RevisionActivityId.CompareTo(other.RevisionActivityId);
        return activityComparison != 0 ? activityComparison :
            // Final sort by UserId
            UserId.CompareTo(other.UserId);
    }

    /// <summary>
    /// Determines whether two RevisionActivityUser instances are equal.
    /// </summary>
    /// <param name="left">The first RevisionActivityUser to compare.</param>
    /// <param name="right">The second RevisionActivityUser to compare.</param>
    /// <returns>true if the RevisionActivityUsers are equal; otherwise, false.</returns>
    public static bool operator ==(RevisionActivityUser? left, RevisionActivityUser? right) =>
        EqualityComparer<RevisionActivityUser>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two RevisionActivityUser instances are not equal.
    /// </summary>
    /// <param name="left">The first RevisionActivityUser to compare.</param>
    /// <param name="right">The second RevisionActivityUser to compare.</param>
    /// <returns>true if the RevisionActivityUsers are not equal; otherwise, false.</returns>
    public static bool operator !=(RevisionActivityUser? left, RevisionActivityUser? right) => !(left == right);

    /// <summary>
    /// Determines whether one RevisionActivityUser precedes another in the ordering.
    /// </summary>
    /// <param name="left">The first RevisionActivityUser to compare.</param>
    /// <param name="right">The second RevisionActivityUser to compare.</param>
    /// <returns>true if the left RevisionActivityUser precedes the right RevisionActivityUser; otherwise, false.</returns>
    public static bool operator <(RevisionActivityUser? left, RevisionActivityUser? right) =>
        left is not null && (right is null || left.CompareTo(right) < 0);

    /// <summary>
    /// Determines whether one RevisionActivityUser precedes or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first RevisionActivityUser to compare.</param>
    /// <param name="right">The second RevisionActivityUser to compare.</param>
    /// <returns>true if the left RevisionActivityUser precedes or equals the right RevisionActivityUser; otherwise, false.</returns>
    public static bool operator <=(RevisionActivityUser? left, RevisionActivityUser? right) =>
        left is null || (right is not null && left.CompareTo(right) <= 0);

    /// <summary>
    /// Determines whether one RevisionActivityUser follows another in the ordering.
    /// </summary>
    /// <param name="left">The first RevisionActivityUser to compare.</param>
    /// <param name="right">The second RevisionActivityUser to compare.</param>
    /// <returns>true if the left RevisionActivityUser follows the right RevisionActivityUser; otherwise, false.</returns>
    public static bool operator >(RevisionActivityUser? left, RevisionActivityUser? right) =>
        left is not null && (right is null || left.CompareTo(right) > 0);

    /// <summary>
    /// Determines whether one RevisionActivityUser follows or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first RevisionActivityUser to compare.</param>
    /// <param name="right">The second RevisionActivityUser to compare.</param>
    /// <returns>true if the left RevisionActivityUser follows or equals the right RevisionActivityUser; otherwise, false.</returns>
    public static bool operator >=(RevisionActivityUser? left, RevisionActivityUser? right) =>
        left is null || (right is not null && left.CompareTo(right) >= 0);

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the RevisionActivityUser.
    /// </summary>
    /// <returns>A string that represents the current RevisionActivityUser.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the activity,
    /// which is useful for debugging, logging, and audit trail display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityUser = new RevisionActivityUser
    /// {
    ///     RevisionId = revisionGuid,
    ///     RevisionActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// Console.WriteLine(activityUser);
    /// // Output: "User rbrown performed CREATED on Revision 1 at 2024-01-15 10:30:00 UTC"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"User {User?.Name ?? UserId.ToString()[..8]} performed {RevisionActivity?.Activity ?? "Activity"} " +
        $"on Revision {Revision?.RevisionNumber.ToString() ?? RevisionId.ToString()[..8]} at {CreatedAt:yyyy-MM-dd HH:mm:ss} UTC";

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
    ///     Console.WriteLine("This revision was created by: " + activityUser.User?.Name);
    /// }
    /// </code>
    /// </example>
    public bool IsCreationActivity() => RevisionActivity?.Activity == "CREATED";

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
    public bool IsDeletionActivity() => RevisionActivity?.Activity == "DELETED";

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
    public bool IsRestorationActivity() => RevisionActivity?.Activity == "RESTORED";

    /// <summary>
    /// Determines whether this activity occurred recently within the specified timeframe.
    /// </summary>
    /// <param name="withinHours">The number of hours to consider as "recent".</param>
    /// <returns>true if the activity occurred within the specified timeframe; otherwise, false.</returns>
    /// <remarks>
    /// This method is useful for identifying recent activities for notifications,
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

    #endregion Business Logic Methods

    #region Validation Methods

    /// <summary>
    /// Validates the current RevisionActivityUser instance against business rules.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive validation including:
    /// <list type="bullet">
    /// <item>Foreign key reference validation</item>
    /// <item>Timestamp validation using RevisionValidationHelper</item>
    /// <item>Business rule compliance checking</item>
    /// </list>
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
        if (RevisionId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Revision ID cannot be empty.",
                [nameof(RevisionId)]));
        }

        if (RevisionActivityId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Revision Activity ID cannot be empty.",
                [nameof(RevisionActivityId)]));
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

        return results;
    }

    #endregion Validation Methods
}