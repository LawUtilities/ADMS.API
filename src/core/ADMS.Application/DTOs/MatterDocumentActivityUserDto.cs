using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing the general document activity audit trail within matter context for complete activity tracking.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of general document activity audit trails within the ADMS legal 
/// document management system, corresponding to <see cref="ADMS.API.Entities.MatterDocumentActivityUser"/>. It captures 
/// comprehensive information about document activities performed within matter context, providing essential audit trail 
/// data for legal compliance, professional responsibility, and document activity tracking.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>General Document Activity Audit:</strong> Captures comprehensive information about general document activities within matter context</item>
/// <item><strong>Professional Validation:</strong> Uses centralized validation helpers for comprehensive data integrity</item>
/// <item><strong>Legal Compliance Support:</strong> Designed for comprehensive audit reporting and legal compliance</item>
/// <item><strong>Entity Synchronization:</strong> Mirrors all properties and relationships from ADMS.API.Entities.MatterDocumentActivityUser</item>
/// <item><strong>Cross-Reference Validation:</strong> Ensures consistency between foreign key IDs and navigation properties</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO represents the composite primary key relationship from ADMS.API.Entities.MatterDocumentActivityUser:
/// <list type="bullet">
/// <item><strong>Matter Association:</strong> Matter providing context for the document activity via MatterId and MatterWithoutDocumentsDto</item>
/// <item><strong>Document Association:</strong> Document being operated on via DocumentId and DocumentWithoutRevisionsDto</item>
/// <item><strong>Activity Classification:</strong> Type of document activity via MatterDocumentActivityId and MatterDocumentActivityDto</item>
/// <item><strong>User Attribution:</strong> User performing the activity via UserId and UserDto</item>
/// <item><strong>Temporal Tracking:</strong> Timestamp of the activity via CreatedAt</item>
/// </list>
/// 
/// <para><strong>Document Activities Tracked:</strong></para>
/// <list type="bullet">
/// <item><strong>SAVED:</strong> Document saved within matter context</item>
/// <item><strong>MOVED:</strong> Document moved between matters (part of general activity tracking)</item>
/// <item><strong>COPIED:</strong> Document copied between matters (part of general activity tracking)</item>
/// <item><strong>DELETED:</strong> Document deleted from matter</item>
/// <item><strong>RESTORED:</strong> Document restored to matter</item>
/// </list>
/// 
/// <para><strong>Legal Practice Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>General Document Activity Tracking:</strong> Complete tracking of document activities within matter context</item>
/// <item><strong>Professional Accountability:</strong> Complete user attribution for document operations within matter context</item>
/// <item><strong>Client Communication:</strong> Enables client notification about document activities and operations</item>
/// <item><strong>Matter-Document Integration:</strong> Links document activities to specific matter context for comprehensive tracking</item>
/// <item><strong>Legal Discovery Support:</strong> Provides detailed document activity history for legal proceedings</item>
/// </list>
/// 
/// <para><strong>Comprehensive Audit System:</strong></para>
/// This DTO is part of a comprehensive document activity tracking system:
/// <list type="bullet">
/// <item><strong>General Activity Tracking:</strong> This DTO tracks general document activities within matter context</item>
/// <item><strong>Directional Transfer Tracking:</strong> MatterDocumentActivityUserFromDto/ToDto track specific transfer operations</item>
/// <item><strong>Complete Activity Coverage:</strong> Together they provide complete document activity audit coverage</item>
/// <item><strong>Legal Compliance:</strong> Ensures every document activity is fully documented and traceable</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Integration:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Management:</strong> Supports professional document activity tracking and matter integration</item>
/// <item><strong>Client Service:</strong> Enables effective client communication about document activities and status</item>
/// <item><strong>Practice Management:</strong> Facilitates professional workflow optimization and activity analysis</item>
/// <item><strong>Quality Control:</strong> Provides oversight capabilities for document activities and quality assurance</item>
/// <item><strong>Compliance Reporting:</strong> Supports regulatory compliance and professional responsibility reporting</item>
/// </list>
/// 
/// <para><strong>Data Integrity and Validation:</strong></para>
/// <list type="bullet">
/// <item><strong>Composite Key Integrity:</strong> All five key components (MatterId, DocumentId, MatterDocumentActivityId, UserId, CreatedAt) are validated</item>
/// <item><strong>Foreign Key Consistency:</strong> Cross-reference validation ensures navigation property IDs match foreign key values</item>
/// <item><strong>Temporal Validation:</strong> CreatedAt timestamp validated for chronological consistency and professional standards</item>
/// <item><strong>Entity Completeness:</strong> All navigation properties validated for completeness and business rule compliance</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Activity Auditing:</strong> Primary use for tracking general document activities within matter context</item>
/// <item><strong>Legal Compliance Reporting:</strong> Comprehensive audit trail reporting for regulatory compliance</item>
/// <item><strong>Matter-Document Management:</strong> Professional document activity tracking with matter context integration</item>
/// <item><strong>Client Communication:</strong> Informing clients about document activities and status changes</item>
/// <item><strong>Administrative Operations:</strong> Document activity oversight and quality assurance</item>
/// </list>
/// 
/// <para><strong>Distinction from Transfer DTOs:</strong></para>
/// <list type="bullet">
/// <item><strong>General Activities:</strong> This DTO tracks general document activities within matter context</item>
/// <item><strong>Transfer Activities:</strong> FromDto/ToDto track specific bidirectional transfer operations</item>
/// <item><strong>Complementary Coverage:</strong> Together they provide complete document activity audit coverage</item>
/// <item><strong>Context Specificity:</strong> This DTO provides matter-document activity linkage for general operations</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive document activity audit entry
/// var activityAudit = new MatterDocumentActivityUserDto
/// {
///     MatterId = matterGuid,
///     Matter = new MatterWithoutDocumentsDto 
///     { 
///         Id = matterGuid, 
///         Description = "Client ABC - Legal Research" 
///     },
///     DocumentId = documentGuid,
///     Document = new DocumentWithoutRevisionsDto 
///     { 
///         Id = documentGuid, 
///         FileName = "Research_Notes.docx" 
///     },
///     MatterDocumentActivityId = saveActivityGuid,
///     MatterDocumentActivity = new MatterDocumentActivityDto 
///     { 
///         Id = saveActivityGuid, 
///         Activity = "SAVED" 
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
/// var validationResults = MatterDocumentActivityUserDto.ValidateModel(activityAudit);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Activity Audit Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Professional audit trail reporting
/// Console.WriteLine($"Document '{activityAudit.Document?.FileName}' was " +
///                  $"{activityAudit.MatterDocumentActivity?.Activity?.ToLower()} in " +
///                  $"matter '{activityAudit.Matter?.Description}' by " +
///                  $"{activityAudit.User?.Name} on {activityAudit.LocalCreatedAtDateString}");
/// </code>
/// </example>
public record MatterDocumentActivityUserDto : IValidatableObject, IEquatable<MatterDocumentActivityUserDto>
{
    #region Composite Primary Key Properties

