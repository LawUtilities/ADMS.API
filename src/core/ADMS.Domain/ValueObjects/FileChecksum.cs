using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using ADMS.Domain.Common;

namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed file checksum value object for document integrity verification.
/// </summary>
/// <remarks>
/// FileChecksum provides secure file integrity verification using SHA-256 hashing, essential
/// for legal document management where document authenticity and tampering detection are critical.
/// 
/// <para><strong>Security Features:</strong></para>
/// <list type="bullet">
/// <item><strong>SHA-256 Hashing:</strong> Industry-standard cryptographic hash function</item>
/// <item><strong>Integrity Verification:</strong> Detects any changes to document content</item>
/// <item><strong>Tampering Detection:</strong> Identifies unauthorized modifications</item>
/// <item><strong>Format Validation:</strong> Ensures checksums are properly formatted</item>
/// </list>
/// 
/// <para><strong>Legal Compliance:</strong></para>
/// <list type="bullet">
/// <item>Supports evidence authenticity requirements</item>
/// <item>Enables document chain of custody verification</item>
/// <item>Provides cryptographic proof of document integrity</item>
/// <item>Meets professional responsibility standards</item>
/// </list>
/// </remarks>
[ComplexType]
public sealed record FileChecksum : IComparable<FileChecksum>
{
    #region Constants

    /// <summary>
    /// The expected length of a SHA-256 checksum in hexadecimal format (64 characters).
    /// </summary>
    public const int Sha256HexLength = 64;

    /// <summary>
    /// Regular expression for validating hexadecimal checksum format.
    /// </summary>
    private static readonly Regex HexRegex = new(
        "^[0-9A-Fa-f]+$",
        RegexOptions.Compiled);

    #endregion

    /// <summary>
    /// Gets the validated checksum value in uppercase hexadecimal format.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the FileChecksum with the specified value.
    /// </summary>
    /// <param name="value">The checksum value in hexadecimal format.</param>
    /// <exception cref="ArgumentException">Thrown when the checksum is invalid.</exception>
    private FileChecksum(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Checksum cannot be null or empty", nameof(value));

        var normalized = NormalizeChecksum(value);
        var validationResult = ValidateChecksum(normalized);

        if (validationResult.IsFailure)
            throw new ArgumentException(validationResult.Error?.Message, nameof(value));

        Value = normalized;
    }

