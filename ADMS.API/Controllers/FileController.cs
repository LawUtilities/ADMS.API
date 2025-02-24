using ADMS.API.Entities;
using ADMS.API.Services;

using Asp.Versioning;

using FileTypeChecker;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.Presentation;
using Syncfusion.PresentationRenderer;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;

using System.Diagnostics;
using System.Net.Mime;

namespace ADMS.API.Controllers
{
    /// <summary>
    /// File actions
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="FileController"/> class.
    /// </remarks>
    /// <param name="logger">The logger to be used by this controller.</param>
    /// <param name="admsRepository">The repository to use for data access.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/matters/{matterId}/documents/{documentId}/revisions/{revisionId}/files")]
    public class FileController(
        ILogger<FileController> logger,
        IAdmsRepository admsRepository) : ControllerBase
    {
        private readonly ILogger<FileController> _logger = logger;
        private readonly IAdmsRepository _admsRepository = admsRepository;

        private const string ServerFilesPath = @"C:\Dev\Repos\ADMSServerFiles";
        private const string PdfFolderName = "PDF";

        enum FileOperation
        {
            Copy,
            Delete
        };

        /// <summary>
        /// Upload an existing file to the ADMS system
        /// </summary>
        /// <param name="matterId">Matter to save the document to</param>
        /// <param name="documentId">Document to add file to</param>
        /// <param name="revisionId">Revision to add file as</param>
        /// <param name="fileUpload">file to be uploaded</param>
        /// <param name="cancelToken">cancellation token</param>
        /// <returns>IActionResult</returns>
        /// <response code="200">Document uploaded</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Document not found</response>
        [HttpPost("uploadExistingFile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadExistingFileAsync(
            Guid matterId,
            Guid documentId,
            Guid revisionId,
            IFormFile fileUpload,
            CancellationToken cancelToken)
        {
            try
            {
                if (fileUpload == null || fileUpload.Length == 0)
                {
                    return BadRequest("File does not exist");
                }

                if (!await CheckExistsAsync(matterId, documentId, revisionId))
                {
                    return NotFound("Matter, Document, or Revision not found");
                }

                Matter? matter = await _admsRepository.GetMatterAsync(matterId, false);
                Document? document = await _admsRepository.GetDocumentAsync(documentId, false);
                Entities.Revision? revision = await _admsRepository.GetRevisionByIdAsync(revisionId);

                if (matter == null || document == null || revision == null)
                {
                    return NotFound("Matter, Document, or Revision not found");
                }

                string extension = Path.GetExtension(fileUpload.FileName);
                string fileName = Path.Combine($"{document.Id}R{revision.RevisionId}{extension}");

                string folderName = Path.Combine("matters", matter.Id.ToString());
                string pathToSave = Path.Combine(ServerFilesPath, folderName);
                string fullPath = Path.Combine(pathToSave, fileName);

                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                using (FileStream stream = new(fullPath, FileMode.Create))
                {
                    await fileUpload.CopyToAsync(stream, cancelToken);
                }

                return Ok();
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                _logger.LogError(fileNotFoundException, "An error occurred while uploading document {DocumentId} in matter {MatterId} with revision {RevisionId}.", documentId, matterId, revisionId);
                return NotFound("File not found");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "An error occurred while uploading document {DocumentId} in matter {MatterId} with revision {RevisionId}.", documentId, matterId, revisionId);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Download the specified file from the ADMS system
        /// </summary>
        /// <param name="matterId">The ID of the matter</param>
        /// <param name="documentId">The ID of the document</param>
        /// <param name="revisionId">The ID of the revision</param>
        /// <returns>IActionResult</returns>
        /// <response code="200">Document downloaded</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Document not found</response>
        [HttpGet("downloadFile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadFile(
            Guid matterId,
            Guid documentId,
            Guid revisionId)
        {
            try
            {
                if (matterId == Guid.Empty || documentId == Guid.Empty || revisionId == Guid.Empty)
                {
                    return BadRequest("Invalid parameters");
                }

                if (!await CheckExistsAsync(matterId, documentId, revisionId))
                {
                    return NotFound("Matter, Document, or Revision not found");
                }

                Matter? downloadMatter = await _admsRepository.GetMatterAsync(matterId, false);
                Document? downloadDocument = await _admsRepository.GetDocumentAsync(documentId, false);
                Entities.Revision? downloadRevision = await _admsRepository.GetRevisionByIdAsync(revisionId);

                if (downloadMatter == null || downloadDocument == null || downloadRevision == null)
                {
                    return NotFound("Matter, Document, or Revision not found");
                }

                string fileName = Path.Combine($"{downloadDocument.Id}R{downloadRevision.RevisionId}{downloadDocument.Extension}");

                string folderName = Path.Combine("matters", downloadMatter.Id.ToString());
                string pathToSave = Path.Combine(ServerFilesPath, folderName);
                string fullPath = Path.Combine(pathToSave, fileName);

                if (!Directory.Exists(pathToSave) || !System.IO.File.Exists(fullPath))
                {
                    return BadRequest("Could not find file");
                }

                FileExtensionContentTypeProvider provider = new();
                if (!provider.TryGetContentType(fullPath, out string? contentType))
                {
                    contentType = "application/octet-stream";
                }

                byte[] bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                ContentDisposition contentDisposition = new()
                {
                    FileName = $"{Path.GetFileNameWithoutExtension(downloadDocument.FileName)}R{downloadRevision.RevisionId}{downloadDocument.Extension}",
                    Inline = false, // Prompt the user for download
                };

                Response.Headers.Append("Content-Disposition", contentDisposition.ToString());

                return File(bytes, contentType);
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                _logger.LogError(fileNotFoundException, "An error occurred while downloading document {DocumentId} in matter {MatterId} with revision {RevisionId}.", documentId, matterId, revisionId);
                return NotFound("File not found");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "An error occurred while downloading document {DocumentId} in matter {MatterId} with revision {RevisionId}.", documentId, matterId, revisionId);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Download the specified PDF file from the ADMS system
        /// </summary>
        /// <param name="matterId">The ID of the matter</param>
        /// <param name="documentId">The ID of the document</param>
        /// <param name="revisionId">The ID of the revision</param>
        /// <returns>IActionResult</returns>
        /// <response code="200">PDF Downloaded</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">PDF not created</response>
        [HttpGet("downloadPDF")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadPDF(
            Guid matterId,
            Guid documentId,
            Guid revisionId)
        {
            try
            {
                if (matterId == Guid.Empty || documentId == Guid.Empty || revisionId == Guid.Empty)
                {
                    return BadRequest("Invalid parameters");
                }

                if (!await CheckExistsAsync(matterId, documentId, revisionId))
                {
                    return NotFound("Matter, Document, or Revision not found");
                }

                Matter? downloadMatter = await _admsRepository.GetMatterAsync(matterId, false);
                Document? downloadDocument = await _admsRepository.GetDocumentAsync(documentId, false);
                Entities.Revision? downloadRevision = await _admsRepository.GetRevisionByIdAsync(revisionId);

                if (downloadMatter == null || downloadDocument == null || downloadRevision == null)
                {
                    return NotFound("Matter, Document, or Revision not found");
                }

                string fileName = Path.Combine($"{downloadDocument.Id}R{downloadRevision.RevisionId}{downloadDocument.Extension}");
                string pdfFileName = Path.Combine($"{Path.GetFileNameWithoutExtension(downloadDocument.FileName)}R{downloadRevision.RevisionId}.pdf");

                string folderName = Path.Combine("matters", downloadMatter.Id.ToString());
                string pathToMatter = Path.Combine(ServerFilesPath, folderName);
                string fullPath = Path.Combine(pathToMatter, fileName);

                if (!Directory.Exists(pathToMatter))
                {
                    return BadRequest("Could not find matter folder");
                }

                string pdfPath = Path.Combine(pathToMatter, PdfFolderName, pdfFileName);

                if (!ConvertToPDF(pathToMatter, fileName, pdfFileName))
                {
                    return BadRequest("File could not be converted to PDF");
                }

                if (!System.IO.File.Exists(pdfPath))
                {
                    return NotFound("PDF not created");
                }

                FileExtensionContentTypeProvider provider = new();
                if (!provider.TryGetContentType(pdfPath, out string? contentType))
                {
                    contentType = "application/pdf";
                }

                byte[] bytes = await System.IO.File.ReadAllBytesAsync(pdfPath);

                return File(bytes, contentType, Path.GetFileName(pdfPath));
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                _logger.LogError(fileNotFoundException, "An error occurred while downloading document {DocumentId} in matter {MatterId} with revision {RevisionId}.", documentId, matterId, revisionId);
                return NotFound("File not found");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "An error occurred while downloading document {DocumentId} in matter {MatterId} with revision {RevisionId}.", documentId, matterId, revisionId);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Verifies if the file to be uploaded matches the type identified
        /// </summary>
        /// <param name="matterId">The ID of the matter</param>
        /// <param name="documentId">The ID of the document</param>
        /// <param name="revisionId">The ID of the revision</param>
        /// <param name="fileUpload">File to be verified</param>
        /// <returns>IActionResult</returns>
        /// <response code="200">File type verified</response>
        /// <response code="400">An error occurred</response>
        /// <response code="404">Document not found</response>
        [HttpGet("verifyFileType")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyFileType(
            Guid matterId,
            Guid documentId,
            Guid revisionId,
            IFormFile fileUpload)
        {
            try
            {
                if (matterId == Guid.Empty || documentId == Guid.Empty || revisionId == Guid.Empty)
                {
                    return BadRequest("Invalid parameters");
                }

                if (!await CheckExistsAsync(matterId, documentId, revisionId))
                {
                    _logger.LogCritical("Matter, Document, or Revision not found for MatterId: {MatterId}, DocumentId: {DocumentId}, RevisionId: {RevisionId}", matterId, documentId, revisionId);
                    return NotFound("Matter, Document, or Revision not found");
                }

                if (fileUpload == null || fileUpload.Length == 0)
                {
                    return BadRequest("File is empty");
                }

                using (Stream stream = fileUpload.OpenReadStream())
                {
                    if (!FileTypeValidator.IsTypeRecognizable(stream))
                    {
                        return BadRequest("Unrecognized file type");
                    }

                    FileTypeChecker.Abstracts.IFileType fileType = FileTypeValidator.GetFileType(stream);

                    return Ok(new { fileType.Extension, Message = "File type recognized" });
                }
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                _logger.LogError(fileNotFoundException, "An error occurred while verifying file type for document {DocumentId} in matter {MatterId} with revision {RevisionId}.", documentId, matterId, revisionId);
                return NotFound("File not found");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "An error occurred while verifying file type for document {DocumentId} in matter {MatterId} with revision {RevisionId}.", documentId, matterId, revisionId);
                return BadRequest("An error occurred");
            }
        }

        /// <summary>
        /// Checks to see if Matter, Document and Revision exists
        /// </summary>
        /// <param name="matterId">matter to check</param>
        /// <param name="documentId">document to check</param>
        /// <param name="revisionId">revision to check</param>
        /// <returns>bool if all three exist, false otherwise</returns>
        private async Task<bool> CheckExistsAsync(Guid matterId, Guid documentId, Guid revisionId)
        {
            bool matterExists = await _admsRepository.MatterExistsAsync(matterId);
            if (!matterExists)
            {
                _logger.LogWarning("Matter with ID {MatterId} does not exist.", matterId);
                return false; // Matter doesn't exist, no need to check the other two
            }

            bool documentExists = await _admsRepository.DocumentExistsAsync(documentId);
            if (!documentExists)
            {
                _logger.LogWarning("Document with ID {DocumentId} does not exist.", documentId);
                return false; // Document doesn't exist
            }

            bool revisionExists = await _admsRepository.RevisionExistsAsync(revisionId);
            if (!revisionExists)
            {
                _logger.LogWarning("Revision with ID {RevisionId} does not exist.", revisionId);
            }

            return revisionExists;
        }

        /// <summary>
        /// Convert an original document to PDF if possible
        /// </summary>
        /// <param name="folder">Matter folder containing original file</param>
        /// <param name="originalFileName">Original file</param>
        /// <param name="pdfFileName">PDF filename</param>
        /// <returns>True if converted, false otherwise</returns>
        private bool ConvertToPDF(string folder, string originalFileName, string pdfFileName)
        {
            try
            {
                if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(originalFileName) || string.IsNullOrEmpty(pdfFileName))
                {
                    _logger.LogError("Invalid folder or file names");
                    return false;
                }

                string pdfFolder = Path.Combine(folder, PdfFolderName);
                if (!Directory.Exists(pdfFolder))
                {
                    DirectoryInfo directoryCreated = Directory.CreateDirectory(pdfFolder);
                    if (!directoryCreated.Exists)
                    {
                        _logger.LogError("Cannot create directory {pdfFolder}", pdfFolder);
                        return false;
                    }
                }

                string originalFile = Path.Combine(folder, originalFileName);
                string secondaryFile = Path.Combine(pdfFolder, originalFileName);

                PerformFileActivity(FileOperation.Copy, originalFile, secondaryFile);

                FileInfo fileInfo = new(originalFile);
                switch (fileInfo.Extension.ToLower())
                {
                    case ".doc":
                    case ".docm":
                    case ".docx":
                    case ".dot":
                    case ".dotm":
                    case ".dotx":
                        // Word to PDF
                        if (ConvertWordToPDF(Path.Combine(folder, PdfFolderName, originalFileName), Path.Combine(folder, PdfFolderName, pdfFileName)))
                        {
                            PerformFileActivity(FileOperation.Delete, secondaryFile, string.Empty);
                            return true;
                        }
                        break;
                    case ".csv":
                    case ".xls":
                    case ".xlsx":
                    case ".xlt":
                    case ".xltm":
                    case ".xlw":
                        // Excel to PDF
                        if (ConvertExcelToPDF(Path.Combine(folder, PdfFolderName, originalFileName), Path.Combine(folder, PdfFolderName, pdfFileName)))
                        {
                            PerformFileActivity(FileOperation.Delete, secondaryFile, string.Empty);
                            return true;
                        }
                        return false;
                    case ".pot":
                    case ".potm":
                    case ".potx":
                    case ".ppt":
                    case ".pptm":
                    case ".pptx":
                        // PowerPoint to PDF
                        if (ConvertPowerPointToPDF(Path.Combine(folder, PdfFolderName, originalFileName), Path.Combine(folder, PdfFolderName, pdfFileName)))
                        {
                            PerformFileActivity(FileOperation.Delete, secondaryFile, string.Empty);
                            return true;
                        }
                        return false;
                    case ".rtf":
                    case ".txt":
                        // Text to PDF
                        return false;
                    case ".jpg":
                    case ".jpeg":
                    case ".gif":
                    case ".bmp":
                    case ".webp":
                    case ".tif":
                    case ".tiff":
                    case ".png":
                    case ".eps":
                        // Image to PDF
                        return false;
                    default:
                        return false;
                }
                return false;
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Exception: {message}\n{stacktrace}", exception.Message, exception.StackTrace);
                return false;
            }
        }

        private void PerformFileActivity(FileOperation fileOperation, string file1, string file2)
        {
            switch (fileOperation)
            {
                case FileOperation.Copy:
                    try
                    {
                        System.IO.File.Copy(file1, file2, true);
                    }
                    catch (IOException ioException)
                    {
                        _logger.LogError(ioException, "Exception: {message}\n{stacktrace}", ioException.Message, ioException.StackTrace);
                        if (ioException.Message.Contains("in use"))
                        {
                            ProcessStartInfo startInfo = new()
                            {
                                FileName = "cmd.exe",
                                UseShellExecute = false,
                                RedirectStandardError = true,
                                Arguments = $"/C copy \"{file1}\" \"{file2}\""
                            };

                            Process? startCopy = Process.Start(startInfo);
                            if (startCopy != null)
                            {
                                startCopy.WaitForExit();
                                startCopy.Close();
                            }
                        }
                    }
                    break;
                case FileOperation.Delete:
                    try
                    {
                        System.IO.File.Delete(file1);
                    }
                    catch (IOException ioException)
                    {
                        _logger.LogError(ioException, "Exception: {message}\n{stacktrace}", ioException.Message, ioException.StackTrace);
                        if (ioException.Message.Contains("in use"))
                        {
                            ProcessStartInfo startInfo = new()
                            {
                                FileName = "cmd.exe",
                                UseShellExecute = false,
                                RedirectStandardError = true,
                                Arguments = $"/C delete \"{file1}\""
                            };

                            Process? startCopy = Process.Start(startInfo);
                            if (startCopy != null)
                            {
                                startCopy.WaitForExit();
                                startCopy.Close();
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private bool ConvertWordToPDF(string fileToConvert, string convertedFile)
        {
            try
            {
                using FileStream docStream = new(fileToConvert, FileMode.Open, FileAccess.Read);
                using WordDocument wordDocument = new(docStream, Syncfusion.DocIO.FormatType.Automatic);
                using DocIORenderer render = new();
                PdfDocument pdfDocument = render.ConvertToPDF(wordDocument);

                using FileStream stream = new(convertedFile, FileMode.Create, FileAccess.Write);
                pdfDocument.Save(stream);

                return true;
            }
            catch (IOException ioException)
            {
                _logger.LogError(ioException, "An IO exception occurred during conversion: {message}", ioException.Message);
            }
            catch (PdfException pdfException)
            {
                _logger.LogError(pdfException, "A PDF exception occurred during conversion: {message}", pdfException.Message);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "An error occurred during conversion: {message}", exception.Message);
            }

            return false;
        }

        private bool ConvertExcelToPDF(string fileToConvert, string convertedFile)
        {
            try
            {
                using ExcelEngine excelEngine = new();
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;

                using FileStream xlsxStream = new(fileToConvert, FileMode.Open, FileAccess.Read);
                IWorkbook workbook = application.Workbooks.Open(xlsxStream, ExcelOpenType.Automatic);

                XlsIORenderer renderer = new();
                PdfDocument pdfDocument = renderer.ConvertToPDF(workbook);

                using FileStream stream = new(convertedFile, FileMode.Create, FileAccess.Write);
                pdfDocument.Save(stream);

                return true;
            }
            catch (IOException ioException)
            {
                _logger.LogError(ioException, "An IO exception occurred during conversion: {message}", ioException.Message);
            }
            catch (PdfException pdfException)
            {
                _logger.LogError(pdfException, "A PDF exception occurred during conversion: {message}", pdfException.Message);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "An error occurred during conversion: {message}", exception.Message);
            }

            return false;
        }

        private bool ConvertPowerPointToPDF(string fileToConvert, string convertedFile)
        {
            try
            {
                using IPresentation pptxDoc = Presentation.Open(fileToConvert);
                PdfDocument pdfDocument = PresentationToPdfConverter.Convert(pptxDoc);

                using FileStream stream = new(convertedFile, FileMode.Create, FileAccess.Write);
                pdfDocument.Save(stream);

                return true;
            }
            catch (IOException ioException)
            {
                _logger.LogError(ioException, "An IO exception occurred during conversion: {message}", ioException.Message);
            }
            catch (PdfException pdfException)
            {
                _logger.LogError(pdfException, "A PDF exception occurred during conversion: {message}", pdfException.Message);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "An error occurred during conversion: {message}", exception.Message);
            }

            return false;
        }
    }
}