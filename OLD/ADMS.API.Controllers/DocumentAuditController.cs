using ADMS.API.ActionConstraints;
using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.Services;

using Asp.Versioning;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ADMS.API.Controllers
{
    /// <summary>
    /// Document Audit actions
    /// </summary>
    [ApiController]
    [Asp.Versioning.ApiVersion("1.0")]
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
        [RequestHeaderMatchesMediaType("Content-Type",
            "*/*",
            "application/json")]
        [Consumes("application/json")]
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
            try
            {
                DocumentActivityUser documentAuditEntity = _mapper.Map<DocumentActivityUser>(audit);

                _admsRepository.AddDocumentAudit(documentAuditEntity);
                await _admsRepository.SaveChangesAsync();

                var documentAuditToReturn = _mapper.Map<DocumentActivityUserDto>(documentAuditEntity);

                return CreatedAtRoute("GetDocumentAudits",
                    new { matterId, documentId },
                    documentAuditToReturn);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while creating audit history: {audit}",
                                    args: exception);
                return BadRequest(
                    _problemDetailsFactory.CreateProblemDetails(HttpContext,
                    statusCode: 400,
                    detail: $"An error occured while creating audit history: {audit}"));
            }
        }

        /// <summary>
        /// Get Audits for specified document
        /// </summary>
        /// <param name="matterId">matter document belongs to</param>
        /// <param name="documentId">document to get audit records for</param>
        /// <returns>DocumentActivityUser record</returns>
        [RequestHeaderMatchesMediaType("Accept",
            "*/*",
            "application/json")]
        [Produces("application/json")]
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
                // get document audit records from repo
                DocumentAuditDto audits = new();

                IEnumerable<DocumentActivityUser> documentAuditsFromRepo = await _admsRepository
                    .GetDocumentAuditsAsync(documentId);

                IEnumerable<MatterDocumentActivityUserFrom> matterFromAudit = await _admsRepository.GetMDAUFromHistoryAsync(Guid.Empty, documentId);
                IEnumerable<MatterDocumentActivityUserTo> matterToAudit = await _admsRepository.GetMDAUToHistoryAsync(Guid.Empty, documentId);

                if (documentAuditsFromRepo == null)
                {
                    return NotFound();
                }

                audits.DocumentActivityUserAudit = _mapper.Map<IEnumerable<DocumentActivityUserMinimalDto>>(documentAuditsFromRepo);
                audits.MatterDocumentActivityUserFromAudit = _mapper.Map<IEnumerable<MatterDocumentActivityUserMinimalDto>>(matterFromAudit);
                audits.MatterDocumentActivityUserToAudit = _mapper.Map<IEnumerable<MatterDocumentActivityUserMinimalDto>>(matterToAudit);

                return Ok(audits);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(message: "An error occured while retrieving audit history for document: {documentId}",
                                    args: exception);
                return BadRequest(
                    _problemDetailsFactory.CreateProblemDetails(HttpContext,
                    statusCode: 400,
                    detail: $"An error occured while retrieving audit history for document: {documentId}"));
            }
        }

        #endregion Actions
    }
}
