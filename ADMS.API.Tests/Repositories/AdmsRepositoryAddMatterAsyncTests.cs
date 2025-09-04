using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.Services;

using MapsterMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="AdmsRepository.AddMatterAsync(MatterForCreationDto)"/>.
    /// </summary>
    public class AdmsRepositoryAddMatterAsyncTests
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
        /// Verifies that BadRequest is returned when the input matter is null.
        /// </summary>
        [Fact]
        public async Task AddMatterAsync_ReturnsBadRequest_WhenMatterIsNull()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = CreateRepository(context);

            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var result = await repo.AddMatterAsync(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Verifies that BadRequest is returned when the matter description is null or empty.
        /// </summary>
        [Theory]
#pragma warning disable xUnit1012 // Null should only be used for nullable parameters
        [InlineData(null)]
#pragma warning restore xUnit1012 // Null should only be used for nullable parameters
        [InlineData("")]
        [InlineData("   ")]
        public async Task AddMatterAsync_ReturnsBadRequest_WhenDescriptionIsNullOrEmpty(string description)
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = CreateRepository(context);
            var matter = new MatterForCreationDto() 
            {
                Description = description,
                CreationDate = DateTime.UtcNow,
                IsArchived = false
            };

            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(description, "Description"))
                .Returns(false);

            // Act
            var result = await repo.AddMatterAsync(matter);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Verifies that BadRequest is returned when model validation fails.
        /// </summary>
        [Fact]
        public async Task AddMatterAsync_ReturnsBadRequest_WhenModelValidationFails()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = CreateRepository(context);
            var matter = new MatterForCreationDto() 
            {
                Description = "Valid",
                CreationDate = DateTime.UtcNow,
                IsArchived = false
            };

            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // Simulate validation errors
            var validationResults = new List<ValidationResult>
            {
                new("Description is required.")
            };
            Mock.Get(typeof(MatterDto))
                .Setup(m => MatterForCreationDto.ValidateModel(matter))
                .Returns(validationResults);

            // Act
            var result = await repo.AddMatterAsync(matter);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Verifies that Conflict is returned when a matter with the same description already exists.
        /// </summary>
        [Fact]
        public async Task AddMatterAsync_ReturnsConflict_WhenMatterWithSameDescriptionExists()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = CreateRepository(context);
            var matter = new MatterForCreationDto() 
            {
                Description = "Duplicate",
                CreationDate = DateTime.UtcNow,
                IsArchived = false
            };

            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            Mock.Get(typeof(MatterDto))
                .Setup(m => MatterForCreationDto.ValidateModel(matter))
                .Returns([]);

            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(matter.Description, "Description"))
                .Returns(true);

            repo
                .GetType()
                .GetMethod("MatterNameExistsAsync")
                ?.Invoke(repo, [matter.Description]);

            // Simulate duplicate
            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(matter.Description, "Description"))
                .Returns(true);

            // Act
            var result = await repo.AddMatterAsync(matter);

            // Assert
            Assert.IsType<ConflictObjectResult>(result.Result);
        }

        /// <summary>
        /// Verifies that a 500 ObjectResult is returned when user or activity is not found.
        /// </summary>
        [Fact]
        public async Task AddMatterAsync_Returns500_WhenUserOrActivityNotFound()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = CreateRepository(context);
            var matter = new MatterForCreationDto() 
            {
                Description = "Valid",
                CreationDate = DateTime.UtcNow,
                IsArchived = false
            };

            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            Mock.Get(typeof(MatterDto))
                .Setup(m => MatterForCreationDto.ValidateModel(matter))
                .Returns([]);

            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(matter.Description, "Description"))
                .Returns(true);

            // Simulate user or activity not found
            repo.GetType().GetMethod("GetUserAsync")?.Invoke(repo, ["rbrown"]);
            repo.GetType().GetMethod("GetMatterActivityByActivityNameAsync")?.Invoke(repo, ["CREATED"]);

            // Act
            var result = await repo.AddMatterAsync(matter);

            // Assert
            var objectResult = result.Result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);
        }

        /// <summary>
        /// Verifies that a 500 ObjectResult is returned when SaveChangesAsync fails.
        /// </summary>
        [Fact]
        public async Task AddMatterAsync_Returns500_WhenSaveChangesFails()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = CreateRepository(context);
            var matter = new MatterForCreationDto() 
            {
                Description = "Valid",
                CreationDate = DateTime.UtcNow
            };

            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            Mock.Get(typeof(MatterDto))
                .Setup(m => MatterForCreationDto.ValidateModel(matter))
                .Returns([]);

            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(matter.Description, "Description"))
                .Returns(true);

            // Simulate user and activity found
            repo.GetType().GetMethod("GetUserAsync")?.Invoke(repo, ["rbrown"]);
            repo.GetType().GetMethod("GetMatterActivityByActivityNameAsync")?.Invoke(repo, ["CREATED"]);

            // Simulate SaveChangesAsync returns false
            Mock.Get(repo).Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);

            // Act
            var result = await repo.AddMatterAsync(matter);

            // Assert
            var objectResult = result.Result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);
        }

        /// <summary>
        /// Verifies that OkObjectResult is returned and the matter is created when all is valid.
        /// </summary>
        [Fact]
        public async Task AddMatterAsync_ReturnsOk_WhenMatterIsCreated()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = CreateRepository(context);
            var matter = new MatterForCreationDto() 
            {
                Description = "Valid",
                CreationDate = DateTime.UtcNow,
            };

            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            Mock.Get(typeof(MatterDto))
                .Setup(m => MatterForCreationDto.ValidateModel(matter))
                .Returns([]);

            _validationServiceMock
                .Setup(v => v.ValidateStringNotEmpty(matter.Description, "Description"))
                .Returns(true);

            // Simulate user and activity found
            repo.GetType().GetMethod("GetUserAsync")?.Invoke(repo, ["rbrown"]);
            repo.GetType().GetMethod("GetMatterActivityByActivityNameAsync")?.Invoke(repo, ["CREATED"]);

            // Simulate SaveChangesAsync returns true
            Mock.Get(repo).Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            // Act
            var result = await repo.AddMatterAsync(matter);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.IsType<Matter>(okResult.Value);
        }

        /// <summary>
        /// Creates an instance of <see cref="AdmsRepository"/> with the provided context and mocked dependencies.
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
