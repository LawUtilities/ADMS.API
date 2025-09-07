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
    /// Unit tests for <see cref="AdmsRepository.RevisionExistsAsync"/>.
    /// </summary>
    public class AdmsRepositoryRevisionExistsAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();
        private readonly DbContextOptions<AdmsContext> _dbContextOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdmsRepositoryRevisionExistsAsyncTests"/> class.
        /// </summary>
        public AdmsRepositoryRevisionExistsAsyncTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(databaseName: $"AdmsTestDb_{Guid.NewGuid()}")
                .Options;
        }

        /// <summary>
        /// Verifies that RevisionExistsAsync returns false and logs a warning if the revisionId is invalid.
        /// </summary>
        [Fact]
        public async Task RevisionExistsAsync_ReturnsFalse_WhenRevisionNumberIsInvalid()
        {
            // Arrange
            var invalidRevisionNumber = Guid.Empty;
            _validationServiceMock
                .Setup(v => v.ValidateGuid(invalidRevisionNumber, nameof(invalidRevisionNumber)))
                .Returns(new Microsoft.AspNetCore.Mvc.BadRequestResult());

            await using var context = new AdmsContext(_dbContextOptions);
            var repo = new AdmsRepository(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.RevisionExistsAsync(invalidRevisionNumber);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid revisionId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that RevisionExistsAsync returns false and logs a warning if the revision does not exist.
        /// </summary>
        [Fact]
        public async Task RevisionExistsAsync_ReturnsFalse_WhenRevisionDoesNotExist()
        {
            // Arrange
            var revisionId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateGuid(revisionId, nameof(revisionId)))
                .Returns((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            await using var context = new AdmsContext(_dbContextOptions);
            var repo = new AdmsRepository(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.RevisionExistsAsync(revisionId);

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
        /// Verifies that RevisionExistsAsync returns true and logs information if the revision exists.
        /// </summary>
        [Fact]
        public async Task RevisionExistsAsync_ReturnsTrue_WhenRevisionExists()
        {
            // Arrange
            var revisionId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateGuid(revisionId, nameof(revisionId)))
                .Returns((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var revision = new Revision { Id = revisionId, RevisionNumber = 1, DocumentId = Guid.NewGuid() };
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
            var result = await repo.RevisionExistsAsync(revisionId);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("exists in the database")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that RevisionExistsAsync returns false and logs an error if an exception is thrown.
        /// </summary>
        [Fact]
        public async Task RevisionExistsAsync_ReturnsFalse_AndLogsError_OnException()
        {
            // Arrange
            var revisionId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateGuid(revisionId, nameof(revisionId)))
                .Returns((Microsoft.AspNetCore.Mvc.ActionResult?)null);

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
            var result = await repo.RevisionExistsAsync(revisionId);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error checking if revision exists")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
