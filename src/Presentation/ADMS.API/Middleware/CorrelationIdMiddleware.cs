using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ADMS.API.Middleware;

/// <summary>
/// Middleware to generate and manage correlation IDs for request tracking.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware to process the HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        // Store in context items for access by other middleware/controllers
        context.Items["CorrelationId"] = correlationId;

        // Add to response headers
        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);

        _logger.LogDebug("Correlation ID {CorrelationId} assigned to request {RequestPath}",
            correlationId, context.Request.Path);

        await _next(context);
    }

    /// <summary>
    /// Gets the correlation ID from the request header or creates a new one.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The correlation ID.</returns>
    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationIdValues) &&
            !string.IsNullOrWhiteSpace(correlationIdValues.FirstOrDefault()))
        {
            return correlationIdValues.First()!;
        }

        return Guid.NewGuid().ToString("D");
    }
}

/// <summary>
/// Middleware to add security headers to HTTP responses.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityHeadersMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware to add security headers.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        var response = context.Response;

        // Remove server header to avoid information disclosure
        response.Headers.Remove("Server");

        // X-Content-Type-Options
        response.Headers.Append("X-Content-Type-Options", "nosniff");

        // X-Frame-Options
        response.Headers.Append("X-Frame-Options", "DENY");

        // X-XSS-Protection
        response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Referrer-Policy
        response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Content-Security-Policy (adjust based on your needs)
        response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self';");

        _logger.LogDebug("Security headers added to response for {RequestPath}", context.Request.Path);

        await _next(context);
    }
}

/// <summary>
/// Middleware for comprehensive request and response logging.
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestResponseLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The application configuration.</param>
    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Invokes the middleware to log request and response information.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var enableDetailedLogging = _configuration.GetValue<bool>("Logging:EnableDetailedRequestResponseLogging", false);

        if (!enableDetailedLogging)
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";

        // Log request
        await LogRequestAsync(context, correlationId);

        // Capture response
        var originalResponseBody = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log response
            await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds, responseBodyStream);

            // Copy response back to original stream
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;
        }
    }

    /// <summary>
    /// Logs request information.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="correlationId">The correlation ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LogRequestAsync(HttpContext context, string correlationId)
    {
        var request = context.Request;
        var requestBody = await ReadRequestBodyAsync(request);

        _logger.LogInformation(
            "REQUEST {CorrelationId}: {Method} {Path} {QueryString} | Headers: {Headers} | Body: {Body}",
            correlationId,
            request.Method,
            request.Path,
            request.QueryString,
            JsonSerializer.Serialize(request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())),
            requestBody);
    }

    /// <summary>
    /// Logs response information.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="elapsedMs">The elapsed time in milliseconds.</param>
    /// <param name="responseBodyStream">The response body stream.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LogResponseAsync(HttpContext context, string correlationId, long elapsedMs, MemoryStream responseBodyStream)
    {
        var response = context.Response;
        var responseBody = await ReadResponseBodyAsync(responseBodyStream);

        _logger.LogInformation(
            "RESPONSE {CorrelationId}: {StatusCode} | Duration: {ElapsedMs}ms | Headers: {Headers} | Body: {Body}",
            correlationId,
            response.StatusCode,
            elapsedMs,
            JsonSerializer.Serialize(response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())),
            responseBody);
    }

    /// <summary>
    /// Reads the request body safely.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>The request body as a string.</returns>
    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!request.Body.CanRead || request.ContentLength == 0)
        {
            return string.Empty;
        }

        try
        {
            request.EnableBuffering();
            request.Body.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Seek(0, SeekOrigin.Begin);

            return body.Length > 1000 ? $"{body[..1000]}... (truncated)" : body;
        }
        catch (Exception)
        {
            return "[Error reading request body]";
        }
    }

    /// <summary>
    /// Reads the response body safely.
    /// </summary>
    /// <param name="responseBodyStream">The response body stream.</param>
    /// <returns>The response body as a string.</returns>
    private static async Task<string> ReadResponseBodyAsync(MemoryStream responseBodyStream)
    {
        try
        {
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseBodyStream, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            return body.Length > 1000 ? $"{body[..1000]}... (truncated)" : body;
        }
        catch (Exception)
        {
            return "[Error reading response body]";
        }
    }
}

/// <summary>
/// Global exception filter for handling unhandled exceptions.
/// </summary>
public class GlobalExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionFilter"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="problemDetailsFactory">The problem details factory.</param>
    /// <param name="environment">The host environment information.</param>
    public GlobalExceptionFilter(
        ILogger<GlobalExceptionFilter> logger,
        ProblemDetailsFactory problemDetailsFactory,
        IHostEnvironment environment)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Handles exceptions asynchronously.
    /// </summary>
    /// <param name="context">The exception context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        var exception = context.Exception;
        var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString();

        _logger.LogError(exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, RequestPath: {RequestPath}",
            correlationId, context.HttpContext.Request.Path);

        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            context.HttpContext,
            statusCode: 500,
            title: "An error occurred while processing your request",
            detail: _environment.IsDevelopment() ? exception.Message : "Please try again later");

        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            problemDetails.Extensions["correlationId"] = correlationId;
        }

        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = 500
        };

        context.ExceptionHandled = true;
    }
}

/// <summary>
/// Model validation filter for standardized validation responses.
/// </summary>
public class ModelValidationFilter : IAsyncActionFilter
{
    private readonly ILogger<ModelValidationFilter> _logger;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelValidationFilter"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="problemDetailsFactory">The problem details factory.</param>
    public ModelValidationFilter(
        ILogger<ModelValidationFilter> logger,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
    }

    /// <summary>
    /// Executes the action filter asynchronously.
    /// </summary>
    /// <param name="context">The action executing context.</param>
    /// <param name="next">The action execution delegate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString();

            _logger.LogWarning(
                "Model validation failed. CorrelationId: {CorrelationId}, Errors: {@Errors}",
                correlationId,
                context.ModelState.Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(x => x.Key, x => x.Value!.Errors.Select(e => e.ErrorMessage)));

            var problemDetails = _problemDetailsFactory.CreateValidationProblemDetails(
                context.HttpContext, context.ModelState);

            problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                problemDetails.Extensions["correlationId"] = correlationId;
            }

            context.Result = new BadRequestObjectResult(problemDetails);
            return;
        }

        await next();
    }
}