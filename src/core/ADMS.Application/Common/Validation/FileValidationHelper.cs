using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ADMS.Application.Common.Validation;

/// <summary>
/// Provides comprehensive validation helper methods for file-related data within the ADMS document management solution.
/// </summary>
/// <remarks>
/// This validation helper follows Clean Architecture principles and provides optimized validation for File/Document entities and DTOs:
/// <list type="bullet">
/// <item><strong>File Name Validation:</strong> Ensures professional file naming conventions and system compatibility</item>
/// <item><strong>File Extension Validation:</strong> Validates file types and security constraints</item>
/// <item><strong>File Size Validation:</strong> Enforces storage limits and professional standards</item>
/// <item><strong>MIME Type Validation:</strong> Ensures content type consistency and security</item>
/// <item><strong>Checksum Validation:</strong> Validates file integrity and security hashes</item>
/// </list>
/// 
/// <para><strong>File Context in ADMS:</strong></para>
/// Files in this system represent digital documents with associated metadata, supporting:
/// <list type="bullet">
/// <item>Legal document storage with professional naming</item>
/// <item>File integrity verification through checksums</item>
/// <item>MIME type consistency for security and processing</item>
/// <item>Cross-platform file system compatibility</item>
/// </list>
/// </remarks>
public static partial class FileValidationHelper
{
    #region Core Constants

    /// <summary>
    /// Maximum allowed length for a file name (excluding extension).
    /// </summary>
    public const int MaxFileNameLength = 128;

    /// <summary>
    /// Minimum allowed length for a file name.
    /// </summary>
    public const int MinFileNameLength = 1;

    /// <summary>
    /// Maximum allowed length for a file extension (including dot).
    /// </summary>
    public const int MaxExtensionLength = 10;

    /// <summary>
    /// Minimum allowed length for a file extension (including dot).
    /// </summary>
    public const int MinExtensionLength = 2; // At least ".x"

    /// <summary>
    /// Maximum allowed file size in bytes (100 MB).
    /// </summary>
    public const long MaxFileSizeBytes = 100 * 1024 * 1024;

    /// <summary>
    /// Minimum allowed file size in bytes (1 byte - empty files not allowed).
    /// </summary>
    public const long MinFileSizeBytes = 1;

    /// <summary>
    /// Maximum allowed length for MIME type strings.
    /// </summary>
    public const int MaxMimeTypeLength = 128;

    /// <summary>
    /// Maximum allowed length for checksum strings (SHA512 hex = 128 chars).
    /// </summary>
    public const int MaxChecksumLength = 128;

    #endregion Core Constants

    #region Allowed Extensions and MIME Types

    /// <summary>
    /// Allowed file extensions for legal document management (optimized with FrozenSet).
    /// </summary>
    private static readonly string[] _allowedExtensionsArray =
    [
        // Primary legal document formats
        ".pdf",   // Portable Document Format - preferred
        ".docx",  // Microsoft Word Document
        ".doc",   // Microsoft Word 97-2003

        // Text formats
        ".txt",   // Plain text
        ".rtf",   // Rich Text Format

        // Spreadsheet formats (for legal schedules, billing, etc.)
        ".xlsx",  // Microsoft Excel
        ".xls",   // Microsoft Excel 97-2003
        ".csv",   // Comma-separated values

        // Image formats (for legal evidence, exhibits)
        ".jpg", ".jpeg", // JPEG images
        ".png",   // PNG images
        ".tiff", ".tif", // TIFF images
        ".bmp",   // Bitmap images
        ".gif",   // GIF images

        // Archive formats (for document bundles)
        ".zip",   // ZIP archives
        ".rar"    // RAR archives (read-only support)
    ];

    /// <summary>
    /// High-performance frozen set for allowed extension lookups.
    /// </summary>
    private static readonly FrozenSet<string> _allowedExtensionsSet =
        _allowedExtensionsArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the immutable list of allowed extensions.
    /// </summary>
    public static IReadOnlyList<string> AllowedExtensions => _allowedExtensionsArray;

