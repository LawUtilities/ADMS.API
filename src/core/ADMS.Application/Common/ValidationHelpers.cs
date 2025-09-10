using ADMS.Application.Constants;

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ADMS.Application.Common;

/// <summary>
/// Centralized validation helper for Matter-related operations.
/// </summary>
public static class MatterValidationHelper
{
    private static readonly string[] ReservedWords = ["SYSTEM", "ADMIN", "ROOT", "NULL", "UNDEFINED"];
    private static readonly Regex DescriptionRegex = new(@"^[a-zA-Z0-9\s\-_.()]+$", RegexOptions.Compiled);

    /// <summary>
    /// Validates matter description against business rules.
    /// </summary>
    public static IEnumerable<ValidationResult> ValidateDescription(string? description, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            yield return new ValidationResult($"{propertyName} is required.", [propertyName]);
            yield break;
        }

        var trimmed = description.Trim();

        if (trimmed.Length < MatterConstants.DescriptionMinLength)
        {
            yield return new ValidationResult(
                $"{propertyName} must be at least {MatterConstants.DescriptionMinLength} characters.",
                [propertyName]);
        }

        if (trimmed.Length > MatterConstants.DescriptionMaxLength)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot exceed {MatterConstants.DescriptionMaxLength} characters.",
                [propertyName]);
        }

        if (!DescriptionRegex.IsMatch(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} contains invalid characters. Only letters, numbers, spaces, hyphens, underscores, dots, and parentheses are allowed.",
                [propertyName]);
        }

        var normalized = NormalizeDescription(trimmed);
        if (ReservedWords.Contains(normalized?.ToUpperInvariant()))
        {
            yield return new ValidationResult(
                $"{propertyName} cannot use reserved words: {string.Join(", ", ReservedWords)}",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates creation/modification dates.
    /// </summary>
    public static IEnumerable<ValidationResult> ValidateDate(DateTime date, string propertyName)
    {
        if (date == default)
        {
            yield return new ValidationResult($"{propertyName} is required.", [propertyName]);
            yield break;
        }

        var minDate = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var maxDate = DateTime.UtcNow.AddDays(1); // Allow slight future tolerance

        if (date < minDate)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot be earlier than {minDate:yyyy-MM-dd}.",
                [propertyName]);
        }

        if (date > maxDate)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot be in the future.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates matter state consistency.
    /// </summary>
    public static IEnumerable<ValidationResult> ValidateStates(
        bool isArchived,
        bool isDeleted,
        string archivedPropertyName,
        string deletedPropertyName)
    {
        // Currently, all state combinations are valid, but this could change
        // For now, we'll just validate basic business rules

        if (isDeleted && !isArchived)
        {
            // Business rule: Deleted matters should typically be archived first
            // This is a warning, not an error
            yield return new ValidationResult(
                "Deleted matters should typically be archived first.",
                [archivedPropertyName, deletedPropertyName]);
        }
    }

    /// <summary>
    /// Normalizes description for consistent comparison.
    /// </summary>
    public static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        // Remove extra whitespace, normalize case
        return Regex.Replace(description.Trim(), @"\s+", " ");
    }

    /// <summary>
    /// Validates if description passes business rules.
    /// </summary>
    public static bool IsValidDescription(string? description) =>
        !ValidateDescription(description, "Description").Any();

    /// <summary>
    /// Validates if date passes business rules.
    /// </summary>
    public static bool IsValidDate(DateTime date) =>
        !ValidateDate(date, "Date").Any();

    /// <summary>
    /// Validates matter archive state consistency.
    /// </summary>
    public static bool IsValidArchiveState(bool isArchived, bool isDeleted) =>
        !ValidateStates(isArchived, isDeleted, "IsArchived", "IsDeleted").Any();
}

