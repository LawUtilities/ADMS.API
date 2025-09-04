using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Helpers;
using ADMS.API.ResourceParameters;
using ADMS.API.Services;

using MapsterMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace ADMS.API.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="AdmsRepository.GetPaginatedRevisionsAsync"/>.
    /// </summary>
    public class AdmsRepositoryGetPaginatedRevisionsAsyncTests
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
        public async Task GetPaginatedRevisionsAsync_ReturnsBadRequest_WhenDocumentIdIsInvalid()
        {
            // Arrange
            var context = CreateContext(nameof(GetPaginatedRevisionsAsync_ReturnsBadRequest_WhenDocumentIdIsInvalid));
            var repo = CreateRepository(context);
            var invalidDocumentId = Guid.Empty;
            var resourceParameters = new RevisionsResourceParameters { PageNumber = 1, PageSize = 10 };

            // Act
            var result = await repo.GetPaginatedRevisionsAsync(invalidDocumentId, resourceParameters);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Verifies that a BadRequest is returned if resourceParameters is null.
        /// </summary>
        [Fact]
        public async Task GetPaginatedRevisionsAsync_ReturnsBadRequest_WhenResourceParametersIsNull()
        {
            // Arrange
            var context = CreateContext(nameof(GetPaginatedRevisionsAsync_ReturnsBadRequest_WhenResourceParametersIsNull));
            var repo = CreateRepository(context);
            var documentId = Guid.NewGuid();

            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var result = await repo.GetPaginatedRevisionsAsync(documentId, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Verifies that a BadRequest is returned if PageNumber is less than or equal to 0.
        /// </summary>
        [Fact]
        public async Task GetPaginatedRevisionsAsync_ReturnsBadRequest_WhenPageNumberIsInvalid()
        {
            // Arrange
            var context = CreateContext(nameof(GetPaginatedRevisionsAsync_ReturnsBadRequest_WhenPageNumberIsInvalid));
            var repo = CreateRepository(context);
            var documentId = Guid.NewGuid();
            var resourceParameters = new RevisionsResourceParameters { PageNumber = 0, PageSize = 10 };

            // Act
            var result = await repo.GetPaginatedRevisionsAsync(documentId, resourceParameters);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Verifies that a BadRequest is returned if PageSize is less than or equal to 0.
        /// </summary>
        [Fact]
        public async Task GetPaginatedRevisionsAsync_ReturnsBadRequest_WhenPageSizeIsInvalid()
        {
            // Arrange
            var context = CreateContext(nameof(GetPaginatedRevisionsAsync_ReturnsBadRequest_WhenPageSizeIsInvalid));
            var repo = CreateRepository(context);
            var documentId = Guid.NewGuid();
            var resourceParameters = new RevisionsResourceParameters { PageNumber = 1, PageSize = 0 };

            // Act
            var result = await repo.GetPaginatedRevisionsAsync(documentId, resourceParameters);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Verifies that a NotFound is returned if the document does not exist.
        /// </summary>
        [Fact]
        public async Task GetPaginatedRevisionsAsync_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            var context = CreateContext(nameof(GetPaginatedRevisionsAsync_ReturnsNotFound_WhenDocumentDoesNotExist));
            var repo = CreateRepository(context);
            var documentId = Guid.NewGuid();
            var resourceParameters = new RevisionsResourceParameters { PageNumber = 1, PageSize = 10 };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync(new NotFoundResult());

            // Act
            var result = await repo.GetPaginatedRevisionsAsync(documentId, resourceParameters);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        /// <summary>
        /// Verifies that a 500 error is returned if an exception occurs during document validation.
        /// </summary>
        [Fact]
        public async Task GetPaginatedRevisionsAsync_ReturnsServerError_WhenValidationThrows()
        {
            // Arrange
            var context = CreateContext(nameof(GetPaginatedRevisionsAsync_ReturnsServerError_WhenValidationThrows));
            var repo = CreateRepository(context);
            var documentId = Guid.NewGuid();
            var resourceParameters = new RevisionsResourceParameters { PageNumber = 1, PageSize = 10 };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ThrowsAsync(new Exception("Validation error"));

            // Act
            var result = await repo.GetPaginatedRevisionsAsync(documentId, resourceParameters);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        /// <summary>
        /// Verifies that a 500 error is returned if an exception occurs during query execution.
        /// </summary>
        [Fact]
        public async Task GetPaginatedRevisionsAsync_ReturnsServerError_WhenQueryThrows()
        {
            // Arrange
            var contextMock = new Mock<AdmsContext>(new DbContextOptionsBuilder<AdmsContext>().Options);
            var repo = CreateRepository(contextMock.Object);
            var documentId = Guid.NewGuid();
            var resourceParameters = new RevisionsResourceParameters { PageNumber = 1, PageSize = 10 };
            _validationServiceMock
                .Setup(v => v.ValidateDocumentExistsAsync(documentId))
                .ReturnsAsync((ActionResult?)null);

            // Simulate exception on query
            contextMock.Setup(c => c.Revisions)
                .Throws(new Exception("Query error"));

            // Act
            var result = await repo.GetPaginatedRevisionsAsync(documentId, resourceParameters);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        /// <summary>
        /// Verifies that OkObjectResult is returned with the correct paginated revisions when the document exists and no errors occur.
        /// </summary>
        [Fact]
        public async Task GetPaginatedRevisionsAsync_ReturnsOk_WithPagedRevisions_WhenDocumentExists()
        {
            // Arrange
            var context = CreateContext(nameof(GetPaginatedRevisionsAsync_ReturnsOk_WithPagedRevisions_WhenDocumentExists));
            var repo = CreateRepository(context);
            var documentId = Guid.NewGuid();
            var resourceParameters = new RevisionsResourceParameters { PageNumber = 1, PageSize = 10, IncludeDeleted = false };
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
            var result = await repo.GetPaginatedRevisionsAsync(documentId, resourceParameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pagedList = Assert.IsType<Helpers.PagedList<Revision>>(okResult.Value);
            Assert.Single(pagedList);
            Assert.All(pagedList, r => Assert.False(r.IsDeleted));
        }

        /// <summary>
        /// Verifies that OkObjectResult is returned with all paged revisions when IncludeDeleted is true.
        /// </summary>
        [Fact]
        public async Task GetPaginatedRevisionsAsync_ReturnsOk_WithAllPagedRevisions_WhenIncludeDeletedTrue()
        {
            // Arrange
            var context = CreateContext(nameof(GetPaginatedRevisionsAsync_ReturnsOk_WithAllPagedRevisions_WhenIncludeDeletedTrue));
            var repo = CreateRepository(context);
            var documentId = Guid.NewGuid();
            var resourceParameters = new RevisionsResourceParameters { PageNumber = 1, PageSize = 10, IncludeDeleted = true };
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
            var result = await repo.GetPaginatedRevisionsAsync(documentId, resourceParameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pagedList = Assert.IsType<Helpers.PagedList<Revision>>(okResult.Value);
            Assert.Equal(2, pagedList.Count);
        }
    }
}
