using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using ADMS.API.Common;

namespace ADMS.API.Models;

/// <summary>
/// Minimal Data Transfer Object representing the association between a revision, revision activity, and user for audit trail purposes.
/// </summary>
/// <remarks>
/// This DTO serves as a lightweight representation of the audit trail junction entity 
/// <see cref="ADMS.API.Entities.RevisionActivityUser"/>, providing essential information for audit log displays 
/// and activity tracking within the ADMS legal document management system. It maintains the complete audit 
/// trail relationship while minimizing data transfer overhead.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Audit Trail Focus:</strong> Optimized for displaying revision activity audit trails and logs</item>
/// <item><strong>Entity Synchronization:</strong> Mirrors the essential properties from ADMS.API.Entities.RevisionActivityUser</item>
/// <item><strong>Minimal Data Transfer:</strong> Contains only essential information needed for audit displays</item>
/// <item><strong>Comprehensive Validation:</strong> Uses centralized validation helpers for data integrity</item>
/// <item><strong>Immutable Design:</strong> Record type with init-only properties for thread-safe operations</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO represents the many-to-many-to-many relationship from ADMS.API.Entities.RevisionActivityUser:
/// <list type="bullet">
/// <item><strong>Revision Association:</strong> Links to the specific document revision via RevisionId and RevisionMinimalDto</item>
/// <item><strong>Activity Classification:</strong> Identifies the type of activity performed via RevisionActivityId and RevisionActivityMinimalDto</item>
/// <item><strong>User Attribution:</strong> Provides user accountability via UserId and UserMinimalDto</item>
/// <item><strong>Temporal Tracking:</strong> Records when the activity occurred via CreatedAt timestamp</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Audit Log Display:</strong> Primary use case for displaying revision activity history</item>
/// <item><strong>Activity Timeline:</strong> Chronological display of revision-related activities</item>
/// <item><strong>User Activity Reports:</strong> User-centric reporting showing revision activities</item>
/// <item><strong>Compliance Reporting:</strong> Legal compliance reports requiring activity attribution</item>
/// <item><strong>API Responses:</strong> Lightweight audit data in REST API responses</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Audit Compliance:</strong> Maintains complete activity attribution for legal compliance</item>
/// <item><strong>Professional Responsibility:</strong> Clear user attribution for professional accountability</item>
/// <item><strong>Legal Discovery:</strong> Supports legal discovery with comprehensive activity tracking</item>
/// <item><strong>Document Integrity:</strong> Maintains document version control audit trails</item>
/// </list>
/// 
/// <para><strong>Data Integrity:</strong></para>
/// <list type="bullet">
/// <item><strong>Referential Consistency:</strong> All foreign keys validated against their respective entities</item>
/// <item><strong>Temporal Accuracy:</strong> Activity timestamps validated for chronological consistency</item>
/// <item><strong>Cross-Reference Validation:</strong> Ensures consistency between IDs and navigation properties</item>
/// <item><strong>Complete Attribution:</strong> Guarantees all activities have proper user and timestamp attribution</item>
/// </list>
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Properties:</strong> Only essential properties to minimize memory footprint</item>
/// <item><strong>Immutable Design:</strong> Thread-safe for concurrent audit log access</item>
/// <item><strong>Validation Optimization:</strong> Efficient validation using centralized helpers</item>
/// <item><strong>Display Ready:</strong> Pre-formatted display properties to avoid runtime calculations</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating an audit trail entry for a revision creation activity
/// var auditEntry = new RevisionActivityUserMinimalDto
/// {
///     RevisionId = revisionGuid,
///     Revision = new RevisionMinimalDto { Id = revisionGuid, RevisionNumber = 2 },
///     RevisionActivityId = activityGuid,
///     RevisionActivity = new RevisionActivityMinimalDto { Id = activityGuid, Activity = "CREATED" },
///     UserId = userGuid,
///     User = new UserMinimalDto { Id = userGuid, Name = "Robert Brown" },
///     CreatedAt = DateTime.UtcNow
/// };
/// 
/// // Displaying in audit log
/// Console.WriteLine($"Audit Entry: {auditEntry.User.Name} {auditEntry.RevisionActivity.Activity} " +
///                  $"revision {auditEntry.Revision.RevisionNumber} at {auditEntry.LocalCreatedAtDateString}");
/// 
/// // Validating audit entry
/// var validationResults = RevisionActivityUserMinimalDto.ValidateModel(auditEntry);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Validation Error: {result.ErrorMessage}");
///     }
/// }
/// </code>
/// </example>
public record RevisionActivityUserMinimalDto : IValidatableObject, IEquatable<RevisionActivityUserMinimalDto>
{
    #region Core Properties

