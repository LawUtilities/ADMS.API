namespace ADMS.API.Services.Common;

/// <summary>
/// Defines the contract for entity existence validation services.
/// </summary>
public interface IEntityExistenceValidator
{
    /// <summary>
    /// Validates whether a matter exists.
    /// </summary>
    Task<bool> MatterExistsAsync(Guid matterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether a document exists.
    /// </summary>
    Task<bool> DocumentExistsAsync(Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether a revision exists.
    /// </summary>
    Task<bool> RevisionExistsAsync(Guid revisionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether a document belongs to a specific matter.
    /// </summary>
    Task<bool> ValidateDocumentBelongsToMatterAsync(Guid matterId, Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether a revision belongs to a specific document.
    /// </summary>
    Task<bool> ValidateRevisionBelongsToDocumentAsync(Guid documentId, Guid revisionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether a file name exists in a matter.
    /// </summary>
    Task<bool> FileNameExistsAsync(Guid matterId, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates multiple entities in a batch operation.
    /// </summary>
    Task<BatchEntityValidationResult> ValidateEntitiesExistAsync(
        IEnumerable<Guid> matterIds,
        IEnumerable<Guid> documentIds,
        IEnumerable<Guid> revisionIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets diagnostic information about the validator.
    /// </summary>
    Dictionary<string, object> GetDiagnosticInfo();

    /// <summary>
    /// Clears the validation cache.
    /// </summary>
    void ClearValidationCache();
}