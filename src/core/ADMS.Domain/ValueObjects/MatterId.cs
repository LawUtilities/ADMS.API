using System;

namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a unique identifier for a legal matter in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// This value object encapsulates a GUID-based identifier for legal matters, providing type safety
/// and preventing accidental mixing of different entity identifiers. Legal matters represent
/// client cases, projects, or legal engagements that contain related documents and activities.
/// 
/// The MatterId is immutable and provides validation to ensure the identifier is never empty
/// or invalid. As a value object, MatterId instances are compared by value rather than reference,
/// ensuring that two MatterId instances with the same GUID value are considered equal.
/// 
/// This strongly-typed identifier helps prevent common programming errors such as passing
/// a DocumentId where a MatterId was expected, improving code reliability and maintaining
/// the integrity of matter-document relationships in the legal document management system.
/// </remarks>
public sealed record MatterId
{
    /// <summary>
    /// Gets the underlying GUID value of this matter identifier.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> that uniquely identifies a legal matter in the system.
    /// </value>
    /// <remarks>
    /// This property provides access to the underlying GUID value while maintaining
    /// encapsulation. The value is guaranteed to never be <see cref="Guid.Empty"/>
    /// due to validation performed during construction.
    /// 
    /// The GUID format ensures global uniqueness across all legal matters,
    /// supporting distributed systems and data migrations.
    /// </remarks>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MatterId"/> record with the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value for this matter identifier.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <remarks>
    /// This constructor is private to enforce the use of factory methods for creating instances.
    /// This ensures consistent validation and provides controlled ways to create MatterId instances.
    /// </remarks>
    private MatterId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Matter ID cannot be empty", nameof(value));
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="MatterId"/> with a randomly generated GUID.
    /// </summary>
    /// <returns>
    /// A new <see cref="MatterId"/> instance with a unique identifier.
    /// </returns>
    /// <remarks>
    /// This method is the primary way to create new matter identifiers when
    /// creating new legal matters. It uses <see cref="Guid.NewGuid()"/> to ensure
    /// the identifier is unique across all systems and time.
    /// 
    /// Example usage:
    /// <code>
    /// var matterId = MatterId.New();
    /// var newMatter = Matter.Create(matterId, "Smith vs. Johnson", clientId);
    /// </code>
    /// </remarks>
    public static MatterId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a <see cref="MatterId"/> from an existing GUID value.
    /// </summary>
    /// <param name="value">The GUID value to convert to a MatterId.</param>
    /// <returns>
    /// A new <see cref="MatterId"/> instance wrapping the provided GUID.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <remarks>
    /// Use this method when you have an existing GUID (e.g., from a database query,
    /// external system integration, or API request) that you need to convert to a
    /// strongly-typed MatterId.
    /// 
    /// Example usage:
    /// <code>
    /// var guid = Guid.Parse("12345678-1234-1234-1234-123456789ABC");
    /// var matterId = MatterId.From(guid);
    /// var matter = await matterRepository.GetByIdAsync(matterId);
    /// </code>
    /// </remarks>
    public static MatterId From(Guid value) => new(value);

    /// <summary>
    /// Creates a <see cref="MatterId"/> from a string representation of a GUID.
    /// </summary>
    /// <param name="value">The string representation of the GUID.</param>
    /// <returns>
    /// A new <see cref="MatterId"/> instance if parsing is successful.
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
    /// This is commonly used when parsing matter identifiers from URLs,
    /// configuration files, or user input in administrative interfaces.
    /// 
    /// Example usage:
    /// <code>
    /// // From API route parameter
    /// var matterId = MatterId.From(Request.RouteValues["matterId"].ToString());
    /// 
    /// // From configuration
    /// var defaultMatterId = MatterId.From(configuration["DefaultMatterId"]);
    /// </code>
    /// </remarks>
    public static MatterId From(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid GUID format: {value}", nameof(value));

        return new MatterId(guid);
    }

    /// <summary>
    /// Implicitly converts a <see cref="MatterId"/> to its underlying <see cref="Guid"/> value.
    /// </summary>
    /// <param name="id">The MatterId to convert.</param>
    /// <returns>The underlying GUID value, or <see cref="Guid.Empty"/> if the id is null.</returns>
    /// <remarks>
    /// This implicit conversion allows MatterId instances to be used seamlessly
    /// where GUID values are expected, such as in Entity Framework queries,
    /// database parameters, or when interfacing with external APIs that expect GUID parameters.
    /// 
    /// Example usage:
    /// <code>
    /// MatterId matterId = MatterId.New();
    /// Guid guid = matterId; // Implicit conversion
    /// 
    /// // Use in Entity Framework query
    /// var documents = await context.Documents
    ///     .Where(d => d.MatterId == matterId) // Implicit conversion
    ///     .ToListAsync();
    /// </code>
    /// </remarks>
    public static implicit operator Guid(MatterId id) => id?.Value ?? Guid.Empty;

    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="MatterId"/>.
    /// </summary>
    /// <param name="value">The GUID value to convert.</param>
    /// <returns>A new MatterId wrapping the GUID value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <remarks>
    /// This implicit conversion allows GUID values to be automatically converted
    /// to MatterId instances when needed, providing convenience while maintaining
    /// type safety. This is particularly useful when working with database results
    /// or external API responses.
    /// 
    /// Example usage:
    /// <code>
    /// Guid guidFromDatabase = dataReader.GetGuid("MatterId");
    /// MatterId matterId = guidFromDatabase; // Implicit conversion
    /// 
    /// // Use in business logic
    /// var matterDocuments = await documentService.GetByMatterAsync(matterId);
    /// </code>
    /// </remarks>
    public static implicit operator MatterId(Guid value) => From(value);

    /// <summary>
    /// Returns a string representation of this matter identifier.
    /// </summary>
    /// <returns>
    /// A string representation of the underlying GUID value in standard format.
    /// </returns>
    /// <remarks>
    /// The returned string uses the standard GUID format with hyphens:
    /// "12345678-1234-1234-1234-123456789ABC"
    /// 
    /// This method is useful for logging, debugging, displaying the identifier
    /// in user interfaces, and serialization for APIs or configuration files.
    /// 
    /// Example usage:
    /// <code>
    /// var matterId = MatterId.New();
    /// logger.LogInformation("Processing matter {MatterId}", matterId.ToString());
    /// 
    /// // For display in UI
    /// Console.WriteLine($"Matter ID: {matterId}");
    /// </code>
    /// </remarks>
    public override string ToString() => Value.ToString();
}