using ADMS.Application.Constants;

using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ADMS.Application.Common.Validation;

/// <summary>
/// Provides comprehensive validation helper methods for matter-related data within the ADMS document management solution.
/// </summary>
/// <remarks>
/// This validation helper follows Clean Architecture principles and provides optimized validation for Matter entities and DTOs:
/// <list type="bullet">
/// <item><strong>Description Validation:</strong> Ensures professional legal naming conventions and uniqueness requirements</item>
/// <item><strong>Date Validation:</strong> Validates creation and modification dates with professional standards</item>
/// <item><strong>State Validation:</strong> Enforces proper matter lifecycle state transitions</item>
/// <item><strong>Business Rule Validation:</strong> Implements legal practice-specific validation rules</item>
/// </list>
/// 
/// <para><strong>Matter Context in ADMS:</strong></para>
/// Matters in this system represent digital filing cabinets for document organization, supporting:
/// <list type="bullet">
/// <item>Client-based document collections (e.g., "Smith Family Trust")</item>
/// <item>Matter-specific collections (e.g., "Smith v. Jones Litigation")</item>
/// <item>Project-based collections for professional legal practice</item>
/// </list>
/// </remarks>
public static partial class MatterValidationHelper
{
    #region Core Constants

    /// <summary>
    /// Maximum allowed length for a matter description (matches domain constraints).
    /// </summary>
    public const int MaxDescriptionLength = MatterConstants.DescriptionMaxLength;

    /// <summary>
    /// Minimum allowed length for a matter description.
    /// </summary>
    public const int MinDescriptionLength = MatterConstants.DescriptionMinLength;

    /// <summary>
    /// Earliest allowed date for matter operations (prevents unrealistic dates).
    /// </summary>
    public static readonly DateTime MinAllowedMatterDate = new(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Tolerance in minutes for future dates (accounts for clock skew).
    /// </summary>
    public const int FutureDateToleranceMinutes = 5;

    /// <summary>
    /// Maximum reasonable age for active matters in years.
    /// </summary>
    public const int MaxReasonableAgeYears = MatterConstants.MaxHistoricalYears;

    #endregion Core Constants

    #region Reserved Words (High Performance)

    /// <summary>
    /// Reserved matter descriptions that cannot be used (optimized with FrozenSet for O(1) lookup).
    /// </summary>
    private static readonly string[] _reservedDescriptionsArray =
    [
        // System terms
        "system", "admin", "administrator", "root", "sa", "default",
        
        // Common terms that could cause confusion
        "matter", "document", "file", "folder", "directory",
        
        // Legal system terms
        "court", "judge", "clerk", "registry", "docket",
        
        // ADMS specific
        "adms", "database", "backup", "temp", "temporary", "test",
        
        // Potentially problematic
        "null", "undefined", "none", "empty", "void", "unknown",
        
        // Security terms
        "security", "auth", "authentication", "token", "session",
        
        // Common problematic names
        "new", "copy", "duplicate", "sample", "example"
    ];

    /// <summary>
    /// High-performance frozen set for reserved description lookups.
    /// </summary>
    private static readonly FrozenSet<string> _reservedDescriptionsSet =
        _reservedDescriptionsArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the immutable list of reserved descriptions.
    /// </summary>
    public static IReadOnlyList<string> ReservedDescriptions => _reservedDescriptionsArray;

    #endregion Reserved Words

    #region Description Validation

    /// <summary>
    /// Validates a matter description according to business rules.
    /// </summary>
    /// <param name="description">The description to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates matter descriptions for legal practice requirements including:
    /// <list type="bullet">
    /// <item>Required field validation</item>
    /// <item>Length constraints aligned with database schema</item>
    /// <item>Professional naming conventions</item>
    /// <item>Reserved word protection</item>
    /// <item>Character set validation for file system compatibility</item>
    /// </list>
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateDescription(string? description, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Required validation
        if (string.IsNullOrWhiteSpace(description))
        {
            yield return new ValidationResult(
                $"{propertyName} is required and cannot be empty.",
                [propertyName]);
            yield break;
        }

        var trimmed = description.Trim();

        switch (trimmed.Length)
        {
            // Length validation
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

        // Reserved word validation
        if (_reservedDescriptionsSet.Contains(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} is a reserved term and cannot be used.",
                [propertyName]);
        }

        // Format validation
        if (!IsValidDescriptionFormat(trimmed))
        {
            yield return new ValidationResult(
                GetDescriptionFormatErrorMessage(propertyName, trimmed),
                [propertyName]);
        }

        // Whitespace validation
        if (description.Trim() != description)
        {
            yield return new ValidationResult(
                $"{propertyName} should not have leading or trailing whitespace.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates a matter creation date according to business rules.
    /// </summary>
    /// <param name="creationDate">The creation date to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateCreationDate(DateTime creationDate, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (creationDate == default)
        {
            yield return new ValidationResult(
                $"{propertyName} is required.",
                [propertyName]);
            yield break;
        }

        if (creationDate < MinAllowedMatterDate)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot be earlier than {MinAllowedMatterDate:yyyy-MM-dd}.",
                [propertyName]);
        }

        if (creationDate > DateTime.UtcNow.AddMinutes(FutureDateToleranceMinutes))
        {
            yield return new ValidationResult(
                $"{propertyName} cannot be in the future.",
                [propertyName]);
        }

        // Business rule: Check for reasonable age
        var ageDays = (DateTime.UtcNow - creationDate).TotalDays;
        if (ageDays > (MaxReasonableAgeYears * 365))
        {
            yield return new ValidationResult(
                $"{propertyName} age exceeds reasonable bounds for active legal practice ({MaxReasonableAgeYears} years).",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates matter audit fields (created by, last modified by, etc.).
    /// </summary>
    /// <param name="createdBy">The user who created the matter.</param>
    /// <param name="lastModifiedBy">The user who last modified the matter.</param>
    /// <param name="lastModifiedDate">The last modification date (optional).</param>
    /// <param name="creationDate">The creation date for comparison.</param>
    /// <param name="propertyPrefix">The property prefix for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyPrefix is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateAuditFields(
        string? createdBy,
        string? lastModifiedBy,
        DateTime? lastModifiedDate,
        DateTime creationDate,
        [NotNull] string propertyPrefix = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyPrefix);

        // Validate CreatedBy
        foreach (var result in UserValidationHelper.ValidateUsername(createdBy, $"{propertyPrefix}CreatedBy"))
            yield return result;

        // Validate LastModifiedBy
        foreach (var result in UserValidationHelper.ValidateUsername(lastModifiedBy, $"{propertyPrefix}LastModifiedBy"))
            yield return result;

        // Validate LastModifiedDate relationship
        if (lastModifiedDate.HasValue && lastModifiedDate < creationDate)
        {
            yield return new ValidationResult(
                "Last modified date cannot be before creation date.",
                [$"{propertyPrefix}LastModifiedDate"]);
        }
    }

    /// <summary>
    /// Validates matter state consistency (archive and delete states).
    /// </summary>
    /// <param name="isArchived">Whether the matter is archived.</param>
    /// <param name="isDeleted">Whether the matter is deleted.</param>
    /// <param name="propertyPrefix">The property prefix for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyPrefix is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateStateConsistency(
        bool isArchived,
        bool isDeleted,
        [NotNull] string propertyPrefix = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyPrefix);

        // Business rule: Deleted matters should be archived
        if (isDeleted && !isArchived)
        {
            yield return new ValidationResult(
                "Deleted matters must be archived for audit trail integrity.",
                [$"{propertyPrefix}IsArchived", $"{propertyPrefix}IsDeleted"]);
        }
    }

