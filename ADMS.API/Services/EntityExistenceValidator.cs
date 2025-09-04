using ADMS.API.DbContexts;
using ADMS.API.Services.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using System.Collections.Immutable;

namespace ADMS.API.Services;

/// <summary>
/// Simple, robust entity existence validation service.
/// </summary>
public sealed class EntityExistenceValidator : IEntityExistenceValidator, IDisposable
{
    private readonly AdmsContext _context;
    private readonly ILogger<EntityExistenceValidator> _logger;
    private readonly IMemoryCache _cache;
    private readonly EntityValidationOptions _options;
    private readonly ADMS.API.Services.Common.PerformanceMetrics _performanceMetrics;
    private bool _disposed;

    public EntityExistenceValidator(
        AdmsContext context,
        ILogger<EntityExistenceValidator> logger,
        IMemoryCache cache,
        IOptions<EntityValidationOptions> options)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _performanceMetrics = new ADMS.API.Services.Common.PerformanceMetrics();

        ValidateOptions();
        _logger.LogInformation("EntityExistenceValidator initialized successfully");
    }

    public async Task<bool> MatterExistsAsync(Guid matterId, CancellationToken cancellationToken = default)
    {
        using var tracker = _performanceMetrics.StartOperation("MatterExists");

        try
        {
            if (matterId == Guid.Empty)
            {
                tracker.RecordSuccess();
                return false;
            }

            var cacheKey = $"matter_exists_{matterId}";
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                tracker.RecordSuccess();
                return cachedResult;
            }

            var exists = await _context.Matters
                .AsNoTracking()
                .Where(m => m.Id == matterId && !m.IsDeleted)
                .AnyAsync(cancellationToken);

            CacheResult(cacheKey, exists);
            tracker.RecordSuccess();

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating matter existence for ID: {MatterId}", matterId);
            tracker.RecordFailure();
            return false;
        }
    }

    public async Task<bool> DocumentExistsAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        using var tracker = _performanceMetrics.StartOperation("DocumentExists");

        try
        {
            if (documentId == Guid.Empty)
            {
                tracker.RecordSuccess();
                return false;
            }

            var cacheKey = $"document_exists_{documentId}";
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                tracker.RecordSuccess();
                return cachedResult;
            }

            var exists = await _context.Documents
                .AsNoTracking()
                .Where(d => d.Id == documentId && !d.IsDeleted)
                .AnyAsync(cancellationToken);

            CacheResult(cacheKey, exists);
            tracker.RecordSuccess();

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating document existence for ID: {DocumentId}", documentId);
            tracker.RecordFailure();
            return false;
        }
    }

    public async Task<bool> RevisionExistsAsync(Guid revisionId, CancellationToken cancellationToken = default)
    {
        using var tracker = _performanceMetrics.StartOperation("RevisionExists");

        try
        {
            if (revisionId == Guid.Empty)
            {
                tracker.RecordSuccess();
                return false;
            }

            var cacheKey = $"revision_exists_{revisionId}";
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                tracker.RecordSuccess();
                return cachedResult;
            }

            var exists = await _context.Revisions
                .AsNoTracking()
                .Where(r => r.Id == revisionId && !r.IsDeleted)
                .AnyAsync(cancellationToken);

            CacheResult(cacheKey, exists);
            tracker.RecordSuccess();

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating revision existence for ID: {RevisionId}", revisionId);
            tracker.RecordFailure();
            return false;
        }
    }

    public async Task<bool> FileNameExistsAsync(Guid matterId, string fileName, CancellationToken cancellationToken = default)
    {
        using var tracker = _performanceMetrics.StartOperation("FileNameExists");

        try
        {
            if (matterId == Guid.Empty || string.IsNullOrWhiteSpace(fileName))
            {
                tracker.RecordSuccess();
                return false;
            }

            var cacheKey = $"filename_exists_{matterId}_{fileName.ToLowerInvariant()}";
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                tracker.RecordSuccess();
                return cachedResult;
            }

            var exists = await _context.Documents
                .AsNoTracking()
                .Where(d => d.MatterId == matterId && d.FileName == fileName && !d.IsDeleted)
                .AnyAsync(cancellationToken);

            CacheResult(cacheKey, exists);
            tracker.RecordSuccess();

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file name existence for MatterId: {MatterId}, FileName: {FileName}", matterId, fileName);
            tracker.RecordFailure();
            return false;
        }
    }

    public async Task<bool> ValidateDocumentBelongsToMatterAsync(Guid matterId, Guid documentId, CancellationToken cancellationToken = default)
    {
        using var tracker = _performanceMetrics.StartOperation("DocumentBelongsToMatter");

        try
        {
            if (matterId == Guid.Empty || documentId == Guid.Empty)
            {
                tracker.RecordSuccess();
                return false;
            }

            var cacheKey = $"doc_belongs_matter_{matterId}_{documentId}";
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                tracker.RecordSuccess();
                return cachedResult;
            }

            var belongs = await _context.Documents
                .AsNoTracking()
                .Where(d => d.Id == documentId && d.MatterId == matterId && !d.IsDeleted)
                .AnyAsync(cancellationToken);

            CacheResult(cacheKey, belongs);
            tracker.RecordSuccess();

            return belongs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating document-matter relationship for MatterId: {MatterId}, DocumentId: {DocumentId}", matterId, documentId);
            tracker.RecordFailure();
            return false;
        }
    }

    public async Task<bool> ValidateRevisionBelongsToDocumentAsync(Guid documentId, Guid revisionId, CancellationToken cancellationToken = default)
    {
        using var tracker = _performanceMetrics.StartOperation("RevisionBelongsToDocument");

        try
        {
            if (documentId == Guid.Empty || revisionId == Guid.Empty)
            {
                tracker.RecordSuccess();
                return false;
            }

            var cacheKey = $"rev_belongs_doc_{documentId}_{revisionId}";
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                tracker.RecordSuccess();
                return cachedResult;
            }

            var belongs = await _context.Revisions
                .AsNoTracking()
                .Where(r => r.Id == revisionId && r.DocumentId == documentId && !r.IsDeleted)
                .AnyAsync(cancellationToken);

            CacheResult(cacheKey, belongs);
            tracker.RecordSuccess();

            return belongs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating revision-document relationship for DocumentId: {DocumentId}, RevisionId: {RevisionId}", documentId, revisionId);
            tracker.RecordFailure();
            return false;
        }
    }

    public async Task<BatchEntityValidationResult> ValidateEntitiesExistAsync(
        IEnumerable<Guid> matterIds,
        IEnumerable<Guid> documentIds,
        IEnumerable<Guid> revisionIds,
        CancellationToken cancellationToken = default)
    {
        using var tracker = _performanceMetrics.StartOperation("BatchValidation");

        try
        {
            var processedMatterIds = FilterValidIds(matterIds);
            var processedDocumentIds = FilterValidIds(documentIds);
            var processedRevisionIds = FilterValidIds(revisionIds);

            var totalEntityCount = processedMatterIds.Count + processedDocumentIds.Count + processedRevisionIds.Count;

            if (totalEntityCount == 0)
            {
                tracker.RecordSuccess(0);
                return BatchEntityValidationResult.AllExist(
                    Array.Empty<Guid>(), Array.Empty<Guid>(), Array.Empty<Guid>());
            }

            var matterTask = ValidateMattersBatchAsync(processedMatterIds, cancellationToken);
            var documentTask = ValidateDocumentsBatchAsync(processedDocumentIds, cancellationToken);
            var revisionTask = ValidateRevisionsBatchAsync(processedRevisionIds, cancellationToken);

            await Task.WhenAll(matterTask, documentTask, revisionTask);

            var (existingMatters, missingMatters) = await matterTask;
            var (existingDocuments, missingDocuments) = await documentTask;
            var (existingRevisions, missingRevisions) = await revisionTask;

            tracker.RecordSuccess(totalEntityCount);

            return BatchEntityValidationResult.WithMissingEntities(
                existingMatters, missingMatters,
                existingDocuments, missingDocuments,
                existingRevisions, missingRevisions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during batch entity validation");
            tracker.RecordFailure();

            // Return all as missing in case of error
            return BatchEntityValidationResult.WithMissingEntities(
                Array.Empty<Guid>(), matterIds,
                Array.Empty<Guid>(), documentIds,
                Array.Empty<Guid>(), revisionIds);
        }
    }

    public Dictionary<string, object> GetDiagnosticInfo()
    {
        try
        {
            var allMetrics = _performanceMetrics.GetAllMetrics();

            return new Dictionary<string, object>
            {
                ["ServiceName"] = nameof(EntityExistenceValidator),
                ["ServiceStatus"] = "Healthy",
                ["Configuration"] = new
                {
                    _options.CacheExpirationMinutes,
                    _options.EnablePerformanceTracking,
                    _options.EnableDetailedLogging
                },
                ["Metrics"] = allMetrics.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new
                    {
                        Count = kvp.Value.Count,
                        Successes = kvp.Value.Successes,
                        Failures = kvp.Value.Failures,
                        SuccessRate = kvp.Value.SuccessRate,
                        AverageDurationMs = kvp.Value.AverageDuration.TotalMilliseconds,
                        EntitiesPerSecond = kvp.Value.EntitiesPerSecond
                    }),
                ["LastUpdate"] = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating diagnostic information");
            return new Dictionary<string, object>
            {
                ["ServiceName"] = nameof(EntityExistenceValidator),
                ["ServiceStatus"] = "Error",
                ["ErrorMessage"] = ex.Message
            };
        }
    }

    public void ClearValidationCache()
    {
        try
        {
            // Note: IMemoryCache doesn't have a direct way to clear all entries
            // In a real implementation, you might use a custom cache wrapper
            _logger.LogInformation("Validation cache clear requested (cache entries will expire based on TTL)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing validation cache");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _logger.LogDebug("EntityExistenceValidator disposed");
        }
    }

    private void ValidateOptions()
    {
        if (_options.CacheExpirationMinutes <= 0)
            throw new InvalidOperationException("CacheExpirationMinutes must be greater than zero");
    }

    private void CacheResult(string cacheKey, bool result)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheExpirationMinutes),
            Priority = CacheItemPriority.Normal
        };

        _cache.Set(cacheKey, result, cacheOptions);
    }

    private static List<Guid> FilterValidIds(IEnumerable<Guid> ids)
    {
        return ids.Where(id => id != Guid.Empty).Distinct().ToList();
    }

    private async Task<(ImmutableList<Guid> Existing, ImmutableList<Guid> Missing)> ValidateMattersBatchAsync(
        List<Guid> matterIds, CancellationToken cancellationToken)
    {
        if (matterIds.Count == 0)
            return (ImmutableList<Guid>.Empty, ImmutableList<Guid>.Empty);

        var existingIds = await _context.Matters
            .AsNoTracking()
            .Where(m => matterIds.Contains(m.Id) && !m.IsDeleted)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        var missingIds = matterIds.Except(existingIds).ToImmutableList();
        return (existingIds.ToImmutableList(), missingIds);
    }

    private async Task<(ImmutableList<Guid> Existing, ImmutableList<Guid> Missing)> ValidateDocumentsBatchAsync(
        List<Guid> documentIds, CancellationToken cancellationToken)
    {
        if (documentIds.Count == 0)
            return (ImmutableList<Guid>.Empty, ImmutableList<Guid>.Empty);

        var existingIds = await _context.Documents
            .AsNoTracking()
            .Where(d => documentIds.Contains(d.Id) && !d.IsDeleted)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        var missingIds = documentIds.Except(existingIds).ToImmutableList();
        return (existingIds.ToImmutableList(), missingIds);
    }

    private async Task<(ImmutableList<Guid> Existing, ImmutableList<Guid> Missing)> ValidateRevisionsBatchAsync(
        List<Guid> revisionIds, CancellationToken cancellationToken)
    {
        if (revisionIds.Count == 0)
            return (ImmutableList<Guid>.Empty, ImmutableList<Guid>.Empty);

        var existingIds = await _context.Revisions
            .AsNoTracking()
            .Where(r => revisionIds.Contains(r.Id) && !r.IsDeleted)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var missingIds = revisionIds.Except(existingIds).ToImmutableList();
        return (existingIds.ToImmutableList(), missingIds);
    }
}

/// <summary>
/// Configuration options for EntityExistenceValidator.
/// </summary>
public sealed class EntityValidationOptions
{
    public int CacheExpirationMinutes { get; set; } = 30;
    public bool EnablePerformanceTracking { get; set; } = true;
    public bool EnableDetailedLogging { get; set; } = false;
}