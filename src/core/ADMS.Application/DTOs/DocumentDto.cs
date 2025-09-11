using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using ADMS.Application.Common.Validation;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Document Data Transfer Object representing complete document information with streamlined validation and professional document management capabilities.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of documents within the ADMS legal document management system,
/// using the standardized BaseValidationDto validation framework for consistency with other DTOs.
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item><strong>Streamlined Validation:</strong> Uses BaseValidationDto for consistent validation patterns</item>
/// <item><strong>Professional Standards:</strong> Enforces legal document management standards</item>
/// <item><strong>Version Control:</strong> Full support for document version control operations</item>
/// <item><strong>Audit Trail Integration:</strong> Complete audit trail support for legal compliance</item>
/// <item><strong>File System Integration:</strong> Professional file management with security validation</item>
/// </list>
/// 
/// <para><strong>Validation Hierarchy:</strong></para>
/// <list type="number">
/// <item><strong>Core Properties:</strong> File metadata, GUIDs, and required fields using validation helpers</item>
/// <item><strong>Business Rules:</strong> Document lifecycle, version control, and professional standards</item>
/// <item><strong>Cross-Property:</strong> MIME type consistency, revision relationships, and temporal validation</item>
/// <item><strong>Collections:</strong> Deep validation of revisions and activity audit trails</item>
/// </list>
/// </remarks>
public sealed class DocumentDto : BaseValidationDto, IEquatable<DocumentDto>, IComparable<DocumentDto>
{
    #region Private Fields

    private RevisionDto? _currentRevision;
    private IEnumerable<RevisionDto>? _revisionHistory;
    private DocumentTransferHistory? _transferHistory;
    private DocumentLifecycleAnalysis? _lifecycleAnalysis;
    private IReadOnlyDictionary<string, object>? _comprehensiveStatistics;

    #endregion Private Fields
        
    #region Core Document Properties

