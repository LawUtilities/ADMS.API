using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ADMS.Application.Common;

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
        for (int i = 0; i < items.Count; i++)
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
        if (obj is null)
        {
            yield return new ValidationResult("Object cannot be null.");
            yield break;
        }

        var type = typeof(T);
        var properties = type.GetProperties()
            .Where(p => p.GetCustomAttribute<RequiredAttribute>() != null);

        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            var requiredAttr = property.GetCustomAttribute<RequiredAttribute>()!;

            if (value is null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                var errorMessage = string.IsNullOrEmpty(requiredAttr.ErrorMessage)
                    ? $"{property.Name} is required."
                    : requiredAttr.ErrorMessage;

                yield return new ValidationResult(errorMessage, [property.Name]);
            }
        }
    }
}