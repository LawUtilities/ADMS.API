namespace ADMS.API.Services;

/// <summary>
/// Property Mapping
/// </summary>
/// <typeparam name="TSource">Source property</typeparam>
/// <typeparam name="TDestination">Destination property</typeparam>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="mappingDictionary">Mapping dictionary</param>
/// <exception cref="ArgumentNullException">mappingDictinary null</exception>
public class PropertyMapping<TSource, TDestination>(Dictionary<string, PropertyMappingValue> mappingDictionary) : IPropertyMapping
{

    /// <summary>
    /// Mapping Dictionary
    /// </summary>
    public Dictionary<string, PropertyMappingValue> MappingDictionary
    { get => mappingDictionary; private set => mappingDictionary = value; }
}