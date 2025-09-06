using System.ComponentModel.DataAnnotations.Schema;

using ADMS.Domain.Common;

namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed user identifier value object.
/// </summary>
/// <remarks>
/// UserId provides type safety for user identifiers, preventing accidental mixing
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
/// <para><strong>Professional Legal Practice Context:</strong></para>
/// <list type="bullet">
/// <item>Ensures user attributions are type-safe and validated</item>
/// <item>Supports audit trail integrity and accountability</item>
/// <item>Provides clear domain intent for user-related operations</item>
/// <item>Enables compile-time validation of user references</item>
/// </list>
/// </remarks>
[ComplexType]
public sealed record UserId : IComparable<UserId>
{
    /// <summary>
    /// Gets the underlying GUID value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the UserId with the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value for the user identifier.</param>
    /// <exception cref="ArgumentException">Thrown when the GUID is empty.</exception>
    private UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Creates a new UserId from the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value to create the UserId from.</param>
    /// <returns>A new UserId instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the GUID is empty.</exception>
    /// <example>
    /// <code>
    /// var userId = UserId.From(Guid.NewGuid());
    /// var existingId = UserId.From(Guid.Parse("50000000-0000-0000-0000-000000000001"));
    /// </code>
    /// </example>
    public static UserId From(Guid value) => new(value);

    /// <summary>
    /// Creates a new UserId with a new GUID value.
    /// </summary>
    /// <returns>A new UserId instance with a unique identifier.</returns>
    /// <example>
    /// <code>
    /// var newUserId = UserId.New();
    /// </code>
    /// </example>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>
    /// Attempts to parse a string into a UserId.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>A Result containing either the parsed UserId or an error.</returns>
    /// <example>
    /// <code>
    /// var result = UserId.TryParse("50000000-0000-0000-0000-000000000001");
    /// if (result.IsSuccess)
    /// {
    ///     var userId = result.Value;
    ///     // Use the user ID
    /// }
    /// </code>
    /// </example>
    public static Result<UserId> TryParse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<UserId>(DomainError.Create(
                "USER_ID_NULL_OR_EMPTY",
                "User ID cannot be null or empty"));

        if (!Guid.TryParse(value, out var guid))
            return Result.Failure<UserId>(DomainError.Create(
                "USER_ID_INVALID_FORMAT",
                $"'{value}' is not a valid user ID format"));

        if (guid == Guid.Empty)
            return Result.Failure<UserId>(DomainError.Create(
                "USER_ID_EMPTY",
                "User ID cannot be empty"));

        return Result.Success(new UserId(guid));
    }

    /// <summary>
    /// Validates whether the specified GUID can be used as a UserId.
    /// </summary>
    /// <param name="value">The GUID to validate.</param>
    /// <returns>True if the GUID is valid for use as a UserId; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var isValid = UserId.IsValid(someGuid);
    /// if (isValid)
    /// {
    ///     var userId = UserId.From(someGuid);
    /// }
    /// </code>
    /// </example>
    public static bool IsValid(Guid value) => value != Guid.Empty;

    /// <summary>
    /// Validates whether the specified string can be parsed as a UserId.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns>True if the string is a valid UserId; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var isValid = UserId.IsValid("50000000-0000-0000-0000-000000000001");
    /// </code>
    /// </example>
    public static bool IsValid(string? value) => TryParse(value).IsSuccess;

    #region Equality and Comparison

    /// <summary>
    /// Compares this UserId with another UserId.
    /// </summary>
    /// <param name="other">The other UserId to compare with.</param>
    /// <returns>A value indicating the relative order of the UserIds.</returns>
    public int CompareTo(UserId? other) => other is null ? 1 : Value.CompareTo(other.Value);

    /// <summary>
    /// Determines whether this UserId is equal to another UserId.
    /// </summary>
    /// <param name="other">The other UserId to compare with.</param>
    /// <returns>True if the UserIds are equal; otherwise, false.</returns>
    public bool Equals(UserId? other) => other is not null && Value.Equals(other.Value);

    /// <summary>
    /// Returns the hash code for this UserId.
    /// </summary>
    /// <returns>A hash code for this UserId.</returns>
    public override int GetHashCode() => Value.GetHashCode();

    #endregion

    #region Operators

    /// <summary>
    /// Implicitly converts a UserId to a Guid.
    /// </summary>
    /// <param name="userId">The UserId to convert.</param>
    /// <returns>The underlying Guid value.</returns>
    public static implicit operator Guid(UserId userId) => userId.Value;

    /// <summary>
    /// Explicitly converts a Guid to a UserId.
    /// </summary>
    /// <param name="value">The Guid value to convert.</param>
    /// <returns>A new UserId instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the GUID is empty.</exception>
    public static explicit operator UserId(Guid value) => From(value);

    /// <summary>
    /// Determines whether one UserId is less than another.
    /// </summary>
    /// <param name="left">The first UserId to compare.</param>
    /// <param name="right">The second UserId to compare.</param>
    /// <returns>True if the first UserId is less than the second; otherwise, false.</returns>
    public static bool operator <(UserId? left, UserId? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether one UserId is less than or equal to another.
    /// </summary>
    /// <param name="left">The first UserId to compare.</param>
    /// <param name="right">The second UserId to compare.</param>
    /// <returns>True if the first UserId is less than or equal to the second; otherwise, false.</returns>
    public static bool operator <=(UserId? left, UserId? right) =>
        left is null || left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether one UserId is greater than another.
    /// </summary>
    /// <param name="left">The first UserId to compare.</param>
    /// <param name="right">The second UserId to compare.</param>
    /// <returns>True if the first UserId is greater than the second; otherwise, false.</returns>
    public static bool operator >(UserId? left, UserId? right) =>
        left is not null && left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether one UserId is greater than or equal to another.
    /// </summary>
    /// <param name="left">The first UserId to compare.</param>
    /// <param name="right">The second UserId to compare.</param>
    /// <returns>True if the first UserId is greater than or equal to the second; otherwise, false.</returns>
    public static bool operator >=(UserId? left, UserId? right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the UserId.
    /// </summary>
    /// <returns>The string representation of the underlying GUID.</returns>
    /// <example>
    /// <code>
    /// var userId = UserId.New();
    /// var idString = userId.ToString(); // e.g., "50000000-0000-0000-0000-000000000001"
    /// </code>
    /// </example>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Returns a string representation of the UserId using the specified format.
    /// </summary>
    /// <param name="format">The format string to use.</param>
    /// <returns>The formatted string representation of the underlying GUID.</returns>
    /// <example>
    /// <code>
    /// var userId = UserId.New();
    /// var shortId = userId.ToString("N"); // No hyphens
    /// var bracketed = userId.ToString("B"); // With braces
    /// </code>
    /// </example>
    public string ToString(string? format) => Value.ToString(format);

    #endregion
}