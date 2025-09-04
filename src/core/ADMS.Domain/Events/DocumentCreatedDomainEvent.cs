using ADMS.Domain.Common;
using ADMS.Domain.Entities;

namespace ADMS.Domain.Events;

public sealed record DocumentCreatedDomainEvent(
    DocumentId DocumentId,
    string FileName,
    UserId CreatedBy
) : DomainEvent;