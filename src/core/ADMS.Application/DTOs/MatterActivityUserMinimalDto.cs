using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Minimal Data Transfer Object representing essential matter activity audit trail information for efficient matter activity tracking.
/// </summary>
/// <remarks>
/// This DTO serves as a lightweight representation of matter activity audit trails within the ADMS legal 
/// document management system, corresponding to <see cref="ADMS.API.Entities.MatterActivityUser"/>. 
/// It provides only essential properties required for matter activity identification, audit trail 
/// display, and activity tracking while excluding detailed relationships for optimal performance.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Activity Audit Representation:</strong> Essential properties for matter activity tracking without relationship overhead</item>
/// <item><strong>Performance Optimized:</strong> Excludes detailed relationships for fast activity enumeration</item>
/// <item><strong>Professional Validation:</strong> Uses centralized validation helpers for comprehensive data integrity</item>
/// <item><strong>Immutable Design:</strong> Record type with init-only properties for thread-safe audit operations</item>
/// <item><strong>Display Ready:</strong> Pre-validated for immediate use in activity displays and matter audit trails</item>
/// </list>
/// 
/// <para><strong>Entity Alignment:</strong></para>
/// This DTO mirrors the composite key structure from ADMS.API.Entities.MatterActivityUser:
/// <list type="bullet">
/// <item><strong>MatterId:</strong> Matter identifier for activity context</item>
/// <item><strong>MatterActivityId:</strong> Activity type identifier for operation classification</item>
/// <item><strong>UserId:</strong> User identifier for professional accountability</item>
/// <item><strong>CreatedAt:</strong> Activity timestamp for chronological tracking</item>
/// <item><strong>Minimal Navigation:</strong> Essential navigation properties (MatterMinimalDto, MatterActivityMinimalDto, UserMinimalDto)</item>
/// </list>
/// 
/// <para><strong>Supported Matter Activities:</strong></para>
/// <list type="bullet">
/// <item><strong>CREATED:</strong> Matter created by user</item>
/// <item><strong>ARCHIVED:</strong> Matter archived by user</item>
/// <item><strong>DELETED:</strong> Matter deleted by user</item>
/// <item><strong>RESTORED:</strong> Matter restored by user</item>
/// <item><strong>UNARCHIVED:</strong> Matter unarchived by user</item>
/// <item><strong>VIEWED:</strong> Matter viewed by user</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Activity Lists:</strong> Matter activity browsing and audit trail displays</item>
/// <item><strong>Audit Reports:</strong> Lightweight activity reporting and compliance documentation</item>
/// <item><strong>API Responses:</strong> Efficient activity data transfer in REST API responses</item>
/// <item><strong>Activity Filtering:</strong> Activity search and filtering operations</item>
/// <item><strong>Performance-Critical Operations:</strong> Bulk activity processing with minimal memory footprint</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter Activity Tracking:</strong> Essential audit trail for legal matter management</item>
/// <item><strong>Professional Accountability:</strong> User attribution for matter operations</item>
/// <item><strong>Compliance Reporting:</strong> Minimal data for regulatory compliance and audit requirements</item>
/// <item><strong>Practice Efficiency:</strong> Optimized for rapid activity tracking and professional workflows</item>
/// </list>
/// 
/// <para><strong>Performance Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Memory Footprint:</strong> Only essential properties and minimal navigation data</item>
/// <item><strong>Fast Serialization:</strong> Quick JSON serialization/deserialization for API operations</item>
/// <item><strong>Database Efficiency:</strong> Optimal for database projections and bulk activity operations</item>
/// <item><strong>UI Responsiveness:</strong> Fast loading in activity lists and audit trail displays</item>
/// </list>
/// 
/// <para><strong>When to Use vs Other Activity DTOs:</strong></para>
/// <list type="bullet">
/// <item><strong>Use MatterActivityUserMinimalDto:</strong> For activity listings, audit reports, and performance-critical scenarios</item>
/// <item><strong>Use MatterActivityUserDto:</strong> For complete activity data with comprehensive relationships</item>
/// <item><strong>Use MatterDocumentActivityUserMinimalDto:</strong> For document-specific activity tracking within matter context</item>
/// </list>
/// 
/// <para><strong>Audit Trail Integration:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter-Activity Linkage:</strong> Links matter lifecycle activities to specific matters</item>
/// <item><strong>User Attribution:</strong> Maintains professional accountability for all matter operations</item>
/// <item><strong>Activity Classification:</strong> Clear categorization of matter operations</item>
/// <item><strong>Temporal Tracking:</strong> Precise timestamp tracking for legal compliance and professional standards</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a minimal matter activity audit entry
/// var activityMinimal = new MatterActivityUserMinimalDto
/// {
///     MatterId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
///     Matter = new MatterMinimalDto 
///     { 
///         Id = Guid.Parse("60000000-0000-0000-0000-000000000001"),
///         Description = "Smith Family Trust"
///     },
///     MatterActivityId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
///     MatterActivity = new MatterActivityMinimalDto
///     {
///         Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
///         Activity = "CREATED"
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
/// var validationResults = MatterActivityUserMinimalDto.ValidateModel(activityMinimal);
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
public record MatterActivityUserMinimalDto : IValidatableObject, IEquatable<MatterActivityUserMinimalDto>
{
    #region Composite Primary Key Properties

