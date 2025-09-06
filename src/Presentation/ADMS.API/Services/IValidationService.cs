using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ADMS.API.Services;

/// <summary>
/// Interface for centralized validation services.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates a GUID parameter.
    /// </summary>
    /// <param name="id">The GUID to validate.</param>
    /// <param name="parameterName">The parameter name for error reporting.</param>
    /// <returns>A BadRequest result if invalid, null if valid.</returns>
    ActionResult? ValidateGuid(Guid id, string parameterName);

    /// <summary>
    /// Validates an object using data annotations.
    /// </summary>
    /// <param name="obj">The object to validate.</param>
    /// <returns>A BadRequest result if invalid, null if valid.</returns>
    ActionResult? ValidateObject(object obj);

    /// <summary>
    /// Validates model state.
    /// </summary>
    /// <param name="modelState">The model state to validate.</param>
    /// <returns>A BadRequest result if invalid, null if valid.</returns>
    ActionResult? ValidateModelState(ModelStateDictionary modelState);

    /// <summary>
    /// Validates that a matter exists.
    /// </summary>
    /// <param name="matterId">The matter ID to validate.</param>
    /// <returns>A NotFound result if not found, null if exists.</returns>
    Task<ActionResult?> ValidateMatterExistsAsync(Guid matterId);

    /// <summary>
    /// Validates that a document exists.
    /// </summary>
    /// <param name="documentId">The document ID to validate.</param>
    /// <returns>A NotFound result if not found, null if exists.</returns>
    Task<ActionResult?> ValidateDocumentExistsAsync(Guid documentId);

    /// <summary>
    /// Validates that a revision exists.
    /// </summary>
    /// <param name="revisionId">The revision ID to validate.</param>
    /// <returns>A NotFound result if not found, null if exists.</returns>
    Task<ActionResult?> ValidateRevisionExistsAsync(Guid revisionId);

    /// <summary>
    /// Validates document for creation with business rules.
    /// </summary>
    /// <param name="matterId">The matter ID.</param>
    /// <param name="document">The document to validate.</param>
    /// <returns>A collection of validation results.</returns>
    Task<IEnumerable<ValidationResult>> ValidateDocumentForCreationAsync(Guid matterId, DocumentForCreationDto document);

    /// <summary>
    /// Validates document for update with business rules.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <returns>A collection of validation results.</returns>
    IEnumerable<ValidationResult> ValidateDocumentForUpdate(DocumentForUpdateDto document);

    /// <summary>
    /// Creates a standardized validation problem response.
    /// </summary>
    /// <param name="validationResults">The validation results.</param>
    /// <param name="title">The problem title.</param>
    /// <returns>A BadRequest result with validation problems.</returns>
    BadRequestObjectResult CreateValidationProblemResponse(IEnumerable<ValidationResult> validationResults, string title);
}

/// <summary>
/// Implementation of centralized validation services.
/// </summary>
public class ValidationService : IValidationService
{
    private readonly IRepository _repository;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly ILogger<ValidationService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationService"/> class.
    /// </summary>
    /// <param name="repository">The repository for entity validation.</param>
    /// <param name="problemDetailsFactory">The problem details factory.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public ValidationService(
        IRepository repository,
        ProblemDetailsFactory problemDetailsFactory,
        ILogger<ValidationService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public ActionResult? ValidateGuid(Guid id, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid GUID parameter: {ParameterName}", parameterName);

            var problemDetails = CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                $"Invalid {parameterName}. GUID cannot be empty.");

