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
    /// Unit tests for <see cref="AdmsRepository.GetRevisionsAsync"/>.
    /// </summary>
    public class AdmsRepositoryGetRevisionsAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        /// <summary>
        /// Creates a new in-memory context for each test.
        /// </summary>
        private static AdmsContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AdmsContext(options);
        }

        /// <summary>
        /// Returns a new instance of <see cref="AdmsRepository"/> with all dependencies mocked.
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
        /// Verifies that a BadRequest is returned if the documentId is invalid.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsBadRequest_WhenDocumentIdIsInvalid()
        {
            // Arrange
            var context = CreateContext(nameof(GetRevisionsAsync_ReturnsBadRequest_WhenDocumentIdIsInvalid));
            var repo = CreateRepository(context);
            var invalidDocumentId = Guid.Empty;
            _validationServiceMock
                .Setup(v => v.ValidateGuid(invalidDocumentId, It.IsAny<string>()))
                .Returns(new BadRequestResult());

            // Act
            var result = await repo.GetRevisionsAsync(invalidDocumentId, false);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Verifies that a NotFound is returned if the document does not exist.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            var context = CreateContext(nameof(GetRevisionsAsync_ReturnsNotFound_WhenDocumentDoesNotExist));
            var repo = CreateRepository(context);
            var documentId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateGuid(documentId, It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync(new NotFoundResult());

            // Act
            var result = await repo.GetRevisionsAsync(documentId, false);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        /// <summary>
        /// Verifies that a 500 error is returned if an exception occurs during document validation.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsServerError_WhenValidationThrows()
        {
            // Arrange
            var context = CreateContext(nameof(GetRevisionsAsync_ReturnsServerError_WhenValidationThrows));
            var repo = CreateRepository(context);
            var documentId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateGuid(documentId, It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ThrowsAsync(new Exception("Validation error"));

            // Act
            var result = await repo.GetRevisionsAsync(documentId, false);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
        }


        /// <summary>
        /// Verifies that OkObjectResult is returned with the correct revisions when the document exists and no errors occur.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsOk_WithRevisions_WhenDocumentExists()
        {
            // Arrange
            var context = CreateContext(nameof(GetRevisionsAsync_ReturnsOk_WithRevisions_WhenDocumentExists));
            var repo = CreateRepository(context);
            var documentId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateGuid(documentId, It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            // Add some revisions
            var revisions = new List<Revision>
            {
                new() { DocumentId = documentId, IsDeleted = false, RevisionNumber = 1 },
                new() { DocumentId = documentId, IsDeleted = true, RevisionNumber = 2 }
            };
            context.Revisions.AddRange(revisions);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetRevisionsAsync(documentId, includeDeleted: false);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var queryable = Assert.IsType<IQueryable<Revision>>(okResult.Value, exactMatch: false);
            var returned = await queryable.ToListAsync();
            Assert.Single(returned);
            Assert.All(returned, r => Assert.False(r.IsDeleted));
        }

        /// <summary>
        /// Verifies that OkObjectResult is returned with all revisions when includeDeleted is true.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsOk_WithAllRevisions_WhenIncludeDeletedTrue()
        {
            // Arrange
            var context = CreateContext(nameof(GetRevisionsAsync_ReturnsOk_WithAllRevisions_WhenIncludeDeletedTrue));
            var repo = CreateRepository(context);
            var documentId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateGuid(documentId, It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            // Add some revisions
            var revisions = new List<Revision>
            {
                new() { DocumentId = documentId, IsDeleted = false, RevisionNumber = 1 },
                new() { DocumentId = documentId, IsDeleted = true, RevisionNumber = 2 }
            };
            context.Revisions.AddRange(revisions);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetRevisionsAsync(documentId, includeDeleted: true);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var queryable = Assert.IsType<IQueryable<Revision>>(okResult.Value, exactMatch: false);
            var returned = await queryable.ToListAsync();
            Assert.Equal(2, returned.Count);
        }

        /// <summary>
        /// Verifies that a 500 error is returned if an exception occurs during query execution.
        /// </summary>
        [Fact]
        public async Task GetRevisionsAsync_ReturnsServerError_WhenQueryThrows()
        {
            // Arrange
            var contextMock = new Mock<AdmsContext>(new DbContextOptionsBuilder<AdmsContext>().Options);
            var repo = CreateRepository(contextMock.Object);
            var documentId = Guid.NewGuid();
            _validationServiceMock
                .Setup(v => v.ValidateGuid(documentId, It.IsAny<string>()))
                .Returns((ActionResult?)null);
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            // Simulate exception on query
            contextMock.Setup(c => c.Revisions)
                .Throws(new Exception("Query error"));

            // Act
            var result = await repo.GetRevisionsAsync(documentId, false);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
