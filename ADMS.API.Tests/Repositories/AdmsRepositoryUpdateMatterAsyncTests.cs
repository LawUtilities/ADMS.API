using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.Services;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="AdmsRepository.UpdateMatterAsync"/>.
    /// </summary>
    public class AdmsRepositoryTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<AdmsContext> _contextMock;
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        private readonly AdmsRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdmsRepositoryTests"/> class.
        /// </summary>
        public AdmsRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _contextMock = new Mock<AdmsContext>(options);

            _repository = new AdmsRepository(
                _loggerMock.Object,
                _contextMock.Object,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object
            );
        }

        /// <summary>
        /// Tests that UpdateMatterAsync returns null and logs a warning when the DTO is null.
        /// </summary>
        [Fact]
        public async Task UpdateMatterAsync_NullDto_ReturnsNull()
        {
            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var result = await _repository.UpdateMatterAsync(Guid.NewGuid(), null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("MatterForUpdateDto is null.")),
                    null,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((state, exception) => true)),
                Times.Once);
        }

        /// <summary>
        /// Tests that UpdateMatterAsync returns null and logs a warning when the DTO model is invalid.
        /// </summary>
        [Fact]
        public async Task UpdateMatterAsync_InvalidDtoModel_ReturnsNull()
        {
            // Arrange
            var dto = new MatterForUpdateDto()
            {
                Description = "Invalid Description",
                IsArchived = false,
                CreationDate = DateTime.UtcNow.AddDays(1) // Future date to trigger validation failure
            };
            var validationResults = new List<ValidationResult> { new("Invalid") };
            Mock.Get(typeof(MatterForUpdateDto))
                .Setup(m => MatterForUpdateDto.ValidateModel(dto))
                .Returns(validationResults);

            // Act
            var result = await _repository.UpdateMatterAsync(Guid.NewGuid(), dto);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("MatterForUpdateDto is null.")),
                    null,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((state, exception) => true)),
                Times.Once);
        }

        /// <summary>
        /// Tests that UpdateMatterAsync returns null and logs a warning when the matter does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateMatterAsync_MatterDoesNotExist_ReturnsNull()
        {
            // Arrange
            var dto = new MatterForUpdateDto()
            {
                Description = "Test Matter",
                IsArchived = false,
                CreationDate = DateTime.UtcNow
            };
            Mock.Get(typeof(MatterForUpdateDto))
                .Setup(m => MatterForUpdateDto.ValidateModel(dto))
                .Returns([]);
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Microsoft.AspNetCore.Mvc.NotFoundResult());

            // Act
            var result = await _repository.UpdateMatterAsync(Guid.NewGuid(), dto);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("MatterForUpdateDto is null.")),
                    null,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((state, exception) => true)),
                Times.Once);
        }

        /// <summary>
        /// Tests that UpdateMatterAsync returns null and logs a warning when the matter is not found in the database.
        /// </summary>
        [Fact]
        public async Task UpdateMatterAsync_MatterNotFoundInDb_ReturnsNull()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var dto = new MatterForUpdateDto()
            {
                Description = "Test Matter",
                IsArchived = false,
                CreationDate = DateTime.UtcNow
            };
            Mock.Get(typeof(MatterForUpdateDto))
                .Setup(m => MatterForUpdateDto.ValidateModel(dto))
                .Returns([]);
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);
            _contextMock.Setup(c => c.Matters.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Matter, bool>>>(),
                default)).ReturnsAsync(null as Matter);

            // Act
            var result = await _repository.UpdateMatterAsync(matterId, dto);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("MatterForUpdateDto is null.")),
                    null,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((state, exception) => true)),
                Times.Once);
        }

        /// <summary>
        /// Tests that UpdateMatterAsync returns null and logs a warning when the user or activity is not found.
        /// </summary>
        [Fact]
        public async Task UpdateMatterAsync_UserOrActivityNotFound_ReturnsNull()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var dto = new MatterForUpdateDto()
            {
                Description = "Test Matter",
                IsArchived = false,
                CreationDate = DateTime.UtcNow
            };
            var dbMatter = new Matter 
            { 
                Id = matterId,
                Description = dto.Description,
                CreationDate = dto.CreationDate,
            };
            Mock.Get(typeof(MatterForUpdateDto))
                .Setup(m => MatterForUpdateDto.ValidateModel(dto))
                .Returns([]);
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);
            _contextMock.Setup(c => c.Matters.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Matter, bool>>>(),
                default)).ReturnsAsync(dbMatter);
            // Simulate user or activity not found
            _contextMock.Setup(c => c.MatterActivities.AsNoTracking().FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<MatterActivity, bool>>>(),
                default)).ReturnsAsync(null as MatterActivity);

            // Act
            var result = await _repository.UpdateMatterAsync(matterId, dto);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("MatterForUpdateDto is null.")),
                    null,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((state, exception) => true)),
                Times.Once);
        }

        /// <summary>
        /// Tests that UpdateMatterAsync returns null and logs a warning when SaveChangesAsync fails.
        /// </summary>
        [Fact]
        public async Task UpdateMatterAsync_SaveFails_ReturnsNull()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var dto = new MatterForUpdateDto()
            {
                Description = "Test Matter",
                IsArchived = false,
                CreationDate = DateTime.UtcNow
            };
            var dbMatter = new Matter 
            { 
                Id = matterId,
                Description = dto.Description,
                CreationDate = dto.CreationDate,
            };
            var user = new User 
            { 
                Id = Guid.NewGuid(),
                Name = "Test User"
            };
            var activity = new MatterActivity 
            { 
                Id = Guid.NewGuid(),
                Activity = "Test Activity"
            };
            Mock.Get(typeof(MatterForUpdateDto))
                .Setup(m => MatterForUpdateDto.ValidateModel(dto))
                .Returns([]);
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);
            _contextMock.Setup(c => c.Matters.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Matter, bool>>>(),
                default)).ReturnsAsync(dbMatter);
            _contextMock.Setup(c => c.MatterActivities.AsNoTracking().FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<MatterActivity, bool>>>(),
                default)).ReturnsAsync(activity);
            // Simulate user found
            var repo = new Mock<AdmsRepository>(_loggerMock.Object, _contextMock.Object, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object) { CallBase = true };
            repo.Setup(r => r.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync(user);
            repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);

            // Act
            var result = await repo.Object.UpdateMatterAsync(matterId, dto);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("MatterForUpdateDto is null.")),
                    null,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((state, exception) => true)),
                Times.Once);
        }

        /// <summary>
        /// Tests that UpdateMatterAsync returns the updated matter when all operations succeed.
        /// </summary>
        [Fact]
        public async Task UpdateMatterAsync_ValidInput_ReturnsUpdatedMatter()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var dto = new MatterForUpdateDto()
            {
                Description = "Updated Matter",
                IsArchived = false,
                CreationDate = DateTime.UtcNow
            };
            var dbMatter = new Matter 
            { 
                Id = matterId,
                Description = dto.Description,
                CreationDate = dto.CreationDate,
            };
            var user = new User 
            { 
                Id = Guid.NewGuid(),
                Name = "Test User"
            };
            var activity = new MatterActivity 
            { 
                Id = Guid.NewGuid(),
                Activity = "Test Activity"
            };
            Mock.Get(typeof(MatterForUpdateDto))
                .Setup(m => MatterForUpdateDto.ValidateModel(dto))
                .Returns([]);
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);
            _contextMock.Setup(c => c.Matters.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Matter, bool>>>(),
                default)).ReturnsAsync(dbMatter);
            _contextMock.Setup(c => c.MatterActivities.AsNoTracking().FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<MatterActivity, bool>>>(),
                default)).ReturnsAsync(activity);
            // Simulate user found
            var repo = new Mock<AdmsRepository>(_loggerMock.Object, _contextMock.Object, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object) { CallBase = true };
            repo.Setup(r => r.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync(user);
            repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            // Act
            var result = await repo.Object.UpdateMatterAsync(matterId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dbMatter, result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("MatterForUpdateDto is null.")),
                    null,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((state, exception) => true)),
                Times.Once);
        }
    }
}
