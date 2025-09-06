namespace ADMS.Domain.Common;

/// <summary>
/// Marker interface that identifies aggregate roots in the domain model.
/// </summary>
/// <remarks>
/// <para>
/// IAggregateRoot identifies entities that serve as aggregate roots within the 
/// Domain-Driven Design architecture. Aggregate roots are the only entities that 
/// can be directly accessed from outside their aggregate boundary and are responsible 
/// for maintaining consistency within their aggregate.
/// </para>
/// <para>
/// <strong>Aggregate Root Responsibilities:</strong>
/// </para>
/// <list type="bullet">
/// <item><strong>Enforce Invariants:</strong> Maintain business rules across the entire aggregate</item>
/// <item><strong>Control Access:</strong> Provide controlled access to internal aggregate entities</item>
/// <item><strong>Coordinate Changes:</strong> Ensure all changes maintain aggregate consistency</item>
/// <item><strong>Publish Events:</strong> Raise domain events for significant aggregate changes</item>
/// </list>
/// <para>
/// <strong>Repository Pattern Integration:</strong>
/// Only aggregate roots are accessed through repositories, which helps maintain
/// proper aggregate boundaries and ensures all access goes through appropriate
/// consistency and business rule enforcement mechanisms.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Implementing an aggregate root
/// public class Document : Entity&lt;DocumentId&gt;, IAggregateRoot
/// {
///     private readonly List&lt;Revision&gt; _revisions = new();
///     
///     // Aggregate root controls access to internal entities
///     public IReadOnlyCollection&lt;Revision&gt; Revisions => _revisions.AsReadOnly();
///     
///     // Aggregate root enforces business rules
///     public Result AddRevision(string content, UserId createdBy)
///     {
///         if (IsDeleted)
///             return Result.Failure(DomainError.Custom("DOCUMENT_DELETED", "Cannot add revision to deleted document"));
///             
///         var revisionNumber = _revisions.Count + 1;
///         var revision = new Revision(RevisionId.New(), Id, revisionNumber, content);
///         _revisions.Add(revision);
///         
///         // Aggregate root raises domain events
///         AddDomainEvent(new RevisionCreatedDomainEvent(revision.Id, Id, revisionNumber, createdBy));
///         
///         return Result.Success();
///     }
/// }
/// 
/// // Repository only works with aggregate roots
/// public interface IDocumentRepository
/// {
///     Task&lt;Document?&gt; GetByIdAsync(DocumentId id);
///     Task AddAsync(Document document);
///     // No direct access to Revision entities - must go through Document
/// }
/// </code>
/// </example>
public interface IAggregateRoot
{
    /// <summary>
    /// Gets the collection of domain events raised by this aggregate root.
    /// </summary>
    /// <value>A read-only collection of domain events associated with this aggregate.</value>
    /// <remarks>
    /// Domain events represent significant business occurrences within the aggregate boundary.
    /// These events are collected during business operations and dispatched by infrastructure
    /// components after successful persistence, enabling event-driven architecture patterns.
    /// </remarks>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Adds a domain event to this aggregate root's event collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    /// <remarks>
    /// This method allows aggregate roots to raise domain events during business operations.
    /// Events added through this method will be processed by infrastructure components
    /// after the aggregate's state changes are successfully persisted.
    /// </remarks>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Clears all domain events from this aggregate root's event collection.
    /// </summary>
    /// <remarks>
    /// This method is called by infrastructure components after domain events have been
    /// successfully dispatched to their handlers. It prevents duplicate event processing
    /// and manages memory usage by removing processed events from the aggregate.
    /// </remarks>
    void ClearDomainEvents();

    /// <summary>
    /// Determines whether this aggregate root has any pending domain events.
    /// </summary>
    /// <returns><c>true</c> if the aggregate has domain events; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method is useful for infrastructure components to determine whether
    /// event processing is needed for this aggregate root.
    /// </remarks>
    bool HasDomainEvents();
}