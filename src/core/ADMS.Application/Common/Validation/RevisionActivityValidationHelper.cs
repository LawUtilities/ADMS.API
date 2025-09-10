using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace ADMS.Application.Common.Validation;

/// <summary>
/// Validation helper for revision activities.
/// </summary>
public static class RevisionActivityValidationHelper
{
    /// <summary>
    /// Valid revision activity types.
    /// </summary>
    public static readonly string[] AllowedActivities = 
    [
        "CREATED", "SAVED", "DELETED", "RESTORED"
    ];

    /// <summary>
    /// High-performance frozen set for activity lookups.
    /// </summary>
    private static readonly FrozenSet<string> _allowedActivitiesSet =
        AllowedActivities.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets comma-separated list of allowed activities.
    /// </summary>
    public static string AllowedActivitiesList => string.Join(", ", AllowedActivities);

    /// <summary>
    /// Validates a revision activity type.
    /// </summary>
    public static IEnumerable<ValidationResult> ValidateActivity(string? activity, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(activity))
        {
            yield return new ValidationResult($"{propertyName} is required.", [propertyName]);
            yield break;
        }

        if (!_allowedActivitiesSet.Contains(activity.Trim()))
        {
            yield return new ValidationResult(
                $"{propertyName} must be one of: {AllowedActivitiesList}.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Quick check if activity is allowed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsActivityAllowed(string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) && 
               _allowedActivitiesSet.Contains(activity.Trim());
    }
}