using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ADMS.Application.Common.Validation;

/// <summary>
/// Provides comprehensive validation helper methods for revision-related data within the ADMS document management solution.
/// </summary>
/// <remarks>
/// This validation helper follows Clean Architecture principles and provides optimized validation for Revision entities and DTOs:
/// <list type="bullet">
/// <item><strong>Revision Number Validation:</strong> Sequential numbering and version control consistency</item>
/// <item><strong>Temporal Validation:</strong> Creation and modification date consistency for audit trails</item>
/// <item><strong>Version Control Validation:</strong> Professional document version control standards</item>
/// <item><strong>Business Rule Validation:</strong> Document revision lifecycle and professional standards</item>
/// </list>
/// 
/// <para><strong>Revision Context in ADMS:</strong></para>
/// Revisions in this system represent document versions with comprehensive audit trails, supporting:
/// <list type="bullet">
/// <item>Sequential version numbering for document evolution tracking</item>
/// <item>Temporal consistency for legal document chronology</item>
/// <item>Version control operations (creation, modification, deletion, restoration)</item>
/// <item>Professional audit trail requirements for legal compliance</item>
/// </list>
/// </remarks>
public static partial class RevisionValidationHelper
{
    #region Core Constants

    /// <summary>
    /// Maximum allowed revision number for version control consistency.
    /// </summary>
    public const int MaxRevisionNumber = 999999;

    /// <summary>
    /// Minimum allowed revision number (revisions start at 1).
    /// </summary>
    public const int MinRevisionNumber = 1;

    /// <summary>
    /// Earliest allowed date for revision operations (prevents unrealistic dates).
    /// </summary>
    public static readonly DateTime MinAllowedRevisionDate = new(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Tolerance in minutes for future dates (accounts for clock skew).
    /// </summary>
    public const int FutureDateToleranceMinutes = 5;

    /// <summary>
    /// Maximum reasonable time span between creation and modification dates.
    /// </summary>
    public static readonly TimeSpan MaxDateTimeSpan = TimeSpan.FromDays(3650); // 10 years

    /// <summary>
    /// Maximum reasonable age for active revisions in years.
    /// </summary>
    public const int MaxReasonableAgeYears = 10;

    #endregion Core Constants

    #region Revision Number Validation

    /// <summary>
    /// Validates a revision number according to version control standards.
    /// </summary>
    /// <param name="revisionNumber">The revision number to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates revision numbers for professional version control including:
    /// <list type="bullet">
    /// <item>Range validation (1 to 999,999)</item>
    /// <item>Sequential numbering requirements</item>
    /// <item>Professional standards for document versioning</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateRevisionNumber(int revisionNumber, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        switch (revisionNumber)
        {
            case < MinRevisionNumber:
                yield return new ValidationResult(
                    $"{propertyName} must be at least {MinRevisionNumber} (revisions start at 1).",
                    [propertyName]);
                break;
            case > MaxRevisionNumber:
                yield return new ValidationResult(
                    $"{propertyName} cannot exceed {MaxRevisionNumber} for version control consistency.",
                    [propertyName]);
                break;
        }
    }

    /// <summary>
    /// Validates sequential revision numbering within a document context.
    /// </summary>
    /// <param name="revisionNumber">The current revision number.</param>
    /// <param name="existingRevisionNumbers">Collection of existing revision numbers for the document.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates that revision numbers maintain sequential order without gaps for
    /// professional version control and audit trail consistency.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateSequentialRevisionNumber(
        int revisionNumber,
        IEnumerable<int>? existingRevisionNumbers,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // First validate basic number constraints
        foreach (var result in ValidateRevisionNumber(revisionNumber, propertyName))
            yield return result;

        if (existingRevisionNumbers == null) yield break;

        var existingNumbers = existingRevisionNumbers.ToList();
        
        // Check for duplicate revision numbers
        if (existingNumbers.Contains(revisionNumber))
        {
            yield return new ValidationResult(
                $"{propertyName} {revisionNumber} already exists. Revision numbers must be unique within each document.",
                [propertyName]);
        }

