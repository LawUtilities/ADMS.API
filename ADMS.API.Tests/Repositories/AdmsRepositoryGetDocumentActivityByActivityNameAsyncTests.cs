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
    /// Unit tests for <see cref="AdmsRepository.GetDocumentActivityByActivityNameAsync"/>.
    /// </summary>
    public class AdmsRepositoryGetDocumentActivityByActivityNameAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock;
        private readonly Mock<IValidationService> _validationServiceMock;
        private readonly DbContextOptions<AdmsContext> _dbContextOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdmsRepositoryGetDocumentActivityByActivityNameAsyncTests"/> class.
        /// </summary>
        public AdmsRepositoryGetDocumentActivityByActivityNameAsyncTests()
        {
            _loggerMock = new Mock<ILogger<AdmsRepository>>();
            _mapperMock = new Mock<IMapper>();
            _propertyMappingServiceMock = new Mock<IPropertyMappingService>();
            _validationServiceMock = new Mock<IValidationService>();
            _dbContextOptions = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        /// <summary>
        /// Tests that the method returns null and logs a warning when activityName is null.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityByActivityNameAsync_ReturnsNull_WhenActivityNameIsNull()
        {
            // Arrange
            string? activityName = null;
            _validationServiceMock
#pragma warning disable CS8604 // Possible null reference argument.
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
#pragma warning restore CS8604 // Possible null reference argument.
                .Returns(false);

            await using var context = new AdmsContext(_dbContextOptions);
            var repo = new AdmsRepository(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
#pragma warning disable CS8604 // Possible null reference argument.
            var result = await repo.GetDocumentActivityByActivityNameAsync(activityName);
#pragma warning restore CS8604 // Possible null reference argument.

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Activity name cannot be null or empty")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that the method returns null and logs a warning when activityName is empty.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityByActivityNameAsync_ReturnsNull_WhenActivityNameIsEmpty()
        {
            // Arrange
            string activityName = "";
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
                .Returns(false);

            await using var context = new AdmsContext(_dbContextOptions);
            var repo = new AdmsRepository(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.GetDocumentActivityByActivityNameAsync(activityName);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Activity name cannot be null or empty")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that the method returns null and logs a warning when activityName is whitespace.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityByActivityNameAsync_ReturnsNull_WhenActivityNameIsWhitespace()
        {
            // Arrange
            string activityName = "   ";
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
                .Returns(false);

            await using var context = new AdmsContext(_dbContextOptions);
            var repo = new AdmsRepository(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.GetDocumentActivityByActivityNameAsync(activityName);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Activity name cannot be null or empty")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that the method returns null and logs a warning when the activity is not found.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityByActivityNameAsync_ReturnsNull_WhenActivityNotFound()
        {
            // Arrange
            const string activityName = "NonExistentActivity";
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
                .Returns(true);

            await using var context = new AdmsContext(_dbContextOptions);
            var repo = new AdmsRepository(
                _loggerMock.Object,
                context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.GetDocumentActivityByActivityNameAsync(activityName);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No DocumentActivity found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that the method returns the correct activity and logs information when found.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityByActivityNameAsync_ReturnsActivity_WhenFound()
        {
            // Arrange
            const string activityName = "CREATE";
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
                .Returns(true);

            var activity = new DocumentActivity
            {
                Id = Guid.NewGuid(),
                Activity = activityName
            };
            await using (var context = new AdmsContext(_dbContextOptions))
            {
                context.DocumentActivities.Add(activity);
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
            var result = await repo.GetDocumentActivityByActivityNameAsync(activityName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(activityName, result.Activity);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("retrieved successfully")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that the method returns null and logs an error when multiple activities with the same name exist.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityByActivityNameAsync_ReturnsNull_WhenMultipleActivitiesExist()
        {
            // Arrange
            const string activityName = "DUPLICATE";
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
                .Returns(true);

            await using (var context = new AdmsContext(_dbContextOptions))
            {
                context.DocumentActivities.Add(new DocumentActivity
                {
                    Id = Guid.NewGuid(),
                    Activity = activityName
                });
                context.DocumentActivities.Add(new DocumentActivity
                {
                    Id = Guid.NewGuid(),
                    Activity = activityName
                });
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
            var result = await repo.GetDocumentActivityByActivityNameAsync(activityName);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Multiple DocumentActivity records found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that the method returns null and logs an error when an unexpected exception occurs.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityByActivityNameAsync_ReturnsNull_OnUnexpectedException()
        {
            // Arrange
            const string activityName = "ANY";
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
                .Returns(true);

            var contextMock = new Mock<AdmsContext>(_dbContextOptions);
            contextMock.Setup(c => c.DocumentActivities)
                .Throws(new Exception("Unexpected"));

            var repo = new AdmsRepository(
                _loggerMock.Object,
                contextMock.Object,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.GetDocumentActivityByActivityNameAsync(activityName);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An unexpected error occurred")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
