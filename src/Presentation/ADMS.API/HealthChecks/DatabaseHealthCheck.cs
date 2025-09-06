using ADMS.API.Extensions;
using ADMS.API.Services;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using System.Data.Common;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace ADMS.API.HealthChecks;

/// <summary>
/// Health check for database connectivity and performance.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IRepository _repository;
    private readonly IOptionsMonitor<DatabaseSettings> _databaseOptions;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseHealthCheck"/> class.
    /// </summary>
    /// <param name="repository">The repository for database access.</param>
    /// <param name="databaseOptions">The database configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public DatabaseHealthCheck(
        IRepository repository,
        IOptionsMonitor<DatabaseSettings> databaseOptions,
        ILogger<DatabaseHealthCheck> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _databaseOptions = databaseOptions ?? throw new ArgumentNullException(nameof(databaseOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs the health check asynchronously.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The health check result.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var data = new Dictionary<string, object>();

        try
        {
            // Test basic connectivity
            var canConnect = await TestDatabaseConnectivityAsync(cancellationToken);
            data["CanConnect"] = canConnect;

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Unable to connect to database", null, data);
            }

            // Test query performance
            var queryTime = await TestQueryPerformanceAsync(cancellationToken);
            data["QueryTimeMs"] = queryTime;

            stopwatch.Stop();
            data["TotalCheckTimeMs"] = stopwatch.ElapsedMilliseconds;

            // Determine health status based on performance
            var maxAcceptableQueryTime = _databaseOptions.CurrentValue.HealthCheckMaxQueryTimeMs;
            if (queryTime > maxAcceptableQueryTime)
            {
                _logger.LogWarning("Database query time ({QueryTime}ms) exceeds acceptable threshold ({MaxTime}ms)",
                    queryTime, maxAcceptableQueryTime);

                return HealthCheckResult.Degraded(
                    $"Database response time ({queryTime}ms) is above acceptable threshold ({maxAcceptableQueryTime}ms)",
                    null, data);
            }

            _logger.LogDebug("Database health check passed. Query time: {QueryTime}ms", queryTime);
            return HealthCheckResult.Healthy("Database is responsive", data);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Database health check was cancelled");
            return HealthCheckResult.Unhealthy("Database health check timed out", null, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database health check failed", ex, data);
        }
    }

    /// <summary>
    /// Tests basic database connectivity.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if connection successful, false otherwise.</returns>
    private async Task<bool> TestDatabaseConnectivityAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Use a simple query to test connectivity
            await _repository.TestConnectionAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connectivity test failed");
            return false;
        }
    }

    /// <summary>
    /// Tests database query performance.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query execution time in milliseconds.</returns>
    private async Task<long> TestQueryPerformanceAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Execute a representative query
            await _repository.GetHealthCheckSummaryAsync(cancellationToken);
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Database query performance test failed after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}

