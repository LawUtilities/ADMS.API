using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Common.Interfaces;

/// <summary>
/// Repository interface for revision activity and revision activity user operations.
/// </summary>
/// <remarks>
/// This interface provides optimized data access methods for revision activities and their
/// associated user activities, designed to avoid N+1 query problems and provide efficient
/// access to audit trail data. The methods are organized by entity type and functionality.
/// 
/// <para><strong>Performance Focus:</strong></para>
/// All methods use optimized queries with proper projections to minimize data transfer
/// and avoid loading unnecessary navigation properties into memory.
/// 
/// <para><strong>Audit Trail Support:</strong></para>
/// Provides comprehensive querying capabilities for legal compliance, reporting,
/// and audit trail analysis requirements in the document management system.
/// </remarks>
public interface IRevisionActivityRepository
{
    #region RevisionActivity Methods

    /// <summary>
    /// Gets the usage count for a specific revision activity.
    /// </summary>
    /// <param name="activityId">The revision activity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of times this activity has been used.</returns>
    /// <remarks>
    /// This method provides an optimized count query without loading entities into memory.
    /// </remarks>
    Task<int> GetActivityUsageCountAsync(Guid activityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the unique user count for a specific revision activity.
    /// </summary>
    /// <param name="activityId">The revision activity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of unique users who have performed this activity.</returns>
    /// <remarks>
    /// This method uses distinct projections to efficiently count unique users.
    /// </remarks>
    Task<int> GetActivityUniqueUserCountAsync(Guid activityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a revision activity has any user associations.
    /// </summary>
    /// <param name="activityId">The revision activity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>true if the activity has user associations; otherwise, false.</returns>
    /// <remarks>
    /// This method uses an optimized Any() query for existence checking.
    /// </remarks>
    Task<bool> ActivityHasUserAssociationsAsync(Guid activityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets usage statistics for all revision activities.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of activity usage statistics.</returns>
    /// <remarks>
    /// This method returns aggregated statistics for all activities in a single query,
    /// ideal for dashboard and reporting scenarios.
    /// </remarks>
    Task<IEnumerable<ActivityUsageStatistics>> GetAllActivityUsageStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most frequently used revision activities.
    /// </summary>
    /// <param name="topCount">The number of top activities to return. Defaults to 10.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of the most used activities with their usage counts.</returns>
    Task<IEnumerable<ActivityUsageStatistics>> GetMostUsedActivitiesAsync(int topCount = 10, CancellationToken cancellationToken = default);

    #endregion RevisionActivity Methods

    #region RevisionActivityUser Query Methods

    /// <summary>
    /// Gets revision activity users for a specific revision with optimal loading.
    /// </summary>
    /// <param name="revisionId">The revision identifier.</param>
    /// <param name="includeNavigationProperties">Whether to include navigation properties.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of revision activity users for the specified revision.</returns>
    Task<IEnumerable<RevisionActivityUser>> GetActivitiesByRevisionAsync(Guid revisionId,
        bool includeNavigationProperties = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets revision activity users for a specific user with optimal loading.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="includeNavigationProperties">Whether to include navigation properties.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of revision activity users for the specified user.</returns>
    Task<IEnumerable<RevisionActivityUser>> GetActivitiesByUserAsync(Guid userId,
        bool includeNavigationProperties = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets revision activity users for a specific activity type.
    /// </summary>
    /// <param name="activityId">The revision activity identifier.</param>
    /// <param name="includeNavigationProperties">Whether to include navigation properties.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of revision activity users for the specified activity.</returns>
    Task<IEnumerable<RevisionActivityUser>> GetActivitiesByTypeAsync(Guid activityId,
        bool includeNavigationProperties = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent revision activity users within the specified timeframe.
    /// </summary>
    /// <param name="withinHours">The number of hours to look back. Defaults to 24.</param>
    /// <param name="includeNavigationProperties">Whether to include navigation properties.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of recent revision activity users.</returns>
    Task<IEnumerable<RevisionActivityUser>> GetRecentActivitiesAsync(double withinHours = 24.0,
        bool includeNavigationProperties = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets revision activity users within a specific date range.
    /// </summary>
    /// <param name="startDate">The start date (inclusive).</param>
    /// <param name="endDate">The end date (inclusive).</param>
    /// <param name="includeNavigationProperties">Whether to include navigation properties.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of revision activity users within the date range.</returns>
    Task<IEnumerable<RevisionActivityUser>> GetActivitiesByDateRangeAsync(DateTime startDate, DateTime endDate,
        bool includeNavigationProperties = true, CancellationToken cancellationToken = default);

    #endregion RevisionActivityUser Query Methods

    #region Advanced Analytics Methods

    /// <summary>
    /// Gets user activity statistics for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>User activity statistics including counts by activity type.</returns>
    Task<UserActivityStatistics> GetUserActivityStatisticsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets revision activity statistics for a specific revision.
    /// </summary>
    /// <param name="revisionId">The revision identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Revision activity statistics including timeline and user involvement.</returns>
    Task<RevisionActivityStatistics> GetRevisionActivityStatisticsAsync(Guid revisionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets activity patterns grouped by time periods for analytics.
    /// </summary>
    /// <param name="startDate">The start date for analysis.</param>
    /// <param name="endDate">The end date for analysis.</param>
    /// <param name="groupingPeriod">The period to group by (Hour, Day, Week, Month).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Activity patterns grouped by the specified time period.</returns>
    Task<IEnumerable<ActivityPatternStatistics>> GetActivityPatternsAsync(DateTime startDate, DateTime endDate,
        TimeGroupingPeriod groupingPeriod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most active users by revision activity count.
    /// </summary>
    /// <param name="topCount">The number of top users to return. Defaults to 10.</param>
    /// <param name="dateRange">Optional date range to filter activities.</param>
    /// <param name="activityTypeFilter">Optional activity type filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of the most active users with their activity counts.</returns>
    Task<IEnumerable<UserActivityStatistics>> GetMostActiveUsersAsync(int topCount = 10,
        DateRange? dateRange = null, string? activityTypeFilter = null, CancellationToken cancellationToken = default);

    #endregion Advanced Analytics Methods

    #region Sequence Management Methods

    /// <summary>
    /// Gets the next sequence number for activities with the same timestamp.
    /// </summary>
    /// <param name="revisionId">The revision identifier.</param>
    /// <param name="activityId">The revision activity identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="timestamp">The timestamp to check for conflicts.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The next available sequence number.</returns>
    /// <remarks>
    /// This method helps prevent unique constraint violations when multiple activities
    /// occur at the same timestamp by providing the next available sequence number.
    /// </remarks>
    Task<int> GetNextSequenceNumberAsync(Guid revisionId, Guid activityId, Guid userId,
        DateTime timestamp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a specific activity user combination already exists.
    /// </summary>
    /// <param name="revisionId">The revision identifier.</param>
    /// <param name="activityId">The revision activity identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="timestamp">The timestamp to check.</param>
    /// <param name="sequenceNumber">The sequence number to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>true if the combination exists; otherwise, false.</returns>
    Task<bool> ActivityUserExistsAsync(Guid revisionId, Guid activityId, Guid userId,
        DateTime timestamp, int sequenceNumber, CancellationToken cancellationToken = default);

    #endregion Sequence Management Methods

    #region Audit Trail Methods

    /// <summary>
    /// Gets a complete audit trail for a specific revision in chronological order.
    /// </summary>
    /// <param name="revisionId">The revision identifier.</param>
    /// <param name="includeNavigationProperties">Whether to include navigation properties for detailed info.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A chronologically ordered audit trail for the revision.</returns>
    Task<IEnumerable<RevisionActivityUser>> GetRevisionAuditTrailAsync(Guid revisionId,
        bool includeNavigationProperties = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit trail entries for compliance reporting.
    /// </summary>
    /// <param name="startDate">The start date for the report.</param>
    /// <param name="endDate">The end date for the report.</param>
    /// <param name="userFilter">Optional user filter.</param>
    /// <param name="activityTypeFilter">Optional activity type filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of audit trail entries formatted for compliance reporting.</returns>
    Task<IEnumerable<AuditTrailEntry>> GetComplianceAuditTrailAsync(DateTime startDate, DateTime endDate,
        Guid? userFilter = null, string? activityTypeFilter = null, CancellationToken cancellationToken = default);

    #endregion Audit Trail Methods
}

#region Supporting Data Transfer Objects

/// <summary>
/// Represents usage statistics for a revision activity.
/// </summary>
/// <remarks>
/// This DTO provides aggregated statistics about activity usage without requiring
/// entity loading, optimizing performance for reporting and analytics scenarios.
/// </remarks>
public record ActivityUsageStatistics
{
    /// <summary>Gets the activity identifier.</summary>
    public required Guid ActivityId { get; init; }

    /// <summary>Gets the activity name.</summary>
    public required string ActivityName { get; init; }

    /// <summary>Gets the total usage count.</summary>
    public required int UsageCount { get; init; }

    /// <summary>Gets the unique user count.</summary>
    public required int UniqueUserCount { get; init; }

    /// <summary>Gets the first usage timestamp.</summary>
    public DateTime? FirstUsed { get; init; }

    /// <summary>Gets the last usage timestamp.</summary>
    public DateTime? LastUsed { get; init; }

    /// <summary>Gets the average daily usage.</summary>
    public double AverageDailyUsage { get; init; }
}

/// <summary>
/// Represents activity statistics for a specific user.
/// </summary>
/// <remarks>
/// This DTO provides user-centric activity analytics for performance monitoring
/// and user behavior analysis.
/// </remarks>
public record UserActivityStatistics
{
    /// <summary>Gets the user identifier.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Gets the user name.</summary>
    public required string UserName { get; init; }

    /// <summary>Gets the total activity count for this user.</summary>
    public required int TotalActivityCount { get; init; }

    /// <summary>Gets the count of unique revisions this user has worked on.</summary>
    public required int UniqueRevisionsCount { get; init; }

    /// <summary>Gets activity counts by type.</summary>
    public required Dictionary<string, int> ActivityCounts { get; init; }

    /// <summary>Gets the first activity timestamp for this user.</summary>
    public DateTime? FirstActivity { get; init; }

    /// <summary>Gets the last activity timestamp for this user.</summary>
    public DateTime? LastActivity { get; init; }

    /// <summary>Gets the average activities per day for this user.</summary>
    public double AverageActivitiesPerDay { get; init; }
}

/// <summary>
/// Represents activity statistics for a specific revision.
/// </summary>
/// <remarks>
/// This DTO provides revision-centric activity analytics for document lifecycle
/// tracking and audit trail analysis.
/// </remarks>
public record RevisionActivityStatistics
{
    /// <summary>Gets the revision identifier.</summary>
    public required Guid RevisionId { get; init; }

    /// <summary>Gets the revision number.</summary>
    public required int RevisionNumber { get; init; }

    /// <summary>Gets the total activity count for this revision.</summary>
    public required int TotalActivityCount { get; init; }

    /// <summary>Gets the count of unique users who worked on this revision.</summary>
    public required int UniqueUsersCount { get; init; }

    /// <summary>Gets activity counts by type.</summary>
    public required Dictionary<string, int> ActivityCounts { get; init; }

    /// <summary>Gets the creation timestamp of this revision.</summary>
    public DateTime? CreatedAt { get; init; }

    /// <summary>Gets the last activity timestamp for this revision.</summary>
    public DateTime? LastActivity { get; init; }

    /// <summary>Gets the revision lifecycle duration in days.</summary>
    public double? LifecycleDays { get; init; }
}

/// <summary>
/// Represents activity pattern statistics grouped by time periods.
/// </summary>
/// <remarks>
/// This DTO provides temporal analytics for understanding activity patterns
/// and system usage trends over time.
/// </remarks>
public record ActivityPatternStatistics
{
    /// <summary>Gets the time period start.</summary>
    public required DateTime PeriodStart { get; init; }

    /// <summary>Gets the time period end.</summary>
    public required DateTime PeriodEnd { get; init; }

    /// <summary>Gets the grouping period type.</summary>
    public required TimeGroupingPeriod GroupingPeriod { get; init; }

    /// <summary>Gets the total activity count for this period.</summary>
    public required int ActivityCount { get; init; }

    /// <summary>Gets the unique user count for this period.</summary>
    public required int UniqueUsersCount { get; init; }

    /// <summary>Gets the unique revision count for this period.</summary>
    public required int UniqueRevisionsCount { get; init; }

    /// <summary>Gets activity counts by type for this period.</summary>
    public required Dictionary<string, int> ActivityCountsByType { get; init; }

    /// <summary>Gets the peak activity hour within this period (0-23).</summary>
    public int? PeakActivityHour { get; init; }
}

/// <summary>
/// Represents an audit trail entry optimized for compliance reporting.
/// </summary>
/// <remarks>
/// This DTO provides flattened audit trail data optimized for export,
/// reporting, and legal compliance scenarios.
/// </remarks>
public record AuditTrailEntry
{
    /// <summary>Gets the unique identifier for this audit entry.</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets the revision identifier.</summary>
    public required Guid RevisionId { get; init; }

    /// <summary>Gets the revision number.</summary>
    public required int RevisionNumber { get; init; }

    /// <summary>Gets the document file name.</summary>
    public required string DocumentFileName { get; init; }

    /// <summary>Gets the activity type performed.</summary>
    public required string ActivityType { get; init; }

    /// <summary>Gets the user identifier.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Gets the user name.</summary>
    public required string UserName { get; init; }

    /// <summary>Gets the user email.</summary>
    public string? UserEmail { get; init; }

    /// <summary>Gets the timestamp when the activity occurred.</summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>Gets the sequence number for activities with the same timestamp.</summary>
    public required int SequenceNumber { get; init; }

    /// <summary>Gets additional context or metadata about the activity.</summary>
    public string? ActivityContext { get; init; }
}

/// <summary>
/// Represents a date range for filtering operations.
/// </summary>
/// <remarks>
/// This record provides a convenient way to specify date ranges for queries
/// with built-in validation and utility methods.
/// </remarks>
public record DateRange
{
    /// <summary>Gets the start date (inclusive).</summary>
    public required DateTime StartDate { get; init; }

    /// <summary>Gets the end date (inclusive).</summary>
    public required DateTime EndDate { get; init; }

    /// <summary>
    /// Gets the duration of this date range.
    /// </summary>
    public TimeSpan Duration => EndDate - StartDate;

    /// <summary>
    /// Gets the number of days in this date range.
    /// </summary>
    public double TotalDays => Duration.TotalDays;

    /// <summary>
    /// Validates that the date range is logical (end date is after start date).
    /// </summary>
    /// <returns>A validation result indicating if the range is valid.</returns>
    public ValidationResult? ValidateDateRange()
    {
        if (EndDate < StartDate)
        {
            return new ValidationResult(
                "End date must be greater than or equal to start date.",
                [nameof(StartDate), nameof(EndDate)]);
        }

        return ValidationResult.Success;
    }

    /// <summary>
    /// Creates a date range for the last N days.
    /// </summary>
    /// <param name="days">The number of days to go back.</param>
    /// <returns>A date range covering the last N days.</returns>
    public static DateRange LastDays(int days)
    {
        var endDate = DateTime.UtcNow.Date.AddDays(1).AddTicks(-1); // End of today
        var startDate = endDate.AddDays(-days).Date; // Start of N days ago

        return new DateRange
        {
            StartDate = startDate,
            EndDate = endDate
        };
    }

    /// <summary>
    /// Creates a date range for the current month.
    /// </summary>
    /// <returns>A date range covering the current month.</returns>
    public static DateRange CurrentMonth()
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        return new DateRange
        {
            StartDate = startDate,
            EndDate = endDate
        };
    }
}

/// <summary>
/// Defines time grouping periods for activity pattern analysis.
/// </summary>
public enum TimeGroupingPeriod
{
    /// <summary>Group by hour.</summary>
    Hour,

    /// <summary>Group by day.</summary>
    Day,

    /// <summary>Group by week.</summary>
    Week,

    /// <summary>Group by month.</summary>
    Month,

    /// <summary>Group by quarter.</summary>
    Quarter,

    /// <summary>Group by year.</summary>
    Year
}

#endregion Supporting Data Transfer Objects