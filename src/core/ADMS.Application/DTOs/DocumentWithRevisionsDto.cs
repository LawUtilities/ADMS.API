using ADMS.Application.Common;
using ADMS.Application.Common.Validation;
using ADMS.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing a document with its complete revision history and associated audit trail relationships.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of a document within the ADMS legal document management system,
/// corresponding to <see cref="ADMS.Domain.Entities.Document"/>. It provides comprehensive document data including
/// complete revision collections and all audit trail associations for legal compliance and professional accountability.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Document Representation:</strong> Full representation of document with all relationships</item>
/// <item><strong>Revision History Inclusion:</strong> Complete revision history for version control and audit trails</item>
/// <item><strong>Professional Validation:</strong> Uses centralized FileValidationHelper for comprehensive data integrity</item>
/// <item><strong>Entity Synchronization:</strong> Mirrors all properties and relationships from ADMS.Domain.Entities.Document</item>
/// <item><strong>Legal Compliance Support:</strong> Designed for comprehensive audit reporting and legal compliance</item>
/// </list>
/// 
/// <para><strong>Performance Optimizations:</strong></para>
/// <list type="bullet">
/// <item><strong>Cached Computed Properties:</strong> Expensive calculations are cached to improve performance</item>
/// <item><strong>Lazy Collection Evaluation:</strong> Collections are evaluated only when accessed</item>
/// <item><strong>Optimized Validation:</strong> Validation uses early termination and efficient patterns</item>
/// <item><strong>Memory Efficient:</strong> Uses collection expressions and optimized LINQ operations</item>
/// </list>
/// </remarks>
public sealed partial class DocumentWithRevisionsDto : IValidatableObject, IEquatable<DocumentWithRevisionsDto>
{
    #region Cached Values
    
    private RevisionDto? _currentRevision;
    private bool _currentRevisionCached;
    private IEnumerable<RevisionDto>? _revisionHistory;
    private string? _formattedFileSize;
    private bool? _hasValidChecksum;
    
    #endregion Cached Values

    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the document.
    /// </summary>
    [Required(ErrorMessage = "Document ID is required for document identification.")]
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the file name of the document.
    /// </summary>
    /// <remarks>
    /// The file name serves as the primary human-readable identifier for the document and must conform 
    /// to professional file naming conventions. This field is validated using FileValidationHelper 
    /// to ensure compliance with file system requirements and professional standards.
    /// </remarks>
    [Required(ErrorMessage = "File name is required and cannot be empty.")]
    [StringLength(FileValidationHelper.MaxFileNameLength,
        ErrorMessage = "File name cannot exceed {1} characters.")]
    [JsonPropertyName("fileName")]
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets the file extension of the document.
    /// </summary>
    /// <remarks>
    /// The file extension identifies the document format and must be one of the allowed extensions 
    /// for the ADMS system. This field is validated using FileValidationHelper to ensure only 
    /// approved file types are accepted for legal document management.
    /// </remarks>
    [Required(ErrorMessage = "File extension is required and cannot be empty.")]
    [StringLength(FileValidationHelper.MaxExtensionLength,
        ErrorMessage = "File extension cannot exceed {1} characters.")]
    [JsonPropertyName("extension")]
    public required string Extension { get; set; }

