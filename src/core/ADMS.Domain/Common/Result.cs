using System.Diagnostics.CodeAnalysis;

namespace ADMS.Domain.Common;

/// <summary>
/// Represents the result of a domain operation that may succeed or fail.
/// </summary>
/// <remarks>
/// This class implements the Result pattern, which is a functional programming
/// approach to error handling that makes error states explicit and helps avoid
/// exceptions for business rule violations.
/// </remarks>
public class Result : IEquatable<Result>
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    /// <value><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</value>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    /// <value><c>true</c> if the operation failed; otherwise, <c>false</c>.</value>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error information if the operation failed.
    /// </summary>
    /// <value>A <see cref="DomainError"/> containing details about the failure, or null if successful.</value>
    public DomainError? Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error information if the operation failed.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when success is true but error is not null, or when success is false but error is null.
    /// </exception>
    protected Result(bool isSuccess, DomainError? error)
    {
        if (isSuccess && error != null)
            throw new ArgumentException("Successful result cannot have an error.", nameof(error));

        if (!isSuccess && error == null)
            throw new ArgumentException("Failed result must have an error.", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A <see cref="Result"/> indicating success.</returns>
    public static Result Success() => new(true, null);

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
    /// <param name="error">The error describing why the operation failed.</param>
    /// <returns>A <see cref="Result"/> indicating failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is null.</exception>
    public static Result Failure(DomainError error) => new(false, error ?? throw new ArgumentNullException(nameof(error)));

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message describing why the operation failed.</param>
    /// <returns>A <see cref="Result"/> indicating failure.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errorMessage"/> is null or empty.</exception>
    public static Result Failure(string errorMessage) => Failure(DomainError.Create("OPERATION_FAILED", errorMessage));

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <typeparam name="T">The type that would have been returned on success.</typeparam>
    /// <param name="error">The error describing why the operation failed.</param>
    /// <returns>A <see cref="Result{T}"/> indicating failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is null.</exception>
    public static Result<T> Failure<T>(DomainError error) => new(default!, false, error ?? throw new ArgumentNullException(nameof(error)));

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <typeparam name="T">The type that would have been returned on success.</typeparam>
    /// <param name="errorMessage">The error message describing why the operation failed.</param>
    /// <returns>A <see cref="Result{T}"/> indicating failure.</returns>
    public static Result<T> Failure<T>(string errorMessage) => Failure<T>(DomainError.Create("OPERATION_FAILED", errorMessage));

    /// <summary>
    /// Combines multiple results into a single result that succeeds only if all input results succeed.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>A successful result if all inputs succeed; otherwise, the first failure.</returns>
    public static Result Combine(params Result[] results) => Combine(results as IEnumerable<Result>);

    /// <summary>
    /// Combines multiple results into a single result that succeeds only if all input results succeed.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>A successful result if all inputs succeed; otherwise, the first failure.</returns>
    public static Result Combine(IEnumerable<Result> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }

        return Success();
    }

    /// <summary>
    /// Combines multiple results and collects all errors.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>A successful result if all inputs succeed; otherwise, a result with all collected errors.</returns>
    public static Result CombineAll(IEnumerable<Result> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var errors = new List<DomainError>();

        foreach (var result in results)
        {
            if (result.IsFailure && result.Error != null)
            {
                errors.Add(result.Error);
            }
        }

        return errors.Count == 0
            ? Success()
            : Failure(DomainError.Create("MULTIPLE_ERRORS",
                $"Multiple errors occurred: {string.Join("; ", errors.Select(e => e.Message))}"));
    }

    /// <summary>
    /// Tries to execute an operation and returns a Result.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <returns>A successful result if the operation completes without exception; otherwise, a failure result.</returns>
    public static Result Try(Action operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        try
        {
            operation();
            return Success();
        }
        catch (Exception ex)
        {
            return Failure(DomainError.Create("OPERATION_EXCEPTION", ex.Message));
        }
    }

    /// <summary>
    /// Tries to execute an operation and returns a Result with a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <returns>A successful result with the value if the operation completes without exception; otherwise, a failure result.</returns>
    public static Result<T> Try<T>(Func<T> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        try
        {
            var value = operation();
            return Success(value);
        }
        catch (Exception ex)
        {
            return Failure<T>(DomainError.Create("OPERATION_EXCEPTION", ex.Message));
        }
    }

    /// <summary>
    /// Executes the specified action if the result is successful.
    /// </summary>
    /// <param name="action">The action to execute on success.</param>
    /// <returns>The current result.</returns>
    public Result OnSuccess(Action action)
    {
        if (IsSuccess)
            action?.Invoke();

        return this;
    }

    /// <summary>
    /// Executes the specified action if the result is a failure.
    /// </summary>
    /// <param name="action">The action to execute on failure.</param>
    /// <returns>The current result.</returns>
    public Result OnFailure(Action<DomainError> action)
    {
        if (IsFailure && Error != null)
            action?.Invoke(Error);

        return this;
    }

    /// <summary>
    /// Executes the specified action regardless of the result status.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The current result.</returns>
    public Result Finally(Action action)
    {
        action?.Invoke();
        return this;
    }

    /// <summary>
    /// Transforms this result into a different result type by applying the specified function.
    /// </summary>
    /// <typeparam name="T">The type of the new result.</typeparam>
    /// <param name="func">The function to apply if this result is successful.</param>
    /// <returns>A new result of type T.</returns>
    public Result<T> Map<T>(Func<T> func)
    {
        return IsSuccess ? Success(func()) : Failure<T>(Error!);
    }

    /// <summary>
    /// Chains this result with another operation that returns a result.
    /// </summary>
    /// <param name="func">The function to apply if this result is successful.</param>
    /// <returns>The result of the function if this result is successful; otherwise, this failure.</returns>
    public Result Bind(Func<Result> func)
    {
        return IsSuccess ? func() : this;
    }

    /// <summary>
    /// Chains this result with an asynchronous operation that returns a result.
    /// </summary>
    /// <param name="func">The async function to apply if this result is successful.</param>
    /// <returns>The result of the function if this result is successful; otherwise, this failure.</returns>
    public async Task<Result> BindAsync(Func<Task<Result>> func)
    {
        return IsSuccess ? await func() : this;
    }

    /// <summary>
    /// Returns the current result if successful; otherwise, returns the alternative result.
    /// </summary>
    /// <param name="alternativeResult">The alternative result to return on failure.</param>
    /// <returns>This result if successful; otherwise, the alternative result.</returns>
    public Result Or(Result alternativeResult)
    {
        return IsSuccess ? this : alternativeResult;
    }

    /// <summary>
    /// Returns the current result if successful; otherwise, returns the result of the alternative function.
    /// </summary>
    /// <param name="alternativeFunc">The function to call to get an alternative result on failure.</param>
    /// <returns>This result if successful; otherwise, the result of the alternative function.</returns>
    public Result Or(Func<Result> alternativeFunc)
    {
        return IsSuccess ? this : alternativeFunc();
    }

    /// <summary>
    /// Ensures that a condition is true, otherwise returns a failure result.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="error">The error to return if the condition is false.</param>
    /// <returns>A successful result if the condition is true; otherwise, a failure result.</returns>
    public Result Ensure(bool condition, DomainError error)
    {
        if (IsFailure)
            return this;

        if (condition)
            return this;

        return Failure(error);
    }

    /// <summary>
    /// Implicitly converts a DomainError to a failed Result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed Result containing the error.</returns>
    [return: NotNullIfNotNull(nameof(error))]
    public static implicit operator Result?(DomainError? error) =>
        error == null ? null : Failure(error);

    /// <summary>
    /// Implicitly converts a boolean to a Result.
    /// </summary>
    /// <param name="success">The success status to convert.</param>
    /// <returns>A successful Result if true; otherwise, a failed Result with a generic error.</returns>
    public static implicit operator Result(bool success) =>
        success ? Success() : Failure("Operation failed.");

    /// <summary>
    /// Determines whether the specified result is equal to the current result.
    /// </summary>
    /// <param name="other">The result to compare with the current result.</param>
    /// <returns><c>true</c> if the results are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(Result? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return IsSuccess == other.IsSuccess && Equals(Error, other.Error);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current result.
    /// </summary>
    /// <param name="obj">The object to compare with the current result.</param>
    /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => Equals(obj as Result);

    /// <summary>
    /// Returns a hash code for this result.
    /// </summary>
    /// <returns>A hash code for the current result.</returns>
    public override int GetHashCode() => HashCode.Combine(IsSuccess, Error);

    /// <summary>
    /// Returns a string representation of the result.
    /// </summary>
    /// <returns>A string that represents the current result.</returns>
    public override string ToString() => IsSuccess ? "Success" : $"Failure: {Error}";

    /// <summary>
    /// Determines whether two result instances are equal.
    /// </summary>
    /// <param name="left">The first result to compare.</param>
    /// <param name="right">The second result to compare.</param>
    /// <returns><c>true</c> if the results are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Result? left, Result? right) => Equals(left, right);

    /// <summary>
    /// Determines whether two result instances are not equal.
    /// </summary>
    /// <param name="left">The first result to compare.</param>
    /// <param name="right">The second result to compare.</param>
    /// <returns><c>true</c> if the results are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Result? left, Result? right) => !Equals(left, right);
}

