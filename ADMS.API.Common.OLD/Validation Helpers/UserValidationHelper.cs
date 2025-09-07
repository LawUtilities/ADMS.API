using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ADMS.API.Common;

/// <summary>
/// Provides comprehensive helper methods and constants for validating user-related data within the ADMS system.
/// </summary>
/// <remarks>
/// This static helper class provides robust user validation functionality for the ADMS legal 
/// document management system, supporting all user-related DTOs including UserDto, UserMinimalDto, 
/// UserForCreationDto, and UserForUpdateDto. The validation methods ensure data integrity, 
/// business rule compliance, and consistent validation logic across the application.
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item>Username validation with database constraint alignment and professional naming standards</item>
/// <item>GUID validation for user IDs with proper empty value checking</item>
/// <item>Reserved name protection to prevent conflicts with system functionality</item>
/// <item>Character format validation ensuring professional and readable usernames</item>
/// <item>Normalization support for consistent storage and comparison</item>
/// <item>Username suggestion algorithms for improved user experience</item>
/// <item>High-performance validation using frozen collections for O(1) lookup performance</item>
/// <item>Thread-safe operations optimized for concurrent access scenarios</item>
/// </list>
/// 
/// <para><strong>Username Standards:</strong></para>
/// <list type="bullet">
/// <item><strong>Professional Names:</strong> Support for full names with spaces and professional formatting</item>
/// <item><strong>Length Requirements:</strong> 2-50 characters matching database constraints</item>
/// <item><strong>Character Support:</strong> Letters, numbers, spaces, periods, hyphens, underscores</item>
/// <item><strong>Format Rules:</strong> Cannot start/end with special characters, no consecutive specials</item>
/// </list>
/// 
/// <para><strong>Database Synchronization:</strong></para>
/// All validation constraints are synchronized with the User entity structure:
/// <list type="bullet">
/// <item>Name: StringLength(50) - matches User.Name constraint</item>
/// <item>Reserved names: Protected system and ADMS-specific terms</item>
/// <item>Professional formatting: Supports real-world user naming conventions</item>
/// </list>
/// 
/// <para><strong>Legal Compliance:</strong></para>
/// User validation enforces legal document management best practices including:
/// <list type="bullet">
/// <item>Audit trail integrity through consistent user identification</item>
/// <item>Professional naming standards for legal document attribution</item>
/// <item>System security through reserved name protection</item>
/// <item>Data integrity through comprehensive validation rules</item>
/// </list>
/// 
/// <para><strong>Thread Safety:</strong></para>
/// All methods in this class are thread-safe and use immutable frozen collections for optimal
/// performance in concurrent scenarios without external synchronization.
/// 
/// <para><strong>Performance Considerations:</strong></para>
/// Uses FrozenSet for O(1) average lookup performance on reserved names and
/// compiled regex patterns for efficient format validation. All validation operations 
/// are optimized for high-frequency usage in API scenarios.
/// </remarks>
public static partial class UserValidationHelper
{
    #region Constants

    /// <summary>
    /// The maximum allowed length for a username.
    /// </summary>
    /// <remarks>
    /// This value matches the StringLength(50) constraint on the User.Name property 
    /// in the ADMS.API.Entities.User entity to ensure consistency between validation 
    /// logic and database constraints.
    /// </remarks>
    public const int MaxUserNameLength = 50;

    /// <summary>
    /// The minimum allowed length for a username.
    /// </summary>
    /// <remarks>
    /// Minimum length helps prevent very short usernames that might cause
    /// confusion or conflict with system identifiers. Set to 2 to allow
    /// reasonable abbreviations while maintaining clarity.
    /// </remarks>
    public const int MinUserNameLength = 2;

