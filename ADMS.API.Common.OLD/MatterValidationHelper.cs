using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ADMS.API.Common;

/// <summary>
/// Provides comprehensive helper methods and constants for validating matter-related data within the ADMS system.
/// </summary>
/// <remarks>
/// This static helper class provides robust matter validation functionality for the ADMS legal 
/// document management system, supporting all matter-related DTOs including MatterDto, MatterMinimalDto, 
/// MatterForCreationDto, and MatterForUpdateDto. The validation methods ensure data integrity, 
/// business rule compliance, and consistent validation logic across the application.
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item>Matter description validation with database constraint alignment</item>
/// <item>GUID validation for matter IDs with proper empty value checking</item>
/// <item>Date validation with timezone normalization and future date protection</item>
/// <item>Business rule validation for matter lifecycle state transitions</item>
/// <item>Uniqueness validation for matter descriptions within contexts</item>
/// <item>Reserved word protection to prevent system conflicts</item>
/// <item>High-performance validation using frozen collections for O(1) lookup performance</item>
/// <item>Thread-safe operations optimized for concurrent access scenarios</item>
/// </list>
/// 
/// <para><strong>Matter Lifecycle States:</strong></para>
/// <list type="bullet">
/// <item><strong>Active:</strong> IsArchived = false, IsDeleted = false</item>
/// <item><strong>Archived:</strong> IsArchived = true, IsDeleted = false</item>
/// <item><strong>Deleted:</strong> IsArchived = true, IsDeleted = true (must be archived first)</item>
/// </list>
/// 
/// <para><strong>Database Synchronization:</strong></para>
/// All validation constraints are synchronized with the Matter entity constraints:
/// <list type="bullet">
/// <item>Description: StringLength(128) - matches Matter.Description constraint</item>
/// <item>CreationDate: DateTime with UTC normalization</item>
/// <item>Business rules: Archive and delete state validation</item>
/// </list>
/// 
/// <para><strong>Legal Compliance:</strong></para>
/// Matter validation enforces legal document management best practices including:
/// <list type="bullet">
/// <item>Audit trail preservation through proper state transitions</item>
/// <item>Data integrity through comprehensive validation rules</item>
/// <item>Unique identification for proper matter management</item>
/// <item>Temporal consistency through date validation</item>
/// </list>
/// 
/// <para><strong>Thread Safety:</strong></para>
/// All methods in this class are thread-safe and use immutable frozen collections for optimal
/// performance in concurrent scenarios without external synchronization.
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// Uses FrozenSet for O(1) average lookup performance and minimal memory allocation.
/// All validation operations are optimized for high-frequency usage in API scenarios.
/// </remarks>
public static partial class MatterValidationHelper
{
    #region Constants

    /// <summary>
    /// The maximum allowed length for a matter description.
    /// </summary>
    /// <remarks>
    /// This value matches the StringLength(128) constraint on the Matter.Description property 
    /// in the ADMS.API.Entities.Matter entity to ensure consistency between validation 
    /// logic and database constraints.
    /// </remarks>
    public const int MaxDescriptionLength = 128;

    /// <summary>
    /// The minimum allowed length for a matter description.
    /// </summary>
    /// <remarks>
    /// Minimum length ensures matter descriptions are meaningful and not just 
    /// single characters or empty strings after trimming. This prevents issues 
    /// with data integrity and user experience.
    /// </remarks>
    public const int MinDescriptionLength = 3;

