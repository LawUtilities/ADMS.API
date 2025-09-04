using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Services;

/// <summary>
/// Defines the contract for strongly-typed property mapping configurations in the ADMS system.
/// </summary>
/// <remarks>
/// This interface provides a comprehensive contract for property mapping implementations including:
/// 
/// Core Property Mapping Capabilities:
/// - Type-safe property mapping between source DTOs and destination entities
/// - Immutable mapping configurations for thread safety and consistency
/// - Comprehensive validation and error handling for mapping operations
/// - High-performance property transformation with optimized lookup operations
/// - Support for complex property mappings including one-to-many scenarios
/// 
/// Property Mapping Features:
/// - Support for simple one-to-one property mappings (DTO.Name -> Entity.Name)
/// - Complex one-to-many property mappings (DTO.FullName -> Entity.FirstName, Entity.LastName)
/// - Bidirectional property mappings with revert support for inverse operations
/// - Case-insensitive property name matching for flexible mapping scenarios
/// - Comprehensive property validation against target type schemas
/// 
/// Thread Safety and Performance:
/// - Immutable mapping configurations prevent accidental modification after construction
/// - Thread-safe operations for concurrent access in multi-threaded scenarios
/// - Optimized property lookup operations with O(1) average complexity
/// - Memory-efficient storage using optimized collection types
/// - Cached property information for improved repeated access patterns
/// 
/// Integration and Framework Support:
/// - Seamless integration with Entity Framework Core for database operations
/// - Support for LINQ expression building for dynamic sorting and filtering
/// - Compatibility with mapping frameworks like AutoMapper for complex scenarios
/// - Integration with dependency injection containers for service lifetime management
/// - Support for API versioning with evolving mapping requirements
/// 
/// Validation and Error Handling:
/// - Comprehensive validation of mapping configurations during initialization
/// - Type-safe property mapping validation at compile time where possible
/// - Runtime validation of property mappings against target schemas
/// - Detailed error messages for troubleshooting mapping configuration issues
/// - Integration with logging infrastructure for debugging and monitoring
/// 
/// Security and Data Protection:
/// - Input validation and sanitization for property mapping operations
/// - Protection against property injection and mapping manipulation attacks
/// - Secure property name validation and filtering
/// - Integration with security policies and access control mechanisms
/// - Audit trail support for mapping operations and configuration changes
/// 
/// The interface is designed to support:
/// - Polymorphic usage in property mapping service collections
/// - Generic type constraints and compile-time type safety
/// - Runtime type information and dynamic property discovery
/// - Comprehensive property mapping validation and verification
/// - Integration with monitoring and observability frameworks
/// 
/// Implementation Considerations:
/// - Implementations must provide immutable mapping configurations
/// - Thread-safe operations are required for concurrent access scenarios
/// - Proper integration with logging infrastructure for diagnostic capabilities
/// - Comprehensive error handling without service interruption
/// - Support for both simple and complex property mapping scenarios
/// 
/// Common Usage Patterns:
/// - Polymorphic storage in property mapping service collections
/// - Type-safe property mapping operations with generic constraints
/// - Runtime property mapping validation and verification
/// - Integration with dependency injection and service composition
/// - Configuration-driven property mapping with validation support
/// 
/// The interface serves as the foundation for:
/// - API request/response transformation between DTOs and domain entities
/// - Database query result mapping from entities to presentation models
/// - Dynamic sorting and filtering operations with property name translation
/// - Data validation and transformation pipelines with type safety
/// - Cross-layer data transformation in layered architecture patterns
/// </remarks>
/// <example>
/// <code>
/// // Example polymorphic usage in a property mapping service
/// public class PropertyMappingService
/// {
///     private readonly List&lt;IPropertyMapping&gt; _mappings;
///     
///     public PropertyMappingService()
///     {
///         _mappings = new List&lt;IPropertyMapping&gt;
///         {
///             new PropertyMapping&lt;DocumentDto, Document&gt;(documentMappings),
///             new PropertyMapping&lt;MatterDto, Matter&gt;(matterMappings),
///             new PropertyMapping&lt;RevisionDto, Revision&gt;(revisionMappings)
///         };
///     }
///     
///     public PropertyMapping&lt;TSource, TDestination&gt; GetMapping&lt;TSource, TDestination&gt;()
///     {
///         return _mappings.OfType&lt;PropertyMapping&lt;TSource, TDestination&gt;&gt;().FirstOrDefault()
///             ?? throw new InvalidOperationException($"No mapping found for {typeof(TSource).Name} to {typeof(TDestination).Name}");
///     }
/// }
/// 
/// // Example validation usage
/// public class MappingValidator
/// {
///     public ValidationResult ValidateAllMappings(IEnumerable&lt;IPropertyMapping&gt; mappings)
///     {
///         var errors = new List&lt;string&gt;();
///         
///         foreach (var mapping in mappings)
///         {
///             // Validate each mapping configuration
///             var validationResults = mapping.ValidateConfiguration();
///             if (validationResults.Any())
///             {
///                 errors.AddRange(validationResults.Select(r =&gt; r.ErrorMessage));
///             }
///         }
///         
///         return errors.Any() 
///             ? ValidationResult.Failed(string.Join("; ", errors))
///             : ValidationResult.Success();
///     }
/// }
/// </code>
/// </example>
public interface IPropertyMapping
{
    #region Type Information

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
    /// - Integration with reflection-based mapping utilities
    /// 
    /// Common source types include:
    /// - Data Transfer Objects (DTOs) for API operations
    /// - View models for presentation layer operations
    /// - Request/response models for service integration
    /// - Configuration objects for system settings
    /// - Any .NET type with public properties for mapping
    /// </remarks>
    /// <value>The Type information for the source type in the mapping configuration.</value>
    /// <example>
    /// <code>
    /// // Example usage in mapping validation
    /// public bool ValidateSourceType(IPropertyMapping mapping, Type expectedType)
    /// {
    ///     return mapping.SourceType == expectedType;
    /// }
    /// 
    /// // Example usage in dynamic mapping scenarios
    /// public string GetSourceTypeName(IPropertyMapping mapping)
    /// {
    ///     return mapping.SourceType.Name; // e.g., "DocumentDto"
    /// }
    /// 
    /// // Example usage in mapping discovery
    /// public IEnumerable&lt;PropertyInfo&gt; GetSourceProperties(IPropertyMapping mapping)
    /// {
    ///     return mapping.SourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    /// }
    /// </code>
    /// </example>
    Type SourceType { get; }

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
    /// - Integration with Entity Framework and ORM operations
    /// 
    /// Common destination types include:
    /// - Domain entities for business logic operations
    /// - Database entities for persistence operations
    /// - Service models for inter-service communication
    /// - Configuration entities for system management
    /// - Any .NET type with public writable properties
    /// </remarks>
    /// <value>The Type information for the destination type in the mapping configuration.</value>
    /// <example>
    /// <code>
    /// // Example usage in entity validation
    /// public bool IsEntityMapping(IPropertyMapping mapping)
    /// {
    ///     return mapping.DestinationType.Assembly.GetName().Name.Contains("Entities");
    /// }
    /// 
    /// // Example usage in database mapping scenarios
    /// public string GetTableName(IPropertyMapping mapping)
    /// {
    ///     var tableAttribute = mapping.DestinationType.GetCustomAttribute&lt;TableAttribute&gt;();
    ///     return tableAttribute?.Name ?? mapping.DestinationType.Name;
    /// }
    /// 
    /// // Example usage in property validation
    /// public bool ValidateDestinationProperty(IPropertyMapping mapping, string propertyName)
    /// {
    ///     return mapping.DestinationType.GetProperty(propertyName) != null;
    /// }
    /// </code>
    /// </example>
    Type DestinationType { get; }

