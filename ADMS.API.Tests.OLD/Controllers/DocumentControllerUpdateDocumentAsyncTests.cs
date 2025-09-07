using System.ComponentModel.DataAnnotations;
using ADMS.API.Controllers;
using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.Services;

using MapsterMapper;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

using Moq;

namespace ADMS.API.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="DocumentController.UpdateDocumentAsync"/>.
    /// </summary>
    public class DocumentControllerUpdateDocumentAsyncTests
    {
        private readonly Mock<ILogger<DocumentController>> _loggerMock = new();
        private readonly Mock<IAdmsRepository> _repoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IPropertyCheckerService> _propertyCheckerServiceMock = new();
        private readonly Mock<ProblemDetailsFactory> _problemDetailsFactoryMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();
        private readonly Mock<IVirusScanner> _virusScannerMock = new();
        private readonly Mock<IFileStorage> _fileStorageMock = new();

        /// <summary>
        /// Creates and initializes a new instance of the <see cref="DocumentController"/> class with mocked dependencies for testing purposes.
        /// </summary>
        /// <returns>A fully initialized <see cref="DocumentController"/> instance with a mocked <see cref="ControllerContext"/>
        /// and <see cref="DefaultHttpContext"/>.</returns>
        private DocumentController CreateController()
        {
            var controller = new DocumentController(
                _loggerMock.Object,
                _repoMock.Object,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _propertyCheckerServiceMock.Object,
                _problemDetailsFactoryMock.Object,
                _validationServiceMock.Object,
                _virusScannerMock.Object,
                _fileStorageMock.Object
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            return controller;
        }

        #region Validation and Input Checks

        /// <summary>
        /// Tests that UpdateDocumentAsync returns BadRequest when matterId is empty.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsBadRequest_WhenMatterIdEmpty()
        {
            var controller = CreateController();
            var result = await controller.UpdateDocumentAsync(Guid.Empty, Guid.NewGuid(), new DocumentForUpdateDto
            {
                FileName = "test file",
                Extension = ".txt"
            }, null, CancellationToken.None);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that UpdateDocumentAsync returns BadRequest when documentId is empty.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsBadRequest_WhenDocumentIdEmpty()
        {
            var controller = CreateController();
            var result = await controller.UpdateDocumentAsync(Guid.NewGuid(), Guid.Empty, new DocumentForUpdateDto
            {
                FileName = "test file",
                Extension = ".txt"
            }, null, CancellationToken.None);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Existence and Validation

        /// <summary>
        /// Tests that UpdateDocumentAsync returns NotFound when matter or document does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsNotFound_WhenMatterOrDocumentNotFound()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.UpdateDocumentAsync(matterId, documentId, new DocumentForUpdateDto
            {
                FileName = "test file",
                Extension = ".txt"
            }, null, CancellationToken.None);
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region File Upload Validation

        /// <summary>
        /// Tests that UpdateDocumentAsync returns PayloadTooLarge when file exceeds max size.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsPayloadTooLarge_WhenFileTooLarge()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(51 * 1024 * 1024); // 51 MB

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            var result = await controller.UpdateDocumentAsync(matterId, documentId, new DocumentForUpdateDto
            {
                FileName = "test file",
                Extension = ".txt"
            }, fileMock.Object, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status413PayloadTooLarge, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that UpdateDocumentAsync returns InternalServerError when file read fails.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsInternalServerError_WhenFileReadFails()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None))
                .ThrowsAsync(new Exception("Read error"));

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            var result = await controller.UpdateDocumentAsync(matterId, documentId, new DocumentForUpdateDto
            {
                FileName = "test file",
                Extension = ".txt"
            }, fileMock.Object, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that UpdateDocumentAsync returns UnprocessableEntity when virus is detected.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsUnprocessableEntity_WhenVirusDetected()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None))
                .Callback<Stream, CancellationToken>((stream, _) => stream.Write(new byte[1024]));
            _virusScannerMock.Setup(v => v.ScanFileForVirusesAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new VirusScanResult { IsClean = false });

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            var result = await controller.UpdateDocumentAsync(matterId, documentId, new DocumentForUpdateDto
            {
                FileName = "test file",
                Extension = ".txt"
            }, fileMock.Object, CancellationToken.None);
            var unprocessable = Assert.IsType<UnprocessableEntityObjectResult>(result);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, unprocessable.StatusCode);
        }

        /// <summary>
        /// Tests that UpdateDocumentAsync returns InternalServerError when virus scan throws.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsInternalServerError_WhenVirusScanThrows()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None))
                .Callback<Stream, CancellationToken>((stream, _) => stream.Write(new byte[1024]));
            _virusScannerMock.Setup(v => v.ScanFileForVirusesAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Virus scan service unavailable"));

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            var result = await controller.UpdateDocumentAsync(matterId, documentId, new DocumentForUpdateDto
            {
                FileName = "test file",
                Extension = ".txt"
            }, fileMock.Object, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that UpdateDocumentAsync returns UnsupportedMediaType when file extension is not allowed.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsUnsupportedMediaType_WhenExtensionNotAllowed()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("test.exe");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None))
                .Callback<Stream, CancellationToken>((stream, _) => stream.Write(new byte[1024]));
            _virusScannerMock.Setup(v => v.ScanFileForVirusesAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new VirusScanResult { IsClean = false });
            // Simulate extension not allowed

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            var result = await controller.UpdateDocumentAsync(matterId, documentId, new DocumentForUpdateDto
            {
                FileName = "test file",
                Extension = ".txt"
            }, fileMock.Object, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status415UnsupportedMediaType, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that UpdateDocumentAsync returns UnsupportedMediaType when file type is not allowed.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsUnsupportedMediaType_WhenFileTypeNotAllowed()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None))
                .Callback<Stream, CancellationToken>((stream, _) => stream.Write(new byte[1024]));
            _virusScannerMock.Setup(v => v.ScanFileForVirusesAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new VirusScanResult { IsClean = false });

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            var result = await controller.UpdateDocumentAsync(matterId, documentId, new DocumentForUpdateDto
            {
                FileName = "test file",
                Extension = ".txt"
            }, fileMock.Object, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status415UnsupportedMediaType, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that UpdateDocumentAsync returns UnsupportedMediaType when file extension does not match detected type.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsUnsupportedMediaType_WhenExtensionMismatch()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("test.docx");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None))
                .Callback<Stream, CancellationToken>((stream, _) => stream.Write(new byte[1024]));
            _virusScannerMock.Setup(v => v.ScanFileForVirusesAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new VirusScanResult { IsClean = false });

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            var result = await controller.UpdateDocumentAsync(matterId, documentId, new DocumentForUpdateDto
            {
                FileName = "test file",
                Extension = ".txt"
            }, fileMock.Object, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status415UnsupportedMediaType, objectResult.StatusCode);
        }

        #endregion

        #region Model and Business Validation

        /// <summary>
        /// Tests that UpdateDocumentAsync returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            controller.ModelState.AddModelError("Test", "Invalid");

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            var result = await controller.UpdateDocumentAsync(matterId, documentId, new DocumentForUpdateDto
            {
                FileName = "test file",
                Extension = ".txt"
            }, null, CancellationToken.None);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that UpdateDocumentAsync returns BadRequest when business validation fails.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsBadRequest_WhenBusinessValidationFails()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var validationResults = new List<ValidationResult>
            {
                new("Invalid")
            };

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentForUpdate(It.IsAny<DocumentForUpdateDto>()))
                .Returns(validationResults);

            var result = await controller.UpdateDocumentAsync(matterId, documentId, new DocumentForUpdateDto
            {
                FileName = "test file",
                Extension = ".txt"
            }, null, CancellationToken.None);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Operation and Success

        /// <summary>
        /// Tests that UpdateDocumentAsync returns NotFound when document is not found in repository.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsNotFound_WhenDocumentNotFoundInRepository()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentForUpdate(It.IsAny<DocumentForUpdateDto>()))
                .Returns((List<ValidationResult>) []);
            _repoMock.Setup(r => r.GetDocumentAsync(documentId, false, false))
                .ReturnsAsync((Document?)null);

            var result = await controller.UpdateDocumentAsync(matterId, documentId, new DocumentForUpdateDto
            {
                FileName = "test file",
                Extension = ".txt"
            }, null, CancellationToken.None);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
        }

        /// <summary>
        /// Tests that UpdateDocumentAsync returns NoContent when update is successful.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsNoContent_WhenSuccessful()
        {
            var controller = CreateController();

            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };

            var documentId = Guid.NewGuid();
            
            var documentEntity = new Document
            {
                FileName = "test file",
                Extension = ".txt",
                MatterId = matter.Id,
                Matter = matter
            };

            var updateDto = new DocumentForUpdateDto
            {
                FileName = "test file 2",
                Extension = ".txt"
            };

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentForUpdate(It.IsAny<DocumentForUpdateDto>()))
                .Returns((List<ValidationResult>) []);
            _repoMock.Setup(r => r.GetDocumentAsync(documentId, false, false))
                .ReturnsAsync(documentEntity);
            _repoMock.Setup(r => r.UpdateDocumentAsync(documentId, It.IsAny<DocumentForUpdateDto>()))
                .ReturnsAsync(documentEntity);
            _repoMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(true);

            var result = await controller.UpdateDocumentAsync(matter.Id, documentId, updateDto, null, CancellationToken.None);
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Tests that UpdateDocumentAsync returns BadRequest when SaveChangesAsync returns false.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsBadRequest_WhenSaveChangesFails()
        {
            var controller = CreateController();

            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var documentId = Guid.NewGuid();
            var documentEntity = new Document
            {
                FileName = "test file",
                Extension = ".txt",
                MatterId = matter.Id,
                Matter = matter
            };
            var updateDto = new DocumentForUpdateDto
            {
                FileName = "test file 2",
                Extension = ".txt"
            };

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentForUpdate(It.IsAny<DocumentForUpdateDto>()))
                .Returns((List<ValidationResult>)[]);
            _repoMock.Setup(r => r.GetDocumentAsync(documentId, false, false))
                .ReturnsAsync(documentEntity);
            _repoMock.Setup(r => r.UpdateDocumentAsync(documentId, It.IsAny<DocumentForUpdateDto>()))
                .ReturnsAsync(documentEntity);
            _repoMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(false);

            var result = await controller.UpdateDocumentAsync(matter.Id, documentId, updateDto, null, CancellationToken.None);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Tests that UpdateDocumentAsync returns InternalServerError when an exception is thrown during the operation.
        /// </summary>
        [Fact]
        public async Task UpdateDocumentAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            var matter = new Matter
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var documentId = Guid.NewGuid();
            var documentEntity = new Document
            {
                FileName = "test file",
                Extension = ".txt",
                MatterId = matter.Id,
                Matter = matter
            };
            var updateDto = new DocumentForUpdateDto
            {
                FileName = "test file 2",
                Extension = ".txt"
            };

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentForUpdate(It.IsAny<DocumentForUpdateDto>()))
                .Returns((List<ValidationResult>)[]);
            _repoMock.Setup(r => r.GetDocumentAsync(documentId, false, false))
                .ReturnsAsync(documentEntity);
            _repoMock.Setup(r => r.UpdateDocumentAsync(documentId, It.IsAny<DocumentForUpdateDto>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await controller.UpdateDocumentAsync(matter.Id, documentId, updateDto, null, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}