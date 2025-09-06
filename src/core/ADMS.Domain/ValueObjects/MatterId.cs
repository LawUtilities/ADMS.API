using System.ComponentModel.DataAnnotations.Schema;

using ADMS.Domain.Common;

namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed matter identifier value object.
/// </summary>
/// <remarks>
/// MatterId provides type safety for matter identifiers, preventing accidental mixing
/// of different entity IDs and adding domain-specific validation and behavior.
/// 
/// <para><strong>Value Object Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Immutable:</strong> Once created, the value cannot be changed</item>
/// <item><strong>Type Safe:</strong> Prevents mixing of different entity identifiers</item>
/// <item><strong>Validated:</strong> Ensures only valid GUIDs are used</item>
/// <item><strong>Comparable:</strong> Supports equality and comparison operations</item>
/// </list>
/// 
/// <para><strong>Legal Document Management Context:</strong></para>
/// <list type="bullet">
/// <item>Ensures matter associations are type-safe and validated</item>
/// <item>Supports legal practice organization requirements</item>
/// <item>Provides clear domain intent for matter-related operations</item>
/// <item>Enables compile-time validation of matter references</item>
/// </list>
/// </remarks>
[ComplexType]
public sealed record MatterId : IComparable<MatterId>
{
    /// <summary>
    /// Gets the underlying GUID value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the MatterId with the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value for the matter identifier.</param>
    /// <exception cref="ArgumentException">Thrown when the GUID is empty.</exception>
    private MatterId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Matter ID cannot be empty", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Creates a new MatterId from the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value to create the MatterId from.</param>
    /// <returns>A new MatterId instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the GUID is empty.</exception>
    /// <example>
    /// <code>
    /// var matterId = MatterId.From(Guid.NewGuid());
    /// var existingId = MatterId.From(Guid.Parse("60000000-0000-0000-0000-000000000001"));
    /// </code>
    /// </example>
    public static MatterId From(Guid value) => new(value);

    /// <summary>
    /// Creates a new MatterId with a new GUID value.
    /// </summary>
    /// <returns>A new MatterId instance with a unique identifier.</returns>
    /// <example>
    /// <code>
    /// var newMatterId = MatterId.New();
    /// </code>
    /// </example>
    public static MatterId New() => new(Guid.NewGuid());

    /// <summary>
    /// Attempts to parse a string into a MatterId.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>A Result containing either the parsed MatterId or an error.</returns>
    /// <example>
    /// <code>
    /// var result = MatterId.TryParse("60000000-0000-0000-0000-000000000001");
    /// if (result.IsSuccess)
    /// {
    ///     var matterId = result.Value;
    ///     // Use the matter ID
    /// }
    /// </code>
    /// </example>
    public static Result<MatterId> TryParse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<MatterId>(DomainError.Create(
                "MATTER_ID_NULL_OR_EMPTY",
                "Matter ID cannot be null or empty"));

        if (!Guid.TryParse(value, out var guid))
            return Result.Failure<MatterId>(DomainError.Create(
                "MATTER_ID_INVALID_FORMAT",
                $"'{value}' is not a valid matter ID format"));

        if (guid == Guid.Empty)
            return Result.Failure<MatterId>(DomainError.Create(
                "MATTER_ID_EMPTY",
                "Matter ID cannot be empty"));

