using System;

namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a unique identifier for a document in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// This value object encapsulates a GUID-based identifier for documents, ensuring type safety
/// and preventing accidental mixing of different entity identifiers. The DocumentId is immutable
/// and provides validation to ensure the identifier is never empty or invalid.
/// 
/// As a value object, DocumentId instances are compared by value rather than reference,
/// and two DocumentId instances with the same GUID value are considered equal.
/// 
/// This strongly-typed identifier helps prevent common programming errors such as passing
/// a MatterId where a DocumentId was expected, improving code reliability and maintainability.
/// </remarks>
public sealed record DocumentId
{
    /// <summary>
    /// Gets the underlying GUID value of this document identifier.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> that uniquely identifies a document in the system.
    /// </value>
    /// <remarks>
    /// This property provides access to the underlying GUID value while maintaining
    /// encapsulation. The value is guaranteed to never be <see cref="Guid.Empty"/>
    /// due to validation performed during construction.
    /// </remarks>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentId"/> record with the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value for this document identifier.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <remarks>
    /// This constructor is private to enforce the use of factory methods for creating instances.
    /// This ensures consistent validation and provides a controlled way to create DocumentId instances.
    /// </remarks>
    private DocumentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty", nameof(value));
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="DocumentId"/> with a randomly generated GUID.
    /// </summary>
    /// <returns>
    /// A new <see cref="DocumentId"/> instance with a unique identifier.
    /// </returns>
    /// <remarks>
    /// This method is the primary way to create new document identifiers when
    /// creating new documents. It uses <see cref="Guid.NewGuid()"/> to ensure
    /// the identifier is unique across all systems and time.
    /// 
    /// Example usage:
    /// <code>
    /// var documentId = DocumentId.New();
    /// </code>
    /// </remarks>
    public static DocumentId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a <see cref="DocumentId"/> from an existing GUID value.
    /// </summary>
    /// <param name="value">The GUID value to convert to a DocumentId.</param>
    /// <returns>
    /// A new <see cref="DocumentId"/> instance wrapping the provided GUID.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <remarks>
    /// Use this method when you have an existing GUID (e.g., from a database query
    /// or external system) that you need to convert to a strongly-typed DocumentId.
    /// 
    /// Example usage:
    /// <code>
    /// var guid = Guid.Parse("12345678-1234-1234-1234-123456789ABC");
    /// var documentId = DocumentId.From(guid);
    /// </code>
    /// </remarks>
    public static DocumentId From(Guid value) => new(value);

    /// <summary>
    /// Creates a <see cref="DocumentId"/> from a string representation of a GUID.
    /// </summary>
    /// <param name="value">The string representation of the GUID.</param>
    /// <returns>
    /// A new <see cref="DocumentId"/> instance if parsing is successful.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is not a valid GUID format or represents <see cref="Guid.Empty"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is null.
    /// </exception>
    /// <remarks>
    /// This method accepts standard GUID string formats such as:
    /// - "12345678-1234-1234-1234-123456789ABC" (with hyphens)
    /// - "12345678123412341234123456789ABC" (without hyphens)
    /// - "{12345678-1234-1234-1234-123456789ABC}" (with braces)
    /// 
    /// Example usage:
    /// <code>
    /// var documentId = DocumentId.From("12345678-1234-1234-1234-123456789ABC");
    /// </code>
    /// </remarks>
    public static DocumentId From(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid GUID format: {value}", nameof(value));

        return new DocumentId(guid);
    }

    /// <summary>
    /// Implicitly converts a <see cref="DocumentId"/> to its underlying <see cref="Guid"/> value.
    /// </summary>
    /// <param name="id">The DocumentId to convert.</param>
    /// <returns>The underlying GUID value.</returns>
    /// <remarks>
    /// This implicit conversion allows DocumentId instances to be used seamlessly
    /// where GUID values are expected, such as in Entity Framework queries or
    /// when interfacing with APIs that expect GUID parameters.
    /// 
    /// Example usage:
    /// <code>
    /// DocumentId documentId = DocumentId.New();
    /// Guid guid = documentId; // Implicit conversion
    /// </code>
    /// </remarks>
    public static implicit operator Guid(DocumentId id) => id?.Value ?? Guid.Empty;

    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="DocumentId"/>.
    /// </summary>
    /// <param name="value">The GUID value to convert.</param>
    /// <returns>A new DocumentId wrapping the GUID value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <remarks>
    /// This implicit conversion allows GUID values to be automatically converted
    /// to DocumentId instances when needed, providing convenience while maintaining
    /// type safety.
    /// 
    /// Example usage:
    /// <code>
    /// Guid guid = Guid.NewGuid();
    /// DocumentId documentId = guid; // Implicit conversion
    /// </code>
    /// </remarks>
    public static implicit operator DocumentId(Guid value) => From(value);

    /// <summary>
    /// Returns a string representation of this document identifier.
    /// </summary>
    /// <returns>
    /// A string representation of the underlying GUID value.
    /// </returns>
    /// <remarks>
    /// The returned string uses the standard GUID format with hyphens:
    /// "12345678-1234-1234-1234-123456789ABC"
    /// 
    /// This method is useful for logging, debugging, and displaying the identifier
    /// in user interfaces.
    /// </remarks>
    public override string ToString() => Value.ToString();
}