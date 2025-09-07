using System.Collections.Immutable;

namespace ADMS.API.Services;

/// <summary>
/// Defines the contract for enterprise-grade property mapping services providing comprehensive mapping between DTO and Entity properties.
/// </summary>
/// <remarks>
/// This interface provides a comprehensive contract for property mapping implementations including:
/// 
/// Core Property Mapping Capabilities:
/// - Automated reflection-based property mapping for matching property names
/// - Custom property mapping overrides for complex transformation scenarios
/// - Thread-safe mapping operations with concurrent access support
/// - Performance-optimized mapping with caching and lazy initialization
/// - Comprehensive validation and error handling for mapping operations
/// 
/// Advanced Mapping Features:
/// - Support for complex property mappings including nested properties and collections
/// - Simple one-to-one property mappings (DTO.Name -> Entity.Name)
/// - Complex one-to-many property mappings (DTO.FullName -> Entity.FirstName, Entity.LastName)
/// - Bidirectional property mappings with revert support for inverse operations
/// - Nested property mappings with dot notation (DTO.Address -> Entity.Address.Street)
/// 
/// Integration and Framework Support:
/// - Seamless integration with Entity Framework Core for database operations
/// - Support for LINQ expression building for dynamic sorting and filtering
/// - Compatibility with AutoMapper and other mapping frameworks for complex scenarios
/// - Integration with API versioning for evolving mapping requirements
/// - Support for dependency injection and service lifetime management
/// 
/// Performance and Scalability:
/// - Concurrent dictionary caching for frequently accessed mappings
/// - Lazy initialization of mapping configurations to reduce startup time
/// - Optimized reflection operations with cached property information
/// - Thread-safe operations supporting high-concurrency scenarios
/// - Memory-efficient storage of mapping configurations using immutable collections
/// 
/// Validation and Error Handling:
/// - Comprehensive validation of mapping configurations during registration
/// - Runtime validation of field mappings for sorting and filtering operations
/// - Detailed error messages for troubleshooting mapping issues
/// - Graceful handling of missing or invalid property mappings
/// - Integration with logging infrastructure for debugging and monitoring
/// 
/// Security and Data Protection:
/// - Input validation and sanitization for property mapping operations
/// - Protection against property injection and mapping manipulation attacks
/// - Secure property name validation and filtering
/// - Integration with security policies and access control mechanisms
/// - Audit trail support for mapping operations and configuration changes
/// 
/// Thread Safety and Concurrency:
/// - All mapping operations are thread-safe for concurrent access
/// - Immutable mapping configurations prevent accidental modification
/// - Concurrent dictionary implementation for high-performance caching
/// - No shared mutable state that could cause race conditions
/// - Safe for use in multi-threaded web application scenarios
/// 
/// Common Implementation Patterns:
/// - Centralized property mapping logic to eliminate code duplication
/// - Consistent mapping behavior across all DTO-Entity transformations
/// - Integration with dependency injection for service composition
/// - Configuration-driven mapping rules for flexible business logic
/// - Comprehensive logging and monitoring for operational visibility
/// 
/// The interface is designed to support:
/// - High-performance property mapping operations with minimal overhead
/// - Comprehensive error handling and diagnostic capabilities
/// - Extensible mapping logic for evolving business requirements
/// - Integration with monitoring and observability frameworks
/// - Enterprise-grade scalability and reliability requirements
/// 
/// Usage Scenarios:
/// - API request/response transformation between DTOs and domain entities
/// - Database query result mapping from entities to presentation models
/// - Dynamic sorting and filtering operations with property name translation
/// - Data validation and transformation pipelines with type safety
/// - Cross-layer data transformation in layered architecture patterns
/// 
/// Implementation Considerations:
/// - Implementations should provide consistent mapping behavior across all scenarios
/// - Proper integration with logging infrastructure for diagnostic capabilities
/// - Thread-safe operations for use in multi-threaded web applications
/// - Graceful handling of mapping errors without service interruption
/// - Configuration support for mapping rules and business logic customization
/// </remarks>
/// <example>
/// <code>
/// // Example implementation usage in a service
/// public class DocumentService
/// {
///     private readonly IPropertyMappingService _mappingService;
///     
///     public DocumentService(IPropertyMappingService mappingService)
///     {
///         _mappingService = mappingService;
///     }
///     
///     public async Task&lt;PagedList&lt;DocumentDto&gt;&gt; GetDocumentsAsync(
///         DocumentParameters parameters)
///     {
///         // Validate sort fields have valid mappings
///         if (!_mappingService.ValidMappingExistsFor&lt;DocumentDto, Document&gt;(parameters.SortBy))
///         {
///             throw new ArgumentException("Invalid sort fields specified");
///         }
///         
///         // Get mapping for building LINQ expressions
///         var mapping = _mappingService.GetPropertyMapping&lt;DocumentDto, Document&gt;();
///         
///         // Build dynamic LINQ expression using property mapping
///         var query = BuildSortedQuery(baseQuery, parameters.SortBy, mapping);
///         
///         return await PagedList&lt;DocumentDto&gt;.CreateAsync(query, parameters.PageNumber, parameters.PageSize);
///     }
/// }
/// 
/// // Example controller usage
/// [HttpGet]
/// public async Task&lt;IActionResult&gt; GetDocuments(
///     [FromQuery] string? sortBy,
///     [FromQuery] string? fields)
/// {
///     // Validate sort parameters
///     if (!string.IsNullOrEmpty(sortBy) && 
///         !_mappingService.ValidMappingExistsFor&lt;DocumentDto, Document&gt;(sortBy))
///     {
///         return BadRequest("Invalid sort fields specified");
///     }
///     
///     // Validate field selection parameters
///     if (!string.IsNullOrEmpty(fields) &&
///         !_propertyCheckerService.TypeHasProperties&lt;DocumentDto&gt;(fields))
///     {
///         return BadRequest("Invalid field selection specified");
///     }
///     
///     var documents = await _documentService.GetDocumentsAsync(sortBy, fields);
///     return Ok(documents);
/// }
/// </code>
/// </example>
public interface IPropertyMappingService
{
    #region Property Mapping Retrieval

