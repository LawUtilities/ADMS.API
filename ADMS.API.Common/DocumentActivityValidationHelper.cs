using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ADMS.API.Common;

/// <summary>
/// Provides comprehensive helper methods and constants for validating document activity types within the ADMS system.
/// </summary>
/// <remarks>
/// This static helper class provides robust document activity validation functionality for the ADMS legal document 
/// management system, supporting DocumentActivityDto and DocumentActivityMinimalDto validation. The validation methods 
/// ensure data integrity, business rule compliance, and consistent validation logic across the application.
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item>Activity name validation against predefined allowed activities from database seed data</item>
/// <item>GUID validation for activity IDs with proper empty value checking</item>
/// <item>Length validation consistent with database constraints (StringLength(50))</item>
/// <item>Case-insensitive validation with normalization support</item>
/// <item>High-performance validation using frozen collections for O(1) lookup performance</item>
/// <item>Thread-safe operations optimized for concurrent access scenarios</item>
/// <item>Integration with standard .NET validation infrastructure</item>
/// </list>
/// 
/// <para><strong>Document Activity Categories:</strong></para>
/// <list type="bullet">
/// <item><strong>Lifecycle Operations:</strong> CREATED, DELETED, RESTORED, SAVED</item>
/// <item><strong>Check-in/Check-out Operations:</strong> CHECKED IN, CHECKED OUT</item>
/// </list>
/// 
/// <para><strong>Validation Process:</strong></para>
/// <list type="number">
/// <item>Validates activity name format and length constraints</item>
/// <item>Normalizes activity names to uppercase with trimming</item>
/// <item>Validates against approved activity list from database seed data</item>
/// <item>Performs GUID validation for activity identifiers</item>
/// <item>Returns detailed validation results for comprehensive error reporting</item>
/// </list>
/// 
/// The helper follows established ADMS validation patterns and integrates seamlessly with
/// the existing validation infrastructure, including centralized validation services and
/// standardized error reporting mechanisms.
/// 
/// <para><strong>Database Integration:</strong></para>
/// The allowed activities list is synchronized with the DocumentActivity seed data in AdmsContext.cs,
/// ensuring consistency between validation logic and database constraints.
/// 
/// <para><strong>Thread Safety:</strong></para>
/// All methods in this class are thread-safe and use immutable frozen collections for optimal
/// performance in concurrent scenarios without external synchronization.
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// Uses FrozenSet for O(1) average lookup performance and minimal memory allocation.
/// All validation operations are optimized for high-frequency usage in API scenarios.
/// </remarks>
public static class DocumentActivityValidationHelper
{
    #region Constants

    /// <summary>
    /// The maximum allowed length for a document activity name.
    /// </summary>
    /// <remarks>
    /// This value matches the StringLength(50) constraint on the DocumentActivity.Activity property 
    /// in the ADMS.API.Entities.DocumentActivity entity to ensure consistency between validation 
    /// logic and database constraints.
    /// </remarks>
    public const int MaxActivityLength = 50;

    /// <summary>
    /// The minimum allowed length for a document activity name.
    /// </summary>
    /// <remarks>
    /// Minimum length ensures activity names are meaningful and not just single characters
    /// or empty strings after trimming.
    /// </remarks>
    public const int MinActivityLength = 2;

