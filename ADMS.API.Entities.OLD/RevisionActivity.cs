using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.API.Entities;

/// <summary>
/// Represents a revision activity, which describes an action or event related to a document revision in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// The RevisionActivity entity serves as a lookup table for the standardized activities that can be performed
/// on document revisions within the legal document management system. This entity is critical for maintaining
/// comprehensive audit trails and ensuring consistent activity classification across all revision operations.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Static Reference Data:</strong> Contains predefined activity types seeded from database</item>
/// <item><strong>Audit Trail Foundation:</strong> Central classification system for revision activities</item>
/// <item><strong>Business Rule Enforcement:</strong> Ensures only valid activities are recorded</item>
/// <item><strong>Legal Compliance:</strong> Supports comprehensive audit requirements</item>
/// <item><strong>Immutable Activities:</strong> Activity types are system-defined and rarely change</item>
/// </list>
/// 
/// <para><strong>Seeded Activities:</strong></para>
/// The following activities are seeded in AdmsContext.SeedRevisionActivities:
/// <list type="bullet">
/// <item><strong>CREATED:</strong> Initial creation of a document revision</item>
/// <item><strong>SAVED:</strong> Saving changes to an existing revision</item>
/// <item><strong>DELETED:</strong> Soft deletion of a revision (can be restored)</item>
/// <item><strong>RESTORED:</strong> Restoration of a previously deleted revision</item>
/// </list>
/// 
/// <para><strong>Database Configuration:</strong></para>
/// <list type="bullet">
/// <item>Primary key: GUID with identity generation</item>
/// <item>Activity constraint: StringLength(50) with required validation</item>
/// <item>Seeded data: Four standard revision activities</item>
/// <item>Relationships: One-to-many with RevisionActivityUser for audit trails</item>
/// </list>
/// 
/// <para><strong>Legal Compliance Support:</strong></para>
/// <list type="bullet">
/// <item>Standardized activity classification for legal audit requirements</item>
/// <item>Immutable reference data preserving audit trail integrity</item>
/// <item>Complete user attribution through RevisionActivityUser associations</item>
/// <item>Support for regulatory reporting and compliance audits</item>
/// </list>
/// 
/// <para><strong>Business Rules:</strong></para>
/// <list type="bullet">
/// <item>Activity names must match RevisionActivityValidationHelper.AllowedActivities</item>
/// <item>Activities are standardized and system-defined (not user-created)</item>
/// <item>Each activity can be associated with multiple users and revisions</item>
/// <item>Activity names are case-insensitive but stored in uppercase</item>
/// </list>
/// 
/// <para><strong>Entity Framework Integration:</strong></para>
/// The entity is configured in AdmsContext with:
/// <list type="bullet">
/// <item>Seeded data for all standard revision activities</item>
/// <item>Required relationships to RevisionActivityUser junction entity</item>
/// <item>Restricted cascade delete to preserve audit trail integrity</item>
/// <item>Performance optimization for frequent lookup operations</item>
/// </list>
/// </remarks>
public class RevisionActivity : IEquatable<RevisionActivity>, IComparable<RevisionActivity>, IValidatableObject
{
    #region Constants

    /// <summary>
    /// Contains the predefined GUIDs for seeded revision activities.
    /// </summary>
    /// <remarks>
    /// These constants ensure consistency across deployments and enable reliable reference
    /// in business logic and reporting without hardcoding GUIDs throughout the codebase.
    /// </remarks>
    public static class SeededActivityIds
    {
        /// <summary>The GUID for the CREATED activity type.</summary>
        public static readonly Guid Created = new("10000000-0000-0000-0000-000000000001");

        /// <summary>The GUID for the DELETED activity type.</summary>
        public static readonly Guid Deleted = new("10000000-0000-0000-0000-000000000002");

        /// <summary>The GUID for the RESTORED activity type.</summary>
        public static readonly Guid Restored = new("10000000-0000-0000-0000-000000000003");

        /// <summary>The GUID for the SAVED activity type.</summary>
        public static readonly Guid Saved = new("10000000-0000-0000-0000-000000000004");

        /// <summary>
        /// Gets a dictionary mapping activity names to their seeded GUIDs.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, Guid> ActivityNameToId = new Dictionary<string, Guid>
        {
            ["CREATED"] = Created,
            ["DELETED"] = Deleted,
            ["RESTORED"] = Restored,
            ["SAVED"] = Saved
        };
    }

