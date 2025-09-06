using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.Serialization;
using System.Text.Json;

namespace ADMS.API.ErrorHandling;

/// <summary>
/// Centralized error handling service for consistent error responses across the API.
/// </summary>
public interface IErrorHandlingService
{
    /// <summary>
    /// Handles an exception and returns an appropriate HTTP response.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="context">The HTTP context.</param>
    /// <returns>An appropriate HTTP response for the exception.</returns>
    Task<IActionResult> HandleExceptionAsync(Exception exception, HttpContext context);

    /// <summary>
    /// Creates a standardized error response.
    /// </summary>
    /// <param name="error">The error details.</param>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A standardized error response.</returns>
    IActionResult CreateErrorResponse(ErrorDetails error, HttpContext context);

    /// <summary>
    /// Logs an error with appropriate context and correlation information.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="context">The HTTP context.</param>
    /// <param name="additionalData">Additional data to include in the log.</param>
    Task LogErrorAsync(Exception exception, HttpContext context, Dictionary<string, object?>? additionalData = null);
}

/// <summary>
/// Implementation of centralized error handling service.
/// </summary>
public class ErrorHandlingService : IErrorHandlingService
{
    private readonly ILogger<ErrorHandlingService> _logger;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorHandlingService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="problemDetailsFactory">The problem details factory.</param>
    /// <param name="environment">The host environment.</param>
    /// <param name="configuration">The application configuration.</param>
    public ErrorHandlingService(
        ILogger<ErrorHandlingService> logger,
        ProblemDetailsFactory problemDetailsFactory,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc />
    public async Task<IActionResult> HandleExceptionAsync(Exception exception, HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(context);

        // Log the error with full context
        await LogErrorAsync(exception, context);

        // Determine the appropriate error response based on exception type
        var errorDetails = MapExceptionToErrorDetails(exception);

        return CreateErrorResponse(errorDetails, context);
    }

    /// <inheritdoc />
    public IActionResult CreateErrorResponse(ErrorDetails error, HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(context);

        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            context,
            statusCode: error.StatusCode,
            title: error.Title,
            detail: error.Detail,
            instance: context.Request.Path);

        // Add correlation and trace information
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        if (context.Items.TryGetValue("CorrelationId", out var correlationId))
        {
            problemDetails.Extensions["correlationId"] = correlationId;
        }

        // Add error code for client handling
        if (!string.IsNullOrWhiteSpace(error.ErrorCode))
        {
            problemDetails.Extensions["errorCode"] = error.ErrorCode;
        }

        // Add validation errors if applicable
        if (error.ValidationErrors?.Count > 0)
        {
            problemDetails.Extensions["validationErrors"] = error.ValidationErrors;
        }

        // Add additional context in development
        if (_environment.IsDevelopment() && error.Exception != null)
        {
            problemDetails.Extensions["stackTrace"] = error.Exception.StackTrace;
            problemDetails.Extensions["exceptionType"] = error.Exception.GetType().Name;
        }

        // Add retry information if applicable
        if (error.IsRetryable)
        {
            problemDetails.Extensions["retryable"] = true;
            if (error.RetryAfter.HasValue)
            {
                problemDetails.Extensions["retryAfter"] = error.RetryAfter.Value.TotalSeconds;
                context.Response.Headers.Append("Retry-After", error.RetryAfter.Value.TotalSeconds.ToString());
            }
        }

        return new ObjectResult(problemDetails)
        {
            StatusCode = error.StatusCode
        };
    }

