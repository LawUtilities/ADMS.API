namespace ADMS.Domain.Common;

/// <summary>
/// Base record for domain events with automatic ID and timestamp generation.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}