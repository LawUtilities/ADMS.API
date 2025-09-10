using System.ComponentModel.DataAnnotations;

namespace ADMS.Application.Common.Validation;

public static class ValidationHelper
{
    public static IEnumerable<ValidationResult> ValidateCollection<T>(
        ICollection<T> collection, 
        string propertyName) where T : IValidatableObject
    {
        var index = 0;
        foreach (var item in collection)
        {
            if (item == null)
            {
                yield return new ValidationResult(
                    $"Item at index {index} in {propertyName} cannot be null.",
                    [$"{propertyName}[{index}]"]);
                continue;
            }

            var context = new ValidationContext(item);
            var results = new List<ValidationResult>();
            
            if (!Validator.TryValidateObject(item, context, results, true))
            {
                foreach (var result in results)
                {
                    yield return new ValidationResult(
                        result.ErrorMessage,
                        result.MemberNames.Select(m => $"{propertyName}[{index}].{m}"));
                }
            }
            
            index++;
        }
    }
}