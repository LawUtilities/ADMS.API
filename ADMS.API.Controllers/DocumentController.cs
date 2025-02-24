using ADMS.API.ActionConstraints;
using ADMS.API.Entities;
using ADMS.API.Helpers;
using ADMS.API.Models;
using ADMS.API.ResourceParameters;
using ADMS.API.Services;

using Asp.Versioning;

using AutoMapper;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

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
    [Asp.Versioning.ApiVersion("1.0")]
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
        [RequestHeaderMatchesMediaType("Content-Type",
            "*/*",
            "application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<DocumentDto>> CreateDocument(
            Guid matterId,
            DocumentForCreationDto document)
        {
            Document documentEntity = _mapper.Map<Document>(document);

            _admsRepository.AddDocument(documentEntity);
            await _admsRepository.SaveChangesAsync();

            var documentToReturn = _mapper.Map<DocumentDto>(documentEntity);

            return CreatedAtRoute("GetDocument",
                new { documentId = documentToReturn.Id },
                documentToReturn);
        }


        /// <summary>
        /// retrieve matter document activity user from list
        /// </summary>
        /// <param name="matterId">Matter Id to retrieve</param>
        /// <param name="documentId">Document Id to retrieve</param>
        /// <returns>list of matter document activity users</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter not found</response>
        [HttpGet("{documentId}/GetMDAUFromHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMDAUFromHistoryAsync(
            Guid matterId,
            Guid documentId)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound(
                        _problemDetailsFactory.CreateProblemDetails(HttpContext,
                        statusCode: 404,
                        detail: $"Matter with id {matterId} not found"));
                }

                if (!await _admsRepository.CheckDocumentExistsAsync(documentId))
                {
                    return NotFound(
                        _problemDetailsFactory.CreateProblemDetails(HttpContext,
                        statusCode: 404,
                        detail: $"Document with id {documentId} not found"));
                }

                return Ok(await _admsRepository.GetMDAUFromHistoryAsync(Guid.Empty, documentId));
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while retrieving Document with id: {documentId}",
                                    args: exception);
                return BadRequest(
                    _problemDetailsFactory.CreateProblemDetails(HttpContext,
                    statusCode: 400,
                    detail: $"Document with id {documentId} could not be retrieved."));
            }
        }

        /// <summary>
        /// retrieve matter document activity user to list
        /// </summary>
        /// <param name="matterId">Matter Id to retrieve</param>
        /// <param name="documentId">Document Id to retrieve</param>
        /// <returns>list of matter document activity users</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter not found</response>
        [HttpGet("{documentId}/GetMDAUToHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMDAUToHistoryAsync(
            Guid matterId,
            Guid documentId)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound(
                        _problemDetailsFactory.CreateProblemDetails(HttpContext,
                        statusCode: 404,
                        detail: $"Matter with id {matterId} not found"));
                }

                if (!await _admsRepository.CheckDocumentExistsAsync(documentId))
                {
                    return NotFound(
                        _problemDetailsFactory.CreateProblemDetails(HttpContext,
                        statusCode: 404,
                        detail: $"Document with id {documentId} not found"));
                }

                return Ok(await _admsRepository.GetMDAUToHistoryAsync(Guid.Empty, documentId));
            }
            catch (Exception exception)
            {
                _logger.LogCritical("An error occured while retrieving Document with id: {documentId}", exception);
                return BadRequest(
                    _problemDetailsFactory.CreateProblemDetails(HttpContext,
                    statusCode: 400,
                    detail: $"Document with id {documentId} could not be retrieved."));
            }
        }

        /// <summary>
        /// Copies a document from one matter to another
        /// </summary>
        /// <param name="matterId">matter to copy document to</param>
        /// <param name="document">document to be copied</param>
        /// <returns>true if document copied, false otherwise</returns>
        /// <response code="200">Returns the created document</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter or document not found</response>
        [HttpPost("MoveDocument")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentWithoutRevisionsDto>> MoveDocument(
            Guid matterId,
            DocumentWithoutRevisionsDto document)
        {
            try
            {
                Matter? matterForMovedDocument = await _admsRepository.GetMatterAsync(matterId, false);

                if (matterForMovedDocument == null)
                {
                    return BadRequest("Cannot find matter to move document from");
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
                    return Ok(Task.FromResult(true));
                }
                return Ok(Task.FromResult(false));
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while creating document: {document}",
                                    args: exception);
                return BadRequest();
            }
        }

        /// <summary>
        /// Delete specified document Id
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">document to be deleted</param>
        /// <returns>IActionResult</returns>
        /// <response code="204">Returns No Content</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Document not found</response>
        [HttpDelete("{documentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDocument(
            Guid matterId,
            Guid documentId)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                Document? document = await _admsRepository.GetDocumentAsync(documentId, false);
                if (document != null)
                {
                    document.IsDeleted = true;
                    if (await _admsRepository.DeleteDocumentAsync(_mapper.Map<DocumentDto>(document)))
                    {
                        return !await _admsRepository.SaveChangesAsync() ? BadRequest("Could not delete Document") : NoContent();
                    }
                    else
                    {
                        return BadRequest("Could not delete Document");
                    }
                }
                return NotFound("Document not found");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while retrieving Document with id: {documentId}",
                                    args: exception);
                return BadRequest("An error occured");
            }
        }


        /// <summary>
        /// Check out document
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">document id to be checked out</param>
        /// <returns>IActionResult</returns>
        [HttpGet("{documentId}/checkout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckoutDocument(
            Guid matterId,
            Guid documentId)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.CheckDocumentExistsAsync(documentId))
                {
                    return NotFound("Document not found");
                }

                return await _admsRepository.CheckoutDocumentAsync(documentId) ? Ok() : BadRequest("Cannot check out document");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while checking out document with id: {documentId}",
                                    args: exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// Check in document
        /// </summary>
        /// <param name="matterId">Matter document belongs to</param>
        /// <param name="documentId">document id to be checked in</param>
        /// <returns>IActionResult</returns>
        [HttpGet("{documentId}/checkin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckinDocument(
            Guid matterId,
            Guid documentId)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.CheckDocumentExistsAsync(documentId))
                {
                    return NotFound("Document not found");
                }

                return await _admsRepository.CheckinDocumentAsync(documentId) ? Ok() : BadRequest("Cannot check in document");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while checking in document with id: {id}",
                                    args: exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// gets specific document history
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">Document Id to retrieve</param>
        /// <returns>document history</returns>
        /// <response code="200">Returns the requested document history</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter or document not found</response>
        [HttpGet("{documentId}/GetHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocumentHistoryAsync(
            Guid matterId,
            Guid documentId)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                IEnumerable<DocumentActivityUser> documentHistory = await _admsRepository.GetDocumentWithHistoryByIdAsync(documentId);
                if (documentHistory == null)
                {
                    return NotFound("Document History not found");
                }

                return Ok(_mapper.Map<IEnumerable<DocumentActivityUserDto>>(documentHistory));
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while retrieving document history with id: {id}",
                                    args: exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// Get document lists
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentsResourceParameters">document search details</param>
        /// <returns>list of documents to be returned</returns>
        [HttpGet(Name = "GetDocuments")]
        [HttpHead]
        public async Task<IActionResult> GetDocuments(
            Guid matterId,
            [FromQuery] DocumentsResourceParameters documentsResourceParameters)
        {
            if (!_propertyMappingService
                .ValidMappingExistsFor<DocumentDto, Document>(
                documentsResourceParameters.OrderBy))
            {
                return BadRequest();
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

            PagedList<Document> documentsFromRepo = await _admsRepository.GetDocumentsAsync(documentsResourceParameters);

            var paginationMetadata = new
            {
                totalCount = documentsFromRepo.TotalCount,
                pageSize = documentsFromRepo.PageSize,
                currentPage = documentsFromRepo.CurrentPage,
                totalPages = documentsFromRepo.TotalPages
            };

            if (Response == null)
            {
                return BadRequest();
            }
            else
            {
                Response.Headers.Append("X-Pagination",
                       JsonSerializer.Serialize(paginationMetadata));
            }

            /*
            // create links
            List<LinkDto> links = CreateLinksForDocuments(documentsResourceParameters,
                documentsFromRepo.HasNext,
                documentsFromRepo.HasPrevious);

            IEnumerable<System.Dynamic.ExpandoObject> shapedDocuments = _mapper.Map<IEnumerable<DocumentDto>>(documentsFromRepo)
                                   .ShapeData(documentsResourceParameters.Fields);

            IEnumerable<IDictionary<string, object?>> shapedDocumentsWithLinks = shapedDocuments.Select(document =>
            {
                IDictionary<string, object?> documentAsDictionary = document;

                Guid documentId = (Guid)documentAsDictionary["Id"]!;

                var documentLinks = CreateLinksForDocument(
                    matterId,
                    documentId,
                    null);
                documentAsDictionary.Add("links", documentLinks);
                return documentAsDictionary;
            });

            (IEnumerable<IDictionary<string, object?>> value, List<LinkDto> links) linkedCollectionResource = (
                value: shapedDocumentsWithLinks,
                links
            );
            */

            // return them
            return Ok(documentsFromRepo);
        }


        /// <summary>
        /// specific document with HATEOAS links
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">Document Id to retrieve</param>
        /// <param name="fields">extra fields if necessary</param>
        /// <returns>document containing HATEOAS links</returns>
        [RequestHeaderMatchesMediaType("Accept",
            "*/*",
            "application/json")]
        [Produces("application/json")]
        [HttpGet("{documentId}", Name = "GetDocument")]
        public async Task<IActionResult> GetDocumentWithLinks(
            Guid matterId,
            Guid documentId,
            string? fields)
        {
            if (matterId == Guid.Empty || documentId == Guid.Empty)
            {
                return NotFound();
            }

            if (!_propertyCheckerService.TypeHasProperties<DocumentDto>
                    (fields))
            {
                return BadRequest(
                  _problemDetailsFactory.CreateProblemDetails(HttpContext,
                      statusCode: 400,
                      detail: $"Not all requested data shaping fields exist on " +
                      $"the resource: {fields}"));
            }

            // get document from repo
            Document documentFromRepo = await _admsRepository
                .GetDocumentAsync(documentId);

            if (documentFromRepo == null)
            {
                return NotFound();
            }

            /*
            IEnumerable<LinkDto> links = CreateLinksForDocument(
                matterId,
                documentId,
                fields);

            // friendly document
            IDictionary<string, object?> friendlyResourceToReturn = _mapper.Map<DocumentDto>(documentFromRepo)
                .ShapeData(fields) as IDictionary<string, object?>;

            friendlyResourceToReturn.Add("links", links);

            return Ok(friendlyResourceToReturn);
            */
            return Ok(_mapper.Map<DocumentDto>(documentFromRepo));
        }


        /// <summary>
        /// Updates document details
        /// </summary>
        /// <param name="matterId">Matter document belongs to</param>
        /// <param name="documentId">document id to be updated</param>
        /// <param name="document">document details in need of updating</param>
        /// <returns>IActionResult</returns>
        /// <response code="204">Returns No Content</response>
        /// <response code="400">An error occured</response>
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
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
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

                DocumentActivity? documentActivity = await _admsRepository.GetDcumentActivityByActivity("SAVED");
                // TODO: Add appropriate username code after authentication
                User? user = await _admsRepository.GetUserByUsername("rbrown");

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
                        CreatedAtRoute("GetDocument",
                            new { matterId, documentId = documentToReturn.Id },
                            documentToReturn) :
                    BadRequest("Could not save updates");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while updating Document with id: {id}",
                                    args: exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// retrieve document options
        /// </summary>
        /// <returns>OK</returns>
        [HttpOptions()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetDocumentOptions()
        {
            Response.Headers.Append("Allow", "GET,HEAD,POST,OPTIONS");
            return Ok();
        }

        #endregion Actions

        #region Helper Methods

        /*

        /// <summary>
        /// Create HATEOAS links for document list
        /// </summary>
        /// <param name="documentsResourceParameters">document details to source data for</param>
        /// <param name="hasNext">find next document</param>
        /// <param name="hasPrevious">find previous document</param>
        /// <returns></returns>
        private List<LinkDto> CreateLinksForDocuments(
            DocumentsResourceParameters documentsResourceParameters,
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>
            {
                // self 
                new(CreateDocumentsResourceUri(documentsResourceParameters,
                    ResourceUriType.Current),
                    "self",
                    "GET")
            };

            if (hasNext)
            {
                links.Add(
                    new(CreateDocumentsResourceUri(documentsResourceParameters,
                        ResourceUriType.NextPage),
                    "nextPage",
                    "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new(CreateDocumentsResourceUri(documentsResourceParameters,
                        ResourceUriType.PreviousPage),
                    "previousPage",
                    "GET"));
            }

            return links;
        }

        /// <summary>
        /// Create HATEOAS links for specific document
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">Document Id to retrieve</param>
        /// <param name="fields">document fields to use</param>
        /// <returns>links for specified document</returns>
        private List<LinkDto> CreateLinksForDocument(
            Guid matterId,
            Guid documentId,
            string? fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new(Url.Link("GetDocument", new { matterId, documentId }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new(Url.Link("GetDocument", new { matterId, documentId, fields }),
                  "self",
                  "GET"));
            }

            links.Add(
                 new(Url.Link("GetMatter", new { matterId }),
                 "matters",
                 "GET"));

            return links;
        }

        //private string? CreateDocumentResourceUri(
        //    DocumentsResourceParameters documentsResourceParameters,
        //    ResourceUriType type)
        //{

        //}

        /// <summary>
        /// Generate document URI
        /// </summary>
        /// <param name="documentsResourceParameters">document details</param>
        /// <param name="type">type of link to create</param>
        /// <returns>link</returns>
        private string? CreateDocumentsResourceUri(
            DocumentsResourceParameters documentsResourceParameters,
            ResourceUriType type)
        {
            return type switch
            {
                ResourceUriType.PreviousPage => Url.Link("GetDocuments",
                                        new
                                        {
                                            fields = documentsResourceParameters.Fields,
                                            orderBy = documentsResourceParameters.OrderBy,
                                            pageNumber = documentsResourceParameters.PageNumber - 1,
                                            pageSize = documentsResourceParameters.PageSize,
                                            fileName = documentsResourceParameters.FileName,
                                            searchQuery = documentsResourceParameters.SearchQuery
                                        }),
                ResourceUriType.NextPage => Url.Link("GetDocuments",
                        new
                        {
                            fields = documentsResourceParameters.Fields,
                            orderBy = documentsResourceParameters.OrderBy,
                            pageNumber = documentsResourceParameters.PageNumber + 1,
                            pageSize = documentsResourceParameters.PageSize,
                            fileName = documentsResourceParameters.FileName,
                            searchQuery = documentsResourceParameters.SearchQuery
                        }),
                _ => Url.Link("GetDocuments",
                        new
                        {
                            fields = documentsResourceParameters.Fields,
                            orderBy = documentsResourceParameters.OrderBy,
                            pageNumber = documentsResourceParameters.PageNumber,
                            pageSize = documentsResourceParameters.PageSize,
                            fileName = documentsResourceParameters.FileName,
                            searchQuery = documentsResourceParameters.SearchQuery
                        }),
            };
        }

        */

        #endregion Helper Methods

        #region Extra Methods Not Used

        /*
        /// <summary>
        /// Get specific Document
        /// </summary>
        /// <param name="matterId">matter containing document(s)</param>
        /// <param name="documentId">Document Id to retrieve</param>
        /// <param name="includeRevisions">Whether to include revisions with the returned data</param>
        /// <returns>An IActionResult</returns>
        /// <response code="200">Returns the requestedDocument</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Document not found</response>
        [HttpGet("{documentId}", Name = "GetDocument")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocument(
            Guid matterId,
            Guid documentId,
            bool includeRevisions = false)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                Document? document = await _admsRepository.GetDocumentAsync(documentId, includeRevisions);
                if (document == null)
                {
                    return NotFound("Document not found");
                }

                return includeRevisions
                    ? Ok(_mapper.Map<DocumentWithRevisionsDto>(document))
                    : Ok(_mapper.Map<DocumentWithoutRevisionsDto>(document));
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while retrieving Document with id: {documentId}",
                                    args: exception);
                return BadRequest("An error occured");
            }
        }
        */

        /*
        /// <summary>
        /// Creates a document attached to a specified matter
        /// </summary>
        /// <param name="matterId">matter to add the document to</param>
        /// <param name="document">document to add</param>
        /// <returns></returns>
        /// <response code="200">Returns the created document</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter or document not found</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentDto>> CreateDocument(
            Guid matterId,
            DocumentForCreationDto document)
        {
            try
            {
                Matter? matterForDocument = await _admsRepository.GetMatterAsync(matterId, false);
                if (matterForDocument == null)
                {
                    return BadRequest("Matter not found");
                }

                if (!ModelState.IsValid) 
                {
                    return BadRequest(ModelState);
                }

                Document? addedDocument = await _admsRepository.AddDocumentAsync(matterId, document);

                if (addedDocument == null)
                {
                    return BadRequest("Document could not be added.");
                }
                return await _admsRepository.SaveChangesAsync() ?
                        CreatedAtRoute("GetDocument",
                            new { matterId, documentId = addedDocument.Id },
                            _mapper.Map<DocumentWithRevisionsDto>(addedDocument)) :
                        NotFound("Could not save document");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while creating document: {document}",
                                    args: exception);
                return BadRequest();
            }
        }
        */

        /*
        /// <summary>
        /// Copies a document from one matter to another
        /// </summary>
        /// <param name="matterId">matter to copy document to</param>
        /// <param name="documentId">document to be copied</param>
        /// <param name="newMatterId">new matter to copy to</param>
        /// <returns>true if document copied, false otherwise</returns>
        /// <response code="200">Returns the created document</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter or document not found</response>
        [HttpPost("{documentId}/CopyDocument/{newMatterId}")]
        /// <param name="documentId">Document Id to copy</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentWithoutRevisionsDto>> CopyDocument(
            Guid matterId,
            Guid documentId,
            Guid newMatterId)
        {
            try
            {
                
                //Document? newDocument = await _admsRepository.GetDocumentAsync(documentId, false);
                //if (newDocument == null)
                //{
                //    return NotFound("Cannot find document");
                //}

                //Matter? newMatter = await _admsRepository.GetMatterAsync(newMatterId, false);
                //if (newMatter == null)
                //{
                //    return NotFound("Cannot find new matter");
                //}

                //newDocument.MatterId = newMatterId;
                //newDocument.Matter = newMatter;
                
                Matter? matterForCopiedDocument = await _admsRepository.GetMatterAsync(newMatterId, false);

                if (matterForCopiedDocument == null)
                {
                    return BadRequest("Cannot find matter to copy document from");
                }

                DocumentWithoutRevisionsDto finalDocument = document;

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!TryValidateModel(finalDocument))
                {
                    return BadRequest(ModelState);
                }

                var result = await _admsRepository.CopyDocumentAsync(matterId, finalDocument);

                if (result)
                {
                    //RevisionDto revisionToAdd = new()
                    //{
                    //    RevisionId = 1,
                    //    CreationDate = DateTime.Now.ToUniversalTime(),
                    //    ModificationDate = DateTime.Now.ToUniversalTime(),
                    //};

                    //await _admsRepository.AddRevisionAsync(finalDocument.Id, revisionToAdd);

                    return await _admsRepository.SaveChangesAsync() ? Ok(Task.FromResult(true)) : Ok(Task.FromResult(false));
                }
                return BadRequest("Could not find document");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while copying document: {document}",
                                    args: exception);
                return BadRequest();
            }
        }
        */

        /*
       /// <summary>
       /// 
       /// </summary>
       /// <param name="matterId">Matter containing document</param>
       /// <param name="documentId">Document Id to retrieve</param>
       /// <param name="fields"></param>
       /// <returns></returns>
       [RequestHeaderMatchesMediaType("Accept",
           "application/json",
           "application/vnd.marvin.document.friendly+json")]
       [Produces("application/json",
           "application/vnd.marvin.document.friendly+json")]
       [HttpGet("{documentId}/GetDocumentWithoutLinks", Name = "GetDocumentWithoutLinks")]
       public async Task<IActionResult> GetDocumentWithoutLinks(
           Guid matterId,
           Guid documentId,
           string? fields)
       {
           if (!_propertyCheckerService.TypeHasProperties<DocumentDto>
                   (fields))
           {
               return BadRequest(
                 _problemDetailsFactory.CreateProblemDetails(HttpContext,
                     statusCode: 400,
                     detail: $"Not all requested data shaping fields exist on " +
                     $"the resource: {fields}"));
           }

           // get author from repo
           var documentFromRepo = await _admsRepository
               .GetDocumentAsync(documentId);

           if (documentFromRepo == null)
           {
               return NotFound();
           }

           // friendly author
           var friendlyResourceToReturn = _mapper.Map<DocumentDto>(documentFromRepo)
               .ShapeData(fields);

           return Ok(friendlyResourceToReturn);
       }
       */

        /*
        /// <summary>
        /// specific document without HATEOAS links
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">Document Id to retrieve</param>
        /// <param name="fields">extra fields if necessary</param>
        /// <returns>document not containing HATEOAS links</returns>
        [RequestHeaderMatchesMediaType("Accept",
            "application/vnd.marvin.document.full+json")]
        [Produces("application/vnd.marvin.document.full+json")]
        [HttpGet("{documentId}/GetFullDocumentWithoutLinks")]
        public async Task<IActionResult> GetFullDocumentWithoutLinks(Guid documentId,
            string? fields)
        {
            if (!_propertyCheckerService.TypeHasProperties<DocumentDto>
                    (fields))
            {
                return BadRequest(
                  _problemDetailsFactory.CreateProblemDetails(HttpContext,
                      statusCode: 400,
                      detail: $"Not all requested data shaping fields exist on " +
                      $"the resource: {fields}"));
            }

            // get author from repo
            var documentFromRepo = await _admsRepository
                .GetDocumentAsync(documentId);

            if (documentFromRepo == null)
            {
                return NotFound();
            }

            var fullResourceToReturn = _mapper.Map<DocumentFullDto>(documentFromRepo)
                .ShapeData(fields);

            return Ok(fullResourceToReturn);
        }
        */

        /*
        /// <summary>
        /// Retrieves desired documents.
        /// </summary>
        /// <param name="matterId">Id of the matter related to the documents in question</param>
        /// <param name="filename">filename required</param>
        /// <param name="searchQuery">additional search parameters</param>
        /// <param name="isDeleted">indicates if deleted documents are returned</param>
        /// <param name="pageNumber">the number of the paged results desired</param>
        /// <param name="pageSize">how large the record set is per page</param>
        /// <returns>An IActionResult</returns>
        /// <response code="200">Returns the requested document</response>
        /// <response code="400">An error occured</response>
        [HttpGet(Name = "GetDocuments")]
        [HttpHead]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocuments(
                    Guid matterId,
                    string? filename,
                    string? searchQuery,
                    bool isDeleted = false,
                    int pageNumber = 1,
                    int pageSize = 10)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (pageSize > maxDocumentsPageSize)
                {
                    pageSize = maxDocumentsPageSize;
                }

                (IEnumerable<Document> documentEntities, PaginationMetadata paginationMetadata) = await _admsRepository.GetDocumentsAsync(
                    matterId, filename, searchQuery, isDeleted, pageNumber, pageSize);

                Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

                return Ok(_mapper.Map<IEnumerable<DocumentWithRevisionsDto>>(documentEntities));
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while retrieving documentsfrom Matter: {matterId}",
                                    args: exception);
                return BadRequest("An error occured");
            }
        }
        */

        /*

        /// <summary>
        /// Updated document with nerw changes
        /// </summary>
        /// <param name="matterId">Matter document belongs to</param>
        /// <param name="documentId">document id to be patched</param>
        /// <param name="patchDocument">updates to be applied</param>
        /// <returns>IACtionResult</returns>
        /// <response code="204">Returns No Content</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Document not found</response>
        [HttpPatch("{documentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PartiallyUpdateDocument(
            Guid matterId,
            Guid documentId,
            JsonPatchDocument<DocumentForUpdateDto> patchDocument)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                Document? documentEntity = await _admsRepository.GetDocumentAsync(documentId, true);
                if (documentEntity == null)
                {
                    var documentDto = new DocumentForUpdateDto()
                    {
                        FileName = "*",
                        Extension = ".*",
                    };
                    patchDocument.ApplyTo(documentDto);
                    var documentToAdd = _mapper.Map<Document>(documentDto);
                    documentToAdd.Id = documentId;
                    
                    await _admsRepository.AddDocumentAsync(matterId, documentToAdd);
                    await _admsRepository.SaveChangesAsync();

                    var documentToReturn = _mapper.Map<DocumentDto>(documentToAdd);
                    return CreatedAtRoute("GetDocument",
                        new { matterId, documentId = documentToReturn.Id },
                        documentToReturn);
                }

                DocumentForUpdateDto documentToPatch = _mapper.Map<DocumentForUpdateDto>(documentEntity);
                patchDocument.ApplyTo(documentToPatch);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!TryValidateModel(documentToPatch))
                {
                    return BadRequest(ModelState);
                }

                _mapper.Map(documentToPatch, documentEntity);

                DocumentActivity? documentActivity = await _admsRepository.GetDcumentActivityByActivity("SAVED");
                // TODO: Add appropriate username code after authentication
                User? user = await _admsRepository.GetUserByUsername("rbrown");

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
                            CreatedAt = DateTime.Now.ToUniversalTime()
                        });

                    Document? result = await _admsRepository.UpdateDocumentAsync(documentEntity);

                    return (result == null) ? BadRequest("Could not update document") : NoContent();
                }
                else
                {
                    return BadRequest("Could not update document");
                }
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while retrieving Document with id: {id}",
                                    args: exception);
                return BadRequest("An error occured");
            }
        }
        */

        #endregion Extra Methods Not Used
    }
}