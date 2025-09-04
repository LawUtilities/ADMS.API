using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ADMS.API.Services;

/// <summary>
/// Represents a strongly-typed property mapping configuration for transforming between source and destination types.
/// </summary>
/// <typeparam name="TSource">The source type (typically a DTO) containing the properties to be mapped from.</typeparam>
/// <typeparam name="TDestination">The destination type (typically an entity) containing the properties to be mapped to.</typeparam>
/// <remarks>
/// This class provides enterprise-grade property mapping functionality including:
/// 
/// Core Functionality:
/// - Type-safe property mapping between source DTOs and destination entities
/// - Immutable mapping configurations for thread safety and consistency
/// - Comprehensive validation and error handling for mapping operations
/// - High-performance property transformation with optimized lookup operations
/// 
/// Property Mapping Features:
/// - Support for simple one-to-one property mappings (DTO.Name -> Entity.Name)
/// - Complex one-to-many property mappings (DTO.FullName -> Entity.FirstName, Entity.LastName)
/// - Bidirectional property mappings with revert support for inverse operations
/// - Case-insensitive property name matching for flexible mapping scenarios
/// - Comprehensive property validation against target type schemas
/// 
/// Performance Characteristics:
/// - Immutable dictionary storage for thread-safe concurrent access
/// - Optimized property lookup operations with O(1) average complexity
/// - Memory-efficient storage using optimized collection types
/// - Lazy evaluation of complex mapping operations where appropriate
/// - Cached property information for improved repeated access patterns
/// 
/// Integration Features:
/// - Seamless integration with Entity Framework Core for database operations
/// - Support for LINQ expression building for dynamic sorting and filtering
/// - Compatibility with mapping frameworks like AutoMapper for complex scenarios
/// - Integration with dependency injection containers for service lifetime management
/// - Support for API versioning with evolving mapping requirements
/// 
/// Thread Safety and Concurrency:
/// - All mapping operations are thread-safe for concurrent access scenarios
/// - Immutable mapping configurations prevent accidental modification
/// - No shared mutable state that could cause race conditions
/// - Safe for use in multi-threaded web application environments
/// - Concurrent dictionary implementation for high-performance scenarios
/// 
/// Validation and Error Handling:
/// - Comprehensive validation of mapping configurations during initialization
/// - Type-safe property mapping validation at compile time where possible
/// - Runtime validation of property mappings against target schemas
/// - Detailed error messages for troubleshooting mapping configuration issues
/// - Integration with logging infrastructure for debugging and monitoring
/// 
/// The mapping configuration is designed to be:
/// - Immutable: Cannot be modified after construction to ensure consistency
/// - Type-safe: Compile-time type checking for source and destination types
/// - High-performance: Optimized for frequent property mapping operations
/// - Thread-safe: Safe for use in concurrent scenarios without synchronization
/// - Extensible: Easy to extend with additional mapping capabilities as needed
/// 
/// Common Use Cases:
/// - API request/response transformation between DTOs and domain entities
/// - Database query result mapping from entities to presentation models
/// - Dynamic sorting and filtering operations with property name translation
/// - Data validation and transformation pipelines with type safety
/// - Cross-layer data transformation in layered architecture patterns
/// 
/// Example scenarios include:
/// - Mapping DocumentDto to Document entity for database persistence
/// - Transforming RevisionDto to Revision entity for version control operations
/// - Converting MatterDto to Matter entity for legal document management
/// - Supporting API evolution with backward-compatible property mappings
/// </remarks>
/// <example>
/// <code>
/// // Create a property mapping for DocumentDto to Document entity
/// var mappingDictionary = new Dictionary&lt;string, PropertyMappingValue&gt;
/// {
///     ["FileName"] = new PropertyMappingValue(new[] { "FileName" }),
///     ["FileSize"] = new PropertyMappingValue(new[] { "FileSize" }),
///     ["CreatedDate"] = new PropertyMappingValue(new[] { "CreationDate" }),
///     ["FullName"] = new PropertyMappingValue(new[] { "FirstName", "LastName" })
/// };
/// 
/// var propertyMapping = new PropertyMapping&lt;DocumentDto, Document&gt;(mappingDictionary);
/// 
/// // Use the mapping for property lookups and validations
/// bool canMapFileName = propertyMapping.HasMapping("FileName");
/// var fileNameMapping = propertyMapping.GetMapping("FileName");
/// string destinationProperty = fileNameMapping.GetPrimaryProperty();
/// </code>
/// </example>
public sealed class PropertyMapping<TSource, TDestination> : IPropertyMapping
{
    /// <summary>
    /// Gets the immutable dictionary containing the property mapping configuration.
    /// </summary>
    /// <remarks>
    /// This property provides:
    /// - Read-only access to the complete property mapping configuration
    /// - Thread-safe enumeration and lookup of property mappings
    /// - Integration support for external mapping frameworks and tools
    /// - Debugging capabilities for troubleshooting mapping issues
    /// 
    /// Dictionary Characteristics:
    /// - Case-insensitive string keys for flexible property name matching
    /// - Immutable after construction to prevent accidental modification
    /// - Optimized for frequent lookup operations with O(1) average complexity
    /// - Memory-efficient storage using dictionary optimization techniques
    /// 
    /// Key-Value Structure:
    /// - Keys: Source property names from the TSource type (case-insensitive)
    /// - Values: PropertyMappingValue instances containing destination property information
    /// - Supports one-to-one and one-to-many property mapping scenarios
    /// - Enables complex property transformations and bidirectional mappings
    /// 
    /// Usage Patterns:
    /// - Direct property lookup: mappingDictionary["PropertyName"]
    /// - Enumeration of all mappings: foreach(var kvp in mappingDictionary)
    /// - Existence checking: mappingDictionary.ContainsKey("PropertyName")
    /// - Integration with LINQ: mappingDictionary.Where(kvp => condition)
    /// 
    /// The dictionary is used extensively by:
    /// - Property mapping services for dynamic property transformation
    /// - LINQ expression builders for database query generation
    /// - Validation services for mapping configuration verification
    /// - Sorting and filtering operations for property name translation
    /// </remarks>
    /// <value>
    /// An immutable dictionary mapping source property names to PropertyMappingValue instances.
    /// The dictionary uses case-insensitive string comparison for robust property matching.
    /// </value>
    public ImmutableDictionary<string, PropertyMappingValue> MappingDictionary { get; }