/// <summary>
/// Represents the result of a domain operation that returns a value on success.
/// </summary>
/// <typeparam name="T">The type of value returned on success.</typeparam>
/// <remarks>
/// This generic version of Result allows operations to return both success/failure
/// status and a value when successful, maintaining type safety throughout the operation.
/// </remarks>
public sealed class Result<T> : Result, IEquatable<Result<T>>
{
    /// <summary>
    /// Gets the value returned by the successful operation.
    /// </summary>
    /// <value>The value of type <typeparamref name="T"/> if successful; otherwise, the default value.</value>
    /// <remarks>This property should only be accessed when <see cref="Result.IsSuccess"/> is true.</remarks>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="value">The value to return on success.</param>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error information if the operation failed.</param>
    internal Result(T value, bool isSuccess, DomainError? error) : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the value if successful, or the default value if failed.
    /// </summary>
    /// <param name="defaultValue">The default value to return on failure.</param>
    /// <returns>The result value if successful; otherwise, the default value.</returns>
    public T GetValueOrDefault(T defaultValue = default!) => IsSuccess ? Value : defaultValue;

    /// <summary>
    /// Gets the value if successful, or the result of the default value function if failed.
    /// </summary>
    /// <param name="defaultValueFunc">A function that provides the default value on failure.</param>
    /// <returns>The result value if successful; otherwise, the result of the default value function.</returns>
    public T GetValueOrDefault(Func<T> defaultValueFunc)
    {
        ArgumentNullException.ThrowIfNull(defaultValueFunc);
        return IsSuccess ? Value : defaultValueFunc();
    }

