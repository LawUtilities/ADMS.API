using System.Collections.Immutable;

namespace ADMS.API.Services;

/// <summary>
/// Represents a property mapping configuration for translating between source and destination object properties.
/// </summary>
/// <remarks>
/// This class is a core component of the property mapping system that enables:
/// - Dynamic property mapping between different object types (DTOs to Entities)
/// - Flexible sorting and filtering operations with property name translation
/// - Support for complex property mappings with multiple destination properties
/// - Bidirectional property mapping with optional revert functionality
/// - Immutable property mapping configurations for thread safety
/// 
/// The PropertyMappingValue is used extensively in:
/// - Data transformation operations between API layers
/// - Dynamic sorting operations in paginated queries
/// - Property name translation for database queries
/// - Filter expression building for search operations
/// - Data shaping operations for API responses
/// 
/// Key Features:
/// - Immutable property collections for thread safety and consistency
/// - Support for one-to-many property mappings (single source to multiple destinations)
/// - Revert functionality for bidirectional mapping operations
/// - Comprehensive validation of property mapping configurations
/// - Performance-optimized property access with cached collections
/// 
/// Usage Examples:
/// - Mapping DTO property "Name" to Entity properties "FirstName" and "LastName"
/// - Mapping API sort parameter "createdDate" to database column "CreationTimestamp"
/// - Mapping filter property "isActive" to database expression "Status = 'Active'"
/// - Supporting reverse mappings for data retrieval operations
/// 
/// Thread Safety:
/// - All properties are immutable after construction
/// - DestinationProperties collection is converted to ImmutableList for thread safety
/// - No mutable state that could cause threading issues
/// - Safe for use in concurrent scenarios and dependency injection
/// 
/// Performance Considerations:
/// - Destination properties are cached as immutable collections
/// - Property validation occurs once during construction
/// - No runtime property modification overhead
/// - Optimized for frequent read operations in mapping scenarios
/// </remarks>
/// <param name="destinationProperties">
/// The collection of destination property names that the source property maps to.
/// Must not be null and should contain at least one valid property name.
/// Property names should correspond to valid properties on the destination object type.
/// </param>
/// <param name="revert">
/// Optional flag indicating whether the mapping should be reversed for bidirectional operations.
/// When true, the property mapping supports reverse translation from destination to source.
/// Default value is false for standard one-way mapping operations.
/// </param>
/// <exception cref="ArgumentNullException">
/// Thrown when destinationProperties parameter is null.
/// </exception>
/// <exception cref="ArgumentException">
/// Thrown when destinationProperties collection is empty or contains null/whitespace property names.
/// </exception>
/// <example>
/// <code>
/// // Simple one-to-one property mapping
/// var nameMapping = new PropertyMappingValue(new[] { "FullName" });
/// 
/// // One-to-many property mapping with multiple destinations
/// var addressMapping = new PropertyMappingValue(
///     new[] { "Street", "City", "PostalCode" });
/// 
/// // Bidirectional property mapping with revert support
/// var bidirectionalMapping = new PropertyMappingValue(
///     new[] { "DatabaseColumnName" }, 
///     revert: true);
/// 
/// // Complex mapping for sorting operations
/// var sortMapping = new PropertyMappingValue(
///     new[] { "LastName", "FirstName" }, 
///     revert: false);
/// </code>
/// </example>
public sealed class PropertyMappingValue
{
    /// <summary>
    /// Gets the immutable collection of destination property names that the source property maps to.
    /// </summary>
    /// <remarks>
    /// This property provides:
    /// - Thread-safe access to destination property names
    /// - Immutable collection that cannot be modified after construction
    /// - Guaranteed to contain at least one valid property name
    /// - Property names correspond to valid properties on destination objects
    /// 
    /// The destination properties are used for:
    /// - Building database query expressions with proper column names
    /// - Constructing LINQ expressions for entity property access
    /// - Validating property mappings against target object schemas
    /// - Generating sort and filter clauses for data operations
    /// 
    /// Collection Characteristics:
    /// - Immutable after construction (ImmutableList implementation)
    /// - Thread-safe for concurrent access scenarios
    /// - Preserves original property name ordering for consistent operations
    /// - Validated to ensure no null or empty property names
    /// 
    /// Common Usage Patterns:
    /// - Single property mapping: ["EntityPropertyName"]
    /// - Composite property mapping: ["LastName", "FirstName"]
    /// - Complex expression mapping: ["Address.Street", "Address.City"]
    /// </remarks>
    /// <value>
    /// An immutable collection of destination property names. 
    /// The collection is guaranteed to contain at least one non-null, non-empty property name.
    /// </value>
    public ImmutableList<string> DestinationProperties { get; }

