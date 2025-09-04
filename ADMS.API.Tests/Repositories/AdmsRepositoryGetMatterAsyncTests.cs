using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Services;

using MapsterMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;
using Moq.Protected;

namespace ADMS.API.Tests.Repositories
{
    /// <summary>
    /// Unit tests for AdmsRepository.GetMatterAsync.
    /// </summary>
    public class AdmsRepositoryGetMatterAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        /// <summary>
        /// Helper to create an in-memory context with seeded matters.
        /// </summary>
        private static AdmsContext CreateContextWithMatters(IEnumerable<Matter> matters, IEnumerable<MatterActivity>? activities = null)
        {
            var options = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new AdmsContext(options);
            context.Matters.AddRange(matters);
            if (activities != null)
                context.MatterActivities.AddRange(activities);
            context.SaveChanges();
            return context;
        }

        /// <summary>
        /// Returns a valid Matter entity.
        /// </summary>
        private static Matter ValidMatter(Guid? id = null)
        {
            return new()
            {
                Id = id ?? Guid.NewGuid(),
                Description = "Test Matter",
                IsArchived = false,
                IsDeleted = false,
                CreationDate = DateTime.UtcNow,
                Documents = [],
                MatterActivityUsers = [],
                MatterDocumentActivityUsersFrom = [],
                MatterDocumentActivityUsersTo = []
            };
        }

        /// <summary>
        /// Test: Returns null if matterId is empty.
        /// </summary>
        [Fact]
        public async Task ReturnsNull_WhenMatterIdIsEmpty()
        {
            // Arrange
            var context = CreateContextWithMatters([]);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(Guid.Empty))
                .Returns(Task.FromResult<ActionResult?>(null));
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.GetMatterAsync(Guid.Empty, false);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Test: Returns null if matter does not exist.
        /// </summary>
        [Fact]
        public async Task ReturnsNull_WhenMatterDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var context = CreateContextWithMatters([]);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(id)).ReturnsAsync(new Microsoft.AspNetCore.Mvc.NotFoundResult());
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.GetMatterAsync(id, false);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Test: Returns the matter if it exists and no documents/history are requested.
        /// </summary>
        [Fact]
        public async Task ReturnsMatter_WhenExists_NoDocumentsOrHistory()
        {
            // Arrange
            var matter = ValidMatter();
            var context = CreateContextWithMatters([matter]);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id)).ReturnsAsync((ActionResult?)null);
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Mock user and activity for audit
            var user = new User { Id = Guid.NewGuid(), Name = "rbrown" };
            context.Users.Add(user);
            context.MatterActivities.Add(new MatterActivity { Id = Guid.NewGuid(), Activity = "VIEWED" });
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetMatterAsync(matter.Id, false, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(matter.Id, result.Id);
        }

        /// <summary>
        /// Test: Returns the matter with documents if includeDocuments is true.
        /// </summary>
        [Fact]
        public async Task ReturnsMatter_WithDocuments_WhenIncludeDocumentsIsTrue()
        {
            // Arrange
            var matter = ValidMatter();
            matter.Documents.Add(new Document 
            { 
                Id = Guid.NewGuid(), 
                MatterId = matter.Id, 
                FileName = "doc1.txt",
                Extension = ".txt",
                Matter = matter,
            });
            var context = CreateContextWithMatters([matter]);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id)).ReturnsAsync((ActionResult?)null);
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Mock user and activity for audit
            var user = new User { Id = Guid.NewGuid(), Name = "rbrown" };
            context.Users.Add(user);
            context.MatterActivities.Add(new MatterActivity { Id = Guid.NewGuid(), Activity = "VIEWED" });
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetMatterAsync(matter.Id, true, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(matter.Id, result.Id);
            Assert.NotNull(result.Documents);
            Assert.Single(result.Documents);
        }

        /// <summary>
        /// Test: Returns the matter with history if includeHistory is true.
        /// </summary>
        [Fact]
        public async Task ReturnsMatter_WithHistory_WhenIncludeHistoryIsTrue()
        {
            // Arrange
            var matter = ValidMatter();
            matter.MatterActivityUsers.Add(new MatterActivityUser { MatterId = matter.Id, MatterActivityId = Guid.NewGuid(), UserId = Guid.NewGuid() });
            var context = CreateContextWithMatters([matter]);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id)).ReturnsAsync((ActionResult?)null);
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Mock user and activity for audit
            var user = new User { Id = Guid.NewGuid(), Name = "rbrown" };
            context.Users.Add(user);
            context.MatterActivities.Add(new MatterActivity { Id = Guid.NewGuid(), Activity = "VIEWED" });
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetMatterAsync(matter.Id, false, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(matter.Id, result.Id);
            Assert.NotNull(result.MatterActivityUsers);
            Assert.Single(result.MatterActivityUsers);
        }

        /// <summary>
        /// Test: Returns the matter even if user or activity for audit is not found.
        /// </summary>
        [Fact]
        public async Task ReturnsMatter_WhenUserOrActivityNotFoundForAudit()
        {
            // Arrange
            var matter = ValidMatter();
            var context = CreateContextWithMatters([matter]);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id)).ReturnsAsync((ActionResult?)null);
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // No user or activity added to context

            // Act
            var result = await repo.GetMatterAsync(matter.Id, false, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(matter.Id, result.Id);
        }

        /// <summary>
        /// Test: Returns null if an exception occurs during query.
        /// </summary>
        [Fact]
        public async Task ReturnsNull_WhenExceptionOccurs()
        {
            // Arrange
            var matter = ValidMatter();
            var contextMock = new Mock<AdmsContext>(new DbContextOptionsBuilder<AdmsContext>().Options);
            contextMock.Setup(c => c.Matters).Throws(new Exception("DB error"));
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id)).ReturnsAsync((ActionResult?)null);
            var repo = new AdmsRepository(_loggerMock.Object, contextMock.Object, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.GetMatterAsync(matter.Id, false, false);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Test: Audit log is called when user and activity are found.
        /// </summary>
        [Fact]
        public async Task CallsAuditLog_WhenUserAndActivityFound()
        {
            // Arrange
            var matter = ValidMatter();
            var context = CreateContextWithMatters([matter]);
            _validationServiceMock.Setup(v => v.ValidateMatterExistsAsync(matter.Id)).ReturnsAsync((ActionResult?)null);

            // Add user and activity
            var user = new User { Id = Guid.NewGuid(), Name = "rbrown" };
            var activity = new MatterActivity { Id = Guid.NewGuid(), Activity = "VIEWED" };
            context.Users.Add(user);
            context.MatterActivities.Add(activity);
            await context.SaveChangesAsync();

            var repoMock = new Mock<AdmsRepository>(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object)
            {
                CallBase = true
            };
            repoMock.Protected()
                .Setup<Task>("AddAuditLogAsync", ItExpr.IsAny<Matter>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<MatterActivity>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<User>(), ItExpr.IsAny<Guid>())
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await repoMock.Object.GetMatterAsync(matter.Id, false, false);

            // Assert
            Assert.NotNull(result);
            repoMock.Protected().Verify("AddAuditLogAsync", Times.Once(), ItExpr.IsAny<Matter>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<MatterActivity>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<User>(), ItExpr.IsAny<Guid>());
        }
    }
}