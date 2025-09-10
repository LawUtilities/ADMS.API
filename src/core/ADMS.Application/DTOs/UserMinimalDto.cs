using ADMS.Application.Common.Validation;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Minimal User DTO for UI display and system operations.
/// </summary>
/// <remarks>
/// Lightweight user representation containing only essential properties (ID and Name).
/// Optimized for dropdowns, audit displays, and nested references within other DTOs.
/// </remarks>
public sealed record UserMinimalDto : IValidatableObject
{
    #region Core Properties

    /// <summary>
    /// Gets the user's unique identifier.
    /// </summary>
    [Required(ErrorMessage = "User ID is required.")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the user's display name.
    /// </summary>
    /// <remarks>
    /// Supports professional naming conventions with validation for length, format, and reserved names.
    /// </remarks>
    [Required(ErrorMessage = "User name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "User name must be between 2 and 50 characters.")]
    public required string Name { get; init; }

    #endregion Core Properties

    #region Computed Properties

    /// <summary>
    /// Gets the normalized version of the user's name for comparison and search operations.
    /// </summary>
    public string NormalizedName => UserValidationHelper.NormalizeUsername(Name);

    /// <summary>
    /// Gets a value indicating whether this user DTO has valid core data.
    /// </summary>
    public bool IsValid => UserValidationHelper.IsValidUserId(Id) && UserValidationHelper.IsUsernameAllowed(Name);

    /// <summary>
    /// Gets the display text for UI controls.
    /// </summary>
    public string DisplayText => NormalizedName;

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the UserMinimalDto for data integrity and business rules compliance.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate using centralized validation helper
        foreach (var result in UserValidationHelper.ValidateUserId(Id, nameof(Id)))
            yield return result;

        foreach (var result in UserValidationHelper.ValidateUsername(Name, nameof(Name)))
            yield return result;
    }

    #endregion Validation Implementation

    #region Static Factory Methods

    /// <summary>
    /// Validates a UserMinimalDto instance and returns validation results.
    /// </summary>
    /// <param name="dto">The UserMinimalDto to validate.</param>
    /// <returns>A list of validation results.</returns>
    public static IList<ValidationResult> ValidateModel([AllowNull] UserMinimalDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("UserMinimalDto instance is required."));
            return results;
        }

        var context = new ValidationContext(dto);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a UserMinimalDto from a Domain User entity.
    /// </summary>
    /// <param name="user">The User entity to convert.</param>
    /// <returns>A valid UserMinimalDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    public static UserMinimalDto FromEntity([NotNull] Domain.Entities.User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var dto = new UserMinimalDto
        {
            Id = user.Id,
            Name = user.Name
        };

        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;

        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid UserMinimalDto from entity: {errorMessages}");
    }

    #endregion Static Factory Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified UserMinimalDto is equal to the current UserMinimalDto.
    /// </summary>
    /// <param name="other">The UserMinimalDto to compare.</param>
    /// <returns>True if equal based on Id comparison; otherwise, false.</returns>
    public bool Equals(UserMinimalDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Returns a hash code based on the user's Id.
    /// </summary>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the UserMinimalDto.
    /// </summary>
    public override string ToString() => $"{DisplayText} ({Id})";

    #endregion String Representation
}