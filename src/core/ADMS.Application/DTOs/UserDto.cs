using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using ADMS.Application.Common.Validation;
using ADMS.Domain.Common;

namespace ADMS.Application.DTOs;

/// <summary>
/// Data Transfer Object representing a user with audit trail relationships.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of a user within the ADMS legal document management system,
/// including activity associations for audit trail support. It provides validation and computed properties
/// optimized for application layer operations.
/// 
/// <para><strong>Clean Architecture Integration:</strong></para>
/// <list type="bullet">
/// <item>Maps to Domain User entity while remaining in Application layer</item>
/// <item>Uses Result pattern for error handling</item>
/// <item>Integrates with centralized validation helpers</item>
/// <item>Supports optional loading of activity relationships</item>
/// </list>
/// 
/// <para><strong>Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item>Professional naming conventions with validation</item>
/// <item>Complete audit trail relationships for compliance</item>
/// <item>User accountability for all system operations</item>
/// <item>Activity attribution for professional responsibility</item>
/// </list>
/// </remarks>
public sealed class UserDto : IValidatableObject, IEquatable<UserDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the user's unique identifier.
    /// </summary>
    [Required(ErrorMessage = "User ID is required.")]
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    /// <remarks>
    /// Supports professional naming conventions used in legal practice environments.
    /// Validated for length, format, and professional standards.
    /// </remarks>
    [Required(ErrorMessage = "User name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "User name must be between 2 and 50 characters.")]
    public required string Name { get; set; }

    #endregion Core Properties

    #region Activity Relationship Collections

    /// <summary>
    /// Gets or sets the collection of matter activity associations for this user.
    /// </summary>
    /// <remarks>
    /// Optional collection populated on-demand for audit trail scenarios.
    /// Contains activities like CREATED, ARCHIVED, DELETED, RESTORED, VIEWED.
    /// </remarks>
    public ICollection<MatterActivityUserDto>? MatterActivityUsers { get; set; }

    /// <summary>
    /// Gets or sets the collection of document activity associations for this user.
    /// </summary>
    /// <remarks>
    /// Optional collection for document operations audit trail.
    /// Contains activities like CREATED, SAVED, DELETED, RESTORED, CHECKED_IN, CHECKED_OUT.
    /// </remarks>
    public ICollection<DocumentActivityUserDto>? DocumentActivityUsers { get; set; }

    /// <summary>
    /// Gets or sets the collection of revision activity associations for this user.
    /// </summary>
    /// <remarks>
    /// Optional collection for document version control audit trail.
    /// Contains revision-specific activities for version tracking.
    /// </remarks>
    public ICollection<RevisionActivityUserDto>? RevisionActivityUsers { get; set; }

    /// <summary>
    /// Gets or sets the collection of document transfer operations initiated by this user.
    /// </summary>
    /// <remarks>
    /// Tracks document transfers FROM matters initiated by this user.
    /// Part of bidirectional audit trail for document custody tracking.
    /// </remarks>
    public ICollection<MatterDocumentActivityUserFromDto>? MatterDocumentActivityUsersFrom { get; set; }

    /// <summary>
    /// Gets or sets the collection of document transfers received by this user.
    /// </summary>
    /// <remarks>
    /// Tracks document transfers TO matters managed by this user.
    /// Complements FROM tracking for complete audit coverage.
    /// </remarks>
    public ICollection<MatterDocumentActivityUserToDto>? MatterDocumentActivityUsersTo { get; set; }

    #endregion Activity Relationship Collections

    #region Computed Properties

    /// <summary>
    /// Gets the total count of activities performed by this user across all activity types.
    /// </summary>
    public int TotalActivityCount =>
        (MatterActivityUsers?.Count ?? 0) +
        (DocumentActivityUsers?.Count ?? 0) +
        (RevisionActivityUsers?.Count ?? 0) +
        (MatterDocumentActivityUsersFrom?.Count ?? 0) +
        (MatterDocumentActivityUsersTo?.Count ?? 0);

    /// <summary>
    /// Gets a value indicating whether this user has any recorded activities.
    /// </summary>
    /// <remarks>
    /// Important for determining if user can be safely deleted or should be deactivated
    /// to preserve audit trail integrity.
    /// </remarks>
    public bool HasActivities => TotalActivityCount > 0;

    /// <summary>
    /// Gets the normalized version of the user's name for comparison operations.
    /// </summary>
    /// <remarks>
    /// Normalizes by trimming whitespace and collapsing multiple spaces.
    /// Returns null for invalid or empty names.
    /// </remarks>
    public string? NormalizedName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
                return null;

            var trimmed = Name.Trim();
            return string.IsNullOrEmpty(trimmed) ? null :
                   System.Text.RegularExpressions.Regex.Replace(trimmed, @"\s+", " ");
        }
    }

    /// <summary>
    /// Gets the display name optimized for user interface presentation.
    /// </summary>
    public string DisplayName => NormalizedName ?? Name;

    /// <summary>
    /// Gets a value indicating whether this user DTO has valid core data.
    /// </summary>
    public bool IsValid => Id != Guid.Empty && !string.IsNullOrWhiteSpace(Name) && Name.Length >= 2;

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the UserDto for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>A collection of validation results.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate core properties
        foreach (var result in ValidateUserId())
            yield return result;

        foreach (var result in ValidateUserName())
            yield return result;

        // Validate collections if loaded
        if (MatterActivityUsers != null)
        {
            foreach (var result in ValidationHelper.ValidateCollection(MatterActivityUsers, nameof(MatterActivityUsers)))
                yield return result;
        }

        if (DocumentActivityUsers != null)
        {
            foreach (var result in ValidationHelper.ValidateCollection(DocumentActivityUsers, nameof(DocumentActivityUsers)))
                yield return result;
        }

        if (RevisionActivityUsers != null)
        {
            foreach (var result in ValidationHelper.ValidateCollection(RevisionActivityUsers, nameof(RevisionActivityUsers)))
                yield return result;
        }

        if (MatterDocumentActivityUsersFrom != null)
        {
            foreach (var result in ValidationHelper.ValidateCollection(MatterDocumentActivityUsersFrom, nameof(MatterDocumentActivityUsersFrom)))
                yield return result;
        }

        if (MatterDocumentActivityUsersTo != null)
        {
            foreach (var result in ValidationHelper.ValidateCollection(MatterDocumentActivityUsersTo, nameof(MatterDocumentActivityUsersTo)))
                yield return result;
        }
    }

    /// <summary>
    /// Validates the user ID property.
    /// </summary>
    private IEnumerable<ValidationResult> ValidateUserId()
    {
        if (Id == Guid.Empty)
        {
            yield return new ValidationResult("User ID cannot be empty.", [nameof(Id)]);
        }
    }

    /// <summary>
    /// Validates the user name property.
    /// </summary>
    private IEnumerable<ValidationResult> ValidateUserName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("User name is required.", [nameof(Name)]);
            yield break;
        }

        if (Name.Length < 2)
        {
            yield return new ValidationResult("User name must be at least 2 characters long.", [nameof(Name)]);
        }

        if (Name.Length > 50)
        {
            yield return new ValidationResult("User name cannot exceed 50 characters.", [nameof(Name)]);
        }

        if (Name.Trim() != Name)
        {
            yield return new ValidationResult("User name should not have leading or trailing whitespace.", [nameof(Name)]);
        }

        // Basic professional naming validation
        if (!System.Text.RegularExpressions.Regex.IsMatch(Name, @"^[a-zA-Z0-9\s\.\-_]+$"))
        {
            yield return new ValidationResult("User name contains invalid characters. Use letters, numbers, spaces, periods, hyphens, or underscores.", [nameof(Name)]);
        }
    }

    #endregion Validation Implementation

    #region Static Factory Methods

    /// <summary>
    /// Creates a new UserDto with validation.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="name">The user name.</param>
    /// <returns>A Result containing either the UserDto or validation errors.</returns>
    public static Result<UserDto> Create(Guid id, string name)
    {
        if (id == Guid.Empty)
            return Result.Failure<UserDto>(DomainError.Create("INVALID_USER_ID", "User ID cannot be empty"));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<UserDto>(DomainError.Create("INVALID_USER_NAME", "User name is required"));

        var dto = new UserDto
        {
            Id = id,
            Name = name.Trim()
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return Result.Success(dto);
        var errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        return Result.Failure<UserDto>(DomainError.Create("VALIDATION_FAILED", errors));

    }

    /// <summary>
    /// Creates a UserDto from a domain User entity.
    /// </summary>
    /// <param name="user">The domain user entity.</param>
    /// <param name="includeActivities">Whether to include activity collections.</param>
    /// <returns>A UserDto instance.</returns>
    /// <remarks>
    /// Maps from Domain layer User entity to Application layer DTO.
    /// Activity collections are excluded by default for performance.
    /// </remarks>
    public static UserDto FromDomainEntity(Domain.Entities.User user, bool includeActivities = false)
    {
        ArgumentNullException.ThrowIfNull(user);

        var dto = new UserDto
        {
            Id = user.Id, // Assumes User.Id is Guid, or user.Id.Value if using UserId value object
            Name = user.Name
        };

        // Activity collections would be mapped here if needed
        // For now, leaving as null for performance unless specifically requested
        if (includeActivities)
        {
            // Note: In a real implementation, you would map the activity collections
            // This would typically be done by a mapping framework like Mapster or AutoMapper
            // dto.MatterActivityUsers = /* mapping logic */;
            // dto.DocumentActivityUsers = /* mapping logic */;
            // etc.
        }

        return dto;
    }

    /// <summary>
    /// Validates a UserDto instance and returns validation results.
    /// </summary>
    /// <param name="dto">The UserDto to validate.</param>
    /// <returns>A list of validation results.</returns>
    public static IList<ValidationResult> ValidateModel([AllowNull] UserDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("UserDto instance is required."));
            return results;
        }

        var context = new ValidationContext(dto);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    #endregion Static Factory Methods

    #region Business Methods

    /// <summary>
    /// Updates the last modified information for audit purposes.
    /// </summary>
    /// <param name="modifiedBy">The user who modified this record.</param>
    /// <remarks>
    /// This method would typically be used in conjunction with audit trail functionality.
    /// Currently simplified since audit properties aren't defined in the core UserDto.
    /// </remarks>
    public static void UpdateLastModified(string modifiedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modifiedBy);
        // In a full implementation, this would update LastModifiedBy and LastModifiedDate properties
        // For now, this serves as a placeholder for audit functionality
    }

    #endregion Business Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified UserDto is equal to the current UserDto.
    /// </summary>
    /// <param name="other">The UserDto to compare.</param>
    /// <returns>True if the UserDtos are equal; otherwise, false.</returns>
    public bool Equals(UserDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // Compare by ID if both have valid IDs
        if (Id != Guid.Empty && other.Id != Guid.Empty)
        {
            return Id == other.Id;
        }

        // Otherwise compare by normalized content
        return string.Equals(NormalizedName, other.NormalizedName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current UserDto.
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as UserDto);

    /// <summary>
    /// Returns a hash code for this UserDto.
    /// </summary>
    public override int GetHashCode()
    {
        return Id != Guid.Empty ? Id.GetHashCode() :
               StringComparer.OrdinalIgnoreCase.GetHashCode(NormalizedName ?? string.Empty);
    }

    /// <summary>
    /// Determines whether two UserDto instances are equal.
    /// </summary>
    public static bool operator ==(UserDto? left, UserDto? right) =>
        EqualityComparer<UserDto>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two UserDto instances are not equal.
    /// </summary>
    public static bool operator !=(UserDto? left, UserDto? right) => !(left == right);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the UserDto.
    /// </summary>
    public override string ToString() => $"{DisplayName} ({Id})";

    #endregion String Representation
}