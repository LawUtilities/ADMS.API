using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.ResourceParameters;
using ADMS.API.Services;

using MapsterMapper;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace ADMS.API.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="AdmsRepository.GetDocumentActivityAuditsAsync(Guid, DocumentAuditsResourceParameters)"/>.
    /// </summary>
    public class AdmsRepositoryGetDocumentActivityAuditsAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        /// <summary>
        /// Creates an in-memory database context for testing.
        /// </summary>
        /// <returns></returns>
        private static AdmsContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AdmsContext(options);
        }

        /// <summary>
        /// Verifies that an ArgumentException is thrown when the document does not exist.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityAuditsAsync_ThrowsArgumentException_WhenDocumentDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var docId = Guid.NewGuid();
            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 10 };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ReturnsAsync(new object() as Microsoft.AspNetCore.Mvc.ActionResult); // Not null means not found

            var repo = CreateRepository(context);

            // Act
            Func<Task> act = async () => await repo.GetDocumentActivityAuditsAsync(docId, resourceParams);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        /// <summary>
        /// Verifies that a paged list is returned with correct sorting by CreatedAt descending when no OrderBy is specified.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityAuditsAsync_ReturnsPagedList_SortedByCreatedAtDescending_WhenNoOrderBy()
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

            var document = new Document 
            { 
                Id = Guid.NewGuid(),
                FileName = "Test Document",
                Extension = ".txt",
                FileSize = 1024,
                MimeType = "text/plain",
                Matter = matter,
                MatterId = matter.Id
            };
            context.Documents.Add(document);

            var activity = new DocumentActivity
            {
                Id = Guid.NewGuid(),
                Activity = "CREATED"
            };
            context.DocumentActivities.Add(activity);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "TestUser"
            };
            context.Users.Add(user);

            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 10 };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(document.Id))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var audits = new List<DocumentActivityUser>
            {
                new() {
                    DocumentId = document.Id,
                    Document = document,
                    DocumentActivityId = activity.Id,
                    DocumentActivity = activity,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-1)
                },
                new() {
                    DocumentId = document.Id,
                    Document = document,
                    DocumentActivityId = activity.Id,
                    DocumentActivity = activity,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.DocumentActivityUsers.AddRange(audits);
            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetDocumentActivityAuditsAsync(document.Id, resourceParams);

            // Assert
            result.Should().NotBeNull();
            result?.Value!.Count.Should().Be(2);
            result?.Value![0].CreatedAt.Should().BeAfter(result.Value[1].CreatedAt);
        }

        /// <summary>
        /// Verifies that a paged list is returned with correct sorting by Activity when OrderBy is "activity".
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityAuditsAsync_ReturnsPagedList_SortedByActivity_WhenOrderByActivity()
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

            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = "Test Document",
                Extension = ".txt",
                FileSize = 1024,
                MimeType = "text/plain",
                Matter = matter,
                MatterId = matter.Id
            };
            context.Documents.Add(document);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "TestUser"
            };
            context.Users.Add(user);

            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 10, OrderBy = "activity" };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(document.Id))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var activityA = new DocumentActivity { Id = Guid.NewGuid(), Activity = "A" };
            var activityB = new DocumentActivity { Id = Guid.NewGuid(), Activity = "B" };

            var audits = new List<DocumentActivityUser>
            {
                new() {
                    DocumentId = document.Id,
                    Document = document,
                    DocumentActivityId = activityB.Id,
                    DocumentActivity = activityB,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow
                },
                new ()
                {
                    DocumentId = document.Id,
                    Document = document,
                    DocumentActivityId = activityA.Id,
                    DocumentActivity = activityA,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.DocumentActivityUsers.AddRange(audits);
            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetDocumentActivityAuditsAsync(document.Id, resourceParams);

            // Assert
            result.Should().NotBeNull();
            result?.Value!.Count.Should().Be(2);
            result?.Value![0].DocumentActivity.Activity.Should().Be("A");
            result?.Value![1].DocumentActivity.Activity.Should().Be("B");
        }

        /// <summary>
        /// Verifies that a paged list is returned with correct sorting by CreatedAt descending when OrderBy is "createdat".
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityAuditsAsync_ReturnsPagedList_SortedByCreatedAtDescending_WhenOrderByCreatedAt()
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

            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = "Test Document",
                Extension = ".txt",
                FileSize = 1024,
                MimeType = "text/plain",
                Matter = matter,
                MatterId = matter.Id
            };
            context.Documents.Add(document);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "TestUser"
            };
            context.Users.Add(user);
            
            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 10, OrderBy = "createdat" };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(document.Id))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var activity = new DocumentActivity { Id = Guid.NewGuid(), Activity = "CREATED" };

            var audits = new List<DocumentActivityUser>
            {
                new() {
                    DocumentId = document.Id,
                    Document = document,
                    DocumentActivityId = activity.Id,
                    DocumentActivity = activity,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-1)
                },
                new() {
                    DocumentId = document.Id,
                    Document = document,
                    DocumentActivityId = activity.Id,
                    DocumentActivity = activity,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.DocumentActivityUsers.AddRange(audits);
            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetDocumentActivityAuditsAsync(document.Id, resourceParams);

            // Assert
            result.Should().NotBeNull();
            result?.Value!.Count.Should().Be(2);
            result?.Value![0].CreatedAt.Should().BeAfter(result.Value[1].CreatedAt);
        }

        /// <summary>
        /// Verifies that an empty paged list is returned when no audits exist.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityAuditsAsync_ReturnsEmptyPagedList_WhenNoAuditsExist()
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

            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = "Test Document",
                Extension = ".txt",
                FileSize = 1024,
                MimeType = "text/plain",
                Matter = matter,
                MatterId = matter.Id
            };
            context.Documents.Add(document);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "TestUser"
            };
            context.Users.Add(user);

            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 10 };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(document.Id))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetDocumentActivityAuditsAsync(document.Id, resourceParams);

            // Assert
            result.Should().NotBeNull();
            result?.Value!.Count.Should().Be(0);
        }

        /// <summary>
        /// Verifies that a paged list is returned with correct paging.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityAuditsAsync_ReturnsPagedList_WithCorrectPaging()
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

            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = "Test Document",
                Extension = ".txt",
                FileSize = 1024,
                MimeType = "text/plain",
                Matter = matter,
                MatterId = matter.Id
            };
            context.Documents.Add(document);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "TestUser"
            };
            context.Users.Add(user);

            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 2, PageSize = 2 };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(document.Id))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            var activity = new DocumentActivity { Id = Guid.NewGuid(), Activity = "CREATED" };
            for (int i = 0; i < 5; i++)
            {
                context.DocumentActivityUsers.Add(new DocumentActivityUser
                {
                    DocumentId = document.Id,
                    Document = document,
                    DocumentActivityId = activity.Id,
                    DocumentActivity = activity,
                    UserId = user.Id,
                    User = user,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i)
                });
            }
            await context.SaveChangesAsync();

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetDocumentActivityAuditsAsync(document.Id, resourceParams);

            // Assert
            result.Should().NotBeNull();
            result?.Value!.Count.Should().Be(2);
            result?.Value!.CurrentPage.Should().Be(2);
            result?.Value!.PageSize.Should().Be(2);
            result?.Value!.TotalCount.Should().Be(5);
        }

        /// <summary>
        /// Verifies that an empty paged list is returned if an exception occurs in the try block.
        /// </summary>
        [Fact]
        public async Task GetDocumentActivityAuditsAsync_ReturnsEmptyPagedList_WhenExceptionOccurs()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var docId = Guid.NewGuid();
            var resourceParams = new DocumentAuditsResourceParameters { PageNumber = 1, PageSize = 10 };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ReturnsAsync((Microsoft.AspNetCore.Mvc.ActionResult?)null);

            // Simulate context disposed to force exception
            await context.DisposeAsync();

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetDocumentActivityAuditsAsync(docId, resourceParams);

            // Assert
            result.Should().NotBeNull();
            result?.Value!.Count.Should().Be(0);
        }

        /// <summary>
        /// Creates an instance of <see cref="AdmsRepository"/> with the provided context and mocks.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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
