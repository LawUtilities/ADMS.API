using System.ComponentModel.DataAnnotations.Schema;
using ADMS.Domain.Common;
using ADMS.Domain.ValueObjects;
using ADMS.Domain.Events;

namespace ADMS.Domain.Entities;

/// <summary>
/// Represents a digital document stored in the ADMS legal document management system.
/// </summary>
public class Document : Entity<DocumentId>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the file name of the document.
    /// </summary>
    public FileName FileName { get; private set; } = null!;

    /// <summary>
    /// Gets or sets the file extension.
    /// </summary>
    public string Extension { get; private set; } = string.Empty;

    /// <summary>
    /// Gets or sets the size of the file in bytes.
    /// </summary>
    public long FileSize { get; private set; }

    /// <summary>
    /// Gets or sets the MIME type of the file.
    /// </summary>
    public string MimeType { get; private set; } = string.Empty;

    /// <summary>
    /// Gets or sets the checksum for integrity verification.
    /// </summary>
    public FileChecksum Checksum { get; private set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier of the matter linked to this document.
    /// </summary>
    public MatterId MatterId { get; private set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether the document is checked out.
    /// </summary>
    public bool IsCheckedOut { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document has been deleted.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Gets the creation date and time of the document in UTC.
    /// </summary>
    public DateTime CreationDate { get; private set; }

    #endregion Core Properties

    #region Navigation Properties

    public virtual Matter Matter { get; set; } = null!;
    public virtual ICollection<Revision> Revisions { get; set; } = new HashSet<Revision>();

    #endregion Navigation Properties

    #region Constructors

    // EF Core constructor
    private Document() { }

    private Document(
        DocumentId id,
        FileName fileName,
        string extension,
        long fileSize,
        string mimeType,
        FileChecksum checksum,
        MatterId matterId,
        DateTime creationDate)
    {
        Id = id;
        FileName = fileName;
        Extension = extension.ToLowerInvariant();
        FileSize = fileSize;
        MimeType = mimeType;
        Checksum = checksum;
        MatterId = matterId;
        CreationDate = creationDate;
        IsCheckedOut = false;
        IsDeleted = false;
    }

    #endregion Constructors

    #region Factory Methods

    /// <summary>
    /// Creates a new document.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="extension">The file extension.</param>
    /// <param name="fileSize">The file size in bytes.</param>
    /// <param name="mimeType">The MIME type.</param>
    /// <param name="checksum">The file checksum.</param>
    /// <param name="matterId">The matter ID.</param>
    /// <param name="createdBy">The user creating the document.</param>
    /// <returns>A Result containing the created document or validation errors.</returns>
    public static Result<Document> Create(
        string fileName,
        string extension,
        long fileSize,
        string mimeType,
        string checksum,
        MatterId matterId,
        UserId createdBy)
    {
        var documentId = DocumentId.New();
        var creationDate = DateTime.UtcNow;

        // Validate and create value objects
        var fileNameResult = FileName.Create(fileName);
        if (fileNameResult.IsFailure)
            return Result.Failure<Document>(fileNameResult.Error);

        var checksumResult = FileChecksum.Create(checksum);
        if (checksumResult.IsFailure)
            return Result.Failure<Document>(checksumResult.Error);

        // Additional validations
        if (string.IsNullOrWhiteSpace(extension))
            return Result.Failure<Document>(new DomainError("DOCUMENT_EXTENSION_REQUIRED", "Extension is required"));

        if (fileSize <= 0)
            return Result.Failure<Document>(new DomainError("DOCUMENT_INVALID_FILE_SIZE", "File size must be greater than zero"));

        if (string.IsNullOrWhiteSpace(mimeType))
            return Result.Failure<Document>(new DomainError("DOCUMENT_MIME_TYPE_REQUIRED", "MIME type is required"));

        var document = new Document(
            documentId,
            fileNameResult.Value,
            extension,
            fileSize,
            mimeType,
            checksumResult.Value,
            matterId,
            creationDate);

        // Raise domain event
        document.AddDomainEvent(new DocumentCreatedDomainEvent(documentId, fileName, createdBy));

        return Result.Success(document);
    }

    #endregion Factory Methods

    #region Business Methods

    /// <summary>
    /// Checks out the document.
    /// </summary>
    /// <param name="userId">The user checking out the document.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public Result CheckOut(UserId userId)
    {
        if (IsDeleted)
            return Result.Failure(new DomainError("DOCUMENT_DELETED", "Cannot check out a deleted document"));

        if (IsCheckedOut)
            return Result.Failure(new DomainError("DOCUMENT_ALREADY_CHECKED_OUT", "Document is already checked out"));

        IsCheckedOut = true;
        AddDomainEvent(new DocumentCheckedOutDomainEvent(Id, userId));

        return Result.Success();
    }

    /// <summary>
    /// Checks in the document.
    /// </summary>
    /// <param name="userId">The user checking in the document.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public Result CheckIn(UserId userId)
    {
        if (IsDeleted)
            return Result.Failure(new DomainError("DOCUMENT_DELETED", "Cannot check in a deleted document"));

        if (!IsCheckedOut)
            return Result.Failure(new DomainError("DOCUMENT_NOT_CHECKED_OUT", "Document is not checked out"));

        IsCheckedOut = false;
        AddDomainEvent(new DocumentCheckedInDomainEvent(Id, userId));

        return Result.Success();
    }

    /// <summary>
    /// Soft deletes the document.
    /// </summary>
    /// <param name="userId">The user deleting the document.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public Result Delete(UserId userId)
    {
        if (IsDeleted)
            return Result.Failure(new DomainError("DOCUMENT_ALREADY_DELETED", "Document is already deleted"));

        if (IsCheckedOut)
            return Result.Failure(new DomainError("DOCUMENT_CANNOT_DELETE_CHECKED_OUT", "Cannot delete a checked out document"));

        IsDeleted = true;
        AddDomainEvent(new DocumentDeletedDomainEvent(Id, userId));

        return Result.Success();
    }

    #endregion Business Methods

    #region Computed Properties

    [NotMapped]
    public string FullFileName => $"{FileName.Value}.{Extension}";

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

    [NotMapped]
    public bool HasRevisions => Revisions.Count > 0;

    [NotMapped]
    public int RevisionCount => Revisions.Count;

    [NotMapped]
    public bool CanBeDeleted => !IsDeleted && !IsCheckedOut;

    [NotMapped]
    public bool CanBeCheckedOut => !IsCheckedOut && !IsDeleted;

    [NotMapped]
    public bool CanBeCheckedIn => IsCheckedOut && !IsDeleted;

    #endregion Computed Properties

    #region String Representation

    public override string ToString() => $"Document: {FullFileName} ({FormattedFileSize}) - {(IsDeleted ? "Deleted" : IsCheckedOut ? "Checked Out" : "Active")}";

    #endregion String Representation
}