    /// <summary>
    /// The earliest allowed date for user operations.
    /// </summary>
    /// <remarks>
    /// This date represents a reasonable lower bound for user-related dates in the ADMS system,
    /// preventing unrealistic historical dates that might indicate data corruption.
    /// Set to January 1, 1980, as a practical minimum for legal document management systems.
    /// </remarks>
    public static readonly DateTime MinAllowedUserDate = new(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The tolerance in minutes for future dates in user-related operations.
    /// </summary>
    /// <remarks>
    /// This tolerance accounts for clock skew between client and server systems,
    /// allowing for small time differences while preventing actual future dates.
    /// </remarks>
    public const int FutureDateToleranceMinutes = 1;

    /// <summary>
    /// Maximum number of username suggestions to generate for user assistance.
    /// </summary>
    /// <remarks>
    /// Limits the number of suggestions to prevent excessive processing while
    /// providing sufficient alternatives for user selection.
    /// </remarks>
    public const int MaxUsernameSuggestions = 10;

    /// <summary>
    /// The list of reserved usernames that cannot be used (case-insensitive).
    /// These terms are protected to prevent conflicts with system functionality.
    /// </summary>
    /// <remarks>
    /// This list includes common system account names, administrative terms,
    /// and names that might cause confusion with ADMS system functionality.
    /// Reserved names are checked using case-insensitive comparison.
    /// 
    /// <para><strong>Reserved Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>System Accounts:</strong> admin, administrator, system, root</item>
    /// <item><strong>Service Accounts:</strong> service, daemon, process, worker</item>
    /// <item><strong>Common Roles:</strong> user, guest, anonymous, public</item>
    /// <item><strong>API Terms:</strong> api, rest, json, xml, http</item>
    /// <item><strong>ADMS Specific:</strong> adms, matter, document, revision</item>
    /// <item><strong>Security Terms:</strong> auth, token, session, password</item>
    /// <item><strong>Internet Standards:</strong> support, help, noreply, webmaster</item>
    /// <item><strong>Confusing Terms:</strong> null, undefined, empty, test</item>
    /// </list>
    /// 
    /// <para><strong>Modification Guidelines:</strong></para>
    /// When adding reserved names:
    /// <list type="bullet">
    /// <item>Consider case-insensitive conflicts</item>
    /// <item>Include variations and common misspellings</item>
    /// <item>Focus on terms that could cause system confusion</item>
    /// <item>Document the reason for reservation</item>
    /// </list>
    /// </remarks>
    private static readonly string[] _reservedNamesArray =
    [
        // System accounts
        "admin", "administrator", "system", "root", "sa", "sysadmin",
        
        // Service accounts
        "service", "daemon", "process", "worker", "scheduler",
        
        // Common roles
        "user", "guest", "anonymous", "public", "default",
        
        // API and technical terms
        "api", "rest", "soap", "json", "xml", "http", "https",
        "www", "mail", "email", "ftp", "ssh", "ssl", "tls",
        
        // ADMS specific terms
        "adms", "matter", "document", "revision", "activity",
        
        // Security related
        "security", "auth", "authentication", "authorization",
        "token", "session", "login", "logout", "password",
        
        // Common internet names
        "support", "help", "info", "contact", "sales", "marketing",
        "noreply", "no-reply", "postmaster", "webmaster",
        
        // Potentially confusing terms
        "null", "undefined", "none", "empty", "void", "test",
        
        // Additional system terms
        "bin", "boot", "dev", "etc", "home", "lib", "mnt", "opt",
        "proc", "run", "srv", "tmp", "usr", "var", "windows",
        
        // Database terms
        "database", "db", "sql", "select", "insert", "update", "delete"
    ];

    /// <summary>
    /// High-performance frozen set of reserved names for O(1) lookup performance.
    /// </summary>
    /// <remarks>
    /// Uses FrozenSet for optimal read performance in validation scenarios.
    /// Case-insensitive comparison for user-friendly validation.
    /// </remarks>
    private static readonly FrozenSet<string> _reservedNamesSet =
        _reservedNamesArray.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the read-only list of reserved usernames.
    /// These names cannot be used for user accounts.
    /// </summary>
    /// <remarks>
    /// Returns an immutable view of the reserved names for external consumption.
    /// This property provides thread-safe access to the reserved names list.
    /// </remarks>
    public static IReadOnlyList<string> ReservedNames => _reservedNamesArray.ToImmutableArray();

    #endregion Constants

    #region Core Validation Methods

    /// <summary>
    /// Determines whether the specified username is allowed according to ADMS business rules.
    /// </summary>
    /// <param name="name">The username to validate. Can be null or whitespace.</param>
    /// <returns>
    /// <c>true</c> if the name is allowed; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// A valid username must meet all the following criteria:
    /// <list type="bullet">
    /// <item>Not be null or whitespace</item>
    /// <item>Be between {MinUserNameLength} and {MaxUserNameLength} characters long (matching database constraint)</item>
    /// <item>Not be a reserved system name (case-insensitive check)</item>
    /// <item>Contain only valid characters (letters, numbers, spaces, periods, hyphens, underscores)</item>
    /// <item>Not start or end with special characters (must start/end with letter or number)</item>
    /// <item>Not contain consecutive special characters</item>
    /// <item>Not contain multiple consecutive spaces</item>
    /// </list>
    /// 
    /// <para><strong>Performance:</strong></para>
    /// Uses FrozenSet for O(1) average lookup performance on reserved names check.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid1 = UserValidationHelper.IsNameAllowed("John Doe");         // true
    /// bool isValid2 = UserValidationHelper.IsNameAllowed("j.smith");          // true
    /// bool isValid3 = UserValidationHelper.IsNameAllowed("user123");          // true
    /// bool isInvalid1 = UserValidationHelper.IsNameAllowed("admin");          // false (reserved)
    /// bool isInvalid2 = UserValidationHelper.IsNameAllowed(".john");          // false (starts with special)
    /// bool isInvalid3 = UserValidationHelper.IsNameAllowed("john..doe");      // false (consecutive specials)
    /// bool isInvalid4 = UserValidationHelper.IsNameAllowed("");               // false (empty)
    /// </code>
    /// </example>
    public static bool IsNameAllowed([NotNullWhen(true)] string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var trimmed = name.Trim();

        // Length validation
        if (trimmed.Length < MinUserNameLength || trimmed.Length > MaxUserNameLength)
            return false;

        // Reserved name check using high-performance frozen set
        return !_reservedNamesSet.Contains(trimmed) &&
               // Character and format validation
               IsValidUserNameFormat(trimmed);
    }

    /// <summary>
    /// Determines whether the specified user ID is valid.
    /// A valid user ID is a non-empty GUID.
    /// </summary>
    /// <param name="userId">The user ID to validate.</param>
    /// <returns>
    /// <c>true</c> if the user ID is not empty; otherwise, <c>false</c>.
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
    /// bool isValid = UserValidationHelper.IsValidUserId(Guid.NewGuid());  // true
    /// bool isInvalid = UserValidationHelper.IsValidUserId(Guid.Empty);    // false
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidUserId(Guid userId) => userId != Guid.Empty;

    /// <summary>
    /// Determines whether the specified date is valid for user-related operations.
    /// A valid date is within reasonable bounds and not in the future.
    /// </summary>
    /// <param name="date">The date to validate.</param>
    /// <returns>
    /// <c>true</c> if the date is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method follows the same validation pattern as other ADMS validation helpers,
    /// ensuring consistency across the validation system.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Must be after the minimum allowed date ({MinAllowedUserDate:yyyy-MM-dd})</item>
    /// <item>Cannot be in the future (with {FutureDateToleranceMinutes} minute tolerance)</item>
    /// <item>Must not be DateTime.MinValue or other sentinel values</item>
    /// </list>
    /// 
    /// <para><strong>Timezone Handling:</strong></para>
    /// The method works with any DateTime kind but normalizes to UTC for comparison.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isValid1 = UserValidationHelper.IsValidUserDate(DateTime.UtcNow);                    // true
    /// bool isValid2 = UserValidationHelper.IsValidUserDate(new DateTime(2020, 1, 1));          // true
    /// bool isInvalid1 = UserValidationHelper.IsValidUserDate(DateTime.MinValue);                // false (too early)
    /// bool isInvalid2 = UserValidationHelper.IsValidUserDate(DateTime.UtcNow.AddHours(1));      // false (future)
    /// bool isInvalid3 = UserValidationHelper.IsValidUserDate(new DateTime(1975, 1, 1));        // false (too early)
    /// </code>
    /// </example>
    public static bool IsValidUserDate(DateTime date)
    {
        var utcDate = date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(date, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(date, DateTimeKind.Utc)
        };

        return utcDate > DateTime.MinValue &&
               utcDate >= MinAllowedUserDate &&
               utcDate <= DateTime.UtcNow.AddMinutes(FutureDateToleranceMinutes);
    }

    #endregion Core Validation Methods

    #region Comprehensive Validation Methods

    /// <summary>
    /// Performs comprehensive validation of a username and returns detailed validation results.
    /// </summary>
    /// <param name="name">The username to validate. Can be null.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages). Cannot be null or whitespace.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    /// <remarks>
    /// This method performs all username validation checks in a single call:
    /// <list type="bullet">
    /// <item>Null/whitespace validation</item>
    /// <item>Length validation (min/max bounds)</item>
    /// <item>Reserved name validation</item>
    /// <item>Character format validation</item>
    /// <item>Professional naming standards validation</item>
    /// </list>
    /// 
    /// <para><strong>Validation Order:</strong></para>
    /// Validations are performed in order of severity, with early termination for null values.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = UserValidationHelper.ValidateName("admin", nameof(MyDto.Name));
    /// if (results.Any())
    /// {
    ///     foreach (var result in results)
    ///     {
    ///         Console.WriteLine($"Error: {result.ErrorMessage}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<ValidationResult> ValidateName(
        string? name,
        [NotNull] string propertyName)
    {
        // Validate input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Validate name is not null or whitespace
        if (string.IsNullOrWhiteSpace(name))
        {
            yield return new ValidationResult(
                $"{propertyName} is required and cannot be empty.",
                [propertyName]);
            yield break; // No point in further validation
        }

        var trimmed = name.Trim();

        switch (trimmed.Length)
        {
            // Validate length constraints
            case < MinUserNameLength:
                yield return new ValidationResult(
                    $"{propertyName} must be at least {MinUserNameLength} characters long.",
                    [propertyName]);
                break;
            case > MaxUserNameLength:
                yield return new ValidationResult(
                    $"{propertyName} cannot exceed {MaxUserNameLength} characters.",
                    [propertyName]);
                break;
        }

        // Check for reserved names
        if (_reservedNamesSet.Contains(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} is a reserved name and cannot be used. Please choose a different name.",
                [propertyName]);
        }

