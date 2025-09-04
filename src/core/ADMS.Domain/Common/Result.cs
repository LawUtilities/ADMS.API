namespace ADMS.Domain.Common;

/// <summary>
/// Represents the result of a domain operation that can succeed or fail.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public DomainError Error { get; }

    protected Result(bool isSuccess, DomainError error)
    {
        if (isSuccess && error != DomainError.None)
            throw new InvalidOperationException("Success result cannot have an error");

        if (!isSuccess && error == DomainError.None)
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, DomainError.None);
    public static Result Failure(DomainError error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, DomainError.None);
    public static Result<T> Failure<T>(DomainError error) => new(default!, false, error);
}

/// <summary>
/// Generic result that includes a value on success.
/// </summary>
public class Result<T> : Result
{
    public T Value { get; }

    internal Result(T value, bool isSuccess, DomainError error) : base(isSuccess, error)
    {
        Value = value;
    }
}
