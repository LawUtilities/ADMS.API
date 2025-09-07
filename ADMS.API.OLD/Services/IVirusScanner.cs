using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Services;

/// <summary>
/// Defines the contract for enterprise-grade virus scanning services with comprehensive malware detection capabilities.
/// </summary>
/// <remarks>
/// This interface provides a comprehensive contract for virus scanning implementations including:
/// 
/// Core Functionality:
/// - Asynchronous virus detection for file streams with cancellation support
/// - Comprehensive malware scanning including viruses, trojans, malware, and suspicious content
/// - High-performance scanning operations optimized for large file processing
/// - Detailed scan results with comprehensive threat information and metadata
/// - Support for multiple scanning engines and threat detection technologies
/// 
/// Security Features:
/// - Real-time threat detection with up-to-date virus definitions
/// - Multi-layered scanning approach with signature and behavioral analysis
/// - Quarantine and threat mitigation recommendations
/// - Integration with threat intelligence feeds for emerging threats
/// - Compliance support for security standards and regulations
/// 
/// Performance Characteristics:
/// - Asynchronous operations for non-blocking file processing
/// - Cancellation token support for responsive user experience
/// - Stream-based processing for memory-efficient large file handling
/// - Configurable timeout and resource management
/// - Optimized scanning algorithms for high-throughput scenarios
/// 
/// Integration Features:
/// - Compatible with cloud-based and on-premises scanning solutions
/// - Support for multiple antivirus engines and detection technologies
/// - Integration with file upload pipelines and document management systems
/// - Extensible architecture for custom threat detection logic
/// - Comprehensive logging and monitoring capabilities
/// 
/// Implementation Considerations:
/// - Implementations should handle various file formats and encoding types
/// - Proper resource management and cleanup of temporary scanning files
/// - Thread-safe operations for concurrent scanning scenarios
/// - Graceful handling of scanning service unavailability or errors
/// - Configuration support for scanning sensitivity and detection rules
/// 
/// Security Best Practices:
/// - Regular updates of virus definitions and threat signatures
/// - Secure handling of potentially infected file content during scanning
/// - Proper disposal of file streams and temporary resources
/// - Audit logging of all scanning activities for compliance
/// - Integration with incident response procedures for threat detection
/// 
/// Common Use Cases:
/// - File upload validation in web applications and APIs
/// - Document management system security screening
/// - Email attachment scanning and filtering
/// - Cloud storage security validation
/// - Automated malware detection in file processing pipelines
/// 
/// Supported Scenarios:
/// - Real-time file scanning during upload operations
/// - Batch processing of multiple files for security validation
/// - Scheduled scanning of stored files and documents
/// - Integration with content delivery networks for security
/// - API-based scanning services for distributed applications
/// 
/// The interface is designed to be:
/// - Extensible: Easy to implement with various scanning technologies
/// - Performant: Asynchronous operations with cancellation support
/// - Secure: Comprehensive threat detection with proper resource handling
/// - Observable: Rich result information for monitoring and compliance
/// - Reliable: Robust error handling and graceful degradation capabilities
/// </remarks>
/// <example>
/// <code>
/// // Basic virus scanning implementation
/// public class MyVirusScannerService : IVirusScanner
/// {
///     public async Task&lt;VirusScanResult&gt; ScanFileForVirusesAsync(
///         Stream fileStream, 
///         CancellationToken cancellationToken = default)
///     {
///         // Implementation details...
///         return new VirusScanResult 
///         { 
///             IsClean = true, 
///             ScanDuration = TimeSpan.FromSeconds(1.5),
///             ThreatDetails = null 
///         };
///     }
/// }
/// 
/// // Usage in file upload controller
/// [HttpPost("upload")]
/// public async Task&lt;IActionResult&gt; UploadFile(
///     IFormFile file, 
///     CancellationToken cancellationToken)
/// {
///     using var stream = file.OpenReadStream();
///     var scanResult = await _virusScanner.ScanFileForVirusesAsync(stream, cancellationToken);
///     
///     if (!scanResult.IsClean)
///     {
///         _logger.LogWarning("Virus detected: {ThreatInfo}", scanResult.ThreatDetails?.ThreatName);
///         return BadRequest("File contains malicious content and cannot be processed");
///     }
///     
///     // Continue with file processing...
///     return Ok("File uploaded successfully");
/// }
/// </code>
/// </example>
public interface IVirusScanner
{
    /// <summary>
    /// Performs comprehensive virus scanning of the provided file stream with detailed result information.
    /// </summary>
    /// <param name="fileStream">
    /// The file stream to scan for viruses and malicious content.
    /// Must be a readable stream containing the file data to be analyzed.
    /// The stream position will be reset to the beginning before scanning.
    /// Stream must remain open during the entire scanning operation.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to allow cancellation of long-running scan operations.
    /// Implementations should respect cancellation requests to provide responsive user experience.
    /// Default value allows the operation to run to completion without cancellation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous scanning operation.
    /// The task result contains a VirusScanResult with comprehensive information about:
    /// - Whether the file is clean or contains malicious content
    /// - Detailed threat information including threat names and types
    /// - Scanning metadata including duration and scanner information
    /// - Recommended actions for threat mitigation if applicable
    /// - Additional context for logging and compliance reporting
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive virus scanning including:
    /// 
    /// Scanning Capabilities:
    /// - Real-time virus detection using up-to-date threat definitions
    /// - Multi-layered analysis including signature-based and behavioral detection
    /// - Support for various file formats and content types
    /// - Detection of viruses, trojans, malware, spyware, and suspicious content
    /// - Integration with cloud-based threat intelligence for emerging threats
    /// 
    /// Performance Characteristics:
    /// - Asynchronous operation for non-blocking execution
    /// - Memory-efficient stream processing for large files
    /// - Configurable timeout and resource management
    /// - Cancellation token support for responsive cancellation
    /// - Optimized scanning algorithms for high-throughput scenarios
    /// 
    /// Security Features:
    /// - Secure handling of potentially malicious content during scanning
    /// - Proper isolation of scanning processes from system resources
    /// - Comprehensive threat classification and severity assessment
    /// - Integration with quarantine and mitigation procedures
    /// - Audit logging of all scanning activities for compliance
    /// 
    /// Error Handling:
    /// - Graceful handling of corrupted or unreadable file content
    /// - Proper exception handling for scanning service failures
    /// - Fallback mechanisms for scanner unavailability
    /// - Detailed error information for troubleshooting and monitoring
    /// - Recovery procedures for transient scanning failures
    /// 
    /// Resource Management:
    /// - Proper cleanup of temporary files and resources
    /// - Memory-efficient processing of large file streams
    /// - Thread-safe operations for concurrent scanning scenarios
    /// - Configurable resource limits and timeout handling
    /// - Integration with system resource monitoring
    /// 
    /// Implementation Requirements:
    /// - Must reset stream position to beginning before scanning
    /// - Should not dispose the provided file stream
    /// - Must handle various file formats and encoding types
    /// - Should provide detailed scan results for all scenarios
    /// - Must implement proper cancellation token handling
    /// 
    /// Common Usage Patterns:
    /// - File upload validation in web applications
    /// - Document management system security screening
    /// - Automated scanning in file processing pipelines
    /// - Real-time content filtering and validation
    /// - Batch processing security validation
    /// 
    /// The method is designed to be:
    /// - Reliable: Consistent results with proper error handling
    /// - Performant: Efficient processing with cancellation support
    /// - Secure: Safe handling of potentially malicious content
    /// - Observable: Rich result information for monitoring
    /// - Compliant: Audit trail support for security requirements
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when fileStream parameter is null.
    /// Implementations must validate input parameters and provide appropriate error handling.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when fileStream is not readable or is in an invalid state.
    /// Implementations should validate stream accessibility before processing.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the cancellation token.
    /// Implementations should properly handle cancellation requests during scanning.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the virus scanner is not properly configured or available.
    /// Implementations should validate scanner readiness before processing.
    /// </exception>
    /// <exception cref="TimeoutException">
    /// Thrown when the scanning operation exceeds configured timeout limits.
    /// Implementations should enforce reasonable timeout policies for responsiveness.
    /// </exception>
    /// <example>
    /// <code>
    /// // Example implementation with comprehensive error handling
    /// public async Task&lt;VirusScanResult&gt; ScanFileForVirusesAsync(
    ///     Stream fileStream, 
    ///     CancellationToken cancellationToken = default)
    /// {
    ///     ArgumentNullException.ThrowIfNull(fileStream);
    ///     
    ///     if (!fileStream.CanRead)
    ///         throw new ArgumentException("Stream must be readable", nameof(fileStream));
    ///     
    ///     try
    ///     {
    ///         // Reset stream position for scanning
    ///         if (fileStream.CanSeek)
    ///             fileStream.Position = 0;
    ///         
    ///         var startTime = DateTime.UtcNow;
    ///         
    ///         // Perform virus scanning (implementation specific)
    ///         var scanResult = await PerformVirusScan(fileStream, cancellationToken);
    ///         
    ///         var endTime = DateTime.UtcNow;
    ///         
    ///         return new VirusScanResult
    ///         {
    ///             IsClean = scanResult.IsClean,
    ///             ThreatDetails = scanResult.ThreatInfo,
    ///             ScanDuration = endTime - startTime,
    ///             ScannerVersion = GetScannerVersion(),
    ///             ScanTimestamp = endTime
    ///         };
    ///     }
    ///     catch (OperationCanceledException)
    ///     {
    ///         _logger.LogInformation("Virus scan operation was canceled");
    ///         throw;
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         _logger.LogError(ex, "Error during virus scanning operation");
    ///         
    ///         // Return inconclusive result for safety
    ///         return VirusScanResult.CreateErrorResult(
    ///             "Scanning failed - treating as potentially unsafe",
    ///             ex.Message);
    ///     }
    /// }
    /// 
    /// // Usage in file processing pipeline
    /// public async Task&lt;ProcessingResult&gt; ProcessUploadedFile(
    ///     Stream fileStream,
    ///     CancellationToken cancellationToken = default)
    /// {
    ///     // Step 1: Virus scanning
    ///     var scanResult = await _virusScanner.ScanFileForVirusesAsync(fileStream, cancellationToken);
    ///     
    ///     if (!scanResult.IsClean)
    ///     {
    ///         // Log security event
    ///         _securityLogger.LogSecurityEvent("VirusDetected", new 
    ///         {
    ///             ThreatName = scanResult.ThreatDetails?.ThreatName,
    ///             ThreatType = scanResult.ThreatDetails?.ThreatType,
    ///             ScanDuration = scanResult.ScanDuration,
    ///             Timestamp = scanResult.ScanTimestamp
    ///         });
    ///         
    ///         // Trigger security response
    ///         await _securityService.HandleThreatDetection(scanResult.ThreatDetails);
    ///         
    ///         return ProcessingResult.SecurityRejected(scanResult.ThreatDetails?.ThreatName);
    ///     }
    ///     
    ///     // Continue with file processing...
    ///     return await ProcessCleanFile(fileStream, cancellationToken);
    /// }
    /// </code>
    /// </example>
    Task<VirusScanResult> ScanFileForVirusesAsync(Stream fileStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether the virus scanner is properly configured and operational.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token for the health check operation.</param>
    /// <returns>
    /// A task that represents the asynchronous health check operation.
    /// The task result contains a ScannerHealthResult with information about:
    /// - Scanner availability and operational status
    /// - Virus definition version and last update timestamp
    /// - Performance metrics and operational statistics
    /// - Configuration validation and system requirements
    /// - Recommendations for optimization or maintenance
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive health checking including:
    /// 
    /// Health Check Capabilities:
    /// - Verification of scanner service availability and responsiveness
    /// - Validation of virus definition currency and update status
    /// - Assessment of system resources and performance metrics
    /// - Configuration validation and compatibility checking
    /// - Integration testing with dependent services and components
    /// 
    /// Operational Monitoring:
    /// - Real-time status of scanning engines and detection capabilities
    /// - Performance metrics including scan speed and resource usage
    /// - Queue status and processing capacity for high-volume scenarios
    /// - Error rates and failure patterns for reliability assessment
    /// - Maintenance schedules and update requirements
    /// 
    /// The health check is designed to be:
    /// - Fast: Quick response time for operational monitoring
    /// - Comprehensive: Complete assessment of scanner capabilities
    /// - Non-invasive: Minimal impact on scanning performance
    /// - Informative: Detailed status information for troubleshooting
    /// - Actionable: Clear recommendations for issue resolution
    /// 
    /// Common usage scenarios:
    /// - Application startup validation and readiness checks
    /// - Periodic health monitoring and alerting systems
    /// - Load balancer health probe endpoints
    /// - Diagnostic troubleshooting and maintenance procedures
    /// - Compliance reporting and audit requirements
    /// </remarks>
    /// <example>
    /// <code>
    /// // Health check implementation
    /// public async Task&lt;ScannerHealthResult&gt; CheckHealthAsync(CancellationToken cancellationToken = default)
    /// {
    ///     var healthResult = new ScannerHealthResult();
    ///     
    ///     try
    ///     {
    ///         // Check scanner service connectivity
    ///         var serviceAvailable = await PingServiceAsync(cancellationToken);
    ///         healthResult.IsAvailable = serviceAvailable;
    ///         
    ///         if (serviceAvailable)
    ///         {
    ///             // Check virus definitions
    ///             var definitionInfo = await GetDefinitionInfoAsync(cancellationToken);
    ///             healthResult.DefinitionVersion = definitionInfo.Version;
    ///             healthResult.LastDefinitionUpdate = definitionInfo.LastUpdate;
    ///             healthResult.DefinitionsAreCurrent = definitionInfo.IsCurrent;
    ///             
    ///             // Performance metrics
    ///             var performanceMetrics = await GetPerformanceMetricsAsync(cancellationToken);
    ///             healthResult.AverageScanTime = performanceMetrics.AverageScanTime;
    ///             healthResult.SuccessRate = performanceMetrics.SuccessRate;
    ///         }
    ///         
    ///         healthResult.OverallHealth = DetermineOverallHealth(healthResult);
    ///         return healthResult;
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         return ScannerHealthResult.CreateErrorResult(ex.Message);
    ///     }
    /// }
    /// 
    /// // Usage in health check endpoint
    /// [HttpGet("health")]
    /// public async Task&lt;IActionResult&gt; HealthCheck()
    /// {
    ///     var healthResult = await _virusScanner.CheckHealthAsync();
    ///     
    ///     if (healthResult.OverallHealth == HealthStatus.Healthy)
    ///     {
    ///         return Ok(healthResult);
    ///     }
    ///     else
    ///     {
    ///         return StatusCode(503, healthResult); // Service Unavailable
    ///     }
    /// }
    /// </code>
    /// </example>
    Task<ScannerHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves comprehensive information about the virus scanner configuration and capabilities.
    /// </summary>
    /// <returns>
    /// A ScannerInfo object containing detailed information about:
    /// - Scanner name, version, and vendor information
    /// - Supported file formats and detection capabilities
    /// - Configuration settings and operational parameters
    /// - Performance characteristics and resource requirements
    /// - Integration details and compatibility information
    /// </returns>
    /// <remarks>
    /// This method provides static information about the scanner implementation including:
    /// 
    /// Scanner Identification:
    /// - Product name, version, and build information
    /// - Vendor details and support contact information
    /// - License information and usage limitations
    /// - Installation details and system requirements
    /// 
    /// Capability Information:
    /// - Supported file formats and content types
    /// - Detection technologies and scanning methods
    /// - Performance characteristics and throughput limits
    /// - Integration features and API compatibility
    /// 
    /// The information is useful for:
    /// - System documentation and compliance reporting
    /// - Integration planning and compatibility assessment
    /// - Performance tuning and capacity planning
    /// - Troubleshooting and support scenarios
    /// - Audit trails and security assessments
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get scanner information for documentation
    /// var scannerInfo = _virusScanner.GetScannerInfo();
    /// 
    /// _logger.LogInformation("Using virus scanner: {ScannerName} v{Version}",
    ///     scannerInfo.ProductName, scannerInfo.Version);
    /// 
    /// // Use for compatibility checking
    /// if (scannerInfo.SupportedFormats.Contains("application/pdf"))
    /// {
    ///     // Safe to scan PDF files
    ///     await _virusScanner.ScanFileForVirusesAsync(pdfStream);
    /// }
    /// </code>
    /// </example>
    ScannerInfo GetScannerInfo();
}

/// <summary>
/// Represents the result of a virus scanning operation with comprehensive threat and metadata information.
/// </summary>
/// <remarks>
/// This class provides complete information about virus scanning results including:
/// - Clean/infected status with confidence levels
/// - Detailed threat information for detected malware
/// - Scanning metadata including performance metrics
/// - Recommendations for threat handling and mitigation
/// - Compliance and audit information for security reporting
/// </remarks>
public sealed class VirusScanResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the scanned file is free from malicious content.
    /// </summary>
    /// <remarks>
    /// - true: File is clean and safe for processing
    /// - false: File contains malicious content and should be rejected
    /// 
    /// This property represents the primary scan result and should be used
    /// for security decisions regarding file processing and storage.
    /// </remarks>
    public required bool IsClean { get; init; }

