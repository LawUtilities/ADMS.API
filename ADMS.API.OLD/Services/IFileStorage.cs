using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Services;

/// <summary>
/// Defines the contract for enterprise-grade file storage services providing comprehensive file management capabilities.
/// </summary>
/// <remarks>
/// This interface provides a comprehensive contract for file storage implementations including:
/// 
/// Core File Storage Capabilities:
/// - Asynchronous file saving and retrieval operations with cancellation support
/// - High-performance file operations optimized for large file processing
/// - Comprehensive error handling and detailed diagnostic information
/// - Support for multiple storage backends (local, cloud, network, database)
/// - Thread-safe operations for concurrent file management scenarios
/// 
/// Storage Backend Support:
/// - Local file system storage with directory management
/// - Cloud storage integration (Azure Blob, AWS S3, Google Cloud)
/// - Network attached storage (NAS) and shared file systems
/// - Database binary large object (BLOB) storage
/// - Hybrid storage configurations with failover support
/// 
/// Security and Access Control:
/// - Secure file handling with proper permission validation
/// - Integration with access control and authorization systems
/// - File encryption support for sensitive document storage
/// - Audit logging for all file operations and access attempts
/// - Compliance support for data protection and retention policies
/// 
/// Performance and Scalability:
/// - Asynchronous operations for non-blocking file processing
/// - Cancellation token support for responsive user experience
/// - Stream-based processing for memory-efficient large file handling
/// - Configurable timeout and resource management
/// - Optimized operations for high-throughput scenarios
/// 
/// File Management Features:
/// - Atomic file operations with rollback capabilities
/// - File versioning and revision management support
/// - Metadata management and file classification
/// - Duplicate detection and content deduplication
/// - Temporary file management and cleanup
/// 
/// Integration Capabilities:
/// - Seamless integration with document management systems
/// - Support for file upload and download pipelines
/// - Integration with virus scanning and content validation
/// - Compatibility with backup and archival systems
/// - API integration for external storage services
/// 
/// Error Handling and Resilience:
/// - Comprehensive error handling with detailed diagnostic information
/// - Retry policies and failure recovery mechanisms
/// - Storage health monitoring and availability checking
/// - Graceful degradation for storage service unavailability
/// - Transaction support for complex file operations
/// 
/// Implementation Considerations:
/// - Implementations should handle various storage backend types
/// - Proper resource management and cleanup of temporary files
/// - Thread-safe operations for concurrent file access scenarios
/// - Integration with logging infrastructure for operational monitoring
/// - Configuration support for storage policies and settings
/// 
/// Common Usage Scenarios:
/// - Document storage and retrieval in content management systems
/// - File upload processing in web applications and APIs
/// - Backup and archival operations for data protection
/// - Temporary file management for processing workflows
/// - Media storage and delivery for multimedia applications
/// 
/// The interface is designed to be:
/// - Extensible: Easy to implement with various storage technologies
/// - Performant: Asynchronous operations with cancellation support
/// - Secure: Comprehensive security and access control integration
/// - Observable: Rich diagnostic information for monitoring and compliance
/// - Reliable: Robust error handling and graceful degradation capabilities
/// </remarks>
/// <example>
/// <code>
/// // Basic file storage implementation
/// public class MyFileStorageService : IFileStorage
/// {
///     public async Task&lt;FileOperationResult&gt; SaveFileAsync(
///         string path, 
///         byte[] content, 
///         CancellationToken cancellationToken = default)
///     {
///         try
///         {
///             await File.WriteAllBytesAsync(path, content, cancellationToken);
///             return FileOperationResult.Success(path, content.Length);
///         }
///         catch (Exception ex)
///         {
///             return FileOperationResult.Failed(ex.Message, ex);
///         }
///     }
/// }
/// 
/// // Usage in document upload controller
/// [HttpPost("upload")]
/// public async Task&lt;IActionResult&gt; UploadDocument(
///     IFormFile file, 
///     CancellationToken cancellationToken)
/// {
///     using var stream = new MemoryStream();
///     await file.CopyToAsync(stream, cancellationToken);
///     var content = stream.ToArray();
///     
///     var filePath = Path.Combine("documents", Guid.NewGuid() + Path.GetExtension(file.FileName));
///     var result = await _fileStorage.SaveFileAsync(filePath, content, cancellationToken);
///     
///     if (result.IsSuccess)
///     {
///         return Ok(new { FilePath = result.FilePath, Size = result.FileSize });
///     }
///     else
///     {
///         _logger.LogError("File upload failed: {Error}", result.ErrorMessage);
///         return StatusCode(500, "File upload failed");
///     }
/// }
/// </code>
/// </example>
public interface IFileStorage
{
    #region Core File Operations

