using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Models;

/// <summary>
/// Minimal Data Transfer Object representing essential document activity audit trail information for efficient matter-document activity tracking.
/// </summary>
/// <remarks>
/// This DTO serves as a lightweight representation of document activity audit trails within the ADMS legal 
/// document management system, corresponding to <see cref="ADMS.API.Entities.MatterDocumentActivityUser"/>. 
/// It provides only essential properties required for matter-document activity identification, audit trail 
/// display, and basic activity tracking while excluding detailed navigation properties for optimal performance.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Audit Trail Representation:</strong> Essential properties for document activity tracking without relationship overhead</item>
/// <item><strong>Performance Optimized:</strong> Excludes detailed navigation properties for fast activity enumeration</item>
/// <item><strong>Professional Validation:</strong> Uses centralized validation helpers for comprehensive data integrity</item>
/// <item><strong>Immutable Design:</strong> Record type with init-only properties for thread-safe audit operations</item>
/// <item><strong>Display Ready:</strong> Pre-validated for immediate use in audit trail displays and activity lists</item>
/// </list>
/// 
/// <para><strong>Entity Alignment:</strong></para>
/// This DTO mirrors the composite key structure from ADMS.API.Entities.MatterDocumentActivityUser:
/// <list type="bullet">
/// <item><strong>MatterId:</strong> Matter identifier for activity context</item>
/// <item><strong>DocumentId:</strong> Document identifier for activity tracking</item>
/// <item><strong>MatterDocumentActivityId:</strong> Activity type identifier for operation classification</item>
/// <item><strong>UserId:</strong> User identifier for professional accountability</item>
/// <item><strong>CreatedAt:</strong> Activity timestamp for chronological tracking</item>
/// <item><strong>Minimal Navigation:</strong> Essential navigation properties (MatterMinimalDto, MatterDocumentActivityMinimalDto, UserMinimalDto)</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Activity Lists:</strong> Document activity browsing and audit trail displays</item>
/// <item><strong>Audit Reports:</strong> Lightweight activity reporting and compliance documentation</item>
/// <item><strong>API Responses:</strong> Efficient activity data transfer in REST API responses</item>
/// <item><strong>Activity Filtering:</strong> Activity search and filtering operations</item>
/// <item><strong>Performance-Critical Operations:</strong> Bulk activity processing with minimal memory footprint</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Activity Tracking:</strong> Essential audit trail for legal document management</item>
/// <item><strong>Professional Accountability:</strong> User attribution for document operations within matter context</item>
/// <item><strong>Compliance Reporting:</strong> Minimal data for regulatory compliance and audit requirements</item>
/// <item><strong>Practice Efficiency:</strong> Optimized for rapid activity tracking and professional workflows</item>
/// </list>
/// 
/// <para><strong>Performance Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Memory Footprint:</strong> Only essential properties and minimal navigation data</item>
/// <item><strong>Fast Serialization:</strong> Quick JSON serialization/deserialization for API operations</item>
/// <item><strong>Database Efficiency:</strong> Optimal for database projections and bulk audit operations</item>
/// <item><strong>UI Responsiveness:</strong> Fast loading in activity lists and audit trail displays</item>
/// </list>
/// 
/// <para><strong>When to Use vs Other Activity DTOs:</strong></para>
/// <list type="bullet">
/// <item><strong>Use MatterDocumentActivityUserMinimalDto:</strong> For activity listings, audit reports, and performance-critical scenarios</item>
/// <item><strong>Use MatterDocumentActivityUserFromDto/ToDto:</strong> For complete bidirectional transfer audit trails</item>
/// <item><strong>Use DocumentActivityUserDto:</strong> For document-specific activity tracking without matter context</item>
/// </list>
/// 
/// <para><strong>Audit Trail Integration:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter-Document Linkage:</strong> Links document activities to specific matters for comprehensive audit trails</item>
/// <item><strong>User Attribution:</strong> Maintains professional accountability for all document operations</item>
/// <item><strong>Activity Classification:</strong> Clear categorization of document operations within matter context</item>
/// <item><strong>Temporal Tracking:</strong> Precise timestamp tracking for legal compliance and professional standards</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a minimal document activity audit entry
/// var activityMinimal = new MatterDocumentActivityUserMinimalDto
/// {
///     MatterId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
///     Matter = new MatterMinimalDto 
///     { 
///         Id = Guid.Parse("60000000-0000-0000-0000-000000000001"),
///         Description = "Smith Family Trust"
///     },
///     DocumentId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
///     MatterDocumentActivityId = Guid.Parse("30000000-0000-0000-0000-000000000001"),
///     MatterDocumentActivity = new MatterDocumentActivityMinimalDto
///     {
///         Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
///         Activity = "SAVED"
///     },
///     UserId = Guid.Parse("50000000-0000-0000-0000-000000000001"),
///     User = new UserMinimalDto
///     {
///         Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
///         Name = "Robert Brown"
///     },
///     CreatedAt = DateTime.UtcNow
/// };
/// 
/// // Validation example
/// var validationResults = MatterDocumentActivityUserMinimalDto.ValidateModel(activityMinimal);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Activity Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Professional audit trail display
/// Console.WriteLine($"Activity: {activityMinimal.ActivitySummary} on {activityMinimal.LocalCreatedAtDateString}");
/// </code>
/// </example>
public record MatterDocumentActivityUserMinimalDto : IValidatableObject, IEquatable<MatterDocumentActivityUserMinimalDto>
{
    #region Composite Primary Key Properties

