using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ADMS.Application.Common.Validation;

/// <summary>
/// Provides validation helper methods for user-related data within the ADMS document management solution.
/// </summary>
/// <remarks>
/// This validation helper follows Clean Architecture principles and supports the core User entity requirements:
/// <list type="bullet">
/// <item><strong>ID Validation:</strong> Ensures valid GUID identifiers for database operations</item>
/// <item><strong>Username Validation:</strong> Validates display names for human users performing actions</item>
/// <item><strong>Activity Context Validation:</strong> Supports audit trail requirements for user actions</item>
/// </list>
/// 
/// <para><strong>User Context in ADMS:</strong></para>
/// Users in this system represent individual human beings performing actions like:
/// <list type="bullet">
/// <item>Creating and managing matters</item>
/// <item>Uploading and managing documents</item>
/// <item>Creating document revisions</item>
/// <item>Moving/copying documents between matters</item>
/// </list>
/// </remarks>
public static partial class UserValidationHelper
{
    #region Core Constants

    /// <summary>
    /// Maximum allowed length for a username (matches domain constraints).
    /// </summary>
    public const int MaxUserNameLength = 50;

    /// <summary>
    /// Minimum allowed length for a username.
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

    /// <summary>
    /// Maximum reasonable daily activity count for monitoring.
    /// </summary>
    public const int MaxDailyActivityCount = 200;

    #endregion Core Constants

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
        "support", "help", "info", "contact", "webmaster",
    
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

    #region Core Validation Methods

    /// <summary>
    /// Validates a user ID according to business rules.
    /// </summary>
    /// <param name="userId">The user ID to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateUserId(Guid userId, [NotNull] string propertyName)
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
    /// Validates a username according to business rules.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates the username for display purposes, supporting human-readable names
    /// for individuals performing actions in the document management system.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateUsername(string? username, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Required validation
        if (string.IsNullOrWhiteSpace(username))
        {
            yield return new ValidationResult(
                $"{propertyName} is required and cannot be empty.",
                [propertyName]);
            yield break;
        }

        var trimmed = username.Trim();

        switch (trimmed.Length)
        {
            // Length validation
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

        // Reserved name validation
        if (_reservedNamesSet.Contains(trimmed))
        {
            yield return new ValidationResult(
                $"{propertyName} is a reserved name and cannot be used.",
                [propertyName]);
        }

        // Format validation
        if (!IsValidUsernameFormat(trimmed))
        {
            yield return new ValidationResult(
                GetFormatErrorMessage(propertyName, trimmed),
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates a user activity timestamp according to business rules.
    /// </summary>
    /// <param name="timestamp">The timestamp to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateActivityTimestamp(DateTime timestamp, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (timestamp <= DateTime.MinValue)
        {
            yield return new ValidationResult(
                $"{propertyName} must be a valid date.",
                [propertyName]);
        }
        else if (timestamp < MinAllowedUserDate)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot be earlier than {MinAllowedUserDate:yyyy-MM-dd}.",
                [propertyName]);
        }
        else if (timestamp > DateTime.UtcNow.AddMinutes(FutureDateToleranceMinutes))
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
    /// Quick validation check for username (optimized for performance).
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsUsernameAllowed([NotNullWhen(true)] string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        var trimmed = username.Trim();

        return trimmed.Length >= MinUserNameLength &&
               trimmed.Length <= MaxUserNameLength &&
               !_reservedNamesSet.Contains(trimmed) &&
               IsValidUsernameFormat(trimmed);
    }

    /// <summary>
    /// Quick validation check for activity timestamp (optimized for performance).
    /// </summary>
    /// <param name="timestamp">The timestamp to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidActivityTimestamp(DateTime timestamp)
    {
        var utcDate = timestamp.Kind != DateTimeKind.Utc ? timestamp.ToUniversalTime() : timestamp;

        return utcDate > MinAllowedUserDate &&
               utcDate <= DateTime.UtcNow.AddMinutes(FutureDateToleranceMinutes);
    }

    /// <summary>
    /// Checks if a username is reserved (optimized for performance).
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <returns>True if reserved; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReservedUsername([NotNullWhen(true)] string? username)
    {
        return !string.IsNullOrWhiteSpace(username) && _reservedNamesSet.Contains(username.Trim());
    }

    #endregion Quick Validation Methods

    #region Activity Context Validation

    /// <summary>
    /// Validates user attribution for activities (document operations, matter actions, etc.).
    /// </summary>
    /// <param name="userId">The user ID performing the action.</param>
    /// <param name="username">The username for display purposes.</param>
    /// <param name="activityTimestamp">The timestamp when the activity occurred.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates the complete user attribution context required for audit trails
    /// in the document management system.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateUserAttribution(
        Guid userId,
        string? username,
        DateTime activityTimestamp,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Validate user ID
        foreach (var result in ValidateUserId(userId, $"{propertyName}.UserId"))
            yield return result;

        // Validate username
        foreach (var result in ValidateUsername(username, $"{propertyName}.Username"))
            yield return result;

        // Validate timestamp
        foreach (var result in ValidateActivityTimestamp(activityTimestamp, $"{propertyName}.Timestamp"))
            yield return result;
    }

