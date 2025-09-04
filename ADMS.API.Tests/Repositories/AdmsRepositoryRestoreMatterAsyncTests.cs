using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Services;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;
using Moq.Protected;

namespace ADMS.API.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="AdmsRepository.RestoreMatterAsync"/>.
    /// </summary>
    public class AdmsRepositoryRestoreMatterAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        /// <summary>
        /// Creates an in-memory context for testing.
        /// </summary>
        private static AdmsContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AdmsContext(options);
        }

        /// <summary>
        /// Test: Returns false if matterId is empty.
        /// </summary>
        [Fact]
        public async Task RestoreMatterAsync_ReturnsFalse_WhenMatterIdIsEmpty()
        {
            // Arrange
            var context = CreateContext(nameof(RestoreMatterAsync_ReturnsFalse_WhenMatterIdIsEmpty));
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.RestoreMatterAsync(Guid.Empty);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Test: Returns false if validation service says matter does not exist.
        /// </summary>
        [Fact]
        public async Task RestoreMatterAsync_ReturnsFalse_WhenMatterDoesNotExist()
        {
            // Arrange
            var context = CreateContext(nameof(RestoreMatterAsync_ReturnsFalse_WhenMatterDoesNotExist));
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Microsoft.AspNetCore.Mvc.NotFoundResult());
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.RestoreMatterAsync(Guid.NewGuid());

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Test: Returns false if matter is not found in the database after validation passes.
        /// </summary>
        [Fact]
        public async Task RestoreMatterAsync_ReturnsFalse_WhenMatterNotInDatabase()
        {
            // Arrange
            var context = CreateContext(nameof(RestoreMatterAsync_ReturnsFalse_WhenMatterNotInDatabase));
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.RestoreMatterAsync(Guid.NewGuid());

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Test: Returns false if user is not found.
        /// </summary>
        [Fact]
        public async Task RestoreMatterAsync_ReturnsFalse_WhenUserNotFound()
        {
            // Arrange
            var context = CreateContext(nameof(RestoreMatterAsync_ReturnsFalse_WhenUserNotFound));
            var matterId = Guid.NewGuid();
            context.Matters.Add(new Matter 
            { 
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow,
                IsDeleted = true 
            });
            await context.SaveChangesAsync();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Simulate GetUserAsync returns null
            var repoMock = new Mock<AdmsRepository>(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object) { CallBase = true };
            repoMock.Protected().Setup<Task<User?>>("GetUserAsync", ItExpr.IsAny<string>()).ReturnsAsync((User?)null);

            // Act
            var result = await repoMock.Object.RestoreMatterAsync(matterId);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Test: Returns false if matter activity is not found.
        /// </summary>
        [Fact]
        public async Task RestoreMatterAsync_ReturnsFalse_WhenMatterActivityNotFound()
        {
            // Arrange
            var context = CreateContext(nameof(RestoreMatterAsync_ReturnsFalse_WhenMatterActivityNotFound));
            var matterId = Guid.NewGuid();
            context.Matters.Add(new Matter 
            { 
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow,
                IsDeleted = true 
            });
            await context.SaveChangesAsync();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var user = new User 
            { 
                Id = Guid.NewGuid(),
                Name = "Test User"
            };
            var repoMock = new Mock<AdmsRepository>(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object) { CallBase = true };
            repoMock.Protected().Setup<Task<User?>>("GetUserAsync", ItExpr.IsAny<string>()).ReturnsAsync(user);

            // No "RESTORED" activity in context
            var repo = repoMock.Object;

            // Act
            var result = await repo.RestoreMatterAsync(matterId);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Test: Returns false if SaveChangesAsync fails.
        /// </summary>
        [Fact]
        public async Task RestoreMatterAsync_ReturnsFalse_WhenSaveChangesFails()
        {
            // Arrange
            var context = CreateContext(nameof(RestoreMatterAsync_ReturnsFalse_WhenSaveChangesFails));
            var matterId = Guid.NewGuid();
            context.Matters.Add(new Matter 
            { 
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow,
                IsDeleted = true 
            });
            context.MatterActivities.Add(new MatterActivity { Id = Guid.NewGuid(), Activity = "RESTORED" });
            await context.SaveChangesAsync();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var user = new User 
            { 
                Id = Guid.NewGuid(),
                Name = "Test User"
            };
            var repoMock = new Mock<AdmsRepository>(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object) { CallBase = true };
            repoMock.Protected().Setup<Task<User?>>("GetUserAsync", ItExpr.IsAny<string>()).ReturnsAsync(user);
            repoMock.Protected().Setup<Task>("AddAuditLogAsync", ItExpr.IsAny<Matter>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<MatterActivity>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<User>(), ItExpr.IsAny<Guid>()).Returns(Task.CompletedTask);
            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);

            // Act
            var result = await repoMock.Object.RestoreMatterAsync(matterId);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Test: Returns true when matter is restored successfully.
        /// </summary>
        [Fact]
        public async Task RestoreMatterAsync_ReturnsTrue_WhenRestorationSucceeds()
        {
            // Arrange
            var context = CreateContext(nameof(RestoreMatterAsync_ReturnsTrue_WhenRestorationSucceeds));
            var matterId = Guid.NewGuid();
            context.Matters.Add(new Matter 
            { 
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow,
                IsDeleted = true 
            });
            context.MatterActivities.Add(new MatterActivity { Id = Guid.NewGuid(), Activity = "RESTORED" });
            await context.SaveChangesAsync();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var user = new User 
            { 
                Id = Guid.NewGuid(),
                Name = "Test User"
            };
            var repoMock = new Mock<AdmsRepository>(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object) { CallBase = true };
            repoMock.Protected().Setup<Task<User?>>("GetUserAsync", ItExpr.IsAny<string>()).ReturnsAsync(user);
            repoMock.Protected().Setup<Task>("AddAuditLogAsync", ItExpr.IsAny<Matter>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<MatterActivity>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<User>(), ItExpr.IsAny<Guid>()).Returns(Task.CompletedTask);
            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            // Act
            var result = await repoMock.Object.RestoreMatterAsync(matterId);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Test: Returns false if an exception is thrown during the process.
        /// </summary>
        [Fact]
        public async Task RestoreMatterAsync_ReturnsFalse_OnException()
        {
            // Arrange
            var context = CreateContext(nameof(RestoreMatterAsync_ReturnsFalse_OnException));
            var matterId = Guid.NewGuid();
            context.Matters.Add(new Matter 
            { 
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow,
                IsDeleted = true 
            });
            context.MatterActivities.Add(new MatterActivity { Id = Guid.NewGuid(), Activity = "RESTORED" });
            await context.SaveChangesAsync();

            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var user = new User 
            { 
                Id = Guid.NewGuid(),
                Name = "Test User"
            };
            var repoMock = new Mock<AdmsRepository>(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object) { CallBase = true };
            repoMock.Protected().Setup<Task<User?>>("GetUserAsync", ItExpr.IsAny<string>()).ReturnsAsync(user);
            repoMock.Protected().Setup<Task>("AddAuditLogAsync", ItExpr.IsAny<Matter>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<MatterActivity>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<User>(), ItExpr.IsAny<Guid>()).Throws(new Exception("Test exception"));

            // Act
            var result = await repoMock.Object.RestoreMatterAsync(matterId);

            // Assert
            Assert.False(result);
        }
    }
}
