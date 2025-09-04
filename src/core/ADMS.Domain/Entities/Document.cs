using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Domain.Entities;

/// <summary>
/// Represents a digital document stored in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// The Document entity serves as the core component of the legal document management system,
/// representing digital files and their associated metadata within the context of legal matters.
/// Each document belongs to exactly one matter and maintains comprehensive metadata for legal
/// compliance, audit trails, and file integrity verification.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Digital File Storage:</strong> Represents files stored on disk with metadata in database</item>
/// <item><strong>Legal Matter Association:</strong> Each document belongs to exactly one legal matter</item>
/// <item><strong>File Integrity:</strong> Maintains checksums for content verification and security</item>
/// <item><strong>Version Control:</strong> Supports document revisions with check-in/check-out functionality</item>
/// <item><strong>Comprehensive Audit Trail:</strong> Tracks all document operations and user activities</item>
/// <item><strong>Soft Deletion:</strong> Preserves deleted documents for audit and recovery purposes</item>
/// </list>
/// 
/// <para><strong>File Management Features:</strong></para>
/// <list type="bullet">
/// <item><strong>File Metadata:</strong> Stores filename, extension, size, and MIME type information</item>
/// <item><strong>Content Integrity:</strong> SHA256 checksums for file verification and security</item>
/// <item><strong>Type Safety:</strong> Validates file types and extensions for security compliance</item>
/// <item><strong>Size Monitoring:</strong> Tracks file sizes for storage management and limits</item>
/// <item><strong>MIME Type Detection:</strong> Automatic content type detection and validation</item>
/// </list>
/// 
/// <para><strong>Document Lifecycle Management:</strong></para>
/// <list type="bullet">
/// <item><strong>Creation:</strong> Upload and metadata capture with virus scanning</item>
/// <item><strong>Modification:</strong> Version control through check-in/check-out workflow</item>
/// <item><strong>Transfer:</strong> Move and copy operations between matters with audit trails</item>
/// <item><strong>Deletion:</strong> Soft deletion preserving audit history</item>
/// <item><strong>Restoration:</strong> Recovery of deleted documents when needed</item>
/// </list>
/// 
/// <para><strong>Database Configuration:</strong></para>
/// <list type="bullet">
/// <item>Primary key: GUID with identity generation</item>
/// <item>Required matter association through foreign key</item>
/// <item>File metadata constraints: FileName(128), Extension(5), MimeType, Checksum</item>
/// <item>Multiple navigation properties for comprehensive audit trail relationships</item>
/// <item>No seeded data - documents are created through user operations</item>
/// </list>
/// 
/// <para><strong>Legal Compliance Support:</strong></para>
/// <list type="bullet">
/// <item>Complete audit trails for all document operations</item>
/// <item>File integrity verification through cryptographic checksums</item>
/// <item>User attribution for all document activities</item>
/// <item>Document custody tracking through check-in/check-out operations</item>
/// <item>Transfer tracking between matters for legal discovery</item>
/// <item>Soft deletion preserving evidence and audit requirements</item>
/// </list>
/// 
/// <para><strong>Security Features:</strong></para>
/// <list type="bullet">
/// <item>Virus scanning integration during upload</item>
/// <item>File type validation and restriction enforcement</item>
/// <item>Size limits to prevent abuse and storage issues</item>
/// <item>Checksum verification for content integrity</item>
/// <item>Extension validation to prevent malicious uploads</item>
/// </list>
/// 
/// <para><strong>Entity Framework Integration:</strong></para>
/// The entity is configured in AdmsContext with:
/// <list type="bullet">
/// <item>Required relationship to Matter entity</item>
/// <item>One-to-many relationships with Revision and activity tracking entities</item>
/// <item>Appropriate cascade behaviors for referential integrity</item>
/// <item>Performance optimization for document queries</item>
/// <item>Support for complex document transfer audit trails</item>
/// </list>
/// 
/// <para><strong>Professional Usage:</strong></para>
/// The Document entity supports professional legal practice requirements including proper
/// document organization, version control, audit trails, and compliance with legal practice
/// standards for maintaining accurate records of document management operations.
/// </remarks>
public class Document : IEquatable<Document>, IComparable<Document>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the document.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and is automatically generated when the document
    /// is created. The unique identifier enables reliable referencing across all system
    /// components and maintains referential integrity in the audit trail system.
    /// 
    /// <para><strong>Database Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Primary key with identity generation</item>
    /// <item>Non-nullable and required for all operations</item>
    /// <item>Used as foreign key in Revision and all activity tracking entities</item>
    /// <item>Remains constant throughout the document's lifecycle</item>
    /// </list>
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// The ID remains constant throughout the document's lifecycle and is used for all
    /// revision associations, activity tracking, file storage paths, and audit trail
    /// operations. It serves as the primary reference point for all document-related
    /// operations in the system.
    /// 
    /// <para><strong>File Storage Integration:</strong></para>
    /// The document ID is typically used as part of the file storage path to ensure
    /// unique file names and enable efficient file retrieval operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var document = new Document 
    /// { 
    ///     FileName = "contract.pdf",
    ///     Extension = "pdf",
    ///     MatterId = matterGuid
    /// };
    /// // ID will be automatically generated when saved to database
    /// 
    /// // Using document ID for file storage path
    /// var filePath = $"matters/{document.MatterId}/documents/{document.Id}.{document.Extension}";
    /// </code>
    /// </example>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the file name of the document.
    /// </summary>
    /// <remarks>
    /// The file name represents the original or desired name of the document file, including
    /// any descriptive text but excluding the file extension. This field is used for display
    /// purposes and document identification within the legal matter context.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Required field - cannot be null or empty</item>
    /// <item>Maximum length: 128 characters (database constraint)</item>
    /// <item>Should not include file extension (stored separately)</item>
    /// <item>Must be suitable for file system storage</item>
    /// <item>Should be descriptive for legal document identification</item>
    /// </list>
    /// 
    /// <para><strong>Business Context:</strong></para>
    /// <list type="bullet">
    /// <item>Used for document identification in user interfaces</item>
    /// <item>Important for legal document organization and search</item>
    /// <item>Displayed in matter document lists and reports</item>
    /// <item>Used in audit trail descriptions and logging</item>
    /// <item>Critical for legal discovery and document production</item>
    /// </list>
    /// 
    /// <para><strong>Professional Usage:</strong></para>
    /// File names should be descriptive and professional, helping legal practitioners
    /// quickly identify document content and purpose within the context of the matter.
    /// Clear naming conventions improve document management efficiency and legal compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional document naming examples
    /// var contract = new Document { FileName = "Service Agreement - ABC Corp" };
    /// var pleading = new Document { FileName = "Motion for Summary Judgment" };
    /// var evidence = new Document { FileName = "Email Correspondence - Smith to Jones" };
    /// 
    /// // Full file name with extension for display
    /// var displayName = $"{document.FileName}.{document.Extension}";
    /// </code>
    /// </example>
    [Required(ErrorMessage = "File name is required and cannot be empty.")]
    [MaxLength(128, ErrorMessage = "File name cannot exceed 128 characters.")]
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets the file extension.
    /// </summary>
    /// <remarks>
    /// The file extension identifies the document format and type, stored without the leading dot.
    /// This field is critical for file type validation, MIME type determination, and security
    /// enforcement within the legal document management system.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Required field - cannot be null or empty</item>
    /// <item>Maximum length: 5 characters (database constraint)</item>
    /// <item>Stored without leading dot (e.g., "pdf" not ".pdf")</item>
    /// <item>Must be from allowed file types list</item>
    /// <item>Case-insensitive but stored in lowercase</item>
    /// </list>
    /// 
    /// <para><strong>Supported File Types:</strong></para>
    /// The system supports various document types common in legal practice:
    /// <list type="bullet">
    /// <item><strong>Documents:</strong> pdf, doc, docx, rtf, txt</item>
    /// <item><strong>Spreadsheets:</strong> xls, xlsx, csv</item>
    /// <item><strong>Presentations:</strong> ppt, pptx</item>
    /// <item><strong>Images:</strong> jpg, jpeg, png, gif, tiff</item>
    /// <item><strong>Archives:</strong> zip, rar (with restrictions)</item>
    /// </list>
    /// 
    /// <para><strong>Security Considerations:</strong></para>
    /// <list type="bullet">
    /// <item>Extensions are validated against allowed types for security</item>
    /// <item>Content type detection prevents extension spoofing</item>
    /// <item>Executable file types are prohibited</item>
    /// <item>Validation occurs during upload and update operations</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Setting extensions properly (without leading dot)
    /// var pdfDocument = new Document { Extension = "pdf" };
    /// var wordDocument = new Document { Extension = "docx" };
    /// var imageDocument = new Document { Extension = "jpg" };
    /// 
    /// // Generating full filename
    /// var fullFileName = $"{document.FileName}.{document.Extension}";
    /// </code>
    /// </example>
    [Required(ErrorMessage = "File extension is required and cannot be empty.")]
    [MaxLength(5, ErrorMessage = "File extension cannot exceed 5 characters.")]
    public required string Extension { get; set; }

    /// <summary>
    /// Gets or sets the size of the file in bytes.
    /// </summary>
    /// <remarks>
    /// The file size represents the exact size of the document content in bytes, used for
    /// storage management, upload validation, and system monitoring. This field is critical
    /// for enforcing upload limits and managing storage capacity.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Required field - must be specified</item>
    /// <item>Must be greater than 0 for valid documents</item>
    /// <item>Subject to maximum upload size limits (typically 50MB)</item>
    /// <item>Used for storage quota management</item>
    /// <item>Automatically calculated during file upload</item>
    /// </list>
    /// 
    /// <para><strong>Business Usage:</strong></para>
    /// <list type="bullet">
    /// <item>Storage management and capacity planning</item>
    /// <item>Upload validation and size limit enforcement</item>
    /// <item>User interface display of file information</item>
    /// <item>System monitoring and reporting</item>
    /// <item>Network transfer optimization</item>
    /// </list>
    /// 
    /// <para><strong>System Limits:</strong></para>
    /// <list type="bullet">
    /// <item>Maximum upload size: 50MB (configurable)</item>
    /// <item>Minimum size: 1 byte (empty files not allowed)</item>
    /// <item>Size limits enforced at upload and update</item>
    /// <item>Large file handling with streaming support</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // File size examples
    /// var smallDocument = new Document { FileSize = 1024 }; // 1KB
    /// var largeDocument = new Document { FileSize = 5 * 1024 * 1024 }; // 5MB
    /// 
    /// // Display file size in human-readable format
    /// string GetFormattedSize(long bytes)
    /// {
    ///     if (bytes < 1024) return $"{bytes} bytes";
    ///     if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
    ///     return $"{bytes / (1024.0 * 1024.0):F1} MB";
    /// }
    /// 
    /// var displaySize = GetFormattedSize(document.FileSize);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "File size is required and must be greater than zero.")]
    [Range(1, long.MaxValue, ErrorMessage = "File size must be greater than zero.")]
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the file (e.g., "application/pdf").
    /// </summary>
    /// <remarks>
    /// The MIME type identifies the content format of the document according to internet
    /// standards. This field is automatically determined during upload based on file
    /// content analysis and is used for proper content handling and security validation.
    /// 
    /// <para><strong>Content Type Detection:</strong></para>
    /// <list type="bullet">
    /// <item>Automatically detected from file content (not just extension)</item>
    /// <item>Validated against known safe MIME types</item>
    /// <item>Used for proper HTTP content-type headers</item>
    /// <item>Critical for browser handling and security</item>
    /// <item>Prevents MIME type spoofing attacks</item>
    /// </list>
    /// 
    /// <para><strong>Common Legal Document MIME Types:</strong></para>
    /// <list type="bullet">
    /// <item><strong>PDF:</strong> application/pdf</item>
    /// <item><strong>Word Documents:</strong> application/vnd.openxmlformats-officedocument.wordprocessingml.document</item>
    /// <item><strong>Plain Text:</strong> text/plain</item>
    /// <item><strong>Images:</strong> image/jpeg, image/png, image/tiff</item>
    /// <item><strong>Excel:</strong> application/vnd.openxmlformats-officedocument.spreadsheetml.sheet</item>
    /// </list>
    /// 
    /// <para><strong>Security Considerations:</strong></para>
    /// <list type="bullet">
    /// <item>MIME type validation prevents malicious file uploads</item>
    /// <item>Content-based detection prevents extension spoofing</item>
    /// <item>Only approved MIME types are allowed</item>
    /// <item>Validation occurs during upload and file operations</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // MIME type examples for legal documents
    /// var pdfDoc = new Document { MimeType = "application/pdf" };
    /// var wordDoc = new Document { MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
    /// var imageDoc = new Document { MimeType = "image/jpeg" };
    /// 
    /// // MIME type validation example
    /// var allowedTypes = new[] { "application/pdf", "text/plain", "image/jpeg" };
    /// bool isAllowed = allowedTypes.Contains(document.MimeType);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "MIME type is required and cannot be empty.")]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the checksum (e.g., SHA256 hash) of the file for integrity verification.
    /// </summary>
    /// <remarks>
    /// The checksum provides cryptographic verification of file integrity, ensuring that
    /// document content has not been corrupted or tampered with. This is critical for
    /// legal document management where content integrity must be guaranteed.
    /// 
    /// <para><strong>Integrity Verification:</strong></para>
    /// <list type="bullet">
    /// <item>SHA256 hash automatically calculated during upload</item>
    /// <item>Stored as 64-character lowercase hexadecimal string</item>
    /// <item>Used to detect file corruption or tampering</item>
    /// <item>Enables duplicate detection across the system</item>
    /// <item>Critical for legal document authenticity</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Proves document content integrity for legal proceedings</item>
    /// <item>Detects unauthorized modifications to legal documents</item>
    /// <item>Supports chain of custody requirements</item>
    /// <item>Enables forensic analysis if needed</item>
    /// <item>Complies with legal practice standards</item>
    /// </list>
    /// 
    /// <para><strong>Technical Implementation:</strong></para>
    /// <list type="bullet">
    /// <item>Calculated using System.Security.Cryptography.SHA256</item>
    /// <item>Computed during file upload and stored in database</item>
    /// <item>Can be recalculated for verification purposes</item>
    /// <item>Used for duplicate detection and deduplication</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example SHA256 checksum
    /// var document = new Document 
    /// { 
    ///     Checksum = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3"
    /// };
    /// 
    /// // Verifying file integrity
    /// public static bool VerifyIntegrity(byte[] fileContent, string expectedChecksum)
    /// {
    ///     var hash = System.Security.Cryptography.SHA256.HashData(fileContent);
    ///     var calculatedChecksum = Convert.ToHexString(hash).ToLowerInvariant();
    ///     return string.Equals(calculatedChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase);
    /// }
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Checksum is required for file integrity verification.")]
    [StringLength(64, MinimumLength = 64, ErrorMessage = "Checksum must be exactly 64 characters (SHA256 hash).")]
    [RegularExpression(@"^[a-f0-9]{64}$", ErrorMessage = "Checksum must be a 64-character lowercase hexadecimal string.")]
    public string Checksum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the matter linked to this document.
    /// </summary>
    /// <remarks>
    /// This property establishes the required foreign key relationship to the matter that
    /// contains this document. Every document must belong to exactly one matter, providing
    /// the organizational context for legal document management.
    /// 
    /// <para><strong>Relationship Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Required field - every document must have a matter</item>
    /// <item>Must reference a valid, existing matter</item>
    /// <item>Cannot be changed after document creation (use transfer operations)</item>
    /// <item>Forms the basis for document organization and access control</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Documents are organized within legal matters</item>
    /// <item>Matter association controls document access and permissions</item>
    /// <item>Used for document search and filtering within matter context</item>
    /// <item>Critical for legal case organization and discovery</item>
    /// </list>
    /// 
    /// <para><strong>Document Transfer:</strong></para>
    /// When documents need to be moved between matters, this is accomplished through
    /// dedicated transfer operations that maintain audit trails rather than direct
    /// property modification.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Creating document in a matter
    /// var document = new Document
    /// {
    ///     MatterId = legalMatter.Id,  // Must reference valid matter
    ///     FileName = "Client Contract",
    ///     Extension = "pdf"
    /// };
    /// 
    /// // Document organization within matter
    /// var matterDocuments = matter.Documents.Where(d => !d.IsDeleted);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter ID is required for document association.")]
    public Guid MatterId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document is checked out.
    /// </summary>
    /// <remarks>
    /// The check-out status implements optimistic locking for document editing, preventing
    /// concurrent modifications that could lead to conflicts or data loss. This is essential
    /// for collaborative legal document management.
    /// 
    /// <para><strong>Version Control Workflow:</strong></para>
    /// <list type="bullet">
    /// <item>Documents must be checked out before editing</item>
    /// <item>Only one user can check out a document at a time</item>
    /// <item>Check-out prevents other users from modifying the document</item>
    /// <item>Documents must be checked in after editing to allow other access</item>
    /// <item>Check-out status is tracked in audit trails</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Checked out documents cannot be deleted</item>
    /// <item>Check-out operations create audit trail entries</item>
    /// <item>User attribution tracks who has document checked out</item>
    /// <item>Check-out status affects document availability</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Prevents simultaneous editing conflicts</item>
    /// <item>Maintains document version integrity</item>
    /// <item>Supports collaborative legal workflows</item>
    /// <item>Provides clear document custody tracking</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check-out workflow
    /// if (!document.IsCheckedOut)
    /// {
    ///     document.IsCheckedOut = true;
    ///     // Create check-out activity record...
    /// }
    /// else
    /// {
    ///     throw new InvalidOperationException("Document is already checked out");
    /// }
    /// 
    /// // Checking document availability
    /// bool canEdit = !document.IsCheckedOut && !document.IsDeleted;
    /// </code>
    /// </example>
    public bool IsCheckedOut { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document has been deleted.
    /// </summary>
    /// <remarks>
    /// The deletion status implements soft deletion, marking documents as deleted while
    /// preserving them for audit trail purposes and potential recovery. This approach
    /// is essential for legal compliance and professional responsibility standards.
    /// 
    /// <para><strong>Soft Deletion Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Preserves audit trails for legal compliance</item>
    /// <item>Maintains referential integrity for related data</item>
    /// <item>Enables document recovery if deletion was in error</item>
    /// <item>Supports legal discovery and historical analysis</item>
    /// <item>Complies with legal practice document retention requirements</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Deleted documents are hidden from normal operations</item>
    /// <item>Physical files may be preserved on storage</item>
    /// <item>Deletion creates comprehensive audit trail entries</item>
    /// <item>Deleted documents can be restored through administrative functions</item>
    /// <item>Checked out documents cannot be deleted</item>
    /// </list>
    /// 
    /// <para><strong>Professional Compliance:</strong></para>
    /// <list type="bullet">
    /// <item>Supports professional responsibility for document preservation</item>
    /// <item>Maintains historical records for potential legal review</item>
    /// <item>Preserves client confidentiality while managing data lifecycle</item>
    /// <item>Enables compliance with legal practice standards</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Soft delete operation
    /// document.IsDeleted = true;
    /// // Physical file remains but document is marked as deleted
    /// 
    /// // Query for active documents
    /// var activeDocuments = matter.Documents.Where(d => !d.IsDeleted);
    /// 
    /// // Business rule validation
    /// if (document.IsCheckedOut && document.IsDeleted)
    /// {
    ///     throw new InvalidOperationException("Cannot delete checked out document");
    /// }
    /// </code>
    /// </example>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time of the document in UTC for temporal tracking and audit compliance.
    /// </summary>
    /// <remarks>
    /// The creation date establishes the temporal foundation for the document lifecycle and must be accurately
    /// maintained for legal audit trail compliance and professional document management standards.
    /// 
    /// <para><strong>Temporal Significance:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Audit Trail Foundation:</strong> Establishes the starting point for comprehensive document timeline tracking</item>
    /// <item><strong>Legal Compliance:</strong> Provides precise temporal data required for legal document management</item>
    /// <item><strong>Professional Standards:</strong> Supports professional practice standards for document creation tracking</item>
    /// <item><strong>Chronological Analysis:</strong> Enables document age analysis and professional document lifecycle management</item>
    /// <item><strong>UTC Standardization:</strong> Uses UTC for consistent global temporal tracking and audit compliance</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Immutable Foundation:</strong> Creation date should not change after initial document creation</item>
    /// <item><strong>Temporal Consistency:</strong> Must be consistent with revision creation dates and activity timestamps</item>
    /// <item><strong>Reasonable Bounds:</strong> Must be within reasonable temporal bounds for system validity</item>
    /// <item><strong>Audit Trail Integration:</strong> Serves as temporal reference for all subsequent document operations</item>
    /// </list>
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateRequiredDate() for comprehensive
    /// temporal validation and ValidateCrossPropertyRules() for temporal consistency with revisions.
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Document.CreationDate"/> exactly, ensuring
    /// consistent temporal tracking and reliable document lifecycle management.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Document creation with temporal tracking
    /// var document = new DocumentDto 
    /// { 
    ///     CreationDate = DateTime.UtcNow,
    ///     /* other properties */
    /// };
    /// 
    /// // Professional temporal analysis
    /// var documentAge = document.DocumentAge; // TimeSpan representing age
    /// var ageInDays = document.DocumentAge.TotalDays;
    /// 
    /// // Professional reporting
    /// Console.WriteLine($"Document created: {document.CreationDate:yyyy-MM-dd HH:mm:ss} UTC");
    /// Console.WriteLine($"Document age: {ageInDays:F1} days");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Creation date is required for temporal tracking and audit compliance.")]
    public required DateTime CreationDate { get; init; }

    #endregion Core Properties

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the matter entity linked to this document.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the matter that contains this document.
    /// The relationship is established through the MatterId foreign key and enables access
    /// to matter-level information and operations for legal document management.
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses MatterId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>Part of one-to-many relationship (Matter has many Documents)</item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Accessing matter context from document operations</item>
    /// <item>Matter-level security and access control</item>
    /// <item>Document organization and case management</item>
    /// <item>Legal compliance and audit trail operations</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// The virtual modifier enables lazy loading, but consider explicit loading or
    /// projections when working with multiple documents to avoid N+1 query issues.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing matter through document
    /// string matterDescription = document.Matter.Description;
    /// bool matterIsArchived = document.Matter.IsArchived;
    /// 
    /// // Using explicit loading to avoid N+1 queries
    /// context.Entry(document)
    ///     .Reference(d => d.Matter)
    ///     .Load();
    /// </code>
    /// </example>
    [ForeignKey(nameof(MatterId))]
    public virtual required Matter Matter { get; set; }

    /// <summary>
    /// Gets or sets the collection of revisions for this document.
    /// </summary>
    /// <remarks>
    /// This collection maintains the version history of the document through revision entities.
    /// Each revision represents a specific version of the document content, supporting
    /// comprehensive version control and document history tracking.
    /// 
    /// <para><strong>Version Control Features:</strong></para>
    /// <list type="bullet">
    /// <item>Sequential revision numbering starting from 1</item>
    /// <item>Complete version history preservation</item>
    /// <item>User attribution for each revision</item>
    /// <item>Temporal tracking of document changes</item>
    /// <item>Support for revision comparison and rollback</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>One-to-many relationship from Document to Revision</item>
    /// <item>Cascade delete behavior for data integrity</item>
    /// <item>Ordered by revision number for chronological access</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Complete audit trail of document changes</item>
    /// <item>Ability to reconstruct document history</item>
    /// <item>Evidence of document evolution for legal purposes</item>
    /// <item>Support for regulatory compliance requirements</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing document revisions
    /// var latestRevision = document.Revisions
    ///     .OrderByDescending(r => r.RevisionNumber)
    ///     .FirstOrDefault();
    /// 
    /// var revisionCount = document.Revisions.Count;
    /// var firstRevision = document.Revisions
    ///     .OrderBy(r => r.RevisionNumber)
    ///     .FirstOrDefault();
    /// 
    /// // Creating new revision
    /// var newRevision = new Revision 
    /// { 
    ///     DocumentId = document.Id,
    ///     RevisionNumber = document.Revisions.Count + 1,
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    public virtual ICollection<Revision> Revisions { get; set; } = new HashSet<Revision>();

    /// <summary>
    /// Gets or sets the collection of document activity user associations for this document.
    /// </summary>
    /// <remarks>
    /// This collection tracks all document-related activities performed by users, maintaining
    /// a comprehensive audit trail of document operations essential for legal document management.
    /// 
    /// <para><strong>Activity Types Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Document creation activity</item>
    /// <item><strong>SAVED:</strong> Document save activity</item>
    /// <item><strong>DELETED:</strong> Document deletion activity</item>
    /// <item><strong>RESTORED:</strong> Document restoration activity</item>
    /// <item><strong>CHECKED IN:</strong> Document check-in activity</item>
    /// <item><strong>CHECKED OUT:</strong> Document check-out activity</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance Support:</strong></para>
    /// <list type="bullet">
    /// <item>Complete user attribution for all document operations</item>
    /// <item>Temporal audit trails for legal discovery and compliance</item>
    /// <item>Immutable record of document lifecycle activities</item>
    /// <item>Support for regulatory reporting and audit requirements</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>One-to-many relationship from Document to DocumentActivityUser</item>
    /// <item>Composite primary key in DocumentActivityUser includes DocumentId</item>
    /// <item>Standard cascade behavior for referential integrity</item>
    /// <item>Performance optimization for activity queries</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing document activities
    /// foreach (var activity in document.DocumentActivityUsers.OrderBy(a => a.CreatedAt))
    /// {
    ///     Console.WriteLine($"{activity.User?.Name} performed {activity.DocumentActivity?.Activity} at {activity.CreatedAt}");
    /// }
    /// 
    /// // Finding specific activities
    /// var creationActivity = document.DocumentActivityUsers
    ///     .FirstOrDefault(a => a.DocumentActivity?.Activity == "CREATED");
    /// 
    /// var recentActivities = document.DocumentActivityUsers
    ///     .Where(a => a.CreatedAt >= DateTime.UtcNow.AddDays(-7))
    ///     .OrderByDescending(a => a.CreatedAt);
    /// </code>
    /// </example>
    public virtual ICollection<DocumentActivityUser> DocumentActivityUsers { get; set; } = new HashSet<DocumentActivityUser>();

    /// <summary>
    /// Gets or sets the collection of "from" matter document activity user associations for this document.
    /// </summary>
    /// <remarks>
    /// This collection tracks document transfer activities where this document was moved or copied
    /// FROM its current matter to other matters. This provides the source-side audit trail for
    /// document transfer operations, essential for legal compliance and document custody tracking.
    /// 
    /// <para><strong>Transfer Operations Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> Document moved from source matter to destination</item>
    /// <item><strong>COPIED:</strong> Document copied from source matter to destination</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Document custody chains for legal discovery</item>
    /// <item>Complete audit trail for document provenance</item>
    /// <item>User attribution for transfer initiation</item>
    /// <item>Temporal tracking for legal timeline reconstruction</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>One-to-many relationship with composite primary key</item>
    /// <item>NoAction cascade delete to preserve audit trail integrity</item>
    /// <item>Supports lazy loading for on-demand access</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing outbound transfer history
    /// foreach (var transfer in document.MatterDocumentActivityUsersFrom)
    /// {
    ///     Console.WriteLine($"Document {transfer.MatterDocumentActivity?.Activity} from matter {transfer.Matter?.Description}");
    /// }
    /// 
    /// // Recent transfers from this document
    /// var recentTransfers = document.MatterDocumentActivityUsersFrom
    ///     .Where(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-30))
    ///     .OrderByDescending(t => t.CreatedAt);
    /// </code>
    /// </example>
    public virtual ICollection<MatterDocumentActivityUserFrom> MatterDocumentActivityUsersFrom { get; set; } = new HashSet<MatterDocumentActivityUserFrom>();

    /// <summary>
    /// Gets or sets the collection of "to" matter document activity user associations for this document.
    /// </summary>
    /// <remarks>
    /// This collection tracks document transfer activities where this document was moved or copied
    /// TO its current matter from other matters. This provides the destination-side audit trail
    /// for document transfer operations, completing the bidirectional tracking system.
    /// 
    /// <para><strong>Transfer Operations Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> Document moved from source matter to current matter</item>
    /// <item><strong>COPIED:</strong> Document copied from source matter to current matter</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Document provenance tracking for legal compliance</item>
    /// <item>Matter consolidation and organization audit trails</item>
    /// <item>User accountability for document receipt</item>
    /// <item>Complete bidirectional transfer documentation</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>One-to-many relationship with composite primary key</item>
    /// <item>NoAction cascade delete to preserve audit trail integrity</item>
    /// <item>Supports lazy loading for on-demand access</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing inbound transfer history
    /// foreach (var transfer in document.MatterDocumentActivityUsersTo)
    /// {
    ///     Console.WriteLine($"Document {transfer.MatterDocumentActivity?.Activity} to matter {transfer.Matter?.Description}");
    /// }
    /// 
    /// // Finding source of document
    /// var originalSource = document.MatterDocumentActivityUsersTo
    ///     .OrderBy(t => t.CreatedAt)
    ///     .FirstOrDefault();
    /// </code>
    /// </example>
    public virtual ICollection<MatterDocumentActivityUserTo> MatterDocumentActivityUsersTo { get; set; } = new HashSet<MatterDocumentActivityUserTo>();

    #endregion Navigation Properties

    #region Computed Properties

    /// <summary>
    /// Gets the full file name including extension.
    /// </summary>
    /// <remarks>
    /// This computed property combines the file name and extension to provide the complete
    /// file name as it would appear in a file system or user interface.
    /// </remarks>
    /// <example>
    /// <code>
    /// var document = new Document { FileName = "Contract", Extension = "pdf" };
    /// Console.WriteLine(document.FullFileName); // Output: "Contract.pdf"
    /// </code>
    /// </example>
    [NotMapped]
    public string FullFileName => $"{FileName}.{Extension}";

    /// <summary>
    /// Gets the formatted file size in human-readable format.
    /// </summary>
    /// <remarks>
    /// This computed property formats the file size in bytes into a more readable
    /// format (bytes, KB, MB, GB) for user interface display.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine(document.FormattedFileSize);
    /// // Examples: "1.5 KB", "2.3 MB", "1.2 GB"
    /// </code>
    /// </example>
    [NotMapped]
    public string FormattedFileSize
    {
        get
        {
            const long kb = 1024;
            const long mb = kb * 1024;
            const long gb = mb * 1024;

            return FileSize switch
            {
                < kb => $"{FileSize} bytes",
                < mb => $"{FileSize / (double)kb:F1} KB",
                < gb => $"{FileSize / (double)mb:F1} MB",
                _ => $"{FileSize / (double)gb:F1} GB"
            };
        }
    }

    /// <summary>
    /// Gets a value indicating whether this document has any revisions.
    /// </summary>
    /// <remarks>
    /// This computed property provides a quick way to determine if the document has
    /// version history, useful for user interface logic and business operations.
    /// 
    /// <para><strong>Performance Note:</strong></para>
    /// This property may trigger database queries if the Revisions collection is not loaded.
    /// Consider using explicit loading when working with multiple documents.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.HasRevisions)
    /// {
    ///     Console.WriteLine($"Document has {document.Revisions.Count} revisions");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Document has no revision history");
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasRevisions => Revisions.Count > 0;

    /// <summary>
    /// Gets the total number of revisions for this document.
    /// </summary>
    /// <remarks>
    /// This computed property provides the count of document revisions, useful for
    /// version control operations and user interface display.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Document has {document.RevisionCount} revisions");
    /// </code>
    /// </example>
    [NotMapped]
    public int RevisionCount => Revisions.Count;

    /// <summary>
    /// Gets the latest revision number for this document.
    /// </summary>
    /// <remarks>
    /// This computed property returns the highest revision number, useful for
    /// creating new revisions and displaying current version information.
    /// Returns 0 if no revisions exist.
    /// </remarks>
    /// <example>
    /// <code>
    /// var nextRevisionNumber = document.LatestRevisionNumber + 1;
    /// Console.WriteLine($"Latest revision: {document.LatestRevisionNumber}");
    /// </code>
    /// </example>
    [NotMapped]
    public int LatestRevisionNumber => Revisions.Any() ? Revisions.Max(r => r.RevisionNumber) : 0;

    /// <summary>
    /// Gets a value indicating whether this document has any activity history.
    /// </summary>
    /// <remarks>
    /// This computed property determines if the document has any recorded activities,
    /// useful for audit trail validation and user interface logic.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.HasActivityHistory)
    /// {
    ///     // Show activity history in user interface
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasActivityHistory => DocumentActivityUsers.Count > 0;

    /// <summary>
    /// Gets the total count of all activities (document + transfer) for this document.
    /// </summary>
    /// <remarks>
    /// This computed property provides a comprehensive count of all activities associated
    /// with the document, including regular document activities and transfer activities.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Total activities: {document.TotalActivityCount}");
    /// </code>
    /// </example>
    [NotMapped]
    public int TotalActivityCount =>
        DocumentActivityUsers.Count +
        MatterDocumentActivityUsersFrom.Count +
        MatterDocumentActivityUsersTo.Count;

    /// <summary>
    /// Gets a value indicating whether the document can be safely deleted.
    /// </summary>
    /// <remarks>
    /// This computed property checks business rules to determine if the document can be deleted,
    /// considering factors like checkout status and current deletion state.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.CanBeDeleted)
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
    public bool CanBeDeleted => !IsDeleted && !IsCheckedOut;

    /// <summary>
    /// Gets a value indicating whether the document can be checked out.
    /// </summary>
    /// <remarks>
    /// This computed property determines if the document is available for checkout
    /// based on current status flags and business rules.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.CanBeCheckedOut)
    /// {
    ///     // Allow checkout operation
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool CanBeCheckedOut => !IsCheckedOut && !IsDeleted;

    /// <summary>
    /// Gets a value indicating whether the document can be checked in.
    /// </summary>
    /// <remarks>
    /// This computed property determines if the document can be checked in
    /// based on current checkout status.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.CanBeCheckedIn)
    /// {
    ///     // Allow checkin operation
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool CanBeCheckedIn => IsCheckedOut && !IsDeleted;

    /// <summary>
    /// Gets a value indicating whether this document is a PDF.
    /// </summary>
    /// <remarks>
    /// This computed property provides a convenient way to identify PDF documents,
    /// which are commonly used in legal practice.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.IsPdf)
    /// {
    ///     // Handle PDF-specific operations
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool IsPdf => string.Equals(Extension, "pdf", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this document is an image.
    /// </summary>
    /// <remarks>
    /// This computed property identifies common image formats used in legal practice.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.IsImage)
    /// {
    ///     // Handle image-specific operations
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool IsImage
    {
        get
        {
            var imageExtensions = new[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            return imageExtensions.Contains(Extension.ToLowerInvariant());
        }
    }

    /// <summary>
    /// Gets a value indicating whether this document is a Microsoft Office document.
    /// </summary>
    /// <remarks>
    /// This computed property identifies Microsoft Office document formats commonly
    /// used in legal practice.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.IsOfficeDocument)
    /// {
    ///     // Handle Office document operations
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool IsOfficeDocument
    {
        get
        {
            var officeExtensions = new[] { "doc", "docx", "xls", "xlsx", "ppt", "pptx" };
            return officeExtensions.Contains(Extension.ToLowerInvariant());
        }
    }

    #endregion Computed Properties

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified Document is equal to the current Document.
    /// </summary>
    /// <param name="other">The Document to compare with the current Document.</param>
    /// <returns>true if the specified Document is equal to the current Document; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each document has a unique identifier.
    /// This follows Entity Framework best practices for entity equality comparison.
    /// </remarks>
    public bool Equals(Document? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current Document.
    /// </summary>
    /// <param name="obj">The object to compare with the current Document.</param>
    /// <returns>true if the specified object is equal to the current Document; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as Document);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current Document.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Compares the current Document with another Document for ordering purposes.
    /// </summary>
    /// <param name="other">The Document to compare with the current Document.</param>
    /// <returns>
    /// A value that indicates the relative order of the documents being compared.
    /// Less than zero: This document precedes the other document.
    /// Zero: This document occurs in the same position as the other document.
    /// Greater than zero: This document follows the other document.
    /// </returns>
    /// <remarks>
    /// Comparison is performed based on file name for alphabetical ordering,
    /// which is most useful for display and user interface purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Sort documents alphabetically
    /// var sortedDocuments = documents.OrderBy(d => d).ToList();
    /// 
    /// // Compare specific documents
    /// if (doc1.CompareTo(doc2) < 0)
    /// {
    ///     Console.WriteLine($"Document '{doc1.FullFileName}' comes before '{doc2.FullFileName}'");
    /// }
    /// </code>
    /// </example>
    public int CompareTo(Document? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by file name for alphabetical ordering
        var nameComparison = string.Compare(FileName, other.FileName, StringComparison.OrdinalIgnoreCase);
        if (nameComparison != 0) return nameComparison;

        // Secondary sort by extension
        var extensionComparison = string.Compare(Extension, other.Extension, StringComparison.OrdinalIgnoreCase);
        return extensionComparison != 0 ? extensionComparison :
            // Final sort by ID for consistency
            Id.CompareTo(other.Id);
    }

    /// <summary>
    /// Determines whether two Document instances are equal.
    /// </summary>
    /// <param name="left">The first Document to compare.</param>
    /// <param name="right">The second Document to compare.</param>
    /// <returns>true if the Documents are equal; otherwise, false.</returns>
    public static bool operator ==(Document? left, Document? right) =>
        EqualityComparer<Document>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two Document instances are not equal.
    /// </summary>
    /// <param name="left">The first Document to compare.</param>
    /// <param name="right">The second Document to compare.</param>
    /// <returns>true if the Documents are not equal; otherwise, false.</returns>
    public static bool operator !=(Document? left, Document? right) => !(left == right);

    /// <summary>
    /// Determines whether one Document precedes another in the ordering.
    /// </summary>
    /// <param name="left">The first Document to compare.</param>
    /// <param name="right">The second Document to compare.</param>
    /// <returns>true if the left Document precedes the right Document; otherwise, false.</returns>
    public static bool operator <(Document? left, Document? right) =>
        left is not null && (right is null || left.CompareTo(right) < 0);

    /// <summary>
    /// Determines whether one Document precedes or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first Document to compare.</param>
    /// <param name="right">The second Document to compare.</param>
    /// <returns>true if the left Document precedes or equals the right Document; otherwise, false.</returns>
    public static bool operator <=(Document? left, Document? right) =>
        left is null || (right is not null && left.CompareTo(right) <= 0);

    /// <summary>
    /// Determines whether one Document follows another in the ordering.
    /// </summary>
    /// <param name="left">The first Document to compare.</param>
    /// <param name="right">The second Document to compare.</param>
    /// <returns>true if the left Document follows the right Document; otherwise, false.</returns>
    public static bool operator >(Document? left, Document? right) =>
        left is not null && (right is null || left.CompareTo(right) > 0);

    /// <summary>
    /// Determines whether one Document follows or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first Document to compare.</param>
    /// <param name="right">The second Document to compare.</param>
    /// <returns>true if the left Document follows or equals the right Document; otherwise, false.</returns>
    public static bool operator >=(Document? left, Document? right) =>
        left is null || (right is not null && left.CompareTo(right) >= 0);

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the Document.
    /// </summary>
    /// <returns>A string that represents the current Document.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the document,
    /// which is useful for debugging, logging, and display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var document = new Document 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     FileName = "Contract", 
    ///     Extension = "pdf",
    ///     FileSize = 1024576,
    ///     IsDeleted = false
    /// };
    /// 
    /// Console.WriteLine(document);
    /// // Output: "Document: Contract.pdf (1.0 MB) - Active"
    /// </code>
    /// </example>
    public override string ToString()
    {
        var status = IsDeleted ? "Deleted" : IsCheckedOut ? "Checked Out" : "Active";
        return $"Document: {FullFileName} ({FormattedFileSize}) - {status}";
    }

    #endregion String Representation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this document is available for editing.
    /// </summary>
    /// <returns>true if the document can be edited; otherwise, false.</returns>
    /// <remarks>
    /// This method checks business rules to determine if the document can be edited,
    /// considering checkout status and deletion state.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.IsAvailableForEditing())
    /// {
    ///     // Allow edit operations
    /// }
    /// else
    /// {
    ///     // Show appropriate message to user
    /// }
    /// </code>
    /// </example>
    public bool IsAvailableForEditing() => !IsDeleted && !IsCheckedOut;

    /// <summary>
    /// Gets usage statistics for this document.
    /// </summary>
    /// <returns>A dictionary containing document statistics.</returns>
    /// <remarks>
    /// This method provides insights into the document's usage patterns and history
    /// for reporting and analysis purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = document.GetDocumentStatistics();
    /// Console.WriteLine($"File size: {stats["FileSize"]}");
    /// Console.WriteLine($"Total activities: {stats["TotalActivities"]}");
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetDocumentStatistics()
    {
        return new Dictionary<string, object>
        {
            ["DocumentId"] = Id,
            ["FileName"] = FullFileName,
            ["FileSize"] = FileSize,
            ["FormattedFileSize"] = FormattedFileSize,
            ["MimeType"] = MimeType,
            ["Extension"] = Extension,
            ["IsDeleted"] = IsDeleted,
            ["IsCheckedOut"] = IsCheckedOut,
            ["RevisionCount"] = RevisionCount,
            ["LatestRevisionNumber"] = LatestRevisionNumber,
            ["TotalActivities"] = TotalActivityCount,
            ["HasActivityHistory"] = HasActivityHistory,
            ["CanBeDeleted"] = CanBeDeleted,
            ["CanBeCheckedOut"] = CanBeCheckedOut,
            ["CanBeCheckedIn"] = CanBeCheckedIn,
            ["IsAvailableForEditing"] = IsAvailableForEditing(),
            ["IsPdf"] = IsPdf,
            ["IsImage"] = IsImage,
            ["IsOfficeDocument"] = IsOfficeDocument
        };
    }

    /// <summary>
    /// Creates the next revision number for this document.
    /// </summary>
    /// <returns>The next sequential revision number.</returns>
    /// <remarks>
    /// This method calculates the next revision number based on existing revisions,
    /// ensuring sequential numbering without gaps.
    /// </remarks>
    /// <example>
    /// <code>
    /// var nextRevision = new Revision
    /// {
    ///     DocumentId = document.Id,
    ///     RevisionNumber = document.GetNextRevisionNumber(),
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    public int GetNextRevisionNumber() => LatestRevisionNumber + 1;

    /// <summary>
    /// Gets the most recent activity performed on this document.
    /// </summary>
    /// <returns>The most recent DocumentActivityUser, or null if no activities exist.</returns>
    /// <remarks>
    /// This method provides access to the most recent activity for user interface
    /// display and business logic purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var lastActivity = document.GetMostRecentActivity();
    /// if (lastActivity != null)
    /// {
    ///     Console.WriteLine($"Last activity: {lastActivity.DocumentActivity?.Activity} " +
    ///                      $"by {lastActivity.User?.Name} at {lastActivity.CreatedAt}");
    /// }
    /// </code>
    /// </example>
    public DocumentActivityUser? GetMostRecentActivity()
    {
        return DocumentActivityUsers
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets activities of a specific type performed on this document.
    /// </summary>
    /// <param name="activityType">The activity type to filter by.</param>
    /// <returns>A collection of matching activities.</returns>
    /// <remarks>
    /// This method enables filtering activities by type for specific analysis
    /// and reporting purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var creationActivities = document.GetActivitiesByType("CREATED");
    /// var saveActivities = document.GetActivitiesByType("SAVED");
    /// </code>
    /// </example>
    public IEnumerable<DocumentActivityUser> GetActivitiesByType(string activityType)
    {
        if (string.IsNullOrWhiteSpace(activityType))
            return Enumerable.Empty<DocumentActivityUser>();

        return DocumentActivityUsers
            .Where(a => string.Equals(a.DocumentActivity?.Activity, activityType, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(a => a.CreatedAt);
    }

    /// <summary>
    /// Validates the file extension against allowed types.
    /// </summary>
    /// <returns>true if the extension is allowed; otherwise, false.</returns>
    /// <remarks>
    /// This method validates the document's extension against the system's allowed
    /// file types for security and compliance purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.IsValidFileExtension())
    /// {
    ///     // Extension is allowed
    /// }
    /// else
    /// {
    ///     // Extension is not permitted
    /// }
    /// </code>
    /// </example>
    public bool IsValidFileExtension()
    {
        // This would integrate with the FileValidationHelper from the controller
        var allowedExtensions = new[] { "pdf", "doc", "docx", "txt", "xls", "xlsx", "ppt", "pptx", "jpg", "jpeg", "png", "gif", "tiff" };
        return allowedExtensions.Contains(Extension.ToLowerInvariant());
    }

    #endregion Business Logic Methods

    #region Validation Methods

    /// <summary>
    /// Validates the current Document instance against business rules.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive validation including business rule compliance,
    /// file metadata validation, and security checks.
    /// </remarks>
    /// <example>
    /// <code>
    /// var validationResults = document.Validate();
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
                "Document ID cannot be empty.",
                [nameof(Id)]));
        }

        if (string.IsNullOrWhiteSpace(FileName))
        {
            results.Add(new ValidationResult(
                "File name is required and cannot be empty.",
                [nameof(FileName)]));
        }
        else if (FileName.Length > 128)
        {
            results.Add(new ValidationResult(
                "File name cannot exceed 128 characters.",
                [nameof(FileName)]));
        }

        if (string.IsNullOrWhiteSpace(Extension))
        {
            results.Add(new ValidationResult(
                "File extension is required and cannot be empty.",
                [nameof(Extension)]));
        }
        else
        {
            if (Extension.Length > 5)
            {
                results.Add(new ValidationResult(
                    "File extension cannot exceed 5 characters.",
                    [nameof(Extension)]));
            }

            if (Extension.StartsWith('.'))
            {
                results.Add(new ValidationResult(
                    "File extension should not include the leading dot.",
                    [nameof(Extension)]));
            }

            if (!IsValidFileExtension())
            {
                results.Add(new ValidationResult(
                    "File extension is not in the list of allowed file types.",
                    [nameof(Extension)]));
            }
        }

        // Validate file size
        if (FileSize <= 0)
        {
            results.Add(new ValidationResult(
                "File size must be greater than zero.",
                [nameof(FileSize)]));
        }

        // Validate MIME type
        if (string.IsNullOrWhiteSpace(MimeType))
        {
            results.Add(new ValidationResult(
                "MIME type is required and cannot be empty.",
                [nameof(MimeType)]));
        }

        // Validate checksum
        if (string.IsNullOrWhiteSpace(Checksum))
        {
            results.Add(new ValidationResult(
                "Checksum is required for file integrity verification.",
                [nameof(Checksum)]));
        }
        else
        {
            if (Checksum.Length != 64)
            {
                results.Add(new ValidationResult(
                    "Checksum must be exactly 64 characters (SHA256 hash).",
                    [nameof(Checksum)]));
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(Checksum, @"^[a-f0-9]{64}$"))
            {
                results.Add(new ValidationResult(
                    "Checksum must be a 64-character lowercase hexadecimal string.",
                    [nameof(Checksum)]));
            }
        }

        // Validate matter association
        if (MatterId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Matter ID is required for document association.",
                [nameof(MatterId)]));
        }

        // Business rule validations
        if (IsDeleted && IsCheckedOut)
        {
            results.Add(new ValidationResult(
                "A document cannot be both deleted and checked out simultaneously.",
                [nameof(IsDeleted), nameof(IsCheckedOut)]));
        }

        return results;
    }

    #endregion Validation Methods
}