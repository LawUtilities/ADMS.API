using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace ADMS.API.Common;

/// <summary>
/// Provides comprehensive helper methods and constants for validating revision activity-related data within the ADMS system.
/// </summary>
/// <remarks>
/// This static helper class provides robust revision activity validation functionality for the ADMS legal 
/// document management system, supporting RevisionActivityDto and RevisionActivityMinimalDto validation.
/// The validation methods ensure data integrity, business rule compliance, and consistent validation logic 
/// across the application.
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item>Activity name validation against predefined allowed activities from database seed data</item>
/// <item>GUID validation for activity IDs with proper empty value checking</item>
/// <item>Length validation consistent with database constraints (StringLength(50))</item>
/// <item>Format validation for activity names with pattern matching</item>
/// <item>Business rule validation for revision lifecycle operations</item>
/// <item>User association validation requirements for audit trail integrity</item>
/// <item>High-performance validation using frozen collections for O(1) lookup performance</item>
/// <item>Thread-safe operations optimized for concurrent access scenarios</item>
/// </list>
/// 
/// <para><strong>Revision Activity Categories:</strong></para>
/// <list type="bullet">
/// <item><strong>Lifecycle Operations:</strong> CREATED, DELETED, RESTORED, SAVED</item>
/// </list>
/// 
/// <para><strong>Database Synchronization:</strong></para>
/// The allowed activities list is synchronized with the RevisionActivity seed data in AdmsContext.cs,
/// ensuring consistency between validation logic and database constraints. These activities represent
/// the standard revision lifecycle events for document versions in the legal document management system.
/// 
/// <para><strong>Business Context:</strong></para>
/// Revision activities track the lifecycle of document revisions, from creation through modification
/// to archival or deletion. Each activity represents a significant state change or operation performed
/// on a document revision, maintaining comprehensive audit trails for legal compliance.
/// 
/// <para><strong>Thread Safety:</strong></para>
/// All methods in this class are thread-safe and use immutable frozen collections for optimal
/// performance in concurrent scenarios without external synchronization.
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// Uses FrozenSet for O(1) average lookup performance and minimal memory allocation.
/// All validation operations are optimized for high-frequency usage in API scenarios.
/// </remarks>
public static class RevisionActivityValidationHelper
{
    #region Constants

    /// <summary>
    /// The maximum allowed length for a revision activity name.
    /// </summary>
    /// <remarks>
    /// This value matches the StringLength(50) constraint on the RevisionActivity.Activity property 
    /// in the ADMS.API.Entities.RevisionActivity entity to ensure consistency between validation 
    /// logic and database constraints.
    /// </remarks>
    public const int MaxActivityLength = 50;

    /// <summary>
    /// The minimum allowed length for a revision activity name.
    /// </summary>
    /// <remarks>
    /// Minimum length ensures activity names are meaningful and not just single characters
    /// or empty strings after trimming. This prevents issues with data integrity and user experience.
    /// </remarks>
    public const int MinActivityLength = 1;

    /// <summary>
    /// Maximum number of activity suggestions to generate for user assistance.
    /// </summary>
    /// <remarks>
    /// Limits the number of suggestions to prevent excessive processing while
    /// providing sufficient alternatives for user selection.
    /// </remarks>
    public const int MaxActivitySuggestions = 8;

    /// <summary>
    /// The list of allowed revision activity names synchronized with database seed data.
    /// All values are uppercase and unique, corresponding to the RevisionActivity seed data in AdmsContext.cs.
    /// </summary>
    /// <remarks>
    /// These activity types represent the standard lifecycle events for document revisions
    /// in the ADMS legal document management system. Activities describe actions taken
    /// on document revisions throughout their lifecycle.
    /// 
    /// <para><strong>Activity Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Lifecycle Operations:</strong> CREATED, DELETED, RESTORED, SAVED</item>
    /// </list>
    /// 
    /// <para><strong>Database Synchronization:</strong></para>
    /// This list must be kept in sync with the RevisionActivity seed data in 
    /// DbContexts/AdmsContext.SeedRevisionActivities() method to ensure validation consistency.
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Initial creation of a document revision</item>
    /// <item><strong>SAVED:</strong> Saving changes to an existing revision</item>
    /// <item><strong>DELETED:</strong> Soft deletion of a revision (can be restored)</item>
    /// <item><strong>RESTORED:</strong> Restoration of a previously deleted revision</item>
    /// </list>
    /// 
    /// <para><strong>Modification Guidelines:</strong></para>
    /// When adding new activities:
    /// <list type="number">
    /// <item>Add the activity to the database seed data first</item>
    /// <item>Update this list to match the seed data</item>
    /// <item>Ensure activity names are uppercase and descriptive</item>
    /// <item>Follow existing naming conventions for consistency</item>
    /// <item>Update documentation and tests accordingly</item>
    /// <item>Consider business implications for revision lifecycle management</item>
    /// </list>
    /// </remarks>
    private static readonly string[] _allowedActivitiesArray =
    [
        "CREATED",
        "DELETED",
        "RESTORED",
        "SAVED"
    ];

