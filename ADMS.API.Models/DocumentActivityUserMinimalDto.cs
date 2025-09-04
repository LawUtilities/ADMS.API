using ADMS.API.Common;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Models;

/// <summary>
/// Minimal Data Transfer Object representing the audit trail association between a document, document activity, and user for lightweight audit operations.
/// </summary>
/// <remarks>
/// This DTO serves as a lightweight representation of document activity audit trails within the ADMS legal document management system,
/// corresponding to <see cref="ADMS.API.Entities.DocumentActivityUser"/>. It provides essential audit trail information 
/// with minimal memory footprint while maintaining comprehensive validation and professional accountability standards 
/// for high-performance audit trail operations and UI display scenarios.
/// 
/// <para><strong>Enhanced with Standardized Validation (.NET 9):</strong></para>
/// <list type="bullet">
/// <item><strong>BaseValidationDto Integration:</strong> Inherits standardized ADMS validation patterns</item>
/// <item><strong>Minimal Audit Validation:</strong> Specialized validation for lightweight audit trail operations</item>
/// <item><strong>Performance Optimized:</strong> Uses yield return for lazy validation evaluation</item>
/// <item><strong>Validation Hierarchy:</strong> Follows standardized core → business → cross-property → custom pattern</item>
/// <item><strong>Minimal DTO Validation:</strong> Advanced validation for minimal navigation properties</item>
/// </list>
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Entity Representation:</strong> Contains essential properties from ADMS.API.Entities.DocumentActivityUser for performance</item>
/// <item><strong>Standardized Validation:</strong> Uses BaseValidationDto for consistent validation patterns</item>
/// <item><strong>Audit Trail Focus:</strong> Optimized for audit trail display and lightweight audit operations</item>
/// <item><strong>Professional Validation:</strong> Uses centralized validation helpers for data integrity</item>
/// <item><strong>Performance Optimized:</strong> Minimal DTOs for activity and user information to reduce memory usage</item>
/// </list>
/// 
/// <para><strong>Validation Hierarchy:</strong></para>
/// Following BaseValidationDto standardized validation pattern:
/// <list type="number">
/// <item><strong>Core Properties:</strong> Composite key validation (DocumentId, CreatedAt), minimal DTO validation</item>
/// <item><strong>Business Rules:</strong> Audit trail integrity, activity-specific business rules, professional standards</item>
/// <item><strong>Cross-Property:</strong> Minimal DTO consistency, foreign key validation, temporal validation</item>
/// <item><strong>Custom Rules:</strong> Audit-specific validation, security validations, performance validations</item>
/// </list>
/// 
/// <para><strong>Performance Benefits with Standardized Validation:</strong></para>
/// <list type="bullet">
/// <item><strong>Early Termination:</strong> Validation stops on critical errors for better performance</item>
/// <item><strong>Lazy Evaluation:</strong> Minimal DTOs validated only when needed</item>
/// <item><strong>Consistent Error Handling:</strong> Standardized error formatting and reporting</item>
/// <item><strong>Memory Efficient:</strong> Optimized validation memory usage for minimal audit operations</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a minimal document activity audit entry with standardized validation
/// var auditEntry = new DocumentActivityUserMinimalDto
/// {
///     DocumentId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
///     DocumentActivity = new DocumentActivityMinimalDto { /* activity data */ },
///     User = new UserMinimalDto { /* user data */ },
///     CreatedAt = DateTime.UtcNow
/// };
/// 
/// // Standardized validation using BaseValidationDto
/// var validationResults = BaseValidationDto.ValidateModel(auditEntry);
/// if (BaseValidationDto.HasValidationErrors(validationResults))
/// {
///     var summary = BaseValidationDto.GetValidationSummary(validationResults);
///     _logger.LogWarning("Minimal audit validation failed: {ValidationSummary}", summary);
/// }
/// 
/// // Professional audit trail processing with validation
/// if (auditEntry.IsValid)
/// {
///     ProcessMinimalAuditEntry(auditEntry);
/// }
/// </code>
/// </example>
public class DocumentActivityUserMinimalDto : BaseValidationDto, IEquatable<DocumentActivityUserMinimalDto>, IComparable<DocumentActivityUserMinimalDto>
{
    #region Composite Primary Key Properties

