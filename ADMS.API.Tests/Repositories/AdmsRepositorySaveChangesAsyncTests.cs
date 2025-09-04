using ADMS.API.DbContexts;
using ADMS.API.Services;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

using Moq;

namespace ADMS.API.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="AdmsRepository.SaveChangesAsync"/>.
    /// </summary>
    public class AdmsRepositorySaveChangesAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        /// <summary>
        /// Helper to create a mock context with a mock ChangeTracker.
        /// </summary>
        private static Mock<AdmsContext> CreateMockContext(bool hasChanges = true)
        {
            var options = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var contextMock = new Mock<AdmsContext>(options) { CallBase = true };

            var changeTrackerMock = new Mock<ChangeTracker>();
            changeTrackerMock.Setup(c => c.HasChanges()).Returns(hasChanges);

            contextMock.Setup(c => c.ChangeTracker).Returns(changeTrackerMock.Object);

            return contextMock;
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
        /// Test: Returns true and logs info if there are no changes to save.
        /// </summary>
        [Fact]
        public async Task ReturnsTrue_WhenNoChangesToSave()
        {
            // Arrange
            var contextMock = CreateMockContext(hasChanges: false);
            var repo = CreateRepository(contextMock.Object);

            // Act
            var result = await repo.SaveChangesAsync();

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No changes detected in the context")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Test: Returns true and logs info if SaveChangesAsync returns >= 0.
        /// </summary>
        [Fact]
        public async Task ReturnsTrue_WhenSaveChangesSucceeds()
        {
            // Arrange
            var contextMock = CreateMockContext(hasChanges: true);
            contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var repo = CreateRepository(contextMock.Object);

            // Act
            var result = await repo.SaveChangesAsync();

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Database changes saved successfully")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Test: Returns true if SaveChangesAsync returns 0 (no rows affected but still a valid save).
        /// </summary>
        [Fact]
        public async Task ReturnsTrue_WhenSaveChangesReturnsZero()
        {
            // Arrange
            var contextMock = CreateMockContext(hasChanges: true);
            contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
            var repo = CreateRepository(contextMock.Object);

            // Act
            var result = await repo.SaveChangesAsync();

            // Assert
            Assert.True(result);
            contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Test: Returns false and logs error if SaveChangesAsync throws DbUpdateException.
        /// </summary>
        [Fact]
        public async Task ReturnsFalse_WhenDbUpdateExceptionThrown()
        {
            // Arrange
            var contextMock = CreateMockContext(hasChanges: true);
            contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("DB error"));
            var repo = CreateRepository(contextMock.Object);

            // Act
            var result = await repo.SaveChangesAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("A database update error occurred")),
                    It.IsAny<DbUpdateException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Test: Returns false and logs error if SaveChangesAsync throws an unexpected exception.
        /// </summary>
        [Fact]
        public async Task ReturnsFalse_WhenUnexpectedExceptionThrown()
        {
            // Arrange
            var contextMock = CreateMockContext(hasChanges: true);
            contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Unexpected error"));
            var repo = CreateRepository(contextMock.Object);

            // Act
            var result = await repo.SaveChangesAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An unexpected error occurred while processing the request")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