    /// <summary>
    /// Gets or sets detailed information about detected threats, if any.
    /// </summary>
    /// <remarks>
    /// Contains comprehensive threat information including:
    /// - Threat name and classification
    /// - Severity level and risk assessment
    /// - Detection method and confidence level
    /// - Recommended mitigation actions
    /// 
    /// This property is null when IsClean is true.
    /// </remarks>
    public ThreatDetails? ThreatDetails { get; init; }

    /// <summary>
    /// Gets or sets the duration of the scanning operation.
    /// </summary>
    /// <remarks>
    /// Provides performance metrics for:
    /// - Monitoring scanning performance
    /// - Capacity planning and optimization
    /// - SLA compliance and reporting
    /// - Performance troubleshooting
    /// </remarks>
    public TimeSpan ScanDuration { get; init; }

    /// <summary>
    /// Gets or sets the timestamp when the scan was completed.
    /// </summary>
    /// <remarks>
    /// Used for:
    /// - Audit logging and compliance reporting
    /// - Result freshness validation
    /// - Performance monitoring and analysis
    /// - Debugging and troubleshooting
    /// </remarks>
    public DateTime ScanTimestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the name of the scanner that performed the scan.
    /// </summary>
    /// <remarks>
    /// Identifies the specific scanner implementation that performed the scan.
    /// Important for audit trails and compatibility tracking.
    /// </remarks>
    public string? ScannerName { get; init; }

