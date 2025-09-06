namespace ADMS.Domain.Common;

/// <summary>
/// Represents a domain-specific error with code and message.
/// </summary>
/// <remarks>
/// Domain errors are used to represent business rule violations and other
/// domain-specific error conditions in a structured way that can be easily
/// handled by application layers.
/// </remarks>
public class DomainError
{
    /// <summary>
    /// Gets the error code that identifies the type of error.
    /// </summary>
    /// <value>
    /// A string representing a unique error code (e.g., "DOCUMENT_NOT_FOUND").
    /// </value>
    /// <remarks>
    /// Error codes should be consistent and meaningful to allow proper error
    /// handling and localization in consuming applications.
    /// </remarks>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    /// <value>
    /// A string containing a descriptive error message.
    /// </value>
    /// <remarks>
    /// The message should be clear and actionable, helping users understand
    /// what went wrong and how to potentially resolve the issue.
    /// </remarks>
    public string Message { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainError"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/> or <paramref name="message"/> is null or empty.
    /// </exception>
    private DomainError(string code, string message)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code cannot be null or empty", nameof(code));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Error message cannot be null or empty", nameof(message));

        Code = code;
        Message = message;
    }

    /// <summary>
    /// Creates a new domain error with the specified code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A new <see cref="DomainError"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/> or <paramref name="message"/> is null or empty.
    /// </exception>
    public static DomainError Create(string code, string message) => new(code, message);

    /// <summary>
    /// Returns a string representation of the domain error.
    /// </summary>
    /// <returns>A string containing the error code and message.</returns>
    public override string ToString() => $"{Code}: {Message}";

    /// <summary>
    /// Determines whether the specified object is equal to the current domain error.
    /// </summary>
    /// <param name="obj">The object to compare with the current domain error.</param>
    /// <returns><c>true</c> if the specified object is equal to the current domain error; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => obj is DomainError other && Code == other.Code && Message == other.Message;

    /// <summary>
    /// Returns a hash code for this domain error.
    /// </summary>
    /// <returns>A hash code for the current domain error.</returns>
    public override int GetHashCode() => HashCode.Combine(Code, Message);
}