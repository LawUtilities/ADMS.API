using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.Services;

using Asp.Versioning;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;

using Syncfusion.DocIO.DLS;
using Syncfusion.Presentation;
using Syncfusion.XlsIO;

using Document = ADMS.API.Entities.Document;

namespace ADMS.API.Controllers
{
    /// <summary>
    /// Matter actions
    /// </summary>
    /// <remarks>
    /// Matter Controller Constructor
    /// </remarks>
    /// <param name="logger">logger to be used by this controller</param>
    /// <param name="admsRepository">repository to use</param>
    /// <param name="mapper">atomapper to use</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/matters")]
    public class MatterController(
        ILogger<MatterController> logger,
        IAdmsRepository admsRepository,
        IMapper mapper) : ControllerBase
    {
        /// <summary>
        /// Creates a matter
        /// </summary>
        /// <param name="matter">Matter to add</param>
        /// <returns>ActionResult of MatterDto</returns>
        /// <response code="200">Returns the created matter</response>
        /// <response code="400">An error occurred</response>
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
                if (matter == null)
                {
                    return BadRequest("Matter cannot be null");
                }

                if (await admsRepository.MatterNameExistsAsync(matter.Description))
                {
                    return BadRequest("Matter already exists");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                MatterDto finalMatter = mapper.Map<MatterDto>(matter);

                if (!TryValidateModel(finalMatter))
                {
                    return BadRequest(ModelState);
                }

                Matter? createdMatter = await admsRepository.AddMatterAsync(finalMatter);

                if (createdMatter != null)
                {
                    return Ok(createdMatter);
                }
                return NotFound("Could not save matter");
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "An error occurred while creating matter: {Description}", matter.Description);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Delete specified matter
        /// </summary>
        /// <param name="matterId">Matter to be deleted</param>
        /// <returns>IActionResult</returns>
        /// <response code="204">Returns No Content</response>
        /// <response code="400">An error occurred</response>
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
                if (!await admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                Matter? matter = await admsRepository.GetMatterAsync(matterId, false);
                if (matter != null && await admsRepository.DeleteMatterAsync(mapper.Map<MatterDto>(matter)))
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest("Could not delete matter");
                }
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "An error occurred while deleting matter with id: {MatterId}", matterId);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Gets specific matter
        /// </summary>
        /// <param name="matterId">Matter Id to retrieve</param>
        /// <param name="includeDocuments">Include documents in returned matter</param>
        /// <returns>ActionResult of Matter</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occurred</response>
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
                if (!await admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                Matter? matter = await admsRepository.GetMatterAsync(matterId, includeDocuments);
                if (matter == null)
                {
                    return NotFound("Matter not found");
                }

                if (includeDocuments)
                {
                    return Ok(mapper.Map<MatterWithDocumentsDto>(matter));
                }
                return Ok(mapper.Map<MatterWithoutDocumentsDto>(matter));
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "An error occurred while retrieving Matter with id: {MatterId}", matterId);
                return BadRequest("An error occurred");
            }
        }
        /// <summary>
        /// Retrieve matter document activity user history
        /// </summary>
        /// <param name="matterId">Matter Id to retrieve</param>
        /// <param name="historyType">Type of history to retrieve (from/to)</param>
        /// <returns>List of matter document activity users</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Matter not found</response>
        [HttpGet("{matterId}/GetAudits")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMDAUHistoryAsync(
            Guid matterId,
            string historyType)
        {
            try
            {
                if (!await admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                IEnumerable<MatterDocumentActivityUserMinimalDto> history;
                if (historyType.Equals("from", StringComparison.OrdinalIgnoreCase))
                {
//                    var result = await admsRepository.GetExtendedAuditsAsync(matterId, Guid.Empty, "from");
//                    history = mapper.Map<IEnumerable<MatterDocumentActivityUser>>(result);
                    history = await admsRepository.GetExtendedAuditsAsync(matterId, Guid.Empty, "from");
//                    history = mapper.Map<IEnumerable<MatterDocumentActivityUser>>(result);
                }
                else if (historyType.Equals("to", StringComparison.OrdinalIgnoreCase))
                {
//                    var result = await admsRepository.GetExtendedAuditsAsync(matterId, Guid.Empty, "to");
//                    history = mapper.Map<IEnumerable<MatterDocumentActivityUser>>(result);
                    history = await admsRepository.GetExtendedAuditsAsync(matterId, Guid.Empty, "to");
//                    history = mapper.Map<IEnumerable<MatterDocumentActivityUser>>(result);
                }
                else
                {
                    return BadRequest("Invalid history type specified");
                }

                return Ok(history);
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "An error occurred while retrieving Matter with id: {MatterId}", matterId);
                return BadRequest("An error occurred");
            }
        }

        /*
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
                if (!await admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                return Ok(await admsRepository.GetMDAUFromHistoryAsync(matterId, Guid.Empty));
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while retrieving Matter with id: {{matterId}}";
                logger.LogCritical(errorMessage, exception);
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
                if (!await admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                return Ok(await admsRepository.GetMDAUToHistoryAsync(matterId, Guid.Empty));
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while retrieving Matter with id: {{matterId}}";
                logger.LogCritical(errorMessage, exception);
                return BadRequest("An error occured");
            }
        }
        */

        /// <summary>
        /// Check if Matter History Exists
        /// </summary>
        /// <param name="matterId">Matter Id to retrieve</param>
        /// <returns>True if Matter History exists, false otherwise</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occurred</response>
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
                if (matterId == Guid.Empty)
                {
                    return BadRequest("Invalid matter ID");
                }

                if (!await admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                bool result = await admsRepository.DoesMatterHistoryExistAsync(matterId);

                return Ok(result);
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "An error occurred while retrieving Matter history with id: {MatterId}", matterId);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Gets specific matter history
        /// </summary>
        /// <param name="matterId">Matter to retrieve</param>
        /// <returns>Matter history</returns>
        /// <response code="200">Returns the requested matter history</response>
        /// <response code="400">An error occurred</response>
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
                if (matterId == Guid.Empty)
                {
                    return BadRequest("Invalid matter ID");
                }

                if (!await admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                Matter matterWithHistory = await admsRepository.GetMatterWithHistoryByIdAsync(matterId);
                if (matterWithHistory == null)
                {
                    return NotFound("Matter history not found");
                }

                return Ok(mapper.Map<MatterWithoutDocumentsDto>(matterWithHistory));
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "An error occurred while retrieving document history with id: {MatterId}", matterId);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Undelete specific matter
        /// </summary>
        /// <param name="matterId">Matter Id to restore</param>
        /// <returns>True if restored, false otherwise</returns>
        /// <response code="200">Returns the requested matter</response>
        /// <response code="400">An error occurred</response>
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
                if (matterId == Guid.Empty)
                {
                    return BadRequest("Invalid matter ID");
                }

                if (!await admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                bool restorationResult = await admsRepository.RestoreMatterAsync(matterId);
                if (!restorationResult)
                {
                    return BadRequest("Could not restore matter");
                }

                return Ok(restorationResult);
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "An error occurred while restoring Matter with id: {MatterId}", matterId);
                return BadRequest("An error occurred");
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
                IEnumerable<Matter> matters = await admsRepository.GetMattersAsync(description, includeArchived, includeDeleted);
                return Ok(mapper.Map<IEnumerable<MatterWithoutDocumentsDto>>(matters));
            }
            catch (Exception exception)
            {
                string errorMessage = $"An error occured while retrieving Matters";
                logger.LogCritical(errorMessage, exception);
                return BadRequest("An error occured");
            }
        }

        /// <summary>
        /// Updates matter details
        /// </summary>
        /// <param name="matterId">Matter Id</param>
        /// <param name="matter">Matter to be updated</param>
        /// <returns>IActionResult</returns>
        /// <response code="204">Returns No Content</response>
        /// <response code="400">An error occurred</response>
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
                if (matter == null)
                {
                    return BadRequest("Matter cannot be null");
                }

                if (!await admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                Matter? matterEntity = await admsRepository.GetMatterAsync(matterId, false);
                if (matterEntity == null)
                {
                    return NotFound("Matter not found");
                }

                mapper.Map(matter, matterEntity);

                MatterActivity? matterActivity = await admsRepository.GetMatterActivityByActivityNameAsync("SAVED");
                // TODO: replace hard-coded get user call with relevant authenticated user name
                User? user = await admsRepository.GetUserByUsernameAsync("rbrown");

                if (matterActivity != null && user != null)
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
                            CreatedAt = DateTime.UtcNow,
                        });
                }

                bool saveResult = await admsRepository.SaveChangesAsync();
                return saveResult ? NoContent() : BadRequest("Could not update matter");
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "An error occurred while updating Matter with id: {MatterId}", matterId);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Upload a new file to the ADMS system
        /// </summary>
        /// <param name="matterId">Matter to save the document to</param>
        /// <param name="fileUpload">File to be uploaded</param>
        /// <param name="cancelToken">Cancellation token</param>
        /// <returns>IActionResult</returns>
        /// <response code="200">Document uploaded</response>
        /// <response code="400">An error occurred</response>
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
                if (fileUpload == null || fileUpload.Length == 0)
                {
                    return BadRequest("File does not exist");
                }

                if (!await admsRepository.MatterExistsAsync(matterId))
                {
                    return NotFound("Matter not found");
                }

                Matter? matter = await admsRepository.GetMatterAsync(matterId, false);

                if (matter == null || matter.Id == Guid.Empty)
                {
                    return BadRequest($"Could not retrieve matter with id: {matterId}");
                }

                FileInfo fileInfo = new(fileUpload.FileName);
                string fileExtension = fileInfo.Extension.ToLower();

                DocumentForCreationDto documentToCreate = new()
                {
                    FileName = fileInfo.Name,
                    Extension = fileInfo.Extension,
                    IsCheckedOut = false
                };

                Document? newDocument = await admsRepository.AddDocumentAsync(matterId, documentToCreate);

                if (newDocument == null || newDocument.Id == Guid.Empty)
                {
                    return BadRequest("Unable to create document");
                }

                Entities.Revision? newRevision = await admsRepository.AddRevisionAsync(newDocument.Id, new RevisionDto()
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

                if (!await admsRepository.SaveChangesAsync())
                {
                    return BadRequest("Unable to create document");
                }

                string fileName = Path.Combine($"{newDocument.Id}R{newRevision.RevisionId}{fileInfo.Extension}");

                string folderName = Path.Combine("matters", matter.Id.ToString());
                string pathToSave = Path.Combine(@"C:\Dev\Repos\ADMSServerFiles", folderName);
                string fullPath = Path.Combine(pathToSave, fileName);

                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                string tempFile = Path.Combine(pathToSave, $"{newDocument.Id}R{newRevision.RevisionId}_123{fileInfo.Extension}");
                using (FileStream stream = new(tempFile, FileMode.Create))
                {
                    await fileUpload.CopyToAsync(stream, cancelToken);
                }

                switch (fileExtension)
                {
                    case ".doc":
                    case ".docm":
                    case ".docx":
                    case ".dot":
                    case ".dotm":
                    case ".dotx":
                        // Add Word properties
                        AddCustomDocumentProperties(tempFile, fullPath, newDocument.Id, newRevision.Id, matter.Id, DocumentType.Word);
                        System.IO.File.Move(tempFile, fullPath);
                        break;
                    case ".csv":
                    case ".xls":
                    case ".xlsx":
                    case ".xlt":
                    case ".xltm":
                    case ".xlw":
                        // Add Excel properties
                        AddCustomDocumentProperties(tempFile, fullPath, newDocument.Id, newRevision.Id, matter.Id, DocumentType.Excel);
                        System.IO.File.Delete(tempFile);
                        break;
                    case ".potx":
                    case ".pptm":
                    case ".pptx":
                        // Add PowerPoint properties
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
                logger.LogCritical(exception, "An error occurred while uploading document for matter with id: {MatterId}", matterId);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Adds custom document properties to various office documents
        /// </summary>
        /// <param name="tempFile">Temporary file used to add properties</param>
        /// <param name="fileName">Final filename to save file to</param>
        /// <param name="documentId">Document Id to add</param>
        /// <param name="revisionId">Revision Id to add</param>
        /// <param name="matterId">Matter Id to add</param>
        /// <param name="docType">Type of document to add properties to</param>
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
                            using FileStream docStream = new(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                            using WordDocument wordDocument = new(docStream, wordFormatType);
                            AddOrUpdateDocumentProperty(wordDocument, "ADMS.DocumentID", documentId.ToString(), docType);
                            AddOrUpdateDocumentProperty(wordDocument, "ADMS.RevisionID", revisionId.ToString(), docType);
                            AddOrUpdateDocumentProperty(wordDocument, "ADMS.MatterID", matterId.ToString(), docType);
                            wordDocument.Save(docStream, wordFormatType);
                        }
                        break;

                    case DocumentType.Excel:
                        using (ExcelEngine excelEngine = new())
                        {
                            IApplication application = excelEngine.Excel;
                            using FileStream inputStream = new(tempFile, FileMode.Open, FileAccess.Read);
                            IWorkbook workbook = application.Workbooks.Open(inputStream);
                            if (excelExtensionsToFormatType.TryGetValue(fileExtension, out ExcelVersion excelFormatType))
                            {
                                workbook.Version = excelFormatType;
                            }

                            AddOrUpdateDocumentProperty(workbook, "ADMS.DocumentID", documentId.ToString(), docType);
                            AddOrUpdateDocumentProperty(workbook, "ADMS.RevisionID", revisionId.ToString(), docType);
                            AddOrUpdateDocumentProperty(workbook, "ADMS.MatterID", matterId.ToString(), docType);

                            using FileStream fileStream = new(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                            workbook.SaveAs(fileStream);
                            workbook.Close();
                        }
                        break;

                    case DocumentType.PowerPoint:
                        using (FileStream inputStream = new(tempFile, FileMode.Open, FileAccess.ReadWrite))
                        {
                            using IPresentation pptxDoc = Presentation.Open(inputStream);
                            AddOrUpdateDocumentProperty(pptxDoc, "ADMS.DocumentID", documentId.ToString(), docType);
                            AddOrUpdateDocumentProperty(pptxDoc, "ADMS.RevisionID", revisionId.ToString(), docType);
                            AddOrUpdateDocumentProperty(pptxDoc, "ADMS.MatterID", matterId.ToString(), docType);
                            pptxDoc.Save(inputStream);
                        }
                        break;
                }
            }
            catch (IOException ioException)
            {
                logger.LogError(ioException, "An IO exception occurred while adding properties to document {DocumentId}, revision {RevisionId}, matter {MatterId}: {Message}", documentId, revisionId, matterId, ioException.Message);
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "An error occurred while adding properties to document {DocumentId}, revision {RevisionId}, matter {MatterId}: {Message}", documentId, revisionId, matterId, exception.Message);
            }
        }

        /// <summary>
        /// Add or update individual document property
        /// </summary>
        /// <param name="document">Document to add property to</param>
        /// <param name="propertyName">Specific property to add</param>
        /// <param name="propertyValue">Specific value to add</param>
        /// <param name="docType">Type of document to add property to</param>
        private void AddOrUpdateDocumentProperty(
            object document,
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
                logger.LogCritical(exception, "An error occurred while adding document properties: {Message}", exception.Message);
            }
        }

        /// <summary>
        /// Sets specific document property
        /// </summary>
        /// <param name="document">Document to add property to</param>
        /// <param name="propertyName">Specific property to add</param>
        /// <param name="propertyValue">Specific value to add</param>
        /// <param name="docType">Type of document to add property to</param>
        private void SetDocumentProperty(
            object document,
            string propertyName,
            string propertyValue,
            DocumentType docType)
        {
            try
            {
                switch (docType)
                {
                    case DocumentType.Word:
                        ((WordDocument)document).CustomDocumentProperties.Add(propertyName, propertyValue);
                        break;
                    case DocumentType.PowerPoint:
                        Syncfusion.Presentation.ICustomDocumentProperties docProps = ((IPresentation)document).CustomDocumentProperties;
                        if (!docProps.Contains(propertyName))
                        {
                            docProps.Add(propertyName);
                            docProps[propertyName].Value = propertyValue;
                        }
                        break;
                    default:
                        ((IWorkbook)document).CustomDocumentProperties[propertyName].Text = propertyValue;
                        break;
                }
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "An error occurred while setting document properties: {Message}", exception.Message);
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
