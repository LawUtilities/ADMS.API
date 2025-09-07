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
    public class AdmsRepositoryGetRevisionActivityByActivityNameAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        /// <summary>
        /// Helper to create an in-memory context with optional seed data.
        /// </summary>
        private static AdmsContext CreateContext(IEnumerable<RevisionActivity>? seed = null)
        {
            var options = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new AdmsContext(options);
            if (seed != null)
            {
                context.RevisionActivities.AddRange(seed);
                context.SaveChanges();
            }
            return context;
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
        /// Test: Returns null and logs warning if activityName is null or empty.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ReturnsNull_WhenActivityNameIsNullOrEmpty(string? activityName)
        {
            // Arrange
            _validationServiceMock
#pragma warning disable CS8604 // Possible null reference argument.
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
#pragma warning restore CS8604 // Possible null reference argument.
                .Returns(false);

            await using var context = CreateContext();
            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetRevisionActivityByActivityNameAsync(activityName!);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Activity name cannot be null or empty.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Test: Returns null and logs warning if no activity is found.
        /// </summary>
        [Fact]
        public async Task ReturnsNull_WhenActivityNotFound()
        {
            // Arrange
            const string activityName = "NOT_FOUND";
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
                .Returns(true);

            await using var context = CreateContext();
            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetRevisionActivityByActivityNameAsync(activityName);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No RevisionActivity found with activity name")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Test: Returns the activity and logs info if found.
        /// </summary>
        [Fact]
        public async Task ReturnsActivity_WhenFound()
        {
            // Arrange
            const string activityName = "REVIEWED";
            var activity = new RevisionActivity { Activity = activityName };
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
                .Returns(true);

            await using var context = CreateContext([activity]);
            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetRevisionActivityByActivityNameAsync(activityName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(activityName, result.Activity);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("RevisionActivity 'REVIEWED' retrieved successfully.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Test: Returns null and logs error if multiple activities with the same name exist.
        /// </summary>
        [Fact]
        public async Task ReturnsNull_WhenMultipleActivitiesWithSameName()
        {
            // Arrange
            const string activityName = "DUPLICATE";
            var activities = new[]
            {
                new RevisionActivity { Activity = activityName },
                new RevisionActivity { Activity = activityName }
            };
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
                .Returns(true);

            await using var context = CreateContext(activities);
            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetRevisionActivityByActivityNameAsync(activityName);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An InvalidOperationException occurred. Context")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Test: Returns null and logs error if an unexpected exception occurs.
        /// </summary>
        [Fact]
        public async Task ReturnsNull_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            const string activityName = "EXCEPTION";
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(activityName, It.IsAny<string>()))
                .Returns(true);

            // Simulate exception by disposing context before use
            var context = CreateContext();
            await context.DisposeAsync();
            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetRevisionActivityByActivityNameAsync(activityName);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An unexpected error occurred while processing the request.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
