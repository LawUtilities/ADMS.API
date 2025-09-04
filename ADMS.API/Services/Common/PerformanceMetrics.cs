using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ADMS.API.Services.Common;

/// <summary>
/// Simple performance metrics collection for validation operations.
/// </summary>
public sealed class PerformanceMetrics
{
    private readonly ConcurrentDictionary<string, OperationMetrics> _metrics = new();
    private readonly object _lock = new();

    /// <summary>
    /// Records an operation with its duration and success status.
    /// </summary>
    public void RecordOperation(string operationType, TimeSpan duration, bool success, int? entityCount = null)
    {
        _metrics.AddOrUpdate(operationType,
            new OperationMetrics { Count = 1, TotalDuration = duration, Successes = success ? 1 : 0, TotalEntities = entityCount ?? 0 },
            (key, existing) => existing with
            {
                Count = existing.Count + 1,
                TotalDuration = existing.TotalDuration + duration,
                Successes = existing.Successes + (success ? 1 : 0),
                TotalEntities = existing.TotalEntities + (entityCount ?? 0)
            });
    }

    /// <summary>
    /// Gets metrics for a specific operation type.
    /// </summary>
    public OperationMetrics? GetOperationMetrics(string operationType)
    {
        return _metrics.TryGetValue(operationType, out var metrics) ? metrics : null;
    }

    /// <summary>
    /// Gets all collected metrics.
    /// </summary>
    public IReadOnlyDictionary<string, OperationMetrics> GetAllMetrics()
    {
        return _metrics.ToImmutableDictionary();
    }

    /// <summary>
    /// Clears all collected metrics.
    /// </summary>
    public void Clear()
    {
        _metrics.Clear();
    }

    /// <summary>
    /// Starts tracking an operation and returns a disposable tracker.
    /// </summary>
    public OperationTracker StartOperation(string operationType)
    {
        return new OperationTracker(this, operationType);
    }
}

/// <summary>
/// Represents metrics for a specific operation type.
/// </summary>
public sealed record OperationMetrics
{
    public required int Count { get; init; }
    public required TimeSpan TotalDuration { get; init; }
    public required int Successes { get; init; }
    public required int TotalEntities { get; init; }

    public int Failures => Count - Successes;
    public double SuccessRate => Count > 0 ? (double)Successes / Count : 0;
    public TimeSpan AverageDuration => Count > 0 ? TimeSpan.FromTicks(TotalDuration.Ticks / Count) : TimeSpan.Zero;
    public double EntitiesPerSecond => TotalDuration.TotalSeconds > 0 ? TotalEntities / TotalDuration.TotalSeconds : 0;
}

/// <summary>
/// Disposable operation tracker for automatic metrics recording.
/// </summary>
public sealed class OperationTracker : IDisposable
{
    private readonly PerformanceMetrics _metrics;
    private readonly string _operationType;
    private readonly Stopwatch _stopwatch;
    private int _entityCount;
    private bool _success;
    private bool _disposed;

    internal OperationTracker(PerformanceMetrics metrics, string operationType)
    {
        _metrics = metrics;
        _operationType = operationType;
        _stopwatch = Stopwatch.StartNew();
    }

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public void RecordSuccess(int entityCount = 0)
    {
        _success = true;
        _entityCount = entityCount;
    }

    public void RecordFailure()
    {
        _success = false;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _metrics.RecordOperation(_operationType, _stopwatch.Elapsed, _success, _entityCount);
            _disposed = true;
        }
    }
}