using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

using ADMS.Domain.Common;

namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed file name value object with validation and normalization.
/// </summary>
/// <remarks>
/// FileName provides validation and normalization for file names used in the legal document
/// management system, ensuring they meet professional standards and security requirements.
/// 
/// <para><strong>Validation Rules:</strong></para>
/// <list type="bullet">
/// <item><strong>Length:</strong> Between 1 and 255 characters</item>
/// <item><strong>Characters:</strong> No invalid file system characters</item>
/// <item><strong>Security:</strong> No malicious patterns or reserved names</item>
/// <item><strong>Professional:</strong> Suitable for legal document management</item>
/// </list>
/// 
/// <para><strong>Normalization Features:</strong></para>
/// <list type="bullet">
/// <item>Trims whitespace</item>
/// <item>Removes multiple consecutive spaces</item>
/// <item>Ensures professional formatting</item>
/// <item>Maintains case sensitivity for professional appearance</item>
/// </list>
/// </remarks>
[ComplexType]
public sealed record FileName : IComparable<FileName>
{
    #region Constants

    /// <summary>
    /// Maximum allowed length for file names.
    /// </summary>
    public const int MaxLength = 255;

    /// <summary>
    /// Minimum allowed length for file names.
    /// </summary>
    public const int MinLength = 1;

    /// <summary>
    /// Characters that are not allowed in file names.
    /// </summary>
    private static readonly char[] InvalidCharacters = ['<', '>', ':', '"', '|', '?', '*', '/', '\\'];

    /// <summary>
    /// Reserved file names that are not allowed.
    /// </summary>
    private static readonly string[] ReservedNames =
    [
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    ];

    /// <summary>
    /// Regular expression for validating file name format.
    /// </summary>
    private static readonly Regex ValidFileNameRegex = new(
        @"^[^<>:""/\\|?*\x00-\x1f]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    #endregion

    /// <summary>
    /// Gets the validated file name value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the FileName with the specified value.
    /// </summary>
    /// <param name="value">The file name value.</param>
    /// <exception cref="ArgumentException">Thrown when the file name is invalid.</exception>
    private FileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("File name cannot be null or empty", nameof(value));

        var normalized = NormalizeFileName(value);
        var validationResult = ValidateFileName(normalized);

        if (validationResult.IsFailure)
            throw new ArgumentException(validationResult.Error?.Message, nameof(value));

        Value = normalized;
    }

    /// <summary>
    /// Creates a new FileName from the specified value with validation.
    /// </summary>
    /// <param name="value">The file name value to create from.</param>
    /// <returns>A Result containing either the created FileName or validation errors.</returns>
    /// <example>
    /// <code>
    /// var result = FileName.Create("My Document.pdf");
    /// if (result.IsSuccess)
    /// {
    ///     var fileName = result.Value;
    ///     // Use the file name
    /// }
    /// else
    /// {
    ///     // Handle validation errors
    ///     Console.WriteLine(result.Error.Message);
    /// }
    /// </code>
    /// </example>
    public static Result<FileName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<FileName>(DomainError.Create(
                "FILENAME_NULL_OR_EMPTY",
                "File name cannot be null or empty"));

        var normalized = NormalizeFileName(value);
        var validationResult = ValidateFileName(normalized);

