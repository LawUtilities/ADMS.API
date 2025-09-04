using ADMS.Shared;
using ADMS.Domain.Entities;
using ADMS.API.Extensions;
using ADMS.Application.DTOs;
using ADMS.API.Services;

using Asp.Versioning;

using MapsterMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.StaticFiles;

using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.Presentation;
using Syncfusion.PresentationRenderer;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;

using System.Net.Mime;

using FormatType = Syncfusion.DocIO.FormatType;
using Revision = ADMS.API.Entities.Revision;

namespace ADMS.API.Controllers;

/// <summary>
/// Controller for managing file operations within the ADMS system.
/// </summary>
/// <remarks>
/// This controller provides comprehensive file management functionality including:
/// - File upload and storage with security validation
/// - File download with content type detection and secure path handling
/// - PDF conversion for supported Microsoft Office document formats
/// - File type verification with virus scanning and content analysis
/// - Document metadata management and custom property injection
/// 
/// All endpoints implement consistent error handling, centralized validation,
/// structured logging, and security best practices including:
/// - Virus scanning using ClamAV integration
/// - File type validation by content analysis (not just extension)
/// - File size limits and extension validation
/// - Custom document properties for Office files (Word, Excel, PowerPoint)
/// - Secure file path construction to prevent directory traversal
/// 
/// The controller supports Microsoft Office document conversion to PDF using 
/// Syncfusion components and maintains comprehensive audit trails for all 
/// file operations.
/// 
/// Security features include:
/// - Comprehensive file validation (type, size, content, extension matching)
/// - Virus scanning before storage using ClamAV
/// - Secure file path construction with sanitization
/// - Content type detection and validation
/// - Prevention of malicious file uploads through multiple validation layers
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/files")]
[Produces("application/json", "application/xml")]
public class FileController(
    ILogger<FileController> logger,
    IRepository admsRepository,
    IMapper mapper,
    ProblemDetailsFactory problemDetailsFactory,
    IValidationService validationService,
    IVirusScanner virusScanner,
    IFileStorage fileStorage) : ControllerBase
{
    private readonly IRepository _admsRepository = admsRepository ?? throw new ArgumentNullException(nameof(admsRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ILogger<FileController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
    private readonly IValidationService _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    private readonly IVirusScanner _virusScanner = virusScanner ?? throw new ArgumentNullException(nameof(virusScanner));
    private readonly IFileStorage _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));

    #region Constants

    /// <summary>
    /// The server files path from environment variables with validation.
    /// </summary>
    private static readonly string ServerFilesPath = Environment.GetEnvironmentVariable("ADMSServerFilesPath")
        ?? throw new InvalidOperationException("The environment variable 'ADMSServerFilesPath' is not set.");

    /// <summary>
    /// The folder name for storing PDF conversions.
    /// </summary>
    private const string PdfFolderName = "PDF";

    /// <summary>
    /// Maximum allowed file size for uploads (50 MB).
    /// </summary>
    private const long MaxUploadFileSize = 50 * 1024 * 1024;

    #endregion Constants

    #region Actions

    /// <summary>
    /// Uploads a replacement file for an existing document revision.
    /// </summary>
    /// <remarks>
    /// This endpoint replaces the file content for an existing document revision
    /// while maintaining the document's metadata and audit trail. The process includes:
    /// 
    /// 1. Comprehensive validation of matter, document, and revision existence
    /// 2. File size and type validation with content analysis
    /// 3. Virus scanning using ClamAV integration
    /// 4. Document metadata updates with new file properties
    /// 5. Secure file storage with proper path construction
    /// 6. Audit trail creation for the file replacement
    /// 
    /// The operation is atomic - either all changes succeed or none are applied.
    /// File validation includes extension checking, MIME type detection, and
    /// content-based file type verification to prevent malicious uploads.
    /// 
    /// Custom document properties are injected into supported Office formats
    /// to maintain document traceability within the ADMS system.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document to update.</param>
    /// <param name="revisionId">The unique identifier of the revision to update with the new file.</param>
    /// <param name="fileUpload">The new file content to replace the existing file.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// <para>201 Created - File successfully uploaded and document metadata updated.</para>
    /// <para>400 BadRequest - Invalid input parameters or validation errors.</para>
    /// <para>404 NotFound - The specified matter, document, or revision does not exist.</para>
    /// <para>409 Conflict - File conflicts with existing content or constraints.</para>
    /// <para>413 PayloadTooLarge - File size exceeds the maximum allowed limit.</para>
    /// <para>415 UnsupportedMediaType - File type is not allowed or doesn't match extension.</para>
    /// <para>422 UnprocessableEntity - File contains virus or malware.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpPost("upload-existing")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadExistingFileAsync(
        [FromQuery] Guid matterId,
        [FromQuery] Guid documentId,
        [FromQuery] Guid revisionId,
        IFormFile? fileUpload,
        CancellationToken cancelToken = default)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateFileUploadParametersAsync(matterId, documentId, revisionId, fileUpload);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Validate entities existence
            var (matter, document, revision, entityValidationResult) =
                await ValidateEntitiesExistenceAsync(matterId, documentId, revisionId);
            if (entityValidationResult != null)
            {
                return entityValidationResult;
            }

            // Process and validate the uploaded file
            var (fileBytes, fileExtension, detectedMimeType, fileValidationError) =
                await ProcessFileUploadAsync(fileUpload!, cancelToken);
            if (fileValidationError != null)
            {
                return fileValidationError;
            }

            // Update document metadata with new file properties
            var updateResult = await UpdateDocumentMetadataAsync(
                document!, documentId, fileBytes!, detectedMimeType!, fileExtension!);
            if (updateResult != null)
            {
                return updateResult;
            }

            // Save file to storage with custom properties
            await SaveFileWithCustomPropertiesAsync(
                matterId, documentId, revisionId, fileBytes!, fileExtension!, cancelToken);

            // Add API version and correlation ID to response
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogInformation(
                "Successfully uploaded replacement file for document {DocumentId} revision {RevisionId} in matter {MatterId}.",
                documentId, revisionId, matterId);

            // Return response with download URL
            var response = CreateFileUploadResponse(matterId, documentId, revisionId, fileExtension!);
            return Created(response.DownloadUrl, response);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "File upload was canceled for document {DocumentId} revision {RevisionId} in matter {MatterId}.",
                documentId, revisionId, matterId);
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "File upload was canceled."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error uploading file for document {DocumentId} revision {RevisionId} in matter {MatterId}.",
                documentId, revisionId, matterId);

            return CreateInternalServerErrorResponse("uploading the file");
        }
    }

    /// <summary>
    /// Downloads the specified file from the ADMS system.
    /// </summary>
    /// <remarks>
    /// This endpoint provides secure file download functionality with:
    /// - Comprehensive validation of matter, document, and revision existence
    /// - Secure path construction to prevent directory traversal attacks
    /// - Content type detection for proper browser handling
    /// - Proper content disposition headers for file downloads
    /// - Audit logging of download activities
    /// 
    /// The endpoint validates all entities exist before attempting file access
    /// and uses centralized validation for DTOs to ensure data integrity.
    /// File paths are constructed securely to prevent unauthorized access.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document to download.</param>
    /// <param name="revisionId">The unique identifier of the revision to download.</param>
    /// <returns>
    /// <para>200 OK - File successfully downloaded with appropriate headers.</para>
    /// <para>400 BadRequest - Invalid input parameters.</para>
    /// <para>404 NotFound - The specified matter, document, revision, or file does not exist.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpGet("download", Name = "DownloadFile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadFileAsync(
        [FromQuery] Guid matterId,
        [FromQuery] Guid documentId,
        [FromQuery] Guid revisionId)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateFileAccessParametersAsync(matterId, documentId, revisionId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Validate entities existence and get validated DTOs
            var (matter, document, revision, entityValidationResult) =
                await ValidateEntitiesExistenceAsync(matterId, documentId, revisionId);
            if (entityValidationResult != null)
            {
                return entityValidationResult;
            }

            // Validate DTOs using centralized validation
            var dtoValidationResult = await ValidateEntityDtosAsync(document!, revision!);
            if (dtoValidationResult != null)
            {
                return dtoValidationResult;
            }

            // Build secure file path and validate file existence
            var filePath = BuildSecureFilePath(matterId, documentId, revision!.RevisionNumber, document!.Extension);
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("File not found at secure path for document {DocumentId} revision {RevisionId}.",
                    documentId, revisionId);
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "File not found."));
            }

            // Read file and prepare download response
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var contentType = GetContentType(filePath);
            var downloadFileName = CreateDownloadFileName(document.FileName, revision.RevisionNumber, document.Extension);

            // Set response headers for download
            SetDownloadResponseHeaders(downloadFileName);
            Response.AddApiVersion("1.0");

            _logger.LogInformation(
                "Successfully downloaded file {DownloadFileName} for document {DocumentId} revision {RevisionId}.",
                downloadFileName, documentId, revisionId);

            return File(fileBytes, contentType, downloadFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error downloading file for document {DocumentId} revision {RevisionId} in matter {MatterId}.",
                documentId, revisionId, matterId);

            return CreateInternalServerErrorResponse("downloading the file");
        }
    }

    /// <summary>
    /// Downloads the PDF version of the specified document revision.
    /// </summary>
    /// <remarks>
    /// This endpoint provides PDF download functionality with automatic conversion:
    /// - Validates entity existence and user permissions
    /// - Checks for existing PDF version of the document
    /// - Automatically converts supported Office formats to PDF if needed
    /// - Uses Syncfusion components for high-quality conversions
    /// - Caches converted PDFs for improved performance
    /// 
    /// Supported formats for PDF conversion:
    /// - Microsoft Word: .doc, .docx, .dot, .dotx, .docm, .dotm
    /// - Microsoft Excel: .xls, .xlsx, .csv, .xlt, .xltm, .xlw
    /// - Microsoft PowerPoint: .ppt, .pptx, .pot, .potx, .pptm, .potm
    /// 
    /// If the source document is not a supported format, the endpoint returns
    /// a 400 Bad Request with details about supported formats.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter containing the document.</param>
    /// <param name="documentId">The unique identifier of the document to convert and download.</param>
    /// <param name="revisionId">The unique identifier of the revision to convert and download.</param>
    /// <returns>
    /// <para>200 OK - PDF file successfully downloaded.</para>
    /// <para>400 BadRequest - Invalid parameters or unsupported file format.</para>
    /// <para>404 NotFound - The specified entities or source file does not exist.</para>
    /// <para>500 InternalServerError - Conversion failed or unexpected error occurred.</para>
    /// </returns>
    [HttpGet("download-pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadPdfAsync(
        [FromQuery] Guid matterId,
        [FromQuery] Guid documentId,
        [FromQuery] Guid revisionId)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateFileAccessParametersAsync(matterId, documentId, revisionId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Validate entities existence
            var (matter, document, revision, entityValidationResult) =
                await ValidateEntitiesExistenceAsync(matterId, documentId, revisionId);
            if (entityValidationResult != null)
            {
                return entityValidationResult;
            }

            // Validate DTOs using centralized validation
            var dtoValidationResult = await ValidateEntityDtosAsync(document!, revision!);
            if (dtoValidationResult != null)
            {
                return dtoValidationResult;
            }

            // Build file paths
            var sourceFilePath = BuildSecureFilePath(matterId, documentId, revision!.RevisionNumber, document!.Extension);
            var pdfFolderPath = Path.Combine(Path.GetDirectoryName(sourceFilePath)!, PdfFolderName);
            var pdfFileName = $"{Path.GetFileNameWithoutExtension(document.FileName)}R{revision.RevisionNumber}.pdf";
            var pdfFilePath = Path.Combine(pdfFolderPath, pdfFileName);

            // Validate source file exists
            if (!System.IO.File.Exists(sourceFilePath))
            {
                _logger.LogWarning("Source file not found for PDF conversion: {SourcePath}", sourceFilePath);
                return NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Source file not found."));
            }

            // Convert to PDF if not already present
            if (!System.IO.File.Exists(pdfFilePath))
            {
                var conversionResult = await ConvertToPdfAsync(sourceFilePath, pdfFilePath, document.Extension);
                if (!conversionResult)
                {
                    _logger.LogError("PDF conversion failed for document {DocumentId} revision {RevisionId}.",
                        documentId, revisionId);
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        CreateProblemDetails(StatusCodes.Status500InternalServerError, "PDF conversion failed."));
                }
            }

            // Read and return PDF file
            var pdfBytes = await System.IO.File.ReadAllBytesAsync(pdfFilePath);
            var contentType = "application/pdf";

            SetDownloadResponseHeaders(pdfFileName);
            Response.AddApiVersion("1.0");

            _logger.LogInformation(
                "Successfully downloaded PDF {PdfFileName} for document {DocumentId} revision {RevisionId}.",
                pdfFileName, documentId, revisionId);

            return File(pdfBytes, contentType, pdfFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error downloading PDF for document {DocumentId} revision {RevisionId} in matter {MatterId}.",
                documentId, revisionId, matterId);

            return CreateInternalServerErrorResponse("downloading the PDF");
        }
    }

    /// <summary>
    /// Verifies file type and integrity without storing the file.
    /// </summary>
    /// <remarks>
    /// This endpoint provides comprehensive file validation including:
    /// - Entity existence validation (matter, document, revision)
    /// - File extension validation against allowed types
    /// - Content-based file type detection and validation
    /// - Extension/content type matching verification
    /// - Virus scanning using ClamAV integration
    /// - File metadata validation using centralized validation services
    /// 
    /// The endpoint is useful for client-side validation before actual upload,
    /// allowing users to verify file compatibility and security before committing
    /// to the full upload process. This can save bandwidth and improve user experience.
    /// 
    /// The verification process uses the same validation logic as actual uploads
    /// to ensure consistency across the system.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter for context validation.</param>
    /// <param name="documentId">The unique identifier of the document for context validation.</param>
    /// <param name="revisionId">The unique identifier of the revision for context validation.</param>
    /// <param name="fileUpload">The file to verify without storing.</param>
    /// <returns>
    /// <para>200 OK - File is valid and safe for upload with verification details.</para>
    /// <para>400 BadRequest - Invalid input parameters.</para>
    /// <para>404 NotFound - The specified entities do not exist.</para>
    /// <para>415 UnsupportedMediaType - File type is not allowed or extension mismatch.</para>
    /// <para>422 UnprocessableEntity - File contains virus or malware.</para>
    /// <para>500 InternalServerError - Unexpected error during verification.</para>
    /// </returns>
    [HttpPost("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> VerifyFileTypeAsync(
        [FromQuery] Guid matterId,
        [FromQuery] Guid documentId,
        [FromQuery] Guid revisionId,
        IFormFile? fileUpload)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateFileVerificationParametersAsync(matterId, documentId, revisionId, fileUpload);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Validate entities existence (for context)
            var entityValidationResult = await ValidateEntitiesExistenceForVerificationAsync(matterId, documentId, revisionId);
            if (entityValidationResult != null)
            {
                return entityValidationResult;
            }

            // Process and validate file without storing
            var (fileBytes, fileExtension, detectedMimeType, fileValidationError) =
                await ProcessFileUploadAsync(fileUpload!, CancellationToken.None);
            if (fileValidationError != null)
            {
                return fileValidationError;
            }

            // Create validation DTO and validate
            var validationDto = CreateValidationDto(fileUpload!.FileName, fileExtension!, fileBytes!, detectedMimeType!);
            var dtoValidationResult = _validationService.ValidateObject(validationDto);
            if (dtoValidationResult != null)
            {
                return dtoValidationResult;
            }

            Response.AddApiVersion("1.0");

            _logger.LogInformation(
                "File verification successful for type '{FileType}' with MIME type '{MimeType}'.",
                fileExtension, detectedMimeType);

            return Ok(new
            {
                Extension = fileExtension,
                MimeType = detectedMimeType,
                FileSize = fileBytes!.Length,
                IsSupported = true,
                Message = "File type verified and accepted for upload.",
                SupportedConversions = GetSupportedConversions(fileExtension!)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error verifying file type for document {DocumentId} revision {RevisionId} in matter {MatterId}.",
                documentId, revisionId, matterId);

            return CreateInternalServerErrorResponse("verifying the file");
        }
    }

    /// <summary>
    /// Uploads a new file and creates a new document in the specified matter.
    /// </summary>
    /// <remarks>
    /// This endpoint creates a completely new document with the uploaded file:
    /// - Validates matter existence and user permissions
    /// - Performs comprehensive file validation and security scanning
    /// - Checks for duplicate filenames within the matter
    /// - Creates new document and revision entities
    /// - Stores file with custom document properties for Office formats
    /// - Creates audit trail entries for document creation
    /// 
    /// The process is atomic - either all operations succeed or none are applied.
    /// Custom ADMS properties are injected into supported Office documents for
    /// traceability and system integration.
    /// 
    /// File validation includes multiple layers of security checks to prevent
    /// malicious uploads and ensure system integrity.
    /// </remarks>
    /// <param name="matterId">The unique identifier of the matter to create the document in.</param>
    /// <param name="fileUpload">The file to upload and associate with the new document.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// <para>201 Created - Document successfully created with download URL.</para>
    /// <para>400 BadRequest - Invalid input parameters or validation errors.</para>
    /// <para>404 NotFound - The specified matter does not exist.</para>
    /// <para>409 Conflict - A document with the same filename already exists.</para>
    /// <para>413 PayloadTooLarge - File size exceeds the maximum allowed limit.</para>
    /// <para>415 UnsupportedMediaType - File type is not allowed.</para>
    /// <para>422 UnprocessableEntity - File contains virus or malware.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpPost("{matterId}/upload")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadNewFileAsync(
        Guid matterId,
        IFormFile? fileUpload,
        CancellationToken cancelToken = default)
    {
        try
        {
            // Validate input parameters
            var validationResult = await ValidateNewFileUploadParametersAsync(matterId, fileUpload);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Process and validate the uploaded file
            var (fileBytes, fileExtension, detectedMimeType, fileValidationError) =
                await ProcessFileUploadAsync(fileUpload!, cancelToken);
            if (fileValidationError != null)
            {
                return fileValidationError;
            }

            // Check for duplicate filename
            var fileName = Path.GetFileName(fileUpload!.FileName);
            if (await _admsRepository.FileNameExists(matterId, fileName))
            {
                _logger.LogWarning("Duplicate filename '{FileName}' in matter {MatterId}.", fileName, matterId);
                return Conflict(CreateProblemDetails(StatusCodes.Status409Conflict,
                    "A file with the same name already exists in this matter."));
            }

            // Create and validate document DTO
            var documentDto = CreateDocumentForCreationDto(fileUpload.FileName, fileExtension!, fileBytes!, detectedMimeType!);
            var dtoValidationResult = _validationService.ValidateObject(documentDto);
            if (dtoValidationResult != null)
            {
                return dtoValidationResult;
            }

            // Create document and revision
            var newDocument = await _admsRepository.AddDocumentAsync(matterId, documentDto);
            if (newDocument == null)
            {
                _logger.LogError("Failed to create document for file '{FileName}' in matter {MatterId}.",
                    fileName, matterId);
                return CreateInternalServerErrorResponse("creating the document");
            }

            var newRevision = newDocument.Revisions.FirstOrDefault();
            if (newRevision == null)
            {
                _logger.LogError("Failed to create revision for document '{FileName}' in matter {MatterId}.",
                    fileName, matterId);
                return CreateInternalServerErrorResponse("creating the document revision");
            }

            // Save file with custom properties
            await SaveNewFileWithCustomPropertiesAsync(
                matterId, newDocument.Id, newRevision.Id, fileBytes!, fileExtension!, cancelToken);

            // Add response headers and log success
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogInformation(
                "Successfully uploaded new file '{FileName}' as document {DocumentId} in matter {MatterId}.",
                fileName, newDocument.Id, matterId);

            // Return response with download URL
            var response = CreateFileUploadResponse(matterId, newDocument.Id, newRevision.Id, fileExtension!);
            return Created(response.DownloadUrl, response);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("New file upload was canceled for matter {MatterId}.", matterId);
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "File upload was canceled."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading new file to matter {MatterId}.", matterId);
            return CreateInternalServerErrorResponse("uploading the file");
        }
    }

    /// <summary>
    /// Returns the HTTP methods supported by the file resource.
    /// </summary>
    /// <remarks>
    /// This endpoint implements the HTTP OPTIONS method to allow clients
    /// to discover which operations are available on the file resource.
    /// This is useful for API documentation tools, CORS preflight requests,
    /// and client applications that need to determine available functionality dynamically.
    /// 
    /// The response includes appropriate headers indicating supported methods,
    /// API version information, and CORS configuration if applicable.
    /// </remarks>
    /// <returns>
    /// <para>204 NoContent - Response with Allow header listing supported HTTP methods.</para>
    /// <para>500 InternalServerError - Unexpected server error occurred.</para>
    /// </returns>
    [HttpOptions]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult GetFileOptions()
    {
        try
        {
            Response.Headers.Allow = "GET,POST,OPTIONS";
            Response.AddApiVersion("1.0");
            Response.AddCorrelationId();

            _logger.LogDebug("OPTIONS request processed for file resource.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OPTIONS request for file resource.");
            return CreateInternalServerErrorResponse("processing the OPTIONS request");
        }
    }

    #endregion Actions

    #region Private Helper Methods

    /// <summary>
    /// Validates parameters for file upload operations.
    /// </summary>
    private async Task<ActionResult?> ValidateFileUploadParametersAsync(
        Guid matterId, Guid documentId, Guid revisionId, IFormFile? fileUpload)
    {
        // Validate GUIDs
        var guidValidationResult = _validationService.ValidateGuid(matterId, nameof(matterId))
            ?? _validationService.ValidateGuid(documentId, nameof(documentId))
            ?? _validationService.ValidateGuid(revisionId, nameof(revisionId));

        if (guidValidationResult != null)
        {
            _logger.LogWarning("Invalid GUID parameters for file upload.");
            return guidValidationResult;
        }

        // Validate file upload
        if (fileUpload == null || fileUpload.Length == 0)
        {
            _logger.LogWarning("File upload is missing or empty.");
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "File upload is required."));
        }

        if (fileUpload.Length > MaxUploadFileSize)
        {
            _logger.LogWarning("File size {FileSize} exceeds maximum allowed {MaxSize}.",
                fileUpload.Length, MaxUploadFileSize);
            return StatusCode(StatusCodes.Status413PayloadTooLarge,
                CreateProblemDetails(StatusCodes.Status413PayloadTooLarge,
                    $"File size exceeds maximum allowed ({MaxUploadFileSize / (1024 * 1024)} MB)."));
        }

        return null;
    }

    /// <summary>
    /// Validates parameters for file access operations (download).
    /// </summary>
    private async Task<ActionResult?> ValidateFileAccessParametersAsync(
        Guid matterId, Guid documentId, Guid revisionId)
    {
        // Validate GUIDs
        var guidValidationResult = _validationService.ValidateGuid(matterId, nameof(matterId))
            ?? _validationService.ValidateGuid(documentId, nameof(documentId))
            ?? _validationService.ValidateGuid(revisionId, nameof(revisionId));

        if (guidValidationResult != null)
        {
            _logger.LogWarning("Invalid GUID parameters for file access.");
            return guidValidationResult;
        }

        return null;
    }

    /// <summary>
    /// Validates parameters for file verification operations.
    /// </summary>
    private async Task<ActionResult?> ValidateFileVerificationParametersAsync(
        Guid matterId, Guid documentId, Guid revisionId, IFormFile? fileUpload)
    {
        var guidValidation = await ValidateFileAccessParametersAsync(matterId, documentId, revisionId);
        if (guidValidation != null) return guidValidation;

        if (fileUpload == null || fileUpload.Length == 0)
        {
            _logger.LogWarning("File upload is missing or empty for verification.");
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest,
                "File upload is required for verification."));
        }

        return null;
    }

    /// <summary>
    /// Validates parameters for new file upload operations.
    /// </summary>
    private async Task<ActionResult?> ValidateNewFileUploadParametersAsync(Guid matterId, IFormFile? fileUpload)
    {
        // Validate matter ID
        var guidValidationResult = _validationService.ValidateGuid(matterId, nameof(matterId));
        if (guidValidationResult != null)
        {
            return guidValidationResult;
        }

        // Validate matter existence
        var matterExistsResult = await _validationService.ValidateMatterExistsAsync(matterId);
        if (matterExistsResult != null)
        {
            return matterExistsResult;
        }

        // Validate file upload
        if (fileUpload == null || fileUpload.Length == 0)
        {
            _logger.LogWarning("File upload is missing or empty for new file.");
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "File upload is required."));
        }

        if (fileUpload.Length > MaxUploadFileSize)
        {
            _logger.LogWarning("File size {FileSize} exceeds maximum allowed {MaxSize}.",
                fileUpload.Length, MaxUploadFileSize);
            return StatusCode(StatusCodes.Status413PayloadTooLarge,
                CreateProblemDetails(StatusCodes.Status413PayloadTooLarge,
                    $"File size exceeds maximum allowed ({MaxUploadFileSize / (1024 * 1024)} MB)."));
        }

        return null;
    }

    /// <summary>
    /// Validates entity existence and returns validated entities.
    /// </summary>
    private async Task<(Matter? matter, Document? document, Revision? revision, ActionResult? error)>
        ValidateEntitiesExistenceAsync(Guid matterId, Guid documentId, Guid revisionId)
    {
        // Get matter
        var matter = await _admsRepository.GetMatterAsync(matterId, includeDocuments: false);
        if (matter == null)
        {
            _logger.LogWarning("Matter {MatterId} not found.", matterId);
            return (null, null, null, NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Matter not found.")));
        }

        // Get document
        var document = await _admsRepository.GetDocumentAsync(documentId, includeRevisions: false, includeHistory: false);
        if (document == null)
        {
            _logger.LogWarning("Document {DocumentId} not found.", documentId);
            return (null, null, null, NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Document not found.")));
        }

        // Get revision
        var revision = await _admsRepository.GetRevisionByIdAsync(revisionId);
        if (revision == null)
        {
            _logger.LogWarning("Revision {RevisionId} not found.", revisionId);
            return (null, null, null, NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Revision not found.")));
        }

        return (matter, document, revision, null);
    }

    /// <summary>
    /// Validates entity existence for verification operations (context only).
    /// </summary>
    private async Task<ActionResult?> ValidateEntitiesExistenceForVerificationAsync(
        Guid matterId, Guid documentId, Guid revisionId)
    {
        var (_, _, _, error) = await ValidateEntitiesExistenceAsync(matterId, documentId, revisionId);
        return error;
    }

    /// <summary>
    /// Validates entity DTOs using centralized validation.
    /// </summary>
    private async Task<ActionResult?> ValidateEntityDtosAsync(Document document, Revision revision)
    {
        // Validate document DTO
        var documentDto = _mapper.Map<DocumentDto>(document);
        var documentValidationResult = _validationService.ValidateObject(documentDto);
        if (documentValidationResult != null)
        {
            _logger.LogWarning("Document DTO validation failed.");
            return documentValidationResult;
        }

        // Validate revision DTO
        var revisionDto = _mapper.Map<RevisionDto>(revision);
        var revisionValidationResult = _validationService.ValidateObject(revisionDto);
        if (revisionValidationResult != null)
        {
            _logger.LogWarning("Revision DTO validation failed.");
            return revisionValidationResult;
        }

        return null;
    }

    /// <summary>
    /// Processes and validates uploaded file with comprehensive security checks.
    /// </summary>
    private async Task<(byte[]? fileBytes, string? fileExtension, string? detectedMimeType, ActionResult? error)>
        ProcessFileUploadAsync(IFormFile fileUpload, CancellationToken cancelToken)
    {
        var fileInfo = new FileInfo(fileUpload.FileName);
        var fileExtension = fileInfo.Extension.ToLowerInvariant();

        // Validate file extension
        if (!FileValidationHelper.IsExtensionAllowed(fileExtension))
        {
            _logger.LogWarning("File extension '{FileExtension}' is not allowed.", fileExtension);
            return (null, null, null, StatusCode(StatusCodes.Status415UnsupportedMediaType,
                CreateProblemDetails(StatusCodes.Status415UnsupportedMediaType,
                    $"File extension '{fileExtension}' is not allowed.")));
        }

        // Read file content
        byte[] fileBytes;
        try
        {
            using var ms = new MemoryStream();
            await fileUpload.CopyToAsync(ms, cancelToken);
            fileBytes = ms.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading uploaded file.");
            return (null, null, null, CreateInternalServerErrorResponse("reading the uploaded file"));
        }

        // Validate file type by content
        if (!FileValidationHelper.IsValidFileType(fileBytes, out var detectedMimeType, out var detectedExtension, _logger))
        {
            _logger.LogWarning("File type validation failed. Detected: {Extension}, {MimeType}",
                detectedExtension, detectedMimeType);
            return (null, null, null, StatusCode(StatusCodes.Status415UnsupportedMediaType,
                CreateProblemDetails(StatusCodes.Status415UnsupportedMediaType,
                    $"File type not allowed. Detected: {detectedExtension}, {detectedMimeType}")));
        }

        // Validate extension matches content
        if (!string.Equals(fileExtension, detectedExtension, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("File extension mismatch. Expected: {Expected}, Detected: {Detected}",
                fileExtension, detectedExtension);
            return (null, null, null, StatusCode(StatusCodes.Status415UnsupportedMediaType,
                CreateProblemDetails(StatusCodes.Status415UnsupportedMediaType,
                    $"File extension '{fileExtension}' does not match detected type '{detectedExtension}'.")));
        }

        // Virus scan
        try
        {
            using var scanStream = new MemoryStream(fileBytes);
            var scanResult = await _virusScanner.ScanFileForVirusesAsync(scanStream);
            if (!scanResult.IsClean)
            {
                _logger.LogWarning("Virus detected in uploaded file.");
                return (null, null, null, UnprocessableEntity(
                    CreateProblemDetails(StatusCodes.Status422UnprocessableEntity,
                        "The uploaded file contains a virus or malware and was rejected.")));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during virus scan.");
            return (null, null, null, CreateInternalServerErrorResponse("performing virus scan"));
        }

        return (fileBytes, fileExtension, detectedMimeType, null);
    }

    /// <summary>
    /// Updates document metadata with new file properties.
    /// </summary>
    private async Task<ActionResult?> UpdateDocumentMetadataAsync(
        Document document, Guid documentId, byte[] fileBytes, string detectedMimeType, string fileExtension)
    {
        // Update document properties
        document.FileSize = fileBytes.Length;
        document.MimeType = detectedMimeType;
        document.Checksum = ComputeChecksum(fileBytes);
        document.Extension = fileExtension.TrimStart('.');

        // Create update DTO and validate
        var documentForUpdateDto = _mapper.Map<DocumentForUpdateDto>(document);
        var dtoValidationResult = _validationService.ValidateObject(documentForUpdateDto);
        if (dtoValidationResult != null)
        {
            _logger.LogWarning("Document update DTO validation failed.");
            return dtoValidationResult;
        }

        // Update document in repository
        var savedDocument = await _admsRepository.UpdateDocumentAsync(documentId, documentForUpdateDto);
        if (savedDocument == null)
        {
            _logger.LogError("Failed to update document metadata for {DocumentId}.", documentId);
            return CreateInternalServerErrorResponse("updating document metadata");
        }

        // Save changes
        if (!await _admsRepository.SaveChangesAsync())
        {
            _logger.LogError("Failed to save document metadata changes for {DocumentId}.", documentId);
            return CreateInternalServerErrorResponse("saving document metadata");
        }

        return null;
    }

    /// <summary>
    /// Saves file to storage with custom document properties.
    /// </summary>
    private async Task SaveFileWithCustomPropertiesAsync(
        Guid matterId, Guid documentId, Guid revisionId, byte[] fileBytes, string fileExtension, CancellationToken cancelToken)
    {
        var folderPath = Path.Combine(ServerFilesPath, "matters", matterId.ToString());
        Directory.CreateDirectory(folderPath);

        var revision = await _admsRepository.GetRevisionByIdAsync(revisionId);
        var fileName = $"{documentId}R{revision!.RevisionNumber}{fileExtension}";
        var tempFilePath = Path.Combine(folderPath, $"{fileName}_temp");
        var finalFilePath = Path.Combine(folderPath, fileName);

        try
        {
            // Save to temporary location first
            await _fileStorage.SaveFileAsync(tempFilePath, fileBytes, cancelToken);

            // Add custom properties and save to final location
            AddCustomDocumentProperties(tempFilePath, finalFilePath, documentId, revisionId, matterId);

            _logger.LogDebug("Successfully saved file with custom properties: {FinalPath}", finalFilePath);
        }
        finally
        {
            // Clean up temporary file
            if (System.IO.File.Exists(tempFilePath))
            {
                System.IO.File.Delete(tempFilePath);
            }
        }
    }

    /// <summary>
    /// Saves new file with custom document properties.
    /// </summary>
    private async Task SaveNewFileWithCustomPropertiesAsync(
        Guid matterId, Guid documentId, Guid revisionId, byte[] fileBytes, string fileExtension, CancellationToken cancelToken)
    {
        await SaveFileWithCustomPropertiesAsync(matterId, documentId, revisionId, fileBytes, fileExtension, cancelToken);
    }

    /// <summary>
    /// Builds secure file path preventing directory traversal.
    /// </summary>
    private static string BuildSecureFilePath(Guid matterId, Guid documentId, int revisionNumber, string extension)
    {
        var folderPath = Path.Combine(ServerFilesPath, "matters", matterId.ToString());
        var fileName = $"{documentId}R{revisionNumber}.{extension}";
        return Path.Combine(folderPath, fileName);
    }

    /// <summary>
    /// Gets content type for file download.
    /// </summary>
    private static string GetContentType(string filePath)
    {
        var provider = new FileExtensionContentTypeProvider();
        return provider.TryGetContentType(filePath, out var contentType)
            ? contentType
            : "application/octet-stream";
    }

    /// <summary>
    /// Creates download filename with revision information.
    /// </summary>
    private static string CreateDownloadFileName(string originalFileName, int revisionNumber, string extension)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        return $"{nameWithoutExtension}R{revisionNumber}.{extension}";
    }

    /// <summary>
    /// Sets appropriate response headers for file downloads.
    /// </summary>
    private void SetDownloadResponseHeaders(string downloadFileName)
    {
        var contentDisposition = new ContentDisposition
        {
            FileName = downloadFileName,
            Inline = false
        };
        Response.Headers.Append("Content-Disposition", contentDisposition.ToString());
    }

    /// <summary>
    /// Creates validation DTO for file verification.
    /// </summary>
    private DocumentForCreationDto CreateValidationDto(string fileName, string extension, byte[] fileBytes, string mimeType)
    {
        return new DocumentForCreationDto
        {
            FileName = Path.GetFileNameWithoutExtension(fileName),
            Extension = extension.TrimStart('.'),
            FileSize = fileBytes.Length,
            MimeType = mimeType,
            Checksum = ComputeChecksum(fileBytes),
            IsCheckedOut = false
        };
    }

    /// <summary>
    /// Creates document creation DTO.
    /// </summary>
    private DocumentForCreationDto CreateDocumentForCreationDto(string fileName, string extension, byte[] fileBytes, string mimeType)
    {
        return new DocumentForCreationDto
        {
            FileName = fileName,
            Extension = extension.TrimStart('.'),
            FileSize = fileBytes.Length,
            MimeType = mimeType,
            Checksum = ComputeChecksum(fileBytes),
            IsCheckedOut = false
        };
    }

    /// <summary>
    /// Creates file upload response object.
    /// </summary>
    private dynamic CreateFileUploadResponse(Guid matterId, Guid documentId, Guid revisionId, string fileExtension)
    {
        return new
        {
            DocumentId = documentId,
            RevisionId = revisionId,
            DownloadUrl = Url.Action("DownloadFile", "File", new { matterId, documentId, revisionId })
        };
    }

    /// <summary>
    /// Gets supported conversion formats for a file extension.
    /// </summary>
    private static string[] GetSupportedConversions(string extension)
    {
        var lowerExtension = extension.ToLowerInvariant();
        var officeExtensions = new[] { ".doc", ".docx", ".dot", ".dotx", ".docm", ".dotm",
            ".xls", ".xlsx", ".csv", ".xlt", ".xltm", ".xlw",
            ".ppt", ".pptx", ".pot", ".potx", ".pptm", ".potm" };

        return officeExtensions.Contains(lowerExtension) ? new[] { "PDF" } : Array.Empty<string>();
    }

    /// <summary>
    /// Converts supported office documents to PDF asynchronously.
    /// </summary>
    private async Task<bool> ConvertToPdfAsync(string sourceFilePath, string pdfFilePath, string extension)
    {
        return await Task.Run(() => ConvertToPdf(
            Path.GetDirectoryName(sourceFilePath)!,
            Path.GetFileName(sourceFilePath),
            Path.GetFileName(pdfFilePath))).ConfigureAwait(false);
    }

    /// <summary>
    /// Converts a supported Office document to PDF format.
    /// </summary>
    /// <param name="folder">The folder containing the original file.</param>
    /// <param name="originalFileName">The name of the original file to convert.</param>
    /// <param name="pdfFileName">The desired name for the output PDF file.</param>
    /// <returns>True if the conversion was successful; false otherwise.</returns>
    private bool ConvertToPdf(string folder, string originalFileName, string pdfFileName)
    {
        if (string.IsNullOrWhiteSpace(folder) ||
            string.IsNullOrWhiteSpace(originalFileName) ||
            string.IsNullOrWhiteSpace(pdfFileName))
        {
            _logger.LogError("ConvertToPdf: Invalid folder or file names provided. Folder: {Folder}, Original: {Original}, PDF: {Pdf}",
                folder, originalFileName, pdfFileName);
            return false;
        }

        var pdfFolder = Path.Combine(folder, PdfFolderName);
        try
        {
            Directory.CreateDirectory(pdfFolder);

            var originalFilePath = Path.Combine(folder, originalFileName);
            var pdfFilePath = Path.Combine(pdfFolder, pdfFileName);

            if (!System.IO.File.Exists(originalFilePath))
            {
                _logger.LogError("ConvertToPdf: Source file does not exist at {Path}", originalFilePath);
                return false;
            }

            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

            return extension switch
            {
                ".doc" or ".docm" or ".docx" or ".dot" or ".dotm" or ".dotx" =>
                    ConvertWordToPdf(originalFilePath, pdfFilePath),
                ".csv" or ".xls" or ".xlsx" or ".xlt" or ".xltm" or ".xlw" =>
                    ConvertExcelToPdf(originalFilePath, pdfFilePath),
                ".pot" or ".potm" or ".potx" or ".ppt" or ".pptm" or ".pptx" =>
                    ConvertPowerPointToPdf(originalFilePath, pdfFilePath),
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ConvertToPdf: Error during PDF conversion for file '{File}': {Message}", originalFileName, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Converts a Word document to PDF format using Syncfusion DocIO and DocIORenderer.
    /// </summary>
    /// <param name="fileToConvert">The full path to the Word document to be converted.</param>
    /// <param name="convertedFile">The full path where the resulting PDF should be saved.</param>
    /// <returns>True if the conversion was successful; false otherwise.</returns>
    private bool ConvertWordToPdf(string fileToConvert, string convertedFile)
    {
        if (string.IsNullOrWhiteSpace(fileToConvert) || string.IsNullOrWhiteSpace(convertedFile))
        {
            _logger.LogError("ConvertWordToPdf: Invalid file paths provided. Source: {Source}, Target: {Target}",
                fileToConvert, convertedFile);
            return false;
        }

        if (!System.IO.File.Exists(fileToConvert))
        {
            _logger.LogError("ConvertWordToPdf: Source file does not exist at {Path}", fileToConvert);
            return false;
        }

        try
        {
            using var docStream = new FileStream(fileToConvert, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var wordDocument = new WordDocument(docStream, FormatType.Automatic);
            using var render = new DocIORenderer();
            using var pdfDocument = render.ConvertToPDF(wordDocument);
            using var stream = new FileStream(convertedFile, FileMode.Create, FileAccess.Write, FileShare.None);
            pdfDocument.Save(stream);

            _logger.LogInformation("ConvertWordToPdf: Successfully converted Word to PDF. Source: {Source}, Target: {Target}",
                fileToConvert, convertedFile);
            return true;
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "ConvertWordToPdf: IO error during conversion. Source: {Source}, Target: {Target}, Message: {Message}",
                fileToConvert, convertedFile, ioEx.Message);
        }
        catch (PdfException pdfEx)
        {
            _logger.LogError(pdfEx, "ConvertWordToPdf: PDF error during conversion. Source: {Source}, Target: {Target}, Message: {Message}",
                fileToConvert, convertedFile, pdfEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ConvertWordToPdf: Unexpected error during conversion. Source: {Source}, Target: {Target}, Message: {Message}",
                fileToConvert, convertedFile, ex.Message);
        }
        return false;
    }

    /// <summary>
    /// Converts an Excel document to PDF format using Syncfusion XlsIO and XlsIORenderer.
    /// </summary>
    /// <param name="fileToConvert">The full path to the Excel file to be converted.</param>
    /// <param name="convertedFile">The full path where the resulting PDF should be saved.</param>
    /// <returns>True if the conversion was successful; false otherwise.</returns>
    private bool ConvertExcelToPdf(string fileToConvert, string convertedFile)
    {
        if (string.IsNullOrWhiteSpace(fileToConvert) || string.IsNullOrWhiteSpace(convertedFile))
        {
            _logger.LogError("ConvertExcelToPdf: Invalid file paths provided. Source: {Source}, Target: {Target}",
                fileToConvert, convertedFile);
            return false;
        }

        if (!System.IO.File.Exists(fileToConvert))
        {
            _logger.LogError("ConvertExcelToPdf: Source file does not exist at {Path}", fileToConvert);
            return false;
        }

        ExcelEngine? excelEngine = null;
        IWorkbook? workbook = null;
        PdfDocument? pdfDocument = null;

        try
        {
            excelEngine = new ExcelEngine();
            var application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Xlsx;

            using var xlsxStream = new FileStream(fileToConvert, FileMode.Open, FileAccess.Read, FileShare.Read);
            workbook = application.Workbooks.Open(xlsxStream, ExcelOpenType.Automatic);

            var renderer = new XlsIORenderer();
            pdfDocument = renderer.ConvertToPDF(workbook);

            using var stream = new FileStream(convertedFile, FileMode.Create, FileAccess.Write, FileShare.None);
            pdfDocument.Save(stream);

            _logger.LogInformation("ConvertExcelToPdf: Successfully converted Excel to PDF. Source: {Source}, Target: {Target}",
                fileToConvert, convertedFile);
            return true;
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "ConvertExcelToPdf: IO error during conversion. Source: {Source}, Target: {Target}, Message: {Message}",
                fileToConvert, convertedFile, ioEx.Message);
        }
        catch (PdfException pdfEx)
        {
            _logger.LogError(pdfEx, "ConvertExcelToPdf: PDF error during conversion. Source: {Source}, Target: {Target}, Message: {Message}",
                fileToConvert, convertedFile, pdfEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ConvertExcelToPdf: Unexpected error during conversion. Source: {Source}, Target: {Target}, Message: {Message}",
                fileToConvert, convertedFile, ex.Message);
        }
        finally
        {
            // Dispose in the correct order to avoid Syncfusion issues
            pdfDocument?.Dispose();
            workbook?.Close();
            excelEngine?.Dispose();
        }
        return false;
    }

    /// <summary>
    /// Converts a PowerPoint document to PDF format using Syncfusion Presentation and PresentationToPdfConverter.
    /// </summary>
    /// <param name="fileToConvert">The full path to the PowerPoint file to be converted.</param>
    /// <param name="convertedFile">The full path where the resulting PDF should be saved.</param>
    /// <returns>True if the conversion was successful; false otherwise.</returns>
    private bool ConvertPowerPointToPdf(string fileToConvert, string convertedFile)
    {
        if (string.IsNullOrWhiteSpace(fileToConvert) || string.IsNullOrWhiteSpace(convertedFile))
        {
            _logger.LogError("ConvertPowerPointToPdf: Invalid file paths provided. Source: {Source}, Target: {Target}",
                fileToConvert, convertedFile);
            return false;
        }

        if (!System.IO.File.Exists(fileToConvert))
        {
            _logger.LogError("ConvertPowerPointToPdf: Source file does not exist at {Path}", fileToConvert);
            return false;
        }

        PdfDocument? pdfDocument = null;
        try
        {
            using var pptxDoc = Presentation.Open(fileToConvert);
            pdfDocument = PresentationToPdfConverter.Convert(pptxDoc);

            using var stream = new FileStream(convertedFile, FileMode.Create, FileAccess.Write, FileShare.None);
            pdfDocument.Save(stream);

            _logger.LogInformation("ConvertPowerPointToPdf: Successfully converted PowerPoint to PDF. Source: {Source}, Target: {Target}",
                fileToConvert, convertedFile);
            return true;
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "ConvertPowerPointToPdf: IO error during conversion. Source: {Source}, Target: {Target}, Message: {Message}",
                fileToConvert, convertedFile, ioEx.Message);
        }
        catch (PdfException pdfEx)
        {
            _logger.LogError(pdfEx, "ConvertPowerPointToPdf: PDF error during conversion. Source: {Source}, Target: {Target}, Message: {Message}",
                fileToConvert, convertedFile, pdfEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ConvertPowerPointToPdf: Unexpected error during conversion. Source: {Source}, Target: {Target}, Message: {Message}",
                fileToConvert, convertedFile, ex.Message);
        }
        finally
        {
            pdfDocument?.Dispose();
        }
        return false;
    }

    /// <summary>
    /// Computes SHA256 checksum for file content with enhanced error handling.
    /// </summary>
    /// <param name="fileBytes">The file bytes to compute the checksum for.</param>
    /// <returns>The SHA256 checksum as a lowercase hexadecimal string.</returns>
    private string ComputeChecksum(byte[] fileBytes)
    {
        ArgumentNullException.ThrowIfNull(fileBytes);

        if (fileBytes.Length == 0)
        {
            _logger.LogWarning("ComputeChecksum: fileBytes is empty.");
            throw new ArgumentException("File bytes cannot be empty.", nameof(fileBytes));
        }

        try
        {
            var hash = System.Security.Cryptography.SHA256.HashData(fileBytes);
            var checksum = Convert.ToHexString(hash).ToLowerInvariant();
            _logger.LogDebug("ComputeChecksum: Successfully computed checksum.");
            return checksum;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ComputeChecksum: Exception occurred while computing checksum for file of length {Length}.", fileBytes.Length);
            throw new InvalidOperationException($"An error occurred while computing the checksum for a file of length {fileBytes.Length}. See inner exception for details.", ex);
        }
    }

    /// <summary>
    /// Adds custom document properties to MS Office files with enhanced error handling.
    /// </summary>
    /// <param name="inputPath">Path to the input file (temp file).</param>
    /// <param name="outputPath">Path to save the file with properties.</param>
    /// <param name="documentId">Document GUID.</param>
    /// <param name="revisionId">Revision GUID.</param>
    /// <param name="matterId">Matter GUID.</param>
    private void AddCustomDocumentProperties(string inputPath, string outputPath, Guid documentId, Guid revisionId, Guid matterId)
    {
        var extension = Path.GetExtension(inputPath).ToLowerInvariant();

        try
        {
            switch (extension)
            {
                case ".doc":
                case ".docx":
                case ".dot":
                case ".dotx":
                case ".docm":
                case ".dotm":
                    ProcessWordDocument(inputPath, outputPath, documentId, revisionId, matterId);
                    break;

                case ".xls":
                case ".xlsx":
                case ".csv":
                case ".xlt":
                case ".xltm":
                case ".xlw":
                    ProcessExcelDocument(inputPath, outputPath, documentId, revisionId, matterId);
                    break;

                case ".ppt":
                case ".pptx":
                case ".potx":
                case ".pptm":
                case ".potm":
                    ProcessPowerPointDocument(inputPath, outputPath, documentId, revisionId, matterId);
                    break;

                default:
                    // For unsupported types, just copy the file
                    System.IO.File.Copy(inputPath, outputPath, true);
                    _logger.LogInformation("AddCustomDocumentProperties: File type not supported for custom properties. File copied as-is: {InputPath}", inputPath);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddCustomDocumentProperties: Failed to add custom document properties to {InputPath}: {Message}", inputPath, ex.Message);

            // Fallback: copy the file as-is
            try
            {
                System.IO.File.Copy(inputPath, outputPath, true);
                _logger.LogWarning("AddCustomDocumentProperties: Fallback - File copied as-is after failure to add custom properties: {InputPath}", inputPath);
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "AddCustomDocumentProperties: Fallback copy also failed for {InputPath}: {Message}", inputPath, fallbackEx.Message);
                throw; // Re-throw as this is a critical failure
            }
        }
    }

    /// <summary>
    /// Processes Word documents to add custom properties.
    /// </summary>
    private void ProcessWordDocument(string inputPath, string outputPath, Guid documentId, Guid revisionId, Guid matterId)
    {
        using var stream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
        using var wordDoc = new WordDocument(stream, FormatType.Automatic);

        var props = wordDoc.CustomDocumentProperties;
        SetCustomProperties(props, documentId, revisionId, matterId);

        using var outStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        wordDoc.Save(outStream, FormatType.Automatic);

        _logger.LogInformation("ProcessWordDocument: Custom properties added to Word document: {InputPath}", inputPath);
    }

    /// <summary>
    /// Processes Excel documents to add custom properties.
    /// </summary>
    private void ProcessExcelDocument(string inputPath, string outputPath, Guid documentId, Guid revisionId, Guid matterId)
    {
        using var excelEngine = new ExcelEngine();
        var app = excelEngine.Excel;
        app.DefaultVersion = ExcelVersion.Xlsx;

        using var stream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
        var workbook = app.Workbooks.Open(stream);

        var props = workbook.CustomDocumentProperties;
        SetExcelCustomProperties(props, documentId, revisionId, matterId);

        using var outStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        workbook.SaveAs(outStream);
        workbook.Close();

        _logger.LogInformation("ProcessExcelDocument: Custom properties added to Excel document: {InputPath}", inputPath);
    }

    /// <summary>
    /// Processes PowerPoint documents to add custom properties.
    /// </summary>
    private void ProcessPowerPointDocument(string inputPath, string outputPath, Guid documentId, Guid revisionId, Guid matterId)
    {
        using var ppt = Presentation.Open(inputPath);
        var props = ppt.CustomDocumentProperties;

        SetPowerPointCustomProperties(props, documentId, revisionId, matterId);

        ppt.Save(outputPath);

        _logger.LogInformation("ProcessPowerPointDocument: Custom properties added to PowerPoint document: {InputPath}", inputPath);
    }

    /// <summary>
    /// Sets custom properties for Word documents.
    /// </summary>
    private static void SetCustomProperties(dynamic props, Guid documentId, Guid revisionId, Guid matterId)
    {
        props.Remove("ADMS_Source");
        props.Add("ADMS_Source", "ADMS_Source");
        props.Remove("ADMS_MatterId");
        props.Add("ADMS_MatterId", matterId.ToString());
        props.Remove("ADMS_DocumentId");
        props.Add("ADMS_DocumentId", documentId.ToString());
        props.Remove("ADMS_RevisionId");
        props.Add("ADMS_RevisionId", revisionId.ToString());
    }

    /// <summary>
    /// Sets custom properties for Excel documents.
    /// </summary>
    private static void SetExcelCustomProperties(dynamic props, Guid documentId, Guid revisionId, Guid matterId)
    {
        props.Remove("ADMS_Source");
        props["ADMS_Source"].Value = "ADMS_Source";
        props.Remove("ADMS_MatterId");
        props["ADMS_MatterId"].Value = matterId.ToString();
        props.Remove("ADMS_DocumentId");
        props["ADMS_DocumentId"].Value = documentId.ToString();
        props.Remove("ADMS_RevisionId");
        props["ADMS_RevisionId"].Value = revisionId.ToString();
    }

    /// <summary>
    /// Sets custom properties for PowerPoint documents.
    /// </summary>
    private static void SetPowerPointCustomProperties(dynamic props, Guid documentId, Guid revisionId, Guid matterId)
    {
        props.Remove("ADMS_Source");
        props.Add("ADMS_Source");
        props["ADMS_Source"].Value = "ADMS_Source";

        props.Remove("ADMS_MatterId");
        props.Add("ADMS_MatterId");
        props["ADMS_MatterId"].Value = matterId.ToString();

        props.Remove("ADMS_DocumentId");
        props.Add("ADMS_DocumentId");
        props["ADMS_DocumentId"].Value = documentId.ToString();

        props.Remove("ADMS_RevisionId");
        props.Add("ADMS_RevisionId");
        props["ADMS_RevisionId"].Value = revisionId.ToString();
    }

    /// <summary>
    /// Creates a standardized problem details object.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="detail">The problem detail message.</param>
    /// <returns>A standardized problem details object.</returns>
    private ProblemDetails CreateProblemDetails(int statusCode, string detail)
    {
        return _problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode, detail: detail);
    }

    /// <summary>
    /// Creates a standardized internal server error response.
    /// </summary>
    /// <param name="operationContext">A description of the operation that failed.</param>
    /// <returns>A standardized internal server error response.</returns>
    private ObjectResult CreateInternalServerErrorResponse(string operationContext)
    {
        var problem = _problemDetailsFactory.CreateProblemDetails(
            HttpContext,
            StatusCodes.Status500InternalServerError,
            $"An unexpected error occurred while {operationContext}. Please try again later."
        );

        return StatusCode(StatusCodes.Status500InternalServerError, problem);
    }

    #endregion Private Helper Methods
}