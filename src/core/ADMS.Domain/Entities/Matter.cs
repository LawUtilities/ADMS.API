using ADMS.Domain.Common;
using ADMS.Domain.Events;
using ADMS.Domain.ValueObjects;

using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.Domain.Entities
{
    /// <summary>
    /// Represents a matter for digital document collection and management in the ADMS legal document management system.
    /// </summary>
    /// <remarks>
    /// The Matter entity serves as the primary organizational container for digital documents within the ADMS legal
    /// document management system. It functions as a digital filing cabinet that can represent either client-based
    /// or matter-specific document collections, making it particularly well-suited for small law firms and legal
    /// practitioners who need flexible document organization strategies.
    /// 
    /// <para><strong>Key Domain Characteristics:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Aggregate Root:</strong> Central container for related digital documents with business invariants</item>
    /// <item><strong>Document Collection:</strong> Maintains document associations with proper lifecycle management</item>
    /// <item><strong>Audit Trail Foundation:</strong> Comprehensive tracking of all matter-related activities</item>
    /// <item><strong>Business Rule Enforcement:</strong> Validates and enforces legal practice requirements</item>
    /// <item><strong>Domain Events:</strong> Publishes events for matter lifecycle changes</item>
    /// </list>
    /// </remarks>
    public class Matter : Entity<MatterId>, ISoftDeletableEntity
    {
        #region Constants

        /// <summary>
        /// Maximum allowed description length.
        /// </summary>
        public const int MaxDescriptionLength = 128;

        /// <summary>
        /// Minimum allowed description length.
        /// </summary>
        public const int MinDescriptionLength = 2;

        #endregion Constants

        #region Core Properties

        /// <summary>
        /// Gets the matter description.
        /// </summary>
        /// <value>
        /// A <see cref="MatterDescription"/> value object representing the matter's description.
        /// </value>
        public MatterDescription Description { get; private set; } = null!;

        /// <summary>
        /// Gets a value indicating whether the matter is archived.
        /// </summary>
        public bool IsArchived { get; private set; }

        /// <summary>
        /// Gets the creation date of the matter (in UTC).
        /// </summary>
        public DateTime CreationDate { get; private set; }

        #endregion Core Properties

        #region Soft Delete Properties (ISoftDeletableEntity)

        /// <summary>
        /// Gets a value indicating whether the entity has been marked as deleted.
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Gets the identifier of the user or system that performed the deletion.
        /// </summary>
        public string? DeletedBy { get; private set; }

        /// <inheritdoc />
        public DateTime? DeletedDate { get; private set; }

        #endregion Soft Delete Properties

        #region Audit Properties (IAuditableEntity)

        /// <inheritdoc />
        public string CreatedBy { get; private set; } = string.Empty;

        /// <inheritdoc />
        public DateTime CreatedDate { get; private set; }

        /// <inheritdoc />
        public string LastModifiedBy { get; private set; } = string.Empty;

        /// <inheritdoc />
        public DateTime? LastModifiedDate { get; private set; }

        #endregion Audit Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the digital documents associated with this matter.
        /// </summary>
        public virtual ICollection<Document> Documents { get; set; } = new HashSet<Document>();

        /// <summary>
        /// Gets or sets the matter activity users associated with this matter.
        /// </summary>
        public virtual ICollection<MatterActivityUser> MatterActivityUsers { get; set; } =
            new HashSet<MatterActivityUser>();

        /// <summary>
        /// Gets or sets the "from" matter document activity users associated with this matter.
        /// </summary>
        public virtual ICollection<MatterDocumentActivityUserFrom> MatterDocumentActivityUsersFrom { get; set; } =
            new HashSet<MatterDocumentActivityUserFrom>();

        /// <summary>
        /// Gets or sets the "to" matter document activity users associated with this matter.
        /// </summary>
        public virtual ICollection<MatterDocumentActivityUserTo> MatterDocumentActivityUsersTo { get; set; } =
            new HashSet<MatterDocumentActivityUserTo>();

        #endregion Navigation Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Matter"/> class.
        /// </summary>
        /// <remarks>
        /// This parameterless constructor is required by Entity Framework Core
        /// and should not be used directly in application code. Use the factory
        /// methods instead.
        /// </remarks>
        private Matter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matter"/> class with specified parameters.
        /// </summary>
        /// <param name="id">The unique identifier for the matter.</param>
        /// <param name="description">The matter description value object.</param>
        /// <param name="createdBy">The user who created the matter.</param>
        private Matter(MatterId id, MatterDescription description, string createdBy)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));

            // Initialize temporal properties
            var utcNow = DateTime.UtcNow;
            CreationDate = utcNow;
            CreatedDate = utcNow;
            LastModifiedBy = createdBy;
            LastModifiedDate = null;

            // Initialize state properties
            IsArchived = false;
            IsDeleted = false;
            DeletedBy = null;
            DeletedDate = null;
        }

        #endregion Constructors

        #region Factory Methods

        /// <summary>
        /// Creates a new matter with the specified parameters.
        /// </summary>
        /// <param name="description">The matter description.</param>
        /// <param name="createdBy">The identifier of the user creating the matter.</param>
        /// <returns>
        /// A <see cref="Result{Matter}"/> containing either the created matter or validation errors.
        /// </returns>
        public static Result<Matter> Create(string description, string createdBy)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(description))
                return Result.Failure<Matter>(DomainError.Create(
                    "MATTER_DESCRIPTION_REQUIRED",
                    "Matter description is required and cannot be empty"));

            if (string.IsNullOrWhiteSpace(createdBy))
                return Result.Failure<Matter>(DomainError.Create(
                    "MATTER_CREATED_BY_REQUIRED",
                    "Created by is required and cannot be empty"));

            // Create value objects
            var descriptionResult = MatterDescription.Create(description);
            if (descriptionResult.IsFailure)
                return Result.Failure<Matter>(descriptionResult.Error);

            // Create matter instance
            var matterId = MatterId.New();
            var matter = new Matter(matterId, descriptionResult.Value, createdBy);

            // Raise domain event
            matter.AddDomainEvent(new MatterCreatedDomainEvent(matterId, description, createdBy));

            return Result.Success(matter);
        }

        #endregion Factory Methods

        #region Business Methods

        /// <summary>
        /// Updates the matter description.
        /// </summary>
        /// <param name="newDescription">The new description.</param>
        /// <param name="modifiedBy">The user making the modification.</param>
        /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
        public Result UpdateDescription(string newDescription, string modifiedBy)
        {
            if (string.IsNullOrWhiteSpace(newDescription))
                return Result.Failure(DomainError.Create(
                    "MATTER_DESCRIPTION_REQUIRED",
                    "Matter description is required and cannot be empty"));

            if (string.IsNullOrWhiteSpace(modifiedBy))
                return Result.Failure(DomainError.Create(
                    "MATTER_MODIFIED_BY_REQUIRED",
                    "Modified by is required"));

            if (IsDeleted)
                return Result.Failure(DomainError.Create(
                    "MATTER_UPDATE_DELETED",
                    "Cannot update a deleted matter"));

            var descriptionResult = MatterDescription.Create(newDescription);
            if (descriptionResult.IsFailure)
                return Result.Failure(descriptionResult.Error);

            var oldDescription = Description.Value;
            Description = descriptionResult.Value;

            UpdateAuditInfo(modifiedBy);

            AddDomainEvent(new MatterDescriptionUpdatedDomainEvent(Id, oldDescription, newDescription, modifiedBy));

            return Result.Success();
        }

        /// <summary>
        /// Archives the matter.
        /// </summary>
        /// <param name="archivedBy">The user archiving the matter.</param>
        /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
        public Result Archive(string archivedBy)
        {
            if (string.IsNullOrWhiteSpace(archivedBy))
                return Result.Failure(DomainError.Create(
                    "MATTER_ARCHIVED_BY_REQUIRED",
                    "Archived by is required"));

            if (IsDeleted)
                return Result.Failure(DomainError.Create(
                    "MATTER_ARCHIVE_DELETED",
                    "Cannot archive a deleted matter"));

            if (IsArchived)
                return Result.Failure(DomainError.Create(
                    "MATTER_ALREADY_ARCHIVED",
                    "Matter is already archived"));

            IsArchived = true;
            UpdateAuditInfo(archivedBy);

            AddDomainEvent(new MatterArchivedDomainEvent(Id, archivedBy));

            return Result.Success();
        }

        /// <summary>
        /// Unarchives the matter.
        /// </summary>
        /// <param name="unarchivedBy">The user unarchiving the matter.</param>
        /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
        public Result Unarchive(string unarchivedBy)
        {
            if (string.IsNullOrWhiteSpace(unarchivedBy))
                return Result.Failure(DomainError.Create(
                    "MATTER_UNARCHIVED_BY_REQUIRED",
                    "Unarchived by is required"));

            if (IsDeleted)
                return Result.Failure(DomainError.Create(
                    "MATTER_UNARCHIVE_DELETED",
                    "Cannot unarchive a deleted matter"));

            if (!IsArchived)
                return Result.Failure(DomainError.Create(
                    "MATTER_NOT_ARCHIVED",
                    "Matter is not currently archived"));

            IsArchived = false;
            UpdateAuditInfo(unarchivedBy);

            AddDomainEvent(new MatterUnarchivedDomainEvent(Id, unarchivedBy));

            return Result.Success();
        }

        /// <summary>
        /// Performs a soft delete of the matter.
        /// </summary>
        /// <param name="deletedBy">The user performing the delete operation.</param>
        /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
        public Result Delete(string deletedBy)
        {
            if (string.IsNullOrWhiteSpace(deletedBy))
                return Result.Failure(DomainError.Create(
                    "MATTER_DELETED_BY_REQUIRED",
                    "Deleted by is required"));

            if (IsDeleted)
                return Result.Failure(DomainError.Create(
                    "MATTER_ALREADY_DELETED",
                    "Matter is already deleted"));

            if (HasActiveDocuments())
                return Result.Failure(DomainError.Create(
                    "MATTER_DELETE_HAS_ACTIVE_DOCUMENTS",
                    "Cannot delete a matter that has active documents"));

            IsDeleted = true;
            DeletedBy = deletedBy;
            DeletedDate = DateTime.UtcNow;
            UpdateAuditInfo(deletedBy);

            AddDomainEvent(new MatterDeletedDomainEvent(Id, deletedBy));

            return Result.Success();
        }

        /// <summary>
        /// Restores a soft-deleted matter.
        /// </summary>
        /// <param name="restoredBy">The user performing the restore operation.</param>
        /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
        public Result Restore(string restoredBy)
        {
            if (string.IsNullOrWhiteSpace(restoredBy))
                return Result.Failure(DomainError.Create(
                    "MATTER_RESTORED_BY_REQUIRED",
                    "Restored by is required"));

            if (!IsDeleted)
                return Result.Failure(DomainError.Create(
                    "MATTER_NOT_DELETED",
                    "Cannot restore a matter that is not deleted"));

            IsDeleted = false;
            DeletedBy = null;
            DeletedDate = null;
            UpdateAuditInfo(restoredBy);

            AddDomainEvent(new MatterRestoredDomainEvent(Id, restoredBy));

            return Result.Success();
        }

        #endregion Business Methods

        #region Query Methods

        /// <summary>
        /// Determines whether this matter is currently active (not archived and not deleted).
        /// </summary>
        /// <returns>true if the matter is active; otherwise, false.</returns>
        public bool IsActive() => !IsArchived && !IsDeleted;

        /// <summary>
        /// Determines whether this matter has any active documents.
        /// </summary>
        /// <returns>true if the matter has active documents; otherwise, false.</returns>
        public bool HasActiveDocuments() => Documents.Any(d => !d.IsDeleted);

        /// <summary>
        /// Gets the total number of documents in this matter.
        /// </summary>
        /// <returns>The total document count.</returns>
        public int GetDocumentCount() => Documents.Count;

        /// <summary>
        /// Gets the number of active (non-deleted) documents in this matter.
        /// </summary>
        /// <returns>The active document count.</returns>
        public int GetActiveDocumentCount() => Documents.Count(d => !d.IsDeleted);

        /// <summary>
        /// Gets the age of this matter in days.
        /// </summary>
        /// <returns>The age in days.</returns>
        public double GetAgeDays() => (DateTime.UtcNow - CreationDate).TotalDays;

        #endregion Query Methods

        #region Computed Properties

        /// <summary>
        /// Gets the current status of the matter as a descriptive string.
        /// </summary>
        [NotMapped]
        public string Status
        {
            get
            {
                if (IsDeleted && IsArchived) return "Archived and Deleted";
                if (IsDeleted) return "Deleted";
                if (IsArchived) return "Archived";
                return "Active";
            }
        }

        #endregion Computed Properties

        #region Helper Methods

        /// <summary>
        /// Updates audit information for the matter.
        /// </summary>
        /// <param name="modifiedBy">The user making the modification.</param>
        private void UpdateAuditInfo(string modifiedBy)
        {
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;
        }

        #endregion Helper Methods

        #region String Representation

        /// <summary>
        /// Returns a string representation of the Matter.
        /// </summary>
        /// <returns>A string that represents the current Matter.</returns>
        public override string ToString() => $"Matter: {Description.Value} ({Id}) - {Status}";

        #endregion String Representation
    }
}