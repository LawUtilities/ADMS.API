using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using ADMS.Application.Common.Validation;

namespace ADMS.Application.DTOs;

/// <summary>
/// Revision Activity User DTO for audit trail tracking.
/// </summary>
/// <remarks>
/// Represents the association between a revision, revision activity, and user for audit trail purposes.
/// Links revision operations to users for accountability and compliance tracking.
/// </remarks>
public sealed record RevisionActivityUserDto : IValidatableObject, IEquatable<RevisionActivityUserDto>
{
    #region Core Properties

    /// <summary>
    /// Gets the revision identifier.
    /// </summary>
    [Required(ErrorMessage = "Revision ID is required.")]
    public required Guid RevisionId { get; init; }

    /// <summary>
    /// Gets the revision activity identifier.
    /// </summary>
    [Required(ErrorMessage = "Revision activity ID is required.")]
    public required Guid RevisionActivityId { get; init; }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    [Required(ErrorMessage = "User ID is required.")]
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the activity timestamp.
    /// </summary>
    [Required(ErrorMessage = "Activity timestamp is required.")]
    public required DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    #endregion Core Properties

    #region Navigation Properties

    /// <summary>
    /// Gets the revision details.
    /// </summary>
    public RevisionDto? Revision { get; init; }

    /// <summary>
    /// Gets the revision activity details.
    /// </summary>
    public RevisionActivityDto? RevisionActivity { get; init; }

    /// <summary>
    /// Gets the user details.
    /// </summary>
    public UserDto? User { get; init; }

    #endregion Navigation Properties

    #region Computed Properties

    /// <summary>
    /// Gets the creation timestamp formatted for display.
    /// </summary>
    public string LocalCreatedAtDateString => CreatedAt.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets a summary of the revision activity for display.
    /// </summary>
    public string ActivitySummary =>
        $"{Revision?.DisplayText ?? "Revision"} " +
        $"{RevisionActivity?.Activity ?? "ACTIVITY"} " +
        $"by {User?.Name ?? "User"}";

    /// <summary>
    /// Gets a value indicating whether this DTO has valid core data.
    /// </summary>
    public bool IsValid =>
        RevisionId != Guid.Empty &&
        RevisionActivityId != Guid.Empty &&
        UserId != Guid.Empty &&
        CreatedAt != default;

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the RevisionActivityUserDto for data integrity and business rules compliance.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate core GUIDs using consistent helper
        foreach (var result in UserValidationHelper.ValidateUserId(RevisionId, nameof(RevisionId)))
            yield return result;

        foreach (var result in UserValidationHelper.ValidateUserId(RevisionActivityId, nameof(RevisionActivityId)))
            yield return result;

        foreach (var result in UserValidationHelper.ValidateUserId(UserId, nameof(UserId)))
            yield return result;

        // Validate timestamp
        foreach (var result in UserValidationHelper.ValidateActivityTimestamp(CreatedAt, nameof(CreatedAt)))
            yield return result;

        // Validate navigation property consistency
        if (Revision != null && Revision.Id != RevisionId)
        {
            yield return new ValidationResult(
                "Revision navigation property ID must match RevisionId.",
                [nameof(Revision), nameof(RevisionId)]);
        }

        if (RevisionActivity != null && RevisionActivity.Id != RevisionActivityId)
        {
            yield return new ValidationResult(
                "RevisionActivity navigation property ID must match RevisionActivityId.",
                [nameof(RevisionActivity), nameof(RevisionActivityId)]);
        }

        if (User != null && User.Id != UserId)
        {
            yield return new ValidationResult(
                "User navigation property ID must match UserId.",
                [nameof(User), nameof(UserId)]);
        }

        // Validate user context if User navigation property is loaded
        if (User != null)
        {
            foreach (var result in UserValidationHelper.ValidateUsername(User.Name, $"{nameof(User)}.{nameof(User.Name)}"))
                yield return result;
        }

        // Validate revision activity type if RevisionActivity is loaded
        if (RevisionActivity != null)
        {
            foreach (var result in RevisionActivityValidationHelper.ValidateActivity(RevisionActivity.Activity, $"{nameof(RevisionActivity)}.{nameof(RevisionActivity.Activity)}"))
                yield return result;
        }
    }

    #endregion Validation Implementation

    #region Static Factory Methods

    /// <summary>
    /// Validates a RevisionActivityUserDto instance and returns validation results.
    /// </summary>
    public static IList<ValidationResult> ValidateModel([AllowNull] RevisionActivityUserDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("RevisionActivityUserDto instance is required."));
            return results;
        }

        var context = new ValidationContext(dto);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a RevisionActivityUserDto from a Domain entity.
    /// </summary>
    public static RevisionActivityUserDto FromEntity([NotNull] Domain.Entities.RevisionActivityUser entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new RevisionActivityUserDto
        {
            RevisionId = entity.RevisionId,
            RevisionActivityId = entity.RevisionActivityId,
            UserId = entity.UserId,
            CreatedAt = entity.CreatedAt
        };

        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;

        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid RevisionActivityUserDto: {errorMessages}");
    }

    #endregion Static Factory Methods

    #region Business Methods

    /// <summary>
    /// Determines whether this activity represents a revision creation operation.
    /// </summary>
    public bool IsCreationOperation() =>
        string.Equals(RevisionActivity?.Activity, "CREATED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a revision modification operation.
    /// </summary>
    public bool IsModificationOperation() =>
        string.Equals(RevisionActivity?.Activity, "SAVED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a revision deletion operation.
    /// </summary>
    public bool IsDeletionOperation() =>
        string.Equals(RevisionActivity?.Activity, "DELETED", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(RevisionActivity?.Activity, "RESTORED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the age of this activity in days.
    /// </summary>
    public double GetActivityAgeDays() => (DateTime.UtcNow - CreatedAt).TotalDays;

    /// <summary>
    /// Determines whether this activity occurred recently.
    /// </summary>
    public bool IsRecentActivity(int withinDays = 7) => GetActivityAgeDays() <= withinDays;

    #endregion Business Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified RevisionActivityUserDto is equal to the current instance.
    /// </summary>
    public bool Equals(RevisionActivityUserDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return RevisionId.Equals(other.RevisionId) &&
               RevisionActivityId.Equals(other.RevisionActivityId) &&
               UserId.Equals(other.UserId) &&
               CreatedAt.Equals(other.CreatedAt);
    }

    /// <summary>
    /// Returns a hash code for the current object based on its key properties.
    /// </summary>
    public override int GetHashCode() =>
        HashCode.Combine(RevisionId, RevisionActivityId, UserId, CreatedAt);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the revision activity.
    /// </summary>
    public override string ToString() =>
        $"Revision Activity: {RevisionActivity?.Activity ?? "ACTIVITY"} on Revision ({RevisionId}) by User ({UserId}) at {CreatedAt:yyyy-MM-dd HH:mm:ss}";

    #endregion String Representation
}