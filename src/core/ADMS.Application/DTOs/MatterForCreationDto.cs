using ADMS.Application.Common;
using ADMS.Application.Constants;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using DomainError = ADMS.Domain.Common.DomainError;
using Result = ADMS.Domain.Common.Result;

namespace ADMS.Application.DTOs;

/// <summary>
/// Data Transfer Object for creating new matters with comprehensive validation.
/// </summary>
public record MatterForCreationDto : IValidatableObject
{
    #region Creation Properties

    /// <summary>
    /// Gets the description of the matter.
    /// </summary>
    [Required(ErrorMessage = "Matter description is required.")]
    [StringLength(MatterConstants.DescriptionMaxLength,
        MinimumLength = MatterConstants.DescriptionMinLength,
        ErrorMessage = "Matter description must be between 3 and 128 characters.")]
    public required string Description { get; init; }

    /// <summary>
    /// Gets a value indicating whether the item is archived.
    /// </summary>
    public bool IsArchived { get; init; }

    /// <summary>
    /// Gets the creation date of the entity.
    /// </summary>
    [Required(ErrorMessage = "Creation date is required.")]
    public required DateTime CreationDate { get; init; } = DateTime.UtcNow;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets the normalized version of the description.
    /// </summary>
    [NotMapped]
    public string? NormalizedDescription => MatterValidationHelper.NormalizeDescription(Description);

    /// <summary>
    /// Gets the creation date and time of the entity as a localized string.
    /// </summary>
    [NotMapped]
    public string LocalCreationDateString => CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the initial status of the entity based on its archived state.
    /// </summary>
    [NotMapped]
    public string InitialStatus => IsArchived ? "Archived" : "Active";

    /// <summary>
    /// Gets the initial age of the entity, in days, calculated as the difference between the current UTC time and the
    /// creation date.
    /// </summary>
    [NotMapped]
    public double InitialAgeDays => (DateTime.UtcNow - CreationDate).TotalDays;

    /// <summary>
    /// Gets a value indicating whether the current object is valid based on its description and creation date.
    /// </summary>
    [NotMapped]
    public bool IsValid => MatterValidationHelper.IsValidDescription(Description) &&
                          MatterValidationHelper.IsValidDate(CreationDate);

    /// <summary>
    /// Gets the display text for the entity.
    /// </summary>
    [NotMapped]
    public string DisplayText => Description;

    #endregion

    #region Validation Implementation

    /// <summary>
    /// Validates the current object based on the provided validation context.
    /// </summary>
    /// <remarks>This method performs validation on the object's properties, including checks for the
    /// description, creation date,  and compliance with specific business rules. It ensures that the description is
    /// valid and normalized, the creation  date is within acceptable limits, and other domain-specific rules are
    /// satisfied.</remarks>
    /// <param name="validationContext">The context in which the validation is performed. This provides additional information about the object being
    /// validated.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ValidationResult"/> objects that represent the validation errors,
    /// if any. If the object is valid, the collection will be empty.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate description
        foreach (var result in MatterValidationHelper.ValidateDescription(Description, nameof(Description)))
            yield return result;

        // Validate creation date for creation scenarios
        foreach (var result in MatterValidationHelper.ValidateDate(CreationDate, nameof(CreationDate)))
            yield return result;

        // Additional validation for creation-specific business rules
        if (InitialAgeDays > MatterConstants.MaxBackdatedDays)
        {
            yield return new ValidationResult(
                $"Creation date is more than {MatterConstants.MaxBackdatedDays / 365} years in the past. Please verify the date is correct.",
                [nameof(CreationDate)]);
        }

        // Validate description normalization
        if (string.IsNullOrEmpty(NormalizedDescription))
        {
            yield return new ValidationResult(
                "Description cannot be normalized properly. Please ensure it contains valid characters.",
                [nameof(Description)]);
        }
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Validates the specified <see cref="MatterForCreationDto"/> instance and returns a list of validation results.
    /// </summary>
    /// <remarks>
    /// This method uses the <see cref="Validator.TryValidateObject(object, ValidationContext, ICollection{ValidationResult}, bool)"/>
    /// method to perform validation on the provided object. Ensure that the <see cref="MatterForCreationDto"/> class is decorated with appropriate
    /// data annotations for validation.
    /// </remarks>
    /// <param name="dto">The <see cref="MatterForCreationDto"/> instance to validate. This parameter can be null.</param>
    /// <returns>A list of <see cref="ValidationResult"/> objects representing the validation errors.  If the <paramref
    /// name="dto"/> is null, the list will contain a single validation error indicating that the instance is required.
    /// If the object is valid, the list will be empty.</returns>
    public static IList<ValidationResult> ValidateModel(MatterForCreationDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterForCreationDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
        return results;
    }

