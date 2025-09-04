using Microsoft.Extensions.Logging;

using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ADMS.API.Common;

/// <summary>
/// Provides comprehensive helper methods and constants for validating file names, extensions, MIME types, and file content within the ADMS system.
/// </summary>
/// <remarks>
/// This static helper class provides enterprise-grade file validation functionality for the ADMS legal 
/// document management system, supporting all document-related DTOs including DocumentDto, 
/// DocumentForCreationDto, DocumentFullDto, DocumentMinimalDto, DocumentWithoutRevisionsDto, 
/// and DocumentWithRevisionsDto.
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item>File name and extension validation with database constraint alignment</item>
/// <item>MIME type validation for legal document formats with comprehensive type detection</item>
/// <item>File content validation using magic number detection for security</item>
/// <item>Checksum validation for file integrity verification using SHA256</item>
/// <item>Reserved name protection for cross-platform system compatibility</item>
/// <item>File size validation with configurable limits and performance considerations</item>
/// <item>Thread-safe operations with high-performance frozen collections</item>
/// <item>Integration with virus scanning and malware detection services</item>
/// </list>
/// 
/// <para><strong>Security Features:</strong></para>
/// <list type="bullet">
/// <item>File type spoofing protection through magic number validation</item>
/// <item>Comprehensive malware scanning integration support</item>
/// <item>Reserved filename protection against system conflicts</item>
/// <item>File size limits to prevent denial-of-service attacks</item>
/// <item>Content validation for data integrity and security</item>
/// </list>
/// 
/// <para><strong>Database Synchronization:</strong></para>
/// All validation constraints are synchronized with the Document entity constraints:
/// <list type="bullet">
/// <item>FileName: MaxLength(128) - matches Document.FileName constraint</item>
/// <item>Extension: MaxLength(5) - matches Document.Extension constraint</item>
/// <item>FileSize: Up to 100MB with configurable limits</item>
/// <item>Checksum: SHA256 format (64 hexadecimal characters)</item>
/// </list>
/// 
/// The class follows enterprise-grade security practices and is specifically designed 
/// for legal document management workflows with comprehensive audit trail support.
/// 
/// <para><strong>Thread Safety:</strong></para>
/// All methods in this class are thread-safe and use immutable frozen collections for optimal
/// performance in concurrent scenarios without external synchronization.
/// 
/// <para><strong>Performance Characteristics:</strong></para>
/// <list type="bullet">
/// <item>FrozenSet collections for O(1) lookup performance</item>
/// <item>Compiled regex patterns for optimal pattern matching</item>
/// <item>Memory-efficient file processing with stream-based operations</item>
/// <item>Lazy evaluation patterns for large collection processing</item>
/// </list>
/// </remarks>
public static partial class FileValidationHelper
{
    #region Constants

    /// <summary>
    /// The maximum allowed length for file names.
    /// </summary>
    /// <remarks>
    /// This value matches the MaxLength(128) constraint on the Document.FileName property 
    /// in the ADMS.API.Entities.Document entity to ensure consistency between validation 
    /// logic and database constraints.
    /// </remarks>
    public const int MaxFileNameLength = 128;

    /// <summary>
    /// The minimum allowed length for file names (excluding extension).
    /// </summary>
    /// <remarks>
    /// Ensures file names are meaningful and not just single characters.
    /// This prevents issues with file system operations and user experience.
    /// </remarks>
    public const int MinFileNameLength = 1;

    /// <summary>
    /// The maximum allowed length for file extensions.
    /// </summary>
    /// <remarks>
    /// This value matches the MaxLength(5) constraint on the Document.Extension property 
    /// in the ADMS.API.Entities.Document entity to ensure database consistency.
    /// Most common file extensions (.docx, .pdf, .jpeg) fit within this constraint.
    /// </remarks>
    public const int MaxExtensionLength = 5;

    /// <summary>
    /// The minimum allowed length for file extensions (excluding the dot).
    /// </summary>
    /// <remarks>
    /// Ensures file extensions are meaningful (minimum 1 character after the dot).
    /// </remarks>
    public const int MinExtensionLength = 1;

    /// <summary>
    /// The maximum allowed file size in bytes (100 MB).
    /// </summary>
    /// <remarks>
    /// This limit prevents excessively large files from impacting system performance
    /// while accommodating typical legal document sizes. Can be adjusted based on
    /// system requirements and storage capacity.
    /// </remarks>
    public const long MaxFileSizeBytes = 100L * 1024 * 1024; // 100 MB

    /// <summary>
    /// The minimum allowed file size in bytes (1 byte).
    /// </summary>
    /// <remarks>
    /// Prevents empty files from being uploaded to the system, which could
    /// indicate upload errors or malicious attempts.
    /// </remarks>
    public const long MinFileSizeBytes = 1L;

    /// <summary>
    /// The standard length for SHA256 checksums (64 hexadecimal characters).
    /// </summary>
    /// <remarks>
    /// SHA256 produces a 256-bit hash, which is represented as 64 hexadecimal characters.
    /// This is the standard format used throughout the ADMS system for file integrity verification.
    /// </remarks>
    public const int Sha256ChecksumLength = 64;

    /// <summary>
    /// Maximum number of alternative file name suggestions to generate.
    /// </summary>
    /// <remarks>
    /// Limits the number of suggestions to prevent excessive processing while
    /// providing sufficient alternatives for user selection.
    /// </remarks>
    public const int MaxFileNameSuggestions = 10;

    public const long LargeFileSizeThreshold = 1024 * 150;

    #endregion Constants

    #region Allowed Extensions and MIME Types

    /// <summary>
    /// The list of allowed file extensions for upload (all lowercase, with leading dot).
    /// </summary>
    /// <remarks>
    /// This list is specifically curated for legal document management, including
    /// common office documents, PDFs, images, and archive formats that lawyers
    /// and legal professionals commonly use.
    /// 
    /// <para><strong>Extension Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Microsoft Office:</strong> Word, Excel, PowerPoint formats (legacy and modern)</item>
    /// <item><strong>Adobe PDF:</strong> Portable Document Format for legal documents</item>
    /// <item><strong>Text Formats:</strong> Plain text, markdown, and log files</item>
    /// <item><strong>Image Formats:</strong> Common formats for evidence and diagrams</item>
    /// <item><strong>Audio/Video:</strong> Formats for depositions and recorded evidence</item>
    /// <item><strong>Archive Formats:</strong> ZIP, 7-Zip, and RAR for document collections</item>
    /// </list>
    /// 
    /// <para><strong>Maintenance Guidelines:</strong></para>
    /// When adding new extensions, ensure they are:
    /// <list type="number">
    /// <item>Lowercase with leading dot</item>
    /// <item>Commonly used in legal practice</item>
    /// <item>Have corresponding MIME type entries</item>
    /// <item>Are security-vetted and safe for storage</item>
    /// <item>Fit within the database extension length constraint (5 characters)</item>
    /// </list>
    /// </remarks>
    private static readonly string[] _allowedExtensionsArray =
    [
        // Microsoft Office Word
        ".doc", ".docx", ".dot", ".dotx", ".docm", ".dotm", ".rtf",
        
        // Microsoft Office Excel  
        ".xls", ".xlsx", ".xlt", ".xltx", ".xlsm", ".xltm", ".csv",
        
        // Microsoft Office PowerPoint
        ".ppt", ".pptx", ".pps", ".ppsx", ".pot", ".potx", ".pptm", ".potm", ".ppsm",
        
        // Microsoft Outlook
        ".msg", ".eml", ".pst", ".ost",
        
        // Adobe PDF
        ".pdf",
        
        // Text formats
        ".txt", ".md", ".log",
        
        // Image formats (for evidence, diagrams, etc.)
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff", ".webp",
        
        // Audio formats (for depositions, recordings)
        ".mp3", ".wav", ".wma", ".aac", ".ogg", ".flac", ".m4a",
        
        // Video formats (for depositions, evidence)
        ".mp4", ".avi", ".mov", ".wmv", ".mkv", ".flv", ".webm", ".mpeg", ".mpg",
        
        // Archive formats
        ".zip", ".7z", ".rar"
    ];

