using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.Services;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace ADMS.API.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="AdmsRepository.DeleteRevisionAsync"/>.
    /// </summary>
    public class AdmsRepositoryDeleteRevisionAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();
        private readonly DbContextOptions<AdmsContext> _dbContextOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdmsRepositoryDeleteRevisionAsyncTests"/> class.
        /// </summary>
        public AdmsRepositoryDeleteRevisionAsyncTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(databaseName: $"AdmsTestDb_{Guid.NewGuid()}")
                .Options;
        }

        /// <summary>
        /// Verifies that DeleteRevisionAsync returns false and logs a warning if the revision DTO is null.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsFalse_WhenRevisionDtoIsNull()
        {
            // Arrange
            RevisionDto? revisionDto = null;
            _validationServiceMock
                .Setup(v => v.ValidateNotNull(revisionDto, nameof(revisionDto)))
                .Returns(new Microsoft.AspNetCore.Mvc.BadRequestResult());

            await using var context = new AdmsContext(_dbContextOptions);
            var repo = new AdmsRepository(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.DeleteRevisionAsync(revisionDto!);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("revision must be provided")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that DeleteRevisionAsync returns false and logs a warning if the revision ID is not set.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsFalse_WhenRevisionNumberIsNull()
        {
            // Arrange
            var revisionDto = new RevisionDto { Id = null, RevisionNumber = 1 };
            _validationServiceMock
                .Setup(v => v.ValidateNotNull(revisionDto, nameof(revisionDto)))
                .Returns((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            await using var context = new AdmsContext(_dbContextOptions);
            var repo = new AdmsRepository(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.DeleteRevisionAsync(revisionDto);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Revision ID must be provided")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that DeleteRevisionAsync returns false and logs a warning if the revision does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsFalse_WhenRevisionDoesNotExist()
        {
            // Arrange
            var revisionId = Guid.NewGuid();
            var revisionDto = new RevisionDto { Id = revisionId, RevisionNumber = 1 };
            _validationServiceMock
                .Setup(v => v.ValidateNotNull(revisionDto, nameof(revisionDto)))
                .Returns((Microsoft.AspNetCore.Mvc.ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateRevisionExistsAsync(revisionId))
                .ReturnsAsync(new Microsoft.AspNetCore.Mvc.NotFoundResult());

            await using var context = new AdmsContext(_dbContextOptions);
            var repo = new AdmsRepository(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.DeleteRevisionAsync(revisionDto);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("does not exist")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that DeleteRevisionAsync returns false and logs a warning if the revision is not found in the database.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsFalse_WhenRevisionNotFoundInDatabase()
        {
            // Arrange
            var revisionId = Guid.NewGuid();
            var revisionDto = new RevisionDto { Id = revisionId, RevisionNumber = 1 };
            _validationServiceMock
                .Setup(v => v.ValidateNotNull(revisionDto, nameof(revisionDto)))
                .Returns((Microsoft.AspNetCore.Mvc.ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateRevisionExistsAsync(revisionId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            await using var context = new AdmsContext(_dbContextOptions);
            var repo = new AdmsRepository(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.DeleteRevisionAsync(revisionDto);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Revision not found with ID")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that DeleteRevisionAsync returns false and logs a warning if the user or revision activity is not found.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsFalse_WhenUserOrRevisionActivityNotFound()
        {
            // Arrange
            var revisionId = Guid.NewGuid();
            var revisionDto = new RevisionDto { Id = revisionId, RevisionNumber = 1 };
            _validationServiceMock
                .Setup(v => v.ValidateNotNull(revisionDto, nameof(revisionDto)))
                .Returns((Microsoft.AspNetCore.Mvc.ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateRevisionExistsAsync(revisionId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var revision = new Revision { Id = revisionId, RevisionNumber = 1, IsDeleted = false };
            await using (var context = new AdmsContext(_dbContextOptions))
            {
                context.Revisions.Add(revision);
                await context.SaveChangesAsync();
            }

            // Simulate user or revision activity not found
            await using var context2 = new AdmsContext(_dbContextOptions);
            var repo = new AdmsRepository(
                _loggerMock.Object,
                context2,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.DeleteRevisionAsync(revisionDto);

            // Assert
            Assert.False(result);
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
        /// Verifies that DeleteRevisionAsync returns true and logs information if the revision is successfully deleted.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsTrue_WhenRevisionDeletedSuccessfully()
        {
            // Arrange
            var revisionId = Guid.NewGuid();
            var revisionDto = new RevisionDto { Id = revisionId, RevisionNumber = 1 };
            _validationServiceMock
                .Setup(v => v.ValidateNotNull(revisionDto, nameof(revisionDto)))
                .Returns((Microsoft.AspNetCore.Mvc.ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateRevisionExistsAsync(revisionId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var revision = new Revision { Id = revisionId, RevisionNumber = 1, IsDeleted = false };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "testuser",
            };
            var revisionActivity = new RevisionActivity
            {
                Id = Guid.NewGuid(),
                Activity = "DELETED",
            };

            await using (var context = new AdmsContext(_dbContextOptions))
            {
                context.Revisions.Add(revision);
                context.Users.Add(user);
                context.RevisionActivities.Add(revisionActivity);
                await context.SaveChangesAsync();
            }

            // Mock GetUserAsync and GetRevisionActivityByActivityNameAsync
            var repo = new AdmsRepository(
                _loggerMock.Object,
                new AdmsContext(_dbContextOptions),
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Use reflection or internal access if needed to mock private methods, or refactor for testability.
            // For this example, assume the repo will find the user and activity in the context.

            // Act
            var result = await repo.DeleteRevisionAsync(revisionDto);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully deleted revision")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that DeleteRevisionAsync returns false and logs an error if an exception is thrown.
        /// </summary>
        [Fact]
        public async Task DeleteRevisionAsync_ReturnsFalse_AndLogsError_OnException()
        {
            // Arrange
            var revisionId = Guid.NewGuid();
            var revisionDto = new RevisionDto { Id = revisionId, RevisionNumber = 1 };
            _validationServiceMock
                .Setup(v => v.ValidateNotNull(revisionDto, nameof(revisionDto)))
                .Returns((Microsoft.AspNetCore.Mvc.ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateRevisionExistsAsync(revisionId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var contextMock = new Mock<AdmsContext>(_dbContextOptions);
            contextMock.Setup(c => c.Revisions)
                .Throws(new Exception("Unexpected"));

            var repo = new AdmsRepository(
                _loggerMock.Object,
                contextMock.Object,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.DeleteRevisionAsync(revisionDto);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error deleting revision")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