    /// <summary>
    /// Creates a new MatterForCreationDto with current timestamp and active state.
    /// </summary>
    public static Domain.Common.Result<MatterForCreationDto> CreateNew(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<MatterForCreationDto>(DomainError.Create(
                "DESCRIPTION_REQUIRED", "Description cannot be null or empty"));

        var dto = new MatterForCreationDto
        {
            Description = description.Trim(),
            CreationDate = DateTime.UtcNow,
            IsArchived = false
        };

        var validationResults = ValidateModel(dto);
        if (validationResults.Count <= 0) return Result.Success(dto);
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        return Result.Failure<MatterForCreationDto>(DomainError.Create(
            "VALIDATION_FAILED", $"Validation failed: {errorMessages}"));

    }

    /// <summary>
    /// Creates multiple MatterForCreationDto instances efficiently.
    /// </summary>
    public static Domain.Common.Result<IList<MatterForCreationDto>> CreateMultiple(IEnumerable<string> descriptions, bool isArchived = false)
    {
        var results = new List<MatterForCreationDto>();
        var errors = new List<string>();
        var currentTime = DateTime.UtcNow;

        foreach (var description in descriptions.Where(d => !string.IsNullOrWhiteSpace(d)))
        {
            var result = CreateWithParameters(description.Trim(), currentTime, isArchived);
            if (result.IsSuccess)
            {
                results.Add(result.Value);
            }
            else
            {
                errors.Add($"'{description}': {result.Error?.Message}");
            }
        }

        return errors.Count > 0
            ? Result.Failure<IList<MatterForCreationDto>>(DomainError.Create(
                "BULK_CREATION_PARTIAL_FAILURE",
                $"Some items failed validation: {string.Join("; ", errors)}"))
            : Result.Success<IList<MatterForCreationDto>>(results);
    }

    private static Domain.Common.Result<MatterForCreationDto> CreateWithParameters(string description, DateTime creationDate, bool isArchived)
    {
        var dto = new MatterForCreationDto
        {
            Description = description,
            CreationDate = creationDate,
            IsArchived = isArchived
        };

        var validationResults = ValidateModel(dto);
        return validationResults.Count > 0
            ? Result.Failure<MatterForCreationDto>(DomainError.Create(
                "VALIDATION_FAILED", string.Join(", ", validationResults.Select(r => r.ErrorMessage))))
            : Result.Success(dto);
    }

    #endregion

    #region Business Logic Methods

    /// <summary>
    /// Determines whether the creation is considered backdated based on the initial age in days.
    /// </summary>
    /// <returns><see langword="true"/> if the initial age in days is greater than 1; otherwise, <see langword="false"/>.</returns>
    public bool IsBackdatedCreation() => InitialAgeDays > 1;

    /// <summary>
    /// Determines whether the creation is considered historical based on its age.
    /// </summary>
    /// <returns><see langword="true"/> if the creation is older than 30 days; otherwise, <see langword="false"/>.</returns>
    public bool IsHistoricalCreation() => InitialAgeDays > 30;

    /// <summary>
    /// Creates and returns a summary of the current object's creation details.
    /// </summary>
    /// <remarks>The returned summary includes key information such as the description, creation date, 
    /// status, and various flags indicating the state of the creation. If the normalized  description is not available,
    /// it defaults to "N/A".</remarks>
    /// <returns>A <see cref="CreationSummary"/> object containing the creation details of the current object.</returns>
    public CreationSummary GetCreationSummary() => new()
    {
        Description = Description,
        NormalizedDescription = NormalizedDescription ?? "N/A",
        InitialStatus = InitialStatus,
        IsArchived = IsArchived,
        CreationDate = CreationDate,
        LocalCreationDateString = LocalCreationDateString,
        InitialAgeDays = InitialAgeDays,
        IsBackdatedCreation = IsBackdatedCreation(),
        IsHistoricalCreation = IsHistoricalCreation(),
        IsValid = IsValid
    };

    #endregion

    #region Equality and String Representation

    /// <summary>
    /// Determines whether the specified <see cref="MatterForCreationDto"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="MatterForCreationDto"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="MatterForCreationDto"/> is equal to the current instance;
    /// otherwise, <see langword="false"/>.</returns>
    public virtual bool Equals(MatterForCreationDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase) &&
               IsArchived == other.IsArchived &&
               CreationDate == other.CreationDate;
    }

    /// <summary>
    /// Returns a hash code for the current object.
    /// </summary>
    /// <remarks>The hash code is computed based on the description (case-insensitive), the archived status,
    /// and the creation date of the object.</remarks>
    /// <returns>An integer representing the hash code for the current object.</returns>
    public override int GetHashCode() => HashCode.Combine(
        Description.GetHashCode(StringComparison.OrdinalIgnoreCase),
        IsArchived,
        CreationDate);

    /// <summary>
    /// Returns a string representation of the object, including the description and initial status of the matter.
    /// </summary>
    /// <returns>A string in the format "Create Matter: {Description} - Status: {InitialStatus}".</returns>
    public override string ToString() => $"Create Matter: {Description} - Status: {InitialStatus}";

    #endregion
}