    /// <summary>
    /// Gets or sets the size of the file in bytes.
    /// </summary>
    [Range(0, long.MaxValue, ErrorMessage = "File size must be non-negative.")]
    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the file (e.g., "application/pdf").
    /// </summary>
    [Required(ErrorMessage = "MIME type is required for proper document handling.")]
    [StringLength(128, ErrorMessage = "MIME type cannot exceed 128 characters.")]
    [RegularExpression(@"^[\w\.\-]+\/[\w\.\-\+]+$", ErrorMessage = "Invalid MIME type format.")]
    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the checksum (SHA256 hash) of the file for integrity verification.
    /// </summary>
    [Required(ErrorMessage = "Checksum is required for document integrity verification.")]
    [StringLength(128, ErrorMessage = "Checksum cannot exceed 128 characters.")]
    [RegularExpression(@"^[A-Fa-f0-9]+$", ErrorMessage = "Checksum must be a hexadecimal string.")]
    [JsonPropertyName("checksum")]
    public string Checksum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the document is checked out.
    /// </summary>
    [JsonPropertyName("isCheckedOut")]
    public bool IsCheckedOut { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document is deleted.
    /// </summary>
    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time of the document in UTC.
    /// </summary>
    [Required(ErrorMessage = "Creation date is required for temporal tracking and audit compliance.")]
    [JsonPropertyName("creationDate")]
    public required DateTime CreationDate { get; init; }

    #endregion Core Properties

    #region Collection Properties

    /// <summary>
    /// Gets or sets the collection of revisions for this document.
    /// </summary>
    [JsonPropertyName("revisions")]
    public ICollection<RevisionDto> Revisions { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of document activity users.
    /// </summary>
    [JsonPropertyName("documentActivityUsers")]
    public ICollection<DocumentActivityUserDto> DocumentActivityUsers { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of "from" matter document activity users.
    /// </summary>
    [JsonPropertyName("matterDocumentActivityUsersFrom")]
    public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of "to" matter document activity users.
    /// </summary>
    [JsonPropertyName("matterDocumentActivityUsersTo")]
    public ICollection<MatterDocumentActivityUserToDto> MatterDocumentActivityUsersTo { get; set; } = [];

    #endregion Collection Properties

    #region Computed Properties

    /// <summary>
    /// Gets the current (latest) revision of the document.
    /// </summary>
    /// <remarks>
    /// This computed property provides quick access to the most recent revision, which is typically 
    /// the active version of the document. The current revision is determined by the highest 
    /// revision number in the collection.
    /// 
    /// <para><strong>Version Control Integration:</strong></para>
    /// The current revision represents the latest state of the document and is used for most 
    /// document operations and displays.
    /// </remarks>
    /// <example>
    /// <code>
    /// var latestRevision = document.CurrentRevision;
    /// if (latestRevision != null)
    /// {
    ///     Console.WriteLine($"Current version: {latestRevision.RevisionNumber}");
    /// }
    /// </code>
    /// </example>
    public RevisionDto? CurrentRevision
    {
        get
        {
            if (_currentRevisionCached)
                return _currentRevision;

            _currentRevision = Revisions
                .Where(r => r is not null)
                .MaxBy(r => r.RevisionNumber);
            
            _currentRevisionCached = true;
            return _currentRevision;
        }
    }

    /// <summary>
    /// Gets the total count of revisions for this document.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.Domain.Entities.Document.RevisionCount"/> and provides 
    /// a quick count of document versions, useful for version control analysis and display.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Document has {document.RevisionCount} versions");
    /// </code>
    /// </example>
    public int RevisionCount => Revisions.Count;

    /// <summary>
    /// Gets the complete revision history ordered chronologically.
    /// </summary>
    /// <remarks>
    /// This method provides a chronologically ordered view of all revisions, 
    /// useful for displaying version history and tracking document evolution.
    /// </remarks>
    /// <example>
    /// <code>
    /// var history = document.GetRevisionHistory();
    /// foreach (var revision in history)
    /// {
    ///     Console.WriteLine($"Version {revision.RevisionNumber}: {revision.CreationDate}");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<RevisionDto> GetRevisionHistory()
    {
        return Revisions
            .Where(r => r is not null)
            .OrderBy(r => r.RevisionNumber)
            .ToList();
    }

    /// <summary>
    /// Gets the total count of all activities (document + transfer) for this document.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.Domain.Entities.Document.TotalActivityCount"/> and provides 
    /// a comprehensive count of all activities associated with the document, useful for activity analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Total document activities: {document.TotalActivityCount}");
    /// </code>
    /// </example>
    public int TotalActivityCount => DocumentActivityUsers.Count +
                                   MatterDocumentActivityUsersFrom.Count +
                                   MatterDocumentActivityUsersTo.Count;

    /// <summary>
    /// Gets a value indicating whether this document has any activities recorded.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.Domain.Entities.Document.HasActivities"/> and 
    /// determines if the document has been used in the system, useful for activity analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.HasActivities)
    /// {
    ///     Console.WriteLine("Document has activity history");
    /// }
    /// </code>
    /// </example>
    public bool HasActivities => TotalActivityCount > 0;

    /// <summary>
    /// Gets a value indicating whether the document is available for editing.
    /// </summary>
    /// <remarks>
    /// This computed property determines document availability based on check-out and deletion status, 
    /// useful for UI controls and business logic decisions.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.IsAvailableForEdit)
    /// {
    ///     // Enable editing controls
    /// }
    /// </code>
    /// </example>
    public bool IsAvailableForEdit => !IsCheckedOut && !IsDeleted;

    /// <summary>
    /// Gets the document status as a human-readable string.
    /// </summary>
    /// <remarks>
    /// This computed property provides a descriptive status based on the document's current state, 
    /// useful for UI display and reporting.
    /// </remarks>
    /// <example>
    /// <code>
    /// var status = document.Status; // "Active", "Checked Out", "Deleted", etc.
    /// </code>
    /// </example>
    public string Status => (IsDeleted, IsCheckedOut) switch
    {
        (true, _) => "Deleted",
        (false, true) => "Checked Out",
        (false, false) => "Active",
    };

    /// <summary>
    /// Gets a value indicating whether the checksum appears to be valid.
    /// </summary>
    /// <remarks>
    /// This computed property performs basic checksum validation to verify it's a properly 
    /// formatted SHA256 hash, useful for integrity verification display.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (document.HasValidChecksum)
    /// {
    ///     Console.WriteLine("Document integrity verified");
    /// }
    /// </code>
    /// </example>
    public bool HasValidChecksum
    {
        get
        {
            if (_hasValidChecksum.HasValue)
                return _hasValidChecksum.Value;

            _hasValidChecksum = !string.IsNullOrWhiteSpace(Checksum) &&
                              ChecksumRegex().IsMatch(Checksum) &&
                              Checksum.Length == 64;

            return _hasValidChecksum.Value;
        }
    }

    /// <summary>
    /// Gets the file size formatted as a human-readable string.
    /// </summary>
    /// <remarks>
    /// This computed property formats the file size in appropriate units (bytes, KB, MB, GB) 
    /// for user-friendly display.
    /// </remarks>
    /// <example>
    /// <code>
    /// var sizeText = document.FormattedFileSize; // "2.4 MB"
    /// </code>
    /// </example>
    public string FormattedFileSize
    {
        get
        {
            if (_formattedFileSize is not null)
                return _formattedFileSize;

            _formattedFileSize = FileSize switch
            {
                < 1024 => $"{FileSize} bytes",
                < 1024 * 1024 => $"{FileSize / 1024.0:F1} KB",
                < 1024 * 1024 * 1024 => $"{FileSize / (1024.0 * 1024.0):F1} MB",
                _ => $"{FileSize / (1024.0 * 1024.0 * 1024.0):F1} GB"
            };

            return _formattedFileSize;
        }
    }

    /// <summary>
    /// Gets the display text suitable for UI controls and document identification.
    /// </summary>
    /// <remarks>
    /// Provides a consistent format for displaying document information in UI elements, 
    /// including the file name for clear identification.
    /// </remarks>
    /// <example>
    /// <code>
    /// var displayText = document.DisplayText; // Returns the file name
    /// documentDropdown.Items.Add(new ListItem(document.DisplayText, document.Id.ToString()));
    /// </code>
    /// </example>
    public string DisplayText => FileName;

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the DocumentWithRevisionsDto using the standardized ADMS validation hierarchy.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Implements comprehensive validation following the standardized ADMS validation hierarchy:
    /// <list type="number">
    /// <item><strong>Core Properties:</strong> Essential document properties using FileValidationHelper</item>
    /// <item><strong>Business Rules:</strong> Document lifecycle, version control, and professional standards</item>
    /// <item><strong>Cross-Property:</strong> MIME type consistency, temporal validation, and referential integrity</item>
    /// <item><strong>Collections:</strong> Deep validation of revision and activity audit trail collections</item>
    /// <item><strong>Revision-Specific:</strong> Version control integrity and revision history validation</item>
    /// </list>
    /// </remarks>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);

        // 1. Core Properties Validation
        foreach (var result in ValidateCoreProperties())
            yield return result;

        // 2. Business Rules Validation 
        foreach (var result in ValidateBusinessRules())
            yield return result;

        // 3. Cross-Property Validation
        foreach (var result in ValidateCrossPropertyRules())
            yield return result;
        
        // 4. Collections Validation
        foreach (var result in ValidateCollections())
            yield return result;

        // 5. Document Classification Validation
        foreach (var result in ValidateDocumentClassification())
            yield return result;
    }

    /// <summary>
    /// Validates core document properties.
    /// </summary>
    /// <returns>A collection of validation results for core properties.</returns>
    private IEnumerable<ValidationResult> ValidateCoreProperties()
    {
        // Document ID validation
        if (Id == Guid.Empty)
        {
            yield return new ValidationResult(
                "Document ID must be a valid non-empty GUID for document identification.",
                [nameof(Id)]);
        }

        // File name validation
        foreach (var result in ValidateFileName())
            yield return result;

        // Extension validation
        foreach (var result in ValidateExtension())
            yield return result;

        // File size validation
        foreach (var result in ValidateFileSize())
            yield return result;

        // MIME type validation
        foreach (var result in ValidateMimeType())
            yield return result;

        // Checksum validation
        foreach (var result in ValidateChecksum())
            yield return result;

        // Creation date validation
        foreach (var result in ValidateCreationDate())
            yield return result;
    }

    /// <summary>
    /// Validates business rules specific to documents with revisions.
    /// </summary>
    /// <returns>A collection of validation results for business rule compliance.</returns>
    private IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Documents must have at least one revision
        if (Revisions.Count == 0)
        {
            yield return new ValidationResult(
                "Documents must have at least one revision for version control integrity.",
                [nameof(Revisions)]);
            yield break; // Critical error - stop validation
        }

        // Document cannot be both checked out and deleted
        if (IsCheckedOut && IsDeleted)
        {
            yield return new ValidationResult(
                "A document cannot be both checked out and deleted simultaneously.",
                [nameof(IsCheckedOut), nameof(IsDeleted)]);
        }

        // Version control business rules
        var maxRevisionNumber = Revisions.Max(r => r.RevisionNumber);
        if (maxRevisionNumber != Revisions.Count)
        {
            yield return new ValidationResult(
                "Revision numbering must be sequential starting from 1 for version control integrity.",
                [nameof(Revisions)]);
        }

        // Professional version control limits
        if (Revisions.Count > RevisionValidationHelper.MaxReasonableRevisionCount)
        {
            yield return new ValidationResult(
                $"Document has an unusually high revision count (>{RevisionValidationHelper.MaxReasonableRevisionCount}). " +
                "Verify version control practices and consider consolidation.",
                [nameof(Revisions)]);
        }

        // Current revision must exist and be valid
        if (CurrentRevision is null)
        {
            yield return new ValidationResult(
                "Document with revisions must have a valid current revision.",
                [nameof(Revisions)]);
        }

        // Deleted documents should have proper audit trail
        if (IsDeleted && !HasValidAuditTrail())
        {
            yield return new ValidationResult(
                "Deleted documents must maintain comprehensive audit trails for legal compliance.",
                [nameof(IsDeleted), nameof(DocumentActivityUsers)]);
        }
    }

    /// <summary>
    /// Validates cross-property relationships and constraints.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    private IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // MIME type and extension consistency
        if (!string.IsNullOrWhiteSpace(Extension) && !string.IsNullOrWhiteSpace(MimeType))
        {
            var expectedMimeTypes = GetExpectedMimeTypesForExtension(Extension.ToLowerInvariant());
            if (expectedMimeTypes.Any() && !expectedMimeTypes.Contains(MimeType, StringComparer.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    $"MIME type '{MimeType}' is inconsistent with file extension '{Extension}'. " +
                    $"Expected: {string.Join(" or ", expectedMimeTypes)}",
                    [nameof(MimeType), nameof(Extension)]);
            }
        }

        // Professional file format validation
        if (!string.IsNullOrWhiteSpace(Extension) && !FileValidationHelper.IsLegalDocumentFormat(Extension))
        {
            yield return new ValidationResult(
                $"Extension '{Extension}' is not a standard legal document format. " +
                "Consider using PDF, DOCX, or other approved legal document formats.",
                [nameof(Extension)]);
        }
    }

