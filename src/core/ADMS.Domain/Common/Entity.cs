using System.ComponentModel.DataAnnotations;

using ADMS.Domain.Entities.Common;

namespace ADMS.Domain.Common;

/// <summary>
/// Abstract base class for all domain entities with strongly-typed identifiers.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
/// <remarks>
/// This class provides domain-driven design patterns including domain events, 
/// equality comparisons, audit trail support, and transient entity detection.
/// Use this class when you want to enforce type safety for entity identifiers.
/// </remarks>
public abstract class Entity<TId> : IEquatable<Entity<TId>>, IAuditableEntity
    where TId : IEquatable<TId>
{
    private List<IDomainEvent>? _domainEvents;

    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// </summary>
    /// <value>A strongly-typed identifier that uniquely identifies this entity instance.</value>
    [Key]
    public virtual TId Id { get; protected set; } = default!;

    /// <summary>
    /// Gets a value indicating whether this entity is considered transient (not yet persisted).
    /// </summary>
    /// <value>
    /// <c>true</c> if the entity is transient (Id is default value); otherwise, <c>false</c>.
    /// </value>
    public virtual bool IsTransient => EqualityComparer<TId>.Default.Equals(Id, default!);

    #region Audit Properties

    /// <inheritdoc />
    public string CreatedBy { get; set; } = string.Empty;

    /// <inheritdoc />
    public DateTime CreatedDate { get; set; }

    /// <inheritdoc />
    public string LastModifiedBy { get; set; } = string.Empty;

    /// <inheritdoc />
    public DateTime? LastModifiedDate { get; set; }

    #endregion Audit Properties

    #region Domain Events

    /// <summary>
    /// Gets the read-only collection of domain events for this entity.
    /// </summary>
    /// <value>An <see cref="IReadOnlyCollection{T}"/> of domain events.</value>
    public virtual IReadOnlyCollection<IDomainEvent> DomainEvents =>
        _domainEvents?.AsReadOnly() ?? Array.Empty<IDomainEvent>();

    /// <summary>
    /// Adds a domain event to be processed when this entity is persisted.
    /// </summary>
    /// <param name="eventItem">The domain event to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventItem"/> is null.</exception>
    public virtual void AddDomainEvent(IDomainEvent eventItem)
    {
        ArgumentNullException.ThrowIfNull(eventItem);

        _domainEvents ??= new List<IDomainEvent>();
        _domainEvents.Add(eventItem);
    }

    /// <summary>
    /// Removes a specific domain event from the entity.
    /// </summary>
    /// <param name="eventItem">The domain event to remove.</param>
    /// <returns><c>true</c> if the event was successfully removed; otherwise, <c>false</c>.</returns>
    public virtual bool RemoveDomainEvent(IDomainEvent eventItem)
    {
        return _domainEvents?.Remove(eventItem) ?? false;
    }

    /// <summary>
    /// Clears all domain events from this entity.
    /// </summary>
    public virtual void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

    /// <summary>
    /// Determines whether this entity has any pending domain events.
    /// </summary>
    /// <returns><c>true</c> if the entity has domain events; otherwise, <c>false</c>.</returns>
    public virtual bool HasDomainEvents() => _domainEvents?.Count > 0;

    #endregion Domain Events

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified entity is equal to the current entity.
    /// </summary>
    /// <param name="other">The entity to compare with the current entity.</param>
    /// <returns>
    /// <c>true</c> if the specified entity is equal to the current entity; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Two entities are considered equal if they have the same type and the same Id,
    /// and neither entity is transient.
    /// </remarks>
    public virtual bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        if (IsTransient || other.IsTransient) return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>
    /// <c>true</c> if the specified object is equal to the current entity; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    /// <summary>
    /// Returns a hash code for this entity.
    /// </summary>
    /// <returns>
    /// A hash code for the current entity, suitable for use in hashing algorithms and data structures.
    /// </returns>
    public override int GetHashCode()
    {
        // S3249: Avoid using base.GetHashCode() for transient entities.
        // Use a type-based hash code for transient entities to avoid reference-based hash codes.
        return IsTransient
            ? GetType().GetHashCode()
            : EqualityComparer<TId>.Default.GetHashCode(Id!);
    }

    /// <summary>
    /// Determines whether two entity instances are equal.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns><c>true</c> if the entities are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two entity instances are not equal.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns><c>true</c> if the entities are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }

    #endregion Equality and Comparison

    /// <summary>
    /// Returns a string representation of the entity.
    /// </summary>
    /// <returns>A string that represents the current entity.</returns>
    public override string ToString()
    {
        return $"{GetType().Name} [Id={Id}]";
    }
}