    /// <summary>
    /// The list of reserved activity names that cannot be used for standard revision operations.
    /// These names are reserved for system operations and error conditions.
    /// </summary>
    /// <remarks>
    /// Reserved names prevent conflicts with system-generated activities and ensure
    /// clear separation between user-initiated activities and system operations.
    /// 
    /// <para><strong>Reserved Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>System Operations:</strong> SYSTEM, ADMIN, AUTO, BATCH</item>
    /// <item><strong>Data Operations:</strong> MIGRATION, IMPORT, EXPORT</item>
    /// <item><strong>Error Conditions:</strong> CORRUPT, ERROR, FAILED</item>
    /// <item><strong>Cleanup Operations:</strong> PURGE</item>
    /// </list>
    /// </remarks>
    private static readonly string[] _reservedActivityNamesArray =
    [
        "SYSTEM",
        "ADMIN",
        "AUTO",
        "BATCH",
        "MIGRATION",
        "IMPORT",
        "EXPORT",
        "PURGE",
        "CORRUPT",
        "ERROR",
        "FAILED"
    ];

    /// <summary>
    /// High-performance frozen set of allowed activities for O(1) lookup performance.
    /// </summary>
    /// <remarks>
    /// Uses FrozenSet for optimal read performance in validation scenarios.
    /// Thread-safe and immutable for concurrent access without locking.
    /// </remarks>
    private static readonly FrozenSet<string> _allowedActivitiesSet =
        _allowedActivitiesArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// High-performance frozen set of reserved activity names for O(1) lookup performance.
    /// </summary>
    /// <remarks>
    /// Uses FrozenSet for optimal read performance in validation scenarios.
    /// Case-insensitive comparison for cross-platform compatibility.
    /// </remarks>
    private static readonly FrozenSet<string> _reservedActivityNamesSet =
        _reservedActivityNamesArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the read-only list of allowed revision activity names.
    /// All values are uppercase and unique.
    /// </summary>
    /// <remarks>
    /// Returns an immutable view of the allowed activities for external consumption.
    /// This property provides thread-safe access to the activities list.
    /// </remarks>
    public static IReadOnlyList<string> AllowedActivities => _allowedActivitiesArray.ToImmutableArray();

    /// <summary>
    /// Gets the read-only list of reserved activity names.
    /// These names cannot be used for custom activities.
    /// </summary>
    /// <remarks>
    /// Returns an immutable view of the reserved activity names for external consumption.
    /// This property provides thread-safe access to the reserved names list.
    /// </remarks>
    public static IReadOnlyList<string> ReservedActivityNames => _reservedActivityNamesArray.ToImmutableArray();

    #endregion Constants

    #region Core Validation Methods