    /// <summary>
    /// Gets the unique identifier for the matter associated with the document activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the matter context for the 
    /// document activity. It corresponds directly to <see cref="ADMS.API.Entities.MatterDocumentActivityUser.MatterId"/>
    /// and provides essential matter context for audit trail operations.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> First component of the five-part composite primary key</item>
    /// <item><strong>Matter Context:</strong> Identifies which matter provides context for the document activity</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing Matter entity</item>
    /// <item><strong>Audit Trail Foundation:</strong> Essential for matter-based activity tracking</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Matter Association:</strong> Links document activities to specific legal matters</item>
    /// <item><strong>Professional Organization:</strong> Supports matter-based document organization strategies</item>
    /// <item><strong>Client Attribution:</strong> Associates document activities with appropriate clients or cases</item>
    /// <item><strong>Audit Context:</strong> Provides essential context for professional audit and compliance</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUser.MatterId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Setting matter context for document activity
    /// var activityAudit = new MatterDocumentActivityUserMinimalDto
    /// {
    ///     MatterId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Professional reporting usage
    /// Console.WriteLine($"Document activity in matter ID: {activityAudit.MatterId}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter ID is required for document activity context.")]
    public required Guid MatterId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the document involved in the activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific document that 
    /// is the subject of the activity. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivityUser.DocumentId"/> and provides precise 
    /// document identification for audit trail operations.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Second component of the five-part composite primary key</item>
    /// <item><strong>Document Identification:</strong> Precisely identifies which document is involved in the activity</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing Document entity</item>
    /// <item><strong>Activity Tracking:</strong> Essential for document-specific audit trail operations</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Custody:</strong> Tracks activities on specific legal documents</item>
    /// <item><strong>Professional Accountability:</strong> Identifies exactly which documents are being operated on</item>
    /// <item><strong>Client Communication:</strong> Enables precise client communication about specific document activities</item>
    /// <item><strong>Legal Discovery:</strong> Critical for legal discovery and document production requirements</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUser.DocumentId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Identifying specific document in activity
    /// var documentActivity = new MatterDocumentActivityUserMinimalDto
    /// {
    ///     MatterId = matterGuid,
    ///     DocumentId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Professional document tracking
    /// Console.WriteLine($"Activity on document ID: {documentActivity.DocumentId}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Document ID is required for document activity identification.")]
    public required Guid DocumentId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the specific matter document activity type.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific type of document 
    /// activity being performed within the matter context. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivityUser.MatterDocumentActivityId"/> and provides 
    /// essential activity classification for audit trail operations.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Third component of the five-part composite primary key</item>
    /// <item><strong>Activity Classification:</strong> Identifies the type of activity being performed</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing MatterDocumentActivity entity</item>
    /// <item><strong>Operation Type Tracking:</strong> Essential for categorizing document operations within matters</item>
    /// </list>
    /// 
    /// <para><strong>Activity Types (via MatterDocumentActivityMinimalDto):</strong></para>
    /// <list type="bullet">
    /// <item><strong>SAVED:</strong> Document saved within matter context</item>
    /// <item><strong>MOVED:</strong> Document moved between matters</item>
    /// <item><strong>COPIED:</strong> Document copied between matters</item>
    /// <item><strong>DELETED:</strong> Document deleted from matter</item>
    /// <item><strong>RESTORED:</strong> Document restored to matter</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUser.MatterDocumentActivityId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Specifying activity type
    /// var saveActivity = new MatterDocumentActivityUserMinimalDto
    /// {
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = saveActivityGuid, // References "SAVED" activity
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Professional activity tracking
    /// Console.WriteLine($"Document activity type ID: {saveActivity.MatterDocumentActivityId}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter document activity ID is required for activity classification.")]
    public required Guid MatterDocumentActivityId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the user performing the document activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific user who performed 
    /// the document activity. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivityUser.UserId"/> and provides essential user 
    /// attribution for professional accountability and audit trail completeness.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Fourth component of the five-part composite primary key</item>
    /// <item><strong>User Attribution:</strong> Identifies who performed the document activity</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing User entity</item>
    /// <item><strong>Accountability Tracking:</strong> Essential for professional accountability and responsibility</item>
    /// </list>
    /// 
    /// <para><strong>Professional Accountability:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Responsibility:</strong> Establishes clear accountability for document operations</item>
    /// <item><strong>Professional Standards:</strong> Supports professional practice responsibility requirements</item>
    /// <item><strong>Audit Trail Completeness:</strong> Ensures all document activities have clear user attribution</item>
    /// <item><strong>Legal Compliance:</strong> Meets legal requirements for document handling accountability</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUser.UserId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // User attribution for document activity
    /// var userActivity = new MatterDocumentActivityUserMinimalDto
    /// {
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = Guid.Parse("50000000-0000-0000-0000-000000000001"), // Robert Brown
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Professional accountability reporting
    /// Console.WriteLine($"Document activity performed by user ID: {userActivity.UserId}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User ID is required for professional accountability in document activities.")]
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the document activity was performed.
    /// </summary>
    /// <remarks>
    /// This DateTime serves as the final component of the composite primary key and records the precise 
    /// moment when the document activity was completed. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivityUser.CreatedAt"/> and provides essential 
    /// temporal tracking for audit trails, legal compliance, and professional accountability.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Fifth and final component of the composite primary key</item>
    /// <item><strong>Temporal Foundation:</strong> Establishes precise timing for document activities</item>
    /// <item><strong>Audit Trail Chronology:</strong> Enables chronological sequencing of document operations</item>
    /// <item><strong>Professional Standards:</strong> Meets professional practice temporal tracking requirements</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUser.CreatedAt"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Setting precise activity timestamp
    /// var timestampedActivity = new MatterDocumentActivityUserMinimalDto
    /// {
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow // Always use UTC
    /// };
    /// 
    /// // Professional temporal reporting
    /// Console.WriteLine($"Activity completed at: {timestampedActivity.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Creation timestamp is required for audit trail temporal tracking.")]
    public required DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    #endregion Composite Primary Key Properties