        return validationResult.IsFailure ? Result.Failure<FileName>(validationResult.Error ?? DomainError.Unknown) : Result.Success(new FileName(normalized));
    }

    /// <summary>
    /// Creates a FileName from a trusted source without full validation.
    /// </summary>
    /// <param name="value">The file name value from a trusted source (e.g., database).</param>
    /// <returns>A new FileName instance.</returns>
    /// <remarks>
    /// This method is intended for use when loading data from trusted sources like databases
    /// where the file name has already been validated. It performs minimal validation only.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when the value is null or empty.</exception>
    /// <example>
    /// <code>
    /// // Used in Entity Framework value converters
    /// var fileName = FileName.FromTrustedSource(databaseValue);
    /// </code>
    /// </example>
    public static FileName FromTrustedSource(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("File name cannot be null or empty", nameof(value));

        return new FileName(value.Trim());
    }

    /// <summary>
    /// Validates whether the specified string is a valid file name.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns>True if the string is a valid file name; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var isValid = FileName.IsValid("Document.pdf"); // true
    /// var isInvalid = FileName.IsValid("Con.txt"); // false (reserved name)
    /// </code>
    /// </example>
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = NormalizeFileName(value);
        return ValidateFileName(normalized).IsSuccess;
    }

    /// <summary>
    /// Gets the file name without any extension.
    /// </summary>
    /// <returns>The file name without the extension.</returns>
    /// <example>
    /// <code>
    /// var fileName = FileName.Create("Document.pdf").Value;
    /// var nameOnly = fileName.GetNameWithoutExtension(); // "Document"
    /// </code>
    /// </example>
    public string GetNameWithoutExtension()
    {
        var lastDotIndex = Value.LastIndexOf('.');
        return lastDotIndex > 0 ? Value[..lastDotIndex] : Value;
    }

    /// <summary>
    /// Gets the file extension from the file name.
    /// </summary>
    /// <returns>The file extension including the dot, or empty string if no extension.</returns>
    /// <example>
    /// <code>
    /// var fileName = FileName.Create("Document.pdf").Value;
    /// var extension = fileName.GetExtension(); // ".pdf"
    /// </code>
    /// </example>
    public string GetExtension()
    {
        var lastDotIndex = Value.LastIndexOf('.');
        return lastDotIndex > 0 && lastDotIndex < Value.Length - 1
            ? Value[lastDotIndex..]
            : string.Empty;
    }

    /// <summary>
    /// Creates a new FileName with the specified extension.
    /// </summary>
    /// <param name="newExtension">The new extension (with or without leading dot).</param>
    /// <returns>A Result containing the new FileName with the updated extension.</returns>
    /// <example>
    /// <code>
    /// var fileName = FileName.Create("Document.pdf").Value;
    /// var result = fileName.WithExtension(".docx");
    /// if (result.IsSuccess)
    /// {
    ///     var newFileName = result.Value; // "Document.docx"
    /// }
    /// </code>
    /// </example>
    public Result<FileName> WithExtension(string newExtension)
    {
        if (string.IsNullOrWhiteSpace(newExtension))
            return Result.Failure<FileName>(DomainError.Create(
                "FILENAME_INVALID_EXTENSION",
                "Extension cannot be null or empty"));

        var extension = newExtension.StartsWith('.') ? newExtension : $".{newExtension}";
        var nameWithoutExt = GetNameWithoutExtension();
        var newFileName = $"{nameWithoutExt}{extension}";

        return Create(newFileName);
    }

    #region Private Methods

    /// <summary>
    /// Normalizes a file name by trimming whitespace and removing invalid patterns.
    /// </summary>
    /// <param name="value">The file name to normalize.</param>
    /// <returns>The normalized file name.</returns>
    private static string NormalizeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Trim whitespace
        var normalized = value.Trim();

        // Remove multiple consecutive spaces
        normalized = Regex.Replace(normalized, @"\s+", " ");

        // Remove leading and trailing dots (security measure)
        normalized = normalized.Trim('.');

        return normalized;
    }

    /// <summary>
    /// Validates a file name against business rules and security requirements.
    /// </summary>
    /// <param name="value">The file name to validate.</param>
    /// <returns>A Result indicating whether the file name is valid.</returns>
    private static Result ValidateFileName(string value)
    {
        // Check length
        if (value.Length < MinLength)
            return Result.Failure(DomainError.Create(
                "FILENAME_TOO_SHORT",
                $"File name must be at least {MinLength} character(s) long"));

        if (value.Length > MaxLength)
            return Result.Failure(DomainError.Create(
                "FILENAME_TOO_LONG",
                $"File name cannot exceed {MaxLength} characters"));

        // Check for invalid characters
        if (value.Any(c => InvalidCharacters.Contains(c)))
            return Result.Failure(DomainError.Create(
                "FILENAME_INVALID_CHARACTERS",
                $"File name contains invalid characters. Invalid characters: {string.Join(", ", InvalidCharacters)}"));

        // Check for control characters
        if (value.Any(char.IsControl))
            return Result.Failure(DomainError.Create(
                "FILENAME_CONTROL_CHARACTERS",
                "File name cannot contain control characters"));

        // Check regex pattern
        if (!ValidFileNameRegex.IsMatch(value))
            return Result.Failure(DomainError.Create(
                "FILENAME_INVALID_FORMAT",
                "File name contains invalid characters or format"));

        // Check for reserved names
        var nameWithoutExtension = value.Contains('.')
            ? value[..value.LastIndexOf('.')]
            : value;

        if (ReservedNames.Contains(nameWithoutExtension.ToUpperInvariant()))
            return Result.Failure(DomainError.Create(
                "FILENAME_RESERVED_NAME",
                $"'{nameWithoutExtension}' is a reserved file name and cannot be used"));

        // Check for names ending with space or dot (Windows restriction)
        if (value.EndsWith(' ') || value.EndsWith('.'))
            return Result.Failure(DomainError.Create(
                "FILENAME_INVALID_ENDING",
                "File name cannot end with a space or dot"));

        return Result.Success();
    }

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Compares this FileName with another FileName.
    /// </summary>
    /// <param name="other">The other FileName to compare with.</param>
    /// <returns>A value indicating the relative order of the FileNames.</returns>
    public int CompareTo(FileName? other)
    {
        if (other is null) return 1;
        return string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether this FileName is equal to another FileName.
    /// </summary>
    /// <param name="other">The other FileName to compare with.</param>
    /// <returns>True if the FileNames are equal; otherwise, false.</returns>
    public bool Equals(FileName? other)
    {
        return other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns the hash code for this FileName.
    /// </summary>
    /// <returns>A hash code for this FileName.</returns>
    public override int GetHashCode() => Value.ToLowerInvariant().GetHashCode();

    #endregion

    #region Operators

    /// <summary>
    /// Implicitly converts a FileName to a string.
    /// </summary>
    /// <param name="fileName">The FileName to convert.</param>
    /// <returns>The underlying string value.</returns>
    public static implicit operator string(FileName fileName) => fileName.Value;

    /// <summary>
    /// Explicitly converts a string to a FileName.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>A new FileName instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the string is not a valid file name.</exception>
    public static explicit operator FileName(string value)
    {
        var result = Create(value);
        return result.IsSuccess
            ? result.Value
            : throw new ArgumentException(result.Error?.Message, nameof(value));
    }

    /// <summary>
    /// Determines whether one FileName is less than another.
    /// </summary>
    /// <param name="left">The first FileName to compare.</param>
    /// <param name="right">The second FileName to compare.</param>
    /// <returns>True if the first FileName is less than the second; otherwise, false.</returns>
    public static bool operator <(FileName? left, FileName? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether one FileName is less than or equal to another.
    /// </summary>
    /// <param name="left">The first FileName to compare.</param>
    /// <param name="right">The second FileName to compare.</param>
    /// <returns>True if the first FileName is less than or equal to the second; otherwise, false.</returns>
    public static bool operator <=(FileName? left, FileName? right) =>
        left is null || left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether one FileName is greater than another.
    /// </summary>
    /// <param name="left">The first FileName to compare.</param>
    /// <param name="right">The second FileName to compare.</param>
    /// <returns>True if the first FileName is greater than the second; otherwise, false.</returns>
    public static bool operator >(FileName? left, FileName? right) =>
        left is not null && left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether one FileName is greater than or equal to another.
    /// </summary>
    /// <param name="left">The first FileName to compare.</param>
    /// <param name="right">The second FileName to compare.</param>
    /// <returns>True if the first FileName is greater than or equal to the second; otherwise, false.</returns>
    public static bool operator >=(FileName? left, FileName? right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the FileName.
    /// </summary>
    /// <returns>The file name value.</returns>
    public override string ToString() => Value;

    #endregion
}