    /// <summary>
    /// The list of allowed document activity names synchronized with database seed data.
    /// All values are uppercase and unique, corresponding to the DocumentActivity seed data in AdmsContext.cs.
    /// </summary>
    /// <remarks>
    /// These activity types represent the standard document operations in the ADMS legal document 
    /// management system. Activities describe actions taken on documents throughout their lifecycle.
    /// 
    /// <para><strong>Activity Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Lifecycle Operations:</strong> CREATED, DELETED, RESTORED, SAVED</item>
    /// <item><strong>Version Control Operations:</strong> CHECKED IN, CHECKED OUT</item>
    /// </list>
    /// 
    /// <para><strong>Database Synchronization:</strong></para>
    /// This list must be kept in sync with the DocumentActivity seed data in 
    /// DbContexts/AdmsContext.SeedDocumentActivities() method to ensure validation consistency.
    /// 
    /// <para><strong>Modification Guidelines:</strong></para>
    /// When adding new activities:
    /// <list type="number">
    /// <item>Add the activity to the database seed data first</item>
    /// <item>Update this list to match the seed data</item>
    /// <item>Ensure activity names are uppercase and descriptive</item>
    /// <item>Follow existing naming conventions for consistency</item>
    /// <item>Update documentation and tests accordingly</item>
    /// </list>
    /// </remarks>
    private static readonly string[] _allowedActivitiesArray =
    [
        "CHECKED IN",
        "CHECKED OUT",
        "CREATED",
        "DELETED",
        "RESTORED",
        "SAVED"
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
    /// Gets the read-only list of allowed document activity names.
    /// All values are uppercase and unique.
    /// </summary>
    /// <remarks>
    /// Returns a read-only view of the allowed activities for external consumption.
    /// This property provides thread-safe access to the activities list.
    /// </remarks>
    public static IReadOnlyList<string> AllowedActivities => _allowedActivitiesArray;

    /// <summary>
    /// The list of reserved activity names that should not be used for standard document operations.
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
    /// <item><strong>Cleanup Operations:</strong> PURGE, DELETE (system-level)</item>
    /// </list>
    /// </remarks>
    public static readonly IReadOnlyList<string> ReservedActivityNames =
    [
        "SYSTEM",
        "ADMIN",
        "AUTO",
        "BATCH",
        "MIGRATION",
        "IMPORT",
        "EXPORT",
        "DELETE", // Different from "DELETED" - this is for system-level deletions
        "PURGE",
        "CORRUPT",
        "ERROR",
        "FAILED"
    ];

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
    /// bool isValid1 = DocumentActivityValidationHelper.IsActivityAllowed("CREATED");        // true
    /// bool isValid2 = DocumentActivityValidationHelper.IsActivityAllowed("created");        // true (case insensitive)
    /// bool isValid3 = DocumentActivityValidationHelper.IsActivityAllowed(" SAVED ");        // true (ignores whitespace)
    /// bool isInvalid1 = DocumentActivityValidationHelper.IsActivityAllowed("INVALID");      // false
    /// bool isInvalid2 = DocumentActivityValidationHelper.IsActivityAllowed("");             // false
    /// bool isInvalid3 = DocumentActivityValidationHelper.IsActivityAllowed(null);           // false
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
    /// bool isValid = DocumentActivityValidationHelper.IsValidActivityId(Guid.NewGuid());  // true
    /// bool isInvalid = DocumentActivityValidationHelper.IsValidActivityId(Guid.Empty);    // false
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
    /// <item>Minimum length: {MinActivityLength} characters (after trimming)</item>
    /// <item>Maximum length: {MaxActivityLength} characters (matches database constraint)</item>
    /// <item>Null or whitespace strings return false</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid = DocumentActivityValidationHelper.IsValidActivityLength("CREATED");     // true
    /// bool isTooShort = DocumentActivityValidationHelper.IsValidActivityLength("A");        // false (too short)
    /// bool isTooLong = DocumentActivityValidationHelper.IsValidActivityLength(new string('A', 51)); // false (too long)
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
    /// Determines whether the specified activity name is reserved for system use.
    /// </summary>
    /// <param name="activity">The activity name to check. Can be null.</param>
    /// <returns>
    /// <c>true</c> if the activity name is reserved; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Reserved activity names should not be used for standard user-initiated document operations
    /// as they are reserved for system operations and error conditions.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isReserved1 = DocumentActivityValidationHelper.IsReservedActivity("SYSTEM");     // true
    /// bool isReserved2 = DocumentActivityValidationHelper.IsReservedActivity("ERROR");      // true
    /// bool isNotReserved = DocumentActivityValidationHelper.IsReservedActivity("CREATED");  // false
    /// </code>
    /// </example>
    public static bool IsReservedActivity([NotNullWhen(true)] string? activity)
    {
        return !string.IsNullOrWhiteSpace(activity) && ReservedActivityNames.Contains(activity.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    #endregion Core Validation Methods

    #region Normalization and Formatting Methods

    /// <summary>
    /// Returns the normalized (uppercase, trimmed) version of an activity name.
    /// </summary>
    /// <param name="activity">The activity name to normalize. Can be null.</param>
    /// <returns>
    /// The normalized activity name, or <c>null</c> if input is null or consists only of whitespace.
    /// </returns>
    /// <remarks>
    /// Normalization ensures consistent activity name format across the system for storage,
    /// comparison, and display purposes.
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
    /// string? normalized1 = DocumentActivityValidationHelper.NormalizeActivity("  created  ");  // "CREATED"
    /// string? normalized2 = DocumentActivityValidationHelper.NormalizeActivity("Saved");         // "SAVED"
    /// string? normalized3 = DocumentActivityValidationHelper.NormalizeActivity("");              // null
    /// string? normalized4 = DocumentActivityValidationHelper.NormalizeActivity(null);            // null
    /// </code>
    /// </example>
    [return: NotNullIfNotNull(nameof(activity))]
    public static string? NormalizeActivity(string? activity)
    {
        return string.IsNullOrWhiteSpace(activity) ? null : activity.Trim().ToUpperInvariant();
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
    /// Returns: "CHECKED IN, CHECKED OUT, CREATED, DELETED, RESTORED, SAVED"
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

    #region Comprehensive Validation Methods

    /// <summary>
    /// Performs comprehensive validation of a document activity name including all validation rules.
    /// </summary>
    /// <param name="activity">The activity name to validate comprehensively. Can be null.</param>
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
    /// <item>Allowed activity validation</item>
    /// <item>Reserved name validation</item>
    /// </list>
    /// 
    /// <para><strong>Validation Order:</strong></para>
    /// Validations are performed in order of severity, with early termination for null values.
    /// Reserved name validation is performed as a warning, not an error.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = DocumentActivityValidationHelper.ValidateActivity("INVALID_ACTIVITY", nameof(MyDto.Activity));
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
                    $"{propertyName} must be at least {MinActivityLength} characters long.",
                    [propertyName]);
                break;
            case > MaxActivityLength:
                yield return new ValidationResult(
                    $"{propertyName} cannot exceed {MaxActivityLength} characters.",
                    [propertyName]);
                break;
        }

        // Validate against allowed activities
        if (!IsActivityAllowed(activity))
        {
            yield return new ValidationResult(
                $"{propertyName} must be one of the following allowed activities: {AllowedActivitiesList}.",
                [propertyName]);
        }

        // Check for reserved names (warning-level validation)
        if (IsReservedActivity(activity))
        {
            yield return new ValidationResult(
                $"{propertyName} uses a reserved activity name. Reserved names: {ReservedActivitiesList}.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Performs comprehensive validation of a document activity ID.
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
    /// var results = DocumentActivityValidationHelper.ValidateActivityId(Guid.Empty, nameof(MyDto.ActivityId));
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
}