    /// <summary>
    /// The earliest allowed date for matter operations.
    /// </summary>
    /// <remarks>
    /// This date represents a reasonable lower bound for matter creation dates in the ADMS system,
    /// preventing unrealistic historical dates that might indicate data corruption or system errors.
    /// Set to January 1, 1980, as a practical minimum for legal document management systems.
    /// </remarks>
    public static readonly DateTime MinAllowedMatterDate = new(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The maximum allowed date for matter operations (current UTC time plus tolerance).
    /// </summary>
    /// <remarks>
    /// This represents a reasonable upper bound that accounts for system clock differences
    /// while preventing future dates that could cause issues with business logic.
    /// </remarks>
    public static DateTime MaxAllowedMatterDate => DateTime.UtcNow.AddMinutes(FutureDateToleranceMinutes);

    /// <summary>
    /// The tolerance in minutes for future dates.
    /// </summary>
    /// <remarks>
    /// This tolerance accounts for clock skew between client and server systems,
    /// allowing for small time differences while preventing actual future dates.
    /// </remarks>
    public const int FutureDateToleranceMinutes = 1;

    /// <summary>
    /// Maximum number of description suggestions to generate for user assistance.
    /// </summary>
    /// <remarks>
    /// Limits the number of suggestions to prevent excessive processing while
    /// providing sufficient alternatives for user selection.
    /// </remarks>
    public const int MaxDescriptionSuggestions = 10;

    /// <summary>
    /// List of reserved words that cannot be used in matter descriptions.
    /// These terms are reserved to prevent conflicts with system functionality.
    /// </summary>
    /// <remarks>
    /// These terms are reserved to prevent conflicts with system functionality 
    /// and maintain data integrity. Reserved words are checked case-insensitively
    /// and can appear anywhere in the description.
    /// 
    /// <para><strong>Reserved Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>System Operations:</strong> SYSTEM, ADMIN</item>
    /// <item><strong>Testing:</strong> TEST, SAMPLE, DEMO</item>
    /// <item><strong>Data States:</strong> NULL, UNDEFINED, EMPTY</item>
    /// <item><strong>Operations:</strong> DELETE, REMOVED, PURGED</item>
    /// </list>
    /// </remarks>
    private static readonly string[] _reservedDescriptionWordsArray =
    [
        "SYSTEM",
        "ADMIN",
        "TEST",
        "SAMPLE",
        "DEMO",
        "NULL",
        "UNDEFINED",
        "EMPTY",
        "DELETE",
        "REMOVED",
        "PURGED"
    ];

    /// <summary>
    /// High-performance frozen set of reserved description words for O(1) lookup performance.
    /// </summary>
    /// <remarks>
    /// Uses FrozenSet for optimal read performance in validation scenarios.
    /// Case-insensitive comparison for user-friendly validation.
    /// </remarks>
    private static readonly FrozenSet<string> _reservedDescriptionWordsSet =
        _reservedDescriptionWordsArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the read-only list of reserved description words.
    /// These words cannot be used in matter descriptions.
    /// </summary>
    /// <remarks>
    /// Returns an immutable view of the reserved description words for external consumption.
    /// This property provides thread-safe access to the reserved words list.
    /// </remarks>
    public static IReadOnlyList<string> ReservedDescriptionWords => _reservedDescriptionWordsArray.ToImmutableArray();

    #endregion Constants

    #region Core Validation Methods

    /// <summary>
    /// Determines whether the specified description is valid for a matter.
    /// A valid description meets length, format, and content requirements.
    /// </summary>
    /// <param name="description">The description to validate. Can be null or whitespace.</param>
    /// <returns>
    /// <c>true</c> if the description is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method validates that matter descriptions meet all requirements including:
    /// <list type="bullet">
    /// <item>Length constraints (between {MinDescriptionLength} and {MaxDescriptionLength} characters)</item>
    /// <item>Content requirements (must contain letters, proper start/end characters)</item>
    /// <item>Format validation (no reserved words, proper character usage)</item>
    /// <item>Normalization compatibility (can be properly normalized)</item>
    /// </list>
    /// 
    /// <para><strong>Performance:</strong></para>
    /// Uses optimized validation checks with early termination for better performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid1 = MatterValidationHelper.IsValidDescription("Contract Review - Client ABC");    // true
    /// bool isValid2 = MatterValidationHelper.IsValidDescription("Legal Matter 2024");              // true
    /// bool isInvalid1 = MatterValidationHelper.IsValidDescription("");                              // false (too short)
    /// bool isInvalid2 = MatterValidationHelper.IsValidDescription("A");                             // false (too short)
    /// bool isInvalid3 = MatterValidationHelper.IsValidDescription(new string('A', 129));           // false (too long)
    /// bool isInvalid4 = MatterValidationHelper.IsValidDescription("SYSTEM Matter");                 // false (reserved word)
    /// </code>
    /// </example>
    public static bool IsValidDescription([NotNullWhen(true)] string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return false;

        var trimmed = description.Trim();
        return IsValidDescriptionLength(trimmed) &&
               HasValidDescriptionFormat(trimmed) &&
               !ContainsReservedWords(trimmed);
    }

    /// <summary>
    /// Determines whether the specified date is valid for a matter.
    /// A valid date is within reasonable bounds and not in the future.
    /// </summary>
    /// <param name="date">The date to validate.</param>
    /// <returns>
    /// <c>true</c> if the date is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method ensures matter dates are realistic and comply with business rules:
    /// <list type="bullet">
    /// <item>Must be after the minimum allowed date ({MinAllowedMatterDate:yyyy-MM-dd})</item>
    /// <item>Cannot be in the future (with {FutureDateToleranceMinutes} minute tolerance)</item>
    /// <item>Must not be DateTime.MinValue or other sentinel values</item>
    /// </list>
    /// 
    /// <para><strong>Timezone Handling:</strong></para>
    /// The method works with any DateTime kind but normalizes to UTC for comparison.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid1 = MatterValidationHelper.IsValidDate(DateTime.UtcNow);                    // true
    /// bool isValid2 = MatterValidationHelper.IsValidDate(new DateTime(2020, 1, 1));          // true
    /// bool isInvalid1 = MatterValidationHelper.IsValidDate(DateTime.MinValue);                // false (too early)
    /// bool isInvalid2 = MatterValidationHelper.IsValidDate(DateTime.UtcNow.AddHours(1));      // false (future)
    /// bool isInvalid3 = MatterValidationHelper.IsValidDate(new DateTime(1975, 1, 1));        // false (too early)
    /// </code>
    /// </example>
    public static bool IsValidDate(DateTime date)
    {
        var utcDate = date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(date, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(date, DateTimeKind.Utc)
        };

        return utcDate > DateTime.MinValue &&
               utcDate >= MinAllowedMatterDate &&
               utcDate <= MaxAllowedMatterDate;
    }

    /// <summary>
    /// Determines whether the specified matter ID is valid.
    /// A valid matter ID is a non-empty GUID.
    /// </summary>
    /// <param name="matterId">The matter ID to validate.</param>
    /// <returns>
    /// <c>true</c> if the matter ID is not empty; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method follows the same pattern as other entity validation helpers in the ADMS system,
    /// ensuring consistency across validation logic.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>GUID must not be Guid.Empty</item>
    /// <item>GUID must represent a valid identifier structure</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid = MatterValidationHelper.IsValidMatterId(Guid.NewGuid());  // true
    /// bool isInvalid = MatterValidationHelper.IsValidMatterId(Guid.Empty);    // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidMatterId(Guid matterId) => matterId != Guid.Empty;

    /// <summary>
    /// Determines whether the specified creation date is valid for a matter.
    /// </summary>
    /// <param name="creationDate">The creation date to validate.</param>
    /// <returns>
    /// <c>true</c> if the creation date is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method provides specialized validation for creation dates, ensuring they meet
    /// the same standards as general matter dates. It's an alias for IsValidDate for semantic clarity.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid = MatterValidationHelper.IsValidCreationDate(DateTime.UtcNow);     // true
    /// bool isInvalid = MatterValidationHelper.IsValidCreationDate(DateTime.MinValue); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidCreationDate(DateTime creationDate) => IsValidDate(creationDate);

    #endregion Core Validation Methods

    #region Business Rule Validation Methods

    /// <summary>
    /// Validates that a matter's archive and delete states are consistent with business rules.
    /// </summary>
    /// <param name="isArchived">Whether the matter is archived.</param>
    /// <param name="isDeleted">Whether the matter is deleted.</param>
    /// <returns>
    /// <c>true</c> if the archive state is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method enforces business rules around matter lifecycle states:
    /// <list type="bullet">
    /// <item><strong>Active:</strong> isArchived = false, isDeleted = false</item>
    /// <item><strong>Archived:</strong> isArchived = true, isDeleted = false</item>
    /// <item><strong>Deleted:</strong> isArchived = true, isDeleted = true (deleted matters must be archived)</item>
    /// <item><strong>Invalid:</strong> isArchived = false, isDeleted = true (cannot delete without archiving)</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance:</strong></para>
    /// The requirement that deleted matters must be archived ensures proper audit trails
    /// and compliance with legal document retention requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool valid1 = MatterValidationHelper.IsValidArchiveState(false, false);   // true (active)
    /// bool valid2 = MatterValidationHelper.IsValidArchiveState(true, false);    // true (archived)
    /// bool valid3 = MatterValidationHelper.IsValidArchiveState(true, true);     // true (deleted and archived)
    /// bool invalid = MatterValidationHelper.IsValidArchiveState(false, true);   // false (deleted but not archived)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidArchiveState(bool isArchived, bool isDeleted) =>
        !isDeleted || isArchived; // If deleted, must also be archived

    /// <summary>
    /// Validates that a matter description is unique within a given context.
    /// </summary>
    /// <param name="description">The description to validate. Can be null.</param>
    /// <param name="existingDescriptions">Collection of existing matter descriptions.</param>
    /// <param name="ignoreCaseComparison">Whether to ignore case when comparing descriptions.</param>
    /// <returns>
    /// <c>true</c> if the description is unique; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="existingDescriptions"/> is null.</exception>
    /// <remarks>
    /// This method helps enforce uniqueness constraints for matter descriptions
    /// within specific contexts, such as client-specific matter collections or
    /// active matter lists. It performs normalized comparison to handle whitespace differences.
    /// 
    /// <para><strong>Comparison Process:</strong></para>
    /// <list type="bullet">
    /// <item>Normalizes both input and existing descriptions</item>
    /// <item>Performs case-sensitive or case-insensitive comparison as specified</item>
    /// <item>Handles null and whitespace variations consistently</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var existing = new[] { "Contract Review", "Litigation Case" };
    /// bool unique1 = MatterValidationHelper.IsUniqueDescription("New Matter", existing);        // true
    /// bool unique2 = MatterValidationHelper.IsUniqueDescription("contract review", existing, true); // false (case insensitive)
    /// bool duplicate = MatterValidationHelper.IsUniqueDescription("Contract Review", existing); // false
    /// </code>
    /// </example>
    public static bool IsUniqueDescription(
        string? description,
        [NotNull] IEnumerable<string> existingDescriptions,
        bool ignoreCaseComparison = true)
    {
        ArgumentNullException.ThrowIfNull(existingDescriptions);

        if (string.IsNullOrWhiteSpace(description))
            return false;

        var normalizedDescription = NormalizeDescription(description);
        if (normalizedDescription == null)
            return false;

        var comparisonType = ignoreCaseComparison ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return !existingDescriptions.Any(existing =>
        {
            var normalizedExisting = NormalizeDescription(existing);
            return normalizedExisting != null &&
                   string.Equals(normalizedExisting, normalizedDescription, comparisonType);
        });
    }

    /// <summary>
    /// Validates the business logic for matter lifecycle state transitions.
    /// </summary>
    /// <param name="currentIsArchived">Current archived state.</param>
    /// <param name="currentIsDeleted">Current deleted state.</param>
    /// <param name="newIsArchived">New archived state.</param>
    /// <param name="newIsDeleted">New deleted state.</param>
    /// <returns>
    /// <c>true</c> if the transition is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method validates that matter state transitions follow proper business rules,
    /// preventing invalid state changes that could compromise data integrity or legal compliance.
    /// 
    /// <para><strong>Valid Transitions:</strong></para>
    /// <list type="bullet">
    /// <item>Active → Archived (normal archival)</item>
    /// <item>Archived → Deleted (proper deletion process)</item>
    /// <item>Active → Deleted (if also archived in same operation)</item>
    /// <item>Any state → Same state (no change)</item>
    /// </list>
    /// 
    /// <para><strong>Invalid Transitions:</strong></para>
    /// <list type="bullet">
    /// <item>Deleted → Any undeleted state (cannot restore deleted matters)</item>
    /// <item>Active → Deleted without archiving (must archive first)</item>
    /// <item>Deleted and Archived → Unarchived but still Deleted (logical inconsistency)</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance:</strong></para>
    /// These rules ensure proper audit trails and prevent accidental data loss
    /// while maintaining compliance with legal document retention requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Valid transitions
    /// bool valid1 = MatterValidationHelper.IsValidStateTransition(false, false, true, false);  // active to archived
    /// bool valid2 = MatterValidationHelper.IsValidStateTransition(true, false, true, true);    // archived to deleted
    /// bool valid3 = MatterValidationHelper.IsValidStateTransition(false, false, true, true);   // active to deleted+archived
    /// 
    /// // Invalid transitions
    /// bool invalid1 = MatterValidationHelper.IsValidStateTransition(true, true, false, false); // deleted to active (not allowed)
    /// bool invalid2 = MatterValidationHelper.IsValidStateTransition(false, false, false, true); // active to deleted without archiving
    /// bool invalid3 = MatterValidationHelper.IsValidStateTransition(true, true, false, true);  // unarchive deleted matter
    /// </code>
    /// </example>
    public static bool IsValidStateTransition(
        bool currentIsArchived,
        bool currentIsDeleted,
        bool newIsArchived,
        bool newIsDeleted)
    {
        switch (currentIsDeleted)
        {
            // Cannot undelete a matter (deleted is permanent)
            case true when !newIsDeleted:
            // Cannot delete without archiving first (unless archiving in same operation)
            case false when newIsDeleted && !newIsArchived:
                return false;
            default:
                // Cannot unarchive a deleted matter (logical inconsistency)
                return !currentIsDeleted || !newIsDeleted || !currentIsArchived || newIsArchived;
        }
    }

    #endregion Business Rule Validation Methods

    #region Comprehensive Validation Methods

    /// <summary>
    /// Performs comprehensive validation of a matter description and returns detailed validation results.
    /// </summary>
    /// <param name="description">The description to validate. Can be null.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages). Cannot be null or whitespace.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    /// <remarks>
    /// This method performs all description validation checks in a single call:
    /// <list type="bullet">
    /// <item>Null/whitespace validation</item>
    /// <item>Length validation (min/max bounds)</item>
    /// <item>Format validation (character patterns, start/end validation)</item>
    /// <item>Reserved word validation</item>
    /// <item>Normalization compatibility validation</item>
    /// </list>
    /// 
    /// <para><strong>Validation Order:</strong></para>
    /// Validations are performed in order of severity, with early termination for null values.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = MatterValidationHelper.ValidateDescription("SYSTEM Test", nameof(MyDto.Description));
    /// if (results.Any())
    /// {
    ///     foreach (var result in results)
    ///     {
    ///         Console.WriteLine($"Error: {result.ErrorMessage}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<ValidationResult> ValidateDescription(
        string? description,
        [NotNull] string propertyName)
    {
        // Validate input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Validate description is not null or whitespace
        if (string.IsNullOrWhiteSpace(description))
        {
            yield return new ValidationResult(
                $"{propertyName} is required and cannot be empty.",
                [propertyName]);
            yield break; // No point in further validation
        }

        var trimmed = description.Trim();

        switch (trimmed.Length)
        {
            // Validate length constraints
            case < MinDescriptionLength:
                yield return new ValidationResult(
                    $"{propertyName} must be at least {MinDescriptionLength} characters long.",
                    [propertyName]);
                break;
            case > MaxDescriptionLength:
                yield return new ValidationResult(
                    $"{propertyName} cannot exceed {MaxDescriptionLength} characters.",
                    [propertyName]);
                break;
        }

        // Validate format requirements
        if (!HasValidDescriptionFormat(trimmed))
        {
            if (!trimmed.Any(char.IsLetter))
            {
                yield return new ValidationResult(
                    $"{propertyName} must contain at least one letter.",
                    [propertyName]);
            }
            else if (!char.IsLetterOrDigit(trimmed[0]))
            {
                yield return new ValidationResult(
                    $"{propertyName} must start with a letter or digit.",
                    [propertyName]);
            }
            else if (!char.IsLetterOrDigit(trimmed[^1]))
            {
                yield return new ValidationResult(
                    $"{propertyName} must end with a letter or digit.",
                    [propertyName]);
            }
            else
            {
                yield return new ValidationResult(
                    $"{propertyName} contains invalid characters or format.",
                    [propertyName]);
            }
        }

        // Check for reserved words
        if (ContainsReservedWords(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} contains reserved words. Reserved words: {ReservedDescriptionWordsList}.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Performs comprehensive validation of a matter ID.
    /// </summary>
    /// <param name="matterId">The matter ID to validate.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages). Cannot be null or whitespace.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    /// <remarks>
    /// This method validates that the matter ID represents a valid, non-empty GUID suitable
    /// for use as a database identifier.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = MatterValidationHelper.ValidateMatterId(Guid.Empty, nameof(MyDto.MatterId));
    /// if (results.Any())
    /// {
    ///     Console.WriteLine($"Matter ID validation failed: {string.Join(", ", results.Select(r => r.ErrorMessage))}");
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<ValidationResult> ValidateMatterId(
        Guid matterId,
        [NotNull] string propertyName)
    {
        // Validate input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Validate GUID is not empty
        if (matterId == Guid.Empty)
        {
            yield return new ValidationResult(
                $"{propertyName} must be a valid non-empty GUID.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Performs comprehensive validation of a matter date.
    /// </summary>
    /// <param name="date">The date to validate.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages). Cannot be null or whitespace.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    /// <remarks>
    /// This method validates that the date is within acceptable bounds for matter operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = MatterValidationHelper.ValidateDate(DateTime.MinValue, nameof(MyDto.CreationDate));
    /// if (results.Any())
    /// {
    ///     Console.WriteLine($"Date validation failed: {string.Join(", ", results.Select(r => r.ErrorMessage))}");
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<ValidationResult> ValidateDate(
        DateTime date,
        [NotNull] string propertyName)
    {
        // Validate input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (IsValidDate(date)) yield break;
        if (date <= DateTime.MinValue)
        {
            yield return new ValidationResult(
                $"{propertyName} must be a valid date.",
                [propertyName]);
        }
        else if (date < MinAllowedMatterDate)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot be earlier than {MinAllowedMatterDate:yyyy-MM-dd}.",
                [propertyName]);
        }
        else if (date > MaxAllowedMatterDate)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot be in the future.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates matter state consistency and transitions.
    /// </summary>
    /// <param name="isArchived">Current or new archived state.</param>
    /// <param name="isDeleted">Current or new deleted state.</param>
    /// <param name="archivedPropertyName">Property name for archived state (for error messages). Cannot be null or whitespace.</param>
    /// <param name="deletedPropertyName">Property name for deleted state (for error messages). Cannot be null or whitespace.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when property names are null, empty, or consist only of whitespace.</exception>
    /// <remarks>
    /// This method validates that matter states are consistent with business rules.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = MatterValidationHelper.ValidateStates(false, true, nameof(MyDto.IsArchived), nameof(MyDto.IsDeleted));
    /// // Will return validation error for deleted but not archived
    /// </code>
    /// </example>
    public static IEnumerable<ValidationResult> ValidateStates(
        bool isArchived,
        bool isDeleted,
        [NotNull] string archivedPropertyName,
        [NotNull] string deletedPropertyName)
    {
        // Validate input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(archivedPropertyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(deletedPropertyName);

        if (!IsValidArchiveState(isArchived, isDeleted))
        {
            yield return new ValidationResult(
                "Deleted matters must also be archived to maintain proper audit trails.",
                [archivedPropertyName, deletedPropertyName]);
        }
    }

    #endregion Comprehensive Validation Methods

    #region Normalization and Formatting Methods

    /// <summary>
    /// Normalizes a matter description for consistent storage and comparison.
    /// </summary>
    /// <param name="description">The description to normalize. Can be null.</param>
    /// <returns>
    /// The normalized description, or null if the input is invalid.
    /// </returns>
    /// <remarks>
    /// Normalization includes trimming whitespace and standardizing spacing
    /// for consistent storage and comparison operations.
    /// 
    /// <para><strong>Normalization Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Trims leading and trailing whitespace</item>
    /// <item>Normalizes multiple consecutive spaces to single spaces</item>
    /// <item>Preserves internal punctuation and formatting</item>
    /// <item>Returns null for null or whitespace-only input</item>
    /// </list>
    /// 
    /// <para><strong>Performance:</strong></para>
    /// Uses compiled regex for efficient whitespace normalization.
    /// </remarks>
    /// <example>
    /// <code>
    /// string? normalized1 = MatterValidationHelper.NormalizeDescription("  Contract   Review  "); // "Contract Review"
    /// string? normalized2 = MatterValidationHelper.NormalizeDescription("Legal\t\tMatter");       // "Legal Matter"
    /// string? invalid1 = MatterValidationHelper.NormalizeDescription("");                         // null
    /// string? invalid2 = MatterValidationHelper.NormalizeDescription(null);                       // null
    /// </code>
    /// </example>
    [return: NotNullIfNotNull(nameof(description))]
    public static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var trimmed = description.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;

        // Normalize multiple spaces to single spaces using compiled regex
        var normalized = MultipleWhitespaceRegex().Replace(trimmed, " ");
        return normalized;
    }

    /// <summary>
    /// Checks if two matter descriptions are equivalent after normalization.
    /// </summary>
    /// <param name="description1">The first description. Can be null.</param>
    /// <param name="description2">The second description. Can be null.</param>
    /// <returns>
    /// <c>true</c> if the descriptions are equivalent; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is useful for comparing descriptions in a case-insensitive manner
    /// while handling whitespace differences through normalization.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool same1 = MatterValidationHelper.AreDescriptionsEquivalent("Contract Review", "contract review");     // true
    /// bool same2 = MatterValidationHelper.AreDescriptionsEquivalent("  Contract  Review  ", "Contract Review"); // true
    /// bool different = MatterValidationHelper.AreDescriptionsEquivalent("Contract Review", "Litigation Case"); // false
    /// bool bothNull = MatterValidationHelper.AreDescriptionsEquivalent(null, null);                            // false
    /// </code>
    /// </example>
    public static bool AreDescriptionsEquivalent(string? description1, string? description2)
    {
        var normalized1 = NormalizeDescription(description1);
        var normalized2 = NormalizeDescription(description2);

        return !string.IsNullOrEmpty(normalized1) &&
               !string.IsNullOrEmpty(normalized2) &&
               string.Equals(normalized1, normalized2, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Normalizes a date to UTC for consistent storage.
    /// </summary>
    /// <param name="date">The date to normalize.</param>
    /// <returns>
    /// The date normalized to UTC, or null if the input date is invalid.
    /// </returns>
    /// <remarks>
    /// This method ensures all matter dates are stored in UTC for consistency,
    /// while validating the date meets basic requirements.
    /// 
    /// <para><strong>Normalization Process:</strong></para>
    /// <list type="bullet">
    /// <item>Validates date is within acceptable bounds</item>
    /// <item>Converts to UTC based on DateTimeKind</item>
    /// <item>Returns null for invalid dates</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var localDate = DateTime.Now;
    /// DateTime? utcDate = MatterValidationHelper.NormalizeDateToUtc(localDate);
    /// // Returns the date converted to UTC if valid
    /// 
    /// DateTime? invalid = MatterValidationHelper.NormalizeDateToUtc(DateTime.MinValue);
    /// // Returns null for invalid dates
    /// </code>
    /// </example>
    public static DateTime? NormalizeDateToUtc(DateTime date)
    {
        if (!IsValidDate(date))
            return null;

        return date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(date, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(date, DateTimeKind.Utc)
        };
    }

    /// <summary>
    /// Gets the list of reserved description words as a formatted, comma-separated string.
    /// </summary>
    /// <returns>
    /// A string containing all reserved description words separated by commas and spaces.
    /// </returns>
    /// <remarks>
    /// This property is useful for error messages and documentation where a human-readable
    /// list of reserved words is needed.
    /// </remarks>
    public static string ReservedDescriptionWordsList => string.Join(", ", ReservedDescriptionWords);

    #endregion Normalization and Formatting Methods

    #region Suggestion and Utility Methods

    /// <summary>
    /// Suggests alternative descriptions if the provided description is not valid.
    /// </summary>
    /// <param name="attemptedDescription">The attempted description. Can be null.</param>
    /// <param name="maxSuggestions">Maximum number of suggestions to return.</param>
    /// <returns>
    /// A read-only list of suggested alternative descriptions.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxSuggestions"/> is less than 1.</exception>
    /// <remarks>
    /// This method provides helpful suggestions when a description validation fails,
    /// improving user experience by offering valid alternatives based on the attempted input.
    /// 
    /// <para><strong>Suggestion Strategies:</strong></para>
    /// <list type="bullet">
    /// <item>For short descriptions: Add prefixes like "Matter -", "Case -"</item>
    /// <item>For long descriptions: Suggest truncation strategies</item>
    /// <item>For reserved words: Offer generic alternatives</item>
    /// <item>For null/empty: Provide common matter description templates</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var suggestions = MatterValidationHelper.SuggestAlternativeDescriptions("A", 3);
    /// // Returns suggestions like ["Matter - A", "Case - A", "Project - A"]
    /// 
    /// var suggestions2 = MatterValidationHelper.SuggestAlternativeDescriptions("SYSTEM", 3);
    /// // Returns suggestions like ["Legal Matter", "Client Case", "Legal Project"]
    /// </code>
    /// </example>
    public static IReadOnlyList<string> SuggestAlternativeDescriptions(
        string? attemptedDescription,
        int maxSuggestions = MaxDescriptionSuggestions)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxSuggestions, 1);

        var suggestions = new List<string>();

        if (string.IsNullOrWhiteSpace(attemptedDescription))
        {
            // Return generic suggestions if no input provided
            var genericSuggestions = new[]
            {
                "Legal Matter",
                "Client Case",
                "Legal Project",
                "Matter Review",
                "Case Analysis",
                "Legal Consultation",
                "Contract Review",
                "Litigation Case",
                "Legal Research",
                "Client Matter"
            };
            return genericSuggestions.Take(maxSuggestions).ToImmutableArray();
        }

        var normalized = attemptedDescription.Trim();

        switch (normalized.Length)
        {
            // If too short, suggest extensions
            case < MinDescriptionLength:
                suggestions.AddRange([
                    $"Matter - {normalized}",
                    $"Case - {normalized}",
                    $"Project - {normalized}",
                    $"Legal Matter {normalized}",
                    $"{normalized} Case"
                ]);
                break;
            // If too long, suggest truncations
            case > MaxDescriptionLength:
            {
                // Smart truncation at word boundaries
                var truncated = normalized.Length > MaxDescriptionLength - 3
                    ? normalized[..(MaxDescriptionLength - 3)] + "..."
                    : normalized;
                suggestions.Add(truncated);

                // Suggest shortened versions based on words
                var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 1)
                {
                    var halfWords = string.Join(" ", words.Take(Math.Max(1, words.Length / 2)));
                    if (halfWords.Length <= MaxDescriptionLength && halfWords.Length >= MinDescriptionLength)
                    {
                        suggestions.Add(halfWords);
                    }

                    // Try first few words
                    for (var i = 1; i <= Math.Min(3, words.Length); i++)
                    {
                        var firstWords = string.Join(" ", words.Take(i));
                        if (firstWords.Length <= MaxDescriptionLength && firstWords.Length >= MinDescriptionLength)
                        {
                            suggestions.Add(firstWords);
                        }
                    }
                }

                break;
            }
            // If contains reserved words, suggest alternatives
            default:
            {
                if (ContainsReservedWords(normalized))
                {
                    suggestions.AddRange([
                        "Legal Matter",
                        "Client Case",
                        "Legal Project",
                        "Matter Review",
                        "Case Study"
                    ]);
                }
                // For format issues, suggest cleaned versions
                else
                {
                    var cleaned = CleanDescription(normalized);
                    if (!string.IsNullOrEmpty(cleaned) && cleaned != normalized)
                    {
                        suggestions.Add(cleaned);
                    }

                    suggestions.AddRange([
                        $"Legal {normalized}",
                        $"{normalized} Matter",
                        $"{normalized} Case"
                    ]);
                }

                break;
            }
        }

        // Fill remaining slots with generic suggestions if needed
        if (suggestions.Count < maxSuggestions)
        {
            var remaining = maxSuggestions - suggestions.Count;
            var genericSuggestions = new[]
            {
                "Legal Matter",
                "Client Case",
                "Legal Project",
                "Contract Review",
                "Litigation Case"
            };

            suggestions.AddRange(genericSuggestions
                .Where(g => !suggestions.Contains(g, StringComparer.OrdinalIgnoreCase))
                .Take(remaining));
        }

        return suggestions
            .Where(s => IsValidDescription(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(maxSuggestions)
            .ToImmutableArray();
    }

    #endregion Suggestion and Utility Methods

    #region Diagnostic and Statistics Methods

    /// <summary>
    /// Gets detailed validation information about a matter description for diagnostic purposes.
    /// </summary>
    /// <param name="description">The description to analyze. Can be null.</param>
    /// <returns>
    /// A dictionary containing detailed validation results and diagnostic information.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive diagnostic information about description validation,
    /// useful for troubleshooting validation failures and providing detailed analysis.
    /// 
    /// <para><strong>Diagnostic Information Includes:</strong></para>
    /// <list type="bullet">
    /// <item>Basic validation results (null, length, format)</item>
    /// <item>Character analysis (letters, digits, special characters)</item>
    /// <item>Word counting and structure analysis</item>
    /// <item>Reserved word detection results</item>
    /// <item>Normalization results</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = MatterValidationHelper.GetDescriptionValidationDetails("Contract Review");
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"{result.Key}: {result.Value}");
    /// }
    /// // Output includes: IsNotNullOrEmpty: True, OriginalLength: 15, IsOverallValid: True, etc.
    /// </code>
    /// </example>
    public static IReadOnlyDictionary<string, object> GetDescriptionValidationDetails(string? description)
    {
        var details = new Dictionary<string, object>
        {
            ["IsNotNullOrEmpty"] = !string.IsNullOrWhiteSpace(description),
            ["OriginalLength"] = description?.Length ?? 0,
            ["TrimmedLength"] = description?.Trim().Length ?? 0,
            ["IsValidLength"] = IsValidDescriptionLength(description),
            ["ContainsReservedWords"] = description != null && ContainsReservedWords(description.Trim()),
            ["HasValidFormat"] = description != null && HasValidDescriptionFormat(description.Trim()),
            ["NormalizedDescription"] = NormalizeDescription(description),
            ["IsOverallValid"] = IsValidDescription(description)
        };

        if (string.IsNullOrWhiteSpace(description)) return details.ToImmutableDictionary();
        var trimmed = description.Trim();
        details["WordCount"] = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        details["ContainsLetters"] = trimmed.Any(char.IsLetter);
        details["ContainsDigits"] = trimmed.Any(char.IsDigit);
        details["ContainsPunctuation"] = trimmed.Any(char.IsPunctuation);
        details["StartsWithLetterOrDigit"] = char.IsLetterOrDigit(trimmed[0]);
        details["EndsWithLetterOrDigit"] = char.IsLetterOrDigit(trimmed[^1]);
        details["HasMultipleSpaces"] = MultipleWhitespaceRegex().IsMatch(trimmed);

        // Find specific reserved words
        var foundReservedWords = _reservedDescriptionWordsSet
            .Where(reserved => trimmed.Contains(reserved, StringComparison.OrdinalIgnoreCase))
            .ToList();
        details["FoundReservedWords"] = foundReservedWords;

        return details.ToImmutableDictionary();
    }

    /// <summary>
    /// Gets validation statistics for the matter validation system.
    /// </summary>
    /// <returns>
    /// A dictionary containing statistical information about the validation system.
    /// </returns>
    /// <remarks>
    /// This method provides insights into the validation system configuration,
    /// useful for monitoring, diagnostics, and system documentation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = MatterValidationHelper.GetValidationStatistics();
    /// Console.WriteLine($"Max description length: {stats["MaxDescriptionLength"]}");
    /// Console.WriteLine($"Min description length: {stats["MinDescriptionLength"]}");
    /// Console.WriteLine($"Reserved words count: {stats["ReservedWordsCount"]}");
    /// </code>
    /// </example>
    public static IReadOnlyDictionary<string, object> GetValidationStatistics()
    {
        return new Dictionary<string, object>
        {
            ["MaxDescriptionLength"] = MaxDescriptionLength,
            ["MinDescriptionLength"] = MinDescriptionLength,
            ["ReservedWordsCount"] = ReservedDescriptionWords.Count,
            ["MinAllowedDate"] = MinAllowedMatterDate,
            ["MaxAllowedDate"] = MaxAllowedMatterDate,
            ["FutureDateToleranceMinutes"] = FutureDateToleranceMinutes,
            ["MaxDescriptionSuggestions"] = MaxDescriptionSuggestions,
            ["ReservedWords"] = ReservedDescriptionWords,
            ["ValidationRules"] = new Dictionary<string, string>
            {
                ["DescriptionRequired"] = "Description cannot be null or empty",
                ["DescriptionLength"] = $"Description must be between {MinDescriptionLength} and {MaxDescriptionLength} characters",
                ["DescriptionFormat"] = "Description must contain letters and start/end with alphanumeric characters",
                ["NoReservedWords"] = "Description cannot contain system reserved words",
                ["ValidDate"] = $"Date must be between {MinAllowedMatterDate:yyyy-MM-dd} and current time",
                ["ValidGuid"] = "GUID must not be empty",
                ["ArchiveState"] = "Deleted matters must be archived"
            }
        }.ToImmutableDictionary();
    }

    #endregion Diagnostic and Statistics Methods

    #region Private Helper Methods

    /// <summary>
    /// Validates if a description length is within acceptable bounds.
    /// </summary>
    /// <param name="description">The description to validate.</param>
    /// <returns><c>true</c> if the length is valid; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidDescriptionLength([NotNullWhen(true)] string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return false;

        var trimmedLength = description.Trim().Length;
        return trimmedLength >= MinDescriptionLength && trimmedLength <= MaxDescriptionLength;
    }

    /// <summary>
    /// Checks if a description contains any reserved words using high-performance lookup.
    /// </summary>
    /// <param name="description">The description to check.</param>
    /// <returns><c>true</c> if reserved words are found; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ContainsReservedWords(string description)
    {
        var upperDescription = description.ToUpperInvariant();
        return _reservedDescriptionWordsSet.Any(reserved => upperDescription.Contains(reserved));
    }

    /// <summary>
    /// Validates the format of a matter description.
    /// </summary>
    /// <param name="description">The description to validate.</param>
    /// <returns><c>true</c> if the format is valid; otherwise, <c>false</c>.</returns>
    private static bool HasValidDescriptionFormat(string description)
    {
        // Must contain at least one letter
        if (!description.Any(char.IsLetter))
            return false;

        // Cannot be only numbers or special characters
        if (description.All(c => !char.IsLetter(c)))
            return false;

        // Cannot start or end with special characters (must be letter or digit)
        return char.IsLetterOrDigit(description[0]) && char.IsLetterOrDigit(description[^1]);
    }

    /// <summary>
    /// Cleans a description by removing or replacing invalid characters.
    /// </summary>
    /// <param name="description">The description to clean.</param>
    /// <returns>The cleaned description.</returns>
    private static string CleanDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        var sb = new StringBuilder(description.Length);
        var previousWasSpace = false;

        foreach (var c in description)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
                previousWasSpace = false;
            }
            else if (char.IsWhiteSpace(c) && !previousWasSpace && sb.Length > 0)
            {
                sb.Append(' ');
                previousWasSpace = true;
            }
            else if (char.IsPunctuation(c) && !previousWasSpace && sb.Length > 0)
            {
                sb.Append(c);
                previousWasSpace = false;
            }
        }

        var result = sb.ToString().Trim();

        // Ensure it ends with a letter or digit
        while (result.Length > 0 && !char.IsLetterOrDigit(result[^1]))
        {
            result = result[..^1];
        }

        return result;
    }

    #endregion Private Helper Methods

    #region Compiled Regex Patterns

    /// <summary>
    /// Compiled regex for normalizing multiple consecutive whitespace characters to single spaces.
    /// </summary>
    /// <remarks>
    /// This regex matches one or more whitespace characters (spaces, tabs, newlines, etc.)
    /// and is used to normalize descriptions by replacing multiple consecutive whitespace
    /// characters with a single space.
    /// 
    /// Pattern: \s+ (one or more whitespace characters)
    /// Options: Compiled for optimal performance with repeated use
    /// </remarks>
    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex MultipleWhitespaceRegex();

    #endregion Compiled Regex Patterns
}