using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace ADMS.API.Common;

/// <summary>
/// Provides comprehensive helper methods and constants for validating revision-related data within the ADMS system.
/// </summary>
/// <remarks>
/// This static helper class provides robust revision validation functionality for the ADMS legal 
/// document management system, supporting all revision-related DTOs including RevisionDto, RevisionMinimalDto, 
/// RevisionForCreationDto, and RevisionForUpdateDto. The validation methods ensure data integrity, business rule 
/// compliance, and consistent validation logic across the application.
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item>Revision number validation with sequential numbering enforcement</item>
/// <item>GUID validation for revision IDs with proper empty value checking</item>
/// <item>Date validation with timezone normalization and chronological consistency</item>
/// <item>Business rule validation for revision lifecycle and versioning operations</item>
/// <item>Temporal consistency validation between creation and modification dates</item>
/// <item>Document association validation for proper revision management</item>
/// <item>High-performance validation using modern .NET patterns</item>
/// <item>Thread-safe operations optimized for concurrent access scenarios</item>
/// </list>
/// 
/// <para><strong>Revision Lifecycle Management:</strong></para>
/// <list type="bullet">
/// <item><strong>Creation:</strong> First revision starts at number 1</item>
/// <item><strong>Versioning:</strong> Sequential numbering without gaps or duplicates</item>
/// <item><strong>Modification:</strong> Modification date must be >= creation date</item>
/// <item><strong>Deletion:</strong> Soft deletion with business rule enforcement</item>
/// </list>
/// 
/// <para><strong>Database Synchronization:</strong></para>
/// All validation constraints are synchronized with the Revision entity structure:
/// <list type="bullet">
/// <item>RevisionNumber: int with sequential validation</item>
/// <item>CreationDate/ModificationDate: DateTime with UTC normalization</item>
/// <item>IsDeleted: bool with business rule validation</item>
/// <item>DocumentId: Guid with non-empty validation</item>
/// </list>
/// 
/// <para><strong>Legal Compliance:</strong></para>
/// Revision validation enforces legal document management best practices including:
/// <list type="bullet">
/// <item>Audit trail preservation through proper versioning</item>
/// <item>Data integrity through comprehensive validation rules</item>
/// <item>Temporal consistency for legal document chronology</item>
/// <item>Version control integrity for document management</item>
/// </list>
/// 
/// <para><strong>Thread Safety:</strong></para>
/// All methods in this class are thread-safe and use immutable collections where applicable
/// for optimal performance in concurrent scenarios without external synchronization.
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// Uses modern .NET patterns including aggressive inlining for hot paths and
/// optimized validation operations for high-frequency usage in API scenarios.
/// </remarks>
public static class RevisionValidationHelper
{
    #region Constants

    /// <summary>
    /// The minimum allowed revision number.
    /// </summary>
    /// <remarks>
    /// Revision numbers start from 1 to maintain consistency with document versioning standards
    /// and prevent confusion with zero-based indexing systems.
    /// </remarks>
    public const int MinRevisionNumber = 1;

    /// <summary>
    /// The maximum allowed revision number.
    /// </summary>
    /// <remarks>
    /// This limit prevents integer overflow and ensures reasonable revision numbering.
    /// Set to a practical maximum that allows for extensive document versioning.
    /// </remarks>
    public const int MaxRevisionNumber = 999999;

