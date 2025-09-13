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
        // Professional user activity validation
        foreach (var result in ValidateUserActivityPatterns())
            yield return result;

        // User role and permissions validation
        foreach (var result in ValidateUserRoleConsistency())
            yield return result;

        // Audit trail completeness validation
        foreach (var result in ValidateAuditTrailCompleteness())
            yield return result;
    }

    /// <summary>
    /// Validates relationships and constraints between multiple properties.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    protected override IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // Base cross-property validation
        foreach (var result in base.ValidateCrossPropertyRules())
            yield return result;

        // User activity consistency validation
        foreach (var result in ValidateActivityConsistency())
            yield return result;

        // Professional naming validation
        foreach (var result in ValidateProfessionalNaming())
            yield return result;
    }

    /// <summary>
    /// Validates activity consistency across different activity types.
    /// </summary>
    /// <returns>A collection of validation results for activity consistency.</returns>
    private IEnumerable<ValidationResult> ValidateActivityConsistency()
    {
        if (!HasActivities) yield break;

        // Validate temporal consistency across activity types
        var activityDates = GetAllActivityDates();
        if (activityDates.Any())
        {
            var earliestActivity = activityDates.Min();
            var latestActivity = activityDates.Max();
            var activitySpan = (latestActivity - earliestActivity).TotalDays;

            // Check for suspicious activity compression
            if (TotalActivityCount > 100 && activitySpan < 1)
            {
                yield return CreateValidationResult(
                    "High volume of activities compressed into short timeframe detected. " +
                    "Verify activity authenticity and professional authorization.",
                    nameof(TotalActivityCount));
            }
        }

        // Validate user attribution consistency
        foreach (var result in ValidateUserAttributionConsistency())
            yield return result;
    }

    /// <summary>
    /// Validates professional naming conventions and standards.
    /// </summary>
    /// <returns>A collection of validation results for professional naming validation.</returns>
    private IEnumerable<ValidationResult> ValidateProfessionalNaming()
    {
        var normalizedName = UserValidationHelper.NormalizeUsername(Name);
        
        // Check for professional naming standards
        if (!string.IsNullOrEmpty(normalizedName) && normalizedName.Length < 3)
        {
            yield return CreateValidationResult(
                "User name is too short for professional identification. " +
                "Legal practice users should have clear, professional identifiers.",
                nameof(Name));
        }

        // Validate professional character usage
        if (Name.Count(c => char.IsDigit(c)) > Name.Length / 2)
        {
            yield return CreateValidationResult(
                "User name contains excessive numeric characters. " +
                "Professional users should have primarily alphabetic identifiers.",
                nameof(Name));
        }
    }

    /// <summary>
    /// Validates collections and nested objects.
    /// </summary>
    /// <returns>A collection of validation results for collection validation.</returns>
    protected override IEnumerable<ValidationResult> ValidateCollections()
    {
        // Base collection validation
        foreach (var result in base.ValidateCollections())
            yield return result;

        // Enhanced activity collection validation
        foreach (var result in ValidateActivityCollectionsEnhanced())
            yield return result;
        
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
        if (MatterDocumentActivityUsersTo == null) yield break;
        foreach (var result in ValidateCollection(MatterDocumentActivityUsersTo, nameof(MatterDocumentActivityUsersTo)))
            yield return result;
    }

    /// <summary>
    /// Validates activity collections with enhanced compliance and professional standards.
    /// </summary>
    /// <returns>A collection of validation results for enhanced collection validation.</returns>
    private IEnumerable<ValidationResult> ValidateActivityCollectionsEnhanced()
    {
        // Validate transfer activity collections for bidirectional compliance
        if (MatterDocumentActivityUsersFrom != null && MatterDocumentActivityUsersTo != null)
        {
            foreach (var result in ValidateBidirectionalTransferCompliance())
                yield return result;
        }

        // Validate activity temporal consistency
        foreach (var result in ValidateActivityTemporalConsistency())
            yield return result;

        // Validate professional activity attribution
        foreach (var result in ValidateProfessionalActivityAttribution())
            yield return result;
    }

    /// <summary>
    /// Validates bidirectional transfer compliance for document transfer activities.
    /// </summary>
    /// <returns>A collection of validation results for transfer compliance validation.</returns>
    private IEnumerable<ValidationResult> ValidateBidirectionalTransferCompliance()
    {
        // Example validation: Ensure that for every FROM transfer, there is a corresponding TO transfer
        var unmatchedFromTransfers = MatterDocumentActivityUsersFrom
            ?.Where(fromActivity => !MatterDocumentActivityUsersTo
                ?.Any(toActivity => toActivity.DocumentId == fromActivity.DocumentId) ?? true)
            .ToList();

        if (unmatchedFromTransfers is { Count: > 0 })
        {
            yield return CreateValidationResult(
                "Some document transfers initiated by the user do not have corresponding received transfers. " +
                "Ensure bidirectional audit trail completeness for document custody tracking.",
                nameof(MatterDocumentActivityUsersFrom));
        }

        // Example validation: Check for suspiciously high volume of transfers
        if (MatterDocumentActivityUsersFrom?.Count > 1000)
        {
            yield return CreateValidationResult(
                "Detected unusually high volume of document transfers initiated by the user. " +
                "Review for compliance with professional transfer authorization standards.",
                nameof(MatterDocumentActivityUsersFrom));
        }
    }

    /// <summary>
    /// Validates the temporal consistency of activities across different types.
    /// </summary>
    /// <returns>A collection of validation results for temporal consistency validation.</returns>
    private IEnumerable<ValidationResult> ValidateActivityTemporalConsistency()
    {
        // Ensure all activity types have consistent timing for the same user
        var allActivities = (MatterActivityUsers ?? Enumerable.Empty<MatterActivityUserDto>()
            ).Concat(DocumentActivityUsers ?? Enumerable.Empty<DocumentActivityUserDto>())
            .Concat(RevisionActivityUsers ?? Enumerable.Empty<RevisionActivityUserDto>())
            .Concat(MatterDocumentActivityUsersFrom ?? Enumerable.Empty<MatterDocumentActivityUserFromDto>())
            .Concat(MatterDocumentActivityUsersTo ?? Enumerable.Empty<MatterDocumentActivityUserToDto>());

        var groupedByUser = allActivities.GroupBy(a => a.UserId);
        foreach (var group in groupedByUser)
        {
            var activityTimes = group.Select(a => a.Timestamp).ToList();
            var minTime = activityTimes.Min();
            var maxTime = activityTimes.Max();

            // Check for activities that are too close together in time
            var suspiciouslyCloseActivities = activityTimes
                .Where((time, index) => index > 0 && (time - activityTimes[index - 1]).TotalSeconds < 1)
                .ToList();

            if (suspiciouslyCloseActivities.Any())
            {
                yield return CreateValidationResult(
                    "Detected activities with suspiciously close timing. " +
                    "Verify activity authenticity and effective user engagement.",
                    nameof(TotalActivityCount));
            }

            // Check for activities spanning an implausibly long time without change
            if ((maxTime - minTime).TotalDays > 30)
            {
                yield return CreateValidationResult(
                    "Activity timeline spans an implausibly long period without significant gaps. " +
                    "Review for potential data integrity issues or unusual user behavior.",
                    nameof(TotalActivityCount));
            }
        }
    }

    /// <summary>
    /// Validates professional attribution of activities ensuring accountability and traceability.
    /// </summary>
    /// <returns>A collection of validation results for professional attribution validation.</returns>
    private IEnumerable<ValidationResult> ValidateProfessionalActivityAttribution()
    {
        // Example validation: Ensure all activities have valid user attribution
        var allActivities = (MatterActivityUsers ?? Enumerable.Empty<MatterActivityUserDto>()
            ).Concat(DocumentActivityUsers ?? Enumerable.Empty<DocumentActivityUserDto>())
            .Concat(RevisionActivityUsers ?? Enumerable.Empty<RevisionActivityUserDto>())
            .Concat(MatterDocumentActivityUsersFrom ?? Enumerable.Empty<MatterDocumentActivityUserFromDto>())
            .Concat(MatterDocumentActivityUsersTo ?? Enumerable.Empty<MatterDocumentActivityUserToDto>());

        foreach (var activity in allActivities)
        {
            if (activity.UserId == Guid.Empty)
            {
                yield return CreateValidationResult(
                    "Detected activity with invalid or missing user attribution. " +
                    "Ensure all activities are properly attributed to a valid user.",
                    nameof(TotalActivityCount));
            }
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
    public string NormalizeUsername()
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

    /// <summary>
    /// Validates user activity patterns for professional standards and anomaly detection.
    /// </summary>
    /// <returns>A collection of validation results for activity pattern validation.</returns>
    private IEnumerable<ValidationResult> ValidateUserActivityPatterns()
    {
        if (TotalActivityCount == 0) yield break;

        // Validate activity distribution across different types
        var activityDistribution = GetActivityDistribution();
        
        // Check for suspicious activity concentrations
        if (activityDistribution.TryGetValue("DocumentTransfers", out var transferCount) && 
            transferCount > 0.8 * TotalActivityCount)
        {
            yield return CreateValidationResult(
                "User has unusually high concentration of document transfer activities. " +
                "Verify professional authorization for extensive transfer operations.",
                nameof(MatterDocumentActivityUsersFrom), nameof(MatterDocumentActivityUsersTo));
        }

        // Validate activity velocity for compliance monitoring
        if (HasActivities && GetRecentActivityCount(days: 7) > UserValidationHelper.MaxDailyActivityCount * 7)
        {
            yield return CreateValidationResult(
                "User has exceeded recommended weekly activity limits. " +
                "Review activity patterns for compliance and professional oversight.",
                nameof(TotalActivityCount));
        }

        // Professional responsibility validation for high-activity users
        if (TotalActivityCount > 1000)
        {
            yield return CreateValidationResult(
                "High-activity user detected. Ensure proper professional oversight " +
                "and compliance monitoring for users with extensive system activity.",
                nameof(TotalActivityCount));
        }
    }

    /// <summary>
    /// Validates user role consistency and professional responsibility requirements.
    /// </summary>
    /// <returns>A collection of validation results for role consistency validation.</returns>
    private IEnumerable<ValidationResult> ValidateUserRoleConsistency()
    {
        // Validate user name for professional standards
        if (UserValidationHelper.IsReservedUsername(Name))
        {
            yield return CreateValidationResult(
                "User name conflicts with system reserved names. Professional users " +
                "should have unique, professional identifiers.",
                nameof(Name));
        }

        // Validate administrative pattern detection
        if (Name.Contains("ADMIN", StringComparison.OrdinalIgnoreCase) || Name.Contains("SYSTEM", StringComparison.OrdinalIgnoreCase))
        {
            yield return CreateValidationResult(
                "Administrative user pattern detected. Ensure proper authorization " +
                "and security protocols for administrative access.",
                nameof(Name));
        }

        // Validate user context sufficiency for operations
        if (!IsSufficientForOperations)
        {
            yield return CreateValidationResult(
                "User does not meet minimum requirements for professional operations. " +
                "Complete user profile information is required for legal compliance.",
                nameof(Id), nameof(Name));
        }
    }

    /// <summary>
    /// Validates audit trail completeness for legal compliance requirements.
    /// </summary>
    /// <returns>A collection of validation results for audit trail validation.</returns>
    private IEnumerable<ValidationResult> ValidateAuditTrailCompleteness()
    {
        // Validate bidirectional transfer audit completeness
        var fromTransfers = MatterDocumentActivityUsersFrom?.Count ?? 0;
        var toTransfers = MatterDocumentActivityUsersTo?.Count ?? 0;
        
        // Check for unbalanced transfer patterns
        if (fromTransfers > 0 && toTransfers == 0)
        {
            yield return CreateValidationResult(
                "User has document transfer initiation activities but no destination activities. " +
                "Bidirectional audit trail may be incomplete for compliance tracking.",
                nameof(MatterDocumentActivityUsersFrom), nameof(MatterDocumentActivityUsersTo));
        }

        // Validate activity attribution completeness
        if (!HasActivities) yield break;
        var incompleteActivities = GetIncompleteActivityCount();
        if (incompleteActivities > 0)
        {
            yield return CreateValidationResult(
                $"User has {incompleteActivities} activities with incomplete attribution. " +
                "Complete user attribution is required for professional responsibility compliance.",
                nameof(MatterActivityUsers), nameof(DocumentActivityUsers));
        }
    }
    
    /// <summary>
    /// Gets activity distribution across different activity types.
    /// </summary>
    /// <returns>Dictionary containing activity type distribution.</returns>
    private Dictionary<string, int> GetActivityDistribution()
    {
        return new Dictionary<string, int>
        {
            ["MatterActivities"] = MatterActivityUsers?.Count ?? 0,
            ["DocumentActivities"] = DocumentActivityUsers?.Count ?? 0,
            ["RevisionActivities"] = RevisionActivityUsers?.Count ?? 0,
            ["DocumentTransfers"] = (MatterDocumentActivityUsersFrom?.Count ?? 0) + 
                                   (MatterDocumentActivityUsersTo?.Count ?? 0)
        };
    }

    /// <summary>
    /// Gets recent activity count within specified days.
    /// </summary>
    /// <param name="days">Number of days to look back.</param>
    /// <returns>Count of activities within the timeframe.</returns>
    private int GetRecentActivityCount(int days = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var recentCount = 0;

        if (MatterActivityUsers != null)
            recentCount += MatterActivityUsers.Count(a => a.CreatedAt >= cutoffDate);
        if (DocumentActivityUsers != null)
            recentCount += DocumentActivityUsers.Count(a => a.CreatedAt >= cutoffDate);
        if (RevisionActivityUsers != null)
            recentCount += RevisionActivityUsers.Count(a => a.CreatedAt >= cutoffDate);
        if (MatterDocumentActivityUsersFrom != null)
            recentCount += MatterDocumentActivityUsersFrom.Count(a => a.CreatedAt >= cutoffDate);
        if (MatterDocumentActivityUsersTo != null)
            recentCount += MatterDocumentActivityUsersTo.Count(a => a.CreatedAt >= cutoffDate);

        return recentCount;
    }

    /// <summary>
    /// Gets all activity dates for temporal analysis.
    /// </summary>
    /// <returns>List of all activity timestamps.</returns>
    private List<DateTime> GetAllActivityDates()
    {
        var dates = new List<DateTime>();

        if (MatterActivityUsers != null)
            dates.AddRange(MatterActivityUsers.Select(a => a.CreatedAt));
        if (DocumentActivityUsers != null)
            dates.AddRange(DocumentActivityUsers.Select(a => a.CreatedAt));
        if (RevisionActivityUsers != null)
            dates.AddRange(RevisionActivityUsers.Select(a => a.CreatedAt));
        if (MatterDocumentActivityUsersFrom != null)
            dates.AddRange(MatterDocumentActivityUsersFrom.Select(a => a.CreatedAt));
        if (MatterDocumentActivityUsersTo != null)
            dates.AddRange(MatterDocumentActivityUsersTo.Select(a => a.CreatedAt));

        return dates;
    }

    /// <summary>
    /// Validates user attribution consistency across all activities.
    /// </summary>
    /// <returns>A collection of validation results for user attribution consistency.</returns>
    private IEnumerable<ValidationResult> ValidateUserAttributionConsistency()
    {
        // Example: Ensure all activities reference the current user's Id
        var allActivities = (MatterActivityUsers ?? Enumerable.Empty<MatterActivityUserDto>())
            .Concat(DocumentActivityUsers ?? Enumerable.Empty<DocumentActivityUserDto>())
            .Concat(RevisionActivityUsers ?? Enumerable.Empty<RevisionActivityUserDto>())
            .Concat(MatterDocumentActivityUsersFrom ?? Enumerable.Empty<MatterDocumentActivityUserFromDto>())
            .Concat(MatterDocumentActivityUsersTo ?? Enumerable.Empty<MatterDocumentActivityUserToDto>());

        foreach (var activity in allActivities)
        {
            if (activity.UserId != Id)
            {
                yield return CreateValidationResult(
                    "Activity attribution inconsistency detected. All activities should reference the current user's Id.",
                    nameof(Id));
            }
        }
    }

    /// <summary>
    /// Gets the count of activities with incomplete attribution.
    /// </summary>
    /// <returns>Count of activities missing required attribution.</returns>
    private int GetIncompleteActivityCount()
    {
        var count = 0;

        if (MatterActivityUsers != null)
            count += MatterActivityUsers.Count(a => a.UserId == Guid.Empty);

        if (DocumentActivityUsers != null)
            count += DocumentActivityUsers.Count(a => a.UserId == Guid.Empty);

        if (RevisionActivityUsers != null)
            count += RevisionActivityUsers.Count(a => a.UserId == Guid.Empty);

        if (MatterDocumentActivityUsersFrom != null)
            count += MatterDocumentActivityUsersFrom.Count(a => a.UserId == Guid.Empty);

        if (MatterDocumentActivityUsersTo != null)
            count += MatterDocumentActivityUsersTo.Count(a => a.UserId == Guid.Empty);

        return count;
    }
}