using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using ADMS.Application.Common.Validation;

namespace ADMS.Application.DTOs;

/// <summary>
/// Matter Document Activity User From DTO for source-side document transfer audit trail tracking.
/// </summary>
/// <remarks>
/// Represents the source side of document transfer operations between matters.
/// Links document transfer origins to users for accountability and compliance tracking.
/// </remarks>
public sealed record MatterDocumentActivityUserFromDto : IValidatableObject, IEquatable<MatterDocumentActivityUserFromDto>
{
    #region Core Properties

    /// <summary>
    /// Gets the source matter identifier.
    /// </summary>
    [Required(ErrorMessage = "Source matter ID is required.")]
    public required Guid MatterId { get; init; }

    /// <summary>
    /// Gets the document identifier.
    /// </summary>
    [Required(ErrorMessage = "Document ID is required.")]
    public required Guid DocumentId { get; init; }

    /// <summary>
    /// Gets the matter document activity identifier.
    /// </summary>
    [Required(ErrorMessage = "Matter document activity ID is required.")]
    public required Guid MatterDocumentActivityId { get; init; }

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
    /// Gets the source matter details.
    /// </summary>
    public MatterWithoutDocumentsDto? Matter { get; init; }

    /// <summary>
    /// Gets the document details.
    /// </summary>
    public DocumentWithoutRevisionsDto? Document { get; init; }

    /// <summary>
    /// Gets the matter document activity details.
    /// </summary>
    public MatterDocumentActivityDto? MatterDocumentActivity { get; init; }

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
    /// Gets a summary of the transfer operation for display.
    /// </summary>
    public string TransferSummary =>
        $"{Document?.FileName ?? "Document"} " +
        $"{MatterDocumentActivity?.Activity ?? "TRANSFERRED"} " +
        $"from {Matter?.Description ?? "Matter"} " +
        $"by {User?.Name ?? "User"}";

    /// <summary>
    /// Gets a value indicating whether this DTO has valid core data.
    /// </summary>
    public bool IsValid =>
        MatterId != Guid.Empty &&
        DocumentId != Guid.Empty &&
        MatterDocumentActivityId != Guid.Empty &&
        UserId != Guid.Empty &&
        CreatedAt != default;

    /// <summary>
    /// Gets comprehensive transfer metrics for analysis and reporting.
    /// </summary>
    public object TransferMetrics => new
    {
        TransferInfo = new
        {
            TransferSummary,
            LocalCreatedAtDateString,
            OperationType = MatterDocumentActivity?.Activity ?? "UNKNOWN",
            TransferDirection = "FROM"
        },
        ParticipantInfo = new
        {
            SourceMatter = Matter?.Description ?? "Unknown Matter",
            DocumentName = Document?.FileName ?? "Unknown Document",
            UserName = User?.Name ?? "Unknown User",
            UserId,
            MatterId,
            DocumentId
        },
        TemporalInfo = new
        {
            CreatedAt,
            LocalCreatedAtDateString,
            TransferAge = (DateTime.UtcNow - CreatedAt).TotalDays
        },
        ValidationInfo = new
        {
            HasCompleteInformation = Matter != null && Document != null &&
                                   MatterDocumentActivity != null && User != null,
            RequiredFieldsPresent = MatterId != Guid.Empty && DocumentId != Guid.Empty &&
                                  MatterDocumentActivityId != Guid.Empty && UserId != Guid.Empty
        }
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the MatterDocumentActivityUserFromDto for data integrity and business rules compliance.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate core GUIDs using consistent helper
        foreach (var result in UserValidationHelper.ValidateUserId(MatterId, nameof(MatterId)))
            yield return result;

        foreach (var result in UserValidationHelper.ValidateUserId(DocumentId, nameof(DocumentId)))
            yield return result;

        foreach (var result in UserValidationHelper.ValidateUserId(MatterDocumentActivityId, nameof(MatterDocumentActivityId)))
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