    /// <summary>
    /// Validates collections and their contents.
    /// </summary>
    /// <returns>A collection of validation results for collection validation.</returns>
    private IEnumerable<ValidationResult> ValidateCollections()
    {
        // Revisions collection validation
        foreach (var result in ValidateRevisions())
            yield return result;

        // Document activity users validation
        foreach (var result in DtoValidationHelper.ValidateCollection(DocumentActivityUsers, nameof(DocumentActivityUsers)))
            yield return result;

        // Transfer activities validation
        foreach (var result in DtoValidationHelper.ValidateCollection(MatterDocumentActivityUsersFrom, nameof(MatterDocumentActivityUsersFrom)))
            yield return result;

        foreach (var result in DtoValidationHelper.ValidateCollection(MatterDocumentActivityUsersTo, nameof(MatterDocumentActivityUsersTo)))
            yield return result;
    }

    /// <summary>
    /// Validates document classification and type-specific rules.
    /// </summary>
    /// <returns>A collection of validation results for document classification.</returns>
    private IEnumerable<ValidationResult> ValidateDocumentClassification()
    {
        // File size reasonable limits based on document type
        var maxSizeForType = Extension.ToLowerInvariant() switch
        {
            "pdf" => 50 * 1024 * 1024,  // 50MB for PDFs
            "docx" or "doc" => 25 * 1024 * 1024,  // 25MB for Word docs
            "xlsx" or "xls" => 15 * 1024 * 1024,  // 15MB for Excel
            "txt" => 5 * 1024 * 1024,   // 5MB for text files
            _ => 100 * 1024 * 1024       // 100MB default
        };

        if (FileSize > maxSizeForType)
        {
            yield return new ValidationResult(
                $"File size ({FormattedFileSize}) exceeds recommended limit for {Extension.ToUpperInvariant()} files.",
                [nameof(FileSize)]);
        }
    }