    /// <inheritdoc />
    public async Task LogErrorAsync(Exception exception, HttpContext context, Dictionary<string, object?>? additionalData = null)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
        var requestPath = context.Request.Path.ToString();
        var requestMethod = context.Request.Method;
        var userAgent = context.Request.Headers.UserAgent.FirstOrDefault();
        var remoteIpAddress = context.Connection.RemoteIpAddress?.ToString();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = requestPath,
            ["RequestMethod"] = requestMethod,
            ["UserAgent"] = userAgent ?? "unknown",
            ["RemoteIpAddress"] = remoteIpAddress ?? "unknown",
            ["TraceId"] = context.TraceIdentifier
        });

        var logLevel = DetermineLogLevel(exception);
        var logMessage = "Unhandled exception occurred during request processing";

        var logData = new Dictionary<string, object?>
        {
            ["ExceptionType"] = exception.GetType().Name,
            ["ExceptionMessage"] = exception.Message,
            ["RequestHeaders"] = SerializeHeaders(context.Request.Headers),
            ["ResponseStatusCode"] = context.Response.StatusCode
        };

        // Add additional data if provided
        if (additionalData != null)
        {
            foreach (var (key, value) in additionalData)
            {
                logData[key] = value;
            }
        }

        // Add inner exception details
        if (exception.InnerException != null)
        {
            logData["InnerExceptionType"] = exception.InnerException.GetType().Name;
            logData["InnerExceptionMessage"] = exception.InnerException.Message;
        }

        _logger.Log(logLevel, exception, logMessage + " {@LogData}", logData);

        // Send to external monitoring if configured
        await SendToExternalMonitoringAsync(exception, context, logData);
    }

    /// <summary>
    /// Maps an exception to standardized error details.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>Standardized error details.</returns>
    private ErrorDetails MapExceptionToErrorDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => new ErrorDetails
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = validationEx.Message,
                ErrorCode = "VALIDATION_ERROR",
                Exception = validationEx
            },

            ArgumentException argumentEx => new ErrorDetails
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Title = "Invalid Argument",
                Detail = argumentEx.Message,
                ErrorCode = "INVALID_ARGUMENT",
                Exception = argumentEx
            },

            ArgumentNullException argumentNullEx => new ErrorDetails
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Title = "Missing Required Parameter",
                Detail = $"The parameter '{argumentNullEx.ParamName}' is required.",
                ErrorCode = "MISSING_PARAMETER",
                Exception = argumentNullEx
            },

            UnauthorizedAccessException unauthorizedEx => new ErrorDetails
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Title = "Access Denied",
                Detail = "You do not have permission to access this resource.",
                ErrorCode = "ACCESS_DENIED",
                Exception = unauthorizedEx
            },

            FileNotFoundException fileNotFoundEx => new ErrorDetails
            {
                StatusCode = StatusCodes.Status404NotFound,
                Title = "File Not Found",
                Detail = "The requested file could not be found.",
                ErrorCode = "FILE_NOT_FOUND",
                Exception = fileNotFoundEx
            },

            DirectoryNotFoundException directoryNotFoundEx => new ErrorDetails
            {
                StatusCode = StatusCodes.Status404NotFound,
                Title = "Directory Not Found",
                Detail = "The requested directory could not be found.",
                ErrorCode = "DIRECTORY_NOT_FOUND",
                Exception = directoryNotFoundEx
            },

            TimeoutException timeoutEx => new ErrorDetails
            {
                StatusCode = StatusCodes.Status408RequestTimeout,
                Title = "Request Timeout",
                Detail = "The request timed out. Please try again.",
                ErrorCode = "TIMEOUT",
                IsRetryable = true,
                RetryAfter = TimeSpan.FromSeconds(30),
                Exception = timeoutEx
            },

            InvalidOperationException invalidOpEx when invalidOpEx.Message.Contains("virus", StringComparison.OrdinalIgnoreCase) => new ErrorDetails
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity,
                Title = "File Security Error",
                Detail = "The uploaded file failed security validation.",
                ErrorCode = "FILE_SECURITY_ERROR",
                Exception = invalidOpEx
            },

            NotSupportedException notSupportedEx => new ErrorDetails
            {
                StatusCode = StatusCodes.Status415UnsupportedMediaType,
                Title = "Unsupported Operation",
                Detail = notSupportedEx.Message,
                ErrorCode = "UNSUPPORTED_OPERATION",
                Exception = notSupportedEx
            },

            HttpRequestException httpEx => new ErrorDetails
            {
                StatusCode = StatusCodes.Status502BadGateway,
                Title = "External Service Error",
                Detail = "An error occurred while communicating with an external service.",
                ErrorCode = "EXTERNAL_SERVICE_ERROR",
                IsRetryable = true,
                RetryAfter = TimeSpan.FromMinutes(1),
                Exception = httpEx
            },

            TaskCanceledException cancelledEx when cancelledEx.InnerException is TimeoutException => new ErrorDetails
            {
                StatusCode = StatusCodes.Status408RequestTimeout,
                Title = "Request Timeout",
                Detail = "The request timed out. Please try again.",
                ErrorCode = "TIMEOUT",
                IsRetryable = true,
                RetryAfter = TimeSpan.FromSeconds(30),
                Exception = cancelledEx
            },

            OperationCanceledException cancelledEx => new ErrorDetails
            {
                StatusCode = StatusCodes.Status499ClientClosedRequest,
                Title = "Request Cancelled",
                Detail = "The request was cancelled.",
                ErrorCode = "REQUEST_CANCELLED",
                Exception = cancelledEx
            },

            BusinessRuleViolationException businessEx => new ErrorDetails
            {
                StatusCode = StatusCodes.Status409Conflict,
                Title = "Business Rule Violation",
                Detail = businessEx.Message,
                ErrorCode = businessEx.RuleCode,
                Exception = businessEx
            },

            ConcurrencyException concurrencyEx => new ErrorDetails
            {
                StatusCode = StatusCodes.Status409Conflict,
                Title = "Concurrency Conflict",
                Detail = "The resource has been modified by another user. Please refresh and try again.",
                ErrorCode = "CONCURRENCY_CONFLICT",
                IsRetryable = true,
                Exception = concurrencyEx
            },

            InsufficientStorageException storageEx => new ErrorDetails
            {
                StatusCode = StatusCodes.Status507InsufficientStorage,
                Title = "Insufficient Storage",
                Detail = "There is not enough storage space to complete the operation.",
                ErrorCode = "INSUFFICIENT_STORAGE",
                Exception = storageEx
            },

            _ => new ErrorDetails
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred. Please try again later.",
                ErrorCode = "INTERNAL_ERROR",
                IsRetryable = true,
                RetryAfter = TimeSpan.FromMinutes(5),
                Exception = exception
            }
        };
    }

    /// <summary>
    /// Determines the appropriate log level for an exception.
    /// </summary>
    /// <param name="exception">The exception to analyze.</param>
    /// <returns>The appropriate log level.</returns>
    private static LogLevel DetermineLogLevel(Exception exception)
    {
        return exception switch
        {
            ValidationException => LogLevel.Warning,
            ArgumentException => LogLevel.Warning,
            ArgumentNullException => LogLevel.Warning,
            UnauthorizedAccessException => LogLevel.Warning,
            FileNotFoundException => LogLevel.Warning,
            DirectoryNotFoundException => LogLevel.Warning,
            NotSupportedException => LogLevel.Warning,
            BusinessRuleViolationException => LogLevel.Warning,
            OperationCanceledException => LogLevel.Information,
            _ => LogLevel.Error
        };
    }

    /// <summary>
    /// Serializes HTTP headers for logging, excluding sensitive information.
    /// </summary>
    /// <param name="headers">The headers to serialize.</param>
    /// <returns>A serialized representation of safe headers.</returns>
    private static string SerializeHeaders(IHeaderDictionary headers)
    {
        var sensitiveHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization", "Cookie", "Set-Cookie", "X-API-Key", "X-Auth-Token"
        };

        var safeHeaders = headers
            .Where(h => !sensitiveHeaders.Contains(h.Key))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        return JsonSerializer.Serialize(safeHeaders);
    }

    /// <summary>
    /// Sends error information to external monitoring services.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="context">The HTTP context.</param>
    /// <param name="logData">Additional log data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SendToExternalMonitoringAsync(Exception exception, HttpContext context, Dictionary<string, object?> logData)
    {
        try
        {
            var enableExternalMonitoring = _configuration.GetValue<bool>("Monitoring:EnableExternalErrorReporting", false);
            if (!enableExternalMonitoring)
                return;

            // Implementation would depend on your monitoring service (Application Insights, Sentry, etc.)
            // This is a placeholder for external monitoring integration
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send error to external monitoring service");
        }
    }
}

