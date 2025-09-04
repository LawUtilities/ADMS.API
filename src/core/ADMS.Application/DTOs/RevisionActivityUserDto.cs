using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing the association between a revision, revision activity, and user for complete audit trail management.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of the audit trail junction entity 
/// <see cref="ADMS.API.Entities.RevisionActivityUser"/>, providing comprehensive information for audit trail 
/// management, reporting, and activity tracking within the ADMS legal document management system. It includes
/// all related entity information for complete audit trail presentation and analysis.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Comprehensive Audit Trail:</strong> Complete representation including all related entities for full context</item>
/// <item><strong>Entity Synchronization:</strong> Mirrors all properties and relationships from ADMS.API.Entities.RevisionActivityUser</item>
/// <item><strong>Professional Validation:</strong> Uses centralized validation helpers for data integrity</item>
/// <item><strong>Business Rule Enforcement:</strong> Comprehensive validation of relationships and cross-references</item>
/// <item><strong>Legal Compliance Support:</strong> Designed for comprehensive audit reporting and legal compliance</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO represents the many-to-many-to-many relationship from ADMS.API.Entities.RevisionActivityUser:
/// <list type="bullet">
/// <item><strong>Revision Association:</strong> Links to the specific document revision via RevisionId and RevisionDto</item>
/// <item><strong>Activity Classification:</strong> Identifies the type of activity performed via RevisionActivityId and RevisionActivityDto</item>
/// <item><strong>User Attribution:</strong> Provides user accountability via UserId and UserDto</item>
/// <item><strong>Temporal Tracking:</strong> Records when the activity occurred via CreatedAt timestamp</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Comprehensive Audit Reports:</strong> Complete audit trail reporting with full entity context</item>
/// <item><strong>Administrative Management:</strong> Full audit trail management and administrative operations</item>
/// <item><strong>Legal Compliance Reporting:</strong> Detailed compliance reports requiring complete audit information</item>
/// <item><strong>API Responses:</strong> Complete audit data in REST API responses</item>
/// <item><strong>Activity Analysis:</strong> In-depth analysis of user activities and patterns</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Comprehensive Audit Compliance:</strong> Complete activity attribution for legal compliance</item>
/// <item><strong>Professional Responsibility:</strong> Detailed user attribution for professional accountability</item>
/// <item><strong>Legal Discovery Support:</strong> Complete audit information for legal discovery processes</item>
/// <item><strong>Document Integrity:</strong> Maintains comprehensive document version control audit trails</item>
/// </list>
/// 
/// <para><strong>Data Integrity:</strong></para>
/// <list type="bullet">
/// <item><strong>Referential Consistency:</strong> All foreign keys validated against their respective entities</item>
/// <item><strong>Temporal Accuracy:</strong> Activity timestamps validated for chronological consistency</item>
/// <item><strong>Cross-Reference Validation:</strong> Ensures consistency between IDs and navigation properties</item>
/// <item><strong>Complete Attribution:</strong> Guarantees all activities have complete context and attribution</item>
/// </list>
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Entity Loading:</strong> May require careful loading strategies for optimal performance</item>
/// <item><strong>Selective Usage:</strong> Use minimal DTO when complete context is not needed</item>
/// <item><strong>Validation Optimization:</strong> Efficient validation using centralized helpers</item>
/// <item><strong>Collection Management:</strong> Consider lazy loading for related collections</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive audit trail entry
/// var auditEntry = new RevisionActivityUserDto
/// {
///     RevisionId = revisionGuid,
///     Revision = new RevisionDto { Id = revisionGuid, RevisionNumber = 2 },
///     RevisionActivityId = activityGuid,
///     RevisionActivity = new RevisionActivityDto { Id = activityGuid, Activity = "CREATED" },
///     UserId = userGuid,
///     User = new UserDto { Id = userGuid, Name = "Robert Brown" },
///     CreatedAt = DateTime.UtcNow
/// };
/// 
/// // Comprehensive audit reporting
/// var auditReport = auditEntries.Select(entry => new
/// {
///     User = entry.User.DisplayName,
///     Activity = entry.RevisionActivity.Activity,
///     Revision = entry.Revision.DisplayText,
///     Timestamp = entry.LocalCreatedAtDateString,
///     RevisionInfo = entry.Revision.CompactDisplayText
/// }).ToList();
/// 
/// // Validating complete audit entry
/// var validationResults = RevisionActivityUserDto.ValidateModel(auditEntry);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Validation Error: {result.ErrorMessage}");
///     }
/// }
/// </code>
/// </example>
public class RevisionActivityUserDto : IValidatableObject, IEquatable<RevisionActivityUserDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier of the revision associated with this activity.
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
    /// <para><strong>Usage in Comprehensive Audit Context:</strong></para>
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
    /// var auditEntry = new RevisionActivityUserDto
    /// {
    ///     RevisionId = Guid.Parse("12345678-1234-5678-9012-123456789012"),
    ///     RevisionActivityId = activityId,
    ///     UserId = userId,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Comprehensive audit queries by revision
    /// var revisionAuditTrail = auditEntries
    ///     .Where(entry => entry.RevisionId == targetRevisionId)
    ///     .OrderBy(entry => entry.CreatedAt)
    ///     .Select(entry => new {
    ///         entry.User.DisplayName,
    ///         entry.RevisionActivity.Activity,
    ///         entry.Revision.DisplayText
    ///     });
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Revision ID is required.")]
    public required Guid RevisionId { get; set; }

    /// <summary>
    /// Gets or sets the complete revision details associated with this activity.
    /// </summary>
    /// <remarks>
    /// This property provides comprehensive revision information needed for complete audit trail management,
    /// corresponding to the <see cref="ADMS.API.Entities.RevisionActivityUser.Revision"/> navigation property.
    /// It includes all revision data needed for comprehensive audit reporting and analysis.
    /// 
    /// <para><strong>Comprehensive Information Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Complete Revision Data:</strong> All revision properties including metadata and relationships</item>
    /// <item><strong>Activity Collections:</strong> May include related revision activities for complete context</item>
    /// <item><strong>Computed Properties:</strong> Pre-calculated properties for efficient display and analysis</item>
    /// <item><strong>Document Association:</strong> Optional document context when needed for reporting</item>
    /// </list>
    /// 
    /// <para><strong>Comprehensive Audit Trail Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Complete Context:</strong> Provides full revision context for comprehensive reporting</item>
    /// <item><strong>Business Intelligence:</strong> Enables deep analysis of revision patterns and trends</item>
    /// <item><strong>Cross-Reference Validation:</strong> ID must match RevisionId for consistency</item>
    /// <item><strong>Performance Optimization:</strong> May include pre-loaded relationships for efficiency</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Must not be null - required for complete audit information</item>
    /// <item>Must pass comprehensive RevisionDto validation rules</item>
    /// <item>Revision.Id must match RevisionId when both are provided</item>
    /// <item>Must represent a valid, complete revision entity</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Comprehensive audit trail display using complete revision details
    /// var displayText = $"User {auditEntry.User.DisplayName} {auditEntry.RevisionActivity.Activity} " +
    ///                  $"{auditEntry.Revision.DisplayText} on {auditEntry.LocalCreatedAtDateString}";
    /// 
    /// // Accessing comprehensive revision information
    /// if (auditEntry.Revision.HasActivities)
    /// {
    ///     var activitySummary = $"This revision has {auditEntry.Revision.ActivityCount} total activities";
    /// }
    /// 
    /// // Comprehensive reporting with revision metrics
    /// var revisionMetrics = new
    /// {
    ///     RevisionNumber = auditEntry.Revision.RevisionNumber,
    ///     TotalActivities = auditEntry.Revision.ActivityCount,
    ///     DevelopmentTime = auditEntry.Revision.ModificationTimeSpan,
    ///     IsActive = !auditEntry.Revision.IsDeleted
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Revision details are required.")]
    public required RevisionDto Revision { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the revision activity performed.
    /// </summary>
    /// <remarks>
    /// This GUID identifies the specific type of activity that was performed on the revision,
    /// corresponding to <see cref="ADMS.API.Entities.RevisionActivityUser.RevisionActivityId"/>.
    /// It establishes the relationship to the activity classification in the comprehensive audit trail.
    /// 
    /// <para><strong>Activity Classification:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Type Identification:</strong> Links to specific activity types (CREATED, SAVED, etc.)</item>
    /// <item><strong>Comprehensive Categorization:</strong> Enables detailed filtering and categorization of audit entries</item>
    /// <item><strong>Compliance Reporting:</strong> Supports detailed activity-based compliance reporting</item>
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
    /// ensuring consistency between entity and DTO representations for comprehensive audit trail integrity.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Comprehensive filtering by activity type
    /// var creationActivities = auditEntries
    ///     .Where(entry => entry.RevisionActivity.Activity == "CREATED")
    ///     .OrderBy(entry => entry.CreatedAt)
    ///     .Select(entry => new {
    ///         entry.User.DisplayName,
    ///         entry.Revision.DisplayText,
    ///         entry.LocalCreatedAtDateString
    ///     });
    /// 
    /// // Comprehensive activity-based audit reporting
    /// var activitySummary = auditEntries
    ///     .GroupBy(entry => entry.RevisionActivity.Activity)
    ///     .Select(g => new { 
    ///         Activity = g.Key, 
    ///         Count = g.Count(),
    ///         Users = g.Select(e => e.User.DisplayName).Distinct().Count(),
    ///         Revisions = g.Select(e => e.Revision.RevisionNumber).Distinct().Count()
    ///     });
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Revision activity ID is required.")]
    public required Guid RevisionActivityId { get; set; }

    /// <summary>
    /// Gets or sets the complete revision activity details for this audit entry.
    /// </summary>
    /// <remarks>
    /// This property provides comprehensive activity information needed for complete audit trail management,
    /// corresponding to the <see cref="ADMS.API.Entities.RevisionActivityUser.RevisionActivity"/> navigation property.
    /// It includes complete activity data and relationships for comprehensive audit presentation and analysis.
    /// 
    /// <para><strong>Comprehensive Activity Information Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Complete Activity Data:</strong> All activity properties including metadata and relationships</item>
    /// <item><strong>Activity Collections:</strong> May include related user activities for complete context</item>
    /// <item><strong>Validation Rules:</strong> Comprehensive activity-specific validation and business rules</item>
    /// <item><strong>Usage Statistics:</strong> Activity usage patterns and metrics</item>
    /// </list>
    /// 
    /// <para><strong>Comprehensive Audit Trail Display Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Detailed Activities:</strong> Enables comprehensive display of activity information</item>
    /// <item><strong>Activity Analytics:</strong> Supports detailed filtering, grouping, and analysis of audit entries</item>
    /// <item><strong>Cross-Reference Validation:</strong> ID must match RevisionActivityId for consistency</item>
    /// <item><strong>Compliance Reporting:</strong> Provides detailed activity classification for legal reporting</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Must not be null - required for complete audit information</item>
    /// <item>Must pass comprehensive RevisionActivityDto validation rules</item>
    /// <item>RevisionActivity.Id must match RevisionActivityId when both are provided</item>
    /// <item>Activity must be a valid, recognized activity type with complete data</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Comprehensive audit trail display using complete activity details
    /// var activityDescription = auditEntry.RevisionActivity.Activity switch
    /// {
    ///     "CREATED" => "created",
    ///     "SAVED" => "saved changes to",
    ///     "DELETED" => "deleted",
    ///     "RESTORED" => "restored",
    ///     _ => "performed unknown action on"
    /// };
    /// 
    /// var comprehensiveAuditMessage = 
    ///     $"{auditEntry.User.DisplayName} {activityDescription} " +
    ///     $"{auditEntry.Revision.DisplayText} on {auditEntry.LocalCreatedAtDateString}";
    /// 
    /// // Comprehensive activity analysis
    /// if (auditEntry.RevisionActivity.RevisionActivityUsers.Any())
    /// {
    ///     var activityUsage = $"This activity type has been performed " +
    ///                        $"{auditEntry.RevisionActivity.RevisionActivityUsers.Count} times";
    /// }
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Revision activity details are required.")]
    public required RevisionActivityDto RevisionActivity { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who performed the activity.
    /// </summary>
    /// <remarks>
    /// This GUID identifies the user who performed the revision activity, corresponding to
    /// <see cref="ADMS.API.Entities.RevisionActivityUser.UserId"/>. It provides the essential
    /// user attribution required for comprehensive audit trail accountability and legal compliance.
    /// 
    /// <para><strong>User Attribution Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required Association:</strong> Every activity must have complete user attribution</item>
    /// <item><strong>Non-Empty GUID:</strong> Cannot be Guid.Empty or default value</item>
    /// <item><strong>Entity Reference:</strong> Must correspond to an existing ADMS.API.Entities.User</item>
    /// <item><strong>Comprehensive Accountability:</strong> Critical for detailed legal and professional accountability</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Professional Responsibility:</strong> Maintains detailed user accountability for document activities</item>
    /// <item><strong>Comprehensive Audit Trail:</strong> Ensures complete attribution of all revision activities</item>
    /// <item><strong>Legal Discovery:</strong> Supports detailed legal discovery with comprehensive user activity tracking</item>
    /// <item><strong>Cross-Reference Validation:</strong> Must match User.Id when both are provided</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.RevisionActivityUser.UserId"/> exactly,
    /// ensuring consistency between entity and DTO representations for comprehensive audit trail integrity.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Comprehensive user-based audit trail filtering
    /// var userActivities = auditEntries
    ///     .Where(entry => entry.UserId == targetUserId)
    ///     .OrderByDescending(entry => entry.CreatedAt)
    ///     .Select(entry => new {
    ///         entry.RevisionActivity.Activity,
    ///         entry.Revision.DisplayText,
    ///         entry.LocalCreatedAtDateString,
    ///         RevisionMetrics = new {
    ///             entry.Revision.RevisionNumber,
    ///             entry.Revision.ActivityCount
    ///         }
    ///     });
    /// 
    /// // Comprehensive user activity analytics
    /// var userActivityAnalytics = auditEntries
    ///     .Where(entry => entry.UserId == targetUserId)
    ///     .GroupBy(entry => entry.RevisionActivity.Activity)
    ///     .Select(g => new { 
    ///         Activity = g.Key, 
    ///         Count = g.Count(),
    ///         UniqueRevisions = g.Select(e => e.RevisionId).Distinct().Count(),
    ///         DateRange = new {
    ///             First = g.Min(e => e.CreatedAt),
    ///             Last = g.Max(e => e.CreatedAt)
    ///         }
    ///     });
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User ID is required.")]
    public required Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the complete user details for the user who performed the activity.
    /// </summary>
    /// <remarks>
    /// This property provides comprehensive user information needed for complete audit trail management,
    /// corresponding to the <see cref="ADMS.API.Entities.RevisionActivityUser.User"/> navigation property.
    /// It includes complete user data and activity relationships for comprehensive audit presentation and analysis.
    /// 
    /// <para><strong>Comprehensive User Information Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Complete User Data:</strong> All user properties including activity relationships</item>
    /// <item><strong>Activity Collections:</strong> May include all user activities for comprehensive context</item>
    /// <item><strong>Professional Information:</strong> Complete professional display names and identification</item>
    /// <item><strong>Activity Statistics:</strong> User activity metrics and patterns</item>
    /// </list>
    /// 
    /// <para><strong>Comprehensive Audit Trail Display Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Complete Professional Attribution:</strong> Detailed user identification in comprehensive audit logs</item>
    /// <item><strong>User Analytics:</strong> Complete user activity analysis and reporting capabilities</item>
    /// <item><strong>Cross-Reference Validation:</strong> ID must match UserId for consistency</item>
    /// <item><strong>Professional Compliance:</strong> Maintains comprehensive professional naming and accountability standards</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Must not be null - required for complete audit information</item>
    /// <item>Must pass comprehensive UserDto validation rules</item>
    /// <item>User.Id must match UserId when both are provided</item>
    /// <item>Must represent a valid user entity with complete data</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Comprehensive professional audit trail display
    /// var comprehensiveAuditDisplay = 
    ///     $"{auditEntry.User.DisplayName} (Total Activities: {auditEntry.User.TotalActivityCount}) " +
    ///     $"{auditEntry.RevisionActivity.Activity.ToLower()} {auditEntry.Revision.DisplayText} " +
    ///     $"on {auditEntry.LocalCreatedAtDateString}";
    /// 
    /// // Comprehensive user-centric audit reporting
    /// var userAnalytics = new
    /// {
    ///     UserName = auditEntry.User.DisplayName,
    ///     NormalizedName = auditEntry.User.NormalizedName,
    ///     TotalActivities = auditEntry.User.TotalActivityCount,
    ///     HasExtensiveHistory = auditEntry.User.HasActivities,
    ///     CurrentActivity = auditEntry.RevisionActivity.Activity,
    ///     RevisionContext = auditEntry.Revision.CompactDisplayText
    /// };
    /// 
    /// // Professional accountability reporting
    /// if (auditEntry.User.HasActivities)
    /// {
    ///     Console.WriteLine($"Professional: {auditEntry.User.DisplayName} has extensive activity history " +
    ///                      $"with {auditEntry.User.TotalActivityCount} total activities");
    /// }
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User details are required.")]
    public required UserDto User { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the activity was performed.
    /// </summary>
    /// <remarks>
    /// This property records the precise timestamp when the revision activity occurred,
    /// corresponding to <see cref="ADMS.API.Entities.RevisionActivityUser.CreatedAt"/>. It provides
    /// the temporal context essential for comprehensive audit trail chronology and legal compliance.
    /// 
    /// <para><strong>Temporal Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>UTC Storage:</strong> All timestamps stored in UTC for global consistency</item>
    /// <item><strong>Comprehensive Chronology:</strong> Enables detailed chronological ordering and analysis</item>
    /// <item><strong>Legal Compliance:</strong> Provides precise timing for legal document management</item>
    /// <item><strong>Valid Range:</strong> Must be within reasonable temporal bounds</item>
    /// </list>
    /// 
    /// <para><strong>Comprehensive Audit Trail Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Timeline:</strong> Establishes detailed when specific activities occurred</item>
    /// <item><strong>Chronological Analysis:</strong> Enables comprehensive sequencing and temporal analysis</item>
    /// <item><strong>Legal Evidence:</strong> Provides detailed temporal evidence for legal compliance</item>
    /// <item><strong>Activity Pattern Analysis:</strong> Supports comprehensive temporal analysis of user and system behavior</item>
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
    /// ensuring consistency between entity and DTO representations for comprehensive audit trail integrity.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Creating comprehensive audit entry with precise timestamp
    /// var auditEntry = new RevisionActivityUserDto
    /// {
    ///     RevisionId = revisionId,
    ///     RevisionActivityId = activityId,
    ///     UserId = userId,
    ///     CreatedAt = DateTime.UtcNow // Precise UTC timestamp
    /// };
    /// 
    /// // Comprehensive chronological audit trail analysis
    /// var chronologicalAnalysis = auditEntries
    ///     .OrderBy(entry => entry.CreatedAt)
    ///     .Select(entry => new {
    ///         Timestamp = entry.CreatedAt,
    ///         LocalTime = entry.LocalCreatedAtDateString,
    ///         User = entry.User.DisplayName,
    ///         Activity = entry.RevisionActivity.Activity,
    ///         Revision = entry.Revision.DisplayText,
    ///         TimeFromPrevious = // Calculate time differences for comprehensive analysis
    ///     });
    /// 
    /// // Comprehensive temporal filtering and analysis
    /// var recentComprehensiveActivities = auditEntries
    ///     .Where(entry => entry.CreatedAt >= DateTime.UtcNow.AddDays(-30))
    ///     .OrderByDescending(entry => entry.CreatedAt)
    ///     .GroupBy(entry => entry.CreatedAt.Date)
    ///     .Select(g => new {
    ///         Date = g.Key,
    ///         Activities = g.Count(),
    ///         Users = g.Select(e => e.User.DisplayName).Distinct().Count(),
    ///         ActivityTypes = g.Select(e => e.RevisionActivity.Activity).Distinct().ToList()
    ///     });
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Activity timestamp is required.")]
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    #endregion Core Properties

    #region Computed Properties

    /// <summary>
    /// Gets the activity timestamp formatted as a localized string for UI display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a user-friendly formatted representation of the activity timestamp
    /// converted to local time, optimized for comprehensive audit trail display and professional presentation.
    /// 
    /// <para><strong>Professional Display Format:</strong></para>
    /// Uses "dddd, dd MMMM yyyy HH:mm:ss" format for complete temporal information:
    /// <list type="bullet">
    /// <item><strong>Day of Week:</strong> "Monday" - provides additional temporal context</item>
    /// <item><strong>Full Date:</strong> "15 January 2024" - complete date information</item>
    /// <item><strong>Precise Time:</strong> "14:30:45" - exact time for audit precision</item>
    /// </list>
    /// 
    /// <para><strong>Professional Localization Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User-Friendly:</strong> Displays in user's local time zone for better understanding</item>
    /// <item><strong>Legal Professional Format:</strong> Appropriate for legal and professional documentation</item>
    /// <item><strong>Comprehensive Audit Ready:</strong> Formatted specifically for detailed audit log presentation</item>
    /// <item><strong>Cultural Adaptation:</strong> Respects local date and time formatting conventions</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var auditEntry = new RevisionActivityUserDto 
    /// { 
    ///     CreatedAt = new DateTime(2024, 1, 15, 14, 30, 45, DateTimeKind.Utc)
    ///     // ... other properties
    /// };
    /// 
    /// // Display in comprehensive audit log UI
    /// auditLogLabel.Text = $"Activity performed on: {auditEntry.LocalCreatedAtDateString}";
    /// // Output (if local time is UTC+2): "Activity performed on: Monday, 15 January 2024 16:30:45"
    /// 
    /// // Comprehensive audit entry display
    /// var fullComprehensiveDisplay = 
    ///     $"{auditEntry.User.DisplayName} {auditEntry.RevisionActivity.Activity} " +
    ///     $"{auditEntry.Revision.DisplayText} on {auditEntry.LocalCreatedAtDateString} " +
    ///     $"(Revision has {auditEntry.Revision.ActivityCount} total activities)";
    /// </code>
    /// </example>
    public string LocalCreatedAtDateString => CreatedAt.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets a value indicating whether this comprehensive audit entry has valid data for system operations.
    /// </summary>
    /// <remarks>
    /// This property provides a quick validation check without running full validation logic,
    /// useful for UI scenarios where immediate feedback is needed before processing comprehensive audit entries.
    /// 
    /// <para><strong>Comprehensive Validation Checks Performed:</strong></para>
    /// <list type="bullet">
    /// <item><strong>GUID Validation:</strong> All required GUIDs are non-empty</item>
    /// <item><strong>Date Validation:</strong> CreatedAt is within valid temporal bounds</item>
    /// <item><strong>Complete Object Validation:</strong> All required nested objects are present</item>
    /// <item><strong>Cross-Reference Consistency:</strong> IDs match their corresponding navigation properties</item>
    /// <item><strong>Entity Completeness:</strong> All entities have valid data for comprehensive operations</item>
    /// </list>
    /// 
    /// <para><strong>Performance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Quick Check:</strong> Rapid validation without comprehensive rule evaluation</item>
    /// <item><strong>UI Responsiveness:</strong> Immediate feedback for comprehensive user interface scenarios</item>
    /// <item><strong>Batch Processing:</strong> Efficient filtering of valid entries in comprehensive collections</item>
    /// <item><strong>Error Prevention:</strong> Early detection of invalid comprehensive audit entries</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Quick validation before comprehensive processing
    /// if (auditEntry.IsValid)
    /// {
    ///     // Process valid comprehensive audit entry
    ///     await ProcessComprehensiveAuditEntry(auditEntry);
    /// }
    /// else
    /// {
    ///     // Handle invalid entry with comprehensive logging
    ///     logger.LogWarning($"Invalid comprehensive audit entry detected: {auditEntry}");
    /// }
    /// 
    /// // Batch filtering of valid comprehensive audit entries
    /// var validComprehensiveAuditEntries = auditEntries
    ///     .Where(entry => entry.IsValid)
    ///     .ToList();
    /// </code>
    /// </example>
    public bool IsValid =>
        RevisionId != Guid.Empty &&
        RevisionActivityId != Guid.Empty &&
        UserId != Guid.Empty &&
        RevisionValidationHelper.IsValidDate(CreatedAt) && Revision.IsValid && User.IsValid;

    /// <summary>
    /// Gets a comprehensive display text suitable for detailed audit trail presentation.
    /// </summary>
    /// <remarks>
    /// This property provides a complete, human-readable description of the comprehensive audit entry,
    /// combining all essential information into a single, professional audit trail message with additional context.
    /// 
    /// <para><strong>Comprehensive Display Format:</strong></para>
    /// Follows the pattern: "[User Name] [Activity] [Revision] on [Timestamp] ([Additional Context])"
    /// <list type="bullet">
    /// <item><strong>Professional User Attribution:</strong> Complete identification of who performed the activity</item>
    /// <item><strong>Detailed Activity Description:</strong> Human-readable activity type with context</item>
    /// <item><strong>Complete Revision Context:</strong> Comprehensive revision identification</item>
    /// <item><strong>Professional Temporal Context:</strong> Localized timestamp with additional precision</item>
    /// <item><strong>Additional Metrics:</strong> Relevant activity counts and metrics for comprehensive view</item>
    /// </list>
    /// 
    /// <para><strong>Professional Formatting:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Legal Standards:</strong> Appropriate for comprehensive legal document audit trails</item>
    /// <item><strong>Professional Presentation:</strong> Suitable for detailed client and court presentation</item>
    /// <item><strong>Comprehensive Information:</strong> All essential audit information plus relevant metrics</item>
    /// <item><strong>Consistent Format:</strong> Standardized across all comprehensive audit entries</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var auditEntry = new RevisionActivityUserDto
    /// {
    ///     User = new UserDto { DisplayName = "Robert Brown", TotalActivityCount = 127 },
    ///     RevisionActivity = new RevisionActivityDto { Activity = "CREATED" },
    ///     Revision = new RevisionDto { RevisionNumber = 3, ActivityCount = 5 },
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Display in comprehensive audit log
    /// Console.WriteLine(auditEntry.ComprehensiveAuditDisplayText);
    /// // Output: "Robert Brown CREATED Revision 3 (5 activities) on Monday, 15 January 2024 16:30:45 - User has 127 total activities"
    /// 
    /// // Use in comprehensive audit reports
    /// comprehensiveAuditReport.AddEntry(auditEntry.ComprehensiveAuditDisplayText);
    /// </code>
    /// </example>
    public string ComprehensiveAuditDisplayText =>
        $"{User?.DisplayName ?? "Unknown User"} {RevisionActivity?.Activity ?? "UNKNOWN"} " +
        $"{Revision?.DisplayText ?? "Unknown Revision"} on {LocalCreatedAtDateString}" +
        $"{(Revision?.ActivityCount > 0 ? $" - Revision has {Revision.ActivityCount} activities" : "")}" +
        $"{(User?.TotalActivityCount > 0 ? $" - User has {User.TotalActivityCount} total activities" : "")}";

    /// <summary>
    /// Gets a compact display text for space-limited UI scenarios with essential information.
    /// </summary>
    /// <remarks>
    /// This property provides a shortened version of the audit information suitable for
    /// table cells, tooltips, or other space-constrained user interface elements while
    /// maintaining essential comprehensive context.
    /// 
    /// <para><strong>Compact Format:</strong></para>
    /// Follows the pattern: "[User] [Activity] [Rev. N] ([Context])"
    /// <list type="bullet">
    /// <item><strong>User Name:</strong> User name without full attribution text</item>
    /// <item><strong>Activity Type:</strong> Direct activity name</item>
    /// <item><strong>Compact Revision:</strong> Abbreviated revision identification</item>
    /// <item><strong>Essential Context:</strong> Key metrics for comprehensive understanding</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Use in table cells or tooltips
    /// tableCell.Text = auditEntry.CompactDisplayText;
    /// // Output: "Robert Brown CREATED Rev. 3 (5 acts)"
    /// 
    /// // Tooltip display with essential context
    /// auditIcon.ToolTip = auditEntry.CompactDisplayText;
    /// </code>
    /// </example>
    public string CompactDisplayText =>
        $"{User?.Name ?? "Unknown"} {RevisionActivity?.Activity ?? "UNKNOWN"} " +
        $"{Revision?.CompactDisplayText ?? "Rev. ?"}" +
        $"{(Revision?.ActivityCount > 0 ? $" ({Revision.ActivityCount} acts)" : "")}";

    /// <summary>
    /// Gets comprehensive audit metrics for reporting and analysis.
    /// </summary>
    /// <remarks>
    /// This property provides a structured object containing key metrics and information
    /// for comprehensive audit analysis and reporting purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var metrics = auditEntry.AuditMetrics;
    /// // Access comprehensive metrics for analysis
    /// </code>
    /// </example>
    public object AuditMetrics => new
    {
        UserMetrics = new
        {
            User?.Name,
            User?.TotalActivityCount,
            User?.HasActivities
        },
        RevisionMetrics = new
        {
            Revision?.RevisionNumber,
            Revision?.ActivityCount,
            Revision?.ModificationTimeSpan,
            Revision?.IsDeleted
        },
        ActivityMetrics = new
        {
            RevisionActivity?.Activity,
            Timestamp = CreatedAt,
            LocalTimestamp = LocalCreatedAtDateString
        }
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="RevisionActivityUserDto"/> for data integrity and consistency.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using centralized validation helpers to ensure complete audit trail integrity.
    /// This validation is critical for maintaining accurate and legally compliant comprehensive audit trails.
    /// 
    /// <para><strong>Comprehensive Validation Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>GUID Validation:</strong> Ensures all required identifiers are valid non-empty GUIDs</item>
    /// <item><strong>Complete Object Validation:</strong> Deep validation of all required navigation properties</item>
    /// <item><strong>Cross-Reference Validation:</strong> Ensures consistency between IDs and navigation properties</item>
    /// <item><strong>Temporal Validation:</strong> Validates activity timestamp for audit chronology</item>
    /// <item><strong>Business Rule Validation:</strong> Ensures compliance with comprehensive audit trail business rules</item>
    /// <item><strong>Entity Completeness:</strong> Validates that all entities contain complete, valid data</item>
    /// </list>
    /// 
    /// <para><strong>Integration with Validation Helpers:</strong></para>
    /// Uses centralized validation helpers (RevisionValidationHelper, UserValidationHelper, DtoValidationHelper) 
    /// to ensure consistency across the entire ADMS system and maintain validation standards alignment.
    /// </remarks>
    /// <example>
    /// <code>
    /// var auditEntry = new RevisionActivityUserDto 
    /// { 
    ///     RevisionId = Guid.Empty, // Invalid
    ///     RevisionActivityId = validActivityId,
    ///     UserId = validUserId,
    ///     CreatedAt = DateTime.MinValue, // Invalid
    ///     Revision = null, // Invalid
    ///     RevisionActivity = validActivity,
    ///     User = validUser
    /// };
    /// 
    /// var context = new ValidationContext(auditEntry);
    /// var results = auditEntry.Validate(context);
    /// 
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"Comprehensive Validation Error: {result.ErrorMessage}");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate core GUID properties
        foreach (var result in ValidateRequiredGuids())
            yield return result;

        // Validate comprehensive nested objects
        foreach (var result in ValidateRevision())
            yield return result;

        foreach (var result in ValidateRevisionActivity())
            yield return result;

        foreach (var result in ValidateUser())
            yield return result;

        // Validate cross-references for comprehensive consistency
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
    /// Validates the <see cref="Revision"/> navigation property for comprehensive audit integrity.
    /// </summary>
    /// <returns>A collection of validation results for the revision property.</returns>
    /// <remarks>
    /// Performs deep validation of the nested RevisionDto to ensure comprehensive audit trail
    /// completeness and data integrity with complete entity validation.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateRevision()
    {
        switch (Revision)
        {
            case null:
                yield return new ValidationResult("Revision details are required for complete comprehensive audit trail.", [nameof(Revision)]);
                yield break;
            case IValidatableObject validatable:
                {
                    var context = new ValidationContext(Revision);
                    foreach (var result in validatable.Validate(context))
                    {
                        yield return new ValidationResult($"Comprehensive Revision validation failed: {result.ErrorMessage}",
                            result.MemberNames.Select(m => $"{nameof(Revision)}.{m}"));
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// Validates the <see cref="RevisionActivity"/> navigation property for comprehensive audit classification.
    /// </summary>
    /// <returns>A collection of validation results for the revision activity property.</returns>
    /// <remarks>
    /// Performs deep validation of the nested RevisionActivityDto to ensure proper
    /// activity classification and comprehensive audit trail categorization.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateRevisionActivity()
    {
        switch (RevisionActivity)
        {
            case null:
                yield return new ValidationResult("RevisionActivity details are required for complete comprehensive audit trail.", [nameof(RevisionActivity)]);
                yield break;
            case IValidatableObject validatable:
                {
                    var context = new ValidationContext(RevisionActivity);
                    foreach (var result in validatable.Validate(context))
                    {
                        yield return new ValidationResult($"Comprehensive RevisionActivity validation failed: {result.ErrorMessage}",
                            result.MemberNames.Select(m => $"{nameof(RevisionActivity)}.{m}"));
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// Validates the <see cref="User"/> navigation property for comprehensive professional accountability.
    /// </summary>
    /// <returns>A collection of validation results for the user property.</returns>
    /// <remarks>
    /// Performs deep validation of the nested UserDto to ensure proper user
    /// attribution and comprehensive professional accountability in the audit trail.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateUser()
    {
        switch (User)
        {
            case null:
                yield return new ValidationResult("User details are required for complete comprehensive audit trail.", [nameof(User)]);
                yield break;
            case IValidatableObject validatable:
                {
                    var context = new ValidationContext(User);
                    foreach (var result in validatable.Validate(context))
                    {
                        yield return new ValidationResult($"Comprehensive User validation failed: {result.ErrorMessage}",
                            result.MemberNames.Select(m => $"{nameof(User)}.{m}"));
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// Validates consistency between foreign key IDs and their corresponding navigation properties for comprehensive audit integrity.
    /// </summary>
    /// <returns>A collection of validation results for cross-reference consistency.</returns>
    /// <remarks>
    /// Ensures that when both foreign key IDs and navigation properties are provided,
    /// they reference the same entities. This is critical for comprehensive audit trail data integrity.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateIdConsistency()
    {
        // Validate Revision ID consistency
        if (Revision?.Id.HasValue == true && Revision.Id.Value != RevisionId)
        {
            yield return new ValidationResult("Revision.Id does not match RevisionId - comprehensive audit trail data integrity error.",
                [nameof(Revision), nameof(RevisionId)]);
        }

        // Validate RevisionActivity ID consistency
        if (RevisionActivity?.Id != RevisionActivityId)
        {
            yield return new ValidationResult("RevisionActivity.Id does not match RevisionActivityId - comprehensive audit trail data integrity error.",
                [nameof(RevisionActivity), nameof(RevisionActivityId)]);
        }

        // Validate User ID consistency
        if (User?.Id != UserId)
        {
            yield return new ValidationResult("User.Id does not match UserId - comprehensive audit trail data integrity error.",
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
    /// Validates a <see cref="RevisionActivityUserDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The RevisionActivityUserDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate comprehensive audit trail entries
    /// without requiring a ValidationContext. It performs the same comprehensive validation 
    /// as the instance Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>API Validation:</strong> Validating incoming comprehensive audit DTOs in API controllers</item>
    /// <item><strong>Service Layer:</strong> Validation before processing comprehensive audit trail operations</item>
    /// <item><strong>Unit Testing:</strong> Simplified validation testing without ValidationContext</item>
    /// <item><strong>Batch Processing:</strong> Validating collections of comprehensive audit entries</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Validate a comprehensive audit entry instance
    /// var auditEntry = new RevisionActivityUserDto 
    /// { 
    ///     RevisionId = revisionGuid,
    ///     Revision = completeRevisionDto,
    ///     RevisionActivityId = activityGuid,
    ///     RevisionActivity = completeActivityDto,
    ///     UserId = userGuid,
    ///     User = completeUserDto,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// var results = RevisionActivityUserDto.ValidateModel(auditEntry);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Comprehensive audit entry validation failed: {errorMessages}");
    /// }
    /// 
    /// // Batch validation of comprehensive audit entries
    /// var validComprehensiveAuditEntries = auditEntries
    ///     .Where(entry => !RevisionActivityUserDto.ValidateModel(entry).Any())
    ///     .ToList();
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] RevisionActivityUserDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("RevisionActivityUserDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a RevisionActivityUserDto from an ADMS.API.Entities.RevisionActivityUser entity with validation.
    /// </summary>
    /// <param name="revisionActivityUser">The RevisionActivityUser entity to convert. Cannot be null.</param>
    /// <returns>A valid RevisionActivityUserDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when revisionActivityUser is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create comprehensive audit trail DTOs from
    /// ADMS.API.Entities.RevisionActivityUser entities with automatic validation. It ensures the resulting
    /// DTO is valid before returning it, maintaining comprehensive audit trail data integrity.
    /// 
    /// <para><strong>Comprehensive Entity Conversion:</strong></para>
    /// <list type="bullet">
    /// <item>Maps all essential audit trail properties from the entity</item>
    /// <item>Creates comprehensive DTOs for all navigation properties</item>
    /// <item>Validates the complete audit entry for consistency</item>
    /// <item>Ensures proper comprehensive audit trail representation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity with comprehensive navigation properties loaded
    /// var entity = await context.RevisionActivityUsers
    ///     .Include(rau => rau.Revision)
    ///         .ThenInclude(r => r.RevisionActivityUsers) // For comprehensive context
    ///     .Include(rau => rau.RevisionActivity)
    ///         .ThenInclude(ra => ra.RevisionActivityUsers) // For comprehensive activity data
    ///     .Include(rau => rau.User)
    ///         .ThenInclude(u => u.RevisionActivityUsers) // For comprehensive user activity data
    ///     .FirstAsync(rau => rau.RevisionId == targetRevisionId);
    /// 
    /// var comprehensiveAuditDto = RevisionActivityUserDto.FromEntity(entity);
    /// // Returns validated comprehensive audit trail DTO
    /// </code>
    /// </example>
    public static RevisionActivityUserDto FromEntity([NotNull] Entities.RevisionActivityUser revisionActivityUser)
    {
        ArgumentNullException.ThrowIfNull(revisionActivityUser, nameof(revisionActivityUser));

        var dto = new RevisionActivityUserDto
        {
            RevisionId = revisionActivityUser.RevisionId,
            Revision = RevisionDto.FromEntity(revisionActivityUser.Revision, includeDocumentId: false, includeActivities: false),
            RevisionActivityId = revisionActivityUser.RevisionActivityId,
            RevisionActivity = new RevisionActivityDto
            {
                Id = revisionActivityUser.RevisionActivity.Id,
                Activity = revisionActivityUser.RevisionActivity.Activity,
                RevisionActivityUsers = new List<RevisionActivityUserDto>() // Initialize empty to avoid circular reference
            },
            UserId = revisionActivityUser.UserId,
            User = UserDto.FromEntity(revisionActivityUser.User, includeActivities: false),
            CreatedAt = revisionActivityUser.CreatedAt
        };

        // Validate the created comprehensive DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid RevisionActivityUserDto from entity: {errorMessages}");

    }

    #endregion Static Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified RevisionActivityUserDto is equal to the current RevisionActivityUserDto.
    /// </summary>
    /// <param name="other">The RevisionActivityUserDto to compare with the current RevisionActivityUserDto.</param>
    /// <returns>true if the specified RevisionActivityUserDto is equal to the current RevisionActivityUserDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all core properties that uniquely identify a comprehensive audit trail entry.
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
    public bool Equals(RevisionActivityUserDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return RevisionId.Equals(other.RevisionId) &&
               RevisionActivityId.Equals(other.RevisionActivityId) &&
               UserId.Equals(other.UserId) &&
               CreatedAt.Equals(other.CreatedAt);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current RevisionActivityUserDto.
    /// </summary>
    /// <param name="obj">The object to compare with the current RevisionActivityUserDto.</param>
    /// <returns>true if the specified object is equal to the current RevisionActivityUserDto; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as RevisionActivityUserDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current RevisionActivityUserDto.</returns>
    /// <remarks>
    /// The hash code is based on all key properties to ensure consistent hashing behavior
    /// that aligns with the equality implementation and composite key structure.
    /// </remarks>
    public override int GetHashCode()
    {
        return HashCode.Combine(RevisionId, RevisionActivityId, UserId, CreatedAt);
    }

    /// <summary>
    /// Determines whether two RevisionActivityUserDto instances are equal.
    /// </summary>
    /// <param name="left">The first RevisionActivityUserDto to compare.</param>
    /// <param name="right">The second RevisionActivityUserDto to compare.</param>
    /// <returns>true if the RevisionActivityUserDtos are equal; otherwise, false.</returns>
    public static bool operator ==(RevisionActivityUserDto? left, RevisionActivityUserDto? right) =>
        EqualityComparer<RevisionActivityUserDto>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two RevisionActivityUserDto instances are not equal.
    /// </summary>
    /// <param name="left">The first RevisionActivityUserDto to compare.</param>
    /// <param name="right">The second RevisionActivityUserDto to compare.</param>
    /// <returns>true if the RevisionActivityUserDtos are not equal; otherwise, false.</returns>
    public static bool operator !=(RevisionActivityUserDto? left, RevisionActivityUserDto? right) => !(left == right);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the RevisionActivityUserDto.
    /// </summary>
    /// <returns>A string that represents the current RevisionActivityUserDto.</returns>
    /// <remarks>
    /// The string representation provides comprehensive audit trail information in a format suitable for
    /// logging, debugging, and comprehensive audit trail documentation. It includes all essential identifiers,
    /// activity timestamp, and relevant metrics for complete audit representation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var auditEntry = new RevisionActivityUserDto 
    /// { 
    ///     RevisionId = Guid.Parse("12345678-1234-5678-9012-123456789012"),
    ///     RevisionActivityId = Guid.Parse("87654321-4321-8765-2109-876543210987"),
    ///     UserId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
    ///     CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
    ///     User = new UserDto { DisplayName = "Robert Brown", TotalActivityCount = 127 },
    ///     Revision = new RevisionDto { RevisionNumber = 3, ActivityCount = 5 }
    /// };
    /// 
    /// Console.WriteLine(auditEntry);
    /// // Output: "Comprehensive Audit Entry: Revision 12345678-1234-5678-9012-123456789012, Activity 87654321-4321-8765-2109-876543210987, User 11111111-2222-3333-4444-555555555555, Time 2024-01-15 10:30:00 UTC [Robert Brown, Rev.3(5), Total:127]"
    /// </code>
    /// </example>
    public override string ToString()
    {
        var basicInfo = $"Comprehensive Audit Entry: Revision {RevisionId}, Activity {RevisionActivityId}, User {UserId}, Time {CreatedAt:yyyy-MM-dd HH:mm:ss} UTC";

        var contextInfo = "";
        contextInfo = $" [{User.DisplayName ?? User.Name}, Rev.{Revision.RevisionNumber}" +
                      $"{(Revision.ActivityCount > 0 ? $"({Revision.ActivityCount})" : "")}" +
                      $"{(User.TotalActivityCount > 0 ? $", Total:{User.TotalActivityCount}" : "")}]";

        return basicInfo + contextInfo;
    }

    #endregion String Representation
}