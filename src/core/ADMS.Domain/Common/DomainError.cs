using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

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
    /// Gets optional additional context or metadata for the error.
    /// </summary>
    /// <value>
    /// A dictionary containing additional error context, or null if no context is provided.
    /// </value>
    public IReadOnlyDictionary<string, object>? Context { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainError"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="context">Optional additional context for the error.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/> or <paramref name="message"/> is null or empty.
    /// </exception>
    [JsonConstructor]
    public DomainError(string code, string message, IReadOnlyDictionary<string, object>? context = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code cannot be null or empty.", nameof(code));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Error message cannot be null or empty.", nameof(message));

        Code = code.Trim().ToUpperInvariant();
        Message = message.Trim();
        Context = context;
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
    /// Gets a domain error representing a concurrency conflict.
    /// </summary>
    public static DomainError ConcurrencyConflict => new("CONCURRENCY_CONFLICT", "The resource was modified by another process.");

    /// <summary>
    /// Gets a domain error representing invalid input data.
    /// </summary>
    public static DomainError InvalidInput => new("INVALID_INPUT", "The provided input data is invalid.");

    /// <summary>
    /// Creates a custom domain error with the specified code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="context">Optional additional context for the error.</param>
    /// <returns>A new <see cref="DomainError"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/> or <paramref name="message"/> is null or empty.
    /// </exception>
    public static DomainError Create(string code, string message, IReadOnlyDictionary<string, object>? context = null)
        => new(code, message, context);

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
    public static DomainError Create(string code, string messageFormat, params object[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return new(code, string.Format(messageFormat, args));
    }

    /// <summary>
    /// Creates a domain error with additional context information.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="contextBuilder">Action to build additional context.</param>
    /// <returns>A new <see cref="DomainError"/> instance.</returns>
    public static DomainError WithContext(string code, string message, Action<Dictionary<string, object>> contextBuilder)
    {
        var context = new Dictionary<string, object>();
        contextBuilder(context);
        return new(code, message, context);
    }

    /// <summary>
    /// Creates a validation error for a specific field.
    /// </summary>
    /// <param name="fieldName">The name of the field that failed validation.</param>
    /// <param name="message">The validation error message.</param>
    /// <param name="attemptedValue">The value that failed validation.</param>
    /// <returns>A new <see cref="DomainError"/> instance for field validation.</returns>
    public static DomainError FieldValidation(string fieldName, string message, object? attemptedValue = null)
    {
        var context = new Dictionary<string, object>
        {
            ["FieldName"] = fieldName
        };

        if (attemptedValue != null)
        {
            context["AttemptedValue"] = attemptedValue;
        }

        return new("FIELD_VALIDATION_FAILED", message, context);
    }

    /// <summary>
    /// Creates a business rule violation error.
    /// </summary>
    /// <param name="ruleName">The name of the business rule that was violated.</param>
    /// <param name="message">The error message describing the violation.</param>
    /// <returns>A new <see cref="DomainError"/> instance for business rule violations.</returns>
    public static DomainError BusinessRuleViolation(string ruleName, string message)
    {
        var context = new Dictionary<string, object>
        {
            ["RuleName"] = ruleName
        };

        return new("BUSINESS_RULE_VIOLATION", message, context);
    }

    /// <summary>
    /// Gets the context value for the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the context value.</typeparam>
    /// <param name="key">The context key.</param>
    /// <returns>The context value if found; otherwise, the default value for the type.</returns>
    public T? GetContextValue<T>(string key)
    {
        if (Context?.TryGetValue(key, out var value) == true && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    /// <summary>
    /// Determines whether this error has the specified context key.
    /// </summary>
    /// <param name="key">The context key to check.</param>
    /// <returns>True if the context contains the key; otherwise, false.</returns>
    public bool HasContextKey(string key) => Context?.ContainsKey(key) == true;

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
}