using ADMS.Domain.Common;

namespace ADMS.Infrastructure.Events;

/// <summary>
/// Defines the contract for dispatching domain events to their respective handlers.
/// </summary>
/// <remarks>
/// The domain event dispatcher is responsible for routing domain events raised by entities
/// to their appropriate handlers. This enables decoupled, event-driven architecture where
/// business logic can trigger side effects (like audit trails) without direct dependencies.
/// 
/// <para><strong>Key Responsibilities:</strong></para>
/// <list type="bullet">
/// <item><strong>Event Routing:</strong> Routes events to all registered handlers</item>
/// <item><strong>Error Handling:</strong> Manages handler failures without breaking the main flow</item>
/// <item><strong>Performance:</strong> Executes handlers efficiently, potentially in parallel</item>
/// <item><strong>Reliability:</strong> Ensures critical events are processed successfully</item>
/// </list>
/// 
/// <para><strong>Usage Patterns:</strong></para>
/// <list type="bullet">
/// <item>Called from DbContext.SaveChanges to process entity events</item>
/// <item>Used in application services for explicit event processing</item>
/// <item>Integrated with background job processing for non-critical events</item>
/// </list>
/// </remarks>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches a single domain event to all registered handlers.
    /// </summary>
    /// <param name="domainEvent">The domain event to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A task representing the dispatch operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when domainEvent is null.</exception>
    /// <exception cref="DomainEventDispatchException">Thrown when critical handlers fail.</exception>
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches multiple domain events to their respective handlers.
    /// </summary>
    /// <param name="domainEvents">The collection of domain events to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A task representing the dispatch operations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when domainEvents is null.</exception>
    /// <exception cref="DomainEventDispatchException">Thrown when critical handlers fail.</exception>
    Task DispatchManyAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches domain events and collects results from all handlers.
    /// </summary>
    /// <param name="domainEvent">The domain event to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A collection of dispatch results from all handlers.</returns>
    /// <remarks>
    /// This method is useful when you need to know the outcome of each handler,
    /// such as for debugging, monitoring, or when handler results affect business logic.
    /// </remarks>
    Task<IEnumerable<DomainEventDispatchResult>> DispatchWithResultsAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of registered handlers for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The domain event type.</typeparam>
    /// <returns>The number of registered handlers for the event type.</returns>
    /// <remarks>
    /// Useful for diagnostics and ensuring proper handler registration during application startup.
    /// </remarks>
    int GetHandlerCount<TEvent>() where TEvent : class, IDomainEvent;

    /// <summary>
    /// Validates that all expected handlers are registered for critical domain events.
    /// </summary>
    /// <returns>A validation result indicating any missing or misconfigured handlers.</returns>
    /// <remarks>
    /// This method should be called during application startup to ensure all critical
    /// domain events have appropriate handlers registered, especially audit trail handlers.
    /// </remarks>
    Task<DomainEventHandlerValidationResult> ValidateHandlerRegistrationAsync();
}

/// <summary>
/// Represents the result of dispatching a domain event to a specific handler.
/// </summary>
/// <param name="HandlerType">The type of handler that processed the event.</param>
/// <param name="Success">Whether the handler processed the event successfully.</param>
/// <param name="Error">The error that occurred, if any.</param>
/// <param name="ExecutionTime">The time taken to execute the handler.</param>
/// <param name="EventType">The type of domain event that was processed.</param>
public sealed record DomainEventDispatchResult(
    Type HandlerType,
    bool Success,
    Exception? Error,
    TimeSpan ExecutionTime,
    Type EventType)
{
    /// <summary>
    /// Gets a user-friendly description of the dispatch result.
    /// </summary>
    public string Description => Success
        ? $"{HandlerType.Name} successfully processed {EventType.Name} in {ExecutionTime.TotalMilliseconds:F1}ms"
        : $"{HandlerType.Name} failed to process {EventType.Name}: {Error?.Message}";
}

/// <summary>
/// Represents the result of validating domain event handler registration.
/// </summary>
/// <param name="IsValid">Whether all required handlers are properly registered.</param>
/// <param name="MissingHandlers">List of event types missing required handlers.</param>
/// <param name="ExtraHandlers">List of handlers registered for unknown event types.</param>
/// <param name="ValidationSummary">Summary of validation results.</param>
public sealed record DomainEventHandlerValidationResult(
    bool IsValid,
    IReadOnlyList<Type> MissingHandlers,
    IReadOnlyList<Type> ExtraHandlers,
    string ValidationSummary);

/// <summary>
/// Exception thrown when domain event dispatch operations fail critically.
/// </summary>
public sealed class DomainEventDispatchException : Exception
{
    public DomainEventDispatchException(string message) : base(message) { }

    public DomainEventDispatchException(string message, Exception innerException) : base(message, innerException) { }

    public DomainEventDispatchException(string message, IEnumerable<DomainEventDispatchResult> failedResults)
        : base(message)
    {
        FailedResults = failedResults.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the results from handlers that failed during dispatch.
    /// </summary>
    public IReadOnlyList<DomainEventDispatchResult>? FailedResults { get; }
}