    /// <summary>
    /// Gets or sets the version information of the scanner that performed the scan.
    /// </summary>
    /// <remarks>
    /// Includes information about:
    /// - Scanner engine version
    /// - Virus definition version
    /// - Detection capability version
    /// - Configuration identifier
    /// 
    /// Important for audit trails and result validation.
    /// </remarks>
    public string? ScannerVersion { get; init; }

    /// <summary>
    /// Gets or sets additional metadata about the scanning operation.
    /// </summary>
    /// <remarks>
    /// May include:
    /// - File size and type information
    /// - Scanning method details
    /// - Performance metrics
    /// - Configuration parameters used
    /// - Integration context information
    /// </remarks>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Creates a VirusScanResult indicating a clean file.
    /// </summary>
    /// <param name="scanDuration">The duration of the scan operation.</param>
    /// <param name="scannerName">Optional scanner name information.</param>
    /// <param name="scannerVersion">Optional scanner version information.</param>
    /// <returns>A VirusScanResult indicating the file is clean.</returns>
    public static VirusScanResult CreateCleanResult(TimeSpan scanDuration, string? scannerName = null, string? scannerVersion = null)
    {
        return new VirusScanResult
        {
            IsClean = true,
            ThreatDetails = null,
            ScanDuration = scanDuration,
            ScannerName = scannerName,
            ScannerVersion = scannerVersion,
            ScanTimestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a VirusScanResult indicating a threat was detected.
    /// </summary>
    /// <param name="threatDetails">Details about the detected threat.</param>
    /// <param name="scanDuration">The duration of the scan operation.</param>
    /// <param name="scannerName">Optional scanner name information.</param>
    /// <param name="scannerVersion">Optional scanner version information.</param>
    /// <returns>A VirusScanResult indicating a threat was detected.</returns>
    public static VirusScanResult CreateThreatDetectedResult(
        ThreatDetails threatDetails,
        TimeSpan scanDuration,
        string? scannerName = null,
        string? scannerVersion = null)
    {
        return new VirusScanResult
        {
            IsClean = false,
            ThreatDetails = threatDetails,
            ScanDuration = scanDuration,
            ScannerName = scannerName,
            ScannerVersion = scannerVersion,
            ScanTimestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a VirusScanResult indicating a scanning error occurred.
    /// </summary>
    /// <param name="errorMessage">The error message describing the scanning failure.</param>
    /// <param name="errorDetails">Additional error details.</param>
    /// <returns>A VirusScanResult indicating a scanning error (treated as unsafe).</returns>
    public static VirusScanResult CreateErrorResult(string errorMessage, string? errorDetails = null)
    {
        return new VirusScanResult
        {
            IsClean = false, // Treat errors as unsafe
            ThreatDetails = new ThreatDetails
            {
                ThreatName = "SCANNING_ERROR",
                ThreatType = ThreatType.ScanningError,
                Severity = ThreatSeverity.High,
                Description = errorMessage,
                RecommendedAction = "Review scanning configuration and retry"
            },
            ScanDuration = TimeSpan.Zero,
            ScanTimestamp = DateTime.UtcNow,
            Metadata = errorDetails != null
                ? new Dictionary<string, object> { ["ErrorDetails"] = errorDetails }
                : null
        };
    }
}

/// <summary>
/// Provides detailed information about detected threats.
/// </summary>
public sealed class ThreatDetails
{
    /// <summary>
    /// Gets or sets the name or signature of the detected threat.
    /// </summary>
    public required string ThreatName { get; init; }

    /// <summary>
    /// Gets or sets the classification type of the threat.
    /// </summary>
    public required ThreatType ThreatType { get; init; }

    /// <summary>
    /// Gets or sets the severity level of the threat.
    /// </summary>
    public required ThreatSeverity Severity { get; init; }

    /// <summary>
    /// Gets or sets a detailed description of the threat.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the method used to detect this threat.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// - "Signature-based detection"
    /// - "Heuristic analysis"
    /// - "Behavioral analysis"
    /// - "Machine learning detection"
    /// </remarks>
    public string? DetectionMethod { get; init; }

    /// <summary>
    /// Gets or sets the confidence level of the threat detection.
    /// </summary>
    /// <remarks>
    /// Range: 0.0 to 1.0 where 1.0 indicates highest confidence.
    /// </remarks>
    [Range(0.0, 1.0)]
    public double ConfidenceLevel { get; init; } = 1.0;

    /// <summary>
    /// Gets or sets the recommended action for handling this threat.
    /// </summary>
    public string? RecommendedAction { get; init; }

    /// <summary>
    /// Gets or sets additional metadata about the threat.
    /// </summary>
    public Dictionary<string, object>? AdditionalInfo { get; init; }
}

/// <summary>
/// Specifies the type of threat detected during virus scanning.
/// </summary>
public enum ThreatType
{
    /// <summary>Unknown or unclassified threat.</summary>
    Unknown,

    /// <summary>Computer virus.</summary>
    Virus,

    /// <summary>Trojan horse malware.</summary>
    Trojan,

    /// <summary>Worm malware.</summary>
    Worm,

    /// <summary>Spyware or privacy threat.</summary>
    Spyware,

    /// <summary>Adware or unwanted advertising software.</summary>
    Adware,

    /// <summary>Rootkit or system-level threat.</summary>
    Rootkit,

    /// <summary>Ransomware threat.</summary>
    Ransomware,

    /// <summary>Potentially unwanted program (PUP).</summary>
    PotentiallyUnwanted,

    /// <summary>Suspicious or potentially malicious content.</summary>
    Suspicious,

    /// <summary>Error occurred during scanning.</summary>
    ScanningError
}

/// <summary>
/// Specifies the severity level of detected threats.
/// </summary>
public enum ThreatSeverity
{
    /// <summary>Low severity threat with minimal risk.</summary>
    Low,

    /// <summary>Medium severity threat with moderate risk.</summary>
    Medium,

    /// <summary>High severity threat requiring immediate attention.</summary>
    High,

    /// <summary>Critical threat requiring urgent action.</summary>
    Critical
}

/// <summary>
/// Represents the health status and operational information of the virus scanner service.
/// This class provides comprehensive health monitoring capabilities with support for
/// real-time status updates and detailed diagnostic information.
/// </summary>
/// <remarks>
/// This class supports both initialization-time setup and runtime updates to accommodate
/// different health checking scenarios and implementation patterns.
/// </remarks>
public sealed class ScannerHealthResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the scanner service is available.
    /// </summary>
    /// <remarks>
    /// This property indicates basic service availability and can be updated during
    /// health check operations to reflect current connectivity status.
    /// </remarks>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Gets or sets the overall health status of the scanner.
    /// </summary>
    /// <remarks>
    /// This property provides a comprehensive assessment of scanner health and can be
    /// updated based on the results of multiple health check components.
    /// </remarks>
    public HealthStatus OverallHealth { get; set; } = HealthStatus.Unhealthy;

    /// <summary>
    /// Gets or sets the name of the scanner being monitored.
    /// </summary>
    /// <remarks>
    /// Identifies the specific scanner implementation being health checked.
    /// Important for monitoring systems that track multiple scanner types.
    /// </remarks>
    public string? ScannerName { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the health check was performed.
    /// </summary>
    /// <remarks>
    /// Provides temporal context for health check results and enables
    /// freshness validation of health status information.
    /// </remarks>
    public DateTime CheckTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the current virus definition version.
    /// </summary>
    /// <remarks>
    /// Tracks the version of virus definitions currently in use by the scanner.
    /// Important for ensuring adequate protection against current threats.
    /// </remarks>
    public string? DefinitionVersion { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last virus definition update.
    /// </summary>
    /// <remarks>
    /// Tracks when virus definitions were last refreshed, which is critical
    /// for maintaining effective threat detection capabilities.
    /// </remarks>
    public DateTime? LastDefinitionUpdate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether virus definitions are current.
    /// </summary>
    /// <remarks>
    /// Indicates whether the virus definitions meet currency requirements
    /// for effective threat detection. Can be updated during health checks.
    /// </remarks>
    public bool DefinitionsAreCurrent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether scanning functionality is operational.
    /// </summary>
    /// <remarks>
    /// Indicates whether the core scanning functionality is working properly.
    /// Typically tested using standard test signatures like EICAR.
    /// </remarks>
    public bool? ScanningFunctional { get; set; }

    /// <summary>
    /// Gets or sets the response time in milliseconds for scanner operations.
    /// </summary>
    /// <remarks>
    /// Measures the responsiveness of the scanner service for performance monitoring.
    /// Can be updated with real-time performance measurements.
    /// </remarks>
    public double ResponseTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the duration of the health check operation.
    /// </summary>
    /// <remarks>
    /// Tracks how long the health check itself took to complete.
    /// Useful for monitoring the overhead of health checking operations.
    /// </remarks>
    public TimeSpan HealthCheckDuration { get; set; }

    /// <summary>
    /// Gets or sets the average scan time for performance monitoring.
    /// </summary>
    /// <remarks>
    /// Provides performance metrics for capacity planning and optimization.
    /// Can be updated with current performance measurements.
    /// </remarks>
    public TimeSpan? AverageScanTime { get; set; }

    /// <summary>
    /// Gets or sets the success rate of scanning operations.
    /// </summary>
    /// <remarks>
    /// Range: 0.0 to 1.0 where 1.0 indicates 100% success rate.
    /// Can be updated with current operational statistics.
    /// </remarks>
    [Range(0.0, 1.0)]
    public double SuccessRate { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets any error messages or diagnostic information.
    /// </summary>
    /// <remarks>
    /// Provides detailed error information when health checks fail.
    /// Can be updated with current error conditions.
    /// </remarks>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets a collection of issues identified during health checking.
    /// </summary>
    /// <remarks>
    /// Provides a comprehensive list of all issues found during health evaluation.
    /// Can be updated to add new issues as they are discovered.
    /// </remarks>
    public List<string> Issues { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets additional health information and metrics.
    /// </summary>
    /// <remarks>
    /// Provides extensible storage for implementation-specific health metrics.
    /// Can be updated with runtime diagnostic information.
    /// </remarks>
    public Dictionary<string, object>? AdditionalInfo { get; set; }

    /// <summary>
    /// Creates a healthy scanner health result with optimal settings.
    /// </summary>
    /// <param name="scannerName">Optional name of the scanner.</param>
    /// <returns>A ScannerHealthResult indicating healthy status.</returns>
    /// <remarks>
    /// This factory method creates a health result representing an optimal,
    /// fully functional scanner with current definitions and good performance.
    /// </remarks>
    public static ScannerHealthResult CreateHealthyResult(string? scannerName = null)
    {
        return new ScannerHealthResult
        {
            IsAvailable = true,
            OverallHealth = HealthStatus.Healthy,
            ScannerName = scannerName,
            CheckTimestamp = DateTime.UtcNow,
            DefinitionsAreCurrent = true,
            ScanningFunctional = true,
            SuccessRate = 1.0,
            Issues = new List<string>(),
            AdditionalInfo = new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Creates an error health result indicating scanner problems.
    /// </summary>
    /// <param name="scannerName">Name of the scanner that failed health check.</param>
    /// <param name="errorMessage">Description of the health check failure.</param>
    /// <param name="healthCheckDuration">Duration of the failed health check.</param>
    /// <returns>A ScannerHealthResult indicating unhealthy status.</returns>
    /// <remarks>
    /// This factory method creates a health result representing a scanner
    /// in an unhealthy state with comprehensive error information.
    /// </remarks>
    public static ScannerHealthResult CreateErrorResult(string scannerName, string errorMessage, TimeSpan healthCheckDuration)
    {
        return new ScannerHealthResult
        {
            IsAvailable = false,
            OverallHealth = HealthStatus.Unhealthy,
            ScannerName = scannerName,
            CheckTimestamp = DateTime.UtcNow,
            ErrorMessage = errorMessage,
            DefinitionsAreCurrent = false,
            ScanningFunctional = false,
            SuccessRate = 0.0,
            HealthCheckDuration = healthCheckDuration,
            Issues = new List<string> { errorMessage },
            AdditionalInfo = new Dictionary<string, object>
            {
                ["HealthCheckFailed"] = true,
                ["FailureTimestamp"] = DateTime.UtcNow
            }
        };
    }

    /// <summary>
    /// Creates an error health result with minimal information for simple error scenarios.
    /// </summary>
    /// <param name="errorMessage">Description of the health check failure.</param>
    /// <returns>A ScannerHealthResult indicating unhealthy status.</returns>
    /// <remarks>
    /// This overload provides a simplified way to create error results when
    /// detailed scanner information is not available.
    /// </remarks>
    public static ScannerHealthResult CreateErrorResult(string errorMessage)
    {
        return CreateErrorResult("Unknown", errorMessage, TimeSpan.Zero);
    }
}

/// <summary>
/// Specifies the health status of the scanner service.
/// </summary>
public enum HealthStatus
{
    /// <summary>Scanner is healthy and fully operational.</summary>
    Healthy,

    /// <summary>Scanner is operational but with some issues or warnings.</summary>
    Warning,

    /// <summary>Scanner is operational but with degraded performance.</summary>
    Degraded,

    /// <summary>Scanner is not operational.</summary>
    Unhealthy
}

/// <summary>
/// Provides comprehensive information about the virus scanner implementation.
/// </summary>
public sealed class ScannerInfo
{
    /// <summary>
    /// Gets or sets the product name of the virus scanner.
    /// </summary>
    public required string ProductName { get; init; }

    /// <summary>
    /// Gets or sets the version of the virus scanner.
    /// </summary>
    public required string ProductVersion { get; init; }

    /// <summary>
    /// Gets or sets the vendor/manufacturer of the scanner.
    /// </summary>
    public required string VendorName { get; init; }

    /// <summary>
    /// Gets or sets the type or category of the scanner technology.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// - "Signature-based Antivirus"
    /// - "Cloud-based Scanner"
    /// - "Behavioral Analysis Engine"
    /// - "Machine Learning Scanner"
    /// </remarks>
    public required string ScannerType { get; init; }

    /// <summary>
    /// Gets or sets the supported file formats and MIME types.
    /// </summary>
    public required IReadOnlyCollection<string> SupportedFormats { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the detection capabilities of the scanner.
    /// </summary>
    public required IReadOnlyCollection<string> DetectionCapabilities { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the maximum file size in MB that can be scanned.
    /// </summary>
    /// <remarks>
    /// Indicates the upper limit for file sizes that the scanner can process.
    /// Important for validation and user guidance.
    /// </remarks>
    public int MaxFileSizeMB { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the scanner supports concurrent scanning.
    /// </summary>
    /// <remarks>
    /// Indicates whether multiple files can be scanned simultaneously.
    /// Important for performance planning and resource allocation.
    /// </remarks>
    public bool SupportsConcurrentScanning { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the scanner supports stream-based scanning.
    /// </summary>
    /// <remarks>
    /// Indicates whether the scanner can process file streams directly
    /// without requiring file system storage.
    /// </remarks>
    public bool SupportsStreamScanning { get; init; }

    /// <summary>
    /// Gets or sets detailed configuration information about the scanner.
    /// </summary>
    /// <remarks>
    /// Contains implementation-specific configuration details that may be
    /// useful for monitoring, troubleshooting, and integration planning.
    /// </remarks>
    public required Dictionary<string, object> ConfigurationDetails { get; init; } = new();
}