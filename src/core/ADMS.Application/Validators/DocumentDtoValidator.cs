using ADMS.Application.DTOs;

using FluentValidation;

namespace ADMS.Application.Validators;

/// <summary>
/// Validator for DocumentDto using FluentValidation.
/// </summary>
public class DocumentDtoValidator : AbstractValidator<DocumentDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentDtoValidator"/> class.
    /// </summary>
    public DocumentDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Document ID is required.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required.")
            .MaximumLength(255)
            .WithMessage("File name cannot exceed 255 characters.")
            .Must(BeValidFileName)
            .WithMessage("File name contains invalid characters.");

        RuleFor(x => x.Extension)
            .NotEmpty()
            .WithMessage("File extension is required.")
            .MaximumLength(10)
            .WithMessage("File extension cannot exceed 10 characters.")
            .Must(BeAllowedExtension)
            .WithMessage("File extension is not allowed.")
            .Must(BeValidExtensionFormat)
            .WithMessage("File extension must be lowercase and contain only letters and numbers.");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("File size must be greater than zero.")
            .LessThanOrEqualTo(100 * 1024 * 1024)
            .WithMessage("File size cannot exceed 100 MB.");

        RuleFor(x => x.MimeType)
            .NotEmpty()
            .WithMessage("MIME type is required.")
            .MaximumLength(128)
            .WithMessage("MIME type cannot exceed 128 characters.")
            .Must(BeValidMimeType)
            .WithMessage("Invalid MIME type format.")
            .Must(BeAllowedMimeType)
            .WithMessage("MIME type is not allowed.");

        RuleFor(x => x.Checksum)
            .NotEmpty()
            .WithMessage("Checksum is required.")
            .Length(64)
            .WithMessage("Checksum must be exactly 64 characters.")
            .Must(BeValidChecksum)
            .WithMessage("Checksum must be a valid hexadecimal string.");

        // Business rules
        RuleFor(x => x)
            .Must(x => !(x.IsCheckedOut && x.IsDeleted))
            .WithMessage("Document cannot be both checked out and deleted.");

        RuleFor(x => x)
            .Must(HaveConsistentMimeTypeAndExtension)
            .WithMessage("MIME type does not match the file extension.");

        RuleFor(x => x.CreationDate)
            .NotEmpty()
            .WithMessage("Creation date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Creation date cannot be in the future.");
    }

    private static bool BeValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        // Check for invalid characters
        var invalidChars = new[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|' };
        return !fileName.Any(c => invalidChars.Contains(c) || char.IsControl(c));
    }

    private static bool BeAllowedExtension(string extension)
    {
        var allowedExtensions = new[]
        {
            "pdf", "doc", "docx", "xls", "xlsx", "ppt", "pptx",
            "txt", "rtf", "csv", "jpg", "jpeg", "png", "gif", "bmp", "tiff"
        };

        return allowedExtensions.Contains(extension.ToLowerInvariant());
    }

    private static bool BeValidExtensionFormat(string extension)
    {
        return !string.IsNullOrWhiteSpace(extension) &&
               extension.All(char.IsLetterOrDigit) &&
               extension.Equals(extension, StringComparison.InvariantCultureIgnoreCase);
    }

    private static bool BeValidMimeType(string mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            return false;

        var parts = mimeType.Split('/');
        return parts.Length == 2 &&
               !string.IsNullOrWhiteSpace(parts[0]) &&
               !string.IsNullOrWhiteSpace(parts[1]);
    }

    private static bool BeAllowedMimeType(string mimeType)
    {
        var allowedMimeTypes = new[]
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-powerpoint",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "text/plain",
            "application/rtf",
            "text/csv",
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/bmp",
            "image/tiff"
        };

        return allowedMimeTypes.Contains(mimeType.ToLowerInvariant());
    }

    private static bool BeValidChecksum(string checksum)
    {
        return !string.IsNullOrWhiteSpace(checksum) &&
               checksum.All(c => char.IsDigit(c) || c is >= 'A' and <= 'F' || c is >= 'a' and <= 'f');
    }

    private static bool HaveConsistentMimeTypeAndExtension(DocumentDto document)
    {
        var expectedMimeTypes = GetExpectedMimeTypesForExtension(document.Extension.ToLowerInvariant());
        return expectedMimeTypes.Contains(document.MimeType.ToLowerInvariant());
    }

    private static string[] GetExpectedMimeTypesForExtension(string? extension)
    {
        return extension switch
        {
            "pdf" => ["application/pdf"],
            "doc" => ["application/msword"],
            "docx" => ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
            "xls" => ["application/vnd.ms-excel"],
            "xlsx" => ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
            "ppt" => ["application/vnd.ms-powerpoint"],
            "pptx" => ["application/vnd.openxmlformats-officedocument.presentationml.presentation"],
            "txt" => ["text/plain"],
            "rtf" => ["application/rtf"],
            "csv" => ["text/csv"],
            "jpg" or "jpeg" => ["image/jpeg"],
            "png" => ["image/png"],
            "gif" => ["image/gif"],
            "bmp" => ["image/bmp"],
            "tiff" or "tif" => ["image/tiff"],
            _ => []
        };
    }
}