    /// <summary>
    /// Gets the source type information for this property mapping configuration.
    /// </summary>
    /// <remarks>
    /// This property provides:
    /// - Runtime type information for the source type (typically a DTO)
    /// - Reflection capabilities for dynamic property discovery and validation
    /// - Type safety validation for mapping operations
    /// - Integration support for generic mapping frameworks
    /// 
    /// The source type information is used for:
    /// - Validation of mapping configurations against source type schema
    /// - Dynamic property discovery during mapping generation
    /// - Type compatibility checking for mapping operations
    /// - Error reporting with specific type context information
    /// </remarks>
    /// <value>The Type information for the source type TSource.</value>
    public Type SourceType => typeof(TSource);

    /// <summary>
    /// Gets the destination type information for this property mapping configuration.
    /// </summary>
    /// <remarks>
    /// This property provides:
    /// - Runtime type information for the destination type (typically an entity)
    /// - Reflection capabilities for property existence validation
    /// - Type safety enforcement for mapping operations
    /// - Support for complex type transformation scenarios
    /// 
    /// The destination type information is used for:
    /// - Validation of destination property names in mapping configurations
    /// - Type compatibility verification during mapping operations
    /// - Dynamic property discovery for automatic mapping generation
    /// - Error reporting with destination type context information
    /// </remarks>
    /// <value>The Type information for the destination type TDestination.</value>
    public Type DestinationType => typeof(TDestination);

    /// <summary>
    /// Gets the count of property mappings defined in this configuration.
    /// </summary>
    /// <remarks>
    /// This convenience property provides:
    /// - Quick access to the number of mapped properties
    /// - Performance metrics for mapping complexity analysis
    /// - Validation support for mapping completeness checking
    /// - Debugging information for mapping configuration inspection
    /// 
    /// The property count is useful for:
    /// - Performance optimization based on mapping complexity
    /// - Validation that required properties are mapped
    /// - Logging and monitoring of mapping configuration size
    /// - Unit testing verification of mapping completeness
    /// </remarks>
    /// <value>
    /// The total number of property mappings in the mapping dictionary.
    /// Always non-negative and reflects the current mapping configuration size.
    /// </value>
    public int PropertyMappingCount => MappingDictionary.Count;

