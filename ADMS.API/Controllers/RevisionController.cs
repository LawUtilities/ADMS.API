using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.Services;

using Asp.Versioning;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;

using Syncfusion.DocIO.DLS;

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
    [ApiVersion("1.0")]
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
        /// <param name="revision">Revision to add</param>
        /// <returns>ActionResult of RevisionDto</returns>
        /// <response code="200">Returns the created revision</response>
        /// <response code="400">An error occurred</response>
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
                if (revision == null)
                {
                    return BadRequest("Revision cannot be null");
                }

                if (!await _admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.DocumentExistsAsync(documentId))
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

                ADMS.API.Entities.Revision? createdRevision = await _admsRepository.AddRevisionAsync(documentId, finalRevision);

                return await _admsRepository.SaveChangesAsync() ?
                    (ActionResult<RevisionDto>)Ok(createdRevision) :
                    (ActionResult<RevisionDto>)BadRequest("Could not create revision");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "An error occurred while creating revision for matter {MatterId} and document {DocumentId}", matterId, documentId);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Delete specified revision
        /// </summary>
        /// <param name="matterId">Matter containing the document in question</param>
        /// <param name="documentId">Document containing revision to be deleted</param>
        /// <param name="revisionId">Revision to be deleted</param>
        /// <returns>IActionResult</returns>
        /// <response code="204">Deletion undertaken</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Matter, Document or Revision not found</response>
        [HttpDelete("{revisionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRevision(
            Guid matterId,
            Guid documentId,
            Guid revisionId)
        {
            try
            {
                if (!await _admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.DocumentExistsAsync(documentId))
                {
                    return NotFound("Document not found");
                }

                if (!await _admsRepository.RevisionExistsAsync(revisionId))
                {
                    return NotFound("Revision not found");
                }

                Entities.Revision? revision = await _admsRepository.GetRevisionByIdAsync(revisionId);

                if (revision == null)
                {
                    return NotFound("Revision not found");
                }
                else
                {

                    if (await _admsRepository.DeleteRevisionAsync(_mapper.Map<RevisionDto>(revision)))
                    {
                        return await _admsRepository.SaveChangesAsync() ? NoContent() : BadRequest("Unable to delete revision");
                    }
                    else
                    {
                        return BadRequest("Unable to delete revision");
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "An error occurred while deleting revision with id: {RevisionId} for document {DocumentId} in matter {MatterId}", revisionId, documentId, matterId);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Gets specific revision
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">Document containing revision</param>
        /// <param name="revisionId">Revision Id to retrieve</param>
        /// <returns>Revision to be retrieved</returns>
        /// <response code="200">Returns the requested revision</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Matter, document or revision not found</response>
        [HttpGet("{revisionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRevisionAsync(
            Guid matterId,
            Guid documentId,
            Guid revisionId)
        {
            try
            {
                if (!await _admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.DocumentExistsAsync(documentId))
                {
                    return NotFound("Document not found");
                }

                if (!await _admsRepository.RevisionExistsAsync(revisionId))
                {
                    return NotFound("Revision not found");
                }

                Entities.Revision? revision = await _admsRepository.GetRevisionByIdAsync(revisionId);
                if (revision == null)
                {
                    return NotFound("Revision not found");
                }

                return Ok(_mapper.Map<RevisionDto>(revision));
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "An error occurred while retrieving revision with id: {RevisionId} for document {DocumentId} in matter {MatterId}", revisionId, documentId, matterId);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Gets specific revision history
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">Document containing revision</param>
        /// <param name="revisionId">Revision Id to retrieve</param>
        /// <returns>Revision history to be retrieved</returns>
        /// <response code="200">Returns the requested revision history</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Matter, document or revision not found</response>
        [HttpGet("{revisionId}/GetHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRevisionHistoryAsync(
            Guid matterId,
            Guid documentId,
            Guid revisionId)
        {
            try
            {
                if (!await _admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.DocumentExistsAsync(documentId))
                {
                    return NotFound("Document not found");
                }

                if (!await _admsRepository.RevisionExistsAsync(revisionId))
                {
                    return NotFound("Revision not found");
                }

                IEnumerable<RevisionActivityUser> revisionHistory = await _admsRepository.GetRevisionWithHistoryByIdAsync(revisionId);
                if (revisionHistory == null || !revisionHistory.Any())
                {
                    return NotFound("Revision history not found");
                }

                IEnumerable<RevisionActivityUserDto> results = _mapper.Map<IEnumerable<RevisionActivityUserDto>>(revisionHistory);

                return Ok(results);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "An error occurred while retrieving revision history with id: {RevisionId} for document {DocumentId} in matter {MatterId}", revisionId, documentId, matterId);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Gets list of revisions
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">Document to retrieve revisions for</param>
        /// <param name="includeDeleted">Include deleted revisions</param>
        /// <returns>List of revisions</returns>
        /// <response code="200">Returns the requested revisions</response>
        /// <response code="400">An error occurred</response>
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
                if (!await _admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.DocumentExistsAsync(documentId))
                {
                    return NotFound("Document not found");
                }

                IEnumerable<Entities.Revision> revisions = await _admsRepository.GetRevisionsAsync(documentId, includeDeleted);
                if (revisions == null || !revisions.Any())
                {
                    return NotFound("No revisions found");
                }

                return Ok(_mapper.Map<IEnumerable<RevisionDto>>(revisions));
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "An error occurred while retrieving revisions for document with id: {DocumentId} in matter {MatterId}", documentId, matterId);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Updates revision details
        /// </summary>
        /// <param name="matterId">Matter containing document</param>
        /// <param name="documentId">Document containing revision</param>
        /// <param name="revisionId">Revision Id</param>
        /// <param name="revision">Revision to be updated</param>
        /// <returns>IActionResult</returns>
        /// <response code="204">Returns No Content</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Matter, Document or Revision not found</response>
        [HttpPut("{revisionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRevision(
            Guid matterId,
            Guid documentId,
            Guid revisionId,
            RevisionForUpdateDto revision)
        {
            try
            {
                if (revision == null)
                {
                    return BadRequest("Revision cannot be null");
                }

                if (!await _admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                if (!await _admsRepository.DocumentExistsAsync(documentId))
                {
                    return NotFound("Document not found");
                }

                if (!await _admsRepository.RevisionExistsAsync(revisionId))
                {
                    return NotFound("Revision not found");
                }

                Entities.Revision? revisionEntity = await _admsRepository.GetRevisionByIdAsync(revisionId);
                if (revisionEntity == null)
                {
                    return NotFound("Revision not found");
                }

                _mapper.Map(revision, revisionEntity);

                Entities.Revision? resultantOp = await _admsRepository.UpdateRevisionAsync(matterId, documentId, revisionId, _mapper.Map<RevisionDto>(revisionEntity));

                return await _admsRepository.SaveChangesAsync() ? NoContent() : BadRequest("Could not save updates");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "An error occurred while updating Revision with revisionId: {RevisionId} for document {DocumentId} in matter {MatterId}", revisionId, documentId, matterId);
                return BadRequest("An error occurred");
            }
        }
    }
}
