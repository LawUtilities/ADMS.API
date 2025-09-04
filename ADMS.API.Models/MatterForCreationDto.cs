using ADMS.API.Common;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Models;

/// <summary>
/// Data Transfer Object for creating new matters with comprehensive validation and business rule enforcement.
/// </summary>
/// <remarks>
/// This DTO serves as the contract for creating new matters within the ADMS legal document management system,
/// providing only the properties that should be specified during matter creation while enforcing data integrity
/// and business rule compliance. It mirrors creatable properties from <see cref="ADMS.API.Entities.Matter"/> while
/// enforcing professional standards and validation rules appropriate for matter creation operations.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Creation-Focused Design:</strong> Contains only properties that should be specified when creating new matters</item>
/// <item><strong>Professional Validation:</strong> Uses ADMS.API.Common.MatterValidationHelper for comprehensive data integrity</item>
/// <item><strong>Business Rule Enforcement:</strong> Enforces matter creation-specific business rules and constraints</item>
/// <item><strong>Audit Trail Ready:</strong> Designed to work with matter activity tracking and audit systems</item>
/// <item><strong>Immutable Design:</strong> Record type with init-only properties for thread-safe operations</item>
/// </list>
/// 
/// <para><strong>Creation Properties:</strong></para>
/// This DTO includes properties from ADMS.API.Entities.Matter that are appropriate for creation:
/// <list type="bullet">
/// <item><strong>Description:</strong> Required matter description with uniqueness validation</item>
/// <item><strong>IsArchived:</strong> Optional initial archival status (defaults to false for new matters)</item>
/// <item><strong>CreationDate:</strong> Required creation timestamp (defaults to current UTC time)</item>
/// </list>
/// 
/// <para><strong>System-Managed Properties:</strong></para>
/// Properties excluded because they are managed by the system during creation:
/// <list type="bullet">
/// <item><strong>Id:</strong> Generated automatically by the database during creation</item>
/// <item><strong>IsDeleted:</strong> Always false for new matters (deletion occurs after creation)</item>
/// <item><strong>Document Collections:</strong> Empty for new matters, populated through separate operations</item>
/// <item><strong>Activity Collections:</strong> Populated automatically through activity tracking systems</item>
/// </list>
/// 
/// <para><strong>Usage Scenarios:</strong></para>
/// <list type="bullet">
/// <item><strong>New Matter Creation:</strong> Primary use case for creating new legal matters in the system</item>
/// <item><strong>Client Intake:</strong> Creating matters during client intake and onboarding processes</item>
/// <item><strong>Matter Setup:</strong> Initial matter setup for new cases, projects, or client relationships</item>
/// <item><strong>Batch Creation:</strong> Bulk matter creation with consistent validation and business rules</item>
/// <item><strong>API Operations:</strong> REST API endpoints for matter creation operations</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter Initialization:</strong> Proper matter setup following legal practice standards</item>
/// <item><strong>Professional Standards:</strong> Maintains professional naming conventions and validation rules</item>
/// <item><strong>Practice Integration:</strong> Designed for integration with legal practice management workflows</item>
/// <item><strong>Client Service:</strong> Supports efficient client service delivery through proper matter creation</item>
/// </list>
/// 
/// <para><strong>Creation Business Rules:</strong></para>
/// <list type="bullet">
/// <item><strong>Description Uniqueness:</strong> Matter descriptions must be unique across the entire system</item>
/// <item><strong>Professional Standards:</strong> Must follow professional legal practice naming conventions</item>
/// <item><strong>Initial State:</strong> New matters are created in active (non-deleted) state</item>
/// <item><strong>Audit Trail Creation:</strong> Creation automatically triggers audit trail initialization</item>
/// </list>
/// 
/// <para><strong>Creation Validation Strategy:</strong></para>
/// <list type="bullet">
/// <item><strong>Required Field Validation:</strong> Ensures all required fields are provided</item>
/// <item><strong>Format Validation:</strong> Validates field formats against professional standards</item>
/// <item><strong>Business Rule Validation:</strong> Enforces creation-specific business rules</item>
/// <item><strong>Uniqueness Validation:</strong> Ensures matter description uniqueness in the system</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a new matter DTO
/// var creationDto = new MatterForCreationDto
/// {
///     Description = "Smith Family Trust - Estate Planning",
///     IsArchived = false, // New matters typically start as active
///     CreationDate = DateTime.UtcNow
/// };
/// 
/// // Validating the creation DTO
/// var validationResults = MatterForCreationDto.ValidateModel(creationDto);
/// if (validationResults.Any())
/// {
///     foreach (var result in validationResults)
///     {
///         Console.WriteLine($"Validation Error: {result.ErrorMessage}");
///     }
/// }
/// 
/// // Using for matter creation
/// if (creationDto.IsValid)
/// {
///     var newMatter = await matterService.CreateMatterAsync(creationDto);
///     Console.WriteLine($"Created matter with ID: {newMatter.Id}");
/// }
/// </code>
/// </example>
public record MatterForCreationDto : IValidatableObject, IEquatable<MatterForCreationDto>
{
    #region Creation Properties