    #region Minimal Navigation Properties

    /// <summary>
    /// Gets the minimal matter information for the matter associated with the document activity.
    /// </summary>
    /// <remarks>
    /// This property provides essential matter information for activity context while maintaining the minimal 
    /// footprint design. It corresponds to the <see cref="ADMS.API.Entities.MatterDocumentActivityUser.Matter"/> 
    /// navigation property but uses MatterMinimalDto to optimize performance and memory usage.
    /// 
    /// <para><strong>Performance Optimization:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Minimal Data:</strong> Only essential matter properties (ID, Description, Status) for context</item>
    /// <item><strong>Memory Efficiency:</strong> Excludes document collections and activity relationships</item>
    /// <item><strong>Display Ready:</strong> Sufficient information for audit trail display and professional reporting</item>
    /// <item><strong>Validation Integration:</strong> Cross-reference validation with MatterId when provided</item>
    /// </list>
    /// 
    /// <para><strong>Professional Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Matter Identification:</strong> Clear matter identification for professional audit trails</item>
    /// <item><strong>Client Context:</strong> Provides client/matter context for document activity reporting</item>
    /// <item><strong>Professional Display:</strong> Enables professional presentation of activity context</item>
    /// <item><strong>Audit Compliance:</strong> Sufficient information for compliance reporting requirements</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Activity with minimal matter context
    /// var activityWithMatter = new MatterDocumentActivityUserMinimalDto
    /// {
    ///     MatterId = matterGuid,
    ///     Matter = new MatterMinimalDto
    ///     {
    ///         Id = matterGuid,
    ///         Description = "Smith Family Trust",
    ///         IsArchived = false
    ///     },
    ///     // ... other properties
    /// };
    /// 
    /// // Professional audit trail display
    /// if (activityWithMatter.Matter != null)
    /// {
    ///     Console.WriteLine($"Document activity in matter: '{activityWithMatter.Matter.Description}'");
    /// }
    /// </code>
    /// </example>
    public MatterMinimalDto? Matter { get; init; }

