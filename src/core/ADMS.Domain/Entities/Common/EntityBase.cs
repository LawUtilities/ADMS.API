using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ADMS.Domain.Entities.Common
{
    /// <summary>
    /// Abstract base class for all domain entities in the ADMS system.
    /// Provides common functionality and properties that all entities should have.
    /// </summary>
    /// <remarks>
    /// This class implements the Entity pattern from Domain-Driven Design (DDD).
    /// All domain entities should inherit from this base class to ensure consistency
    /// and provide common functionality such as identity management and equality comparison.
    /// </remarks>
    public abstract class EntityBase : IEquatable<EntityBase>
    {
        /// <summary>
        /// Gets or sets the unique identifier for this entity.
        /// </summary>
        /// <value>
        /// A <see cref="Guid"/> that uniquely identifies this entity instance.
        /// </value>
        /// <remarks>
        /// This property serves as the primary key for the entity. It is initialized
        /// with a new GUID when the entity is created to ensure uniqueness across
        /// all instances and systems.
        /// </remarks>
        [Key]
        public virtual Guid Id { get; protected set; } = Guid.NewGuid();

        /// <summary>
        /// Gets a value indicating whether this entity is considered transient (not yet persisted).
        /// </summary>
        /// <value>
        /// <c>true</c> if the entity is transient (Id is empty); otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// A transient entity is one that has been created but not yet saved to the database.
        /// This is useful for determining whether an entity should be inserted or updated
        /// during persistence operations.
        /// </remarks>
        public virtual bool IsTransient => Id == Guid.Empty;

        /// <summary>
        /// Collection of domain events associated with this entity.
        /// </summary>
        /// <remarks>
        /// Domain events are used in DDD to capture and handle important business
        /// occurrences within the domain. Events in this collection will be processed
        /// when the entity is persisted.
        /// </remarks>
        private List<object> _domainEvents;

        /// <summary>
        /// Gets the read-only collection of domain events for this entity.
        /// </summary>
        /// <value>
        /// An <see cref="IReadOnlyCollection{T}"/> of domain events.
        /// </value>
        public virtual IReadOnlyCollection<object> DomainEvents =>
            _domainEvents?.AsReadOnly() ?? new List<object>().AsReadOnly();

        /// <summary>
        /// Adds a domain event to be processed when this entity is persisted.
        /// </summary>
        /// <param name="eventItem">The domain event to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventItem"/> is null.</exception>
        /// <remarks>
        /// Domain events allow the entity to communicate important business occurrences
        /// to other parts of the system without creating tight coupling.
        /// </remarks>
        public virtual void AddDomainEvent(object eventItem)
        {
            if (eventItem != null)
            {
                _domainEvents ??= [];
                _domainEvents.Add(eventItem);
            }
            else
            {
                throw new ArgumentNullException(nameof(eventItem));
            }
        }

        /// <summary>
        /// Removes a specific domain event from the entity.
        /// </summary>
        /// <param name="eventItem">The domain event to remove.</param>
        /// <returns><c>true</c> if the event was successfully removed; otherwise, <c>false</c>.</returns>
        public virtual bool RemoveDomainEvent(object eventItem)
        {
            return _domainEvents?.Remove(eventItem) ?? false;
        }

        /// <summary>
        /// Clears all domain events from this entity.
        /// </summary>
        /// <remarks>
        /// This method is typically called after domain events have been processed
        /// to prevent them from being processed multiple times.
        /// </remarks>
        public virtual void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }

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
        public virtual bool Equals(EntityBase other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;
            if (IsTransient || other.IsTransient) return false;

            return Id == other.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current entity.
        /// </summary>
        /// <param name="obj">The object to compare with the current entity.</param>
        /// <returns>
        /// <c>true</c> if the specified object is equal to the current entity; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as EntityBase);
        }

        /// <summary>
        /// Returns a hash code for this entity.
        /// </summary>
        /// <returns>
        /// A hash code for the current entity, suitable for use in hashing algorithms and data structures.
        /// </returns>
        /// <remarks>
        /// The hash code is based on the entity's Id to ensure consistency with the equality comparison.
        /// For transient entities, the base object hash code is used.
        /// </remarks>
        public override int GetHashCode()
        {
            return IsTransient ? base.GetHashCode() : Id.GetHashCode();
        }

        /// <summary>
        /// Determines whether two entity instances are equal.
        /// </summary>
        /// <param name="left">The first entity to compare.</param>
        /// <param name="right">The second entity to compare.</param>
        /// <returns><c>true</c> if the entities are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(EntityBase left, EntityBase right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines whether two entity instances are not equal.
        /// </summary>
        /// <param name="left">The first entity to compare.</param>
        /// <param name="right">The second entity to compare.</param>
        /// <returns><c>true</c> if the entities are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(EntityBase left, EntityBase right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Returns a string representation of the entity.
        /// </summary>
        /// <returns>A string that represents the current entity.</returns>
        public override string ToString()
        {
            return $"{GetType().Name} [Id={Id}]";
        }
    }
}