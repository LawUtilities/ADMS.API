using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Minimal User Data Transfer Object providing essential user information for UI display and system operations.
/// </summary>
/// <remarks>
/// This immutable record represents a lightweight user DTO optimized for UI scenarios where only essential 
/// user information is needed. It provides the minimal set of properties required to identify and display 
/// a user throughout the ADMS legal document management system while maintaining data integrity through 
/// comprehensive validation.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Immutable Design:</strong> Record type with init-only properties for thread-safe operations</item>
/// <item><strong>Essential Properties:</strong> Contains only ID and Name for minimal data transfer</item>
/// <item><strong>UI Optimized:</strong> Designed for user interface display and selection scenarios</item>
/// <item><strong>Validation Integrated:</strong> Comprehensive validation using ADMS.API.Common.UserValidationHelper</item>
/// <item><strong>Audit Trail Compatible:</strong> Supports user attribution in audit trail and activity records</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>User Selection Lists:</strong> Dropdown menus, autocomplete controls, user pickers</item>
/// <item><strong>Audit Trail Display:</strong> User attribution in activity logs and audit records</item>
/// <item><strong>Reference Data:</strong> Nested within other DTOs for user association display</item>
/// <item><strong>API Responses:</strong> Lightweight user data in document, matter, and activity DTOs</item>
/// <item><strong>Search Results:</strong> User search and filtering operations</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Integration:</strong></para>
/// This DTO is referenced by multiple ADMS.API.Entities activity junction entities:
/// <list type="bullet">
/// <item><strong>DocumentActivityUser:</strong> User attribution in document operations</item>
/// <item><strong>MatterActivityUser:</strong> User attribution in matter lifecycle activities</item>
/// <item><strong>RevisionActivityUser:</strong> User attribution in document revision operations</item>
/// <item><strong>MatterDocumentActivityUser:</strong> User attribution in document transfer operations</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Professional Names:</strong> Supports full professional names with proper formatting</item>
/// <item><strong>User Identification:</strong> Clear user identification for legal document attribution</item>
/// <item><strong>Audit Compliance:</strong> Maintains user accountability for legal compliance requirements</item>
/// <item><strong>System Integration:</strong> Compatible with ADMS entity relationships and audit trails</item>
/// </list>
/// 
/// <para><strong>Data Integrity:</strong></para>
/// <list type="bullet">
/// <item><strong>Required Properties:</strong> All properties are required with non-null validation</item>
/// <item><strong>GUID Validation:</strong> Ensures valid user identifiers for system operations</item>
/// <item><strong>Name Validation:</strong> Professional name format validation and reserved name checking</item>
/// <item><strong>Length Constraints:</strong> Database-synchronized length limits for consistency</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a minimal user DTO
/// var userMinimal = new UserMinimalDto
/// {
///     Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
///     Name = "Robert Brown"
/// };
/// 
/// // Using in UI scenarios
/// var userOptions = users.Select(u => new UserMinimalDto 
/// { 
///     Id = u.Id, 
///     Name = u.Name 
/// }).ToList();
/// 
/// // Validation example
/// var validationResults = UserMinimalDto.ValidateModel(userMinimal);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Validation Error: {result.ErrorMessage}");
///     }
/// }
/// </code>
/// </example>
public record UserMinimalDto : IValidatableObject, IEquatable<UserMinimalDto>
{
    #region Core Properties

