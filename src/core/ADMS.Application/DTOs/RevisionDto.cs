using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using ADMS.Application.Common.Validation;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Document Revision Data Transfer Object with streamlined validation and professional version control capabilities.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of a document revision within the ADMS legal document management system,
/// using the standardized BaseValidationDto validation framework for consistency with other DTOs.
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item><strong>Streamlined Validation:</strong> Uses BaseValidationDto for consistent validation patterns</item>
/// <item><strong>Professional Version Control:</strong> Sequential numbering and temporal consistency</item>
/// <item><strong>Audit Trail Integration:</strong> Complete activity tracking for legal compliance</item>
/// <item><strong>Document Association:</strong> Optional parent document context for cross-document scenarios</item>
/// <item><strong>Legal Compliance:</strong> Comprehensive audit trail support for legal requirements</item>
/// </list>
/// 
/// <para><strong>Validation Hierarchy:</strong></para>
/// <list type="number">
/// <item><strong>Core Properties:</strong> Revision number, dates, and GUID validation using helpers</item>
/// <item><strong>Business Rules:</strong> Version control consistency, professional standards, and audit requirements</item>
/// <item><strong>Cross-Property:</strong> Date sequence validation and document association consistency</item>
/// <item><strong>Collections:</strong> Deep validation of audit trail collections with detailed error contexts</item>
/// </list>
/// </remarks>
public sealed class RevisionDto : BaseValidationDto, IEquatable<RevisionDto>, IComparable<RevisionDto>
{
    #region Core Properties

    /// <summary>
    /// Gets the unique identifier for this revision within the ADMS legal document management system.
    /// </summary>
    [Required(ErrorMessage = "Revision ID is required for system identification and operations.")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the sequential revision number representing the document version.
    /// </summary>
    [Required(ErrorMessage = "Revision number is required for version control.")]
    [Range(RevisionValidationHelper.MinRevisionNumber, RevisionValidationHelper.MaxRevisionNumber,
        ErrorMessage = "Revision number must be between {1} and {2} for version control consistency.")]
    public required int RevisionNumber { get; init; }

    /// <summary>
    /// Gets the creation date for the revision (stored in UTC).
    /// </summary>
    [Required(ErrorMessage = "Creation date is required for temporal tracking and audit compliance.")]
    public required DateTime CreationDate { get; init; }

    /// <summary>
    /// Gets the modification date for the revision (stored in UTC).
    /// </summary>
    [Required(ErrorMessage = "Modification date is required for change tracking and audit compliance.")]
    public required DateTime ModificationDate { get; init; }

    /// <summary>
    /// Gets the document ID associated with this revision for cross-document scenarios.
    /// </summary>
    public Guid? DocumentId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the revision has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; init; }

    #endregion Core Properties

    #region Navigation Properties

    /// <summary>
    /// Gets the collection of revision activity user associations for this revision.
    /// </summary>
    /// <remarks>
    /// Contains audit trail entries for all activities performed on this revision,
    /// including CREATED, SAVED, DELETED, and RESTORED operations with user attribution.
    /// </remarks>
    public ICollection<RevisionActivityUserDto> RevisionActivityUsers { get; init; } = [];

    #endregion Navigation Properties

    #region Computed Properties

    /// <summary>
    /// Gets the creation date formatted for local display.
    /// </summary>
    public string LocalCreationDateString => CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets the modification date formatted for local display.
    /// </summary>
    public string LocalModificationDateString => ModificationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets the time span between creation and modification dates.
    /// </summary>
    public TimeSpan ModificationTimeSpan => ModificationDate - CreationDate;

    /// <summary>
    /// Gets a value indicating whether this revision has recorded activities.
    /// </summary>
    public bool HasActivities => RevisionActivityUsers.Count > 0;

    /// <summary>
    /// Gets the total count of activities recorded for this revision.
    /// </summary>
    public int ActivityCount => RevisionActivityUsers.Count;

    /// <summary>
    /// Gets a value indicating whether this revision DTO has valid data using quick validation.
    /// </summary>
    public bool IsValid =>
        RevisionValidationHelper.IsValidRevisionId(Id) &&
        RevisionValidationHelper.IsValidRevisionNumber(RevisionNumber) &&
        RevisionValidationHelper.IsValidDate(CreationDate) &&
        RevisionValidationHelper.IsValidDate(ModificationDate) &&
        RevisionValidationHelper.IsValidDateSequence(CreationDate, ModificationDate) &&
        (DocumentId == null || RevisionValidationHelper.IsValidDocumentId(DocumentId.Value));

