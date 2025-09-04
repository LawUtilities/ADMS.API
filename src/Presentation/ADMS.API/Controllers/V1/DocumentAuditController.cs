using ADMS.API.Extensions;
using ADMS.API.Helpers;
using ADMS.API.Models;
using ADMS.API.ResourceParameters;
using ADMS.API.Services;

using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Controllers;

/// <summary>
/// Controller for managing document audit records.
/// Provides endpoints for retrieving paginated audit records for document activities,
/// move/copy operations, and related audit trails.
/// </summary>
/// <remarks>
/// This controller handles three types of audit records:
/// - Document activities (create, update, delete, restore, check in/out)
/// - Document move/copy FROM operations (where the document was moved/copied from)
/// - Document move/copy TO operations (where the document was moved/copied to)
/// 
/// All endpoints support pagination, sorting, and filtering through resource parameters.
/// The controller implements consistent error handling, validation, and logging patterns.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/matters/{matterId}/documents/{documentId}/audits")]
public class DocumentAuditController(
    ILogger<DocumentAuditController> logger,
    IRepository admsRepository,
    IPropertyMappingService propertyMappingService,
    ProblemDetailsFactory problemDetailsFactory,
    IValidationService validationService) : ControllerBase
{
    private readonly IRepository _admsRepository = admsRepository ?? throw new ArgumentNullException(nameof(admsRepository));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
    private readonly IValidationService _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    private readonly IPropertyMappingService _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));

    #region Actions

    /// <summary>
    /// Retrieves paginated document activity audit records for a specific document.
    /// </summary>
    /// <remarks>
    /// Returns audit records for document activities such as:
    /// - Document creation, updates, deletion, and restoration
    /// - Check-in and check-out operations
    /// - File uploads and modifications
    /// 
    /// The endpoint supports pagination, sorting, and field validation.
    /// All returned DTOs are validated to ensure data integrity.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document for which to retrieve audit records.</param>
    /// <param name="resourceParameters">Pagination and sorting parameters including page size, page number, and order by fields.</param>
    /// <returns>
    /// <para>200 OK - A paged list of document activity audit records with pagination metadata in headers.</para>
    /// <para>400 BadRequest - Invalid input parameters or validation errors.</para>
    /// <para>404 NotFound - The specified matter or document does not exist.</para>
    /// <para>500 InternalServerError - An unexpected server error occurred.</para>
    /// </returns>
    [HttpGet("activities", Name = "GetDocumentActivityAudits")]
    [ProducesResponseType(typeof(PagedList<DocumentActivityUserMinimalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedList<DocumentActivityUserMinimalDto>>> GetDocumentActivityAuditsAsync(
        Guid matterId,
        Guid documentId,
        [FromQuery] DocumentAuditsResourceParameters resourceParameters)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateRequestParametersAsync(matterId, documentId, resourceParameters);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Validate property mapping for sorting
            var propertyValidationResult = ValidatePropertyMapping<DocumentActivityUserMinimalDto>(resourceParameters.OrderBy);
            if (propertyValidationResult != null)
            {
                return propertyValidationResult;
            }

            // Retrieve audit records from repository
            var pagedAudits = await _admsRepository.GetDocumentActivityAuditsAsync(documentId, resourceParameters);

            Response.AddPaginationMetadata<DocumentActivityUserMinimalDto>(pagedAudits.Value);
            
            _logger.LogInformation(
                "Successfully retrieved {Count} document activity audits for document {DocumentId} in matter {MatterId}. Page {CurrentPage} of {TotalPages}.",
                pagedAudits.Value.Count, documentId, matterId, pagedAudits.Value.CurrentPage, pagedAudits.Value.TotalPages);

            return Ok(pagedAudits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while retrieving document activity audits for document {DocumentId} in matter {MatterId}.",
                documentId, matterId);

            return CreateInternalServerErrorResponse("retrieving document activity audits");
        }
    }

    /// <summary>
    /// Retrieves paginated document move/copy FROM audit records for a specific document.
    /// </summary>
    /// <remarks>
    /// Returns audit records showing where the document was moved or copied FROM.
    /// This includes the source matter information and the user who performed the operation.
    /// 
    /// The endpoint supports pagination, sorting, and comprehensive error handling.
    /// Validation ensures data integrity and proper error responses.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document for which to retrieve move/copy FROM audit records.</param>
    /// <param name="resourceParameters">Pagination and sorting parameters including page size, page number, and order by fields.</param>
    /// <returns>
    /// <para>200 OK - A paged list of move/copy FROM audit records with pagination metadata in headers.</para>
    /// <para>400 BadRequest - Invalid input parameters or validation errors.</para>
    /// <para>404 NotFound - The specified matter or document does not exist.</para>
    /// <para>500 InternalServerError - An unexpected server error occurred.</para>
    /// </returns>
    [HttpGet("movefrom", Name = "GetDocumentMoveFromAudits")]
    [ProducesResponseType(typeof(PagedList<MatterDocumentActivityUserMinimalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedList<MatterDocumentActivityUserMinimalDto>>> GetDocumentMoveFromAuditsAsync(
        Guid matterId,
        Guid documentId,
        [FromQuery] DocumentAuditsResourceParameters resourceParameters)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateRequestParametersAsync(matterId, documentId, resourceParameters);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Validate property mapping for sorting
            var propertyValidationResult = ValidatePropertyMapping<MatterDocumentActivityUserMinimalDto>(resourceParameters.OrderBy);
            if (propertyValidationResult != null)
            {
                return propertyValidationResult;
            }

            // Retrieve audit records from repository
            var pagedAudits = await _admsRepository.GetPaginatedDocumentMoveFromAuditsAsync(documentId, resourceParameters);

            // Add pagination metadata to response headers
            Response.AddPaginationMetadata(pagedAudits);

            _logger.LogInformation(
                "Successfully retrieved {Count} move/copy FROM audits for document {DocumentId} in matter {MatterId}. Page {CurrentPage} of {TotalPages}.",
                pagedAudits.Count, documentId, matterId, pagedAudits.CurrentPage, pagedAudits.TotalPages);

            return Ok(pagedAudits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while retrieving move/copy FROM audits for document {DocumentId} in matter {MatterId}.",
                documentId, matterId);

            return CreateInternalServerErrorResponse("retrieving move/copy FROM audits");
        }
    }

    /// <summary>
    /// Retrieves paginated document move/copy TO audit records for a specific document.
    /// </summary>
    /// <remarks>
    /// Returns audit records showing where the document was moved or copied TO.
    /// This includes the destination matter information and the user who performed the operation.
    /// 
    /// The endpoint supports pagination, sorting, and comprehensive error handling.
    /// All responses include consistent pagination metadata and proper HTTP status codes.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document for which to retrieve move/copy TO audit records.</param>
    /// <param name="resourceParameters">Pagination and sorting parameters including page size, page number, and order by fields.</param>
    /// <returns>
    /// <para>200 OK - A paged list of move/copy TO audit records with pagination metadata in headers.</para>
    /// <para>400 BadRequest - Invalid input parameters or validation errors.</para>
    /// <para>404 NotFound - The specified matter or document does not exist.</para>
    /// <para>500 InternalServerError - An unexpected server error occurred.</para>
    /// </returns>
    [HttpGet("moveto", Name = "GetDocumentMoveToAudits")]
    [ProducesResponseType(typeof(PagedList<MatterDocumentActivityUserMinimalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedList<MatterDocumentActivityUserMinimalDto>>> GetDocumentMoveToAuditsAsync(
        Guid matterId,
        Guid documentId,
        [FromQuery] DocumentAuditsResourceParameters resourceParameters)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateRequestParametersAsync(matterId, documentId, resourceParameters);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Validate property mapping for sorting
            var propertyValidationResult = ValidatePropertyMapping<MatterDocumentActivityUserMinimalDto>(resourceParameters.OrderBy);
            if (propertyValidationResult != null)
            {
                return propertyValidationResult;
            }

            // Retrieve audit records from repository
            var pagedAudits = await _admsRepository.GetPaginatedDocumentMoveToAuditsAsync(documentId, resourceParameters);

            // Add pagination metadata to response headers
            Response.AddPaginationMetadata(pagedAudits);

            _logger.LogInformation(
                "Successfully retrieved {Count} move/copy TO audits for document {DocumentId} in matter {MatterId}. Page {CurrentPage} of {TotalPages}.",
                pagedAudits.Count, documentId, matterId, pagedAudits.CurrentPage, pagedAudits.TotalPages);

            return Ok(pagedAudits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while retrieving move/copy TO audits for document {DocumentId} in matter {MatterId}.",
                documentId, matterId);

            return CreateInternalServerErrorResponse("retrieving move/copy TO audits");
        }
    }

    #endregion Actions

    #region Private Helper Methods

    /// <summary>
    /// Validates the common request parameters for all audit endpoints.
    /// </summary>
    /// <remarks>
    /// Performs validation on:
    /// - ModelState validation
    /// - Matter existence
    /// - Document existence
    /// 
    /// This method centralizes common validation logic to reduce code duplication
    /// and ensure consistent error handling across all endpoints.
    /// </remarks>
    /// <param name="matterId">The matter ID to validate.</param>
    /// <param name="documentId">The document ID to validate.</param>
    /// <param name="resourceParameters">The resource parameters to validate.</param>
    /// <returns>An ActionResult if validation fails, null if validation passes.</returns>
    private async Task<ActionResult?> ValidateRequestParametersAsync(
        Guid matterId,
        Guid documentId,
        DocumentAuditsResourceParameters resourceParameters)
    {
        // Validate ModelState
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            _logger.LogWarning(
                "ModelState validation failed for audit request on document {DocumentId} in matter {MatterId}. Errors: {@Errors}",
                documentId, matterId, errors);

            return BadRequest(new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Detail = "Please check the request parameters and try again."
            });
        }

        // Validate matter existence
        var matterValidationResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (matterValidationResult != null)
        {
            _logger.LogWarning("Matter with ID {MatterId} does not exist.", matterId);
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Matter not found.",
                Detail = $"Matter with ID {matterId} does not exist."
            });
        }

        // Validate document existence
        var documentValidationResult = await _validationService.ValidateDocumentExistsAsync(documentId);
        if (documentValidationResult != null)
        {
            _logger.LogWarning("Document with ID {DocumentId} does not exist.", documentId);
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Document not found.",
                Detail = $"Document with ID {documentId} does not exist."
            });
        }

        return null;
    }

    /// <summary>
    /// Validates property mapping for sorting parameters.
    /// </summary>
    /// <remarks>
    /// Ensures that the requested order by fields are valid for the specified DTO type.
    /// This prevents SQL injection and ensures proper sorting functionality.
    /// </remarks>
    /// <typeparam name="TDto">The DTO type to validate property mapping for.</typeparam>
    /// <param name="orderBy">The order by string to validate.</param>
    /// <returns>An ActionResult if validation fails, null if validation passes.</returns>
    private ActionResult? ValidatePropertyMapping<TDto>(string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return null;
        }

        if (!_propertyMappingService.ValidMappingExistsFor<TDto, TDto>(orderBy))
        {
            var errors = new Dictionary<string, string[]>
            {
                { nameof(orderBy), [$"Requested order by fields '{orderBy}' are invalid for {typeof(TDto).Name}."] }
            };

            _logger.LogWarning(
                "Invalid order by fields '{OrderBy}' for {DtoType}.",
                orderBy, typeof(TDto).Name);

            return BadRequest(new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid order by fields.",
                Detail = "The specified order by fields are not valid for this resource type."
            });
        }

        return null;
    }

    /// <summary>
    /// Creates a standardized internal server error response.
    /// </summary>
    /// <remarks>
    /// Provides consistent error responses for unexpected server errors.
    /// The operation context helps identify which operation failed for better debugging.
    /// </remarks>
    /// <param name="operationContext">A description of the operation that failed.</param>
    /// <returns>A standardized internal server error response.</returns>
    private ObjectResult CreateInternalServerErrorResponse(string operationContext)
    {
        var problem = _problemDetailsFactory.CreateProblemDetails(
            HttpContext,
            StatusCodes.Status500InternalServerError,
            $"An unexpected error occurred while {operationContext}. Please try again later."
        );

        return StatusCode(StatusCodes.Status500InternalServerError, problem);
    }

    #endregion Private Helper Methods
}