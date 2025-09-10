using ADMS.Application.Constants;

using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ADMS.Application.Common.Validation;

/// <summary>
/// Provides comprehensive validation helper methods for revision activity-related data within the ADMS document management solution.
/// </summary>
/// <remarks>
/// This validation helper follows Clean Architecture principles and provides optimized validation for RevisionActivity entities and DTOs:
/// <list type="bullet">
/// <item><strong>Activity Name Validation:</strong> Ensures professional revision activity naming conventions</item>
/// <item><strong>Activity ID Validation:</strong> Validates revision activity identifiers for proper GUID structure</item>
/// <item><strong>Version Control Validation:</strong> Validates revision-specific operations and business rules</item>
/// <item><strong>Professional Standards:</strong> Implements document version control best practices</item>
/// </list>
/// 
/// <para><strong>Revision Activity Context in ADMS:</strong></para>
/// Revision activities in this system represent standardized operations on document revisions, supporting:
/// <list type="bullet">
/// <item>Document version control operations (CREATED, SAVED, DELETED, RESTORED)</item>
/// <item>Professional accountability through user attribution</item>
/// <item>Audit trail compliance for document versioning requirements</item>
/// <item>Version control reporting and analytics</item>
/// </list>
/// </remarks>
public static class RevisionActivityValidationHelper
{
    #region Core Constants

    /// <summary>
    /// Maximum allowed length for a revision activity name (matches domain constraints).
    /// </summary>
    public const int MaxActivityLength = MatterConstants.ActivityMaxLength;

    /// <summary>
    /// Minimum allowed length for a revision activity name.
    /// </summary>
    public const int MinActivityLength = MatterConstants.ActivityMinLength;

    /// <summary>
    /// Earliest allowed date for revision activity operations (prevents unrealistic dates).
    /// </summary>
    public static readonly DateTime MinAllowedRevisionDate = new(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Tolerance in minutes for future dates (accounts for clock skew).
    /// </summary>
    public const int FutureDateToleranceMinutes = 5;

    #endregion Core Constants

    #region Allowed Activities (High Performance)

    /// <summary>
    /// Standard revision activities that are allowed in the system (optimized with FrozenSet for O(1) lookup).
    /// </summary>
    private static readonly string[] _allowedActivitiesArray =
    [
        "CREATED",      // Revision creation activity
        "SAVED",        // Revision save/modification activity
        "DELETED",      // Revision deletion activity
        "RESTORED"      // Revision restoration activity
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

    #region Activity Validation

    /// <summary>
    /// Validates a revision activity ID according to business rules.
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
                $"{propertyName} must be a valid non-empty GUID for revision activity identification.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates a revision activity name according to business rules.
    /// </summary>
    /// <param name="activity">The activity name to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates revision activity names for document version control requirements including:
    /// <list type="bullet">
    /// <item>Required field validation</item>
    /// <item>Length constraints aligned with domain requirements</item>
    /// <item>Allowed activity validation for version control operations</item>
    /// <item>Professional naming conventions for document management</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateActivity(string? activity, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Required validation
        if (string.IsNullOrWhiteSpace(activity))
        {
            yield return new ValidationResult(
                $"{propertyName} is required for revision activity classification.",
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
            yield return new ValidationResult(
                $"{propertyName} '{trimmed}' is not allowed for revision operations. Allowed activities: {AllowedActivitiesList}",
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

    #endregion Activity Validation

    #region Quick Validation Methods (Performance Optimized)

    /// <summary>
    /// Quick validation check for revision activity ID (optimized for performance).
    /// </summary>
    /// <param name="activityId">The activity ID to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidActivityId(Guid activityId) => activityId != Guid.Empty;

    /// <summary>
    /// Quick validation check for revision activity name (optimized for performance).
    /// </summary>
    /// <param name="activity">The activity name to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsActivityAllowed([NotNullWhen(true)] string? activity)
    {
        if (string.IsNullOrWhiteSpace(activity))
            return false;

        var trimmed = activity.Trim();

        return trimmed.Length is >= MinActivityLength and <= MaxActivityLength &&
               _allowedActivitiesSet.Contains(trimmed) &&
               activity.Trim() == activity;
    }

    /// <summary>
    /// Validates revision status for activity appropriateness.
    /// </summary>
    /// <param name="activity">The activity to validate.</param>
    /// <param name="isDeleted">Whether the revision is deleted.</param>
    /// <returns>True if activity is appropriate for the revision status; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsActivityAppropriateForRevisionStatus(string? activity, bool isDeleted)
    {
        if (string.IsNullOrWhiteSpace(activity))
            return false;

        var normalized = activity.Trim().ToUpperInvariant();

        return normalized switch
        {
            "DELETED" => !isDeleted,          // Cannot delete if already deleted
            "RESTORED" => isDeleted,          // Can only restore if deleted
            "SAVED" => !isDeleted,            // Cannot save deleted revisions
            "CREATED" => false,               // Cannot create existing revision
            _ => true                         // Other activities generally allowed
        };
    }

    #endregion Quick Validation Methods

    #region Normalization Methods

    /// <summary>
    /// Normalizes a revision activity name for consistent storage and comparison.
    /// </summary>
    /// <param name="activity">The activity name to normalize.</param>
    /// <returns>Normalized activity name or null if invalid.</returns>
    /// <remarks>
    /// Normalization includes:
    /// <list type="bullet">
    /// <item>Trimming whitespace</item>
    /// <item>Converting to uppercase for consistency</item>
    /// <item>Validation against allowed activities</item>
    /// </list>
    /// </remarks>
    [return: NotNullIfNotNull(nameof(activity))]
    public static string? NormalizeActivity(string? activity)
    {
        if (string.IsNullOrWhiteSpace(activity))
            return null;

        var trimmed = activity.Trim();
        if (trimmed.Length == 0) return string.Empty;

        var normalized = trimmed.ToUpperInvariant();
        // Fix: If input is non-null, always return non-null (empty string if not allowed)
        return _allowedActivitiesSet.Contains(normalized) ? normalized : string.Empty;
    }

    #endregion Normalization Methods

    #region Business Rule Validation

    /// <summary>
    /// Validates revision activity context for professional practice requirements.
    /// </summary>
    /// <param name="activity">The activity name.</param>
    /// <param name="revisionIsDeleted">Whether the revision is deleted.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateActivityContext(
        string? activity,
        bool revisionIsDeleted,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (string.IsNullOrWhiteSpace(activity))
            yield break;

        if (IsActivityAppropriateForRevisionStatus(activity, revisionIsDeleted)) yield break;
        var revisionStatus = revisionIsDeleted ? "deleted" : "active";
        yield return new ValidationResult(
            $"Activity '{activity}' is not appropriate for a {revisionStatus} revision.",
            [propertyName]);
    }

    #endregion Business Rule Validation

    #region Utility Methods

    /// <summary>
    /// Determines if two revision activities are equivalent after normalization.
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
            ["MinAllowedDate"] = MinAllowedRevisionDate,
            ["FutureDateToleranceMinutes"] = FutureDateToleranceMinutes,
            ["ValidationRules"] = new Dictionary<string, string>
            {
                ["ActivityLength"] = $"Between {MinActivityLength} and {MaxActivityLength} characters",
                ["AllowedValues"] = AllowedActivitiesList,
                ["DateRange"] = $"Between {MinAllowedRevisionDate:yyyy-MM-dd} and present",
                ["FormatRule"] = "Must be uppercase and match allowed activities"
            }
        };
    }

    #endregion Utility Methods
}