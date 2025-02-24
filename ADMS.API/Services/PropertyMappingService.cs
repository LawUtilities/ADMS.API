using ADMS.API.Entities;
using ADMS.API.Models;

namespace ADMS.API.Services;

/// <summary>
/// Property Mapping Service
/// </summary>
public class PropertyMappingService : IPropertyMappingService
{
    private readonly Dictionary<string, PropertyMappingValue> _documentPropertyMapping =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "Id", new(new[] { "Id" }) },
            { "FileName", new(new[] { "FileName" }) },
            { "Extension", new(new[] { "Extension" }) }
        };

    private readonly IList<IPropertyMapping> _propertyMappings =
        [];

    /// <summary>
    /// Constructor
    /// </summary>
    public PropertyMappingService() => _propertyMappings.Add(new PropertyMapping<DocumentDto, Document>(
            _documentPropertyMapping));

    /// <summary>
    /// Get Property Mapping
    /// </summary>
    /// <typeparam name="TSource">Source property</typeparam>
    /// <typeparam name="TDestination">Destination property</typeparam>
    /// <returns>dictionary of string / property mapping value</returns>
    /// <exception cref="Exception">no explicit mapping found</exception>
    public Dictionary<string, PropertyMappingValue> GetPropertyMapping
          <TSource, TDestination>()
    {
        // get matching mapping
        var matchingMapping = _propertyMappings
            .OfType<PropertyMapping<TSource, TDestination>>();

        if (matchingMapping.Count() == 1)
        {
            return matchingMapping.First().MappingDictionary;
        }

        throw new Exception($"Cannot find exact property mapping instance " +
            $"for <{typeof(TSource)},{typeof(TDestination)}");
    }

/// <inheritdoc/>

    public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
    {
        var propertyMapping = GetPropertyMapping<TSource, TDestination>();

        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }

        // the string is separated by ",", so we split it.
        var fieldsAfterSplit = fields.Split(',');

        // run through the fields clauses
        foreach (var field in fieldsAfterSplit)
        {
            // trim
            var trimmedField = field.Trim();

            // remove everything after the first " " - if the fields 
            // are coming from an orderBy string, this part must be 
            // ignored
            var indexOfFirstSpace = trimmedField.IndexOf(" ", StringComparison.CurrentCulture);
            var propertyName = indexOfFirstSpace == -1 ?
                trimmedField : trimmedField.Remove(indexOfFirstSpace);

            // find the matching property
            if (!propertyMapping.ContainsKey(propertyName))
            {
                return false;
            }
        }
        return true;
    }
}