    /// <summary>
    /// Retrieves the comprehensive property mapping dictionary for the specified source and destination types.
    /// </summary>
    /// <typeparam name="TSource">
    /// The source DTO type for the mapping lookup.
    /// Must be a type with public properties that can be mapped to the destination type.
    /// Common examples include DocumentDto, MatterDto, RevisionDto, and other API data transfer objects.
    /// </typeparam>
    /// <typeparam name="TDestination">
    /// The destination entity type for the mapping lookup.
    /// Must be a type with public properties that can receive mapped values from the source type.
    /// Common examples include Document, Matter, Revision, and other domain entities.
    /// </typeparam>
    /// <returns>
    /// A dictionary mapping source property names (keys) to PropertyMappingValue instances (values).
    /// The dictionary provides complete mapping configuration for transforming between the specified types.
    /// Keys are case-insensitive source property names from the TSource type.
    /// Values contain destination property information, bidirectional flags, and mapping metadata.
    /// </returns>
    /// <remarks>
    /// This method provides high-performance property mapping retrieval with:
    /// 
    /// Caching and Performance Features:
    /// - Implements intelligent caching to avoid repeated lookup operations
    /// - Uses concurrent dictionary for thread-safe caching in multi-threaded scenarios
    /// - Optimizes frequently accessed mappings for improved response times
    /// - Provides consistent performance characteristics under varying loads
    /// - Memory-efficient caching with automatic cleanup and optimization
    /// 
    /// Mapping Resolution Process:
    /// 1. Generates a unique cache key based on source and destination type information
    /// 2. Checks the mapping cache for previously resolved mapping configurations
    /// 3. If cached mapping exists, returns the cached result for optimal performance
    /// 4. If no cached mapping exists, performs mapping resolution from registered mappings
    /// 5. Caches the resolved mapping for future requests to improve performance
    /// 6. Validates mapping completeness and consistency before returning
    /// 
    /// Error Handling and Validation:
    /// - Validates that exactly one matching mapping exists for the specified types
    /// - Provides detailed error messages when mappings are missing or ambiguous
    /// - Handles edge cases where multiple mappings might match the same type pair
    /// - Logs mapping resolution activities for debugging and monitoring purposes
    /// - Integrates with diagnostic logging for operational visibility
    /// 
    /// Thread Safety and Concurrency:
    /// - All mapping resolution operations are thread-safe for concurrent access
    /// - Uses concurrent collections to prevent race conditions during caching
    /// - Supports high-concurrency scenarios without performance degradation
    /// - No shared mutable state that could cause threading issues
    /// - Optimized for multi-threaded web application usage patterns
    /// 
    /// Mapping Dictionary Characteristics:
    /// - Case-insensitive string keys for flexible property name matching
    /// - PropertyMappingValue objects containing destination property information
    /// - Support for one-to-one and one-to-many property mapping scenarios
    /// - Bidirectional mapping support with revert flag configuration
    /// - Immutable mapping configurations for thread safety and consistency
    /// 
    /// The returned mapping dictionary is used extensively for:
    /// - Building dynamic LINQ expressions for database queries with property translation
    /// - Translating API parameter names to entity property names
    /// - Constructing sort and filter expressions for paginated operations
    /// - Validating that requested sort/filter fields have valid mappings
    /// - Data transformation operations between different object layers
    /// 
    /// Common usage patterns include:
    /// - Direct property lookup: mapping["PropertyName"] for specific property access
    /// - Enumeration: foreach(var kvp in mapping) for complete mapping iteration
    /// - Existence checking: mapping.ContainsKey("PropertyName") for validation
    /// - LINQ operations: mapping.Where(kvp => condition) for filtering scenarios
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no mapping is found for the specified type pair, multiple ambiguous mappings exist,
    /// or mapping resolution fails due to configuration errors or infrastructure issues.
    /// The exception includes detailed information about the failure reason and context.
    /// </exception>
    /// <example>
    /// <code>
    /// // Get mapping for DocumentDto to Document entity transformation
    /// var mapping = _mappingService.GetPropertyMapping&lt;DocumentDto, Document&gt;();
    /// 
    /// // Use mapping for property existence validation
    /// bool canSortByFileName = mapping.ContainsKey("FileName");
    /// bool canSortByCreatedDate = mapping.ContainsKey("CreatedDate");
    /// 
    /// // Get specific property mapping for transformation
    /// if (mapping.TryGetValue("FileName", out var fileNameMapping))
    /// {
    ///     string destinationProperty = fileNameMapping.GetPrimaryProperty(); // "FileName"
    ///     bool shouldReverse = fileNameMapping.Revert; // false for normal mapping
    ///     var destinationProperties = fileNameMapping.DestinationProperties; // ["FileName"]
    /// }
    /// 
    /// // Build LINQ expression using property mapping
    /// public IQueryable&lt;Document&gt; ApplySorting(IQueryable&lt;Document&gt; query, string sortBy)
    /// {
    ///     var mapping = _mappingService.GetPropertyMapping&lt;DocumentDto, Document&gt;();
    ///     
    ///     if (mapping.TryGetValue(sortBy, out var mappingValue))
    ///     {
    ///         var propertyName = mappingValue.GetPrimaryProperty();
    ///         return mappingValue.Revert 
    ///             ? query.OrderByDescending(GetPropertyExpression&lt;Document&gt;(propertyName))
    ///             : query.OrderBy(GetPropertyExpression&lt;Document&gt;(propertyName));
    ///     }
    ///     
    ///     return query; // Return original query if no mapping found
    /// }
    /// </code>
    /// </example>
    Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();

