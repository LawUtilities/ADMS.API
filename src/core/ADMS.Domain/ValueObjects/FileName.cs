using ADMS.Domain.Common;
using ADMS.Domain.Errors;

namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a validated file name for documents in the ADMS system.
/// </summary>
/// <remarks>
/// This value object encapsulates file name validation rules and ensures that all file names
/// meet legal document management standards for professional practice.
/// </remarks>
public sealed record FileName
{
    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars()
        .Concat(new[] { '<', '>', ':', '"', '|', '?', '*' })
        .ToArray();

    public string Value { get; }

    private FileName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new FileName value object with validation.
    /// </summary>
    /// <param name="value">The file name to validate.</param>
    /// <returns>A Result containing the FileName or validation errors.</returns>
    public static Result<FileName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<FileName>(DocumentErrors.FileNameRequired);

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > 128)
            return Result.Failure<FileName>(DocumentErrors.FileNameTooLong);

        if (trimmedValue.Length < 1)
            return Result.Failure<FileName>(DocumentErrors.FileNameRequired);

        // Check for invalid characters
        if (trimmedValue.IndexOfAny(InvalidChars) >= 0)
            return Result.Failure<FileName>(DocumentErrors.FileNameInvalidCharacters);

        // Check for reserved Windows names
        if (IsReservedName(trimmedValue))
            return Result.Failure<FileName>(DocumentErrors.FileNameInvalidCharacters);

        // Check for names ending with period or space (Windows restriction)
        if (trimmedValue.EndsWith('.') || trimmedValue.EndsWith(' '))
            return Result.Failure<FileName>(DocumentErrors.FileNameInvalidCharacters);

        return Result.Success(new FileName(trimmedValue));
    }

    /// <summary>
    /// Creates a FileName from a trusted source (e.g., database) without validation.
    /// </summary>
    /// <param name="value">The file name value.</param>
    /// <returns>A FileName instance.</returns>
    /// <remarks>
    /// Use this method only when the value is known to be valid (e.g., from database).
    /// </remarks>
    public static FileName FromTrustedSource(string value) => new(value);

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
               reservedNames.Any(reserved => upperName.StartsWith(reserved + "."));
    }

    public static implicit operator string(FileName fileName) => fileName.Value;

    public override string ToString() => Value;
}