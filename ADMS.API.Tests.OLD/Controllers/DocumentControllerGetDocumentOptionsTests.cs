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
    /// Unit tests for <see cref="DocumentController.GetDocumentOptions"/>.
    /// </summary>
    public class DocumentControllerGetDocumentOptionsTests
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

        #region Success

        /// <summary>
        /// Tests that GetDocumentOptions returns NoContent and sets the Allow header with supported HTTP methods.
        /// </summary>
        [Fact]
        public void GetDocumentOptions_ReturnsNoContent_AndSetsAllowHeader()
        {
            // Arrange
            var controller = CreateController();
            var httpContext = controller.ControllerContext.HttpContext;

            // Act
            var result = controller.GetDocumentOptions();

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);

            // The Allow header should be set
            var allowHeader = httpContext.Response.Headers.Allow.ToString();
            Assert.Contains("GET", allowHeader);
            Assert.Contains("HEAD", allowHeader);
            Assert.Contains("POST", allowHeader);
            Assert.Contains("PUT", allowHeader);
            Assert.Contains("DELETE", allowHeader);
            Assert.Contains("OPTIONS", allowHeader);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Tests that GetDocumentOptions returns InternalServerError when an exception is thrown.
        /// </summary>
        [Fact]
        public void GetDocumentOptions_ReturnsInternalServerError_OnException()
        {
            // Arrange
            var controller = CreateController();
            var httpContext = controller.ControllerContext.HttpContext;

            // Simulate an exception by making Response.Headers.Allow throw
            var headersMock = new Mock<IHeaderDictionary>();
            headersMock.SetupProperty(h => h.Allow);
            headersMock.SetupSet(h => h.Allow = It.IsAny<string>()).Throws(new Exception("Header error"));
            Mock.Get(httpContext.Response).SetupGet(r => r.Headers).Returns(headersMock.Object);

            // Setup ProblemDetailsFactory to return a ProblemDetails object
            _problemDetailsFactoryMock.Setup(f => f.CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    null, null, null, null))
                .Returns(new ProblemDetails { Status = StatusCodes.Status500InternalServerError, Detail = "Internal server error" });

            // Act
            var result = controller.GetDocumentOptions();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        }
        #endregion
    }
}