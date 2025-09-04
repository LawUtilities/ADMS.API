using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.Services;

using Asp.Versioning;

using AutoMapper;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Syncfusion.DocIO.DLS;
using Syncfusion.Presentation;
using Syncfusion.XlsIO;

using Document = ADMS.API.Entities.Document;

namespace ADMS.API.Controllers
{
    /// <summary>
    /// Matter actions
    /// </summary>
    [ApiController]
    [Asp.Versioning.ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/matters")]
    public class MatterController : ControllerBase
    {
        private readonly ILogger<MatterController> _logger;
        private readonly IAdmsRepository _admsRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Matter Controller Constructor
        /// </summary>
        /// <param name="logger">logger to be used by this controller</param>
        /// <param name="admsRepository">repository to use</param>
        /// <param name="mapper">atomapper to use</param>
        public MatterController(
            ILogger<MatterController> logger,
            IAdmsRepository admsRepository,
            IMapper mapper)
        {
            _logger = logger;
            _admsRepository = admsRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a matter
        /// </summary>
        /// <param name="matter">matter to add</param>
        /// <returns></returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter not found</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MatterDto>> CreateMatter(
            MatterForCreationDto matter)
        {
            try
            {
                if (await _admsRepository.CheckMatterExists(matter.Description))
                {
                    return BadRequest("Matter already exists");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                MatterDto finalMatter = _mapper.Map<MatterDto>(matter);

                if (!TryValidateModel(finalMatter))
                {
                    return BadRequest(ModelState);
                }

                Matter? createdMatter = await _admsRepository.AddMatterAsync(finalMatter);

                if (createdMatter != null)
                {
                    return Ok(createdMatter);
                }
                return NotFound("Could not save matter");
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while creating matter: {matter.Description}";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// Delete specified matter
        /// </summary>
        /// <param name="matterId">matter to be deleted</param>
        /// <returns></returns>
        /// <response code="204">Returns No Content</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter not found</response>
        [HttpDelete("{matterId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMatter(
            Guid matterId)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                Matter? matter = await _admsRepository.GetMatterAsync(matterId, false);
                if (matter != null && await _admsRepository.DeleteMatterAsync(_mapper.Map<MatterDto>(matter)))
                {
                    return Ok(Task.FromResult(true));
                }
                else
                {
                    return BadRequest("Could not delete matter");
                }
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while deleting matter with id: {matterId}";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// gets specific matter
        /// </summary>
        /// <param name="matterId">Matter Id to retrieve</param>
        /// <param name="includeDocuments">Include documents in returned matter</param>
        /// <returns>list of matter</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter not found</response>
        [HttpGet("{matterId}", Name = "GetMatter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMatterAsync(
            Guid matterId,
            bool includeDocuments = false)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                Matter? matter = await _admsRepository.GetMatterAsync(matterId, includeDocuments);
                if (includeDocuments)
                {
                    return Ok(_mapper.Map<MatterWithDocumentsDto>(matter));
                }
                return Ok(_mapper.Map<MatterWithoutDocumentsDto>(matter));
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while retrieving Matter with id: {{matterId}}";
                _logger.LogCritical(errorMessage, exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// retrieve matter document activity user from list
        /// </summary>
        /// <param name="matterId">Matter Id to retrieve</param>
        /// <returns>list of matter document activity users</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter not found</response>
        [HttpGet("{matterId}/GetMDAUFromHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMDAUFromHistoryAsync(
            Guid matterId)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                return Ok(await _admsRepository.GetMDAUFromHistoryAsync(matterId, Guid.Empty));
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while retrieving Matter with id: {{matterId}}";
                _logger.LogCritical(errorMessage, exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// retrieve matter document activity user to list
        /// </summary>
        /// <param name="matterId">Matter Id to retrieve</param>
        /// <returns>list of matter document activity users</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter not found</response>
        [HttpGet("{matterId}/GetMDAUToHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMDAUToHistoryAsync(
            Guid matterId)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                return Ok(await _admsRepository.GetMDAUToHistoryAsync(matterId, Guid.Empty));
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while retrieving Matter with id: {{matterId}}";
                _logger.LogCritical(errorMessage, exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// Check if Matter History Exists
        /// </summary>
        /// <param name="matterId">Matter Id to retrieve</param>
        /// <returns>True if Matter History exists, false otherwise</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter not found</response>
        [HttpGet("{matterId}/MatterHistoryExists")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MatterHistoryExists(
            Guid matterId)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                bool result = await _admsRepository.DoesMatterHistoryExists(matterId);

                return Ok(new JsonResult(result));
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while retrieving Matter history with id: {{matterId}}";
                _logger.LogCritical(errorMessage, exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// gets specific matter history
        /// </summary>
        /// <param name="matterId">Matter to retrieve</param>
        /// <returns>Matter history</returns>
        /// <response code="200">Returns the requested matter history</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter or document not found</response>
        [HttpGet("{matterId}/GetHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMatterHistoryAsync(
            Guid matterId)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                Matter matterWithHistory = await _admsRepository.GetMatterWithHistoryByIdAsync(matterId);
                matterWithHistory.MatterActivityUsers.OrderByDescending(mau => mau.CreatedAt);

                return Ok(_mapper.Map<MatterWithoutDocumentsDto>(matterWithHistory));
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while retrieving document history with id: {matterId}";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// undelete specific matter
        /// </summary>
        /// <param name="matterId">Matter Id to restore</param>
        /// <returns>true if restored, false otherwise</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter not found</response>
        [HttpGet("{matterId}/RestoreMatter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreMatterAsync(
            Guid matterId)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                var restorationResult = await _admsRepository.RestoreMatterAsync(matterId);
                return Ok(restorationResult);
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while retrieving Matter with id: {{matterId}}";
                _logger.LogCritical(errorMessage, exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// gets list of matters
        /// </summary>
        /// <param name="description">matter description</param>
        /// <param name="includeArchived">include archived matters</param>
        /// <param name="includeDeleted">include deleted matters</param>
        /// <returns>list of matters not including documents</returns>
        /// <response code="200">Returns the requested matters</response>
        /// <response code="400">An error occured</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMattersAsync(
            string description,
            bool includeArchived = false,
            bool includeDeleted = false)
        {
            try
            {
                IEnumerable<Matter> matters = await _admsRepository.GetMattersAsync(description, includeArchived, includeDeleted);
                return Ok(_mapper.Map<IEnumerable<MatterWithoutDocumentsDto>>(matters));
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while retrieving Matters";
                _logger.LogCritical(errorMessage, exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// Updates matter details
        /// </summary>
        /// <param name="matterId">Matter Id</param>
        /// <param name="matter">matter to be updated</param>
        /// <returns>IActionResult</returns>
        /// <response code="204">Returns No Content</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Matter not found</response>
        [HttpPut("{matterId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMatter(
            Guid matterId,
            MatterForUpdateDto matter)
        {
            try
            {
                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }
                Matter? matterEntity = await _admsRepository.GetMatterAsync(matterId, false);

                _mapper.Map(matter, matterEntity);

                MatterActivity? matterActivity = await _admsRepository.GetMatterActivityByActivity("SAVED");
                // TODO: replace hard coded get user call with relevant authenticated user name
                User? user = await _admsRepository.GetUserByUsername("rbrown");

                if (matterEntity != null && matterActivity != null && user != null)
                {
                    matterEntity.MatterActivityUsers.Add(
                        new MatterActivityUser()
                        {
                            Matter = matterEntity,
                            MatterId = matterEntity.Id,
                            MatterActivity = matterActivity,
                            MatterActivityId = matterActivity.Id,
                            User = user,
                            UserId = user.Id,
                            CreatedAt = DateTime.Now.ToUniversalTime(),
                        });
                }

                return await _admsRepository.SaveChangesAsync() ? NoContent() : BadRequest("Could not update matter");
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while updating Matter with id: {matterId}";
                _logger.LogCritical(message: errorMessage, args: exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// Upload a new file to the ADMS system
        /// </summary>
        /// <param name="matterId">Matter to save the document to</param>
        /// <param name="fileUpload">file to be uploaded</param>
        /// <param name="cancelToken">cancellation token</param>
        /// <returns>IActionResult</returns>
        /// <response code="200">Document uploaded</response>
        /// <response code="400">An error occured</response>
        /// <response code="404">Document not found</response>
        [HttpPost("{matterId}/uploadNewFile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadNewFileAsync(
            Guid matterId,
            IFormFile fileUpload,
            CancellationToken cancelToken)
        {
            try
            {
                if (fileUpload.Length == 0)
                    return BadRequest("File does not exist");

                if (!await _admsRepository.CheckMatterExists(matterId))
                {
                    return NotFound("Matter not found");
                }

                Matter? matter = await _admsRepository.GetMatterAsync(matterId, false);

                if (matter == null || matter.Id == Guid.Empty)
                    return BadRequest($"Could not retrieve matter with id: {matterId}");

                FileInfo? fileInfo = new(fileUpload.FileName);
                string fileExtension = fileInfo.Extension.ToLower();

                DocumentForCreationDto documentToCreate = new()
                {
                    FileName = fileInfo.Name,
                    Extension = fileInfo.Extension,
                    IsCheckedOut = false
                };

                Document? newDocument = await _admsRepository.AddDocumentAsync(matterId, documentToCreate);

                if (newDocument == null || newDocument.Id == Guid.Empty)
                {
                    return BadRequest("Unable to create document");
                }

                Entities.Revision? newRevision = await _admsRepository.AddRevisionAsync(newDocument.Id, new RevisionDto()
                {
                    CreationDate = fileInfo.CreationTimeUtc,
                    ModificationDate = fileInfo.LastWriteTimeUtc,
                    IsDeleted = false,
                    RevisionId = 1
                });

                if (newRevision == null || newRevision.Id == Guid.Empty)
                {
                    return BadRequest("Unable to create revision");
                }

                if (!await _admsRepository.SaveChangesAsync())
                {
                    return BadRequest("Unable to create document");
                }

                string fileName = $"{newDocument.Id}R{newRevision.RevisionId}{fileInfo.Extension}";

                string folderName = Path.Combine("matters", matter.Id.ToString());
                string pathToSave = Path.Combine(@"C:\Dev\Repos\ADMSServerFiles", folderName);
                string fullPath = Path.Combine(pathToSave, $"{fileName}");

                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }

                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);

                string tempFile = Path.Combine(pathToSave, $"{newDocument.Id}R{newRevision.RevisionId}_123{fileInfo.Extension}");
                using (FileStream stream = new(tempFile, FileMode.Create))
                {
                    await fileUpload.CopyToAsync(stream, cancelToken);
                }
                fileInfo = null;
                switch (fileExtension)
                {
                    case ".doc":
                    case ".docm":
                    case ".docx":
                    case ".dot":
                    case ".dotm":
                    case ".dotx":
                        //Add Word properties
                        AddCustomDocumentProperties(tempFile, fullPath, newDocument.Id, newRevision.Id, matter.Id, DocumentType.Word);
                        System.IO.File.Move(tempFile, fullPath);
                        break;
                    case ".csv":
                    case ".xls":
                    case ".xlsx":
                    case ".xlt":
                    case ".xltm":
                    case ".xlw":
                        //Add Excel properties
                        AddCustomDocumentProperties(tempFile, fullPath, newDocument.Id, newRevision.Id, matter.Id, DocumentType.Excel);
                        System.IO.File.Delete(tempFile);
                        break;
                    case ".potx":
                    case ".pptm":
                    case ".pptx":
                        //Add PowerPoint properties
                        AddCustomDocumentProperties(tempFile, fullPath, newDocument.Id, newRevision.Id, matter.Id, DocumentType.PowerPoint);
                        System.IO.File.Move(tempFile, fullPath);
                        break;
                    default:
                        break;
                }
                return Ok();
            }
            catch (Exception exception)
            {
                _logger.LogCritical("An error occurred while uploading document: {Exception}", exception);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Adds custom document properties to various office documents
        /// </summary>
        /// <param name="tempFile">temporary file used to add properties</param>
        /// <param name="fileName">final filename to save file to</param>
        /// <param name="documentId">document Id to add</param>
        /// <param name="revisionId">revision Id to add</param>
        /// <param name="matterId">matter Id to add</param>
        /// <param name="docType">type of document to add properties to</param>
        private void AddCustomDocumentProperties(
            string tempFile, 
            string fileName, 
            Guid documentId, 
            Guid revisionId, 
            Guid matterId, 
            DocumentType docType)
        {
            try
            {
                string fileExtension = Path.GetExtension(tempFile);

                switch (docType)
                {
                    case DocumentType.Word:
                        if (wordExtensionsToFormatType.TryGetValue(fileExtension, out Syncfusion.DocIO.FormatType wordFormatType))
                        {
                            using (FileStream docStream = new(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                            {
                                using (WordDocument wordDocument = new(docStream, wordFormatType))
                                {
                                    AddOrUpdateDocumentProperty(wordDocument, "ADMS.DocumentID", documentId.ToString(), docType);
                                    AddOrUpdateDocumentProperty(wordDocument, "ADMS.RevisionID", revisionId.ToString(), docType);
                                    AddOrUpdateDocumentProperty(wordDocument, "ADMS.MatterID", matterId.ToString(), docType);
                                    wordDocument.Save(docStream, wordFormatType);
                                }
                            }
                        }
                        break;

                    case DocumentType.Excel:
                        using (ExcelEngine excelEngine = new())
                        {
                            IApplication application = excelEngine.Excel;
                            using (FileStream inputStream = new(tempFile, FileMode.Open, FileAccess.Read))
                            {
                                IWorkbook workbook = application.Workbooks.Open(inputStream);
                                if (excelExtensionsToFormatType.TryGetValue(fileExtension, out ExcelVersion excelFormatType))
                                {
                                    workbook.Version = excelFormatType;
                                }

                                AddOrUpdateDocumentProperty(workbook, "ADMS.DocumentID", documentId.ToString(), docType);
                                AddOrUpdateDocumentProperty(workbook, "ADMS.RevisionID", revisionId.ToString(), docType);
                                AddOrUpdateDocumentProperty(workbook, "ADMS.MatterID", matterId.ToString(), docType);

                                using (FileStream fileStream = new(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                                {
                                    workbook.SaveAs(fileStream);
                                    workbook.Close();
                                }
                            }
                        }
                        break;
                    case DocumentType.PowerPoint:
                        using (FileStream inputStream = new(tempFile, FileMode.Open, FileAccess.ReadWrite))
                        {
                            using (IPresentation pptxDoc = Presentation.Open(inputStream))
                            {
                                AddOrUpdateDocumentProperty(pptxDoc, "ADMS.DocumentID", documentId.ToString(), docType);
                                AddOrUpdateDocumentProperty(pptxDoc, "ADMS.RevisionID", revisionId.ToString(), docType);
                                AddOrUpdateDocumentProperty(pptxDoc, "ADMS.MatterID", matterId.ToString(), docType);
                                pptxDoc.Save(inputStream);
                            }
                        }
                        break;
                }
            }
            catch (IOException ioException)
            {
                _logger.LogError(ioException, "An IO exception occurred during conversion: {message}", ioException.Message);
            }
            catch (Exception exception)
            {
                _logger.LogCritical("An error occurred while uploading document: {Exception}", exception);
            }
        }

        /// <summary>
        /// Add or update individual document property
        /// </summary>
        /// <param name="document">document to add property to</param>
        /// <param name="propertyName">specific property to add</param>
        /// <param name="propertyValue">specific value to add</param>
        /// <param name="docType">type of document to add property to</param>
        private void AddOrUpdateDocumentProperty(
            dynamic document, 
            string propertyName, 
            string propertyValue, 
            DocumentType docType)
        {
            try
            {
                SetDocumentProperty(document, propertyName, propertyValue, docType);
            }
            catch (Exception exception)
            {
                _logger.LogCritical("An error occurred while adding document properties: {Exception}", exception);
            }
        }

        /// <summary>
        /// Sets specific document property
        /// </summary>
        /// <param name="document">document to add property to</param>
        /// <param name="propertyName">specific property to add</param>
        /// <param name="propertyValue">specific value to add</param>
        /// <param name="docType">type of document to add property to</param>
        private void SetDocumentProperty(
            dynamic document, 
            string propertyName, 
            string propertyValue, 
            DocumentType docType)
        {
            try
            {
                switch (docType)
                {
                    case DocumentType.Word:
                        document.CustomDocumentProperties.Add(propertyName, propertyValue);
                        break;
                    case DocumentType.PowerPoint:
                        Syncfusion.Presentation.ICustomDocumentProperties docProps = document.CustomDocumentProperties;
                        if (!docProps.Contains(propertyName))
                        {
                            docProps.Add(propertyName);
                            docProps[propertyName].Value = propertyValue;
                        }
                        break;
                    default:
                        document.CustomDocumentProperties[propertyName].Text = propertyValue;
                        break;
                }
            }
            catch (Exception exception)
            {
                _logger.LogCritical("An error occurred while setting document properties: {Exception}", exception);
            }
        }

        /// <summary>
        /// Word extension to Syncfusion format
        /// </summary>
        private static readonly Dictionary<string, Syncfusion.DocIO.FormatType> wordExtensionsToFormatType = new()
        {
            {".doc", Syncfusion.DocIO.FormatType.Doc},
            {".docm", Syncfusion.DocIO.FormatType.Docm},
            {".docx", Syncfusion.DocIO.FormatType.Docx},
            {".dot", Syncfusion.DocIO.FormatType.Dot},
            {".dotm", Syncfusion.DocIO.FormatType.Dotm},
            {".dotx", Syncfusion.DocIO.FormatType.Dotx},
            {".rtf", Syncfusion.DocIO.FormatType.Rtf}
        };

        /// <summary>
        /// Excel extension to Syncfusion format
        /// </summary>
        private static readonly Dictionary<string, ExcelVersion> excelExtensionsToFormatType = new()
        {
            {".xls", ExcelVersion.Excel97to2003},
            {".xlsx", ExcelVersion.Xlsx}
        };

        /// <summary>
        /// Office document types
        /// </summary>
        private enum DocumentType
        {
            Word,
            Excel,
            PowerPoint
        }
    }
}