/// <summary>
/// Represents standardized error details for consistent error responses.
/// </summary>
public class ErrorDetails
{
    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error detail message.
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application-specific error code.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the operation can be retried.
    /// </summary>
    public bool IsRetryable { get; set; }

    /// <summary>
    /// Gets or sets the recommended retry delay.
    /// </summary>
    public TimeSpan? RetryAfter { get; set; }

    /// <summary>
    /// Gets or sets validation errors if applicable.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    /// <summary>
    /// Gets or sets the original exception (for internal use).
    /// </summary>
    public Exception? Exception { get; set; }
}

/// <summary>
/// Custom exception for business rule violations.
/// </summary>
[Serializable]
public class BusinessRuleViolationException : Exception
{
    /// <summary>
    /// Gets the business rule code.
    /// </summary>
    public string RuleCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleViolationException"/> class.
    /// </summary>
    /// <param name="ruleCode">The business rule code.</param>
    /// <param name="message">The exception message.</param>
    public BusinessRuleViolationException(string ruleCode, string message) : base(message)
    {
        RuleCode = ruleCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleViolationException"/> class.
    /// </summary>
    /// <param name="ruleCode">The business rule code.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public BusinessRuleViolationException(string ruleCode, string message, Exception innerException) : base(message, innerException)
    {
        RuleCode = ruleCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleViolationException"/> class.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context.</param>
    protected BusinessRuleViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        RuleCode = info.GetString(nameof(RuleCode)) ?? string.Empty;
    }

    /// <inheritdoc />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(RuleCode), RuleCode);
    }
}

