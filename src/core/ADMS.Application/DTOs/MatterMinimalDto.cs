using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Minimal Data Transfer Object representing essential matter metadata for efficient matter identification and selection operations.
/// </summary>
/// <remarks>
/// This DTO serves as a lightweight representation of a matter within the ADMS legal document management system,
/// containing only essential properties from <see cref="ADMS.API.Entities.Matter"/> required for matter identification,
/// selection, and basic metadata display. It excludes document collections and activity relationships for optimal
/// performance in scenarios requiring rapid matter lookup and selection.
/// 
/// <para><strong>Enhanced with Standardized Validation (.NET 9):</strong></para>
/// <list type="bullet">
/// <item><strong>BaseValidationDto Integration:</strong> Inherits standardized ADMS validation patterns</item>
/// <item><strong>Consistent Error Handling:</strong> Standardized validation error messages and formatting</item>
/// <item><strong>Performance Optimized:</strong> Uses yield return for lazy validation evaluation</item>
/// <item><strong>Validation Hierarchy:</strong> Follows standardized core → business → cross-property → collections pattern</item>
/// <item><strong>Helper Integration:</strong> Seamlessly integrates with existing MatterValidationHelper</item>
/// </list>
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Entity Representation:</strong> Contains only essential properties from ADMS.API.Entities.Matter</item>
/// <item><strong>Performance Optimized:</strong> Excludes collections and relationships for fast matter enumeration</item>
/// <item><strong>Standardized Validation:</strong> Uses BaseValidationDto for consistent validation patterns</item>
/// <item><strong>Immutable Design:</strong> Record type with init-only properties for thread-safe operations</item>
/// <item><strong>Display Ready:</strong> Pre-validated for immediate use in UI controls and selection lists</item>
/// </list>
/// 
/// <para><strong>Entity Alignment:</strong></para>
/// This DTO mirrors core properties from ADMS.API.Entities.Matter:
/// <list type="bullet">
/// <item><strong>Id:</strong> Unique matter identifier for precise matter referencing</item>
/// <item><strong>Description:</strong> Human-readable matter description for identification</item>
/// <item><strong>IsArchived:</strong> Current archival status for matter state awareness</item>
/// <item><strong>IsDeleted:</strong> Soft deletion status for proper matter filtering</item>
/// <item><strong>CreationDate:</strong> Matter creation timestamp for chronological ordering</item>
/// </list>
/// 
/// <para><strong>Validation Hierarchy:</strong></para>
/// Following BaseValidationDto standardized validation pattern:
/// <list type="number">
/// <item><strong>Core Properties:</strong> ID, Description, CreationDate validation using ADMS helpers</item>
/// <item><strong>Business Rules:</strong> Matter state transitions and professional standards</item>
/// <item><strong>Cross-Property:</strong> Archive/Delete state consistency validation</item>
/// <item><strong>Collections:</strong> N/A for minimal DTO (no collections)</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter Selection:</strong> Dropdown lists, autocomplete controls, and matter picker interfaces</item>
/// <item><strong>Matter Listings:</strong> Matter browsing and navigation interfaces without detailed information</item>
/// <item><strong>API Responses:</strong> Lightweight matter references in REST API responses</item>
/// <item><strong>Search Results:</strong> Matter search results requiring basic identification information</item>
/// <item><strong>Relationship References:</strong> Matter references in other DTOs and entities</item>
/// </list>
/// 
/// <para><strong>Performance Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Memory Footprint:</strong> Only essential properties to reduce memory usage</item>
/// <item><strong>Fast Serialization:</strong> Quick JSON serialization/deserialization for API operations</item>
/// <item><strong>Lazy Validation:</strong> Early termination on validation errors for performance</item>
/// <item><strong>Database Efficiency:</strong> Optimal for database projections and bulk operations</item>
/// <item><strong>UI Responsiveness:</strong> Fast loading in user interface controls and lists</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a minimal matter DTO for selection controls
/// var matterMinimal = new MatterMinimalDto
/// {
///     Id = Guid.Parse("60000000-0000-0000-0000-000000000001"),
///     Description = "Smith Family Trust",
///     IsArchived = false,
///     IsDeleted = false,
///     CreationDate = DateTime.UtcNow
/// };
/// 
/// // Standardized validation using BaseValidationDto
/// var validationResults = BaseValidationDto.ValidateModel(matterMinimal);
/// if (BaseValidationDto.HasValidationErrors(validationResults))
/// {
///     var summary = BaseValidationDto.GetValidationSummary(validationResults);
///     _logger.LogWarning("Matter validation failed: {ValidationSummary}", summary);
/// }
/// 
/// // Using in dropdown population with validation
/// var matters = await GetActiveMattersAsync();
/// foreach (var matter in matters.Where(m => m.IsValid))
/// {
///     matterDropdown.Items.Add(new ListItem(matter.DisplayText, matter.Id.ToString()));
/// }
/// </code>
/// </example>
public class MatterMinimalDto : BaseValidationDto, IEquatable<MatterMinimalDto>
{
    #region Core Properties