        // Validate character format
        if (IsValidUserNameFormat(trimmed)) yield break;
        if (!UserNameCharacterRegex().IsMatch(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} can only contain letters, numbers, spaces, periods, hyphens, and underscores.",
                [propertyName]);
        }
        else if (trimmed.StartsWith('.') || trimmed.StartsWith('_') || trimmed.StartsWith('-'))
        {
            yield return new ValidationResult(
                $"{propertyName} cannot start with a special character.",
                [propertyName]);
        }
        else if (trimmed.EndsWith('.') || trimmed.EndsWith('_') || trimmed.EndsWith('-'))
        {
            yield return new ValidationResult(
                $"{propertyName} cannot end with a special character.",
                [propertyName]);
        }
        else if (trimmed.Contains("..") || trimmed.Contains("__") || trimmed.Contains("--"))
        {
            yield return new ValidationResult(
                $"{propertyName} cannot contain consecutive special characters.",
                [propertyName]);
        }
        else if (trimmed.Contains("  "))
        {
            yield return new ValidationResult(
                $"{propertyName} cannot contain multiple consecutive spaces.",
                [propertyName]);
        }
        else
        {
            yield return new ValidationResult(
                $"{propertyName} contains invalid characters or format.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Performs comprehensive validation of a user ID.
    /// </summary>
    /// <param name="userId">The user ID to validate.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages). Cannot be null or whitespace.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    /// <remarks>
    /// This method validates that the user ID represents a valid, non-empty GUID suitable
    /// for use as a database identifier.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = UserValidationHelper.ValidateUserId(Guid.Empty, nameof(MyDto.UserId));
    /// if (results.Any())
    /// {
    ///     Console.WriteLine($"User ID validation failed: {string.Join(", ", results.Select(r => r.ErrorMessage))}");
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<ValidationResult> ValidateUserId(
        Guid userId,
        [NotNull] string propertyName)
    {
        // Validate input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (userId == Guid.Empty)
        {
            yield return new ValidationResult(
                $"{propertyName} must be a valid non-empty GUID.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Performs comprehensive validation of a user-related date.
    /// </summary>
    /// <param name="date">The date to validate.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages). Cannot be null or whitespace.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of whitespace.</exception>
    /// <remarks>
    /// This method validates that dates are within acceptable bounds for user-related operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = UserValidationHelper.ValidateDate(DateTime.MinValue, nameof(MyDto.CreatedDate));
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

        if (IsValidUserDate(date)) yield break;
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

    #endregion Comprehensive Validation Methods

    #region Normalization Methods

    /// <summary>
    /// Returns the normalized version of a username for consistent storage.
    /// </summary>
    /// <param name="name">The username to normalize. Can be null.</param>
    /// <returns>The normalized username, or <c>null</c> if input is invalid.</returns>
    /// <remarks>
    /// Normalization includes:
    /// <list type="bullet">
    /// <item>Trimming leading and trailing whitespace</item>
    /// <item>Collapsing multiple consecutive spaces to single spaces</item>
    /// <item>Preserving case for professional names</item>
    /// <item>Maintaining original character encoding</item>
    /// </list>
    /// 
    /// <para><strong>Professional Name Support:</strong></para>
    /// Unlike some systems that force lowercase, this normalization preserves
    /// case to maintain professional appearance of names like "John Doe" or "Dr. Smith".
    /// </remarks>
    /// <example>
    /// <code>
    /// string? normalized1 = UserValidationHelper.NormalizeName("  John   Doe  ");  // "John Doe"
    /// string? normalized2 = UserValidationHelper.NormalizeName("j.smith");         // "j.smith"
    /// string? normalized3 = UserValidationHelper.NormalizeName("");                // null
    /// string? normalized4 = UserValidationHelper.NormalizeName(null);              // null
    /// </code>
    /// </example>
    [return: NotNullIfNotNull(nameof(name))]
    public static string? NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var trimmed = name.Trim();
        return string.IsNullOrEmpty(trimmed) ? null :
            // Collapse multiple spaces to single spaces using compiled regex
            MultipleSpacesRegex().Replace(trimmed, " ");
    }

    /// <summary>
    /// Normalizes a user-related date to UTC for consistent storage.
    /// </summary>
    /// <param name="date">The date to normalize.</param>
    /// <returns>
    /// The date normalized to UTC, or null if the input date is invalid.
    /// </returns>
    /// <remarks>
    /// This method ensures all user-related dates are stored in UTC for consistency,
    /// while validating the date meets basic requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// var localDate = DateTime.Now;
    /// DateTime? utcDate = UserValidationHelper.NormalizeDateToUtc(localDate);
    /// // Returns the date converted to UTC if valid
    /// 
    /// DateTime? invalid = UserValidationHelper.NormalizeDateToUtc(DateTime.MinValue);
    /// // Returns null for invalid dates
    /// </code>
    /// </example>
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

    #region Utility Methods

    /// <summary>
    /// Checks if two usernames are equivalent after normalization.
    /// </summary>
    /// <param name="userName1">The first username. Can be null.</param>
    /// <param name="userName2">The second username. Can be null.</param>
    /// <returns><c>true</c> if the usernames are equivalent; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method is useful for checking username uniqueness while handling
    /// minor formatting differences like extra spaces. Uses case-insensitive
    /// comparison to handle variations in capitalization.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool same1 = UserValidationHelper.AreUserNamesEquivalent("John Doe", "john doe");       // true
    /// bool same2 = UserValidationHelper.AreUserNamesEquivalent("  John  Doe  ", "John Doe");  // true
    /// bool different = UserValidationHelper.AreUserNamesEquivalent("John Doe", "Jane Doe");   // false
    /// bool bothNull = UserValidationHelper.AreUserNamesEquivalent(null, null);                // false
    /// </code>
    /// </example>
    public static bool AreUserNamesEquivalent(string? userName1, string? userName2)
    {
        var normalized1 = NormalizeName(userName1);
        var normalized2 = NormalizeName(userName2);

        return !string.IsNullOrEmpty(normalized1) &&
               !string.IsNullOrEmpty(normalized2) &&
               string.Equals(normalized1, normalized2, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Suggests alternative usernames if the provided name is not allowed.
    /// </summary>
    /// <param name="attemptedName">The attempted username. Can be null.</param>
    /// <param name="maxSuggestions">Maximum number of suggestions to generate.</param>
    /// <returns>A read-only list of suggested alternative usernames.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxSuggestions"/> is less than 1.</exception>
    /// <remarks>
    /// This method provides user-friendly suggestions when a username is rejected
    /// due to validation rules or uniqueness constraints. Uses multiple strategies:
    /// <list type="bullet">
    /// <item>Cleaning invalid characters from the attempted name</item>
    /// <item>Adding numeric suffixes to create unique variations</item>
    /// <item>Shortening names that exceed length limits</item>
    /// <item>Providing alternative formatting suggestions</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var suggestions = UserValidationHelper.SuggestAlternativeNames("admin", 3);
    /// // Returns alternatives like ["administrator1", "admin123", "user-admin"]
    /// 
    /// var suggestions2 = UserValidationHelper.SuggestAlternativeNames("john..doe", 3);
    /// // Returns cleaned alternatives like ["john.doe", "johndoe", "john_doe"]
    /// </code>
    /// </example>
    public static IReadOnlyList<string> SuggestAlternativeNames(
        string? attemptedName,
        int maxSuggestions = MaxUsernameSuggestions)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxSuggestions, 1);

        var suggestions = new List<string>();

        if (string.IsNullOrWhiteSpace(attemptedName))
        {
            // Return generic suggestions if no input provided
            return new List<string> { "user1", "user123", "newuser", "member", "person" }
                .Take(maxSuggestions).ToImmutableArray();
        }

        var baseName = attemptedName.Trim();
        var random = new Random();

        // Strategy 1: Clean the attempted name
        var cleaned = CleanUserName(baseName);
        if (!string.IsNullOrEmpty(cleaned) && IsNameAllowed(cleaned))
        {
            suggestions.Add(cleaned);
        }

        // Strategy 2: Add numeric suffixes
        var nameBase = cleaned ?? baseName;
        if (nameBase.Length <= MaxUserNameLength - 3) // Leave room for numbers
        {
            for (var i = 1; suggestions.Count < maxSuggestions && i <= 20; i++)
            {
                var suggestion = $"{nameBase}{i}";
                if (suggestion.Length <= MaxUserNameLength && IsNameAllowed(suggestion))
                {
                    suggestions.Add(suggestion);
                }
            }
        }

        // Strategy 3: Random numeric suffixes
        while (suggestions.Count < maxSuggestions)
        {
            var suffix = random.Next(10, 9999);
            var suggestion = nameBase.Length + suffix.ToString().Length <= MaxUserNameLength
                ? $"{nameBase}{suffix}"
                : $"{nameBase[..(MaxUserNameLength - suffix.ToString().Length)]}{suffix}";

            if (IsNameAllowed(suggestion) && !suggestions.Contains(suggestion))
            {
                suggestions.Add(suggestion);
            }

            // Prevent infinite loops
            if (suggestions.Count == 0 && random.Next(1, 10) > 7)
                break;
        }

        return suggestions.Take(maxSuggestions).ToImmutableArray();
    }

    /// <summary>
    /// Checks if a username is reserved (case-insensitive).
    /// </summary>
    /// <param name="name">The username to check. Can be null.</param>
    /// <returns><c>true</c> if the username is reserved; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method provides a direct way to check if a username conflicts with
    /// reserved system names without performing full validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool isReserved1 = UserValidationHelper.IsReservedName("admin");    // true
    /// bool isReserved2 = UserValidationHelper.IsReservedName("ADMIN");    // true (case insensitive)
    /// bool isNotReserved = UserValidationHelper.IsReservedName("johndoe"); // false
    /// </code>
    /// </example>
    public static bool IsReservedName([NotNullWhen(true)] string? name)
    {
        return !string.IsNullOrWhiteSpace(name) && _reservedNamesSet.Contains(name.Trim());
    }

    #endregion Utility Methods

    #region Diagnostic and Statistics Methods

    /// <summary>
    /// Gets detailed validation information about a username for diagnostic purposes.
    /// </summary>
    /// <param name="name">The username to analyze. Can be null.</param>
    /// <returns>
    /// A dictionary containing detailed validation results and diagnostic information.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive diagnostic information about username validation,
    /// useful for troubleshooting validation failures and providing detailed analysis.
    /// 
    /// <para><strong>Diagnostic Information Includes:</strong></para>
    /// <list type="bullet">
    /// <item>Basic validation results (null, length, reserved)</item>
    /// <item>Character analysis and format validation</item>
    /// <item>Normalization results</item>
    /// <item>Professional naming assessment</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = UserValidationHelper.GetNameValidationDetails("John Doe");
    /// foreach (var result in results)
    /// {
    ///     Console.WriteLine($"{result.Key}: {result.Value}");
    /// }
    /// </code>
    /// </example>
    public static IReadOnlyDictionary<string, object> GetNameValidationDetails(string? name)
    {
        var details = new Dictionary<string, object>
        {
            ["IsNotNullOrEmpty"] = !string.IsNullOrWhiteSpace(name),
            ["OriginalLength"] = name?.Length ?? 0,
            ["TrimmedLength"] = name?.Trim().Length ?? 0,
            ["IsValidLength"] = name != null && IsValidNameLength(name),
            ["IsReserved"] = IsReservedName(name),
            ["HasValidFormat"] = name != null && IsValidUserNameFormat(name.Trim()),
            ["NormalizedName"] = NormalizeName(name),
            ["IsOverallValid"] = IsNameAllowed(name)
        };

        if (string.IsNullOrWhiteSpace(name)) return details.ToImmutableDictionary();
        var trimmed = name.Trim();
        details["ContainsSpaces"] = trimmed.Contains(' ');
        details["ContainsPeriods"] = trimmed.Contains('.');
        details["ContainsHyphens"] = trimmed.Contains('-');
        details["ContainsUnderscores"] = trimmed.Contains('_');
        details["ContainsNumbers"] = trimmed.Any(char.IsDigit);
        details["ContainsLetters"] = trimmed.Any(char.IsLetter);
        details["StartsWithLetter"] = char.IsLetter(trimmed[0]);
        details["EndsWithLetter"] = char.IsLetter(trimmed[^1]);
        details["HasConsecutiveSpaces"] = trimmed.Contains("  ");
        details["WordCount"] = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

        return details.ToImmutableDictionary();
    }

    /// <summary>
    /// Gets validation statistics for the user validation system.
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
    /// var stats = UserValidationHelper.GetValidationStatistics();
    /// Console.WriteLine($"Min username length: {stats["MinUserNameLength"]}");
    /// Console.WriteLine($"Max username length: {stats["MaxUserNameLength"]}");
    /// Console.WriteLine($"Reserved names count: {stats["ReservedNamesCount"]}");
    /// </code>
    /// </example>
    public static IReadOnlyDictionary<string, object> GetValidationStatistics()
    {
        return new Dictionary<string, object>
        {
            ["MinUserNameLength"] = MinUserNameLength,
            ["MaxUserNameLength"] = MaxUserNameLength,
            ["ReservedNamesCount"] = ReservedNames.Count,
            ["MinAllowedDate"] = MinAllowedUserDate,
            ["FutureDateToleranceMinutes"] = FutureDateToleranceMinutes,
            ["MaxUsernameSuggestions"] = MaxUsernameSuggestions,
            ["ReservedNames"] = ReservedNames,
            ["ValidationRules"] = new Dictionary<string, string>
            {
                ["NameLength"] = $"Must be between {MinUserNameLength} and {MaxUserNameLength} characters",
                ["AllowedCharacters"] = "Letters, numbers, spaces, periods, hyphens, underscores",
                ["StartEndRule"] = "Must start and end with letter or number",
                ["ConsecutiveRule"] = "No consecutive special characters or spaces",
                ["ReservedRule"] = "Cannot use reserved system names",
                ["CaseRule"] = "Case-insensitive comparison for reserved names"
            }.ToImmutableDictionary()
        }.ToImmutableDictionary();
    }

    /// <summary>
    /// Generates a comprehensive validation report for a username.
    /// </summary>
    /// <param name="name">The username to analyze. Can be null.</param>
    /// <returns>
    /// A formatted string containing detailed validation information.
    /// </returns>
    /// <remarks>
    /// This method provides a human-readable validation report useful for debugging
    /// and troubleshooting username validation issues.
    /// </remarks>
    /// <example>
    /// <code>
    /// string report = UserValidationHelper.GenerateValidationReport("admin");
    /// Console.WriteLine(report);
    /// // Outputs detailed validation results and suggestions
    /// </code>
    /// </example>
    public static string GenerateValidationReport(string? name)
    {
        var report = new StringBuilder();
        report.AppendLine($"Username Validation Report for: '{name ?? "<null>"}'");
        report.AppendLine(new string('=', 45));

        var details = GetNameValidationDetails(name);
        foreach (var detail in details)
        {
            var status = detail.Value switch
            {
                bool b => b ? "✓ Yes" : "✗ No",
                _ => detail.Value?.ToString() ?? "<null>"
            };
            report.AppendLine($"{detail.Key}: {status}");
        }

        report.AppendLine();
        report.AppendLine($"Normalized Name: '{NormalizeName(name) ?? "<invalid>"}'");
        report.AppendLine($"Overall Result: {(IsNameAllowed(name) ? "✓ VALID" : "✗ INVALID")}");

        if (!IsNameAllowed(name))
        {
            report.AppendLine();
            report.AppendLine("Suggestions:");
            var suggestions = SuggestAlternativeNames(name, 3);
            foreach (var suggestion in suggestions)
            {
                report.AppendLine($"  - {suggestion}");
            }
        }

        report.AppendLine();
        report.AppendLine($"Reserved Names (first 10): {string.Join(", ", ReservedNames.Take(10))}");
        if (ReservedNames.Count > 10)
        {
            report.AppendLine($"... and {ReservedNames.Count - 10} more");
        }

        return report.ToString();
    }

    #endregion Diagnostic and Statistics Methods

    #region Private Helper Methods

    /// <summary>
    /// Validates if a username length is within acceptable bounds.
    /// </summary>
    /// <param name="name">The username to validate.</param>
    /// <returns><c>true</c> if the length is valid; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidNameLength([NotNullWhen(true)] string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var trimmedLength = name.Trim().Length;
        return trimmedLength >= MinUserNameLength && trimmedLength <= MaxUserNameLength;
    }

    /// <summary>
    /// Validates username format using comprehensive rules.
    /// </summary>
    /// <param name="userName">The username to validate (must be trimmed).</param>
    /// <returns><c>true</c> if the format is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method performs comprehensive format validation including:
    /// <list type="bullet">
    /// <item>Character set validation (letters, numbers, allowed specials)</item>
    /// <item>Start/end character validation (must be letter or number)</item>
    /// <item>Consecutive special character prevention</item>
    /// <item>Multiple space prevention</item>
    /// </list>
    /// </remarks>
    private static bool IsValidUserNameFormat(string userName)
    {
        // Quick length check for single character usernames
        if (userName.Length == 1)
            return char.IsLetterOrDigit(userName[0]);

        // Check for valid characters using compiled regex
        if (!UserNameFormatRegex().IsMatch(userName))
            return false;

        // Additional format checks for multi-character names

        // Check start and end characters
        if (!char.IsLetterOrDigit(userName[0]) || !char.IsLetterOrDigit(userName[^1]))
            return false;

        // Check for consecutive special characters (but allow consecutive letters/numbers)
        for (var i = 0; i < userName.Length - 1; i++)
        {
            var current = userName[i];
            var next = userName[i + 1];

            switch (current)
            {
                // Check for consecutive periods, underscores, or hyphens
                case '.' when next == '.':
                case '_' when next == '_':
                case '-' when next == '-':
                // Check for consecutive spaces
                case ' ' when next == ' ':
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Cleans a username by removing or replacing invalid characters.
    /// </summary>
    /// <param name="name">The username to clean.</param>
    /// <returns>The cleaned username, or null if it cannot be cleaned.</returns>
    private static string? CleanUserName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var cleaned = name.Trim();

        // Replace consecutive spaces with single spaces
        cleaned = MultipleSpacesRegex().Replace(cleaned, " ");

        // Replace consecutive special characters
        cleaned = cleaned.Replace("..", ".").Replace("__", "_").Replace("--", "-");

        // Remove special characters from start and end
        while (cleaned.Length > 0 && !char.IsLetterOrDigit(cleaned[0]))
            cleaned = cleaned[1..];

        while (cleaned.Length > 0 && !char.IsLetterOrDigit(cleaned[^1]))
            cleaned = cleaned[..^1];

        // Truncate if too long
        if (cleaned.Length > MaxUserNameLength)
            cleaned = cleaned[..MaxUserNameLength];

        // Check if result is valid and not reserved
        return cleaned.Length >= MinUserNameLength &&
               IsValidUserNameFormat(cleaned) &&
               !_reservedNamesSet.Contains(cleaned)
            ? cleaned
            : null;
    }

    #endregion Private Helper Methods

    #region Compiled Regex Patterns

    /// <summary>
    /// Compiled regex for username format validation.
    /// </summary>
    /// <remarks>
    /// Pattern allows letters, numbers, spaces, periods, hyphens, and underscores.
    /// The pattern handles both single character and multi-character names.
    /// 
    /// <para><strong>Pattern Details:</strong></para>
    /// <list type="bullet">
    /// <item>^[a-zA-Z0-9][a-zA-Z0-9._\s-]*[a-zA-Z0-9]$ - Multi-character names</item>
    /// <item>^[a-zA-Z0-9]$ - Single character names</item>
    /// </list>
    /// 
    /// Additional validation for consecutive characters is handled separately
    /// for better performance and clearer error messages.
    /// </remarks>
    [GeneratedRegex(@"^[a-zA-Z0-9][a-zA-Z0-9._\s-]*[a-zA-Z0-9]$|^[a-zA-Z0-9]$", RegexOptions.Compiled)]
    private static partial Regex UserNameFormatRegex();

    /// <summary>
    /// Compiled regex for checking valid username characters.
    /// </summary>
    /// <remarks>
    /// This regex is used for character set validation separate from format rules.
    /// It allows any combination of valid characters without position restrictions.
    /// 
    /// <para><strong>Pattern Details:</strong></para>
    /// <list type="bullet">
    /// <item>^[a-zA-Z0-9._\s-]+$ - One or more valid characters</item>
    /// </list>
    /// </remarks>
    [GeneratedRegex(@"^[a-zA-Z0-9._\s-]+$", RegexOptions.Compiled)]
    private static partial Regex UserNameCharacterRegex();

    /// <summary>
    /// Compiled regex for collapsing multiple spaces.
    /// </summary>
    /// <remarks>
    /// Used for normalizing usernames by replacing multiple consecutive
    /// spaces with a single space.
    /// 
    /// <para><strong>Pattern Details:</strong></para>
    /// <list type="bullet">
    /// <item>\s+ - One or more whitespace characters</item>
    /// </list>
    /// </remarks>
    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex MultipleSpacesRegex();

    #endregion Compiled Regex Patterns
}