using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing matter document activities with complete audit trail relationships for document transfer operations.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of matter document activities within the ADMS legal 
/// document management system, corresponding to <see cref="ADMS.API.Entities.MatterDocumentActivity"/>. 
/// It provides comprehensive activity classification and includes all user association collections for 
/// complete audit trail management and document transfer operation tracking.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Activity Classification:</strong> Identifies and categorizes document transfer activities (MOVE/COPY)</item>
/// <item><strong>Complete Audit Integration:</strong> Includes all user association collections for comprehensive audit trails</item>
/// <item><strong>Professional Validation:</strong> Uses ADMS.API.Common.MatterDocumentActivityValidationHelper for data integrity</item>
/// <item><strong>Immutable Design:</strong> Record type with init-only properties for thread-safe audit operations</item>
/// <item><strong>Bidirectional Tracking:</strong> Supports both source (FROM) and destination (TO) audit trail collections</item>
/// </list>
/// 
/// <para><strong>Entity Alignment:</strong></para>
/// This DTO mirrors all properties and relationships from ADMS.API.Entities.MatterDocumentActivity:
/// <list type="bullet">
/// <item><strong>Id:</strong> Unique identifier for the matter document activity</item>
/// <item><strong>Activity:</strong> Activity classification string (MOVED, COPIED, etc.)</item>
/// <item><strong>MatterDocumentActivityUsersFrom:</strong> Source-side user associations for document transfers</item>
/// <item><strong>MatterDocumentActivityUsersTo:</strong> Destination-side user associations for document transfers</item>
/// </list>
/// 
/// <para><strong>Supported Document Transfer Activities:</strong></para>
/// <list type="bullet">
/// <item><strong>MOVED:</strong> Document moved between matters - custody transfer operation</item>
/// <item><strong>COPIED:</strong> Document copied between matters - duplication operation</item>
/// <item><strong>SAVED:</strong> Document saved within matter context - storage operation</item>
/// <item><strong>DELETED:</strong> Document deleted from matter - removal operation</item>
/// <item><strong>RESTORED:</strong> Document restored to matter - recovery operation</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Activity Management:</strong> Managing and configuring document activity types</item>
/// <item><strong>Audit Trail Analysis:</strong> Complete audit trail analysis with all user associations</item>
/// <item><strong>Transfer Operations:</strong> Document transfer operations between matters</item>
/// <item><strong>Compliance Reporting:</strong> Comprehensive compliance reporting with complete audit data</item>
/// <item><strong>Administrative Operations:</strong> Activity administration and system configuration</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Transfer Management:</strong> Essential support for legal document transfer operations</item>
/// <item><strong>Professional Accountability:</strong> Complete user attribution for all document activities</item>
/// <item><strong>Legal Compliance:</strong> Comprehensive audit trails for regulatory compliance</item>
/// <item><strong>Practice Efficiency:</strong> Optimized for professional document management workflows</item>
/// </list>
/// 
/// <para><strong>Bidirectional Audit System:</strong></para>
/// <list type="bullet">
/// <item><strong>Source-Side Tracking:</strong> MatterDocumentActivityUsersFrom tracks document transfer origins</item>
/// <item><strong>Destination-Side Tracking:</strong> MatterDocumentActivityUsersTo tracks document transfer destinations</item>
/// <item><strong>Complete Coverage:</strong> Together they provide complete bidirectional audit coverage</item>
/// <item><strong>Legal Compliance:</strong> Ensures every document movement is fully documented</item>
/// </list>
/// 
/// <para><strong>Data Integrity and Validation:</strong></para>
/// <list type="bullet">
/// <item><strong>Activity Classification:</strong> Comprehensive validation of activity types and format</item>
/// <item><strong>Collection Validation:</strong> Deep validation of all user association collections</item>
/// <item><strong>Professional Standards:</strong> Enforces professional activity naming conventions</item>
/// <item><strong>Business Rule Compliance:</strong> Validates against allowed activity types and constraints</item>
/// </list>
/// 
/// <para><strong>When to Use vs Other Activity DTOs:</strong></para>
/// <list type="bullet">
/// <item><strong>Use MatterDocumentActivityDto:</strong> For complete activity management with full audit relationships</item>
/// <item><strong>Use MatterDocumentActivityMinimalDto:</strong> For lightweight activity selection and classification</item>
/// <item><strong>Use UserFromDto/ToDto:</strong> For specific directional transfer audit operations</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive matter document activity
/// var moveActivity = new MatterDocumentActivityDto
/// {
///     Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
///     Activity = "MOVED",
///     MatterDocumentActivityUsersFrom = new List&lt;MatterDocumentActivityUserFromDto&gt;
///     {
///         new MatterDocumentActivityUserFromDto
///         {
///             MatterId = sourceMatterId,
///             DocumentId = documentId,
///             MatterDocumentActivityId = Guid.Parse("40000000-0000-0000-0000-000000000002"),
///             UserId = userId,
///             CreatedAt = DateTime.UtcNow
///         }
///     },
///     MatterDocumentActivityUsersTo = new List&lt;MatterDocumentActivityUserToDto&gt;
///     {
///         new MatterDocumentActivityUserToDto
///         {
///             MatterId = destinationMatterId,
///             DocumentId = documentId,
///             MatterDocumentActivityId = Guid.Parse("40000000-0000-0000-0000-000000000002"),
///             UserId = userId,
///             CreatedAt = DateTime.UtcNow
///         }
///     }
/// };
/// 
/// // Comprehensive validation
/// var validationResults = MatterDocumentActivityDto.ValidateModel(moveActivity);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Activity Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Professional activity analysis
/// Console.WriteLine($"Activity: {moveActivity.DisplayActivity}");
/// Console.WriteLine($"Transfer Operations: {moveActivity.TotalTransferOperations}");
/// Console.WriteLine($"Active Users: {moveActivity.ActiveUserCount}");
/// </code>
/// </example>
public record MatterDocumentActivityDto : IValidatableObject, IEquatable<MatterDocumentActivityDto>
{
    #region Core Properties