    /// <summary>
    /// Gets the user's unique identifier.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the user within the ADMS system.
    /// The ID is used for establishing relationships, audit trail associations, and system operations
    /// requiring precise user identification.
    /// 
    /// <para><strong>Usage in System:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Foreign Key Relations:</strong> Referenced in audit trail and activity junction entities</item>
    /// <item><strong>User Identification:</strong> Unique identification across all system operations</item>
    /// <item><strong>Database Operations:</strong> Primary key for user-related database queries</item>
    /// <item><strong>API Operations:</strong> User identification in REST API operations</item>
    /// </list>
    /// 
    /// <para><strong>Validation Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Must be a valid non-empty GUID</item>
    /// <item>Cannot be Guid.Empty</item>
    /// <item>Should correspond to an existing ADMS.API.Entities.User entity</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Valid user ID examples from ADMS.API.Entities seed data
    /// var robertBrownId = Guid.Parse("50000000-0000-0000-0000-000000000001");
    /// var jenniferSmithId = Guid.Parse("50000000-0000-0000-0000-000000000002");
    /// 
    /// // Creating UserMinimalDto with valid ID
    /// var user = new UserMinimalDto 
    /// { 
    ///     Id = robertBrownId, 
    ///     Name = "Robert Brown" 
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User ID is required.")]
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the user's display name.
    /// </summary>
    /// <remarks>
    /// The user's name serves as the primary display identifier throughout the ADMS system user interface.
    /// This field supports professional naming conventions and is used for user identification in UI elements,
    /// audit trail displays, activity records, and system reports.
    /// 
    /// <para><strong>Professional Name Support:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Full Names:</strong> "Robert Brown", "Jennifer Smith", "Michael Johnson"</item>
    /// <item><strong>Professional Titles:</strong> "Dr. Smith", "Prof. Johnson", "J. Brown"</item>
    /// <item><strong>Complex Names:</strong> "Mary Johnson-Brown", "Robert Smith III"</item>
    /// <item><strong>International Names:</strong> Various cultural naming conventions</item>
    /// </list>
    /// 
    /// <para><strong>Validation Rules (via ADMS.API.Common.UserValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null, empty, or whitespace</item>
    /// <item><strong>Length:</strong> 2-50 characters (matching ADMS.API.Entities.User constraints)</item>
    /// <item><strong>Format:</strong> Letters, numbers, spaces, periods, hyphens, underscores</item>
    /// <item><strong>Professional Standards:</strong> Must start/end with letter or number</item>
    /// <item><strong>Reserved Names:</strong> Cannot use system reserved names</item>
    /// </list>
    /// 
    /// <para><strong>UI Display Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>User Selection:</strong> Dropdown lists, autocomplete controls</item>
    /// <item><strong>Audit Display:</strong> "Created by Robert Brown" in activity logs</item>
    /// <item><strong>Attribution:</strong> "Document checked out by Jennifer Smith"</item>
    /// <item><strong>User Lists:</strong> User management and selection interfaces</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional name examples
    /// var user1 = new UserMinimalDto { Id = id1, Name = "Robert Brown" };
    /// var user2 = new UserMinimalDto { Id = id2, Name = "Dr. Jennifer Smith" };
    /// var user3 = new UserMinimalDto { Id = id3, Name = "M. Johnson-Brown" };
    /// 
    /// // UI display usage
    /// var displayText = $"Document created by {user.Name}";
    /// var auditEntry = $"{user.Name} performed action at {DateTime.Now}";
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User name is required.")]
    [StringLength(
        50, // UserValidationHelper.MaxUserNameLength
        MinimumLength = 2, // UserValidationHelper.MinUserNameLength
        ErrorMessage = "User name must be between 2 and 50 characters.")]
    public required string Name { get; init; }

    #endregion Core Properties

    #region Computed Properties

    /// <summary>
    /// Gets the normalized version of the user's name for comparison and search operations.
    /// </summary>
    /// <remarks>
    /// This computed property provides a normalized version of the user name suitable for:
    /// <list type="bullet">
    /// <item><strong>Search Operations:</strong> Case-insensitive user searches and filtering</item>
    /// <item><strong>Uniqueness Checks:</strong> Preventing duplicate user names with formatting differences</item>
    /// <item><strong>Comparison Operations:</strong> Reliable user name equivalence testing</item>
    /// <item><strong>Data Cleanup:</strong> Consistent name formatting across the system</item>
    /// </list>
    /// 
    /// The normalization follows the same rules as ADMS.API.Entities.User.NormalizedName and
    /// ADMS.API.Common.UserValidationHelper.NormalizeName() for consistency.
    /// </remarks>
    /// <example>
    /// <code>
    /// var user = new UserMinimalDto { Id = id, Name = "  Robert   Brown  " };
    /// var normalized = user.NormalizedName; // Returns "Robert Brown"
    /// 
    /// // Search and comparison usage
    /// var matchingUsers = users.Where(u => u.NormalizedName?.Contains(searchTerm, 
    ///     StringComparison.OrdinalIgnoreCase) == true);
    /// </code>
    /// </example>
    public string? NormalizedName => UserValidationHelper.NormalizeName(Name);

