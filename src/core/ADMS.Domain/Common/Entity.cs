namespace ADMS.Domain.Common;

/// <summary>
/// Base class for domain entities with strong-typed identifiers and domain events.
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>, IAggregateRoot
    where TId : class
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public TId Id { get; protected set; } = null!;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj) =>
        obj is Entity<TId> entity && Equals(entity);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        EqualityComparer<Entity<TId>>.Default.Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !(left == right);
}