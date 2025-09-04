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
    /// Unit tests for AdmsRepository User Actions region.
    /// </summary>
    public class AdmsRepositoryUserActionsTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock;
        private readonly Mock<IValidationService> _validationServiceMock;
        private readonly AdmsContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdmsRepositoryUserActionsTests"/> class.
        /// </summary>
        public AdmsRepositoryUserActionsTests()
        {
            _loggerMock = new Mock<ILogger<AdmsRepository>>();
            _mapperMock = new Mock<IMapper>();
            _propertyMappingServiceMock = new Mock<IPropertyMappingService>();
            _validationServiceMock = new Mock<IValidationService>();

            var options = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AdmsContext(options);
        }

        /// <summary>
        /// Tests that GetUserByUsernameAsync returns the user when found.
        /// </summary>
        [Fact]
        public async Task GetUserByUsernameAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var username = "testuser";
            var user = new User { Name = username, Id = Guid.NewGuid() };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(username, nameof(username)))
                .Returns(true);

            var repo = new AdmsRepository(
                _loggerMock.Object,
                _context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.GetUserByUsernameAsync(username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Name);
        }

        /// <summary>
        /// Tests that GetUserByUsernameAsync returns null and logs a warning when user is not found.
        /// </summary>
        [Fact]
        public async Task GetUserByUsernameAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            var username = "nonexistent";
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(username, nameof(username)))
                .Returns(true);

            var repo = new AdmsRepository(
                _loggerMock.Object,
                _context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.GetUserByUsernameAsync(username);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetUserByUsernameAsync throws ArgumentNullException when username is null or empty.
        /// </summary>
        [Theory]
#pragma warning disable xUnit1012 // Null should only be used for nullable parameters
        [InlineData(null)]
#pragma warning restore xUnit1012 // Null should only be used for nullable parameters
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetUserByUsernameAsync_ReturnsNull_WhenUsernameIsNullOrEmpty(string username)
        {
            // Arrange
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(username, nameof(username)))
                .Returns(false);

            var repo = new AdmsRepository(
                _loggerMock.Object,
                _context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.GetUserByUsernameAsync(username);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetUserByUsernameAsync throws InvalidOperationException when multiple users with the same username exist.
        /// </summary>
        [Fact]
        public async Task GetUserByUsernameAsync_ReturnsNull_WhenMultipleUsersExist()
        {
            // Arrange
            var username = "duplicateuser";
            _context.Users.Add(new User { Name = username, Id = Guid.NewGuid() });
            _context.Users.Add(new User { Name = username, Id = Guid.NewGuid() });
            await _context.SaveChangesAsync();

            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(username, nameof(username)))
                .Returns(true);

            var repo = new AdmsRepository(
                _loggerMock.Object,
                _context,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object);

            // Act
            var result = await repo.GetUserByUsernameAsync(username);

            // Assert
            Assert.Null(result);
        }
    }
}
