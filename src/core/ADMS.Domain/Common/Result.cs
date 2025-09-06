namespace ADMS.Domain.Common;

/// <summary>
/// Represents the result of a domain operation that may succeed or fail.
/// </summary>
/// <remarks>
/// This class implements the Result pattern, which is a functional programming
/// approach to error handling that makes error states explicit and helps avoid
/// exceptions for business rule violations.
/// </remarks>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    /// <value>
    /// <c>true</c> if the operation succeeded; otherwise, <c>false</c>.
    /// </value>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    /// <value>
    /// <c>true</c> if the operation failed; otherwise, <c>false</c>.
    /// </value>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error information if the operation failed.
    /// </summary>
    /// <value>
    /// A <see cref="DomainError"/> containing details about the failure, or null if successful.
    /// </value>
    public DomainError Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error information if the operation failed.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when success is true but error is not null, or when success is false but error is null.
    /// </exception>
    protected Result(bool isSuccess, DomainError error)
    {
        if (isSuccess && error != null)
            throw new ArgumentException("Successful result cannot have an error", nameof(error));

        if (!isSuccess && error == null)
            throw new ArgumentException("Failed result must have an error", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A <see cref="Result"/> indicating success.</returns>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error describing why the operation failed.</param>
    /// <returns>A <see cref="Result"/> indicating failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is null.</exception>
    public static Result Failure(DomainError error) => new(false, error ?? throw new ArgumentNullException(nameof(error)));

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to return.</param>
    /// <returns>A <see cref="Result{T}"/> containing the value.</returns>
    public static Result<T> Success<T>(T value) => new(value, true, null);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <typeparam name="T">The type that would have been returned on success.</typeparam>
    /// <param name="error">The error describing why the operation failed.</param>
    /// <returns>A <see cref="Result{T}"/> indicating failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is null.</exception>
    public static Result<T> Failure<T>(DomainError error) => new(default(T), false, error ?? throw new ArgumentNullException(nameof(error)));
}

/// <summary>
/// Represents the result of a domain operation that returns a value on success.
/// </summary>
/// <typeparam name="T">The type of value returned on success.</typeparam>
/// <remarks>
/// This generic version of Result allows operations to return both success/failure
/// status and a value when successful, maintaining type safety throughout the operation.
/// </remarks>
public class Result<T> : Result
{
    /// <summary>
    /// Gets the value returned by the successful operation.
    /// </summary>
    /// <value>
    /// The value of type <typeparamref name="T"/> if successful; otherwise, the default value.
    /// </value>
    /// <remarks>
    /// This property should only be accessed when <see cref="Result.IsSuccess"/> is true.
    /// Accessing it when the result failed may return default(T) or throw an exception.
    /// </remarks>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="value">The value to return on success.</param>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error information if the operation failed.</param>
    internal Result(T value, bool isSuccess, DomainError error) : base(isSuccess, error)
    {
        Value = value;
    }
}