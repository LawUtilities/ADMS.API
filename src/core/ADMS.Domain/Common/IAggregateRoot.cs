namespace ADMS.Domain.Common;

/// <summary>
/// Marker interface that identifies aggregate roots in the domain model.
/// </summary>
/// <remarks>
/// IAggregateRoot serves as a marker interface that identifies entities capable of serving
/// as aggregate roots within the Domain-Driven Design architecture. Aggregate roots are
/// the only entities that can be directly accessed and modified from outside their aggregate
/// boundary, and they are responsible for maintaining consistency within their aggregate.
/// 
/// <para><strong>Domain-Driven Design Concepts:</strong></para>
/// <list type="bullet">
/// <item><strong>Aggregate Root:</strong> The single entry point for accessing and modifying an aggregate</item>
/// <item><strong>Consistency Boundary:</strong> Defines the scope of data consistency rules</item>
/// <item><strong>Transaction Boundary:</strong> Typically maps to database transaction boundaries</item>
/// <item><strong>Repository Access:</strong> Only aggregate roots are accessed through repositories</item>
/// </list>
/// 
/// <para><strong>Aggregate Root Responsibilities:</strong></para>
/// <list type="bullet">
/// <item><strong>Enforce Invariants:</strong> Maintain business rules across the entire aggregate</item>
/// <item><strong>Control Access:</strong> Provide controlled access to internal aggregate entities</item>
/// <item><strong>Coordinate Changes:</strong> Ensure all changes maintain aggregate consistency</item>
/// <item><strong>Publish Events:</strong> Raise domain events for significant aggregate changes</item>
/// <item><strong>Transaction Control:</strong> Define atomic transaction boundaries</item>
/// </list>
/// 
/// <para><strong>ADMS Legal System Aggregates:</strong></para>
/// In the legal document management context, typical aggregates include:
/// <list type="bullet">
/// <item><strong>Matter Aggregate:</strong> Matter as root with associated documents and activities</item>
/// <item><strong>Document Aggregate:</strong> Document as root with revisions and audit trails</item>
/// <item><strong>User Aggregate:</strong> User as root with permissions and activity history</item>
/// </list>
/// 
/// <para><strong>Domain Events Integration:</strong></para>
/// Aggregate roots are the primary sources of domain events, raising events when
/// significant business operations occur within their boundaries. The domain events
/// enable loose coupling between aggregates and support comprehensive audit trail
/// automation essential for legal compliance.
/// 
/// <para><strong>Repository Pattern Integration:</strong></para>
/// Only aggregate roots are accessed through repositories, which helps maintain
/// proper aggregate boundaries and ensures that all access to the domain model
/// goes through the appropriate consistency and business rule enforcement mechanisms.
/// 
/// <para><strong>Legal Compliance Benefits:</strong></para>
/// <list type="bullet">
/// <item>Consistent audit trail generation through controlled aggregate access</item>
/// <item>Professional responsibility compliance through invariant enforcement</item>
/// <item>Client confidentiality protection through controlled data access</item>
/// <item>Regulatory compliance through comprehensive event tracking</item>
/// </list>
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
///             return Result.Failure(new DomainError("DOCUMENT_DELETED", "Cannot add revision to deleted document"));
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
/// 
/// // Infrastructure identifies aggregate roots for event processing
/// public class DbContext : DbContext
/// {
///     public override async Task&lt;int&gt; SaveChangesAsync(CancellationToken cancellationToken = default)
///     {
///         // Only aggregate roots are processed for domain events
///         var aggregateRoots = ChangeTracker.Entries&lt;IAggregateRoot&gt;()
///             .Where(x => x.Entity.DomainEvents.Any())
///             .Select(x => x.Entity);
///             
///         foreach (var aggregateRoot in aggregateRoots)
///         {
///             await ProcessDomainEvents(aggregateRoot.DomainEvents);
///             aggregateRoot.ClearDomainEvents();
///         }
///         
///         return await base.SaveChangesAsync(cancellationToken);
///     }
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
    /// components after successful persistence, enabling event-driven architecture patterns
    /// and comprehensive audit trail automation.
    /// 
    /// <para><strong>Event Lifecycle:</strong></para>
    /// <list type="number">
    /// <item>Business operation occurs within aggregate boundary</item>
    /// <item>Aggregate root raises appropriate domain event</item>
    /// <item>Event is collected in this collection</item>
    /// <item>Infrastructure persists aggregate state changes</item>
    /// <item>Infrastructure dispatches collected events to handlers</item>
    /// <item>Events are cleared from aggregate root</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Integration:</strong></para>
    /// Domain events from aggregate roots provide the foundation for:
    /// <list type="bullet">
    /// <item>Automated audit trail generation for legal compliance</item>
    /// <item>Professional responsibility documentation</item>
    /// <item>Client confidentiality protection through controlled event publication</item>
    /// <item>Regulatory compliance through comprehensive activity tracking</item>
    /// </list>
    /// 
    /// <para><strong>Infrastructure Usage:</strong></para>
    /// Infrastructure components (like DbContext implementations) use this property to:
    /// <list type="bullet">
    /// <item>Collect events from all aggregate roots before persistence</item>
    /// <item>Dispatch events to appropriate handlers after successful persistence</item>
    /// <item>Implement transactional event processing patterns</item>
    /// <item>Ensure event processing consistency across aggregate boundaries</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Aggregate root collecting events during business operations
    /// public Result ProcessDocumentUpdate(string newContent, UserId updatedBy)
    /// {
    ///     // Perform business logic
    ///     var oldContent = Content;
    ///     Content = newContent;
    ///     
    ///     // Raise domain event - automatically collected in DomainEvents
    ///     AddDomainEvent(new DocumentUpdatedDomainEvent(Id, oldContent, newContent, updatedBy));
    ///     
    ///     return Result.Success();
    /// }
    /// 
    /// // Infrastructure processing events from aggregate roots
    /// private async Task ProcessAggregateEvents()
    /// {
    ///     var aggregatesWithEvents = ChangeTracker.Entries&lt;IAggregateRoot&gt;()
    ///         .Where(entry => entry.Entity.DomainEvents.Any())
    ///         .Select(entry => entry.Entity);
    ///         
    ///     foreach (var aggregate in aggregatesWithEvents)
    ///     {
    ///         // Process events from this aggregate
    ///         foreach (var domainEvent in aggregate.DomainEvents)
    ///         {
    ///             await _eventDispatcher.DispatchAsync(domainEvent);
    ///         }
    ///         
    ///         // Clear events after processing
    ///         aggregate.ClearDomainEvents();
    ///     }
    /// }
    /// </code>
    /// </example>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events from this aggregate root's event collection.
    /// </summary>
    /// <remarks>
    /// This method is called by infrastructure components after domain events have been
    /// successfully dispatched to their handlers. It prevents duplicate event processing
    /// and manages memory usage by removing processed events from the aggregate.
    /// 
    /// <para><strong>Infrastructure Responsibility:</strong></para>
    /// This method should only be called by infrastructure components (such as DbContext
    /// implementations or Unit of Work patterns) after ensuring that:
    /// <list type="bullet">
    /// <item>All aggregate state changes have been successfully persisted</item>
    /// <item>All domain events have been successfully dispatched to their handlers</item>
    /// <item>Any transaction boundaries have been properly committed</item>
    /// </list>
    /// 
    /// <para><strong>Event Processing Guarantees:</strong></para>
    /// The infrastructure should ensure that events are not cleared if:
    /// <list type="bullet">
    /// <item>Persistence operations fail</item>
    /// <item>Event dispatching fails</item>
    /// <item>Transaction rollback occurs</item>
    /// <item>System errors prevent complete processing</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance Considerations:</strong></para>
    /// In legal systems, event processing failures could compromise audit trail
    /// integrity. The infrastructure should implement appropriate error handling
    /// and retry mechanisms to ensure no events are lost during processing.
    /// 
    /// <para><strong>Transactional Consistency:</strong></para>
    /// Event clearing should be coordinated with database transactions to ensure
    /// that events are only cleared after successful persistence of all related
    /// state changes, maintaining consistency between aggregate state and event processing.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Typical infrastructure usage in DbContext
    /// public override async Task&lt;int&gt; SaveChangesAsync(CancellationToken cancellationToken = default)
    /// {
    ///     var aggregatesWithEvents = ChangeTracker.Entries&lt;IAggregateRoot&gt;()
    ///         .Where(entry => entry.Entity.DomainEvents.Any())
    ///         .Select(entry => entry.Entity)
    ///         .ToList();
    ///         
    ///     // Collect events before clearing
    ///     var allEvents = aggregatesWithEvents
    ///         .SelectMany(aggregate => aggregate.DomainEvents)
    ///         .ToList();
    ///     
    ///     try
    ///     {
    ///         // Persist state changes first
    ///         var result = await base.SaveChangesAsync(cancellationToken);
    ///         
    ///         // Clear events only after successful persistence
    ///         foreach (var aggregate in aggregatesWithEvents)
    ///         {
    ///             aggregate.ClearDomainEvents();
    ///         }
    ///         
    ///         // Dispatch events after successful persistence and clearing
    ///         await _eventDispatcher.DispatchAsync(allEvents, cancellationToken);
    ///         
    ///         return result;
    ///     }
    ///     catch
    ///     {
    ///         // Don't clear events if persistence fails
    ///         // Events remain available for retry processing
    ///         throw;
    ///     }
    /// }
    /// 
    /// // Error handling preserves events for retry
    /// public async Task ProcessAggregateWithRetry(IAggregateRoot aggregate)
    /// {
    ///     var maxRetries = 3;
    ///     var attempt = 0;
    ///     
    ///     while (attempt &lt; maxRetries)
    ///     {
    ///         try
    ///         {
    ///             await ProcessAggregate(aggregate);
    ///             aggregate.ClearDomainEvents(); // Only clear on success
    ///             break;
    ///         }
    ///         catch (Exception ex)
    ///         {
    ///             attempt++;
    ///             if (attempt >= maxRetries)
    ///             {
    ///                 // Events remain available for manual processing
    ///                 _logger.LogError(ex, "Failed to process aggregate after {MaxRetries} attempts. " +
    ///                                     "Events preserved for manual intervention.", maxRetries);
    ///                 throw;
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    void ClearDomainEvents();
}