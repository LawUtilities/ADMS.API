using ADMS.API.Entities;
using ADMS.API.Helpers;
using ADMS.API.Models;
using ADMS.API.ResourceParameters;
using ADMS.API.Services;

using Asp.Versioning;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using System.Text.Json;

namespace ADMS.API.Controllers
{
    /// <summary>
    /// Document actions
    /// </summary>
    /// <remarks>
    /// Document Controller constructor
    /// </remarks>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/matters/{matterId}/documents")]
    public class DocumentController(
        ILogger<DocumentController> logger,
        IAdmsRepository admsRepository,
        IMapper mapper,
        IPropertyMappingService propertyMappingService,
        IPropertyCheckerService propertyCheckerService,
        ProblemDetailsFactory problemDetailsFactory) : ControllerBase
    {
        private readonly ILogger<DocumentController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IAdmsRepository _admsRepository = admsRepository ?? throw new ArgumentNullException(nameof(admsRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        private readonly IPropertyMappingService _propertyMappingService = propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService = propertyCheckerService;
        private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));

        #region Actions

        /// <summary>
        /// Create new document
        /// </summary>
        /// <param name="matterId">Matter Id to retrieve</param>
        /// <param name="document">document to create</param>
        /// <returns>Document</returns>
        [HttpPost(Name = "CreateDocument")]
        public async Task<ActionResult<DocumentDto>> CreateDocument(
            Guid matterId,
            DocumentForCreationDto document)
        {
            if (document == null)
            {
                return BadRequest("Document is null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model object");
            }

            try
            {
                _logger.LogInformation("Creating a new document for matterId: {matterId}", matterId);

                var resultantDoc = await _admsRepository.AddDocumentAsync(matterId, document);
                await _admsRepository.SaveChangesAsync();

                var documentToReturn = _mapper.Map<DocumentDto>(resultantDoc);

                return CreatedAtRoute("GetDocument",
                    new { matterId, documentId = documentToReturn.Id },
                    documentToReturn);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while creating document: {document}",
                                    args: exception);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieve matter document activity user from list
        /// </summary>
        /// <param name="matterId">Matter Id to retrieve</param>
        /// <param name="documentId">Document Id to retrieve</param>
        /// <returns>List of matter document activity users</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Matter not found</response>
        [HttpGet("{documentId}/GetMDAUFromHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMDAUFromHistoryAsync(
            Guid matterId,
            Guid documentId)
        {
            if (matterId == Guid.Empty || documentId == Guid.Empty)
            {
                return BadRequest("Invalid matterId or documentId");
            }

            try
            {
                _logger.LogInformation("Retrieving document activity users for matterId: {matterId}, documentId: {documentId}", matterId, documentId);

                if (!await _admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound(
                        _problemDetailsFactory.CreateProblemDetails(HttpContext,
                        statusCode: 404,
                        detail: $"Matter with id {matterId} not found"));
                }

                if (!await _admsRepository.DocumentExistsAsync(documentId))
                {
                    return NotFound(
                        _problemDetailsFactory.CreateProblemDetails(HttpContext,
                        statusCode: 404,
                        detail: $"Document with id {documentId} not found"));
                }

                return Ok(await _admsRepository.GetExtendedAuditsAsync(matterId, documentId, "from"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving document activity users for documentId: {documentId}", documentId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieve matter document activity user to list
        /// </summary>
        /// <param name="matterId">Matter Id to retrieve</param>
        /// <param name="documentId">Document Id to retrieve</param>
        /// <returns>List of matter document activity users</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Matter not found</response>
        [HttpGet("{documentId}/GetMDAUToHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMDAUToHistoryAsync(
            Guid matterId,
            Guid documentId)
        {
            if (matterId == Guid.Empty || documentId == Guid.Empty)
            {
                return BadRequest("Invalid matterId or documentId");
            }

            try
            {
                _logger.LogInformation("Retrieving document activity users for matterId: {matterId}, documentId: {documentId}", matterId, documentId);

                if (!await _admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound(
                        _problemDetailsFactory.CreateProblemDetails(HttpContext,
                        statusCode: 404,
                        detail: $"Matter with id {matterId} not found"));
                }

                if (!await _admsRepository.DocumentExistsAsync(documentId))
                {
                    return NotFound(
                        _problemDetailsFactory.CreateProblemDetails(HttpContext,
                        statusCode: 404,
                        detail: $"Document with id {documentId} not found"));
                }

//                return Ok(await _admsRepository.GetMDAUToHistoryAsync(matterId, documentId));
                return Ok(await _admsRepository.GetExtendedAuditsAsync(matterId, documentId, "to"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving document activity users for documentId: {documentId}", documentId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Copies a document from one matter to another
        /// </summary>
        /// <param name="matterId">Matter to copy document to</param>
        /// <param name="document">Document to be copied</param>
        /// <returns>True if document copied, false otherwise</returns>
        /// <response code="200">Returns the created document</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Matter or document not found</response>
        [HttpPost("CopyDocument")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentWithoutRevisionsDto>> CopyDocument(
            Guid matterId,
            DocumentWithoutRevisionsDto document)
        {
            if (matterId == Guid.Empty || document == null)
            {
                return BadRequest("Invalid matterId or document");
            }

            try
            {
                _logger.LogInformation("Copying document to matterId: {matterId}", matterId);

                Matter? matterForCopiedDocument = await _admsRepository.GetMatterAsync(matterId, false);

                if (matterForCopiedDocument == null)
                {
                    return NotFound("Cannot find matter to copy document to");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!TryValidateModel(document))
                {
                    return BadRequest(ModelState);
                }

                var result = await _admsRepository.CopyDocumentAsync(matterId, document);

                if (result)
                {
                    return Ok(true);
                }
                return Ok(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while copying the document to matterId: {matterId}", matterId);
                return StatusCode(500, "Internal server error");
            }
        }
        /// <summary>
        /// Moves a document from one matter to another
        /// </summary>
        /// <param name="matterId">Matter to move document to</param>
        /// <param name="document">Document to be moved</param>
        /// <returns>True if document moved, false otherwise</returns>
        /// <response code="200">Returns the created document</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Matter or document not found</response>
        [HttpPost("MoveDocument")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentWithoutRevisionsDto>> MoveDocument(
            Guid matterId,
            DocumentWithoutRevisionsDto document)
        {
            if (matterId == Guid.Empty || document == null)
            {
                return BadRequest("Invalid matterId or document");
            }

            try
            {
                _logger.LogInformation("Moving document to matterId: {matterId}", matterId);

                Matter? matterForMovedDocument = await _admsRepository.GetMatterAsync(matterId, false);

                if (matterForMovedDocument == null)
                {
                    return NotFound("Cannot find matter to move document to");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!TryValidateModel(document))
                {
                    return BadRequest(ModelState);
                }

                var result = await _admsRepository.MoveDocumentAsync(matterId, document);

                if (result)
                {
                    return Ok(true);
                }
                return Ok(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while moving the document to matterId: {matterId}", matterId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete specified document Id
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">Document to be deleted</param>
        /// <returns>IActionResult</returns>
        /// <response code="204">Returns No Content</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Document not found</response>
        [HttpDelete("{documentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDocument(
            Guid matterId,
            Guid documentId)
        {
            if (matterId == Guid.Empty || documentId == Guid.Empty)
            {
                return BadRequest("Invalid matterId or documentId");
            }

            try
            {
                _logger.LogInformation("Deleting document with documentId: {documentId} from matterId: {matterId}", documentId, matterId);

                if (!await _admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                Document? document = await _admsRepository.GetDocumentAsync(documentId, false);
                if (document == null)
                {
                    return NotFound("Document not found");
                }

                document.IsDeleted = true;
                if (!await _admsRepository.DeleteDocumentAsync(_mapper.Map<DocumentDto>(document)))
                {
                    return BadRequest("Could not delete Document");
                }

                if (!await _admsRepository.SaveChangesAsync())
                {
                    return BadRequest("Could not delete Document");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the document with documentId: {documentId}", documentId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Check out document
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">Document id to be checked out</param>
        /// <returns>IActionResult</returns>
        [HttpGet("{documentId}/checkout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckoutDocument(
            Guid matterId,
            Guid documentId)
        {
            if (matterId == Guid.Empty || documentId == Guid.Empty)
            {
                return BadRequest("Invalid matterId or documentId");
            }

            try
            {
                _logger.LogInformation("Checking out document with documentId: {documentId} from matterId: {matterId}", documentId, matterId);

                if (!await _admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.DocumentExistsAsync(documentId))
                {
                    return NotFound("Document not found");
                }

                return await _admsRepository.CheckoutDocumentAsync(documentId) ? Ok() : BadRequest("Cannot check out document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking out the document with documentId: {documentId}", documentId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Check in document
        /// </summary>
        /// <param name="matterId">Matter document belongs to</param>
        /// <param name="documentId">Document id to be checked in</param>
        /// <returns>IActionResult</returns>
        [HttpGet("{documentId}/checkin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckinDocument(
            Guid matterId,
            Guid documentId)
        {
            if (matterId == Guid.Empty || documentId == Guid.Empty)
            {
                return BadRequest("Invalid matterId or documentId");
            }

            try
            {
                _logger.LogInformation("Checking in document with documentId: {documentId} from matterId: {matterId}", documentId, matterId);

                if (!await _admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.DocumentExistsAsync(documentId))
                {
                    return NotFound("Document not found");
                }

                return await _admsRepository.CheckinDocumentAsync(documentId) ? Ok() : BadRequest("Cannot check in document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking in the document with documentId: {documentId}", documentId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets specific document history
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">Document Id to retrieve</param>
        /// <returns>Document history</returns>
        /// <response code="200">Returns the requested document history</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Matter or document not found</response>
        [HttpGet("{documentId}/GetHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocumentHistoryAsync(
            Guid matterId,
            Guid documentId)
        {
            if (matterId == Guid.Empty || documentId == Guid.Empty)
            {
                return BadRequest("Invalid matterId or documentId");
            }

            try
            {
                _logger.LogInformation("Retrieving document history for documentId: {documentId} from matterId: {matterId}", documentId, matterId);

                if (!await _admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                IEnumerable<DocumentActivityUser> documentHistory = await _admsRepository.GetDocumentWithHistoryByIdAsync(documentId);
                if (documentHistory == null)
                {
                    return NotFound("Document history not found");
                }

                return Ok(_mapper.Map<IEnumerable<DocumentActivityUserDto>>(documentHistory));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving document history for documentId: {documentId}", documentId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get document lists
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentsResourceParameters">Document search details</param>
        /// <returns>List of documents to be returned</returns>
        [HttpGet(Name = "GetDocuments")]
        [HttpHead]
        public async Task<IActionResult> GetDocuments(
            Guid matterId,
            [FromQuery] DocumentsResourceParameters documentsResourceParameters)
        {
            if (matterId == Guid.Empty)
            {
                return BadRequest("Invalid matterId");
            }

            if (!_propertyMappingService
                .ValidMappingExistsFor<DocumentDto, Document>(
                documentsResourceParameters.OrderBy))
            {
                return BadRequest("Invalid order by parameter");
            }

            if (!_propertyCheckerService.TypeHasProperties<DocumentDto>
                (documentsResourceParameters.Fields))
            {
                return BadRequest(
                    _problemDetailsFactory.CreateProblemDetails(HttpContext,
                        statusCode: 400,
                        detail: $"Not all requested data shaping fields exist on " +
                        $"the resource: {documentsResourceParameters.Fields}"));
            }

            _logger.LogInformation("Retrieving documents for matterId: {matterId}", matterId);

            PagedList<Document> documentsFromRepo = await _admsRepository.GetDocumentsAsync(matterId, documentsResourceParameters);

            var paginationMetadata = new
            {
                totalCount = documentsFromRepo.TotalCount,
                pageSize = documentsFromRepo.PageSize,
                currentPage = documentsFromRepo.CurrentPage,
                totalPages = documentsFromRepo.TotalPages
            };

            if (Response == null)
            {
                _logger.LogError("Response object is null while retrieving documents for matterId: {matterId}", matterId);
                return StatusCode(500, "Internal server error");
            }
            else
            {
                Response.Headers.Append("X-Pagination",
                       JsonSerializer.Serialize(paginationMetadata));
            }

            return Ok(_mapper.Map<IEnumerable<DocumentDto>>(documentsFromRepo));
        }

        /// <summary>
        /// Get specific document
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">Document Id to retrieve</param>
        /// <param name="fields">Extra fields if necessary</param>
        [HttpGet("{documentId}", Name = "GetDocument")]
        public async Task<IActionResult> GetDocument(
            Guid matterId,
            Guid documentId,
            string? fields)
        {
            if (matterId == Guid.Empty || documentId == Guid.Empty)
            {
                return BadRequest("Invalid matterId or documentId");
            }

            if (!_propertyCheckerService.TypeHasProperties<DocumentDto>(fields))
            {
                return BadRequest(
                  _problemDetailsFactory.CreateProblemDetails(HttpContext,
                      statusCode: 400,
                      detail: $"Not all requested data shaping fields exist on " +
                      $"the resource: {fields}"));
            }

            _logger.LogInformation("Retrieving document with documentId: {documentId} from matterId: {matterId}", documentId, matterId);

            // Get document from repo
            var documentFromRepo = await _admsRepository.GetDocumentAsync(documentId);

            if (documentFromRepo == null)
            {
                return NotFound("Document not found");
            }

            return Ok(_mapper.Map<DocumentDto>(documentFromRepo));

            /*
            IEnumerable<LinkDto> links = CreateLinksForDocument(
                matterId,
                documentId,
                fields);

            IDictionary<string, object?> friendlyResourceToReturn = _mapper.Map<DocumentDto>(documentFromRepo)
                .ShapeData(fields) as IDictionary<string, object?>;

            friendlyResourceToReturn.Add("links", links);

            return Ok(friendlyResourceToReturn);
            */
        }

        /// <summary>
        /// Updates document details
        /// </summary>
        /// <param name="matterId">Matter document belongs to</param>
        /// <param name="documentId">Document id to be updated</param>
        /// <param name="document">Document details in need of updating</param>
        /// <returns>IActionResult</returns>
        /// <response code="204">Returns No Content</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Document not found</response>
        [HttpPut("{documentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDocument(
            Guid matterId,
            Guid documentId,
            DocumentForUpdateDto document)
        {
            if (matterId == Guid.Empty || documentId == Guid.Empty)
            {
                return BadRequest("Invalid matterId or documentId");
            }

            try
            {
                _logger.LogInformation("Updating document with documentId: {documentId} for matterId: {matterId}", documentId, matterId);

                if (!await _admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                Document? documentEntity = await _admsRepository.GetDocumentAsync(documentId, false);
                if (documentEntity == null)
                {
                    return NotFound("Document not found");
                }

                _mapper.Map(document, documentEntity);
                await _admsRepository.UpdateDocumentAsync(documentEntity);

                if (!await _admsRepository.SaveChangesAsync())
                {
                    return BadRequest("Could not update document");
                }

                DocumentActivity? documentActivity = await _admsRepository.GetDocumentActivityByActivityNameAsync("SAVED");
                // TODO: Add appropriate username code after authentication
                User? user = await _admsRepository.GetUserByUsernameAsync("rbrown");

                if (documentActivity != null && user != null)
                {
                    documentEntity.DocumentActivityUsers.Add(
                        new DocumentActivityUser()
                        {
                            Document = documentEntity,
                            DocumentId = documentEntity.Id,
                            DocumentActivity = documentActivity,
                            DocumentActivityId = documentActivity.Id,
                            User = user,
                            UserId = user.Id,
                            CreatedAt = DateTime.Now.ToUniversalTime(),
                        });
                }

                var documentToReturn = _mapper.Map<DocumentWithoutRevisionsDto>(documentEntity);
                return await _admsRepository.SaveChangesAsync() ?
                    CreatedAtRoute("GetDocument", new { matterId, documentId = documentToReturn.Id }, documentToReturn) :
                    BadRequest("Could not save updates");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the document with documentId: {documentId}", documentId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieve document options
        /// </summary>
        /// <returns>OK</returns>
        [HttpOptions()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetDocumentOptions()
        {
            _logger.LogInformation("Retrieving document options");

            try
            {
                Response.Headers.Append("Allow", "GET,HEAD,POST,OPTIONS");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving document options");
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion Actions
    }
}