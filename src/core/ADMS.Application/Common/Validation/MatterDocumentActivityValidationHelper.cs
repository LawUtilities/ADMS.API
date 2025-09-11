using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ADMS.Application.Common.Validation;

/// <summary>
/// Provides comprehensive validation helper methods for matter document activity classification within the ADMS document management solution.
/// </summary>
/// <remarks>
/// This validation helper follows Clean Architecture principles and provides optimized validation for MatterDocumentActivity entities and DTOs:
/// <list type="bullet">
/// <item><strong>Transfer Activity Classification:</strong> Validates document transfer operations between matters</item>
/// <item><strong>Bidirectional Validation:</strong> Ensures proper source/destination activity tracking</item>
/// <item><strong>Professional Standards:</strong> Validates activities for legal practice requirements</item>
/// <item><strong>Audit Trail Integrity:</strong> Ensures complete audit trail coverage for document transfers</item>
/// </list>
/// 
/// <para><strong>Matter Document Activity Context in ADMS:</strong></para>
/// Matter document activities in this system represent document transfer operations between matters, supporting:
/// <list type="bullet">
/// <item>Document movement between client matters</item>
/// <item>Document copying for multi-matter case management</item>
/// <item>Professional audit trail requirements for document custody</item>
/// <item>Legal compliance for document provenance tracking</item>
/// </list>
/// </remarks>
public static class MatterDocumentActivityValidationHelper
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

    #region Allowed Matter Document Activities

    /// <summary>
    /// Standard matter document transfer activities supported by the ADMS system (optimized with FrozenSet).
    /// </summary>
    /// <remarks>
    /// These activities correspond to the seeded data in ADMS.API.Entities.MatterDocumentActivity:
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> Document moved from source matter to destination matter (custody transfer)</item>
    /// <item><strong>COPIED:</strong> Document copied from source matter to destination matter (duplication)</item>
    /// </list>
    /// </remarks>
    private static readonly string[] _allowedActivitiesArray =
    [
        "MOVED",
        "COPIED"
    ];

    /// <summary>
    /// High-performance frozen set for allowed activity lookups.
    /// </summary>
    private static readonly FrozenSet<string> _allowedActivitiesSet =
        _allowedActivitiesArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the immutable list of allowed matter document activities.
    /// </summary>
    public static IReadOnlyList<string> AllowedActivities => _allowedActivitiesArray;

    /// <summary>
    /// Activities that represent document transfers between matters.
    /// </summary>
    private static readonly FrozenSet<string> _transferActivities =
        _allowedActivitiesArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Activities that require bidirectional audit trail tracking.
    /// </summary>
    private static readonly FrozenSet<string> _bidirectionalActivities =
        _allowedActivitiesArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    #endregion Allowed Matter Document Activities

    #region Activity Validation

    /// <summary>
    /// Validates a matter document activity classification according to business rules.
    /// </summary>
    /// <param name="activity">The activity classification to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates matter document activities for professional legal document management including:
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
                $"{propertyName} is required for transfer activity classification.",
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
                $"{propertyName} '{trimmed}' is not a recognized matter document activity. Allowed activities: {allowedList}",
                [propertyName]);
        }

        // Format validation
        foreach (var result in ValidateActivityFormat(trimmed, propertyName))
            yield return result;
    }

    /// <summary>
    /// Validates matter document activity context for professional legal practice.
    /// </summary>
    /// <param name="activity">The matter document activity.</param>
    /// <param name="sourceMatterId">The source matter ID.</param>
    /// <param name="destinationMatterId">The destination matter ID.</param>
    /// <param name="documentId">The document being transferred.</param>
    /// <param name="userId">The user performing the activity.</param>
    /// <param name="activityTimestamp">The activity timestamp.</param>
    /// <param name="propertyPrefix">The property prefix for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyPrefix is null or whitespace.</exception>
    /// <remarks>
    /// Validates the complete context of a matter document transfer activity to ensure professional
    /// accountability and audit trail integrity.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateTransferContext(
        string? activity,
        Guid sourceMatterId,
        Guid destinationMatterId,
        Guid documentId,
        Guid userId,
        DateTime activityTimestamp,
        [NotNull] string propertyPrefix = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyPrefix);

        // Core activity validation
        foreach (var result in ValidateActivity(activity, $"{propertyPrefix}Activity"))
            yield return result;

        // Context validation
        if (sourceMatterId == Guid.Empty)
        {
            yield return new ValidationResult(
                "Transfer activity must have a valid source matter for audit trail integrity.",
                [$"{propertyPrefix}SourceMatterId"]);
        }

        if (destinationMatterId == Guid.Empty)
        {
            yield return new ValidationResult(
                "Transfer activity must have a valid destination matter for audit trail integrity.",
                [$"{propertyPrefix}DestinationMatterId"]);
        }

        if (sourceMatterId == destinationMatterId && sourceMatterId != Guid.Empty)
        {
            yield return new ValidationResult(
                "Source and destination matters cannot be the same for transfer operations.",
                [$"{propertyPrefix}SourceMatterId", $"{propertyPrefix}DestinationMatterId"]);
        }

        if (documentId == Guid.Empty)
        {
            yield return new ValidationResult(
                "Transfer activity must be associated with a valid document.",
                [$"{propertyPrefix}DocumentId"]);
        }

        if (userId == Guid.Empty)
        {
            yield return new ValidationResult(
                "Transfer activity must be attributed to a valid user for accountability.",
                [$"{propertyPrefix}UserId"]);
        }

        // Timestamp validation using UserValidationHelper patterns
        foreach (var result in UserValidationHelper.ValidateActivityTimestamp(activityTimestamp, $"{propertyPrefix}Timestamp"))
            yield return result;
    }

    /// <summary>
    /// Validates transfer activity business rules for professional legal practice.
    /// </summary>
    /// <param name="activity">The transfer activity.</param>
    /// <param name="sourceMatterIsDeleted">Whether the source matter is deleted.</param>
    /// <param name="destinationMatterIsDeleted">Whether the destination matter is deleted.</param>
    /// <param name="documentIsDeleted">Whether the document is deleted.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates business rules for document transfers between matters:
    /// <list type="bullet">
    /// <item>Cannot transfer documents from or to deleted matters</item>
    /// <item>Cannot transfer deleted documents (except for restoration scenarios)</item>
    /// <item>Professional practice requirements for document custody</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateTransferBusinessRules(
        string? activity,
        bool sourceMatterIsDeleted,
        bool destinationMatterIsDeleted,
        bool documentIsDeleted,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (string.IsNullOrWhiteSpace(activity))
            yield break;

        var normalizedActivity = activity.Trim().ToUpperInvariant();

        // Cannot transfer from deleted matters
        if (sourceMatterIsDeleted)
        {
            yield return new ValidationResult(
                $"{propertyName}: Cannot {normalizedActivity.ToLowerInvariant()} documents from deleted matters.",
                [propertyName]);
        }

        // Cannot transfer to deleted matters
        if (destinationMatterIsDeleted)
        {
            yield return new ValidationResult(
                $"{propertyName}: Cannot {normalizedActivity.ToLowerInvariant()} documents to deleted matters.",
                [propertyName]);
        }

        // Cannot transfer deleted documents
        if (documentIsDeleted)
        {
            yield return new ValidationResult(
                $"{propertyName}: Cannot {normalizedActivity.ToLowerInvariant()} deleted documents. Restore document first.",
                [propertyName]);
        }

        // Additional business rules based on activity type
        switch (normalizedActivity)
        {
            case "MOVED":
                // For MOVED operations, additional custody validation could be added here
                break;

            case "COPIED":
                // For COPIED operations, additional duplication validation could be added here
                break;
        }
    }

    #endregion Activity Validation

    #region Quick Validation Methods (Performance Optimized)

    /// <summary>
    /// Quick validation check for matter document activity (optimized for performance).
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
    /// Determines if an activity is a transfer operation.
    /// </summary>
    /// <param name="activity">The activity to check.</param>
    /// <returns>True if transfer activity; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTransferActivity([NotNullWhen(true)] string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) &&
               _transferActivities.Contains(activity.Trim());
    }

    /// <summary>
    /// Determines if an activity requires bidirectional tracking.
    /// </summary>
    /// <param name="activity">The activity to check.</param>
    /// <returns>True if requires bidirectional tracking; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool RequiresBidirectionalTracking([NotNullWhen(true)] string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) &&
               _bidirectionalActivities.Contains(activity.Trim());
    }

    /// <summary>
    /// Determines if an activity represents document custody transfer.
    /// </summary>
    /// <param name="activity">The activity to check.</param>
    /// <returns>True if custody transfer; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCustodyTransfer([NotNullWhen(true)] string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) &&
               activity.Trim().Equals("MOVED", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if an activity represents document duplication.
    /// </summary>
    /// <param name="activity">The activity to check.</param>
    /// <returns>True if duplication; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDuplication([NotNullWhen(true)] string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) &&
               activity.Trim().Equals("COPIED", StringComparison.OrdinalIgnoreCase);
    }

    #endregion Quick Validation Methods

    #region Normalization and Utility Methods

    /// <summary>
    /// Normalizes a matter document activity classification for consistent storage.
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
        return _allowedActivitiesSet.Contains(normalized) ? normalized : trimmed;
    }

    /// <summary>
    /// Gets the activity category for classification and reporting.
    /// </summary>
    /// <param name="activity">The matter document activity.</param>
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
            "MOVED" => "Custody Transfer",
            "COPIED" => "Duplication",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Determines the professional impact level of a matter document activity.
    /// </summary>
    /// <param name="activity">The matter document activity.</param>
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
            "MOVED" => "High",    // Custody transfer has high impact
            "COPIED" => "Medium", // Duplication has medium impact
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets the professional description for a transfer activity.
    /// </summary>
    /// <param name="activity">The transfer activity.</param>
    /// <returns>Professional description string.</returns>
    /// <remarks>
    /// Professional descriptions provide clear, professional language for audit trails
    /// and client communication about document transfer operations.
    /// </remarks>
    public static string GetProfessionalDescription(string? activity)
    {
        if (string.IsNullOrWhiteSpace(activity))
            return "Unknown transfer operation";

        var normalized = activity.Trim().ToUpperInvariant();

        return normalized switch
        {
            "MOVED" => "Document custody transferred between matters",
            "COPIED" => "Document duplicated for multi-matter access",
            _ => "Transfer operation performed"
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
            ["TransferActivitiesCount"] = _transferActivities.Count,
            ["BidirectionalActivitiesCount"] = _bidirectionalActivities.Count,
            ["AllowedActivities"] = AllowedActivities.ToArray(),
            ["ValidationRules"] = new Dictionary<string, string>
            {
                ["ActivityLength"] = $"Between {MinActivityLength} and {MaxActivityLength} characters",
                ["AllowedValues"] = string.Join(", ", AllowedActivities),
                ["TransferTypes"] = "MOVED (custody transfer), COPIED (duplication)",
                ["BidirectionalRule"] = "All activities require source and destination tracking",
                ["BusinessRule"] = "Cannot transfer documents from/to deleted matters"
            }
        };
    }

    #endregion Normalization and Utility Methods

    #region Private Helper Methods

    /// <summary>
    /// Validates matter document activity format for consistency and professionalism.
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
        if (!activity.All(c => char.IsLetter(c)))
        {
            yield return new ValidationResult(
                $"{propertyName} can only contain letters for professional activity classification.",
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