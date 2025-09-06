using ADMS.API.Extensions;
using ADMS.API.Services;
using ADMS.Domain.Entities;

using Microsoft.Extensions.Options;

using System.Diagnostics;

namespace ADMS.API.BackgroundServices;

/// <summary>
/// Background service for cleaning up temporary files and expired cache entries.
/// </summary>
public class FileCleanupService : BackgroundService
{
    private readonly ILogger<FileCleanupService> _logger;
    private readonly IOptionsMonitor<FileStorageSettings> _fileStorageOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="FileCleanupService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="fileStorageOptions">The file storage configuration options.</param>
    /// <param name="serviceScopeFactory">The service scope factory for creating scoped services.</param>
    public FileCleanupService(
        ILogger<FileCleanupService> logger,
        IOptionsMonitor<FileStorageSettings> fileStorageOptions,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileStorageOptions = fileStorageOptions ?? throw new ArgumentNullException(nameof(fileStorageOptions));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token for stopping the service.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("File cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupAsync(stoppingToken);
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("File cleanup service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during file cleanup");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait before retrying
            }
        }

        _logger.LogInformation("File cleanup service stopped");
    }

    /// <summary>
    /// Performs the file cleanup operations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task PerformCleanupAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var settings = _fileStorageOptions.CurrentValue;
        var cleanupStats = new CleanupStatistics();

        _logger.LogDebug("Starting file cleanup operation");

        try
        {
            // Clean up temporary files
            await CleanupTemporaryFilesAsync(settings, cleanupStats, cancellationToken);

            // Clean up orphaned files
            await CleanupOrphanedFilesAsync(settings, cleanupStats, cancellationToken);

            // Clean up empty directories
            await CleanupEmptyDirectoriesAsync(settings, cleanupStats, cancellationToken);

            // Log cleanup statistics
            stopwatch.Stop();
            _logger.LogInformation(
                "File cleanup completed in {Duration}ms. Deleted: {DeletedFiles} files, {DeletedDirs} directories, Freed: {FreedSpace:N0} bytes",
                stopwatch.ElapsedMilliseconds,
                cleanupStats.DeletedFiles,
                cleanupStats.DeletedDirectories,
                cleanupStats.FreedSpace);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during file cleanup operation");
            throw;
        }
    }

    /// <summary>
    /// Cleans up temporary files that have exceeded their retention period.
    /// </summary>
    /// <param name="settings">The file storage settings.</param>
    /// <param name="stats">The cleanup statistics to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CleanupTemporaryFilesAsync(FileStorageSettings settings, CleanupStatistics stats, CancellationToken cancellationToken)
    {
        var tempPath = Path.Combine(settings.BasePath, "temp");
        if (!Directory.Exists(tempPath))
        {
            return;
        }

        var cutoffTime = DateTime.UtcNow.AddHours(-settings.TempFileRetentionHours);
        var tempFiles = Directory.GetFiles(tempPath, "*", SearchOption.AllDirectories)
            .Where(file => File.GetCreationTimeUtc(file) < cutoffTime);

        foreach (var file in tempFiles)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var fileInfo = new FileInfo(file);
                var fileSize = fileInfo.Length;

                File.Delete(file);
                stats.DeletedFiles++;
                stats.FreedSpace += fileSize;

                _logger.LogDebug("Deleted temporary file: {FilePath}", file);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temporary file: {FilePath}", file);
            }

            // Yield control periodically
            if (stats.DeletedFiles % 100 == 0)
            {
                await Task.Yield();
            }
        }
    }

    /// <summary>
    /// Cleans up orphaned files that no longer have corresponding database records.
    /// </summary>
    /// <param name="settings">The file storage settings.</param>
    /// <param name="stats">The cleanup statistics to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CleanupOrphanedFilesAsync(FileStorageSettings settings, CleanupStatistics stats, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

        var mattersPath = Path.Combine(settings.BasePath, "matters");
        if (!Directory.Exists(mattersPath))
        {
            return;
        }

        var matterDirectories = Directory.GetDirectories(mattersPath);

        foreach (var matterDir in matterDirectories)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var matterIdStr = Path.GetFileName(matterDir);
            if (!Guid.TryParse(matterIdStr, out var matterId))
            {
                continue;
            }

            try
            {
                // Check if matter exists in database
                var matter = await repository.GetMatterAsync(matterId, includeDocuments: true);
                if (matter == null)
                {
                    // Matter doesn't exist - delete entire directory
                    Directory.Delete(matterDir, recursive: true);
                    stats.DeletedDirectories++;
                    _logger.LogInformation("Deleted orphaned matter directory: {MatterId}", matterId);
                    continue;
                }

                // Check individual document files
                await CleanupOrphanedDocumentFilesAsync(matterDir, matter, stats, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error cleaning up matter directory: {MatterId}", matterId);
            }
        }
    }

    /// <summary>
    /// Cleans up orphaned document files within a matter directory.
    /// </summary>
    /// <param name="matterDir">The matter directory path.</param>
    /// <param name="matter">The matter entity with documents.</param>
    /// <param name="stats">The cleanup statistics to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CleanupOrphanedDocumentFilesAsync(string matterDir, Matter matter, CleanupStatistics stats, CancellationToken cancellationToken)
    {
        var documentFiles = Directory.GetFiles(matterDir, "*", SearchOption.TopDirectoryOnly)
            .Where(f => !Path.GetFileName(f).StartsWith('.')) // Exclude hidden files
            .ToList();

        var validDocumentIds = matter.Documents.SelectMany(d => d.Revisions)
            .Select(r => $"{r.DocumentId}R{r.RevisionNumber}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var file in documentFiles)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var fileName = Path.GetFileNameWithoutExtension(file);
            if (!validDocumentIds.Contains(fileName))
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    var fileSize = fileInfo.Length;

                    File.Delete(file);
                    stats.DeletedFiles++;
                    stats.FreedSpace += fileSize;

                    _logger.LogDebug("Deleted orphaned document file: {FilePath}", file);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete orphaned file: {FilePath}", file);
                }
            }

            await Task.Yield();
        }
    }

    /// <summary>
    /// Cleans up empty directories.
    /// </summary>
    /// <param name="settings">The file storage settings.</param>
    /// <param name="stats">The cleanup statistics to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CleanupEmptyDirectoriesAsync(FileStorageSettings settings, CleanupStatistics stats, CancellationToken cancellationToken)
    {
        var directories = Directory.GetDirectories(settings.BasePath, "*", SearchOption.AllDirectories)
            .OrderByDescending(d => d.Length); // Process deepest directories first

        foreach (var directory in directories)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                if (!Directory.EnumerateFileSystemEntries(directory).Any())
                {
                    Directory.Delete(directory);
                    stats.DeletedDirectories++;
                    _logger.LogDebug("Deleted empty directory: {DirectoryPath}", directory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete empty directory: {DirectoryPath}", directory);
            }

            await Task.Yield();
        }
    }

    /// <summary>
    /// Statistics for cleanup operations.
    /// </summary>
    private class CleanupStatistics
    {
        public int DeletedFiles { get; set; }
        public int DeletedDirectories { get; set; }
        public long FreedSpace { get; set; }
    }
}

