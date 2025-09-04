using ADMS.API.DbContexts;
using ADMS.API.Services.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace ADMS.API.Services;

/// <summary>
/// Enterprise-grade batch entity validation service providing efficient multi-entity verification.
/// </summary>
/// <remarks>
/// This class provides comprehensive batch validation functionality including:
/// - High-performance batch entity existence checking across multiple types
/// - Optimized database queries using IN clauses and parallel execution
/// - Intelligent caching with configurable expiration policies
/// - Real-time performance metrics and health monitoring
/// - Comprehensive error handling and detailed diagnostic information
/// </remarks>
public sealed class BatchEntityValidator : IBatchEntityValidator, IDisposable
{
    private readonly AdmsContext _context;
    private readonly ILogger<BatchEntityValidator> _logger;
    private readonly IMemoryCache _cache;
    private readonly BatchValidationOptions _options;
    private readonly ADMS.API.Services.Common.PerformanceMetrics _performanceMetrics;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _operationSemaphores;
    private readonly ConcurrentDictionary<string, object> _realTimeStatistics;
    private DateTimeOffset _lastSuccessfulOperation;
    private volatile bool _disposed;

    /// <summary>
    /// Initializes a new instance of the BatchEntityValidator class.
    /// </summary>
    /// <param name="context">The database context for entity operations</param>
    /// <param name="logger">The logger for diagnostic information</param>
    /// <param name="cache">The memory cache for performance optimization</param>
    /// <param name="options">The configuration options for batch validation</param>
    public BatchEntityValidator(
        AdmsContext context,
        ILogger<BatchEntityValidator> logger,
        IMemoryCache cache,
        IOptions<BatchValidationOptions> options)
    {
        // Perform comprehensive null validation for all dependencies with detailed error context
        _context = context ?? throw new ArgumentNullException(nameof(context),
            "Database context is required for entity validation operations");

        _logger = logger ?? throw new ArgumentNullException(nameof(logger),
            "Logger is required for operational monitoring and diagnostic information");

        _cache = cache ?? throw new ArgumentNullException(nameof(cache),
            "Memory cache is required for performance optimization through result caching");

        // Extract and validate the configuration options with comprehensive null safety
        if (options?.Value == null)
        {
            throw new ArgumentNullException(nameof(options),
                "Batch validation configuration options are required and must be properly configured");
        }
        _options = options.Value;

        // Initialize enterprise-grade performance metrics with appropriate retention policies
        _performanceMetrics = new ADMS.API.Services.Common.PerformanceMetrics();

        // Initialize thread-safe concurrent collections for managing operation semaphores
        _operationSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        // Initialize concurrent dictionary for real-time operational statistics
        _realTimeStatistics = new ConcurrentDictionary<string, object>();

        // Set service initialization timestamp for uptime tracking and health monitoring
        _lastSuccessfulOperation = DateTimeOffset.UtcNow;

        try
        {
            // Perform comprehensive configuration validation to ensure operational parameters are reasonable
            ValidateConfigurationInternal();

            // Initialize real-time statistics with baseline values for immediate operational visibility
            InitializeRealTimeStatistics();

            // Log successful initialization with comprehensive configuration summary for operational visibility
            _logger.LogInformation("BatchEntityValidator initialized successfully with configuration: " +
                "MaxBatchSize={MaxBatchSize}, EnableParallelProcessing={EnableParallelProcessing}, " +
                "EnableCaching={EnableCaching}, BatchTimeoutSeconds={BatchTimeoutSeconds}",
                _options.MaxBatchSize,
                _options.EnableParallelProcessing,
                _options.EnableCaching,
                _options.BatchTimeoutSeconds);

            // Log additional operational context for enhanced monitoring and diagnostics
            _logger.LogDebug("BatchEntityValidator operational context: " +
                "ProcessorCount={ProcessorCount}, ServiceStartTime={ServiceStartTime}",
                Environment.ProcessorCount,
                _lastSuccessfulOperation);
        }
        catch (Exception ex)
        {
            // Log comprehensive initialization failure information for troubleshooting
            _logger.LogError(ex, "Failed to initialize BatchEntityValidator with configuration: " +
                "MaxBatchSize={MaxBatchSize}, EnableParallelProcessing={EnableParallelProcessing}, " +
                "EnableCaching={EnableCaching}, BatchTimeoutSeconds={BatchTimeoutSeconds}. " +
                "Error: {ErrorMessage}",
                _options?.MaxBatchSize ?? -1,
                _options?.EnableParallelProcessing ?? false,
                _options?.EnableCaching ?? false,
                _options?.BatchTimeoutSeconds ?? -1,
                ex.Message);

            throw new InvalidOperationException(
                "BatchEntityValidator initialization failed due to configuration or dependency issues. " +
                "Please verify database connectivity, cache availability, and configuration parameters. " +
                $"Original error: {ex.Message}", ex);
        }
    }

    #region Core Batch Validation Methods

    /// <inheritdoc />
    public async Task<BatchEntityValidationResult> ValidateEntitiesExistAsync(
        IEnumerable<Guid> matterIds,
        IEnumerable<Guid> documentIds,
        IEnumerable<Guid> revisionIds,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        ArgumentNullException.ThrowIfNull(matterIds);
        ArgumentNullException.ThrowIfNull(documentIds);
        ArgumentNullException.ThrowIfNull(revisionIds);

        ThrowIfDisposed();

        using var tracker = _performanceMetrics.StartOperation("BatchValidation");
        var operationId = GenerateOperationId();

        try
        {
            _logger.LogDebug("Starting batch entity validation {OperationId}", operationId);

            // Process and sanitize entity ID collections
            var processedMatterIds = FilterValidIds(matterIds);
            var processedDocumentIds = FilterValidIds(documentIds);
            var processedRevisionIds = FilterValidIds(revisionIds);

            var totalEntityCount = processedMatterIds.Count + processedDocumentIds.Count + processedRevisionIds.Count;

            // Enforce batch size limits
            ValidateBatchSize(totalEntityCount, operationId);

            // Handle empty batch scenario
            if (totalEntityCount == 0)
            {
                _logger.LogDebug("Empty batch validation {OperationId}: No entities to validate", operationId);
                tracker.RecordSuccess(0);
                return BatchEntityValidationResult.AllExist(
                    Array.Empty<Guid>(), Array.Empty<Guid>(), Array.Empty<Guid>());
            }

            // Attempt cache retrieval if caching is enabled
            if (_options.EnableCaching)
            {
                var cacheKey = GenerateBatchCacheKey(processedMatterIds, processedDocumentIds, processedRevisionIds);
                if (_cache.TryGetValue(cacheKey, out BatchEntityValidationResult? cachedResult) && cachedResult != null)
                {
                    _logger.LogDebug("Cache hit for batch validation {OperationId}", operationId);
                    UpdateRealTimeStatistics("CacheHits", 1);
                    tracker.RecordSuccess(totalEntityCount);
                    return cachedResult;
                }
                else
                {
                    UpdateRealTimeStatistics("CacheMisses", 1);
                }
            }

            // Create timeout-aware cancellation token
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.BatchTimeoutSeconds));

            // Choose validation strategy based on configuration and batch size
            BatchEntityValidationResult result;
            if (_options.EnableParallelProcessing && ShouldUseParallelProcessing(totalEntityCount))
            {
                _logger.LogDebug("Using parallel validation strategy for {TotalEntities} entities", totalEntityCount);
                result = await ValidateEntitiesParallelAsync(
                    processedMatterIds, processedDocumentIds, processedRevisionIds, timeoutCts.Token);
            }
            else
            {
                _logger.LogDebug("Using sequential validation strategy for {TotalEntities} entities", totalEntityCount);
                result = await ValidateEntitiesSequentialAsync(
                    processedMatterIds, processedDocumentIds, processedRevisionIds, timeoutCts.Token);
            }

            // Cache successful results
            if (_options.EnableCaching)
            {
                await CacheResultAsync(processedMatterIds, processedDocumentIds, processedRevisionIds, result, totalEntityCount);
            }

            // Record successful operation
            tracker.RecordSuccess(totalEntityCount);
            _lastSuccessfulOperation = DateTimeOffset.UtcNow;
            UpdateRealTimeStatistics("TotalSuccessfulOperations", 1);