    /// <summary>
    /// Gets the unique identifier for the matter providing context for the document activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the matter that provides 
    /// context for the document activity. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivityUser.MatterId"/> and must reference a valid, 
    /// existing matter in the system.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> First component of the five-part composite primary key</item>
    /// <item><strong>Context Identification:</strong> Identifies which matter provides context for the document activity</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing Matter entity</item>
    /// <item><strong>Audit Trail Foundation:</strong> Essential for matter-context activity tracking</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Matter Context:</strong> Links document activities to specific legal matters for comprehensive organization</item>
    /// <item><strong>Professional Organization:</strong> Enables matter-based document activity organization and reporting</item>
    /// <item><strong>Client Attribution:</strong> Associates document activities with appropriate clients or cases</item>
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
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUser.MatterId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations for reliable audit trail management.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Setting matter context for document activity
    /// var activityAudit = new MatterDocumentActivityUserDto
    /// {
    ///     MatterId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
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
    /// Console.WriteLine($"Document activity in matter context ID: {activityAudit.MatterId}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter ID is required for document activity context.")]
    public required Guid MatterId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the document being operated on in the activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific document that is 
    /// the subject of the activity. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivityUser.DocumentId"/> and must reference a valid, 
    /// existing document in the system.
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
    /// <item><strong>Document Operations:</strong> Tracks activities on specific legal documents within matter context</item>
    /// <item><strong>Professional Accountability:</strong> Identifies exactly which documents are being operated on</item>
    /// <item><strong>Client Communication:</strong> Enables precise client communication about specific document activities</item>
    /// <item><strong>Legal Discovery:</strong> Critical for legal discovery and document production requirements</item>
    /// </list>
    /// 
    /// <para><strong>Activity Operation Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>SAVE Operations:</strong> Document saved within matter context</item>
    /// <item><strong>DELETE Operations:</strong> Document deleted from matter</item>
    /// <item><strong>RESTORE Operations:</strong> Document restored to matter</item>
    /// <item><strong>Audit Completeness:</strong> Ensures every document activity is precisely tracked</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null or Guid.Empty - essential for audit trail integrity</item>
    /// <item><strong>Valid Reference:</strong> Must reference an existing document in the system</item>
    /// <item><strong>Cross-Reference Consistency:</strong> Must match Document.Id when Document navigation property is provided</item>
    /// <item><strong>Activity Eligibility:</strong> Document must be eligible for the specific activity operation</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUser.DocumentId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations for reliable document tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Identifying specific document in activity
    /// var documentActivity = new MatterDocumentActivityUserDto
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
    /// 
    /// // Audit trail reporting
    /// if (documentActivity.Document != null)
    /// {
    ///     Console.WriteLine($"Document '{documentActivity.Document.FileName}' " +
    ///                      $"({documentActivity.DocumentId}) activity tracked");
    /// }
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Document ID is required for document activity identification.")]
    public required Guid DocumentId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the specific matter document activity operation.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific type of document 
    /// activity being performed within the matter context. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivityUser.MatterDocumentActivityId"/> and must 
    /// reference a valid matter document activity.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Third component of the five-part composite primary key</item>
    /// <item><strong>Activity Classification:</strong> Identifies the type of document activity being performed</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing MatterDocumentActivity entity</item>
    /// <item><strong>Operation Type Tracking:</strong> Distinguishes between different types of document operations</item>
    /// </list>
    /// 
    /// <para><strong>Supported Document Activities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>SAVED:</strong> Document saved within matter context</item>
    /// <item><strong>DELETED:</strong> Document deleted from matter</item>
    /// <item><strong>RESTORED:</strong> Document restored to matter</item>
    /// <item><strong>MOVED:</strong> Document moved between matters (tracked as general activity)</item>
    /// <item><strong>COPIED:</strong> Document copied between matters (tracked as general activity)</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Operation Classification:</strong> Clear categorization of document operations within matter context</item>
    /// <item><strong>Professional Accountability:</strong> Tracks the specific nature of document operations</item>
    /// <item><strong>Client Communication:</strong> Enables precise client communication about document operations</item>
    /// <item><strong>Legal Discovery:</strong> Important for understanding document operations and status changes</item>
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
    /// <item><strong>Valid Reference:</strong> Must reference an existing matter document activity</item>
    /// <item><strong>Cross-Reference Consistency:</strong> Must match MatterDocumentActivity.Id when navigation property is provided</item>
    /// <item><strong>Activity Type Validation:</strong> Must represent a valid document activity operation</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUser.MatterDocumentActivityId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations for reliable activity tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Specifying save operation
    /// var saveOperation = new MatterDocumentActivityUserDto
    /// {
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = saveActivityGuid, // References "SAVED" activity
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Professional operation tracking
    /// if (saveOperation.MatterDocumentActivity?.Activity == "SAVED")
    /// {
    ///     Console.WriteLine("Document was saved in matter context");
    /// }
    /// else if (saveOperation.MatterDocumentActivity?.Activity == "DELETED")
    /// {
    ///     Console.WriteLine("Document was deleted from matter");
    /// }
    /// 
    /// // Audit trail classification
    /// var operationType = saveOperation.MatterDocumentActivity?.Activity ?? "UNKNOWN";
    /// Console.WriteLine($"Document activity operation type: {operationType}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter document activity ID is required for operation classification.")]
    public required Guid MatterDocumentActivityId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the user performing the document activity operation.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific user who is 
    /// performing the document activity operation. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivityUser.UserId"/> and provides essential user 
    /// attribution for professional accountability and audit trail completeness.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Fourth component of the five-part composite primary key</item>
    /// <item><strong>User Attribution:</strong> Identifies who performed the document activity operation</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing User entity</item>
    /// <item><strong>Accountability Tracking:</strong> Essential for professional accountability and responsibility</item>
    /// </list>
    /// 
    /// <para><strong>Professional Accountability:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Responsibility:</strong> Establishes clear accountability for document activity decisions</item>
    /// <item><strong>Professional Standards:</strong> Supports professional practice responsibility requirements</item>
    /// <item><strong>Audit Trail Completeness:</strong> Ensures all document activities have clear user attribution</item>
    /// <item><strong>Legal Compliance:</strong> Meets legal requirements for document handling accountability</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Professional Responsibility:</strong> Links document activities to specific legal professionals</item>
    /// <item><strong>Client Communication:</strong> Enables client communication about who handled their documents</item>
    /// <item><strong>Quality Control:</strong> Supports professional oversight and quality assurance processes</item>
    /// <item><strong>Practice Management:</strong> Facilitates practice management and workflow analysis</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Complete Attribution:</strong> Every document activity has clear user attribution</item>
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
    /// <item><strong>Professional Authorization:</strong> User must be authorized to perform document activity operations</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUser.UserId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations for reliable user tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// // User attribution for document activity
    /// var activityWithUser = new MatterDocumentActivityUserDto
    /// {
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = Guid.Parse("50000000-0000-0000-0000-000000000001"), // Robert Brown
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Professional accountability reporting
    /// Console.WriteLine($"Document activity performed by user ID: {activityWithUser.UserId}");
    /// 
    /// // Professional attribution display
    /// if (activityWithUser.User != null)
    /// {
    ///     Console.WriteLine($"Professional: {activityWithUser.User.Name} " +
    ///                      $"performed document activity at {activityWithUser.LocalCreatedAtDateString}");
    /// }
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User ID is required for professional accountability in document activities.")]
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the document activity operation was performed.
    /// </summary>
    /// <remarks>
    /// This DateTime serves as the final component of the composite primary key and records the precise 
    /// moment when the document activity operation was completed. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivityUser.CreatedAt"/> and provides essential 
    /// temporal tracking for audit trails, legal compliance, and professional accountability.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Fifth and final component of the composite primary key</item>
    /// <item><strong>Temporal Foundation:</strong> Establishes precise timing for document activity operations</item>
    /// <item><strong>Audit Trail Chronology:</strong> Enables chronological sequencing of document operations</item>
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
    /// <item><strong>Professional Timeline:</strong> Establishes professional timeline for document handling operations</item>
    /// <item><strong>Legal Discovery:</strong> Provides precise timing information for legal discovery requirements</item>
    /// <item><strong>Client Communication:</strong> Enables accurate client communication about when operations occurred</item>
    /// <item><strong>Compliance Documentation:</strong> Meets professional and regulatory temporal documentation requirements</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Chronological Ordering:</strong> Enables proper chronological ordering of document operations</item>
    /// <item><strong>Temporal Analysis:</strong> Supports temporal analysis of document activity patterns</item>
    /// <item><strong>Professional Reporting:</strong> Provides temporal foundation for professional activity reports</item>
    /// <item><strong>Compliance Auditing:</strong> Supports compliance auditing with precise temporal tracking</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements (via ADMS.API.Common.MatterDocumentActivityValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Valid Range:</strong> Between January 1, 1980, and current time (with tolerance for system delays)</item>
    /// <item><strong>Not Future:</strong> Cannot be in the future to prevent temporal inconsistencies</item>
    /// <item><strong>Not Default:</strong> Must be a valid DateTime, not DateTime.MinValue or default values</item>
    /// <item><strong>Professional Reasonableness:</strong> Must be reasonable for professional legal practice context</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUser.CreatedAt"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations for reliable temporal tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Setting precise activity timestamp
    /// var timestampedActivity = new MatterDocumentActivityUserDto
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
    /// 
    /// // Professional display for client communication
    /// Console.WriteLine($"Document activity performed on: {timestampedActivity.LocalCreatedAtDateString}");
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
    /// Gets the comprehensive matter details for the matter providing context for the document activity.
    /// </summary>
    /// <remarks>
    /// This property provides complete matter information for the matter context in the document 
    /// activity operation, corresponding to the <see cref="ADMS.API.Entities.MatterDocumentActivityUser.Matter"/> 
    /// navigation property. It uses MatterWithoutDocumentsDto to provide comprehensive matter information 
    /// while avoiding potential circular references with document collections.
    /// 
    /// <para><strong>Matter Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Context:</strong> Complete information about the matter context for the document activity</item>
    /// <item><strong>Matter Organization:</strong> Provides context for matter-based document activity organization</item>
    /// <item><strong>Professional Display:</strong> Enables professional presentation of activity context</item>
    /// <item><strong>Client Communication:</strong> Supports client communication about document activities within matters</item>
    /// </list>
    /// 
    /// <para><strong>Comprehensive Matter Information Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Matter Identification:</strong> Complete matter description and identification information</item>
    /// <item><strong>Matter Status:</strong> Current matter status (active, archived, etc.) for context</item>
    /// <item><strong>Activity Relationships:</strong> Matter activity associations for comprehensive audit context</item>
    /// <item><strong>Professional Metadata:</strong> Creation dates, status flags, and professional information</item>
    /// </list>
    /// 
    /// <para><strong>Professional Practice Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Matter-Based Organization:</strong> Supports professional matter-based document activity organization strategies</item>
    /// <item><strong>Client Service:</strong> Enables effective client communication about document activities</item>
    /// <item><strong>Professional Display:</strong> Provides professional presentation of matter information</item>
    /// <item><strong>Quality Assurance:</strong> Enables validation of appropriate matter context for activities</item>
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
    /// var activityWithMatter = new MatterDocumentActivityUserDto
    /// {
    ///     MatterId = matterGuid,
    ///     Matter = new MatterWithoutDocumentsDto
    ///     {
    ///         Id = matterGuid,
    ///         Description = "Client ABC - Legal Research",
    ///         IsArchived = false,
    ///         CreationDate = DateTime.UtcNow.AddMonths(-2)
    ///     },
    ///     DocumentId = documentGuid,
    ///     // ... other properties
    /// };
    /// 
    /// // Professional audit trail reporting
    /// if (activityWithMatter.Matter != null)
    /// {
    ///     Console.WriteLine($"Document activity in matter: '{activityWithMatter.Matter.Description}' " +
    ///                      $"(Status: {activityWithMatter.Matter.Status})");
    /// }
    /// 
    /// // Professional client communication
    /// var matterDescription = activityWithMatter.Matter?.DisplayText ?? "Unknown Matter";
    /// Console.WriteLine($"Your document activity occurred in: {matterDescription}");
    /// </code>
    /// </example>
    public MatterWithoutDocumentsDto? Matter { get; init; }

