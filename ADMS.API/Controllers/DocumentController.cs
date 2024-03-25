using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.Services;

using Asp.Versioning;

using AutoMapper;

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

using System.Text.Json;

namespace ADMS.API.Controllers
{
    /// <summary>
    /// Document actions
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/matters/{matterId}/documents")]
    public class DocumentController : ControllerBase
    {
        private readonly ILogger<DocumentController> _logger;
        private readonly IAdmsRepository _admsRepository;
        private readonly IMapper _mapper;

        const int maxDocumentsPageSize = 50;
        private const string ServerFilesPath = @"C:\Dev\Repos\ADMSServerFiles";
        private const string PdfFolderName = "PDF";

        /// <summary>
        /// Document Controller constructor
        /// </summary>
        public DocumentController(
            ILogger<DocumentController> logger,
            IAdmsRepository admsRepository,
            IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _admsRepository = admsRepository ?? throw new ArgumentNullException(nameof(admsRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        #region Actions

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
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }
                Matter? matterForDocument = await _admsRepository.GetMatterAsync(matterId, false);

                if (await _admsRepository.GetDocumentByFileNameAsync(matterId, document.FileName) != null)
                {
                    return BadRequest("Document already exists");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Document? addedDocument = await _admsRepository.AddDocumentAsync(matterId, document);
                if (!await _admsRepository.SaveChangesAsync())
                {
                    return BadRequest("Could not save document");
                }

                if (addedDocument != null)
                {
                    RevisionDto revisionToAdd = new()
                    {
                        RevisionId = 1,
                        CreationDate = document.CreationDate,
                        ModificationDate = document.ModificationDate
                    };

                    await _admsRepository.AddRevisionAsync(addedDocument.Id, revisionToAdd);

                    return await _admsRepository.SaveChangesAsync() ? (ActionResult<DocumentDto>)Ok(_mapper.Map<DocumentDto>(addedDocument)) : NotFound("Could not save audit log");
                }
                return BadRequest("Could not find document");
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while creating document: {document}";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest();
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
        [HttpPost("CopyDocument")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentWithoutRevisionsDto>> CopyDocument(
            Guid matterId,
            DocumentWithoutRevisionsDto document)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                Matter? matterForCopiedDocument = await _admsRepository.GetMatterAsync(matterId, false);

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
/*
                    RevisionDto revisionToAdd = new()
                    {
                        RevisionId = 1,
                        CreationDate = DateTime.Now.ToUniversalTime(),
                        ModificationDate = DateTime.Now.ToUniversalTime(),
                    };

                    await _admsRepository.AddRevisionAsync(finalDocument.Id, revisionToAdd);
*/
                    return await _admsRepository.SaveChangesAsync() ? Ok(Task.FromResult(true)) : Ok(Task.FromResult(false));
                }
                return BadRequest("Could not find document");
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while copying document: {document}";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest();
            }
        }

        /// <summary>
        /// retrieve matter document activity user from list
        /// </summary>
        /// <param name="id">Matter Id to retrieve</param>
        /// <returns>list of matter document activity users</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter not found</response>
        [HttpGet("{id}/GetMDAUFromHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMDAUFromHistoryAsync(
            Guid id)
        {
            try
            {
                if (!await _admsRepository.CheckDocumentExistsAsync(id))
                {
                    return NotFound("Document not found");
                }

                return Ok(await _admsRepository.GetMDAUFromHistoryAsync(Guid.Empty, id));
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while retrieving Document with id: {{id}}";
                _logger.LogCritical(errorMessage, exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// retrieve matter document activity user to list
        /// </summary>
        /// <param name="id">Matter Id to retrieve</param>
        /// <returns>list of matter document activity users</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter not found</response>
        [HttpGet("{id}/GetMDAUToHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMDAUToHistoryAsync(
            Guid id)
        {
            try
            {
                if (!await _admsRepository.CheckDocumentExistsAsync(id))
                {
                    return NotFound("Document not found");
                }

                return Ok(await _admsRepository.GetMDAUToHistoryAsync(Guid.Empty, id));
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while retrieving Document with id: {{id}}";
                _logger.LogCritical(errorMessage, exception);
                return BadRequest("An error occured");
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
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

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
                string errorMessage = $"An error occured while creating document: {document}";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest();
            }
        }

        /// <summary>
        /// Delete specified document Id
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="id">document to be deleted</param>
        /// <returns>IActionResult</returns>
        /// <response code="204">Returns No Content</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Document not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDocument(
            Guid matterId,
            Guid id)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }
                if (!await _admsRepository.CheckDocumentExistsAsync(id))
                {
                    return NotFound("Document not found");
                }

                Document? document = await _admsRepository.GetDocumentAsync(id, false);
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
                string errorMessage = $"An error occured while retrieving Document with id: {id}";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// Get specific Document
        /// </summary>
        /// <param name="matterId">matter containing document(s)</param>
        /// <param name="id">Document Id to retrieve</param>
        /// <param name="includeRevisions">Whether to include revisions with the returned data</param>
        /// <returns>An IActionResult</returns>
        /// <response code="200">Returns the requestedDocument</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Document not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocument(
            Guid matterId,
            Guid id,
            bool includeRevisions = false)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.CheckDocumentExistsAsync(id))
                {
                    return NotFound("Document not found");
                }

                Document? document = await _admsRepository.GetDocumentAsync(id, includeRevisions);
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
                string errorMessage = $"An error occured while retrieving Document with id: {id}";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// Check out document
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="id">document id to be checked out</param>
        /// <returns>IActionResult</returns>
        [HttpGet("{id}/checkout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckoutDocument(
            Guid matterId,
            Guid id)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.CheckDocumentExistsAsync(id))
                {
                    return NotFound("Document not found");
                }

                return await _admsRepository.CheckoutDocumentAsync(id) ? Ok() : BadRequest("Cannot check out document");
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while checking out document with id: {id}";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// Check in document
        /// </summary>
        /// <param name="matterId">Matter document belongs to</param>
        /// <param name="id">document id to be checked in</param>
        /// <returns>IActionResult</returns>
        [HttpGet("{id}/checkin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckinDocument(
            Guid matterId,
            Guid id)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.CheckDocumentExistsAsync(id))
                {
                    return NotFound("Document not found");
                }

                return await _admsRepository.CheckinDocumentAsync(id) ? Ok() : BadRequest("Cannot check in document");
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while checking in document with id: {id}";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// gets specific document history
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="id">Document Id to retrieve</param>
        /// <returns>document history</returns>
        /// <response code="200">Returns the requested document history</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter or document not found</response>
        [HttpGet("{id}/GetHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocumentHistoryAsync(
            Guid matterId,
            Guid id)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.CheckDocumentExistsAsync(id))
                {
                    return NotFound("Document not found");
                }

                IEnumerable<DocumentActivityUser> documentHistory = await _admsRepository.GetDocumentWithHistoryByIdAsync(id);

                IEnumerable<DocumentActivityUserDto> results = _mapper.Map<IEnumerable<DocumentActivityUserDto>>(documentHistory);

                return Ok(results);
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while retrieving document history with id: {id}";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest("An error occured");
            }
        }

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
        [HttpGet]
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
                string errorMessage = $"An error occured while retrieving Documents";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// Updated document with nerw changes
        /// </summary>
        /// <param name="matterId">Matter document belongs to</param>
        /// <param name="id">document id to be patched</param>
        /// <param name="patchDocument">updates to be applied</param>
        /// <returns>IACtionResult</returns>
        /// <response code="204">Returns No Content</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Document not found</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PartiallyUpdateDocument(
            Guid matterId,
            Guid id,
            JsonPatchDocument<DocumentDto> patchDocument)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.CheckDocumentExistsAsync(id))
                {
                    return NotFound("Document not found");
                }

                Document? documentEntity = await _admsRepository.GetDocumentAsync(id, true);
                if (documentEntity == null)
                {
                    return NotFound("Document not found");
                }

                DocumentDto documentToPatch = _mapper.Map<DocumentDto>(documentEntity);

                patchDocument.ApplyTo(documentToPatch, ModelState);

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
                            CreatedAt = DateTime.Now.ToUniversalTime(),
                        });
                }

                return await _admsRepository.SaveChangesAsync() ? NoContent() : BadRequest("Could not save updates");
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while retrieving Document with id: {id}";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// Updates document details
        /// </summary>
        /// <param name="matterId">Matter document belongs to</param>
        /// <param name="id">document id to be updated</param>
        /// <param name="document">document details in need of updating</param>
        /// <returns>IActionResult</returns>
        /// <response code="204">Returns No Content</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Document not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDocument(
            Guid matterId,
            Guid id,
            DocumentForUpdateDto document)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.CheckDocumentExistsAsync(id))
                {
                    return NotFound("Document not found");
                }

                Document? documentEntity = await _admsRepository.GetDocumentAsync(id, false);
                if (documentEntity == null)
                {
                    return NotFound("Document not found");
                }

                _mapper.Map(document, documentEntity);

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

                return await _admsRepository.SaveChangesAsync() ? NoContent() : BadRequest("Could not save updates");
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while updating Document with id: {id}";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest("An error occured");
            }
        }

        #endregion Actions
    }
}