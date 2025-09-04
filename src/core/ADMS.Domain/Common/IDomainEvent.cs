namespace ADMS.Domain.Common;

/// <summary>
/// Interface for domain events that occur within the domain.
/// </summary>
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
