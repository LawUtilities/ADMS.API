using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ADMS.Application.Common.Validation;

/// <summary>
/// Provides comprehensive validation helper methods for document activity classification within the ADMS document management solution.
/// </summary>
/// <remarks>
/// This validation helper follows Clean Architecture principles and provides optimized validation for DocumentActivity entities and DTOs:
/// <list type="bullet">
/// <item><strong>Activity Classification:</strong> Validates document activity types and their professional contexts</item>
/// <item><strong>Version Control Validation:</strong> Ensures proper check-in/check-out workflow validation</item>
/// <item><strong>Lifecycle Validation:</strong> Validates document lifecycle state transitions</item>
/// <item><strong>Audit Trail Validation:</strong> Ensures proper activity tracking for legal compliance</item>
/// </list>
/// 
/// <para><strong>Document Activity Context in ADMS:</strong></para>
/// Document activities in this system represent user operations on documents, supporting:
/// <list type="bullet">
/// <item>Document creation and modification tracking</item>
/// <item>Version control operations (check-in/check-out)</item>
/// <item>Document lifecycle management (deleted/restored)</item>
/// <item>Professional audit trail requirements</item>
/// </list>
/// </remarks>
public static class DocumentActivityValidationHelper
{
    #region Core Constants

    /// <summary>
    /// Maximum allowed length for activity classification strings.
    /// </summary>
    public const int MaxActivityLength = 50;

    /// <summary>
    /// Minimum allowed length for activity classification strings.
    /// </summary>
    public const int MinActivityLength = 3;

    #endregion Core Constants

    #region Allowed Document Activities

    /// <summary>
    /// Standard document activities supported by the ADMS system (optimized with FrozenSet).
    /// </summary>
    /// <remarks>
    /// These activities correspond to the seeded data in ADMS.API.Entities.DocumentActivity:
    /// <list type="bullet">
    /// <item><strong>CHECKED IN:</strong> Document checked into version control (ID: 20000000-0000-0000-0000-000000000001)</item>
    /// <item><strong>CHECKED OUT:</strong> Document checked out for editing (ID: 20000000-0000-0000-0000-000000000002)</item>
    /// <item><strong>CREATED:</strong> Initial document creation (ID: 20000000-0000-0000-0000-000000000003)</item>
    /// <item><strong>DELETED:</strong> Document marked for deletion (ID: 20000000-0000-0000-0000-000000000004)</item>
    /// <item><strong>RESTORED:</strong> Deleted document restored to active status (ID: 20000000-0000-0000-0000-000000000005)</item>
    /// <item><strong>SAVED:</strong> Document saved with changes (ID: 20000000-0000-0000-0000-000000000006)</item>
    /// </list>
    /// </remarks>
    private static readonly string[] _allowedActivitiesArray =
    [
        "CHECKED IN",
        "CHECKED OUT", 
        "CREATED",
        "DELETED",
        "RESTORED",
        "SAVED"
    ];

    /// <summary>
    /// High-performance frozen set for allowed activity lookups.
    /// </summary>
    private static readonly FrozenSet<string> _allowedActivitiesSet =
        _allowedActivitiesArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the immutable list of allowed document activities.
    /// </summary>
    public static IReadOnlyList<string> AllowedActivities => _allowedActivitiesArray;