    /// <summary>
    /// Validates the file name property.
    /// </summary>
    /// <returns>A collection of validation results for the file name.</returns>
    private IEnumerable<ValidationResult> ValidateFileName()
    {
        if (string.IsNullOrWhiteSpace(FileName))
        {
            yield return new ValidationResult("File name is required and cannot be empty.", [nameof(FileName)]);
            yield break;
        }

        if (!FileValidationHelper.IsFileNameValid(FileName))
        {
            yield return new ValidationResult(
                "File name contains invalid characters or format.",
                [nameof(FileName)]);
        }

        if (!FileName.Any(char.IsLetterOrDigit))
        {
            yield return new ValidationResult(
                "File name must contain at least one alphanumeric character for professional identification.",
                [nameof(FileName)]);
        }
    }

    /// <summary>
    /// Validates the file extension property.
    /// </summary>
    /// <returns>A collection of validation results for the file extension.</returns>
    private IEnumerable<ValidationResult> ValidateExtension()
    {
        if (string.IsNullOrWhiteSpace(Extension))
        {
            yield return new ValidationResult("File extension is required and cannot be empty.", [nameof(Extension)]);
            yield break;
        }

        if (!FileValidationHelper.IsExtensionAllowed(Extension))
        {
            yield return new ValidationResult(
                $"Extension '{Extension}' is not allowed for legal document management. " +
                $"Allowed extensions: {string.Join(", ", FileValidationHelper.AllowedExtensions)}",
                [nameof(Extension)]);
        }

        if (Extension.Any(char.IsWhiteSpace))
        {
            yield return new ValidationResult(
                "File extension cannot contain whitespace characters.",
                [nameof(Extension)]);
        }
    }

