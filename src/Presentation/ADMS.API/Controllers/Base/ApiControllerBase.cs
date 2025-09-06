using ADMS.API.Extensions;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Controllers.Base;

/// <summary>
/// Base controller providing common functionality for all API controllers.
/// </summary>
/// <remarks>
/// This base controller encapsulates common patterns used across all controllers including:
/// - Standardized error response creation
/// - Common validation patterns
/// - Logging patterns with correlation ID support
/// - Response header management
/// - Problem details creation using factory pattern
/// 
/// All controllers should inherit from this base class to ensure consistency
/// and reduce code duplication across the API surface.
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json", "application/xml")]
public abstract class ApiControllerBase : ControllerBase
{
    private readonly ILogger _logger;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiControllerBase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for this controller.</param>
    /// <param name="problemDetailsFactory">The factory for creating problem details responses.</param>
    protected ApiControllerBase(ILogger logger, ProblemDetailsFactory problemDetailsFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
    }

    /// <summary>
    /// Gets the logger instance for this controller.
    /// </summary>
    protected ILogger Logger => _logger;

    /// <summary>
    /// Gets the correlation ID for the current request.
    /// </summary>
    protected string? CorrelationId => HttpContext.Items["CorrelationId"]?.ToString();

    #region Common Response Methods

    /// <summary>
    /// Creates a standardized internal server error response.
    /// </summary>
    /// <param name="operationContext">A description of the operation that failed.</param>
    /// <param name="exception">The optional exception that caused the error.</param>
    /// <returns>A standardized 500 Internal Server Error response.</returns>
    protected ObjectResult CreateInternalServerErrorResponse(string operationContext, Exception? exception = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationContext);

        var correlationId = CorrelationId;

