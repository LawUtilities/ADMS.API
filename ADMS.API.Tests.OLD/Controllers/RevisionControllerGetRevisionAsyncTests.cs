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
    /// Unit tests for <see cref="RevisionController.GetRevisionAsync"/>.
    /// </summary>
    public class RevisionControllerGetRevisionAsyncTests
    {
        private readonly Mock<ILogger<RevisionController>> _loggerMock = new();
        private readonly Mock<IAdmsRepository> _repoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<ProblemDetailsFactory> _problemDetailsFactoryMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        /// <summary>
        /// Creates and initializes a new instance of the <see cref="RevisionController"/> class with mocked
        /// dependencies for testing purposes.
        /// </summary>
        /// <returns>A fully initialized <see cref="RevisionController"/> instance with a default <see cref="ControllerContext"/>
        /// containing an <see cref="HttpContext"/>.</returns>
        private RevisionController CreateController()
        {
            var controller = new RevisionController(
                _loggerMock.Object,
                _repoMock.Object,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _problemDetailsFactoryMock.Object,
                _validationServiceMock.Object
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            return controller;
        }

        #region Validation and Existence Checks

        /// <summary>
        /// Tests that GetRevisionAsync returns BadRequest when matterId is invalid.
        /// </summary>
        [Fact]
        public async Task GetRevisionAsync_ReturnsBadRequest_WhenMatterIdInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), nameof(Guid)))
                .Returns(new BadRequestResult());

            var result = await controller.GetRevisionAsync(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<BadRequestResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAsync returns BadRequest when documentId is invalid.
        /// </summary>
        [Fact]
        public async Task GetRevisionAsync_ReturnsBadRequest_WhenDocumentIdInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns(new BadRequestResult());

            var result = await controller.GetRevisionAsync(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());
            Assert.IsType<BadRequestResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAsync returns BadRequest when revisionId is invalid.
        /// </summary>
        [Fact]
        public async Task GetRevisionAsync_ReturnsBadRequest_WhenRevisionNumberInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns(new BadRequestResult());

            var result = await controller.GetRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);
            Assert.IsType<BadRequestResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task GetRevisionAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAsync returns NotFound when document does not exist.
        /// </summary>
        [Fact]
        public async Task GetRevisionAsync_ReturnsNotFound_WhenDocumentNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAsync returns NotFound when revision does not exist.
        /// </summary>
        [Fact]
        public async Task GetRevisionAsync_ReturnsNotFound_WhenRevisionNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateRevisionExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region Repository and Relationship Checks

        /// <summary>
        /// Tests that GetRevisionAsync returns NotFound when revision is not found in repository.
        /// </summary>
        [Fact]
        public async Task GetRevisionAsync_ReturnsNotFound_WhenRevisionNullFromRepo()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateRevisionExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetRevisionByIdAsync(It.IsAny<Guid>(), false))
                .ReturnsAsync((Revision?)null);

            var result = await controller.GetRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAsync returns NotFound when revision does not belong to the specified document.
        /// </summary>
        [Fact]
        public async Task GetRevisionAsync_ReturnsNotFound_WhenRevisionNotInDocument()
        {
            var controller = CreateController();
            var revision = new Revision { DocumentId = Guid.NewGuid() };
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateRevisionExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetRevisionByIdAsync(It.IsAny<Guid>(), false))
                .ReturnsAsync(revision);

            var result = await controller.GetRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAsync returns NotFound when document does not belong to the specified matter.
        /// </summary>
        [Fact]
        public async Task GetRevisionAsync_ReturnsNotFound_WhenDocumentNotInMatter()
        {
            var controller = CreateController();
            var matter = new Matter
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var revision = new Revision { DocumentId = Guid.NewGuid() };
            var document = new Document
            {
                IsDeleted = false,
                FileName = "test file",
                Extension = ".txt",
                MatterId = matter.Id,
                Matter = matter
            };
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateRevisionExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetRevisionByIdAsync(It.IsAny<Guid>(), false))
                .ReturnsAsync(revision);
            _repoMock.Setup(r => r.GetDocumentAsync(It.IsAny<Guid>(), false, false))
                .ReturnsAsync(document);

            var result = await controller.GetRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        #endregion

        #region Success and Exception Handling

        /// <summary>
        /// Tests that GetRevisionAsync returns Ok with the mapped DTO when successful.
        /// </summary>
        [Fact]
        public async Task GetRevisionAsync_ReturnsOk_WhenSuccessful()
        {
            var controller = CreateController();
            var matter = new Matter
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var document = new Document
            {
                IsDeleted = false,
                FileName = "test file",
                Extension = ".txt",
                MatterId = matter.Id,
                Matter = matter
            };
            var revision = new Revision
            {
                Id = Guid.NewGuid(),
                RevisionNumber = 1,
                CreationDate = DateTime.UtcNow,
                ModificationDate = DateTime.UtcNow,
                DocumentId = document.Id,
                Document = document
            };
            var revisionDto = new RevisionDto 
            { 
                Id = Guid.NewGuid(),
                RevisionNumber = 1,
                CreationDate = DateTime.UtcNow,
                ModificationDate = DateTime.UtcNow,
            };

            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(document.Id))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateRevisionExistsAsync(revision.Id))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetRevisionByIdAsync(revision.Id, false))
                .ReturnsAsync(revision);
            _repoMock.Setup(r => r.GetDocumentAsync(document.Id, false, false))
                .ReturnsAsync(document);
            _mapperMock.Setup(m => m.Map<RevisionDto>(revision))
                .Returns(revisionDto);

            var result = await controller.GetRevisionAsync(matter.Id, document.Id, revision.Id);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(revisionDto, okResult.Value);
        }

        /// <summary>
        /// Tests that GetRevisionAsync returns InternalServerError on exception.
        /// </summary>
        [Fact]
        public async Task GetRevisionAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            var docId = Guid.NewGuid();
            var matterId = Guid.NewGuid();
            var revisionId = Guid.NewGuid();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateRevisionExistsAsync(revisionId))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetRevisionByIdAsync(revisionId, false))
                .ThrowsAsync(new Exception("Test"));

            var result = await controller.GetRevisionAsync(matterId, docId, revisionId);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}