            return new BadRequestObjectResult(problemDetails);
        }

        return null;
    }

    /// <inheritdoc />
    public ActionResult? ValidateObject(object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var validationContext = new ValidationContext(obj);
        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(obj, validationContext, validationResults, validateAllProperties: true))
        {
            _logger.LogWarning("Object validation failed for type {ObjectType}. Errors: {@Errors}",
                obj.GetType().Name,
                validationResults.Select(vr => new { vr.ErrorMessage, MemberNames = vr.MemberNames }));

            return CreateValidationProblemResponse(validationResults, "Validation failed");
        }

        return null;
    }

    /// <inheritdoc />
    public ActionResult? ValidateModelState(ModelStateDictionary modelState)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        if (!modelState.IsValid)
        {
            _logger.LogWarning("Model state validation failed. Errors: {@Errors}",
                modelState.Where(ms => ms.Value?.Errors.Count > 0)
                    .ToDictionary(ms => ms.Key, ms => ms.Value!.Errors.Select(e => e.ErrorMessage)));

            var problemDetails = _problemDetailsFactory.CreateValidationProblemDetails(
                _httpContextAccessor.HttpContext!, modelState);

            AddCorrelationId(problemDetails);

            return new BadRequestObjectResult(problemDetails);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<ActionResult?> ValidateMatterExistsAsync(Guid matterId)
    {
        try
        {
            var matter = await _repository.GetMatterAsync(matterId, includeDocuments: false);
            if (matter == null)
            {
                _logger.LogWarning("Matter not found: {MatterId}", matterId);

                var problemDetails = CreateProblemDetails(
                    StatusCodes.Status404NotFound,
                    $"Matter with ID {matterId} does not exist.");

                return new NotFoundObjectResult(problemDetails);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating matter existence: {MatterId}", matterId);

            var problemDetails = CreateProblemDetails(
                StatusCodes.Status500InternalServerError,
                "An error occurred while validating the matter.");

            return new ObjectResult(problemDetails) { StatusCode = 500 };
        }
    }

    /// <inheritdoc />
    public async Task<ActionResult?> ValidateDocumentExistsAsync(Guid documentId)
    {
        try
        {
            var document = await _repository.GetDocumentAsync(documentId, includeRevisions: false, includeHistory: false);
            if (document == null)
            {
                _logger.LogWarning("Document not found: {DocumentId}", documentId);

                var problemDetails = CreateProblemDetails(
                    StatusCodes.Status404NotFound,
                    $"Document with ID {documentId} does not exist.");

                return new NotFoundObjectResult(problemDetails);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating document existence: {DocumentId}", documentId);

            var problemDetails = CreateProblemDetails(
                StatusCodes.Status500InternalServerError,
                "An error occurred while validating the document.");

            return new ObjectResult(problemDetails) { StatusCode = 500 };
        }
    }

    /// <inheritdoc />
    public async Task<ActionResult?> ValidateRevisionExistsAsync(Guid revisionId)
    {
        try
        {
            var revision = await _repository.GetRevisionByIdAsync(revisionId);
            if (revision == null)
            {
                _logger.LogWarning("Revision not found: {RevisionId}", revisionId);

                var problemDetails = CreateProblemDetails(
                    StatusCodes.Status404NotFound,
                    $"Revision with ID {revisionId} does not exist.");

                return new NotFoundObjectResult(problemDetails);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating revision existence: {RevisionId}", revisionId);

            var problemDetails = CreateProblemDetails(
                StatusCodes.Status500InternalServerError,
                "An error occurred while validating the revision.");

            return new ObjectResult(problemDetails) { StatusCode = 500 };
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ValidationResult>> ValidateDocumentForCreationAsync(Guid matterId, DocumentForCreationDto document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var validationResults = new List<ValidationResult>();

        try
        {
            // Basic object validation
            var validationContext = new ValidationContext(document);
            Validator.TryValidateObject(document, validationContext, validationResults, validateAllProperties: true);

            // Business rule validations
            await ValidateDocumentBusinessRulesAsync(matterId, document, validationResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during document creation validation for matter {MatterId}", matterId);
            validationResults.Add(new ValidationResult("An error occurred during validation."));
        }

        return validationResults;
    }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> ValidateDocumentForUpdate(DocumentForUpdateDto document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var validationResults = new List<ValidationResult>();

        try
        {
            // Basic object validation
            var validationContext = new ValidationContext(document);
            Validator.TryValidateObject(document, validationContext, validationResults, validateAllProperties: true);

            // Additional business rule validations for updates
            ValidateDocumentUpdateBusinessRules(document, validationResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during document update validation");
            validationResults.Add(new ValidationResult("An error occurred during validation."));
        }

        return validationResults;
    }

    /// <inheritdoc />
    public BadRequestObjectResult CreateValidationProblemResponse(IEnumerable<ValidationResult> validationResults, string title)
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
            Instance = _httpContextAccessor.HttpContext?.Request.Path
        };

        AddCorrelationId(validationProblemDetails);

        return new BadRequestObjectResult(validationProblemDetails);
    }

    /// <summary>
    /// Validates document business rules for creation.
    /// </summary>
    /// <param name="matterId">The matter ID.</param>
    /// <param name="document">The document to validate.</param>
    /// <param name="validationResults">The validation results collection.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ValidateDocumentBusinessRulesAsync(Guid matterId, DocumentForCreationDto document, List<ValidationResult> validationResults)
    {
        // Check for duplicate filenames
        var existingDocuments = await _repository.GetPaginatedDocumentsAsync(matterId, new DocumentsResourceParameters
        {
            FileName = document.FileName,
            PageSize = 1
        });

        if (existingDocuments.Any())
        {
            validationResults.Add(new ValidationResult(
                "A document with the same filename already exists in this matter.",
                new[] { nameof(document.FileName) }));
        }

        // Validate file extension if provided
        if (!string.IsNullOrWhiteSpace(document.Extension))
        {
            if (!FileValidationHelper.IsExtensionAllowed($".{document.Extension}"))
            {
                validationResults.Add(new ValidationResult(
                    $"File extension '.{document.Extension}' is not allowed.",
                    new[] { nameof(document.Extension) }));
            }
        }

        // Validate file size
        if (document.FileSize > 50 * 1024 * 1024) // 50 MB
        {
            validationResults.Add(new ValidationResult(
                "File size cannot exceed 50 MB.",
                new[] { nameof(document.FileSize) }));
        }

        // Validate MIME type
        if (!string.IsNullOrWhiteSpace(document.MimeType))
        {
            if (!FileValidationHelper.IsValidMimeType(document.MimeType))
            {
                validationResults.Add(new ValidationResult(
                    $"MIME type '{document.MimeType}' is not allowed.",
                    new[] { nameof(document.MimeType) }));
            }
        }
    }

    /// <summary>
    /// Validates document business rules for updates.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <param name="validationResults">The validation results collection.</param>
    private static void ValidateDocumentUpdateBusinessRules(DocumentForUpdateDto document, List<ValidationResult> validationResults)
    {
        // Validate file extension if provided
        if (!string.IsNullOrWhiteSpace(document.Extension))
        {
            if (!FileValidationHelper.IsExtensionAllowed($".{document.Extension}"))
            {
                validationResults.Add(new ValidationResult(
                    $"File extension '.{document.Extension}' is not allowed.",
                    new[] { nameof(document.Extension) }));
            }
        }

        // Validate file size
        if (document.FileSize > 50 * 1024 * 1024) // 50 MB
        {
            validationResults.Add(new ValidationResult(
                "File size cannot exceed 50 MB.",
                new[] { nameof(document.FileSize) }));
        }

        // Additional update-specific validations can go here
    }

    /// <summary>
    /// Creates a standardized problem details object.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="detail">The problem detail message.</param>
    /// <returns>A problem details object.</returns>
    private ProblemDetails CreateProblemDetails(int statusCode, string detail)
    {
        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            _httpContextAccessor.HttpContext!,
            statusCode,
            detail: detail);

        AddCorrelationId(problemDetails);

        return problemDetails;
    }

    /// <summary>
    /// Adds correlation ID to problem details if available.
    /// </summary>
    /// <param name="problemDetails">The problem details object.</param>
    private void AddCorrelationId(ProblemDetails problemDetails)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
            {
                problemDetails.Extensions["correlationId"] = correlationId;
            }
        }
    }
}

/// <summary>
/// Extension methods for common validation scenarios.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates that a collection is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="propertyName">The property name for error reporting.</param>
    /// <returns>A validation result if invalid, null if valid.</returns>
    public static ValidationResult? ValidateNotNullOrEmpty<T>(this IEnumerable<T>? collection, string propertyName)
    {
        if (collection == null || !collection.Any())
        {
            return new ValidationResult($"{propertyName} cannot be null or empty.", new[] { propertyName });
        }

        return null;
    }

    /// <summary>
    /// Validates that a string is not null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="propertyName">The property name for error reporting.</param>
    /// <returns>A validation result if invalid, null if valid.</returns>
    public static ValidationResult? ValidateNotNullOrWhiteSpace(this string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new ValidationResult($"{propertyName} cannot be null, empty, or whitespace.", new[] { propertyName });
        }

        return null;
    }

    /// <summary>
    /// Validates that a value is within a specified range.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="minimum">The minimum allowed value.</param>
    /// <param name="maximum">The maximum allowed value.</param>
    /// <param name="propertyName">The property name for error reporting.</param>
    /// <returns>A validation result if invalid, null if valid.</returns>
    public static ValidationResult? ValidateRange<T>(this T value, T minimum, T maximum, string propertyName)
        where T : IComparable<T>
    {
        if (value.CompareTo(minimum) < 0 || value.CompareTo(maximum) > 0)
        {
            return new ValidationResult(
                $"{propertyName} must be between {minimum} and {maximum}.",
                new[] { propertyName });
        }

        return null;
    }
}