using ADMS.Domain.Common;
using ADMS.Domain.Events;
using ADMS.Domain.ValueObjects;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.Domain.Entities
{
    /// <summary>
    /// Represents a digital document stored in the ADMS legal document management system.
    /// </summary>
    /// <remarks>
    /// This entity encapsulates all information related to a legal document including
    /// its metadata, file information, checksum for integrity verification, and
    /// business rules related to document lifecycle management such as check-in/check-out
    /// and soft deletion functionality.
    /// 
    /// The Document entity follows Domain-Driven Design principles and maintains
    /// business invariants through its methods rather than allowing direct property
    /// manipulation.
    /// </remarks>
    public class Document : Entity<DocumentId>, ISoftDeletableEntity
    {
        #region Constants

        /// <summary>
        /// Maximum allowed file size in bytes (100 MB).
        /// </summary>
        public const long MaxFileSizeBytes = 100 * 1024 * 1024;

        /// <summary>
        /// Minimum allowed file size in bytes (1 byte).
        /// </summary>
        public const long MinFileSizeBytes = 1;

        #endregion Constants

        #region Core Properties

        /// <summary>
        /// Gets the file name of the document.
        /// </summary>
        /// <value>
        /// A <see cref="FileName"/> value object representing the document's file name.
        /// </value>
        /// <remarks>
        /// The file name is immutable after creation and is validated through the
        /// FileName value object to ensure it meets business rules.
        /// </remarks>
        public FileName FileName { get; private set; } = null!;

        /// <summary>
        /// Gets the file extension without the leading dot.
        /// </summary>
        /// <value>
        /// A string representing the file extension in lowercase format.
        /// </value>
        /// <remarks>
        /// The extension is normalized to lowercase during creation to ensure
        /// consistent storage and comparison operations.
        /// </remarks>
        [Required]
        [StringLength(10)]
        public string Extension { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the size of the file in bytes.
        /// </summary>
        /// <value>
        /// A long value representing the file size in bytes.
        /// </value>
        /// <remarks>
        /// The file size is validated to be within acceptable limits during
        /// document creation to prevent storage abuse.
        /// </remarks>
        [Range(MinFileSizeBytes, MaxFileSizeBytes)]
        public long FileSize { get; private set; }

        /// <summary>
        /// Gets the MIME type of the file.
        /// </summary>
        /// <value>
        /// A string representing the MIME type (e.g., "application/pdf", "text/plain").
        /// </value>
        /// <remarks>
        /// The MIME type is used for proper file handling and security validation
        /// to ensure only acceptable file types are processed by the system.
        /// </remarks>
        [Required]
        [StringLength(100)]
        public string MimeType { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the checksum for integrity verification.
        /// </summary>
        /// <value>
        /// A <see cref="FileChecksum"/> value object containing the file's hash.
        /// </value>
        /// <remarks>
        /// The checksum is used to verify file integrity and detect corruption
        /// or tampering. It's calculated during document creation.
        /// </remarks>
        public FileChecksum Checksum { get; private set; } = null!;

        /// <summary>
        /// Gets the unique identifier of the matter associated with this document.
        /// </summary>
        /// <value>
        /// A <see cref="MatterId"/> representing the associated legal matter.
        /// </value>
        /// <remarks>
        /// Every document must be associated with a matter for proper organization
        /// and access control within the legal document management system.
        /// </remarks>
        public MatterId MatterId { get; private set; } = null!;

        /// <summary>
        /// Gets a value indicating whether the document is currently checked out.
        /// </summary>
        /// <value>
        /// <c>true</c> if the document is checked out; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// When a document is checked out, it cannot be modified by other users
        /// or deleted until it is checked back in, ensuring document integrity
        /// during editing operations.
        /// </remarks>
        public bool IsCheckedOut { get; private set; }

        /// <summary>
        /// Gets the identifier of the user who has checked out the document.
        /// </summary>
        /// <value>
        /// A <see cref="UserId"/> if the document is checked out; otherwise, null.
        /// </value>
        /// <remarks>
        /// This property is populated when the document is checked out and cleared
        /// when the document is checked back in.
        /// </remarks>
        public UserId? CheckedOutBy { get; private set; }

        /// <summary>
        /// Gets the date and time when the document was checked out.
        /// </summary>
        /// <value>
        /// A nullable <see cref="DateTime"/> representing when the document was checked out.
        /// </value>
        /// <remarks>
        /// This timestamp helps track how long a document has been checked out
        /// and can be used for administrative purposes.
        /// </remarks>
        public DateTime? CheckedOutDate { get; private set; }

        #endregion Core Properties

        #region Soft Delete Properties (ISoftDeletableEntity)

        /// <inheritdoc />
        public bool IsDeleted { get; private set; }

        /// <inheritdoc />
        public string DeletedBy { get; private set; } = string.Empty;

        /// <inheritdoc />
        public DateTime? DeletedDate { get; private set; }

        #endregion Soft Delete Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the matter associated with this document.
        /// </summary>
        /// <value>
        /// The <see cref="Matter"/> entity that this document belongs to.
        /// </value>
        /// <remarks>
        /// This navigation property is used by Entity Framework for relationship
        /// mapping and should not be used for business logic operations.
        /// </remarks>
        public virtual Matter Matter { get; set; } = null!;

        /// <summary>
        /// Gets or sets the collection of revisions for this document.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Revision"/> entities representing document versions.
        /// </value>
        /// <remarks>
        /// This collection tracks all revisions of the document, maintaining
        /// a complete audit trail of changes over time.
        /// </remarks>
        public virtual ICollection<Revision> Revisions { get; set; } = new HashSet<Revision>();

        #endregion Navigation Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Document"/> class.
        /// </summary>
        /// <remarks>
        /// This parameterless constructor is required by Entity Framework Core
        /// and should not be used directly in application code. Use the factory
        /// methods instead.
        /// </remarks>
        private Document() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Document"/> class with specified parameters.
        /// </summary>
        /// <param name="id">The unique identifier for the document.</param>
        /// <param name="fileName">The file name value object.</param>
        /// <param name="extension">The file extension.</param>
        /// <param name="fileSize">The file size in bytes.</param>
        /// <param name="mimeType">The MIME type.</param>
        /// <param name="checksum">The file checksum.</param>
        /// <param name="matterId">The associated matter identifier.</param>
        /// <param name="createdBy">The user who created the document.</param>
        /// <remarks>
        /// This constructor is used internally by factory methods to create
        /// valid document instances with all required properties set.
        /// </remarks>
        private Document(
            DocumentId id,
            FileName fileName,
            string extension,
            long fileSize,
            string mimeType,
            FileChecksum checksum,
            MatterId matterId,
            string createdBy)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            Extension = extension?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(extension));
            FileSize = fileSize;
            MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
            Checksum = checksum ?? throw new ArgumentNullException(nameof(checksum));
            MatterId = matterId ?? throw new ArgumentNullException(nameof(matterId));

            // Initialize audit properties
            CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
            CreatedDate = DateTime.UtcNow;
            LastModifiedBy = createdBy;
            LastModifiedDate = null;

            // Initialize state properties
            IsCheckedOut = false;
            CheckedOutBy = null;
            CheckedOutDate = null;
            IsDeleted = false;
            DeletedBy = string.Empty;
            DeletedDate = null;
        }

        #endregion Constructors

        #region Factory Methods

        /// <summary>
        /// Creates a new document with the specified parameters.
        /// </summary>
        /// <param name="fileName">The file name for the document.</param>
        /// <param name="extension">The file extension (without leading dot).</param>
        /// <param name="fileSize">The file size in bytes.</param>
        /// <param name="mimeType">The MIME type of the file.</param>
        /// <param name="checksum">The file checksum for integrity verification.</param>
        /// <param name="matterId">The identifier of the associated matter.</param>
        /// <param name="createdBy">The identifier of the user creating the document.</param>
        /// <returns>
        /// A <see cref="Result{Document}"/> containing either the created document or validation errors.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any required parameter is null or empty.
        /// </exception>
        /// <remarks>
        /// This factory method validates all input parameters and creates value objects
        /// before constructing the document instance. It also raises the appropriate
        /// domain event for document creation.
        /// </remarks>
        public static Result<Document> Create(
            string fileName,
            string extension,
            long fileSize,
            string mimeType,
            string checksum,
            MatterId matterId,
            string createdBy)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(fileName))
                return Result.Failure<Document>(DomainError.Create(
                    "DOCUMENT_FILENAME_REQUIRED",
                    "File name is required and cannot be empty"));

            if (string.IsNullOrWhiteSpace(extension))
                return Result.Failure<Document>(DomainError.Create(
                    "DOCUMENT_EXTENSION_REQUIRED",
                    "File extension is required and cannot be empty"));

            if (string.IsNullOrWhiteSpace(mimeType))
                return Result.Failure<Document>(DomainError.Create(
                    "DOCUMENT_MIMETYPE_REQUIRED",
                    "MIME type is required and cannot be empty"));

            if (string.IsNullOrWhiteSpace(checksum))
                return Result.Failure<Document>(DomainError.Create(
                    "DOCUMENT_CHECKSUM_REQUIRED",
                    "File checksum is required and cannot be empty"));

            if (matterId == null)
                return Result.Failure<Document>(DomainError.Create(
                    "DOCUMENT_MATTER_ID_REQUIRED",
                    "Matter ID is required"));

            if (string.IsNullOrWhiteSpace(createdBy))
                return Result.Failure<Document>(DomainError.Create(
                    "DOCUMENT_CREATED_BY_REQUIRED",
                    "Created by is required and cannot be empty"));

            // File size validation
            if (fileSize < MinFileSizeBytes)
                return Result.Failure<Document>(DomainError.Create(
                    "DOCUMENT_INVALID_FILE_SIZE_TOO_SMALL",
                    $"File size must be at least {MinFileSizeBytes} byte(s)"));

            if (fileSize > MaxFileSizeBytes)
                return Result.Failure<Document>(DomainError.Create(
                    "DOCUMENT_INVALID_FILE_SIZE_TOO_LARGE",
                    $"File size cannot exceed {MaxFileSizeBytes} bytes ({MaxFileSizeBytes / (1024 * 1024)} MB)"));

            // Create value objects
            var fileNameResult = FileName.Create(fileName);
            if (fileNameResult.IsFailure)
                return Result.Failure<Document>(fileNameResult.Error);

            var checksumResult = FileChecksum.Create(checksum);
            if (checksumResult.IsFailure)
                return Result.Failure<Document>(checksumResult.Error);

            // Create document instance
            var documentId = DocumentId.New();
            var document = new Document(
                documentId,
                fileNameResult.Value,
                extension,
                fileSize,
                mimeType,
                checksumResult.Value,
                matterId,
                createdBy);

            // Raise domain event
            document.AddDomainEvent(new DocumentCreatedDomainEvent(documentId, fileName, createdBy));

            return Result.Success(document);
        }

        #endregion Factory Methods

        #region Business Methods

        /// <summary>
        /// Checks out the document to the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user checking out the document.</param>
        /// <param name="modifiedBy">The identifier of the user performing the operation.</param>
        /// <returns>A <see cref="Result"/> indicating success or failure with error details.</returns>
        /// <remarks>
        /// When a document is checked out, it becomes locked for editing by other users.
        /// The check-out operation updates audit information and raises a domain event.
        /// </remarks>
        public Result CheckOut(UserId userId, string modifiedBy)
        {
            if (userId == null)
                return Result.Failure(DomainError.Create(
                    "DOCUMENT_CHECKOUT_USER_ID_REQUIRED",
                    "User ID is required for check-out operation"));

            if (string.IsNullOrWhiteSpace(modifiedBy))
                return Result.Failure(DomainError.Create(
                    "DOCUMENT_CHECKOUT_MODIFIED_BY_REQUIRED",
                    "Modified by is required for check-out operation"));

            if (IsDeleted)
                return Result.Failure(DomainError.Create(
                    "DOCUMENT_CHECKOUT_DELETED",
                    "Cannot check out a deleted document"));

            if (IsCheckedOut)
                return Result.Failure(DomainError.Create(
                    "DOCUMENT_ALREADY_CHECKED_OUT",
                    $"Document is already checked out by user {CheckedOutBy}"));

            IsCheckedOut = true;
            CheckedOutBy = userId;
            CheckedOutDate = DateTime.UtcNow;

            // Update audit information
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;

            AddDomainEvent(new DocumentCheckedOutDomainEvent(Id, userId, modifiedBy));

            return Result.Success();
        }

        /// <summary>
        /// Checks in the document from the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user checking in the document.</param>
        /// <param name="modifiedBy">The identifier of the user performing the operation.</param>
        /// <returns>A <see cref="Result"/> indicating success or failure with error details.</returns>
        /// <remarks>
        /// When a document is checked in, it becomes available for other users to check out.
        /// The check-in operation clears check-out information and raises a domain event.
        /// </remarks>
        public Result CheckIn(UserId userId, string modifiedBy)
        {
            if (userId == null)
                return Result.Failure(DomainError.Create(
                    "DOCUMENT_CHECKIN_USER_ID_REQUIRED",
                    "User ID is required for check-in operation"));

            if (string.IsNullOrWhiteSpace(modifiedBy))
                return Result.Failure(DomainError.Create(
                    "DOCUMENT_CHECKIN_MODIFIED_BY_REQUIRED",
                    "Modified by is required for check-in operation"));

            if (IsDeleted)
                return Result.Failure(DomainError.Create(
                    "DOCUMENT_CHECKIN_DELETED",
                    "Cannot check in a deleted document"));

            if (!IsCheckedOut)
                return Result.Failure(DomainError.Create(
                    "DOCUMENT_NOT_CHECKED_OUT",
                    "Document is not currently checked out"));

            if (!CheckedOutBy?.Equals(userId) == true)
                return Result.Failure(DomainError.Create(
                    "DOCUMENT_CHECKIN_UNAUTHORIZED",
                    "Only the user who checked out the document can check it back in"));

            IsCheckedOut = false;
            CheckedOutBy = null;
            CheckedOutDate = null;

            // Update audit information
            LastModifiedBy = modifiedBy;
            LastModifiedDate = DateTime.UtcNow;

            AddDomainEvent(new DocumentCheckedInDomainEvent(Id, userId, modifiedBy));

            return Result.Success();
        }

        /// <summary>
        /// Performs a soft delete of the document.
        /// </summary>
        /// <param name="deletedBy">The identifier of the user performing the delete operation.</param>
        /// <returns>A <see cref="Result"/> indicating success or failure with error details.</returns>
        /// <remarks>
        /// Soft deletion marks the document as deleted without removing it from the database.
        /// This preserves audit trails and allows for potential recovery operations.
        /// </remarks>
        public Result Delete(string deletedBy)
        {
            if (string.IsNullOrWhiteSpace(deletedBy))
                return Result.Failure(DomainError.Create(
                    "DOCUMENT_DELETE_DELETED_BY_REQUIRED",
                    "Deleted by is required for delete operation"));

            if (IsDeleted)
                return Result.Failure(DomainError.Create(
                    "DOCUMENT_ALREADY_DELETED",
                    "Document is already deleted"));

            if (IsCheckedOut)
                return Result.Failure(DomainError.Create(
                    "DOCUMENT_DELETE_CHECKED_OUT",
                    "Cannot delete a document that is currently checked out"));

            IsDeleted = true;
            DeletedBy = deletedBy;
            DeletedDate = DateTime.UtcNow;

            // Update audit information
            LastModifiedBy = deletedBy;
            LastModifiedDate = DateTime.UtcNow;

            AddDomainEvent(new DocumentDeletedDomainEvent(Id, deletedBy));

            return Result.Success();
        }

        /// <summary>
        /// Restores a soft-deleted document.
        /// </summary>
        /// <param name="restoredBy">The identifier of the user performing the restore operation.</param>
        /// <returns>A <see cref="Result"/> indicating success or failure with error details.</returns>
        /// <remarks>
        /// This operation can only be performed on soft-deleted documents and makes
        /// them available again for normal operations.
        /// </remarks>
        public Result Restore(string restoredBy)
        {
            if (string.IsNullOrWhiteSpace(restoredBy))
                return Result.Failure(DomainError.Create(
                    "DOCUMENT_RESTORE_RESTORED_BY_REQUIRED",
                    "Restored by is required for restore operation"));

            if (!IsDeleted)
                return Result.Failure(DomainError.Create(
                    "DOCUMENT_NOT_DELETED",
                    "Cannot restore a document that is not deleted"));

            IsDeleted = false;
            DeletedBy = string.Empty;
            DeletedDate = null;

            // Update audit information
            LastModifiedBy = restoredBy;
            LastModifiedDate = DateTime.UtcNow;

            AddDomainEvent(new DocumentRestoredDomainEvent(Id, restoredBy));

            return Result.Success();
        }

        #endregion Business Methods

        #region Computed Properties

        /// <summary>
        /// Gets the full file name including the extension.
        /// </summary>
        /// <value>
        /// A string combining the file name and extension (e.g., "document.pdf").
        /// </value>
        [NotMapped]
        public string FullFileName => $"{FileName.Value}.{Extension}";

        /// <summary>
        /// Gets a human-readable representation of the file size.
        /// </summary>
        /// <value>
        /// A formatted string representing the file size with appropriate units.
        /// </value>
        /// <remarks>
        /// The format automatically scales from bytes to KB, MB, or GB as appropriate
        /// for better readability in user interfaces.
        /// </remarks>
        [NotMapped]
        public string FormattedFileSize
        {
            get
            {
                const long kb = 1024;
                const long mb = kb * 1024;
                const long gb = mb * 1024;

                return FileSize switch
                {
                    < kb => $"{FileSize} bytes",
                    < mb => $"{FileSize / (double)kb:F1} KB",
                    < gb => $"{FileSize / (double)mb:F1} MB",
                    _ => $"{FileSize / (double)gb:F1} GB"
                };
            }
        }

        /// <summary>
        /// Gets a value indicating whether the document has any revisions.
        /// </summary>
        /// <value>
        /// <c>true</c> if the document has one or more revisions; otherwise, <c>false</c>.
        /// </value>
        [NotMapped]
        public bool HasRevisions => Revisions.Count > 0;

        /// <summary>
        /// Gets the total number of revisions for this document.
        /// </summary>
        /// <value>
        /// An integer representing the number of revisions.
        /// </value>
        [NotMapped]
        public int RevisionCount => Revisions.Count;

        /// <summary>
        /// Gets a value indicating whether the document can be deleted.
        /// </summary>
        /// <value>
        /// <c>true</c> if the document can be deleted; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// A document can only be deleted if it is not already deleted and not checked out.
        /// </remarks>
        [NotMapped]
        public bool CanBeDeleted => !IsDeleted && !IsCheckedOut;

        /// <summary>
        /// Gets a value indicating whether the document can be checked out.
        /// </summary>
        /// <value>
        /// <c>true</c> if the document can be checked out; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// A document can only be checked out if it is not already checked out and not deleted.
        /// </remarks>
        [NotMapped]
        public bool CanBeCheckedOut => !IsCheckedOut && !IsDeleted;

        /// <summary>
        /// Gets a value indicating whether the document can be checked in.
        /// </summary>
        /// <value>
        /// <c>true</c> if the document can be checked in; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// A document can only be checked in if it is currently checked out and not deleted.
        /// </remarks>
        [NotMapped]
        public bool CanBeCheckedIn => IsCheckedOut && !IsDeleted;

        /// <summary>
        /// Gets a value indicating whether the document can be restored.
        /// </summary>
        /// <value>
        /// <c>true</c> if the document can be restored; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// A document can only be restored if it is currently soft-deleted.
        /// </remarks>
        [NotMapped]
        public bool CanBeRestored => IsDeleted;

        /// <summary>
        /// Gets the current status of the document.
        /// </summary>
        /// <value>
        /// A string describing the current document status.
        /// </value>
        [NotMapped]
        public string Status => IsDeleted ? "Deleted" : IsCheckedOut ? "Checked Out" : "Active";

        #endregion Computed Properties

        #region String Representation

        /// <summary>
        /// Returns a string that represents the current document.
        /// </summary>
        /// <returns>
        /// A string containing the document's full file name, formatted file size, and status.
        /// </returns>
        public override string ToString() =>
            $"Document: {FullFileName} ({FormattedFileSize}) - {Status}";

        #endregion String Representation
    }
}