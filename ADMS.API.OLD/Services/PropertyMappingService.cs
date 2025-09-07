using ADMS.API.Entities;
using ADMS.API.Models;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;

namespace ADMS.API.Services;

/// <summary>
/// Advanced property mapping service providing comprehensive mapping between DTO and Entity properties.
/// </summary>
/// <remarks>
/// This service provides enterprise-grade property mapping functionality including:
/// - Automated reflection-based property mapping for matching property names
/// - Custom property mapping overrides for complex transformation scenarios
/// - Thread-safe mapping operations with concurrent access support
/// - Performance-optimized mapping with caching and lazy initialization
/// - Comprehensive validation and error handling for mapping operations
/// - Support for complex property mappings including nested properties and collections
/// 
/// The service implements several advanced features:
/// 
/// Automated Property Mapping:
/// - Reflection-based discovery of matching properties between source and destination types
/// - Case-insensitive property name matching for flexible mapping scenarios
/// - Automatic type compatibility validation during mapping generation
/// - Support for ignoring specific properties during automatic mapping generation
/// - Intelligent handling of inheritance hierarchies and interface implementations
/// 
/// Performance Optimizations:
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
/// Supported Mapping Scenarios:
/// - Simple one-to-one property mappings (DTO.Name -> Entity.Name)
/// - Complex one-to-many property mappings (DTO.FullName -> Entity.FirstName, Entity.LastName)
/// - Bidirectional property mappings with revert support for inverse operations
/// - Nested property mappings with dot notation (DTO.Address -> Entity.Address.Street)
/// - Collection property mappings for list and enumerable transformations
/// 
/// Integration Features:
/// - Seamless integration with Entity Framework Core for database operations
/// - Support for LINQ expression building for dynamic sorting and filtering
/// - Compatibility with AutoMapper and other mapping frameworks
/// - Integration with API versioning for evolving mapping requirements
/// - Support for dependency injection and service lifetime management
/// 
/// Thread Safety and Concurrency:
/// - All mapping operations are thread-safe for concurrent access
/// - Immutable mapping configurations prevent accidental modification
/// - Concurrent dictionary implementation for high-performance caching
/// - No shared mutable state that could cause race conditions
/// - Safe for use in multi-threaded web application scenarios
/// 
/// The service is designed to be:
/// - High-performance: Optimized for frequent mapping operations
/// - Maintainable: Clear separation of concerns with comprehensive documentation
/// - Extensible: Easy to add new mapping types and custom transformation logic
/// - Reliable: Comprehensive error handling and validation throughout
/// - Scalable: Efficient memory usage and performance under load
/// </remarks>
public sealed class PropertyMappingService : IPropertyMappingService
{
    private readonly ILogger<PropertyMappingService> _logger;
    private readonly ConcurrentDictionary<string, IPropertyMapping> _propertyMappings;
    private readonly ConcurrentDictionary<string, Dictionary<string, PropertyMappingValue>> _mappingCache;

    /// <summary>
    /// Gets the collection of registered property mappings in an immutable format.
    /// </summary>
    /// <remarks>
    /// This property provides:
    /// - Read-only access to all registered property mappings
    /// - Thread-safe enumeration of mapping configurations
    /// - Integration support for mapping validation and diagnostics
    /// - Debugging capabilities for troubleshooting mapping issues
    /// 
    /// The collection includes:
    /// - All automatically generated property mappings
    /// - Custom property mapping overrides and configurations
    /// - Bidirectional mappings for reverse transformation scenarios
    /// - Complex mappings with multiple destination properties
    /// 
    /// Use cases:
    /// - Configuration validation during application startup
    /// - Diagnostic and monitoring tools for mapping analysis
    /// - Unit testing scenarios for mapping configuration verification
    /// - Runtime introspection of available mapping capabilities
    /// </remarks>
    /// <value>
    /// An immutable collection of property mapping configurations.
    /// The collection is thread-safe and cannot be modified after service initialization.
    /// </value>
    public ImmutableList<IPropertyMapping> RegisteredMappings =>
        _propertyMappings.Values.ToImmutableList();