    /// <summary>
    /// Gets the unique identifier for the matter.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the matter within the ADMS system.
    /// The ID corresponds directly to the <see cref="ADMS.API.Entities.Matter.Id"/> property and is
    /// used for establishing relationships, foreign key references, and precise matter identification
    /// across all system operations.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateGuid() for consistency.
    /// </remarks>
    [Required(ErrorMessage = "Matter ID is required.")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the matter description.
    /// </summary>
    /// <remarks>
    /// The matter description serves as the primary human-readable identifier and display text for matter
    /// selection and identification. This field corresponds to <see cref="ADMS.API.Entities.Matter.Description"/>
    /// and must conform to professional naming conventions and validation rules.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using MatterValidationHelper for comprehensive validation
    /// including format, length, reserved words, and professional naming standards.
    /// </remarks>
    [Required(ErrorMessage = "Matter description is required.")]
    [StringLength(MatterValidationHelper.MaxDescriptionLength, MinimumLength = MatterValidationHelper.MinDescriptionLength,
        ErrorMessage = "Matter description must be between 3 and 128 characters.")]
    public required string Description { get; init; }

    /// <summary>
    /// Gets a value indicating whether the matter is archived.
    /// </summary>
    /// <remarks>
    /// The archived status indicates whether the matter has been moved to an inactive state for long-term
    /// retention. This property corresponds to <see cref="ADMS.API.Entities.Matter.IsArchived"/> and is
    /// essential for proper matter filtering and display in user interfaces.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCrossPropertyRules() for state consistency with IsDeleted property.
    /// </remarks>
    public bool IsArchived { get; init; }

    /// <summary>
    /// Gets a value indicating whether the matter is deleted.
    /// </summary>
    /// <remarks>
    /// The deletion status indicates whether the matter has been soft-deleted while preserving audit trails
    /// and referential integrity. This property corresponds to <see cref="ADMS.API.Entities.Matter.IsDeleted"/>
    /// and is crucial for proper matter filtering and access control.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCrossPropertyRules() for state consistency with IsArchived property.
    /// </remarks>
    public bool IsDeleted { get; init; }

    /// <summary>
    /// Gets the UTC creation date of the matter.
    /// </summary>
    /// <remarks>
    /// The creation date establishes the temporal foundation for the matter and provides chronological
    /// context for matter identification and sorting. This property corresponds to 
    /// <see cref="ADMS.API.Entities.Matter.CreationDate"/> and is essential for temporal organization
    /// and professional practice management.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateRequiredDate() for temporal consistency.
    /// </remarks>
    [Required(ErrorMessage = "Creation date is required.")]
    public required DateTime CreationDate { get; init; }

    #endregion Core Properties

    #region Computed Properties

    /// <summary>
    /// Gets the normalized description for consistent comparison and search operations.
    /// </summary>
    public string? NormalizedDescription => MatterValidationHelper.NormalizeDescription(Description);

    /// <summary>
    /// Gets the creation date formatted as a localized string for UI display.
    /// </summary>
    public string LocalCreationDateString => CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

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
    /// Gets a value indicating whether this matter DTO has valid data for system operations.
    /// </summary>
    public bool IsValid =>
        Id != Guid.Empty &&
        MatterValidationHelper.IsValidDescription(Description) &&
        MatterValidationHelper.IsValidDate(CreationDate) &&
        MatterValidationHelper.IsValidArchiveState(IsArchived, IsDeleted);

    /// <summary>
    /// Gets the display text suitable for UI controls and matter identification.
    /// </summary>
    public string DisplayText => Description;

    /// <summary>
    /// Gets essential matter metrics for quick analysis and reporting.
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
            LocalCreationDateString,
            DisplayText
        },
        StateInfo = new
        {
            IsArchived,
            IsDeleted,
            CreationDate,
            AgeDays
        },
        ValidationInfo = new
        {
            IsValid
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
    /// validating essential properties using standardized ADMS validation helpers.
    /// 
    /// <para><strong>Validation Steps:</strong></para>
    /// <list type="number">
    /// <item>Matter ID validation using BaseValidationDto.ValidateGuid()</item>
    /// <item>Description validation using MatterValidationHelper.ValidateDescription()</item>
    /// <item>Creation date validation using BaseValidationDto.ValidateRequiredDate()</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCoreProperties()
    {
        // Validate matter ID using standardized GUID validation
        foreach (var result in ValidateGuid(Id, nameof(Id)))
            yield return result;

        // Validate matter description using ADMS validation helper
        foreach (var result in MatterValidationHelper.ValidateDescription(Description, nameof(Description)))
            yield return result;

        // Validate creation date using standardized date validation
        foreach (var result in ValidateRequiredDate(CreationDate, nameof(CreationDate)))
            yield return result;
    }

    /// <summary>
    /// Validates business rules specific to matter management and professional standards.
    /// </summary>
    /// <returns>A collection of validation results for business rule validation.</returns>
    /// <remarks>
    /// This method implements the second step of the BaseValidationDto validation hierarchy,
    /// validating domain-specific business rules for matter management.
    /// 
    /// <para><strong>Business Rules Validated:</strong></para>
    /// <list type="bullet">
    /// <item>Professional naming standards compliance</item>
    /// <item>Legal practice requirements</item>
    /// <item>Matter lifecycle state validity</item>
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

        // Validate professional standards for matter identification
        if (!string.IsNullOrWhiteSpace(Description) && Description.Length < 3)
        {
            yield return CreateValidationResult(
                "Matter description is too short for professional identification standards.",
                nameof(Description));
        }
    }

