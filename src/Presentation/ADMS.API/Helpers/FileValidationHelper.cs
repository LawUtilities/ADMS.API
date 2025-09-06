using Microsoft.Extensions.Options;

using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace ADMS.API.Helpers;

/// <summary>
/// Enhanced file validation helper with comprehensive security checks.
/// </summary>
public static class FileValidationHelper
{
    private static readonly Dictionary<string, string[]> _allowedMimeTypes = new()
    {
        // Documents
        { ".pdf", new[] { "application/pdf" } },
        { ".doc", new[] { "application/msword" } },
        { ".docx", new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } },
        { ".xls", new[] { "application/vnd.ms-excel" } },
        { ".xlsx", new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
        { ".ppt", new[] { "application/vnd.ms-powerpoint" } },
        { ".pptx", new[] { "application/vnd.openxmlformats-officedocument.presentationml.presentation" } },
        { ".txt", new[] { "text/plain" } },
        { ".rtf", new[] { "application/rtf", "text/rtf" } },
        { ".csv", new[] { "text/csv", "application/csv" } },
        { ".xml", new[] { "text/xml", "application/xml" } },
        { ".json", new[] { "application/json", "text/json" } },
        
        // Images
        { ".jpg", new[] { "image/jpeg" } },
        { ".jpeg", new[] { "image/jpeg" } },
        { ".png", new[] { "image/png" } },
        { ".gif", new[] { "image/gif" } },
        { ".bmp", new[] { "image/bmp" } },
        { ".tiff", new[] { "image/tiff" } },
        { ".webp", new[] { "image/webp" } },
        
        // Archives
        { ".zip", new[] { "application/zip" } },
        { ".rar", new[] { "application/vnd.rar", "application/x-rar-compressed" } },
        { ".7z", new[] { "application/x-7z-compressed" } }
    };

    private static readonly Dictionary<string, byte[]> _fileSignatures = new()
    {
        // PDF
        { ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } }, // %PDF
        
        // Microsoft Office (newer formats)
        { ".docx", new byte[] { 0x50, 0x4B, 0x03, 0x04 } }, // PK..
        { ".xlsx", new byte[] { 0x50, 0x4B, 0x03, 0x04 } }, // PK..
        { ".pptx", new byte[] { 0x50, 0x4B, 0x03, 0x04 } }, // PK..
        
