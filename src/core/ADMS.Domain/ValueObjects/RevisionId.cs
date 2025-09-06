using System;

namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a unique identifier for a document revision in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// This value object encapsulates a GUID-based identifier for document revisions, ensuring type safety
/// and preventing accidental mixing of different entity identifiers. Document revisions represent
/// specific versions or iterations of legal documents, maintaining a complete audit trail of changes
/// over time.
/// 
/// The RevisionId is immutable and provides validation to ensure the identifier is never empty
/// or invalid. As a value object, RevisionId instances are compared by value rather than reference,
/// ensuring that two RevisionId instances with the same GUID value are considered equal.
/// 
/// This strongly-typed identifier is crucial for maintaining document version integrity and
/// supports legal compliance requirements by ensuring each revision can be uniquely identified
/// and tracked throughout the document lifecycle.
/// </remarks>
public sealed record RevisionId
{
    /// <summary>
    /// Gets the underlying GUID value of this revision identifier.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> that uniquely identifies a document revision in the system.
    /// </value>
    /// <remarks>
    /// This property provides access to the underlying GUID value while maintaining
    /// encapsulation. The value is guaranteed to never be <see cref="Guid.Empty"/>
    /// due to validation performed during construction.
    /// 
    /// Each revision identifier is globally unique, enabling precise tracking of
    /// document changes across distributed systems and ensuring audit trail integrity
    /// required for legal document management.
    /// </remarks>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RevisionId"/> record with the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value for this revision identifier.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <remarks>
    /// This constructor is private to enforce the use of factory methods for creating instances.
    /// This ensures consistent validation and provides controlled ways to create RevisionId instances,
    /// maintaining the integrity of revision tracking throughout the system.
    /// </remarks>
    private RevisionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Revision ID cannot be empty", nameof(value));
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="RevisionId"/> with a randomly generated GUID.
    /// </summary>
    /// <returns>
    /// A new <see cref="RevisionId"/> instance with a unique identifier.
    /// </returns>
    /// <remarks>
    /// This method is the primary way to create new revision identifiers when
    /// creating new document revisions. It uses <see cref="Guid.NewGuid()"/> to ensure
    /// the identifier is unique across all systems and time, supporting the audit
    /// trail requirements of legal document management.
    /// 
    /// Example usage:
    /// <code>
    /// var revisionId = RevisionId.New();
    /// var revision = Revision.Create(revisionId, documentId, version, content, userId);
    /// </code>
    /// </remarks>
    public static RevisionId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a <see cref="RevisionId"/> from an existing GUID value.
    /// </summary>
    /// <param name="value">The GUID value to convert to a RevisionId.</param>
    /// <returns>
    /// A new <see cref="RevisionId"/> instance wrapping the provided GUID.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <remarks>
    /// Use this method when you have an existing GUID (e.g., from a database query,
    /// revision history, or external system) that you need to convert to a
    /// strongly-typed RevisionId. This is commonly used when loading revision
    /// data from persistent storage.
    /// 
    /// Example usage:
    /// <code>
    /// var guid = Guid.Parse("12345678-1234-1234-1234-123456789ABC");
    /// var revisionId = RevisionId.From(guid);
    /// var revision = await revisionRepository.GetByIdAsync(revisionId);
    /// </code>
    /// </remarks>
    public static RevisionId From(Guid value) => new(value);

    /// <summary>
    /// Creates a <see cref="RevisionId"/> from a string representation of a GUID.
    /// </summary>
    /// <param name="value">The string representation of the GUID.</param>
    /// <returns>
    /// A new <see cref="RevisionId"/> instance if parsing is successful.
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
    /// This is commonly used when parsing revision identifiers from URLs,
    /// audit logs, or when integrating with external systems that provide
    /// revision tracking capabilities.
    /// 
    /// Example usage:
    /// <code>
    /// // From API route parameter for revision history
    /// var revisionId = RevisionId.From(Request.RouteValues["revisionId"].ToString());
    /// 
    /// // From audit log entry
    /// var revisionId = RevisionId.From(auditEntry.RevisionIdentifier);
    /// </code>
    /// </remarks>
    public static RevisionId From(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid GUID format: {value}", nameof(value));

        return new RevisionId(guid);
    }

    /// <summary>
    /// Implicitly converts a <see cref="RevisionId"/> to its underlying <see cref="Guid"/> value.
    /// </summary>
    /// <param name="id">The RevisionId to convert.</param>
    /// <returns>The underlying GUID value, or <see cref="Guid.Empty"/> if the id is null.</returns>
    /// <remarks>
    /// This implicit conversion allows RevisionId instances to be used seamlessly
    /// where GUID values are expected, such as in Entity Framework queries for
    /// revision history, database parameters, or when interfacing with audit
    /// logging systems that expect GUID parameters.
    /// 
    /// Example usage:
    /// <code>
    /// RevisionId revisionId = RevisionId.New();
    /// Guid guid = revisionId; // Implicit conversion
    /// 
    /// // Use in Entity Framework query for revision history
    /// var revisionContent = await context.Revisions
    ///     .Where(r => r.Id == revisionId) // Implicit conversion
    ///     .Select(r => r.Content)
    ///     .FirstOrDefaultAsync();
    /// </code>
    /// </remarks>
    public static implicit operator Guid(RevisionId id) => id?.Value ?? Guid.Empty;

    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="RevisionId"/>.
    /// </summary>
    /// <param name="value">The GUID value to convert.</param>
    /// <returns>A new RevisionId wrapping the GUID value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <remarks>
    /// This implicit conversion allows GUID values to be automatically converted
    /// to RevisionId instances when needed, providing convenience while maintaining
    /// type safety. This is particularly useful when working with database results
    /// from revision queries or external revision tracking systems.
    /// 
    /// Example usage:
    /// <code>
    /// Guid guidFromDatabase = dataReader.GetGuid("RevisionId");
    /// RevisionId revisionId = guidFromDatabase; // Implicit conversion
    /// 
    /// // Use in revision comparison logic
    /// var isLatestRevision = revisionId == document.LatestRevisionId;
    /// </code>
    /// </remarks>
    public static implicit operator RevisionId(Guid value) => From(value);

    /// <summary>
    /// Returns a string representation of this revision identifier.
    /// </summary>
    /// <returns>
    /// A string representation of the underlying GUID value in standard format.
    /// </returns>
    /// <remarks>
    /// The returned string uses the standard GUID format with hyphens:
    /// "12345678-1234-1234-1234-123456789ABC"
    /// 
    /// This method is essential for logging revision activities, debugging document
    /// version issues, displaying revision information in user interfaces, and
    /// maintaining audit trails required for legal document compliance.
    /// 
    /// Example usage:
    /// <code>
    /// var revisionId = RevisionId.New();
    /// logger.LogInformation("Created revision {RevisionId} for document", revisionId.ToString());
    /// 
    /// // For audit trail display
    /// Console.WriteLine($"Document revision: {revisionId}");
    /// 
    /// // For revision history UI
    /// var displayText = $"Revision {revisionId} - {revision.CreatedDate}";
    /// </code>
    /// </remarks>
    public override string ToString() => Value.ToString();
}