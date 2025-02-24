using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.Services;

using Asp.Versioning;

using AutoMapper;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ADMS.API.Controllers
{
    /// <summary>
    /// Matter actions
    /// </summary>
    /// <remarks>
    /// Revision Controller Constructor
    /// </remarks>
    /// <param name="logger">logger to be used by this controller</param>
    /// <param name="admsRepository">repository to use</param>
    /// <param name="mapper">atomapper to use</param>
    [ApiController]
    [Asp.Versioning.ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/matters/{matterId}/documents/{documentId}/revisions")]
    public class RevisionController(
        ILogger<RevisionController> logger,
        IAdmsRepository admsRepository,
        IMapper mapper) : ControllerBase
    {
        private readonly ILogger<RevisionController> _logger = logger;
        private readonly IAdmsRepository _admsRepository = admsRepository;
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Creates a revision
        /// </summary>
        /// <param name="matterId">Matter to be added to</param>
        /// <param name="documentId">Document to add revision to</param>
        /// <param name="revision">revision to add</param>
        /// <returns></returns>
        /// <response code="200">Returns the created revision</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter or Document not found</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RevisionDto>> CreateRevision(
            Guid matterId, 
            Guid documentId, 
            RevisionForCreationDto revision)
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
                Document? documentForUpload = await _admsRepository.GetDocumentAsync(documentId, true);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                RevisionDto finalRevision = _mapper.Map<RevisionDto>(revision);
                if (documentForUpload != null)
                {
                    finalRevision.RevisionId = documentForUpload.Revisions.Count + 1;
                }

                if (!TryValidateModel(finalRevision))
                {
                    return BadRequest(ModelState);
                }

                Revision? createdRevision = await _admsRepository.AddRevisionAsync(documentId, finalRevision);

                return await _admsRepository.SaveChangesAsync() ?
                    (ActionResult<RevisionDto>)Ok(createdRevision) :
                    (ActionResult<RevisionDto>)BadRequest("Could not create revision");
            }
            catch (Exception exception)
            {
                if (exception == null)
                    return BadRequest("An error occured");
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    return BadRequest("An error occured");
                }
                _logger.LogCritical(exception: exception,
                                    message: "An error occured while creating revision: {revision}",
                                    args: [exception.StackTrace]);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// Delete specified revision
        /// </summary>
        /// <param name="matterId">Matter containing the docunment in question</param>
        /// <param name="documentId">document containing revision to be deleted</param>
        /// <param name="id">revision to be deleted</param>
        /// <returns></returns>
        /// <response code="204">Deletion undertaken</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter, Document or Revision not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRevision(
            Guid matterId,
            Guid documentId,
            Guid id)
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

                if (!await _admsRepository.CheckRevisionExistsAsync(id))
                {
                    return NotFound("Revision not found");
                }

                Revision? revision = await _admsRepository.GetRevisionByIdAsync(id);

                if (revision == null)
                {
                    return NotFound("Revision not found");
                }

                if (await _admsRepository.DeleteRevisionAsync(_mapper.Map<RevisionDto>(revision)))
                {
                    return await _admsRepository.SaveChangesAsync() ? NoContent() : BadRequest("Unable to delete revision");
                }
                else
                {
                    return BadRequest("Unable to delete revision");
                }
            }
            catch (Exception exception)
            {
                if (exception == null)
                    return BadRequest("An error occured");
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    return BadRequest("An error occured");
                }
                _logger.LogCritical(exception: exception,
                                    message: "An error occured while deleting revision with id: {id}",
                                    args: [exception.StackTrace]);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// gets specific revision
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">document containing revision</param>
        /// <param name="id">Revision Id to retrieve</param>
        /// <returns>revision to be retrieved</returns>
        /// <response code="200">Returns the requested matters</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter, document or revision not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRevisionAsync(
            Guid matterId,
            Guid documentId,
            Guid id)
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

                if (!await _admsRepository.CheckRevisionExistsAsync(id))
                {
                    return NotFound("Revision not found");
                }

                Revision? revision = await _admsRepository.GetRevisionByIdAsync(id);
                return Ok(_mapper.Map<RevisionDto>(revision));
            }
            catch (Exception exception)
            {
                if (exception == null)
                    return BadRequest("An error occured");
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    return BadRequest("An error occured");
                }
                _logger.LogCritical(exception: exception,
                                    message: "An error occured while retrieving revision with id: {id}",
                                    args: [exception.StackTrace]);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// gets specific revision history
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">document containing revision</param>
        /// <param name="id">Revision Id to retrieve</param>
        /// <returns>revision to be retrieved</returns>
        /// <response code="200">Returns the requested matters</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter, document or revision not found</response>
        [HttpGet("{id}/GetHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRevisionHistoryAsync(
            Guid matterId,
            Guid documentId,
            Guid id)
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

                if (!await _admsRepository.CheckRevisionExistsAsync(id))
                {
                    return NotFound("Revision not found");
                }

                IEnumerable<RevisionActivityUser> revisionHistory = await _admsRepository.GetRevisionWithHistoryByIdAsync(id);

                IEnumerable<RevisionActivityUserDto> results = _mapper.Map<IEnumerable<RevisionActivityUserDto>>(revisionHistory);

                return Ok(results);
            }
            catch (Exception exception)
            {
                if (exception == null)
                    return BadRequest("An error occured");
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    return BadRequest("An error occured");
                }
                _logger.LogCritical(exception: exception,
                                    message: "An error occured while retrieving revision history with id: {id}",
                                    args: [exception.StackTrace]);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// gets list of revisions
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">document to retrieve revisions for</param>
        /// <param name="includeDeleted">include deleted revisions</param>
        /// <returns>list of revisions</returns>
        /// <response code="200">Returns the requested revisions</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter or Document not found</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRevisionsAsync(
            Guid matterId,
            Guid documentId,
            bool includeDeleted = false)
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

                IEnumerable<Revision> revisions = await _admsRepository.GetRevisionsAsync(documentId, includeDeleted);
                return Ok(_mapper.Map<IEnumerable<RevisionDto>>(revisions));
            }
            catch (Exception exception)
            {
                if (exception == null)
                    return BadRequest("An error occured");
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    return BadRequest("An error occured");
                }
                _logger.LogCritical(exception: exception,
                                    message: "An error occured while retrieving revisions for document with id: {documentId}",
                                    args: [exception.StackTrace]);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// Updates revision details
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">Document containing revision</param>
        /// <param name="id">Revision Id</param>
        /// <param name="revision">revision to be updated</param>
        /// <returns>IActionResult</returns>
        /// <response code="204">Returns No Content</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter, Document or Revision not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRevision(
            Guid matterId,
            Guid documentId,
            Guid id, 
            RevisionForUpdateDto revision)
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

                if (!await _admsRepository.CheckRevisionExistsAsync(id))
                {
                    return NotFound("Revision not found");
                }

                Revision? revisionEntity = await _admsRepository.GetRevisionByIdAsync(id);
                if (revisionEntity == null)
                {
                    return NotFound("Revision not found");
                }

                _mapper.Map(revision, revisionEntity);

                Revision? resultantOp = await _admsRepository.UpdateRevisionsAsync(matterId, documentId, id, _mapper.Map<RevisionDto>(revisionEntity));

                return await _admsRepository.SaveChangesAsync() ? NoContent() : BadRequest("Could not save updates");
            }
            catch (Exception exception)
            {
                if (exception == null)
                    return BadRequest("An error occured");
                if (string.IsNullOrEmpty(exception.StackTrace))
                {
                    return BadRequest("An error occured");
                }
                _logger.LogCritical(exception: exception,
                                    message: "An error occured while updating Revision with revisionId: {id}",
                                    args: [exception.StackTrace]);
                return BadRequest("An error occured");
            }
        }
    }
}
