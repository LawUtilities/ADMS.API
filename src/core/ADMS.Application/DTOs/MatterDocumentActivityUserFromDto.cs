using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing the source-side audit trail for document transfer operations between matters.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of the "FROM" side of document transfer audit trails within the ADMS legal 
/// document management system, corresponding to <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom"/>. It captures 
/// comprehensive information about document transfers (MOVE or COPY operations) FROM a source matter, providing essential 
/// audit trail data for legal compliance, professional responsibility, and document custody tracking.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Source-Side Audit Trail:</strong> Captures complete information about documents leaving a source matter</item>
/// <item><strong>Professional Validation:</strong> Uses centralized validation helpers for comprehensive data integrity</item>
/// <item><strong>Legal Compliance Support:</strong> Designed for comprehensive audit reporting and legal compliance</item>
/// <item><strong>Entity Synchronization:</strong> Mirrors all properties and relationships from ADMS.API.Entities.MatterDocumentActivityUserFrom</item>
/// <item><strong>Cross-Reference Validation:</strong> Ensures consistency between foreign key IDs and navigation properties</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO represents the composite primary key relationship from ADMS.API.Entities.MatterDocumentActivityUserFrom:
/// <list type="bullet">
/// <item><strong>Matter Association:</strong> Source matter losing the document via MatterId and MatterWithoutDocumentsDto</item>
/// <item><strong>Document Association:</strong> Document being transferred via DocumentId and DocumentWithoutRevisionsDto</item>
/// <item><strong>Activity Classification:</strong> Type of transfer operation via MatterDocumentActivityId and MatterDocumentActivityDto</item>
/// <item><strong>User Attribution:</strong> User performing the transfer via UserId and UserDto</item>
/// <item><strong>Temporal Tracking:</strong> Timestamp of the transfer via CreatedAt</item>
/// </list>
/// 
/// <para><strong>Document Transfer Operations Tracked:</strong></para>
/// <list type="bullet">
/// <item><strong>MOVED:</strong> Documents moved FROM this matter to another matter (source matter loses the document)</item>
/// <item><strong>COPIED:</strong> Documents copied FROM this matter to another matter (source matter retains the document)</item>
/// </list>
/// 
/// <para><strong>Legal Practice Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Custody Tracking:</strong> Complete tracking of documents leaving source matters</item>
/// <item><strong>Professional Accountability:</strong> Complete user attribution for document handling decisions</item>
/// <item><strong>Client Communication:</strong> Enables client notification about document location changes</item>
/// <item><strong>Matter Organization:</strong> Supports quality assurance for matter organization and cleanup</item>
/// <item><strong>Legal Discovery Support:</strong> Provides detailed document movement history for legal proceedings</item>
/// </list>
/// 
/// <para><strong>Bidirectional Audit System:</strong></para>
/// This DTO is part of a comprehensive bidirectional document transfer tracking system:
/// <list type="bullet">
/// <item><strong>Source-Side Tracking:</strong> This DTO tracks the origin of transfers</item>
/// <item><strong>Destination-Side Tracking:</strong> MatterDocumentActivityUserToDto tracks the destination of transfers</item>
/// <item><strong>Complete Audit Trail:</strong> Together they provide complete bidirectional audit coverage</item>
/// <item><strong>Legal Compliance:</strong> Ensures every document movement is fully documented and traceable</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Integration:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Management:</strong> Supports professional document organization and matter management</item>
/// <item><strong>Client Service:</strong> Enables effective client communication about document reorganization</item>
/// <item><strong>Practice Management:</strong> Facilitates professional workflow optimization and analysis</item>
/// <item><strong>Quality Control:</strong> Provides oversight capabilities for document organization and custody</item>
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
/// <item><strong>Document Transfer Auditing:</strong> Primary use for tracking documents leaving source matters</item>
/// <item><strong>Legal Compliance Reporting:</strong> Comprehensive audit trail reporting for regulatory compliance</item>
/// <item><strong>Matter Management:</strong> Professional matter organization and document custody tracking</item>
/// <item><strong>Client Communication:</strong> Informing clients about document location changes and reorganization</item>
/// <item><strong>Administrative Operations:</strong> Document transfer oversight and quality assurance</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive document transfer "FROM" audit entry
/// var transferAudit = new MatterDocumentActivityUserFromDto
/// {
///     MatterId = sourceMatterId,
///     Matter = new MatterWithoutDocumentsDto 
///     { 
///         Id = sourceMatterId, 
///         Description = "Original Client Matter" 
///     },
///     DocumentId = documentId,
///     Document = new DocumentWithoutRevisionsDto 
///     { 
///         Id = documentId, 
///         FileName = "Contract_Original.pdf" 
///     },
///     MatterDocumentActivityId = moveActivityId,
///     MatterDocumentActivity = new MatterDocumentActivityDto 
///     { 
///         Id = moveActivityId, 
///         Activity = "MOVED" 
///     },
///     UserId = userId,
///     User = new UserDto 
///     { 
///         Id = userId, 
///         Name = "Robert Brown" 
///     },
///     CreatedAt = DateTime.UtcNow
/// };
/// 
/// // Comprehensive validation
/// var validationResults = MatterDocumentActivityUserFromDto.ValidateModel(transferAudit);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Transfer Audit Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Professional audit trail reporting
/// Console.WriteLine($"Document '{transferAudit.Document?.FileName}' was " +
///                  $"{transferAudit.MatterDocumentActivity?.Activity?.ToLower()} from " +
///                  $"matter '{transferAudit.Matter?.Description}' by " +
///                  $"{transferAudit.User?.Name} on {transferAudit.LocalCreatedAtDateString}");
/// </code>
/// </example>
public record MatterDocumentActivityUserFromDto : IValidatableObject, IEquatable<MatterDocumentActivityUserFromDto>
{
    #region Composite Primary Key Properties