    /// <summary>
    /// The list of allowed MIME types for upload (all lowercase).
    /// </summary>
    /// <remarks>
    /// MIME types are validated to ensure file content matches the claimed file type,
    /// providing an additional security layer against file spoofing attacks.
    /// Each entry corresponds to one or more file extensions in the allowed extensions list.
    /// 
    /// <para><strong>MIME Type Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Office Documents:</strong> Microsoft Office application types</item>
    /// <item><strong>Text Types:</strong> Plain text, CSV, and markup formats</item>
    /// <item><strong>Media Types:</strong> Image, audio, and video formats</item>
    /// <item><strong>Archive Types:</strong> Compressed file formats</item>
    /// <item><strong>Email Types:</strong> Email message and container formats</item>
    /// </list>
    /// 
    /// <para><strong>Security Considerations:</strong></para>
    /// All MIME types in this list have been security-reviewed and are considered
    /// safe for document management systems when combined with virus scanning.
    /// </remarks>
    private static readonly string[] _allowedMimeTypesArray =
    [
        // Archive formats
        "application/zip",
        "application/x-7z-compressed",
        "application/vnd.rar",
        
        // Microsoft Office Word
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-word.document.macroenabled.12",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
        "application/vnd.ms-word.template.macroenabled.12",
        "application/rtf",
        
        // Microsoft Office Excel
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-excel.sheet.macroenabled.12",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.template",
        "application/vnd.ms-excel.template.macroenabled.12",
        "text/csv",
        "application/csv",
        
        // Microsoft Office PowerPoint
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "application/vnd.ms-powerpoint.presentation.macroenabled.12",
        "application/vnd.openxmlformats-officedocument.presentationml.template",
        "application/vnd.ms-powerpoint.template.macroenabled.12",
        "application/vnd.openxmlformats-officedocument.presentationml.slideshow",
        "application/vnd.ms-powerpoint.slideshow.macroenabled.12",
        
        // Microsoft Outlook
        "application/vnd.ms-outlook",
        "application/vnd.ms-outlook.pst",
        "application/vnd.ms-outlook.ost",
        "message/rfc822",
        
        // Adobe PDF
        "application/pdf",
        
        // Text formats
        "text/plain",
        "text/markdown",
        "text/x-log",
        "text/csv",
        
        // Image formats
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/bmp",
        "image/tiff",
        "image/webp",
        "image/svg+xml",
        
        // Audio formats
        "audio/mpeg",
        "audio/mp3",
        "audio/wav",
        "audio/x-wav",
        "audio/x-ms-wma",
        "audio/aac",
        "audio/ogg",
        "audio/flac",
        "audio/mp4",
        "audio/m4a",
        
        // Video formats
        "video/mp4",
        "video/x-msvideo",
        "video/quicktime",
        "video/x-ms-wmv",
        "video/x-matroska",
        "video/x-flv",
        "video/webm",
        "video/mpeg"
    ];

    /// <summary>
    /// List of reserved file names that cannot be used (Windows system reserved names).
    /// </summary>
    /// <remarks>
    /// These names are reserved by Windows and other operating systems and cannot be used
    /// as file names to prevent system conflicts and compatibility issues.
    /// 
    /// <para><strong>Reserved Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Device Names:</strong> CON, PRN, AUX, NUL</item>
    /// <item><strong>Serial Ports:</strong> COM1-COM9</item>
    /// <item><strong>Parallel Ports:</strong> LPT1-LPT9</item>
    /// <item><strong>System Files:</strong> Windows system file names</item>
    /// </list>
    /// 
    /// This list ensures cross-platform compatibility and prevents issues when
    /// files are accessed from different operating systems.
    /// </remarks>
    private static readonly string[] _reservedFileNamesArray =
    [
        // Windows device names
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
        
        // Additional system reserved names
        "CLOCK$", "CONFIG$", "KEYBD$", "SCREEN$", "$MFTMIRR", "$LOGFILE", "$VOLUME"
    ];

    /// <summary>
    /// High-performance frozen set of allowed extensions for O(1) lookup performance.
    /// </summary>
    /// <remarks>
    /// Uses FrozenSet for optimal read performance in validation scenarios.
    /// Thread-safe and immutable for concurrent access without locking.
    /// </remarks>
    private static readonly FrozenSet<string> _allowedExtensionsSet =
        _allowedExtensionsArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// High-performance frozen set of allowed MIME types for O(1) lookup performance.
    /// </summary>
    /// <remarks>
    /// Uses FrozenSet for optimal read performance in validation scenarios.
    /// Thread-safe and immutable for concurrent access without locking.
    /// </remarks>
    private static readonly FrozenSet<string> _allowedMimeTypesSet =
        _allowedMimeTypesArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// High-performance frozen set of reserved file names for O(1) lookup performance.
    /// </summary>
    /// <remarks>
    /// Uses FrozenSet for optimal read performance in validation scenarios.
    /// Case-insensitive comparison for cross-platform compatibility.
    /// </remarks>
    private static readonly FrozenSet<string> _reservedFileNamesSet =
        _reservedFileNamesArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the read-only list of allowed file extensions.
    /// All values are lowercase with leading dots.
    /// </summary>
    /// <remarks>
    /// Returns an immutable view of the allowed extensions for external consumption.
    /// This property provides thread-safe access to the extensions list.
    /// </remarks>
    public static IReadOnlyList<string> AllowedExtensions => _allowedExtensionsArray.ToImmutableArray();

    /// <summary>
    /// Gets the read-only list of allowed MIME types.
    /// All values are lowercase.
    /// </summary>
    /// <remarks>
    /// Returns an immutable view of the allowed MIME types for external consumption.
    /// This property provides thread-safe access to the MIME types list.
    /// </remarks>
    public static IReadOnlyList<string> AllowedMimeTypes => _allowedMimeTypesArray.ToImmutableArray();

    /// <summary>
    /// Gets the read-only list of reserved file names.
    /// All values are uppercase for consistency.
    /// </summary>
    /// <remarks>
    /// Returns an immutable view of the reserved file names for external consumption.
    /// This property provides thread-safe access to the reserved names list.
    /// </remarks>
    public static IReadOnlyList<string> ReservedFileNames => _reservedFileNamesArray.ToImmutableArray();

    #endregion Allowed Extensions and MIME Types

    #region Core Validation Methods

    /// <summary>
    /// Gets the list of allowed file extensions as a comma-separated string (without leading dots).
    /// </summary>
    /// <value>
    /// A comma-separated string of all allowed file extensions.
    /// </value>
    /// <remarks>
    /// This property is useful for generating error messages that list all valid options,
    /// helping users understand what file types are acceptable.
    /// Extensions are returned without leading dots for better readability.
    /// </remarks>
    /// <example>
    /// <code>
    /// string extensions = FileValidationHelper.AllowedExtensionsList;
    /// // Returns: "doc, docx, pdf, txt, jpg, mp4, zip, ..."
    /// </code>
    /// </example>
    public static string AllowedExtensionsList =>
        string.Join(", ", AllowedExtensions.Select(ext => ext.TrimStart('.')));

    /// <summary>
    /// Gets the list of allowed MIME types as a comma-separated string.
    /// </summary>
    /// <value>
    /// A comma-separated string of all allowed MIME types.
    /// </value>
    /// <remarks>
    /// This property provides a comprehensive list of supported MIME types for documentation
    /// and error message purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// string mimeTypes = FileValidationHelper.AllowedMimeTypesList;
    /// // Returns: "application/pdf, application/msword, image/jpeg, ..."
    /// </code>
    /// </example>
    public static string AllowedMimeTypesList => string.Join(", ", AllowedMimeTypes);

    /// <summary>
    /// Gets the list of reserved file names as a comma-separated string.
    /// </summary>
    /// <value>
    /// A comma-separated string of all reserved file names.
    /// </value>
    /// <remarks>
    /// This property provides a list of names that cannot be used as file names
    /// for error message and documentation purposes.
    /// </remarks>
    public static string ReservedFileNamesList => string.Join(", ", ReservedFileNames);

    /// <summary>
    /// Checks if the file extension is allowed.
    /// </summary>
    /// <param name="extension">The file extension to check (with or without leading dot).</param>
    /// <returns><c>true</c> if the extension is allowed; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method normalizes the extension format and performs a case-insensitive comparison
    /// against the allowed extensions list using high-performance frozen set lookup.
    /// 
    /// <para><strong>Normalization Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Null or whitespace extensions return false</item>
    /// <item>Extensions are converted to lowercase</item>
    /// <item>Leading dot is added if missing</item>
    /// <item>Trailing whitespace is trimmed</item>
    /// </list>
    /// 
    /// <para><strong>Performance:</strong></para>
    /// Uses FrozenSet for O(1) average lookup performance, optimized for high-frequency validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid1 = FileValidationHelper.IsExtensionAllowed(".pdf");     // true
    /// bool isValid2 = FileValidationHelper.IsExtensionAllowed("pdf");      // true (normalized)
    /// bool isValid3 = FileValidationHelper.IsExtensionAllowed(".PDF");     // true (case insensitive)
    /// bool isInvalid = FileValidationHelper.IsExtensionAllowed(".exe");    // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsExtensionAllowed([NotNullWhen(true)] string? extension)
    {
        var normalized = NormalizeExtension(extension);
        return normalized != null && _allowedExtensionsSet.Contains(normalized);
    }

