using ADMS.Application.DTOs;

using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
/// <item><strong>Activity Audit Trail Validation:</strong> Complete audit trail validation for legal compliance</item>
/// <item><strong>Professional Standards Validation:</strong> Legal practice requirements and workload patterns</item>
/// </list>
/// 
/// <para><strong>Revision Context in ADMS:</strong></para>
/// Revisions in this system represent document versions with comprehensive audit trails, supporting:
/// <list type="bullet">
/// <item>Sequential version numbering for document evolution tracking</item>
/// <item>Temporal consistency for legal document chronology</item>
/// <item>Version control operations (creation, modification, deletion, restoration)</item>
/// <item>Professional audit trail requirements for legal compliance</item>
/// <item>Document-wide consistency and professional standards</item>
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

    /// <summary>
    /// Maximum reasonable activity count for professional document management.
    /// </summary>
    public const int MaxReasonableActivityCount = 50;

    /// <summary>
    /// Maximum reasonable revision number for professional practice.
    /// </summary>
    public const int MaxProfessionalRevisionNumber = 100;

    /// <summary>
    /// Maximum reasonable development time for first revision (in hours).
    /// </summary>
    public const int MaxFirstRevisionDevelopmentHours = 24;

    #endregion Core Constants

    #region Allowed Activity Types

    /// <summary>
    /// Standard revision activity types supported by the ADMS system.
    /// </summary>
    private static readonly string[] _allowedRevisionActivitiesArray =
    [
        "CREATED",
        "SAVED", 
        "DELETED",
        "RESTORED"
    ];

    /// <summary>
    /// High-performance frozen set for allowed activity lookups.
    /// </summary>
    private static readonly FrozenSet<string> _allowedRevisionActivitiesSet =
        _allowedRevisionActivitiesArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the immutable list of allowed revision activities.
    /// </summary>
    public static IReadOnlyList<string> AllowedRevisionActivities => _allowedRevisionActivitiesArray;

    #endregion Allowed Activity Types

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

    /// <summary>
    /// Validates revision date consistency within document context.
    /// </summary>
    /// <param name="revisionCreationDate">The revision creation date.</param>
    /// <param name="documentCreationDate">The document creation date.</param>
    /// <param name="revisionNumber">The revision number.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates that revision dates are consistent with document creation timeline
    /// and professional document management standards.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateDocumentRevisionDates(
        DateTime revisionCreationDate,
        DateTime documentCreationDate,
        int revisionNumber,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // First revision can be created at the same time or after document creation
        if (revisionNumber == 1)
        {
            if (revisionCreationDate < documentCreationDate.AddMinutes(-FutureDateToleranceMinutes))
            {
                yield return new ValidationResult(
                    $"{propertyName} cannot be before document creation date for first revision.",
                    [propertyName]);
            }
        }
        else
        {
            // Subsequent revisions should be after document creation
            if (revisionCreationDate < documentCreationDate)
            {
                yield return new ValidationResult(
                    $"{propertyName} cannot be before document creation date for revision {revisionNumber}.",
                    [propertyName]);
            }
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

    #region Activity Audit Trail Validation

    /// <summary>
    /// Validates that revision activity audit trails are complete and consistent for legal compliance.
    /// </summary>
    /// <param name="activities">The collection of revision activities to validate.</param>
    /// <param name="revisionNumber">The revision number for context.</param>
    /// <param name="isDeleted">Whether the revision is deleted.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates audit trail completeness for professional legal practice:
    /// <list type="bullet">
    /// <item>Required activities based on revision lifecycle</item>
    /// <item>Activity sequence and chronological order</item>
    /// <item>Professional audit trail standards</item>
    /// <item>Legal compliance requirements</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateActivityAuditTrail(
        IEnumerable<RevisionActivityUserDto>? activities,
        int revisionNumber,
        bool isDeleted,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (activities == null || !activities.Any())
        {
            if (!isDeleted)
            {
                yield return new ValidationResult(
                    $"{propertyName}: Active revisions must have audit trail activities for legal compliance.",
                    [propertyName]);
            }
            yield break;
        }

        var activityList = activities.ToList();
        
        // Validate required activities exist
        foreach (var result in ValidateRequiredActivities(activityList, revisionNumber, isDeleted, propertyName))
            yield return result;

        // Validate activity sequence
        foreach (var result in ValidateActivitySequence(activityList, propertyName))
            yield return result;

        // Validate activity uniqueness (no duplicate activity types by same user at same time)
        var duplicateActivities = activityList
            .GroupBy(a => new { a.RevisionActivity?.Activity, a.UserId, a.CreatedAt.Date })
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var duplicate in duplicateActivities)
        {
            yield return new ValidationResult(
                $"{propertyName}: Duplicate {duplicate.Key.Activity} activities found for same user on same date. Verify audit trail integrity.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates that required activities exist for revision lifecycle stages.
    /// </summary>
    /// <param name="activities">The collection of revision activities.</param>
    /// <param name="revisionNumber">The revision number for context.</param>
    /// <param name="isDeleted">Whether the revision is deleted.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates that essential activities exist based on revision state:
    /// <list type="bullet">
    /// <item>CREATED activity for all revisions (especially first revision)</item>
    /// <item>RESTORED activity for previously deleted revisions</item>
    /// <item>Professional activity coverage</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateRequiredActivities(
        IEnumerable<RevisionActivityUserDto> activities,
        int revisionNumber,
        bool isDeleted,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var activityTypes = activities
            .Select(a => a.RevisionActivity?.Activity?.ToUpperInvariant())
            .Where(a => !string.IsNullOrEmpty(a))
            .ToList();

        // First revision should have a CREATED activity
        if (revisionNumber == 1 && !activityTypes.Contains("CREATED"))
        {
            yield return new ValidationResult(
                $"{propertyName}: First revision must have a CREATED activity for complete audit trail.",
                [propertyName]);
        }

        // Active revisions should have at least a CREATED activity
        if (!isDeleted && activityTypes.Count == 0)
        {
            yield return new ValidationResult(
                $"{propertyName}: Active revisions must have at least one activity for audit trail compliance.",
                [propertyName]);
        }

        // If revision is not deleted but has DELETED activity, should have RESTORED
        if (!isDeleted && activityTypes.Contains("DELETED") && !activityTypes.Contains("RESTORED"))
        {
            yield return new ValidationResult(
                $"{propertyName}: Revision with DELETED activity should have corresponding RESTORED activity if currently active.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates activity sequence consistency for revision lifecycle.
    /// </summary>
    /// <param name="activities">The collection of revision activities in temporal order.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates professional activity sequences:
    /// <list type="bullet">
    /// <item>CREATED → SAVED → DELETED → RESTORED patterns</item>
    /// <item>Logical workflow consistency</item>
    /// <item>Professional version control standards</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateActivitySequence(
        IEnumerable<RevisionActivityUserDto> activities,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var orderedActivities = activities
            .Where(a => a.RevisionActivity?.Activity != null)
            .OrderBy(a => a.CreatedAt)
            .ToList();

        if (orderedActivities.Count <= 1) yield break;

        for (int i = 1; i < orderedActivities.Count; i++)
        {
            var previousActivity = orderedActivities[i - 1].RevisionActivity?.Activity?.ToUpperInvariant();
            var currentActivity = orderedActivities[i].RevisionActivity?.Activity?.ToUpperInvariant();

            if (!IsValidActivitySequence(previousActivity, currentActivity))
            {
                yield return new ValidationResult(
                    $"{propertyName}: Activity sequence from '{previousActivity}' to '{currentActivity}' may not follow professional version control standards.",
                    [propertyName]);
            }
        }
    }

    /// <summary>
    /// Validates activity timestamps align with revision lifecycle dates.
    /// </summary>
    /// <param name="revisionCreationDate">The revision creation date.</param>
    /// <param name="revisionModificationDate">The revision modification date.</param>
    /// <param name="activities">The collection of revision activities.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates temporal consistency between revision dates and activity timestamps.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateActivityTimestamps(
        DateTime revisionCreationDate,
        DateTime revisionModificationDate,
        IEnumerable<RevisionActivityUserDto>? activities,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (activities == null || !activities.Any()) yield break;

        var createdAtDates = activities.Select(activity => activity.CreatedAt);

        foreach (var createdAt in createdAtDates)
        {
            // Activities should generally occur between creation and modification dates
            if (createdAt < revisionCreationDate.AddMinutes(-FutureDateToleranceMinutes))
            {
                yield return new ValidationResult(
                    $"{propertyName}: Activity timestamp cannot be significantly before revision creation date.",
                    [propertyName]);
            }

            if (createdAt > revisionModificationDate.AddMinutes(FutureDateToleranceMinutes))
            {
                yield return new ValidationResult(
                    $"{propertyName}: Activity timestamp cannot be significantly after revision modification date.",
                    [propertyName]);
            }
        }
    }

    #endregion Activity Audit Trail Validation

    #region Professional Standards Validation

    /// <summary>
    /// Validates revision follows professional legal document standards.
    /// </summary>
    /// <param name="revisionNumber">The revision number.</param>
    /// <param name="creationDate">The creation date.</param>
    /// <param name="developmentTime">The time between creation and modification.</param>
    /// <param name="activityCount">The number of activities.</param>
    /// <param name="propertyPrefix">The property prefix for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyPrefix is null or whitespace.</exception>
    /// <remarks>
    /// Validates professional practice standards for legal document management.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateProfessionalStandards(
        int revisionNumber,
        DateTime creationDate,
        TimeSpan developmentTime,
        int activityCount,
        [NotNull] string propertyPrefix = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyPrefix);

        // Validate professional revision number
        if (revisionNumber > MaxProfessionalRevisionNumber)
        {
            yield return new ValidationResult(
                $"{propertyPrefix}RevisionNumber exceeds professional practice recommendations (>{MaxProfessionalRevisionNumber}). Consider document consolidation.",
                [propertyPrefix + "RevisionNumber"]);
        }

        // Validate first revision development time
        if (revisionNumber == 1 && developmentTime.TotalHours > MaxFirstRevisionDevelopmentHours)
        {
            yield return new ValidationResult(
                $"{propertyPrefix}First revision development time exceeds professional standards (>{MaxFirstRevisionDevelopmentHours} hours).",
                [propertyPrefix + "CreationDate", propertyPrefix + "ModificationDate"]);
        }

        // Validate activity count
        if (activityCount > MaxReasonableActivityCount)
        {
            yield return new ValidationResult(
                $"{propertyPrefix}Activity count exceeds reasonable bounds for professional practice (>{MaxReasonableActivityCount}).",
                [propertyPrefix + "ActivityCount"]);
        }

        // Validate revision age for active documents
        var revisionAge = DateTime.UtcNow - creationDate;
        if (revisionAge.TotalDays > MaxReasonableAgeYears * 365)
        {
            yield return new ValidationResult(
                $"{propertyPrefix}Revision age exceeds retention policy guidelines (>{MaxReasonableAgeYears} years). Review archival procedures.",
                [propertyPrefix + "CreationDate"]);
        }
    }

    /// <summary>
    /// Validates revision workload and activity patterns are reasonable.
    /// </summary>
    /// <param name="activityCount">The number of activities.</param>
    /// <param name="developmentTime">The development time span.</param>
    /// <param name="revisionNumber">The revision number.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates that revision workload patterns are professionally reasonable.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateRevisionWorkload(
        int activityCount,
        TimeSpan developmentTime,
        int revisionNumber,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Validate activity density (activities per day)
        if (developmentTime.TotalDays > 0)
        {
            var activitiesPerDay = activityCount / developmentTime.TotalDays;
            if (activitiesPerDay > 10) // More than 10 activities per day might indicate issues
            {
                yield return new ValidationResult(
                    $"{propertyName}: High activity density ({activitiesPerDay:F1} activities/day) may indicate data quality issues.",
                    [propertyName]);
            }
        }

        // Validate extended development time
        if (developmentTime.TotalDays > 365 && activityCount < 5)
        {
            yield return new ValidationResult(
                $"{propertyName}: Extended development time with minimal activity may indicate stale revision.",
                [propertyName]);
        }

        // Validate revision complexity vs activity count
        if (revisionNumber > 10 && activityCount == 1)
        {
            yield return new ValidationResult(
                $"{propertyName}: Higher revision number with minimal activities may indicate missing audit trail entries.",
                [propertyName]);
        }
    }

    #endregion Professional Standards Validation

    #region Collection and Document-Wide Validation

    /// <summary>
    /// Validates a collection of revisions for document-wide consistency.
    /// </summary>
    /// <param name="revisions">The collection of revisions to validate.</param>
    /// <param name="documentId">The document ID for context.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates revision collection consistency across an entire document.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateRevisionCollection(
        IEnumerable<RevisionDto>? revisions,
        Guid documentId,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (revisions == null || !revisions.Any())
        {
            yield return new ValidationResult(
                $"{propertyName}: Document should have at least one revision for professional document management.",
                [propertyName]);
            yield break;
        }

        var revisionList = revisions.ToList();

        // Validate document association
        var inconsistentDocuments = revisionList
            .Where(r => r.DocumentId.HasValue && r.DocumentId != documentId)
            .ToList();

        if (inconsistentDocuments.Count > 0)
        {
            yield return new ValidationResult(
                $"{propertyName}: All revisions must be associated with the same document ({documentId}).",
                [propertyName]);
        }

        // Validate revision sequence
        foreach (var result in ValidateDocumentRevisionSequence(revisionList, propertyName))
            yield return result;

        // Validate overall document health
        var totalActivities = revisionList.Sum(r => r.ActivityCount);
        if (totalActivities == 0)
        {
            yield return new ValidationResult(
                $"{propertyName}: Document revisions must have audit trail activities for legal compliance.",
                [propertyName]);
        }

        // Validate professional revision count
        if (revisionList.Count > MaxProfessionalRevisionNumber)
        {
            yield return new ValidationResult(
                $"{propertyName}: Document has excessive revision count (>{MaxProfessionalRevisionNumber}). Consider document lifecycle review.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates revision numbering sequence across a document.
    /// </summary>
    /// <param name="revisions">The collection of revisions to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates sequential revision numbering across all document revisions.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateDocumentRevisionSequence(
        IEnumerable<RevisionDto> revisions,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var revisionNumbers = revisions.Select(r => r.RevisionNumber).OrderBy(x => x).ToList();

        // Check for gaps in sequence
        for (int i = 0; i < revisionNumbers.Count; i++)
        {
            var expected = i + 1;
            if (revisionNumbers[i] != expected)
            {
                yield return new ValidationResult(
                    $"{propertyName}: Revision sequence has gaps. Expected revision {expected}, found {revisionNumbers[i]}.",
                    [propertyName]);
                break;
            }
        }

        // Check for duplicates
        var duplicates = revisionNumbers.GroupBy(x => x).Where(g => g.Count() > 1).ToList();
        foreach (var duplicate in duplicates)
        {
            yield return new ValidationResult(
                $"{propertyName}: Duplicate revision number {duplicate.Key} found. Revision numbers must be unique.",
                [propertyName]);
        }
    }

    #endregion Collection and Document-Wide Validation

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

    /// <summary>
    /// Quick validation check for revision activity type (optimized for performance).
    /// </summary>
    /// <param name="activityType">The activity type to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidRevisionActivityType([NotNullWhen(true)] string? activityType)
    {
        return !string.IsNullOrWhiteSpace(activityType) &&
               _allowedRevisionActivitiesSet.Contains(activityType.Trim());
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
            ["MaxReasonableActivityCount"] = MaxReasonableActivityCount,
            ["MaxProfessionalRevisionNumber"] = MaxProfessionalRevisionNumber,
            ["AllowedRevisionActivities"] = AllowedRevisionActivities.ToArray(),
            ["ValidationRules"] = new Dictionary<string, string>
            {
                ["RevisionNumbers"] = $"Sequential from {MinRevisionNumber} to {MaxRevisionNumber}",
                ["DateRange"] = $"Between {MinAllowedRevisionDate:yyyy-MM-dd} and present (+{FutureDateToleranceMinutes}min tolerance)",
                ["SequentialRule"] = "Revision numbers must be sequential without gaps within documents",
                ["TemporalRule"] = "Modification date must be >= creation date",
                ["AgeLimit"] = $"Maximum reasonable age: {MaxReasonableAgeYears} years",
                ["ActivityTypes"] = string.Join(", ", AllowedRevisionActivities),
                ["AuditTrailRule"] = "All revisions must have complete audit trail for legal compliance",
                ["ProfessionalStandards"] = $"Maximum professional revision count: {MaxProfessionalRevisionNumber}"
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
        foreach (var result in ValidateProfessionalStandards(
            revisionNumber, creationDate, timeSpan, 0, propertyPrefix))
            yield return result;

        // Check for deleted revision consistency
        if (isDeleted)
        {
            // Deleted revisions should maintain audit trail integrity
            // Additional business rules for deleted revisions can be added here
        }
    }

    #endregion Business Rule Validation

    #region Private Helper Methods

    /// <summary>
    /// Validates if an activity sequence is professionally appropriate.
    /// </summary>
    /// <param name="previousActivity">The previous activity type.</param>
    /// <param name="currentActivity">The current activity type.</param>
    /// <returns>True if valid sequence; otherwise, false.</returns>
    private static bool IsValidActivitySequence(string? previousActivity, string? currentActivity)
    {
        if (string.IsNullOrWhiteSpace(previousActivity) || string.IsNullOrWhiteSpace(currentActivity))
            return true; // Can't validate without both activities

        var prev = previousActivity.Trim().ToUpperInvariant();
        var curr = currentActivity.Trim().ToUpperInvariant();

        // Define professional activity sequences
        // Only return true for known valid transitions, otherwise false
        return (prev, curr) switch
        {
            ("CREATED", "SAVED") => true,
            ("SAVED", "SAVED") => true,
            ("CREATED", "DELETED") => true,
            ("SAVED", "DELETED") => true,
            ("DELETED", "RESTORED") => true,
            ("RESTORED", "SAVED") => true,
            ("RESTORED", "DELETED") => true,
            _ => false // All other transitions are considered invalid
        };
    }

    #endregion Private Helper Methods
}