    #endregion Type Information

    #region Mapping Configuration

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
    /// - Keys: Source property names from the source type (case-insensitive)
    /// - Values: PropertyMappingValue instances containing destination property information
    /// - Supports one-to-one and one-to-many property mapping scenarios
    /// - Enables complex property transformations and bidirectional mappings
    /// 
    /// Usage Patterns:
    /// - Direct property lookup: MappingDictionary["PropertyName"]
    /// - Enumeration of all mappings: foreach(var kvp in MappingDictionary)
    /// - Existence checking: MappingDictionary.ContainsKey("PropertyName")
    /// - Integration with LINQ: MappingDictionary.Where(kvp => condition)
    /// 
    /// The dictionary is used extensively by:
    /// - Property mapping services for dynamic property transformation
    /// - LINQ expression builders for database query generation
    /// - Validation services for mapping configuration verification
    /// - Sorting and filtering operations for property name translation
    /// - Debugging and diagnostic tools for mapping analysis
    /// </remarks>
    /// <value>
    /// An immutable dictionary mapping source property names to PropertyMappingValue instances.
    /// The dictionary uses case-insensitive string comparison for robust property matching.
    /// </value>
    /// <example>
    /// <code>
    /// // Example property lookup
    /// public PropertyMappingValue? GetPropertyMapping(IPropertyMapping mapping, string propertyName)
    /// {
    ///     return mapping.MappingDictionary.TryGetValue(propertyName, out var value) ? value : null;
    /// }
    /// 
    /// // Example mapping enumeration
    /// public void LogAllMappings(IPropertyMapping mapping)
    /// {
    ///     foreach (var kvp in mapping.MappingDictionary)
    ///     {
    ///         _logger.LogInformation("Property {Source} maps to {Destinations}",
    ///             kvp.Key, string.Join(", ", kvp.Value.DestinationProperties));
    ///     }
    /// }
    /// 
    /// // Example LINQ usage for filtering
    /// public IEnumerable&lt;string&gt; GetBidirectionalProperties(IPropertyMapping mapping)
    /// {
    ///     return mapping.MappingDictionary
    ///         .Where(kvp =&gt; kvp.Value.Revert)
    ///         .Select(kvp =&gt; kvp.Key);
    /// }
    /// </code>
    /// </example>
    ImmutableDictionary<string, PropertyMappingValue> MappingDictionary { get; }

