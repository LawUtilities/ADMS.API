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
    /// Unit tests for <see cref="AdmsRepository.MatterExistsAsync(Guid)"/>.
    /// </summary>
    public class AdmsRepositoryMatterExistsAsyncTests
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
        /// Verifies that a BadRequestObjectResult is returned when the matterId is Guid.Empty.
        /// </summary>
        [Fact]
        public async Task MatterExistsAsync_ReturnsBadRequest_WhenMatterIdIsEmpty()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = CreateRepository(context);

            // Act
            var result = await repo.MatterExistsAsync(Guid.Empty);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Invalid matterId", badRequest?.Value!.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that OkObjectResult is returned with true when the matter exists.
        /// </summary>
        [Fact]
        public async Task MatterExistsAsync_ReturnsOkTrue_WhenMatterExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var matter = new Matter 
            { 
                Id = Guid.NewGuid(),
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            context.Matters.Add(matter);
            await context.SaveChangesAsync();
            var repo = CreateRepository(context);

            // Act
            var result = await repo.MatterExistsAsync(matter.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True(okResult.Value is bool b && b);
        }

        /// <summary>
        /// Verifies that OkObjectResult is returned with false when the matter does not exist.
        /// </summary>
        [Fact]
        public async Task MatterExistsAsync_ReturnsOkFalse_WhenMatterDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = CreateRepository(context);

            // Act
            var result = await repo.MatterExistsAsync(Guid.NewGuid());

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.False(okResult.Value is bool b && b);
        }

        /// <summary>
        /// Verifies that a 500 ObjectResult is returned when an exception occurs.
        /// </summary>
        [Fact]
        public async Task MatterExistsAsync_Returns500_WhenExceptionIsThrown()
        {
            // Arrange
            var contextMock = new Mock<AdmsContext>(new DbContextOptionsBuilder<AdmsContext>().Options);
            contextMock.Setup(c => c.Matters).Throws(new Exception("DB error"));
            var repo = new AdmsRepository(
                _loggerMock.Object,
                contextMock.Object,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object
            );

            // Act
            var result = await repo.MatterExistsAsync(Guid.NewGuid());

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
