using System.Collections.Immutable;

namespace ADMS.API.Services;

/// <summary>
/// Defines the contract for advanced property validation services providing comprehensive type and property existence verification.
/// </summary>
/// <remarks>
/// This interface provides a comprehensive contract for property validation implementations including:
/// 
/// Core Property Validation Capabilities:
/// - Type-safe property existence validation using reflection
/// - High-performance property checking with intelligent caching
/// - Case-insensitive property name matching for flexible API usage
/// - Comprehensive error handling and detailed logging capabilities
/// - Thread-safe operations optimized for concurrent access scenarios
/// 
/// Property Validation Features:
/// - Support for simple property names (e.g., "Name", "Email", "CreatedDate")
/// - Complex nested property validation with dot notation (e.g., "Address.Street", "User.Profile.DisplayName")
/// - Comma-separated field list validation for batch property checking
/// - Whitespace-tolerant input parsing for robust API parameter handling
/// - Generic type parameter support for compile-time type safety
/// 
/// Performance and Scalability:
/// - Intelligent caching of reflection-based property information
/// - Concurrent dictionary implementation for thread-safe caching
/// - Optimized property lookup operations with O(1) average complexity
/// - Lazy evaluation of complex property paths for improved performance
/// - Memory-efficient storage of property metadata using immutable collections
/// 
/// Integration Features:
/// - Seamless integration with API parameter validation pipelines
/// - Support for data shaping operations with property existence verification
/// - Compatibility with Entity Framework Core entity validation
/// - Integration with model binding validation for comprehensive input checking
/// - Support for custom validation scenarios with extensible architecture
/// 
/// Thread Safety and Concurrency:
/// - All property validation operations are thread-safe for concurrent access
/// - Concurrent caching implementation prevents race conditions
/// - No shared mutable state that could cause threading issues
/// - Optimized for high-concurrency web application scenarios
/// - Lock-free operations for maximum performance under load
/// 
/// Error Handling and Logging:
/// - Comprehensive logging of property validation operations
/// - Detailed error reporting for troubleshooting validation failures
/// - Integration with structured logging frameworks for monitoring
/// - Performance metrics collection for optimization insights
/// - Graceful handling of reflection errors and edge cases
/// 
/// Common Implementation Patterns:
/// - Centralized property validation logic to eliminate code duplication
/// - Consistent validation behavior across all property checking operations
/// - Integration with dependency injection for service composition
/// - Configuration-driven validation rules for flexible business logic
/// - Comprehensive logging and monitoring for operational visibility
/// 
/// The interface is designed to support:
/// - High-performance property validation operations with minimal overhead
/// - Comprehensive error handling and diagnostic capabilities
/// - Extensible validation logic for evolving business requirements
/// - Integration with monitoring and observability frameworks
/// - Enterprise-grade scalability and reliability requirements
/// 
/// Usage Scenarios:
/// - API parameter validation for data shaping operations
/// - Dynamic property existence checking for flexible query building
/// - Model binding validation for complex object structures
/// - Entity Framework projection validation for database queries
/// - Generic property validation in data transformation pipelines
/// 
/// Implementation Considerations:
/// - Implementations should provide consistent validation behavior across all scenarios
/// - Proper integration with logging infrastructure for diagnostic capabilities
/// - Thread-safe operations for use in multi-threaded web applications
/// - Graceful handling of validation errors without service interruption
/// - Configuration support for validation rules and property filtering
/// 
/// Security Considerations:
/// - Input validation and sanitization for property name parameters
/// - Protection against reflection-based injection attacks
/// - Secure property access validation and filtering
/// - Integration with security policies and access control mechanisms
/// - Audit trail support for property validation operations
/// </remarks>
/// <example>
/// <code>
/// // Example implementation usage in a controller
/// [HttpGet]
/// public async Task&lt;IActionResult&gt; GetDocuments([FromQuery] string? fields)
/// {
///     // Validate that requested fields exist on the DTO
///     if (!string.IsNullOrEmpty(fields) && 
///         !_propertyChecker.TypeHasProperties&lt;DocumentDto&gt;(fields))
///     {
///         return BadRequest("One or more specified fields do not exist on the Document type");
///     }
///     
///     // Fields are valid - continue with data shaping
///     var documents = await _documentService.GetDocumentsAsync();
///     return Ok(documents.ShapeData(fields));
/// }
/// 
/// // Example service implementation
/// public class PropertyCheckerService : IPropertyCheckerService
/// {
///     public bool TypeHasProperties&lt;T&gt;(string? fields)
///     {
///         if (string.IsNullOrWhiteSpace(fields)) return true;
///         
///         var properties = GetCachedProperties&lt;T&gt;();
///         var fieldsArray = fields.Split(',', StringSplitOptions.RemoveEmptyEntries);
///         
///         return fieldsArray.All(field =&gt; 
///             properties.Any(p =&gt; string.Equals(p.Name, field.Trim(), StringComparison.OrdinalIgnoreCase)));
///     }
/// }
/// 
/// // Example integration with data shaping
/// public static class DataShapingExtensions
/// {
///     public static IEnumerable&lt;T&gt; ShapeData&lt;T&gt;(this IEnumerable&lt;T&gt; source, string? fields)
///     {
///         if (string.IsNullOrWhiteSpace(fields))
///             return source;
///             
///         // Property validation should be done before calling this method
///         // using IPropertyCheckerService.TypeHasProperties&lt;T&gt;(fields)
///         
///         return source.Select(item =&gt; ShapeDataForObject(item, fields));
///     }
/// }
/// </code>
/// </example>
public interface IPropertyCheckerService
{
    #region Core Property Validation