    /// <summary>
    /// Gets the count of property mappings defined in this configuration.
    /// </summary>
    /// <remarks>
    /// This convenience property provides:
    /// - Quick access to the number of mapped properties without dictionary enumeration
    /// - Performance metrics for mapping complexity analysis
    /// - Validation support for mapping completeness checking
    /// - Debugging information for mapping configuration inspection
    /// 
    /// The property count is useful for:
    /// - Performance optimization based on mapping complexity
    /// - Validation that required properties are mapped
    /// - Logging and monitoring of mapping configuration size
    /// - Unit testing verification of mapping completeness
    /// - Capacity planning for mapping operations
    /// 
    /// Typical property counts vary by mapping scenario:
    /// - Simple DTOs: 5-15 properties
    /// - Complex entities: 15-50 properties
    /// - Configuration objects: 3-20 properties
    /// - Integration models: 10-100+ properties
    /// </remarks>
    /// <value>
    /// The total number of property mappings in the mapping dictionary.
    /// Always non-negative and reflects the current mapping configuration size.
    /// </value>
    /// <example>
    /// <code>
    /// // Example usage in validation
    /// public ValidationResult ValidateMappingCompleteness(IPropertyMapping mapping)
    /// {
    ///     if (mapping.PropertyMappingCount == 0)
    ///     {
    ///         return ValidationResult.Failed("Property mapping contains no property mappings");
    ///     }
    ///     
    ///     return ValidationResult.Success();
    /// }
    /// 
    /// // Example usage in performance analysis
    /// public string CategorizeMappingComplexity(IPropertyMapping mapping)
    /// {
    ///     return mapping.PropertyMappingCount switch
    ///     {
    ///         &lt; 10 =&gt; "Simple",
    ///         &lt; 25 =&gt; "Moderate",
    ///         &lt; 50 =&gt; "Complex",
    ///         _ =&gt; "Very Complex"
    ///     };
    /// }
    /// 
    /// // Example usage in monitoring
    /// public void LogMappingStatistics(IEnumerable&lt;IPropertyMapping&gt; mappings)
    /// {
    ///     var totalMappings = mappings.Sum(m =&gt; m.PropertyMappingCount);
    ///     var averageMappings = mappings.Average(m =&gt; m.PropertyMappingCount);
    ///     
    ///     _logger.LogInformation("Mapping statistics: Total={Total}, Average={Average:F1}",
    ///         totalMappings, averageMappings);
    /// }
    /// </code>
    /// </example>
    int PropertyMappingCount { get; }