    /// <summary>
    /// Document activities that require version control context.
    /// </summary>
    private static readonly FrozenSet<string> _versionControlActivities =
        new[] { "CHECKED IN", "CHECKED OUT" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Document activities that represent lifecycle changes.
    /// </summary>
    private static readonly FrozenSet<string> _lifecycleActivities =
        new[] { "CREATED", "DELETED", "RESTORED" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Document activities that represent content modifications.
    /// </summary>
    private static readonly FrozenSet<string> _modificationActivities =
        new[] { "SAVED", "CREATED" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    #endregion Allowed Document Activities

    #region Activity Validation

    /// <summary>
    /// Validates a document activity classification according to business rules.
    /// </summary>
    /// <param name="activity">The activity classification to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates document activities for professional legal document management including:
    /// <list type="bullet">
    /// <item>Required field validation</item>
    /// <item>Length constraints</item>
    /// <item>Allowed activity list validation</item>
    /// <item>Professional naming conventions</item>
    /// <item>Activity format consistency</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateActivity(string? activity, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Required validation
        if (string.IsNullOrWhiteSpace(activity))
        {
            yield return new ValidationResult(
                $"{propertyName} is required for activity classification.",
                [propertyName]);
            yield break;
        }

        var trimmed = activity.Trim();

        switch (trimmed.Length)
        {
            // Length validation
            case < MinActivityLength:
                yield return new ValidationResult(
                    $"{propertyName} must be at least {MinActivityLength} characters long.",
                    [propertyName]);
                break;
            case > MaxActivityLength:
                yield return new ValidationResult(
                    $"{propertyName} cannot exceed {MaxActivityLength} characters.",
                    [propertyName]);
                break;
        }

        // Allowed activity validation
        if (!_allowedActivitiesSet.Contains(trimmed))
        {
            var allowedList = string.Join(", ", _allowedActivitiesArray);
            yield return new ValidationResult(
                $"{propertyName} '{trimmed}' is not a recognized document activity. Allowed activities: {allowedList}",
                [propertyName]);
        }

        // Format validation
        foreach (var result in ValidateActivityFormat(trimmed, propertyName))
            yield return result;
    }

    /// <summary>
    /// Validates document activity context for professional legal practice.
    /// </summary>
    /// <param name="activity">The document activity.</param>
    /// <param name="userId">The user performing the activity.</param>
    /// <param name="documentId">The document being acted upon.</param>
    /// <param name="activityTimestamp">The activity timestamp.</param>
    /// <param name="propertyPrefix">The property prefix for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyPrefix is null or whitespace.</exception>
    /// <remarks>
    /// Validates the complete context of a document activity to ensure professional
    /// accountability and audit trail integrity.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateActivityContext(
        string? activity,
        Guid userId,
        Guid documentId,
        DateTime activityTimestamp,
        [NotNull] string propertyPrefix = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyPrefix);

        // Core activity validation
        foreach (var result in ValidateActivity(activity, $"{propertyPrefix}Activity"))
            yield return result;

        // Context validation
        if (userId == Guid.Empty)
        {
            yield return new ValidationResult(
                "Document activity must be attributed to a valid user for accountability.",
                [$"{propertyPrefix}UserId"]);
        }

        if (documentId == Guid.Empty)
        {
            yield return new ValidationResult(
                "Document activity must be associated with a valid document.",
                [$"{propertyPrefix}DocumentId"]);
        }

        // Timestamp validation using UserValidationHelper patterns
        foreach (var result in UserValidationHelper.ValidateActivityTimestamp(activityTimestamp, $"{propertyPrefix}Timestamp"))
            yield return result;
    }

    /// <summary>
    /// Validates document activity sequence consistency for workflow integrity.
    /// </summary>
    /// <param name="currentActivity">The current activity being validated.</param>
    /// <param name="previousActivity">The previous activity in the sequence (optional).</param>
    /// <param name="documentIsCheckedOut">Whether the document is currently checked out.</param>
    /// <param name="documentIsDeleted">Whether the document is currently deleted.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates activity sequences to ensure logical workflow consistency:
    /// <list type="bullet">
    /// <item>Check-out must precede check-in operations</item>
    /// <item>Documents must be checked out before modification</item>
    /// <item>Deleted documents cannot be modified</item>
    /// <item>Proper lifecycle state transitions</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateActivitySequence(
        string? currentActivity,
        string? previousActivity,
        bool documentIsCheckedOut,
        bool documentIsDeleted,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (string.IsNullOrWhiteSpace(currentActivity))
            yield break;

        var currentNormalized = currentActivity.Trim().ToUpperInvariant();

