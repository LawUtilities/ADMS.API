using ADMS.Domain.Common;
using ADMS.Domain.Entities;
using ADMS.Domain.Errors;
using ADMS.Domain.ValueObjects;

namespace ADMS.Domain.Services;

/// <summary>
/// Domain service for validating document-related business rules and operations.
/// </summary>
public interface IDocumentValidationService
{
    /// <summary>
    /// Validates document creation parameters.
    /// </summary>
    /// <param name="fileName">The proposed file name.</param>
    /// <param name="extension">The file extension.</param>
    /// <param name="fileSize">The file size in bytes.</param>
    /// <param name="mimeType">The MIME type.</param>
    /// <param name="checksum">The file checksum.</param>
    /// <param name="matterId">The matter ID.</param>
    /// <returns>A result indicating validation success or failure.</returns>
    Result ValidateDocumentCreation(
        string fileName,
        string extension,
        long fileSize,
        string mimeType,
        string checksum,
        MatterId matterId);

    /// <summary>
    /// Validates that a document can be deleted.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <returns>A result indicating whether deletion is allowed.</returns>
    Result ValidateDocumentDeletion(Document document);

    /// <summary>
    /// Validates that a document can be checked out.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <returns>A result indicating whether checkout is allowed.</returns>
    Result ValidateDocumentCheckOut(Document document);

    /// <summary>
    /// Validates that a document can be checked in.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <returns>A result indicating whether checkin is allowed.</returns>
    Result ValidateDocumentCheckIn(Document document);

    /// <summary>
    /// Validates file extension against allowed types.
    /// </summary>
    /// <param name="extension">The file extension to validate.</param>
    /// <returns>A result indicating whether the extension is valid.</returns>
    Result ValidateFileExtension(string extension);

    /// <summary>
    /// Validates file size against system limits.
    /// </summary>
    /// <param name="fileSize">The file size to validate.</param>
    /// <returns>A result indicating whether the file size is valid.</returns>
    Result ValidateFileSize(long fileSize);

    /// <summary>
    /// Validates MIME type against allowed types.
    /// </summary>
    /// <param name="mimeType">The MIME type to validate.</param>
    /// <param name="extension">The file extension for consistency checking.</param>
    /// <returns>A result indicating whether the MIME type is valid.</returns>
    Result ValidateMimeType(string mimeType, string extension);
}