    /// <summary>
    /// Gets the unique identifier for the matter document activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the matter document activity within the ADMS system.
    /// The ID corresponds directly to the <see cref="ADMS.API.Entities.MatterDocumentActivity.Id"/> property and is
    /// used for establishing relationships, activity references, and all system operations requiring precise activity
    /// identification.
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required Property:</strong> Always required for existing activity operations</item>
    /// <item><strong>Foreign Key Reference:</strong> Used as foreign key in all activity-related junction entities</item>
    /// <item><strong>Activity Classification:</strong> Links to specific activity types for document operations</item>
    /// <item><strong>API Operations:</strong> Primary identifier for REST API operations and activity references</item>
    /// <item><strong>Database Queries:</strong> Primary key for all activity-specific database operations</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivity.Id"/> exactly, ensuring consistency
    /// between entity and DTO representations for reliable activity identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard seeded activity IDs
    /// var moveActivityId = Guid.Parse("40000000-0000-0000-0000-000000000002"); // MOVED
    /// var copyActivityId = Guid.Parse("40000000-0000-0000-0000-000000000001"); // COPIED
    /// 
    /// // Using ID for activity references
    /// var transferOperations = auditTrail.Where(a => a.MatterDocumentActivityId == moveActivityId);
    /// 
    /// // API response with ID
    /// return Ok(new { Id = activity.Id, Activity = activity.Activity });
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter document activity ID is required.")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the activity classification string describing the type of document operation.
    /// </summary>
    /// <remarks>
    /// The activity string serves as the primary classification for document operations and must conform to 
    /// professional standards and validation rules. This field corresponds to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivity.Activity"/> and provides essential operation 
    /// type identification for audit trails and professional compliance.
    /// 
    /// <para><strong>Validation Rules (via ADMS.API.Common.MatterDocumentActivityValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null, empty, or whitespace</item>
    /// <item><strong>Length:</strong> Must not exceed maximum activity length constraint</item>
    /// <item><strong>Allowed Values:</strong> Must be one of the predefined allowed activity types</item>
    /// <item><strong>Format:</strong> Must contain at least one letter for professional readability</item>
    /// <item><strong>Professional Standards:</strong> Must follow professional activity naming conventions</item>
    /// </list>
    /// 
    /// <para><strong>Supported Activity Classifications:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> Document moved between matters - custody transfer operation</item>
    /// <item><strong>COPIED:</strong> Document copied between matters - duplication operation</item>
    /// <item><strong>SAVED:</strong> Document saved within matter context - storage operation</item>
    /// <item><strong>DELETED:</strong> Document deleted from matter - removal operation</item>
    /// <item><strong>RESTORED:</strong> Document restored to matter - recovery operation</item>
    /// </list>
    /// 
    /// <para><strong>Professional Activity Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Lifecycle:</strong> Represents different stages of document lifecycle management</item>
    /// <item><strong>Professional Operations:</strong> Aligns with professional legal document handling practices</item>
    /// <item><strong>Audit Classification:</strong> Provides clear categorization for audit trail analysis</item>
    /// <item><strong>Client Communication:</strong> Supports professional client communication about document operations</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivity.Activity"/> with identical validation
    /// rules and professional standards, ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional activity classifications
    /// var moveActivity = new MatterDocumentActivityDto 
    /// { 
    ///     Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
    ///     Activity = "MOVED" 
    /// };
    /// 
    /// var copyActivity = new MatterDocumentActivityDto 
    /// { 
    ///     Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
    ///     Activity = "COPIED" 
    /// };
    /// 
    /// // Activity validation
    /// var isValidActivity = MatterDocumentActivityValidationHelper.IsActivityAllowed(moveActivity.Activity);
    /// 
    /// // Professional display
    /// Console.WriteLine($"Document Operation: {moveActivity.DisplayActivity}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Activity classification is required.")]
    [StringLength(50, MinimumLength = 1,
        ErrorMessage = "Activity must be between 1 and 50 characters.")]
    public required string Activity { get; init; }

    #endregion Core Properties

    #region User Association Collections

