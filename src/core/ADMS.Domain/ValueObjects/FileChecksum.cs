using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ADMS.Domain.Common;

namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a validated SHA-256 file checksum for document integrity verification in the ADMS system.
/// </summary>
/// <remarks>
/// This value object ensures that all checksums are valid SHA-256 hashes in the correct hexadecimal format,
/// supporting legal document integrity verification and compliance requirements. The checksum provides
/// cryptographic assurance that document content has not been altered or corrupted.
/// 
/// SHA-256 is chosen for its balance of security and performance, providing 256-bit hash values that
/// are extremely resistant to collision attacks while being computationally efficient for document
/// management operations.
/// 
/// As a value object, FileChecksum instances are immutable and compared by value, ensuring consistent
/// integrity verification throughout the document lifecycle.
/// </remarks>
public sealed record FileChecksum
{
    /// <summary>
    /// Regular expression pattern for validating SHA-256 hash format.
    /// </summary>
    /// <remarks>
    /// This pattern matches exactly 64 hexadecimal characters (lowercase a-f and digits 0-9),
    /// which corresponds to the standard SHA-256 hash output format.
    /// The pattern is compiled for improved performance in repeated validations.
    /// </remarks>
    private static readonly Regex Sha256Regex = new(@"^[a-f0-9]{64}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// The expected length of a SHA-256 hash in hexadecimal characters.
    /// </summary>
    private const int Sha256HexLength = 64;

    /// <summary>
    /// Gets the SHA-256 checksum value as a lowercase hexadecimal string.
    /// </summary>
    /// <value>
    /// A 64-character lowercase hexadecimal string representing the SHA-256 hash.
    /// </value>
    /// <remarks>
    /// The checksum value is guaranteed to be a valid SHA-256 hash in lowercase
    /// hexadecimal format due to validation performed during construction.
    /// This standardized format ensures consistent comparison and storage operations.
    /// </remarks>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileChecksum"/> record with the specified checksum value.
    /// </summary>
    /// <param name="value">The validated SHA-256 checksum value.</param>
    /// <remarks>
    /// This constructor is private to enforce the use of factory methods for creating instances.
    /// This ensures all checksums are properly validated and normalized before construction.
    /// </remarks>
    private FileChecksum(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="FileChecksum"/> value object with validation.
    /// </summary>
    /// <param name="value">The checksum string to validate and normalize.</param>
    /// <returns>
    /// A <see cref="Result{FileChecksum}"/> containing the created FileChecksum on success,
    /// or validation errors if the input is invalid.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive validation including:
    /// - Null or whitespace checking
    /// - Length validation (must be exactly 64 characters)
    /// - Format validation (must contain only hexadecimal characters)
    /// - Automatic normalization to lowercase
    /// 
    /// Example usage:
    /// <code>
    /// var checksumResult = FileChecksum.Create("A1B2C3D4E5F67890123456789ABCDEF0123456789ABCDEF0123456789ABCDEF01");
    /// if (checksumResult.IsSuccess)
    /// {
    ///     var checksum = checksumResult.Value;
    ///     Console.WriteLine($"Valid checksum: {checksum.Value}");
    /// }
    /// </code>
    /// </remarks>
    public static Result<FileChecksum> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_REQUIRED",
                "File checksum is required and cannot be empty"));

        var trimmedValue = value.Trim().ToLowerInvariant();

