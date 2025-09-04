using ADMS.Domain.Common;
using ADMS.Domain.ValueObjects;

namespace ADMS.Domain.Events;

/// <summary>
/// Domain event raised when a document is created.
/// </summary>
public sealed record DocumentCreatedDomainEvent(
    DocumentId DocumentId,
    string FileName,
    UserId CreatedBy
) : DomainEvent;

/// <summary>
/// Domain event raised when a document is checked out.
/// </summary>
public sealed record DocumentCheckedOutDomainEvent(
    DocumentId DocumentId,
    UserId CheckedOutBy
) : DomainEvent;

/// <summary>
/// Domain event raised when a document is checked in.
/// </summary>
public sealed record DocumentCheckedInDomainEvent(
    DocumentId DocumentId,
    UserId CheckedInBy
) : DomainEvent;

/// <summary>
/// Domain event raised when a document is deleted.
/// </summary>
public sealed record DocumentDeletedDomainEvent(
    DocumentId DocumentId,
    UserId DeletedBy
) : DomainEvent;

/// <summary>
/// Domain event raised when a document is restored from deleted state.
/// </summary>
public sealed record DocumentRestoredDomainEvent(
    DocumentId DocumentId,
    UserId RestoredBy
) : DomainEvent;

/// <summary>
/// Domain event raised when a document is updated.
/// </summary>
public sealed record DocumentUpdatedDomainEvent(
    DocumentId DocumentId,
    string? OldFileName,
    string? NewFileName,
    UserId UpdatedBy
) : DomainEvent;