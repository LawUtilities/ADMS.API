using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using ADMS.Application.Common.Validation;

namespace ADMS.Application.DTOs;

/// <summary>
/// Document Activity User DTO for audit trail tracking.
/// </summary>
/// <remarks>
/// Represents the association between a document, document activity, and user for audit trail purposes.
/// Links document operations to users for accountability and compliance tracking.
/// </remarks>
public sealed class DocumentActivityUserDto : IValidatableObject, IEquatable<DocumentActivityUserDto>, IComparable<DocumentActivityUserDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    [Required(ErrorMessage = "Document ID is required.")]
    public required Guid DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the document activity identifier.
    /// </summary>
    [Required(ErrorMessage = "Document activity ID is required.")]
    public required Guid DocumentActivityId { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [Required(ErrorMessage = "User ID is required.")]
    public required Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the activity timestamp.
    /// </summary>
    [Required(ErrorMessage = "Activity timestamp is required.")]
    public required DateTime CreatedAt { get; set; }

    #endregion Core Properties

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the document details.
    /// </summary>
    public DocumentWithoutRevisionsDto? Document { get; set; }

    /// <summary>
    /// Gets or sets the document activity details.
    /// </summary>
    public DocumentActivityDto? DocumentActivity { get; set; }

    /// <summary>
    /// Gets or sets the user details.
    /// </summary>
    public UserDto? User { get; set; }

    #endregion Navigation Properties

    #region Computed Properties

    /// <summary>
    /// Gets a summary of the document activity for display.
    /// </summary>
    public string ActivitySummary =>
        $"{Document?.FileName ?? "Unknown Document"} " +
        $"{DocumentActivity?.Activity ?? "UNKNOWN"} " +
        $"by {User?.Name ?? "Unknown User"}";

    /// <summary>
    /// Gets a value indicating whether this DTO has valid core data.
    /// </summary>
    public bool IsValid =>
        DocumentId != Guid.Empty &&
        DocumentActivityId != Guid.Empty &&
        UserId != Guid.Empty &&
        CreatedAt != default;

    /// <summary>
    /// Gets the activity category for classification.
    /// </summary>
    public string ActivityCategory => DocumentActivity?.Activity.ToUpperInvariant() switch
    {
        "CREATED" or "DELETED" or "RESTORED" => "Lifecycle",
        "CHECKED_IN" or "CHECKED_OUT" => "Version Control",
        "SAVED" => "Content Management",
        _ => "Unknown"
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the DocumentActivityUserDto for data integrity and business rules.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate core GUIDs
        foreach (var result in UserValidationHelper.ValidateUserId(DocumentId, nameof(DocumentId)))
            yield return result;

        foreach (var result in UserValidationHelper.ValidateUserId(DocumentActivityId, nameof(DocumentActivityId)))
            yield return result;

        foreach (var result in UserValidationHelper.ValidateUserId(UserId, nameof(UserId)))
            yield return result;

        // Validate timestamp
        foreach (var result in UserValidationHelper.ValidateActivityTimestamp(CreatedAt, nameof(CreatedAt)))
            yield return result;

        // Validate navigation property consistency
        if (Document != null && Document.Id != DocumentId)
        {
            yield return new ValidationResult(
                "Document navigation property ID must match DocumentId.",
                [nameof(Document), nameof(DocumentId)]);
        }

        if (DocumentActivity != null && DocumentActivity.Id != DocumentActivityId)
        {
            yield return new ValidationResult(
                "DocumentActivity navigation property ID must match DocumentActivityId.",
                [nameof(DocumentActivity), nameof(DocumentActivityId)]);
        }

        if (User != null && User.Id != UserId)
        {
            yield return new ValidationResult(
                "User navigation property ID must match UserId.",
                [nameof(User), nameof(UserId)]);
        }

        // Validate user context if User navigation property is loaded
        if (User == null) yield break;
        foreach (var result in UserValidationHelper.ValidateUsername(User.Name, $"{nameof(User)}.{nameof(User.Name)}"))
            yield return result;
    }

    #endregion Validation Implementation

    #region Static Factory Methods

    /// <summary>
    /// Determines whether two <see cref="DocumentActivityUserDto"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="DocumentActivityUserDto"/> instance to compare, or <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="DocumentActivityUserDto"/> instance to compare, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the specified <see cref="DocumentActivityUserDto"/> instances are equal; otherwise,
    /// <see langword="false"/>.</returns>
    public static bool operator ==(DocumentActivityUserDto? left, DocumentActivityUserDto? right)
        => Equals(left, right);

    /// <summary>
    /// Determines whether two <see cref="DocumentActivityUserDto"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="DocumentActivityUserDto"/> instance to compare, or <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="DocumentActivityUserDto"/> instance to compare, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the specified instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(DocumentActivityUserDto? left, DocumentActivityUserDto? right)
        => !Equals(left, right);

    public static bool operator <(DocumentActivityUserDto? left, DocumentActivityUserDto? right)
        => left is null ? right is not null : left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether the left <see cref="DocumentActivityUserDto"/> instance is less than or equal to the right
    /// instance.
    /// </summary>
    /// <param name="left">The first <see cref="DocumentActivityUserDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="DocumentActivityUserDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <paramref name="left"/> instance is less than or equal to the <paramref
    /// name="right"/> instance; otherwise, <see langword="false"/>. If <paramref name="left"/> is <see
    /// langword="null"/>, the result is always <see langword="true"/>.</returns>
    public static bool operator <=(DocumentActivityUserDto? left, DocumentActivityUserDto? right)
        => left is null || left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether one <see cref="DocumentActivityUserDto"/> instance is greater than another.
    /// </summary>
    /// <param name="left">The first <see cref="DocumentActivityUserDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="DocumentActivityUserDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is not <see langword="null"/> and is greater than <paramref
    /// name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(DocumentActivityUserDto? left, DocumentActivityUserDto? right)
        => left is not null && left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether the left <see cref="DocumentActivityUserDto"/> is greater than or equal to the right <see
    /// cref="DocumentActivityUserDto"/>.
    /// </summary>
    /// <remarks>If both <paramref name="left"/> and <paramref name="right"/> are <see langword="null"/>, the
    /// operator returns <see langword="true"/>. If only one of the operands is <see langword="null"/>, the operator
    /// returns <see langword="false"/>.</remarks>
    /// <param name="left">The first <see cref="DocumentActivityUserDto"/> to compare. Can be <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="DocumentActivityUserDto"/> to compare. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(DocumentActivityUserDto? left, DocumentActivityUserDto? right)
        => left is null ? right is null : left.CompareTo(right) >= 0;

    /// <summary>
    /// Validates a DocumentActivityUserDto instance and returns validation results.
    /// </summary>
    public static IList<ValidationResult> ValidateModel([AllowNull] DocumentActivityUserDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("DocumentActivityUserDto instance is required."));
            return results;
        }

        var context = new ValidationContext(dto);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a DocumentActivityUserDto from a Domain entity.
    /// </summary>
    public static DocumentActivityUserDto FromEntity([NotNull] Domain.Entities.DocumentActivityUser entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new DocumentActivityUserDto
        {
            DocumentId = entity.DocumentId,
            DocumentActivityId = entity.DocumentActivityId,
            UserId = entity.UserId,
            CreatedAt = entity.CreatedAt
        };

        // Navigation properties can be set separately if needed for performance
        // dto.Document = entity.Document != null ? DocumentWithoutRevisionsDto.FromEntity(entity.Document) : null;
        // dto.DocumentActivity = entity.DocumentActivity != null ? DocumentActivityDto.FromEntity(entity.DocumentActivity) : null;
        // dto.User = entity.User != null ? UserDto.FromEntity(entity.User) : null;

        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;

        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid DocumentActivityUserDto: {errorMessages}");
    }

    #endregion Static Factory Methods

    #region Business Methods

    /// <summary>
    /// Gets the seeded GUID for a specific document activity name.
    /// </summary>
    public static Guid GetSeededActivityId(string activityName)
    {
        if (string.IsNullOrWhiteSpace(activityName))
            return Guid.Empty;

        return activityName.Trim().ToUpperInvariant() switch
        {
            "CHECKED IN" => Guid.Parse("20000000-0000-0000-0000-000000000001"),
            "CHECKED OUT" => Guid.Parse("20000000-0000-0000-0000-000000000002"),
            "CREATED" => Guid.Parse("20000000-0000-0000-0000-000000000003"),
            "DELETED" => Guid.Parse("20000000-0000-0000-0000-000000000004"),
            "RESTORED" => Guid.Parse("20000000-0000-0000-0000-000000000005"),
            "SAVED" => Guid.Parse("20000000-0000-0000-0000-000000000006"),
            _ => Guid.Empty
        };
    }

    /// <summary>
    /// Determines whether this represents the same logical operation as another entry.
    /// </summary>
    public bool IsSameOperation(DocumentActivityUserDto? other)
    {
        if (other is null) return false;

        return DocumentId == other.DocumentId &&
               DocumentActivityId == other.DocumentActivityId &&
               UserId == other.UserId;
    }

    /// <summary>
    /// Gets the time elapsed since this activity was performed.
    /// </summary>
    public TimeSpan GetTimeElapsed() => DateTime.UtcNow - CreatedAt;

    #endregion Business Methods

    #region Equality Implementation

    public bool Equals(DocumentActivityUserDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return DocumentId.Equals(other.DocumentId) &&
               DocumentActivityId.Equals(other.DocumentActivityId) &&
               UserId.Equals(other.UserId) &&
               CreatedAt.Equals(other.CreatedAt);
    }

    public override bool Equals(object? obj) => Equals(obj as DocumentActivityUserDto);

    public override int GetHashCode() =>
        HashCode.Combine(DocumentId, DocumentActivityId, UserId, CreatedAt);

    #endregion Equality Implementation

    #region Comparison Implementation

    public int CompareTo(DocumentActivityUserDto? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by timestamp for chronological ordering
        var timestampComparison = CreatedAt.CompareTo(other.CreatedAt);
        return timestampComparison == 0 ? DocumentId.CompareTo(other.DocumentId) : timestampComparison;

        // Secondary sort by document ID for consistency
    }

    #endregion Comparison Implementation

    #region String Representation

    public override string ToString() => ActivitySummary;

    #endregion String Representation
}