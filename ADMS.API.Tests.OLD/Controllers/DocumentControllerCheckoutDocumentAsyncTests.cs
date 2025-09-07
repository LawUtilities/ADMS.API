using ADMS.API.Controllers;
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
    /// Unit tests for <see cref="DocumentController.CheckoutDocumentAsync"/>.
    /// </summary>
    public class DocumentControllerCheckoutDocumentAsyncTests
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
        /// Tests that CheckoutDocumentAsync returns BadRequest when matterId is empty.
        /// </summary>
        [Fact]
        public async Task CheckoutDocumentAsync_ReturnsBadRequest_WhenMatterIdEmpty()
        {
            var controller = CreateController();
            var result = await controller.CheckoutDocumentAsync(Guid.Empty, Guid.NewGuid());
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that CheckoutDocumentAsync returns BadRequest when documentId is empty.
        /// </summary>
        [Fact]
        public async Task CheckoutDocumentAsync_ReturnsBadRequest_WhenDocumentIdEmpty()
        {
            var controller = CreateController();
            var result = await controller.CheckoutDocumentAsync(Guid.NewGuid(), Guid.Empty);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Existence and Validation

        /// <summary>
        /// Tests that CheckoutDocumentAsync returns NotFound when matter or document does not exist.
        /// </summary>
        [Fact]
        public async Task CheckoutDocumentAsync_ReturnsNotFound_WhenMatterOrDocumentNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());
            var result = await controller.CheckoutDocumentAsync(Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Operation and Success

        /// <summary>
        /// Tests that CheckoutDocumentAsync returns Ok when the checkout operation succeeds.
        /// </summary>
        [Fact]
        public async Task CheckoutDocumentAsync_ReturnsOk_WhenCheckoutSucceeds()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.SetDocumentCheckStateAsync(documentId, true))
                .ReturnsAsync(true);

            var result = await controller.CheckoutDocumentAsync(matterId, documentId);
            Assert.IsType<OkResult>(result);
        }

        /// <summary>
        /// Tests that CheckoutDocumentAsync returns BadRequest when the document cannot be checked out.
        /// </summary>
        [Fact]
        public async Task CheckoutDocumentAsync_ReturnsBadRequest_WhenCannotCheckout()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.SetDocumentCheckStateAsync(documentId, true))
                .ReturnsAsync(false);

            var result = await controller.CheckoutDocumentAsync(matterId, documentId);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Tests that CheckoutDocumentAsync returns InternalServerError when an exception is thrown during the operation.
        /// </summary>
        [Fact]
        public async Task CheckoutDocumentAsync_ReturnsInternalServerError_OnException()
        {
            var controller = CreateController();
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);
            _repoMock.Setup(r => r.SetDocumentCheckStateAsync(documentId, true))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await controller.CheckoutDocumentAsync(matterId, documentId);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion
    }
}