namespace ADMS.Domain.Common;

/// <summary>
/// Abstract base record for domain events with automatic ID generation and UTC timestamp capture.
/// </summary>
/// <remarks>
/// DomainEvent serves as the foundation for all domain events in the ADMS system, providing
/// essential infrastructure for event-driven architecture and comprehensive audit trail automation.
/// This base implementation ensures consistent event identification and temporal tracking
/// across all domain operations.
/// 
/// <para><strong>Domain-Driven Design Integration:</strong></para>
/// Domain events are a core DDD pattern that captures and communicates important business
/// occurrences within the domain. They enable loose coupling between domain components
/// and support sophisticated audit trails, notifications, and business process automation
/// essential for legal document management systems.
/// 
/// <para><strong>Event Infrastructure Features:</strong></para>
/// <list type="bullet">
/// <item><strong>Unique Identification:</strong> Each event receives a unique GUID for tracking</item>
/// <item><strong>Temporal Precision:</strong> UTC timestamps ensure consistent global timing</item>
/// <item><strong>Immutability:</strong> Record-based implementation prevents event tampering</item>
/// <item><strong>Type Safety:</strong> Strongly-typed events enable compile-time validation</item>
/// <item><strong>Serialization Support:</strong> Compatible with JSON and other serialization formats</item>
/// </list>
/// 
/// <para><strong>Legal Practice Benefits:</strong></para>
/// The event infrastructure supports legal compliance requirements by providing:
/// <list type="bullet">
/// <item>Complete audit trails for all business operations</item>
/// <item>Precise temporal tracking for legal timeline reconstruction</item>
/// <item>Immutable event records for evidence and compliance purposes</item>
/// <item>User attribution and accountability through event data</item>
/// <item>Professional responsibility documentation and reporting</item>
/// </list>
/// 
/// <para><strong>Event Processing Lifecycle:</strong></para>
/// <list type="number">
/// <item>Domain operation occurs (e.g., document creation)</item>
/// <item>Domain event is created and added to aggregate</item>
/// <item>Event is automatically assigned unique ID and timestamp</item>
/// <item>Event is dispatched after successful persistence</item>
/// <item>Event handlers process the event for various concerns (audit, notifications, etc.)</item>
/// </list>
/// 
/// <para><strong>Audit Trail Integration:</strong></para>
/// Domain events form the backbone of the automated audit trail system, enabling
/// comprehensive tracking of all business operations without coupling domain logic
/// to audit trail implementation details. This separation supports maintainability
/// and ensures audit trails remain complete even as business logic evolves.
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
///     {
///         var document = new Document(fileName);
///         
///         // Domain event is automatically assigned ID and timestamp
///         document.AddDomainEvent(new DocumentCreatedDomainEvent(
///             document.Id, fileName, createdBy));
///             
///         return Result.Success(document);
///     }
/// }
/// 
/// // Event handler processing
/// public class DocumentCreatedAuditHandler : IDomainEventHandler&lt;DocumentCreatedDomainEvent&gt;
/// {
///     public async Task Handle(DocumentCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
///     {
///         // Event automatically has unique ID and precise timestamp
///         _logger.LogInformation("Processing document creation: {EventId} at {Timestamp}",
///             domainEvent.Id, domainEvent.OccurredOn);
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
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Event correlation and tracking in distributed processing</item>
    /// <item>Deduplication in event processing pipelines</item>
    /// <item>Audit trail event identification and referencing</item>
    /// <item>Event sourcing and replay scenario identification</item>
    /// <item>Monitoring and observability correlation</item>
    /// </list>
    /// 
    /// <para><strong>Automatic Generation:</strong></para>
    /// The ID is automatically generated using <see cref="Guid.NewGuid()"/> when the event
    /// is instantiated, ensuring uniqueness without requiring explicit ID management
    /// in domain logic. This approach maintains clean domain code while providing
    /// robust event identification infrastructure.
    /// </remarks>
    /// <example>
    /// <code>
    /// var documentEvent = new DocumentCreatedDomainEvent(documentId, fileName, userId);
    /// 
    /// // ID is automatically generated
    /// Console.WriteLine($"Event ID: {documentEvent.Id}");
    /// // Output: Event ID: 12345678-1234-5678-9012-123456789012
    /// 
    /// // Use in logging and correlation
    /// _logger.LogInformation("Processing event {EventId} for document {DocumentId}",
    ///     documentEvent.Id, documentEvent.DocumentId);
    /// </code>
    /// </example>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the UTC timestamp when this domain event occurred.
    /// </summary>
    /// <value>A UTC DateTime representing when the event was created.</value>
    /// <remarks>
    /// The occurrence timestamp captures the precise moment when the domain event was created,
    /// providing essential temporal data for audit trails, business process analysis, and
    /// legal compliance requirements. All timestamps are stored in UTC to ensure consistency
    /// across different time zones and system deployments.
    /// 
    /// <para><strong>Temporal Precision Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Audit Compliance:</strong> Precise timing for legal and regulatory requirements</item>
    /// <item><strong>Business Analysis:</strong> Temporal analysis of business process performance</item>
    /// <item><strong>Event Ordering:</strong> Reliable event sequencing for complex workflows</item>
    /// <item><strong>SLA Monitoring:</strong> Performance measurement and service level tracking</item>
    /// <item><strong>Forensic Analysis:</strong> Detailed timeline reconstruction for investigation</item>
    /// </list>
    /// 
    /// <para><strong>UTC Standardization:</strong></para>
    /// Using UTC ensures temporal consistency across global deployments, different time zones,
    /// and daylight saving time transitions. This standardization is critical for legal
    /// document management where precise timing may have legal significance.
    /// 
    /// <para><strong>Automatic Capture:</strong></para>
    /// The timestamp is automatically captured using <see cref="DateTime.UtcNow"/> when the event
    /// is instantiated, ensuring accurate temporal recording without requiring explicit
    /// timestamp management in domain logic.
    /// </remarks>
    /// <example>
    /// <code>
    /// var documentEvent = new DocumentCreatedDomainEvent(documentId, fileName, userId);
    /// 
    /// // Timestamp is automatically captured in UTC
    /// Console.WriteLine($"Event occurred at: {documentEvent.OccurredOn:yyyy-MM-dd HH:mm:ss} UTC");
    /// // Output: Event occurred at: 2024-03-15 14:30:22 UTC
    /// 
    /// // Use in audit trail creation
    /// var auditEntry = new AuditEntry
    /// {
    ///     EventId = documentEvent.Id,
    ///     Timestamp = documentEvent.OccurredOn,
    ///     Description = "Document created"
    /// };
    /// 
    /// // Calculate processing delay
    /// var processingDelay = DateTime.UtcNow - documentEvent.OccurredOn;
    /// _logger.LogDebug("Event processed with {Delay}ms delay", processingDelay.TotalMilliseconds);
    /// </code>
    /// </example>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}