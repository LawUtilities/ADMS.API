using ADMS.API.Common;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Models;

/// <summary>
/// Minimal Data Transfer Object representing a document activity with essential information for audit trail and activity classification.
/// </summary>
/// <remarks>
/// This DTO serves as a lightweight representation of the document activity lookup entity 
/// <see cref="ADMS.API.Entities.DocumentActivity"/>, providing essential information for activity classification,
/// audit trail display, and workflow operations within the ADMS legal document management system.
/// It focuses on core activity identification while maintaining minimal memory footprint for performance-critical operations.
/// 
/// <para><strong>Enhanced with Standardized Validation (.NET 9):</strong></para>
/// <list type="bullet">
/// <item><strong>BaseValidationDto Integration:</strong> Inherits standardized ADMS validation patterns</item>
/// <item><strong>Minimal DTO Validation:</strong> Optimized validation for lightweight lookup operations</item>
/// <item><strong>Performance Optimized:</strong> Uses yield return for lazy validation evaluation</item>
/// <item><strong>Validation Hierarchy:</strong> Follows standardized core → business → cross-property → custom pattern</item>
/// <item><strong>Activity Classification:</strong> Enhanced activity validation using DocumentActivityValidationHelper</item>
/// </list>
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Minimal Entity Representation:</strong> Contains only essential properties from ADMS.API.Entities.DocumentActivity</item>
/// <item><strong>Standardized Validation:</strong> Uses BaseValidationDto for consistent validation patterns</item>
/// <item><strong>Activity Classification Focus:</strong> Optimized for activity type identification and classification</item>
/// <item><strong>Audit Trail Integration:</strong> Designed for audit log displays and activity tracking</item>
/// <item><strong>Professional Validation:</strong> Uses ADMS.API.Common.DocumentActivityValidationHelper for data integrity</item>
/// </list>
/// 
/// <para><strong>Validation Hierarchy:</strong></para>
/// Following BaseValidationDto standardized validation pattern:
/// <list type="number">
/// <item><strong>Core Properties:</strong> ID and Activity validation using ADMS helpers</item>
/// <item><strong>Business Rules:</strong> Activity normalization, professional standards</item>
/// <item><strong>Cross-Property:</strong> Activity classification consistency</item>
/// <item><strong>Custom Rules:</strong> Lookup-specific validation and seeded ID verification</item>
/// </list>
/// 
/// <para><strong>Standard Document Activities Supported:</strong></para>
/// Based on <see cref="ADMS.API.Entities.DocumentActivity"/> seeded data:
/// <list type="bullet">
/// <item><strong>CHECKED IN:</strong> Document checked into version control system (ID: 20000000-0000-0000-0000-000000000001)</item>
/// <item><strong>CHECKED OUT:</strong> Document checked out for editing (ID: 20000000-0000-0000-0000-000000000002)</item>
/// <item><strong>CREATED:</strong> Initial document creation (ID: 20000000-0000-0000-0000-000000000003)</item>
/// <item><strong>DELETED:</strong> Document marked for deletion (ID: 20000000-0000-0000-0000-000000000004)</item>
/// <item><strong>RESTORED:</strong> Deleted document restored to active status (ID: 20000000-0000-0000-0000-000000000005)</item>
/// <item><strong>SAVED:</strong> Document saved with changes (ID: 20000000-0000-0000-0000-000000000006)</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>API Responses:</strong> Lightweight activity information for client applications</item>
/// <item><strong>Audit Trail Display:</strong> Activity classification for user interfaces</item>
/// <item><strong>Dropdown Population:</strong> Activity selection lists for document operations</item>
/// <item><strong>Activity Filtering:</strong> Activity-based filtering and search operations</item>
/// <item><strong>Workflow Operations:</strong> Activity identification for business rule enforcement</item>
/// </list>
/// 
/// <para><strong>Performance Benefits with Standardized Validation:</strong></para>
/// <list type="bullet">
/// <item><strong>Early Termination:</strong> Validation stops on critical errors for better performance</item>
/// <item><strong>Lazy Evaluation:</strong> Activity properties validated only when needed</item>
/// <item><strong>Consistent Error Handling:</strong> Standardized error formatting and reporting</item>
/// <item><strong>Memory Efficient:</strong> Optimized validation memory usage for minimal DTOs</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating minimal document activity DTOs with standardized validation
/// var createdActivity = new DocumentActivityMinimalDto
/// {
///     Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
///     Activity = "CREATED"
/// };
/// 
/// // Standardized validation using BaseValidationDto
/// var validationResults = BaseValidationDto.ValidateModel(createdActivity);
/// if (BaseValidationDto.HasValidationErrors(validationResults))
/// {
///     var summary = BaseValidationDto.GetValidationSummary(validationResults);
///     _logger.LogWarning("Activity validation failed: {ValidationSummary}", summary);
/// }
/// 
/// // Professional activity analysis with validation
/// if (createdActivity.IsValid)
/// {
///     ProcessDocumentActivity(createdActivity);
/// }
/// </code>
/// </example>
public class DocumentActivityMinimalDto : BaseValidationDto, IEquatable<DocumentActivityMinimalDto>, IComparable<DocumentActivityMinimalDto>
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
    [StringLength(DocumentActivityValidationHelper.MaxActivityLength, MinimumLength = 2,
        ErrorMessage = "Activity classification must be between 2 and 50 characters for professional readability.")]
    public required string Activity { get; set; }

    #endregion Core Properties

    #region Computed Properties

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
        "CHECKED IN" or "CHECKED OUT" => "Version Control",
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
        string.Equals(Activity, "CHECKED IN", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Activity, "CHECKED OUT", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this activity represents a content modification operation.
    /// </summary>
    public bool IsContentModificationActivity =>
        string.Equals(Activity, "SAVED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this activity represents a state change operation.
    /// </summary>
    public bool IsStateChangeActivity =>
        string.Equals(Activity, "DELETED", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Activity, "RESTORED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a user-friendly display text for the activity.
    /// </summary>
    public string DisplayText => Activity switch
    {
        "CHECKED IN" => "Checked In",
        "CHECKED OUT" => "Checked Out",
        "CREATED" => "Created",
        "DELETED" => "Deleted",
        "RESTORED" => "Restored",
        "SAVED" => "Saved",
        _ => Activity?.Trim() ?? "Unknown Activity"
    };

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
            maxLength: DocumentActivityValidationHelper.MaxActivityLength))
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
    /// <item>Activity format compliance</item>
    /// <item>Professional standards compliance</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        switch (string.IsNullOrWhiteSpace(Activity))
        {
            // Business rule: Activity names should be normalized (uppercase, trimmed)
            case false:
            {
                var normalizedActivity = Activity.ToUpperInvariant().Trim();
                if (Activity != normalizedActivity)
                {
                    yield return CreateValidationResult(
                        "Activity classification should be normalized (uppercase, trimmed) for consistency.",
                        nameof(Activity));
                }

                // Business rule: Professional format validation
                if (Activity.Trim().Length != Activity.Length)
                {
                    yield return CreateValidationResult(
                        "Activity classification should not have leading or trailing whitespace for professional presentation.",
                        nameof(Activity));
                }

                // Check for invalid characters that could cause display issues
                if (Activity.Any(c => char.IsControl(c) && !char.IsWhiteSpace(c)))
                {
                    yield return CreateValidationResult(
                        "Activity classification cannot contain control characters for professional presentation.",
                        nameof(Activity));
                }

                // Business rule: Activity should not contain inappropriate patterns
                var inappropriatePatterns = new[] { "test", "temp", "debug", "sample" };
                foreach (var pattern in inappropriatePatterns)
                {
                    if (Activity.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return CreateValidationResult(
                            $"Activity classification contains '{pattern}' which may not be appropriate for professional use.",
                            nameof(Activity));
                    }
                }

                break;
            }
        }
    }

    /// <summary>
    /// Validates cross-property relationships and consistency for activity classification.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    /// <remarks>
    /// This method implements the third step of the BaseValidationDto validation hierarchy,
    /// validating relationships between activity properties and ensuring consistency.
    /// 
    /// <para><strong>Cross-Property Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Activity ID and classification consistency</item>
    /// <item>Seeded ID validation for standard activities</item>
    /// <item>Activity type categorization consistency</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // Cross-property rule: For standard activities, validate seeded ID consistency
        if (!string.IsNullOrWhiteSpace(Activity) && Id != Guid.Empty)
        {
            var expectedSeededId = GetSeededActivityId(Activity);
            if (expectedSeededId != Guid.Empty && expectedSeededId != Id)
            {
                yield return CreateValidationResult(
                    $"Activity '{Activity}' should use seeded ID {expectedSeededId} but has ID {Id}. " +
                    "Standard activities must use their designated seeded GUIDs.",
                    nameof(Id), nameof(Activity));
            }
        }

        switch (string.IsNullOrWhiteSpace(Activity))
        {
            // Cross-property rule: Activity classification and category consistency
            case false:
            {
                var computedCategory = ActivityCategory;
                if (computedCategory == "Unknown" && DocumentActivityValidationHelper.IsActivityAllowed(Activity))
                {
                    yield return CreateValidationResult(
                        $"Activity '{Activity}' is allowed but cannot be categorized. This may indicate a system configuration issue.",
                        nameof(Activity));
                }

                break;
            }
        }
    }

    /// <summary>
    /// Validates custom rules specific to document activity lookup data and seeded values.
    /// </summary>
    /// <returns>A collection of validation results for custom validation.</returns>
    /// <remarks>
    /// This method implements custom validation logic specific to document activity lookups,
    /// including seeded data validation and activity pattern analysis.
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCustomRules()
    {
        // Custom rule: Validate that this appears to be a standard seeded activity
        if (!string.IsNullOrWhiteSpace(Activity) && Id != Guid.Empty)
        {
            var isStandardActivity = GetSeededActivityId(Activity) == Id;
            if (!isStandardActivity)
            {
                // This is not necessarily an error, but worth noting
                yield return CreateValidationResult(
                    $"Activity '{Activity}' with ID {Id} does not match standard seeded activities. " +
                    "Ensure this is intentional for custom activity configurations.",
                    nameof(Id), nameof(Activity));
            }
        }

        switch (string.IsNullOrWhiteSpace(Activity))
        {
            // Custom rule: Activity naming pattern validation for professional standards
            case false:
            {
                // Check for overly long activity names that might indicate issues
                if (Activity.Split(' ').Length > 3)
                {
                    yield return CreateValidationResult(
                        "Activity classification should be concise (preferably 3 words or fewer) for professional presentation.",
                        nameof(Activity));
                }

                // Check for activities with numeric suffixes (might indicate versioning issues)
                if (System.Text.RegularExpressions.Regex.IsMatch(Activity, @"\d+$"))
                {
                    yield return CreateValidationResult(
                        "Activity classification should not end with numbers. Consider using descriptive names instead.",
                        nameof(Activity));
                }

                // Check for activities that might conflict with system operations
                var reservedPatterns = new[] { "SYSTEM", "INTERNAL", "AUTO", "BATCH" };
                foreach (var pattern in reservedPatterns)
                {
                    if (Activity.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return CreateValidationResult(
                            $"Activity classification contains reserved pattern '{pattern}'. This may conflict with system operations.",
                            nameof(Activity));
                    }
                }

                break;
            }
        }
    }

    #endregion Standardized Validation Implementation

    #region Static Methods

    /// <summary>
    /// Creates a DocumentActivityMinimalDto from an ADMS.API.Entities.DocumentActivity entity with standardized validation.
    /// </summary>
    /// <param name="entity">The DocumentActivity entity to convert. Cannot be null.</param>
    /// <returns>A valid DocumentActivityMinimalDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entity = await context.DocumentActivities
    ///     .FirstAsync(da => da.Activity == "CREATED");
    /// 
    /// var activityDto = DocumentActivityMinimalDto.FromEntity(entity);
    /// // DTO is guaranteed to be valid due to standardized validation
    /// </code>
    /// </example>
    public static DocumentActivityMinimalDto FromEntity([NotNull] Entities.DocumentActivity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var activityDto = new DocumentActivityMinimalDto
        {
            Id = entity.Id,
            Activity = entity.Activity
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(activityDto);
        if (!HasValidationErrors(validationResults)) return activityDto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Document activity validation failed: {summary}");
    }

    /// <summary>
    /// Creates multiple DocumentActivityMinimalDto instances from a collection of entities with standardized validation.
    /// </summary>
    /// <param name="entities">The collection of DocumentActivity entities to convert. Cannot be null.</param>
    /// <returns>A collection of valid DocumentActivityMinimalDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method uses standardized validation and provides detailed error handling
    /// for invalid entities.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entities = await context.DocumentActivities.ToListAsync();
    /// var activityDtos = DocumentActivityMinimalDto.FromEntities(entities);
    /// 
    /// // All DTOs are guaranteed to be valid
    /// foreach (var dto in activityDtos)
    /// {
    ///     ProcessDocumentActivity(dto);
    /// }
    /// </code>
    /// </example>
    public static IList<DocumentActivityMinimalDto> FromEntities([NotNull] IEnumerable<Entities.DocumentActivity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var result = new List<DocumentActivityMinimalDto>();
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
                errors.Add($"Activity {entity.Id} ({entity.Activity}): {ex.Message}");

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
    /// Creates a DocumentActivityMinimalDto for a specific activity type using seeded GUIDs with standardized validation.
    /// </summary>
    /// <param name="activityName">The standardized activity name.</param>
    /// <returns>A DocumentActivityMinimalDto with the appropriate seeded GUID.</returns>
    /// <exception cref="ArgumentException">Thrown when activity name is invalid or not allowed.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation
    /// and creates activities using seeded GUIDs for system consistency.
    /// </remarks>
    /// <example>
    /// <code>
    /// var createdActivity = DocumentActivityMinimalDto.CreateStandardActivity("CREATED");
    /// var savedActivity = DocumentActivityMinimalDto.CreateStandardActivity("SAVED");
    /// var checkedOutActivity = DocumentActivityMinimalDto.CreateStandardActivity("CHECKED OUT");
    /// 
    /// // All DTOs are guaranteed to be valid with correct seeded GUIDs
    /// Console.WriteLine($"Created activity ID: {createdActivity.Id}");
    /// // Output: Created activity ID: 20000000-0000-0000-0000-000000000003
    /// </code>
    /// </example>
    public static DocumentActivityMinimalDto CreateStandardActivity([NotNull] string activityName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activityName, nameof(activityName));

        var normalizedActivity = activityName.Trim().ToUpperInvariant();
        var activityId = GetSeededActivityId(normalizedActivity);

        if (activityId == Guid.Empty)
        {
            throw new ArgumentException(
                $"Activity '{activityName}' is not a standard seeded activity. " +
                $"Allowed activities: {DocumentActivityValidationHelper.AllowedActivitiesList}",
                nameof(activityName));
        }

        var activityDto = new DocumentActivityMinimalDto
        {
            Id = activityId,
            Activity = normalizedActivity
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(activityDto);
        if (!HasValidationErrors(validationResults)) return activityDto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Document activity validation failed: {summary}");
    }

    /// <summary>
    /// Gets all allowed document activities as DocumentActivityMinimalDto instances with standardized validation.
    /// </summary>
    /// <returns>A collection of DocumentActivityMinimalDto instances for all allowed activities.</returns>
    /// <remarks>
    /// This method uses the standardized BaseValidationDto.ValidateModel() to ensure all returned DTOs are valid.
    /// </remarks>
    /// <example>
    /// <code>
    /// var allowedActivities = DocumentActivityMinimalDto.GetAllowedActivities();
    /// foreach (var activity in allowedActivities)
    /// {
    ///     activityDropdown.Items.Add(new ListItem(activity.DisplayText, activity.Id.ToString()));
    /// }
    /// 
    /// // All activities are guaranteed to be valid
    /// Console.WriteLine($"Found {allowedActivities.Count} valid activities");
    /// </code>
    /// </example>
    public static IList<DocumentActivityMinimalDto> GetAllowedActivities()
    {
        var result = new List<DocumentActivityMinimalDto>();
        var errors = new List<string>();

        foreach (var activity in DocumentActivityValidationHelper.AllowedActivitiesList)
        {
            try
            {
                var dto = CreateStandardActivity(activity.ToString());
                result.Add(dto);
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                // Collect errors for comprehensive error reporting
                errors.Add($"Activity {activity}: {ex.Message}");

                // In production, use proper logging framework
                Console.WriteLine($"Warning: Skipped invalid allowed activity '{activity}': {ex.Message}");
            }
        }

        // Log summary if there were errors
        if (errors.Any())
        {
            Console.WriteLine($"Activity creation completed with {errors.Count} errors out of {DocumentActivityValidationHelper.AllowedActivities.Count} activities processed.");
        }

        return result.OrderBy(a => a.Activity).ToList();
    }

    /// <summary>
    /// Creates a custom DocumentActivityMinimalDto with specified values and standardized validation.
    /// </summary>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="activityName">The activity name.</param>
    /// <returns>A DocumentActivityMinimalDto with specified values.</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method allows creating custom activities while using standardized validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var customActivity = DocumentActivityMinimalDto.CreateCustomActivity(
    ///     Guid.NewGuid(),
    ///     "CUSTOM_REVIEW");
    /// 
    /// // DTO is guaranteed to be valid due to standardized validation
    /// </code>
    /// </example>
    public static DocumentActivityMinimalDto CreateCustomActivity(Guid activityId, [NotNull] string activityName)
    {
        if (activityId == Guid.Empty)
            throw new ArgumentException("Activity ID cannot be empty.", nameof(activityId));

        ArgumentException.ThrowIfNullOrWhiteSpace(activityName, nameof(activityName));

        var activityDto = new DocumentActivityMinimalDto
        {
            Id = activityId,
            Activity = activityName.Trim().ToUpperInvariant()
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(activityDto);
        if (!HasValidationErrors(validationResults)) return activityDto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Custom activity validation failed: {summary}");
    }

    #endregion Static Methods

    #region Helper Methods

    /// <summary>
    /// Gets the seeded GUID for a specific document activity name from ADMS.API.Entities.DocumentActivity.
    /// </summary>
    /// <param name="activityName">The activity name to get the GUID for.</param>
    /// <returns>The seeded GUID if found; otherwise, Guid.Empty.</returns>
    /// <remarks>
    /// This method returns the specific GUIDs used in database seeding for standard activities from
    /// ADMS.API.Entities.DocumentActivity, useful for business logic that needs to reference specific activity types.
    /// </remarks>
    /// <example>
    /// <code>
    /// var createdActivityId = DocumentActivityMinimalDto.GetSeededActivityId("CREATED");
    /// // Returns: 20000000-0000-0000-0000-000000000003
    /// 
    /// var checkedInActivityId = DocumentActivityMinimalDto.GetSeededActivityId("CHECKED IN");
    /// // Returns: 20000000-0000-0000-0000-000000000001
    /// </code>
    /// </example>
    public static Guid GetSeededActivityId(string activityName)
    {
        if (string.IsNullOrWhiteSpace(activityName))
            return Guid.Empty;

        return activityName.Trim().ToUpperInvariant() switch
        {
            "CHECKED IN" => Guid.Parse("20000000-0000-0000-0000-000000000001"),
            "CHECKED OUT" => Guid.Parse("20000000-0000-0000-0000-000000000002"),
            "CREATED" => Guid.Parse("20000000-0000-0000-0000-000000000003"),
            "DELETED" => Guid.Parse("20000000-0000-0000-0000-000000000004"),
            "RESTORED" => Guid.Parse("20000000-0000-0000-0000-000000000005"),
            "SAVED" => Guid.Parse("20000000-0000-0000-0000-000000000006"),
            _ => Guid.Empty
        };
    }

    /// <summary>
    /// Gets a comprehensive summary of the activity for reporting purposes.
    /// </summary>
    /// <returns>A formatted string containing activity summary information.</returns>
    /// <remarks>
    /// This method provides a professional summary of the activity suitable for
    /// reports, logs, and user interface displays.
    /// </remarks>
    /// <example>
    /// <code>
    /// var summary = activity.GetActivitySummary();
    /// Console.WriteLine(summary);
    /// // Output: "Document Activity: Created (Lifecycle) - ID: 20000000-0000-0000-0000-000000000003"
    /// </code>
    /// </example>
    public string GetActivitySummary()
    {
        return $"Document Activity: {DisplayText} ({ActivityCategory}) - ID: {Id}";
    }

    /// <summary>
    /// Determines whether this activity matches the specified activity name.
    /// </summary>
    /// <param name="activityName">The activity name to compare (case-insensitive).</param>
    /// <returns>true if the activities match; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a safe way to compare activity names with proper null handling
    /// and case-insensitive comparison for business logic operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (activity.MatchesActivity("CREATED"))
    /// {
    ///     Console.WriteLine("This is a creation activity");
    /// }
    /// 
    /// // Case-insensitive matching
    /// bool matches = activity.MatchesActivity("created"); // Still returns true for CREATED activity
    /// </code>
    /// </example>
    public bool MatchesActivity(string? activityName)
    {
        if (string.IsNullOrWhiteSpace(activityName) || string.IsNullOrWhiteSpace(Activity))
            return false;

        return string.Equals(Activity.Trim(), activityName.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the opposite activity for certain activity pairs.
    /// </summary>
    /// <returns>The opposite activity name if applicable; otherwise, null.</returns>
    /// <remarks>
    /// This method provides the logical opposite for paired activities like check-in/check-out
    /// and delete/restore, useful for workflow operations and business logic.
    /// </remarks>
    /// <example>
    /// <code>
    /// var checkedOutActivity = new DocumentActivityMinimalDto { Activity = "CHECKED OUT" };
    /// var oppositeActivity = checkedOutActivity.GetOppositeActivity();
    /// // Returns: "CHECKED IN"
    /// 
    /// var deletedActivity = new DocumentActivityMinimalDto { Activity = "DELETED" };
    /// var restoreActivity = deletedActivity.GetOppositeActivity();
    /// // Returns: "RESTORED"
    /// </code>
    /// </example>
    public string? GetOppositeActivity() => Activity?.ToUpperInvariant() switch
    {
        "CHECKED IN" => "CHECKED OUT",
        "CHECKED OUT" => "CHECKED IN",
        "DELETED" => "RESTORED",
        "RESTORED" => "DELETED",
        _ => null
    };

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
        // Perform validation to get current status
        var validationResults = ValidateModel(this);
        var validationStatus = HasValidationErrors(validationResults)
            ? GetValidationSummary(validationResults)
            : "Valid";

        return new Dictionary<string, object>
        {
            // Core Information
            [nameof(Id)] = Id,
            [nameof(Activity)] = Activity,
            [nameof(NormalizedActivity)] = NormalizedActivity,
            [nameof(DisplayText)] = DisplayText,

            // Classification Information
            [nameof(ActivityCategory)] = ActivityCategory,
            [nameof(IsCreationActivity)] = IsCreationActivity,
            [nameof(IsVersionControlActivity)] = IsVersionControlActivity,
            [nameof(IsContentModificationActivity)] = IsContentModificationActivity,
            [nameof(IsStateChangeActivity)] = IsStateChangeActivity,

            // Validation Information
            ["ValidationStatus"] = validationStatus,
            ["IsValid"] = !HasValidationErrors(validationResults),
            ["IsStandardSeededActivity"] = GetSeededActivityId(Activity) == Id,

            // Additional Information
            ["OppositeActivity"] = GetOppositeActivity() ?? "None",
            ["ActivitySummary"] = GetActivitySummary()
        };
    }

    #endregion Helper Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified DocumentActivityMinimalDto is equal to the current DocumentActivityMinimalDto.
    /// </summary>
    public bool Equals(DocumentActivityMinimalDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current DocumentActivityMinimalDto.
    /// </summary>
    public override bool Equals(object? obj) =>
        obj is DocumentActivityMinimalDto other && Equals(other);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode() => Id.GetHashCode();

    #endregion Equality Implementation

    #region Comparison Implementation

    /// <summary>
    /// Compares the current DocumentActivityMinimalDto with another DocumentActivityMinimalDto for ordering purposes.
    /// </summary>
    public int CompareTo(DocumentActivityMinimalDto? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by activity name for alphabetical ordering
        var activityComparison = string.Compare(Activity, other.Activity, StringComparison.OrdinalIgnoreCase);
        return activityComparison != 0 ? activityComparison :
            // Secondary sort by ID for consistency when activities have same name
            Id.CompareTo(other.Id);
    }

    #endregion Comparison Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the DocumentActivityMinimalDto.
    /// </summary>
    public override string ToString()
    {
        return $"Document Activity: '{Activity}' ({ActivityCategory})";
    }

    #endregion String Representation
}