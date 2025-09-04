using ADMS.Domain.Common;

namespace ADMS.Domain.Errors;

public static class DocumentErrors
{
    public static readonly DomainError FileNameRequired =
        new("DOCUMENT_FILENAME_REQUIRED", "Document filename cannot be empty");

    public static readonly DomainError FileNameTooLong =
        new("DOCUMENT_FILENAME_TOO_LONG", "Document filename cannot exceed 128 characters");

    public static readonly DomainError FileNameInvalidCharacters =
        new("DOCUMENT_FILENAME_INVALID_CHARACTERS", "Document filename contains invalid characters");

    public static readonly DomainError ExtensionRequired =
        new("DOCUMENT_EXTENSION_REQUIRED", "Document extension cannot be empty");

    public static readonly DomainError InvalidFileSize =
        new("DOCUMENT_INVALID_FILE_SIZE", "Document file size must be greater than zero");

    public static readonly DomainError ChecksumRequired =
        new("DOCUMENT_CHECKSUM_REQUIRED", "Document checksum is required for integrity verification");

    public static readonly DomainError ChecksumInvalidLength =
        new("DOCUMENT_CHECKSUM_INVALID_LENGTH", "Document checksum must be exactly 64 characters");

    public static readonly DomainError ChecksumInvalidFormat =
        new("DOCUMENT_CHECKSUM_INVALID_FORMAT", "Document checksum must be a valid hexadecimal string");

    public static readonly DomainError AlreadyCheckedOut =
        new("DOCUMENT_ALREADY_CHECKED_OUT", "Document is already checked out");

    public static readonly DomainError NotCheckedOut =
        new("DOCUMENT_NOT_CHECKED_OUT", "Document is not currently checked out");

    public static readonly DomainError DocumentDeleted =
        new("DOCUMENT_DELETED", "Cannot perform operation on deleted document");

    public static readonly DomainError AlreadyDeleted =
        new("DOCUMENT_ALREADY_DELETED", "Document is already deleted");

    public static readonly DomainError CannotDeleteCheckedOutDocument =
        new("DOCUMENT_CANNOT_DELETE_CHECKED_OUT", "Cannot delete a document that is checked out");
}