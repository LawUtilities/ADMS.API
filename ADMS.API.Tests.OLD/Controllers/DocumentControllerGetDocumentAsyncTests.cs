using ADMS.API.Controllers;
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
    /// Unit tests for <see cref="DocumentController.GetDocumentAsync"/>.
    /// </summary>
    public class DocumentControllerGetDocumentAsyncTests
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
        /// Tests that GetDocumentAsync returns BadRequest when matterId is empty.
        /// </summary>
        [Fact]
        public async Task GetDocumentAsync_ReturnsBadRequest_WhenMatterIdEmpty()
        {
            var controller = CreateController();
            var result = await controller.GetDocumentAsync(Guid.Empty, Guid.NewGuid(), null);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that GetDocumentAsync returns BadRequest when documentId is empty.
        /// </summary>
        [Fact]
        public async Task GetDocumentAsync_ReturnsBadRequest_WhenDocumentIdEmpty()
        {
            var controller = CreateController();
            var result = await controller.GetDocumentAsync(Guid.NewGuid(), Guid.Empty, null);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that GetDocumentAsync returns BadRequest when fields parameter is invalid.
        /// </summary>
        [Fact]
        public async Task GetDocumentAsync_ReturnsBadRequest_WhenFieldsInvalid()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var fields = "invalidField";

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);
            _propertyCheckerServiceMock.Setup(p => p.TypeHasProperties<DocumentDto>(fields))
                .Returns(false);

            var result = await controller.GetDocumentAsync(matterId, documentId, fields);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Existence and Validation

        /// <summary>
        /// Tests that GetDocumentAsync returns NotFound when matter or document does not exist.
        /// </summary>
        [Fact]
        public async Task GetDocumentAsync_ReturnsNotFound_WhenMatterOrDocumentNotFound()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetDocumentAsync(matterId, documentId, null);
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that GetDocumentAsync returns NotFound when document is not found in repository.
        /// </summary>
        [Fact]
        public async Task GetDocumentAsync_ReturnsNotFound_WhenDocumentNotFoundInRepository()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetDocumentAsync(documentId, false, false))
                .ReturnsAsync((Entities.Document?)null);

            var result = await controller.GetDocumentAsync(matterId, documentId, null);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
        }

        #endregion

        #region Operation and Success

        /// <summary>
        /// Tests that GetDocumentAsync returns Ok with mapped document when all parameters are valid.
        /// </summary>
        [Fact]
        public async Task GetDocumentAsync_ReturnsOk_WithDocument()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            var documentEntity = new Entities.Document
            {
                Id = documentId,
                MatterId = matterId,
                Matter = new Entities.Matter
                {
                    Id = matterId,
                    Description = "test matter",
                    CreationDate = DateTime.UtcNow
                },
                FileName = "test file",
                Extension = ".txt"
            };
            var documentDto = new DocumentDto
            {
                Id = documentId,
                FileName = "test file",
                Extension = ".txt"
            };

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetDocumentAsync(documentId, false, false))
                .ReturnsAsync(documentEntity);
            _mapperMock.Setup(m => m.Map<DocumentDto>(documentEntity))
                .Returns(documentDto);

            var result = await controller.GetDocumentAsync(matterId, documentId, null);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<DocumentDto>(okResult.Value);
            Assert.Equal(documentId, returned.Id);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Tests that GetDocumentAsync returns InternalServerError when an exception is thrown during the operation.
        /// </summary>
        [Fact]
        public async Task GetDocumentAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetDocumentAsync(documentId, false, false))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await controller.GetDocumentAsync(matterId, documentId, null);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}