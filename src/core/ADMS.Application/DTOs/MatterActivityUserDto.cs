using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing matter activity audit trail with complete user attribution for legal compliance and professional accountability.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of matter activity audit trails within the ADMS legal 
/// document management system, corresponding to <see cref="ADMS.API.Entities.MatterActivityUser"/>. 
/// It captures comprehensive information about matter lifecycle activities performed by users, providing 
/// essential audit trail data for legal compliance, professional responsibility, and matter management.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter Activity Audit Trail:</strong> Complete representation of matter lifecycle activities with full context</item>
/// <item><strong>Professional Validation:</strong> Uses centralized validation helpers for comprehensive data integrity</item>
/// <item><strong>Legal Compliance Support:</strong> Designed for comprehensive audit reporting and legal compliance</item>
/// <item><strong>Entity Synchronization:</strong> Mirrors all properties and relationships from ADMS.API.Entities.MatterActivityUser</item>
/// <item><strong>Cross-Reference Validation:</strong> Ensures consistency between foreign key IDs and navigation properties</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO represents the composite primary key relationship from ADMS.API.Entities.MatterActivityUser:
/// <list type="bullet">
/// <item><strong>Matter Association:</strong> Matter being operated on via MatterId and MatterWithoutDocumentsDto</item>
/// <item><strong>Activity Classification:</strong> Type of matter operation via MatterActivityId and MatterActivityDto</item>
/// <item><strong>User Attribution:</strong> User performing the activity via UserId and UserDto</item>
/// <item><strong>Temporal Tracking:</strong> Timestamp of the activity via CreatedAt</item>
/// </list>
/// 
/// <para><strong>Matter Activities Tracked:</strong></para>
/// <list type="bullet">
/// <item><strong>CREATED:</strong> Matter created by user - foundational activity</item>
/// <item><strong>ARCHIVED:</strong> Matter archived by user - lifecycle management</item>
/// <item><strong>DELETED:</strong> Matter deleted by user - removal activity</item>
/// <item><strong>RESTORED:</strong> Matter restored by user - recovery activity</item>
/// <item><strong>UNARCHIVED:</strong> Matter unarchived by user - reactivation activity</item>
/// <item><strong>VIEWED:</strong> Matter viewed by user - access tracking</item>
/// </list>
/// 
/// <para><strong>Legal Practice Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter Lifecycle Tracking:</strong> Complete tracking of all matter operations and status changes</item>
/// <item><strong>Professional Accountability:</strong> Complete user attribution for matter management decisions</item>
/// <item><strong>Client Communication:</strong> Enables client notification about matter status and activities</item>
/// <item><strong>Matter Organization:</strong> Supports quality assurance for matter management and organization</item>
/// <item><strong>Legal Discovery Support:</strong> Provides detailed matter activity history for legal proceedings</item>
/// </list>
/// 
/// <para><strong>Comprehensive Audit System:</strong></para>
/// This DTO is part of a comprehensive matter and document activity tracking system:
/// <list type="bullet">
/// <item><strong>Matter Activity Tracking:</strong> This DTO tracks general matter lifecycle activities</item>
/// <item><strong>Document Activity Tracking:</strong> MatterDocumentActivityUserDto tracks document-specific activities</item>
/// <item><strong>Complete Activity Coverage:</strong> Together they provide complete activity audit coverage</item>
/// <item><strong>Legal Compliance:</strong> Ensures every matter operation is fully documented and traceable</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Integration:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter Management:</strong> Supports professional matter lifecycle management and organization</item>
/// <item><strong>Client Service:</strong> Enables effective client communication about matter activities and status</item>
/// <item><strong>Practice Management:</strong> Facilitates professional workflow optimization and matter analysis</item>
/// <item><strong>Quality Control:</strong> Provides oversight capabilities for matter operations and quality assurance</item>
/// <item><strong>Compliance Reporting:</strong> Supports regulatory compliance and professional responsibility reporting</item>
/// </list>
/// 
/// <para><strong>Data Integrity and Validation:</strong></para>
/// <list type="bullet">
/// <item><strong>Composite Key Integrity:</strong> All four key components (MatterId, MatterActivityId, UserId, CreatedAt) are validated</item>
/// <item><strong>Foreign Key Consistency:</strong> Cross-reference validation ensures navigation property IDs match foreign key values</item>
/// <item><strong>Temporal Validation:</strong> CreatedAt timestamp validated for chronological consistency and professional standards</item>
/// <item><strong>Entity Completeness:</strong> All navigation properties validated for completeness and business rule compliance</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter Activity Auditing:</strong> Primary use for tracking matter lifecycle activities</item>
/// <item><strong>Legal Compliance Reporting:</strong> Comprehensive audit trail reporting for regulatory compliance</item>
/// <item><strong>Matter Management:</strong> Professional matter lifecycle management and organization</item>
/// <item><strong>Client Communication:</strong> Informing clients about matter status changes and activities</item>
/// <item><strong>Administrative Operations:</strong> Matter activity oversight and quality assurance</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive matter activity audit entry
/// var activityAudit = new MatterActivityUserDto
/// {
///     MatterId = matterGuid,
///     Matter = new MatterWithoutDocumentsDto 
///     { 
///         Id = matterGuid, 
///         Description = "Smith Family Trust" 
///     },
///     MatterActivityId = createdActivityGuid,
///     MatterActivity = new MatterActivityDto 
///     { 
///         Id = createdActivityGuid, 
///         Activity = "CREATED" 
///     },
///     UserId = userGuid,
///     User = new UserDto 
///     { 
///         Id = userGuid, 
///         Name = "Robert Brown" 
///     },
///     CreatedAt = DateTime.UtcNow
/// };
/// 
/// // Comprehensive validation
/// var validationResults = MatterActivityUserDto.ValidateModel(activityAudit);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Activity Audit Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Professional audit trail reporting
/// Console.WriteLine($"Matter '{activityAudit.Matter?.Description}' was " +
///                  $"{activityAudit.MatterActivity?.Activity?.ToLower()} by " +
///                  $"{activityAudit.User?.Name} on {activityAudit.LocalCreatedAtDateString}");
/// </code>
/// </example>
public record MatterActivityUserDto : IValidatableObject, IEquatable<MatterActivityUserDto>
{
    #region Composite Primary Key Properties

