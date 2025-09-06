using System;
using System.IO;
using System.Linq;

using ADMS.Domain.Common;

namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a validated file name for documents in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// This value object encapsulates file name validation rules and ensures that all file names
/// meet legal document management standards for professional practice. It enforces cross-platform
/// compatibility and prevents common file system issues that could compromise document accessibility.
/// 
/// The validation includes:
/// - Length restrictions to ensure compatibility with various file systems
/// - Character validation to prevent file system conflicts
/// - Reserved name checking to avoid system conflicts on Windows
/// - Format validation to ensure professional naming standards
/// 
/// As a value object, FileName instances are immutable and compared by value, ensuring
/// consistent file naming throughout the document management system.
/// </remarks>
public sealed record FileName
{
    /// <summary>
    /// Maximum allowed length for file names in characters.
    /// </summary>
    /// <remarks>
    /// This limit ensures compatibility with most file systems while providing
    /// sufficient length for descriptive legal document names.
    /// </remarks>
    public const int MaxLength = 128;

    /// <summary>
    /// Minimum allowed length for file names in characters.
    /// </summary>
    /// <remarks>
    /// File names must have at least one meaningful character to be valid.
    /// </remarks>
    public const int MinLength = 1;

    /// <summary>
    /// Array of characters that are invalid in file names across different operating systems.
    /// </summary>
    /// <remarks>
    /// This includes the standard invalid file name characters from Path.GetInvalidFileNameChars()
    /// plus additional characters that can cause issues in web-based systems or specific platforms.
    /// The comprehensive list ensures maximum compatibility across all target environments.
    /// </remarks>
    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars()
        .Concat(new[] { '<', '>', ':', '"', '|', '?', '*' })
        .Distinct()
        .ToArray();

    /// <summary>
    /// Gets the validated file name value.
    /// </summary>
    /// <value>
    /// A string representing the file name that has been validated for compatibility
    /// and professional naming standards.
    /// </value>
    /// <remarks>
    /// The file name is guaranteed to:
    /// - Be within the acceptable length range
    /// - Contain only valid characters for cross-platform compatibility
    /// - Not conflict with reserved system names
    /// - Follow professional naming conventions suitable for legal documents
    /// </remarks>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileName"/> record with the specified value.
    /// </summary>
    /// <param name="value">The validated file name value.</param>
    /// <remarks>
    /// This constructor is private to enforce the use of factory methods for creating instances.
    /// This ensures all file names are properly validated before construction.
    /// </remarks>
    private FileName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="FileName"/> value object with comprehensive validation.
    /// </summary>
    /// <param name="value">The file name string to validate.</param>
    /// <returns>
    /// A <see cref="Result{FileName}"/> containing the created FileName on success,
    /// or validation errors if the input is invalid.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive validation including:
    /// - Null and whitespace checking
    /// - Length validation (1-128 characters)
    /// - Invalid character detection
    /// - Reserved name checking (Windows compatibility)
    /// - Trailing character validation (periods and spaces)
    /// 
    /// Example usage:
    /// <code>
    /// var fileNameResult = FileName.Create("Legal Contract v2.1");
    /// if (fileNameResult.IsSuccess)
    /// {
    ///     var fileName = fileNameResult.Value;
    ///     Console.WriteLine($"Valid file name: {fileName.Value}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Invalid file name: {fileNameResult.Error.Message}");
    /// }
    /// </code>
    /// </remarks>
    public static Result<FileName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<FileName>(DomainError.Create(
                "FILENAME_REQUIRED",
                "File name is required and cannot be empty"));

        var trimmedValue = value.Trim();

        if (trimmedValue.Length < MinLength)
            return Result.Failure<FileName>(DomainError.Create(
                "FILENAME_TOO_SHORT",
                $"File name must be at least {MinLength} character(s) long"));

        if (trimmedValue.Length > MaxLength)
            return Result.Failure<FileName>(DomainError.Create(
                "FILENAME_TOO_LONG",
                $"File name cannot exceed {MaxLength} characters"));

        // Check for invalid characters
        var invalidCharsFound = trimmedValue.Where(c => InvalidChars.Contains(c)).ToArray();
        if (invalidCharsFound.Length > 0)
        {
            var invalidCharsList = string.Join(", ", invalidCharsFound.Distinct().Select(c => $"'{c}'"));
            return Result.Failure<FileName>(DomainError.Create(
                "FILENAME_INVALID_CHARACTERS",
                $"File name contains invalid characters: {invalidCharsList}"));
        }

        // Check for reserved Windows names
        if (IsReservedName(trimmedValue))
            return Result.Failure<FileName>(DomainError.Create(
                "FILENAME_RESERVED_NAME",
                $"'{trimmedValue}' is a reserved system name and cannot be used as a file name"));

        // Check for names ending with period or space (Windows restriction)
        if (trimmedValue.EndsWith('.'))
            return Result.Failure<FileName>(DomainError.Create(
                "FILENAME_ENDS_WITH_PERIOD",
                "File name cannot end with a period"));

        if (trimmedValue.EndsWith(' '))
            return Result.Failure<FileName>(DomainError.Create(
                "FILENAME_ENDS_WITH_SPACE",
                "File name cannot end with a space"));