    /// <summary>
    /// Validates that all specified properties exist on the given type with comprehensive error handling and caching.
    /// </summary>
    /// <typeparam name="T">
    /// The type to validate properties against. Must be a reference type or value type with public properties.
    /// Common examples include DTO classes, entity classes, or any .NET type with public properties.
    /// The type must be accessible for reflection operations and property discovery.
    /// </typeparam>
    /// <param name="fields">
    /// A comma-separated string of property names to validate against the specified type.
    /// Supports simple property names (e.g., "Name,Email") and nested property paths (e.g., "Address.Street,User.Profile.Name").
    /// Null, empty, or whitespace-only values are considered valid (returns true) as no validation is required.
    /// Property names are matched case-insensitively for flexible API usage and improved user experience.
    /// Whitespace around property names is automatically trimmed for robust input handling.
    /// </param>
    /// <returns>
    /// true if all specified properties exist on the type T and are accessible for operations;
    /// false if any property does not exist, is not accessible, or if validation fails due to reflection errors.
    /// Returns true for null, empty, or whitespace-only field parameters as no validation is required.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive property validation including:
    /// 
    /// Input Processing and Parsing:
    /// - Handles null, empty, and whitespace-only field parameters gracefully
    /// - Parses comma-separated field lists with robust whitespace handling
    /// - Trims individual field names to handle formatting variations
    /// - Supports flexible input formats common in API parameter usage
    /// - Validates input format and structure before processing
    /// 
    /// Property Resolution and Discovery:
    /// - Uses reflection to discover public instance properties on the target type
    /// - Supports case-insensitive property name matching for API flexibility
    /// - Handles nested property paths with dot notation (e.g., "Address.Street")
    /// - Validates property accessibility and visibility requirements
    /// - Supports both simple and complex property hierarchies
    /// 
    /// Caching and Performance:
    /// - Implements intelligent caching of reflection-based property information
    /// - Uses concurrent dictionaries for thread-safe cache operations
    /// - Provides O(1) average case lookup time for cached property information
    /// - Optimizes memory usage with efficient caching strategies
    /// - Supports high-frequency validation scenarios with minimal overhead
    /// 
    /// Error Handling and Resilience:
    /// - Gracefully handles reflection errors and type resolution failures
    /// - Provides detailed logging for debugging and troubleshooting
    /// - Returns false for validation failures rather than throwing exceptions
    /// - Maintains service stability even with invalid or problematic input
    /// - Integrates with logging infrastructure for operational monitoring
    /// 
    /// Thread Safety and Concurrency:
    /// - All operations are thread-safe for concurrent access scenarios
    /// - Uses concurrent collections to prevent race conditions
    /// - No shared mutable state that could cause threading issues
    /// - Optimized for high-concurrency web application usage
    /// - Supports parallel validation operations without performance degradation
    /// 
    /// Security and Validation:
    /// - Input validation and sanitization for property name parameters
    /// - Protection against reflection-based attacks and injection attempts
    /// - Secure property access validation and filtering
    /// - Integration with security policies and access control
    /// - Audit trail support for property validation operations
    /// 
    /// Supported Field Specification Formats:
    /// - Simple property names: "Name,Email,CreatedDate"
    /// - Nested property paths: "Address.Street,User.Profile.DisplayName"
    /// - Mixed formats: "Name,Address.Street,Email"
    /// - Whitespace variations: " Name , Email , CreatedDate "
    /// - Case variations: "name,EMAIL,CreatedDate"
    /// 
    /// Common use cases include:
    /// - API parameter validation for data shaping operations
    /// - Dynamic property existence checking before LINQ expression building
    /// - Model binding validation for complex object structures
    /// - Entity Framework projection validation for database queries
    /// - Generic property validation in data transformation pipelines
    /// - Custom validation scenarios with property-based logic
    /// 
    /// Performance characteristics:
    /// - O(1) average case lookup for cached properties
    /// - O(n) worst case for uncached property discovery
    /// - Minimal memory allocation for validation operations
    /// - Efficient string parsing and property matching
    /// - Optimized for frequent validation scenarios
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// This method does not throw exceptions for validation failures. Instead, it returns false
    /// and logs appropriate error information. This design ensures service stability and
    /// provides predictable behavior for API validation scenarios.
    /// Implementations may throw ArgumentException for severely malformed input that indicates
    /// programming errors rather than user input validation failures.
    /// </exception>
    /// <example>
    /// <code>
    /// // Basic property validation
    /// bool hasValidProperties = _propertyChecker.TypeHasProperties&lt;DocumentDto&gt;("FileName,FileSize");
    /// 
    /// // Nested property validation
    /// bool hasNestedProperties = _propertyChecker.TypeHasProperties&lt;UserDto&gt;("Profile.DisplayName,Address.Street");
    /// 
    /// // Handle null/empty fields (returns true)
    /// bool emptyIsValid = _propertyChecker.TypeHasProperties&lt;DocumentDto&gt;(null);        // Returns true
    /// bool whitespaceIsValid = _propertyChecker.TypeHasProperties&lt;DocumentDto&gt;("  ");  // Returns true
    /// 
    /// // API parameter validation
    /// [HttpGet]
    /// public IActionResult GetDocuments([FromQuery] string? fields)
    /// {
    ///     if (!_propertyChecker.TypeHasProperties&lt;DocumentDto&gt;(fields))
    ///     {
    ///         return BadRequest("One or more specified fields do not exist on the Document type");
    ///     }
    ///     
    ///     // Continue with data shaping using validated fields
    ///     var documents = _repository.GetDocuments();
    ///     return Ok(documents.ShapeData(fields));
    /// }
    /// 
    /// // Dynamic query building with validation
    /// public IQueryable&lt;T&gt; BuildDynamicQuery&lt;T&gt;(IQueryable&lt;T&gt; query, string? selectFields)
    /// {
    ///     if (string.IsNullOrWhiteSpace(selectFields)) return query;
    ///     
    ///     if (!_propertyChecker.TypeHasProperties&lt;T&gt;(selectFields))
    ///     {
    ///         throw new ArgumentException("Invalid fields specified for selection");
    ///     }
    ///     
    ///     // Safe to build LINQ expression with validated fields
    ///     return query.Select(BuildSelectExpression&lt;T&gt;(selectFields));
    /// }
    /// 
    /// // Batch validation for multiple types
    /// public Dictionary&lt;Type, bool&gt; ValidateFieldsForTypes(string fields, params Type[] types)
    /// {
    ///     var results = new Dictionary&lt;Type, bool&gt;();
    ///     
    ///     foreach (var type in types)
    ///     {
    ///         var method = typeof(IPropertyCheckerService).GetMethod(nameof(TypeHasProperties))
    ///             ?.MakeGenericMethod(type);
    ///         var result = (bool)(method?.Invoke(this, new object[] { fields }) ?? false);
    ///         results[type] = result;
    ///     }
    ///     
    ///     return results;
    /// }
    /// </code>
    /// </example>
    bool TypeHasProperties<T>(string? fields);