    /// <summary>
    /// Gets the unique identifier for the matter being operated on in the activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the matter that is the subject 
    /// of the activity. It corresponds directly to <see cref="ADMS.API.Entities.MatterActivityUser.MatterId"/>
    /// and must reference a valid, existing matter in the system.
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
    /// <item><strong>Matter Operations:</strong> Links activities to specific legal matters for comprehensive organization</item>
    /// <item><strong>Professional Organization:</strong> Enables matter-based activity organization and reporting</item>
    /// <item><strong>Client Attribution:</strong> Associates activities with appropriate clients or cases</item>
    /// <item><strong>Audit Context:</strong> Provides essential context for professional audit and compliance reporting</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null or Guid.Empty - essential for audit trail integrity</item>
    /// <item><strong>Valid Reference:</strong> Must reference an existing matter in the system</item>
    /// <item><strong>Cross-Reference Consistency:</strong> Must match Matter.Id when Matter navigation property is provided</item>
    /// <item><strong>Professional Standards:</strong> Subject to matter-specific business rule validation</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterActivityUser.MatterId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations for reliable audit trail management.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Setting matter context for activity
    /// var activityAudit = new MatterActivityUserDto
    /// {
    ///     MatterId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
    ///     MatterActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Validation example
    /// if (activityAudit.MatterId == Guid.Empty)
    /// {
    ///     throw new ArgumentException("Matter ID is required for activity context");
    /// }
    /// 
    /// // Professional reporting usage
    /// Console.WriteLine($"Matter activity on matter ID: {activityAudit.MatterId}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter ID is required for matter activity context.")]
    public required Guid MatterId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the specific matter activity type being performed.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific type of matter 
    /// activity being performed. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterActivityUser.MatterActivityId"/> and must reference a valid 
    /// matter activity (CREATED, ARCHIVED, DELETED, RESTORED, UNARCHIVED, VIEWED).
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Second component of the four-part composite primary key</item>
    /// <item><strong>Activity Classification:</strong> Identifies the type of matter activity being performed</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing MatterActivity entity</item>
    /// <item><strong>Operation Type Tracking:</strong> Distinguishes between different types of matter operations</item>
    /// </list>
    /// 
    /// <para><strong>Supported Matter Activities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Matter created by user - foundational activity for new matters</item>
    /// <item><strong>ARCHIVED:</strong> Matter archived by user - lifecycle management for inactive matters</item>
    /// <item><strong>DELETED:</strong> Matter deleted by user - removal activity with audit preservation</item>
    /// <item><strong>RESTORED:</strong> Matter restored by user - recovery activity for deleted matters</item>
    /// <item><strong>UNARCHIVED:</strong> Matter unarchived by user - reactivation of archived matters</item>
    /// <item><strong>VIEWED:</strong> Matter viewed by user - access tracking for audit and analytics</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Classification:</strong> Clear categorization of matter operations for professional management</item>
    /// <item><strong>Professional Accountability:</strong> Tracks the specific nature of matter operations</item>
    /// <item><strong>Client Communication:</strong> Enables precise client communication about matter status changes</item>
    /// <item><strong>Legal Discovery:</strong> Important for understanding matter lifecycle and status history</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Categorization:</strong> Enables filtering and analysis by operation type</item>
    /// <item><strong>Compliance Reporting:</strong> Supports detailed compliance reporting requirements</item>
    /// <item><strong>Professional Standards:</strong> Maintains professional practice activity classification</item>
    /// <item><strong>System Integration:</strong> Integrates with broader activity tracking systems</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null or Guid.Empty - essential for operation classification</item>
    /// <item><strong>Valid Reference:</strong> Must reference an existing matter activity</item>
    /// <item><strong>Cross-Reference Consistency:</strong> Must match MatterActivity.Id when navigation property is provided</item>
    /// <item><strong>Activity Type Validation:</strong> Must represent a valid matter activity operation</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterActivityUser.MatterActivityId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations for reliable activity tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Specifying creation operation
    /// var creationOperation = new MatterActivityUserDto
    /// {
    ///     MatterId = matterGuid,
    ///     MatterActivityId = createdActivityGuid, // References "CREATED" activity
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Professional operation tracking
    /// if (creationOperation.MatterActivity?.Activity == "CREATED")
    /// {
    ///     Console.WriteLine("Matter was created by user");
    /// }
    /// else if (creationOperation.MatterActivity?.Activity == "ARCHIVED")
    /// {
    ///     Console.WriteLine("Matter was archived by user");
    /// }
    /// 
    /// // Audit trail classification
    /// var operationType = creationOperation.MatterActivity?.Activity ?? "UNKNOWN";
    /// Console.WriteLine($"Matter activity operation type: {operationType}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter activity ID is required for operation classification.")]
    public required Guid MatterActivityId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the user performing the matter activity operation.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific user who is 
    /// performing the matter activity operation. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterActivityUser.UserId"/> and provides essential user 
    /// attribution for professional accountability and audit trail completeness.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Third component of the four-part composite primary key</item>
    /// <item><strong>User Attribution:</strong> Identifies who performed the matter activity operation</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing User entity</item>
    /// <item><strong>Accountability Tracking:</strong> Essential for professional accountability and responsibility</item>
    /// </list>
    /// 
    /// <para><strong>Professional Accountability:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Responsibility:</strong> Establishes clear accountability for matter management decisions</item>
    /// <item><strong>Professional Standards:</strong> Supports professional practice responsibility requirements</item>
    /// <item><strong>Audit Trail Completeness:</strong> Ensures all matter activities have clear user attribution</item>
    /// <item><strong>Legal Compliance:</strong> Meets legal requirements for matter handling accountability</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Professional Responsibility:</strong> Links matter activities to specific legal professionals</item>
    /// <item><strong>Client Communication:</strong> Enables client communication about who handled their matters</item>
    /// <item><strong>Quality Control:</strong> Supports professional oversight and quality assurance processes</item>
    /// <item><strong>Practice Management:</strong> Facilitates practice management and workflow analysis</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Complete Attribution:</strong> Every matter activity has clear user attribution</item>
    /// <item><strong>Compliance Reporting:</strong> Supports detailed compliance and professional responsibility reporting</item>
    /// <item><strong>Legal Discovery:</strong> Provides user information for legal discovery and professional review</item>
    /// <item><strong>Professional Oversight:</strong> Enables professional oversight and quality assurance</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null or Guid.Empty - essential for professional accountability</item>
    /// <item><strong>Valid Reference:</strong> Must reference an existing, active user in the system</item>
    /// <item><strong>Cross-Reference Consistency:</strong> Must match User.Id when User navigation property is provided</item>
    /// <item><strong>Professional Authorization:</strong> User must be authorized to perform matter activity operations</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterActivityUser.UserId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations for reliable user tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// // User attribution for matter activity
    /// var activityWithUser = new MatterActivityUserDto
    /// {
    ///     MatterId = matterGuid,
    ///     MatterActivityId = activityGuid,
    ///     UserId = Guid.Parse("50000000-0000-0000-0000-000000000001"), // Robert Brown
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Professional accountability reporting
    /// Console.WriteLine($"Matter activity performed by user ID: {activityWithUser.UserId}");
    /// 
    /// // Professional attribution display
    /// if (activityWithUser.User != null)
    /// {
    ///     Console.WriteLine($"Professional: {activityWithUser.User.Name} " +
    ///                      $"performed matter activity at {activityWithUser.LocalCreatedAtDateString}");
    /// }
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User ID is required for professional accountability in matter activities.")]
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the matter activity operation was performed.
    /// </summary>
    /// <remarks>
    /// This DateTime serves as the final component of the composite primary key and records the precise 
    /// moment when the matter activity operation was completed. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterActivityUser.CreatedAt"/> and provides essential 
    /// temporal tracking for audit trails, legal compliance, and professional accountability.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Fourth and final component of the composite primary key</item>
    /// <item><strong>Temporal Foundation:</strong> Establishes precise timing for matter activity operations</item>
    /// <item><strong>Audit Trail Chronology:</strong> Enables chronological sequencing of matter operations</item>
    /// <item><strong>Professional Standards:</strong> Meets professional practice temporal tracking requirements</item>
    /// </list>
    /// 
    /// <para><strong>Temporal Standards and Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>UTC Storage:</strong> Stored in UTC format for global consistency and timezone independence</item>
    /// <item><strong>Immutable Timestamp:</strong> Set once during operation completion and never modified</item>
    /// <item><strong>Professional Accuracy:</strong> Must accurately reflect when the activity operation occurred</item>
    /// <item><strong>Chronological Integrity:</strong> Maintains proper temporal sequencing for audit trails</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Professional Timeline:</strong> Establishes professional timeline for matter handling operations</item>
    /// <item><strong>Legal Discovery:</strong> Provides precise timing information for legal discovery requirements</item>
    /// <item><strong>Client Communication:</strong> Enables accurate client communication about when operations occurred</item>
    /// <item><strong>Compliance Documentation:</strong> Meets professional and regulatory temporal documentation requirements</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Chronological Ordering:</strong> Enables proper chronological ordering of matter operations</item>
    /// <item><strong>Temporal Analysis:</strong> Supports temporal analysis of matter management patterns</item>
    /// <item><strong>Professional Reporting:</strong> Provides temporal foundation for professional activity reports</item>
    /// <item><strong>Compliance Auditing:</strong> Supports compliance auditing with precise temporal tracking</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Valid Range:</strong> Between January 1, 1980, and current time (with tolerance for system delays)</item>
    /// <item><strong>Not Future:</strong> Cannot be in the future to prevent temporal inconsistencies</item>
    /// <item><strong>Not Default:</strong> Must be a valid DateTime, not DateTime.MinValue or default values</item>
    /// <item><strong>Professional Reasonableness:</strong> Must be reasonable for professional legal practice context</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterActivityUser.CreatedAt"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations for reliable temporal tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Setting precise activity timestamp
    /// var timestampedActivity = new MatterActivityUserDto
    /// {
    ///     MatterId = matterGuid,
    ///     MatterActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow // Always use UTC
    /// };
    /// 
    /// // Professional temporal reporting
    /// Console.WriteLine($"Activity completed at: {timestampedActivity.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
    /// 
    /// // Professional display for client communication
    /// Console.WriteLine($"Matter activity performed on: {timestampedActivity.LocalCreatedAtDateString}");
    /// 
    /// // Chronological audit trail analysis
    /// var activityAge = DateTime.UtcNow - timestampedActivity.CreatedAt;
    /// Console.WriteLine($"Activity occurred {activityAge.TotalDays:F1} days ago");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Creation timestamp is required for audit trail temporal tracking.")]
    public required DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    #endregion Composite Primary Key Properties