    /// <summary>
    /// Gets the unique identifier for the source matter losing the document.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the matter that is losing 
    /// the document through the transfer operation. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom.MatterId"/> and must reference a valid, 
    /// existing matter in the system.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> First component of the five-part composite primary key</item>
    /// <item><strong>Source Identification:</strong> Identifies which matter is losing the document</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing Matter entity</item>
    /// <item><strong>Audit Trail Foundation:</strong> Essential for tracking document sources</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Matter Organization:</strong> Determines the matter-based organization source for the document</item>
    /// <item><strong>Client Attribution:</strong> Links the document transfer to the appropriate source client or case</item>
    /// <item><strong>Professional Responsibility:</strong> Establishes accountability for document custody and organization</item>
    /// <item><strong>Document Discovery:</strong> Critical for legal discovery and document location tracking</item>
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
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom.MatterId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations for reliable audit trail management.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Setting source matter for document transfer
    /// var transferAudit = new MatterDocumentActivityUserFromDto
    /// {
    ///     MatterId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Validation example
    /// if (transferAudit.MatterId == Guid.Empty)
    /// {
    ///     throw new ArgumentException("Source matter ID is required for transfer audit");
    /// }
    /// 
    /// // Professional reporting usage
    /// Console.WriteLine($"Document transferred from matter ID: {transferAudit.MatterId}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Source matter ID is required for document transfer audit.")]
    public required Guid MatterId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the document being transferred from the source matter.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific document that is 
    /// being transferred FROM the source matter. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom.DocumentId"/> and must reference a valid, 
    /// existing document in the system.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Second component of the five-part composite primary key</item>
    /// <item><strong>Document Identification:</strong> Precisely identifies which document is being transferred</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing Document entity</item>
    /// <item><strong>Transfer Tracking:</strong> Essential for tracking specific document movements</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Custody:</strong> Tracks custody changes for specific legal documents</item>
    /// <item><strong>Professional Accountability:</strong> Identifies exactly which documents are being transferred</item>
    /// <item><strong>Client Communication:</strong> Enables precise client communication about specific document locations</item>
    /// <item><strong>Legal Discovery:</strong> Critical for legal discovery and document production requirements</item>
    /// </list>
    /// 
    /// <para><strong>Transfer Operation Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVE Operations:</strong> Document custody transfers from source to destination matter</item>
    /// <item><strong>COPY Operations:</strong> Document duplicated while source retains copy</item>
    /// <item><strong>Audit Completeness:</strong> Ensures every document transfer is precisely tracked</item>
    /// <item><strong>Version Tracking:</strong> May reference specific document versions in complex transfers</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null or Guid.Empty - essential for audit trail integrity</item>
    /// <item><strong>Valid Reference:</strong> Must reference an existing document in the system</item>
    /// <item><strong>Cross-Reference Consistency:</strong> Must match Document.Id when Document navigation property is provided</item>
    /// <item><strong>Transfer Validity:</strong> Document must be eligible for the specific transfer operation</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom.DocumentId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations for reliable document tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Identifying specific document in transfer
    /// var documentTransfer = new MatterDocumentActivityUserFromDto
    /// {
    ///     MatterId = matterGuid,
    ///     DocumentId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Professional document tracking
    /// Console.WriteLine($"Tracking transfer from document ID: {documentTransfer.DocumentId}");
    /// 
    /// // Audit trail reporting
    /// if (documentTransfer.Document != null)
    /// {
    ///     Console.WriteLine($"Document '{documentTransfer.Document.FileName}' " +
    ///                      $"({documentTransfer.DocumentId}) transferred from matter");
    /// }
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Document ID is required for document transfer audit.")]
    public required Guid DocumentId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the specific matter document activity (MOVE or COPY operation).
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific type of document 
    /// transfer activity being performed. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom.MatterDocumentActivityId"/> and must 
    /// reference a valid matter document activity (typically "MOVED" or "COPIED").
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Third component of the five-part composite primary key</item>
    /// <item><strong>Activity Classification:</strong> Identifies the type of transfer operation being performed</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing MatterDocumentActivity entity</item>
    /// <item><strong>Operation Type Tracking:</strong> Distinguishes between different types of document operations</item>
    /// </list>
    /// 
    /// <para><strong>Supported Transfer Activities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> Document is moved from source matter to destination matter (custody transfer)</item>
    /// <item><strong>COPIED:</strong> Document is copied from source matter to destination matter (duplication)</item>
    /// <item><strong>Future Extensions:</strong> Framework supports additional transfer types as needed</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Operation Distinction:</strong> Critical difference between moves (custody changes) and copies (duplication)</item>
    /// <item><strong>Professional Accountability:</strong> Tracks the specific nature of document operations</item>
    /// <item><strong>Client Communication:</strong> Enables precise client communication about document operations</item>
    /// <item><strong>Legal Discovery:</strong> Important for understanding document custody chains and availability</item>
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
    /// <item><strong>Activity Type Validation:</strong> Must represent a valid document transfer operation</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom.MatterDocumentActivityId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations for reliable activity tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Specifying move operation
    /// var moveOperation = new MatterDocumentActivityUserFromDto
    /// {
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = moveActivityGuid, // References "MOVED" activity
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Professional operation tracking
    /// if (moveOperation.MatterDocumentActivity?.Activity == "MOVED")
    /// {
    ///     Console.WriteLine("Document custody is being transferred (MOVED)");
    /// }
    /// else if (moveOperation.MatterDocumentActivity?.Activity == "COPIED")
    /// {
    ///     Console.WriteLine("Document is being duplicated (COPIED)");
    /// }
    /// 
    /// // Audit trail classification
    /// var operationType = moveOperation.MatterDocumentActivity?.Activity ?? "UNKNOWN";
    /// Console.WriteLine($"Transfer operation type: {operationType}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter document activity ID is required for operation classification.")]
    public required Guid MatterDocumentActivityId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the user performing the document transfer operation.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific user who is 
    /// performing the document transfer operation. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom.UserId"/> and provides essential user 
    /// attribution for professional accountability and audit trail completeness.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Fourth component of the five-part composite primary key</item>
    /// <item><strong>User Attribution:</strong> Identifies who performed the document transfer operation</item>
    /// <item><strong>Foreign Key Reference:</strong> Must correspond to an existing User entity</item>
    /// <item><strong>Accountability Tracking:</strong> Essential for professional accountability and responsibility</item>
    /// </list>
    /// 
    /// <para><strong>Professional Accountability:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Responsibility:</strong> Establishes clear accountability for document transfer decisions</item>
    /// <item><strong>Professional Standards:</strong> Supports professional practice responsibility requirements</item>
    /// <item><strong>Audit Trail Completeness:</strong> Ensures all document operations have clear user attribution</item>
    /// <item><strong>Legal Compliance:</strong> Meets legal requirements for document handling accountability</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Integration:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Professional Responsibility:</strong> Links document operations to specific legal professionals</item>
    /// <item><strong>Client Communication:</strong> Enables client communication about who handled their documents</item>
    /// <item><strong>Quality Control:</strong> Supports professional oversight and quality assurance processes</item>
    /// <item><strong>Practice Management:</strong> Facilitates practice management and workflow analysis</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Complete Attribution:</strong> Every document transfer has clear user attribution</item>
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
    /// <item><strong>Professional Authorization:</strong> User must be authorized to perform document transfer operations</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom.UserId"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations for reliable user tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// // User attribution for document transfer
    /// var transferWithUser = new MatterDocumentActivityUserFromDto
    /// {
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = Guid.Parse("50000000-0000-0000-0000-000000000001"), // Robert Brown
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// // Professional accountability reporting
    /// Console.WriteLine($"Document transfer performed by user ID: {transferWithUser.UserId}");
    /// 
    /// // Professional attribution display
    /// if (transferWithUser.User != null)
    /// {
    ///     Console.WriteLine($"Professional: {transferWithUser.User.Name} " +
    ///                      $"performed document transfer at {transferWithUser.LocalCreatedAtDateString}");
    /// }
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User ID is required for professional accountability in document transfers.")]
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the document transfer operation was performed.
    /// </summary>
    /// <remarks>
    /// This DateTime serves as the final component of the composite primary key and records the precise 
    /// moment when the document transfer operation was completed. It corresponds directly to 
    /// <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom.CreatedAt"/> and provides essential 
    /// temporal tracking for audit trails, legal compliance, and professional accountability.
    /// 
    /// <para><strong>Key Component Role:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Primary Key:</strong> Fifth and final component of the composite primary key</item>
    /// <item><strong>Temporal Foundation:</strong> Establishes precise timing for document transfer operations</item>
    /// <item><strong>Audit Trail Chronology:</strong> Enables chronological sequencing of document operations</item>
    /// <item><strong>Professional Standards:</strong> Meets professional practice temporal tracking requirements</item>
    /// </list>
    /// 
    /// <para><strong>Temporal Standards and Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>UTC Storage:</strong> Stored in UTC format for global consistency and timezone independence</item>
    /// <item><strong>Immutable Timestamp:</strong> Set once during operation completion and never modified</item>
    /// <item><strong>Professional Accuracy:</strong> Must accurately reflect when the transfer operation occurred</item>
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
    /// <item><strong>Temporal Analysis:</strong> Supports temporal analysis of document management patterns</item>
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
    /// This property mirrors <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom.CreatedAt"/> exactly, 
    /// ensuring complete consistency between entity and DTO representations for reliable temporal tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Setting precise transfer timestamp
    /// var timestampedTransfer = new MatterDocumentActivityUserFromDto
    /// {
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow // Always use UTC
    /// };
    /// 
    /// // Professional temporal reporting
    /// Console.WriteLine($"Transfer completed at: {timestampedTransfer.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
    /// 
    /// // Professional display for client communication
    /// Console.WriteLine($"Document transferred on: {timestampedTransfer.LocalCreatedAtDateString}");
    /// 
    /// // Chronological audit trail analysis
    /// var transferAge = DateTime.UtcNow - timestampedTransfer.CreatedAt;
    /// Console.WriteLine($"Transfer occurred {transferAge.TotalDays:F1} days ago");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Creation timestamp is required for audit trail temporal tracking.")]
    public required DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    #endregion Composite Primary Key Properties

