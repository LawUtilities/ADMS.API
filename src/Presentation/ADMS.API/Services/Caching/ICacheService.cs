using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using System.Collections.Concurrent;
using System.Text.Json;

namespace ADMS.API.Services.Caching;

/// <summary>
/// Interface for advanced caching operations with multi-level support.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached value or null if not found.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in the cache with the specified expiration.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">The cache expiration options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetAsync<T>(string key, T value, CacheExpiration? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets a cached value or creates it using the provided factory function.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to create the value if not cached.</param>
    /// <param name="expiration">The cache expiration options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheExpiration? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes multiple values from the cache by pattern.
    /// </summary>
    /// <param name="pattern">The key pattern to match.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    /// <param name="key">The cache key to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the key exists, false otherwise.</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the expiration of a cached item.
    /// </summary>
    /// <param name="key">The cache key to refresh.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RefreshAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cache statistics and performance metrics.
    /// </summary>
    /// <returns>Cache statistics information.</returns>
    CacheStatistics GetStatistics();
}

/// <summary>
/// Multi-level caching service that supports both memory and distributed caching.
/// </summary>
public class MultiLevelCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache? _distributedCache;
    private readonly IOptionsMonitor<CacheSettings> _cacheOptions;
    private readonly ILogger<MultiLevelCacheService> _logger;
    private readonly ConcurrentDictionary<string, DateTime> _keyTracker;
    private readonly Timer _cleanupTimer;
    private readonly CacheStatistics _statistics;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiLevelCacheService"/> class.
    /// </summary>
    /// <param name="memoryCache">The memory cache instance.</param>
    /// <param name="distributedCache">The distributed cache instance (optional).</param>
    /// <param name="cacheOptions">The cache configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public MultiLevelCacheService(
        IMemoryCache memoryCache,
        IDistributedCache? distributedCache,
        IOptionsMonitor<CacheSettings> cacheOptions,
        ILogger<MultiLevelCacheService> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _distributedCache = distributedCache;
        _cacheOptions = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _keyTracker = new ConcurrentDictionary<string, DateTime>();
        _statistics = new CacheStatistics();

        // Start cleanup timer for key tracking
        _cleanupTimer = new Timer(CleanupExpiredKeys, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

        _logger.LogDebug("Multi-level cache service initialized with {DistributedCacheType}",
            _distributedCache?.GetType().Name ?? "memory-only");
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var normalizedKey = NormalizeKey(key);
        _statistics.RecordRequest();

        try
        {
            // Try memory cache first (L1)
            if (_memoryCache.TryGetValue(normalizedKey, out T? memoryValue))
            {
                _statistics.RecordHit(CacheLevel.Memory);
                _logger.LogDebug("Cache hit (memory): {Key}", normalizedKey);
                return memoryValue;
            }

            // Try distributed cache (L2)
            if (_distributedCache != null)
            {
                var distributedValue = await GetFromDistributedCacheAsync<T>(normalizedKey, cancellationToken);
                if (distributedValue != null)
                {
                    // Store in memory cache for faster subsequent access
                    var memoryCacheOptions = CreateMemoryCacheOptions();
                    _memoryCache.Set(normalizedKey, distributedValue, memoryCacheOptions);

                    _statistics.RecordHit(CacheLevel.Distributed);
                    _logger.LogDebug("Cache hit (distributed): {Key}", normalizedKey);
                    return distributedValue;
                }
            }

            _statistics.RecordMiss();
            _logger.LogDebug("Cache miss: {Key}", normalizedKey);
            return null;
        }
        catch (Exception ex)
        {
            _statistics.RecordError();
            _logger.LogError(ex, "Error retrieving from cache: {Key}", normalizedKey);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, CacheExpiration? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        var normalizedKey = NormalizeKey(key);
        expiration ??= CreateDefaultExpiration();

        try
        {
            // Set in memory cache (L1)
            var memoryCacheOptions = CreateMemoryCacheOptions(expiration);
            _memoryCache.Set(normalizedKey, value, memoryCacheOptions);

            // Set in distributed cache (L2)
            if (_distributedCache != null)
            {
                await SetInDistributedCacheAsync(normalizedKey, value, expiration, cancellationToken);
            }

            // Track the key for cleanup
            _keyTracker[normalizedKey] = DateTime.UtcNow.Add(expiration.AbsoluteExpirationRelativeToNow ?? TimeSpan.FromHours(1));

            _statistics.RecordSet();
            _logger.LogDebug("Cache set: {Key} with expiration {Expiration}", normalizedKey, expiration.AbsoluteExpirationRelativeToNow);
        }
        catch (Exception ex)
        {
            _statistics.RecordError();
            _logger.LogError(ex, "Error setting cache: {Key}", normalizedKey);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheExpiration? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        // Use a semaphore to prevent multiple threads from executing the factory for the same key
        var semaphoreKey = $"semaphore:{key}";
        using var semaphore = await GetOrCreateSemaphoreAsync(semaphoreKey);

        await semaphore.WaitAsync(cancellationToken);
        try
        {
            // Double-check pattern: another thread might have set the value while we were waiting
            cachedValue = await GetAsync<T>(key, cancellationToken);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            // Create the value using the factory
            _logger.LogDebug("Executing factory for cache key: {Key}", key);
            var value = await factory();

            // Cache the newly created value
            await SetAsync(key, value, expiration, cancellationToken);

            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var normalizedKey = NormalizeKey(key);

        try
        {
            // Remove from memory cache
            _memoryCache.Remove(normalizedKey);

            // Remove from distributed cache
            if (_distributedCache != null)
            {
                await _distributedCache.RemoveAsync(normalizedKey, cancellationToken);
            }

            // Remove from key tracker
            _keyTracker.TryRemove(normalizedKey, out _);

            _statistics.RecordRemove();
            _logger.LogDebug("Cache removed: {Key}", normalizedKey);
        }
        catch (Exception ex)
        {
            _statistics.RecordError();
            _logger.LogError(ex, "Error removing from cache: {Key}", normalizedKey);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        try
        {
            var keysToRemove = _keyTracker.Keys
                .Where(key => IsPatternMatch(key, pattern))
                .ToList();

            var removeTasks = keysToRemove.Select(key => RemoveAsync(key, cancellationToken));
            await Task.WhenAll(removeTasks);

            _logger.LogDebug("Cache pattern removal completed: {Pattern}, Removed: {Count}", pattern, keysToRemove.Count);
        }
        catch (Exception ex)
        {
            _statistics.RecordError();
            _logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var normalizedKey = NormalizeKey(key);

        try
        {
            // Check memory cache first
            if (_memoryCache.TryGetValue(normalizedKey, out _))
            {
                return true;
            }

            // Check distributed cache
            if (_distributedCache != null)
            {
                var distributedValue = await _distributedCache.GetAsync(normalizedKey, cancellationToken);
                return distributedValue != null;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence: {Key}", normalizedKey);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var normalizedKey = NormalizeKey(key);

        try
        {
            if (_distributedCache != null)
            {
                await _distributedCache.RefreshAsync(normalizedKey, cancellationToken);
                _logger.LogDebug("Cache refreshed: {Key}", normalizedKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache: {Key}", normalizedKey);
            throw;
        }
    }

    /// <inheritdoc />
    public CacheStatistics GetStatistics()
    {
        return _statistics.CreateSnapshot();
    }

    #region Private Helper Methods

    /// <summary>
    /// Normalizes a cache key to ensure consistency.
    /// </summary>
    /// <param name="key">The original key.</param>
    /// <returns>The normalized key.</returns>
    private string NormalizeKey(string key)
    {
        var settings = _cacheOptions.CurrentValue;
        var prefix = !string.IsNullOrWhiteSpace(settings.KeyPrefix) ? $"{settings.KeyPrefix}:" : "";
        return $"{prefix}{key}".ToLowerInvariant();
    }

    /// <summary>
    /// Creates default cache expiration settings.
    /// </summary>
    /// <returns>Default cache expiration settings.</returns>
    private CacheExpiration CreateDefaultExpiration()
    {
        var settings = _cacheOptions.CurrentValue;
        return new CacheExpiration
        {
            AbsoluteExpirationRelativeToNow = settings.DefaultAbsoluteExpiration,
            SlidingExpiration = settings.DefaultSlidingExpiration
        };
    }

    /// <summary>
    /// Creates memory cache entry options from cache expiration settings.
    /// </summary>
    /// <param name="expiration">The cache expiration settings.</param>
    /// <returns>Memory cache entry options.</returns>
    private MemoryCacheEntryOptions CreateMemoryCacheOptions(CacheExpiration? expiration = null)
    {
        expiration ??= CreateDefaultExpiration();

        var options = new MemoryCacheEntryOptions();

        if (expiration.AbsoluteExpirationRelativeToNow.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.AbsoluteExpirationRelativeToNow;
        }

        if (expiration.SlidingExpiration.HasValue)
        {
            options.SlidingExpiration = expiration.SlidingExpiration;
        }

        options.Priority = expiration.Priority;

        return options;
    }

    /// <summary>
    /// Creates distributed cache entry options from cache expiration settings.
    /// </summary>
    /// <param name="expiration">The cache expiration settings.</param>
    /// <returns>Distributed cache entry options.</returns>
    private static DistributedCacheEntryOptions CreateDistributedCacheOptions(CacheExpiration expiration)
    {
        var options = new DistributedCacheEntryOptions();

        if (expiration.AbsoluteExpirationRelativeToNow.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.AbsoluteExpirationRelativeToNow;
        }

        if (expiration.SlidingExpiration.HasValue)
        {
            options.SlidingExpiration = expiration.SlidingExpiration;
        }

        return options;
    }

    /// <summary>
    /// Gets a value from the distributed cache.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached value or null if not found.</returns>
    private async Task<T?> GetFromDistributedCacheAsync<T>(string key, CancellationToken cancellationToken) where T : class
    {
        var serializedValue = await _distributedCache!.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrWhiteSpace(serializedValue))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(serializedValue, GetJsonSerializerOptions());
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize cached value for key: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// Sets a value in the distributed cache.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">The cache expiration settings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SetInDistributedCacheAsync<T>(string key, T value, CacheExpiration expiration, CancellationToken cancellationToken)
    {
        var serializedValue = JsonSerializer.Serialize(value, GetJsonSerializerOptions());
        var options = CreateDistributedCacheOptions(expiration);

        await _distributedCache!.SetStringAsync(key, serializedValue, options, cancellationToken);
    }

    /// <summary>
    /// Gets or creates a semaphore for the specified key.
    /// </summary>
    /// <param name="key">The semaphore key.</param>
    /// <returns>A semaphore for the specified key.</returns>
    private async Task<SemaphoreSlim> GetOrCreateSemaphoreAsync(string key)
    {
        // For simplicity, we'll use a static concurrent dictionary
        // In a production environment, consider using a more sophisticated approach
        return await Task.FromResult(SemaphoreCollection.GetOrAdd(key, _ => new SemaphoreSlim(1, 1)));
    }

    /// <summary>
    /// Gets JSON serializer options for distributed cache serialization.
    /// </summary>
    /// <returns>JSON serializer options.</returns>
    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Checks if a key matches a pattern (supports * wildcard).
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <param name="pattern">The pattern to match against.</param>
    /// <returns>True if the key matches the pattern, false otherwise.</returns>
    private static bool IsPatternMatch(string key, string pattern)
    {
        // Simple pattern matching with * wildcard
        if (!pattern.Contains('*'))
        {
            return string.Equals(key, pattern, StringComparison.OrdinalIgnoreCase);
        }

        var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace(@"\*", ".*") + "$";
        return System.Text.RegularExpressions.Regex.IsMatch(key, regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Cleans up expired keys from the key tracker.
    /// </summary>
    /// <param name="state">Timer state (not used).</param>
    private void CleanupExpiredKeys(object? state)
    {
        try
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _keyTracker
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _keyTracker.TryRemove(key, out _);
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogDebug("Cleaned up {Count} expired cache keys", expiredKeys.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during cache key cleanup");
        }
    }

    #endregion

    /// <summary>
    /// Static collection to manage semaphores for cache key locking.
    /// </summary>
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> SemaphoreCollection = new();
}

/// <summary>
/// Represents cache expiration settings.
/// </summary>
public class CacheExpiration
{
    /// <summary>
    /// Gets or sets the absolute expiration relative to now.
    /// </summary>
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    /// <summary>
    /// Gets or sets the sliding expiration.
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// Gets or sets the cache priority for memory cache.
    /// </summary>
    public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;
}

/// <summary>
/// Represents cache level enumeration.
/// </summary>
public enum CacheLevel
{
    /// <summary>Memory cache level.</summary>
    Memory,
    /// <summary>Distributed cache level.</summary>
    Distributed
}

/// <summary>
/// Tracks cache statistics and performance metrics.
/// </summary>
public class CacheStatistics
{
    private long _totalRequests;
    private long _memoryHits;
    private long _distributedHits;
    private long _misses;
    private long _sets;
    private long _removes;
    private long _errors;

    /// <summary>
    /// Gets the total number of cache requests.
    /// </summary>
    public long TotalRequests => _totalRequests;

    /// <summary>
    /// Gets the number of memory cache hits.
    /// </summary>
    public long MemoryHits => _memoryHits;

    /// <summary>
    /// Gets the number of distributed cache hits.
    /// </summary>
    public long DistributedHits => _distributedHits;

    /// <summary>
    /// Gets the total number of cache hits.
    /// </summary>
    public long TotalHits => _memoryHits + _distributedHits;

    /// <summary>
    /// Gets the number of cache misses.
    /// </summary>
    public long Misses => _misses;

    /// <summary>
    /// Gets the number of cache sets.
    /// </summary>
    public long Sets => _sets;

    /// <summary>
    /// Gets the number of cache removes.
    /// </summary>
    public long Removes => _removes;

    /// <summary>
    /// Gets the number of cache errors.
    /// </summary>
    public long Errors => _errors;

    /// <summary>
    /// Gets the cache hit ratio as a percentage.
    /// </summary>
    public double HitRatio => _totalRequests > 0 ? (double)TotalHits / _totalRequests * 100 : 0;

    /// <summary>
    /// Gets the memory cache hit ratio as a percentage.
    /// </summary>
    public double MemoryHitRatio => _totalRequests > 0 ? (double)_memoryHits / _totalRequests * 100 : 0;

    /// <summary>
    /// Records a cache request.
    /// </summary>
    internal void RecordRequest() => Interlocked.Increment(ref _totalRequests);

    /// <summary>
    /// Records a cache hit.
    /// </summary>
    /// <param name="level">The cache level that had the hit.</param>
    internal void RecordHit(CacheLevel level)
    {
        switch (level)
        {
            case CacheLevel.Memory:
                Interlocked.Increment(ref _memoryHits);
                break;
            case CacheLevel.Distributed:
                Interlocked.Increment(ref _distributedHits);
                break;
        }
    }

    /// <summary>
    /// Records a cache miss.
    /// </summary>
    internal void RecordMiss() => Interlocked.Increment(ref _misses);

    /// <summary>
    /// Records a cache set operation.
    /// </summary>
    internal void RecordSet() => Interlocked.Increment(ref _sets);

    /// <summary>
    /// Records a cache remove operation.
    /// </summary>
    internal void RecordRemove() => Interlocked.Increment(ref _removes);

    /// <summary>
    /// Records a cache error.
    /// </summary>
    internal void RecordError() => Interlocked.Increment(ref _errors);

    /// <summary>
    /// Creates a snapshot of current statistics.
    /// </summary>
    /// <returns>A snapshot of current statistics.</returns>
    internal CacheStatistics CreateSnapshot()
    {
        return new CacheStatistics
        {
            _totalRequests = _totalRequests,
            _memoryHits = _memoryHits,
            _distributedHits = _distributedHits,
            _misses = _misses,
            _sets = _sets,
            _removes = _removes,
            _errors = _errors
        };
    }
}

/// <summary>
/// Configuration settings for the cache service.
/// </summary>
public class CacheSettings
{
    /// <summary>
    /// Gets or sets the cache key prefix.
    /// </summary>
    public string KeyPrefix { get; set; } = "adms";

    /// <summary>
    /// Gets or sets the default absolute expiration.
    /// </summary>
    public TimeSpan DefaultAbsoluteExpiration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets the default sliding expiration.
    /// </summary>
    public TimeSpan DefaultSlidingExpiration { get; set; } = TimeSpan.FromMinutes(20);

    /// <summary>
    /// Gets or sets a value indicating whether distributed caching is enabled.
    /// </summary>
    public bool EnableDistributedCache { get; set; } = false;

    /// <summary>
    /// Gets or sets the Redis connection string for distributed caching.
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether cache compression is enabled.
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum memory cache size in MB.
    /// </summary>
    public int MaxMemoryCacheSizeMB { get; set; } = 100;
}

/// <summary>
/// Extension methods for cache service registration.
/// </summary>
public static class CacheServiceExtensions
{
    /// <summary>
    /// Adds cache services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCacheServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure cache settings
        services.Configure<CacheSettings>(configuration.GetSection("Cache"));

        // Add memory cache
        services.AddMemoryCache(options =>
        {
            var cacheSettings = configuration.GetSection("Cache").Get<CacheSettings>() ?? new CacheSettings();
            options.SizeLimit = cacheSettings.MaxMemoryCacheSizeMB * 1024 * 1024; // Convert MB to bytes
        });

        // Add distributed cache if enabled
        var enableDistributedCache = configuration.GetValue<bool>("Cache:EnableDistributedCache", false);
        if (enableDistributedCache)
        {
            var redisConnectionString = configuration.GetValue<string>("Cache:RedisConnectionString");
            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "ADMS";
                });
            }
            else
            {
                // Fallback to SQL Server distributed cache
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    services.AddSqlServerCache(options =>
                    {
                        options.ConnectionString = connectionString;
                        options.SchemaName = "dbo";
                        options.TableName = "CacheEntries";
                    });
                }
            }
        }

        // Register cache service
        services.AddScoped<ICacheService, MultiLevelCacheService>();

        return services;
    }

    /// <summary>
    /// Creates a cache key for the specified entity and identifier.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="id">The entity identifier.</param>
    /// <param name="suffix">Optional suffix for the key.</param>
    /// <returns>A standardized cache key.</returns>
    public static string CreateCacheKey(this string entityType, object id, string? suffix = null)
    {
        var key = $"{entityType}:{id}";
        return !string.IsNullOrWhiteSpace(suffix) ? $"{key}:{suffix}" : key;
    }

    /// <summary>
    /// Creates a cache key for listing operations.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="parameters">The query parameters.</param>
    /// <returns>A standardized cache key for list operations.</returns>
    public static string CreateListCacheKey(this string entityType, object parameters)
    {
        var parameterString = JsonSerializer.Serialize(parameters, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(parameterString));
        var hashString = Convert.ToHexString(hash)[..16]; // Take first 16 characters for brevity

        return $"{entityType}:list:{hashString}";
    }
}