    #region Navigation Properties

    /// <summary>
    /// Gets the comprehensive matter details for the matter being operated on in the activity.
    /// </summary>
    /// <remarks>
    /// This property provides complete matter information for the matter in the activity operation, 
    /// corresponding to the <see cref="ADMS.API.Entities.MatterActivityUser.Matter"/> navigation property. 
    /// It uses MatterWithoutDocumentsDto to provide comprehensive matter information while avoiding 
    /// potential circular references with document collections.
    /// 
    /// <para><strong>Matter Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Subject:</strong> Complete information about the matter being operated on</item>
    /// <item><strong>Matter Organization:</strong> Provides context for matter-based activity organization</item>
    /// <item><strong>Professional Display:</strong> Enables professional presentation of activity context</item>
    /// <item><strong>Client Communication:</strong> Supports client communication about matter activities</item>
    /// </list>
    /// 
    /// <para><strong>Comprehensive Matter Information Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Matter Identification:</strong> Complete matter description and identification information</item>
    /// <item><strong>Matter Status:</strong> Current matter status (active, archived, deleted) for context</item>
    /// <item><strong>Activity Relationships:</strong> Matter activity associations for comprehensive audit context</item>
    /// <item><strong>Professional Metadata:</strong> Creation dates, status flags, and professional information</item>
    /// </list>
    /// 
    /// <para><strong>Professional Practice Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Matter-Based Organization:</strong> Supports professional matter-based activity organization strategies</item>
    /// <item><strong>Client Service:</strong> Enables effective client communication about matter status and activities</item>
    /// <item><strong>Professional Display:</strong> Provides professional presentation of matter information</item>
    /// <item><strong>Quality Assurance:</strong> Enables validation of appropriate matter operations</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Presentation:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Complete Context:</strong> Provides complete matter context for audit trail reports</item>
    /// <item><strong>Professional Reporting:</strong> Enables professional audit trail and activity reports</item>
    /// <item><strong>Cross-Reference Validation:</strong> ID must match MatterId for consistency</item>
    /// <item><strong>Compliance Documentation:</strong> Supports compliance reporting requirements</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Conditional Requirement:</strong> Should be provided for complete audit trail context</item>
    /// <item><strong>Cross-Reference Consistency:</strong> Matter.Id must match MatterId when both are provided</item>
    /// <item><strong>Entity Validity:</strong> Must pass MatterWithoutDocumentsDto validation when provided</item>
    /// <item><strong>Professional Standards:</strong> Must represent a valid, accessible matter</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Complete matter context information
    /// var activityWithMatter = new MatterActivityUserDto
    /// {
    ///     MatterId = matterGuid,
    ///     Matter = new MatterWithoutDocumentsDto
    ///     {
    ///         Id = matterGuid,
    ///         Description = "Smith Family Trust",
    ///         IsArchived = false,
    ///         CreationDate = DateTime.UtcNow.AddMonths(-6)
    ///     },
    ///     MatterActivityId = activityGuid,
    ///     // ... other properties
    /// };
    /// 
    /// // Professional audit trail reporting
    /// if (activityWithMatter.Matter != null)
    /// {
    ///     Console.WriteLine($"Matter activity on: '{activityWithMatter.Matter.Description}' " +
    ///                      $"(Status: {activityWithMatter.Matter.Status})");
    /// }
    /// 
    /// // Professional client communication
    /// var matterDescription = activityWithMatter.Matter?.DisplayText ?? "Unknown Matter";
    /// Console.WriteLine($"Activity performed on your matter: {matterDescription}");
    /// </code>
    /// </example>
    public MatterWithoutDocumentsDto? Matter { get; init; }