    #endregion Property Mapping Retrieval

    #region Property Mapping Validation

    /// <summary>
    /// Validates that all specified fields have corresponding property mappings between the source and destination types.
    /// </summary>
    /// <typeparam name="TSource">
    /// The source DTO type for mapping validation.
    /// Must be a type with public properties that can be validated against the mapping configuration.
    /// Common examples include DocumentDto, MatterDto, RevisionDto, and other API data transfer objects.
    /// </typeparam>
    /// <typeparam name="TDestination">
    /// The destination entity type for mapping validation.
    /// Must be a type with public properties that serve as mapping destinations.
    /// Common examples include Document, Matter, Revision, and other domain entities.
    /// </typeparam>
    /// <param name="fields">
    /// A comma-separated string of field names to validate against the property mapping configuration.
    /// Field names may include sort direction indicators (ascending/descending) that are automatically parsed and removed.
    /// Null, empty, or whitespace-only values are considered valid (returns true) as no validation is required.
    /// Field names are matched case-insensitively for flexible API parameter handling.
    /// </param>
    /// <returns>
    /// true if all specified fields have valid mappings in the property mapping configuration;
    /// false if any field lacks a corresponding mapping or if validation fails due to configuration errors.
    /// Returns true for null, empty, or whitespace-only field parameters as no validation is required.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive field validation including:
    /// 
    /// Field Parsing and Processing:
    /// - Parses comma-separated field lists with robust whitespace handling
    /// - Automatically removes sort direction indicators (e.g., "name asc", "date desc", "field ascending")
    /// - Handles empty or whitespace-only field specifications gracefully
    /// - Supports flexible field name formats commonly used in API sorting and filtering
    /// - Trims individual field names to handle various formatting variations
    /// 
    /// Validation Process and Logic:
    /// 1. Handles null/empty field specifications by returning true (no validation needed)
    /// 2. Retrieves the appropriate property mapping for the specified type pair
    /// 3. Parses the fields string into individual field names with direction cleanup
    /// 4. Validates each field name against the available property mappings
    /// 5. Returns true only if all fields have valid corresponding mappings
    /// 6. Logs validation results for debugging and monitoring purposes
    /// 
    /// Performance Characteristics:
    /// - Leverages cached property mappings for optimal validation performance
    /// - Efficient string parsing with minimal memory allocation overhead
    /// - Short-circuits validation on first invalid field for improved performance
    /// - Scales well with large field lists and complex property mapping configurations
    /// - Thread-safe operations suitable for concurrent validation scenarios
    /// 
    /// Error Handling and Resilience:
    /// - Gracefully handles missing or invalid property mappings without exceptions
    /// - Returns false for any validation failures rather than throwing exceptions
    /// - Logs detailed information about validation failures for debugging support
    /// - Provides robust behavior even with malformed field specifications
    /// - Integrates with logging infrastructure for operational monitoring
    /// 
    /// Common Use Cases:
    /// - Validating API sort parameters before building database queries
    /// - Checking filter field specifications in search operations
    /// - Ensuring data shaping field requests are mappable to entity properties
    /// - Pre-validation of user input for property-based operations
    /// - Integration with API parameter validation middleware and filters
    /// 
    /// Supported Field Specification Formats:
    /// - Simple field names: "name,email,createdDate"
    /// - Fields with sort directions: "name asc,email desc,createdDate ascending"
    /// - Mixed formats: "name asc,email,createdDate desc"
    /// - Whitespace variations: " name , email , createdDate "
    /// - Case variations: "Name ASC,EMAIL desc,CreatedDate"
    /// 
    /// Integration Benefits:
    /// - Seamless integration with ASP.NET Core model binding and validation
    /// - Support for dynamic API parameter validation in controller actions
    /// - Integration with custom validation attributes and middleware
    /// - Compatible with API versioning and backward compatibility requirements
    /// - Support for automated API documentation and OpenAPI specification generation
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when property mapping retrieval fails due to configuration errors.
    /// This exception is propagated from the GetPropertyMapping method and indicates
    /// a serious configuration issue that should be addressed during development.
    /// The exception includes detailed context about the mapping configuration failure.
    /// </exception>
    /// <example>
    /// <code>
    /// // Simple field list validation
    /// bool isValidSimple = _mappingService.ValidMappingExistsFor&lt;DocumentDto, Document&gt;("fileName,fileSize");
    /// 
    /// // Fields with sort directions validation
    /// bool isValidWithSort = _mappingService.ValidMappingExistsFor&lt;DocumentDto, Document&gt;(
    ///     "fileName asc,createdDate desc,fileSize ascending");
    /// 
    /// // Empty/null field validation (returns true)
    /// bool emptyIsValid = _mappingService.ValidMappingExistsFor&lt;DocumentDto, Document&gt;("");
    /// bool nullIsValid = _mappingService.ValidMappingExistsFor&lt;DocumentDto, Document&gt;(null);
    /// 
    /// // API controller integration
    /// [HttpGet]
    /// public async Task&lt;IActionResult&gt; GetDocuments(
    ///     [FromQuery] string? sortBy,
    ///     [FromQuery] string? orderBy)
    /// {
    ///     // Validate sort parameters before processing
    ///     if (!string.IsNullOrEmpty(sortBy) && 
    ///         !_mappingService.ValidMappingExistsFor&lt;DocumentDto, Document&gt;(sortBy))
    ///     {
    ///         return BadRequest($"Invalid sort fields specified: {sortBy}");
    ///     }
    ///     
    ///     if (!string.IsNullOrEmpty(orderBy) &&
    ///         !_mappingService.ValidMappingExistsFor&lt;DocumentDto, Document&gt;(orderBy))
    ///     {
    ///         return BadRequest($"Invalid order fields specified: {orderBy}");
    ///     }
    ///     
    ///     // All validation passed - continue with query building
    ///     var documents = await _documentService.GetDocumentsAsync(sortBy, orderBy);
    ///     return Ok(documents);
    /// }
    /// 
    /// // Batch validation for multiple field sets
    /// public Dictionary&lt;string, bool&gt; ValidateMultipleFieldSets(Dictionary&lt;string, string&gt; fieldSets)
    /// {
    ///     var validationResults = new Dictionary&lt;string, bool&gt;();
    ///     
    ///     foreach (var kvp in fieldSets)
    ///     {
    ///         var setName = kvp.Key;
    ///         var fields = kvp.Value;
    ///         
    ///         bool isValid = _mappingService.ValidMappingExistsFor&lt;DocumentDto, Document&gt;(fields);
    ///         validationResults[setName] = isValid;
    ///         
    ///         if (!isValid)
    ///         {
    ///             _logger.LogWarning("Invalid field set '{SetName}': {Fields}", setName, fields);
    ///         }
    ///     }
    ///     
    ///     return validationResults;
    /// }
    /// 
    /// // Custom validation attribute example
    /// public class ValidMappingFieldsAttribute : ValidationAttribute
    /// {
    ///     private readonly Type _sourceType;
    ///     private readonly Type _destinationType;
    ///     
    ///     public ValidMappingFieldsAttribute(Type sourceType, Type destinationType)
    ///     {
    ///         _sourceType = sourceType;
    ///         _destinationType = destinationType;
    ///     }
    ///     
    ///     protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    ///     {
    ///         if (value is not string fields || string.IsNullOrWhiteSpace(fields))
    ///             return ValidationResult.Success;
    ///             
    ///         var mappingService = validationContext.GetService&lt;IPropertyMappingService&gt;();
    ///         var method = typeof(IPropertyMappingService).GetMethod(nameof(ValidMappingExistsFor))
    ///             ?.MakeGenericMethod(_sourceType, _destinationType);
    ///             
    ///         var result = (bool)method?.Invoke(mappingService, new object[] { fields });
    ///         
    ///         return result 
    ///             ? ValidationResult.Success 
    ///             : new ValidationResult($"Invalid mapping fields: {fields}");
    ///     }
    /// }
    /// 
    /// // Usage: [ValidMappingFields(typeof(DocumentDto), typeof(Document))]
    /// public string SortBy { get; set; }
    /// </code>
    /// </example>
    bool ValidMappingExistsFor<TSource, TDestination>(string fields);

