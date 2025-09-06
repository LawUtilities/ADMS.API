using System.Text.Json.Serialization;

namespace ADMS.Domain.Common;

/// <summary>
/// Exception that represents a violation of domain business rules or invariants.
/// </summary>
/// <remarks>
/// <para>
/// DomainException serves as the standard exception type for domain rule violations
/// within the ADMS legal document management system. It encapsulates structured error
/// information through DomainError while providing standard .NET exception behavior
/// for exceptional circumstances that cannot be handled through the Result pattern.
/// </para>
/// <para>
/// <strong>Usage Guidelines:</strong>
/// </para>
/// <list type="bullet">
/// <item><strong>Domain Rule Violations:</strong> Use for invariant violations that should never occur in normal operation</item>
/// <item><strong>System Integrity Issues:</strong> Use for critical failures that compromise data integrity</item>
/// <item><strong>Avoid for Validation:</strong> Prefer Result pattern for expected validation failures</item>
/// </list>
/// <para>
/// <strong>Error Handling Strategy:</strong>
/// </para>
/// <list type="bullet">
/// <item><strong>Expected Failures:</strong> Use Result pattern with DomainError</item>
/// <item><strong>Validation Errors:</strong> Use Result pattern for user-correctable issues</item>
/// <item><strong>System Failures:</strong> Use DomainException for critical system issues</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Using DomainException for critical invariant violations
/// public class Document : Entity&lt;DocumentId&gt;
/// {
///     public void CheckOut(UserId userId)
///     {
///         if (Id == null)
///             throw new DomainException(DomainError.Custom("DOCUMENT_INVALID_STATE", 
///                 "Document entity is in an invalid state - missing identifier"));
///     }
/// }
/// 
/// // Preferred approach for expected validation failures
/// public Result CheckOutDocument(DocumentId documentId, UserId userId)
/// {
///     var document = _repository.GetById(documentId);
///     if (document == null)
///         return Result.Failure(DomainError.NotFound);
///         
///     return document.CheckOut(userId); // Returns Result, not exception
/// }
/// </code>
/// </example>
public sealed class DomainException : Exception
{
    /// <summary>
    /// Gets the structured domain error information associated with this exception.
    /// </summary>
    /// <value>A DomainError containing the error code and message.</value>
    [JsonInclude]
    public DomainError Error { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a structured domain error.
    /// </summary>
    /// <param name="error">The domain error that caused this exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is null.</exception>
    public DomainException(DomainError error)
        : base(error?.Message ?? throw new ArgumentNullException(nameof(error)))
    {
        Error = error;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a simple error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or empty.</exception>
    /// <remarks>
    /// This constructor creates a DomainError with code "DOMAIN_EXCEPTION" and the provided message.
    /// For production systems, prefer using the constructor that accepts a DomainError with specific error codes.
    /// </remarks>
    public DomainException(string message) : base(message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Exception message cannot be null or empty.", nameof(message));

        Error = DomainError.Custom("DOMAIN_EXCEPTION", message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with an error message and inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or empty.</exception>
    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Exception message cannot be null or empty.", nameof(message));

        Error = DomainError.Custom("DOMAIN_EXCEPTION", message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a domain error and inner exception.
    /// </summary>
    /// <param name="error">The domain error that caused this exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is null.</exception>
    public DomainException(DomainError error, Exception innerException)
        : base(error?.Message ?? throw new ArgumentNullException(nameof(error)), innerException)
    {
        Error = error;
    }

    /// <summary>
    /// Parameterless constructor for JSON deserialization.
    /// </summary>
    /// <remarks>
    /// This constructor is required for JSON serialization frameworks.
    /// It should not be used directly in application code.
    /// </remarks>
    [JsonConstructor]
    private DomainException() : base("Unknown domain error occurred")
    {
        Error = DomainError.Unknown;
    }

    /// <summary>
    /// Creates a DomainException from JSON-serialized data.
    /// </summary>
    /// <param name="errorCode">The error code from serialized data.</param>
    /// <param name="errorMessage">The error message from serialized data.</param>
    /// <param name="exceptionMessage">The exception message from serialized data.</param>
    /// <returns>A new DomainException instance.</returns>
    /// <remarks>
    /// This method is used for deserializing exceptions from modern JSON-based APIs
    /// and logging systems. It replaces the obsolete binary serialization approach.
    /// </remarks>
    public static DomainException FromSerialized(string errorCode, string errorMessage, string exceptionMessage)
    {
        var error = DomainError.Custom(
            errorCode ?? "DESERIALIZATION_ERROR",
            errorMessage ?? "Error occurred during deserialization");

        return new DomainException(error)
        {
            // Note: We can't directly set the Message property, but the Error contains the information
        };
    }

    /// <summary>
    /// Converts this exception to a serializable format for modern APIs.
    /// </summary>
    /// <returns>An anonymous object containing the serializable exception data.</returns>
    /// <remarks>
    /// This method provides a modern alternative to the obsolete GetObjectData method,
    /// returning data suitable for JSON serialization in APIs and logging systems.
    /// </remarks>
    public object ToSerializable()
    {
        return new
        {
            ErrorCode = Error.Code,
            ErrorMessage = Error.Message,
            ExceptionMessage = Message,
            ExceptionType = GetType().FullName,
            StackTrace = StackTrace,
            InnerException = InnerException?.ToString()
        };
    }

    /// <summary>
    /// Returns a string representation of the domain exception.
    /// </summary>
    /// <returns>A string that contains the structured error information and stack trace.</returns>
    public override string ToString()
    {
        return $"Domain Error [{Error.Code}]: {Error.Message}{Environment.NewLine}{base.ToString()}";
    }
}