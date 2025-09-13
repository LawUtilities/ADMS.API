using ADMS.Application.Common;
using ADMS.Application.Common.Validation;
using ADMS.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing a document with its complete revision history and associated audit trail relationships.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of a document within the ADMS legal document management system,
/// corresponding to <see cref="ADMS.Domain.Entities.Document"/>. It provides comprehensive document data including
/// complete revision collections and all audit trail associations for legal compliance and professional accountability.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Document Representation:</strong> Full representation of document with all relationships</item>
/// <item><strong>Revision History Inclusion:</strong> Complete revision history for version control and audit trails</item>
/// <item><strong>Professional Validation:</strong> Uses centralized FileValidationHelper for comprehensive data integrity</item>
/// <item><strong>Entity Synchronization:</strong> Mirrors all properties and relationships from ADMS.Domain.Entities.Document</item>
/// <item><strong>Legal Compliance Support:</strong> Designed for comprehensive audit reporting and legal compliance</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO represents the complete structure from ADMS.Domain.Entities.Document:
/// <list type="bullet">
/// <item><strong>Document Metadata:</strong> Complete file information, checksums, and status flags</item>
/// <item><strong>Revision History:</strong> Complete RevisionDto collection for version control</item>
/// <item><strong>Document Activities:</strong> DocumentActivityUser collection for document-level audit trails</item>
/// <item><strong>Matter Transfer Activities:</strong> Bidirectional document transfer audit collections</item>
/// </list>
/// 
/// <para><strong>Supported Document Operations:</strong></para>
/// <list type="bullet">
/// <item><strong>Version Control:</strong> Complete revision history with sequential numbering</item>
/// <item><strong>Document Activities:</strong> CREATED, SAVED, DELETED, RESTORED, CHECKED_IN, CHECKED_OUT operations</item>
/// <item><strong>Transfer Operations:</strong> MOVED/COPIED operations between matters with complete audit trails</item>
/// <item><strong>File Integrity:</strong> Checksum validation and file consistency verification</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Integrity:</strong> Complete file integrity verification with checksums and validation</item>
/// <item><strong>Version Control:</strong> Professional-grade version control with complete revision history</item>
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
/// </list>
/// 
/// <para><strong>Data Integrity and Validation:</strong></para>
/// <list type="bullet">
/// <item><strong>File System Validation:</strong> Comprehensive file metadata validation using FileValidationHelper</item>
/// <item><strong>Collection Validation:</strong> Deep validation of revision and audit trail collections</item>
/// <item><strong>Business Rule Compliance:</strong> Enforces professional document management business rules</item>
/// <item><strong>Entity Completeness:</strong> Ensures all critical document properties are validated</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Management:</strong> Complete document administration and lifecycle operations</item>
/// <item><strong>Version Control:</strong> Document versioning with complete revision history</item>
/// <item><strong>Audit Trail Display:</strong> Complete document activity history and user attribution</item>
/// <item><strong>Legal Compliance:</strong> Document-based compliance reporting and analysis</item>
/// <item><strong>API Operations:</strong> Complete document data transfer in REST API operations</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive document DTO
/// var documentDto = new DocumentWithRevisionsDto
/// {
///     Id = Guid.NewGuid(),
///     FileName = "Contract_Amendment_v2.pdf",
///     Extension = "pdf",
///     FileSize = 2547832,
///     MimeType = "application/pdf",
///     Checksum = "a3b5c7d9e1f2a4b6c8d0e2f4a6b8c0d2e4f6a8b0c2d4e6f8a0b2c4d6e8f0a2b4c6",
///     IsCheckedOut = false,
///     IsDeleted = false,
///     Revisions = new List&lt;RevisionDto&gt;
///     {
///         new RevisionDto 
///         { 
///             Id = Guid.NewGuid(), 
///             RevisionNumber = 1, 
///             CreationDate = DateTime.UtcNow.AddDays(-14) 
///         }
///     }
/// };
/// 
/// // Comprehensive validation
/// var validationResults = DocumentWithRevisionsDto.ValidateModel(documentDto);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Document Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Professional document analysis
/// Console.WriteLine($"Document '{documentDto.FileName}' has {documentDto.RevisionCount} revisions");
/// Console.WriteLine($"Document integrity: {(documentDto.HasValidChecksum ? "Valid" : "Invalid")}");
/// </code>
/// </example>
public partial class DocumentWithRevisionsDto : IValidatableObject, IEquatable<DocumentWithRevisionsDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the document.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the document within the ADMS system.
    /// It corresponds directly to <see cref="ADMS.Domain.Entities.Document.Id"/> and is used for 
    /// establishing relationships, revision associations, and all system operations requiring 
    /// precise document identification.
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required for Updates:</strong> Must be provided when updating existing documents</item>
    /// <item><strong>Foreign Key Reference:</strong> Referenced in Revision, DocumentActivityUser entities</item>
    /// <item><strong>API Operations:</strong> Document identification in REST API operations</item>
    /// <item><strong>Version Control:</strong> Links all revisions to the parent document</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using document ID for operations
    /// var document = new DocumentWithRevisionsDto 
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
    /// var contract = new DocumentWithRevisionsDto { FileName = "Smith_Purchase_Agreement_v2.pdf" };
    /// var motion = new DocumentWithRevisionsDto { FileName = "Motion_Summary_Judgment_2024.docx" };
    /// var exhibit = new DocumentWithRevisionsDto { FileName = "Exhibit_A_Financial_Records.xlsx" };
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
    /// <item><strong>Case Insensitive:</strong> Extensions are normalized for consistent handling</item>
    /// </list>
    /// 
    /// <para><strong>Supported Legal Document Formats:</strong></para>
    /// Common extensions include PDF, DOC, DOCX, XLS, XLSX, TXT for comprehensive legal 
    /// document support while maintaining security and compatibility standards.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Supported document extensions
    /// var pdfDoc = new DocumentWithRevisionsDto { Extension = "pdf" };   // Legal contracts, pleadings
    /// var wordDoc = new DocumentWithRevisionsDto { Extension = "docx" }; // Draft documents
    /// var excelDoc = new DocumentWithRevisionsDto { Extension = "xlsx" }; // Financial data
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
    /// var smallDoc = new DocumentWithRevisionsDto { FileSize = 45678 };     // 44.6 KB
    /// var mediumDoc = new DocumentWithRevisionsDto { FileSize = 2547832 };  // 2.4 MB
    /// var largeDoc = new DocumentWithRevisionsDto { FileSize = 15728640 };  // 15 MB
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
    /// var pdfContract = new DocumentWithRevisionsDto { MimeType = "application/pdf" };
    /// var wordDoc = new DocumentWithRevisionsDto { 
    ///     MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document" 
    /// };
    /// var excelSheet = new DocumentWithRevisionsDto { MimeType = "application/vnd.ms-excel" };
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
    /// var document = new DocumentWithRevisionsDto 
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
    /// var document = new DocumentWithRevisionsDto 
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
    /// var document = new DocumentWithRevisionsDto 
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
    /// Gets or sets the creation date and time of the document in UTC.
    /// </summary>
    /// <remarks>
    /// The creation date establishes the temporal foundation for the document lifecycle and must be accurately
    /// maintained for legal audit trail compliance and professional document management standards.
    /// </remarks>
    [Required(ErrorMessage = "Creation date is required for temporal tracking and audit compliance.")]
    public required DateTime CreationDate { get; init; }

    /// <summary>
    /// Validates creation date using BaseValidationDto patterns.
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

    #endregion Core Properties

    #region Collection Properties

    /// <summary>
    /// Gets or sets the collection of revisions for this document.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.Domain.Entities.Document.Revisions"/> and maintains the 
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
    /// <item><strong>Minimum Requirement:</strong> Documents must have at least one revision</item>
    /// <item><strong>Sequential Integrity:</strong> Revision numbers must be sequential without gaps</item>
    /// <item><strong>Temporal Consistency:</strong> Revision dates must be chronologically consistent</item>
    /// <item><strong>Validation Completeness:</strong> Each revision must pass comprehensive validation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Document with revision history
    /// var document = new DocumentWithRevisionsDto
    /// {
    ///     FileName = "Contract_Amendment.pdf",
    ///     Revisions = new List&lt;RevisionDto&gt;
    ///     {
    ///         new RevisionDto 
    ///         { 
    ///             RevisionNumber = 1, 
    ///             CreationDate = DateTime.UtcNow.AddDays(-14) 
    ///         },
    ///         new RevisionDto 
    ///         { 
    ///             RevisionNumber = 2, 
    ///             CreationDate = DateTime.UtcNow.AddDays(-7) 
    ///         }
    ///     }
    /// };
    /// 
    /// // Version analysis
    /// var currentVersion = document.CurrentRevision;
    /// var versionHistory = document.RevisionHistory;
    /// Console.WriteLine($"Document has {document.RevisionCount} versions");
    /// </code>
    /// </example>
    public ICollection<RevisionDto> Revisions { get; set; } = [];

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
    public ICollection<DocumentActivityUserDto> DocumentActivityUsers { get; set; } = [];

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
    /// <item><strong>MOVED:</strong> Documents moved from source matters</item>
    /// <item><strong>COPIED:</strong> Documents copied from source matters</item>
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
    public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; set; } = [];

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
    /// This computed property mirrors <see cref="ADMS.Domain.Entities.Document.RevisionCount"/> and provides 
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
    ///     Console.WriteLine($"Version {revision.RevisionNumber}: {revision.CreationDate}");
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
    /// Validates the DocumentWithRevisionsDto using the standardized ADMS validation hierarchy.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Implements comprehensive validation following the standardized ADMS validation hierarchy:
    /// <list type="number">
    /// <item><strong>Core Properties:</strong> Essential document properties using FileValidationHelper</item>
    /// <item><strong>Business Rules:</strong> Document lifecycle, version control, and professional standards</item>
    /// <item><strong>Cross-Property:</strong> MIME type consistency, temporal validation, and referential integrity</item>
    /// <item><strong>Collections:</strong> Deep validation of revision and activity audit trail collections</item>
    /// <item><strong>Revision-Specific:</strong> Version control integrity and revision history validation</item>
    /// </list>
    /// </remarks>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);

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

        // 5. Revision-Specific Validation
        foreach (var result in ValidateRevisionIntegrity())
            yield return result;

        // 6. Document Classification Validation
        foreach (var result in ValidateDocumentClassification())
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
    /// length constraints, and professional naming standards.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateFileName()
    {
        if (string.IsNullOrWhiteSpace(FileName))
        {
            yield return new ValidationResult("File name is required and cannot be empty.", [nameof(FileName)]);
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
    /// length constraints, and format validation.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateExtension()
    {
        if (string.IsNullOrWhiteSpace(Extension))
        {
            yield return new ValidationResult("File extension is required and cannot be empty.", [nameof(Extension)]);
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
                $"Allowed extensions: {string.Join(", ", FileValidationHelper.AllowedExtensions)}",
                [nameof(Extension)]);
        }

        if (Extension.Any(char.IsWhiteSpace))
        {
            yield return new ValidationResult(
                "File extension cannot contain whitespace characters.",
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
    /// Validates business rules specific to documents with revisions.
    /// </summary>
    /// <returns>A collection of validation results for business rule compliance.</returns>
    private IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Base business rules from DocumentWithoutRevisionsDto
        foreach (var result in FileValidationHelper.ValidateMimeTypeConsistency(
            MimeType, Extension, nameof(MimeType)))
            yield return result;

        // Document cannot be both checked out and deleted
        if (IsCheckedOut && IsDeleted)
        {
            yield return new ValidationResult(
                "A document cannot be both checked out and deleted simultaneously. " +
                "This violates professional document management business rules.",
                [nameof(IsCheckedOut), nameof(IsDeleted)]);
        }

        // Professional standards validation
        if (!string.IsNullOrWhiteSpace(Extension) && !FileValidationHelper.IsLegalDocumentFormat(Extension))
        {
            yield return new ValidationResult(
                $"Extension '{Extension}' is not a standard legal document format. " +
                "Consider using PDF, DOCX, or other approved legal document formats.",
                [nameof(Extension)]);
        }

        // REVISION-SPECIFIC BUSINESS RULES
        
        // Documents with revisions must have at least one revision
        if (Revisions.Count == 0)
        {
            yield return new ValidationResult(
                "Documents must have at least one revision for version control integrity.",
                [nameof(Revisions)]);
        }

        // Current revision must exist and be valid
        if (CurrentRevision == null && Revisions.Count > 0)
        {
            yield return new ValidationResult(
                "Document with revisions must have a valid current revision.",
                [nameof(Revisions)]);
        }

        // Version control business rules
        if (Revisions.Count > 0)
        {
            var maxRevisionNumber = Revisions.Max(r => r.RevisionNumber);
            if (maxRevisionNumber != Revisions.Count)
            {
                yield return new ValidationResult(
                    "Revision numbering must be sequential starting from 1 for version control integrity.",
                    [nameof(Revisions)]);
            }

            // Professional version control limits
            if (Revisions.Count > RevisionValidationHelper.MaxReasonableRevisionCount)
            {
                yield return new ValidationResult(
                    $"Document has an unusually high revision count (>{RevisionValidationHelper.MaxReasonableRevisionCount}). " +
                    "Verify version control practices and consider consolidation.",
                    [nameof(Revisions)]);
            }
        }

        // Deleted documents with revisions should have deletion audit trail
        if (IsDeleted && Revisions.Count > 0 && !HasValidAuditTrail())
        {
            yield return new ValidationResult(
                "Deleted documents with revision history must maintain comprehensive audit trails for legal compliance.",
                [nameof(IsDeleted), nameof(Revisions)]);
        }
    }

    /// <summary>
    /// Validates the <see cref="Revisions"/> collection using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the revisions collection.</returns>
    /// <remarks>
    /// Uses DtoValidationHelper for comprehensive validation of the revision collection,
    /// including business rule validation for required revisions.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateRevisions()
    {
        // Business rule: Documents must have at least one revision
        return DtoValidationHelper.ValidateCollection(
            Revisions,
            nameof(Revisions),
            new ValidationContext(this),
            allowEmptyCollection: false
        );
    }

    /// <summary>
    /// Validates the <see cref="DocumentActivityUsers"/> collection using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the document activity users collection.</returns>
    /// <remarks>
    /// Uses DtoValidationHelper for comprehensive validation of the document activity users collection.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateDocumentActivityUsers()
    {
        return DtoValidationHelper.ValidateCollection(DocumentActivityUsers, nameof(DocumentActivityUsers));
    }

    /// <summary>
    /// Validates the <see cref="MatterDocumentActivityUsersFrom"/> collection using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the from transfer activities collection.</returns>
    /// <remarks>
    /// Uses DtoValidationHelper for comprehensive validation of the source-side transfer activities collection.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateMatterDocumentActivityUsersFrom()
    {
        return DtoValidationHelper.ValidateCollection(MatterDocumentActivityUsersFrom, nameof(MatterDocumentActivityUsersFrom));
    }

    /// <summary>
    /// Validates the <see cref="MatterDocumentActivityUsersTo"/> collection using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the to transfer activities collection.</returns>
    /// <remarks>
    /// Uses DtoValidationHelper for comprehensive validation of the destination-side transfer activities collection.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateMatterDocumentActivityUsersTo()
    {
        return DtoValidationHelper.ValidateCollection(MatterDocumentActivityUsersTo, nameof(MatterDocumentActivityUsersTo));
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
    /// Validates a <see cref="DocumentWithRevisionsDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The DocumentWithRevisionsDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate DocumentWithRevisionsDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new DocumentWithRevisionsDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     FileName = "Legal_Contract.pdf",
    ///     Extension = "pdf",
    ///     MimeType = "application/pdf",
    ///     FileSize = 1234567,
    ///     Checksum = "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4"
    /// };
    /// 
    /// var results = DocumentWithRevisionsDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Document validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] DocumentWithRevisionsDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult(
                "DocumentWithRevisionsDto instance is required and cannot be null."));
            return results;
        }

        try
        {
            var context = new ValidationContext(dto);
            results.AddRange(dto.Validate(context));
        }
        catch (Exception ex)
        {
            results.Add(new ValidationResult(
                $"Validation failed with exception: {ex.Message}"));
        }

        return results;
    }

    /// <summary>
    /// Creates a DocumentWithRevisionsDto from ADMS.Domain.Entities.Document entity with validation.
    /// </summary>
    /// <param name="entity">The Document entity to convert. Cannot be null.</param>
    /// <param name="includeRevisions">Whether to include revision collections in the conversion.</param>
    /// <param name="includeActivityUsers">Whether to include activity user collections in the conversion.</param>
    /// <returns>A valid DocumentWithRevisionsDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create DocumentWithRevisionsDto instances from
    /// ADMS.Domain.Entities.Document entities with automatic validation and comprehensive error handling.
    /// 
    /// <para><strong>Entity Mapping:</strong></para>
    /// Maps all core properties and conditionally includes related collections based on parameters.
    /// 
    /// <para><strong>Validation Integration:</strong></para>
    /// Automatically validates the created DTO to ensure data integrity and business rule compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity with full relationships
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
    /// var dto = DocumentWithRevisionsDto.FromEntity(entity, 
    ///                                             includeRevisions: true, 
    ///                                             includeActivityUsers: true);
    /// </code>
    /// </example>
    public static DocumentWithRevisionsDto FromEntity(ADMS.Domain.Entities.Document entity,
                                                    bool includeRevisions = true,
                                                    bool includeActivityUsers = false)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new DocumentWithRevisionsDto
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
            Revisions = includeRevisions
                ? entity.Revisions.Select(r => RevisionDto.FromEntity(r)).ToList()
                : [],
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
                        IsCheckedOut = dau.Document.IsCheckedOut,
                        IsDeleted = dau.Document.IsDeleted,
                        CreationDate = dau.CreatedAt,
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
                ? entity.MatterDocumentActivityUsersTo.Select(mdaut => MatterDocumentActivityUserTo.FromEntity(mdaut)).ToList()
                : []
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Created DocumentWithRevisionsDto failed validation: {errorMessages}");

    }

    /// <summary>
    /// Creates multiple DocumentWithRevisionsDto instances from a collection of entities.
    /// </summary>
    /// <param name="entities">The collection of Document entities to convert. Cannot be null.</param>
    /// <param name="includeRevisions">Whether to include revision collections in the conversion.</param>
    /// <param name="includeActivityUsers">Whether to include activity user collections in the conversion.</param>
    /// <returns>A collection of valid DocumentWithRevisionsDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method is optimized for creating multiple document DTOs efficiently,
    /// with comprehensive error handling and validation for each converted entity.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Convert collection of document entities
    /// var entities = await context.Documents.Include(d => d.Revisions).ToListAsync();
    /// var documentDtos = DocumentWithRevisionsDto.FromEntities(entities, 
    ///                                                         includeRevisions: true, 
    ///                                                         includeActivityUsers: false);
    /// 
    /// // Process documents
    /// foreach (var documentDto in documentDtos)
    /// {
    ///     ProcessDocument(documentDto);
    /// }
    /// </code>
    /// </example>
    public static IList<DocumentWithRevisionsDto> FromEntities(IEnumerable<Domain.Entities.Document> entities,
                                                             bool includeRevisions = true,
                                                             bool includeActivityUsers = false)
    {
        ArgumentNullException.ThrowIfNull(entities);

        return entities.Select(entity => FromEntity(entity, includeRevisions, includeActivityUsers)).ToList();
    }

    #endregion Static Methods

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified DocumentWithRevisionsDto is equal to the current DocumentWithRevisionsDto.
    /// </summary>
    /// <param name="other">The DocumentWithRevisionsDto to compare with the current DocumentWithRevisionsDto.</param>
    /// <returns>true if the specified DocumentWithRevisionsDto is equal to the current DocumentWithRevisionsDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each document has a unique identifier.
    /// This follows the same equality pattern as ADMS.Domain.Entities.Document for consistency.
    /// </remarks>
    public bool Equals(DocumentWithRevisionsDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current DocumentWithRevisionsDto.
    /// </summary>
    /// <param name="obj">The object to compare with the current DocumentWithRevisionsDto.</param>
    /// <returns>true if the specified object is equal to the current DocumentWithRevisionsDto; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as DocumentWithRevisionsDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current DocumentWithRevisionsDto.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the DocumentWithRevisionsDto.
    /// </summary>
    /// <returns>A string that represents the current DocumentWithRevisionsDto.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the document,
    /// which is useful for debugging, logging, and display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new DocumentWithRevisionsDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     FileName = "Contract.pdf",
    ///     FileSize = 1234567
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "Document: Contract.pdf (1.2 MB) - 0 revisions, 0 activities"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Document: {FileName} ({FormattedFileSize}) - {RevisionCount} revisions, {TotalActivityCount} activities";

    #endregion String Representation

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
    /// Console.WriteLine($"Document revisions: {stats["RevisionCount"]}");
    /// Console.WriteLine($"Total activities: {stats["TotalActivityCount"]}");
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
            ["RevisionCount"] = RevisionCount,
            ["TotalActivityCount"] = TotalActivityCount,
            ["HasActivities"] = HasActivities,
            ["IsAvailableForEdit"] = IsAvailableForEdit,
            ["HasValidChecksum"] = HasValidChecksum,
            ["IsCheckedOut"] = IsCheckedOut,
            ["IsDeleted"] = IsDeleted
        };
    }

    #endregion Business Logic Methods

    #region Validation Extensions

    /// <summary>
    /// Comprehensive quick validation for DocumentWithRevisionsDto.
    /// </summary>
    /// <remarks>
    /// This validation checks core properties, file integrity, revision consistency, and professional standards.
    /// Designed for high-performance scenarios with bulk data processing or API input validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new DocumentWithRevisionsDto 
    /// { 
    ///     Id = Guid.NewGuid(),
    ///     FileName = "Legal_Contract.pdf",
    ///     Extension = "pdf",
    ///     MimeType = "application/pdf",
    ///     FileSize = 1234567,
    ///     Checksum = "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4"
    /// };
    /// 
    /// if (!DocumentWithRevisionsDto.IsValid(dto))
    /// {
    ///     throw new ValidationException("Document DTO is not valid for processing.");
    /// }
    /// </code>
    /// </example>
    public static bool IsValid(DocumentWithRevisionsDto dto) =>
        dto.Id != Guid.Empty &&
        FileValidationHelper.IsFileNameValid(dto.FileName) &&
        FileValidationHelper.IsExtensionAllowed(dto.Extension) &&
        FileValidationHelper.IsFileSizeValid(dto.FileSize) &&
        FileValidationHelper.IsMimeTypeAllowed(dto.MimeType) &&
        FileValidationHelper.IsValidChecksum(dto.Checksum) &&
        RevisionValidationHelper.IsValidDate(dto.CreationDate) && // Use RevisionValidationHelper for dates
        dto is not { IsCheckedOut: true, IsDeleted: true } &&
        dto.Revisions.Count > 0 &&
        dto.Revisions.All(r => 
            RevisionValidationHelper.IsValidRevisionNumber(r.RevisionNumber) &&
            RevisionValidationHelper.IsValidDate(r.CreationDate) &&
            RevisionValidationHelper.IsValidDate(r.ModificationDate) &&
            RevisionValidationHelper.IsValidDateSequence(r.CreationDate, r.ModificationDate) &&
            (r.DocumentId == null || RevisionValidationHelper.IsValidDocumentId(r.DocumentId.Value))) &&
        HasValidAuditTrail(dto);

    /// <summary>
    /// Determines whether the specified document is in a valid state based on its properties.
    /// </summary>
    /// <param name="dto">The document to validate, including its revisions and state flags.</param>
    /// <returns><see langword="true"/> if the document has a non-empty identifier, contains at least one revision,  and is
    /// either not checked out or not marked as deleted; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidState(DocumentWithRevisionsDto dto) =>
        dto.Id != Guid.Empty &&
        dto.Revisions.Count > 0 &&
        !dto.IsCheckedOut || !dto.IsDeleted;

    /// <summary>
    /// Checks if the document has a valid audit trail based on activity users.
    /// </summary>
    /// <remarks>
    /// Ensures that all critical document operations (create, update, delete, restore) have corresponding audit trail entries.
    /// </remarks>
    private static bool HasValidAuditTrail(DocumentWithRevisionsDto document)
    {
        // Check required activities based on document state
        var requiredActivities = new[] { "CREATED" };
        if (document.IsDeleted)
            requiredActivities = requiredActivities.Concat(["DELETED"]).ToArray();

        var documentActivities = document.DocumentActivityUsers
            .Select(a => a.DocumentActivity?.Activity?.ToUpperInvariant())
            .Where(a => !string.IsNullOrEmpty(a))
            .ToHashSet();

        // Verify all required activities are present
        var hasRequiredActivities = requiredActivities.All(activity => 
            documentActivities.Contains(activity));

        // Verify revision audit trails
        var hasRevisionAudits = document.Revisions.All(r => 
            r.RevisionActivityUsers?.Count > 0 || r.IsDeleted);

        return hasRequiredActivities && hasRevisionAudits;
    }
    #endregion Validation Extensions

    #region Revisions Validation

    /// <summary>
    /// Validates the collections associated with the current object and returns any validation errors.
    /// </summary>
    /// <remarks>This method performs validation on specific collections only if they contain items,
    /// optimizing performance. It validates the <see cref="Revisions"/>, <see cref="DocumentActivityUsers"/>,  <see
    /// cref="MatterDocumentActivityUsersFrom"/>, and <see cref="MatterDocumentActivityUsersTo"/> collections.</remarks>
    /// <returns>An <see cref="IEnumerable{ValidationResult}"/> containing the validation results.  The collection will be empty
    /// if no validation errors are found.</returns>
    protected IEnumerable<ValidationResult> ValidateCollections()
    {
        // Validate only when collection has items (performance optimization)
        if (Revisions.Count > 0)
        {
            foreach (var result in ValidateRevisionsCollection())
                yield return result;
        }

        if (DocumentActivityUsers.Count > 0)
        {
            foreach (var result in ValidateDocumentActivityUsers())
                yield return result;
        }

        // Use DtoValidationHelper for consistent collection validation
        foreach (var result in DtoValidationHelper.ValidateCollection(
                     MatterDocumentActivityUsersFrom, nameof(MatterDocumentActivityUsersFrom)))
            yield return result;

        foreach (var result in DtoValidationHelper.ValidateCollection(
                     MatterDocumentActivityUsersTo, nameof(MatterDocumentActivityUsersTo)))
            yield return result;
    }

    /// <summary>
    /// Validates the revisions collection for DocumentWithRevisionsDto using advanced revision validation rules.
    /// </summary>
    /// <remarks>
    /// This validation ensures the revisions collection is properly structured, with sequential numbering,
    /// and that each revision adheres to professional standards and business rules.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateRevisionsCollection()
    {
        var index = 0;
        foreach (var revision in Revisions)
        {
            // Provide specific context for each revision error
            var revisionContext = new ValidationContext(revision);
            var revisionErrors = revision.Validate(revisionContext);

            foreach (var error in revisionErrors)
            {
                var memberNames = error.MemberNames.Any()
                    ? error.MemberNames.Select(memberName => $"Revisions[{index}].{memberName}")
                    : [$"Revisions[{index}]"];

                yield return new ValidationResult(
                    $"Revision {revision.RevisionNumber}: {error.ErrorMessage}", 
                    memberNames);
            }
            index++;
        }
    }

    /// <summary>
    /// Validates the revision integrity for DocumentWithRevisionsDto using professional standards and business rules.
    /// </summary>
    /// <remarks>
    /// This validation checks that revisions adhere to professional standards for document management,
    /// including proper dating, sequencing, and compliance with business rules.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateRevisionIntegrity()
    {
        if (Revisions.Count == 0) yield break;

        var index = 0;
        foreach (var revision in Revisions)
        {
            // Document-revision date consistency
            foreach (var result in RevisionValidationHelper.ValidateDocumentRevisionDates(
                revision.CreationDate, CreationDate, revision.RevisionNumber,
                $"Revisions[{index}].CreationDate"))
                yield return result;

            // Professional standards validation
            var developmentTime = revision.ModificationDate - revision.CreationDate;
            foreach (var result in RevisionValidationHelper.ValidateProfessionalStandards(
                revision.RevisionNumber, revision.CreationDate, developmentTime,
                revision.ActivityCount, $"Revisions[{index}]."))
                yield return result;

            index++;
        }
    }

    #endregion Revisions Validation
}