    /// <summary>
    /// Checks if the MIME type is allowed.
    /// </summary>
    /// <param name="mimeType">The MIME type to check.</param>
    /// <returns><c>true</c> if the MIME type is allowed; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method performs a case-insensitive comparison against the allowed MIME types list
    /// and validates the basic format of the MIME type using high-performance frozen set lookup.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Null or whitespace MIME types return false</item>
    /// <item>MIME types are converted to lowercase</item>
    /// <item>Basic format validation (type/subtype)</item>
    /// <item>Must match exactly one of the predefined allowed MIME types</item>
    /// </list>
    /// 
    /// <para><strong>Performance:</strong></para>
    /// Uses FrozenSet for O(1) average lookup performance with format pre-validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid1 = FileValidationHelper.IsMimeTypeAllowed("application/pdf");           // true
    /// bool isValid2 = FileValidationHelper.IsMimeTypeAllowed("APPLICATION/PDF");           // true (case insensitive)
    /// bool isInvalid = FileValidationHelper.IsMimeTypeAllowed("application/x-executable"); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMimeTypeAllowed([NotNullWhen(true)] string? mimeType)
    {
        var normalized = NormalizeMimeType(mimeType);
        return normalized != null && _allowedMimeTypesSet.Contains(normalized);
    }

    /// <summary>
    /// Validates if the file name is valid (does not contain invalid characters and meets length requirements).
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <returns><c>true</c> if the file name is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method checks for invalid file name characters, length constraints, and ensures
    /// the file name is not empty or whitespace-only.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Must not be null, empty, or whitespace-only</item>
    /// <item>Must be between {MinFileNameLength} and {MaxFileNameLength} characters</item>
    /// <item>Must not contain invalid file name characters</item>
    /// <item>Must not contain problematic characters (&lt;, &gt;, :, ", |, ?, *)</item>
    /// </list>
    /// 
    /// <para><strong>Security Considerations:</strong></para>
    /// This validation helps prevent path traversal attacks and ensures file names
    /// are safe for storage across different file systems.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid1 = FileValidationHelper.IsFileNameValid("document.pdf");       // true
    /// bool isValid2 = FileValidationHelper.IsFileNameValid("My Document.docx");   // true
    /// bool isInvalid1 = FileValidationHelper.IsFileNameValid("doc|ument.pdf");    // false (invalid character)
    /// bool isInvalid2 = FileValidationHelper.IsFileNameValid("");                 // false (empty)
    /// </code>
    /// </example>
    public static bool IsFileNameValid([NotNullWhen(true)] string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var trimmed = fileName.Trim();

        // Check length constraints
        if (trimmed.Length is < MinFileNameLength or > MaxFileNameLength)
            return false;

        // Check for invalid file name characters
        if (trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            return false;

        // Additional checks for problematic characters that may not be in the system list
        return !ContainsProblematicCharacters(trimmed);
    }

    /// <summary>
    /// Checks if the provided file name (without path) is a reserved system name.
    /// </summary>
    /// <param name="fileName">The file name to check (with or without extension).</param>
    /// <returns><c>true</c> if the file name is reserved; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Reserved file names include Windows device names and system reserved names that
    /// cannot be used as regular file names due to operating system restrictions.
    /// 
    /// <para><strong>Validation Process:</strong></para>
    /// <list type="bullet">
    /// <item>Extracts base file name (without extension)</item>
    /// <item>Performs case-insensitive comparison with reserved names</item>
    /// <item>Uses high-performance frozen set lookup</item>
    /// </list>
    /// 
    /// <para><strong>Cross-Platform Compatibility:</strong></para>
    /// This check ensures files will be accessible across different operating systems
    /// and prevents conflicts with system device names.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isReserved1 = FileValidationHelper.IsReservedFileName("CON");        // true
    /// bool isReserved2 = FileValidationHelper.IsReservedFileName("CON.txt");    // true
    /// bool isReserved3 = FileValidationHelper.IsReservedFileName("COM1.pdf");   // true
    /// bool isNotReserved = FileValidationHelper.IsReservedFileName("document"); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReservedFileName([NotNullWhen(true)] string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var baseName = ExtractBaseFileName(fileName);
        return _reservedFileNamesSet.Contains(baseName);
    }

    /// <summary>
    /// Validates file size against configured limits.
    /// </summary>
    /// <param name="fileSize">The file size in bytes.</param>
    /// <returns><c>true</c> if the file size is within allowed limits; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method ensures file sizes are within reasonable limits to prevent system
    /// performance issues, storage concerns, and potential denial-of-service attacks.
    /// 
    /// <para><strong>Size Limits:</strong></para>
    /// <list type="bullet">
    /// <item>Minimum: {MinFileSizeBytes:N0} byte (prevents empty files)</item>
    /// <item>Maximum: {MaxFileSizeBytes:N0} bytes ({MaxFileSizeBytes / (1024 * 1024)} MB)</item>
    /// </list>
    /// 
    /// <para><strong>Security Considerations:</strong></para>
    /// Size limits help prevent resource exhaustion attacks and ensure the system
    /// remains responsive under normal operating conditions.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid1 = FileValidationHelper.IsFileSizeValid(1024);              // true (1 KB)
    /// bool isValid2 = FileValidationHelper.IsFileSizeValid(50 * 1024 * 1024);  // true (50 MB)
    /// bool isInvalid1 = FileValidationHelper.IsFileSizeValid(0);               // false (empty file)
    /// bool isInvalid2 = FileValidationHelper.IsFileSizeValid(200 * 1024 * 1024); // false (too large)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFileSizeValid(long fileSize)
    {
        return fileSize is >= MinFileSizeBytes and <= MaxFileSizeBytes;
    }

    /// <summary>
    /// Validates a SHA256 checksum format.
    /// </summary>
    /// <param name="checksum">The checksum to validate.</param>
    /// <returns><c>true</c> if the checksum format is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method validates that the checksum is exactly 64 hexadecimal characters,
    /// which is the standard format for SHA256 hash values used throughout the ADMS system.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Must be exactly {Sha256ChecksumLength} characters long</item>
    /// <item>Must contain only hexadecimal characters (0-9, A-F, a-f)</item>
    /// <item>Must not be null, empty, or whitespace-only</item>
    /// </list>
    /// 
    /// <para><strong>Performance:</strong></para>
    /// Uses compiled regex pattern for optimal validation performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid = FileValidationHelper.IsValidChecksum("a1b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef123456"); // true
    /// bool isInvalid1 = FileValidationHelper.IsValidChecksum("invalid");        // false (not hex)
    /// bool isInvalid2 = FileValidationHelper.IsValidChecksum("a1b2c3");         // false (too short)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidChecksum([NotNullWhen(true)] string? checksum)
    {
        if (string.IsNullOrWhiteSpace(checksum))
            return false;

        var trimmed = checksum.Trim();
        return trimmed.Length == Sha256ChecksumLength &&
               Sha256ChecksumRegex().IsMatch(trimmed);
    }

    #endregion Core Validation Methods

    #region File Content Validation

