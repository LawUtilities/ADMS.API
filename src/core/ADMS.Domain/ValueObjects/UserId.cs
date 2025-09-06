using System;

namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a unique identifier for a user in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// This value object encapsulates a GUID-based identifier for system users, ensuring type safety
/// and preventing accidental mixing of different entity identifiers. Users represent individuals
/// who interact with the legal document management system, including attorneys, paralegals,
/// administrators, and clients with appropriate access permissions.
/// 
/// The UserId is immutable and provides validation to ensure the identifier is never empty
/// or invalid. As a value object, UserId instances are compared by value rather than reference,
/// ensuring that two UserId instances with the same GUID value are considered equal.
/// 
/// This strongly-typed identifier is crucial for maintaining security, access control,
/// and audit trail integrity throughout the legal document management system. It supports
/// compliance requirements by ensuring all user actions can be uniquely traced and attributed.
/// </remarks>
public sealed record UserId
{
    /// <summary>
    /// Gets the underlying GUID value of this user identifier.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> that uniquely identifies a user in the system.
    /// </value>
    /// <remarks>
    /// This property provides access to the underlying GUID value while maintaining
    /// encapsulation. The value is guaranteed to never be <see cref="Guid.Empty"/>
    /// due to validation performed during construction.
    /// 
    /// The GUID format ensures global uniqueness across all users, supporting
    /// distributed authentication systems, user migrations, and integration with
    /// external identity providers while maintaining audit trail integrity.
    /// </remarks>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserId"/> record with the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value for this user identifier.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <remarks>
    /// This constructor is private to enforce the use of factory methods for creating instances.
    /// This ensures consistent validation and provides controlled ways to create UserId instances,
    /// maintaining the integrity of user identification and security throughout the system.
    /// </remarks>
    private UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(value));
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="UserId"/> with a randomly generated GUID.
    /// </summary>
    /// <returns>
    /// A new <see cref="UserId"/> instance with a unique identifier.
    /// </returns>
    /// <remarks>
    /// This method is the primary way to create new user identifiers when
    /// registering new users in the system. It uses <see cref="Guid.NewGuid()"/> to ensure
    /// the identifier is unique across all systems and time, supporting the security
    /// and audit requirements of legal document management.
    /// 
    /// Example usage:
    /// <code>
    /// var userId = UserId.New();
    /// var user = User.Create(userId, email, firstName, lastName, role);
    /// await userRepository.AddAsync(user);
    /// </code>
    /// </remarks>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a <see cref="UserId"/> from an existing GUID value.
    /// </summary>
    /// <param name="value">The GUID value to convert to a UserId.</param>
    /// <returns>
    /// A new <see cref="UserId"/> instance wrapping the provided GUID.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <remarks>
    /// Use this method when you have an existing GUID (e.g., from authentication
    /// systems, database queries, or external identity providers) that you need
    /// to convert to a strongly-typed UserId. This is commonly used during
    /// authentication and authorization processes.
    /// 
    /// Example usage:
    /// <code>
    /// // From authentication context
    /// var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    /// var guid = Guid.Parse(claimValue);
    /// var userId = UserId.From(guid);
    /// 
    /// // From database query
    /// var user = await userRepository.GetByIdAsync(UserId.From(userGuid));
    /// </code>
    /// </remarks>
    public static UserId From(Guid value) => new(value);

    /// <summary>
    /// Creates a <see cref="UserId"/> from a string representation of a GUID.
    /// </summary>
    /// <param name="value">The string representation of the GUID.</param>
    /// <returns>
    /// A new <see cref="UserId"/> instance if parsing is successful.
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
    /// This is commonly used when parsing user identifiers from authentication
    /// tokens, API requests, configuration files, or when integrating with
    /// external identity management systems.
    /// 
    /// Example usage:
    /// <code>
    /// // From JWT token claim
    /// var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
    /// var userId = UserId.From(userIdClaim);
    /// 
    /// // From API route parameter
    /// var userId = UserId.From(Request.RouteValues["userId"].ToString());
    /// 
    /// // From external identity provider
    /// var userId = UserId.From(externalProvider.UserIdentifier);
    /// </code>
    /// </remarks>
    public static UserId From(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid GUID format: {value}", nameof(value));

        return new UserId(guid);
    }

    /// <summary>
    /// Implicitly converts a <see cref="UserId"/> to its underlying <see cref="Guid"/> value.
    /// </summary>
    /// <param name="id">The UserId to convert.</param>
    /// <returns>The underlying GUID value, or <see cref="Guid.Empty"/> if the id is null.</returns>
    /// <remarks>
    /// This implicit conversion allows UserId instances to be used seamlessly
    /// where GUID values are expected, such as in Entity Framework queries for
    /// user-related data, database parameters, audit logging systems, or when
    /// interfacing with authentication providers that expect GUID parameters.
    /// 
    /// Example usage:
    /// <code>
    /// UserId userId = UserId.New();
    /// Guid guid = userId; // Implicit conversion
    /// 
    /// // Use in Entity Framework query
    /// var userDocuments = await context.Documents
    ///     .Where(d => d.CreatedBy == userId.Value) // Or just userId with conversion
    ///     .ToListAsync();
    /// 
    /// // Use in audit logging
    /// auditLogger.Log(userId, "Document accessed", documentId);
    /// </code>
    /// </remarks>
    public static implicit operator Guid(UserId id) => id?.Value ?? Guid.Empty;

    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="UserId"/>.
    /// </summary>
    /// <param name="value">The GUID value to convert.</param>
    /// <returns>A new UserId wrapping the GUID value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <remarks>
    /// This implicit conversion allows GUID values to be automatically converted
    /// to UserId instances when needed, providing convenience while maintaining
    /// type safety. This is particularly useful when working with authentication
    /// results, database queries, or external identity provider responses.
    /// 
    /// Example usage:
    /// <code>
    /// // From authentication result
    /// Guid userGuidFromAuth = authResult.UserId;
    /// UserId userId = userGuidFromAuth; // Implicit conversion
    /// 
    /// // From database reader
    /// Guid guidFromDatabase = dataReader.GetGuid("UserId");
    /// UserId userId = guidFromDatabase; // Implicit conversion
    /// 
    /// // Use in business logic
    /// var canAccess = await accessControlService.CanAccessDocumentAsync(userId, documentId);
    /// </code>
    /// </remarks>
    public static implicit operator UserId(Guid value) => From(value);

    /// <summary>
    /// Returns a string representation of this user identifier.
    /// </summary>
    /// <returns>
    /// A string representation of the underlying GUID value in standard format.
    /// </returns>
    /// <remarks>
    /// The returned string uses the standard GUID format with hyphens:
    /// "12345678-1234-1234-1234-123456789ABC"
    /// 
    /// This method is essential for logging user activities, debugging authentication
    /// issues, displaying user information in administrative interfaces, and
    /// maintaining audit trails required for legal document compliance and security.
    /// 
    /// Example usage:
    /// <code>
    /// var userId = UserId.New();
    /// logger.LogInformation("User {UserId} accessed document {DocumentId}", 
    ///     userId.ToString(), documentId);
    /// 
    /// // For audit trail display
    /// Console.WriteLine($"Action performed by user: {userId}");
    /// 
    /// // For administrative UI
    /// var displayText = $"User ID: {userId} - Last login: {user.LastLoginDate}";
    /// 
    /// // For JWT token claims
    /// claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
    /// </code>
    /// </remarks>
    public override string ToString() => Value.ToString();
}