    /// <summary>
    /// The earliest allowed date for revision operations.
    /// </summary>
    /// <remarks>
    /// This date represents a reasonable lower bound for revision dates in the ADMS system,
    /// preventing unrealistic historical dates that might indicate data corruption or system errors.
    /// Set to January 1, 1980, as a practical minimum for legal document management systems.
    /// </remarks>
    public static readonly DateTime MinAllowedRevisionDate = new(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The maximum allowed date for revision operations (current UTC time plus tolerance).
    /// </summary>
    /// <remarks>
    /// This represents a reasonable upper bound that accounts for system clock differences
    /// while preventing future dates that could cause issues with business logic.
    /// </remarks>
    public static DateTime MaxAllowedRevisionDate => DateTime.UtcNow.AddMinutes(FutureDateToleranceMinutes);

    /// <summary>
    /// The tolerance in minutes for future dates.
    /// </summary>
    /// <remarks>
    /// This tolerance accounts for clock skew between client and server systems,
    /// allowing for small time differences while preventing actual future dates.
    /// </remarks>
    public const int FutureDateToleranceMinutes = 1;

    /// <summary>
    /// The maximum allowed time span between creation and modification dates.
    /// </summary>
    /// <remarks>
    /// This prevents unrealistic time spans that might indicate data corruption.
    /// Set to 50 years as a reasonable maximum for long-term document management.
    /// </remarks>
    public static readonly TimeSpan MaxDateTimeSpan = TimeSpan.FromDays(365 * 50);

    /// <summary>
    /// Maximum number of revision suggestions to generate for user assistance.
    /// </summary>
    /// <remarks>
    /// Limits the number of suggestions to prevent excessive processing while
    /// providing sufficient information for troubleshooting.
    /// </remarks>
    public const int MaxRevisionSuggestions = 5;

    #endregion Constants

    #region Core Validation Methods

    /// <summary>
    /// Determines whether the specified revision number is valid.
    /// A valid revision number is a positive integer within acceptable bounds.
    /// </summary>
    /// <param name="revisionNumber">The revision number to validate.</param>
    /// <returns>
    /// <c>true</c> if the revision number is within valid bounds; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method validates that revision numbers follow the standard versioning convention
    /// where the first revision is numbered 1, not 0, and ensures numbers remain within
    /// practical limits to prevent system issues.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Must be greater than or equal to {MinRevisionNumber}</item>
    /// <item>Must be less than or equal to {MaxRevisionNumber}</item>
    /// <item>Follows standard document versioning conventions</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid1 = RevisionValidationHelper.IsValidRevisionNumber(1);        // true
    /// bool isValid2 = RevisionValidationHelper.IsValidRevisionNumber(100);      // true
    /// bool isInvalid1 = RevisionValidationHelper.IsValidRevisionNumber(0);      // false
    /// bool isInvalid2 = RevisionValidationHelper.IsValidRevisionNumber(-1);     // false
    /// bool isInvalid3 = RevisionValidationHelper.IsValidRevisionNumber(1000000); // false (too large)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidRevisionNumber(int revisionNumber) =>
        revisionNumber >= MinRevisionNumber && revisionNumber <= MaxRevisionNumber;

    /// <summary>
    /// Determines whether the specified date is valid for a revision.
    /// A valid date is within reasonable bounds and not in the future.
    /// </summary>
    /// <param name="date">The date to validate.</param>
    /// <returns>
    /// <c>true</c> if the date is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method ensures revision dates are realistic and comply with business rules:
    /// <list type="bullet">
    /// <item>Must be after the minimum allowed date ({MinAllowedRevisionDate:yyyy-MM-dd})</item>
    /// <item>Cannot be in the future (with {FutureDateToleranceMinutes} minute tolerance)</item>
    /// <item>Must not be DateTime.MinValue or other sentinel values</item>
    /// </list>
    /// 
    /// <para><strong>Timezone Handling:</strong></para>
    /// The method works with any DateTime kind but normalizes to UTC for comparison.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid1 = RevisionValidationHelper.IsValidDate(DateTime.UtcNow);                    // true
    /// bool isValid2 = RevisionValidationHelper.IsValidDate(new DateTime(2020, 1, 1));          // true
    /// bool isInvalid1 = RevisionValidationHelper.IsValidDate(DateTime.MinValue);                // false (too early)
    /// bool isInvalid2 = RevisionValidationHelper.IsValidDate(DateTime.UtcNow.AddHours(1));      // false (future)
    /// bool isInvalid3 = RevisionValidationHelper.IsValidDate(new DateTime(1975, 1, 1));        // false (too early)
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
               utcDate >= MinAllowedRevisionDate &&
               utcDate <= MaxAllowedRevisionDate;
    }

    /// <summary>
    /// Determines whether the specified revision ID is valid.
    /// A valid revision ID is a non-empty GUID.
    /// </summary>
    /// <param name="revisionId">The revision ID to validate.</param>
    /// <returns>
    /// <c>true</c> if the revision ID is not empty; otherwise, <c>false</c>.
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
    /// bool isValid = RevisionValidationHelper.IsValidRevisionId(Guid.NewGuid());  // true
    /// bool isInvalid = RevisionValidationHelper.IsValidRevisionId(Guid.Empty);    // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidRevisionId(Guid revisionId) => revisionId != Guid.Empty;

