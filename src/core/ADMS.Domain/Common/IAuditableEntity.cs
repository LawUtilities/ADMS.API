using System;

namespace ADMS.Domain.Common
{
    /// <summary>
    /// Defines the contract for entities that require audit trail functionality.
    /// </summary>
    /// <remarks>
    /// This interface provides a standardized way to track who created or modified
    /// an entity and when those operations occurred. Implementing this interface
    /// enables automatic population of audit fields through entity framework
    /// interceptors or repository patterns.
    /// 
    /// This follows the Interface Segregation Principle by separating audit
    /// concerns from the base entity functionality, allowing entities to opt-in
    /// to audit tracking as needed.
    /// </remarks>
    /// <summary>
    /// Interface for entities that support audit trail functionality.
    /// </summary>
    /// <remarks>
    /// This interface defines the contract for entities that require tracking of 
    /// creation and modification metadata for audit and compliance purposes.
    /// </remarks>
    public interface IAuditableEntity
    {
        /// <summary>
        /// Gets or sets the identifier of the user who created this entity.
        /// </summary>
        /// <value>The user identifier who created the entity.</value>
        string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time when this entity was created.
        /// </summary>
        /// <value>The UTC timestamp of entity creation.</value>
        DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who last modified this entity.
        /// </summary>
        /// <value>The user identifier who last modified the entity.</value>
        string LastModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time when this entity was last modified.
        /// </summary>
        /// <value>The UTC timestamp of the last modification, or null if never modified.</value>
        DateTime? LastModifiedDate { get; set; }
    }

    /// <summary>
    /// Extended audit interface that includes soft delete functionality.
    /// </summary>
    /// <remarks>
    /// This interface extends the basic audit functionality to include soft delete
    /// capabilities, allowing entities to be marked as deleted without actually
    /// removing them from the database. This is useful for maintaining data
    /// integrity and audit trails while providing delete functionality.
    /// </remarks>
    public interface ISoftDeletableEntity : IAuditableEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether this entity is deleted.
        /// </summary>
        /// <value>
        /// <c>true</c> if the entity is soft deleted; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// When this property is true, the entity should be treated as deleted
        /// by the application logic, but the record remains in the database
        /// for audit purposes. Query filters should be applied to exclude
        /// soft-deleted entities from normal operations.
        /// </remarks>
        bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who deleted this entity.
        /// </summary>
        /// <value>
        /// A string representing the user identifier who performed the delete operation,
        /// or null if the entity is not deleted.
        /// </value>
        /// <remarks>
        /// This property should be populated when the entity is soft deleted.
        /// It should be null when IsDeleted is false.
        /// </remarks>
        string DeletedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this entity was deleted.
        /// </summary>
        /// <value>
        /// A nullable <see cref="DateTime"/> representing when the entity was deleted,
        /// or null if the entity is not deleted.
        /// </value>
        /// <remarks>
        /// This property should be automatically populated when the entity is soft deleted.
        /// It should be null when IsDeleted is false.
        /// </remarks>
        DateTime? DeletedDate { get; set; }
    }
}