    /// <summary>
    /// Gets a value indicating whether this mapping configuration is empty (contains no property mappings).
    /// </summary>
    /// <remarks>
    /// This convenience property identifies:
    /// - Mapping configurations that contain no property mappings
    /// - Potential configuration errors or incomplete setup scenarios
    /// - Edge cases that require special handling in mapping operations
    /// - Validation scenarios for mapping completeness checking
    /// 
    /// Empty mappings might indicate:
    /// - Incomplete mapping configuration during development
    /// - Types with no compatible properties for automatic mapping
    /// - Configuration errors that need to be addressed
    /// - Special cases where no property transformation is needed
    /// - Mapping configurations that failed during initialization
    /// 
    /// The property is commonly used for:
    /// - Configuration validation during service initialization
    /// - Error detection and reporting in mapping setup
    /// - Conditional logic for handling empty mapping scenarios
    /// - Unit testing validation of mapping configuration
    /// - Diagnostic and troubleshooting scenarios
    /// </remarks>
    /// <value>
    /// true if the mapping configuration contains no property mappings;
    /// false if one or more property mappings are defined.
    /// </value>
    /// <example>
    /// <code>
    /// // Example validation usage
    /// public void ValidateMappingConfiguration(IEnumerable&lt;IPropertyMapping&gt; mappings)
    /// {
    ///     var emptyMappings = mappings.Where(m =&gt; m.IsEmpty).ToList();
    ///     
    ///     if (emptyMappings.Any())
    ///     {
    ///         var emptyTypes = emptyMappings.Select(m =&gt; $"{m.SourceType.Name}-&gt;{m.DestinationType.Name}");
    ///         _logger.LogWarning("Empty property mappings detected: {EmptyMappings}",
    ///             string.Join(", ", emptyTypes));
    ///     }
    /// }
    /// 
    /// // Example conditional processing
    /// public ProcessingResult ProcessMapping(IPropertyMapping mapping, object source)
    /// {
    ///     if (mapping.IsEmpty)
    ///     {
    ///         _logger.LogInformation("Skipping empty mapping for {SourceType}", 
    ///             mapping.SourceType.Name);
    ///         return ProcessingResult.Skipped("No mappings defined");
    ///     }
    ///     
    ///     return ProcessMappingWithProperties(mapping, source);
    /// }
    /// 
    /// // Example testing usage
    /// [Test]
    /// public void ValidateNoEmptyMappings()
    /// {
    ///     var mappings = _mappingService.GetAllMappings();
    ///     var emptyMappings = mappings.Where(m =&gt; m.IsEmpty);
    ///     
    ///     Assert.That(emptyMappings, Is.Empty, "No mappings should be empty after initialization");
    /// }
    /// </code>
    /// </example>
    bool IsEmpty { get; }

    #endregion Mapping Configuration

    #region Validation and Diagnostics

