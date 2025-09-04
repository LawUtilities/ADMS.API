using ADMS.API.Models;
using ADMS.API.Services;
using ADMS.API.Services.Common;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

using Moq;

namespace ADMS.API.Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="ValidationService"/>.
    /// </summary>
    public class ValidationServiceTests
    {
        private readonly Mock<IEntityExistenceValidator> _entityExistenceValidatorMock = new();
        private readonly Mock<ILogger<ValidationService>> _loggerMock = new();
        private readonly Mock<ProblemDetailsFactory> _problemDetailsFactoryMock = new();

        private ValidationService CreateService() =>
            new(_entityExistenceValidatorMock.Object, _loggerMock.Object, _problemDetailsFactoryMock.Object);

        #region ValidateGuid

        /// <summary>
        /// Tests that ValidateGuid returns null for a non-empty Guid.
        /// </summary>
        [Fact]
        public void ValidateGuid_ReturnsNull_ForValidGuid()
        {
            var service = CreateService();
            var result = service.ValidateGuid(Guid.NewGuid(), "TestId");
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ValidateGuid returns BadRequestObjectResult for an empty Guid.
        /// </summary>
        [Fact]
        public void ValidateGuid_ReturnsBadRequest_ForEmptyGuid()
        {
            var service = CreateService();
            var result = service.ValidateGuid(Guid.Empty, "TestId");
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid TestId", badRequest.Value?.ToString());
        }

        #endregion

        #region ValidateStringNotEmpty

        /// <summary>
        /// Tests that ValidateStringNotEmpty returns true for a non-empty string.
        /// </summary>
        [Fact]
        public void ValidateStringNotEmpty_ReturnsTrue_ForNonEmptyString()
        {
            var service = CreateService();
            Assert.True(service.ValidateStringNotEmpty("abc", "TestParam"));
        }

        /// <summary>
        /// Tests that ValidateStringNotEmpty returns false for a null or whitespace string.
        /// </summary>
        [Theory]
#pragma warning disable xUnit1012 // Null should only be used for nullable parameters
        [InlineData(null)]
#pragma warning restore xUnit1012 // Null should only be used for nullable parameters
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateStringNotEmpty_ReturnsFalse_ForNullOrWhitespace(string value)
        {
            var service = CreateService();
            Assert.False(service.ValidateStringNotEmpty(value, "TestParam"));
        }

        #endregion

        #region ValidateNotNull

        /// <summary>
        /// Tests that ValidateNotNull returns null for a non-null object.
        /// </summary>
        [Fact]
        public void ValidateNotNull_ReturnsNull_ForNonNullObject()
        {
            var service = CreateService();
            Assert.Null(service.ValidateNotNull(new object(), "TestParam"));
        }

        /// <summary>
        /// Tests that ValidateNotNull returns BadRequestObjectResult for a null object.
        /// </summary>
        [Fact]
        public void ValidateNotNull_ReturnsBadRequest_ForNullObject()
        {
            var service = CreateService();
            var result = service.ValidateNotNull(null, "TestParam");
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("TestParam cannot be null", badRequest.Value?.ToString());
        }

        #endregion

        #region ValidateModelState

        /// <summary>
        /// Tests that ValidateModelState returns null for a valid model state.
        /// </summary>
        [Fact]
        public void ValidateModelState_ReturnsNull_ForValidModelState()
        {
            var service = CreateService();
            var modelState = new ModelStateDictionary();
            Assert.Null(service.ValidateModelState(modelState));
        }

        /// <summary>
        /// Tests that ValidateModelState returns BadRequestObjectResult for an invalid model state.
        /// </summary>
        [Fact]
        public void ValidateModelState_ReturnsBadRequest_ForInvalidModelState()
        {
            var service = CreateService();
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("key", "error");
            var result = service.ValidateModelState(modelState);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(modelState, badRequest.Value);
        }

        #endregion

        #region ValidateMatterExistsAsync

        /// <summary>
        /// Tests that ValidateMatterExistsAsync returns null if the matter exists.
        /// </summary>
        [Fact]
        public async Task ValidateMatterExistsAsync_ReturnsNull_IfMatterExists()
        {
            var service = CreateService();
            _entityExistenceValidatorMock.Setup(x => x.MatterExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await service.ValidateMatterExistsAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ValidateMatterExistsAsync returns NotFoundObjectResult if the matter does not exist.
        /// </summary>
        [Fact]
        public async Task ValidateMatterExistsAsync_ReturnsNotFound_IfMatterDoesNotExist()
        {
            var service = CreateService();
            var id = Guid.NewGuid();
            _entityExistenceValidatorMock.Setup(x => x.MatterExistsAsync(id)).ReturnsAsync(false);
            var result = await service.ValidateMatterExistsAsync(id);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains(id.ToString(), notFound.Value?.ToString());
        }

        #endregion

        #region ValidateDocumentExistsAsync

        /// <summary>
        /// Tests that ValidateDocumentExistsAsync returns null if the document exists.
        /// </summary>
        [Fact]
        public async Task ValidateDocumentExistsAsync_ReturnsNull_IfDocumentExists()
        {
            var service = CreateService();
            _entityExistenceValidatorMock.Setup(x => x.DocumentExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await service.ValidateDocumentExistsAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ValidateDocumentExistsAsync returns NotFoundObjectResult if the document does not exist.
        /// </summary>
        [Fact]
        public async Task ValidateDocumentExistsAsync_ReturnsNotFound_IfDocumentDoesNotExist()
        {
            var service = CreateService();
            var id = Guid.NewGuid();
            _entityExistenceValidatorMock.Setup(x => x.DocumentExistsAsync(id)).ReturnsAsync(false);
            var result = await service.ValidateDocumentExistsAsync(id);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains(id.ToString(), notFound.Value?.ToString());
        }

        #endregion

        #region ValidateRevisionExistsAsync

        /// <summary>
        /// Tests that ValidateRevisionExistsAsync returns null if the revision exists.
        /// </summary>
        [Fact]
        public async Task ValidateRevisionExistsAsync_ReturnsNull_IfRevisionExists()
        {
            var service = CreateService();
            _entityExistenceValidatorMock.Setup(x => x.RevisionExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await service.ValidateRevisionExistsAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ValidateRevisionExistsAsync returns NotFoundObjectResult if the revision does not exist.
        /// </summary>
        [Fact]
        public async Task ValidateRevisionExistsAsync_ReturnsNotFound_IfRevisionDoesNotExist()
        {
            var service = CreateService();
            var id = Guid.NewGuid();
            _entityExistenceValidatorMock.Setup(x => x.RevisionExistsAsync(id)).ReturnsAsync(false);
            var result = await service.ValidateRevisionExistsAsync(id);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains(id.ToString(), notFound.Value?.ToString());
        }

        #endregion

        #region ValidateDocumentForCreationAsync

        /// <summary>
        /// Tests that ValidateDocumentForCreationAsync returns validation errors for missing or invalid fields.
        /// </summary>
        [Fact]
        public async Task ValidateDocumentForCreationAsync_ReturnsValidationErrors_ForInvalidFields()
        {
            var service = CreateService();
            var matterId = Guid.NewGuid();
            var document = new DocumentForCreationDto
            {
                FileName = "",
                Extension = "bad*ext",
                MimeType = "bad/type",
                Checksum = "abc",
                Description = null,
                FileSize = 0,
                IsCheckedOut = false
            };

            _entityExistenceValidatorMock.Setup(x => x.FileNameExistsAsync(matterId, It.IsAny<string>(), default)).ReturnsAsync(false);

            var results = await service.ValidateDocumentForCreationAsync(matterId, document);

            var validationResults = results.ToList(); // Simplified collection initialization
            Assert.Contains(validationResults, r => r.ErrorMessage == "FileName is required.");
            Assert.Contains(validationResults, r => r.ErrorMessage == "Invalid file extension.");
            Assert.Contains(validationResults, r => r.ErrorMessage == "Invalid MIME type.");
        }

        /// <summary>
        /// Tests that ValidateDocumentForCreationAsync returns a validation error for duplicate file name.
        /// </summary>
        [Fact]
        public async Task ValidateDocumentForCreationAsync_ReturnsValidationError_ForDuplicateFileName()
        {
            var service = CreateService();
            var matterId = Guid.NewGuid();
            var document = new DocumentForCreationDto
            {
                FileName = "file",
                Extension = "txt",
                MimeType = "text/plain",
                Checksum = "abc",
                Description = null,
                FileSize = 0,
                IsCheckedOut = false
            };

            _entityExistenceValidatorMock.Setup(x => x.FileNameExistsAsync(matterId, "file", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var results = await service.ValidateDocumentForCreationAsync(matterId, document);

            Assert.Contains(results, r => r.ErrorMessage!.Contains("already exists"));
        }

        /// <summary>
        /// Tests that ValidateDocumentForCreationAsync returns an empty list for valid input.
        /// </summary>
        [Fact]
        public async Task ValidateDocumentForCreationAsync_ReturnsNoErrors_ForValidInput()
        {
            var service = CreateService();
            var matterId = Guid.NewGuid();
            var document = new DocumentForCreationDto
            {
                FileName = "file",
                Extension = "txt",
                MimeType = "text/plain",
                Checksum = "abc",
                Description = null,
                FileSize = 0,
                IsCheckedOut = false
            };

            _entityExistenceValidatorMock.Setup(x => x.FileNameExistsAsync(matterId, "file", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var results = await service.ValidateDocumentForCreationAsync(matterId, document);

            Assert.Empty(results);
        }

        #endregion

        #region ValidateDocumentForUpdate

        /// <summary>
        /// Tests that ValidateDocumentForUpdate returns validation errors for missing or invalid fields.
        /// </summary>
        [Fact]
        public void ValidateDocumentForUpdate_ReturnsValidationErrors_ForInvalidFields()
        {
            var service = CreateService();
            var document = new DocumentForUpdateDto
            {
                FileName = "",
                Extension = "bad*ext",
                MimeType = "bad/type",
                Checksum = "abc",
                Description = null,
                FileSize = 0,
                IsCheckedOut = false,
                IsDeleted = false
            };

            var results = service.ValidateDocumentForUpdate(document);

            var validationResults = results.ToList(); // Simplified collection initialization
            Assert.Contains(validationResults, r => r.ErrorMessage == "FileName is required.");
            Assert.Contains(validationResults, r => r.ErrorMessage == "Invalid file extension.");
            Assert.Contains(validationResults, r => r.ErrorMessage == "Invalid MIME type.");
        }

        /// <summary>
        /// Tests that ValidateDocumentForUpdate returns a validation error if both IsDeleted and IsCheckedOut are true.
        /// </summary>
        [Fact]
        public void ValidateDocumentForUpdate_ReturnsValidationError_IfDeletedAndCheckedOut()
        {
            var service = CreateService();
            var document = new DocumentForUpdateDto
            {
                FileName = "file",
                Extension = "txt",
                MimeType = "text/plain",
                Checksum = "abc",
                Description = null,
                FileSize = 0,
                IsCheckedOut = true,
                IsDeleted = true
            };

            var results = service.ValidateDocumentForUpdate(document);

            Assert.Contains(results, r => r.ErrorMessage == "A document cannot be both deleted and checked out.");
        }

        /// <summary>
        /// Tests that ValidateDocumentForUpdate returns no errors for valid input.
        /// </summary>
        [Fact]
        public void ValidateDocumentForUpdate_ReturnsNoErrors_ForValidInput()
        {
            var service = CreateService();
            var document = new DocumentForUpdateDto
            {
                FileName = "file",
                Extension = "txt",
                MimeType = "text/plain",
                Checksum = "abc",
                Description = null,
                FileSize = 0,
                IsCheckedOut = false,
                IsDeleted = false
            };

            var results = service.ValidateDocumentForUpdate(document);

            Assert.Empty(results);
        }

        #endregion

        #region ValidateEmail

        /// <summary>
        /// Tests that ValidateEmail returns true for a valid email.
        /// </summary>
        [Fact]
        public void ValidateEmail_ReturnsTrue_ForValidEmail()
        {
            var service = CreateService();
            Assert.True(service.ValidateEmail("test@example.com", "Email"));
        }

        /// <summary>
        /// Tests that ValidateEmail returns false for an invalid email.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("not-an-email")]
        [InlineData("test@")]
        [InlineData("@example.com")]
        public void ValidateEmail_ReturnsFalse_ForInvalidEmail(string email)
        {
            var service = CreateService();
            Assert.False(service.ValidateEmail(email, "Email"));
        }

        #endregion

        #region ValidateFileExtension

        /// <summary>
        /// Tests that ValidateFileExtension returns true for a valid extension.
        /// </summary>
        [Theory]
        [InlineData("txt")]
        [InlineData(".pdf")]
        [InlineData("docx")]
        public void ValidateFileExtension_ReturnsTrue_ForValidExtension(string ext)
        {
            var service = CreateService();
            Assert.True(service.ValidateFileExtension(ext, "Extension"));
        }

        /// <summary>
        /// Tests that ValidateFileExtension returns false for an invalid extension.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("bad*ext")]
        [InlineData(".")]
        [InlineData("toolongext")]
        public void ValidateFileExtension_ReturnsFalse_ForInvalidExtension(string ext)
        {
            var service = CreateService();
            Assert.False(service.ValidateFileExtension(ext, "Extension"));
        }

        #endregion

        #region ValidateMimeType

        /// <summary>
        /// Tests that ValidateMimeType returns true for a valid MIME type.
        /// </summary>
        [Theory]
        [InlineData("application/pdf")]
        [InlineData("text/plain")]
        [InlineData("image/jpeg")]
        public void ValidateMimeType_ReturnsTrue_ForValidMimeType(string mime)
        {
            var service = CreateService();
            Assert.True(service.ValidateMimeType(mime, "MimeType"));
        }

        /// <summary>
        /// Tests that ValidateMimeType returns false for an invalid MIME type.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("badtype")]
        [InlineData("image/")]
        [InlineData("/jpeg")]
        public void ValidateMimeType_ReturnsFalse_ForInvalidMimeType(string mime)
        {
            var service = CreateService();
            Assert.False(service.ValidateMimeType(mime, "MimeType"));
        }

        #endregion
    }
}