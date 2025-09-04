namespace ADMS.Domain.Common;

/// <summary>
/// Abstract base class for domain entities with strong-typed identifiers and integrated domain events.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier, constrained to reference types.</typeparam>
/// <remarks>
/// Entity serves as the foundational base class for all domain entities in the ADMS system,
/// providing essential infrastructure for identity, equality, and domain event management.
/// This implementation follows Domain-Driven Design principles and supports comprehensive
/// audit trail automation through integrated domain events.
/// 
/// <para><strong>Domain-Driven Design Principles:</strong></para>
/// <list type="bullet">
/// <item><strong>Identity:</strong> Each entity has a unique, immutable identifier</item>
/// <item><strong>Equality:</strong> Entities are equal if their identifiers are equal</item>
/// <item><strong>Lifecycle:</strong> Entities maintain state and behavior throughout their lifecycle</item>
/// <item><strong>Business Focus:</strong> Entities represent core business concepts with rich behavior</item>
/// </list>
/// 
/// <para><strong>Strong-Typed Identifiers:</strong></para>
/// The use of strong-typed identifiers (value objects) instead of primitive types provides:
/// <list type="bullet">
/// <item>Type safety preventing identifier mix-ups between entities</item>
/// <item>Compile-time validation of identifier usage</item>
/// <item>Encapsulation of identifier validation logic</item>
/// <item>Enhanced readability and maintainability</item>
/// <item>Support for identifier-specific operations and formatting</item>
/// </list>
/// 
/// <para><strong>Domain Events Integration:</strong></para>
/// Integrated domain events support enables:
/// <list type="bullet">
/// <item>Comprehensive audit trail automation without coupling</item>
/// <item>Event-driven architecture for cross-cutting concerns</item>
/// <item>Loose coupling between domain components</item>
/// <item>Scalable notification and integration patterns</item>
/// <item>Professional legal practice compliance automation</item>
/// </list>
/// 
/// <para><strong>Aggregate Root Support:</strong></para>
/// This base class implements IAggregateRoot, enabling entities to serve as
/// aggregate roots in DDD architectures. Aggregate roots control access to
/// their internal entities and maintain consistency boundaries.
/// 
/// <para><strong>Legal Document Management Benefits:</strong></para>
/// For legal practice systems, this foundation provides:
/// <list type="bullet">
/// <item>Consistent entity identity for legal document tracking</item>
/// <item>Immutable audit trails through domain events</item>
/// <item>Professional responsibility compliance support</item>
/// <item>Client confidentiality protection through controlled access</item>
/// <item>Regulatory compliance through comprehensive event tracking</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Implementing a concrete entity
/// public class Document : Entity&lt;DocumentId&gt;
/// {
///     public FileName FileName { get; private set; }
///     public bool IsDeleted { get; private set; }
///     
///     public static Result&lt;Document&gt; Create(FileName fileName, UserId createdBy)
///     {
///         var document = new Document(DocumentId.New(), fileName);
///         
///         // Domain events are automatically managed by base class
///         document.AddDomainEvent(new DocumentCreatedDomainEvent(
///             document.Id, fileName.Value, createdBy));
///             
///         return Result.Success(document);
///     }
///     
///     public Result Delete(UserId userId)
///     {
///         if (IsDeleted)
///             return Result.Failure(new DomainError("ALREADY_DELETED", "Document is already deleted"));
///             
///         IsDeleted = true;
///         AddDomainEvent(new DocumentDeletedDomainEvent(Id, userId));
///         return Result.Success();
///     }
/// }
/// 
/// // Entity equality and comparison
/// var doc1 = Document.Create(fileName, userId).Value;
/// var doc2 = Document.Create(fileName, userId).Value;
/// 
/// Console.WriteLine(doc1 == doc2); // False - different identifiers
/// Console.WriteLine(doc1.Equals(doc1)); // True - same entity
/// 
/// // Domain events are automatically collected
/// Console.WriteLine($"Document has {doc1.DomainEvents.Count} events"); // 1 event
/// </code>
/// </example>
public abstract class Entity<TId> : IEquatable<Entity<TId>>, IAggregateRoot
    where TId : class
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    /// <value>The strong-typed identifier for this entity instance.</value>
    /// <remarks>
    /// The identifier serves as the primary means of entity identification and equality
    /// comparison. It must be unique within the entity type and should remain immutable
    /// throughout the entity's lifecycle.
    /// 
    /// <para><strong>Identity Characteristics:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Immutable:</strong> Cannot be changed after entity creation</item>
    /// <item><strong>Unique:</strong> No two entities of the same type share an identifier</item>
    /// <item><strong>Strong-Typed:</strong> Uses value objects instead of primitives</item>
    /// <item><strong>Non-Nullable:</strong> Always has a valid identifier value</item>
    /// </list>
    /// 
    /// <para><strong>Usage in Legal Context:</strong></para>
    /// In legal document management, entity identifiers provide the foundation for:
    /// <list type="bullet">
    /// <item>Document referencing and citation</item>
    /// <item>Audit trail entity correlation</item>
    /// <item>Client matter organization</item>
    /// <item>Professional responsibility tracking</item>
    /// </list>
    /// 
    /// <para><strong>Protected Setter:</strong></para>
    /// The protected setter allows derived classes to set the identifier during
    /// construction while preventing external modification that could compromise
    /// entity identity and system integrity.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing entity identifier
    /// DocumentId documentId = document.Id;
    /// 
    /// // Using identifier for operations
    /// var foundDocument = repository.GetById(document.Id);
    /// var isMatch = foundDocument.Id == document.Id; // True
    /// 
    /// // Identifier remains constant throughout lifecycle
    /// document.UpdateFileName(newName);
    /// Console.WriteLine(document.Id); // Same identifier as before
    /// </code>
    /// </example>
    public TId Id { get; protected set; } = null!;

    /// <summary>
    /// Gets the collection of domain events raised by this entity.
    /// </summary>
    /// <value>A read-only collection of domain events associated with this entity.</value>
    /// <remarks>
    /// Domain events represent significant business occurrences that have happened
    /// to this entity. They enable event-driven architecture patterns and support
    /// comprehensive audit trail automation without coupling domain logic to
    /// cross-cutting concerns.
    /// 
    /// <para><strong>Event Management:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Read-Only Access:</strong> External code can observe but not modify events</item>
    /// <item><strong>Automatic Collection:</strong> Events are collected as domain operations occur</item>
    /// <item><strong>Dispatch Integration:</strong> Events are dispatched by infrastructure after persistence</item>
    /// <item><strong>Audit Trail Source:</strong> Events provide the foundation for audit trail automation</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance Integration:</strong></para>
    /// Domain events support legal practice requirements by:
    /// <list type="bullet">
    /// <item>Providing immutable records of business operations</item>
    /// <item>Enabling comprehensive audit trail generation</item>
    /// <item>Supporting professional responsibility documentation</item>
    /// <item>Facilitating client confidentiality protection</item>
    /// <item>Enabling regulatory compliance automation</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// Events are stored in memory during the entity's lifecycle and dispatched
    /// after successful persistence. This approach ensures consistency while
    /// maintaining performance and avoiding premature event processing.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Creating an entity that raises events
    /// var document = Document.Create(fileName, userId).Value;
    /// Console.WriteLine($"Events raised: {document.DomainEvents.Count}"); // 1
    /// 
    /// // Performing operations that raise additional events
    /// document.CheckOut(userId);
    /// document.CheckIn(userId);
    /// Console.WriteLine($"Total events: {document.DomainEvents.Count}"); // 3
    /// 
    /// // Events are cleared after dispatch
    /// await dbContext.SaveChangesAsync(); // Events are dispatched here
    /// Console.WriteLine($"Events after save: {document.DomainEvents.Count}"); // 0
    /// 
    /// // Accessing event information
    /// foreach (var domainEvent in document.DomainEvents)
    /// {
    ///     Console.WriteLine($"Event: {domainEvent.GetType().Name} at {domainEvent.OccurredOn}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to this entity's event collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    /// <remarks>
    /// This protected method allows derived entities to raise domain events when
    /// significant business operations occur. Events are collected and later
    /// dispatched by the infrastructure layer after successful persistence.
    /// 
    /// <para><strong>Usage Guidelines:</strong></para>
    /// <list type="bullet">
    /// <item>Raise events immediately after the business operation occurs</item>
    /// <item>Include all relevant context in the event data</item>
    /// <item>Use specific event types rather than generic events</item>
    /// <item>Consider the business significance of the operation</item>
    /// <item>Ensure event data is immutable and serializable</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Events:</strong></para>
    /// In legal document management, typical events might include:
    /// <list type="bullet">
    /// <item>Document creation, modification, or deletion</item>
    /// <item>Matter archival or status changes</item>
    /// <item>User access and authorization changes</item>
    /// <item>Document check-in and check-out operations</item>
    /// <item>Revision creation and version control events</item>
    /// </list>
    /// 
    /// <para><strong>Event Timing:</strong></para>
    /// Events should be added immediately after the business operation succeeds
    /// but before the entity is persisted. This ensures that events accurately
    /// reflect the state changes and are available for processing after
    /// successful persistence.
    /// </remarks>
    /// <example>
    /// <code>
    /// // In a domain entity method
    /// public Result CheckOut(UserId userId)
    /// {
    ///     if (IsCheckedOut)
    ///         return Result.Failure(new DomainError("ALREADY_CHECKED_OUT", "Document is already checked out"));
    ///     
    ///     // Perform business operation
    ///     IsCheckedOut = true;
    ///     
    ///     // Raise domain event immediately after successful operation
    ///     AddDomainEvent(new DocumentCheckedOutDomainEvent(Id, userId));
    ///     
    ///     return Result.Success();
    /// }
    /// 
    /// // Events can include rich context
    /// public Result UpdateFileName(FileName newFileName, UserId updatedBy)
    /// {
    ///     var oldFileName = FileName;
    ///     FileName = newFileName;
    ///     
    ///     // Event includes both old and new values for comprehensive audit trail
    ///     AddDomainEvent(new DocumentFileNameUpdatedDomainEvent(Id, oldFileName, newFileName, updatedBy));
    ///     
    ///     return Result.Success();
    /// }
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is null.</exception>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from this entity's event collection.
    /// </summary>
    /// <remarks>
    /// This method is called by the infrastructure layer after domain events have
    /// been successfully dispatched to their handlers. It prevents events from
    /// being processed multiple times and keeps the entity's memory footprint manageable.
    /// 
    /// <para><strong>Infrastructure Usage:</strong></para>
    /// This method is typically called by:
    /// <list type="bullet">
    /// <item>DbContext implementations after successful SaveChanges operations</item>
    /// <item>Event dispatching infrastructure after processing all events</item>
    /// <item>Unit of work implementations as part of transaction completion</item>
    /// </list>
    /// 
    /// <para><strong>Timing Considerations:</strong></para>
    /// Events should only be cleared after:
    /// <list type="bullet">
    /// <item>Successful persistence of entity changes to the database</item>
    /// <item>Successful dispatch of all domain events to their handlers</item>
    /// <item>Completion of any transaction boundaries</item>
    /// </list>
    /// 
    /// <para><strong>Error Handling:</strong></para>
    /// If event processing fails, events should not be cleared to enable
    /// retry mechanisms and ensure no events are lost. The infrastructure
    /// should handle event processing failures appropriately.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Typical usage in DbContext
    /// public override async Task&lt;int&gt; SaveChangesAsync(CancellationToken cancellationToken = default)
    /// {
    ///     // Collect events before saving
    ///     var domainEvents = CollectDomainEvents();
    ///     
    ///     // Save changes to database
    ///     var result = await base.SaveChangesAsync(cancellationToken);
    ///     
    ///     // Clear events from entities after successful save
    ///     ClearDomainEvents();
    ///     
    ///     // Dispatch events after successful persistence
    ///     await _eventDispatcher.DispatchAsync(domainEvents, cancellationToken);
    ///     
    ///     return result;
    /// }
    /// 
    /// private void ClearDomainEvents()
    /// {
    ///     var entities = ChangeTracker.Entries&lt;Entity&lt;object&gt;&gt;()
    ///         .Select(x => x.Entity)
    ///         .ToList();
    ///         
    ///     foreach (var entity in entities)
    ///     {
    ///         entity.ClearDomainEvents(); // Called on each entity
    ///     }
    /// }
    /// </code>
    /// </example>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Determines whether the specified entity is equal to the current entity.
    /// </summary>
    /// <param name="other">The entity to compare with the current entity.</param>
    /// <returns>true if the specified entity is equal to the current entity; otherwise, false.</returns>
    /// <remarks>
    /// Entity equality is based solely on identifier comparison, following Domain-Driven Design
    /// principles. Two entities are considered equal if they have the same identifier,
    /// regardless of their other property values.
    /// 
    /// <para><strong>DDD Identity Principle:</strong></para>
    /// In DDD, entities are distinguished by their identity, not their attributes.
    /// This means that two entities with the same identifier are considered the
    /// same entity, even if they have different attribute values (which might
    /// represent different states of the entity over time).
    /// 
    /// <para><strong>Null and Reference Handling:</strong></para>
    /// <list type="bullet">
    /// <item>Returns false if the other entity is null</item>
    /// <item>Returns true if comparing with the same reference</item>
    /// <item>Uses identifier equality for different references</item>
    /// <item>Handles value object identifier comparison properly</item>
    /// </list>
    /// 
    /// <para><strong>Legal Document Context:</strong></para>
    /// In legal document management, entity equality enables:
    /// <list type="bullet">
    /// <item>Consistent document identification across system layers</item>
    /// <item>Reliable audit trail correlation</item>
    /// <item>Proper collection and set operations</item>
    /// <item>Database relationship integrity</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var document1 = Document.Create(fileName, userId).Value;
    /// var document2 = repository.GetById(document1.Id);
    /// 
    /// // Entities with same identifier are equal
    /// Console.WriteLine(document1.Equals(document2)); // True
    /// 
    /// // Even if attributes differ
    /// document1.UpdateFileName(newFileName);
    /// Console.WriteLine(document1.Equals(document2)); // Still True - same identity
    /// 
    /// // Different entities are not equal
    /// var document3 = Document.Create(fileName, userId).Value;
    /// Console.WriteLine(document1.Equals(document3)); // False - different identifiers
    /// 
    /// // Null comparison
    /// Console.WriteLine(document1.Equals(null)); // False
    /// 
    /// // Reference equality
    /// Console.WriteLine(document1.Equals(document1)); // True
    /// </code>
    /// </example>
    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>true if the specified object is equal to the current entity; otherwise, false.</returns>
    /// <remarks>
    /// This method provides standard .NET object equality semantics by delegating to
    /// the strongly-typed Equals method after appropriate type checking.
    /// 
    /// <para><strong>Type Safety:</strong></para>
    /// The method performs proper type checking to ensure that only entities of
    /// the same type with compatible identifiers are compared for equality.
    /// </remarks>
    /// <example>
    /// <code>
    /// var document = Document.Create(fileName, userId).Value;
    /// object documentAsObject = document;
    /// 
    /// // Object equality delegates to typed equality
    /// Console.WriteLine(document.Equals(documentAsObject)); // True
    /// 
    /// // Different types return false
    /// var matter = Matter.Create(description, userId).Value;
    /// Console.WriteLine(document.Equals(matter)); // False
    /// 
    /// // Non-entity objects return false
    /// Console.WriteLine(document.Equals("string")); // False
    /// </code>
    /// </example>
    public override bool Equals(object? obj) =>
        obj is Entity<TId> entity && Equals(entity);

    /// <summary>
    /// Serves as the default hash function for the entity.
    /// </summary>
    /// <returns>A hash code for the current entity based on its identifier.</returns>
    /// <remarks>
    /// The hash code is based solely on the entity's identifier, ensuring that
    /// entities with the same identifier produce the same hash code. This is
    /// essential for proper behavior in hash-based collections like HashSet
    /// and Dictionary.
    /// 
    /// <para><strong>Hash Code Consistency:</strong></para>
    /// <list type="bullet">
    /// <item>Entities with equal identifiers have equal hash codes</item>
    /// <item>Hash code remains constant throughout entity lifecycle</item>
    /// <item>Compatible with equality semantics</item>
    /// <item>Suitable for use in hash-based collections</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// The hash code delegates to the identifier's hash code implementation,
    /// which should be efficient and well-distributed for optimal collection
    /// performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// var document1 = Document.Create(fileName, userId).Value;
    /// var document2 = repository.GetById(document1.Id);
    /// 
    /// // Equal entities have equal hash codes
    /// Console.WriteLine(document1.GetHashCode() == document2.GetHashCode()); // True
    /// 
    /// // Using in HashSet
    /// var documentSet = new HashSet&lt;Document&gt; { document1, document2 };
    /// Console.WriteLine(documentSet.Count); // 1 - treated as same entity
    /// 
    /// // Using as Dictionary key
    /// var documentMap = new Dictionary&lt;Document, string&gt;
    /// {
    ///     [document1] = "First entry",
    ///     [document2] = "Second entry"  // Overwrites first entry
    /// };
    /// Console.WriteLine(documentMap.Count); // 1
    /// </code>
    /// </example>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Determines whether two entities are equal using the equality operator.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns>true if the entities are equal; otherwise, false.</returns>
    /// <remarks>
    /// This operator provides syntactic convenience for entity equality comparison
    /// while maintaining the same semantics as the Equals method. It properly
    /// handles null references on either side of the comparison.
    /// </remarks>
    /// <example>
    /// <code>
    /// var doc1 = Document.Create(fileName, userId).Value;
    /// var doc2 = repository.GetById(doc1.Id);
    /// var doc3 = Document.Create(fileName, userId).Value;
    /// 
    /// Console.WriteLine(doc1 == doc2); // True - same identifier
    /// Console.WriteLine(doc1 == doc3); // False - different identifiers
    /// Console.WriteLine(doc1 == null); // False - null comparison
    /// Console.WriteLine(null == (Document?)null); // True - both null
    /// </code>
    /// </example>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        EqualityComparer<Entity<TId>>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two entities are not equal using the inequality operator.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns>true if the entities are not equal; otherwise, false.</returns>
    /// <remarks>
    /// This operator provides the logical negation of the equality operator,
    /// offering convenient syntax for inequality comparisons while maintaining
    /// consistent semantics with the equality implementation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var doc1 = Document.Create(fileName, userId).Value;
    /// var doc2 = repository.GetById(doc1.Id);
    /// var doc3 = Document.Create(fileName, userId).Value;
    /// 
    /// Console.WriteLine(doc1 != doc2); // False - same identifier
    /// Console.WriteLine(doc1 != doc3); // True - different identifiers
    /// Console.WriteLine(doc1 != null); // True - not equal to null
    /// </code>
    /// </example>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !(left == right);
}