    /// <summary>
    /// Gets the comprehensive matter activity details describing the type of activity operation.
    /// </summary>
    /// <remarks>
    /// This property provides complete activity information for the matter activity operation being performed, 
    /// corresponding to the <see cref="ADMS.API.Entities.MatterActivityUser.MatterActivity"/> navigation property. 
    /// It includes comprehensive activity data and relationships for complete audit trail management and 
    /// professional compliance.
    /// 
    /// <para><strong>Activity Classification:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Operation Type Identification:</strong> Clearly identifies the type of matter activity</item>
    /// <item><strong>Professional Standards:</strong> Maintains professional activity classification standards</item>
    /// <item><strong>Audit Trail Integration:</strong> Integrates with broader activity tracking and audit systems</item>
    /// <item><strong>Compliance Reporting:</strong> Supports detailed compliance reporting requirements</item>
    /// </list>
    /// 
    /// <para><strong>Supported Matter Activities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Matter created by user - foundational activity</item>
    /// <item><strong>ARCHIVED:</strong> Matter archived by user - lifecycle management</item>
    /// <item><strong>DELETED:</strong> Matter deleted by user - removal activity</item>
    /// <item><strong>RESTORED:</strong> Matter restored by user - recovery activity</item>
    /// <item><strong>UNARCHIVED:</strong> Matter unarchived by user - reactivation activity</item>
    /// <item><strong>VIEWED:</strong> Matter viewed by user - access tracking</item>
    /// </list>
    /// 
    /// <para><strong>Comprehensive Activity Information Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Identification:</strong> Complete activity type and classification information</item>
    /// <item><strong>Activity Metadata:</strong> Activity-specific metadata and configuration</item>
    /// <item><strong>Usage Statistics:</strong> Activity usage patterns and metrics for analysis</item>
    /// <item><strong>Professional Context:</strong> Professional practice context and implications</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Professional Classification:</strong> Clear categorization of matter operations</item>
    /// <item><strong>Client Communication:</strong> Enables precise client communication about matter operations</item>
    /// <item><strong>Legal Discovery:</strong> Important for understanding matter lifecycle and status changes</item>
    /// <item><strong>Professional Accountability:</strong> Clear classification supports professional responsibility</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Conditional Requirement:</strong> Should be provided for complete operation classification</item>
    /// <item><strong>Cross-Reference Consistency:</strong> MatterActivity.Id must match MatterActivityId when both provided</item>
    /// <item><strong>Entity Validity:</strong> Must pass MatterActivityDto validation when provided</item>
    /// <item><strong>Operation Validity:</strong> Must represent a valid matter activity operation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Complete activity information
    /// var activityWithClassification = new MatterActivityUserDto
    /// {
    ///     MatterActivityId = activityGuid,
    ///     MatterActivity = new MatterActivityDto
    ///     {
    ///         Id = activityGuid,
    ///         Activity = "CREATED"
    ///     },
    ///     MatterId = matterGuid,
    ///     // ... other properties
    /// };
    /// 
    /// // Professional operation classification
    /// if (activityWithClassification.MatterActivity != null)
    /// {
    ///     var operationType = activityWithClassification.MatterActivity.Activity;
    ///     var operationDescription = operationType switch
    ///     {
    ///         "CREATED" => "created",
    ///         "ARCHIVED" => "archived",
    ///         "DELETED" => "deleted",
    ///         "RESTORED" => "restored",
    ///         _ => "performed unknown operation on"
    ///     };
    ///     
    ///     Console.WriteLine($"User {operationDescription} the matter");
    /// }
    /// 
    /// // Professional compliance reporting
    /// var activityType = activityWithClassification.MatterActivity?.Activity ?? "UNKNOWN";
    /// Console.WriteLine($"Matter activity operation type: {activityType}");
    /// </code>
    /// </example>
    public MatterActivityDto? MatterActivity { get; init; }

