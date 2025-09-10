using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using ADMS.Application.Common.Validation;

namespace ADMS.Application.DTOs;

/// <summary>
/// Matter Activity User DTO for audit trail tracking.
/// </summary>
/// <remarks>
/// Represents the association between a matter, matter activity, and user for audit trail purposes.
/// Links matter operations to users for accountability and compliance tracking.
/// </remarks>
public sealed record MatterActivityUserDto : IValidatableObject, IEquatable<MatterActivityUserDto>
{
    #region Core Properties

    /// <summary>
    /// Gets the matter identifier.
    /// </summary>
    [Required(ErrorMessage = "Matter ID is required.")]
    public required Guid MatterId { get; init; }

    /// <summary>
    /// Gets the matter activity identifier.
    /// </summary>
    [Required(ErrorMessage = "Matter activity ID is required.")]
    public required Guid MatterActivityId { get; init; }

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
    /// Gets the matter details.
    /// </summary>
    public MatterWithoutDocumentsDto? Matter { get; init; }

    /// <summary>
    /// Gets the matter activity details.
    /// </summary>
    public MatterActivityDto? MatterActivity { get; init; }

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
    /// Gets a summary of the matter activity for display.
    /// </summary>
    public string ActivitySummary =>
        $"{Matter?.Description ?? "Matter"} " +
        $"{MatterActivity?.Activity ?? "ACTIVITY"} " +
        $"by {User?.Name ?? "User"}";

    /// <summary>
    /// Gets comprehensive activity metrics for analysis and reporting.
    /// </summary>
    public object ActivityMetrics => new
    {
        ActivityInfo = new
        {
            ActivitySummary,
            LocalCreatedAtDateString,
            OperationType = MatterActivity?.Activity ?? "UNKNOWN",
            ActivityContext = "Matter Activity"
        },
        ParticipantInfo = new
        {
            MatterContext = Matter?.Description ?? "Unknown Matter",
            UserName = User?.Name ?? "Unknown User",
            UserId,
            MatterId
        },
        TemporalInfo = new
        {
            CreatedAt,
            LocalCreatedAtDateString,
            ActivityAge = (DateTime.UtcNow - CreatedAt).TotalDays
        },
        ValidationInfo = new
        {
            HasCompleteInformation = Matter != null && MatterActivity != null && User != null,
            RequiredFieldsPresent = MatterId != Guid.Empty && MatterActivityId != Guid.Empty && UserId != Guid.Empty
        }
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the MatterActivityUserDto for data integrity and business rules compliance.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate core GUIDs
        foreach (var result in UserValidationHelper.ValidateUserId(MatterId, nameof(MatterId)))
            yield return result;

        foreach (var result in UserValidationHelper.ValidateUserId(MatterActivityId, nameof(MatterActivityId)))
            yield return result;

        foreach (var result in UserValidationHelper.ValidateUserId(UserId, nameof(UserId)))
            yield return result;

        // Validate timestamp
        foreach (var result in UserValidationHelper.ValidateActivityTimestamp(CreatedAt, nameof(CreatedAt)))
            yield return result;

        // Validate navigation property consistency
        if (Matter != null && Matter.Id != MatterId)
        {
            yield return new ValidationResult(
                "Matter navigation property ID must match MatterId.",
                [nameof(Matter), nameof(MatterId)]);
        }

        if (MatterActivity != null && MatterActivity.Id != MatterActivityId)
        {
            yield return new ValidationResult(
                "MatterActivity navigation property ID must match MatterActivityId.",
                [nameof(MatterActivity), nameof(MatterActivityId)]);
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

        // Validate matter activity type if MatterActivity is loaded
        if (MatterActivity == null) yield break;
        foreach (var result in MatterActivityValidationHelper.ValidateActivity(MatterActivity.Activity, $"{nameof(MatterActivity)}.{nameof(MatterActivity.Activity)}"))
            yield return result;
    }

    #endregion Validation Implementation

    #region Static Factory Methods

    /// <summary>
    /// Validates a MatterActivityUserDto instance and returns validation results.
    /// </summary>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterActivityUserDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterActivityUserDto instance is required."));
            return results;
        }

        var context = new ValidationContext(dto);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a MatterActivityUserDto from a Domain entity.
    /// </summary>
    public static MatterActivityUserDto FromEntity([NotNull] Domain.Entities.MatterActivityUser entity, bool includeNavigationProperties = false)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new MatterActivityUserDto
        {
            MatterId = entity.MatterId,
            MatterActivityId = entity.MatterActivityId,
            UserId = entity.UserId,
            CreatedAt = entity.CreatedAt
        };

