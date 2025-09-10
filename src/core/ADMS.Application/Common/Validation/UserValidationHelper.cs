using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ADMS.Application.Common.Validation;

/// <summary>
/// Provides validation helper methods for user-related data within the ADMS Clean Architecture solution.
/// </summary>
/// <remarks>
/// This validation helper follows Clean Architecture principles by:
/// <list type="bullet">
/// <item>Residing in the Application layer for cross-cutting validation concerns</item>
/// <item>Being independent of external frameworks and infrastructure</item>
/// <item>Supporting domain validation requirements without domain dependencies</item>
/// <item>Providing consistent validation across DTOs and commands/queries</item>
/// </list>
/// 
/// <para><strong>Clean Architecture Alignment:</strong></para>
/// <list type="bullet">
/// <item><strong>Application Layer:</strong> Serves application services and DTOs</item>
/// <item><strong>Framework Independent:</strong> Uses only .NET base class library</item>
/// <item><strong>Domain Agnostic:</strong> Validates data without domain entity knowledge</item>
/// <item><strong>Testable:</strong> Pure functions with no side effects</item>
/// </list>
/// </remarks>
public static partial class UserValidationHelper
{
    #region Clean Architecture Constants

    /// <summary>
    /// Maximum allowed length for a user name (matches domain constraints).
    /// </summary>
    public const int MaxUserNameLength = 50;

    /// <summary>
    /// Minimum allowed length for a user name.
    /// </summary>
    public const int MinUserNameLength = 2;

    /// <summary>
    /// Earliest allowed date for user operations (prevents unrealistic dates).
    /// </summary>
    public static readonly DateTime MinAllowedUserDate = new(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Tolerance in minutes for future dates (accounts for clock skew).
    /// </summary>
    public const int FutureDateToleranceMinutes = 5;

    #endregion Clean Architecture Constants

    #region Reserved Names (High Performance)

    /// <summary>
    /// Reserved usernames that cannot be used (optimized with FrozenSet for O(1) lookup).
    /// </summary>
    private static readonly string[] _reservedNamesArray =
    [
        // System accounts
        "admin", "administrator", "system", "root", "sa", "sysadmin",
        
        // Service accounts  
        "service", "daemon", "process", "worker", "scheduler",
        
        // Common roles
        "user", "guest", "anonymous", "public", "default",
        
        // API terms
        "api", "rest", "json", "xml", "http", "https",
        
        // ADMS specific
        "adms", "matter", "document", "revision", "activity",
        
        // Security terms
        "security", "auth", "authentication", "token", "session",
        
        // Common internet names
        "support", "help", "info", "contact", "admin", "webmaster",
        
        // Potentially confusing
        "null", "undefined", "none", "empty", "void", "test"
    ];

    /// <summary>
    /// High-performance frozen set for reserved name lookups.
    /// </summary>
    private static readonly FrozenSet<string> _reservedNamesSet =
        _reservedNamesArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the immutable list of reserved names.
    /// </summary>
    public static IReadOnlyList<string> ReservedNames => _reservedNamesArray.ToImmutableArray();

    #endregion Reserved Names

    #region Core Validation Methods (Clean Architecture)

    /// <summary>
    /// Validates a user ID according to Clean Architecture business rules.
    /// </summary>
    /// <param name="userId">The user ID to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateUserId(Guid userId, string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (userId == Guid.Empty)
        {
            yield return new ValidationResult(
                $"{propertyName} must be a valid non-empty GUID.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates a user name according to Clean Architecture business rules.
    /// </summary>
    /// <param name="name">The user name to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateName(string? name, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Required validation
        if (string.IsNullOrWhiteSpace(name))
        {
            yield return new ValidationResult(
                $"{propertyName} is required and cannot be empty.",
                [propertyName]);
            yield break;
        }

        var trimmed = name.Trim();

        // Length validation
        if (trimmed.Length < MinUserNameLength)
        {
            yield return new ValidationResult(
                $"{propertyName} must be at least {MinUserNameLength} characters long.",
                [propertyName]);
        }

        if (trimmed.Length > MaxUserNameLength)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot exceed {MaxUserNameLength} characters.",
                [propertyName]);
        }

        // Reserved name validation
        if (_reservedNamesSet.Contains(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} is a reserved name and cannot be used.",
                [propertyName]);
        }