    /// <summary>
    /// Gets or sets the unique identifier of the document that the activity was performed on.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific document 
    /// that was the subject of the activity.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateGuid() with allowEmpty=false.
    /// </remarks>
    [Required(ErrorMessage = "Document ID is required for activity audit trail identification.")]
    public required Guid DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the document activity was performed.
    /// </summary>
    /// <remarks>
    /// This DateTime serves as part of the composite primary key and records the precise moment when 
    /// the document activity occurred.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateDateTime() with audit trail constraints.
    /// </remarks>
    [Required(ErrorMessage = "Activity timestamp is required for audit trail chronology.")]
    public required DateTime CreatedAt { get; set; }

    #endregion Composite Primary Key Properties

    #region Minimal Navigation Properties

    /// <summary>
    /// Gets or sets the minimal document activity information describing the type of operation performed.
    /// </summary>
    /// <remarks>
    /// This property provides essential activity classification information for the document operation, 
    /// using <see cref="DocumentActivityMinimalDto"/> to minimize memory usage while maintaining complete 
    /// activity identification.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCrossPropertyRules() for minimal DTO consistency and validity.
    /// </remarks>
    [Required(ErrorMessage = "Document activity information is required for operation classification.")]
    public required DocumentActivityMinimalDto DocumentActivity { get; set; }

    /// <summary>
    /// Gets or sets the minimal user information for the user who performed the document activity.
    /// </summary>
    /// <remarks>
    /// This property provides essential user information for the professional who performed the document 
    /// operation, using <see cref="UserMinimalDto"/> to minimize memory usage while maintaining complete 
    /// user identification.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCrossPropertyRules() for minimal DTO consistency and validity.
    /// </remarks>
    [Required(ErrorMessage = "User information is required for professional accountability and audit attribution.")]
    public required UserMinimalDto User { get; set; }

    #endregion Minimal Navigation Properties

    #region Computed Properties

    /// <summary>
    /// Gets the creation date and time in local time zone, formatted for professional display.
    /// </summary>
    public string LocalCreatedAtDateString => CreatedAt.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets a professional summary of the document activity for audit trail display.
    /// </summary>
    public string ActivitySummary =>
        $"Document {DocumentId} " +
        $"{DocumentActivity?.Activity ?? "UNKNOWN"} " +
        $"by {User?.Name ?? "Unknown User"}";

