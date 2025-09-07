namespace ADMS.API.Common.Interfaces;

/// <summary>
/// Marker interface for entities that contain and manage document collections.
/// </summary>
/// <remarks>
/// Document container functionality provides consistent document management capabilities
/// across different entity types that organize and manage document collections.
/// </remarks>
public interface IDocumentContainer
{
    /// <summary>
    /// Gets a value indicating whether the container has any documents.
    /// </summary>
    bool HasDocuments { get; }

    /// <summary>
    /// Gets the total number of documents in the container.
    /// </summary>
    int DocumentCount { get; }

    /// <summary>
    /// Gets the number of active (non-deleted) documents in the container.
    /// </summary>
    int ActiveDocumentCount { get; }
}