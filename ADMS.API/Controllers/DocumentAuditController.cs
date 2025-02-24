using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.Services;

using Asp.Versioning;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ADMS.API.Controllers
{
    /// <summary>
    /// Document Audit actions
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/matters/{matterId}/documents/{documentId}/audits")]
    public class DocumentAuditController(
        ILogger<DocumentAuditController> logger,
        IAdmsRepository admsRepository,
        IMapper mapper,
        ProblemDetailsFactory problemDetailsFactory) : Controller
    {
        private readonly ILogger<DocumentAuditController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IAdmsRepository _admsRepository = admsRepository ?? throw new ArgumentNullException(nameof(admsRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));

        #region Actions

        /// <summary>
        /// Creates Audit record for Document
        /// </summary>
        /// <param name="matterId">matter document belongs to</param>
        /// <param name="documentId">document to create audit record for</param>
        /// <param name="audit">audit record to create</param>
        /// <returns>DocumentActivityUser record</returns>
        [HttpPost(Name = "CreateDocumentAudit")]
        public async Task<ActionResult<DocumentActivityUserDto>> CreateDocumentAudit(
            Guid matterId,
            Guid documentId,
            DocumentActivityUserDto audit)
        {
            if (matterId == Guid.Empty || documentId == Guid.Empty)
            {
                return NotFound();
            }

            if (audit == null)
            {
                return BadRequest("Audit record cannot be null.");
            }

            try
            {
                DocumentActivityUser documentAuditEntity = _mapper.Map<DocumentActivityUser>(audit);

                await _admsRepository.AddDocumentAuditAsync(documentAuditEntity);
                await _admsRepository.SaveChangesAsync();

                var documentAuditToReturn = _mapper.Map<DocumentActivityUserDto>(documentAuditEntity);

                return CreatedAtRoute("GetDocumentAudits",
                    new { matterId, documentId },
                    documentAuditToReturn);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "A database error occurred while creating audit history for document {DocumentId} in matter {MatterId}.", documentId, matterId);
                return StatusCode(500, "A database error occurred while creating the audit history.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An error occurred while creating audit history for document {DocumentId} in matter {MatterId}.", documentId, matterId);
                return BadRequest(
                    _problemDetailsFactory.CreateProblemDetails(HttpContext,
                    statusCode: 400,
                    detail: $"An error occurred while creating audit history: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get Audits for specified document
        /// </summary>
        /// <param name="matterId">matter document belongs to</param>
        /// <param name="documentId">document to get audit records for</param>
        /// <returns>DocumentActivityUser record</returns>
        [HttpGet(Name = "GetDocumentAudits")]
        public async Task<IActionResult> GetDocumentAudits(
            Guid matterId,
            Guid documentId)
        {
            if (matterId == Guid.Empty || documentId == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                DocumentAuditDto audits = new()
                {
                    DocumentActivityUserAudit = await _admsRepository.GetDocumentAuditsAsync(documentId),
                    MatterDocumentActivityUserFromAudit = await _admsRepository.GetExtendedAuditsAsync(Guid.Empty, documentId, "from"),
                    MatterDocumentActivityUserToAudit = await _admsRepository.GetExtendedAuditsAsync(Guid.Empty, documentId, "to")
                };
                return Ok(audits);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "A database error occurred while retrieving audit history for document {DocumentId} in matter {MatterId}.", documentId, matterId);
                return StatusCode(500, "A database error occurred while retrieving the audit history.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An error occurred while retrieving audit history for document {DocumentId} in matter {MatterId}.", documentId, matterId);
                return BadRequest(
                    _problemDetailsFactory.CreateProblemDetails(HttpContext,
                    statusCode: 400,
                    detail: $"An error occurred while retrieving audit history: {ex.Message}"));
            }
        }

        #endregion Actions
    }
}
