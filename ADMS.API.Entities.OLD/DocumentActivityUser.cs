using ADMS.API.Common;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Entities;

/// <summary>
/// Represents the association between a document, a document activity, and a user in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// The DocumentActivityUser entity serves as a junction table implementing the many-to-many relationship
/// between documents, document activities, and users. This entity is critical for maintaining comprehensive
/// audit trails of all document-related operations within the legal document management system.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Audit Trail Foundation:</strong> Central component of document activity tracking</item>
/// <item><strong>Composite Primary Key:</strong> Ensures uniqueness while allowing temporal tracking</item>
/// <item><strong>User Attribution:</strong> Links every document activity to a responsible user</item>
/// <item><strong>Temporal Tracking:</strong> Maintains precise timestamps for all activities</item>
/// <item><strong>Legal Compliance:</strong> Supports comprehensive audit requirements</item>
/// </list>
/// 
/// <para><strong>Database Configuration:</strong></para>
/// <list type="bullet">
/// <item>Composite primary key: DocumentId + DocumentActivityId + UserId + CreatedAt</item>
/// <item>All foreign key relationships are required</item>
/// <item>Standard cascade behavior for referential integrity</item>
/// <item>Indexed on CreatedAt for temporal queries</item>
/// </list>
/// 
/// <para><strong>Document Activities Supported:</strong></para>
/// This entity tracks various document lifecycle activities:
/// <list type="bullet">
/// <item><strong>CREATED:</strong> Document creation activity</item>
/// <item><strong>SAVED:</strong> Document save activity</item>
/// <item><strong>DELETED:</strong> Document deletion activity</item>
/// <item><strong>RESTORED:</strong> Document restoration activity</item>
/// <item><strong>CHECKED IN:</strong> Document check-in activity</item>
/// <item><strong>CHECKED OUT:</strong> Document check-out activity</item>
/// </list>
/// 
/// <para><strong>Audit Trail Functionality:</strong></para>
/// This entity enables comprehensive tracking of document activities:
/// <list type="bullet">
/// <item><strong>Who:</strong> Which user performed the document activity</item>
/// <item><strong>What:</strong> What type of activity was performed (CREATED/SAVED/etc.)</item>
/// <item><strong>Where:</strong> Which document the activity was performed on</item>
/// <item><strong>When:</strong> Precise timestamp of the activity</item>
/// </list>
/// 
/// <para><strong>Legal Compliance Support:</strong></para>
/// <list type="bullet">
/// <item>Complete user attribution for all document operations</item>
/// <item>Temporal audit trails for legal discovery and compliance</item>
/// <item>Immutable record of document lifecycle activities</item>
/// <item>Support for regulatory reporting and audit requirements</item>
/// <item>Check-in/check-out tracking for document custody</item>
/// </list>
/// 
/// <para><strong>Business Rules:</strong></para>
/// <list type="bullet">
/// <item>Every document activity must be attributed to a user</item>
/// <item>Multiple activities of the same type can occur with different timestamps</item>
/// <item>CreatedAt timestamp must be within reasonable bounds</item>
/// <item>All foreign key relationships must reference valid entities</item>
/// <item>Check-out activities should have corresponding check-in activities</item>
/// </list>
/// 
/// <para><strong>Entity Framework Configuration:</strong></para>
/// The entity is configured in AdmsContext.ConfigureDocumentActivityUser with:
/// <list type="bullet">
/// <item>Composite key including all four core properties</item>
/// <item>Required relationships to Document, DocumentActivity, and User</item>
/// <item>Standard cascade behaviors for referential integrity</item>
/// <item>Performance indexes on commonly queried fields</item>
/// </list>
/// </remarks>
public class DocumentActivityUser : IEquatable<DocumentActivityUser>, IComparable<DocumentActivityUser>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier of the document.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the document that this activity
    /// is associated with. It forms part of the composite primary key and is required for all
    /// document activity user associations.
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to Document entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>Standard cascade behavior for referential integrity</item>
    /// </list>
    /// 
    /// <para><strong>Business Context:</strong></para>
    /// <list type="bullet">
    /// <item>Represents the document on which the activity was performed</item>
    /// <item>Links activity to specific legal document and matter</item>
    /// <item>Enables document-scoped activity queries</item>
    /// <item>Supports document-level audit trail aggregation</item>
    /// </list>
    /// 
    /// <para><strong>Legal Significance:</strong></para>
    /// The document ID is crucial for legal document management as it establishes which
    /// specific document was involved in the activity, supporting case management and
    /// legal compliance requirements for document operation tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityUser = new DocumentActivityUser
    /// {
    ///     DocumentId = legalDocument.Id,  // Must reference valid document
    ///     DocumentActivityId = activityId,
    ///     UserId = performingUser.Id,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Document ID is required for document association.")]
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the document activity.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the document activity type
    /// that was performed. It forms part of the composite primary key and must reference
    /// a valid document activity from the seeded data.
    /// 
    /// <para><strong>Activity Types:</strong></para>
    /// Must reference one of the seeded document activities:
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Document creation activity</item>
    /// <item><strong>SAVED:</strong> Document save activity</item>
    /// <item><strong>DELETED:</strong> Document deletion activity</item>
    /// <item><strong>RESTORED:</strong> Document restoration activity</item>
    /// <item><strong>CHECKED IN:</strong> Document check-in activity</item>
    /// <item><strong>CHECKED OUT:</strong> Document check-out activity</item>
    /// </list>
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to DocumentActivity entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>Standard cascade behavior for referential integrity</item>
    /// </list>
    /// 
    /// <para><strong>Validation Integration:</strong></para>
    /// The DocumentActivityId is validated using DocumentActivityValidationHelper
    /// to ensure it references a valid, allowed activity type.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Linking to a CREATED activity (seeded ID: 20000000-0000-0000-0000-000000000003)
    /// var createdActivityId = Guid.Parse("20000000-0000-0000-0000-000000000003");
    /// var activityUser = new DocumentActivityUser
    /// {
    ///     DocumentActivityId = createdActivityId,  // Must be valid activity
    ///     // ... other properties
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Document Activity ID is required for activity classification.")]
    public Guid DocumentActivityId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who performed the activity.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the user who performed
    /// the document activity. It forms part of the composite primary key and provides
    /// essential user attribution for audit trail purposes.
    /// 
    /// <para><strong>User Attribution:</strong></para>
    /// <list type="bullet">
    /// <item>Links activity to responsible user for accountability</item>
    /// <item>Enables user-scoped document activity reporting</item>
    /// <item>Supports legal compliance and audit requirements</item>
    /// <item>Facilitates user activity analytics and monitoring</item>
    /// </list>
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to User entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>Standard cascade behavior for referential integrity</item>
    /// </list>
    /// 
    /// <para><strong>Legal Significance:</strong></para>
    /// User attribution is critical for:
    /// <list type="bullet">
    /// <item>Legal discovery and compliance audits</item>
    /// <item>Professional responsibility tracking for document operations</item>
    /// <item>Accountability in legal document management</item>
    /// <item>Evidence of who performed specific document activities</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Attributing document activity to a specific user
    /// var activityUser = new DocumentActivityUser
    /// {
    ///     UserId = currentUser.Id,  // Must reference valid user
    ///     DocumentId = document.Id,
    ///     DocumentActivityId = activityId,
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
    /// This property maintains the precise timestamp of when the document activity occurred.
    /// It forms part of the composite primary key, enabling multiple activities of the same
    /// type with different timestamps while maintaining uniqueness.
    /// 
    /// <para><strong>Temporal Tracking:</strong></para>
    /// <list type="bullet">
    /// <item>Provides precise timing for document activity audit trails</item>
    /// <item>Enables temporal analysis of document lifecycle patterns</item>
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
    /// var activityUser = new DocumentActivityUser
    /// {
    ///     CreatedAt = DateTime.UtcNow,  // Always use UTC
    ///     // ... other properties
    /// };
    /// 
    /// // For historical data import
    /// var historicalActivity = new DocumentActivityUser
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
    /// Gets or sets the document associated with this activity.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the document on which the activity was
    /// performed. The relationship is established through the DocumentId foreign key and
    /// enables rich querying and navigation within Entity Framework.
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses DocumentId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>Standard cascade behavior for referential integrity</item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Accessing document metadata from activity context</item>
    /// <item>Document-level activity operations and reporting</item>
    /// <item>Audit trail reporting with document information</item>
    /// <item>Cross-document activity analysis</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// The virtual modifier enables lazy loading, but consider explicit loading or
    /// projections when working with multiple activities to avoid N+1 query issues.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing document information through activity
    /// var fileName = activityUser.Document?.FileName;
    /// var fileSize = activityUser.Document?.FileSize;
    /// 
    /// // Using explicit loading to avoid N+1 queries
    /// await context.Entry(activityUser)
    ///     .Reference(a => a.Document)
    ///     .LoadAsync();
    /// </code>
    /// </example>
    [ForeignKey(nameof(DocumentId))]
    public virtual required Document Document { get; set; }

    /// <summary>
    /// Gets or sets the document activity associated with this user.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the document activity type that was performed.
    /// The relationship is established through the DocumentActivityId foreign key and enables
    /// access to activity metadata such as the activity name and description.
    /// 
    /// <para><strong>Activity Information Access:</strong></para>
    /// Provides access to:
    /// <list type="bullet">
    /// <item>Activity name (CREATED, SAVED, DELETED, etc.)</item>
    /// <item>Activity metadata and configuration</item>
    /// <item>Activity validation rules and constraints</item>
    /// <item>Cross-activity analysis and reporting</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses DocumentActivityId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>Standard cascade behavior for referential integrity</item>
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
    /// var activityName = activityUser.DocumentActivity?.Activity;
    /// var isCreationActivity = activityName == "CREATED";
    /// 
    /// // Activity-based filtering
    /// var saveActivities = activities
    ///     .Where(a => a.DocumentActivity?.Activity == "SAVED");
    /// </code>
    /// </example>
    [ForeignKey(nameof(DocumentActivityId))]
    public virtual required DocumentActivity DocumentActivity { get; set; }

    /// <summary>
    /// Gets or sets the user associated with this document activity.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the user who performed the document activity.
    /// The relationship is established through the UserId foreign key and enables comprehensive
    /// user-based reporting and analysis of document activities.
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
    /// <item>Standard cascade behavior for referential integrity</item>
    /// </list>
    /// 
    /// <para><strong>Legal and Compliance Support:</strong></para>
    /// <list type="bullet">
    /// <item>User accountability for document activities</item>
    /// <item>Professional responsibility tracking</item>
    /// <item>Legal discovery and audit trail support</item>
    /// <item>Compliance reporting and analysis</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing user information through activity
    /// var userName = activityUser.User?.Name;
    /// var userActivities = activityUser.User?.DocumentActivityUsers;
    /// 
    /// // User-based activity analysis
    /// var userCreationCount = user.DocumentActivityUsers
    ///     .Count(au => au.DocumentActivity?.Activity == "CREATED");
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
    /// <item>DocumentId is not Guid.Empty</item>
    /// <item>DocumentActivityId is not Guid.Empty</item>
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
        DocumentId != Guid.Empty &&
        DocumentActivityId != Guid.Empty &&
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
    /// This computed property provides a human-readable description of the document activity
    /// including user attribution, activity type, and timestamp information.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine(activityUser.ActivityDescription);
    /// // Output: "rbrown performed CREATED on document 'contract.pdf' at 2024-01-15 10:30 UTC"
    /// </code>
    /// </example>
    [NotMapped]
    public string ActivityDescription =>
        $"{User?.Name ?? "Unknown User"} performed {DocumentActivity?.Activity ?? "Unknown Activity"} " +
        $"on document '{Document?.FileName ?? "Unknown"}' at {CreatedAt:yyyy-MM-dd HH:mm} UTC";

    #endregion Computed Properties

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified DocumentActivityUser is equal to the current DocumentActivityUser.
    /// </summary>
    /// <param name="other">The DocumentActivityUser to compare with the current DocumentActivityUser.</param>
    /// <returns>true if the specified DocumentActivityUser is equal to the current DocumentActivityUser; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all four components of the composite primary key:
    /// DocumentId, DocumentActivityId, UserId, and CreatedAt. This follows Entity Framework
    /// best practices for entities with composite keys.
    /// </remarks>
    public bool Equals(DocumentActivityUser? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return DocumentId.Equals(other.DocumentId) &&
               DocumentActivityId.Equals(other.DocumentActivityId) &&
               UserId.Equals(other.UserId) &&
               CreatedAt.Equals(other.CreatedAt);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current DocumentActivityUser.
    /// </summary>
    /// <param name="obj">The object to compare with the current DocumentActivityUser.</param>
    /// <returns>true if the specified object is equal to the current DocumentActivityUser; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as DocumentActivityUser);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current DocumentActivityUser.</returns>
    /// <remarks>
    /// The hash code is computed from all four components of the composite primary key
    /// to ensure consistent hashing behavior that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => HashCode.Combine(DocumentId, DocumentActivityId, UserId, CreatedAt);

    /// <summary>
    /// Compares the current DocumentActivityUser with another DocumentActivityUser for ordering purposes.
    /// </summary>
    /// <param name="other">The DocumentActivityUser to compare with the current DocumentActivityUser.</param>
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
    public int CompareTo(DocumentActivityUser? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by timestamp for chronological ordering
        var timeComparison = CreatedAt.CompareTo(other.CreatedAt);
        if (timeComparison != 0) return timeComparison;

        // Secondary sort by DocumentId for consistency
        var documentComparison = DocumentId.CompareTo(other.DocumentId);
        if (documentComparison != 0) return documentComparison;

        // Tertiary sort by ActivityId
        var activityComparison = DocumentActivityId.CompareTo(other.DocumentActivityId);
        return activityComparison != 0 ? activityComparison :
            // Final sort by UserId
            UserId.CompareTo(other.UserId);
    }

    /// <summary>
    /// Determines whether two DocumentActivityUser instances are equal.
    /// </summary>
    /// <param name="left">The first DocumentActivityUser to compare.</param>
    /// <param name="right">The second DocumentActivityUser to compare.</param>
    /// <returns>true if the DocumentActivityUsers are equal; otherwise, false.</returns>
    public static bool operator ==(DocumentActivityUser? left, DocumentActivityUser? right) =>
        EqualityComparer<DocumentActivityUser>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two DocumentActivityUser instances are not equal.
    /// </summary>
    /// <param name="left">The first DocumentActivityUser to compare.</param>
    /// <param name="right">The second DocumentActivityUser to compare.</param>
    /// <returns>true if the DocumentActivityUsers are not equal; otherwise, false.</returns>
    public static bool operator !=(DocumentActivityUser? left, DocumentActivityUser? right) => !(left == right);

    /// <summary>
    /// Determines whether one DocumentActivityUser precedes another in the ordering.
    /// </summary>
    /// <param name="left">The first DocumentActivityUser to compare.</param>
    /// <param name="right">The second DocumentActivityUser to compare.</param>
    /// <returns>true if the left DocumentActivityUser precedes the right DocumentActivityUser; otherwise, false.</returns>
    public static bool operator <(DocumentActivityUser? left, DocumentActivityUser? right) =>
        left is not null && (right is null || left.CompareTo(right) < 0);

    /// <summary>
    /// Determines whether one DocumentActivityUser precedes or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first DocumentActivityUser to compare.</param>
    /// <param name="right">The second DocumentActivityUser to compare.</param>
    /// <returns>true if the left DocumentActivityUser precedes or equals the right DocumentActivityUser; otherwise, false.</returns>
    public static bool operator <=(DocumentActivityUser? left, DocumentActivityUser? right) =>
        left is null || (right is not null && left.CompareTo(right) <= 0);

    /// <summary>
    /// Determines whether one DocumentActivityUser follows another in the ordering.
    /// </summary>
    /// <param name="left">The first DocumentActivityUser to compare.</param>
    /// <param name="right">The second DocumentActivityUser to compare.</param>
    /// <returns>true if the left DocumentActivityUser follows the right DocumentActivityUser; otherwise, false.</returns>
    public static bool operator >(DocumentActivityUser? left, DocumentActivityUser? right) =>
        left is not null && (right is null || left.CompareTo(right) > 0);

    /// <summary>
    /// Determines whether one DocumentActivityUser follows or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first DocumentActivityUser to compare.</param>
    /// <param name="right">The second DocumentActivityUser to compare.</param>
    /// <returns>true if the left DocumentActivityUser follows or equals the right DocumentActivityUser; otherwise, false.</returns>
    public static bool operator >=(DocumentActivityUser? left, DocumentActivityUser? right) =>
        left is null || (right is not null && left.CompareTo(right) >= 0);

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the DocumentActivityUser.
    /// </summary>
    /// <returns>A string that represents the current DocumentActivityUser.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the activity,
    /// which is useful for debugging, logging, and audit trail display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityUser = new DocumentActivityUser
    /// {
    ///     DocumentId = documentGuid,
    ///     DocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// Console.WriteLine(activityUser);
    /// // Output: "User rbrown performed CREATED on Document 'contract.pdf' at 2024-01-15 10:30:00 UTC"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"User {User?.Name ?? UserId.ToString()[..8]} performed {DocumentActivity?.Activity ?? "Activity"} " +
        $"on Document '{Document?.FileName ?? DocumentId.ToString()[..8]}' at {CreatedAt:yyyy-MM-dd HH:mm:ss} UTC";

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
    ///     Console.WriteLine("This document was created by: " + activityUser.User?.Name);
    /// }
    /// </code>
    /// </example>
    public bool IsCreationActivity() => DocumentActivity?.Activity == "CREATED";

    /// <summary>
    /// Determines whether this activity record represents a save activity.
    /// </summary>
    /// <returns>true if this is a save activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify save activities for
    /// audit trail analysis and business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityUser.IsSaveActivity())
    /// {
    ///     // Handle save-specific logic
    ///     Console.WriteLine("Document was saved by: " + activityUser.User?.Name);
    /// }
    /// </code>
    /// </example>
    public bool IsSaveActivity() => DocumentActivity?.Activity == "SAVED";

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
    public bool IsDeletionActivity() => DocumentActivity?.Activity == "DELETED";

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
    public bool IsRestorationActivity() => DocumentActivity?.Activity == "RESTORED";

    /// <summary>
    /// Determines whether this activity record represents a check-in activity.
    /// </summary>
    /// <returns>true if this is a check-in activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify check-in activities for
    /// document custody tracking and version control management.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityUser.IsCheckInActivity())
    /// {
    ///     // Handle check-in-specific logic
    ///     versionControlLogger.LogCheckIn(activityUser);
    /// }
    /// </code>
    /// </example>
    public bool IsCheckInActivity() => DocumentActivity?.Activity == "CHECKED IN";

    /// <summary>
    /// Determines whether this activity record represents a check-out activity.
    /// </summary>
    /// <returns>true if this is a check-out activity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify check-out activities for
    /// document custody tracking and version control management.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityUser.IsCheckOutActivity())
    /// {
    ///     // Handle check-out-specific logic
    ///     versionControlLogger.LogCheckOut(activityUser);
    /// }
    /// </code>
    /// </example>
    public bool IsCheckOutActivity() => DocumentActivity?.Activity == "CHECKED OUT";

    /// <summary>
    /// Determines whether this activity occurred recently within the specified timeframe.
    /// </summary>
    /// <param name="withinHours">The number of hours to consider as "recent".</param>
    /// <returns>true if the activity occurred within the specified timeframe; otherwise, false.</returns>
    /// <remarks>
    /// This method is useful for identifying recent document activities for notifications,
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
    /// Gets the seeded GUID for a specific document activity name.
    /// </summary>
    /// <param name="activityName">The activity name to get the GUID for.</param>
    /// <returns>The seeded GUID if found; otherwise, Guid.Empty.</returns>
    /// <remarks>
    /// This method returns the specific GUIDs used in database seeding for standard document activities,
    /// useful for business logic that needs to reference specific activity types.
    /// </remarks>
    /// <example>
    /// <code>
    /// var createdActivityId = DocumentActivityUser.GetSeededActivityId("CREATED");
    /// // Returns: 20000000-0000-0000-0000-000000000003
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

    #endregion Business Logic Methods

    #region Validation Methods

    /// <summary>
    /// Validates the current DocumentActivityUser instance against business rules.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive validation using DocumentActivityValidationHelper
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
        if (DocumentId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Document ID cannot be empty.",
                [nameof(DocumentId)]));
        }

        if (DocumentActivityId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Document Activity ID cannot be empty.",
                [nameof(DocumentActivityId)]));
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
        if (DocumentActivity == null) return results;
        if (!DocumentActivityValidationHelper.IsActivityAllowed(DocumentActivity.Activity))
        {
            results.Add(new ValidationResult(
                $"Activity '{DocumentActivity.Activity}' is not allowed for document operations.",
                [nameof(DocumentActivityId)]));
        }

        return results;
    }

    #endregion Validation Methods
}