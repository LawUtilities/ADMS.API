using ADMS.Domain.Common;

namespace ADMS.Domain.Errors;

/// <summary>
/// Contains all user-related domain errors.
/// </summary>
public static class UserErrors
{
    public static readonly DomainError NameRequired =
        new("USER_NAME_REQUIRED", "User name cannot be empty");

    public static readonly DomainError NameTooShort =
        new("USER_NAME_TOO_SHORT", "User name must be at least 2 characters long");

    public static readonly DomainError NameTooLong =
        new("USER_NAME_TOO_LONG", "User name cannot exceed 50 characters");

    public static readonly DomainError DuplicateName =
        new("USER_DUPLICATE_NAME", "A user with this name already exists");

    public static readonly DomainError CannotDeleteUserWithActivities =
        new("USER_CANNOT_DELETE_WITH_ACTIVITIES", "Cannot delete user with existing activities");
}