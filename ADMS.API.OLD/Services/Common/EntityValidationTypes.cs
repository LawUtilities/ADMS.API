namespace ADMS.API.Services.Common;

/// <summary>
/// Enumeration of entity types supported by the validation system.
/// </summary>
public enum EntityType
{
    Matter,
    Document,
    Revision
}

/// <summary>
/// Represents the result of a single entity validation operation.
/// </summary>
public sealed record EntityValidationResult
{
    public required bool Exists { get; init; }
    public required Guid EntityId { get; init; }
    public required EntityType EntityType { get; init; }
    public string? ErrorMessage { get; init; }

    public static EntityValidationResult Success(Guid entityId, EntityType entityType) =>
        new() { Exists = true, EntityId = entityId, EntityType = entityType };

    public static EntityValidationResult NotFound(Guid entityId, EntityType entityType, string? error = null) =>
        new() { Exists = false, EntityId = entityId, EntityType = entityType, ErrorMessage = error };
}

/// <summary>
/// Represents file name conflict information.
/// </summary>
public enum FileNameConflictType
{
    None,
    ExactMatch,
    CaseInsensitiveMatch,
    InvalidCharacters,
    TooLong,
    Reserved
}

/// <summary>
/// Represents a file name conflict result.
/// </summary>
public sealed record FileNameConflict
{
    public required Guid MatterId { get; init; }
    public required string FileName { get; init; }
    public required FileNameConflictType ConflictType { get; init; }
    public string? ConflictDetails { get; init; }
    public Guid? ExistingDocumentId { get; init; }
}