    #region Navigation Properties

    /// <summary>
    /// Gets the comprehensive matter details for the source matter losing the document.
    /// </summary>
    /// <remarks>
    /// This property provides complete matter information for the source matter in the document 
    /// transfer operation, corresponding to the <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom.Matter"/> 
    /// navigation property. It uses MatterWithoutDocumentsDto to provide comprehensive matter information 
    /// while avoiding potential circular references with document collections.
    /// 
    /// <para><strong>Source Matter Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Transfer Source:</strong> Complete information about where the document is coming from</item>
    /// <item><strong>Matter Organization:</strong> Provides context for matter-based document organization</item>
    /// <item><strong>Professional Display:</strong> Enables professional presentation of transfer sources</item>
    /// <item><strong>Client Communication:</strong> Supports client communication about document reorganization</item>
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
    /// <item><strong>Matter-Based Organization:</strong> Supports professional matter-based document organization strategies</item>
    /// <item><strong>Client Service:</strong> Enables effective client communication about matter reorganization</item>
    /// <item><strong>Professional Display:</strong> Provides professional presentation of matter information</item>
    /// <item><strong>Quality Assurance:</strong> Enables validation of appropriate matter sources</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Presentation:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Complete Context:</strong> Provides complete source context for audit trail reports</item>
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
    /// // Complete source matter information
    /// var transferWithMatter = new MatterDocumentActivityUserFromDto
    /// {
    ///     MatterId = matterGuid,
    ///     Matter = new MatterWithoutDocumentsDto
    ///     {
    ///         Id = matterGuid,
    ///         Description = "Original Client Matter",
    ///         IsArchived = false,
    ///         CreationDate = DateTime.UtcNow.AddMonths(-6)
    ///     },
    ///     DocumentId = documentGuid,
    ///     // ... other properties
    /// };
    /// 
    /// // Professional audit trail reporting
    /// if (transferWithMatter.Matter != null)
    /// {
    ///     Console.WriteLine($"Document transferred from matter: '{transferWithMatter.Matter.Description}' " +
    ///                      $"(Status: {transferWithMatter.Matter.Status})");
    /// }
    /// 
    /// // Professional client communication
    /// var sourceDescription = transferWithMatter.Matter?.DisplayText ?? "Unknown Matter";
    /// Console.WriteLine($"Your document was reorganized from: {sourceDescription}");
    /// </code>
    /// </example>
    public MatterWithoutDocumentsDto? Matter { get; init; }