    /// <summary>
    /// Gets the unique identifier of the revision associated with this activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the foreign key to the revision that this activity was performed on,
    /// corresponding to <see cref="ADMS.API.Entities.RevisionActivityUser.RevisionId"/>. It establishes
    /// the relationship to the specific document revision in the audit trail.
    /// 
    /// <para><strong>Relationship Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required Association:</strong> Every activity must be linked to a valid revision</item>
    /// <item><strong>Non-Empty GUID:</strong> Cannot be Guid.Empty or default value</item>
    /// <item><strong>Entity Reference:</strong> Must correspond to an existing ADMS.API.Entities.Revision</item>
    /// <item><strong>Audit Integrity:</strong> Critical for maintaining complete audit trail linkage</item>
    /// </list>
    /// 
    /// <para><strong>Usage in Audit Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Attribution:</strong> Links activities to specific document revisions</item>
    /// <item><strong>Version Control:</strong> Enables revision-specific audit trail queries</item>
    /// <item><strong>Legal Compliance:</strong> Maintains precise activity-to-revision associations</item>
    /// <item><strong>Cross-Reference Validation:</strong> Must match Revision.Id when both are provided</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.RevisionActivityUser.RevisionId"/> exactly,
    /// ensuring consistency between entity and DTO representations for audit trail integrity.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Linking activity to specific revision
    /// var auditEntry = new RevisionActivityUserMinimalDto
    /// {
    ///     RevisionId = Guid.Parse("12345678-1234-5678-9012-123456789012"),
    ///     RevisionActivityId = activityId,
    ///     UserId = userId,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Querying audit trail by revision
    /// var revisionAuditTrail = auditEntries
    ///     .Where(entry => entry.RevisionId == targetRevisionId)
    ///     .OrderBy(entry => entry.CreatedAt);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Revision ID is required.")]
    public required Guid RevisionId { get; init; }

    /// <summary>
    /// Gets the minimal revision details associated with this activity.
    /// </summary>
    /// <remarks>
    /// This property provides the essential revision information needed for audit trail display,
    /// corresponding to the <see cref="ADMS.API.Entities.RevisionActivityUser.Revision"/> navigation property.
    /// It includes only the minimal data needed for audit log presentation and user interface display.
    /// 
    /// <para><strong>Display Information Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Revision Number:</strong> Sequential version number for user-friendly display</item>
    /// <item><strong>Creation/Modification Dates:</strong> Temporal context for the revision</item>
    /// <item><strong>Deletion Status:</strong> Whether the revision has been soft-deleted</item>
    /// <item><strong>Document Association:</strong> Optional document context when needed</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Version Identification:</strong> Enables display of "Revision 3 created by User"</item>
    /// <item><strong>Context Provision:</strong> Provides revision context without full entity loading</item>
    /// <item><strong>Cross-Reference Validation:</strong> ID must match RevisionId for consistency</item>
    /// <item><strong>Display Optimization:</strong> Includes pre-calculated display properties</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Must not be null - required for complete audit information</item>
    /// <item>Must pass its own validation rules</item>
    /// <item>Revision.Id must match RevisionId when both are provided</item>
    /// <item>Must represent a valid revision entity</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Audit trail display using revision details
    /// var displayText = $"User {auditEntry.User.Name} {auditEntry.RevisionActivity.Activity} " +
    ///                  $"{auditEntry.Revision.CompactDisplayText} on {auditEntry.LocalCreatedAtDateString}";
    /// 
    /// // Accessing revision information for audit context
    /// if (auditEntry.Revision.IsDeleted)
    /// {
    ///     displayText += " (Revision is now deleted)";
    /// }
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Revision details are required.")]
    public required RevisionMinimalDto Revision { get; init; }

    /// <summary>
    /// Gets the unique identifier of the revision activity performed.
    /// </summary>
    /// <remarks>
    /// This GUID identifies the specific type of activity that was performed on the revision,
    /// corresponding to <see cref="ADMS.API.Entities.RevisionActivityUser.RevisionActivityId"/>.
    /// It establishes the relationship to the activity classification in the audit trail.
    /// 
    /// <para><strong>Activity Classification:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Type Identification:</strong> Links to specific activity types (CREATED, SAVED, etc.)</item>
    /// <item><strong>Audit Categorization:</strong> Enables filtering and categorization of audit entries</item>
    /// <item><strong>Compliance Reporting:</strong> Supports activity-based compliance reporting</item>
    /// <item><strong>Cross-Reference Validation:</strong> Must match RevisionActivity.Id when both are provided</item>
    /// </list>
    /// 
    /// <para><strong>Supported Activity Types:</strong></para>
    /// Based on <see cref="ADMS.API.Entities.RevisionActivity"/> seeded data:
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Revision creation activities</item>
    /// <item><strong>SAVED:</strong> Revision save/update activities</item>
    /// <item><strong>DELETED:</strong> Revision soft deletion activities</item>
    /// <item><strong>RESTORED:</strong> Revision restoration from deleted state</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.RevisionActivityUser.RevisionActivityId"/> exactly,
    /// ensuring consistency between entity and DTO representations for audit trail integrity.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Filtering audit trail by activity type
    /// var creationActivities = auditEntries
    ///     .Where(entry => entry.RevisionActivity.Activity == "CREATED")
    ///     .OrderBy(entry => entry.CreatedAt);
    /// 
    /// // Activity-based audit reporting
    /// var activitySummary = auditEntries
    ///     .GroupBy(entry => entry.RevisionActivity.Activity)
    ///     .Select(g => new { Activity = g.Key, Count = g.Count() });
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Revision activity ID is required.")]
    public required Guid RevisionActivityId { get; init; }