    /// <summary>
    /// Contains standard activity type names as constants.
    /// </summary>
    public static class ActivityTypes
    {
        public const string Created = "CREATED";
        public const string Deleted = "DELETED";
        public const string Restored = "RESTORED";
        public const string Saved = "SAVED";
    }

    #endregion Constants

    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the revision activity.
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
    /// <item>Used as foreign key in RevisionActivityUser relationship table</item>
    /// <item>Seeded with specific GUIDs for standard activities</item>
    /// </list>
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// The ID remains constant throughout the activity's lifecycle and is used for all
    /// audit trail associations, business logic references, and reporting operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new RevisionActivity 
    /// { 
    ///     Activity = ActivityTypes.Created
    /// };
    /// // ID will be automatically generated when saved to database
    /// 
    /// // Accessing seeded activity
    /// var createdActivityId = SeededActivityIds.Created;
    /// </code>
    /// </example>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the description of the revision activity.
    /// </summary>
    /// <remarks>
    /// The activity name serves as the primary identifier and classifier for revision operations.
    /// This field must conform to the standardized activity names defined in 
    /// RevisionActivityValidationHelper.AllowedActivities to ensure consistency across the system.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Required field - cannot be null or empty</item>
    /// <item>Maximum length: 50 characters (database constraint)</item>
    /// <item>Minimum length: 1 character (business rule)</item>
    /// <item>Must be one of the allowed activities from RevisionActivityValidationHelper</item>
    /// <item>Must contain only letters, numbers, and underscores</item>
    /// <item>Must contain at least one letter</item>
    /// <item>Cannot use reserved activity names</item>
    /// </list>
    /// 
    /// <para><strong>Standard Activities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Initial creation of a document revision</item>
    /// <item><strong>SAVED:</strong> Saving changes to an existing revision</item>
    /// <item><strong>DELETED:</strong> Soft deletion of a revision</item>
    /// <item><strong>RESTORED:</strong> Restoration of a deleted revision</item>
    /// </list>
    /// 
    /// <para><strong>Business Context:</strong></para>
    /// Activity names are used throughout the system for:
    /// <list type="bullet">
    /// <item>Audit trail classification and reporting</item>
    /// <item>Business rule enforcement and workflow control</item>
    /// <item>User interface display and activity filtering</item>
    /// <item>Legal compliance reporting and analysis</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard seeded activities using constants
    /// var createdActivity = new RevisionActivity { Activity = ActivityTypes.Created };
    /// var savedActivity = new RevisionActivity { Activity = ActivityTypes.Saved };
    /// var deletedActivity = new RevisionActivity { Activity = ActivityTypes.Deleted };
    /// var restoredActivity = new RevisionActivity { Activity = ActivityTypes.Restored };
    /// 
    /// // Validation example
    /// bool isValid = Common.RevisionActivityValidationHelper.IsActivityAllowed(createdActivity.Activity);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Activity description is required and cannot be empty.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Activity description must be between 1 and 50 characters.")]
    public required string Activity { get; set; }

    #endregion Core Properties

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the collection of revision activity user associations for this activity.
    /// </summary>
    /// <remarks>
    /// This collection maintains the many-to-many relationship between revision activities, users,
    /// and revisions through the RevisionActivityUser junction entity. Each association represents
    /// a specific instance of this activity being performed by a user on a revision.
    /// 
    /// <para><strong>Audit Trail Functionality:</strong></para>
    /// Each association in this collection provides:
    /// <list type="bullet">
    /// <item>User attribution - which user performed this activity</item>
    /// <item>Revision context - which revision the activity was performed on</item>
    /// <item>Temporal tracking - when the activity occurred</item>
    /// <item>Unique identification - composite key preventing duplicates</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured in AdmsContext.ConfigureRevisionActivityUser</item>
    /// <item>Many-to-many relationship through junction entity</item>
    /// <item>Composite primary key includes ActivityId, RevisionId, UserId, CreatedAt</item>
    /// <item>Cascade behavior: Restricted to preserve audit trail integrity</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// Be cautious with direct access to this collection. Consider using repository methods
    /// or explicit loading to avoid N+1 query issues when working with multiple activities.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Safe iteration with null checks
    /// foreach (var activityUser in revisionActivity.RevisionActivityUsers ?? Enumerable.Empty&lt;RevisionActivityUser&gt;())
    /// {
    ///     var userName = activityUser.User?.Name ?? "Unknown User";
    ///     var revisionNumber = activityUser.Revision?.RevisionNumber?.ToString() ?? "Unknown";
    ///     Console.WriteLine($"User {userName} performed {revisionActivity.Activity} " +
    ///                      $"on revision {revisionNumber} at {activityUser.CreatedAt}");
    /// }
    /// </code>
    /// </example>
    public virtual ICollection<RevisionActivityUser> RevisionActivityUsers { get; set; } = new HashSet<RevisionActivityUser>();

