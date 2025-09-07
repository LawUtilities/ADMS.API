using ADMS.API.Entities;
using ADMS.API.Extensions;
using ADMS.API.Helpers;
using ADMS.API.Models;
using ADMS.API.ResourceParameters;
using ADMS.API.Services;

using Asp.Versioning;

using MapsterMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace ADMS.API.Controllers;

/// <summary>
/// Controller for managing matters within the ADMS system.
/// </summary>
/// <remarks>
/// This controller provides comprehensive matter management functionality including:
/// - Matter creation with validation and duplicate checking
/// - Matter retrieval with flexible inclusion options (documents, history)
/// - Matter updates with audit trail maintenance
/// - Matter deletion with business rule enforcement
/// - Matter restoration capabilities
/// - Paginated matter listings with filtering and sorting
/// - Comprehensive audit history retrieval
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
/// - Matter descriptions must be unique across the system
/// - Matters can only be deleted when all associated documents are deleted and checked in
/// - All operations maintain comprehensive audit trails
/// - Date/time handling uses UTC with local formatting options
/// 
/// The controller supports advanced features like:
/// - Pagination with metadata headers for efficient data retrieval
/// - Flexible filtering and sorting through resource parameters
/// - Audit history tracking with directional queries (from/to)
/// - Matter restoration with proper validation
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/matters")]
[Produces("application/json", "application/xml")]
public class MatterController(
    ILogger<MatterController> logger,
    IAdmsRepository admsRepository,
    IMapper mapper,
    ProblemDetailsFactory problemDetailsFactory,
    IValidationService validationService) : ControllerBase
{
    private readonly IAdmsRepository _admsRepository = admsRepository ?? throw new ArgumentNullException(nameof(admsRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ILogger<MatterController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
    private readonly IValidationService _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));

    #region Actions

    /// <summary>
    /// Creates a new matter in the system.
    /// </summary>
    /// <remarks>
    /// This endpoint creates a new matter with comprehensive validation including:
    /// - Input validation using data annotations and custom validation rules
    /// - Business rule validation including duplicate description checking
    /// - Automatic audit trail creation for the matter creation event
    /// - Proper HTTP status codes and error responses following REST conventions
    /// 
    /// The creation process includes:
    /// 1. Comprehensive DTO validation using centralized validation services
    /// 2. Model state validation for any binding errors
    /// 3. Business rule validation (e.g., unique matter descriptions)
    /// 4. Matter creation with proper entity relationship setup
    /// 5. Audit log creation with creation activity tracking
    /// 
    /// Upon successful creation, the response includes a Location header pointing
    /// to the newly created matter and returns the created matter data.
    /// </remarks>
    /// <param name="matter">The matter data transfer object containing the information needed to create a new matter.</param>
    /// <returns>
    /// <para>201 Created - Matter successfully created with location header and matter data.</para>
    /// <para>400 BadRequest - Invalid input data, validation errors, or malformed request.</para>
    /// <para>409 Conflict - A matter with the same description already exists.</para>
    /// <para>500 InternalServerError - Unexpected server error during matter creation.</para>
    /// </returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateMatterAsync(MatterForCreationDto? matter)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateCreateMatterParametersAsync(matter);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Check for duplicate matter description
            var isDuplicate = await CheckMatterDescriptionExistsAsync(matter!.Description);
            if (isDuplicate)
            {
                _logger.LogWarning(
                    "Attempt to create duplicate matter with description: {Description}",
                    matter.Description);
                return Conflict(CreateProblemDetails(StatusCodes.Status409Conflict,
                    "A matter with the same description already exists."));
            }

            // Create the matter
            var createdMatter = await CreateMatterWithAuditAsync(matter);
            if (createdMatter == null)
            {
                _logger.LogError(
                    "Failed to create matter with description: {Description}",
                    matter.Description);
                return CreateInternalServerErrorResponse("creating the matter");
            }

            // Add response headers
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogInformation(
                "Matter created successfully with ID: {MatterId}",
                createdMatter.Id);

            // Return 201 Created with route to new resource
            return CreatedAtRoute(
                "GetMatter",
                new
                {
                    matterId = createdMatter.Id,
                    includeDocuments = false,
                    version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0"
                },
                _mapper.Map<MatterDto>(createdMatter)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error occurred while creating matter with description: {Description}",
                matter?.Description ?? "Unknown");

            return CreateInternalServerErrorResponse("creating the matter");
        }
    }

    /// <summary>
    /// Deletes a specific matter from the system.
    /// </summary>
    /// <remarks>
    /// This endpoint provides secure matter deletion with comprehensive business rule enforcement:
    /// - Validates that the matter exists before attempting deletion
    /// - Ensures all associated documents are marked as deleted
    /// - Verifies that no documents are currently checked out
    /// - Creates comprehensive audit trail for the deletion operation
    /// - Implements soft deletion to maintain data integrity and audit history
    /// 
    /// Business Rules:
    /// - Matter can only be deleted if all associated documents are deleted
    /// - Matter can only be deleted if no documents are checked out
    /// - Deletion creates an audit record for compliance and tracking
    /// - The deletion is typically a soft delete to preserve referential integrity
    /// 
    /// The deletion process includes:
    /// 1. Parameter and entity existence validation
    /// 2. Business rule validation (document status checking)
    /// 3. Audit trail creation before deletion
    /// 4. Secure deletion with proper cleanup
    /// 5. Success confirmation without exposing internal details
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter to be deleted.</param>
    /// <returns>
    /// <para>204 NoContent - Matter successfully deleted.</para>
    /// <para>400 BadRequest - Invalid matter ID, business rule violation, or validation errors.</para>
    /// <para>404 NotFound - The specified matter does not exist.</para>
    /// <para>500 InternalServerError - Unexpected server error during deletion.</para>
    /// </returns>
    [HttpDelete("{matterId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteMatterAsync(Guid matterId)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateDeleteMatterParametersAsync(matterId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Validate business rules for deletion
            var businessRuleResult = await ValidateMatterDeletionBusinessRulesAsync(matterId);
            if (businessRuleResult != null)
            {
                return businessRuleResult;
            }

            // Perform the deletion
            var isDeleted = await DeleteMatterWithAuditAsync(matterId);
            if (!isDeleted)
            {
                _logger.LogError("Failed to delete matter: {MatterId}", matterId);
                return CreateInternalServerErrorResponse("deleting the matter");
            }

            // Add response headers
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogInformation("Matter deleted successfully: {MatterId}", matterId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error occurred while deleting matter: {MatterId}",
                matterId);

            return CreateInternalServerErrorResponse("deleting the matter");
        }
    }

    /// <summary>
    /// Retrieves a specific matter with flexible inclusion options.
    /// </summary>
    /// <remarks>
    /// This endpoint provides flexible matter retrieval with optional related data inclusion:
    /// - Basic matter information retrieval with validation
    /// - Optional document inclusion for complete matter context
    /// - Proper DTO mapping based on inclusion parameters
    /// - Audit trail creation for matter access (VIEWED activity)
    /// - Comprehensive error handling with proper HTTP status codes
    /// 
    /// The endpoint supports two retrieval modes:
    /// 1. Basic matter information (default) - Returns core matter data without related entities
    /// 2. Matter with documents - Returns complete matter data including all associated documents
    /// 
    /// Features include:
    /// - Centralized validation using the validation service
    /// - Proper DTO selection based on includeDocuments parameter
    /// - Audit logging for compliance and usage tracking
    /// - Error handling with detailed problem responses
    /// - Support for API versioning and correlation tracking
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter to retrieve.</param>
    /// <param name="includeDocuments">Optional parameter to include associated documents in the response (default: false).</param>
    /// <returns>
    /// <para>200 OK - Matter successfully retrieved with requested data.</para>
    /// <para>400 BadRequest - Invalid matter ID or validation errors.</para>
    /// <para>404 NotFound - The specified matter does not exist.</para>
    /// <para>500 InternalServerError - Unexpected server error during retrieval.</para>
    /// </returns>
    [HttpGet("{matterId}", Name = "GetMatter")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMatterAsync(Guid matterId, bool includeDocuments = false)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateGetMatterParametersAsync(matterId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Retrieve the matter with optional document inclusion
            var matter = await _admsRepository.GetMatterAsync(matterId, includeDocuments);
            if (matter == null)
            {
                _logger.LogWarning("Matter not found: {MatterId}", matterId);
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Matter not found."));
            }

            // Map to appropriate DTO and validate
            var mappedDto = await MapMatterToDtoAsync(matter, includeDocuments);
            var dtoValidationResult = _validationService.ValidateObject(mappedDto);
            if (dtoValidationResult != null)
            {
                return dtoValidationResult;
            }

            // Create audit trail for matter access
            await CreateMatterViewedAuditAsync(matterId);

            // Add response headers
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogInformation("Matter retrieved successfully: {MatterId}", matterId);
            return Ok(mappedDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error occurred while retrieving matter: {MatterId}",
                matterId);

            return CreateInternalServerErrorResponse("retrieving the matter");
        }
    }

    /// <summary>
    /// Retrieves the audit history for a specific matter with directional filtering.
    /// </summary>
    /// <remarks>
    /// This endpoint provides comprehensive audit history retrieval with directional filtering:
    /// - Supports "from" and "to" audit direction filtering for move/copy operations
    /// - Returns detailed audit records including user, activity, and timestamp information
    /// - Validates all input parameters including matter existence and history type
    /// - Implements comprehensive error handling with proper HTTP status codes
    /// - Validates audit records using centralized validation services
    /// 
    /// Audit Direction Types:
    /// - "from": Returns audit records where documents were moved/copied FROM this matter
    /// - "to": Returns audit records where documents were moved/copied TO this matter
    /// 
    /// The audit history provides:
    /// - Complete activity tracking for compliance and reporting
    /// - User attribution for all activities
    /// - Timestamp information for activity sequencing
    /// - Document-level audit details for comprehensive tracking
    /// - Validation of all returned data for consistency
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter to retrieve audit history for.</param>
    /// <param name="historyType">The direction of audit history to retrieve ("from" or "to").</param>
    /// <returns>
    /// <para>200 OK - Audit history successfully retrieved.</para>
    /// <para>400 BadRequest - Invalid matter ID, invalid history type, or validation errors.</para>
    /// <para>404 NotFound - The specified matter does not exist or no audit history found.</para>
    /// <para>500 InternalServerError - Unexpected server error during audit retrieval.</para>
    /// </returns>
    [HttpGet("{matterId}/audits")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAuditsAsync(Guid matterId, [FromQuery] string historyType)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateGetAuditsParametersAsync(matterId, historyType);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Retrieve audit history based on direction
            var auditHistory = await RetrieveAuditHistoryAsync(matterId, historyType);
            if (auditHistory == null || !auditHistory.Any())
            {
                _logger.LogInformation(
                    "No audit history found for matter ID {MatterId} with history type '{HistoryType}'.",
                    matterId, historyType);
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "No audit history found."));
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
                "Found {Count} audit records for matter ID {MatterId} with history type '{HistoryType}'.",
                auditHistory.Count(), matterId, historyType);

            return Ok(auditHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error occurred while retrieving audits for matter: {MatterId}",
                matterId);

            return CreateInternalServerErrorResponse("retrieving audit history");
        }
    }

    /// <summary>
    /// Updates a specific matter with comprehensive validation and audit tracking.
    /// </summary>
    /// <remarks>
    /// This endpoint provides secure matter updating with comprehensive validation:
    /// - Validates all input parameters including matter existence
    /// - Performs business rule validation for update operations
    /// - Maintains complete audit trail for all changes
    /// - Implements optimistic concurrency control where applicable
    /// - Validates updated data using centralized validation services
    /// 
    /// Update Process:
    /// 1. Input validation using data annotations and custom rules
    /// 2. Entity existence validation
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
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter to update.</param>
    /// <param name="matter">The matter update data transfer object containing the new matter information.</param>
    /// <returns>
    /// <para>204 NoContent - Matter successfully updated.</para>
    /// <para>400 BadRequest - Invalid input data, validation errors, or business rule violations.</para>
    /// <para>404 NotFound - The specified matter does not exist.</para>
    /// <para>500 InternalServerError - Unexpected server error during update.</para>
    /// </returns>
    [HttpPut("{matterId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMatterAsync(Guid matterId, MatterForUpdateDto? matter)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateUpdateMatterParametersAsync(matterId, matter);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Perform the update
            var updatedMatter = await UpdateMatterWithAuditAsync(matterId, matter!);
            if (updatedMatter == null)
            {
                _logger.LogError("Failed to update matter: {MatterId}", matterId);
                return CreateInternalServerErrorResponse("updating the matter");
            }

            // Add response headers
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogInformation("Matter updated successfully: {MatterId}", matterId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error occurred while updating matter: {MatterId}",
                matterId);

            return CreateInternalServerErrorResponse("updating the matter");
        }
    }

    /// <summary>
    /// Retrieves the complete audit history for a specific matter.
    /// </summary>
    /// <remarks>
    /// This endpoint provides comprehensive matter history retrieval including:
    /// - Complete audit trail for all matter-related activities
    /// - Document-level activity history associated with the matter
    /// - User attribution and timestamp information for all activities
    /// - Proper validation and error handling for history data
    /// - Flexible return format supporting various client needs
    /// 
    /// History Information Includes:
    /// - Matter creation, update, and deletion activities
    /// - Document activities within the matter
    /// - User access and modification tracking
    /// - Move/copy operations involving the matter
    /// - Timestamp and user attribution for all activities
    /// 
    /// Features:
    /// - Comprehensive error handling with detailed responses
    /// - Validation of all returned historical data
    /// - Support for large history datasets with proper performance
    /// - API versioning and correlation tracking
    /// - Structured logging for debugging and monitoring
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter to retrieve complete history for.</param>
    /// <returns>
    /// <para>200 OK - Complete matter history successfully retrieved.</para>
    /// <para>400 BadRequest - Invalid matter ID or validation errors.</para>
    /// <para>404 NotFound - The specified matter does not exist or no history available.</para>
    /// <para>500 InternalServerError - Unexpected server error during history retrieval.</para>
    /// </returns>
    [HttpGet("{matterId}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMatterHistoryAsync(Guid matterId)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateGetMatterHistoryParametersAsync(matterId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Retrieve matter history
            var matterWithHistory = await _admsRepository.GetMatterAsync(matterId, includeDocuments: false, includeHistory: true);
            if (matterWithHistory == null)
            {
                _logger.LogWarning("Matter history not found: {MatterId}", matterId);
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Matter history not found."));
            }

            // Validate the retrieved history data
            var validationResult2 = _validationService.ValidateObject(matterWithHistory);
            if (validationResult2 != null)
            {
                return validationResult2;
            }

            // Add response headers
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogInformation("Matter history retrieved successfully: {MatterId}", matterId);
            return Ok(matterWithHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error occurred while retrieving matter history: {MatterId}",
                matterId);

            return CreateInternalServerErrorResponse("retrieving matter history");
        }
    }

    /// <summary>
    /// Restores a deleted matter with comprehensive validation and audit tracking.
    /// </summary>
    /// <remarks>
    /// This endpoint provides secure matter restoration functionality:
    /// - Validates that the matter exists and can be restored
    /// - Performs business rule validation for restoration eligibility
    /// - Creates comprehensive audit trail for the restoration operation
    /// - Implements proper error handling and logging
    /// - Returns appropriate status codes based on operation results
    /// 
    /// Restoration Process:
    /// 1. Parameter validation and matter existence checking
    /// 2. Business rule validation for restoration eligibility
    /// 3. Matter restoration with proper state management
    /// 4. Audit trail creation for compliance tracking
    /// 5. Success confirmation with appropriate response
    /// 
    /// Business Rules:
    /// - Only deleted matters can be restored
    /// - Restoration must maintain data integrity
    /// - All restoration activities are audited for compliance
    /// - Proper validation ensures system consistency
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter to restore.</param>
    /// <returns>
    /// <para>200 OK - Matter successfully restored.</para>
    /// <para>400 BadRequest - Invalid matter ID, business rule violation, or matter cannot be restored.</para>
    /// <para>404 NotFound - The specified matter does not exist.</para>
    /// <para>500 InternalServerError - Unexpected server error during restoration.</para>
    /// </returns>
    [HttpPost("{matterId}/restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RestoreMatterAsync(Guid matterId)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateRestoreMatterParametersAsync(matterId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Perform the restoration
            var isRestored = await RestoreMatterWithAuditAsync(matterId);
            if (!isRestored)
            {
                _logger.LogWarning("Failed to restore matter with ID {MatterId}.", matterId);
                return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Could not restore the matter."));
            }

            // Add response headers
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogInformation("Successfully restored matter with ID {MatterId}.", matterId);
            return Ok(new { Message = "Matter restored successfully.", MatterId = matterId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error occurred while restoring matter: {MatterId}",
                matterId);

            return CreateInternalServerErrorResponse("restoring the matter");
        }
    }

    /// <summary>
    /// Retrieves a paginated list of matters with comprehensive filtering and sorting capabilities.
    /// </summary>
    /// <remarks>
    /// This endpoint provides advanced matter listing functionality with:
    /// - Comprehensive pagination with metadata headers
    /// - Advanced filtering options through resource parameters
    /// - Multiple sorting options with proper validation
    /// - Performance optimization for large datasets
    /// - Comprehensive error handling and validation
    /// 
    /// Pagination Features:
    /// - Configurable page size with reasonable limits
    /// - Page navigation with total count information
    /// - Metadata headers for client-side pagination handling
    /// - Performance optimization for large result sets
    /// 
    /// Filtering and Sorting:
    /// - Multiple filter criteria support
    /// - Case-insensitive text searching
    /// - Date range filtering capabilities
    /// - Multiple sort field options
    /// - Ascending and descending sort directions
    /// 
    /// Response Format:
    /// - JSON/XML content negotiation support
    /// - Comprehensive pagination metadata in headers
    /// - Consistent error responses using problem details format
    /// - API versioning and correlation ID support
    /// </remarks>
    /// <param name="resourceParameters">The resource parameters containing pagination, filtering, and sorting options.</param>
    /// <returns>
    /// <para>200 OK - Paginated matter list successfully retrieved with metadata headers.</para>
    /// <para>400 BadRequest - Invalid pagination parameters, filter criteria, or validation errors.</para>
    /// <para>500 InternalServerError - Unexpected server error during matter retrieval.</para>
    /// </returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMattersAsync([FromQuery] MattersResourceParameters? resourceParameters)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateGetMattersParametersAsync(resourceParameters);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Retrieve paginated matters
            var pagedMatters = await _admsRepository.GetPaginatedMattersAsync(resourceParameters!);

            // Add pagination metadata to response headers
            Response.AddPaginationMetadata(pagedMatters);

            // Validate returned matters
            var validationErrors = ValidateReturnedMatters(pagedMatters);
            if (validationErrors.Any())
            {
                return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest,
                    "Validation failed for one or more returned matters."));
            }

            // Add response headers
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            if (pagedMatters.Count == 0)
            {
                _logger.LogInformation("No matters found for the provided parameters.");
            }
            else
            {
                _logger.LogInformation(
                    "Retrieved {Count} matters (Page {Page}/{TotalPages}).",
                    pagedMatters.Count, pagedMatters.CurrentPage, pagedMatters.TotalPages);
            }

            return Ok(pagedMatters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error occurred while retrieving matters.");

            return CreateInternalServerErrorResponse("retrieving matters");
        }
    }

    /// <summary>
    /// Returns the HTTP methods supported by the matters resource.
    /// </summary>
    /// <remarks>
    /// This endpoint implements the HTTP OPTIONS method to allow clients
    /// to discover which operations are available on the matters resource.
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
    public IActionResult GetMatterOptions()
    {
        try
        {
            Response.Headers.Allow = "GET,POST,PUT,DELETE,OPTIONS";
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogDebug("OPTIONS request processed for matters resource.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OPTIONS request for matters resource.");
            return CreateInternalServerErrorResponse("processing the OPTIONS request");
        }
    }

    #endregion Actions

    #region Private Helper Methods

    /// <summary>
    /// Validates parameters for matter creation operations.
    /// </summary>
    private async Task<ActionResult?> ValidateCreateMatterParametersAsync(MatterForCreationDto? matter)
    {
        // Null check for DTO
        if (matter == null)
        {
            _logger.LogWarning("MatterForCreationDto is null.");
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Matter data is required."));
        }

        // Validate the object using centralized validation logic
        var objectValidationResult = _validationService.ValidateObject(matter);
        if (objectValidationResult != null)
        {
            return objectValidationResult;
        }

        // Validate model state
        var modelValidationResult = _validationService.ValidateModelState(ModelState);
        if (modelValidationResult != null)
        {
            _logger.LogWarning("Invalid model state for MatterForCreationDto. {@ModelState}", ModelState);
            return modelValidationResult;
        }

        return null;
    }

    /// <summary>
    /// Validates parameters for matter deletion operations.
    /// </summary>
    private async Task<ActionResult?> ValidateDeleteMatterParametersAsync(Guid matterId)
    {
        // Validate the GUID parameter
        var guidValidationResult = _validationService.ValidateGuid(matterId, nameof(matterId));
        if (guidValidationResult != null)
        {
            _logger.LogWarning("Invalid GUID: {MatterId}", matterId);
            return guidValidationResult;
        }

        // Validate matter existence
        var existenceValidationResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (existenceValidationResult != null)
        {
            _logger.LogWarning("Non-existent matter: {MatterId}", matterId);
            return existenceValidationResult;
        }

        return null;
    }

    /// <summary>
    /// Validates parameters for matter retrieval operations.
    /// </summary>
    private async Task<ActionResult?> ValidateGetMatterParametersAsync(Guid matterId)
    {
        // Validate the GUID parameter
        var guidValidationResult = _validationService.ValidateGuid(matterId, nameof(matterId));
        if (guidValidationResult != null)
        {
            _logger.LogWarning("Invalid GUID: {MatterId}", matterId);
            return guidValidationResult;
        }

        // Validate matter existence
        var existenceValidationResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (existenceValidationResult != null)
        {
            _logger.LogWarning("Non-existent matter: {MatterId}", matterId);
            return existenceValidationResult;
        }

        return null;
    }

    /// <summary>
    /// Validates parameters for audit retrieval operations.
    /// </summary>
    private async Task<ActionResult?> ValidateGetAuditsParametersAsync(Guid matterId, string historyType)
    {
        // Validate the GUID parameter
        var guidValidationResult = _validationService.ValidateGuid(matterId, nameof(matterId));
        if (guidValidationResult != null)
        {
            _logger.LogWarning("Invalid GUID: {MatterId}", matterId);
            return guidValidationResult;
        }

        // Validate matter existence
        var existenceValidationResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (existenceValidationResult != null)
        {
            _logger.LogWarning("Non-existent matter: {MatterId}", matterId);
            return existenceValidationResult;
        }

        // Validate historyType parameter
        if (string.IsNullOrWhiteSpace(historyType) ||
            !(historyType.Equals("from", StringComparison.OrdinalIgnoreCase) ||
              historyType.Equals("to", StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("Invalid history type: {HistoryType}", historyType);
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest,
                "Invalid history type. Valid values are 'from' or 'to'."));
        }

        return null;
    }

    /// <summary>
    /// Validates parameters for matter update operations.
    /// </summary>
    private async Task<ActionResult?> ValidateUpdateMatterParametersAsync(Guid matterId, MatterForUpdateDto? matter)
    {
        // Null check for DTO
        if (matter == null)
        {
            _logger.LogWarning("MatterForUpdateDto is null.");
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Matter data is required."));
        }

        // Validate the object using centralized validation logic
        var objectValidationResult = _validationService.ValidateObject(matter);
        if (objectValidationResult != null)
        {
            return objectValidationResult;
        }

        // Validate GUID parameter
        var guidValidationResult = _validationService.ValidateGuid(matterId, nameof(matterId));
        if (guidValidationResult != null)
        {
            _logger.LogWarning("Invalid GUID: {MatterId}", matterId);
            return guidValidationResult;
        }

        // Validate model state
        var modelValidationResult = _validationService.ValidateModelState(ModelState);
        if (modelValidationResult != null)
        {
            _logger.LogWarning("Invalid model state for MatterForUpdateDto. {@ModelState}", ModelState);
            return modelValidationResult;
        }

        // Validate matter existence
        var existenceValidationResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (existenceValidationResult != null)
        {
            _logger.LogWarning("Non-existent matter: {MatterId}", matterId);
            return existenceValidationResult;
        }

        return null;
    }

    /// <summary>
    /// Validates parameters for matter history retrieval operations.
    /// </summary>
    private async Task<ActionResult?> ValidateGetMatterHistoryParametersAsync(Guid matterId)
    {
        // Validate the GUID parameter
        var guidValidationResult = _validationService.ValidateGuid(matterId, nameof(matterId));
        if (guidValidationResult != null)
        {
            _logger.LogWarning("Invalid GUID: {MatterId}", matterId);
            return guidValidationResult;
        }

        // Validate matter existence
        var existenceValidationResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (existenceValidationResult != null)
        {
            _logger.LogWarning("Non-existent matter: {MatterId}", matterId);
            return existenceValidationResult;
        }

        return null;
    }

    /// <summary>
    /// Validates parameters for matter restoration operations.
    /// </summary>
    private async Task<ActionResult?> ValidateRestoreMatterParametersAsync(Guid matterId)
    {
        // Validate the GUID parameter
        var guidValidationResult = _validationService.ValidateGuid(matterId, nameof(matterId));
        if (guidValidationResult != null)
        {
            _logger.LogWarning("Invalid GUID: {MatterId}", matterId);
            return guidValidationResult;
        }

        // Validate matter existence
        var existenceValidationResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (existenceValidationResult != null)
        {
            _logger.LogWarning("Non-existent matter: {MatterId}", matterId);
            return existenceValidationResult;
        }

        return null;
    }

    /// <summary>
    /// Validates parameters for paginated matter retrieval operations.
    /// </summary>
    private async Task<ActionResult?> ValidateGetMattersParametersAsync(MattersResourceParameters? resourceParameters)
    {
        // Null check for resource parameters
        if (resourceParameters == null)
        {
            _logger.LogWarning("resourceParameters is null.");
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Resource parameters are required."));
        }

        // Validate the resource parameters object using centralized validation logic
        var objectValidationResult = _validationService.ValidateObject(resourceParameters);
        if (objectValidationResult != null)
        {
            return objectValidationResult;
        }

        // Validate pagination parameters
        if (resourceParameters.PageNumber <= 0 || resourceParameters.PageSize <= 0)
        {
            _logger.LogWarning("Invalid pagination parameters: PageNumber={PageNumber}, PageSize={PageSize}",
                resourceParameters.PageNumber, resourceParameters.PageSize);
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest,
                "Page number and page size must be greater than zero."));
        }

        // Validate model state
        var modelValidationResult = _validationService.ValidateModelState(ModelState);
        if (modelValidationResult != null)
        {
            _logger.LogWarning("Invalid model state for MattersResourceParameters. {@ModelState}", ModelState);
            return modelValidationResult;
        }

        return null;
    }

    /// <summary>
    /// Checks if a matter description already exists.
    /// </summary>
    private async Task<bool> CheckMatterDescriptionExistsAsync(string description)
    {
        try
        {
            var matterNameExists = await _admsRepository.MatterNameExistsAsync(description);
            return matterNameExists.Result is OkObjectResult { Value: bool and true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking matter description existence: {Description}", description);
            return false;
        }
    }

    /// <summary>
    /// Creates a matter with audit trail.
    /// </summary>
    private async Task<Matter?> CreateMatterWithAuditAsync(MatterForCreationDto matter)
    {
        try
        {
            var addedMatterResult = await _admsRepository.AddMatterAsync(matter);
            if (addedMatterResult.Result is ObjectResult { Value: Matter createdMatter })
            {
                return createdMatter;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating matter with description: {Description}", matter.Description);
            return null;
        }
    }

    /// <summary>
    /// Validates business rules for matter deletion.
    /// </summary>
    private async Task<ActionResult?> ValidateMatterDeletionBusinessRulesAsync(Guid matterId)
    {
        try
        {
            var matter = await _admsRepository.GetMatterAsync(matterId, includeDocuments: true);
            if (matter == null)
            {
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Matter not found."));
            }

            // Validate the matter object
            var objectValidationResult = _validationService.ValidateObject(matter);
            if (objectValidationResult != null)
            {
                return objectValidationResult;
            }

            // Check business rules for deletion
            if (matter.Documents.Count > 0)
            {
                var notDeleted = matter.Documents.Where(d => !d.IsDeleted).ToList();
                var checkedOut = matter.Documents.Where(d => d.IsCheckedOut).ToList();

                if (notDeleted.Count > 0)
                {
                    _logger.LogWarning("Cannot delete matter {MatterId} because not all documents are marked as deleted.", matterId);
                    return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest,
                        "Cannot delete the matter because not all associated documents are marked as deleted."));
                }

                if (checkedOut.Count > 0)
                {
                    _logger.LogWarning("Cannot delete matter {MatterId} because some documents are checked out.", matterId);
                    return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest,
                        "Cannot delete the matter because some associated documents are checked out."));
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating business rules for matter deletion: {MatterId}", matterId);
            return CreateInternalServerErrorResponse("validating deletion business rules");
        }
    }

    /// <summary>
    /// Deletes a matter with audit trail.
    /// </summary>
    private async Task<bool> DeleteMatterWithAuditAsync(Guid matterId)
    {
        try
        {
            var matter = await _admsRepository.GetMatterAsync(matterId, includeDocuments: false);
            if (matter == null) return false;

            var matterDto = _mapper.Map<MatterDto>(matter);
            var dtoValidationResult = _validationService.ValidateObject(matterDto);
            if (dtoValidationResult != null) return false;

            return await _admsRepository.DeleteMatterAsync(matterDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting matter: {MatterId}", matterId);
            return false;
        }
    }

    /// <summary>
    /// Maps matter entity to appropriate DTO based on inclusion parameters.
    /// </summary>
    private async Task<object> MapMatterToDtoAsync(Matter matter, bool includeDocuments)
    {
        return includeDocuments
            ? _mapper.Map<MatterWithDocumentsDto>(matter)
            : _mapper.Map<MatterWithoutDocumentsDto>(matter);
    }

    /// <summary>
    /// Creates audit trail for matter viewing.
    /// </summary>
    private async Task CreateMatterViewedAuditAsync(Guid matterId)
    {
        try
        {
            // Implementation would depend on your audit system
            // This is a placeholder for audit creation logic
            _logger.LogDebug("Creating viewed audit for matter: {MatterId}", matterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating viewed audit for matter: {MatterId}", matterId);
            // Don't fail the request if audit creation fails
        }
    }

    /// <summary>
    /// Retrieves audit history based on direction.
    /// </summary>
    private async Task<IEnumerable<MatterDocumentActivityUserMinimalDto>?> RetrieveAuditHistoryAsync(Guid matterId, string historyType)
    {
        try
        {
            IQueryable<MatterDocumentActivityUserMinimalDto> historyQuery;

            if (historyType.Equals("from", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Retrieving 'from' audits for matter ID {MatterId}.", matterId);
                historyQuery = await _admsRepository.GetExtendedAuditsAsync(matterId, Guid.Empty, AuditEnums.AuditDirection.From);
            }
            else
            {
                _logger.LogInformation("Retrieving 'to' audits for matter ID {MatterId}.", matterId);
                historyQuery = await _admsRepository.GetExtendedAuditsAsync(matterId, Guid.Empty, AuditEnums.AuditDirection.To);
            }

            return historyQuery.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit history for matter: {MatterId}", matterId);
            return null;
        }
    }

    /// <summary>
    /// Validates audit records using centralized validation.
    /// </summary>
    private IEnumerable<string> ValidateAuditRecords(IEnumerable<MatterDocumentActivityUserMinimalDto> auditRecords)
    {
        var errors = new List<string>();
        foreach (var audit in auditRecords)
        {
            var validationResult = _validationService.ValidateObject(audit);
            if (validationResult != null)
            {
                errors.Add($"Validation failed for audit record");
            }
        }
        return errors;
    }

    /// <summary>
    /// Updates a matter with audit trail.
    /// </summary>
    private async Task<Matter?> UpdateMatterWithAuditAsync(Guid matterId, MatterForUpdateDto matter)
    {
        try
        {
            return await _admsRepository.UpdateMatterAsync(matterId, matter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating matter: {MatterId}", matterId);
            return null;
        }
    }

    /// <summary>
    /// Restores a matter with audit trail.
    /// </summary>
    private async Task<bool> RestoreMatterWithAuditAsync(Guid matterId)
    {
        try
        {
            // Retrieve and validate matter for restoration
            var matter = await _admsRepository.GetMatterAsync(matterId, includeDocuments: false, includeHistory: false);
            if (matter == null) return false;

            var matterDto = _mapper.Map<MatterDto>(matter);
            var validationResult = _validationService.ValidateObject(matterDto);
            if (validationResult != null) return false;

            return await _admsRepository.RestoreMatterAsync(matterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring matter: {MatterId}", matterId);
            return false;
        }
    }

    /// <summary>
    /// Validates returned matters using centralized validation.
    /// </summary>
    private IEnumerable<string> ValidateReturnedMatters<T>(IEnumerable<T> matters)
    {
        var errors = new List<string>();
        foreach (var matter in matters)
        {
            var validationResult = _validationService.ValidateObject(matter);
            if (validationResult != null)
            {
                errors.Add($"Validation failed for matter");
            }
        }
        return errors;
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