    /// <summary>
    /// Gets the comprehensive user details for the user who performed the matter activity operation.
    /// </summary>
    /// <remarks>
    /// This property provides complete user information for the professional who performed the matter 
    /// activity operation, corresponding to the <see cref="ADMS.API.Entities.MatterActivityUser.User"/> 
    /// navigation property. It includes comprehensive user data and activity relationships for complete 
    /// professional accountability and audit trail management.
    /// 
    /// <para><strong>Professional Accountability Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Attribution:</strong> Complete professional attribution for matter activity operations</item>
    /// <item><strong>Professional Responsibility:</strong> Supports professional responsibility and accountability standards</item>
    /// <item><strong>Quality Assurance:</strong> Enables professional quality assurance and oversight</item>
    /// <item><strong>Client Communication:</strong> Provides professional context for client communications</item>
    /// </list>
    /// 
    /// <para><strong>Comprehensive User Information Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Professional Identity:</strong> Complete professional name and identification information</item>
    /// <item><strong>Activity History:</strong> User activity relationships for comprehensive context</item>
    /// <item><strong>Professional Metadata:</strong> Professional display names and presentation information</item>
    /// <item><strong>System Integration:</strong> User system integration and authorization context</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Professional Standards:</strong> Maintains professional naming conventions and accountability</item>
    /// <item><strong>Legal Compliance:</strong> Supports legal compliance and professional responsibility requirements</item>
    /// <item><strong>Client Relations:</strong> Enables professional client relations and communication</item>
    /// <item><strong>Practice Management:</strong> Facilitates professional practice management and oversight</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Presentation:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Complete Attribution:</strong> Provides complete user attribution for audit trail reports</item>
    /// <item><strong>Professional Display:</strong> Enables professional presentation of user information</item>
    /// <item><strong>Cross-Reference Validation:</strong> ID must match UserId for consistency</item>
    /// <item><strong>Compliance Documentation:</strong> Supports compliance reporting and professional oversight</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Conditional Requirement:</strong> Should be provided for complete professional accountability</item>
    /// <item><strong>Cross-Reference Consistency:</strong> User.Id must match UserId when both are provided</item>
    /// <item><strong>Entity Validity:</strong> Must pass UserDto validation when provided</item>
    /// <item><strong>Professional Authorization:</strong> User must be authorized for matter activity operations</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Complete user information in activity
    /// var activityWithUser = new MatterActivityUserDto
    /// {
    ///     UserId = userGuid,
    ///     User = new UserDto
    ///     {
    ///         Id = userGuid,
    ///         Name = "Robert Brown"
    ///     },
    ///     MatterId = matterGuid,
    ///     // ... other properties
    /// };
    /// 
    /// // Professional accountability reporting
    /// if (activityWithUser.User != null)
    /// {
    ///     Console.WriteLine($"Matter activity performed by: {activityWithUser.User.DisplayName} " +
    ///                      $"on {activityWithUser.LocalCreatedAtDateString}");
    /// }
    /// 
    /// // Professional client communication
    /// var professionalName = activityWithUser.User?.DisplayName ?? "System Administrator";
    /// Console.WriteLine($"Your matter was professionally handled by {professionalName}");
    /// 
    /// // Professional practice oversight
    /// if (activityWithUser.User?.HasActivities == true)
    /// {
    ///     Console.WriteLine($"Professional {activityWithUser.User.DisplayName} has " +
    ///                      $"{activityWithUser.User.TotalActivityCount} total system activities");
    /// }
    /// </code>
    /// </example>
    public UserDto? User { get; init; }

