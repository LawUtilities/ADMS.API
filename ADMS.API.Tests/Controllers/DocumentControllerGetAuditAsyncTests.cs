using ADMS.API.Controllers;
using ADMS.API.Entities;
using ADMS.API.Helpers;
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
    /// Unit tests for <see cref="DocumentController.GetAuditAsync"/>.
    /// </summary>
    public class DocumentControllerGetAuditAsyncTests
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
        /// Tests that GetAuditAsync returns BadRequest when matterId is empty.
        /// </summary>
        [Fact]
        public async Task GetAuditAsync_ReturnsBadRequest_WhenMatterIdEmpty()
        {
            var controller = CreateController();
            var result = await controller.GetAuditAsync(Guid.Empty, Guid.NewGuid(), AuditEnums.AuditDirection.From);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetAuditAsync returns BadRequest when documentId is empty.
        /// </summary>
        [Fact]
        public async Task GetAuditAsync_ReturnsBadRequest_WhenDocumentIdEmpty()
        {
            var controller = CreateController();
            var result = await controller.GetAuditAsync(Guid.NewGuid(), Guid.Empty, AuditEnums.AuditDirection.From);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetAuditAsync returns BadRequest when direction is invalid.
        /// </summary>
        [Fact]
        public async Task GetAuditAsync_ReturnsBadRequest_WhenDirectionInvalid()
        {
            var controller = CreateController();
            // Use an invalid enum value (e.g., -1)
            var result = await controller.GetAuditAsync(Guid.NewGuid(), Guid.NewGuid(), (AuditEnums.AuditDirection)(-1));
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        #endregion

        #region Existence and Validation

        /// <summary>
        /// Tests that GetAuditAsync returns NotFound when matter or document does not exist.
        /// </summary>
        [Fact]
        public async Task GetAuditAsync_ReturnsNotFound_WhenMatterOrDocumentNotFound()
        {
            var controller = CreateController();
            // Simulate validation returning NotFound
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());
            // ValidateMatterAndDocumentAsync will return NotFound if matter is missing
            var result = await controller.GetAuditAsync(Guid.NewGuid(), Guid.NewGuid(), AuditEnums.AuditDirection.From);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region Success and Data Retrieval

        /// <summary>
        /// Tests that GetAuditAsync returns Ok with audit records when input is valid and audits exist.
        /// </summary>
        [Fact]
        public async Task GetAuditAsync_ReturnsOk_WithAuditRecords()
        {
            var controller = CreateController();

            var user = new UserMinimalDto()
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
            };

            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };

            var document = new Document()
            {
                Id = Guid.NewGuid(),
                FileName = "test file",
                Extension = ".txt",
                FileSize = 1234,
                MimeType = "text/plain",
                Matter = matter,
                MatterId = matter.Id
            };

            const AuditEnums.AuditDirection direction = AuditEnums.AuditDirection.From;

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(document.Id))
                .ReturnsAsync((ActionResult?)null);

            var audits = new List<MatterDocumentActivityUserMinimalDto>
            {
                new()
                {
                    Matter = new MatterMinimalDto()
                    {
                        Id = matter.Id,
                        Description = matter.Description,
                        CreationDate = matter.CreationDate
                    },
                    DocumentId = document.Id,
                    MatterDocumentActivityId = Guid.NewGuid(),
                    MatterDocumentActivity = null,
                    UserId = user.Id,
                    User = user,
                    MatterId = matter.Id,
                    CreatedAt = matter.CreationDate
                }
            }.AsQueryable();

            _repoMock.Setup(r => r.GetExtendedAuditsAsync(matter.Id, document.Id, direction))
                .ReturnsAsync(audits);

            var result = await controller.GetAuditAsync(matter.Id, document.Id, direction);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAudits = Assert.IsAssignableFrom<IEnumerable<MatterDocumentActivityUserMinimalDto>>(okResult.Value);
            Assert.Single(returnedAudits);
        }

        /// <summary>
        /// Tests that GetAuditAsync returns Ok with an empty list when no audits exist.
        /// </summary>
        [Fact]
        public async Task GetAuditAsync_ReturnsOk_WithEmptyList_WhenNoAuditsExist()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var direction = AuditEnums.AuditDirection.To;

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            var audits = new List<MatterDocumentActivityUserMinimalDto>().AsQueryable();

            _repoMock.Setup(r => r.GetExtendedAuditsAsync(matterId, documentId, direction))
                .ReturnsAsync(audits);

            var result = await controller.GetAuditAsync(matterId, documentId, direction);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAudits = Assert.IsAssignableFrom<IEnumerable<MatterDocumentActivityUserMinimalDto>>(okResult.Value);
            Assert.Empty(returnedAudits);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Tests that GetAuditAsync returns InternalServerError when an exception occurs in the repository.
        /// </summary>
        [Fact]
        public async Task GetAuditAsync_ReturnsInternalServerError_OnRepositoryException()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var direction = AuditEnums.AuditDirection.From;

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            _repoMock.Setup(r => r.GetExtendedAuditsAsync(matterId, documentId, direction))
                .ThrowsAsync(new Exception("Repository failure"));

            var result = await controller.GetAuditAsync(matterId, documentId, direction);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}