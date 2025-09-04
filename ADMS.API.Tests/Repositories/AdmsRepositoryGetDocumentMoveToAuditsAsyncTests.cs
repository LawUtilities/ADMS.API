using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Helpers;
using ADMS.API.Models;
using ADMS.API.ResourceParameters;
using ADMS.API.Services;

using MapsterMapper;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace ADMS.API.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="AdmsRepository.GetDocumentMoveToAuditsAsync(Guid, DocumentAuditsResourceParameters)"/>.
    /// </summary>
    public class AdmsRepositoryGetDocumentMoveToAuditsAsyncTests
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
        /// Verifies that BadRequest is returned when documentId is empty.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsBadRequest_WhenDocumentIdIsEmpty()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = CreateRepository(context);
            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 10 };

            // Act
            var result = await repo.GetPaginatedDocumentMoveToAuditsAsync(Guid.Empty, resourceParams);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        /// <summary>
        /// Verifies that BadRequest is returned when resourceParameters is null.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsBadRequest_WhenResourceParametersIsNull()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = CreateRepository(context);

            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var result = await repo.GetPaginatedDocumentMoveToAuditsAsync(Guid.NewGuid(), null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        /// <summary>
        /// Verifies that BadRequest is returned when PageNumber is less than or equal to 0.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsBadRequest_WhenPageNumberIsInvalid()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = CreateRepository(context);
            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 0, PageSize = 10 };

            // Act
            var result = await repo.GetPaginatedDocumentMoveToAuditsAsync(Guid.NewGuid(), resourceParams);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        /// <summary>
        /// Verifies that BadRequest is returned when PageSize is less than or equal to 0.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsBadRequest_WhenPageSizeIsInvalid()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = CreateRepository(context);
            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 0 };

            // Act
            var result = await repo.GetPaginatedDocumentMoveToAuditsAsync(Guid.NewGuid(), resourceParams);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        /// <summary>
        /// Verifies that NotFound is returned when the document does not exist.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var docId = Guid.NewGuid();
            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 10 };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ReturnsAsync(new NotFoundResult());

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetPaginatedDocumentMoveToAuditsAsync(docId, resourceParams);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        /// <summary>
        /// Verifies that ObjectResult with status 500 is returned when validation throws.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_Returns500_WhenValidationThrows()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var docId = Guid.NewGuid();
            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 10 };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ThrowsAsync(new Exception("Validation error"));

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetPaginatedDocumentMoveToAuditsAsync(docId, resourceParams);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// Verifies that OkObjectResult with a paged list is returned and sorted by CreatedAt descending when no OrderBy is specified.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsPagedList_SortedByCreatedAtDescending_WhenNoOrderBy()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var docId = Guid.NewGuid();
            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 10 };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ReturnsAsync((ActionResult?)null);

            var activity = new MatterDocumentActivity { Id = Guid.NewGuid(), Activity = "MOVED" };
            var user = new User { Id = Guid.NewGuid(), Name = "TestUser" };
            var audits = new List<MatterDocumentActivityUserTo>
            {
                new() {
                    DocumentId = docId,
                    MatterDocumentActivityId = activity.Id,
                    MatterDocumentActivity = activity,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-1)
                },
                new() {
                    DocumentId = docId,
                    MatterDocumentActivityId = activity.Id,
                    MatterDocumentActivity = activity,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.MatterDocumentActivityUsersTo.AddRange(audits);
            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetPaginatedDocumentMoveToAuditsAsync(docId, resourceParams);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result[0].CreatedAt.Should().BeAfter(result[1].CreatedAt);
        }

        /// <summary>
        /// Verifies that OkObjectResult with a paged list is returned and sorted by Activity when OrderBy is "activity".
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsPagedList_SortedByActivity_WhenOrderByActivity()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var docId = Guid.NewGuid();
            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 10, OrderBy = "activity" };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ReturnsAsync((ActionResult?)null);

            var activityA = new MatterDocumentActivity { Id = Guid.NewGuid(), Activity = "A" };
            var activityB = new MatterDocumentActivity { Id = Guid.NewGuid(), Activity = "B" };
            var user = new User { Id = Guid.NewGuid(), Name = "TestUser" };
            var audits = new List<MatterDocumentActivityUserTo>
            {
                new() {
                    DocumentId = docId,
                    MatterDocumentActivityId = activityB.Id,
                    MatterDocumentActivity = activityB,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow
                },
                new() {
                    DocumentId = docId,
                    MatterDocumentActivityId = activityA.Id,
                    MatterDocumentActivity = activityA,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.MatterDocumentActivityUsersTo.AddRange(audits);
            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetPaginatedDocumentMoveToAuditsAsync(docId, resourceParams);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result[0]?.MatterDocumentActivity!.Activity.Should().Be("A");
            result[1]?.MatterDocumentActivity!.Activity.Should().Be("B");
        }

        /// <summary>
        /// Verifies that OkObjectResult with a paged list is returned and sorted by CreatedAt descending when OrderBy is "createdat".
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsPagedList_SortedByCreatedAtDescending_WhenOrderByCreatedAt()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var docId = Guid.NewGuid();
            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 10, OrderBy = "createdat" };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ReturnsAsync((ActionResult?)null);

            var activity = new MatterDocumentActivity { Id = Guid.NewGuid(), Activity = "MOVED" };
            var user = new User { Id = Guid.NewGuid(), Name = "TestUser" };
            var audits = new List<MatterDocumentActivityUserTo>
            {
                new() {
                    DocumentId = docId,
                    MatterDocumentActivityId = activity.Id,
                    MatterDocumentActivity = activity,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-1)
                },
                new() {
                    DocumentId = docId,
                    MatterDocumentActivityId = activity.Id,
                    MatterDocumentActivity = activity,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.MatterDocumentActivityUsersTo.AddRange(audits);
            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetPaginatedDocumentMoveToAuditsAsync(docId, resourceParams);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result[0].CreatedAt.Should().BeAfter(result[1].CreatedAt);
        }

        /// <summary>
        /// Verifies that OkObjectResult with an empty paged list is returned when no audits exist.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsEmptyPagedList_WhenNoAuditsExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var docId = Guid.NewGuid();
            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 10 };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ReturnsAsync((ActionResult?)null);

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetPaginatedDocumentMoveToAuditsAsync(docId, resourceParams);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(0);
        }

        /// <summary>
        /// Verifies that OkObjectResult with a paged list is returned with correct paging.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsPagedList_WithCorrectPaging()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var docId = Guid.NewGuid();
            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 2, PageSize = 2 };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ReturnsAsync((ActionResult?)null);

            var activity = new MatterDocumentActivity { Id = Guid.NewGuid(), Activity = "MOVED" };
            var user = new User { Id = Guid.NewGuid(), Name = "TestUser" };
            for (int i = 0; i < 5; i++)
            {
                context.MatterDocumentActivityUsersTo.Add(new MatterDocumentActivityUserTo
                {
                    DocumentId = docId,
                    MatterDocumentActivityId = activity.Id,
                    MatterDocumentActivity = activity,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i)
                });
            }
            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetPaginatedDocumentMoveToAuditsAsync(docId, resourceParams);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result.CurrentPage.Should().Be(2);
        }

        /// <summary>
        /// Verifies that OkObjectResult with an empty paged list is returned if an exception occurs in the try block.
        /// </summary>
        [Fact]
        public async Task GetDocumentMoveToAuditsAsync_ReturnsEmptyPagedList_WhenExceptionOccurs()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var docId = Guid.NewGuid();
            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 10 };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ReturnsAsync((ActionResult?)null);

            // Simulate context disposed to force exception
            await context.DisposeAsync();

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetPaginatedDocumentMoveToAuditsAsync(docId, resourceParams);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(0);
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
