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
    /// Unit tests for <see cref="RevisionController.CreateRevisionAsync"/>.
    /// </summary>
    public class RevisionControllerCreateRevisionAsyncTests
    {
        private readonly Mock<ILogger<RevisionController>> _loggerMock = new();
        private readonly Mock<IAdmsRepository> _repoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<ProblemDetailsFactory> _problemDetailsFactoryMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

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

        #region Null and Validation Checks

        /// <summary>
        /// Tests that CreateRevisionAsync returns BadRequest when the DTO is null.
        /// </summary>
        [Fact]
        public async Task CreateRevisionAsync_ReturnsBadRequest_WhenDtoIsNull()
        {
            var controller = CreateController();
            var result = await controller.CreateRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), null);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that CreateRevisionAsync returns BadRequest when matterId is invalid.
        /// </summary>
        [Fact]
        public async Task CreateRevisionAsync_ReturnsBadRequest_WhenMatterIdInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), nameof(Guid)))
                .Returns(new BadRequestResult());

            var result = await controller.CreateRevisionAsync(Guid.Empty, Guid.NewGuid(), new RevisionForCreationDto(false) { RevisionNumber = 1 });
            Assert.IsType<BadRequestResult>(result.Result);
        }

        /// <summary>
        /// Tests that CreateRevisionAsync returns BadRequest when documentId is invalid.
        /// </summary>
        [Fact]
        public async Task CreateRevisionAsync_ReturnsBadRequest_WhenDocumentIdInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns(new BadRequestResult());

            var result = await controller.CreateRevisionAsync(Guid.NewGuid(), Guid.Empty, new RevisionForCreationDto(false) { RevisionNumber = 1 });
            Assert.IsType<BadRequestResult>(result.Result);
        }

        /// <summary>
        /// Tests that CreateRevisionAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task CreateRevisionAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.CreateRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), new RevisionForCreationDto(false) { RevisionNumber = 1 });
            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Tests that CreateRevisionAsync returns NotFound when document does not exist.
        /// </summary>
        [Fact]
        public async Task CreateRevisionAsync_ReturnsNotFound_WhenDocumentNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.CreateRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), new RevisionForCreationDto(false) { RevisionNumber = 1 });
            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Tests that CreateRevisionAsync returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task CreateRevisionAsync_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("RevisionNumber", "Required");
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);

            var result = await controller.CreateRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), new RevisionForCreationDto(false) { RevisionNumber = 1 });
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        #endregion

        #region Document and Business Logic

        /// <summary>
        /// Tests that CreateRevisionAsync returns NotFound when document does not belong to matter.
        /// </summary>
        [Fact]
        public async Task CreateRevisionAsync_ReturnsNotFound_WhenDocumentNotInMatter()
        {
            var controller = CreateController();
            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow,
            };
            var doc = new Document
            {
                MatterId = Guid.NewGuid(),
                Revisions = (List<Revision>) [],
                FileName = "Test File",
                Extension = ".txt",
                Matter = matter
            };
            var matterId = Guid.NewGuid();
            var docId = Guid.NewGuid();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetDocumentAsync(docId, true, false))
                .ReturnsAsync(doc);

            var result = await controller.CreateRevisionAsync(matterId, docId, new RevisionForCreationDto(false) { RevisionNumber = 1 });
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that CreateRevisionAsync returns BadRequest if creation fails.
        /// </summary>
        [Fact]
        public async Task CreateRevisionAsync_ReturnsBadRequest_WhenCreationFails()
        {
            var controller = CreateController();
            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow,
            };
            var doc = new Document
            {
                MatterId = matter.Id,
                Revisions = (List<Revision>) [],
                FileName = "test file",
                Extension = ".txt",
                Matter = matter
            };
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(doc.Id))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetDocumentAsync(doc.Id, true, false))
                .ReturnsAsync(doc);
            _mapperMock.Setup(m => m.Map<RevisionDto>(It.IsAny<RevisionForCreationDto>()))
                .Returns(new RevisionDto());
            _repoMock.Setup(r => r.AddRevisionAsync(doc.Id, It.IsAny<RevisionDto>()))
                .ReturnsAsync((Revision?)null);

            var result = await controller.CreateRevisionAsync(matter.Id, doc.Id, new RevisionForCreationDto(false) { RevisionNumber = 1 });
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that CreateRevisionAsync returns InternalServerError if SaveChangesAsync fails.
        /// </summary>
        [Fact]
        public async Task CreateRevisionAsync_ReturnsInternalServerError_WhenSaveFails()
        {
            var controller = CreateController();
            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow,
            };
            var doc = new Document
            {
                MatterId = matter.Id,
                Revisions = (List<Revision>) [],
                FileName = "test file",
                Extension = ".txt",
                Matter = matter
            };
            var revision = new Revision();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(doc.Id))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetDocumentAsync(doc.Id, true, false))
                .ReturnsAsync(doc);
            _mapperMock.Setup(m => m.Map<RevisionDto>(It.IsAny<RevisionForCreationDto>()))
                .Returns(new RevisionDto());
            _repoMock.Setup(r => r.AddRevisionAsync(doc.Id, It.IsAny<RevisionDto>()))
                .ReturnsAsync(revision);
            _repoMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(false);

            var result = await controller.CreateRevisionAsync(matter.Id, doc.Id, new RevisionForCreationDto(false) { RevisionNumber = 1 });
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that CreateRevisionAsync returns CreatedAtRoute when successful.
        /// </summary>
        [Fact]
        public async Task CreateRevisionAsync_ReturnsCreatedAtRoute_WhenSuccessful()
        {
            var controller = CreateController();
            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow,
            };
            var doc = new Document
            {
                Id = Guid.NewGuid(),
                MatterId = matter.Id,
                Revisions = (List<Revision>) [],
                FileName = "test file",
                Extension = ".txt",
                Matter = matter
            };
            var revision = new Revision();
            var revisionDto = new RevisionDto { Id = Guid.NewGuid() };
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(doc.Id))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetDocumentAsync(doc.Id, true, false))
                .ReturnsAsync(doc);
            _mapperMock.Setup(m => m.Map<RevisionDto>(It.IsAny<RevisionForCreationDto>()))
                .Returns(new RevisionDto());
            _repoMock.Setup(r => r.AddRevisionAsync(doc.Id, It.IsAny<RevisionDto>()))
                .ReturnsAsync(revision);
            _repoMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<RevisionDto>(revision))
                .Returns(revisionDto);

            var result = await controller.CreateRevisionAsync(matter.Id, doc.Id, new RevisionForCreationDto(false) { RevisionNumber = 1 });
            Assert.IsType<CreatedAtRouteResult>(result.Result);
        }

        /// <summary>
        /// Tests that CreateRevisionAsync returns InternalServerError on exception.
        /// </summary>
        [Fact]
        public async Task CreateRevisionAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow,
            };
            var doc = new Document
            {
                Id = Guid.NewGuid(),
                MatterId = matter.Id,
                Revisions = (List<Revision>) [],
                FileName = "test file",
                Extension = ".txt",
                Matter = matter
            };
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(doc.Id))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetDocumentAsync(doc.Id, true, false))
                .ThrowsAsync(new Exception("Test"));

            var result = await controller.CreateRevisionAsync(matter.Id, doc.Id, new RevisionForCreationDto(false) { RevisionNumber = 1 });
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}