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
    /// Unit tests for <see cref="DocumentController.GetDocumentHistoryAsync"/>.
    /// </summary>
    public class DocumentControllerGetDocumentHistoryAsyncTests
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
        /// Tests that GetDocumentHistoryAsync returns BadRequest when matterId is empty.
        /// </summary>
        [Fact]
        public async Task GetDocumentHistoryAsync_ReturnsBadRequest_WhenMatterIdEmpty()
        {
            var controller = CreateController();
            var result = await controller.GetDocumentHistoryAsync(Guid.Empty, Guid.NewGuid());
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that GetDocumentHistoryAsync returns BadRequest when documentId is empty.
        /// </summary>
        [Fact]
        public async Task GetDocumentHistoryAsync_ReturnsBadRequest_WhenDocumentIdEmpty()
        {
            var controller = CreateController();
            var result = await controller.GetDocumentHistoryAsync(Guid.NewGuid(), Guid.Empty);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Existence and Validation

        /// <summary>
        /// Tests that GetDocumentHistoryAsync returns NotFound when matter or document does not exist.
        /// </summary>
        [Fact]
        public async Task GetDocumentHistoryAsync_ReturnsNotFound_WhenMatterOrDocumentNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());
            var result = await controller.GetDocumentHistoryAsync(Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Operation and Success

        /// <summary>
        /// Tests that GetDocumentHistoryAsync returns Ok with mapped history when document and matter exist and history is present.
        /// </summary>
        [Fact]
        public async Task GetDocumentHistoryAsync_ReturnsOk_WithHistory()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            var user = new UserDto
            {
                Id = Guid.NewGuid(),
                Name = "test user"
            };

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            var documentEntity = new Document
            {
                Id = documentId,
                MatterId = matterId,
                Matter = new Matter
                {
                    Id = matterId,
                    Description = "test matter",
                    CreationDate = DateTime.UtcNow
                },
                DocumentActivityUsers = (List<DocumentActivityUser>) [],
                FileName = "test file",
                Extension = ".txt"
            };

            var mappedHistory = new List<DocumentActivityUserDto>
            {
                new()
                {
                    DocumentId = documentId,
                    Document = new DocumentWithoutRevisionsDto
                    {
                        Id = documentId,
                        FileName = "test file",
                        Extension = ".txt"
                    },
                    DocumentActivityId = Guid.NewGuid(),
                    DocumentActivity = new DocumentActivityDto
                    {
                        Id = Guid.NewGuid(),
                        Activity = "SAVED"
                    },
                    UserId = user.Id,
                    User = user,
                    CreatedAt = default
                }
            };

            _repoMock.Setup(r => r.GetDocumentAsync(documentId, false, true))
                .ReturnsAsync(documentEntity);
            _mapperMock.Setup(m => m.Map<IEnumerable<DocumentActivityUserDto>>(documentEntity.DocumentActivityUsers))
                .Returns(mappedHistory);

            var result = await controller.GetDocumentHistoryAsync(matterId, documentId);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedHistory = Assert.IsType<IEnumerable<DocumentActivityUserDto>>(okResult.Value, false);
            Assert.Single(returnedHistory);
        }

        /// <summary>
        /// Tests that GetDocumentHistoryAsync returns Ok with an empty list when document has no history.
        /// </summary>
        [Fact]
        public async Task GetDocumentHistoryAsync_ReturnsOk_WithEmptyList_WhenNoHistory()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            var documentEntity = new Document
            {
                Id = documentId,
                MatterId = matterId,
                Matter = new Matter
                {
                    Id = matterId,
                    Description = "test matter",
                    CreationDate = DateTime.UtcNow
                },
                DocumentActivityUsers = (List<DocumentActivityUser>) [],
                FileName = "test file",
                Extension = ".txt"
            };

            _repoMock.Setup(r => r.GetDocumentAsync(documentId, false, true))
                .ReturnsAsync(documentEntity);
            _mapperMock.Setup(m => m.Map<IEnumerable<DocumentActivityUserDto>>(documentEntity.DocumentActivityUsers))
                .Returns((List<DocumentActivityUserDto>) []);

            var result = await controller.GetDocumentHistoryAsync(matterId, documentId);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedHistory = Assert.IsType<IEnumerable<DocumentActivityUserDto>>(okResult, false);
            Assert.Single(returnedHistory);
        }

        /// <summary>
        /// Tests that GetDocumentHistoryAsync returns NotFound when document is not found in repository.
        /// </summary>
        [Fact]
        public async Task GetDocumentHistoryAsync_ReturnsNotFound_WhenDocumentNotFoundInRepository()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetDocumentAsync(documentId, false, true))
                .ReturnsAsync((Document?)null);

            var result = await controller.GetDocumentHistoryAsync(matterId, documentId);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Tests that GetDocumentHistoryAsync returns InternalServerError when an exception is thrown during the operation.
        /// </summary>
        [Fact]
        public async Task GetDocumentHistoryAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetDocumentAsync(documentId, false, true))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await controller.GetDocumentHistoryAsync(matterId, documentId);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}