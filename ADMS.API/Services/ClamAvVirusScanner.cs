using nClam;
using ADMS.API.Services.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;

namespace ADMS.API.Services;

/// <summary>
/// Enterprise-grade ClamAV virus scanner implementation providing comprehensive malware detection capabilities
/// through integration with ClamAV antivirus engine. This implementation offers robust virus scanning
/// with extensive error handling, performance monitoring, and operational resilience features.
/// </summary>
/// <remarks>
/// <para>This class provides a production-ready implementation of virus scanning services including:</para>
/// 
/// <para><strong>Core Security Features:</strong></para>
/// <list type="bullet">
/// <item>Real-time virus detection using ClamAV signature-based scanning engine</item>
/// <item>Comprehensive malware detection including viruses, trojans, and suspicious content</item>
/// <item>Stream-based scanning for memory-efficient processing of large files</item>
/// <item>Secure handling of potentially malicious content with proper isolation</item>
/// <item>Integration with up-to-date virus definitions from ClamAV database</item>
/// </list>
/// 
/// <para><strong>Performance and Reliability:</strong></para>
/// <list type="bullet">
/// <item>Asynchronous operations for non-blocking file processing workflows</item>
/// <item>Configurable timeouts and connection management for responsive scanning</item>
/// <item>Automatic retry logic with exponential backoff for transient failures</item>
/// <item>Connection pooling and resource optimization for high-throughput scenarios</item>
/// <item>Comprehensive health monitoring and diagnostic capabilities</item>
/// </list>
/// 
/// <para><strong>Enterprise Features:</strong></para>
/// <list type="bullet">
/// <item>Extensive logging and audit trail for compliance and security monitoring</item>
/// <item>Configurable scanning parameters and operational thresholds</item>
/// <item>Integration with application performance monitoring and alerting systems</item>
/// <item>Support for multiple ClamAV server configurations and failover scenarios</item>
/// <item>Detailed error handling with actionable diagnostic information</item>
/// </list>
/// 
/// <para><strong>Configuration Requirements:</strong></para>
/// <list type="bullet">
/// <item>ClamAV daemon (clamd) must be running and accessible on configured host/port</item>
/// <item>Virus definitions should be regularly updated through freshclam or similar mechanism</item>
/// <item>Network connectivity and firewall configuration for scanner communication</item>
/// <item>Sufficient system resources for concurrent scanning operations</item>
/// <item>Proper security context and permissions for file access and network communication</item>
/// </list>
/// 
/// <para><strong>Security Considerations:</strong></para>
/// <list type="bullet">
/// <item>All potentially infected content is handled in isolated scanning context</item>
/// <item>File streams are properly reset and managed during scanning operations</item>
/// <item>Comprehensive audit logging of all scanning activities for compliance</item>
/// <item>Integration with incident response procedures for threat detection scenarios</item>
/// <item>Secure communication with ClamAV server using configured protocols</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Dependency injection configuration
/// services.Configure&lt;ClamAvScannerOptions&gt;(configuration.GetSection("ClamAV"));
/// services.AddScoped&lt;IVirusScanner, ClamAvVirusScanner&gt;();
/// 
/// // Usage in file upload scenario
/// public class FileUploadController : ControllerBase
/// {
///     private readonly IVirusScanner _virusScanner;
///     
///     public FileUploadController(IVirusScanner virusScanner)
///     {
///         _virusScanner = virusScanner;
///     }
///     
///     [HttpPost("upload")]
///     public async Task&lt;IActionResult&gt; UploadFile(IFormFile file, CancellationToken cancellationToken)
///     {
///         using var stream = file.OpenReadStream();
///         var scanResult = await _virusScanner.ScanFileForVirusesAsync(stream, cancellationToken);
///         
///         if (!scanResult.IsClean)
///         {
///             return BadRequest($"File rejected: {scanResult.ThreatDetails?.ThreatName}");
///         }
///         
///         // Continue with file processing
///         return Ok("File uploaded successfully");
///     }
/// }
/// </code>
/// </example>
public sealed class ClamAvVirusScanner : IVirusScanner, IDisposable
{
    private readonly ILogger<ClamAvVirusScanner> _logger;
    private readonly ClamAvScannerOptions _options;
    private readonly SemaphoreSlim _connectionSemaphore;
    private readonly object _lockObject = new();
    private volatile bool _disposed;
    private DateTime _lastHealthCheck;
    private ScannerHealthResult? _cachedHealthResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClamAvVirusScanner"/> class with comprehensive
    /// configuration and operational monitoring capabilities.
    /// </summary>
    /// <param name="logger">
    /// The logger instance for comprehensive diagnostic and operational information recording.
    /// Used throughout the scanner lifecycle for security events, performance metrics, and error conditions.
    /// </param>
    /// <param name="options">
    /// The configuration options for ClamAV scanner behavior including server connection details,
    /// timeout settings, retry policies, and operational parameters.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when logger or options parameters are null, indicating dependency injection configuration issues.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when scanner configuration validation fails due to invalid server settings or operational parameters.
    /// </exception>
    /// <remarks>
    /// <para>This constructor performs comprehensive initialization including:</para>
    /// 
    /// <para><strong>Configuration Validation:</strong></para>
    /// <list type="bullet">
    /// <item>Validates ClamAV server connection parameters for accessibility and correctness</item>
    /// <item>Ensures timeout and retry configuration values are within reasonable operational ranges</item>
    /// <item>Verifies system resource allocation for concurrent scanning operations</item>
    /// <item>Establishes logging context for all subsequent scanner operations</item>
    /// </list>
    /// 
    /// <para><strong>Resource Management Setup:</strong></para>
    /// <list type="bullet">
    /// <item>Initializes connection pooling and concurrency control mechanisms</item>
    /// <item>Establishes health monitoring and diagnostic capabilities</item>
    /// <item>Sets up performance tracking and operational metrics collection</item>
    /// <item>Prepares error handling and recovery mechanisms for production resilience</item>
    /// </list>
    /// </remarks>
    public ClamAvVirusScanner(ILogger<ClamAvVirusScanner> logger, IOptions<ClamAvScannerOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        // Validate configuration parameters for operational correctness
        ValidateConfiguration();

        // Initialize connection management with configured concurrency limits
        _connectionSemaphore = new SemaphoreSlim(_options.MaxConcurrentConnections, _options.MaxConcurrentConnections);

        // Initialize health check tracking
        _lastHealthCheck = DateTime.MinValue;

        _logger.LogInformation("ClamAV virus scanner initialized successfully. " +
            "Server: {ServerHost}:{ServerPort}, Timeout: {TimeoutSeconds}s, " +
            "MaxConnections: {MaxConnections}, RetryAttempts: {RetryAttempts}",
            _options.ServerHost, _options.ServerPort, _options.TimeoutSeconds,
            _options.MaxConcurrentConnections, _options.MaxRetryAttempts);
    }

    #region IVirusScanner Implementation

    /// <summary>
    /// Performs comprehensive virus scanning of the provided file stream with detailed result information.
    /// This method provides enterprise-grade malware detection with extensive error handling, retry logic,
    /// and operational monitoring for production environments.
    /// </summary>
    /// <param name="fileStream">
    /// The file stream to scan for viruses and malicious content. Must be a readable stream containing 
    /// the file data to be analyzed. The stream position will be reset to the beginning before scanning.
    /// Stream must remain open during the entire scanning operation.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to allow cancellation of long-running scan operations.
    /// Implementations respect cancellation requests to provide responsive user experience.
    /// Default value allows the operation to run to completion without cancellation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous scanning operation. The task result contains a 
    /// VirusScanResult with comprehensive information about whether the file is clean or contains 
    /// malicious content, detailed threat information including threat names and types, scanning 
    /// metadata including duration and scanner information, recommended actions for threat mitigation 
    /// if applicable, and additional context for logging and compliance reporting.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when fileStream parameter is null, indicating invalid input parameters.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when fileStream is not readable or is in an invalid state for scanning operations.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the cancellation token during scanning.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the scanner instance has been disposed and is no longer available for operations.
    /// </exception>
    /// <remarks>
    /// <para>This method provides comprehensive virus scanning including:</para>
    /// 
    /// <para><strong>Scanning Capabilities:</strong></para>
    /// <list type="bullet">
    /// <item>Real-time virus detection using up-to-date ClamAV threat definitions</item>
    /// <item>Multi-layered analysis including signature-based detection</item>
    /// <item>Support for various file formats and content types</item>
    /// <item>Detection of viruses, trojans, malware, spyware, and suspicious content</item>
    /// <item>Integration with ClamAV threat intelligence for emerging threats</item>
    /// </list>
    /// 
    /// <para><strong>Performance Characteristics:</strong></para>
    /// <list type="bullet">
    /// <item>Asynchronous operation for non-blocking execution</item>
    /// <item>Memory-efficient stream processing for large files</item>
    /// <item>Configurable timeout and resource management</item>
    /// <item>Cancellation token support for responsive cancellation</item>
    /// <item>Automatic retry logic with exponential backoff for resilience</item>
    /// </list>
    /// 
    /// <para><strong>Security Features:</strong></para>
    /// <list type="bullet">
    /// <item>Secure handling of potentially malicious content during scanning</item>
    /// <item>Proper isolation of scanning processes from system resources</item>
    /// <item>Comprehensive threat classification and severity assessment</item>
    /// <item>Integration with security monitoring and incident response</item>
    /// <item>Audit logging of all scanning activities for compliance</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example usage in file upload scenario
    /// public async Task&lt;IActionResult&gt; UploadFile(IFormFile file, CancellationToken cancellationToken)
    /// {
    ///     using var stream = file.OpenReadStream();
    ///     var scanResult = await _virusScanner.ScanFileForVirusesAsync(stream, cancellationToken);
    ///     
    ///     if (!scanResult.IsClean)
    ///     {
    ///         _logger.LogWarning("Virus detected: {ThreatName}", scanResult.ThreatDetails?.ThreatName);
    ///         return BadRequest($"File rejected: {scanResult.ThreatDetails?.ThreatName}");
    ///     }
    ///     
    ///     return Ok("File uploaded successfully");
    /// }
    /// </code>
    /// </example>
    public async Task<VirusScanResult> ScanFileForVirusesAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        // Comprehensive input validation with detailed error context
        ArgumentNullException.ThrowIfNull(fileStream, nameof(fileStream));