    /// <summary>
    /// Gets the minimal revision activity details for this audit entry.
    /// </summary>
    /// <remarks>
    /// This property provides the essential activity information needed for audit trail display,
    /// corresponding to the <see cref="ADMS.API.Entities.RevisionActivityUser.RevisionActivity"/> navigation property.
    /// It includes the activity type and classification needed for comprehensive audit presentation.
    /// 
    /// <para><strong>Activity Information Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Name:</strong> Human-readable activity description (e.g., "CREATED", "SAVED")</item>
    /// <item><strong>Activity ID:</strong> Unique identifier for cross-referencing</item>
    /// <item><strong>Validation Rules:</strong> Activity-specific validation and business rules</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Display Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Human-Readable Activities:</strong> Enables display of "User CREATED revision"</item>
    /// <item><strong>Activity Categorization:</strong> Supports filtering and grouping of audit entries</item>
    /// <item><strong>Cross-Reference Validation:</strong> ID must match RevisionActivityId for consistency</item>
    /// <item><strong>Compliance Reporting:</strong> Provides activity classification for legal reporting</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Must not be null - required for complete audit information</item>
    /// <item>Must pass RevisionActivityMinimalDto validation rules</item>
    /// <item>RevisionActivity.Id must match RevisionActivityId when both are provided</item>
    /// <item>Activity must be a valid, recognized activity type</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Audit trail display using activity details
    /// var activityDescription = auditEntry.RevisionActivity.Activity switch
    /// {
    ///     "CREATED" => "created",
    ///     "SAVED" => "saved changes to",
    ///     "DELETED" => "deleted",
    ///     "RESTORED" => "restored",
    ///     _ => "performed unknown action on"
    /// };
    /// 
    /// var auditMessage = $"{auditEntry.User.Name} {activityDescription} " +
    ///                    $"revision {auditEntry.Revision.RevisionNumber} at {auditEntry.LocalCreatedAtDateString}";
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Revision activity details are required.")]
    public required RevisionActivityMinimalDto RevisionActivity { get; init; }

    /// <summary>
    /// Gets the unique identifier of the user who performed the activity.
    /// </summary>
    /// <remarks>
    /// This GUID identifies the user who performed the revision activity, corresponding to
    /// <see cref="ADMS.API.Entities.RevisionActivityUser.UserId"/>. It provides the essential
    /// user attribution required for complete audit trail accountability.
    /// 
    /// <para><strong>User Attribution Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required Association:</strong> Every activity must have proper user attribution</item>
    /// <item><strong>Non-Empty GUID:</strong> Cannot be Guid.Empty or default value</item>
    /// <item><strong>Entity Reference:</strong> Must correspond to an existing ADMS.API.Entities.User</item>
    /// <item><strong>Accountability:</strong> Critical for legal and professional accountability</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Professional Responsibility:</strong> Maintains user accountability for document activities</item>
    /// <item><strong>Audit Trail Integrity:</strong> Ensures complete attribution of all revision activities</item>
    /// <item><strong>Legal Discovery:</strong> Supports legal discovery with user-specific activity tracking</item>
    /// <item><strong>Cross-Reference Validation:</strong> Must match User.Id when both are provided</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.RevisionActivityUser.UserId"/> exactly,
    /// ensuring consistency between entity and DTO representations for audit trail integrity.
    /// </remarks>
    /// <example>
    /// <code>
    /// // User-based audit trail filtering
    /// var userActivities = auditEntries
    ///     .Where(entry => entry.UserId == targetUserId)
    ///     .OrderByDescending(entry => entry.CreatedAt);
    /// 
    /// // User activity summary
    /// var userActivitySummary = auditEntries
    ///     .Where(entry => entry.UserId == targetUserId)
    ///     .GroupBy(entry => entry.RevisionActivity.Activity)
    ///     .Select(g => new { Activity = g.Key, Count = g.Count() });
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User ID is required.")]
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the minimal user details for the user who performed the activity.
    /// </summary>
    /// <remarks>
    /// This property provides the essential user information needed for audit trail display,
    /// corresponding to the <see cref="ADMS.API.Entities.RevisionActivityUser.User"/> navigation property.
    /// It includes the user identification information required for comprehensive audit presentation.
    /// 
    /// <para><strong>User Information Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Name:</strong> Professional display name for audit trail presentation</item>
    /// <item><strong>User ID:</strong> Unique identifier for cross-referencing and validation</item>
    /// <item><strong>Display Properties:</strong> Pre-calculated display text for efficient rendering</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Display Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Professional Attribution:</strong> Enables display of "Robert Brown created revision"</item>
    /// <item><strong>User Identification:</strong> Clear user identification in audit logs</item>
    /// <item><strong>Cross-Reference Validation:</strong> ID must match UserId for consistency</item>
    /// <item><strong>Professional Compliance:</strong> Maintains professional naming standards</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Must not be null - required for complete audit information</item>
    /// <item>Must pass UserMinimalDto validation rules</item>
    /// <item>User.Id must match UserId when both are provided</item>
    /// <item>Must represent a valid user entity</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional audit trail display
    /// var auditDisplay = $"{auditEntry.User.DisplayText} {auditEntry.RevisionActivity.Activity.ToLower()} " +
    ///                   $"{auditEntry.Revision.CompactDisplayText} on {auditEntry.LocalCreatedAtDateString}";
    /// 
    /// // User-centric audit reporting
    /// Console.WriteLine($"Activity by {auditEntry.User.Name}: {auditEntry.RevisionActivity.Activity} " +
    ///                  $"on revision {auditEntry.Revision.RevisionNumber}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User details are required.")]
    public required UserMinimalDto User { get; init; }

