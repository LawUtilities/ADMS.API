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
    /// Unit tests for <see cref="AdmsRepository.GetRevisionActivityByActivityNameAsync"/>.
    /// </summary>
    public class AdmsRepositoryRevisionActivityTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();
        private readonly DbContextOptions<AdmsContext> _dbContextOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdmsRepositoryRevisionActivityTests"/> class.
        /// </summary>
        public AdmsRepositoryRevisionActivityTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(databaseName: $"AdmsTestDb_{Guid.NewGuid()}")
                .Options;
        }

        /// <summary>
        /// Verifies that null is returned and a warning is logged when the activity name is null or empty.
        /// </summary>
        [Theory]
#pragma warning disable xUnit1012 // Null should only be used for nullable parameters
        [InlineData(null)]
#pragma warning restore xUnit1012 // Null should only be used for nullable parameters
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetRevisionActivityByActivityNameAsync_ReturnsNull_WhenActivityNameIsNullOrEmpty(string activityName)
        {
            // Arrange
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
            var result = await repo.GetRevisionActivityByActivityNameAsync(activityName);

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
        /// Verifies that null is returned and a warning is logged when the activity is not found.
        /// </summary>
        [Fact]
        public async Task GetRevisionActivityByActivityNameAsync_ReturnsNull_WhenActivityNotFound()
        {
            // Arrange
            const string activityName = "NonExistent";
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
            var result = await repo.GetRevisionActivityByActivityNameAsync(activityName);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No RevisionActivity found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that the correct RevisionActivity is returned and information is logged when found.
        /// </summary>
        [Fact]
        public async Task GetRevisionActivityByActivityNameAsync_ReturnsActivity_WhenFound()
        {
            // Arrange
            const string activityName = "CREATED";
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
                .Returns(true);

            var activity = new RevisionActivity { Activity = activityName };
            await using (var context = new AdmsContext(_dbContextOptions))
            {
                context.RevisionActivities.Add(activity);
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
            var result = await repo.GetRevisionActivityByActivityNameAsync(activityName);

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
        /// Verifies that null is returned and an error is logged when multiple activities with the same name exist.
        /// </summary>
        [Fact]
        public async Task GetRevisionActivityByActivityNameAsync_ReturnsNull_WhenMultipleActivitiesExist()
        {
            // Arrange
            const string activityName = "DUPLICATE";
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
                .Returns(true);

            await using (var context = new AdmsContext(_dbContextOptions))
            {
                context.RevisionActivities.Add(new RevisionActivity { Activity = activityName });
                context.RevisionActivities.Add(new RevisionActivity { Activity = activityName });
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
            var result = await repo.GetRevisionActivityByActivityNameAsync(activityName);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An InvalidOperationException occurred") || v.ToString()!.Contains("Multiple RevisionActivity records found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that null is returned and an error is logged when an unexpected exception occurs.
        /// </summary>
        [Fact]
        public async Task GetRevisionActivityByActivityNameAsync_ReturnsNull_OnUnexpectedException()
        {
            // Arrange
            const string activityName = "ANY";
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
                .Returns(true);

            var contextMock = new Mock<AdmsContext>(_dbContextOptions);
            contextMock.Setup(c => c.RevisionActivities)
                .Throws(new Exception("Unexpected"));

            var repo = new AdmsRepository(
                _loggerMock.Object,
                contextMock.Object,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.GetRevisionActivityByActivityNameAsync(activityName);

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
