using ADMS.Application.Constants;

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