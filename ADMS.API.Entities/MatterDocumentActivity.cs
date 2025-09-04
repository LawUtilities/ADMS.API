using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Entities;

/// <summary>
/// Represents an activity performed on a matter document in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// The MatterDocumentActivity entity serves as a lookup table for the standardized activities that can be
/// performed on documents within the context of legal matters. This entity is critical for maintaining
/// comprehensive audit trails and ensuring consistent activity classification across all matter-document
/// operations in the legal document management system.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Static Reference Data:</strong> Contains predefined activity types seeded from database</item>
/// <item><strong>Transfer Operations Focus:</strong> Specializes in document movement operations between matters</item>
/// <item><strong>Audit Trail Foundation:</strong> Central classification system for matter document activities</item>
/// <item><strong>Business Rule Enforcement:</strong> Ensures only valid transfer activities are recorded</item>
/// <item><strong>Legal Compliance:</strong> Supports comprehensive audit requirements</item>
/// <item><strong>Directional Tracking:</strong> Enables bidirectional document transfer audit trails</item>
/// </list>
/// 
/// <para><strong>Seeded Activities:</strong></para>
/// The following transfer activities are seeded in AdmsContext.SeedMatterDocumentActivities:
/// <list type="bullet">
/// <item><strong>COPIED:</strong> Document copied from one matter to another (preserves original)</item>
/// <item><strong>MOVED:</strong> Document moved from one matter to another (transfers ownership)</item>
/// </list>
/// 
/// <para><strong>Database Configuration:</strong></para>
/// <list type="bullet">
/// <item>Primary key: GUID with identity generation</item>
/// <item>Activity constraint: StringLength(50) with required validation</item>
/// <item>Seeded data: Two standard matter document transfer activities</item>
/// <item>Relationships: One-to-many with directional user association entities</item>
/// </list>
/// 
/// <para><strong>Transfer Operation Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Bidirectional Tracking:</strong> From and To user associations for complete audit trails</item>
/// <item><strong>Document Provenance:</strong> Maintains complete custody chains for legal documents</item>
/// <item><strong>Matter Integration:</strong> Links document operations to specific legal cases</item>
/// <item><strong>User Attribution:</strong> Tracks both source and destination users</item>
/// </list>
/// 
/// <para><strong>Legal Compliance Support:</strong></para>
/// <list type="bullet">
/// <item>Standardized activity classification for legal audit requirements</item>
/// <item>Immutable reference data preserving audit trail integrity</item>
/// <item>Complete user attribution through directional associations</item>
/// <item>Support for regulatory reporting and compliance audits</item>
/// </list>
/// 
/// <para><strong>Business Rules:</strong></para>
/// <list type="bullet">
/// <item>Activity names must match MatterDocumentActivityValidationHelper.AllowedActivities</item>
/// <item>Activities are standardized and system-defined (not user-created)</item>
/// <item>Each activity requires both "from" and "to" user associations for complete audit trails</item>
/// <item>Activity names are case-insensitive but stored in uppercase</item>
/// </list>
/// 
/// <para><strong>Entity Framework Integration:</strong></para>
/// The entity is configured in AdmsContext with:
/// <list type="bullet">
/// <item>Seeded data for standard matter document transfer activities</item>
/// <item>Required relationships to directional user association entities</item>
/// <item>NoAction cascade delete to preserve audit trail integrity</item>
/// <item>Performance optimization for frequent lookup operations</item>
/// </list>
/// 
/// <para><strong>Relationship Architecture:</strong></para>
/// <list type="bullet">
/// <item><strong>MatterDocumentActivityUserFrom:</strong> Tracks source users who initiated transfers</item>
/// <item><strong>MatterDocumentActivityUserTo:</strong> Tracks destination users who received transfers</item>
/// <item><strong>MatterDocumentActivityUser:</strong> Central junction for general matter-document activities</item>
/// </list>
/// </remarks>
public class MatterDocumentActivity : IEquatable<MatterDocumentActivity>, IComparable<MatterDocumentActivity>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the matter document activity.
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
    /// <item>Used as foreign key in directional user association entities</item>
    /// <item>Seeded with specific GUIDs for standard transfer activities</item>
    /// </list>
    /// 
    /// <para><strong>Seeded Activity IDs:</strong></para>
    /// <list type="bullet">
    /// <item>COPIED: 40000000-0000-0000-0000-000000000001</item>
    /// <item>MOVED: 40000000-0000-0000-0000-000000000002</item>
    /// </list>
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// The ID remains constant throughout the activity's lifecycle and is used for all
    /// audit trail associations, business logic references, and reporting operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new MatterDocumentActivity 
    /// { 
    ///     Activity = "CUSTOM_TRANSFER"
    /// };
    /// // ID will be automatically generated when saved to database
    /// 
    /// // Accessing seeded activity
    /// var copiedActivityId = Guid.Parse("40000000-0000-0000-0000-000000000001");
    /// </code>
    /// </example>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the description of the matter document activity.
    /// </summary>
    /// <remarks>
    /// The activity name serves as the primary identifier and classifier for matter document operations.
    /// This field must conform to the standardized activity names defined in 
    /// MatterDocumentActivityValidationHelper.AllowedActivities to ensure consistency across the system.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Required field - cannot be null or empty</item>
    /// <item>Maximum length: 50 characters (database constraint)</item>
    /// <item>Minimum length: 2 characters (business rule)</item>
    /// <item>Must be one of the allowed activities from MatterDocumentActivityValidationHelper</item>
    /// <item>Must contain only letters, numbers, and underscores</item>
    /// <item>Must contain at least one letter</item>
    /// <item>Cannot use reserved activity names</item>
    /// </list>
    /// 
    /// <para><strong>Standard Activities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>COPIED:</strong> Document copied between matters (preserves original)</item>
    /// <item><strong>MOVED:</strong> Document moved between matters (transfers ownership)</item>
    /// </list>
    /// 
    /// <para><strong>Business Context:</strong></para>
    /// Activity names are used throughout the system for:
    /// <list type="bullet">
    /// <item>Document transfer classification and reporting</item>
    /// <item>Business rule enforcement and workflow control</item>
    /// <item>User interface display and activity filtering</item>
    /// <item>Legal compliance reporting and analysis</item>
    /// <item>Audit trail generation and document provenance tracking</item>
    /// </list>
    /// 
    /// <para><strong>Validation Integration:</strong></para>
    /// Activity names are validated using MatterDocumentActivityValidationHelper to ensure
    /// they conform to business rules and legal compliance requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard seeded activities
    /// var copiedActivity = new MatterDocumentActivity { Activity = "COPIED" };
    /// var movedActivity = new MatterDocumentActivity { Activity = "MOVED" };
    /// 
    /// // Validation example
    /// bool isValid = Common.MatterDocumentActivityValidationHelper.IsActivityAllowed(copiedActivity.Activity);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Activity description is required and cannot be empty.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Activity description must be between 2 and 50 characters.")]
    public required string Activity { get; set; }

    #endregion Core Properties

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the collection of "from" user associations for this activity.
    /// </summary>
    /// <remarks>
    /// This collection maintains the one-to-many relationship between this matter document activity
    /// and the source-side user associations. Each association represents a specific instance of
    /// this activity being initiated by a user from a source matter to transfer a document.
    /// 
    /// <para><strong>Source-Side Tracking:</strong></para>
    /// Each association in this collection provides:
    /// <list type="bullet">
    /// <item>Source matter identification - where the document originated</item>
    /// <item>User attribution - who initiated the document transfer</item>
    /// <item>Document context - which specific document was transferred</item>
    /// <item>Temporal tracking - when the transfer was initiated</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured in AdmsContext.ConfigureMatterDocumentActivityUserFrom</item>
    /// <item>One-to-many relationship from activity to user associations</item>
    /// <item>Composite primary key includes ActivityId in the From entity</item>
    /// <item>NoAction cascade behavior preserves audit trail integrity</item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Activity-based source tracking for audit trails</item>
    /// <item>User initiation reporting and monitoring</item>
    /// <item>Document provenance analysis</item>
    /// <item>Transfer pattern analysis and optimization</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// The virtual modifier enables lazy loading, but consider explicit loading or
    /// projections when working with multiple activities to avoid N+1 query issues.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing users who initiated this type of transfer
    /// foreach (var fromUser in matterDocumentActivity.MatterDocumentActivityUsersFrom)
    /// {
    ///     Console.WriteLine($"User {fromUser.User?.Name} initiated {matterDocumentActivity.Activity} " +
    ///                      $"from matter {fromUser.Matter?.Description} at {fromUser.CreatedAt}");
    /// }
    /// 
    /// // Finding source patterns for MOVED activities
    /// var movedActivity = activities.FirstOrDefault(a => a.Activity == "MOVED");
    /// var sourceMatters = movedActivity?.MatterDocumentActivityUsersFrom
    ///     .Select(f => f.Matter)
    ///     .Distinct();
    /// </code>
    /// </example>
    public virtual ICollection<MatterDocumentActivityUserFrom> MatterDocumentActivityUsersFrom { get; set; }
        = new HashSet<MatterDocumentActivityUserFrom>();

    /// <summary>
    /// Gets or sets the collection of "to" user associations for this activity.
    /// </summary>
    /// <remarks>
    /// This collection maintains the one-to-many relationship between this matter document activity
    /// and the destination-side user associations. Each association represents a specific instance of
    /// this activity being received by a user at a destination matter for document transfers.
    /// 
    /// <para><strong>Destination-Side Tracking:</strong></para>
    /// Each association in this collection provides:
    /// <list type="bullet">
    /// <item>Destination matter identification - where the document was transferred to</item>
    /// <item>User attribution - who received the document transfer</item>
    /// <item>Document context - which specific document was received</item>
    /// <item>Temporal tracking - when the transfer was completed</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured in AdmsContext.ConfigureMatterDocumentActivityUserTo</item>
    /// <item>One-to-many relationship from activity to user associations</item>
    /// <item>Composite primary key includes ActivityId in the To entity</item>
    /// <item>NoAction cascade behavior preserves audit trail integrity</item>
    /// </list>
    /// 
    /// <para><strong>Bidirectional Tracking:</strong></para>
    /// <list type="bullet">
    /// <item>Complements MatterDocumentActivityUsersFrom for complete audit trails</item>
    /// <item>Enables comprehensive document custody chain analysis</item>
    /// <item>Supports legal discovery and compliance requirements</item>
    /// <item>Facilitates transfer completion verification</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance:</strong></para>
    /// The bidirectional tracking ensures complete accountability for document transfers,
    /// supporting legal practice requirements for maintaining accurate records of document
    /// custody and access chains between matters.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing users who received this type of transfer
    /// foreach (var toUser in matterDocumentActivity.MatterDocumentActivityUsersTo)
    /// {
    ///     Console.WriteLine($"User {toUser.User?.Name} received {matterDocumentActivity.Activity} " +
    ///                      $"to matter {toUser.Matter?.Description} at {toUser.CreatedAt}");
    /// }
    /// 
    /// // Analyzing destination patterns for COPIED activities
    /// var copiedActivity = activities.FirstOrDefault(a => a.Activity == "COPIED");
    /// var destinationMatters = copiedActivity?.MatterDocumentActivityUsersTo
    ///     .GroupBy(t => t.Matter)
    ///     .Select(g => new { Matter = g.Key, Count = g.Count() })
    ///     .OrderByDescending(x => x.Count);
    /// </code>
    /// </example>
    public virtual ICollection<MatterDocumentActivityUserTo> MatterDocumentActivityUsersTo { get; set; }
        = new HashSet<MatterDocumentActivityUserTo>();

    /// <summary>
    /// Gets or sets the collection of general user associations for this activity.
    /// </summary>
    /// <remarks>
    /// This collection maintains the relationship to the central junction entity that tracks
    /// matter-document activities in a unified manner, complementing the directional From/To
    /// tracking with general activity associations.
    /// 
    /// <para><strong>Central Activity Tracking:</strong></para>
    /// This collection provides:
    /// <list type="bullet">
    /// <item>Unified view of all matter-document activities</item>
    /// <item>General activity tracking beyond directional transfers</item>
    /// <item>Simplified queries for activity-based reporting</item>
    /// <item>Integration with broader audit trail systems</item>
    /// </list>
    /// 
    /// <para><strong>Relationship Context:</strong></para>
    /// While the From/To collections track directional document transfers, this collection
    /// provides a central point for all matter-document activity tracking, supporting both
    /// transfer operations and other potential future activity types.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing all general activity associations
    /// var totalActivityCount = matterDocumentActivity.MatterDocumentActivityUsers.Count;
    /// var uniqueUsers = matterDocumentActivity.MatterDocumentActivityUsers
    ///     .Select(a => a.User)
    ///     .Distinct();
    /// </code>
    /// </example>
    public virtual ICollection<MatterDocumentActivityUser> MatterDocumentActivityUsers { get; set; }
        = new HashSet<MatterDocumentActivityUser>();

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
    /// if (!matterDocumentActivity.HasUserAssociations)
    /// {
    ///     logger.LogInformation($"Activity {matterDocumentActivity.Activity} has no user associations");
    /// }
    /// 
    /// // Finding unused activities
    /// var unusedActivities = activities.Where(a => !a.HasUserAssociations).ToList();
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasUserAssociations =>
        MatterDocumentActivityUsersFrom.Count > 0 ||
        MatterDocumentActivityUsersTo.Count > 0 ||
        MatterDocumentActivityUsers.Count > 0;

    /// <summary>
    /// Gets the total count of user associations across all relationship types.
    /// </summary>
    /// <remarks>
    /// This computed property provides insight into the frequency of use for this activity
    /// type, including both directional (From/To) and general associations.
    /// 
    /// <para><strong>Performance Note:</strong></para>
    /// This property triggers database queries to count related entities. Consider using
    /// explicit loading or projections when working with multiple activities to avoid N+1 queries.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Activity {matterDocumentActivity.Activity} has been used {matterDocumentActivity.TotalUsageCount} times");
    /// 
    /// // Finding most used activities
    /// var mostUsedActivities = activities
    ///     .OrderByDescending(a => a.TotalUsageCount)
    ///     .Take(10);
    /// </code>
    /// </example>
    [NotMapped]
    public int TotalUsageCount =>
        MatterDocumentActivityUsersFrom.Count +
        MatterDocumentActivityUsersTo.Count +
        MatterDocumentActivityUsers.Count;

    /// <summary>
    /// Gets the count of unique users who have performed this activity.
    /// </summary>
    /// <remarks>
    /// This computed property provides insight into the breadth of user engagement
    /// with this activity type across all association types.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Activity {matterDocumentActivity.Activity} has been performed by {matterDocumentActivity.UniqueUserCount} different users");
    /// </code>
    /// </example>
    [NotMapped]
    public int UniqueUserCount
    {
        get
        {
            var fromUsers = MatterDocumentActivityUsersFrom.Select(f => f.UserId);
            var toUsers = MatterDocumentActivityUsersTo.Select(t => t.UserId);
            var generalUsers = MatterDocumentActivityUsers.Select(g => g.UserId);

            return fromUsers.Union(toUsers).Union(generalUsers).Distinct().Count();
        }
    }

    /// <summary>
    /// Gets a value indicating whether this activity is one of the standard seeded activities.
    /// </summary>
    /// <remarks>
    /// This property identifies whether the activity is one of the standard system-defined
    /// transfer activities or a custom activity. Standard activities are those seeded in the
    /// database and validated by MatterDocumentActivityValidationHelper.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matterDocumentActivity.IsStandardActivity)
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
        Common.MatterDocumentActivityValidationHelper.IsActivityAllowed(Activity);

    /// <summary>
    /// Gets the normalized version of the activity name.
    /// </summary>
    /// <remarks>
    /// This property provides a normalized version of the activity name following
    /// the normalization rules from MatterDocumentActivityValidationHelper for consistent
    /// comparison and storage operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var normalized = matterDocumentActivity.NormalizedActivity; // Always uppercase, trimmed
    /// bool areEqual = matterDocumentActivity.NormalizedActivity == other.NormalizedActivity;
    /// </code>
    /// </example>
    [NotMapped]
    public string NormalizedActivity =>
        Common.MatterDocumentActivityValidationHelper.NormalizeActivity(Activity) ?? Activity.ToUpperInvariant();

    /// <summary>
    /// Gets a value indicating whether this activity has balanced directional associations.
    /// </summary>
    /// <remarks>
    /// This property checks if the activity has balanced From and To associations, which is
    /// important for transfer activities where each transfer should have both source and
    /// destination tracking for complete audit trails.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!matterDocumentActivity.HasBalancedDirectionalTracking)
    /// {
    ///     logger.LogWarning($"Activity {matterDocumentActivity.Activity} has unbalanced directional tracking");
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasBalancedDirectionalTracking =>
        MatterDocumentActivityUsersFrom.Count == MatterDocumentActivityUsersTo.Count;

    #endregion Computed Properties

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified MatterDocumentActivity is equal to the current MatterDocumentActivity.
    /// </summary>
    /// <param name="other">The MatterDocumentActivity to compare with the current MatterDocumentActivity.</param>
    /// <returns>true if the specified MatterDocumentActivity is equal to the current MatterDocumentActivity; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each activity has a unique identifier.
    /// This follows Entity Framework best practices for entity equality comparison.
    /// </remarks>
    public bool Equals(MatterDocumentActivity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current MatterDocumentActivity.
    /// </summary>
    /// <param name="obj">The object to compare with the current MatterDocumentActivity.</param>
    /// <returns>true if the specified object is equal to the current MatterDocumentActivity; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as MatterDocumentActivity);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterDocumentActivity.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Compares the current MatterDocumentActivity with another MatterDocumentActivity for ordering purposes.
    /// </summary>
    /// <param name="other">The MatterDocumentActivity to compare with the current MatterDocumentActivity.</param>
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
    public int CompareTo(MatterDocumentActivity? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by activity name for alphabetical ordering
        var activityComparison = string.Compare(Activity, other.Activity, StringComparison.OrdinalIgnoreCase);
        return activityComparison != 0 ? activityComparison :
            // Secondary sort by ID for consistency when activities have same name
            Id.CompareTo(other.Id);
    }

    /// <summary>
    /// Determines whether two MatterDocumentActivity instances are equal.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivity to compare.</param>
    /// <param name="right">The second MatterDocumentActivity to compare.</param>
    /// <returns>true if the MatterDocumentActivities are equal; otherwise, false.</returns>
    public static bool operator ==(MatterDocumentActivity? left, MatterDocumentActivity? right) =>
        EqualityComparer<MatterDocumentActivity>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two MatterDocumentActivity instances are not equal.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivity to compare.</param>
    /// <param name="right">The second MatterDocumentActivity to compare.</param>
    /// <returns>true if the MatterDocumentActivities are not equal; otherwise, false.</returns>
    public static bool operator !=(MatterDocumentActivity? left, MatterDocumentActivity? right) => !(left == right);

    /// <summary>
    /// Determines whether one MatterDocumentActivity precedes another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivity to compare.</param>
    /// <param name="right">The second MatterDocumentActivity to compare.</param>
    /// <returns>true if the left MatterDocumentActivity precedes the right MatterDocumentActivity; otherwise, false.</returns>
    public static bool operator <(MatterDocumentActivity? left, MatterDocumentActivity? right) =>
        left is not null && (right is null || left.CompareTo(right) < 0);

    /// <summary>
    /// Determines whether one MatterDocumentActivity precedes or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivity to compare.</param>
    /// <param name="right">The second MatterDocumentActivity to compare.</param>
    /// <returns>true if the left MatterDocumentActivity precedes or equals the right MatterDocumentActivity; otherwise, false.</returns>
    public static bool operator <=(MatterDocumentActivity? left, MatterDocumentActivity? right) =>
        left is null || (right is not null && left.CompareTo(right) <= 0);

    /// <summary>
    /// Determines whether one MatterDocumentActivity follows another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivity to compare.</param>
    /// <param name="right">The second MatterDocumentActivity to compare.</param>
    /// <returns>true if the left MatterDocumentActivity follows the right MatterDocumentActivity; otherwise, false.</returns>
    public static bool operator >(MatterDocumentActivity? left, MatterDocumentActivity? right) =>
        left is not null && (right is null || left.CompareTo(right) > 0);

    /// <summary>
    /// Determines whether one MatterDocumentActivity follows or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivity to compare.</param>
    /// <param name="right">The second MatterDocumentActivity to compare.</param>
    /// <returns>true if the left MatterDocumentActivity follows or equals the right MatterDocumentActivity; otherwise, false.</returns>
    public static bool operator >=(MatterDocumentActivity? left, MatterDocumentActivity? right) =>
        left is null || (right is not null && left.CompareTo(right) >= 0);

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterDocumentActivity.
    /// </summary>
    /// <returns>A string that represents the current MatterDocumentActivity.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the activity,
    /// which is useful for debugging, logging, and display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new MatterDocumentActivity 
    /// { 
    ///     Id = Guid.Parse("40000000-0000-0000-0000-000000000001"), 
    ///     Activity = "COPIED"
    /// };
    /// 
    /// Console.WriteLine(activity);
    /// // Output: "MatterDocumentActivity: COPIED (40000000-0000-0000-0000-000000000001)"
    /// </code>
    /// </example>
    public override string ToString() => $"MatterDocumentActivity: {Activity} ({Id})";

    #endregion String Representation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this activity represents a copy operation.
    /// </summary>
    /// <returns>true if this is a copy activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify copy activities for
    /// business rule enforcement and audit trail analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matterDocumentActivity.IsCopyActivity())
    /// {
    ///     // Apply copy-specific business rules
    ///     Console.WriteLine("This is a document copy activity");
    /// }
    /// </code>
    /// </example>
    public bool IsCopyActivity() =>
        string.Equals(Activity, "COPIED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a move operation.
    /// </summary>
    /// <returns>true if this is a move activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify move activities for
    /// audit trail analysis and business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matterDocumentActivity.IsMoveActivity())
    /// {
    ///     // Apply move-specific business rules
    ///     auditLogger.LogMove(matterDocumentActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsMoveActivity() =>
        string.Equals(Activity, "MOVED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a transfer operation (either move or copy).
    /// </summary>
    /// <returns>true if this is a transfer activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify any type of document transfer
    /// activity for business rule enforcement and workflow analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matterDocumentActivity.IsTransferActivity())
    /// {
    ///     // Apply transfer-specific business rules
    ///     transferService.ProcessTransfer(matterDocumentActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsTransferActivity() => IsCopyActivity() || IsMoveActivity();

    /// <summary>
    /// Determines whether this activity requires bidirectional user associations.
    /// </summary>
    /// <returns>true if the activity requires both from and to user associations; otherwise, false.</returns>
    /// <remarks>
    /// This method uses the MatterDocumentActivityValidationHelper to validate business rules
    /// for activity context requirements, specifically whether the activity needs both
    /// source and destination user attribution.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool requiresBidirectional = activity.RequiresBidirectionalAssociations(); // true for MOVED/COPIED
    /// </code>
    /// </example>
    public bool RequiresBidirectionalAssociations() =>
        Common.MatterDocumentActivityValidationHelper.IsActivityAppropriateForContext(Activity, true, true);

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
    /// var copiedActivityId = MatterDocumentActivity.GetSeededActivityId("COPIED");
    /// // Returns: 40000000-0000-0000-0000-000000000001
    /// 
    /// var movedActivityId = MatterDocumentActivity.GetSeededActivityId("MOVED");
    /// // Returns: 40000000-0000-0000-0000-000000000002
    /// </code>
    /// </example>
    public static Guid GetSeededActivityId(string activityName)
    {
        if (string.IsNullOrWhiteSpace(activityName))
            return Guid.Empty;

        return activityName.Trim().ToUpperInvariant() switch
        {
            "COPIED" => Guid.Parse("40000000-0000-0000-0000-000000000001"),
            "MOVED" => Guid.Parse("40000000-0000-0000-0000-000000000002"),
            _ => Guid.Empty
        };
    }

    /// <summary>
    /// Gets transfer statistics for this activity.
    /// </summary>
    /// <returns>A dictionary containing transfer statistics.</returns>
    /// <remarks>
    /// This method provides insights into the transfer patterns and usage of this activity
    /// for reporting and analysis purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = matterDocumentActivity.GetTransferStatistics();
    /// Console.WriteLine($"Total transfers: {stats["TotalTransfers"]}");
    /// Console.WriteLine($"Unique users: {stats["UniqueUsers"]}");
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetTransferStatistics()
    {
        return new Dictionary<string, object>
        {
            ["ActivityType"] = Activity,
            ["TotalFromAssociations"] = MatterDocumentActivityUsersFrom.Count,
            ["TotalToAssociations"] = MatterDocumentActivityUsersTo.Count,
            ["TotalGeneralAssociations"] = MatterDocumentActivityUsers.Count,
            ["TotalTransfers"] = TotalUsageCount,
            ["UniqueUsers"] = UniqueUserCount,
            ["IsBalanced"] = HasBalancedDirectionalTracking,
            ["IsStandardActivity"] = IsStandardActivity
        };
    }

    #endregion Business Logic Methods

    #region Validation Methods

    /// <summary>
    /// Validates the current MatterDocumentActivity instance against business rules.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive validation using MatterDocumentActivityValidationHelper
    /// including activity name validation, length constraints, and business rule compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// var validationResults = matterDocumentActivity.Validate();
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

        // Validate using MatterDocumentActivityValidationHelper
        results.AddRange(Common.MatterDocumentActivityValidationHelper.ValidateActivity(Activity, nameof(Activity)));

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