/// <summary>
/// Background service for performing health checks and sending alerts.
/// </summary>
public class HealthCheckService : BackgroundService
{
    private readonly ILogger<HealthCheckService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IOptionsMonitor<HealthCheckSettings> _healthCheckOptions;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceScopeFactory">The service scope factory.</param>
    /// <param name="healthCheckOptions">The health check configuration options.</param>
    public HealthCheckService(
        ILogger<HealthCheckService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<HealthCheckSettings> healthCheckOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _healthCheckOptions = healthCheckOptions ?? throw new ArgumentNullException(nameof(healthCheckOptions));
    }

    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token for stopping the service.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Health check service started");

        // Wait for application to fully start
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformHealthChecksAsync(stoppingToken);
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Health check service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during health checks");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("Health check service stopped");
    }

    /// <summary>
    /// Performs health checks and handles alerts.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task PerformHealthChecksAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            // Check database health
            await CheckDatabaseHealthAsync(services, cancellationToken);

            // Check file storage health
            await CheckFileStorageHealthAsync(services, cancellationToken);

            // Check external services health
            await CheckExternalServicesHealthAsync(services, cancellationToken);

            // Check system resources
            await CheckSystemResourcesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check execution");
        }
    }

    /// <summary>
    /// Checks database health and connectivity.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CheckDatabaseHealthAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        try
        {
            var repository = services.GetRequiredService<IRepository>();
            var stopwatch = Stopwatch.StartNew();

            await repository.TestConnectionAsync(cancellationToken);
            stopwatch.Stop();

            var responseTime = stopwatch.ElapsedMilliseconds;
            if (responseTime > 5000) // 5 seconds
            {
                _logger.LogWarning("Database response time is slow: {ResponseTime}ms", responseTime);
            }
            else
            {
                _logger.LogDebug("Database health check passed: {ResponseTime}ms", responseTime);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            await SendHealthAlertAsync("Database", "Database connectivity failed", ex.Message);
        }
    }

    /// <summary>
    /// Checks file storage health and available space.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CheckFileStorageHealthAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        try
        {
            var fileStorageOptions = services.GetRequiredService<IOptionsMonitor<FileStorageSettings>>();
            var settings = fileStorageOptions.CurrentValue;

            if (!Directory.Exists(settings.BasePath))
            {
                _logger.LogError("File storage base path does not exist: {BasePath}", settings.BasePath);
                await SendHealthAlertAsync("FileStorage", "Base path not found", $"Path: {settings.BasePath}");
                return;
            }

            // Check available disk space
            var driveInfo = new DriveInfo(Path.GetPathRoot(settings.BasePath) ?? settings.BasePath);
            var availableSpaceGB = driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
            var totalSpaceGB = driveInfo.TotalSize / (1024.0 * 1024.0 * 1024.0);
            var usagePercentage = (1.0 - (driveInfo.AvailableFreeSpace / (double)driveInfo.TotalSize)) * 100;

            _logger.LogDebug("File storage space: {Available:F1}GB / {Total:F1}GB ({Usage:F1}% used)",
                availableSpaceGB, totalSpaceGB, usagePercentage);

            if (usagePercentage > 90)
            {
                _logger.LogWarning("File storage space is critically low: {Usage:F1}% used", usagePercentage);
                await SendHealthAlertAsync("FileStorage", "Low disk space",
                    $"Usage: {usagePercentage:F1}%, Available: {availableSpaceGB:F1}GB");
            }
            else if (usagePercentage > 80)
            {
                _logger.LogWarning("File storage space is getting low: {Usage:F1}% used", usagePercentage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File storage health check failed");
            await SendHealthAlertAsync("FileStorage", "Health check failed", ex.Message);
        }
    }

    /// <summary>
    /// Checks external services health.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CheckExternalServicesHealthAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        // Check virus scanner if enabled
        try
        {
            var virusScannerOptions = services.GetRequiredService<IOptionsMonitor<VirusScannerSettings>>();
            var settings = virusScannerOptions.CurrentValue;

            if (settings.Enabled)
            {
                var virusScanner = services.GetRequiredService<IVirusScanner>();
                // Perform a simple connectivity test
                using var testStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("test"));
                var result = await virusScanner.ScanFileForVirusesAsync(testStream);

                if (!result.IsClean && !result.Message.Contains("test", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Virus scanner health check returned unexpected result: {Message}", result.Message);
                }
                else
                {
                    _logger.LogDebug("Virus scanner health check passed");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Virus scanner health check failed");
            await SendHealthAlertAsync("VirusScanner", "Service unavailable", ex.Message);
        }
    }

    /// <summary>
    /// Checks system resources like memory and CPU usage.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CheckSystemResourcesAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Check memory usage
            var memoryBefore = GC.GetTotalMemory(false);
            var memoryMB = memoryBefore / (1024.0 * 1024.0);

            _logger.LogDebug("Current memory usage: {Memory:F1} MB", memoryMB);

            // Alert if memory usage is very high (this is a simple check)
            if (memoryMB > 1000) // 1 GB
            {
                _logger.LogWarning("High memory usage detected: {Memory:F1} MB", memoryMB);

                // Force garbage collection and check again
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var memoryAfter = GC.GetTotalMemory(true);
                var memoryAfterMB = memoryAfter / (1024.0 * 1024.0);

                if (memoryAfterMB > 500) // Still high after GC
                {
                    await SendHealthAlertAsync("Memory", "High memory usage",
                        $"Memory usage: {memoryAfterMB:F1} MB after GC");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System resources health check failed");
        }
    }

    /// <summary>
    /// Sends a health alert notification.
    /// </summary>
    /// <param name="component">The component that triggered the alert.</param>
    /// <param name="issue">The issue description.</param>
    /// <param name="details">Additional details about the issue.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SendHealthAlertAsync(string component, string issue, string details)
    {
        try
        {
            var settings = _healthCheckOptions.CurrentValue;
            if (!settings.EnableAlerts)
            {
                return;
            }

            _logger.LogWarning("Health alert triggered - Component: {Component}, Issue: {Issue}, Details: {Details}",
                component, issue, details);

            // Here you would integrate with your alerting system (email, Slack, etc.)
            // For now, we'll just log the alert

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send health alert");
        }
    }
}

/// <summary>
/// Background service for optimizing database performance and maintenance.
/// </summary>
public class DatabaseMaintenanceService : BackgroundService
{
    private readonly ILogger<DatabaseMaintenanceService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IOptionsMonitor<DatabaseMaintenanceSettings> _maintenanceOptions;
    private readonly TimeSpan _maintenanceInterval = TimeSpan.FromHours(6);

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseMaintenanceService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceScopeFactory">The service scope factory.</param>
    /// <param name="maintenanceOptions">The database maintenance configuration options.</param>
    public DatabaseMaintenanceService(
        ILogger<DatabaseMaintenanceService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<DatabaseMaintenanceSettings> maintenanceOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _maintenanceOptions = maintenanceOptions ?? throw new ArgumentNullException(nameof(maintenanceOptions));
    }

    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token for stopping the service.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Database maintenance service started");

        // Wait for application to fully start
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformMaintenanceAsync(stoppingToken);
                await Task.Delay(_maintenanceInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Database maintenance service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database maintenance");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        _logger.LogInformation("Database maintenance service stopped");
    }

    /// <summary>
    /// Performs database maintenance operations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task PerformMaintenanceAsync(CancellationToken cancellationToken)
    {
        var settings = _maintenanceOptions.CurrentValue;
        if (!settings.EnableAutomaticMaintenance)
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting database maintenance");

        try
        {
            // Clean up old audit records
            if (settings.CleanupAuditRecords)
            {
                await CleanupOldAuditRecordsAsync(repository, settings, cancellationToken);
            }

            // Update database statistics
            if (settings.UpdateStatistics)
            {
                await UpdateDatabaseStatisticsAsync(repository, cancellationToken);
            }

            // Rebuild fragmented indexes
            if (settings.RebuildIndexes)
            {
                await RebuildFragmentedIndexesAsync(repository, cancellationToken);
            }

            stopwatch.Stop();
            _logger.LogInformation("Database maintenance completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database maintenance");
            throw;
        }
    }

    /// <summary>
    /// Cleans up old audit records based on retention policy.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="settings">The maintenance settings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CleanupOldAuditRecordsAsync(IRepository repository, DatabaseMaintenanceSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-settings.AuditRetentionDays);
            var deletedCount = await repository.CleanupOldAuditRecordsAsync(cutoffDate, cancellationToken);

            if (deletedCount > 0)
            {
                _logger.LogInformation("Cleaned up {Count} old audit records older than {CutoffDate}",
                    deletedCount, cutoffDate.ToString("yyyy-MM-dd"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old audit records");
        }
    }

    /// <summary>
    /// Updates database statistics for better query performance.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UpdateDatabaseStatisticsAsync(IRepository repository, CancellationToken cancellationToken)
    {
        try
        {
            await repository.UpdateStatisticsAsync(cancellationToken);
            _logger.LogDebug("Database statistics updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating database statistics");
        }
    }

    /// <summary>
    /// Rebuilds fragmented indexes to improve query performance.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RebuildFragmentedIndexesAsync(IRepository repository, CancellationToken cancellationToken)
    {
        try
        {
            var rebuiltIndexes = await repository.RebuildFragmentedIndexesAsync(cancellationToken);

            if (rebuiltIndexes > 0)
            {
                _logger.LogInformation("Rebuilt {Count} fragmented indexes", rebuiltIndexes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding fragmented indexes");
        }
    }
}

/// <summary>
/// Configuration settings for health checks.
/// </summary>
public class HealthCheckSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether health check alerts are enabled.
    /// </summary>
    public bool EnableAlerts { get; set; } = true;

    /// <summary>
    /// Gets or sets the alert threshold for disk usage percentage.
    /// </summary>
    public double DiskUsageAlertThreshold { get; set; } = 80.0;

    /// <summary>
    /// Gets or sets the alert threshold for memory usage in MB.
    /// </summary>
    public double MemoryUsageAlertThreshold { get; set; } = 1000.0;

    /// <summary>
    /// Gets or sets the alert threshold for database response time in milliseconds.
    /// </summary>
    public int DatabaseResponseTimeAlertThreshold { get; set; } = 5000;
}

/// <summary>
/// Configuration settings for database maintenance.
/// </summary>
public class DatabaseMaintenanceSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether automatic maintenance is enabled.
    /// </summary>
    public bool EnableAutomaticMaintenance { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to clean up old audit records.
    /// </summary>
    public bool CleanupAuditRecords { get; set; } = true;

    /// <summary>
    /// Gets or sets the audit record retention period in days.
    /// </summary>
    public int AuditRetentionDays { get; set; } = 365;

    /// <summary>
    /// Gets or sets a value indicating whether to update database statistics.
    /// </summary>
    public bool UpdateStatistics { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to rebuild fragmented indexes.
    /// </summary>
    public bool RebuildIndexes { get; set; } = true;

    /// <summary>
    /// Gets or sets the fragmentation threshold for index rebuilding.
    /// </summary>
    public double IndexFragmentationThreshold { get; set; } = 30.0;
}

/// <summary>
/// Extension methods for registering background services.
/// </summary>
public static class BackgroundServiceExtensions
{
    /// <summary>
    /// Adds background services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure settings
        services.Configure<HealthCheckSettings>(configuration.GetSection("HealthChecks"));
        services.Configure<DatabaseMaintenanceSettings>(configuration.GetSection("DatabaseMaintenance"));

        // Register background services
        services.AddHostedService<FileCleanupService>();
        services.AddHostedService<HealthCheckService>();
        services.AddHostedService<DatabaseMaintenanceService>();

        return services;
    }
}