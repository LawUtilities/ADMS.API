using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using ADMS.Application.Common.Validation;

namespace ADMS.Application.DTOs;

/// <summary>
/// Matter Document Activity User To DTO for destination-side document transfer audit trail tracking.
/// </summary>
/// <remarks>
/// Represents the destination side of document transfer operations between matters.
/// Links document transfer destinations to users for accountability and compliance tracking.
/// </remarks>
public sealed record MatterDocumentActivityUserToDto : IValidatableObject, IEquatable<MatterDocumentActivityUserToDto>
{
    #region Core Properties

    /// <summary>
    /// Gets the destination matter identifier.
    /// </summary>
    [Required(ErrorMessage = "Destination matter ID is required.")]
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
    /// Gets the destination matter details.
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
    public string LocalCreatedAtDateString => CreatedAt.ToLocalTime()
        .ToString("dddd, dd MMMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets a summary of the transfer operation for display.
    /// </summary>
    public string TransferSummary =>
        $"{Document?.FileName ?? "Document"} " +
        $"{MatterDocumentActivity?.Activity ?? "TRANSFERRED"} " +
        $"to {Matter?.Description ?? "Matter"} " +
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
            TransferDirection = "TO"
        },
        ParticipantInfo = new
        {
            DestinationMatter = Matter?.Description ?? "Unknown Matter",
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
    /// Validates the MatterDocumentActivityUserToDto using enhanced UserValidationHelper methods.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);

        // Existing validation (Phase 1)
        foreach (var result in ValidateCompositeKey())
            yield return result;

        foreach (var result in ValidateMatterDocumentActivity())
            yield return result;

        foreach (var result in ValidateNavigationPropertyConsistency())
            yield return result;

        foreach (var result in ValidateTransferBusinessRules())
            yield return result;

        foreach (var result in ValidateProfessionalCompliance())
            yield return result;

        // Enhanced validation (Phase 2)
        foreach (var result in ValidateBidirectionalTransferConsistency())
            yield return result;

        foreach (var result in ValidateDocumentTransferState())
            yield return result;

        foreach (var result in ValidateRegulatoryCompliance())
            yield return result;

        foreach (var result in ValidateDestinationAppropriatenesss())
            yield return result;

        foreach (var result in ValidateTransferPatterns())
            yield return result;

        foreach (var result in ValidateLegalDiscoveryCompliance())
            yield return result;

        foreach (var result in ValidateEdgeCases())
            yield return result;
    }

    /// <summary>
    /// Validates the composite key components using specialized validation helpers.
    /// </summary>
    /// <returns>A collection of validation results for composite key validation.</returns>
    private IEnumerable<ValidationResult> ValidateCompositeKey()
    {
        // Use UserValidationHelper for consistent GUID validation
        foreach (var result in UserValidationHelper.ValidateUserId(MatterId, nameof(MatterId)))
            yield return result;

        foreach (var result in UserValidationHelper.ValidateUserId(DocumentId, nameof(DocumentId)))
            yield return result;

        foreach (var result in UserValidationHelper.ValidateUserId(MatterDocumentActivityId,
                     nameof(MatterDocumentActivityId)))
            yield return result;

        foreach (var result in UserValidationHelper.ValidateUserId(UserId, nameof(UserId)))
            yield return result;

        // Use specialized timestamp validation
        foreach (var result in UserValidationHelper.ValidateActivityTimestamp(CreatedAt, nameof(CreatedAt)))
            yield return result;
    }

    #endregion Validation Implementation

    #region Static Factory Methods

