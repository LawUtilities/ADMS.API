using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using ADMS.Domain.Common;
using ADMS.Domain.ValueObjects;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing a matter with core properties and navigation collections for complete matter operations.
/// </summary>
/// <remarks>
/// This DTO serves as the standard representation of a matter within the ADMS legal document management system,
/// providing a balanced approach between comprehensive data access and performance optimization. It includes core 
/// matter properties and essential navigation collections while supporting flexible document inclusion strategies.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Balanced Representation:</strong> Core properties with selective document inclusion</item>
/// <item><strong>Domain Alignment:</strong> Maps directly to ADMS.Domain.Entities.Matter</item>
/// <item><strong>Flexible Loading:</strong> Supports both minimal and comprehensive data loading strategies</item>
/// <item><strong>Clean Architecture:</strong> Maintains separation between domain and application layers</item>
/// <item><strong>Validation Integration:</strong> Uses domain validation helpers for consistency</item>
/// </list>
/// 
/// <para><strong>Clean Architecture Integration:</strong></para>
/// <list type="bullet">
/// <item><strong>Domain Entity Mirror:</strong> Directly corresponds to ADMS.Domain.Entities.Matter</item>
/// <item><strong>Application Layer DTO:</strong> Optimized for application service operations</item>
/// <item><strong>Value Object Support:</strong> Works with MatterId and MatterDescription value objects</item>
/// <item><strong>Result Pattern Ready:</strong> Compatible with domain Result pattern implementations</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>API Responses:</strong> Standard matter representation for REST API responses</item>
/// <item><strong>Application Services:</strong> Matter operations within application services</item>
/// <item><strong>CQRS Operations:</strong> Query and command result representation</item>
/// <item><strong>Client Communications:</strong> Professional matter information for client interfaces</item>
/// <item><strong>Business Operations:</strong> Matter lifecycle operations and state management</item>
/// </list>
/// </remarks>
public class MatterDto : IValidatableObject, IEquatable<MatterDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the matter.
    /// </summary>
    /// <remarks>
    /// Maps to the MatterId value object in the domain layer. Represents the unique identity
    /// of the matter throughout the system and serves as the primary key for all matter operations.
    /// </remarks>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the matter description.
    /// </summary>
    /// <remarks>
    /// Maps to the MatterDescription value object in the domain layer. Must be unique across
    /// the system and follows professional legal practice naming conventions.
    /// </remarks>
    [Required(ErrorMessage = "Matter description is required.")]
    [StringLength(128, MinimumLength = 3, ErrorMessage = "Matter description must be between 3 and 128 characters.")]
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the matter is archived.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the matter is soft deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the UTC creation date of the matter.
    /// </summary>
    [Required(ErrorMessage = "Creation date is required.")]
    public required DateTime CreationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the user who created the matter.
    /// </summary>
    [Required(ErrorMessage = "Created by is required.")]
    public required string CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the matter.
    /// </summary>
    [Required(ErrorMessage = "Last modified by is required.")]
    public required string LastModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC date when the matter was last modified.
    /// </summary>
    public DateTime? LastModifiedDate { get; set; }

    #endregion Core Properties

    #region Navigation Collections (Optional)

    /// <summary>
    /// Gets or sets the collection of documents associated with this matter.
    /// </summary>
    /// <remarks>
    /// This collection can be optionally populated based on the specific requirements
    /// of the operation. For performance reasons, it may be loaded separately.
    /// </remarks>
    public ICollection<DocumentDto>? Documents { get; set; }

    /// <summary>
    /// Gets or sets the collection of matter activity users associated with this matter.
    /// </summary>
    /// <remarks>
    /// Contains audit trail information for matter lifecycle operations.
    /// Populated on-demand for audit and reporting scenarios.
    /// </remarks>
    public ICollection<MatterActivityUserDto>? MatterActivityUsers { get; set; }

    #endregion Navigation Collections

    #region Computed Properties

    /// <summary>
    /// Gets the current status of the matter as a descriptive string.
    /// </summary>
    public string Status => (IsDeleted, IsArchived) switch
    {
        (true, true) => "Archived and Deleted",
        (true, false) => "Deleted",
        (false, true) => "Archived",
        (false, false) => "Active"
    };

    /// <summary>
    /// Gets a value indicating whether this matter is currently active.
    /// </summary>
    public bool IsActive => !IsArchived && !IsDeleted;

    /// <summary>
    /// Gets the age of this matter in days since creation.
    /// </summary>
    public double AgeDays => (DateTime.UtcNow - CreationDate).TotalDays;

    /// <summary>
    /// Gets the creation date formatted for display.
    /// </summary>
    public string FormattedCreationDate => CreationDate.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";

    /// <summary>
    /// Gets the number of documents in this matter (if documents are loaded).
    /// </summary>
    public int DocumentCount => Documents?.Count ?? 0;

    /// <summary>
    /// Gets the number of active documents in this matter (if documents are loaded).
    /// </summary>
    public int ActiveDocumentCount => Documents?.Count(d => !d.IsDeleted) ?? 0;

    /// <summary>
    /// Gets a value indicating whether this matter has documents loaded.
    /// </summary>
    public bool HasDocumentsLoaded => Documents != null;

    /// <summary>
    /// Gets the display text for UI presentation.
    /// </summary>
    public string DisplayText => Description;

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the MatterDto using domain validation rules.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate ID (allow empty for creation scenarios)
        if (Id != Guid.Empty && !IsValidGuid(Id))
        {
            yield return new ValidationResult("Invalid matter ID format.", [nameof(Id)]);
        }

        // Validate Description using domain rules
        foreach (var result in ValidateDescription())
        {
            yield return result;
        }

        // Validate creation date
        foreach (var result in ValidateCreationDate())
        {
            yield return result;
        }

        // Validate audit fields
        foreach (var result in ValidateAuditFields())
        {
            yield return result;
        }

        // Validate business rules
        foreach (var result in ValidateBusinessRules())
        {
            yield return result;
        }
    }

    private IEnumerable<ValidationResult> ValidateDescription()
    {
        if (string.IsNullOrWhiteSpace(Description))
        {
            yield return new ValidationResult("Description is required.", [nameof(Description)]);
            yield break;
        }

        if (Description.Length < 3)
        {
            yield return new ValidationResult("Description must be at least 3 characters long.", [nameof(Description)]);
        }

        if (Description.Length > 128)
        {
            yield return new ValidationResult("Description cannot exceed 128 characters.", [nameof(Description)]);
        }

        if (Description.Trim() != Description)
        {
            yield return new ValidationResult("Description should not have leading or trailing whitespace.", [nameof(Description)]);
        }
    }

    private IEnumerable<ValidationResult> ValidateCreationDate()
    {
        if (CreationDate == default)
        {
            yield return new ValidationResult("Creation date is required.", [nameof(CreationDate)]);
        }

        if (CreationDate > DateTime.UtcNow.AddMinutes(5))
        {
            yield return new ValidationResult("Creation date cannot be in the future.", [nameof(CreationDate)]);
        }

        if (CreationDate < new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc))
        {
            yield return new ValidationResult("Creation date is unreasonably far in the past.", [nameof(CreationDate)]);
        }
    }

    private IEnumerable<ValidationResult> ValidateAuditFields()
    {
        if (string.IsNullOrWhiteSpace(CreatedBy))
        {
            yield return new ValidationResult("Created by is required.", [nameof(CreatedBy)]);
        }

        if (string.IsNullOrWhiteSpace(LastModifiedBy))
        {
            yield return new ValidationResult("Last modified by is required.", [nameof(LastModifiedBy)]);
        }

        if (LastModifiedDate.HasValue && LastModifiedDate < CreationDate)
        {
            yield return new ValidationResult("Last modified date cannot be before creation date.", [nameof(LastModifiedDate)]);
        }
    }

    private IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Business rule: Deleted matters should be archived
        if (IsDeleted && !IsArchived)
        {
            yield return new ValidationResult("Deleted matters must be archived.", [nameof(IsDeleted), nameof(IsArchived)]);
        }

        // Validate age reasonableness for business context
        if (AgeDays > 365 * 20) // 20 years
        {
            yield return new ValidationResult("Matter age exceeds reasonable bounds for active practice.", [nameof(CreationDate)]);
        }
    }

    private static bool IsValidGuid(Guid guid)
    {
        return guid != Guid.Empty;
    }

    #endregion Validation Implementation

    #region Static Factory Methods

    /// <summary>
    /// Creates a new MatterDto with validation.
    /// </summary>
    public static Result<MatterDto> Create(string description, string createdBy, DateTime? creationDate = null)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<MatterDto>(DomainError.Create("MATTER_DESCRIPTION_REQUIRED", "Description is required"));
        }

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            return Result.Failure<MatterDto>(DomainError.Create("MATTER_CREATED_BY_REQUIRED", "Created by is required"));
        }

        var dto = new MatterDto
        {
            Id = Guid.NewGuid(),
            Description = description.Trim(),
            CreationDate = creationDate ?? DateTime.UtcNow,
            CreatedBy = createdBy,
            LastModifiedBy = createdBy,
            IsArchived = false,
            IsDeleted = false
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return Result.Success(dto);
        var errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        return Result.Failure<MatterDto>(DomainError.Create("VALIDATION_FAILED", errors));

    }

    /// <summary>
    /// Creates a MatterDto from a domain Matter entity.
    /// </summary>
    public static MatterDto FromDomainEntity(ADMS.Domain.Entities.Matter matter)
    {
        ArgumentNullException.ThrowIfNull(matter);

        return new MatterDto
        {
            Id = matter.Id.Value,
            Description = matter.Description.Value,
            IsArchived = matter.IsArchived,
            IsDeleted = matter.IsDeleted,
            CreationDate = matter.CreationDate,
            CreatedBy = matter.CreatedBy,
            LastModifiedBy = matter.LastModifiedBy,
            LastModifiedDate = matter.LastModifiedDate
        };
    }

    /// <summary>
    /// Validates a MatterDto and returns validation results.
    /// </summary>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterDto instance is required."));
            return results;
        }

        var context = new ValidationContext(dto);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    #endregion Static Factory Methods

    #region Business Methods

    /// <summary>
    /// Determines whether this matter can be archived based on current state.
    /// </summary>
    public bool CanBeArchived() => !IsArchived && !IsDeleted;

    /// <summary>
    /// Determines whether this matter can be restored from deleted state.
    /// </summary>
    public bool CanBeRestored() => IsDeleted;

    /// <summary>
    /// Determines whether this matter can be deleted safely.
    /// </summary>
    public bool CanBeDeleted() => !IsDeleted && (!HasDocumentsLoaded || ActiveDocumentCount == 0);

    /// <summary>
    /// Updates the last modified information.
    /// </summary>
    public void UpdateLastModified(string modifiedBy)
    {
        LastModifiedBy = modifiedBy ?? throw new ArgumentNullException(nameof(modifiedBy));
        LastModifiedDate = DateTime.UtcNow;
    }

    #endregion Business Methods

    #region Equality Implementation

    public bool Equals(MatterDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // Compare by ID if both have valid IDs
        if (Id != Guid.Empty && other.Id != Guid.Empty)
        {
            return Id == other.Id;
        }

        // Otherwise compare by content
        return Description == other.Description &&
               IsArchived == other.IsArchived &&
               IsDeleted == other.IsDeleted;
    }

    public override bool Equals(object? obj) => Equals(obj as MatterDto);

    public override int GetHashCode()
    {
        return Id != Guid.Empty ? Id.GetHashCode() : HashCode.Combine(Description, IsArchived, IsDeleted);
    }

    public static bool operator ==(MatterDto? left, MatterDto? right) => EqualityComparer<MatterDto>.Default.Equals(left, right);
    public static bool operator !=(MatterDto? left, MatterDto? right) => !(left == right);

    #endregion Equality Implementation

    #region String Representation

    public override string ToString()
    {
        return $"Matter: {Description} ({Status})";
    }

    #endregion String Representation
}