    /// <summary>
    /// Gets the matter description for new matter creation.
    /// </summary>
    /// <remarks>
    /// The matter description serves as the primary human-readable identifier for the new matter and must be
    /// unique across the entire system. This property corresponds to <see cref="ADMS.API.Entities.Matter.Description"/>
    /// and must conform to professional naming conventions and validation rules for legal practice.
    /// 
    /// <para><strong>Creation Validation Rules (via ADMS.API.Common.MatterValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Required:</strong> Cannot be null, empty, or whitespace for new matters</item>
    /// <item><strong>Length:</strong> 3-128 characters (matching database constraint)</item>
    /// <item><strong>Uniqueness:</strong> Must be unique across all existing matters in the system</item>
    /// <item><strong>Format:</strong> Must contain letters and start/end with alphanumeric characters</item>
    /// <item><strong>Reserved Words:</strong> Cannot contain system-reserved terms</item>
    /// <item><strong>Professional Standards:</strong> Must follow professional legal practice naming conventions</item>
    /// </list>
    /// 
    /// <para><strong>Professional Naming Guidelines for New Matters:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Client-Based:</strong> "Smith Family Trust", "ABC Corporation Legal Services", "Johnson Estate Planning"</item>
    /// <item><strong>Matter-Specific:</strong> "Smith v. Jones Contract Dispute", "ABC Corp IPO Legal Support", "Johnson Probate Case"</item>
    /// <item><strong>Project-Based:</strong> "Downtown Development Legal Review", "Patent Portfolio Management - XYZ Tech"</item>
    /// <item><strong>Practice Area:</strong> "Corporate Law - Merger Advisory", "Estate Planning - Trust Administration"</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Matter.Description"/> with identical validation
    /// rules and professional standards, ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional matter descriptions for creation
    /// var trustMatter = new MatterForCreationDto 
    /// { 
    ///     Description = "Smith Family Revocable Living Trust",
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// 
    /// var corporateMatter = new MatterForCreationDto 
    /// { 
    ///     Description = "ABC Corporation - Merger and Acquisition Advisory",
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// 
    /// // Validation before creation
    /// var isValid = MatterValidationHelper.IsValidDescription(trustMatter.Description);
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter description is required.")]
    [StringLength(
        128, // Use constant value directly
        MinimumLength = 3, // Use constant value directly
        ErrorMessage = "Matter description must be between 3 and 128 characters.")]
    public required string Description { get; init; }