    #endregion Navigation Properties

    #region Safe Computed Properties

    /// <summary>
    /// Gets a value indicating whether this activity is one of the standard seeded activities.
    /// </summary>
    /// <remarks>
    /// This property identifies whether the activity is one of the standard system-defined
    /// activities or a custom activity. Uses the validation helper for consistent logic.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (revisionActivity.IsStandardActivity)
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
        !string.IsNullOrWhiteSpace(Activity) &&
        Common.RevisionActivityValidationHelper.IsActivityAllowed(Activity);

    /// <summary>
    /// Gets the normalized version of the activity name.
    /// </summary>
    /// <remarks>
    /// This property provides a normalized version of the activity name following
    /// the normalization rules from RevisionActivityValidationHelper for consistent
    /// comparison and storage operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var normalized = revisionActivity.NormalizedActivity; // Always uppercase, trimmed
    /// bool areEqual = revisionActivity.NormalizedActivity == other.NormalizedActivity;
    /// </code>
    /// </example>
    [NotMapped]
    public string NormalizedActivity =>
        Common.RevisionActivityValidationHelper.NormalizeActivity(Activity) ??
        (Activity?.ToUpperInvariant() ?? string.Empty);

    /// <summary>
    /// Gets a value indicating whether this activity has any user associations loaded.
    /// </summary>
    /// <remarks>
    /// This property safely checks if the navigation property has been loaded and contains data.
    /// It does NOT trigger database queries and only checks the currently loaded collection.
    /// For accurate database counts, use repository methods with proper queries.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (revisionActivity.HasLoadedUserAssociations)
    /// {
    ///     // Work with loaded data
    ///     var loadedCount = revisionActivity.RevisionActivityUsers.Count;
    /// }
    /// else
    /// {
    ///     // Need to explicitly load or use repository method
    ///     await context.Entry(revisionActivity)
    ///         .Collection(ra => ra.RevisionActivityUsers)
    ///         .LoadAsync();
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasLoadedUserAssociations =>
        RevisionActivityUsers.Count > 0;

