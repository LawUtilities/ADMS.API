using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ADMS.Application.DTOs;

/// <summary>
/// Data Transfer Object for creating new documents within the ADMS legal document management system.
/// </summary>
/// <remarks>
/// This DTO serves as the specialized representation for document creation operations within the ADMS legal document management system,
/// corresponding to creatable properties of <see cref="ADMS.API.Entities.Document"/>. It provides focused document creation
/// capabilities while enforcing professional validation standards and business rules specific to new document creation.
/// 
/// <para><strong>Enhanced with Standardized Validation (.NET 9):</strong></para>
/// <list type="bullet">
/// <item><strong>BaseValidationDto Integration:</strong> Inherits standardized ADMS validation patterns</item>
/// <item><strong>File-Specific Validation:</strong> Comprehensive file metadata and integrity validation</item>
/// <item><strong>Performance Optimized:</strong> Uses yield return for lazy validation evaluation</item>
/// <item><strong>Validation Hierarchy:</strong> Follows standardized core → business → cross-property → custom pattern</item>
/// <item><strong>Security Enhanced:</strong> Advanced file security validation and integrity checks</item>
/// </list>
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Creation-Focused Design:</strong> Specifically designed for new document creation operations</item>
/// <item><strong>Standardized Validation:</strong> Uses BaseValidationDto for consistent validation patterns</item>
/// <item><strong>Professional File Validation:</strong> Uses centralized FileValidationHelper for comprehensive data integrity</item>
/// <item><strong>Business Rule Enforcement:</strong> Enforces document creation business rules and constraints</item>
/// <item><strong>Entity Synchronization:</strong> Maps to creatable properties of ADMS.API.Entities.Document</item>
/// </list>
/// 
/// <para><strong>Validation Hierarchy:</strong></para>
/// Following BaseValidationDto standardized validation pattern:
/// <list type="number">
/// <item><strong>Core Properties:</strong> FileName, Extension, FileSize, MimeType, Checksum validation</item>
/// <item><strong>Business Rules:</strong> File security, creation safety, professional standards</item>
/// <item><strong>Cross-Property:</strong> MIME type and extension consistency, checksum integrity</item>
/// <item><strong>Custom Rules:</strong> Document creation specific rules and security validations</item>
/// </list>
/// 
/// <para><strong>Entity Property Mapping:</strong></para>
/// This DTO maps to creatable properties from ADMS.API.Entities.Document:
/// <list type="bullet">
/// <item><strong>File Metadata:</strong> FileName, Extension, FileSize, MimeType for file identification</item>
/// <item><strong>Integrity Properties:</strong> Checksum for file integrity verification during creation</item>
/// <item><strong>Creation Status:</strong> IsCheckedOut for initial document state (typically false for new documents)</item>
/// <item><strong>Extended Metadata:</strong> Description for enhanced document classification during creation</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>New Document Creation:</strong> Creating new documents with proper metadata and validation</item>
/// <item><strong>File Upload Operations:</strong> Processing uploaded files with comprehensive validation</item>
/// <item><strong>Document Import:</strong> Importing documents from external sources with validation</item>
/// <item><strong>API Operations:</strong> Document creation operations in REST API endpoints</item>
/// </list>
/// 
/// <para><strong>Performance Benefits with Standardized Validation:</strong></para>
/// <list type="bullet">
/// <item><strong>Early Termination:</strong> Validation stops on critical errors for better performance</item>
/// <item><strong>Lazy Evaluation:</strong> File properties validated only when needed</item>
/// <item><strong>Consistent Error Handling:</strong> Standardized error formatting and reporting</item>
/// <item><strong>Memory Efficient:</strong> Optimized validation memory usage</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a new document with standardized validation
/// var creationDto = new DocumentForCreationDto
/// {
///     FileName = "New_Contract_Agreement",
///     Extension = "pdf",
///     FileSize = 2547832,
///     MimeType = "application/pdf",
///     Checksum = "a3b5c7d9e1f2a4b6c8d0e2f4a6b8c0d2e4f6a8b0c2d4e6f8a0b2c4d6e8f0a2b4c6",
///     IsCheckedOut = false,
///     Description = "New contract agreement for client engagement and legal services"
/// };
/// 
/// // Standardized validation using BaseValidationDto
/// var validationResults = BaseValidationDto.ValidateModel(creationDto);
/// if (BaseValidationDto.HasValidationErrors(validationResults))
/// {
///     var summary = BaseValidationDto.GetValidationSummary(validationResults);
///     _logger.LogWarning("Document creation validation failed: {ValidationSummary}", summary);
/// }
/// 
/// // Professional creation analysis with validation
/// if (creationDto.IsValid && creationDto.HasValidChecksum)
/// {
///     ProcessDocumentCreation(creationDto);
/// }
/// </code>
/// </example>
public partial class DocumentForCreationDto : BaseValidationDto, IEquatable<DocumentForCreationDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the filename of the document to be created (without extension).
    /// </summary>
    /// <remarks>
    /// The file name serves as the primary human-readable identifier for the new document and must conform 
    /// to professional file naming conventions.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using FileValidationHelper and BaseValidationDto.ValidateString().
    /// </remarks>
    [Required(ErrorMessage = "File name is required for new document creation.")]
    [MaxLength(FileValidationHelper.MaxFileNameLength,
        ErrorMessage = "File name cannot exceed {1} characters.")]
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets the extension for the file in question (without dot).
    /// </summary>
    /// <remarks>
    /// The file extension identifies the document format and must be one of the allowed extensions 
    /// for the ADMS system.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using FileValidationHelper and BaseValidationDto.ValidateString().
    /// </remarks>
    [Required(ErrorMessage = "File extension is required for document format identification.")]
    [MaxLength(FileValidationHelper.MaxExtensionLength,
        ErrorMessage = "File extension cannot exceed {1} characters.")]
    public required string Extension { get; set; }

    /// <summary>
    /// Gets or sets the size of the file in bytes.
    /// </summary>
    /// <remarks>
    /// The file size provides essential metadata for storage management and creation validation.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() with range and business rule validation.
    /// </remarks>
    [Range(0, long.MaxValue, ErrorMessage = "File size must be non-negative.")]
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the file (e.g., "application/pdf").
    /// </summary>
    /// <remarks>
    /// The MIME type provides precise content type identification essential for proper document handling 
    /// during creation.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() and ValidateCrossPropertyRules() for consistency with extension.
    /// </remarks>
    [Required(ErrorMessage = "MIME type is required for proper document handling.")]
    [MaxLength(128, ErrorMessage = "MIME type cannot exceed 128 characters.")]
    [RegularExpression(@"^[\w\.\-]+\/[\w\.\-\+]+$", ErrorMessage = "Invalid MIME type format.")]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the checksum (e.g., SHA256 hash) of the file for integrity verification.
    /// </summary>
    /// <remarks>
    /// The checksum provides critical file integrity verification capabilities essential for legal 
    /// document creation.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() and ValidateCrossPropertyRules() for format and integrity.
    /// </remarks>
    [Required(ErrorMessage = "Checksum is required for document integrity verification.")]
    [MaxLength(128, ErrorMessage = "Checksum cannot exceed 128 characters.")]
    [RegularExpression(@"^[A-Fa-f0-9]+$", ErrorMessage = "Checksum must be a hexadecimal string.")]
    public string Checksum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description of the document for enhanced categorization during creation.
    /// </summary>
    /// <remarks>
    /// The description field provides additional metadata capabilities for comprehensive document management
    /// during creation.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateString() with professional standards.
    /// </remarks>
    [MaxLength(256, ErrorMessage = "Description cannot exceed 256 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if the document is checked out (should typically be false on creation).
    /// </summary>
    /// <remarks>
    /// The check-out status for new documents typically should be false, as new documents are created 
    /// in an available state.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateBusinessRules() for creation best practices.
    /// </remarks>
    public bool IsCheckedOut { get; set; }

    #endregion Core Properties

    #region Computed Properties

    /// <summary>
    /// Gets a value indicating whether the document will be available for editing after creation.
    /// </summary>
    public bool IsAvailableForEdit => !IsCheckedOut;

    /// <summary>
    /// Gets the document status as a human-readable string for the new document.
    /// </summary>
    public string Status => IsCheckedOut ? "Checked Out" : "Active";

    /// <summary>
    /// Gets a value indicating whether the checksum appears to be valid for creation.
    /// </summary>
    public bool HasValidChecksum => !string.IsNullOrWhiteSpace(Checksum) &&
                                   Checksum64HexRegex().IsMatch(Checksum) &&
                                   Checksum.Length == 64;

    /// <summary>
    /// Gets the file size formatted as a human-readable string for the new document.
    /// </summary>
    public string FormattedFileSize => FileSize switch
    {
        < 1024 => $"{FileSize} bytes",
        < 1024 * 1024 => $"{FileSize / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{FileSize / (1024.0 * 1024.0):F1} MB",
        _ => $"{FileSize / (1024.0 * 1024.0 * 1024.0):F1} GB"
    };

    /// <summary>
    /// Gets the display text suitable for UI controls and document identification during creation.
    /// </summary>
    public string DisplayText => !string.IsNullOrWhiteSpace(Description)
        ? $"{FileName} - {Description}"
        : FileName;

    /// <summary>
    /// Gets the full file name including extension for complete identification during creation.
    /// </summary>
    public string FullFileName => string.IsNullOrWhiteSpace(Extension)
        ? FileName
        : $"{FileName}.{Extension}";

    /// <summary>
    /// Gets a value indicating whether this creation includes an extended description.
    /// </summary>
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    /// <summary>
    /// Gets a value indicating whether this creation DTO has valid data for system operations.
    /// </summary>
    public bool IsValid => HasValidChecksum &&
                          !string.IsNullOrWhiteSpace(FileName) &&
                          !string.IsNullOrWhiteSpace(Extension) &&
                          !string.IsNullOrWhiteSpace(MimeType) &&
                          FileSize > 0;

    #endregion Computed Properties

    #region Standardized Validation Implementation

    /// <summary>
    /// Validates core properties such as file metadata, integrity, and required fields.
    /// </summary>
    /// <returns>A collection of validation results for core property validation.</returns>
    /// <remarks>
    /// This method implements the first step of the BaseValidationDto validation hierarchy,
    /// validating essential file properties using standardized validation helpers.
    /// 
    /// <para><strong>Core Property Validation Steps:</strong></para>
    /// <list type="number">
    /// <item>File name validation using BaseValidationDto.ValidateString() and FileValidationHelper</item>
    /// <item>Extension validation using BaseValidationDto.ValidateString() and FileValidationHelper</item>
    /// <item>MIME type validation using BaseValidationDto.ValidateString() and format validation</item>
    /// <item>Checksum validation using BaseValidationDto.ValidateString() and format validation</item>
    /// <item>File size validation with range constraints</item>
    /// <item>Description validation using BaseValidationDto.ValidateString()</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCoreProperties()
    {
        // Validate file name using standardized string validation
        foreach (var result in ValidateString(
            FileName,
            nameof(FileName),
            required: true,
            minLength: 1,
            maxLength: FileValidationHelper.MaxFileNameLength))
        {
            yield return result;
        }

        // Additional file name validation using FileValidationHelper
        if (!string.IsNullOrWhiteSpace(FileName))
        {
            if (!FileValidationHelper.IsFileNameValid(FileName))
            {
                yield return CreateValidationResult(
                    "File name contains invalid characters or format for new document creation.",
                    nameof(FileName));
            }

            if (FileValidationHelper.IsReservedFileName(FileName))
            {
                yield return CreateValidationResult(
                    "File name cannot use a reserved system name for new document creation.",
                    nameof(FileName));
            }

            if (!FileName.Any(char.IsLetterOrDigit))
            {
                yield return CreateValidationResult(
                    "File name must contain at least one alphanumeric character for professional identification.",
                    nameof(FileName));
            }
        }

        // Validate extension using standardized string validation
        foreach (var result in ValidateString(
            Extension,
            nameof(Extension),
            required: true,
            minLength: 1,
            maxLength: FileValidationHelper.MaxExtensionLength))
        {
            yield return result;
        }

        // Additional extension validation using FileValidationHelper
        if (!string.IsNullOrWhiteSpace(Extension))
        {
            var extensionToCheck = Extension.TrimStart('.');
            if (!ExtensionAlphanumericRegex().IsMatch(extensionToCheck))
            {
                yield return CreateValidationResult(
                    "File extension must be alphanumeric for security and compatibility.",
                    nameof(Extension));
            }

            if (!FileValidationHelper.IsExtensionAllowed(Extension))
            {
                yield return CreateValidationResult(
                    $"Extension '{Extension}' is not allowed for legal document management creation. " +
                    $"Allowed extensions: {FileValidationHelper.AllowedExtensionsList}",
                    nameof(Extension));
            }

            if (!Extension.Equals(Extension.ToLowerInvariant(), StringComparison.Ordinal))
            {
                yield return CreateValidationResult(
                    "File extension must be lowercase for consistency during creation.",
                    nameof(Extension));
            }
        }

        // Validate MIME type using standardized string validation
        foreach (var result in ValidateString(
            MimeType,
            nameof(MimeType),
            required: true,
            maxLength: 128))
        {
            yield return result;
        }

        // Additional MIME type validation
        if (!string.IsNullOrWhiteSpace(MimeType))
        {
            if (!FileValidationHelper.IsMimeTypeAllowed(MimeType))
            {
                yield return CreateValidationResult(
                    $"MIME type '{MimeType}' is not allowed for legal document management creation.",
                    nameof(MimeType));
            }

            if (!MimeTypeRegex().IsMatch(MimeType))
            {
                yield return CreateValidationResult(
                    "Invalid MIME type format. Expected format: type/subtype (e.g., application/pdf) for creation.",
                    nameof(MimeType));
            }
        }

        // Validate checksum using standardized string validation
        foreach (var result in ValidateString(
            Checksum,
            nameof(Checksum),
            required: true,
            maxLength: 128))
        {
            yield return result;
        }

        // Additional checksum format validation
        if (!string.IsNullOrWhiteSpace(Checksum))
        {
            if (!Checksum64HexRegex().IsMatch(Checksum))
            {
                yield return CreateValidationResult(
                    "Checksum must be a valid 64-character hexadecimal string (SHA256 hash) for proper integrity verification during creation.",
                    nameof(Checksum));
            }
        }

        // Validate file size with business constraints
        if (FileSize <= 0)
        {
            yield return CreateValidationResult(
                "File size must be greater than zero for actual document files during creation.",
                nameof(FileSize));
        }

        // Validate description using standardized string validation (optional)
        foreach (var result in ValidateString(
            Description,
            nameof(Description),
            required: false,
            minLength: 3,
            maxLength: 256))
        {
            yield return result;
        }
    }

    /// <summary>
    /// Validates business rules specific to document creation and professional standards.
    /// </summary>
    /// <returns>A collection of validation results for business rule validation.</returns>
    /// <remarks>
    /// This method implements the second step of the BaseValidationDto validation hierarchy,
    /// validating domain-specific business rules for document creation and file management.
    /// 
    /// <para><strong>Business Rules Validated:</strong></para>
    /// <list type="bullet">
    /// <item>Document creation safety (checked out status)</item>
    /// <item>File size constraints for professional document management</item>
    /// <item>Professional naming standards compliance</item>
    /// <item>Security validation for file types</item>
    /// <item>Professional description standards</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Business rule: New documents typically should not be checked out on creation
        if (IsCheckedOut)
        {
            yield return CreateValidationResult(
                "A new document typically should not be checked out during creation. " +
                "Consider creating the document first, then checking it out if immediate editing is needed.",
                nameof(IsCheckedOut));
        }

        // Business rule: File size constraints for professional document management
        if (FileSize > 100 * 1024 * 1024) // 100 MB limit for typical legal documents
        {
            yield return CreateValidationResult(
                "File size exceeds recommended limit for efficient document management during creation (100 MB).",
                nameof(FileSize));
        }

        // Business rule: Very large files may need special handling
        if (FileSize > 500 * 1024 * 1024) // 500 MB critical threshold
        {
            yield return CreateValidationResult(
                "File size exceeds critical threshold (500 MB). Large files may require special processing procedures.",
                nameof(FileSize));
        }

        // Business rule: Professional naming standards for file names
        if (!string.IsNullOrWhiteSpace(FileName))
        {
            if (FileName.Trim().Length != FileName.Length)
            {
                yield return CreateValidationResult(
                    "File name should not have leading or trailing whitespace for professional presentation.",
                    nameof(FileName));
            }

            // Check for professional naming patterns
            if (FileName.Contains("..") || FileName.Contains("--"))
            {
                yield return CreateValidationResult(
                    "File name contains consecutive special characters which may cause system compatibility issues.",
                    nameof(FileName));
            }

            // Check for overly short names
            if (FileName.Length < 3)
            {
                yield return CreateValidationResult(
                    "File name is too short for professional identification standards.",
                    nameof(FileName));
            }
        }

        // Business rule: Professional description standards when provided
        if (HasDescription)
        {
            var trimmedDescription = Description!.Trim();
            if (trimmedDescription != Description)
            {
                yield return CreateValidationResult(
                    "Description should not have leading or trailing whitespace for professional presentation during creation.",
                    nameof(Description));
            }

            // Check for inappropriate placeholder text
            var invalidPatterns = new[] { "TODO", "FIXME", "XXX", "HACK", "TEMP" };
            foreach (var pattern in invalidPatterns)
            {
                if (trimmedDescription.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    yield return CreateValidationResult(
                        $"Description contains placeholder text '{pattern}' which is not appropriate for professional documents during creation.",
                        nameof(Description));
                }
            }

            if (trimmedDescription.All(char.IsWhiteSpace))
            {
                yield return CreateValidationResult(
                    "Description cannot consist entirely of whitespace characters during creation.",
                    nameof(Description));
            }
        }

        // Business rule: Security validation for executable extensions
        if (!string.IsNullOrWhiteSpace(Extension))
        {
            var dangerousExtensions = new[] { "exe", "bat", "com", "scr", "pif", "cmd", "vbs", "js" };
            if (dangerousExtensions.Contains(Extension.ToLowerInvariant()))
            {
                yield return CreateValidationResult(
                    $"Extension '{Extension}' is not allowed for security reasons in legal document management.",
                    nameof(Extension));
            }
        }
    }

    /// <summary>
    /// Validates cross-property relationships and consistency including MIME type and extension matching.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    /// <remarks>
    /// This method implements the third step of the BaseValidationDto validation hierarchy,
    /// validating relationships between file properties and ensuring consistency.
    /// 
    /// <para><strong>Cross-Property Rules:</strong></para>
    /// <list type="bullet">
    /// <item>MIME type and extension consistency validation</item>
    /// <item>File metadata coherence validation</item>
    /// <item>Checksum and file size relationship validation</item>
    /// <item>Professional standards consistency</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // Validate MIME type and extension consistency
        if (!string.IsNullOrWhiteSpace(Extension) && !string.IsNullOrWhiteSpace(MimeType))
        {
            var expectedMimeTypes = GetExpectedMimeTypesForExtension(Extension.ToLowerInvariant());
            if (expectedMimeTypes.Any() && !expectedMimeTypes.Contains(MimeType, StringComparer.OrdinalIgnoreCase))
            {
                yield return CreateValidationResult(
                    $"MIME type '{MimeType}' does not match the expected types for extension '{Extension}' during creation. " +
                    $"Expected: {string.Join(", ", expectedMimeTypes)}",
                    nameof(MimeType), nameof(Extension));
            }
        }

        // Validate meaningful creation data consistency
        if (string.IsNullOrWhiteSpace(FileName) && string.IsNullOrWhiteSpace(Extension) &&
            string.IsNullOrWhiteSpace(MimeType) && FileSize == 0)
        {
            yield return CreateValidationResult(
                "Document creation must include meaningful metadata for professional document management.",
                nameof(FileName), nameof(Extension), nameof(MimeType), nameof(FileSize));
        }

        // Validate file size and checksum relationship (empty checksum suggests no actual file)
        if (FileSize > 0 && string.IsNullOrWhiteSpace(Checksum))
        {
            yield return CreateValidationResult(
                "Files with size greater than zero must have a checksum for integrity verification.",
                nameof(FileSize), nameof(Checksum));
        }

        // Validate file name and extension consistency (professional naming)
        if (!string.IsNullOrWhiteSpace(FileName) && !string.IsNullOrWhiteSpace(Extension))
        {
            // Check if filename already contains the extension
            if (FileName.EndsWith($".{Extension}", StringComparison.OrdinalIgnoreCase))
            {
                yield return CreateValidationResult(
                    "File name should not include the extension as it is specified separately.",
                    nameof(FileName), nameof(Extension));
            }
        }

        // Validate description and filename consistency (avoid redundancy)
        if (HasDescription && !string.IsNullOrWhiteSpace(FileName))
        {
            if (Description!.Trim().Equals(FileName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                yield return CreateValidationResult(
                    "Description should provide additional information beyond the file name for enhanced categorization.",
                    nameof(Description), nameof(FileName));
            }
        }
    }

    /// <summary>
    /// Validates custom rules specific to document creation security and professional standards.
    /// </summary>
    /// <returns>A collection of validation results for custom validation.</returns>
    /// <remarks>
    /// This method implements custom validation logic specific to document creation,
    /// including security considerations and advanced professional standards.
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCustomRules()
    {
        // Custom rule: Validate checksum format integrity for different hash types
        if (!string.IsNullOrWhiteSpace(Checksum))
        {
            // Advanced checksum validation based on length
            switch (Checksum.Length)
            {
                case 32: // MD5 - warn but don't fail (legacy support)
                    yield return CreateValidationResult(
                        "MD5 checksums are deprecated for security reasons. Consider using SHA256 for better integrity verification.",
                        nameof(Checksum));
                    break;
                case 40: // SHA-1 - warn but don't fail
                    yield return CreateValidationResult(
                        "SHA-1 checksums are deprecated for security reasons. Consider using SHA256 for better integrity verification.",
                        nameof(Checksum));
                    break;
                case 64: // SHA-256 - preferred
                    // This is good, no warning needed
                    break;
                default:
                    if (Checksum.Length < 32)
                    {
                        yield return CreateValidationResult(
                            "Checksum appears to be too short for reliable integrity verification.",
                            nameof(Checksum));
                    }
                    break;
            }
        }

        // Custom rule: Validate professional file naming patterns
        if (!string.IsNullOrWhiteSpace(FileName))
        {
            // Check for version numbering patterns
            if (System.Text.RegularExpressions.Regex.IsMatch(FileName, @"[vV]\d+\.\d+"))
            {
                // This is fine - professional version numbering
            }
            else if (FileName.Contains("copy", StringComparison.OrdinalIgnoreCase) ||
                     FileName.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
            {
                yield return CreateValidationResult(
                    "File name suggests this may be a duplicate. Consider using version numbering for document revisions.",
                    nameof(FileName));
            }
        }

        // Custom rule: Advanced MIME type security validation
        if (!string.IsNullOrWhiteSpace(MimeType))
        {
            // Check for potentially dangerous MIME types
            var dangerousMimeTypes = new[]
            {
                "application/x-msdownload",
                "application/x-executable",
                "application/x-dosexec"
            };

            if (dangerousMimeTypes.Contains(MimeType, StringComparer.OrdinalIgnoreCase))
            {
                yield return CreateValidationResult(
                    $"MIME type '{MimeType}' is not allowed for security reasons in legal document management.",
                    nameof(MimeType));
            }
        }

        // Custom rule: File size reasonableness validation
        if (FileSize > 0)
        {
            // Check for suspiciously small files for certain types
            if (!string.IsNullOrWhiteSpace(Extension) && FileSize < 100) // Less than 100 bytes
            {
                var extensionsThatShouldBeLarger = new[] { "pdf", "docx", "xlsx", "pptx" };
                if (extensionsThatShouldBeLarger.Contains(Extension.ToLowerInvariant()))
                {
                    yield return CreateValidationResult(
                        $"File size ({FileSize} bytes) seems unusually small for a {Extension.ToUpper()} file. Please verify the file integrity.",
                        nameof(FileSize), nameof(Extension));
                }
            }
        }
    }

    #endregion Standardized Validation Implementation

    #region Helper Methods

    /// <summary>
    /// Gets expected MIME types for a given file extension during creation.
    /// </summary>
    /// <param name="extension">The file extension (without dot, lowercase).</param>
    /// <returns>A collection of expected MIME types for the extension.</returns>
    /// <remarks>
    /// Provides MIME type validation support for common legal document formats during creation operations.
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

    #endregion Helper Methods

    #region Static Methods

    /// <summary>
    /// Creates a DocumentForCreationDto template with default values and standardized validation.
    /// </summary>
    /// <param name="fileName">The initial file name for the new document.</param>
    /// <param name="extension">The file extension for the new document.</param>
    /// <param name="fileSize">The file size in bytes for the new document.</param>
    /// <param name="mimeType">The MIME type for the new document.</param>
    /// <param name="checksum">The SHA256 checksum for the new document.</param>
    /// <param name="description">Optional description for the new document.</param>
    /// <returns>A DocumentForCreationDto instance with provided values and professional defaults.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var creationDto = DocumentForCreationDto.CreateNew(
    ///     fileName: "New_Contract_Agreement",
    ///     extension: "pdf",
    ///     fileSize: 2547832,
    ///     mimeType: "application/pdf",
    ///     checksum: "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4",
    ///     description: "New contract agreement for professional services"
    /// );
    /// 
    /// // DTO is guaranteed to be valid due to standardized validation
    /// Console.WriteLine($"Created valid document: {creationDto.FullFileName}");
    /// </code>
    /// </example>
    public static DocumentForCreationDto CreateNew(
        [NotNull] string fileName,
        [NotNull] string extension,
        long fileSize,
        [NotNull] string mimeType,
        [NotNull] string checksum,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(extension);
        ArgumentException.ThrowIfNullOrWhiteSpace(mimeType);
        ArgumentException.ThrowIfNullOrWhiteSpace(checksum);

        var dto = new DocumentForCreationDto
        {
            FileName = fileName.Trim(),
            Extension = extension.Trim().ToLowerInvariant(),
            FileSize = fileSize,
            MimeType = mimeType.Trim(),
            Checksum = checksum.Trim(),
            Description = description?.Trim(),
            IsCheckedOut = false // Professional default - new documents should be available
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(dto);
        if (!HasValidationErrors(validationResults)) return dto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Created DocumentForCreationDto failed validation: {summary}");
    }

    /// <summary>
    /// Creates multiple DocumentForCreationDto instances from file information with standardized validation.
    /// </summary>
    /// <param name="fileInfos">The collection of file information to convert.</param>
    /// <returns>A collection of valid DocumentForCreationDto instances.</returns>
    /// <remarks>
    /// This bulk creation method uses standardized validation and provides detailed error handling
    /// for invalid file information.
    /// </remarks>
    /// <example>
    /// <code>
    /// var fileInfos = new[]
    /// {
    ///     new { FileName = "Contract1", Extension = "pdf", FileSize = 1024L, MimeType = "application/pdf", Checksum = "abc123..." },
    ///     new { FileName = "Contract2", Extension = "docx", FileSize = 2048L, MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document", Checksum = "def456..." }
    /// };
    /// 
    /// var creationDtos = DocumentForCreationDto.CreateFromFiles(fileInfos);
    /// // All DTOs are guaranteed to be valid
    /// </code>
    /// </example>
    public static IList<DocumentForCreationDto> CreateFromFiles<T>(
        [NotNull] IEnumerable<T> fileInfos,
        Func<T, string> fileNameSelector,
        Func<T, string> extensionSelector,
        Func<T, long> fileSizeSelector,
        Func<T, string> mimeTypeSelector,
        Func<T, string> checksumSelector,
        Func<T, string?>? descriptionSelector = null)
    {
        ArgumentNullException.ThrowIfNull(fileInfos);
        ArgumentNullException.ThrowIfNull(fileNameSelector);
        ArgumentNullException.ThrowIfNull(extensionSelector);
        ArgumentNullException.ThrowIfNull(fileSizeSelector);
        ArgumentNullException.ThrowIfNull(mimeTypeSelector);
        ArgumentNullException.ThrowIfNull(checksumSelector);

        var result = new List<DocumentForCreationDto>();
        var errors = new List<string>();

        foreach (var fileInfo in fileInfos)
        {
            try
            {
                var dto = CreateNew(
                    fileNameSelector(fileInfo),
                    extensionSelector(fileInfo),
                    fileSizeSelector(fileInfo),
                    mimeTypeSelector(fileInfo),
                    checksumSelector(fileInfo),
                    descriptionSelector?.Invoke(fileInfo));
                result.Add(dto);
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                // Collect errors for comprehensive error reporting
                var fileName = fileNameSelector(fileInfo);
                errors.Add($"File {fileName}: {ex.Message}");

                // In production, use proper logging framework
                Console.WriteLine($"Warning: Skipped invalid file {fileName}: {ex.Message}");
            }
        }

        // Log summary if there were errors
        if (errors.Any())
        {
            Console.WriteLine($"File creation completed with {errors.Count} errors out of {fileInfos.Count()} files processed.");
        }

        return result;
    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this document will be available for editing after creation.
    /// </summary>
    /// <returns>true if the document will be available for editing after creation; otherwise, false.</returns>
    public bool WillBeAvailableAfterCreation() => IsAvailableForEdit;

    /// <summary>
    /// Gets creation summary information for this document creation operation.
    /// </summary>
    /// <returns>A dictionary containing creation summary information and metadata.</returns>
    public IReadOnlyDictionary<string, object> GetCreationSummary()
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
            ["IsAvailableForEdit"] = IsAvailableForEdit,
            ["HasValidChecksum"] = HasValidChecksum,
            ["IsCheckedOut"] = IsCheckedOut,
            ["DisplayText"] = DisplayText,
            ["IsValid"] = IsValid
        };
    }

    /// <summary>
    /// Analyzes the creation operation for potential impacts and validation concerns.
    /// </summary>
    /// <returns>An analysis of the creation operation including potential considerations.</returns>
    /// <remarks>
    /// This method provides analysis using the standardized validation results for comprehensive assessment.
    /// </remarks>
    /// <example>
    /// <code>
    /// var analysis = creationDto.AnalyzeCreation();
    /// Console.WriteLine($"Creation readiness: {analysis["CreationReadiness"]}");
    /// Console.WriteLine($"Validation summary: {analysis["ValidationSummary"]}");
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> AnalyzeCreation()
    {
        var recommendations = new List<string>();
        var considerations = new List<string>();

        // Perform validation to get current status
        var validationResults = ValidateModel(this);
        var validationStatus = HasValidationErrors(validationResults)
            ? GetValidationSummary(validationResults)
            : "Valid";

        // Analyze creation characteristics
        if (HasDescription)
        {
            considerations.Add("Enhanced document classification with description");
        }
        else
        {
            recommendations.Add("Consider adding a meaningful description for better document organization");
        }

        if (IsCheckedOut)
        {
            considerations.Add("Document will be checked out immediately upon creation");
            recommendations.Add("Ensure immediate check-out is intentional for new document workflow");
        }

        if (!HasValidChecksum)
        {
            recommendations.Add("Verify checksum format and content integrity before proceeding with creation");
        }

        if (FileSize > 50 * 1024 * 1024) // 50 MB threshold
        {
            considerations.Add("Large file size may impact system performance");
            recommendations.Add("Consider file optimization if possible for better system performance");
        }

        // Determine creation readiness based on validation
        var creationReadiness = !HasValidationErrors(validationResults) ? "Ready" : "Validation Required";

        return new Dictionary<string, object>
        {
            ["CreationReadiness"] = creationReadiness,
            ["Considerations"] = considerations,
            ["Recommendations"] = recommendations,
            ["ValidationStatus"] = validationStatus,
            ["ValidationSummary"] = validationStatus,
            ["FileSize"] = FormattedFileSize,
            ["HasEnhancedMetadata"] = HasDescription,
            ["SecurityStatus"] = HasValidChecksum ? "Verified" : "Requires Verification"
        };
    }

    /// <summary>
    /// Gets comprehensive document creation information including validation analysis.
    /// </summary>
    /// <returns>A dictionary containing detailed creation information and validation status.</returns>
    /// <remarks>
    /// This method provides structured creation information including validation status,
    /// useful for debugging, reporting, and administrative operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var info = creationDto.GetCreationInformation();
    /// foreach (var (key, value) in info)
    /// {
    ///     Console.WriteLine($"{key}: {value}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetCreationInformation()
    {
        // Get creation analysis and validation status
        var analysis = AnalyzeCreation();
        var summary = GetCreationSummary();

        return new Dictionary<string, object>
        {
            // File Information
            [nameof(FileName)] = FileName,
            [nameof(Extension)] = Extension,
            [nameof(FullFileName)] = FullFileName,
            [nameof(FileSize)] = FileSize,
            [nameof(FormattedFileSize)] = FormattedFileSize,
            [nameof(MimeType)] = MimeType,
            [nameof(Checksum)] = Checksum.Length > 10 ? $"{Checksum[..10]}..." : Checksum, // Truncated for display

            // Status Information
            [nameof(Status)] = Status,
            [nameof(IsCheckedOut)] = IsCheckedOut,
            [nameof(IsAvailableForEdit)] = IsAvailableForEdit,
            [nameof(HasValidChecksum)] = HasValidChecksum,
            [nameof(IsValid)] = IsValid,

            // Metadata Information
            [nameof(Description)] = Description ?? string.Empty,
            [nameof(HasDescription)] = HasDescription,
            [nameof(DisplayText)] = DisplayText,

            // Analysis Information
            ["CreationReadiness"] = analysis["CreationReadiness"],
            ["ValidationStatus"] = analysis["ValidationStatus"],
            ["SecurityStatus"] = analysis["SecurityStatus"],
            ["Considerations"] = analysis["Considerations"],
            ["Recommendations"] = analysis["Recommendations"]
        };
    }

    #endregion Business Logic Methods

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified DocumentForCreationDto is equal to the current DocumentForCreationDto.
    /// </summary>
    public bool Equals(DocumentForCreationDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return FileName == other.FileName &&
               Extension == other.Extension &&
               FileSize == other.FileSize &&
               MimeType == other.MimeType &&
               Checksum == other.Checksum &&
               IsCheckedOut == other.IsCheckedOut &&
               Description == other.Description;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current DocumentForCreationDto.
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as DocumentForCreationDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode() =>
        HashCode.Combine(FileName, Extension, FileSize, MimeType, Checksum, IsCheckedOut, Description);

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the DocumentForCreationDto.
    /// </summary>
    public override string ToString()
    {
        var baseInfo = $"Creation: {FileName}.{Extension} ({FormattedFileSize})";
        var descriptionInfo = HasDescription ? $" - {Description}" : string.Empty;
        var statusInfo = $" - {Status}";
        return baseInfo + descriptionInfo + statusInfo;
    }

    #endregion String Representation

    #region Compiled Regular Expressions

    /// <summary>
    /// Provides a compiled regex for validating MIME types at compile time.
    /// </summary>
    [GeneratedRegex(@"^[\w\.\-]+\/[\w\.\-\+]+$", RegexOptions.Compiled)]
    private static partial Regex MimeTypeRegex();

    /// <summary>
    /// Gets a compiled regex for alphanumeric file extensions.
    /// </summary>
    [GeneratedRegex(@"^[a-zA-Z0-9]+$", RegexOptions.Compiled)]
    private static partial Regex ExtensionAlphanumericRegex();

    /// <summary>
    /// Gets a compiled regex for 64-character hexadecimal checksums.
    /// </summary>
    [GeneratedRegex(@"^[A-Fa-f0-9]{64}$", RegexOptions.Compiled)]
    private static partial Regex Checksum64HexRegex();

    #endregion Compiled Regular Expressions
}