    /// <summary>
    /// Validates user activity metrics to detect unusual patterns.
    /// </summary>
    /// <param name="activityCount">The activity count to validate.</param>
    /// <param name="timeFrameHours">The time frame in hours for the activity count.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Helps identify potential issues like automated behavior or system problems
    /// by validating activity levels are within reasonable human ranges.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateActivityMetrics(
        int activityCount,
        double timeFrameHours,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (activityCount < 0)
        {
            yield return new ValidationResult(
                $"{propertyName} cannot be negative.",
                [propertyName]);
        }

        if (timeFrameHours <= 0)
        {
            yield return new ValidationResult(
                "Time frame must be positive.",
                [propertyName]);
        }

        // Check for excessive activity
        if (timeFrameHours <= 24 && activityCount > MaxDailyActivityCount)
        {
            yield return new ValidationResult(
                $"{propertyName} exceeds reasonable daily activity limit ({MaxDailyActivityCount}). Please verify user behavior.",
                [propertyName]);
        }
    }

    #endregion Activity Context Validation

    #region Normalization Methods

    /// <summary>
    /// Normalizes a username for consistent storage and comparison.
    /// </summary>
    /// <param name="username">The username to normalize.</param>
    /// <returns>Normalized username or null if invalid.</returns>
    /// <remarks>
    /// Normalization includes:
    /// <list type="bullet">
    /// <item>Trimming whitespace</item>
    /// <item>Collapsing multiple spaces to single spaces</item>
    /// <item>Preserving case for proper display</item>
    /// </list>
    /// </remarks>
    [return: NotNullIfNotNull(nameof(username))]
    public static string? NormalizeUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        var trimmed = username.Trim();
        return trimmed.Length == 0 ? string.Empty : MultipleSpacesRegex().Replace(trimmed, " ");
    }

    /// <summary>
    /// Normalizes a timestamp to UTC for consistent storage.
    /// </summary>
    /// <param name="timestamp">The timestamp to normalize.</param>
    /// <returns>UTC timestamp or null if invalid.</returns>
    public static DateTime? NormalizeTimestampToUtc(DateTime timestamp)
    {
        if (!IsValidActivityTimestamp(timestamp))
            return null;

        return timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
        };
    }

    #endregion Normalization Methods

    #region Utility Methods

    /// <summary>
    /// Checks if two usernames are equivalent after normalization.
    /// </summary>
    /// <param name="username1">First username to compare.</param>
    /// <param name="username2">Second username to compare.</param>
    /// <returns>True if equivalent; otherwise, false.</returns>
    public static bool AreUsernamesEquivalent(string? username1, string? username2)
    {
        var normalized1 = NormalizeUsername(username1);
        var normalized2 = NormalizeUsername(username2);

        return !string.IsNullOrEmpty(normalized1) &&
               !string.IsNullOrEmpty(normalized2) &&
               string.Equals(normalized1, normalized2, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if user context is sufficient for audit requirements.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="username">The username.</param>
    /// <param name="activityTimestamp">The activity timestamp.</param>
    /// <returns>True if context is sufficient; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSufficientUserContext(Guid userId, string? username, DateTime activityTimestamp)
    {
        return IsValidUserId(userId) &&
               IsUsernameAllowed(username) &&
               IsValidActivityTimestamp(activityTimestamp);
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
            ["MaxDailyActivityCount"] = MaxDailyActivityCount,
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
    /// Validates username format using comprehensive rules.
    /// </summary>
    /// <param name="username">The username to validate (must be trimmed).</param>
    /// <returns>True if format is valid; otherwise, false.</returns>
    private static bool IsValidUsernameFormat(string username)
    {
        switch (username.Length)
        {
            case 0:
                return false;
            case 1:
                return char.IsLetterOrDigit(username[0]);
        }

        // Check character set
        if (!UsernameCharacterRegex().IsMatch(username))
            return false;

        // Check start and end characters
        if (!char.IsLetterOrDigit(username[0]) || !char.IsLetterOrDigit(username[^1]))
            return false;

        // Check for consecutive special characters
        for (var i = 0; i < username.Length - 1; i++)
        {
            var current = username[i];
            var next = username[i + 1];

            // Prevent consecutive special characters or spaces
            if (IsSpecialCharacter(current) && (current == next || IsSpecialCharacter(next)))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines if a character is a special character allowed in usernames.
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

        return !UsernameCharacterRegex().IsMatch(value) ? $"{propertyName} can only contain letters, numbers, spaces, periods, hyphens, and underscores." : $"{propertyName} contains invalid format.";
    }

    #endregion Private Helper Methods

    #region Compiled Regex Patterns

    /// <summary>
    /// Compiled regex for validating username characters.
    /// </summary>
    [GeneratedRegex(@"^[a-zA-Z0-9._\s-]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex UsernameCharacterRegex();

    /// <summary>
    /// Compiled regex for collapsing multiple spaces.
    /// </summary>
    [GeneratedRegex(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MultipleSpacesRegex();

    #endregion Compiled Regex Patterns

    #region Activity Record Validation

    /// <summary>
    /// Valid activity types for each entity category in the system.
    /// </summary>
    private static readonly Dictionary<string, string[]> _validActivityTypes = new()
    {
        ["MATTER"] = ["CREATED", "ARCHIVED", "DELETED", "RESTORED", "UNARCHIVED", "VIEWED"],
        ["DOCUMENT"] = ["CREATED", "SAVED", "DELETED", "RESTORED", "CHECKED_IN", "CHECKED_OUT"],
        ["REVISION"] = ["CREATED", "SAVED", "DELETED", "RESTORED"],
        ["MATTER_DOCUMENT"] = ["MOVED", "COPIED"]
    };

    private static readonly FrozenSet<string> _allValidActivityTypes =
        _validActivityTypes.Values.SelectMany(x => x).ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Maximum time gap between related activities (prevents backdating).
    /// </summary>
    public const int MaxActivityBackdateHours = 24;

    /// <summary>
    /// Maximum activity burst count (activities within short timeframe).
    /// </summary>
    public const int MaxActivityBurstCount = 10;

    /// <summary>
    /// Activity burst window in minutes.
    /// </summary>
    public const int ActivityBurstWindowMinutes = 5;

    /// <summary>
    /// Validates a complete user activity record for integrity and business rules.
    /// </summary>
    /// <param name="userId">The user ID performing the activity.</param>
    /// <param name="username">The username for display purposes.</param>
    /// <param name="entityId">The ID of the entity being acted upon.</param>
    /// <param name="entityType">The type of entity (MATTER, DOCUMENT, REVISION, etc.).</param>
    /// <param name="activityType">The type of activity being performed.</param>
    /// <param name="activityTimestamp">The timestamp when the activity occurred.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Comprehensive validation ensuring activity record integrity, business rule compliance,
    /// and audit trail accuracy for the document management system.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateActivityRecord(
        Guid userId,
        string? username,
        Guid entityId,
        string? entityType,
        string? activityType,
        DateTime activityTimestamp,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // Core user attribution validation
        foreach (var result in ValidateUserAttribution(userId, username, activityTimestamp, propertyName))
            yield return result;

        // Entity ID validation
        if (entityId == Guid.Empty)
        {
            yield return new ValidationResult(
                $"{propertyName}.EntityId must be a valid non-empty GUID.",
                [$"{propertyName}.EntityId"]);
        }

        // Entity type validation
        foreach (var result in ValidateEntityType(entityType, $"{propertyName}.EntityType"))
            yield return result;

        // Activity type validation
        foreach (var result in ValidateActivityType(activityType, entityType, $"{propertyName}.ActivityType"))
            yield return result;

        // Timestamp business rules
        foreach (var result in ValidateActivityTimestampBusinessRules(activityTimestamp, $"{propertyName}.Timestamp"))
            yield return result;
    }

    /// <summary>
    /// Validates entity type for activity records.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateEntityType(string? entityType, [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (string.IsNullOrWhiteSpace(entityType))
        {
            yield return new ValidationResult(
                $"{propertyName} is required for activity classification.",
                [propertyName]);
            yield break;
        }

        var normalizedType = entityType.Trim().ToUpperInvariant();

        if (!_validActivityTypes.ContainsKey(normalizedType))
        {
            var validTypes = string.Join(", ", _validActivityTypes.Keys);
            yield return new ValidationResult(
                $"{propertyName} must be one of: {validTypes}.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates activity type for the given entity type.
    /// </summary>
    /// <param name="activityType">The activity type to validate.</param>
    /// <param name="entityType">The entity type context.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    public static IEnumerable<ValidationResult> ValidateActivityType(
        string? activityType,
        string? entityType,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (string.IsNullOrWhiteSpace(activityType))
        {
            yield return new ValidationResult(
                $"{propertyName} is required for activity identification.",
                [propertyName]);
            yield break;
        }

        var normalizedActivity = activityType.Trim().ToUpperInvariant();
        var normalizedEntity = entityType?.Trim().ToUpperInvariant();

        // Check if activity type is valid overall
        if (!_allValidActivityTypes.Contains(normalizedActivity))
        {
            var validActivities = string.Join(", ", _allValidActivityTypes);
            yield return new ValidationResult(
                $"{propertyName} must be one of: {validActivities}.",
                [propertyName]);
            yield break;
        }

        // Check if activity type is valid for the specific entity type
        if (normalizedEntity != null &&
            _validActivityTypes.TryGetValue(normalizedEntity, out var validActivitiesForEntity) &&
            !validActivitiesForEntity.Contains(normalizedActivity, StringComparer.OrdinalIgnoreCase))
        {
            var validForEntity = string.Join(", ", validActivitiesForEntity);
            yield return new ValidationResult(
                $"{propertyName} '{normalizedActivity}' is not valid for entity type '{normalizedEntity}'. Valid activities: {validForEntity}.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates activity timestamp against business rules for integrity.
    /// </summary>
    /// <param name="timestamp">The timestamp to validate.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Applies business rules to prevent backdating, future dating, and other timestamp anomalies
    /// that could compromise audit trail integrity.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateActivityTimestampBusinessRules(
        DateTime timestamp,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var utcTimestamp = timestamp.Kind != DateTimeKind.Utc ? timestamp.ToUniversalTime() : timestamp;
        var now = DateTime.UtcNow;

        // Basic timestamp validation
        foreach (var result in ValidateActivityTimestamp(timestamp, propertyName))
            yield return result;

        // Business rule: prevent excessive backdating
        if (utcTimestamp < now.AddHours(-MaxActivityBackdateHours))
        {
            yield return new ValidationResult(
                $"{propertyName} cannot be more than {MaxActivityBackdateHours} hours in the past. This helps maintain audit trail integrity.",
                [propertyName]);
        }

        // Business rule: check for suspicious precision (microsecond precision might indicate automation)
        if (timestamp.Millisecond == 0 && timestamp.Second == 0 && timestamp.Minute % 5 == 0)
        {
            // This is just a warning-level validation - exact times on 5-minute boundaries might indicate automation
            // In a real system, you might want to log this for investigation rather than fail validation
        }
    }

    /// <summary>
    /// Validates activity sequence to detect rapid-fire or suspicious patterns.
    /// </summary>
    /// <param name="activityTimestamps">Collection of recent activity timestamps for the user.</param>
    /// <param name="newActivityTimestamp">The new activity timestamp being validated.</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Helps detect suspicious activity patterns that might indicate automation, system abuse,
    /// or data integrity issues in the audit trail.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateActivitySequence(
        IEnumerable<DateTime>? activityTimestamps,
        DateTime newActivityTimestamp,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (activityTimestamps == null)
            yield break;

        var timestamps = activityTimestamps.ToList();
        if (timestamps.Count == 0)
            yield break;

        var utcNewTimestamp = newActivityTimestamp.Kind != DateTimeKind.Utc
            ? newActivityTimestamp.ToUniversalTime()
            : newActivityTimestamp;

        // Check for activity bursts (too many activities in short timeframe)
        var burstWindow = TimeSpan.FromMinutes(ActivityBurstWindowMinutes);
        var burstStart = utcNewTimestamp.Subtract(burstWindow);

        var activitiesInBurstWindow = timestamps.Count(t =>
        {
            var utcT = t.Kind != DateTimeKind.Utc ? t.ToUniversalTime() : t;
            return utcT >= burstStart && utcT <= utcNewTimestamp;
        });

        if (activitiesInBurstWindow >= MaxActivityBurstCount)
        {
            yield return new ValidationResult(
                $"{propertyName} indicates suspicious activity burst: {activitiesInBurstWindow} activities within {ActivityBurstWindowMinutes} minutes. Maximum allowed: {MaxActivityBurstCount}.",
                [propertyName]);
        }

        // Check for duplicate timestamps (exact duplicates are suspicious)
        var duplicateCount = timestamps.Count(t =>
        {
            var utcT = t.Kind != DateTimeKind.Utc ? t.ToUniversalTime() : t;
            return Math.Abs((utcT - utcNewTimestamp).TotalMilliseconds) < 100; // Within 100ms
        });

        if (duplicateCount > 0)
        {
            yield return new ValidationResult(
                $"{propertyName} has timestamp very close to existing activity ({duplicateCount} activities within 100ms). This may indicate data duplication.",
                [propertyName]);
        }
    }

    /// <summary>
    /// Validates activity consistency for related entity operations.
    /// </summary>
    /// <param name="userId">The user performing the activity.</param>
    /// <param name="entityId">The entity being acted upon.</param>
    /// <param name="activityType">The activity type.</param>
    /// <param name="relatedEntityId">Related entity ID (for transfer operations).</param>
    /// <param name="propertyName">The property name for validation messages.</param>
    /// <returns>Validation results (empty if valid).</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or whitespace.</exception>
    /// <remarks>
    /// Validates consistency rules for operations that span multiple entities,
    /// such as document transfers between matters.
    /// </remarks>
    public static IEnumerable<ValidationResult> ValidateActivityConsistency(
        Guid userId,
        Guid entityId,
        string? activityType,
        Guid? relatedEntityId,
        [NotNull] string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (string.IsNullOrWhiteSpace(activityType))
            yield break;

        var normalizedActivity = activityType.Trim().ToUpperInvariant();

        // Transfer operations require related entity
        if (normalizedActivity is "MOVED" or "COPIED")
        {
            if (!relatedEntityId.HasValue || relatedEntityId.Value == Guid.Empty)
            {
                yield return new ValidationResult(
                    $"{propertyName}: {normalizedActivity} operations require a valid related entity ID.",
                    [propertyName]);
            }
            else if (entityId == relatedEntityId.Value)
            {
                yield return new ValidationResult(
                    $"{propertyName}: Cannot {normalizedActivity.ToLowerInvariant()} entity to itself.",
                    [propertyName]);
            }
        }

        // User validation
        if (userId == Guid.Empty)
        {
            yield return new ValidationResult(
                $"{propertyName}: Activity must be attributed to a valid user.",
                [propertyName]);
        }
    }

    #endregion Activity Record Validation

    #region Activity Type Utilities

    /// <summary>
    /// Gets valid activity types for a specific entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>Array of valid activity types for the entity.</returns>
    public static string[] GetValidActivityTypes(string? entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return [];

        var normalizedType = entityType.Trim().ToUpperInvariant();
        return _validActivityTypes.TryGetValue(normalizedType, out var activities)
            ? activities
            : [];
    }

    /// <summary>
    /// Determines if an activity type is valid for a specific entity type.
    /// </summary>
    /// <param name="activityType">The activity type to check.</param>
    /// <param name="entityType">The entity type context.</param>
    /// <returns>True if the activity is valid for the entity type; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsActivityValidForEntity(string? activityType, string? entityType)
    {
        if (string.IsNullOrWhiteSpace(activityType) || string.IsNullOrWhiteSpace(entityType))
            return false;

        var normalizedActivity = activityType.Trim().ToUpperInvariant();
        var normalizedEntity = entityType.Trim().ToUpperInvariant();

        return _validActivityTypes.TryGetValue(normalizedEntity, out var validActivities) &&
               validActivities.Contains(normalizedActivity, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if an activity type requires a related entity (like transfers).
    /// </summary>
    /// <param name="activityType">The activity type to check.</param>
    /// <returns>True if the activity requires a related entity; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool RequiresRelatedEntity(string? activityType)
    {
        if (string.IsNullOrWhiteSpace(activityType))
            return false;

        var normalized = activityType.Trim().ToUpperInvariant();
        return normalized is "MOVED" or "COPIED";
    }

    /// <summary>
    /// Gets activity category for classification and reporting.
    /// </summary>
    /// <param name="activityType">The activity type.</param>
    /// <returns>Activity category string.</returns>
    public static string GetActivityCategory(string? activityType)
    {
        if (string.IsNullOrWhiteSpace(activityType))
            return "Unknown";

        return activityType.Trim().ToUpperInvariant() switch
        {
            "CREATED" => "Creation",
            "DELETED" or "RESTORED" => "Lifecycle",
            "ARCHIVED" or "UNARCHIVED" => "Archive",
            "SAVED" => "Modification",
            "VIEWED" => "Access",
            "CHECKED_IN" or "CHECKED_OUT" => "Version Control",
            "MOVED" or "COPIED" => "Transfer",
            _ => "Unknown"
        };
    }

    #endregion Activity Type Utilities
}