    #endregion Property Mapping Validation

    #region Diagnostic and Management Operations

    /// <summary>
    /// Retrieves comprehensive diagnostic information about the property mapping service configuration and performance.
    /// </summary>
    /// <returns>
    /// A dictionary containing detailed diagnostic information about:
    /// - Service configuration and operational status
    /// - Registered property mappings count and details
    /// - Cache statistics and performance metrics
    /// - Memory usage and optimization insights
    /// - Configuration validation results and health indicators
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive diagnostic capabilities including:
    /// 
    /// Service Configuration Information:
    /// - Total count of registered property mappings across all type pairs
    /// - Service initialization status and configuration validation results
    /// - Supported type pairs and mapping coverage analysis
    /// - Configuration errors or warnings detected during initialization
    /// - Service health indicators and operational status
    /// 
    /// Performance Metrics and Statistics:
    /// - Cache hit rates and miss statistics for mapping lookups
    /// - Average response times for mapping resolution operations
    /// - Memory usage statistics for cached mapping configurations
    /// - Request volume and throughput metrics for monitoring
    /// - Performance bottlenecks and optimization opportunities
    /// 
    /// Operational Monitoring Data:
    /// - Recent mapping lookup patterns and frequency analysis
    /// - Error rates and failure patterns for reliability assessment
    /// - Resource utilization metrics for capacity planning
    /// - Integration health with dependent services and components
    /// - System resource consumption and optimization recommendations
    /// 
    /// The diagnostic information is particularly useful for:
    /// - Service health monitoring and alerting in production environments
    /// - Performance optimization and capacity planning initiatives
    /// - Configuration validation and verification during deployment
    /// - Development and debugging scenarios for mapping issues
    /// - Operational monitoring and service level agreement compliance
    /// 
    /// Integration with monitoring systems:
    /// - Compatible with application performance monitoring tools
    /// - Structured data format suitable for logging and analytics
    /// - Metric collection support for dashboard and alerting systems
    /// - Health check integration for load balancer and orchestration
    /// - Audit trail support for compliance and governance requirements
    /// </remarks>
    /// <example>
    /// <code>
    /// // Basic diagnostic information retrieval
    /// var diagnostics = _mappingService.GetDiagnosticInfo();
    /// 
    /// // Log key metrics for monitoring
    /// _logger.LogInformation("Property mapping service diagnostics: " +
    ///     "Registered mappings: {RegisteredMappings}, " +
    ///     "Cache entries: {CacheEntries}, " +
    ///     "Service status: {ServiceStatus}",
    ///     diagnostics["RegisteredMappingCount"],
    ///     diagnostics["CachedMappingCount"],
    ///     diagnostics["ServiceStatus"]);
    /// 
    /// // Health check integration
    /// public class PropertyMappingHealthCheck : IHealthCheck
    /// {
    ///     private readonly IPropertyMappingService _mappingService;
    ///     
    ///     public async Task&lt;HealthCheckResult&gt; CheckHealthAsync(
    ///         HealthCheckContext context, 
    ///         CancellationToken cancellationToken = default)
    ///     {
    ///         try
    ///         {
    ///             var diagnostics = _mappingService.GetDiagnosticInfo();
    ///             var status = diagnostics["ServiceStatus"]?.ToString();
    ///             
    ///             if (status == "Healthy")
    ///             {
    ///                 return HealthCheckResult.Healthy(
    ///                     "Property mapping service is operating normally", 
    ///                     diagnostics);
    ///             }
    ///             else
    ///             {
    ///                 return HealthCheckResult.Degraded(
    ///                     $"Property mapping service status: {status}", 
    ///                     data: diagnostics);
    ///             }
    ///         }
    ///         catch (Exception ex)
    ///         {
    ///             return HealthCheckResult.Unhealthy(
    ///                 "Property mapping service health check failed", 
    ///                 ex);
    ///         }
    ///     }
    /// }
    /// 
    /// // Performance monitoring integration
    /// public class PropertyMappingMonitor
    /// {
    ///     public void LogPerformanceMetrics()
    ///     {
    ///         var diagnostics = _mappingService.GetDiagnosticInfo();
    ///         
    ///         // Extract performance metrics
    ///         if (diagnostics.TryGetValue("CacheHitRate", out var hitRate))
    ///         {
    ///             _performanceLogger.LogMetric("PropertyMapping.CacheHitRate", (double)hitRate);
    ///         }
    ///         
    ///         if (diagnostics.TryGetValue("AverageResponseTime", out var responseTime))
    ///         {
    ///             _performanceLogger.LogMetric("PropertyMapping.AvgResponseTime", (TimeSpan)responseTime);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    Dictionary<string, object> GetDiagnosticInfo();