    /// <summary>
    /// Gets a value indicating whether the property mapping supports bidirectional operations.
    /// </summary>
    /// <remarks>
    /// This property controls:
    /// - Direction of property mapping operations (one-way vs bidirectional)
    /// - Sort order behavior for database queries (ascending vs descending)
    /// - Filter expression construction for reverse mapping scenarios
    /// - Data transformation direction for complex mapping operations
    /// 
    /// When Revert is true:
    /// - Property mappings can be applied in reverse direction
    /// - Sort operations may use descending order by default
    /// - Filter expressions support reverse property name resolution
    /// - Data transformation supports bidirectional conversion
    /// 
    /// When Revert is false (default):
    /// - Property mappings are applied in forward direction only
    /// - Sort operations use ascending order by default
    /// - Filter expressions use standard property name resolution
    /// - Data transformation is unidirectional (source to destination)
    /// 
    /// Usage Scenarios:
    /// - API sorting parameters that should reverse database sort order
    /// - Filter mappings that require inverse logic application
    /// - Data synchronization scenarios requiring bidirectional mapping
    /// - Complex property transformations with multiple mapping directions
    /// </remarks>
    /// <value>
    /// true if the property mapping supports bidirectional operations; 
    /// false for standard one-way mapping operations.
    /// </value>
    public bool Revert { get; }

    /// <summary>
    /// Gets the count of destination properties in the mapping configuration.
    /// </summary>
    /// <remarks>
    /// This convenience property provides:
    /// - Quick access to the number of destination properties without enumerating the collection
    /// - Performance optimization for scenarios that only need the count
    /// - Validation support for mapping configuration analysis
    /// - Debugging and monitoring capabilities for mapping complexity
    /// 
    /// Common usage patterns:
    /// - Count = 1: Simple one-to-one property mapping
    /// - Count > 1: Complex one-to-many property mapping
    /// - Count validation in mapping configuration validation
    /// - Performance analysis of mapping complexity
    /// </remarks>
    /// <value>
    /// The number of destination properties in the mapping configuration.
    /// Always greater than zero due to constructor validation.
    /// </value>
    public int PropertyCount => DestinationProperties.Count;

    /// <summary>
    /// Gets a value indicating whether this is a simple one-to-one property mapping.
    /// </summary>
    /// <remarks>
    /// This convenience property identifies:
    /// - Simple mappings with exactly one destination property
    /// - Optimized processing paths for common mapping scenarios
    /// - Validation scenarios for simple property transformations
    /// - Performance optimization opportunities for single property access
    /// 
    /// Simple mappings (IsSimpleMapping = true) enable:
    /// - Direct property access without collection enumeration
    /// - Optimized LINQ expression generation
    /// - Faster validation and transformation operations
    /// - Simplified debugging and logging scenarios
    /// </remarks>
    /// <value>
    /// true if the mapping contains exactly one destination property; 
    /// false for complex mappings with multiple destination properties.
    /// </value>
    public bool IsSimpleMapping => DestinationProperties.Count == 1;

    /// <summary>
    /// Initializes a new instance of the PropertyMappingValue class with comprehensive validation.
    /// </summary>
    /// <param name="destinationProperties">
    /// The collection of destination property names that the source property maps to.
    /// Must not be null and should contain at least one valid property name.
    /// Property names should correspond to valid properties on the destination object type.
    /// </param>
    /// <param name="revert">
    /// Optional flag indicating whether the mapping should be reversed for bidirectional operations.
    /// When true, the property mapping supports reverse translation from destination to source.
    /// Default value is false for standard one-way mapping operations.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when destinationProperties parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when destinationProperties collection is empty or contains null/whitespace property names.
    /// </exception>
    public PropertyMappingValue(
        IEnumerable<string> destinationProperties,
        bool revert = false)
    {
        // Validate destinationProperties parameter
        ArgumentNullException.ThrowIfNull(destinationProperties, nameof(destinationProperties));

        // Convert to list for validation and ensure immutability
        var propertyList = destinationProperties.ToList();

        // Validate collection is not empty
        if (propertyList.Count == 0)
        {
            throw new ArgumentException(
                "Destination properties collection cannot be empty. At least one property name is required.",
                nameof(destinationProperties));
        }

        // Validate individual property names
        for (int i = 0; i < propertyList.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(propertyList[i]))
            {
                throw new ArgumentException(
                    $"Destination property at index {i} cannot be null, empty, or whitespace. " +
                    "All property names must be valid non-empty strings.",
                    nameof(destinationProperties));
            }
        }

