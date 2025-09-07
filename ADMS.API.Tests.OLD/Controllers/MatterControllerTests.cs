using ADMS.API.Controllers;
using ADMS.API.Entities;
using ADMS.API.Helpers;
using ADMS.API.Models;
using ADMS.API.ResourceParameters;
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
    /// Unit tests for <see cref="MatterController"/>.
    /// </summary>
    public class MatterControllerTests
    {
        private readonly Mock<ILogger<MatterController>> _loggerMock = new();
        private readonly Mock<IAdmsRepository> _repoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ProblemDetailsFactory> _problemDetailsFactoryMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        private MatterController CreateController()
        {
            var controller = new MatterController(
                _loggerMock.Object,
                _repoMock.Object,
                _mapperMock.Object,
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

        #region CreateMatterAsync

        /// <summary>
        /// Tests that CreateMatterAsync returns BadRequest when the DTO is null.
        /// </summary>
        [Fact]
        public async Task CreateMatterAsync_ReturnsBadRequest_WhenDtoIsNull()
        {
            var controller = CreateController();
            var result = await controller.CreateMatterAsync(null);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that CreateMatterAsync returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task CreateMatterAsync_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("Description", "Required");
            _validationServiceMock.Setup(v => v.ValidateModelState(controller.ModelState))
                .Returns(new BadRequestResult());

            var result = await controller.CreateMatterAsync(new MatterForCreationDto
            {
                Description = "",
                CreationDate = DateTime.UtcNow
            });
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that CreateMatterAsync returns Conflict when a matter with the same description exists.
        /// </summary>
        [Fact]
        public async Task CreateMatterAsync_ReturnsConflict_WhenDuplicateDescription()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateModelState(controller.ModelState))
                .Returns((ActionResult?)null);
            _repoMock.Setup(r => r.MatterNameExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(new ActionResult<bool>(true));

            var result = await controller.CreateMatterAsync(new MatterForCreationDto
            {
                Description = "Duplicate",
                CreationDate = DateTime.UtcNow
            });
            Assert.IsType<ConflictObjectResult>(result);
        }

        /// <summary>
        /// Tests that CreateMatterAsync returns CreatedAtRoute when successful.
        /// </summary>
        [Fact]
        public async Task CreateMatterAsync_ReturnsCreatedAtRoute_WhenSuccessful()
        {
            var controller = CreateController();
            var matterDto = new MatterDto
            {
                Description = "Test",
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow
            };
            _validationServiceMock.Setup(v => v.ValidateModelState(controller.ModelState))
                .Returns((ActionResult?)null);
            _repoMock.Setup(r => r.MatterNameExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(new ActionResult<bool>(false));
            _mapperMock.Setup(m => m.Map<MatterDto>(It.IsAny<MatterForCreationDto>()))
                .Returns(matterDto);
            _repoMock.Setup(r => r.AddMatterAsync(It.IsAny<MatterForCreationDto>()))
                .ReturnsAsync(new ActionResult<Matter>(new Matter
                {
                    Id = Guid.NewGuid(),
                    Description = "Test Matter",
                    CreationDate = DateTime.UtcNow
                }));

            var result = await controller.CreateMatterAsync(new MatterForCreationDto
            {
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            });
            Assert.IsType<CreatedAtRouteResult>(result);
        }

        /// <summary>
        /// Tests that CreateMatterAsync returns InternalServerError on exception.
        /// </summary>
        [Fact]
        public async Task CreateMatterAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateModelState(controller.ModelState))
                .Returns((ActionResult?)null);
            _repoMock.Setup(r => r.MatterNameExistsAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test"));

            var result = await controller.CreateMatterAsync(new MatterForCreationDto
            {
                Description = "Test",
                CreationDate = DateTime.UtcNow
            });
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region DeleteMatterAsync

        /// <summary>
        /// Tests that DeleteMatterAsync returns BadRequest when GUID is invalid.
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ReturnsBadRequest_WhenGuidInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new BadRequestResult());

            var result = await controller.DeleteMatterAsync(Guid.Empty);
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that DeleteMatterAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.DeleteMatterAsync(Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that DeleteMatterAsync returns NotFound when matter is not found in repository.
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ReturnsNotFound_WhenMatterNullFromRepo()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), true, false))
                .ReturnsAsync((Matter?)null);

            var result = await controller.DeleteMatterAsync(Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Tests that DeleteMatterAsync returns BadRequest if not all documents are deleted.
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ReturnsBadRequest_WhenNotAllDocumentsDeleted()
        {
            var controller = CreateController();
            var matter = new Matter
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };

            var documents = new List<Document>
            {
                new()
                {
                    IsDeleted = false,
                    IsCheckedOut = false,
                    FileName = "test Document",
                    Extension = ".txt",
                    MatterId = matter.Id,
                    Matter = matter
                }
            };

            matter.Documents = documents;

            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), true, false))
                .ReturnsAsync(matter);

            var result = await controller.DeleteMatterAsync(Guid.NewGuid());
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that DeleteMatterAsync returns BadRequest if any document is checked out.
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ReturnsBadRequest_WhenAnyDocumentCheckedOut()
        {
            var controller = CreateController();
            var matter = new Matter
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };

            var documents = new List<Document>
            {
                new()
                {
                    IsDeleted = true,
                    IsCheckedOut = true,
                    FileName = "test document",
                    Extension = ".txt",
                    MatterId = matter.Id,
                    Matter = matter
                }
            };

            matter.Documents = documents;

            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), true, false))
                .ReturnsAsync(matter);

            var result = await controller.DeleteMatterAsync(Guid.NewGuid());
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that DeleteMatterAsync returns InternalServerError if deletion fails.
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ReturnsInternalServerError_WhenDeleteFails()
        {
            var controller = CreateController();
            var matter = new Matter
            {
                Documents = (List<Document>) [],
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), true, false))
                .ReturnsAsync(matter);
            _repoMock.Setup(r => r.DeleteMatterAsync(It.IsAny<MatterDto>()))
                .ReturnsAsync(false);

            var result = await controller.DeleteMatterAsync(Guid.NewGuid());
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that DeleteMatterAsync returns NoContent when successful.
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ReturnsNoContent_WhenSuccessful()
        {
            var controller = CreateController();
            var matter = new Matter
            {
                Documents = (List<Document>) [],
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), true, false))
                .ReturnsAsync(matter);
            _repoMock.Setup(r => r.DeleteMatterAsync(It.IsAny<MatterDto>()))
                .ReturnsAsync(true);

            var result = await controller.DeleteMatterAsync(Guid.NewGuid());
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Tests that DeleteMatterAsync returns InternalServerError on exception.
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();

            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), true, false))
                .ThrowsAsync(new Exception("Test"));

            var result = await controller.DeleteMatterAsync(Guid.NewGuid());
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region GetMatterAsync

        /// <summary>
        /// Tests that GetMatterAsync returns BadRequest when GUID is invalid.
        /// </summary>
        [Fact]
        public async Task GetMatterAsync_ReturnsBadRequest_WhenGuidInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new BadRequestResult());

            var result = await controller.GetMatterAsync(Guid.Empty);
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that GetMatterAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task GetMatterAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetMatterAsync(Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that GetMatterAsync returns NotFound when matter is null from repository.
        /// </summary>
        [Fact]
        public async Task GetMatterAsync_ReturnsNotFound_WhenMatterNullFromRepo()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), false, false))
                .ReturnsAsync((Matter?)null);

            var result = await controller.GetMatterAsync(Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Tests that GetMatterAsync returns Ok with correct DTO when includeDocuments is false.
        /// </summary>
        [Fact]
        public async Task GetMatterAsync_ReturnsOk_WithoutDocuments()
        {
            var controller = CreateController();
            var matter = new Matter
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var dto = new MatterWithoutDocumentsDto
            {
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), false, false))
                .ReturnsAsync(matter);
            _mapperMock.Setup(m => m.Map<MatterWithoutDocumentsDto>(matter))
                .Returns(dto);

            var result = await controller.GetMatterAsync(Guid.NewGuid());
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, okResult.Value);
        }

        /// <summary>
        /// Tests that GetMatterAsync returns Ok with correct DTO when includeDocuments is true.
        /// </summary>
        [Fact]
        public async Task GetMatterAsync_ReturnsOk_WithDocuments()
        {
            var controller = CreateController();
            var matter = new Matter
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var dto = new MatterWithDocumentsDto
            {
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), true, false))
                .ReturnsAsync(matter);
            _mapperMock.Setup(m => m.Map<MatterWithDocumentsDto>(matter))
                .Returns(dto);

            var result = await controller.GetMatterAsync(Guid.NewGuid(), true);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, okResult.Value);
        }

        /// <summary>
        /// Tests that GetMatterAsync returns InternalServerError on exception.
        /// </summary>
        [Fact]
        public async Task GetMatterAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), false, false))
                .ThrowsAsync(new Exception("Test"));

            var result = await controller.GetMatterAsync(Guid.NewGuid());
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region GetAuditsAsync

        /// <summary>
        /// Tests that GetAuditsAsync returns BadRequest when GUID is invalid.
        /// </summary>
        [Fact]
        public async Task GetAuditsAsync_ReturnsBadRequest_WhenGuidInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new BadRequestResult());

            var result = await controller.GetAuditsAsync(Guid.Empty, "from");
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that GetAuditsAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task GetAuditsAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetAuditsAsync(Guid.NewGuid(), "from");
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that GetAuditsAsync returns BadRequest for invalid historyType.
        /// </summary>
        [Fact]
        public async Task GetAuditsAsync_ReturnsBadRequest_WhenHistoryTypeInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);

            var result = await controller.GetAuditsAsync(Guid.NewGuid(), "invalid");
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that GetAuditsAsync returns Ok when audits are found.
        /// </summary>
        [Fact]
        public async Task GetAuditsAsync_ReturnsOk_WhenAuditsFound()
        {
            var controller = CreateController();
            var audits = new List<MatterDocumentActivityUserMinimalDto> { new()
                {
                    MatterId = Guid.NewGuid(),
                    DocumentId = Guid.NewGuid(),
                    MatterDocumentActivityId = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                }
            }.AsQueryable();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetExtendedAuditsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<AuditEnums.AuditDirection>()))
                .ReturnsAsync(audits);

            var result = await controller.GetAuditsAsync(Guid.NewGuid(), "from");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        /// <summary>
        /// Tests that GetAuditsAsync returns NotFound when no audits are found.
        /// </summary>
        [Fact]
        public async Task GetAuditsAsync_ReturnsNotFound_WhenNoAuditsFound()
        {
            var controller = CreateController();
            var audits = new List<MatterDocumentActivityUserMinimalDto>().AsQueryable();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetExtendedAuditsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<AuditEnums.AuditDirection>()))
                .ReturnsAsync(audits);

            var result = await controller.GetAuditsAsync(Guid.NewGuid(), "from");
            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Tests that GetAuditsAsync returns InternalServerError on exception.
        /// </summary>
        [Fact]
        public async Task GetAuditsAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetExtendedAuditsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<AuditEnums.AuditDirection>()))
                .ThrowsAsync(new Exception("Test"));

            var result = await controller.GetAuditsAsync(Guid.NewGuid(), "from");
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region UpdateMatterAsync

        /// <summary>
        /// Tests that UpdateMatterAsync returns BadRequest when DTO is null.
        /// </summary>
        [Fact]
        public async Task UpdateMatterAsync_ReturnsBadRequest_WhenDtoIsNull()
        {
            var controller = CreateController();
            var result = await controller.UpdateMatterAsync(Guid.NewGuid(), null);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that UpdateMatterAsync returns BadRequest when GUID is invalid.
        /// </summary>
        [Fact]
        public async Task UpdateMatterAsync_ReturnsBadRequest_WhenGuidInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new BadRequestResult());

            var result = await controller.UpdateMatterAsync(Guid.Empty, new MatterForUpdateDto
            {
                Description = "Test",
                CreationDate = DateTime.UtcNow
            });
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that UpdateMatterAsync returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task UpdateMatterAsync_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("Description", "Required");
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateModelState(controller.ModelState))
                .Returns(new BadRequestResult());

            var result = await controller.UpdateMatterAsync(Guid.NewGuid(), new MatterForUpdateDto
            {
                Description = "",
                CreationDate = DateTime.UtcNow
            });
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that UpdateMatterAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateMatterAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateModelState(controller.ModelState))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.UpdateMatterAsync(Guid.NewGuid(), new MatterForUpdateDto
            {
                Description = "Test",
                CreationDate = DateTime.UtcNow
            });
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that UpdateMatterAsync returns InternalServerError if update fails.
        /// </summary>
        [Fact]
        public async Task UpdateMatterAsync_ReturnsInternalServerError_WhenUpdateFails()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateModelState(controller.ModelState))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.UpdateMatterAsync(It.IsAny<Guid>(), It.IsAny<MatterForUpdateDto>()))
                .ReturnsAsync((Matter?)null);

            var result = await controller.UpdateMatterAsync(Guid.NewGuid(), new MatterForUpdateDto
            {
                Description = "Test",
                CreationDate = DateTime.UtcNow
            });
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that UpdateMatterAsync returns NoContent when successful.
        /// </summary>
        [Fact]
        public async Task UpdateMatterAsync_ReturnsNoContent_WhenSuccessful()
        {
            var controller = CreateController();
            var matter = new Matter
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateModelState(controller.ModelState))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.UpdateMatterAsync(It.IsAny<Guid>(), It.IsAny<MatterForUpdateDto>()))
                .ReturnsAsync(matter);

            var result = await controller.UpdateMatterAsync(Guid.NewGuid(), new MatterForUpdateDto
            {
                Description = "Test",
                CreationDate = DateTime.UtcNow
            });
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Tests that UpdateMatterAsync returns InternalServerError on exception.
        /// </summary>
        [Fact]
        public async Task UpdateMatterAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateModelState(controller.ModelState))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.UpdateMatterAsync(It.IsAny<Guid>(), It.IsAny<MatterForUpdateDto>()))
                .ThrowsAsync(new Exception("Test"));

            var result = await controller.UpdateMatterAsync(Guid.NewGuid(), new MatterForUpdateDto
            {
                Description = "Test",
                CreationDate = DateTime.UtcNow
            });
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region GetMatterHistoryAsync

        /// <summary>
        /// Tests that GetMatterHistoryAsync returns BadRequest when GUID is invalid.
        /// </summary>
        [Fact]
        public async Task GetMatterHistoryAsync_ReturnsBadRequest_WhenGuidInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new BadRequestResult());

            var result = await controller.GetMatterHistoryAsync(Guid.Empty);
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that GetMatterHistoryAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task GetMatterHistoryAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetMatterHistoryAsync(Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that GetMatterHistoryAsync returns NotFound when matterWithHistory is null.
        /// </summary>
        [Fact]
        public async Task GetMatterHistoryAsync_ReturnsNotFound_WhenMatterWithHistoryNull()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), false, true))
                .ReturnsAsync((Matter?)null);

            var result = await controller.GetMatterHistoryAsync(Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Tests that GetMatterHistoryAsync returns Ok when successful.
        /// </summary>
        [Fact]
        public async Task GetMatterHistoryAsync_ReturnsOk_WhenSuccessful()
        {
            var controller = CreateController();
            var matter = new Matter
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var dto = new MatterWithoutDocumentsDto
            {
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), false, true))
                .ReturnsAsync(matter);
            _mapperMock.Setup(m => m.Map<MatterWithoutDocumentsDto>(matter))
                .Returns(dto);

            var result = await controller.GetMatterHistoryAsync(Guid.NewGuid());
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, okResult.Value);
        }

        /// <summary>
        /// Tests that GetMatterHistoryAsync returns InternalServerError on exception.
        /// </summary>
        [Fact]
        public async Task GetMatterHistoryAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), false, true))
                .ThrowsAsync(new Exception("Test"));

            var result = await controller.GetMatterHistoryAsync(Guid.NewGuid());
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region RestoreMatterAsync

        /// <summary>
        /// Tests that RestoreMatterAsync returns BadRequest when GUID is invalid.
        /// </summary>
        [Fact]
        public async Task RestoreMatterAsync_ReturnsBadRequest_WhenGuidInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new BadRequestResult());

            var result = await controller.RestoreMatterAsync(Guid.Empty);
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that RestoreMatterAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task RestoreMatterAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.RestoreMatterAsync(Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that RestoreMatterAsync returns BadRequest if restoration fails.
        /// </summary>
        [Fact]
        public async Task RestoreMatterAsync_ReturnsBadRequest_WhenRestorationFails()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.RestoreMatterAsync(It.IsAny<Guid>()))
                .ReturnsAsync(false);

            var result = await controller.RestoreMatterAsync(Guid.NewGuid());
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that RestoreMatterAsync returns Ok when successful.
        /// </summary>
        [Fact]
        public async Task RestoreMatterAsync_ReturnsOk_WhenSuccessful()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.RestoreMatterAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            var result = await controller.RestoreMatterAsync(Guid.NewGuid());
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Matter restored successfully.", okResult.Value);
        }

        /// <summary>
        /// Tests that RestoreMatterAsync returns InternalServerError on exception.
        /// </summary>
        [Fact]
        public async Task RestoreMatterAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.RestoreMatterAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Test"));

            var result = await controller.RestoreMatterAsync(Guid.NewGuid());
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region GetMattersAsync

        /// <summary>
        /// Tests that GetMattersAsync returns BadRequest when resourceParameters is null.
        /// </summary>
        [Fact]
        public async Task GetMattersAsync_ReturnsBadRequest_WhenResourceParametersNull()
        {
            var controller = CreateController();
            var result = await controller.GetMattersAsync(null);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that GetMattersAsync returns BadRequest for invalid pagination.
        /// </summary>
        [Fact]
        public async Task GetMattersAsync_ReturnsBadRequest_WhenPaginationInvalid()
        {
            var controller = CreateController();
            var resourceParameters = new MattersResourceParameters { PageNumber = 0, PageSize = 0 };
            var result = await controller.GetMattersAsync(resourceParameters);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that GetMattersAsync returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task GetMattersAsync_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("PageNumber", "Required");
            var resourceParameters = new MattersResourceParameters { PageNumber = 1, PageSize = 1 };
            _validationServiceMock.Setup(v => v.ValidateModelState(controller.ModelState))
                .Returns(new BadRequestResult());

            var result = await controller.GetMattersAsync(resourceParameters);
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that GetMattersAsync returns Ok with results when successful.
        /// </summary>
        [Fact]
        public async Task GetMattersAsync_ReturnsOk_WhenSuccessful()
        {
            var controller = CreateController();
            var pagedMatters = new Helpers.PagedList<Matter>([
                new Matter
                {
                    Id = Guid.NewGuid(),
                    Description = "Test Matter",
                    CreationDate = DateTime.UtcNow
                }
            ], 1, 1, 1);
            var resourceParameters = new MattersResourceParameters { PageNumber = 1, PageSize = 1 };
            var mapped = new List<MatterWithoutDocumentsDto> { new()
                {
                    Description = "Test Matter",
                    CreationDate = DateTime.UtcNow
                }
            };
            _validationServiceMock.Setup(v => v.ValidateModelState(controller.ModelState))
                .Returns((ActionResult?)null);
            _repoMock.Setup(r => r.GetPaginatedMattersAsync(resourceParameters))
                .ReturnsAsync(pagedMatters);
            _mapperMock.Setup(m => m.Map<IEnumerable<MatterWithoutDocumentsDto>>(pagedMatters))
                .Returns(mapped);

            var result = await controller.GetMattersAsync(resourceParameters);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        /// <summary>
        /// Tests that GetMattersAsync returns InternalServerError on exception.
        /// </summary>
        [Fact]
        public async Task GetMattersAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            var resourceParameters = new MattersResourceParameters { PageNumber = 1, PageSize = 1 };
            _validationServiceMock.Setup(v => v.ValidateModelState(controller.ModelState))
                .Returns((ActionResult?)null);
            _repoMock.Setup(r => r.GetPaginatedMattersAsync(resourceParameters))
                .ThrowsAsync(new Exception("Test"));

            var result = await controller.GetMattersAsync(resourceParameters);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}