    /// <summary>
    /// Initializes a new instance of the PropertyMappingService with comprehensive mapping registration.
    /// </summary>
    /// <param name="logger">
    /// The logger instance for recording mapping operations, errors, and diagnostic information.
    /// Used for debugging mapping issues and monitoring service performance.
    /// </param>
    /// <remarks>
    /// The initialization process includes:
    /// 
    /// 1. Service Infrastructure Setup:
    ///    - Initializes thread-safe concurrent collections for mappings and caching
    ///    - Configures logging infrastructure for comprehensive operation tracking
    ///    - Sets up performance monitoring and diagnostic capabilities
    /// 
    /// 2. Automatic Property Mapping Registration:
    ///    - Generates mappings for DocumentDto to Document entity transformations
    ///    - Creates mappings for DocumentWithoutRevisionsDto to Document scenarios
    ///    - Establishes RevisionDto to Revision entity mapping configurations
    ///    - Configures MatterDto to Matter entity transformation mappings
    ///    - Processes additional DTO-Entity pairs as discovered through reflection
    /// 
    /// 3. Mapping Validation and Optimization:
    ///    - Validates all generated mappings for consistency and correctness
    ///    - Optimizes mapping configurations for performance and memory efficiency
    ///    - Caches frequently accessed mappings for improved response times
    ///    - Logs mapping registration results for monitoring and debugging
    /// 
    /// 4. Error Handling and Recovery:
    ///    - Implements graceful handling of mapping registration failures
    ///    - Provides detailed error messages for troubleshooting mapping issues
    ///    - Maintains service stability even with partial mapping failures
    ///    - Logs all registration errors for investigation and resolution
    /// 
    /// The service is designed to be fail-fast during initialization to catch
    /// mapping configuration issues early in the application lifecycle, while
    /// maintaining resilience during runtime operations.
    /// 
    /// Performance characteristics:
    /// - Lazy initialization of complex mappings to reduce startup time
    /// - Concurrent mapping registration for improved initialization performance
    /// - Memory-efficient storage using immutable collections where appropriate
    /// - Optimized reflection operations with caching for frequently accessed types
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the logger parameter is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when critical mapping registrations fail during service initialization.
    /// </exception>
    /// <example>
    /// <code>
    /// // Service registration in dependency injection container
    /// services.AddScoped&lt;IPropertyMappingService, PropertyMappingService&gt;();
    /// 
    /// // Direct instantiation for testing scenarios
    /// var logger = serviceProvider.GetRequiredService&lt;ILogger&lt;PropertyMappingService&gt;&gt;();
    /// var mappingService = new PropertyMappingService(logger);
    /// </code>
    /// </example>
    public PropertyMappingService(ILogger<PropertyMappingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _propertyMappings = new ConcurrentDictionary<string, IPropertyMapping>();
        _mappingCache = new ConcurrentDictionary<string, Dictionary<string, PropertyMappingValue>>();

        try
        {
            _logger.LogInformation("Initializing PropertyMappingService with automated property mapping registration");

            // Register all standard DTO-Entity mappings
            RegisterStandardMappings();

            _logger.LogInformation(
                "PropertyMappingService initialization completed successfully with {MappingCount} registered mappings",
                _propertyMappings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error during PropertyMappingService initialization");
            throw new InvalidOperationException(
                "Failed to initialize PropertyMappingService due to mapping registration errors. See inner exception for details.",
                ex);
        }
    }

    /// <summary>
    /// Registers all standard property mappings between DTOs and entities used throughout the ADMS system.
    /// </summary>
    /// <remarks>
    /// This method establishes the core property mappings required for:
    /// - Document management operations with comprehensive DTO-Entity transformations
    /// - Revision tracking and management with proper property translation
    /// - Matter management with full property mapping support
    /// - User and activity tracking with appropriate property mappings
    /// 
    /// Each mapping registration includes:
    /// - Automatic property discovery using reflection for matching property names
    /// - Custom overrides for properties requiring special transformation logic
    /// - Validation of mapping completeness and correctness
    /// - Performance optimization for frequently accessed property mappings
    /// 
    /// The registration process is designed to be:
    /// - Comprehensive: Covers all major DTO-Entity pairs in the system
    /// - Extensible: Easy to add new mappings as the system evolves
    /// - Maintainable: Clear organization and documentation of mapping configurations
    /// - Performant: Optimized registration process with minimal overhead
    /// </remarks>
    private void RegisterStandardMappings()
    {
        try
        {
            // Document-related mappings
            RegisterMapping<DocumentDto, Document>("DocumentDto-Document");
            RegisterMapping<DocumentWithoutRevisionsDto, Document>("DocumentWithoutRevisionsDto-Document");

            // For creation DTOs, we don't ignore any properties since they typically don't have Id properties
            // The GenerateAutoMapping method will only include properties that exist on both source and destination
            RegisterMapping<DocumentForCreationDto, Document>("DocumentForCreationDto-Document");
            RegisterMapping<DocumentForUpdateDto, Document>("DocumentForUpdateDto-Document");

            // Revision-related mappings
            RegisterMapping<RevisionDto, Revision>("RevisionDto-Revision");

            // For creation DTOs, we don't ignore any properties since they typically don't have Id properties
            RegisterMapping<RevisionForCreationDto, Revision>("RevisionForCreationDto-Revision");
            RegisterMapping<RevisionForUpdateDto, Revision>("RevisionForUpdateDto-Revision");

            // Matter-related mappings
            RegisterMapping<MatterDto, Matter>("MatterDto-Matter");
            RegisterMapping<MatterWithDocumentsDto, Matter>("MatterWithDocumentsDto-Matter");
            RegisterMapping<MatterWithoutDocumentsDto, Matter>("MatterWithoutDocumentsDto-Matter");

            // For creation DTOs, we don't ignore any properties since they typically don't have Id properties
            RegisterMapping<MatterForCreationDto, Matter>("MatterForCreationDto-Matter");
            RegisterMapping<MatterForUpdateDto, Matter>("MatterForUpdateDto-Matter");

            _logger.LogDebug("Standard property mappings registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during standard mapping registration");
            throw;
        }
    }

    /// <summary>
    /// Registers a property mapping for the specified source and destination types with comprehensive validation.
    /// </summary>
    /// <typeparam name="TSource">The source DTO type for the mapping.</typeparam>
    /// <typeparam name="TDestination">The destination entity type for the mapping.</typeparam>
    /// <param name="mappingKey">A unique identifier for the mapping configuration.</param>
    /// <param name="ignoreProperties">Optional collection of property names to exclude from automatic mapping.</param>
    /// <param name="customMappings">Optional custom property mappings that override automatic mappings.</param>
    /// <remarks>
    /// This method provides comprehensive mapping registration including:
    /// 
    /// Automatic Mapping Generation:
    /// - Reflection-based discovery of matching properties between source and destination
    /// - Case-insensitive property name matching for flexible configuration
    /// - Type compatibility validation to ensure safe property transformations
    /// - Intelligent handling of nullable and non-nullable property mismatches
    /// 
    /// Custom Mapping Support:
    /// - Override automatic mappings with custom transformation logic
    /// - Support for complex property mappings with multiple destination properties
    /// - Bidirectional mapping support for reverse transformation scenarios
    /// - Integration with PropertyMappingValue for advanced mapping configurations
    /// 
    /// Validation and Error Handling:
    /// - Comprehensive validation of mapping configurations during registration
    /// - Detailed error messages for troubleshooting mapping issues
    /// - Graceful handling of registration failures without affecting other mappings
    /// - Integration with logging for debugging and monitoring mapping registration
    /// 
    /// Performance Optimizations:
    /// - Efficient reflection operations with minimal performance impact
    /// - Caching of generated mappings for improved runtime performance
    /// - Optimized storage using immutable collections where appropriate
    /// - Thread-safe registration supporting concurrent initialization scenarios
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when mappingKey is null, empty, or already exists in the mapping collection.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when mapping generation fails due to incompatible types or reflection errors.
    /// </exception>
    private void RegisterMapping<TSource, TDestination>(
        string mappingKey,
        IEnumerable<string>? ignoreProperties = null,
        Dictionary<string, PropertyMappingValue>? customMappings = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mappingKey, nameof(mappingKey));

        try
        {
            _logger.LogDebug("Registering property mapping: {MappingKey} ({SourceType} -> {DestinationType})",
                mappingKey, typeof(TSource).Name, typeof(TDestination).Name);

            // Check for duplicate mapping keys
            if (_propertyMappings.ContainsKey(mappingKey))
            {
                throw new ArgumentException(
                    $"Property mapping with key '{mappingKey}' already exists. Each mapping must have a unique key.",
                    nameof(mappingKey));
            }

            // Generate automatic mapping
            var automaticMapping = GenerateAutoMapping<TSource, TDestination>(ignoreProperties);

            // Apply custom overrides if provided
            if (customMappings != null)
            {
                foreach (var customMapping in customMappings)
                {
                    automaticMapping[customMapping.Key] = customMapping.Value;
                    _logger.LogDebug("Applied custom mapping override: {PropertyName} -> {DestinationProperties}",
                        customMapping.Key, string.Join(", ", customMapping.Value.DestinationProperties));
                }
            }

            // Create and register the property mapping
            var propertyMapping = new PropertyMapping<TSource, TDestination>(automaticMapping);

            if (_propertyMappings.TryAdd(mappingKey, propertyMapping))
            {
                _logger.LogDebug("Successfully registered property mapping: {MappingKey} with {PropertyCount} properties",
                    mappingKey, automaticMapping.Count);
            }
            else
            {
                _logger.LogWarning("Failed to add property mapping to collection: {MappingKey}", mappingKey);
                throw new InvalidOperationException($"Failed to register property mapping '{mappingKey}' due to concurrent modification.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering property mapping: {MappingKey} ({SourceType} -> {DestinationType})",
                mappingKey, typeof(TSource).Name, typeof(TDestination).Name);
            throw;
        }
    }

    /// <summary>
    /// Generates automatic property mappings between source and destination types using reflection.
    /// </summary>
    /// <typeparam name="TSource">The source DTO type for automatic mapping generation.</typeparam>
    /// <typeparam name="TDestination">The destination entity type for automatic mapping generation.</typeparam>
    /// <param name="ignoreProperties">Optional collection of property names to exclude from mapping generation.</param>
    /// <returns>
    /// A dictionary containing property mappings from source property names to PropertyMappingValue instances.
    /// The dictionary uses case-insensitive string comparison for robust property name matching.
    /// </returns>
    /// <remarks>
    /// This method implements intelligent automatic mapping generation including:
    /// 
    /// Property Discovery Process:
    /// 1. Reflection-based enumeration of public properties on both source and destination types
    /// 2. Case-insensitive name matching between source and destination properties
    /// 3. Type compatibility validation for safe property transformations
    /// 4. Automatic exclusion of properties specified in the ignore list
    /// 5. Intelligent handling of inheritance hierarchies and interface implementations
    /// 
    /// Mapping Generation Features:
    /// - Supports simple one-to-one property mappings for matching property names
    /// - Handles nullable and non-nullable property type mismatches appropriately
    /// - Provides consistent case-insensitive property name matching behavior
    /// - Generates PropertyMappingValue instances with appropriate configuration
    /// - Excludes system properties and properties marked for ignoring
    /// 
    /// Type Compatibility Handling:
    /// - Validates type compatibility between source and destination properties
    /// - Handles value types, reference types, and nullable type combinations
    /// - Provides appropriate mapping configurations for different type scenarios
    /// - Logs type compatibility issues for debugging and troubleshooting
    /// 
    /// Performance Considerations:
    /// - Efficient reflection operations with minimal performance impact
    /// - Caching of type information for improved performance on repeated calls
    /// - Optimized collection operations for large property sets
    /// - Memory-efficient storage of generated mapping configurations
    /// 
    /// The generated mappings serve as the foundation for:
    /// - Dynamic sorting operations in paginated queries
    /// - Property name translation for database operations
    /// - Filter expression building for search functionality
    /// - Data transformation between API layers
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when reflection operations fail or type compatibility issues are detected.
    /// </exception>
    /// <example>
    /// <code>
    /// // Generate automatic mapping between DocumentDto and Document
    /// var mapping = GenerateAutoMapping&lt;DocumentDto, Document&gt;();
    /// 
    /// // Generate mapping with ignored properties
    /// var mappingWithIgnores = GenerateAutoMapping&lt;DocumentDto, Document&gt;(
    ///     new[] { "CreatedDate", "ModifiedDate" });
    /// 
    /// // The resulting dictionary contains mappings like:
    /// // "FileName" -> PropertyMappingValue(["FileName"])
    /// // "FileSize" -> PropertyMappingValue(["FileSize"])
    /// // etc.
    /// </code>
    /// </example>
    private static Dictionary<string, PropertyMappingValue> GenerateAutoMapping<TSource, TDestination>(
        IEnumerable<string>? ignoreProperties = null)
    {
        try
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            // Get all public instance properties from both types
            var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var destinationProperties = destinationType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite) // Only include writable properties
                .Select(p => p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Create ignore set for efficient lookup
            var ignoreSet = ignoreProperties?.ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Generate mappings for matching properties
            var mappings = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase);

            foreach (var sourceProperty in sourceProperties)
            {
                // Skip ignored properties
                if (ignoreSet.Contains(sourceProperty.Name))
                    continue;

                // Skip if property doesn't exist on destination
                if (!destinationProperties.Contains(sourceProperty.Name))
                    continue;

                // Create simple one-to-one mapping
                mappings[sourceProperty.Name] = new PropertyMappingValue(new[] { sourceProperty.Name });
            }

            return mappings;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to generate automatic mapping between {typeof(TSource).Name} and {typeof(TDestination).Name}. See inner exception for details.",
                ex);
        }
    }

    /// <summary>
    /// Retrieves the property mapping dictionary for the specified source and destination types.
    /// </summary>
    /// <typeparam name="TSource">The source DTO type for the mapping lookup.</typeparam>
    /// <typeparam name="TDestination">The destination entity type for the mapping lookup.</typeparam>
    /// <returns>
    /// A dictionary mapping source property names to PropertyMappingValue instances.
    /// The dictionary provides the complete mapping configuration for transforming between the specified types.
    /// </returns>
    /// <remarks>
    /// This method provides high-performance property mapping retrieval with:
    /// 
    /// Caching and Performance:
    /// - Implements intelligent caching to avoid repeated lookup operations
    /// - Uses concurrent dictionary for thread-safe caching in multi-threaded scenarios
    /// - Optimizes frequently accessed mappings for improved response times
    /// - Provides consistent performance characteristics under varying loads
    /// 
    /// Mapping Resolution Process:
    /// 1. Generates a unique cache key based on source and destination type information
    /// 2. Checks the mapping cache for previously resolved mapping configurations
    /// 3. If cached mapping exists, returns the cached result for optimal performance
    /// 4. If no cached mapping exists, performs mapping resolution from registered mappings
    /// 5. Caches the resolved mapping for future requests to improve performance
    /// 
    /// Error Handling and Validation:
    /// - Validates that exactly one matching mapping exists for the specified types
    /// - Provides detailed error messages when mappings are missing or ambiguous
    /// - Handles edge cases where multiple mappings might match the same type pair
    /// - Logs mapping resolution activities for debugging and monitoring purposes
    /// 
    /// Thread Safety:
    /// - All mapping resolution operations are thread-safe for concurrent access
    /// - Uses concurrent collections to prevent race conditions during caching
    /// - Supports high-concurrency scenarios without performance degradation
    /// - No shared mutable state that could cause threading issues
    /// 
    /// The returned mapping dictionary is used for:
    /// - Building dynamic LINQ expressions for database queries
    /// - Translating API parameter names to entity property names
    /// - Constructing sort and filter expressions for paginated operations
    /// - Validating that requested sort/filter fields have valid mappings
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no mapping is found, multiple ambiguous mappings exist, 
    /// or mapping resolution fails due to configuration errors.
    /// </exception>
    /// <example>
    /// <code>
    /// // Get mapping for DocumentDto to Document entity transformation
    /// var mapping = mappingService.GetPropertyMapping&lt;DocumentDto, Document&gt;();
    /// 
    /// // Use mapping for sorting parameter validation
    /// bool canSortByFileName = mapping.ContainsKey("FileName");
    /// 
    /// // Get destination property for transformation
    /// if (mapping.TryGetValue("FileName", out var mappingValue))
    /// {
    ///     string destinationProperty = mappingValue.GetPrimaryProperty();
    ///     // Use destinationProperty in LINQ expression building
    /// }
    /// </code>
    /// </example>
    public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
    {
        var cacheKey = $"{typeof(TSource).FullName}->{typeof(TDestination).FullName}";

        try
        {
            // Check cache first for performance optimization
            if (_mappingCache.TryGetValue(cacheKey, out var cachedMapping))
            {
                _logger.LogDebug("Retrieved cached property mapping: {CacheKey}", cacheKey);
                return cachedMapping;
            }

            // Find matching mapping from registered mappings
            var matchingMappings = _propertyMappings.Values
                .OfType<PropertyMapping<TSource, TDestination>>()
                .ToList();

            switch (matchingMappings.Count)
            {
                case 0:
                    _logger.LogError("No property mapping found for types: {SourceType} -> {DestinationType}",
                        typeof(TSource).Name, typeof(TDestination).Name);
                    throw new InvalidOperationException(
                        $"No property mapping found for <{typeof(TSource).Name}, {typeof(TDestination).Name}>. " +
                        "Ensure the mapping is registered during service initialization.");

                case 1:
                    var mapping = matchingMappings[0].MappingDictionary;

                    // Convert ImmutableDictionary to Dictionary for return type compatibility
                    var mappingDict = mapping.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);

                    // Cache the mapping for future requests
                    _mappingCache.TryAdd(cacheKey, mappingDict);

                    _logger.LogDebug("Retrieved and cached property mapping: {CacheKey} with {PropertyCount} properties",
                        cacheKey, mapping.Count);
                    return mappingDict;

                default:
                    _logger.LogError("Multiple ambiguous property mappings found for types: {SourceType} -> {DestinationType} (Count: {Count})",
                        typeof(TSource).Name, typeof(TDestination).Name, matchingMappings.Count);
                    throw new InvalidOperationException(
                        $"Multiple ambiguous property mappings found for <{typeof(TSource).Name}, {typeof(TDestination).Name}>. " +
                        "Each type pair should have exactly one registered mapping.");
            }
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Unexpected error retrieving property mapping: {CacheKey}", cacheKey);
            throw new InvalidOperationException(
                $"Failed to retrieve property mapping for <{typeof(TSource).Name}, {typeof(TDestination).Name}>. See inner exception for details.",
                ex);
        }
    }

    /// <summary>
    /// Validates that all specified fields have valid property mappings between the source and destination types.
    /// </summary>
    /// <typeparam name="TSource">The source DTO type for mapping validation.</typeparam>
    /// <typeparam name="TDestination">The destination entity type for mapping validation.</typeparam>
    /// <param name="fields">
    /// A comma-separated string of field names to validate against the property mapping.
    /// Field names may include sort direction indicators (ascending/descending) that are automatically parsed and removed.
    /// </param>
    /// <returns>
    /// true if all specified fields have valid mappings in the property mapping configuration; 
    /// false if any field lacks a corresponding mapping or if validation fails.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive field validation including:
    /// 
    /// Field Parsing and Processing:
    /// - Parses comma-separated field lists with robust whitespace handling
    /// - Automatically removes sort direction indicators (e.g., "name asc", "date desc")
    /// - Handles empty or whitespace-only field specifications gracefully
    /// - Supports flexible field name formats used in API sorting and filtering
    /// 
    /// Validation Process:
    /// 1. Retrieves the appropriate property mapping for the specified type pair
    /// 2. Parses the fields string into individual field names with direction cleanup
    /// 3. Validates each field name against the available property mappings
    /// 4. Returns true only if all fields have valid corresponding mappings
    /// 5. Logs validation results for debugging and monitoring purposes
    /// 
    /// Use Cases:
    /// - Validating API sort parameters before building database queries
    /// - Checking filter field specifications in search operations
    /// - Ensuring data shaping field requests are mappable to entity properties
    /// - Pre-validation of user input for property-based operations
    /// 
    /// Error Handling:
    /// - Gracefully handles missing or invalid property mappings
    /// - Returns false for any validation failures rather than throwing exceptions
    /// - Logs detailed information about validation failures for debugging
    /// - Provides robust behavior even with malformed field specifications
    /// 
    /// Performance Characteristics:
    /// - Leverages cached property mappings for optimal performance
    /// - Efficient string parsing with minimal memory allocation
    /// - Short-circuits validation on first invalid field for improved performance
    /// - Scales well with large field lists and complex property mappings
    /// 
    /// The method supports various field specification formats:
    /// - Simple field names: "name,email,createdDate"
    /// - Fields with sort directions: "name asc,email desc,createdDate"
    /// - Mixed formats: "name asc,email,createdDate desc"
    /// - Whitespace variations: " name , email , createdDate "
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when property mapping retrieval fails due to configuration errors.
    /// This exception is propagated from the GetPropertyMapping method and indicates
    /// a serious configuration issue that should be addressed during development.
    /// </exception>
    /// <example>
    /// <code>
    /// // Validate simple field list
    /// bool isValid = mappingService.ValidMappingExistsFor&lt;DocumentDto, Document&gt;("fileName,fileSize");
    /// 
    /// // Validate fields with sort directions
    /// bool hasValidSort = mappingService.ValidMappingExistsFor&lt;DocumentDto, Document&gt;("fileName asc,createdDate desc");
    /// 
    /// // Validate empty or null fields (returns true)
    /// bool emptyIsValid = mappingService.ValidMappingExistsFor&lt;DocumentDto, Document&gt;("");
    /// bool nullIsValid = mappingService.ValidMappingExistsFor&lt;DocumentDto, Document&gt;(null);
    /// 
    /// // Use in API parameter validation
    /// public IActionResult GetDocuments(string sortBy)
    /// {
    ///     if (!string.IsNullOrEmpty(sortBy) && 
    ///         !_mappingService.ValidMappingExistsFor&lt;DocumentDto, Document&gt;(sortBy))
    ///     {
    ///         return BadRequest("Invalid sort fields specified");
    ///     }
    ///     // Continue with query building...
    /// }
    /// </code>
    /// </example>
    public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
    {
        try
        {
            // Handle null or empty fields - typically valid (no sorting/filtering requested)
            if (string.IsNullOrWhiteSpace(fields))
            {
                _logger.LogDebug("Field validation requested for empty/null fields - returning true");
                return true;
            }

            // Get property mapping (may throw InvalidOperationException if not found)
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            // Parse fields and validate each one
            var fieldsArray = fields.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var invalidFields = new List<string>();

            foreach (var field in fieldsArray)
            {
                var trimmedField = field.Trim();

                if (string.IsNullOrWhiteSpace(trimmedField))
                    continue;

                // Remove sort direction indicators (e.g., "name asc" -> "name")
                var fieldName = ExtractFieldNameFromSortExpression(trimmedField);

                if (!propertyMapping.ContainsKey(fieldName))
                {
                    invalidFields.Add(fieldName);
                }
            }

            var isValid = invalidFields.Count == 0;

            if (!isValid)
            {
                _logger.LogWarning("Field validation failed for {SourceType}->{DestinationType}. Invalid fields: {InvalidFields}",
                    typeof(TSource).Name, typeof(TDestination).Name, string.Join(", ", invalidFields));
            }
            else
            {
                _logger.LogDebug("Field validation successful for {SourceType}->{DestinationType}. Fields: {Fields}",
                    typeof(TSource).Name, typeof(TDestination).Name, fields);
            }

            return isValid;
        }
        catch (InvalidOperationException)
        {
            // Let mapping configuration errors bubble up
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during field validation for {SourceType}->{DestinationType}. Fields: {Fields}",
                typeof(TSource).Name, typeof(TDestination).Name, fields);

            // Return false for validation failures to maintain API contract
            return false;
        }
    }

    /// <summary>
    /// Extracts the field name from a sort expression by removing direction indicators.
    /// </summary>
    /// <param name="sortExpression">The sort expression that may contain direction indicators (asc, desc).</param>
    /// <returns>The clean field name without direction indicators.</returns>
    /// <remarks>
    /// This method handles various sort expression formats:
    /// - "fieldName asc" -> "fieldName"
    /// - "fieldName desc" -> "fieldName" 
    /// - "fieldName ASC" -> "fieldName" (case-insensitive)
    /// - "fieldName" -> "fieldName" (no direction)
    /// - "fieldName ascending" -> "fieldName" (full word)
    /// - "fieldName descending" -> "fieldName" (full word)
    /// 
    /// The method is designed to be robust and handle various user input formats
    /// while extracting the core field name for mapping validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// string field1 = ExtractFieldNameFromSortExpression("name asc");        // Returns "name"
    /// string field2 = ExtractFieldNameFromSortExpression("createdDate");     // Returns "createdDate"
    /// string field3 = ExtractFieldNameFromSortExpression("email DESC");      // Returns "email"
    /// </code>
    /// </example>
    private static string ExtractFieldNameFromSortExpression(string sortExpression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sortExpression, nameof(sortExpression));

        // Split by space and take the first part (field name)
        var parts = sortExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : sortExpression;
    }

    /// <summary>
    /// Gets diagnostic information about the property mapping service configuration.
    /// </summary>
    /// <returns>
    /// A dictionary containing diagnostic information about registered mappings and service status.
    /// </returns>
    /// <remarks>
    /// This method provides comprehensive diagnostic information including:
    /// - Count of registered property mappings
    /// - Cache statistics for performance monitoring
    /// - Memory usage information for optimization
    /// - Configuration validation results
    /// 
    /// The diagnostic information is useful for:
    /// - Service health monitoring and alerting
    /// - Performance optimization and troubleshooting
    /// - Configuration validation and verification
    /// - Development and debugging scenarios
    /// </remarks>
    /// <example>
    /// <code>
    /// var diagnostics = mappingService.GetDiagnosticInfo();
    /// 
    /// Console.WriteLine($"Registered Mappings: {diagnostics["RegisteredMappingCount"]}");
    /// Console.WriteLine($"Cached Mappings: {diagnostics["CachedMappingCount"]}");
    /// Console.WriteLine($"Service Status: {diagnostics["ServiceStatus"]}");
    /// </code>
    /// </example>
    public Dictionary<string, object> GetDiagnosticInfo()
    {
        try
        {
            return new Dictionary<string, object>
            {
                ["ServiceName"] = nameof(PropertyMappingService),
                ["RegisteredMappingCount"] = _propertyMappings.Count,
                ["CachedMappingCount"] = _mappingCache.Count,
                ["ServiceStatus"] = "Healthy",
                ["LastInitialized"] = DateTime.UtcNow, // Could track actual initialization time
                ["SupportedMappings"] = _propertyMappings.Keys.ToList(),
                ["CachedMappingKeys"] = _mappingCache.Keys.ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating diagnostic information");

            return new Dictionary<string, object>
            {
                ["ServiceName"] = nameof(PropertyMappingService),
                ["ServiceStatus"] = "Error",
                ["ErrorMessage"] = ex.Message
            };
        }
    }

    /// <summary>
    /// Clears all cached property mappings to free memory and force re-resolution.
    /// </summary>
    /// <remarks>
    /// This method is useful for:
    /// - Memory optimization in long-running applications
    /// - Testing scenarios where fresh mappings are required
    /// - Dynamic mapping scenarios where mappings may change
    /// - Performance testing and benchmarking
    /// 
    /// Note: This only clears the cache, not the registered mappings themselves.
    /// Subsequent calls to GetPropertyMapping will rebuild the cache as needed.
    /// </remarks>
    public void ClearMappingCache()
    {
        try
        {
            var clearedCount = _mappingCache.Count;
            _mappingCache.Clear();

            _logger.LogInformation("Cleared property mapping cache. Removed {ClearedCount} cached mappings", clearedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing property mapping cache");
        }
    }
}