    /// <summary>
    /// Validates the entire property mapping configuration against the source and destination type schemas.
    /// </summary>
    /// <returns>
    /// An enumerable collection of ValidationResult objects describing any configuration issues found.
    /// An empty collection indicates that the mapping configuration is valid and ready for use.
    /// Each ValidationResult includes detailed error messages and affected property names for debugging.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive validation including:
    /// 
    /// Source Type Validation:
    /// - Verifies that all mapped source properties exist on the source type
    /// - Validates property accessibility and visibility requirements
    /// - Checks for deprecated or obsolete property usage
    /// - Ensures property name spelling and casing consistency
    /// - Validates property types and compatibility requirements
    /// 
    /// Destination Type Validation:
    /// - Confirms that all destination properties exist on the destination type
    /// - Validates property write accessibility for mapping operations
    /// - Checks type compatibility between source and destination properties
    /// - Ensures complex property paths are valid (e.g., "Address.Street")
    /// - Validates property constraints and business rule compliance
    /// 
    /// Mapping Configuration Validation:
    /// - Identifies duplicate property mappings and conflicting configurations
    /// - Validates bidirectional mapping configurations for consistency
    /// - Checks for circular references in complex property mappings
    /// - Ensures mapping completeness for critical properties
    /// - Validates mapping metadata and configuration parameters
    /// 
    /// Business Rule Validation:
    /// - Applies domain-specific validation rules where applicable
    /// - Validates against known property mapping anti-patterns
    /// - Checks for performance implications of complex mappings
    /// - Ensures security considerations are addressed
    /// - Validates compliance with organizational mapping standards
    /// 
    /// Validation Result Format:
    /// - Detailed error descriptions for each validation failure
    /// - Property names and context information for troubleshooting
    /// - Severity levels (errors, warnings, informational messages)
    /// - Recommended actions for resolving configuration issues
    /// - Integration with structured logging and monitoring systems
    /// 
    /// The validation is particularly useful for:
    /// - Unit testing of property mapping configurations
    /// - Development-time validation of mapping completeness
    /// - Configuration validation during application startup
    /// - Debugging complex property mapping scenarios
    /// - Continuous integration validation pipelines
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when validation process itself fails due to reflection errors,
    /// type loading issues, or other infrastructure problems.
    /// The original exception is included as the inner exception for debugging.
    /// </exception>
    /// <example>
    /// <code>
    /// // Example validation in service initialization
    /// public class PropertyMappingService
    /// {
    ///     public void ValidateAllMappings()
    ///     {
    ///         foreach (var mapping in _registeredMappings)
    ///         {
    ///             var validationResults = mapping.ValidateConfiguration();
    ///             
    ///             if (validationResults.Any())
    ///             {
    ///                 var errors = validationResults.Select(r =&gt; r.ErrorMessage);
    ///                 _logger.LogError("Mapping validation failed for {SourceType}-&gt;{DestinationType}: {Errors}",
    ///                     mapping.SourceType.Name, mapping.DestinationType.Name, string.Join("; ", errors));
    ///                 
    ///                 throw new InvalidOperationException($"Invalid mapping configuration detected: {string.Join("; ", errors)}");
    ///             }
    ///         }
    ///     }
    /// }
    /// 
    /// // Example unit testing validation
    /// [Test]
    /// public void ValidateMappingConfiguration()
    /// {
    ///     var mapping = new PropertyMapping&lt;DocumentDto, Document&gt;(testMappings);
    ///     var validationResults = mapping.ValidateConfiguration();
    ///     
    ///     Assert.That(validationResults, Is.Empty, 
    ///         $"Mapping validation failed: {string.Join(", ", validationResults.Select(r =&gt; r.ErrorMessage))}");
    /// }
    /// 
    /// // Example diagnostic validation
    /// public class MappingDiagnosticService
    /// {
    ///     public DiagnosticReport GenerateValidationReport(IEnumerable&lt;IPropertyMapping&gt; mappings)
    ///     {
    ///         var report = new DiagnosticReport();
    ///         
    ///         foreach (var mapping in mappings)
    ///         {
    ///             var validationResults = mapping.ValidateConfiguration();
    ///             
    ///             if (validationResults.Any())
    ///             {
    ///                 report.AddMappingIssues(mapping.SourceType, mapping.DestinationType, validationResults);
    ///             }
    ///             else
    ///             {
    ///                 report.AddValidMapping(mapping.SourceType, mapping.DestinationType);
    ///             }
    ///         }
    ///         
    ///         return report;
    ///     }
    /// }
    /// </code>
    /// </example>
    IEnumerable<ValidationResult> ValidateConfiguration();