    /// <summary>
    /// Gets the UTC date and time when the activity was performed.
    /// </summary>
    /// <remarks>
    /// This property records the precise timestamp when the revision activity occurred,
    /// corresponding to <see cref="ADMS.API.Entities.RevisionActivityUser.CreatedAt"/>. It provides
    /// the temporal context essential for audit trail chronology and legal compliance.
    /// 
    /// <para><strong>Temporal Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>UTC Storage:</strong> All timestamps stored in UTC for global consistency</item>
    /// <item><strong>Audit Chronology:</strong> Enables proper chronological ordering of audit entries</item>
    /// <item><strong>Legal Compliance:</strong> Provides precise timing for legal document management</item>
    /// <item><strong>Valid Range:</strong> Must be within reasonable temporal bounds</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Timeline:</strong> Establishes when specific activities occurred</item>
    /// <item><strong>Chronological Ordering:</strong> Enables proper sequencing of audit entries</item>
    /// <item><strong>Legal Evidence:</strong> Provides temporal evidence for legal compliance</item>
    /// <item><strong>Activity Analysis:</strong> Supports temporal analysis of user and system behavior</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Must be a valid DateTime value</item>
    /// <item>Should be within reasonable temporal bounds (not too far in past/future)</item>
    /// <item>Must be greater than system minimum date (January 1, 1980)</item>
    /// <item>Should not be in the future (with tolerance for clock skew)</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.RevisionActivityUser.CreatedAt"/> exactly,
    /// ensuring consistency between entity and DTO representations for audit trail integrity.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Creating audit entry with current timestamp
    /// var auditEntry = new RevisionActivityUserMinimalDto
    /// {
    ///     RevisionId = revisionId,
    ///     RevisionActivityId = activityId,
    ///     UserId = userId,
    ///     CreatedAt = DateTime.UtcNow // Current UTC timestamp
    /// };
    /// 
    /// // Chronological audit trail sorting
    /// var chronologicalAuditTrail = auditEntries
    ///     .OrderBy(entry => entry.CreatedAt)
    ///     .ToList();
    /// 
    /// // Recent activity filtering
    /// var recentActivities = auditEntries
    ///     .Where(entry => entry.CreatedAt >= DateTime.UtcNow.AddDays(-7))
    ///     .OrderByDescending(entry => entry.CreatedAt);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Activity timestamp is required.")]
    public required DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    #endregion Core Properties

    #region Computed Properties

