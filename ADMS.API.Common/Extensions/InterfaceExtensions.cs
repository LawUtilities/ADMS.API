using ADMS.API.Common.Interfaces;

namespace ADMS.API.Common.Extensions;

/// <summary>
/// Extension methods for working with ADMS marker interfaces.
/// </summary>
public static class InterfaceExtensions
{
    /// <summary>
    /// Filters a collection to only include active entities.
    /// </summary>
    public static IEnumerable<T> WhereActive<T>(this IEnumerable<T> source)
        where T : ISoftDeletable, IArchivable
    {
        return source.Where(x => !x.IsDeleted && !x.IsArchived);
    }

    /// <summary>
    /// Filters a collection to only include entities with audit trails.
    /// </summary>
    public static IEnumerable<T> WithAuditTrails<T>(this IEnumerable<T> source)
        where T : IAuditable
    {
        return source.Where(x => x.HasActivities);
    }

    /// <summary>
    /// Gets all documents from containers that meet specified criteria.
    /// </summary>
    public static IEnumerable<T> WithDocuments<T>(this IEnumerable<T> source)
        where T : IDocumentContainer
    {
        return source.Where(x => x.HasDocuments);
    }
}