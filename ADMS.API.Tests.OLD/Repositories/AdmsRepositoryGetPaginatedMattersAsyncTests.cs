using ADMS.API.DbContexts;
using ADMS.API.Entities;
using ADMS.API.Models;
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
    /// Unit tests for AdmsRepository.GetPaginatedMattersAsync.
    /// </summary>
    public class AdmsRepositoryGetPaginatedMattersAsyncTests
    {
        private readonly Mock<ILogger<AdmsRepository>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IPropertyMappingService> _propertyMappingServiceMock = new();
        private readonly Mock<IValidationService> _validationServiceMock = new();

        /// <summary>
        /// Helper to create an in-memory context with seeded matters.
        /// </summary>
        private static AdmsContext CreateContextWithMatters(IEnumerable<Matter> matters)
        {
            var options = new DbContextOptionsBuilder<AdmsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new AdmsContext(options);
            context.Matters.AddRange(matters);
            context.SaveChanges();
            return context;
        }

        /// <summary>
        /// Returns a default valid MattersResourceParameters.
        /// </summary>
        private static MattersResourceParameters ValidParameters() => new()
        {
            PageNumber = 1,
            PageSize = 10,
            Description = string.Empty,
            IsArchived = false,
            IsDeleted = false
        };

        /// <summary>
        /// Test: Returns empty paged list if resource parameters are null.
        /// </summary>
        [Fact]
        public async Task ReturnsEmptyList_WhenResourceParametersIsNull()
        {
            // Arrange
            var context = CreateContextWithMatters([]);
            _validationServiceMock.Setup(v => v.ValidateNotNull(null, "resourceParameters")).Returns(new OkResult());
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var result = await repo.GetPaginatedMattersAsync(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(10, result.PageSize);
        }

        /// <summary>
        /// Test: Returns empty paged list if PageNumber is less than or equal to zero.
        /// </summary>
        [Fact]
        public async Task ReturnsEmptyList_WhenPageNumberIsInvalid()
        {
            // Arrange
            var context = CreateContextWithMatters([]);
            var parameters = ValidParameters();
            parameters.PageNumber = 0;
            _validationServiceMock.Setup(v => v.ValidateNotNull(parameters, "resourceParameters"))
                .Returns(new OkResult() as ActionResult);
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.GetPaginatedMattersAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(10, result.PageSize);
        }

        /// <summary>
        /// Test: Returns empty paged list if PageSize is less than or equal to zero.
        /// </summary>
        [Fact]
        public async Task ReturnsEmptyList_WhenPageSizeIsInvalid()
        {
            // Arrange
            var context = CreateContextWithMatters([]);
            var parameters = ValidParameters();
            parameters.PageSize = 0;
            _validationServiceMock.Setup(v => v.ValidateNotNull(parameters, "resourceParameters"))
                .Returns(new OkResult() as ActionResult);
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.GetPaginatedMattersAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(10, result.PageSize);
        }

        /// <summary>
        /// Test: Returns only matters matching the description filter.
        /// </summary>
        [Fact]
        public async Task ReturnsFilteredByDescription()
        {
            // Arrange
            var matters = new[]
            {
            new Matter 
            { 
                Id = Guid.NewGuid(), 
                IsArchived = false, 
                IsDeleted = false, 
                Description = "Alpha",
                CreationDate = DateTime.UtcNow
            },
            new Matter 
            { 
                Id = Guid.NewGuid(), 
                IsArchived = false, 
                IsDeleted = false, 
                Description = "Beta",
                CreationDate = DateTime.UtcNow
            }
        };
            var context = CreateContextWithMatters(matters);
            var parameters = ValidParameters();
            parameters.Description = "Alpha";
            _validationServiceMock.Setup(v => v.ValidateNotNull(parameters, "resourceParameters"))
                .Returns(new OkResult() as ActionResult);
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.GetPaginatedMattersAsync(parameters);

            // Assert
            Assert.Single(result);
            Assert.Contains(result, m => m.Description == "Alpha");
        }

        /// <summary>
        /// Test: Returns only matters that are not archived or deleted when flags are false.
        /// </summary>
        [Fact]
        public async Task ReturnsOnlyNotArchivedAndNotDeleted_WhenFlagsAreFalse()
        {
            // Arrange
            var matters = new[]
            {
            new Matter 
            { 
                Id = Guid.NewGuid(), 
                IsArchived = false, 
                IsDeleted = false, 
                Description = "A",
                CreationDate = DateTime.UtcNow
            },
            new Matter 
            { 
                Id = Guid.NewGuid(), 
                IsArchived = true, 
                IsDeleted = false, 
                Description = "B",
                CreationDate = DateTime.UtcNow
            },
            new Matter 
            { 
                Id = Guid.NewGuid(), 
                IsArchived = false, 
                IsDeleted = true, 
                Description = "C",
                CreationDate = DateTime.UtcNow
            }
        };
            var context = CreateContextWithMatters(matters);
            var parameters = ValidParameters();
            parameters.IsArchived = false;
            parameters.IsDeleted = false;
            _validationServiceMock.Setup(v => v.ValidateNotNull(parameters, "resourceParameters"))
                .Returns(new OkResult() as ActionResult);
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.GetPaginatedMattersAsync(parameters);

            // Assert
            Assert.Single(result);
            Assert.All(result, m => Assert.False(m.IsArchived || m.IsDeleted));
        }

        /// <summary>
        /// Test: Returns all matters when IsArchived and IsDeleted are true.
        /// </summary>
        [Fact]
        public async Task ReturnsAll_WhenIsArchivedAndIsDeletedAreTrue()
        {
            // Arrange
            var matters = new[]
            {
            new Matter 
            { 
                Id = Guid.NewGuid(), 
                IsArchived = false, 
                IsDeleted = false, 
                Description = "A",
                CreationDate = DateTime.UtcNow
            },
            new Matter 
            { 
                Id = Guid.NewGuid(), 
                IsArchived = true, 
                IsDeleted = false, 
                Description = "B",
                CreationDate = DateTime.UtcNow
            },
            new Matter 
            { 
                Id = Guid.NewGuid(), 
                IsArchived = false, 
                IsDeleted = true, 
                Description = "C",
                CreationDate = DateTime.UtcNow
            }
        };
            var context = CreateContextWithMatters(matters);
            var parameters = ValidParameters();
            parameters.IsArchived = true;
            parameters.IsDeleted = true;
            _validationServiceMock.Setup(v => v.ValidateNotNull(parameters, "resourceParameters"))
                .Returns(new OkResult() as ActionResult);
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.GetPaginatedMattersAsync(parameters);

            // Assert
            Assert.Equal(3, result.Count);
        }

        /// <summary>
        /// Test: Returns paged results according to PageNumber and PageSize.
        /// </summary>
        [Fact]
        public async Task ReturnsPagedResults()
        {
            // Arrange
            var matters = Enumerable.Range(1, 25)
                .Select(i => new Matter 
                { 
                    Id = Guid.NewGuid(), 
                    IsArchived = false, 
                    IsDeleted = false, 
                    Description = $"Matter {i}",
                    CreationDate = DateTime.UtcNow.AddDays(-i) // Ensure different creation dates
                })
                .ToList();
            var context = CreateContextWithMatters(matters);
            var parameters = ValidParameters();
            parameters.PageNumber = 2;
            parameters.PageSize = 10;
            _validationServiceMock.Setup(v => v.ValidateNotNull(parameters, "resourceParameters"))
    .Returns(new OkResult() as ActionResult);
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.GetPaginatedMattersAsync(parameters);

            // Assert
            Assert.Equal(10, result.Count);
            Assert.Equal(2, result.CurrentPage);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(25, result.TotalCount);
        }

        /// <summary>
        /// Test: Returns sorted results when OrderBy is provided.
        /// </summary>
        [Fact]
        public async Task ReturnsSortedResults_WhenOrderByProvided()
        {
            // Arrange
            var matters = new[]
            {
            new Matter 
            { 
                Id = Guid.NewGuid(), 
                IsArchived = false, 
                IsDeleted = false, 
                Description = "Zebra", 
                CreationDate = DateTime.UtcNow 
            },
            new Matter 
            { 
                Id = Guid.NewGuid(), 
                IsArchived = false, 
                IsDeleted = false, 
                Description = "Alpha", 
                CreationDate = DateTime.UtcNow 
            }
        };
            var context = CreateContextWithMatters(matters);
            var parameters = ValidParameters();
            parameters.OrderBy = "Description";
            _validationServiceMock.Setup(v => v.ValidateNotNull(parameters, "resourceParameters"))
    .Returns(new OkResult() as ActionResult);
            _propertyMappingServiceMock.Setup(p => p.GetPropertyMapping<MatterDto, Matter>()).Returns(new Dictionary<string, PropertyMappingValue>
        {
            { "Description", new PropertyMappingValue(["Description"]) }
        });
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.GetPaginatedMattersAsync(parameters);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Alpha", result[0].Description);
            Assert.Equal("Zebra", result[1].Description);
        }

        /// <summary>
        /// Test: Returns empty paged list if an exception is thrown.
        /// </summary>
        [Fact]
        public async Task ReturnsEmptyList_WhenExceptionThrown()
        {
            // Arrange
            var contextMock = new Mock<AdmsContext>(new DbContextOptionsBuilder<AdmsContext>().Options);
            contextMock.Setup(c => c.Matters).Throws(new Exception("DB error"));
            var parameters = ValidParameters();
            _validationServiceMock.Setup(v => v.ValidateNotNull(parameters, "resourceParameters"))
    .Returns(new OkResult() as ActionResult);
            var repo = new AdmsRepository(_loggerMock.Object, contextMock.Object, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.GetPaginatedMattersAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            Assert.Equal(parameters.PageNumber, result.CurrentPage);
            Assert.Equal(parameters.PageSize, result.PageSize);
        }

        /// <summary>
        /// Test: Returns expected matters for a normal valid request.
        /// </summary>
        [Fact]
        public async Task ReturnsExpectedMatters_WhenValidRequest()
        {
            // Arrange
            var matters = new[]
            {
                new Matter
                {
                    Id = Guid.NewGuid(),
                    IsArchived = false,
                    IsDeleted = false,
                    Description = "A",
                    CreationDate = DateTime.UtcNow
                },
                new Matter
                {
                    Id = Guid.NewGuid(),
                    IsArchived = false,
                    IsDeleted = false,
                    Description = "B",
                    CreationDate = DateTime.UtcNow
                }
            };
            var context = CreateContextWithMatters(matters);
            var parameters = ValidParameters();
            _validationServiceMock.Setup(v => v.ValidateNotNull(parameters, "resourceParameters"))
    .Returns(new OkResult() as ActionResult);
            var repo = new AdmsRepository(_loggerMock.Object, context, _mapperMock.Object, _propertyMappingServiceMock.Object, _validationServiceMock.Object);

            // Act
            var result = await repo.GetPaginatedMattersAsync(parameters);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, m => m.Description == "A");
            Assert.Contains(result, m => m.Description == "B");
        }
    }
}