/// <summary>
/// Health check for file storage system.
/// </summary>
public class FileStorageHealthCheck : IHealthCheck
{
    private readonly IFileStorage _fileStorage;
    private readonly IOptionsMonitor<FileStorageSettings> _fileStorageOptions;
    private readonly ILogger<FileStorageHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileStorageHealthCheck"/> class.
    /// </summary>
    /// <param name="fileStorage">The file storage service.</param>
    /// <param name="fileStorageOptions">The file storage configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public FileStorageHealthCheck(
        IFileStorage fileStorage,
        IOptionsMonitor<FileStorageSettings> fileStorageOptions,
        ILogger<FileStorageHealthCheck> logger)
    {
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _fileStorageOptions = fileStorageOptions ?? throw new ArgumentNullException(nameof(fileStorageOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs the health check asynchronously.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The health check result.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var basePath = _fileStorageOptions.CurrentValue.BasePath;
            data["BasePath"] = basePath;

            // Check if base directory exists and is accessible
            var directoryExists = Directory.Exists(basePath);
            data["DirectoryExists"] = directoryExists;

            if (!directoryExists)
            {
                return HealthCheckResult.Unhealthy($"File storage base directory does not exist: {basePath}", null, data);
            }

            // Test write permissions
            var canWrite = await TestWritePermissionsAsync(basePath, cancellationToken);
            data["CanWrite"] = canWrite;

            if (!canWrite)
            {
                return HealthCheckResult.Unhealthy("No write permissions to file storage directory", null, data);
            }

            // Check available disk space
            var availableSpace = GetAvailableDiskSpace(basePath);
            data["AvailableSpaceGB"] = Math.Round(availableSpace / (1024.0 * 1024.0 * 1024.0), 2);

            var minRequiredSpaceGB = _fileStorageOptions.CurrentValue.MinRequiredSpaceGB;
            if (availableSpace < minRequiredSpaceGB * 1024 * 1024 * 1024)
            {
                return HealthCheckResult.Degraded(
                    $"Available disk space ({availableSpace / (1024 * 1024 * 1024):F2} GB) is below minimum required ({minRequiredSpaceGB} GB)",
                    null, data);
            }

            stopwatch.Stop();
            data["CheckTimeMs"] = stopwatch.ElapsedMilliseconds;

            _logger.LogDebug("File storage health check passed. Available space: {AvailableSpaceGB} GB",
                Math.Round(availableSpace / (1024.0 * 1024.0 * 1024.0), 2));

            return HealthCheckResult.Healthy("File storage is accessible", data);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("File storage health check was cancelled");
            return HealthCheckResult.Unhealthy("File storage health check timed out", null, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File storage health check failed");
            return HealthCheckResult.Unhealthy("File storage health check failed", ex, data);
        }
    }

    /// <summary>
    /// Tests write permissions to the specified directory.
    /// </summary>
    /// <param name="path">The directory path to test.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if write permissions exist, false otherwise.</returns>
    private async Task<bool> TestWritePermissionsAsync(string path, CancellationToken cancellationToken)
    {
        var testFileName = Path.Combine(path, $"health-check-{Guid.NewGuid()}.tmp");

        try
        {
            var testData = System.Text.Encoding.UTF8.GetBytes("health check test");
            await _fileStorage.SaveFileAsync(testFileName, testData, cancellationToken);

            // Clean up test file
            if (File.Exists(testFileName))
            {
                File.Delete(testFileName);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Write permission test failed for path: {Path}", path);

            // Ensure cleanup even if save failed
            try
            {
                if (File.Exists(testFileName))
                {
                    File.Delete(testFileName);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }

            return false;
        }
    }

    /// <summary>
    /// Gets the available disk space for the specified path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>The available space in bytes.</returns>
    private static long GetAvailableDiskSpace(string path)
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(path) ?? path);
            return driveInfo.AvailableFreeSpace;
        }
        catch
        {
            return 0;
        }
    }
}

/// <summary>
/// Health check for virus scanner service.
/// </summary>
public class VirusScannerHealthCheck : IHealthCheck
{
    private readonly IVirusScanner _virusScanner;
    private readonly IOptionsMonitor<VirusScannerSettings> _virusScannerOptions;
    private readonly ILogger<VirusScannerHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VirusScannerHealthCheck"/> class.
    /// </summary>
    /// <param name="virusScanner">The virus scanner service.</param>
    /// <param name="virusScannerOptions">The virus scanner configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public VirusScannerHealthCheck(
        IVirusScanner virusScanner,
        IOptionsMonitor<VirusScannerSettings> virusScannerOptions,
        ILogger<VirusScannerHealthCheck> logger)
    {
        _virusScanner = virusScanner ?? throw new ArgumentNullException(nameof(virusScanner));
        _virusScannerOptions = virusScannerOptions ?? throw new ArgumentNullException(nameof(virusScannerOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs the health check asynchronously.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The health check result.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var settings = _virusScannerOptions.CurrentValue;

        try
        {
            data["Enabled"] = settings.Enabled;
            data["Host"] = settings.Host;
            data["Port"] = settings.Port;

            if (!settings.Enabled)
            {
                return HealthCheckResult.Healthy("Virus scanner is disabled", data);
            }

            // Test connectivity to ClamAV
            var canConnect = await TestClamAvConnectivityAsync(settings, cancellationToken);
            data["CanConnect"] = canConnect;

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to virus scanner", null, data);
            }

            // Test scanning functionality with a test string
            var testData = System.Text.Encoding.UTF8.GetBytes("This is a test file for virus scanning health check.");
            using var testStream = new MemoryStream(testData);

            var scanResult = await _virusScanner.ScanFileForVirusesAsync(testStream);
            data["ScanResult"] = scanResult.IsClean ? "Clean" : "Infected";
            data["ScanMessage"] = scanResult.Message;

            if (!scanResult.IsClean && !scanResult.Message.Contains("test", StringComparison.OrdinalIgnoreCase))
            {
                // If it's not clean and not a test virus, something might be wrong
                _logger.LogWarning("Virus scanner health check returned unexpected result: {Message}", scanResult.Message);
                return HealthCheckResult.Degraded($"Virus scanner returned unexpected result: {scanResult.Message}", null, data);
            }

            _logger.LogDebug("Virus scanner health check passed");
            return HealthCheckResult.Healthy("Virus scanner is operational", data);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Virus scanner health check was cancelled");
            return HealthCheckResult.Unhealthy("Virus scanner health check timed out", null, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Virus scanner health check failed");
            return HealthCheckResult.Unhealthy("Virus scanner health check failed", ex, data);
        }
    }

    /// <summary>
    /// Tests connectivity to ClamAV daemon.
    /// </summary>
    /// <param name="settings">The virus scanner settings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if connection successful, false otherwise.</returns>
    private async Task<bool> TestClamAvConnectivityAsync(VirusScannerSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(settings.Host, settings.TimeoutSeconds * 1000);

            if (reply.Status != IPStatus.Success)
            {
                _logger.LogWarning("Ping to virus scanner host {Host} failed: {Status}", settings.Host, reply.Status);
                return false;
            }

            // Additional port connectivity test could be added here
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Connectivity test to virus scanner failed");
            return false;
        }
    }
}