        // Format validation
        if (!IsValidUserNameFormat(trimmed))
        {
            yield return new ValidationResult(
                GetFormatErrorMessage(propertyName, trimmed),
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates a user-related date according to Clean Architecture business rules.
    /// </summary>
    /// <param name="date">The date to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateDate(DateTime date, string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (date <= DateTime.MinValue)
        {
            yield return new ValidationResult(
                $"{propertyName} must be a valid date.",
                [propertyName]);
        }
        else if (date < MinAllowedUserDate)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot be earlier than {MinAllowedUserDate:yyyy-MM-dd}.",
                [propertyName]);
        }
        else if (date > DateTime.UtcNow.AddMinutes(FutureDateToleranceMinutes))
        {
            yield return new ValidationResult(
                $"{propertyName} cannot be in the future.",
                [propertyName]);
        }
    }

    #endregion Core Validation Methods

    #region Quick Validation Methods (Performance Optimized)

    /// <summary>
    /// Quick validation check for user ID (optimized for performance).
    /// </summary>
    /// <param name="userId">The user ID to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidUserId(Guid userId) => userId != Guid.Empty;

    /// <summary>
    /// Quick validation check for user name (optimized for performance).
    /// </summary>
    /// <param name="name">The user name to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsNameAllowed([NotNullWhen(true)] string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var trimmed = name.Trim();

        return trimmed.Length >= MinUserNameLength &&
               trimmed.Length <= MaxUserNameLength &&
               !_reservedNamesSet.Contains(trimmed) &&
               IsValidUserNameFormat(trimmed);
    }

    /// <summary>
    /// Quick validation check for user date (optimized for performance).
    /// </summary>
    /// <param name="date">The date to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidUserDate(DateTime date)
    {
        var utcDate = date.Kind != DateTimeKind.Utc ? date.ToUniversalTime() : date;

        return utcDate > MinAllowedUserDate &&
               utcDate <= DateTime.UtcNow.AddMinutes(FutureDateToleranceMinutes);
    }

    /// <summary>
    /// Checks if a name is reserved (optimized for performance).
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if reserved; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReservedName([NotNullWhen(true)] string? name)
    {
        return !string.IsNullOrWhiteSpace(name) && _reservedNamesSet.Contains(name.Trim());
    }

    #endregion Quick Validation Methods

    #region Normalization Methods (Clean Architecture)

    /// <summary>
    /// Normalizes a user name for consistent storage and comparison.
    /// </summary>
    /// <param name="name">The name to normalize.</param>
    /// <returns>Normalized name or null if invalid.</returns>
    /// <remarks>
    /// Normalization includes:
    /// <list type="bullet">
    /// <item>Trimming whitespace</item>
    /// <item>Collapsing multiple spaces to single spaces</item>
    /// <item>Preserving case for professional appearance</item>
    /// </list>
    /// </remarks>
    [return: NotNullIfNotNull(nameof(name))]
    public static string? NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var trimmed = name.Trim();
        // Ensure that if input is non-null, output is also non-null
        return trimmed.Length == 0 ? string.Empty : MultipleSpacesRegex().Replace(trimmed, " ");
    }

    /// <summary>
    /// Normalizes a date to UTC for consistent storage.
    /// </summary>
    /// <param name="date">The date to normalize.</param>
    /// <returns>UTC date or null if invalid.</returns>
    public static DateTime? NormalizeDateToUtc(DateTime date)
    {
        if (!IsValidUserDate(date))
            return null;

        return date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(date, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(date, DateTimeKind.Utc)
        };
    }

    #endregion Normalization Methods

    #region Utility Methods (Clean Architecture)

    /// <summary>
    /// Checks if two user names are equivalent after normalization.
    /// </summary>
    /// <param name="name1">First name to compare.</param>
    /// <param name="name2">Second name to compare.</param>
    /// <returns>True if equivalent; otherwise, false.</returns>
    public static bool AreNamesEquivalent(string? name1, string? name2)
    {
        var normalized1 = NormalizeName(name1);
        var normalized2 = NormalizeName(name2);

        return !string.IsNullOrEmpty(normalized1) &&
               !string.IsNullOrEmpty(normalized2) &&
               string.Equals(normalized1, normalized2, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets validation statistics for diagnostics and monitoring.
    /// </summary>
    /// <returns>Dictionary containing validation statistics.</returns>
    public static IReadOnlyDictionary<string, object> GetValidationStatistics()
    {
        return new Dictionary<string, object>
        {
            ["MinUserNameLength"] = MinUserNameLength,
            ["MaxUserNameLength"] = MaxUserNameLength,
            ["ReservedNamesCount"] = ReservedNames.Count,
            ["MinAllowedDate"] = MinAllowedUserDate,
            ["FutureDateToleranceMinutes"] = FutureDateToleranceMinutes,
            ["ValidationRules"] = new Dictionary<string, string>
            {
                ["NameLength"] = $"Between {MinUserNameLength} and {MaxUserNameLength} characters",
                ["AllowedCharacters"] = "Letters, numbers, spaces, periods, hyphens, underscores",
                ["StartEndRule"] = "Must start and end with letter or number",
                ["ReservedRule"] = "Cannot use reserved system names",
                ["DateRange"] = $"Between {MinAllowedUserDate:yyyy-MM-dd} and present"
            }.ToImmutableDictionary()
        }.ToImmutableDictionary();
    }

    #endregion Utility Methods

    #region Private Helper Methods

    /// <summary>
    /// Validates user name format using comprehensive rules.
    /// </summary>
    /// <param name="userName">The user name to validate (must be trimmed).</param>
    /// <returns>True if format is valid; otherwise, false.</returns>
    private static bool IsValidUserNameFormat(string userName)
    {
        switch (userName.Length)
        {
            case 0:
                return false;
            case 1:
                return char.IsLetterOrDigit(userName[0]);
        }

        // Check character set
        if (!UserNameCharacterRegex().IsMatch(userName))
            return false;

        // Check start and end characters
        if (!char.IsLetterOrDigit(userName[0]) || !char.IsLetterOrDigit(userName[^1]))
            return false;

        // Check for consecutive special characters
        for (var i = 0; i < userName.Length - 1; i++)
        {
            var current = userName[i];
            var next = userName[i + 1];

            // Prevent consecutive special characters or spaces
            if (IsSpecialCharacter(current) && (current == next || IsSpecialCharacter(next)))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines if a character is a special character allowed in user names.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>True if special character; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSpecialCharacter(char c) => c is '.' or '_' or '-' or ' ';

    /// <summary>
    /// Gets a specific format error message based on the validation failure.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="value">The invalid value.</param>
    /// <returns>Specific error message.</returns>
    private static string GetFormatErrorMessage(string propertyName, string value)
    {
        if (value.StartsWith('.') || value.StartsWith('_') || value.StartsWith('-'))
            return $"{propertyName} cannot start with a special character.";

        if (value.EndsWith('.') || value.EndsWith('_') || value.EndsWith('-'))
            return $"{propertyName} cannot end with a special character.";

        if (value.Contains("  "))
            return $"{propertyName} cannot contain multiple consecutive spaces.";

        if (value.Contains("..") || value.Contains("__") || value.Contains("--"))
            return $"{propertyName} cannot contain consecutive special characters.";

        if (!UserNameCharacterRegex().IsMatch(value))
            return $"{propertyName} can only contain letters, numbers, spaces, periods, hyphens, and underscores.";

        return $"{propertyName} contains invalid format.";
    }

    #endregion Private Helper Methods

    #region Compiled Regex Patterns

    /// <summary>
    /// Compiled regex for validating user name characters.
    /// </summary>
    [GeneratedRegex(@"^[a-zA-Z0-9._\s-]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex UserNameCharacterRegex();

    /// <summary>
    /// Compiled regex for collapsing multiple spaces.
    /// </summary>
    [GeneratedRegex(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MultipleSpacesRegex();

    #endregion Compiled Regex Patterns
}