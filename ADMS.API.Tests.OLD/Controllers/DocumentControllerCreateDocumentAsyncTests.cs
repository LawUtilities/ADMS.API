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

using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="DocumentController.CreateDocumentAsync"/>.
    /// </summary>
    public class DocumentControllerCreateDocumentAsyncTests
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
        /// Creates and initializes a new instance of the <see cref="DocumentController"/> class with mocked
        /// dependencies for testing purposes.
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
        /// Tests that CreateDocumentAsync returns BadRequest when matterId is empty.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsBadRequest_WhenMatterIdEmpty()
        {
            var controller = CreateController();
            var result = await controller.CreateDocumentAsync(Guid.Empty, new DocumentForCreationDto
            {
                FileName = "Test File",
                Extension = "txt"
            }, Mock.Of<IFormFile>(), CancellationToken.None);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that CreateDocumentAsync returns BadRequest when document is null.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsBadRequest_WhenDocumentNull()
        {
            var controller = CreateController();
            var result = await controller.CreateDocumentAsync(Guid.NewGuid(), null, Mock.Of<IFormFile>(), CancellationToken.None);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that CreateDocumentAsync returns BadRequest when fileUpload is null.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsBadRequest_WhenFileNull()
        {
            var controller = CreateController();
            var result = await controller.CreateDocumentAsync(Guid.NewGuid(), new DocumentForCreationDto
            {
                FileName = "test file",
                Extension = "txt"
            }, null, CancellationToken.None);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that CreateDocumentAsync returns BadRequest when fileUpload is empty.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsBadRequest_WhenFileEmpty()
        {
            var controller = CreateController();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);
            var result = await controller.CreateDocumentAsync(Guid.NewGuid(), new DocumentForCreationDto
            {
                FileName = "test file",
                Extension = "txt"
            }, fileMock.Object, CancellationToken.None);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that CreateDocumentAsync returns PayloadTooLarge when file exceeds max size.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsPayloadTooLarge_WhenFileTooLarge()
        {
            var controller = CreateController();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(51 * 1024 * 1024); // 51 MB
            var result = await controller.CreateDocumentAsync(Guid.NewGuid(), new DocumentForCreationDto
            {
                FileName = "test file",
                Extension = "txt"
            }, fileMock.Object, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status413PayloadTooLarge, objectResult.StatusCode);
        }

        #endregion

        #region Existence and Business Validation

        /// <summary>
        /// Tests that CreateDocumentAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.CreateDocumentAsync(Guid.NewGuid(), new DocumentForCreationDto
            {
                FileName = "test file",
                Extension = "txt"
            }, fileMock.Object, CancellationToken.None);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Tests that CreateDocumentAsync returns BadRequest when document business validation fails.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsBadRequest_WhenBusinessValidationFails()
        {
            var controller = CreateController();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentForCreationAsync(It.IsAny<Guid>(), It.IsAny<DocumentForCreationDto>()))
                .ReturnsAsync((List<ValidationResult>) [new ValidationResult("Duplicate")]);

            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>((stream, _) => { stream.Write(new byte[100]); return Task.CompletedTask; });

            var result = await controller.CreateDocumentAsync(Guid.NewGuid(), new DocumentForCreationDto
            {
                FileName = "test file",
                Extension = "txt"
            }, fileMock.Object, CancellationToken.None);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        #endregion

        #region File Validation

        /// <summary>
        /// Tests that CreateDocumentAsync returns UnsupportedMediaType when file type is not allowed.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsUnsupportedMediaType_WhenFileTypeNotAllowed()
        {
            var controller = CreateController();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);

            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>((stream, _) => { stream.Write(new byte[100]); return Task.CompletedTask; });

            var result = await controller.CreateDocumentAsync(Guid.NewGuid(), new DocumentForCreationDto
            {
                FileName = "test file",
                Extension = "txt"
            }, fileMock.Object, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status415UnsupportedMediaType, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that CreateDocumentAsync returns UnsupportedMediaType when file extension does not match detected type.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsUnsupportedMediaType_WhenExtensionMismatch()
        {
            var controller = CreateController();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns("test.docx");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);

            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>((stream, _) => { stream.Write(new byte[100]); return Task.CompletedTask; });

            var result = await controller.CreateDocumentAsync(Guid.NewGuid(), new DocumentForCreationDto
            {
                FileName = "test file",
                Extension = "docx"
            }, fileMock.Object, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status415UnsupportedMediaType, objectResult.StatusCode);
        }

        #endregion

        #region Virus Scanning

        /// <summary>
        /// Tests that CreateDocumentAsync returns UnprocessableEntity when virus is detected.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsUnprocessableEntity_WhenVirusDetected()
        {
            var controller = CreateController();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentForCreationAsync(It.IsAny<Guid>(), It.IsAny<DocumentForCreationDto>()))
                .ReturnsAsync((List<ValidationResult>) []);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>((stream, _) => { stream.Write(new byte[100]); return Task.CompletedTask; });

            // Simulate virus detected
            _virusScannerMock.Setup(v => v.ScanFileForVirusesAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new VirusScanResult { IsClean = false });

            var result = await controller.CreateDocumentAsync(Guid.NewGuid(), new DocumentForCreationDto
            {
                FileName = "test file",
                Extension = "txt"
            }, fileMock.Object, CancellationToken.None);

            var unprocessableResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, unprocessableResult.StatusCode);
        }

        #endregion

        #region Persistence and Success

        /// <summary>
        /// Tests that CreateDocumentAsync returns CreatedAtRoute when document is created successfully.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsCreatedAtRoute_WhenSuccessful()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var docId = Guid.NewGuid();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentForCreationAsync(matterId, It.IsAny<DocumentForCreationDto>()))
                .ReturnsAsync((List<ValidationResult>) []);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>((stream, _) => { stream.Write(new byte[100]); return Task.CompletedTask; });

            var matter = new Matter()
            {
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };

            var createdDoc = new Document
            {
                FileName = "test file",
                Extension = "txt",
                MatterId = matter.Id,
                Matter = matter
            };

            _repoMock.Setup(r => r.AddDocumentAsync(matterId, It.IsAny<DocumentForCreationDto>()))
                .ReturnsAsync(createdDoc);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<DocumentDto>(createdDoc))
                .Returns(new DocumentDto
                {
                    Id = docId,
                    FileName = "test file",
                    Extension = "txt"
                });

            var result = await controller.CreateDocumentAsync(matterId, new DocumentForCreationDto
            {
                FileName = "test file",
                Extension = "txt"
            }, fileMock.Object, CancellationToken.None);
            var createdAtRoute = Assert.IsType<CreatedAtRouteResult>(result.Result);
            Assert.Equal("GetDocument", createdAtRoute.RouteName);
        }

        /// <summary>
        /// Tests that CreateDocumentAsync returns InternalServerError when AddDocumentAsync fails.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsInternalServerError_WhenAddDocumentFails()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentForCreationAsync(matterId, It.IsAny<DocumentForCreationDto>()))
                .ReturnsAsync((List<ValidationResult>) []);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>((stream, _) => { stream.Write(new byte[100]); return Task.CompletedTask; });

            _repoMock.Setup(r => r.AddDocumentAsync(matterId, It.IsAny<DocumentForCreationDto>()))
                .ReturnsAsync((Document?)null);

            var result = await controller.CreateDocumentAsync(matterId, new DocumentForCreationDto
            {
                FileName = "test file",
                Extension = "txt"
            }, fileMock.Object, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that CreateDocumentAsync returns InternalServerError when SaveChangesAsync fails.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsInternalServerError_WhenSaveChangesFails()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentForCreationAsync(matterId, It.IsAny<DocumentForCreationDto>()))
                .ReturnsAsync((List<ValidationResult>) []);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>((stream, _) => { stream.Write(new byte[100]); return Task.CompletedTask; });

            var matter = new Matter()
            {
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };

            var createdDoc = new Document
            {
                FileName = "test file",
                Extension = "txt",
                MatterId = matter.Id,
                Matter = matter
            };
            _repoMock.Setup(r => r.AddDocumentAsync(matterId, It.IsAny<DocumentForCreationDto>()))
                .ReturnsAsync(createdDoc);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);

            var result = await controller.CreateDocumentAsync(matterId, new DocumentForCreationDto
            {
                FileName = "test file",
                Extension = "txt"
            }, fileMock.Object, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Tests that CreateDocumentAsync returns InternalServerError on file read exception.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsInternalServerError_OnFileReadException()
        {
            var controller = CreateController();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("Read error"));
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);

            var result = await controller.CreateDocumentAsync(Guid.NewGuid(), new DocumentForCreationDto
            {
                FileName = "test file",
                Extension = "txt"
            }, fileMock.Object, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that CreateDocumentAsync returns InternalServerError on virus scan exception.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsInternalServerError_OnVirusScanException()
        {
            var controller = CreateController();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentForCreationAsync(It.IsAny<Guid>(), It.IsAny<DocumentForCreationDto>()))
                .ReturnsAsync((List<ValidationResult>) []);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>((stream, _) => { stream.Write(new byte[100]); return Task.CompletedTask; });

            // Simulate virus scanner throwing an exception
            _virusScannerMock.Setup(v => v.ScanFileForVirusesAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Virus scan service unavailable"));

            var result = await controller.CreateDocumentAsync(Guid.NewGuid(), new DocumentForCreationDto
            {
                FileName = "test file",
                Extension = "txt"
            }, fileMock.Object, CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that CreateDocumentAsync returns InternalServerError on file save exception.
        /// </summary>
        [Fact]
        public async Task CreateDocumentAsync_ReturnsInternalServerError_OnFileSaveException()
        {
            var controller = CreateController();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentForCreationAsync(It.IsAny<Guid>(), It.IsAny<DocumentForCreationDto>()))
                .ReturnsAsync((List<ValidationResult>) []);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>((stream, _) => { stream.Write(new byte[100]); return Task.CompletedTask; });

            // Simulate file save throwing an exception
            _fileStorageMock.Setup(f => f.SaveFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("Disk error"));

            var result = await controller.CreateDocumentAsync(Guid.NewGuid(), new DocumentForCreationDto
            {
                FileName = "test file",
                Extension = "txt"
            }, fileMock.Object, CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}