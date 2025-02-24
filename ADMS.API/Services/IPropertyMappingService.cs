
namespace ADMS.API.Services
{
    /// <summary>
    /// Property Mapping Service
    /// </summary>
    public interface IPropertyMappingService
    {
        /// <summary>
        /// Gets Property Mapping
        /// </summary>
        /// <typeparam name="TSource">Source property</typeparam>
        /// <typeparam name="TDestination">destination property</typeparam>
        /// <returns>Dictionary of properties</returns>
        Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();

        /// <summary>
        /// Verifies mapping exists for source and destination
        /// </summary>
        /// <typeparam name="TSource">Source property</typeparam>
        /// <typeparam name="TDestination">Destination Property</typeparam>
        /// <param name="fields">fields to verify</param>
        /// <returns>True if mapping exists, False otherwise</returns>
        bool ValidMappingExistsFor<TSource, TDestination>(string fields);
    }
}