    #endregion Core Property Validation

    #region Single Property Validation

    /// <summary>
    /// Validates that a specific property exists on the given type with detailed validation.
    /// </summary>
    /// <typeparam name="T">The type to validate the property against.</typeparam>
    /// <param name="propertyName">
    /// The name of the property to validate (case-insensitive).
    /// Supports simple property names and nested property paths with dot notation.
    /// Cannot be null, empty, or whitespace-only.
    /// </param>
    /// <returns>
    /// true if the property exists and is accessible; 
    /// false if the property does not exist or is not accessible.
    /// </returns>
    /// <remarks>
    /// This method provides:
    /// - Single property validation for specific use cases
    /// - Case-insensitive property name matching for flexibility
    /// - Support for nested property paths with dot notation
    /// - Caching for improved performance on repeated calls
    /// - Detailed logging for debugging and troubleshooting
    /// 
    /// The method is optimized for scenarios where:
    /// - Only a single property needs validation
    /// - Performance is critical and batch validation is not needed
    /// - Integration with existing validation pipelines is required
    /// - Detailed per-property validation results are needed
    /// 
    /// Performance characteristics:
    /// - O(1) average case for cached property information
    /// - Minimal memory allocation for single property checks
    /// - Thread-safe operation for concurrent access
    /// - Optimized for high-frequency single property validation
    /// </remarks>
    /// <example>
    /// <code>
    /// // Validate single property
    /// bool hasFileName = _propertyChecker.HasProperty&lt;DocumentDto&gt;("FileName");
    /// 
    /// // Validate nested property
    /// bool hasStreet = _propertyChecker.HasProperty&lt;UserDto&gt;("Address.Street");
    /// 
    /// // Use in conditional logic
    /// if (_propertyChecker.HasProperty&lt;DocumentDto&gt;("LastModifiedDate"))
    /// {
    ///     // Safe to use LastModifiedDate property
    ///     query = query.OrderBy(d =&gt; d.LastModifiedDate);
    /// }
    /// </code>
    /// </example>
    bool HasProperty<T>(string propertyName);

