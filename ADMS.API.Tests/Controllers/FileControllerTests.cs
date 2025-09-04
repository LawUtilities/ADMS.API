using ADMS.API.Controllers;
using ADMS.API.Entities;
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
    /// Unit tests for <see cref="FileController"/>.
    /// </summary>
    public class FileControllerTests
    {
        private readonly Mock<ILogger<FileController>> _loggerMock = new();
        private readonly Mock<IAdmsRepository> _repoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ProblemDetailsFactory> _problemDetailsFactoryMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();
        private readonly Mock<IVirusScanner> _virusScannerMock = new();
        private readonly Mock<IFileStorage> _fileStorageMock = new();

        private FileController CreateController()
        {
            var controller = new FileController(
                _loggerMock.Object,
                _repoMock.Object,
                _mapperMock.Object,
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

        #region UploadExistingFileAsync

        /// <summary>
        /// Tests that UploadExistingFileAsync returns BadRequest when any GUID is invalid.
        /// </summary>
        [Fact]
        public async Task UploadExistingFileAsync_ReturnsBadRequest_WhenGuidInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new BadRequestResult());

            var result = await controller.UploadExistingFileAsync(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), null, CancellationToken.None);
            Assert.IsType<IActionResult>(result, exactMatch: false);
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that UploadExistingFileAsync returns BadRequest when file is null or empty.
        /// </summary>
        [Fact]
        public async Task UploadExistingFileAsync_ReturnsBadRequest_WhenFileIsNullOrEmpty()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            var result = await controller.UploadExistingFileAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, CancellationToken.None);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests that UploadExistingFileAsync returns PayloadTooLarge when file is too big.
        /// </summary>
        [Fact]
        public async Task UploadExistingFileAsync_ReturnsPayloadTooLarge_WhenFileTooBig()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(51 * 1024 * 1024); // 51MB
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            var result = await controller.UploadExistingFileAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), fileMock.Object, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status413PayloadTooLarge, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that UploadExistingFileAsync returns UnsupportedMediaType for disallowed extension.
        /// </summary>
        [Fact]
        public async Task UploadExistingFileAsync_ReturnsUnsupportedMediaType_WhenExtensionNotAllowed()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns("test.badext");
            var result = await controller.UploadExistingFileAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), fileMock.Object, CancellationToken.None);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status415UnsupportedMediaType, objectResult.StatusCode);
        }

        // Additional tests for virus scan, repository failures, and success scenarios would follow a similar pattern.
        // For brevity, only the main branches are shown here.

        #endregion

        #region DownloadFileAsync

        /// <summary>
        /// Tests that DownloadFileAsync returns BadRequest when any GUID is invalid.
        /// </summary>
        [Fact]
        public async Task DownloadFileAsync_ReturnsBadRequest_WhenGuidInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new BadRequestResult());

            var result = await controller.DownloadFileAsync(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<IActionResult>(result, exactMatch: false);
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that DownloadFileAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task DownloadFileAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
                _repoMock.Setup(r => r.GetMatterAsync(It.IsAny<Guid>(), false, false)).ReturnsAsync(null as Matter);

            var result = await controller.DownloadFileAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // Additional tests for document/revision not found, file not found, and success would follow.

        #endregion

        #region DownloadPdfAsync

        /// <summary>
        /// Tests that DownloadPdfAsync returns BadRequest when any GUID is invalid.
        /// </summary>
        [Fact]
        public async Task DownloadPdfAsync_ReturnsBadRequest_WhenGuidInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new BadRequestResult());

            var result = await controller.DownloadPdfAsync(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<IActionResult>(result, exactMatch: false);
            Assert.IsType<BadRequestResult>(result);
        }

        // Additional tests for not found, conversion failure, and success would follow.

        #endregion

        #region VerifyFileTypeAsync

        /// <summary>
        /// Tests that VerifyFileTypeAsync returns BadRequest when any GUID is invalid.
        /// </summary>
        [Fact]
        public async Task VerifyFileTypeAsync_ReturnsBadRequest_WhenGuidInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new BadRequestResult());

            var result = await controller.VerifyFileTypeAsync(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), null);
            Assert.IsType<IActionResult>(result, exactMatch: false);
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that VerifyFileTypeAsync returns BadRequest when file is null or empty.
        /// </summary>
        [Fact]
        public async Task VerifyFileTypeAsync_ReturnsBadRequest_WhenFileIsNullOrEmpty()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);

            var result = await controller.VerifyFileTypeAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // Additional tests for not found, unsupported media type, virus, and success would follow.

        #endregion

        #region UploadNewFileAsync

        /// <summary>
        /// Tests that UploadNewFileAsync returns BadRequest when matterId is invalid.
        /// </summary>
        [Fact]
        public async Task UploadNewFileAsync_ReturnsBadRequest_WhenMatterIdInvalid()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new BadRequestResult());

            var result = await controller.UploadNewFileAsync(Guid.Empty, null, CancellationToken.None);
            Assert.IsType<IActionResult>(result, exactMatch: false);
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Tests that UploadNewFileAsync returns NotFound when matter does not exist.
        /// </summary>
        [Fact]
        public async Task UploadNewFileAsync_ReturnsNotFound_WhenMatterNotFound()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.UploadNewFileAsync(Guid.NewGuid(), null, CancellationToken.None);
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that UploadNewFileAsync returns BadRequest when file is null or empty.
        /// </summary>
        [Fact]
        public async Task UploadNewFileAsync_ReturnsBadRequest_WhenFileIsNullOrEmpty()
        {
            var controller = CreateController();
            _validationServiceMock.Setup(v => v.ValidateGuid(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((ActionResult?)null);

            var result = await controller.UploadNewFileAsync(Guid.NewGuid(), null, CancellationToken.None);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // Additional tests for file too large, unsupported extension, duplicate file, virus, and success would follow.

        #endregion
    }
}