    /// <summary>
    /// Gets the unique identifier for this document within the ADMS legal document management system.
    /// </summary>
    [Required(ErrorMessage = "Document ID is required for document identification and system operations.")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the file name portion of the document without the extension.
    /// </summary>
    [Required(ErrorMessage = "File name is required and cannot be empty for document identification.")]
    [MaxLength(FileValidationHelper.MaxFileNameLength,
        ErrorMessage = "File name cannot exceed {1} characters for file system compatibility.")]
    public required string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the file extension that identifies the document format and content type.
    /// </summary>
    [Required(ErrorMessage = "File extension is required and cannot be empty for document format identification.")]
    [MaxLength(FileValidationHelper.MaxExtensionLength,
        ErrorMessage = "File extension cannot exceed {1} characters for system compatibility.")]
    public required string Extension { get; init; } = string.Empty;

    /// <summary>
    /// Gets the size of the document file in bytes.
    /// </summary>
    [Range(0, long.MaxValue, ErrorMessage = "File size must be non-negative for accurate storage tracking.")]
    public long FileSize { get; init; }

    /// <summary>
    /// Gets the MIME type that identifies the document's content format and handling requirements.
    /// </summary>
    [Required(ErrorMessage = "MIME type is required for proper document content identification and handling.")]
    [MaxLength(128, ErrorMessage = "MIME type cannot exceed 128 characters for system compatibility.")]
    [RegularExpression(@"^[\w\.\-]+\/[\w\.\-\+]+$",
        ErrorMessage = "MIME type must follow standard format (type/subtype) for content identification.")]
    public string MimeType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the cryptographic checksum for document content integrity verification and security compliance.
    /// </summary>
    [Required(ErrorMessage = "Checksum is required for document integrity verification and security compliance.")]
    [MaxLength(128, ErrorMessage = "Checksum cannot exceed 128 characters for cryptographic compatibility.")]
    [RegularExpression(@"^[A-Fa-f0-9]+$",
        ErrorMessage = "Checksum must be a hexadecimal string for cryptographic integrity verification.")]
    public string Checksum { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the document is currently checked out for editing in the version control system.
    /// </summary>
    public bool IsCheckedOut { get; init; }

    /// <summary>
    /// Gets a value indicating whether the document has been soft-deleted from the system.
    /// </summary>
    public bool IsDeleted { get; init; }

    /// <summary>
    /// Gets the UTC date and time when the document was initially created in the system.
    /// </summary>
    [Required(ErrorMessage = "Creation date is required for temporal tracking and audit compliance.")]
    public required DateTime CreationDate { get; init; }

    #endregion Core Document Properties

    #region Navigation Properties

    /// <summary>
    /// Gets the collection of document revisions representing the complete version history for this document.
    /// </summary>
    public ICollection<RevisionDto> Revisions { get; init; } = [];

    /// <summary>
    /// Gets the collection of document activity user associations representing the complete audit trail.
    /// </summary>
    public ICollection<DocumentActivityUserMinimalDto> DocumentActivityUsers { get; init; } = [];

    /// <summary>
    /// Gets the collection of source-side transfer audit entries for documents moved/copied FROM other matters.
    /// </summary>
    public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; init; } = [];

    /// <summary>
    /// Gets the collection of destination-side transfer audit entries for documents moved/copied TO other matters.
    /// </summary>
    public ICollection<MatterDocumentActivityUserToDto> MatterDocumentActivityUsersTo { get; init; } = [];

    #endregion Navigation Properties

    #region Computed Properties

    /// <summary>
    /// Gets the display text suitable for UI controls and document identification.
    /// </summary>
    public string DisplayText => $"{FileName}{Extension}";

    /// <summary>
    /// Gets the complete file name including extension.
    /// </summary>
    public string FullFileName => $"{FileName}{Extension}";

    /// <summary>
    /// Gets the formatted file size for user-friendly display.
    /// </summary>
    public string FormattedFileSize => FileValidationHelper.FormatFileSize(FileSize);

    /// <summary>
    /// Gets a value indicating whether this document has a valid checksum.
    /// </summary>
    public bool HasValidChecksum => FileValidationHelper.IsValidChecksum(Checksum);

    /// <summary>
    /// Gets the current status of the document based on its state flags.
    /// </summary>
    public string Status
    {
        get
        {
            if (IsDeleted)
                return "Deleted";
            return IsCheckedOut ? "Checked Out" : "Available";
        }
    }

    /// <summary>
    /// Gets the total count of all activities associated with this document.
    /// </summary>
    public int TotalActivityCount =>
        (DocumentActivityUsers?.Count ?? 0) +
        (MatterDocumentActivityUsersFrom?.Count ?? 0) +
        (MatterDocumentActivityUsersTo?.Count ?? 0);

    /// <summary>
    /// Gets a value indicating whether the document is available for editing.
    /// </summary>
    public bool IsAvailableForEdit => !IsDeleted && !IsCheckedOut;

    /// <summary>
    /// Gets the document age since creation.
    /// </summary>
    public TimeSpan DocumentAge => DateTime.UtcNow - CreationDate;

    /// <summary>
    /// Gets the current revision (highest numbered revision).
    /// </summary>
    public RevisionDto? CurrentRevision =>
        _currentRevision ??= Revisions?.OrderByDescending(r => r.RevisionNumber).FirstOrDefault();

    /// <summary>
    /// Gets the total number of revisions for this document.
    /// </summary>
    public int RevisionCount => Revisions?.Count ?? 0;

    /// <summary>
    /// Gets the revision history in chronological order.
    /// </summary>
    public IEnumerable<RevisionDto> RevisionHistory =>
        _revisionHistory ??= Revisions.OrderBy(r => r.RevisionNumber);

    /// <summary>
    /// Gets a value indicating whether this document has comprehensive activity history.
    /// </summary>
    public bool HasActivityHistory =>
        TotalActivityCount > 0 &&
        GetActivitiesByType("CREATED").Any() &&
        TotalActivityCount >= RevisionCount;

    /// <summary>
    /// Gets a value indicating whether this DTO is valid using quick validation.
    /// </summary>
    public bool IsValid =>
        UserValidationHelper.IsValidUserId(Id) &&
        FileValidationHelper.IsFileNameValid(FileName) &&
        FileValidationHelper.IsExtensionAllowed(Extension) &&
        FileValidationHelper.IsFileSizeValid(FileSize) &&
        FileValidationHelper.IsMimeTypeAllowed(MimeType) &&
        FileValidationHelper.IsValidChecksum(Checksum) &&
        UserValidationHelper.IsValidActivityTimestamp(CreationDate);

    #endregion Computed Properties

    #region BaseValidationDto Implementation

    /// <summary>
    /// Validates core document properties using ADMS validation helpers.
    /// </summary>
    protected override IEnumerable<ValidationResult> ValidateCoreProperties()
    {
        // Validate document ID
        foreach (var result in ValidateGuid(Id, nameof(Id)))
            yield return result;

        // Validate file name using FileValidationHelper
        foreach (var result in FileValidationHelper.ValidateFileName(FileName, nameof(FileName)))
            yield return result;

        // Validate extension using FileValidationHelper
        foreach (var result in FileValidationHelper.ValidateExtension(Extension, nameof(Extension)))
            yield return result;

        // Validate file size using FileValidationHelper
        foreach (var result in FileValidationHelper.ValidateFileSize(FileSize, nameof(FileSize)))
            yield return result;

        // Validate MIME type using FileValidationHelper
        foreach (var result in FileValidationHelper.ValidateMimeType(MimeType, nameof(MimeType)))
            yield return result;

        // Validate checksum using FileValidationHelper
        foreach (var result in FileValidationHelper.ValidateChecksum(Checksum, nameof(Checksum)))
            yield return result;

        // Validate creation date using BaseValidationDto helper
        foreach (var result in ValidateDateTime(CreationDate, nameof(CreationDate), allowFuture: false, allowPast: true, allowDefault: false))
            yield return result;
    }

    /// <summary>
    /// Validates document business rules and professional standards.
    /// </summary>
    protected override IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Version control business rules
        if (IsCheckedOut && IsDeleted)
        {
            yield return CreateValidationResult(
                "Document cannot be both checked out and deleted.",
                nameof(IsCheckedOut), nameof(IsDeleted));
        }

        // MIME type consistency with extension
        foreach (var result in FileValidationHelper.ValidateMimeTypeConsistency(MimeType, Extension, nameof(MimeType)))
            yield return result;

        // Professional standards validation
        if (!FileValidationHelper.IsLegalDocumentFormat(Extension))
        {
            yield return CreateValidationResult(
                $"Extension '{Extension}' is not a standard legal document format. Consider using PDF, DOCX, or other legal formats.",
                nameof(Extension));
        }

        // Document lifecycle validation
        if (IsDeleted && IsCheckedOut)
        {
            yield return CreateValidationResult(
                "Deleted documents cannot be in checked-out state. Check-in before deletion or restore before editing.",
                nameof(IsDeleted), nameof(IsCheckedOut));
        }
    }