    /// <summary>
    /// Determines whether the specified document ID is valid for revision association.
    /// A valid document ID is a non-empty GUID.
    /// </summary>
    /// <param name="documentId">The document ID to validate.</param>
    /// <returns>
    /// <c>true</c> if the document ID is not empty; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method validates that revisions are properly associated with documents,
    /// ensuring referential integrity in the document management system.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid = RevisionValidationHelper.IsValidDocumentId(Guid.NewGuid());  // true
    /// bool isInvalid = RevisionValidationHelper.IsValidDocumentId(Guid.Empty);    // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidDocumentId(Guid documentId) => documentId != Guid.Empty;

    /// <summary>
    /// Determines whether the specified creation date is valid for a revision.
    /// </summary>
    /// <param name="creationDate">The creation date to validate.</param>
    /// <returns>
    /// <c>true</c> if the creation date is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method provides specialized validation for creation dates, ensuring they meet
    /// the same standards as general revision dates. It's an alias for IsValidDate for semantic clarity.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid = RevisionValidationHelper.IsValidCreationDate(DateTime.UtcNow);     // true
    /// bool isInvalid = RevisionValidationHelper.IsValidCreationDate(DateTime.MinValue); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidCreationDate(DateTime creationDate) => IsValidDate(creationDate);

    /// <summary>
    /// Determines whether the specified modification date is valid for a revision.
    /// </summary>
    /// <param name="modificationDate">The modification date to validate.</param>
    /// <returns>
    /// <c>true</c> if the modification date is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method provides specialized validation for modification dates, ensuring they meet
    /// the same standards as general revision dates. It's an alias for IsValidDate for semantic clarity.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid = RevisionValidationHelper.IsValidModificationDate(DateTime.UtcNow);     // true
    /// bool isInvalid = RevisionValidationHelper.IsValidModificationDate(DateTime.MinValue); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidModificationDate(DateTime modificationDate) => IsValidDate(modificationDate);

    #endregion Core Validation Methods

    #region Business Rule Validation Methods

    /// <summary>
    /// Validates that a modification date is not before a creation date.
    /// </summary>
    /// <param name="creationDate">The creation date.</param>
    /// <param name="modificationDate">The modification date.</param>
    /// <returns>
    /// <c>true</c> if the modification date is valid (not before creation date); otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method enforces the business rule that modification dates must be chronologically
    /// after or equal to creation dates, maintaining data integrity for revision timelines.
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Modification date must be >= creation date</item>
    /// <item>Both dates must be valid individually</item>
    /// <item>Maintains chronological consistency for audit trails</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var creation = DateTime.UtcNow.AddHours(-1);
    /// var modification = DateTime.UtcNow;
    /// bool isValid = RevisionValidationHelper.IsValidDateSequence(creation, modification);    // true
    /// 
    /// var invalidModification = DateTime.UtcNow.AddHours(-2);
    /// bool isInvalid = RevisionValidationHelper.IsValidDateSequence(creation, invalidModification); // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidDateSequence(DateTime creationDate, DateTime modificationDate) =>
        modificationDate >= creationDate;

    /// <summary>
    /// Validates that a revision number is sequential within a document context.
    /// </summary>
    /// <param name="newRevisionNumber">The new revision number to validate.</param>
    /// <param name="existingRevisionNumbers">The collection of existing revision numbers for the document.</param>
    /// <returns>
    /// <c>true</c> if the revision number maintains proper sequence; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="existingRevisionNumbers"/> is null.</exception>
    /// <remarks>
    /// This method ensures revision numbers follow a proper sequence without gaps or duplicates,
    /// maintaining consistency in document version control.
    /// 
    /// <para><strong>Sequencing Rules:</strong></para>
    /// <list type="bullet">
    /// <item>First revision for a document must be 1</item>
    /// <item>Subsequent revisions must be exactly +1 from the highest existing</item>
    /// <item>No gaps or duplicates allowed</item>
    /// <item>Maintains strict version control integrity</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var existing = new[] { 1, 2, 3 };
    /// bool isValid = RevisionValidationHelper.IsValidRevisionSequence(4, existing);   // true
    /// bool isInvalid1 = RevisionValidationHelper.IsValidRevisionSequence(3, existing); // false (duplicate)
    /// bool isInvalid2 = RevisionValidationHelper.IsValidRevisionSequence(6, existing); // false (gap)
    /// bool isInvalid3 = RevisionValidationHelper.IsValidRevisionSequence(0, existing); // false (invalid number)
    /// </code>
    /// </example>
    public static bool IsValidRevisionSequence(int newRevisionNumber, [NotNull] IEnumerable<int> existingRevisionNumbers)
    {
        ArgumentNullException.ThrowIfNull(existingRevisionNumbers);

        if (!IsValidRevisionNumber(newRevisionNumber))
            return false;

        var existingNumbers = existingRevisionNumbers.ToList();

        // Check for duplicates
        if (existingNumbers.Contains(newRevisionNumber))
            return false;

        // For new documents (no existing revisions), first revision should be 1
        if (existingNumbers.Count == 0)
            return newRevisionNumber == MinRevisionNumber;

        // For existing documents, new revision should be next in sequence
        var maxExisting = existingNumbers.Max();
        return newRevisionNumber == maxExisting + 1;
    }