    #endregion Single Property Validation

    #region Property Discovery and Enumeration

    /// <summary>
    /// Gets all public properties available on the specified type for validation and enumeration purposes.
    /// </summary>
    /// <typeparam name="T">The type to retrieve properties for.</typeparam>
    /// <returns>
    /// An enumerable collection of property names available on the specified type.
    /// The collection includes all public instance properties that can be accessed.
    /// </returns>
    /// <remarks>
    /// This method provides:
    /// - Complete enumeration of available properties for validation
    /// - Cached property information for optimal performance
    /// - Integration support for dynamic property discovery
    /// - Debugging and diagnostic capabilities for type inspection
    /// 
    /// The returned properties include:
    /// - All public instance properties with get accessors
    /// - Properties inherited from base classes
    /// - Properties defined by implemented interfaces
    /// - Auto-implemented properties and traditional properties
    /// 
    /// Use cases include:
    /// - Generating documentation for available data shaping fields
    /// - Creating dynamic user interfaces for property selection
    /// - Validating comprehensive property coverage in tests
    /// - Debugging type structure and property availability
    /// - API documentation generation for supported fields
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get all available properties
    /// var availableProperties = _propertyChecker.GetAvailableProperties&lt;DocumentDto&gt;().ToList();
    /// 
    /// // Use for documentation
    /// var helpText = $"Available fields: {string.Join(", ", availableProperties)}";
    /// 
    /// // Use for validation
    /// var invalidFields = requestedFields.Except(availableProperties).ToList();
    /// if (invalidFields.Any())
    /// {
    ///     return BadRequest($"Invalid fields: {string.Join(", ", invalidFields)}");
    /// }
    /// </code>
    /// </example>
    IEnumerable<string> GetAvailableProperties<T>();