    /// <summary>
    /// Clears all cached property mappings to free memory and force re-resolution of mapping configurations.
    /// </summary>
    /// <remarks>
    /// This method provides cache management capabilities for:
    /// 
    /// Memory Optimization Scenarios:
    /// - Long-running application memory optimization and cleanup
    /// - Periodic cache maintenance to prevent memory leaks
    /// - Resource management in memory-constrained environments
    /// - Cache size control in high-volume applications
    /// 
    /// Development and Testing Support:
    /// - Testing scenarios where fresh mapping resolution is required
    /// - Development environment cache invalidation during testing
    /// - Configuration change scenarios requiring cache refresh
    /// - Performance testing and benchmarking with clean cache state
    /// 
    /// Dynamic Configuration Management:
    /// - Runtime configuration changes requiring cache invalidation
    /// - Dynamic mapping updates in configuration-driven scenarios
    /// - Hot-swapping of mapping configurations without service restart
    /// - A/B testing scenarios with different mapping configurations
    /// 
    /// Operational Maintenance:
    /// - Scheduled maintenance procedures for cache optimization
    /// - Memory pressure response and resource management
    /// - Diagnostic procedures for troubleshooting mapping issues
    /// - Performance optimization and capacity management
    /// 
    /// Important Considerations:
    /// - Clearing the cache will cause temporary performance impact
    /// - Property mappings will be re-resolved on subsequent requests
    /// - Thread-safe operation suitable for runtime execution
    /// - Logged operation for audit trail and monitoring purposes
    /// - No impact on registered mapping configurations (only cache cleared)
    /// 
    /// The method is designed to be:
    /// - Safe: No risk to service stability or mapping configurations
    /// - Fast: Efficient cache clearing with minimal service disruption
    /// - Observable: Comprehensive logging for operational monitoring
    /// - Recoverable: Automatic cache rebuilding on subsequent requests
    /// </remarks>
    /// <example>
    /// <code>
    /// // Scheduled cache maintenance
    /// public class PropertyMappingMaintenanceService
    /// {
    ///     private readonly IPropertyMappingService _mappingService;
    ///     private readonly Timer _maintenanceTimer;
    ///     
    ///     public void StartMaintenanceSchedule()
    ///     {
    ///         // Clear cache every hour for memory optimization
    ///         _maintenanceTimer = new Timer(PerformMaintenance, null, 
    ///             TimeSpan.FromHours(1), TimeSpan.FromHours(1));
    ///     }
    ///     
    ///     private void PerformMaintenance(object state)
    ///     {
    ///         _logger.LogInformation("Performing scheduled property mapping cache maintenance");
    ///         _mappingService.ClearMappingCache();
    ///         _logger.LogInformation("Property mapping cache maintenance completed");
    ///     }
    /// }
    /// 
    /// // Memory pressure response
    /// public class MemoryPressureHandler
    /// {
    ///     public void HandleMemoryPressure()
    ///     {
    ///         _logger.LogWarning("Memory pressure detected - clearing property mapping cache");
    ///         _mappingService.ClearMappingCache();
    ///         
    ///         // Force garbage collection after cache clearing
    ///         GC.Collect();
    ///         GC.WaitForPendingFinalizers();
    ///         GC.Collect();
    ///     }
    /// }
    /// 
    /// // Configuration change handler
    /// public class MappingConfigurationService
    /// {
    ///     public async Task UpdateMappingConfigurationAsync(MappingConfiguration newConfig)
    ///     {
    ///         _logger.LogInformation("Updating property mapping configuration");
    ///         
    ///         // Clear existing cache before applying new configuration
    ///         _mappingService.ClearMappingCache();
    ///         
    ///         // Apply new configuration
    ///         await ApplyConfigurationAsync(newConfig);
    ///         
    ///         _logger.LogInformation("Property mapping configuration update completed");
    ///     }
    /// }
    /// 
    /// // Testing utility
    /// public class PropertyMappingTestHelper
    /// {
    ///     public void PrepareCleanTestEnvironment()
    ///     {
    ///         // Ensure clean cache state for testing
    ///         _mappingService.ClearMappingCache();
    ///         
    ///         // Verify cache is empty
    ///         var diagnostics = _mappingService.GetDiagnosticInfo();
    ///         Assert.Equal(0, diagnostics["CachedMappingCount"]);
    ///     }
    /// }
    /// </code>
    /// </example>
    void ClearMappingCache();