    /// <summary>
    /// Gets a value indicating whether this activity represents a version control operation.
    /// </summary>
    public bool IsVersionControlActivity =>
        string.Equals(DocumentActivity?.Activity, "CHECKED IN", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(DocumentActivity?.Activity, "CHECKED OUT", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this activity represents a document creation operation.
    /// </summary>
    public bool IsCreationActivity =>
        string.Equals(DocumentActivity?.Activity, "CREATED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this activity represents a content modification operation.
    /// </summary>
    public bool IsContentModificationActivity =>
        string.Equals(DocumentActivity?.Activity, "SAVED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this activity represents a state change operation.
    /// </summary>
    public bool IsStateChangeActivity =>
        string.Equals(DocumentActivity?.Activity, "DELETED", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(DocumentActivity?.Activity, "RESTORED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the activity category for classification purposes.
    /// </summary>
    public string ActivityCategory => DocumentActivity?.Activity?.ToUpperInvariant() switch
    {
        "CREATED" or "DELETED" or "RESTORED" => "Lifecycle",
        "CHECKED IN" or "CHECKED OUT" => "Version Control",
        "SAVED" => "Content Management",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets a user-friendly display of the time elapsed since the activity was performed.
    /// </summary>
    public string TimeElapsedDisplay
    {
        get
        {
            var elapsed = DateTime.UtcNow - CreatedAt;

            return elapsed.TotalMinutes switch
            {
                < 1 => "Just now",
                < 60 => $"{elapsed.TotalMinutes:F0} minutes ago",
                < 1440 => $"{elapsed.TotalHours:F0} hours ago", // < 24 hours
                < 43200 => $"{elapsed.TotalDays:F0} days ago", // < 30 days
                _ => CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
            };
        }
    }

    /// <summary>
    /// Gets a value indicating whether this audit entry DTO is valid for system operations.
    /// </summary>
    public bool IsValid =>
        DocumentId != Guid.Empty &&
        DocumentActivity.IsValid == true &&
        User.IsValid == true &&
        CreatedAt != default;

    /// <summary>
    /// Gets a value indicating whether this activity was performed recently (within the last hour).
    /// </summary>
    public bool IsRecentActivity => (DateTime.UtcNow - CreatedAt).TotalHours < 1;

    #endregion Computed Properties

    #region Standardized Validation Implementation

    /// <summary>
    /// Validates core properties such as composite key components using ADMS validation helpers.
    /// </summary>
    /// <returns>A collection of validation results for core property validation.</returns>
    /// <remarks>
    /// This method implements the first step of the BaseValidationDto validation hierarchy,
    /// validating the essential components of the minimal audit trail entry.
    /// 
    /// <para><strong>Core Property Validation Steps:</strong></para>
    /// <list type="number">
    /// <item>Document ID validation using BaseValidationDto.ValidateGuid() (does not allow empty)</item>
    /// <item>Created timestamp validation using BaseValidationDto.ValidateDateTime() with audit constraints</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCoreProperties()
    {
        // Validate document ID using standardized GUID validation
        foreach (var result in ValidateGuid(DocumentId, nameof(DocumentId), allowEmpty: false))
            yield return result;

        // Validate timestamp using standardized date validation with audit trail constraints
        foreach (var result in ValidateDateTime(CreatedAt, nameof(CreatedAt),
                     allowFuture: false, // Audit entries cannot be in future
                     allowPast: true,    // Allow historical entries
                     maxFutureOffset: TimeSpan.FromMinutes(1), // Allow small clock skew
                     maxPastOffset: TimeSpan.FromDays(365 * 10),    // Reasonable audit history
                     allowDefault: false)) // Must have valid timestamp
        {
            yield return result;
        }
    }

    /// <summary>
    /// Validates business rules specific to minimal document activity audit trails and professional standards.
    /// </summary>
    /// <returns>A collection of validation results for business rule validation.</returns>
    /// <remarks>
    /// This method implements the second step of the BaseValidationDto validation hierarchy,
    /// validating domain-specific business rules for minimal audit trail operations.
    /// 
    /// <para><strong>Business Rules Validated:</strong></para>
    /// <list type="bullet">
    /// <item>Audit trail temporal integrity</item>
    /// <item>Professional standards for minimal audit entries</item>
    /// <item>Activity frequency and pattern validation</item>
    /// <item>System user activity restrictions</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Business rule: Audit trail temporal integrity
        if (CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            yield return CreateValidationResult(
                "Activity creation timestamp cannot be significantly in the future for audit trail integrity.",
                nameof(CreatedAt));
        }

        if (CreatedAt < DateTime.UtcNow.AddYears(-15))
        {
            yield return CreateValidationResult(
                "Activity creation timestamp seems unreasonably old. Verify timestamp accuracy for audit trail integrity.",
                nameof(CreatedAt));
        }

        // Business rule: Professional standards for audit entries
        // System users should have limited activity types
        if (User.Name.Contains("System", StringComparison.OrdinalIgnoreCase) == true)
        {
            var restrictedActivities = new[] { "CHECKED OUT", "CHECKED IN" };
            if (restrictedActivities.Contains(DocumentActivity.Activity?.ToUpperInvariant()))
            {
                yield return CreateValidationResult(
                    "System users should not perform manual version control operations for professional accountability.",
                    nameof(User), nameof(DocumentActivity));
            }
        }

        // Validate professional naming patterns
        if (User.Name?.Contains("test", StringComparison.OrdinalIgnoreCase) == true ||
            User.Name?.Contains("temp", StringComparison.OrdinalIgnoreCase) == true)
        {
            yield return CreateValidationResult(
                "Audit entries should not reference test or temporary users for professional accountability.",
                nameof(User));
        }

        // Business rule: Activity frequency validation for audit integrity
        var currentHour = DateTime.UtcNow.Hour;
        var activityHour = CreatedAt.Hour;

        // Check for activities outside normal business hours (might indicate automated processes)
        if (activityHour is < 6 or > 22) // Outside 6 AM - 10 PM
        {
            yield return CreateValidationResult(
                "Activity performed outside normal business hours. Verify this is intentional for audit trail accuracy.",
                nameof(CreatedAt));
        }
    }

    /// <summary>
    /// Validates cross-property relationships and consistency including minimal DTO validation.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    /// <remarks>
    /// This method implements the third step of the BaseValidationDto validation hierarchy,
    /// validating relationships between properties and minimal DTO consistency.
    /// 
    /// <para><strong>Cross-Property Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Minimal DTO validation and consistency</item>
    /// <item>Activity classification and timing consistency</item>
    /// <item>User authorization and activity appropriateness</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // Cross-property rule: DocumentActivity minimal DTO validation
        // Validate the minimal DTO using BaseValidationDto if it inherits from it
        if (DocumentActivity is BaseValidationDto validatableActivity)
        {
            var activityResults = ValidateModel(validatableActivity);
            foreach (var result in activityResults)
            {
                yield return CreateValidationResult(
                    $"Document activity validation failed: {result.ErrorMessage}",
                    $"{nameof(DocumentActivity)}.{string.Join(",", result.MemberNames)}");
            }
        }

        // Validate activity appropriateness for audit trails
        if (string.IsNullOrWhiteSpace(DocumentActivity.Activity))
        {
            yield return CreateValidationResult(
                "Document activity must have a valid activity classification for audit trail integrity.",
                nameof(DocumentActivity));
        }

        // Cross-property rule: User minimal DTO validation
        // Validate the minimal DTO using BaseValidationDto if it inherits from it
        if (User is IValidatableObject validatableUser)
        {
            var userContext = new ValidationContext(User);
            var userResults = validatableUser.Validate(userContext);
            foreach (var result in userResults)
            {
                yield return CreateValidationResult(
                    $"User validation failed: {result.ErrorMessage}",
                    $"{nameof(User)}.{string.Join(",", result.MemberNames)}");
            }
        }

        // Validate user appropriateness for audit attribution
        if (string.IsNullOrWhiteSpace(User.Name))
        {
            yield return CreateValidationResult(
                "User must have a valid name for professional accountability in audit trails.",
                nameof(User));
        }

        if (User.Id == Guid.Empty)
        {
            yield return CreateValidationResult(
                "User must have a valid ID for audit trail consistency.",
                nameof(User));
        }

        // Cross-property rule: Activity and user consistency
        // Validate that user and activity combination makes sense
        if (DocumentActivity.Activity.ToUpperInvariant() == "CREATED" &&
            User.Name?.Contains("Guest", StringComparison.OrdinalIgnoreCase) == true)
        {
            yield return CreateValidationResult(
                "Guest users should not be able to create documents for professional accountability.",
                nameof(User), nameof(DocumentActivity));
        }

        // Validate activity timing consistency
        if (DocumentActivity.Activity?.ToUpperInvariant() == "RESTORED" &&
            (DateTime.UtcNow - CreatedAt).TotalDays < 1)
        {
            yield return CreateValidationResult(
                "Document restoration activities should typically occur after a reasonable period following deletion.",
                nameof(CreatedAt), nameof(DocumentActivity));
        }
    }

    /// <summary>
    /// Validates custom rules specific to minimal audit trail entries and performance optimization.
    /// </summary>
    /// <returns>A collection of validation results for custom validation.</returns>
    /// <remarks>
    /// This method implements custom validation logic specific to minimal audit trail entries,
    /// including performance considerations and minimal DTO-specific validations.
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCustomRules()
    {
        // Custom rule: Validate audit trail completeness for minimal DTOs

        // Custom rule: Performance validation for minimal DTOs
        // Validate that minimal DTOs contain sufficient information
        var hasMinimalActivityInfo = !string.IsNullOrWhiteSpace(DocumentActivity.Activity) &&
                                     DocumentActivity.Id != Guid.Empty;
        var hasMinimalUserInfo = !string.IsNullOrWhiteSpace(User.Name) &&
                                 User.Id != Guid.Empty;

        if (!hasMinimalActivityInfo || !hasMinimalUserInfo)
        {
            yield return CreateValidationResult(
                "Minimal audit trail entries must contain complete essential information for audit integrity.",
                nameof(DocumentActivity), nameof(User));
        }

        // Custom rule: Temporal consistency validation for audit chains
        if (CreatedAt != default)
        {
            // Check for suspiciously precise timestamps (might indicate batch processing)
            if (CreatedAt is { Millisecond: 0, Second: 0 })
            {
                yield return CreateValidationResult(
                    "Activity timestamp has exact minute precision which may indicate batch processing. Verify individual activity attribution.",
                    nameof(CreatedAt));
            }

            // Validate activity sequence logic for creation activities
            if (DocumentActivity.Activity.ToUpperInvariant() == "CREATED" &&
                CreatedAt.Date != DateTime.UtcNow.Date)
            {
                // Creation activities more than a day old should be verified
                yield return CreateValidationResult(
                    "Creation activities recorded with non-current dates should be verified for audit trail accuracy.",
                    nameof(CreatedAt), nameof(DocumentActivity));
            }
        }

        // Custom rule: Minimal DTO security validation
        if (User != null)
        {
            // Validate that user names don't contain potential security issues
            var securityPatterns = new[] { "<script", "javascript:", "data:", "vbscript:" };
            foreach (var pattern in securityPatterns)
            {
                if (User.Name?.Contains(pattern, StringComparison.OrdinalIgnoreCase) == true)
                {
                    yield return CreateValidationResult(
                        $"User name contains potentially unsafe pattern '{pattern}'. This may indicate a security issue.",
                        nameof(User));
                }
            }

            // Validate activity patterns for potential security issues
            if (DocumentActivity.Activity?.Contains("SCRIPT", StringComparison.OrdinalIgnoreCase) == true)
            {
                yield return CreateValidationResult(
                    "Activity classification contains potentially unsafe patterns. Verify activity legitimacy.",
                    nameof(DocumentActivity));
            }
        }

        // Custom rule: Performance optimization validation
        if (User == null) yield break;
        // Check for excessively long names that might impact performance
        if (User.Name?.Length > 100)
        {
            yield return CreateValidationResult(
                "User name is excessively long and may impact performance in minimal DTO scenarios.",
                nameof(User));
        }

        if (DocumentActivity.Activity?.Length > 50)
        {
            yield return CreateValidationResult(
                "Activity classification is excessively long and may impact performance in minimal DTO scenarios.",
                nameof(DocumentActivity));
        }
    }

    #endregion Standardized Validation Implementation

    #region Static Methods

    /// <summary>
    /// Creates a DocumentActivityUserMinimalDto from an ADMS.API.Entities.DocumentActivityUser entity with standardized validation.
    /// </summary>
    /// <param name="entity">The DocumentActivityUser entity to convert. Cannot be null.</param>
    /// <returns>A valid DocumentActivityUserMinimalDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entity = await context.DocumentActivityUsers
    ///     .Include(dau => dau.DocumentActivity)
    ///     .Include(dau => dau.User)
    ///     .FirstAsync(dau => dau.DocumentId == targetDocumentId);
    /// 
    /// var auditDto = DocumentActivityUserMinimalDto.FromEntity(entity);
    /// // DTO is guaranteed to be valid due to standardized validation
    /// </code>
    /// </example>
    public static DocumentActivityUserMinimalDto FromEntity([NotNull] Entities.DocumentActivityUser entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var auditDto = new DocumentActivityUserMinimalDto
        {
            DocumentId = entity.DocumentId,
            CreatedAt = entity.CreatedAt,
            DocumentActivity = DocumentActivityMinimalDto.FromEntity(entity.DocumentActivity),
            User = UserMinimalDto.FromEntity(entity.User)
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(auditDto);
        if (!HasValidationErrors(validationResults)) return auditDto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Document activity audit validation failed: {summary}");
    }

    /// <summary>
    /// Creates multiple DocumentActivityUserMinimalDto instances from a collection of entities with standardized validation.
    /// </summary>
    /// <param name="entities">The collection of DocumentActivityUser entities to convert. Cannot be null.</param>
    /// <returns>A collection of valid DocumentActivityUserMinimalDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method uses standardized validation and provides detailed error handling
    /// for invalid entities.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entities = await context.DocumentActivityUsers
    ///     .Include(dau => dau.DocumentActivity)
    ///     .Include(dau => dau.User)
    ///     .Where(dau => dau.DocumentId == documentId)
    ///     .ToListAsync();
    /// 
    /// var auditDtos = DocumentActivityUserMinimalDto.FromEntities(entities);
    /// // All DTOs are guaranteed to be valid
    /// </code>
    /// </example>
    public static IList<DocumentActivityUserMinimalDto> FromEntities([NotNull] IEnumerable<Entities.DocumentActivityUser> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var result = new List<DocumentActivityUserMinimalDto>();
        var errors = new List<string>();

        foreach (var entity in entities)
        {
            try
            {
                var dto = FromEntity(entity);
                result.Add(dto);
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                // Collect errors for comprehensive error reporting
                errors.Add($"Minimal Audit {entity.DocumentId}/{entity.DocumentActivityId}/{entity.UserId}: {ex.Message}");

                // In production, use proper logging framework
                Console.WriteLine($"Warning: Skipped invalid minimal audit entity: {ex.Message}");
            }
        }

        // Log summary if there were errors
        if (errors.Any())
        {
            Console.WriteLine($"Entity conversion completed with {errors.Count} errors out of {entities.Count()} entities processed.");
        }

        return result;
    }

    /// <summary>
    /// Creates a DocumentActivityUserMinimalDto for a specific activity type using seeded activity GUIDs with standardized validation.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="activityName">The standardized activity name.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="userName">The user name.</param>
    /// <param name="timestamp">Optional timestamp (uses current UTC time if not provided).</param>
    /// <returns>A DocumentActivityUserMinimalDto with the appropriate seeded activity GUID.</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid or activity is not allowed.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var creationAudit = DocumentActivityUserMinimalDto.CreateStandardActivity(
    ///     documentId,
    ///     "CREATED",
    ///     userId,
    ///     "Robert Brown");
    /// 
    /// // DTO is guaranteed to have valid minimal components and appropriate seeded GUID
    /// Console.WriteLine($"Created activity ID: {creationAudit.DocumentActivity.Id}");
    /// </code>
    /// </example>
    public static DocumentActivityUserMinimalDto CreateStandardActivity(
        Guid documentId,
        [NotNull] string activityName,
        Guid userId,
        [NotNull] string userName,
        DateTime? timestamp = null)
    {
        if (documentId == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty.", nameof(documentId));

        ArgumentException.ThrowIfNullOrWhiteSpace(activityName, nameof(activityName));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        ArgumentException.ThrowIfNullOrWhiteSpace(userName, nameof(userName));

        var normalizedActivity = activityName.Trim().ToUpperInvariant();
        var activityId = DocumentActivityMinimalDto.GetSeededActivityId(normalizedActivity);

        if (activityId == Guid.Empty)
        {
            throw new ArgumentException(
                $"Activity '{activityName}' is not a standard seeded activity. " +
                $"Allowed activities: {DocumentActivityValidationHelper.AllowedActivitiesList}",
                nameof(activityName));
        }

        var auditDto = new DocumentActivityUserMinimalDto
        {
            DocumentId = documentId,
            CreatedAt = timestamp ?? DateTime.UtcNow,
            DocumentActivity = new DocumentActivityMinimalDto
            {
                Id = activityId,
                Activity = normalizedActivity
            },
            User = new UserMinimalDto
            {
                Id = userId,
                Name = userName.Trim()
            }
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(auditDto);
        if (!HasValidationErrors(validationResults)) return auditDto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Document activity audit validation failed: {summary}");
    }

    /// <summary>
    /// Creates a DocumentActivityUserMinimalDto with validated minimal DTOs and standardized validation.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="activityMinimal">The document activity minimal DTO.</param>
    /// <param name="userMinimal">The user minimal DTO.</param>
    /// <param name="timestamp">Optional timestamp (uses current UTC time if not provided).</param>
    /// <returns>A DocumentActivityUserMinimalDto with validated minimal DTOs.</returns>
    /// <exception cref="ArgumentException">Thrown when core parameters are invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation
    /// while allowing pre-validated minimal DTOs to be provided.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activityMinimal = DocumentActivityMinimalDto.CreateStandardActivity("CREATED");
    /// var userMinimal = UserMinimalDto.FromEntity(userEntity);
    /// 
    /// var auditEntry = DocumentActivityUserMinimalDto.CreateFromMinimalDtos(
    ///     documentId,
    ///     activityMinimal,
    ///     userMinimal);
    /// 
    /// // DTO is guaranteed to be valid with validated minimal DTOs
    /// </code>
    /// </example>
    public static DocumentActivityUserMinimalDto CreateFromMinimalDtos(
        Guid documentId,
        [NotNull] DocumentActivityMinimalDto activityMinimal,
        [NotNull] UserMinimalDto userMinimal,
        DateTime? timestamp = null)
    {
        if (documentId == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty.", nameof(documentId));

        ArgumentNullException.ThrowIfNull(activityMinimal);
        ArgumentNullException.ThrowIfNull(userMinimal);

        var auditDto = new DocumentActivityUserMinimalDto
        {
            DocumentId = documentId,
            CreatedAt = timestamp ?? DateTime.UtcNow,
            DocumentActivity = activityMinimal,
            User = userMinimal
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(auditDto);
        if (!HasValidationErrors(validationResults)) return auditDto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Document activity audit validation failed: {summary}");
    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Gets a comprehensive audit trail summary for this minimal activity entry.
    /// </summary>
    /// <returns>A detailed string containing activity, user, and timing information.</returns>
    public string GetAuditTrailSummary()
    {
        var activityType = DocumentActivity?.Activity ?? "UNKNOWN";
        var userName = User?.Name ?? "Unknown User";

        return $"Document {DocumentId} {activityType} by {userName} on {LocalCreatedAtDateString}";
    }

    /// <summary>
    /// Determines whether this audit entry represents the same logical operation as another entry.
    /// </summary>
    /// <param name="other">The other audit entry to compare.</param>
    /// <returns>true if both entries represent the same logical operation; otherwise, false.</returns>
    public bool IsSameOperation(DocumentActivityUserMinimalDto? other)
    {
        if (other is null) return false;

        return DocumentId == other.DocumentId &&
               DocumentActivity.Id == other.DocumentActivity.Id &&
               User.Id == other.User.Id;
    }

    /// <summary>
    /// Gets the time elapsed since this activity was performed.
    /// </summary>
    /// <returns>A TimeSpan representing the time elapsed since the activity.</returns>
    public TimeSpan GetTimeElapsed() => DateTime.UtcNow - CreatedAt;

    /// <summary>
    /// Determines whether this activity was performed within the specified time period.
    /// </summary>
    /// <param name="timespan">The time period to check against.</param>
    /// <returns>true if the activity was performed within the specified timespan; otherwise, false.</returns>
    public bool WasPerformedWithin(TimeSpan timespan) => GetTimeElapsed() <= timespan;

    /// <summary>
    /// Gets usage statistics for this minimal audit entry for reporting purposes with validation analysis.
    /// </summary>
    /// <returns>A dictionary containing comprehensive audit entry statistics and validation status.</returns>
    /// <remarks>
    /// This method provides comprehensive insights including validation status for enhanced analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = auditEntry.GetAuditStatistics();
    /// Console.WriteLine($"Activity: {stats["ActivityType"]}");
    /// Console.WriteLine($"Validation status: {stats["ValidationStatus"]}");
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetAuditStatistics()
    {
        // Perform validation to get current status
        var validationResults = ValidateModel(this);
        var validationStatus = HasValidationErrors(validationResults)
            ? GetValidationSummary(validationResults)
            : "Valid";

        return new Dictionary<string, object>
        {
            ["ActivityType"] = DocumentActivity?.Activity ?? "Unknown",
            ["ActivityCategory"] = ActivityCategory,
            ["UserName"] = User?.Name ?? "Unknown",
            ["DocumentId"] = DocumentId,
            ["Timestamp"] = CreatedAt,
            ["LocalTimestamp"] = LocalCreatedAtDateString,
            ["TimeElapsed"] = TimeElapsedDisplay,
            ["IsRecentActivity"] = IsRecentActivity,
            ["IsVersionControlActivity"] = IsVersionControlActivity,
            ["IsCreationActivity"] = IsCreationActivity,
            ["IsContentModificationActivity"] = IsContentModificationActivity,
            ["IsStateChangeActivity"] = IsStateChangeActivity,
            ["ActivitySummary"] = ActivitySummary,
            ["ValidationStatus"] = validationStatus,
            ["IsValid"] = !HasValidationErrors(validationResults)
        };
    }

    /// <summary>
    /// Determines whether this audit entry involves a specific document activity type.
    /// </summary>
    /// <param name="activityName">The activity name to check (case-insensitive).</param>
    /// <returns>true if this audit entry involves the specified activity type; otherwise, false.</returns>
    public bool Involves(string? activityName) =>
        DocumentActivity?.MatchesActivity(activityName) == true;

    /// <summary>
    /// Determines whether this audit entry was performed by a specific user.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <returns>true if this audit entry was performed by the specified user; otherwise, false.</returns>
    public bool WasPerformedBy(Guid userId) => User?.Id == userId;

    /// <summary>
    /// Determines whether this audit entry was performed by a user with a specific name.
    /// </summary>
    /// <param name="userName">The user name to check (case-insensitive).</param>
    /// <returns>true if this audit entry was performed by a user with the specified name; otherwise, false.</returns>
    public bool WasPerformedByUser(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(User?.Name))
            return false;

        return string.Equals(User.Name.Trim(), userName.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets comprehensive minimal audit information including validation analysis.
    /// </summary>
    /// <returns>A dictionary containing detailed minimal audit information and validation status.</returns>
    /// <remarks>
    /// This method provides structured audit information including validation status,
    /// useful for debugging, reporting, and administrative operations for minimal DTOs.
    /// </remarks>
    /// <example>
    /// <code>
    /// var info = auditEntry.GetMinimalAuditInformation();
    /// foreach (var (key, value) in info)
    /// {
    ///     Console.WriteLine($"{key}: {value}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetMinimalAuditInformation()
    {
        // Get audit statistics and validation status
        var stats = GetAuditStatistics();

        return new Dictionary<string, object>
        {
            // Core Information
            [nameof(DocumentId)] = DocumentId,
            [nameof(CreatedAt)] = CreatedAt,

            // Minimal DTO Information
            ["ActivityId"] = DocumentActivity?.Id ?? Guid.Empty,
            ["ActivityType"] = DocumentActivity?.Activity ?? "Unknown",
            ["UserId"] = User?.Id ?? Guid.Empty,
            ["UserName"] = User?.Name ?? "Unknown",

            // Classification Information
            ["ActivityCategory"] = ActivityCategory,
            ["IsVersionControlActivity"] = IsVersionControlActivity,
            ["IsCreationActivity"] = IsCreationActivity,
            ["IsContentModificationActivity"] = IsContentModificationActivity,
            ["IsStateChangeActivity"] = IsStateChangeActivity,

            // Temporal Information
            ["LocalTimestamp"] = LocalCreatedAtDateString,
            ["TimeElapsed"] = GetTimeElapsed(),
            ["TimeElapsedDisplay"] = TimeElapsedDisplay,
            ["IsRecentActivity"] = IsRecentActivity,

            // Summary Information
            ["ActivitySummary"] = ActivitySummary,
            ["AuditTrailSummary"] = GetAuditTrailSummary(),

            // Validation Information
            ["ValidationStatus"] = stats["ValidationStatus"],
            ["IsValid"] = stats["IsValid"],

            // Performance Information
            ["DTOType"] = "Minimal",
            ["MemoryFootprint"] = "Optimized"
        };
    }

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified DocumentActivityUserMinimalDto is equal to the current DocumentActivityUserMinimalDto.
    /// </summary>
    public bool Equals(DocumentActivityUserMinimalDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return DocumentId.Equals(other.DocumentId) &&
               DocumentActivity?.Id == other.DocumentActivity?.Id &&
               User?.Id == other.User?.Id &&
               CreatedAt.Equals(other.CreatedAt);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current DocumentActivityUserMinimalDto.
    /// </summary>
    public override bool Equals(object? obj) =>
        obj is DocumentActivityUserMinimalDto other && Equals(other);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode() =>
        HashCode.Combine(DocumentId, DocumentActivity?.Id, User?.Id, CreatedAt);

    #endregion Equality Implementation

    #region Comparison Implementation

    /// <summary>
    /// Compares the current DocumentActivityUserMinimalDto with another DocumentActivityUserMinimalDto for ordering purposes.
    /// </summary>
    public int CompareTo(DocumentActivityUserMinimalDto? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by timestamp for chronological ordering
        var timestampComparison = CreatedAt.CompareTo(other.CreatedAt);
        if (timestampComparison != 0) return timestampComparison;

        // Secondary sort by activity type for consistency
        var activityComparison = string.Compare(
            DocumentActivity?.Activity,
            other.DocumentActivity?.Activity,
            StringComparison.OrdinalIgnoreCase);
        if (activityComparison != 0) return activityComparison;

        // Tertiary sort by document ID for complete consistency
        var documentComparison = DocumentId.CompareTo(other.DocumentId);
        if (documentComparison != 0) return documentComparison;

        // Final sort by user ID for complete deterministic ordering
        return User?.Id.CompareTo(other.User?.Id) ?? 0;
    }

    #endregion Comparison Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the DocumentActivityUserMinimalDto.
    /// </summary>
    public override string ToString()
    {
        var activityType = DocumentActivity?.Activity ?? "UNKNOWN";
        var userName = User?.Name ?? "Unknown User";

        return $"Document Activity Audit: Document {DocumentId} {activityType} by {userName}";
    }

    #endregion String Representation
}