    /// <summary>
    /// Executes the specified action with the value if the result is successful.
    /// </summary>
    /// <param name="action">The action to execute with the value on success.</param>
    /// <returns>The current result.</returns>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
            action?.Invoke(Value);

        return this;
    }

    /// <summary>
    /// Transforms the value of this result if successful.
    /// </summary>
    /// <typeparam name="TNew">The type of the new value.</typeparam>
    /// <param name="func">The function to apply to the value if successful.</param>
    /// <returns>A new result with the transformed value if successful; otherwise, the failure.</returns>
    public Result<TNew> Map<TNew>(Func<T, TNew> func)
    {
        return IsSuccess ? Success(func(Value)) : Failure<TNew>(Error!);
    }

    /// <summary>
    /// Chains this result with another operation that takes the value and returns a result.
    /// </summary>
    /// <typeparam name="TNew">The type of the new result value.</typeparam>
    /// <param name="func">The function to apply to the value if successful.</param>
    /// <returns>The result of the function if this result is successful; otherwise, this failure.</returns>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> func)
    {
        return IsSuccess ? func(Value) : Failure<TNew>(Error!);
    }

    /// <summary>
    /// Chains this result with an asynchronous operation that takes the value and returns a result.
    /// </summary>
    /// <typeparam name="TNew">The type of the new result value.</typeparam>
    /// <param name="func">The async function to apply to the value if successful.</param>
    /// <returns>The result of the function if this result is successful; otherwise, this failure.</returns>
    public async Task<Result<TNew>> BindAsync<TNew>(Func<T, Task<Result<TNew>>> func)
    {
        return IsSuccess ? await func(Value) : Failure<TNew>(Error!);
    }

    /// <summary>
    /// Returns the current result if successful; otherwise, returns the alternative result.
    /// </summary>
    /// <param name="alternativeResult">The alternative result to return on failure.</param>
    /// <returns>This result if successful; otherwise, the alternative result.</returns>
    public Result<T> Or(Result<T> alternativeResult)
    {
        return IsSuccess ? this : alternativeResult;
    }

    /// <summary>
    /// Returns the current result if successful; otherwise, returns the result of the alternative function.
    /// </summary>
    /// <param name="alternativeFunc">The function to call to get an alternative result on failure.</param>
    /// <returns>This result if successful; otherwise, the result of the alternative function.</returns>
    public Result<T> Or(Func<Result<T>> alternativeFunc)
    {
        return IsSuccess ? this : alternativeFunc();
    }

    /// <summary>
    /// Ensures that a condition based on the value is true, otherwise returns a failure result.
    /// </summary>
    /// <param name="predicate">The predicate to evaluate with the value.</param>
    /// <param name="error">The error to return if the predicate returns false.</param>
    /// <returns>This result if successful and the predicate returns true; otherwise, a failure result.</returns>
    public Result<T> Ensure(Func<T, bool> predicate, DomainError error)
    {
        if (IsFailure)
            return this;

        if (predicate(Value))
            return this;

        return Failure<T>(error);
    }

    /// <summary>
    /// Implicitly converts a value to a successful Result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful Result containing the value.</returns>
    [return: NotNullIfNotNull(nameof(value))]
    public static implicit operator Result<T>?([DisallowNull] T? value) =>
        EqualityComparer<T>.Default.Equals(value, default(T)) ? null : Success(value);

    /// <summary>
    /// Implicitly converts a DomainError to a failed Result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed Result containing the error.</returns>
    [return: NotNullIfNotNull(nameof(error))]
    public static implicit operator Result<T>?(DomainError? error) =>
        error == null ? null : Failure<T>(error);

    /// <summary>
    /// Determines whether the specified result is equal to the current result.
    /// </summary>
    /// <param name="other">The result to compare with the current result.</param>
    /// <returns><c>true</c> if the results are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(Result<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current result.
    /// </summary>
    /// <param name="obj">The object to compare with the current result.</param>
    /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => Equals(obj as Result<T>);

    /// <summary>
    /// Returns a hash code for this result.
    /// </summary>
    /// <returns>A hash code for the current result.</returns>
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Value);

    /// <summary>
    /// Returns a string representation of the result.
    /// </summary>
    /// <returns>A string that represents the current result.</returns>
    public override string ToString() => IsSuccess ? $"Success: {Value}" : $"Failure: {Error}";
}