    /// <summary>
    /// Validates cross-property relationships and state consistency.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    /// <remarks>
    /// This method implements the third step of the BaseValidationDto validation hierarchy,
    /// validating relationships between properties and ensuring state consistency.
    /// 
    /// <para><strong>Cross-Property Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Archive and delete state consistency (deleted matters must be archived)</item>
    /// <item>State transition validity</item>
    /// <item>Temporal consistency constraints</item>
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

        // Validate matter ID and description consistency (ID should not be empty if description exists)
        if (!string.IsNullOrWhiteSpace(Description) && Id == Guid.Empty)
        {
            yield return CreateValidationResult(
                "Matter must have a valid ID when description is provided.",
                nameof(Id), nameof(Description));
        }
    }

    // Note: No collections to validate in minimal DTO, so ValidateCollections() uses default empty implementation
    // Note: No custom validation needed, so ValidateCustomRules() uses default empty implementation

    #endregion Standardized Validation Implementation

    #region Static Methods

    /// <summary>
    /// Creates a MatterMinimalDto from an ADMS.API.Entities.Matter entity with standardized validation.
    /// </summary>
    /// <param name="matter">The Matter entity to convert. Cannot be null.</param>
    /// <returns>A valid MatterMinimalDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when matter is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entity = await context.Matters.FirstAsync(m => m.Id == matterId);
    /// var dto = MatterMinimalDto.FromEntity(entity);
    /// 
    /// // DTO is guaranteed to be valid due to standardized validation
    /// Console.WriteLine($"Created valid DTO: {dto.DisplayText}");
    /// </code>
    /// </example>
    public static MatterMinimalDto FromEntity([NotNull] Entities.Matter matter)
    {
        ArgumentNullException.ThrowIfNull(matter, nameof(matter));

        var dto = new MatterMinimalDto
        {
            Id = matter.Id,
            Description = matter.Description,
            IsArchived = matter.IsArchived,
            IsDeleted = matter.IsDeleted,
            CreationDate = matter.CreationDate
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(dto);
        if (!HasValidationErrors(validationResults)) return dto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Failed to create valid MatterMinimalDto from entity: {summary}");

    }

    /// <summary>
    /// Creates multiple MatterMinimalDto instances from a collection of entities with standardized validation.
    /// </summary>
    /// <param name="matters">The collection of Matter entities to convert. Cannot be null.</param>
    /// <param name="includeInactive">Whether to include archived and deleted matters in the result.</param>
    /// <returns>A collection of valid MatterMinimalDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when matters collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method uses standardized validation and provides detailed error handling
    /// for invalid entities, logging warnings while continuing to process valid entities.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entities = await context.Matters.Where(m => m.IsActive()).ToListAsync();
    /// var dtos = MatterMinimalDto.FromEntities(entities, includeInactive: false);
    /// 
    /// // All DTOs are guaranteed to be valid
    /// var activeMatters = dtos.Where(m => m.IsActive).ToList();
    /// </code>
    /// </example>
    public static IList<MatterMinimalDto> FromEntities([NotNull] IEnumerable<Entities.Matter> matters, bool includeInactive = true)
    {
        ArgumentNullException.ThrowIfNull(matters, nameof(matters));

        var result = new List<MatterMinimalDto>();
        var errors = new List<string>();

        foreach (var matter in matters)
        {
            // Skip inactive matters if not requested
            if (!includeInactive && (!matter.IsActive()))
                continue;

            try
            {
                var dto = FromEntity(matter);
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
    /// Creates a matter minimal DTO with default values and standardized validation.
    /// </summary>
    /// <param name="description">The matter description.</param>
    /// <param name="id">Optional matter ID (generates new GUID if not provided).</param>
    /// <param name="creationDate">Optional creation date (defaults to current UTC time).</param>
    /// <returns>A valid MatterMinimalDto instance.</returns>
    /// <exception cref="ArgumentException">Thrown when description is invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a convenient way to create new matter DTOs with proper defaults
    /// and standardized validation, useful for testing and API operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create with minimal parameters
    /// var matter = MatterMinimalDto.CreateNew("Smith Family Trust");
    /// 
    /// // Create with custom parameters
    /// var customMatter = MatterMinimalDto.CreateNew(
    ///     "ABC Corporation Legal Services",
    ///     Guid.NewGuid(),
    ///     DateTime.UtcNow.AddMonths(-1));
    /// 
    /// // Both are guaranteed to be valid
    /// </code>
    /// </example>
    public static MatterMinimalDto CreateNew(
        [NotNull] string description,
        Guid? id = null,
        DateTime? creationDate = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        var dto = new MatterMinimalDto
        {
            Id = id ?? Guid.NewGuid(),
            Description = description.Trim(),
            IsArchived = false,
            IsDeleted = false,
            CreationDate = creationDate ?? DateTime.UtcNow
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(dto);
        if (!HasValidationErrors(validationResults)) return dto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Failed to create valid MatterMinimalDto: {summary}");

    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this matter can be archived based on business rules.
    /// </summary>
    /// <returns>true if the matter can be archived; otherwise, false.</returns>
    public bool CanBeArchived() => !IsArchived && !IsDeleted;

    /// <summary>
    /// Determines whether this matter can be restored from deleted state.
    /// </summary>
    /// <returns>true if the matter can be restored; otherwise, false.</returns>
    public bool CanBeRestored() => IsDeleted;

    /// <summary>
    /// Gets comprehensive matter information for reporting and analysis.
    /// </summary>
    /// <returns>A dictionary containing detailed matter information and validation status.</returns>
    /// <remarks>
    /// This method provides structured matter information including validation status,
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

        return new Dictionary<string, object>
        {
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
            ["CanBeArchived"] = CanBeArchived(),
            ["CanBeRestored"] = CanBeRestored(),
            [nameof(DisplayText)] = DisplayText,
            ["ValidationStatus"] = validationStatus,
            ["IsValid"] = !HasValidationErrors(validationResults)
        };
    }

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterMinimalDto is equal to the current MatterMinimalDto.
    /// </summary>
    public virtual bool Equals(MatterMinimalDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterMinimalDto.
    /// </summary>
    public override string ToString() => $"Matter: {Description} ({Id}) - {Status}";

    #endregion String Representation
}