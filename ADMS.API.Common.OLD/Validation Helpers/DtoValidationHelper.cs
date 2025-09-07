using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ADMS.API.Common;

/// <summary>
/// Provides comprehensive helper methods for validating collections of DTOs that implement <see cref="IValidatableObject"/>.
/// </summary>
/// <remarks>
/// This static helper class provides robust validation functionality for the ADMS legal document management system,
/// supporting comprehensive collection validation with detailed error reporting and performance optimization.
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item>Null-safe collection validation with early termination for invalid states</item>
/// <item>Index-based error reporting for precise error location identification</item>
/// <item>Nested property path construction for hierarchical validation results</item>
/// <item>Generic type support for compile-time type safety and reusability</item>
/// <item>Performance-optimized validation with minimal allocations</item>
/// <item>Integration with standard .NET validation infrastructure</item>
/// </list>
/// 
/// <para><strong>Validation Process:</strong></para>
/// <list type="number">
/// <item>Validates collection existence (null checking)</item>
/// <item>Validates individual item existence within the collection</item>
/// <item>Performs deep validation on items implementing IValidatableObject</item>
/// <item>Constructs detailed error messages with property path context</item>
/// <item>Returns comprehensive validation results for all discovered issues</item>
/// </list>
/// 
/// The helper follows established ADMS validation patterns and integrates seamlessly with
/// the existing validation infrastructure, including centralized validation services
/// and standardized error reporting mechanisms.
/// 
/// <para><strong>Thread Safety:</strong></para>
/// All methods in this class are thread-safe and can be safely used in concurrent scenarios
/// without external synchronization. The class uses only immutable operations and local variables.
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// The validation methods use yield return for lazy evaluation, allowing callers to stop
/// enumeration early if needed and reducing memory allocation for large collections.
/// </remarks>
public static class DtoValidationHelper
{
    /// <summary>
    /// Validates a collection of DTOs, ensuring the collection exists and that each item is valid.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection. Must be a reference type.</typeparam>
    /// <param name="collection">The collection to validate. Can be null.</param>
    /// <param name="propertyName">The name of the property representing the collection (used in error messages). Cannot be null or whitespace.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing validation errors for the collection or its items.
    /// Returns an empty enumerable if all validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    /// <remarks>
    /// This method performs comprehensive validation including:
    /// <list type="bullet">
    /// <item>Collection null checking with immediate error return</item>
    /// <item>Individual item null checking with index-specific error messages</item>
    /// <item>Deep validation for items implementing <see cref="IValidatableObject"/></item>
    /// <item>Hierarchical error message construction with property path context</item>
    /// </list>
    /// 
    /// <para><strong>Validation Order:</strong></para>
    /// <list type="number">
    /// <item>Validates collection is not null</item>
    /// <item>Iterates through collection items with index tracking</item>
    /// <item>Validates each item is not null/default</item>
    /// <item>Performs IValidatableObject.Validate() if item supports it</item>
    /// <item>Constructs detailed error messages with property paths</item>
    /// </list>
    /// 
    /// <para><strong>Error Message Format:</strong></para>
    /// <list type="bullet">
    /// <item>Collection errors: "{propertyName} collection is required."</item>
    /// <item>Item null errors: "{propertyName}[{index}] is null."</item>
    /// <item>Validation errors: "{propertyName}[{index}]: {original error message}"</item>
    /// </list>
    /// 
    /// <para><strong>Performance Notes:</strong></para>
    /// Uses yield return for lazy evaluation, allowing early termination and memory efficiency.
    /// Validation context is created once per item for optimal performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Validate a collection of user DTOs
    /// var users = new List&lt;UserDto&gt; { new UserDto { Name = "John" }, null, new UserDto { Name = "" } };
    /// var results = DtoValidationHelper.ValidateCollection(users, nameof(MyDto.Users));
    /// 
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"Error: {result.ErrorMessage}");
    ///     Console.WriteLine($"Members: {string.Join(", ", result.MemberNames)}");
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<ValidationResult> ValidateCollection<T>(
        ICollection<T>? collection,
        [NotNull] string propertyName)
        where T : class
    {
        // Validate input parameters using .NET 8+ argument validation
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Early return for null collections
        if (collection is null)
        {
            yield return new ValidationResult(
                $"{propertyName} collection is required.",
                [propertyName]);
            yield break;
        }

        // Validate each item in the collection with index tracking
        var index = 0;
        foreach (var item in collection)
        {
            switch (item)
            {
                // Validate item is not null
                case null:
                    yield return new ValidationResult(
                        $"{propertyName}[{index}] is null.",
                        [$"{propertyName}[{index}]"]);
                    break;
                case IValidatableObject validatable:
                {
                    // Perform deep validation on validatable items
                    var validationContext = new ValidationContext(item);

                    foreach (var validationResult in validatable.Validate(validationContext))
                    {
                        // Construct hierarchical error messages with property paths
                        var memberNames = validationResult.MemberNames.Any()
                            ? validationResult.MemberNames.Select(memberName => $"{propertyName}[{index}].{memberName}")
                            : [$"{propertyName}[{index}]"];

                        yield return new ValidationResult(
                            $"{propertyName}[{index}]: {validationResult.ErrorMessage}",
                            memberNames);
                    }

                    break;
                }
            }

            index++;
        }
    }

    /// <summary>
    /// Validates a collection of DTOs with additional validation context and custom error handling.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection. Must be a reference type.</typeparam>
    /// <param name="collection">The collection to validate. Can be null.</param>
    /// <param name="propertyName">The name of the property representing the collection (used in error messages). Cannot be null or whitespace.</param>
    /// <param name="parentValidationContext">The parent validation context for nested validation scenarios. Cannot be null.</param>
    /// <param name="allowEmptyCollection">If true, allows empty collections; if false, requires at least one item.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing validation errors for the collection or its items.
    /// Returns an empty enumerable if all validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parentValidationContext"/> is null.</exception>
    /// <remarks>
    /// This overload provides additional validation capabilities including:
    /// <list type="bullet">
    /// <item>Parent validation context inheritance for consistent validation state</item>
    /// <item>Empty collection validation control</item>
    /// <item>Enhanced validation context propagation</item>
    /// <item>Consistent validation behavior across nested object hierarchies</item>
    /// </list>
    /// 
    /// <para><strong>Extended Validation Features:</strong></para>
    /// <list type="bullet">
    /// <item>Inherits validation context items and services from parent</item>
    /// <item>Optionally validates collection contains at least one item</item>
    /// <item>Maintains validation context consistency across object graphs</item>
    /// <item>Supports complex validation scenarios with shared state</item>
    /// </list>
    /// 
    /// This method is particularly useful when validating nested object hierarchies where
    /// validation context needs to be preserved across multiple levels of validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Validate nested collections with context inheritance
    /// public IEnumerable&lt;ValidationResult&gt; Validate(ValidationContext validationContext)
    /// {
    ///     // Validate required collection with at least one item
    ///     foreach (var result in DtoValidationHelper.ValidateCollection(
    ///         Documents, 
    ///         nameof(Documents), 
    ///         validationContext, 
    ///         allowEmptyCollection: false))
    ///     {
    ///         yield return result;
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<ValidationResult> ValidateCollection<T>(
        ICollection<T>? collection,
        [NotNull] string propertyName,
        [NotNull] ValidationContext parentValidationContext,
        bool allowEmptyCollection = true)
        where T : class
    {
        // Validate input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        ArgumentNullException.ThrowIfNull(parentValidationContext);

        // Early return for null collections
        if (collection is null)
        {
            yield return new ValidationResult(
                $"{propertyName} collection is required.",
                [propertyName]);
            yield break;
        }

        // Validate empty collection if not allowed
        if (!allowEmptyCollection && collection.Count == 0)
        {
            yield return new ValidationResult(
                $"{propertyName} collection must contain at least one item.",
                [propertyName]);
            yield break;
        }

        // Validate each item in the collection with inherited context
        var index = 0;
        foreach (var item in collection)
        {
            switch (item)
            {
                // Validate item is not null
                case null:
                    yield return new ValidationResult(
                        $"{propertyName}[{index}] is null.",
                        [$"{propertyName}[{index}]"]);
                    break;
                case IValidatableObject validatable:
                {
                    // Create validation context inheriting from parent
                    var validationContext = new ValidationContext(
                        item,
                        parentValidationContext,
                        parentValidationContext.Items);

                    foreach (var validationResult in validatable.Validate(validationContext))
                    {
                        // Construct hierarchical error messages with property paths
                        var memberNames = validationResult.MemberNames.Any()
                            ? validationResult.MemberNames.Select(memberName => $"{propertyName}[{index}].{memberName}")
                            : [$"{propertyName}[{index}]"];

                        yield return new ValidationResult(
                            $"{propertyName}[{index}]: {validationResult.ErrorMessage}",
                            memberNames);
                    }

                    break;
                }
            }

            index++;
        }
    }

    /// <summary>
    /// Validates that a collection property name is valid for error reporting.
    /// </summary>
    /// <param name="propertyName">The property name to validate.</param>
    /// <param name="parameterName">The parameter name for exception reporting.</param>
    /// <exception cref="ArgumentException">Thrown when the property name is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidatePropertyName([NotNull] string? propertyName, [CallerArgumentExpression(nameof(propertyName))] string? parameterName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, parameterName);
    }
}