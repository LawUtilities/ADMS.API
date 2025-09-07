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
    /// Unit tests for <see cref="RevisionController.GetRevisionsAsync"/>.
    /// </summary>
    public class RevisionControllerGetRevisionsAsyncTests
    {
        private readonly Mock<ILogger<RevisionController>> _loggerMock = new();
        private readonly Mock<IAdmsRepository> _repoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<ProblemDetailsFactory> _problemDetailsFactoryMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        /// <summary>
        /// Creates and initializes a new instance of the <see cref="RevisionController"/> class.
        /// </summary>
        /// <remarks>The created controller is configured with mocked dependencies and a default HTTP
        /// context for testing purposes. This method is intended for use in unit tests or scenarios where a fully
        /// initialized <see cref="RevisionController"/> is required.</remarks>
        /// <returns>A fully initialized instance of the <see cref="RevisionController"/> class.</returns>
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
        /// Tests that GetRevisionsAsync returns BadRequest when matterId is invalid.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsBadRequest_WhenMatterIdInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), nameof(Guid)))
                .Returns(new BadRequestResult());

            var result = await controller.GetRevisionsAsync(Guid.Empty, Guid.NewGuid(), new RevisionsResourceParameters());
            Assert.IsType<BadRequestResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionsAsync returns BadRequest when documentId is invalid.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsBadRequest_WhenDocumentIdInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns(new BadRequestResult());

            var result = await controller.GetRevisionsAsync(Guid.NewGuid(), Guid.Empty, new RevisionsResourceParameters());
            Assert.IsType<BadRequestResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionsAsync returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("PageNumber", "Required");
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);

            var result = await controller.GetRevisionsAsync(Guid.NewGuid(), Guid.NewGuid(), new RevisionsResourceParameters());
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionsAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetRevisionsAsync(Guid.NewGuid(), Guid.NewGuid(), new RevisionsResourceParameters());
            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionsAsync returns NotFound when document does not exist.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsNotFound_WhenDocumentNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetRevisionsAsync(Guid.NewGuid(), Guid.NewGuid(), new RevisionsResourceParameters());
            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionsAsync returns BadRequest when orderBy is invalid.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsBadRequest_WhenOrderByInvalid()
        {
            var controller = CreateController();
            var resourceParameters = new RevisionsResourceParameters { OrderBy = "InvalidField" };
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock.Setup(p => p.ValidMappingExistsFor<RevisionDto, Revision>(resourceParameters.OrderBy))
                .Returns(false);

            var result = await controller.GetRevisionsAsync(Guid.NewGuid(), Guid.NewGuid(), resourceParameters);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        #endregion

        #region Repository and Relationship Checks

        /// <summary>
        /// Tests that GetRevisionsAsync returns NotFound when document does not belong to the specified matter.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsNotFound_WhenDocumentNotInMatter()
        {
            var controller = CreateController();
            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var document = new Document
            {
                FileName = "test file",
                Extension = ".txt",
                MatterId = matter.Id,
                Matter = matter,
            };
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock.Setup(p => p.ValidMappingExistsFor<RevisionDto, Revision>(It.IsAny<string>()))
                .Returns(true);
            _repoMock.Setup(r => r.GetDocumentAsync(It.IsAny<Guid>(), false, false))
                .ReturnsAsync(document);

            var result = await controller.GetRevisionsAsync(Guid.NewGuid(), Guid.NewGuid(), new RevisionsResourceParameters());
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests that GetRevisionsAsync returns InternalServerError when paginated revisions retrieval fails.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsInternalServerError_WhenPagedRevisionsNull()
        {
            var controller = CreateController();
            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var document = new Document
            {
                FileName = "test file",
                Extension = ".txt",
                MatterId = matter.Id,
                Matter = matter,
            };

            var pagedResult = new ActionResult<Helpers.PagedList<Revision>>(new Helpers.PagedList<Revision>([], 1, 1, 0));
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(document.Id))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock.Setup(p => p.ValidMappingExistsFor<RevisionDto, Revision>(It.IsAny<string>()))
                .Returns(true);
            _repoMock.Setup(r => r.GetDocumentAsync(document.Id, false, false))
                .ReturnsAsync(document);
            _repoMock.Setup(r => r.GetPaginatedRevisionsAsync(document.Id, It.IsAny<RevisionsResourceParameters>()))
                .ReturnsAsync(pagedResult);

            var result = await controller.GetRevisionsAsync(matter.Id, document.Id, new RevisionsResourceParameters());
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region Success and Exception Handling

        /// <summary>
        /// Tests that GetRevisionsAsync returns Ok with mapped DTOs and pagination metadata when successful.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsOk_WhenSuccessful()
        {
            var controller = CreateController();
            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var document = new Document
            {
                FileName = "test file",
                Extension = ".txt",
                MatterId = matter.Id,
                Matter = matter,
            };
            var pagedRevisions = new Helpers.PagedList<Revision>([], 1, 1, 1);
            var pagedResult = new ActionResult<Helpers.PagedList<Revision>>(pagedRevisions);
            var revisionDtos = new List<RevisionDto> { new() };
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(document.Id))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock.Setup(p => p.ValidMappingExistsFor<RevisionDto, Revision>(It.IsAny<string>()))
                .Returns(true);
            _repoMock.Setup(r => r.GetDocumentAsync(document.Id, false, false))
                .ReturnsAsync(document);
            _repoMock.Setup(r => r.GetPaginatedRevisionsAsync(document.Id, It.IsAny<RevisionsResourceParameters>()))
                .ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<IEnumerable<RevisionDto>>(pagedRevisions))
                .Returns(revisionDtos);

            var result = await controller.GetRevisionsAsync(matter.Id, document.Id, new RevisionsResourceParameters());
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(revisionDtos, okResult.Value);
        }

        /// <summary>
        /// Tests that GetRevisionsAsync returns InternalServerError on exception.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var document = new Document
            {
                FileName = "test file",
                Extension = ".txt",
                MatterId = matter.Id,
                Matter = matter,
            };
            _validationServiceMock.SetupSequence(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null)
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(document.Id))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock.Setup(p => p.ValidMappingExistsFor<RevisionDto, Revision>(It.IsAny<string>()))
                .Returns(true);
            _repoMock.Setup(r => r.GetDocumentAsync(document.Id, false, false))
                .ReturnsAsync(document);
            _repoMock.Setup(r => r.GetPaginatedRevisionsAsync(document.Id, It.IsAny<RevisionsResourceParameters>()))
                .ThrowsAsync(new Exception("Test"));

            var result = await controller.GetRevisionsAsync(matter.Id, document.Id, new RevisionsResourceParameters());
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}