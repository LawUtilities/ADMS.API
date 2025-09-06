using System.Runtime.Serialization;

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
[Serializable]
public sealed class DomainException : Exception
{
    /// <summary>
    /// Gets the structured domain error information associated with this exception.
    /// </summary>
    /// <value>A DomainError containing the error code and message.</value>
    public DomainError Error { get; }

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
    /// Initializes a new instance of the <see cref="DomainException"/> class with serialized data.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context.</param>
    private DomainException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        var errorCode = info.GetString(nameof(Error) + ".Code") ?? "UNKNOWN_ERROR";
        var errorMessage = info.GetString(nameof(Error) + ".Message") ?? "An unknown error occurred.";
        Error = DomainError.Custom(errorCode, errorMessage);
    }

    /// <summary>
    /// Sets the SerializationInfo with information about the exception.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException(nameof(info));

        base.GetObjectData(info, context);
        info.AddValue(nameof(Error) + ".Code", Error.Code);
        info.AddValue(nameof(Error) + ".Message", Error.Message);
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