    #endregion Safe Computed Properties

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified RevisionActivity is equal to the current RevisionActivity.
    /// </summary>
    /// <param name="other">The RevisionActivity to compare with the current RevisionActivity.</param>
    /// <returns>true if the specified RevisionActivity is equal to the current RevisionActivity; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each activity has a unique identifier.
    /// This follows Entity Framework best practices for entity equality comparison.
    /// </remarks>
    public bool Equals(RevisionActivity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current RevisionActivity.
    /// </summary>
    /// <param name="obj">The object to compare with the current RevisionActivity.</param>
    /// <returns>true if the specified object is equal to the current RevisionActivity; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as RevisionActivity);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current RevisionActivity.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Compares the current RevisionActivity with another RevisionActivity for ordering purposes.
    /// </summary>
    /// <param name="other">The RevisionActivity to compare with the current RevisionActivity.</param>
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
    public int CompareTo(RevisionActivity? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by activity name for alphabetical ordering
        var activityComparison = string.Compare(Activity ?? string.Empty,
                                               other.Activity ?? string.Empty,
                                               StringComparison.OrdinalIgnoreCase);
        return activityComparison != 0 ? activityComparison :
            // Secondary sort by ID for consistency when activities have same name
            Id.CompareTo(other.Id);
    }

    /// <summary>
    /// Determines whether two RevisionActivity instances are equal.
    /// </summary>
    /// <param name="left">The first RevisionActivity to compare.</param>
    /// <param name="right">The second RevisionActivity to compare.</param>
    /// <returns>true if the RevisionActivities are equal; otherwise, false.</returns>
    public static bool operator ==(RevisionActivity? left, RevisionActivity? right) =>
        EqualityComparer<RevisionActivity>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two RevisionActivity instances are not equal.
    /// </summary>
    /// <param name="left">The first RevisionActivity to compare.</param>
    /// <param name="right">The second RevisionActivity to compare.</param>
    /// <returns>true if the RevisionActivities are not equal; otherwise, false.</returns>
    public static bool operator !=(RevisionActivity? left, RevisionActivity? right) =>
        !(left == right);

    /// <summary>
    /// Determines whether one RevisionActivity precedes another in the ordering.
    /// </summary>
    /// <param name="left">The first RevisionActivity to compare.</param>
    /// <param name="right">The second RevisionActivity to compare.</param>
    /// <returns>true if the left RevisionActivity precedes the right RevisionActivity; otherwise, false.</returns>
    public static bool operator <(RevisionActivity? left, RevisionActivity? right) =>
        Comparer<RevisionActivity>.Default.Compare(left, right) < 0;

    /// <summary>
    /// Determines whether one RevisionActivity precedes or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first RevisionActivity to compare.</param>
    /// <param name="right">The second RevisionActivity to compare.</param>
    /// <returns>true if the left RevisionActivity precedes or equals the right RevisionActivity; otherwise, false.</returns>
    public static bool operator <=(RevisionActivity? left, RevisionActivity? right) =>
        Comparer<RevisionActivity>.Default.Compare(left, right) <= 0;

    /// <summary>
    /// Determines whether one RevisionActivity follows another in the ordering.
    /// </summary>
    /// <param name="left">The first RevisionActivity to compare.</param>
    /// <param name="right">The second RevisionActivity to compare.</param>
    /// <returns>true if the left RevisionActivity follows the right RevisionActivity; otherwise, false.</returns>
    public static bool operator >(RevisionActivity? left, RevisionActivity? right) =>
        Comparer<RevisionActivity>.Default.Compare(left, right) > 0;

    /// <summary>
    /// Determines whether one RevisionActivity follows or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first RevisionActivity to compare.</param>
    /// <param name="right">The second RevisionActivity to compare.</param>
    /// <returns>true if the left RevisionActivity follows or equals the right RevisionActivity; otherwise, false.</returns>
    public static bool operator >=(RevisionActivity? left, RevisionActivity? right) =>
        Comparer<RevisionActivity>.Default.Compare(left, right) >= 0;

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the RevisionActivity.
    /// </summary>
    /// <returns>A string that represents the current RevisionActivity.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the activity,
    /// which is useful for debugging, logging, and display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new RevisionActivity 
    /// { 
    ///     Id = SeededActivityIds.Created, 
    ///     Activity = ActivityTypes.Created
    /// };
    /// 
    /// Console.WriteLine(activity);
    /// // Output: "RevisionActivity: CREATED (10000000-0000-0000-0000-000000000001)"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"RevisionActivity: {Activity ?? "Unknown"} ({Id})";

    #endregion String Representation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this activity represents a lifecycle creation operation.
    /// </summary>
    /// <returns>true if this is a creation activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify creation activities for
    /// business rule enforcement and audit trail analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (revisionActivity.IsCreationActivity())
    /// {
    ///     // Apply creation-specific business rules
    ///     Console.WriteLine("This is a revision creation activity");
    /// }
    /// </code>
    /// </example>
    public bool IsCreationActivity() =>
        string.Equals(Activity, ActivityTypes.Created, StringComparison.OrdinalIgnoreCase);

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
    /// if (revisionActivity.IsDeletionActivity())
    /// {
    ///     // Apply deletion-specific business rules
    ///     auditLogger.LogDeletion(revisionActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsDeletionActivity() =>
        string.Equals(Activity, ActivityTypes.Deleted, StringComparison.OrdinalIgnoreCase);

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
    /// if (revisionActivity.IsRestorationActivity())
    /// {
    ///     // Apply restoration-specific business rules
    ///     recoveryLogger.LogRestoration(revisionActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsRestorationActivity() =>
        string.Equals(Activity, ActivityTypes.Restored, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a save operation.
    /// </summary>
    /// <returns>true if this is a save activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify save activities for
    /// business rule enforcement and workflow analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (revisionActivity.IsSaveActivity())
    /// {
    ///     // Apply save-specific business rules
    ///     versionControl.ProcessSave(revisionActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsSaveActivity() =>
        string.Equals(Activity, ActivityTypes.Saved, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity is appropriate for the given revision context.
    /// </summary>
    /// <param name="revisionExists">Whether the revision already exists.</param>
    /// <param name="isDeleted">Whether the revision is currently deleted.</param>
    /// <returns>true if the activity is appropriate for the context; otherwise, false.</returns>
    /// <remarks>
    /// This method uses the RevisionActivityValidationHelper to validate business rules
    /// for activity context appropriateness.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool canApply = createdActivity.IsAppropriateForContext(false, false);  // true for new revision
    /// bool cannotApply = createdActivity.IsAppropriateForContext(true, false); // false for existing revision
    /// </code>
    /// </example>
    public bool IsAppropriateForContext(bool revisionExists, bool isDeleted)
    {
        if (string.IsNullOrWhiteSpace(Activity))
            return false;

        return Common.RevisionActivityValidationHelper.IsActivityAppropriateForContext(
            Activity, revisionExists, isDeleted);
    }

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
    /// var createdActivityId = RevisionActivity.GetSeededActivityId(ActivityTypes.Created);
    /// // Returns: 10000000-0000-0000-0000-000000000001
    /// </code>
    /// </example>
    public static Guid GetSeededActivityId(string activityName)
    {
        if (string.IsNullOrWhiteSpace(activityName))
            return Guid.Empty;

        var normalizedName = activityName.Trim().ToUpperInvariant();
        return SeededActivityIds.ActivityNameToId.TryGetValue(normalizedName, out var id)
            ? id
            : Guid.Empty;
    }

    /// <summary>
    /// Gets the activity name for a seeded GUID.
    /// </summary>
    /// <param name="activityId">The activity ID to get the name for.</param>
    /// <returns>The activity name if found; otherwise, null.</returns>
    /// <remarks>
    /// This method provides reverse lookup from GUID to activity name for seeded activities.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityName = RevisionActivity.GetSeededActivityName(SeededActivityIds.Created);
    /// // Returns: "CREATED"
    /// </code>
    /// </example>
    public static string? GetSeededActivityName(Guid activityId)
    {
        return SeededActivityIds.ActivityNameToId
            .FirstOrDefault(kvp => kvp.Value == activityId)
            .Key;
    }

    #endregion Business Logic Methods

    #region Validation Methods

    /// <summary>
    /// Validates the current RevisionActivity instance against business rules.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive validation using RevisionActivityValidationHelper
    /// including activity name validation, length constraints, and business rule compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// var validationResults = revisionActivity.Validate();
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

        // Validate using RevisionActivityValidationHelper
        if (!string.IsNullOrWhiteSpace(Activity))
        {
            results.AddRange(Common.RevisionActivityValidationHelper.ValidateActivity(Activity, nameof(Activity)));
        }

        // Additional entity-specific validations
        if (Id == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Activity ID cannot be empty.",
                new[] { nameof(Id) }));
        }

        return results;
    }

