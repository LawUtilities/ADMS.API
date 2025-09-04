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
    /// Unit tests for <see cref="DocumentController.DeleteDocumentAsync"/>.
    /// </summary>
    public class DocumentControllerDeleteDocumentAsyncTests
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
        /// Tests that DeleteDocumentAsync returns BadRequest when matterId is empty.
        /// </summary>
        [Fact]
        public async Task DeleteDocumentAsync_ReturnsBadRequest_WhenMatterIdEmpty()
        {
            var controller = CreateController();
            var result = await controller.DeleteDocumentAsync(Guid.Empty, Guid.NewGuid());
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that DeleteDocumentAsync returns BadRequest when documentId is empty.
        /// </summary>
        [Fact]
        public async Task DeleteDocumentAsync_ReturnsBadRequest_WhenDocumentIdEmpty()
        {
            var controller = CreateController();
            var result = await controller.DeleteDocumentAsync(Guid.NewGuid(), Guid.Empty);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Existence and Validation

        /// <summary>
        /// Tests that DeleteDocumentAsync returns NotFound when matter or document does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteDocumentAsync_ReturnsNotFound_WhenMatterOrDocumentNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());
            var result = await controller.DeleteDocumentAsync(Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that DeleteDocumentAsync returns NotFound when document is not found in repository.
        /// </summary>
        [Fact]
        public async Task DeleteDocumentAsync_ReturnsNotFound_WhenDocumentNotFoundInRepository()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetDocumentAsync(It.IsAny<Guid>(), false, false))
                .ReturnsAsync((Entities.Document?)null);
            var result = await controller.DeleteDocumentAsync(Guid.NewGuid(), Guid.NewGuid());
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
        }

        #endregion

        #region Operation and Success

        /// <summary>
        /// Tests that DeleteDocumentAsync returns NoContent when deletion is successful.
        /// </summary>
        [Fact]
        public async Task DeleteDocumentAsync_ReturnsNoContent_WhenSuccessful()
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
            _repoMock.Setup(r => r.DeleteDocumentAsync(documentDto))
                .ReturnsAsync(true);
            _repoMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(true);

            var result = await controller.DeleteDocumentAsync(matterId, documentId);
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Tests that DeleteDocumentAsync returns InternalServerError when deletion fails.
        /// </summary>
        [Fact]
        public async Task DeleteDocumentAsync_ReturnsInternalServerError_WhenDeleteFails()
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
            _repoMock.Setup(r => r.DeleteDocumentAsync(documentDto))
                .ReturnsAsync(false);
            _repoMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(true);

            var result = await controller.DeleteDocumentAsync(matterId, documentId);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that DeleteDocumentAsync returns InternalServerError when save changes fails after deletion.
        /// </summary>
        [Fact]
        public async Task DeleteDocumentAsync_ReturnsInternalServerError_WhenSaveChangesFails()
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
            _repoMock.Setup(r => r.DeleteDocumentAsync(documentDto))
                .ReturnsAsync(true);
            _repoMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(false);

            var result = await controller.DeleteDocumentAsync(matterId, documentId);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Tests that DeleteDocumentAsync returns InternalServerError when an exception is thrown during the operation.
        /// </summary>
        [Fact]
        public async Task DeleteDocumentAsync_ReturnsInternalServerError_OnException()
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

            var result = await controller.DeleteDocumentAsync(matterId, documentId);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}