    /// <summary>
    /// Gets a value indicating whether the new matter should be created in archived state.
    /// </summary>
    /// <remarks>
    /// The archived status determines the initial lifecycle state of the new matter. This property corresponds to
    /// <see cref="ADMS.API.Entities.Matter.IsArchived"/> and allows for creating matters directly in archived state
    /// for specific business scenarios such as historical data migration or inactive matter setup.
    /// 
    /// <para><strong>Creation State Options:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Active Creation (Default):</strong> IsArchived = false for normal new matters</item>
    /// <item><strong>Archived Creation:</strong> IsArchived = true for historical or immediately inactive matters</item>
    /// <item><strong>Never Deleted:</strong> New matters are never created in deleted state (IsDeleted = false always)</item>
    /// </list>
    /// 
    /// <para><strong>Business Scenarios for Archived Creation:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Historical Migration:</strong> Creating matters for historical records during system migration</item>
    /// <item><strong>Template Matters:</strong> Creating template matters that are immediately archived</item>
    /// <item><strong>Completed Matters:</strong> Rare cases where matters are created for already-completed work</item>
    /// <item><strong>Inactive Setup:</strong> Creating matters that will not be immediately active</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Matter.IsArchived"/> with identical business logic
    /// and state management rules, ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard active matter creation (most common)
    /// var activeMatter = new MatterForCreationDto 
    /// { 
    ///     Description = "New Client Matter - Active Work",
    ///     IsArchived = false, // Default - can be omitted
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// 
    /// // Historical matter creation (special scenarios)
    /// var archivedMatter = new MatterForCreationDto 
    /// { 
    ///     Description = "Historical Matter - Data Migration",
    ///     IsArchived = true, // Created directly in archived state
    ///     CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    /// };
    /// </code>
    /// </example>
    public bool IsArchived { get; init; }

    /// <summary>
    /// Gets the UTC creation date for the new matter.
    /// </summary>
    /// <remarks>
    /// The creation date establishes the temporal foundation for the new matter and all associated activities.
    /// This property corresponds to <see cref="ADMS.API.Entities.Matter.CreationDate"/> and should typically
    /// be set to the current UTC time for new matters, though historical dates are allowed for specific scenarios.
    /// 
    /// <para><strong>Validation Requirements (via ADMS.API.Common.MatterValidationHelper):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Valid Range:</strong> Between January 1, 1980, and current time (with tolerance)</item>
    /// <item><strong>Not Future:</strong> Cannot be set to future dates to prevent temporal inconsistencies</item>
    /// <item><strong>Not Default:</strong> Must be a valid DateTime, not DateTime.MinValue or default values</item>
    /// <item><strong>UTC Requirement:</strong> Must be provided in UTC for global consistency</item>
    /// </list>
    /// 
    /// <para><strong>Entity Alignment:</strong></para>
    /// This property mirrors <see cref="ADMS.API.Entities.Matter.CreationDate"/> with identical validation
    /// rules and temporal constraints, ensuring consistency between entity and DTO representations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard new matter creation with current date
    /// var currentMatter = new MatterForCreationDto 
    /// { 
    ///     Description = "New Client Intake - Current Matter",
    ///     CreationDate = DateTime.UtcNow, // Standard approach
    ///     IsArchived = false
    /// };
    /// 
    /// // Historical matter creation (migration scenario)
    /// var historicalMatter = new MatterForCreationDto 
    /// { 
    ///     Description = "Historical Case - Data Migration",
    ///     CreationDate = new DateTime(2022, 6, 15, 10, 30, 0, DateTimeKind.Utc),
    ///     IsArchived = true
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Creation date is required.")]
    public required DateTime CreationDate { get; init; } = DateTime.UtcNow;

    #endregion Creation Properties

    #region Computed Properties

    /// <summary>
    /// Gets the normalized description for consistent comparison and uniqueness validation.
    /// </summary>
    /// <remarks>
    /// This computed property provides a normalized version of the matter description using
    /// ADMS.API.Common.MatterValidationHelper normalization rules for consistent comparison
    /// and uniqueness validation during creation operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var creation1 = new MatterForCreationDto { Description = "  Contract   Review  " };
    /// var creation2 = new MatterForCreationDto { Description = "Contract Review" };
    /// 
    /// // Both will have the same normalized description: "Contract Review"
    /// bool areEquivalent = creation1.NormalizedDescription == creation2.NormalizedDescription; // true
    /// </code>
    /// </example>
    public string? NormalizedDescription => MatterValidationHelper.NormalizeDescription(Description);

