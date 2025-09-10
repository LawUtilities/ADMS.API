using ADMS.Application.Common.Validation;
using ADMS.Domain.Common;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Data Transfer Object representing a user with streamlined validation and comprehensive audit trail relationships.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of a user within the ADMS legal document management system,
/// including activity associations for audit trail support. It provides validation and computed properties
/// optimized for application layer operations using the standardized BaseValidationDto approach.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Streamlined Validation:</strong> Uses BaseValidationDto for consistent, performant validation</item>
/// <item><strong>Domain Alignment:</strong> Maps directly to ADMS.Domain.Entities.User</item>
/// <item><strong>Professional Standards:</strong> Enforces legal practice naming conventions and business rules</item>
/// <item><strong>Clean Architecture:</strong> Maintains separation between domain and application layers</item>
/// <item><strong>Validation Integration:</strong> Uses UserValidationHelper for consistency</item>
/// </list>
/// 
/// <para><strong>Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Professional naming conventions with validation</strong></item>
/// <item><strong>Complete audit trail relationships for compliance</strong></item>
/// <item><strong>User accountability for all system operations</strong></item>
/// <item><strong>Activity attribution for professional responsibility</strong></item>
/// </list>
/// </remarks>
public sealed class UserDto : BaseValidationDto, IEquatable<UserDto>
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
    [StringLength(UserValidationHelper.MaxUserNameLength, MinimumLength = UserValidationHelper.MinUserNameLength, 
        ErrorMessage = "User name must be between 2 and 50 characters.")]
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
    /// Uses UserValidationHelper for consistent normalization across the system.
    /// Returns null for invalid or empty names.
    /// </remarks>
    public string NormalizedName => UserValidationHelper.NormalizeUsername(Name);

    /// <summary>
    /// Gets the display name optimized for user interface presentation.
    /// </summary>
    public string DisplayName => NormalizedName;

    /// <summary>
    /// Gets a value indicating whether this user DTO has valid core data.
    /// </summary>
    public bool IsValid => 
        UserValidationHelper.IsValidUserId(Id) && 
        UserValidationHelper.IsUsernameAllowed(Name);

    /// <summary>
    /// Gets a value indicating whether the user has sufficient data for professional operations.
    /// </summary>
    public bool IsSufficientForOperations => 
        UserValidationHelper.IsSufficientUserContext(Id, Name, DateTime.UtcNow);

    #endregion Computed Properties

    #region Validation Implementation (BaseValidationDto)

    /// <summary>
    /// Validates core properties such as ID and name.
    /// </summary>
    /// <returns>A collection of validation results for core property validation.</returns>
    protected override IEnumerable<ValidationResult> ValidateCoreProperties()
    {
        // Validate User ID using UserValidationHelper
        foreach (var result in UserValidationHelper.ValidateUserId(Id, nameof(Id)))
            yield return result;

        // Validate Name using UserValidationHelper
        foreach (var result in UserValidationHelper.ValidateUsername(Name, nameof(Name)))
            yield return result;
    }

    /// <summary>
    /// Validates business rules and domain-specific logic.
    /// </summary>
    /// <returns>A collection of validation results for business rule validation.</returns>
    protected override IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Validate that user has sufficient context for operations
        if (!IsSufficientForOperations)
        {
            yield return CreateValidationResult(
                "User does not have sufficient data for professional operations.",
                nameof(Id), nameof(Name));
        }

        // Validate normalized name consistency
        var normalizedName = NormalizeUsername();
        if (!string.IsNullOrEmpty(normalizedName) && normalizedName != Name?.Trim())
        {
            yield return CreateValidationResult(
                "User name contains formatting that will be normalized. Consider using the normalized format.",
                nameof(Name));
        }
    }

    /// <summary>
    /// Validates relationships and constraints between multiple properties.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    protected override IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // Validate name and ID consistency for professional standards
        if (Id == Guid.Empty || string.IsNullOrWhiteSpace(Name)) yield break;
        // Check for reserved names that conflict with system operations
        if (UserValidationHelper.IsReservedUsername(Name))
        {
            yield return CreateValidationResult(
                "User name conflicts with system reserved names.",
                nameof(Name));
        }
    }

    /// <summary>
    /// Validates collections and nested objects.
    /// </summary>
    /// <returns>A collection of validation results for collection validation.</returns>
    protected override IEnumerable<ValidationResult> ValidateCollections()
    {
        // Validate MatterActivityUsers collection if loaded
        if (MatterActivityUsers != null)
        {
            foreach (var result in ValidateCollection(MatterActivityUsers, nameof(MatterActivityUsers)))
                yield return result;
        }

        // Validate DocumentActivityUsers collection if loaded
        if (DocumentActivityUsers != null)
        {
            foreach (var result in ValidateCollection(DocumentActivityUsers, nameof(DocumentActivityUsers)))
                yield return result;
        }

        // Validate RevisionActivityUsers collection if loaded
        if (RevisionActivityUsers != null)
        {
            foreach (var result in ValidateCollection(RevisionActivityUsers, nameof(RevisionActivityUsers)))
                yield return result;
        }

        // Validate MatterDocumentActivityUsersFrom collection if loaded
        if (MatterDocumentActivityUsersFrom != null)
        {
            foreach (var result in ValidateCollection(MatterDocumentActivityUsersFrom, nameof(MatterDocumentActivityUsersFrom)))
                yield return result;
        }

        // Validate MatterDocumentActivityUsersTo collection if loaded
        if (MatterDocumentActivityUsersTo != null)
        {
            foreach (var result in ValidateCollection(MatterDocumentActivityUsersTo, nameof(MatterDocumentActivityUsersTo)))
                yield return result;
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
        return BaseValidationDto.ValidateModel(dto);
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

    /// <summary>
    /// Normalizes the user's name using the validation helper.
    /// </summary>
    /// <returns>Normalized username or null if invalid.</returns>
    public string? NormalizeUsername()
    {
        return UserValidationHelper.NormalizeUsername(Name);
    }

    /// <summary>
    /// Determines if this user can be safely deleted without compromising audit trails.
    /// </summary>
    /// <returns>True if the user can be deleted; otherwise, false.</returns>
    public bool CanBeDeleted()
    {
        return !HasActivities;
    }

    /// <summary>
    /// Gets user activity statistics for monitoring and reporting.
    /// </summary>
    /// <returns>Dictionary containing activity statistics.</returns>
    public IReadOnlyDictionary<string, object> GetActivityStatistics()
    {
        return new Dictionary<string, object>
        {
            ["TotalActivities"] = TotalActivityCount,
            ["MatterActivities"] = MatterActivityUsers?.Count ?? 0,
            ["DocumentActivities"] = DocumentActivityUsers?.Count ?? 0,
            ["RevisionActivities"] = RevisionActivityUsers?.Count ?? 0,
            ["TransferActivitiesFrom"] = MatterDocumentActivityUsersFrom?.Count ?? 0,
            ["TransferActivitiesTo"] = MatterDocumentActivityUsersTo?.Count ?? 0,
            ["HasAuditTrail"] = HasActivities,
            ["CanBeDeleted"] = CanBeDeleted()
        };
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
        if (UserValidationHelper.IsValidUserId(Id) && UserValidationHelper.IsValidUserId(other.Id))
        {
            return Id == other.Id;
        }

        // Otherwise compare by normalized content
        return UserValidationHelper.AreUsernamesEquivalent(Name, other.Name);
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
        if (UserValidationHelper.IsValidUserId(Id))
            return Id.GetHashCode();

        var normalizedName = UserValidationHelper.NormalizeUsername(Name);
        return StringComparer.OrdinalIgnoreCase.GetHashCode(normalizedName);
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