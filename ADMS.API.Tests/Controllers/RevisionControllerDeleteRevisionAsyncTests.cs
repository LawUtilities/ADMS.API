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
    /// Unit tests for <see cref="RevisionController.DeleteRevisionAsync"/>.
    /// </summary>
    public class RevisionControllerDeleteRevisionAsyncTests
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

        #region Validation and Existence Checks

        /// <summary>
        /// Tests that DeleteRevisionAsync returns BadRequest when any GUID is invalid.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsBadRequest_WhenAnyGuidInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new BadRequestResult());

            var result = await controller.DeleteRevisionAsync(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that DeleteRevisionAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.DeleteRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that DeleteRevisionAsync returns NotFound when document does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsNotFound_WhenDocumentNotFound()
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

            var result = await controller.DeleteRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that DeleteRevisionAsync returns NotFound when revision does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsNotFound_WhenRevisionNotFound()
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

            var result = await controller.DeleteRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Repository and Relationship Checks

        /// <summary>
        /// Tests that DeleteRevisionAsync returns NotFound when revision is not found in repository.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsNotFound_WhenRevisionNullFromRepo()
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

            var result = await controller.DeleteRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Tests that DeleteRevisionAsync returns NotFound when revision does not belong to the specified document.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsNotFound_WhenRevisionNotInDocument()
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

            var result = await controller.DeleteRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Tests that DeleteRevisionAsync returns NotFound when document does not belong to the specified matter.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsNotFound_WhenDocumentNotInMatter()
        {
            var controller = CreateController();
            var matter = new Matter()
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

            var result = await controller.DeleteRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result);
        }

        #endregion

        #region Deletion and Persistence

        /// <summary>
        /// Tests that DeleteRevisionAsync returns BadRequest if deletion fails.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsBadRequest_WhenDeleteFails()
        {
            var controller = CreateController();
            var matter = new Matter()
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
            _repoMock.Setup(r => r.DeleteRevisionAsync(It.IsAny<RevisionDto>()))
                .ReturnsAsync(false);

            var result = await controller.DeleteRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that DeleteRevisionAsync returns NoContent when successful.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsNoContent_WhenSuccessful()
        {
            var controller = CreateController();
            var matter = new Matter()
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
            _repoMock.Setup(r => r.DeleteRevisionAsync(It.IsAny<RevisionDto>()))
                .ReturnsAsync(true);
            _repoMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(true);

            var result = await controller.DeleteRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Tests that DeleteRevisionAsync returns InternalServerError if SaveChangesAsync fails.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsInternalServerError_WhenSaveFails()
        {
            var controller = CreateController();
            var matter = new Matter()
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
            _repoMock.Setup(r => r.DeleteRevisionAsync(It.IsAny<RevisionDto>()))
                .ReturnsAsync(true);
            _repoMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(false);

            var result = await controller.DeleteRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Tests that DeleteRevisionAsync returns InternalServerError on exception.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsInternalServerError_OnException()
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
                .ThrowsAsync(new Exception("Test"));

            var result = await controller.DeleteRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}