        // Navigation properties can be set separately if needed for performance
        // dto.Matter = entity.Matter != null ? MatterWithoutDocumentsDto.FromEntity(entity.Matter) : null;
        // dto.MatterActivity = entity.MatterActivity != null ? MatterActivityDto.FromEntity(entity.MatterActivity) : null;
        // dto.User = entity.User != null ? UserDto.FromEntity(entity.User) : null;

        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;

        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterActivityUserDto: {errorMessages}");
    }

    /// <summary>
    /// Creates multiple MatterActivityUserDto instances from a collection of entities.
    /// </summary>
    public static IList<MatterActivityUserDto> FromEntities([NotNull] IEnumerable<Domain.Entities.MatterActivityUser> entities, bool includeNavigationProperties = false)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var result = new List<MatterActivityUserDto>();

        foreach (var entity in entities)
        {
            try
            {
                var dto = FromEntity(entity, includeNavigationProperties);
                result.Add(dto);
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                // Log invalid entity but continue processing others
                Console.WriteLine($"Warning: Skipped invalid matter activity entity: {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a matter activity audit entry with specified parameters.
    /// </summary>
    public static MatterActivityUserDto CreateActivityAudit(
        Guid matterId,
        Guid activityId,
        Guid userId,
        DateTime? timestamp = null)
    {
        if (matterId == Guid.Empty)
            throw new ArgumentException("Matter ID cannot be empty.", nameof(matterId));
        if (activityId == Guid.Empty)
            throw new ArgumentException("Activity ID cannot be empty.", nameof(activityId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        var dto = new MatterActivityUserDto
        {
            MatterId = matterId,
            MatterActivityId = activityId,
            UserId = userId,
            CreatedAt = timestamp ?? DateTime.UtcNow
        };

        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;

        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid activity audit entry: {errorMessages}");
    }

    #endregion Static Factory Methods

    #region Business Methods

    /// <summary>
    /// Determines whether this activity represents a matter creation operation.
    /// </summary>
    public bool IsCreationOperation() =>
        string.Equals(MatterActivity?.Activity, "CREATED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this activity represents a matter lifecycle operation.
    /// </summary>
    public bool IsLifecycleOperation()
    {
        var activityType = MatterActivity?.Activity;
        return string.Equals(activityType, "ARCHIVED", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(activityType, "DELETED", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(activityType, "RESTORED", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(activityType, "UNARCHIVED", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether this activity represents a viewing operation.
    /// </summary>
    public bool IsViewingOperation() =>
        string.Equals(MatterActivity?.Activity, "VIEWED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the age of this activity in days.
    /// </summary>
    public double GetActivityAgeDays() => (DateTime.UtcNow - CreatedAt).TotalDays;

    /// <summary>
    /// Determines whether this activity occurred recently.
    /// </summary>
    public bool IsRecentActivity(int withinDays = 7) => GetActivityAgeDays() <= withinDays;

    /// <summary>
    /// Generates a professional audit trail message.
    /// </summary>
    public string GenerateAuditMessage()
    {
        var operationType = MatterActivity?.Activity ?? "PERFORMED ACTIVITY ON";
        var matterDescription = Matter?.Description ?? "matter";
        var userName = User?.Name ?? "user";

        return $"On {LocalCreatedAtDateString}, {userName} {operationType} {matterDescription}";
    }

    #endregion Business Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified <see cref="MatterActivityUserDto"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="MatterActivityUserDto"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="MatterActivityUserDto"/> is equal to the current instance;
    /// otherwise, <see langword="false"/>.</returns>
    public bool Equals(MatterActivityUserDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return MatterId.Equals(other.MatterId) &&
               MatterActivityId.Equals(other.MatterActivityId) &&
               UserId.Equals(other.UserId) &&
               CreatedAt.Equals(other.CreatedAt);
    }

    /// <summary>
    /// Returns a hash code for the current object.
    /// </summary>
    /// <remarks>The hash code is computed based on the values of the <see cref="MatterId"/>,  <see
    /// cref="MatterActivityId"/>, <see cref="UserId"/>, and <see cref="CreatedAt"/> properties.</remarks>
    /// <returns>A 32-bit signed integer hash code that represents the current object.</returns>
    public override int GetHashCode() =>
        HashCode.Combine(MatterId, MatterActivityId, UserId, CreatedAt);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the matter activity, including the activity type, matter ID, user ID, and
    /// creation timestamp.
    /// </summary>
    /// <remarks>The returned string includes the activity type (or "ACTIVITY" if the activity is null), the
    /// associated matter ID, the user ID, and the creation timestamp formatted as "yyyy-MM-dd HH:mm:ss".</remarks>
    /// <returns>A string that represents the matter activity in the format:  "Matter Activity: {Activity} on Matter ({MatterId})
    /// by User ({UserId}) at {CreatedAt:yyyy-MM-dd HH:mm:ss}".</returns>
    public override string ToString() =>
        $"Matter Activity: {MatterActivity?.Activity ?? "ACTIVITY"} on Matter ({MatterId}) by User ({UserId}) at {CreatedAt:yyyy-MM-dd HH:mm:ss}";

    #endregion String Representation
}