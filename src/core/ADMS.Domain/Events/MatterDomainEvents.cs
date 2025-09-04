using ADMS.Domain.Common;
using ADMS.Domain.ValueObjects;

namespace ADMS.Domain.Events;

/// <summary>
/// Domain event raised when a matter is created.
/// </summary>
public sealed record MatterCreatedDomainEvent(
    MatterId MatterId,
    string Description,
    UserId CreatedBy
) : DomainEvent;

/// <summary>
/// Domain event raised when a matter is archived.
/// </summary>
public sealed record MatterArchivedDomainEvent(
    MatterId MatterId,
    string Description,
    UserId ArchivedBy,
    string? ArchivalReason = null,
    int DocumentCount = 0
) : DomainEvent;

/// <summary>
/// Domain event raised when a matter is deleted.
/// </summary>
public sealed record MatterDeletedDomainEvent(
    MatterId MatterId,
    string Description,
    UserId DeletedBy,
    string? DeletionReason = null,
    int DocumentCount = 0,
    bool HasActiveDocuments = false
) : DomainEvent;

/// <summary>
/// Domain event raised when a matter is restored from deleted state.
/// </summary>
public sealed record MatterRestoredDomainEvent(
    MatterId MatterId,
    string Description,
    UserId RestoredBy,
    string? RestorationReason = null
) : DomainEvent;

/// <summary>
/// Domain event raised when a matter is unarchived (restored to active status).
/// </summary>
public sealed record MatterUnarchivedDomainEvent(
    MatterId MatterId,
    string Description,
    UserId UnarchivedBy,
    string? UnarchivalReason = null
) : DomainEvent;

/// <summary>
/// Domain event raised when a matter is updated.
/// </summary>
public sealed record MatterUpdatedDomainEvent(
    MatterId MatterId,
    string? OldDescription,
    string NewDescription,
    UserId UpdatedBy
) : DomainEvent;