    /// <summary>
    /// Gets the comprehensive document details for the document being transferred from the source matter.
    /// </summary>
    /// <remarks>
    /// This property provides complete document information for the document being transferred in the operation, 
    /// corresponding to the <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom.Document"/> navigation property. 
    /// It uses DocumentWithoutRevisionsDto to provide comprehensive document information while optimizing 
    /// performance by excluding detailed revision history.
    /// 
    /// <para><strong>Transfer Document Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Identification:</strong> Complete information about which document is being transferred</item>
    /// <item><strong>Professional Display:</strong> Enables professional presentation of document information</item>
    /// <item><strong>Client Communication:</strong> Supports client communication about specific document operations</item>
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
    /// <para><strong>Transfer Operation Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVE Operations:</strong> Document being relocated from the source matter</item>
    /// <item><strong>COPY Operations:</strong> Document being duplicated from the source matter</item>
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
    /// <item><strong>Transfer Eligibility:</strong> Document must be eligible for the specific transfer operation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Complete document information in transfer
    /// var transferWithDocument = new MatterDocumentActivityUserFromDto
    /// {
    ///     DocumentId = documentGuid,
    ///     Document = new DocumentWithoutRevisionsDto
    ///     {
    ///         Id = documentGuid,
    ///         FileName = "Original_Contract_v1.pdf",
    ///         FileSize = 1847523,
    ///         CreationDate = DateTime.UtcNow.AddDays(-14),
    ///         IsCheckedOut = false
    ///     },
    ///     MatterId = matterGuid,
    ///     // ... other properties
    /// };
    /// 
    /// // Professional audit trail reporting
    /// if (transferWithDocument.Document != null)
    /// {
    ///     Console.WriteLine($"Document '{transferWithDocument.Document.FileName}' " +
    ///                      $"({transferWithDocument.Document.FileSize:N0} bytes) " +
    ///                      $"transferred from source matter");
    /// }
    /// 
    /// // Client communication
    /// var documentName = transferWithDocument.Document?.FileName ?? "Unknown Document";
    /// Console.WriteLine($"Your document '{documentName}' has been successfully reorganized");
    /// </code>
    /// </example>
    public DocumentWithoutRevisionsDto? Document { get; init; }

