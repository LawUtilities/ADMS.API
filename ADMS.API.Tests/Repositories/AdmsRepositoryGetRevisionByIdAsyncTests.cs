using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Services;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace ADMS.API.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="AdmsRepository.GetRevisionByIdAsync"/>.
    /// </summary>
    public class AdmsRepositoryGetRevisionByIdAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();
        private readonly DbContextOptions<AdmsContext> _dbContextOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdmsRepositoryGetRevisionByIdAsyncTests"/> class.
        /// </summary>
        public AdmsRepositoryGetRevisionByIdAsyncTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(databaseName: $"AdmsTestDb_{Guid.NewGuid()}")
                .Options;
        }

        /// <summary>
        /// Verifies that GetRevisionByIdAsync returns null and logs a warning if the revision does not exist (validation fails).
        /// </summary>
        [Fact]
        public async Task GetRevisionByIdAsync_ReturnsNull_WhenValidationFails()
        {
            // Arrange
            var revisionId = Guid.NewGuid();
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
            var result = await repo.GetRevisionByIdAsync(revisionId);

            // Assert
            Assert.Null(result);
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
        /// Verifies that GetRevisionByIdAsync returns the revision when it exists and includeHistory is false.
        /// </summary>
        [Fact]
        public async Task GetRevisionByIdAsync_ReturnsRevision_WhenExists_AndIncludeHistoryFalse()
        {
            // Arrange
            var revisionId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateRevisionExistsAsync(revisionId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var revision = new Revision { Id = revisionId, DocumentId = Guid.NewGuid() };
            await using (var context = new AdmsContext(_dbContextOptions))
            {
                context.Revisions.Add(revision);
                await context.SaveChangesAsync();
            }

            await using var context2 = new AdmsContext(_dbContextOptions);
            var repo = new AdmsRepository(
                _loggerMock.Object,
                context2,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.GetRevisionByIdAsync(revisionId, includeHistory: false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(revisionId, result.Id);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved revision")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that GetRevisionByIdAsync returns the revision and includes navigation properties when includeHistory is true.
        /// </summary>
        [Fact]
        public async Task GetRevisionByIdAsync_ReturnsRevisionWithHistory_WhenIncludeHistoryTrue()
        {
            // Arrange
            var revisionId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateRevisionExistsAsync(revisionId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var matter = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Description",
                CreationDate = DateTime.UtcNow
            };

            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = "Test Document",
                Matter = matter,
                MatterId = matter.Id,
                Extension = ".txt"
            };

            var revision = new Revision
            {
                Id = revisionId, 
                DocumentId = document.Id,
                CreationDate = DateTime.UtcNow,
                ModificationDate = DateTime.UtcNow,
                Document = document,
            };
            
            var activity = new RevisionActivity
            {
                Id = Guid.NewGuid(),
                Activity = "CREATED",
            };
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "testuser",
            };

            var activityUser = new RevisionActivityUser
            {
                RevisionId = revisionId,
                RevisionActivityId = activity.Id,
                UserId = user.Id,
                User = user,
                RevisionActivity = activity,
                Revision = revision,
            };

            await using (var context = new AdmsContext(_dbContextOptions))
            {
                context.Matters.Add(matter);
                context.Documents.Add(document);
                context.Revisions.Add(revision);
                context.RevisionActivities.Add(activity);
                context.Users.Add(user);
                context.RevisionActivityUsers.Add(activityUser);
                await context.SaveChangesAsync();
            }

            await using var context2 = new AdmsContext(_dbContextOptions);
            var repo = new AdmsRepository(
                _loggerMock.Object,
                context2,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.GetRevisionByIdAsync(revisionId, includeHistory: true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(revisionId, result.Id);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved revision")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that GetRevisionByIdAsync returns null and logs a warning if the revision is not found in the database.
        /// </summary>
        [Fact]
        public async Task GetRevisionByIdAsync_ReturnsNull_WhenRevisionNotFoundInDatabase()
        {
            // Arrange
            var revisionId = Guid.NewGuid();
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
            var result = await repo.GetRevisionByIdAsync(revisionId);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No revision found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that GetRevisionByIdAsync returns null and logs an error if an exception is thrown.
        /// </summary>
        [Fact]
        public async Task GetRevisionByIdAsync_ReturnsNull_AndLogsError_OnException()
        {
            // Arrange
            var revisionId = Guid.NewGuid();
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
            var result = await repo.GetRevisionByIdAsync(revisionId);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error retrieving revision")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
