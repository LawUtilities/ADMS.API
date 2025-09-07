using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Entities;

/// <summary>
/// Represents a matter for digital document collection and management in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// The Matter entity serves as the primary organizational container for digital documents within the ADMS legal
/// document management system. It functions as a digital filing cabinet that can represent either client-based
/// or matter-specific document collections, making it particularly well-suited for small law firms and legal
/// practitioners who need flexible document organization strategies.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Collection:</strong> Central container for related digital documents</item>
/// <item><strong>Client or Matter Based:</strong> Flexible organization supporting both client-specific and matter-specific groupings</item>
/// <item><strong>Comprehensive Audit Trail:</strong> Full tracking of all matter-related activities</item>
/// <item><strong>Document Transfer Support:</strong> Enables moving and copying documents between matters</item>
/// <item><strong>Lifecycle Management:</strong> Supports archival, deletion, and restoration workflows</item>
/// <item><strong>Legal Compliance:</strong> Maintains audit trails required for legal practice</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Client-Based Collections:</strong> All documents for a specific client (e.g., "Smith Family Trust")</item>
/// <item><strong>Matter-Specific Collections:</strong> Documents for a specific legal matter (e.g., "Smith v. Jones Litigation")</item>
/// <item><strong>Project-Based Collections:</strong> Documents for legal projects (e.g., "Corporate Merger - ABC Corp")</item>
/// <item><strong>Subject-Based Collections:</strong> Documents organized by legal subject matter</item>
/// </list>
/// 
/// <para><strong>Database Configuration:</strong></para>
/// <list type="bullet">
/// <item>Primary key: GUID with identity generation</item>
/// <item>Description constraint: StringLength(128) with required validation and uniqueness</item>
/// <item>Status flags: IsArchived and IsDeleted for lifecycle management</item>
/// <item>Temporal tracking: CreationDate in UTC for audit and ordering</item>
/// <item>Seeded data: Multiple test matters with various states for development and testing</item>
/// </list>
/// 
/// <para><strong>Document Management Features:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Association:</strong> One-to-many relationship with Document entities</item>
/// <item><strong>Document Transfers:</strong> Support for moving/copying documents between matters</item>
/// <item><strong>Document Organization:</strong> Logical grouping of related documents</item>
/// <item><strong>Access Control:</strong> Foundation for document-level security and permissions</item>
/// </list>
/// 
/// <para><strong>Activity Tracking:</strong></para>
/// The Matter entity maintains comprehensive audit trails through multiple relationship types:
/// <list type="bullet">
/// <item><strong>MatterActivityUsers:</strong> General matter lifecycle activities (CREATED, ARCHIVED, DELETED, etc.)</item>
/// <item><strong>MatterDocumentActivityUsersFrom:</strong> Source-side document transfer tracking</item>
/// <item><strong>MatterDocumentActivityUsersTo:</strong> Destination-side document transfer tracking</item>
/// </list>
/// 
/// <para><strong>Legal Compliance Support:</strong></para>
/// <list type="bullet">
/// <item>Complete audit trails for all matter-related operations</item>
/// <item>Document custody tracking for legal discovery requirements</item>
/// <item>User attribution for all activities within the matter</item>
/// <item>Temporal consistency for legal timeline reconstruction</item>
/// <item>Soft deletion with preservation of historical data</item>
/// </list>
/// 
/// <para><strong>Business Rules:</strong></para>
/// <list type="bullet">
/// <item>Matter descriptions must be unique across the system</item>
/// <item>Creation date is automatically set and cannot be modified</item>
/// <item>Matters can be archived and unarchived as needed</item>
/// <item>Deleted matters are soft-deleted to preserve audit trails</item>
/// <item>All matter operations must be attributed to users for accountability</item>
/// </list>
/// 
/// <para><strong>Entity Framework Configuration:</strong></para>
/// The entity is configured in AdmsContext with:
/// <list type="bullet">
/// <item>Required relationships to all related activity tracking entities</item>
/// <item>Cascade delete restrictions to preserve audit trail integrity</item>
/// <item>Performance optimization for document queries</item>
/// <item>Seeded test data for development and testing purposes</item>
/// </list>
/// 
/// <para><strong>Small Law Firm Benefits:</strong></para>
/// <list type="bullet">
/// <item>Flexible organization supporting various practice areas</item>
/// <item>Simple client-based organization for client-centric practices</item>
/// <item>Matter-specific organization for complex litigation</item>
/// <item>Comprehensive audit trails for professional compliance</item>
/// <item>Document transfer capabilities for case management</item>
/// </list>
/// </remarks>
public class Matter : IEquatable<Matter>, IComparable<Matter>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the matter.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and is automatically generated when the matter is created.
    /// The unique identifier enables reliable referencing across all system components and maintains
    /// referential integrity in the audit trail system.
    /// 
    /// <para><strong>Database Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Primary key with identity generation</item>
    /// <item>Non-nullable and required for all operations</item>
    /// <item>Used as foreign key in Document and all activity tracking entities</item>
    /// <item>Remains constant throughout the matter's lifecycle</item>
    /// </list>
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// The ID remains constant throughout the matter's lifecycle and is used for all
    /// document associations, activity tracking, and audit trail operations. It serves
    /// as the primary reference point for all matter-related operations in the system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var matter = new Matter 
    /// { 
    ///     Description = "Client ABC - Corporate Formation",
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// // ID will be automatically generated when saved to database
    /// 
    /// // Accessing matter by ID
    /// var matterId = Guid.Parse("60000000-0000-0000-0000-000000000001");
    /// </code>
    /// </example>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the description of the matter.
    /// </summary>
    /// <remarks>
    /// The matter description serves as the primary human-readable identifier and must be unique
    /// across the entire system. This field supports both client-based and matter-specific
    /// naming conventions to accommodate various legal practice organization strategies.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Required field - cannot be null or empty</item>
    /// <item>Maximum length: 128 characters (database constraint)</item>
    /// <item>Must be unique across all matters in the system</item>
    /// <item>Should be descriptive enough to identify the matter's purpose</item>
    /// <item>Supports professional naming conventions</item>
    /// </list>
    /// 
    /// <para><strong>Naming Convention Examples:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Client-Based:</strong> "Smith Family Trust", "ABC Corporation", "Johnson Estate"</item>
    /// <item><strong>Matter-Specific:</strong> "Smith v. Jones Litigation", "ABC Corp Merger", "Johnson Will Contest"</item>
    /// <item><strong>Project-Based:</strong> "Downtown Development Project", "Patent Application - XYZ Technology"</item>
    /// </list>
    /// 
    /// <para><strong>Business Context:</strong></para>
    /// The description is used throughout the system for:
    /// <list type="bullet">
    /// <item>User interface display and matter selection</item>
    /// <item>Document organization and filing</item>
    /// <item>Audit trail reporting and analysis</item>
    /// <item>Legal compliance documentation</item>
    /// <item>Client communication and billing</item>
    /// </list>
    /// 
    /// <para><strong>Professional Usage:</strong></para>
    /// For small law firms, the description should clearly identify either the client
    /// or the specific legal matter to support effective practice management and
    /// professional organization standards.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Client-based matter descriptions
    /// var clientMatter = new Matter { Description = "Smith Family Trust" };
    /// var corporateMatter = new Matter { Description = "ABC Corporation" };
    /// 
    /// // Matter-specific descriptions
    /// var litigationMatter = new Matter { Description = "Smith v. Jones Contract Dispute" };
    /// var realEstateMatter = new Matter { Description = "123 Main St Property Purchase" };
    /// 
    /// // Validation example
    /// bool isValidLength = clientMatter.Description.Length <= 128;
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter description is required and cannot be empty.")]
    [StringLength(128, ErrorMessage = "Matter description cannot exceed 128 characters.")]
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the matter is archived.
    /// </summary>
    /// <remarks>
    /// The archived status indicates that the matter has been moved to an inactive state,
    /// typically when the legal work is complete but the matter and its documents must
    /// be retained for legal, professional, or client requirements.
    /// 
    /// <para><strong>Archival Purposes:</strong></para>
    /// <list type="bullet">
    /// <item>Completed legal matters that require document retention</item>
    /// <item>Inactive client relationships with historical document value</item>
    /// <item>Matters awaiting final disposition or client instruction</item>
    /// <item>Long-term storage for compliance and audit purposes</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Archived matters remain accessible but are typically hidden from active views</item>
    /// <item>Documents within archived matters are preserved and accessible</item>
    /// <item>Archived matters can be unarchived if work resumes</item>
    /// <item>Archival activities are tracked in the audit trail</item>
    /// </list>
    /// 
    /// <para><strong>Workflow Integration:</strong></para>
    /// <list type="bullet">
    /// <item>Supports practice management workflows for matter lifecycle</item>
    /// <item>Enables reporting and analytics on matter completion</item>
    /// <item>Facilitates compliance with document retention policies</item>
    /// <item>Supports client communication about matter status</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Benefits:</strong></para>
    /// Archival status helps legal professionals maintain organized practices by
    /// distinguishing between active and completed work while preserving access
    /// to historical documents and maintaining professional compliance standards.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Archive a completed matter
    /// var completedMatter = new Matter 
    /// { 
    ///     Description = "Johnson Estate - Probate Complete",
    ///     IsArchived = true,
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// 
    /// // Check archival status
    /// if (matter.IsArchived)
    /// {
    ///     // Handle archived matter display logic
    ///     Console.WriteLine($"Matter '{matter.Description}' is archived");
    /// }
    /// </code>
    /// </example>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the matter has been deleted.
    /// </summary>
    /// <remarks>
    /// The deletion status indicates that the matter has been marked for removal while
    /// preserving the data for audit trail integrity and potential restoration. This
    /// implements soft deletion to maintain legal compliance and professional standards.
    /// 
    /// <para><strong>Soft Deletion Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Preserves audit trails for legal compliance and professional responsibility</item>
    /// <item>Maintains referential integrity for associated documents and activities</item>
    /// <item>Enables matter restoration if deletion was in error</item>
    /// <item>Supports legal discovery and historical analysis requirements</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Deleted matters are hidden from normal user interfaces</item>
    /// <item>Associated documents must be handled according to deletion policies</item>
    /// <item>Deletion activities are tracked in the audit trail</item>
    /// <item>Deleted matters can be restored through administrative functions</item>
    /// </list>
    /// 
    /// <para><strong>Professional Compliance:</strong></para>
    /// <list type="bullet">
    /// <item>Supports professional responsibility for document retention</item>
    /// <item>Maintains historical records for potential legal review</item>
    /// <item>Preserves client confidentiality while managing data lifecycle</item>
    /// <item>Enables compliance with legal practice standards</item>
    /// </list>
    /// 
    /// <para><strong>Data Lifecycle Management:</strong></para>
    /// The soft deletion approach ensures that legal professionals can manage their
    /// practice data lifecycle while maintaining the integrity and availability of
    /// historical information required for professional and legal compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check deletion status
    /// if (matter.IsDeleted)
    /// {
    ///     // Handle deleted matter - typically hidden from normal views
    ///     logger.LogInformation($"Matter '{matter.Description}' is marked as deleted");
    /// }
    /// 
    /// // Soft delete a matter
    /// matter.IsDeleted = true;
    /// // Note: Actual deletion would be handled through the repository layer
    /// 
    /// // Query for active (non-deleted) matters
    /// var activeMatters = matters.Where(m => !m.IsDeleted);
    /// </code>
    /// </example>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the matter (in UTC).
    /// </summary>
    /// <remarks>
    /// The creation date establishes the temporal foundation for the matter and all associated
    /// activities. It is automatically set when the matter is created and should never be
    /// modified to maintain audit trail integrity and professional compliance standards.
    /// 
    /// <para><strong>Temporal Standards:</strong></para>
    /// <list type="bullet">
    /// <item>Stored in UTC format for global consistency and timezone independence</item>
    /// <item>Automatically set during matter creation</item>
    /// <item>Immutable after creation to maintain historical accuracy</item>
    /// <item>Used for chronological ordering and audit trail sequencing</item>
    /// </list>
    /// 
    /// <para><strong>Professional Usage:</strong></para>
    /// <list type="bullet">
    /// <item>Establishes matter timeline for billing and case management</item>
    /// <item>Supports professional reporting and practice analytics</item>
    /// <item>Provides foundation for document retention and compliance policies</item>
    /// <item>Enables historical analysis and matter lifecycle tracking</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance:</strong></para>
    /// <list type="bullet">
    /// <item>Supports legal discovery timeline requirements</item>
    /// <item>Enables professional responsibility documentation</item>
    /// <item>Facilitates client communication about matter duration</item>
    /// <item>Provides foundation for professional practice standards</item>
    /// </list>
    /// 
    /// <para><strong>System Integration:</strong></para>
    /// The creation date integrates with various system components including audit
    /// trails, reporting systems, and document management workflows to provide
    /// comprehensive temporal tracking and professional practice support.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Matter creation with automatic timestamp
    /// var newMatter = new Matter
    /// {
    ///     Description = "New Client Intake - XYZ Corp",
    ///     CreationDate = DateTime.UtcNow,  // Always use UTC
    ///     IsArchived = false,
    ///     IsDeleted = false
    /// };
    /// 
    /// // Calculate matter age
    /// var matterAge = DateTime.UtcNow - matter.CreationDate;
    /// Console.WriteLine($"Matter is {matterAge.Days} days old");
    /// 
    /// // Query matters by creation date range
    /// var recentMatters = matters.Where(m => 
    ///     m.CreationDate >= DateTime.UtcNow.AddMonths(-1) && !m.IsDeleted);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Creation date is required.")]
    public DateTime CreationDate { get; set; }

    #endregion Core Properties

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the digital documents associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection represents the core document management functionality of the Matter entity,
    /// containing all digital documents that belong to this matter. The relationship supports
    /// comprehensive document organization and management for legal practice.
    /// 
    /// <para><strong>Document Collection Features:</strong></para>
    /// <list type="bullet">
    /// <item>One-to-many relationship supporting unlimited document storage</item>
    /// <item>Automatic association of documents with the matter</item>
    /// <item>Support for various document types and formats</item>
    /// <item>Integration with document versioning and revision control</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as one-to-many relationship in AdmsContext</item>
    /// <item>Foreign key relationship through Document.MatterId</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>Cascade behavior preserves referential integrity</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Usage:</strong></para>
    /// <list type="bullet">
    /// <item>Organizes client documents by matter or client</item>
    /// <item>Supports case file management and organization</item>
    /// <item>Enables comprehensive document search and retrieval</item>
    /// <item>Facilitates professional compliance and documentation</item>
    /// </list>
    /// 
    /// <para><strong>Document Management Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Centralized document storage and organization</item>
    /// <item>Automatic document association and categorization</item>
    /// <item>Support for document lifecycle management</item>
    /// <item>Integration with audit trails and activity tracking</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// The virtual modifier enables lazy loading, but consider explicit loading or
    /// projections when working with matters that have large document collections
    /// to avoid N+1 query issues and optimize performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing documents in a matter
    /// foreach (var document in matter.Documents)
    /// {
    ///     Console.WriteLine($"Document: {document.FileName} - Size: {document.FileSize} bytes");
    /// }
    /// 
    /// // Counting documents
    /// var documentCount = matter.Documents.Count;
    /// var activeDocuments = matter.Documents.Where(d => !d.IsDeleted).Count();
    /// 
    /// // Using explicit loading to avoid N+1 queries
    /// await context.Entry(matter)
    ///     .Collection(m => m.Documents)
    ///     .LoadAsync();
    /// </code>
    /// </example>
    public virtual ICollection<Document> Documents { get; set; } = new HashSet<Document>();

    /// <summary>
    /// Gets or sets the matter activity users associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection maintains the comprehensive audit trail for all matter-related activities,
    /// tracking who performed what actions on the matter and when. This supports legal compliance,
    /// professional accountability, and practice management requirements.
    /// 
    /// <para><strong>Activity Tracking Features:</strong></para>
    /// <list type="bullet">
    /// <item>Complete audit trail for all matter lifecycle activities</item>
    /// <item>User attribution for professional accountability</item>
    /// <item>Temporal tracking for compliance and reporting</item>
    /// <item>Support for various matter activities (CREATED, ARCHIVED, DELETED, etc.)</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance Support:</strong></para>
    /// <list type="bullet">
    /// <item>Maintains professional responsibility documentation</item>
    /// <item>Supports legal discovery and audit requirements</item>
    /// <item>Enables client communication about matter activities</item>
    /// <item>Facilitates practice management and oversight</item>
    /// </list>
    /// 
    /// <para><strong>Professional Practice Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Comprehensive activity logging for professional standards</item>
    /// <item>User accountability for all matter operations</item>
    /// <item>Historical tracking for practice improvement</item>
    /// <item>Integration with billing and time tracking systems</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>One-to-many relationship from Matter to MatterActivityUser</item>
    /// <item>Composite primary key in MatterActivityUser includes MatterId</item>
    /// <item>Required relationship preserves audit trail integrity</item>
    /// <item>Supports efficient querying and reporting</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing matter activity history
    /// foreach (var activity in matter.MatterActivityUsers.OrderBy(a => a.CreatedAt))
    /// {
    ///     Console.WriteLine($"{activity.User?.Name} performed {activity.MatterActivity?.Activity} at {activity.CreatedAt}");
    /// }
    /// 
    /// // Finding specific activities
    /// var creationActivity = matter.MatterActivityUsers
    ///     .FirstOrDefault(a => a.MatterActivity?.Activity == "CREATED");
    /// 
    /// var recentActivities = matter.MatterActivityUsers
    ///     .Where(a => a.CreatedAt >= DateTime.UtcNow.AddDays(-30))
    ///     .OrderByDescending(a => a.CreatedAt);
    /// </code>
    /// </example>
    public virtual ICollection<MatterActivityUser> MatterActivityUsers { get; set; } = new HashSet<MatterActivityUser>();

    /// <summary>
    /// Gets or sets the "from" matter document activity users associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection tracks all document transfer activities where documents were moved or copied
    /// FROM this matter to other matters. This provides the source-side audit trail for document
    /// transfer operations, essential for legal compliance and document custody tracking.
    /// 
    /// <para><strong>Document Transfer Tracking:</strong></para>
    /// <list type="bullet">
    /// <item>Source-side tracking for document moves and copies</item>
    /// <item>Complete audit trail for document custody changes</item>
    /// <item>User attribution for document transfer initiation</item>
    /// <item>Temporal tracking for legal timeline reconstruction</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Document custody chain maintenance for legal discovery</item>
    /// <item>Professional responsibility tracking for document handling</item>
    /// <item>Client confidentiality and document security monitoring</item>
    /// <item>Audit trail preservation for compliance requirements</item>
    /// </list>
    /// 
    /// <para><strong>Professional Practice Support:</strong></para>
    /// <list type="bullet">
    /// <item>Document organization and matter management</item>
    /// <item>Client communication about document location changes</item>
    /// <item>Practice workflow optimization and analysis</item>
    /// <item>Professional oversight and quality assurance</item>
    /// </list>
    /// 
    /// <para><strong>Integration with Document Management:</strong></para>
    /// This collection works in conjunction with MatterDocumentActivityUsersTo to provide
    /// complete bidirectional audit trails for document transfers, ensuring comprehensive
    /// tracking of document movement between matters in the legal practice.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing documents transferred FROM this matter
    /// foreach (var transferFrom in matter.MatterDocumentActivityUsersFrom)
    /// {
    ///     Console.WriteLine($"Document {transferFrom.Document?.FileName} " +
    ///                      $"{transferFrom.MatterDocumentActivity?.Activity} from this matter " +
    ///                      $"by {transferFrom.User?.Name} at {transferFrom.CreatedAt}");
    /// }
    /// 
    /// // Finding recent outbound transfers
    /// var recentTransfersOut = matter.MatterDocumentActivityUsersFrom
    ///     .Where(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-30))
    ///     .OrderByDescending(t => t.CreatedAt);
    /// 
    /// // Counting transfer activities by type
    /// var moveCount = matter.MatterDocumentActivityUsersFrom
    ///     .Count(t => t.MatterDocumentActivity?.Activity == "MOVED");
    /// var copyCount = matter.MatterDocumentActivityUsersFrom
    ///     .Count(t => t.MatterDocumentActivity?.Activity == "COPIED");
    /// </code>
    /// </example>
    public virtual ICollection<MatterDocumentActivityUserFrom> MatterDocumentActivityUsersFrom { get; set; } = new HashSet<MatterDocumentActivityUserFrom>();

    /// <summary>
    /// Gets or sets the "to" matter document activity users associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection tracks all document transfer activities where documents were moved or copied
    /// TO this matter from other matters. This provides the destination-side audit trail for document
    /// transfer operations, completing the bidirectional tracking system for legal compliance.
    /// 
    /// <para><strong>Document Reception Tracking:</strong></para>
    /// <list type="bullet">
    /// <item>Destination-side tracking for document moves and copies</item>
    /// <item>Complete audit trail for documents received into this matter</item>
    /// <item>User attribution for document transfer receipt</item>
    /// <item>Temporal tracking for document arrival and integration</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Document provenance tracking for legal discovery</item>
    /// <item>Client notification about document additions</item>
    /// <item>Matter completeness validation and quality assurance</item>
    /// <item>Professional responsibility for document custody</item>
    /// </list>
    /// 
    /// <para><strong>Practice Management Support:</strong></para>
    /// <list type="bullet">
    /// <item>Matter organization and document consolidation</item>
    /// <item>Client service delivery and communication</item>
    /// <item>Professional workflow optimization</item>
    /// <item>Quality control and oversight capabilities</item>
    /// </list>
    /// 
    /// <para><strong>Bidirectional Audit System:</strong></para>
    /// This collection complements MatterDocumentActivityUsersFrom to provide complete
    /// bidirectional tracking of document transfers, ensuring that every document movement
    /// between matters is fully documented and traceable for legal and professional compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing documents transferred TO this matter
    /// foreach (var transferTo in matter.MatterDocumentActivityUsersTo)
    /// {
    ///     Console.WriteLine($"Document {transferTo.Document?.FileName} " +
    ///                      $"{transferTo.MatterDocumentActivity?.Activity} to this matter " +
    ///                      $"by {transferTo.User?.Name} at {transferTo.CreatedAt}");
    /// }
    /// 
    /// // Finding recent inbound transfers
    /// var recentTransfersIn = matter.MatterDocumentActivityUsersTo
    ///     .Where(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-7))
    ///     .OrderByDescending(t => t.CreatedAt);
    /// 
    /// // Analyzing transfer patterns
    /// var transferSummary = matter.MatterDocumentActivityUsersTo
    ///     .GroupBy(t => t.MatterDocumentActivity?.Activity)
    ///     .Select(g => new { Activity = g.Key, Count = g.Count() });
    /// </code>
    /// </example>
    public virtual ICollection<MatterDocumentActivityUserTo> MatterDocumentActivityUsersTo { get; set; } = new HashSet<MatterDocumentActivityUserTo>();

    #endregion Navigation Properties

    #region Computed Properties

    /// <summary>
    /// Gets a value indicating whether this matter contains any documents.
    /// </summary>
    /// <remarks>
    /// This computed property provides a quick way to determine if the matter has any associated
    /// documents, useful for validation, user interface logic, and business rule enforcement.
    /// 
    /// <para><strong>Performance Note:</strong></para>
    /// This property triggers database queries if the Documents collection is not loaded.
    /// Consider using explicit loading or projections when working with multiple matters.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.HasDocuments)
    /// {
    ///     Console.WriteLine($"Matter '{matter.Description}' contains {matter.Documents.Count} documents");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Matter '{matter.Description}' is empty");
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasDocuments => Documents.Count > 0;

    /// <summary>
    /// Gets the total number of documents in this matter.
    /// </summary>
    /// <remarks>
    /// This computed property provides the total count of documents associated with the matter,
    /// useful for reporting, user interface display, and practice management analytics.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Matter contains {matter.DocumentCount} total documents");
    /// 
    /// // Finding matters with many documents
    /// var largeMatters = matters.Where(m => m.DocumentCount > 100);
    /// </code>
    /// </example>
    [NotMapped]
    public int DocumentCount => Documents.Count;

    /// <summary>
    /// Gets the number of active (non-deleted) documents in this matter.
    /// </summary>
    /// <remarks>
    /// This computed property provides the count of active documents, excluding those that
    /// have been soft-deleted, useful for active document management and user interface display.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Matter has {matter.ActiveDocumentCount} active documents");
    /// </code>
    /// </example>
    [NotMapped]
    public int ActiveDocumentCount => Documents.Count(d => !d.IsDeleted);

    /// <summary>
    /// Gets the age of this matter in days.
    /// </summary>
    /// <remarks>
    /// This computed property calculates the number of days since the matter was created,
    /// useful for practice management, billing analysis, and matter lifecycle tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Matter is {matter.AgeDays} days old");
    /// 
    /// // Finding old matters
    /// var oldMatters = matters.Where(m => m.AgeDays > 365);
    /// </code>
    /// </example>
    [NotMapped]
    public double AgeDays => (DateTime.UtcNow - CreationDate).TotalDays;

    /// <summary>
    /// Gets the current status of the matter as a descriptive string.
    /// </summary>
    /// <remarks>
    /// This computed property provides a human-readable status description based on the
    /// matter's current state flags, useful for user interface display and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Matter status: {matter.Status}");
    /// // Possible outputs: "Active", "Archived", "Deleted", "Archived and Deleted"
    /// </code>
    /// </example>
    [NotMapped]
    public string Status
    {
        get
        {
            if (IsDeleted && IsArchived) return "Archived and Deleted";
            if (IsDeleted) return "Deleted";
            if (IsArchived) return "Archived";
            return "Active";
        }
    }

    /// <summary>
    /// Gets a value indicating whether this matter has any activity history.
    /// </summary>
    /// <remarks>
    /// This computed property determines if the matter has any recorded activities,
    /// useful for audit trail validation and user interface logic.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.HasActivityHistory)
    /// {
    ///     // Display activity history in user interface
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasActivityHistory => MatterActivityUsers.Count > 0;

    /// <summary>
    /// Gets the total count of all activities (matter + document transfer) for this matter.
    /// </summary>
    /// <remarks>
    /// This computed property provides a comprehensive count of all activities associated
    /// with the matter, including general matter activities and document transfer activities.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Total activities: {matter.TotalActivityCount}");
    /// </code>
    /// </example>
    [NotMapped]
    public int TotalActivityCount =>
        MatterActivityUsers.Count +
        MatterDocumentActivityUsersFrom.Count +
        MatterDocumentActivityUsersTo.Count;

    /// <summary>
    /// Gets a value indicating whether the matter can be safely deleted.
    /// </summary>
    /// <remarks>
    /// This computed property checks business rules to determine if the matter can be deleted,
    /// considering factors like document status and checkout state.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.CanBeDeleted)
    /// {
    ///     // Allow deletion operation
    /// }
    /// else
    /// {
    ///     // Show user why deletion is not allowed
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool CanBeDeleted => !IsDeleted && Documents.All(d => d.IsDeleted && !d.IsCheckedOut);

    #endregion Computed Properties

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified Matter is equal to the current Matter.
    /// </summary>
    /// <param name="other">The Matter to compare with the current Matter.</param>
    /// <returns>true if the specified Matter is equal to the current Matter; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each matter has a unique identifier.
    /// This follows Entity Framework best practices for entity equality comparison.
    /// </remarks>
    public bool Equals(Matter? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current Matter.
    /// </summary>
    /// <param name="obj">The object to compare with the current Matter.</param>
    /// <returns>true if the specified object is equal to the current Matter; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as Matter);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current Matter.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Compares the current Matter with another Matter for ordering purposes.
    /// </summary>
    /// <param name="other">The Matter to compare with the current Matter.</param>
    /// <returns>
    /// A value that indicates the relative order of the matters being compared.
    /// Less than zero: This matter precedes the other matter.
    /// Zero: This matter occurs in the same position as the other matter.
    /// Greater than zero: This matter follows the other matter.
    /// </returns>
    /// <remarks>
    /// Comparison is performed based on matter description for alphabetical ordering,
    /// which is most useful for display and user interface purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Sort matters alphabetically
    /// var sortedMatters = matters.OrderBy(m => m).ToList();
    /// 
    /// // Compare specific matters
    /// if (matter1.CompareTo(matter2) < 0)
    /// {
    ///     Console.WriteLine($"Matter '{matter1.Description}' comes before '{matter2.Description}'");
    /// }
    /// </code>
    /// </example>
    public int CompareTo(Matter? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by description for alphabetical ordering
        var descriptionComparison = string.Compare(Description, other.Description, StringComparison.OrdinalIgnoreCase);
        if (descriptionComparison != 0) return descriptionComparison;

        // Secondary sort by creation date for consistency
        var dateComparison = CreationDate.CompareTo(other.CreationDate);
        if (dateComparison != 0) return dateComparison;

        // Final sort by ID for absolute consistency
        return Id.CompareTo(other.Id);
    }

    /// <summary>
    /// Determines whether two Matter instances are equal.
    /// </summary>
    /// <param name="left">The first Matter to compare.</param>
    /// <param name="right">The second Matter to compare.</param>
    /// <returns>true if the Matters are equal; otherwise, false.</returns>
    public static bool operator ==(Matter? left, Matter? right) =>
        EqualityComparer<Matter>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two Matter instances are not equal.
    /// </summary>
    /// <param name="left">The first Matter to compare.</param>
    /// <param name="right">The second Matter to compare.</param>
    /// <returns>true if the Matters are not equal; otherwise, false.</returns>
    public static bool operator !=(Matter? left, Matter? right) => !(left == right);

    /// <summary>
    /// Determines whether one Matter precedes another in the ordering.
    /// </summary>
    /// <param name="left">The first Matter to compare.</param>
    /// <param name="right">The second Matter to compare.</param>
    /// <returns>true if the left Matter precedes the right Matter; otherwise, false.</returns>
    public static bool operator <(Matter? left, Matter? right) =>
        left is not null && (right is null || left.CompareTo(right) < 0);

    /// <summary>
    /// Determines whether one Matter precedes or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first Matter to compare.</param>
    /// <param name="right">The second Matter to compare.</param>
    /// <returns>true if the left Matter precedes or equals the right Matter; otherwise, false.</returns>
    public static bool operator <=(Matter? left, Matter? right) =>
        left is null || (right is not null && left.CompareTo(right) <= 0);

    /// <summary>
    /// Determines whether one Matter follows another in the ordering.
    /// </summary>
    /// <param name="left">The first Matter to compare.</param>
    /// <param name="right">The second Matter to compare.</param>
    /// <returns>true if the left Matter follows the right Matter; otherwise, false.</returns>
    public static bool operator >(Matter? left, Matter? right) =>
        left is not null && (right is null || left.CompareTo(right) > 0);

    /// <summary>
    /// Determines whether one Matter follows or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first Matter to compare.</param>
    /// <param name="right">The second Matter to compare.</param>
    /// <returns>true if the left Matter follows or equals the right Matter; otherwise, false.</returns>
    public static bool operator >=(Matter? left, Matter? right) =>
        left is null || (right is not null && left.CompareTo(right) >= 0);

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the Matter.
    /// </summary>
    /// <returns>A string that represents the current Matter.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the matter,
    /// which is useful for debugging, logging, and display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var matter = new Matter 
    /// { 
    ///     Id = Guid.Parse("60000000-0000-0000-0000-000000000001"), 
    ///     Description = "Smith Family Trust",
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// 
    /// Console.WriteLine(matter);
    /// // Output: "Matter: Smith Family Trust (60000000-0000-0000-0000-000000000001) - Active"
    /// </code>
    /// </example>
    public override string ToString() => $"Matter: {Description} ({Id}) - {Status}";

    #endregion String Representation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this matter is currently active (not archived and not deleted).
    /// </summary>
    /// <returns>true if the matter is active; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify active matters for
    /// user interface display and business logic enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.IsActive())
    /// {
    ///     // Show matter in active matters list
    ///     Console.WriteLine($"Active matter: {matter.Description}");
    /// }
    /// </code>
    /// </example>
    public bool IsActive() => !IsArchived && !IsDeleted;

    /// <summary>
    /// Determines whether this matter can be archived based on business rules.
    /// </summary>
    /// <returns>true if the matter can be archived; otherwise, false.</returns>
    /// <remarks>
    /// This method checks business rules to determine if archival is allowed,
    /// considering the current state and any associated constraints.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.CanBeArchived())
    /// {
    ///     matter.IsArchived = true;
    ///     // Log archival activity...
    /// }
    /// </code>
    /// </example>
    public bool CanBeArchived() => !IsArchived && !IsDeleted;

    /// <summary>
    /// Determines whether this matter can be restored from deleted state.
    /// </summary>
    /// <returns>true if the matter can be restored; otherwise, false.</returns>
    /// <remarks>
    /// This method determines if a deleted matter can be restored based on
    /// business rules and system constraints.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (matter.CanBeRestored())
    /// {
    ///     matter.IsDeleted = false;
    ///     // Log restoration activity...
    /// }
    /// </code>
    /// </example>
    public bool CanBeRestored() => IsDeleted;

    /// <summary>
    /// Gets summary statistics about the matter's documents and activities.
    /// </summary>
    /// <returns>A dictionary containing matter statistics.</returns>
    /// <remarks>
    /// This method provides comprehensive statistics for reporting and analysis purposes,
    /// useful for practice management and client communication.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = matter.GetMatterStatistics();
    /// Console.WriteLine($"Total documents: {stats["TotalDocuments"]}");
    /// Console.WriteLine($"Active documents: {stats["ActiveDocuments"]}");
    /// Console.WriteLine($"Total activities: {stats["TotalActivities"]}");
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetMatterStatistics()
    {
        return new Dictionary<string, object>
        {
            ["MatterId"] = Id,
            ["Description"] = Description,
            ["Status"] = Status,
            ["IsActive"] = IsActive(),
            ["AgeDays"] = AgeDays,
            ["CreationDate"] = CreationDate,
            ["TotalDocuments"] = DocumentCount,
            ["ActiveDocuments"] = ActiveDocumentCount,
            ["TotalActivities"] = TotalActivityCount,
            ["HasActivityHistory"] = HasActivityHistory,
            ["CanBeDeleted"] = CanBeDeleted,
            ["CanBeArchived"] = CanBeArchived(),
            ["CanBeRestored"] = CanBeRestored()
        };
    }

    /// <summary>
    /// Gets the most recent activity performed on this matter.
    /// </summary>
    /// <returns>The most recent MatterActivityUser, or null if no activities exist.</returns>
    /// <remarks>
    /// This method provides access to the most recent activity for user interface
    /// display and business logic purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var lastActivity = matter.GetMostRecentActivity();
    /// if (lastActivity != null)
    /// {
    ///     Console.WriteLine($"Last activity: {lastActivity.MatterActivity?.Activity} " +
    ///                      $"by {lastActivity.User?.Name} at {lastActivity.CreatedAt}");
    /// }
    /// </code>
    /// </example>
    public MatterActivityUser? GetMostRecentActivity()
    {
        return MatterActivityUsers
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets activities of a specific type performed on this matter.
    /// </summary>
    /// <param name="activityType">The activity type to filter by.</param>
    /// <returns>A collection of matching activities.</returns>
    /// <remarks>
    /// This method enables filtering activities by type for specific analysis
    /// and reporting purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var creationActivities = matter.GetActivitiesByType("CREATED");
    /// var archivalActivities = matter.GetActivitiesByType("ARCHIVED");
    /// </code>
    /// </example>
    public IEnumerable<MatterActivityUser> GetActivitiesByType(string activityType)
    {
        if (string.IsNullOrWhiteSpace(activityType))
            return Enumerable.Empty<MatterActivityUser>();

        return MatterActivityUsers
            .Where(a => string.Equals(a.MatterActivity?.Activity, activityType, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(a => a.CreatedAt);
    }

    #endregion Business Logic Methods

    #region Validation Methods

    /// <summary>
    /// Validates the current Matter instance against business rules.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive validation including business rule compliance,
    /// data integrity checks, and professional practice standards.
    /// </remarks>
    /// <example>
    /// <code>
    /// var validationResults = matter.Validate();
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

        // Validate required fields
        if (Id == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Matter ID cannot be empty.",
                [nameof(Id)]));
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            results.Add(new ValidationResult(
                "Matter description is required and cannot be empty.",
                [nameof(Description)]));
        }
        else if (Description.Length > 128)
        {
            results.Add(new ValidationResult(
                "Matter description cannot exceed 128 characters.",
                [nameof(Description)]));
        }

        // Validate creation date
        if (CreationDate == default)
        {
            results.Add(new ValidationResult(
                "Creation date is required.",
                [nameof(CreationDate)]));
        }
        else if (!Common.RevisionValidationHelper.IsValidDate(CreationDate))
        {
            results.Add(new ValidationResult(
                "Creation date is not within valid date range.",
                [nameof(CreationDate)]));
        }

        // Business rule validations
        if (IsDeleted && IsArchived)
        {
            // This combination is allowed but log for monitoring
        }

        return results;
    }

    #endregion Validation Methods
}