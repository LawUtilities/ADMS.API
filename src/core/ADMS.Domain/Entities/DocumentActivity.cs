using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Domain.Entities;

/// <summary>
/// Represents an activity that can be performed on a document in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// The DocumentActivity entity serves as a lookup table for the standardized activities that can be
/// performed on documents within the legal document management system. This entity is critical for
/// maintaining comprehensive audit trails and ensuring consistent activity classification across all
/// document operations.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Static Reference Data:</strong> Contains predefined activity types seeded from database</item>
/// <item><strong>Document Operations Focus:</strong> Specializes in document lifecycle and version control operations</item>
/// <item><strong>Audit Trail Foundation:</strong> Central classification system for document activities</item>
/// <item><strong>Business Rule Enforcement:</strong> Ensures only valid document activities are recorded</item>
/// <item><strong>Legal Compliance:</strong> Supports comprehensive audit requirements</item>
/// <item><strong>Version Control Integration:</strong> Supports document check-in/check-out workflows</item>
/// </list>
/// 
/// <para><strong>Seeded Activities:</strong></para>
/// The following document activities are seeded in AdmsContext.SeedDocumentActivities:
/// <list type="bullet">
/// <item><strong>CHECKED IN:</strong> Document checked into version control system</item>
/// <item><strong>CHECKED OUT:</strong> Document checked out for editing</item>
/// <item><strong>CREATED:</strong> Initial document creation</item>
/// <item><strong>DELETED:</strong> Document marked for deletion (soft delete)</item>
/// <item><strong>RESTORED:</strong> Deleted document restored to active status</item>
/// <item><strong>SAVED:</strong> Document saved with changes</item>
/// </list>
/// 
/// <para><strong>Database Configuration:</strong></para>
/// <list type="bullet">
/// <item>Primary key: GUID with identity generation</item>
/// <item>Activity constraint: StringLength(50) with required validation</item>
/// <item>Seeded data: Six standard document lifecycle activities</item>
/// <item>Relationships: One-to-many with DocumentActivityUser for audit trails</item>
/// </list>
/// 
/// <para><strong>Document Lifecycle Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Creation Operations:</strong> CREATED for new document establishment</item>
/// <item><strong>Version Control Operations:</strong> CHECKED IN/CHECKED OUT for document custody</item>
/// <item><strong>Modification Operations:</strong> SAVED for document content changes</item>
/// <item><strong>Lifecycle Operations:</strong> DELETED/RESTORED for document state management</item>
/// </list>
/// 
/// <para><strong>Legal Compliance Support:</strong></para>
/// <list type="bullet">
/// <item>Standardized activity classification for legal audit requirements</item>
/// <item>Immutable reference data preserving audit trail integrity</item>
/// <item>Complete user attribution through DocumentActivityUser associations</item>
/// <item>Support for regulatory reporting and compliance audits</item>
/// <item>Document custody tracking through check-in/check-out operations</item>
/// </list>
/// 
/// <para><strong>Business Rules:</strong></para>
/// <list type="bullet">
/// <item>Activity names must match DocumentActivityValidationHelper.AllowedActivities</item>
/// <item>Activities are standardized and system-defined (not user-created)</item>
/// <item>Each activity can be associated with multiple users and documents</item>
/// <item>Activity names are case-insensitive but stored in uppercase</item>
/// <item>Check-out activities should have corresponding check-in activities</item>
/// </list>
/// 
/// <para><strong>Entity Framework Integration:</strong></para>
/// The entity is configured in AdmsContext with:
/// <list type="bullet">
/// <item>Seeded data for all standard document lifecycle activities</item>
/// <item>Required relationships to DocumentActivityUser junction entity</item>
/// <item>Standard cascade delete behavior for referential integrity</item>
/// <item>Performance optimization for frequent lookup operations</item>
/// </list>
/// 
/// <para><strong>Version Control Integration:</strong></para>
/// The DocumentActivity entity supports sophisticated version control workflows through
/// check-in/check-out operations, enabling proper document custody tracking and preventing
/// simultaneous editing conflicts in legal document management scenarios.
/// </remarks>
public class DocumentActivity : IEquatable<DocumentActivity>, IComparable<DocumentActivity>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the document activity.
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
    /// <item>Used as foreign key in DocumentActivityUser relationship table</item>
    /// <item>Seeded with specific GUIDs for standard document activities</item>
    /// </list>
    /// 
    /// <para><strong>Seeded Activity IDs:</strong></para>
    /// <list type="bullet">
    /// <item>CHECKED IN: 20000000-0000-0000-0000-000000000001</item>
    /// <item>CHECKED OUT: 20000000-0000-0000-0000-000000000002</item>
    /// <item>CREATED: 20000000-0000-0000-0000-000000000003</item>
    /// <item>DELETED: 20000000-0000-0000-0000-000000000004</item>
    /// <item>RESTORED: 20000000-0000-0000-0000-000000000005</item>
    /// <item>SAVED: 20000000-0000-0000-0000-000000000006</item>
    /// </list>
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// The ID remains constant throughout the activity's lifecycle and is used for all
    /// audit trail associations, business logic references, and reporting operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new DocumentActivity 
    /// { 
    ///     Activity = "CUSTOM_DOCUMENT_ACTIVITY"
    /// };
    /// // ID will be automatically generated when saved to database
    /// 
    /// // Accessing seeded activity
    /// var createdActivityId = Guid.Parse("20000000-0000-0000-0000-000000000003");
    /// </code>
    /// </example>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name or description of the document activity.
    /// </summary>
    /// <remarks>
    /// The activity name serves as the primary identifier and classifier for document operations.
    /// This field must conform to the standardized activity names defined in 
    /// DocumentActivityValidationHelper.AllowedActivities to ensure consistency across the system.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Required field - cannot be null or empty</item>
    /// <item>Maximum length: 50 characters (database constraint)</item>
    /// <item>Minimum length: 2 characters (business rule)</item>
    /// <item>Must be one of the allowed activities from DocumentActivityValidationHelper</item>
    /// <item>Must contain only letters, numbers, and spaces</item>
    /// <item>Must contain at least one letter</item>
    /// <item>Cannot use reserved activity names</item>
    /// </list>
    /// 
    /// <para><strong>Standard Activities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CHECKED IN:</strong> Document checked into version control</item>
    /// <item><strong>CHECKED OUT:</strong> Document checked out for editing</item>
    /// <item><strong>CREATED:</strong> Document creation activity</item>
    /// <item><strong>DELETED:</strong> Document deletion activity</item>
    /// <item><strong>RESTORED:</strong> Document restoration activity</item>
    /// <item><strong>SAVED:</strong> Document save activity</item>
    /// </list>
    /// 
    /// <para><strong>Business Context:</strong></para>
    /// Activity names are used throughout the system for:
    /// <list type="bullet">
    /// <item>Document lifecycle classification and reporting</item>
    /// <item>Version control workflow management</item>
    /// <item>Business rule enforcement and workflow control</item>
    /// <item>User interface display and activity filtering</item>
    /// <item>Legal compliance reporting and analysis</item>
    /// <item>Audit trail generation and document operation tracking</item>
    /// </list>
    /// 
    /// <para><strong>Validation Integration:</strong></para>
    /// Activity names are validated using DocumentActivityValidationHelper to ensure
    /// they conform to business rules and legal compliance requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard seeded activities
    /// var checkedInActivity = new DocumentActivity { Activity = "CHECKED IN" };
    /// var checkedOutActivity = new DocumentActivity { Activity = "CHECKED OUT" };
    /// var createdActivity = new DocumentActivity { Activity = "CREATED" };
    /// var deletedActivity = new DocumentActivity { Activity = "DELETED" };
    /// var restoredActivity = new DocumentActivity { Activity = "RESTORED" };
    /// var savedActivity = new DocumentActivity { Activity = "SAVED" };
    /// 
    /// // Validation example
    /// bool isValid = Common.DocumentActivityValidationHelper.IsActivityAllowed(createdActivity.Activity);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Activity description is required and cannot be empty.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Activity description must be between 2 and 50 characters.")]
    public required string Activity { get; set; }

    #endregion Core Properties

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the collection of document activity user associations for this activity.
    /// </summary>
    /// <remarks>
    /// This collection maintains the one-to-many relationship between this document activity and
    /// the user associations. Each association represents a specific instance of this activity
    /// being performed by a user on a document.
    /// 
    /// <para><strong>Document Activity Tracking:</strong></para>
    /// Each association in this collection provides:
    /// <list type="bullet">
    /// <item>User attribution - which user performed this activity</item>
    /// <item>Document context - which document the activity was performed on</item>
    /// <item>Temporal tracking - when the activity occurred</item>
    /// <item>Unique identification - composite key preventing duplicates</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured in AdmsContext.ConfigureDocumentActivityUser</item>
    /// <item>One-to-many relationship from activity to user associations</item>
    /// <item>Composite primary key includes ActivityId in the DocumentActivityUser entity</item>
    /// <item>Standard cascade behavior for referential integrity</item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Activity-based document audit trail analysis</item>
    /// <item>User activity reporting and monitoring</item>
    /// <item>Document lifecycle tracking</item>
    /// <item>Version control operation analysis</item>
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
    /// foreach (var activityUser in documentActivity.DocumentActivityUsers)
    /// {
    ///     Console.WriteLine($"User {activityUser.User?.Name} performed {documentActivity.Activity} " +
    ///                      $"on document {activityUser.Document?.FileName} at {activityUser.CreatedAt}");
    /// }
    /// 
    /// // Finding all users who performed CREATED activity
    /// var createdActivity = activities.FirstOrDefault(a => a.Activity == "CREATED");
    /// var creators = createdActivity?.DocumentActivityUsers
    ///     .Select(au => au.User)
    ///     .Distinct();
    /// 
    /// // Activity usage statistics
    /// var usageCount = documentActivity.DocumentActivityUsers.Count;
    /// var uniqueUsers = documentActivity.DocumentActivityUsers
    ///     .Select(au => au.UserId)
    ///     .Distinct()
    ///     .Count();
    /// </code>
    /// </example>
    public virtual ICollection<DocumentActivityUser> DocumentActivityUsers { get; set; } = new HashSet<DocumentActivityUser>();

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
    /// if (!documentActivity.HasUserAssociations)
    /// {
    ///     logger.LogInformation($"Activity {documentActivity.Activity} has no user associations");
    /// }
    /// 
    /// // Finding unused activities
    /// var unusedActivities = activities.Where(a => !a.HasUserAssociations).ToList();
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasUserAssociations => DocumentActivityUsers.Count > 0;

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
    /// Console.WriteLine($"Activity {documentActivity.Activity} has been used {documentActivity.UsageCount} times");
    /// 
    /// // Finding most used activities
    /// var mostUsedActivities = activities
    ///     .OrderByDescending(a => a.UsageCount)
    ///     .Take(10);
    /// </code>
    /// </example>
    [NotMapped]
    public int UsageCount => DocumentActivityUsers.Count;

    /// <summary>
    /// Gets the count of unique users who have performed this activity.
    /// </summary>
    /// <remarks>
    /// This computed property provides insight into the breadth of user engagement
    /// with this activity type, useful for user adoption analysis and training needs assessment.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Activity {documentActivity.Activity} has been performed by {documentActivity.UniqueUserCount} different users");
    /// 
    /// // Analyzing user engagement
    /// var engagementMetric = documentActivity.UniqueUserCount / (double)documentActivity.UsageCount;
    /// </code>
    /// </example>
    [NotMapped]
    public int UniqueUserCount => DocumentActivityUsers
        .Select(au => au.UserId)
        .Distinct()
        .Count();

    /// <summary>
    /// Gets the count of unique documents this activity has been performed on.
    /// </summary>
    /// <remarks>
    /// This computed property provides insight into the breadth of document engagement
    /// with this activity type, useful for document lifecycle analysis and system usage patterns.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Activity {documentActivity.Activity} has been performed on {documentActivity.UniqueDocumentCount} different documents");
    /// </code>
    /// </example>
    [NotMapped]
    public int UniqueDocumentCount => DocumentActivityUsers
        .Select(au => au.DocumentId)
        .Distinct()
        .Count();

    /// <summary>
    /// Gets a value indicating whether this activity is one of the standard seeded activities.
    /// </summary>
    /// <remarks>
    /// This property identifies whether the activity is one of the standard system-defined
    /// activities or a custom activity. Standard activities are those seeded in the database
    /// and validated by DocumentActivityValidationHelper.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (documentActivity.IsStandardActivity)
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
        DocumentActivityValidationHelper.IsActivityAllowed(Activity);

    /// <summary>
    /// Gets the normalized version of the activity name.
    /// </summary>
    /// <remarks>
    /// This property provides a normalized version of the activity name following
    /// the normalization rules from DocumentActivityValidationHelper for consistent
    /// comparison and storage operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var normalized = documentActivity.NormalizedActivity; // Always uppercase, trimmed
    /// bool areEqual = documentActivity.NormalizedActivity == other.NormalizedActivity;
    /// </code>
    /// </example>
    [NotMapped]
    public string NormalizedActivity =>
        DocumentActivityValidationHelper.NormalizeActivity(Activity) ?? Activity.ToUpperInvariant();

    /// <summary>
    /// Gets the activity category for classification purposes.
    /// </summary>
    /// <remarks>
    /// This property categorizes the activity based on its type for reporting and analysis purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var category = documentActivity.ActivityCategory;
    /// // Returns: "Lifecycle", "Version Control", or "Unknown"
    /// </code>
    /// </example>
    [NotMapped]
    public string ActivityCategory => Activity.ToUpperInvariant() switch
    {
        "CREATED" or "DELETED" or "RESTORED" or "SAVED" => "Lifecycle",
        "CHECKED IN" or "CHECKED OUT" => "Version Control",
        _ => "Unknown"
    };

    #endregion Computed Properties

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified DocumentActivity is equal to the current DocumentActivity.
    /// </summary>
    /// <param name="other">The DocumentActivity to compare with the current DocumentActivity.</param>
    /// <returns>true if the specified DocumentActivity is equal to the current DocumentActivity; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each activity has a unique identifier.
    /// This follows Entity Framework best practices for entity equality comparison.
    /// </remarks>
    public bool Equals(DocumentActivity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current DocumentActivity.
    /// </summary>
    /// <param name="obj">The object to compare with the current DocumentActivity.</param>
    /// <returns>true if the specified object is equal to the current DocumentActivity; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as DocumentActivity);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current DocumentActivity.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Compares the current DocumentActivity with another DocumentActivity for ordering purposes.
    /// </summary>
    /// <param name="other">The DocumentActivity to compare with the current DocumentActivity.</param>
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
    public int CompareTo(DocumentActivity? other)
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
    /// Determines whether two DocumentActivity instances are equal.
    /// </summary>
    /// <param name="left">The first DocumentActivity to compare.</param>
    /// <param name="right">The second DocumentActivity to compare.</param>
    /// <returns>true if the DocumentActivities are equal; otherwise, false.</returns>
    public static bool operator ==(DocumentActivity? left, DocumentActivity? right) =>
        EqualityComparer<DocumentActivity>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two DocumentActivity instances are not equal.
    /// </summary>
    /// <param name="left">The first DocumentActivity to compare.</param>
    /// <param name="right">The second DocumentActivity to compare.</param>
    /// <returns>true if the DocumentActivities are not equal; otherwise, false.</returns>
    public static bool operator !=(DocumentActivity? left, DocumentActivity? right) => !(left == right);

    /// <summary>
    /// Determines whether one DocumentActivity precedes another in the ordering.
    /// </summary>
    /// <param name="left">The first DocumentActivity to compare.</param>
    /// <param name="right">The second DocumentActivity to compare.</param>
    /// <returns>true if the left DocumentActivity precedes the right DocumentActivity; otherwise, false.</returns>
    public static bool operator <(DocumentActivity? left, DocumentActivity? right) =>
        left is not null && (right is null || left.CompareTo(right) < 0);

    /// <summary>
    /// Determines whether one DocumentActivity precedes or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first DocumentActivity to compare.</param>
    /// <param name="right">The second DocumentActivity to compare.</param>
    /// <returns>true if the left DocumentActivity precedes or equals the right DocumentActivity; otherwise, false.</returns>
    public static bool operator <=(DocumentActivity? left, DocumentActivity? right) =>
        left is null || (right is not null && left.CompareTo(right) <= 0);

    /// <summary>
    /// Determines whether one DocumentActivity follows another in the ordering.
    /// </summary>
    /// <param name="left">The first DocumentActivity to compare.</param>
    /// <param name="right">The second DocumentActivity to compare.</param>
    /// <returns>true if the left DocumentActivity follows the right DocumentActivity; otherwise, false.</returns>
    public static bool operator >(DocumentActivity? left, DocumentActivity? right) =>
        left is not null && (right is null || left.CompareTo(right) > 0);

    /// <summary>
    /// Determines whether one DocumentActivity follows or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first DocumentActivity to compare.</param>
    /// <param name="right">The second DocumentActivity to compare.</param>
    /// <returns>true if the left DocumentActivity follows or equals the right DocumentActivity; otherwise, false.</returns>
    public static bool operator >=(DocumentActivity? left, DocumentActivity? right) =>
        left is null || (right is not null && left.CompareTo(right) >= 0);

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the DocumentActivity.
    /// </summary>
    /// <returns>A string that represents the current DocumentActivity.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the activity,
    /// which is useful for debugging, logging, and display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new DocumentActivity 
    /// { 
    ///     Id = Guid.Parse("20000000-0000-0000-0000-000000000003"), 
    ///     Activity = "CREATED"
    /// };
    /// 
    /// Console.WriteLine(activity);
    /// // Output: "DocumentActivity: CREATED (20000000-0000-0000-0000-000000000003)"
    /// </code>
    /// </example>
    public override string ToString() => $"DocumentActivity: {Activity} ({Id})";

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
    /// if (documentActivity.IsCreationActivity())
    /// {
    ///     // Apply creation-specific business rules
    ///     Console.WriteLine("This is a document creation activity");
    /// }
    /// </code>
    /// </example>
    public bool IsCreationActivity() =>
        string.Equals(Activity, "CREATED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a save operation.
    /// </summary>
    /// <returns>true if this is a save activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify save activities for
    /// audit trail analysis and business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (documentActivity.IsSaveActivity())
    /// {
    ///     // Apply save-specific business rules
    ///     auditLogger.LogSave(documentActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsSaveActivity() =>
        string.Equals(Activity, "SAVED", StringComparison.OrdinalIgnoreCase);

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
    /// if (documentActivity.IsDeletionActivity())
    /// {
    ///     // Apply deletion-specific business rules
    ///     auditLogger.LogDeletion(documentActivity);
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
    /// if (documentActivity.IsRestorationActivity())
    /// {
    ///     // Apply restoration-specific business rules
    ///     recoveryLogger.LogRestoration(documentActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsRestorationActivity() =>
        string.Equals(Activity, "RESTORED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a check-in operation.
    /// </summary>
    /// <returns>true if this is a check-in activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify check-in activities for
    /// version control management and document custody tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (documentActivity.IsCheckInActivity())
    /// {
    ///     // Apply check-in-specific business rules
    ///     versionControlService.ProcessCheckIn(documentActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsCheckInActivity() =>
        string.Equals(Activity, "CHECKED IN", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a check-out operation.
    /// </summary>
    /// <returns>true if this is a check-out activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify check-out activities for
    /// version control management and document custody tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (documentActivity.IsCheckOutActivity())
    /// {
    ///     // Apply check-out-specific business rules
    ///     versionControlService.ProcessCheckOut(documentActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsCheckOutActivity() =>
        string.Equals(Activity, "CHECKED OUT", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a version control operation (check-in or check-out).
    /// </summary>
    /// <returns>true if this is a version control activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify any version control activity
    /// for business rule enforcement and workflow analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (documentActivity.IsVersionControlActivity())
    /// {
    ///     // Apply version control-specific business rules
    ///     versionControlService.ProcessActivity(documentActivity);
    /// }
    /// </code>
    /// </example>
    public bool IsVersionControlActivity() => IsCheckInActivity() || IsCheckOutActivity();

    /// <summary>
    /// Gets the seeded GUID for a specific document activity name.
    /// </summary>
    /// <param name="activityName">The activity name to get the GUID for.</param>
    /// <returns>The seeded GUID if found; otherwise, Guid.Empty.</returns>
    /// <remarks>
    /// This method returns the specific GUIDs used in database seeding for standard activities,
    /// useful for business logic that needs to reference specific activity types.
    /// </remarks>
    /// <example>
    /// <code>
    /// var createdActivityId = DocumentActivity.GetSeededActivityId("CREATED");
    /// // Returns: 20000000-0000-0000-0000-000000000003
    /// 
    /// var checkedInActivityId = DocumentActivity.GetSeededActivityId("CHECKED IN");
    /// // Returns: 20000000-0000-0000-0000-000000000001
    /// </code>
    /// </example>
    public static Guid GetSeededActivityId(string activityName)
    {
        if (string.IsNullOrWhiteSpace(activityName))
            return Guid.Empty;

        return activityName.Trim().ToUpperInvariant() switch
        {
            "CHECKED IN" => Guid.Parse("20000000-0000-0000-0000-000000000001"),
            "CHECKED OUT" => Guid.Parse("20000000-0000-0000-0000-000000000002"),
            "CREATED" => Guid.Parse("20000000-0000-0000-0000-000000000003"),
            "DELETED" => Guid.Parse("20000000-0000-0000-0000-000000000004"),
            "RESTORED" => Guid.Parse("20000000-0000-0000-0000-000000000005"),
            "SAVED" => Guid.Parse("20000000-0000-0000-0000-000000000006"),
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
    /// var stats = documentActivity.GetUsageStatistics();
    /// Console.WriteLine($"Total usage: {stats["TotalUsage"]}");
    /// Console.WriteLine($"Unique users: {stats["UniqueUsers"]}");
    /// Console.WriteLine($"Unique documents: {stats["UniqueDocuments"]}");
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
            ["UniqueDocuments"] = UniqueDocumentCount,
            ["IsStandardActivity"] = IsStandardActivity,
            ["HasUserAssociations"] = HasUserAssociations,
            ["IsVersionControlActivity"] = IsVersionControlActivity()
        };
    }

    #endregion Business Logic Methods

    #region Validation Methods

    /// <summary>
    /// Validates the current DocumentActivity instance against business rules.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive validation using DocumentActivityValidationHelper
    /// including activity name validation, length constraints, and business rule compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// var validationResults = documentActivity.Validate();
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

        // Validate using DocumentActivityValidationHelper
        results.AddRange(DocumentActivityValidationHelper.ValidateActivity(Activity, nameof(Activity)));

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