    #endregion Description Validation

    #region Quick Validation Methods (Performance Optimized)

    /// <summary>
    /// Quick validation check for matter description (optimized for performance).
    /// </summary>
    /// <param name="description">The description to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsDescriptionAllowed([NotNullWhen(true)] string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return false;

        var trimmed = description.Trim();

        return trimmed.Length >= MinDescriptionLength &&
               trimmed.Length <= MaxDescriptionLength &&
               !_reservedDescriptionsSet.Contains(trimmed) &&
               IsValidDescriptionFormat(trimmed) &&
               description.Trim() == description;
    }

    /// <summary>
    /// Quick validation check for creation date (optimized for performance).
    /// </summary>
    /// <param name="creationDate">The creation date to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidCreationDate(DateTime creationDate)
    {
        return creationDate > MinAllowedMatterDate &&
               creationDate <= DateTime.UtcNow.AddMinutes(FutureDateToleranceMinutes) &&
               (DateTime.UtcNow - creationDate).TotalDays <= (MaxReasonableAgeYears * 365);
    }

    /// <summary>
    /// Checks if a description is reserved (optimized for performance).
    /// </summary>
    /// <param name="description">The description to check.</param>
    /// <returns>True if reserved; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReservedDescription([NotNullWhen(true)] string? description)
    {
        return !string.IsNullOrWhiteSpace(description) && _reservedDescriptionsSet.Contains(description.Trim());
    }

    /// <summary>
    /// Validates matter state consistency (optimized for performance).
    /// </summary>
    /// <param name="isArchived">Whether the matter is archived.</param>
    /// <param name="isDeleted">Whether the matter is deleted.</param>
    /// <returns>True if state is consistent; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidStateConsistency(bool isArchived, bool isDeleted)
    {
        return !isDeleted || isArchived; // If deleted, must be archived
    }

    #endregion Quick Validation Methods

    #region Normalization Methods

    /// <summary>
    /// Normalizes a matter description for consistent storage and comparison.
    /// </summary>
    /// <param name="description">The description to normalize.</param>
    /// <returns>Normalized description or null if invalid.</returns>
    /// <remarks>
    /// Normalization includes:
    /// <list type="bullet">
    /// <item>Trimming whitespace</item>
    /// <item>Collapsing multiple spaces to single spaces</item>
    /// <item>Preserving case for proper display</item>
    /// <item>Removing non-printable characters</item>
    /// </list>
    /// </remarks>
    [return: NotNullIfNotNull(nameof(description))]
    public static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var trimmed = description.Trim();
        if (trimmed.Length == 0) return string.Empty;

        // Collapse multiple spaces and remove non-printable characters
        var normalized = MultipleSpacesRegex().Replace(trimmed, " ");
        return NonPrintableRegex().Replace(normalized, "");
    }

    /// <summary>
    /// Normalizes a creation date to UTC for consistent storage.
    /// </summary>
    /// <param name="creationDate">The creation date to normalize.</param>
    /// <returns>UTC creation date or null if invalid.</returns>
    public static DateTime? NormalizeCreationDate(DateTime creationDate)
    {
        if (!IsValidCreationDate(creationDate))
            return null;

        return creationDate.Kind switch
        {
            DateTimeKind.Utc => creationDate,
            DateTimeKind.Local => creationDate.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(creationDate, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(creationDate, DateTimeKind.Utc)
        };
    }

    #endregion Normalization Methods

    #region Business Rule Validation

    /// <summary>
    /// Validates matter business rules for professional legal practice.
    /// </summary>
    /// <param name="description">The matter description.</param>
    /// <param name="creationDate">The creation date.</param>
    /// <param name="isArchived">Whether the matter is archived.</param>
    /// <param name="isDeleted">Whether the matter is deleted.</param>
    /// <param name="propertyPrefix">The property prefix for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyPrefix is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateBusinessRules(
        string? description,
        DateTime creationDate,
        bool isArchived,
        bool isDeleted,
        [NotNull] string propertyPrefix = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyPrefix);

        // Description business rules
        if (!string.IsNullOrWhiteSpace(description))
        {
            var normalized = NormalizeDescription(description);
            
            // Check for professional naming patterns
            if (ContainsProfessionalAntiPatterns(normalized))
            {
                yield return new ValidationResult(
                    $"{propertyPrefix}Description should follow professional legal naming conventions.",
                    [$"{propertyPrefix}Description"]);
            }
        }

        // State business rules
        foreach (var result in ValidateStateConsistency(isArchived, isDeleted, propertyPrefix))
            yield return result;

        // Date business rules
        if (creationDate == default) yield break;
        // Check for suspicious timing (might indicate automation)
        if (creationDate is { Millisecond: 0, Second: 0 } && creationDate.Minute % 5 == 0)
        {
            // This might indicate automated creation - in production, you might log this for investigation
            // rather than fail validation
        }
    }

    /// <summary>
    /// Validates matter uniqueness constraints.
    /// </summary>
    /// <param name="description">The matter description to check for uniqueness.</param>
    /// <param name="existingMatterDescriptions">Collection of existing matter descriptions for comparison.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if unique).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// This method helps enforce uniqueness constraints at the application level.
    /// The actual uniqueness should also be enforced at the database level.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateUniqueness(
        string? description,
        IEnumerable<string>? existingMatterDescriptions,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (string.IsNullOrWhiteSpace(description) || existingMatterDescriptions == null)
            yield break;

        var normalizedDescription = NormalizeDescription(description);

        var normalizedExisting = existingMatterDescriptions
            .Select(NormalizeDescription)
            .Where(x => x != null)
            .ToList();

        var isDuplicate = normalizedExisting.Any(existing => 
            string.Equals(existing, normalizedDescription, StringComparison.OrdinalIgnoreCase));

        if (isDuplicate)
        {
            yield return new ValidationResult(
                $"{propertyName} must be unique. A matter with this description already exists.",
                [propertyName]);
        }
    }

    #endregion Business Rule Validation

    #region Utility Methods

    /// <summary>
    /// Checks if two matter descriptions are equivalent after normalization.
    /// </summary>
    /// <param name="description1">First description to compare.</param>
    /// <param name="description2">Second description to compare.</param>
    /// <returns>True if equivalent; otherwise, false.</returns>
    public static bool AreDescriptionsEquivalent(string? description1, string? description2)
    {
        var normalized1 = NormalizeDescription(description1);
        var normalized2 = NormalizeDescription(description2);

        return !string.IsNullOrEmpty(normalized1) &&
               !string.IsNullOrEmpty(normalized2) &&
               string.Equals(normalized1, normalized2, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if matter data is sufficient for professional legal practice requirements.
    /// </summary>
    /// <param name="description">The matter description.</param>
    /// <param name="creationDate">The creation date.</param>
    /// <param name="createdBy">The user who created the matter.</param>
    /// <returns>True if data is sufficient; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSufficientMatterData(string? description, DateTime creationDate, string? createdBy)
    {
        return IsDescriptionAllowed(description) &&
               IsValidCreationDate(creationDate) &&
               UserValidationHelper.IsUsernameAllowed(createdBy);
    }

    /// <summary>
    /// Gets validation statistics for diagnostics and monitoring.
    /// </summary>
    /// <returns>Dictionary containing validation statistics.</returns>
    public static IReadOnlyDictionary<string, object> GetValidationStatistics()
    {
        return new Dictionary<string, object>
        {
            ["MinDescriptionLength"] = MinDescriptionLength,
            ["MaxDescriptionLength"] = MaxDescriptionLength,
            ["ReservedDescriptionsCount"] = ReservedDescriptions.Count,
            ["MinAllowedDate"] = MinAllowedMatterDate,
            ["FutureDateToleranceMinutes"] = FutureDateToleranceMinutes,
            ["MaxReasonableAgeYears"] = MaxReasonableAgeYears,
            ["ValidationRules"] = new Dictionary<string, string>
            {
                ["DescriptionLength"] = $"Between {MinDescriptionLength} and {MaxDescriptionLength} characters",
                ["AllowedCharacters"] = "Letters, numbers, spaces, hyphens, underscores, dots, parentheses",
                ["ReservedRule"] = "Cannot use reserved legal or system terms",
                ["DateRange"] = $"Between {MinAllowedMatterDate:yyyy-MM-dd} and present",
                ["StateRule"] = "Deleted matters must be archived"
            }
        };
    }

    #endregion Utility Methods

    #region Private Helper Methods

    /// <summary>
    /// Validates matter description format using comprehensive rules.
    /// </summary>
    /// <param name="description">The description to validate (must be trimmed).</param>
    /// <returns>True if format is valid; otherwise, false.</returns>
    private static bool IsValidDescriptionFormat(string description)
    {
        if (string.IsNullOrEmpty(description))
            return false;

        // Check character set (letters, numbers, spaces, professional punctuation)
        if (!DescriptionCharacterRegex().IsMatch(description))
            return false;

        // Check for multiple consecutive spaces
        if (description.Contains("  "))
            return false;

        // Check for professional naming patterns
        return !ContainsProfessionalAntiPatterns(description);
    }

    /// <summary>
    /// Checks for anti-patterns that might indicate unprofessional naming.
    /// </summary>
    /// <param name="description">The description to check.</param>
    /// <returns>True if contains anti-patterns; otherwise, false.</returns>
    private static bool ContainsProfessionalAntiPatterns(string description)
    {
        var lower = description.ToLowerInvariant();

        // Check for obviously test or placeholder patterns
        var testPatterns = new[] { "test", "sample", "example", "placeholder", "temp", "dummy" };
        if (testPatterns.Any(pattern => lower.Contains(pattern)))
            return true;

        // Check for excessive punctuation
        var punctuationCount = description.Count(c => "!@#$%^&*".Contains(c));
        if (punctuationCount > 2)
            return true;

        // Check for all caps (unprofessional)
        return description.Length > 10 &&
               string.Equals(description, description.ToUpperInvariant(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets a specific format error message based on the validation failure.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="value">The invalid value.</param>
    /// <returns>Specific error message.</returns>
    private static string GetDescriptionFormatErrorMessage(string propertyName, string value)
    {
        if (value.Contains("  "))
            return $"{propertyName} cannot contain multiple consecutive spaces.";

        if (!DescriptionCharacterRegex().IsMatch(value))
            return $"{propertyName} can only contain letters, numbers, spaces, hyphens, underscores, dots, and parentheses.";

        return ContainsProfessionalAntiPatterns(value) ? $"{propertyName} should follow professional legal naming conventions." : $"{propertyName} contains invalid format.";
    }

    #endregion Private Helper Methods

    #region Compiled Regex Patterns

    /// <summary>
    /// Compiled regex for validating description characters.
    /// </summary>
    [GeneratedRegex(@"^[a-zA-Z0-9\s._\-()]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex DescriptionCharacterRegex();

    /// <summary>
    /// Compiled regex for collapsing multiple spaces.
    /// </summary>
    [GeneratedRegex(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MultipleSpacesRegex();

    /// <summary>
    /// Compiled regex for removing non-printable characters.
    /// </summary>
    [GeneratedRegex(@"[^\u0020-\u007E]", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex NonPrintableRegex();

    #endregion Compiled Regex Patterns
}