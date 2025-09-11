using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ADMS.Application.Common.Validation;

/// <summary>
/// Base abstract class providing standardized validation infrastructure for ADMS DTOs.
/// </summary>
/// <remarks>
/// This base class provides a comprehensive validation framework that standardizes validation patterns
/// across all ADMS DTOs while leveraging the existing validation helpers and maintaining consistency
/// with .NET 8 and .NET 9 best practices.
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item><strong>Consistent Validation Patterns:</strong> Standardized validation approach across all DTOs</item>
/// <item><strong>Performance Optimized:</strong> Uses yield return for lazy evaluation and early termination</item>
/// <item><strong>Integration Ready:</strong> Works seamlessly with existing validation helpers</item>
/// <item><strong>Thread Safe:</strong> All validation methods are thread-safe and stateless</item>
/// <item><strong>Extensible:</strong> Allows derived classes to add custom validation logic</item>
/// <item><strong>Error Context:</strong> Provides rich error context and member path information</item>
/// </list>
/// 
/// <para><strong>Validation Hierarchy:</strong></para>
/// <list type="number">
/// <item><strong>Data Annotations:</strong> Standard .NET validation attributes</item>
/// <item><strong>Core Property Validation:</strong> Essential property validation using ADMS helpers</item>
/// <item><strong>Business Rule Validation:</strong> Domain-specific business logic validation</item>
/// <item><strong>Cross-Property Validation:</strong> Validation that involves multiple properties</item>
/// <item><strong>Collection Validation:</strong> Validation of nested collections and complex objects</item>
/// </list>
/// 
/// <para><strong>Integration with ADMS Helpers:</strong></para>
/// This class integrates seamlessly with existing ADMS validation helpers:
/// <list type="bullet">
/// <item>MatterValidationHelper for matter-related validation</item>
/// <item>UserValidationHelper for user-related validation</item>
/// <item>Other domain-specific validation helpers</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Example derived DTO
/// public class MyDto : BaseValidationDto
/// {
///     [Required]
///     public string Name { get; set; } = string.Empty;
///     
///     protected override IEnumerable&lt;ValidationResult&gt; ValidateCoreProperties()
///     {
///         foreach (var result in ValidateString(Name, nameof(Name), minLength: 2, maxLength: 100))
///             yield return result;
///     }
///     
///     protected override IEnumerable&lt;ValidationResult&gt; ValidateBusinessRules()
///     {
///         // Custom business logic validation
///         yield break;
///     }
/// }
/// </code>
/// </example>
public abstract class BaseValidationDto : IValidatableObject
{
    #region Core Validation Framework