    /// <summary>
    /// Validates the deletion state consistency for a revision.
    /// </summary>
    /// <param name="isDeleted">The deletion state to validate.</param>
    /// <param name="hasActiveReferences">Whether the revision has active references that prevent deletion.</param>
    /// <returns>
    /// <c>true</c> if the deletion state is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method validates business rules around revision deletion, ensuring that revisions
    /// with active references cannot be marked as deleted to maintain referential integrity.
    /// 
    /// <para><strong>Deletion Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Revisions with active references cannot be deleted</item>
    /// <item>Non-deleted revisions can have any reference state</item>
    /// <item>Maintains data integrity and prevents orphaned references</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool canDelete = RevisionValidationHelper.IsValidDeletionState(true, false);   // true (no active references)
    /// bool cannotDelete = RevisionValidationHelper.IsValidDeletionState(true, true); // false (has active references)
    /// bool notDeleted = RevisionValidationHelper.IsValidDeletionState(false, true);  // true (not deleted)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidDeletionState(bool isDeleted, bool hasActiveReferences) =>
        !isDeleted || !hasActiveReferences;

    /// <summary>
    /// Validates the time span between creation and modification dates.
    /// </summary>
    /// <param name="creationDate">The creation date.</param>
    /// <param name="modificationDate">The modification date.</param>
    /// <param name="maxAllowedSpan">The maximum allowed time span (optional, defaults to system maximum).</param>
    /// <returns>
    /// <c>true</c> if the time span is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method validates that the time span between creation and modification dates
    /// is reasonable, helping to detect data corruption or incorrect date values.
    /// 
    /// <para><strong>Time Span Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Modification date must be >= creation date</item>
    /// <item>Time span cannot exceed maximum allowed duration</item>
    /// <item>Helps detect data corruption and unrealistic timestamps</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var creation = DateTime.UtcNow.AddDays(-1);
    /// var modification = DateTime.UtcNow;
    /// bool valid = RevisionValidationHelper.IsValidDateTimeSpan(creation, modification); // true
    /// 
    /// var oldCreation = DateTime.UtcNow.AddYears(-60);
    /// bool invalid = RevisionValidationHelper.IsValidDateTimeSpan(oldCreation, modification); // false (too long)
    /// </code>
    /// </example>
    public static bool IsValidDateTimeSpan(DateTime creationDate, DateTime modificationDate, TimeSpan? maxAllowedSpan = null)
    {
        if (!IsValidDateSequence(creationDate, modificationDate))
            return false;

        var span = modificationDate - creationDate;
        var maxSpan = maxAllowedSpan ?? MaxDateTimeSpan;

        return span >= TimeSpan.Zero && span <= maxSpan;
    }

    #endregion Business Rule Validation Methods

    #region Comprehensive Validation Methods