    /// <summary>
    /// Gets the display text for UI controls and revision identification.
    /// </summary>
    public string DisplayText => $"Revision {RevisionNumber} (Created: {LocalCreationDateString})";

    /// <summary>
    /// Gets compact display text for space-limited UI scenarios.
    /// </summary>
    public string CompactDisplayText => $"Rev. {RevisionNumber}";

    /// <summary>
    /// Gets the age of this revision since creation.
    /// </summary>
    public TimeSpan RevisionAge => DateTime.UtcNow - CreationDate;

    #endregion Computed Properties

    #region BaseValidationDto Implementation

    /// <summary>
    /// Validates core revision properties using ADMS validation helpers.
    /// </summary>
    protected override IEnumerable<ValidationResult> ValidateCoreProperties()
    {
        // Validate revision ID
        foreach (var result in ValidateGuid(Id, nameof(Id)))
            yield return result;

        // Validate revision number using RevisionValidationHelper
        foreach (var result in RevisionValidationHelper.ValidateRevisionNumber(RevisionNumber, nameof(RevisionNumber)))
            yield return result;

        // Validate creation date using RevisionValidationHelper
        foreach (var result in RevisionValidationHelper.ValidateDate(CreationDate, nameof(CreationDate)))
            yield return result;

        // Validate modification date using RevisionValidationHelper
        foreach (var result in RevisionValidationHelper.ValidateDate(ModificationDate, nameof(ModificationDate)))
            yield return result;

        // Validate document ID if provided
        if (!DocumentId.HasValue) yield break;
        foreach (var result in RevisionValidationHelper.ValidateDocumentId(DocumentId.Value, nameof(DocumentId)))
            yield return result;
    }

    /// <summary>
    /// Validates revision business rules and professional standards.
    /// </summary>
    protected override IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Comprehensive business rule validation
        foreach (var result in RevisionValidationHelper.ValidateRevisionBusinessRules(
            RevisionNumber, CreationDate, ModificationDate, DocumentId, IsDeleted))
            yield return result;

        switch (RevisionNumber)
        {
            // Professional standards validation
            // 24 hours
            case 1 when ModificationTimeSpan.TotalMinutes > 1440:
                yield return CreateValidationResult(
                    "First revision should typically be created and modified within a reasonable timeframe.",
                    nameof(CreationDate), nameof(ModificationDate));
                break;
        }

        switch (HasActivities)
        {
            // Audit trail completeness validation
            case false when !IsDeleted:
                yield return CreateValidationResult(
                    "Active revisions should have at least one recorded activity for audit trail compliance.",
                    nameof(RevisionActivityUsers));
                break;
        }

