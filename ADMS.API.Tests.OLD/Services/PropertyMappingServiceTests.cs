using ADMS.API.Entities;
using ADMS.API.Models;
using ADMS.API.Services;

using Microsoft.Extensions.Logging;

namespace ADMS.API.Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="PropertyMappingService"/>.
    /// </summary>
    public class PropertyMappingServiceTests
    {
        private readonly PropertyMappingService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMappingServiceTests"/> class.
        /// </summary>
        /// <remarks>This constructor sets up the test class by creating a logger instance and
        /// initializing the  <see cref="PropertyMappingService"/> with it. This ensures that the service is properly
        /// configured  for testing.</remarks>
        public PropertyMappingServiceTests()
        {
            var logger = new LoggerFactory().CreateLogger<PropertyMappingService>();
            _service = new PropertyMappingService(logger);
        }

        #region GetPropertyMapping

        /// <summary>
        /// Tests that GetPropertyMapping returns a valid mapping dictionary for a known DTO/entity pair.
        /// </summary>
        [Fact]
        public void GetPropertyMapping_ReturnsMapping_ForKnownTypes()
        {
            // Act
            var mapping = _service.GetPropertyMapping<DocumentDto, Document>();

            // Assert
            Assert.NotNull(mapping);
            Assert.True(mapping.ContainsKey("Id"));
            Assert.True(mapping.ContainsKey("FileName"));
            Assert.True(mapping.ContainsKey("CreatedAt"));
        }

        /// <summary>
        /// Tests that GetPropertyMapping throws InvalidOperationException for unknown type pairs.
        /// </summary>
        [Fact]
        public void GetPropertyMapping_Throws_ForUnknownTypes()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _service.GetPropertyMapping<FakeDto, FakeEntity>());
        }

        #endregion

        #region ValidMappingExistsFor

        /// <summary>
        /// Tests that ValidMappingExistsFor returns true for empty or whitespace fields.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidMappingExistsFor_ReturnsTrue_ForEmptyOrWhitespaceFields(string? fields)
        {
            var result = _service.ValidMappingExistsFor<DocumentDto, Document>(fields!);
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ValidMappingExistsFor returns true for a single valid field.
        /// </summary>
        [Fact]
        public void ValidMappingExistsFor_ReturnsTrue_ForSingleValidField()
        {
            var result = _service.ValidMappingExistsFor<DocumentDto, Document>("FileName");
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ValidMappingExistsFor returns true for multiple valid fields.
        /// </summary>
        [Fact]
        public void ValidMappingExistsFor_ReturnsTrue_ForMultipleValidFields()
        {
            var result = _service.ValidMappingExistsFor<DocumentDto, Document>("FileName,CreatedAt,Id");
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ValidMappingExistsFor returns false if any field is invalid.
        /// </summary>
        [Fact]
        public void ValidMappingExistsFor_ReturnsFalse_IfAnyFieldIsInvalid()
        {
            var result = _service.ValidMappingExistsFor<DocumentDto, Document>("FileName,NotAProperty");
            Assert.False(result);
        }

        /// <summary>
        /// Tests that ValidMappingExistsFor ignores sort direction keywords (asc/desc) and validates property names only.
        /// </summary>
        [Fact]
        public void ValidMappingExistsFor_IgnoresSortDirection()
        {
            var result = _service.ValidMappingExistsFor<DocumentDto, Document>("FileName desc, CreatedAt asc");
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ValidMappingExistsFor throws InvalidOperationException for unknown type pairs.
        /// </summary>
        [Fact]
        public void ValidMappingExistsFor_Throws_ForUnknownTypes()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _service.ValidMappingExistsFor<FakeDto, FakeEntity>("SomeField"));
        }

        /// <summary>
        /// Dummy DTO for negative test cases.
        /// </summary>
        private class FakeDto(string? SomeField)
        {
            public string? SomeField { get; } = SomeField;
        }

        /// <summary>
        /// Dummy entity for negative test cases.
        /// </summary>
        private class FakeEntity(string? SomeField)
        {
            public string? SomeField { get; } = SomeField;
        }

        #endregion
    }
}