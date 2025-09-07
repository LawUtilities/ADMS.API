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
    /// Unit tests for <see cref="RevisionController.UpdateRevisionAsync"/>.
    /// </summary>
    public class RevisionControllerUpdateRevisionAsyncTests
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
        /// Tests that UpdateRevisionAsync returns BadRequest when matterId is invalid.
        /// </summary>
        [Fact]
        public async Task UpdateRevisionAsync_ReturnsBadRequest_WhenMatterIdInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), nameof(Guid)))
                .Returns(new BadRequestResult());

            var result = await controller.UpdateRevisionAsync(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), new RevisionForUpdateDto { RevisionNumber = 1 });
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that UpdateRevisionAsync returns BadRequest when documentId is invalid.
        /// </summary>
        [Fact]
        public async Task UpdateRevisionAsync_ReturnsBadRequest_WhenDocumentIdInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns(new BadRequestResult());

            var result = await controller.UpdateRevisionAsync(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), new RevisionForUpdateDto { RevisionNumber = 1 });
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that UpdateRevisionAsync returns BadRequest when revisionId is invalid.
        /// </summary>
        [Fact]
        public async Task UpdateRevisionAsync_ReturnsBadRequest_WhenRevisionNumberInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns(new BadRequestResult());

            var result = await controller.UpdateRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, new RevisionForUpdateDto { RevisionNumber = 1 });
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that UpdateRevisionAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateRevisionAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.UpdateRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new RevisionForUpdateDto { RevisionNumber = 1 });
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that UpdateRevisionAsync returns NotFound when document does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateRevisionAsync_ReturnsNotFound_WhenDocumentNotFound()
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

            var result = await controller.UpdateRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new RevisionForUpdateDto { RevisionNumber = 1 });
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that UpdateRevisionAsync returns NotFound when revision does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateRevisionAsync_ReturnsNotFound_WhenRevisionNotFound()
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

            var result = await controller.UpdateRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new RevisionForUpdateDto { RevisionNumber = 1 });
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that UpdateRevisionAsync returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task UpdateRevisionAsync_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("RevisionNumber", "Required");
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

            var result = await controller.UpdateRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new RevisionForUpdateDto { RevisionNumber = 1 });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Repository and Relationship Checks

        /// <summary>
        /// Tests that UpdateRevisionAsync returns NotFound when revision is not found in repository.
        /// </summary>
        [Fact]
        public async Task UpdateRevisionAsync_ReturnsNotFound_WhenRevisionNullFromRepo()
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

            var result = await controller.UpdateRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new RevisionForUpdateDto { RevisionNumber = 1 });
            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Tests that UpdateRevisionAsync returns NotFound when revision does not belong to the specified document.
        /// </summary>
        [Fact]
        public async Task UpdateRevisionAsync_ReturnsNotFound_WhenRevisionNotInDocument()
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

            var result = await controller.UpdateRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new RevisionForUpdateDto { RevisionNumber = 1 });
            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Tests that UpdateRevisionAsync returns NotFound when document does not belong to the specified matter.
        /// </summary>
        [Fact]
        public async Task UpdateRevisionAsync_ReturnsNotFound_WhenDocumentNotInMatter()
        {
            var controller = CreateController();
            var matter = new Matter
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
            var revision = new Revision
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                Document = document,
                RevisionNumber = 1,
                ModificationDate = DateTime.UtcNow,
                CreationDate = DateTime.UtcNow
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

            var result = await controller.UpdateRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new RevisionForUpdateDto { RevisionNumber = 1 });
            Assert.IsType<NotFoundObjectResult>(result);
        }

        #endregion

        #region Update and Persistence

        /// <summary>
        /// Tests that UpdateRevisionAsync returns NoContent when update and save are successful.
        /// </summary>
        [Fact]
        public async Task UpdateRevisionAsync_ReturnsNoContent_WhenSuccessful()
        {
            var controller = CreateController();
            var matter = new Matter
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
            var revision = new Revision
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                Document = document,
                RevisionNumber = 1,
                ModificationDate = DateTime.UtcNow,
                CreationDate = DateTime.UtcNow
            };
            var updateDto = new RevisionForUpdateDto { RevisionNumber = 2 };
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
            _repoMock.Setup(r => r.UpdateRevisionAsync(matter.Id, document.Id, revision.Id, revision))
                .ReturnsAsync(revision);
            _repoMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(true);

            var result = await controller.UpdateRevisionAsync(matter.Id, document.Id, revision.Id, updateDto);
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Tests that UpdateRevisionAsync returns BadRequest if SaveChangesAsync fails.
        /// </summary>
        [Fact]
        public async Task UpdateRevisionAsync_ReturnsBadRequest_WhenSaveFails()
        {
            var controller = CreateController();
            var matter = new Matter
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
            var revision = new Revision
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                Document = document,
                RevisionNumber = 1,
                ModificationDate = DateTime.UtcNow,
                CreationDate = DateTime.UtcNow
            };
            var updateDto = new RevisionForUpdateDto
            {
                RevisionNumber = 2
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
            _repoMock.Setup(r => r.UpdateRevisionAsync(matter.Id, document.Id, revision.Id, revision))
                .ReturnsAsync(revision);
            _repoMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(false);

            var result = await controller.UpdateRevisionAsync(matter.Id, document.Id, revision.Id, updateDto);
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Tests that UpdateRevisionAsync returns InternalServerError on exception.
        /// </summary>
        [Fact]
        public async Task UpdateRevisionAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            var matter = new Matter
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
            var revision = new Revision
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                Document = document,
                RevisionNumber = 1,
                ModificationDate = DateTime.UtcNow,
                CreationDate = DateTime.UtcNow
            };
            var updateDto = new RevisionForUpdateDto
            {
                RevisionNumber = 2
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
                .ThrowsAsync(new Exception("Test"));

            var result = await controller.UpdateRevisionAsync(matter.Id, document.Id, revision.Id, updateDto);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}