using ADMS.API.Common;
using ADMS.API.Extensions;
using ADMS.API.Helpers;
using ADMS.API.Models;
using ADMS.API.ResourceParameters;
using ADMS.API.Services;

using Asp.Versioning;

using MapsterMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using System.ComponentModel.DataAnnotations;

using Document = ADMS.API.Entities.Document;

namespace ADMS.API.Controllers;

/// <summary>
/// Controller for managing documents within a matter in the ADMS system.
/// </summary>
/// <remarks>
/// This controller provides comprehensive document management functionality including:
/// - Document creation with file upload, virus scanning, and validation
/// - Document retrieval with pagination, filtering, and data shaping
/// - Document updates with metadata and file content changes
/// - Document operations (copy, move, delete, check in/out)
/// - Document history and audit trail access
/// 
/// All endpoints implement consistent error handling, centralized validation,
/// structured logging, and security best practices including virus scanning
/// and file type validation.
/// 
/// The controller follows RESTful principles and supports content negotiation
/// for JSON and XML responses.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/matters/{matterId}/documents")]
[Produces("application/json", "application/xml")]
public class DocumentController(
    ILogger<DocumentController> logger,
    IRepository admsRepository,
    IMapper mapper,
    IPropertyMappingService propertyMappingService,
    IPropertyCheckerService propertyCheckerService,
    ProblemDetailsFactory problemDetailsFactory,
    IValidationService validationService,
    IVirusScanner virusScanner,
    IFileStorage fileStorage) : ControllerBase
{
    private readonly IRepository _admsRepository = admsRepository ?? throw new ArgumentNullException(nameof(admsRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ILogger<DocumentController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IPropertyMappingService _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
    private readonly IPropertyCheckerService _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
    private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
    private readonly IValidationService _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    private readonly IVirusScanner _virusScanner = virusScanner ?? throw new ArgumentNullException(nameof(virusScanner));
    private readonly IFileStorage _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));

    #region Constants

    /// <summary>
    /// Maximum allowed file size for uploads (50 MB).
    /// </summary>
    private const long MaxUploadFileSize = 50 * 1024 * 1024;

    #endregion Constants

    #region Actions

    /// <summary>
    /// Creates a new document within the specified matter.
    /// </summary>
    /// <remarks>
    /// This endpoint accepts both document metadata and a file upload to create
    /// a new document. The process includes:
    /// 
    /// 1. Input validation and file size checks
    /// 2. Virus scanning using ClamAV
    /// 3. File type validation and MIME type detection
    /// 4. Business rule validation (duplicate filename, etc.)
    /// 5. Document persistence and file storage
    /// 6. Audit trail creation
    /// 
    /// The endpoint supports multipart/form-data for file uploads and enforces
    /// security policies including file type restrictions and virus scanning.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter to contain the document.</param>
    /// <param name="document">The document metadata including filename and extension.</param>
    /// <param name="fileUpload">The file content to be uploaded and associated with the document.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// <para>201 Created - Document successfully created with location header.</para>
    /// <para>400 BadRequest - Invalid input parameters or validation errors.</para>
    /// <para>404 NotFound - The specified matter does not exist.</para>
    /// <para>409 Conflict - A document with the same filename already exists.</para>
    /// <para>413 PayloadTooLarge - File size exceeds the maximum allowed limit.</para>
    /// <para>415 UnsupportedMediaType - File type is not allowed.</para>
    /// <para>422 UnprocessableEntity - File contains virus or malware.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpPost(Name = "CreateDocument")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DocumentDto>> CreateDocumentAsync(
        Guid matterId,
        [FromForm] DocumentForCreationDto? document,
        IFormFile? fileUpload,
        CancellationToken cancelToken = default)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateCreateDocumentParametersAsync(matterId, document, fileUpload);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Process and validate file upload
            var (fileBytes, fileExtension, detectedMimeType, fileValidationError) =
                await ProcessFileUploadAsync(fileUpload!, cancelToken);
            if (fileValidationError != null)
            {
                return fileValidationError;
            }

            // Set calculated fields on the DTO
            document!.FileSize = fileBytes!.Length;
            document.MimeType = detectedMimeType!;
            document.Checksum = ComputeChecksum(fileBytes);
            document.Extension = fileExtension!.TrimStart('.');

            // Perform comprehensive document validation
            var documentValidationResult = await ValidateDocumentForCreationAsync(matterId, document);
            if (documentValidationResult != null)
            {
                return documentValidationResult;
            }

            // Create document and save file
            var resultantDoc = await CreateAndSaveDocumentAsync(matterId, document, fileBytes, fileExtension, cancelToken);

            var documentToReturn = _mapper.Map<DocumentDto>(resultantDoc);

            // Add API version to response headers
            Response.AddApiVersion("1.0");

            _logger.LogInformation(
                "Successfully created document {DocumentId} '{FileName}' for matter {MatterId}.",
                resultantDoc.Id, document.FileName, matterId);

            return CreatedAtRoute("GetDocument", new { matterId, documentId = documentToReturn.Id }, documentToReturn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error creating document for matter {MatterId}.", matterId);

            return CreateInternalServerErrorResponse("creating the document");
        }
    }

    /// <summary>
    /// Retrieves a paginated list of documents for the specified matter.
    /// </summary>
    /// <remarks>
    /// This endpoint supports advanced querying capabilities including:
    /// - Pagination with configurable page size
    /// - Sorting by multiple fields with ascending/descending order
    /// - Filtering by document filename
    /// - Data shaping to return only requested fields
    /// - Response caching for improved performance
    /// 
    /// Pagination metadata is returned in the X-Pagination response header.
    /// The endpoint supports both GET and HEAD methods for metadata retrieval.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the documents.</param>
    /// <param name="documentsResourceParameters">Parameters for pagination, filtering, sorting, and data shaping.</param>
    /// <returns>
    /// <para>200 OK - Paginated list of documents with metadata in headers.</para>
    /// <para>400 BadRequest - Invalid parameters or validation errors.</para>
    /// <para>404 NotFound - The specified matter does not exist.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpGet(Name = "GetDocuments")]
    [HttpHead(Name = "GetDocumentsHead")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ResponseCache(CacheProfileName = "240SecondsCacheProfile")]
    public async Task<IActionResult> GetDocumentsAsync(
        Guid matterId,
        [FromQuery] DocumentsResourceParameters documentsResourceParameters)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateGetDocumentsParametersAsync(matterId, documentsResourceParameters);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Retrieve paginated documents
            var pagedDocuments = await _admsRepository.GetPaginatedDocumentsAsync(matterId, documentsResourceParameters);

            // Add pagination metadata to response headers
            Response.AddPaginationMetadata(pagedDocuments);
            Response.AddApiVersion("1.0");

            var documentDtos = _mapper.Map<IEnumerable<DocumentDto>>(pagedDocuments);

            _logger.LogInformation(
                "Successfully retrieved {Count} documents for matter {MatterId}. Page {CurrentPage} of {TotalPages}.",
                pagedDocuments.Count, matterId, pagedDocuments.CurrentPage, pagedDocuments.TotalPages);

            return Ok(documentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving documents for matter {MatterId}.", matterId);

            return CreateInternalServerErrorResponse("retrieving documents");
        }
    }

    /// <summary>
    /// Retrieves a specific document by its identifier.
    /// </summary>
    /// <remarks>
    /// This endpoint retrieves a single document with optional data shaping
    /// to return only requested fields. The response includes document metadata,
    /// file information, and audit trail summary.
    /// 
    /// Data shaping allows clients to specify which properties to include
    /// in the response using a comma-separated list of field names.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document to retrieve.</param>
    /// <param name="fields">Optional comma-separated list of fields to include in the response.</param>
    /// <returns>
    /// <para>200 OK - The requested document with specified fields.</para>
    /// <para>400 BadRequest - Invalid parameters or field specifications.</para>
    /// <para>404 NotFound - The specified matter or document does not exist.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpGet("{documentId}", Name = "GetDocument")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ResponseCache(CacheProfileName = "240SecondsCacheProfile")]
    public async Task<IActionResult> GetDocumentAsync(Guid matterId, Guid documentId, string? fields = null)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateDocumentAccessParametersAsync(matterId, documentId, fields);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Retrieve document
            var documentFromRepo = await _admsRepository.GetDocumentAsync(documentId, includeRevisions: false, includeHistory: false);
            if (documentFromRepo == null)
            {
                _logger.LogWarning("Document {DocumentId} not found in matter {MatterId}.", documentId, matterId);
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Document not found"));
            }

            var documentDto = _mapper.Map<DocumentDto>(documentFromRepo);

            // Validate the returned DTO
            var dtoValidationResult = _validationService.ValidateObject(documentDto);
            if (dtoValidationResult != null)
            {
                return dtoValidationResult;
            }

            Response.AddApiVersion("1.0");

            _logger.LogInformation("Successfully retrieved document {DocumentId} from matter {MatterId}.", documentId, matterId);

            return Ok(documentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving document {DocumentId} from matter {MatterId}.", documentId, matterId);

            return CreateInternalServerErrorResponse("retrieving the document");
        }
    }

    /// <summary>
    /// Updates an existing document's metadata and optionally replaces its file content.
    /// </summary>
    /// <remarks>
    /// This endpoint supports partial updates of document metadata and complete
    /// file replacement. When a new file is provided, it undergoes the same
    /// validation process as document creation including virus scanning and
    /// file type validation.
    /// 
    /// The update operation is atomic - either all changes succeed or none are applied.
    /// An audit trail entry is automatically created for the update operation.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document to update.</param>
    /// <param name="document">The updated document metadata.</param>
    /// <param name="fileUpload">Optional new file content to replace the existing file.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// <para>204 NoContent - Document successfully updated.</para>
    /// <para>400 BadRequest - Invalid input parameters or validation errors.</para>
    /// <para>404 NotFound - The specified matter or document does not exist.</para>
    /// <para>413 PayloadTooLarge - File size exceeds the maximum allowed limit.</para>
    /// <para>415 UnsupportedMediaType - File type is not allowed.</para>
    /// <para>422 UnprocessableEntity - File contains virus or malware.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpPut("{documentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateDocumentAsync(
        Guid matterId,
        Guid documentId,
        [FromForm] DocumentForUpdateDto document,
        IFormFile? fileUpload = null,
        CancellationToken cancelToken = default)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateUpdateDocumentParametersAsync(matterId, documentId, document);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Retrieve existing document
            var existingDocument = await _admsRepository.GetDocumentAsync(documentId, includeRevisions: false, includeHistory: false);
            if (existingDocument == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for update in matter {MatterId}.", documentId, matterId);
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Document not found"));
            }

            // Process file upload if provided
            if (fileUpload != null && fileUpload.Length > 0)
            {
                var (fileBytes, fileExtension, detectedMimeType, fileError) =
                    await ProcessFileUploadAsync(fileUpload, cancelToken);
                if (fileError != null)
                {
                    return fileError;
                }

                document.FileSize = fileBytes!.Length;
                document.MimeType = detectedMimeType!;
                document.Checksum = ComputeChecksum(fileBytes);
                document.Extension = fileExtension!.TrimStart('.');

                // Save updated file
                await SaveDocumentFileAsync(matterId, documentId, fileBytes, fileExtension, cancelToken);
            }

            // Validate document for update
            var updateValidationResults = _validationService.ValidateDocumentForUpdate(document);
            var validationResultsList = updateValidationResults.ToList();
            if (validationResultsList.Count > 0)
            {
                return CreateValidationErrorResponse(validationResultsList, "Document update validation failed.");
            }

            // Perform update
            var updatedDocument = await _admsRepository.UpdateDocumentAsync(documentId, document);
            if (updatedDocument == null)
            {
                _logger.LogError("Failed to update document {DocumentId} in matter {MatterId}.", documentId, matterId);
                return CreateInternalServerErrorResponse("updating the document");
            }

            await _admsRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully updated document {DocumentId} '{FileName}' in matter {MatterId}.",
                documentId, document.FileName, matterId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating document {DocumentId} in matter {MatterId}.", documentId, matterId);

            return CreateInternalServerErrorResponse("updating the document");
        }
    }

    /// <summary>
    /// Soft deletes a document by marking it as deleted without removing it from storage.
    /// </summary>
    /// <remarks>
    /// This endpoint performs a soft delete operation, marking the document as deleted
    /// while preserving it for audit purposes. The document can be restored later
    /// if needed. An audit trail entry is created to track the deletion.
    /// 
    /// The physical file remains on storage but becomes inaccessible through normal
    /// document retrieval operations.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document to delete.</param>
    /// <returns>
    /// <para>204 NoContent - Document successfully deleted.</para>
    /// <para>400 BadRequest - Invalid input parameters.</para>
    /// <para>404 NotFound - The specified matter or document does not exist.</para>
    /// <para>409 Conflict - Document cannot be deleted in its current state.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpDelete("{documentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteDocumentAsync(Guid matterId, Guid documentId)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateDocumentAccessParametersAsync(matterId, documentId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Retrieve document for deletion
            var document = await _admsRepository.GetDocumentAsync(documentId, includeRevisions: false, includeHistory: false);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for deletion in matter {MatterId}.", documentId, matterId);
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Document not found"));
            }

            // Create DTO and mark as deleted
            var documentDto = _mapper.Map<DocumentDto>(document);
            documentDto.IsDeleted = true;

            // Validate before deletion
            var dtoValidationResult = _validationService.ValidateObject(documentDto);
            if (dtoValidationResult != null)
            {
                return dtoValidationResult;
            }

            // Perform soft delete
            var deleted = await _admsRepository.DeleteDocumentAsync(documentDto);
            if (!deleted)
            {
                _logger.LogError("Failed to delete document {DocumentId} in matter {MatterId}.", documentId, matterId);
                return CreateInternalServerErrorResponse("deleting the document");
            }

            await _admsRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully deleted document {DocumentId} from matter {MatterId}.",
                documentId, matterId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting document {DocumentId} from matter {MatterId}.", documentId, matterId);

            return CreateInternalServerErrorResponse("deleting the document");
        }
    }

    /// <summary>
    /// Copies a document to the specified target matter.
    /// </summary>
    /// <remarks>
    /// Creates a copy of the specified document in the target matter while
    /// preserving the original. The copied document receives a new identifier
    /// and creation timestamp, but maintains the same content and metadata.
    /// 
    /// An audit trail entry is created for both the source and target documents
    /// to track the copy operation.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the target matter for the document copy.</param>
    /// <param name="document">The document to be copied (without revision history).</param>
    /// <returns>
    /// <para>200 OK - Document successfully copied with the new document details.</para>
    /// <para>400 BadRequest - Invalid input parameters or validation errors.</para>
    /// <para>404 NotFound - The specified source document or target matter does not exist.</para>
    /// <para>409 Conflict - A document with the same filename already exists in the target matter.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpPost("copy")]
    [ProducesResponseType(typeof(DocumentWithoutRevisionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DocumentWithoutRevisionsDto>> CopyDocumentAsync(
        Guid matterId,
        [FromBody] DocumentWithoutRevisionsDto? document)
    {
        try
        {
            _logger.LogInformation("Attempting to copy document to matter {MatterId}.", matterId);

            return await PerformDocumentOperationAsync(matterId, document, DocumentOperationType.Copy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying document to matter {MatterId}.", matterId);
            return CreateInternalServerErrorResponse("copying the document");
        }
    }

    /// <summary>
    /// Moves a document to the specified target matter.
    /// </summary>
    /// <remarks>
    /// Transfers the specified document from its current matter to the target matter.
    /// The document is removed from the source matter and added to the target matter,
    /// preserving all metadata and content.
    /// 
    /// An audit trail entry is created for both the source and target matters
    /// to track the move operation.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the target matter for the document move.</param>
    /// <param name="document">The document to be moved (without revision history).</param>
    /// <returns>
    /// <para>200 OK - Document successfully moved with updated matter association.</para>
    /// <para>400 BadRequest - Invalid input parameters or validation errors.</para>
    /// <para>404 NotFound - The specified document or target matter does not exist.</para>
    /// <para>409 Conflict - A document with the same filename already exists in the target matter.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpPost("move")]
    [ProducesResponseType(typeof(DocumentWithoutRevisionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DocumentWithoutRevisionsDto>> MoveDocumentAsync(
        Guid matterId,
        [FromBody] DocumentWithoutRevisionsDto? document)
    {
        try
        {
            _logger.LogInformation("Attempting to move document to matter {MatterId}.", matterId);

            return await PerformDocumentOperationAsync(matterId, document, DocumentOperationType.Move);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving document to matter {MatterId}.", matterId);
            return CreateInternalServerErrorResponse("moving the document");
        }
    }

    /// <summary>
    /// Checks out a document for exclusive editing by the current user.
    /// </summary>
    /// <remarks>
    /// Marks the document as checked out, preventing other users from
    /// modifying it until it is checked back in. This implements an
    /// optimistic locking mechanism for document editing.
    /// 
    /// An audit trail entry is created to track the checkout operation
    /// and user information.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document to check out.</param>
    /// <returns>
    /// <para>200 OK - Document successfully checked out.</para>
    /// <para>400 BadRequest - Invalid parameters or document cannot be checked out.</para>
    /// <para>404 NotFound - The specified matter or document does not exist.</para>
    /// <para>409 Conflict - Document is already checked out by another user.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpPost("{documentId}/checkout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckoutDocumentAsync(Guid matterId, Guid documentId)
    {
        try
        {
            return await PerformCheckStateOperationAsync(matterId, documentId, true, "checkout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out document {DocumentId} from matter {MatterId}.", documentId, matterId);
            return CreateInternalServerErrorResponse("checking out the document");
        }
    }

    /// <summary>
    /// Checks in a previously checked out document, making it available for other users.
    /// </summary>
    /// <remarks>
    /// Removes the checked out status from the document, allowing other users
    /// to check it out for editing. This completes the optimistic locking cycle
    /// for document editing.
    /// 
    /// An audit trail entry is created to track the check-in operation
    /// and user information.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document to check in.</param>
    /// <returns>
    /// <para>200 OK - Document successfully checked in.</para>
    /// <para>400 BadRequest - Invalid parameters or document cannot be checked in.</para>
    /// <para>404 NotFound - The specified matter or document does not exist.</para>
    /// <para>409 Conflict - Document is not currently checked out.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpPost("{documentId}/checkin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckinDocumentAsync(Guid matterId, Guid documentId)
    {
        try
        {
            return await PerformCheckStateOperationAsync(matterId, documentId, false, "checkin");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in document {DocumentId} from matter {MatterId}.", documentId, matterId);
            return CreateInternalServerErrorResponse("checking in the document");
        }
    }

    /// <summary>
    /// Retrieves the complete activity history for a document.
    /// </summary>
    /// <remarks>
    /// Returns a chronological list of all activities performed on the document
    /// including creation, modifications, check in/out operations, and user
    /// information. The history provides a complete audit trail for compliance
    /// and tracking purposes.
    /// 
    /// Activities are returned in reverse chronological order (most recent first).
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document whose history is requested.</param>
    /// <returns>
    /// <para>200 OK - Complete chronological activity history for the document.</para>
    /// <para>400 BadRequest - Invalid input parameters.</para>
    /// <para>404 NotFound - The specified matter or document does not exist.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpGet("{documentId}/history")]
    [ProducesResponseType(typeof(IEnumerable<DocumentActivityUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDocumentHistoryAsync(Guid matterId, Guid documentId)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateDocumentAccessParametersAsync(matterId, documentId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Retrieve document history
            var documentHistory = await _admsRepository.GetDocumentAsync(documentId, includeRevisions: false, includeHistory: true);
            if (documentHistory == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for history retrieval in matter {MatterId}.", documentId, matterId);
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Document not found"));
            }

            var historyDtos = _mapper.Map<IEnumerable<DocumentActivityUserDto>>(documentHistory.DocumentActivityUsers);
            var historyList = historyDtos.ToList();

            // Validate history DTOs
            var validationErrors = await ValidateHistoryDtosAsync(historyList);
            if (validationErrors != null)
            {
                return validationErrors;
            }

            _logger.LogInformation(
                "Successfully retrieved {Count} history records for document {DocumentId} in matter {MatterId}.",
                historyList.Count, documentId, matterId);

            return Ok(historyList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving history for document {DocumentId} in matter {MatterId}.", documentId, matterId);

            return CreateInternalServerErrorResponse("retrieving document history");
        }
    }

    /// <summary>
    /// Retrieves audit records for document operations in the specified direction.
    /// </summary>
    /// <remarks>
    /// Returns audit records showing document move or copy operations either
    /// FROM the current matter (outgoing) or TO the current matter (incoming).
    /// This provides visibility into document transfer activities across matters.
    /// 
    /// The direction parameter determines whether to show:
    /// - From: Documents that were moved/copied FROM this matter to others
    /// - To: Documents that were moved/copied TO this matter from others
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter.</param>
    /// <param name="documentId">The unique identifier of the document.</param>
    /// <param name="direction">The direction of audit records to retrieve (From or To).</param>
    /// <returns>
    /// <para>200 OK - List of audit records for the specified direction.</para>
    /// <para>400 BadRequest - Invalid input parameters or direction value.</para>
    /// <para>404 NotFound - The specified matter or document does not exist.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpGet("{documentId}/audit")]
    [ProducesResponseType(typeof(IEnumerable<MatterDocumentActivityUserMinimalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MatterDocumentActivityUserMinimalDto>>> GetAuditAsync(
        Guid matterId,
        Guid documentId,
        [FromQuery] AuditEnums.AuditDirection direction)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateAuditParametersAsync(matterId, documentId, direction);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Retrieve audit records
            var auditsQueryable = await _admsRepository.GetExtendedAuditsAsync(matterId, documentId, direction);
            var auditList = auditsQueryable.ToList();

            // Validate audit DTOs
            var auditValidationErrors = await ValidateAuditDtosAsync(auditList);
            if (auditValidationErrors != null)
            {
                return auditValidationErrors;
            }

            _logger.LogInformation(
                "Successfully retrieved {Count} audit records ({Direction}) for document {DocumentId} in matter {MatterId}.",
                auditList.Count, direction, documentId, matterId);

            return Ok(auditList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving audit records ({Direction}) for document {DocumentId} in matter {MatterId}.",
                direction, documentId, matterId);

            return CreateInternalServerErrorResponse($"retrieving {direction.ToString().ToLower()} audit records");
        }
    }

    /// <summary>
    /// Returns the HTTP methods supported by the document resource.
    /// </summary>
    /// <remarks>
    /// This endpoint implements the HTTP OPTIONS method to allow clients
    /// to discover which operations are available on the document resource.
    /// This is useful for API documentation tools and client applications
    /// that need to determine available functionality dynamically.
    /// </remarks>
    /// <returns>
    /// <para>204 NoContent - Response with Allow header listing supported HTTP methods.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpOptions]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult GetDocumentOptions()
    {
        try
        {
            Response.Headers.Allow = "GET,HEAD,POST,PUT,DELETE,OPTIONS";
            Response.AddApiVersion("1.0");

            _logger.LogDebug("OPTIONS request processed for document resource.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OPTIONS request for document resource.");
            return CreateInternalServerErrorResponse("processing the OPTIONS request");
        }
    }

    #endregion Actions

    #region Private Helper Methods

    /// <summary>
    /// Enum representing document operation types for copy and move operations.
    /// </summary>
    private enum DocumentOperationType
    {
        /// <summary>Document copy operation.</summary>
        Copy,
        /// <summary>Document move operation.</summary>
        Move
    }

    /// <summary>
    /// Validates parameters for document creation operation.
    /// </summary>
    private async Task<ActionResult?> ValidateCreateDocumentParametersAsync(
        Guid matterId, DocumentForCreationDto? document, IFormFile? fileUpload)
    {
        if (matterId == Guid.Empty)
        {
            _logger.LogWarning("Invalid matterId provided for document creation.");
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Invalid matterId."));
        }

        if (document == null)
        {
            _logger.LogWarning("Null document provided for creation.");
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Document metadata cannot be null."));
        }

        if (fileUpload == null || fileUpload.Length == 0)
        {
            _logger.LogWarning("File upload is missing or empty for document creation.");
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "File upload is required."));
        }

        // Validate matter existence
        return await _validationService.ValidateMatterExistsAsync(matterId);
    }

    /// <summary>
    /// Validates parameters for document retrieval operations.
    /// </summary>
    private async Task<ActionResult?> ValidateGetDocumentsParametersAsync(
        Guid matterId, DocumentsResourceParameters parameters)
    {
        if (matterId == Guid.Empty)
        {
            _logger.LogWarning("Invalid matterId provided for documents retrieval.");
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Invalid matterId."));
        }

        // Validate resource parameters
        var paramValidationResult = _validationService.ValidateObject(parameters);
        if (paramValidationResult != null)
        {
            return paramValidationResult;
        }

        // Validate matter existence
        var matterResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (matterResult != null) return matterResult;

        // Validate property mapping for sorting
        if (!string.IsNullOrWhiteSpace(parameters.OrderBy) &&
            !_propertyMappingService.ValidMappingExistsFor<DocumentDto, Document>(parameters.OrderBy))
        {
            _logger.LogWarning("Invalid OrderBy parameter: {OrderBy}", parameters.OrderBy);
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Invalid OrderBy parameter."));
        }

        // Validate fields for data shaping
        if (!string.IsNullOrWhiteSpace(parameters.Fields) &&
            !_propertyCheckerService.TypeHasProperties<DocumentDto>(parameters.Fields))
        {
            _logger.LogWarning("Invalid Fields parameter: {Fields}", parameters.Fields);
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest,
                $"Not all requested fields exist on the resource: {parameters.Fields}"));
        }

        return null;
    }

    /// <summary>
    /// Validates parameters for document access operations (get, update, delete).
    /// </summary>
    private async Task<ActionResult?> ValidateDocumentAccessParametersAsync(
        Guid matterId, Guid documentId, string? fields = null)
    {
        if (matterId == Guid.Empty)
        {
            _logger.LogWarning("Invalid matterId provided for document access.");
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Invalid matterId."));
        }

        if (documentId == Guid.Empty)
        {
            _logger.LogWarning("Invalid documentId provided for document access.");
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Invalid documentId."));
        }

        // Validate fields parameter if provided
        if (!string.IsNullOrWhiteSpace(fields) &&
            !_propertyCheckerService.TypeHasProperties<DocumentDto>(fields))
        {
            _logger.LogWarning("Invalid fields parameter: {Fields}", fields);
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest,
                $"Not all requested fields exist on the resource: {fields}"));
        }

        // Validate matter existence
        var matterResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (matterResult != null) return matterResult;

        // Validate document existence
        return await _validationService.ValidateDocumentExistsAsync(documentId);
    }

    /// <summary>
    /// Validates parameters for document update operations.
    /// </summary>
    private async Task<ActionResult?> ValidateUpdateDocumentParametersAsync(
        Guid matterId, Guid documentId, DocumentForUpdateDto document)
    {
        var basicValidation = await ValidateDocumentAccessParametersAsync(matterId, documentId);
        if (basicValidation != null) return basicValidation;

        // Validate update DTO
        var dtoValidationResult = _validationService.ValidateObject(document);
        if (dtoValidationResult != null)
        {
            _logger.LogWarning("Validation failed for DocumentForUpdateDto.");
            return dtoValidationResult;
        }

        return null;
    }

    /// <summary>
    /// Validates parameters for audit retrieval operations.
    /// </summary>
    private async Task<ActionResult?> ValidateAuditParametersAsync(
        Guid matterId, Guid documentId, AuditEnums.AuditDirection direction)
    {
        var basicValidation = await ValidateDocumentAccessParametersAsync(matterId, documentId);
        if (basicValidation != null) return basicValidation;

        // Validate direction enum
        if (!Enum.IsDefined(direction))
        {
            _logger.LogWarning("Invalid audit direction parameter: {Direction}", direction);
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest,
                "Invalid direction parameter. Valid values are 'From' or 'To'."));
        }

        return null;
    }

    /// <summary>
    /// Processes and validates file upload including virus scanning and file type validation.
    /// </summary>
    private async Task<(byte[]? fileBytes, string? fileExtension, string? detectedMimeType, ActionResult? error)>
        ProcessFileUploadAsync(IFormFile fileUpload, CancellationToken cancelToken)
    {
        // File size validation
        if (fileUpload.Length > MaxUploadFileSize)
        {
            _logger.LogWarning("File size {FileSize} exceeds maximum allowed {MaxSize}.",
                fileUpload.Length, MaxUploadFileSize);
            var problem = CreateProblemDetails(StatusCodes.Status413PayloadTooLarge,
                $"File size exceeds the maximum allowed ({MaxUploadFileSize / (1024 * 1024)} MB).");
            return (null, null, null, StatusCode(StatusCodes.Status413PayloadTooLarge, problem));
        }

        // Read file content
        byte[] fileBytes;
        try
        {
            using var ms = new MemoryStream();
            await fileUpload.CopyToAsync(ms, cancelToken);
            fileBytes = ms.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading uploaded file.");
            return (null, null, null, CreateInternalServerErrorResponse("reading the uploaded file"));
        }

        // Virus scanning
        try
        {
            using var scanStream = new MemoryStream(fileBytes);
            var scanResult = await _virusScanner.ScanFileForVirusesAsync(scanStream);

            if (!scanResult.IsClean)
            {
                _logger.LogWarning("Virus detected in uploaded file.");
                var problem = CreateProblemDetails(StatusCodes.Status422UnprocessableEntity,
                    "The uploaded file contains a virus or malware and was rejected.");
                return (null, null, null, UnprocessableEntity(problem));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during virus scan.");
            return (null, null, null, CreateInternalServerErrorResponse("performing virus scan"));
        }

        // File type validation
        var fileInfo = new FileInfo(fileUpload.FileName);
        var fileExtension = fileInfo.Extension.ToLowerInvariant();

        if (!FileValidationHelper.IsExtensionAllowed(fileExtension))
        {
            _logger.LogWarning("File extension '{FileExtension}' is not allowed.", fileExtension);
            var problem = CreateProblemDetails(StatusCodes.Status415UnsupportedMediaType,
                $"File extension '{fileExtension}' is not allowed.");
            return (null, null, null, StatusCode(StatusCodes.Status415UnsupportedMediaType, problem));
        }

        if (!FileValidationHelper.IsValidFileType(fileBytes, out var detectedMimeType, out var detectedExtension, _logger))
        {
            _logger.LogWarning("File type validation failed. Detected: {Extension}, {MimeType}",
                detectedExtension, detectedMimeType);
            var problem = CreateProblemDetails(StatusCodes.Status415UnsupportedMediaType,
                $"File type not allowed. Detected: {detectedExtension}, {detectedMimeType}");
            return (null, null, null, StatusCode(StatusCodes.Status415UnsupportedMediaType, problem));
        }

        if (!string.Equals(fileExtension, detectedExtension, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("File extension mismatch. Expected: {Expected}, Detected: {Detected}",
                fileExtension, detectedExtension);
            var problem = CreateProblemDetails(StatusCodes.Status415UnsupportedMediaType,
                $"File extension '{fileExtension}' does not match detected type '{detectedExtension}'.");
            return (null, null, null, StatusCode(StatusCodes.Status415UnsupportedMediaType, problem));
        }

        return (fileBytes, fileExtension, detectedMimeType, null);
    }

    /// <summary>
    /// Validates document for creation including business rules and DTO validation.
    /// </summary>
    private async Task<ActionResult?> ValidateDocumentForCreationAsync(Guid matterId, DocumentForCreationDto document)
    {
        // DTO validation
        var dtoValidationResult = _validationService.ValidateObject(document);
        if (dtoValidationResult != null)
        {
            _logger.LogWarning("DTO validation failed for document creation.");
            return dtoValidationResult;
        }

        // Business rule validation
        var businessValidationResults = await _validationService.ValidateDocumentForCreationAsync(matterId, document);
        var validationResults = businessValidationResults.ToList();
        if (validationResults.Count > 0)
        {
            return CreateValidationErrorResponse(validationResults, "Document creation validation failed.");
        }

        return null;
    }

    /// <summary>
    /// Creates and saves a document with its associated file.
    /// </summary>
    private async Task<Document> CreateAndSaveDocumentAsync(
        Guid matterId, DocumentForCreationDto document, byte[] fileBytes, string fileExtension, CancellationToken cancelToken)
    {
        // Create document entity
        var resultantDoc = await _admsRepository.AddDocumentAsync(matterId, document);
        if (resultantDoc == null)
        {
            _logger.LogError("Failed to create document entity for matter {MatterId}.", matterId);
            throw new InvalidOperationException("Failed to create document entity.");
        }

        await _admsRepository.SaveChangesAsync();

        // Save file to storage
        await SaveDocumentFileAsync(matterId, resultantDoc.Id, fileBytes, fileExtension, cancelToken);

        return resultantDoc;
    }

    /// <summary>
    /// Saves document file content to storage.
    /// </summary>
    private async Task SaveDocumentFileAsync(
        Guid matterId, Guid documentId, byte[] fileBytes, string fileExtension, CancellationToken cancelToken)
    {
        var folderPath = Path.Combine("ServerFiles", "matters", matterId.ToString());
        Directory.CreateDirectory(folderPath);
        var fileName = $"{documentId}{fileExtension}";
        var filePath = Path.Combine(folderPath, fileName);

        try
        {
            await _fileStorage.SaveFileAsync(filePath, fileBytes, cancelToken);
            _logger.LogDebug("Successfully saved file for document {DocumentId} to {FilePath}.", documentId, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file for document {DocumentId}.", documentId);
            throw new InvalidOperationException($"Failed to save file for document {documentId}.", ex);
        }
    }

    /// <summary>
    /// Performs document copy or move operations with comprehensive validation and error handling.
    /// </summary>
    private async Task<ActionResult<DocumentWithoutRevisionsDto>> PerformDocumentOperationAsync(
        Guid matterId, DocumentWithoutRevisionsDto? document, DocumentOperationType operationType)
    {
        if (matterId == Guid.Empty)
        {
            _logger.LogWarning("Invalid matterId provided for {Operation}.", operationType);
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Invalid matterId."));
        }

        if (document == null)
        {
            _logger.LogWarning("Null document provided for {Operation}.", operationType);
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Document cannot be null."));
        }

        // Validate DTO
        var dtoValidationResult = _validationService.ValidateObject(document);
        if (dtoValidationResult != null)
        {
            _logger.LogWarning("Validation failed for document {Operation}.", operationType);
            return dtoValidationResult;
        }

        // Validate matter existence
        var matterExistsResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (matterExistsResult != null) return matterExistsResult;

        // For move operations, validate source document existence
        if (operationType == DocumentOperationType.Move)
        {
            var documentExistsResult = await _validationService.ValidateDocumentExistsAsync(document.Id);
            if (documentExistsResult != null) return documentExistsResult;
        }

        // Retrieve target matter
        var matter = await _admsRepository.GetMatterAsync(matterId, includeDocuments: false);
        if (matter == null)
        {
            _logger.LogWarning("Target matter {MatterId} not found for {Operation}.", matterId, operationType);
            return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Target matter not found."));
        }

        // Perform the operation
        var operationString = operationType == DocumentOperationType.Copy ? "COPIED" : "MOVED";
        var result = await _admsRepository.PerformDocumentOperationAsync(matterId, matter.Id, document, operationString);

        if (!result)
        {
            _logger.LogError("Failed to {Operation} document to matter {MatterId}.", operationString.ToLower(), matterId);
            return CreateInternalServerErrorResponse($"{operationType.ToString().ToLower()}ing the document");
        }

        _logger.LogInformation("Successfully {Operation} document to matter {MatterId}.", operationString.ToLower(), matterId);
        return Ok(document);
    }

    /// <summary>
    /// Performs document check-in or check-out operations.
    /// </summary>
    private async Task<IActionResult> PerformCheckStateOperationAsync(
        Guid matterId, Guid documentId, bool isCheckOut, string operationName)
    {
        // Validate parameters
        var validationResult = await ValidateDocumentAccessParametersAsync(matterId, documentId);
        if (validationResult != null) return validationResult;

        // Retrieve and validate document
        var document = await _admsRepository.GetDocumentAsync(documentId, includeRevisions: false, includeHistory: false);
        if (document == null)
        {
            _logger.LogWarning("Document {DocumentId} not found for {Operation} in matter {MatterId}.",
                documentId, operationName, matterId);
            return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Document not found."));
        }

        // Validate DTO before operation
        var documentDto = _mapper.Map<DocumentDto>(document);
        var dtoValidationResult = _validationService.ValidateObject(documentDto);
        if (dtoValidationResult != null)
        {
            return dtoValidationResult;
        }

        // Perform check operation
        var success = await _admsRepository.SetDocumentCheckStateAsync(documentId, isCheckOut);
        if (!success)
        {
            _logger.LogWarning("Cannot {Operation} document {DocumentId} in matter {MatterId}.",
                operationName, documentId, matterId);
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, $"Cannot {operationName} document."));
        }

        _logger.LogInformation("Successfully {Operation} document {DocumentId} in matter {MatterId}.",
            operationName, documentId, matterId);
        return Ok();
    }

    /// <summary>
    /// Validates a collection of document history DTOs.
    /// </summary>
    private async Task<ActionResult?> ValidateHistoryDtosAsync(IList<DocumentActivityUserDto> historyDtos)
    {
        var validationErrors = new List<ValidationResult>();

        await Task.Run(() =>
        {
            foreach (var result in historyDtos.Select(dto => _validationService.ValidateObject(dto)))
            {
                if (result is ObjectResult { Value: ValidationProblemDetails vpd })
                {
                    validationErrors.AddRange(
                        vpd.Errors.SelectMany(e => e.Value.Select(msg => new ValidationResult(msg, [e.Key])))
                    );
                }
            }
        });

        return validationErrors.Count > 0
            ? CreateValidationErrorResponse(validationErrors, "Validation failed for document history records.")
            : null;
    }

    /// <summary>
    /// Validates a collection of audit DTOs.
    /// </summary>
    private async Task<ActionResult?> ValidateAuditDtosAsync(IList<MatterDocumentActivityUserMinimalDto> auditDtos)
    {
        var validationErrors = new List<ValidationResult>();

        await Task.Run(() =>
        {
            foreach (var result in auditDtos.Select(dto => _validationService.ValidateObject(dto)))
            {
                if (result is ObjectResult { Value: ValidationProblemDetails vpd })
                {
                    validationErrors.AddRange(
                        vpd.Errors.SelectMany(e => e.Value.Select(msg => new ValidationResult(msg, [e.Key])))
                    );
                }
            }
        });

        return validationErrors.Count > 0
            ? CreateValidationErrorResponse(validationErrors, "Validation failed for audit records.")
            : null;
    }

    /// <summary>
    /// Computes SHA256 checksum for file content.
    /// </summary>
    private string ComputeChecksum(byte[] fileBytes)
    {
        ArgumentNullException.ThrowIfNull(fileBytes);
        if (fileBytes.Length == 0)
        {
            throw new ArgumentException("File bytes cannot be empty.", nameof(fileBytes));
        }

        try
        {
            var hash = System.Security.Cryptography.SHA256.HashData(fileBytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing checksum for file of length {Length}.", fileBytes.Length);
            throw new InvalidOperationException("Failed to compute file checksum.", ex);
        }
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

    /// <summary>
    /// Creates a standardized problem details object.
    /// </summary>
    private ProblemDetails CreateProblemDetails(int statusCode, string detail)
    {
        return _problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode, detail: detail);
    }

    /// <summary>
    /// Creates a validation error response from validation results.
    /// </summary>
    private BadRequestObjectResult CreateValidationErrorResponse(
        IEnumerable<ValidationResult> validationResults, string title)
    {
        var errors = validationResults
            .GroupBy(e => e.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage ?? string.Empty).ToArray()
            );

        _logger.LogWarning("{Title} Errors: {@Errors}", title, errors);

        return BadRequest(new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = title,
            Detail = "See the errors property for details."
        });
    }

    #endregion Private Helper Methods
}