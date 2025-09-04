using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.Services;

using MapsterMapper;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;
using Moq.Protected;

namespace ADMS.API.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="AdmsRepository.DeleteMatterAsync"/>.
    /// </summary>
    public class AdmsRepositoryDeleteMatterAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<AdmsContext> _contextMock;
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AdmsRepositoryDeleteMatterAsyncTests"/> class.
        /// </summary>
        public AdmsRepositoryDeleteMatterAsyncTests()
        {
            var options = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _contextMock = new Mock<AdmsContext>(options) { CallBase = true };
        }

        /// <summary>
        /// Should return false and log a warning when the input MatterDto is null.
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ShouldReturnFalse_WhenMatterToDeleteIsNull()
        {
            // Arrange
            _validationServiceMock
                .Setup(v => v.ValidateNotNull(null, It.IsAny<string>()))
                .Returns(new Microsoft.AspNetCore.Mvc.BadRequestResult());

            var repo = CreateRepository();

            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var result = await repo.DeleteMatterAsync(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Assert
            result.Should().BeFalse();
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Matter to delete is null.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once());
        }

        /// <summary>
        /// Should return false and log a warning when the matter does not exist (validation fails).
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ShouldReturnFalse_WhenMatterDoesNotExist()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var matterDto = new MatterDto
            {
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            _validationServiceMock
                .Setup(v => v.ValidateNotNull(matterDto, It.IsAny<string>()))
                .Returns(null as Microsoft.AspNetCore.Mvc.ActionResult);
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync(new Microsoft.AspNetCore.Mvc.NotFoundResult());

            var repo = CreateRepository();

            // Act
            var result = await repo.DeleteMatterAsync(matterDto);

            // Assert
            result.Should().BeFalse();
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()!.Contains("Matter with ID:") &&
                        v.ToString()!.Contains("does not exist") &&
                        v.ToString()!.Contains(matterId.ToString())),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once());
        }

        /// <summary>
        /// Should return false and log a warning when the matter is not found in the database after validation.
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ShouldReturnFalse_WhenMatterNotFoundInDb()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var matterDto = new MatterDto
            {
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            _validationServiceMock
                .Setup(v => v.ValidateNotNull(matterDto, It.IsAny<string>()))
                .Returns(null as Microsoft.AspNetCore.Mvc.ActionResult);
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync(null as Microsoft.AspNetCore.Mvc.ActionResult);

            _contextMock.Setup(c => c.Matters.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Matter, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(null as Matter);

            var repo = CreateRepository();

            // Act
            var result = await repo.DeleteMatterAsync(matterDto);

            // Assert
            result.Should().BeFalse();
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()!.Contains("Matter not found with ID:") &&
                        v.ToString()!.Contains(matterId.ToString())),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once());
        }

        /// <summary>
        /// Should return false and log a warning when the user or MatterActivity is not found.
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ShouldReturnFalse_WhenUserOrMatterActivityNotFound()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var matterDto = new MatterDto
            {
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var dbMatter = new Matter
            {
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };

            _validationServiceMock
                .Setup(v => v.ValidateNotNull(matterDto, It.IsAny<string>()))
                .Returns(null as Microsoft.AspNetCore.Mvc.ActionResult);
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync(null as Microsoft.AspNetCore.Mvc.ActionResult);

            _contextMock.Setup(c => c.Matters.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Matter, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbMatter);

            var repoMock = CreateRepositoryMock();
            repoMock.Setup(r => r.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync(null as User);
            _contextMock.Setup(c => c.MatterActivities.AsNoTracking().FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<MatterActivity, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(null as MatterActivity);

            // Act
            var result = await repoMock.Object.DeleteMatterAsync(matterDto);

            // Assert
            result.Should().BeFalse();
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()!.Contains("User or MatterActivity not found") &&
                        v.ToString()!.Contains("User:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once());
        }

        /// <summary>
        /// Should return false and log a warning when SaveChangesAsync returns false.
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ShouldReturnFalse_WhenSaveChangesFails()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var matterDto = new MatterDto
            {
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var dbMatter = new Matter
            {
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var user = new User 
            { 
                Id = Guid.NewGuid(),
                Name = "Test User"
            };
            var matterActivity = new MatterActivity 
            { 
                Id = Guid.NewGuid(),
                Activity = "DELETED"
            };

            _validationServiceMock
                .Setup(v => v.ValidateNotNull(matterDto, It.IsAny<string>()))
                .Returns(null as Microsoft.AspNetCore.Mvc.ActionResult);
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync(null as Microsoft.AspNetCore.Mvc.ActionResult);

            _contextMock.Setup(c => c.Matters.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Matter, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbMatter);

            var repoMock = CreateRepositoryMock();
            repoMock.Setup(r => r.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync(user);
            _contextMock.Setup(c => c.MatterActivities.AsNoTracking().FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<MatterActivity, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(matterActivity);

            repoMock.Protected()
                .Setup<Task>("AddAuditLogAsync", dbMatter, dbMatter.Id, matterActivity, matterActivity.Id, user, user.Id)
                .Returns(Task.CompletedTask);

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);

            // Act
            var result = await repoMock.Object.DeleteMatterAsync(matterDto);

            // Assert
            result.Should().BeFalse();
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()!.Contains("Matter with ID:") &&
                        v.ToString()!.Contains("does not exist") &&
                        v.ToString()!.Contains(matterId.ToString())),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once());
        }

        /// <summary>
        /// Should return false and log an error if an exception is thrown during the operation.
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ShouldReturnFalse_WhenExceptionThrown()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var matterDto = new MatterDto
            {
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };

            _validationServiceMock
                .Setup(v => v.ValidateNotNull(matterDto, It.IsAny<string>()))
                .Returns(null as Microsoft.AspNetCore.Mvc.ActionResult);
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync(null as Microsoft.AspNetCore.Mvc.ActionResult);

            _contextMock.Setup(c => c.Matters.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Matter, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB error"));

            var repo = CreateRepository();

            // Act
            var result = await repo.DeleteMatterAsync(matterDto);

            // Assert
            result.Should().BeFalse();
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "An error occurred while deleting the matter: {ErrorMessage}", It.IsAny<object[]>()), Times.Once());
        }

        /// <summary>
        /// Should return true and log information when the matter is successfully deleted.
        /// </summary>
        [Fact]
        public async Task DeleteMatterAsync_ShouldReturnTrue_WhenSuccessful()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var matterDto = new MatterDto
            {
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var dbMatter = new Matter
            {
                Id = matterId,
                Description = "Test Matter",
                CreationDate = DateTime.UtcNow
            };
            var user = new User 
            { 
                Id = Guid.NewGuid(),
                Name = "Test User"
            };
            var matterActivity = new MatterActivity 
            { 
                Id = Guid.NewGuid(),
                Activity = "DELETED"
            };

            _validationServiceMock
                .Setup(v => v.ValidateNotNull(matterDto, It.IsAny<string>()))
                .Returns(null as Microsoft.AspNetCore.Mvc.ActionResult);
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync(null as Microsoft.AspNetCore.Mvc.ActionResult);

            _contextMock.Setup(c => c.Matters.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Matter, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbMatter);

            var repoMock = CreateRepositoryMock();
            repoMock.Setup(r => r.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync(user);
            _contextMock.Setup(c => c.MatterActivities.AsNoTracking().FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<MatterActivity, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(matterActivity);

            repoMock.Protected()
                .Setup<Task>("AddAuditLogAsync", dbMatter, dbMatter.Id, matterActivity, matterActivity.Id, user, user.Id)
                .Returns(Task.CompletedTask);

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            // Act
            var result = await repoMock.Object.DeleteMatterAsync(matterDto);

            // Assert
            result.Should().BeTrue();
            _loggerMock.Verify(l => l.LogInformation("Successfully deleted matter with ID {MatterId}", matterId), Times.Once());
        }

        private AdmsRepository CreateRepository()
        {
            return new AdmsRepository(
                _loggerMock.Object,
                _contextMock.Object,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object
            );
        }

        private Mock<AdmsRepository> CreateRepositoryMock()
        {
            return new Mock<AdmsRepository>(
                _loggerMock.Object,
                _contextMock.Object,
                _mapperMock.Object,
                _propertyMappingServiceMock.Object,
                _validationServiceMock.Object
            )
            { CallBase = true };
        }
    }
}
