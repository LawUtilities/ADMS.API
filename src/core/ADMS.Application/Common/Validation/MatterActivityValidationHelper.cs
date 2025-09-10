using ADMS.Application.Constants;

using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ADMS.Application.Common.Validation;

/// <summary>
/// Provides comprehensive validation helper methods for matter activity-related data within the ADMS document management solution.
/// </summary>
/// <remarks>
/// This validation helper follows Clean Architecture principles and provides optimized validation for MatterActivity entities and DTOs:
/// <list type="bullet">
/// <item><strong>Activity Name Validation:</strong> Ensures professional activity naming conventions and allowed values</item>
/// <item><strong>Activity ID Validation:</strong> Validates activity identifiers for proper GUID structure</item>
/// <item><strong>User Association Validation:</strong> Validates activity-user relationships for audit trail integrity</item>
/// <item><strong>Business Rule Validation:</strong> Implements legal practice-specific activity validation rules</item>
/// </list>
/// 
/// <para><strong>Matter Activity Context in ADMS:</strong></para>
/// Matter activities in this system represent standardized operations on legal matters, supporting:
/// <list type="bullet">
/// <item>Matter lifecycle management (CREATED, ARCHIVED, DELETED, etc.)</item>
/// <item>Professional accountability through user attribution</item>
/// <item>Audit trail compliance for legal requirements</item>
/// <item>Activity-based reporting and analytics</item>
/// </list>
/// </remarks>
public static partial class MatterActivityValidationHelper
{
    #region Core Constants

    /// <summary>
    /// Maximum allowed length for a matter activity name (matches domain constraints).
    /// </summary>
    public const int MaxActivityLength = MatterConstants.ActivityMaxLength;

    /// <summary>
    /// Minimum allowed length for a matter activity name.
    /// </summary>
    public const int MinActivityLength = MatterConstants.ActivityMinLength;