    #endregion Property Discovery and Enumeration

    #region Diagnostic and Management Operations

    /// <summary>
    /// Gets comprehensive diagnostic information about the property checker service for monitoring and debugging.
    /// </summary>
    /// <returns>
    /// A dictionary containing diagnostic information about service performance and cache utilization including:
    /// - Service configuration and operational status
    /// - Cache utilization statistics and performance metrics
    /// - Memory usage information for optimization purposes
    /// - Service health indicators for operational monitoring
    /// - Performance metrics for cache effectiveness analysis
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive diagnostic capabilities including:
    /// 
    /// Service Configuration Information:
    /// - Property cache entries count and utilization statistics
    /// - Validation cache entries and hit rate analysis
    /// - Service initialization status and configuration health
    /// - Supported type information and coverage analysis
    /// - Error rates and failure patterns for reliability assessment
    /// 
    /// Performance Metrics:
    /// - Cache hit rates and miss statistics for optimization
    /// - Average response times for property validation operations
    /// - Memory usage patterns and optimization opportunities
    /// - Request volume and throughput analysis
    /// - Resource utilization metrics for capacity planning
    /// 
    /// Operational Monitoring Data:
    /// - Recent validation patterns and frequency analysis
    /// - Error tracking and diagnostic information
    /// - Resource consumption and performance characteristics
    /// - Integration health with dependent services
    /// - System resource utilization and recommendations
    /// 
    /// The diagnostic information is useful for:
    /// - Performance optimization and capacity planning
    /// - Memory usage monitoring in long-running applications
    /// - Service health checking and alerting systems
    /// - Debugging cache effectiveness and hit rates
    /// - Operational monitoring and service level compliance
    /// </remarks>
    /// <example>
    /// <code>
    /// var diagnostics = _propertyChecker.GetDiagnosticInfo();
    /// 
    /// Console.WriteLine($"Property Cache Entries: {diagnostics["PropertyCacheCount"]}");
    /// Console.WriteLine($"Validation Cache Entries: {diagnostics["ValidationCacheCount"]}");
    /// Console.WriteLine($"Service Status: {diagnostics["ServiceStatus"]}");
    /// 
    /// // Use for monitoring
    /// var logger = serviceProvider.GetRequiredService&lt;ILogger&gt;();
    /// logger.LogInformation("PropertyChecker diagnostics: {@Diagnostics}", diagnostics);
    /// </code>
    /// </example>
    Dictionary<string, object> GetDiagnosticInfo();

    /// <summary>
    /// Clears all cached property information and validation results to free memory.
    /// </summary>
    /// <remarks>
    /// This method is useful for:
    /// - Memory optimization in long-running applications
    /// - Testing scenarios where fresh property resolution is required
    /// - Dynamic type scenarios where cached information may become stale
    /// - Performance testing and benchmarking scenarios
    /// 
    /// Important considerations:
    /// - Clearing the cache will cause temporary performance impact
    /// - Property information is re-resolved on subsequent validation requests
    /// - Thread-safe operation suitable for runtime execution
    /// - Logged operation for audit trail and monitoring purposes
    /// 
    /// The method provides:
    /// - Complete cache invalidation for fresh property discovery
    /// - Memory optimization for long-running applications
    /// - Testing support for clean state scenarios
    /// - Operational maintenance capabilities
    /// </remarks>
    /// <example>
    /// <code>
    /// // Clear cache for memory optimization
    /// _propertyChecker.ClearCache();
    /// 
    /// // Use in testing for fresh state
    /// [Test]
    /// public void TestPropertyValidation()
    /// {
    ///     _propertyChecker.ClearCache(); // Ensure fresh state
    ///     var result = _propertyChecker.TypeHasProperties&lt;TestDto&gt;("TestProperty");
    ///     Assert.IsTrue(result);
    /// }
    /// </code>
    /// </example>
    void ClearCache();