    #endregion Navigation Properties

    #region Computed Properties

    /// <summary>
    /// Gets the creation timestamp formatted as a localized string for professional presentation.
    /// </summary>
    /// <remarks>
    /// This computed property provides a user-friendly formatted representation of the activity timestamp 
    /// converted to local time, optimized for professional interfaces, client communications, and audit 
    /// trail presentation.
    /// 
    /// <para><strong>Professional Presentation Format:</strong></para>
    /// Uses "dddd, dd MMMM yyyy HH:mm:ss" format providing complete temporal information:
    /// <list type="bullet">
    /// <item><strong>Day of Week:</strong> "Monday" - provides additional temporal context for professional scheduling</item>
    /// <item><strong>Full Date:</strong> "15 January 2024" - complete date information for professional records</item>
    /// <item><strong>Precise Time:</strong> "14:30:45" - exact time for professional accuracy and audit requirements</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var activity = new MatterActivityUserDto 
    /// { 
    ///     CreatedAt = new DateTime(2024, 1, 15, 14, 30, 45, DateTimeKind.Utc),
    ///     // ... other properties
    /// };
    /// 
    /// // Professional audit trail display
    /// Console.WriteLine($"Activity completed: {activity.LocalCreatedAtDateString}");
    /// // Output: "Activity completed: Monday, 15 January 2024 16:30:45" (if local time is UTC+2)
    /// 
    /// // Client communication
    /// Console.WriteLine($"Your matter activity occurred on {activity.LocalCreatedAtDateString}");
    /// </code>
    /// </example>
    public string LocalCreatedAtDateString => CreatedAt.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the matter activity operation summary for professional audit trail display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a concise, professional summary of the matter activity operation 
    /// including key participants and operation details, optimized for audit trail reports and professional 
    /// communication.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activitySummary = activity.ActivitySummary;
    /// // Returns: "Smith Family Trust CREATED by Robert Brown"
    /// 
    /// Console.WriteLine($"Activity Summary: {activitySummary}");
    /// </code>
    /// </example>
    public string ActivitySummary =>
        $"{Matter?.Description ?? "Matter"} " +
        $"{MatterActivity?.Activity ?? "ACTIVITY"} " +
        $"by {User?.Name ?? "User"}";

