using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Services;

using MapsterMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace ADMS.API.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="AdmsRepository.UpdateRevisionAsync"/>.
    /// </summary>
    public class AdmsRepositoryUpdateRevisionAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        /// <summary>
        /// Helper to create an in-memory context.
        /// </summary>
        private static AdmsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AdmsContext(options);
        }

        /// <summary>
        /// Returns a new instance of <see cref="AdmsRepository"/> with the provided context and mocks.
        /// </summary>
        private AdmsRepository CreateRepository(AdmsContext context)
        {
            return new AdmsRepository(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object
            );
        }

        /// <summary>
        /// Test: Returns null and logs if revision is null.
        /// </summary>
        [Fact]
        public async Task ReturnsNull_WhenRevisionIsNull()
        {
            // Arrange
            var repo = CreateRepository(CreateContext());
            _validationServiceMock
                .Setup(v => v.ValidateNotNull(null, "revision"))
                .Returns(new BadRequestResult());

            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var result = await repo.UpdateRevisionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
#pragma warning restore CS8625

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Revision cannot be null.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Test: Returns null and logs if matter does not exist.
        /// </summary>
        [Fact]
        public async Task ReturnsNull_WhenMatterDoesNotExist()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var revision = new Revision();
            _validationServiceMock.Setup(v => v.ValidateNotNull(revision, "revision")).Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId)).ReturnsAsync(new NotFoundResult());
            var repo = CreateRepository(CreateContext());

            // Act
            var result = await repo.UpdateRevisionAsync(matterId, Guid.NewGuid(), Guid.NewGuid(), revision);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Matter with ID")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Test: Returns null and logs if document does not exist.
        /// </summary>
        [Fact]
        public async Task ReturnsNull_WhenDocumentDoesNotExist()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var revision = new Revision();
            _validationServiceMock.Setup(v => v.ValidateNotNull(revision, "revision")).Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId)).ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId)).ReturnsAsync(new NotFoundResult());
            var repo = CreateRepository(CreateContext());

            // Act
            var result = await repo.UpdateRevisionAsync(matterId, documentId, Guid.NewGuid(), revision);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Document with ID")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Test: Returns null and logs if revision does not exist.
        /// </summary>
        [Fact]
        public async Task ReturnsNull_WhenRevisionDoesNotExist()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var revisionId = Guid.NewGuid();
            var revision = new Revision();
            _validationServiceMock.Setup(v => v.ValidateNotNull(revision, "revision")).Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId)).ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId)).ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateRevisionExistsAsync(revisionId)).ReturnsAsync(new NotFoundResult());
            var repo = CreateRepository(CreateContext());

            // Act
            var result = await repo.UpdateRevisionAsync(matterId, documentId, revisionId, revision);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Revision with ID")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Test: Returns null and logs if the user is not found.
        /// </summary>
        [Fact]
        public async Task ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var revisionId = Guid.NewGuid();
            var revision = new Revision();
            var context = CreateContext();

            _validationServiceMock.Setup(v => v.ValidateNotNull(revision, "revision")).Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId)).ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId)).ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateRevisionExistsAsync(revisionId)).ReturnsAsync((ActionResult?)null);

            _mapperMock.Setup(m => m.Map<Revision>(revision)).Returns(revision);

            var repo = new Mock<AdmsRepository>(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object)
            { CallBase = true };

            repo.Setup(r => r.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            repo.Setup(r => r.GetRevisionActivityByActivityNameAsync("SAVED")).ReturnsAsync(new RevisionActivity
            {
                Activity = "SAVED",
                Id = Guid.NewGuid()
            });

            // Act
            var result = await repo.Object.UpdateRevisionAsync(matterId, documentId, revisionId, revision);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User or RevisionActivity not found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Test: Returns null and logs if the revision activity is not found.
        /// </summary>
        [Fact]
        public async Task ReturnsNull_WhenRevisionActivityNotFound()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var revisionId = Guid.NewGuid();
            var revision = new Revision();
            var context = CreateContext();
            var user = new User
            {
                Name = "testuser",
                Id = Guid.NewGuid()
            };

            _validationServiceMock.Setup(v => v.ValidateNotNull(revision, "revision")).Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId)).ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId)).ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateRevisionExistsAsync(revisionId)).ReturnsAsync((ActionResult?)null);

            _mapperMock.Setup(m => m.Map<Revision>(revision)).Returns(revision);

            var repo = new Mock<AdmsRepository>(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object)
            { CallBase = true };

            context.Users.Add(user);

            await context.SaveChangesAsync();

            repo.Setup(r => r.GetUserByUsernameAsync(user.Name)).ReturnsAsync(user);
            repo.Setup(r => r.GetRevisionActivityByActivityNameAsync("SAVED")).ReturnsAsync((RevisionActivity?)null);

            // Act
            var result = await repo.Object.UpdateRevisionAsync(matterId, documentId, revisionId, revision);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User or RevisionActivity not found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Test: Returns updated revision when all validations pass and save succeeds.
        /// </summary>
        [Fact]
        public async Task ReturnsUpdatedRevision_WhenAllValidationsPassAndSaveSucceeds()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var revisionId = Guid.NewGuid();
            var revision = new Revision();
            var updatedRevision = new Revision();
            var user = new User
            {
                Name = "testuser",
                Id = Guid.NewGuid()
            };
            var revisionActivity = new RevisionActivity()
            {
                Activity = "SAVED",
                Id = Guid.NewGuid()
            };
            var context = CreateContext();

            _validationServiceMock.Setup(v => v.ValidateNotNull(revision, "revision")).Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId)).ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId)).ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateRevisionExistsAsync(revisionId)).ReturnsAsync((ActionResult?)null);

            _mapperMock.Setup(m => m.Map<Revision>(revision)).Returns(updatedRevision);

            var repo = new Mock<AdmsRepository>(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object)
            { CallBase = true };

            context.Users.Add(user);
            context.RevisionActivities.Add(revisionActivity);

            await context.SaveChangesAsync();

            repo.Setup(r => r.GetUserByUsernameAsync(user.Name)).ReturnsAsync(user);
            repo.Setup(r => r.GetRevisionActivityByActivityNameAsync("SAVED")).ReturnsAsync(revisionActivity);

            repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            // Act
            var result = await repo.Object.UpdateRevisionAsync(matterId, documentId, revisionId, revision);

            // Assert
            Assert.Equal(updatedRevision, result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Revision with ID")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Test: Returns null and logs if SaveChangesAsync returns false.
        /// </summary>
        [Fact]
        public async Task ReturnsNull_WhenSaveChangesFails()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var revisionId = Guid.NewGuid();
            var revision = new Revision();
            var updatedRevision = new Revision();
            var user = new User
            {
                Name = "testuser",
                Id = Guid.NewGuid()
            };
            var revisionActivity = new RevisionActivity()
            {
                Activity = "SAVED",
                Id = Guid.NewGuid()
            };
            var context = CreateContext();

            _validationServiceMock.Setup(v => v.ValidateNotNull(revision, "revision")).Returns((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId)).ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateDocumentExistsAsync(documentId)).ReturnsAsync((ActionResult?)null);
            _validationServiceMock.Setup(v => v.ValidateRevisionExistsAsync(revisionId)).ReturnsAsync((ActionResult?)null);

            _mapperMock.Setup(m => m.Map<Revision>(revision)).Returns(updatedRevision);

            var repo = new Mock<AdmsRepository>(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object)
            { CallBase = true };

            repo.Setup(r => r.GetUserByUsernameAsync(user.Name)).ReturnsAsync(user);
            repo.Setup(r => r.GetRevisionActivityByActivityNameAsync("SAVED")).ReturnsAsync(revisionActivity);
            repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);

            // Act
            var result = await repo.Object.UpdateRevisionAsync(matterId, documentId, revisionId, revision);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to save changes after updating revision")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