    /// <summary>
    /// Validates cross-property relationships and constraints.
    /// </summary>
    protected override IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // Validate revision consistency
        if (RevisionCount > 0 && CurrentRevision != null && CurrentRevision.CreationDate < CreationDate)
        {
            yield return CreateValidationResult(
                "Current revision creation date cannot be before document creation date.",
                nameof(Revisions));
        }

        // Validate activity timeline consistency
        if (DocumentActivityUsers.Count > 0)
        {
            var creationActivity = DocumentActivityUsers
                .FirstOrDefault(a => string.Equals(a.DocumentActivity?.Activity, "CREATED", StringComparison.OrdinalIgnoreCase));
            
            if (creationActivity != null && creationActivity.CreatedAt < CreationDate)
            {
                yield return CreateValidationResult(
                    "Document creation activity cannot be before document creation date.",
                    nameof(DocumentActivityUsers));
            }
        }

        // Validate transfer audit consistency
        var fromTransfers = MatterDocumentActivityUsersFrom?.Count ?? 0;
        var toTransfers = MatterDocumentActivityUsersTo?.Count ?? 0;
        
        if ((fromTransfers > 0 || toTransfers > 0) && Math.Abs(fromTransfers - toTransfers) > 5)
        {
            // Ensure bidirectional tracking for transfers
            // Allow some variance for data migration scenarios
            yield return CreateValidationResult(
                "Transfer audit trail appears unbalanced. Verify bidirectional tracking is complete.",
                nameof(MatterDocumentActivityUsersFrom), nameof(MatterDocumentActivityUsersTo));
        }
    }

    /// <summary>
    /// Validates collections and nested objects.
    /// </summary>
    protected override IEnumerable<ValidationResult> ValidateCollections()
    {
        // Validate revisions collection
        if (Revisions.Count > 0)
        {
            var revisionNumbers = Revisions.Select(r => r.RevisionNumber).ToList();
            if (revisionNumbers.Distinct().Count() != revisionNumbers.Count)
            {
                yield return CreateValidationResult(
                    "Revision numbers must be unique within the document.",
                    nameof(Revisions));
            }

            // Validate sequential numbering
            var sortedNumbers = revisionNumbers.OrderBy(x => x).ToList();
            if (sortedNumbers.Where((t, i) => t != i + 1).Any())
            {
                yield return CreateValidationResult(
                    "Revision numbers should be sequential starting from 1 for proper version control.",
                    nameof(Revisions));
            }
        }

        // Validate document activities using helper validation
        if (DocumentActivityUsers.Count > 0)
        {
            if (DocumentActivityUsers.Any(a => a == null))
            {
                yield return CreateValidationResult(
                    "Document activity users collection cannot contain null items.",
                    nameof(DocumentActivityUsers));
            }

            // Validate required creation activity
            var hasCreationActivity = DocumentActivityUsers.Any(a => 
                string.Equals(a.DocumentActivity?.Activity, "CREATED", StringComparison.OrdinalIgnoreCase));
            
            if (!hasCreationActivity)
            {
                yield return CreateValidationResult(
                    "Document must have at least one CREATED activity for audit trail compliance.",
                    nameof(DocumentActivityUsers));
            }
        }

        // Validate transfer collections
        if (MatterDocumentActivityUsersFrom.Count > 0 && MatterDocumentActivityUsersFrom.Any(t => t == null))
        {
            yield return CreateValidationResult(
                "Matter document activity users from collection cannot contain null items.",
                nameof(MatterDocumentActivityUsersFrom));
        }

        if (MatterDocumentActivityUsersTo.Count == 0) yield break;
        if (MatterDocumentActivityUsersTo.Any(t => t == null))
        {
            yield return CreateValidationResult(
                "Matter document activity users to collection cannot contain null items.",
                nameof(MatterDocumentActivityUsersTo));
        }
    }

    /// <summary>
    /// Validates custom document-specific rules.
    /// </summary>
    protected override IEnumerable<ValidationResult> ValidateCustomRules()
    {
        // Security validation for file names
        if (!string.IsNullOrWhiteSpace(FileName))
        {
            var securityPatterns = new[] { "<script", "javascript:", "vbscript:", "<object", "<embed", "<iframe" };
            var fileNameLower = FileName.ToLowerInvariant();

            if (securityPatterns.Any(pattern => fileNameLower.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
            {
                yield return CreateValidationResult(
                    "File name contains potentially malicious content patterns.",
                    nameof(FileName));
            }
        }

        // Professional standards validation
        if (RevisionCount > 50)
        {
            yield return CreateValidationResult(
                "Document has an unusually high number of revisions (>50). Consider document consolidation.",
                nameof(Revisions));
        }

        // Document age validation
        if (DocumentAge.TotalDays > 3650) // 10 years
        {
            yield return CreateValidationResult(
                "Document is very old (>10 years). Verify retention policy compliance.",
                nameof(CreationDate));
        }

        // Activity validation
        if (TotalActivityCount > 1000)
        {
            yield return CreateValidationResult(
                "Document has an unusually high activity count (>1000). Verify data integrity.",
                nameof(DocumentActivityUsers));
        }
    }

    #endregion BaseValidationDto Implementation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this document can be deleted based on business rules.
    /// </summary>
    public bool CanBeDeleted() => !IsDeleted && !IsCheckedOut;

    /// <summary>
    /// Determines whether this document can be restored from deleted state.
    /// </summary>
    public bool CanBeRestored() => IsDeleted && HasValidChecksum && IsValid;

    /// <summary>
    /// Determines whether this document can be checked out for editing.
    /// </summary>
    public bool CanBeCheckedOut() => !IsCheckedOut && !IsDeleted;

    /// <summary>
    /// Determines whether this document can be checked in from editing.
    /// </summary>
    public bool CanBeCheckedIn() => IsCheckedOut && !IsDeleted;

    /// <summary>
    /// Determines whether the document represents a legal document format.
    /// </summary>
    public bool IsLegalDocument() => FileValidationHelper.IsLegalDocumentFormat(Extension);

    /// <summary>
    /// Gets activities of a specific type performed on this document.
    /// </summary>
    public IEnumerable<DocumentActivityUserMinimalDto> GetActivitiesByType([NotNull] string activityType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activityType);

        return DocumentActivityUsers?
            .Where(a => string.Equals(a.DocumentActivity.Activity, activityType, StringComparison.OrdinalIgnoreCase))
            .OrderBy(a => a.CreatedAt) ?? Enumerable.Empty<DocumentActivityUserMinimalDto>();
    }

    /// <summary>
    /// Gets the most recent activity performed on this document.
    /// </summary>
    public DocumentActivityUserMinimalDto? GetMostRecentActivity()
    {
        return DocumentActivityUsers?
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets comprehensive document metrics for reporting and analysis.
    /// </summary>
    public object GetDocumentMetrics() => new
    {
        FileInfo = new { FileName, Extension, FormattedFileSize, MimeType, IsLegalDocument = IsLegalDocument() },
        StatusInfo = new { Status, IsAvailableForEdit, HasValidChecksum },
        ActivityInfo = new { TotalActivityCount, RevisionCount, HasActivityHistory },
        LifecycleInfo = new { DocumentAge.TotalDays, CreationDate, IsValid }
    };

    #endregion Business Logic Methods

    #region Static Factory Methods

    /// <summary>
    /// Creates a DocumentDto from a Domain entity with enhanced validation.
    /// </summary>
    public static DocumentDto FromEntity(
        [NotNull] Domain.Entities.Document entity,
        bool includeRevisions = true,
        bool includeActivityUsers = true,
        bool includeMatterTransfers = true)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new DocumentDto
        {
            Id = entity.Id,
            FileName = entity.FileName,
            Extension = entity.Extension,
            FileSize = entity.FileSize,
            MimeType = entity.MimeType,
            Checksum = entity.Checksum,
            IsCheckedOut = entity.IsCheckedOut,
            IsDeleted = entity.IsDeleted,
            CreationDate = entity.CreatedDate,
            Revisions = includeRevisions && entity.Revisions?.Count > 0
                ? entity.Revisions.Select(r => RevisionDto.FromEntity(r)).ToList()
                : [],
            DocumentActivityUsers = includeActivityUsers && entity.DocumentActivityUsers?.Count > 0
                ? entity.DocumentActivityUsers.Select(da => DocumentActivityUserMinimalDto.FromEntity(da)).ToList()
                : [],
            MatterDocumentActivityUsersFrom = includeMatterTransfers && entity.MatterDocumentActivityUsersFrom?.Count > 0
                ? entity.MatterDocumentActivityUsersFrom.Select(mda => MatterDocumentActivityUserFromDto.FromEntity(mda)).ToList()
                : [],
            MatterDocumentActivityUsersTo = includeMatterTransfers && entity.MatterDocumentActivityUsersTo?.Count > 0
                ? entity.MatterDocumentActivityUsersTo.Select(mda => MatterDocumentActivityUserToDto.FromEntity(mda)).ToList()
                : []
        };

        var validationResults = ValidateModel(dto);
        if (!HasValidationErrors(validationResults)) return dto;
        var summary = GetValidationSummary(validationResults);
        var entityInfo = $"{entity.FileName}{entity.Extension}";
        throw new ValidationException($"DocumentDto creation failed for '{entityInfo}' ({entity.Id}): {summary}");
    }

    #endregion Static Factory Methods

    #region String Representation Methods

    /// <summary>
    /// Gets a professional summary of the document for client communication.
    /// </summary>
    public string GetProfessionalSummary()
    {
        var revisionInfo = "";
        if (RevisionCount > 0)
        {
            revisionInfo = $", {RevisionCount} revision";
            if (RevisionCount != 1)
            {
                revisionInfo += "s";
            }
        }

        var activityInfo = "";
        if (TotalActivityCount > 0)
        {
            activityInfo = $", {TotalActivityCount} " + (TotalActivityCount != 1 ? "activities" : "activity");
        }

        return $"{DisplayText} ({FormattedFileSize}, {Status}{revisionInfo}{activityInfo})";
    }

    /// <summary>
    /// Gets a detailed description of the document for professional reporting.
    /// </summary>
    public string GetDetailedDescription()
    {
        var integrityStatus = HasValidChecksum ? "integrity verified" : "integrity unverified";
        string revisionInfo;
        if (RevisionCount > 0)
        {
            revisionInfo = $"{RevisionCount} revision" + (RevisionCount != 1 ? "s" : "");
        }
        else
        {
            revisionInfo = "no revisions";
        }
        string activityInfo;
        if (TotalActivityCount > 0)
        {
            activityInfo = $"{TotalActivityCount} total activit" + (TotalActivityCount != 1 ? "ies" : "y");
        }
        else
        {
            activityInfo = "no activities";
        }
        var ageInfo = $"{DocumentAge.TotalDays:F0} days old";

        return $"Document '{DisplayText}' (ID: {Id}) created on {CreationDate:yyyy-MM-dd}, " +
               $"size {FormattedFileSize}, {Extension.TrimStart('.')} format, status {Status}, " +
               $"{revisionInfo}, {activityInfo}, {ageInfo}, {integrityStatus}";
    }

    #endregion String Representation Methods

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified DocumentDto is equal to the current instance.
    /// </summary>
    public bool Equals(DocumentDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current DocumentDto.
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as DocumentDto);

    /// <summary>
    /// Returns a hash code for the current DocumentDto.
    /// </summary>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Compares the current DocumentDto with another DocumentDto for ordering.
    /// </summary>
    public int CompareTo(DocumentDto? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        var displayComparison = string.Compare(DisplayText, other.DisplayText, StringComparison.OrdinalIgnoreCase);
        if (displayComparison != 0) return displayComparison;

        var dateComparison = CreationDate.CompareTo(other.CreationDate);
        return dateComparison != 0 ? dateComparison : Id.CompareTo(other.Id);
    }

    /// <summary>
    /// Returns a string representation of the document.
    /// </summary>
    public override string ToString() =>
        $"{DisplayText} ({Id}) [{Status}] - {FormattedFileSize}";

    /// <summary>
    /// Determines whether two <see cref="DocumentDto"/> instances are equal.
    /// </summary>
    /// <remarks>Two <see cref="DocumentDto"/> instances are considered equal if they are both <see
    /// langword="null"/>,  or if they are not <see langword="null"/> and their <see cref="Equals(object?)"/> method
    /// returns <see langword="true"/>.</remarks>
    /// <param name="left">The first <see cref="DocumentDto"/> instance to compare, or <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="DocumentDto"/> instance to compare, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the specified <see cref="DocumentDto"/> instances are equal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator ==(DocumentDto? left, DocumentDto? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="DocumentDto"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="DocumentDto"/> instance to compare, or <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="DocumentDto"/> instance to compare, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the specified <see cref="DocumentDto"/> instances are not equal; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator !=(DocumentDto? left, DocumentDto? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Determines whether one <see cref="DocumentDto"/> instance is less than another.
    /// </summary>
    /// <remarks>If <paramref name="left"/> is <see langword="null"/>, the result is <see langword="true"/>
    /// unless <paramref name="right"/> is also <see langword="null"/>. If both instances are non-null, the comparison
    /// is based on the result of <see cref="DocumentDto.CompareTo(DocumentDto)"/>.</remarks>
    /// <param name="left">The first <see cref="DocumentDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="DocumentDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator <(DocumentDto? left, DocumentDto? right)
    {
        if (left is null) return right is not null;
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    /// Determines whether one <see cref="DocumentDto"/> instance is less than or equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="DocumentDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="DocumentDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise,
    /// <see langword="false"/>. If <paramref name="left"/> is <see langword="null"/>, the result is always <see
    /// langword="true"/>.</returns>
    public static bool operator <=(DocumentDto? left, DocumentDto? right)
    {
        if (left is null) return true;
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// Determines whether one <see cref="DocumentDto"/> instance is greater than another.
    /// </summary>
    /// <remarks>If <paramref name="left"/> is <see langword="null"/>, the operator returns <see
    /// langword="false"/>. If <paramref name="right"/> is <see langword="null"/>, the comparison is based solely on
    /// <paramref name="left"/>.</remarks>
    /// <param name="left">The first <see cref="DocumentDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="DocumentDto"/> instance to compare. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator >(DocumentDto? left, DocumentDto? right)
    {
        if (left is null) return false;
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    /// Determines whether the left <see cref="DocumentDto"/> is greater than or equal to the right <see
    /// cref="DocumentDto"/>.
    /// </summary>
    /// <remarks>This operator uses the <see cref="DocumentDto.CompareTo"/> method to perform the comparison. 
    /// If <paramref name="left"/> is <see langword="null"/>, the result is <see langword="true"/> only if <paramref
    /// name="right"/> is also <see langword="null"/>.</remarks>
    /// <param name="left">The first <see cref="DocumentDto"/> to compare. Can be <see langword="null"/>.</param>
    /// <param name="right">The second <see cref="DocumentDto"/> to compare. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; 
    /// otherwise, <see langword="false"/>. If both are <see langword="null"/>, the result is <see langword="true"/>.</returns>
    public static bool operator >=(DocumentDto? left, DocumentDto? right)
    {
        if (left is null) return right is null;
        return left.CompareTo(right) >= 0;
    }

    #endregion Equality and Comparison
}

#region Supporting Data Classes

/// <summary>
/// Represents comprehensive document transfer history analysis.
/// </summary>
public class DocumentTransferHistory
{
    public required Guid DocumentId { get; init; }
    public required string DocumentName { get; init; }
    public required int SourceTransferCount { get; init; }
    public required int DestinationTransferCount { get; init; }
    public required int TotalTransferCount { get; init; }
    public required int UniqueTransferUsers { get; init; }
    public required int RecentTransferCount { get; init; }
    public required bool HasTransferActivity { get; init; }
    public required bool IsFrequentlyTransferred { get; init; }
    public DateTime? LastTransferDate { get; init; }
    public required int TransferBalance { get; init; }
    public required bool IsWellDistributed { get; init; }
    public required bool HasComprehensiveTransferAudit { get; init; }
    public required bool TransferAuditCompliance { get; init; }

    public string GetProfessionalAssessment()
    {
        if (!HasTransferActivity)
            return "Document has remained in its original matter - stable document placement.";

        if (IsFrequentlyTransferred && !IsWellDistributed)
            return "Document shows frequent transfer activity with imbalanced patterns - review organization strategy.";

        if (HasComprehensiveTransferAudit && TransferAuditCompliance)
            return "Document transfer history shows good audit trail compliance and professional management.";

        return !TransferAuditCompliance ? "Transfer audit trail may be incomplete - verify all transfers are properly documented." : "Document transfer patterns appear normal with adequate audit trail coverage.";
    }
}

/// <summary>
/// Represents comprehensive document lifecycle analysis.
/// </summary>
public class DocumentLifecycleAnalysis
{
    public required Guid DocumentId { get; init; }
    public required string DocumentName { get; init; }
    public required DateTime CreationDate { get; init; }
    public required string CreatedByUser { get; init; }
    public required string CurrentStatus { get; init; }
    public required string LifecycleStage { get; init; }
    public required int ModificationCount { get; init; }
    public required int CheckOutCount { get; init; }
    public required int CheckInCount { get; init; }
    public required int DeleteCount { get; init; }
    public required int RestoreCount { get; init; }
    public required int UniqueModifyingUsers { get; init; }
    public required int TotalUniqueUsers { get; init; }
    public required bool VersionControlCompliance { get; init; }
    public required bool HasBalancedVersionControl { get; init; }
    public required bool IsActivelyManaged { get; init; }
    public required TimeSpan DocumentAge { get; init; }
    public DateTime? LastActivityDate { get; init; }
    public required bool HasComprehensiveAuditTrail { get; init; }
    public required bool AuditTrailCompliance { get; init; }

    public string GetProfessionalRecommendations()
    {
        var recommendations = new List<string>();

        if (!IsActivelyManaged && DocumentAge.TotalDays > 365)
            recommendations.Add("Consider archiving - document has been inactive for over a year.");

        if (!VersionControlCompliance)
            recommendations.Add("Review version control practices - check-in/check-out operations are imbalanced.");

        if (!HasComprehensiveAuditTrail)
            recommendations.Add("Audit trail may be incomplete - verify all activities are properly recorded.");

        if (recommendations.Count == 0)
            recommendations.Add("Document lifecycle management meets professional standards.");

        return string.Join(" ", recommendations);
    }
}

#endregion Supporting Data Classes