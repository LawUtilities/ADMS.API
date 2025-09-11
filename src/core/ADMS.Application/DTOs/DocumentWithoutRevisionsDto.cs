using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using ADMS.Application.Common.Validation;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing a document without its revision history for optimized document management operations.
/// </summary>
/// <remarks>
/// This DTO serves as a complete representation of a document within the ADMS legal document management system,
/// corresponding to <see cref="ADMS.Domain.Entities.Document"/>. It provides comprehensive document data including
/// all audit trail associations while excluding revision collections for performance optimization in scenarios
/// where version history is not required.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Revision-Free Representation:</strong> Excludes revision collections for optimal performance in document-focused operations</item>
/// <item><strong>Complete Activity Integration:</strong> Includes all document and transfer activity collections</item>
/// <item><strong>Professional Validation:</strong> Uses centralized FileValidationHelper for comprehensive data integrity</item>
/// <item><strong>Entity Synchronization:</strong> Mirrors all properties and relationships from ADMS.Domain.Entities.Document (except revisions)</item>
/// <item><strong>Legal Compliance Support:</strong> Designed for comprehensive audit reporting and legal compliance</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO represents the complete structure from ADMS.Domain.Entities.Document, excluding Revisions:
/// <list type="bullet">
/// <item><strong>Document Metadata:</strong> Complete file information, checksums, and status flags</item>
/// <item><strong>Document Activities:</strong> DocumentActivityUser collection for document-level audit trails</item>
/// <item><strong>Matter Transfer Activities:</strong> Bidirectional document transfer audit collections</item>
/// </list>
/// 
/// <para><strong>Supported Document Operations:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Activities:</strong> CREATED, SAVED, DELETED, RESTORED, CHECKED_IN, CHECKED_OUT operations</item>
/// <item><strong>Transfer Operations:</strong> MOVED/COPIED operations between matters with complete audit trails</item>
/// <item><strong>File Integrity:</strong> Checksum validation and file consistency verification</item>
/// <item><strong>Status Management:</strong> Check-out status and deletion state management</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Integrity:</strong> Complete file integrity verification with checksums and validation</item>
/// <item><strong>Activity Attribution:</strong> Complete user attribution for all document operations</item>
/// <item><strong>Audit Compliance:</strong> Comprehensive audit trail relationships for legal compliance</item>
/// <item><strong>Professional Accountability:</strong> Complete user attribution for all document operations</item>
/// <item><strong>Legal Discovery Support:</strong> Complete document metadata and provenance for legal proceedings</item>
/// </list>
/// 
/// <para><strong>File Management Features:</strong></para>
/// <list type="bullet">
/// <item><strong>File Validation:</strong> Comprehensive file name, extension, and MIME type validation</item>
/// <item><strong>Checksum Integrity:</strong> SHA256 checksum validation for file integrity verification</item>
/// <item><strong>Size Validation:</strong> File size validation with professional limits and constraints</item>
/// <item><strong>Status Management:</strong> Check-out status and deletion state management</item>
/// </list>
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// <list type="bullet">
/// <item><strong>Revision Exclusion:</strong> Excludes potentially large revision collections for optimal performance</item>
/// <item><strong>Selective Loading:</strong> Activity collections can be populated on-demand</item>
/// <item><strong>Validation Optimization:</strong> Efficient validation using centralized helpers</item>
/// <item><strong>Memory Efficiency:</strong> Reduced memory footprint compared to DocumentWithRevisionsDto</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Listing:</strong> Document lists and search results where revision history is not needed</item>
/// <item><strong>API Responses:</strong> Document data without revision overhead for performance-critical scenarios</item>
/// <item><strong>Transfer Operations:</strong> Document transfer between matters with complete audit trails</item>
/// <item><strong>Activity Analysis:</strong> Document activity tracking and audit trail analysis</item>
/// <item><strong>UI Operations:</strong> Document selection, metadata display, and basic operations</item>
/// </list>
/// 
/// <para><strong>When to Use vs DocumentWithRevisionsDto:</strong></para>
/// <list type="bullet">
/// <item><strong>Use DocumentWithoutRevisionsDto:</strong> For document metadata, activities, transfers, and performance-critical operations</item>
/// <item><strong>Use DocumentWithRevisionsDto:</strong> For version control operations, complete document history, and revision management</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive document DTO without revisions
/// var documentDto = new DocumentWithoutRevisionsDto
/// {
///     Id = Guid.NewGuid(),
///     FileName = "Contract_Amendment.pdf",
///     Extension = "pdf",
///     FileSize = 2547832,
///     MimeType = "application/pdf",
///     Checksum = "a3b5c7d9e1f2a4b6c8d0e2f4a6b8c0d2e4f6a8b0c2d4e6f8a0b2c4d6e8f0a2b4c6",
///     IsCheckedOut = false,
///     IsDeleted = false
/// };
/// 
/// // Comprehensive validation
/// var validationResults = DocumentWithoutRevisionsDto.ValidateModel(documentDto);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Document Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Professional document analysis
/// Console.WriteLine($"Document '{documentDto.FileName}' - Status: {documentDto.Status}");
/// Console.WriteLine($"File integrity: {(documentDto.HasValidChecksum ? "Valid" : "Invalid")}");
/// </code>
/// </example>
public sealed partial class DocumentWithoutRevisionsDto : IValidatableObject, IEquatable<DocumentWithoutRevisionsDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the document.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the document within the ADMS system.
    /// It corresponds directly to <see cref="ADMS.Domain.Entities.Document.Id"/> and is used for 
    /// establishing relationships, activity associations, and all system operations requiring 
    /// precise document identification.
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required for Updates:</strong> Must be provided when updating existing documents</item>
    /// <item><strong>Foreign Key Reference:</strong> Referenced in DocumentActivityUser and transfer entities</item>
    /// <item><strong>API Operations:</strong> Document identification in REST API operations</item>
    /// <item><strong>Activity Tracking:</strong> Links all activities and transfers to the document</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using document ID for operations
    /// var document = new DocumentWithoutRevisionsDto 
    /// { 
    ///     Id = Guid.Parse("87654321-4321-8765-2109-876543210987"),
    ///     FileName = "Legal_Contract.pdf"
    /// };
    /// 
    /// // Document identification in API operations
    /// var documentUrl = $"/api/documents/{document.Id}";
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
    /// // Professional file naming examples
    /// var contract = new DocumentWithoutRevisionsDto { FileName = "Smith_Purchase_Agreement_v2.pdf" };
    /// var motion = new DocumentWithoutRevisionsDto { FileName = "Motion_Summary_Judgment_2024.docx" };
    /// var exhibit = new DocumentWithoutRevisionsDto { FileName = "Exhibit_A_Financial_Records.xlsx" };
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
    /// // Supported document extensions
    /// var pdfDoc = new DocumentWithoutRevisionsDto { Extension = "pdf" };   // Legal contracts, pleadings
    /// var wordDoc = new DocumentWithoutRevisionsDto { Extension = "docx" }; // Draft documents
    /// var excelDoc = new DocumentWithoutRevisionsDto { Extension = "xlsx" }; // Financial data
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
    /// // File size examples
    /// var smallDoc = new DocumentWithoutRevisionsDto { FileSize = 45678 };     // 44.6 KB
    /// var mediumDoc = new DocumentWithoutRevisionsDto { FileSize = 2547832 };  // 2.4 MB
    /// var largeDoc = new DocumentWithoutRevisionsDto { FileSize = 15728640 };  // 15 MB
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
    /// // Professional MIME type examples
    /// var pdfContract = new DocumentWithoutRevisionsDto { MimeType = "application/pdf" };
    /// var wordDoc = new DocumentWithoutRevisionsDto { 
    ///     MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document" 
    /// };
    /// var excelSheet = new DocumentWithoutRevisionsDto { MimeType = "application/vnd.ms-excel" };
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
    /// // SHA256 checksum examples
    /// var document = new DocumentWithoutRevisionsDto 
    /// { 
    ///     Checksum = "a3b5c7d9e1f2a4b6c8d0e2f4a6b8c0d2e4f6a8b0c2d4e6f8a0b2c4d6e8f0a2b4c6",
    ///     FileName = "Contract.pdf"
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
    /// // Document check-out status
    /// var document = new DocumentWithoutRevisionsDto 
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
    /// // Document deletion status
    /// var document = new DocumentWithoutRevisionsDto 
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
    /// This property mirrors <see cref="ADMS.Domain.Entities.Document.CreationDate"/> exactly, ensuring
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

    #region Collection Properties

    /// <summary>
    /// Gets or sets the collection of document activity users.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.Domain.Entities.Document.DocumentActivityUsers"/> and tracks 
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
    /// // Accessing document audit trail
    /// foreach (var activity in document.DocumentActivityUsers.OrderBy(a => a.CreatedAt))
    /// {
    ///     Console.WriteLine($"User {activity.User?.Name} performed {activity.DocumentActivity?.Activity} " +
    ///                      $"on document at {activity.CreatedAt}");
    /// }
    /// 
    /// // Finding who created the document
    /// var creator = document.DocumentActivityUsers
    ///     .FirstOrDefault(da => da.DocumentActivity?.Activity == "CREATED")?.User;
    /// </code>
    /// </example>
    public ICollection<DocumentActivityUserDto> DocumentActivityUsers { get; set; } = new List<DocumentActivityUserDto>();

    /// <summary>
    /// Gets or sets the collection of "from" matter document activity users.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.Domain.Entities.Document.MatterDocumentActivityUsersFrom"/> and 
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
    /// // Analyzing document transfer sources
    /// foreach (var transferFrom in document.MatterDocumentActivityUsersFrom)
    /// {
    ///     Console.WriteLine($"Document {transferFrom.MatterDocumentActivity?.Activity} " +
    ///                      $"from matter '{transferFrom.Matter?.Description}' " +
    ///                      $"by {transferFrom.User?.Name} at {transferFrom.CreatedAt}");
    /// }
    /// 
    /// // Transfer history analysis
    /// var transferCount = document.MatterDocumentActivityUsersFrom.Count;
    /// </code>
    /// </example>
    public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; set; } = new List<MatterDocumentActivityUserFromDto>();

    /// <summary>
    /// Gets or sets the collection of "to" matter document activity users.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.Domain.Entities.Document.MatterDocumentActivityUsersTo"/> and 
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
    /// // Analyzing document transfer destinations
    /// foreach (var transferTo in document.MatterDocumentActivityUsersTo)
    /// {
    ///     Console.WriteLine($"Document {transferTo.MatterDocumentActivity?.Activity} " +
    ///                      $"to matter '{transferTo.Matter?.Description}' " +
    ///                      $"by {transferTo.User?.Name} at {transferTo.CreatedAt}");
    /// }
    /// 
    /// // Complete transfer analysis
    /// var totalTransfers = document.MatterDocumentActivityUsersFrom.Count + 
    ///                     document.MatterDocumentActivityUsersTo.Count;
    /// </code>
    /// </example>
    public ICollection<MatterDocumentActivityUserToDto> MatterDocumentActivityUsersTo { get; set; } = new List<MatterDocumentActivityUserToDto>();

    #endregion Collection Properties

    #region Computed Properties

    /// <summary>
    /// Gets the total count of all activities (document + transfer) for this document.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.Domain.Entities.Document.TotalActivityCount"/> and provides 
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
    /// This computed property mirrors <see cref="ADMS.Domain.Entities.Document.HasActivities"/> and 
    /// determines if the document has been used in the system, useful for activity analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.HasActivities)
    /// {
    ///     Console.WriteLine("Document has activity history");
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
    ///     // Enable editing controls
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
    /// var sizeText = document.FormattedFileSize; // "2.4 MB"
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
    /// including the file name for clear identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// var displayText = document.DisplayText; // Returns the file name
    /// documentDropdown.Items.Add(new ListItem(document.DisplayText, document.Id.ToString()));
    /// </code>
    /// </example>
    public string DisplayText => FileName;

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="DocumentWithoutRevisionsDto"/> using standardized ADMS validation patterns.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Implements comprehensive validation following the standardized ADMS validation hierarchy:
    /// <list type="number">
    /// <item><strong>Core Properties:</strong> Essential document properties using FileValidationHelper and BaseValidationDto</item>
    /// <item><strong>Business Rules:</strong> Document lifecycle, version control, and professional standards</item>
    /// <item><strong>Cross-Property:</strong> MIME type consistency, temporal validation, and referential integrity</item>
    /// <item><strong>Collections:</strong> Deep validation of activity audit trail collections</item>
    /// </list>
    /// 
    /// <para><strong>Professional Standards Integration:</strong></para>
    /// Uses centralized validation helpers (FileValidationHelper, BaseValidationDto) to ensure
    /// consistency across all document validation in the ADMS system.
    /// </remarks>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        ValidateParameters(validationContext);
        return ValidateIterator();
    }

    /// <summary>
    /// Validates the specified <see cref="ValidationContext"/> to ensure it is not null.
    /// </summary>
    /// <param name="validationContext">The context to validate. Must not be <see langword="null"/>.</param>
    private static void ValidateParameters(ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);
    }

    /// <summary>
    /// Performs validation on the object and yields a sequence of <see cref="ValidationResult"/> objects representing
    /// validation errors, if any.
    /// </summary>
    /// <remarks>This method performs validation in multiple stages: <list type="number"> <item>Core
    /// properties validation.</item> <item>Business rules validation.</item> <item>Cross-property validation.</item>
    /// <item>Collections validation.</item> </list> Each stage contributes to the overall validation results. Callers
    /// can enumerate the returned sequence to process validation errors as they are discovered.</remarks>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ValidationResult"/> objects. Each result represents a validation
    /// error. If the object passes all validation checks, the sequence will be empty.</returns>
    private IEnumerable<ValidationResult> ValidateIterator()
    {
        // 1. Core Properties Validation
        foreach (var result in ValidateCoreProperties())
            yield return result;

        // 2. Business Rules Validation
        foreach (var result in ValidateBusinessRules())
            yield return result;

        // 3. Cross-Property Validation
        foreach (var result in ValidateCrossPropertyRules())
            yield return result;

        // 4. Collections Validation
        foreach (var result in ValidateCollections())
            yield return result;
    }

    /// <summary>
    /// Validates core document properties using ADMS validation helpers.
    /// </summary>
    /// <returns>A collection of validation results for core property validation.</returns>
    /// <remarks>
    /// Validates essential document properties following RevisionValidationHelper patterns:
    /// <list type="bullet">
    /// <item>Document ID validation using BaseValidationDto patterns</item>
    /// <item>File system properties using FileValidationHelper</item>
    /// <item>Temporal properties using BaseValidationDto patterns</item>
    /// <item>Professional standards enforcement</item>
    /// </list>
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateCoreProperties()
    {
        // Validate document ID using RevisionValidationHelper patterns
        if (Id == Guid.Empty)
        {
            yield return new ValidationResult(
                "Document ID must be a valid non-empty GUID for document identification and system operations.",
                [nameof(Id)]);
        }

        // Validate file name using FileValidationHelper
        foreach (var result in ValidateFileName())
            yield return result;

        // Validate extension using FileValidationHelper
        foreach (var result in ValidateExtension())
            yield return result;

        // Validate file size using FileValidationHelper patterns
        foreach (var result in ValidateFileSize())
            yield return result;

        // Validate MIME type using FileValidationHelper
        foreach (var result in ValidateMimeType())
            yield return result;

        // Validate checksum using FileValidationHelper patterns
        foreach (var result in ValidateChecksum())
            yield return result;

        // Validate creation date using BaseValidationDto patterns
        foreach (var result in ValidateCreationDate())
            yield return result;
    }

    /// <summary>
    /// Validates file name using ADMS FileValidationHelper standards.
    /// </summary>
    /// <returns>A collection of validation results for file name validation.</returns>
    private IEnumerable<ValidationResult> ValidateFileName()
    {
        if (string.IsNullOrWhiteSpace(FileName))
        {
            yield return new ValidationResult(
                "File name is required and cannot be empty for document identification.",
                [nameof(FileName)]);
            yield break;
        }

        if (FileName.Length > FileValidationHelper.MaxFileNameLength)
        {
            yield return new ValidationResult(
                $"File name cannot exceed {FileValidationHelper.MaxFileNameLength} characters for file system compatibility.",
                [nameof(FileName)]);
        }

        if (!FileValidationHelper.IsFileNameValid(FileName))
        {
            yield return new ValidationResult(
                "File name contains invalid characters or format. Use alphanumeric characters, spaces, hyphens, and underscores.",
                [nameof(FileName)]);
        }

        if (FileValidationHelper.IsReservedFileName(FileName))
        {
            yield return new ValidationResult(
                "File name cannot use a reserved system name for security and compatibility.",
                [nameof(FileName)]);
        }

        if (!FileName.Any(char.IsLetterOrDigit))
        {
            yield return new ValidationResult(
                "File name must contain at least one alphanumeric character for professional identification.",
                [nameof(FileName)]);
        }

        // Security validation following RevisionValidationHelper patterns
        var securityPatterns = new[] { "<script", "javascript:", "vbscript:", "<object", "<embed", "<iframe" };
        var fileNameLower = FileName.ToLowerInvariant();

        if (securityPatterns.Any(pattern => fileNameLower.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            yield return new ValidationResult(
                "File name contains potentially malicious content patterns and cannot be processed.",
                [nameof(FileName)]);
        }
    }

    /// <summary>
    /// Validates file extension using ADMS FileValidationHelper standards.
    /// </summary>
    /// <returns>A collection of validation results for file extension validation.</returns>
    private IEnumerable<ValidationResult> ValidateExtension()
    {
        if (string.IsNullOrWhiteSpace(Extension))
        {
            yield return new ValidationResult(
                "File extension is required and cannot be empty for document format identification.",
                [nameof(Extension)]);
            yield break;
        }

        if (Extension.Length > FileValidationHelper.MaxExtensionLength)
        {
            yield return new ValidationResult(
                $"File extension cannot exceed {FileValidationHelper.MaxExtensionLength} characters for system compatibility.",
                [nameof(Extension)]);
        }

        if (!FileValidationHelper.IsExtensionAllowed(Extension))
        {
            yield return new ValidationResult(
                $"Extension '{Extension}' is not allowed for legal document management. " +
                $"Allowed extensions: {string.Join(", ", FileValidationHelper.AllowedExtensions)}",
                [nameof(Extension)]);
        }

        if (Extension.Any(char.IsWhiteSpace))
        {
            yield return new ValidationResult(
                "File extension cannot contain whitespace characters for system compatibility.",
                [nameof(Extension)]);
        }

        // Case consistency following professional standards
        if (!Extension.Equals(Extension.ToLowerInvariant(), StringComparison.Ordinal))
        {
            yield return new ValidationResult(
                "File extension must be lowercase for consistency with professional document management standards.",
                [nameof(Extension)]);
        }
    }

    /// <summary>
    /// Validates file size using ADMS professional document management standards.
    /// </summary>
    /// <returns>A collection of validation results for file size validation.</returns>
    private IEnumerable<ValidationResult> ValidateFileSize()
    {
        if (FileSize < 0)
        {
            yield return new ValidationResult(
                "File size must be non-negative for valid document metadata and storage tracking.",
                [nameof(FileSize)]);
        }

        if (FileSize == 0)
        {
            yield return new ValidationResult(
                "File size must be greater than zero for actual document files with content.",
                [nameof(FileSize)]);
        }

        // Professional document size limits following RevisionValidationHelper patterns
        if (FileSize > 100 * 1024 * 1024) // 100 MB limit
        {
            yield return new ValidationResult(
                "File size exceeds recommended limit for efficient document management (100 MB). " +
                "Large files may require special handling or compression.",
                [nameof(FileSize)]);
        }

        // Professional practice warnings
        if (FileSize > 50 * 1024 * 1024) // 50 MB warning
        {
            yield return new ValidationResult(
                "File size is very large (>50 MB). Consider optimizing for professional document management efficiency.",
                [nameof(FileSize)]);
        }
    }

    /// <summary>
    /// Validates MIME type using ADMS FileValidationHelper standards.
    /// </summary>
    /// <returns>A collection of validation results for MIME type validation.</returns>
    private IEnumerable<ValidationResult> ValidateMimeType()
    {
        if (string.IsNullOrWhiteSpace(MimeType))
        {
            yield return new ValidationResult(
                "MIME type is required for proper document handling, security validation, and client application integration.",
                [nameof(MimeType)]);
            yield break;
        }

        if (!FileValidationHelper.IsMimeTypeAllowed(MimeType))
        {
            yield return new ValidationResult(
                $"MIME type '{MimeType}' is not allowed for legal document management. " +
                $"Contact system administrator for approved MIME types.",
                [nameof(MimeType)]);
        }

        if (!MimeTypeRegex().IsMatch(MimeType))
        {
            yield return new ValidationResult(
                "Invalid MIME type format. Expected standard format: type/subtype (e.g., application/pdf).",
                [nameof(MimeType)]);
        }
    }

    /// <summary>
    /// Returns the expected MIME types for a given file extension.
    /// </summary>
    /// <param name="extension">The file extension (lowercase, without dot).</param>
    /// <returns>A list of expected MIME types for the extension.</returns>
    private static string[] GetExpectedMimeTypesForExtension(string extension)
    {
        // This mapping should be extended as needed for your application's supported types.
        return extension switch
        {
            "pdf" => ["application/pdf"],
            "doc" => ["application/msword"],
            "docx" => ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
            "xls" => ["application/vnd.ms-excel"],
            "xlsx" => ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
            "txt" => ["text/plain"],
            _ => []
        };
    }

    /// <summary>
    /// Validates checksum using ADMS FileValidationHelper standards.
    /// </summary>
    /// <returns>A collection of validation results for checksum validation.</returns>
    private IEnumerable<ValidationResult> ValidateChecksum()
    {
        if (string.IsNullOrWhiteSpace(Checksum))
        {
            yield return new ValidationResult(
                "Checksum is required for document integrity verification, security compliance, and audit trail requirements.",
                [nameof(Checksum)]);
            yield break;
        }

        if (!ChecksumRegex().IsMatch(Checksum) || Checksum.Length != 64)
        {
            yield return new ValidationResult(
                "Checksum must be a valid 64-character hexadecimal string (SHA256 hash) for proper integrity verification and security compliance.",
                [nameof(Checksum)]);
        }

        // Additional checksum validation following FileValidationHelper patterns
        if (!FileValidationHelper.IsValidChecksum(Checksum))
        {
            yield return new ValidationResult(
                "Checksum format is invalid or does not meet security requirements for document integrity verification.",
                [nameof(Checksum)]);
        }
    }

    /// <summary>
    /// Validates creation date using BaseValidationDto temporal validation patterns.
    /// </summary>
    /// <returns>A collection of validation results for creation date validation.</returns>
    private IEnumerable<ValidationResult> ValidateCreationDate()
    {
        if (CreationDate == default)
        {
            yield return new ValidationResult(
                "Creation date is required and cannot be the default value for temporal tracking and audit compliance.",
                [nameof(CreationDate)]);
            yield break;
        }

        // Use RevisionValidationHelper patterns for date validation
        var minAllowedDate = RevisionValidationHelper.MinAllowedRevisionDate;
        if (CreationDate < minAllowedDate)
        {
            yield return new ValidationResult(
                $"Creation date cannot be earlier than {minAllowedDate:yyyy-MM-dd} for system consistency and reasonable temporal bounds.",
                [nameof(CreationDate)]);
        }

        var maxAllowedDate = DateTime.UtcNow.AddMinutes(RevisionValidationHelper.FutureDateToleranceMinutes);
        if (CreationDate > maxAllowedDate)
        {
            yield return new ValidationResult(
                $"Creation date cannot be in the future (beyond clock skew tolerance of {RevisionValidationHelper.FutureDateToleranceMinutes} minutes).",
                [nameof(CreationDate)]);
        }

        // Professional age validation
        var age = DateTime.UtcNow - CreationDate;
        if (age.TotalDays > RevisionValidationHelper.MaxReasonableAgeYears * 365)
        {
            yield return new ValidationResult(
                $"Creation date age exceeds reasonable bounds for active document management ({RevisionValidationHelper.MaxReasonableAgeYears} years). " +
                "Verify data accuracy and retention policy compliance.",
                [nameof(CreationDate)]);
        }
    }

    /// <summary>
    /// Validates document-specific business rules and professional standards.
    /// </summary>
    /// <returns>A collection of validation results for business rule compliance.</returns>
    private IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Mutual exclusivity validation following RevisionValidationHelper patterns
        if (IsCheckedOut && IsDeleted)
        {
            yield return new ValidationResult(
                "A document cannot be both checked out and deleted simultaneously. " +
                "This violates professional document management business rules and version control integrity.",
                [nameof(IsCheckedOut), nameof(IsDeleted)]);
        }

        // Professional standards validation
        if (!string.IsNullOrWhiteSpace(Extension) && !FileValidationHelper.IsLegalDocumentFormat(Extension))
        {
            yield return new ValidationResult(
                $"Extension '{Extension}' is not a standard legal document format. " +
                "Consider using PDF, DOCX, or other approved legal document formats for professional compliance.",
                [nameof(Extension)]);
        }

        // Document lifecycle validation
        if (IsDeleted && !HasValidAuditTrail())
        {
            yield return new ValidationResult(
                "Deleted documents must have appropriate audit trail activities for legal compliance and accountability.",
                [nameof(IsDeleted)]);
        }

        // Professional file management validation
        if (IsCheckedOut && TotalActivityCount == 0)
        {
            yield return new ValidationResult(
                "Checked out documents should have corresponding activity audit trail for professional accountability.",
                [nameof(IsCheckedOut)]);
        }
    }

    /// <summary>
    /// Validates cross-property relationships and consistency rules.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    private IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // MIME type and extension consistency validation
        if (!string.IsNullOrWhiteSpace(Extension) && !string.IsNullOrWhiteSpace(MimeType))
        {
            var expectedMimeTypes = GetExpectedMimeTypesForExtension(Extension.ToLowerInvariant());
            if (expectedMimeTypes.Length > 0 && !expectedMimeTypes.Contains(MimeType, StringComparer.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    $"MIME type '{MimeType}' does not match the expected types for extension '{Extension}'. " +
                    $"Expected: {string.Join(", ", expectedMimeTypes)}. " +
                    "Ensure file format consistency for proper document handling.",
                    [nameof(MimeType), nameof(Extension)]);
            }
        }

        // File size and format consistency
        if (!string.IsNullOrWhiteSpace(Extension) && FileSize > 0 && Extension.Equals("txt", StringComparison.OrdinalIgnoreCase) && FileSize > 10 * 1024 * 1024)
        {
            // 10 MB
            yield return new ValidationResult(
                "Text files should typically be smaller. Verify this is actually a text document.",
                [nameof(FileSize), nameof(Extension)]);
        }

        // Professional naming consistency
        if (string.IsNullOrWhiteSpace(FileName) || string.IsNullOrWhiteSpace(Extension)) yield break;
        var fullFileName = $"{FileName}.{Extension}";
        if (fullFileName.Length > 255) // Windows MAX_PATH consideration
        {
            yield return new ValidationResult(
                "Complete file name (name + extension) exceeds maximum path length for file system compatibility (255 characters).",
                [nameof(FileName), nameof(Extension)]);
        }
    }

    /// <summary>
    /// Validates collections and audit trail relationships.
    /// </summary>
    /// <returns>A collection of validation results for collection validation.</returns>
    private IEnumerable<ValidationResult> ValidateCollections()
    {
        // Document activity users validation following RevisionValidationHelper patterns
        foreach (var result in ValidateDocumentActivityUsers())
            yield return result;

        // Matter transfer activities validation
        foreach (var result in ValidateMatterTransferActivities())
            yield return result;

        // Audit trail completeness validation
        foreach (var result in ValidateAuditTrailCompleteness())
            yield return result;
    }

    /// <summary>
    /// Validates document activity users collection following ADMS patterns.
    /// </summary>
    /// <returns>A collection of validation results for document activity validation.</returns>
    private IEnumerable<ValidationResult> ValidateDocumentActivityUsers()
    {
        var index = 0;
        foreach (var activity in DocumentActivityUsers)
        {
            if (activity == null)
            {
                yield return new ValidationResult(
                    $"DocumentActivityUsers[{index}] cannot be null for audit trail integrity.",
                    [$"DocumentActivityUsers[{index}]"]);
            }
            else
            {
                // Cross-reference validation
                if (activity.DocumentId != Guid.Empty && activity.DocumentId != Id)
                {
                    yield return new ValidationResult(
                        $"DocumentActivityUsers[{index}] references incorrect document ID. " +
                        "All activities must be associated with this document.",
                        [$"DocumentActivityUsers[{index}].DocumentId"]);
                }

                if (activity.UserId == Guid.Empty)
                {
                    yield return new ValidationResult(
                        $"DocumentActivityUsers[{index}] must reference a valid user ID for accountability.",
                        [$"DocumentActivityUsers[{index}].UserId"]);
                }

                // Activity timestamp validation
                if (activity.CreatedAt < CreationDate.AddMinutes(-RevisionValidationHelper.FutureDateToleranceMinutes))
                {
                    yield return new ValidationResult(
                        $"DocumentActivityUsers[{index}] activity timestamp cannot be significantly before document creation date.",
                        [$"DocumentActivityUsers[{index}].CreatedAt"]);
                }
            }
            index++;
        }

        // Required activities validation following RevisionValidationHelper patterns
        if (IsDeleted || DocumentActivityUsers.Count <= 0) yield break;
        var hasCreationActivity = DocumentActivityUsers.Any(a =>
            string.Equals(a.DocumentActivity?.Activity, "CREATED", StringComparison.OrdinalIgnoreCase));

        if (!hasCreationActivity)
        {
            yield return new ValidationResult(
                "Document must have at least one CREATED activity for complete audit trail and legal compliance.",
                [nameof(DocumentActivityUsers)]);
        }
    }

    /// <summary>
    /// Validates matter transfer activities for bidirectional audit compliance.
    /// </summary>
    /// <returns>A collection of validation results for transfer activity validation.</returns>
    private IEnumerable<ValidationResult> ValidateMatterTransferActivities()
    {
        // Validate FROM transfers
        foreach (var result in ValidateTransferCollection(MatterDocumentActivityUsersFrom, nameof(MatterDocumentActivityUsersFrom)))
            yield return result;

        // Validate TO transfers
        foreach (var result in ValidateTransferCollection(MatterDocumentActivityUsersTo, nameof(MatterDocumentActivityUsersTo)))
            yield return result;

        // Bidirectional consistency validation
        var fromCount = MatterDocumentActivityUsersFrom.Count;
        var toCount = MatterDocumentActivityUsersTo.Count;

        if ((fromCount > 0 || toCount > 0) && Math.Abs(fromCount - toCount) > 10)
        {
            yield return new ValidationResult(
                "Transfer audit trail appears significantly unbalanced between source and destination activities. " +
                "Verify bidirectional tracking is complete for legal compliance.",
                [nameof(MatterDocumentActivityUsersFrom), nameof(MatterDocumentActivityUsersTo)]);
        }
    }

    /// <summary>
    /// Validates a transfer collection following ADMS audit trail standards.
    /// </summary>
    /// <param name="collection">The transfer collection to validate.</param>
    /// <param name="propertyName">The property name for error reporting.</param>
    /// <returns>A collection of validation results.</returns>
    private static IEnumerable<ValidationResult> ValidateTransferCollection<T>(ICollection<T> collection, string propertyName)
        where T : class
    {
        var index = 0;
        foreach (var transfer in collection)
        {
            if (transfer == null)
            {
                yield return new ValidationResult(
                    $"{propertyName}[{index}] cannot be null for transfer audit trail integrity.",
                    [$"{propertyName}[{index}]"]);
            }
            // Additional transfer-specific validation can be added here
            index++;
        }
    }

    /// <summary>
    /// Validates overall audit trail completeness following professional standards.
    /// </summary>
    /// <returns>A collection of validation results for audit trail validation.</returns>
    private IEnumerable<ValidationResult> ValidateAuditTrailCompleteness()
    {
        // Professional audit trail validation
        if (!IsDeleted && TotalActivityCount == 0)
        {
            yield return new ValidationResult(
                "Active documents must have audit trail activities for legal compliance and professional accountability.",
                [nameof(DocumentActivityUsers)]);
        }

        // Activity count reasonableness following RevisionValidationHelper patterns
        if (TotalActivityCount > RevisionValidationHelper.MaxReasonableActivityCount * 2) // Allow higher for documents vs revisions
        {
            yield return new ValidationResult(
                $"Document has an unusually high activity count (>{RevisionValidationHelper.MaxReasonableActivityCount * 2}). " +
                "Verify data integrity and consider system optimization.",
                [nameof(DocumentActivityUsers)]);
        }
    }

    /// <summary>
    /// Helper method to check if document has valid audit trail.
    /// </summary>
    /// <returns>True if document has valid audit trail; otherwise, false.</returns>
    private bool HasValidAuditTrail()
    {
        return TotalActivityCount > 0 && 
               DocumentActivityUsers.Any(a => 
                   !string.IsNullOrWhiteSpace(a.DocumentActivity?.Activity));
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="DocumentWithoutRevisionsDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The DocumentWithoutRevisionsDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate DocumentWithoutRevisionsDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>API Validation:</strong> Validating incoming document DTOs in API controllers</item>
    /// <item><strong>Service Layer:</strong> Validation before processing document operations</item>
    /// <item><strong>Unit Testing:</strong> Simplified validation testing without ValidationContext</item>
    /// <item><strong>Batch Processing:</strong> Validating collections of document DTOs</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new DocumentWithoutRevisionsDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     FileName = "Legal_Contract.pdf",
    ///     Extension = "pdf",
    ///     MimeType = "application/pdf",
    ///     FileSize = 1234567,
    ///     Checksum = "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4"
    /// };
    /// 
    /// var results = DocumentWithoutRevisionsDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Document validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] DocumentWithoutRevisionsDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("DocumentWithoutRevisionsDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto);
        results.AddRange(dto.Validate(context));

        return results;
    }

    /// <summary>
    /// Creates a DocumentWithoutRevisionsDto from ADMS.Domain.Entities.Document entity with validation.
    /// </summary>
    /// <param name="entity">The Document entity to convert. Cannot be null.</param>
    /// <param name="includeActivityUsers">Whether to include activity user collections in the conversion.</param>
    /// <returns>A valid DocumentWithoutRevisionsDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create DocumentWithoutRevisionsDto instances from
    /// ADMS.Domain.Entities.Document entities with automatic validation and comprehensive error handling.
    /// 
    /// <para><strong>Entity Mapping:</strong></para>
    /// Maps all core properties and conditionally includes related collections based on parameters.
    /// Explicitly excludes revision collections for performance optimization.
    /// 
    /// <para><strong>Validation Integration:</strong></para>
    /// Automatically validates the created DTO to ensure data integrity and business rule compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity with activity collections
    /// var entity = new ADMS.Domain.Entities.Document 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     FileName = "Contract.pdf",
    ///     Extension = "pdf",
    ///     FileSize = 1234567,
    ///     MimeType = "application/pdf",
    ///     Checksum = "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4"
    /// };
    /// 
    /// var dto = DocumentWithoutRevisionsDto.FromEntity(entity, includeActivityUsers: true);
    /// </code>
    /// </example>
    public static DocumentWithoutRevisionsDto FromEntity([NotNull] Domain.Entities.Document entity,
                                                       bool includeActivityUsers = false)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new DocumentWithoutRevisionsDto
        {
            Id = entity.Id,
            FileName = entity.FileName,
            Extension = entity.Extension,
            FileSize = entity.FileSize,
            MimeType = entity.MimeType,
            Checksum = entity.Checksum,
            IsCheckedOut = entity.IsCheckedOut,
            IsDeleted = entity.IsDeleted,
            CreationDate = entity.CreatedDate,
            DocumentActivityUsers = includeActivityUsers
                ? entity.DocumentActivityUsers.Select(dau => new DocumentActivityUserDto
                {
                    DocumentId = dau.DocumentId,
                    Document = new DocumentWithoutRevisionsDto
                    {
                        Id = dau.Document.Id,
                        FileName = dau.Document.FileName,
                        Extension = dau.Document.Extension,
                        FileSize = dau.Document.FileSize,
                        MimeType = dau.Document.MimeType,
                        Checksum = dau.Document.Checksum,
                        CreationDate = entity.CreatedDate,
                        IsCheckedOut = dau.Document.IsCheckedOut,
                        IsDeleted = dau.Document.IsDeleted,
                        DocumentActivityUsers = [],
                        MatterDocumentActivityUsersFrom = [],
                        MatterDocumentActivityUsersTo = []
                    },
                    DocumentActivityId = dau.DocumentActivityId,
                    DocumentActivity = new DocumentActivityDto
                    {
                        Id = dau.DocumentActivity.Id,
                        Activity = dau.DocumentActivity.Activity,
                        DocumentActivityUsers = []
                    },
                    UserId = dau.UserId,
                    User = new UserDto
                    {
                        Id = dau.User.Id,
                        Name = dau.User.Name,
                        MatterActivityUsers = [],
                        DocumentActivityUsers = [],
                        RevisionActivityUsers = [],
                        MatterDocumentActivityUsersFrom = [],
                        MatterDocumentActivityUsersTo = []
                    },
                    CreatedAt = dau.CreatedAt
                }).ToList()
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
        throw new ValidationException($"Created DocumentWithoutRevisionsDto failed validation: {errorMessages}");

    }

    /// <summary>
    /// Creates multiple DocumentWithoutRevisionsDto instances from a collection of entities.
    /// </summary>
    /// <param name="entities">The collection of Document entities to convert. Cannot be null.</param>
    /// <param name="includeActivityUsers">Whether to include activity user collections in the conversion.</param>
    /// <returns>A collection of valid DocumentWithoutRevisionsDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method is optimized for creating multiple document DTOs efficiently,
    /// with comprehensive error handling and validation for each converted entity.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convert collection of document entities
    /// var entities = await context.Documents.ToListAsync();
    /// var documentDtos = DocumentWithoutRevisionsDto.FromEntities(entities, includeActivityUsers: false);
    /// 
    /// // Process documents
    /// foreach (var documentDto in documentDtos)
    /// {
    ///     ProcessDocument(documentDto);
    /// }
    /// </code>
    /// </example>
    public static IList<DocumentWithoutRevisionsDto> FromEntities([NotNull] IEnumerable<Domain.Entities.Document> entities,
                                                                bool includeActivityUsers = false)
    {
        ArgumentNullException.ThrowIfNull(entities);

        return entities.Select(entity => FromEntity(entity, includeActivityUsers)).ToList();
    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this document can be edited based on its current state.
    /// </summary>
    /// <returns>true if the document can be edited; otherwise, false.</returns>
    /// <remarks>
    /// This method encapsulates the business logic for determining document editability,
    /// considering check-out status, deletion state, and other business rules.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.CanBeEdited())
    /// {
    ///     // Allow editing operations
    /// }
    /// </code>
    /// </example>
    public bool CanBeEdited() => IsAvailableForEdit;

    /// <summary>
    /// Gets usage statistics for this document.
    /// </summary>
    /// <returns>A dictionary containing document statistics and metrics.</returns>
    /// <remarks>
    /// This method provides insights into document usage patterns and statistics 
    /// for reporting and analysis purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = document.GetUsageStatistics();
    /// Console.WriteLine($"Total activities: {stats["TotalActivityCount"]}");
    /// Console.WriteLine($"Document status: {stats["Status"]}");
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetUsageStatistics()
    {
        return new Dictionary<string, object>
        {
            ["FileName"] = FileName,
            ["Extension"] = Extension,
            ["FileSize"] = FileSize,
            ["FormattedFileSize"] = FormattedFileSize,
            ["MimeType"] = MimeType,
            ["Status"] = Status,
            ["TotalActivityCount"] = TotalActivityCount,
            ["HasActivities"] = HasActivities,
            ["IsAvailableForEdit"] = IsAvailableForEdit,
            ["HasValidChecksum"] = HasValidChecksum,
            ["IsCheckedOut"] = IsCheckedOut,
            ["IsDeleted"] = IsDeleted
        };
    }

    #endregion Business Logic Methods

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified DocumentWithoutRevisionsDto is equal to the current DocumentWithoutRevisionsDto.
    /// </summary>
    /// <param name="other">The DocumentWithoutRevisionsDto to compare with the current DocumentWithoutRevisionsDto.</param>
    /// <returns>true if the specified DocumentWithoutRevisionsDto is equal to the current DocumentWithoutRevisionsDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each document has a unique identifier.
    /// This follows the same equality pattern as ADMS.Domain.Entities.Document for consistency.
    /// </remarks>
    public bool Equals(DocumentWithoutRevisionsDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current DocumentWithoutRevisionsDto.
    /// </summary>
    /// <param name="obj">The object to compare with the current DocumentWithoutRevisionsDto.</param>
    /// <returns>true if the specified object is equal to the current DocumentWithoutRevisionsDto; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as DocumentWithoutRevisionsDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current DocumentWithoutRevisionsDto.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the DocumentWithoutRevisionsDto.
    /// </summary>
    /// <returns>A string that represents the current DocumentWithoutRevisionsDto.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the document,
    /// which is useful for debugging, logging, and display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new DocumentWithoutRevisionsDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     FileName = "Contract.pdf",
    ///     FileSize = 1234567
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "Document: Contract.pdf (1.2 MB) - 0 activities"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Document: {FileName} ({FormattedFileSize}) - {TotalActivityCount} activities";

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