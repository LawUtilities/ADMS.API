using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Entities;

/// <summary>
/// Represents the linkage of a user to a matter document activity in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// The MatterDocumentActivityUser entity serves as the central junction table for document activities performed
/// on documents within matters by users in the legal document management system. This entity provides comprehensive
/// audit trails for document operations within the context of legal matters, ensuring complete traceability and
/// accountability for all document-related activities.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Comprehensive Audit Trail:</strong> Tracks all document activities within matter contexts</item>
/// <item><strong>Composite Primary Key:</strong> Ensures uniqueness while allowing temporal tracking</item>
/// <item><strong>User Attribution:</strong> Links document activities to responsible users</item>
/// <item><strong>Matter Integration:</strong> Connects document activities to specific legal cases</item>
/// <item><strong>Temporal Tracking:</strong> Maintains precise timestamps for all activities</item>
/// <item><strong>Legal Compliance:</strong> Supports comprehensive audit requirements</item>
/// </list>
/// 
/// <para><strong>Database Configuration:</strong></para>
/// <list type="bullet">
/// <item>Composite primary key: MatterId + DocumentId + MatterDocumentActivityId + UserId + CreatedAt</item>
/// <item>All foreign key relationships are required</item>
/// <item>NoAction cascade delete to preserve audit trail integrity</item>
/// <item>Indexed on CreatedAt for temporal queries</item>
/// </list>
/// 
/// <para><strong>Document Activities Supported:</strong></para>
/// This entity tracks various document activities within matter contexts:
/// <list type="bullet">
/// <item><strong>MOVED:</strong> Document moved between matters</item>
/// <item><strong>COPIED:</strong> Document copied between matters</item>
/// <item>Additional activities as supported by MatterDocumentActivity seeded data</item>
/// </list>
/// 
/// <para><strong>Audit Trail Functionality:</strong></para>
/// This entity enables comprehensive tracking of matter-document activities:
/// <list type="bullet">
/// <item><strong>Who:</strong> Which user performed the document activity</item>
/// <item><strong>What:</strong> What type of activity was performed (MOVED/COPIED)</item>
/// <item><strong>Where:</strong> Which matter and document were involved</item>
/// <item><strong>When:</strong> Precise timestamp of the activity</item>
/// </list>
/// 
/// <para><strong>Legal Compliance Support:</strong></para>
/// <list type="bullet">
/// <item>Complete document activity chains for legal discovery</item>
/// <item>Matter-specific audit trails for case management</item>
/// <item>User accountability for document operations</item>
/// <item>Temporal tracking for legal timeline reconstruction</item>
/// </list>
/// 
/// <para><strong>Business Rules:</strong></para>
/// <list type="bullet">
/// <item>Every matter document activity must be attributed to a user</item>
/// <item>Activities must be associated with valid matters and documents</item>
/// <item>Multiple activities of the same type can occur with different timestamps</item>
/// <item>CreatedAt timestamp must be within reasonable bounds</item>
/// <item>All foreign key relationships must reference valid entities</item>
/// </list>
/// 
/// <para><strong>Relationship to Directional Classes:</strong></para>
/// While MatterDocumentActivityUserFrom and MatterDocumentActivityUserTo track the directional
/// aspects of document transfers, this entity serves as the central record for all matter-document
/// activities, providing a unified view of document operations within matter contexts.
/// 
/// <para><strong>Entity Framework Configuration:</strong></para>
/// The entity is configured in AdmsContext with:
/// <list type="bullet">
/// <item>Composite key including all five core properties</item>
/// <item>Required relationships to Matter, Document, MatterDocumentActivity, and User</item>
/// <item>NoAction cascade behaviors to maintain audit trail integrity</item>
/// <item>Performance indexes on commonly queried fields</item>
/// </list>
/// </remarks>
public class MatterDocumentActivityUser : IEquatable<MatterDocumentActivityUser>, IComparable<MatterDocumentActivityUser>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier of the matter.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the matter where the document
    /// activity was performed. It forms part of the composite primary key and is required for
    /// all matter document activity user associations.
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
    /// <item>Represents the matter where the document activity occurred</item>
    /// <item>Links activity to specific legal case or project</item>
    /// <item>Enables matter-scoped document activity queries</item>
    /// <item>Supports matter-level audit trail aggregation</item>
    /// </list>
    /// 
    /// <para><strong>Legal Significance:</strong></para>
    /// The matter ID is crucial for legal document management as it establishes the context
    /// within which document activities occur, supporting case management and legal compliance
    /// requirements for document operation tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityUser = new MatterDocumentActivityUser
    /// {
    ///     MatterId = legalMatter.Id,  // Must reference valid matter
    ///     DocumentId = document.Id,
    ///     MatterDocumentActivityId = activityId,
    ///     UserId = performingUser.Id,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter ID is required for matter association.")]
    public Guid MatterId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the document.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the document on which the
    /// activity was performed. It forms part of the composite primary key and identifies the
    /// specific document involved in the matter document activity.
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to Document entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>NoAction cascade delete to preserve audit trail integrity</item>
    /// </list>
    /// 
    /// <para><strong>Activity Context:</strong></para>
    /// <list type="bullet">
    /// <item>Identifies the specific document being operated on</item>
    /// <item>Links activity to document metadata and content</item>
    /// <item>Enables document-scoped activity history queries</item>
    /// <item>Supports document lifecycle tracking</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Integration:</strong></para>
    /// The document association enables comprehensive tracking of all activities performed
    /// on documents within matter contexts, supporting legal discovery, compliance audits,
    /// and case management requirements for maintaining accurate document operation records.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityUser = new MatterDocumentActivityUser
    /// {
    ///     DocumentId = legalDocument.Id,  // Must reference valid document
    ///     MatterId = matter.Id,
    ///     MatterDocumentActivityId = activityId,
    ///     UserId = performingUser.Id,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Document ID is required for document association.")]
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the matter document activity.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the matter document activity type
    /// that was performed. It forms part of the composite primary key and must reference a valid
    /// matter document activity from the seeded data (such as MOVED or COPIED).
    /// 
    /// <para><strong>Activity Types:</strong></para>
    /// Must reference one of the seeded matter document activities:
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> Document moved between matters</item>
    /// <item><strong>COPIED:</strong> Document copied between matters</item>
    /// </list>
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to MatterDocumentActivity entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>NoAction cascade delete to preserve audit integrity</item>
    /// </list>
    /// 
    /// <para><strong>Validation Integration:</strong></para>
    /// The MatterDocumentActivityId is validated using MatterDocumentActivityValidationHelper
    /// to ensure it references a valid, allowed activity type for matter document operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Linking to a MOVED activity (seeded ID: 40000000-0000-0000-0000-000000000002)
    /// var movedActivityId = Guid.Parse("40000000-0000-0000-0000-000000000002");
    /// var activityUser = new MatterDocumentActivityUser
    /// {
    ///     MatterDocumentActivityId = movedActivityId,  // Must be valid activity
    ///     // ... other properties
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter Document Activity ID is required for activity classification.")]
    public Guid MatterDocumentActivityId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who performed the activity.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the user who performed the
    /// matter document activity. It forms part of the composite primary key and provides
    /// essential user attribution for audit trail purposes.
    /// 
    /// <para><strong>User Attribution:</strong></para>
    /// <list type="bullet">
    /// <item>Links activity to responsible user for accountability</item>
    /// <item>Enables user-scoped matter document activity reporting</item>
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
    /// <item>Document activity accountability and legal discovery</item>
    /// <item>Professional responsibility tracking for document operations</item>
    /// <item>Evidence of who performed specific matter document activities</item>
    /// <item>Compliance with legal practice standards for document management</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Attributing document activity to a specific user
    /// var activityUser = new MatterDocumentActivityUser
    /// {
    ///     UserId = performingUser.Id,  // Must reference valid user
    ///     MatterId = matter.Id,
    ///     DocumentId = document.Id,
    ///     MatterDocumentActivityId = activityId,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User ID is required for user attribution.")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this activity entry was created (in UTC).
    /// </summary>
    /// <remarks>
    /// This property maintains the precise timestamp of when the matter document activity
    /// occurred. It forms part of the composite primary key, enabling multiple activities of
    /// the same type with different timestamps while maintaining uniqueness.
    /// 
    /// <para><strong>Temporal Tracking:</strong></para>
    /// <list type="bullet">
    /// <item>Provides precise timing for matter document activity audit trails</item>
    /// <item>Enables temporal analysis of document operation patterns</item>
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
    /// <item>Should reflect the actual time the document activity occurred</item>
    /// <item>Used for chronological ordering in audit reports</item>
    /// <item>Critical for legal compliance and document activity tracking</item>
    /// </list>
    /// 
    /// <para><strong>Validation:</strong></para>
    /// The CreatedAt timestamp is validated using RevisionValidationHelper date validation
    /// methods to ensure it falls within acceptable ranges for legal document management.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityUser = new MatterDocumentActivityUser
    /// {
    ///     CreatedAt = DateTime.UtcNow,  // Always use UTC
    ///     // ... other properties
    /// };
    /// 
    /// // For historical data import
    /// var historicalActivity = new MatterDocumentActivityUser
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
    /// This navigation property provides access to the matter where the document activity was
    /// performed. The relationship is established through the MatterId foreign key and enables
    /// rich querying and navigation within Entity Framework for matter-specific operations.
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
    /// <item>Accessing matter metadata from activity context</item>
    /// <item>Matter-level document activity operations and reporting</item>
    /// <item>Audit trail reporting including matter information</item>
    /// <item>Cross-matter document activity analysis</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// The virtual modifier enables lazy loading, but consider explicit loading or
    /// projections when working with multiple activities to avoid N+1 query issues.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing matter information through activity
    /// var matterName = activityUser.Matter?.Description;
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
    /// Gets or sets the document associated with this activity.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the document on which the activity was
    /// performed. The relationship is established through the DocumentId foreign key and
    /// enables access to document metadata, content, and related information.
    /// 
    /// <para><strong>Document Information Access:</strong></para>
    /// Provides access to:
    /// <list type="bullet">
    /// <item>Document metadata including filename, size, and content type</item>
    /// <item>Document content and version information</item>
    /// <item>Related document operations and audit trails</item>
    /// <item>Cross-document activity analysis and reporting</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses DocumentId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>NoAction cascade delete behavior preserves audit trail integrity</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Integration:</strong></para>
    /// <list type="bullet">
    /// <item>Document-centric audit trail analysis and reporting</item>
    /// <item>Document lifecycle tracking within matter contexts</item>
    /// <item>Legal discovery and compliance support</item>
    /// <item>Cross-document activity pattern analysis</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing document information through activity
    /// var documentName = activityUser.Document?.FileName;
    /// var documentSize = activityUser.Document?.FileSize;
    /// 
    /// // Document activity history analysis
    /// var allActivities = activityUser.Document?.MatterDocumentActivityUsers
    ///     .OrderBy(a => a.CreatedAt);
    /// </code>
    /// </example>
    [ForeignKey(nameof(DocumentId))]
    public virtual required Document Document { get; set; }

    /// <summary>
    /// Gets or sets the matter document activity associated with this user.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the matter document activity type that was
    /// performed. The relationship is established through the MatterDocumentActivityId foreign
    /// key and enables access to activity metadata and classification.
    /// 
    /// <para><strong>Activity Information Access:</strong></para>
    /// Provides access to:
    /// <list type="bullet">
    /// <item>Activity type name (MOVED, COPIED, etc.)</item>
    /// <item>Activity metadata and configuration</item>
    /// <item>Activity validation rules and constraints</item>
    /// <item>Cross-activity analysis and reporting</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses MatterDocumentActivityId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>NoAction cascade delete behavior preserves audit integrity</item>
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
    /// var activityType = activityUser.MatterDocumentActivity?.Activity;
    /// var isMoveOperation = activityType == "MOVED";
    /// 
    /// // Activity-based filtering
    /// var copyOperations = activities
    ///     .Where(a => a.MatterDocumentActivity?.Activity == "COPIED");
    /// </code>
    /// </example>
    [ForeignKey(nameof(MatterDocumentActivityId))]
    public virtual required MatterDocumentActivity MatterDocumentActivity { get; set; }

    /// <summary>
    /// Gets or sets the user associated with this activity.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the user who performed the matter document
    /// activity. The relationship is established through the UserId foreign key and enables
    /// comprehensive user-based reporting and analysis.
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
    /// <item>NoAction cascade delete behavior preserves audit integrity</item>
    /// </list>
    /// 
    /// <para><strong>Legal and Compliance Support:</strong></para>
    /// <list type="bullet">
    /// <item>User accountability for matter document activities</item>
    /// <item>Professional responsibility tracking</item>
    /// <item>Legal discovery and audit trail support</item>
    /// <item>Compliance reporting and analysis</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing user information through activity
    /// var userName = activityUser.User?.Name;
    /// var userActivities = activityUser.User?.MatterDocumentActivityUsers;
    /// 
    /// // User-based activity analysis
    /// var userActivityCount = user.MatterDocumentActivityUsers.Count;
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
    /// valid (non-empty) GUID values, ensuring referential integrity for matter document
    /// activities.
    /// 
    /// <para><strong>Validation Checks:</strong></para>
    /// <list type="bullet">
    /// <item>MatterId is not Guid.Empty</item>
    /// <item>DocumentId is not Guid.Empty</item>
    /// <item>MatterDocumentActivityId is not Guid.Empty</item>
    /// <item>UserId is not Guid.Empty</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!activityUser.HasValidReferences)
    /// {
    ///     throw new InvalidOperationException("Activity record has invalid foreign key references");
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasValidReferences =>
        MatterId != Guid.Empty &&
        DocumentId != Guid.Empty &&
        MatterDocumentActivityId != Guid.Empty &&
        UserId != Guid.Empty;

    /// <summary>
    /// Gets a value indicating whether the CreatedAt timestamp is within reasonable bounds.
    /// </summary>
    /// <remarks>
    /// This computed property uses the RevisionValidationHelper to validate that the
    /// CreatedAt timestamp falls within acceptable date ranges for the legal document
    /// management system.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!activityUser.HasValidTimestamp)
    /// {
    ///     logger.LogWarning($"Activity record {activityUser} has invalid timestamp: {activityUser.CreatedAt}");
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
    /// This computed property provides a human-readable description of the matter document
    /// activity including user attribution, activity type, and timestamp information.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine(activityUser.ActivityDescription);
    /// // Output: "Document 'contract.pdf' MOVED in 'Corporate Matter' by rbrown at 2024-01-15 10:30 UTC"
    /// </code>
    /// </example>
    [NotMapped]
    public string ActivityDescription =>
        $"Document '{Document?.FileName ?? "Unknown"}' {MatterDocumentActivity?.Activity ?? "PROCESSED"} " +
        $"in '{Matter?.Description ?? "Unknown Matter"}' by {User?.Name ?? "Unknown User"} at {CreatedAt:yyyy-MM-dd HH:mm} UTC";

    #endregion Computed Properties

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified MatterDocumentActivityUser is equal to the current MatterDocumentActivityUser.
    /// </summary>
    /// <param name="other">The MatterDocumentActivityUser to compare with the current MatterDocumentActivityUser.</param>
    /// <returns>true if the specified MatterDocumentActivityUser is equal to the current MatterDocumentActivityUser; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all five components of the composite primary key:
    /// MatterId, DocumentId, MatterDocumentActivityId, UserId, and CreatedAt. This follows
    /// Entity Framework best practices for entities with composite keys.
    /// </remarks>
    public bool Equals(MatterDocumentActivityUser? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return MatterId.Equals(other.MatterId) &&
               DocumentId.Equals(other.DocumentId) &&
               MatterDocumentActivityId.Equals(other.MatterDocumentActivityId) &&
               UserId.Equals(other.UserId) &&
               CreatedAt.Equals(other.CreatedAt);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current MatterDocumentActivityUser.
    /// </summary>
    /// <param name="obj">The object to compare with the current MatterDocumentActivityUser.</param>
    /// <returns>true if the specified object is equal to the current MatterDocumentActivityUser; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as MatterDocumentActivityUser);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterDocumentActivityUser.</returns>
    /// <remarks>
    /// The hash code is computed from all five components of the composite primary key
    /// to ensure consistent hashing behavior that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => HashCode.Combine(MatterId, DocumentId, MatterDocumentActivityId, UserId, CreatedAt);

    /// <summary>
    /// Compares the current MatterDocumentActivityUser with another MatterDocumentActivityUser for ordering purposes.
    /// </summary>
    /// <param name="other">The MatterDocumentActivityUser to compare with the current MatterDocumentActivityUser.</param>
    /// <returns>
    /// A value that indicates the relative order of the activity records being compared.
    /// Less than zero: This activity precedes the other activity.
    /// Zero: This activity occurs in the same position as the other activity.
    /// Greater than zero: This activity follows the other activity.
    /// </returns>
    /// <remarks>
    /// Comparison is performed based on CreatedAt timestamp for chronological ordering,
    /// which is most useful for audit trail analysis and reporting. Secondary sorts
    /// provide consistency when timestamps are equal.
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
    public int CompareTo(MatterDocumentActivityUser? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by timestamp for chronological ordering
        var timeComparison = CreatedAt.CompareTo(other.CreatedAt);
        if (timeComparison != 0) return timeComparison;

        // Secondary sort by MatterId for consistency
        var matterComparison = MatterId.CompareTo(other.MatterId);
        if (matterComparison != 0) return matterComparison;

        // Tertiary sort by DocumentId
        var documentComparison = DocumentId.CompareTo(other.DocumentId);
        if (documentComparison != 0) return documentComparison;

        // Quaternary sort by ActivityId
        var activityComparison = MatterDocumentActivityId.CompareTo(other.MatterDocumentActivityId);
        return activityComparison != 0 ? activityComparison :
            // Final sort by UserId
            UserId.CompareTo(other.UserId);
    }

    /// <summary>
    /// Determines whether two MatterDocumentActivityUser instances are equal.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivityUser to compare.</param>
    /// <param name="right">The second MatterDocumentActivityUser to compare.</param>
    /// <returns>true if the MatterDocumentActivityUsers are equal; otherwise, false.</returns>
    public static bool operator ==(MatterDocumentActivityUser? left, MatterDocumentActivityUser? right) =>
        EqualityComparer<MatterDocumentActivityUser>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two MatterDocumentActivityUser instances are not equal.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivityUser to compare.</param>
    /// <param name="right">The second MatterDocumentActivityUser to compare.</param>
    /// <returns>true if the MatterDocumentActivityUsers are not equal; otherwise, false.</returns>
    public static bool operator !=(MatterDocumentActivityUser? left, MatterDocumentActivityUser? right) => !(left == right);

    /// <summary>
    /// Determines whether one MatterDocumentActivityUser precedes another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivityUser to compare.</param>
    /// <param name="right">The second MatterDocumentActivityUser to compare.</param>
    /// <returns>true if the left MatterDocumentActivityUser precedes the right MatterDocumentActivityUser; otherwise, false.</returns>
    public static bool operator <(MatterDocumentActivityUser? left, MatterDocumentActivityUser? right) =>
        left is not null && (right is null || left.CompareTo(right) < 0);

    /// <summary>
    /// Determines whether one MatterDocumentActivityUser precedes or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivityUser to compare.</param>
    /// <param name="right">The second MatterDocumentActivityUser to compare.</param>
    /// <returns>true if the left MatterDocumentActivityUser precedes or equals the right MatterDocumentActivityUser; otherwise, false.</returns>
    public static bool operator <=(MatterDocumentActivityUser? left, MatterDocumentActivityUser? right) =>
        left is null || (right is not null && left.CompareTo(right) <= 0);

    /// <summary>
    /// Determines whether one MatterDocumentActivityUser follows another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivityUser to compare.</param>
    /// <param name="right">The second MatterDocumentActivityUser to compare.</param>
    /// <returns>true if the left MatterDocumentActivityUser follows the right MatterDocumentActivityUser; otherwise, false.</returns>
    public static bool operator >(MatterDocumentActivityUser? left, MatterDocumentActivityUser? right) =>
        left is not null && (right is null || left.CompareTo(right) > 0);

    /// <summary>
    /// Determines whether one MatterDocumentActivityUser follows or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivityUser to compare.</param>
    /// <param name="right">The second MatterDocumentActivityUser to compare.</param>
    /// <returns>true if the left MatterDocumentActivityUser follows or equals the right MatterDocumentActivityUser; otherwise, false.</returns>
    public static bool operator >=(MatterDocumentActivityUser? left, MatterDocumentActivityUser? right) =>
        left is null || (right is not null && left.CompareTo(right) >= 0);

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterDocumentActivityUser.
    /// </summary>
    /// <returns>A string that represents the current MatterDocumentActivityUser.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the activity,
    /// which is useful for debugging, logging, and audit trail display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityUser = new MatterDocumentActivityUser
    /// {
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// Console.WriteLine(activityUser);
    /// // Output: "Document MOVED in Matter 'Corporate Case' by rbrown at 2024-01-15 10:30:00 UTC"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Document {MatterDocumentActivity?.Activity ?? "PROCESSED"} in Matter '{Matter?.Description ?? MatterId.ToString()[..8]}' " +
        $"by {User?.Name ?? UserId.ToString()[..8]} at {CreatedAt:yyyy-MM-dd HH:mm:ss} UTC";

    #endregion String Representation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this activity record represents a move operation.
    /// </summary>
    /// <returns>true if this is a move operation; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify move operations without
    /// directly accessing the navigation property, which can help avoid lazy loading
    /// in performance-sensitive scenarios.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityUser.IsMoveOperation())
    /// {
    ///     // Handle move-specific logic
    ///     Console.WriteLine("Document was moved in: " + activityUser.Matter?.Description);
    /// }
    /// </code>
    /// </example>
    public bool IsMoveOperation() => MatterDocumentActivity?.Activity == "MOVED";

    /// <summary>
    /// Determines whether this activity record represents a copy operation.
    /// </summary>
    /// <returns>true if this is a copy operation; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify copy operations for
    /// audit trail analysis and business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityUser.IsCopyOperation())
    /// {
    ///     // Handle copy-specific logic
    ///     auditLogger.LogDocumentCopy(activityUser);
    /// }
    /// </code>
    /// </example>
    public bool IsCopyOperation() => MatterDocumentActivity?.Activity == "COPIED";

    /// <summary>
    /// Determines whether this activity occurred recently within the specified timeframe.
    /// </summary>
    /// <param name="withinHours">The number of hours to consider as "recent".</param>
    /// <returns>true if the activity occurred within the specified timeframe; otherwise, false.</returns>
    /// <remarks>
    /// This method is useful for identifying recent matter document activities for notifications,
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
    /// Gets the seeded GUID for a specific matter document activity name.
    /// </summary>
    /// <param name="activityName">The activity name to get the GUID for.</param>
    /// <returns>The seeded GUID if found; otherwise, Guid.Empty.</returns>
    /// <remarks>
    /// This method returns the specific GUIDs used in database seeding for standard
    /// matter document activities, useful for business logic that needs to reference
    /// specific activity types.
    /// </remarks>
    /// <example>
    /// <code>
    /// var copiedActivityId = MatterDocumentActivityUser.GetSeededActivityId("COPIED");
    /// // Returns: 40000000-0000-0000-0000-000000000001
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

    #endregion Business Logic Methods

    #region Validation Methods

    /// <summary>
    /// Validates the current MatterDocumentActivityUser instance against business rules.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive validation using MatterDocumentActivityValidationHelper
    /// and additional entity-specific business rules including foreign key validation,
    /// timestamp validation, and activity appropriateness checking.
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

        if (DocumentId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Document ID cannot be empty.",
                [nameof(DocumentId)]));
        }

        if (MatterDocumentActivityId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Matter Document Activity ID cannot be empty.",
                [nameof(MatterDocumentActivityId)]));
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
        if (MatterDocumentActivity == null) return results;
        var hasUser = UserId != Guid.Empty;
        if (!Common.MatterDocumentActivityValidationHelper.IsActivityAllowed(MatterDocumentActivity.Activity))
        {
            results.Add(new ValidationResult(
                $"Activity '{MatterDocumentActivity.Activity}' is not allowed for matter document operations.",
                [nameof(MatterDocumentActivityId)]));
        }

        return results;
    }

    #endregion Validation Methods
}