    /// <summary>
    /// Gets the unique identifier for the matter associated with the activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the matter that is the subject 
    /// of the activity. It corresponds directly to <see cref="ADMS.API.Entities.MatterActivityUser.MatterId"/>
    /// and provides essential matter context for audit trail operations.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> First component of the four-part composite primary key</item>
    /// <item><strong>Matter Context:</strong> Identifies which matter the activity was performed on</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing Matter entity</item>
    /// <item><strong>Audit Trail Foundation:</strong> Essential for matter-based activity tracking</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Matter Operations:</strong> Links activities to specific legal matters</item>
    /// <item><strong>Professional Organization:</strong> Supports matter-based activity organization strategies</item>
    /// <item><strong>Client Attribution:</strong> Associates activities with appropriate clients or cases</item>
    /// <item><strong>Audit Context:</strong> Provides essential context for professional audit and compliance</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterActivityUser.MatterId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Setting matter context for activity
    /// var activityAudit = new MatterActivityUserMinimalDto
    /// {
    ///     MatterId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
    ///     MatterActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Professional reporting usage
    /// Console.WriteLine($"Matter activity in matter ID: {activityAudit.MatterId}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter ID is required for matter activity context.")]
    public required Guid MatterId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the specific matter activity type.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific type of matter 
    /// activity being performed. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterActivityUser.MatterActivityId"/> and provides essential 
    /// activity classification for audit trail operations.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Second component of the four-part composite primary key</item>
    /// <item><strong>Activity Classification:</strong> Identifies the type of activity being performed</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing MatterActivity entity</item>
    /// <item><strong>Operation Type Tracking:</strong> Essential for categorizing matter operations</item>
    /// </list>
    /// 
    /// <para><strong>Activity Types (via MatterActivityMinimalDto):</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Matter created by user</item>
    /// <item><strong>ARCHIVED:</strong> Matter archived by user</item>
    /// <item><strong>DELETED:</strong> Matter deleted by user</item>
    /// <item><strong>RESTORED:</strong> Matter restored by user</item>
    /// <item><strong>UNARCHIVED:</strong> Matter unarchived by user</item>
    /// <item><strong>VIEWED:</strong> Matter viewed by user</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterActivityUser.MatterActivityId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Setting activity type
    /// var createdActivity = new MatterActivityUserMinimalDto
    /// {
    ///     MatterId = matterGuid,
    ///     MatterActivityId = createdActivityGuid, // References "CREATED" activity
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Professional activity tracking
    /// Console.WriteLine($"Matter activity type ID: {createdActivity.MatterActivityId}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter activity ID is required for activity classification.")]
    public required Guid MatterActivityId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the user performing the matter activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific user who performed 
    /// the matter activity. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterActivityUser.UserId"/> and provides essential user 
    /// attribution for professional accountability and audit trail completeness.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Third component of the four-part composite primary key</item>
    /// <item><strong>User Attribution:</strong> Identifies who performed the matter activity</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing User entity</item>
    /// <item><strong>Accountability Tracking:</strong> Essential for professional accountability and responsibility</item>
    /// </list>
    /// 
    /// <para><strong>Professional Accountability:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Responsibility:</strong> Establishes clear accountability for matter operations</item>
    /// <item><strong>Professional Standards:</strong> Supports professional practice responsibility requirements</item>
    /// <item><strong>Audit Trail Completeness:</strong> Ensures all matter activities have clear user attribution</item>
    /// <item><strong>Legal Compliance:</strong> Meets legal requirements for matter handling accountability</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterActivityUser.UserId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // User attribution for matter activity
    /// var userActivity = new MatterActivityUserMinimalDto
    /// {
    ///     MatterId = matterGuid,
    ///     MatterActivityId = activityGuid,
    ///     UserId = Guid.Parse("50000000-0000-0000-0000-000000000001"), // Robert Brown
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Professional accountability reporting
    /// Console.WriteLine($"Matter activity performed by user ID: {userActivity.UserId}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User ID is required for professional accountability in matter activities.")]
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the matter activity was performed.
    /// </summary>
    /// <remarks>
    /// This DateTime serves as the final component of the composite primary key and records the precise 
    /// moment when the matter activity was completed. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterActivityUser.CreatedAt"/> and provides essential 
    /// temporal tracking for audit trails, legal compliance, and professional accountability.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Fourth and final component of the composite primary key</item>
    /// <item><strong>Temporal Foundation:</strong> Establishes precise timing for matter activities</item>
    /// <item><strong>Audit Trail Chronology:</strong> Enables chronological sequencing of matter operations</item>
    /// <item><strong>Professional Standards:</strong> Meets professional practice temporal tracking requirements</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterActivityUser.CreatedAt"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Setting precise activity timestamp
    /// var timestampedActivity = new MatterActivityUserMinimalDto
    /// {
    ///     MatterId = matterGuid,
    ///     MatterActivityId = activityGuid,
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
    /// Gets the minimal matter information for the matter associated with the activity.
    /// </summary>
    /// <remarks>
    /// This property provides essential matter information for activity context while maintaining the minimal 
    /// footprint design. It corresponds to the <see cref="ADMS.API.Entities.MatterActivityUser.Matter"/> 
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
    /// <item><strong>Client Context:</strong> Provides client/matter context for activity reporting</item>
    /// <item><strong>Professional Display:</strong> Enables professional presentation of activity context</item>
    /// <item><strong>Audit Compliance:</strong> Sufficient information for compliance reporting requirements</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Activity with minimal matter context
    /// var activityWithMatter = new MatterActivityUserMinimalDto
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
    ///     Console.WriteLine($"Matter activity in: '{activityWithMatter.Matter.Description}'");
    /// }
    /// </code>
    /// </example>
    public MatterMinimalDto? Matter { get; init; }