    /// <summary>
    /// Creates a new FileChecksum from the specified hexadecimal string with validation.
    /// </summary>
    /// <param name="value">The checksum value in hexadecimal format.</param>
    /// <returns>A Result containing either the created FileChecksum or validation errors.</returns>
    /// <example>
    /// <code>
    /// var result = FileChecksum.Create("A1B2C3D4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E3F4A5B6C7D8E9F0A1B2C3D4");
    /// if (result.IsSuccess)
    /// {
    ///     var checksum = result.Value;
    ///     // Use the checksum
    /// }
    /// else
    /// {
    ///     // Handle validation errors
    ///     Console.WriteLine(result.Error.Message);
    /// }
    /// </code>
    /// </example>
    public static Result<FileChecksum> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_NULL_OR_EMPTY",
                "File checksum cannot be null or empty"));

        var normalized = NormalizeChecksum(value);
        var validationResult = ValidateChecksum(normalized);

        return validationResult.IsFailure
            ? Result.Failure<FileChecksum>(validationResult.Error ?? DomainError.Unknown)
            : Result.Success(new FileChecksum(normalized));
    }

    /// <summary>
    /// Creates a FileChecksum from a trusted source without full validation.
    /// </summary>
    /// <param name="value">The checksum value from a trusted source (e.g., database).</param>
    /// <returns>A new FileChecksum instance.</returns>
    /// <remarks>
    /// This method is intended for use when loading data from trusted sources like databases
    /// where the checksum has already been validated. It performs minimal validation only.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when the value is null or empty.</exception>
    /// <example>
    /// <code>
    /// // Used in Entity Framework value converters
    /// var checksum = FileChecksum.FromTrustedSource(databaseValue);
    /// </code>
    /// </example>
    public static FileChecksum FromTrustedSource(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Checksum cannot be null or empty", nameof(value));

        return new FileChecksum(NormalizeChecksum(value));
    }

    /// <summary>
    /// Computes the SHA-256 checksum for the specified file content.
    /// </summary>
    /// <param name="fileContent">The file content as a byte array.</param>
    /// <returns>A Result containing the computed FileChecksum.</returns>
    /// <example>
    /// <code>
    /// var fileBytes = File.ReadAllBytes("document.pdf");
    /// var result = FileChecksum.ComputeFromBytes(fileBytes);
    /// if (result.IsSuccess)
    /// {
    ///     var checksum = result.Value;
    ///     // Store checksum with document
    /// }
    /// </code>
    /// </example>
    public static Result<FileChecksum> ComputeFromBytes(byte[] fileContent)
    {
        if (fileContent == null)
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_NULL_CONTENT",
                "File content cannot be null"));

        if (fileContent.Length == 0)
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_EMPTY_CONTENT",
                "File content cannot be empty"));

        try
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(fileContent);
            var hexString = Convert.ToHexString(hashBytes);

            return Result.Success(new FileChecksum(hexString));
        }
        catch (Exception ex)
        {
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_COMPUTATION_FAILED",
                $"Failed to compute file checksum: {ex.Message}"));
        }
    }

    /// <summary>
    /// Computes the SHA-256 checksum for the specified file stream.
    /// </summary>
    /// <param name="stream">The file stream to compute the checksum for.</param>
    /// <returns>A Result containing the computed FileChecksum.</returns>
    /// <example>
    /// <code>
    /// using var fileStream = File.OpenRead("document.pdf");
    /// var result = await FileChecksum.ComputeFromStreamAsync(fileStream);
    /// if (result.IsSuccess)
    /// {
    ///     var checksum = result.Value;
    ///     // Store checksum with document
    /// }
    /// </code>
    /// </example>
    public static async Task<Result<FileChecksum>> ComputeFromStreamAsync(Stream stream)
    {
        if (stream == null)
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_NULL_STREAM",
                "File stream cannot be null"));

        if (!stream.CanRead)
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_UNREADABLE_STREAM",
                "File stream must be readable"));

        try
        {
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            var hexString = Convert.ToHexString(hashBytes);

            return Result.Success(new FileChecksum(hexString));
        }
        catch (Exception ex)
        {
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_COMPUTATION_FAILED",
                $"Failed to compute file checksum: {ex.Message}"));
        }
    }

    /// <summary>
    /// Validates whether the specified string is a valid file checksum.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns>True if the string is a valid checksum; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var isValid = FileChecksum.IsValid("A1B2C3D4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E3F4A5B6C7D8E9F0A1B2C3D4"); // true
    /// var isInvalid = FileChecksum.IsValid("invalid-checksum"); // false
    /// </code>
    /// </example>
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = NormalizeChecksum(value);
        return ValidateChecksum(normalized).IsSuccess;
    }

    /// <summary>
    /// Verifies that this checksum matches the provided file content.
    /// </summary>
    /// <param name="fileContent">The file content to verify against.</param>
    /// <returns>A Result indicating whether the checksum matches the file content.</returns>
    /// <example>
    /// <code>
    /// var fileBytes = File.ReadAllBytes("document.pdf");
    /// var verificationResult = checksum.VerifyContent(fileBytes);
    /// if (verificationResult.IsSuccess)
    /// {
    ///     Console.WriteLine("File integrity verified!");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("File has been tampered with!");
    /// }
    /// </code>
    /// </example>
    public Result VerifyContent(byte[] fileContent)
    {
        var computedResult = ComputeFromBytes(fileContent);

        if (computedResult.IsFailure)
            return Result.Failure(computedResult.Error ?? DomainError.Unknown);

        if (!Equals(computedResult.Value))
            return Result.Failure(DomainError.Create(
                "CHECKSUM_VERIFICATION_FAILED",
                "File content does not match the expected checksum"));

        return Result.Success();
    }

    /// <summary>
    /// Verifies that this checksum matches the provided file stream.
    /// </summary>
    /// <param name="stream">The file stream to verify against.</param>
    /// <returns>A Result indicating whether the checksum matches the stream content.</returns>
    /// <example>
    /// <code>
    /// using var fileStream = File.OpenRead("document.pdf");
    /// var verificationResult = await checksum.VerifyContentAsync(fileStream);
    /// if (verificationResult.IsSuccess)
    /// {
    ///     Console.WriteLine("File integrity verified!");
    /// }
    /// </code>
    /// </example>
    public async Task<Result> VerifyContentAsync(Stream stream)
    {
        var computedResult = await ComputeFromStreamAsync(stream);

        if (computedResult.IsFailure)
            return Result.Failure(computedResult.Error ?? DomainError.Unknown);

        if (!Equals(computedResult.Value))
            return Result.Failure(DomainError.Create(
                "CHECKSUM_VERIFICATION_FAILED",
                "Stream content does not match the expected checksum"));

        return Result.Success();
    }

    /// <summary>
    /// Gets the checksum as a byte array.
    /// </summary>
    /// <returns>The checksum as a byte array.</returns>
    /// <example>
    /// <code>
    /// var checksum = FileChecksum.Create("A1B2C3D4...").Value;
    /// var bytes = checksum.ToByteArray();
    /// </code>
    /// </example>
    public byte[] ToByteArray()
    {
        return Convert.FromHexString(Value);
    }

    #region Private Methods

    /// <summary>
    /// Normalizes a checksum string by converting to uppercase and removing whitespace.
    /// </summary>
    /// <param name="value">The checksum to normalize.</param>
    /// <returns>The normalized checksum.</returns>
    private static string NormalizeChecksum(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Remove whitespace and convert to uppercase
        return value.Replace(" ", "").Replace("-", "").ToUpperInvariant();
    }

    /// <summary>
    /// Validates a checksum against business rules and security requirements.
    /// </summary>
    /// <param name="value">The checksum to validate.</param>
    /// <returns>A Result indicating whether the checksum is valid.</returns>
    private static Result ValidateChecksum(string value)
    {
        // Check length (SHA-256 produces 64 hex characters)
        if (value.Length != Sha256HexLength)
            return Result.Failure(DomainError.Create(
                "CHECKSUM_INVALID_LENGTH",
                $"Checksum must be exactly {Sha256HexLength} characters long (SHA-256 hex format)"));

        // Check format (must be hexadecimal)
        if (!HexRegex.IsMatch(value))
            return Result.Failure(DomainError.Create(
                "CHECKSUM_INVALID_FORMAT",
                "Checksum must contain only hexadecimal characters (0-9, A-F)"));

        // Check for obviously invalid patterns
        if (value.All(c => c == '0') || value.All(c => c == 'F'))
            return Result.Failure(DomainError.Create(
                "CHECKSUM_SUSPICIOUS_PATTERN",
                "Checksum appears to have an invalid pattern"));

        return Result.Success();
    }

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Compares this FileChecksum with another FileChecksum.
    /// </summary>
    /// <param name="other">The other FileChecksum to compare with.</param>
    /// <returns>A value indicating the relative order of the checksums.</returns>
    public int CompareTo(FileChecksum? other)
    {
        if (other is null) return 1;
        return string.Compare(Value, other.Value, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether this FileChecksum is equal to another FileChecksum.
    /// </summary>
    /// <param name="other">The other FileChecksum to compare with.</param>
    /// <returns>True if the checksums are equal; otherwise, false.</returns>
    public bool Equals(FileChecksum? other)
    {
        return other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    /// <summary>
    /// Returns the hash code for this FileChecksum.
    /// </summary>
    /// <returns>A hash code for this FileChecksum.</returns>
    public override int GetHashCode() => Value.GetHashCode();

    #endregion

    #region Operators

    /// <summary>
    /// Implicitly converts a FileChecksum to a string.
    /// </summary>
    /// <param name="checksum">The FileChecksum to convert.</param>
    /// <returns>The underlying string value.</returns>
    public static implicit operator string(FileChecksum checksum) => checksum.Value;

    /// <summary>
    /// Explicitly converts a string to a FileChecksum.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>A new FileChecksum instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the string is not a valid checksum.</exception>
    public static explicit operator FileChecksum(string value)
    {
        var result = Create(value);
        return result.IsSuccess
            ? result.Value
            : throw new ArgumentException(result.Error?.Message, nameof(value));
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the FileChecksum.
    /// </summary>
    /// <returns>The checksum value in uppercase hexadecimal format.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Returns a formatted string representation of the FileChecksum.
    /// </summary>
    /// <param name="format">The format to use ("S" for short, "F" for full, or null for default).</param>
    /// <returns>The formatted checksum string.</returns>
    /// <example>
    /// <code>
    /// var checksum = FileChecksum.Create("A1B2C3D4...").Value;
    /// var short = checksum.ToString("S"); // "A1B2C3D4..."
    /// var full = checksum.ToString("F");  // "SHA256:A1B2C3D4..."
    /// </code>
    /// </example>
    public string ToString(string? format) => format?.ToUpperInvariant() switch
    {
        "S" => Value[..8] + "...",
        "F" => $"SHA256:{Value}",
        _ => Value
    };

    #endregion
}