    /// <summary>
    /// Gets the comprehensive document details for the document being operated on in the activity.
    /// </summary>
    /// <remarks>
    /// This property provides complete document information for the document being operated on in the activity, 
    /// corresponding to the <see cref="ADMS.API.Entities.MatterDocumentActivityUser.Document"/> navigation property. 
    /// It uses DocumentWithoutRevisionsDto to provide comprehensive document information while optimizing 
    /// performance by excluding detailed revision history.
    /// 
    /// <para><strong>Activity Document Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Identification:</strong> Complete information about which document is being operated on</item>
    /// <item><strong>Professional Display:</strong> Enables professional presentation of document information</item>
    /// <item><strong>Client Communication:</strong> Supports client communication about specific document activities</item>
    /// <item><strong>Audit Trail Completeness:</strong> Provides complete document context for audit trails</item>
    /// </list>
    /// 
    /// <para><strong>Comprehensive Document Information Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Identity:</strong> File names, descriptions, and unique identification</item>
    /// <item><strong>Document Metadata:</strong> File sizes, creation dates, modification information</item>
    /// <item><strong>Document Status:</strong> Current document status and availability information</item>
    /// <item><strong>Activity Associations:</strong> Document activity relationships for comprehensive context</item>
    /// </list>
    /// 
    /// <para><strong>Activity Operation Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>SAVE Operations:</strong> Document being saved within matter context</item>
    /// <item><strong>DELETE Operations:</strong> Document being deleted from matter</item>
    /// <item><strong>RESTORE Operations:</strong> Document being restored to matter</item>
    /// <item><strong>Professional Tracking:</strong> Enables precise tracking of specific document operations</item>
    /// <item><strong>Legal Discovery:</strong> Provides document details for legal discovery requirements</item>
    /// </list>
    /// 
    /// <para><strong>Professional Practice Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Management:</strong> Supports professional document management and organization</item>
    /// <item><strong>Client Service:</strong> Enables effective client communication about their documents</item>
    /// <item><strong>Quality Control:</strong> Provides document validation and quality assurance capabilities</item>
    /// <item><strong>Professional Responsibility:</strong> Supports professional responsibility for document handling</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Conditional Requirement:</strong> Should be provided for complete audit trail context</item>
    /// <item><strong>Cross-Reference Consistency:</strong> Document.Id must match DocumentId when both are provided</item>
    /// <item><strong>Entity Validity:</strong> Must pass DocumentWithoutRevisionsDto validation when provided</item>
    /// <item><strong>Activity Eligibility:</strong> Document must be eligible for the specific activity operation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Complete document information in activity
    /// var activityWithDocument = new MatterDocumentActivityUserDto
    /// {
    ///     DocumentId = documentGuid,
    ///     Document = new DocumentWithoutRevisionsDto
    ///     {
    ///         Id = documentGuid,
    ///         FileName = "Research_Notes_v3.docx",
    ///         FileSize = 1245678,
    ///         CreationDate = DateTime.UtcNow.AddDays(-5),
    ///         IsCheckedOut = false
    ///     },
    ///     MatterId = matterGuid,
    ///     // ... other properties
    /// };
    /// 
    /// // Professional audit trail reporting
    /// if (activityWithDocument.Document != null)
    /// {
    ///     Console.WriteLine($"Document '{activityWithDocument.Document.FileName}' " +
    ///                      $"({activityWithDocument.Document.FileSize:N0} bytes) " +
    ///                      $"activity tracked in matter context");
    /// }
    /// 
    /// // Client communication
    /// var documentName = activityWithDocument.Document?.FileName ?? "Unknown Document";
    /// Console.WriteLine($"Activity on your document '{documentName}' has been recorded");
    /// </code>
    /// </example>
    public DocumentWithoutRevisionsDto? Document { get; init; }

