using ADMS.API.Common;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Models;

/// <summary>
/// Comprehensive Data Transfer Object representing a document activity with associated user audit trails.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of a document activity within the ADMS legal document management system,
/// corresponding to <see cref="ADMS.API.Entities.DocumentActivity"/>. It provides comprehensive activity information
/// including all user associations for complete audit trail tracking and professional document management compliance.
/// 
/// <para><strong>Enhanced with Standardized Validation (.NET 9):</strong></para>
/// <list type="bullet">
/// <item><strong>BaseValidationDto Integration:</strong> Inherits standardized ADMS validation patterns</item>
/// <item><strong>Activity-Specific Validation:</strong> Comprehensive document activity validation patterns</item>
/// <item><strong>Performance Optimized:</strong> Uses yield return for lazy validation evaluation</item>
/// <item><strong>Validation Hierarchy:</strong> Follows standardized core → business → cross-property → collections pattern</item>
/// <item><strong>Collection Validation:</strong> Advanced collection validation for user associations</item>
/// </list>
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Complete Entity Representation:</strong> Mirrors all properties and relationships from ADMS.API.Entities.DocumentActivity</item>
/// <item><strong>Standardized Validation:</strong> Uses BaseValidationDto for consistent validation patterns</item>
/// <item><strong>Activity Classification:</strong> Provides standardized document operation classification for audit trails</item>
/// <item><strong>Professional Validation:</strong> Uses ADMS.API.Common.DocumentActivityValidationHelper for comprehensive data integrity</item>
/// <item><strong>User Association Integration:</strong> Includes complete user audit trail collections for accountability</item>
/// </list>
/// 
/// <para><strong>Validation Hierarchy:</strong></para>
/// Following BaseValidationDto standardized validation pattern:
/// <list type="number">
/// <item><strong>Core Properties:</strong> ID and Activity validation using ADMS helpers</item>
/// <item><strong>Business Rules:</strong> Activity-specific business rules and professional standards</item>
/// <item><strong>Cross-Property:</strong> Activity and collection consistency validation</item>
/// <item><strong>Collections:</strong> DocumentActivityUsers collection with comprehensive validation</item>
/// </list>
/// 
/// <para><strong>Standard Document Activities:</strong></para>
/// This DTO represents the standardized document activities seeded in ADMS.API.Entities.DocumentActivity:
/// <list type="bullet">
/// <item><strong>CREATED:</strong> Document creation operations for new document establishment</item>
/// <item><strong>SAVED:</strong> Document save operations for content preservation</item>
/// <item><strong>DELETED:</strong> Document deletion operations (soft deletion for audit preservation)</item>
/// <item><strong>RESTORED:</strong> Document restoration operations from deleted state</item>
/// <item><strong>CHECKED_IN:</strong> Document check-in operations for version control</item>
/// <item><strong>CHECKED_OUT:</strong> Document check-out operations for exclusive editing</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>API Responses:</strong> Complete activity data including user associations for client applications</item>
/// <item><strong>Activity Management:</strong> Activity configuration and administration operations</item>
/// <item><strong>Audit Trail Generation:</strong> Complete activity information for audit trail creation</item>
/// <item><strong>Reporting and Analytics:</strong> Activity-based reporting and usage analysis</item>
/// <item><strong>Business Rule Enforcement:</strong> Activity validation and business rule compliance</item>
/// </list>
/// 
/// <para><strong>Performance Benefits with Standardized Validation:</strong></para>
/// <list type="bullet">
/// <item><strong>Early Termination:</strong> Validation stops on critical errors for better performance</item>
/// <item><strong>Lazy Evaluation:</strong> Collections validated only when needed</item>
/// <item><strong>Consistent Error Handling:</strong> Standardized error formatting and reporting</item>
/// <item><strong>Memory Efficient:</strong> Optimized validation memory usage</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive document activity DTO with standardized validation
/// var activityDto = new DocumentActivityDto
/// {
///     Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
///     Activity = "CREATED",
///     DocumentActivityUsers = new List<DocumentActivityUserDto>
///     {
///         new DocumentActivityUserDto
///         {
///             DocumentId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
///             DocumentActivityId = Guid.Parse("20000000-0000-0000-0000-000000000003"),
///             UserId = Guid.Parse("50000000-0000-0000-0000-000000000001"),
///             CreatedAt = DateTime.UtcNow
///         }
///     }
/// };
/// 
/// // Standardized validation using BaseValidationDto
/// var validationResults = BaseValidationDto.ValidateModel(activityDto);
/// if (BaseValidationDto.HasValidationErrors(validationResults))
/// {
///     var summary = BaseValidationDto.GetValidationSummary(validationResults);
///     _logger.LogWarning("Document activity validation failed: {ValidationSummary}", summary);
/// }
/// 
/// // Professional activity analysis with validation
/// if (activityDto.IsValid)
/// {
///     ProcessDocumentActivity(activityDto);
/// }
/// </code>
/// </example>
public class DocumentActivityDto : BaseValidationDto
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the document activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and uniquely identifies the document activity within
    /// the ADMS legal document management system.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateGuid() with allowEmpty=false.
    /// </remarks>
    [Required(ErrorMessage = "Document activity ID is required for activity identification.")]
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the standardized activity classification for document operations.
    /// </summary>
    /// <remarks>
    /// This property defines the type of document operation being represented and must conform to the
    /// standardized activity classifications defined in <see cref="Common.DocumentActivityValidationHelper"/>.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using DocumentActivityValidationHelper for comprehensive validation.
    /// </remarks>
    [Required(ErrorMessage = "Activity classification is required for document operation identification.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Activity classification must be between 2 and 50 characters for professional readability.")]
    public required string Activity { get; set; }

    /// <summary>
    /// Gets or sets the collection of user associations for this document activity.
    /// </summary>
    /// <remarks>
    /// This collection represents the relationship between this activity and the users who have performed it.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCollections() using DtoValidationHelper for comprehensive collection validation.
    /// </remarks>
    public IReadOnlyCollection<DocumentActivityUserDto> DocumentActivityUsers { get; set; } = [];

    #endregion Core Properties

    #region Computed Properties

    /// <summary>
    /// Gets the total count of user associations for this activity.
    /// </summary>
    public int UsageCount => DocumentActivityUsers.Count;

    /// <summary>
    /// Gets the count of unique users who have performed this activity.
    /// </summary>
    public int UniqueUserCount => DocumentActivityUsers
        .Select(u => u.UserId)
        .Distinct()
        .Count();

    /// <summary>
    /// Gets the count of unique documents this activity has been performed on.
    /// </summary>
    public int UniqueDocumentCount => DocumentActivityUsers
        .Select(u => u.DocumentId)
        .Distinct()
        .Count();

    /// <summary>
    /// Gets a value indicating whether this activity has any recorded user associations.
    /// </summary>
    public bool HasUserAssociations => DocumentActivityUsers.Any();

    /// <summary>
    /// Gets the normalized version of the activity name for consistent comparison.
    /// </summary>
    public string NormalizedActivity => Activity?.ToUpperInvariant().Trim() ?? string.Empty;

    /// <summary>
    /// Gets the activity category for classification purposes.
    /// </summary>
    public string ActivityCategory => Activity?.ToUpperInvariant() switch
    {
        "CREATED" or "DELETED" or "RESTORED" => "Lifecycle",
        "CHECKED_IN" or "CHECKED_OUT" => "Version Control",
        "SAVED" => "Content Management",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets a value indicating whether this activity represents a creation operation.
    /// </summary>
    public bool IsCreationActivity =>
        string.Equals(Activity, "CREATED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this activity represents a version control operation.
    /// </summary>
    public bool IsVersionControlActivity =>
        string.Equals(Activity, "CHECKED_IN", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Activity, "CHECKED_OUT", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this activity DTO is valid for system operations.
    /// </summary>
    public bool IsValid =>
        Id != Guid.Empty &&
        !string.IsNullOrWhiteSpace(Activity) &&
        DocumentActivityValidationHelper.IsActivityAllowed(Activity);

    #endregion Computed Properties

    #region Standardized Validation Implementation

    /// <summary>
    /// Validates core properties such as ID and Activity using ADMS validation helpers.
    /// </summary>
    /// <returns>A collection of validation results for core property validation.</returns>
    /// <remarks>
    /// This method implements the first step of the BaseValidationDto validation hierarchy,
    /// validating essential activity properties using standardized ADMS validation helpers.
    /// 
    /// <para><strong>Core Property Validation Steps:</strong></para>
    /// <list type="number">
    /// <item>Activity ID validation using BaseValidationDto.ValidateGuid() (does not allow empty)</item>
    /// <item>Activity classification validation using DocumentActivityValidationHelper</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCoreProperties()
    {
        // Validate activity ID using standardized GUID validation (do not allow empty)
        foreach (var result in ValidateGuid(Id, nameof(Id), allowEmpty: false))
            yield return result;

        // Validate activity classification using standardized string validation
        foreach (var result in ValidateString(
            Activity,
            nameof(Activity),
            required: true,
            minLength: 2,
            maxLength: 50))
        {
            yield return result;
        }

        // Additional activity validation using DocumentActivityValidationHelper
        if (string.IsNullOrWhiteSpace(Activity)) yield break;
        if (!DocumentActivityValidationHelper.IsActivityAllowed(Activity))
        {
            yield return CreateValidationResult(
                $"Activity '{Activity}' is not allowed. Allowed activities: {DocumentActivityValidationHelper.AllowedActivitiesList}",
                nameof(Activity));
        }

        if (!Activity.Any(char.IsLetter))
        {
            yield return CreateValidationResult(
                "Activity classification must contain at least one letter for professional readability.",
                nameof(Activity));
        }
    }

    /// <summary>
    /// Validates business rules specific to document activities and professional standards.
    /// </summary>
    /// <returns>A collection of validation results for business rule validation.</returns>
    /// <remarks>
    /// This method implements the second step of the BaseValidationDto validation hierarchy,
    /// validating domain-specific business rules for document activities.
    /// 
    /// <para><strong>Business Rules Validated:</strong></para>
    /// <list type="bullet">
    /// <item>Activity normalization and consistency</item>
    /// <item>Professional naming standards</item>
    /// <item>Version control activity business rules</item>
    /// <item>Professional standards compliance</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Business rule: Activity names should be normalized (uppercase, trimmed)
        if (!string.IsNullOrWhiteSpace(Activity))
        {
            var normalizedActivity = Activity.ToUpperInvariant().Trim();
            if (Activity != normalizedActivity)
            {
                yield return CreateValidationResult(
                    "Activity classification should be normalized (uppercase, trimmed) for consistency.",
                    nameof(Activity));
            }
        }

        // Business rule: Version control activities should follow professional patterns
        if (IsVersionControlActivity && HasUserAssociations)
        {
            var hasMultipleUsersForSameDocument = DocumentActivityUsers
                .GroupBy(u => u.DocumentId)
                .Any(g => g.Count() > 1);

            if (hasMultipleUsersForSameDocument && Activity?.ToUpperInvariant() == "CHECKED_OUT")
            {
                yield return CreateValidationResult(
                    "A document cannot be checked out by multiple users simultaneously.",
                    nameof(DocumentActivityUsers));
            }
        }

        // Business rule: Creation activities should be unique per document
        if (IsCreationActivity && HasUserAssociations)
        {
            var multipleCreationsForSameDocument = DocumentActivityUsers
                .GroupBy(u => u.DocumentId)
                .Any(g => g.Count() > 1);

            if (multipleCreationsForSameDocument)
            {
                yield return CreateValidationResult(
                    "A document should only have one creation activity per document.",
                    nameof(DocumentActivityUsers));
            }
        }

        // Business rule: Professional activity standards
        if (!string.IsNullOrWhiteSpace(Activity) && Activity.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
        {
            // Check for invalid characters in activity names
            yield return CreateValidationResult(
                "Activity classification should contain only letters, digits, and underscores for professional standards.",
                nameof(Activity));
        }
    }

    /// <summary>
    /// Validates cross-property relationships and consistency including activity and collection relationships.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    /// <remarks>
    /// This method implements the third step of the BaseValidationDto validation hierarchy,
    /// validating relationships between activity properties and user association collections.
    /// 
    /// <para><strong>Cross-Property Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Activity ID and user association consistency</item>
    /// <item>Collection reference consistency</item>
    /// <item>Activity usage pattern validation</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // Validate cross-references between activity and user association entries
        if (HasUserAssociations)
        {
            var invalidAssociations = DocumentActivityUsers
                .Where(a => a?.DocumentActivityId != Id)
                .ToList();

            if (invalidAssociations.Any())
            {
                yield return CreateValidationResult(
                    "All user associations must reference this activity ID for audit trail consistency.",
                    nameof(DocumentActivityUsers), nameof(Id));
            }
        }

        // Validate activity usage patterns for consistency
        if (Id != Guid.Empty && !string.IsNullOrWhiteSpace(Activity) && HasUserAssociations && UsageCount == 0)
        {
            // Ensure the activity has meaningful usage if it has associations
            yield return CreateValidationResult(
                "Activity has user associations but reports zero usage count. This indicates a data consistency issue.",
                nameof(DocumentActivityUsers), nameof(UsageCount));
        }
    }

    /// <summary>
    /// Validates collections including user associations using DtoValidationHelper.
    /// </summary>
    /// <returns>A collection of validation results for collection validation.</returns>
    /// <remarks>
    /// This method implements the fourth step of the BaseValidationDto validation hierarchy,
    /// validating the DocumentActivityUsers collection using standardized patterns.
    /// 
    /// <para><strong>Collections Validated:</strong></para>
    /// <list type="bullet">
    /// <item>DocumentActivityUsers collection with deep validation of each association</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCollections()
    {
        // Convert IReadOnlyCollection to ICollection for compatibility
        var collection = DocumentActivityUsers as ICollection<DocumentActivityUserDto>
                         ?? DocumentActivityUsers.ToList();

        return DtoValidationHelper.ValidateCollection<DocumentActivityUserDto>(
            collection,
            nameof(DocumentActivityUsers),
            ValidationContext!,
            allowEmptyCollection: true);
    }

    /// <summary>
    /// Validates custom rules specific to document activities and audit trail integrity.
    /// </summary>
    /// <returns>A collection of validation results for custom validation.</returns>
    /// <remarks>
    /// This method implements custom validation logic specific to document activities,
    /// including audit trail integrity and professional standards.
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCustomRules()
    {
        // Custom rule: Activity should have reasonable usage for established activities
        if (HasUserAssociations && UsageCount > 10000)
        {
            yield return CreateValidationResult(
                "Activity has unusually high usage count. Consider analyzing for potential data issues or abuse.",
                nameof(DocumentActivityUsers));
        }

        // Custom rule: Version control activities should have balanced check-in/check-out patterns
        if (IsVersionControlActivity && HasUserAssociations)
        {
            var checkOutCount = DocumentActivityUsers.Count(u =>
                u.DocumentActivity?.Activity?.ToUpperInvariant() == "CHECKED_OUT");
            var checkInCount = DocumentActivityUsers.Count(u =>
                u.DocumentActivity?.Activity?.ToUpperInvariant() == "CHECKED_IN");

            if (Activity?.ToUpperInvariant() == "CHECKED_OUT" && checkOutCount > checkInCount + 10)
            {
                yield return CreateValidationResult(
                    "Significantly more check-outs than check-ins detected. Consider reviewing document custody patterns.",
                    nameof(DocumentActivityUsers));
            }
        }

        // Custom rule: Creation activities should have temporal consistency
        if (!IsCreationActivity || !HasUserAssociations) yield break;
        var futureCreations = DocumentActivityUsers.Count(u => u.CreatedAt > DateTime.UtcNow.AddMinutes(5));
        if (futureCreations > 0)
        {
            yield return CreateValidationResult(
                $"{futureCreations} creation activities have future timestamps. Verify system clock accuracy.",
                nameof(DocumentActivityUsers));
        }
    }

    #endregion Standardized Validation Implementation

    #region Static Methods

    /// <summary>
    /// Creates a DocumentActivityDto from an ADMS.API.Entities.DocumentActivity entity with standardized validation.
    /// </summary>
    /// <param name="entity">The DocumentActivity entity to convert. Cannot be null.</param>
    /// <param name="includeUserAssociations">Whether to include user association collections in the conversion.</param>
    /// <returns>A valid DocumentActivityDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entity = await context.DocumentActivities
    ///     .Include(da => da.DocumentActivityUsers)
    ///         .ThenInclude(dau => dau.Document)
    ///     .Include(da => da.DocumentActivityUsers)
    ///         .ThenInclude(dau => dau.User)
    ///     .FirstAsync(da => da.Id == activityId);
    /// 
    /// var activityDto = DocumentActivityDto.FromEntity(entity, includeUserAssociations: true);
    /// // DTO is guaranteed to be valid due to standardized validation
    /// </code>
    /// </example>
    public static DocumentActivityDto FromEntity(
        [NotNull] Entities.DocumentActivity entity,
        bool includeUserAssociations = true)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var activityDto = new DocumentActivityDto
        {
            Id = entity.Id,
            Activity = entity.Activity,
            DocumentActivityUsers = includeUserAssociations
                ? entity.DocumentActivityUsers?.Select(DocumentActivityUserDto.FromEntity).ToArray() ?? []
                : []
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(activityDto);
        if (!HasValidationErrors(validationResults)) return activityDto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Document activity validation failed: {summary}");
    }

    /// <summary>
    /// Creates multiple DocumentActivityDto instances from a collection of entities with standardized validation.
    /// </summary>
    /// <param name="entities">The collection of DocumentActivity entities to convert. Cannot be null.</param>
    /// <param name="includeUserAssociations">Whether to include user association collections in the conversion.</param>
    /// <returns>A collection of valid DocumentActivityDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method uses standardized validation and provides detailed error handling
    /// for invalid entities.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entities = await context.DocumentActivities
    ///     .Include(da => da.DocumentActivityUsers)
    ///     .ToListAsync();
    /// 
    /// var activityDtos = DocumentActivityDto.FromEntities(entities, includeUserAssociations: true);
    /// // All DTOs are guaranteed to be valid
    /// </code>
    /// </example>
    public static IList<DocumentActivityDto> FromEntities(
        [NotNull] IEnumerable<Entities.DocumentActivity> entities,
        bool includeUserAssociations = true)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var result = new List<DocumentActivityDto>();
        var errors = new List<string>();

        foreach (var entity in entities)
        {
            try
            {
                var dto = FromEntity(entity, includeUserAssociations);
                result.Add(dto);
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                // Collect errors for comprehensive error reporting
                errors.Add($"Activity {entity.Id}: {ex.Message}");

                // In production, use proper logging framework
                Console.WriteLine($"Warning: Skipped invalid activity entity {entity.Id}: {ex.Message}");
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
    /// Creates a DocumentActivityDto with only core activity information and standardized validation.
    /// </summary>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="activityName">The activity name.</param>
    /// <returns>A DocumentActivityDto with only core activity information.</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var coreActivity = DocumentActivityDto.CreateCoreActivity(
    ///     Guid.Parse("20000000-0000-0000-0000-000000000003"),
    ///     "CREATED");
    /// 
    /// // DTO is guaranteed to be valid due to standardized validation
    /// Console.WriteLine($"Created core activity: {coreActivity.Activity}");
    /// </code>
    /// </example>
    public static DocumentActivityDto CreateCoreActivity(Guid activityId, string activityName)
    {
        if (activityId == Guid.Empty)
            throw new ArgumentException("Activity ID cannot be empty.", nameof(activityId));

        if (string.IsNullOrWhiteSpace(activityName))
            throw new ArgumentException("Activity name cannot be null or empty.", nameof(activityName));

        var activityDto = new DocumentActivityDto
        {
            Id = activityId,
            Activity = activityName.Trim().ToUpperInvariant(),
            DocumentActivityUsers = []
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(activityDto);
        if (!HasValidationErrors(validationResults)) return activityDto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Document activity validation failed: {summary}");
    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Gets all unique users who have performed this activity.
    /// </summary>
    /// <returns>A collection of unique users associated with this activity.</returns>
    public IEnumerable<UserDto> GetAssociatedUsers()
    {
        return DocumentActivityUsers
            .Where(u => u.User != null)
            .Select(u => u.User!)
            .DistinctBy(u => u.Id)
            .OrderBy(u => u.Name);
    }

    /// <summary>
    /// Gets all unique documents this activity has been performed on.
    /// </summary>
    /// <returns>A collection of unique documents associated with this activity.</returns>
    public IEnumerable<DocumentWithoutRevisionsDto> GetAssociatedDocuments()
    {
        return DocumentActivityUsers
            .Where(u => u.Document != null)
            .Select(u => u.Document!)
            .DistinctBy(d => d.Id)
            .OrderBy(d => d.FileName);
    }

    /// <summary>
    /// Gets user associations filtered by document ID.
    /// </summary>
    /// <param name="documentId">The document ID to filter by.</param>
    /// <returns>A collection of user associations for the specified document.</returns>
    public IEnumerable<DocumentActivityUserDto> GetAssociationsForDocument(Guid documentId)
    {
        return DocumentActivityUsers
            .Where(u => u.DocumentId == documentId)
            .OrderBy(u => u.CreatedAt);
    }

    /// <summary>
    /// Gets user associations filtered by user ID.
    /// </summary>
    /// <param name="userId">The user ID to filter by.</param>
    /// <returns>A collection of user associations for the specified user.</returns>
    public IEnumerable<DocumentActivityUserDto> GetAssociationsForUser(Guid userId)
    {
        return DocumentActivityUsers
            .Where(u => u.UserId == userId)
            .OrderBy(u => u.CreatedAt);
    }

    /// <summary>
    /// Gets usage statistics for this activity with validation analysis.
    /// </summary>
    /// <returns>A dictionary containing comprehensive usage statistics and validation status.</returns>
    /// <remarks>
    /// This method provides comprehensive insights including validation status for enhanced analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = activity.GetUsageStatistics();
    /// Console.WriteLine($"Total usage: {stats["TotalUsage"]}");
    /// Console.WriteLine($"Validation status: {stats["ValidationStatus"]}");
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetUsageStatistics()
    {
        // Perform validation to get current status
        var validationResults = ValidateModel(this);
        var validationStatus = HasValidationErrors(validationResults)
            ? GetValidationSummary(validationResults)
            : "Valid";

        return new Dictionary<string, object>
        {
            ["ActivityName"] = Activity,
            ["ActivityId"] = Id,
            ["TotalUsage"] = UsageCount,
            ["UniqueUsers"] = UniqueUserCount,
            ["UniqueDocuments"] = UniqueDocumentCount,
            ["Category"] = ActivityCategory,
            ["IsCreationActivity"] = IsCreationActivity,
            ["IsVersionControlActivity"] = IsVersionControlActivity,
            ["HasUserAssociations"] = HasUserAssociations,
            ["ValidationStatus"] = validationStatus,
            ["IsValid"] = !HasValidationErrors(validationResults)
        };
    }

    /// <summary>
    /// Determines whether this activity has been performed on the specified document.
    /// </summary>
    /// <param name="documentId">The document ID to check.</param>
    /// <returns>true if this activity has been performed on the specified document; otherwise, false.</returns>
    public bool HasBeenPerformedOnDocument(Guid documentId) =>
        DocumentActivityUsers.Any(u => u.DocumentId == documentId);

    /// <summary>
    /// Determines whether this activity has been performed by the specified user.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <returns>true if this activity has been performed by the specified user; otherwise, false.</returns>
    public bool HasBeenPerformedByUser(Guid userId) =>
        DocumentActivityUsers.Any(u => u.UserId == userId);

    /// <summary>
    /// Gets comprehensive activity information including validation analysis.
    /// </summary>
    /// <returns>A dictionary containing detailed activity information and validation status.</returns>
    /// <remarks>
    /// This method provides structured activity information including validation status,
    /// useful for debugging, reporting, and administrative operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var info = activity.GetActivityInformation();
    /// foreach (var (key, value) in info)
    /// {
    ///     Console.WriteLine($"{key}: {value}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetActivityInformation()
    {
        // Get usage statistics and validation status
        var stats = GetUsageStatistics();

        return new Dictionary<string, object>
        {
            // Activity Information
            [nameof(Id)] = Id,
            [nameof(Activity)] = Activity,
            [nameof(NormalizedActivity)] = NormalizedActivity,
            [nameof(ActivityCategory)] = ActivityCategory,
            [nameof(IsCreationActivity)] = IsCreationActivity,
            [nameof(IsVersionControlActivity)] = IsVersionControlActivity,

            // Usage Information
            [nameof(UsageCount)] = UsageCount,
            [nameof(UniqueUserCount)] = UniqueUserCount,
            [nameof(UniqueDocumentCount)] = UniqueDocumentCount,
            [nameof(HasUserAssociations)] = HasUserAssociations,

            // Validation Information
            ["ValidationStatus"] = stats["ValidationStatus"],
            ["IsValid"] = stats["IsValid"]
        };
    }

    #endregion Business Logic Methods

    #region String Representation

    /// <summary>
    /// Returns a string representation of the DocumentActivityDto.
    /// </summary>
    public override string ToString()
    {
        var usageInfo = HasUserAssociations
            ? $" with {UsageCount} user associations ({UniqueUserCount} unique users, {UniqueDocumentCount} unique documents)"
            : " with no user associations";

        return $"Document Activity: '{Activity}'{usageInfo}";
    }

    #endregion String Representation

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified DocumentActivityDto is equal to the current DocumentActivityDto.
    /// </summary>
    public bool Equals(DocumentActivityDto? other)
    {
        if (other is null) return false;
        return ReferenceEquals(this, other) || Id.Equals(other.Id);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current DocumentActivityDto.
    /// </summary>
    public override bool Equals(object? obj) =>
        obj is DocumentActivityDto other && Equals(other);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality Implementation
}