using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Helpers;
using ADMS.API.Services;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace ADMS.API.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="AdmsRepository.GetExtendedAuditsAsync"/>.
    /// </summary>
    public class AdmsRepositoryGetExtendedAuditsAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();
        private readonly Mock<IMapper> _mapperMock = new();

        /// <summary>
        /// Helper to create an in-memory context with optional seed data.
        /// </summary>
        private static AdmsContext CreateContext(
            IEnumerable<MatterDocumentActivityUserFrom>? from = null,
            IEnumerable<MatterDocumentActivityUserTo>? to = null)
        {
            var options = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new AdmsContext(options);
            if (from != null)
            {
                context.MatterDocumentActivityUsersFrom.AddRange(from);
            }
            if (to != null)
            {
                context.MatterDocumentActivityUsersTo.AddRange(to);
            }
            context.SaveChanges();
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
        /// Test: Throws ArgumentException if both matterId and documentId are empty.
        /// </summary>
        [Fact]
        public async Task ThrowsArgumentException_WhenBothMatterIdAndDocumentIdAreEmpty()
        {
            // Arrange
            var context = CreateContext();
            var repo = CreateRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                repo.GetExtendedAuditsAsync(Guid.Empty, Guid.Empty, AuditEnums.AuditDirection.From));
        }

        /// <summary>
        /// Test: Throws ArgumentException if matterId is provided but does not exist.
        /// </summary>
        [Fact]
        public async Task ThrowsArgumentException_WhenMatterIdDoesNotExist()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync(new Microsoft.AspNetCore.Mvc.NotFoundResult());
            var context = CreateContext();
            var repo = CreateRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                repo.GetExtendedAuditsAsync(matterId, Guid.Empty, AuditEnums.AuditDirection.From));
        }

        /// <summary>
        /// Test: Throws ArgumentException if documentId is provided but does not exist.
        /// </summary>
        [Fact]
        public async Task ThrowsArgumentException_WhenDocumentIdDoesNotExist()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync(new Microsoft.AspNetCore.Mvc.NotFoundResult());
            var context = CreateContext();
            var repo = CreateRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                repo.GetExtendedAuditsAsync(Guid.NewGuid(), documentId, AuditEnums.AuditDirection.From));
        }

        /// <summary>
        /// Test: Throws ArgumentOutOfRangeException if direction is invalid.
        /// </summary>
        [Fact]
        public async Task ThrowsArgumentOutOfRangeException_WhenDirectionIsInvalid()
        {
            // Arrange
            var context = CreateContext();
            var repo = CreateRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                repo.GetExtendedAuditsAsync(Guid.NewGuid(), Guid.Empty, (AuditEnums.AuditDirection)999));
        }

        /// <summary>
        /// Test: Returns correct query for FROM direction with filters.
        /// </summary>
        [Fact]
        public async Task ReturnsQuery_ForFromDirection_WithFilters()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var activityId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;

            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var from = new[]
            {
                new MatterDocumentActivityUserFrom
                {
                    MatterId = matterId,
                    DocumentId = documentId,
                    MatterDocumentActivityId = activityId,
                    UserId = userId,
                    CreatedAt = createdAt
                },
                // Should be filtered out
                new MatterDocumentActivityUserFrom
                {
                    MatterId = Guid.NewGuid(),
                    DocumentId = Guid.NewGuid(),
                    MatterDocumentActivityId = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    CreatedAt = createdAt
                }
            };

            var context = CreateContext(from: from);
            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetExtendedAuditsAsync(matterId, documentId, AuditEnums.AuditDirection.From);

            // Assert
            var list = await result.ToListAsync();
            Assert.Single(list);
            Assert.Equal(matterId, list[0].MatterId);
            Assert.Equal(documentId, list[0].DocumentId);
        }

        /// <summary>
        /// Test: Returns correct query for TO direction with filters.
        /// </summary>
        [Fact]
        public async Task ReturnsQuery_ForToDirection_WithFilters()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var activityId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;

            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var to = new[]
            {
                new MatterDocumentActivityUserTo
                {
                    MatterId = matterId,
                    DocumentId = documentId,
                    MatterDocumentActivityId = activityId,
                    UserId = userId,
                    CreatedAt = createdAt
                }
            };

            var context = CreateContext(to: to);
            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetExtendedAuditsAsync(matterId, documentId, AuditEnums.AuditDirection.To);

            // Assert
            var list = await result.ToListAsync();
            Assert.Single(list);
            Assert.Equal(matterId, list[0].MatterId);
            Assert.Equal(documentId, list[0].DocumentId);
        }

        /// <summary>
        /// Test: Throws InvalidOperationException if an unexpected error occurs.
        /// </summary>
        [Fact]
        public async Task ThrowsInvalidOperationException_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            var matterId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateMatterExistsAsync(matterId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            // Simulate exception by disposing context before use
            var context = CreateContext();
            await context.DisposeAsync();
            var repo = CreateRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.GetExtendedAuditsAsync(matterId, Guid.Empty, AuditEnums.AuditDirection.From));
        }
    }
}