    /// <summary>
    /// Mapping of file extensions to their expected MIME types.
    /// </summary>
    private static readonly Dictionary<string, string[]> _extensionToMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = ["application/pdf"],
        [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
        [".doc"] = ["application/msword"],
        [".txt"] = ["text/plain"],
        [".rtf"] = ["application/rtf", "text/rtf"],
        [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
        [".xls"] = ["application/vnd.ms-excel"],
        [".csv"] = ["text/csv", "application/csv"],
        [".jpg"] = ["image/jpeg"],
        [".jpeg"] = ["image/jpeg"],
        [".png"] = ["image/png"],
        [".tiff"] = ["image/tiff"],
        [".tif"] = ["image/tiff"],
        [".bmp"] = ["image/bmp"],
        [".gif"] = ["image/gif"],
        [".zip"] = ["application/zip"],
        [".rar"] = ["application/rar", "application/x-rar-compressed"]
    };

    /// <summary>
    /// Allowed MIME types for legal document management.
    /// </summary>
    private static readonly FrozenSet<string> _allowedMimeTypesSet =
        _extensionToMimeTypes.Values
            .SelectMany(types => types)
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    #endregion Allowed Extensions and MIME Types

    #region File Name Validation

    /// <summary>
    /// Validates a file name according to professional and system requirements.
    /// </summary>
    /// <param name="fileName">The file name to validate (without extension).</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates file names for professional legal practice including:
    /// <list type="bullet">
    /// <item>Required field validation</item>
    /// <item>Length constraints for file system compatibility</item>
    /// <item>Character set validation for cross-platform support</item>
    /// <item>Professional naming conventions</item>
    /// <item>Security considerations (no script injection)</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateFileName(string? fileName, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Required validation
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
                    $"{propertyName} must be at least {MinFileNameLength} character long.",
                    [propertyName]);
                break;
            case > MaxFileNameLength:
                yield return new ValidationResult(
                    $"{propertyName} cannot exceed {MaxFileNameLength} characters.",
                    [propertyName]);
                break;
        }

        // Format validation
        if (!IsValidFileNameFormat(trimmed))
        {
            yield return new ValidationResult(
                GetFileNameFormatErrorMessage(propertyName, trimmed),
                [propertyName]);
        }

        // Security validation
        foreach (var result in ValidateFileNameSecurity(trimmed, propertyName))
            yield return result;
    }

    /// <summary>
    /// Validates a file extension according to security and format requirements.
    /// </summary>
    /// <param name="extension">The file extension to validate (including dot).</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates file extensions for legal document management including:
    /// <list type="bullet">
    /// <item>Required field validation</item>
    /// <item>Format validation (must start with dot)</item>
    /// <item>Length constraints</item>
    /// <item>Allowed extension list validation</item>
    /// <item>Security considerations</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateExtension(string? extension, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Required validation
        if (string.IsNullOrWhiteSpace(extension))
        {
            yield return new ValidationResult(
                $"{propertyName} is required and cannot be empty.",
                [propertyName]);
            yield break;
        }

        var trimmed = extension.Trim();

        // Format validation - must start with dot
        if (!trimmed.StartsWith('.'))
        {
            yield return new ValidationResult(
                $"{propertyName} must start with a dot (e.g., '.pdf').",
                [propertyName]);
            yield break;
        }

        switch (trimmed.Length)
        {
            // Length validation
            case < MinExtensionLength:
                yield return new ValidationResult(
                    $"{propertyName} must be at least {MinExtensionLength} characters long (including dot).",
                    [propertyName]);
                break;
            case > MaxExtensionLength:
                yield return new ValidationResult(
                    $"{propertyName} cannot exceed {MaxExtensionLength} characters.",
                    [propertyName]);
                break;
        }

        // Allowed extension validation
        if (!_allowedExtensionsSet.Contains(trimmed))
        {
            var allowedList = string.Join(", ", _allowedExtensionsArray);
            yield return new ValidationResult(
                $"{propertyName} '{trimmed}' is not allowed. Allowed extensions: {allowedList}",
                [propertyName]);
        }

        // Character validation
        if (!ExtensionFormatRegex().IsMatch(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} contains invalid characters. Extensions can only contain letters and numbers after the dot.",
                [propertyName]);
        }
    }

    #endregion File Name Validation

    #region File Size Validation

    /// <summary>
    /// Validates a file size according to system and business requirements.
    /// </summary>
    /// <param name="fileSize">The file size in bytes to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates file sizes for professional document management including:
    /// <list type="bullet">
    /// <item>Non-negative validation</item>
    /// <item>Minimum size requirements (no empty files)</item>
    /// <item>Maximum size limits for system performance</item>
    /// <item>Professional practice considerations</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateFileSize(long fileSize, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        switch (fileSize)
        {
            case < 0:
                yield return new ValidationResult(
                    $"{propertyName} cannot be negative.",
                    [propertyName]);
                break;
            case < MinFileSizeBytes:
                yield return new ValidationResult(
                    $"{propertyName} must be at least {MinFileSizeBytes} byte(s). Empty files are not allowed.",
                    [propertyName]);
                break;
            case > MaxFileSizeBytes:
            {
                var formattedMax = FormatFileSize(MaxFileSizeBytes);
                yield return new ValidationResult(
                    $"{propertyName} cannot exceed {formattedMax} ({MaxFileSizeBytes:N0} bytes).",
                    [propertyName]);
                break;
            }
        }
    }

    #endregion File Size Validation

    #region MIME Type Validation

    /// <summary>
    /// Validates a MIME type according to content type and security requirements.
    /// </summary>
    /// <param name="mimeType">The MIME type to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates MIME types for legal document management including:
    /// <list type="bullet">
    /// <item>Required field validation</item>
    /// <item>Format validation (type/subtype)</item>
    /// <item>Length constraints</item>
    /// <item>Allowed MIME type list validation</item>
    /// <item>Content type consistency</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateMimeType(string? mimeType, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Required validation
        if (string.IsNullOrWhiteSpace(mimeType))
        {
            yield return new ValidationResult(
                $"{propertyName} is required and cannot be empty.",
                [propertyName]);
            yield break;
        }

        var trimmed = mimeType.Trim();

        // Length validation
        if (trimmed.Length > MaxMimeTypeLength)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot exceed {MaxMimeTypeLength} characters.",
                [propertyName]);
        }

        // Format validation
        if (!MimeTypeFormatRegex().IsMatch(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} must follow the format 'type/subtype' (e.g., 'application/pdf').",
                [propertyName]);
        }
        else
        {
            // Allowed MIME type validation
            if (_allowedMimeTypesSet.Contains(trimmed)) yield break;
            var allowedList = string.Join(", ", _allowedMimeTypesSet.Take(10)); // Show first 10
            var remaining = _allowedMimeTypesSet.Count - 10;
            var suffix = remaining > 0 ? $" and {remaining} more" : "";
            yield return new ValidationResult(
                $"{propertyName} '{trimmed}' is not allowed. Examples of allowed types: {allowedList}{suffix}",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates MIME type consistency with file extension.
    /// </summary>
    /// <param name="mimeType">The MIME type.</param>
    /// <param name="extension">The file extension.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if consistent).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates that the MIME type is consistent with the file extension to prevent
    /// content type spoofing and ensure file integrity.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateMimeTypeConsistency(
        string? mimeType,
        string? extension,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (string.IsNullOrWhiteSpace(mimeType) || string.IsNullOrWhiteSpace(extension))
            yield break;

        var normalizedExtension = extension.Trim().ToLowerInvariant();
        var normalizedMimeType = mimeType.Trim().ToLowerInvariant();

        if (!_extensionToMimeTypes.TryGetValue(normalizedExtension, out var expectedMimeTypes)) yield break;
        if (expectedMimeTypes.Contains(normalizedMimeType, StringComparer.OrdinalIgnoreCase)) yield break;
        var expectedList = string.Join(", ", expectedMimeTypes);
        yield return new ValidationResult(
            $"{propertyName} '{mimeType}' does not match extension '{extension}'. Expected: {expectedList}",
            [propertyName]);
    }

    #endregion MIME Type Validation

    #region Checksum Validation

    /// <summary>
    /// Validates a file checksum according to cryptographic and format requirements.
    /// </summary>
    /// <param name="checksum">The checksum to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates checksums for file integrity verification including:
    /// <list type="bullet">
    /// <item>Required field validation</item>
    /// <item>Hexadecimal format validation</item>
    /// <item>Length constraints for common hash algorithms</item>
    /// <item>Professional standards for file integrity</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateChecksum(string? checksum, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Required validation
        if (string.IsNullOrWhiteSpace(checksum))
        {
            yield return new ValidationResult(
                $"{propertyName} is required for file integrity verification.",
                [propertyName]);
            yield break;
        }

        var trimmed = checksum.Trim();

        // Length validation
        if (trimmed.Length > MaxChecksumLength)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot exceed {MaxChecksumLength} characters.",
                [propertyName]);
        }

        // Format validation - must be hexadecimal
        if (!HexChecksumFormatRegex().IsMatch(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} must be a hexadecimal string (0-9, A-F) for cryptographic integrity.",
                [propertyName]);
        }
        else
        {
            // Length-specific validation for common hash types
            switch (trimmed.Length)
            {
                case 32:
                    // MD5 - discouraged but might exist in legacy data
                    yield return new ValidationResult(
                        $"{propertyName} appears to be MD5 (32 characters). Consider using SHA256 or better for enhanced security.",
                        [propertyName]);
                    break;
                case 40:
                    // SHA1 - discouraged but might exist in legacy data
                    yield return new ValidationResult(
                        $"{propertyName} appears to be SHA1 (40 characters). Consider using SHA256 or better for enhanced security.",
                        [propertyName]);
                    break;
                case 64:
                    // SHA256 - preferred
                    break;
                case 128:
                    // SHA512 - acceptable
                    break;
                default:
                    if (trimmed.Length < 32)
                    {
                        yield return new ValidationResult(
                            $"{propertyName} is too short for a cryptographic hash. Minimum 32 characters (MD5 equivalent).",
                            [propertyName]);
                    }
                    break;
            }
        }
    }

    #endregion Checksum Validation

    #region Quick Validation Methods (Performance Optimized)

    /// <summary>
    /// Quick validation check for file name (optimized for performance).
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFileNameValid([NotNullWhen(true)] string? fileName)
    {
        return !string.IsNullOrWhiteSpace(fileName) &&
               fileName.Trim().Length >= MinFileNameLength &&
               fileName.Trim().Length <= MaxFileNameLength &&
               IsValidFileNameFormat(fileName.Trim()) &&
               !ContainsSecurityRisk(fileName.Trim());
    }

    /// <summary>
    /// Quick validation check for file extension (optimized for performance).
    /// </summary>
    /// <param name="extension">The extension to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsExtensionAllowed([NotNullWhen(true)] string? extension)
    {
        return !string.IsNullOrWhiteSpace(extension) &&
               _allowedExtensionsSet.Contains(extension.Trim());
    }

    /// <summary>
    /// Quick validation check for file size (optimized for performance).
    /// </summary>
    /// <param name="fileSize">The file size to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFileSizeValid(long fileSize)
    {
        return fileSize is >= MinFileSizeBytes and <= MaxFileSizeBytes;
    }

    /// <summary>
    /// Quick validation check for MIME type (optimized for performance).
    /// </summary>
    /// <param name="mimeType">The MIME type to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMimeTypeAllowed([NotNullWhen(true)] string? mimeType)
    {
        return !string.IsNullOrWhiteSpace(mimeType) &&
               _allowedMimeTypesSet.Contains(mimeType.Trim());
    }

    /// <summary>
    /// Quick validation check for checksum (optimized for performance).
    /// </summary>
    /// <param name="checksum">The checksum to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidChecksum([NotNullWhen(true)] string? checksum)
    {
        return !string.IsNullOrWhiteSpace(checksum) &&
               checksum.Trim().Length >= 32 &&
               checksum.Trim().Length <= MaxChecksumLength &&
               HexChecksumFormatRegex().IsMatch(checksum.Trim());
    }

    #endregion Quick Validation Methods

    #region Utility Methods

    /// <summary>
    /// Formats a file size in bytes to a human-readable string.
    /// </summary>
    /// <param name="bytes">The file size in bytes.</param>
    /// <returns>Formatted file size string (e.g., "1.5 MB").</returns>
    /// <remarks>
    /// Provides professional file size formatting for display in user interfaces
    /// and validation messages.
    /// </remarks>
    public static string FormatFileSize(long bytes)
    {
        if (bytes < 0) return "Invalid";

        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        double len = bytes;
        var order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Gets expected MIME types for a file extension.
    /// </summary>
    /// <param name="extension">The file extension (including dot).</param>
    /// <returns>Array of expected MIME types.</returns>
    public static string[] GetExpectedMimeTypes(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return [];

        var normalized = extension.Trim().ToLowerInvariant();
        return _extensionToMimeTypes.TryGetValue(normalized, out var mimeTypes)
            ? mimeTypes
            : [];
    }

    /// <summary>
    /// Normalizes a file name for consistent storage and comparison.
    /// </summary>
    /// <param name="fileName">The file name to normalize.</param>
    /// <returns>Normalized file name or null if invalid.</returns>
    /// <remarks>
    /// Normalization includes:
    /// <list type="bullet">
    /// <item>Trimming whitespace</item>
    /// <item>Replacing invalid characters</item>
    /// <item>Collapsing multiple spaces</item>
    /// <item>Professional name formatting</item>
    /// </list>
    /// </remarks>
    [return: NotNullIfNotNull(nameof(fileName))]
    public static string? NormalizeFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var trimmed = fileName.Trim();
        if (trimmed.Length == 0) return string.Empty;

        // Replace problematic characters with safe alternatives
        var normalized = trimmed
            .Replace("\\", "_")
            .Replace("/", "_")
            .Replace(":", "_")
            .Replace("*", "_")
            .Replace("?", "_")
            .Replace("\"", "_")
            .Replace("<", "_")
            .Replace(">", "_")
            .Replace("|", "_");

        // Collapse multiple spaces and underscores
        normalized = MultipleSpacesRegex().Replace(normalized, " ");
        normalized = MultipleUnderscoresRegex().Replace(normalized, "_");

        return normalized;
    }

    /// <summary>
    /// Determines if a file extension represents a legal document format.
    /// </summary>
    /// <param name="extension">The file extension to check.</param>
    /// <returns>True if it's a legal document format; otherwise, false.</returns>
    public static bool IsLegalDocumentFormat([NotNullWhen(true)] string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return false;

        var normalized = extension.Trim().ToLowerInvariant();
        var legalFormats = new[] { ".pdf", ".docx", ".doc", ".rtf", ".txt" };
        return legalFormats.Contains(normalized);
    }

    /// <summary>
    /// Gets file validation statistics for diagnostics and monitoring.
    /// </summary>
    /// <returns>Dictionary containing file validation statistics.</returns>
    public static IReadOnlyDictionary<string, object> GetValidationStatistics()
    {
        return new Dictionary<string, object>
        {
            ["MaxFileNameLength"] = MaxFileNameLength,
            ["MinFileNameLength"] = MinFileNameLength,
            ["MaxExtensionLength"] = MaxExtensionLength,
            ["MinExtensionLength"] = MinExtensionLength,
            ["MaxFileSizeBytes"] = MaxFileSizeBytes,
            ["MaxFileSizeFormatted"] = FormatFileSize(MaxFileSizeBytes),
            ["MinFileSizeBytes"] = MinFileSizeBytes,
            ["AllowedExtensionsCount"] = AllowedExtensions.Count,
            ["AllowedMimeTypesCount"] = _allowedMimeTypesSet.Count,
            ["MaxChecksumLength"] = MaxChecksumLength,
            ["PreferredHashAlgorithm"] = "SHA256",
            ["ValidationRules"] = new Dictionary<string, string>
            {
                ["FileNameLength"] = $"Between {MinFileNameLength} and {MaxFileNameLength} characters",
                ["ExtensionLength"] = $"Between {MinExtensionLength} and {MaxExtensionLength} characters",
                ["FileSize"] = $"Between {MinFileSizeBytes} and {FormatFileSize(MaxFileSizeBytes)}",
                ["AllowedCharacters"] = "Letters, numbers, spaces, hyphens, underscores, periods, parentheses",
                ["SecurityRule"] = "No script tags, executable extensions, or malicious patterns",
                ["LegalFormats"] = "PDF (preferred), DOCX, DOC, RTF, TXT"
            }
        };
    }

    #endregion Utility Methods

    #region Private Helper Methods

    /// <summary>
    /// Validates file name format using comprehensive rules.
    /// </summary>
    /// <param name="fileName">The file name to validate (must be trimmed).</param>
    /// <returns>True if format is valid; otherwise, false.</returns>
    private static bool IsValidFileNameFormat(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        // Check character set (letters, numbers, spaces, professional punctuation)
        if (!FileNameCharacterRegex().IsMatch(fileName))
            return false;

        // Check for multiple consecutive spaces or underscores
        if (fileName.Contains("  ") || fileName.Contains("__"))
            return false;

        // Check start/end characters
        return char.IsLetterOrDigit(fileName[0]) && char.IsLetterOrDigit(fileName[^1]);
    }

    /// <summary>
    /// Validates file name security to prevent malicious content.
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <param name="propertyName">The property name for error messages.</param>
    /// <returns>Validation results for security checks.</returns>
    private static IEnumerable<ValidationResult> ValidateFileNameSecurity(string fileName, string propertyName)
    {
        if (ContainsSecurityRisk(fileName))
        {
            yield return new ValidationResult(
                $"{propertyName} contains potentially unsafe content patterns.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Checks if a file name contains security risk patterns.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns>True if contains security risks; otherwise, false.</returns>
    private static bool ContainsSecurityRisk(string fileName)
    {
        var lowerFileName = fileName.ToLowerInvariant();
        
        // Check for script injection patterns
        var dangerousPatterns = new[]
        {
            "<script", "javascript:", "vbscript:", "<object", "<embed", "<iframe",
            "data:", "blob:", "eval(", "alert(", "confirm(", "prompt(",
            ".exe", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs", ".js"
        };

        return dangerousPatterns.Any(pattern => lowerFileName.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a specific format error message based on the validation failure.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="value">The invalid value.</param>
    /// <returns>Specific error message.</returns>
    private static string GetFileNameFormatErrorMessage(string propertyName, string value)
    {
        if (value.Contains("  "))
            return $"{propertyName} cannot contain multiple consecutive spaces.";

        if (value.Contains("__"))
            return $"{propertyName} cannot contain multiple consecutive underscores.";

        switch (value.Length)
        {
            case > 0 when !char.IsLetterOrDigit(value[0]):
                return $"{propertyName} must start with a letter or number.";
            case > 0 when !char.IsLetterOrDigit(value[^1]):
                return $"{propertyName} must end with a letter or number.";
        }

        return !FileNameCharacterRegex().IsMatch(value) ? $"{propertyName} can only contain letters, numbers, spaces, hyphens, underscores, periods, and parentheses." : $"{propertyName} contains invalid format.";
    }

    #endregion Private Helper Methods

    #region Compiled Regex Patterns

    /// <summary>
    /// Compiled regex for validating file name characters.
    /// </summary>
    [GeneratedRegex(@"^[a-zA-Z0-9\s._\-()]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex FileNameCharacterRegex();

    /// <summary>
    /// Compiled regex for validating extension format.
    /// </summary>
    [GeneratedRegex(@"^\.[a-zA-Z0-9]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex ExtensionFormatRegex();

    /// <summary>
    /// Compiled regex for validating MIME type format.
    /// </summary>
    [GeneratedRegex(@"^[\w\.\-]+/[\w\.\-\+]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MimeTypeFormatRegex();

    /// <summary>
    /// Compiled regex for validating hexadecimal checksum format.
    /// </summary>
    [GeneratedRegex(@"^[A-Fa-f0-9]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex HexChecksumFormatRegex();

    /// <summary>
    /// Compiled regex for collapsing multiple spaces.
    /// </summary>
    [GeneratedRegex(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MultipleSpacesRegex();

    /// <summary>
    /// Compiled regex for collapsing multiple underscores.
    /// </summary>
    [GeneratedRegex(@"_+", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MultipleUnderscoresRegex();

    #endregion Compiled Regex Patterns
}