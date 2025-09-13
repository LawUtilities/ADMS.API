using ADMS.Application.Common.Validation;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ADMS.Application.DTOs;

/// <summary>
/// Matter Document Activity User From DTO for source-side document transfer audit trail tracking.
/// </summary>
/// <remarks>
/// Represents the source side of document transfer operations between matters.
/// Links document transfer origins to users for accountability and compliance tracking.
/// </remarks>
public sealed record MatterDocumentActivityUserFromDto : IValidatableObject
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
        UserValidationHelper.IsValidUserId(MatterId) &&
        UserValidationHelper.IsValidUserId(DocumentId) &&
        UserValidationHelper.IsValidUserId(MatterDocumentActivityId) &&
        UserValidationHelper.IsValidUserId(UserId) &&
        UserValidationHelper.IsValidActivityTimestamp(CreatedAt);

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
            RequiredFieldsPresent = IsValid
        }
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the MatterDocumentActivityUserFromDto for data integrity and business rules compliance.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate core GUIDs using UserValidationHelper
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

        // Validate activity consistency for transfer operations
        if (MatterDocumentActivity != null && DocumentId != Guid.Empty)
        {
            foreach (var result in UserValidationHelper.ValidateActivityConsistency(
                UserId, DocumentId, MatterDocumentActivity.Activity, MatterId, "TransferActivity"))
                yield return result;
        }

        // Validate complete user activity record if navigation properties are loaded
        if (User != null && MatterDocumentActivity != null)
        {
            foreach (var result in UserValidationHelper.ValidateActivityRecord(
                UserId, User.Name, MatterId, "MATTER_DOCUMENT", MatterDocumentActivity.Activity, CreatedAt, "TransferActivity"))
                yield return result;
        }

        // Validate activity sequence if multiple activities are present
        if (MatterDocumentActivity == null || User == null) yield break;
        
        // Validate that the activity makes sense for the context
        if (MatterDocumentActivity.Activity == "MOVED" && Matter?.IsDeleted == true)
        {
            yield return new ValidationResult(
                "Cannot move documents from a deleted matter.",
                [nameof(MatterDocumentActivity), nameof(Matter)]);
        }

        // Validate source matter state transitions that could affect document transfers
        if (Matter == null) yield break;

        // Edge case: Source matter was archived DURING transfer operation
        if (Matter.IsArchived && GetTransferAgeDays() < 0.1) // Less than 2.4 hours
        {
            yield return new ValidationResult(
                "Source matter was archived shortly after document transfer initiation. " +
                "Verify transfer completion and matter lifecycle consistency.",
                [nameof(Matter)]);
        }

        // Edge case: Source matter being emptied (potential final document)
        if (Matter.IsArchived && !Matter.IsDeleted)
        {
            yield return new ValidationResult(
                "Document transferred from archived source matter. " +
                "Verify authorization for accessing archived matter documents.",
                [nameof(Matter)]);
        }

        // Edge case: Source matter marked for deletion
        if (Matter.IsDeleted)
        {
            yield return new ValidationResult(
                "Cannot transfer documents from deleted source matter. " +
                "Deleted matters should not allow document transfers.",
                [nameof(Matter)]);
        }

        // Validate edge cases specific to source-side document transfer operations
        foreach (var result in ValidateSourceSpecificEdgeCases())
            yield return result;
    }

    /// <summary>
    /// Validates edge cases specific to source-side document transfer operations.
    /// </summary>
    /// <returns>A collection of validation results for source-specific edge cases.</returns>
    private IEnumerable<ValidationResult> ValidateSourceSpecificEdgeCases()
    {
        // Source matter state transitions
        foreach (var result in ValidateSourceMatterStateTransitions())
            yield return result;

        // Document custody release validation
        foreach (var result in ValidateDocumentCustodyRelease())
            yield return result;

        // Source authority and permissions
        foreach (var result in ValidateSourceAuthorityPermissions())
            yield return result;

        // Document provenance from source
        foreach (var result in ValidateSourceDocumentProvenance())
            yield return result;

        // Source matter context validation
        foreach (var result in ValidateSourceMatterContext())
            yield return result;

        // Professional responsibility for source operations
        foreach (var result in ValidateSourceProfessionalResponsibility())
            yield return result;

        // Source activity pattern detection
        foreach (var result in ValidateSourceActivityPatterns())
            yield return result;
    }

    /// <summary>
    /// Validates document custody release from source matter perspective.
    /// /// </summary>
    /// <returns>A collection of validation results for custody release validation.</returns>
    private IEnumerable<ValidationResult> ValidateDocumentCustodyRelease()
    {
        if (Document == null || MatterDocumentActivity == null) yield break;

        // Edge case: Document was checked out at transfer time
        if (Document.IsCheckedOut && MatterDocumentActivity.Activity == "MOVED")
        {
            yield return new ValidationResult(
                "Document was checked out when MOVED from source matter. " +
                "Checked-out documents should be checked in before custody transfer to ensure version control integrity.",
                [nameof(Document), nameof(MatterDocumentActivity)]);
        }

        // Edge case: Last known copy being moved
        if (MatterDocumentActivity.Activity == "MOVED")
        {
            yield return new ValidationResult(
                "MOVED operation represents custody transfer from source matter. " +
                "Ensure proper authorization and client notification for document custody changes.",
                [nameof(MatterDocumentActivity)]);
        }

        // Edge case: Document with pending revisions
        if (Document.IsCheckedOut && MatterDocumentActivity.Activity == "COPIED")
        {
            yield return new ValidationResult(
                "Document with checked-out status being copied from source matter. " +
                "Verify that copied version reflects intended document state.",
                [nameof(Document)]);
        }
    }

    /// <summary>
    /// Validates source-side authority and permissions for document transfers.
    /// </summary>
    /// <returns>A collection of validation results for source authority validation.</returns>
    private IEnumerable<ValidationResult> ValidateSourceAuthorityPermissions()
    {
        if (User == null || Matter == null) yield break;

        // Edge case: Transfer during weekend hours
        if (IsWeekend(CreatedAt))
        {
            yield return new ValidationResult(
                "Document transfer from source matter during weekend hours. " +
                "Verify authorization for off-hours document management operations.",
                [nameof(CreatedAt)]);
        }

        // Edge case: Transfer outside business hours
        var transferHour = CreatedAt.Hour;
        if (transferHour is < 6 or > 22)
        {
            yield return new ValidationResult(
                $"Document transfer from source matter at {CreatedAt:HH:mm} (outside normal business hours). " +
                "Unusual timing may require additional authorization verification.",
                [nameof(CreatedAt)]);
        }

        // Edge case: Rapid succession transfers from same source
        var transferAge = GetTransferAgeDays();
        if (transferAge < 0.01) // Less than 14.4 minutes
        {
            yield return new ValidationResult(
                "Very recent document transfer from source matter. " +
                "Rapid succession transfers may indicate automated processing or potential data issues.",
                [nameof(CreatedAt)]);
        }
    }

    /// <summary>
    /// Validates document provenance and source authenticity.
    /// </summary>
    /// <returns>A collection of validation results for source provenance validation.</returns>
    private IEnumerable<ValidationResult> ValidateSourceDocumentProvenance()
    {
        if (Document == null) yield break;

        // Edge case: Document creation date after transfer date
        if (Document.CreationDate > CreatedAt)
        {
            yield return new ValidationResult(
                "Document creation date is after transfer timestamp. " +
                "This temporal inconsistency may indicate data corruption or clock synchronization issues.",
                [nameof(Document), nameof(CreatedAt)]);
        }

        // Edge case: Document with suspicious creation patterns
        if (Document.FileName.StartsWith("TEMP_", StringComparison.Ordinal) || 
            Document.FileName.Contains("BACKUP"))
        {
            yield return new ValidationResult(
                "Document with temporary or backup filename pattern being transferred from source. " +
                "Verify this is not a system-generated file being inadvertently processed.",
                [nameof(Document)]);
        }

        // Edge case: Zero-byte document transfer
        if (Document.FileSize == 0)
        {
            yield return new ValidationResult(
                "Zero-byte document being transferred from source matter. " +
                "Empty files may indicate upload or storage issues requiring investigation.",
                [nameof(Document)]);
        }
    }

    /// <summary>
    /// Validates source matter context and organizational integrity.
    /// </summary>
    /// <returns>A collection of validation results for source context validation.</returns>
    private IEnumerable<ValidationResult> ValidateSourceMatterContext()
    {
        if (Matter == null || Document == null) yield break;

        // Edge case: Confidential matter as source
        var matterDescription = Matter.Description.ToUpperInvariant();
        if (matterDescription.Contains("CONFIDENTIAL") || matterDescription.Contains("SEALED"))
        {
            yield return new ValidationResult(
                "Document transferred from confidential or sealed source matter. " +
                "Ensure proper security clearance and confidentiality protocols are maintained.",
                [nameof(Matter)]);
        }

        // Edge case: Client-specific matter as source
        if (matterDescription.Contains("PRIVILEGED") || matterDescription.Contains("ATTORNEY-CLIENT"))
        {
            yield return new ValidationResult(
                "Document transferred from privileged source matter. " +
                "Verify attorney-client privilege considerations and transfer authorization.",
                [nameof(Matter)]);
        }

        // Edge case: Cross-client document transfer
        if (!matterDescription.Contains("CLIENT") || Document.FileName.Contains("CLIENT")) yield break;
        var documentClientRef = ExtractClientReference(Document.FileName);
        var matterClientRef = ExtractClientReference(matterDescription);
            
        if (!string.IsNullOrEmpty(documentClientRef) && 
            !string.IsNullOrEmpty(matterClientRef) && 
            !documentClientRef.Equals(matterClientRef, StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Potential cross-client document transfer detected from source matter. " +
                "Verify client confidentiality and transfer authorization.",
                [nameof(Matter), nameof(Document)]);
        }
    }

    /// <summary>
    /// Validates professional responsibility requirements for source-side operations.
    /// </summary>
    /// <returns>A collection of validation results for professional responsibility compliance.</returns>
    private IEnumerable<ValidationResult> ValidateSourceProfessionalResponsibility()
    {
        if (MatterDocumentActivity?.Activity == "MOVED" && Matter != null)
        {
            yield return new ValidationResult(
                "MOVED operation removes document from source matter. " +
                "Ensure client notification and professional responsibility requirements are met for document custody transfer.",
                [nameof(MatterDocumentActivity), nameof(Matter)]);
        }

        // Edge case: High-value document transfer (large files may be important)
        if (Document is { FileSize: > 50 * 1024 * 1024 }) // 50MB
        {
            yield return new ValidationResult(
                $"Large document ({Document.FormattedFileSize}) being transferred from source matter. " +
                "High-value document transfers may require additional oversight and authorization.",
                [nameof(Document)]);
        }

        // Edge case: Recent source matter activity
        var sourceMatterAge = Matter != null ? (DateTime.UtcNow - Matter.CreationDate).TotalDays : 0;
        if (sourceMatterAge < 1) // Less than 1 day old
        {
            yield return new ValidationResult(
                "Document transferred from very recently created source matter. " +
                "Verify proper matter setup and organization before document transfers.",
                [nameof(Matter)]);
        }
    }

    /// <summary>
    /// Validates source-side activity patterns for anomaly detection.
    /// </summary>
    /// <returns>A collection of validation results for pattern analysis.</returns>
    private IEnumerable<ValidationResult> ValidateSourceActivityPatterns()
    {
        // Edge case: Transfer on exact hour boundaries (potential automation)
        if (CreatedAt is { Minute: 0, Second: 0 })
        {
            yield return new ValidationResult(
                "Document transfer from source occurred exactly on hour boundary. " +
                "This timing pattern may indicate automated processing requiring verification.",
                [nameof(CreatedAt)]);
        }

        // Edge case: User performing transfers from multiple sources simultaneously
        if (User?.Name.Contains("ADMIN", StringComparison.OrdinalIgnoreCase) == true ||
            User?.Name.Contains("SYSTEM", StringComparison.OrdinalIgnoreCase) == true)
        {
            yield return new ValidationResult(
                "Document transfer from source performed by administrative or system user. " +
                "Verify proper authorization and professional oversight for system-level operations.",
                [nameof(User)]);
        }

        // Edge case: Transfer timing patterns
        var dayOfWeek = CreatedAt.DayOfWeek;
        if (dayOfWeek == DayOfWeek.Sunday)
        {
            yield return new ValidationResult(
                "Document transfer from source matter on Sunday. " +
                "Weekend operations may require additional authorization verification.",
                [nameof(CreatedAt)]);
        }
    }

    private static string? ExtractClientReference(string text)
    {
        // Simple pattern matching for client references
        var patterns = new[] { @"CLIENT[\s_-]*([A-Z]+)", @"([A-Z]{2,})\s*CLIENT" };
        return (from pattern in patterns select Regex.Match(text, pattern, RegexOptions.IgnoreCase) into match where match.Success select match.Groups[1].Value).FirstOrDefault();
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

        switch (includeNavigationProperties)
        {
            case false:
                return dto;
            case true:
            {
                if (entity.MatterDocumentActivity != null)
                    dto = dto with
                    {
                        MatterDocumentActivity = MatterDocumentActivityDto.FromEntity(entity.MatterDocumentActivity)
                    };
                if (entity.Document != null)
                    dto = dto with { Document = DocumentWithoutRevisionsDto.FromEntity(entity.Document) };
                if (entity.User != null)
                    dto = dto with { User = UserDto.FromEntity(entity.User) };
                if (entity.Matter != null)
                    dto = dto with { Matter = MatterWithoutDocumentsDto.FromEntity(entity.Matter) };
                break;
            }
        }

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
                Console.WriteLine($"Warning: Skipped invalid source transfer audit entity: {ex.Message}");
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
        throw new ValidationException($"Failed to create valid source transfer audit entry: {errorMessages}");
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
                                       MatterDocumentActivity != null && User != null,
            ["ValidationStatus"] = IsValid
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

    /// <summary>
    /// Determines if this transfer activity requires bidirectional tracking.
    /// </summary>
    public bool RequiresBidirectionalTracking()
    {
        return UserValidationHelper.RequiresRelatedEntity(MatterDocumentActivity?.Activity);
    }

    #endregion Business Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterDocumentActivityUserFromDto is equal to the current instance.
    /// </summary>
    /// <param name="other">The MatterDocumentActivityUserFromDto to compare with the current instance.</param>
    /// <returns>True if the specified object is equal to the current instance; otherwise, false.</returns>
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
    public override int GetHashCode() =>
        HashCode.Combine(MatterId, DocumentId, MatterDocumentActivityId, UserId, CreatedAt);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the transfer, including the document, matter, user, and timestamp details.
    /// </summary>
    public override string ToString() =>
        $"Transfer FROM: Document ({DocumentId}) ← Matter ({MatterId}) by User ({UserId}) at {CreatedAt:yyyy-MM-dd HH:mm:ss}";

    #endregion String Representation

    /// <summary>
    /// Validates bidirectional audit trail completeness and consistency.
    /// </summary>
    /// <returns>A collection of validation results for bidirectional audit validation.</returns>
    private IEnumerable<ValidationResult> ValidateBidirectionalAuditTrailCompleteness()
    {
        // This would require service-level integration to verify corresponding TO entries exist
        if (MatterDocumentActivity?.Activity == "MOVED")
        {
            yield return new ValidationResult(
                "MOVED operations require corresponding destination-side audit trail entries. " +
                "Verify that MatterDocumentActivityUserToDto exists for complete bidirectional coverage.",
                [nameof(MatterDocumentActivity)]);
        }

        // Validate activity timing consistency
        if (RequiresBidirectionalTracking())
        {
            yield return new ValidationResult(
                "This transfer operation requires bidirectional tracking for legal compliance. " +
                "Ensure both source and destination audit entries are created.",
                [nameof(MatterDocumentActivity)]);
        }
    }

    /// <summary>
    /// Validates source matter state integrity for document releases.
    /// </summary>
    /// <returns>A collection of validation results for source matter state validation.</returns>
    private IEnumerable<ValidationResult> ValidateSourceMatterStateIntegrity()
    {
        if (Matter == null) yield break;

        // Enhanced source matter validation
        if (Matter.IsDeleted)
        {
            yield return new ValidationResult(
                "Cannot transfer documents from deleted source matter. " +
                "Source matter integrity is compromised for document custody operations.",
                [nameof(Matter)]);
        }

        // Validate source matter lifecycle state
        if (Matter.IsArchived && !MatterDocumentActivity?.Activity.Equals("MOVED", StringComparison.OrdinalIgnoreCase) == true)
        {
            yield return new ValidationResult(
                "Source matter is archived. Only MOVED operations should be performed on archived matters " +
                "to complete matter closure procedures.",
                [nameof(Matter)]);
        }

        // Professional practice validation for matter emptying
        var matterAge = (DateTime.UtcNow - Matter.CreationDate).TotalDays;
        if (matterAge < 1 && MatterDocumentActivity?.Activity == "MOVED")
        {
            yield return new ValidationResult(
                "Document being moved from very recently created source matter. " +
                "Verify proper matter setup and organization before document transfers.",
                [nameof(Matter), nameof(CreatedAt)]);
        }
    }

    /// <summary>
    /// Validates document chain of custody and professional handling requirements.
    /// </summary>
    /// <returns>A collection of validation results for chain of custody validation.</returns>
    private IEnumerable<ValidationResult> ValidateDocumentChainOfCustody()
    {
        if (Document == null) yield break;

        // Document custody integrity validation
        if (Document.IsCheckedOut && MatterDocumentActivity?.Activity == "MOVED")
        {
            yield return new ValidationResult(
                "Document is checked out and cannot be moved from source matter. " +
                "Complete check-in process before transferring document custody to maintain version control integrity.",
                [nameof(Document), nameof(MatterDocumentActivity)]);
        }

        // Document integrity validation at transfer time
        if (!Document.HasValidChecksum)
        {
            yield return new ValidationResult(
                "Document checksum validation failed at source transfer time. " +
                "Document integrity must be verified before releasing from source matter for legal compliance.",
                [nameof(Document)]);
        }

        // Professional document handling validation
        if (Document.FileSize == 0)
        {
            yield return new ValidationResult(
                "Zero-byte document cannot be transferred from source matter. " +
                "Empty files may indicate corruption or incomplete upload requiring investigation.",
                [nameof(Document)]);
        }
    }

    /// <summary>
    /// Validates user authorization and professional standards for source-side operations.
    /// </summary>
    /// <returns>A collection of validation results for user authorization validation.</returns>
    private IEnumerable<ValidationResult> ValidateUserAuthorizationForSourceOperations()
    {
        if (User == null) yield break;

        // Enhanced user validation using UserValidationHelper methods
        if (!UserValidationHelper.IsSufficientUserContext(UserId, User.Name, CreatedAt))
        {
            yield return new ValidationResult(
                "Insufficient user context for source-side document transfer operations. " +
                "Complete user attribution is required for professional accountability.",
                [nameof(User)]);
        }

        // Professional responsibility for document release
        if (MatterDocumentActivity?.Activity == "MOVED")
        {
            yield return new ValidationResult(
                "MOVED operation represents document custody release from source matter. " +
                "Ensure user has proper authorization and professional oversight for document custody changes.",
                [nameof(User), nameof(MatterDocumentActivity)]);
        }

        // Validate user activity patterns using UserValidationHelper
        foreach (var result in UserValidationHelper.ValidateUserAttribution(
            UserId, User.Name, CreatedAt, nameof(User)))
            yield return result;
    }

    /// <summary>
    /// Validates client confidentiality and matter context for source transfers.
    /// </summary>
    /// <returns>A collection of validation results for confidentiality validation.</returns>
    private IEnumerable<ValidationResult> ValidateClientConfidentialityAndMatterContext()
    {
        if (Matter == null || Document == null) yield break;

        var matterDescription = Matter.Description.ToUpperInvariant();
        var documentFileName = Document.FileName.ToUpperInvariant();

        // Enhanced confidentiality validation
        if (matterDescription.Contains("CONFIDENTIAL") || matterDescription.Contains("PRIVILEGED"))
        {
            yield return new ValidationResult(
                "Document being transferred from confidential or privileged source matter. " +
                "Ensure proper security clearance and maintain confidentiality protocols during transfer.",
                [nameof(Matter)]);
        }

        // Cross-client validation using extracted client references
        var documentClientRef = ExtractClientReference(documentFileName);
        var matterClientRef = ExtractClientReference(matterDescription);
        
        if (!string.IsNullOrEmpty(documentClientRef) && 
            !string.IsNullOrEmpty(matterClientRef) && 
            !documentClientRef.Equals(matterClientRef, StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Potential cross-client document transfer detected from source matter. " +
                "Verify client confidentiality boundaries and transfer authorization.",
                [nameof(Matter), nameof(Document)]);
        }

        // Professional practice validation for sensitive matters
        if (matterDescription.Contains("LITIGATION") && !documentFileName.Contains("PUBLIC"))
        {
            yield return new ValidationResult(
                "Document being transferred from litigation matter. " +
                "Verify privilege protection and discovery implications before transfer completion.",
                [nameof(Matter), nameof(Document)]);
        }
    }

    /// <summary>
    /// Validates transfer patterns for fraud detection and compliance monitoring.
    /// </summary>
    /// <returns>A collection of validation results for pattern analysis.</returns>
    private IEnumerable<ValidationResult> ValidateTransferPatternsAndFraudDetection()
    {
        // Temporal pattern analysis
        var transferTime = CreatedAt;
        var isWeekend = IsWeekend(transferTime);
        var isAfterHours = transferTime.Hour is < 6 or > 22;

        if (isWeekend && isAfterHours)
        {
            yield return new ValidationResult(
                "Document transfer from source matter during weekend off-hours. " +
                "Unusual timing may require additional oversight and authorization verification.",
                [nameof(CreatedAt)]);
        }

        // High-frequency pattern detection
        if (transferTime is { Minute: 0, Second: 0 })
        {
            yield return new ValidationResult(
                "Transfer from source occurred exactly on hour boundary. " +
                "This timing pattern may indicate automated processing requiring verification.",
                [nameof(CreatedAt)]);
        }

        // User pattern validation
        if (User?.Name.Contains("TEMP", StringComparison.OrdinalIgnoreCase) == true ||
            User?.Name.Contains("TEST", StringComparison.OrdinalIgnoreCase) == true)
        {
            yield return new ValidationResult(
                "Document transfer from source performed by temporary or test user account. " +
                "Verify proper authorization and production data handling procedures.",
                [nameof(User)]);
        }

        // Transfer velocity validation
        var transferAge = GetTransferAgeDays();
        if (transferAge < 0.001) // Less than ~1.4 minutes
        {
            yield return new ValidationResult(
                "Extremely rapid document transfer from source detected. " +
                "Verify transfer process integrity and prevent potential data corruption.",
                [nameof(CreatedAt)]);
        }
    }

    /// <summary>
    /// Determines whether the specified date falls on a weekend.
    /// </summary>
    /// <param name="transferTime">The date and time to evaluate.</param>
    /// <returns><see langword="true"/> if the specified date falls on a Saturday or Sunday; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="NotImplementedException"></exception>
    private static bool IsWeekend(DateTime transferTime)
    {
        // Returns true if the day is Saturday or Sunday
        return transferTime.DayOfWeek == DayOfWeek.Saturday || transferTime.DayOfWeek == DayOfWeek.Sunday;
    }

    /// <summary>
    /// Validates advanced activity record integrity and compliance requirements.
    /// </summary>
    /// <returns>A collection of validation results for advanced activity validation.</returns>
    private IEnumerable<ValidationResult> ValidateAdvancedActivityRecord()
    {
        // Use UserValidationHelper for enhanced activity sequence validation
        if (User?.MatterDocumentActivityUsersFrom != null)
        {
            var recentSourceTransfers = User.MatterDocumentActivityUsersFrom
                .Where(t => t.CreatedAt >= CreatedAt.AddMinutes(-UserValidationHelper.ActivityBurstWindowMinutes))
                .Select(t => t.CreatedAt);

            foreach (var result in UserValidationHelper.ValidateActivitySequence(
                recentSourceTransfers, CreatedAt, nameof(CreatedAt)))
                yield return result;
        }

        // Enhanced activity consistency validation
        if (MatterDocumentActivity != null)
        {
            foreach (var result in UserValidationHelper.ValidateActivityConsistency(
                UserId, DocumentId, MatterDocumentActivity.Activity, MatterId, 
                "SourceTransferActivity"))
                yield return result;
        }

        // Professional activity metrics validation
        foreach (var result in UserValidationHelper.ValidateActivityMetrics(
            1, 24.0, nameof(CreatedAt))) // This transfer in last 24 hours
            yield return result;
    }

    /// <summary>
    /// Validates source matter state transitions for document transfer operations.
    /// </summary>
    /// <returns>A collection of validation results for source matter state transitions.</returns>
    private IEnumerable<ValidationResult> ValidateSourceMatterStateTransitions()
    {
        // Example implementation: checks for archived or deleted state transitions
        if (Matter == null) yield break;

        if (Matter.IsDeleted)
        {
            yield return new ValidationResult(
                "Source matter is deleted. Document transfers from deleted matters are not allowed.",
                [nameof(Matter)]);
        }

        if (Matter.IsArchived && MatterDocumentActivity?.Activity != "MOVED")
        {
            yield return new ValidationResult(
                "Source matter is archived. Only MOVED operations should be performed on archived matters.",
                [nameof(Matter), nameof(MatterDocumentActivity)]);
        }

        // Add further state transition checks as needed
    }

    /// <summary>
    /// Validates comprehensive source-side specific requirements for enhanced compliance.
    /// </summary>
    /// <returns>A collection of validation results for comprehensive source validation.</returns>
    private IEnumerable<ValidationResult> ValidateComprehensiveSourceCompliance()
    {
        // Bidirectional audit trail validation
        foreach (var result in ValidateBidirectionalAuditTrailCompleteness())
            yield return result;

        // Source matter state integrity
        foreach (var result in ValidateSourceMatterStateIntegrity())
            yield return result;

        // Document chain of custody
        foreach (var result in ValidateDocumentChainOfCustody())
            yield return result;

        // User authorization for source operations
        foreach (var result in ValidateUserAuthorizationForSourceOperations())
            yield return result;

        // Client confidentiality and matter context
        foreach (var result in ValidateClientConfidentialityAndMatterContext())
            yield return result;

        // Transfer patterns and fraud detection
        foreach (var result in ValidateTransferPatternsAndFraudDetection())
            yield return result;

        // Advanced activity record validation
        foreach (var result in ValidateAdvancedActivityRecord())
            yield return result;

    }
}