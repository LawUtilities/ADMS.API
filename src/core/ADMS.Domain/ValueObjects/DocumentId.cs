using System.ComponentModel.DataAnnotations.Schema;

using ADMS.Domain.Common;

namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed document identifier value object.
/// </summary>
/// <remarks>
/// DocumentId provides type safety for document identifiers, preventing accidental mixing
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
/// <para><strong>Benefits:</strong></para>
/// <list type="bullet">
/// <item>Compile-time type safety prevents ID mixing errors</item>
/// <item>Clear domain intent in method signatures</item>
/// <item>Centralized validation logic</item>
/// <item>Enhanced debugging and code readability</item>
/// </list>
/// </remarks>
[ComplexType]
public sealed record DocumentId : IComparable<DocumentId>
{
    /// <summary>
    /// Gets the underlying GUID value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the DocumentId with the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value for the document identifier.</param>
    /// <exception cref="ArgumentException">Thrown when the GUID is empty.</exception>
    private DocumentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Creates a new DocumentId from the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value to create the DocumentId from.</param>
    /// <returns>A new DocumentId instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the GUID is empty.</exception>
    /// <example>
    /// <code>
    /// var documentId = DocumentId.From(Guid.NewGuid());
    /// var existingId = DocumentId.From(Guid.Parse("12345678-1234-5678-9012-123456789012"));
    /// </code>
    /// </example>
    public static DocumentId From(Guid value) => new(value);

    /// <summary>
    /// Creates a new DocumentId with a new GUID value.
    /// </summary>
    /// <returns>A new DocumentId instance with a unique identifier.</returns>
    /// <example>
    /// <code>
    /// var newDocumentId = DocumentId.New();
    /// </code>
    /// </example>
    public static DocumentId New() => new(Guid.NewGuid());

    /// <summary>
    /// Attempts to parse a string into a DocumentId.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>A Result containing either the parsed DocumentId or an error.</returns>
    /// <example>
    /// <code>
    /// var result = DocumentId.TryParse("12345678-1234-5678-9012-123456789012");
    /// if (result.IsSuccess)
    /// {
    ///     var documentId = result.Value;
    ///     // Use the document ID
    /// }
    /// </code>
    /// </example>
    public static Result<DocumentId> TryParse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<DocumentId>(DomainError.Create(
                "DOCUMENT_ID_NULL_OR_EMPTY",
                "Document ID cannot be null or empty"));

        if (!Guid.TryParse(value, out var guid))
            return Result.Failure<DocumentId>(DomainError.Create(
                "DOCUMENT_ID_INVALID_FORMAT",
                $"'{value}' is not a valid document ID format"));

        if (guid == Guid.Empty)
            return Result.Failure<DocumentId>(DomainError.Create(
                "DOCUMENT_ID_EMPTY",
                "Document ID cannot be empty"));

        return Result.Success(new DocumentId(guid));
    }

    /// <summary>
    /// Validates whether the specified GUID can be used as a DocumentId.
    /// </summary>
    /// <param name="value">The GUID to validate.</param>
    /// <returns>True if the GUID is valid for use as a DocumentId; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var isValid = DocumentId.IsValid(someGuid);
    /// if (isValid)
    /// {
    ///     var documentId = DocumentId.From(someGuid);
    /// }
    /// </code>
    /// </example>
    public static bool IsValid(Guid value) => value != Guid.Empty;

    /// <summary>
    /// Validates whether the specified string can be parsed as a DocumentId.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns>True if the string is a valid DocumentId; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// var isValid = DocumentId.IsValid("12345678-1234-5678-9012-123456789012");
    /// </code>
    /// </example>
    public static bool IsValid(string? value) => TryParse(value).IsSuccess;

    #region Equality and Comparison

    /// <summary>
    /// Compares this DocumentId with another DocumentId.
    /// </summary>
    /// <param name="other">The other DocumentId to compare with.</param>
    /// <returns>A value indicating the relative order of the DocumentIds.</returns>
    public int CompareTo(DocumentId? other) => other is null ? 1 : Value.CompareTo(other.Value);

    /// <summary>
    /// Determines whether this DocumentId is equal to another DocumentId.
    /// </summary>
    /// <param name="other">The other DocumentId to compare with.</param>
    /// <returns>True if the DocumentIds are equal; otherwise, false.</returns>
    public bool Equals(DocumentId? other) => other is not null && Value.Equals(other.Value);

    /// <summary>
    /// Returns the hash code for this DocumentId.
    /// </summary>
    /// <returns>A hash code for this DocumentId.</returns>
    public override int GetHashCode() => Value.GetHashCode();

    #endregion

    #region Operators

    /// <summary>
    /// Implicitly converts a DocumentId to a Guid.
    /// </summary>
    /// <param name="documentId">The DocumentId to convert.</param>
    /// <returns>The underlying Guid value.</returns>
    public static implicit operator Guid(DocumentId documentId) => documentId.Value;

    /// <summary>
    /// Explicitly converts a Guid to a DocumentId.
    /// </summary>
    /// <param name="value">The Guid value to convert.</param>
    /// <returns>A new DocumentId instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the GUID is empty.</exception>
    public static explicit operator DocumentId(Guid value) => From(value);

    /// <summary>
    /// Determines whether one DocumentId is less than another.
    /// </summary>
    /// <param name="left">The first DocumentId to compare.</param>
    /// <param name="right">The second DocumentId to compare.</param>
    /// <returns>True if the first DocumentId is less than the second; otherwise, false.</returns>
    public static bool operator <(DocumentId? left, DocumentId? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether one DocumentId is less than or equal to another.
    /// </summary>
    /// <param name="left">The first DocumentId to compare.</param>
    /// <param name="right">The second DocumentId to compare.</param>
    /// <returns>True if the first DocumentId is less than or equal to the second; otherwise, false.</returns>
    public static bool operator <=(DocumentId? left, DocumentId? right) =>
        left is null || left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether one DocumentId is greater than another.
    /// </summary>
    /// <param name="left">The first DocumentId to compare.</param>
    /// <param name="right">The second DocumentId to compare.</param>
    /// <returns>True if the first DocumentId is greater than the second; otherwise, false.</returns>
    public static bool operator >(DocumentId? left, DocumentId? right) =>
        left is not null && left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether one DocumentId is greater than or equal to another.
    /// </summary>
    /// <param name="left">The first DocumentId to compare.</param>
    /// <param name="right">The second DocumentId to compare.</param>
    /// <returns>True if the first DocumentId is greater than or equal to the second; otherwise, false.</returns>
    public static bool operator >=(DocumentId? left, DocumentId? right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the DocumentId.
    /// </summary>
    /// <returns>The string representation of the underlying GUID.</returns>
    /// <example>
    /// <code>
    /// var documentId = DocumentId.New();
    /// var idString = documentId.ToString(); // e.g., "12345678-1234-5678-9012-123456789012"
    /// </code>
    /// </example>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Returns a string representation of the DocumentId using the specified format.
    /// </summary>
    /// <param name="format">The format string to use.</param>
    /// <returns>The formatted string representation of the underlying GUID.</returns>
    /// <example>
    /// <code>
    /// var documentId = DocumentId.New();
    /// var shortId = documentId.ToString("N"); // No hyphens
    /// var bracketed = documentId.ToString("B"); // With braces
    /// </code>
    /// </example>
    public string ToString(string? format) => Value.ToString(format);

    #endregion
}