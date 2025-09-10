using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ADMS.Application.Common.Validation;

/// <summary>
/// Provides comprehensive validation helper methods for matter document activity-related data within the ADMS document management solution.
/// </summary>
/// <remarks>
/// This validation helper follows Clean Architecture principles and provides optimized validation for MatterDocumentActivity operations:
/// <list type="bullet">
/// <item><strong>Document Transfer Validation:</strong> Ensures proper document transfer operations between matters</item>
/// <item><strong>Activity Classification:</strong> Validates transfer activity types (MOVED, COPIED)</item>
/// <item><strong>Professional Standards:</strong> Enforces legal practice document handling requirements</item>
/// </list>
/// </remarks>
public static class MatterDocumentActivityValidationHelper
{
    #region Core Constants

    /// <summary>
    /// Maximum allowed length for a matter document activity name.
    /// </summary>
    public const int MaxActivityLength = 50;

    /// <summary>
    /// Minimum allowed length for a matter document activity name.
    /// </summary>
    public const int MinActivityLength = 2;

    #endregion Core Constants

    #region Allowed Activities (High Performance)

    /// <summary>
    /// Standard matter document activities that are allowed in the system.
    /// </summary>
    private static readonly string[] _allowedActivitiesArray =
    [
        "MOVED",    // Document moved from one matter to another
        "COPIED"    // Document copied from one matter to another
    ];

    /// <summary>
    /// High-performance frozen set for allowed activity lookups.
    /// </summary>
    private static readonly FrozenSet<string> _allowedActivitiesSet =
        _allowedActivitiesArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the immutable list of allowed activities.
    /// </summary>
    public static IReadOnlyList<string> AllowedActivitiesList => _allowedActivitiesArray;

    #endregion Allowed Activities

    #region Activity Validation

    /// <summary>
    /// Validates a matter document activity name according to business rules.
    /// </summary>
    /// <param name="activity">The activity name to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateActivity(string? activity, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Required validation
        if (string.IsNullOrWhiteSpace(activity))
        {
            yield return new ValidationResult(
                $"{propertyName} is required for document transfer classification.",
                [propertyName]);
            yield break;
        }

        var trimmed = activity.Trim();

        // Length validation
        if (trimmed.Length < MinActivityLength)
        {
            yield return new ValidationResult(
                $"{propertyName} must be at least {MinActivityLength} characters long.",
                [propertyName]);
        }

        if (trimmed.Length > MaxActivityLength)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot exceed {MaxActivityLength} characters.",
                [propertyName]);
        }

        // Allowed activity validation
        if (!_allowedActivitiesSet.Contains(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} '{trimmed}' is not allowed. Allowed activities: {string.Join(", ", _allowedActivitiesArray)}",
                [propertyName]);
        }
    }

    #endregion Activity Validation

    #region Quick Validation Methods (Performance Optimized)

    /// <summary>
    /// Quick validation check for activity name (optimized for performance).
    /// </summary>
    /// <param name="activity">The activity name to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsActivityAllowed([NotNullWhen(true)] string? activity)
    {
        if (string.IsNullOrWhiteSpace(activity))
            return false;

        var trimmed = activity.Trim();
        return trimmed.Length >= MinActivityLength &&
               trimmed.Length <= MaxActivityLength &&
               _allowedActivitiesSet.Contains(trimmed);
    }

    #endregion Quick Validation Methods
}