            _logger.LogInformation("Batch entity validation {OperationId} completed: AllExist={AllExist}, " +
                "TotalEntities={TotalEntities}, Duration={Duration}ms",
                operationId, result.AllEntitiesExist, totalEntityCount, tracker.Elapsed.TotalMilliseconds);

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Batch entity validation {OperationId} cancelled by client", operationId);
            tracker.RecordFailure();
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Batch entity validation {OperationId} timed out after {Timeout} seconds",
                operationId, _options.BatchTimeoutSeconds);
            tracker.RecordFailure();
            throw new TimeoutException($"Batch validation timed out after {_options.BatchTimeoutSeconds} seconds");
        }
        catch (Exception ex)
        {
            tracker.RecordFailure();
            UpdateRealTimeStatistics("TotalFailedOperations", 1);

            _logger.LogError(ex, "Error during batch entity validation {OperationId}", operationId);

            // Return all entities as missing in case of error
            return BatchEntityValidationResult.WithMissingEntities(
                Array.Empty<Guid>(), matterIds,
                Array.Empty<Guid>(), documentIds,
                Array.Empty<Guid>(), revisionIds);
        }
    }

    /// <inheritdoc />
    public async Task<BatchRelationshipValidationResult> ValidateRelationshipsAsync(
        IEnumerable<(Guid MatterId, Guid DocumentId)> documentMatterPairs,
        IEnumerable<(Guid DocumentId, Guid RevisionId)> revisionDocumentPairs,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        ArgumentNullException.ThrowIfNull(documentMatterPairs);
        ArgumentNullException.ThrowIfNull(revisionDocumentPairs);

        ThrowIfDisposed();

        using var tracker = _performanceMetrics.StartOperation("RelationshipValidation");
        var operationId = GenerateOperationId();

        try
        {
            _logger.LogDebug("Starting batch relationship validation {OperationId}", operationId);

            // Process and sanitize relationship pairs
            var docMatterPairsList = documentMatterPairs
                .Where(p => p.MatterId != Guid.Empty && p.DocumentId != Guid.Empty)
                .Distinct()
                .ToList();

            var revDocPairsList = revisionDocumentPairs
                .Where(p => p.DocumentId != Guid.Empty && p.RevisionId != Guid.Empty)
                .Distinct()
                .ToList();

            var totalPairCount = docMatterPairsList.Count + revDocPairsList.Count;

            // Handle empty batch
            if (totalPairCount == 0)
            {
                _logger.LogDebug("Empty batch relationship validation {OperationId}", operationId);
                tracker.RecordSuccess(0);
                return BatchRelationshipValidationResult.AllValid(
                    Array.Empty<(Guid, Guid)>(), Array.Empty<(Guid, Guid)>());
            }

            // Enforce batch size limits
            ValidateBatchSize(totalPairCount, operationId);

            // Create timeout-aware cancellation token
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.BatchTimeoutSeconds));

            // Execute relationship validation tasks concurrently
            var docMatterTask = ValidateDocumentMatterRelationshipsAsync(docMatterPairsList, timeoutCts.Token);
            var revDocTask = ValidateRevisionDocumentRelationshipsAsync(revDocPairsList, timeoutCts.Token);

            await Task.WhenAll(docMatterTask, revDocTask);

            var docMatterResults = await docMatterTask;
            var revDocResults = await revDocTask;

            var result = BatchRelationshipValidationResult.WithInvalidRelationships(
                docMatterResults.ValidPairs,
                docMatterResults.InvalidPairs,
                revDocResults.ValidPairs,
                revDocResults.InvalidPairs);

            tracker.RecordSuccess(totalPairCount);
            _lastSuccessfulOperation = DateTimeOffset.UtcNow;
            UpdateRealTimeStatistics("TotalSuccessfulOperations", 1);

            _logger.LogInformation("Batch relationship validation {OperationId} completed: AllValid={AllValid}, " +
                "TotalPairs={TotalPairs}, Duration={Duration}ms",
                operationId, result.AllRelationshipsValid, totalPairCount, tracker.Elapsed.TotalMilliseconds);

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Batch relationship validation {OperationId} cancelled by client", operationId);
            tracker.RecordFailure();
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Batch relationship validation {OperationId} timed out after {Timeout} seconds",
                operationId, _options.BatchTimeoutSeconds);
            tracker.RecordFailure();
            throw new TimeoutException($"Batch relationship validation timed out after {_options.BatchTimeoutSeconds} seconds");
        }
        catch (Exception ex)
        {
            tracker.RecordFailure();
            UpdateRealTimeStatistics("TotalFailedOperations", 1);

            _logger.LogError(ex, "Error during batch relationship validation {OperationId}", operationId);

            // Return all relationships as invalid in case of error
            return BatchRelationshipValidationResult.WithInvalidRelationships(
                Array.Empty<(Guid, Guid)>(), documentMatterPairs,
                Array.Empty<(Guid, Guid)>(), revisionDocumentPairs);
        }
    }

    /// <inheritdoc />
    public async Task<ComprehensiveValidationResult> ValidateComprehensiveAsync(
        ComprehensiveValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfDisposed();

        using var tracker = _performanceMetrics.StartOperation("ComprehensiveValidation");
        var operationId = GenerateOperationId();

        try
        {
            _logger.LogDebug("Starting comprehensive validation {OperationId} for {TotalEntities} entities " +
                "and {TotalRelationships} relationships",
                operationId, request.TotalEntityCount, request.TotalRelationshipCount);

            // Execute entity validation
            var entityValidationTask = ValidateEntitiesExistAsync(
                request.MatterIds,
                request.DocumentIds,
                request.RevisionIds,
                cancellationToken);

            // Conditionally execute relationship validation
            Task<BatchRelationshipValidationResult>? relationshipValidationTask = null;
            if (request.ValidateRelationships &&
                (request.DocumentMatterPairs.Any() || request.RevisionDocumentPairs.Any()))
            {
                relationshipValidationTask = ValidateRelationshipsAsync(
                    request.DocumentMatterPairs,
                    request.RevisionDocumentPairs,
                    cancellationToken);
            }

            // Wait for entity validation
            var entityResult = await entityValidationTask;

            // Wait for relationship validation if initiated
            BatchRelationshipValidationResult? relationshipResult = null;
            if (relationshipValidationTask != null)
            {
                relationshipResult = await relationshipValidationTask;
            }

            // Determine overall validation success
            var isValid = entityResult.AllEntitiesExist && (relationshipResult?.AllRelationshipsValid ?? true);

            var result = new ComprehensiveValidationResult
            {
                IsValid = isValid,
                EntityValidationResult = entityResult,
                RelationshipValidationResult = relationshipResult,
                OperationId = operationId,
                Duration = tracker.Elapsed,
                Metadata = request.IncludeMetadata ? CreateValidationMetadata(request, tracker.Elapsed, entityResult, relationshipResult) : null
            };

            tracker.RecordSuccess(request.TotalEntityCount + request.TotalRelationshipCount);
            _lastSuccessfulOperation = DateTimeOffset.UtcNow;
            UpdateRealTimeStatistics("TotalSuccessfulOperations", 1);

            _logger.LogInformation("Comprehensive validation {OperationId} completed: IsValid={IsValid}, " +
                "Duration={Duration}ms", operationId, isValid, tracker.Elapsed.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            tracker.RecordFailure();
            UpdateRealTimeStatistics("TotalFailedOperations", 1);

            _logger.LogError(ex, "Error during comprehensive validation {OperationId}", operationId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BatchFileNameValidationResult> ValidateFileNamesAsync(
        IEnumerable<FileNameValidationRequest> fileNameValidationRequests,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileNameValidationRequests);
        ThrowIfDisposed();

        using var tracker = _performanceMetrics.StartOperation("FileNameValidation");
        var operationId = GenerateOperationId();

        try
        {
            _logger.LogDebug("Starting batch file name validation {OperationId}", operationId);

            var requests = fileNameValidationRequests.ToList();
            if (requests.Count == 0)
            {
                tracker.RecordSuccess(0);
                return new BatchFileNameValidationResult
                {
                    HasConflicts = false,
                    ConflictingFileNames = ImmutableList<FileNameConflict>.Empty,
                    ValidFileNames = ImmutableList<FileNameValidationRequest>.Empty,
                    Metadata = ImmutableDictionary<string, object>.Empty
                };
            }

            // Pre-validate requests
            var (validRequests, invalidRequests) = ValidateFileNameRequests(requests);

            if (validRequests.Count == 0)
            {
                tracker.RecordFailure();
                return new BatchFileNameValidationResult
                {
                    HasConflicts = true,
                    ConflictingFileNames = invalidRequests.ToImmutableList(),
                    ValidFileNames = ImmutableList<FileNameValidationRequest>.Empty,
                    Metadata = ImmutableDictionary<string, object>.Empty
                };
            }

            ValidateBatchSize(validRequests.Count, operationId);

            var conflicts = new List<FileNameConflict>(invalidRequests);
            var requestsByMatter = validRequests.GroupBy(r => r.MatterId).ToList();

            foreach (var matterGroup in requestsByMatter)
            {
                var matterId = matterGroup.Key;
                var fileNames = matterGroup.Select(r => r.FileName).ToList();

                try
                {
                    var existingDocuments = await _context.Documents
                        .AsNoTracking()
                        .Where(d => d.MatterId == matterId && fileNames.Contains(d.FileName) && !d.IsDeleted)
                        .Select(d => new { d.Id, d.FileName })
                        .ToListAsync(cancellationToken);

                    foreach (var request in matterGroup)
                    {
                        var existingDoc = existingDocuments.FirstOrDefault(d =>
                            string.Equals(d.FileName, request.FileName, StringComparison.OrdinalIgnoreCase));

                        if (existingDoc != null)
                        {
                            var conflictType = string.Equals(existingDoc.FileName, request.FileName, StringComparison.Ordinal)
                                ? FileNameConflictType.ExactMatch
                                : FileNameConflictType.CaseInsensitiveMatch;

                            conflicts.Add(new FileNameConflict
                            {
                                MatterId = matterId,
                                FileName = request.FileName,
                                ConflictType = conflictType,
                                ExistingDocumentId = existingDoc.Id,
                                ConflictDetails = $"Conflicts with document {existingDoc.Id}: '{existingDoc.FileName}'"
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database error validating file names for matter {MatterId}", matterId);

                    foreach (var request in matterGroup)
                    {
                        conflicts.Add(new FileNameConflict
                        {
                            MatterId = matterId,
                            FileName = request.FileName,
                            ConflictType = FileNameConflictType.InvalidCharacters,
                            ConflictDetails = $"Database validation error: {ex.Message}"
                        });
                    }
                }
            }

            var validFileNames = validRequests.Where(r =>
                !conflicts.Any(c => c.MatterId == r.MatterId &&
                    string.Equals(c.FileName, r.FileName, StringComparison.OrdinalIgnoreCase))).ToList();

            var result = new BatchFileNameValidationResult
            {
                HasConflicts = conflicts.Count > 0,
                ConflictingFileNames = conflicts.ToImmutableList(),
                ValidFileNames = validFileNames.ToImmutableList(),
                Metadata = new Dictionary<string, object>
                {
                    ["ProcessingTimeMs"] = tracker.Elapsed.TotalMilliseconds,
                    ["TotalRequestsProcessed"] = requests.Count,
                    ["ValidRequestsProcessed"] = validRequests.Count,
                    ["TotalConflictsDetected"] = conflicts.Count,
                    ["MattersProcessed"] = requestsByMatter.Count,
                    ["ValidFileNamesIdentified"] = validFileNames.Count,
                    ["OperationId"] = operationId
                }.ToImmutableDictionary()
            };

            tracker.RecordSuccess(validRequests.Count);
            _lastSuccessfulOperation = DateTimeOffset.UtcNow;
            UpdateRealTimeStatistics("TotalSuccessfulOperations", 1);

            _logger.LogInformation("Batch file name validation {OperationId} completed: HasConflicts={HasConflicts}, " +
                "Duration={Duration}ms", operationId, result.HasConflicts, tracker.Elapsed.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            tracker.RecordFailure();
            UpdateRealTimeStatistics("TotalFailedOperations", 1);

            _logger.LogError(ex, "Error during batch file name validation {OperationId}", operationId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BatchEntityValidationResult> ValidateBulkRequestAsync(
        BulkValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfDisposed();

        var operationId = GenerateOperationId();

        try
        {
            // Validate the bulk request structure
            var validationErrors = request.ValidateRequest();
            if (validationErrors.Any())
            {
                var errorMessage = $"Invalid bulk validation request: {string.Join("; ", validationErrors)}";
                _logger.LogWarning("Bulk validation request validation failed {OperationId}: {ValidationErrors}",
                    operationId, errorMessage);
                throw new ArgumentException(errorMessage, nameof(request));
            }

            // Create timeout cancellation token
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(request.TimeoutMilliseconds));

            // Delegate to the primary entity validation method
            var result = await ValidateEntitiesExistAsync(
                request.MatterIds,
                request.DocumentIds,
                request.RevisionIds,
                timeoutCts.Token);

            _logger.LogInformation("Bulk validation request {OperationId} completed: AllExist={AllExist}, " +
                "TotalEntities={TotalEntities}", operationId, result.AllEntitiesExist, request.TotalEntityCount);

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Bulk validation request {OperationId} cancelled by client", operationId);
            throw;
        }
        catch (OperationCanceledException)
        {
            var timeoutMessage = $"Bulk validation request {operationId} timed out after {request.TimeoutMilliseconds}ms";
            _logger.LogWarning(timeoutMessage);
            throw new TimeoutException(timeoutMessage);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bulk validation request {OperationId}", operationId);
            throw;
        }
    }

    #endregion Core Batch Validation Methods

    #region Performance and Monitoring

    /// <inheritdoc />
    public BatchValidationMetrics GetPerformanceMetrics()
    {
        try
        {
            var allMetrics = _performanceMetrics.GetAllMetrics();
            var batchOperations = allMetrics.Where(kvp => kvp.Key.Contains("Validation")).ToList();

            var totalBatchOps = batchOperations.Sum(m => m.Value.Count);
            var avgBatchSize = batchOperations.Any() && totalBatchOps > 0
                ? batchOperations.Sum(m => m.Value.TotalEntities) / (double)totalBatchOps
                : 0.0;
            var avgProcessingTime = batchOperations.Any()
                ? TimeSpan.FromTicks((long)batchOperations.Average(m => m.Value.AverageDuration.Ticks))
                : TimeSpan.Zero;
            var successRate = totalBatchOps > 0
                ? batchOperations.Sum(m => m.Value.Successes) / (double)totalBatchOps
                : 1.0;

            return new BatchValidationMetrics
            {
                TotalBatchOperations = totalBatchOps,
                AverageBatchSize = avgBatchSize,
                AverageBatchProcessingTime = avgProcessingTime,
                BatchSuccessRate = successRate,
                TotalEntitiesProcessed = batchOperations.Sum(m => m.Value.TotalEntities),
                EntitiesPerSecond = avgProcessingTime.TotalSeconds > 0 && avgBatchSize > 0
                    ? avgBatchSize / avgProcessingTime.TotalSeconds
                    : 0.0,
                BatchSizeDistribution = CreateBatchSizeDistribution(),
                EntityTypeBreakdown = CreateEntityTypeBreakdown(batchOperations),
                AdditionalMetrics = new Dictionary<string, object>
                {
                    ["ConfigurationSnapshot"] = _options,
                    ["LastOperationTimestamp"] = _lastSuccessfulOperation,
                    ["CacheEnabled"] = _options.EnableCaching,
                    ["ParallelProcessingEnabled"] = _options.EnableParallelProcessing,
                    ["ServiceUptime"] = DateTimeOffset.UtcNow - _lastSuccessfulOperation,
                    ["RealTimeStatistics"] = new Dictionary<string, object>(_realTimeStatistics)
                }.AsReadOnly()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating batch validation metrics");
            return BatchValidationMetrics.Create(0, 0.0, TimeSpan.Zero, 0.0);
        }
    }

    /// <inheritdoc />
    public async Task<BatchValidationHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var healthIssues = new List<string>();
        var healthScore = 1.0;

        try
        {
            // Test database connectivity
            var dbHealthResult = await TestDatabaseConnectivityAsync(cancellationToken);
            if (!dbHealthResult.IsHealthy)
            {
                healthIssues.Add($"Database connectivity issue: {dbHealthResult.Issue}");
                healthScore -= 0.4;
            }

            // Check performance metrics
            var metrics = GetPerformanceMetrics();
            if (metrics.BatchSuccessRate < 0.95 && metrics.TotalBatchOperations > 10)
            {
                healthIssues.Add($"Low success rate: {metrics.BatchSuccessRate:P2} (threshold: 95%)");
                healthScore -= 0.2;
            }

            if (metrics.AverageBatchProcessingTime > TimeSpan.FromSeconds(30))
            {
                healthIssues.Add($"High processing time: {metrics.AverageBatchProcessingTime.TotalSeconds:F2}s (threshold: 30s)");
                healthScore -= 0.15;
            }

            // Configuration validation
            var configIssues = ValidateConfiguration();
            if (configIssues.Any())
            {
                healthIssues.AddRange(configIssues.Select(issue => $"Configuration: {issue}"));
                healthScore -= 0.1 * configIssues.Count();
            }

            // Cache health check
            if (_options.EnableCaching)
            {
                try
                {
                    var testKey = $"health_check_{Guid.NewGuid():N}";
                    var testValue = "health_test";
                    _cache.Set(testKey, testValue, TimeSpan.FromSeconds(1));

                    if (!_cache.TryGetValue(testKey, out var retrievedValue) || !testValue.Equals(retrievedValue))
                    {
                        healthIssues.Add("Cache functionality impaired");
                        healthScore -= 0.1;
                    }
                    else
                    {
                        _cache.Remove(testKey);
                    }
                }
                catch (Exception ex)
                {
                    healthIssues.Add($"Cache error: {ex.Message}");
                    healthScore -= 0.1;
                }
            }

            var isHealthy = healthScore >= 0.7 && !healthIssues.Any(issue => issue.Contains("Database"));

            return new BatchValidationHealthResult
            {
                IsHealthy = isHealthy,
                HealthScore = Math.Max(0.0, Math.Min(1.0, healthScore)),
                Issues = healthIssues.ToImmutableList(),
                LastSuccessfulOperation = _lastSuccessfulOperation.DateTime,
                Metadata = new Dictionary<string, object>
                {
                    ["TotalOperations"] = metrics.TotalBatchOperations,
                    ["AverageProcessingTimeMs"] = metrics.AverageBatchProcessingTime.TotalMilliseconds,
                    ["SuccessRate"] = metrics.BatchSuccessRate,
                    ["CacheEnabled"] = _options.EnableCaching,
                    ["ParallelProcessingEnabled"] = _options.EnableParallelProcessing,
                    ["MaxBatchSize"] = _options.MaxBatchSize,
                    ["ServiceUptimeMinutes"] = (DateTimeOffset.UtcNow - _lastSuccessfulOperation).TotalMinutes
                }.AsReadOnly()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check");
            return new BatchValidationHealthResult
            {
                IsHealthy = false,
                HealthScore = 0.0,
                Issues = ImmutableList.Create($"Health check failed: {ex.Message}"),
                LastSuccessfulOperation = null,
                Metadata = ImmutableDictionary<string, object>.Empty
            };
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object> GetRealTimeStatistics()
    {
        var statistics = new Dictionary<string, object>(_realTimeStatistics);

        try
        {
            statistics["CurrentThroughput"] = CalculateCurrentThroughput();
            statistics["ActiveOperations"] = _operationSemaphores.Count;
            statistics["CacheHitRate"] = CalculateCacheHitRate();
            statistics["LastUpdateTimestamp"] = DateTimeOffset.UtcNow;
            statistics["ServiceStatus"] = "Running";
            statistics["MemoryUsageMB"] = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calculating real-time statistics");
            statistics["Error"] = ex.Message;
        }

        return statistics.AsReadOnly();
    }

    /// <inheritdoc />
    public BatchValidationOptions GetConfiguration()
    {
        return _options;
    }

    /// <inheritdoc />
    public void ClearCache(string? cacheType = null)
    {
        try
        {
            if (cacheType == null || cacheType.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                _performanceMetrics.Clear();
                _realTimeStatistics.Clear();
                InitializeRealTimeStatistics();
                _logger.LogInformation("All batch validation cache and metrics cleared");
            }
            else if (cacheType.Equals("PerformanceMetrics", StringComparison.OrdinalIgnoreCase))
            {
                _performanceMetrics.Clear();
                _logger.LogInformation("Performance metrics cache cleared");
            }
            else if (cacheType.Equals("ValidationResults", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Validation results cache clear requested - entries will expire based on policy");
            }
            else
            {
                throw new ArgumentException($"Unknown cache type: {cacheType}", nameof(cacheType));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing batch validation cache: {CacheType}", cacheType ?? "All");
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> ValidateConfiguration()
    {
        var issues = new List<string>();

        try
        {
            if (_options.MaxBatchSize <= 0)
                issues.Add("MaxBatchSize must be greater than zero");

            if (_options.BatchTimeoutSeconds <= 0)
                issues.Add("BatchTimeoutSeconds must be greater than zero");

            if (_options.EnableCaching && _options.CacheExpirationMinutes <= 0)
                issues.Add("CacheExpirationMinutes must be greater than zero when caching is enabled");

            if (_context == null)
                issues.Add("Database context is not available");

            if (_cache == null)
                issues.Add("Memory cache is not available");
        }
        catch (Exception ex)
        {
            issues.Add($"Configuration validation failed: {ex.Message}");
        }

        return issues;
    }

    #endregion Performance and Monitoring

    #region IDisposable Implementation

    /// <summary>
    /// Disposes of resources used by the batch entity validator.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var semaphore in _operationSemaphores.Values)
            {
                try
                {
                    semaphore?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing operation semaphore");
                }
            }
            _operationSemaphores.Clear();

            _disposed = true;
            _logger.LogDebug("BatchEntityValidator disposed successfully");
        }
    }

    #endregion IDisposable Implementation

    #region Private Helper Methods

    /// <summary>
    /// Throws ObjectDisposedException if the service has been disposed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(BatchEntityValidator),
                "The batch entity validator service has been disposed and is no longer available for operations.");
        }
    }

    /// <summary>
    /// Validates configuration settings during initialization.
    /// </summary>
    private void ValidateConfigurationInternal()
    {
        var configurationIssues = ValidateConfiguration().ToList();

        if (configurationIssues.Count > 0)
        {
            var issueDetails = string.Join(Environment.NewLine + "  - ", configurationIssues);
            var comprehensiveMessage = $"Batch entity validator configuration validation failed with {configurationIssues.Count} issue(s):" +
                $"{Environment.NewLine}  - {issueDetails}" +
                $"{Environment.NewLine}{Environment.NewLine}" +
                "Please review the configuration settings and ensure all parameters are within valid ranges.";

            throw new InvalidOperationException(comprehensiveMessage);
        }
    }

    /// <summary>
    /// Generates a unique operation identifier for tracking.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GenerateOperationId()
    {
        return Guid.NewGuid().ToString("N")[..8];
    }

    /// <summary>
    /// Filters and validates entity IDs, removing empty GUIDs and duplicates.
    /// </summary>
    private static List<Guid> FilterValidIds(IEnumerable<Guid> entityIds)
    {
        return entityIds.Where(id => id != Guid.Empty).Distinct().ToList();
    }

    /// <summary>
    /// Validates that the entity count doesn't exceed the maximum batch size.
    /// </summary>
    private void ValidateBatchSize(int entityCount, string operationId)
    {
        if (entityCount > _options.MaxBatchSize)
        {
            var errorMessage = $"Batch size validation failed: {entityCount} entities exceeds maximum allowed size of {_options.MaxBatchSize}";
            _logger.LogWarning("Batch validation rejected for operation {OperationId}: {ErrorMessage}",
                operationId, errorMessage);

            throw new ArgumentException(
                $"{errorMessage}. " +
                "To resolve this issue: " +
                "1) Reduce the number of entities in the batch operation, " +
                "2) Process entities in smaller batches, or " +
                "3) Increase the MaxBatchSize configuration setting if system resources allow. " +
                $"Current limit: {_options.MaxBatchSize} entities per batch.",
                nameof(entityCount));
        }
    }

    /// <summary>
    /// Determines whether parallel processing should be used based on entity count.
    /// </summary>
    private static bool ShouldUseParallelProcessing(int entityCount)
    {
        const int parallelProcessingThreshold = 50;
        return entityCount >= parallelProcessingThreshold;
    }

    /// <summary>
    /// Generates a cache key for batch validation results.
    /// </summary>
    private static string GenerateBatchCacheKey(List<Guid> matterIds, List<Guid> documentIds, List<Guid> revisionIds)
    {
        var keyComponents = new List<string>(capacity: 3);

        if (matterIds.Count > 0)
        {
            var sortedMatterIds = matterIds.Order().Select(id => id.ToString("N"));
            keyComponents.Add($"M:{string.Join(",", sortedMatterIds)}");
        }

        if (documentIds.Count > 0)
        {
            var sortedDocumentIds = documentIds.Order().Select(id => id.ToString("N"));
            keyComponents.Add($"D:{string.Join(",", sortedDocumentIds)}");
        }

        if (revisionIds.Count > 0)
        {
            var sortedRevisionIds = revisionIds.Order().Select(id => id.ToString("N"));
            keyComponents.Add($"R:{string.Join(",", sortedRevisionIds)}");
        }

        return $"batch_validation:{string.Join("|", keyComponents)}";
    }

    /// <summary>
    /// Caches the batch validation result.
    /// </summary>
    private Task CacheResultAsync(
        List<Guid> matterIds,
        List<Guid> documentIds,
        List<Guid> revisionIds,
        BatchEntityValidationResult result,
        int totalEntityCount)
    {
        try
        {
            var cacheKey = GenerateBatchCacheKey(matterIds, documentIds, revisionIds);
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheExpirationMinutes),
                Priority = CacheItemPriority.Normal,
                Size = Math.Min(totalEntityCount, 1000)
            };

            _cache.Set(cacheKey, result, cacheOptions);

            _logger.LogDebug("Cached batch validation result: Key={CacheKey}, EntityCount={EntityCount}",
                cacheKey, totalEntityCount);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache batch validation result for {TotalEntityCount} entities",
                totalEntityCount);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates entities using parallel processing.
    /// </summary>
    private async Task<BatchEntityValidationResult> ValidateEntitiesParallelAsync(
        List<Guid> matterIds,
        List<Guid> documentIds,
        List<Guid> revisionIds,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Executing parallel entity validation for {MatterCount} matters, {DocumentCount} documents, {RevisionCount} revisions",
            matterIds.Count, documentIds.Count, revisionIds.Count);

        var validationTasks = new List<Task>(capacity: 3);

        Task<(ImmutableList<Guid> Existing, ImmutableList<Guid> Missing)>? matterTask = null;
        Task<(ImmutableList<Guid> Existing, ImmutableList<Guid> Missing)>? documentTask = null;
        Task<(ImmutableList<Guid> Existing, ImmutableList<Guid> Missing)>? revisionTask = null;

        if (matterIds.Count > 0)
        {
            matterTask = ValidateMattersBatchAsync(matterIds, cancellationToken);
            validationTasks.Add(matterTask);
        }

        if (documentIds.Count > 0)
        {
            documentTask = ValidateDocumentsBatchAsync(documentIds, cancellationToken);
            validationTasks.Add(documentTask);
        }

        if (revisionIds.Count > 0)
        {
            revisionTask = ValidateRevisionsBatchAsync(revisionIds, cancellationToken);
            validationTasks.Add(revisionTask);
        }

        await Task.WhenAll(validationTasks);

        var existingMatters = matterTask?.Result.Existing ?? ImmutableList<Guid>.Empty;
        var missingMatters = matterTask?.Result.Missing ?? ImmutableList<Guid>.Empty;
        var existingDocuments = documentTask?.Result.Existing ?? ImmutableList<Guid>.Empty;
        var missingDocuments = documentTask?.Result.Missing ?? ImmutableList<Guid>.Empty;
        var existingRevisions = revisionTask?.Result.Existing ?? ImmutableList<Guid>.Empty;
        var missingRevisions = revisionTask?.Result.Missing ?? ImmutableList<Guid>.Empty;

        return BatchEntityValidationResult.WithMissingEntities(
            existingMatters, missingMatters,
            existingDocuments, missingDocuments,
            existingRevisions, missingRevisions);
    }

    /// <summary>
    /// Validates entities using sequential processing.
    /// </summary>
    private async Task<BatchEntityValidationResult> ValidateEntitiesSequentialAsync(
        List<Guid> matterIds,
        List<Guid> documentIds,
        List<Guid> revisionIds,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Executing sequential entity validation for {MatterCount} matters, {DocumentCount} documents, {RevisionCount} revisions",
            matterIds.Count, documentIds.Count, revisionIds.Count);

        var matterResults = matterIds.Count > 0
            ? await ValidateMattersBatchAsync(matterIds, cancellationToken)
            : (ImmutableList<Guid>.Empty, ImmutableList<Guid>.Empty);

        var documentResults = documentIds.Count > 0
            ? await ValidateDocumentsBatchAsync(documentIds, cancellationToken)
            : (ImmutableList<Guid>.Empty, ImmutableList<Guid>.Empty);

        var revisionResults = revisionIds.Count > 0
            ? await ValidateRevisionsBatchAsync(revisionIds, cancellationToken)
            : (ImmutableList<Guid>.Empty, ImmutableList<Guid>.Empty);

        return BatchEntityValidationResult.WithMissingEntities(
            matterResults.Item1, matterResults.Item2,
            documentResults.Item1, documentResults.Item2,
            revisionResults.Item1, revisionResults.Item2);
    }

    /// <summary>
    /// Validates matters in batch.
    /// </summary>
    private async Task<(ImmutableList<Guid> Existing, ImmutableList<Guid> Missing)> ValidateMattersBatchAsync(
        List<Guid> matterIds,
        CancellationToken cancellationToken)
    {
        var existingIds = await _context.Matters
            .AsNoTracking()
            .Where(m => matterIds.Contains(m.Id) && !m.IsDeleted)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        var existingSet = existingIds.ToImmutableHashSet();
        var missingIds = matterIds.Where(id => !existingSet.Contains(id)).ToImmutableList();

        return (existingIds.ToImmutableList(), missingIds);
    }

    /// <summary>
    /// Validates documents in batch.
    /// </summary>
    private async Task<(ImmutableList<Guid> Existing, ImmutableList<Guid> Missing)> ValidateDocumentsBatchAsync(
        List<Guid> documentIds,
        CancellationToken cancellationToken)
    {
        var existingIds = await _context.Documents
            .AsNoTracking()
            .Where(d => documentIds.Contains(d.Id) && !d.IsDeleted)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        var existingSet = existingIds.ToImmutableHashSet();
        var missingIds = documentIds.Where(id => !existingSet.Contains(id)).ToImmutableList();

        return (existingIds.ToImmutableList(), missingIds);
    }

    /// <summary>
    /// Validates revisions in batch.
    /// </summary>
    private async Task<(ImmutableList<Guid> Existing, ImmutableList<Guid> Missing)> ValidateRevisionsBatchAsync(
        List<Guid> revisionIds,
        CancellationToken cancellationToken)
    {
        var existingIds = await _context.Revisions
            .AsNoTracking()
            .Where(r => revisionIds.Contains(r.Id) && !r.IsDeleted)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var existingSet = existingIds.ToImmutableHashSet();
        var missingIds = revisionIds.Where(id => !existingSet.Contains(id)).ToImmutableList();

        return (existingIds.ToImmutableList(), missingIds);
    }

    /// <summary>
    /// Validates document-matter relationships.
    /// </summary>
    private async Task<(ImmutableList<(Guid, Guid)> ValidPairs, ImmutableList<(Guid, Guid)> InvalidPairs)>
        ValidateDocumentMatterRelationshipsAsync(
            List<(Guid MatterId, Guid DocumentId)> pairs,
            CancellationToken cancellationToken)
    {
        if (pairs.Count == 0)
            return (ImmutableList<(Guid, Guid)>.Empty, ImmutableList<(Guid, Guid)>.Empty);

        var validRelationships = await _context.Documents
            .AsNoTracking()
            .Where(d => pairs.Any(p => p.MatterId == d.MatterId && p.DocumentId == d.Id) && !d.IsDeleted)
            .Select(d => new { d.MatterId, d.Id })
            .ToListAsync(cancellationToken);

        var validTuples = validRelationships.Select(r => (r.MatterId, r.Id)).ToImmutableHashSet();
        var invalidTuples = pairs.Where(p => !validTuples.Contains(p)).ToImmutableList();

        return (validTuples.ToImmutableList(), invalidTuples);
    }

    /// <summary>
    /// Validates revision-document relationships.
    /// </summary>
    private async Task<(ImmutableList<(Guid, Guid)> ValidPairs, ImmutableList<(Guid, Guid)> InvalidPairs)>
        ValidateRevisionDocumentRelationshipsAsync(
            List<(Guid DocumentId, Guid RevisionId)> pairs,
            CancellationToken cancellationToken)
    {
        if (pairs.Count == 0)
            return (ImmutableList<(Guid, Guid)>.Empty, ImmutableList<(Guid, Guid)>.Empty);

        var validRelationships = await _context.Revisions
            .AsNoTracking()
            .Where(r => pairs.Any(p => p.DocumentId == r.DocumentId && p.RevisionId == r.Id) && !r.IsDeleted)
            .Select(r => new { r.DocumentId, r.Id })
            .ToListAsync(cancellationToken);

        var validTuples = validRelationships.Select(r => (r.DocumentId, r.Id)).ToImmutableHashSet();
        var invalidTuples = pairs.Where(p => !validTuples.Contains(p)).ToImmutableList();

        return (validTuples.ToImmutableList(), invalidTuples);
    }

    /// <summary>
    /// Tests database connectivity for health checks.
    /// </summary>
    private async Task<(bool IsHealthy, string? Issue)> TestDatabaseConnectivityAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

            var canConnect = await _context.Database.CanConnectAsync(timeoutCts.Token);

            return canConnect
                ? (true, null)
                : (false, "Database connection test failed - database may be unavailable");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return (false, "Database connectivity test cancelled by client request");
        }
        catch (OperationCanceledException)
        {
            return (false, "Database connectivity test timed out after 10 seconds");
        }
        catch (Exception ex)
        {
            return (false, $"Database connectivity test failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates real-time statistics with thread-safe operations.
    /// </summary>
    private void UpdateRealTimeStatistics(string key, object value)
    {
        try
        {
            _realTimeStatistics.AddOrUpdate(key, value, (k, existing) =>
            {
                if (existing is int existingInt && value is int valueInt)
                    return existingInt + valueInt;
                if (existing is long existingLong && value is long valueLong)
                    return existingLong + valueLong;
                if (existing is double existingDouble && value is double valueDouble)
                    return existingDouble + valueDouble;

                return value;
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update real-time statistic '{StatisticKey}' with value '{StatisticValue}'",
                key, value);
        }
    }

    /// <summary>
    /// Initializes real-time statistics with baseline values.
    /// </summary>
    private void InitializeRealTimeStatistics()
    {
        try
        {
            _realTimeStatistics.TryAdd("TotalSuccessfulOperations", 0);
            _realTimeStatistics.TryAdd("TotalFailedOperations", 0);
            _realTimeStatistics.TryAdd("CacheHits", 0);
            _realTimeStatistics.TryAdd("CacheMisses", 0);
            _realTimeStatistics.TryAdd("LastSuccessfulOperation", _lastSuccessfulOperation);
            _realTimeStatistics.TryAdd("ServiceInitializationTime", _lastSuccessfulOperation);
            _realTimeStatistics.TryAdd("ServiceStatus", "Running");
            _realTimeStatistics.TryAdd("MaxBatchSizeConfigured", _options.MaxBatchSize);
            _realTimeStatistics.TryAdd("ParallelProcessingEnabled", _options.EnableParallelProcessing);
            _realTimeStatistics.TryAdd("CachingEnabled", _options.EnableCaching);

            _logger.LogDebug("Real-time statistics initialized successfully with {StatisticCount} baseline metrics",
                _realTimeStatistics.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize real-time statistics");
        }
    }

    /// <summary>
    /// Creates entity type breakdown for performance metrics.
    /// </summary>
    private IReadOnlyDictionary<string, EntityTypeMetrics> CreateEntityTypeBreakdown(
        List<KeyValuePair<string, OperationMetrics>> batchMetrics)
    {
        var breakdown = new Dictionary<string, EntityTypeMetrics>();

        try
        {
            var operationTypes = new[] { "BatchValidation", "RelationshipValidation", "FileNameValidation", "ComprehensiveValidation" };

            foreach (var operationName in operationTypes)
            {
                var metric = batchMetrics.FirstOrDefault(m => m.Key == operationName);
                if (metric.Value != null)
                {
                    var entityType = operationName.Replace("Validation", "");
                    var entityTypeMetrics = new EntityTypeMetrics
                    {
                        EntityType = entityType,
                        TotalProcessed = metric.Value.Count,
                        AverageProcessingTime = metric.Value.AverageDuration,
                        SuccessRate = metric.Value.SuccessRate,
                        CacheHitRate = CalculateCacheHitRate(),
                        EntitiesPerSecond = metric.Value.EntitiesPerSecond
                    };

                    breakdown[entityType] = entityTypeMetrics;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity type breakdown");
        }

        return breakdown.AsReadOnly();
    }

    /// <summary>
    /// Creates batch size distribution for metrics.
    /// </summary>
    private IReadOnlyDictionary<string, int> CreateBatchSizeDistribution()
    {
        try
        {
            var distribution = new Dictionary<string, int>
            {
                ["Small (1-10)"] = 0,
                ["Medium (11-100)"] = 0,
                ["Large (101-1000)"] = 0,
                ["XLarge (1000+)"] = 0
            };

            _logger.LogDebug("Batch size distribution structure created");
            return distribution.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error creating batch size distribution");
            return new Dictionary<string, int>().AsReadOnly();
        }
    }

    /// <summary>
    /// Calculates current throughput.
    /// </summary>
    private double CalculateCurrentThroughput()
    {
        try
        {
            var allMetrics = _performanceMetrics.GetAllMetrics();
            var batchMetrics = allMetrics.Where(m => m.Key.Contains("Validation")).ToList();

            if (batchMetrics.Any())
            {
                var avgTime = TimeSpan.FromTicks((long)batchMetrics.Average(m => m.Value.AverageDuration.Ticks));
                return avgTime.TotalSeconds > 0 ? 1.0 / avgTime.TotalSeconds : 0.0;
            }

            return 0.0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calculating current throughput");
            return 0.0;
        }
    }

    /// <summary>
    /// Calculates cache hit rate.
    /// </summary>
    private double CalculateCacheHitRate()
    {
        try
        {
            var hits = _realTimeStatistics.TryGetValue("CacheHits", out var h) && h is int hitsInt ? hitsInt : 0;
            var misses = _realTimeStatistics.TryGetValue("CacheMisses", out var m) && m is int missesInt ? missesInt : 0;
            var total = hits + misses;

            return total > 0 ? hits / (double)total : 0.0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calculating cache hit rate");
            return 0.0;
        }
    }

    /// <summary>
    /// Validates file name requests and separates valid from invalid ones.
    /// </summary>
    private (List<FileNameValidationRequest> ValidRequests, List<FileNameConflict> InvalidRequests) ValidateFileNameRequests(
        List<FileNameValidationRequest> requests)
    {
        var validRequests = new List<FileNameValidationRequest>();
        var invalidRequests = new List<FileNameConflict>();

        foreach (var request in requests)
        {
            try
            {
                var validationErrors = request.ValidateRequest();
                if (validationErrors.Any())
                {
                    invalidRequests.Add(new FileNameConflict
                    {
                        MatterId = request.MatterId,
                        FileName = request.FileName,
                        ConflictType = FileNameConflictType.InvalidCharacters,
                        ConflictDetails = string.Join("; ", validationErrors)
                    });
                }
                else
                {
                    validRequests.Add(request);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception during file name request validation");
                invalidRequests.Add(new FileNameConflict
                {
                    MatterId = request.MatterId,
                    FileName = request.FileName ?? "<invalid>",
                    ConflictType = FileNameConflictType.InvalidCharacters,
                    ConflictDetails = $"Request validation error: {ex.Message}"
                });
            }
        }

        return (validRequests, invalidRequests);
    }

    /// <summary>
    /// Creates validation metadata for comprehensive validation results.
    /// </summary>
    private IReadOnlyDictionary<string, object> CreateValidationMetadata(
        ComprehensiveValidationRequest request,
        TimeSpan duration,
        BatchEntityValidationResult entityResult,
        BatchRelationshipValidationResult? relationshipResult)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["TotalEntities"] = request.TotalEntityCount,
                ["TotalRelationships"] = request.TotalRelationshipCount,
                ["ProcessingTimeMs"] = duration.TotalMilliseconds,
                ["ProcessingTimeSeconds"] = duration.TotalSeconds,
                ["ProcessingStartTimestamp"] = DateTimeOffset.UtcNow.Subtract(duration),
                ["ProcessingEndTimestamp"] = DateTimeOffset.UtcNow,
                ["EntityValidationDetails"] = new
                {
                    AllExist = entityResult.AllEntitiesExist,
                    ExistingCount = entityResult.GetTotalExistingCount(),
                    MissingCount = entityResult.GetTotalMissingCount(),
                    TotalValidated = entityResult.GetTotalExistingCount() + entityResult.GetTotalMissingCount(),
                    SuccessRate = entityResult.GetTotalExistingCount() > 0 || entityResult.GetTotalMissingCount() > 0
                        ? entityResult.GetTotalExistingCount() / (double)(entityResult.GetTotalExistingCount() + entityResult.GetTotalMissingCount())
                        : 0.0
                },
                ["RelationshipValidationDetails"] = relationshipResult != null ? new
                {
                    AllValid = relationshipResult.AllRelationshipsValid,
                    ValidCount = relationshipResult.GetTotalValidCount(),
                    InvalidCount = relationshipResult.GetTotalInvalidCount(),
                    TotalValidated = relationshipResult.GetTotalValidCount() + relationshipResult.GetTotalInvalidCount(),
                    SuccessRate = relationshipResult.GetTotalValidCount() > 0 || relationshipResult.GetTotalInvalidCount() > 0
                        ? relationshipResult.GetTotalValidCount() / (double)(relationshipResult.GetTotalValidCount() + relationshipResult.GetTotalInvalidCount())
                        : 0.0
                } : null,
                ["ValidationMode"] = request.ValidateRelationships ? "EntityAndRelationship" : "EntityOnly",
                ["RequestId"] = request.RequestId ?? "None",
                ["IncludeMetadata"] = request.IncludeMetadata,
                ["PerformanceMetrics"] = new
                {
                    EntitiesPerSecond = request.TotalEntityCount > 0 && duration.TotalSeconds > 0
                        ? request.TotalEntityCount / duration.TotalSeconds
                        : 0.0,
                    RelationshipsPerSecond = request.TotalRelationshipCount > 0 && duration.TotalSeconds > 0
                        ? request.TotalRelationshipCount / duration.TotalSeconds
                        : 0.0,
                    TotalItemsPerSecond = (request.TotalEntityCount + request.TotalRelationshipCount) > 0 && duration.TotalSeconds > 0
                        ? (request.TotalEntityCount + request.TotalRelationshipCount) / duration.TotalSeconds
                        : 0.0
                },
                ["ServiceConfiguration"] = new
                {
                    MaxBatchSize = _options.MaxBatchSize,
                    ParallelProcessingEnabled = _options.EnableParallelProcessing,
                    CachingEnabled = _options.EnableCaching,
                    TimeoutSeconds = _options.BatchTimeoutSeconds
                },
                ["OperationalContext"] = new
                {
                    ProcessorCount = Environment.ProcessorCount,
                    ServiceUptime = DateTimeOffset.UtcNow - _lastSuccessfulOperation,
                    MemoryUsageMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0)
                }
            };

            _logger.LogDebug("Comprehensive validation metadata created with {MetadataItemCount} data points",
                metadata.Count);

            return metadata.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error creating comprehensive validation metadata");

            return new Dictionary<string, object>
            {
                ["TotalEntities"] = request.TotalEntityCount,
                ["TotalRelationships"] = request.TotalRelationshipCount,
                ["ProcessingTimeMs"] = duration.TotalMilliseconds,
                ["ValidationMode"] = request.ValidateRelationships ? "EntityAndRelationship" : "EntityOnly",
                ["RequestId"] = request.RequestId ?? "None",
                ["MetadataCreationError"] = ex.Message
            }.AsReadOnly();
        }
    }

    #endregion Private Helper Methods
}

#region Supporting Types and Classes

/// <summary>
/// Configuration options for batch validation operations.
/// </summary>
public sealed class BatchValidationOptions
{
    /// <summary>
    /// Gets or sets the maximum number of entities allowed in a single batch operation.
    /// Default value is 1000.
    /// </summary>
    public int MaxBatchSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the timeout in seconds for batch validation operations.
    /// Default value is 30 seconds.
    /// </summary>
    public int BatchTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether parallel processing is enabled for batch operations.
    /// Default value is true.
    /// </summary>
    public bool EnableParallelProcessing { get; set; } = true;

    /// <summary>
    /// Gets or sets whether caching is enabled for validation results.
    /// Default value is true.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache expiration time in minutes.
    /// Default value is 15 minutes.
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Validates the configuration options and returns any validation errors.
    /// </summary>
    /// <returns>A collection of validation error messages.</returns>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (MaxBatchSize <= 0)
            errors.Add("MaxBatchSize must be greater than zero");

        if (MaxBatchSize > 10000)
            errors.Add("MaxBatchSize should not exceed 10,000 for optimal performance");

        if (BatchTimeoutSeconds <= 0)
            errors.Add("BatchTimeoutSeconds must be greater than zero");

        if (BatchTimeoutSeconds > 300)
            errors.Add("BatchTimeoutSeconds should not exceed 300 seconds (5 minutes)");

        if (EnableCaching && CacheExpirationMinutes <= 0)
            errors.Add("CacheExpirationMinutes must be greater than zero when caching is enabled");

        return errors;
    }
}

/// <summary>
/// Represents the result of a batch entity validation operation.
/// </summary>
public sealed class BatchEntityValidationResult
{
    /// <summary>
    /// Gets whether all entities in the batch exist.
    /// </summary>
    public required bool AllEntitiesExist { get; init; }

    /// <summary>
    /// Gets the collection of existing matter IDs.
    /// </summary>
    public required ImmutableList<Guid> ExistingMatters { get; init; }

    /// <summary>
    /// Gets the collection of missing matter IDs.
    /// </summary>
    public required ImmutableList<Guid> MissingMatters { get; init; }

    /// <summary>
    /// Gets the collection of existing document IDs.
    /// </summary>
    public required ImmutableList<Guid> ExistingDocuments { get; init; }

    /// <summary>
    /// Gets the collection of missing document IDs.
    /// </summary>
    public required ImmutableList<Guid> MissingDocuments { get; init; }

    /// <summary>
    /// Gets the collection of existing revision IDs.
    /// </summary>
    public required ImmutableList<Guid> ExistingRevisions { get; init; }

    /// <summary>
    /// Gets the collection of missing revision IDs.
    /// </summary>
    public required ImmutableList<Guid> MissingRevisions { get; init; }

    /// <summary>
    /// Gets optional metadata about the validation operation.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Gets the total number of existing entities across all types.
    /// </summary>
    public int GetTotalExistingCount() => ExistingMatters.Count + ExistingDocuments.Count + ExistingRevisions.Count;

    /// <summary>
    /// Gets the total number of missing entities across all types.
    /// </summary>
    public int GetTotalMissingCount() => MissingMatters.Count + MissingDocuments.Count + MissingRevisions.Count;

    /// <summary>
    /// Gets a human-readable description of missing entities.
    /// </summary>
    public string GetMissingEntitiesDescription()
    {
        var descriptions = new List<string>();

        if (MissingMatters.Count > 0)
            descriptions.Add($"{MissingMatters.Count} matter(s)");

        if (MissingDocuments.Count > 0)
            descriptions.Add($"{MissingDocuments.Count} document(s)");

        if (MissingRevisions.Count > 0)
            descriptions.Add($"{MissingRevisions.Count} revision(s)");

        return descriptions.Count > 0 ? string.Join(", ", descriptions) : "None";
    }

    /// <summary>
    /// Creates a result indicating all entities exist.
    /// </summary>
    public static BatchEntityValidationResult AllExist(
        IEnumerable<Guid> matterIds,
        IEnumerable<Guid> documentIds,
        IEnumerable<Guid> revisionIds)
    {
        return new BatchEntityValidationResult
        {
            AllEntitiesExist = true,
            ExistingMatters = matterIds.ToImmutableList(),
            MissingMatters = ImmutableList<Guid>.Empty,
            ExistingDocuments = documentIds.ToImmutableList(),
            MissingDocuments = ImmutableList<Guid>.Empty,
            ExistingRevisions = revisionIds.ToImmutableList(),
            MissingRevisions = ImmutableList<Guid>.Empty
        };
    }

    /// <summary>
    /// Creates a result with specified missing entities.
    /// </summary>
    public static BatchEntityValidationResult WithMissingEntities(
        IEnumerable<Guid> existingMatters,
        IEnumerable<Guid> missingMatters,
        IEnumerable<Guid> existingDocuments,
        IEnumerable<Guid> missingDocuments,
        IEnumerable<Guid> existingRevisions,
        IEnumerable<Guid> missingRevisions)
    {
        var existingMattersList = existingMatters.ToImmutableList();
        var missingMattersList = missingMatters.ToImmutableList();
        var existingDocumentsList = existingDocuments.ToImmutableList();
        var missingDocumentsList = missingDocuments.ToImmutableList();
        var existingRevisionsList = existingRevisions.ToImmutableList();
        var missingRevisionsList = missingRevisions.ToImmutableList();

        return new BatchEntityValidationResult
        {
            AllEntitiesExist = missingMattersList.Count == 0 &&
                              missingDocumentsList.Count == 0 &&
                              missingRevisionsList.Count == 0,
            ExistingMatters = existingMattersList,
            MissingMatters = missingMattersList,
            ExistingDocuments = existingDocumentsList,
            MissingDocuments = missingDocumentsList,
            ExistingRevisions = existingRevisionsList,
            MissingRevisions = missingRevisionsList
        };
    }
}

/// <summary>
/// Represents the result of a batch relationship validation operation.
/// </summary>
public sealed class BatchRelationshipValidationResult
{
    /// <summary>
    /// Gets whether all relationships in the batch are valid.
    /// </summary>
    public required bool AllRelationshipsValid { get; init; }

    /// <summary>
    /// Gets the collection of valid document-matter relationship pairs.
    /// </summary>
    public required ImmutableList<(Guid MatterId, Guid DocumentId)> ValidDocumentMatterPairs { get; init; }

    /// <summary>
    /// Gets the collection of invalid document-matter relationship pairs.
    /// </summary>
    public required ImmutableList<(Guid MatterId, Guid DocumentId)> InvalidDocumentMatterPairs { get; init; }

    /// <summary>
    /// Gets the collection of valid revision-document relationship pairs.
    /// </summary>
    public required ImmutableList<(Guid DocumentId, Guid RevisionId)> ValidRevisionDocumentPairs { get; init; }

    /// <summary>
    /// Gets the collection of invalid revision-document relationship pairs.
    /// </summary>
    public required ImmutableList<(Guid DocumentId, Guid RevisionId)> InvalidRevisionDocumentPairs { get; init; }

    /// <summary>
    /// Gets the total number of valid relationships across all types.
    /// </summary>
    public int GetTotalValidCount() => ValidDocumentMatterPairs.Count + ValidRevisionDocumentPairs.Count;

    /// <summary>
    /// Gets the total number of invalid relationships across all types.
    /// </summary>
    public int GetTotalInvalidCount() => InvalidDocumentMatterPairs.Count + InvalidRevisionDocumentPairs.Count;

    /// <summary>
    /// Gets a human-readable description of invalid relationships.
    /// </summary>
    public string GetInvalidRelationshipsDescription()
    {
        var descriptions = new List<string>();

        if (InvalidDocumentMatterPairs.Count > 0)
            descriptions.Add($"{InvalidDocumentMatterPairs.Count} document-matter relationship(s)");

        if (InvalidRevisionDocumentPairs.Count > 0)
            descriptions.Add($"{InvalidRevisionDocumentPairs.Count} revision-document relationship(s)");

        return descriptions.Count > 0 ? string.Join(", ", descriptions) : "None";
    }

    /// <summary>
    /// Creates a result indicating all relationships are valid.
    /// </summary>
    public static BatchRelationshipValidationResult AllValid(
        IEnumerable<(Guid MatterId, Guid DocumentId)> documentMatterPairs,
        IEnumerable<(Guid DocumentId, Guid RevisionId)> revisionDocumentPairs)
    {
        return new BatchRelationshipValidationResult
        {
            AllRelationshipsValid = true,
            ValidDocumentMatterPairs = documentMatterPairs.ToImmutableList(),
            InvalidDocumentMatterPairs = ImmutableList<(Guid, Guid)>.Empty,
            ValidRevisionDocumentPairs = revisionDocumentPairs.ToImmutableList(),
            InvalidRevisionDocumentPairs = ImmutableList<(Guid, Guid)>.Empty
        };
    }

    /// <summary>
    /// Creates a result with specified invalid relationships.
    /// </summary>
    public static BatchRelationshipValidationResult WithInvalidRelationships(
        IEnumerable<(Guid MatterId, Guid DocumentId)> validDocumentMatterPairs,
        IEnumerable<(Guid MatterId, Guid DocumentId)> invalidDocumentMatterPairs,
        IEnumerable<(Guid DocumentId, Guid RevisionId)> validRevisionDocumentPairs,
        IEnumerable<(Guid DocumentId, Guid RevisionId)> invalidRevisionDocumentPairs)
    {
        var validDocMatterList = validDocumentMatterPairs.ToImmutableList();
        var invalidDocMatterList = invalidDocumentMatterPairs.ToImmutableList();
        var validRevDocList = validRevisionDocumentPairs.ToImmutableList();
        var invalidRevDocList = invalidRevisionDocumentPairs.ToImmutableList();

        return new BatchRelationshipValidationResult
        {
            AllRelationshipsValid = invalidDocMatterList.Count == 0 && invalidRevDocList.Count == 0,
            ValidDocumentMatterPairs = validDocMatterList,
            InvalidDocumentMatterPairs = invalidDocMatterList,
            ValidRevisionDocumentPairs = validRevDocList,
            InvalidRevisionDocumentPairs = invalidRevDocList
        };
    }
}

/// <summary>
/// Represents a comprehensive validation request containing entities and relationships to validate.
/// </summary>
public sealed class ComprehensiveValidationRequest
{
    /// <summary>
    /// Gets or sets the collection of matter IDs to validate.
    /// </summary>
    public IEnumerable<Guid> MatterIds { get; set; } = Enumerable.Empty<Guid>();

    /// <summary>
    /// Gets or sets the collection of document IDs to validate.
    /// </summary>
    public IEnumerable<Guid> DocumentIds { get; set; } = Enumerable.Empty<Guid>();

    /// <summary>
    /// Gets or sets the collection of revision IDs to validate.
    /// </summary>
    public IEnumerable<Guid> RevisionIds { get; set; } = Enumerable.Empty<Guid>();

    /// <summary>
    /// Gets or sets the collection of document-matter relationship pairs to validate.
    /// </summary>
    public IEnumerable<(Guid MatterId, Guid DocumentId)> DocumentMatterPairs { get; set; } = Enumerable.Empty<(Guid, Guid)>();

    /// <summary>
    /// Gets or sets the collection of revision-document relationship pairs to validate.
    /// </summary>
    public IEnumerable<(Guid DocumentId, Guid RevisionId)> RevisionDocumentPairs { get; set; } = Enumerable.Empty<(Guid, Guid)>();

    /// <summary>
    /// Gets or sets whether relationship validation should be performed.
    /// </summary>
    public bool ValidateRelationships { get; set; }

    /// <summary>
    /// Gets or sets whether detailed metadata should be included in the result.
    /// </summary>
    public bool IncludeMetadata { get; set; }

    /// <summary>
    /// Gets or sets an optional request identifier for tracking purposes.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets the total number of entities to validate.
    /// </summary>
    public int TotalEntityCount => MatterIds.Count() + DocumentIds.Count() + RevisionIds.Count();

    /// <summary>
    /// Gets the total number of relationships to validate.
    /// </summary>
    public int TotalRelationshipCount => DocumentMatterPairs.Count() + RevisionDocumentPairs.Count();
}

/// <summary>
/// Represents the result of a comprehensive validation operation.
/// </summary>
public sealed class ComprehensiveValidationResult
{
    /// <summary>
    /// Gets whether the overall validation is valid (all entities exist and all relationships are valid).
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Gets the entity validation result.
    /// </summary>
    public required BatchEntityValidationResult EntityValidationResult { get; init; }

    /// <summary>
    /// Gets the relationship validation result, if relationship validation was performed.
    /// </summary>
    public BatchRelationshipValidationResult? RelationshipValidationResult { get; init; }

    /// <summary>
    /// Gets the operation identifier for tracking purposes.
    /// </summary>
    public required string OperationId { get; init; }

    /// <summary>
    /// Gets the total duration of the validation operation.
    /// </summary>
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets optional metadata about the validation operation.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Gets a comprehensive validation summary.
    /// </summary>
    public string GetValidationSummary()
    {
        var summary = new List<string>();

        if (!EntityValidationResult.AllEntitiesExist)
        {
            summary.Add($"Missing entities: {EntityValidationResult.GetMissingEntitiesDescription()}");
        }

        if (RelationshipValidationResult != null && !RelationshipValidationResult.AllRelationshipsValid)
        {
            summary.Add($"Invalid relationships: {RelationshipValidationResult.GetInvalidRelationshipsDescription()}");
        }

        if (summary.Count == 0)
        {
            return "All validations passed successfully";
        }

        return string.Join("; ", summary);
    }
}

/// <summary>
/// Represents a request to validate file name uniqueness within a matter.
/// </summary>
public sealed class FileNameValidationRequest
{
    /// <summary>
    /// Gets or sets the matter ID within which to check for file name uniqueness.
    /// </summary>
    public required Guid MatterId { get; set; }

    /// <summary>
    /// Gets or sets the file name to check for uniqueness.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// Validates the request and returns any validation errors.
    /// </summary>
    /// <returns>A collection of validation error messages.</returns>
    public IEnumerable<string> ValidateRequest()
    {
        var errors = new List<string>();

        if (MatterId == Guid.Empty)
            errors.Add("MatterId cannot be empty");

        if (string.IsNullOrWhiteSpace(FileName))
            errors.Add("FileName cannot be null or empty");
        else
        {
            if (FileName.Length > 255)
                errors.Add("FileName cannot exceed 255 characters");

            var invalidChars = Path.GetInvalidFileNameChars();
            if (FileName.Any(c => invalidChars.Contains(c)))
                errors.Add("FileName contains invalid characters");
        }

        return errors;
    }
}

/// <summary>
/// Represents the result of a batch file name validation operation.
/// </summary>
public sealed class BatchFileNameValidationResult
{
    /// <summary>
    /// Gets whether any file name conflicts were detected.
    /// </summary>
    public required bool HasConflicts { get; init; }

    /// <summary>
    /// Gets the collection of conflicting file names with details about the conflicts.
    /// </summary>
    public required ImmutableList<FileNameConflict> ConflictingFileNames { get; init; }

    /// <summary>
    /// Gets the collection of valid file names that do not have conflicts.
    /// </summary>
    public required ImmutableList<FileNameValidationRequest> ValidFileNames { get; init; }

    /// <summary>
    /// Gets optional metadata about the validation operation.
    /// </summary>
    public required IReadOnlyDictionary<string, object> Metadata { get; init; }

    /// <summary>
    /// Gets a validation summary describing the results.
    /// </summary>
    public string GetValidationSummary()
    {
        if (!HasConflicts)
        {
            return $"All {ValidFileNames.Count} file names are available";
        }

        return $"{ConflictingFileNames.Count} conflicts detected, {ValidFileNames.Count} file names available";
    }
}

/// <summary>
/// Represents a bulk validation request with timeout and configuration options.
/// </summary>
public sealed class BulkValidationRequest
{
    /// <summary>
    /// Gets or sets the collection of matter IDs to validate.
    /// </summary>
    public IEnumerable<Guid> MatterIds { get; set; } = Enumerable.Empty<Guid>();

    /// <summary>
    /// Gets or sets the collection of document IDs to validate.
    /// </summary>
    public IEnumerable<Guid> DocumentIds { get; set; } = Enumerable.Empty<Guid>();

    /// <summary>
    /// Gets or sets the collection of revision IDs to validate.
    /// </summary>
    public IEnumerable<Guid> RevisionIds { get; set; } = Enumerable.Empty<Guid>();

    /// <summary>
    /// Gets or sets whether relationship validation should be performed.
    /// </summary>
    public bool ValidateRelationships { get; set; }

    /// <summary>
    /// Gets or sets whether detailed metadata should be included in the result.
    /// </summary>
    public bool IncludeMetadata { get; set; }

    /// <summary>
    /// Gets or sets the timeout in milliseconds for the validation operation.
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 30000; // 30 seconds default

    /// <summary>
    /// Gets or sets an optional request identifier for tracking purposes.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets the total number of entities to validate.
    /// </summary>
    public int TotalEntityCount => MatterIds.Count() + DocumentIds.Count() + RevisionIds.Count();

    /// <summary>
    /// Validates the request and returns any validation errors.
    /// </summary>
    /// <returns>A collection of validation error messages.</returns>
    public IEnumerable<string> ValidateRequest()
    {
        var errors = new List<string>();

        if (TotalEntityCount == 0)
            errors.Add("At least one entity ID must be provided for validation");

        if (TotalEntityCount > 10000)
            errors.Add("Total entity count exceeds maximum allowed limit of 10,000");

        if (TimeoutMilliseconds <= 0)
            errors.Add("TimeoutMilliseconds must be greater than zero");

        if (TimeoutMilliseconds > 300000) // 5 minutes
            errors.Add("TimeoutMilliseconds should not exceed 300,000 (5 minutes)");

        return errors;
    }
}

/// <summary>
/// Represents batch validation performance metrics.
/// </summary>
public sealed class BatchValidationMetrics
{
    /// <summary>
    /// Gets the total number of batch operations performed.
    /// </summary>
    public required int TotalBatchOperations { get; init; }

    /// <summary>
    /// Gets the average batch size across all operations.
    /// </summary>
    public required double AverageBatchSize { get; init; }

    /// <summary>
    /// Gets the average processing time for batch operations.
    /// </summary>
    public required TimeSpan AverageBatchProcessingTime { get; init; }

    /// <summary>
    /// Gets the success rate for batch operations (0.0 to 1.0).
    /// </summary>
    public required double BatchSuccessRate { get; init; }

    /// <summary>
    /// Gets the total number of entities processed across all batch operations.
    /// </summary>
    public required long TotalEntitiesProcessed { get; init; }

    /// <summary>
    /// Gets the average number of entities processed per second.
    /// </summary>
    public required double EntitiesPerSecond { get; init; }

    /// <summary>
    /// Gets the distribution of batch sizes.
    /// </summary>
    public required IReadOnlyDictionary<string, int> BatchSizeDistribution { get; init; }

    /// <summary>
    /// Gets performance metrics broken down by entity type.
    /// </summary>
    public required IReadOnlyDictionary<string, EntityTypeMetrics> EntityTypeBreakdown { get; init; }

    /// <summary>
    /// Gets additional operational metrics.
    /// </summary>
    public required IReadOnlyDictionary<string, object> AdditionalMetrics { get; init; }

    /// <summary>
    /// Creates a basic metrics instance for error scenarios.
    /// </summary>
    public static BatchValidationMetrics Create(int totalOps, double avgSize, TimeSpan avgTime, double successRate)
    {
        return new BatchValidationMetrics
        {
            TotalBatchOperations = totalOps,
            AverageBatchSize = avgSize,
            AverageBatchProcessingTime = avgTime,
            BatchSuccessRate = successRate,
            TotalEntitiesProcessed = 0,
            EntitiesPerSecond = 0.0,
            BatchSizeDistribution = new Dictionary<string, int>().AsReadOnly(),
            EntityTypeBreakdown = new Dictionary<string, EntityTypeMetrics>().AsReadOnly(),
            AdditionalMetrics = new Dictionary<string, object>().AsReadOnly()
        };
    }
}

/// <summary>
/// Represents performance metrics for a specific entity type.
/// </summary>
public sealed record EntityTypeMetrics
{
    public required string EntityType { get; init; }
    public required int TotalProcessed { get; init; }
    public required TimeSpan AverageProcessingTime { get; init; }
    public required double SuccessRate { get; init; }
    public required double CacheHitRate { get; init; }
    public required double EntitiesPerSecond { get; init; }
}

/// <summary>
/// Represents the result of a batch validation health check.
/// </summary>
public sealed class BatchValidationHealthResult
{
    /// <summary>
    /// Gets whether the service is healthy.
    /// </summary>
    public required bool IsHealthy { get; init; }

    /// <summary>
    /// Gets the health score (0.0 to 1.0).
    /// </summary>
    public required double HealthScore { get; init; }

    /// <summary>
    /// Gets the collection of health issues detected.
    /// </summary>
    public required ImmutableList<string> Issues { get; init; }

    /// <summary>
    /// Gets the timestamp of the last successful operation.
    /// </summary>
    public DateTime? LastSuccessfulOperation { get; init; }

    /// <summary>
    /// Gets additional health check metadata.
    /// </summary>
    public required IReadOnlyDictionary<string, object> Metadata { get; init; }

    /// <summary>
    /// Gets a readable health status description.
    /// </summary>
    public string GetHealthStatus()
    {
        return IsHealthy ? "Healthy" : "Unhealthy";
    }
}

#endregion Supporting Types and Classes