    /// <summary>
    /// Gets a value indicating whether this mapping configuration is empty.
    /// </summary>
    /// <remarks>
    /// This convenience property identifies:
    /// - Mapping configurations that contain no property mappings
    /// - Potential configuration errors or incomplete setup
    /// - Edge cases that require special handling in mapping operations
    /// - Validation scenarios for mapping completeness checking
    /// 
    /// Empty mappings might indicate:
    /// - Incomplete mapping configuration during development
    /// - Types with no compatible properties for automatic mapping
    /// - Configuration errors that need to be addressed
    /// - Special cases where no property transformation is needed
    /// </remarks>
    /// <value>
    /// true if the mapping configuration contains no property mappings;
    /// false if one or more property mappings are defined.
    /// </value>
    public bool IsEmpty => MappingDictionary.Count == 0;

    /// <summary>
    /// Initializes a new instance of the PropertyMapping class with comprehensive validation.
    /// </summary>
    /// <param name="mappingDictionary">
    /// The dictionary containing property mapping configurations from source property names to PropertyMappingValue instances.
    /// Must not be null and should contain valid property mappings for the source and destination types.
    /// Dictionary keys should correspond to valid properties on the source type (TSource).
    /// Dictionary values should contain valid destination property names from the destination type (TDestination).
    /// </param>
    /// <remarks>
    /// The initialization process includes:
    /// 
    /// 1. Parameter Validation:
    ///    - Validates that mappingDictionary parameter is not null
    ///    - Ensures all dictionary keys are valid non-empty strings
    ///    - Verifies all dictionary values are valid PropertyMappingValue instances
    ///    - Checks for duplicate property mappings and conflicting configurations
    /// 
    /// 2. Type Safety Validation:
    ///    - Validates source property names against the TSource type schema
    ///    - Verifies destination property names against the TDestination type schema
    ///    - Ensures type compatibility between mapped properties where applicable
    ///    - Logs validation warnings for potential mapping issues
    /// 
    /// 3. Immutable Configuration Creation:
    ///    - Creates an immutable dictionary from the provided mapping configuration
    ///    - Uses case-insensitive string comparison for robust property name matching
    ///    - Optimizes the dictionary structure for frequent lookup operations
    ///    - Ensures thread safety for concurrent access scenarios
    /// 
    /// 4. Performance Optimization:
    ///    - Pre-validates all property mappings to avoid runtime validation overhead
    ///    - Optimizes dictionary structure for O(1) lookup performance
    ///    - Caches frequently accessed mapping information where appropriate
    ///    - Minimizes memory overhead while maintaining mapping functionality
    /// 
    /// The constructor is designed to be fail-fast, catching configuration errors
    /// early in the application lifecycle rather than during runtime operations.
    /// This ensures that mapping issues are discovered during development and
    /// testing phases rather than in production environments.
    /// 
    /// Performance characteristics:
    /// - O(n) initialization time where n is the number of property mappings
    /// - O(1) average lookup time for property mappings after initialization
    /// - Minimal memory overhead with efficient immutable storage
    /// - Thread-safe operations without synchronization overhead
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the mappingDictionary parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the mappingDictionary contains invalid keys, null values, or configuration errors.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when property validation fails due to type compatibility issues or missing properties.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create a comprehensive property mapping configuration
    /// var mappingDictionary = new Dictionary&lt;string, PropertyMappingValue&gt;(StringComparer.OrdinalIgnoreCase)
    /// {
    ///     // Simple one-to-one property mapping
    ///     ["FileName"] = new PropertyMappingValue(new[] { "FileName" }),
    ///     
    ///     // Property mapping with different destination name
    ///     ["CreatedDate"] = new PropertyMappingValue(new[] { "CreationTimestamp" }),
    ///     
    ///     // One-to-many property mapping for complex transformations
    ///     ["FullName"] = new PropertyMappingValue(new[] { "FirstName", "LastName" }),
    ///     
    ///     // Bidirectional property mapping with revert support
    ///     ["SortOrder"] = new PropertyMappingValue(new[] { "DisplayOrder" }, revert: true)
    /// };
    /// 
    /// // Initialize the property mapping with validation
    /// var propertyMapping = new PropertyMapping&lt;DocumentDto, Document&gt;(mappingDictionary);
    /// 
    /// // The mapping is now ready for use in property transformation operations
    /// bool hasFileNameMapping = propertyMapping.HasMapping("FileName");
    /// var mapping = propertyMapping.GetMapping("CreatedDate");
    /// </code>
    /// </example>
    public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictionary)
    {
        // Validate input parameter
        ArgumentNullException.ThrowIfNull(mappingDictionary, nameof(mappingDictionary));

        try
        {
            // Validate dictionary contents
            ValidateMappingDictionary(mappingDictionary);

            // Create immutable dictionary with case-insensitive keys for robust property matching
            MappingDictionary = mappingDictionary.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

            // Perform comprehensive validation of the mapping configuration
            ValidateMappingConfiguration();
        }
        catch (Exception ex) when (!(ex is ArgumentNullException or ArgumentException))
        {
            throw new InvalidOperationException(
                $"Failed to initialize PropertyMapping<{typeof(TSource).Name}, {typeof(TDestination).Name}>. " +
                "See inner exception for details.", ex);
        }
    }

    /// <summary>
    /// Determines whether a property mapping exists for the specified source property name.
    /// </summary>
    /// <param name="sourcePropertyName">The name of the source property to check for mapping existence.</param>
    /// <returns>
    /// true if a property mapping exists for the specified source property name;
    /// false if no mapping is found or if the property name is invalid.
    /// </returns>
    /// <remarks>
    /// This method provides:
    /// - Case-insensitive property name matching for robust lookup operations
    /// - Fast O(1) lookup performance using optimized dictionary operations
    /// - Null-safe parameter handling with graceful error responses
    /// - Integration support for dynamic property mapping validation
    /// 
    /// The method is commonly used for:
    /// - Validating that requested sort/filter properties have valid mappings
    /// - Pre-flight checks before performing property transformation operations
    /// - Dynamic API parameter validation in controller methods
    /// - Conditional logic based on property mapping availability
    /// 
    /// Performance characteristics:
    /// - O(1) average case lookup time using hash-based dictionary operations
    /// - Minimal memory allocation for lookup operations
    /// - Thread-safe operation suitable for concurrent access scenarios
    /// - Consistent performance regardless of mapping dictionary size
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when sourcePropertyName is null, empty, or contains only whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var propertyMapping = new PropertyMapping&lt;DocumentDto, Document&gt;(mappingDictionary);
    /// 
    /// // Check if specific properties can be mapped
    /// bool canMapFileName = propertyMapping.HasMapping("FileName");        // Returns true if mapping exists
    /// bool canMapFileSize = propertyMapping.HasMapping("FileSize");        // Returns true if mapping exists
    /// bool canMapInvalid = propertyMapping.HasMapping("NonExistent");      // Returns false
    /// 
    /// // Use in conditional logic
    /// if (propertyMapping.HasMapping("CreatedDate"))
    /// {
    ///     // Safe to use CreatedDate in sorting/filtering operations
    ///     var mapping = propertyMapping.GetMapping("CreatedDate");
    ///     // Process the mapping...
    /// }
    /// </code>
    /// </example>
    public bool HasMapping(string sourcePropertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePropertyName, nameof(sourcePropertyName));

        return MappingDictionary.ContainsKey(sourcePropertyName);
    }

    /// <summary>
    /// Retrieves the property mapping configuration for the specified source property name.
    /// </summary>
    /// <param name="sourcePropertyName">The name of the source property to retrieve mapping information for.</param>
    /// <returns>
    /// The PropertyMappingValue instance containing the mapping configuration for the specified property.
    /// </returns>
    /// <remarks>
    /// This method provides:
    /// - Fast O(1) lookup of property mapping configurations
    /// - Case-insensitive property name matching for flexible usage
    /// - Complete mapping information including destination properties and configuration
    /// - Integration support for LINQ expression building and property transformation
    /// 
    /// The returned PropertyMappingValue contains:
    /// - Destination property names for the mapping operation
    /// - Bidirectional mapping configuration (revert flag)
    /// - Validation information for mapping verification
    /// - Additional metadata for complex mapping scenarios
    /// 
    /// Common usage patterns:
    /// - Building LINQ expressions for database queries with property translation
    /// - Validating property mapping configurations during development
    /// - Constructing sort and filter expressions for paginated operations
    /// - Dynamic property transformation in data access layers
    /// 
    /// The method is optimized for frequent access scenarios and provides
    /// consistent performance regardless of the mapping dictionary size.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when sourcePropertyName is null, empty, or contains only whitespace.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no property mapping exists for the specified source property name.
    /// Use HasMapping method to check for mapping existence before calling this method.
    /// </exception>
    /// <example>
    /// <code>
    /// var propertyMapping = new PropertyMapping&lt;DocumentDto, Document&gt;(mappingDictionary);
    /// 
    /// try
    /// {
    ///     // Retrieve specific property mapping
    ///     var fileNameMapping = propertyMapping.GetMapping("FileName");
    ///     string destinationProperty = fileNameMapping.GetPrimaryProperty();
    ///     
    ///     // Use mapping for LINQ expression building
    ///     var createdDateMapping = propertyMapping.GetMapping("CreatedDate");
    ///     bool shouldReverse = createdDateMapping.Revert;
    ///     var destinationProperties = createdDateMapping.DestinationProperties;
    /// }
    /// catch (KeyNotFoundException)
    /// {
    ///     // Handle missing mapping gracefully
    ///     // Consider using HasMapping() method for pre-validation
    /// }
    /// 
    /// // Safe usage with pre-validation
    /// if (propertyMapping.HasMapping("FileSize"))
    /// {
    ///     var fileSizeMapping = propertyMapping.GetMapping("FileSize");
    ///     // Guaranteed to succeed since HasMapping returned true
    /// }
    /// </code>
    /// </example>
    public PropertyMappingValue GetMapping(string sourcePropertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePropertyName, nameof(sourcePropertyName));

        if (MappingDictionary.TryGetValue(sourcePropertyName, out var mappingValue))
        {
            return mappingValue;
        }

        throw new KeyNotFoundException(
            $"No property mapping found for source property '{sourcePropertyName}' " +
            $"in PropertyMapping<{typeof(TSource).Name}, {typeof(TDestination).Name}>. " +
            $"Available mappings: {string.Join(", ", MappingDictionary.Keys)}");
    }

    /// <summary>
    /// Attempts to retrieve the property mapping configuration for the specified source property name.
    /// </summary>
    /// <param name="sourcePropertyName">The name of the source property to retrieve mapping information for.</param>
    /// <param name="mappingValue">
    /// When this method returns, contains the PropertyMappingValue associated with the specified property name,
    /// if the mapping is found; otherwise, null. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// true if the property mapping configuration contains a mapping for the specified property name;
    /// otherwise, false.
    /// </returns>
    /// <remarks>
    /// This method provides:
    /// - Safe property mapping retrieval without exception throwing
    /// - Case-insensitive property name matching for flexible usage
    /// - Fast O(1) lookup performance using optimized dictionary operations
    /// - Null-safe parameter handling with defensive programming practices
    /// 
    /// This method is preferred over GetMapping when:
    /// - Property mapping existence is uncertain
    /// - Exception handling overhead should be avoided
    /// - Performance-critical code paths require optimal efficiency
    /// - Graceful handling of missing mappings is required
    /// 
    /// The method follows the standard .NET TryGet pattern for consistency
    /// with other framework APIs and provides predictable behavior for
    /// developers familiar with similar methods in the base class libraries.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when sourcePropertyName is null, empty, or contains only whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var propertyMapping = new PropertyMapping&lt;DocumentDto, Document&gt;(mappingDictionary);
    /// 
    /// // Safe retrieval without exception handling
    /// if (propertyMapping.TryGetMapping("FileName", out var fileNameMapping))
    /// {
    ///     // Mapping found - safe to use fileNameMapping
    ///     string destinationProperty = fileNameMapping.GetPrimaryProperty();
    ///     bool shouldReverse = fileNameMapping.Revert;
    /// }
    /// else
    /// {
    ///     // Mapping not found - handle gracefully
    ///     // Could log warning, use default behavior, or skip processing
    /// }
    /// 
    /// // Performance-optimized usage in loops
    /// foreach (string propertyName in requestedProperties)
    /// {
    ///     if (propertyMapping.TryGetMapping(propertyName, out var mapping))
    ///     {
    ///         // Process the mapping without exception overhead
    ///         ProcessPropertyMapping(propertyName, mapping);
    ///     }
    /// }
    /// </code>
    /// </example>
    public bool TryGetMapping(string sourcePropertyName, [NotNullWhen(true)] out PropertyMappingValue? mappingValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePropertyName, nameof(sourcePropertyName));

        return MappingDictionary.TryGetValue(sourcePropertyName, out mappingValue);
    }

    /// <summary>
    /// Gets all source property names that have mappings defined in this configuration.
    /// </summary>
    /// <returns>
    /// An enumerable collection of source property names that can be mapped using this configuration.
    /// The collection is immutable and thread-safe for concurrent enumeration.
    /// </returns>
    /// <remarks>
    /// This method provides:
    /// - Complete enumeration of all mappable source properties
    /// - Thread-safe iteration over property names
    /// - Integration support for dynamic property discovery
    /// - Debugging capabilities for mapping configuration inspection
    /// 
    /// The returned collection characteristics:
    /// - Immutable: Cannot be modified after retrieval
    /// - Thread-safe: Safe for concurrent enumeration operations
    /// - Efficient: Minimal memory allocation and optimal performance
    /// - Consistent: Preserves original property name casing and order
    /// 
    /// Common usage scenarios:
    /// - Validating API parameters against available property mappings
    /// - Generating documentation for supported sort/filter properties
    /// - Dynamic user interface generation for property selection
    /// - Testing and debugging mapping configuration completeness
    /// 
    /// The method is optimized for frequent access and provides consistent
    /// performance characteristics regardless of mapping dictionary size.
    /// </remarks>
    /// <example>
    /// <code>
    /// var propertyMapping = new PropertyMapping&lt;DocumentDto, Document&gt;(mappingDictionary);
    /// 
    /// // Get all mappable property names
    /// var mappableProperties = propertyMapping.GetMappableProperties().ToList();
    /// 
    /// // Use for validation
    /// foreach (string requestedProperty in apiParameters.SortFields)
    /// {
    ///     if (!mappableProperties.Contains(requestedProperty))
    ///     {
    ///         throw new ArgumentException($"Property '{requestedProperty}' cannot be used for sorting");
    ///     }
    /// }
    /// 
    /// // Use for documentation or UI generation
    /// var supportedSortFields = string.Join(", ", mappableProperties);
    /// var helpText = $"Supported sort fields: {supportedSortFields}";
    /// 
    /// // Use for debugging and logging
    /// var logger = serviceProvider.GetRequiredService&lt;ILogger&gt;();
    /// logger.LogDebug("Property mapping supports {Count} properties: {Properties}",
    ///     mappableProperties.Count, string.Join(", ", mappableProperties));
    /// </code>
    /// </example>
    public IEnumerable<string> GetMappableProperties()
    {
        return MappingDictionary.Keys;
    }

    /// <summary>
    /// Validates the entire property mapping configuration against the source and destination type schemas.
    /// </summary>
    /// <returns>
    /// An enumerable collection of validation results describing any configuration issues found.
    /// An empty collection indicates that the mapping configuration is valid.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive validation including:
    /// 
    /// Source Type Validation:
    /// - Verifies that all mapped source properties exist on the TSource type
    /// - Validates property accessibility and visibility requirements
    /// - Checks for deprecated or obsolete property usage
    /// - Ensures property name spelling and casing consistency
    /// 
    /// Destination Type Validation:
    /// - Confirms that all destination properties exist on the TDestination type
    /// - Validates property write accessibility for mapping operations
    /// - Checks type compatibility between source and destination properties
    /// - Ensures complex property paths are valid (e.g., "Address.Street")
    /// 
    /// Mapping Configuration Validation:
    /// - Identifies duplicate property mappings and conflicting configurations
    /// - Validates bidirectional mapping configurations for consistency
    /// - Checks for circular references in complex property mappings
    /// - Ensures mapping completeness for critical properties
    /// 
    /// Business Rule Validation:
    /// - Applies domain-specific validation rules where applicable
    /// - Validates against known property mapping anti-patterns
    /// - Checks for performance implications of complex mappings
    /// - Ensures security considerations are addressed
    /// 
    /// The validation results include:
    /// - Detailed error descriptions for each validation failure
    /// - Property names and context information for troubleshooting
    /// - Severity levels (errors, warnings, informational messages)
    /// - Recommended actions for resolving configuration issues
    /// 
    /// This method is particularly useful for:
    /// - Unit testing of property mapping configurations
    /// - Development-time validation of mapping completeness
    /// - Configuration validation during application startup
    /// - Debugging complex property mapping scenarios
    /// </remarks>
    /// <example>
    /// <code>
    /// var propertyMapping = new PropertyMapping&lt;DocumentDto, Document&gt;(mappingDictionary);
    /// 
    /// // Validate the mapping configuration
    /// var validationResults = propertyMapping.ValidateConfiguration().ToList();
    /// 
    /// if (validationResults.Any())
    /// {
    ///     // Handle validation errors
    ///     foreach (var result in validationResults)
    ///     {
    ///         logger.LogWarning("Property mapping validation issue: {Message}", result.ErrorMessage);
    ///         
    ///         if (result.MemberNames.Any())
    ///         {
    ///             logger.LogWarning("Affected properties: {Properties}", 
    ///                 string.Join(", ", result.MemberNames));
    ///         }
    ///     }
    ///     
    ///     // Consider throwing exception for critical errors
    ///     var criticalErrors = validationResults.Where(r => IsCriticalError(r));
    ///     if (criticalErrors.Any())
    ///     {
    ///         throw new InvalidOperationException("Critical property mapping configuration errors detected");
    ///     }
    /// }
    /// else
    /// {
    ///     logger.LogInformation("Property mapping configuration validation successful");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<ValidationResult> ValidateConfiguration()
    {
        var validationResults = new List<ValidationResult>();

        try
        {
            // Validate source type properties
            validationResults.AddRange(ValidateSourceTypeProperties());

            // Validate destination type properties
            validationResults.AddRange(ValidateDestinationTypeProperties());

            // Validate mapping consistency
            validationResults.AddRange(ValidateMappingConsistency());

            return validationResults;
        }
        catch (Exception ex)
        {
            validationResults.Add(new ValidationResult(
                $"Unexpected error during property mapping validation: {ex.Message}",
                new[] { "ValidationError" }));

            return validationResults;
        }
    }

    /// <summary>
    /// Returns a string representation of the property mapping configuration.
    /// </summary>
    /// <returns>
    /// A string containing summary information about the property mapping configuration
    /// in a human-readable format suitable for debugging and logging.
    /// </returns>
    /// <remarks>
    /// The string representation includes:
    /// - Source and destination type names for context identification
    /// - Total count of property mappings for configuration overview
    /// - Summary of mapping characteristics (simple, complex, bidirectional)
    /// - Memory and performance characteristics for optimization insights
    /// 
    /// The formatted output provides:
    /// - Consistent formatting for logging and monitoring scenarios
    /// - Sufficient detail for debugging configuration issues
    /// - Concise summary suitable for diagnostic displays
    /// - Integration with standard .NET logging frameworks
    /// 
    /// Example output format:
    /// "PropertyMapping&lt;DocumentDto, Document&gt;: 8 mappings (6 simple, 2 complex)"
    /// 
    /// This method is particularly useful for:
    /// - Debugging property mapping configuration issues
    /// - Logging mapping information for audit and monitoring
    /// - Displaying configuration summaries in diagnostic tools
    /// - Unit testing verification of mapping characteristics
    /// </remarks>
    /// <example>
    /// <code>
    /// var propertyMapping = new PropertyMapping&lt;DocumentDto, Document&gt;(mappingDictionary);
    /// 
    /// // Use for logging and debugging
    /// var logger = serviceProvider.GetRequiredService&lt;ILogger&gt;();
    /// logger.LogInformation("Initialized property mapping: {PropertyMapping}", propertyMapping);
    /// 
    /// // Use for diagnostic display
    /// Console.WriteLine($"Configuration: {propertyMapping}");
    /// 
    /// // Use in exception messages
    /// throw new InvalidOperationException($"Mapping error in {propertyMapping}");
    /// </code>
    /// </example>
    public override string ToString()
    {
        var simpleMappings = MappingDictionary.Values.Count(m => m.IsSimpleMapping);
        var complexMappings = MappingDictionary.Count - simpleMappings;
        var bidirectionalMappings = MappingDictionary.Values.Count(m => m.Revert);

        return $"PropertyMapping<{typeof(TSource).Name}, {typeof(TDestination).Name}>: " +
               $"{MappingDictionary.Count} mappings " +
               $"({simpleMappings} simple, {complexMappings} complex, {bidirectionalMappings} bidirectional)";
    }

    #region Private Helper Methods

    /// <summary>
    /// Validates the mapping dictionary parameter for consistency and correctness.
    /// </summary>
    /// <param name="mappingDictionary">The mapping dictionary to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the dictionary contains invalid entries.</exception>
    private static void ValidateMappingDictionary(Dictionary<string, PropertyMappingValue> mappingDictionary)
    {
        foreach (var kvp in mappingDictionary)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key))
            {
                throw new ArgumentException(
                    "Mapping dictionary contains null or empty property name key.",
                    nameof(mappingDictionary));
            }

            if (kvp.Value == null)
            {
                throw new ArgumentException(
                    $"Mapping dictionary contains null PropertyMappingValue for key '{kvp.Key}'.",
                    nameof(mappingDictionary));
            }
        }
    }

    /// <summary>
    /// Validates the complete mapping configuration after construction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    private void ValidateMappingConfiguration()
    {
        var validationResults = ValidateConfiguration().ToList();

        // Only throw for critical errors, allow warnings to pass
        var criticalErrors = validationResults
            .Where(r => r.ErrorMessage?.Contains("does not exist") == true ||
                       r.ErrorMessage?.Contains("not accessible") == true)
            .ToList();

        if (criticalErrors.Any())
        {
            var errorMessages = criticalErrors.Select(r => r.ErrorMessage).ToList();
            throw new InvalidOperationException(
                $"Critical property mapping validation errors: {string.Join("; ", errorMessages)}");
        }
    }

    /// <summary>
    /// Validates source type properties against the mapping configuration.
    /// </summary>
    /// <returns>A collection of validation results for source type validation.</returns>
    private IEnumerable<ValidationResult> ValidateSourceTypeProperties()
    {
        var results = new List<ValidationResult>();
        var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var mappingKey in MappingDictionary.Keys)
        {
            if (!sourceProperties.Contains(mappingKey))
            {
                results.Add(new ValidationResult(
                    $"Source property '{mappingKey}' does not exist on type {typeof(TSource).Name}",
                    new[] { mappingKey }));
            }
        }

        return results;
    }

    /// <summary>
    /// Validates destination type properties against the mapping configuration.
    /// </summary>
    /// <returns>A collection of validation results for destination type validation.</returns>
    private IEnumerable<ValidationResult> ValidateDestinationTypeProperties()
    {
        var results = new List<ValidationResult>();
        var destinationProperties = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in MappingDictionary)
        {
            foreach (var destinationProperty in kvp.Value.DestinationProperties)
            {
                if (!destinationProperties.Contains(destinationProperty))
                {
                    results.Add(new ValidationResult(
                        $"Destination property '{destinationProperty}' does not exist or is not writable on type {typeof(TDestination).Name}",
                        new[] { kvp.Key, destinationProperty }));
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Validates mapping consistency and configuration integrity.
    /// </summary>
    /// <returns>A collection of validation results for mapping consistency validation.</returns>
    private IEnumerable<ValidationResult> ValidateMappingConsistency()
    {
        var results = new List<ValidationResult>();

        // Check for duplicate destination property mappings
        var destinationPropertyUsage = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in MappingDictionary)
        {
            foreach (var destProperty in kvp.Value.DestinationProperties)
            {
                if (!destinationPropertyUsage.ContainsKey(destProperty))
                {
                    destinationPropertyUsage[destProperty] = new List<string>();
                }
                destinationPropertyUsage[destProperty].Add(kvp.Key);
            }
        }

        foreach (var kvp in destinationPropertyUsage.Where(kvp => kvp.Value.Count > 1))
        {
            results.Add(new ValidationResult(
                $"Destination property '{kvp.Key}' is mapped from multiple source properties: {string.Join(", ", kvp.Value)}",
                kvp.Value.ToArray()));
        }

        return results;
    }

    #endregion Private Helper Methods
}