    /// <summary>
    /// Validates a MatterDocumentActivityUserToDto instance and returns validation results.
    /// </summary>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterDocumentActivityUserToDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterDocumentActivityUserToDto instance is required."));
            return results;
        }

        var context = new ValidationContext(dto);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a MatterDocumentActivityUserToDto from a Domain entity.
    /// </summary>
    public static MatterDocumentActivityUserToDto FromEntity(
        [NotNull] Domain.Entities.MatterDocumentActivityUserTo entity, bool includeNavigationProperties = false)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new MatterDocumentActivityUserToDto
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
        throw new ValidationException($"Failed to create valid MatterDocumentActivityUserToDto: {errorMessages}");
    }

    /// <summary>
    /// Creates multiple MatterDocumentActivityUserToDto instances from a collection of entities.
    /// </summary>
    public static IList<MatterDocumentActivityUserToDto> FromEntities(
        [NotNull] IEnumerable<Domain.Entities.MatterDocumentActivityUserTo> entities,
        bool includeNavigationProperties = false)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var result = new List<MatterDocumentActivityUserToDto>();

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
    public static MatterDocumentActivityUserToDto CreateTransferAudit(
        Guid matterId,
        Guid documentId,
        Guid activityId,
        Guid userId,
        DateTime? timestamp = null)
    {
        if (matterId == Guid.Empty)
            throw new ArgumentException("Destination matter ID cannot be empty.", nameof(matterId));
        if (documentId == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty.", nameof(documentId));
        if (activityId == Guid.Empty)
            throw new ArgumentException("Activity ID cannot be empty.", nameof(activityId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        var dto = new MatterDocumentActivityUserToDto
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
    public bool IsRecentTransferDays(int withinDays = 7) => GetTransferAgeDays() <= withinDays;

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
            ["DestinationMatter"] = Matter?.Description ?? "Unknown Matter",
            ["DocumentName"] = Document?.FileName ?? "Unknown Document",
            ["UserName"] = User?.Name ?? "Unknown User",
            ["TransferDate"] = CreatedAt,
            ["LocalTransferDate"] = LocalCreatedAtDateString,
            ["TransferAgeDays"] = GetTransferAgeDays(),
            ["IsRecentTransferDays"] = IsRecentTransferDays(),
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
        var destinationMatter = Matter?.Description ?? "matter";
        var userName = User?.Name ?? "user";

        return $"On {LocalCreatedAtDateString}, {userName} {operationType} {documentName} TO {destinationMatter}";
    }

    #endregion Business Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified <see cref="MatterDocumentActivityUserToDto"/> is equal to the current instance.
    /// </summary>
    /// <remarks>Two instances are considered equal if all of the following properties are equal:
    /// <c>MatterId</c>, <c>DocumentId</c>, <c>MatterDocumentActivityId</c>, <c>UserId</c>, and
    /// <c>CreatedAt</c>.</remarks>
    /// <param name="other">The <see cref="MatterDocumentActivityUserToDto"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current instance; otherwise, <see
    /// langword="false"/>.</returns>
    public bool Equals(MatterDocumentActivityUserToDto? other)
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
    /// Returns a hash code for the current object based on its key properties.
    /// </summary>
    /// <remarks>The hash code is computed using the values of <see cref="MatterId"/>, <see
    /// cref="DocumentId"/>,  <see cref="MatterDocumentActivityId"/>, <see cref="UserId"/>, and <see cref="CreatedAt"/>.
    /// This ensures that objects with the same key property values produce the same hash code.</remarks>
    /// <returns>An integer representing the hash code for the current object.</returns>
    public override int GetHashCode() =>
        HashCode.Combine(MatterId, DocumentId, MatterDocumentActivityId, UserId, CreatedAt);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the transfer, including the associated document, matter, user, and creation
    /// timestamp.
    /// </summary>
    /// <returns>A string that describes the transfer in the format:  "Transfer TO: Document (<c>DocumentId</c>) → Matter
    /// (<c>MatterId</c>) by User (<c>UserId</c>) at <c>CreatedAt</c>".</returns>
    public override string ToString() =>
        $"Transfer TO: Document ({DocumentId}) → Matter ({MatterId}) by User ({UserId}) at {CreatedAt:yyyy-MM-dd HH:mm:ss}";

    #endregion String Representation

    #region Additional Validation Methods

    /// <summary>
    /// Validates the matter document activity type for transfer operations.
    /// </summary>
    /// <returns>A collection of validation results for activity validation.</returns>
    private IEnumerable<ValidationResult> ValidateMatterDocumentActivity()
    {
        if (MatterDocumentActivity == null) yield break;

        // Validate activity type using MatterDocumentActivityValidationHelper
        foreach (var result in MatterDocumentActivityValidationHelper.ValidateActivity(
                     MatterDocumentActivity.Activity, $"{nameof(MatterDocumentActivity)}.EntityType"))
            yield return result;

        // Validate that activity supports destination tracking
        if (!MatterDocumentActivityValidationHelper.IsValidTransferActivity(MatterDocumentActivity.Activity))
        {
            yield return new ValidationResult(
                $"Activity '{MatterDocumentActivity.Activity}' is not a valid document transfer operation. " +
                "Only MOVED and COPIED operations are supported for destination tracking.",
                [nameof(MatterDocumentActivity)]);
        }

        // Ensure activity supports bidirectional tracking
        if (!MatterDocumentActivityValidationHelper.RequiresBidirectionalTracking(MatterDocumentActivity.Activity))
        {
            yield return new ValidationResult(
                $"Activity '{MatterDocumentActivity.Activity}' does not require destination-side tracking.",
                [nameof(MatterDocumentActivity)]);
        }
    }

    /// <summary>
    /// Validates consistency between foreign key IDs and navigation properties.
    /// </summary>
    /// <returns>A collection of validation results for cross-reference validation.</returns>
    private IEnumerable<ValidationResult> ValidateNavigationPropertyConsistency()
    {
        // Matter navigation property consistency
        if (Matter != null)
        {
            if (Matter.Id != MatterId)
            {
                yield return new ValidationResult(
                    "Matter navigation property ID must match MatterId for destination tracking integrity.",
                    [nameof(Matter), nameof(MatterId)]);
            }

            // Validate that destination matter is in valid state for receiving documents
            if (Matter.IsDeleted)
            {
                yield return new ValidationResult(
                    "Cannot transfer documents to a deleted destination matter.",
                    [nameof(Matter)]);
            }

            if (Matter.IsArchived)
            {
                yield return new ValidationResult(
                    "Transferring documents to archived matters may not be appropriate for active document management.",
                    [nameof(Matter)]);
            }
        }

        // Document navigation property consistency
        if (Document != null)
        {
            if (Document.Id != DocumentId)
            {
                yield return new ValidationResult(
                    "Document navigation property ID must match DocumentId for transfer tracking integrity.",
                    [nameof(Document), nameof(DocumentId)]);
            }

            // Validate document state for transfer
            if (Document.IsDeleted)
            {
                yield return new ValidationResult(
                    "Cannot create transfer audit entries for deleted documents.",
                    [nameof(Document)]);
            }

            if (Document.IsCheckedOut)
            {
                yield return new ValidationResult(
                    "Documents that are checked out should not be transferred without proper check-in procedures.",
                    [nameof(Document)]);
            }
        }

        // Activity navigation property consistency
        if (MatterDocumentActivity != null && MatterDocumentActivity.Id != MatterDocumentActivityId)
        {
            yield return new ValidationResult(
                "MatterDocumentActivity navigation property ID must match MatterDocumentActivityId.",
                [nameof(MatterDocumentActivity), nameof(MatterDocumentActivityId)]);
        }

        // User navigation property consistency
        if (User == null) yield break;
        if (User.Id != UserId)
        {
            yield return new ValidationResult(
                "User navigation property ID must match UserId for proper audit attribution.",
                [nameof(User), nameof(UserId)]);
        }

        // Validate user for professional responsibility
        foreach (var result in UserValidationHelper.ValidateUsername(
                     User.Name, $"{nameof(User)}.{nameof(User.Name)}"))
            yield return result;
    }

    /// <summary>
    /// Validates business rules specific to document transfer operations.
    /// </summary>
    /// <returns>A collection of validation results for business rule compliance.</returns>
    private IEnumerable<ValidationResult> ValidateTransferBusinessRules()
    {
        // Validate temporal consistency for transfers
        var transferAge = GetTransferAgeDays();
        if (transferAge > UserValidationHelper.MaxReasonableActivityAgeDays)
        {
            yield return new ValidationResult(
                $"Transfer operation timestamp is unusually old ({transferAge:F1} days). " +
                "Verify timestamp accuracy for audit trail integrity.",
                [nameof(CreatedAt)]);
        }

        // Future date validation with tolerance
        if (CreatedAt > DateTime.UtcNow.AddMinutes(UserValidationHelper.FutureDateToleranceMinutes))
        {
            yield return new ValidationResult(
                $"Transfer timestamp cannot be in the future beyond clock skew tolerance " +
                $"({UserValidationHelper.FutureDateToleranceMinutes} minutes).",
                [nameof(CreatedAt)]);
        }

        // Professional standards for document transfers
        if (MatterDocumentActivity?.Activity == "MOVED" && Document != null)
        {
            yield return new ValidationResult(
                "MOVED operations represent document custody transfer. " +
                "Ensure proper professional procedures are followed for document custody changes.",
                [nameof(MatterDocumentActivity)]);
        }

        // Validate destination matter appropriateness
        if (Matter == null || Document == null) yield break;
        // Check for potential circular transfers or inappropriate destinations
        var documentExtension = Document.Extension?.ToLowerInvariant();
        if (!string.IsNullOrEmpty(documentExtension) &&
            !FileValidationHelper.IsLegalDocumentFormat(documentExtension))
        {
            yield return new ValidationResult(
                $"Document with extension '{Document.Extension}' may not be appropriate for legal matter transfer.",
                [nameof(Document)]);
        }
    }

    /// <summary>
    /// Validates legal compliance and professional responsibility requirements.
    /// </summary>
    /// <returns>A collection of validation results for professional compliance.</returns>
    private IEnumerable<ValidationResult> ValidateProfessionalCompliance()
    {
        // User attribution requirements for legal compliance
        if (User == null && UserId != Guid.Empty)
        {
            yield return new ValidationResult(
                "Professional responsibility requires complete user attribution for document transfers. " +
                "User navigation property should be populated for audit trail completeness.",
                [nameof(User)]);
        }

        // Activity classification requirements
        if (MatterDocumentActivity == null && MatterDocumentActivityId != Guid.Empty)
        {
            yield return new ValidationResult(
                "Professional document management requires complete activity classification. " +
                "MatterDocumentActivity navigation property should be populated for compliance reporting.",
                [nameof(MatterDocumentActivity)]);
        }

        // Matter context requirements
        if (Matter == null && MatterId != Guid.Empty)
        {
            yield return new ValidationResult(
                "Legal compliance requires complete matter context for document transfers. " +
                "Matter navigation property should be populated for audit trail completeness.",
                [nameof(Matter)]);
        }

        // Document context requirements
        if (Document == null && DocumentId != Guid.Empty)
        {
            yield return new ValidationResult(
                "Professional document management requires complete document context. " +
                "Document navigation property should be populated for audit trail completeness.",
                [nameof(Document)]);
        }

        // Temporal validation for legal timeline requirements
        if (CreatedAt < UserValidationHelper.MinReasonableActivityDate)
        {
            yield return new ValidationResult(
                $"Transfer timestamp predates reasonable system operation bounds " +
                $"({UserValidationHelper.MinReasonableActivityDate:yyyy-MM-dd}). " +
                "Verify timestamp accuracy for legal compliance.",
                [nameof(CreatedAt)]);
        }
    }

    /// <summary>
    /// Validates that bidirectional transfer audit trails are consistent and complete.
    /// </summary>
    /// <returns>A collection of validation results for bidirectional consistency.</returns>
    private IEnumerable<ValidationResult> ValidateBidirectionalTransferConsistency()
    {
        // This would require access to the corresponding "FROM" audit trail
        // to ensure complete bidirectional tracking
        if (MatterDocumentActivity?.Activity == "MOVED")
        {
            yield return new ValidationResult(
                "MOVED operations must have corresponding source-side audit trail entries. " +
                "Verify that MatterDocumentActivityUserFromDto exists for complete audit coverage.",
                [nameof(MatterDocumentActivity)]);
        }

        // Validate transfer sequence integrity
        var transferAge = GetTransferAgeDays();
        if (transferAge is > 0 and < 0.001) // Less than 1.44 minutes
        {
            yield return new ValidationResult(
                "Transfer operation completed unusually quickly. " +
                "Verify transfer process integrity and professional procedures.",
                [nameof(CreatedAt)]);
        }
    }

    /// <summary>
    /// Validates document state appropriateness for transfer operations.
    /// </summary>
    /// <returns>A collection of validation results for document state validation.</returns>
    private IEnumerable<ValidationResult> ValidateDocumentTransferState()
    {
        if (Document == null) yield break;

        // Enhanced document state validation
        if (Document.IsCheckedOut)
        {
            yield return new ValidationResult(
                "Documents that are checked out should be checked in before transfer operations. " +
                "This ensures document state consistency and prevents concurrent modification conflicts.",
                [nameof(Document)]);
        }

        // File integrity validation at transfer time
        if (!Document.HasValidChecksum)
        {
            yield return new ValidationResult(
                "Document checksum validation failed at transfer time. " +
                "Document integrity must be verified before transfer operations for legal compliance.",
                [nameof(Document)]);
        }

        // Document format validation for legal transfers
        if (!string.IsNullOrEmpty(Document.Extension) &&
            !FileValidationHelper.IsLegalDocumentFormat(Document.Extension))
        {
            yield return new ValidationResult(
                $"Document format '{Document.Extension}' may not be appropriate for legal matter transfers. " +
                "Verify document classification and transfer authorization.",
                [nameof(Document)]);
        }

        // Size validation for transfer operations
        if (Document.FileSize > 100 * 1024 * 1024) // 100 MB
        {
            yield return new ValidationResult(
                $"Large document transfer ({Document.FormattedFileSize}) requires additional verification. " +
                "Ensure adequate storage capacity and transfer authorization for large files.",
                [nameof(Document)]);
        }
    }

    /// <summary>
    /// Validates compliance with regulatory timeline requirements for document transfers.
    /// </summary>
    /// <returns>A collection of validation results for regulatory compliance.</returns>
    private IEnumerable<ValidationResult> ValidateRegulatoryCompliance()
    {
        // Transfer timing validation for legal deadlines
        var businessDaysSinceTransfer = CalculateBusinessDays(CreatedAt, DateTime.UtcNow);

        if (MatterDocumentActivity?.Activity == "MOVED" && businessDaysSinceTransfer > 30)
        {
            yield return new ValidationResult(
                $"Document custody transfer occurred {businessDaysSinceTransfer} business days ago. " +
                "Extended custody periods may require additional compliance documentation.",
                [nameof(CreatedAt)]);
        }

        // Professional responsibility validation
        if (Matter != null && IsWeekend(CreatedAt))
        {
            yield return new ValidationResult(
                "Document transfer performed during weekend hours. " +
                "Verify authorization and professional oversight for non-standard hour operations.",
                [nameof(CreatedAt)]);
        }

        // Validation for high-volume transfer patterns
        var hourOfDay = CreatedAt.Hour;
        if (hourOfDay is < 6 or > 22) // Outside normal business hours
        {
            yield return new ValidationResult(
                $"Document transfer performed at {CreatedAt:HH:mm} (outside normal business hours). " +
                "Unusual transfer timing may require additional authorization verification.",
                [nameof(CreatedAt)]);
        }
    }

    private static int CalculateBusinessDays(DateTime startDate, DateTime endDate)
    {
        var businessDays = 0;
        var current = startDate.Date;

        while (current <= endDate.Date)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                businessDays++;
            current = current.AddDays(1);
        }

        return businessDays;
    }

    private static bool IsWeekend(DateTime date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    /// <summary>
    /// Validates that the destination matter is appropriate for receiving the specific document type.
    /// </summary>
    /// <returns>A collection of validation results for destination appropriateness.</returns>
    private IEnumerable<ValidationResult> ValidateDestinationAppropriatenesss()
    {
        if (Matter == null || Document == null) yield break;

        // Enhanced matter state validation
        if (Matter.IsArchived)
        {
            yield return new ValidationResult(
                $"Cannot transfer documents to archived matter '{Matter.Description}'. " +
                "Archived matters should not receive new documents to maintain organizational integrity.",
                [nameof(Matter)]);
        }

        // Matter capacity and organization validation
        if (Matter.IsDeleted)
        {
            yield return new ValidationResult(
                $"Cannot transfer documents to deleted matter '{Matter.Description}'. " +
                "Document transfers to deleted matters violate audit trail integrity.",
                [nameof(Matter)]);
        }

        // Professional practice validation for matter organization
        var matterDescription = Matter.Description?.ToUpperInvariant() ?? "";
        var documentFileName = Document.FileName?.ToUpperInvariant() ?? "";

        // Enhanced semantic validation for matter-document relationships
        if (matterDescription.Contains("CLOSED") && !documentFileName.Contains("FINAL"))
        {
            yield return new ValidationResult(
                "Transferring non-final documents to matters marked as 'CLOSED' may indicate " +
                "organizational inconsistency. Verify matter status and document classification.",
                [nameof(Matter), nameof(Document)]);
        }

        // Client confidentiality validation (example pattern)
        if (matterDescription.Contains("CONFIDENTIAL") && GetTransferAgeDays() > 1)
        {
            yield return new ValidationResult(
                "Delayed transfers to confidential matters require additional security verification. " +
                "Ensure proper authorization and confidentiality protocols are followed.",
                [nameof(Matter), nameof(CreatedAt)]);
        }
    }

    /// <summary>
    /// Validates transfer patterns for potential security or compliance concerns.
    /// </summary>
    /// <returns>A collection of validation results for transfer pattern analysis.</returns>
    private IEnumerable<ValidationResult> ValidateTransferPatterns()
    {
        // This would require access to broader transfer history for pattern analysis
        var transferDay = CreatedAt.DayOfWeek;
        var transferHour = CreatedAt.Hour;

        // Unusual timing pattern detection
        if (transferDay == DayOfWeek.Sunday || (transferDay == DayOfWeek.Saturday && transferHour < 9))
        {
            yield return new ValidationResult(
                "Document transfer during unusual hours may require additional oversight. " +
                "Verify authorization for off-hours document management operations.",
                [nameof(CreatedAt)]);
        }

        // High-frequency transfer validation
        var minutesFromTopOfHour = CreatedAt.Minute;
        if (minutesFromTopOfHour == 0) // Exactly on the hour
        {
            yield return new ValidationResult(
                "Transfer occurred exactly on the hour, which may indicate automated processing. " +
                "Verify that automated transfers have proper authorization and audit trails.",
                [nameof(CreatedAt)]);
        }

        // Professional responsibility timing validation
        if (MatterDocumentActivity?.Activity == "MOVED" && IsRecentTransferMinutes(minutes: 5))
        {
            yield return new ValidationResult(
                "Very recent MOVED operation detected. Verify that proper professional " +
                "procedures were followed for document custody transfers.",
                [nameof(CreatedAt), nameof(MatterDocumentActivity)]);
        }
    }

    private bool IsRecentTransferMinutes(int minutes = 30) =>
        (DateTime.UtcNow - CreatedAt).TotalMinutes <= minutes;

    /// <summary>
    /// Validates transfer operations for legal discovery and litigation hold compliance.
    /// </summary>
    /// <returns>A collection of validation results for legal discovery compliance.</returns>
    private IEnumerable<ValidationResult> ValidateLegalDiscoveryCompliance()
    {
        // Document preservation validation
        if (MatterDocumentActivity?.Activity == "MOVED" && Document != null)
        {
            yield return new ValidationResult(
                "MOVED operation represents document custody transfer. " +
                "Ensure compliance with litigation hold requirements and document preservation obligations. " +
                "Original matter may require notification of document custody change.",
                [nameof(MatterDocumentActivity), nameof(Document)]);
        }

        // Audit trail completeness for legal compliance
        if (User == null && UserId != Guid.Empty)
        {
            yield return new ValidationResult(
                "Legal discovery requires complete user attribution for all document operations. " +
                "Missing user information compromises audit trail completeness for legal proceedings.",
                [nameof(User), nameof(UserId)]);
        }

        // Matter association validation for legal context
        if (Matter == null && MatterId != Guid.Empty)
        {
            yield return new ValidationResult(
                "Legal discovery requires complete matter context for document transfers. " +
                "Missing matter information compromises legal case documentation and discovery processes.",
                [nameof(Matter), nameof(MatterId)]);
        }

        // Document metadata preservation validation
        if (Document != null && string.IsNullOrWhiteSpace(Document.FileName))
        {
            yield return new ValidationResult(
                "Document transfers for legal discovery require complete file metadata. " +
                "Missing or incomplete document information affects discovery compliance.",
                [nameof(Document)]);
        }

        // Temporal validation for legal timeline requirements
        var transferAge = GetTransferAgeDays();
        if (transferAge > 2555) // More than 7 years
        {
            yield return new ValidationResult(
                $"Document transfer audit entry is {transferAge:F0} days old (over 7 years). " +
                "Extended audit trail retention may require special compliance consideration.",
                [nameof(CreatCreatedAtedAt)]);
        }
    }

    /// <summary>
    /// Validates the comprehensive user activity record using UserValidationHelper.
    /// </summary>
    /// <returns>A collection of validation results for complete activity record validation.</returns>
    private IEnumerable<ValidationResult> ValidateUserActivityRecord()
    {
        // Use UserValidationHelper.ValidateActivityRecord for comprehensive validation
        if (User != null && MatterDocumentActivity != null)
        {
            foreach (var result in UserValidationHelper.ValidateActivityRecord(
                         UserId, User.Name, DocumentId, "MATTER_DOCUMENT",
                         MatterDocumentActivity.Activity, CreatedAt, nameof(MatterDocumentActivityUserToDto)))
                yield return result;
        }
    }

    /// <summary>
    /// Validates activity type consistency using UserValidationHelper methods.
    /// </summary>
    /// <returns>A collection of validation results for activity type validation.</returns>
    private IEnumerable<ValidationResult> ValidateActivityTypeConsistency()
    {
        if (MatterDocumentActivity?.Activity == null) yield break;
        // Validate entity type
        foreach (var result in UserValidationHelper.ValidateEntityType(
                     "MATTER_DOCUMENT", $"{nameof(MatterDocumentActivity)}.EntityType"))
            yield return result;

        // Validate activity type for entity
        foreach (var result in UserValidationHelper.ValidateActivityType(
                     MatterDocumentActivity.Activity, "MATTER_DOCUMENT",
                     $"{nameof(MatterDocumentActivity)}.{nameof(MatterDocumentActivity.Activity)}"))
            yield return result;

        // Validate that activity requires related entity (bidirectional tracking)
        if (!UserValidationHelper.RequiresRelatedEntity(MatterDocumentActivity.Activity))
        {
            yield return new ValidationResult(
                $"Activity '{MatterDocumentActivity.Activity}' does not require destination-side tracking.",
                [nameof(MatterDocumentActivity)]);
        }
    }

    /// <summary>
    /// Validates activity consistency for document transfer operations.
    /// </summary>
    /// <returns>A collection of validation results for activity consistency.</returns>
    private IEnumerable<ValidationResult> ValidateActivityConsistencyForTransfers()
    {
        if (MatterDocumentActivity?.Activity != null)
        {
            // Use UserValidationHelper for transfer operation consistency
            foreach (var result in UserValidationHelper.ValidateActivityConsistency(
                         UserId, DocumentId, MatterDocumentActivity.Activity, MatterId,
                         nameof(MatterDocumentActivityUserToDto)))
                yield return result;
        }
    }

    /// <summary>
    /// Validates activity timestamp against enhanced business rules.
    /// </summary>
    /// <returns>A collection of validation results for timestamp business rules.</returns>
    private IEnumerable<ValidationResult> ValidateActivityTimestampBusinessRules()
    {
        // Use UserValidationHelper for enhanced timestamp validation
        foreach (var result in UserValidationHelper.ValidateActivityTimestampBusinessRules(
                     CreatedAt, nameof(CreatedAt)))
            yield return result;

        // Additional validation for transfer operations
        if (MatterDocumentActivity?.Activity is not ("MOVED" or "COPIED")) yield break;
        var transferAge = GetTransferAgeDays();
        if (transferAge > UserValidationHelper.MaxActivityBackdateHours / 24.0)
        {
            yield return new ValidationResult(
                $"Document transfer timestamp is {transferAge:F1} days old, " +
                $"exceeding maximum allowed backdate period of {UserValidationHelper.MaxActivityBackdateHours} hours.",
                [nameof(CreatedAt)]);
        }
    }

    /// <summary>
    /// Validates complete user attribution using UserValidationHelper.
    /// </summary>
    /// <returns>A collection of validation results for user attribution.</returns>
    private IEnumerable<ValidationResult> ValidateUserAttributionComprehensive()
    {
        // Use UserValidationHelper for comprehensive user attribution
        if (User != null)
        {
            foreach (var result in UserValidationHelper.ValidateUserAttribution(
                         UserId, User.Name, CreatedAt, nameof(User)))
                yield return result;
        }

        // Validate username format specifically
        if (User?.Name == null) yield break;
        foreach (var result in UserValidationHelper.ValidateUsername(
                     User.Name, $"{nameof(User)}.{nameof(User.Name)}"))
            yield return result;
    }

    /// <summary>
    /// Validates activity sequences to detect suspicious patterns.
    /// </summary>
    /// <returns>A collection of validation results for activity sequence validation.</returns>
    private IEnumerable<ValidationResult> ValidateActivitySequencePatterns()
    {
        // This would require access to recent activity history for the user
        // Implementation would need to be coordinated with service layer

        // Example framework for sequence validation
        if (User?.MatterDocumentActivityUsersTo == null) yield break;
        var recentTimestamps = User.MatterDocumentActivityUsersTo
            .Where(a => a.CreatedAt >=
                        DateTime.UtcNow.AddHours(-UserValidationHelper.ActivityBurstWindowMinutes / 60.0 * 24))
            .Select(a => a.CreatedAt);

        foreach (var result in UserValidationHelper.ValidateActivitySequence(
                     recentTimestamps, CreatedAt, nameof(CreatedAt)))
            yield return result;
    }

    /// <summary>
    /// Validates activity metrics for compliance monitoring.
    /// </summary>
    /// <returns>A collection of validation results for activity metrics.</returns>
    private IEnumerable<ValidationResult> ValidateActivityMetrics()
    {
        // Calculate activity metrics for monitoring
        var activityCount = 1; // This transfer activity
        var timeFrameHours = 24.0; // Last 24 hours

        // Use UserValidationHelper for activity metrics validation
        foreach (var result in UserValidationHelper.ValidateActivityMetrics(
                     activityCount, timeFrameHours, nameof(CreatedAt)))
            yield return result;

        // Professional standards for document transfers
        if (MatterDocumentActivity?.Activity != "MOVED") yield break;
        yield return new ValidationResult(
            "MOVED operations represent document custody transfer. " +
            "Ensure compliance with professional document handling standards and client notification requirements.",
            [nameof(MatterDocumentActivity)]);
    }

    // Edge case: TO entry exists without corresponding FROM entry
    private IEnumerable<ValidationResult> ValidateOrphanedAuditEntries()
    {
        if (MatterDocumentActivity?.Activity == "MOVED")
        {
            // This would require service-level validation to check for corresponding FROM entry
            yield return new ValidationResult(
                "MOVED operations require corresponding source-side audit trail. " +
                "This destination entry may be orphaned.",
                [nameof(MatterDocumentActivity)]);
        }
    }

    // Edge case: Matter becomes archived during transfer operation
    private IEnumerable<ValidationResult> ValidateDestinationStateTransitions()
    {
        if (Matter == null) yield break;
        // Edge case: Recently archived matter
        if (Matter.IsArchived && GetTransferAgeDays() < 1)
        {
            yield return new ValidationResult(
                "Document transferred to matter that was archived shortly after transfer. " +
                "Verify transfer timing and matter lifecycle consistency.",
                [nameof(Matter)]);
        }

        // Edge case: Matter with suspicious description patterns
        var description = Matter.Description?.ToUpperInvariant() ?? "";
        if (description.Contains("TEST") && !description.Contains("CLOSED"))
        {
            yield return new ValidationResult(
                "Transfer to matter with 'TEST' designation detected. " +
                "Verify this is not a production data contamination issue.",
                [nameof(Matter)]);
        }
    }

    // Edge case: Document with zero file size
    // Edge case: Document with corrupted checksum
    // Edge case: Document with system-generated filename patterns
    private IEnumerable<ValidationResult> ValidateDocumentAnomalies()
    {
        if (Document == null) yield break;
        // Edge case: Suspiciously large document
        if (Document.FileSize > 500 * 1024 * 1024) // 500MB
        {
            yield return new ValidationResult(
                $"Extremely large document transfer ({Document.FormattedFileSize}). " +
                "Verify storage capacity and transfer authorization.",
                [nameof(Document)]);
        }

        // Edge case: Document with potential system filename
        if (Document.FileName.StartsWith("TEMP_", StringComparison.Ordinal) ||
            Document.FileName?.Contains("SYSTEM") == true)
        {
            yield return new ValidationResult(
                "Document with system-generated filename pattern detected. " +
                "Verify this is not a temporary or system file.",
                [nameof(Document)]);
        }
    }

    // Edge case: Rapid-fire transfers (potential automation)
    private IEnumerable<ValidationResult> ValidateTransferFrequency()
    {
        var transferMinute = CreatedAt.Minute;
        var transferSecond = CreatedAt.Second;

        // Edge case: Transfer exactly on minute boundary
        if (transferSecond == 0)
        {
            yield return new ValidationResult(
                "Transfer occurred exactly on minute boundary. " +
                "This may indicate automated processing requiring verification.",
                [nameof(CreatedAt)]);
        }

        // Edge case: Transfer during system maintenance windows
        var hour = CreatedAt.Hour;
        if (hour is >= 2 and <= 4) // Typical maintenance window
        {
            yield return new ValidationResult(
                "Document transfer during typical system maintenance hours (2-4 AM). " +
                "Verify this was an authorized operation.",
                [nameof(CreatedAt)]);
        }
    }

    // Edge case: Very old pending transfers
    private IEnumerable<ValidationResult> ValidatePendingTransferAge()
    {
        var transferAge = GetTransferAgeDays();

        // Edge case: Transfer from distant past
        if (transferAge > 2555) // 7+ years
        {
            yield return new ValidationResult(
                $"Document transfer audit entry is exceptionally old ({transferAge:F0} days). " +
                "Verify data migration accuracy and retention policy compliance.",
                [nameof(CreatedAt)]);
        }

        // Edge case: Transfer from before system deployment
        if (CreatedAt < new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc))
        {
            yield return new ValidationResult(
                "Transfer timestamp predates expected system deployment. " +
                "Verify data migration integrity.",
                [nameof(CreatedAt)]);
        }
    }

    // Edge case: Inactive or suspended users
    private IEnumerable<ValidationResult> ValidateUserStatus()
    {
        if (User == null) yield break;
        // Edge case: User with suspicious naming patterns
        if ((User.Name?.Contains("ADMIN", StringComparison.OrdinalIgnoreCase) == true) ||
            (User.Name?.Contains("SYSTEM", StringComparison.OrdinalIgnoreCase) == true))
        {
            yield return new ValidationResult(
                "Document transfer attributed to administrative or system user. " +
                "Verify proper authorization and audit trail requirements.",
                [nameof(User)]);
        }

        // Edge case: Very short or very long usernames
        if (User.Name?.Length <= 2)
        {
            yield return new ValidationResult(
                "Unusually short username detected. Verify user identity validation.",
                [nameof(User)]);
        }
    }

    // Edge case: Activity type mismatches
    private IEnumerable<ValidationResult> ValidateCrossReferenceConsistency()
    {
        // Edge case: Activity supports transfers but isn't MOVED or COPIED
        if (MatterDocumentActivity != null)
        {
            var activity = MatterDocumentActivity.Activity?.ToUpperInvariant();
            if (!string.IsNullOrEmpty(activity) && 
                activity != "MOVED" && activity != "COPIED")
            {
                yield return new ValidationResult(
                    $"Unexpected activity type '{activity}' for destination tracking. " +
                    "Only MOVED and COPIED operations should have destination entries.",
                    [nameof(MatterDocumentActivity)]);
            }
        }

        // Edge case: GUID collision (extremely rare but possible)
        if (MatterId == DocumentId || MatterId == UserId || DocumentId == UserId)
        {
            yield return new ValidationResult(
                "GUID collision detected in composite key components. " +
                "This is extremely rare and may indicate data corruption.",
                [nameof(MatterId), nameof(DocumentId), nameof(UserId)]);
        }
    }

    // Edge case: Performance impact of validating large collections
    private IEnumerable<ValidationResult> ValidatePerformanceImpact()
    {
        // This would be at the service level, but awareness is important
        var collectionCount = 
            (User?.MatterDocumentActivityUsersTo?.Count ?? 0) +
            (Matter?.MatterDocumentActivityUsersTo?.Count ?? 0) +
            (Document?.MatterDocumentActivityUsersTo?.Count ?? 0);

        if (collectionCount > 1000)
        {
            yield return new ValidationResult(
                "Large collection detected during validation. " +
                "Consider pagination or selective loading for performance.",
                [nameof(User)]);
        }
    }

    /// <summary>
    /// Validates edge cases for document transfer operations.
    /// </summary>
    /// <returns>A collection of validation results for edge case validation.</returns>
    private IEnumerable<ValidationResult> ValidateEdgeCases()
    {
        // Temporal edge cases
        foreach (var result in ValidateTransferFrequency())
            yield return result;

        foreach (var result in ValidatePendingTransferAge())
            yield return result;

        // Data integrity edge cases
        foreach (var result in ValidateOrphanedAuditEntries())
            yield return result;

        foreach (var result in ValidateCrossReferenceConsistency())
            yield return result;

        // Professional standards edge cases
        foreach (var result in ValidateDestinationStateTransitions())
            yield return result;

        foreach (var result in ValidateDocumentAnomalies())
            yield return result;

        // Security edge cases
        foreach (var result in ValidateUserStatus())
            yield return result;

        // Performance awareness
        foreach (var result in ValidatePerformanceImpact())
            yield return result;
    }
}