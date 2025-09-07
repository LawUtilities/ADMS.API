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
    /// Unit tests for <see cref="DocumentController.CopyDocumentAsync"/>.
    /// </summary>
    public class DocumentControllerCopyDocumentAsyncTests
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
        /// Tests that CopyDocumentAsync returns BadRequest when matterId is empty.
        /// </summary>
        [Fact]
        public async Task CopyDocumentAsync_ReturnsBadRequest_WhenMatterIdEmpty()
        {
            var controller = CreateController();
            var result = await controller.CopyDocumentAsync(Guid.Empty, new DocumentWithoutRevisionsDto
            {
                Id = Guid.NewGuid(),
                FileName = "test file",
                Extension = ".txt"
            });
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that CopyDocumentAsync returns BadRequest when document is null.
        /// </summary>
        [Fact]
        public async Task CopyDocumentAsync_ReturnsBadRequest_WhenDocumentNull()
        {
            var controller = CreateController();
            var result = await controller.CopyDocumentAsync(Guid.NewGuid(), null);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that CopyDocumentAsync returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task CopyDocumentAsync_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("Test", "Invalid");
            var result = await controller.CopyDocumentAsync(Guid.NewGuid(), new DocumentWithoutRevisionsDto
            {
                Id = Guid.NewGuid(),
                FileName = "test file",
                Extension = ".txt"
            });
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        #endregion

        #region Existence and Validation

        /// <summary>
        /// Tests that CopyDocumentAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task CopyDocumentAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());
            var result = await controller.CopyDocumentAsync(Guid.NewGuid(), new DocumentWithoutRevisionsDto
            {
                Id = Guid.NewGuid(),
                FileName = "test file",
                Extension = ".txt"
            });
            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Tests that CopyDocumentAsync returns NotFound when target matter is not found in repository.
        /// </summary>
        [Fact]
        public async Task CopyDocumentAsync_ReturnsNotFound_WhenTargetMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), false, false))
                    .ReturnsAsync((Entities.Matter?)null);
            var doc = new DocumentWithoutRevisionsDto
            {
                Id = Guid.NewGuid(),
                FileName = "test file",
                Extension = ".txt"
            };
            var result = await controller.CopyDocumentAsync(Guid.NewGuid(), doc);
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
        }

        #endregion

        #region Operation and Success

        /// <summary>
        /// Tests that CopyDocumentAsync returns Ok when the copy operation succeeds.
        /// </summary>
        [Fact]
        public async Task CopyDocumentAsync_ReturnsOk_WhenCopySucceeds()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var doc = new DocumentWithoutRevisionsDto
            {
                Id = Guid.NewGuid(),
                FileName = "test file",
                Extension = ".txt"
            };
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(matterId, false, false))
                .ReturnsAsync(new Entities.Matter
                {
                    Id = Guid.NewGuid(),
                    Description = "test matter",
                    CreationDate = DateTime.Now
                });
            _repoMock.Setup(r => r.PerformDocumentOperationAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), doc, "COPIED"))
                .ReturnsAsync(true);

            var result = await controller.CopyDocumentAsync(matterId, doc);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(doc, okResult.Value);
        }

        /// <summary>
        /// Tests that CopyDocumentAsync returns InternalServerError when the copy operation fails.
        /// </summary>
        [Fact]
        public async Task CopyDocumentAsync_ReturnsInternalServerError_WhenCopyFails()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var doc = new DocumentWithoutRevisionsDto
            {
                Id = Guid.NewGuid(),
                FileName = "test file",
                Extension = ".txt"
            };
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(matterId, false, false))
                .ReturnsAsync(new Entities.Matter
                {
                    Id = Guid.NewGuid(),
                    Description = "test matter",
                    CreationDate = DateTime.UtcNow
                });
            _repoMock.Setup(r => r.PerformDocumentOperationAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), doc, "COPIED"))
                .ReturnsAsync(false);

            var result = await controller.CopyDocumentAsync(matterId, doc);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Tests that CopyDocumentAsync returns InternalServerError when an exception is thrown during the operation.
        /// </summary>
        [Fact]
        public async Task CopyDocumentAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var doc = new DocumentWithoutRevisionsDto
            {
                Id = Guid.NewGuid(),
                FileName = "test file",
                Extension = ".txt"
            };
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(matterId, false, false))
                .ReturnsAsync(new Entities.Matter
                {
                    Id = Guid.NewGuid(),
                    Description = "test matter",
                    CreationDate = DateTime.UtcNow
                });
            _repoMock.Setup(r => r.PerformDocumentOperationAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), doc, "COPIED"))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await controller.CopyDocumentAsync(matterId, doc);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}