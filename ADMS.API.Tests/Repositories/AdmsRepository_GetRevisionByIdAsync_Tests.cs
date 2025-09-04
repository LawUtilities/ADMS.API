using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.Services;

using AutoMapper;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace ADMS.API.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="AdmsRepository.GetDocumentAuditsAsync(Guid)"/>.
    /// </summary>
    public class AdmsRepository_GetDocumentAuditsAsync_Tests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        private static AdmsContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AdmsContext(options);
        }

        /// <summary>
        /// Ensures BadRequestObjectResult is returned when documentId is empty.
        /// </summary>
        [Fact]
        public async Task GetDocumentAuditsAsync_ReturnsBadRequest_WhenDocumentIdIsEmpty()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetDocumentAuditsAsync(Guid.Empty);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        /// <summary>
        /// Ensures NotFoundObjectResult is returned when document does not exist.
        /// </summary>
        [Fact]
        public async Task GetDocumentAuditsAsync_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new NotFoundResult());
            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetDocumentAuditsAsync(Guid.NewGuid());

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        /// <summary>
        /// Ensures ObjectResult with status code 500 is returned when validation throws.
        /// </summary>
        [Fact]
        public async Task GetDocumentAuditsAsync_Returns500_WhenValidationThrows()
        {
            // Arrange
            var context = CreateInMemoryContext();
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Validation error"));
            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetDocumentAuditsAsync(Guid.NewGuid());

            // Assert
            var objectResult = result.Result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(500);
        }

        /// <summary>
        /// Ensures OkObjectResult with empty list is returned when no audits exist.
        /// </summary>
        [Fact]
        public async Task GetDocumentAuditsAsync_ReturnsOkWithEmptyList_WhenNoAuditsExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var docId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ReturnsAsync((ActionResult?)null);
            _mapperMock
                .Setup(m => m.Map<IEnumerable<DocumentActivityUserMinimalDto>>(It.IsAny<IEnumerable<DocumentActivityUser>>()))
                .Returns([]);
            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetDocumentAuditsAsync(docId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var value = okResult!.Value as IEnumerable<DocumentActivityUserMinimalDto>;
            value.Should().NotBeNull();
            value.Should().BeEmpty();
        }

        /// <summary>
        /// Ensures OkObjectResult with mapped audits is returned when audits exist.
        /// </summary>
        [Fact]
        public async Task GetDocumentAuditsAsync_ReturnsOkWithMappedAudits_WhenAuditsExist()
        {
            // Arrange
            var context = CreateInMemoryContext();
            
            var matter = new Matter
            {
                Id = Guid.NewGuid(),
                Description = "TestMatter",
                CreationDate = DateTime.UtcNow,
            };
            
            var document = new Document 
            { 
                Id = Guid.NewGuid(), 
                FileName = "TestDocument",
                Extension = ".txt",
                FileSize = 1234,
                MimeType = "text/plain",
                Matter = matter,
                MatterId = matter.Id,
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "TestUser"
            };

            var activity = new DocumentActivity 
            { 
                Id = Guid.NewGuid(), 
                Activity = "CREATED" 
            };

            var audit = new DocumentActivityUser
            {
                DocumentId = document.Id,
                Document = document,
                UserId = user.Id,
                DocumentActivityId = activity.Id,
                User = user,
                DocumentActivity = activity,
                CreatedAt = DateTime.UtcNow
            };
            context.Matters.Add(matter);
            context.Documents.Add(document);
            context.Users.Add(user);
            context.DocumentActivities.Add(activity);
            context.DocumentActivityUsers.Add(audit);
            await context.SaveChangesAsync();

            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(document.Id))
                .ReturnsAsync((ActionResult?)null);

            var mappedDto = new DocumentActivityUserMinimalDto
            {
                DocumentId = document.Id,
                DocumentActivity = new DocumentActivityMinimalDto { Activity = "CREATED" },
                User = new UserMinimalDto { Id = user.Id, Name = "TestUser" },
                CreatedAt = audit.CreatedAt,
            };
            _mapperMock
                .Setup(m => m.Map<IEnumerable<DocumentActivityUserMinimalDto>>(It.IsAny<IEnumerable<DocumentActivityUser>>()))
                .Returns([mappedDto]);

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetDocumentAuditsAsync(document.Id);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var value = okResult!.Value as IEnumerable<DocumentActivityUserMinimalDto>;
            value.Should().NotBeNull();
            value.Should().ContainSingle();
            value!.First().DocumentId.Should().Be(document.Id);
            value!.First().DocumentActivity.Activity.Should().Be("CREATED");
        }

        /// <summary>
        /// Ensures ObjectResult with status code 500 is returned when an exception occurs during query.
        /// </summary>
        [Fact]
        public async Task GetDocumentAuditsAsync_Returns500_WhenExceptionDuringQuery()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var docId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(docId))
                .ReturnsAsync((ActionResult?)null);

            // Simulate context disposed to force exception
            await context.DisposeAsync();

            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetDocumentAuditsAsync(docId);

            // Assert
            var objectResult = result.Result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(500);
        }

        /// <summary>
        /// Creates a new instance of <see cref="AdmsRepository"/> with the provided context and mocks.
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
