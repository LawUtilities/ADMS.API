using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing a complete document with all associated metadata, relationships, and audit trails.
/// </summary>
/// <remarks>
/// This DTO serves as the most comprehensive representation of a document within the ADMS legal document management system,
/// corresponding to <see cref="ADMS.API.Entities.Document"/>. It provides complete document data including all revision 
/// collections, activity associations, and audit trail relationships for comprehensive document management operations 
/// where complete context and metadata are required.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Document Representation:</strong> Includes all document properties, relationships, and collections</item>
/// <item><strong>Full Revision History:</strong> Complete revision collections for comprehensive version control</item>
/// <item><strong>Comprehensive Activity Integration:</strong> All document and transfer activity collections</item>
/// <item><strong>Extended Metadata:</strong> Additional description field for enhanced document classification</item>
/// <item><strong>Professional Validation:</strong> Uses centralized FileValidationHelper for comprehensive data integrity</item>
/// <item><strong>Entity Synchronization:</strong> Mirrors all properties and relationships from ADMS.API.Entities.Document</item>
/// <item><strong>Legal Compliance Support:</strong> Designed for comprehensive audit reporting and legal compliance</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO represents the complete structure from ADMS.API.Entities.Document including all relationships:
/// <list type="bullet">
/// <item><strong>Document Metadata:</strong> Complete file information, checksums, status flags, and extended description</item>
/// <item><strong>Revision History:</strong> Complete RevisionDto collection for comprehensive version control</item>
/// <item><strong>Document Activities:</strong> DocumentActivityUser collection for document-level audit trails</item>
/// <item><strong>Matter Transfer Activities:</strong> Bidirectional document transfer audit collections</item>
/// </list>
/// 
/// <para><strong>Supported Document Operations:</strong></para>
/// <list type="bullet">
/// <item><strong>Version Control:</strong> Complete revision history with sequential numbering and comprehensive metadata</item>
/// <item><strong>Document Activities:</strong> CREATED, SAVED, DELETED, RESTORED, CHECKED_IN, CHECKED_OUT operations</item>
/// <item><strong>Transfer Operations:</strong> MOVED/COPIED operations between matters with complete audit trails</item>
/// <item><strong>File Integrity:</strong> Checksum validation and file consistency verification</item>
/// <item><strong>Enhanced Classification:</strong> Extended description support for advanced document categorization</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Document Management:</strong> Comprehensive document administration with all metadata</item>
/// <item><strong>Advanced Classification:</strong> Extended description support for professional document organization</item>
/// <item><strong>Version Control Excellence:</strong> Professional-grade version control with complete revision history</item>
/// <item><strong>Audit Compliance:</strong> Comprehensive audit trail relationships for legal compliance</item>
/// <item><strong>Professional Accountability:</strong> Complete user attribution for all document operations</item>
/// <item><strong>Legal Discovery Support:</strong> Complete document history and provenance for legal proceedings</item>
/// </list>
/// 
/// <para><strong>File Management Features:</strong></para>
/// <list type="bullet">
/// <item><strong>File Validation:</strong> Comprehensive file name, extension, and MIME type validation</item>
/// <item><strong>Checksum Integrity:</strong> SHA256 checksum validation for file integrity verification</item>
/// <item><strong>Size Validation:</strong> File size validation with professional limits and constraints</item>
/// <item><strong>Status Management:</strong> Check-out status and deletion state management</item>
/// <item><strong>Enhanced Metadata:</strong> Additional description field for extended document classification</item>
/// </list>
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// <list type="bullet">
/// <item><strong>Comprehensive Data Loading:</strong> Includes all relationships and collections for complete context</item>
/// <item><strong>Selective Usage:</strong> Use lighter DTOs when complete metadata is not needed</item>
/// <item><strong>Validation Optimization:</strong> Efficient validation using centralized helpers</item>
/// <item><strong>Memory Considerations:</strong> Largest document DTO - use judiciously for performance-critical operations</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Document Administration:</strong> Full document management with all metadata and relationships</item>
/// <item><strong>Comprehensive Reporting:</strong> Complete document reports with all associated data</item>
/// <item><strong>Legal Compliance:</strong> Full document compliance reporting with complete audit trails</item>
/// <item><strong>Administrative Operations:</strong> Complete document lifecycle management</item>
/// <item><strong>API Responses:</strong> Complete document data when full context is required</item>
/// </list>
/// 
/// <para><strong>When to Use vs Other Document DTOs:</strong></para>
/// <list type="bullet">
/// <item><strong>Use DocumentFullDto:</strong> For complete document management, comprehensive reporting, and full metadata operations</item>
/// <item><strong>Use DocumentWithRevisionsDto:</strong> When revisions are needed but without extended metadata</item>
/// <item><strong>Use DocumentWithoutRevisionsDto:</strong> For document activities and transfers without revision history</item>
/// <item><strong>Use DocumentMinimalDto:</strong> For document identification, lists, and performance-critical operations</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive document DTO with all metadata
/// var documentDto = new DocumentFullDto
/// {
///     Id = Guid.NewGuid(),
///     FileName = "Corporate_Merger_Agreement_v3.pdf",
///     Extension = "pdf",
///     FileSize = 4123567,
///     MimeType = "application/pdf",
///     Checksum = "a3b5c7d9e1f2a4b6c8d0e2f4a6b8c0d2e4f6a8b0c2d4e6f8a0b2c4d6e8f0a2b4c6",
///     IsCheckedOut = false,
///     IsDeleted = false,
///     Description = "Primary merger agreement document with all exhibits and amendments",
///     Revisions = new List&lt;RevisionDto&gt;
///     {
///         new RevisionDto 
///         { 
///             Id = Guid.NewGuid(), 
///             RevisionNumber = 1, 
///             CreationDate = DateTime.UtcNow.AddDays(-30) 
///         },
///         new RevisionDto 
///         { 
///             Id = Guid.NewGuid(), 
///             RevisionNumber = 2, 
///             CreationDate = DateTime.UtcNow.AddDays(-15) 
///         },
///         new RevisionDto 
///         { 
///             Id = Guid.NewGuid(), 
///             RevisionNumber = 3, 
///             CreationDate = DateTime.UtcNow 
///         }
///     }
/// };
/// 
/// // Comprehensive validation
/// var validationResults = DocumentFullDto.ValidateModel(documentDto);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Document Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Professional document analysis with complete metadata
/// Console.WriteLine($"Document '{documentDto.FileName}' ({documentDto.FormattedFileSize})");
/// Console.WriteLine($"Description: {documentDto.Description}");
/// Console.WriteLine($"Version Control: {documentDto.RevisionCount} revisions, Current: v{documentDto.CurrentRevision?.RevisionNumber}");
/// Console.WriteLine($"Status: {documentDto.Status}, Activities: {documentDto.TotalActivityCount}");
/// Console.WriteLine($"File integrity: {(documentDto.HasValidChecksum ? "Verified" : "Compromised")}");
/// </code>
/// </example>
public partial class DocumentFullDto : IValidatableObject, IEquatable<DocumentFullDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the document.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the document within the ADMS system.
    /// It corresponds directly to <see cref="ADMS.API.Entities.Document.Id"/> and is used for 
    /// establishing relationships, revision associations, activity tracking, and all system operations 
    /// requiring precise document identification.
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required for Updates:</strong> Must be provided when updating existing documents</item>
    /// <item><strong>Foreign Key Reference:</strong> Referenced in Revision, DocumentActivityUser entities</item>
    /// <item><strong>API Operations:</strong> Document identification in REST API operations</item>
    /// <item><strong>Version Control:</strong> Links all revisions to the parent document</item>
    /// <item><strong>Activity Tracking:</strong> Links all activities and transfers to the document</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using document ID for comprehensive operations
    /// var document = new DocumentFullDto 
    /// { 
    ///     Id = Guid.Parse("87654321-4321-8765-2109-876543210987"),
    ///     FileName = "Complete_Legal_Contract.pdf"
    /// };
    /// 
    /// // Document identification in comprehensive API operations
    /// var documentUrl = $"/api/documents/{document.Id}/complete";
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Document ID is required for document identification.")]
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the file name of the document.
    /// </summary>
    /// <remarks>
    /// The file name serves as the primary human-readable identifier for the document and must conform 
    /// to professional file naming conventions. This field is validated using FileValidationHelper 
    /// to ensure compliance with file system requirements and professional standards.
    /// 
    /// <para><strong>Validation Rules (via ADMS.API.Common.FileValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null, empty, or whitespace</item>
    /// <item><strong>Length:</strong> Cannot exceed FileValidationHelper.MaxFileNameLength characters</item>
    /// <item><strong>Character Validation:</strong> Must contain valid file name characters only</item>
    /// <item><strong>Content Requirement:</strong> Must contain at least one alphanumeric character</item>
    /// <item><strong>Reserved Names:</strong> Cannot use system-reserved file names</item>
    /// <item><strong>Professional Standards:</strong> Should follow professional file naming conventions</item>
    /// </list>
    /// 
    /// <para><strong>Professional Significance:</strong></para>
    /// File names are critical for legal document organization, client communication, and system 
    /// integration. They must be descriptive, professional, and compliant with legal practice standards.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional file naming examples for comprehensive documents
    /// var contract = new DocumentFullDto { FileName = "Smith_Purchase_Agreement_Final_v3.pdf" };
    /// var motion = new DocumentFullDto { FileName = "Motion_Summary_Judgment_Supporting_Docs_2024.docx" };
    /// var exhibit = new DocumentFullDto { FileName = "Exhibit_A_Financial_Records_Complete.xlsx" };
    /// 
    /// // Validation example
    /// bool isValid = FileValidationHelper.IsFileNameValid(contract.FileName); // true
    /// </code>
    /// </example>
    [Required(ErrorMessage = "File name is required and cannot be empty.")]
    [MaxLength(FileValidationHelper.MaxFileNameLength,
        ErrorMessage = "File name cannot exceed {1} characters.")]
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets the file extension of the document.
    /// </summary>
    /// <remarks>
    /// The file extension identifies the document format and must be one of the allowed extensions 
    /// for the ADMS system. This field is validated using FileValidationHelper to ensure only 
    /// approved file types are accepted for legal document management.
    /// 
    /// <para><strong>Validation Rules (via ADMS.API.Common.FileValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null, empty, or whitespace</item>
    /// <item><strong>Length:</strong> Cannot exceed FileValidationHelper.MaxExtensionLength characters</item>
    /// <item><strong>Allowed Extensions:</strong> Must be in FileValidationHelper.AllowedExtensionsList</item>
    /// <item><strong>Format:</strong> Cannot contain whitespace characters</item>
    /// <item><strong>Case Sensitivity:</strong> Must be lowercase for consistency</item>
    /// </list>
    /// 
    /// <para><strong>Supported Legal Document Formats:</strong></para>
    /// Common extensions include PDF, DOC, DOCX, XLS, XLSX, TXT for comprehensive legal 
    /// document support while maintaining security and compatibility standards.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Supported document extensions for comprehensive documents
    /// var pdfDoc = new DocumentFullDto { Extension = "pdf" };   // Legal contracts, pleadings
    /// var wordDoc = new DocumentFullDto { Extension = "docx" }; // Draft documents
    /// var excelDoc = new DocumentFullDto { Extension = "xlsx" }; // Financial data
    /// 
    /// // Extension validation
    /// bool isAllowed = FileValidationHelper.IsExtensionAllowed(pdfDoc.Extension); // true
    /// </code>
    /// </example>
    [Required(ErrorMessage = "File extension is required and cannot be empty.")]
    [MaxLength(FileValidationHelper.MaxExtensionLength,
        ErrorMessage = "File extension cannot exceed {1} characters.")]
    public required string Extension { get; set; }

    /// <summary>
    /// Gets or sets the size of the file in bytes.
    /// </summary>
    /// <remarks>
    /// The file size provides essential metadata for storage management, transfer operations, and 
    /// system performance optimization. File size validation ensures documents are within acceptable 
    /// limits for the legal document management system.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Non-negative:</strong> Must be zero or positive (handled by Range attribute)</item>
    /// <item><strong>Positive Requirement:</strong> Must be greater than zero for actual files</item>
    /// <item><strong>System Limits:</strong> Should be within reasonable bounds for legal documents</item>
    /// <item><strong>Performance Consideration:</strong> Large files may require special handling</item>
    /// </list>
    /// 
    /// <para><strong>Professional Context:</strong></para>
    /// File size information supports storage planning, network transfer optimization, and helps 
    /// identify potentially corrupted files in legal document workflows.
    /// </remarks>
    /// <example>
    /// <code>
    /// // File size examples for comprehensive documents
    /// var smallDoc = new DocumentFullDto { FileSize = 456789 };     // 446 KB
    /// var mediumDoc = new DocumentFullDto { FileSize = 5247832 };   // 5.0 MB
    /// var largeDoc = new DocumentFullDto { FileSize = 25728640 };   // 24.5 MB
    /// 
    /// // Size validation and display
    /// var sizeInMB = document.FileSize / (1024.0 * 1024.0);
    /// Console.WriteLine($"Document size: {sizeInMB:F2} MB");
    /// </code>
    /// </example>
    [Range(0, long.MaxValue, ErrorMessage = "File size must be non-negative.")]
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the file (e.g., "application/pdf").
    /// </summary>
    /// <remarks>
    /// The MIME type provides precise content type identification essential for proper document handling, 
    /// security validation, and client application integration. MIME types must conform to standard 
    /// formats and be approved for legal document management.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null, empty, or whitespace</item>
    /// <item><strong>Length:</strong> Maximum 128 characters for standard MIME type formats</item>
    /// <item><strong>Format:</strong> Must match standard MIME type pattern (type/subtype)</item>
    /// <item><strong>Allowed Types:</strong> Must be in FileValidationHelper.AllowedMimeTypes</item>
    /// <item><strong>Security:</strong> Validated to prevent security vulnerabilities</item>
    /// </list>
    /// 
    /// <para><strong>Common Legal Document MIME Types:</strong></para>
    /// <list type="bullet">
    /// <item><strong>application/pdf:</strong> PDF documents for final legal documents</item>
    /// <item><strong>application/vnd.openxmlformats-officedocument.wordprocessingml.document:</strong> DOCX files</item>
    /// <item><strong>application/vnd.ms-excel:</strong> Excel files for financial data</item>
    /// <item><strong>text/plain:</strong> Plain text files for simple documents</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional MIME type examples for comprehensive documents
    /// var pdfContract = new DocumentFullDto { MimeType = "application/pdf" };
    /// var wordDoc = new DocumentFullDto { 
    ///     MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document" 
    /// };
    /// var excelSheet = new DocumentFullDto { MimeType = "application/vnd.ms-excel" };
    /// 
    /// // MIME type validation
    /// bool isValidMime = FileValidationHelper.IsMimeTypeAllowed(pdfContract.MimeType);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "MIME type is required for proper document handling.")]
    [MaxLength(128, ErrorMessage = "MIME type cannot exceed 128 characters.")]
    [RegularExpression(@"^[\w\.\-]+\/[\w\.\-\+]+$", ErrorMessage = "Invalid MIME type format.")]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the checksum (SHA256 hash) of the file for integrity verification.
    /// </summary>
    /// <remarks>
    /// The checksum provides critical file integrity verification capabilities essential for legal 
    /// document management. SHA256 hashing ensures document authenticity, detects corruption, and 
    /// supports compliance with legal document integrity requirements.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null, empty, or whitespace</item>
    /// <item><strong>Length:</strong> Must be exactly 64 characters for SHA256 hashes</item>
    /// <item><strong>Format:</strong> Must be valid hexadecimal characters only</item>
    /// <item><strong>Case Insensitive:</strong> Accepts both uppercase and lowercase hex digits</item>
    /// <item><strong>Integrity:</strong> Must match the actual file content hash</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Authenticity:</strong> Verifies documents haven't been altered</item>
    /// <item><strong>Corruption Detection:</strong> Identifies file corruption during storage/transfer</item>
    /// <item><strong>Legal Evidence:</strong> Supports document integrity in legal proceedings</item>
    /// <item><strong>Compliance:</strong> Meets regulatory requirements for document integrity</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // SHA256 checksum examples for comprehensive documents
    /// var document = new DocumentFullDto 
    /// { 
    ///     Checksum = "a3b5c7d9e1f2a4b6c8d0e2f4a6b8c0d2e4f6a8b0c2d4e6f8a0b2c4d6e8f0a2b4c6",
    ///     FileName = "Complete_Contract.pdf"
    /// };
    /// 
    /// // Integrity verification
    /// bool integrityValid = document.HasValidChecksum;
    /// Console.WriteLine($"Document integrity: {(integrityValid ? "Valid" : "Compromised")}");
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Checksum is required for document integrity verification.")]
    [MaxLength(128, ErrorMessage = "Checksum cannot exceed 128 characters.")]
    [RegularExpression(@"^[A-Fa-f0-9]+$", ErrorMessage = "Checksum must be a hexadecimal string.")]
    public string Checksum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the document is checked out.
    /// </summary>
    /// <remarks>
    /// The check-out status provides document locking functionality essential for collaborative legal 
    /// document editing. When checked out, the document is typically locked for editing by a specific 
    /// user to prevent concurrent modification conflicts.
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Mutual Exclusivity:</strong> Cannot be checked out and deleted simultaneously</item>
    /// <item><strong>User Attribution:</strong> Check-out status should be associated with specific users</item>
    /// <item><strong>Temporal Tracking:</strong> Check-out events are tracked in audit trails</item>
    /// <item><strong>Professional Workflow:</strong> Supports professional document collaboration</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Document check-out status for comprehensive documents
    /// var document = new DocumentFullDto 
    /// { 
    ///     IsCheckedOut = true,
    ///     IsDeleted = false  // Business rule: cannot be both
    /// };
    /// 
    /// if (document.IsAvailableForEdit)
    /// {
    ///     Console.WriteLine("Document is available for editing");
    /// }
    /// </code>
    /// </example>
    public bool IsCheckedOut { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document is deleted.
    /// </summary>
    /// <remarks>
    /// The deletion flag implements soft deletion functionality, preserving document data for audit 
    /// trails while marking it as logically deleted. This approach maintains legal compliance by 
    /// preserving historical data while removing documents from active use.
    /// 
    /// <para><strong>Soft Deletion Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Audit Trail Preservation:</strong> Maintains complete document history</item>
    /// <item><strong>Legal Compliance:</strong> Supports legal requirements for data retention</item>
    /// <item><strong>Recovery Capability:</strong> Enables document restoration when needed</item>
    /// <item><strong>Professional Accountability:</strong> Tracks deletion activities and attribution</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Mutual Exclusivity:</strong> Cannot be deleted and checked out simultaneously</item>
    /// <item><strong>Restoration Support:</strong> Deleted documents can be restored through audit activities</item>
    /// <item><strong>Access Restriction:</strong> Deleted documents are hidden from normal operations</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Document deletion status for comprehensive documents
    /// var document = new DocumentFullDto 
    /// { 
    ///     IsDeleted = true,
    ///     IsCheckedOut = false  // Business rule: cannot be both
    /// };
    /// 
    /// var status = document.Status; // Returns appropriate status based on flags
    /// </code>
    /// </example>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the extended description of the document for enhanced categorization and professional organization.
    /// </summary>
    /// <remarks>
    /// The description field provides additional metadata capabilities for comprehensive document management,
    /// corresponding to <see cref="ADMS.API.Entities.Document.Description"/>. This optional field enables 
    /// enhanced document categorization, professional organization, and detailed documentation of document 
    /// purpose and content for legal practice management.
    /// 
    /// <para><strong>Professional Usage Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Enhanced Categorization:</strong> Provides detailed document classification beyond file names</item>
    /// <item><strong>Client Communication:</strong> Professional descriptions for client document references</item>
    /// <item><strong>Legal Practice Organization:</strong> Advanced document organization and retrieval capabilities</item>
    /// <item><strong>Search Optimization:</strong> Improved document search and discovery through descriptive metadata</item>
    /// <item><strong>Professional Standards:</strong> Maintains professional document identification standards</item>
    /// </list>
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Optional Field:</strong> Can be null or empty without affecting document functionality</item>
    /// <item><strong>Length Limit:</strong> Maximum 256 characters for reasonable description length</item>
    /// <item><strong>Professional Content:</strong> Should contain professional, descriptive content when provided</item>
    /// <item><strong>Character Validation:</strong> Must not contain invalid characters that could cause system issues</item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Classification:</strong> Detailed categorization of document types and purposes</item>
    /// <item><strong>Client Reports:</strong> Professional document descriptions in client communications</item>
    /// <item><strong>Legal Discovery:</strong> Enhanced document identification for legal discovery processes</item>
    /// <item><strong>Practice Management:</strong> Advanced document organization and workflow management</item>
    /// <item><strong>System Integration:</strong> Enhanced metadata for system integration and automation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional document descriptions for comprehensive documents
    /// var contract = new DocumentFullDto 
    /// { 
    ///     FileName = "Smith_Purchase_Agreement.pdf",
    ///     Description = "Primary purchase agreement for Smith residential property transaction including all standard contingencies and buyer protections"
    /// };
    /// 
    /// var motion = new DocumentFullDto 
    /// { 
    ///     FileName = "Motion_Summary_Judgment.docx",
    ///     Description = "Motion for summary judgment in ABC Corp v. XYZ LLC with supporting legal arguments and case citations"
    /// };
    /// 
    /// var exhibit = new DocumentFullDto 
    /// { 
    ///     FileName = "Financial_Records.xlsx",
    ///     Description = "Comprehensive financial records including balance sheets, income statements, and cash flow analysis for litigation support"
    /// };
    /// 
    /// // Using description for enhanced search and organization
    /// bool hasDescription = !string.IsNullOrWhiteSpace(contract.Description);
    /// var searchableContent = hasDescription ? $"{contract.FileName} - {contract.Description}" : contract.FileName;
    /// </code>
    /// </example>
    [MaxLength(256, ErrorMessage = "Description cannot exceed 256 characters.")]
    public string? Description { get; set; }

    #endregion Core Properties

    #region Collection Properties

    /// <summary>
    /// Gets or sets the collection of revisions for this document.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.Document.Revisions"/> and maintains the 
    /// complete version control history for the document. Each revision represents a specific version 
    /// with sequential numbering and comprehensive audit trails essential for legal document management.
    /// 
    /// <para><strong>Version Control Features:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Sequential Numbering:</strong> Revisions are numbered sequentially starting from 1</item>
    /// <item><strong>Complete History:</strong> Maintains complete chronological document evolution</item>
    /// <item><strong>Audit Trails:</strong> Each revision includes comprehensive user activity tracking</item>
    /// <item><strong>Professional Standards:</strong> Supports professional document versioning requirements</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance Support:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Evolution:</strong> Tracks complete document development history</item>
    /// <item><strong>Professional Accountability:</strong> Complete user attribution for all changes</item>
    /// <item><strong>Legal Discovery:</strong> Comprehensive version history for legal proceedings</item>
    /// <item><strong>Compliance Reporting:</strong> Detailed version control for regulatory compliance</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Minimum Requirement:</strong> Documents should have at least one revision for completeness</item>
    /// <item><strong>Sequential Integrity:</strong> Revision numbers must be sequential without gaps</item>
    /// <item><strong>Temporal Consistency:</strong> Revision dates must be chronologically consistent</item>
    /// <item><strong>Validation Completeness:</strong> Each revision must pass comprehensive validation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Comprehensive document with complete revision history
    /// var document = new DocumentFullDto
    /// {
    ///     FileName = "Corporate_Merger_Agreement.pdf",
    ///     Description = "Complete merger agreement with all amendments and exhibits",
    ///     Revisions = new List&lt;RevisionDto&gt;
    ///     {
    ///         new RevisionDto 
    ///         { 
    ///             RevisionNumber = 1, 
    ///             CreationDate = DateTime.UtcNow.AddDays(-30),
    ///             ModificationDate = DateTime.UtcNow.AddDays(-30)
    ///         },
    ///         new RevisionDto 
    ///         { 
    ///             RevisionNumber = 2, 
    ///             CreationDate = DateTime.UtcNow.AddDays(-15),
    ///             ModificationDate = DateTime.UtcNow.AddDays(-14)
    ///         },
    ///         new RevisionDto 
    ///         { 
    ///             RevisionNumber = 3, 
    ///             CreationDate = DateTime.UtcNow.AddDays(-1),
    ///             ModificationDate = DateTime.UtcNow
    ///         }
    ///     }
    /// };
    /// 
    /// // Comprehensive version analysis
    /// var currentVersion = document.CurrentRevision;
    /// var versionHistory = document.RevisionHistory;
    /// var developmentTimespan = document.DocumentDevelopmentTimespan;
    /// Console.WriteLine($"Document has {document.RevisionCount} versions spanning {developmentTimespan?.TotalDays:F0} days");
    /// </code>
    /// </example>
    public ICollection<RevisionDto> Revisions { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of document activity users.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.Document.DocumentActivityUsers"/> and tracks 
    /// all document-level activities performed by users. This provides comprehensive audit trails for 
    /// document operations essential for legal compliance and professional accountability.
    /// 
    /// <para><strong>Activity Types Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Document creation activities</item>
    /// <item><strong>SAVED:</strong> Document save operations</item>
    /// <item><strong>DELETED:</strong> Document deletion activities</item>
    /// <item><strong>RESTORED:</strong> Document restoration activities</item>
    /// <item><strong>CHECKED_IN:</strong> Document check-in operations</item>
    /// <item><strong>CHECKED_OUT:</strong> Document check-out operations</item>
    /// </list>
    /// 
    /// <para><strong>Professional Accountability:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Attribution:</strong> Complete user accountability for document operations</item>
    /// <item><strong>Activity Classification:</strong> Categorization of operations for audit analysis</item>
    /// <item><strong>Temporal Tracking:</strong> Precise temporal tracking for audit chronology</item>
    /// <item><strong>Legal Compliance:</strong> Complete audit trail for legal and regulatory requirements</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing comprehensive document audit trail
    /// foreach (var activity in document.DocumentActivityUsers.OrderBy(a => a.CreatedAt))
    /// {
    ///     Console.WriteLine($"User {activity.User?.Name} performed {activity.DocumentActivity?.Activity} " +
    ///                      $"on document at {activity.CreatedAt}");
    /// }
    /// 
    /// // Finding comprehensive document lifecycle events
    /// var creator = document.DocumentActivityUsers
    ///     .FirstOrDefault(da => da.DocumentActivity?.Activity == "CREATED")?.User;
    /// var lastSaved = document.DocumentActivityUsers
    ///     .Where(da => da.DocumentActivity?.Activity == "SAVED")
    ///     .OrderByDescending(da => da.CreatedAt)
    ///     .FirstOrDefault();
    /// </code>
    /// </example>
    public ICollection<DocumentActivityUserDto> DocumentActivityUsers { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of "from" matter document activity users.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.Document.MatterDocumentActivityUsersFrom"/> and 
    /// tracks all document transfer activities where this document was moved or copied FROM source matters to 
    /// destination matters. This provides source-side audit trails for document transfer operations.
    /// 
    /// <para><strong>Transfer Operations Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> Documents moved FROM source matters</item>
    /// <item><strong>COPIED:</strong> Documents copied FROM source matters</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Document Provenance:</strong> Complete tracking of document origins and sources</item>
    /// <item><strong>Custody Chain:</strong> Maintains complete document custody tracking</item>
    /// <item><strong>Professional Responsibility:</strong> Detailed user attribution for document handling</item>
    /// <item><strong>Audit Trail Preservation:</strong> Complete audit trail for compliance requirements</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Analyzing comprehensive document transfer sources
    /// foreach (var transferFrom in document.MatterDocumentActivityUsersFrom)
    /// {
    ///     Console.WriteLine($"Document {transferFrom.MatterDocumentActivity?.Activity} " +
    ///                      $"from matter '{transferFrom.Matter?.Description}' " +
    ///                      $"by {transferFrom.User?.Name} at {transferFrom.CreatedAt}");
    /// }
    /// 
    /// // Comprehensive transfer pattern analysis
    /// var transferPatterns = document.MatterDocumentActivityUsersFrom
    ///     .GroupBy(t => new { t.MatterDocumentActivity?.Activity, Month = t.CreatedAt.Month })
    ///     .Select(g => new { g.Key.Activity, g.Key.Month, Count = g.Count() });
    /// </code>
    /// </example>
    public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of "to" matter document activity users.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.Document.MatterDocumentActivityUsersTo"/> and 
    /// tracks all document transfer activities where this document was moved or copied TO destination matters 
    /// from source matters. This provides destination-side audit trails completing the bidirectional transfer system.
    /// 
    /// <para><strong>Transfer Operations Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> Documents moved to destination matters</item>
    /// <item><strong>COPIED:</strong> Documents copied to destination matters</item>
    /// </list>
    /// 
    /// <para><strong>Bidirectional Audit System:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Complete Coverage:</strong> Together with MatterDocumentActivityUsersFrom provides complete audit coverage</item>
    /// <item><strong>Legal Compliance:</strong> Ensures every document movement is fully documented</item>
    /// <item><strong>Professional Accountability:</strong> Complete user attribution for both source and destination operations</item>
    /// <item><strong>Audit Trail Integrity:</strong> Maintains comprehensive audit trail integrity across all transfers</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Analyzing comprehensive document transfer destinations
    /// foreach (var transferTo in document.MatterDocumentActivityUsersTo)
    /// {
    ///     Console.WriteLine($"Document {transferTo.MatterDocumentActivity?.Activity} " +
    ///                      $"to matter '{transferTo.Matter?.Description}' " +
    ///                      $"by {transferTo.User?.Name} at {transferTo.CreatedAt}");
    /// }
    /// 
    /// // Complete comprehensive transfer analysis
    /// var totalTransfers = document.MatterDocumentActivityUsersFrom.Count + 
    ///                     document.MatterDocumentActivityUsersTo.Count;
    /// var transferMetrics = new
    /// {
    ///     TotalTransfers = totalTransfers,
    ///     OutboundTransfers = document.MatterDocumentActivityUsersFrom.Count,
    ///     InboundTransfers = document.MatterDocumentActivityUsersTo.Count,
    ///     RecentActivity = DateTime.UtcNow.AddDays(-30)
    /// };
    /// </code>
    /// </example>
    public ICollection<MatterDocumentActivityUserToDto> MatterDocumentActivityUsersTo { get; set; } = [];

    #endregion Collection Properties

    #region Computed Properties

    /// <summary>
    /// Gets the current (latest) revision of the document.
    /// </summary>
    /// <remarks>
    /// This computed property provides quick access to the most recent revision, which is typically 
    /// the active version of the document. The current revision is determined by the highest 
    /// revision number in the collection.
    /// 
    /// <para><strong>Version Control Integration:</strong></para>
    /// The current revision represents the latest state of the document and is used for most 
    /// document operations and displays.
    /// </remarks>
    /// <example>
    /// <code>
    /// var latestRevision = document.CurrentRevision;
    /// if (latestRevision != null)
    /// {
    ///     Console.WriteLine($"Current version: {latestRevision.RevisionNumber}");
    ///     Console.WriteLine($"Last modified: {latestRevision.ModificationDate}");
    /// }
    /// </code>
    /// </example>
    public RevisionDto? CurrentRevision => Revisions
        .Where(r => r != null)
        .OrderByDescending(r => r.RevisionNumber)
        .FirstOrDefault();

    /// <summary>
    /// Gets the total count of revisions for this document.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.Document.RevisionCount"/> and provides 
    /// a quick count of document versions, useful for version control analysis and display.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Document has {document.RevisionCount} versions");
    /// </code>
    /// </example>
    public int RevisionCount => Revisions.Count;

    /// <summary>
    /// Gets the complete revision history ordered chronologically.
    /// </summary>
    /// <remarks>
    /// This computed property provides a chronologically ordered view of all revisions, 
    /// useful for displaying version history and tracking document evolution.
    /// </remarks>
    /// <example>
    /// <code>
    /// var history = document.RevisionHistory;
    /// foreach (var revision in history)
    /// {
    ///     Console.WriteLine($"Version {revision.RevisionNumber}: {revision.CreationDate} - {revision.ModificationDate}");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<RevisionDto> RevisionHistory => Revisions
        .Where(r => r != null)
        .OrderBy(r => r.RevisionNumber);

    /// <summary>
    /// Gets the total count of all activities (document + transfer) for this document.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.Document.TotalActivityCount"/> and provides 
    /// a comprehensive count of all activities associated with the document, useful for activity analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Total document activities: {document.TotalActivityCount}");
    /// </code>
    /// </example>
    public int TotalActivityCount => DocumentActivityUsers.Count +
                                   MatterDocumentActivityUsersFrom.Count +
                                   MatterDocumentActivityUsersTo.Count;

    /// <summary>
    /// Gets a value indicating whether this document has any activities recorded.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.Document.HasActivities"/> and 
    /// determines if the document has been used in the system, useful for activity analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.HasActivities)
    /// {
    ///     Console.WriteLine("Document has comprehensive activity history");
    /// }
    /// </code>
    /// </example>
    public bool HasActivities => TotalActivityCount > 0;

    /// <summary>
    /// Gets a value indicating whether the document is available for editing.
    /// </summary>
    /// <remarks>
    /// This computed property determines document availability based on check-out and deletion status, 
    /// useful for UI controls and business logic decisions.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.IsAvailableForEdit)
    /// {
    ///     // Enable comprehensive editing controls
    /// }
    /// </code>
    /// </example>
    public bool IsAvailableForEdit => !IsCheckedOut && !IsDeleted;

    /// <summary>
    /// Gets the document status as a human-readable string.
    /// </summary>
    /// <remarks>
    /// This computed property provides a descriptive status based on the document's current state, 
    /// useful for UI display and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// var status = document.Status; // "Active", "Checked Out", "Deleted", etc.
    /// </code>
    /// </example>
    public string Status => (IsDeleted, IsCheckedOut) switch
    {
        (true, _) => "Deleted",
        (false, true) => "Checked Out",
        (false, false) => "Active",
    };

    /// <summary>
    /// Gets a value indicating whether the checksum appears to be valid.
    /// </summary>
    /// <remarks>
    /// This computed property performs basic checksum validation to verify it's a properly 
    /// formatted SHA256 hash, useful for integrity verification display.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.HasValidChecksum)
    /// {
    ///     Console.WriteLine("Document integrity verified");
    /// }
    /// </code>
    /// </example>
    public bool HasValidChecksum => !string.IsNullOrWhiteSpace(Checksum) &&
                                   ChecksumRegex().IsMatch(Checksum) &&
                                   Checksum.Length == 64;

    /// <summary>
    /// Gets the file size formatted as a human-readable string.
    /// </summary>
    /// <remarks>
    /// This computed property formats the file size in appropriate units (bytes, KB, MB, GB) 
    /// for user-friendly display.
    /// </remarks>
    /// <example>
    /// <code>
    /// var sizeText = document.FormattedFileSize; // "5.2 MB"
    /// </code>
    /// </example>
    public string FormattedFileSize => FileSize switch
    {
        < 1024 => $"{FileSize} bytes",
        < 1024 * 1024 => $"{FileSize / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{FileSize / (1024.0 * 1024.0):F1} MB",
        _ => $"{FileSize / (1024.0 * 1024.0 * 1024.0):F1} GB"
    };

    /// <summary>
    /// Gets the display text suitable for UI controls and document identification.
    /// </summary>
    /// <remarks>
    /// Provides a consistent format for displaying document information in UI elements, 
    /// including the file name and optionally the description for enhanced identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// var displayText = document.DisplayText; // Returns file name or file name with description
    /// documentDropdown.Items.Add(new ListItem(document.DisplayText, document.Id.ToString()));
    /// </code>
    /// </example>
    public string DisplayText => !string.IsNullOrWhiteSpace(Description)
        ? $"{FileName} - {Description}"
        : FileName;

    /// <summary>
    /// Gets the full file name including extension for complete identification.
    /// </summary>
    /// <remarks>
    /// This computed property combines the file name and extension to provide the complete 
    /// file identifier as it would appear in a file system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var fullName = document.FullFileName; // "Contract_Amendment.pdf"
    /// </code>
    /// </example>
    public string FullFileName => string.IsNullOrWhiteSpace(Extension)
        ? FileName
        : $"{FileName}.{Extension}";

    /// <summary>
    /// Gets the timespan representing the document's development period from first to latest revision.
    /// </summary>
    /// <remarks>
    /// This computed property calculates the total development timespan for documents with multiple revisions,
    /// useful for analyzing document development patterns and project timelines.
    /// </remarks>
    /// <example>
    /// <code>
    /// var developmentTime = document.DocumentDevelopmentTimespan;
    /// if (developmentTime.HasValue)
    /// {
    ///     Console.WriteLine($"Document developed over {developmentTime.Value.TotalDays:F0} days");
    /// }
    /// </code>
    /// </example>
    public TimeSpan? DocumentDevelopmentTimespan
    {
        get
        {
            if (RevisionCount < 2) return null;

            var orderedRevisions = Revisions
                .Where(r => r != null) // Removed r.CreationDate.HasValue
                .OrderBy(r => r.CreationDate)
                .ToList();

            if (orderedRevisions.Count < 2) return null;

            return orderedRevisions.Last().CreationDate - orderedRevisions.First().CreationDate;
        }
    }

    /// <summary>
    /// Gets a value indicating whether this document has an extended description.
    /// </summary>
    /// <remarks>
    /// This computed property indicates whether the document includes extended descriptive metadata,
    /// useful for determining display options and search capabilities.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.HasDescription)
    /// {
    ///     // Show extended description in UI
    ///     Console.WriteLine($"Description: {document.Description}");
    /// }
    /// </code>
    /// </example>
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="DocumentFullDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using centralized validation helpers for consistency with entity
    /// validation rules. This ensures the DTO maintains the same validation standards as the corresponding
    /// ADMS.API.Entities.Document entity while enforcing professional document management standards.
    /// 
    /// <para><strong>Comprehensive Validation Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>ID Validation:</strong> Document ID validated for proper GUID structure</item>
    /// <item><strong>File System Validation:</strong> File name, extension, and metadata validation</item>
    /// <item><strong>Integrity Validation:</strong> Checksum, MIME type, and file size validation</item>
    /// <item><strong>Extended Validation:</strong> Description field validation for professional standards</item>
    /// <item><strong>Collection Validation:</strong> Deep validation of revision and audit trail collections</item>
    /// <item><strong>Business Rule Validation:</strong> Document-specific business rule compliance</item>
    /// </list>
    /// 
    /// <para><strong>Professional Standards Integration:</strong></para>
    /// Uses centralized validation helpers (FileValidationHelper, DtoValidationHelper) to ensure
    /// consistency across all document validation in the system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new DocumentFullDto 
    /// { 
    ///     Id = Guid.Empty, // Invalid
    ///     FileName = "", // Invalid
    ///     Description = new string('A', 300), // Invalid - too long
    ///     Revisions = new List&lt;RevisionDto&gt; { null } // Invalid
    /// };
    /// 
    /// var context = new ValidationContext(dto);
    /// var results = dto.Validate(context);
    /// 
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"Document Validation Error: {result.ErrorMessage}");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate document ID
        foreach (var result in ValidateDocumentId())
            yield return result;

        // Validate file system properties
        foreach (var result in ValidateFileName())
            yield return result;

        foreach (var result in ValidateExtension())
            yield return result;

        foreach (var result in ValidateFileSize())
            yield return result;

        foreach (var result in ValidateMimeType())
            yield return result;

        foreach (var result in ValidateChecksum())
            yield return result;

        // Validate extended properties
        foreach (var result in ValidateDescription())
            yield return result;

        // Validate business rules
        foreach (var result in ValidateBusinessRules())
            yield return result;

        // Validate collections using centralized helper
        foreach (var result in ValidateRevisions())
            yield return result;

        foreach (var result in ValidateDocumentActivityUsers())
            yield return result;

        foreach (var result in ValidateMatterDocumentActivityUsersFrom())
            yield return result;

        foreach (var result in ValidateMatterDocumentActivityUsersTo())
            yield return result;
    }

    /// <summary>
    /// Validates the <see cref="Id"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the document ID.</returns>
    /// <remarks>
    /// Validates that the document ID is a valid, non-empty GUID suitable for use as a database identifier.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateDocumentId()
    {
        if (Id == Guid.Empty)
        {
            yield return new ValidationResult(
                "Document ID must be a valid non-empty GUID for document identification.",
                [nameof(Id)]);
        }
    }

    /// <summary>
    /// Validates the <see cref="FileName"/> property using ADMS file validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the file name.</returns>
    /// <remarks>
    /// Uses FileValidationHelper for comprehensive file name validation including character validation,
    /// length constraints, reserved name checking, and professional naming standards.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateFileName()
    {
        if (string.IsNullOrWhiteSpace(FileName))
        {
            yield return new ValidationResult(
                "File name is required and cannot be empty.",
                [nameof(FileName)]);
            yield break;
        }

        if (FileName.Length > FileValidationHelper.MaxFileNameLength)
        {
            yield return new ValidationResult(
                $"File name cannot exceed {FileValidationHelper.MaxFileNameLength} characters.",
                [nameof(FileName)]);
        }

        if (!FileValidationHelper.IsFileNameValid(FileName))
        {
            yield return new ValidationResult(
                "File name contains invalid characters or format.",
                [nameof(FileName)]);
        }

        if (FileValidationHelper.IsReservedFileName(FileName))
        {
            yield return new ValidationResult(
                "File name cannot use a reserved system name.",
                [nameof(FileName)]);
        }

        if (!FileName.Any(char.IsLetterOrDigit))
        {
            yield return new ValidationResult(
                "File name must contain at least one alphanumeric character for professional identification.",
                [nameof(FileName)]);
        }
    }

    /// <summary>
    /// Validates the <see cref="Extension"/> property using ADMS file validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the file extension.</returns>
    /// <remarks>
    /// Uses FileValidationHelper for comprehensive extension validation including allowed extensions,
    /// length constraints, format validation, and case sensitivity checks.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateExtension()
    {
        if (string.IsNullOrWhiteSpace(Extension))
        {
            yield return new ValidationResult(
                "File extension is required and cannot be empty.",
                [nameof(Extension)]);
            yield break;
        }

        if (Extension.Length > FileValidationHelper.MaxExtensionLength)
        {
            yield return new ValidationResult(
                $"File extension cannot exceed {FileValidationHelper.MaxExtensionLength} characters.",
                [nameof(Extension)]);
        }

        if (!FileValidationHelper.IsExtensionAllowed(Extension))
        {
            yield return new ValidationResult(
                $"Extension '{Extension}' is not allowed for legal document management. " +
                $"Allowed extensions: {FileValidationHelper.AllowedExtensionsList}",
                [nameof(Extension)]);
        }

        if (Extension.Any(char.IsWhiteSpace))
        {
            yield return new ValidationResult(
                "File extension cannot contain whitespace characters.",
                [nameof(Extension)]);
        }

        // Check for lowercase consistency
        if (!Extension.Equals(Extension.ToLowerInvariant(), StringComparison.Ordinal))
        {
            yield return new ValidationResult(
                "File extension must be lowercase for consistency.",
                [nameof(Extension)]);
        }
    }

    /// <summary>
    /// Validates the <see cref="FileSize"/> property for professional document management standards.
    /// </summary>
    /// <returns>A collection of validation results for the file size.</returns>
    /// <remarks>
    /// Validates file size constraints ensuring documents are within acceptable limits for 
    /// legal document management operations.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateFileSize()
    {
        if (FileSize < 0)
        {
            yield return new ValidationResult(
                "File size must be non-negative for valid document metadata.",
                [nameof(FileSize)]);
        }

        if (FileSize == 0)
        {
            yield return new ValidationResult(
                "File size must be greater than zero for actual document files.",
                [nameof(FileSize)]);
        }

        // Additional file size constraints for professional document management
        if (FileSize > 100 * 1024 * 1024) // 100 MB limit for typical legal documents
        {
            yield return new ValidationResult(
                "File size exceeds recommended limit for efficient document management (100 MB).",
                [nameof(FileSize)]);
        }
    }

    /// <summary>
    /// Validates the <see cref="MimeType"/> property using ADMS file validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the MIME type.</returns>
    /// <remarks>
    /// Uses FileValidationHelper for comprehensive MIME type validation including allowed types,
    /// format validation, and security considerations.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateMimeType()
    {
        if (string.IsNullOrWhiteSpace(MimeType))
        {
            yield return new ValidationResult(
                "MIME type is required for proper document handling and security.",
                [nameof(MimeType)]);
            yield break;
        }

        if (!FileValidationHelper.IsMimeTypeAllowed(MimeType))
        {
            yield return new ValidationResult(
                $"MIME type '{MimeType}' is not allowed for legal document management.",
                [nameof(MimeType)]);
        }

        if (!MimeTypeRegex().IsMatch(MimeType))
        {
            yield return new ValidationResult(
                "Invalid MIME type format. Expected format: type/subtype (e.g., application/pdf).",
                [nameof(MimeType)]);
        }
    }

    /// <summary>
    /// Validates the <see cref="Checksum"/> property for document integrity verification.
    /// </summary>
    /// <returns>A collection of validation results for the checksum.</returns>
    /// <remarks>
    /// Validates checksum format and structure to ensure proper SHA256 hash for document integrity verification.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateChecksum()
    {
        if (string.IsNullOrWhiteSpace(Checksum))
        {
            yield return new ValidationResult(
                "Checksum is required for document integrity verification and legal compliance.",
                [nameof(Checksum)]);
            yield break;
        }

        if (!ChecksumRegex().IsMatch(Checksum) || Checksum.Length != 64)
        {
            yield return new ValidationResult(
                "Checksum must be a valid 64-character hexadecimal string (SHA256 hash) for proper integrity verification.",
                [nameof(Checksum)]);
        }
    }

    /// <summary>
    /// Validates the <see cref="Description"/> property for professional document management standards.
    /// </summary>
    /// <returns>A collection of validation results for the description field.</returns>
    /// <remarks>
    /// Validates the optional description field to ensure it meets professional standards when provided,
    /// including length constraints and content appropriateness.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateDescription()
    {
        if (Description is { Length: > 256 })
        {
            yield return new ValidationResult(
                "Description cannot exceed 256 characters for reasonable professional documentation.",
                [nameof(Description)]);
        }

        // Additional validation for description content when provided
        if (string.IsNullOrWhiteSpace(Description)) yield break;
        // Check for basic professional content standards
        var trimmedDescription = Description.Trim();
        if (trimmedDescription != Description)
        {
            yield return new ValidationResult(
                "Description should not have leading or trailing whitespace for professional presentation.",
                [nameof(Description)]);
        }

        // Ensure description has meaningful content if provided
        if (trimmedDescription.Length < 3)
        {
            yield return new ValidationResult(
                "Description should contain meaningful content when provided (minimum 3 characters).",
                [nameof(Description)]);
        }

        // Check for professional content standards
        if (trimmedDescription.All(char.IsWhiteSpace))
        {
            yield return new ValidationResult(
                "Description cannot consist entirely of whitespace characters.",
                [nameof(Description)]);
        }
    }

    /// <summary>
    /// Validates business rules specific to comprehensive document management.
    /// </summary>
    /// <returns>A collection of validation results for business rule compliance.</returns>
    /// <remarks>
    /// Validates document-specific business rules such as mutual exclusivity constraints,
    /// comprehensive metadata consistency, and professional document management standards.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Business rule: Document cannot be both checked out and deleted
        if (IsCheckedOut && IsDeleted)
        {
            yield return new ValidationResult(
                "A document cannot be both checked out and deleted simultaneously. " +
                "This violates professional document management business rules.",
                [nameof(IsCheckedOut), nameof(IsDeleted)]);
        }

        // Business rule: Validate MIME type and extension consistency
        if (!string.IsNullOrWhiteSpace(Extension) && !string.IsNullOrWhiteSpace(MimeType))
        {
            var expectedMimeTypes = GetExpectedMimeTypesForExtension(Extension.ToLowerInvariant());
            if (expectedMimeTypes.Any() && !expectedMimeTypes.Contains(MimeType, StringComparer.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    $"MIME type '{MimeType}' does not match the expected types for extension '{Extension}'. " +
                    $"Expected: {string.Join(", ", expectedMimeTypes)}",
                    [nameof(MimeType), nameof(Extension)]);
            }
        }

        // Business rule: Enhanced description validation for comprehensive documents
        if (!HasDescription || string.IsNullOrWhiteSpace(Description)) yield break;
        // Check for appropriate professional language in descriptions
        var invalidPatterns = new[] { "TODO", "FIXME", "XXX", "HACK", "TEMP" };
        foreach (var pattern in invalidPatterns)
        {
            if (Description.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    $"Description contains placeholder text '{pattern}' which is not appropriate for professional documents.",
                    [nameof(Description)]);
            }
        }
    }

    /// <summary>
    /// Validates the <see cref="Revisions"/> collection using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the revisions collection.</returns>
    /// <remarks>
    /// Uses comprehensive validation for the revision collection, including business rule validation
    /// for comprehensive documents that should have meaningful revision history.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateRevisions()
    {
        return ValidateCollection(Revisions, nameof(Revisions), allowEmptyCollection: true);
    }

    /// <summary>
    /// Validates the <see cref="DocumentActivityUsers"/> collection using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the document activity users collection.</returns>
    /// <remarks>
    /// Uses comprehensive validation of the document activity users collection for complete audit trails.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateDocumentActivityUsers()
    {
        return ValidateCollection(DocumentActivityUsers, nameof(DocumentActivityUsers));
    }

    /// <summary>
    /// Validates the <see cref="MatterDocumentActivityUsersFrom"/> collection using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the from transfer activities collection.</returns>
    /// <remarks>
    /// Uses comprehensive validation of the source-side transfer activities collection.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateMatterDocumentActivityUsersFrom()
    {
        return ValidateCollection(MatterDocumentActivityUsersFrom, nameof(MatterDocumentActivityUsersFrom));
    }

    /// <summary>
    /// Validates the <see cref="MatterDocumentActivityUsersTo"/> collection using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the to transfer activities collection.</returns>
    /// <remarks>
    /// Uses comprehensive validation of the destination-side transfer activities collection.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateMatterDocumentActivityUsersTo()
    {
        return ValidateCollection(MatterDocumentActivityUsersTo, nameof(MatterDocumentActivityUsersTo));
    }

    /// <summary>
    /// Gets expected MIME types for a given file extension.
    /// </summary>
    /// <param name="extension">The file extension (without dot, lowercase).</param>
    /// <returns>A collection of expected MIME types for the extension.</returns>
    /// <remarks>
    /// Provides MIME type validation support for common legal document formats.
    /// </remarks>
    private static IEnumerable<string> GetExpectedMimeTypesForExtension(string extension)
    {
        return extension switch
        {
            "pdf" => ["application/pdf"],
            "doc" => ["application/msword"],
            "docx" => ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
            "xls" => ["application/vnd.ms-excel"],
            "xlsx" => ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
            "txt" => ["text/plain"],
            "rtf" => ["application/rtf", "text/rtf"],
            "jpg" or "jpeg" => ["image/jpeg"],
            "png" => ["image/png"],
            "gif" => ["image/gif"],
            "tiff" or "tif" => ["image/tiff"],
            _ => []
        };
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="DocumentFullDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The DocumentFullDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate DocumentFullDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>API Validation:</strong> Validating incoming document DTOs in API controllers</item>
    /// <item><strong>Service Layer:</strong> Validation before processing comprehensive document operations</item>
    /// <item><strong>Unit Testing:</strong> Simplified validation testing without ValidationContext</item>
    /// <item><strong>Batch Processing:</strong> Validating collections of comprehensive document DTOs</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new DocumentFullDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     FileName = "Corporate_Merger_Agreement.pdf",
    ///     Extension = "pdf",
    ///     MimeType = "application/pdf",
    ///     FileSize = 4123567,
    ///     Checksum = "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4",
    ///     Description = "Complete merger agreement with all exhibits and amendments"
    /// };
    /// 
    /// var results = DocumentFullDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Document validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] DocumentFullDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("DocumentFullDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto);
        results.AddRange(dto.Validate(context));

        return results;
    }

    /// <summary>
    /// Creates a DocumentFullDto from ADMS.API.Entities.Document entity with validation.
    /// </summary>
    /// <param name="entity">The Document entity to convert. Cannot be null.</param>
    /// <param name="includeRevisions">Whether to include revision collections in the conversion.</param>
    /// <param name="includeActivityUsers">Whether to include activity user collections in the conversion.</param>
    /// <returns>A valid DocumentFullDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create DocumentFullDto instances from
    /// ADMS.API.Entities.Document entities with automatic validation and comprehensive error handling.
    /// 
    /// <para><strong>Entity Mapping:</strong></para>
    /// Maps all properties including the extended Description field and conditionally includes 
    /// related collections based on parameters for comprehensive document representation.
    /// 
    /// <para><strong>Validation Integration:</strong></para>
    /// Automatically validates the created DTO to ensure data integrity and business rule compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create comprehensive document from entity
    /// var entity = new ADMS.API.Entities.Document 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     FileName = "Corporate_Agreement.pdf",
    ///     Extension = "pdf",
    ///     FileSize = 4123567,
    ///     MimeType = "application/pdf",
    ///     Checksum = "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4",
    ///     Description = "Comprehensive corporate agreement with all amendments"
    /// };
    /// 
    /// var dto = DocumentFullDto.FromEntity(entity, 
    ///                                     includeRevisions: true, 
    ///                                     includeActivityUsers: true);
    /// </code>
    /// </example>
    public static DocumentFullDto FromEntity([NotNull] Entities.Document entity,
                                           bool includeRevisions = true,
                                           bool includeActivityUsers = false)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new DocumentFullDto
        {
            Id = entity.Id,
            FileName = entity.FileName,
            Extension = entity.Extension,
            FileSize = entity.FileSize,
            MimeType = entity.MimeType,
            Checksum = entity.Checksum,
            IsCheckedOut = entity.IsCheckedOut,
            IsDeleted = entity.IsDeleted,
            Description = null, // Extended field specific to DocumentFullDto
            Revisions = includeRevisions
                ? entity.Revisions.Select(r => RevisionDto.FromEntity(r)).ToList()
                : [],
            DocumentActivityUsers = includeActivityUsers
                ? entity.DocumentActivityUsers.Select(dau => DocumentActivityUserDto.FromEntity(dau)).ToList()
                : [],
            MatterDocumentActivityUsersFrom = includeActivityUsers
                ? entity.MatterDocumentActivityUsersFrom.Select(mdauf => MatterDocumentActivityUserFromDto.FromEntity(mdauf)).ToList()
                : [],
            MatterDocumentActivityUsersTo = includeActivityUsers
                ? entity.MatterDocumentActivityUsersTo.Select(mdaut => MatterDocumentActivityUserToDto.FromEntity(mdaut)).ToList()
                : []
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Created DocumentFullDto failed validation: {errorMessages}");

    }

    /// <summary>
    /// Creates multiple DocumentFullDto instances from a collection of entities.
    /// </summary>
    /// <param name="entities">The collection of Document entities to convert. Cannot be null.</param>
    /// <param name="includeRevisions">Whether to include revision collections in the conversion.</param>
    /// <param name="includeActivityUsers">Whether to include activity user collections in the conversion.</param>
    /// <returns>A collection of valid DocumentFullDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method is optimized for creating multiple comprehensive document DTOs efficiently,
    /// with comprehensive error handling and validation for each converted entity.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convert collection of document entities to comprehensive DTOs
    /// var entities = await context.Documents
    ///     .Include(d => d.Revisions)
    ///     .Include(d => d.DocumentActivityUsers)
    ///     .ToListAsync();
    /// 
    /// var documentDtos = DocumentFullDto.FromEntities(entities, 
    ///                                                includeRevisions: true, 
    ///                                                includeActivityUsers: true);
    /// 
    /// // Process comprehensive documents
    /// foreach (var documentDto in documentDtos)
    /// {
    ///     ProcessComprehensiveDocument(documentDto);
    /// }
    /// </code>
    /// </example>
    public static IList<DocumentFullDto> FromEntities([NotNull] IEnumerable<Entities.Document> entities,
                                                    bool includeRevisions = true,
                                                    bool includeActivityUsers = false)
    {
        ArgumentNullException.ThrowIfNull(entities);

        return entities.Select(entity => FromEntity(entity, includeRevisions, includeActivityUsers)).ToList();
    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this comprehensive document can be edited based on its current state.
    /// </summary>
    /// <returns>true if the document can be edited; otherwise, false.</returns>
    /// <remarks>
    /// This method encapsulates the business logic for determining document editability for comprehensive documents,
    /// considering check-out status, deletion state, and comprehensive document-specific business rules.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.CanBeEdited())
    /// {
    ///     // Allow comprehensive editing operations
    /// }
    /// </code>
    /// </example>
    public bool CanBeEdited() => IsAvailableForEdit;

    /// <summary>
    /// Gets comprehensive usage statistics for this document.
    /// </summary>
    /// <returns>A dictionary containing comprehensive document statistics and metrics.</returns>
    /// <remarks>
    /// This method provides comprehensive insights into document usage patterns, revision history,
    /// and activity statistics for reporting and analysis purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = document.GetComprehensiveStatistics();
    /// Console.WriteLine($"Document revisions: {stats["RevisionCount"]}");
    /// Console.WriteLine($"Total activities: {stats["TotalActivityCount"]}");
    /// Console.WriteLine($"Development timespan: {stats["DocumentDevelopmentTimespan"]}");
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetComprehensiveStatistics()
    {
        return new Dictionary<string, object>
        {
            ["FileName"] = FileName,
            ["Extension"] = Extension,
            ["FullFileName"] = FullFileName,
            ["FileSize"] = FileSize,
            ["FormattedFileSize"] = FormattedFileSize,
            ["MimeType"] = MimeType,
            ["Status"] = Status,
            ["Description"] = Description ?? string.Empty,
            ["HasDescription"] = HasDescription,
            ["RevisionCount"] = RevisionCount,
            ["TotalActivityCount"] = TotalActivityCount,
            ["HasActivities"] = HasActivities,
            ["IsAvailableForEdit"] = IsAvailableForEdit,
            ["HasValidChecksum"] = HasValidChecksum,
            ["IsCheckedOut"] = IsCheckedOut,
            ["IsDeleted"] = IsDeleted,
            ["DocumentDevelopmentTimespan"] = DocumentDevelopmentTimespan,
            ["CurrentRevision"] = CurrentRevision?.RevisionNumber,
            ["DisplayText"] = DisplayText
        };
    }

    /// <summary>
    /// Analyzes the comprehensive document for completeness and quality metrics.
    /// </summary>
    /// <returns>A comprehensive analysis report of the document.</returns>
    /// <remarks>
    /// This method provides a comprehensive analysis of the document including revision patterns,
    /// activity distribution, and completeness metrics for quality assurance and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// var analysis = document.AnalyzeDocument();
    /// Console.WriteLine($"Completeness Score: {analysis["CompletenessScore"]}");
    /// Console.WriteLine($"Activity Distribution: {analysis["ActivityDistribution"]}");
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> AnalyzeDocument()
    {
        var completenessScore = CalculateCompletenessScore();
        var activityDistribution = AnalyzeActivityDistribution();
        var revisionMetrics = AnalyzeRevisionPatterns();

        return new Dictionary<string, object>
        {
            ["CompletenessScore"] = completenessScore,
            ["ActivityDistribution"] = activityDistribution,
            ["RevisionMetrics"] = revisionMetrics,
            ["QualityIndicators"] = GetQualityIndicators(),
            ["RecommendedActions"] = GetRecommendedActions(),
            ["ComplianceStatus"] = GetComplianceStatus()
        };
    }

    /// <summary>
    /// Calculates a completeness score for the document based on available metadata and content.
    /// </summary>
    /// <returns>A completeness score between 0.0 and 1.0.</returns>
    private double CalculateCompletenessScore()
    {
        var score = 0.0;
        var maxScore = 8.0;

        // Core properties (4 points)
        if (Id != Guid.Empty) score += 1.0;
        if (!string.IsNullOrWhiteSpace(FileName)) score += 1.0;
        if (!string.IsNullOrWhiteSpace(Extension)) score += 1.0;
        if (FileSize > 0) score += 1.0;

        // Extended properties (2 points)
        if (!string.IsNullOrWhiteSpace(MimeType)) score += 1.0;
        if (HasValidChecksum) score += 1.0;

        // Optional enhanced properties (2 points)
        if (HasDescription) score += 1.0;
        if (RevisionCount > 0) score += 1.0;

        return score / maxScore;
    }

    /// <summary>
    /// Analyzes the distribution of activities across the document lifecycle.
    /// </summary>
    /// <returns>Activity distribution analysis.</returns>
    private object AnalyzeActivityDistribution()
    {
        var documentActivities = DocumentActivityUsers.Count;
        var transferFromActivities = MatterDocumentActivityUsersFrom.Count;
        var transferToActivities = MatterDocumentActivityUsersTo.Count;

        return new
        {
            DocumentActivities = documentActivities,
            TransferFromActivities = transferFromActivities,
            TransferToActivities = transferToActivities,
            TotalTransfers = transferFromActivities + transferToActivities,
            ActivityRatio = TotalActivityCount > 0 ? (double)documentActivities / TotalActivityCount : 0.0
        };
    }

    /// <summary>
    /// Analyzes revision patterns and development metrics.
    /// </summary>
    /// <returns>Revision pattern analysis.</returns>
    private object AnalyzeRevisionPatterns()
    {
        return new
        {
            TotalRevisions = RevisionCount,
            HasRevisions = RevisionCount > 0,
            CurrentVersion = CurrentRevision?.RevisionNumber,
            DevelopmentTimespan = DocumentDevelopmentTimespan,
            HasVersionHistory = RevisionCount > 1,
            RevisionVelocity = CalculateRevisionVelocity()
        };
    }

    /// <summary>
    /// Calculates revision velocity (revisions per day).
    /// </summary>
    /// <returns>Revisions per day or null if not applicable.</returns>
    private double? CalculateRevisionVelocity()
    {
        if (DocumentDevelopmentTimespan is null || DocumentDevelopmentTimespan.Value.TotalDays <= 0)
            return null;

        return RevisionCount / DocumentDevelopmentTimespan.Value.TotalDays;
    }

    /// <summary>
    /// Gets quality indicators for the document.
    /// </summary>
    /// <returns>Quality indicator analysis.</returns>
    private object GetQualityIndicators()
    {
        return new
        {
            HasIntegrityVerification = HasValidChecksum,
            HasMeaningfulDescription = HasDescription && Description!.Length >= 10,
            HasVersionControl = RevisionCount > 0,
            HasAuditTrail = HasActivities,
            IsWellFormed = !IsDeleted && !IsCheckedOut,
            CompletenessScore = CalculateCompletenessScore()
        };
    }

    /// <summary>
    /// Gets recommended actions for document improvement.
    /// </summary>
    /// <returns>List of recommended actions.</returns>
    private IList<string> GetRecommendedActions()
    {
        var recommendations = new List<string>();

        if (!HasDescription)
            recommendations.Add("Consider adding a meaningful description for better document organization");

        if (RevisionCount == 0)
            recommendations.Add("Document lacks version history - consider implementing revision tracking");

        if (!HasValidChecksum)
            recommendations.Add("Verify document checksum for integrity validation");

        if (!HasActivities)
            recommendations.Add("No activity history found - document may need audit trail setup");

        if (IsCheckedOut)
            recommendations.Add("Document is currently checked out - consider checking in when work is complete");

        return recommendations;
    }

    /// <summary>
    /// Gets compliance status indicators.
    /// </summary>
    /// <returns>Compliance status analysis.</returns>
    private object GetComplianceStatus()
    {
        return new
        {
            HasRequiredMetadata = !string.IsNullOrWhiteSpace(FileName) && !string.IsNullOrWhiteSpace(Extension),
            HasIntegrityVerification = HasValidChecksum,
            HasAuditTrail = HasActivities,
            HasVersionControl = RevisionCount > 0,
            IsCompliant = HasValidChecksum && HasActivities && !string.IsNullOrWhiteSpace(FileName),
            ComplianceScore = CalculateComplianceScore()
        };
    }

    /// <summary>
    /// Calculates a compliance score based on professional document management standards.
    /// </summary>
    /// <returns>Compliance score between 0.0 and 1.0.</returns>
    private double CalculateComplianceScore()
    {
        var score = 0.0;
        var maxScore = 5.0;

        if (!string.IsNullOrWhiteSpace(FileName)) score += 1.0;
        if (HasValidChecksum) score += 1.0;
        if (HasActivities) score += 1.0;
        if (RevisionCount > 0) score += 1.0;
        if (!IsDeleted) score += 1.0;

        return score / maxScore;
    }

    #endregion Business Logic Methods

    #region Enhanced ValidateCollection Method

    /// <summary>
    /// Validates a collection of <see cref="IValidatableObject"/> items with enhanced error handling.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="propertyName">The property name for error reporting.</param>
    /// <param name="allowEmptyCollection">Whether to allow empty collections.</param>
    /// <returns>A collection of validation results.</returns>
    /// <remarks>
    /// This enhanced validation method provides better error handling and follows the patterns
    /// established in DtoValidationHelper while maintaining backward compatibility.
    /// </remarks>
    private static IEnumerable<ValidationResult> ValidateCollection<T>(ICollection<T> collection, string propertyName, bool allowEmptyCollection = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (!allowEmptyCollection && collection.Count == 0)
        {
            yield return new ValidationResult($"{propertyName} must contain at least one item for comprehensive document management.", [propertyName]);
            yield break;
        }

        var index = 0;
        foreach (var item in collection)
        {
            switch (item)
            {
                case null:
                    yield return new ValidationResult($"{propertyName}[{index}] is null.", [$"{propertyName}[{index}]"]);
                    break;
                case IValidatableObject validatable:
                    {
                        var context = new ValidationContext(item);
                        foreach (var result in validatable.Validate(context))
                        {
                            var memberNames = result.MemberNames.Any()
                                ? result.MemberNames.Select(memberName => $"{propertyName}[{index}].{memberName}")
                                : [$"{propertyName}[{index}]"];

                            yield return new ValidationResult($"{propertyName}[{index}]: {result.ErrorMessage}", memberNames);
                        }
                        break;
                    }
            }

            index++;
        }
    }

    #endregion Enhanced ValidateCollection Method

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified DocumentFullDto is equal to the current DocumentFullDto.
    /// </summary>
    /// <param name="other">The DocumentFullDto to compare with the current DocumentFullDto.</param>
    /// <returns>true if the specified DocumentFullDto is equal to the current DocumentFullDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each document has a unique identifier.
    /// This follows the same equality pattern as ADMS.API.Entities.Document for consistency.
    /// </remarks>
    public bool Equals(DocumentFullDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current DocumentFullDto.
    /// </summary>
    /// <param name="obj">The object to compare with the current DocumentFullDto.</param>
    /// <returns>true if the specified object is equal to the current DocumentFullDto; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as DocumentFullDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current DocumentFullDto.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the DocumentFullDto.
    /// </summary>
    /// <returns>A string that represents the current DocumentFullDto.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the comprehensive document,
    /// which is useful for debugging, logging, and display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new DocumentFullDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     FileName = "Corporate_Agreement.pdf",
    ///     FileSize = 4123567,
    ///     Description = "Comprehensive corporate merger agreement"
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "Document: Corporate_Agreement.pdf (4.0 MB) - Comprehensive corporate merger agreement - 0 revisions, 0 activities"
    /// </code>
    /// </example>
    public override string ToString()
    {
        var baseInfo = $"Document: {FileName} ({FormattedFileSize})";
        var descriptionInfo = HasDescription ? $" - {Description}" : string.Empty;
        var activityInfo = $" - {RevisionCount} revisions, {TotalActivityCount} activities";
        return baseInfo + descriptionInfo + activityInfo;
    }

    #endregion String Representation

    #region Compiled Regular Expressions

    /// <summary>
    /// Provides a compiled regex for validating MIME types at compile time.
    /// </summary>
    /// <remarks>
    /// Uses .NET source generators for optimal performance in validation scenarios.
    /// The pattern validates standard MIME type format (type/subtype).
    /// </remarks>
    [GeneratedRegex(@"^[\w\.\-]+\/[\w\.\-\+]+$", RegexOptions.Compiled)]
    private static partial Regex MimeTypeRegex();

    /// <summary>
    /// Provides a compiled regex for validating SHA256 checksums at compile time.
    /// </summary>
    /// <remarks>
    /// Uses .NET source generators for optimal performance in validation scenarios.
    /// The pattern validates 64-character hexadecimal strings as required for SHA256 hashes.
    /// </remarks>
    [GeneratedRegex(@"^[A-Fa-f0-9]{64}$", RegexOptions.Compiled)]
    private static partial Regex ChecksumRegex();

    #endregion Compiled Regular Expressions
}