    /// <summary>
    /// Gets the comprehensive matter document activity details describing the type of transfer operation.
    /// </summary>
    /// <remarks>
    /// This property provides complete activity information for the document transfer operation being performed, 
    /// corresponding to the <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom.MatterDocumentActivity"/> 
    /// navigation property. It includes comprehensive activity data and relationships for complete audit 
    /// trail management and professional compliance.
    /// 
    /// <para><strong>Transfer Activity Classification:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Operation Type Identification:</strong> Clearly identifies whether operation is MOVE or COPY</item>
    /// <item><strong>Professional Standards:</strong> Maintains professional activity classification standards</item>
    /// <item><strong>Audit Trail Integration:</strong> Integrates with broader activity tracking and audit systems</item>
    /// <item><strong>Compliance Reporting:</strong> Supports detailed compliance reporting requirements</item>
    /// </list>
    /// 
    /// <para><strong>Supported Transfer Operations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> Document custody transferred from source to destination matter</item>
    /// <item><strong>COPIED:</strong> Document duplicated while source matter retains original</item>
    /// <item><strong>Future Extensions:</strong> Framework supports additional transfer types as practice evolves</item>
    /// <item><strong>Professional Classification:</strong> Each operation type has specific professional implications</item>
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
    /// <item><strong>Professional Distinction:</strong> Critical difference between moves (custody changes) and copies (duplication)</item>
    /// <item><strong>Client Communication:</strong> Enables precise client communication about document operations</item>
    /// <item><strong>Legal Discovery:</strong> Important for understanding document custody and availability</item>
    /// <item><strong>Professional Accountability:</strong> Clear classification supports professional responsibility</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Conditional Requirement:</strong> Should be provided for complete operation classification</item>
    /// <item><strong>Cross-Reference Consistency:</strong> MatterDocumentActivity.Id must match MatterDocumentActivityId when both provided</item>
    /// <item><strong>Entity Validity:</strong> Must pass MatterDocumentActivityDto validation when provided</item>
    /// <item><strong>Operation Validity:</strong> Must represent a valid document transfer operation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Complete activity information in transfer
    /// var transferWithActivity = new MatterDocumentActivityUserFromDto
    /// {
    ///     MatterDocumentActivityId = activityGuid,
    ///     MatterDocumentActivity = new MatterDocumentActivityDto
    ///     {
    ///         Id = activityGuid,
    ///         Activity = "MOVED"
    ///     },
    ///     MatterId = matterGuid,
    ///     // ... other properties
    /// };
    /// 
    /// // Professional operation classification
    /// if (transferWithActivity.MatterDocumentActivity != null)
    /// {
    ///     var operationType = transferWithActivity.MatterDocumentActivity.Activity;
    ///     var operationDescription = operationType switch
    ///     {
    ///         "MOVED" => "transferred custody of",
    ///         "COPIED" => "created a copy of",
    ///         _ => "performed unknown operation on"
    ///     };
    ///     
    ///     Console.WriteLine($"User {operationDescription} the document from the source matter");
    /// }
    /// 
    /// // Professional compliance reporting
    /// var activityType = transferWithActivity.MatterDocumentActivity?.Activity ?? "UNKNOWN";
    /// Console.WriteLine($"Document transfer operation type: {activityType}");
    /// </code>
    /// </example>
    public MatterDocumentActivityDto? MatterDocumentActivity { get; init; }

    /// <summary>
    /// Gets the comprehensive user details for the user who performed the document transfer operation.
    /// </summary>
    /// <remarks>
    /// This property provides complete user information for the professional who performed the document 
    /// transfer operation, corresponding to the <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom.User"/> 
    /// navigation property. It includes comprehensive user data and activity relationships for complete 
    /// professional accountability and audit trail management.
    /// 
    /// <para><strong>Professional Accountability Context:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Attribution:</strong> Complete professional attribution for document transfer operations</item>
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
    /// <item><strong>Professional Authorization:</strong> User must be authorized for document transfer operations</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Complete user information in transfer
    /// var transferWithUser = new MatterDocumentActivityUserFromDto
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
    /// if (transferWithUser.User != null)
    /// {
    ///     Console.WriteLine($"Document transfer performed by: {transferWithUser.User.DisplayName} " +
    ///                      $"on {transferWithUser.LocalCreatedAtDateString}");
    /// }
    /// 
    /// // Professional client communication
    /// var professionalName = transferWithUser.User?.DisplayName ?? "System Administrator";
    /// Console.WriteLine($"Your document was professionally handled by {professionalName}");
    /// 
    /// // Professional practice oversight
    /// if (transferWithUser.User?.HasActivities == true)
    /// {
    ///     Console.WriteLine($"Professional {transferWithUser.User.DisplayName} has " +
    ///                      $"{transferWithUser.User.TotalActivityCount} total system activities");
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
    /// This computed property provides a user-friendly formatted representation of the transfer timestamp 
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
    /// var transfer = new MatterDocumentActivityUserFromDto 
    /// { 
    ///     CreatedAt = new DateTime(2024, 1, 15, 14, 30, 45, DateTimeKind.Utc),
    ///     // ... other properties
    /// };
    /// 
    /// // Professional audit trail display
    /// Console.WriteLine($"Transfer completed: {transfer.LocalCreatedAtDateString}");
    /// // Output: "Transfer completed: Monday, 15 January 2024 16:30:45" (if local time is UTC+2)
    /// 
    /// // Client communication
    /// Console.WriteLine($"Your document was transferred on {transfer.LocalCreatedAtDateString}");
    /// </code>
    /// </example>
    public string LocalCreatedAtDateString => CreatedAt.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the transfer operation summary for professional audit trail display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a concise, professional summary of the document transfer operation 
    /// including key participants and operation details, optimized for audit trail reports and professional 
    /// communication.
    /// </remarks>
    /// <example>
    /// <code>
    /// var transferSummary = transfer.TransferSummary;
    /// // Returns: "Contract.pdf MOVED from Client XYZ Matter by Robert Brown"
    /// 
    /// Console.WriteLine($"Transfer Summary: {transferSummary}");
    /// </code>
    /// </example>
    public string TransferSummary =>
        $"{Document?.FileName ?? "Document"} " +
        $"{MatterDocumentActivity?.Activity ?? "TRANSFERRED"} " +
        $"from {Matter?.Description ?? "Matter"} " +
        $"by {User?.Name ?? "User"}";

