using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace ADMS.API.Performance;

/// <summary>
/// Performance monitoring service for tracking API metrics and performance.
/// </summary>
public interface IPerformanceMonitoringService
{
    /// <summary>
    /// Starts measuring performance for an operation.
    /// </summary>
    /// <param name="operationName">The name of the operation being measured.</param>
    /// <param name="tags">Optional tags for categorizing the measurement.</param>
    /// <returns>A disposable performance measurement context.</returns>
    IDisposable StartMeasurement(string operationName, Dictionary<string, object?>? tags = null);

    /// <summary>
    /// Records a custom metric value.
    /// </summary>
    /// <param name="name">The metric name.</param>
    /// <param name="value">The metric value.</param>
    /// <param name="tags">Optional tags for categorizing the metric.</param>
    void RecordMetric(string name, double value, Dictionary<string, object?>? tags = null);

    /// <summary>
    /// Increments a counter metric.
    /// </summary>
    /// <param name="name">The counter name.</param>
    /// <param name="increment">The increment value (default: 1).</param>
    /// <param name="tags">Optional tags for categorizing the counter.</param>
    void IncrementCounter(string name, long increment = 1, Dictionary<string, object?>? tags = null);

    /// <summary>
    /// Records the current memory usage.
    /// </summary>
    void RecordMemoryUsage();

    /// <summary>
    /// Gets performance summary for the specified time period.
    /// </summary>
    /// <param name="period">The time period to analyze.</param>
    /// <returns>Performance summary data.</returns>
    PerformanceSummary GetPerformanceSummary(TimeSpan period);
}

/// <summary>
/// Implementation of performance monitoring service using .NET meters and custom tracking.
/// </summary>
public class PerformanceMonitoringService : IPerformanceMonitoringService, IDisposable
{
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly Meter _meter;
    private readonly Counter<long> _requestCounter;
    private readonly Histogram<double> _requestDuration;
    private readonly Histogram<double> _customMetrics;
    private readonly ObservableGauge<long> _memoryUsage;
    private readonly ConcurrentDictionary<string, OperationMetrics> _operationMetrics;
    private readonly Timer _memoryMonitoringTimer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceMonitoringService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _operationMetrics = new ConcurrentDictionary<string, OperationMetrics>();

        // Initialize .NET Meter for OpenTelemetry integration
        _meter = new Meter("ADMS.API", "1.0.0");

        // Create instruments
        _requestCounter = _meter.CreateCounter<long>(
            "adms_requests_total",
            description: "Total number of API requests");

        _requestDuration = _meter.CreateHistogram<double>(
            "adms_request_duration_seconds",
            unit: "s",
            description: "Duration of API requests in seconds");

        _customMetrics = _meter.CreateHistogram<double>(
            "adms_custom_metrics",
            description: "Custom application metrics");

        _memoryUsage = _meter.CreateObservableGauge<long>(
            "adms_memory_usage_bytes",
            observeValue: () => GC.GetTotalMemory(false),
            description: "Current memory usage in bytes");

        // Start periodic memory monitoring
        _memoryMonitoringTimer = new Timer(RecordMemoryUsageCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

        _logger.LogDebug("Performance monitoring service initialized");
    }

    /// <inheritdoc />
    public IDisposable StartMeasurement(string operationName, Dictionary<string, object?>? tags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        return new PerformanceMeasurementContext(this, operationName, tags);
    }

    /// <inheritdoc />
    public void RecordMetric(string name, double value, Dictionary<string, object?>? tags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        try
        {
            var tagPairs = tags?.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)).ToArray();
            _customMetrics.Record(value, tagPairs);

