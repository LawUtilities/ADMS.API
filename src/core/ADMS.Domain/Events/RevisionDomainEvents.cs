using ADMS.Domain.Common;
using ADMS.Domain.ValueObjects;

namespace ADMS.Domain.Events;

/// <summary>
/// Domain event raised when a document revision is created.
/// </summary>
public sealed record RevisionCreatedDomainEvent(
    RevisionId RevisionId,
    DocumentId DocumentId,
    int RevisionNumber,
    UserId CreatedBy,
    string? CreationReason = null
) : DomainEvent;

/// <summary>
/// Domain event raised when a document revision is updated.
/// </summary>
public sealed record RevisionUpdatedDomainEvent(
    RevisionId RevisionId,
    DocumentId DocumentId,
    int RevisionNumber,
    UserId UpdatedBy,
    string? UpdateReason = null
) : DomainEvent;

/// <summary>
/// Domain event raised when a document revision is deleted.
/// </summary>
public sealed record RevisionDeletedDomainEvent(
    RevisionId RevisionId,
    DocumentId DocumentId,
    int RevisionNumber,
    UserId DeletedBy,
    string? DeletionReason = null
) : DomainEvent;

/// <summary>
/// Domain event raised when a document revision is restored from deleted state.
/// </summary>
public sealed record RevisionRestoredDomainEvent(
    RevisionId RevisionId,
    DocumentId DocumentId,
    int RevisionNumber,
    UserId RestoredBy,
    string? RestorationReason = null
) : DomainEvent;