        // Check for duplicate property names
        var duplicateProperties = propertyList
            .GroupBy(prop => prop, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateProperties.Count > 0)
        {
            throw new ArgumentException(
                $"Duplicate destination properties detected: {string.Join(", ", duplicateProperties)}. " +
                "Each destination property name must be unique (case-insensitive comparison).",
                nameof(destinationProperties));
        }

        // Create immutable collection for thread safety
        DestinationProperties = propertyList.ToImmutableList();
        Revert = revert;
    }

    /// <summary>
    /// Gets the first destination property name for simple mapping scenarios.
    /// </summary>
    /// <remarks>
    /// This convenience method provides:
    /// - Direct access to the primary destination property for simple mappings
    /// - Performance optimization for common single-property mapping scenarios
    /// - Simplified API for scenarios that only need the first property
    /// - Backward compatibility with code expecting single property names
    /// 
    /// Usage considerations:
    /// - Best used with simple mappings (IsSimpleMapping = true)
    /// - For complex mappings, consider using DestinationProperties collection
    /// - Returns the first property in order-dependent mapping scenarios
    /// - Provides consistent behavior regardless of mapping complexity
    /// </remarks>
    /// <returns>
    /// The first destination property name in the mapping configuration.
    /// Guaranteed to be a non-null, non-empty string.
    /// </returns>
    /// <example>
    /// <code>
    /// var mapping = new PropertyMappingValue(new[] { "EntityPropertyName" });
    /// string primaryProperty = mapping.GetPrimaryProperty(); // Returns "EntityPropertyName"
    /// 
    /// var complexMapping = new PropertyMappingValue(new[] { "LastName", "FirstName" });
    /// string firstProperty = complexMapping.GetPrimaryProperty(); // Returns "LastName"
    /// </code>
    /// </example>
    public string GetPrimaryProperty()
    {
        return DestinationProperties[0];
    }

    /// <summary>
    /// Determines whether the mapping contains the specified destination property name.
    /// </summary>
    /// <param name="propertyName">The property name to search for in the destination properties.</param>
    /// <returns>
    /// true if the specified property name is found in the destination properties; 
    /// otherwise, false.
    /// </returns>
    /// <remarks>
    /// This method provides:
    /// - Case-insensitive property name matching for robust property lookup
    /// - Validation support for mapping configuration verification
    /// - Dynamic property mapping validation during runtime operations
    /// - Integration support for property mapping validation frameworks
    /// 
    /// The search is performed using:
    /// - Case-insensitive string comparison (OrdinalIgnoreCase)
    /// - Null-safe comparison handling
    /// - Performance-optimized collection search
    /// - Consistent behavior across different property name formats
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when propertyName is null, empty, or contains only whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var mapping = new PropertyMappingValue(new[] { "FirstName", "LastName" });
    /// 
    /// bool hasFirstName = mapping.ContainsProperty("FirstName");     // Returns true
    /// bool hasLastName = mapping.ContainsProperty("lastname");       // Returns true (case-insensitive)
    /// bool hasMiddleName = mapping.ContainsProperty("MiddleName");   // Returns false
    /// </code>
    /// </example>
    public bool ContainsProperty(string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));

        return DestinationProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a new PropertyMappingValue instance with the revert flag toggled.
    /// </summary>
    /// <returns>
    /// A new PropertyMappingValue instance with the same destination properties 
    /// but with the revert flag set to the opposite of the current value.
    /// </returns>
    /// <remarks>
    /// This method provides:
    /// - Immutable object pattern for creating modified mapping instances
    /// - Support for dynamic mapping direction changes during runtime
    /// - Functional programming approach to mapping configuration modification
    /// - Thread-safe mapping transformation without modifying original instance
    /// 
    /// Use cases include:
    /// - Dynamic sort direction changes in user interface operations
    /// - Bidirectional data synchronization scenarios
    /// - API parameter processing with direction-specific mapping requirements
    /// - Configuration-driven mapping direction control
    /// 
    /// The returned instance:
    /// - Shares the same destination properties collection (immutable)
    /// - Has the opposite revert flag value
    /// - Is completely independent of the original instance
    /// - Maintains all validation guarantees of the original mapping
    /// </remarks>
    /// <example>
    /// <code>
    /// var originalMapping = new PropertyMappingValue(new[] { "Name" }, revert: false);
    /// var reversedMapping = originalMapping.WithReversedDirection();
    /// 
    /// Console.WriteLine(originalMapping.Revert);  // Output: False
    /// Console.WriteLine(reversedMapping.Revert);  // Output: True
    /// </code>
    /// </example>
    public PropertyMappingValue WithReversedDirection()
    {
        return new PropertyMappingValue(DestinationProperties, revert: !Revert);
    }

    /// <summary>
    /// Returns a string representation of the property mapping configuration.
    /// </summary>
    /// <returns>
    /// A string containing the destination properties and revert flag information
    /// in a human-readable format suitable for debugging and logging.
    /// </returns>
    /// <remarks>
    /// The string representation includes:
    /// - All destination property names in a comma-separated list
    /// - Revert flag status for bidirectional mapping indication
    /// - Formatted output suitable for debugging and diagnostic purposes
    /// - Consistent formatting for logging and monitoring scenarios
    /// 
    /// Output format examples:
    /// - Simple mapping: "Properties: [Name], Revert: False"
    /// - Complex mapping: "Properties: [LastName, FirstName], Revert: True"
    /// - Single property with revert: "Properties: [CreatedDate], Revert: True"
    /// 
    /// This method is particularly useful for:
    /// - Debugging property mapping configurations
    /// - Logging mapping operations for audit trails
    /// - Displaying mapping information in diagnostic tools
    /// - Troubleshooting property mapping issues
    /// </remarks>
    /// <example>
    /// <code>
    /// var mapping = new PropertyMappingValue(new[] { "FirstName", "LastName" }, revert: true);
    /// string description = mapping.ToString();
    /// // Output: "Properties: [FirstName, LastName], Revert: True"
    /// </code>
    /// </example>
    public override string ToString()
    {
        var properties = string.Join(", ", DestinationProperties);
        return $"Properties: [{properties}], Revert: {Revert}";
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current PropertyMappingValue.
    /// </summary>
    /// <param name="obj">The object to compare with the current PropertyMappingValue.</param>
    /// <returns>
    /// true if the specified object is equal to the current PropertyMappingValue; 
    /// otherwise, false.
    /// </returns>
    /// <remarks>
    /// Equality comparison includes:
    /// - Type compatibility checking (must be PropertyMappingValue)
    /// - Destination properties collection comparison (order-sensitive)
    /// - Revert flag value comparison
    /// - Case-insensitive property name comparison for robustness
    /// 
    /// Two PropertyMappingValue instances are considered equal when:
    /// - They have the same number of destination properties
    /// - All destination properties match (case-insensitive, order-sensitive)
    /// - Both have the same revert flag value
    /// - Both are non-null PropertyMappingValue instances
    /// 
    /// This implementation supports:
    /// - Dictionary and HashSet operations with PropertyMappingValue keys
    /// - Equality-based collection operations and comparisons
    /// - Unit testing scenarios with equality assertions
    /// - Configuration comparison and validation scenarios
    /// </remarks>
    /// <example>
    /// <code>
    /// var mapping1 = new PropertyMappingValue(new[] { "Name" }, revert: false);
    /// var mapping2 = new PropertyMappingValue(new[] { "Name" }, revert: false);
    /// var mapping3 = new PropertyMappingValue(new[] { "Name" }, revert: true);
    /// 
    /// bool areEqual1 = mapping1.Equals(mapping2);  // Returns true
    /// bool areEqual2 = mapping1.Equals(mapping3);  // Returns false (different revert values)
    /// </code>
    /// </example>
    public override bool Equals(object? obj)
    {
        if (obj is not PropertyMappingValue other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Revert == other.Revert &&
               DestinationProperties.Count == other.DestinationProperties.Count &&
               DestinationProperties.SequenceEqual(other.DestinationProperties, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Serves as the default hash function for PropertyMappingValue instances.
    /// </summary>
    /// <returns>
    /// A hash code for the current PropertyMappingValue instance.
    /// </returns>
    /// <remarks>
    /// The hash code is computed based on:
    /// - All destination property names (case-insensitive)
    /// - Revert flag value
    /// - Consistent hashing algorithm for reliable dictionary operations
    /// 
    /// Hash code characteristics:
    /// - Consistent across multiple calls on the same instance
    /// - Equal objects produce equal hash codes
    /// - Distributed hash codes for good dictionary performance
    /// - Case-insensitive property name handling
    /// 
    /// This implementation supports:
    /// - Use as keys in Dictionary<PropertyMappingValue, T>
    /// - HashSet<PropertyMappingValue> operations
    /// - Efficient lookup operations in hash-based collections
    /// - Performance optimization for large mapping collections
    /// </remarks>
    /// <example>
    /// <code>
    /// var mapping = new PropertyMappingValue(new[] { "Name" }, revert: false);
    /// int hashCode = mapping.GetHashCode();
    /// 
    /// var dictionary = new Dictionary<PropertyMappingValue, string>();
    /// dictionary[mapping] = "User Name Mapping";  // Uses GetHashCode() for efficient storage
    /// </code>
    /// </example>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        // Add revert flag to hash
        hashCode.Add(Revert);

        // Add each property name to hash (case-insensitive)
        foreach (var property in DestinationProperties)
        {
            hashCode.Add(property, StringComparer.OrdinalIgnoreCase);
        }

        return hashCode.ToHashCode();
    }

    /// <summary>
    /// Validates the property mapping configuration against a specified type's properties.
    /// </summary>
    /// <typeparam name="T">The target type to validate properties against.</typeparam>
    /// <returns>
    /// true if all destination properties exist on the specified type; 
    /// otherwise, false.
    /// </returns>
    /// <remarks>
    /// This method provides:
    /// - Compile-time type safety for property mapping validation
    /// - Runtime validation of property mapping configurations
    /// - Support for complex property path validation (e.g., "Address.Street")
    /// - Integration with reflection-based property discovery
    /// 
    /// Validation includes:
    /// - Property existence checking using reflection
    /// - Case-insensitive property name matching
    /// - Support for nested property paths with dot notation
    /// - Public property accessibility validation
    /// 
    /// Use cases:
    /// - Configuration validation during application startup
    /// - Dynamic mapping validation in mapping service initialization
    /// - Unit testing scenarios for mapping configuration verification
    /// - Runtime property mapping validation for dynamic scenarios
    /// </remarks>
    /// <example>
    /// <code>
    /// public class UserEntity
    /// {
    ///     public string FirstName { get; set; }
    ///     public string LastName { get; set; }
    /// }
    /// 
    /// var mapping = new PropertyMappingValue(new[] { "FirstName", "LastName" });
    /// bool isValid = mapping.IsValidForType<UserEntity>();  // Returns true
    /// 
    /// var invalidMapping = new PropertyMappingValue(new[] { "InvalidProperty" });
    /// bool isInvalid = invalidMapping.IsValidForType<UserEntity>();  // Returns false
    /// </code>
    /// </example>
    public bool IsValidForType<T>()
    {
        var type = typeof(T);
        var properties = type.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return DestinationProperties.All(destProperty =>
            ValidatePropertyPath(destProperty, properties));
    }

    /// <summary>
    /// Validates a property path against available properties, supporting nested property access.
    /// </summary>
    /// <param name="propertyPath">The property path to validate (e.g., "Address.Street").</param>
    /// <param name="availableProperties">The set of available property names for validation.</param>
    /// <returns>true if the property path is valid; otherwise, false.</returns>
    private static bool ValidatePropertyPath(string propertyPath, ISet<string> availableProperties)
    {
        // For simple property names (no dots), check directly
        if (!propertyPath.Contains('.'))
        {
            return availableProperties.Contains(propertyPath);
        }

        // For nested properties, validate the first part
        // Note: Full nested validation would require more complex reflection
        var firstProperty = propertyPath.Split('.')[0];
        return availableProperties.Contains(firstProperty);
    }
}