/// <summary>
/// Helper for validating DTO collections and complex objects.
/// </summary>
public static class DtoValidationHelper
{
    /// <summary>
    /// Validates a collection of DTOs and returns any validation errors.
    /// </summary>
    public static IEnumerable<ValidationResult> ValidateCollection<T>(
        IEnumerable<T>? collection,
        string propertyName) where T : class
    {
        if (collection is null)
        {
            yield return new ValidationResult($"{propertyName} collection cannot be null.", [propertyName]);
            yield break;
        }

        var items = collection.ToList();

        // Check for null items
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i] is null)
            {
                yield return new ValidationResult(
                    $"{propertyName}[{i}] cannot be null.",
                    [$"{propertyName}[{i}]"]);
            }
        }

        // If the items implement IValidatableObject, validate them
        foreach (var item in items.OfType<IValidatableObject>().Select((item, index) => new { item, index }))
        {
            var context = new ValidationContext(item.item);
            var itemResults = item.item.Validate(context);

            foreach (var result in itemResults)
            {
                var memberNames = result.MemberNames.Select(name => $"{propertyName}[{item.index}].{name}");
                yield return new ValidationResult(result.ErrorMessage, memberNames);
            }
        }
    }

    /// <summary>
    /// Validates that a collection doesn't exceed reasonable size limits.
    /// </summary>
    public static IEnumerable<ValidationResult> ValidateCollectionSize<T>(
        ICollection<T>? collection,
        string propertyName,
        int maxItems = 10000)
    {
        if (collection is null)
            yield break;

        if (collection.Count > maxItems)
        {
            yield return new ValidationResult(
                $"{propertyName} collection exceeds maximum size of {maxItems} items.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates required properties on an object using reflection.
    /// </summary>
    public static IEnumerable<ValidationResult> ValidateRequiredProperties<T>(T obj) where T : class
    {
        var type = typeof(T);
        var properties = type.GetProperties()
            .Where(p => p.GetCustomAttribute<RequiredAttribute>() != null);

        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            var requiredAttr = property.GetCustomAttribute<RequiredAttribute>()!;

            if (value is not null && (value is not string str || !string.IsNullOrWhiteSpace(str))) continue;
            var errorMessage = string.IsNullOrEmpty(requiredAttr.ErrorMessage)
                ? $"{property.Name} is required."
                : requiredAttr.ErrorMessage;

            yield return new ValidationResult(errorMessage, [property.Name]);
        }
    }
}

/// <summary>
/// Domain error representation for Result pattern.
/// </summary>
public sealed record DomainError
{
    /// <summary>
    /// Gets the code associated with the current instance.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the message associated with the current instance.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the underlying error that caused the current error, if available.
    /// </summary>
    public DomainError? InnerError { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainError"/> class with the specified error code, message, and an
    /// optional inner error.
    /// </summary>
    /// <remarks>This constructor is typically used to create a hierarchical structure of domain errors, where
    /// the <paramref name="innerError"/> provides additional context for the root error.</remarks>
    /// <param name="code">The unique code that identifies the domain error. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="message">A descriptive message that explains the error. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="innerError">An optional inner <see cref="DomainError"/> instance that provides additional context for the error. Can be <see
    /// langword="null"/>.</param>
    private DomainError(string code, string message, DomainError? innerError = null)
    {
        Code = code;
        Message = message;
        InnerError = innerError;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="DomainError"/> class with the specified error code, message, and an
    /// optional inner error.
    /// </summary>
    /// <param name="code">The unique code that identifies the error. This value cannot be null or empty.</param>
    /// <param name="message">A descriptive message that explains the error. This value cannot be null or empty.</param>
    /// <param name="innerError">An optional inner <see cref="DomainError"/> that provides additional context for the error. Can be null.</param>
    /// <returns>A new <see cref="DomainError"/> instance initialized with the specified code, message, and inner error.</returns>
    public static DomainError Create(string code, string message, DomainError? innerError = null) =>
        new(code, message, innerError);

    /// <summary>
    /// Gets a predefined error representing an unknown error condition.
    /// </summary>
    public static DomainError Unknown => new("UNKNOWN", "An unknown error occurred.");

    /// <summary>
    /// Returns a string representation of the error, including the error code, message,  and any inner error details if
    /// available.
    /// </summary>
    /// <returns>A string that contains the error code and message. If an inner error is present,  its details are appended to
    /// the string.</returns>
    public override string ToString() =>
        InnerError is not null
            ? $"{Code}: {Message} -> {InnerError}"
            : $"{Code}: {Message}";
}

/// <summary>
/// Result pattern implementation for error handling.
/// </summary>
public readonly struct Result
{
    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation was unsuccessful.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with the current operation, if any.
    /// </summary>
    public DomainError? Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class with the specified success state and error details.
    /// </summary>
    /// <remarks>This constructor is intended for internal use to create a <see cref="Result"/> instance
    /// representing either a successful or failed operation.</remarks>
    /// <param name="isSuccess">A value indicating whether the operation was successful.</param>
    /// <param name="error">The error associated with the operation, or <see langword="null"/> if the operation was successful.</param>
    private Result(bool isSuccess, DomainError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result with no associated error message.
    /// </summary>
    /// <returns>A <see cref="Result"/> instance representing a successful operation.</returns>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="value">The value to include in the successful result.</param>
    /// <returns>A <see cref="Result{T}"/> instance representing a successful operation with the specified value.</returns>
    public static Result<T> Success<T>(T value) => new(value, true, null);

    /// <summary>
    /// Creates a failed <see cref="Result"/> instance with the specified error.
    /// </summary>
    /// <param name="error">The <see cref="DomainError"/> representing the reason for the failure. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="Result"/> instance indicating failure, containing the provided error.</returns>
    public static Result Failure(DomainError error) => new(false, error);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <typeparam name="T">The type of the value associated with the result.</typeparam>
    /// <param name="error">The error describing the reason for the failure. Cannot be <c>null</c>.</param>
    /// <returns>A <see cref="Result{T}"/> instance representing a failure, containing the specified error.</returns>
    public static Result<T> Failure<T>(DomainError error) => new(default, false, error);
}

/// <summary>
/// Generic Result pattern implementation.
/// </summary>
public readonly struct Result<T>
{
    /// <summary>
    /// Gets the value stored in the current instance, or <see langword="null"/> if no value is present.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation was unsuccessful.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with the current operation, if any.
    /// </summary>
    public DomainError? Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class with the specified value, success status, and
    /// error details.
    /// </summary>
    /// <param name="value">The value associated with the result. This may be <see langword="null"/> if the operation did not produce a
    /// value.</param>
    /// <param name="isSuccess"><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</param>
    /// <param name="error">The error details associated with the result, or <see langword="null"/> if the operation was successful.</param>
    internal Result(T? value, bool isSuccess, DomainError? error)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> to a <see cref="Result{T}"/> instance representing
    /// a successful result.
    /// </summary>
    /// <param name="value">The value to be wrapped in a successful <see cref="Result{T}"/>.</param>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <param name="value">The value to be encapsulated in the successful result.</param>
    /// <returns>A <see cref="Result{T}"/> instance representing a successful operation with the specified value.</returns>
    public static Result<T> Success(T value) => new(value, true, null);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The <see cref="DomainError"/> that describes the reason for the failure. This parameter cannot be <see
    /// langword="null"/>.</param>
    /// <returns>A <see cref="Result{T}"/> instance representing a failure, containing the specified error.</returns>
    public static Result<T> Failure(DomainError error) => new(default, false, error);
}

/// <summary>
/// Interface marker for documents that have creation dates (for type safety).
/// </summary>
public interface IDocumentWithCreationDate
{
    /// <summary>
    /// Gets the date and time when the object was created.
    /// </summary>
    DateTime CreationDate { get; }
}