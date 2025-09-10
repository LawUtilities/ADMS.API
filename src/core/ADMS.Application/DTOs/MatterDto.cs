using ADMS.Application.Common.Validation;
using ADMS.Application.Constants;
using ADMS.Domain.Common;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing a matter with streamlined validation and professional standards.
/// </summary>
/// <remarks>
/// This DTO serves as the standard representation of a matter within the ADMS legal document management system,
/// providing a balanced approach between comprehensive data access and performance optimization. It includes core 
/// matter properties and essential navigation collections while supporting flexible document inclusion strategies.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Streamlined Validation:</strong> Uses BaseValidationDto for consistent, performant validation</item>
/// <item><strong>Domain Alignment:</strong> Maps directly to ADMS.Domain.Entities.Matter</item>
/// <item><strong>Professional Standards:</strong> Enforces legal practice naming conventions and business rules</item>
/// <item><strong>Clean Architecture:</strong> Maintains separation between domain and application layers</item>
/// <item><strong>Validation Integration:</strong> Uses domain validation helpers for consistency</item>
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
public sealed class MatterDto : BaseValidationDto, IEquatable<MatterDto>
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
    [StringLength(MatterConstants.DescriptionMaxLength, MinimumLength = MatterConstants.DescriptionMinLength,
        ErrorMessage = "Matter description must be between 3 and 128 characters.")]
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
    [StringLength(UserValidationHelper.MaxUserNameLength, MinimumLength = UserValidationHelper.MinUserNameLength,
        ErrorMessage = "Created by must be between 2 and 50 characters.")]
    public required string CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the matter.
    /// </summary>
    [Required(ErrorMessage = "Last modified by is required.")]
    [StringLength(UserValidationHelper.MaxUserNameLength, MinimumLength = UserValidationHelper.MinUserNameLength,
        ErrorMessage = "Last modified by must be between 2 and 50 characters.")]
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
    public string FormattedCreationDate => CreationDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC";

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

    /// <summary>
    /// Gets the normalized description using validation helper.
    /// </summary>
    public string NormalizedDescription => MatterValidationHelper.NormalizeDescription(Description);

    /// <summary>
    /// Gets the local creation date string for display.
    /// </summary>
    public string LocalCreationDateString => CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss", CultureInfo.InvariantCulture);

    #endregion Computed Properties

    #region Validation Implementation (BaseValidationDto)

    /// <summary>
    /// Validates core properties such as ID, description, and creation date.
    /// </summary>
    /// <returns>A collection of validation results for core property validation.</returns>
    protected override IEnumerable<ValidationResult> ValidateCoreProperties()
    {
        // Validate ID (allow empty for creation scenarios)
        foreach (var result in ValidateGuid(Id, nameof(Id), allowEmpty: true))
            yield return result;

        // Validate Description using MatterValidationHelper
        foreach (var result in MatterValidationHelper.ValidateDescription(Description, nameof(Description)))
            yield return result;

        // Validate CreationDate using MatterValidationHelper
        foreach (var result in MatterValidationHelper.ValidateCreationDate(CreationDate, nameof(CreationDate)))
            yield return result;

        // Validate user fields using UserValidationHelper
        foreach (var result in UserValidationHelper.ValidateUsername(CreatedBy, nameof(CreatedBy)))
            yield return result;

        foreach (var result in UserValidationHelper.ValidateUsername(LastModifiedBy, nameof(LastModifiedBy)))
            yield return result;
    }

    /// <summary>
    /// Validates business rules and domain-specific logic.
    /// </summary>
    /// <returns>A collection of validation results for business rule validation.</returns>
    protected override IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Validate matter state consistency using MatterValidationHelper
        foreach (var result in MatterValidationHelper.ValidateStateConsistency(IsArchived, IsDeleted))
            yield return result;

        // Validate audit fields consistency
        foreach (var result in MatterValidationHelper.ValidateAuditFields(
            CreatedBy, LastModifiedBy, LastModifiedDate, CreationDate))
            yield return result;

        // Validate business rules using MatterValidationHelper
        foreach (var result in MatterValidationHelper.ValidateBusinessRules(
            Description, CreationDate, IsArchived, IsDeleted))
            yield return result;

        // Age validation for business context
        if (AgeDays > MatterConstants.MaxHistoricalYears * 365)
        {
            yield return CreateValidationResult(
                $"Matter age exceeds reasonable bounds for active practice ({MatterConstants.MaxHistoricalYears} years).",
                nameof(CreationDate));
        }
    }

    /// <summary>
    /// Validates relationships and constraints between multiple properties.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    protected override IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // Validate LastModifiedDate relationship
        if (LastModifiedDate < CreationDate)
        {
            yield return CreateValidationResult(
                "Last modified date cannot be before creation date.",
                nameof(LastModifiedDate), nameof(CreationDate));
        }

        // Validate normalized description consistency
        var normalizedInput = MatterValidationHelper.NormalizeDescription(Description);
        if (!string.IsNullOrEmpty(normalizedInput) && normalizedInput != Description.Trim())
        {
            yield return CreateValidationResult(
                "Description contains formatting that will be normalized. Consider using the normalized format.",
                nameof(Description));
        }
    }

    /// <summary>
    /// Validates collections and nested objects.
    /// </summary>
    /// <returns>A collection of validation results for collection validation.</returns>
    protected override IEnumerable<ValidationResult> ValidateCollections()
    {
        // Validate Documents collection if loaded
        if (Documents != null)
        {
            foreach (var result in ValidateCollection(Documents, nameof(Documents), maxItems: MatterConstants.LargeDocumentCollectionThreshold))
                yield return result;
        }

        // Validate MatterActivityUsers collection if loaded
        if (MatterActivityUsers != null)
        {
            foreach (var result in ValidateCollection(MatterActivityUsers, nameof(MatterActivityUsers), maxItems: MatterConstants.HighActivityCountThreshold))
                yield return result;
        }
    }

    #endregion Validation Implementation

    #region Static Factory Methods

    /// <summary>
    /// Creates a new MatterDto with validation.
    /// </summary>
    /// <param name="description">The matter description.</param>
    /// <param name="createdBy">The user creating the matter.</param>
    /// <param name="creationDate">Optional creation date (defaults to UtcNow).</param>
    /// <returns>A Result containing either the MatterDto or validation errors.</returns>
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
            CreatedBy = createdBy.Trim(),
            LastModifiedBy = createdBy.Trim(),
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
    /// <param name="matter">The domain matter entity.</param>
    /// <param name="includeDocuments">Whether to include document collections.</param>
    /// <returns>A MatterDto instance.</returns>
    /// <remarks>
    /// Maps from Domain layer Matter entity to Application layer DTO.
    /// Document collections are excluded by default for performance.
    /// </remarks>
    public static MatterDto FromDomainEntity(ADMS.Domain.Entities.Matter matter, bool includeDocuments = false)
    {
        ArgumentNullException.ThrowIfNull(matter);

        var dto = new MatterDto
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

        // Document collections would be mapped here if needed
        if (includeDocuments)
        {
            // Note: In a real implementation, you would map the document collections
            // This would typically be done by a mapping framework like Mapster or AutoMapper
            // dto.Documents = matter.Documents?.Select(DocumentDto.FromDomainEntity).ToList();
        }

        return dto;
    }

    /// <summary>
    /// Validates a MatterDto instance and returns validation results.
    /// </summary>
    /// <param name="dto">The MatterDto to validate.</param>
    /// <returns>A list of validation results.</returns>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterDto? dto)
    {
        return BaseValidationDto.ValidateModel(dto);
    }

    #endregion Static Factory Methods

    #region Business Methods

    /// <summary>
    /// Determines whether this matter can be archived based on current state.
    /// </summary>
    /// <returns>True if the matter can be archived; otherwise, false.</returns>
    public bool CanBeArchived() => !IsArchived && !IsDeleted;

    /// <summary>
    /// Determines whether this matter can be restored from deleted state.
    /// </summary>
    /// <returns>True if the matter can be restored; otherwise, false.</returns>
    public bool CanBeRestored() => IsDeleted;

    /// <summary>
    /// Determines whether this matter can be deleted safely.
    /// </summary>
    /// <returns>True if the matter can be deleted; otherwise, false.</returns>
    public bool CanBeDeleted() => !IsDeleted && (!HasDocumentsLoaded || ActiveDocumentCount == 0);

    /// <summary>
    /// Updates the last modified information for audit purposes.
    /// </summary>
    /// <param name="modifiedBy">The user making the modification.</param>
    /// <exception cref="ArgumentException">Thrown when modifiedBy is null or whitespace.</exception>
    public void UpdateLastModified(string modifiedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modifiedBy);

        LastModifiedBy = modifiedBy.Trim();
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Determines if matter data is sufficient for professional legal practice requirements.
    /// </summary>
    /// <returns>True if data is sufficient; otherwise, false.</returns>
    public bool IsSufficientMatterData()
    {
        return MatterValidationHelper.IsSufficientMatterData(Description, CreationDate, CreatedBy);
    }

    #endregion Business Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterDto is equal to the current MatterDto.
    /// </summary>
    /// <param name="other">The MatterDto to compare with the current MatterDto.</param>
    /// <returns>True if the specified MatterDto is equal to the current MatterDto; otherwise, false.</returns>
    public bool Equals(MatterDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // Compare by ID if both have valid IDs
        if (Id != Guid.Empty && other.Id != Guid.Empty)
        {
            return Id == other.Id;
        }

        // Otherwise compare by normalized content
        var thisNormalized = MatterValidationHelper.NormalizeDescription(Description);
        var otherNormalized = MatterValidationHelper.NormalizeDescription(other.Description);

        return string.Equals(thisNormalized, otherNormalized, StringComparison.OrdinalIgnoreCase) &&
               IsArchived == other.IsArchived &&
               IsDeleted == other.IsDeleted;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current MatterDto.
    /// </summary>
    /// <param name="obj">The object to compare with the current MatterDto.</param>
    /// <returns>True if the specified object is equal to the current MatterDto; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as MatterDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterDto.</returns>
    public override int GetHashCode()
    {
        if (Id != Guid.Empty) return Id.GetHashCode();

        var normalizedDescription = MatterValidationHelper.NormalizeDescription(Description);
        return HashCode.Combine(
            normalizedDescription?.GetHashCode(StringComparison.OrdinalIgnoreCase),
            IsArchived,
            IsDeleted
        );
    }

    /// <summary>
    /// Determines whether two specified MatterDto instances are equal.
    /// </summary>
    /// <param name="left">The first MatterDto to compare.</param>
    /// <param name="right">The second MatterDto to compare.</param>
    /// <returns>True if the MatterDto instances are equal; otherwise, false.</returns>
    public static bool operator ==(MatterDto? left, MatterDto? right) => EqualityComparer<MatterDto>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two specified MatterDto instances are not equal.
    /// </summary>
    /// <param name="left">The first MatterDto to compare.</param>
    /// <param name="right">The second MatterDto to compare.</param>
    /// <returns>True if the MatterDto instances are not equal; otherwise, false.</returns>
    public static bool operator !=(MatterDto? left, MatterDto? right) => !(left == right);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string that represents the current MatterDto.
    /// </summary>
    /// <returns>A string that represents the current MatterDto.</returns>
    public override string ToString()
    {
        return $"Matter: {Description} ({Status})";
    }

    #endregion String Representation
}