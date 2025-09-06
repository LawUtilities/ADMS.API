using System.Diagnostics.CodeAnalysis;

namespace ADMS.Domain.Common;

/// <summary>
/// Represents a domain-specific error with code and message.
/// </summary>
/// <remarks>
/// Domain errors represent business rule violations and other domain-specific error conditions 
/// in a structured, immutable format that can be easily handled by application layers.
/// </remarks>
public sealed class DomainError : IEquatable<DomainError>
{
    /// <summary>
    /// Gets the error code that identifies the type of error.
    /// </summary>
    /// <value>
    /// A string representing a unique error code (e.g., "DOCUMENT_NOT_FOUND").
    /// </value>
    public string Code { get; init; }

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    /// <value>
    /// A string containing a descriptive error message.
    /// </value>
    public string Message { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainError"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/> or <paramref name="message"/> is null or empty.
    /// </exception>
    public DomainError(string code, string message)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code cannot be null or empty.", nameof(code));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Error message cannot be null or empty.", nameof(message));

        Code = code;
        Message = message;
    }

    /// <summary>
    /// Gets a domain error representing an unknown or unexpected error.
    /// </summary>
    public static DomainError Unknown => new("UNKNOWN_ERROR", "An unknown error has occurred.");

    /// <summary>
    /// Gets a domain error representing an invalid operation.
    /// </summary>
    public static DomainError InvalidOperation => new("INVALID_OPERATION", "The requested operation is not valid.");

    /// <summary>
    /// Gets a domain error representing a resource not found.
    /// </summary>
    public static DomainError NotFound => new("RESOURCE_NOT_FOUND", "The requested resource was not found.");

    /// <summary>
    /// Gets a domain error representing a validation failure.
    /// </summary>
    public static DomainError ValidationFailed => new("VALIDATION_FAILED", "Validation failed for the provided data.");

    /// <summary>
    /// Gets a domain error representing an unauthorized access attempt.
    /// </summary>
    public static DomainError Unauthorized => new("UNAUTHORIZED_ACCESS", "Access to the requested resource is not authorized.");

    /// <summary>
    /// Gets a domain error representing a conflict with existing data.
    /// </summary>
    public static DomainError Conflict => new("RESOURCE_CONFLICT", "The operation conflicts with existing data.");

    /// <summary>
    /// Creates a custom domain error with the specified code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A new <see cref="DomainError"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/> or <paramref name="message"/> is null or empty.
    /// </exception>
    public static DomainError Custom(string code, string message) => new(code, message);

    /// <summary>
    /// Creates a domain error with the specified code and a formatted message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="messageFormat">The message format string.</param>
    /// <param name="args">Arguments for the format string.</param>
    /// <returns>A new <see cref="DomainError"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/> or <paramref name="messageFormat"/> is null or empty.
    /// </exception>
    public static DomainError Custom(string code, string messageFormat, params object[] args) =>
        new(code, string.Format(messageFormat, args));

    /// <summary>
    /// Returns a string representation of the domain error.
    /// </summary>
    /// <returns>A string containing the error code and message.</returns>
    public override string ToString() => $"[{Code}] {Message}";

    /// <summary>
    /// Determines whether the specified domain error is equal to the current domain error.
    /// </summary>
    /// <param name="other">The domain error to compare with the current domain error.</param>
    /// <returns>
    /// <c>true</c> if the specified domain error is equal to the current domain error; otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(DomainError? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Code == other.Code && Message == other.Message;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current domain error.
    /// </summary>
    /// <param name="obj">The object to compare with the current domain error.</param>
    /// <returns>
    /// <c>true</c> if the specified object is equal to the current domain error; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj) => Equals(obj as DomainError);

    /// <summary>
    /// Returns a hash code for this domain error.
    /// </summary>
    /// <returns>A hash code for the current domain error.</returns>
    public override int GetHashCode() => HashCode.Combine(Code, Message);

    /// <summary>
    /// Determines whether two domain error instances are equal.
    /// </summary>
    /// <param name="left">The first domain error to compare.</param>
    /// <param name="right">The second domain error to compare.</param>
    /// <returns><c>true</c> if the domain errors are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(DomainError? left, DomainError? right) => Equals(left, right);

    /// <summary>
    /// Determines whether two domain error instances are not equal.
    /// </summary>
    /// <param name="left">The first domain error to compare.</param>
    /// <param name="right">The second domain error to compare.</param>
    /// <returns><c>true</c> if the domain errors are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(DomainError? left, DomainError? right) => !Equals(left, right);

    /// <summary>
    /// Implicitly converts a string to a DomainError with a generic error code.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new <see cref="DomainError"/> instance.</returns>
    [return: NotNullIfNotNull(nameof(message))]
    public static implicit operator DomainError?(string? message) =>
        string.IsNullOrWhiteSpace(message) ? null : new DomainError("GENERIC_ERROR", message);
}