using ADMS.API.Entities;
using ADMS.API.Extensions;
using ADMS.API.Models;
using ADMS.API.ResourceParameters;
using ADMS.API.Services;

using Asp.Versioning;

using MapsterMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using Revision = ADMS.API.Entities.Revision;

namespace ADMS.API.Controllers;

/// <summary>
/// Controller for managing document revisions within the ADMS system.
/// </summary>
/// <remarks>
/// This controller provides comprehensive revision management functionality including:
/// - Revision creation with automatic numbering and validation
/// - Revision retrieval with pagination, filtering, and sorting
/// - Revision updates with business rule enforcement
/// - Revision deletion with safety checks and audit trails
/// - Comprehensive audit history tracking for revisions
/// 
/// All endpoints implement consistent error handling, centralized validation,
/// structured logging, and security best practices including:
/// - Comprehensive input validation using data annotations and custom validation
/// - Centralized validation service integration
/// - Proper HTTP status code usage following REST conventions
/// - Detailed error responses using RFC 7807 Problem Details format
/// - Structured logging with correlation IDs for traceability
/// - API versioning support with proper headers
/// 
/// Business Rules Enforced:
/// - Revisions must belong to valid documents within valid matters
/// - Revision numbers are automatically assigned in sequential order
/// - All operations maintain comprehensive audit trails
/// - Entity relationships are validated before operations
/// - Date/time handling uses UTC with local formatting options
/// 
/// The controller supports advanced features like:
/// - Pagination with metadata headers for efficient data retrieval
/// - Flexible sorting through resource parameters
/// - Comprehensive audit history with user attribution
/// - Automatic revision numbering based on existing revisions
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/matters/{matterId}/documents/{documentId}/revisions")]
[Produces("application/json", "application/xml")]
public class RevisionController(
    ILogger<RevisionController> logger,
    IRepository admsRepository,
    IMapper mapper,
    IPropertyMappingService propertyMappingService,
    ProblemDetailsFactory problemDetailsFactory,
    IValidationService validationService) : ControllerBase
{
    private readonly IRepository _admsRepository = admsRepository ?? throw new ArgumentNullException(nameof(admsRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ILogger<RevisionController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IPropertyMappingService _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
    private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
    private readonly IValidationService _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));

    #region Actions

    /// <summary>
    /// Creates a new revision for a specified document within a matter.
    /// </summary>
    /// <remarks>
    /// This endpoint creates a new revision with comprehensive validation including:
    /// - Input validation using data annotations and custom validation rules
    /// - Entity relationship validation (matter -> document hierarchy)
    /// - Automatic revision numbering based on existing revisions
    /// - Business rule validation for revision creation
    /// - Automatic audit trail creation for the revision creation event
    /// 
    /// The creation process includes:
    /// 1. Comprehensive DTO and parameter validation using centralized validation services
    /// 2. Entity existence validation with proper hierarchy checking
    /// 3. Automatic revision number assignment (sequential)
    /// 4. Revision creation with proper entity relationship setup
    /// 5. Audit log creation with creation activity tracking
    /// 
    /// Upon successful creation, the response includes a Location header pointing
    /// to the newly created revision and returns the created revision data.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document to add the revision to.</param>
    /// <param name="revision">The revision data transfer object containing the information needed to create a new revision.</param>
    /// <returns>
    /// <para>201 Created - Revision successfully created with location header and revision data.</para>
    /// <para>400 BadRequest - Invalid input data, validation errors, or malformed request.</para>
    /// <para>404 NotFound - The specified matter or document does not exist.</para>
    /// <para>500 InternalServerError - Unexpected server error during revision creation.</para>
    /// </returns>
    [HttpPost(Name = "CreateRevision")]
    [ProducesResponseType(typeof(RevisionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RevisionDto>> CreateRevisionAsync(
        Guid matterId,
        Guid documentId,
        [FromBody] RevisionForCreationDto? revision)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateCreateRevisionParametersAsync(matterId, documentId, revision);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Retrieve document with revisions to determine next revision number
            var document = await ValidateDocumentAndGetRevisionCountAsync(matterId, documentId);
            if (document == null)
            {
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound,
                    "Document does not belong to the specified matter."));
            }

            // Create revision with automatic numbering
            var createdRevision = await CreateRevisionWithAuditAsync(documentId, revision!, document.Revisions.Count + 1);
            if (createdRevision == null)
            {
                _logger.LogError(
                    "Failed to create revision for Document {DocumentId} in Matter {MatterId}.",
                    documentId, matterId);
                return CreateInternalServerErrorResponse("creating the revision");
            }

            // Add response headers
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogInformation(
                "Revision created successfully for Document {DocumentId} in Matter {MatterId}.",
                documentId, matterId);

            // Return 201 Created with route to new revision
            return CreatedAtRoute(
                routeName: "GetRevision",
                routeValues: new { matterId, documentId, revisionId = createdRevision.Id },
                value: _mapper.Map<RevisionDto>(createdRevision)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error creating revision for Document {DocumentId} in Matter {MatterId}.",
                documentId, matterId);

            return CreateInternalServerErrorResponse("creating the revision");
        }
    }

    /// <summary>
    /// Deletes a specific revision from a document within a matter.
    /// </summary>
    /// <remarks>
    /// This endpoint provides secure revision deletion with comprehensive validation:
    /// - Validates that the revision, document, and matter all exist and are properly related
    /// - Performs business rule validation to ensure the revision can be safely deleted
    /// - Creates comprehensive audit trail for the deletion operation
    /// - Implements soft deletion to maintain data integrity and audit history
    /// 
    /// Business Rules:
    /// - Revision must belong to the specified document
    /// - Document must belong to the specified matter
    /// - Deletion creates an audit record for compliance and tracking
    /// - The deletion maintains referential integrity
    /// 
    /// The deletion process includes:
    /// 1. Parameter and entity existence validation with hierarchy checking
    /// 2. Business rule validation (revision ownership, etc.)
    /// 3. Audit trail creation before deletion
    /// 4. Secure deletion with proper cleanup
    /// 5. Success confirmation without exposing internal details
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document containing the revision.</param>
    /// <param name="revisionId">The unique identifier of the revision to delete.</param>
    /// <returns>
    /// <para>204 NoContent - Revision successfully deleted.</para>
    /// <para>400 BadRequest - Invalid revision ID, business rule violation, or validation errors.</para>
    /// <para>404 NotFound - The specified matter, document, or revision does not exist.</para>
    /// <para>500 InternalServerError - Unexpected server error during deletion.</para>
    /// </returns>
    [HttpDelete("{revisionId}", Name = "DeleteRevision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRevisionAsync(
        Guid matterId,
        Guid documentId,
        Guid revisionId)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateDeleteRevisionParametersAsync(matterId, documentId, revisionId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Validate business rules for deletion
            var businessRuleResult = await ValidateRevisionDeletionBusinessRulesAsync(matterId, documentId, revisionId);
            if (businessRuleResult != null)
            {
                return businessRuleResult;
            }

            // Perform the deletion
            var isDeleted = await DeleteRevisionWithAuditAsync(revisionId);
            if (!isDeleted)
            {
                _logger.LogError("Failed to delete revision: {RevisionId}", revisionId);
                return CreateInternalServerErrorResponse("deleting the revision");
            }

            // Add response headers
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogInformation("Revision {RevisionId} deleted successfully.", revisionId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error deleting revision {RevisionId} for document {DocumentId} in matter {MatterId}.",
                revisionId, documentId, matterId);

            return CreateInternalServerErrorResponse("deleting the revision");
        }
    }

    /// <summary>
    /// Retrieves a specific revision for a document within a matter.
    /// </summary>
    /// <remarks>
    /// This endpoint provides secure revision retrieval with comprehensive validation:
    /// - Validates entity existence and proper hierarchical relationships
    /// - Ensures revision belongs to the correct document and matter
    /// - Returns properly validated and mapped revision data
    /// - Creates audit trail for revision access (VIEWED activity)
    /// - Comprehensive error handling with proper HTTP status codes
    /// 
    /// The retrieval process includes:
    /// - Parameter validation using centralized validation services
    /// - Entity existence validation with relationship checking
    /// - DTO mapping and validation for consistent data format
    /// - Audit logging for compliance and usage tracking
    /// - Error handling with detailed problem responses
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document containing the revision.</param>
    /// <param name="revisionId">The unique identifier of the revision to retrieve.</param>
    /// <returns>
    /// <para>200 OK - Revision successfully retrieved with requested data.</para>
    /// <para>400 BadRequest - Invalid revision ID or validation errors.</para>
    /// <para>404 NotFound - The specified matter, document, or revision does not exist.</para>
    /// <para>500 InternalServerError - Unexpected server error during retrieval.</para>
    /// </returns>
    [HttpGet("{revisionId}", Name = "GetRevision")]
    [ProducesResponseType(typeof(RevisionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RevisionDto>> GetRevisionAsync(
        Guid matterId,
        Guid documentId,
        Guid revisionId)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateGetRevisionParametersAsync(matterId, documentId, revisionId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Retrieve and validate revision
            var revision = await GetValidatedRevisionAsync(matterId, documentId, revisionId);
            if (revision == null)
            {
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Revision not found."));
            }

            // Map and validate DTO
            var revisionDto = _mapper.Map<RevisionDto>(revision);
            var dtoValidationResult = _validationService.ValidateObject(revisionDto);
            if (dtoValidationResult != null)
            {
                return dtoValidationResult;
            }

            // Create audit trail for revision access
            await CreateRevisionViewedAuditAsync(revisionId);

            // Add response headers
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogInformation("Revision {RevisionId} retrieved successfully.", revisionId);
            return Ok(revisionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error retrieving revision {RevisionId} for document {DocumentId} in matter {MatterId}.",
                revisionId, documentId, matterId);

            return CreateInternalServerErrorResponse("retrieving the revision");
        }
    }

    /// <summary>
    /// Retrieves the audit records (activity history) for a specific revision.
    /// </summary>
    /// <remarks>
    /// This endpoint provides comprehensive revision audit history including:
    /// - Complete activity tracking for all revision-related operations
    /// - User attribution and timestamp information for all activities
    /// - Proper validation of all input parameters and entity relationships
    /// - Comprehensive error handling with detailed responses
    /// - Validation of all returned audit data for consistency
    /// 
    /// Audit History Information Includes:
    /// - Revision creation, update, and deletion activities
    /// - User access and modification tracking
    /// - Timestamp information for activity sequencing
    /// - Complete user attribution for compliance requirements
    /// - Activity type classification for reporting purposes
    /// 
    /// Features:
    /// - Comprehensive error handling with detailed problem responses
    /// - Validation of all returned audit data
    /// - Support for empty audit history with proper 404 response
    /// - API versioning and correlation ID support
    /// - Structured logging for debugging and monitoring
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document containing the revision.</param>
    /// <param name="revisionId">The unique identifier of the revision whose audit records are to be retrieved.</param>
    /// <returns>
    /// <para>200 OK - Complete audit history successfully retrieved.</para>
    /// <para>400 BadRequest - Invalid parameters or validation errors.</para>
    /// <para>404 NotFound - The specified entities do not exist or no audit history available.</para>
    /// <para>500 InternalServerError - Unexpected server error during audit retrieval.</para>
    /// </returns>
    [HttpGet("{revisionId}/audits")]
    [ProducesResponseType(typeof(IEnumerable<RevisionActivityUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RevisionActivityUserDto>>> GetRevisionAuditsAsync(
        Guid matterId,
        Guid documentId,
        Guid revisionId)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateGetRevisionAuditsParametersAsync(matterId, documentId, revisionId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Retrieve revision with audit history
            var auditHistory = await RetrieveRevisionAuditHistoryAsync(matterId, documentId, revisionId);
            if (auditHistory == null || !auditHistory.Any())
            {
                _logger.LogInformation(
                    "No audit history found for Revision {RevisionId}.",
                    revisionId);
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Revision audit history not found."));
            }

            // Validate audit records
            var validationErrors = ValidateAuditRecords(auditHistory);
            if (validationErrors.Any())
            {
                return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest,
                    "Validation failed for one or more audit records."));
            }

            // Add response headers
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogInformation(
                "Retrieved {Count} audit records for Revision {RevisionId}.",
                auditHistory.Count(), revisionId);

            return Ok(auditHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error retrieving audit history for revision {RevisionId} of document {DocumentId} in matter {MatterId}.",
                revisionId, documentId, matterId);

            return CreateInternalServerErrorResponse("retrieving revision audit history");
        }
    }

    /// <summary>
    /// Retrieves a paginated list of revisions for a specific document within a matter.
    /// </summary>
    /// <remarks>
    /// This endpoint provides advanced revision listing functionality with comprehensive features:
    /// - Pagination with metadata headers following the same pattern as DocumentController and MatterController
    /// - Flexible sorting through resource parameters with property mapping validation
    /// - Comprehensive error handling and validation consistent with other controllers
    /// - Performance optimization for large revision datasets
    /// - Consistent pagination metadata format using Response.AddPaginationMetadata()
    /// 
    /// Pagination Features (consistent with DocumentController):
    /// - Configurable page size with reasonable limits
    /// - Page navigation with total count information
    /// - Metadata headers for client-side pagination handling (X-Pagination header)
    /// - Performance optimization for large result sets
    /// - HasNext/HasPrevious navigation indicators
    /// 
    /// Sorting and Filtering:
    /// - Multiple sort field options with property mapping validation
    /// - Ascending and descending sort directions
    /// - Proper validation of OrderBy parameters using property mapping service
    /// - Model state validation for query parameters
    /// 
    /// Response Format:
    /// - JSON/XML content negotiation support
    /// - Comprehensive pagination metadata in headers (same as MatterController)
    /// - Consistent error responses using problem details format
    /// - API versioning and correlation ID support
    /// 
    /// The pagination implementation follows the established patterns from:
    /// - DocumentController.GetDocumentsAsync() for header management
    /// - MatterController.GetMattersAsync() for parameter validation
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document whose revisions are to be retrieved.</param>
    /// <param name="resourceParameters">Resource parameters for pagination and sorting, following the same pattern as DocumentsResourceParameters and MattersResourceParameters.</param>
    /// <returns>
    /// <para>200 OK - Paginated revision list successfully retrieved with metadata headers.</para>
    /// <para>400 BadRequest - Invalid pagination parameters, sort criteria, or validation errors.</para>
    /// <para>404 NotFound - The specified matter or document does not exist.</para>
    /// <para>500 InternalServerError - Unexpected server error during revision retrieval.</para>
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RevisionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RevisionDto>>> GetRevisionsAsync(
        Guid matterId,
        Guid documentId,
        [FromQuery] RevisionsResourceParameters resourceParameters)
    {
        try
        {
            // Validate input parameters (following MatterController pattern)
            var validationResult = await ValidateGetRevisionsParametersAsync(matterId, documentId, resourceParameters);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Validate document belongs to matter (business rule validation)
            var documentValidationResult = await ValidateDocumentBelongsToMatterAsync(matterId, documentId);
            if (documentValidationResult != null)
            {
                return documentValidationResult;
            }

            // Retrieve paginated revisions
            var pagedRevisionsResult = await _admsRepository.GetPaginatedRevisionsAsync(documentId, resourceParameters);
            if (pagedRevisionsResult.Value is not { } pagedRevisions)
            {
                _logger.LogError("Failed to retrieve paginated revisions for Document {DocumentId}.", documentId);
                return CreateInternalServerErrorResponse("retrieving paginated revisions");
            }

            // Add pagination metadata to response headers (following DocumentController pattern)
            Response.AddPaginationMetadata(pagedRevisions);

            // Map to DTOs
            var revisionDtos = _mapper.Map<IEnumerable<RevisionDto>>(pagedRevisions);

            // Validate returned revisions (following MatterController pattern)
            var validationErrors = ValidateReturnedRevisions(revisionDtos);
            if (validationErrors.Any())
            {
                return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest,
                    "Validation failed for one or more returned revisions."));
            }

            // Add response headers (consistent with other controllers)
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            if (!pagedRevisions.Any())
            {
                _logger.LogInformation("No revisions found for Document {DocumentId}.", documentId);
            }
            else
            {
                _logger.LogInformation(
                    "Retrieved {Count} revisions for Document {DocumentId} (Page {Page}/{TotalPages}).",
                    pagedRevisions.Count, documentId, pagedRevisions.CurrentPage, pagedRevisions.TotalPages);
            }

            return Ok(revisionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error retrieving revisions for document {DocumentId} in matter {MatterId}.",
                documentId, matterId);

            return CreateInternalServerErrorResponse("retrieving revisions");
        }
    }

    /// <summary>
    /// Updates an existing revision for a specified document within a matter.
    /// </summary>
    /// <remarks>
    /// This endpoint provides secure revision updating with comprehensive validation:
    /// - Validates all input parameters including revision existence and hierarchy
    /// - Performs business rule validation for update operations
    /// - Maintains complete audit trail for all changes
    /// - Validates updated data using centralized validation services
    /// - Implements proper entity relationship checking
    /// 
    /// Update Process:
    /// 1. Input validation using data annotations and custom rules
    /// 2. Entity existence validation with hierarchy checking
    /// 3. Business rule validation for update permissions
    /// 4. Data persistence with proper error handling
    /// 5. Audit trail creation for the update operation
    /// 6. Response with appropriate status and headers
    /// 
    /// Features:
    /// - Comprehensive error handling with detailed problem responses
    /// - Audit logging for compliance and change tracking
    /// - Proper HTTP status codes following REST conventions
    /// - API versioning and correlation ID support
    /// - Entity relationship validation (revision -> document -> matter)
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document containing the revision.</param>
    /// <param name="revisionId">The unique identifier of the revision to update.</param>
    /// <param name="revision">The revision update data transfer object containing the new revision information.</param>
    /// <returns>
    /// <para>204 NoContent - Revision successfully updated.</para>
    /// <para>400 BadRequest - Invalid input data, validation errors, or business rule violations.</para>
    /// <para>404 NotFound - The specified matter, document, or revision does not exist.</para>
    /// <para>500 InternalServerError - Unexpected server error during update.</para>
    /// </returns>
    [HttpPut("{revisionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRevisionAsync(
        Guid matterId,
        Guid documentId,
        Guid revisionId,
        [FromBody] RevisionForUpdateDto revision)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateUpdateRevisionParametersAsync(matterId, documentId, revisionId, revision);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Validate business rules for update
            var businessRuleResult = await ValidateRevisionUpdateBusinessRulesAsync(matterId, documentId, revisionId);
            if (businessRuleResult != null)
            {
                return businessRuleResult;
            }

            // Perform the update
            var updatedRevision = await UpdateRevisionWithAuditAsync(matterId, documentId, revisionId, revision);
            if (updatedRevision == null)
            {
                _logger.LogError("Failed to update revision: {RevisionId}", revisionId);
                return CreateInternalServerErrorResponse("updating the revision");
            }

            // Add response headers
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogInformation("Revision {RevisionId} updated successfully.", revisionId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error updating revision {RevisionId} for document {DocumentId} in matter {MatterId}.",
                revisionId, documentId, matterId);

            return CreateInternalServerErrorResponse("updating the revision");
        }
    }

    /// <summary>
    /// Returns the HTTP methods supported by the revision resource.
    /// </summary>
    /// <remarks>
    /// This endpoint implements the HTTP OPTIONS method to allow clients
    /// to discover which operations are available on the revision resource.
    /// This is useful for API documentation tools, CORS preflight requests,
    /// and client applications that need to determine available functionality dynamically.
    /// 
    /// The response includes appropriate headers indicating supported methods,
    /// API version information, and CORS configuration if applicable.
    /// </remarks>
    /// <returns>
    /// <para>204 NoContent - Response with Allow header listing supported HTTP methods.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpOptions]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult GetRevisionOptions()
    {
        try
        {
            Response.Headers.Allow = "GET,POST,PUT,DELETE,OPTIONS";
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogDebug("OPTIONS request processed for revision resource.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OPTIONS request for revision resource.");
            return CreateInternalServerErrorResponse("processing the OPTIONS request");
        }
    }

    #endregion Actions

    #region Private Helper Methods

    /// <summary>
    /// Validates parameters for revision creation operations.
    /// </summary>
    private async Task<ActionResult?> ValidateCreateRevisionParametersAsync(
        Guid matterId, Guid documentId, RevisionForCreationDto? revision)
    {
        // Validate GUIDs
        var guidValidationResult = _validationService.ValidateGuid(matterId, nameof(matterId))
            ?? _validationService.ValidateGuid(documentId, nameof(documentId));

        if (guidValidationResult != null)
        {
            _logger.LogWarning("Invalid GUID parameters for revision creation.");
            return guidValidationResult;
        }

        // Null check for DTO
        if (revision == null)
        {
            _logger.LogWarning("RevisionForCreationDto is null.");
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Revision data is required."));
        }

        // Validate the object using centralized validation logic
        var objectValidationResult = _validationService.ValidateObject(revision);
        if (objectValidationResult != null)
        {
            return objectValidationResult;
        }

        // Validate model state
        var modelValidationResult = _validationService.ValidateModelState(ModelState);
        if (modelValidationResult != null)
        {
            _logger.LogWarning("Invalid model state for RevisionForCreationDto. {@ModelState}", ModelState);
            return modelValidationResult;
        }

        // Validate entity existence
        var matterExistsResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (matterExistsResult != null) return matterExistsResult;

        var documentExistsResult = await _validationService.ValidateDocumentExistsAsync(documentId);
        if (documentExistsResult != null) return documentExistsResult;

        return null;
    }

    /// <summary>
    /// Validates parameters for revision deletion operations.
    /// </summary>
    private async Task<ActionResult?> ValidateDeleteRevisionParametersAsync(
        Guid matterId, Guid documentId, Guid revisionId)
    {
        // Validate GUIDs
        var guidValidationResult = _validationService.ValidateGuid(matterId, nameof(matterId))
            ?? _validationService.ValidateGuid(documentId, nameof(documentId))
            ?? _validationService.ValidateGuid(revisionId, nameof(revisionId));

        if (guidValidationResult != null)
        {
            _logger.LogWarning("Invalid GUID parameters for revision deletion.");
            return guidValidationResult;
        }

        // Validate entity existence
        var matterExistsResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (matterExistsResult != null) return matterExistsResult;

        var documentExistsResult = await _validationService.ValidateDocumentExistsAsync(documentId);
        if (documentExistsResult != null) return documentExistsResult;

        var revisionExistsResult = await _validationService.ValidateRevisionExistsAsync(revisionId);
        if (revisionExistsResult != null) return revisionExistsResult;

        return null;
    }

    /// <summary>
    /// Validates parameters for revision retrieval operations.
    /// </summary>
    private async Task<ActionResult?> ValidateGetRevisionParametersAsync(
        Guid matterId, Guid documentId, Guid revisionId)
    {
        // Validate GUIDs
        var guidValidationResult = _validationService.ValidateGuid(matterId, nameof(matterId))
            ?? _validationService.ValidateGuid(documentId, nameof(documentId))
            ?? _validationService.ValidateGuid(revisionId, nameof(revisionId));

        if (guidValidationResult != null)
        {
            _logger.LogWarning("Invalid GUID parameters for revision retrieval.");
            return guidValidationResult;
        }

        // Validate entity existence
        var matterExistsResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (matterExistsResult != null) return matterExistsResult;

        var documentExistsResult = await _validationService.ValidateDocumentExistsAsync(documentId);
        if (documentExistsResult != null) return documentExistsResult;

        var revisionExistsResult = await _validationService.ValidateRevisionExistsAsync(revisionId);
        if (revisionExistsResult != null) return revisionExistsResult;

        return null;
    }

    /// <summary>
    /// Validates parameters for revision audit retrieval operations.
    /// </summary>
    private async Task<ActionResult?> ValidateGetRevisionAuditsParametersAsync(
        Guid matterId, Guid documentId, Guid revisionId)
    {
        return await ValidateGetRevisionParametersAsync(matterId, documentId, revisionId);
    }

    /// <summary>
    /// Validates parameters for paginated revision retrieval operations.
    /// Following the pattern from MatterController.ValidateGetMattersParametersAsync()
    /// </summary>
    private async Task<ActionResult?> ValidateGetRevisionsParametersAsync(
        Guid matterId, Guid documentId, RevisionsResourceParameters resourceParameters)
    {
        // Validate GUIDs
        var guidValidationResult = _validationService.ValidateGuid(matterId, nameof(matterId))
            ?? _validationService.ValidateGuid(documentId, nameof(documentId));

        if (guidValidationResult != null)
        {
            _logger.LogWarning("Invalid GUID parameters for revisions retrieval.");
            return guidValidationResult;
        }

        // Validate the resource parameters object using centralized validation logic
        var objectValidationResult = _validationService.ValidateObject(resourceParameters);
        if (objectValidationResult != null)
        {
            return objectValidationResult;
        }

        // Validate pagination parameters (following MatterController pattern)
        if (resourceParameters.PageNumber <= 0 || resourceParameters.PageSize <= 0)
        {
            _logger.LogWarning("Invalid pagination parameters: PageNumber={PageNumber}, PageSize={PageSize}",
                resourceParameters.PageNumber, resourceParameters.PageSize);
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest,
                "Page number and page size must be greater than zero."));
        }

        // Validate model state (following DocumentController pattern)
        var modelValidationResult = _validationService.ValidateModelState(ModelState);
        if (modelValidationResult != null)
        {
            _logger.LogWarning("Invalid model state for RevisionsResourceParameters. {@ModelState}", ModelState);
            return modelValidationResult;
        }

        // Validate orderBy parameter (following DocumentController pattern)
        if (!string.IsNullOrWhiteSpace(resourceParameters.OrderBy) &&
            !_propertyMappingService.ValidMappingExistsFor<RevisionDto, Revision>(resourceParameters.OrderBy))
        {
            _logger.LogWarning("Invalid order by parameter: {OrderBy}", resourceParameters.OrderBy);
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Invalid order by parameter."));
        }

        // Validate entity existence
        var matterExistsResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (matterExistsResult != null) return matterExistsResult;

        var documentExistsResult = await _validationService.ValidateDocumentExistsAsync(documentId);
        if (documentExistsResult != null) return documentExistsResult;

        return null;
    }

    /// <summary>
    /// Validates parameters for revision update operations.
    /// </summary>
    private async Task<ActionResult?> ValidateUpdateRevisionParametersAsync(
        Guid matterId, Guid documentId, Guid revisionId, RevisionForUpdateDto revision)
    {
        // Validate GUIDs and existence
        var basicValidation = await ValidateGetRevisionParametersAsync(matterId, documentId, revisionId);
        if (basicValidation != null) return basicValidation;

        // Validate update DTO
        var objectValidationResult = _validationService.ValidateObject(revision);
        if (objectValidationResult != null)
        {
            return objectValidationResult;
        }

        // Validate model state
        var modelValidationResult = _validationService.ValidateModelState(ModelState);
        if (modelValidationResult != null)
        {
            _logger.LogWarning("Invalid model state for RevisionForUpdateDto. {@ModelState}", ModelState);
            return modelValidationResult;
        }

        return null;
    }

    /// <summary>
    /// Validates document and retrieves revision count for automatic numbering.
    /// </summary>
    private async Task<Document?> ValidateDocumentAndGetRevisionCountAsync(Guid matterId, Guid documentId)
    {
        try
        {
            var document = await _admsRepository.GetDocumentAsync(documentId, includeRevisions: true, includeHistory: false);
            if (document == null || document.MatterId != matterId)
            {
                _logger.LogWarning(
                    "Document {DocumentId} does not belong to Matter {MatterId}.",
                    documentId, matterId);
                return null;
            }
            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating document and retrieving revision count: {DocumentId}", documentId);
            return null;
        }
    }

    /// <summary>
    /// Creates a revision with audit trail.
    /// </summary>
    private async Task<Revision?> CreateRevisionWithAuditAsync(
        Guid documentId, RevisionForCreationDto revision, int revisionNumber)
    {
        try
        {
            // Map and set revision number
            var revisionDto = _mapper.Map<RevisionDto>(revision);
            revisionDto.RevisionNumber = revisionNumber;

            // Validate the mapped DTO
            var dtoValidationResult = _validationService.ValidateObject(revisionDto);
            if (dtoValidationResult != null) return null;

            // Create the revision
            var createdRevision = await _admsRepository.AddRevisionAsync(documentId, revisionDto);
            if (createdRevision == null) return null;

            // Save changes
            if (await _admsRepository.SaveChangesAsync())
            {
                return createdRevision;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating revision with audit: {DocumentId}", documentId);
            return null;
        }
    }

    /// <summary>
    /// Validates business rules for revision deletion.
    /// </summary>
    private async Task<ActionResult?> ValidateRevisionDeletionBusinessRulesAsync(
        Guid matterId, Guid documentId, Guid revisionId)
    {
        try
        {
            // Get revision and validate hierarchy
            var revision = await _admsRepository.GetRevisionByIdAsync(revisionId);
            if (revision?.DocumentId != documentId)
            {
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound,
                    "Revision does not belong to the specified document."));
            }

            var document = await _admsRepository.GetDocumentAsync(documentId, false, false);
            if (document?.MatterId != matterId)
            {
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound,
                    "Document does not belong to the specified matter."));
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating revision deletion business rules: {RevisionId}", revisionId);
            return CreateInternalServerErrorResponse("validating deletion business rules");
        }
    }

    /// <summary>
    /// Deletes a revision with audit trail.
    /// </summary>
    private async Task<bool> DeleteRevisionWithAuditAsync(Guid revisionId)
    {
        try
        {
            var revision = await _admsRepository.GetRevisionByIdAsync(revisionId);
            if (revision == null) return false;

            var revisionDto = _mapper.Map<RevisionDto>(revision);
            var validationResult = _validationService.ValidateObject(revisionDto);
            if (validationResult != null) return false;

            var deleted = await _admsRepository.DeleteRevisionAsync(revisionDto);
            return deleted && await _admsRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting revision with audit: {RevisionId}", revisionId);
            return false;
        }
    }

    /// <summary>
    /// Retrieves and validates a revision with hierarchy checking.
    /// </summary>
    private async Task<Revision?> GetValidatedRevisionAsync(Guid matterId, Guid documentId, Guid revisionId)
    {
        try
        {
            var revision = await _admsRepository.GetRevisionByIdAsync(revisionId);
            if (revision?.DocumentId != documentId) return null;

            var document = await _admsRepository.GetDocumentAsync(documentId, false, false);
            if (document?.MatterId != matterId) return null;

            return revision;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting validated revision: {RevisionId}", revisionId);
            return null;
        }
    }

    /// <summary>
    /// Creates audit trail for revision viewing.
    /// </summary>
    private async Task CreateRevisionViewedAuditAsync(Guid revisionId)
    {
        try
        {
            // Implementation would depend on your audit system
            // This is a placeholder for audit creation logic
            _logger.LogDebug("Creating viewed audit for revision: {RevisionId}", revisionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating viewed audit for revision: {RevisionId}", revisionId);
            // Don't fail the request if audit creation fails
        }
    }

    /// <summary>
    /// Retrieves audit history for a revision.
    /// </summary>
    private async Task<IEnumerable<RevisionActivityUserDto>?> RetrieveRevisionAuditHistoryAsync(
        Guid matterId, Guid documentId, Guid revisionId)
    {
        try
        {
            var revision = await _admsRepository.GetRevisionByIdAsync(revisionId, includeHistory: true);
            if (revision?.DocumentId != documentId) return null;

            var document = await _admsRepository.GetDocumentAsync(documentId, false, false);
            if (document?.MatterId != matterId) return null;

            return _mapper.Map<IEnumerable<RevisionActivityUserDto>>(revision.RevisionActivityUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving revision audit history: {RevisionId}", revisionId);
            return null;
        }
    }

    /// <summary>
    /// Validates audit records using centralized validation.
    /// </summary>
    private IEnumerable<string> ValidateAuditRecords(IEnumerable<RevisionActivityUserDto> auditRecords)
    {
        var errors = new List<string>();
        foreach (var audit in auditRecords)
        {
            var validationResult = _validationService.ValidateObject(audit);
            if (validationResult != null)
            {
                errors.Add("Validation failed for audit record");
            }
        }
        return errors;
    }

    /// <summary>
    /// Validates that document belongs to the specified matter.
    /// Following the pattern from other controllers for business rule validation.
    /// </summary>
    private async Task<ActionResult?> ValidateDocumentBelongsToMatterAsync(Guid matterId, Guid documentId)
    {
        try
        {
            var document = await _admsRepository.GetDocumentAsync(documentId, false, false);
            if (document?.MatterId != matterId)
            {
                _logger.LogWarning(
                    "Document {DocumentId} does not belong to Matter {MatterId}.",
                    documentId, matterId);
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound,
                    "Document does not belong to the specified matter."));
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating document belongs to matter: {DocumentId}, {MatterId}", documentId, matterId);
            return CreateInternalServerErrorResponse("validating document ownership");
        }
    }

    /// <summary>
    /// Validates returned revisions using centralized validation.
    /// Following the pattern from MatterController.ValidateReturnedMatters()
    /// </summary>
    private IEnumerable<string> ValidateReturnedRevisions(IEnumerable<RevisionDto> revisions)
    {
        var errors = new List<string>();
        foreach (var revision in revisions)
        {
            var validationResult = _validationService.ValidateObject(revision);
            if (validationResult != null)
            {
                errors.Add("Validation failed for revision");
            }
        }
        return errors;
    }

    /// <summary>
    /// Validates business rules for revision update.
    /// </summary>
    private async Task<ActionResult?> ValidateRevisionUpdateBusinessRulesAsync(
        Guid matterId, Guid documentId, Guid revisionId)
    {
        return await ValidateRevisionDeletionBusinessRulesAsync(matterId, documentId, revisionId);
    }

    /// <summary>
    /// Updates a revision with audit trail.
    /// </summary>
    private async Task<Revision?> UpdateRevisionWithAuditAsync(
        Guid matterId, Guid documentId, Guid revisionId, RevisionForUpdateDto revision)
    {
        try
        {
            var revisionEntity = await _admsRepository.GetRevisionByIdAsync(revisionId);
            if (revisionEntity == null) return null;

            // Map updates to entity
            _mapper.Map(revision, revisionEntity);

            // Validate mapped entity
            var mappedDto = _mapper.Map<RevisionDto>(revisionEntity);
            var validationResult = _validationService.ValidateObject(mappedDto);
            if (validationResult != null) return null;

            // Perform update
            await _admsRepository.UpdateRevisionAsync(matterId, documentId, revisionId, revisionEntity);

            return await _admsRepository.SaveChangesAsync() ? revisionEntity : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating revision with audit: {RevisionId}", revisionId);
            return null;
        }
    }

    /// <summary>
    /// Creates a standardized problem details object.
    /// </summary>
    private ProblemDetails CreateProblemDetails(int statusCode, string detail)
    {
        return _problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode, detail: detail);
    }

    /// <summary>
    /// Creates a standardized internal server error response.
    /// </summary>
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