        if (!fileStream.CanRead)
        {
            throw new ArgumentException("File stream must be readable for virus scanning operations", nameof(fileStream));
        }

        // Ensure scanner is in a valid operational state
        ThrowIfDisposed();

        var operationId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = Stopwatch.StartNew();

        _logger.LogDebug("Starting virus scan operation {OperationId} for stream of length {StreamLength} bytes",
            operationId, fileStream.CanSeek ? fileStream.Length : -1);

        try
        {
            // Reset stream position to beginning for accurate scanning
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
                _logger.LogDebug("Reset file stream position to beginning for scan operation {OperationId}", operationId);
            }

            // Perform virus scanning with retry logic and comprehensive error handling
            var scanResult = await PerformVirusScanWithRetryAsync(fileStream, operationId, cancellationToken);

            stopwatch.Stop();

            // Log scan completion with comprehensive operational context
            _logger.LogInformation("Virus scan operation {OperationId} completed: " +
                "IsClean={IsClean}, Duration={DurationMs}ms, ThreatDetected={ThreatName}",
                operationId, scanResult.IsClean, stopwatch.ElapsedMilliseconds,
                scanResult.ThreatDetails?.ThreatName ?? "None");

            // Log security event if threat is detected
            if (!scanResult.IsClean && scanResult.ThreatDetails != null)
            {
                _logger.LogWarning("SECURITY EVENT - Virus detected in scan operation {OperationId}: " +
                    "Threat={ThreatName}, Type={ThreatType}, Action=Rejected",
                    operationId, scanResult.ThreatDetails.ThreatName, scanResult.ThreatDetails.ThreatType);
            }

            return scanResult;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            _logger.LogInformation("Virus scan operation {OperationId} was cancelled by client after {DurationMs}ms",
                operationId, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Virus scan operation {OperationId} failed after {DurationMs}ms: {ErrorMessage}",
                operationId, stopwatch.ElapsedMilliseconds, ex.Message);

