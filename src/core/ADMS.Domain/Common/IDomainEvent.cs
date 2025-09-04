namespace ADMS.Domain.Common;

/// <summary>
/// Interface that defines the contract for domain events within the ADMS system.
/// </summary>
/// <remarks>
/// IDomainEvent establishes the fundamental contract for all domain events in the system,
/// ensuring consistent event identification and temporal tracking across all business
/// operations. This interface supports Domain-Driven Design principles by providing
/// the foundation for event-driven architecture and comprehensive audit trail automation.
/// 
/// <para><strong>Domain Event Principles:</strong></para>
/// Domain events represent significant business occurrences that have happened within
/// the domain model. They capture the "what happened" rather than the "what should happen,"
/// making them historical records of business activities that support various system
/// concerns without coupling domain logic to infrastructure.
/// 
/// <para><strong>Event-Driven Architecture Benefits:</strong></para>
/// <list type="bullet">
/// <item><strong>Loose Coupling:</strong> Domain operations don't directly depend on side effects</item>
/// <item><strong>Separation of Concerns:</strong> Audit trails, notifications, and integrations are separate</item>
/// <item><strong>Scalability:</strong> Event processing can be scaled independently</item>
/// <item><strong>Maintainability:</strong> New event handlers can be added without changing domain logic</item>
/// <item><strong>Testability:</strong> Domain logic and event processing can be tested independently</item>
/// </list>
/// 
/// <para><strong>Legal Practice Integration:</strong></para>
/// In legal document management systems, domain events provide the foundation for:
/// <list type="bullet">
/// <item><strong>Audit Trail Automation:</strong> Every business operation generates traceable events</item>
/// <item><strong>Professional Responsibility:</strong> Complete activity attribution and documentation</item>
/// <item><strong>Client Confidentiality:</strong> Controlled event processing respects confidentiality boundaries</item>
/// <item><strong>Regulatory Compliance:</strong> Comprehensive event logs support compliance requirements</item>
/// <item><strong>Legal Discovery:</strong> Event trails provide evidence for legal proceedings</item>
/// </list>
/// 
/// <para><strong>Event Processing Pipeline:</strong></para>
/// <list type="number">
/// <item>Domain operation occurs in aggregate root</item>
/// <item>Domain event implementing IDomainEvent is created</item>
/// <item>Event is added to aggregate root's event collection</item>
/// <item>Infrastructure persists aggregate state changes</item>
/// <item>Infrastructure dispatches events to registered handlers</item>
/// <item>Event handlers process cross-cutting concerns (audit, notifications, etc.)</item>
/// </list>
/// 
/// <para><strong>Temporal Consistency:</strong></para>
/// All domain events include precise UTC timestamps, ensuring consistent temporal
/// ordering across distributed systems and supporting accurate timeline reconstruction
/// for legal and business analysis purposes.
/// 
/// <para><strong>Event Immutability:</strong></para>
/// Domain events should be immutable after creation to ensure audit trail integrity
/// and prevent tampering with historical business activity records. The record-based
/// implementation pattern supports this immutability requirement.
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
///         // Access event-specific properties
///         await CreateAuditEntry(domainEvent.DocumentId, domainEvent.CreatedBy, 
///             domainEvent.OccurredOn);
///     }
/// }
/// 
/// // Infrastructure dispatches events by IDomainEvent interface
/// public async Task DispatchEventsAsync(IEnumerable&lt;IDomainEvent&gt; events)
/// {
///     foreach (var domainEvent in events)
///     {
///         // Standard event processing using IDomainEvent properties
///         await ProcessEvent(domainEvent);
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
    /// 
    /// <para><strong>Uniqueness Guarantee:</strong></para>
    /// Each domain event instance receives a globally unique identifier that remains
    /// constant throughout the event's lifecycle. This identifier is typically
    /// generated automatically when the event is created and should never be modified.
    /// 
    /// <para><strong>Event Correlation:</strong></para>
    /// Event IDs enable:
    /// <list type="bullet">
    /// <item>Correlation across distributed event processing systems</item>
    /// <item>Event deduplication in at-least-once delivery scenarios</item>
    /// <item>Audit trail correlation and cross-referencing</item>
    /// <item>Event sourcing and replay scenario identification</item>
    /// <item>Monitoring and observability event tracking</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Applications:</strong></para>
    /// In legal document management, event IDs support:
    /// <list type="bullet">
    /// <item>Precise audit trail event identification for compliance</item>
    /// <item>Event correlation across multiple system boundaries</item>
    /// <item>Professional responsibility documentation with specific event references</item>
    /// <item>Legal discovery with exact event identification and sequencing</item>
    /// </list>
    /// 
    /// <para><strong>Infrastructure Integration:</strong></para>
    /// Infrastructure components use event IDs for:
    /// <list type="bullet">
    /// <item>Event handler dispatch and routing</item>
    /// <item>Event persistence and storage operations</item>
    /// <item>Error handling and retry mechanism identification</item>
    /// <item>Performance monitoring and event processing analytics</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Event ID is automatically generated and remains constant
    /// var documentEvent = new DocumentCreatedDomainEvent(documentId, fileName, userId);
    /// var eventId = documentEvent.Id; // Unique GUID
    /// 
    /// // Use in event correlation
    /// _logger.LogInformation("Processing event {EventId} for document {DocumentId}",
    ///     documentEvent.Id, documentEvent.DocumentId);
    /// 
    /// // Infrastructure event processing
    /// public async Task ProcessEventWithTracking(IDomainEvent domainEvent)
    /// {
    ///     using var activity = _activitySource.StartActivity($"ProcessEvent-{domainEvent.Id}");
    ///     
    ///     try
    ///     {
    ///         await ProcessEvent(domainEvent);
    ///         _metrics.IncrementEventProcessed(domainEvent.GetType().Name);
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         _logger.LogError(ex, "Failed to process event {EventId} of type {EventType}",
    ///             domainEvent.Id, domainEvent.GetType().Name);
    ///         throw;
    ///     }
    /// }
    /// 
    /// // Event deduplication
    /// private readonly HashSet&lt;Guid&gt; _processedEvents = new();
    /// 
    /// public async Task ProcessEventOnce(IDomainEvent domainEvent)
    /// {
    ///     if (_processedEvents.Contains(domainEvent.Id))
    ///     {
    ///         _logger.LogDebug("Skipping already processed event {EventId}", domainEvent.Id);
    ///         return;
    ///     }
    ///     
    ///     await ProcessEvent(domainEvent);
    ///     _processedEvents.Add(domainEvent.Id);
    /// }
    /// </code>
    /// </example>
    Guid Id { get; }

    /// <summary>
    /// Gets the UTC timestamp indicating when this domain event occurred.
    /// </summary>
    /// <value>A UTC DateTime representing the precise moment when the event was created.</value>
    /// <remarks>
    /// The occurrence timestamp provides essential temporal context for domain events,
    /// enabling accurate timeline reconstruction, event sequencing, and temporal analysis
    /// across the legal document management system. All timestamps are standardized to
    /// UTC to ensure consistency across different time zones and system deployments.
    /// 
    /// <para><strong>Temporal Precision:</strong></para>
    /// The timestamp captures the exact moment when the business event occurred,
    /// not when it was processed or persisted. This distinction is crucial for
    /// maintaining accurate business timelines and supporting legal requirements
    /// for precise activity documentation.
    /// 
    /// <para><strong>UTC Standardization Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Global Consistency:</strong> Works correctly across all time zones</item>
    /// <item><strong>Daylight Saving Independence:</strong> Unaffected by DST transitions</item>
    /// <item><strong>Legal Documentation:</strong> Provides consistent timestamps for legal records</item>
    /// <item><strong>System Integration:</strong> Enables reliable cross-system event ordering</item>
    /// <item><strong>Audit Compliance:</strong> Meets regulatory requirements for time tracking</item>
    /// </list>
    /// 
    /// <para><strong>Legal Practice Applications:</strong></para>
    /// In legal document management, precise timestamps support:
    /// <list type="bullet">
    /// <item><strong>Professional Responsibility:</strong> Accurate activity timing for ethical compliance</item>
    /// <item><strong>Client Billing:</strong> Precise time tracking for billable activities</item>
    /// <item><strong>Legal Discovery:</strong> Accurate timeline reconstruction for litigation</item>
    /// <item><strong>Regulatory Compliance:</strong> Meeting time-based compliance requirements</item>
    /// <item><strong>Audit Trails:</strong> Comprehensive temporal documentation</item>
    /// </list>
    /// 
    /// <para><strong>Event Sequencing:</strong></para>
    /// Timestamps enable proper event ordering even in distributed systems,
    /// supporting event sourcing patterns and ensuring that business activity
    /// timelines can be accurately reconstructed from event histories.
    /// 
    /// <para><strong>Performance Monitoring:</strong></para>
    /// Event timestamps enable analysis of system performance, business process
    /// efficiency, and user activity patterns, supporting continuous improvement
    /// of legal practice workflows.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing event timestamp
    /// var documentEvent = new DocumentCreatedDomainEvent(documentId, fileName, userId);
    /// var eventTime = documentEvent.OccurredOn; // UTC timestamp
    /// 
    /// Console.WriteLine($"Document created at: {eventTime:yyyy-MM-dd HH:mm:ss} UTC");
    /// 
    /// // Event sequencing and timeline analysis
    /// public class EventTimelineAnalyzer
    /// {
    ///     public TimeSpan CalculateProcessingDelay(IDomainEvent domainEvent)
    ///     {
    ///         return DateTime.UtcNow - domainEvent.OccurredOn;
    ///     }
    ///     
    ///     public IEnumerable&lt;IDomainEvent&gt; OrderEventsByTime(IEnumerable&lt;IDomainEvent&gt; events)
    ///     {
    ///         return events.OrderBy(e => e.OccurredOn);
    ///     }
    ///     
    ///     public bool IsEventRecent(IDomainEvent domainEvent, TimeSpan threshold)
    ///     {
    ///         return DateTime.UtcNow - domainEvent.OccurredOn &lt;= threshold;
    ///     }
    /// }
    /// 
    /// // Audit trail creation with precise timing
    /// public async Task CreateAuditEntry(IDomainEvent domainEvent, string activityType)
    /// {
    ///     var auditEntry = new AuditTrailEntry
    ///     {
    ///         EventId = domainEvent.Id,
    ///         EventType = domainEvent.GetType().Name,
    ///         OccurredAt = domainEvent.OccurredOn, // Use event's timestamp, not current time
    ///         ActivityType = activityType,
    ///         ProcessedAt = DateTime.UtcNow // When we processed it
    ///     };
    ///     
    ///     await _auditRepository.AddAsync(auditEntry);
    /// }
    /// 
    /// // Legal timeline reconstruction
    /// public async Task&lt;IEnumerable&lt;IDomainEvent&gt;&gt; GetDocumentTimelineAsync(
    ///     DocumentId documentId, 
    ///     DateTime fromDate, 
    ///     DateTime toDate)
    /// {
    ///     var events = await _eventStore.GetEventsAsync(documentId, fromDate, toDate);
    ///     return events.OrderBy(e => e.OccurredOn); // Chronological order
    /// }
    /// 
    /// // SLA and performance monitoring
    /// public class EventPerformanceMonitor
    /// {
    ///     public void TrackEventProcessingTime(IDomainEvent domainEvent)
    ///     {
    ///         var processingDelay = DateTime.UtcNow - domainEvent.OccurredOn;
    ///         
    ///         _telemetry.TrackMetric("EventProcessingDelay", processingDelay.TotalMilliseconds, 
    ///             new Dictionary&lt;string, string&gt;
    ///             {
    ///                 ["EventType"] = domainEvent.GetType().Name,
    ///                 ["EventId"] = domainEvent.Id.ToString()
    ///             });
    ///     }
    /// }
    /// </code>
    /// </example>
    DateTime OccurredOn { get; }
}