using ADMS.API.Services;

using System.Linq.Dynamic.Core;

namespace ADMS.API.Helpers;

/// <summary>
/// Provides extension methods for IQueryable to support dynamic sorting using property mapping.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies dynamic sorting to the source IQueryable based on the provided orderBy string and property mapping dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the IQueryable elements.</typeparam>
    /// <param name="source">The source IQueryable to apply sorting to.</param>
    /// <param name="orderBy">A comma-separated string specifying the sort order (e.g., "Name desc, Age").</param>
    /// <param name="mappingDictionary">A dictionary mapping source property names to destination property mappings.</param>
    /// <returns>The sorted IQueryable.</returns>
    /// <exception cref="ArgumentNullException">Thrown if source or mappingDictionary is null.</exception>
    /// <exception cref="ArgumentException">Thrown if a property in orderBy is not found in the mapping dictionary.</exception>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        string orderBy,
        Dictionary<string, PropertyMappingValue> mappingDictionary)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mappingDictionary);

        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return source;
        }

        // Split the orderBy string into individual clauses
        var orderByClauses = orderBy.Split(',', StringSplitOptions.RemoveEmptyEntries);

        // Build the orderBy string using LINQ
        var orderByString = string.Join(", ", orderByClauses.Select(clause =>
        {
            var trimmedClause = clause.Trim();

            // Determine sort direction
            var isDescending = trimmedClause.EndsWith(" desc", StringComparison.OrdinalIgnoreCase);
            var propertyName = ExtractPropertyName(trimmedClause);

            // Validate property name
            if (!mappingDictionary.TryGetValue(propertyName, out var propertyMappingValue))
            {
                var availableKeys = string.Join(", ", mappingDictionary.Keys);
                throw new ArgumentException($"Key mapping for property '{propertyName}' is missing. Available keys: {availableKeys}");
            }

            // Adjust sort direction if necessary
            if (propertyMappingValue.Revert)
            {
                isDescending = !isDescending;
            }

            // Build the orderBy string for the current clause
            return string.Join(", ", propertyMappingValue.DestinationProperties.Select(destinationProperty =>
                $"{destinationProperty} {(isDescending ? "descending" : "ascending")}"
            ));
        }));

        // Apply the sorting
        return source.OrderBy(orderByString);
    }

    /// <summary>
    /// Extracts the property name from an orderBy clause.
    /// </summary>
    /// <param name="orderByClause">The orderBy clause (e.g., "Name desc").</param>
    /// <returns>The extracted property name (e.g., "Name").</returns>
    private static string ExtractPropertyName(string orderByClause)
    {
        var indexOfSpace = orderByClause.IndexOf(' ');
        return indexOfSpace == -1
            ? orderByClause
            : orderByClause[..indexOfSpace];
    }
}