    #endregion Diagnostic and Management Operations

    #region Registration and Configuration

    /// <summary>
    /// Gets the collection of registered property mappings in an immutable format for inspection and validation.
    /// </summary>
    /// <value>
    /// An immutable collection of property mapping configurations that provides:
    /// - Read-only access to all registered property mappings
    /// - Thread-safe enumeration of mapping configurations
    /// - Integration support for mapping validation and diagnostics
    /// - Debugging capabilities for troubleshooting mapping issues
    /// </value>
    /// <remarks>
    /// This property provides comprehensive access to mapping configurations including:
    /// 
    /// Mapping Configuration Information:
    /// - All automatically generated property mappings between DTO and entity types
    /// - Custom property mapping overrides and configurations
    /// - Bidirectional mappings for reverse transformation scenarios
    /// - Complex mappings with multiple destination properties
    /// 
    /// Diagnostic and Validation Support:
    /// - Configuration validation during application startup
    /// - Runtime introspection of available mapping capabilities
    /// - Unit testing scenarios for mapping configuration verification
    /// - Debugging and troubleshooting of mapping issues
    /// 
    /// Integration Capabilities:
    /// - Monitoring tools for mapping configuration analysis
    /// - Documentation generation for API specifications
    /// - Configuration management and deployment validation
    /// - Health checking and service verification
    /// 
    /// Thread Safety and Performance:
    /// - Immutable collection prevents accidental modification
    /// - Thread-safe enumeration for concurrent access scenarios
    /// - Efficient access without performance overhead
    /// - Cached collection for repeated access operations
    /// 
    /// The collection is particularly useful for:
    /// - Service initialization validation and configuration checking
    /// - Development tools and diagnostic utilities
    /// - Automated testing and configuration verification
    /// - Documentation and API specification generation
    /// - Operational monitoring and health checking
    /// </remarks>
    /// <example>
    /// <code>
    /// // Configuration validation during startup
    /// public class PropertyMappingValidator
    /// {
    ///     public ValidationResult ValidateAllMappings()
    ///     {
    ///         var mappings = _mappingService.RegisteredMappings;
    ///         var errors = new List&lt;string&gt;();
    ///         
    ///         foreach (var mapping in mappings)
    ///         {
    ///             // Validate each mapping configuration
    ///             if (mapping.PropertyMappingCount == 0)
    ///             {
    ///                 errors.Add($"Empty mapping found: {mapping}");
    ///             }
    ///         }
    ///         
    ///         return errors.Any() 
    ///             ? ValidationResult.Failed(errors)
    ///             : ValidationResult.Success();
    ///     }
    /// }
    /// 
    /// // Documentation generation
    /// public class ApiDocumentationGenerator
    /// {
    ///     public string GenerateMappingDocumentation()
    ///     {
    ///         var mappings = _mappingService.RegisteredMappings;
    ///         var documentation = new StringBuilder();
    ///         
    ///         documentation.AppendLine("## Property Mappings");
    ///         
    ///         foreach (var mapping in mappings)
    ///         {
    ///             documentation.AppendLine($"- {mapping}");
    ///         }
    ///         
    ///         return documentation.ToString();
    ///     }
    /// }
    /// </code>
    /// </example>
    ImmutableList<IPropertyMapping> RegisteredMappings { get; }

    #endregion Registration and Configuration
}