    /// <summary>
    /// Asynchronously saves the specified content to a file at the given path with comprehensive error handling and validation.
    /// </summary>
    /// <param name="path">
    /// The full file path where the content will be saved.
    /// Must be a valid file path that is accessible for write operations.
    /// Directory structure will be created automatically if it doesn't exist.
    /// Path must not contain invalid characters or exceed system path length limits.
    /// </param>
    /// <param name="content">
    /// The byte array containing the content to be written to the file.
    /// Must not be null and should contain the complete file data.
    /// Large files will be handled efficiently with appropriate memory management.
    /// Content will be written atomically to prevent partial file corruption.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to allow cancellation of long-running save operations.
    /// Implementations should respect cancellation requests to provide responsive user experience.
    /// Default value allows the operation to run to completion without cancellation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous save operation.
    /// The task result contains a FileOperationResult with comprehensive information about:
    /// - Success or failure status of the save operation
    /// - File path and size information for successful operations
    /// - Detailed error information and recommended actions for failures
    /// - Operation metadata including timing and storage backend information
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive file saving capabilities including:
    /// 
    /// File Operation Features:
    /// - Atomic file writing to prevent partial file corruption during failures
    /// - Automatic directory creation if the target directory doesn't exist
    /// - File overwrite handling with proper backup and recovery options
    /// - Large file support with memory-efficient streaming operations
    /// - Concurrent file access coordination and lock management
    /// 
    /// Error Handling and Validation:
    /// - Comprehensive input validation for path and content parameters
    /// - Storage space validation to ensure sufficient disk space availability
    /// - Permission validation for write access to the target location
    /// - File system error handling with detailed diagnostic information
    /// - Recovery recommendations for various failure scenarios
    /// 
    /// Security and Access Control:
    /// - Path validation to prevent directory traversal attacks
    /// - File permission management and access control integration
    /// - Content validation and virus scanning integration where required
    /// - Audit logging for all file save operations and security events
    /// - Integration with organizational security policies and compliance
    /// 
    /// Performance and Scalability:
    /// - Asynchronous operation to prevent blocking of calling threads
    /// - Memory-efficient handling of large files with streaming support
    /// - Optimized file I/O operations for high-throughput scenarios
    /// - Cancellation token support for responsive cancellation handling
    /// - Resource management and cleanup for temporary files and streams
    /// 
    /// Storage Backend Integration:
    /// - Support for local file system storage with directory management
    /// - Cloud storage integration with proper error handling and retry logic
    /// - Network storage support with connectivity monitoring and failover
    /// - Database storage integration for binary large object handling
    /// - Hybrid storage configurations with automatic backend selection
    /// 
    /// Operational Monitoring:
    /// - Comprehensive logging of file save operations for audit and debugging
    /// - Performance metrics collection for monitoring and optimization
    /// - Error reporting with detailed context for troubleshooting
    /// - Integration with application performance monitoring systems
    /// - Storage health monitoring and capacity management
    /// 
    /// The method handles common scenarios including:
    /// - New file creation with proper directory structure setup
    /// - File overwriting with backup and recovery capabilities
    /// - Large file processing with memory-efficient operations
    /// - Concurrent file access with proper synchronization
    /// - Storage failure scenarios with error recovery and reporting
    /// 
    /// Implementation Requirements:
    /// - Must validate all input parameters and provide detailed error information
    /// - Should create target directories automatically if they don't exist
    /// - Must handle file system errors gracefully with proper error reporting
    /// - Should implement atomic file operations to prevent corruption
    /// - Must support cancellation tokens for responsive user experience
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when path or content parameters are null.
    /// Implementations should validate input parameters and return appropriate FileOperationResult.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when path contains invalid characters or is in an invalid format.
    /// Implementations should validate path format and return appropriate FileOperationResult.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the cancellation token.
    /// Implementations should properly handle cancellation requests during file operations.
    /// </exception>
    /// <example>
    /// <code>
    /// // Example implementation with comprehensive error handling
    /// public async Task&lt;FileOperationResult&gt; SaveFileAsync(
    ///     string path, 
    ///     byte[] content, 
    ///     CancellationToken cancellationToken = default)
    /// {
    ///     try
    ///     {
    ///         // Validate input parameters
    ///         ArgumentException.ThrowIfNullOrWhiteSpace(path, nameof(path));
    ///         ArgumentNullException.ThrowIfNull(content, nameof(content));
    ///         
    ///         // Validate path security
    ///         if (!IsValidPath(path))
    ///             return FileOperationResult.Failed("Invalid file path", "Path contains invalid characters or patterns");
    ///         
    ///         // Create directory if needed
    ///         var directory = Path.GetDirectoryName(path);
    ///         if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
    ///         {
    ///             Directory.CreateDirectory(directory);
    ///         }
    ///         
    ///         var startTime = DateTime.UtcNow;
    ///         
    ///         // Atomic file save operation
    ///         await File.WriteAllBytesAsync(path, content, cancellationToken);
    ///         
    ///         var endTime = DateTime.UtcNow;
    ///         
    ///         _logger.LogInformation("File saved successfully: {Path}, Size: {Size} bytes, Duration: {Duration}ms",
    ///             path, content.Length, (endTime - startTime).TotalMilliseconds);
    ///         
    ///         return FileOperationResult.Success(path, content.Length, endTime - startTime);
    ///     }
    ///     catch (OperationCanceledException)
    ///     {
    ///         _logger.LogInformation("File save operation was canceled for path: {Path}", path);
    ///         return FileOperationResult.Canceled("File save operation was canceled");
    ///     }
    ///     catch (DirectoryNotFoundException ex)
    ///     {
    ///         _logger.LogError(ex, "Directory not found for file save: {Path}", path);
    ///         return FileOperationResult.Failed("Directory not found", ex.Message);
    ///     }
    ///     catch (UnauthorizedAccessException ex)
    ///     {
    ///         _logger.LogError(ex, "Access denied saving file: {Path}", path);
    ///         return FileOperationResult.Failed("Access denied", "Insufficient permissions to save file");
    ///     }
    ///     catch (IOException ex)
    ///     {
    ///         _logger.LogError(ex, "IO error saving file: {Path}", path);
    ///         return FileOperationResult.Failed("File system error", ex.Message);
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         _logger.LogError(ex, "Unexpected error saving file: {Path}", path);
    ///         return FileOperationResult.Failed("Unexpected error", ex.Message);
    ///     }
    /// }
    /// 
    /// // Usage in document processing pipeline
    /// public async Task&lt;ProcessingResult&gt; ProcessDocumentUpload(
    ///     Stream fileStream,
    ///     string fileName,
    ///     CancellationToken cancellationToken = default)
    /// {
    ///     using var memoryStream = new MemoryStream();
    ///     await fileStream.CopyToAsync(memoryStream, cancellationToken);
    ///     var content = memoryStream.ToArray();
    ///     
    ///     var filePath = Path.Combine(_storageConfig.BasePath, "documents", fileName);
    ///     var saveResult = await _fileStorage.SaveFileAsync(filePath, content, cancellationToken);
    ///     
    ///     if (saveResult.IsSuccess)
    ///     {
    ///         _logger.LogInformation("Document saved: {FileName}, Path: {Path}, Size: {Size}",
    ///             fileName, saveResult.FilePath, saveResult.FileSize);
    ///         return ProcessingResult.Success(saveResult.FilePath);
    ///     }
    ///     else
    ///     {
    ///         _logger.LogError("Document save failed: {FileName}, Error: {Error}",
    ///             fileName, saveResult.ErrorMessage);
    ///         return ProcessingResult.Failed(saveResult.ErrorMessage);
    ///     }
    /// }
    /// </code>
    /// </example>
    Task<FileOperationResult> SaveFileAsync(string path, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously saves content from a stream to a file at the given path with memory-efficient processing.
    /// </summary>
    /// <param name="path">
    /// The full file path where the stream content will be saved.
    /// Must be a valid file path that is accessible for write operations.
    /// Directory structure will be created automatically if it doesn't exist.
    /// </param>
    /// <param name="contentStream">
    /// The stream containing the content to be written to the file.
    /// Stream must be readable and positioned at the beginning of the content.
    /// Stream will be consumed completely and should remain open during the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to allow cancellation of the save operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous save operation with comprehensive result information.
    /// </returns>
    /// <remarks>
    /// This method provides memory-efficient file saving for large files by:
    /// - Using stream-based operations to minimize memory usage
    /// - Supporting files larger than available system memory
    /// - Providing optimal performance for large file transfers
    /// - Maintaining low memory footprint during processing
    /// </remarks>
    /// <example>
    /// <code>
    /// // Stream-based file saving for large files
    /// using var fileStream = File.OpenRead(sourceFile);
    /// var result = await _fileStorage.SaveFileAsync(targetPath, fileStream, cancellationToken);
    /// 
    /// if (result.IsSuccess)
    /// {
    ///     Console.WriteLine($"Large file saved: {result.FileSize} bytes");
    /// }
    /// </code>
    /// </example>
    Task<FileOperationResult> SaveFileAsync(string path, Stream contentStream, CancellationToken cancellationToken = default);

    #endregion Core File Operations

    #region File Retrieval Operations

    /// <summary>
    /// Asynchronously retrieves file content as a byte array from the specified path.
    /// </summary>
    /// <param name="path">
    /// The full file path from which to read the content.
    /// Must be a valid existing file path that is accessible for read operations.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to allow cancellation of the read operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous read operation with file content and metadata.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive file retrieval including:
    /// - Atomic file reading to ensure data consistency
    /// - File existence validation before reading
    /// - Large file support with memory management
    /// - Comprehensive error handling for various failure scenarios
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await _fileStorage.GetFileAsync(filePath, cancellationToken);
    /// if (result.IsSuccess)
    /// {
    ///     var content = result.Content;
    ///     // Process file content...
    /// }
    /// </code>
    /// </example>
    Task<FileRetrievalResult> GetFileAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously opens a stream to read file content from the specified path.
    /// </summary>
    /// <param name="path">
    /// The full file path from which to open the stream.
    /// Must be a valid existing file path that is accessible for read operations.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to allow cancellation of the stream opening operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous stream opening operation.
    /// Returns a FileStreamResult containing the stream and metadata.
    /// </returns>
    /// <remarks>
    /// This method provides memory-efficient file reading for large files by:
    /// - Opening a stream without loading entire file into memory
    /// - Supporting files larger than available system memory
    /// - Providing optimal performance for large file operations
    /// - Maintaining low memory footprint during processing
    /// 
    /// Important: The caller is responsible for disposing the returned stream.
    /// </remarks>
    /// <example>
    /// <code>
    /// var streamResult = await _fileStorage.GetFileStreamAsync(filePath, cancellationToken);
    /// if (streamResult.IsSuccess)
    /// {
    ///     using var stream = streamResult.Stream;
    ///     // Process stream content...
    /// }
    /// </code>
    /// </example>
    Task<FileStreamResult> GetFileStreamAsync(string path, CancellationToken cancellationToken = default);

    #endregion File Retrieval Operations

    #region File Management Operations

    /// <summary>
    /// Asynchronously checks whether a file exists at the specified path.
    /// </summary>
    /// <param name="path">
    /// The full file path to check for existence.
    /// Must be a valid file path format.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to allow cancellation of the existence check.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous existence check operation.
    /// Returns true if the file exists and is accessible; false otherwise.
    /// </returns>
    /// <remarks>
    /// This method provides reliable file existence checking with:
    /// - Path validation and security checking
    /// - Access permission verification
    /// - Network storage connectivity validation
    /// - Comprehensive error handling for inaccessible files
    /// </remarks>
    /// <example>
    /// <code>
    /// bool exists = await _fileStorage.FileExistsAsync(filePath, cancellationToken);
    /// if (exists)
    /// {
    ///     // File exists - proceed with operations
    /// }
    /// </code>
    /// </example>
    Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes a file at the specified path.
    /// </summary>
    /// <param name="path">
    /// The full file path of the file to delete.
    /// Must be a valid existing file path that is accessible for delete operations.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to allow cancellation of the delete operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous delete operation with result information.
    /// </returns>
    /// <remarks>
    /// This method provides secure file deletion including:
    /// - File existence verification before deletion
    /// - Permission validation for delete operations
    /// - Atomic deletion to prevent partial file removal
    /// - Audit logging for security and compliance
    /// - Recovery recommendations for failure scenarios
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await _fileStorage.DeleteFileAsync(filePath, cancellationToken);
    /// if (result.IsSuccess)
    /// {
    ///     _logger.LogInformation("File deleted successfully: {Path}", result.FilePath);
    /// }
    /// </code>
    /// </example>
    Task<FileOperationResult> DeleteFileAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously gets metadata information about a file at the specified path.
    /// </summary>
    /// <param name="path">
    /// The full file path for which to retrieve metadata.
    /// Must be a valid existing file path that is accessible for read operations.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to allow cancellation of the metadata retrieval.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous metadata retrieval operation.
    /// Returns comprehensive file metadata including size, dates, and attributes.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive file metadata including:
    /// - File size and content information
    /// - Creation, modification, and access timestamps
    /// - File attributes and permissions
    /// - Storage backend specific metadata
    /// - Checksum and integrity information where available
    /// </remarks>
    /// <example>
    /// <code>
    /// var metadata = await _fileStorage.GetFileMetadataAsync(filePath, cancellationToken);
    /// if (metadata.IsSuccess)
    /// {
    ///     Console.WriteLine($"File size: {metadata.FileSize} bytes, Modified: {metadata.LastModified}");
    /// }
    /// </code>
    /// </example>
    Task<FileMetadataResult> GetFileMetadataAsync(string path, CancellationToken cancellationToken = default);

    #endregion File Management Operations

    #region Storage Health and Diagnostics

    /// <summary>
    /// Asynchronously validates whether the storage system is healthy and operational.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token for the health check operation.</param>
    /// <returns>
    /// A task that represents the asynchronous health check operation.
    /// The task result contains a StorageHealthResult with comprehensive health information.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive storage health checking including:
    /// - Storage backend connectivity and availability
    /// - Disk space and capacity monitoring
    /// - Performance metrics and response times
    /// - Configuration validation and system requirements
    /// - Error rates and reliability assessment
    /// </remarks>
    /// <example>
    /// <code>
    /// var health = await _fileStorage.CheckStorageHealthAsync();
    /// if (health.IsHealthy)
    /// {
    ///     _logger.LogInformation("Storage is healthy - Available space: {AvailableSpace} GB", 
    ///         health.AvailableSpaceGB);
    /// }
    /// </code>
    /// </example>
    Task<StorageHealthResult> CheckStorageHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves comprehensive information about the storage system configuration and capabilities.
    /// </summary>
    /// <returns>
    /// A StorageInfo object containing detailed information about the storage implementation.
    /// </returns>
    /// <remarks>
    /// This method provides static information about the storage system including:
    /// - Storage backend type and configuration details
    /// - Supported features and capabilities
    /// - Performance characteristics and limitations
    /// - Security features and access control mechanisms
    /// </remarks>
    /// <example>
    /// <code>
    /// var storageInfo = _fileStorage.GetStorageInfo();
    /// _logger.LogInformation("Storage backend: {Backend}, Max file size: {MaxSize} MB",
    ///     storageInfo.BackendType, storageInfo.MaxFileSizeMB);
    /// </code>
    /// </example>
    StorageInfo GetStorageInfo();

    #endregion Storage Health and Diagnostics
}

#region Result Types and Supporting Classes

/// <summary>
/// Represents the result of a file operation with comprehensive status and metadata information.
/// </summary>
public sealed class FileOperationResult
{
    /// <summary>
    /// Gets a value indicating whether the file operation completed successfully.
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the file path that was operated on.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets the size of the file in bytes for successful operations.
    /// </summary>
    public long? FileSize { get; init; }