    /// <summary>
    /// Gets the creation date formatted as a localized string for UI display.
    /// </summary>
    /// <remarks>
    /// This computed property provides a user-friendly formatted representation of the creation date
    /// converted to local time, optimized for creation interface display and professional presentation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Display in creation interface
    /// var displayText = $"Matter will be created on: {creationDto.LocalCreationDateString}";
    /// </code>
    /// </example>
    public string LocalCreationDateString => CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets the initial status the matter will have upon creation.
    /// </summary>
    /// <remarks>
    /// This computed property indicates the initial status the matter will have when created,
    /// based on the IsArchived property. New matters are never created in deleted state.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"New matter will be created with status: {creationDto.InitialStatus}");
    /// // Possible outputs: "Active", "Archived"
    /// </code>
    /// </example>
    public string InitialStatus => IsArchived ? "Archived" : "Active";

    /// <summary>
    /// Gets the initial age the matter will have upon creation (typically zero for current date).
    /// </summary>
    /// <remarks>
    /// This computed property calculates the age the matter will have when created, useful for
    /// validating the reasonableness of creation dates and providing temporal context.
    /// </remarks>
    /// <example>
    /// <code>
    /// var age = creationDto.InitialAgeDays;
    /// if (age > 0)
    /// {
    ///     Console.WriteLine($"Matter will be created with initial age of {age:F0} days (backdated)");
    /// }
    /// </code>
    /// </example>
    public double InitialAgeDays => (DateTime.UtcNow - CreationDate).TotalDays;

    /// <summary>
    /// Gets a value indicating whether this creation DTO has valid data for system operations.
    /// </summary>
    /// <remarks>
    /// This property provides a quick validation check without running full validation logic,
    /// useful for UI scenarios where immediate feedback is needed before processing creation operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (creationDto.IsValid)
    /// {
    ///     // Proceed with matter creation
    ///     await ProcessMatterCreation(creationDto);
    /// }
    /// else
    /// {
    ///     // Show validation errors to user
    ///     DisplayCreationValidationErrors(creationDto);
    /// }
    /// </code>
    /// </example>
    public bool IsValid =>
        MatterValidationHelper.IsValidDescription(Description) &&
        MatterValidationHelper.IsValidDate(CreationDate);

    /// <summary>
    /// Gets the display text suitable for creation confirmation and UI display.
    /// </summary>
    /// <remarks>
    /// Provides a consistent format for displaying matter creation information in UI elements,
    /// using the description for clear identification during creation operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Creation confirmation display
    /// var confirmationText = $"Create new matter: {creationDto.DisplayText}";
    /// </code>
    /// </example>
    public string DisplayText => Description;

    /// <summary>
    /// Gets comprehensive creation metrics for validation and analysis.
    /// </summary>
    /// <remarks>
    /// This property provides a structured object containing key metrics and information
    /// for creation validation and professional compliance analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// var metrics = creationDto.CreationMetrics;
    /// // Access comprehensive creation validation metrics
    /// </code>
    /// </example>
    public object CreationMetrics => new
    {
        CreationInfo = new
        {
            Description,
            NormalizedDescription,
            IsArchived,
            InitialStatus,
            LocalCreationDateString,
            DisplayText
        },
        ValidationInfo = new
        {
            IsValid,
            InitialAgeDays
        },
        TemporalInfo = new
        {
            CreationDate,
            InitialAgeDays,
            IsCurrentDate = Math.Abs(InitialAgeDays) < 1, // Within 1 day of current
            IsBackdated = InitialAgeDays > 1, // More than 1 day in the past
            IsReasonableAge = InitialAgeDays is >= 0 and <= 365 * 5 // Within 5 years
        }
    };

    #endregion Computed Properties

    #region Validation Implementation

