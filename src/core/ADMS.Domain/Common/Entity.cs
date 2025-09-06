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
        _domainEvents is not null ? _domainEvents.AsReadOnly() : Array.Empty<IDomainEvent>();

    /// <summary>
    /// Adds a domain event to be processed when this entity is persisted.
    /// </summary>
    /// <param name="eventItem">The domain event to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventItem"/> is null.</exception>
    public virtual void AddDomainEvent(IDomainEvent eventItem)
    {
        ArgumentNullException.ThrowIfNull(eventItem);

        _domainEvents ??= [];
        _domainEvents.Add(eventItem);
    }

    /// <summary>
    /// Adds multiple domain events to be processed when this entity is persisted.
    /// </summary>
    /// <param name="events">The domain events to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="events"/> is null.</exception>
    public virtual void AddDomainEvents(IEnumerable<IDomainEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        foreach (var domainEvent in events)
        {
            AddDomainEvent(domainEvent);
        }
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

    /// <summary>
    /// Gets the count of pending domain events.
    /// </summary>
    /// <returns>The number of pending domain events.</returns>
    public virtual int DomainEventCount => _domainEvents?.Count ?? 0;

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

        // Fast path: check if types are different
        if (GetType() != other.GetType()) return false;

        // If either entity is transient, they can only be equal if they're the same reference
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
        // For transient entities, use type-based hash code to avoid reference-based hash codes
        if (IsTransient)
        {
            return GetType().GetHashCode();
        }

        // For non-transient entities, use only immutable fields (Id and type)
        return HashCode.Combine(GetType(), EqualityComparer<TId>.Default.GetHashCode(Id!));
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

    #region Audit Helper Methods

    /// <summary>
    /// Sets the audit information for entity creation.
    /// </summary>
    /// <param name="createdBy">The user who created the entity.</param>
    /// <param name="createdDate">The creation date. If null, uses current UTC time.</param>
    protected virtual void SetCreatedAudit(string createdBy, DateTime? createdDate = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);

        CreatedBy = createdBy;
        CreatedDate = createdDate ?? DateTime.UtcNow;
        LastModifiedBy = createdBy;
        LastModifiedDate = null; // No modification at creation
    }

    /// <summary>
    /// Sets the audit information for entity modification.
    /// </summary>
    /// <param name="modifiedBy">The user who modified the entity.</param>
    /// <param name="modifiedDate">The modification date. If null, uses current UTC time.</param>
    protected virtual void SetModifiedAudit(string modifiedBy, DateTime? modifiedDate = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modifiedBy);

        LastModifiedBy = modifiedBy;
        LastModifiedDate = modifiedDate ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the age of this entity in days since creation.
    /// </summary>
    /// <returns>The number of days since the entity was created.</returns>
    public virtual double GetAgeDays() => (DateTime.UtcNow - CreatedDate).TotalDays;

    /// <summary>
    /// Gets the time since last modification in days.
    /// </summary>
    /// <returns>The number of days since last modification, or null if never modified.</returns>
    public virtual double? GetDaysSinceLastModification()
    {
        return LastModifiedDate.HasValue
            ? (DateTime.UtcNow - LastModifiedDate.Value).TotalDays
            : null;
    }

    #endregion Audit Helper Methods

    /// <summary>
    /// Returns a string representation of the entity.
    /// </summary>
    /// <returns>A string that represents the current entity.</returns>
    public override string ToString()
    {
        var typeName = GetType().Name;
        var id = IsTransient ? "Transient" : Id?.ToString() ?? "Unknown";
        return $"{typeName} [Id={id}]";
    }
}

/// <summary>
/// Extension methods for nullable values.
/// </summary>
internal static class NullableExtensions
{
    /// <summary>
    /// Transforms a nullable value if it has a value.
    /// </summary>
    /// <typeparam name="T">The type of the nullable value.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The transformed value if the nullable has a value; otherwise, null.</returns>
    public static TResult? Let<T, TResult>(this T? value, Func<T, TResult> transform)
        where T : struct
        where TResult : struct
    {
        return value.HasValue ? transform(value.Value) : null;
    }
}