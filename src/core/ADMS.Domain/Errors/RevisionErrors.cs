using ADMS.Domain.Common;

namespace ADMS.Domain.Errors;

/// <summary>
/// Contains all revision-related domain errors.
/// </summary>
public static class RevisionErrors
{
    public static readonly DomainError InvalidRevisionNumber =
        new("REVISION_INVALID_NUMBER", "Revision number must be greater than zero");

    public static readonly DomainError RevisionNumberExists =
        new("REVISION_NUMBER_EXISTS", "A revision with this number already exists for this document");

    public static readonly DomainError InvalidDateSequence =
        new("REVISION_INVALID_DATE_SEQUENCE", "Modification date cannot be earlier than creation date");

    public static readonly DomainError CreationDateInFuture =
        new("REVISION_CREATION_DATE_FUTURE", "Revision creation date cannot be in the future");

    public static readonly DomainError AlreadyDeleted =
        new("REVISION_ALREADY_DELETED", "Revision is already deleted");

    public static readonly DomainError DocumentNotFound =
        new("REVISION_DOCUMENT_NOT_FOUND", "The associated document was not found");
}