        if (trimmedValue.Length != Sha256HexLength)
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_INVALID_LENGTH",
                $"File checksum must be exactly {Sha256HexLength} characters long (SHA-256 format)"));

        if (!Sha256Regex.IsMatch(trimmedValue))
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_INVALID_FORMAT",
                "File checksum must contain only hexadecimal characters (0-9, a-f)"));

        return Result.Success(new FileChecksum(trimmedValue));
    }

    /// <summary>
    /// Creates a <see cref="FileChecksum"/> from a trusted source without validation.
    /// </summary>
    /// <param name="value">The checksum value from a trusted source.</param>
    /// <returns>A new <see cref="FileChecksum"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is null or empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>WARNING:</strong> This method bypasses validation and should only be used
    /// when the checksum value is known to be valid and properly formatted, such as
    /// when loading from a database or trusted configuration source.
    /// </para>
    /// 
    /// The value is still normalized to lowercase for consistency, but no format
    /// or length validation is performed.
    /// 
    /// Example usage:
    /// <code>
    /// // Loading from database where checksum is known to be valid
    /// var checksum = FileChecksum.FromTrustedSource(dbRecord.Checksum);
    /// </code>
    /// </remarks>
    public static FileChecksum FromTrustedSource(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value), "Checksum value cannot be null or empty");

        return new(value.ToLowerInvariant());
    }

    /// <summary>
    /// Computes the SHA-256 checksum of the provided byte array.
    /// </summary>
    /// <param name="data">The data to compute the checksum for.</param>
    /// <returns>
    /// A <see cref="Result{FileChecksum}"/> containing the computed checksum on success,
    /// or error information if the computation fails.
    /// </returns>
    /// <remarks>
    /// This method provides a convenient way to compute checksums for in-memory data.
    /// It uses the system's SHA-256 implementation for cryptographic integrity.
    /// 
    /// Example usage:
    /// <code>
    /// byte[] fileData = File.ReadAllBytes("document.pdf");
    /// var checksumResult = FileChecksum.ComputeFromData(fileData);
    /// if (checksumResult.IsSuccess)
    /// {
    ///     var checksum = checksumResult.Value;
    ///     Console.WriteLine($"Computed checksum: {checksum}");
    /// }
    /// </code>
    /// </remarks>
    public static Result<FileChecksum> ComputeFromData(byte[] data)
    {
        if (data == null)
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_NULL_DATA",
                "Cannot compute checksum from null data"));

        if (data.Length == 0)
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_EMPTY_DATA",
                "Cannot compute checksum from empty data"));

        try
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(data);
            var hashString = Convert.ToHexString(hashBytes).ToLowerInvariant();

            return Result.Success(new FileChecksum(hashString));
        }
        catch (Exception ex)
        {
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_COMPUTATION_FAILED",
                $"Failed to compute checksum: {ex.Message}"));
        }
    }

    /// <summary>
    /// Asynchronously computes the SHA-256 checksum of the provided stream.
    /// </summary>
    /// <param name="stream">The stream to compute the checksum for.</param>
    /// <returns>
    /// A <see cref="Task{Result}"/> that represents the asynchronous computation,
    /// containing the computed checksum on success or error information if the computation fails.
    /// </returns>
    /// <remarks>
    /// This method is optimized for computing checksums of large files without loading
    /// the entire file into memory. The stream position is not modified by this operation.
    /// 
    /// The stream must be readable and positioned at the beginning of the data to be hashed.
    /// If you need to preserve the original stream position, save it before calling this method.
    /// 
    /// Example usage:
    /// <code>
    /// using var fileStream = File.OpenRead("large-document.pdf");
    /// var checksumResult = await FileChecksum.ComputeFromStreamAsync(fileStream);
    /// if (checksumResult.IsSuccess)
    /// {
    ///     var checksum = checksumResult.Value;
    ///     Console.WriteLine($"Computed checksum: {checksum}");
    /// }
    /// </code>
    /// </remarks>
    public static async Task<Result<FileChecksum>> ComputeFromStreamAsync(Stream stream)
    {
        if (stream == null)
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_NULL_STREAM",
                "Cannot compute checksum from null stream"));

        if (!stream.CanRead)
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_STREAM_NOT_READABLE",
                "Cannot compute checksum from a non-readable stream"));

        try
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            var hashString = Convert.ToHexString(hashBytes).ToLowerInvariant();

            return Result.Success(new FileChecksum(hashString));
        }
        catch (Exception ex)
        {
            return Result.Failure<FileChecksum>(DomainError.Create(
                "CHECKSUM_COMPUTATION_FAILED",
                $"Failed to compute checksum from stream: {ex.Message}"));
        }
    }

    /// <summary>
    /// Verifies that the provided byte array matches this checksum.
    /// </summary>
    /// <param name="data">The data to verify against this checksum.</param>
    /// <returns>
    /// <c>true</c> if the computed checksum of the data matches this checksum; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method computes the SHA-256 hash of the provided data and compares it
    /// with this checksum value. It returns false if the data is null, empty, or
    /// if the checksum computation fails for any reason.
    /// 
    /// Example usage:
    /// <code>
    /// byte[] originalData = GetDocumentData();
    /// var checksum = FileChecksum.ComputeFromData(originalData).Value;
    /// 
    /// byte[] retrievedData = RetrieveDocumentData();
    /// bool isIntact = checksum.VerifyData(retrievedData);
    /// if (!isIntact)
    /// {
    ///     Console.WriteLine("Document integrity compromised!");
    /// }
    /// </code>
    /// </remarks>
    public bool VerifyData(byte[] data)
    {
        var computedResult = ComputeFromData(data);
        return computedResult.IsSuccess &&
               string.Equals(computedResult.Value.Value, Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Asynchronously verifies that the provided stream content matches this checksum.
    /// </summary>
    /// <param name="stream">The stream to verify against this checksum.</param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> representing the asynchronous verification operation.
    /// The result is <c>true</c> if the computed checksum matches this checksum; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method computes the SHA-256 hash of the stream content and compares it
    /// with this checksum value. It returns false if the stream is null, not readable,
    /// or if the checksum computation fails for any reason.
    /// 
    /// The stream position may be modified during verification. If you need to preserve
    /// the original position, save it before calling this method and restore it afterward.
    /// 
    /// Example usage:
    /// <code>
    /// var originalChecksum = await FileChecksum.ComputeFromStreamAsync(originalStream);
    /// 
    /// using var retrievedStream = OpenRetrievedDocument();
    /// bool isIntact = await originalChecksum.Value.VerifyStreamAsync(retrievedStream);
    /// if (!isIntact)
    /// {
    ///     Console.WriteLine("Document integrity compromised!");
    /// }
    /// </code>
    /// </remarks>
    public async Task<bool> VerifyStreamAsync(Stream stream)
    {
        var computedResult = await ComputeFromStreamAsync(stream);
        return computedResult.IsSuccess &&
               string.Equals(computedResult.Value.Value, Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Implicitly converts a <see cref="FileChecksum"/> to its string representation.
    /// </summary>
    /// <param name="checksum">The FileChecksum to convert.</param>
    /// <returns>The checksum value as a string, or null if the checksum is null.</returns>
    /// <remarks>
    /// This implicit conversion allows FileChecksum instances to be used seamlessly
    /// where string values are expected, such as in logging, database operations,
    /// or API responses.
    /// 
    /// Example usage:
    /// <code>
    /// var checksum = FileChecksum.Create("abc123...").Value;
    /// string checksumString = checksum; // Implicit conversion
    /// </code>
    /// </remarks>
    public static implicit operator string(FileChecksum checksum) => checksum.Value;

    /// <summary>
    /// Returns a string representation of this file checksum.
    /// </summary>
    /// <returns>
    /// The SHA-256 checksum value as a lowercase hexadecimal string.
    /// </returns>
    /// <remarks>
    /// This method returns the same value as the <see cref="Value"/> property
    /// and is suitable for display, logging, and serialization purposes.
    /// </remarks>
    public override string ToString() => Value;
}