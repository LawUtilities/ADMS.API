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
    /// Unit tests for <see cref="AdmsRepository.MatterNameExistsAsync(string)"/>.
    /// </summary>
    public class AdmsRepositoryMatterNameExistsAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        /// <summary>
        /// Creates an in-memory <see cref="AdmsContext"/> for testing.
        /// </summary>
        private static AdmsContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AdmsContext(options);
        }

        /// <summary>
        /// Verifies that a BadRequestObjectResult is returned when the matter name is null.
        /// </summary>
        [Fact]
        public async Task MatterNameExistsAsync_ReturnsBadRequest_WhenNameIsNull()
        {
            // Arrange
            var context = CreateInMemoryContext();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            _validationServiceMock.Setup(v => v.ValidateStringNotEmpty(null, "matterName")).Returns(false);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            var repo = CreateRepository(context);

            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var result = await repo.MatterNameExistsAsync(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("cannot be null or empty", badRequest?.Value!.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that a BadRequestObjectResult is returned when the matter name is empty.
        /// </summary>
        [Fact]
        public async Task MatterNameExistsAsync_ReturnsBadRequest_WhenNameIsEmpty()
        {
            // Arrange
            var context = CreateInMemoryContext();
            _validationServiceMock.Setup(v => v.ValidateStringNotEmpty("", "matterName")).Returns(false);
            var repo = CreateRepository(context);

            // Act
            var result = await repo.MatterNameExistsAsync("");

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("cannot be null or empty", badRequest?.Value!.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that OkObjectResult is returned with true when the matter name exists.
        /// </summary>
        [Fact]
        public async Task MatterNameExistsAsync_ReturnsOkTrue_WhenNameExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var matter = new Matter 
            { 
                Id = Guid.NewGuid(), 
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow,
            };
            context.Matters.Add(matter);
            await context.SaveChangesAsync();
            _validationServiceMock.Setup(v => v.ValidateStringNotEmpty("Test Matter", "matterName")).Returns(true);
            var repo = CreateRepository(context);

            // Act
            var result = await repo.MatterNameExistsAsync("Test Matter");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True(okResult.Value is bool b && b);
        }

        /// <summary>
        /// Verifies that OkObjectResult is returned with false when the matter name does not exist.
        /// </summary>
        [Fact]
        public async Task MatterNameExistsAsync_ReturnsOkFalse_WhenNameDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            _validationServiceMock.Setup(v => v.ValidateStringNotEmpty("Nonexistent", "matterName")).Returns(true);
            var repo = CreateRepository(context);

            // Act
            var result = await repo.MatterNameExistsAsync("Nonexistent");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.False(okResult.Value is bool b && b);
        }

        /// <summary>
        /// Verifies that a 500 ObjectResult is returned when an exception occurs.
        /// </summary>
        [Fact]
        public async Task MatterNameExistsAsync_Returns500_WhenExceptionIsThrown()
        {
            // Arrange
            var contextMock = new Mock<AdmsContext>(new DbContextOptionsBuilder<AdmsContext>().Options);
            contextMock.Setup(c => c.Matters).Throws(new Exception("DB error"));
            _validationServiceMock.Setup(v => v.ValidateStringNotEmpty("Test", "matterName")).Returns(true);
            var repo = new AdmsRepository(
                _loggerMock.Object,
                contextMock.Object,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object
            );

            // Act
            var result = await repo.MatterNameExistsAsync("Test");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        /// <summary>
        /// Helper to create a repository with the provided context and mocked dependencies.
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
    }
}