/// <summary>
/// Implementation of document validation service.
/// </summary>
public sealed class DocumentValidationService : IDocumentValidationService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "pdf", "doc", "docx", "rtf", "txt",
        "xls", "xlsx", "csv",
        "ppt", "pptx",
        "jpg", "jpeg", "png", "gif", "tiff", "bmp"
    };

    private static readonly IReadOnlyDictionary<string, string[]> MimeTypeMap =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["pdf"] = ["application/pdf"],
            ["doc"] = ["application/msword"],
            ["docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
            ["rtf"] = ["application/rtf", "text/rtf"],
            ["txt"] = ["text/plain"],
            ["xls"] = ["application/vnd.ms-excel"],
            ["xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
            ["csv"] = ["text/csv", "application/csv"],
            ["ppt"] = ["application/vnd.ms-powerpoint"],
            ["pptx"] = ["application/vnd.openxmlformats-officedocument.presentationml.presentation"],
            ["jpg"] = ["image/jpeg"],
            ["jpeg"] = ["image/jpeg"],
            ["png"] = ["image/png"],
            ["gif"] = ["image/gif"],
            ["tiff"] = ["image/tiff"],
            ["bmp"] = ["image/bmp"]
        };

    private const long MaxFileSize = 50L * 1024 * 1024; // 50 MB
    private const long MinFileSize = 1L; // 1 byte
    private const int MaxExtensionLength = 10; // Maximum reasonable extension length

    public Result ValidateDocumentCreation(
        string fileName,
        string extension,
        long fileSize,
        string mimeType,
        string checksum,
        MatterId matterId)
    {
        ArgumentNullException.ThrowIfNull(matterId);

        // Validate file name
        var fileNameResult = FileName.Create(fileName);
        if (fileNameResult.IsFailure)
            return fileNameResult;

        // Validate extension
        var extensionResult = ValidateFileExtension(extension);
        if (extensionResult.IsFailure)
            return extensionResult;

        // Validate file size
        var fileSizeResult = ValidateFileSize(fileSize);
        if (fileSizeResult.IsFailure)
            return fileSizeResult;

        // Validate MIME type
        var mimeTypeResult = ValidateMimeType(mimeType, extension);
        if (mimeTypeResult.IsFailure)
            return mimeTypeResult;

        // Validate checksum
        var checksumResult = FileChecksum.Create(checksum);
        if (checksumResult.IsFailure)
            return checksumResult;

        return Result.Success();
    }

    public Result ValidateDocumentDeletion(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.IsDeleted)
            return Result.Failure(DocumentErrors.AlreadyDeleted);

        if (document.IsCheckedOut)
            return Result.Failure(DocumentErrors.CannotDeleteCheckedOutDocument);

        return Result.Success();
    }

    public Result ValidateDocumentCheckOut(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.IsDeleted)
            return Result.Failure(DocumentErrors.DocumentDeleted);

        if (document.IsCheckedOut)
            return Result.Failure(DocumentErrors.AlreadyCheckedOut);

        return Result.Success();
    }

    public Result ValidateDocumentCheckIn(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.IsDeleted)
            return Result.Failure(DocumentErrors.DocumentDeleted);

        if (!document.IsCheckedOut)
            return Result.Failure(DocumentErrors.NotCheckedOut);

        return Result.Success();
    }

    public Result ValidateFileExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return Result.Failure(DocumentErrors.ExtensionRequired);

        var normalizedExtension = NormalizeExtension(extension);

        if (normalizedExtension.Length == 0)
            return Result.Failure(DocumentErrors.ExtensionRequired);

        if (normalizedExtension.Length > MaxExtensionLength)
            return Result.Failure(DocumentErrors.ExtensionTooLong);

        if (!IsValidExtensionFormat(normalizedExtension))
            return Result.Failure(DocumentErrors.ExtensionInvalidFormat);

        if (!AllowedExtensions.Contains(normalizedExtension))
            return Result.Failure(DomainError.Custom(
                "DOCUMENT_EXTENSION_NOT_ALLOWED",
                $"File extension '{extension}' is not allowed. Supported extensions: {string.Join(", ", AllowedExtensions.Order())}"));

        return Result.Success();
    }

    public Result ValidateFileSize(long fileSize)
    {
        if (fileSize < MinFileSize)
            return Result.Failure(DomainError.Custom(
                "DOCUMENT_FILE_TOO_SMALL",
                $"File size must be at least {MinFileSize} byte(s). Current size: {fileSize} bytes"));

        if (fileSize > MaxFileSize)
            return Result.Failure(DomainError.Custom(
                "DOCUMENT_FILE_TOO_LARGE",
                $"File size cannot exceed {MaxFileSize:N0} bytes ({MaxFileSize / (1024.0 * 1024.0):F1} MB). Current size: {fileSize:N0} bytes ({fileSize / (1024.0 * 1024.0):F1} MB)"));

        return Result.Success();
    }

    public Result ValidateMimeType(string mimeType, string extension)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            return Result.Failure(DocumentErrors.InvalidMimeType);

        var normalizedExtension = NormalizeExtension(extension);
        var normalizedMimeType = mimeType.Trim().ToLowerInvariant();

        if (!MimeTypeMap.TryGetValue(normalizedExtension, out var allowedMimeTypes))
            return Result.Failure(DomainError.Custom(
                "DOCUMENT_EXTENSION_MIME_TYPE_MISMATCH",
                $"Extension '{extension}' is not supported or MIME type mapping is not available"));

        if (!allowedMimeTypes.Any(allowed => string.Equals(allowed, normalizedMimeType, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure(DomainError.Custom(
                "DOCUMENT_MIME_TYPE_EXTENSION_MISMATCH",
                $"MIME type '{mimeType}' does not match extension '{extension}'. Expected MIME types: {string.Join(", ", allowedMimeTypes)}"));

        return Result.Success();
    }

    /// <summary>
    /// Normalizes a file extension by trimming, converting to lowercase, and removing leading dots.
    /// </summary>
    /// <param name="extension">The extension to normalize.</param>
    /// <returns>The normalized extension.</returns>
    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return string.Empty;

        return extension.Trim().TrimStart('.').ToLowerInvariant();
    }

    /// <summary>
    /// Determines whether the extension contains only valid characters.
    /// </summary>
    /// <param name="extension">The normalized extension to validate.</param>
    /// <returns>True if the extension format is valid; otherwise, false.</returns>
    private static bool IsValidExtensionFormat(string extension)
    {
        // Extension should contain only letters and numbers, no special characters
        return !string.IsNullOrEmpty(extension) &&
               extension.All(c => char.IsLetterOrDigit(c));
    }
}