        if (Document != null && Document.Id != DocumentId)
        {
            yield return new ValidationResult(
                "Document navigation property ID must match DocumentId.",
                [nameof(Document), nameof(DocumentId)]);
        }

        if (MatterDocumentActivity != null && MatterDocumentActivity.Id != MatterDocumentActivityId)
        {
            yield return new ValidationResult(
                "MatterDocumentActivity navigation property ID must match MatterDocumentActivityId.",
                [nameof(MatterDocumentActivity), nameof(MatterDocumentActivityId)]);
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

        // Validate matter document activity type if MatterDocumentActivity is loaded
        if (MatterDocumentActivity != null)
        {
            foreach (var result in MatterDocumentActivityValidationHelper.ValidateActivity(MatterDocumentActivity.Activity, $"{nameof(MatterDocumentActivity)}.{nameof(MatterDocumentActivity.Activity)}"))
                yield return result;
        }
    }

    #endregion Validation Implementation

    #region Static Factory Methods

    /// <summary>
    /// Validates a MatterDocumentActivityUserFromDto instance and returns validation results.
    /// </summary>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterDocumentActivityUserFromDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterDocumentActivityUserFromDto instance is required."));
            return results;
        }

        var context = new ValidationContext(dto);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a MatterDocumentActivityUserFromDto from a Domain entity.
    /// </summary>
    public static MatterDocumentActivityUserFromDto FromEntity([NotNull] Domain.Entities.MatterDocumentActivityUserFrom entity, bool includeNavigationProperties = false)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new MatterDocumentActivityUserFromDto
        {
            MatterId = entity.MatterId,
            DocumentId = entity.DocumentId,
            MatterDocumentActivityId = entity.MatterDocumentActivityId,
            UserId = entity.UserId,
            CreatedAt = entity.CreatedAt
        };