        if (exception != null)
        {
            _logger.LogError(exception,
                "Internal server error occurred while {OperationContext}. CorrelationId: {CorrelationId}",
                operationContext, correlationId);
        }
        else
        {
            _logger.LogError(
                "Internal server error occurred while {OperationContext}. CorrelationId: {CorrelationId}",
                operationContext, correlationId);
        }

        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            HttpContext,
            StatusCodes.Status500InternalServerError,
            title: "An unexpected error occurred",
            detail: $"An unexpected error occurred while {operationContext}. Please try again later.");

        AddCommonResponseHeaders();
        AddCorrelationIdToProblemDetails(problemDetails);

        return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
    }

    /// <summary>
    /// Creates a standardized problem details response.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="title">The problem title.</param>
    /// <param name="detail">The problem detail message.</param>
    /// <param name="instance">The optional problem instance URI.</param>
    /// <returns>A problem details response.</returns>
    protected ObjectResult CreateProblemDetailsResponse(int statusCode, string title, string detail, string? instance = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(detail);

        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            HttpContext,
            statusCode,
            title: title,
            detail: detail,
            instance: instance);

        AddCommonResponseHeaders();
        AddCorrelationIdToProblemDetails(problemDetails);

        return StatusCode(statusCode, problemDetails);
    }

    /// <summary>
    /// Creates a standardized validation problem response.
    /// </summary>
    /// <param name="validationResults">The validation results.</param>
    /// <param name="title">The problem title.</param>
    /// <returns>A BadRequest response with validation problem details.</returns>
    protected BadRequestObjectResult CreateValidationProblemResponse(
        IEnumerable<ValidationResult> validationResults,
        string title = "One or more validation errors occurred")
    {
        ArgumentNullException.ThrowIfNull(validationResults);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var errors = validationResults
            .GroupBy(vr => vr.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(
                g => g.Key,
                g => g.Select(vr => vr.ErrorMessage ?? string.Empty).ToArray()
            );

        var validationProblemDetails = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = title,
            Detail = "See the errors property for validation details.",
            Instance = HttpContext.Request.Path
        };

        AddCommonResponseHeaders();
        AddCorrelationIdToProblemDetails(validationProblemDetails);

        _logger.LogWarning(
            "Validation failed. Title: {Title}, Errors: {@Errors}, CorrelationId: {CorrelationId}",
            title, errors, CorrelationId);

        return BadRequest(validationProblemDetails);
    }

    /// <summary>
    /// Creates a successful response with standard headers.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="data">The response data.</param>
    /// <param name="statusCode">The HTTP status code (default: 200 OK).</param>
    /// <returns>A successful response with the specified data.</returns>
    protected ObjectResult CreateSuccessResponse<T>(T data, int statusCode = StatusCodes.Status200OK)
    {
        AddCommonResponseHeaders();
        return StatusCode(statusCode, data);
    }

    /// <summary>
    /// Creates a successful response for created resources.
    /// </summary>
    /// <typeparam name="T">The type of the created resource.</typeparam>
    /// <param name="routeName">The route name for the created resource.</param>
    /// <param name="routeValues">The route values for the created resource.</param>
    /// <param name="data">The created resource data.</param>
    /// <returns>A 201 Created response with location header.</returns>
    protected CreatedAtRouteResult CreateCreatedResponse<T>(string routeName, object routeValues, T data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeName);
        ArgumentNullException.ThrowIfNull(routeValues);

        AddCommonResponseHeaders();

        _logger.LogInformation(
            "Resource created successfully. Route: {RouteName}, CorrelationId: {CorrelationId}",
            routeName, CorrelationId);

        return CreatedAtRoute(routeName, routeValues, data);
    }

    /// <summary>
    /// Creates a No Content response with standard headers.
    /// </summary>
    /// <returns>A 204 No Content response.</returns>
    protected NoContentResult CreateNoContentResponse()
    {
        AddCommonResponseHeaders();
        return NoContent();
    }

    #endregion

    #region Validation Helper Methods

    /// <summary>
    /// Validates required parameters and returns appropriate error responses.
    /// </summary>
    /// <param name="validations">A collection of validation functions that return error responses if validation fails.</param>
    /// <returns>The first validation error found, or null if all validations pass.</returns>
    protected async Task<ActionResult?> ValidateParametersAsync(params Func<Task<ActionResult?>>[] validations)
    {
        ArgumentNullException.ThrowIfNull(validations);

        foreach (var validation in validations)
        {
            var result = await validation();
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Validates a GUID parameter.
    /// </summary>
    /// <param name="id">The GUID to validate.</param>
    /// <param name="parameterName">The parameter name for error reporting.</param>
    /// <returns>A BadRequest result if invalid, null if valid.</returns>
    protected ActionResult? ValidateGuidParameter(Guid id, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        if (id == Guid.Empty)
        {
            _logger.LogWarning(
                "Invalid GUID parameter: {ParameterName}. CorrelationId: {CorrelationId}",
                parameterName, CorrelationId);

            return CreateProblemDetailsResponse(
                StatusCodes.Status400BadRequest,
                "Invalid Parameter",
                $"The {parameterName} parameter cannot be empty.");
        }

        return null;
    }

    /// <summary>
    /// Validates that a required object is not null.
    /// </summary>
    /// <param name="obj">The object to validate.</param>
    /// <param name="parameterName">The parameter name for error reporting.</param>
    /// <returns>A BadRequest result if null, null if valid.</returns>
    protected ActionResult? ValidateNotNull(object? obj, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        if (obj == null)
        {
            _logger.LogWarning(
                "Required parameter is null: {ParameterName}. CorrelationId: {CorrelationId}",
                parameterName, CorrelationId);

            return CreateProblemDetailsResponse(
                StatusCodes.Status400BadRequest,
                "Missing Required Data",
                $"The {parameterName} is required and cannot be null.");
        }

        return null;
    }

    #endregion

    #region Logging Helper Methods

    /// <summary>
    /// Logs an information message with correlation ID.
    /// </summary>
    /// <param name="message">The log message template.</param>
    /// <param name="args">The message arguments.</param>
    protected void LogInformation(string message, params object[] args)
    {
        var correlationId = CorrelationId;
        var enrichedArgs = args.Append(correlationId).ToArray();
        var enrichedMessage = $"{message} CorrelationId: {{CorrelationId}}";

        _logger.LogInformation(enrichedMessage, enrichedArgs);
    }

    /// <summary>
    /// Logs a warning message with correlation ID.
    /// </summary>
    /// <param name="message">The log message template.</param>
    /// <param name="args">The message arguments.</param>
    protected void LogWarning(string message, params object[] args)
    {
        var correlationId = CorrelationId;
        var enrichedArgs = args.Append(correlationId).ToArray();
        var enrichedMessage = $"{message} CorrelationId: {{CorrelationId}}";

        _logger.LogWarning(enrichedMessage, enrichedArgs);
    }

    /// <summary>
    /// Logs an error message with correlation ID.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The log message template.</param>
    /// <param name="args">The message arguments.</param>
    protected void LogError(Exception exception, string message, params object[] args)
    {
        var correlationId = CorrelationId;
        var enrichedArgs = args.Append(correlationId).ToArray();
        var enrichedMessage = $"{message} CorrelationId: {{CorrelationId}}";

        _logger.LogError(exception, enrichedMessage, enrichedArgs);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Adds common response headers to all responses.
    /// </summary>
    private void AddCommonResponseHeaders()
    {
        Response.AddApiVersion("1.0");
        Response.AddCorrelationId(CorrelationId);

        // Add security headers
        Response.Headers.Append("X-Content-Type-Options", "nosniff");
        Response.Headers.Append("X-Frame-Options", "DENY");
    }

    /// <summary>
    /// Adds correlation ID to problem details if available.
    /// </summary>
    /// <param name="problemDetails">The problem details object.</param>
    private void AddCorrelationIdToProblemDetails(ProblemDetails problemDetails)
    {
        ArgumentNullException.ThrowIfNull(problemDetails);

        problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;

        if (!string.IsNullOrWhiteSpace(CorrelationId))
        {
            problemDetails.Extensions["correlationId"] = CorrelationId;
        }
    }

    #endregion
}

/// <summary>
/// Specialized base controller for controllers that work with paginated data.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TDto">The DTO type.</typeparam>
/// <typeparam name="TResourceParameters">The resource parameters type.</typeparam>
public abstract class PaginatedApiControllerBase<TEntity, TDto, TResourceParameters> : ApiControllerBase
    where TResourceParameters : PagedResourceParameters
{
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedApiControllerBase{TEntity, TDto, TResourceParameters}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="problemDetailsFactory">The problem details factory.</param>
    /// <param name="mapper">The object mapper.</param>
    protected PaginatedApiControllerBase(
        ILogger logger,
        ProblemDetailsFactory problemDetailsFactory,
        IMapper mapper)
        : base(logger, problemDetailsFactory)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Gets the object mapper.
    /// </summary>
    protected IMapper Mapper => _mapper;

    /// <summary>
    /// Creates a paginated response with appropriate headers.
    /// </summary>
    /// <param name="pagedEntities">The paged entities.</param>
    /// <returns>A successful response with pagination metadata.</returns>
    protected ActionResult<IEnumerable<TDto>> CreatePaginatedResponse(PagedList<TEntity> pagedEntities)
    {
        ArgumentNullException.ThrowIfNull(pagedEntities);

        // Add pagination metadata to response headers
        Response.AddPaginationMetadata(pagedEntities);

        // Map entities to DTOs
        var dtos = _mapper.Map<IEnumerable<TDto>>(pagedEntities);

        LogInformation(
            "Retrieved {Count} {EntityType} items (Page {CurrentPage}/{TotalPages})",
            pagedEntities.Count, typeof(TEntity).Name, pagedEntities.CurrentPage, pagedEntities.TotalPages);

        return CreateSuccessResponse(dtos);
    }

    /// <summary>
    /// Validates resource parameters for pagination.
    /// </summary>
    /// <param name="resourceParameters">The resource parameters to validate.</param>
    /// <returns>A validation error response if invalid, null if valid.</returns>
    protected ActionResult? ValidateResourceParameters(TResourceParameters? resourceParameters)
    {
        if (resourceParameters == null)
        {
            return CreateProblemDetailsResponse(
                StatusCodes.Status400BadRequest,
                "Missing Parameters",
                "Resource parameters are required.");
        }

        if (resourceParameters.PageNumber <= 0)
        {
            return CreateProblemDetailsResponse(
                StatusCodes.Status400BadRequest,
                "Invalid Page Number",
                "Page number must be greater than zero.");
        }

        if (resourceParameters.PageSize <= 0)
        {
            return CreateProblemDetailsResponse(
                StatusCodes.Status400BadRequest,
                "Invalid Page Size",
                "Page size must be greater than zero.");
        }

        return null;
    }
}