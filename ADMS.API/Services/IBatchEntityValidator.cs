using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

using ADMS.API.Services.Common;

namespace ADMS.API.Services;

/// <summary>
/// Defines the contract for enterprise-grade batch entity validation services providing efficient multi-entity verification.
/// </summary>
/// <remarks>
/// <para>This interface provides specialized batch validation capabilities including:</para>
/// 
/// <para><strong>Core Batch Validation Features:</strong></para>
/// <list type="bullet">
/// <item>High-performance batch entity existence checking across multiple types</item>
/// <item>Single database round-trip optimization for multiple entity validation</item>
/// <item>Parallel processing of different entity type validations</item>
/// <item>Comprehensive result reporting with detailed entity breakdown</item>
/// <item>Memory-efficient processing of large entity collections</item>
/// </list>
/// 
/// <para><strong>Performance Optimization:</strong></para>
/// <list type="bullet">
/// <item>Optimized database queries using IN clauses and parallel execution</item>
/// <item>Intelligent query batching to minimize database round-trips</item>
/// <item>Support for large-scale entity validation operations</item>
/// <item>Configurable batch size limits for memory management</item>
/// <item>Integration with caching strategies for repeated validations</item>
/// </list>
/// 
/// <para><strong>Business Rule Integration:</strong></para>
/// <list type="bullet">
/// <item>Support for complex multi-entity business rule validation</item>
/// <item>Atomic validation of related entity groups</item>
/// <item>Cross-entity dependency validation</item>
/// <item>Workflow-based validation scenarios</item>
/// <item>Data integrity enforcement across entity boundaries</item>
/// </list>
/// 
/// <para><strong>Scalability and Reliability:</strong></para>
/// <list type="bullet">
/// <item>Horizontal scaling support for high-volume validation</item>
/// <item>Circuit breaker patterns for resilience</item>
/// <item>Retry mechanisms for transient failures</item>
/// <item>Load balancing across multiple validation instances</item>
/// <item>Comprehensive error handling and recovery</item>
/// </list>
/// 
/// <para><strong>Monitoring and Observability:</strong></para>
/// <list type="bullet">
/// <item>Real-time performance metrics and monitoring</item>
/// <item>Detailed operation tracking and analytics</item>
/// <item>Health check integration for operational monitoring</item>
/// <item>Alert threshold support for proactive monitoring</item>
/// <item>Integration with application performance monitoring tools</item>
/// </list>
/// 
/// <para><strong>Integration Scenarios:</strong></para>
/// <list type="bullet">
/// <item>Bulk data import and validation workflows</item>
/// <item>Multi-entity API operations requiring validation</item>
/// <item>Workflow orchestration with entity dependencies</item>
/// <item>Data migration and synchronization processes</item>
/// <item>Batch processing and ETL operations</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Service registration and configuration
/// services.Configure&lt;BatchValidationOptions&gt;(configuration.GetSection("BatchValidation"));
/// services.AddScoped&lt;IBatchEntityValidator, BatchEntityValidator&gt;();
/// 
/// // Usage in document service for bulk operations
/// public class DocumentService
/// {
///     private readonly IBatchEntityValidator _batchValidator;
///     
///     public async Task&lt;BulkOperationResult&gt; CreateMultipleDocumentsAsync(
///         IEnumerable&lt;DocumentCreationRequest&gt; requests,
///         CancellationToken cancellationToken = default)
///     {
///         var matterIds = requests.Select(r => r.MatterId).Distinct();
///         
///         var validationResult = await _batchValidator.ValidateEntitiesExistAsync(
///             matterIds, Array.Empty&lt;Guid&gt;(), Array.Empty&lt;Guid&gt;(), cancellationToken);
///             
///         if (!validationResult.AllEntitiesExist)
///         {
///             return BulkOperationResult.Failed(
///                 $"Invalid matters: {validationResult.GetMissingEntitiesDescription()}");
///         }
///         
///         return await ProcessBulkDocumentCreation(requests, cancellationToken);
///     }
/// }
/// </code>
/// </example>
public interface IBatchEntityValidator
{
    #region Core Batch Validation Methods

