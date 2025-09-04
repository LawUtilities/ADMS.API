using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive User Data Transfer Object representing a user with all associated activity relationships.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of a user within the ADMS legal document management system,
/// including all activity associations and audit trail relationships. It mirrors the structure of 
/// <see cref="ADMS.API.Entities.User"/> while providing comprehensive validation and computed properties
/// for client-side operations.
/// 
/// <para><strong>Key Characteristics:</strong></strong>
/// <list type="bullet">
/// <item><strong>Complete Entity Representation:</strong> Mirrors all properties and relationships from ADMS.API.Entities.User</item>
/// <item><strong>Activity Integration:</strong> Includes all user activity collections for comprehensive audit trail support</item>
/// <item><strong>Professional Validation:</strong> Comprehensive validation using ADMS.API.Common.UserValidationHelper</item>
/// <item><strong>Computed Properties:</strong> Client-optimized computed properties for UI and business logic</item>
/// <item><strong>Collection Validation:</strong> Deep validation of all activity collections using DtoValidationHelper</item>
/// </list>
/// 
/// <para><strong>Entity Relationship Mirror:</strong></para>
/// This DTO maintains the same relationship structure as ADMS.API.Entities.User:
/// <list type="bullet">
/// <item><strong>MatterActivityUsers:</strong> User involvement in matter lifecycle events</item>
/// <item><strong>DocumentActivityUsers:</strong> User actions on documents (create, modify, delete)</item>
/// <item><strong>RevisionActivityUsers:</strong> User attribution for document version control</item>
/// <item><strong>MatterDocumentActivityUsersFrom:</strong> Source-side document transfers between matters</item>
/// <item><strong>MatterDocumentActivityUsersTo:</strong> Destination-side document transfers between matters</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>API Responses:</strong> Complete user data including all activity relationships</item>
/// <item><strong>User Management:</strong> Comprehensive user administration and management operations</item>
/// <item><strong>Audit Trail Display:</strong> Full user activity history and attribution</item>
/// <item><strong>Reporting:</strong> User-based reporting and analytics</item>
/// <item><strong>Administrative Operations:</strong> Complete user lifecycle management</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Professional Names:</strong> Support for complex professional names with validation</item>
/// <item><strong>Activity Attribution:</strong> Complete user accountability for all system operations</item>
/// <item><strong>Audit Compliance:</strong> Comprehensive audit trail relationships for legal compliance</item>
/// <item><strong>System Integration:</strong> Full integration with ADMS entity relationships and workflows</item>
/// </list>
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// <list type="bullet">
/// <item><strong>Lazy Loading Support:</strong> Activity collections can be populated on-demand</item>
/// <item><strong>Selective Loading:</strong> Individual activity collections can be loaded independently</item>
/// <item><strong>Computed Properties:</strong> Cached computed values for frequently accessed calculations</item>
/// <item><strong>Validation Optimization:</strong> Efficient validation using centralized helpers</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive user DTO
/// var userDto = new UserDto
/// {
///     Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
///     Name = "Robert Brown"
/// };
/// 
/// // Validating the complete user DTO
/// var validationResults = UserDto.ValidateModel(userDto);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Using computed properties
/// var totalActivities = userDto.TotalActivityCount;
/// var hasAuditTrail = userDto.HasActivities;
/// var displayName = userDto.DisplayName;
/// </code>
/// </example>
public class UserDto : IValidatableObject, IEquatable<UserDto>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the user's unique identifier.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the user within the ADMS system.
    /// The ID corresponds directly to the <see cref="ADMS.API.Entities.User.Id"/> property and is
    /// used for establishing relationships, audit trail associations, and all system operations
    /// requiring precise user identification.
    /// 
    /// <para><strong>Usage in System:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Foreign Key Relations:</strong> Referenced in all user activity junction entities</item>
    /// <item><strong>User Identification:</strong> Unique identification across all system operations</item>
    /// <item><strong>Database Operations:</strong> Primary key for user-related database queries</item>
    /// <item><strong>API Operations:</strong> User identification in REST API operations</item>
    /// <item><strong>Audit Trail Attribution:</strong> User accountability in audit and activity records</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.User.Id"/> with identical validation
    /// and behavior, ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Valid user ID examples from ADMS.API.Entities seed data
    /// var robertBrownId = Guid.Parse("50000000-0000-0000-0000-000000000001");
    /// var jenniferSmithId = Guid.Parse("50000000-0000-0000-0000-000000000002");
    /// 
    /// var userDto = new UserDto 
    /// { 
    ///     Id = robertBrownId, 
    ///     Name = "Robert Brown" 
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User ID is required.")]
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    /// <remarks>
    /// The user's name serves as the primary display identifier throughout the ADMS system.
    /// This property mirrors <see cref="ADMS.API.Entities.User.Name"/> with identical validation
    /// rules and supports professional naming conventions used in legal practice environments.
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
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property uses identical validation logic to <see cref="ADMS.API.Entities.User.Name"/>,
    /// ensuring consistency between entity and DTO validation results.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional name examples
    /// var user1 = new UserDto { Id = id1, Name = "Robert Brown" };
    /// var user2 = new UserDto { Id = id2, Name = "Dr. Jennifer Smith" };
    /// var user3 = new UserDto { Id = id3, Name = "M. Johnson-Brown" };
    /// 
    /// // Display usage
    /// var auditEntry = $"{user.Name} performed action at {DateTime.Now}";
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User name is required.")]
    [StringLength(UserValidationHelper.MaxUserNameLength, MinimumLength = UserValidationHelper.MinUserNameLength,
        ErrorMessage = "User name must be between 2 and 50 characters.")]
    public required string Name { get; set; }

    #endregion Core Properties

    #region Activity Relationship Collections

    /// <summary>
    /// Gets or sets the collection of matter activity user associations for this user.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.User.MatterActivityUsers"/> and maintains
    /// the many-to-many relationship between users and matter activities, providing a complete audit
    /// trail of user involvement in matter lifecycle events.
    /// 
    /// <para><strong>Activity Types Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> User created a new matter</item>
    /// <item><strong>ARCHIVED:</strong> User archived a matter</item>
    /// <item><strong>DELETED:</strong> User deleted a matter</item>
    /// <item><strong>RESTORED:</strong> User restored a deleted matter</item>
    /// <item><strong>UNARCHIVED:</strong> User unarchived a matter</item>
    /// <item><strong>VIEWED:</strong> User viewed a matter</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Significance:</strong></para>
    /// Each association includes a timestamp (CreatedAt) providing chronological tracking of
    /// user activities on matters, essential for legal compliance and operational oversight.
    /// 
    /// <para><strong>DTO Composition:</strong></para>
    /// Contains <see cref="MatterActivityUserDto"/> instances that include complete matter, activity,
    /// and user information for comprehensive audit trail presentation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing user's matter activities
    /// foreach (var activity in user.MatterActivityUsers)
    /// {
    ///     Console.WriteLine($"User performed {activity.MatterActivity?.Activity} " +
    ///                      $"on matter {activity.Matter?.Description} at {activity.CreatedAt}");
    /// }
    /// </code>
    /// </example>
    public ICollection<MatterActivityUserDto> MatterActivityUsers { get; set; } = new List<MatterActivityUserDto>();

    /// <summary>
    /// Gets or sets the collection of document activity user associations for this user.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.User.DocumentActivityUsers"/> and tracks
    /// all document-related activities performed by the user, maintaining a comprehensive audit trail
    /// of document operations essential for legal document management.
    /// 
    /// <para><strong>Activity Types Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> User created a new document</item>
    /// <item><strong>SAVED:</strong> User saved changes to a document</item>
    /// <item><strong>DELETED:</strong> User deleted a document</item>
    /// <item><strong>RESTORED:</strong> User restored a deleted document</item>
    /// <item><strong>CHECKED IN:</strong> User checked in a document</item>
    /// <item><strong>CHECKED OUT:</strong> User checked out a document</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance:</strong></para>
    /// Document activities are critical for legal compliance, providing evidence of who
    /// performed what actions on legal documents and when, supporting audit requirements
    /// and professional responsibility standards.
    /// 
    /// <para><strong>DTO Composition:</strong></para>
    /// Contains <see cref="DocumentActivityUserDto"/> instances that include complete document,
    /// activity, and user information for comprehensive audit trail presentation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Finding user's recent document activities
    /// var recentDocumentActivities = user.DocumentActivityUsers
    ///     .Where(da => da.CreatedAt >= DateTime.UtcNow.AddDays(-30))
    ///     .OrderByDescending(da => da.CreatedAt);
    /// </code>
    /// </example>
    public ICollection<DocumentActivityUserDto> DocumentActivityUsers { get; set; } = new List<DocumentActivityUserDto>();

    /// <summary>
    /// Gets or sets the collection of revision activity user associations for this user.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.User.RevisionActivityUsers"/> and maintains
    /// the relationship between users and document revision activities, providing detailed version control
    /// audit trails essential for legal document management and compliance.
    /// 
    /// <para><strong>Activity Types Included:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> User created a new document revision</item>
    /// <item><strong>SAVED:</strong> User saved changes to a revision</item>
    /// <item><strong>DELETED:</strong> User deleted a revision</item>
    /// <item><strong>RESTORED:</strong> User restored a deleted revision</item>
    /// </list>
    /// 
    /// <para><strong>Version Control Integration:</strong></para>
    /// Revision activities provide granular tracking of document version changes, enabling
    /// comprehensive version control and change management for legal documents where
    /// maintaining accurate revision histories is critical.
    /// 
    /// <para><strong>DTO Composition:</strong></para>
    /// Contains <see cref="RevisionActivityUserDto"/> instances that include complete revision,
    /// activity, and user information for detailed version control audit trails.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Tracking user's revision activities
    /// var userRevisionWork = user.RevisionActivityUsers
    ///     .GroupBy(ra => ra.Revision?.Document?.FileName)
    ///     .Select(g => new { Document = g.Key, RevisionCount = g.Count() });
    /// </code>
    /// </example>
    public ICollection<RevisionActivityUserDto> RevisionActivityUsers { get; set; } = new List<RevisionActivityUserDto>();

    /// <summary>
    /// Gets or sets the collection of "from" matter document activity user associations for this user.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.User.MatterDocumentActivityUsersFrom"/> and
    /// tracks instances where this user was the source (initiator) of document transfer operations
    /// between matters, such as moves or copies. This directional tracking is essential for maintaining
    /// comprehensive audit trails of document provenance.
    /// 
    /// <para><strong>Transfer Operations Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> User moved document from one matter to another</item>
    /// <item><strong>COPIED:</strong> User copied document from one matter to another</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Significance:</strong></para>
    /// The "from" association identifies the user who initiated the transfer, providing
    /// accountability and traceability for document movements between matters, which is
    /// critical for legal document management and compliance.
    /// 
    /// <para><strong>DTO Composition:</strong></para>
    /// Contains <see cref="MatterDocumentActivityUserFromDto"/> instances that include complete
    /// matter, document, activity, and user information for comprehensive transfer audit trails.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Finding documents this user has moved or copied from matters
    /// var documentsMovedByUser = user.MatterDocumentActivityUsersFrom
    ///     .Where(mdau => mdau.MatterDocumentActivity?.Activity == "MOVED")
    ///     .Select(mdau => new { 
    ///         Document = mdau.Document?.FileName, 
    ///         FromMatter = mdau.Matter?.Description,
    ///         When = mdau.CreatedAt 
    ///     });
    /// </code>
    /// </example>
    public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; set; } = new List<MatterDocumentActivityUserFromDto>();

    /// <summary>
    /// Gets or sets the collection of "to" matter document activity user associations for this user.
    /// </summary>
    /// <remarks>
    /// This collection mirrors <see cref="ADMS.API.Entities.User.MatterDocumentActivityUsersTo"/> and
    /// tracks instances where this user was the recipient (destination) of document transfer operations
    /// between matters. This complements the "from" tracking to provide complete bidirectional audit
    /// trails for document transfers.
    /// 
    /// <para><strong>Transfer Operations Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> User received document moved from another matter</item>
    /// <item><strong>COPIED:</strong> User received document copied from another matter</item>
    /// </list>
    /// 
    /// <para><strong>Bidirectional Tracking:</strong></para>
    /// While MatterDocumentActivityUsersFrom tracks who initiated transfers, this collection
    /// tracks who received them, providing complete visibility into document transfer chains
    /// and supporting comprehensive audit requirements.
    /// 
    /// <para><strong>DTO Composition:</strong></para>
    /// Contains <see cref="MatterDocumentActivityUserToDto"/> instances that include complete
    /// matter, document, activity, and user information for comprehensive transfer audit trails.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Finding documents received by this user from other matters
    /// var documentsReceivedByUser = user.MatterDocumentActivityUsersTo
    ///     .Where(mdau => mdau.MatterDocumentActivity?.Activity == "COPIED")
    ///     .Select(mdau => new { 
    ///         Document = mdau.Document?.FileName, 
    ///         ToMatter = mdau.Matter?.Description,
    ///         When = mdau.CreatedAt 
    ///     });
    /// </code>
    /// </example>
    public ICollection<MatterDocumentActivityUserToDto> MatterDocumentActivityUsersTo { get; set; } = new List<MatterDocumentActivityUserToDto>();

    #endregion Activity Relationship Collections

    #region Computed Properties

    /// <summary>
    /// Gets the total count of activities performed by this user across all activity types.
    /// </summary>
    /// <remarks>
    /// This computed property mirrors <see cref="ADMS.API.Entities.User.TotalActivityCount"/> and provides
    /// a quick overview of user activity levels within the system, useful for activity monitoring,
    /// user engagement tracking, and system usage analytics.
    /// 
    /// <para><strong>Activity Types Included:</strong></para>
    /// <list type="bullet">
    /// <item>Matter activities (create, archive, delete, etc.)</item>
    /// <item>Document activities (create, save, check in/out, etc.)</item>
    /// <item>Revision activities (create, save, delete, restore)</item>
    /// <item>Matter document transfer activities (move, copy)</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// This property calculates the count from loaded collections. For large datasets,
    /// consider using database-level aggregation for better performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"User {user.Name} has performed {user.TotalActivityCount} activities");
    /// </code>
    /// </example>
    public int TotalActivityCount =>
        MatterActivityUsers.Count +
        DocumentActivityUsers.Count +
        RevisionActivityUsers.Count +
        MatterDocumentActivityUsersFrom.Count +
        MatterDocumentActivityUsersTo.Count;

    /// <summary>
    /// Gets a value indicating whether this user has any recorded activities.
    /// </summary>
    /// <remarks>
    /// This property mirrors <see cref="ADMS.API.Entities.User.HasActivities"/> and is useful for
    /// determining if a user can be safely removed from the system or if doing so would compromise
    /// audit trail integrity. Users with activities should typically be deactivated rather than
    /// deleted to preserve audit trails.
    /// 
    /// <para><strong>Audit Trail Preservation:</strong></para>
    /// Users with activities represent important audit trail data that should be preserved
    /// for legal compliance and operational transparency requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (user.HasActivities)
    /// {
    ///     // User has audit trail data - consider deactivation instead of deletion
    ///     Console.WriteLine($"User {user.Name} has activity history and should not be deleted");
    /// }
    /// </code>
    /// </example>
    public bool HasActivities => TotalActivityCount > 0;

    /// <summary>
    /// Gets the normalized version of the user's name for comparison and search operations.
    /// </summary>
    /// <remarks>
    /// This property mirrors <see cref="ADMS.API.Entities.User.NormalizedName"/> and provides
    /// a normalized version of the user name suitable for case-insensitive comparisons, search
    /// operations, and uniqueness validation. The normalization follows the same patterns
    /// established in ADMS.API.Common.UserValidationHelper.
    /// 
    /// <para><strong>Normalization Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Trims leading and trailing whitespace</item>
    /// <item>Collapses multiple consecutive spaces to single spaces</item>
    /// <item>Preserves case for professional appearance</item>
    /// <item>Returns null for invalid or empty names</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var user1 = new UserDto { Id = id1, Name = "  Robert   Brown  " };
    /// var user2 = new UserDto { Id = id2, Name = "Robert Brown" };
    /// 
    /// // Both will have the same normalized name: "Robert Brown"
    /// bool areEquivalent = user1.NormalizedName == user2.NormalizedName; // true
    /// </code>
    /// </example>
    public string? NormalizedName => UserValidationHelper.NormalizeName(Name);

    /// <summary>
    /// Gets the display name optimized for user interface presentation.
    /// </summary>
    /// <remarks>
    /// This property provides a consistent display name for UI elements, using the normalized
    /// name when available or falling back to the original name. This ensures consistent
    /// user display across the application.
    /// </remarks>
    /// <example>
    /// <code>
    /// var displayText = user.DisplayName; // Returns normalized name or original name
    /// userLabel.Text = displayText;
    /// </code>
    /// </example>
    public string DisplayName => NormalizedName ?? Name;

    /// <summary>
    /// Gets a value indicating whether this user DTO has valid data.
    /// </summary>
    /// <remarks>
    /// This property provides a quick validation check without running full validation logic.
    /// Useful for UI scenarios where immediate feedback is needed.
    /// </remarks>
    public bool IsValid => UserValidationHelper.IsValidUserId(Id) && UserValidationHelper.IsNameAllowed(Name);

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="UserDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation including:
    /// <list type="bullet">
    /// <item><strong>Core Property Validation:</strong> ID and Name validation using UserValidationHelper</item>
    /// <item><strong>Collection Validation:</strong> Deep validation of all activity collections</item>
    /// <item><strong>Business Rule Validation:</strong> Ensures data consistency and integrity</item>
    /// <item><strong>Relationship Validation:</strong> Validates collection items and their relationships</item>
    /// </list>
    /// 
    /// <para><strong>Integration with Validation Helpers:</strong></para>
    /// Uses centralized validation helpers (UserValidationHelper, DtoValidationHelper) to ensure
    /// consistency across all user-related validation in the system.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new UserDto { Id = Guid.Empty, Name = "" };
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
        // Validate core properties
        foreach (var result in ValidateUserId())
            yield return result;

        foreach (var result in ValidateUserName())
            yield return result;

        // Validate all activity collections using centralized helper
        foreach (var result in DtoValidationHelper.ValidateCollection(MatterActivityUsers, nameof(MatterActivityUsers)))
            yield return result;

        foreach (var result in DtoValidationHelper.ValidateCollection(DocumentActivityUsers, nameof(DocumentActivityUsers)))
            yield return result;

        foreach (var result in DtoValidationHelper.ValidateCollection(RevisionActivityUsers, nameof(RevisionActivityUsers)))
            yield return result;

        foreach (var result in DtoValidationHelper.ValidateCollection(MatterDocumentActivityUsersFrom, nameof(MatterDocumentActivityUsersFrom)))
            yield return result;

        foreach (var result in DtoValidationHelper.ValidateCollection(MatterDocumentActivityUsersTo, nameof(MatterDocumentActivityUsersTo)))
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

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="UserDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The UserDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate UserDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// 
    /// <para><strong>Null Safety:</strong></para>
    /// Handles null input gracefully by returning appropriate validation errors rather than throwing exceptions.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new UserDto { Id = Guid.NewGuid(), Name = "Robert Brown" };
    /// var results = UserDto.ValidateModel(dto);
    /// 
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"User validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] UserDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("UserDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a UserDto from an ADMS.API.Entities.User entity with validation.
    /// </summary>
    /// <param name="user">The User entity to convert. Cannot be null.</param>
    /// <param name="includeActivities">Whether to include activity collections in the conversion.</param>
    /// <returns>A valid UserDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a safe way to create UserDto instances from
    /// ADMS.API.Entities.User entities with automatic validation. It ensures the resulting
    /// DTO is valid before returning it.
    /// 
    /// <para><strong>Activity Collection Handling:</strong></para>
    /// When includeActivities is true, the method will attempt to map activity collections.
    /// For performance reasons, activity collections should typically be loaded separately
    /// using projection or explicit loading strategies.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from entity without activities for better performance
    /// var user = new ADMS.API.Entities.User 
    /// { 
    ///     Id = Guid.NewGuid(), 
    ///     Name = "Robert Brown" 
    /// };
    /// 
    /// var dto = UserDto.FromEntity(user, includeActivities: false);
    /// // Returns validated UserDto instance
    /// </code>
    /// </example>
    public static UserDto FromEntity([NotNull] Entities.User user, bool includeActivities = false)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        var dto = new UserDto
        {
            Id = user.Id,
            Name = user.Name
        };

        // Optionally include activity collections
        // Note: In practice, these would typically be mapped using a mapping framework
        // like AutoMapper or Mapster for better performance and maintainability
        if (includeActivities)
        {
            // Activity collections would be mapped here if needed
            // This is a placeholder for actual mapping logic
        }

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid UserDto from entity: {errorMessages}");

    }

    #endregion Static Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified UserDto is equal to the current UserDto.
    /// </summary>
    /// <param name="other">The UserDto to compare with the current UserDto.</param>
    /// <returns>true if the specified UserDto is equal to the current UserDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each user has a unique identifier.
    /// This follows the same equality pattern as ADMS.API.Entities.User for consistency.
    /// </remarks>
    public bool Equals(UserDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current UserDto.
    /// </summary>
    /// <param name="obj">The object to compare with the current UserDto.</param>
    /// <returns>true if the specified object is equal to the current UserDto; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as UserDto);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current UserDto.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Determines whether two UserDto instances are equal.
    /// </summary>
    /// <param name="left">The first UserDto to compare.</param>
    /// <param name="right">The second UserDto to compare.</param>
    /// <returns>true if the UserDtos are equal; otherwise, false.</returns>
    public static bool operator ==(UserDto? left, UserDto? right) => EqualityComparer<UserDto>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two UserDto instances are not equal.
    /// </summary>
    /// <param name="left">The first UserDto to compare.</param>
    /// <param name="right">The second UserDto to compare.</param>
    /// <returns>true if the UserDtos are not equal; otherwise, false.</returns>
    public static bool operator !=(UserDto? left, UserDto? right) => !(left == right);

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the UserDto.
    /// </summary>
    /// <returns>A string that represents the current UserDto.</returns>
    /// <remarks>
    /// The string representation includes both the user's name and ID for identification
    /// purposes, following the same pattern as ADMS.API.Entities.User.ToString() for consistency.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new UserDto 
    /// { 
    ///     Id = Guid.Parse("50000000-0000-0000-0000-000000000001"), 
    ///     Name = "Robert Brown" 
    /// };
    /// Console.WriteLine(dto); // Output: "Robert Brown (50000000-0000-0000-0000-000000000001)"
    /// </code>
    /// </example>
    public override string ToString() => $"{DisplayName} ({Id})";

    #endregion String Representation
}