    /// <summary>
    /// Gets the minimal matter document activity information describing the type of activity performed.
    /// </summary>
    /// <remarks>
    /// This property provides essential activity classification information while maintaining the minimal 
    /// footprint design. It corresponds to the <see cref="ADMS.API.Entities.MatterDocumentActivityUser.MatterDocumentActivity"/> 
    /// navigation property but uses MatterDocumentActivityMinimalDto to optimize performance.
    /// 
    /// <para><strong>Activity Classification:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Operation Type:</strong> Clear identification of document operation type (SAVED, MOVED, COPIED, etc.)</item>
    /// <item><strong>Professional Standards:</strong> Maintains professional activity classification standards</item>
    /// <item><strong>Audit Integration:</strong> Integrates with audit trail systems for comprehensive tracking</item>
    /// <item><strong>Display Optimization:</strong> Optimized for activity list and audit trail display</item>
    /// </list>
    /// 
    /// <para><strong>Performance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Essential Data Only:</strong> Contains only ID and Activity description for minimal footprint</item>
    /// <item><strong>Fast Loading:</strong> Quick loading in activity lists and audit displays</item>
    /// <item><strong>Memory Efficient:</strong> Minimal memory usage for bulk activity operations</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Activity with classification information
    /// var classifiedActivity = new MatterDocumentActivityUserMinimalDto
    /// {
    ///     MatterDocumentActivityId = activityGuid,
    ///     MatterDocumentActivity = new MatterDocumentActivityMinimalDto
    ///     {
    ///         Id = activityGuid,
    ///         Activity = "SAVED"
    ///     },
    ///     // ... other properties
    /// };
    /// 
    /// // Professional activity reporting
    /// if (classifiedActivity.MatterDocumentActivity != null)
    /// {
    ///     var activityType = classifiedActivity.MatterDocumentActivity.Activity;
    ///     Console.WriteLine($"Document operation: {activityType}");
    /// }
    /// </code>
    /// </example>
    public MatterDocumentActivityMinimalDto? MatterDocumentActivity { get; init; }

    /// <summary>
    /// Gets the minimal user information for the user who performed the document activity.
    /// </summary>
    /// <remarks>
    /// This property provides essential user information for professional accountability while maintaining 
    /// the minimal footprint design. It corresponds to the <see cref="ADMS.API.Entities.MatterDocumentActivityUser.User"/> 
    /// navigation property but uses UserMinimalDto to optimize performance and memory usage.
    /// 
    /// <para><strong>Professional Accountability:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Attribution:</strong> Clear professional attribution for document activities</item>
    /// <item><strong>Professional Display:</strong> Professional name presentation for audit trails and reports</item>
    /// <item><strong>Accountability Standards:</strong> Meets professional practice accountability requirements</item>
    /// <item><strong>Legal Compliance:</strong> Supports legal compliance and professional responsibility requirements</item>
    /// </list>
    /// 
    /// <para><strong>Performance Optimization:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Essential Data:</strong> Only ID and Name properties for minimal footprint</item>
    /// <item><strong>Display Ready:</strong> Sufficient information for professional audit trail display</item>
    /// <item><strong>Memory Efficient:</strong> Excludes activity collections and detailed user information</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Activity with user attribution
    /// var userAttributedActivity = new MatterDocumentActivityUserMinimalDto
    /// {
    ///     UserId = userGuid,
    ///     User = new UserMinimalDto
    ///     {
    ///         Id = userGuid,
    ///         Name = "Robert Brown"
    ///     },
    ///     // ... other properties
    /// };
    /// 
    /// // Professional accountability display
    /// if (userAttributedActivity.User != null)
    /// {
    ///     Console.WriteLine($"Activity performed by: {userAttributedActivity.User.Name}");
    /// }
    /// </code>
    /// </example>
    public UserMinimalDto? User { get; init; }

    #endregion Minimal Navigation Properties

    #region Computed Properties