    /// <summary>
    /// Gets the comprehensive matter document activity details describing the type of activity operation.
    /// </summary>
    /// <remarks>
    /// This property provides complete activity information for the document activity operation being performed, 
    /// corresponding to the <see cref="ADMS.API.Entities.MatterDocumentActivityUser.MatterDocumentActivity"/> 
    /// navigation property. It includes comprehensive activity data and relationships for complete audit 
    /// trail management and professional compliance.
    /// 
    /// <para><strong>Activity Classification:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Operation Type Identification:</strong> Clearly identifies the type of document activity</item>
    /// <item><strong>Professional Standards:</strong> Maintains professional activity classification standards</item>
    /// <item><strong>Audit Trail Integration:</strong> Integrates with broader activity tracking and audit systems</item>
    /// <item><strong>Compliance Reporting:</strong> Supports detailed compliance reporting requirements</item>
    /// </list>
    /// 
    /// <para><strong>Supported Document Activities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>SAVED:</strong> Document saved within matter context</item>
    /// <item><strong>DELETED:</strong> Document deleted from matter</item>
    /// <item><strong>RESTORED:</strong> Document restored to matter</item>
    /// <item><strong>MOVED:</strong> Document moved between matters (general activity tracking)</item>
    /// <item><strong>COPIED:</strong> Document copied between matters (general activity tracking)</item>
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
    /// <item><strong>Professional Classification:</strong> Clear categorization of document operations</item>
    /// <item><strong>Client Communication:</strong> Enables precise client communication about document operations</item>
    /// <item><strong>Legal Discovery:</strong> Important for understanding document operations and status changes</item>
    /// <item><strong>Professional Accountability:</strong> Clear classification supports professional responsibility</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Conditional Requirement:</strong> Should be provided for complete operation classification</item>
    /// <item><strong>Cross-Reference Consistency:</strong> MatterDocumentActivity.Id must match MatterDocumentActivityId when both provided</item>
    /// <item><strong>Entity Validity:</strong> Must pass MatterDocumentActivityDto validation when provided</item>
    /// <item><strong>Operation Validity:</strong> Must represent a valid document activity operation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Complete activity information
    /// var activityWithClassification = new MatterDocumentActivityUserDto
    /// {
    ///     MatterDocumentActivityId = activityGuid,
    ///     MatterDocumentActivity = new MatterDocumentActivityDto
    ///     {
    ///         Id = activityGuid,
    ///         Activity = "SAVED"
    ///     },
    ///     MatterId = matterGuid,
    ///     // ... other properties
    /// };
    /// 
    /// // Professional operation classification
    /// if (activityWithClassification.MatterDocumentActivity != null)
    /// {
    ///     var operationType = activityWithClassification.MatterDocumentActivity.Activity;
    ///     var operationDescription = operationType switch
    ///     {
    ///         "SAVED" => "saved",
    ///         "DELETED" => "deleted from",
    ///         "RESTORED" => "restored to",
    ///         _ => "performed unknown operation on"
    ///     };
    ///     
    ///     Console.WriteLine($"User {operationDescription} the document in matter context");
    /// }
    /// 
    /// // Professional compliance reporting
    /// var activityType = activityWithClassification.MatterDocumentActivity?.Activity ?? "UNKNOWN";
    /// Console.WriteLine($"Document activity operation type: {activityType}");
    /// </code>
    /// </example>
    public MatterDocumentActivityDto? MatterDocumentActivity { get; init; }