    /// <summary>
    /// Validates the <see cref="MatterForCreationDto"/> for data integrity and business rules compliance.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// Performs comprehensive validation using the ADMS.API.Common.MatterValidationHelper for consistency 
    /// with entity validation rules. This ensures the DTO maintains the same validation standards as 
    /// the corresponding ADMS.API.Entities.Matter entity while enforcing creation-specific business rules.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterForCreationDto 
    /// { 
    ///     Description = "", // Invalid
    ///     IsArchived = false,
    ///     CreationDate = DateTime.MinValue // Invalid
    /// };
    /// 
    /// var context = new ValidationContext(dto);
    /// var results = dto.Validate(context);
    /// 
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"Creation Validation Error: {result.ErrorMessage}");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate matter description using centralized helper
        foreach (var result in ValidateDescription())
            yield return result;

        // Validate creation date for creation appropriateness
        foreach (var result in ValidateCreationDate())
            yield return result;

        // Validate creation-specific business rules
        foreach (var result in ValidateCreationBusinessRules())
            yield return result;
    }

    /// <summary>
    /// Validates the <see cref="Description"/> property using ADMS validation standards for creation.
    /// </summary>
    /// <returns>A collection of validation results for the matter description.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.MatterValidationHelper.ValidateDescription for comprehensive validation
    /// including format, length, reserved words, and professional naming standards.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateDescription()
    {
        return MatterValidationHelper.ValidateDescription(Description, nameof(Description));
    }

    /// <summary>
    /// Validates the <see cref="CreationDate"/> property using ADMS validation standards for creation.
    /// </summary>
    /// <returns>A collection of validation results for the creation date.</returns>
    /// <remarks>
    /// Uses ADMS.API.Common.MatterValidationHelper.ValidateDate with additional checks for creation
    /// appropriateness and temporal consistency requirements.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateCreationDate()
    {
        foreach (var result in MatterValidationHelper.ValidateDate(CreationDate, nameof(CreationDate)))
            yield return result;

        // Additional validation for creation scenarios
        if (InitialAgeDays > 365 * 10) // More than 10 years old
        {
            yield return new ValidationResult(
                "Creation date is unreasonably far in the past for a new matter. Please verify the date is correct.",
                [nameof(CreationDate)]);
        }
    }

    /// <summary>
    /// Validates creation-specific business rules and constraints.
    /// </summary>
    /// <returns>A collection of validation results for creation business rules.</returns>
    /// <remarks>
    /// Validates business rules specific to matter creation operations, including professional
    /// practice standards and creation-specific constraints.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateCreationBusinessRules()
    {
        // Validate description normalization for creation
        if (string.IsNullOrEmpty(NormalizedDescription))
        {
            yield return new ValidationResult(
                "Description cannot be normalized properly. Please ensure it contains valid characters and content.",
                [nameof(Description)]);
        }

        // Additional creation-specific validations can be added here
    }

    #endregion Validation Implementation

    #region Static Methods

    /// <summary>
    /// Validates a <see cref="MatterForCreationDto"/> instance and returns detailed validation results.
    /// </summary>
    /// <param name="dto">The MatterForCreationDto instance to validate. Can be null.</param>
    /// <returns>A list of validation results indicating any validation failures.</returns>
    /// <remarks>
    /// This static helper method provides a convenient way to validate MatterForCreationDto instances
    /// without requiring a ValidationContext. It performs the same validation as the instance
    /// Validate method but with null-safety and simplified usage.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterForCreationDto 
    /// { 
    ///     Description = "New Smith Family Trust",
    ///     IsArchived = false,
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// 
    /// var results = MatterForCreationDto.ValidateModel(dto);
    /// if (results.Any())
    /// {
    ///     var errorMessages = string.Join(", ", results.Select(r => r.ErrorMessage));
    ///     throw new ValidationException($"Matter creation validation failed: {errorMessages}");
    /// }
    /// </code>
    /// </example>
    public static IList<ValidationResult> ValidateModel([AllowNull] MatterForCreationDto? dto)
    {
        var results = new List<ValidationResult>();

        if (dto is null)
        {
            results.Add(new ValidationResult("MatterForCreationDto instance is required and cannot be null."));
            return results;
        }

        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        return results;
    }

    /// <summary>
    /// Creates a new MatterForCreationDto with current timestamp and active state.
    /// </summary>
    /// <param name="description">The matter description. Cannot be null or empty.</param>
    /// <returns>A valid MatterForCreationDto instance for immediate matter creation.</returns>
    /// <exception cref="ArgumentException">Thrown when description is null or empty.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides a convenient way to create new matter DTOs with standard defaults.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a new matter DTO with defaults
    /// var newMatterDto = MatterForCreationDto.CreateNew("Smith Family Estate Planning");
    /// await matterService.CreateMatterAsync(newMatterDto);
    /// </code>
    /// </example>
    public static MatterForCreationDto CreateNew([NotNull] string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        var dto = new MatterForCreationDto
        {
            Description = description.Trim(),
            CreationDate = DateTime.UtcNow,
            IsArchived = false
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterForCreationDto: {errorMessages}");

    }

    /// <summary>
    /// Creates a new MatterForCreationDto with specified parameters and validation.
    /// </summary>
    /// <param name="description">The matter description. Cannot be null or empty.</param>
    /// <param name="creationDate">The creation date for the matter.</param>
    /// <param name="isArchived">Whether the matter should be created in archived state.</param>
    /// <returns>A valid MatterForCreationDto instance.</returns>
    /// <exception cref="ArgumentException">Thrown when description is null or empty.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method provides full control over matter creation parameters while ensuring
    /// validation compliance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a historical matter
    /// var historicalMatter = MatterForCreationDto.CreateWithParameters(
    ///     "Historical Client Matter - Data Migration",
    ///     new DateTime(2020, 1, 15, 0, 0, 0, DateTimeKind.Utc),
    ///     isArchived: true);
    /// </code>
    /// </example>
    public static MatterForCreationDto CreateWithParameters([NotNull] string description, DateTime creationDate, bool isArchived = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        var dto = new MatterForCreationDto
        {
            Description = description.Trim(),
            CreationDate = creationDate,
            IsArchived = isArchived
        };

        // Validate the created DTO
        var validationResults = ValidateModel(dto);
        if (!validationResults.Any()) return dto;
        var errorMessages = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
        throw new ValidationException($"Failed to create valid MatterForCreationDto with specified parameters: {errorMessages}");

    }

    /// <summary>
    /// Creates multiple MatterForCreationDto instances from a collection of descriptions.
    /// </summary>
    /// <param name="descriptions">The collection of matter descriptions. Cannot be null.</param>
    /// <param name="isArchived">Whether all matters should be created in archived state.</param>
    /// <returns>A collection of valid MatterForCreationDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when descriptions collection is null.</exception>
    /// <remarks>
    /// This bulk creation method is optimized for creating multiple matters efficiently,
    /// useful for batch matter creation operations or initial system setup.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Bulk create active matters
    /// var descriptions = new[] { "Client A Matter", "Client B Matter", "Client C Matter" };
    /// var creationDtos = MatterForCreationDto.CreateMultiple(descriptions, isArchived: false);
    /// 
    /// foreach (var dto in creationDtos)
    /// {
    ///     await matterService.CreateMatterAsync(dto);
    /// }
    /// </code>
    /// </example>
    public static IList<MatterForCreationDto> CreateMultiple([NotNull] IEnumerable<string> descriptions, bool isArchived = false)
    {
        ArgumentNullException.ThrowIfNull(descriptions, nameof(descriptions));

        var result = new List<MatterForCreationDto>();
        var currentTime = DateTime.UtcNow;

        foreach (var description in descriptions)
        {
            if (string.IsNullOrWhiteSpace(description))
                continue;

            try
            {
                var dto = new MatterForCreationDto
                {
                    Description = description.Trim(),
                    CreationDate = currentTime,
                    IsArchived = isArchived
                };

                var validationResults = ValidateModel(dto);
                if (!validationResults.Any())
                {
                    result.Add(dto);
                }
                else
                {
                    // Log invalid description but continue processing others
                    Console.WriteLine($"Warning: Skipped invalid matter description '{description}': {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}");
                }
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                // Log invalid description but continue processing others
                Console.WriteLine($"Warning: Skipped invalid matter description '{description}': {ex.Message}");
            }
        }

        return result;
    }

    #endregion Static Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified MatterForCreationDto is equal to the current MatterForCreationDto.
    /// </summary>
    /// <param name="other">The MatterForCreationDto to compare with the current MatterForCreationDto.</param>
    /// <returns>true if the specified MatterForCreationDto is equal to the current MatterForCreationDto; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all creation properties to identify identical creation operations.
    /// This is useful for detecting duplicate creation requests and optimizing creation operations.
    /// </remarks>
    public virtual bool Equals(MatterForCreationDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase) &&
               IsArchived == other.IsArchived &&
               CreationDate == other.CreationDate;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterForCreationDto.</returns>
    /// <remarks>
    /// The hash code is based on all creation properties to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Description.GetHashCode(StringComparison.OrdinalIgnoreCase),
            IsArchived,
            CreationDate);
    }

    #endregion Equality Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterForCreationDto.
    /// </summary>
    /// <returns>A string that represents the current MatterForCreationDto.</returns>
    /// <remarks>
    /// The string representation includes key creation information for identification and logging purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new MatterForCreationDto 
    /// { 
    ///     Description = "New Smith Family Trust",
    ///     IsArchived = false,
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// 
    /// Console.WriteLine(dto);
    /// // Output: "Create Matter: New Smith Family Trust - Status: Active"
    /// </code>
    /// </example>
    public override string ToString() => $"Create Matter: {Description} - Status: {InitialStatus}";

    #endregion String Representation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this creation DTO represents a backdated matter creation.
    /// </summary>
    /// <returns>true if the creation date is more than one day in the past; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify when a creation operation involves backdating,
    /// which may require special handling, authorization, or audit trail considerations.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (creationDto.IsBackdatedCreation())
    /// {
    ///     // Handle backdated creation
    ///     await auditService.LogBackdatedCreation(creationDto);
    /// }
    /// </code>
    /// </example>
    public bool IsBackdatedCreation() => InitialAgeDays > 1;

    /// <summary>
    /// Determines whether this creation DTO represents a historical matter creation.
    /// </summary>
    /// <returns>true if the creation date is more than 30 days in the past; otherwise, false.</returns>
    /// <remarks>
    /// This method helps identify when a creation operation involves historical data,
    /// which typically occurs during data migration or administrative scenarios.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (creationDto.IsHistoricalCreation())
    /// {
    ///     // Handle historical creation with special processing
    ///     await migrationService.ProcessHistoricalMatter(creationDto);
    /// }
    /// </code>
    /// </example>
    public bool IsHistoricalCreation() => InitialAgeDays > 30;

    /// <summary>
    /// Gets creation summary information for audit and logging purposes.
    /// </summary>
    /// <returns>A dictionary containing comprehensive creation information.</returns>
    /// <remarks>
    /// This method provides structured creation information useful for audit logging,
    /// administrative reporting, and creation process analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// var summary = creationDto.GetCreationSummary();
    /// foreach (var item in summary)
    /// {
    ///     Console.WriteLine($"{item.Key}: {item.Value}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetCreationSummary()
    {
        return new Dictionary<string, object>
        {
            ["Description"] = Description,
            ["NormalizedDescription"] = NormalizedDescription ?? "N/A",
            ["InitialStatus"] = InitialStatus,
            ["IsArchived"] = IsArchived,
            ["CreationDate"] = CreationDate,
            ["LocalCreationDateString"] = LocalCreationDateString,
            ["InitialAgeDays"] = InitialAgeDays,
            ["IsBackdatedCreation"] = IsBackdatedCreation(),
            ["IsHistoricalCreation"] = IsHistoricalCreation(),
            ["IsValid"] = IsValid
        }.ToImmutableDictionary();
    }

    #endregion Business Logic Methods
}