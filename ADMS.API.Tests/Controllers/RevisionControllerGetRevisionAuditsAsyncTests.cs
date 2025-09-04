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
    /// Unit tests for <see cref="RevisionController.GetRevisionAuditsAsync"/>.
    /// </summary>
    public class RevisionControllerGetRevisionAuditsAsyncTests
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
        /// Tests that GetRevisionAuditsAsync returns BadRequest when matterId is invalid.
        /// </summary>
        [Fact]
        public async Task GetRevisionAuditsAsync_ReturnsBadRequest_WhenMatterIdInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), nameof(Guid)))
                .Returns(new BadRequestResult());

            var result = await controller.GetRevisionAuditsAsync(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<BadRequestResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAuditsAsync returns BadRequest when documentId is invalid.
        /// </summary>
        [Fact]
        public async Task GetRevisionAuditsAsync_ReturnsBadRequest_WhenDocumentIdInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns(new BadRequestResult());

            var result = await controller.GetRevisionAuditsAsync(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());
            Assert.IsType<BadRequestResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAuditsAsync returns BadRequest when revisionId is invalid.
        /// </summary>
        [Fact]
        public async Task GetRevisionAuditsAsync_ReturnsBadRequest_WhenRevisionNumberInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns(new BadRequestResult());

            var result = await controller.GetRevisionAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);
            Assert.IsType<BadRequestResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAuditsAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task GetRevisionAuditsAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetRevisionAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAuditsAsync returns NotFound when document does not exist.
        /// </summary>
        [Fact]
        public async Task GetRevisionAuditsAsync_ReturnsNotFound_WhenDocumentNotFound()
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

            var result = await controller.GetRevisionAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAuditsAsync returns NotFound when revision does not exist.
        /// </summary>
        [Fact]
        public async Task GetRevisionAuditsAsync_ReturnsNotFound_WhenRevisionNotFound()
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

            var result = await controller.GetRevisionAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region Repository and Relationship Checks

        /// <summary>
        /// Tests that GetRevisionAuditsAsync returns NotFound when revision is not found in repository.
        /// </summary>
        [Fact]
        public async Task GetRevisionAuditsAsync_ReturnsNotFound_WhenRevisionNullFromRepo()
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
            _repoMock.Setup(r => r.GetRevisionByIdAsync(It.IsAny<Guid>(), true))
                .ReturnsAsync((Revision?)null);

            var result = await controller.GetRevisionAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAuditsAsync returns NotFound when revision does not belong to the specified document.
        /// </summary>
        [Fact]
        public async Task GetRevisionAuditsAsync_ReturnsNotFound_WhenRevisionNotInDocument()
        {
            var controller = CreateController();
            var revision = new Revision { DocumentId = Guid.NewGuid(), RevisionActivityUsers = (List<RevisionActivityUser>)
                []
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
            _repoMock.Setup(r => r.GetRevisionByIdAsync(It.IsAny<Guid>(), true))
                .ReturnsAsync(revision);

            var result = await controller.GetRevisionAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAuditsAsync returns NotFound when document does not belong to the specified matter.
        /// </summary>
        [Fact]
        public async Task GetRevisionAuditsAsync_ReturnsNotFound_WhenDocumentNotInMatter()
        {
            var controller = CreateController();
            var document = new Document()
            {
                Id = Guid.NewGuid(),
                FileName = "Test File",
                MatterId = Guid.NewGuid(), // Different matter ID
                Matter = new Matter()
                {
                    Id = Guid.NewGuid(),
                    CreationDate = DateTime.UtcNow,
                    Description = "Test Matter"
                },
                Extension = ".txt"
            };
            var revision = new Revision
            {
                DocumentId = Guid.NewGuid(), 
                RevisionActivityUsers = (List<RevisionActivityUser>) []
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
            _repoMock.Setup(r => r.GetRevisionByIdAsync(It.IsAny<Guid>(), true))
                .ReturnsAsync(revision);
            _repoMock.Setup(r => r.GetDocumentAsync(It.IsAny<Guid>(), false, false))
                .ReturnsAsync(document);

            var result = await controller.GetRevisionAuditsAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionAuditsAsync returns NotFound when revision has no audit history.
        /// </summary>
        [Fact]
        public async Task GetRevisionAuditsAsync_ReturnsNotFound_WhenNoAuditHistory()
        {
            var controller = CreateController();
            var docId = Guid.NewGuid();
            var matterId = Guid.NewGuid();
            var revision = new Revision { DocumentId = docId, RevisionActivityUsers = (List<RevisionActivityUser>) [] };
            var document = new Document()
            {
                Id = docId,
                FileName = "Test Document",
                MatterId = matterId,
                Matter = new Matter
                {
                    Id = matterId,
                    CreationDate = DateTime.UtcNow,
                    Description = "Test Matter"
                },
                Extension = ".txt"
            };
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateRevisionExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetRevisionByIdAsync(It.IsAny<Guid>(), true))
                .ReturnsAsync(revision);
            _repoMock.Setup(r => r.GetDocumentAsync(docId, false, false))
                .ReturnsAsync(document);

            var result = await controller.GetRevisionAuditsAsync(matterId, docId, Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        #endregion

        #region Success and Exception Handling

        /// <summary>
        /// Tests that GetRevisionAuditsAsync returns Ok with mapped audit records when successful.
        /// </summary>
        [Fact]
        public async Task GetRevisionAuditsAsync_ReturnsOk_WhenSuccessful()
        {
            var controller = CreateController();
            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow,
                Description = "Test Matter"
            };
            var document = new Document()
            {
                Id = Guid.NewGuid(),
                FileName = "Test Document",
                MatterId = matter.Id,
                Matter = matter,
                Extension = ".txt"
            };
            var revision = new Revision()
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                CreationDate = DateTime.UtcNow
            };
            var revisionActivityUsers = new List<RevisionActivityUser>
            {
                new()
                {
                    UserId = Guid.NewGuid(),
                    Revision = revision,
                    RevisionActivity = new RevisionActivity
                    {
                        Id = Guid.NewGuid(),
                        Activity = "CREATED"
                    },
                    User = new User()
                    {
                        Name = "Test User",
                        Id = Guid.NewGuid()
                    }
                }
            };
            revision.RevisionActivityUsers = revisionActivityUsers;
            var auditDtos = new List<RevisionActivityUserDto>()
            {
                new()
                {
                    UserId = revisionActivityUsers[0].UserId,
                    RevisionActivityId = revisionActivityUsers[0].RevisionActivityId,
                    RevisionNumber = revision.Id,
                    RevisionActivity = new RevisionActivityDto()
                    {
                        Id = Guid.NewGuid(),
                        Activity = "CREATED"
                    },
                    User = new UserDto
                    {
                        Id = revisionActivityUsers[0].UserId,
                        Name = revisionActivityUsers[0].User.Name
                    },
                    CreatedAt = revisionActivityUsers[0].CreatedAt,
                    Revision = new RevisionDto()
                    {
                        Id = revision.Id,
                        CreationDate = revision.CreationDate
                    }
                }
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
            _repoMock.Setup(r => r.GetRevisionByIdAsync(revision.Id, true))
                .ReturnsAsync(revision);
            _repoMock.Setup(r => r.GetDocumentAsync(document.Id, false, false))
                .ReturnsAsync(document);
            _mapperMock.Setup(m => m.Map<IEnumerable<RevisionActivityUserDto>>(revisionActivityUsers))
                .Returns(auditDtos);

            var result = await controller.GetRevisionAuditsAsync(matter.Id, document.Id, revision.Id);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(auditDtos, okResult.Value);
        }

        /// <summary>
        /// Tests that GetRevisionAuditsAsync returns InternalServerError on exception.
        /// </summary>
        [Fact]
        public async Task GetRevisionAuditsAsync_ReturnsInternalServerError_OnException()
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
            _repoMock.Setup(r => r.GetRevisionByIdAsync(revisionId, true))
                .ThrowsAsync(new Exception("Test"));

            var result = await controller.GetRevisionAuditsAsync(matterId, docId, revisionId);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}