        return Result.Success(new MatterId(guid));
    }

    /// <summary>
    /// Validates whether the specified GUID can be used as a MatterId.
    /// </summary>
    /// <param name="value">The GUID to validate.</param>
    /// <returns>True if the GUID is valid for use as a MatterId; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var isValid = MatterId.IsValid(someGuid);
    /// if (isValid)
    /// {
    ///     var matterId = MatterId.From(someGuid);
    /// }
    /// </code>
    /// </example>
    public static bool IsValid(Guid value) => value != Guid.Empty;

    /// <summary>
    /// Validates whether the specified string can be parsed as a MatterId.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns>True if the string is a valid MatterId; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var isValid = MatterId.IsValid("60000000-0000-0000-0000-000000000001");
    /// </code>
    /// </example>
    public static bool IsValid(string? value) => TryParse(value).IsSuccess;

    #region Equality and Comparison

    /// <summary>
    /// Compares this MatterId with another MatterId.
    /// </summary>
    /// <param name="other">The other MatterId to compare with.</param>
    /// <returns>A value indicating the relative order of the MatterIds.</returns>
    public int CompareTo(MatterId? other) => other is null ? 1 : Value.CompareTo(other.Value);

    /// <summary>
    /// Determines whether this MatterId is equal to another MatterId.
    /// </summary>
    /// <param name="other">The other MatterId to compare with.</param>
    /// <returns>True if the MatterIds are equal; otherwise, false.</returns>
    public bool Equals(MatterId? other) => other is not null && Value.Equals(other.Value);

    /// <summary>
    /// Returns the hash code for this MatterId.
    /// </summary>
    /// <returns>A hash code for this MatterId.</returns>
    public override int GetHashCode() => Value.GetHashCode();

    #endregion

    #region Operators

    /// <summary>
    /// Implicitly converts a MatterId to a Guid.
    /// </summary>
    /// <param name="matterId">The MatterId to convert.</param>
    /// <returns>The underlying Guid value.</returns>
    public static implicit operator Guid(MatterId matterId) => matterId.Value;

    /// <summary>
    /// Explicitly converts a Guid to a MatterId.
    /// </summary>
    /// <param name="value">The Guid value to convert.</param>
    /// <returns>A new MatterId instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the GUID is empty.</exception>
    public static explicit operator MatterId(Guid value) => From(value);

    /// <summary>
    /// Determines whether one MatterId is less than another.
    /// </summary>
    /// <param name="left">The first MatterId to compare.</param>
    /// <param name="right">The second MatterId to compare.</param>
    /// <returns>True if the first MatterId is less than the second; otherwise, false.</returns>
    public static bool operator <(MatterId? left, MatterId? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether one MatterId is less than or equal to another.
    /// </summary>
    /// <param name="left">The first MatterId to compare.</param>
    /// <param name="right">The second MatterId to compare.</param>
    /// <returns>True if the first MatterId is less than or equal to the second; otherwise, false.</returns>
    public static bool operator <=(MatterId? left, MatterId? right) =>
        left is null || left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether one MatterId is greater than another.
    /// </summary>
    /// <param name="left">The first MatterId to compare.</param>
    /// <param name="right">The second MatterId to compare.</param>
    /// <returns>True if the first MatterId is greater than the second; otherwise, false.</returns>
    public static bool operator >(MatterId? left, MatterId? right) =>
        left is not null && left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether one MatterId is greater than or equal to another.
    /// </summary>
    /// <param name="left">The first MatterId to compare.</param>
    /// <param name="right">The second MatterId to compare.</param>
    /// <returns>True if the first MatterId is greater than or equal to the second; otherwise, false.</returns>
    public static bool operator >=(MatterId? left, MatterId? right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterId.
    /// </summary>
    /// <returns>The string representation of the underlying GUID.</returns>
    /// <example>
    /// <code>
    /// var matterId = MatterId.New();
    /// var idString = matterId.ToString(); // e.g., "60000000-0000-0000-0000-000000000001"
    /// </code>
    /// </example>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Returns a string representation of the MatterId using the specified format.
    /// </summary>
    /// <param name="format">The format string to use.</param>
    /// <returns>The formatted string representation of the underlying GUID.</returns>
    /// <example>
    /// <code>
    /// var matterId = MatterId.New();
    /// var shortId = matterId.ToString("N"); // No hyphens
    /// var bracketed = matterId.ToString("B"); // With braces
    /// </code>
    /// </example>
    public string ToString(string? format) => Value.ToString(format);

    #endregion
}