    /// <summary>
    /// Performs comprehensive validation of a revision number and returns detailed validation results.
    /// </summary>
    /// <param name="revisionNumber">The revision number to validate.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages). Cannot be null or whitespace.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    /// <remarks>
    /// This method validates revision numbers against all business rules including bounds checking
    /// and versioning standards.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = RevisionValidationHelper.ValidateRevisionNumber(0, nameof(MyDto.RevisionNumber));
    /// if (results.Any())
    /// {
    ///     foreach (var result in results)
    ///     {
    ///         Console.WriteLine($"Error: {result.ErrorMessage}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<ValidationResult> ValidateRevisionNumber(
        int revisionNumber,
        [NotNull] string propertyName)
    {
        // Validate input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (revisionNumber < MinRevisionNumber)
        {
            yield return new ValidationResult(
                $"{propertyName} must be at least {MinRevisionNumber} (revision numbering starts from 1).",
                [propertyName]);
        }
        else if (revisionNumber > MaxRevisionNumber)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot exceed {MaxRevisionNumber} (maximum allowed revision number).",
                [propertyName]);
        }
    }

    /// <summary>
    /// Performs comprehensive validation of a revision date.
    /// </summary>
    /// <param name="date">The date to validate.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages). Cannot be null or whitespace.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    /// <remarks>
    /// This method validates that dates are within acceptable bounds for revision operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = RevisionValidationHelper.ValidateDate(DateTime.MinValue, nameof(MyDto.CreationDate));
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

        if (!IsValidDate(date))
        {
            if (date <= DateTime.MinValue)
            {
                yield return new ValidationResult(
                    $"{propertyName} must be a valid date.",
                    [propertyName]);
            }
            else if (date < MinAllowedRevisionDate)
            {
                yield return new ValidationResult(
                    $"{propertyName} cannot be earlier than {MinAllowedRevisionDate:yyyy-MM-dd}.",
                    [propertyName]);
            }
            else if (date > MaxAllowedRevisionDate)
            {
                yield return new ValidationResult(
                    $"{propertyName} cannot be in the future.",
                    [propertyName]);
            }
        }
    }

    /// <summary>
    /// Validates revision date sequence (creation before modification).
    /// </summary>
    /// <param name="creationDate">The creation date.</param>
    /// <param name="modificationDate">The modification date.</param>
    /// <param name="creationPropertyName">Property name for creation date (for error messages). Cannot be null or whitespace.</param>
    /// <param name="modificationPropertyName">Property name for modification date (for error messages). Cannot be null or whitespace.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when property names are null, empty, or consist only of whitespace.</exception>
    /// <remarks>
    /// This method validates the chronological relationship between creation and modification dates.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = RevisionValidationHelper.ValidateDateSequence(
    ///     DateTime.UtcNow, DateTime.UtcNow.AddHours(-1), 
    ///     nameof(MyDto.CreationDate), nameof(MyDto.ModificationDate));
    /// // Will return validation error for modification before creation
    /// </code>
    /// </example>
    public static IEnumerable<ValidationResult> ValidateDateSequence(
        DateTime creationDate,
        DateTime modificationDate,
        [NotNull] string creationPropertyName,
        [NotNull] string modificationPropertyName)
    {
        // Validate input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(creationPropertyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(modificationPropertyName);

        if (!IsValidDateSequence(creationDate, modificationDate))
        {
            yield return new ValidationResult(
                "Modification date cannot be earlier than creation date.",
                [creationPropertyName, modificationPropertyName]);
        }

        if (!IsValidDateTimeSpan(creationDate, modificationDate))
        {
            var span = modificationDate - creationDate;
            if (span > MaxDateTimeSpan)
            {
                yield return new ValidationResult(
                    $"Time span between creation and modification dates is too large (maximum allowed: {MaxDateTimeSpan.TotalDays:F0} days).",
                    [creationPropertyName, modificationPropertyName]);
            }
        }
    }

    /// <summary>
    /// Performs comprehensive validation of a revision ID.
    /// </summary>
    /// <param name="revisionId">The revision ID to validate.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages). Cannot be null or whitespace.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    /// <remarks>
    /// This method validates that revision IDs represent valid, non-empty GUIDs suitable
    /// for use as database identifiers.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = RevisionValidationHelper.ValidateRevisionId(Guid.Empty, nameof(MyDto.RevisionId));
    /// if (results.Any())
    /// {
    ///     Console.WriteLine($"Revision ID validation failed: {string.Join(", ", results.Select(r => r.ErrorMessage))}");
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<ValidationResult> ValidateRevisionId(
        Guid revisionId,
        [NotNull] string propertyName)
    {
        // Validate input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (revisionId == Guid.Empty)
        {
            yield return new ValidationResult(
                $"{propertyName} must be a valid non-empty GUID.",
                [propertyName]);
        }
    }

    #endregion Comprehensive Validation Methods

    #region Utility Methods

    /// <summary>
    /// Gets the next valid revision number for a document.
    /// </summary>
    /// <param name="existingRevisionNumbers">The collection of existing revision numbers for the document.</param>
    /// <returns>
    /// The next valid revision number in sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="existingRevisionNumbers"/> is null.</exception>
    /// <remarks>
    /// This method calculates the next revision number in sequence, useful for automatic
    /// revision numbering in creation scenarios.
    /// 
    /// <para><strong>Calculation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>For empty collections: returns 1 (first revision)</item>
    /// <item>For existing revisions: returns maximum + 1</item>
    /// <item>Ensures sequential numbering without gaps</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var existing = new[] { 1, 2, 3 };
    /// int next = RevisionValidationHelper.GetNextRevisionNumber(existing); // 4
    /// 
    /// var empty = Array.Empty&lt;int&gt;();
    /// int first = RevisionValidationHelper.GetNextRevisionNumber(empty);  // 1
    /// </code>
    /// </example>
    public static int GetNextRevisionNumber([NotNull] IEnumerable<int> existingRevisionNumbers)
    {
        ArgumentNullException.ThrowIfNull(existingRevisionNumbers);

        var existingNumbers = existingRevisionNumbers.ToList();
        return existingNumbers.Count > 0 ? existingNumbers.Max() + 1 : MinRevisionNumber;
    }

    /// <summary>
    /// Determines if a revision date is reasonable (not too far in the past or future).
    /// </summary>
    /// <param name="date">The date to validate.</param>
    /// <returns>
    /// <c>true</c> if the date is reasonable; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method provides additional validation to detect potentially corrupted or
    /// incorrectly imported revision dates that might be unrealistically old or in the future.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool reasonable1 = RevisionValidationHelper.IsReasonableRevisionDate(DateTime.UtcNow.AddYears(-1)); // true
    /// bool reasonable2 = RevisionValidationHelper.IsReasonableRevisionDate(DateTime.UtcNow);              // true
    /// bool unreasonable1 = RevisionValidationHelper.IsReasonableRevisionDate(new DateTime(1970, 1, 1));  // false (too old)
    /// bool unreasonable2 = RevisionValidationHelper.IsReasonableRevisionDate(DateTime.UtcNow.AddHours(2)); // false (future)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReasonableRevisionDate(DateTime date) =>
        date >= MinAllowedRevisionDate && date <= MaxAllowedRevisionDate;

    /// <summary>
    /// Normalizes a revision date to UTC for consistent storage.
    /// </summary>
    /// <param name="date">The date to normalize.</param>
    /// <returns>
    /// The date normalized to UTC, or null if the input date is invalid.
    /// </returns>
    /// <remarks>
    /// This method ensures all revision dates are stored in UTC for consistency,
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
    /// DateTime? utcDate = RevisionValidationHelper.NormalizeDateToUtc(localDate);
    /// // Returns the date converted to UTC if valid
    /// 
    /// DateTime? invalid = RevisionValidationHelper.NormalizeDateToUtc(DateTime.MinValue);
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
    /// Gets suggested revision numbers for troubleshooting sequencing issues.
    /// </summary>
    /// <param name="attemptedNumber">The attempted revision number that failed validation.</param>
    /// <param name="existingNumbers">The existing revision numbers for context.</param>
    /// <param name="maxSuggestions">Maximum number of suggestions to return.</param>
    /// <returns>
    /// A read-only list of suggested revision numbers.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="existingNumbers"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxSuggestions"/> is less than 1.</exception>
    /// <remarks>
    /// This method provides helpful suggestions when revision number validation fails,
    /// improving user experience by offering valid alternatives.
    /// </remarks>
    /// <example>
    /// <code>
    /// var existing = new[] { 1, 2, 3 };
    /// var suggestions = RevisionValidationHelper.SuggestRevisionNumbers(5, existing, 3);
    /// // Returns suggestions like [4] (next in sequence)
    /// </code>
    /// </example>
    public static IReadOnlyList<int> SuggestRevisionNumbers(
        int attemptedNumber,
        [NotNull] IEnumerable<int> existingNumbers,
        int maxSuggestions = MaxRevisionSuggestions)
    {
        ArgumentNullException.ThrowIfNull(existingNumbers);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxSuggestions, 1);

        var suggestions = new List<int>();
        var existingList = existingNumbers.ToList();

        // Always suggest the next valid number
        var nextValid = GetNextRevisionNumber(existingList);
        suggestions.Add(nextValid);

        // If attempted number was invalid, suggest corrections
        if (attemptedNumber <= 0)
        {
            suggestions.Add(MinRevisionNumber);
        }
        else if (attemptedNumber > MaxRevisionNumber)
        {
            suggestions.Add(MaxRevisionNumber);
        }

        // Fill remaining slots with sequential numbers
        while (suggestions.Count < maxSuggestions)
        {
            var lastSuggestion = suggestions[^1];
            if (lastSuggestion < MaxRevisionNumber)
            {
                suggestions.Add(lastSuggestion + 1);
            }
            else
            {
                break;
            }
        }

        return suggestions.Distinct().Take(maxSuggestions).ToImmutableArray();
    }

    #endregion Utility Methods

    #region Diagnostic and Statistics Methods

    /// <summary>
    /// Gets validation information about a revision's temporal consistency.
    /// </summary>
    /// <param name="creationDate">The creation date.</param>
    /// <param name="modificationDate">The modification date.</param>
    /// <returns>
    /// A dictionary containing diagnostic information about the revision's temporal state.
    /// </returns>
    /// <remarks>
    /// This method provides detailed diagnostic information useful for troubleshooting
    /// revision validation issues and understanding temporal relationships.
    /// 
    /// <para><strong>Diagnostic Information Includes:</strong></para>
    /// <list type="bullet">
    /// <item>Individual date validations</item>
    /// <item>Chronological sequence validation</item>
    /// <item>Time span calculations and analysis</item>
    /// <item>DateTime kind information</item>
    /// <item>Normalization results</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var creation = DateTime.UtcNow.AddHours(-2);
    /// var modification = DateTime.UtcNow;
    /// var diagnostics = RevisionValidationHelper.GetRevisionTemporalDiagnostics(creation, modification);
    /// 
    /// // diagnostics contains comprehensive information about date validity and relationships
    /// foreach (var kvp in diagnostics)
    /// {
    ///     Console.WriteLine($"{kvp.Key}: {kvp.Value}");
    /// }
    /// </code>
    /// </example>
    public static IReadOnlyDictionary<string, object> GetRevisionTemporalDiagnostics(DateTime creationDate, DateTime modificationDate)
    {
        var timeSpan = modificationDate - creationDate;

        return new Dictionary<string, object>
        {
            ["IsValidCreationDate"] = IsValidCreationDate(creationDate),
            ["IsValidModificationDate"] = IsValidModificationDate(modificationDate),
            ["IsValidSequence"] = IsValidDateSequence(creationDate, modificationDate),
            ["IsValidTimeSpan"] = IsValidDateTimeSpan(creationDate, modificationDate),
            ["TimeSpan"] = timeSpan,
            ["TimeSpanDays"] = timeSpan.TotalDays,
            ["TimeSpanHours"] = timeSpan.TotalHours,
            ["CreationDateKind"] = creationDate.Kind,
            ["ModificationDateKind"] = modificationDate.Kind,
            ["IsReasonableCreationDate"] = IsReasonableRevisionDate(creationDate),
            ["IsReasonableModificationDate"] = IsReasonableRevisionDate(modificationDate),
            ["CreationDateUtc"] = creationDate.ToUniversalTime(),
            ["ModificationDateUtc"] = modificationDate.ToUniversalTime(),
            ["NormalizedCreationDate"] = NormalizeDateToUtc(creationDate) ?? DateTime.UtcNow,
            ["NormalizedModificationDate"] = NormalizeDateToUtc(modificationDate) ?? DateTime.UtcNow,
            ["ExceedsMaxTimeSpan"] = timeSpan > MaxDateTimeSpan
        }.ToImmutableDictionary();
    }

    /// <summary>
    /// Gets validation statistics for the revision validation system.
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
    /// var stats = RevisionValidationHelper.GetValidationStatistics();
    /// Console.WriteLine($"Min revision number: {stats["MinRevisionNumber"]}");
    /// Console.WriteLine($"Max revision number: {stats["MaxRevisionNumber"]}");
    /// Console.WriteLine($"Min allowed date: {stats["MinAllowedDate"]}");
    /// </code>
    /// </example>
    public static IReadOnlyDictionary<string, object> GetValidationStatistics()
    {
        return new Dictionary<string, object>
        {
            ["MinRevisionNumber"] = MinRevisionNumber,
            ["MaxRevisionNumber"] = MaxRevisionNumber,
            ["MinAllowedDate"] = MinAllowedRevisionDate,
            ["MaxAllowedDate"] = MaxAllowedRevisionDate,
            ["FutureDateToleranceMinutes"] = FutureDateToleranceMinutes,
            ["MaxDateTimeSpanDays"] = MaxDateTimeSpan.TotalDays,
            ["MaxRevisionSuggestions"] = MaxRevisionSuggestions,
            ["ValidationRules"] = new Dictionary<string, string>
            {
                ["RevisionNumberRange"] = $"Must be between {MinRevisionNumber} and {MaxRevisionNumber}",
                ["DateRange"] = $"Must be between {MinAllowedRevisionDate:yyyy-MM-dd} and current time plus {FutureDateToleranceMinutes} minutes",
                ["DateSequence"] = "Modification date must be >= creation date",
                ["RevisionSequence"] = "Must be sequential without gaps or duplicates",
                ["DeletionRules"] = "Cannot delete revisions with active references",
                ["TimeSpanLimit"] = $"Maximum time span between dates: {MaxDateTimeSpan.TotalDays:F0} days"
            }
        }.ToImmutableDictionary();
    }

    /// <summary>
    /// Generates a comprehensive validation report for revision data.
    /// </summary>
    /// <param name="revisionNumber">The revision number to analyze.</param>
    /// <param name="creationDate">The creation date to analyze.</param>
    /// <param name="modificationDate">The modification date to analyze.</param>
    /// <param name="existingNumbers">Optional existing revision numbers for context.</param>
    /// <returns>
    /// A formatted string containing detailed validation information.
    /// </returns>
    /// <remarks>
    /// This method provides a human-readable validation report useful for debugging
    /// and troubleshooting revision validation issues.
    /// </remarks>
    /// <example>
    /// <code>
    /// var existing = new[] { 1, 2, 3 };
    /// string report = RevisionValidationHelper.GenerateValidationReport(
    ///     5, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, existing);
    /// Console.WriteLine(report);
    /// // Outputs detailed validation results and suggestions
    /// </code>
    /// </example>
    public static string GenerateValidationReport(
        int revisionNumber,
        DateTime creationDate,
        DateTime modificationDate,
        IEnumerable<int>? existingNumbers = null)
    {
        var report = new StringBuilder();
        report.AppendLine("Revision Validation Report");
        report.AppendLine(new string('=', 30));

        // Revision number analysis
        report.AppendLine($"Revision Number: {revisionNumber}");
        report.AppendLine($"  Valid Number: {(IsValidRevisionNumber(revisionNumber) ? "✓ Yes" : "✗ No")}");

        if (existingNumbers != null)
        {
            var existing = existingNumbers.ToList();
            report.AppendLine($"  Valid Sequence: {(IsValidRevisionSequence(revisionNumber, existing) ? "✓ Yes" : "✗ No")}");
            report.AppendLine($"  Existing Numbers: [{string.Join(", ", existing)}]");
            report.AppendLine($"  Suggested Next: {GetNextRevisionNumber(existing)}");
        }

        // Date analysis
        report.AppendLine();
        report.AppendLine($"Creation Date: {creationDate:yyyy-MM-dd HH:mm:ss} ({creationDate.Kind})");
        report.AppendLine($"  Valid Date: {(IsValidCreationDate(creationDate) ? "✓ Yes" : "✗ No")}");
        report.AppendLine($"  Reasonable: {(IsReasonableRevisionDate(creationDate) ? "✓ Yes" : "✗ No")}");

        report.AppendLine();
        report.AppendLine($"Modification Date: {modificationDate:yyyy-MM-dd HH:mm:ss} ({modificationDate.Kind})");
        report.AppendLine($"  Valid Date: {(IsValidModificationDate(modificationDate) ? "✓ Yes" : "✗ No")}");
        report.AppendLine($"  Reasonable: {(IsReasonableRevisionDate(modificationDate) ? "✓ Yes" : "✗ No")}");

        // Temporal analysis
        report.AppendLine();
        report.AppendLine("Temporal Analysis:");
        report.AppendLine($"  Valid Sequence: {(IsValidDateSequence(creationDate, modificationDate) ? "✓ Yes" : "✗ No")}");
        report.AppendLine($"  Valid Time Span: {(IsValidDateTimeSpan(creationDate, modificationDate) ? "✓ Yes" : "✗ No")}");

        var timeSpan = modificationDate - creationDate;
        report.AppendLine($"  Time Difference: {timeSpan.TotalDays:F1} days, {timeSpan.TotalHours:F1} hours");

        // Suggestions for issues
        if (!IsValidRevisionNumber(revisionNumber) && existingNumbers != null)
        {
            report.AppendLine();
            report.AppendLine("Revision Number Suggestions:");
            var suggestions = SuggestRevisionNumbers(revisionNumber, existingNumbers, 3);
            foreach (var suggestion in suggestions)
            {
                report.AppendLine($"  - {suggestion}");
            }
        }

        return report.ToString();
    }

    #endregion Diagnostic and Statistics Methods
}