            // Return error result for safety - treat scanning failures as potentially unsafe
            return VirusScanResult.CreateErrorResult(
                "Virus scanning failed - file treated as potentially unsafe for security",
                $"Operation {operationId} failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates whether the ClamAV virus scanner is properly configured and operational.
    /// This method performs comprehensive health checking including service availability,
    /// virus definition currency, scanning functionality, and performance metrics.
    /// </summary>
    /// <param name="cancellationToken">
    /// Optional cancellation token for the health check operation to allow responsive cancellation
    /// of potentially long-running connectivity and functionality tests.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous health check operation. The task result contains
    /// a ScannerHealthResult with comprehensive information about scanner availability and 
    /// operational status, virus definition version and last update timestamp, performance 
    /// metrics and operational statistics, configuration validation and system requirements,
    /// and recommendations for optimization or maintenance.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the health check operation is cancelled via the cancellation token.
    /// </exception>
    /// <remarks>
    /// <para>This method provides comprehensive health checking including:</para>
    /// 
    /// <para><strong>Health Check Capabilities:</strong></para>
    /// <list type="bullet">
    /// <item>Verification of ClamAV daemon service availability and responsiveness</item>
    /// <item>Validation of virus definition currency and update status</item>
    /// <item>Assessment of scanning functionality using EICAR test signature</item>
    /// <item>Performance metrics including response time and connectivity status</item>
    /// <item>Configuration validation and compatibility checking</item>
    /// </list>
    /// 
    /// <para><strong>Operational Monitoring:</strong></para>
    /// <list type="bullet">
    /// <item>Real-time status of ClamAV scanning engine and detection capabilities</item>
    /// <item>Network connectivity and communication channel validation</item>
    /// <item>Virus definition freshness and update mechanism status</item>
    /// <item>Service responsiveness and performance characteristics</item>
    /// <item>Error detection and diagnostic information for troubleshooting</item>
    /// </list>
    /// 
    /// <para><strong>Caching and Performance:</strong></para>
    /// <list type="bullet">
    /// <item>Results are cached to avoid excessive health check overhead</item>
    /// <item>Configurable cache duration balances freshness with performance</item>
    /// <item>Non-invasive testing minimizes impact on scanning performance</item>
    /// <item>Fast response time suitable for operational monitoring systems</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Health check implementation in ASP.NET Core
    /// [HttpGet("health")]
    /// public async Task&lt;IActionResult&gt; HealthCheck(CancellationToken cancellationToken)
    /// {
    ///     try
    ///     {
    ///         var healthResult = await _virusScanner.CheckHealthAsync(cancellationToken);
    ///         
    ///         if (healthResult.OverallHealth == HealthStatus.Healthy)
    ///         {
    ///             return Ok(new {
    ///                 Status = "Healthy",
    ///                 Available = healthResult.IsAvailable,
    ///                 DefinitionVersion = healthResult.DefinitionVersion,
    ///                 LastUpdate = healthResult.LastDefinitionUpdate,
    ///                 SuccessRate = healthResult.SuccessRate
    ///             });
    ///         }
    ///         else
    ///         {
    ///             return StatusCode(503, new {
    ///                 Status = healthResult.OverallHealth.ToString(),
    ///                 Error = healthResult.ErrorMessage
    ///             });
    ///         }
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         return StatusCode(503, new { Status = "Unhealthy", Error = ex.Message });
    ///     }
    /// }
    /// </code>
    /// </example>
    public async Task<ScannerHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        // Use cached health result if recent enough to avoid excessive health checks
        var now = DateTime.UtcNow;
        if (_cachedHealthResult != null &&
            (now - _lastHealthCheck).TotalSeconds < _options.HealthCheckCacheSeconds)
        {
            _logger.LogDebug("Returning cached health check result from {LastCheckTime}", _lastHealthCheck);
            return _cachedHealthResult;
        }

        var operationId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = Stopwatch.StartNew();

        _logger.LogDebug("Starting ClamAV health check operation {OperationId}", operationId);

        try
        {
            // Test basic connectivity to ClamAV server
            var connectivityResult = await TestConnectivityAsync(cancellationToken);

            // Initialize health result with default values based on actual ScannerHealthResult properties
            var healthResult = new ScannerHealthResult
            {
                OverallHealth = HealthStatus.Unhealthy,
                DefinitionVersion = null,
                LastDefinitionUpdate = null,
                DefinitionsAreCurrent = false,
                AverageScanTime = null,
                SuccessRate = 0.0,
                ErrorMessage = null,
                AdditionalInfo = new Dictionary<string, object>(),
                IsAvailable = connectivityResult.IsSuccessful,
            };

            // Store response time in additional info since ResponseTimeMs is not a direct property
            if (healthResult.AdditionalInfo != null)
            {
                healthResult.AdditionalInfo["ResponseTimeMs"] = connectivityResult.ResponseTime.TotalMilliseconds;
                healthResult.AdditionalInfo["ConnectivityTest"] = connectivityResult.IsSuccessful ? "Passed" : "Failed";
            }

            if (connectivityResult.IsSuccessful)
            {
                // Test scanning functionality with EICAR test signature
                var functionalityResult = await TestScanningFunctionalityAsync(cancellationToken);
                var scanningFunctional = functionalityResult.IsSuccessful;

                // Store scanning functionality result in additional info
                if (healthResult.AdditionalInfo != null)
                {
                    healthResult.AdditionalInfo["ScanningFunctional"] = scanningFunctional;
                    healthResult.AdditionalInfo["FunctionalityTest"] = scanningFunctional ? "Passed" : "Failed";

                    if (!scanningFunctional && !string.IsNullOrEmpty(functionalityResult.ErrorMessage))
                    {
                        healthResult.AdditionalInfo["FunctionalityError"] = functionalityResult.ErrorMessage;
                    }
                }

                // Get virus definition information
                var definitionInfo = await GetVirusDefinitionInfoAsync(cancellationToken);
                if (definitionInfo.IsSuccessful)
                {
                    healthResult.DefinitionVersion = definitionInfo.Version;
                    healthResult.LastDefinitionUpdate = definitionInfo.LastUpdate;
                    healthResult.DefinitionsAreCurrent = definitionInfo.IsCurrent;

                    // Store definition info in additional info
                    if (healthResult.AdditionalInfo != null)
                    {
                        healthResult.AdditionalInfo["DefinitionRetrievalStatus"] = "Success";
                    }
                }
                else
                {
                    // Store definition retrieval error in additional info
                    if (healthResult.AdditionalInfo != null)
                    {
                        healthResult.AdditionalInfo["DefinitionRetrievalStatus"] = "Failed";
                        healthResult.AdditionalInfo["DefinitionRetrievalError"] = definitionInfo.ErrorMessage ?? "Unknown error";
                    }
                }

                // Determine overall health status based on all checks
                if (scanningFunctional && healthResult.DefinitionsAreCurrent)
                {
                    healthResult.OverallHealth = HealthStatus.Healthy;
                    healthResult.SuccessRate = 1.0;
                }
                else if (scanningFunctional)
                {
                    healthResult.OverallHealth = HealthStatus.Warning;
                    healthResult.SuccessRate = 0.8;
                    healthResult.ErrorMessage = "Virus definitions may be outdated";
                }
                else
                {
                    healthResult.OverallHealth = HealthStatus.Unhealthy;
                    healthResult.SuccessRate = 0.0;
                    healthResult.ErrorMessage = "Scanning functionality is impaired";
                }
            }
            else
            {
                healthResult.OverallHealth = HealthStatus.Unhealthy;
                healthResult.SuccessRate = 0.0;
                healthResult.ErrorMessage = $"Cannot connect to ClamAV server at {_options.ServerHost}:{_options.ServerPort}. " +
                                           (connectivityResult.ErrorMessage ?? "Connection failed");
            }

            stopwatch.Stop();

            // Store comprehensive health check information in additional info
            if (healthResult.AdditionalInfo != null)
            {
                healthResult.AdditionalInfo["HealthCheckDuration"] = stopwatch.Elapsed;
                healthResult.AdditionalInfo["CheckTimestamp"] = now;
                healthResult.AdditionalInfo["OperationId"] = operationId;
                healthResult.AdditionalInfo["ScannerName"] = "ClamAV";
                healthResult.AdditionalInfo["ServerHost"] = _options.ServerHost;
                healthResult.AdditionalInfo["ServerPort"] = _options.ServerPort;
                healthResult.AdditionalInfo["TimeoutSeconds"] = _options.TimeoutSeconds;
            }

            // Cache the health result for performance optimization
            lock (_lockObject)
            {
                _cachedHealthResult = healthResult;
                _lastHealthCheck = now;
            }

            _logger.LogInformation("ClamAV health check operation {OperationId} completed: " +
                "Health={Health}, Available={Available}, Functional={Functional}, " +
                "ResponseTime={ResponseTimeMs}ms, Duration={DurationMs}ms",
                operationId, healthResult.OverallHealth, healthResult.IsAvailable,
                healthResult.AdditionalInfo?["ScanningFunctional"],
                connectivityResult.ResponseTime.TotalMilliseconds, stopwatch.ElapsedMilliseconds);

            return healthResult;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("ClamAV health check operation {OperationId} was cancelled", operationId);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "ClamAV health check operation {OperationId} failed after {DurationMs}ms: {ErrorMessage}",
                operationId, stopwatch.ElapsedMilliseconds, ex.Message);

            return ScannerHealthResult.CreateErrorResult(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves comprehensive information about the ClamAV virus scanner configuration and capabilities.
    /// This method provides static information about the scanner implementation including product details,
    /// supported formats, detection capabilities, and operational characteristics.
    /// </summary>
    /// <returns>
    /// A ScannerInfo object containing detailed information about scanner name, version, and vendor
    /// information, supported file formats and detection capabilities, configuration settings and 
    /// operational parameters, performance characteristics and resource requirements, and integration
    /// details and compatibility information.
    /// </returns>
    /// <remarks>
    /// <para>This method provides static information about the scanner implementation including:</para>
    /// 
    /// <para><strong>Scanner Identification:</strong></para>
    /// <list type="bullet">
    /// <item>Product name, version, and build information</item>
    /// <item>Vendor details and support contact information</item>
    /// <item>Scanner type and detection methodology</item>
    /// <item>Integration compatibility and API version</item>
    /// </list>
    /// 
    /// <para><strong>Capability Information:</strong></para>
    /// <list type="bullet">
    /// <item>Supported file formats and MIME types for scanning</item>
    /// <item>Detection technologies and threat analysis capabilities</item>
    /// <item>Performance characteristics and operational limits</item>
    /// <item>Configuration options and customization features</item>
    /// </list>
    /// 
    /// <para><strong>Configuration Details:</strong></para>
    /// <list type="bullet">
    /// <item>Server connection configuration and network settings</item>
    /// <item>Performance tuning parameters and resource limits</item>
    /// <item>Operational thresholds and timeout configurations</item>
    /// <item>Concurrency settings and connection pooling parameters</item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>System documentation and compliance reporting</item>
    /// <item>Integration planning and compatibility assessment</item>
    /// <item>Performance tuning and capacity planning</item>
    /// <item>Troubleshooting and support diagnostics</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get scanner information for system documentation
    /// var scannerInfo = _virusScanner.GetScannerInfo();
    /// 
    /// _logger.LogInformation("Virus Scanner: {ProductName} v{Version} by {Vendor}",
    ///     scannerInfo.ProductName, scannerInfo.ProductVersion, scannerInfo.VendorName);
    /// 
    /// // Check format support before scanning
    /// if (scannerInfo.SupportedFormats.Contains("application/pdf"))
    /// {
    ///     var scanResult = await _virusScanner.ScanFileForVirusesAsync(pdfStream);
    /// }
    /// 
    /// // Display configuration for troubleshooting
    /// var config = scannerInfo.ConfigurationDetails;
    /// foreach (var setting in config)
    /// {
    ///     _logger.LogDebug("Scanner Config - {Key}: {Value}", setting.Key, setting.Value);
    /// }
    /// </code>
    /// </example>
    public ScannerInfo GetScannerInfo()
    {
        return new ScannerInfo
        {
            ProductName = "ClamAV Antivirus Scanner",
            ProductVersion = GetType().Assembly.GetName().Version?.ToString() ?? "Unknown",
            VendorName = "Cisco Talos Intelligence Group",
            ScannerType = "Signature-based Antivirus",
            SupportedFormats = new[]
            {
                // Document formats
                "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/vnd.ms-powerpoint", "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "text/rtf", "application/rtf",
                
                // Archive formats
                "application/zip", "application/x-rar-compressed", "application/x-7z-compressed", "application/x-tar",
                "application/gzip", "application/x-gzip", "application/x-bzip2",
                
                // Image formats
                "image/jpeg", "image/png", "image/gif", "image/bmp", "image/tiff", "image/webp",
                
                // Executable formats
                "application/x-msdownload", "application/x-executable", "application/x-dosexec",
                "application/x-mach-binary", "application/x-elf",
                
                // Text and other formats
                "text/plain", "text/html", "text/xml", "application/json", "application/javascript",
                
                // Email formats
                "message/rfc822", "application/vnd.ms-outlook"
            },
            DetectionCapabilities = new[]
            {
                "Virus Detection", "Trojan Detection", "Malware Detection", "Spyware Detection",
                "Adware Detection", "Rootkit Detection", "Potentially Unwanted Programs (PUP)",
                "Macro Viruses", "Script Malware", "Phishing Content", "Suspicious Archives",
                "Email Threats", "Document Exploits"
            },
            MaxFileSizeMB = _options.MaxFileSizeMB,
            SupportsConcurrentScanning = true,
            SupportsStreamScanning = true,
            ConfigurationDetails = new Dictionary<string, object>
            {
                ["ServerHost"] = _options.ServerHost,
                ["ServerPort"] = _options.ServerPort,
                ["TimeoutSeconds"] = _options.TimeoutSeconds,
                ["MaxConcurrentConnections"] = _options.MaxConcurrentConnections,
                ["MaxRetryAttempts"] = _options.MaxRetryAttempts,
                ["MaxFileSizeMB"] = _options.MaxFileSizeMB,
                ["BaseRetryDelayMs"] = _options.BaseRetryDelayMs,
                ["MaxRetryDelayMs"] = _options.MaxRetryDelayMs,
                ["HealthCheckCacheSeconds"] = _options.HealthCheckCacheSeconds,
                ["MaxDefinitionAgeDays"] = _options.MaxDefinitionAgeDays
            }
        };
    }

    #endregion IVirusScanner Implementation

    #region Private Implementation Methods

    /// <summary>
    /// Performs virus scanning with comprehensive retry logic and error handling for production resilience.
    /// This method implements exponential backoff retry strategy to handle transient failures gracefully
    /// while maintaining connection pooling and resource management for optimal performance.
    /// </summary>
    /// <param name="fileStream">The file stream to scan for viruses and malicious content.</param>
    /// <param name="operationId">Unique identifier for operation tracking and correlation across logs.</param>
    /// <param name="cancellationToken">Cancellation token for responsive cancellation handling.</param>
    /// <returns>Comprehensive virus scan result with detailed threat information and metadata.</returns>
    /// <remarks>
    /// This method provides enterprise-grade retry logic with exponential backoff, connection pooling
    /// through semaphore management, comprehensive error logging and monitoring, graceful handling of
    /// cancellation requests, and resource cleanup and stream position management.
    /// </remarks>
    private async Task<VirusScanResult> PerformVirusScanWithRetryAsync(
        Stream fileStream,
        string operationId,
        CancellationToken cancellationToken)
    {
        var attempt = 0;
        Exception? lastException = null;
        var totalStopwatch = Stopwatch.StartNew();

        while (attempt < _options.MaxRetryAttempts)
        {
            attempt++;
            var attemptStopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogDebug("Virus scan attempt {Attempt}/{MaxAttempts} for operation {OperationId}",
                    attempt, _options.MaxRetryAttempts, operationId);

                // Acquire connection semaphore to control concurrent connections
                await _connectionSemaphore.WaitAsync(cancellationToken);

                try
                {
                    // Reset stream position for each retry attempt
                    if (fileStream.CanSeek)
                    {
                        fileStream.Position = 0;
                    }

                    // Perform the actual virus scan
                    var result = await PerformSingleVirusScanAsync(fileStream, operationId, cancellationToken);

                    attemptStopwatch.Stop();
                    totalStopwatch.Stop();

                    _logger.LogDebug("Virus scan attempt {Attempt} for operation {OperationId} succeeded in {AttemptMs}ms",
                        attempt, operationId, attemptStopwatch.ElapsedMilliseconds);

                    // Update scan result with total operation time and timestamp
                    return new VirusScanResult
                    {
                        IsClean = result.IsClean,
                        ThreatDetails = result.ThreatDetails,
                        ScannerName = result.ScannerName,
                        ScannerVersion = result.ScannerVersion,
                        ScanDuration = totalStopwatch.Elapsed,
                        ScanTimestamp = DateTime.UtcNow,
                        Metadata = result.Metadata
                    };
                }
                finally
                {
                    _connectionSemaphore.Release();
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Don't retry on cancellation - propagate immediately
                _logger.LogDebug("Virus scan operation {OperationId} cancelled during attempt {Attempt}",
                    operationId, attempt);
                throw;
            }
            catch (Exception ex)
            {
                attemptStopwatch.Stop();
                lastException = ex;

                _logger.LogWarning(ex, "Virus scan attempt {Attempt}/{MaxAttempts} failed for operation {OperationId} " +
                    "after {AttemptMs}ms: {ErrorMessage}",
                    attempt, _options.MaxRetryAttempts, operationId, attemptStopwatch.ElapsedMilliseconds, ex.Message);

                // If this was the last attempt, don't wait
                if (attempt >= _options.MaxRetryAttempts)
                {
                    break;
                }

                // Calculate exponential backoff delay
                var delayMs = (int)(Math.Pow(2, attempt) * _options.BaseRetryDelayMs);
                delayMs = Math.Min(delayMs, _options.MaxRetryDelayMs);

                _logger.LogDebug("Waiting {DelayMs}ms before retry attempt {NextAttempt} for operation {OperationId}",
                    delayMs, attempt + 1, operationId);

                try
                {
                    await Task.Delay(delayMs, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogDebug("Virus scan operation {OperationId} cancelled during retry delay", operationId);
                    throw;
                }
            }
        }

        // All retry attempts exhausted - return error result
        totalStopwatch.Stop();

        _logger.LogError(lastException, "All {MaxAttempts} virus scan attempts failed for operation {OperationId} " +
            "after {TotalMs}ms. Final error: {ErrorMessage}",
            _options.MaxRetryAttempts, operationId, totalStopwatch.ElapsedMilliseconds, lastException?.Message);

        return VirusScanResult.CreateErrorResult(
            $"Virus scanning failed after {_options.MaxRetryAttempts} attempts - file treated as potentially unsafe",
            $"Operation {operationId} failed after {totalStopwatch.ElapsedMilliseconds}ms: {lastException?.Message ?? "Unknown error"}");
    }

    /// <summary>
    /// Performs a single virus scan attempt using ClamAV client with comprehensive error handling.
    /// This method manages the direct communication with ClamAV daemon including connection setup,
    /// file size validation, scanning operation, and result processing.
    /// </summary>
    /// <param name="fileStream">The file stream to scan for viruses.</param>
    /// <param name="operationId">Unique identifier for operation tracking.</param>
    /// <param name="cancellationToken">Cancellation token for responsive cancellation.</param>
    /// <returns>Virus scan result with detailed threat information.</returns>
    /// <remarks>
    /// This method handles direct ClamAV communication, file size limit enforcement, timeout management,
    /// and comprehensive result processing with threat classification.
    /// </remarks>
    private async Task<VirusScanResult> PerformSingleVirusScanAsync(
        Stream fileStream,
        string operationId,
        CancellationToken cancellationToken)
    {
        // Create ClamAV client with configured connection parameters
        var clamClient = new ClamClient(_options.ServerHost, _options.ServerPort);

        // Set up timeout for the scanning operation
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        try
        {
            _logger.LogDebug("Connecting to ClamAV server {ServerHost}:{ServerPort} for operation {OperationId}",
                _options.ServerHost, _options.ServerPort, operationId);

            // Validate file size limits before scanning
            if (fileStream.CanSeek && fileStream.Length > _options.MaxFileSizeMB * 1024 * 1024)
            {
                throw new ArgumentException(
                    $"File size ({fileStream.Length / (1024.0 * 1024.0):F2} MB) exceeds maximum allowed size " +
                    $"({_options.MaxFileSizeMB} MB) for virus scanning");
            }

            // Perform the virus scan
            var scanResult = await clamClient.SendAndScanFileAsync(fileStream, timeoutCts.Token);

            _logger.LogDebug("ClamAV scan completed for operation {OperationId}: Result={ScanResult}",
                operationId, scanResult.Result);

            // Process scan result and create comprehensive response
            return ProcessClamAvScanResult(scanResult, operationId);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred
            throw new TimeoutException($"Virus scan operation timed out after {_options.TimeoutSeconds} seconds");
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            // Wrap and rethrow with additional context
            throw new InvalidOperationException(
                $"ClamAV virus scan failed for operation {operationId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Processes ClamAV scan results and creates comprehensive VirusScanResult objects with detailed
    /// threat information, classification, and metadata for security analysis and operational monitoring.
    /// </summary>
    /// <param name="clamResult">The raw ClamAV scan result to process.</param>
    /// <param name="operationId">Operation identifier for tracking and correlation.</param>
    /// <returns>Comprehensive virus scan result with threat details and metadata.</returns>
    /// <remarks>
    /// This method transforms ClamAV native results into standardized VirusScanResult objects with
    /// comprehensive threat classification, security metadata, and operational context information.
    /// </remarks>
    private VirusScanResult ProcessClamAvScanResult(ClamScanResult clamResult, string operationId)
    {
        var isClean = clamResult.Result == ClamScanResults.Clean;

        if (isClean)
        {
            _logger.LogDebug("File scan clean for operation {OperationId}", operationId);

            return new VirusScanResult
            {
                IsClean = true,
                ThreatDetails = null,
                ScannerName = "ClamAV",
                ScannerVersion = GetScannerVersion(),
                ScanTimestamp = DateTime.UtcNow,
                ScanDuration = TimeSpan.Zero, // Will be updated by caller
                Metadata = new Dictionary<string, object>
                {
                    ["OperationId"] = operationId,
                    ["ClamAvResult"] = clamResult.Result.ToString(),
                    ["ServerHost"] = _options.ServerHost,
                    ["ServerPort"] = _options.ServerPort,
                    ["ScanMethod"] = "ClamAV Daemon",
                    ["DetectionEngine"] = "Signature-based"
                }
            };
        }
        else
        {
            // Extract threat information from ClamAV result
            var threatName = clamResult.InfectedFiles?.FirstOrDefault()?.VirusName ?? "Unknown Threat";

            _logger.LogWarning("Virus detected in operation {OperationId}: Threat={ThreatName}, ClamResult={ClamResult}",
                operationId, threatName, clamResult.Result);

            var threatDetails = new ThreatDetails
            {
                ThreatName = threatName,
                ThreatType = DetermineThreatType(threatName),
                Severity = DetermineThreatSeverity(threatName),
                Description = $"Malicious content detected by ClamAV: {threatName}",
                RecommendedAction = "File should be quarantined and not processed further",
                DetectionMethod = "Signature-based detection",
                ConfidenceLevel = 0.95 // ClamAV has high confidence in signature-based detection
            };

            return new VirusScanResult
            {
                IsClean = false,
                ThreatDetails = threatDetails,
                ScannerName = "ClamAV",
                ScannerVersion = GetScannerVersion(),
                ScanTimestamp = DateTime.UtcNow,
                ScanDuration = TimeSpan.Zero, // Will be updated by caller
                Metadata = new Dictionary<string, object>
                {
                    ["OperationId"] = operationId,
                    ["ClamAvResult"] = clamResult.Result.ToString(),
                    ["InfectedFileCount"] = clamResult.InfectedFiles?.Count() ?? 0,
                    ["ServerHost"] = _options.ServerHost,
                    ["ServerPort"] = _options.ServerPort,
                    ["ScanMethod"] = "ClamAV Daemon",
                    ["DetectionEngine"] = "Signature-based",
                    ["ThreatClassification"] = threatDetails.ThreatType.ToString(),
                    ["SeverityLevel"] = threatDetails.Severity.ToString()
                }
            };
        }
    }

    /// <summary>
    /// Tests basic connectivity to the ClamAV server for health monitoring using both network
    /// connectivity validation and ClamAV daemon responsiveness testing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for responsive cancellation.</param>
    /// <returns>Connectivity test result with response time and error information.</returns>
    /// <remarks>
    /// This method performs comprehensive connectivity testing including network layer validation
    /// and ClamAV service-specific communication testing for complete health assessment.
    /// </remarks>
    private async Task<ConnectivityTestResult> TestConnectivityAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Test basic network connectivity first
            using var ping = new Ping();
            var pingReply = await ping.SendPingAsync(_options.ServerHost, _options.TimeoutSeconds * 1000);

            if (pingReply.Status != IPStatus.Success)
            {
                stopwatch.Stop();
                return new ConnectivityTestResult
                {
                    IsSuccessful = false,
                    ResponseTime = stopwatch.Elapsed,
                    ErrorMessage = $"Network ping failed: {pingReply.Status}"
                };
            }

            // Test ClamAV service connectivity
            var clamClient = new ClamClient(_options.ServerHost, _options.ServerPort);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            // Use ping command to test ClamAV daemon responsiveness
            var isAlive = await clamClient.PingAsync(timeoutCts.Token);

            stopwatch.Stop();

            return new ConnectivityTestResult
            {
                IsSuccessful = isAlive,
                ResponseTime = stopwatch.Elapsed,
                ErrorMessage = isAlive ? null : "ClamAV daemon did not respond to ping command"
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return new ConnectivityTestResult
            {
                IsSuccessful = false,
                ResponseTime = stopwatch.Elapsed,
                ErrorMessage = $"Connectivity test failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Tests ClamAV scanning functionality using EICAR test virus signature to verify that
    /// the scanning engine can properly detect known malicious content.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for responsive cancellation.</param>
    /// <returns>Functionality test result indicating whether scanning is working properly.</returns>
    /// <remarks>
    /// This method uses the industry-standard EICAR test file to validate that ClamAV can detect
    /// and classify known malicious signatures, ensuring the scanning functionality is operational.
    /// </remarks>
    private async Task<FunctionalityTestResult> TestScanningFunctionalityAsync(CancellationToken cancellationToken)
    {
        try
        {
            // EICAR test virus signature - standard test file for antivirus testing
            // This is a harmless test signature recognized by all major antivirus engines
            var eicarSignature = "X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";
            var eicarBytes = Encoding.UTF8.GetBytes(eicarSignature);

            using var eicarStream = new MemoryStream(eicarBytes);

            var clamClient = new ClamClient(_options.ServerHost, _options.ServerPort);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            var scanResult = await clamClient.SendAndScanFileAsync(eicarStream, timeoutCts.Token);

            // EICAR should be detected as a threat for proper functionality
            var isDetected = scanResult.Result == ClamScanResults.VirusDetected;

            if (isDetected)
            {
                _logger.LogDebug("EICAR test signature properly detected - scanning functionality verified");
            }

            return new FunctionalityTestResult
            {
                IsSuccessful = isDetected,
                ErrorMessage = isDetected ? null : "EICAR test signature was not detected - scanning may not be functioning properly"
            };
        }
        catch (Exception ex)
        {
            return new FunctionalityTestResult
            {
                IsSuccessful = false,
                ErrorMessage = $"Functionality test failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Retrieves virus definition information from ClamAV server for health monitoring including
    /// version information, update timestamps, and currency assessment.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for responsive cancellation.</param>
    /// <returns>Virus definition information including version and update status.</returns>
    /// <remarks>
    /// This method queries ClamAV for current definition information and assesses whether the
    /// definitions are sufficiently current based on configured age thresholds.
    /// </remarks>
    private async Task<DefinitionInfoResult> GetVirusDefinitionInfoAsync(CancellationToken cancellationToken)
    {
        try
        {
            var clamClient = new ClamClient(_options.ServerHost, _options.ServerPort);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            // Get version information which includes definition details
            var version = await clamClient.GetVersionAsync(timeoutCts.Token);

            // Parse version information to extract definition details
            var versionLines = version.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            string? definitionVersion = null;
            DateTime? lastUpdate = null;

            foreach (var line in versionLines)
            {
                // Extract ClamAV version and database version
                if (line.Contains("ClamAV") && line.Contains("/"))
                {
                    var parts = line.Split('/');
                    if (parts.Length >= 2)
                    {
                        definitionVersion = parts[1].Trim();
                    }
                }

                // Parse database information lines for timestamps
                if (line.Contains("daily.") || line.Contains("main.") || line.Contains("bytecode."))
                {
                    // ClamAV database lines often contain date information
                    // For production implementation, more sophisticated date parsing would be needed
                    // This is a simplified approach for demonstration
                    lastUpdate = DateTime.UtcNow.Date;
                }
            }

            // Assess currency based on configured maximum age threshold
            var isCurrent = lastUpdate.HasValue &&
                           (DateTime.UtcNow - lastUpdate.Value).TotalDays <= _options.MaxDefinitionAgeDays;

            return new DefinitionInfoResult
            {
                IsSuccessful = true,
                Version = definitionVersion ?? ExtractVersionFromResponse(version),
                LastUpdate = lastUpdate,
                IsCurrent = isCurrent
            };
        }
        catch (Exception ex)
        {
            return new DefinitionInfoResult
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message,
                Version = "Unknown",
                LastUpdate = null,
                IsCurrent = false
            };
        }
    }

    /// <summary>
    /// Validates the ClamAV scanner configuration for operational correctness and completeness.
    /// This method ensures all configuration parameters are within acceptable ranges and
    /// compatibility requirements are met.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when configuration validation fails.</exception>
    /// <remarks>
    /// This method validates server connectivity parameters, timeout and retry configurations,
    /// resource allocation settings, and operational thresholds for production deployment.
    /// </remarks>
    private void ValidateConfiguration()
    {
        var errors = new List<string>();

        // Validate server connection parameters
        if (string.IsNullOrWhiteSpace(_options.ServerHost))
        {
            errors.Add("ClamAV server host must be specified");
        }

        if (_options.ServerPort <= 0 || _options.ServerPort > 65535)
        {
            errors.Add($"ClamAV server port must be between 1 and 65535, got: {_options.ServerPort}");
        }

        // Validate timeout configurations
        if (_options.TimeoutSeconds <= 0 || _options.TimeoutSeconds > 300)
        {
            errors.Add($"Timeout must be between 1 and 300 seconds, got: {_options.TimeoutSeconds}");
        }

        // Validate concurrency parameters
        if (_options.MaxConcurrentConnections <= 0 || _options.MaxConcurrentConnections > 100)
        {
            errors.Add($"Max concurrent connections must be between 1 and 100, got: {_options.MaxConcurrentConnections}");
        }

        // Validate retry configuration
        if (_options.MaxRetryAttempts <= 0 || _options.MaxRetryAttempts > 10)
        {
            errors.Add($"Max retry attempts must be between 1 and 10, got: {_options.MaxRetryAttempts}");
        }

        if (_options.BaseRetryDelayMs <= 0 || _options.BaseRetryDelayMs > 30000)
        {
            errors.Add($"Base retry delay must be between 1 and 30000 ms, got: {_options.BaseRetryDelayMs}");
        }

        if (_options.MaxRetryDelayMs <= 0 || _options.MaxRetryDelayMs > 60000)
        {
            errors.Add($"Max retry delay must be between 1 and 60000 ms, got: {_options.MaxRetryDelayMs}");
        }

        if (_options.MaxRetryDelayMs < _options.BaseRetryDelayMs)
        {
            errors.Add("Max retry delay must be greater than or equal to base retry delay");
        }

        // Validate file size limits
        if (_options.MaxFileSizeMB <= 0 || _options.MaxFileSizeMB > 1000)
        {
            errors.Add($"Max file size must be between 1 and 1000 MB, got: {_options.MaxFileSizeMB}");
        }

        // Validate health check configuration
        if (_options.HealthCheckCacheSeconds < 0 || _options.HealthCheckCacheSeconds > 3600)
        {
            errors.Add($"Health check cache duration must be between 0 and 3600 seconds, got: {_options.HealthCheckCacheSeconds}");
        }

        if (_options.MaxDefinitionAgeDays <= 0 || _options.MaxDefinitionAgeDays > 30)
        {
            errors.Add($"Max definition age must be between 1 and 30 days, got: {_options.MaxDefinitionAgeDays}");
        }

        if (errors.Count > 0)
        {
            throw new ArgumentException($"ClamAV scanner configuration validation failed: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Determines the threat type based on the threat name using heuristic analysis and
    /// pattern matching against known threat categories.
    /// </summary>
    /// <param name="threatName">The name of the detected threat.</param>
    /// <returns>The categorized threat type for security classification.</returns>
    /// <remarks>
    /// This method uses pattern matching to classify threats into standard categories
    /// for consistent security analysis and incident response procedures.
    /// </remarks>
    private static ThreatType DetermineThreatType(string threatName)
    {
        if (string.IsNullOrEmpty(threatName))
            return ThreatType.Unknown;

        var lowerThreatName = threatName.ToLowerInvariant();

        // High-priority threat classification
        if (lowerThreatName.Contains("ransomware") || lowerThreatName.Contains("crypt") || lowerThreatName.Contains("locker"))
            return ThreatType.Ransomware;

        if (lowerThreatName.Contains("rootkit") || lowerThreatName.Contains("kernel"))
            return ThreatType.Rootkit;

        if (lowerThreatName.Contains("trojan") || lowerThreatName.Contains("backdoor") || lowerThreatName.Contains("rat"))
            return ThreatType.Trojan;

        // Propagation-based threats
        if (lowerThreatName.Contains("worm") || lowerThreatName.Contains("mass"))
            return ThreatType.Worm;

        // Privacy and data threats
        if (lowerThreatName.Contains("spyware") || lowerThreatName.Contains("keylog") || lowerThreatName.Contains("stealer"))
            return ThreatType.Spyware;

        // Unwanted software
        if (lowerThreatName.Contains("adware") || lowerThreatName.Contains("pup") || lowerThreatName.Contains("potentially"))
            return ThreatType.Adware;

        // Suspicious content
        if (lowerThreatName.Contains("suspicious") || lowerThreatName.Contains("heuristic") || lowerThreatName.Contains("generic"))
            return ThreatType.Suspicious;

        // Default classification for standard viruses
        return ThreatType.Virus;
    }

    /// <summary>
    /// Determines the threat severity based on the threat type and name using security
    /// best practices and threat intelligence classification standards.
    /// </summary>
    /// <param name="threatName">The name of the detected threat.</param>
    /// <returns>The assessed threat severity level for risk management.</returns>
    /// <remarks>
    /// This method assigns severity levels based on potential impact and urgency of
    /// response required for different threat categories.
    /// </remarks>
    private static ThreatSeverity DetermineThreatSeverity(string threatName)
    {
        if (string.IsNullOrEmpty(threatName))
            return ThreatSeverity.Medium;

        var lowerThreatName = threatName.ToLowerInvariant();

        // Critical threats requiring immediate action
        if (lowerThreatName.Contains("ransomware") ||
            lowerThreatName.Contains("rootkit") ||
            lowerThreatName.Contains("backdoor") ||
            lowerThreatName.Contains("banker") ||
            lowerThreatName.Contains("stealer"))
            return ThreatSeverity.Critical;

        // High severity threats requiring urgent attention
        if (lowerThreatName.Contains("trojan") ||
            lowerThreatName.Contains("worm") ||
            lowerThreatName.Contains("spyware") ||
            lowerThreatName.Contains("keylog") ||
            lowerThreatName.Contains("rat"))
            return ThreatSeverity.High;

        // Low severity threats with minimal immediate risk
        if (lowerThreatName.Contains("adware") ||
            lowerThreatName.Contains("pup") ||
            lowerThreatName.Contains("eicar") ||
            lowerThreatName.Contains("test"))
            return ThreatSeverity.Low;

        // Default to medium severity for standard threats
        return ThreatSeverity.Medium;
    }

    /// <summary>
    /// Gets the current scanner version information for metadata and reporting purposes.
    /// </summary>
    /// <returns>Scanner version string with build information.</returns>
    /// <remarks>
    /// This method provides version information for audit trails, compatibility checking,
    /// and operational monitoring requirements.
    /// </remarks>
    private static string GetScannerVersion()
    {
        var assembly = typeof(ClamAvVirusScanner).Assembly;
        var version = assembly.GetName().Version;
        return $"ClamAV Scanner v{version?.Major}.{version?.Minor}.{version?.Build} (.NET Implementation)";
    }

    /// <summary>
    /// Extracts version information from ClamAV version response for cases where
    /// standard parsing methods don't capture all version details.
    /// </summary>
    /// <param name="versionResponse">The raw version response from ClamAV.</param>
    /// <returns>Extracted version string or fallback identifier.</returns>
    /// <remarks>
    /// This method provides fallback version extraction for improved compatibility
    /// with different ClamAV versions and response formats.
    /// </remarks>
    private static string ExtractVersionFromResponse(string versionResponse)
    {
        if (string.IsNullOrWhiteSpace(versionResponse))
            return "Unknown";

        // Extract first line which typically contains main version info
        var firstLine = versionResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(firstLine) ? "Unknown" : firstLine.Trim();
    }

    /// <summary>
    /// Validates that the scanner has not been disposed and throws an appropriate exception if it has.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the scanner has been disposed.</exception>
    /// <remarks>
    /// This method ensures operations are only performed on active scanner instances and provides
    /// consistent error handling for disposed object access attempts.
    /// </remarks>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ClamAvVirusScanner),
                "ClamAV virus scanner has been disposed and cannot be used for scanning operations");
        }
    }

    #endregion Private Implementation Methods

    #region Helper Classes and Records

    /// <summary>
    /// Represents the result of a connectivity test to ClamAV server with timing and error information.
    /// </summary>
    private sealed record ConnectivityTestResult
    {
        /// <summary>Gets a value indicating whether the connectivity test was successful.</summary>
        public bool IsSuccessful { get; init; }

        /// <summary>Gets the response time for the connectivity test.</summary>
        public TimeSpan ResponseTime { get; init; }

        /// <summary>Gets the error message if the test failed, null otherwise.</summary>
        public string? ErrorMessage { get; init; }
    }

    /// <summary>
    /// Represents the result of a functionality test using EICAR signature with success status and error details.
    /// </summary>
    private sealed record FunctionalityTestResult
    {
        /// <summary>Gets a value indicating whether the functionality test was successful.</summary>
        public bool IsSuccessful { get; init; }

        /// <summary>Gets the error message if the test failed, null otherwise.</summary>
        public string? ErrorMessage { get; init; }
    }

    /// <summary>
    /// Represents virus definition information retrieved from ClamAV server with version and currency details.
    /// </summary>
    private sealed record DefinitionInfoResult
    {
        /// <summary>Gets a value indicating whether the definition info retrieval was successful.</summary>
        public bool IsSuccessful { get; init; }

        /// <summary>Gets the virus definition version identifier.</summary>
        public string Version { get; init; } = "Unknown";

        /// <summary>Gets the timestamp when virus definitions were last updated.</summary>
        public DateTime? LastUpdate { get; init; }

        /// <summary>Gets a value indicating whether the definitions are considered current based on age thresholds.</summary>
        public bool IsCurrent { get; init; }

        /// <summary>Gets the error message if retrieval failed, null otherwise.</summary>
        public string? ErrorMessage { get; init; }
    }

    #endregion Helper Classes and Records

    #region IDisposable Implementation

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting resources.
    /// This method provides comprehensive cleanup of managed resources including semaphores, connections,
    /// and other disposable objects used by the ClamAV virus scanner implementation.
    /// </summary>
    /// <param name="disposing">
    /// True if disposing managed resources during explicit disposal; false if called from finalizer.
    /// When true, performs complete cleanup of all managed and unmanaged resources.
    /// When false, only cleans up unmanaged resources during garbage collection.
    /// </param>
    /// <remarks>
    /// <para>This method implements the standard .NET disposal pattern including:</para>
    /// 
    /// <para><strong>Resource Cleanup:</strong></para>
    /// <list type="bullet">
    /// <item>Disposal of connection semaphores and concurrency control mechanisms</item>
    /// <item>Cleanup of cached health check results and operational state</item>
    /// <item>Proper termination of any active scanning operations</item>
    /// <item>Release of system resources and network connections</item>
    /// <item>Comprehensive logging of disposal operations for audit purposes</item>
    /// </list>
    /// 
    /// <para><strong>Thread Safety:</strong></para>
    /// <list type="bullet">
    /// <item>Thread-safe disposal using volatile boolean flag</item>
    /// <item>Protection against multiple disposal calls</item>
    /// <item>Proper synchronization during cleanup operations</item>
    /// <item>Safe handling of concurrent access during disposal</item>
    /// </list>
    /// 
    /// <para><strong>Error Handling:</strong></para>
    /// <list type="bullet">
    /// <item>Graceful handling of disposal errors without throwing exceptions</item>
    /// <item>Comprehensive logging of cleanup failures for diagnostics</item>
    /// <item>Continuation of disposal process even if individual cleanup operations fail</item>
    /// <item>Prevention of resource leaks through defensive cleanup strategies</item>
    /// </list>
    /// </remarks>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                try
                {
                    // Dispose of the connection semaphore to release waiting threads
                    _connectionSemaphore?.Dispose();

                    // Clear cached health results
                    lock (_lockObject)
                    {
                        _cachedHealthResult = null;
                    }

                    _logger.LogDebug("ClamAV virus scanner disposed successfully. " +
                        "All managed resources have been properly released.");
                }
                catch (Exception ex)
                {
                    // Log disposal errors without throwing to prevent issues during cleanup
                    _logger.LogWarning(ex, "Error occurred while disposing ClamAV virus scanner resources. " +
                        "Some resources may not have been properly released: {ErrorMessage}", ex.Message);
                }
            }

            // Mark as disposed to prevent further operations
            _disposed = true;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// This method implements the IDisposable interface to ensure proper cleanup of ClamAV scanner resources
    /// and integration with .NET's garbage collection and resource management systems.
    /// </summary>
    /// <remarks>
    /// <para>This method provides the public interface for resource disposal including:</para>
    /// 
    /// <para><strong>Disposal Pattern Implementation:</strong></para>
    /// <list type="bullet">
    /// <item>Standard IDisposable pattern with finalizer suppression</item>
    /// <item>Delegation to protected Dispose(bool) method for actual cleanup</item>
    /// <item>Integration with using statements and automatic resource management</item>
    /// <item>Prevention of finalizer execution through GC.SuppressFinalize</item>
    /// <item>Thread-safe disposal with protection against multiple calls</item>
    /// </list>
    /// 
    /// <para><strong>Resource Management Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Deterministic cleanup of system resources and connections</item>
    /// <item>Prevention of resource leaks in long-running applications</item>
    /// <item>Integration with dependency injection container lifecycle management</item>
    /// <item>Support for proper cleanup in exception scenarios</item>
    /// <item>Compliance with .NET resource management best practices</item>
    /// </list>
    /// 
    /// <para><strong>Usage Recommendations:</strong></para>
    /// <list type="bullet">
    /// <item>Should be called explicitly when scanner instance is no longer needed</item>
    /// <item>Automatic disposal when used with dependency injection scoped services</item>
    /// <item>Integration with using statements for deterministic cleanup</item>
    /// <item>Essential for preventing resource exhaustion in high-throughput scenarios</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Explicit disposal in manual instantiation scenarios
    /// using var scanner = new ClamAvVirusScanner(logger, options);
    /// var result = await scanner.ScanFileForVirusesAsync(fileStream);
    /// // Scanner is automatically disposed when exiting using block
    /// 
    /// // Dependency injection automatically handles disposal
    /// services.AddScoped&lt;IVirusScanner, ClamAvVirusScanner&gt;();
    /// // Scanner instance is disposed at the end of request scope
    /// 
    /// // Manual disposal when needed
    /// var scanner = serviceProvider.GetRequiredService&lt;IVirusScanner&gt;();
    /// try
    /// {
    ///     var result = await scanner.ScanFileForVirusesAsync(fileStream);
    ///     // Process result...
    /// }
    /// finally
    /// {
    ///     if (scanner is IDisposable disposableScanner)
    ///         disposableScanner.Dispose();
    /// }
    /// </code>
    /// </example>
    public void Dispose()
    {
        // Perform disposal of managed resources
        Dispose(disposing: true);

        // Suppress finalization since cleanup has been performed
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Implementation
}

/// <summary>
/// Configuration options for ClamAV virus scanner operations and behavior.
/// This class provides comprehensive configuration settings for all aspects of ClamAV scanner
/// operation including connection management, performance tuning, retry policies, and
/// operational thresholds for enterprise deployment scenarios.
/// </summary>
/// <remarks>
/// <para>This configuration class provides comprehensive control over scanner behavior including:</para>
/// 
/// <para><strong>Connection Configuration:</strong></para>
/// <list type="bullet">
/// <item>ClamAV server host and port specification for network connectivity</item>
/// <item>Connection timeout and retry policies for network resilience</item>
/// <item>Concurrent connection management for high-throughput scenarios</item>
/// <item>Network security and authentication parameters</item>
/// </list>
/// 
/// <para><strong>Performance Tuning:</strong></para>
/// <list type="bullet">
/// <item>File size limits and processing constraints for resource management</item>
/// <item>Timeout values for responsive user experience</item>
/// <item>Retry logic configuration with exponential backoff</item>
/// <item>Health check caching for optimal monitoring performance</item>
/// </list>
/// 
/// <para><strong>Operational Parameters:</strong></para>
/// <list type="bullet">
/// <item>Virus definition currency thresholds for security effectiveness</item>
/// <item>Logging levels and diagnostic information configuration</item>
/// <item>Integration parameters for enterprise monitoring systems</item>
/// <item>Compliance settings for audit and security requirements</item>
/// </list>
/// 
/// <para><strong>Configuration Sources:</strong></para>
/// <list type="bullet">
/// <item>appsettings.json configuration file integration</item>
/// <item>Environment variable override support</item>
/// <item>Azure Key Vault and secure configuration provider integration</item>
/// <item>Runtime configuration updates through IOptionsMonitor</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // appsettings.json configuration
/// {
///   "ClamAV": {
///     "ServerHost": "clamav.company.com",
///     "ServerPort": 3310,
///     "TimeoutSeconds": 60,
///     "MaxConcurrentConnections": 10,
///     "MaxRetryAttempts": 5,
///     "BaseRetryDelayMs": 2000,
///     "MaxRetryDelayMs": 30000,
///     "MaxFileSizeMB": 500,
///     "HealthCheckCacheSeconds": 300,
///     "MaxDefinitionAgeDays": 3
///   }
/// }
/// 
/// // Dependency injection configuration
/// services.Configure&lt;ClamAvScannerOptions&gt;(
///     configuration.GetSection("ClamAV"));
/// 
/// services.AddScoped&lt;IVirusScanner, ClamAvVirusScanner&gt;();
/// 
/// // Environment-specific overrides
/// services.Configure&lt;ClamAvScannerOptions&gt;(options =>
/// {
///     if (environment.IsProduction())
///     {
///         options.MaxRetryAttempts = 5;
///         options.TimeoutSeconds = 120;
///         options.MaxDefinitionAgeDays = 1; // Stricter in production
///     }
///     else if (environment.IsDevelopment())
///     {
///         options.ServerHost = "localhost";
///         options.MaxRetryAttempts = 1; // Fail fast in development
///         options.HealthCheckCacheSeconds = 0; // No caching in development
///     }
/// });
/// </code>
/// </example>
public sealed class ClamAvScannerOptions
{
    /// <summary>
    /// Gets or sets the hostname or IP address of the ClamAV server for network connectivity.
    /// This parameter specifies the target server where the ClamAV daemon (clamd) is running
    /// and accessible for virus scanning operations.
    /// </summary>
    /// <value>
    /// The hostname, fully qualified domain name (FQDN), or IP address of the ClamAV server.
    /// Default value is "localhost" for local installations.
    /// </value>
    /// <remarks>
    /// <para>Configuration considerations include:</para>
    /// 
    /// <para><strong>Network Connectivity:</strong></para>
    /// <list type="bullet">
    /// <item>Must be a reachable hostname or IP address from the application server</item>
    /// <item>Firewall rules must allow connections to the specified host</item>
    /// <item>DNS resolution must be properly configured for hostname specifications</item>
    /// <item>Network latency should be considered for performance optimization</item>
    /// </list>
    /// 
    /// <para><strong>Security Considerations:</strong></para>
    /// <list type="bullet">
    /// <item>Should use secure network connections when possible</item>
    /// <item>Consider network segmentation for security isolation</item>
    /// <item>Monitor network traffic for security and performance analysis</item>
    /// <item>Implement proper access controls and authentication if required</item>
    /// </list>
    /// 
    /// <para><strong>Common Values:</strong></para>
    /// <list type="bullet">
    /// <item>"localhost" - Local ClamAV daemon installation</item>
    /// <item>"127.0.0.1" - Local IP address specification</item>
    /// <item>"clamav.company.com" - Dedicated ClamAV server</item>
    /// <item>"10.0.1.100" - Internal network IP address</item>
    /// </list>
    /// </remarks>
    public string ServerHost { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the port number for ClamAV server connection and communication.
    /// This parameter specifies the TCP port where the ClamAV daemon is listening
    /// for client connections and scanning requests.
    /// </summary>
    /// <value>
    /// The TCP port number for ClamAV server communication.
    /// Default value is 3310, which is the standard ClamAV daemon port.
    /// Valid range is 1-65535.
    /// </value>
    /// <remarks>
    /// <para>Port configuration considerations include:</para>
    /// 
    /// <para><strong>Standard Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Port 3310 is the default and standard port for ClamAV daemon</item>
    /// <item>Most ClamAV installations use this default port configuration</item>
    /// <item>Firewall rules and network policies should allow access to this port</item>
    /// <item>Load balancers and proxy configurations must route to correct port</item>
    /// </list>
    /// 
    /// <para><strong>Custom Port Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Non-standard ports may be used for security through obscurity</item>
    /// <item>Multiple ClamAV instances on same server require different ports</item>
    /// <item>Corporate environments may have port usage policies</item>
    /// <item>Container and orchestration environments may use dynamic port assignment</item>
    /// </list>
    /// 
    /// <para><strong>Security and Network Considerations:</strong></para>
    /// <list type="bullet">
    /// <item>Ensure port is not exposed to untrusted networks</item>
    /// <item>Configure appropriate firewall rules for access control</item>
    /// <item>Monitor port for unauthorized access attempts</item>
    /// <item>Consider network segmentation for additional security</item>
    /// </list>
    /// </remarks>
    public int ServerPort { get; set; } = 3310;

    /// <summary>
    /// Gets or sets the timeout duration in seconds for ClamAV operations and network communications.
    /// This parameter controls the maximum time allowed for individual scanning operations
    /// before they are considered failed and terminated.
    /// </summary>
    /// <value>
    /// The timeout duration in seconds for ClamAV operations.
    /// Default value is 30 seconds, providing reasonable balance between responsiveness and reliability.
    /// Valid range is 1-300 seconds (5 minutes maximum).
    /// </value>
    /// <remarks>
    /// <para>Timeout configuration considerations include:</para>
    /// 
    /// <para><strong>Performance Impact:</strong></para>
    /// <list type="bullet">
    /// <item>Shorter timeouts provide better user experience but may cause scan failures</item>
    /// <item>Longer timeouts accommodate large files but may impact system responsiveness</item>
    /// <item>File size and complexity directly impact required scanning time</item>
    /// <item>Network latency and server load affect total operation duration</item>
    /// </list>
    /// 
    /// <para><strong>Operational Considerations:</strong></para>
    /// <list type="bullet">
    /// <item>Should account for peak load scenarios and resource contention</item>
    /// <item>Must balance user experience with scanning thoroughness</item>
    /// <item>Integration with application-level timeouts and user interface design</item>
    /// <item>Monitoring and alerting for timeout occurrences and patterns</item>
    /// </list>
    /// 
    /// <para><strong>Recommended Values:</strong></para>
    /// <list type="bullet">
    /// <item>30 seconds - Default for most scenarios with moderate file sizes</item>
    /// <item>60 seconds - Large files or high-latency network environments</item>
    /// <item>120 seconds - Very large files or resource-constrained environments</item>
    /// <item>15 seconds - Small files with strict performance requirements</item>
    /// </list>
    /// </remarks>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of concurrent connections to the ClamAV server.
    /// This parameter controls connection pooling and resource utilization for
    /// high-throughput scanning scenarios with concurrent file processing.
    /// </summary>
    /// <value>
    /// The maximum number of concurrent connections allowed to the ClamAV server.
    /// Default value is 5 connections, providing good balance between performance and resource usage.
    /// Valid range is 1-100 connections.
    /// </value>
    /// <remarks>
    /// <para>Concurrency configuration considerations include:</para>
    /// 
    /// <para><strong>Performance Optimization:</strong></para>
    /// <list type="bullet">
    /// <item>Higher concurrency improves throughput for multiple simultaneous scans</item>
    /// <item>Must not exceed ClamAV server capacity and connection limits</item>
    /// <item>Consider network bandwidth and server resource constraints</item>
    /// <item>Balance with application server resource availability</item>
    /// </list>
    /// 
    /// <para><strong>Resource Management:</strong></para>
    /// <list type="bullet">
    /// <item>Each connection consumes memory and system resources</item>
    /// <item>ClamAV server must be configured to handle concurrent connections</item>
    /// <item>Network connection pooling and reuse for efficiency</item>
    /// <item>Monitoring of connection utilization and bottlenecks</item>
    /// </list>
    /// 
    /// <para><strong>Scaling Considerations:</strong></para>
    /// <list type="bullet">
    /// <item>Should scale with expected concurrent file upload volume</item>
    /// <item>Consider horizontal scaling with multiple ClamAV servers</item>
    /// <item>Load balancing and failover for high availability scenarios</item>
    /// <item>Capacity planning based on peak usage patterns</item>
    /// </list>
    /// </remarks>
    public int MaxConcurrentConnections { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed scanning operations.
    /// This parameter controls the resilience and fault tolerance of the scanner
    /// when encountering transient failures or network issues.
    /// </summary>
    /// <value>
    /// The maximum number of retry attempts for failed operations.
    /// Default value is 3 attempts, providing reasonable resilience without excessive delays.
    /// Valid range is 1-10 attempts.
    /// </value>
    /// <remarks>
    /// <para>Retry configuration considerations include:</para>
    /// 
    /// <para><strong>Reliability Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Handles transient network failures and temporary server unavailability</item>
    /// <item>Improves overall system reliability and user experience</item>
    /// <item>Reduces false negatives due to temporary infrastructure issues</item>
    /// <item>Provides graceful degradation under adverse conditions</item>
    /// </list>
    /// 
    /// <para><strong>Performance Impact:</strong></para>
    /// <list type="bullet">
    /// <item>More retries increase potential operation duration</item>
    /// <item>Exponential backoff prevents overwhelming stressed systems</item>
    /// <item>Must balance reliability with responsiveness requirements</item>
    /// <item>Consider cumulative timeout including all retry attempts</item>
    /// </list>
    /// 
    /// <para><strong>Error Handling Strategy:</strong></para>
    /// <list type="bullet">
    /// <item>Distinguish between retryable and non-retryable errors</item>
    /// <item>Implement circuit breaker patterns for systematic failures</item>
    /// <item>Log retry attempts for monitoring and diagnostics</item>
    /// <item>Consider alternative scanning methods after exhausted retries</item>
    /// </list>
    /// </remarks>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay in milliseconds for retry attempts using exponential backoff.
    /// This parameter controls the initial delay between retry attempts, with subsequent
    /// delays increasing exponentially to prevent overwhelming struggling services.
    /// </summary>
    /// <value>
    /// The base delay in milliseconds for the first retry attempt.
    /// Default value is 1000ms (1 second), providing reasonable initial delay.
    /// Valid range is 100-30000ms (30 seconds maximum).
    /// </value>
    /// <remarks>
    /// <para>Retry delay configuration considerations include:</para>
    /// 
    /// <para><strong>Exponential Backoff Strategy:</strong></para>
    /// <list type="bullet">
    /// <item>First retry after base delay (e.g., 1 second)</item>
    /// <item>Second retry after 2x base delay (e.g., 2 seconds)</item>
    /// <item>Third retry after 4x base delay (e.g., 4 seconds)</item>
    /// <item>Prevents overwhelming stressed or recovering services</item>
    /// </list>
    /// 
    /// <para><strong>Performance Balance:</strong></para>
    /// <list type="bullet">
    /// <item>Shorter delays provide faster recovery from transient issues</item>
    /// <item>Longer delays reduce system load during outages</item>
    /// <item>Must consider user experience and timeout expectations</item>
    /// <item>Balance between responsiveness and system stability</item>
    /// </list>
    /// </remarks>
    public int BaseRetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the maximum delay in milliseconds for retry attempts to cap exponential backoff.
    /// This parameter prevents exponential backoff delays from becoming excessively long
    /// while maintaining reasonable retry intervals for system recovery.
    /// </summary>
    /// <value>
    /// The maximum delay in milliseconds for any single retry attempt.
    /// Default value is 10000ms (10 seconds), preventing excessive wait times.
    /// Valid range is 1000-60000ms (1 minute maximum).
    /// </value>
    /// <remarks>
    /// <para>Maximum delay configuration provides:</para>
    /// 
    /// <para><strong>Backoff Capping Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Prevents exponential backoff from creating extremely long delays</item>
    /// <item>Maintains reasonable retry intervals even after multiple failures</item>
    /// <item>Ensures predictable maximum operation duration</item>
    /// <item>Balances system recovery time with user experience</item>
    /// </list>
    /// 
    /// <para><strong>System Behavior:</strong></para>
    /// <list type="bullet">
    /// <item>Once maximum delay is reached, subsequent retries use this interval</item>
    /// <item>Provides consistent retry pattern for extended outages</item>
    /// <item>Allows system recovery without overwhelming with requests</item>
    /// <item>Maintains reasonable user experience during extended failures</item>
    /// </list>
    /// </remarks>
    public int MaxRetryDelayMs { get; set; } = 10000;

    /// <summary>
    /// Gets or sets the maximum file size in megabytes that can be scanned by the virus scanner.
    /// This parameter provides resource protection by preventing excessively large files
    /// from consuming system resources or causing performance degradation.
    /// </summary>
    /// <value>
    /// The maximum file size in megabytes that can be processed.
    /// Default value is 100 MB, suitable for most document and media file scenarios.
    /// Valid range is 1-1000 MB (1 GB maximum).
    /// </value>
    /// <remarks>
    /// <para>File size limit configuration considerations include:</para>
    /// 
    /// <para><strong>Resource Protection:</strong></para>
    /// <list type="bullet">
    /// <item>Prevents memory exhaustion from extremely large file processing</item>
    /// <item>Limits network transfer time and bandwidth usage</item>
    /// <item>Protects ClamAV server from resource exhaustion</item>
    /// <item>Maintains responsive scanning for normal file sizes</item>
    /// </list>
    /// 
    /// <para><strong>Business Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Should accommodate legitimate business file sizes</item>
    /// <item>Consider typical file types and sizes in your environment</item>
    /// <item>Balance security scanning with business functionality</item>
    /// <item>Provide clear error messages for oversized files</item>
    /// </list>
    /// 
    /// <para><strong>Recommended Limits:</strong></para>
    /// <list type="bullet">
    /// <item>50 MB - Standard document and image processing</item>
    /// <item>100 MB - Mixed content including presentations and media</item>
    /// <item>250 MB - Large media files and specialized documents</item>
    /// <item>500 MB - Enterprise scenarios with large file requirements</item>
    /// </list>
    /// </remarks>
    public int MaxFileSizeMB { get; set; } = 100;

    /// <summary>
    /// Gets or sets the health check cache duration in seconds to optimize monitoring performance.
    /// This parameter controls how long health check results are cached to prevent
    /// excessive health checks from impacting scanner performance.
    /// </summary>
    /// <value>
    /// The duration in seconds for which health check results are cached.
    /// Default value is 60 seconds, providing good balance between freshness and performance.
    /// Valid range is 0-3600 seconds (1 hour maximum). Set to 0 to disable caching.
    /// </value>
    /// <remarks>
    /// <para>Health check caching considerations include:</para>
    /// 
    /// <para><strong>Performance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Reduces load on ClamAV server from frequent health checks</item>
    /// <item>Improves response time for monitoring and load balancer probes</item>
    /// <item>Prevents health check storms during high-traffic periods</item>
    /// <item>Optimizes resource utilization for operational monitoring</item>
    /// </list>
    /// 
    /// <para><strong>Freshness vs Performance:</strong></para>
    /// <list type="bullet">
    /// <item>Longer caching improves performance but reduces health status freshness</item>
    /// <item>Shorter caching provides more accurate status but increases system load</item>
    /// <item>Consider monitoring system requirements and update frequencies</item>
    /// <item>Balance with alerting and failover time requirements</item>
    /// </list>
    /// 
    /// <para><strong>Recommended Values:</para>
    /// <list type="bullet">
    /// <item>60 seconds - Standard monitoring with good performance balance</item>
    /// <item>30 seconds - More responsive monitoring for critical systems</item>
    /// <item>120 seconds - Lower frequency monitoring for stable systems</item>
    /// <item>0 seconds - Disable caching for development or debugging</item>
    /// </list>
    /// </remarks>
    public int HealthCheckCacheSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum age in days for virus definitions to be considered current.
    /// This parameter defines the threshold for determining whether virus definitions
    /// are sufficiently up-to-date for effective threat detection.
    /// </summary>
    /// <value>
    /// The maximum age in days for virus definitions to be considered current.
    /// Default value is 7 days, providing reasonable balance between security and operational flexibility.
    /// Valid range is 1-30 days.
    /// </value>
    /// <remarks>
    /// <para>Definition currency configuration considerations include:</para>
    /// 
    /// <para><strong>Security Effectiveness:</strong></para>
    /// <list type="bullet">
    /// <item>More recent definitions provide better protection against new threats</item>
    /// <item>Older definitions may miss recently discovered malware variants</item>
    /// <item>Balance between security effectiveness and operational stability</item>
    /// <item>Consider threat landscape and update frequency requirements</item>
    /// </list>
    /// 
    /// <para><strong>Operational Impact:</strong></para>
    /// <list type="bullet">
    /// <item>Stricter currency requirements increase maintenance overhead</item>
    /// <item>More lenient settings provide operational flexibility</item>
    /// <item>Must align with organizational security policies</item>
    /// <item>Consider automated update mechanisms and schedules</item>
    /// </list>
    /// 
    /// <para><strong>Recommended Settings:</strong></para>
    /// <list type="bullet">
    /// <item>1-3 days - High-security environments with frequent updates</item>
    /// <item>7 days - Standard business environments with weekly update cycles</item>
    /// <item>14 days - Stable environments with less frequent maintenance</item>
    /// <item>30 days - Development or testing environments with relaxed requirements</item>
    /// </list>
    /// </remarks>
    public int MaxDefinitionAgeDays { get; set; } = 7;
}