    /// <summary>
    /// Validates the file size property.
    /// </summary>
    /// <returns>A collection of validation results for the file size.</returns>
    private IEnumerable<ValidationResult> ValidateFileSize()
    {
        if (FileSize <= 0)
        {
            yield return new ValidationResult(
                "File size must be greater than zero for actual document files.",
                [nameof(FileSize)]);
        }
    }

    /// <summary>
    /// Validates the MIME type property.
    /// </summary>
    /// <returns>A collection of validation results for the MIME type.</returns>
    private IEnumerable<ValidationResult> ValidateMimeType()
    {
        if (string.IsNullOrWhiteSpace(MimeType))
        {
            yield return new ValidationResult(
                "MIME type is required for proper document handling and security.",
                [nameof(MimeType)]);
            yield break;
        }

        if (!FileValidationHelper.IsMimeTypeAllowed(MimeType))
        {
            yield return new ValidationResult(
                $"MIME type '{MimeType}' is not allowed for legal document management.",
                [nameof(MimeType)]);
        }

        if (!MimeTypeRegex().IsMatch(MimeType))
        {
            yield return new ValidationResult(
                "Invalid MIME type format. Expected format: type/subtype (e.g., application/pdf).",
                [nameof(MimeType)]);
        }
    }

    /// <summary>
    /// Validates the checksum property.
    /// </summary>
    /// <returns>A collection of validation results for the checksum.</returns>
    private IEnumerable<ValidationResult> ValidateChecksum()
    {
        if (string.IsNullOrWhiteSpace(Checksum))
        {
            yield return new ValidationResult(
                "Checksum is required for document integrity verification and legal compliance.",
                [nameof(Checksum)]);
            yield break;
        }

        if (!ChecksumRegex().IsMatch(Checksum) || Checksum.Length != 64)
        {
            yield return new ValidationResult(
                "Checksum must be a valid 64-character hexadecimal string (SHA256 hash) for proper integrity verification.",
                [nameof(Checksum)]);
        }
    }