    /// <summary>
    /// Gets the duration of the file operation.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// Gets the error message for failed operations.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets additional error details for troubleshooting.
    /// </summary>
    public string? ErrorDetails { get; init; }

    /// <summary>
    /// Gets the timestamp when the operation completed.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets additional operation metadata.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Creates a successful file operation result.
    /// </summary>
    public static FileOperationResult Success(string filePath, long fileSize, TimeSpan? duration = null)
    {
        return new FileOperationResult
        {
            IsSuccess = true,
            FilePath = filePath,
            FileSize = fileSize,
            Duration = duration
        };
    }

    /// <summary>
    /// Creates a failed file operation result.
    /// </summary>
    public static FileOperationResult Failed(string errorMessage, string? errorDetails = null)
    {
        return new FileOperationResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorDetails = errorDetails
        };
    }

    /// <summary>
    /// Creates a canceled file operation result.
    /// </summary>
    public static FileOperationResult Canceled(string message = "Operation was canceled")
    {
        return new FileOperationResult
        {
            IsSuccess = false,
            ErrorMessage = message
        };
    }
}

/// <summary>
/// Represents the result of a file retrieval operation with content and metadata.
/// </summary>
public sealed class FileRetrievalResult
{
    /// <summary>
    /// Gets a value indicating whether the file retrieval completed successfully.
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the file content as a byte array for successful retrievals.
    /// </summary>
    public byte[]? Content { get; init; }