    /// <summary>
    /// Validates the object in the context of model validation.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results.</returns>
    /// <remarks>
    /// This method integrates with ASP.NET Core's model validation pipeline,
    /// enabling automatic validation when the entity is used in controllers or services.
    /// </remarks>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        return Validate();
    }

    #endregion Validation Methods

    #region Static Factory Methods

    /// <summary>
    /// Creates a new RevisionActivity instance for the CREATED activity type.
    /// </summary>
    /// <returns>A new RevisionActivity configured for creation activities.</returns>
    /// <remarks>
    /// This factory method ensures consistent creation of standard activity types
    /// and can be useful for maintaining type safety in business logic.
    /// </remarks>
    /// <example>
    /// <code>
    /// var createdActivity = RevisionActivity.ForCreated();
    /// // Equivalent to: new RevisionActivity { Activity = ActivityTypes.Created }
    /// </code>
    /// </example>
    public static RevisionActivity ForCreated() => new() { Activity = ActivityTypes.Created };

    /// <summary>
    /// Creates a new RevisionActivity instance for the SAVED activity type.
    /// </summary>
    /// <returns>A new RevisionActivity configured for save activities.</returns>
    public static RevisionActivity ForSaved() => new() { Activity = ActivityTypes.Saved };

    /// <summary>
    /// Creates a new RevisionActivity instance for the DELETED activity type.
    /// </summary>
    /// <returns>A new RevisionActivity configured for deletion activities.</returns>
    public static RevisionActivity ForDeleted() => new() { Activity = ActivityTypes.Deleted };

    /// <summary>
    /// Creates a new RevisionActivity instance for the RESTORED activity type.
    /// </summary>
    /// <returns>A new RevisionActivity configured for restoration activities.</returns>
    public static RevisionActivity ForRestored() => new() { Activity = ActivityTypes.Restored };

    #endregion Static Factory Methods
}