    /// <summary>
    /// Gets the creation timestamp formatted as a localized string for professional presentation.
    /// </summary>
    /// <remarks>
    /// This computed property provides a user-friendly formatted representation of the activity timestamp 
    /// converted to local time, optimized for professional interfaces, audit trail displays, and client 
    /// communications.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new MatterDocumentActivityUserMinimalDto 
    /// { 
    ///     CreatedAt = new DateTime(2024, 1, 15, 14, 30, 45, DateTimeKind.Utc),
    ///     // ... other properties
    /// };
    /// 
    /// // Professional audit trail display
    /// Console.WriteLine($"Activity completed: {activity.LocalCreatedAtDateString}");
    /// // Output: "Activity completed: Monday, 15 January 2024 16:30:45" (if local time is UTC+2)
    /// </code>
    /// </example>
    public string LocalCreatedAtDateString => CreatedAt.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets a concise activity summary for professional audit trail display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a professional summary of the document activity including key 
    /// participants and operation details, optimized for audit trail reports and professional communication.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activitySummary = activity.ActivitySummary;
    /// // Returns: "SAVED by Robert Brown in Smith Family Trust"
    /// 
    /// Console.WriteLine($"Activity Summary: {activitySummary}");
    /// </code>
    /// </example>
    public string ActivitySummary =>
        $"{MatterDocumentActivity?.Activity ?? "ACTIVITY"} " +
        $"by {User?.Name ?? "User"} " +
        $"in {Matter?.Description ?? "Matter"}";

    /// <summary>
    /// Gets the age of this activity in days since it was performed.
    /// </summary>
    /// <remarks>
    /// This computed property calculates how long ago the activity occurred, useful for activity 
    /// analysis, audit trail review, and professional practice management.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityAge = activity.ActivityAgeDays;
    /// if (activityAge < 1)
    /// {
    ///     Console.WriteLine("Recent activity (today)");
    /// }
    /// else if (activityAge <= 7)
    /// {
    ///     Console.WriteLine($"Recent activity ({activityAge:F0} days ago)");
    /// }
    /// </code>
    /// </example>
    public double ActivityAgeDays => (DateTime.UtcNow - CreatedAt).TotalDays;

    /// <summary>
    /// Gets a value indicating whether this activity occurred recently (within 7 days).
    /// </summary>
    /// <remarks>
    /// This computed property helps identify recent activity for audit analysis, user interface 
    /// highlighting, and activity reporting scenarios.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsRecentActivity)
    /// {
    ///     // Highlight recent activity in UI
    ///     activityItem.CssClass = "recent-activity";
    /// }
    /// </code>
    /// </example>
    public bool IsRecentActivity => ActivityAgeDays <= 7;

    /// <summary>
    /// Gets comprehensive activity metrics for analysis and reporting.
    /// </summary>
    /// <remarks>
    /// This property provides a structured object containing key metrics and information 
    /// for comprehensive activity analysis and professional reporting purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var metrics = activity.ActivityMetrics;
    /// // Access comprehensive activity metrics for analysis
    /// </code>
    /// </example>
    public object ActivityMetrics => new
    {
        ActivityInfo = new
        {
            ActivitySummary,
            LocalCreatedAtDateString,
            ActivityType = MatterDocumentActivity?.Activity ?? "UNKNOWN",
            ActivityAgeDays,
            IsRecentActivity
        },
        ParticipantInfo = new
        {
            MatterDescription = Matter?.Description ?? "Unknown Matter",
            UserName = User?.Name ?? "Unknown User",
            UserId,
            MatterId,
            DocumentId
        },
        TemporalInfo = new
        {
            CreatedAt,
            LocalCreatedAtDateString,
            ActivityAgeDays,
            IsRecentActivity
        },
        ValidationInfo = new
        {
            HasCompleteInformation = Matter != null && MatterDocumentActivity != null && User != null,
            RequiredFieldsPresent = MatterId != Guid.Empty && DocumentId != Guid.Empty &&
                                  MatterDocumentActivityId != Guid.Empty && UserId != Guid.Empty
        }
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="MatterDocumentActivityUserMinimalDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using centralized validation helpers for consistency with entity 
    /// validation rules while optimized for minimal DTO requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityUserMinimalDto 
    /// { 
    ///     MatterId = Guid.Empty, // Invalid
    ///     DocumentId = Guid.Empty, // Invalid
    ///     MatterDocumentActivityId = Guid.Empty, // Invalid
    ///     UserId = Guid.Empty, // Invalid
    ///     CreatedAt = DateTime.MinValue // Invalid
    /// };
    /// 
    /// var context = new ValidationContext(dto);
    /// var results = dto.Validate(context);
    /// 
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"Activity Validation Error: {result.ErrorMessage}");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate all composite primary key GUID components
        foreach (var result in ValidateGuids())
            yield return result;

        // Validate navigation properties when provided (optional for minimal DTO)
        foreach (var result in ValidateMatter())
            yield return result;

        foreach (var result in ValidateMatterDocumentActivity())
            yield return result;

        foreach (var result in ValidateUser())
            yield return result;

        // Validate cross-reference consistency between IDs and navigation properties
        foreach (var result in ValidateIdConsistency())
            yield return result;

