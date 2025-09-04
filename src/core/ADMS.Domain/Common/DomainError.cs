namespace ADMS.Domain.Common;

/// <summary>
/// Represents a domain-specific error with standardized code and human-readable description.
/// </summary>
/// <remarks>
/// DomainError provides a structured approach to error handling within the domain layer,
/// enabling consistent error reporting and supporting professional legal practice standards.
/// This record encapsulates both machine-readable error codes and human-readable descriptions
/// for comprehensive error handling and client communication.
/// 
/// <para><strong>Design Principles:</strong></para>
/// <list type="bullet">
/// <item><strong>Immutable:</strong> Error information cannot be modified after creation</item>
/// <item><strong>Structured:</strong> Consistent format for error codes and descriptions</item>
/// <item><strong>Professional:</strong> Human-readable messages suitable for legal professionals</item>
/// <item><strong>Traceable:</strong> Unique error codes enable precise error identification</item>
/// </list>
/// 
/// <para><strong>Usage in Legal Document Management:</strong></para>
/// Domain errors support legal practice requirements by providing clear, professional
/// error messages that can be safely communicated to legal professionals while maintaining
/// technical precision for system developers and administrators.
/// 
/// <para><strong>Error Code Conventions:</strong></para>
/// <list type="bullet">
/// <item><strong>Entity Prefix:</strong> DOCUMENT_, MATTER_, REVISION_, USER_</item>
/// <item><strong>Action Context:</strong> CREATION_, VALIDATION_, ACCESS_, etc.</item>
/// <item><strong>Specific Issue:</strong> REQUIRED, TOO_LONG, INVALID_FORMAT, etc.</item>
/// </list>
/// 
/// <para><strong>Integration with Result Pattern:</strong></para>
/// DomainError integrates seamlessly with the Result pattern to provide robust
/// error handling without exceptions, supporting functional programming approaches
/// and improving system reliability and maintainability.
/// </remarks>
/// <example>
/// <code>
/// // Creating domain errors for validation scenarios
/// var fileNameError = new DomainError("DOCUMENT_FILENAME_REQUIRED", "Document filename cannot be empty");
/// var sizeError = new DomainError("DOCUMENT_FILE_SIZE_EXCEEDS_LIMIT", "Document file size exceeds maximum allowed size");
/// 
/// // Using with Result pattern
/// public static Result&lt;Document&gt; Create(string fileName)
/// {
///     if (string.IsNullOrEmpty(fileName))
///         return Result.Failure&lt;Document&gt;(new DomainError("DOCUMENT_FILENAME_REQUIRED", "Document filename cannot be empty"));
///     
///     // ... create document
///     return Result.Success(document);
/// }
/// 
/// // Implicit conversion to string returns the error code
/// string errorCode = fileNameError; // Returns "DOCUMENT_FILENAME_REQUIRED"
/// </code>
/// </example>
public sealed record DomainError(string Code, string Description)
{
    /// <summary>
    /// Represents the absence of an error, used for successful operations.
    /// </summary>
    /// <remarks>
    /// This static instance represents a successful state with no error information.
    /// It is used throughout the domain layer to indicate successful operations
    /// when using the Result pattern, avoiding null references and providing
    /// explicit success state representation.
    /// </remarks>
    public static readonly DomainError None = new(string.Empty, string.Empty);

    /// <summary>
    /// Gets the error code for machine processing and identification.
    /// </summary>
    /// <value>A unique string identifier for this specific error type.</value>
    /// <remarks>
    /// The error code follows a standardized format to enable precise error identification,
    /// logging, and automated error handling. Codes are designed to be stable across
    /// system versions to support consistent error handling and monitoring.
    /// </remarks>
    public string Code { get; } = Code ?? throw new ArgumentNullException(nameof(Code));

    /// <summary>
    /// Gets the human-readable error description.
    /// </summary>
    /// <value>A descriptive message explaining the error in professional language.</value>
    /// <remarks>
    /// The description provides clear, professional language suitable for legal practitioners
    /// while maintaining technical accuracy. Messages are designed to be actionable,
    /// helping users understand both what went wrong and how to resolve the issue.
    /// </remarks>
    public string Description { get; } = Description ?? throw new ArgumentNullException(nameof(Description));

    /// <summary>
    /// Implicitly converts a DomainError to its error code string.
    /// </summary>
    /// <param name="error">The domain error to convert.</param>
    /// <returns>The error code as a string.</returns>
    /// <remarks>
    /// This implicit conversion operator enables seamless integration with logging systems,
    /// monitoring tools, and other components that expect string-based error identifiers.
    /// The conversion returns the error code rather than the description to maintain
    /// consistency with machine-readable error handling patterns.
    /// </remarks>
    /// <example>
    /// <code>
    /// var error = new DomainError("DOCUMENT_NOT_FOUND", "The requested document could not be found");
    /// string errorCode = error; // Returns "DOCUMENT_NOT_FOUND"
    /// 
    /// // Useful for logging and monitoring
    /// _logger.LogError("Operation failed with error: {ErrorCode}", (string)error);
    /// </code>
    /// </example>
    public static implicit operator string(DomainError error) => error.Code;

    /// <summary>
    /// Returns a string representation of the domain error.
    /// </summary>
    /// <returns>A formatted string containing both the error code and description.</returns>
    /// <remarks>
    /// The string representation includes both the machine-readable code and the human-readable
    /// description, making it suitable for comprehensive logging and debugging scenarios
    /// where both pieces of information are valuable.
    /// </remarks>
    /// <example>
    /// <code>
    /// var error = new DomainError("DOCUMENT_NOT_FOUND", "The requested document could not be found");
    /// Console.WriteLine(error.ToString()); 
    /// // Output: "DOCUMENT_NOT_FOUND: The requested document could not be found"
    /// </code>
    /// </example>
    public override string ToString() => $"{Code}: {Description}";
}