        // Navigation properties can be set separately if needed for performance
        // dto.Matter = entity.Matter != null ? MatterWithoutDocumentsDto.FromEntity(entity.Matter) : null;
        // dto.Document = entity.Document != null ? DocumentWithoutRevisionsDto.FromEntity(entity.Document) : null;
        // dto.MatterDocumentActivity = entity.MatterDocumentActivity != null ? MatterDocumentActivityDto.FromEntity(entity.MatterDocumentActivity) : null;
        // dto.User = entity.User != null ? UserDto.FromEntity(entity.User) : null;

        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;

        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterDocumentActivityUserFromDto: {errorMessages}");
    }

    /// <summary>
    /// Creates multiple MatterDocumentActivityUserFromDto instances from a collection of entities.
    /// </summary>
    public static IList<MatterDocumentActivityUserFromDto> FromEntities([NotNull] IEnumerable<Domain.Entities.MatterDocumentActivityUserFrom> entities, bool includeNavigationProperties = false)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var result = new List<MatterDocumentActivityUserFromDto>();

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
                Console.WriteLine($"Warning: Skipped invalid transfer audit entity: {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a document transfer audit entry with specified parameters.
    /// </summary>
    public static MatterDocumentActivityUserFromDto CreateTransferAudit(
        Guid matterId,
        Guid documentId,
        Guid activityId,
        Guid userId,
        DateTime? timestamp = null)
    {
        if (matterId == Guid.Empty)
            throw new ArgumentException("Source matter ID cannot be empty.", nameof(matterId));
        if (documentId == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty.", nameof(documentId));
        if (activityId == Guid.Empty)
            throw new ArgumentException("Activity ID cannot be empty.", nameof(activityId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        var dto = new MatterDocumentActivityUserFromDto
        {
            MatterId = matterId,
            DocumentId = documentId,
            MatterDocumentActivityId = activityId,
            UserId = userId,
            CreatedAt = timestamp ?? DateTime.UtcNow
        };

        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;

        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid transfer audit entry: {errorMessages}");
    }

    #endregion Static Factory Methods

    #region Business Methods

    /// <summary>
    /// Determines whether this transfer represents a document move operation.
    /// </summary>
    public bool IsMoveOperation() =>
        string.Equals(MatterDocumentActivity?.Activity, "MOVED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this transfer represents a document copy operation.
    /// </summary>
    public bool IsCopyOperation() =>
        string.Equals(MatterDocumentActivity?.Activity, "COPIED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the age of this transfer operation in days.
    /// </summary>
    public double GetTransferAgeDays() => (DateTime.UtcNow - CreatedAt).TotalDays;

    /// <summary>
    /// Determines whether this transfer occurred recently.
    /// </summary>
    public bool IsRecentTransfer(int withinDays = 7) => GetTransferAgeDays() <= withinDays;

    /// <summary>
    /// Gets comprehensive transfer information for reporting and analysis.
    /// </summary>
    public IReadOnlyDictionary<string, object> GetTransferInformation()
    {
        return new Dictionary<string, object>
        {
            ["TransferType"] = MatterDocumentActivity?.Activity ?? "UNKNOWN",
            ["IsMove"] = IsMoveOperation(),
            ["IsCopy"] = IsCopyOperation(),
            ["SourceMatter"] = Matter?.Description ?? "Unknown Matter",
            ["DocumentName"] = Document?.FileName ?? "Unknown Document",
            ["UserName"] = User?.Name ?? "Unknown User",
            ["TransferDate"] = CreatedAt,
            ["LocalTransferDate"] = LocalCreatedAtDateString,
            ["TransferAgeDays"] = GetTransferAgeDays(),
            ["IsRecentTransfer"] = IsRecentTransfer(),
            ["TransferSummary"] = TransferSummary,
            ["HasCompleteInformation"] = Matter != null && Document != null &&
                                       MatterDocumentActivity != null && User != null
        };
    }

    /// <summary>
    /// Generates a professional audit trail message for this transfer.
    /// </summary>
    public string GenerateAuditMessage()
    {
        var operationType = MatterDocumentActivity?.Activity ?? "TRANSFERRED";
        var documentName = Document?.FileName ?? "document";
        var sourceMatter = Matter?.Description ?? "matter";
        var userName = User?.Name ?? "user";

        return $"On {LocalCreatedAtDateString}, {userName} {operationType} {documentName} FROM {sourceMatter}";
    }

    #endregion Business Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified <see cref="MatterDocumentActivityUserFromDto"/> is equal to the current
    /// instance.
    /// </summary>
    /// <param name="other">The <see cref="MatterDocumentActivityUserFromDto"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current instance; otherwise, <see
    /// langword="false"/>.</returns>
    public bool Equals(MatterDocumentActivityUserFromDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return MatterId.Equals(other.MatterId) &&
               DocumentId.Equals(other.DocumentId) &&
               MatterDocumentActivityId.Equals(other.MatterDocumentActivityId) &&
               UserId.Equals(other.UserId) &&
               CreatedAt.Equals(other.CreatedAt);
    }

    /// <summary>
    /// Returns a hash code for the current object.
    /// </summary>
    /// <remarks>The hash code is computed based on the values of the <see cref="MatterId"/>, <see
    /// cref="DocumentId"/>, <see cref="MatterDocumentActivityId"/>, <see cref="UserId"/>, and <see cref="CreatedAt"/>
    /// properties.</remarks>
    /// <returns>A 32-bit signed integer hash code that can be used for hashing algorithms and data structures such as a hash
    /// table.</returns>
    public override int GetHashCode() =>
        HashCode.Combine(MatterId, DocumentId, MatterDocumentActivityId, UserId, CreatedAt);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the transfer, including the document, matter, user, and timestamp details.
    /// </summary>
    /// <returns>A string in the format: "Transfer FROM: Document (<c>DocumentId</c>) ← Matter (<c>MatterId</c>) by User
    /// (<c>UserId</c>) at <c>CreatedAt</c>".</returns>
    public override string ToString() =>
        $"Transfer FROM: Document ({DocumentId}) ← Matter ({MatterId}) by User ({UserId}) at {CreatedAt:yyyy-MM-dd HH:mm:ss}";

    #endregion String Representation
}