        // Validate temporal requirements for audit trail
        foreach (var result in ValidateCreatedAt())
            yield return result;
    }

    /// <summary>
    /// Validates all GUID properties that form the composite primary key.
    /// </summary>
    /// <returns>A collection of validation results for GUID validation.</returns>
    /// <remarks>
    /// Ensures all five GUID components of the composite primary key are valid non-empty GUIDs, 
    /// which is essential for audit trail integrity and entity relationship consistency.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateGuids()
    {
        if (MatterId == Guid.Empty)
            yield return new ValidationResult(
                "Matter ID must be a valid non-empty GUID for audit trail context.",
                [nameof(MatterId)]);

        if (DocumentId == Guid.Empty)
            yield return new ValidationResult(
                "Document ID must be a valid non-empty GUID for activity tracking.",
                [nameof(DocumentId)]);

        if (MatterDocumentActivityId == Guid.Empty)
            yield return new ValidationResult(
                "Matter document activity ID must be a valid non-empty GUID for operation classification.",
                [nameof(MatterDocumentActivityId)]);

        if (UserId == Guid.Empty)
            yield return new ValidationResult(
                "User ID must be a valid non-empty GUID for professional accountability.",
                [nameof(UserId)]);
    }

    /// <summary>
    /// Validates the <see cref="Matter"/> navigation property using centralized validation.
    /// </summary>
    /// <returns>A collection of validation results for matter validation.</returns>
    /// <remarks>
    /// Validates the minimal matter information when provided, ensuring data integrity for audit context.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateMatter()
    {
        if (Matter is not IValidatableObject validatable) yield break;
        var context = new ValidationContext(Matter);
        foreach (var result in validatable.Validate(context))
            yield return new ValidationResult($"Matter Context: {result.ErrorMessage}", result.MemberNames);
    }

    /// <summary>
    /// Validates the <see cref="MatterDocumentActivity"/> navigation property using centralized validation.
    /// </summary>
    /// <returns>A collection of validation results for activity validation.</returns>
    /// <remarks>
    /// Validates the minimal activity information when provided, ensuring proper activity classification.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateMatterDocumentActivity()
    {
        if (MatterDocumentActivity is not IValidatableObject validatable) yield break;
        var context = new ValidationContext(MatterDocumentActivity);
        foreach (var result in validatable.Validate(context))
            yield return new ValidationResult($"Activity Classification: {result.ErrorMessage}", result.MemberNames);
    }

    /// <summary>
    /// Validates the <see cref="User"/> navigation property using centralized validation.
    /// </summary>
    /// <returns>A collection of validation results for user validation.</returns>
    /// <remarks>
    /// Validates the minimal user information when provided, ensuring professional accountability integrity.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateUser()
    {
        if (User is not IValidatableObject validatable) yield break;
        var context = new ValidationContext(User);
        foreach (var result in validatable.Validate(context))
            yield return new ValidationResult($"User Attribution: {result.ErrorMessage}", result.MemberNames);
    }

    /// <summary>
    /// Validates consistency between foreign key IDs and their corresponding navigation properties.
    /// </summary>
    /// <returns>A collection of validation results for cross-reference consistency.</returns>
    /// <remarks>
    /// Ensures that when navigation properties are provided, their ID values match the corresponding 
    /// foreign key properties, maintaining referential integrity.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateIdConsistency()
    {
        if (Matter != null && Matter.Id != MatterId)
            yield return new ValidationResult(
                "Matter.Id does not match MatterId - referential integrity violation.",
                [nameof(Matter), nameof(MatterId)]);

        if (MatterDocumentActivity != null && MatterDocumentActivity.Id != MatterDocumentActivityId)
            yield return new ValidationResult(
                "MatterDocumentActivity.Id does not match MatterDocumentActivityId - referential integrity violation.",
                [nameof(MatterDocumentActivity), nameof(MatterDocumentActivityId)]);

        if (User != null && User.Id != UserId)
            yield return new ValidationResult(
                "User.Id does not match UserId - referential integrity violation.",
                [nameof(User), nameof(UserId)]);
    }

    /// <summary>
    /// Validates the <see cref="CreatedAt"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for timestamp validation.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.MatterDocumentActivityValidationHelper.IsValidDate for comprehensive temporal 
    /// validation, ensuring the timestamp meets professional standards.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateCreatedAt()
    {
        // Replace IsValidDate with direct validation logic
        if (CreatedAt > DateTime.UtcNow)
            yield return new ValidationResult(
                "CreatedAt must not be in the future for audit trail integrity.",
                [nameof(CreatedAt)]);

        if (CreatedAt < new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            yield return new ValidationResult(
                "CreatedAt is unreasonably far in the past for a document activity.",
                [nameof(CreatedAt)]);
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="MatterDocumentActivityUserMinimalDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The MatterDocumentActivityUserMinimalDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate minimal activity DTOs 
    /// without requiring a ValidationContext.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityUserMinimalDto 
    /// { 
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// var results = MatterDocumentActivityUserMinimalDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Activity validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterDocumentActivityUserMinimalDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterDocumentActivityUserMinimalDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a MatterDocumentActivityUserMinimalDto from ADMS.API.Entities.MatterDocumentActivityUser entity with validation.
    /// </summary>
    /// <param name="entity">The MatterDocumentActivityUser entity to convert. Cannot be null.</param>
    /// <param name="includeMinimalNavigation">Whether to include minimal navigation properties.</param>
    /// <returns>A valid MatterDocumentActivityUserMinimalDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create minimal activity DTOs from entities.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entity = new ADMS.API.Entities.MatterDocumentActivityUser 
    /// { 
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// var dto = MatterDocumentActivityUserMinimalDto.FromEntity(entity, includeMinimalNavigation: true);
    /// </code>
    /// </example>
    public static MatterDocumentActivityUserMinimalDto FromEntity([NotNull] Entities.MatterDocumentActivityUser entity, bool includeMinimalNavigation = false)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var dto = new MatterDocumentActivityUserMinimalDto
        {
            MatterId = entity.MatterId,
            DocumentId = entity.DocumentId,
            MatterDocumentActivityId = entity.MatterDocumentActivityId,
            UserId = entity.UserId,
            CreatedAt = entity.CreatedAt
        };

        // Note: In practice, navigation properties would be mapped using a mapping framework
        // when includeMinimalNavigation is true

        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterDocumentActivityUserMinimalDto from entity: {errorMessages}");

    }

    /// <summary>
    /// Creates a MatterDocumentActivityUserMinimalDto from ADMS.API.Entities.MatterDocumentActivityUserFrom entity with validation.
    /// </summary>
    /// <param name="entity">The MatterDocumentActivityUserFrom entity to convert. Cannot be null.</param>
    /// <param name="includeMinimalNavigation">Whether to include minimal navigation properties.</param>
    /// <returns>A valid MatterDocumentActivityUserMinimalDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create minimal activity DTOs from "From" transfer entities.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entity = new ADMS.API.Entities.MatterDocumentActivityUserFrom 
    /// { 
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// var dto = MatterDocumentActivityUserMinimalDto.FromEntity(entity, includeMinimalNavigation: true);
    /// </code>
    /// </example>
    public static MatterDocumentActivityUserMinimalDto FromEntity([NotNull] Entities.MatterDocumentActivityUserFrom entity, bool includeMinimalNavigation = false)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var dto = new MatterDocumentActivityUserMinimalDto
        {
            MatterId = entity.MatterId,
            DocumentId = entity.DocumentId,
            MatterDocumentActivityId = entity.MatterDocumentActivityId,
            UserId = entity.UserId,
            CreatedAt = entity.CreatedAt,
            Matter = includeMinimalNavigation ? MatterMinimalDto.FromEntity(entity.Matter) : null,
            MatterDocumentActivity = includeMinimalNavigation ? MatterDocumentActivityMinimalDto.FromEntity(entity.MatterDocumentActivity) : null,
            User = includeMinimalNavigation ? UserMinimalDto.FromEntity(entity.User) : null
        };

        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterDocumentActivityUserMinimalDto from MatterDocumentActivityUserFrom entity: {errorMessages}");
    }

    /// <summary>
    /// Creates a MatterDocumentActivityUserMinimalDto from ADMS.API.Entities.MatterDocumentActivityUserTo entity with validation.
    /// </summary>
    /// <param name="entity">The MatterDocumentActivityUserTo entity to convert. Cannot be null.</param>
    /// <param name="includeMinimalNavigation">Whether to include minimal navigation properties.</param>
    /// <returns>A valid MatterDocumentActivityUserMinimalDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create minimal activity DTOs from "To" transfer entities.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entity = new ADMS.API.Entities.MatterDocumentActivityUserTo 
    /// { 
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// var dto = MatterDocumentActivityUserMinimalDto.FromEntity(entity, includeMinimalNavigation: true);
    /// </code>
    /// </example>
    public static MatterDocumentActivityUserMinimalDto FromEntity([NotNull] Entities.MatterDocumentActivityUserTo entity, bool includeMinimalNavigation = false)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var dto = new MatterDocumentActivityUserMinimalDto
        {
            MatterId = entity.MatterId,
            DocumentId = entity.DocumentId,
            MatterDocumentActivityId = entity.MatterDocumentActivityId,
            UserId = entity.UserId,
            CreatedAt = entity.CreatedAt,
            Matter = includeMinimalNavigation ? MatterMinimalDto.FromEntity(entity.Matter) : null,
            MatterDocumentActivity = includeMinimalNavigation ? MatterDocumentActivityMinimalDto.FromEntity(entity.MatterDocumentActivity) : null,
            User = includeMinimalNavigation ? UserMinimalDto.FromEntity(entity.User) : null
        };

        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterDocumentActivityUserMinimalDto from MatterDocumentActivityUserTo entity: {errorMessages}");
    }
    
    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this activity represents a document save operation.
    /// </summary>
    /// <returns>true if this represents a SAVED operation; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify save operations for activity analysis and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsSaveOperation())
    /// {
    ///     Console.WriteLine("Document was saved in matter context");
    /// }
    /// </code>
    /// </example>
    public bool IsSaveOperation() =>
        string.Equals(MatterDocumentActivity?.Activity, "SAVED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a document transfer operation.
    /// </summary>
    /// <returns>true if this represents a MOVED or COPIED operation; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify transfer operations for audit analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsTransferOperation())
    /// {
    ///     Console.WriteLine("Document was transferred between matters");
    /// }
    /// </code>
    /// </example>
    public bool IsTransferOperation()
    {
        var activityType = MatterDocumentActivity?.Activity;
        return string.Equals(activityType, "MOVED", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(activityType, "COPIED", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets comprehensive activity information for reporting and analysis.
    /// </summary>
    /// <returns>A dictionary containing detailed activity information.</returns>
    /// <remarks>
    /// This method provides structured activity information for audit reports and analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityInfo = activity.GetActivityInformation();
    /// foreach (var item in activityInfo)
    /// {
    ///     Console.WriteLine($"{item.Key}: {item.Value}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetActivityInformation()
    {
        return new Dictionary<string, object>
        {
            ["ActivityType"] = MatterDocumentActivity?.Activity ?? "UNKNOWN",
            ["IsSaveOperation"] = IsSaveOperation(),
            ["IsTransferOperation"] = IsTransferOperation(),
            ["MatterContext"] = Matter?.Description ?? "Unknown Matter",
            ["UserName"] = User?.Name ?? "Unknown User",
            ["ActivityDate"] = CreatedAt,
            ["LocalActivityDate"] = LocalCreatedAtDateString,
            ["ActivityAge"] = ActivityAgeDays,
            ["IsRecent"] = IsRecentActivity,
            ["ActivitySummary"] = ActivitySummary,
            ["HasCompleteInformation"] = Matter != null && MatterDocumentActivity != null && User != null
        }.ToImmutableDictionary();
    }

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterDocumentActivityUserMinimalDto is equal to the current MatterDocumentActivityUserMinimalDto.
    /// </summary>
    /// <param name="other">The MatterDocumentActivityUserMinimalDto to compare with the current MatterDocumentActivityUserMinimalDto.</param>
    /// <returns>true if the specified MatterDocumentActivityUserMinimalDto is equal to the current MatterDocumentActivityUserMinimalDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all composite key properties that uniquely identify an activity audit entry.
    /// </remarks>
    public virtual bool Equals(MatterDocumentActivityUserMinimalDto? other)
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
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterDocumentActivityUserMinimalDto.</returns>
    /// <remarks>
    /// The hash code is based on all composite key properties to ensure consistent hashing behavior.
    /// </remarks>
    public override int GetHashCode()
    {
        return HashCode.Combine(MatterId, DocumentId, MatterDocumentActivityId, UserId, CreatedAt);
    }

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterDocumentActivityUserMinimalDto.
    /// </summary>
    /// <returns>A string that represents the current MatterDocumentActivityUserMinimalDto.</returns>
    /// <remarks>
    /// The string representation includes key activity information for identification and logging purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityUserMinimalDto 
    /// { 
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "Activity: Document (70000000-...) in Matter (60000000-...) by User (50000000-...) at 2024-01-15 14:30:45"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Activity: Document ({DocumentId}) in Matter ({MatterId}) by User ({UserId}) at {CreatedAt:yyyy-MM-dd HH:mm:ss}";

    #endregion String Representation
}