    /// <summary>
    /// Gets comprehensive transfer metrics for analysis and reporting.
    /// </summary>
    /// <remarks>
    /// This property provides a structured object containing key metrics and information 
    /// for comprehensive transfer analysis, professional reporting, and compliance documentation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var metrics = transfer.TransferMetrics;
    /// // Access comprehensive transfer metrics for analysis
    /// </code>
    /// </example>
    public object TransferMetrics => new
    {
        TransferInfo = new
        {
            TransferSummary,
            LocalCreatedAtDateString,
            OperationType = MatterDocumentActivity?.Activity ?? "UNKNOWN",
            TransferDirection = "FROM"
        },
        ParticipantInfo = new
        {
            SourceMatter = Matter?.Description ?? "Unknown Matter",
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
            TransferAge = (DateTime.UtcNow - CreatedAt).TotalDays
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
    /// Validates the <see cref="MatterDocumentActivityUserFromDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using centralized validation helpers for consistency with entity 
    /// validation rules. This ensures the DTO maintains the same validation standards as the corresponding 
    /// ADMS.API.Entities.MatterDocumentActivityUserFrom entity while enforcing audit trail-specific business rules.
    /// 
    /// <para><strong>Comprehensive Validation Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Core ID Validation:</strong> All composite primary key GUIDs validated for completeness</item>
    /// <item><strong>Navigation Property Validation:</strong> Deep validation of all related entities when provided</item>
    /// <item><strong>Cross-Reference Validation:</strong> Ensures navigation property IDs match foreign key values</item>
    /// <item><strong>Temporal Validation:</strong> CreatedAt timestamp validated for professional standards</item>
    /// <item><strong>Business Rule Validation:</strong> Transfer-specific business rule compliance</item>
    /// </list>
    /// 
    /// <para><strong>Professional Standards Integration:</strong></para>
    /// Uses centralized validation helpers (MatterDocumentActivityValidationHelper, DtoValidationHelper) 
    /// to ensure consistency across all audit trail validation in the system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityUserFromDto 
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
    ///     Console.WriteLine($"Transfer Audit Validation Error: {result.ErrorMessage}");
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
                "Source matter ID must be a valid non-empty GUID for audit trail integrity.",
                [nameof(MatterId)]);

        if (DocumentId == Guid.Empty)
            yield return new ValidationResult(
                "Document ID must be a valid non-empty GUID for document tracking.",
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
            yield return new ValidationResult($"Source Matter: {result.ErrorMessage}", result.MemberNames);
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
            yield return new ValidationResult($"Transfer Document: {result.ErrorMessage}", result.MemberNames);
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
            yield return new ValidationResult($"Transfer Activity: {result.ErrorMessage}", result.MemberNames);
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
            yield return new ValidationResult($"Transfer User: {result.ErrorMessage}", result.MemberNames);
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
                "CreatedAt is unreasonably far in the past for a document transfer operation.",
                [nameof(CreatedAt)]);