    /// <summary>
    /// Gets comprehensive activity metrics for analysis and reporting.
    /// </summary>
    /// <remarks>
    /// This property provides a structured object containing key metrics and information 
    /// for comprehensive activity analysis, professional reporting, and compliance documentation.
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
            OperationType = MatterActivity?.Activity ?? "UNKNOWN",
            ActivityContext = "Matter Activity"
        },
        ParticipantInfo = new
        {
            MatterContext = Matter?.Description ?? "Unknown Matter",
            UserName = User?.Name ?? "Unknown User",
            UserId,
            MatterId
        },
        TemporalInfo = new
        {
            CreatedAt,
            LocalCreatedAtDateString,
            ActivityAge = (DateTime.UtcNow - CreatedAt).TotalDays
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
    /// Validates the <see cref="MatterActivityUserDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using centralized validation helpers for consistency with entity 
    /// validation rules. This ensures the DTO maintains the same validation standards as the corresponding 
    /// ADMS.API.Entities.MatterActivityUser entity while enforcing audit trail-specific business rules.
    /// 
    /// <para><strong>Comprehensive Validation Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Core ID Validation:</strong> All composite primary key GUIDs validated for completeness</item>
    /// <item><strong>Navigation Property Validation:</strong> Deep validation of all related entities when provided</item>
    /// <item><strong>Cross-Reference Validation:</strong> Ensures navigation property IDs match foreign key values</item>
    /// <item><strong>Temporal Validation:</strong> CreatedAt timestamp validated for professional standards</item>
    /// <item><strong>Business Rule Validation:</strong> Activity-specific business rule compliance</item>
    /// </list>
    /// 
    /// <para><strong>Professional Standards Integration:</strong></para>
    /// Uses centralized validation helpers to ensure consistency across all audit trail validation in the system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterActivityUserDto 
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
    ///     Console.WriteLine($"Activity Audit Validation Error: {result.ErrorMessage}");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate all composite primary key GUID components
        foreach (var result in ValidateGuids())
            yield return result;

        // Validate navigation properties when provided
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
    /// Uses DtoValidationHelper for consistent validation when the Matter navigation property is provided, 
    /// ensuring comprehensive matter information validation.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateMatter()
    {
        if (Matter is not IValidatableObject validatable) yield break;
        var context = new ValidationContext(Matter);
        foreach (var result in validatable.Validate(context))
            yield return new ValidationResult($"Activity Matter Context: {result.ErrorMessage}", result.MemberNames);
    }

    /// <summary>
    /// Validates the <see cref="MatterActivity"/> navigation property using centralized validation.
    /// </summary>
    /// <returns>A collection of validation results for activity validation.</returns>
    /// <remarks>
    /// Uses DtoValidationHelper for consistent validation when the MatterActivity navigation 
    /// property is provided, ensuring comprehensive activity classification validation.
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
    /// Uses DtoValidationHelper for consistent validation when the User navigation property is provided, 
    /// ensuring comprehensive user information validation for professional accountability.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateUser()
    {
        if (User is not IValidatableObject validatable) yield break;
        var context = new ValidationContext(User);
        foreach (var result in validatable.Validate(context))
            yield return new ValidationResult($"Activity User: {result.ErrorMessage}", result.MemberNames);
    }

    /// <summary>
    /// Validates consistency between foreign key IDs and their corresponding navigation properties.
    /// </summary>
    /// <returns>A collection of validation results for cross-reference consistency.</returns>
    /// <remarks>
    /// Ensures that when navigation properties are provided, their ID values match the corresponding 
    /// foreign key properties, maintaining referential integrity and preventing data inconsistencies.
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
    /// Uses professional standards for temporal validation, ensuring the timestamp meets professional 
    /// standards and audit trail requirements.
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
    /// Validates a <see cref="MatterActivityUserDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The MatterActivityUserDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate MatterActivityUserDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance Validate method 
    /// but with null-safety and simplified usage for audit trail validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterActivityUserDto 
    /// { 
    ///     MatterId = matterGuid,
    ///     MatterActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// var results = MatterActivityUserDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Activity audit validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterActivityUserDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterActivityUserDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a MatterActivityUserDto from ADMS.API.Entities.MatterActivityUser entity with validation.
    /// </summary>
    /// <param name="entity">The MatterActivityUser entity to convert. Cannot be null.</param>
    /// <param name="includeNavigationProperties">Whether to include navigation properties in the conversion.</param>
    /// <returns>A valid MatterActivityUserDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create MatterActivityUserDto instances from
    /// ADMS.API.Entities.MatterActivityUser entities with automatic validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity with complete information
    /// var entity = new ADMS.API.Entities.MatterActivityUser 
    /// { 
    ///     MatterId = matterGuid,
    ///     MatterActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// var dto = MatterActivityUserDto.FromEntity(entity, includeNavigationProperties: true);
    /// </code>
    /// </example>
    public static MatterActivityUserDto FromEntity([NotNull] Entities.MatterActivityUser entity, bool includeNavigationProperties = false)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var dto = new MatterActivityUserDto
        {
            MatterId = entity.MatterId,
            MatterActivityId = entity.MatterActivityId,
            UserId = entity.UserId,
            CreatedAt = entity.CreatedAt
        };

        // Note: In practice, navigation properties would be mapped using a mapping framework
        // like AutoMapper or Mapster when includeNavigationProperties is true

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterActivityUserDto from entity: {errorMessages}");

    }

    /// <summary>
    /// Creates multiple MatterActivityUserDto instances from a collection of entities.
    /// </summary>
    /// <param name="entities">The collection of MatterActivityUser entities to convert. Cannot be null.</param>
    /// <param name="includeNavigationProperties">Whether to include navigation properties in the conversion.</param>
    /// <returns>A collection of valid MatterActivityUserDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method is optimized for creating multiple activity audit DTOs efficiently.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convert collection of activity audit entities
    /// var entities = await context.MatterActivityUsers.ToListAsync();
    /// var activityAuditDtos = MatterActivityUserDto.FromEntities(entities, includeNavigationProperties: false);
    /// 
    /// // Process audit trail
    /// foreach (var auditDto in activityAuditDtos)
    /// {
    ///     ProcessMatterActivity(auditDto);
    /// }
    /// </code>
    /// </example>
    public static IList<MatterActivityUserDto> FromEntities([NotNull] IEnumerable<Entities.MatterActivityUser> entities, bool includeNavigationProperties = false)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        var result = new List<MatterActivityUserDto>();

        foreach (var entity in entities)
        {
            try
            {
                var dto = FromEntity(entity, includeNavigationProperties);
                result.Add(dto);
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                // Log invalid entity but continue processing others
                Console.WriteLine($"Warning: Skipped invalid matter activity entity: {ex.Message}");
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
    /// <returns>A valid MatterActivityUserDto instance.</returns>
    /// <exception cref="ArgumentException">Thrown when any GUID parameter is empty.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a convenient way to create activity audit entries with all required
    /// parameters while ensuring validation compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create activity audit entry
    /// var activityAudit = MatterActivityUserDto.CreateActivityAudit(
    ///     matterGuid,
    ///     createdActivityGuid,
    ///     userGuid);
    /// 
    /// // Save to audit system
    /// await auditService.RecordActivityAsync(activityAudit);
    /// </code>
    /// </example>
    public static MatterActivityUserDto CreateActivityAudit(
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

        var dto = new MatterActivityUserDto
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
    /// if (activityAudit.IsCreationOperation())
    /// {
    ///     Console.WriteLine($"Matter was created: {activityAudit.Matter?.Description}");
    /// }
    /// </code>
    /// </example>
    public bool IsCreationOperation() =>
        string.Equals(MatterActivity?.Activity, "CREATED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a matter lifecycle operation.
    /// </summary>
    /// <returns>true if this represents an ARCHIVED, DELETED, RESTORED, or UNARCHIVED operation; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify lifecycle operations for activity analysis and matter state management.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityAudit.IsLifecycleOperation())
    /// {
    ///     Console.WriteLine($"Matter lifecycle change: {activityAudit.MatterActivity?.Activity}");
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
    /// This method helps identify viewing operations for access tracking and analytics.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityAudit.IsViewingOperation())
    /// {
    ///     Console.WriteLine($"Matter was viewed: {activityAudit.Matter?.Description}");
    /// }
    /// </code>
    /// </example>
    public bool IsViewingOperation() =>
        string.Equals(MatterActivity?.Activity, "VIEWED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the age of this activity in days since it was performed.
    /// </summary>
    /// <returns>The number of days since the activity was performed.</returns>
    /// <remarks>
    /// This method calculates how long ago the activity occurred, useful for activity 
    /// analysis, audit trail review, and professional practice management.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityAge = activityAudit.GetActivityAgeDays();
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
    public double GetActivityAgeDays() => (DateTime.UtcNow - CreatedAt).TotalDays;

    /// <summary>
    /// Determines whether this activity occurred recently (within specified days).
    /// </summary>
    /// <param name="withinDays">The number of days to consider as recent (defaults to 7).</param>
    /// <returns>true if the activity occurred within the specified timeframe; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify recent activity for audit analysis and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityAudit.IsRecentActivity(30))
    /// {
    ///     // Activity occurred within last 30 days
    ///     Console.WriteLine("Recent matter activity detected");
    /// }
    /// </code>
    /// </example>
    public bool IsRecentActivity(int withinDays = 7) => GetActivityAgeDays() <= withinDays;

    /// <summary>
    /// Gets comprehensive activity information for reporting and analysis.
    /// </summary>
    /// <returns>A dictionary containing detailed activity information.</returns>
    /// <remarks>
    /// This method provides structured activity information for audit reports,
    /// compliance documentation, and activity analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityInfo = activityAudit.GetActivityInformation();
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
            ["ActivityAge"] = GetActivityAgeDays(),
            ["IsRecent"] = IsRecentActivity(),
            ["ActivitySummary"] = ActivitySummary,
            ["HasCompleteInformation"] = Matter != null && MatterActivity != null && User != null
        }.ToImmutableDictionary();
    }

    /// <summary>
    /// Generates a professional audit trail message for this activity.
    /// </summary>
    /// <returns>A formatted audit trail message suitable for professional reporting.</returns>
    /// <remarks>
    /// This method creates a professional audit message that can be used in audit reports,
    /// client communications, and compliance documentation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var auditMessage = activityAudit.GenerateAuditMessage();
    /// // Returns: "On Monday, 15 January 2024 14:30:45, Robert Brown CREATED Smith Family Trust"
    /// 
    /// Console.WriteLine(auditMessage);
    /// </code>
    /// </example>
    public string GenerateAuditMessage()
    {
        var operationType = MatterActivity?.Activity ?? "PERFORMED ACTIVITY ON";
        var matterDescription = Matter?.Description ?? "matter";
        var userName = User?.Name ?? "user";

        return $"On {LocalCreatedAtDateString}, {userName} {operationType} {matterDescription}";
    }

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterActivityUserDto is equal to the current MatterActivityUserDto.
    /// </summary>
    /// <param name="other">The MatterActivityUserDto to compare with the current MatterActivityUserDto.</param>
    /// <returns>true if the specified MatterActivityUserDto is equal to the current MatterActivityUserDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all composite key properties that uniquely identify an activity audit entry.
    /// This follows the same pattern as the entity composite key for consistency.
    /// </remarks>
    public virtual bool Equals(MatterActivityUserDto? other)
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
    /// <returns>A hash code for the current MatterActivityUserDto.</returns>
    /// <remarks>
    /// The hash code is based on all composite key properties to ensure consistent hashing behavior
    /// that aligns with the equality implementation and composite key structure.
    /// </remarks>
    public override int GetHashCode()
    {
        return HashCode.Combine(MatterId, MatterActivityId, UserId, CreatedAt);
    }

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterActivityUserDto.
    /// </summary>
    /// <returns>A string that represents the current MatterActivityUserDto.</returns>
    /// <remarks>
    /// The string representation includes key activity audit information for identification and logging purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterActivityUserDto 
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