    /// <summary>
    /// Gets the collection of "from" user associations for document transfer activities where this activity represents the source side.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.MatterDocumentActivity.MatterDocumentActivityUsersFrom"/> and
    /// maintains all source-side user associations for document transfer operations. This provides the complete audit trail
    /// for documents being transferred FROM source matters, essential for legal compliance and professional accountability.
    /// 
    /// <para><strong>Source-Side Audit Trail:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Origins:</strong> Tracks where documents are being transferred from</item>
    /// <item><strong>User Attribution:</strong> Complete user accountability for document transfer initiation</item>
    /// <item><strong>Professional Responsibility:</strong> Professional responsibility for document handling decisions</item>
    /// <item><strong>Legal Compliance:</strong> Regulatory compliance for document custody tracking</item>
    /// </list>
    /// 
    /// <para><strong>Transfer Operations Covered:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED Operations:</strong> Documents moved from source matters (custody transfer)</item>
    /// <item><strong>COPIED Operations:</strong> Documents copied from source matters (duplication)</item>
    /// <item><strong>Multi-Document Operations:</strong> Multiple documents transferred in batch operations</item>
    /// <item><strong>Cross-Matter Operations:</strong> Documents transferred across multiple matter contexts</item>
    /// </list>
    /// 
    /// <para><strong>DTO Composition:</strong></para>
    /// Contains <see cref="MatterDocumentActivityUserFromDto"/> instances that include:
    /// <list type="bullet">
    /// <item>Complete source matter information via MatterWithoutDocumentsDto</item>
    /// <item>Complete document information via DocumentWithoutRevisionsDto</item>
    /// <item>Complete user attribution via UserDto</item>
    /// <item>Precise temporal tracking via CreatedAt timestamps</item>
    /// </list>
    /// 
    /// <para><strong>Professional Practice Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Provenance:</strong> Maintains complete document provenance chains</item>
    /// <item><strong>Client Communication:</strong> Enables client communication about document reorganization</item>
    /// <item><strong>Practice Management:</strong> Supports practice management and workflow analysis</item>
    /// <item><strong>Quality Control:</strong> Provides oversight capabilities for document operations</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Collection Validation:</strong> Each item validated using MatterDocumentActivityUserFromDto.Validate</item>
    /// <item><strong>Referential Integrity:</strong> All items must reference this activity's ID</item>
    /// <item><strong>Business Rule Compliance:</strong> Must comply with transfer operation business rules</item>
    /// <item><strong>Professional Standards:</strong> Must meet professional document handling standards</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Analyzing source-side transfer operations
    /// foreach (var fromTransfer in activity.MatterDocumentActivityUsersFrom)
    /// {
    ///     Console.WriteLine($"Document '{fromTransfer.Document?.FileName}' " +
    ///                      $"transferred FROM '{fromTransfer.Matter?.Description}' " +
    ///                      $"by {fromTransfer.User?.Name} at {fromTransfer.CreatedAt}");
    /// }
    /// 
    /// // Finding recent outbound transfers
    /// var recentOutbound = activity.MatterDocumentActivityUsersFrom
    ///     .Where(f => f.CreatedAt >= DateTime.UtcNow.AddDays(-7))
    ///     .OrderByDescending(f => f.CreatedAt);
    /// 
    /// // Transfer pattern analysis
    /// var transfersByMatter = activity.MatterDocumentActivityUsersFrom
    ///     .GroupBy(f => f.Matter?.Description)
    ///     .Select(g => new { Matter = g.Key, Count = g.Count() });
    /// </code>
    /// </example>
    public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; init; } = [];

    /// <summary>
    /// Gets the collection of "to" user associations for document transfer activities where this activity represents the destination side.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.MatterDocumentActivity.MatterDocumentActivityUsersTo"/> and
    /// maintains all destination-side user associations for document transfer operations. This provides the complete audit trail
    /// for documents being transferred TO destination matters, completing the bidirectional audit system essential for legal compliance.
    /// 
    /// <para><strong>Destination-Side Audit Trail:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Destinations:</strong> Tracks where documents are being transferred to</item>
    /// <item><strong>User Attribution:</strong> Complete user accountability for document receipt and organization</item>
    /// <item><strong>Professional Responsibility:</strong> Professional responsibility for document custody acceptance</item>
    /// <item><strong>Legal Compliance:</strong> Regulatory compliance for document custody tracking</item>
    /// </list>
    /// 
    /// <para><strong>Transfer Operations Covered:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED Operations:</strong> Documents moved to destination matters (custody reception)</item>
    /// <item><strong>COPIED Operations:</strong> Documents copied to destination matters (duplication reception)</item>
    /// <item><strong>Multi-Document Operations:</strong> Multiple documents received in batch operations</item>
    /// <item><strong>Cross-Matter Operations:</strong> Documents received across multiple matter contexts</item>
    /// </list>
    /// 
    /// <para><strong>DTO Composition:</strong></para>
    /// Contains <see cref="MatterDocumentActivityUserToDto"/> instances that include:
    /// <list type="bullet">
    /// <item>Complete destination matter information via MatterWithoutDocumentsDto</item>
    /// <item>Complete document information via DocumentWithoutRevisionsDto</item>
    /// <item>Complete user attribution via UserDto</item>
    /// <item>Precise temporal tracking via CreatedAt timestamps</item>
    /// </list>
    /// 
    /// <para><strong>Bidirectional Audit System:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Complete Coverage:</strong> Together with MatterDocumentActivityUsersFrom provides complete audit coverage</item>
    /// <item><strong>Legal Compliance:</strong> Ensures every document movement is fully documented and traceable</item>
    /// <item><strong>Professional Accountability:</strong> Complete user attribution for both source and destination operations</item>
    /// <item><strong>Audit Trail Integrity:</strong> Maintains comprehensive audit trail integrity across all transfers</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Collection Validation:</strong> Each item validated using MatterDocumentActivityUserToDto.Validate</item>
    /// <item><strong>Referential Integrity:</strong> All items must reference this activity's ID</item>
    /// <item><strong>Business Rule Compliance:</strong> Must comply with transfer operation business rules</item>
    /// <item><strong>Professional Standards:</strong> Must meet professional document handling standards</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Analyzing destination-side transfer operations
    /// foreach (var toTransfer in activity.MatterDocumentActivityUsersTo)
    /// {
    ///     Console.WriteLine($"Document '{toTransfer.Document?.FileName}' " +
    ///                      $"transferred TO '{toTransfer.Matter?.Description}' " +
    ///                      $"by {toTransfer.User?.Name} at {toTransfer.CreatedAt}");
    /// }
    /// 
    /// // Finding recent inbound transfers
    /// var recentInbound = activity.MatterDocumentActivityUsersTo
    ///     .Where(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-7))
    ///     .OrderByDescending(t => t.CreatedAt);
    /// 
    /// // Destination analysis
    /// var transfersByDestination = activity.MatterDocumentActivityUsersTo
    ///     .GroupBy(t => t.Matter?.Description)
    ///     .Select(g => new { Matter = g.Key, Count = g.Count() });
    /// </code>
    /// </example>
    public ICollection<MatterDocumentActivityUserToDto> MatterDocumentActivityUsersTo { get; init; } = [];

    #endregion User Association Collections

    #region Computed Properties

    /// <summary>
    /// Gets the normalized activity string for consistent comparison and analysis.
    /// </summary>
    /// <remarks>
    /// This computed property provides a normalized version of the activity string using
    /// consistent formatting rules for comparison, analysis, and professional presentation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity1 = new MatterDocumentActivityDto { Activity = "moved" };
    /// var activity2 = new MatterDocumentActivityDto { Activity = "MOVED" };
    /// 
    /// // Both will have the same normalized activity: "MOVED"
    /// bool areEquivalent = activity1.NormalizedActivity == activity2.NormalizedActivity; // true
    /// </code>
    /// </example>
    public string NormalizedActivity => Activity.Trim().ToUpperInvariant();

    /// <summary>
    /// Gets the professional display representation of the activity for client communication and UI display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a user-friendly formatted representation of the activity
    /// optimized for professional interfaces, client communications, and audit trail presentation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new MatterDocumentActivityDto { Activity = "MOVED" };
    /// 
    /// // Professional display
    /// Console.WriteLine($"Document Operation: {activity.DisplayActivity}");
    /// // Output: "Document Operation: Moved"
    /// </code>
    /// </example>
    public string DisplayActivity => Activity switch
    {
        "MOVED" => "Moved",
        "COPIED" => "Copied",
        "SAVED" => "Saved",
        "DELETED" => "Deleted",
        "RESTORED" => "Restored",
        _ => Activity.ToLowerInvariant().Trim() switch
        {
            var act when !string.IsNullOrEmpty(act) => char.ToUpperInvariant(act[0]) + act[1..],
            _ => Activity
        }
    };

    /// <summary>
    /// Gets the activity type category for classification and analysis purposes.
    /// </summary>
    /// <remarks>
    /// This computed property categorizes activities into logical groups for analysis,
    /// reporting, and business rule application.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new MatterDocumentActivityDto { Activity = "MOVED" };
    /// 
    /// Console.WriteLine($"Activity Category: {activity.ActivityType}");
    /// // Output: "Activity Category: Transfer"
    /// </code>
    /// </example>
    public string ActivityType => NormalizedActivity switch
    {
        "MOVED" or "COPIED" => "Transfer",
        "SAVED" => "Storage",
        "DELETED" => "Removal",
        "RESTORED" => "Recovery",
        _ => "Other"
    };

    /// <summary>
    /// Gets a value indicating whether this activity represents a transfer operation (MOVE or COPY).
    /// </summary>
    /// <remarks>
    /// This computed property helps identify transfer operations, which require bidirectional
    /// audit trail tracking and have different implications for document custody.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsTransferActivity)
    /// {
    ///     Console.WriteLine("This activity involves document transfer between matters");
    ///     Console.WriteLine($"Source operations: {activity.MatterDocumentActivityUsersFrom.Count}");
    ///     Console.WriteLine($"Destination operations: {activity.MatterDocumentActivityUsersTo.Count}");
    /// }
    /// </code>
    /// </example>
    public bool IsTransferActivity => ActivityType == "Transfer";

    /// <summary>
    /// Gets a value indicating whether this activity represents a destructive operation.
    /// </summary>
    /// <remarks>
    /// This computed property helps identify destructive operations that affect document
    /// availability and require special handling in audit trails.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.IsDestructiveActivity)
    /// {
    ///     Console.WriteLine("Warning: This activity affects document availability");
    /// }
    /// </code>
    /// </example>
    public bool IsDestructiveActivity => ActivityType == "Removal";

    /// <summary>
    /// Gets the total number of transfer operations (from + to) associated with this activity.
    /// </summary>
    /// <remarks>
    /// This computed property provides a quick overview of activity usage levels,
    /// useful for activity analysis and system usage metrics.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Activity '{activity.Activity}' has {activity.TotalTransferOperations} transfer operations");
    /// </code>
    /// </example>
    public int TotalTransferOperations => MatterDocumentActivityUsersFrom.Count + MatterDocumentActivityUsersTo.Count;

    /// <summary>
    /// Gets the count of unique users who have performed operations with this activity.
    /// </summary>
    /// <remarks>
    /// This computed property calculates the number of distinct users involved in operations
    /// using this activity, useful for user engagement and activity analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Activity '{activity.Activity}' involves {activity.ActiveUserCount} unique users");
    /// </code>
    /// </example>
    public int ActiveUserCount => MatterDocumentActivityUsersFrom
        .Select(f => f.UserId)
        .Union(MatterDocumentActivityUsersTo.Select(t => t.UserId))
        .Distinct()
        .Count();

    /// <summary>
    /// Gets a value indicating whether this activity has any recorded operations.
    /// </summary>
    /// <remarks>
    /// This property determines if the activity has been used in any transfer operations,
    /// useful for activity management and cleanup operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.HasOperations)
    /// {
    ///     // Activity has audit trail data - consider implications before removal
    ///     Console.WriteLine($"Activity '{activity.Activity}' has operational history");
    /// }
    /// </code>
    /// </example>
    public bool HasOperations => TotalTransferOperations > 0;

    /// <summary>
    /// Gets the display text suitable for UI controls and activity identification.
    /// </summary>
    /// <remarks>
    /// Provides a consistent format for displaying activity information in UI elements,
    /// optimized for professional presentation and user comprehension.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new MatterDocumentActivityDto 
    /// { 
    ///     Id = activityGuid,
    ///     Activity = "MOVED" 
    /// };
    /// var displayText = activity.DisplayText; // Returns "Moved"
    /// 
    /// // UI usage
    /// activityDropdown.Items.Add(new ListItem(activity.DisplayText, activity.Id.ToString()));
    /// </code>
    /// </example>
    public string DisplayText => DisplayActivity;

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
            Id,
            Activity,
            NormalizedActivity,
            DisplayActivity,
            ActivityType,
            DisplayText
        },
        ClassificationInfo = new
        {
            IsTransferActivity,
            IsDestructiveActivity,
            HasOperations
        },
        UsageMetrics = new
        {
            TotalTransferOperations,
            SourceOperations = MatterDocumentActivityUsersFrom.Count,
            DestinationOperations = MatterDocumentActivityUsersTo.Count,
            ActiveUserCount,
            HasOperations
        },
        ValidationInfo = new
        {
            IsValid = Id != Guid.Empty && !string.IsNullOrWhiteSpace(Activity),
            HasValidId = Id != Guid.Empty,
            HasValidActivity = !string.IsNullOrWhiteSpace(Activity),
            IsAllowedActivity = MatterDocumentActivityValidationHelper.IsActivityAllowed(Activity ?? string.Empty)
        }
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="MatterDocumentActivityDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using the ADMS.API.Common.MatterDocumentActivityValidationHelper for consistency 
    /// with entity validation rules. This ensures the DTO maintains the same validation standards as the corresponding 
    /// ADMS.API.Entities.MatterDocumentActivity entity while enforcing complete audit trail requirements.
    /// 
    /// <para><strong>Comprehensive Validation Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Core Property Validation:</strong> ID and Activity validation using centralized helpers</item>
    /// <item><strong>Collection Validation:</strong> Deep validation of both FROM and TO user association collections</item>
    /// <item><strong>Referential Integrity:</strong> Ensures all collection items reference this activity correctly</item>
    /// <item><strong>Business Rule Compliance:</strong> Validates business rules for transfer operations</item>
    /// <item><strong>Professional Standards:</strong> Enforces professional activity standards</item>
    /// </list>
    /// 
    /// <para><strong>Professional Standards Integration:</strong></para>
    /// Uses centralized validation helpers (MatterDocumentActivityValidationHelper, DtoValidationHelper) 
    /// to ensure consistency across all activity validation in the system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityDto 
    /// { 
    ///     Id = Guid.Empty, // Invalid
    ///     Activity = "INVALID_ACTIVITY", // Invalid
    ///     MatterDocumentActivityUsersFrom = new List&lt;MatterDocumentActivityUserFromDto&gt; { /* invalid items */ }
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
        // Validate core properties
        foreach (var result in ValidateId())
            yield return result;

        foreach (var result in ValidateActivity())
            yield return result;

        // Validate user association collections
        foreach (var result in ValidateFromCollection())
            yield return result;

        foreach (var result in ValidateToCollection())
            yield return result;

        // Validate business rules
        foreach (var result in ValidateBusinessRules())
            yield return result;
    }

    /// <summary>
    /// Validates the <see cref="Id"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the activity ID.</returns>
    /// <remarks>
    /// Ensures the activity ID is a valid, non-empty GUID which is essential for activity identification
    /// and referential integrity.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateId()
    {
        if (Id == Guid.Empty)
            yield return new ValidationResult(
                "Activity ID must be a valid non-empty GUID for activity identification.",
                [nameof(Id)]);
    }

    /// <summary>
    /// Validates the <see cref="Activity"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the activity classification.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.MatterDocumentActivityValidationHelper for comprehensive activity validation
    /// including allowed values, format requirements, and professional standards.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateActivity()
    {
        // Basic null/empty validation
        if (string.IsNullOrWhiteSpace(Activity))
        {
            yield return new ValidationResult(
                "Activity classification is required for document operation identification.",
                [nameof(Activity)]);
            yield break;
        }

        // Length validation
        if (Activity.Length > MatterDocumentActivityValidationHelper.MaxActivityLength)
            yield return new ValidationResult(
                $"Activity classification cannot exceed {MatterDocumentActivityValidationHelper.MaxActivityLength} characters.",
                [nameof(Activity)]);

        // Allowed activity validation
        if (!MatterDocumentActivityValidationHelper.IsActivityAllowed(Activity))
            yield return new ValidationResult(
                $"Activity '{Activity}' is not allowed. Allowed activities: {string.Join(", ", MatterDocumentActivityValidationHelper.AllowedActivitiesList)}",
                [nameof(Activity)]);

        // Professional format validation
        if (!Activity.Any(char.IsLetter))
            yield return new ValidationResult(
                "Activity classification must contain at least one letter for professional readability.",
                [nameof(Activity)]);
    }

    /// <summary>
    /// Validates the <see cref="MatterDocumentActivityUsersFrom"/> collection.
    /// </summary>
    /// <returns>A collection of validation results for the FROM collection.</returns>
    /// <remarks>
    /// Performs deep validation of the source-side user association collection including
    /// individual item validation and referential integrity checks.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateFromCollection()
    {
        foreach (var result in ValidateCollection(MatterDocumentActivityUsersFrom, nameof(MatterDocumentActivityUsersFrom)))
            yield return result;

        // Validate referential integrity for FROM collection
        foreach (var (item, index) in MatterDocumentActivityUsersFrom.Select((item, i) => (item, i)))
        {
            if (item.MatterDocumentActivityId != Id)
                yield return new ValidationResult(
                    $"FROM collection item [{index}] references incorrect activity ID. Expected: {Id}, Found: {item.MatterDocumentActivityId}",
                    [nameof(MatterDocumentActivityUsersFrom)]);
        }
    }

    /// <summary>
    /// Validates the <see cref="MatterDocumentActivityUsersTo"/> collection.
    /// </summary>
    /// <returns>A collection of validation results for the TO collection.</returns>
    /// <remarks>
    /// Performs deep validation of the destination-side user association collection including
    /// individual item validation and referential integrity checks.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateToCollection()
    {
        foreach (var result in ValidateCollection(MatterDocumentActivityUsersTo, nameof(MatterDocumentActivityUsersTo)))
            yield return result;

        // Validate referential integrity for TO collection
        foreach (var (item, index) in MatterDocumentActivityUsersTo.Select((item, i) => (item, i)))
        {
            if (item.MatterDocumentActivityId != Id)
                yield return new ValidationResult(
                    $"TO collection item [{index}] references incorrect activity ID. Expected: {Id}, Found: {item.MatterDocumentActivityId}",
                    [nameof(MatterDocumentActivityUsersTo)]);
        }
    }

    /// <summary>
    /// Validates business rules for the activity and its collections.
    /// </summary>
    /// <returns>A collection of validation results for business rule validation.</returns>
    /// <remarks>
    /// Validates business rules such as ensuring transfer activities have appropriate
    /// collection usage patterns and professional practice requirements.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // For transfer activities, validate that at least one collection has entries if the other does
        if (IsTransferActivity)
        {
            var hasFrom = MatterDocumentActivityUsersFrom.Any();
            var hasTo = MatterDocumentActivityUsersTo.Any();

            if (hasFrom && !hasTo)
                yield return new ValidationResult(
                    "Transfer activities with source operations should typically have corresponding destination operations for complete audit trails.",
                    [nameof(MatterDocumentActivityUsersFrom), nameof(MatterDocumentActivityUsersTo)]);

            if (hasTo && !hasFrom)
                yield return new ValidationResult(
                    "Transfer activities with destination operations should typically have corresponding source operations for complete audit trails.",
                    [nameof(MatterDocumentActivityUsersFrom), nameof(MatterDocumentActivityUsersTo)]);
        }

        // Validate collection consistency for non-transfer activities
        if (!IsTransferActivity && (MatterDocumentActivityUsersFrom.Any() || MatterDocumentActivityUsersTo.Any()))
        {
            yield return new ValidationResult(
                $"Non-transfer activity '{Activity}' should not have transfer-specific user associations.",
                [nameof(MatterDocumentActivityUsersFrom), nameof(MatterDocumentActivityUsersTo)]);
        }
    }

    /// <summary>
    /// Validates a collection of <see cref="IValidatableObject"/> items.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="propertyName">The property name for error reporting.</param>
    /// <returns>A collection of validation results.</returns>
    /// <remarks>
    /// Provides centralized collection validation for both FROM and TO user association collections,
    /// ensuring consistent validation across all collection types.
    /// </remarks>
    private static IEnumerable<ValidationResult> ValidateCollection<T>(ICollection<T> collection, string propertyName)
    {
        foreach (var (item, idx) in collection.Select((v, i) => (v, i)))
        {
            if (item is null)
            {
                yield return new ValidationResult($"{propertyName}[{idx}] is null.", [propertyName]);
            }
            else if (item is IValidatableObject validatable)
            {
                var context = new ValidationContext(item);
                foreach (var result in validatable.Validate(context))
                    yield return new ValidationResult($"{propertyName}[{idx}]: {result.ErrorMessage}", result.MemberNames);
            }
        }
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="MatterDocumentActivityDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The MatterDocumentActivityDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate MatterDocumentActivityDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance Validate method 
    /// but with null-safety and simplified usage.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     Activity = "MOVED"
    /// };
    /// 
    /// var results = MatterDocumentActivityDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Activity validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterDocumentActivityDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterDocumentActivityDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a MatterDocumentActivityDto from ADMS.API.Entities.MatterDocumentActivity entity with validation.
    /// </summary>
    /// <param name="entity">The MatterDocumentActivity entity to convert. Cannot be null.</param>
    /// <param name="includeUserAssociations">Whether to include user association collections.</param>
    /// <returns>A valid MatterDocumentActivityDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create MatterDocumentActivityDto instances from
    /// ADMS.API.Entities.MatterDocumentActivity entities with automatic validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity with complete information
    /// var entity = new ADMS.API.Entities.MatterDocumentActivity 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     Activity = "MOVED"
    /// };
    /// 
    /// var dto = MatterDocumentActivityDto.FromEntity(entity, includeUserAssociations: true);
    /// </code>
    /// </example>
    public static MatterDocumentActivityDto FromEntity([NotNull] Entities.MatterDocumentActivity entity, bool includeUserAssociations = false)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var dto = new MatterDocumentActivityDto
        {
            Id = entity.Id,
            Activity = entity.Activity,
            MatterDocumentActivityUsersFrom = includeUserAssociations
                ? entity.MatterDocumentActivityUsersFrom?.Select(e => MatterDocumentActivityUserFromDto.FromEntity(e)).ToList()
                : [],
            MatterDocumentActivityUsersTo = includeUserAssociations
                ? entity.MatterDocumentActivityUsersTo?.Select(e => MatterDocumentActivityUserToDto.FromEntity(e)).ToList<MatterDocumentActivityUserToDto>()
                : []
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (validationResults.Any())
        {
            var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
            throw new ValidationException($"Failed to create valid MatterDocumentActivityDto from entity: {errorMessages}");
        }

        return dto;
    }

    /// <summary>
    /// Creates multiple MatterDocumentActivityDto instances from a collection of entities.
    /// </summary>
    /// <param name="entities">The collection of MatterDocumentActivity entities to convert. Cannot be null.</param>
    /// <param name="includeUserAssociations">Whether to include user association collections.</param>
    /// <returns>A collection of valid MatterDocumentActivityDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method is optimized for creating multiple activity DTOs efficiently.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convert collection of activity entities
    /// var entities = await context.MatterDocumentActivities.ToListAsync();
    /// var activityDtos = MatterDocumentActivityDto.FromEntities(entities, includeUserAssociations: false);
    /// 
    /// // Process activities
    /// foreach (var activityDto in activityDtos)
    /// {
    ///     ProcessActivity(activityDto);
    /// }
    /// </code>
    /// </example>
    public static IList<MatterDocumentActivityDto> FromEntities([NotNull] IEnumerable<Entities.MatterDocumentActivity> entities, bool includeUserAssociations = false)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        var result = new List<MatterDocumentActivityDto>();

        foreach (var entity in entities)
        {
            try
            {
                var dto = FromEntity(entity, includeUserAssociations);
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
    /// Creates a new activity with specified parameters and comprehensive validation.
    /// </summary>
    /// <param name="activity">The activity classification string.</param>
    /// <param name="id">Optional ID (generates new GUID if not provided).</param>
    /// <returns>A valid MatterDocumentActivityDto instance.</returns>
    /// <exception cref="ArgumentException">Thrown when activity is invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a convenient way to create activity DTOs with validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create new activity
    /// var moveActivity = MatterDocumentActivityDto.CreateActivity("MOVED");
    /// var copyActivity = MatterDocumentActivityDto.CreateActivity("COPIED", specificGuid);
    /// 
    /// // Use in system operations
    /// await activityService.RegisterActivityAsync(moveActivity);
    /// </code>
    /// </example>
    public static MatterDocumentActivityDto CreateActivity([NotNull] string activity, Guid? id = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activity, nameof(activity));

        var dto = new MatterDocumentActivityDto
        {
            Id = id ?? Guid.NewGuid(),
            Activity = activity.Trim().ToUpperInvariant(),
            MatterDocumentActivityUsersFrom = [],
            MatterDocumentActivityUsersTo = []
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid activity: {errorMessages}");

    }

    /// <summary>
    /// Gets all allowed activities as MatterDocumentActivityDto instances.
    /// </summary>
    /// <returns>A collection of MatterDocumentActivityDto instances for all allowed activities.</returns>
    /// <remarks>
    /// This method provides a convenient way to get all allowed activities as DTOs for
    /// UI population, validation, and system operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Populate activity selection dropdown
    /// var allowedActivities = MatterDocumentActivityDto.GetAllowedActivities();
    /// foreach (var activity in allowedActivities)
    /// {
    ///     activityDropdown.Items.Add(new ListItem(activity.DisplayText, activity.Id.ToString()));
    /// }
    /// </code>
    /// </example>
    public static IList<MatterDocumentActivityDto> GetAllowedActivities()
    {
        var result = new List<MatterDocumentActivityDto>();

        foreach (var activity in MatterDocumentActivityValidationHelper.AllowedActivitiesList)
        {
            try
            {
                var dto = CreateActivity(activity.ToString());
                result.Add(dto);
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                // Log invalid activity but continue processing others
                Console.WriteLine($"Warning: Skipped invalid allowed activity '{activity}': {ex.Message}");
            }
        }

        return result;
    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this activity can be applied to documents in the specified state.
    /// </summary>
    /// <param name="isDeleted">Whether the document is currently deleted.</param>
    /// <param name="isCheckedOut">Whether the document is currently checked out.</param>
    /// <returns>true if the activity can be applied; otherwise, false.</returns>
    /// <remarks>
    /// This method validates business rules for activity application based on document state.
    /// </remarks>
    /// <example>
    /// <code>
    /// var moveActivity = MatterDocumentActivityDto.CreateActivity("MOVED");
    /// var deleteActivity = MatterDocumentActivityDto.CreateActivity("DELETED");
    /// 
    /// // Check if activities can be applied
    /// var canMoveDeleted = moveActivity.CanApplyToDocument(isDeleted: true, isCheckedOut: false); // false
    /// var canDeleteActive = deleteActivity.CanApplyToDocument(isDeleted: false, isCheckedOut: false); // true
    /// </code>
    /// </example>
    public bool CanApplyToDocument(bool isDeleted, bool isCheckedOut)
    {
        return NormalizedActivity switch
        {
            "MOVED" or "COPIED" => !isDeleted && !isCheckedOut, // Can't transfer deleted or checked out documents
            "SAVED" => !isDeleted, // Can't save deleted documents
            "DELETED" => !isDeleted, // Can't delete already deleted documents
            "RESTORED" => isDeleted, // Can only restore deleted documents
            _ => true // Unknown activities allowed by default
        };
    }

    /// <summary>
    /// Gets the professional description of what this activity does.
    /// </summary>
    /// <returns>A professional description of the activity operation.</returns>
    /// <remarks>
    /// This method provides human-readable descriptions for client communication and documentation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = MatterDocumentActivityDto.CreateActivity("MOVED");
    /// var description = activity.GetActivityDescription();
    /// // Returns: "Transfers document custody between matters, removing it from the source matter"
    /// </code>
    /// </example>
    public string GetActivityDescription()
    {
        return NormalizedActivity switch
        {
            "MOVED" => "Transfers document custody between matters, removing it from the source matter",
            "COPIED" => "Creates a duplicate of the document in another matter while retaining the original",
            "SAVED" => "Saves document within the matter context, creating or updating the document record",
            "DELETED" => "Removes the document from the matter, marking it as deleted but preserving audit history",
            "RESTORED" => "Restores a previously deleted document, making it available again in the matter",
            _ => $"Performs {DisplayActivity.ToLowerInvariant()} operation on the document"
        };
    }

    /// <summary>
    /// Gets activities that are compatible with this activity for sequential operations.
    /// </summary>
    /// <returns>A collection of activity names that can follow this activity.</returns>
    /// <remarks>
    /// This method helps identify valid activity sequences for workflow validation and UI logic.
    /// </remarks>
    /// <example>
    /// <code>
    /// var moveActivity = MatterDocumentActivityDto.CreateActivity("MOVED");
    /// var compatibleActivities = moveActivity.GetCompatibleFollowUpActivities();
    /// // Returns: ["RESTORED"] (can restore to original location)
    /// </code>
    /// </example>
    public IReadOnlyList<string> GetCompatibleFollowUpActivities()
    {
        return NormalizedActivity switch
        {
            "MOVED" => new[] { "RESTORED" }, // Can restore to original location
            "COPIED" => new[] { "DELETED" }, // Can delete the copy
            "SAVED" => new[] { "MOVED", "COPIED", "DELETED" },
            "DELETED" => new[] { "RESTORED" },
            "RESTORED" => new[] { "SAVED", "MOVED", "COPIED", "DELETED" },
            _ => Array.Empty<string>()
        };
    }

    /// <summary>
    /// Gets transfer operations by date range for analysis and reporting.
    /// </summary>
    /// <param name="startDate">Start date for the range (inclusive).</param>
    /// <param name="endDate">End date for the range (inclusive).</param>
    /// <returns>A collection of transfer operations within the specified date range.</returns>
    /// <remarks>
    /// This method filters transfer operations by date range for audit analysis and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// var lastMonth = DateTime.UtcNow.AddMonths(-1);
    /// var today = DateTime.UtcNow;
    /// var recentTransfers = activity.GetTransferOperationsByDateRange(lastMonth, today);
    /// 
    /// Console.WriteLine($"Found {recentTransfers.Count()} transfers in the last month");
    /// </code>
    /// </example>
    public IEnumerable<object> GetTransferOperationsByDateRange(DateTime startDate, DateTime endDate)
    {
        var fromOperations = MatterDocumentActivityUsersFrom
            .Where(f => f.CreatedAt >= startDate && f.CreatedAt <= endDate)
            .Select(f => new { Direction = "FROM", Operation = f, f.CreatedAt });

        var toOperations = MatterDocumentActivityUsersTo
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .Select(t => new { Direction = "TO", Operation = t, t.CreatedAt });

        return fromOperations.Cast<object>().Union(toOperations.Cast<object>())
            .OrderBy(op => ((dynamic)op).CreatedAt);
    }

    /// <summary>
    /// Gets comprehensive activity usage statistics for analysis and reporting.
    /// </summary>
    /// <returns>A dictionary containing detailed activity usage statistics.</returns>
    /// <remarks>
    /// This method provides structured activity statistics for audit reports and analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = activity.GetActivityStatistics();
    /// foreach (var stat in stats)
    /// {
    ///     Console.WriteLine($"{stat.Key}: {stat.Value}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetActivityStatistics()
    {
        var mostRecentOperation = MatterDocumentActivityUsersFrom
            .Select(f => f.CreatedAt)
            .Union(MatterDocumentActivityUsersTo.Select(t => t.CreatedAt))
            .OrderByDescending(date => date)
            .FirstOrDefault();

        var oldestOperation = MatterDocumentActivityUsersFrom
            .Select(f => f.CreatedAt)
            .Union(MatterDocumentActivityUsersTo.Select(t => t.CreatedAt))
            .OrderBy(date => date)
            .FirstOrDefault();

        return new Dictionary<string, object>
        {
            ["Activity"] = Activity,
            ["ActivityType"] = ActivityType,
            ["IsTransferActivity"] = IsTransferActivity,
            ["IsDestructiveActivity"] = IsDestructiveActivity,
            ["TotalOperations"] = TotalTransferOperations,
            ["SourceOperations"] = MatterDocumentActivityUsersFrom.Count,
            ["DestinationOperations"] = MatterDocumentActivityUsersTo.Count,
            ["ActiveUserCount"] = ActiveUserCount,
            ["HasOperations"] = HasOperations,
            ["MostRecentOperation"] = mostRecentOperation,
            ["OldestOperation"] = oldestOperation,
            ["OperationSpanDays"] = mostRecentOperation != default && oldestOperation != default
                ? (mostRecentOperation - oldestOperation).TotalDays
                : 0,
            ["DisplayActivity"] = DisplayActivity,
            ["ActivityDescription"] = GetActivityDescription()
        }.ToImmutableDictionary();
    }

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterDocumentActivityDto is equal to the current MatterDocumentActivityDto.
    /// </summary>
    /// <param name="other">The MatterDocumentActivityDto to compare with the current MatterDocumentActivityDto.</param>
    /// <returns>true if the specified MatterDocumentActivityDto is equal to the current MatterDocumentActivityDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each activity should have a unique identifier.
    /// </remarks>
    public virtual bool Equals(MatterDocumentActivityDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterDocumentActivityDto.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterDocumentActivityDto.
    /// </summary>
    /// <returns>A string that represents the current MatterDocumentActivityDto.</returns>
    /// <remarks>
    /// The string representation includes key identifying information for logging and debugging purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityDto 
    /// { 
    ///     Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
    ///     Activity = "MOVED"
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "Activity: MOVED (40000000-0000-0000-0000-000000000002) - Transfer - 0 operations"
    /// </code>
    /// </example>
    public override string ToString() => $"Activity: {Activity} ({Id}) - {ActivityType} - {TotalTransferOperations} operations";

    #endregion String Representation
}