            _logger.LogDebug("Recorded custom metric: {MetricName} = {Value}", name, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording custom metric: {MetricName}", name);
        }
    }

    /// <inheritdoc />
    public void IncrementCounter(string name, long increment = 1, Dictionary<string, object?>? tags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        try
        {
            var tagPairs = tags?.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)).ToArray();
            _requestCounter.Add(increment, tagPairs);

            _logger.LogDebug("Incremented counter: {CounterName} by {Increment}", name, increment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing counter: {CounterName}", name);
        }
    }

    /// <inheritdoc />
    public void RecordMemoryUsage()
    {
        try
        {
            var memoryBefore = GC.GetTotalMemory(false);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var memoryAfter = GC.GetTotalMemory(true);

            _logger.LogDebug("Memory usage: Before GC: {MemoryBefore:N0} bytes, After GC: {MemoryAfter:N0} bytes",
                memoryBefore, memoryAfter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording memory usage");
        }
    }

    /// <inheritdoc />
    public PerformanceSummary GetPerformanceSummary(TimeSpan period)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(period);
        var summary = new PerformanceSummary
        {
            Period = period,
            GeneratedAt = DateTime.UtcNow
        };

        try
        {
            foreach (var (operationName, metrics) in _operationMetrics)
            {
                var recentMeasurements = metrics.GetMeasurementsSince(cutoffTime);
                if (recentMeasurements.Any())
                {
                    summary.OperationSummaries[operationName] = new OperationSummary
                    {
                        OperationName = operationName,
                        TotalCalls = recentMeasurements.Count,
                        AverageResponseTime = TimeSpan.FromMilliseconds(recentMeasurements.Average(m => m.Duration.TotalMilliseconds)),
                        MinResponseTime = recentMeasurements.Min(m => m.Duration),
                        MaxResponseTime = recentMeasurements.Max(m => m.Duration),
                        MedianResponseTime = TimeSpan.FromMilliseconds(GetPercentile(recentMeasurements.Select(m => m.Duration.TotalMilliseconds), 50)),
                        P95ResponseTime = TimeSpan.FromMilliseconds(GetPercentile(recentMeasurements.Select(m => m.Duration.TotalMilliseconds), 95)),
                        P99ResponseTime = TimeSpan.FromMilliseconds(GetPercentile(recentMeasurements.Select(m => m.Duration.TotalMilliseconds), 99)),
                        ErrorCount = recentMeasurements.Count(m => m.HasError),
                        SuccessRate = (double)(recentMeasurements.Count - recentMeasurements.Count(m => m.HasError)) / recentMeasurements.Count * 100
                    };
                }
            }

            _logger.LogDebug("Generated performance summary for period: {Period}", period);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating performance summary");
        }

        return summary;
    }

    /// <summary>
    /// Records the completion of a performance measurement.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="duration">The operation duration.</param>
    /// <param name="tags">Optional tags for categorizing the measurement.</param>
    /// <param name="hasError">Whether the operation had an error.</param>
    internal void RecordMeasurement(string operationName, TimeSpan duration, Dictionary<string, object?>? tags, bool hasError)
    {
        try
        {
            // Record in .NET Meter
            var tagPairs = tags?.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)).ToList() ?? new List<KeyValuePair<string, object?>>();
            tagPairs.Add(new KeyValuePair<string, object?>("operation", operationName));
            tagPairs.Add(new KeyValuePair<string, object?>("status", hasError ? "error" : "success"));

            _requestDuration.Record(duration.TotalSeconds, tagPairs.ToArray());
            _requestCounter.Add(1, tagPairs.ToArray());

            // Record in local metrics
            var metrics = _operationMetrics.GetOrAdd(operationName, _ => new OperationMetrics());
            metrics.AddMeasurement(duration, hasError);

            _logger.LogDebug("Recorded measurement for {OperationName}: {Duration}ms, HasError: {HasError}",
                operationName, duration.TotalMilliseconds, hasError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording performance measurement for {OperationName}", operationName);
        }
    }

    /// <summary>
    /// Callback method for periodic memory usage recording.
    /// </summary>
    /// <param name="state">Timer state (not used).</param>
    private void RecordMemoryUsageCallback(object? state)
    {
        RecordMemoryUsage();
    }

    /// <summary>
    /// Calculates the specified percentile from a collection of values.
    /// </summary>
    /// <param name="values">The values to analyze.</param>
    /// <param name="percentile">The percentile to calculate (0-100).</param>
    /// <returns>The percentile value.</returns>
    private static double GetPercentile(IEnumerable<double> values, double percentile)
    {
        var sortedValues = values.OrderBy(x => x).ToArray();
        if (sortedValues.Length == 0) return 0;

        var index = (percentile / 100.0) * (sortedValues.Length - 1);
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);

        if (lower == upper) return sortedValues[lower];

        var weight = index - lower;
        return sortedValues[lower] * (1 - weight) + sortedValues[upper] * weight;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _memoryMonitoringTimer?.Dispose();
            _meter?.Dispose();
            _disposed = true;

            _logger.LogDebug("Performance monitoring service disposed");
        }
    }
}