        // Version control sequence validation
        switch (currentNormalized)
        {
            case "CHECKED IN":
                if (!documentIsCheckedOut)
                {
                    yield return new ValidationResult(
                        $"{propertyName}: Cannot check in document that is not checked out.",
                        [propertyName]);
                }
                break;

            case "CHECKED OUT":
                if (documentIsCheckedOut)
                {
                    yield return new ValidationResult(
                        $"{propertyName}: Cannot check out document that is already checked out.",
                        [propertyName]);
                }
                if (documentIsDeleted)
                {
                    yield return new ValidationResult(
                        $"{propertyName}: Cannot check out deleted document.",
                        [propertyName]);
                }
                break;

            case "SAVED":
                if (!documentIsCheckedOut)
                {
                    yield return new ValidationResult(
                        $"{propertyName}: Document must be checked out before saving changes.",
                        [propertyName]);
                }
                if (documentIsDeleted)
                {
                    yield return new ValidationResult(
                        $"{propertyName}: Cannot save deleted document.",
                        [propertyName]);
                }
                break;

            case "DELETED":
                if (documentIsDeleted)
                {
                    yield return new ValidationResult(
                        $"{propertyName}: Document is already deleted.",
                        [propertyName]);
                }
                break;

            case "RESTORED":
                if (!documentIsDeleted)
                {
                    yield return new ValidationResult(
                        $"{propertyName}: Cannot restore document that is not deleted.",
                        [propertyName]);
                }
                break;
        }

        // Previous activity context validation
        if (string.IsNullOrWhiteSpace(previousActivity)) yield break;
        var previousNormalized = previousActivity.Trim().ToUpperInvariant();

