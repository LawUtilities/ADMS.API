using ADMS.Domain.Common;

namespace ADMS.Domain.Errors;

/// <summary>
/// Contains all matter-related domain errors.
/// </summary>
public static class MatterErrors
{
    public static readonly DomainError DescriptionRequired =
        new("MATTER_DESCRIPTION_REQUIRED", "Matter description cannot be empty");

    public static readonly DomainError DescriptionTooLong =
        new("MATTER_DESCRIPTION_TOO_LONG", "Matter description cannot exceed 128 characters");

    public static readonly DomainError DuplicateDescription =
        new("MATTER_DUPLICATE_DESCRIPTION", "A matter with this description already exists");

    public static readonly DomainError AlreadyArchived =
        new("MATTER_ALREADY_ARCHIVED", "Matter is already archived");

    public static readonly DomainError NotArchived =
        new("MATTER_NOT_ARCHIVED", "Matter is not currently archived");

    public static readonly DomainError AlreadyDeleted =
        new("MATTER_ALREADY_DELETED", "Matter is already deleted");

    public static readonly DomainError CannotDeleteWithActiveDocuments =
        new("MATTER_CANNOT_DELETE_WITH_ACTIVE_DOCUMENTS", "Cannot delete matter with active documents");

    public static readonly DomainError CannotArchiveWithCheckedOutDocuments =
        new("MATTER_CANNOT_ARCHIVE_WITH_CHECKED_OUT_DOCUMENTS", "Cannot archive matter with checked out documents");
}