    /// <summary>
    /// Determines whether the specified activity is allowed.
    /// The comparison is case-insensitive and ignores leading/trailing whitespace.
    /// </summary>
    /// <param name="activity">The activity name to validate. Can be null or whitespace.</param>
    /// <returns>
    /// <c>true</c> if the activity is non-empty and exists in <see cref="AllowedActivities"/>; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method validates that activity names conform to the predefined list of allowed activities,
    /// ensuring consistency across the ADMS system and preventing invalid activity types that could
    /// compromise business logic or reporting.
    /// 
    /// <para><strong>Performance:</strong></para>
    /// Uses FrozenSet for O(1) average lookup performance, optimized for high-frequency validation.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Null or whitespace strings return false</item>
    /// <item>Leading/trailing whitespace is ignored</item>
    /// <item>Case-insensitive comparison</item>
    /// <item>Must match exactly one of the predefined allowed activities</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid1 = RevisionActivityValidationHelper.IsActivityAllowed("CREATED");     // true
    /// bool isValid2 = RevisionActivityValidationHelper.IsActivityAllowed("created");     // true (case insensitive)
    /// bool isValid3 = RevisionActivityValidationHelper.IsActivityAllowed(" SAVED ");     // true (ignores whitespace)
    /// bool isInvalid1 = RevisionActivityValidationHelper.IsActivityAllowed("INVALID");   // false
    /// bool isInvalid2 = RevisionActivityValidationHelper.IsActivityAllowed("");          // false
    /// bool isInvalid3 = RevisionActivityValidationHelper.IsActivityAllowed(null);        // false
    /// </code>
    /// </example>
    public static bool IsActivityAllowed([NotNullWhen(true)] string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) && _allowedActivitiesSet.Contains(activity.Trim());
    }

    /// <summary>
    /// Determines whether the specified activity ID is valid.
    /// A valid activity ID is a non-empty GUID.
    /// </summary>
    /// <param name="activityId">The activity ID to validate.</param>
    /// <returns>
    /// <c>true</c> if the activity ID is not empty; otherwise, <c>false</c>.
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
    /// bool isValid = RevisionActivityValidationHelper.IsValidActivityId(Guid.NewGuid());  // true
    /// bool isInvalid = RevisionActivityValidationHelper.IsValidActivityId(Guid.Empty);    // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidActivityId(Guid activityId) => activityId != Guid.Empty;

    /// <summary>
    /// Determines whether the specified activity name has valid length.
    /// </summary>
    /// <param name="activity">The activity name to validate. Can be null.</param>
    /// <returns>
    /// <c>true</c> if the activity length is within valid bounds; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method validates activity name length against the database constraints to prevent
    /// data truncation or invalid data storage.
    /// 
    /// <para><strong>Length Constraints:</strong></para>
    /// <list type="bullet">
    /// <item>Minimum length: {MinActivityLength} character (after trimming)</item>
    /// <item>Maximum length: {MaxActivityLength} characters (matches database constraint)</item>
    /// <item>Null or whitespace strings return false</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid = RevisionActivityValidationHelper.IsValidActivityLength("CREATED");          // true
    /// bool isValid2 = RevisionActivityValidationHelper.IsValidActivityLength("A");               // true (minimum 1 char)
    /// bool isInvalid = RevisionActivityValidationHelper.IsValidActivityLength("");               // false (empty)
    /// bool isInvalid2 = RevisionActivityValidationHelper.IsValidActivityLength(new string('A', 51)); // false (too long)
    /// </code>
    /// </example>
    public static bool IsValidActivityLength([NotNullWhen(true)] string? activity)
    {
        if (string.IsNullOrWhiteSpace(activity))
            return false;

        var trimmedLength = activity.Trim().Length;
        return trimmedLength >= MinActivityLength && trimmedLength <= MaxActivityLength;
    }

    /// <summary>
    /// Determines whether the specified activity name contains only valid characters.
    /// </summary>
    /// <param name="activity">The activity name to validate. Can be null.</param>
    /// <returns>
    /// <c>true</c> if the activity contains only valid characters; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Valid activity names should contain only letters, numbers, and underscores,
    /// following standard naming conventions for activity types in legal document management.
    /// 
    /// <para><strong>Format Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Must contain at least one letter</item>
    /// <item>Can only contain letters, numbers, and underscores</item>
    /// <item>No spaces or special characters allowed</item>
    /// <item>Case-insensitive validation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid1 = RevisionActivityValidationHelper.IsValidActivityFormat("CREATED");      // true
    /// bool isValid2 = RevisionActivityValidationHelper.IsValidActivityFormat("STEP_1");       // true
    /// bool isInvalid1 = RevisionActivityValidationHelper.IsValidActivityFormat("CREATED!");   // false
    /// bool isInvalid2 = RevisionActivityValidationHelper.IsValidActivityFormat("CREATED 1");  // false
    /// </code>
    /// </example>
    public static bool IsValidActivityFormat([NotNullWhen(true)] string? activity)
    {
        if (string.IsNullOrWhiteSpace(activity))
            return false;

        var trimmed = activity.Trim();

        // Must contain at least one letter
        return trimmed.Any(char.IsLetter) &&
               // Can only contain letters, numbers, and underscores
               trimmed.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    /// <summary>
    /// Determines whether the specified activity name is reserved for system use.
    /// </summary>
    /// <param name="activity">The activity name to check. Can be null.</param>
    /// <returns>
    /// <c>true</c> if the activity name is reserved; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Reserved activity names should not be used for standard user-initiated revision operations
    /// as they are reserved for system operations and error conditions.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isReserved1 = RevisionActivityValidationHelper.IsReservedActivity("SYSTEM");    // true
    /// bool isReserved2 = RevisionActivityValidationHelper.IsReservedActivity("ERROR");     // true
    /// bool isNotReserved = RevisionActivityValidationHelper.IsReservedActivity("CREATED"); // false
    /// </code>
    /// </example>
    public static bool IsReservedActivity([NotNullWhen(true)] string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) && _reservedActivityNamesSet.Contains(activity.Trim());
    }

    #endregion Core Validation Methods

    #region Business Rule Validation Methods

    /// <summary>
    /// Validates that a revision activity is appropriate for the given revision context.
    /// </summary>
    /// <param name="activity">The activity to validate. Can be null.</param>
    /// <param name="revisionExists">Whether the revision already exists.</param>
    /// <param name="isDeleted">Whether the revision is currently deleted.</param>
    /// <returns>
    /// <c>true</c> if the activity is appropriate for the revision context; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method validates that certain activities are only used in appropriate revision contexts.
    /// 
    /// <para><strong>Business Rules by Activity:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Only valid for new revisions (revisionExists = false)</item>
    /// <item><strong>SAVED:</strong> Only valid for existing revisions (revisionExists = true)</item>
    /// <item><strong>DELETED:</strong> Only valid for existing, non-deleted revisions</item>
    /// <item><strong>RESTORED:</strong> Only valid for existing, deleted revisions</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance:</strong></para>
    /// These rules ensure proper audit trails and prevent invalid state transitions
    /// that could compromise document revision integrity.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool valid1 = RevisionActivityValidationHelper.IsActivityAppropriateForContext("CREATED", false, false); // true
    /// bool valid2 = RevisionActivityValidationHelper.IsActivityAppropriateForContext("SAVED", true, false);    // true
    /// bool valid3 = RevisionActivityValidationHelper.IsActivityAppropriateForContext("RESTORED", true, true);  // true
    /// bool invalid1 = RevisionActivityValidationHelper.IsActivityAppropriateForContext("CREATED", true, false); // false
    /// bool invalid2 = RevisionActivityValidationHelper.IsActivityAppropriateForContext("RESTORED", true, false); // false
    /// </code>
    /// </example>
    public static bool IsActivityAppropriateForContext(string? activity, bool revisionExists, bool isDeleted)
    {
        if (string.IsNullOrWhiteSpace(activity))
            return false;

        var normalizedActivity = activity.Trim().ToUpperInvariant();

        return normalizedActivity switch
        {
            "CREATED" => !revisionExists, // Can only create new revisions
            "SAVED" => revisionExists && !isDeleted, // Can only save existing, non-deleted revisions
            "DELETED" => revisionExists && !isDeleted, // Can only delete existing, non-deleted revisions
            "RESTORED" => revisionExists && isDeleted, // Can only restore existing, deleted revisions
            _ => true // Other activities are generally valid
        };
    }

    /// <summary>
    /// Validates that duplicate activities are not being created inappropriately.
    /// </summary>
    /// <param name="activity">The activity to validate. Can be null.</param>
    /// <param name="existingActivities">Collection of existing activity names.</param>
    /// <param name="allowDuplicates">Whether to allow duplicate activities.</param>
    /// <returns>
    /// <c>true</c> if the activity duplication is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="existingActivities"/> is null.</exception>
    /// <remarks>
    /// Some revision activities may be allowed to occur multiple times (e.g., SAVED),
    /// while others should typically only happen once (e.g., CREATED, DELETED).
    /// 
    /// <para><strong>Activity Duplication Rules:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Single Occurrence:</strong> CREATED, DELETED (per revision lifecycle)</item>
    /// <item><strong>Multiple Allowed:</strong> SAVED, RESTORED (can occur multiple times)</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var existing = new[] { "CREATED", "SAVED" };
    /// bool valid1 = RevisionActivityValidationHelper.IsValidActivityDuplication("SAVED", existing, true);    // true
    /// bool invalid = RevisionActivityValidationHelper.IsValidActivityDuplication("CREATED", existing, false); // false
    /// </code>
    /// </example>
    public static bool IsValidActivityDuplication(
        string? activity,
        [NotNull] IEnumerable<string> existingActivities,
        bool allowDuplicates = true)
    {
        ArgumentNullException.ThrowIfNull(existingActivities);

        if (string.IsNullOrWhiteSpace(activity))
            return false;

        var normalizedActivity = activity.Trim().ToUpperInvariant();

        // Activities that typically should only happen once per revision
        string[] singleOccurrenceActivities = ["CREATED"];

        if (allowDuplicates && !singleOccurrenceActivities.Contains(normalizedActivity)) return true;
        var existingNormalized = existingActivities
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim().ToUpperInvariant());

        return !existingNormalized.Contains(normalizedActivity, StringComparer.Ordinal);

    }

    /// <summary>
    /// Validates that a revision activity has the required user associations.
    /// </summary>
    /// <param name="userCount">Number of user associations.</param>
    /// <param name="allowEmptyUsers">Whether to allow activities without users (for system activities).</param>
    /// <returns>
    /// <c>true</c> if the user association requirements are met; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method enforces the business rule that most revision activities must have
    /// at least one user association to maintain audit trail integrity and ensure
    /// activities are properly attributed.
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Standard activities require at least one user association</item>
    /// <item>System activities may be allowed without user associations</item>
    /// <item>User count cannot be negative</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool valid1 = RevisionActivityValidationHelper.HasRequiredUserAssociations(1, false);  // true
    /// bool valid2 = RevisionActivityValidationHelper.HasRequiredUserAssociations(0, true);   // true (system activity)
    /// bool invalid = RevisionActivityValidationHelper.HasRequiredUserAssociations(0, false); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasRequiredUserAssociations(int userCount, bool allowEmptyUsers = false)
    {
        return userCount >= 0 && (allowEmptyUsers || userCount > 0);
    }

    #endregion Business Rule Validation Methods

    #region Comprehensive Validation Methods

    /// <summary>
    /// Performs comprehensive validation of an activity name including all validation rules.
    /// </summary>
    /// <param name="activity">The activity name to validate. Can be null.</param>
    /// <returns>
    /// <c>true</c> if the activity passes all validation rules; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method combines all individual validation rules into a single comprehensive check,
    /// providing a complete validation result for activity names.
    /// 
    /// <para><strong>Validation Checks Performed:</strong></para>
    /// <list type="bullet">
    /// <item>Null/empty validation</item>
    /// <item>Length constraints validation</item>
    /// <item>Character format validation</item>
    /// <item>Reserved name validation</item>
    /// <item>Allowed activity list validation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid = RevisionActivityValidationHelper.IsValidActivity("CREATED");        // true
    /// bool isInvalid1 = RevisionActivityValidationHelper.IsValidActivity("INVALID");     // false
    /// bool isInvalid2 = RevisionActivityValidationHelper.IsValidActivity("");            // false
    /// </code>
    /// </example>
    public static bool IsValidActivity(string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) &&
               IsValidActivityLength(activity) &&
               IsValidActivityFormat(activity) &&
               !IsReservedActivity(activity) &&
               IsActivityAllowed(activity);
    }

    /// <summary>
    /// Performs comprehensive validation of an activity name and returns detailed validation results.
    /// </summary>
    /// <param name="activity">The activity name to validate. Can be null.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages). Cannot be null or whitespace.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    /// <remarks>
    /// This method performs all validation checks in a single call:
    /// <list type="bullet">
    /// <item>Null/whitespace validation</item>
    /// <item>Length validation (min/max bounds)</item>
    /// <item>Format validation (character patterns)</item>
    /// <item>Reserved name validation</item>
    /// <item>Allowed activity validation</item>
    /// </list>
    /// 
    /// <para><strong>Validation Order:</strong></para>
    /// Validations are performed in order of severity, with early termination for null values.
    /// Reserved name validation is performed as an error, not a warning.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = RevisionActivityValidationHelper.ValidateActivity("INVALID_ACTIVITY", nameof(MyDto.Activity));
    /// if (results.Any())
    /// {
    ///     foreach (var result in results)
    ///     {
    ///         Console.WriteLine($"Error: {result.ErrorMessage}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<ValidationResult> ValidateActivity(
        string? activity,
        [NotNull] string propertyName)
    {
        // Validate input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Validate activity is not null or whitespace
        if (string.IsNullOrWhiteSpace(activity))
        {
            yield return new ValidationResult(
                $"{propertyName} is required and cannot be empty.",
                [propertyName]);
            yield break; // No point in further validation
        }

        var trimmedActivity = activity.Trim();

        switch (trimmedActivity.Length)
        {
            // Validate length constraints
            case < MinActivityLength:
                yield return new ValidationResult(
                    $"{propertyName} must be at least {MinActivityLength} character long.",
                    [propertyName]);
                break;
            case > MaxActivityLength:
                yield return new ValidationResult(
                    $"{propertyName} cannot exceed {MaxActivityLength} characters.",
                    [propertyName]);
                break;
        }

        // Validate character format
        if (!IsValidActivityFormat(activity))
        {
            yield return new ValidationResult(
                $"{propertyName} must contain only letters, numbers, and underscores, and include at least one letter.",
                [propertyName]);
        }

        // Check for reserved names
        if (IsReservedActivity(activity))
        {
            yield return new ValidationResult(
                $"{propertyName} uses a reserved activity name. Reserved names: {ReservedActivitiesList}.",
                [propertyName]);
        }

        // Validate against allowed activities
        if (!IsActivityAllowed(activity))
        {
            yield return new ValidationResult(
                $"{propertyName} must be one of the following allowed activities: {AllowedActivitiesList}.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Performs comprehensive validation of an activity ID.
    /// </summary>
    /// <param name="activityId">The activity ID to validate.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages). Cannot be null or whitespace.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    /// <remarks>
    /// This method validates that the activity ID represents a valid, non-empty GUID suitable
    /// for use as a database identifier.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = RevisionActivityValidationHelper.ValidateActivityId(Guid.Empty, nameof(MyDto.ActivityId));
    /// if (results.Any())
    /// {
    ///     Console.WriteLine($"Activity ID validation failed: {string.Join(", ", results.Select(r => r.ErrorMessage))}");
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<ValidationResult> ValidateActivityId(
        Guid activityId,
        [NotNull] string propertyName)
    {
        // Validate input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Validate GUID is not empty
        if (activityId == Guid.Empty)
        {
            yield return new ValidationResult(
                $"{propertyName} must be a valid non-empty GUID.",
                [propertyName]);
        }
    }

    #endregion Comprehensive Validation Methods

    #region Normalization and Formatting Methods

    /// <summary>
    /// Normalizes an activity name for consistent storage and comparison.
    /// </summary>
    /// <param name="activity">The activity name to normalize. Can be null.</param>
    /// <returns>
    /// The normalized activity name, or null if the input is invalid.
    /// </returns>
    /// <remarks>
    /// Normalization includes trimming whitespace and converting to uppercase
    /// for consistent storage and comparison operations.
    /// 
    /// <para><strong>Normalization Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Trims leading and trailing whitespace</item>
    /// <item>Converts to uppercase using invariant culture</item>
    /// <item>Returns null for null or whitespace-only input</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// string? normalized1 = RevisionActivityValidationHelper.NormalizeActivity("  created  "); // "CREATED"
    /// string? normalized2 = RevisionActivityValidationHelper.NormalizeActivity("Saved");        // "SAVED"
    /// string? normalized3 = RevisionActivityValidationHelper.NormalizeActivity("");             // null
    /// string? normalized4 = RevisionActivityValidationHelper.NormalizeActivity(null);           // null
    /// </code>
    /// </example>
    [return: NotNullIfNotNull(nameof(activity))]
    public static string? NormalizeActivity(string? activity)
    {
        return string.IsNullOrWhiteSpace(activity) ? null : activity.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Checks if two activity names are equivalent after normalization.
    /// </summary>
    /// <param name="activity1">The first activity name. Can be null.</param>
    /// <param name="activity2">The second activity name. Can be null.</param>
    /// <returns>
    /// <c>true</c> if the activity names are equivalent; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is useful for comparing activity names in a case-insensitive manner
    /// while handling whitespace differences.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool same1 = RevisionActivityValidationHelper.AreActivitiesEquivalent("CREATED", "created");     // true
    /// bool same2 = RevisionActivityValidationHelper.AreActivitiesEquivalent("  CREATED  ", "CREATED");  // true
    /// bool different = RevisionActivityValidationHelper.AreActivitiesEquivalent("CREATED", "SAVED");   // false
    /// bool bothNull = RevisionActivityValidationHelper.AreActivitiesEquivalent(null, null);            // false
    /// </code>
    /// </example>
    public static bool AreActivitiesEquivalent(string? activity1, string? activity2)
    {
        var normalized1 = NormalizeActivity(activity1);
        var normalized2 = NormalizeActivity(activity2);

        return !string.IsNullOrEmpty(normalized1) &&
               !string.IsNullOrEmpty(normalized2) &&
               string.Equals(normalized1, normalized2, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the list of allowed activities as a formatted, comma-separated string.
    /// </summary>
    /// <returns>
    /// A string containing all allowed activities separated by commas and spaces.
    /// </returns>
    /// <remarks>
    /// This property is useful for error messages and documentation where a human-readable
    /// list of allowed activities is needed.
    /// </remarks>
    /// <example>
    /// Returns: "CREATED, DELETED, RESTORED, SAVED"
    /// </example>
    public static string AllowedActivitiesList => string.Join(", ", AllowedActivities);

    /// <summary>
    /// Gets the list of reserved activities as a formatted, comma-separated string.
    /// </summary>
    /// <returns>
    /// A string containing all reserved activities separated by commas and spaces.
    /// </returns>
    /// <remarks>
    /// This property is useful for error messages and documentation where a human-readable
    /// list of reserved activities is needed.
    /// </remarks>
    public static string ReservedActivitiesList => string.Join(", ", ReservedActivityNames);

    #endregion Normalization and Formatting Methods

    #region Suggestion and Utility Methods

    /// <summary>
    /// Suggests alternative activity names if the provided activity is not allowed.
    /// </summary>
    /// <param name="attemptedActivity">The attempted activity name. Can be null.</param>
    /// <param name="categoryHint">Optional category hint for better suggestions (lifecycle).</param>
    /// <param name="maxSuggestions">Maximum number of suggestions to return.</param>
    /// <returns>
    /// A read-only list of suggested alternative activity names.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxSuggestions"/> is less than 1.</exception>
    /// <remarks>
    /// This method provides helpful suggestions when an activity name validation fails,
    /// improving user experience by offering valid alternatives based on the context.
    /// 
    /// <para><strong>Suggestion Strategies:</strong></para>
    /// <list type="bullet">
    /// <item>Find activities containing similar text</item>
    /// <item>Provide category-specific suggestions</item>
    /// <item>Offer all available activities for the limited set</item>
    /// <item>Prioritize exact matches and partial matches</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var suggestions = RevisionActivityValidationHelper.SuggestAlternativeActivities("CREATE", "lifecycle", 3);
    /// // Returns lifecycle-related activities like "CREATED", "SAVED", "DELETED"
    /// </code>
    /// </example>
    public static IReadOnlyList<string> SuggestAlternativeActivities(
        string? attemptedActivity,
        string? categoryHint = null,
        int maxSuggestions = MaxActivitySuggestions)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxSuggestions, 1);

        var suggestions = new List<string>();

        if (string.IsNullOrWhiteSpace(attemptedActivity))
        {
            // Return all activities if no input provided (small set)
            return AllowedActivities.Take(maxSuggestions).ToImmutableArray();
        }

        var normalized = attemptedActivity.Trim().ToUpperInvariant();

        // Find activities that contain similar text or are contained in the attempted activity
        var similarActivities = AllowedActivities
            .Where(activity =>
                activity.Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains(activity, StringComparison.OrdinalIgnoreCase))
            .Take(maxSuggestions)
            .ToList();

        if (similarActivities.Count < maxSuggestions && !string.IsNullOrEmpty(categoryHint))
        {
            // Add category-specific suggestions
            var categorySpecificActivities = GetActivitiesByCategory(categoryHint)
                .Except(similarActivities)
                .Take(maxSuggestions - similarActivities.Count);

            similarActivities.AddRange(categorySpecificActivities);
        }

        if (similarActivities.Count >= maxSuggestions) return similarActivities.Take(maxSuggestions).ToImmutableArray();
        // Fill remaining slots with all available activities
        var remainingActivities = AllowedActivities
            .Except(similarActivities)
            .Take(maxSuggestions - similarActivities.Count);

        similarActivities.AddRange(remainingActivities);

        return similarActivities.Take(maxSuggestions).ToImmutableArray();
    }

    /// <summary>
    /// Gets activities by category for contextual suggestions.
    /// </summary>
    /// <param name="category">The category (lifecycle). Can be null.</param>
    /// <returns>A collection of activities for the specified category.</returns>
    /// <remarks>
    /// This method categorizes activities by their purpose to provide
    /// contextually relevant suggestions.
    /// 
    /// <para><strong>Available Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>lifecycle:</strong> CREATED, SAVED, DELETED, RESTORED</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var lifecycleActivities = RevisionActivityValidationHelper.GetActivitiesByCategory("lifecycle");
    /// // Returns: ["CREATED", "DELETED", "RESTORED", "SAVED"]
    /// </code>
    /// </example>
    public static IEnumerable<string> GetActivitiesByCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return [];

        return category.ToLowerInvariant() switch
        {
            "lifecycle" => ["CREATED", "SAVED", "DELETED", "RESTORED"],
            _ => []
        };
    }

    #endregion Suggestion and Utility Methods

    #region Diagnostic and Statistics Methods

    /// <summary>
    /// Gets statistical information about activity validation rules.
    /// </summary>
    /// <returns>
    /// A dictionary containing statistical information about the validation system.
    /// </returns>
    /// <remarks>
    /// This method provides insights into the validation system configuration,
    /// useful for monitoring and diagnostics.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = RevisionActivityValidationHelper.GetValidationStatistics();
    /// Console.WriteLine($"Total allowed activities: {stats["TotalAllowedActivities"]}");
    /// Console.WriteLine($"Total reserved activities: {stats["TotalReservedActivities"]}");
    /// </code>
    /// </example>
    public static IReadOnlyDictionary<string, object> GetValidationStatistics()
    {
        return new Dictionary<string, object>
        {
            ["TotalAllowedActivities"] = AllowedActivities.Count,
            ["TotalReservedActivities"] = ReservedActivityNames.Count,
            ["MaxActivityLength"] = MaxActivityLength,
            ["MinActivityLength"] = MinActivityLength,
            ["MaxActivitySuggestions"] = MaxActivitySuggestions,
            ["AverageActivityLength"] = AllowedActivities.Average(a => a.Length),
            ["LongestActivityName"] = AllowedActivities.OrderByDescending(a => a.Length).First(),
            ["ShortestActivityName"] = AllowedActivities.OrderBy(a => a.Length).First(),
            ["ActivityCategories"] = new Dictionary<string, int>
            {
                ["Lifecycle"] = GetActivitiesByCategory("lifecycle").Count()
            }
        }.ToImmutableDictionary();
    }

    /// <summary>
    /// Generates a comprehensive validation report for an activity name.
    /// </summary>
    /// <param name="activity">The activity name to analyze. Can be null.</param>
    /// <returns>
    /// A formatted string containing detailed validation information.
    /// </returns>
    /// <remarks>
    /// This method provides a human-readable validation report useful for debugging
    /// and troubleshooting activity validation issues.
    /// 
    /// <para><strong>Report Contents:</strong></para>
    /// <list type="bullet">
    /// <item>Detailed validation results for each rule</item>
    /// <item>Normalized activity name</item>
    /// <item>Overall validation result</item>
    /// <item>Suggestions for invalid activities</item>
    /// <item>Available activity categories</item>
    /// <item>Reserved activity names</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// string report = RevisionActivityValidationHelper.GenerateValidationReport("INVALID_ACTIVITY");
    /// Console.WriteLine(report);
    /// // Outputs detailed validation results and suggestions
    /// </code>
    /// </example>
    public static string GenerateValidationReport(string? activity)
    {
        var report = new StringBuilder();
        report.AppendLine($"Revision Activity Validation Report for: '{activity ?? "<null>"}'");
        report.AppendLine(new string('=', 55));

        var results = GetDetailedValidationResults(activity);
        foreach (var result in results)
        {
            var status = result.Value ? "✓ PASS" : "✗ FAIL";
            report.AppendLine($"{result.Key}: {status}");
        }

        report.AppendLine();
        report.AppendLine($"Normalized Activity: '{NormalizeActivity(activity) ?? "<invalid>"}'");
        report.AppendLine($"Overall Result: {(IsValidActivity(activity) ? "VALID" : "INVALID")}");

        if (!IsValidActivity(activity))
        {
            report.AppendLine();
            report.AppendLine("Suggestions:");
            var suggestions = SuggestAlternativeActivities(activity, maxSuggestions: 3);
            foreach (var suggestion in suggestions)
            {
                report.AppendLine($"  - {suggestion}");
            }
        }

        report.AppendLine();
        report.AppendLine("Activity Categories:");
        report.AppendLine($"  Lifecycle: {string.Join(", ", GetActivitiesByCategory("lifecycle"))}");

        if (ReservedActivityNames.Count <= 0) return report.ToString();
        report.AppendLine();
        report.AppendLine($"Reserved Activities: {ReservedActivitiesList}");

        return report.ToString();
    }

    /// <summary>
    /// Validates an activity name and returns detailed validation results.
    /// </summary>
    /// <param name="activity">The activity name to validate. Can be null.</param>
    /// <returns>
    /// A dictionary containing detailed validation results for each validation rule.
    /// </returns>
    /// <remarks>
    /// This method provides detailed diagnostic information about activity validation,
    /// useful for troubleshooting validation failures and providing specific error messages.
    /// 
    /// <para><strong>Validation Rules Checked:</strong></para>
    /// <list type="bullet">
    /// <item>IsNotNullOrEmpty: Activity is not null, empty, or whitespace</item>
    /// <item>HasValidLength: Activity length is within bounds</item>
    /// <item>HasValidFormat: Activity contains valid characters</item>
    /// <item>IsNotReserved: Activity is not in reserved names list</item>
    /// <item>IsInAllowedList: Activity is in the allowed activities list</item>
    /// <item>PassesAllRules: Overall validation result</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = RevisionActivityValidationHelper.GetDetailedValidationResults("CREATED");
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"{result.Key}: {result.Value}");
    /// }
    /// </code>
    /// </example>
    public static IReadOnlyDictionary<string, bool> GetDetailedValidationResults(string? activity)
    {
        return new Dictionary<string, bool>
        {
            ["IsNotNullOrEmpty"] = !string.IsNullOrWhiteSpace(activity),
            ["HasValidLength"] = IsValidActivityLength(activity),
            ["HasValidFormat"] = IsValidActivityFormat(activity),
            ["IsNotReserved"] = !IsReservedActivity(activity),
            ["IsInAllowedList"] = IsActivityAllowed(activity),
            ["PassesAllRules"] = IsValidActivity(activity)
        }.ToImmutableDictionary();
    }

    #endregion Diagnostic and Statistics Methods
}