/// <summary>
/// Represents a performance measurement context that automatically records timing when disposed.
/// </summary>
internal class PerformanceMeasurementContext : IDisposable
{
    private readonly PerformanceMonitoringService _service;
    private readonly string _operationName;
    private readonly Dictionary<string, object?>? _tags;
    private readonly Stopwatch _stopwatch;
    private bool _hasError;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceMeasurementContext"/> class.
    /// </summary>
    /// <param name="service">The performance monitoring service.</param>
    /// <param name="operationName">The operation name.</param>
    /// <param name="tags">Optional tags for categorizing the measurement.</param>
    public PerformanceMeasurementContext(PerformanceMonitoringService service, string operationName, Dictionary<string, object?>? tags)
    {
        _service = service;
        _operationName = operationName;
        _tags = tags;
        _stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Marks the measurement as having an error.
    /// </summary>
    public void MarkError()
    {
        _hasError = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _service.RecordMeasurement(_operationName, _stopwatch.Elapsed, _tags, _hasError);
            _disposed = true;
        }
    }
}

/// <summary>
/// Tracks metrics for a specific operation.
/// </summary>
internal class OperationMetrics
{
    private readonly List<PerformanceMeasurement> _measurements = new();
    private readonly object _lock = new();

    /// <summary>
    /// Adds a new measurement.
    /// </summary>
    /// <param name="duration">The operation duration.</param>
    /// <param name="hasError">Whether the operation had an error.</param>
    public void AddMeasurement(TimeSpan duration, bool hasError)
    {
        lock (_lock)
        {
            _measurements.Add(new PerformanceMeasurement
            {
                Timestamp = DateTime.UtcNow,
                Duration = duration,
                HasError = hasError
            });

            // Keep only recent measurements (last 24 hours)
            var cutoff = DateTime.UtcNow.AddHours(-24);
            _measurements.RemoveAll(m => m.Timestamp < cutoff);
        }
    }

    /// <summary>
    /// Gets measurements since the specified time.
    /// </summary>
    /// <param name="since">The cutoff time.</param>
    /// <returns>Measurements since the specified time.</returns>
    public List<PerformanceMeasurement> GetMeasurementsSince(DateTime since)
    {
        lock (_lock)
        {
            return _measurements.Where(m => m.Timestamp >= since).ToList();
        }
    }
}

/// <summary>
/// Represents a single performance measurement.
/// </summary>
internal class PerformanceMeasurement
{
    /// <summary>
    /// Gets or sets the timestamp when the measurement was taken.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the duration of the operation.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the operation had an error.
    /// </summary>
    public bool HasError { get; set; }
}

/// <summary>
/// Represents a performance summary for a specific time period.
/// </summary>
public class PerformanceSummary
{
    /// <summary>
    /// Gets or sets the time period this summary covers.
    /// </summary>
    public TimeSpan Period { get; set; }

    /// <summary>
    /// Gets or sets when this summary was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Gets the operation summaries by operation name.
    /// </summary>
    public Dictionary<string, OperationSummary> OperationSummaries { get; } = new();
}

/// <summary>
/// Represents a summary of performance metrics for a specific operation.
/// </summary>
public class OperationSummary
{
    /// <summary>
    /// Gets or sets the operation name.
    /// </summary>
    public string OperationName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total number of calls.
    /// </summary>
    public int TotalCalls { get; set; }