        return Result.Success(new FileName(trimmedValue));
    }

    /// <summary>
    /// Creates a <see cref="FileName"/> from a trusted source without validation.
    /// </summary>
    /// <param name="value">The file name value from a trusted source.</param>
    /// <returns>A new <see cref="FileName"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is null or empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>WARNING:</strong> This method bypasses validation and should only be used
    /// when the file name is known to be valid and properly formatted, such as
    /// when loading from a database or trusted configuration source.
    /// </para>
    /// 
    /// Example usage:
    /// <code>
    /// // Loading from database where file name is known to be valid
    /// var fileName = FileName.FromTrustedSource(dbRecord.FileName);
    /// </code>
    /// </remarks>
    public static FileName FromTrustedSource(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value), "File name value cannot be null or empty");

        return new(value);
    }

    /// <summary>
    /// Generates a sanitized file name from potentially invalid input.
    /// </summary>
    /// <param name="value">The potentially invalid file name to sanitize.</param>
    /// <param name="replacementChar">The character to use as replacement for invalid characters (default: '_').</param>
    /// <returns>
    /// A <see cref="Result{FileName}"/> containing a valid FileName with invalid characters replaced,
    /// or an error if the sanitization cannot produce a valid file name.
    /// </returns>
    /// <remarks>
    /// This method attempts to create a valid file name by:
    /// - Replacing invalid characters with the specified replacement character
    /// - Truncating overly long names to the maximum length
    /// - Adding a prefix if the name would conflict with reserved names
    /// - Removing trailing periods and spaces
    /// 
    /// This is useful when processing file names from external sources that may not
    /// meet the system's validation requirements.
    /// 
    /// Example usage:
    /// <code>
    /// var sanitizedResult = FileName.Sanitize("My File: Version <2>");
    /// if (sanitizedResult.IsSuccess)
    /// {
    ///     Console.WriteLine(sanitizedResult.Value.Value); // "My File_ Version _2_"
    /// }
    /// </code>
    /// </remarks>
    public static Result<FileName> Sanitize(string value, char replacementChar = '_')
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<FileName>(DomainError.Create(
                "FILENAME_SANITIZE_EMPTY",
                "Cannot sanitize null or empty file name"));

        var trimmedValue = value.Trim();

        // Replace invalid characters
        var sanitized = new string(trimmedValue.Select(c => InvalidChars.Contains(c) ? replacementChar : c).ToArray());

        // Remove trailing periods and spaces
        sanitized = sanitized.TrimEnd('.', ' ');

        // Ensure we have at least one character
        if (string.IsNullOrEmpty(sanitized))
        {
            sanitized = "document";
        }

        // Handle reserved names by prefixing
        if (IsReservedName(sanitized))
        {
            sanitized = $"doc_{sanitized}";
        }

        // Truncate if too long
        if (sanitized.Length > MaxLength)
        {
            sanitized = sanitized.Substring(0, MaxLength);
            // Ensure we don't end with a period or space after truncation
            sanitized = sanitized.TrimEnd('.', ' ');
        }

        // Final validation
        return Create(sanitized);
    }

    /// <summary>
    /// Determines whether the specified name is a reserved system name on Windows.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>
    /// <c>true</c> if the name is reserved; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method checks against Windows reserved names including:
    /// - Device names: CON, PRN, AUX, NUL
    /// - Serial port names: COM1-COM9
    /// - Parallel port names: LPT1-LPT9
    /// - Names with extensions (e.g., CON.txt)
    /// 
    /// The check is case-insensitive to ensure compatibility.
    /// </remarks>
    private static bool IsReservedName(string name)
    {
        var upperName = name.ToUpperInvariant();
        var reservedNames = new[]
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        return reservedNames.Contains(upperName) ||
               reservedNames.Any(reserved => upperName.StartsWith(reserved + ".", StringComparison.Ordinal));
    }

    /// <summary>
    /// Gets the file name without any extension.
    /// </summary>
    /// <returns>
    /// The file name with the extension removed, or the full name if no extension is present.
    /// </returns>
    /// <remarks>
    /// This method removes everything from the last period (.) to the end of the file name.
    /// If no period is found, the entire file name is returned.
    /// 
    /// Example:
    /// - "document.pdf" returns "document"
    /// - "my.file.v2.docx" returns "my.file.v2"  
    /// - "no-extension" returns "no-extension"
    /// </remarks>
    public string GetNameWithoutExtension()
    {
        var lastDotIndex = Value.LastIndexOf('.');
        return lastDotIndex >= 0 ? Value.Substring(0, lastDotIndex) : Value;
    }

    /// <summary>
    /// Determines whether this file name appears to have an extension.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the file name contains a period followed by at least one character; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method checks for the presence of an extension pattern but does not validate
    /// whether the extension is a recognized file type.
    /// 
    /// Examples:
    /// - "document.pdf" returns true
    /// - "file." returns false (no characters after period)
    /// - "no-extension" returns false
    /// </remarks>
    public bool HasExtension()
    {
        var lastDotIndex = Value.LastIndexOf('.');
        return lastDotIndex >= 0 && lastDotIndex < Value.Length - 1;
    }

    /// <summary>
    /// Implicitly converts a <see cref="FileName"/> to its string representation.
    /// </summary>
    /// <param name="fileName">The FileName to convert.</param>
    /// <returns>The file name value as a string, or null if the fileName is null.</returns>
    /// <remarks>
    /// This implicit conversion allows FileName instances to be used seamlessly
    /// where string values are expected, such as in file I/O operations,
    /// logging, or API responses.
    /// 
    /// Example usage:
    /// <code>
    /// var fileName = FileName.Create("document.pdf").Value;
    /// string fileNameString = fileName; // Implicit conversion
    /// File.WriteAllText(fileNameString, content);
    /// </code>
    /// </remarks>
    public static implicit operator string(FileName fileName) => fileName?.Value;

    /// <summary>
    /// Returns a string representation of this file name.
    /// </summary>
    /// <returns>
    /// The validated file name value.
    /// </returns>
    /// <remarks>
    /// This method returns the same value as the <see cref="Value"/> property
    /// and is suitable for display, logging, and file system operations.
    /// </remarks>
    public override string ToString() => Value;
}