namespace ADMS.Domain.Common;

/// <summary>
/// Interface that defines the contract for domain events within the ADMS system.
/// </summary>
/// <remarks>
/// <para>
/// IDomainEvent establishes the fundamental contract for all domain events in the system,
/// ensuring consistent event identification and temporal tracking across all business
/// operations. This interface supports Domain-Driven Design principles by providing
/// the foundation for event-driven architecture and audit trail automation.
/// </para>
/// <para>
/// Domain events represent significant business occurrences that have happened within
/// the domain model. They capture the "what happened" rather than the "what should happen,"
/// making them historical records of business activities.
/// </para>
/// <para>
/// <strong>Event Processing Pipeline:</strong>
/// </para>
/// <list type="number">
/// <item>Domain operation occurs in aggregate root</item>
/// <item>Domain event implementing IDomainEvent is created</item>
/// <item>Event is added to aggregate root's event collection</item>
/// <item>Infrastructure persists aggregate state changes</item>
/// <item>Infrastructure dispatches events to registered handlers</item>
/// <item>Event handlers process cross-cutting concerns (audit, notifications, etc.)</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Implementing a concrete domain event
/// public sealed record DocumentCreatedDomainEvent(
///     DocumentId DocumentId,
///     string FileName,
///     UserId CreatedBy
/// ) : DomainEvent; // DomainEvent implements IDomainEvent
/// 
/// // Using domain events in aggregate roots
/// public class Document : Entity&lt;DocumentId&gt;
/// {
///     public static Result&lt;Document&gt; Create(string fileName, UserId createdBy)
///     {
///         var document = new Document(DocumentId.New(), fileName);
///         
///         // Domain event is automatically assigned Id and OccurredOn
///         document.AddDomainEvent(new DocumentCreatedDomainEvent(
///             document.Id, fileName, createdBy));
///             
///         return Result.Success(document);
///     }
/// }
/// 
/// // Event handlers process events implementing IDomainEvent
/// public class DocumentCreatedAuditHandler : IDomainEventHandler&lt;DocumentCreatedDomainEvent&gt;
/// {
///     public async Task Handle(DocumentCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
///     {
///         // Access standard IDomainEvent properties
///         _logger.LogInformation("Processing event {EventId} that occurred at {Timestamp}",
///             domainEvent.Id, domainEvent.OccurredOn);
///             
///         await CreateAuditEntry(domainEvent.DocumentId, domainEvent.CreatedBy, 
///             domainEvent.OccurredOn);
///     }
/// }
/// </code>
/// </example>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this domain event instance.
    /// </summary>
    /// <value>A unique GUID that identifies this specific event occurrence.</value>
    /// <remarks>
    /// The event ID provides unique identification for each domain event instance,
    /// enabling precise event tracking, correlation, and deduplication across
    /// distributed systems and complex event processing scenarios.
    /// </remarks>
    Guid Id { get; }

    /// <summary>
    /// Gets the UTC timestamp indicating when this domain event occurred.
    /// </summary>
    /// <value>A UTC DateTime representing the precise moment when the event was created.</value>
    /// <remarks>
    /// The occurrence timestamp provides essential temporal context for domain events,
    /// enabling accurate timeline reconstruction, event sequencing, and temporal analysis.
    /// All timestamps are standardized to UTC to ensure consistency across different 
    /// time zones and system deployments.
    /// </remarks>
    DateTime OccurredOn { get; }
}