    #endregion Diagnostic and Management Operations

    #region Cache and Performance Information

    /// <summary>
    /// Gets the collection of cached type information for performance monitoring and diagnostics.
    /// </summary>
    /// <value>
    /// An immutable collection of cached type information entries that provides:
    /// - Read-only access to cached type information for diagnostic purposes
    /// - Performance monitoring capabilities for cache effectiveness analysis
    /// - Memory usage insights for optimization and capacity planning
    /// - Debugging support for property validation troubleshooting
    /// </value>
    /// <remarks>
    /// This property provides access to:
    /// 
    /// Cached Information Details:
    /// - Type names and their associated property collections
    /// - Property metadata for fast lookup operations
    /// - Reflection information cached for performance optimization
    /// - Validation results for frequently accessed property combinations
    /// 
    /// Performance Monitoring:
    /// - Cache effectiveness and utilization patterns
    /// - Memory usage tracking for optimization
    /// - Property access frequency analysis
    /// - Cache hit/miss ratio calculations
    /// 
    /// Diagnostic Capabilities:
    /// - Property validation troubleshooting support
    /// - Service health monitoring and reporting
    /// - Configuration validation and verification
    /// - Development and debugging assistance
    /// 
    /// Use cases:
    /// - Performance monitoring and optimization analysis
    /// - Memory usage tracking for long-running applications
    /// - Debugging property validation issues and cache effectiveness
    /// - Service health monitoring and diagnostic reporting
    /// - Capacity planning and optimization insights
    /// </remarks>
    /// <example>
    /// <code>
    /// // Monitor cache utilization
    /// var cachedTypes = _propertyChecker.CachedTypeInformation;
    /// _logger.LogInformation("Cached {Count} types with property information", cachedTypes.Count);
    /// 
    /// // Analyze cache effectiveness
    /// foreach (var kvp in cachedTypes)
    /// {
    ///     _logger.LogDebug("Type {TypeName} has {PropertyCount} cached properties",
    ///         kvp.Key, kvp.Value.Length);
    /// }
    /// </code>
    /// </example>
    ImmutableDictionary<string, System.Reflection.PropertyInfo[]> CachedTypeInformation { get; }

    /// <summary>
    /// Gets the count of cached validation results for performance monitoring.
    /// </summary>
    /// <value>
    /// The total number of cached validation results for performance and utilization analysis.
    /// Always non-negative and reflects current cache utilization patterns.
    /// </value>
    /// <remarks>
    /// This property provides:
    /// - Quick access to cache utilization metrics
    /// - Performance monitoring for cache effectiveness
    /// - Memory usage insights for optimization purposes
    /// - Service health indicators for diagnostic scenarios
    /// 
    /// The cache count reflects:
    /// - Number of unique type-field combinations validated
    /// - Cache effectiveness and hit rates over time
    /// - Memory utilization patterns for capacity planning
    /// - Service usage patterns for optimization insights
    /// 
    /// Monitoring applications:
    /// - Performance dashboard integration
    /// - Memory usage tracking and alerting
    /// - Cache optimization and tuning
    /// - Service health monitoring and reporting
    /// </remarks>
    /// <example>
    /// <code>
    /// // Monitor cache growth
    /// var cacheCount = _propertyChecker.CachedValidationCount;
    /// if (cacheCount > 10000)
    /// {
    ///     _logger.LogWarning("Property validation cache has grown to {Count} entries", cacheCount);
    ///     // Consider clearing cache or implementing cache size limits
    /// }
    /// 
    /// // Performance monitoring
    /// _performanceLogger.LogMetric("PropertyChecker.CacheSize", cacheCount);
    /// </code>
    /// </example>
    int CachedValidationCount { get; }

    #endregion Cache and Performance Information
}