/// <summary>
/// Health check for external API dependencies.
/// </summary>
public class ExternalApiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<ExternalApiSettings> _externalApiOptions;
    private readonly ILogger<ExternalApiHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalApiHealthCheck"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="externalApiOptions">The external API configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public ExternalApiHealthCheck(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<ExternalApiSettings> externalApiOptions,
        ILogger<ExternalApiHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _externalApiOptions = externalApiOptions ?? throw new ArgumentNullException(nameof(externalApiOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs the health check asynchronously.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The health check result.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var settings = _externalApiOptions.CurrentValue;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            data["BaseUrl"] = settings.BaseUrl;

            if (string.IsNullOrWhiteSpace(settings.BaseUrl))
            {
                return HealthCheckResult.Healthy("External API is not configured", data);
            }

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);

            // Test basic connectivity with a HEAD request to avoid data transfer
            var healthCheckUrl = new Uri(new Uri(settings.BaseUrl), "/health");
            using var response = await httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, healthCheckUrl),
                cancellationToken);

            stopwatch.Stop();
            data["ResponseTimeMs"] = stopwatch.ElapsedMilliseconds;
            data["StatusCode"] = (int)response.StatusCode;
            data["IsSuccessful"] = response.IsSuccessStatusCode;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("External API health check failed with status code: {StatusCode}", response.StatusCode);
                return HealthCheckResult.Degraded(
                    $"External API responded with status code: {response.StatusCode}",
                    null, data);
            }

            var maxAcceptableResponseTime = settings.MaxAcceptableResponseTimeMs;
            if (stopwatch.ElapsedMilliseconds > maxAcceptableResponseTime)
            {
                _logger.LogWarning("External API response time ({ResponseTime}ms) exceeds acceptable threshold ({MaxTime}ms)",
                    stopwatch.ElapsedMilliseconds, maxAcceptableResponseTime);

                return HealthCheckResult.Degraded(
                    $"External API response time ({stopwatch.ElapsedMilliseconds}ms) is above acceptable threshold ({maxAcceptableResponseTime}ms)",
                    null, data);
            }

            _logger.LogDebug("External API health check passed. Response time: {ResponseTime}ms", stopwatch.ElapsedMilliseconds);
            return HealthCheckResult.Healthy("External API is responsive", data);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            stopwatch.Stop();
            data["ResponseTimeMs"] = stopwatch.ElapsedMilliseconds;
            _logger.LogWarning("External API health check timed out after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            return HealthCheckResult.Unhealthy("External API health check timed out", ex, data);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            data["ResponseTimeMs"] = stopwatch.ElapsedMilliseconds;
            _logger.LogError(ex, "External API health check failed with HTTP request exception");
            return HealthCheckResult.Unhealthy("External API is unreachable", ex, data);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            data["ResponseTimeMs"] = stopwatch.ElapsedMilliseconds;
            _logger.LogError(ex, "External API health check failed");
            return HealthCheckResult.Unhealthy("External API health check failed", ex, data);
        }
    }
}

/// <summary>
/// Configuration settings for database health checks.
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// Gets or sets the maximum acceptable query time in milliseconds for health checks.
    /// </summary>
    public int HealthCheckMaxQueryTimeMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the health check timeout in seconds.
    /// </summary>
    public int HealthCheckTimeoutSeconds { get; set; } = 10;
}

/// <summary>
/// Configuration settings for external API health checks.
/// </summary>
public class ExternalApiSettings
{
    /// <summary>
    /// Gets or sets the base URL for the external API.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout for API calls in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum acceptable response time in milliseconds.
    /// </summary>
    public int MaxAcceptableResponseTimeMs { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the API key for authentication.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}