    /// <summary>
    /// Gets the comprehensive user details for the user who performed the document activity operation.
    /// </summary>
    /// <remarks>
    /// This property provides complete user information for the professional who performed the document 
    /// activity operation, corresponding to the <see cref="ADMS.API.Entities.MatterDocumentActivityUser.User"/> 
    /// navigation property. It includes comprehensive user data and activity relationships for complete 
    /// professional accountability and audit trail management.
    /// 
    /// <para><strong>Professional Accountability Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Attribution:</strong> Complete professional attribution for document activity operations</item>
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
    /// <item><strong>Professional Authorization:</strong> User must be authorized for document activity operations</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Complete user information in activity
    /// var activityWithUser = new MatterDocumentActivityUserDto
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
    ///     Console.WriteLine($"Document activity performed by: {activityWithUser.User.DisplayName} " +
    ///                      $"on {activityWithUser.LocalCreatedAtDateString}");
    /// }
    /// 
    /// // Professional client communication
    /// var professionalName = activityWithUser.User?.DisplayName ?? "System Administrator";
    /// Console.WriteLine($"Your document was professionally handled by {professionalName}");
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
    /// var activity = new MatterDocumentActivityUserDto 
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
    /// Console.WriteLine($"Your document activity occurred on {activity.LocalCreatedAtDateString}");
    /// </code>
    /// </example>
    public string LocalCreatedAtDateString => CreatedAt.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the activity operation summary for professional audit trail display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a concise, professional summary of the document activity operation 
    /// including key participants and operation details, optimized for audit trail reports and professional 
    /// communication.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activitySummary = activity.ActivitySummary;
    /// // Returns: "Research_Notes.docx SAVED in Client ABC Matter by Robert Brown"
    /// 
    /// Console.WriteLine($"Activity Summary: {activitySummary}");
    /// </code>
    /// </example>
    public string ActivitySummary =>
        $"{Document?.FileName ?? "Document"} " +
        $"{MatterDocumentActivity?.Activity ?? "ACTIVITY"} " +
        $"in {Matter?.Description ?? "Matter"} " +
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
            OperationType = MatterDocumentActivity?.Activity ?? "UNKNOWN",
            ActivityContext = "General Document Activity"
        },
        ParticipantInfo = new
        {
            MatterContext = Matter?.Description ?? "Unknown Matter",
            DocumentName = Document?.FileName ?? "Unknown Document",
            UserName = User?.Name ?? "Unknown User",
            UserId,
            MatterId,
            DocumentId
        },
        TemporalInfo = new
        {
            CreatedAt,
            LocalCreatedAtDateString,
            ActivityAge = (DateTime.UtcNow - CreatedAt).TotalDays
        },
        ValidationInfo = new
        {
            HasCompleteInformation = Matter != null && Document != null &&
                                   MatterDocumentActivity != null && User != null,
            RequiredFieldsPresent = MatterId != Guid.Empty && DocumentId != Guid.Empty &&
                                  MatterDocumentActivityId != Guid.Empty && UserId != Guid.Empty
        }
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="MatterDocumentActivityUserDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using centralized validation helpers for consistency with entity 
    /// validation rules. This ensures the DTO maintains the same validation standards as the corresponding 
    /// ADMS.API.Entities.MatterDocumentActivityUser entity while enforcing audit trail-specific business rules.
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
    /// Uses centralized validation helpers (MatterDocumentActivityValidationHelper, DtoValidationHelper) 
    /// to ensure consistency across all audit trail validation in the system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityUserDto 
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

        foreach (var result in ValidateDocument())
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
    /// Validates the <see cref="Document"/> navigation property using centralized validation.
    /// </summary>
    /// <returns>A collection of validation results for document validation.</returns>
    /// <remarks>
    /// Uses DtoValidationHelper for consistent validation when the Document navigation property is provided, 
    /// ensuring comprehensive document information validation.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateDocument()
    {
        if (Document is not IValidatableObject validatable) yield break;
        var context = new ValidationContext(Document);
        foreach (var result in validatable.Validate(context))
            yield return new ValidationResult($"Activity Document: {result.ErrorMessage}", result.MemberNames);
    }

    /// <summary>
    /// Validates the <see cref="MatterDocumentActivity"/> navigation property using centralized validation.
    /// </summary>
    /// <returns>A collection of validation results for activity validation.</returns>
    /// <remarks>
    /// Uses DtoValidationHelper for consistent validation when the MatterDocumentActivity navigation 
    /// property is provided, ensuring comprehensive activity classification validation.
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
    /// Validates the <see cref="CreatedAt"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for timestamp validation.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.MatterDocumentActivityValidationHelper.IsValidDate for comprehensive temporal 
    /// validation, ensuring the timestamp meets professional standards and audit trail requirements.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateCreatedAt()
    {
        // Replace IsValidDate with direct DateTime validation logic
        if (CreatedAt > DateTime.UtcNow)
            yield return new ValidationResult(
                "CreatedAt must not be in the future for audit trail integrity.",
                [nameof(CreatedAt)]);

        if (CreatedAt < new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            yield return new ValidationResult(
                "CreatedAt is unreasonably far in the past for a document activity.",
                [nameof(CreatedAt)]);

        if (CreatedAt == DateTime.MinValue || CreatedAt == default)
            yield return new ValidationResult(
                "CreatedAt must be a valid date and not default value for audit trail integrity.",
                [nameof(CreatedAt)]);
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

        if (Document != null && Document.Id != DocumentId)
            yield return new ValidationResult(
                "Document.Id does not match DocumentId - referential integrity violation.",
                [nameof(Document), nameof(DocumentId)]);

        if (MatterDocumentActivity != null && MatterDocumentActivity.Id != MatterDocumentActivityId)
            yield return new ValidationResult(
                "MatterDocumentActivity.Id does not match MatterDocumentActivityId - referential integrity violation.",
                [nameof(MatterDocumentActivity), nameof(MatterDocumentActivityId)]);

        if (User != null && User.Id != UserId)
            yield return new ValidationResult(
                "User.Id does not match UserId - referential integrity violation.",
                [nameof(User), nameof(UserId)]);
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="MatterDocumentActivityUserDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The MatterDocumentActivityUserDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate MatterDocumentActivityUserDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance Validate method 
    /// but with null-safety and simplified usage for audit trail validation.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>API Validation:</strong> Validating incoming activity audit DTOs in API controllers</item>
    /// <item><strong>Service Layer:</strong> Validation before processing document activity operations</item>
    /// <item><strong>Unit Testing:</strong> Simplified validation testing without ValidationContext</item>
    /// <item><strong>Batch Processing:</strong> Validating collections of activity audit entries</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityUserDto 
    /// { 
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// var results = MatterDocumentActivityUserDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Activity audit validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterDocumentActivityUserDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterDocumentActivityUserDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a MatterDocumentActivityUserDto from ADMS.API.Entities.MatterDocumentActivityUser entity with validation.
    /// </summary>
    /// <param name="entity">The MatterDocumentActivityUser entity to convert. Cannot be null.</param>
    /// <param name="includeNavigationProperties">Whether to include navigation properties in the conversion.</param>
    /// <returns>A valid MatterDocumentActivityUserDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create MatterDocumentActivityUserDto instances from
    /// ADMS.API.Entities.MatterDocumentActivityUser entities with automatic validation. It ensures the 
    /// resulting DTO is valid before returning it.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity with complete information
    /// var entity = new ADMS.API.Entities.MatterDocumentActivityUser 
    /// { 
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// var dto = MatterDocumentActivityUserDto.FromEntity(entity, includeNavigationProperties: true);
    /// </code>
    /// </example>
    public static MatterDocumentActivityUserDto FromEntity([NotNull] Entities.MatterDocumentActivityUser entity, bool includeNavigationProperties = false)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var dto = new MatterDocumentActivityUserDto
        {
            MatterId = entity.MatterId,
            DocumentId = entity.DocumentId,
            MatterDocumentActivityId = entity.MatterDocumentActivityId,
            UserId = entity.UserId,
            CreatedAt = entity.CreatedAt
        };

        // Note: In practice, navigation properties would be mapped using a mapping framework
        // like AutoMapper or Mapster when includeNavigationProperties is true

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterDocumentActivityUserDto from entity: {errorMessages}");

    }

    /// <summary>
    /// Creates multiple MatterDocumentActivityUserDto instances from a collection of entities.
    /// </summary>
    /// <param name="entities">The collection of MatterDocumentActivityUser entities to convert. Cannot be null.</param>
    /// <param name="includeNavigationProperties">Whether to include navigation properties in the conversion.</param>
    /// <returns>A collection of valid MatterDocumentActivityUserDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method is optimized for creating multiple activity audit DTOs efficiently.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convert collection of activity audit entities
    /// var entities = await context.MatterDocumentActivityUsers.ToListAsync();
    /// var activityAuditDtos = MatterDocumentActivityUserDto.FromEntities(entities, includeNavigationProperties: false);
    /// 
    /// // Process audit trail
    /// foreach (var auditDto in activityAuditDtos)
    /// {
    ///     ProcessActivityAudit(auditDto);
    /// }
    /// </code>
    /// </example>
    public static IList<MatterDocumentActivityUserDto> FromEntities([NotNull] IEnumerable<Entities.MatterDocumentActivityUser> entities, bool includeNavigationProperties = false)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        var result = new List<MatterDocumentActivityUserDto>();

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
                Console.WriteLine($"Warning: Skipped invalid activity audit entity: {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a document activity audit entry with specified parameters and comprehensive validation.
    /// </summary>
    /// <param name="matterId">The matter ID providing context.</param>
    /// <param name="documentId">The document ID being operated on.</param>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="userId">The user performing the activity.</param>
    /// <param name="timestamp">Optional timestamp (defaults to current UTC time).</param>
    /// <returns>A valid MatterDocumentActivityUserDto instance.</returns>
    /// <exception cref="ArgumentException">Thrown when any GUID parameter is empty.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a convenient way to create activity audit entries with all required
    /// parameters while ensuring validation compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create activity audit entry
    /// var activityAudit = MatterDocumentActivityUserDto.CreateActivityAudit(
    ///     matterGuid,
    ///     documentGuid,
    ///     saveActivityGuid,
    ///     userGuid);
    /// 
    /// // Save to audit system
    /// await auditService.RecordActivityAsync(activityAudit);
    /// </code>
    /// </example>
    public static MatterDocumentActivityUserDto CreateActivityAudit(
        Guid matterId,
        Guid documentId,
        Guid activityId,
        Guid userId,
        DateTime? timestamp = null)
    {
        if (matterId == Guid.Empty)
            throw new ArgumentException("Matter ID cannot be empty.", nameof(matterId));
        if (documentId == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty.", nameof(documentId));
        if (activityId == Guid.Empty)
            throw new ArgumentException("Activity ID cannot be empty.", nameof(activityId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        var dto = new MatterDocumentActivityUserDto
        {
            MatterId = matterId,
            DocumentId = documentId,
            MatterDocumentActivityId = activityId,
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
    /// Determines whether this activity represents a document save operation.
    /// </summary>
    /// <returns>true if this represents a SAVED operation; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify save operations for activity analysis and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityAudit.IsSaveOperation())
    /// {
    ///     Console.WriteLine($"Document saved in matter context: {activityAudit.Matter?.Description}");
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
    /// This method helps identify transfer operations for audit analysis. Note that specific directional
    /// transfer tracking should use MatterDocumentActivityUserFromDto and MatterDocumentActivityUserToDto.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityAudit.IsTransferOperation())
    /// {
    ///     Console.WriteLine($"Document transfer operation in matter: {activityAudit.Matter?.Description}");
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
    /// Determines whether this activity represents a document deletion operation.
    /// </summary>
    /// <returns>true if this represents a DELETED operation; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify deletion operations for activity analysis and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activityAudit.IsDeleteOperation())
    /// {
    ///     Console.WriteLine($"Document deleted from matter: {activityAudit.Matter?.Description}");
    /// }
    /// </code>
    /// </example>
    public bool IsDeleteOperation() =>
        string.Equals(MatterDocumentActivity?.Activity, "DELETED", StringComparison.OrdinalIgnoreCase);

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
    ///     Console.WriteLine("Recent document activity detected");
    /// }
    /// </code>
    /// </example>
    public bool IsRecentActivity(int withinDays = 7) => GetActivityAgeDays() <= withinDays;

    /// <summary>
    /// Gets comprehensive activity information for reporting and analysis.
    /// </summary>
    /// <returns>A dictionary containing detailed activity information.</returns>
    /// <remarks>
    /// This method provides structured activity information useful for audit reports,
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
            ["ActivityType"] = MatterDocumentActivity?.Activity ?? "UNKNOWN",
            ["IsSaveOperation"] = IsSaveOperation(),
            ["IsTransferOperation"] = IsTransferOperation(),
            ["IsDeleteOperation"] = IsDeleteOperation(),
            ["MatterContext"] = Matter?.Description ?? "Unknown Matter",
            ["DocumentName"] = Document?.FileName ?? "Unknown Document",
            ["UserName"] = User?.Name ?? "Unknown User",
            ["ActivityDate"] = CreatedAt,
            ["LocalActivityDate"] = LocalCreatedAtDateString,
            ["ActivityAge"] = GetActivityAgeDays(),
            ["IsRecent"] = IsRecentActivity(),
            ["ActivitySummary"] = ActivitySummary,
            ["HasCompleteInformation"] = Matter != null && Document != null &&
                                       MatterDocumentActivity != null && User != null
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
    /// // Returns: "On Monday, 15 January 2024 14:30:45, Robert Brown SAVED Research_Notes.docx in Client ABC Matter"
    /// 
    /// Console.WriteLine(auditMessage);
    /// </code>
    /// </example>
    public string GenerateAuditMessage()
    {
        var operationType = MatterDocumentActivity?.Activity ?? "PERFORMED ACTIVITY ON";
        var documentName = Document?.FileName ?? "document";
        var matterContext = Matter?.Description ?? "matter";
        var userName = User?.Name ?? "user";

        return $"On {LocalCreatedAtDateString}, {userName} {operationType} {documentName} in {matterContext}";
    }

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterDocumentActivityUserDto is equal to the current MatterDocumentActivityUserDto.
    /// </summary>
    /// <param name="other">The MatterDocumentActivityUserDto to compare with the current MatterDocumentActivityUserDto.</param>
    /// <returns>true if the specified MatterDocumentActivityUserDto is equal to the current MatterDocumentActivityUserDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all composite key properties that uniquely identify an activity audit entry.
    /// This follows the same pattern as the entity composite key for consistency.
    /// </remarks>
    public virtual bool Equals(MatterDocumentActivityUserDto? other)
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
    /// <returns>A hash code for the current MatterDocumentActivityUserDto.</returns>
    /// <remarks>
    /// The hash code is based on all composite key properties to ensure consistent hashing behavior
    /// that aligns with the equality implementation and composite key structure.
    /// </remarks>
    public override int GetHashCode()
    {
        return HashCode.Combine(MatterId, DocumentId, MatterDocumentActivityId, UserId, CreatedAt);
    }

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterDocumentActivityUserDto.
    /// </summary>
    /// <returns>A string that represents the current MatterDocumentActivityUserDto.</returns>
    /// <remarks>
    /// The string representation includes key activity audit information for identification and logging purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityUserDto 
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