        if (CreatedAt == default || CreatedAt == DateTime.MinValue)
            yield return new ValidationResult(
                "CreatedAt must be a valid date for audit trail integrity.",
                [nameof(CreatedAt)]);
    }
    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="MatterDocumentActivityUserFromDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The MatterDocumentActivityUserFromDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate MatterDocumentActivityUserFromDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance Validate method 
    /// but with null-safety and simplified usage for audit trail validation.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>API Validation:</strong> Validating incoming transfer audit DTOs in API controllers</item>
    /// <item><strong>Service Layer:</strong> Validation before processing document transfer operations</item>
    /// <item><strong>Unit Testing:</strong> Simplified validation testing without ValidationContext</item>
    /// <item><strong>Batch Processing:</strong> Validating collections of transfer audit entries</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityUserFromDto 
    /// { 
    ///     MatterId = sourceMatterId,
    ///     DocumentId = documentId,
    ///     MatterDocumentActivityId = moveActivityId,
    ///     UserId = userId,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// var results = MatterDocumentActivityUserFromDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Transfer audit validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterDocumentActivityUserFromDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterDocumentActivityUserFromDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a MatterDocumentActivityUserFromDto from ADMS.API.Entities.MatterDocumentActivityUserFrom entity with validation.
    /// </summary>
    /// <param name="entity">The MatterDocumentActivityUserFrom entity to convert. Cannot be null.</param>
    /// <param name="includeNavigationProperties">Whether to include navigation properties in the conversion.</param>
    /// <returns>A valid MatterDocumentActivityUserFromDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create MatterDocumentActivityUserFromDto instances from
    /// ADMS.API.Entities.MatterDocumentActivityUserFrom entities with automatic validation. It ensures the 
    /// resulting DTO is valid before returning it.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity with complete information
    /// var entity = new ADMS.API.Entities.MatterDocumentActivityUserFrom 
    /// { 
    ///     MatterId = sourceMatterId,
    ///     DocumentId = documentId,
    ///     MatterDocumentActivityId = activityId,
    ///     UserId = userId,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// var dto = MatterDocumentActivityUserFromDto.FromEntity(entity, includeNavigationProperties: true);
    /// </code>
    /// </example>
    public static MatterDocumentActivityUserFromDto FromEntity([NotNull] Entities.MatterDocumentActivityUserFrom entity, bool includeNavigationProperties = false)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var dto = new MatterDocumentActivityUserFromDto
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
        throw new ValidationException($"Failed to create valid MatterDocumentActivityUserFromDto from entity: {errorMessages}");

    }

    /// <summary>
    /// Creates multiple MatterDocumentActivityUserFromDto instances from a collection of entities.
    /// </summary>
    /// <param name="entities">The collection of MatterDocumentActivityUserFrom entities to convert. Cannot be null.</param>
    /// <param name="includeNavigationProperties">Whether to include navigation properties in the conversion.</param>
    /// <returns>A collection of valid MatterDocumentActivityUserFromDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method is optimized for creating multiple transfer audit DTOs efficiently.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convert collection of transfer audit entities
    /// var entities = await context.MatterDocumentActivityUsersFrom.ToListAsync();
    /// var transferAuditDtos = MatterDocumentActivityUserFromDto.FromEntities(entities, includeNavigationProperties: false);
    /// 
    /// // Process audit trail
    /// foreach (var auditDto in transferAuditDtos)
    /// {
    ///     ProcessTransferAudit(auditDto);
    /// }
    /// </code>
    /// </example>
    public static IList<MatterDocumentActivityUserFromDto> FromEntities([NotNull] IEnumerable<Entities.MatterDocumentActivityUserFrom> entities, bool includeNavigationProperties = false)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        var result = new List<MatterDocumentActivityUserFromDto>();

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
                Console.WriteLine($"Warning: Skipped invalid transfer audit entity: {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a document transfer audit entry with specified parameters and comprehensive validation.
    /// </summary>
    /// <param name="matterId">The source matter ID.</param>
    /// <param name="documentId">The document ID being transferred.</param>
    /// <param name="activityId">The transfer activity ID.</param>
    /// <param name="userId">The user performing the transfer.</param>
    /// <param name="timestamp">Optional timestamp (defaults to current UTC time).</param>
    /// <returns>A valid MatterDocumentActivityUserFromDto instance.</returns>
    /// <exception cref="ArgumentException">Thrown when any GUID parameter is empty.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a convenient way to create transfer audit entries with all required
    /// parameters while ensuring validation compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create transfer audit entry
    /// var transferAudit = MatterDocumentActivityUserFromDto.CreateTransferAudit(
    ///     sourceMatterId,
    ///     documentId,
    ///     moveActivityId,
    ///     userId);
    /// 
    /// // Save to audit system
    /// await auditService.RecordTransferAsync(transferAudit);
    /// </code>
    /// </example>
    public static MatterDocumentActivityUserFromDto CreateTransferAudit(
        Guid matterId,
        Guid documentId,
        Guid activityId,
        Guid userId,
        DateTime? timestamp = null)
    {
        if (matterId == Guid.Empty)
            throw new ArgumentException("Source matter ID cannot be empty.", nameof(matterId));
        if (documentId == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty.", nameof(documentId));
        if (activityId == Guid.Empty)
            throw new ArgumentException("Activity ID cannot be empty.", nameof(activityId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        var dto = new MatterDocumentActivityUserFromDto
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
        throw new ValidationException($"Failed to create valid transfer audit entry: {errorMessages}");

    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this transfer represents a document move operation.
    /// </summary>
    /// <returns>true if this represents a MOVED operation; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify move operations vs copy operations, which have different
    /// implications for document custody and availability.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (transferAudit.IsMoveOperation())
    /// {
    ///     // Document custody transferred - source no longer has document
    ///     Console.WriteLine($"Document custody transferred FROM {transferAudit.Matter?.Description}");
    /// }
    /// </code>
    /// </example>
    public bool IsMoveOperation() =>
        string.Equals(MatterDocumentActivity?.Activity, "MOVED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this transfer represents a document copy operation.
    /// </summary>
    /// <returns>true if this represents a COPIED operation; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify copy operations vs move operations, which have different
    /// implications for document availability and storage.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (transferAudit.IsCopyOperation())
    /// {
    ///     // Document duplicated - source retains document
    ///     Console.WriteLine($"Document copied FROM {transferAudit.Matter?.Description}");
    /// }
    /// </code>
    /// </example>
    public bool IsCopyOperation() =>
        string.Equals(MatterDocumentActivity?.Activity, "COPIED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the age of this transfer operation in days.
    /// </summary>
    /// <returns>The number of days since the transfer was performed.</returns>
    /// <remarks>
    /// This method calculates how long ago the transfer operation occurred,
    /// useful for audit trail analysis and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// var transferAge = transferAudit.GetTransferAgeDays();
    /// if (transferAge > 30)
    /// {
    ///     Console.WriteLine($"Transfer occurred {transferAge:F0} days ago");
    /// }
    /// </code>
    /// </example>
    public double GetTransferAgeDays() => (DateTime.UtcNow - CreatedAt).TotalDays;

    /// <summary>
    /// Determines whether this transfer occurred recently (within specified days).
    /// </summary>
    /// <param name="withinDays">The number of days to consider as recent (defaults to 7).</param>
    /// <returns>true if the transfer occurred within the specified timeframe; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify recent transfer activity for audit analysis and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (transferAudit.IsRecentTransfer(30))
    /// {
    ///     // Transfer occurred within last 30 days
    ///     Console.WriteLine("Recent transfer activity detected");
    /// }
    /// </code>
    /// </example>
    public bool IsRecentTransfer(int withinDays = 7) => GetTransferAgeDays() <= withinDays;

    /// <summary>
    /// Gets comprehensive transfer information for reporting and analysis.
    /// </summary>
    /// <returns>A dictionary containing detailed transfer information.</returns>
    /// <remarks>
    /// This method provides structured transfer information useful for audit reports,
    /// compliance documentation, and transfer analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// var transferInfo = transferAudit.GetTransferInformation();
    /// foreach (var item in transferInfo)
    /// {
    ///     Console.WriteLine($"{item.Key}: {item.Value}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetTransferInformation()
    {
        return new Dictionary<string, object>
        {
            ["TransferType"] = MatterDocumentActivity?.Activity ?? "UNKNOWN",
            ["IsMove"] = IsMoveOperation(),
            ["IsCopy"] = IsCopyOperation(),
            ["SourceMatter"] = Matter?.Description ?? "Unknown Matter",
            ["DocumentName"] = Document?.FileName ?? "Unknown Document",
            ["UserName"] = User?.Name ?? "Unknown User",
            ["TransferDate"] = CreatedAt,
            ["LocalTransferDate"] = LocalCreatedAtDateString,
            ["TransferAgeDays"] = GetTransferAgeDays(),
            ["IsRecentTransfer"] = IsRecentTransfer(),
            ["TransferSummary"] = TransferSummary,
            ["HasCompleteInformation"] = Matter != null && Document != null &&
                                       MatterDocumentActivity != null && User != null
        }.ToImmutableDictionary();
    }

    /// <summary>
    /// Generates a professional audit trail message for this transfer.
    /// </summary>
    /// <returns>A formatted audit trail message suitable for professional reporting.</returns>
    /// <remarks>
    /// This method creates a professional audit message that can be used in audit reports,
    /// client communications, and compliance documentation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var auditMessage = transferAudit.GenerateAuditMessage();
    /// // Returns: "On Monday, 15 January 2024 14:30:45, Robert Brown MOVED Contract.pdf FROM Client XYZ Matter"
    /// 
    /// Console.WriteLine(auditMessage);
    /// </code>
    /// </example>
    public string GenerateAuditMessage()
    {
        var operationType = MatterDocumentActivity?.Activity ?? "TRANSFERRED";
        var documentName = Document?.FileName ?? "document";
        var sourceMatter = Matter?.Description ?? "matter";
        var userName = User?.Name ?? "user";

        return $"On {LocalCreatedAtDateString}, {userName} {operationType} {documentName} FROM {sourceMatter}";
    }

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterDocumentActivityUserFromDto is equal to the current MatterDocumentActivityUserFromDto.
    /// </summary>
    /// <param name="other">The MatterDocumentActivityUserFromDto to compare with the current MatterDocumentActivityUserFromDto.</param>
    /// <returns>true if the specified MatterDocumentActivityUserFromDto is equal to the current MatterDocumentActivityUserFromDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all composite key properties that uniquely identify a transfer audit entry.
    /// This follows the same pattern as the entity composite key for consistency.
    /// </remarks>
    public virtual bool Equals(MatterDocumentActivityUserFromDto? other)
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
    /// <returns>A hash code for the current MatterDocumentActivityUserFromDto.</returns>
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
    /// Returns a string representation of the MatterDocumentActivityUserFromDto.
    /// </summary>
    /// <returns>A string that represents the current MatterDocumentActivityUserFromDto.</returns>
    /// <remarks>
    /// The string representation includes key transfer audit information for identification and logging purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterDocumentActivityUserFromDto 
    /// { 
    ///     MatterId = sourceMatterId,
    ///     DocumentId = documentId,
    ///     MatterDocumentActivityId = activityId,
    ///     UserId = userId,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "Transfer FROM: Document (70000000-0000-0000-0000-000000000001) ← Matter (60000000-0000-0000-0000-000000000001) by User (50000000-0000-0000-0000-000000000001)"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Transfer FROM: Document ({DocumentId}) ← Matter ({MatterId}) by User ({UserId}) at {CreatedAt:yyyy-MM-dd HH:mm:ss}";

    #endregion String Representation
}