    #endregion Validation and Diagnostics

    #region String Representation and Debugging

    /// <summary>
    /// Returns a comprehensive string representation of the property mapping configuration.
    /// </summary>
    /// <returns>
    /// A detailed string containing summary information about the property mapping configuration
    /// in a human-readable format suitable for debugging, logging, and diagnostic purposes.
    /// </returns>
    /// <remarks>
    /// The string representation includes:
    /// - Source and destination type names for context identification
    /// - Total count of property mappings for configuration overview
    /// - Summary of mapping characteristics (simple, complex, bidirectional)
    /// - Memory and performance characteristics for optimization insights
    /// - Configuration health and validation status
    /// 
    /// The formatted output provides:
    /// - Consistent formatting for logging and monitoring scenarios
    /// - Sufficient detail for debugging configuration issues
    /// - Concise summary suitable for diagnostic displays
    /// - Integration with standard .NET logging frameworks
    /// - Structured information for automated parsing and analysis
    /// 
    /// Example output formats:
    /// - "PropertyMapping&lt;DocumentDto, Document&gt;: 8 mappings (6 simple, 2 complex, 1 bidirectional)"
    /// - "PropertyMapping&lt;MatterDto, Matter&gt;: 12 mappings (10 simple, 2 complex)"
    /// - "PropertyMapping&lt;RevisionDto, Revision&gt;: Empty mapping configuration"
    /// 
    /// This method is particularly useful for:
    /// - Debugging property mapping configuration issues
    /// - Logging mapping information for audit and monitoring
    /// - Displaying configuration summaries in diagnostic tools
    /// - Unit testing verification of mapping characteristics
    /// - Integration with application performance monitoring
    /// 
    /// Integration scenarios:
    /// - Structured logging with mapping context information
    /// - Exception messages with detailed mapping information
    /// - Diagnostic displays and configuration summaries
    /// - Performance monitoring and optimization analysis
    /// - Automated testing and validation reporting
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example logging integration
    /// public void LogMappingConfiguration(IPropertyMapping mapping)
    /// {
    ///     _logger.LogInformation("Initialized property mapping: {Mapping}", mapping);
    ///     // Output: "Initialized property mapping: PropertyMapping&lt;DocumentDto, Document&gt;: 8 mappings (6 simple, 2 complex)"
    /// }
    /// 
    /// // Example exception handling
    /// public void ValidateMapping(IPropertyMapping mapping)
    /// {
    ///     try
    ///     {
    ///         var validationResults = mapping.ValidateConfiguration();
    ///         if (validationResults.Any())
    ///         {
    ///             throw new InvalidOperationException($"Mapping validation failed for {mapping}");
    ///         }
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         _logger.LogError(ex, "Error validating mapping: {Mapping}", mapping);
    ///         throw;
    ///     }
    /// }
    /// 
    /// // Example diagnostic collection
    /// public class MappingDiagnosticCollector
    /// {
    ///     public void CollectMappingDiagnostics(IEnumerable&lt;IPropertyMapping&gt; mappings)
    ///     {
    ///         var diagnostics = mappings.Select(m =&gt; new
    ///         {
    ///             MappingInfo = m.ToString(),
    ///             PropertyCount = m.PropertyMappingCount,
    ///             IsEmpty = m.IsEmpty,
    ///             SourceType = m.SourceType.Name,
    ///             DestinationType = m.DestinationType.Name
    ///         });
    ///         
    ///         _logger.LogInformation("Mapping diagnostics: {@Diagnostics}", diagnostics);
    ///     }
    /// }
    /// </code>
    /// </example>
    string ToString();

    #endregion String Representation and Debugging
}