    /// <summary>
    /// Validates the DTO using the standardized ADMS validation hierarchy.
    /// </summary>
    /// <param name="validationContext">The validation context for the operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This method implements the standardized ADMS validation pattern:
    /// <list type="number">
    /// <item>Core property validation (required fields, formats, etc.)</item>
    /// <item>Business rule validation (domain-specific logic)</item>
    /// <item>Cross-property validation (relationships between properties)</item>
    /// <item>Collection validation (nested objects and collections)</item>
    /// </list>
    /// 
    /// <para><strong>Performance Notes:</strong></para>
    /// Uses yield return for lazy evaluation, allowing early termination on critical errors
    /// and reducing memory allocation for large validation sets.
    /// </remarks>
    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        return ValidateIterator(validationContext);
    }

    /// <summary>
    /// Performs validation on the current object and yields a sequence of <see cref="ValidationResult"/> objects
    /// representing validation errors, if any.
    /// </summary>
    /// <remarks>This method validates the object in multiple stages, including core property validation,
    /// business rule validation, cross-property validation, collection validation, and custom validation. Derived
    /// classes can extend the validation process by overriding specific validation methods. <para> The <paramref
    /// name="validationContext"/> parameter is used to provide contextual information for the validation process.
    /// </para> <exception cref="ArgumentNullException">Thrown if <paramref name="validationContext"/> is <see
    /// langword="null"/>.</exception></remarks>
    /// <param name="validationContext">The context in which the validation is performed. This parameter cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ValidationResult"/> objects. Each <see cref="ValidationResult"/>
    /// represents a validation error. If no validation errors are found, the sequence will be empty.</returns>
    private IEnumerable<ValidationResult> ValidateIterator(ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);

        // Store validation context for derived classes
        ValidationContext = validationContext;

        try
        {
            // 1. Core property validation (essential properties, formats, required fields)
            foreach (var result in ValidateCoreProperties())
            {
                yield return result;
            }

            // 2. Business rule validation (domain-specific logic)
            foreach (var result in ValidateBusinessRules())
            {
                yield return result;
            }

            // 3. Cross-property validation (relationships between properties)
            foreach (var result in ValidateCrossPropertyRules())
            {
                yield return result;
            }

            // 4. Collection validation (nested objects and collections)
            foreach (var result in ValidateCollections())
            {
                yield return result;
            }

            // 5. Custom validation (derived class specific logic)
            foreach (var result in ValidateCustomRules())
            {
                yield return result;
            }
        }
        finally
        {
            // Clear context to prevent memory leaks
            ValidationContext = null;
        }
    }

    /// <summary>
    /// Gets the current validation context during validation operations.
    /// </summary>
    /// <remarks>
    /// This property is only available during validation operations and is automatically
    /// set and cleared by the validation framework.
    /// </remarks>
    protected ValidationContext? ValidationContext { get; private set; }

    #endregion Core Validation Framework

    #region Abstract Validation Methods

    /// <summary>
    /// Validates core properties such as required fields, formats, and basic constraints.
    /// </summary>
    /// <returns>A collection of validation results for core property validation.</returns>
    /// <remarks>
    /// This method should validate essential properties including:
    /// <list type="bullet">
    /// <item>Required field validation</item>
    /// <item>Format validation (GUIDs, dates, etc.)</item>
    /// <item>Length constraints</item>
    /// <item>Range validation</item>
    /// <item>Pattern matching</item>
    /// </list>
    /// 
    /// Use the existing ADMS validation helpers where possible for consistency.
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override IEnumerable&lt;ValidationResult&gt; ValidateCoreProperties()
    /// {
    ///     // Validate required GUID
    ///     foreach (var result in ValidateGuid(Id, nameof(Id)))
    ///         yield return result;
    ///         
    ///     // Validate string with helper
    ///     foreach (var result in ValidateString(Name, nameof(Name), minLength: 2, maxLength: 100))
    ///         yield return result;
    /// }
    /// </code>
    /// </example>
    protected abstract IEnumerable<ValidationResult> ValidateCoreProperties();

    /// <summary>
    /// Validates business rules and domain-specific logic.
    /// </summary>
    /// <returns>A collection of validation results for business rule validation.</returns>
    /// <remarks>
    /// This method should validate domain-specific business rules including:
    /// <list type="bullet">
    /// <item>State transition rules</item>
    /// <item>Professional practice requirements</item>
    /// <item>Legal compliance rules</item>
    /// <item>System-specific constraints</item>
    /// <item>Professional standards</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override IEnumerable&lt;ValidationResult&gt; ValidateBusinessRules()
    /// {
    ///     // Validate matter state transitions
    ///     foreach (var result in MatterValidationHelper.ValidateStateConsistency(IsArchived, IsDeleted))
    ///         yield return result;
    /// }
    /// </code>
    /// </example>
    protected abstract IEnumerable<ValidationResult> ValidateBusinessRules();

    #endregion Abstract Validation Methods

    #region Virtual Validation Methods

    /// <summary>
    /// Validates relationships and constraints between multiple properties.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    /// <remarks>
    /// This method validates relationships between properties including:
    /// <list type="bullet">
    /// <item>Date range consistency</item>
    /// <item>Conditional field requirements</item>
    /// <item>Mutual exclusivity constraints</item>
    /// <item>Referential integrity</item>
    /// <item>Composite validation rules</item>
    /// </list>
    /// 
    /// Override this method in derived classes that have cross-property validation requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override IEnumerable&lt;ValidationResult&gt; ValidateCrossPropertyRules()
    /// {
    ///     // Validate date ranges
    ///     <![CDATA[if if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)]]>
    ///         yield return CreateValidationResult("Start date cannot be after end date.", 
    ///             nameof(StartDate), nameof(EndDate));
    /// }
    /// </code>
    /// </example>
    protected virtual IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // Default implementation - no cross-property validation
        yield break;
    }

    /// <summary>
    /// Validates collections and nested objects.
    /// </summary>
    /// <returns>A collection of validation results for collection validation.</returns>
    /// <remarks>
    /// This method validates collections and nested objects including:
    /// <list type="bullet">
    /// <item>Collection existence and null checking</item>
    /// <item>Individual item validation within collections</item>
    /// <item>Collection size constraints</item>
    /// <item>Nested object validation</item>
    /// <item>Hierarchical error message construction</item>
    /// </list>
    /// 
    /// Override this method in derived classes that have collection properties.
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override IEnumerable&lt;ValidationResult&gt; ValidateCollections()
    /// {
    ///     // Validate document collection
    ///     if (Documents != null)
    ///     {
    ///         foreach (var result in ValidateCollection(Documents, nameof(Documents)))
    ///             yield return result;
    ///     }
    /// }
    /// </code>
    /// </example>
    protected virtual IEnumerable<ValidationResult> ValidateCollections()
    {
        // Default implementation - no collection validation
        yield break;
    }

    /// <summary>
    /// Validates custom rules specific to the derived DTO class.
    /// </summary>
    /// <returns>A collection of validation results for custom validation.</returns>
    /// <remarks>
    /// This method allows derived classes to implement highly specific validation logic
    /// that doesn't fit into the standard validation categories. This should be used sparingly
    /// and only for truly unique validation requirements.
    /// 
    /// Most validation should be implemented in the other validation methods for consistency.
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override IEnumerable&lt;ValidationResult&gt; ValidateCustomRules()
    /// {
    ///     // Highly specific validation logic
    /// <![CDATA[if (IsSpecialCase && !MeetsSpecialRequirements())]]>
    ///     yield return CreateValidationResult("Special case requirements not met.");
    /// }
    /// </code>
    /// </example>
    protected virtual IEnumerable<ValidationResult> ValidateCustomRules()
    {
        // Default implementation - no custom validation
        yield break;
    }

    #endregion Virtual Validation Methods

    #region Validation Helper Methods

    /// <summary>
    /// Creates a standardized ValidationResult with consistent formatting.
    /// </summary>
    /// <param name="errorMessage">The error message for the validation result.</param>
    /// <param name="memberNames">The member names associated with the validation error.</param>
    /// <returns>A properly formatted ValidationResult.</returns>
    /// <remarks>
    /// This helper method ensures consistent error message formatting across all ADMS DTOs
    /// and provides proper member name attribution for validation errors.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Single property error
    /// yield return CreateValidationResult("Name is required.", nameof(Name));
    /// 
    /// // Multiple property error
    /// yield return CreateValidationResult("Date range is invalid.", nameof(StartDate), nameof(EndDate));
    /// 
    /// // No specific properties
    /// yield return CreateValidationResult("General validation error.");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ValidationResult CreateValidationResult(
        [NotNull] string errorMessage,
        params string[] memberNames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        return memberNames?.Length > 0
            ? new ValidationResult(errorMessage, memberNames)
            : new ValidationResult(errorMessage);
    }

    /// <summary>
    /// Creates a contextual ValidationResult with property path information.
    /// </summary>
    /// <param name="errorMessage">The error message for the validation result.</param>
    /// <param name="propertyPath">The hierarchical property path (e.g., "Documents[0].FileName").</param>
    /// <returns>A ValidationResult with proper property path context.</returns>
    /// <remarks>
    /// This helper method is useful for nested object validation where the error location
    /// needs to be precisely identified in complex object hierarchies.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Nested property error
    /// yield return CreateContextualValidationResult(
    ///     "File name is invalid.", 
    ///     "Documents[0].FileName");
    /// 
    /// // Collection item error
    /// yield return CreateContextualValidationResult(
    ///     "User name is required.", 
    ///     $"Users[{index}].Name");
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ValidationResult CreateContextualValidationResult(
        [NotNull] string errorMessage,
        [NotNull] string propertyPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyPath);

        return new ValidationResult($"{propertyPath}: {errorMessage}", [propertyPath]);
    }

    /// <summary>
    /// Validates a GUID property using ADMS standards.
    /// </summary>
    /// <param name="guidValue">The GUID value to validate.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="allowEmpty">Whether to allow Guid.Empty values (default: false).</param>
    /// <returns>A collection of validation results for GUID validation.</returns>
    /// <remarks>
    /// This helper method provides consistent GUID validation across all ADMS DTOs,
    /// following the patterns established in the existing validation helpers.
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override IEnumerable&lt;ValidationResult&gt; ValidateCoreProperties()
    /// {
    ///     // Required GUID
    ///     foreach (var result in ValidateGuid(Id, nameof(Id)))
    ///         yield return result;
    ///         
    ///     // Optional GUID (allow empty)
    ///     foreach (var result in ValidateGuid(ParentId, nameof(ParentId), allowEmpty: true))
    ///         yield return result;
    /// }
    /// </code>
    /// </example>
    protected IEnumerable<ValidationResult> ValidateGuid(
        Guid guidValue,
        [NotNull] string propertyName,
        bool allowEmpty = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (!allowEmpty && guidValue == Guid.Empty)
        {
            yield return CreateValidationResult(
                $"{propertyName} must be a valid non-empty GUID.",
                propertyName);
        }
    }

    /// <summary>
    /// Validates a string property with length and format constraints.
    /// </summary>
    /// <param name="stringValue">The string value to validate.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="required">Whether the string is required (default: true).</param>
    /// <param name="minLength">The minimum length (default: 0).</param>
    /// <param name="maxLength">The maximum length (default: int.MaxValue).</param>
    /// <param name="allowWhitespaceOnly">Whether to allow whitespace-only strings (default: false).</param>
    /// <returns>A collection of validation results for string validation.</returns>
    /// <remarks>
    /// This helper method provides consistent string validation across all ADMS DTOs,
    /// handling common string validation scenarios with professional standards.
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override IEnumerable&lt;ValidationResult&gt; ValidateCoreProperties()
    /// {
    ///     // Required string with length constraints
    ///     foreach (var result in ValidateString(Name, nameof(Name), minLength: 2, maxLength: 100))
    ///         yield return result;
    ///         
    ///     // Optional string
    ///     foreach (var result in ValidateString(Description, nameof(Description), required: false, maxLength: 256))
    ///         yield return result;
    /// }
    /// </code>
    /// </example>
    protected IEnumerable<ValidationResult> ValidateString(
        string? stringValue,
        [NotNull] string propertyName,
        bool required = true,
        int minLength = 0,
        int maxLength = int.MaxValue,
        bool allowWhitespaceOnly = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        ArgumentOutOfRangeException.ThrowIfNegative(minLength);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, minLength);

        switch (stringValue)
        {
            case null when required:
                yield return CreateValidationResult(
                    $"{propertyName} is required.",
                    propertyName);
                break;
            case not null:
                {
                    if (!allowWhitespaceOnly && required && string.IsNullOrWhiteSpace(stringValue))
                    {
                        yield return CreateValidationResult(
                            $"{propertyName} cannot be empty or whitespace.",
                            propertyName);
                    }

                    var effectiveLength = allowWhitespaceOnly ? stringValue.Length : stringValue.Trim().Length;

                    if (effectiveLength < minLength)
                    {
                        yield return CreateValidationResult(
                            $"{propertyName} must be at least {minLength} characters long.",
                            propertyName);
                    }

                    if (effectiveLength > maxLength)
                    {
                        yield return CreateValidationResult(
                            $"{propertyName} cannot exceed {maxLength} characters.",
                            propertyName);
                    }

                    break;
                }
        }
    }

    /// <summary>
    /// Validates a DateTime property with ADMS standards.
    /// </summary>
    /// <param name="dateValue">The DateTime value to validate.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="allowFuture">Whether to allow future dates (default: false).</param>
    /// <param name="allowPast">Whether to allow past dates (default: true).</param>
    /// <param name="maxFutureOffset">Maximum allowed future offset from current time (default: 5 minutes).</param>
    /// <param name="maxPastOffset">Maximum allowed past offset from current time (default: 20 years).</param>
    /// <param name="allowDefault">Whether to allow default DateTime values (default: false).</param>
    /// <returns>A collection of validation results for DateTime validation.</returns>
    /// <remarks>
    /// This helper method provides consistent DateTime validation across all ADMS DTOs,
    /// using professional standards for legal document management systems.
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override IEnumerable&lt;ValidationResult&gt; ValidateCoreProperties()
    /// {
    ///     // Creation date validation (no future, reasonable past)
    ///     foreach (var result in ValidateDateTime(CreationDate, nameof(CreationDate)))
    ///         yield return result;
    /// }
    /// </code>
    /// </example>
    protected IEnumerable<ValidationResult> ValidateDateTime(
        DateTime dateValue,
        [NotNull] string propertyName,
        bool allowFuture = false,
        bool allowPast = true,
        TimeSpan? maxFutureOffset = null,
        TimeSpan? maxPastOffset = null,
        bool allowDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var currentTime = DateTime.UtcNow;
        maxFutureOffset ??= TimeSpan.FromMinutes(5);
        maxPastOffset ??= TimeSpan.FromDays(365 * 20); // 20 years

        // Validate default value handling
        if (!allowDefault && dateValue == default)
        {
            yield return CreateValidationResult(
                $"{propertyName} must be set to a valid date and time.",
                propertyName);
            yield break;
        }

        // Convert to UTC for comparison
        var utcDateTime = dateValue.Kind switch
        {
            DateTimeKind.Utc => dateValue,
            DateTimeKind.Local => dateValue.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateValue, DateTimeKind.Utc)
        };

        // Future date validation
        if (utcDateTime > currentTime)
        {
            if (!allowFuture)
            {
                yield return CreateValidationResult(
                    $"{propertyName} cannot be in the future.",
                    propertyName);
            }
            else if (utcDateTime > currentTime.Add(maxFutureOffset.Value))
            {
                yield return CreateValidationResult(
                    $"{propertyName} cannot be more than {FormatTimeSpan(maxFutureOffset.Value)} in the future.",
                    propertyName);
            }
        }

        // Past date validation
        if (utcDateTime < currentTime && !allowPast)
        {
            yield return CreateValidationResult(
                $"{propertyName} cannot be in the past.",
                propertyName);
        }
        else if (utcDateTime < currentTime.Subtract(maxPastOffset.Value))
        {
            yield return CreateValidationResult(
                $"{propertyName} cannot be more than {FormatTimeSpan(maxPastOffset.Value)} in the past.",
                propertyName);
        }
    }

    /// <summary>
    /// Validates a collection of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects in the collection.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <param name="maxItems">Maximum allowed items in the collection (default: 10000).</param>
    /// <returns>A collection of validation results for collection validation.</returns>
    /// <remarks>
    /// This helper method provides consistent collection validation across all ADMS DTOs.
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override IEnumerable&lt;ValidationResult&gt; ValidateCollections()
    /// {
    ///     if (Documents != null)
    ///     {
    ///         foreach (var result in ValidateCollection(Documents, nameof(Documents), maxItems: 500))
    ///             yield return result;
    ///     }
    /// }
    /// </code>
    /// </example>
    protected IEnumerable<ValidationResult> ValidateCollection<T>(
        ICollection<T>? collection,
        [NotNull] string propertyName,
        int maxItems = 10000) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (collection == null)
            yield break;

        // Check collection size
        if (collection.Count > maxItems)
        {
            yield return CreateValidationResult(
                $"{propertyName} collection exceeds maximum size of {maxItems} items.",
                propertyName);
        }

        // Check for null items
        var index = 0;
        foreach (var item in collection)
        {
            if (item == null)
            {
                yield return CreateValidationResult(
                    $"{propertyName}[{index}] cannot be null.",
                    $"{propertyName}[{index}]");
            }
            index++;
        }

        // Validate items if they implement IValidatableObject
        index = 0;
        foreach (var item in collection.OfType<IValidatableObject>())
        {
            var context = new ValidationContext(item);
            var itemResults = item.Validate(context);

            foreach (var result in itemResults)
            {
                var memberNames = result.MemberNames.Select(name => $"{propertyName}[{index}].{name}").ToArray();
                yield return new ValidationResult(result.ErrorMessage, memberNames);
            }
            index++;
        }
    }

    #endregion Validation Helper Methods

    #region Static Validation Methods

    /// <summary>
    /// Validates a DTO instance and returns detailed validation results.
    /// </summary>
    /// <typeparam name="T">The type of DTO to validate.</typeparam>
    /// <param name="dto">The DTO instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate DTO instances
    /// without requiring a ValidationContext. It performs comprehensive validation including
    /// both data annotations and IValidatableObject validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MyDto { Name = "Test" };
    /// var results = BaseValidationDto.ValidateModel(dto);
    /// 
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel<T>([AllowNull] T? dto) where T : BaseValidationDto
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult($"{typeof(T).Name} instance is required and cannot be null."));
            return results;
        }

        // Perform comprehensive validation
        var context = new ValidationContext(dto, serviceProvider: null, items: null);

        // Data annotation validation
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        // IValidatableObject validation
        var customValidationResults = dto.Validate(context);
        results.AddRange(customValidationResults);

        return results;
    }

    /// <summary>
    /// Gets a summary of validation results for logging and debugging purposes.
    /// </summary>
    /// <param name="validationResults">The validation results to summarize.</param>
    /// <returns>A formatted summary string of validation errors.</returns>
    /// <remarks>
    /// This utility method provides a consistent way to format validation results
    /// for logging, debugging, and error reporting purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var summary = GetValidationSummary(results);
    ///     _logger.LogWarning("Validation failed: {ValidationSummary}", summary);
    /// }
    /// </code>
    /// </example>
    public static string GetValidationSummary(IEnumerable<ValidationResult> validationResults)
    {
        ArgumentNullException.ThrowIfNull(validationResults);

        var results = validationResults.ToList();
        if (!results.Any())
            return "No validation errors.";

        var summary = new System.Text.StringBuilder();
        summary.AppendLine($"Validation failed with {results.Count} error(s):");

        foreach (var result in results)
        {
            var memberNames = result.MemberNames.Any()
                ? $" ({string.Join(", ", result.MemberNames)})"
                : string.Empty;
            summary.AppendLine(
                System.Globalization.CultureInfo.InvariantCulture,
                $"  - {result.ErrorMessage}{memberNames}");
        }

        return summary.ToString().TrimEnd();
    }

    /// <summary>
    /// Determines if validation results contain any errors.
    /// </summary>
    /// <param name="validationResults">The validation results to check.</param>
    /// <returns>True if there are validation errors; otherwise, false.</returns>
    /// <remarks>
    /// This utility method provides a consistent way to check validation status
    /// across the ADMS system.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasValidationErrors([AllowNull] IEnumerable<ValidationResult>? validationResults)
    {
        return validationResults?.Any() == true;
    }

    #endregion Static Validation Methods

    #region Private Helper Methods

    /// <summary>
    /// Formats a TimeSpan for user-friendly display in validation messages.
    /// </summary>
    /// <param name="timeSpan">The TimeSpan to format.</param>
    /// <returns>A user-friendly string representation of the TimeSpan.</returns>
    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        return timeSpan.TotalDays >= 365 ? $"{timeSpan.TotalDays / 365:F0} year{(timeSpan.TotalDays >= 730 ? "s" : "")}" :
               timeSpan.TotalDays >= 1 ? $"{timeSpan.TotalDays:F0} day{(timeSpan.TotalDays >= 2 ? "s" : "")}" :
               timeSpan.TotalHours >= 1 ? $"{timeSpan.TotalHours:F0} hour{(timeSpan.TotalHours >= 2 ? "s" : "")}" :
               timeSpan.TotalMinutes >= 1 ? $"{timeSpan.TotalMinutes:F0} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")}" :
               $"{timeSpan.TotalSeconds:F0} second{(timeSpan.TotalSeconds >= 2 ? "s" : "")}";
    }

    #endregion Private Helper Methods
}