        switch (currentNormalized)
        {
            // Validate logical sequences
            case "CHECKED IN" when previousNormalized != "CHECKED OUT" && previousNormalized != "SAVED":
                yield return new ValidationResult(
                    $"{propertyName}: Check-in should typically follow check-out or save operations.",
                    [propertyName]);
                break;
            case "RESTORED" when previousNormalized != "DELETED":
                yield return new ValidationResult(
                    $"{propertyName}: Restore operation should follow deletion.",
                    [propertyName]);
                break;
        }
    }

    #endregion Activity Validation

    #region Quick Validation Methods (Performance Optimized)

    /// <summary>
    /// Quick validation check for document activity (optimized for performance).
    /// </summary>
    /// <param name="activity">The activity to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsActivityAllowed([NotNullWhen(true)] string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) &&
               activity.Trim().Length >= MinActivityLength &&
               activity.Trim().Length <= MaxActivityLength &&
               _allowedActivitiesSet.Contains(activity.Trim());
    }

    /// <summary>
    /// Determines if an activity is a version control operation.
    /// </summary>
    /// <param name="activity">The activity to check.</param>
    /// <returns>True if version control activity; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsVersionControlActivity([NotNullWhen(true)] string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) &&
               _versionControlActivities.Contains(activity.Trim());
    }

    /// <summary>
    /// Determines if an activity is a lifecycle operation.
    /// </summary>
    /// <param name="activity">The activity to check.</param>
    /// <returns>True if lifecycle activity; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLifecycleActivity([NotNullWhen(true)] string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) &&
               _lifecycleActivities.Contains(activity.Trim());
    }

    /// <summary>
    /// Determines if an activity represents content modification.
    /// </summary>
    /// <param name="activity">The activity to check.</param>
    /// <returns>True if modification activity; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsModificationActivity([NotNullWhen(true)] string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) &&
               _modificationActivities.Contains(activity.Trim());
    }

    /// <summary>
    /// Determines if an activity requires the document to be checked out.
    /// </summary>
    /// <param name="activity">The activity to check.</param>
    /// <returns>True if requires check-out; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool RequiresCheckOut([NotNullWhen(true)] string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) &&
               (activity.Trim().Equals("SAVED", StringComparison.OrdinalIgnoreCase) ||
                activity.Trim().Equals("CHECKED IN", StringComparison.OrdinalIgnoreCase));
    }

    #endregion Quick Validation Methods

    #region Normalization and Utility Methods

    /// <summary>
    /// Normalizes a document activity classification for consistent storage.
    /// </summary>
    /// <param name="activity">The activity to normalize.</param>
    /// <returns>Normalized activity or null if invalid.</returns>
    /// <remarks>
    /// Normalization includes:
    /// <list type="bullet">
    /// <item>Trimming whitespace</item>
    /// <item>Converting to standard case (uppercase)</item>
    /// <item>Validating against allowed activities</item>
    /// </list>
    /// </remarks>
    [return: NotNullIfNotNull(nameof(activity))]
    public static string? NormalizeActivity(string? activity)
    {
        if (string.IsNullOrWhiteSpace(activity))
            return null;

        var trimmed = activity.Trim();
        if (trimmed.Length == 0) return string.Empty;

        // Normalize to uppercase for consistency
        var normalized = trimmed.ToUpperInvariant();

        // Return only if it's an allowed activity
        return _allowedActivitiesSet.Contains(normalized) ? normalized : string.Empty;
    }

    /// <summary>
    /// Gets the activity category for classification and reporting.
    /// </summary>
    /// <param name="activity">The document activity.</param>
    /// <returns>Activity category string.</returns>
    /// <remarks>
    /// Activity categories help organize activities into logical groups for reporting
    /// and business rule processing.
    /// </remarks>
    public static string GetActivityCategory(string? activity)
    {
        if (string.IsNullOrWhiteSpace(activity))
            return "Unknown";

        var normalized = activity.Trim().ToUpperInvariant();

        return normalized switch
        {
            "CREATED" => "Creation",
            "SAVED" => "Modification", 
            "CHECKED OUT" or "CHECKED IN" => "Version Control",
            "DELETED" or "RESTORED" => "Lifecycle",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Determines the professional impact level of a document activity.
    /// </summary>
    /// <param name="activity">The document activity.</param>
    /// <returns>Professional impact level (Low, Medium, High).</returns>
    /// <remarks>
    /// Impact levels help prioritize activities for audit trail analysis and
    /// professional accountability tracking.
    /// </remarks>
    public static string GetProfessionalImpactLevel(string? activity)
    {
        if (string.IsNullOrWhiteSpace(activity))
            return "Unknown";

        var normalized = activity.Trim().ToUpperInvariant();

        return normalized switch
        {
            "CREATED" or "DELETED" or "RESTORED" => "High",
            "SAVED" or "CHECKED OUT" => "Medium",
            "CHECKED IN" => "Low",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets validation statistics for diagnostics and monitoring.
    /// </summary>
    /// <returns>Dictionary containing validation statistics.</returns>
    public static IReadOnlyDictionary<string, object> GetValidationStatistics()
    {
        return new Dictionary<string, object>
        {
            ["MaxActivityLength"] = MaxActivityLength,
            ["MinActivityLength"] = MinActivityLength,
            ["AllowedActivitiesCount"] = AllowedActivities.Count,
            ["VersionControlActivitiesCount"] = _versionControlActivities.Count,
            ["LifecycleActivitiesCount"] = _lifecycleActivities.Count,
            ["ModificationActivitiesCount"] = _modificationActivities.Count,
            ["AllowedActivities"] = AllowedActivities.ToArray(),
            ["ValidationRules"] = new Dictionary<string, string>
            {
                ["ActivityLength"] = $"Between {MinActivityLength} and {MaxActivityLength} characters",
                ["AllowedValues"] = string.Join(", ", AllowedActivities),
                ["VersionControl"] = "CHECKED IN, CHECKED OUT",
                ["Lifecycle"] = "CREATED, DELETED, RESTORED",
                ["Modification"] = "SAVED, CREATED",
                ["SequenceRule"] = "Check-out before modification, check-in after changes"
            }
        };
    }

    #endregion Normalization and Utility Methods

    #region Private Helper Methods

    /// <summary>
    /// Validates document activity format for consistency and professionalism.
    /// </summary>
    /// <param name="activity">The activity to validate (must be trimmed).</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results for format validation.</returns>
    private static IEnumerable<ValidationResult> ValidateActivityFormat(string activity, string propertyName)
    {
        // Check for consistent uppercase format
        if (!string.Equals(activity, activity.ToUpperInvariant(), StringComparison.Ordinal))
        {
            yield return new ValidationResult(
                $"{propertyName} should be in uppercase for consistency (e.g., '{activity.ToUpperInvariant()}').",
                [propertyName]);
        }

        // Check for invalid characters
        if (!activity.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
        {
            yield return new ValidationResult(
                $"{propertyName} can only contain letters and spaces for professional activity classification.",
                [propertyName]);
        }

        // Check for excessive whitespace
        if (activity.Contains("  "))
        {
            yield return new ValidationResult(
                $"{propertyName} cannot contain multiple consecutive spaces.",
                [propertyName]);
        }

        // Check for leading/trailing spaces (should be trimmed before this call)
        if (activity.StartsWith(' ') || activity.EndsWith(' '))
        {
            yield return new ValidationResult(
                $"{propertyName} should not have leading or trailing spaces.",
                [propertyName]);
        }
    }
    #endregion Private Helper Methods
}