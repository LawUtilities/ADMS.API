namespace ADMS.Domain.Common;

/// <summary>
/// Exception that represents a violation of domain business rules or invariants.
/// </summary>
/// <remarks>
/// DomainException serves as the standard exception type for domain rule violations
/// within the ADMS legal document management system. It encapsulates structured error
/// information through DomainError while providing standard .NET exception behavior
/// for exceptional circumstances that cannot be handled through the Result pattern.
/// 
/// <para><strong>Usage Guidelines:</strong></para>
/// <list type="bullet">
/// <item><strong>Domain Rule Violations:</strong> Use for invariant violations that should never occur in normal operation</item>
/// <item><strong>System Integrity Issues:</strong> Use for critical failures that compromise data integrity</item>
/// <item><strong>Exceptional Conditions:</strong> Use sparingly for truly exceptional scenarios</item>
/// <item><strong>Avoid for Validation:</strong> Prefer Result pattern for expected validation failures</item>
/// </list>
/// 
/// <para><strong>DDD Exception Strategy:</strong></para>
/// In Domain-Driven Design, exceptions should be reserved for truly exceptional conditions.
/// Most domain validation and business rule failures should use the Result pattern
/// instead of exceptions to enable functional programming approaches and improve
/// system reliability and performance.
/// 
/// <para><strong>Legal Practice Context:</strong></para>
/// In legal document management systems, exceptions may represent:
/// <list type="bullet">
/// <item>Data integrity violations that could compromise legal compliance</item>
/// <item>Security breaches or unauthorized access attempts</item>
/// <item>System state corruption that requires immediate attention</item>
/// <item>Critical failures that could affect client confidentiality</item>
/// </list>
/// 
/// <para><strong>Error Handling Strategy:</strong></para>
/// <list type="bullet">
/// <item><strong>Expected Failures:</strong> Use Result pattern with DomainError</item>
/// <item><strong>Validation Errors:</strong> Use Result pattern for user-correctable issues</item>
/// <item><strong>Business Rule Violations:</strong> Use Result pattern for rule enforcement</item>
/// <item><strong>System Failures:</strong> Use DomainException for critical system issues</item>
/// </list>
/// 
/// <para><strong>Integration with Structured Logging:</strong></para>
/// DomainException integrates with structured logging systems to provide detailed
/// error context while maintaining the structured error information from DomainError
/// for monitoring, alerting, and diagnostic purposes.
/// </remarks>
/// <example>
/// <code>
/// // Using DomainException for critical invariant violations
/// public class Document : Entity&lt;DocumentId&gt;
/// {
///     public void CheckOut(UserId userId)
///     {
///         if (Id == null)
///             throw new DomainException(new DomainError("DOCUMENT_INVALID_STATE", 
///                 "Document entity is in an invalid state - missing identifier"));
///                 
///         // Normal validation uses Result pattern instead
///         var validationResult = ValidateCheckOut(userId);
///         if (validationResult.IsFailure)
///             return validationResult; // Result pattern for expected failures
///     }
/// }
/// 
/// // Exception handling in application layer
/// try
/// {
///     document.CheckOut(userId);
/// }
/// catch (DomainException ex)
/// {
///     _logger.LogCritical(ex, "Domain exception occurred: {ErrorCode} - {ErrorDescription}",
///         ex.Error.Code, ex.Error.Description);
///     throw; // Re-throw after logging
/// }
/// 
/// // Preferred approach for expected validation failures
/// public Result CheckOutDocument(DocumentId documentId, UserId userId)
/// {
///     var document = _repository.GetById(documentId);
///     if (document == null)
///         return Result.Failure(new DomainError("DOCUMENT_NOT_FOUND", "Document not found"));
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
    /// <value>A DomainError containing the error code and description.</value>
    /// <remarks>
    /// The Error property provides access to the structured error information that caused
    /// this exception, enabling consistent error handling and logging across the application.
    /// This property bridges the gap between the structured domain error system and
    /// traditional .NET exception handling patterns.
    /// 
    /// <para><strong>Structured Error Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Machine-readable error codes for automated handling</item>
    /// <item>Human-readable descriptions for user communication</item>
    /// <item>Consistent error format across all domain exceptions</item>
    /// <item>Integration with monitoring and alerting systems</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     // Some domain operation that may throw
    ///     domain.PerformCriticalOperation();
    /// }
    /// catch (DomainException ex)
    /// {
    ///     // Access structured error information
    ///     string errorCode = ex.Error.Code;
    ///     string description = ex.Error.Description;
    ///     
    ///     _logger.LogError("Domain error {ErrorCode}: {Description}", errorCode, description);
    ///     
    ///     // Use error code for conditional handling
    ///     if (errorCode == "DOCUMENT_INVALID_STATE")
    ///     {
    ///         // Handle specific error type
    ///     }
    /// }
    /// </code>
    /// </example>
    public DomainError Error { get; }

    /// <summary>
    /// Initializes a new instance of the DomainException class with a structured domain error.
    /// </summary>
    /// <param name="error">The domain error that caused this exception.</param>
    /// <remarks>
    /// This constructor creates a domain exception with structured error information,
    /// using the error's description as the exception message while preserving the
    /// full error context for structured error handling.
    /// 
    /// <para><strong>Preferred Constructor:</strong></para>
    /// This constructor should be preferred when creating domain exceptions as it
    /// provides the most structured and consistent error information for logging,
    /// monitoring, and error handling throughout the application.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Creating domain exception with structured error
    /// var error = new DomainError("DOCUMENT_INVALID_STATE", 
    ///     "Document cannot be processed due to invalid state");
    /// throw new DomainException(error);
    /// 
    /// // The exception message will be the error description
    /// // The Error property will contain the full DomainError
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is null.</exception>
    public DomainException(DomainError error) : base(error?.Description ?? throw new ArgumentNullException(nameof(error)))
    {
        Error = error;
    }

    /// <summary>
    /// Initializes a new instance of the DomainException class with a simple error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <remarks>
    /// This constructor provides a simple way to create domain exceptions when only a message
    /// is available. It automatically creates a generic DomainError with a standard error code.
    /// 
    /// <para><strong>Usage Recommendation:</strong></para>
    /// While convenient, prefer the constructor that accepts a DomainError for better
    /// structured error handling and more precise error classification. This constructor
    /// should be used primarily for quick prototyping or when migrating existing code.
    /// 
    /// <para><strong>Generated Error Code:</strong></para>
    /// This constructor automatically generates a generic "DOMAIN_ERROR" code. For production
    /// systems, consider using specific error codes that enable better error categorization
    /// and automated handling.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Simple domain exception creation
    /// throw new DomainException("Document processing failed due to invalid data");
    /// 
    /// // The Error property will contain:
    /// // Code: "DOMAIN_ERROR"
    /// // Description: "Document processing failed due to invalid data"
    /// 
    /// // Preferred approach with specific error
    /// var specificError = new DomainError("DOCUMENT_PROCESSING_FAILED", 
    ///     "Document processing failed due to invalid data");
    /// throw new DomainException(specificError);
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is empty or whitespace.</exception>
    public DomainException(string message) : base(message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Exception message cannot be null or empty", nameof(message));

        Error = new DomainError("DOMAIN_ERROR", message);
    }

    /// <summary>
    /// Returns a string representation of the domain exception.
    /// </summary>
    /// <returns>A string that contains the structured error information and stack trace.</returns>
    /// <remarks>
    /// The string representation includes both the structured error information from
    /// the DomainError and the standard exception details, providing comprehensive
    /// diagnostic information for logging and debugging purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     throw new DomainException(new DomainError("DOCUMENT_NOT_FOUND", "Document could not be located"));
    /// }
    /// catch (DomainException ex)
    /// {
    ///     Console.WriteLine(ex.ToString());
    ///     // Output includes:
    ///     // Domain Error [DOCUMENT_NOT_FOUND]: Document could not be located
    ///     // [Standard exception details and stack trace]
    /// }
    /// </code>
    /// </example>
    public override string ToString()
    {
        return $"Domain Error [{Error.Code}]: {Error.Description}{Environment.NewLine}{base.ToString()}";
    }
}