    /// <summary>
    /// Gets the file path that was retrieved.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets the size of the retrieved file in bytes.
    /// </summary>
    public long? FileSize { get; init; }

    /// <summary>
    /// Gets the error message for failed retrievals.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets additional metadata about the retrieved file.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Creates a successful file retrieval result.
    /// </summary>
    public static FileRetrievalResult Success(string filePath, byte[] content)
    {
        return new FileRetrievalResult
        {
            IsSuccess = true,
            FilePath = filePath,
            Content = content,
            FileSize = content.Length
        };
    }

    /// <summary>
    /// Creates a failed file retrieval result.
    /// </summary>
    public static FileRetrievalResult Failed(string errorMessage)
    {
        return new FileRetrievalResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Represents the result of opening a file stream with metadata information.
/// </summary>
public sealed class FileStreamResult : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the stream opening completed successfully.
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the opened file stream for successful operations.
    /// </summary>
    public Stream? Stream { get; init; }

    /// <summary>
    /// Gets the file path for the opened stream.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets the size of the file in bytes.
    /// </summary>
    public long? FileSize { get; init; }

    /// <summary>
    /// Gets the error message for failed operations.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful file stream result.
    /// </summary>
    public static FileStreamResult Success(string filePath, Stream stream, long fileSize)
    {
        return new FileStreamResult
        {
            IsSuccess = true,
            FilePath = filePath,
            Stream = stream,
            FileSize = fileSize
        };
    }

    /// <summary>
    /// Creates a failed file stream result.
    /// </summary>
    public static FileStreamResult Failed(string errorMessage)
    {
        return new FileStreamResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    /// Disposes the underlying stream if it exists.
    /// </summary>
    public void Dispose()
    {
        Stream?.Dispose();
    }
}

/// <summary>
/// Represents file metadata information.
/// </summary>
public sealed class FileMetadataResult
{
    /// <summary>
    /// Gets a value indicating whether the metadata retrieval completed successfully.
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the file path.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public long? FileSize { get; init; }

    /// <summary>
    /// Gets the file creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; init; }

    /// <summary>
    /// Gets the file last modification timestamp.
    /// </summary>
    public DateTime? LastModified { get; init; }

    /// <summary>
    /// Gets the file last access timestamp.
    /// </summary>
    public DateTime? LastAccessed { get; init; }

    /// <summary>
    /// Gets file attributes and properties.
    /// </summary>
    public Dictionary<string, object>? Attributes { get; init; }

    /// <summary>
    /// Gets the error message for failed operations.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Represents the health status of the storage system.
/// </summary>
public sealed class StorageHealthResult
{
    /// <summary>
    /// Gets a value indicating whether the storage system is healthy.
    /// </summary>
    public required bool IsHealthy { get; init; }

    /// <summary>
    /// Gets the available storage space in gigabytes.
    /// </summary>
    public double? AvailableSpaceGB { get; init; }

    /// <summary>
    /// Gets the total storage capacity in gigabytes.
    /// </summary>
    public double? TotalCapacityGB { get; init; }

    /// <summary>
    /// Gets the average response time for storage operations.
    /// </summary>
    public TimeSpan? AverageResponseTime { get; init; }

    /// <summary>
    /// Gets the error rate percentage for recent operations.
    /// </summary>
    [Range(0.0, 100.0)]
    public double ErrorRate { get; init; }

    /// <summary>
    /// Gets health check error messages if unhealthy.
    /// </summary>
    public List<string>? HealthIssues { get; init; }

    /// <summary>
    /// Gets additional health information and metrics.
    /// </summary>
    public Dictionary<string, object>? HealthMetrics { get; init; }
}

/// <summary>
/// Provides information about the storage system implementation.
/// </summary>
public sealed class StorageInfo
{
    /// <summary>
    /// Gets the storage backend type (e.g., "FileSystem", "AzureBlob", "AmazonS3").
    /// </summary>
    public required string BackendType { get; init; }

    /// <summary>
    /// Gets the version of the storage implementation.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Gets the maximum file size supported in megabytes.
    /// </summary>
    public long? MaxFileSizeMB { get; init; }

    /// <summary>
    /// Gets the supported features of this storage implementation.
    /// </summary>
    public List<string> SupportedFeatures { get; init; } = new();

    /// <summary>
    /// Gets performance characteristics of the storage system.
    /// </summary>
    public Dictionary<string, object>? PerformanceCharacteristics { get; init; }

    /// <summary>
    /// Gets configuration information about the storage system.
    /// </summary>
    public Dictionary<string, object>? Configuration { get; init; }
}

#endregion Result Types and Supporting Classes