/// <summary>
/// Custom exception for concurrency conflicts.
/// </summary>
[Serializable]
public class ConcurrencyException : Exception
{
    /// <summary>
    /// Gets the entity that caused the concurrency conflict.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Gets the identifier of the entity.
    /// </summary>
    public string EntityId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
    /// </summary>
    /// <param name="entityName">The entity name.</param>
    /// <param name="entityId">The entity identifier.</param>
    public ConcurrencyException(string entityName, string entityId)
        : base($"Concurrency conflict occurred for {entityName} with ID {entityId}")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
    /// </summary>
    /// <param name="entityName">The entity name.</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="message">The exception message.</param>
    public ConcurrencyException(string entityName, string entityId, string message) : base(message)
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context.</param>
    protected ConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        EntityName = info.GetString(nameof(EntityName)) ?? string.Empty;
        EntityId = info.GetString(nameof(EntityId)) ?? string.Empty;
    }

    /// <inheritdoc />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(EntityName), EntityName);
        info.AddValue(nameof(EntityId), EntityId);
    }
}

/// <summary>
/// Custom exception for insufficient storage conditions.
/// </summary>
[Serializable]
public class InsufficientStorageException : Exception
{
    /// <summary>
    /// Gets the required storage space.
    /// </summary>
    public long RequiredSpace { get; }

    /// <summary>
    /// Gets the available storage space.
    /// </summary>
    public long AvailableSpace { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InsufficientStorageException"/> class.
    /// </summary>
    /// <param name="requiredSpace">The required storage space.</param>
    /// <param name="availableSpace">The available storage space.</param>
    public InsufficientStorageException(long requiredSpace, long availableSpace)
        : base($"Insufficient storage: Required {requiredSpace:N0} bytes, Available {availableSpace:N0} bytes")
    {
        RequiredSpace = requiredSpace;
        AvailableSpace = availableSpace;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InsufficientStorageException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public InsufficientStorageException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InsufficientStorageException"/> class.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context.</param>
    protected InsufficientStorageException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        RequiredSpace = info.GetInt64(nameof(RequiredSpace));
        AvailableSpace = info.GetInt64(nameof(AvailableSpace));
    }

    /// <inheritdoc />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(RequiredSpace), RequiredSpace);
        info.AddValue(nameof(AvailableSpace), AvailableSpace);
    }
}

/// <summary>
/// Extension methods for error handling.
/// </summary>
public static class ErrorHandlingExtensions
{
    /// <summary>
    /// Creates a business rule violation exception.
    /// </summary>
    /// <param name="ruleCode">The business rule code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A business rule violation exception.</returns>
    public static BusinessRuleViolationException AsBusinessRuleViolation(this string ruleCode, string message)
    {
        return new BusinessRuleViolationException(ruleCode, message);
    }

    /// <summary>
    /// Creates a concurrency exception.
    /// </summary>
    /// <param name="entityName">The entity name.</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <returns>A concurrency exception.</returns>
    public static ConcurrencyException AsConcurrencyConflict(this string entityName, string entityId)
    {
        return new ConcurrencyException(entityName, entityId);
    }

    /// <summary>
    /// Determines if an exception represents a client error (4xx).
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception represents a client error, false otherwise.</returns>
    public static bool IsClientError(this Exception exception)
    {
        return exception switch
        {
            ValidationException => true,
            ArgumentException => true,
            ArgumentNullException => true,
            UnauthorizedAccessException => true,
            FileNotFoundException => true,
            DirectoryNotFoundException => true,
            NotSupportedException => true,
            BusinessRuleViolationException => true,
            _ => false
        };
    }

    /// <summary>
    /// Determines if an exception represents a server error (5xx).
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception represents a server error, false otherwise.</returns>
    public static bool IsServerError(this Exception exception)
    {
        return !exception.IsClientError() && exception is not OperationCanceledException;
    }
}