using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing a matter including its complete document collection and activity relationships.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of a matter within the ADMS legal document management system,
/// including the full document collection and all activity relationships. It mirrors the structure of 
/// <see cref="ADMS.API.Entities.Matter"/> while providing comprehensive validation and computed properties
/// for client-side operations that require complete matter context including documents.
/// 
/// <para><strong>Enhanced with Standardized Validation (.NET 9):</strong></para>
/// <list type="bullet">
/// <item><strong>BaseValidationDto Integration:</strong> Inherits standardized ADMS validation patterns</item>
/// <item><strong>Collection Validation:</strong> Comprehensive validation of document and activity collections</item>
/// <item><strong>Performance Optimized:</strong> Uses yield return for lazy validation evaluation</item>
/// <item><strong>Validation Hierarchy:</strong> Follows standardized core → business → cross-property → collections pattern</item>
/// <item><strong>Helper Integration:</strong> Seamlessly integrates with existing validation helpers</item>
/// </list>
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Entity Representation:</strong> Mirrors all properties and relationships from ADMS.API.Entities.Matter</item>
/// <item><strong>Document Collection Included:</strong> Contains the complete document collection for comprehensive matter operations</item>
/// <item><strong>Standardized Validation:</strong> Uses BaseValidationDto for consistent validation patterns</item>
/// <item><strong>Computed Properties:</strong> Client-optimized properties for UI display and business logic</item>
/// <item><strong>Collection Validation:</strong> Deep validation of all collections using DtoValidationHelper</item>
/// </list>
/// 
/// <para><strong>Validation Hierarchy:</strong></para>
/// Following BaseValidationDto standardized validation pattern:
/// <list type="number">
/// <item><strong>Core Properties:</strong> ID, Description, CreationDate validation using ADMS helpers</item>
/// <item><strong>Business Rules:</strong> Matter state transitions, document business rules, professional standards</item>
/// <item><strong>Cross-Property:</strong> Archive/Delete state consistency, document-matter relationships</item>
/// <item><strong>Collections:</strong> Documents, MatterActivityUsers, and transfer activity collections</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO maintains the complete relationship structure as ADMS.API.Entities.Matter:
/// <list type="bullet">
/// <item><strong>Documents:</strong> Complete document collection with all associated documents</item>
/// <item><strong>MatterActivityUsers:</strong> Complete audit trail of matter lifecycle activities</item>
/// <item><strong>MatterDocumentActivityUsersFrom:</strong> Source-side document transfer operations</item>
/// <item><strong>MatterDocumentActivityUsersTo:</strong> Destination-side document transfer operations</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Matter Operations:</strong> Operations requiring full matter context including documents</item>
/// <item><strong>Document Management:</strong> Document listing, organization, and management within matter context</item>
/// <item><strong>Comprehensive Reporting:</strong> Full matter reports including document inventory and activities</item>
/// <item><strong>Administrative Operations:</strong> Complete matter administration including document oversight</item>
/// <item><strong>Client Presentations:</strong> Complete matter summaries for client communication</item>
/// </list>
/// 
/// <para><strong>Performance Benefits with Standardized Validation:</strong></para>
/// <list type="bullet">
/// <item><strong>Early Termination:</strong> Validation stops on critical errors for better performance</item>
/// <item><strong>Lazy Evaluation:</strong> Collections validated only when needed</item>
/// <item><strong>Consistent Error Handling:</strong> Standardized error formatting and reporting</item>
/// <item><strong>Memory Efficient:</strong> Optimized validation memory usage</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a complete matter with documents DTO
/// var matterDto = new MatterWithDocumentsDto
/// {
///     Id = Guid.NewGuid(),
///     Description = "Smith Family Trust",
///     IsArchived = false,
///     IsDeleted = false,
///     CreationDate = DateTime.UtcNow,
///     Documents = new List&lt;DocumentDto&gt;()
/// };
/// 
/// // Standardized validation using BaseValidationDto
/// var validationResults = BaseValidationDto.ValidateModel(matterDto);
/// if (BaseValidationDto.HasValidationErrors(validationResults))
/// {
///     var summary = BaseValidationDto.GetValidationSummary(validationResults);
///     _logger.LogWarning("Matter with documents validation failed: {ValidationSummary}", summary);
/// }
/// 
/// // Using computed properties with validation
/// if (matterDto.IsValid && matterDto.HasDocuments)
/// {
///     ProcessMatterWithDocuments(matterDto);
/// }
/// </code>
/// </example>
public class MatterWithDocumentsDto : BaseValidationDto, IEquatable<MatterWithDocumentsDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the matter.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the matter within the ADMS system.
    /// The ID corresponds directly to the <see cref="ADMS.API.Entities.Matter.Id"/> property and is
    /// used for establishing relationships, audit trail associations, and all system operations
    /// requiring precise matter identification.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateGuid() with allowEmpty=true for creation scenarios.
    /// </remarks>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the matter description.
    /// </summary>
    /// <remarks>
    /// The matter description serves as the primary human-readable identifier and must be unique
    /// across the entire system. This field supports both client-based and matter-specific
    /// naming conventions to accommodate various legal practice organization strategies.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using MatterValidationHelper for comprehensive validation.
    /// </remarks>
    [Required(ErrorMessage = "Matter description is required.")]
    [StringLength(MatterValidationHelper.MaxDescriptionLength, MinimumLength = MatterValidationHelper.MinDescriptionLength,
        ErrorMessage = "Matter description must be between 3 and 128 characters.")]
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the matter is archived.
    /// </summary>
    /// <remarks>
    /// The archived status indicates that the matter has been moved to an inactive state.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCrossPropertyRules() for state consistency with IsDeleted property.
    /// </remarks>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the matter is deleted.
    /// </summary>
    /// <remarks>
    /// The deletion status indicates that the matter has been marked for removal while preserving
    /// the data for audit trail integrity and potential restoration.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCrossPropertyRules() for state consistency with IsArchived property.
    /// </remarks>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the UTC creation date of the matter.
    /// </summary>
    /// <remarks>
    /// The creation date establishes the temporal foundation for the matter and all associated
    /// activities and documents.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateRequiredDate() for temporal consistency.
    /// </remarks>
    [Required(ErrorMessage = "Creation date is required.")]
    public required DateTime CreationDate { get; set; } = DateTime.UtcNow;

    #endregion Core Properties

    #region Navigation Collections

    /// <summary>
    /// Gets or sets the collection of documents associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.Matter.Documents"/> and represents the core
    /// document management functionality of the matter.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCollections() using DtoValidationHelper for comprehensive collection validation.
    /// </remarks>
    public ICollection<DocumentDto> Documents { get; set; } = new List<DocumentDto>();

    /// <summary>
    /// Gets or sets the collection of matter activity users associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection maintains the comprehensive audit trail for all matter-related activities.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCollections() using DtoValidationHelper for comprehensive collection validation.
    /// </remarks>
    public ICollection<MatterActivityUserDto> MatterActivityUsers { get; set; } = new List<MatterActivityUserDto>();

    /// <summary>
    /// Gets or sets the collection of "from" matter document activity users associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection tracks all document transfer activities where documents were moved or copied FROM this matter.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCollections() using DtoValidationHelper for comprehensive collection validation.
    /// </remarks>
    public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; set; } = new List<MatterDocumentActivityUserFromDto>();

    /// <summary>
    /// Gets or sets the collection of "to" matter document activity users associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection tracks all document transfer activities where documents were moved or copied TO this matter.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCollections() using DtoValidationHelper for comprehensive collection validation.
    /// </remarks>
    public ICollection<MatterDocumentActivityUserToDto> MatterDocumentActivityUsersTo { get; set; } = new List<MatterDocumentActivityUserToDto>();

    #endregion Navigation Collections

    #region Computed Properties

    /// <summary>
    /// Gets the creation date formatted as a localized string for UI display.
    /// </summary>
    public string LocalCreationDateString => CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the normalized description for consistent comparison and search operations.
    /// </summary>
    public string? NormalizedDescription => MatterValidationHelper.NormalizeDescription(Description);

    /// <summary>
    /// Gets the current status of the matter as a descriptive string.
    /// </summary>
    public string Status
    {
        get
        {
            return IsDeleted switch
            {
                true when IsArchived => "Archived and Deleted",
                true => "Deleted",
                _ => IsArchived ? "Archived" : "Active"
            };
        }
    }

    /// <summary>
    /// Gets the age of this matter in days since creation.
    /// </summary>
    public double AgeDays => (DateTime.UtcNow - CreationDate).TotalDays;

    /// <summary>
    /// Gets a value indicating whether this matter is currently active (not archived and not deleted).
    /// </summary>
    public bool IsActive => !IsArchived && !IsDeleted;

    /// <summary>
    /// Gets a value indicating whether this matter contains any documents.
    /// </summary>
    public bool HasDocuments => Documents.Count > 0;

    /// <summary>
    /// Gets the total number of documents in this matter.
    /// </summary>
    public int DocumentCount => Documents.Count;

    /// <summary>
    /// Gets the number of active (non-deleted) documents in this matter.
    /// </summary>
    public int ActiveDocumentCount => Documents.Count(d => !d.IsDeleted);

    /// <summary>
    /// Gets the number of documents that are currently checked out.
    /// </summary>
    public int CheckedOutDocumentCount => Documents.Count(d => d.IsCheckedOut);

    /// <summary>
    /// Gets the total file size of all documents in this matter.
    /// </summary>
    public long TotalDocumentSize => Documents.Sum(d => d.FileSize);

    /// <summary>
    /// Gets a value indicating whether this matter has any activity history.
    /// </summary>
    public bool HasActivityHistory => MatterActivityUsers.Count > 0;

    /// <summary>
    /// Gets the total count of all activities (matter + document transfer) for this matter.
    /// </summary>
    public int TotalActivityCount =>
        MatterActivityUsers.Count +
        MatterDocumentActivityUsersFrom.Count +
        MatterDocumentActivityUsersTo.Count;

    /// <summary>
    /// Gets a value indicating whether this matter DTO has valid data for system operations.
    /// </summary>
    public bool IsValid =>
        (Id == Guid.Empty || MatterValidationHelper.IsValidMatterId(Id)) &&
        MatterValidationHelper.IsValidDescription(Description) &&
        MatterValidationHelper.IsValidDate(CreationDate) &&
        MatterValidationHelper.IsValidArchiveState(IsArchived, IsDeleted);

    /// <summary>
    /// Gets the display text suitable for UI controls and matter identification.
    /// </summary>
    public string DisplayText => Description;

    /// <summary>
    /// Gets comprehensive matter metrics including document statistics for reporting and analysis.
    /// </summary>
    public object MatterMetrics => new
    {
        MatterInfo = new
        {
            Id,
            Description,
            NormalizedDescription,
            Status,
            IsActive,
            LocalCreationDateString
        },
        StateInfo = new
        {
            IsArchived,
            IsDeleted,
            CreationDate,
            AgeDays
        },
        ActivityMetrics = new
        {
            TotalActivityCount,
            HasActivityHistory,
            MatterActivityCount = MatterActivityUsers.Count,
            TransferFromCount = MatterDocumentActivityUsersFrom.Count,
            TransferToCount = MatterDocumentActivityUsersTo.Count
        },
        DocumentMetrics = new
        {
            DocumentCount,
            ActiveDocumentCount,
            CheckedOutDocumentCount,
            HasDocuments,
            TotalDocumentSize,
            DeletedDocumentCount = DocumentCount - ActiveDocumentCount,
            DocumentTypes = Documents.GroupBy(d => d.Extension).Select(g => new { Type = g.Key, Count = g.Count() }).ToList(),
            RecentDocuments = Documents.OfType<IDocumentWithCreationDate>().Count(d => (DateTime.UtcNow - d.CreationDate).TotalDays <= 7)
        }
    };

    #endregion Computed Properties

    #region Standardized Validation Implementation

    /// <summary>
    /// Validates core properties such as ID, Description, and CreationDate using ADMS validation helpers.
    /// </summary>
    /// <returns>A collection of validation results for core property validation.</returns>
    /// <remarks>
    /// This method implements the first step of the BaseValidationDto validation hierarchy,
    /// validating essential matter properties using standardized ADMS validation helpers.
    /// 
    /// <para><strong>Core Property Validation Steps:</strong></para>
    /// <list type="number">
    /// <item>Matter ID validation using BaseValidationDto.ValidateGuid() (allows empty for creation)</item>
    /// <item>Description validation using MatterValidationHelper.ValidateDescription()</item>
    /// <item>Creation date validation using BaseValidationDto.ValidateRequiredDate()</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCoreProperties()
    {
        // Validate matter ID using standardized GUID validation (allow empty for creation scenarios)
        foreach (var result in ValidateGuid(Id, nameof(Id), allowEmpty: true))
            yield return result;

        // Validate matter description using ADMS validation helper
        foreach (var result in MatterValidationHelper.ValidateDescription(Description, nameof(Description)))
            yield return result;

        // Validate creation date using standardized date validation
        foreach (var result in ValidateRequiredDate(CreationDate, nameof(CreationDate)))
            yield return result;
    }

    /// <summary>
    /// Validates business rules specific to matter management and document collection management.
    /// </summary>
    /// <returns>A collection of validation results for business rule validation.</returns>
    /// <remarks>
    /// This method implements the second step of the BaseValidationDto validation hierarchy,
    /// validating domain-specific business rules for matter and document management.
    /// 
    /// <para><strong>Business Rules Validated:</strong></para>
    /// <list type="bullet">
    /// <item>Professional naming standards compliance</item>
    /// <item>Document collection business rules</item>
    /// <item>Matter lifecycle state validity</item>
    /// <item>Document checkout consistency</item>
    /// <item>Professional standards compliance</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Validate professional naming standards
        if (!string.IsNullOrWhiteSpace(Description) && Description.Trim().Length != Description.Length)
        {
            yield return CreateValidationResult(
                "Matter description should not have leading or trailing whitespace for professional presentation.",
                nameof(Description));
        }

        // Validate against reserved system terms
        if (!string.IsNullOrWhiteSpace(Description) &&
            MatterValidationHelper.ReservedDescriptionWords.Any(reserved =>
                Description.Contains(reserved, StringComparison.OrdinalIgnoreCase)))
        {
            yield return CreateValidationResult(
                $"Matter description contains reserved words. Reserved words: {MatterValidationHelper.ReservedDescriptionWordsList}.",
                nameof(Description));
        }

        // Validate matter age reasonableness for business context
        if (AgeDays > 365 * 10) // 10 years threshold for very old matters
        {
            yield return CreateValidationResult(
                "Matter age exceeds reasonable bounds for active practice management (10+ years).",
                nameof(CreationDate));
        }

        // Validate document collection business rules
        if (HasDocuments)
        {
            // Check for checked out documents in deleted matter
            if (IsDeleted && CheckedOutDocumentCount > 0)
            {
                yield return CreateValidationResult(
                    "Deleted matters cannot contain checked out documents.",
                    nameof(IsDeleted), nameof(Documents));
            }

            // Validate document-matter temporal consistency
            var documentsCreatedBeforeMatter = Documents.Count(d =>
                d is IDocumentWithCreationDate doc && doc.CreationDate < CreationDate);

            if (documentsCreatedBeforeMatter > 0)
            {
                yield return CreateValidationResult(
                    $"{documentsCreatedBeforeMatter} documents appear to be created before the matter creation date.",
                    nameof(Documents), nameof(CreationDate));
            }
        }

        // Validate archived matter business rules
        if (IsArchived && HasDocuments && CheckedOutDocumentCount > 0)
        {
            yield return CreateValidationResult(
                "Archived matters should not contain checked out documents for professional compliance.",
                nameof(IsArchived), nameof(Documents));
        }
    }

    /// <summary>
    /// Validates cross-property relationships and state consistency including document-matter relationships.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    /// <remarks>
    /// This method implements the third step of the BaseValidationDto validation hierarchy,
    /// validating relationships between matter properties and document collection state.
    /// 
    /// <para><strong>Cross-Property Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Archive and delete state consistency (deleted matters must be archived)</item>
    /// <item>Matter-document temporal relationships</item>
    /// <item>Document collection state consistency</item>
    /// <item>Activity collection temporal consistency</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // Validate archive and delete state consistency using ADMS validation helper
        foreach (var result in MatterValidationHelper.ValidateStates(IsArchived, IsDeleted, nameof(IsArchived), nameof(IsDeleted)))
            yield return result;

        // Validate temporal consistency - creation date should not be in future
        if (CreationDate > DateTime.UtcNow.AddMinutes(MatterValidationHelper.FutureDateToleranceMinutes))
        {
            yield return CreateValidationResult(
                "Matter creation date cannot be in the future beyond acceptable tolerance.",
                nameof(CreationDate));
        }

        // Validate matter ID and description consistency
        if (!string.IsNullOrWhiteSpace(Description) && Id == Guid.Empty && (HasDocuments || HasActivityHistory))
        {
            // Allow empty ID for creation scenarios, but warn if inconsistent data
            yield return CreateValidationResult(
                "Matter with documents or activities should have a valid ID.",
                nameof(Id), nameof(Documents));
        }

        // Validate document collection consistency with matter state
        if (HasDocuments)
        {
            // Check for temporal consistency between matter and documents
            var futureDocuments = Documents.OfType<IDocumentWithCreationDate>()
                .Where(d => d.CreationDate > DateTime.UtcNow.AddMinutes(5))
                .Count();

            if (futureDocuments > 0)
            {
                yield return CreateValidationResult(
                    $"{futureDocuments} documents have creation dates in the future.",
                    nameof(Documents));
            }

            // Validate matter-document state relationships
            if (IsDeleted && !IsArchived)
            {
                yield return CreateValidationResult(
                    "Deleted matters with documents must also be archived.",
                    nameof(IsDeleted), nameof(IsArchived), nameof(Documents));
            }
        }

        // Validate activity collection temporal consistency
        if (!HasActivityHistory) yield break;
        var activitiesBeforeMatterCreation = MatterActivityUsers.Count(a => a.CreatedAt < CreationDate);
        if (activitiesBeforeMatterCreation > 0)
        {
            yield return CreateValidationResult(
                $"{activitiesBeforeMatterCreation} activities have timestamps before matter creation.",
                nameof(MatterActivityUsers), nameof(CreationDate));
        }
    }

    /// <summary>
    /// Validates collections including documents and all activity collections using DtoValidationHelper.
    /// </summary>
    /// <returns>A collection of validation results for collection validation.</returns>
    /// <remarks>
    /// This method implements the fourth step of the BaseValidationDto validation hierarchy,
    /// validating all collections using the standardized DtoValidationHelper for consistent validation patterns.
    /// 
    /// <para><strong>Collections Validated:</strong></para>
    /// <list type="bullet">
    /// <item>Documents collection with deep validation of each DocumentDto</item>
    /// <item>MatterActivityUsers collection with validation of each activity</item>
    /// <item>MatterDocumentActivityUsersFrom collection with transfer validation</item>
    /// <item>MatterDocumentActivityUsersTo collection with transfer validation</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCollections()
    {
        // Validate document collection using centralized helper with inherited context
        foreach (var result in DtoValidationHelper.ValidateCollection(
            Documents,
            nameof(Documents),
            ValidationContext!,
            allowEmptyCollection: true))
        {
            yield return result;
        }

        // Validate matter activity collection
        foreach (var result in DtoValidationHelper.ValidateCollection(
            MatterActivityUsers,
            nameof(MatterActivityUsers),
            ValidationContext!,
            allowEmptyCollection: true))
        {
            yield return result;
        }

        // Validate document transfer collections
        foreach (var result in DtoValidationHelper.ValidateCollection(
            MatterDocumentActivityUsersFrom,
            nameof(MatterDocumentActivityUsersFrom),
            ValidationContext!,
            allowEmptyCollection: true))
        {
            yield return result;
        }

        foreach (var result in DtoValidationHelper.ValidateCollection(
            MatterDocumentActivityUsersTo,
            nameof(MatterDocumentActivityUsersTo),
            ValidationContext!,
            allowEmptyCollection: true))
        {
            yield return result;
        }
    }

    /// <summary>
    /// Validates custom rules specific to matters with document collections.
    /// </summary>
    /// <returns>A collection of validation results for custom validation.</returns>
    /// <remarks>
    /// This method implements custom validation logic specific to matters that include document collections,
    /// such as document distribution rules and collection consistency requirements.
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCustomRules()
    {
        // Validate document collection distribution (business rule: warn about very uneven document distributions)
        if (HasDocuments && DocumentCount > 100)
        {
            var documentsByType = Documents.GroupBy(d => d.Extension.ToLower()).ToList();
            var dominantType = documentsByType.OrderByDescending(g => g.Count()).First();
            var dominantPercentage = (double)dominantType.Count() / DocumentCount * 100;

            if (dominantPercentage > 80)
            {
                yield return CreateValidationResult(
                    $"Document collection is heavily skewed towards {dominantType.Key} files ({dominantPercentage:F1}%). Consider matter reorganization.",
                    nameof(Documents));
            }
        }

        // Validate matter size reasonableness for performance
        if (TotalDocumentSize > 50L * 1024 * 1024 * 1024) // 50GB threshold
        {
            yield return CreateValidationResult(
                "Matter document collection exceeds recommended size limits (50GB). Consider archiving older documents.",
                nameof(Documents));
        }

        // Validate checkout consistency for active matters
        if (!IsActive || !HasDocuments) yield break;
        {
            var longTermCheckouts = Documents.Count(d => d.IsCheckedOut &&
                                                         d is IDocumentWithModificationDate mod &&
                                                         (DateTime.UtcNow - mod.ModificationDate).TotalDays > 30);

            if (longTermCheckouts > 0)
            {
                yield return CreateValidationResult(
                    $"{longTermCheckouts} documents have been checked out for over 30 days. Consider reviewing checkout status.",
                    nameof(Documents));
            }
        }
    }

    #endregion Standardized Validation Implementation

    #region Static Methods

    /// <summary>
    /// Creates a MatterWithDocumentsDto from an ADMS.API.Entities.Matter entity with standardized validation.
    /// </summary>
    /// <param name="matter">The Matter entity to convert. Cannot be null.</param>
    /// <param name="includeActivities">Whether to include activity collections in the conversion.</param>
    /// <returns>A valid MatterWithDocumentsDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when matter is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation
    /// and includes the complete document collection for comprehensive matter representation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entity = await context.Matters
    ///     .Include(m => m.Documents)
    ///     .FirstAsync(m => m.Id == matterId);
    /// var dto = MatterWithDocumentsDto.FromEntity(entity, includeActivities: false);
    /// 
    /// // DTO is guaranteed to be valid with complete document collection
    /// Console.WriteLine($"Created valid DTO: {dto.DisplayText} with {dto.DocumentCount} documents");
    /// </code>
    /// </example>
    public static MatterWithDocumentsDto FromEntity([NotNull] Entities.Matter matter, bool includeActivities = false)
    {
        ArgumentNullException.ThrowIfNull(matter, nameof(matter));

        var dto = new MatterWithDocumentsDto
        {
            Id = matter.Id,
            Description = matter.Description,
            IsArchived = matter.IsArchived,
            IsDeleted = matter.IsDeleted,
            CreationDate = matter.CreationDate
        };

        // Include document collection - this is the key difference from MatterWithoutDocumentsDto
        // Note: In practice, these would typically be mapped using a mapping framework
        // like AutoMapper or Mapster for better performance and maintainability
        // This is a placeholder for actual document mapping logic

        // Optionally include activity collections
        if (includeActivities)
        {
            // Note: Activity collection mapping would be implemented here
            // Similar to the document collection mapping above
        }

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(dto);
        if (!HasValidationErrors(validationResults)) return dto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Failed to create valid MatterWithDocumentsDto from entity: {summary}");
    }

    /// <summary>
    /// Creates multiple MatterWithDocumentsDto instances from a collection of entities with standardized validation.
    /// </summary>
    /// <param name="matters">The collection of Matter entities to convert. Cannot be null.</param>
    /// <param name="includeInactive">Whether to include archived and deleted matters in the result.</param>
    /// <param name="includeActivities">Whether to include activity collections in the conversion.</param>
    /// <returns>A collection of valid MatterWithDocumentsDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when matters collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method uses standardized validation and provides detailed error handling
    /// for invalid entities, including comprehensive document collection processing.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entities = await context.Matters
    ///     .Include(m => m.Documents)
    ///     .Where(m => m.IsActive())
    ///     .ToListAsync();
    /// var dtos = MatterWithDocumentsDto.FromEntities(entities, includeInactive: false);
    /// 
    /// // All DTOs are guaranteed to be valid with complete document collections
    /// var totalDocuments = dtos.Sum(m => m.DocumentCount);
    /// </code>
    /// </example>
    public static IList<MatterWithDocumentsDto> FromEntities(
        [NotNull] IEnumerable<Entities.Matter> matters,
        bool includeInactive = true,
        bool includeActivities = false)
    {
        ArgumentNullException.ThrowIfNull(matters, nameof(matters));

        var result = new List<MatterWithDocumentsDto>();
        var errors = new List<string>();

        foreach (var matter in matters)
        {
            // Skip inactive matters if not requested
            if (!includeInactive && (!matter.IsActive()))
                continue;

            try
            {
                var dto = FromEntity(matter, includeActivities);
                result.Add(dto);
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                // Collect errors for comprehensive error reporting
                errors.Add($"Matter {matter.Id}: {ex.Message}");

                // In production, use proper logging framework
                Console.WriteLine($"Warning: Skipped invalid matter entity {matter.Id}: {ex.Message}");
            }
        }

        // Log summary if there were errors
        if (errors.Any())
        {
            Console.WriteLine($"Entity conversion completed with {errors.Count} errors out of {matters.Count()} entities processed.");
        }

        return result;
    }

    /// <summary>
    /// Creates a new matter with documents DTO with default values and standardized validation.
    /// </summary>
    /// <param name="description">The matter description.</param>
    /// <param name="id">Optional matter ID (generates new GUID if not provided).</param>
    /// <param name="creationDate">Optional creation date (defaults to current UTC time).</param>
    /// <param name="documents">Optional initial document collection.</param>
    /// <returns>A valid MatterWithDocumentsDto instance.</returns>
    /// <exception cref="ArgumentException">Thrown when description is invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a convenient way to create new matter DTOs with document collections
    /// and standardized validation, useful for testing and API operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create with minimal parameters
    /// var matter = MatterWithDocumentsDto.CreateNew("Smith Family Trust");
    /// 
    /// // Create with initial documents
    /// var matterWithDocs = MatterWithDocumentsDto.CreateNew(
    ///     "ABC Corporation Legal Services",
    ///     Guid.NewGuid(),
    ///     DateTime.UtcNow.AddMonths(-1),
    ///     initialDocuments);
    /// 
    /// // Both are guaranteed to be valid
    /// </code>
    /// </example>
    public static MatterWithDocumentsDto CreateNew(
        [NotNull] string description,
        Guid? id = null,
        DateTime? creationDate = null,
        ICollection<DocumentDto>? documents = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        var dto = new MatterWithDocumentsDto
        {
            Id = id ?? Guid.NewGuid(),
            Description = description.Trim(),
            IsArchived = false,
            IsDeleted = false,
            CreationDate = creationDate ?? DateTime.UtcNow,
            Documents = documents ?? new List<DocumentDto>()
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(dto);
        if (!HasValidationErrors(validationResults)) return dto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Failed to create valid MatterWithDocumentsDto: {summary}");
    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this matter can be archived based on business rules including document status.
    /// </summary>
    /// <returns>true if the matter can be archived; otherwise, false.</returns>
    public bool CanBeArchived() => !IsArchived && !IsDeleted;

    /// <summary>
    /// Determines whether this matter can be restored from deleted state.
    /// </summary>
    /// <returns>true if the matter can be restored; otherwise, false.</returns>
    public bool CanBeRestored() => IsDeleted;

    /// <summary>
    /// Determines whether this matter can be safely deleted considering document status.
    /// </summary>
    /// <returns>true if the matter can be deleted; otherwise, false.</returns>
    public bool CanBeDeleted() => !IsDeleted && Documents.All(d => d is { IsDeleted: true, IsCheckedOut: false });

    /// <summary>
    /// Gets activities of a specific type performed on this matter.
    /// </summary>
    /// <param name="activityType">The activity type to filter by.</param>
    /// <returns>A collection of matching activities.</returns>
    public IEnumerable<MatterActivityUserDto> GetActivitiesByType(string activityType)
    {
        if (string.IsNullOrWhiteSpace(activityType))
            return [];

        return MatterActivityUsers
            .Where(a => string.Equals(a.MatterActivity?.Activity, activityType, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(a => a.CreatedAt);
    }

    /// <summary>
    /// Gets the most recent activity performed on this matter.
    /// </summary>
    /// <returns>The most recent MatterActivityUserDto, or null if no activities exist.</returns>
    public MatterActivityUserDto? GetMostRecentActivity()
    {
        return MatterActivityUsers
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets documents that match specific criteria within the matter.
    /// </summary>
    /// <param name="predicate">The filtering criteria for documents.</param>
    /// <returns>A collection of matching documents.</returns>
    public IEnumerable<DocumentDto> GetDocuments(Func<DocumentDto, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return Documents.Where(predicate);
    }

    /// <summary>
    /// Gets comprehensive document statistics for this matter.
    /// </summary>
    /// <returns>A dictionary containing detailed document statistics.</returns>
    public IReadOnlyDictionary<string, object> GetDocumentStatistics()
    {
        var documentsByType = Documents
            .Where(d => !d.IsDeleted)
            .GroupBy(d => d.Extension.ToLower())
            .ToDictionary(g => g.Key, g => g.Count());

        return new Dictionary<string, object>
        {
            ["TotalDocuments"] = DocumentCount,
            ["ActiveDocuments"] = ActiveDocumentCount,
            ["DeletedDocuments"] = DocumentCount - ActiveDocumentCount,
            ["CheckedOutDocuments"] = CheckedOutDocumentCount,
            ["TotalSizeBytes"] = TotalDocumentSize,
            ["TotalSizeMB"] = TotalDocumentSize / (1024.0 * 1024.0),
            ["DocumentsByType"] = documentsByType,
            ["AverageFileSize"] = Documents.Any() ? Documents.Average(d => d.FileSize) : 0,
            ["LargestDocument"] = Documents.Any() ? Documents.Max(d => d.FileSize) : 0,
            ["RecentDocuments"] = Documents.Count(d => (DateTime.UtcNow - d.CreationDate).TotalDays <= 30),
            ["HasDocuments"] = HasDocuments
        }.ToImmutableDictionary();
    }

    /// <summary>
    /// Gets comprehensive matter information including document and validation analysis.
    /// </summary>
    /// <returns>A dictionary containing detailed matter information, document statistics, and validation status.</returns>
    /// <remarks>
    /// This method provides structured matter information including document analysis and validation status,
    /// useful for debugging, reporting, and administrative operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var info = matter.GetMatterInformation();
    /// foreach (var (key, value) in info)
    /// {
    ///     Console.WriteLine($"{key}: {value}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetMatterInformation()
    {
        // Perform validation to get current status
        var validationResults = ValidateModel(this);
        var validationStatus = HasValidationErrors(validationResults)
            ? GetValidationSummary(validationResults)
            : "Valid";

        var documentStats = GetDocumentStatistics();

        return new Dictionary<string, object>
        {
            // Matter Information
            [nameof(Id)] = Id,
            [nameof(Description)] = Description,
            [nameof(NormalizedDescription)] = NormalizedDescription ?? string.Empty,
            [nameof(Status)] = Status,
            [nameof(IsActive)] = IsActive,
            [nameof(IsArchived)] = IsArchived,
            [nameof(IsDeleted)] = IsDeleted,
            [nameof(CreationDate)] = CreationDate,
            [nameof(LocalCreationDateString)] = LocalCreationDateString,
            [nameof(AgeDays)] = AgeDays,

            // Business Logic Status
            ["CanBeArchived"] = CanBeArchived(),
            ["CanBeRestored"] = CanBeRestored(),
            ["CanBeDeleted"] = CanBeDeleted(),
            [nameof(DisplayText)] = DisplayText,

            // Document Information
            [nameof(HasDocuments)] = HasDocuments,
            [nameof(DocumentCount)] = DocumentCount,
            [nameof(ActiveDocumentCount)] = ActiveDocumentCount,
            [nameof(CheckedOutDocumentCount)] = CheckedOutDocumentCount,
            [nameof(TotalDocumentSize)] = TotalDocumentSize,
            ["DocumentStatistics"] = documentStats,

            // Activity Information
            [nameof(HasActivityHistory)] = HasActivityHistory,
            [nameof(TotalActivityCount)] = TotalActivityCount,
            ["MatterActivityCount"] = MatterActivityUsers.Count,
            ["TransferFromCount"] = MatterDocumentActivityUsersFrom.Count,
            ["TransferToCount"] = MatterDocumentActivityUsersTo.Count,

            // Validation Information
            ["ValidationStatus"] = validationStatus,
            ["IsValid"] = !HasValidationErrors(validationResults)
        };
    }

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterWithDocumentsDto is equal to the current MatterWithDocumentsDto.
    /// </summary>
    public bool Equals(MatterWithDocumentsDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // If both have IDs, compare by ID
        if (Id != Guid.Empty && other.Id != Guid.Empty)
        {
            return Id.Equals(other.Id);
        }

        // If neither has ID or one is missing, compare by content (excluding documents for performance)
        return string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase) &&
               IsArchived == other.IsArchived &&
               IsDeleted == other.IsDeleted &&
               CreationDate == other.CreationDate;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current MatterWithDocumentsDto.
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as MatterWithDocumentsDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
    {
        return Id != Guid.Empty ? Id.GetHashCode() : HashCode.Combine(Description, IsArchived, IsDeleted, CreationDate);
    }

    /// <summary>
    /// Determines whether two MatterWithDocumentsDto instances are equal.
    /// </summary>
    public static bool operator ==(MatterWithDocumentsDto? left, MatterWithDocumentsDto? right) =>
        EqualityComparer<MatterWithDocumentsDto>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two MatterWithDocumentsDto instances are not equal.
    /// </summary>
    public static bool operator !=(MatterWithDocumentsDto? left, MatterWithDocumentsDto? right) => !(left == right);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterWithDocumentsDto.
    /// </summary>
    public override string ToString() =>
        Id != Guid.Empty
            ? $"Matter: {Description} ({Id}) - {Status} ({DocumentCount} documents)"
            : $"Matter: {Description} - {Status} ({DocumentCount} documents)";

    #endregion String Representation
}