        // Check for sequential numbering (no gaps)
        var allNumbers = existingNumbers.Concat([revisionNumber]).OrderBy(x => x).ToList();
        for (var i = 0; i < allNumbers.Count; i++)
        {
            if (allNumbers[i] == i + 1) continue;
            yield return new ValidationResult(
                $"Revision numbering must be sequential without gaps. Expected revision {i + 1}, found {allNumbers[i]}.",
                [propertyName]);
            break;
        }
    }

    #endregion Revision Number Validation

    #region Date Validation

    /// <summary>
    /// Validates a revision date according to professional and system requirements.
    /// </summary>
    /// <param name="dateValue">The date to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates revision dates for professional legal practice including:
    /// <list type="bullet">
    /// <item>Valid date range for system operations</item>
    /// <item>UTC consistency requirements</item>
    /// <item>Reasonable bounds for legal document management</item>
    /// <item>Clock skew tolerance for distributed systems</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateDate(DateTime dateValue, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (dateValue == default)
        {
            yield return new ValidationResult(
                $"{propertyName} is required and cannot be the default value.",
                [propertyName]);
            yield break;
        }

        if (dateValue < MinAllowedRevisionDate)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot be earlier than {MinAllowedRevisionDate:yyyy-MM-dd} for system consistency.",
                [propertyName]);
        }

        var maxAllowedDate = DateTime.UtcNow.AddMinutes(FutureDateToleranceMinutes);
        if (dateValue > maxAllowedDate)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot be in the future (beyond clock skew tolerance of {FutureDateToleranceMinutes} minutes).",
                [propertyName]);
        }

        // Check for reasonable age
        var age = DateTime.UtcNow - dateValue;
        if (age.TotalDays > MaxReasonableAgeYears * 365)
        {
            yield return new ValidationResult(
                $"{propertyName} age exceeds reasonable bounds for active document management ({MaxReasonableAgeYears} years).",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates the chronological sequence of revision dates.
    /// </summary>
    /// <param name="creationDate">The creation date.</param>
    /// <param name="modificationDate">The modification date.</param>
    /// <param name="creationPropertyName">The property name for creation date.</param>
    /// <param name="modificationPropertyName">The property name for modification date.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when property names are null or whitespace.</exception>
    /// <remarks>
    /// Validates that modification dates are not before creation dates for
    /// chronological consistency in audit trails and version control.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateDateSequence(
        DateTime creationDate,
        DateTime modificationDate,
        [NotNull] string creationPropertyName,
        [NotNull] string modificationPropertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(creationPropertyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(modificationPropertyName);

        if (modificationDate < creationDate)
        {
            yield return new ValidationResult(
                $"{modificationPropertyName} cannot be before {creationPropertyName} for chronological consistency.",
                [modificationPropertyName, creationPropertyName]);
        }

        // Check for reasonable time span
        var timeSpan = modificationDate - creationDate;
        if (timeSpan > MaxDateTimeSpan)
        {
            yield return new ValidationResult(
                $"Time span between {creationPropertyName} and {modificationPropertyName} exceeds reasonable bounds ({MaxDateTimeSpan.TotalDays:F0} days).",
                [creationPropertyName, modificationPropertyName]);
        }
    }

    #endregion Date Validation

    #region GUID Validation

    /// <summary>
    /// Validates a revision ID GUID according to system requirements.
    /// </summary>
    /// <param name="revisionId">The revision ID to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates revision IDs for system consistency including:
    /// <list type="bullet">
    /// <item>Non-empty GUID requirements</item>
    /// <item>System identifier standards</item>
    /// <item>Database integrity requirements</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateRevisionId(Guid revisionId, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (revisionId == Guid.Empty)
        {
            yield return new ValidationResult(
                $"{propertyName} must be a valid non-empty GUID for system identification.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates a document ID GUID for revision association.
    /// </summary>
    /// <param name="documentId">The document ID to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates document association for revision context and referential integrity.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateDocumentId(Guid documentId, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (documentId == Guid.Empty)
        {
            yield return new ValidationResult(
                $"{propertyName} must be a valid non-empty GUID for document association.",
                [propertyName]);
        }
    }

    #endregion GUID Validation

    #region Quick Validation Methods (Performance Optimized)

    /// <summary>
    /// Quick validation check for revision number (optimized for performance).
    /// </summary>
    /// <param name="revisionNumber">The revision number to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidRevisionNumber(int revisionNumber)
    {
        return revisionNumber >= MinRevisionNumber && revisionNumber <= MaxRevisionNumber;
    }

    /// <summary>
    /// Quick validation check for revision date (optimized for performance).
    /// </summary>
    /// <param name="dateValue">The date to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidDate(DateTime dateValue)
    {
        return dateValue > MinAllowedRevisionDate &&
               dateValue <= DateTime.UtcNow.AddMinutes(FutureDateToleranceMinutes) &&
               (DateTime.UtcNow - dateValue).TotalDays <= (MaxReasonableAgeYears * 365);
    }

    /// <summary>
    /// Quick validation check for date sequence (optimized for performance).
    /// </summary>
    /// <param name="creationDate">The creation date.</param>
    /// <param name="modificationDate">The modification date.</param>
    /// <returns>True if valid sequence; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidDateSequence(DateTime creationDate, DateTime modificationDate)
    {
        return modificationDate >= creationDate &&
               modificationDate - creationDate <= MaxDateTimeSpan;
    }

    /// <summary>
    /// Quick validation check for document ID (optimized for performance).
    /// </summary>
    /// <param name="documentId">The document ID to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidDocumentId(Guid documentId)
    {
        return documentId != Guid.Empty;
    }

    /// <summary>
    /// Quick validation check for revision ID (optimized for performance).
    /// </summary>
    /// <param name="revisionId">The revision ID to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidRevisionId(Guid revisionId)
    {
        return revisionId != Guid.Empty;
    }

    /// <summary>
    /// Quick validation check for date time span (optimized for performance).
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>True if valid time span; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidDateTimeSpan(DateTime startDate, DateTime endDate)
    {
        return endDate >= startDate && endDate - startDate <= MaxDateTimeSpan;
    }

    #endregion Quick Validation Methods

    #region Normalization and Utility Methods

    /// <summary>
    /// Normalizes a revision date to UTC for consistent storage.
    /// </summary>
    /// <param name="dateValue">The date to normalize.</param>
    /// <returns>UTC date or null if invalid.</returns>
    public static DateTime? NormalizeRevisionDate(DateTime dateValue)
    {
        if (!IsValidDate(dateValue))
            return null;

        return dateValue.Kind switch
        {
            DateTimeKind.Utc => dateValue,
            DateTimeKind.Local => dateValue.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateValue, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(dateValue, DateTimeKind.Utc)
        };
    }

    /// <summary>
    /// Calculates the expected next revision number in a sequence.
    /// </summary>
    /// <param name="existingRevisionNumbers">Existing revision numbers for the document.</param>
    /// <returns>The next expected revision number.</returns>
    public static int GetNextRevisionNumber(IEnumerable<int>? existingRevisionNumbers)
    {
        if (existingRevisionNumbers == null || !existingRevisionNumbers.Any())
            return MinRevisionNumber;

        return existingRevisionNumbers.Max() + 1;
    }

    /// <summary>
    /// Validates revision number sequence and suggests corrections.
    /// </summary>
    /// <param name="existingRevisionNumbers">Existing revision numbers for validation.</param>
    /// <returns>Dictionary containing validation status and suggestions.</returns>
    public static IReadOnlyDictionary<string, object> AnalyzeRevisionSequence(IEnumerable<int>? existingRevisionNumbers)
    {
        var analysis = new Dictionary<string, object>();

        if (existingRevisionNumbers == null || !existingRevisionNumbers.Any())
        {
            analysis["IsValid"] = true;
            analysis["NextRevisionNumber"] = MinRevisionNumber;
            analysis["HasGaps"] = false;
            analysis["Suggestions"] = Array.Empty<string>();
            return analysis;
        }

        var numbers = existingRevisionNumbers.OrderBy(x => x).ToList();
        var hasGaps = false;
        var gaps = new List<int>();

        for (var i = 0; i < numbers.Count; i++)
        {
            var expected = i + 1;
            if (numbers[i] == expected) continue;
            hasGaps = true;
            gaps.Add(expected);
        }

        var suggestions = new List<string>();
        if (hasGaps)
        {
            suggestions.Add($"Fill revision number gaps: {string.Join(", ", gaps)}");
        }

        analysis["IsValid"] = !hasGaps && numbers.All(IsValidRevisionNumber);
        analysis["NextRevisionNumber"] = GetNextRevisionNumber(numbers);
        analysis["HasGaps"] = hasGaps;
        analysis["MissingNumbers"] = gaps;
        analysis["Suggestions"] = suggestions;
        analysis["HighestRevision"] = numbers.Count > 0 ? numbers.Max() : 0;
        analysis["TotalRevisions"] = numbers.Count;

        return analysis;
    }

    /// <summary>
    /// Gets revision validation statistics for diagnostics and monitoring.
    /// </summary>
    /// <returns>Dictionary containing revision validation statistics.</returns>
    public static IReadOnlyDictionary<string, object> GetValidationStatistics()
    {
        return new Dictionary<string, object>
        {
            ["MaxRevisionNumber"] = MaxRevisionNumber,
            ["MinRevisionNumber"] = MinRevisionNumber,
            ["MinAllowedDate"] = MinAllowedRevisionDate,
            ["FutureDateToleranceMinutes"] = FutureDateToleranceMinutes,
            ["MaxDateTimeSpanDays"] = MaxDateTimeSpan.TotalDays,
            ["MaxReasonableAgeYears"] = MaxReasonableAgeYears,
            ["ValidationRules"] = new Dictionary<string, string>
            {
                ["RevisionNumbers"] = $"Sequential from {MinRevisionNumber} to {MaxRevisionNumber}",
                ["DateRange"] = $"Between {MinAllowedRevisionDate:yyyy-MM-dd} and present (+{FutureDateToleranceMinutes}min tolerance)",
                ["SequentialRule"] = "Revision numbers must be sequential without gaps within documents",
                ["TemporalRule"] = "Modification date must be >= creation date",
                ["AgeLimit"] = $"Maximum reasonable age: {MaxReasonableAgeYears} years"
            }
        };
    }

    #endregion Normalization and Utility Methods

    #region Business Rule Validation

    /// <summary>
    /// Validates comprehensive revision business rules for professional document management.
    /// </summary>
    /// <param name="revisionNumber">The revision number.</param>
    /// <param name="creationDate">The creation date.</param>
    /// <param name="modificationDate">The modification date.</param>
    /// <param name="documentId">The associated document ID.</param>
    /// <param name="isDeleted">Whether the revision is deleted.</param>
    /// <param name="propertyPrefix">The property prefix for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyPrefix is null or whitespace.</exception>
    /// <remarks>
    /// Validates comprehensive business rules for revision management including professional
    /// standards, audit trail requirements, and version control integrity.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateRevisionBusinessRules(
        int revisionNumber,
        DateTime creationDate,
        DateTime modificationDate,
        Guid? documentId,
        bool isDeleted,
        [NotNull] string propertyPrefix = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyPrefix);

        // Core validation
        foreach (var result in ValidateRevisionNumber(revisionNumber, $"{propertyPrefix}RevisionNumber"))
            yield return result;

        foreach (var result in ValidateDate(creationDate, $"{propertyPrefix}CreationDate"))
            yield return result;

        foreach (var result in ValidateDate(modificationDate, $"{propertyPrefix}ModificationDate"))
            yield return result;

        foreach (var result in ValidateDateSequence(creationDate, modificationDate, 
            $"{propertyPrefix}CreationDate", $"{propertyPrefix}ModificationDate"))
            yield return result;

        // Document association validation
        if (documentId.HasValue)
        {
            foreach (var result in ValidateDocumentId(documentId.Value, $"{propertyPrefix}DocumentId"))
                yield return result;
        }

        // Professional standards validation
        var timeSpan = modificationDate - creationDate;
        
        // Check for suspicious immediate modifications (might indicate data issues)
        if (timeSpan == TimeSpan.Zero && revisionNumber > 1)
        {
            // This is informational rather than an error - some systems create revisions instantaneously
            // In production, you might log this for investigation rather than fail validation
        }

        // Check for deleted revision consistency
        if (isDeleted)
        {
            // Deleted revisions should maintain audit trail integrity
            // Additional business rules for deleted revisions can be added here
        }
    }

    #endregion Business Rule Validation
}