    /// <summary>
    /// Gets the minimal matter activity information describing the type of activity performed.
    /// </summary>
    /// <remarks>
    /// This property provides essential activity classification information while maintaining the minimal 
    /// footprint design. It corresponds to the <see cref="ADMS.API.Entities.MatterActivityUser.MatterActivity"/> 
    /// navigation property but uses MatterActivityMinimalDto to optimize performance.
    /// 
    /// <para><strong>Activity Classification:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Operation Type:</strong> Clear identification of matter operation type (CREATED, ARCHIVED, DELETED, etc.)</item>
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
    /// var classifiedActivity = new MatterActivityUserMinimalDto
    /// {
    ///     MatterActivityId = activityGuid,
    ///     MatterActivity = new MatterActivityMinimalDto
    ///     {
    ///         Id = activityGuid,
    ///         Activity = "CREATED"
    ///     },
    ///     // ... other properties
    /// };
    /// 
    /// // Professional activity reporting
    /// if (classifiedActivity.MatterActivity != null)
    /// {
    ///     var activityType = classifiedActivity.MatterActivity.Activity;
    ///     Console.WriteLine($"Matter operation: {activityType}");
    /// }
    /// </code>
    /// </example>
    public MatterActivityMinimalDto? MatterActivity { get; init; }