    /// <summary>
    /// Validates the creation date property.
    /// </summary>
    /// <returns>A collection of validation results for the creation date.</returns>
    private IEnumerable<ValidationResult> ValidateCreationDate()
    {
        if (CreationDate == default)
        {
            yield return new ValidationResult(
                "Creation date is required and cannot be the default value for temporal tracking and audit compliance.",
                [nameof(CreationDate)]);
            yield break;
        }

        var minAllowedDate = RevisionValidationHelper.MinAllowedRevisionDate;
        if (CreationDate < minAllowedDate)
        {
            yield return new ValidationResult(
                $"Creation date cannot be earlier than {minAllowedDate:yyyy-MM-dd} for system consistency.",
                [nameof(CreationDate)]);
        }

        var maxAllowedDate = DateTime.UtcNow.AddMinutes(RevisionValidationHelper.FutureDateToleranceMinutes);
        if (CreationDate > maxAllowedDate)
        {
            yield return new ValidationResult(
                $"Creation date cannot be in the future (beyond clock skew tolerance of {RevisionValidationHelper.FutureDateToleranceMinutes} minutes).",
                [nameof(CreationDate)]);
        }

        var age = DateTime.UtcNow - CreationDate;
        if (age.TotalDays > RevisionValidationHelper.MaxReasonableAgeYears * 365)
        {
            yield return new ValidationResult(
                $"Creation date age exceeds reasonable bounds for active document management ({RevisionValidationHelper.MaxReasonableAgeYears} years).",
                [nameof(CreationDate)]);
        }
    }

    /// <summary>
    /// Validates the revisions collection.
    /// </summary>
    /// <returns>A collection of validation results for the revisions collection.</returns>
    private IEnumerable<ValidationResult> ValidateRevisions()
    {
        // Collection-level validation
        foreach (var result in RevisionValidationHelper.ValidateRevisionCollection(
            Revisions, Id, nameof(Revisions)))
            yield return result;

        foreach (var result in RevisionValidationHelper.ValidateDocumentRevisionSequence(
            Revisions, nameof(Revisions)))
            yield return result;

        if (Revisions.Count == 0) yield break;

        // Individual revision validation
        var index = 0;
        foreach (var revision in Revisions)
        {
            foreach (var result in RevisionValidationHelper.ValidateRevisionBusinessRules(
                revision.RevisionNumber, revision.CreationDate, revision.ModificationDate,
                revision.DocumentId, revision.IsDeleted, $"Revisions[{index}]."))
                yield return result;

            foreach (var result in RevisionValidationHelper.ValidateDocumentRevisionDates(
                revision.CreationDate, CreationDate, revision.RevisionNumber,
                $"Revisions[{index}].CreationDate"))
                yield return result;

            index++;
        }

        // Sequence analysis
        var revisionNumbers = Revisions.Select(r => r.RevisionNumber);
        var sequenceAnalysis = RevisionValidationHelper.AnalyzeRevisionSequence(revisionNumbers);
        
        if (!(bool)sequenceAnalysis["IsValid"])
        {
            yield return new ValidationResult(
                "Revision sequence validation failed. Ensure sequential numbering without gaps.",
                [nameof(Revisions)]);
        }
    }

    /// <summary>
    /// Gets expected MIME types for a given file extension.
    /// </summary>
    /// <param name="extension">The file extension (without dot, lowercase).</param>
    /// <returns>A collection of expected MIME types for the extension.</returns>
    private static ReadOnlySpan<string> GetExpectedMimeTypesForExtension(string extension)
    {
        return extension switch
        {
            "pdf" => ["application/pdf"],
            "doc" => ["application/msword"],
            "docx" => ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
            "xls" => ["application/vnd.ms-excel"],
            "xlsx" => ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
            "txt" => ["text/plain"],
            "rtf" => ["application/rtf", "text/rtf"],
            "jpg" or "jpeg" => ["image/jpeg"],
            "png" => ["image/png"],
            "gif" => ["image/gif"],
            "tiff" or "tif" => ["image/tiff"],
            _ => []
        };
    }

    /// <summary>
    /// Checks if the document has a valid audit trail based on activity users.
    /// </summary>
    /// <returns>True if the document has a valid audit trail; otherwise, false.</returns>
    private bool HasValidAuditTrail()
    {
        // Check required activities based on document state
        var requiredActivities = new[] { "CREATED" };
        if (IsDeleted)
            requiredActivities = [.. requiredActivities, "DELETED"];

        var documentActivities = DocumentActivityUsers
            .Select(a => a.DocumentActivity?.Activity.ToUpperInvariant())
            .Where(a => !string.IsNullOrEmpty(a))
            .ToHashSet();

        return requiredActivities.All(activity => documentActivities.Contains(activity));
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a DocumentWithRevisionsDto instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The DocumentWithRevisionsDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    public static IList<ValidationResult> ValidateModel([AllowNull] DocumentWithRevisionsDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult(
                "DocumentWithRevisionsDto instance is required and cannot be null."));
            return results;
        }