    /// <summary>
    /// Asynchronously validates the existence of multiple entities across different types in a single operation.
    /// </summary>
    /// <param name="matterIds">
    /// A collection of matter IDs to validate for existence.
    /// Can be empty if no matter validation is required.
    /// All IDs must be valid GUIDs representing matter entities.
    /// Supports up to the configured maximum batch size.
    /// </param>
    /// <param name="documentIds">
    /// A collection of document IDs to validate for existence.
    /// Can be empty if no document validation is required.
    /// All IDs must be valid GUIDs representing document entities.
    /// Supports up to the configured maximum batch size.
    /// </param>
    /// <param name="revisionIds">
    /// A collection of revision IDs to validate for existence.
    /// Can be empty if no revision validation is required.
    /// All IDs must be valid GUIDs representing revision entities.
    /// Supports up to the configured maximum batch size.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to allow cancellation of the batch validation operation.
    /// Implementations should respect cancellation requests for responsive operations.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous batch validation operation.
    /// The task result contains a <see cref="BatchEntityValidationResult"/> with comprehensive validation information including:
    /// - Overall validation status indicating if all entities exist
    /// - Detailed breakdown of existing vs missing entities by type
    /// - Performance metrics and operation metadata
    /// - Human-readable descriptions for error reporting
    /// </returns>
    /// <remarks>
    /// <para>This method provides efficient batch validation including:</para>
    /// 
    /// <para><strong>Performance Optimization:</strong></para>
    /// <list type="bullet">
    /// <item>Single database round-trip for multiple entity validation</item>
    /// <item>Parallel processing of different entity types when possible</item>
    /// <item>Optimized queries using IN clauses for efficient database operations</item>
    /// <item>Intelligent query batching based on database capabilities</item>
    /// <item>Memory-efficient processing of large entity collections</item>
    /// </list>
    /// 
    /// <para><strong>Validation Features:</strong></para>
    /// <list type="bullet">
    /// <item>Comprehensive existence checking across all supported entity types</item>
    /// <item>Detailed results indicating which specific entities exist or are missing</item>
    /// <item>Support for soft-delete filtering and entity state validation</item>
    /// <item>Integration with caching strategies for improved performance</item>
    /// <item>Atomic validation ensuring consistent results across all entities</item>
    /// </list>
    /// 
    /// <para><strong>Security and Access Control:</strong></para>
    /// <list type="bullet">
    /// <item>Integration with security context for entity access validation</item>
    /// <item>Support for role-based entity visibility filtering</item>
    /// <item>Audit logging for batch validation activities</item>
    /// <item>Prevention of information disclosure through validation responses</item>
    /// <item>Multi-tenant support with proper data isolation</item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any entity ID collection is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when any entity ID collection contains invalid GUIDs (<see cref="Guid.Empty"/>)
    /// or when the total number of entities exceeds the configured maximum batch size.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the batch validator is not properly configured or when database
    /// connectivity issues prevent the validation operation from completing.
    /// </exception>
    /// <example>
    /// <code>
    /// // Basic batch validation
    /// var matterIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
    /// var documentIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    /// var revisionIds = new[] { Guid.NewGuid() };
    /// 
    /// var result = await _batchValidator.ValidateEntitiesExistAsync(
    ///     matterIds, documentIds, revisionIds);
    ///     
    /// if (!result.AllEntitiesExist)
    /// {
    ///     Console.WriteLine($"Missing entities: {result.GetMissingEntitiesDescription()}");
    ///     Console.WriteLine($"Total missing: {result.GetTotalMissingCount()}");
    /// }
    /// 
    /// // Integration with bulk operations
    /// public async Task&lt;BulkProcessingResult&gt; ProcessEntitiesAsync(
    ///     IEnumerable&lt;EntityProcessingRequest&gt; requests)
    /// {
    ///     var matterIds = requests.Select(r => r.MatterId).Distinct();
    ///     var documentIds = requests.Select(r => r.DocumentId).Distinct().Where(id => id != Guid.Empty);
    ///     
    ///     var validation = await _batchValidator.ValidateEntitiesExistAsync(
    ///         matterIds, documentIds, Array.Empty&lt;Guid&gt;());
    ///         
    ///     if (!validation.AllEntitiesExist)
    ///     {
    ///         return BulkProcessingResult.ValidationFailed(validation);
    ///     }
    ///     
    ///     return await ProcessValidatedEntities(requests);
    /// }
    /// </code>
    /// </example>
    Task<BatchEntityValidationResult> ValidateEntitiesExistAsync(
        IEnumerable<Guid> matterIds,
        IEnumerable<Guid> documentIds,
        IEnumerable<Guid> revisionIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously validates entity relationships in batch operations.
    /// </summary>
    /// <param name="documentMatterPairs">
    /// A collection of document-matter ID pairs to validate for proper relationships.
    /// Each tuple represents a (MatterId, DocumentId) relationship that should exist.
    /// Can be empty if no document-matter relationship validation is required.
    /// All IDs must be valid GUIDs representing existing entities.
    /// </param>
    /// <param name="revisionDocumentPairs">
    /// A collection of revision-document ID pairs to validate for proper relationships.
    /// Each tuple represents a (DocumentId, RevisionId) relationship that should exist.
    /// Can be empty if no revision-document relationship validation is required.
    /// All IDs must be valid GUIDs representing existing entities.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to allow cancellation of the batch relationship validation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous batch relationship validation operation.
    /// The task result contains a <see cref="BatchRelationshipValidationResult"/> with comprehensive information including:
    /// - Overall validation status indicating if all relationships are valid
    /// - Detailed breakdown of valid vs invalid relationships by type
    /// - Performance metrics and operation metadata
    /// - Human-readable descriptions for error reporting and troubleshooting
    /// </returns>
    /// <remarks>
    /// <para>This method provides comprehensive relationship validation including:</para>
    /// 
    /// <para><strong>Relationship Validation Features:</strong></para>
    /// <list type="bullet">
    /// <item>Document-matter ownership validation ensuring proper entity hierarchy</item>
    /// <item>Revision-document relationship validation for version control integrity</item>
    /// <item>Batch processing for efficient relationship verification</item>
    /// <item>Cross-entity reference integrity validation</item>
    /// <item>Support for complex relationship dependency validation</item>
    /// </list>
    /// 
    /// <para><strong>Performance and Efficiency:</strong></para>
    /// <list type="bullet">
    /// <item>Optimized database queries using JOIN operations for relationship checking</item>
    /// <item>Parallel processing of different relationship types when possible</item>
    /// <item>Single database round-trip for multiple relationship validation</item>
    /// <item>Intelligent query optimization based on relationship complexity</item>
    /// <item>Memory-efficient processing of large relationship collections</item>
    /// </list>
    /// 
    /// <para><strong>Data Integrity and Consistency:</strong></para>
    /// <list type="bullet">
    /// <item>Referential integrity validation across related entities</item>
    /// <item>Support for soft-delete filtering in relationship validation</item>
    /// <item>Temporal consistency checking for entity lifecycle relationships</item>
    /// <item>Transaction isolation support for consistent relationship state</item>
    /// <item>Integration with entity existence validation for comprehensive checking</item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any relationship collection is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when relationship pairs contain invalid GUIDs or when the total number
    /// of relationships exceeds the configured maximum batch size.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the batch validator is not properly configured for relationship validation.
    /// </exception>
    /// <example>
    /// <code>
    /// // Document-matter relationship validation
    /// var documentMatterPairs = new[]
    /// {
    ///     (matterId1, documentId1),
    ///     (matterId1, documentId2),
    ///     (matterId2, documentId3)
    /// };
    /// 
    /// var result = await _batchValidator.ValidateRelationshipsAsync(
    ///     documentMatterPairs, Array.Empty&lt;(Guid, Guid)&gt;());
    ///     
    /// if (!result.AllRelationshipsValid)
    /// {
    ///     Console.WriteLine($"Invalid relationships: {result.GetInvalidRelationshipsDescription()}");
    ///     
    ///     foreach (var invalidPair in result.InvalidDocumentMatterPairs)
    ///     {
    ///         _logger.LogWarning("Document {DocumentId} does not belong to Matter {MatterId}",
    ///             invalidPair.DocumentId, invalidPair.MatterId);
    ///     }
    /// }
    /// 
    /// // Integration with bulk operations
    /// public async Task&lt;BulkUpdateResult&gt; UpdateDocumentHierarchyAsync(
    ///     IEnumerable&lt;HierarchyUpdateRequest&gt; updates)
    /// {
    ///     var docMatterPairs = updates.Select(u => (u.MatterId, u.DocumentId)).Distinct();
    ///     var revDocPairs = updates.Select(u => (u.DocumentId, u.RevisionId)).Distinct();
    ///     
    ///     var relationshipValidation = await _batchValidator.ValidateRelationshipsAsync(
    ///         docMatterPairs, revDocPairs);
    ///         
    ///     if (!relationshipValidation.AllRelationshipsValid)
    ///     {
    ///         return BulkUpdateResult.InvalidRelationships(relationshipValidation);
    ///     }
    ///     
    ///     return await ProcessHierarchyUpdates(updates);
    /// }
    /// </code>
    /// </example>
    Task<BatchRelationshipValidationResult> ValidateRelationshipsAsync(
        IEnumerable<(Guid MatterId, Guid DocumentId)> documentMatterPairs,
        IEnumerable<(Guid DocumentId, Guid RevisionId)> revisionDocumentPairs,
        CancellationToken cancellationToken = default);

    #endregion Core Batch Validation Methods

    #region Advanced Batch Validation Methods

    /// <summary>
    /// Asynchronously validates entities and their relationships in a single comprehensive operation.
    /// </summary>
    /// <param name="request">
    /// A comprehensive validation request containing entity IDs and relationship pairs to validate.
    /// Includes configuration options for the validation operation and metadata for tracking.
    /// Must not be <c>null</c> and should contain valid entity IDs and relationship pairs.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to allow cancellation of the comprehensive validation operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous comprehensive validation operation.
    /// The task result contains a <see cref="ComprehensiveValidationResult"/> combining both entity and relationship validation.
    /// </returns>
    /// <remarks>
    /// <para>This method provides a comprehensive validation approach that combines:</para>
    /// 
    /// <para><strong>Integrated Validation Features:</strong></para>
    /// <list type="bullet">
    /// <item>Entity existence validation across all types in a single operation</item>
    /// <item>Relationship validation for complex hierarchies</item>
    /// <item>Performance optimization through single-operation processing</item>
    /// <item>Detailed result breakdown for comprehensive error reporting</item>
    /// <item>Atomic validation ensuring consistency across all validations</item>
    /// </list>
    /// 
    /// <para><strong>Performance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Reduced database round-trips through combined validation</item>
    /// <item>Optimized query execution for related validations</item>
    /// <item>Memory-efficient processing of complex validation scenarios</item>
    /// <item>Parallel processing where possible for better throughput</item>
    /// </list>
    /// 
    /// <para><strong>Use Cases:</strong></para>
    /// <list type="bullet">
    /// <item>Complex business operations requiring full entity hierarchy validation</item>
    /// <item>Data import processes with comprehensive validation requirements</item>
    /// <item>Workflow operations with multiple entity dependencies</item>
    /// <item>API operations requiring complete validation before processing</item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the request contains invalid data or exceeds validation limits.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the service is not properly configured for comprehensive validation.
    /// </exception>
    /// <example>
    /// <code>
    /// var request = new ComprehensiveValidationRequest
    /// {
    ///     MatterIds = new[] { matterId1, matterId2 },
    ///     DocumentIds = new[] { docId1, docId2, docId3 },
    ///     RevisionIds = new[] { revId1, revId2 },
    ///     DocumentMatterPairs = new[] { (matterId1, docId1), (matterId2, docId2) },
    ///     RevisionDocumentPairs = new[] { (docId1, revId1), (docId2, revId2) },
    ///     ValidateRelationships = true,
    ///     IncludeMetadata = true
    /// };
    /// 
    /// var result = await _batchValidator.ValidateComprehensiveAsync(request);
    /// 
    /// if (!result.IsValid)
    /// {
    ///     Console.WriteLine($"Validation failed: {result.GetValidationSummary()}");
    ///     return BadRequest(result);
    /// }
    /// 
    /// // Continue with business operation
    /// return await ProcessValidatedEntities(request);
    /// </code>
    /// </example>
    Task<ComprehensiveValidationResult> ValidateComprehensiveAsync(
        ComprehensiveValidationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously validates file name uniqueness across multiple matters in batch.
    /// </summary>
    /// <param name="fileNameValidationRequests">
    /// A collection of file name validation requests, each containing a matter ID and file name to check.
    /// Used for bulk validation of file name conflicts across different matters.
    /// Must not be <c>null</c> and should contain valid file name validation requests.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token for the batch file name validation operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous batch file name validation operation.
    /// The task result contains a <see cref="BatchFileNameValidationResult"/> with detailed information 
    /// about file name conflicts and availability.
    /// </returns>
    /// <remarks>
    /// <para>This method provides efficient batch file name validation including:</para>
    /// 
    /// <para><strong>Conflict Detection Features:</strong></para>
    /// <list type="bullet">
    /// <item>Bulk file name conflict detection across multiple matters</item>
    /// <item>Optimized database queries for file name uniqueness checking</item>
    /// <item>Support for case-sensitive and case-insensitive file name comparison</item>
    /// <item>Integration with matter-scoped file naming policies</item>
    /// <item>Detection of various conflict types (exact, normalized, reserved names)</item>
    /// </list>
    /// 
    /// <para><strong>Performance Optimization:</strong></para>
    /// <list type="bullet">
    /// <item>Batch processing for reduced database overhead</item>
    /// <item>Intelligent query optimization for file name lookups</item>
    /// <item>Memory-efficient processing of large file name collections</item>
    /// <item>Parallel validation across different matters when possible</item>
    /// </list>
    /// 
    /// <para><strong>Business Rule Support:</strong></para>
    /// <list type="bullet">
    /// <item>Matter-scoped file naming policy enforcement</item>
    /// <item>Reserved file name detection and prevention</item>
    /// <item>File name normalization and conflict resolution</item>
    /// <item>Integration with document management workflows</item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="fileNameValidationRequests"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the collection contains invalid requests or exceeds batch size limits.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the service is not properly configured for file name validation.
    /// </exception>
    /// <example>
    /// <code>
    /// var requests = new[]
    /// {
    ///     new FileNameValidationRequest { MatterId = matterId1, FileName = "document1.pdf" },
    ///     new FileNameValidationRequest { MatterId = matterId1, FileName = "document2.docx" },
    ///     new FileNameValidationRequest { MatterId = matterId2, FileName = "contract.pdf" }
    /// };
    /// 
    /// var result = await _batchValidator.ValidateFileNamesAsync(requests);
    /// 
    /// if (result.HasConflicts)
    /// {
    ///     foreach (var conflict in result.ConflictingFileNames)
    ///     {
    ///         Console.WriteLine($"File name '{conflict.FileName}' conflicts in matter {conflict.MatterId}: {conflict.ConflictType}");
    ///     }
    ///     
    ///     return BadRequest(new { 
    ///         Message = "File name conflicts detected", 
    ///         Conflicts = result.ConflictingFileNames,
    ///         Summary = result.GetValidationSummary()
    ///     });
    /// }
    /// 
    /// // Continue with file operations
    /// return Ok(result);
    /// </code>
    /// </example>
    Task<BatchFileNameValidationResult> ValidateFileNamesAsync(
        IEnumerable<FileNameValidationRequest> fileNameValidationRequests,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously validates a bulk validation request with comprehensive entity and relationship checking.
    /// </summary>
    /// <param name="request">
    /// A bulk validation request containing the entities to validate and validation options.
    /// Must not be <c>null</c> and should pass request validation.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token for the bulk validation operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous bulk validation operation.
    /// The task result contains a <see cref="BatchEntityValidationResult"/> with validation details.
    /// </returns>
    /// <remarks>
    /// <para>This method provides a structured approach to bulk validation using a request object:</para>
    /// 
    /// <para><strong>Request-Based Validation:</strong></para>
    /// <list type="bullet">
    /// <item>Structured request object for complex validation scenarios</item>
    /// <item>Built-in request validation and parameter checking</item>
    /// <item>Support for additional validation options and metadata</item>
    /// <item>Timeout configuration and operation tracking</item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the request fails validation or contains invalid parameters.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var bulkRequest = new BulkValidationRequest
    /// {
    ///     MatterIds = matterIds,
    ///     DocumentIds = documentIds,
    ///     RevisionIds = revisionIds,
    ///     ValidateRelationships = false,
    ///     IncludeMetadata = true,
    ///     TimeoutMilliseconds = 30000
    /// };
    /// 
    /// var validationErrors = bulkRequest.ValidateRequest();
    /// if (validationErrors.Any())
    /// {
    ///     return BadRequest(validationErrors);
    /// }
    /// 
    /// var result = await _batchValidator.ValidateBulkRequestAsync(bulkRequest);
    /// return Ok(result);
    /// </code>
    /// </example>
    Task<BatchEntityValidationResult> ValidateBulkRequestAsync(
        BulkValidationRequest request,
        CancellationToken cancellationToken = default);

    #endregion Advanced Batch Validation Methods

    #region Performance and Monitoring

    /// <summary>
    /// Gets comprehensive performance metrics for batch validation operations.
    /// </summary>
    /// <returns>
    /// A <see cref="BatchValidationMetrics"/> object containing detailed performance information including:
    /// - Total number of batch operations performed
    /// - Average batch sizes and processing times
    /// - Success and failure rates for batch operations
    /// - Entity type distribution and processing statistics
    /// - Performance trends and optimization recommendations
    /// </returns>
    /// <remarks>
    /// <para>This method provides comprehensive performance insights including:</para>
    /// 
    /// <para><strong>Operational Metrics:</strong></para>
    /// <list type="bullet">
    /// <item>Total batch operations and entity processing counts</item>
    /// <item>Average processing times and throughput statistics</item>
    /// <item>Success and failure rates with trend analysis</item>
    /// <item>Resource utilization patterns and efficiency metrics</item>
    /// <item>Performance comparison between different batch sizes</item>
    /// </list>
    /// 
    /// <para><strong>Optimization Insights:</strong></para>
    /// <list type="bullet">
    /// <item>Identification of optimal batch sizes for performance</item>
    /// <item>Analysis of database query efficiency patterns</item>
    /// <item>Cache utilization rates for batch validation scenarios</item>
    /// <item>Resource bottleneck identification and recommendations</item>
    /// <item>Performance degradation detection and alerting</item>
    /// </list>
    /// 
    /// <para><strong>Monitoring Integration:</strong></para>
    /// <list type="bullet">
    /// <item>Real-time performance tracking and alerting</item>
    /// <item>Integration with application performance monitoring tools</item>
    /// <item>Health check data for operational monitoring</item>
    /// <item>Custom metric export for analytics and reporting</item>
    /// <item>Historical trend analysis for capacity planning</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var metrics = _batchValidator.GetPerformanceMetrics();
    /// 
    /// Console.WriteLine($"Total batch operations: {metrics.TotalBatchOperations}");
    /// Console.WriteLine($"Average batch size: {metrics.AverageBatchSize:F2}");
    /// Console.WriteLine($"Average processing time: {metrics.AverageBatchProcessingTime.TotalMilliseconds:F2}ms");
    /// Console.WriteLine($"Success rate: {metrics.BatchSuccessRate:P2}");
    /// Console.WriteLine($"Entities per second: {metrics.EntitiesPerSecond:F2}");
    /// Console.WriteLine($"Efficiency score: {metrics.GetEfficiencyScore():P2}");
    /// 
    /// // Performance monitoring integration
    /// if (metrics.BatchSuccessRate < 0.95)
    /// {
    ///     _logger.LogWarning("Batch validation success rate below threshold: {SuccessRate:P2}", 
    ///         metrics.BatchSuccessRate);
    /// }
    /// 
    /// // Entity type analysis
    /// foreach (var (entityType, typeMetrics) in metrics.EntityTypeBreakdown)
    /// {
    ///     _logger.LogInformation("{EntityType}: {TotalProcessed} processed, Success: {SuccessRate:P2}, " +
    ///         "Avg time: {AvgTime}ms, Cache hit: {CacheHitRate:P2}",
    ///         entityType, typeMetrics.TotalProcessed, typeMetrics.SuccessRate,
    ///         typeMetrics.AverageProcessingTime.TotalMilliseconds, typeMetrics.CacheHitRate);
    /// }
    /// </code>
    /// </example>
    BatchValidationMetrics GetPerformanceMetrics();

    /// <summary>
    /// Asynchronously performs a comprehensive health check of the batch validation service.
    /// </summary>
    /// <param name="cancellationToken">
    /// Optional cancellation token for the health check operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous health check operation.
    /// The task result contains a <see cref="BatchValidationHealthResult"/> with detailed health information 
    /// about the batch validation service.
    /// </returns>
    /// <remarks>
    /// <para>This method provides comprehensive health validation including:</para>
    /// 
    /// <para><strong>System Health Checks:</strong></para>
    /// <list type="bullet">
    /// <item>Database connectivity and performance validation</item>
    /// <item>Service configuration and dependency health</item>
    /// <item>Performance metrics and threshold validation</item>
    /// <item>Resource availability and capacity assessment</item>
    /// <item>Integration point health and connectivity</item>
    /// </list>
    /// 
    /// <para><strong>Performance Health:</strong></para>
    /// <list type="bullet">
    /// <item>Response time and throughput health indicators</item>
    /// <item>Error rate and success rate trend analysis</item>
    /// <item>Resource utilization and capacity health</item>
    /// <item>Cache performance and effectiveness metrics</item>
    /// </list>
    /// 
    /// <para><strong>Integration Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Compatible with ASP.NET Core health check framework</item>
    /// <item>Suitable for container orchestration health probes</item>
    /// <item>Integration with monitoring and alerting systems</item>
    /// <item>Support for custom health check extensions</item>
    /// </list>
    /// </remarks>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// // Basic health check
    /// var health = await _batchValidator.CheckHealthAsync();
    /// 
    /// Console.WriteLine($"Health Status: {health.GetHealthStatus()}");
    /// Console.WriteLine($"Health Score: {health.HealthScore:P2}");
    /// Console.WriteLine($"Is Healthy: {health.IsHealthy}");
    /// 
    /// if (!health.IsHealthy)
    /// {
    ///     Console.WriteLine($"Health issues: {string.Join(", ", health.Issues)}");
    ///     _logger.LogWarning("Batch validator health issues detected: {Issues}", health.Issues);
    /// }
    /// 
    /// if (health.LastSuccessfulOperation.HasValue)
    /// {
    ///     var timeSinceLastSuccess = DateTimeOffset.UtcNow - health.LastSuccessfulOperation.Value;
    ///     if (timeSinceLastSuccess > TimeSpan.FromMinutes(10))
    ///     {
    ///         _logger.LogWarning("No successful operations for {Duration}", timeSinceLastSuccess);
    ///     }
    /// }
    /// 
    /// // Health check integration with ASP.NET Core
    /// public void ConfigureServices(IServiceCollection services)
    /// {
    ///     services.AddHealthChecks()
    ///         .AddCheck&lt;BatchValidatorHealthCheck&gt;("batch-validator");
    /// }
    /// </code>
    /// </example>
    Task<BatchValidationHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets real-time performance statistics for the batch validation service.
    /// </summary>
    /// <returns>
    /// A dictionary containing real-time performance statistics including current throughput,
    /// active operation count, recent error rates, and resource utilization metrics.
    /// </returns>
    /// <remarks>
    /// <para>This method provides real-time operational insights for:</para>
    /// 
    /// <para><strong>Real-Time Metrics:</strong></para>
    /// <list type="bullet">
    /// <item>Current active operations and queue depths</item>
    /// <item>Recent throughput and response time statistics</item>
    /// <item>Current error rates and success percentages</item>
    /// <item>Resource utilization and capacity indicators</item>
    /// </list>
    /// 
    /// <para><strong>Operational Visibility:</strong></para>
    /// <list type="bullet">
    /// <item>Live monitoring dashboard integration</item>
    /// <item>Real-time alerting and threshold monitoring</item>
    /// <item>Performance trending and anomaly detection</item>
    /// <item>Capacity planning and scaling indicators</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = _batchValidator.GetRealTimeStatistics();
    /// 
    /// if (stats.TryGetValue("CurrentThroughput", out var throughput))
    /// {
    ///     Console.WriteLine($"Current throughput: {throughput} ops/sec");
    /// }
    /// 
    /// if (stats.TryGetValue("ActiveOperations", out var activeOps))
    /// {
    ///     Console.WriteLine($"Active operations: {activeOps}");
    /// }
    /// 
    /// if (stats.TryGetValue("RecentErrorRate", out var errorRate))
    /// {
    ///     Console.WriteLine($"Recent error rate: {errorRate:P2}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyDictionary<string, object> GetRealTimeStatistics();

    #endregion Performance and Monitoring

    #region Configuration and Management

    /// <summary>
    /// Gets the current configuration settings for the batch validation service.
    /// </summary>
    /// <returns>
    /// A <see cref="BatchValidationOptions"/> object containing current configuration settings including
    /// batch size limits, timeout values, performance thresholds, and operational parameters.
    /// </returns>
    /// <remarks>
    /// <para>This method provides access to current service configuration for:</para>
    /// 
    /// <para><strong>Configuration Access:</strong></para>
    /// <list type="bullet">
    /// <item>Runtime configuration validation and verification</item>
    /// <item>Dynamic configuration adjustment recommendations</item>
    /// <item>Integration with configuration management systems</item>
    /// <item>Operational visibility into service behavior</item>
    /// <item>Troubleshooting and diagnostic information</item>
    /// </list>
    /// 
    /// <para><strong>Operational Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Configuration drift detection and monitoring</item>
    /// <item>Performance tuning and optimization guidance</item>
    /// <item>Compliance and governance reporting</item>
    /// <item>Change management and audit trails</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var config = _batchValidator.GetConfiguration();
    /// 
    /// Console.WriteLine($"Max batch size: {config.MaxBatchSize}");
    /// Console.WriteLine($"Timeout: {config.BatchTimeoutSeconds}s");
    /// Console.WriteLine($"Parallel processing: {config.EnableParallelProcessing}");
    /// Console.WriteLine($"Max parallelism: {config.MaxDegreeOfParallelism}");
    /// Console.WriteLine($"Caching enabled: {config.EnableCaching}");
    /// Console.WriteLine($"Cache expiration: {config.CacheExpirationMinutes}min");
    /// 
    /// // Validate configuration
    /// var configErrors = config.Validate();
    /// if (configErrors.Any())
    /// {
    ///     _logger.LogWarning("Configuration issues detected: {Issues}", configErrors);
    /// }
    /// 
    /// // Performance optimization recommendations
    /// if (config.MaxBatchSize > 5000)
    /// {
    ///     _logger.LogInformation("Consider reducing batch size for better memory utilization");
    /// }
    /// </code>
    /// </example>
    BatchValidationOptions GetConfiguration();

    /// <summary>
    /// Clears any cached validation results and performance metrics to reset service state.
    /// </summary>
    /// <param name="cacheType">
    /// Optional parameter to specify which cache type to clear.
    /// Supported values: "ValidationResults", "PerformanceMetrics", "All".
    /// If <c>null</c> or "All", clears all cached data.
    /// </param>
    /// <remarks>
    /// <para>This method is useful for:</para>
    /// 
    /// <para><strong>Operational Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Memory optimization in long-running applications</item>
    /// <item>Testing scenarios requiring clean service state</item>
    /// <item>Performance benchmark preparation</item>
    /// <item>Cache invalidation after configuration changes</item>
    /// <item>Troubleshooting scenarios requiring fresh state</item>
    /// </list>
    /// 
    /// <para><strong>Cache Management:</strong></para>
    /// <list type="bullet">
    /// <item>Selective cache clearing by type</item>
    /// <item>Complete cache reset when needed</item>
    /// <item>Memory pressure relief mechanisms</item>
    /// <item>Development and testing support</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Clear all caches for memory optimization
    /// _batchValidator.ClearCache();
    /// 
    /// // Clear only validation result cache
    /// _batchValidator.ClearCache("ValidationResults");
    /// 
    /// // Clear only performance metrics
    /// _batchValidator.ClearCache("PerformanceMetrics");
    /// 
    /// // Verify cache is cleared
    /// var metrics = _batchValidator.GetPerformanceMetrics();
    /// Console.WriteLine($"Operations after clear: {metrics.TotalBatchOperations}");
    /// 
    /// // Memory management scenario
    /// var beforeMemory = GC.GetTotalMemory(false);
    /// _batchValidator.ClearCache();
    /// GC.Collect();
    /// var afterMemory = GC.GetTotalMemory(true);
    /// Console.WriteLine($"Memory freed: {(beforeMemory - afterMemory) / 1024 / 1024:F2} MB");
    /// </code>
    /// </example>
    void ClearCache(string? cacheType = null);

    /// <summary>
    /// Validates the service configuration and returns any configuration issues.
    /// </summary>
    /// <returns>
    /// A collection of configuration validation messages. Empty if configuration is valid.
    /// </returns>
    /// <remarks>
    /// <para>This method provides configuration validation including:</para>
    /// 
    /// <para><strong>Configuration Checks:</strong></para>
    /// <list type="bullet">
    /// <item>Parameter range and format validation</item>
    /// <item>Dependency and integration point validation</item>
    /// <item>Performance threshold and limit validation</item>
    /// <item>Security and access control configuration</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var configIssues = _batchValidator.ValidateConfiguration();
    /// if (configIssues.Any())
    /// {
    ///     _logger.LogWarning("Configuration issues detected: {Issues}", 
    ///         string.Join("; ", configIssues));
    /// }
    /// </code>
    /// </example>
    IEnumerable<string> ValidateConfiguration();

    #endregion Configuration and Management
}