        switch (IsDeleted)
        {
            // Version control consistency
            case true when HasActivities:
            {
                var hasRestoreActivity = RevisionActivityUsers.Any(a => 
                    string.Equals(a.RevisionActivity?.Activity, "RESTORED", StringComparison.OrdinalIgnoreCase));
            
                switch (hasRestoreActivity)
                {
                    case false:
                        // Note: This is informational - deleted revisions might not always have restore activities
                        // but it's good to track for audit trail completeness
                        break;
                }

                break;
            }
        }
    }

    /// <summary>
    /// Validates cross-property relationships and constraints.
    /// </summary>
    protected override IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // Date sequence validation using RevisionValidationHelper
        foreach (var result in RevisionValidationHelper.ValidateDateSequence(
            CreationDate, ModificationDate, nameof(CreationDate), nameof(ModificationDate)))
            yield return result;

        // Activity timeline consistency
        if (RevisionActivityUsers.Count > 0)
        {
            var earliestActivity = RevisionActivityUsers.Min(a => a.CreatedAt);
            var latestActivity = RevisionActivityUsers.Max(a => a.CreatedAt);

            if (earliestActivity < CreationDate)
            {
                yield return CreateValidationResult(
                    "Revision activities cannot occur before the revision creation date.",
                    nameof(RevisionActivityUsers), nameof(CreationDate));
            }

            if (latestActivity > ModificationDate.AddMinutes(RevisionValidationHelper.FutureDateToleranceMinutes))
            {
                yield return CreateValidationResult(
                    "Revision activities should not occur significantly after the modification date.",
                    nameof(RevisionActivityUsers), nameof(ModificationDate));
            }
        }

        // Document association consistency
        if (!DocumentId.HasValue || RevisionActivityUsers.Count <= 0) yield break;
        var inconsistentActivities = RevisionActivityUsers.Where(a => 
            a.Revision is { DocumentId: not null } && 
            a.Revision.DocumentId != DocumentId).ToList();

        if (inconsistentActivities.Count > 0)
        {
            yield return CreateValidationResult(
                "All revision activities must be associated with the same document.",
                nameof(RevisionActivityUsers), nameof(DocumentId));
        }
    }

    /// <summary>
    /// Validates collections and nested objects.
    /// </summary>
    protected override IEnumerable<ValidationResult> ValidateCollections()
    {
        // Validate revision activity users collection
        if (RevisionActivityUsers.Count == 0) yield break;
        var index = 0;
        foreach (var activity in RevisionActivityUsers)
        {
            switch (activity)
            {
                case null:
                    yield return CreateContextualValidationResult(
                        "Revision activity user entry cannot be null.",
                        $"{nameof(RevisionActivityUsers)}[{index}]");
                    break;
                default:
                {
                    // Validate that all activities reference this revision
                    if (activity.RevisionId != Id)
                    {
                        yield return CreateContextualValidationResult(
                            "Activity must be associated with this revision.",
                            $"{nameof(RevisionActivityUsers)}[{index}].{nameof(activity.RevisionId)}");
                    }

                    // Validate activity type appropriateness
                    if (activity.RevisionActivity != null)
                    {
                        var activityType = activity.RevisionActivity.Activity;
                        var isAppropriate = ValidateActivityTypeForRevisionState(activityType, IsDeleted);
                        
                        switch (isAppropriate)
                        {
                            case false:
                                yield return CreateContextualValidationResult(
                                    $"Activity type '{activityType}' is not appropriate for current revision state.",
                                    $"{nameof(RevisionActivityUsers)}[{index}].{nameof(activity.RevisionActivity)}.{nameof(activity.RevisionActivity.Activity)}");
                                break;
                        }
                    }

                    break;
                }
            }

            index++;
        }

        switch (RevisionActivityUsers.Count)
        {
            // Validate activity sequence logic
            case > 1:
            {
                var activitiesByTime = RevisionActivityUsers
                    .Where(a => a.RevisionActivity != null)
                    .OrderBy(a => a.CreatedAt)
                    .ToList();

                for (var i = 1; i < activitiesByTime.Count; i++)
                {
                    var previousActivity = activitiesByTime[i - 1].RevisionActivity?.Activity;
                    var currentActivity = activitiesByTime[i].RevisionActivity?.Activity;

                    if (!IsValidActivitySequence(previousActivity, currentActivity))
                    {
                        yield return CreateValidationResult(
                            $"Activity sequence from '{previousActivity}' to '{currentActivity}' may not be valid for professional version control.",
                            nameof(RevisionActivityUsers));
                    }
                }

                break;
            }
        }
    }

    /// <summary>
    /// Validates custom revision-specific rules.
    /// </summary>
    protected override IEnumerable<ValidationResult> ValidateCustomRules()
    {
        switch (RevisionNumber)
        {
            // Professional practice validation
            case > 100:
                yield return CreateValidationResult(
                    "Revision has unusually high version number (>100). Consider document consolidation for professional practice.",
                    nameof(RevisionNumber));
                break;
        }

        switch (HasActivities)
        {
            // Audit trail completeness for legal compliance
            case true:
            {
                var hasCreationActivity = RevisionActivityUsers.Any(a => 
                    string.Equals(a.RevisionActivity?.Activity, "CREATED", StringComparison.OrdinalIgnoreCase));

                switch (hasCreationActivity)
                {
                    case false when RevisionNumber == 1:
                        yield return CreateValidationResult(
                            "First revision should have a CREATED activity for complete audit trail.",
                            nameof(RevisionActivityUsers));
                        break;
                }

                break;
            }
        }

        switch (ModificationTimeSpan.TotalDays)
        {
            // Temporal consistency validation
            // Modified more than a year after creation
            case > 365:
                yield return CreateValidationResult(
                    "Revision has been in development for over a year. Verify this is correct for professional practice.",
                    nameof(CreationDate), nameof(ModificationDate));
                break;
        }

        switch (ActivityCount)
        {
            // Activity density validation (prevent audit trail spam)
            case > 50:
                yield return CreateValidationResult(
                    "Revision has unusually high activity count (>50). Verify audit trail integrity.",
                    nameof(RevisionActivityUsers));
                break;
        }
    }

    #endregion BaseValidationDto Implementation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this revision can be deleted based on business rules.
    /// </summary>
    public bool CanBeDeleted() => !IsDeleted && IsValid;

    /// <summary>
    /// Determines whether this revision can be restored from deleted state.
    /// </summary>
    public bool CanBeRestored() => IsDeleted && IsValid;

    /// <summary>
    /// Determines whether the revision represents a legal document version.
    /// </summary>
    public bool IsLegalDocumentRevision() => RevisionNumber >= 1 && HasActivities;

    /// <summary>
    /// Gets activities of a specific type performed on this revision.
    /// </summary>
    public IEnumerable<RevisionActivityUserDto> GetActivitiesByType([NotNull] string activityType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activityType);

        return RevisionActivityUsers?
            .Where(a => string.Equals(a.RevisionActivity?.Activity, activityType, StringComparison.OrdinalIgnoreCase))
            .OrderBy(a => a.CreatedAt) ?? Enumerable.Empty<RevisionActivityUserDto>();
    }

    /// <summary>
    /// Gets the most recent activity performed on this revision.
    /// </summary>
    public RevisionActivityUserDto? GetMostRecentActivity()
    {
        return RevisionActivityUsers?
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets comprehensive revision metrics for reporting and analysis.
    /// </summary>
    public object GetRevisionMetrics() => new
    {
        VersionInfo = new { RevisionNumber, IsDeleted, RevisionAge.TotalDays },
        ActivityInfo = new { ActivityCount, HasActivities },
        TemporalInfo = new { CreationDate, ModificationDate, ModificationTimeSpan.TotalMinutes },
        ValidationInfo = new { IsValid }
    };

    #endregion Business Logic Methods

    #region Static Factory Methods

    /// <summary>
    /// Creates a RevisionDto from a Domain entity with enhanced validation.
    /// </summary>
    public static RevisionDto FromEntity(
        [NotNull] Domain.Entities.Revision entity,
        bool includeDocumentId = false,
        bool includeActivities = true)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new RevisionDto
        {
            Id = entity.Id,
            RevisionNumber = entity.RevisionNumber,
            CreationDate = entity.CreationDate,
            ModificationDate = entity.ModificationDate,
            DocumentId = includeDocumentId ? entity.DocumentId : null,
            IsDeleted = entity.IsDeleted,
            RevisionActivityUsers = includeActivities && entity.RevisionActivityUsers?.Count > 0
                ? entity.RevisionActivityUsers.Select(ra => RevisionActivityUserDto.FromEntity(ra)).ToList()
                : []
        };

        var validationResults = ValidateModel(dto);
        if (!HasValidationErrors(validationResults)) return dto;
        var summary = GetValidationSummary(validationResults);
        var entityInfo = $"Rev. {entity.RevisionNumber}";
        throw new ValidationException($"RevisionDto creation failed for '{entityInfo}' ({entity.Id}): {summary}");
    }

    #endregion Static Factory Methods

    #region Private Helper Methods

    /// <summary>
    /// Validates if an activity type is appropriate for the current revision state.
    /// </summary>
    private static bool ValidateActivityTypeForRevisionState(string? activityType, bool isDeleted)
    {
        if (string.IsNullOrWhiteSpace(activityType))
            return false;

        var normalizedActivity = activityType.Trim().ToUpperInvariant();

        return normalizedActivity switch
        {
            "RESTORED" => isDeleted, // Can only restore deleted revisions
            "DELETED" => !isDeleted, // Can only delete non-deleted revisions
            "CREATED" or "SAVED" => !isDeleted, // Can only create/save non-deleted revisions
            _ => true // Other activities are generally valid
        };
    }

    /// <summary>
    /// Validates if an activity sequence is professionally appropriate.
    /// </summary>
    private static bool IsValidActivitySequence(string? previousActivity, string? currentActivity)
    {
        if (string.IsNullOrWhiteSpace(previousActivity) || string.IsNullOrWhiteSpace(currentActivity))
            return true; // Can't validate without both activities

        var prev = previousActivity.Trim().ToUpperInvariant();
        var curr = currentActivity.Trim().ToUpperInvariant();

        // Define professional activity sequences
        return (prev == "CREATED" && curr == "SAVED") ||
               (prev == "SAVED" && curr == "SAVED") ||
               (prev == "CREATED" && curr == "DELETED") ||
               (prev == "SAVED" && curr == "DELETED") ||
               (prev == "DELETED" && curr == "RESTORED") ||
               (prev == "RESTORED" && curr == "SAVED") ||
               (prev == "RESTORED" && curr == "DELETED");
        // For other sequences, could log for review, but do not always return true
    }

    #endregion Private Helper Methods

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified RevisionDto is equal to the current instance.
    /// </summary>
    public bool Equals(RevisionDto? other)
    {
        switch (other)
        {
            case null:
                return false;
        }
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current RevisionDto.
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as RevisionDto);

    /// <summary>
    /// Returns a hash code for the current RevisionDto.
    /// </summary>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Compares the current RevisionDto with another RevisionDto for ordering.
    /// </summary>
    public int CompareTo(RevisionDto? other)
    {
        switch (other)
        {
            case null:
                return 1;
        }
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by revision number
        var revisionComparison = RevisionNumber.CompareTo(other.RevisionNumber);
        if (revisionComparison != 0) return revisionComparison;

        // Secondary sort by creation date
        var dateComparison = CreationDate.CompareTo(other.CreationDate);
        return dateComparison != 0 ? dateComparison : Id.CompareTo(other.Id);
    }

    /// <summary>
    /// Returns a string representation of the revision.
    /// </summary>
    public override string ToString() =>
        $"Revision {RevisionNumber} ({Id}) - Created: {CreationDate:yyyy-MM-dd HH:mm:ss} UTC";

    /// <summary>
    /// Determines whether two <see cref="RevisionDto"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="RevisionDto"/> instance to compare, or <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="RevisionDto"/> instance to compare, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the specified <see cref="RevisionDto"/> instances are equal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator ==(RevisionDto? left, RevisionDto? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="RevisionDto"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="RevisionDto"/> instance to compare, or <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="RevisionDto"/> instance to compare, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the specified <see cref="RevisionDto"/> instances are not equal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator !=(RevisionDto? left, RevisionDto? right) => !(left == right);

    /// <summary>
    /// Determines whether one <see cref="RevisionDto"/> instance is less than another.
    /// </summary>
    /// <remarks>If <paramref name="left"/> is <see langword="null"/> and <paramref name="right"/> is not <see
    /// langword="null"/>, the operator returns <see langword="true"/>. If both are <see langword="null"/>, the operator
    /// returns <see langword="false"/>.</remarks>
    /// <param name="left">The first <see cref="RevisionDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="RevisionDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator <(RevisionDto? left, RevisionDto? right)
    {
        if (left is null) return right is not null;
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    /// Determines whether one <see cref="RevisionDto"/> instance is less than or equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="RevisionDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="RevisionDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise,
    /// <see langword="false"/>. If <paramref name="left"/> is <see langword="null"/>, the result is always <see
    /// langword="true"/>.</returns>
    public static bool operator <=(RevisionDto? left, RevisionDto? right)
    {
        if (left is null) return true;
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// Determines whether one <see cref="RevisionDto"/> instance is greater than another.
    /// </summary>
    /// <param name="left">The first <see cref="RevisionDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="RevisionDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see
    /// langword="false"/>. If <paramref name="left"/> is <see langword="null"/>, the result is always <see
    /// langword="false"/>.</returns>
    public static bool operator >(RevisionDto? left, RevisionDto? right)
    {
        if (left is null) return false;
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    /// Determines whether the left <see cref="RevisionDto"/> is greater than or equal to the right <see
    /// cref="RevisionDto"/>.
    /// </summary>
    /// <param name="left">The first <see cref="RevisionDto"/> to compare. Can be <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="RevisionDto"/> to compare. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; 
    /// otherwise, <see langword="false"/>. If <paramref name="right"/> is <see langword="null"/>, the result is always
    /// <see langword="true"/>.</returns>
    public static bool operator >=(RevisionDto? left, RevisionDto? right)
    {
        if (right is null) return true;
        return left?.CompareTo(right) >= 0;
    }

    #endregion Equality and Comparison
}