        try
        {
            var context = new ValidationContext(dto);
            results.AddRange(dto.Validate(context));
        }
        catch (Exception ex)
        {
            results.Add(new ValidationResult(
                $"Validation failed with exception: {ex.Message}"));
        }

        return results;
    }

    /// <summary>
    /// Creates a DocumentWithRevisionsDto from ADMS.Domain.Entities.Document entity with validation.
    /// </summary>
    /// <param name="entity">The Document entity to convert. Cannot be null.</param>
    /// <param name="includeRevisions">Whether to include revision collections in the conversion.</param>
    /// <param name="includeActivityUsers">Whether to include activity user collections in the conversion.</param>
    /// <returns>A valid DocumentWithRevisionsDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    public static DocumentWithRevisionsDto FromEntity(
        ADMS.Domain.Entities.Document entity,
        bool includeRevisions = true,
        bool includeActivityUsers = false)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dto = new DocumentWithRevisionsDto
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
            Revisions = includeRevisions && entity.Revisions is not null
                ? entity.Revisions.Select(RevisionDto.FromEntity).ToList()
                : [],
            DocumentActivityUsers = includeActivityUsers && entity.DocumentActivityUsers is not null
                ? entity.DocumentActivityUsers.Select(CreateDocumentActivityUserDto).ToList()
                : [],
            MatterDocumentActivityUsersFrom = includeActivityUsers && entity.MatterDocumentActivityUsersFrom is not null
                ? entity.MatterDocumentActivityUsersFrom.Select(MatterDocumentActivityUserFromDto.FromEntity).ToList()
                : [],
            MatterDocumentActivityUsersTo = includeActivityUsers && entity.MatterDocumentActivityUsersTo is not null
                ? entity.MatterDocumentActivityUsersTo.Select(MatterDocumentActivityUserToDto.FromEntity).ToList()
                : []
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (validationResults.Count != 0)
        {
            var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
            throw new ValidationException($"Created DocumentWithRevisionsDto failed validation: {errorMessages}");
        }

        return dto;
    }

    /// <summary>
    /// Creates multiple DocumentWithRevisionsDto instances from a collection of entities.
    /// </summary>
    /// <param name="entities">The collection of Document entities to convert. Cannot be null.</param>
    /// <param name="includeRevisions">Whether to include revision collections in the conversion.</param>
    /// <param name="includeActivityUsers">Whether to include activity user collections in the conversion.</param>
    /// <returns>A collection of valid DocumentWithRevisionsDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    public static IList<DocumentWithRevisionsDto> FromEntities(
        IEnumerable<Domain.Entities.Document> entities,
        bool includeRevisions = true,
        bool includeActivityUsers = false)
    {
        ArgumentNullException.ThrowIfNull(entities);

        return entities.Select(entity => FromEntity(entity, includeRevisions, includeActivityUsers)).ToList();
    }

    /// <summary>
    /// Comprehensive quick validation for DocumentWithRevisionsDto.
    /// </summary>
    /// <param name="dto">The document DTO to validate.</param>
    /// <returns>True if the document is valid; otherwise, false.</returns>
    public static bool IsValid(DocumentWithRevisionsDto dto) =>
        dto.Id != Guid.Empty &&
        FileValidationHelper.IsFileNameValid(dto.FileName) &&
        FileValidationHelper.IsExtensionAllowed(dto.Extension) &&
        dto.FileSize > 0 &&
        FileValidationHelper.IsMimeTypeAllowed(dto.MimeType) &&
        FileValidationHelper.IsValidChecksum(dto.Checksum) &&
        RevisionValidationHelper.IsValidDate(dto.CreationDate) &&
        dto is not { IsCheckedOut: true, IsDeleted: true } &&
        dto.Revisions.Count > 0 &&
        dto.Revisions.All(IsValidRevision) &&
        dto.HasValidAuditTrail();

    /// <summary>
    /// Helper method to create DocumentActivityUserDto from entity.
    /// </summary>
    /// <param name="dau">The DocumentActivityUser entity.</param>
    /// <returns>A DocumentActivityUserDto instance.</returns>
    private static DocumentActivityUserDto CreateDocumentActivityUserDto(DocumentActivityUser dau) => new()
    {
        DocumentId = dau.DocumentId,
        Document = new DocumentWithoutRevisionsDto
        {
            Id = dau.Document.Id,
            FileName = dau.Document.FileName,
            Extension = dau.Document.Extension,
            FileSize = dau.Document.FileSize,
            MimeType = dau.Document.MimeType,
            Checksum = dau.Document.Checksum,
            IsCheckedOut = dau.Document.IsCheckedOut,
            IsDeleted = dau.Document.IsDeleted,
            CreationDate = dau.Document.CreatedDate
        },
        DocumentActivityId = dau.DocumentActivityId,
        DocumentActivity = new DocumentActivityDto
        {
            Id = dau.DocumentActivity.Id,
            Activity = dau.DocumentActivity.Activity
        },
        UserId = dau.UserId,
        User = new UserDto
        {
            Id = dau.User.Id,
            Name = dau.User.Name
        },
        CreatedAt = dau.CreatedAt
    };

    /// <summary>
    /// Validates if a revision is valid.
    /// </summary>
    /// <param name="revision">The revision to validate.</param>
    /// <returns>True if the revision is valid; otherwise, false.</returns>
    private static bool IsValidRevision(RevisionDto revision) =>
        RevisionValidationHelper.IsValidRevisionNumber(revision.RevisionNumber) &&
        RevisionValidationHelper.IsValidDate(revision.CreationDate) &&
        RevisionValidationHelper.IsValidDate(revision.ModificationDate) &&
        RevisionValidationHelper.IsValidDateSequence(revision.CreationDate, revision.ModificationDate);

    #endregion Static Methods

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified DocumentWithRevisionsDto is equal to the current DocumentWithRevisionsDto.
    /// </summary>
    /// <param name="other">The DocumentWithRevisionsDto to compare with the current DocumentWithRevisionsDto.</param>
    /// <returns>true if the specified DocumentWithRevisionsDto is equal to the current DocumentWithRevisionsDto; otherwise, false.</returns>
    public bool Equals(DocumentWithRevisionsDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current DocumentWithRevisionsDto.
    /// </summary>
    /// <param name="obj">The object to compare with the current DocumentWithRevisionsDto.</param>
    /// <returns>true if the specified object is equal to the current DocumentWithRevisionsDto; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as DocumentWithRevisionsDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current DocumentWithRevisionsDto.</returns>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the DocumentWithRevisionsDto.
    /// </summary>
    /// <returns>A string that represents the current DocumentWithRevisionsDto.</returns>
    public override string ToString() =>
        $"Document: {FileName} ({FormattedFileSize}) - {RevisionCount} revisions, {TotalActivityCount} activities";

    #endregion String Representation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this document can be edited based on its current state.
    /// </summary>
    /// <returns>true if the document can be edited; otherwise, false.</returns>
    public bool CanBeEdited() => IsAvailableForEdit;

    /// <summary>
    /// Gets usage statistics for this document.
    /// </summary>
    /// <returns>A dictionary containing document statistics and metrics.</returns>
    public IReadOnlyDictionary<string, object> GetUsageStatistics() => new Dictionary<string, object>
    {
        [nameof(FileName)] = FileName,
        [nameof(Extension)] = Extension,
        [nameof(FileSize)] = FileSize,
        [nameof(FormattedFileSize)] = FormattedFileSize,
        [nameof(MimeType)] = MimeType,
        [nameof(Status)] = Status,
        [nameof(RevisionCount)] = RevisionCount,
        [nameof(TotalActivityCount)] = TotalActivityCount,
        [nameof(HasActivities)] = HasActivities,
        [nameof(IsAvailableForEdit)] = IsAvailableForEdit,
        [nameof(HasValidChecksum)] = HasValidChecksum,
        [nameof(IsCheckedOut)] = IsCheckedOut,
        [nameof(IsDeleted)] = IsDeleted
    };

    /// <summary>
    /// Clears any cached computed values to ensure fresh calculations.
    /// </summary>
    /// <remarks>
    /// Call this method when underlying collections or properties change to ensure computed properties reflect current state.
    /// </remarks>
    public void ClearCache()
    {
        _currentRevision = null;
        _currentRevisionCached = false;
        _revisionHistory = null;
        _formattedFileSize = null;
        _hasValidChecksum = null;
    }

    #endregion Business Logic Methods

    #region Regex Generators

    [GeneratedRegex(@"^[A-Fa-f0-9]+$", RegexOptions.Compiled)]
    private static partial Regex ChecksumRegex();

    [GeneratedRegex(@"^[\w\.\-]+\/[\w\.\-\+]+$", RegexOptions.Compiled)]
    private static partial Regex MimeTypeRegex();

    #endregion Regex Generators
}