    /// <summary>
    /// Gets the activity timestamp formatted as a localized string for UI display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a user-friendly formatted representation of the activity timestamp
    /// converted to local time, optimized for audit trail display and user interface presentation.
    /// 
    /// <para><strong>Display Format:</strong></para>
    /// Uses "dddd, dd MMMM yyyy HH:mm:ss" format for complete temporal information:
    /// <list type="bullet">
    /// <item><strong>Day of Week:</strong> "Monday" - provides additional temporal context</item>
    /// <item><strong>Full Date:</strong> "15 January 2024" - complete date information</item>
    /// <item><strong>Precise Time:</strong> "14:30:45" - exact time for audit precision</item>
    /// </list>
    /// 
    /// <para><strong>Localization Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User-Friendly:</strong> Displays in user's local time zone for better understanding</item>
    /// <item><strong>Professional Format:</strong> Appropriate for legal and professional documentation</item>
    /// <item><strong>Audit Trail Ready:</strong> Formatted specifically for audit log presentation</item>
    /// <item><strong>Cultural Adaptation:</strong> Respects local date and time formatting conventions</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var auditEntry = new RevisionActivityUserMinimalDto 
    /// { 
    ///     CreatedAt = new DateTime(2024, 1, 15, 14, 30, 45, DateTimeKind.Utc)
    ///     // ... other properties
    /// };
    /// 
    /// // Display in audit log UI
    /// auditLogLabel.Text = $"Activity performed on: {auditEntry.LocalCreatedAtDateString}";
    /// // Output (if local time is UTC+2): "Activity performed on: Monday, 15 January 2024 16:30:45"
    /// 
    /// // Comprehensive audit entry display
    /// var fullAuditDisplay = $"{auditEntry.User.Name} {auditEntry.RevisionActivity.Activity} " +
    ///                       $"{auditEntry.Revision.CompactDisplayText} on {auditEntry.LocalCreatedAtDateString}";
    /// </code>
    /// </example>
    public string LocalCreatedAtDateString => CreatedAt.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets a value indicating whether this audit entry has valid data for system operations.
    /// </summary>
    /// <remarks>
    /// This property provides a quick validation check without running full validation logic,
    /// useful for UI scenarios where immediate feedback is needed before displaying audit entries.
    /// 
    /// <para><strong>Validation Checks Performed:</strong></para>
    /// <list type="bullet">
    /// <item><strong>GUID Validation:</strong> All required GUIDs are non-empty</item>
    /// <item><strong>Date Validation:</strong> CreatedAt is within valid temporal bounds</item>
    /// <item><strong>Object Validation:</strong> All required nested objects are present</item>
    /// <item><strong>Cross-Reference Consistency:</strong> IDs match their corresponding navigation properties</item>
    /// </list>
    /// 
    /// <para><strong>Performance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Quick Check:</strong> Rapid validation without comprehensive rule evaluation</item>
    /// <item><strong>UI Responsiveness:</strong> Immediate feedback for user interface scenarios</item>
    /// <item><strong>Batch Processing:</strong> Efficient filtering of valid entries in collections</item>
    /// <item><strong>Error Prevention:</strong> Early detection of invalid audit entries</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Quick validation before processing
    /// if (auditEntry.IsValid)
    /// {
    ///     // Process valid audit entry
    ///     DisplayAuditEntry(auditEntry);
    /// }
    /// else
    /// {
    ///     // Handle invalid entry
    ///     logger.LogWarning($"Invalid audit entry detected: {auditEntry}");
    /// }
    /// 
    /// // Batch filtering of valid audit entries
    /// var validAuditEntries = auditEntries
    ///     .Where(entry => entry.IsValid)
    ///     .ToList();
    /// </code>
    /// </example>
    public bool IsValid =>
        RevisionId != Guid.Empty &&
        RevisionActivityId != Guid.Empty &&
        UserId != Guid.Empty &&
        RevisionValidationHelper.IsValidDate(CreatedAt);

    /// <summary>
    /// Gets a comprehensive display text suitable for audit trail presentation.
    /// </summary>
    /// <remarks>
    /// This property provides a complete, human-readable description of the audit entry,
    /// combining all essential information into a single, professional audit trail message.
    /// 
    /// <para><strong>Display Format:</strong></para>
    /// Follows the pattern: "[User Name] [Activity] [Revision] on [Timestamp]"
    /// <list type="bullet">
    /// <item><strong>User Attribution:</strong> Clear identification of who performed the activity</item>
    /// <item><strong>Activity Description:</strong> Human-readable activity type</item>
    /// <item><strong>Revision Context:</strong> Compact revision identification</item>
    /// <item><strong>Temporal Context:</strong> Localized timestamp for when it occurred</item>
    /// </list>
    /// 
    /// <para><strong>Professional Formatting:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Legal Standards:</strong> Appropriate for legal document audit trails</item>
    /// <item><strong>Professional Presentation:</strong> Suitable for client and court presentation</item>
    /// <item><strong>Comprehensive Information:</strong> All essential audit information in one display</item>
    /// <item><strong>Consistent Format:</strong> Standardized across all audit entries</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var auditEntry = new RevisionActivityUserMinimalDto
    /// {
    ///     User = new UserMinimalDto { Name = "Robert Brown" },
    ///     RevisionActivity = new RevisionActivityMinimalDto { Activity = "CREATED" },
    ///     Revision = new RevisionMinimalDto { RevisionNumber = 3 },
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Display in audit log
    /// Console.WriteLine(auditEntry.AuditDisplayText);
    /// // Output: "Robert Brown CREATED Rev. 3 on Monday, 15 January 2024 16:30:45"
    /// 
    /// // Use in audit reports
    /// auditReport.AddEntry(auditEntry.AuditDisplayText);
    /// </code>
    /// </example>
    public string AuditDisplayText =>
        $"{User?.DisplayText ?? "Unknown User"} {RevisionActivity?.Activity ?? "UNKNOWN"} " +
        $"{Revision?.CompactDisplayText ?? "Unknown Revision"} on {LocalCreatedAtDateString}";