    /// <summary>
    /// Earliest allowed date for matter activity operations (prevents unrealistic dates).
    /// </summary>
    public static readonly DateTime MinAllowedActivityDate = new(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Tolerance in minutes for future dates (accounts for clock skew).
    /// </summary>
    public const int FutureDateToleranceMinutes = 5;

    /// <summary>
    /// Maximum reasonable activity count per user per day for monitoring.
    /// </summary>
    public const int MaxDailyActivityCount = 200;

    #endregion Core Constants

    #region Allowed Activities (High Performance)

    /// <summary>
    /// Standard matter activities that are allowed in the system (optimized with FrozenSet for O(1) lookup).
    /// </summary>
    private static readonly string[] _allowedActivitiesArray =
    [
        "CREATED",      // Matter creation activity
        "ARCHIVED",     // Matter archival activity
        "DELETED",      // Matter deletion activity
        "RESTORED",     // Matter restoration activity
        "UNARCHIVED",   // Matter unarchival activity
        "VIEWED"        // Matter viewing activity
    ];

    /// <summary>
    /// High-performance frozen set for allowed activity lookups.
    /// </summary>
    private static readonly FrozenSet<string> _allowedActivitiesSet =
        _allowedActivitiesArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the immutable list of allowed activities.
    /// </summary>
    public static IReadOnlyList<string> AllowedActivities => _allowedActivitiesArray;

    /// <summary>
    /// Gets the allowed activities as a comma-separated string for error messages.
    /// </summary>
    public static string AllowedActivitiesList => string.Join(", ", _allowedActivitiesArray);

    #endregion Allowed Activities

    #region Reserved Words (High Performance)

    /// <summary>
    /// Reserved activity names that cannot be used (optimized with FrozenSet for O(1) lookup).
    /// </summary>
    private static readonly string[] _reservedActivitiesArray =
    [
        // System terms
        "SYSTEM", "ADMIN", "ROOT", "SETUP", "CONFIG",
        
        // Database terms
        "INSERT", "UPDATE", "DELETE", "SELECT", "DROP",
        
        // General reserved
        "NULL", "VOID", "EMPTY", "NONE", "UNDEFINED",
        
        // Security terms
        "AUTH", "LOGIN", "LOGOUT", "SECURITY", "TOKEN",
        
        // ADMS specific reserved
        "ADMS", "MATTER", "DOCUMENT", "USER", "ACTIVITY"
    ];

    /// <summary>
    /// High-performance frozen set for reserved activity lookups.
    /// </summary>
    private static readonly FrozenSet<string> _reservedActivitiesSet =
        _reservedActivitiesArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the immutable list of reserved activities.
    /// </summary>
    public static IReadOnlyList<string> ReservedActivities => _reservedActivitiesArray;

    #endregion Reserved Words

    #region Activity Validation

    /// <summary>
    /// Validates a matter activity ID according to business rules.
    /// </summary>
    /// <param name="activityId">The activity ID to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateActivityId(Guid activityId, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (activityId == Guid.Empty)
        {
            yield return new ValidationResult(
                $"{propertyName} must be a valid non-empty GUID for activity identification.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates a matter activity name according to business rules.
    /// </summary>
    /// <param name="activity">The activity name to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates matter activity names for legal practice requirements including:
    /// <list type="bullet">
    /// <item>Required field validation</item>
    /// <item>Length constraints aligned with domain requirements</item>
    /// <item>Allowed activity validation</item>
    /// <item>Reserved word protection</item>
    /// <item>Professional naming conventions</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateActivity(string? activity, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Required validation
        if (string.IsNullOrWhiteSpace(activity))
        {
            yield return new ValidationResult(
                $"{propertyName} is required and cannot be empty.",
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

        // Reserved word validation
        if (_reservedActivitiesSet.Contains(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} is a reserved activity name and cannot be used.",
                [propertyName]);
        }

        // Allowed activity validation
        if (!_allowedActivitiesSet.Contains(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} '{trimmed}' is not allowed. Allowed activities: {AllowedActivitiesList}",
                [propertyName]);
        }

        // Format validation
        if (!IsValidActivityFormat(trimmed))
        {
            yield return new ValidationResult(
                GetActivityFormatErrorMessage(propertyName, trimmed),
                [propertyName]);
        }

        // Whitespace validation
        if (activity.Trim() != activity)
        {
            yield return new ValidationResult(
                $"{propertyName} should not have leading or trailing whitespace.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates user association requirements for activities.
    /// </summary>
    /// <param name="associationCount">The number of user associations.</param>
    /// <param name="allowEmptyUsers">Whether to allow empty user associations.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool HasRequiredUserAssociations(int associationCount, bool allowEmptyUsers = false)
    {
        return allowEmptyUsers || associationCount > 0;
    }

    /// <summary>
    /// Validates activity duplication patterns.
    /// </summary>
    /// <param name="activity">The current activity.</param>
    /// <param name="existingActivities">Existing activities for comparison.</param>
    /// <param name="allowDuplicates">Whether to allow duplicate activities.</param>
    /// <returns>True if duplication is valid; otherwise, false.</returns>
    public static bool IsValidActivityDuplication(string? activity, string[] existingActivities, bool allowDuplicates = true)
    {
        if (string.IsNullOrWhiteSpace(activity) || existingActivities.Length == 0)
            return true;

        if (!allowDuplicates)
        {
            return !existingActivities.Contains(activity, StringComparer.OrdinalIgnoreCase);
        }

        // For matter activities, duplicates are generally allowed (same user can view multiple times, etc.)
        return true;
    }

    #endregion Activity Validation

    #region Quick Validation Methods (Performance Optimized)

    /// <summary>
    /// Quick validation check for activity ID (optimized for performance).
    /// </summary>
    /// <param name="activityId">The activity ID to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidActivityId(Guid activityId) => activityId != Guid.Empty;

    /// <summary>
    /// Quick validation check for activity name (optimized for performance).
    /// </summary>
    /// <param name="activity">The activity name to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsActivityAllowed([NotNullWhen(true)] string? activity)
    {
        if (string.IsNullOrWhiteSpace(activity))
            return false;

        var trimmed = activity.Trim();

        return trimmed.Length >= MinActivityLength &&
               trimmed.Length <= MaxActivityLength &&
               !_reservedActivitiesSet.Contains(trimmed) &&
               _allowedActivitiesSet.Contains(trimmed) &&
               IsValidActivityFormat(trimmed) &&
               activity.Trim() == activity;
    }

    /// <summary>
    /// Checks if an activity name is reserved (optimized for performance).
    /// </summary>
    /// <param name="activity">The activity name to check.</param>
    /// <returns>True if reserved; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReservedActivity([NotNullWhen(true)] string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) && _reservedActivitiesSet.Contains(activity.Trim());
    }

    /// <summary>
    /// Validates matter status for activity appropriateness.
    /// </summary>
    /// <param name="activity">The activity to validate.</param>
    /// <param name="isArchived">Whether the matter is archived.</param>
    /// <param name="isDeleted">Whether the matter is deleted.</param>
    /// <returns>True if activity is appropriate for the matter status; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsActivityAppropriateForMatterStatus(string? activity, bool isArchived, bool isDeleted)
    {
        if (string.IsNullOrWhiteSpace(activity))
            return false;

        var normalized = activity.Trim().ToUpperInvariant();

        return normalized switch
        {
            "ARCHIVED" => !isArchived && !isDeleted,      // Cannot archive if already archived or deleted
            "UNARCHIVED" => isArchived && !isDeleted,     // Can only unarchive if archived but not deleted
            "DELETED" => !isDeleted,                      // Cannot delete if already deleted
            "RESTORED" => isDeleted,                      // Can only restore if deleted
            "CREATED" => false,                           // Cannot create existing matter
            "VIEWED" => !isDeleted,                       // Cannot view deleted matters
            _ => true                                     // Other activities generally allowed
        };
    }

    #endregion Quick Validation Methods

    #region Normalization Methods

    /// <summary>
    /// Normalizes an activity name for consistent storage and comparison.
    /// </summary>
    /// <param name="activity">The activity name to normalize.</param>
    /// <returns>Normalized activity name or null if invalid.</returns>
    /// <remarks>
    /// Normalization includes:
    /// <list type="bullet">
    /// <item>Trimming whitespace</item>
    /// <item>Converting to uppercase for consistency</item>
    /// <item>Removing non-alphanumeric characters except underscores</item>
    /// </list>
    /// </remarks>
    [return: NotNullIfNotNull(nameof(activity))]
    public static string? NormalizeActivity(string? activity)
    {
        if (string.IsNullOrWhiteSpace(activity))
            return null;

        var trimmed = activity.Trim();
        if (trimmed.Length == 0) return string.Empty;

        // Convert to uppercase and remove invalid characters
        var normalized = ActivityNormalizationRegex().Replace(trimmed.ToUpperInvariant(), "_");
        
        // Remove multiple consecutive underscores
        return MultipleUnderscoreRegex().Replace(normalized, "_").Trim('_');
    }

    #endregion Normalization Methods

    #region Business Rule Validation

    /// <summary>
    /// Validates activity business rules for professional legal practice.
    /// </summary>
    /// <param name="activity">The activity name.</param>
    /// <param name="userId">The user performing the activity.</param>
    /// <param name="matterId">The matter being acted upon.</param>
    /// <param name="timestamp">The activity timestamp.</param>
    /// <param name="propertyPrefix">The property prefix for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyPrefix is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateActivityBusinessRules(
        string? activity,
        Guid userId,
        Guid matterId,
        DateTime timestamp,
        [NotNull] string propertyPrefix = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyPrefix);

        // Core validation
        foreach (var result in ValidateActivity(activity, $"{propertyPrefix}Activity"))
            yield return result;

        foreach (var result in UserValidationHelper.ValidateUserId(userId, $"{propertyPrefix}UserId"))
            yield return result;

        if (matterId == Guid.Empty)
        {
            yield return new ValidationResult(
                "Matter ID is required for activity context.",
                [$"{propertyPrefix}MatterId"]);
        }

        // Timestamp validation
        foreach (var result in UserValidationHelper.ValidateActivityTimestamp(timestamp, $"{propertyPrefix}Timestamp"))
            yield return result;

        // Activity-specific business rules
        if (string.IsNullOrWhiteSpace(activity)) yield break;
        var normalized = activity.Trim().ToUpperInvariant();
            
        // Validate activity timing patterns
        if (IsHighFrequencyActivity(normalized) && IsWithinBurstWindow(timestamp))
        {
            yield return new ValidationResult(
                $"Activity '{normalized}' appears to be occurring too frequently. Please verify user behavior.",
                [$"{propertyPrefix}Activity", $"{propertyPrefix}Timestamp"]);
        }
    }

    /// <summary>
    /// Validates activity context for professional practice requirements.
    /// </summary>
    /// <param name="activity">The activity name.</param>
    /// <param name="matterIsArchived">Whether the matter is archived.</param>
    /// <param name="matterIsDeleted">Whether the matter is deleted.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateActivityContext(
        string? activity,
        bool matterIsArchived,
        bool matterIsDeleted,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (string.IsNullOrWhiteSpace(activity))
            yield break;

        if (IsActivityAppropriateForMatterStatus(activity, matterIsArchived, matterIsDeleted)) yield break;
        var matterStatus = (matterIsDeleted, matterIsArchived) switch
        {
            (true, true) => "archived and deleted",
            (true, false) => "deleted",
            (false, true) => "archived",
            (false, false) => "active"
        };

        yield return new ValidationResult(
            $"Activity '{activity}' is not appropriate for a {matterStatus} matter.",
            [propertyName]);
    }

    #endregion Business Rule Validation

    #region Utility Methods

    /// <summary>
    /// Determines if two activities are equivalent after normalization.
    /// </summary>
    /// <param name="activity1">First activity to compare.</param>
    /// <param name="activity2">Second activity to compare.</param>
    /// <returns>True if equivalent; otherwise, false.</returns>
    public static bool AreActivitiesEquivalent(string? activity1, string? activity2)
    {
        var normalized1 = NormalizeActivity(activity1);
        var normalized2 = NormalizeActivity(activity2);

        return !string.IsNullOrEmpty(normalized1) &&
               !string.IsNullOrEmpty(normalized2) &&
               string.Equals(normalized1, normalized2, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets validation statistics for diagnostics and monitoring.
    /// </summary>
    /// <returns>Dictionary containing validation statistics.</returns>
    public static IReadOnlyDictionary<string, object> GetValidationStatistics()
    {
        return new Dictionary<string, object>
        {
            ["MinActivityLength"] = MinActivityLength,
            ["MaxActivityLength"] = MaxActivityLength,
            ["AllowedActivitiesCount"] = AllowedActivities.Count,
            ["ReservedActivitiesCount"] = ReservedActivities.Count,
            ["MinAllowedDate"] = MinAllowedActivityDate,
            ["FutureDateToleranceMinutes"] = FutureDateToleranceMinutes,
            ["MaxDailyActivityCount"] = MaxDailyActivityCount,
            ["ValidationRules"] = new Dictionary<string, string>
            {
                ["ActivityLength"] = $"Between {MinActivityLength} and {MaxActivityLength} characters",
                ["AllowedValues"] = AllowedActivitiesList,
                ["ReservedRule"] = "Cannot use reserved activity names",
                ["DateRange"] = $"Between {MinAllowedActivityDate:yyyy-MM-dd} and present",
                ["FormatRule"] = "Must contain only letters, numbers, and underscores"
            }
        };
    }

    #endregion Utility Methods

    #region Private Helper Methods

    /// <summary>
    /// Validates activity format using comprehensive rules.
    /// </summary>
    /// <param name="activity">The activity to validate (must be trimmed).</param>
    /// <returns>True if format is valid; otherwise, false.</returns>
    private static bool IsValidActivityFormat(string activity)
    {
        if (string.IsNullOrEmpty(activity))
            return false;

        // Check character set (letters, numbers, underscores only)
        if (!ActivityCharacterRegex().IsMatch(activity))
            return false;

        // Must start and end with alphanumeric
        if (!char.IsLetterOrDigit(activity[0]) || !char.IsLetterOrDigit(activity[^1]))
            return false;

        // Cannot have consecutive underscores
        return !activity.Contains("__");
    }

    /// <summary>
    /// Gets a specific format error message based on the validation failure.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="value">The invalid value.</param>
    /// <returns>Specific error message.</returns>
    private static string GetActivityFormatErrorMessage(string propertyName, string value)
    {
        if (value.StartsWith('_') || value.EndsWith('_'))
            return $"{propertyName} cannot start or end with an underscore.";

        if (value.Contains("__"))
            return $"{propertyName} cannot contain consecutive underscores.";

        return !ActivityCharacterRegex().IsMatch(value) ? $"{propertyName} can only contain letters, numbers, and underscores." : $"{propertyName} contains invalid format.";
    }

    /// <summary>
    /// Determines if an activity is considered high-frequency.
    /// </summary>
    /// <param name="normalizedActivity">The normalized activity name.</param>
    /// <returns>True if high-frequency; otherwise, false.</returns>
    private static bool IsHighFrequencyActivity(string normalizedActivity)
    {
        return normalizedActivity is "VIEWED" or "CREATED";
    }

    /// <summary>
    /// Determines if a timestamp is within the burst detection window.
    /// </summary>
    /// <param name="timestamp">The timestamp to check.</param>
    /// <returns>True if within burst window; otherwise, false.</returns>
    private static bool IsWithinBurstWindow(DateTime timestamp)
    {
        var utcTimestamp = timestamp.Kind != DateTimeKind.Utc ? timestamp.ToUniversalTime() : timestamp;
        var windowStart = DateTime.UtcNow.AddMinutes(-5); // 5-minute window
        return utcTimestamp >= windowStart;
    }

    #endregion Private Helper Methods

    #region Compiled Regex Patterns

    /// <summary>
    /// Compiled regex for validating activity characters.
    /// </summary>
    [GeneratedRegex(@"^[A-Z0-9_]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex ActivityCharacterRegex();

    /// <summary>
    /// Compiled regex for activity normalization.
    /// </summary>
    [GeneratedRegex(@"[^A-Z0-9_]", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex ActivityNormalizationRegex();

    /// <summary>
    /// Compiled regex for removing multiple consecutive underscores.
    /// </summary>
    [GeneratedRegex(@"_+", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MultipleUnderscoreRegex();

    #endregion Compiled Regex Patterns
}