        // Microsoft Office (older formats)
        { ".doc", new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } },
        { ".xls", new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } },
        { ".ppt", new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } },
        
        // Images
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
        { ".gif", new byte[] { 0x47, 0x49, 0x46, 0x38 } }, // GIF8
        { ".bmp", new byte[] { 0x42, 0x4D } }, // BM
        { ".tiff", new byte[] { 0x49, 0x49, 0x2A, 0x00 } }, // II*
        
        // Archives
        { ".zip", new byte[] { 0x50, 0x4B, 0x03, 0x04 } }, // PK..
        { ".rar", new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 } }, // Rar!
        { ".7z", new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C } } // 7z
    };

    private static readonly HashSet<string> _dangerousExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs", ".vbe", ".js", ".jse",
        ".ws", ".wsf", ".wsc", ".wsh", ".ps1", ".ps1xml", ".ps2", ".ps2xml", ".psc1",
        ".psc2", ".msh", ".msh1", ".msh2", ".mshxml", ".msh1xml", ".msh2xml", ".scf",
        ".lnk", ".inf", ".reg", ".dll", ".cpl", ".msc", ".msi", ".jar", ".app", ".deb",
        ".pkg", ".dmg", ".iso", ".img"
    };

    /// <summary>
    /// Validates if the file extension is allowed.
    /// </summary>
    /// <param name="extension">The file extension to validate (with or without leading dot).</param>
    /// <returns>True if the extension is allowed, false otherwise.</returns>
    public static bool IsExtensionAllowed(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return false;

        var normalizedExtension = NormalizeExtension(extension);

        // Check if it's a dangerous extension
        if (_dangerousExtensions.Contains(normalizedExtension))
            return false;

        // Check if it's in our allowed list
        return _allowedMimeTypes.ContainsKey(normalizedExtension);
    }

    /// <summary>
    /// Validates if the MIME type is allowed.
    /// </summary>
    /// <param name="mimeType">The MIME type to validate.</param>
    /// <returns>True if the MIME type is allowed, false otherwise.</returns>
    public static bool IsValidMimeType(string mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            return false;

        var normalizedMimeType = mimeType.Trim().ToLowerInvariant();
        return _allowedMimeTypes.Values.Any(mimes =>
            mimes.Any(mime => mime.Equals(normalizedMimeType, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Validates file type by analyzing file content and comparing with extension.
    /// </summary>
    /// <param name="fileBytes">The file content bytes.</param>
    /// <param name="detectedMimeType">The detected MIME type based on content analysis.</param>
    /// <param name="detectedExtension">The detected file extension based on content analysis.</param>
    /// <param name="logger">Optional logger for detailed error reporting.</param>
    /// <returns>True if the file type is valid and matches expectations, false otherwise.</returns>
    public static bool IsValidFileType(byte[] fileBytes, out string? detectedMimeType,
        out string? detectedExtension, ILogger? logger = null)
    {
        detectedMimeType = null;
        detectedExtension = null;

        if (fileBytes == null || fileBytes.Length == 0)
        {
            logger?.LogWarning("File validation failed: Empty or null file content");
            return false;
        }

        try
        {
            // Detect file type by magic bytes
            var detectedType = DetectFileTypeBySignature(fileBytes);
            if (detectedType.HasValue)
            {
                detectedExtension = detectedType.Value.extension;
                detectedMimeType = GetMimeTypeForExtension(detectedExtension);

                logger?.LogDebug("File type detected by signature: Extension={Extension}, MimeType={MimeType}",
                    detectedExtension, detectedMimeType);

                return IsExtensionAllowed(detectedExtension);
            }

            // Fallback: Try to detect text files
            if (IsTextFile(fileBytes))
            {
                detectedExtension = ".txt";
                detectedMimeType = "text/plain";

                logger?.LogDebug("File detected as text file");
                return true;
            }

            logger?.LogWarning("File type could not be determined from content analysis");
            return false;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error during file type validation");
            return false;
        }
    }

    /// <summary>
    /// Validates file size against allowed limits.
    /// </summary>
    /// <param name="fileSize">The file size in bytes.</param>
    /// <param name="maxAllowedSize">The maximum allowed size in bytes.</param>
    /// <returns>True if the file size is within limits, false otherwise.</returns>
    public static bool IsValidFileSize(long fileSize, long maxAllowedSize)
    {
        return fileSize > 0 && fileSize <= maxAllowedSize;
    }

    /// <summary>
    /// Validates that the file extension matches the detected content type.
    /// </summary>
    /// <param name="providedExtension">The file extension provided by the user.</param>
    /// <param name="fileBytes">The file content bytes.</param>
    /// <param name="logger">Optional logger for detailed error reporting.</param>
    /// <returns>True if extension matches content, false otherwise.</returns>
    public static bool ValidateExtensionMatchesContent(string providedExtension, byte[] fileBytes, ILogger? logger = null)
    {
        var normalizedExtension = NormalizeExtension(providedExtension);

        if (!IsValidFileType(fileBytes, out var detectedMimeType, out var detectedExtension, logger))
        {
            logger?.LogWarning("Could not validate file content for extension matching");
            return false;
        }

        var isMatch = string.Equals(normalizedExtension, detectedExtension, StringComparison.OrdinalIgnoreCase);

        if (!isMatch)
        {
            logger?.LogWarning("Extension mismatch: Provided={ProvidedExtension}, Detected={DetectedExtension}",
                normalizedExtension, detectedExtension);
        }

        return isMatch;
    }

    /// <summary>
    /// Computes a secure hash of the file content for integrity verification.
    /// </summary>
    /// <param name="fileBytes">The file content bytes.</param>
    /// <param name="algorithm">The hash algorithm to use (default: SHA256).</param>
    /// <returns>The computed hash as a hexadecimal string.</returns>
    public static string ComputeFileHash(byte[] fileBytes, HashAlgorithmName algorithm = default)
    {
        ArgumentNullException.ThrowIfNull(fileBytes);

        if (algorithm == default)
            algorithm = HashAlgorithmName.SHA256;

        using var hashAlgorithm = HashAlgorithm.Create(algorithm.Name!) ?? throw new InvalidOperationException($"Hash algorithm {algorithm.Name} not available");
        var hashBytes = hashAlgorithm.ComputeHash(fileBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Performs comprehensive file validation including all security checks.
    /// </summary>
    /// <param name="fileName">The original file name.</param>
    /// <param name="fileBytes">The file content bytes.</param>
    /// <param name="maxFileSize">The maximum allowed file size in bytes.</param>
    /// <param name="logger">Optional logger for detailed error reporting.</param>
    /// <returns>A comprehensive validation result.</returns>
    public static FileValidationResult ValidateFile(string fileName, byte[] fileBytes, long maxFileSize, ILogger? logger = null)
    {
        var result = new FileValidationResult { FileName = fileName };

        try
        {
            // Basic null/empty checks
            if (string.IsNullOrWhiteSpace(fileName))
            {
                result.AddError("File name cannot be empty");
                return result;
            }

            if (fileBytes == null || fileBytes.Length == 0)
            {
                result.AddError("File content cannot be empty");
                return result;
            }

            // Extract and validate extension
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                result.AddError("File must have an extension");
                return result;
            }

            result.Extension = NormalizeExtension(extension);

            // Check if extension is allowed
            if (!IsExtensionAllowed(result.Extension))
            {
                result.AddError($"File extension '{result.Extension}' is not allowed");
                return result;
            }

            // Validate file size
            result.FileSize = fileBytes.Length;
            if (!IsValidFileSize(result.FileSize, maxFileSize))
            {
                result.AddError($"File size ({result.FileSize:N0} bytes) exceeds maximum allowed ({maxFileSize:N0} bytes)");
                return result;
            }

            // Validate file content type
            if (!IsValidFileType(fileBytes, out var detectedMimeType, out var detectedExtension, logger))
            {
                result.AddError("File content type is not supported or could not be determined");
                return result;
            }

            result.DetectedMimeType = detectedMimeType;
            result.DetectedExtension = detectedExtension;

            // Validate extension matches content
            if (!string.Equals(result.Extension, result.DetectedExtension, StringComparison.OrdinalIgnoreCase))
            {
                result.AddError($"File extension '{result.Extension}' does not match detected type '{result.DetectedExtension}'");
                return result;
            }

            // Compute file hash for integrity
            result.FileHash = ComputeFileHash(fileBytes);

            result.IsValid = true;
            logger?.LogDebug("File validation successful: {FileName}, Extension: {Extension}, Size: {Size}, Hash: {Hash}",
                fileName, result.Extension, result.FileSize, result.FileHash);

        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Unexpected error during file validation for {FileName}", fileName);
            result.AddError("An unexpected error occurred during file validation");
        }

        return result;
    }

    #region Private Helper Methods

    /// <summary>
    /// Normalizes file extension to lowercase with leading dot.
    /// </summary>
    /// <param name="extension">The extension to normalize.</param>
    /// <returns>The normalized extension.</returns>
    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return string.Empty;

        var normalized = extension.Trim().ToLowerInvariant();
        return normalized.StartsWith('.') ? normalized : $".{normalized}";
    }

    /// <summary>
    /// Detects file type by analyzing magic bytes/file signature.
    /// </summary>
    /// <param name="fileBytes">The file content bytes.</param>
    /// <returns>The detected extension and MIME type, or null if not detected.</returns>
    private static (string extension, string mimeType)? DetectFileTypeBySignature(byte[] fileBytes)
    {
        if (fileBytes.Length < 4)
            return null;

        foreach (var (extension, signature) in _fileSignatures)
        {
            if (signature.Length <= fileBytes.Length &&
                fileBytes.Take(signature.Length).SequenceEqual(signature))
            {
                var mimeType = GetMimeTypeForExtension(extension);
                return (extension, mimeType);
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the primary MIME type for a given extension.
    /// </summary>
    /// <param name="extension">The file extension.</param>
    /// <returns>The primary MIME type.</returns>
    private static string GetMimeTypeForExtension(string extension)
    {
        return _allowedMimeTypes.TryGetValue(extension, out var mimeTypes) ? mimeTypes[0] : "application/octet-stream";
    }

    /// <summary>
    /// Determines if the file content appears to be text.
    /// </summary>
    /// <param name="fileBytes">The file content bytes.</param>
    /// <returns>True if the content appears to be text, false otherwise.</returns>
    private static bool IsTextFile(byte[] fileBytes)
    {
        if (fileBytes.Length == 0)
            return false;

        // Sample first 8KB or entire file if smaller
        var sampleSize = Math.Min(fileBytes.Length, 8192);
        var sample = fileBytes.Take(sampleSize);

        // Check for null bytes (indicating binary content)
        if (sample.Contains((byte)0))
            return false;

        // Check if most bytes are printable ASCII or common Unicode characters
        var printableCount = sample.Count(b =>
            (b >= 32 && b <= 126) || // Printable ASCII
            b == 9 || b == 10 || b == 13); // Tab, LF, CR

        var printableRatio = (double)printableCount / sampleSize;
        return printableRatio > 0.95; // 95% printable characters
    }

    #endregion
}

/// <summary>
/// Represents the result of file validation.
/// </summary>
public class FileValidationResult
{
    /// <summary>
    /// Gets or sets the original file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normalized file extension.
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detected file extension based on content analysis.
    /// </summary>
    public string? DetectedExtension { get; set; }

    /// <summary>
    /// Gets or sets the detected MIME type based on content analysis.
    /// </summary>
    public string? DetectedMimeType { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the computed file hash.
    /// </summary>
    public string? FileHash { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public List<string> Errors { get; } = new();

    /// <summary>
    /// Adds a validation error.
    /// </summary>
    /// <param name="error">The error message.</param>
    public void AddError(string error)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            Errors.Add(error);
        }
    }

    /// <summary>
    /// Gets a value indicating whether there are any validation errors.
    /// </summary>
    public bool HasErrors => Errors.Count > 0;
}

/// <summary>
/// Custom validation attribute for file extensions.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class FileExtensionValidationAttribute : ValidationAttribute
{
    private readonly string[] _allowedExtensions;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileExtensionValidationAttribute"/> class.
    /// </summary>
    /// <param name="allowedExtensions">The allowed file extensions.</param>
    public FileExtensionValidationAttribute(params string[] allowedExtensions)
    {
        _allowedExtensions = allowedExtensions ?? throw new ArgumentNullException(nameof(allowedExtensions));
        ErrorMessage = "The file extension is not allowed.";
    }

    /// <summary>
    /// Validates the file extension.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>The validation result.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        var extension = value.ToString();
        if (string.IsNullOrWhiteSpace(extension))
            return ValidationResult.Success;

        var normalizedExtension = extension.StartsWith('.') ? extension : $".{extension}";

        if (!FileValidationHelper.IsExtensionAllowed(normalizedExtension))
        {
            return new ValidationResult(
                ErrorMessage ?? $"The file extension '{normalizedExtension}' is not allowed.",
                validationContext.MemberName != null ? new[] { validationContext.MemberName } : Array.Empty<string>());
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Custom validation attribute for file sizes.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class FileSizeValidationAttribute : ValidationAttribute
{
    private readonly long _maxSizeBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSizeValidationAttribute"/> class.
    /// </summary>
    /// <param name="maxSizeBytes">The maximum allowed file size in bytes.</param>
    public FileSizeValidationAttribute(long maxSizeBytes)
    {
        _maxSizeBytes = maxSizeBytes;
        ErrorMessage = "The file size exceeds the maximum allowed limit.";
    }

    /// <summary>
    /// Validates the file size.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>The validation result.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is not long fileSize)
        {
            if (value is int intSize)
                fileSize = intSize;
            else if (long.TryParse(value.ToString(), out var parsedSize))
                fileSize = parsedSize;
            else
                return new ValidationResult("Invalid file size format.");
        }

        if (!FileValidationHelper.IsValidFileSize(fileSize, _maxSizeBytes))
        {
            var maxSizeMB = _maxSizeBytes / (1024.0 * 1024.0);
            return new ValidationResult(
                ErrorMessage ?? $"File size ({fileSize:N0} bytes) exceeds maximum allowed ({maxSizeMB:F1} MB).",
                validationContext.MemberName != null ? new[] { validationContext.MemberName } : Array.Empty<string>());
        }

        return ValidationResult.Success;
    }
}