    /// <summary>
    /// Gets a compact display text for space-limited UI scenarios.
    /// </summary>
    /// <remarks>
    /// This property provides a shortened version of the audit information suitable for
    /// table cells, tooltips, or other space-constrained user interface elements.
    /// 
    /// <para><strong>Compact Format:</strong></para>
    /// Follows the pattern: "[User] [Activity] [Rev. N]"
    /// <list type="bullet">
    /// <item><strong>Abbreviated User:</strong> User name without full attribution text</item>
    /// <item><strong>Activity Type:</strong> Direct activity name</item>
    /// <item><strong>Compact Revision:</strong> Abbreviated revision identification</item>
    /// <item><strong>Space Efficient:</strong> Minimal characters while maintaining clarity</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Use in table cells or tooltips
    /// tableCell.Text = auditEntry.CompactDisplayText;
    /// // Output: "Robert Brown CREATED Rev. 3"
    /// 
    /// // Tooltip display
    /// auditIcon.ToolTip = auditEntry.CompactDisplayText;
    /// </code>
    /// </example>
    public string CompactDisplayText =>
        $"{User?.Name ?? "Unknown"} {RevisionActivity?.Activity ?? "UNKNOWN"} " +
        $"{Revision?.CompactDisplayText ?? "Rev. ?"}";

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="RevisionActivityUserMinimalDto"/> for data integrity and consistency.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using centralized validation helpers to ensure audit trail integrity.
    /// This validation is critical for maintaining accurate and legally compliant audit trails.
    /// 
    /// <para><strong>Validation Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>GUID Validation:</strong> Ensures all required identifiers are valid non-empty GUIDs</item>
    /// <item><strong>Object Validation:</strong> Deep validation of all required navigation properties</item>
    /// <item><strong>Cross-Reference Validation:</strong> Ensures consistency between IDs and navigation properties</item>
    /// <item><strong>Temporal Validation:</strong> Validates activity timestamp for audit chronology</item>
    /// <item><strong>Business Rule Validation:</strong> Ensures compliance with audit trail business rules</item>
    /// </list>
    /// 
    /// <para><strong>Integration with Validation Helpers:</strong></para>
    /// Uses centralized validation helpers (RevisionValidationHelper, UserValidationHelper) to ensure
    /// consistency across the entire ADMS system and maintain validation standards alignment.
    /// </remarks>
    /// <example>
    /// <code>
    /// var auditEntry = new RevisionActivityUserMinimalDto 
    /// { 
    ///     RevisionId = Guid.Empty, // Invalid
    ///     RevisionActivityId = validActivityId,
    ///     UserId = validUserId,
    ///     CreatedAt = DateTime.MinValue // Invalid
    /// };
    /// 
    /// var context = new ValidationContext(auditEntry);
    /// var results = auditEntry.Validate(context);
    /// 
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"Validation Error: {result.ErrorMessage}");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate core GUID properties
        foreach (var result in ValidateRequiredGuids())
            yield return result;

        // Validate nested objects
        foreach (var result in ValidateRevision())
            yield return result;

        foreach (var result in ValidateRevisionActivity())
            yield return result;

        foreach (var result in ValidateUser())
            yield return result;

        // Validate cross-references
        foreach (var result in ValidateIdConsistency())
            yield return result;

        // Validate temporal properties
        foreach (var result in ValidateCreatedAt())
            yield return result;
    }

    /// <summary>
    /// Validates all required GUID properties using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for GUID properties.</returns>
    /// <remarks>
    /// Ensures all critical audit trail identifiers are valid non-empty GUIDs using
    /// the appropriate validation helpers for each entity type.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateRequiredGuids()
    {
        // Validate RevisionId
        if (RevisionId == Guid.Empty)
            yield return new ValidationResult("RevisionId must be a valid non-empty GUID.", [nameof(RevisionId)]);

        // Validate RevisionActivityId
        if (RevisionActivityId == Guid.Empty)
            yield return new ValidationResult("RevisionActivityId must be a valid non-empty GUID.", [nameof(RevisionActivityId)]);

        // Validate UserId using UserValidationHelper for consistency
        foreach (var result in UserValidationHelper.ValidateUserId(UserId, nameof(UserId)))
            yield return result;
    }

    /// <summary>
    /// Validates the <see cref="Revision"/> navigation property.
    /// </summary>
    /// <returns>A collection of validation results for the revision property.</returns>
    /// <remarks>
    /// Performs deep validation of the nested RevisionMinimalDto to ensure audit trail
    /// completeness and data integrity.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateRevision()
    {
        switch (Revision)
        {
            case null:
                yield return new ValidationResult("Revision details are required for complete audit trail.", [nameof(Revision)]);
                yield break;
            case IValidatableObject validatable:
                {
                    var context = new ValidationContext(Revision);
                    foreach (var result in validatable.Validate(context))
                    {
                        yield return new ValidationResult($"Revision validation failed: {result.ErrorMessage}",
                            result.MemberNames.Select(m => $"{nameof(Revision)}.{m}"));
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// Validates the <see cref="RevisionActivity"/> navigation property.
    /// </summary>
    /// <returns>A collection of validation results for the revision activity property.</returns>
    /// <remarks>
    /// Performs deep validation of the nested RevisionActivityMinimalDto to ensure proper
    /// activity classification and audit trail categorization.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateRevisionActivity()
    {
        switch (RevisionActivity)
        {
            case null:
                yield return new ValidationResult("RevisionActivity details are required for complete audit trail.", [nameof(RevisionActivity)]);
                yield break;
            case IValidatableObject validatable:
                {
                    var context = new ValidationContext(RevisionActivity);
                    foreach (var result in validatable.Validate(context))
                    {
                        yield return new ValidationResult($"RevisionActivity validation failed: {result.ErrorMessage}",
                            result.MemberNames.Select(m => $"{nameof(RevisionActivity)}.{m}"));
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// Validates the <see cref="User"/> navigation property.
    /// </summary>
    /// <returns>A collection of validation results for the user property.</returns>
    /// <remarks>
    /// Performs deep validation of the nested UserMinimalDto to ensure proper user
    /// attribution and professional accountability in the audit trail.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateUser()
    {
        switch (User)
        {
            case null:
                yield return new ValidationResult("User details are required for complete audit trail.", [nameof(User)]);
                yield break;
            case IValidatableObject validatable:
                {
                    var context = new ValidationContext(User);
                    foreach (var result in validatable.Validate(context))
                    {
                        yield return new ValidationResult($"User validation failed: {result.ErrorMessage}",
                            result.MemberNames.Select(m => $"{nameof(User)}.{m}"));
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// Validates consistency between foreign key IDs and their corresponding navigation properties.
    /// </summary>
    /// <returns>A collection of validation results for cross-reference consistency.</returns>
    /// <remarks>
    /// Ensures that when both foreign key IDs and navigation properties are provided,
    /// they reference the same entities. This is critical for audit trail data integrity.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateIdConsistency()
    {
        // Validate Revision ID consistency
        if (Revision?.Id.HasValue == true && Revision.Id.Value != RevisionId)
        {
            yield return new ValidationResult("Revision.Id does not match RevisionId - audit trail data integrity error.",
                [nameof(Revision), nameof(RevisionId)]);
        }

        // Validate RevisionActivity ID consistency
        if (RevisionActivity?.Id != RevisionActivityId)
        {
            yield return new ValidationResult("RevisionActivity.Id does not match RevisionActivityId - audit trail data integrity error.",
                [nameof(RevisionActivity), nameof(RevisionActivityId)]);
        }

        // Validate User ID consistency
        if (User?.Id != UserId)
        {
            yield return new ValidationResult("User.Id does not match UserId - audit trail data integrity error.",
                [nameof(User), nameof(UserId)]);
        }
    }

    /// <summary>
    /// Validates the <see cref="CreatedAt"/> timestamp using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the activity timestamp.</returns>
    /// <remarks>
    /// Uses RevisionValidationHelper to ensure the activity timestamp meets the same
    /// standards as other temporal data in the ADMS system for consistency.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateCreatedAt()
    {
        return RevisionValidationHelper.ValidateDate(CreatedAt, nameof(CreatedAt));
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="RevisionActivityUserMinimalDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The RevisionActivityUserMinimalDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate audit trail entries
    /// without requiring a ValidationContext. It performs the same comprehensive validation 
    /// as the instance Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>API Validation:</strong> Validating incoming audit DTOs in API controllers</item>
    /// <item><strong>Service Layer:</strong> Validation before processing audit trail operations</item>
    /// <item><strong>Unit Testing:</strong> Simplified validation testing without ValidationContext</item>
    /// <item><strong>Batch Processing:</strong> Validating collections of audit entries</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Validate an audit entry instance
    /// var auditEntry = new RevisionActivityUserMinimalDto 
    /// { 
    ///     RevisionId = revisionGuid,
    ///     RevisionActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// var results = RevisionActivityUserMinimalDto.ValidateModel(auditEntry);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Audit entry validation failed: {errorMessages}");
    /// }
    /// 
    /// // Batch validation of audit entries
    /// var validAuditEntries = auditEntries
    ///     .Where(entry => !RevisionActivityUserMinimalDto.ValidateModel(entry).Any())
    ///     .ToList();
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] RevisionActivityUserMinimalDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("RevisionActivityUserMinimalDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a RevisionActivityUserMinimalDto from an ADMS.API.Entities.RevisionActivityUser entity with validation.
    /// </summary>
    /// <param name="revisionActivityUser">The RevisionActivityUser entity to convert. Cannot be null.</param>
    /// <returns>A valid RevisionActivityUserMinimalDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when revisionActivityUser is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create audit trail DTOs from
    /// ADMS.API.Entities.RevisionActivityUser entities with automatic validation. It ensures the resulting
    /// DTO is valid before returning it, maintaining audit trail data integrity.
    /// 
    /// <para><strong>Entity Conversion:</strong></para>
    /// <list type="bullet">
    /// <item>Maps all essential audit trail properties from the entity</item>
    /// <item>Creates minimal DTOs for navigation properties</item>
    /// <item>Validates the complete audit entry for consistency</item>
    /// <item>Ensures proper audit trail representation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity with navigation properties loaded
    /// var entity = await context.RevisionActivityUsers
    ///     .Include(rau => rau.Revision)
    ///     .Include(rau => rau.RevisionActivity)
    ///     .Include(rau => rau.User)
    ///     .FirstAsync(rau => rau.RevisionId == targetRevisionId);
    /// 
    /// var auditDto = RevisionActivityUserMinimalDto.FromEntity(entity);
    /// // Returns validated audit trail DTO
    /// </code>
    /// </example>
    public static RevisionActivityUserMinimalDto FromEntity([NotNull] Entities.RevisionActivityUser revisionActivityUser)
    {
        ArgumentNullException.ThrowIfNull(revisionActivityUser, nameof(revisionActivityUser));

        var dto = new RevisionActivityUserMinimalDto
        {
            RevisionId = revisionActivityUser.RevisionId,
            Revision = RevisionMinimalDto.FromEntity(revisionActivityUser.Revision, includeDocumentId: false),
            RevisionActivityId = revisionActivityUser.RevisionActivityId,
            RevisionActivity = new RevisionActivityMinimalDto
            {
                Id = revisionActivityUser.RevisionActivity.Id,
                Activity = revisionActivityUser.RevisionActivity.Activity
            },
            UserId = revisionActivityUser.UserId,
            User = UserMinimalDto.FromEntity(revisionActivityUser.User),
            CreatedAt = revisionActivityUser.CreatedAt
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid RevisionActivityUserMinimalDto from entity: {errorMessages}");

    }

    #endregion Static Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified RevisionActivityUserMinimalDto is equal to the current RevisionActivityUserMinimalDto.
    /// </summary>
    /// <param name="other">The RevisionActivityUserMinimalDto to compare with the current RevisionActivityUserMinimalDto.</param>
    /// <returns>true if the specified RevisionActivityUserMinimalDto is equal to the current RevisionActivityUserMinimalDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all core properties that uniquely identify an audit trail entry.
    /// This follows the same pattern as the entity composite key for consistency.
    /// 
    /// <para><strong>Equality Criteria:</strong></para>
    /// <list type="bullet">
    /// <item>RevisionId must be equal</item>
    /// <item>RevisionActivityId must be equal</item>
    /// <item>UserId must be equal</item>
    /// <item>CreatedAt must be equal</item>
    /// </list>
    /// </remarks>
    public virtual bool Equals(RevisionActivityUserMinimalDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return RevisionId.Equals(other.RevisionId) &&
               RevisionActivityId.Equals(other.RevisionActivityId) &&
               UserId.Equals(other.UserId) &&
               CreatedAt.Equals(other.CreatedAt);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current RevisionActivityUserMinimalDto.</returns>
    /// <remarks>
    /// The hash code is based on all key properties to ensure consistent hashing behavior
    /// that aligns with the equality implementation and composite key structure.
    /// </remarks>
    public override int GetHashCode()
    {
        return HashCode.Combine(RevisionId, RevisionActivityId, UserId, CreatedAt);
    }

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the RevisionActivityUserMinimalDto.
    /// </summary>
    /// <returns>A string that represents the current RevisionActivityUserMinimalDto.</returns>
    /// <remarks>
    /// The string representation provides key audit trail information in a format suitable for
    /// logging, debugging, and audit trail documentation. It includes all essential identifiers
    /// and the activity timestamp for comprehensive audit representation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var auditEntry = new RevisionActivityUserMinimalDto 
    /// { 
    ///     RevisionId = Guid.Parse("12345678-1234-5678-9012-123456789012"),
    ///     RevisionActivityId = Guid.Parse("87654321-4321-8765-2109-876543210987"),
    ///     UserId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
    ///     CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)
    /// };
    /// 
    /// Console.WriteLine(auditEntry);
    /// // Output: "Audit Entry: Revision 12345678-1234-5678-9012-123456789012, Activity 87654321-4321-8765-2109-876543210987, User 11111111-2222-3333-4444-555555555555, Time 2024-01-15 10:30:00 UTC"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Audit Entry: Revision {RevisionId}, Activity {RevisionActivityId}, User {UserId}, Time {CreatedAt:yyyy-MM-dd HH:mm:ss} UTC";

    #endregion String Representation
}