    /// <summary>
    /// Gets a value indicating whether this user DTO has valid data for system operations.
    /// </summary>
    /// <remarks>
    /// This property provides a quick validation check without running full validation logic.
    /// Useful for UI scenarios where immediate feedback is needed.
    /// 
    /// <para><strong>Validation Checks:</strong></para>
    /// <list type="bullet">
    /// <item>User ID is not empty</item>
    /// <item>Name is allowed by validation rules</item>
    /// <item>All required properties are properly set</item>
    /// </list>
    /// </remarks>
    public bool IsValid => UserValidationHelper.IsValidUserId(Id) && UserValidationHelper.IsNameAllowed(Name);

    /// <summary>
    /// Gets the display text suitable for UI controls and user identification.
    /// </summary>
    /// <remarks>
    /// Provides a consistent format for displaying user information in UI elements.
    /// Uses the normalized name for consistent display while preserving professional formatting.
    /// </remarks>
    /// <example>
    /// <code>
    /// var user = new UserMinimalDto { Id = id, Name = "Robert Brown" };
    /// var displayText = user.DisplayText; // Returns "Robert Brown"
    /// 
    /// // UI usage
    /// userDropdown.Items.Add(new ListItem(user.DisplayText, user.Id.ToString()));
    /// </code>
    /// </example>
    public string DisplayText => NormalizedName ?? Name;

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="UserMinimalDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using the ADMS.API.Common.UserValidationHelper for consistency 
    /// with entity validation rules. This ensures the DTO maintains the same validation standards as 
    /// the corresponding ADMS.API.Entities.User entity.
    /// 
    /// <para><strong>Validation Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>GUID Validation:</strong> Ensures valid user ID for system operations</item>
    /// <item><strong>Name Validation:</strong> Professional name format and business rule compliance</item>
    /// <item><strong>Length Validation:</strong> Database constraint alignment</item>
    /// <item><strong>Format Validation:</strong> Character set and professional naming standards</item>
    /// <item><strong>Reserved Name Protection:</strong> Prevents conflicts with system names</item>
    /// </list>
    /// 
    /// <para><strong>Integration with UserValidationHelper:</strong></para>
    /// Uses the centralized validation helper to ensure consistency across:
    /// <list type="bullet">
    /// <item>UserDto validation</item>
    /// <item>UserForCreationDto validation</item>
    /// <item>UserForUpdateDto validation</item>
    /// <item>ADMS.API.Entities.User entity validation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new UserMinimalDto { Id = Guid.Empty, Name = "" };
    /// var context = new ValidationContext(dto);
    /// var results = dto.Validate(context);
    /// 
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"Validation Error: {result.ErrorMessage}");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate using centralized validation helper for consistency
        foreach (var result in ValidateUserId())
            yield return result;

        foreach (var result in ValidateUserName())
            yield return result;
    }

    /// <summary>
    /// Validates the <see cref="Id"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the user ID.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.UserValidationHelper.ValidateUserId for consistent validation
    /// across all user-related DTOs and entities.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateUserId()
    {
        return UserValidationHelper.ValidateUserId(Id, nameof(Id));
    }

    /// <summary>
    /// Validates the <see cref="Name"/> property using ADMS validation standards.
    /// </summary>
    /// <returns>A collection of validation results for the user name.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.UserValidationHelper.ValidateName for consistent validation
    /// across all user-related DTOs and entities. This ensures professional naming
    /// standards and business rule compliance.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateUserName()
    {
        return UserValidationHelper.ValidateName(Name, nameof(Name));
    }

    #endregion Validation Implementation

    #region Static Validation Methods

    /// <summary>
    /// Validates a <see cref="UserMinimalDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The UserMinimalDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate UserMinimalDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item><strong>API Validation:</strong> Validating incoming DTOs in API controllers</item>
    /// <item><strong>Service Layer:</strong> Validation before processing user operations</item>
    /// <item><strong>Unit Testing:</strong> Simplified validation testing without ValidationContext</item>
    /// <item><strong>UI Validation:</strong> Client-side validation feedback</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Validate a DTO instance
    /// var dto = new UserMinimalDto { Id = Guid.NewGuid(), Name = "Robert Brown" };
    /// var results = UserMinimalDto.ValidateModel(dto);
    /// 
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"User validation failed: {errorMessages}");
    /// }
    /// 
    /// // Validate null DTO
    /// var nullResults = UserMinimalDto.ValidateModel(null);
    /// // Returns validation error for null DTO
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] UserMinimalDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("UserMinimalDto instance is required and cannot be null."));
            return results;
        }

        // Use the built-in validation framework for comprehensive validation
        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a UserMinimalDto from an ADMS.API.Entities.User entity with validation.
    /// </summary>
    /// <param name="user">The User entity to convert. Cannot be null.</param>
    /// <returns>A valid UserMinimalDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create UserMinimalDto instances from
    /// ADMS.API.Entities.User entities with automatic validation. It ensures the resulting
    /// DTO is valid before returning it.
    /// 
    /// <para><strong>Validation Integration:</strong></para>
    /// The method validates the created DTO and throws a ValidationException if validation
    /// fails, ensuring only valid DTOs are returned.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity
    /// var user = new ADMS.API.Entities.User 
    /// { 
    ///     Id = Guid.NewGuid(), 
    ///     Name = "Robert Brown" 
    /// };
    /// 
    /// var dto = UserMinimalDto.FromEntity(user);
    /// // Returns validated UserMinimalDto instance
    /// </code>
    /// </example>
    public static UserMinimalDto FromEntity([NotNull] Entities.User user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        var dto = new UserMinimalDto
        {
            Id = user.Id,
            Name = user.Name
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid UserMinimalDto from entity: {errorMessages}");

    }

    #endregion Static Validation Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified UserMinimalDto is equal to the current UserMinimalDto.
    /// </summary>
    /// <param name="other">The UserMinimalDto to compare with the current UserMinimalDto.</param>
    /// <returns>true if the specified UserMinimalDto is equal to the current UserMinimalDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each user has a unique identifier.
    /// This follows the same equality pattern as ADMS.API.Entities.User for consistency.
    /// 
    /// <para><strong>Equality Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Two UserMinimalDto instances are equal if their Id properties are equal</item>
    /// <item>Empty GUIDs (Guid.Empty) are never considered equal</item>
    /// <item>Name differences do not affect equality (ID is the unique identifier)</item>
    /// </list>
    /// </remarks>
    public virtual bool Equals(UserMinimalDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current UserMinimalDto.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the UserMinimalDto.
    /// </summary>
    /// <returns>A string that represents the current UserMinimalDto.</returns>
    /// <remarks>
    /// The string representation includes both the user's name and ID for identification
    /// purposes, which is useful for debugging, logging, and development operations.
    /// Follows the same pattern as ADMS.API.Entities.User.ToString() for consistency.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new UserMinimalDto 
    /// { 
    ///     Id = Guid.Parse("50000000-0000-0000-0000-000000000001"), 
    ///     Name = "Robert Brown" 
    /// };
    /// Console.WriteLine(dto); // Output: "Robert Brown (50000000-0000-0000-0000-000000000001)"
    /// </code>
    /// </example>
    public override string ToString() => $"{DisplayText} ({Id})";

    #endregion String Representation
}