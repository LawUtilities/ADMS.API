public static class UserValidationHelper
{
    public const int MaxUserNameLength = 50;
    public const int MinUserNameLength = 2;
    
    public static IEnumerable<ValidationResult> ValidateUserId(Guid id, string propertyName)
    {
        if (id == Guid.Empty)
            yield return new ValidationResult("User ID cannot be empty.", new[] { propertyName });
    }
    
    public static IEnumerable<ValidationResult> ValidateName(string name, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(name))
            yield return new ValidationResult("User name is required.", new[] { propertyName });
        
        if (name.Length < MinUserNameLength || name.Length > MaxUserNameLength)
            yield return new ValidationResult($"User name must be between {MinUserNameLength} and {MaxUserNameLength} characters.", new[] { propertyName });
    }
    
    public static string? NormalizeName(string name) => string.IsNullOrWhiteSpace(name) ? null : name.Trim();
    public static bool IsValidUserId(Guid id) => id != Guid.Empty;
    public static bool IsNameAllowed(string name) => !string.IsNullOrWhiteSpace(name) && name.Length >= MinUserNameLength;
}