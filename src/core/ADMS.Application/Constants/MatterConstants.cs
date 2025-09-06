namespace ADMS.Application.Constants;

/// <summary>
/// Constants for Matter validation and business rules.
/// </summary>
public static class MatterConstants
{
    /// <summary>
    /// Represents the maximum allowable length, in characters, for a description.
    /// </summary>
    public const int DescriptionMaxLength = 128;

    /// <summary>
    /// The minimum allowable length for a description.
    /// </summary>
    public const int DescriptionMinLength = 3;

    /// <summary>
    /// Represents the maximum allowed length for an activity name.
    /// </summary>
    /// <remarks>This constant defines the upper limit for the number of characters in an activity name. It
    /// can be used to validate input or enforce constraints in systems that handle activity data.</remarks>
    public const int ActivityMaxLength = 50;

    /// <summary>
    /// The minimum length required for an activity name.
    /// </summary>
    public const int ActivityMinLength = 2;

    // Age limits for validation
    /// <summary>
    /// Represents the maximum number of years in the past that can be used for validation.
    /// </summary>
    public const int MaxHistoricalYears = 20;

    /// <summary>
    /// Represents the maximum number of days that can be backdated, equivalent to 10 years.
    /// </summary>
    public const int MaxBackdatedDays = 365 * 10; // 10 years

    // Performance thresholds
    /// <summary>
    /// Represents the threshold for determining a large collection of documents.
    /// </summary>
    /// <remarks>This constant defines the number of documents that qualifies a collection as "large." It can
    /// be used to optimize operations or apply specific logic for large document collections.</remarks>
    public const int LargeDocumentCollectionThreshold = 100;

    /// <summary>
    /// Represents the threshold for determining high activity counts.
    /// </summary>
    /// <remarks>This constant defines the value used to classify an activity count as "high." It can be used
    /// in scenarios where activity levels need to be monitored or categorized.</remarks>
    public const int HighActivityCountThreshold = 1000;
}