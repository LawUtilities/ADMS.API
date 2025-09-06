using ADMS.Application.Common;
using ADMS.Application.Constants;
using ADMS.Domain.Common;
using ADMS.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DomainError = ADMS.Domain.Common.DomainError;
using Result = ADMS.Domain.Common.Result;

namespace ADMS.Application.DTOs;

/// <summary>
/// Represents a data transfer object (DTO) for updating a matter, including its description, archive status, and
/// creation date.
/// </summary>
/// <remarks>This DTO is used to encapsulate the data required for updating a matter. It includes validation logic
/// to ensure that the provided data adheres to the expected constraints. The object also provides computed properties
/// for additional derived information, such as the normalized description, the age of the matter in days, and a
/// localized string representation of the creation date.</remarks>
public record MatterForUpdateDto : IValidatableObject
{
    #region Updatable Properties

    /// <summary>
    /// Gets or initializes the description of the matter.
    /// </summary>
    [StringLength(
        MatterConstants.DescriptionMaxLength,
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
    public string NormalizedDescription => MatterValidationHelper.NormalizeDescription(Description) ?? string.Empty;

    /// <summary>
    /// Gets the creation date and time of the entity as a localized string.
    /// </summary>
    [NotMapped]
    public string LocalCreationDateString => CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the age of the entity in days, calculated as the difference between the current UTC time and the creation
    /// date.
    /// </summary>
    [NotMapped]
    public double AgeDays => (DateTime.UtcNow - CreationDate).TotalDays;

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
    /// Validates the current object based on its properties and returns a collection of validation results.
    /// </summary>
    /// <remarks>This method performs validation on the <see cref="Description"/>, <see cref="CreationDate"/>,
    /// and other related properties. It ensures that the description is valid, the creation date does not result in an
    /// age exceeding the maximum allowed years,  and that the description can be normalized. If any validation rule is
    /// violated, a corresponding <see cref="ValidationResult"/>  is returned.</remarks>
    /// <param name="validationContext">The context in which the validation is performed. This parameter provides additional information about the
    /// validation process.</param>
    /// <returns>An <see cref="IEnumerable{ValidationResult}"/> containing the validation results. The collection will be empty
    /// if the object is valid.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in MatterValidationHelper.ValidateDescription(Description, nameof(Description)))
            yield return result;

        foreach (var result in MatterValidationHelper.ValidateDate(CreationDate, nameof(CreationDate)))
            yield return result;

        // Additional validation for update scenarios
        if (AgeDays > MatterConstants.MaxHistoricalYears * 365)
        {
            yield return new ValidationResult(
                $"Creation date results in matter age exceeding {MatterConstants.MaxHistoricalYears} years. Please verify.",
                [nameof(CreationDate)]);
        }

        if (string.IsNullOrEmpty(NormalizedDescription))
        {
            yield return new ValidationResult(
                "Description cannot be normalized properly.",
                [nameof(Description)]);
        }
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Validates the specified <see cref="MatterForUpdateDto"/> instance and returns a collection of validation
    /// results.
    /// </summary>
    /// <remarks>
    /// This method uses the <see cref="Validator.TryValidateObject(object, ValidationContext, ICollection{ValidationResult}, bool)"/>
    /// method to perform validation on the provided object. Ensure that the <see cref="MatterForUpdateDto"/> class is decorated with appropriate
    /// data annotations for validation.
    /// </remarks>
    /// <param name="dto">The <see cref="MatterForUpdateDto"/> instance to validate. This parameter can be null.</param>
    /// <returns>A collection of <see cref="ValidationResult"/> objects that describe any validation errors.  If the <paramref
    /// name="dto"/> is null, the collection will contain a single validation error indicating that the instance is
    /// required. If the object is valid, the collection will be empty.</returns>
    public static IList<ValidationResult> ValidateModel(MatterForUpdateDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterForUpdateDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
        return results;
    }

    /// <summary>
    /// Converts a <see cref="Matter"/> entity into a <see cref="MatterForUpdateDto"/> object.
    /// </summary>
    /// <remarks>This method validates the resulting <see cref="MatterForUpdateDto"/> object after conversion.
    /// If the validation fails, the method returns a failure result with the validation errors.</remarks>
    /// <param name="matter">The <see cref="Matter"/> entity to convert. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="Result{T}"/> containing the converted <see cref="MatterForUpdateDto"/> if the operation succeeds; 
    /// otherwise, a failure result containing the relevant error details.</returns>
    public static Domain.Common.Result<MatterForUpdateDto> FromEntity(Matter matter)
    {
        var dto = new MatterForUpdateDto
        {
            Description = matter.Description,
            IsArchived = matter.IsArchived,
            CreationDate = matter.CreationDate
        };

        var validationResults = ValidateModel(dto);
        return validationResults.Count > 0
            ? Result.Failure<MatterForUpdateDto>(DomainError.Create(
                "VALIDATION_FAILED", string.Join(", ", validationResults.Select(r => r.ErrorMessage))))
            : Result.Success(dto);
    }

    #endregion

    #region Change Tracking Methods

    /// <summary>
    /// Analyzes changes compared to current matter state.
    /// </summary>
    public ChangeAnalysis AnalyzeChanges(Matter currentMatter)
    {
        ArgumentNullException.ThrowIfNull(currentMatter);

        return new ChangeAnalysis
        {
            HasChanges = HasAnyChanges(currentMatter),
            DescriptionChanged = IsDescriptionChange(currentMatter),
            ArchiveStatusChanged = IsArchiveStatusChange(currentMatter),
            CreationDateChanged = IsCreationDateChange(currentMatter),
            Changes = GetDetailedChanges(currentMatter)
        };
    }

    /// <summary>
    /// Determines whether the description of the current matter differs from the specified matter.
    /// </summary>
    /// <param name="currentMatter">The matter to compare against the current instance.</param>
    /// <returns><see langword="true"/> if the descriptions are different; otherwise, <see langword="false"/>.</returns>
    public bool IsDescriptionChange(Matter currentMatter) =>
        !string.Equals(Description, currentMatter.Description, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the archive status of the current matter differs from the specified matter.
    /// </summary>
    /// <param name="currentMatter">The matter to compare against the current instance.</param>
    /// <returns><see langword="true"/> if the archive status of the current matter differs from the specified matter; 
    /// otherwise, <see langword="false"/>.</returns>
    public bool IsArchiveStatusChange(Matter currentMatter) =>
        IsArchived != currentMatter.IsArchived;

    /// <summary>
    /// Determines whether the creation date of the current matter differs from the specified matter.
    /// </summary>
    /// <param name="currentMatter">The matter to compare against the current instance.</param>
    /// <returns><see langword="true"/> if the creation date of the current matter is different from the specified matter;
    /// otherwise, <see langword="false"/>.</returns>
    public bool IsCreationDateChange(Matter currentMatter) =>
        CreationDate != currentMatter.CreationDate;

    /// <summary>
    /// Determines whether any changes have occurred in the specified matter.
    /// </summary>
    /// <param name="currentMatter">The current state of the matter to compare against.</param>
    /// <returns><see langword="true"/> if there are changes in the matter's description, archive status, or creation date;
    /// otherwise, <see langword="false"/>.</returns>
    private bool HasAnyChanges(Matter currentMatter) =>
        IsDescriptionChange(currentMatter) ||
        IsArchiveStatusChange(currentMatter) ||
        IsCreationDateChange(currentMatter);

    /// <summary>
    /// Compares the current instance of a matter with the specified matter and identifies detailed property changes.
    /// </summary>
    /// <remarks>This method evaluates specific properties of the matter, such as <see cref="Description"/>,
    /// <see cref="IsArchived"/>, and <see cref="CreationDate"/>, to determine if their values differ between the
    /// current instance and the specified <paramref name="currentMatter"/>.</remarks>
    /// <param name="currentMatter">The matter to compare against the current instance.</param>
    /// <returns>A list of <see cref="PropertyChange"/> objects representing the properties that have changed, including their
    /// previous and current values. Returns an empty list if no changes are detected.</returns>
    private List<PropertyChange> GetDetailedChanges(Matter currentMatter)
    {
        var changes = new List<PropertyChange>();

        if (IsDescriptionChange(currentMatter))
            changes.Add(new PropertyChange(nameof(Description), currentMatter.Description, Description));

        if (IsArchiveStatusChange(currentMatter))
            changes.Add(new PropertyChange(nameof(IsArchived), currentMatter.IsArchived, IsArchived));

        if (IsCreationDateChange(currentMatter))
            changes.Add(new PropertyChange(nameof(CreationDate), currentMatter.CreationDate, CreationDate));

        return changes;
    }

    #endregion

    #region Equality and String Representation

    /// <summary>
    /// Determines whether the specified <see cref="MatterForUpdateDto"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="MatterForUpdateDto"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="MatterForUpdateDto"/> is equal to the current instance;
    /// otherwise, <see langword="false"/>.</returns>
    public virtual bool Equals(MatterForUpdateDto? other)
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
    /// <remarks>The hash code is computed based on the description (case-insensitive), the archived state, 
    /// and the creation date of the object. This ensures that objects with the same values for  these properties
    /// produce the same hash code.</remarks>
    /// <returns>A 32-bit signed integer that serves as the hash code for the current object.</returns>
    public override int GetHashCode() => HashCode.Combine(
        Description.GetHashCode(StringComparison.OrdinalIgnoreCase),
        IsArchived,
        CreationDate);

    /// <summary>
    /// Returns a string representation of the update matter, including its description and archive status.
    /// </summary>
    /// <returns>A string that contains the description of the update matter and its archive status in the format: "Update
    /// Matter: {Description} - Archive: {IsArchived}".</returns>
    public override string ToString() => $"Update Matter: {Description} - Archive: {IsArchived}";

    #endregion
}

/// <summary>
/// Represents a summary of the creation details for an object, including its description, status,  creation date, and
/// other metadata.
/// </summary>
/// <remarks>This record provides a comprehensive overview of an object's creation-related information,  such as
/// its description, status, and creation date, along with additional metadata like  whether the object is archived,
/// backdated, or considered historical. It is designed to be  immutable after initialization, with all properties being
/// required during construction.</remarks>
public sealed record CreationSummary
{
    /// <summary>
    /// Gets the description associated with the object.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the normalized description of the entity.
    /// </summary>
    public required string NormalizedDescription { get; init; }

    /// <summary>
    /// Gets the initial status of the object, which must be specified during initialization.
    /// </summary>
    public required string InitialStatus { get; init; }

    /// <summary>
    /// Gets a value indicating whether the item is archived.
    /// </summary>
    public required bool IsArchived { get; init; }

    /// <summary>
    /// Gets the date and time when the object was created.
    /// </summary>
    public required DateTime CreationDate { get; init; }

    /// <summary>
    /// Gets the creation date as a string formatted in the local culture.
    /// </summary>
    public required string LocalCreationDateString { get; init; }

    /// <summary>
    /// Gets the initial age, in days, used to initialize the object.
    /// </summary>
    public required double InitialAgeDays { get; init; }

    /// <summary>
    /// Gets a value indicating whether the creation is backdated.
    /// </summary>
    public required bool IsBackdatedCreation { get; init; }

    /// <summary>
    /// Gets a value indicating whether the creation is considered historical.
    /// </summary>
    public required bool IsHistoricalCreation { get; init; }

    /// <summary>
    /// Gets a value indicating whether the current object is in a valid state.
    /// </summary>
    public required bool IsValid { get; init; }
}

/// <summary>
/// Represents the analysis of changes made to an object, including details about specific property modifications.
/// </summary>
/// <remarks>This type provides a summary of changes to an object, including whether specific aspects of the
/// object  (such as its description, archive status, or creation date) have been modified. It also includes a
/// collection  of detailed property changes for further inspection.</remarks>
public sealed record ChangeAnalysis
{
    /// <summary>
    /// Gets a value indicating whether there are any changes to the current object.
    /// </summary>
    public required bool HasChanges { get; init; }

    /// <summary>
    /// Gets a value indicating whether the description has been modified.
    /// </summary>
    public required bool DescriptionChanged { get; init; }

    /// <summary>
    /// Gets a value indicating whether the archive status has changed.
    /// </summary>
    public required bool ArchiveStatusChanged { get; init; }

    /// <summary>
    /// Gets a value indicating whether the creation date of the entity has been modified.
    /// </summary>
    public required bool CreationDateChanged { get; init; }

    /// <summary>
    /// Gets the collection of property changes associated with the current operation.
    /// </summary>
    public required List<PropertyChange> Changes { get; init; }
}

/// <summary>
/// Represents a change to a property, including the property's name, its previous value, and its new value.
/// </summary>
/// <remarks>This record is immutable and can be used to track changes to properties in scenarios such as data
/// binding, change tracking, or logging. The <see cref="Summary"/> property provides a human-readable description of
/// the change.</remarks>
/// <param name="PropertyName">The name of the property that changed. This value cannot be <see langword="null"/> or empty.</param>
/// <param name="OldValue">The previous value of the property. This value may be <see langword="null"/> if the property was not previously set.</param>
/// <param name="NewValue">The new value of the property. This value may be <see langword="null"/> if the property was cleared or unset.</param>
public sealed record PropertyChange(string PropertyName, object? OldValue, object? NewValue)
{
    /// <summary>
    /// Gets a summary of the property change, including the property name, old value, and new value.
    /// </summary>
    public string Summary => $"{PropertyName}: '{OldValue}' → '{NewValue}'";
}

/// <summary>
/// Provides a builder for creating and configuring instances of <see cref="MatterForCreationDto"/>.
/// </summary>
/// <remarks>This builder allows for the step-by-step configuration of a <see cref="MatterForCreationDto"/>
/// instance, including setting properties such as the description, archived status, and creation date. It supports
/// method chaining for a fluent API experience. The <see cref="Build"/> method validates the configured state and
/// returns the constructed <see cref="MatterForCreationDto"/> or a failure result with error details.</remarks>
public sealed class MatterForCreationDtoBuilder
{
    private string? _description;
    private bool _isArchived;
    private DateTime _creationDate = DateTime.UtcNow;

    /// <summary>
    /// Sets the description for the matter being created.
    /// </summary>
    /// <param name="description">The description of the matter. Leading and trailing whitespace will be trimmed. Can be null or empty.</param>
    /// <returns>The current <see cref="MatterForCreationDtoBuilder"/> instance, allowing for method chaining.</returns>
    public MatterForCreationDtoBuilder WithDescription(string description)
    {
        _description = description?.Trim();
        return this;
    }

    /// <summary>
    /// Sets the archived status for the matter being created.
    /// </summary>
    /// <param name="isArchived">A value indicating whether the matter should be marked as archived. The default value is <see langword="true"/>.</param>
    /// <returns>The current instance of <see cref="MatterForCreationDtoBuilder"/> to allow for method chaining.</returns>
    public MatterForCreationDtoBuilder AsArchived(bool isArchived = true)
    {
        _isArchived = isArchived;
        return this;
    }

    /// <summary>
    /// Sets the creation date for the matter being built.
    /// </summary>
    /// <param name="creationDate">The creation date to assign to the matter.</param>
    /// <returns>The current instance of <see cref="MatterForCreationDtoBuilder"/> to allow method chaining.</returns>
    public MatterForCreationDtoBuilder WithCreationDate(DateTime creationDate)
    {
        _creationDate = creationDate;
        return this;
    }

    /// <summary>
    /// Configures the builder to create a matter for historical migration with the specified creation date.
    /// </summary>
    /// <remarks>This method sets the creation date of the matter to the specified <paramref
    /// name="historicalDate"/>  and marks the matter as archived, as historical matters are typically
    /// archived.</remarks>
    /// <param name="historicalDate">The date to set as the creation date for the historical matter.</param>
    /// <returns>The current <see cref="MatterForCreationDtoBuilder"/> instance with the historical migration configuration
    /// applied.</returns>
    public MatterForCreationDtoBuilder ForHistoricalMigration(DateTime historicalDate)
    {
        _creationDate = historicalDate;
        _isArchived = true; // Historical matters are typically archived
        return this;
    }

    /// <summary>
    /// Builds a new <see cref="MatterForCreationDto"/> instance based on the current state of the builder.
    /// </summary>
    /// <remarks>This method validates the required fields and the overall model before returning the result. 
    /// If the description is missing or the model fails validation, the method returns a failure result  containing the
    /// relevant error details.</remarks>
    /// <returns>A <see cref="Result{T}"/> containing the constructed <see cref="MatterForCreationDto"/> if successful;
    /// otherwise, a failure result with error details.</returns>
    public Domain.Common.Result<MatterForCreationDto> Build()
    {
        if (string.IsNullOrWhiteSpace(_description))
            return Result.Failure<MatterForCreationDto>(DomainError.Create(
                "DESCRIPTION_REQUIRED", "Description is required"));

        var dto = new MatterForCreationDto
        {
            Description = _description,
            IsArchived = _isArchived,
            CreationDate = _creationDate
        };

        var validationResults = MatterForCreationDto.ValidateModel(dto);
        return validationResults.Count > 0
            ? Result.Failure<MatterForCreationDto>(DomainError.Create(
                "VALIDATION_FAILED", string.Join(", ", validationResults.Select(r => r.ErrorMessage))))
            : Result.Success(dto);
    }
}

/// <summary>
/// Provides extension methods for creating and initializing instances of <see cref="MatterForCreationDtoBuilder"/>.
/// </summary>
/// <remarks>This static class contains helper methods to simplify the creation of <see
/// cref="MatterForCreationDtoBuilder"/> instances, either with default values or pre-initialized with specific data
/// such as a description.</remarks>
public static class MatterForCreationDtoExtensions
{
    /// <summary>
    /// Creates and returns a new instance of <see cref="MatterForCreationDtoBuilder"/>.
    /// </summary>
    /// <returns>A new <see cref="MatterForCreationDtoBuilder"/> instance for constructing a matter creation DTO.</returns>
    public static MatterForCreationDtoBuilder CreateMatter() => new();

    /// <summary>
    /// Creates a new <see cref="MatterForCreationDtoBuilder"/> instance and initializes it with the specified
    /// description.
    /// </summary>
    /// <param name="description">The description to associate with the new matter. Cannot be null or empty.</param>
    /// <returns>A <see cref="MatterForCreationDtoBuilder"/> instance initialized with the provided description.</returns>
    public static MatterForCreationDtoBuilder CreateMatter(this string description) =>
        new MatterForCreationDtoBuilder().WithDescription(description);
}