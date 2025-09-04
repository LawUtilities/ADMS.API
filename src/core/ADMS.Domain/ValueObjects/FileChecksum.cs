using ADMS.Domain.Common;
using ADMS.Domain.Errors;

using System.Text.RegularExpressions;

namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a validated SHA256 file checksum for document integrity verification.
/// </summary>
/// <remarks>
/// This value object ensures that all checksums are valid SHA256 hashes in the correct format,
/// supporting legal document integrity verification and compliance requirements.
/// </remarks>
public sealed record FileChecksum
{
    private static readonly Regex Sha256Regex = new(@"^[a-f0-9]{64}$", RegexOptions.Compiled);

    public string Value { get; }

    private FileChecksum(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new FileChecksum value object with validation.
    /// </summary>
    /// <param name="value">The checksum to validate.</param>
    /// <returns>A Result containing the FileChecksum or validation errors.</returns>
    public static Result<FileChecksum> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<FileChecksum>(DocumentErrors.ChecksumRequired);

        var trimmedValue = value.Trim().ToLowerInvariant();

        if (trimmedValue.Length != 64)
            return Result.Failure<FileChecksum>(DocumentErrors.ChecksumInvalidLength);

        if (!Sha256Regex.IsMatch(trimmedValue))
            return Result.Failure<FileChecksum>(DocumentErrors.ChecksumInvalidFormat);

        return Result.Success(new FileChecksum(trimmedValue));
    }

    /// <summary>
    /// Creates a FileChecksum from a trusted source (e.g., database) without validation.
    /// </summary>
    /// <param name="value">The checksum value.</param>
    /// <returns>A FileChecksum instance.</returns>
    /// <remarks>
    /// Use this method only when the value is known to be valid (e.g., from database).
    /// </remarks>
    public static FileChecksum FromTrustedSource(string value) => new(value.ToLowerInvariant());

    /// <summary>
    /// Computes the SHA256 checksum of the provided data.
    /// </summary>
    /// <param name="data">The data to compute checksum for.</param>
    /// <returns>A Result containing the computed FileChecksum.</returns>
    public static Result<FileChecksum> ComputeFromData(byte[] data)
    {
        if (data == null || data.Length == 0)
            return Result.Failure<FileChecksum>(new DomainError("CHECKSUM_NO_DATA", "Cannot compute checksum from empty data"));

        try
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(data);
            var hashString = Convert.ToHexString(hashBytes).ToLowerInvariant();

            return Result.Success(new FileChecksum(hashString));
        }
        catch (Exception ex)
        {
            return Result.Failure<FileChecksum>(new DomainError("CHECKSUM_COMPUTATION_FAILED", $"Failed to compute checksum: {ex.Message}"));
        }
    }

    /// <summary>
    /// Computes the SHA256 checksum of the provided stream.
    /// </summary>
    /// <param name="stream">The stream to compute checksum for.</param>
    /// <returns>A Result containing the computed FileChecksum.</returns>
    public static async Task<Result<FileChecksum>> ComputeFromStreamAsync(Stream stream)
    {
        if (stream == null)
            return Result.Failure<FileChecksum>(new DomainError("CHECKSUM_NO_STREAM", "Cannot compute checksum from null stream"));

        if (!stream.CanRead)
            return Result.Failure<FileChecksum>(new DomainError("CHECKSUM_STREAM_NOT_READABLE", "Cannot read from stream"));

        try
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            var hashString = Convert.ToHexString(hashBytes).ToLowerInvariant();

            return Result.Success(new FileChecksum(hashString));
        }
        catch (Exception ex)
        {
            return Result.Failure<FileChecksum>(new DomainError("CHECKSUM_COMPUTATION_FAILED", $"Failed to compute checksum: {ex.Message}"));
        }
    }

    /// <summary>
    /// Verifies that the provided data matches this checksum.
    /// </summary>
    /// <param name="data">The data to verify.</param>
    /// <returns>True if the data matches the checksum, false otherwise.</returns>
    public bool VerifyData(byte[] data)
    {
        var computedResult = ComputeFromData(data);
        return computedResult.IsSuccess && computedResult.Value.Value.Equals(Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that the provided stream matches this checksum.
    /// </summary>
    /// <param name="stream">The stream to verify.</param>
    /// <returns>True if the stream matches the checksum, false otherwise.</returns>
    public async Task<bool> VerifyStreamAsync(Stream stream)
    {
        var computedResult = await ComputeFromStreamAsync(stream);
        return computedResult.IsSuccess && computedResult.Value.Value.Equals(Value, StringComparison.OrdinalIgnoreCase);
    }

    public static implicit operator string(FileChecksum checksum) => checksum.Value;

    public override string ToString() => Value;
}