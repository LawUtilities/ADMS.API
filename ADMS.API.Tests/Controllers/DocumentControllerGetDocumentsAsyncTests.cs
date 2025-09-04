using ADMS.API.Controllers;
using ADMS.API.Entities;
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
    /// Unit tests for <see cref="DocumentController.GetDocumentsAsync"/>.
    /// </summary>
    public class DocumentControllerGetDocumentsAsyncTests
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
        /// Tests that GetDocumentsAsync returns BadRequest when matterId is empty.
        /// </summary>
        [Fact]
        public async Task GetDocumentsAsync_ReturnsBadRequest_WhenMatterIdEmpty()
        {
            var controller = CreateController();
            var parameters = new DocumentsResourceParameters();
            var result = await controller.GetDocumentsAsync(Guid.Empty, parameters);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that GetDocumentsAsync returns BadRequest when OrderBy is invalid.
        /// </summary>
        [Fact]
        public async Task GetDocumentsAsync_ReturnsBadRequest_WhenOrderByInvalid()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var parameters = new DocumentsResourceParameters { OrderBy = "InvalidField" };

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock.Setup(p => p.ValidMappingExistsFor<DocumentDto, Document>(parameters.OrderBy))
                .Returns(false);

            var result = await controller.GetDocumentsAsync(matterId, parameters);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that GetDocumentsAsync returns BadRequest when Fields parameter is invalid.
        /// </summary>
        [Fact]
        public async Task GetDocumentsAsync_ReturnsBadRequest_WhenFieldsInvalid()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var parameters = new DocumentsResourceParameters { Fields = "InvalidField" };

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock.Setup(p => p.ValidMappingExistsFor<DocumentDto, Document>(It.IsAny<string>()))
                .Returns(true);
            _propertyCheckerServiceMock.Setup(p => p.TypeHasProperties<DocumentDto>(parameters.Fields))
                .Returns(false);

            var result = await controller.GetDocumentsAsync(matterId, parameters);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Existence and Validation

        /// <summary>
        /// Tests that GetDocumentsAsync returns NotFound when the matter does not exist.
        /// </summary>
        [Fact]
        public async Task GetDocumentsAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var parameters = new DocumentsResourceParameters();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetDocumentsAsync(matterId, parameters);
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Operation and Success

        /// <summary>
        /// Tests that GetDocumentsAsync returns Ok with mapped documents when all parameters are valid.
        /// </summary>
        [Fact]
        public async Task GetDocumentsAsync_ReturnsOk_WithDocuments()
        {
            var controller = CreateController();
            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };

            var parameters = new DocumentsResourceParameters();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock.Setup(p => p.ValidMappingExistsFor<DocumentDto, Document>(It.IsAny<string>()))
                .Returns(true);
            _propertyCheckerServiceMock.Setup(p => p.TypeHasProperties<DocumentDto>(It.IsAny<string>()))
                .Returns(true);

            var pagedDocuments = new Helpers.PagedList<Document>([
                new Document
                {
                    FileSize = 123,
                    Checksum = "abc",
                    IsCheckedOut = false,
                    IsDeleted = false,
                    FileName = "test file",
                    Extension = ".txt",
                    MatterId = matter.Id,
                    Matter = matter
                }
            ], 1,1,10);
            _repoMock.Setup(r => r.GetPaginatedDocumentsAsync(matter.Id, parameters))
                .ReturnsAsync(pagedDocuments);

            var mappedDtos = new List<DocumentDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    IsCheckedOut = false,
                    IsDeleted = false,
                    Revisions = (List<RevisionDto>) [],
                    FileName = "test file",
                    Extension = ".txt"
                }
            };
            _mapperMock.Setup(m => m.Map<IEnumerable<DocumentDto>>(pagedDocuments))
                .Returns(mappedDtos);

            var result = await controller.GetDocumentsAsync(matter.Id, parameters);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<IEnumerable<DocumentDto>>(okResult.Value, false);
            Assert.Single(returned);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Tests that GetDocumentsAsync returns InternalServerError when an exception is thrown during the operation.
        /// </summary>
        [Fact]
        public async Task GetDocumentsAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var parameters = new DocumentsResourceParameters();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _propertyMappingServiceMock.Setup(p => p.ValidMappingExistsFor<DocumentDto, Document>(It.IsAny<string>()))
                .Returns(true);
            _propertyCheckerServiceMock.Setup(p => p.TypeHasProperties<DocumentDto>(It.IsAny<string>()))
                .Returns(true);
            _repoMock.Setup(r => r.GetPaginatedDocumentsAsync(matterId, parameters))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await controller.GetDocumentsAsync(matterId, parameters);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}