    /// <summary>
    /// Detects the MIME type and extension of a file based on its content (magic number).
    /// Optionally logs warnings or errors if detection fails.
    /// </summary>
    /// <param name="fileBytes">The file bytes to analyze.</param>
    /// <param name="detectedMimeType">The detected MIME type (output).</param>
    /// <param name="detectedExtension">The detected file extension (output, with leading dot).</param>
    /// <param name="logger">Optional logger for warnings/errors.</param>
    /// <returns><c>true</c> if the file type is recognized and allowed; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method uses file signature detection (magic numbers) to identify file types,
    /// providing security against file type spoofing attacks. It's more reliable than
    /// relying solely on file extensions or MIME types provided by clients.
    /// 
    /// <para><strong>Detection Process:</strong></para>
    /// <list type="number">
    /// <item>Validates input parameters</item>
    /// <item>Reads file signature (magic numbers) from beginning of file</item>
    /// <item>Matches against known file type signatures</item>
    /// <item>Performs special handling for complex formats (Office files)</item>
    /// <item>Validates detected type against allowed types</item>
    /// </list>
    /// 
    /// <para><strong>Security Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Prevents file type spoofing attacks</item>
    /// <item>Validates actual file content vs claimed type</item>
    /// <item>Detects potentially malicious files with misleading extensions</item>
    /// </list>
    /// 
    /// <para><strong>Supported File Types:</strong></para>
    /// The method can detect common legal document formats including PDF, Office documents,
    /// images, audio/video files, and archive formats through their unique signatures.
    /// </remarks>
    /// <example>
    /// <code>
    /// byte[] pdfBytes = File.ReadAllBytes("document.pdf");
    /// bool isValid = FileValidationHelper.IsValidFileType(pdfBytes, out string mimeType, out string extension);
    /// // mimeType: "application/pdf", extension: ".pdf", isValid: true
    /// </code>
    /// </example>
    public static bool IsValidFileType(
        byte[]? fileBytes,
        out string detectedMimeType,
        out string detectedExtension,
        ILogger? logger = null)
    {
        detectedMimeType = "application/octet-stream";
        detectedExtension = ".bin";

        // Validate input parameters
        if (fileBytes is null or { Length: 0 })
        {
            logger?.LogWarning("FileValidationHelper: File bytes are null or empty");
            return false;
        }

        try
        {
            // Find matching file signature
            var matchingSignature = GetFileSignatures().FirstOrDefault(sig =>
                fileBytes.Length >= sig.MagicBytes.Length &&
                fileBytes.AsSpan(0, sig.MagicBytes.Length).SequenceEqual(sig.MagicBytes));

            if (matchingSignature != null)
            {
                detectedMimeType = matchingSignature.MimeType;
                detectedExtension = matchingSignature.Extension;

                logger?.LogDebug("FileValidationHelper: Detected file type {MimeType} ({Extension}) by signature",
                    detectedMimeType, detectedExtension);

                // Special handling for Office files (ZIP-based formats)
                if (!IsOfficeFile(detectedExtension, fileBytes))
                    return IsExtensionAllowed(detectedExtension) && IsMimeTypeAllowed(detectedMimeType);
                var (actualMimeType, actualExtension) = DetectOfficeFileType(fileBytes, logger);
                if (actualMimeType == null || actualExtension == null)
                    return IsExtensionAllowed(detectedExtension) && IsMimeTypeAllowed(detectedMimeType);
                detectedMimeType = actualMimeType;
                detectedExtension = actualExtension;

                return IsExtensionAllowed(detectedExtension) && IsMimeTypeAllowed(detectedMimeType);
            }

            logger?.LogWarning("FileValidationHelper: Unknown file signature, first 16 bytes: {FirstBytes}",
                Convert.ToHexString(fileBytes.AsSpan(0, Math.Min(16, fileBytes.Length))));

            return false;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "FileValidationHelper: Exception during file type detection");
            return false;
        }
    }

    /// <summary>
    /// Validates file content integrity by comparing provided checksum with calculated checksum.
    /// </summary>
    /// <param name="fileBytes">The file bytes to validate.</param>
    /// <param name="providedChecksum">The checksum provided by the client.</param>
    /// <param name="logger">Optional logger for validation results.</param>
    /// <returns><c>true</c> if the checksums match; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method calculates the SHA256 hash of the file content and compares it with
    /// the provided checksum to ensure file integrity and detect tampering.
    /// 
    /// <para><strong>Validation Process:</strong></para>
    /// <list type="number">
    /// <item>Validates input parameters (file bytes and checksum format)</item>
    /// <item>Calculates SHA256 hash of file content</item>
    /// <item>Performs case-insensitive comparison with provided checksum</item>
    /// <item>Logs results for audit and debugging purposes</item>
    /// </list>
    /// 
    /// <para><strong>Security Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Detects file corruption during transmission</item>
    /// <item>Prevents tampering with file content</item>
    /// <item>Ensures data integrity throughout the system</item>
    /// <item>Provides audit trail for file validation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// byte[] fileBytes = File.ReadAllBytes("document.pdf");
    /// string expectedChecksum = "a1b2c3d4..."; // 64-character hex string
    /// bool isIntact = FileValidationHelper.ValidateFileIntegrity(fileBytes, expectedChecksum);
    /// </code>
    /// </example>
    public static bool ValidateFileIntegrity(byte[]? fileBytes, string? providedChecksum, ILogger? logger = null)
    {
        // Validate input parameters
        if (fileBytes is null or { Length: 0 })
        {
            logger?.LogWarning("FileValidationHelper: Cannot validate integrity of null or empty file");
            return false;
        }

        if (!IsValidChecksum(providedChecksum))
        {
            logger?.LogWarning("FileValidationHelper: Invalid checksum format provided");
            return false;
        }

        try
        {
            var calculatedChecksum = CalculateFileChecksum(fileBytes);
            var isMatch = string.Equals(calculatedChecksum, providedChecksum, StringComparison.OrdinalIgnoreCase);

            if (!isMatch)
            {
                logger?.LogWarning("FileValidationHelper: Checksum mismatch - provided: {ProvidedChecksum}, calculated: {CalculatedChecksum}",
                    providedChecksum, calculatedChecksum);
            }
            else
            {
                logger?.LogDebug("FileValidationHelper: File integrity validated successfully");
            }

            return isMatch;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "FileValidationHelper: Exception during file integrity validation");
            return false;
        }
    }

    /// <summary>
    /// Calculates the SHA256 checksum of file bytes.
    /// </summary>
    /// <param name="fileBytes">The file bytes to hash.</param>
    /// <returns>The SHA256 checksum as a lowercase hexadecimal string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileBytes"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fileBytes"/> is empty.</exception>
    /// <remarks>
    /// This method provides a secure hash calculation using the SHA256 algorithm,
    /// which is the standard for file integrity verification in the ADMS system.
    /// 
    /// <para><strong>Security Features:</strong></para>
    /// <list type="bullet">
    /// <item>Uses SHA256 algorithm for cryptographic strength</item>
    /// <item>Returns lowercase hexadecimal for consistency</item>
    /// <item>Validates input to prevent calculation errors</item>
    /// </list>
    /// 
    /// <para><strong>Performance:</strong></para>
    /// Uses the static SHA256.HashData method for optimal performance
    /// without creating unnecessary object instances.
    /// </remarks>
    /// <example>
    /// <code>
    /// byte[] fileBytes = File.ReadAllBytes("document.pdf");
    /// string checksum = FileValidationHelper.CalculateFileChecksum(fileBytes);
    /// // Returns: "a1b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef123456"
    /// </code>
    /// </example>
    public static string CalculateFileChecksum(byte[] fileBytes)
    {
        ArgumentNullException.ThrowIfNull(fileBytes);

        if (fileBytes.Length == 0)
            throw new ArgumentException("File bytes cannot be empty", nameof(fileBytes));

        var hash = SHA256.HashData(fileBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    #endregion File Content Validation

    #region Comprehensive Validation Methods

    /// <summary>
    /// Performs comprehensive validation of a file including name, extension, size, content, and integrity.
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <param name="extension">The file extension to validate.</param>
    /// <param name="fileSize">The file size in bytes.</param>
    /// <param name="mimeType">The MIME type to validate.</param>
    /// <param name="checksum">The file checksum to validate.</param>
    /// <param name="fileBytes">Optional file bytes for content validation.</param>
    /// <param name="logger">Optional logger for detailed validation results.</param>
    /// <returns>A <see cref="FileValidationResult"/> containing comprehensive validation details.</returns>
    /// <remarks>
    /// This method provides complete file validation suitable for document upload scenarios,
    /// combining all individual validation checks into a single comprehensive assessment.
    /// 
    /// <para><strong>Validation Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Basic Validation:</strong> File name, extension, size, MIME type format</item>
    /// <item><strong>Security Validation:</strong> Reserved names, allowed types, content validation</item>
    /// <item><strong>Integrity Validation:</strong> Checksum verification, size consistency</item>
    /// <item><strong>Content Validation:</strong> File type detection, spoofing protection</item>
    /// </list>
    /// 
    /// <para><strong>Result Details:</strong></para>
    /// Returns a comprehensive result object containing:
    /// <list type="bullet">
    /// <item>Overall validation status (IsValid)</item>
    /// <item>Primary error message for user display</item>
    /// <item>Detailed list of validation errors</item>
    /// <item>List of warnings that don't prevent validity</item>
    /// <item>Validated file metadata for storage</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// byte[] fileBytes = File.ReadAllBytes("document.pdf");
    /// var result = FileValidationHelper.ValidateFile(
    ///     "document.pdf", ".pdf", fileBytes.Length, "application/pdf", 
    ///     "a1b2c3...", fileBytes);
    /// 
    /// if (result.IsValid)
    /// {
    ///     Console.WriteLine("File validation passed");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Validation failed: {result.ErrorMessage}");
    /// }
    /// </code>
    /// </example>
    public static FileValidationResult ValidateFile(
        string? fileName,
        string? extension,
        long fileSize,
        string? mimeType,
        string? checksum,
        byte[]? fileBytes = null,
        ILogger? logger = null)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        logger?.LogDebug("FileValidationHelper: Starting comprehensive file validation for {FileName}",
            fileName ?? "unnamed file");

        // Validate file name
        ValidateFileName(fileName, errors);

        // Validate extension
        ValidateExtension(extension, errors);

        // Validate file size
        ValidateFileSize(fileSize, errors);

        // Validate MIME type
        ValidateMimeType(mimeType, errors);

        // Validate checksum format
        ValidateChecksumFormat(checksum, errors);

        // Validate file content if bytes provided
        if (fileBytes != null)
        {
            ValidateFileContent(fileBytes, extension, mimeType, checksum, fileSize, errors, warnings, logger);
        }

        // Determine overall validation result
        var isValid = errors.Count == 0;
        var errorMessage = isValid ? string.Empty : string.Join("; ", errors);

        logger?.LogDebug("FileValidationHelper: Validation completed - Valid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}",
            isValid, errors.Count, warnings.Count);

        return new FileValidationResult(
            isValid,
            errorMessage,
            errors.ToImmutableArray(),
            warnings.ToImmutableArray(),
            fileName,
            extension,
            fileSize,
            mimeType,
            checksum);
    }

    /// <summary>
    /// Validates file name according to all file name rules.
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages).</param>
    /// <returns>An enumerable of validation results.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    /// <remarks>
    /// This method performs comprehensive file name validation including:
    /// <list type="bullet">
    /// <item>Null/empty validation</item>
    /// <item>Length constraints validation</item>
    /// <item>Invalid character validation</item>
    /// <item>Reserved name validation</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateFileName(
        string? fileName,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            yield return new ValidationResult(
                $"{propertyName} is required and cannot be empty.",
                [propertyName]);
            yield break;
        }

        var trimmed = fileName.Trim();

        switch (trimmed.Length)
        {
            // Length validation
            case < MinFileNameLength:
                yield return new ValidationResult(
                    $"{propertyName} must be at least {MinFileNameLength} character(s) long.",
                    [propertyName]);
                break;
            case > MaxFileNameLength:
                yield return new ValidationResult(
                    $"{propertyName} cannot exceed {MaxFileNameLength} characters.",
                    [propertyName]);
                break;
        }

        // Character validation
        if (trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            yield return new ValidationResult(
                $"{propertyName} contains invalid file name characters.",
                [propertyName]);
        }

        if (ContainsProblematicCharacters(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} contains problematic characters that may cause issues.",
                [propertyName]);
        }

        // Reserved name validation
        if (IsReservedFileName(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} uses a reserved system name and cannot be used.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates file extension according to all extension rules.
    /// </summary>
    /// <param name="extension">The extension to validate.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages).</param>
    /// <returns>An enumerable of validation results.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateExtension(
        string? extension,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (string.IsNullOrWhiteSpace(extension))
        {
            yield return new ValidationResult(
                $"{propertyName} is required and cannot be empty.",
                [propertyName]);
            yield break;
        }

        var normalized = NormalizeExtension(extension);
        if (normalized == null)
        {
            yield return new ValidationResult(
                $"{propertyName} format is invalid.",
                [propertyName]);
            yield break;
        }

        // Length validation (excluding dot)
        var extensionWithoutDot = normalized.TrimStart('.');
        switch (extensionWithoutDot.Length)
        {
            case < MinExtensionLength:
                yield return new ValidationResult(
                    $"{propertyName} must be at least {MinExtensionLength} character(s) long.",
                    [propertyName]);
                break;
            case > MaxExtensionLength:
                yield return new ValidationResult(
                    $"{propertyName} cannot exceed {MaxExtensionLength} characters.",
                    [propertyName]);
                break;
        }

        // Allowed extension validation
        if (!IsExtensionAllowed(normalized))
        {
            yield return new ValidationResult(
                $"{propertyName} '{extension}' is not allowed. Allowed extensions: {AllowedExtensionsList}.",
                [propertyName]);
        }
    }

    #endregion Comprehensive Validation Methods

    #region Utility Methods

    /// <summary>
    /// Normalizes a file extension to standard format (lowercase with leading dot).
    /// </summary>
    /// <param name="extension">The extension to normalize.</param>
    /// <returns>The normalized extension, or null if invalid.</returns>
    /// <remarks>
    /// This method ensures consistent extension format for comparison and storage.
    /// 
    /// <para><strong>Normalization Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Trims leading and trailing whitespace</item>
    /// <item>Converts to lowercase</item>
    /// <item>Adds leading dot if missing</item>
    /// <item>Returns null for empty or invalid extensions</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// string? normalized1 = FileValidationHelper.NormalizeExtension("PDF");   // ".pdf"
    /// string? normalized2 = FileValidationHelper.NormalizeExtension(".DOC");  // ".doc"
    /// string? normalized3 = FileValidationHelper.NormalizeExtension("txt");   // ".txt"
    /// </code>
    /// </example>
    [return: NotNullIfNotNull(nameof(extension))]
    public static string? NormalizeExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return null;

        var trimmed = extension.Trim().ToLowerInvariant();

        // Ensure extension starts with a dot
        if (!trimmed.StartsWith('.'))
            trimmed = $".{trimmed}";

        // Validate the extension format (basic validation)
        return ExtensionFormatRegex().IsMatch(trimmed) ? trimmed : null;
    }

    /// <summary>
    /// Normalizes a MIME type to standard format (lowercase).
    /// </summary>
    /// <param name="mimeType">The MIME type to normalize.</param>
    /// <returns>The normalized MIME type, or null if invalid.</returns>
    /// <remarks>
    /// This method ensures consistent MIME type format for comparison and storage.
    /// 
    /// <para><strong>Normalization Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Trims leading and trailing whitespace</item>
    /// <item>Converts to lowercase</item>
    /// <item>Validates basic MIME type format (type/subtype)</item>
    /// <item>Returns null for empty or invalid MIME types</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// string? normalized1 = FileValidationHelper.NormalizeMimeType("APPLICATION/PDF");  // "application/pdf"
    /// string? normalized2 = FileValidationHelper.NormalizeMimeType("text/PLAIN");       // "text/plain"
    /// </code>
    /// </example>
    [return: NotNullIfNotNull(nameof(mimeType))]
    public static string? NormalizeMimeType(string? mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            return null;

        var trimmed = mimeType.Trim().ToLowerInvariant();
        return MimeTypeFormatRegex().IsMatch(trimmed) ? trimmed : null;
    }

    /// <summary>
    /// Extracts the base file name without extension.
    /// </summary>
    /// <param name="fileName">The full file name.</param>
    /// <returns>The base name without extension.</returns>
    /// <remarks>
    /// This method is used for reserved name checking and file name analysis.
    /// Handles edge cases like files with no extension or multiple dots.
    /// </remarks>
    /// <example>
    /// <code>
    /// string baseName1 = FileValidationHelper.ExtractBaseFileName("document.pdf");  // "document"
    /// string baseName2 = FileValidationHelper.ExtractBaseFileName("CON.txt");       // "CON"
    /// string baseName3 = FileValidationHelper.ExtractBaseFileName("README");        // "README"
    /// </code>
    /// </example>
    public static string ExtractBaseFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        var trimmed = fileName.Trim();
        var lastDotIndex = trimmed.LastIndexOf('.');

        // If no dot found or dot is at the beginning, return the whole name
        return lastDotIndex <= 0 ? trimmed : trimmed[..lastDotIndex];
    }

    /// <summary>
    /// Suggests alternative file names if the provided name is not valid.
    /// </summary>
    /// <param name="attemptedFileName">The attempted file name.</param>
    /// <param name="maxSuggestions">Maximum number of suggestions to return.</param>
    /// <returns>A list of suggested alternative file names.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxSuggestions"/> is less than 1.</exception>
    /// <remarks>
    /// This method provides user-friendly suggestions when a file name validation fails,
    /// helping users understand how to correct naming issues.
    /// 
    /// <para><strong>Suggestion Strategies:</strong></para>
    /// <list type="bullet">
    /// <item>Clean invalid characters from the original name</item>
    /// <item>Add prefixes/suffixes to avoid reserved names</item>
    /// <item>Generate timestamp-based alternatives</item>
    /// <item>Create GUID-based unique names as fallback</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var suggestions = FileValidationHelper.SuggestAlternativeFileNames("CON.pdf", 3);
    /// // Returns: ["CON_File.pdf", "Legal_CON.pdf", "Doc_CON.pdf"]
    /// </code>
    /// </example>
    public static IReadOnlyList<string> SuggestAlternativeFileNames(
        string? attemptedFileName,
        int maxSuggestions = MaxFileNameSuggestions)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxSuggestions, 1);

        var suggestions = new List<string>();

        if (string.IsNullOrWhiteSpace(attemptedFileName))
        {
            suggestions.AddRange([
                "Document",
                "File",
                "Legal_Document",
                "New_File",
                "Untitled"
            ]);
            return suggestions.Take(maxSuggestions).ToImmutableArray();
        }

        var baseName = ExtractBaseFileName(attemptedFileName);
        var extension = Path.GetExtension(attemptedFileName);

        // Clean up the base name by removing invalid characters
        var cleanBaseName = CleanFileName(baseName);
        if (string.IsNullOrEmpty(cleanBaseName))
            cleanBaseName = "Document";

        // Generate different types of suggestions
        GenerateReservedNameSuggestions(baseName, cleanBaseName, extension, suggestions);
        GenerateGenericSuggestions(cleanBaseName, extension, suggestions);
        GenerateTimestampSuggestions(cleanBaseName, extension, suggestions);
        GenerateUniqueSuggestions(cleanBaseName, extension, suggestions);

        return suggestions
            .Distinct()
            .Take(maxSuggestions)
            .ToImmutableArray();
    }

    #endregion Utility Methods

    #region Private Helper Methods

    /// <summary>
    /// Validates file name and adds errors to the error list.
    /// </summary>
    private static void ValidateFileName(string? fileName, List<string> errors)
    {
        if (!IsFileNameValid(fileName))
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                errors.Add("File name is required and cannot be empty.");
            }
            else
            {
                var trimmed = fileName.Trim();
                switch (trimmed.Length)
                {
                    case > MaxFileNameLength:
                        errors.Add($"File name cannot exceed {MaxFileNameLength} characters.");
                        break;
                    case < MinFileNameLength:
                        errors.Add($"File name must be at least {MinFileNameLength} character long.");
                        break;
                    default:
                        errors.Add("File name contains invalid characters.");
                        break;
                }
            }
        }

        if (IsReservedFileName(fileName))
        {
            errors.Add($"File name '{fileName}' is reserved and cannot be used.");
        }
    }

    /// <summary>
    /// Validates extension and adds errors to the error list.
    /// </summary>
    private static void ValidateExtension(string? extension, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            errors.Add("File extension is required.");
            return;
        }

        var normalized = NormalizeExtension(extension);
        if (normalized == null)
        {
            errors.Add("File extension format is invalid.");
            return;
        }

        var extensionWithoutDot = normalized.TrimStart('.');
        if (extensionWithoutDot.Length > MaxExtensionLength)
        {
            errors.Add($"File extension cannot exceed {MaxExtensionLength} characters.");
        }

        if (!IsExtensionAllowed(extension))
        {
            errors.Add($"File extension '{extension}' is not allowed. Allowed extensions: {AllowedExtensionsList}");
        }
    }

    /// <summary>
    /// Validates file size against configured limits.
    /// </summary>
    private static void ValidateFileSize(long fileSize, List<string> errors)
    {
        if (IsFileSizeValid(fileSize)) return;
        switch (fileSize)
        {
            case <= 0:
                errors.Add("File size must be greater than 0 bytes.");
                break;
            case > MaxFileSizeBytes:
                errors.Add($"File size {FormatFileSize(fileSize)} exceeds the maximum allowed size of {FormatFileSize(MaxFileSizeBytes)}.");
                break;
            default:
                errors.Add($"File size {fileSize:N0} bytes is invalid. Must be between {MinFileSizeBytes:N0} and {MaxFileSizeBytes:N0} bytes.");
                break;
        }
    }

    /// <summary>
    /// Validates MIME type and adds errors to the error list.
    /// </summary>
    private static void ValidateMimeType(string? mimeType, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
        {
            errors.Add("MIME type is required.");
            return;
        }

        var normalized = NormalizeMimeType(mimeType);
        if (normalized == null)
        {
            errors.Add("MIME type format is invalid.");
            return;
        }

        if (!IsMimeTypeAllowed(mimeType))
        {
            errors.Add($"MIME type '{mimeType}' is not allowed.");
        }
    }

    /// <summary>
    /// Validates checksum format and adds errors to the error list.
    /// </summary>
    private static void ValidateChecksumFormat(string? checksum, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(checksum))
        {
            errors.Add("Checksum is required.");
            return;
        }

        if (IsValidChecksum(checksum)) return;
        var trimmed = checksum.Trim();
        errors.Add(trimmed.Length != Sha256ChecksumLength
            ? $"Checksum must be exactly {Sha256ChecksumLength} characters long. Provided: {trimmed.Length} characters."
            : "Checksum must contain only hexadecimal characters (0-9, A-F).");
    }

    /// <summary>
    /// Validates file content including type detection and integrity checks.
    /// </summary>
    private static void ValidateFileContent(
        byte[] fileBytes,
        string? extension,
        string? mimeType,
        string? checksum,
        long fileSize,
        List<string> errors,
        List<string> warnings,
        ILogger? logger)
    {
        // Validate file type detection
        if (!IsValidFileType(fileBytes, out var detectedMimeType, out var detectedExtension, logger))
        {
            errors.Add($"File content validation failed. Detected type: {detectedMimeType} ({detectedExtension})");
        }
        else
        {
            // Check for type mismatches (warnings only)
            var normalizedClaimedMime = NormalizeMimeType(mimeType);
            var normalizedDetectedMime = NormalizeMimeType(detectedMimeType);
            var normalizedClaimedExt = NormalizeExtension(extension);
            var normalizedDetectedExt = NormalizeExtension(detectedExtension);

            if (!string.Equals(normalizedClaimedMime, normalizedDetectedMime, StringComparison.Ordinal))
            {
                warnings.Add($"MIME type mismatch. Claimed: {mimeType}, Detected: {detectedMimeType}");
            }

            if (!string.Equals(normalizedClaimedExt, normalizedDetectedExt, StringComparison.Ordinal))
            {
                warnings.Add($"File extension mismatch. Claimed: {extension}, Detected: {detectedExtension}");
            }
        }

        // Validate file integrity
        if (IsValidChecksum(checksum) && !ValidateFileIntegrity(fileBytes, checksum, logger))
        {
            errors.Add("File integrity validation failed. Checksum does not match file content.");
        }

        // Validate file size consistency
        if (fileBytes.Length != fileSize)
        {
            errors.Add($"File size mismatch. Claimed: {FormatFileSize(fileSize)}, Actual: {FormatFileSize(fileBytes.Length)}");
        }
    }

    /// <summary>
    /// Cleans invalid characters from a file name.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string CleanFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        var invalidChars = Path.GetInvalidFileNameChars();
        ReadOnlySpan<char> problematicChars = ['<', '>', ':', '"', '|', '?', '*'];

        var result = new StringBuilder(fileName.Length);

        foreach (var c in fileName.AsSpan())
        {
            if (!invalidChars.Contains(c) && !problematicChars.Contains(c))
            {
                result.Append(c);
            }
            else
            {
                result.Append('_');
            }
        }

        return result.ToString().Trim();
    }

    /// <summary>
    /// Generates suggestions for reserved file names.
    /// </summary>
    private static void GenerateReservedNameSuggestions(
        string baseName,
        string cleanBaseName,
        string extension,
        List<string> suggestions)
    {
        if (IsReservedFileName(baseName))
        {
            suggestions.AddRange([
                $"{cleanBaseName}_File{extension}",
                $"Legal_{cleanBaseName}{extension}",
                $"Doc_{cleanBaseName}{extension}",
                $"{cleanBaseName}_Document{extension}"
            ]);
        }
    }

    /// <summary>
    /// Generates generic file name suggestions.
    /// </summary>
    private static void GenerateGenericSuggestions(string cleanBaseName, string extension, List<string> suggestions)
    {
        suggestions.AddRange([
            $"{cleanBaseName}_Copy{extension}",
            $"{cleanBaseName}_1{extension}",
            $"New_{cleanBaseName}{extension}",
            $"{cleanBaseName}_Modified{extension}"
        ]);
    }

    /// <summary>
    /// Generates timestamp-based file name suggestions.
    /// </summary>
    private static void GenerateTimestampSuggestions(string cleanBaseName, string extension, List<string> suggestions)
    {
        var now = DateTime.Now;
        suggestions.AddRange([
            $"{cleanBaseName}_{now:yyyyMMdd}{extension}",
            $"{cleanBaseName}_{now:yyyyMMdd_HHmm}{extension}",
            $"Document_{now:yyyyMMdd}{extension}"
        ]);
    }

    /// <summary>
    /// Generates unique file name suggestions using GUIDs.
    /// </summary>
    private static void GenerateUniqueSuggestions(string cleanBaseName, string extension, List<string> suggestions)
    {
        var shortGuid = Guid.NewGuid().ToString("N")[..8];
        suggestions.AddRange([
            $"{cleanBaseName}_{shortGuid}{extension}",
            $"File_{shortGuid}{extension}",
            $"Document_{shortGuid}{extension}"
        ]);
    }

    /// <summary>
    /// Formats file size in human-readable format with appropriate units.
    /// </summary>
    /// <param name="bytes">The size in bytes.</param>
    /// <returns>A human-readable string representation of the file size.</returns>
    /// <remarks>
    /// Converts byte values to appropriate units (B, KB, MB, GB, TB) for better readability
    /// in user interfaces and error messages.
    /// 
    /// <para><strong>Unit Conversion:</strong></para>
    /// <list type="bullet">
    /// <item>Bytes: Displayed as whole numbers (e.g., "500 B")</item>
    /// <item>Kilobytes and above: Displayed with 2 decimal places (e.g., "1.46 MB")</item>
    /// <item>Uses binary units (1024 bytes = 1 KB)</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// string size1 = FileValidationHelper.FormatFileSize(1024);      // "1.00 KB"
    /// string size2 = FileValidationHelper.FormatFileSize(1536000);   // "1.46 MB"
    /// string size3 = FileValidationHelper.FormatFileSize(500);       // "500 B"
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FormatFileSize(long bytes)
    {
        if (bytes < 0)
            return "0 B";

        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double size = bytes;
        var unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return unitIndex == 0
            ? $"{size:F0} {units[unitIndex]}"
            : $"{size:F2} {units[unitIndex]}";
    }

    /// <summary>
    /// Checks if a string contains problematic characters that may cause file system issues.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns><c>true</c> if the string contains problematic characters; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method checks for characters that, while not necessarily invalid for file names,
    /// can cause issues across different file systems or applications.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ContainsProblematicCharacters(string input)
    {
        ReadOnlySpan<char> problematicChars = ['<', '>', ':', '"', '|', '?', '*'];
        return input.AsSpan().IndexOfAny(problematicChars) >= 0;
    }

    /// <summary>
    /// Gets an immutable list of file signatures used for file type detection.
    /// </summary>
    /// <returns>An immutable collection of file signature definitions.</returns>
    /// <remarks>
    /// File signatures (magic numbers) are unique byte sequences at the beginning of files
    /// that identify the file format. This method provides signatures for common legal document
    /// formats including Office documents, PDFs, images, and archive formats.
    /// 
    /// <para><strong>Supported File Types:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Documents:</strong> PDF, Microsoft Office formats</item>
    /// <item><strong>Images:</strong> JPEG, PNG, GIF, BMP, TIFF</item>
    /// <item><strong>Audio:</strong> MP3, WAV</item>
    /// <item><strong>Video:</strong> MP4</item>
    /// <item><strong>Archives:</strong> ZIP, RAR, 7-Zip</item>
    /// </list>
    /// </remarks>
    private static ImmutableArray<FileSignature> GetFileSignatures()
    {
        return
        [
            // Adobe PDF
            new("%PDF"u8.ToArray(), "application/pdf", ".pdf"),
            
            // JPEG images
            new([0xFF, 0xD8, 0xFF], "image/jpeg", ".jpg"),
            
            // PNG images
            new([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], "image/png", ".png"),
            
            // GIF images
            new("GIF87a"u8.ToArray(), "image/gif", ".gif"),
            new("GIF89a"u8.ToArray(), "image/gif", ".gif"),
            
            // BMP images
            new("BM"u8.ToArray(), "image/bmp", ".bmp"),
            
            // TIFF images
            new("II*\0"u8.ToArray(), "image/tiff", ".tif"),
            new("MM\0*"u8.ToArray(), "image/tiff", ".tif"),
            
            // ZIP files (also used by Office formats)
            new([0x50, 0x4B, 0x03, 0x04], "application/zip", ".zip"),
            new([0x50, 0x4B, 0x05, 0x06], "application/zip", ".zip"),
            new([0x50, 0x4B, 0x07, 0x08], "application/zip", ".zip"),
            
            // Microsoft Office legacy formats (OLE Compound Document)
            new([0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1], "application/vnd.ms-office", ".ole"),
            
            // RAR archives
            new("Rar!\x1A\x07\0"u8.ToArray(), "application/vnd.rar", ".rar"),
            
            // 7-Zip archives
            new("7z\xBC\xAF\x27\x1C"u8.ToArray(), "application/x-7z-compressed", ".7z"),
            
            // MP3 audio
            new("ID3"u8.ToArray(), "audio/mpeg", ".mp3"),
            new([0xFF, 0xFB], "audio/mpeg", ".mp3"),
            
            // WAV audio
            new("RIFF"u8.ToArray(), "audio/wav", ".wav"),
            
            // MP4 video/audio
            new([0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70], "video/mp4", ".mp4"),
            new([0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70], "video/mp4", ".mp4"),
            
            // Plain text (UTF-8 BOM)
            new([0xEF, 0xBB, 0xBF], "text/plain", ".txt")
        ];
    }

    /// <summary>
    /// Determines if a file is a ZIP-based Office file that requires special detection.
    /// </summary>
    /// <param name="extension">The detected extension.</param>
    /// <param name="fileBytes">The file bytes.</param>
    /// <returns><c>true</c> if the file is an Office file requiring special handling; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Modern Microsoft Office files (.docx, .xlsx, .pptx) are ZIP archives containing XML files.
    /// This method identifies such files for specialized processing to determine the specific Office format.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsOfficeFile(string extension, byte[] fileBytes)
    {
        return extension == ".zip" &&
               fileBytes.Length >= 4 &&
               fileBytes.AsSpan(0, 4).SequenceEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 });
    }

    /// <summary>
    /// Detects specific Office file type from ZIP-based Office documents.
    /// </summary>
    /// <param name="fileBytes">The file bytes to analyze.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <returns>A tuple containing the detected MIME type and extension, or null values if detection fails.</returns>
    /// <remarks>
    /// This method analyzes ZIP-based Office files to determine their specific format.
    /// In a production implementation, this would parse the ZIP structure to examine
    /// the [Content_Types].xml file or specific directory structures.
    /// 
    /// <para><strong>Current Implementation:</strong></para>
    /// This is a simplified implementation that defaults to Word document format.
    /// A complete implementation would:
    /// <list type="bullet">
    /// <item>Parse the ZIP archive structure</item>
    /// <item>Examine [Content_Types].xml for format identification</item>
    /// <item>Check for specific directories (word/, xl/, ppt/)</item>
    /// <item>Validate XML schema references</item>
    /// </list>
    /// </remarks>
    private static (string? mimeType, string? extension) DetectOfficeFileType(byte[] fileBytes, ILogger? logger = null)
    {
        try
        {
            // TODO: Implement comprehensive Office file type detection
            // This should parse the ZIP structure and examine content types
            // For now, we default to Word document format as the most common

            logger?.LogDebug("FileValidationHelper: Office file detected, defaulting to Word document format");

            return ("application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx");
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "FileValidationHelper: Error detecting Office file type");
            return (null, null);
        }
    }

    #endregion Private Helper Methods

    #region Compiled Regex Patterns

    /// <summary>
    /// Compiled regex for validating SHA256 checksums (64 hexadecimal characters).
    /// </summary>
    /// <remarks>
    /// This regex pattern validates that a string contains exactly 64 hexadecimal characters,
    /// which is the standard format for SHA256 hash values.
    /// 
    /// <para><strong>Pattern Details:</strong></para>
    /// <list type="bullet">
    /// <item>^ - Start of string</item>
    /// <item>[A-Fa-f0-9]{64} - Exactly 64 hexadecimal characters</item>
    /// <item>$ - End of string</item>
    /// </list>
    /// 
    /// The pattern is compiled using the GeneratedRegex attribute for optimal performance.
    /// </remarks>
    [GeneratedRegex(@"^[A-Fa-f0-9]{64}$", RegexOptions.Compiled)]
    private static partial Regex Sha256ChecksumRegex();

    /// <summary>
    /// Compiled regex for validating MIME type format.
    /// </summary>
    /// <remarks>
    /// This regex pattern validates the basic structure of MIME types (type/subtype).
    /// 
    /// <para><strong>Pattern Details:</strong></para>
    /// <list type="bullet">
    /// <item>^ - Start of string</item>
    /// <item>[\w\.\-]+ - One or more word characters, dots, or hyphens (main type)</item>
    /// <item>\/ - Forward slash separator</item>
    /// <item>[\w\.\-\+]+ - One or more word characters, dots, hyphens, or plus signs (subtype)</item>
    /// <item>$ - End of string</item>
    /// </list>
    /// 
    /// The pattern is compiled using the GeneratedRegex attribute for optimal performance.
    /// </remarks>
    [GeneratedRegex(@"^[\w\.\-]+\/[\w\.\-\+]+$", RegexOptions.Compiled)]
    private static partial Regex MimeTypeFormatRegex();

    /// <summary>
    /// Compiled regex for validating file extension format.
    /// </summary>
    /// <remarks>
    /// This regex pattern validates file extension format to ensure they contain only
    /// alphanumeric characters after the dot.
    /// 
    /// <para><strong>Pattern Details:</strong></para>
    /// <list type="bullet">
    /// <item>^ - Start of string</item>
    /// <item>\. - Literal dot character</item>
    /// <item>[a-zA-Z0-9]{1,10} - 1 to 10 alphanumeric characters</item>
    /// <item>$ - End of string</item>
    /// </list>
    /// 
    /// The pattern is compiled using the GeneratedRegex attribute for optimal performance.
    /// </remarks>
    [GeneratedRegex(@"^\.([a-zA-Z0-9]{1,10})$", RegexOptions.Compiled)]
    private static partial Regex ExtensionFormatRegex();

    #endregion Compiled Regex Patterns

    #region File Signature Class

    /// <summary>
    /// Represents a file signature (magic number) for file type detection.
    /// </summary>
    /// <param name="magicBytes">The magic bytes that identify the file type.</param>
    /// <param name="mimeType">The MIME type associated with this signature.</param>
    /// <param name="extension">The file extension associated with this signature.</param>
    /// <remarks>
    /// File signatures are unique byte sequences found at the beginning of files that
    /// identify their format. This record provides a type-safe way to define and
    /// work with file signatures for content validation.
    /// 
    /// <para><strong>Usage:</strong></para>
    /// File signatures are used to detect file types based on actual content rather than
    /// relying on file extensions, which can be easily spoofed or incorrect.
    /// </remarks>
    private sealed record FileSignature(
        byte[] MagicBytes,
        string MimeType,
        string Extension)
    {
        /// <summary>
        /// Gets the magic bytes that identify this file type.
        /// </summary>
        public byte[] MagicBytes { get; init; } = MagicBytes ?? throw new ArgumentNullException(nameof(MagicBytes));

        /// <summary>
        /// Gets the MIME type associated with this file signature.
        /// </summary>
        public string MimeType { get; init; } = MimeType ?? throw new ArgumentNullException(nameof(MimeType));

        /// <summary>
        /// Gets the file extension associated with this file signature.
        /// </summary>
        public string Extension { get; init; } = Extension ?? throw new ArgumentNullException(nameof(Extension));

        /// <summary>
        /// Determines whether the specified byte array starts with this file signature.
        /// </summary>
        /// <param name="fileBytes">The file bytes to check.</param>
        /// <returns><c>true</c> if the file bytes start with this signature; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Matches(ReadOnlySpan<byte> fileBytes)
        {
            return fileBytes.Length >= MagicBytes.Length &&
                   fileBytes[..MagicBytes.Length].SequenceEqual(MagicBytes);
        }
    }

    #endregion File Signature Class
}