    /// <summary>
    /// Gets the minimal user information for the user who performed the matter activity.
    /// </summary>
    /// <remarks>
    /// This property provides essential user information for professional accountability while maintaining 
    /// the minimal footprint design. It corresponds to the <see cref="ADMS.API.Entities.MatterActivityUser.User"/> 
    /// navigation property but uses UserMinimalDto to optimize performance and memory usage.
    /// 
    /// <para><strong>Professional Accountability:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Attribution:</strong> Clear professional attribution for matter activities</item>
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
    /// var userAttributedActivity = new MatterActivityUserMinimalDto
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
    /// var activity = new MatterActivityUserMinimalDto 
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
    /// This computed property provides a professional summary of the matter activity including key 
    /// participants and operation details, optimized for audit trail reports and professional communication.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activitySummary = activity.ActivitySummary;
    /// // Returns: "CREATED by Robert Brown in Smith Family Trust"
    /// 
    /// Console.WriteLine($"Activity Summary: {activitySummary}");
    /// </code>
    /// </example>
    public string ActivitySummary =>
        $"{MatterActivity?.Activity ?? "ACTIVITY"} " +
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
            ActivityType = MatterActivity?.Activity ?? "UNKNOWN",
            ActivityAgeDays,
            IsRecentActivity
        },
        ParticipantInfo = new
        {
            MatterDescription = Matter?.Description ?? "Unknown Matter",
            UserName = User?.Name ?? "Unknown User",
            UserId,
            MatterId
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
            HasCompleteInformation = Matter != null && MatterActivity != null && User != null,
            RequiredFieldsPresent = MatterId != Guid.Empty && MatterActivityId != Guid.Empty && UserId != Guid.Empty
        }
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="MatterActivityUserMinimalDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using centralized validation helpers for consistency with entity 
    /// validation rules while optimized for minimal DTO requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterActivityUserMinimalDto 
    /// { 
    ///     MatterId = Guid.Empty, // Invalid
    ///     MatterActivityId = Guid.Empty, // Invalid
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

        foreach (var result in ValidateMatterActivity())
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
    /// Ensures all four GUID components of the composite primary key are valid non-empty GUIDs, 
    /// which is essential for audit trail integrity and entity relationship consistency.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateGuids()
    {
        if (MatterId == Guid.Empty)
            yield return new ValidationResult(
                "Matter ID must be a valid non-empty GUID for audit trail context.",
                [nameof(MatterId)]);

        if (MatterActivityId == Guid.Empty)
            yield return new ValidationResult(
                "Matter activity ID must be a valid non-empty GUID for operation classification.",
                [nameof(MatterActivityId)]);

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
    /// Validates the <see cref="MatterActivity"/> navigation property using centralized validation.
    /// </summary>
    /// <returns>A collection of validation results for activity validation.</returns>
    /// <remarks>
    /// Validates the minimal activity information when provided, ensuring proper activity classification.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateMatterActivity()
    {
        if (MatterActivity is not IValidatableObject validatable) yield break;
        var context = new ValidationContext(MatterActivity);
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

        if (MatterActivity != null && MatterActivity.Id != MatterActivityId)
            yield return new ValidationResult(
                "MatterActivity.Id does not match MatterActivityId - referential integrity violation.",
                [nameof(MatterActivity), nameof(MatterActivityId)]);

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
    /// Uses ADMS.API.Common.MatterActivityValidationHelper.IsValidDate for comprehensive temporal 
    /// validation, ensuring the timestamp meets professional standards.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateCreatedAt()
    {
        if (CreatedAt > DateTime.UtcNow.AddMinutes(1))
            yield return new ValidationResult(
                "CreatedAt cannot be in the future for audit trail integrity.",
                [nameof(CreatedAt)]);

        if (CreatedAt < new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            yield return new ValidationResult(
                "CreatedAt is unreasonably far in the past for a matter activity.",
                [nameof(CreatedAt)]);
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="MatterActivityUserMinimalDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The MatterActivityUserMinimalDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate minimal activity DTOs 
    /// without requiring a ValidationContext.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterActivityUserMinimalDto 
    /// { 
    ///     MatterId = matterGuid,
    ///     MatterActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// var results = MatterActivityUserMinimalDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Activity validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterActivityUserMinimalDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterActivityUserMinimalDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a MatterActivityUserMinimalDto from ADMS.API.Entities.MatterActivityUser entity with validation.
    /// </summary>
    /// <param name="entity">The MatterActivityUser entity to convert. Cannot be null.</param>
    /// <param name="includeMinimalNavigation">Whether to include minimal navigation properties.</param>
    /// <returns>A valid MatterActivityUserMinimalDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create minimal activity DTOs from entities.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entity = new ADMS.API.Entities.MatterActivityUser 
    /// { 
    ///     MatterId = matterGuid,
    ///     MatterActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// var dto = MatterActivityUserMinimalDto.FromEntity(entity, includeMinimalNavigation: true);
    /// </code>
    /// </example>
    public static MatterActivityUserMinimalDto FromEntity([NotNull] Entities.MatterActivityUser entity, bool includeMinimalNavigation = false)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var dto = new MatterActivityUserMinimalDto
        {
            MatterId = entity.MatterId,
            MatterActivityId = entity.MatterActivityId,
            UserId = entity.UserId,
            CreatedAt = entity.CreatedAt
        };

        // Note: In practice, navigation properties would be mapped using a mapping framework
        // when includeMinimalNavigation is true

        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterActivityUserMinimalDto from entity: {errorMessages}");

    }

    /// <summary>
    /// Creates multiple MatterActivityUserMinimalDto instances from a collection of entities.
    /// </summary>
    /// <param name="entities">The collection of MatterActivityUser entities to convert. Cannot be null.</param>
    /// <param name="includeMinimalNavigation">Whether to include minimal navigation properties.</param>
    /// <returns>A collection of valid MatterActivityUserMinimalDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method is optimized for creating multiple minimal activity DTOs efficiently.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convert collection of activity entities
    /// var entities = await context.MatterActivityUsers.ToListAsync();
    /// var activityDtos = MatterActivityUserMinimalDto.FromEntities(entities, includeMinimalNavigation: false);
    /// 
    /// // Use in activity list display
    /// foreach (var activityDto in activityDtos)
    /// {
    ///     DisplayActivity(activityDto);
    /// }
    /// </code>
    /// </example>
    public static IList<MatterActivityUserMinimalDto> FromEntities([NotNull] IEnumerable<Entities.MatterActivityUser> entities, bool includeMinimalNavigation = false)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        var result = new List<MatterActivityUserMinimalDto>();

        foreach (var entity in entities)
        {
            try
            {
                var dto = FromEntity(entity, includeMinimalNavigation);
                result.Add(dto);
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                // Log invalid entity but continue processing others
                Console.WriteLine($"Warning: Skipped invalid activity entity: {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a matter activity audit entry with specified parameters and comprehensive validation.
    /// </summary>
    /// <param name="matterId">The matter ID.</param>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="userId">The user performing the activity.</param>
    /// <param name="timestamp">Optional timestamp (defaults to current UTC time).</param>
    /// <returns>A valid MatterActivityUserMinimalDto instance.</returns>
    /// <exception cref="ArgumentException">Thrown when any GUID parameter is empty.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a convenient way to create activity audit entries with all required
    /// parameters while ensuring validation compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create activity audit entry
    /// var activityAudit = MatterActivityUserMinimalDto.CreateActivityAudit(
    ///     matterGuid,
    ///     createdActivityGuid,
    ///     userGuid);
    /// 
    /// // Save to audit system
    /// await auditService.RecordActivityAsync(activityAudit);
    /// </code>
    /// </example>
    public static MatterActivityUserMinimalDto CreateActivityAudit(
        Guid matterId,
        Guid activityId,
        Guid userId,
        DateTime? timestamp = null)
    {
        if (matterId == Guid.Empty)
            throw new ArgumentException("Matter ID cannot be empty.", nameof(matterId));
        if (activityId == Guid.Empty)
            throw new ArgumentException("Activity ID cannot be empty.", nameof(activityId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        var dto = new MatterActivityUserMinimalDto
        {
            MatterId = matterId,
            MatterActivityId = activityId,
            UserId = userId,
            CreatedAt = timestamp ?? DateTime.UtcNow
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid activity audit entry: {errorMessages}");

    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this activity represents a matter creation operation.
    /// </summary>
    /// <returns>true if this represents a CREATED operation; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify creation operations for activity analysis and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsCreationOperation())
    /// {
    ///     Console.WriteLine($"Matter was created: {activity.Matter?.Description}");
    /// }
    /// </code>
    /// </example>
    public bool IsCreationOperation() =>
        string.Equals(MatterActivity?.Activity, "CREATED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a matter lifecycle operation.
    /// </summary>
    /// <returns>true if this represents an ARCHIVED, DELETED, or RESTORED operation; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify lifecycle operations for activity analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsLifecycleOperation())
    /// {
    ///     Console.WriteLine($"Matter lifecycle change: {activity.MatterActivity?.Activity}");
    /// }
    /// </code>
    /// </example>
    public bool IsLifecycleOperation()
    {
        var activityType = MatterActivity?.Activity;
        return string.Equals(activityType, "ARCHIVED", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(activityType, "DELETED", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(activityType, "RESTORED", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(activityType, "UNARCHIVED", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether this activity represents a viewing operation.
    /// </summary>
    /// <returns>true if this represents a VIEWED operation; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify viewing operations for activity analysis and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsViewingOperation())
    /// {
    ///     Console.WriteLine($"Matter was viewed: {activity.Matter?.Description}");
    /// }
    /// </code>
    /// </example>
    public bool IsViewingOperation() =>
        string.Equals(MatterActivity?.Activity, "VIEWED", StringComparison.OrdinalIgnoreCase);

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
            ["ActivityType"] = MatterActivity?.Activity ?? "UNKNOWN",
            ["IsCreationOperation"] = IsCreationOperation(),
            ["IsLifecycleOperation"] = IsLifecycleOperation(),
            ["IsViewingOperation"] = IsViewingOperation(),
            ["MatterContext"] = Matter?.Description ?? "Unknown Matter",
            ["UserName"] = User?.Name ?? "Unknown User",
            ["ActivityDate"] = CreatedAt,
            ["LocalActivityDate"] = LocalCreatedAtDateString,
            ["ActivityAge"] = ActivityAgeDays,
            ["IsRecent"] = IsRecentActivity,
            ["ActivitySummary"] = ActivitySummary,
            ["HasCompleteInformation"] = Matter != null && MatterActivity != null && User != null
        }.ToImmutableDictionary();
    }

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterActivityUserMinimalDto is equal to the current MatterActivityUserMinimalDto.
    /// </summary>
    /// <param name="other">The MatterActivityUserMinimalDto to compare with the current MatterActivityUserMinimalDto.</param>
    /// <returns>true if the specified MatterActivityUserMinimalDto is equal to the current MatterActivityUserMinimalDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all composite key properties that uniquely identify an activity audit entry.
    /// </remarks>
    public virtual bool Equals(MatterActivityUserMinimalDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return MatterId.Equals(other.MatterId) &&
               MatterActivityId.Equals(other.MatterActivityId) &&
               UserId.Equals(other.UserId) &&
               CreatedAt.Equals(other.CreatedAt);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterActivityUserMinimalDto.</returns>
    /// <remarks>
    /// The hash code is based on all composite key properties to ensure consistent hashing behavior.
    /// </remarks>
    public override int GetHashCode()
    {
        return HashCode.Combine(MatterId, MatterActivityId, UserId, CreatedAt);
    }

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterActivityUserMinimalDto.
    /// </summary>
    /// <returns>A string that represents the current MatterActivityUserMinimalDto.</returns>
    /// <remarks>
    /// The string representation includes key activity information for identification and logging purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterActivityUserMinimalDto 
    /// { 
    ///     MatterId = matterGuid,
    ///     MatterActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "Matter Activity: CREATED on Matter (60000000-...) by User (50000000-...) at 2024-01-15 14:30:45"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Matter Activity: {MatterActivity?.Activity ?? "ACTIVITY"} on Matter ({MatterId}) by User ({UserId}) at {CreatedAt:yyyy-MM-dd HH:mm:ss}";

    #endregion String Representation
}