    /// <summary>
    /// Gets or sets the average response time.
    /// </summary>
    public TimeSpan AverageResponseTime { get; set; }

    /// <summary>
    /// Gets or sets the minimum response time.
    /// </summary>
    public TimeSpan MinResponseTime { get; set; }

    /// <summary>
    /// Gets or sets the maximum response time.
    /// </summary>
    public TimeSpan MaxResponseTime { get; set; }

    /// <summary>
    /// Gets or sets the median response time.
    /// </summary>
    public TimeSpan MedianResponseTime { get; set; }

    /// <summary>
    /// Gets or sets the 95th percentile response time.
    /// </summary>
    public TimeSpan P95ResponseTime { get; set; }

    /// <summary>
    /// Gets or sets the 99th percentile response time.
    /// </summary>
    public TimeSpan P99ResponseTime { get; set; }

    /// <summary>
    /// Gets or sets the number of errors.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Gets or sets the success rate as a percentage.
    /// </summary>
    public double SuccessRate { get; set; }
}

/// <summary>
/// Extension methods for performance monitoring.
/// </summary>
public static class PerformanceExtensions
{
    /// <summary>
    /// Measures the performance of an asynchronous operation.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="monitoringService">The performance monitoring service.</param>
    /// <param name="operationName">The operation name.</param>
    /// <param name="operation">The operation to measure.</param>
    /// <param name="tags">Optional tags for categorizing the measurement.</param>
    /// <returns>The result of the operation.</returns>
    public static async Task<T> MeasureAsync<T>(
        this IPerformanceMonitoringService monitoringService,
        string operationName,
        Func<Task<T>> operation,
        Dictionary<string, object?>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(monitoringService);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(operation);

        using var measurement = monitoringService.StartMeasurement(operationName, tags);

        try
        {
            return await operation();
        }
        catch
        {
            if (measurement is PerformanceMeasurementContext context)
            {
                context.MarkError();
            }
            throw;
        }
    }

    /// <summary>
    /// Measures the performance of a synchronous operation.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="monitoringService">The performance monitoring service.</param>
    /// <param name="operationName">The operation name.</param>
    /// <param name="operation">The operation to measure.</param>
    /// <param name="tags">Optional tags for categorizing the measurement.</param>
    /// <returns>The result of the operation.</returns>
    public static T Measure<T>(
        this IPerformanceMonitoringService monitoringService,
        string operationName,
        Func<T> operation,
        Dictionary<string, object?>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(monitoringService);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(operation);

        using var measurement = monitoringService.StartMeasurement(operationName, tags);

        try
        {
            return operation();
        }
        catch
        {
            if (measurement is PerformanceMeasurementContext context)
            {
                context.MarkError();
            }
            throw;
        }
    }

    /// <summary>
    /// Measures the performance of an asynchronous action.
    /// </summary>
    /// <param name="monitoringService">The performance monitoring service.</param>
    /// <param name="operationName">The operation name.</param>
    /// <param name="operation">The operation to measure.</param>
    /// <param name="tags">Optional tags for categorizing the measurement.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task MeasureAsync(
        this IPerformanceMonitoringService monitoringService,
        string operationName,
        Func<Task> operation,
        Dictionary<string, object?>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(monitoringService);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(operation);

        using var measurement = monitoringService.StartMeasurement(operationName, tags);

        try
        {
            await operation();
        }
        catch
        {
            if (measurement is PerformanceMeasurementContext context)
            {
                context.MarkError();
            }
            throw;
        }
    }
}

/// <summary>
/// Attribute for automatically measuring method performance.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class MeasurePerformanceAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the operation name. If not specified, the method name is used.
    /// </summary>
    public string? OperationName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include method parameters in tags.
    /// </summary>
    public bool IncludeParameters { get; set; } = false;

    /// <summary>
    /// Gets or sets custom tags to add to the measurement.
    /// </summary>
    public string[]? CustomTags { get; set; }
}