#region Supporting Types

/// <summary>
/// Represents the result of comprehensive file validation operations.
/// </summary>
/// <param name="IsValid">Whether the file passed all validation checks.</param>
/// <param name="ErrorMessage">The primary error message if validation failed.</param>
/// <param name="ValidationErrors">Detailed list of validation errors.</param>
/// <param name="Warnings">List of validation warnings that don't prevent validity.</param>
/// <param name="FileName">The validated file name.</param>
/// <param name="Extension">The validated file extension.</param>
/// <param name="FileSize">The validated file size in bytes.</param>
/// <param name="MimeType">The validated MIME type.</param>
/// <param name="Checksum">The validated checksum.</param>
/// <remarks>
/// This record provides a comprehensive result object for file validation operations,
/// including both success/failure status and detailed diagnostic information.
/// 
/// <para><strong>Usage Patterns:</strong></para>
/// <list type="bullet">
/// <item><strong>Success Check:</strong> Use IsValid property for basic validation status</item>
/// <item><strong>User Display:</strong> Use ErrorMessage for primary error display</item>
/// <item><strong>Detailed Analysis:</strong> Use ValidationErrors for comprehensive error listing</item>
/// <item><strong>User Feedback:</strong> Use Warnings for non-critical issues</item>
/// <item><strong>Storage:</strong> Use validated properties for safe data storage</item>
/// </list>
/// 
/// <para><strong>Immutability:</strong></para>
/// This record is immutable, ensuring thread safety and preventing accidental modification
/// of validation results after creation.
/// </remarks>
public sealed record FileValidationResult(
    bool IsValid,
    string ErrorMessage,
    IReadOnlyList<string> ValidationErrors,
    IReadOnlyList<string> Warnings,
    string? FileName = null,
    string? Extension = null,
    long? FileSize = null,
    string? MimeType = null,
    string? Checksum = null)
{
    /// <summary>
    /// Gets a value indicating whether the validation passed without any errors.
    /// </summary>
    public bool IsValid { get; init; } = IsValid;

    /// <summary>
    /// Gets the primary error message suitable for user display.
    /// </summary>
    /// <remarks>
    /// This message provides a concise summary of validation failures.
    /// For detailed error information, use the ValidationErrors collection.
    /// </remarks>
    public string ErrorMessage { get; init; } = ErrorMessage ?? string.Empty;

    /// <summary>
    /// Gets the detailed list of validation errors.
    /// </summary>
    /// <remarks>
    /// This collection contains all validation errors discovered during the validation process.
    /// Each error provides specific information about what validation rule was violated.
    /// </remarks>
    public IReadOnlyList<string> ValidationErrors { get; init; } = ValidationErrors ?? [];

    /// <summary>
    /// Gets the list of validation warnings that don't prevent validity.
    /// </summary>
    /// <remarks>
    /// Warnings indicate potential issues that don't make the file invalid but may
    /// require attention, such as type mismatches or unusual characteristics.
    /// </remarks>
    public IReadOnlyList<string> Warnings { get; init; } = Warnings ?? [];

    /// <summary>
    /// Gets the count of validation errors.
    /// </summary>
    public int ErrorCount => ValidationErrors.Count;

    /// <summary>
    /// Gets the count of validation warnings.
    /// </summary>
    public int WarningCount => Warnings.Count;

    /// <summary>
    /// Gets a value indicating whether the validation result has any warnings.
    /// </summary>
    public bool HasWarnings => WarningCount > 0;

    /// <summary>
    /// Returns a string representation of the validation result.
    /// </summary>
    /// <returns>A string summarizing the validation result.</returns>
    public override string ToString()
    {
        if (IsValid)
        {
            return HasWarnings
                ? $"Valid (with {WarningCount} warning{(WarningCount == 1 ? "" : "s")})"
                : "Valid";
        }

        return $"Invalid ({ErrorCount} error{(ErrorCount == 1 ? "" : "s")}{(HasWarnings ? $", {WarningCount} warning{(WarningCount == 1 ? "" : "s")}" : "")})";
    }
}

#endregion Supporting Types