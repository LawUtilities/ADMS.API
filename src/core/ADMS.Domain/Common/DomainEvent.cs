namespace ADMS.Domain.Common;

/// <summary>
/// Abstract base record for domain events with automatic ID generation and UTC timestamp capture.
/// </summary>
/// <remarks>
/// <para>
/// DomainEvent serves as the foundation for all domain events in the ADMS system, providing
/// essential infrastructure for event-driven architecture and audit trail automation.
/// Each event receives a unique identifier and UTC timestamp for consistent tracking
/// across the system.
/// </para>
/// <para>
/// Domain events capture significant business occurrences within the domain, enabling
/// loose coupling between domain components and supporting audit trails, notifications,
/// and business process automation.
/// </para>
/// <para>
/// <strong>Event Processing Lifecycle:</strong>
/// </para>
/// <list type="number">
/// <item>Domain operation occurs (e.g., document creation)</item>
/// <item>Domain event is created and added to aggregate</item>
/// <item>Event is automatically assigned unique ID and timestamp</item>
/// <item>Event is dispatched after successful persistence</item>
/// <item>Event handlers process the event for various concerns (audit, notifications, etc.)</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a concrete domain event
/// public sealed record DocumentCreatedDomainEvent(
///     DocumentId DocumentId,
///     string FileName,
///     UserId CreatedBy
/// ) : DomainEvent;
/// 
/// // Using in domain entity
/// public class Document : Entity&lt;DocumentId&gt;
/// {
///     public static Result&lt;Document&gt; Create(string fileName, UserId createdBy)
/// {
///         var document = new Document(fileName);
///         
///         // Domain event is automatically assigned ID and timestamp
///         document.AddDomainEvent(new DocumentCreatedDomainEvent(
///             document.Id, fileName, createdBy));
///             
///         return Result.Success(document);
///     }
/// }
/// </code>
/// </example>
public abstract record DomainEvent : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this domain event.
    /// </summary>
    /// <value>A unique GUID automatically generated when the event is created.</value>
    /// <remarks>
    /// The event ID provides unique identification for each domain event instance,
    /// enabling precise event tracking, deduplication, and correlation analysis
    /// across distributed systems and audit trail processing.
    /// </remarks>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the UTC timestamp when this domain event occurred.
    /// </summary>
    /// <value>A UTC DateTime representing when the event was created.</value>
    /// <remarks>
    /// The occurrence timestamp captures the precise moment when the domain event was created,
    /// providing essential temporal data for audit trails, business process analysis, and
    /// legal compliance requirements. All timestamps are stored in UTC to ensure consistency
    /// across different time zones and system deployments.
    /// </remarks>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the name of the event type.
    /// </summary>
    /// <value>The simple name of the concrete event type.</value>
    public string EventName => GetType().Name;

    /// <summary>
    /// Gets the full type name of the event.
    /// </summary>
    /// <value>The full type name including namespace.</value>
    public string EventType => GetType().FullName ?? GetType().Name;

    /// <summary>
    /// Returns a string representation of the domain event.
    /// </summary>
    /// <returns>A string containing the event name, ID, and timestamp.</returns>
    public override sealed string ToString() => $"{EventName} [Id={Id}, OccurredOn={OccurredOn:yyyy-MM-dd HH:mm:ss} UTC]";
}