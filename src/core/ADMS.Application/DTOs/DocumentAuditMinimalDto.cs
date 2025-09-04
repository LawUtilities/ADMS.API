using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object for document audit information, aggregating document metadata with complete activity audit trails.
/// </summary>
/// <remarks>
/// This DTO serves as the primary audit trail representation for documents within the ADMS legal document management system,
/// providing complete audit information including document metadata and all associated activity audit trails. It aggregates
/// audit data from multiple sources to provide a unified view of document lifecycle activities for legal compliance,
/// professional accountability, and comprehensive audit reporting.
/// 
/// <para><strong>Enhanced with Standardized Validation (.NET 9):</strong></para>
/// <list type="bullet">
/// <item><strong>BaseValidationDto Integration:</strong> Inherits standardized ADMS validation patterns</item>
/// <item><strong>Comprehensive Audit Validation:</strong> Specialized validation for complex audit aggregation scenarios</item>
/// <item><strong>Performance Optimized:</strong> Uses yield return for lazy validation evaluation</item>
/// <item><strong>Validation Hierarchy:</strong> Follows standardized core → business → cross-property → collections pattern</item>
/// <item><strong>Collection Validation:</strong> Advanced validation for multiple audit trail collections</item>
/// </list>
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Comprehensive Audit Aggregation:</strong> Combines document metadata with complete activity audit trails</item>
/// <item><strong>Standardized Validation:</strong> Uses BaseValidationDto for consistent validation patterns</item>
/// <item><strong>Multi-Source Integration:</strong> Integrates document activities and matter-document transfer activities</item>
/// <item><strong>Professional Validation:</strong> Uses centralized validation helpers for comprehensive data integrity</item>
/// <item><strong>Legal Compliance Support:</strong> Provides complete audit trails for regulatory and legal requirements</item>
/// </list>
/// 
/// <para><strong>Validation Hierarchy:</strong></para>
/// Following BaseValidationDto standardized validation pattern:
/// <list type="number">
/// <item><strong>Core Properties:</strong> Document validation using nested DTO validation</item>
/// <item><strong>Business Rules:</strong> Audit trail integrity, activity sequencing, professional standards</item>
/// <item><strong>Cross-Property:</strong> Document-activity consistency, cross-reference validation</item>
/// <item><strong>Collections:</strong> Deep validation of all audit trail collections with detailed error contexts</item>
/// </list>
/// 
/// <para><strong>Performance Benefits with Standardized Validation:</strong></para>
/// <list type="bullet">
/// <item><strong>Early Termination:</strong> Validation stops on critical errors for better performance</item>
/// <item><strong>Lazy Evaluation:</strong> Collections validated only when needed</item>
/// <item><strong>Consistent Error Handling:</strong> Standardized error formatting and reporting</item>
/// <item><strong>Memory Efficient:</strong> Optimized validation memory usage for large audit collections</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive document audit DTO with standardized validation
/// var documentAudit = new DocumentAuditMinimalDto
/// {
///     Document = new DocumentMinimalDto { /* document data */ },
///     DocumentActivityUserAudit = new List<DocumentActivityUserMinimalDto> { /* activities */ },
///     MatterDocumentActivityUserFromAudit = new List<MatterDocumentActivityUserMinimalDto> { /* from transfers */ },
///     MatterDocumentActivityUserToAudit = new List<MatterDocumentActivityUserMinimalDto> { /* to transfers */ }
/// };
/// 
/// // Standardized validation using BaseValidationDto
/// var validationResults = BaseValidationDto.ValidateModel(documentAudit);
/// if (BaseValidationDto.HasValidationErrors(validationResults))
/// {
///     var summary = BaseValidationDto.GetValidationSummary(validationResults);
///     _logger.LogWarning("Document audit validation failed: {ValidationSummary}", summary);
/// }
/// 
/// // Professional audit processing with validation
/// if (documentAudit.IsValid)
/// {
///     ProcessDocumentAudit(documentAudit);
/// }
/// </code>
/// </example>
public class DocumentAuditMinimalDto : BaseValidationDto
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the minimal document information with essential metadata.
    /// </summary>
    /// <remarks>
    /// This property provides the foundation document information for the audit trail, including
    /// essential metadata such as filename, size, checksum, and status flags.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCrossPropertyRules() for nested DTO validation and consistency.
    /// </remarks>
    [Required(ErrorMessage = "Document information is required for audit trail generation.")]
    public required DocumentMinimalDto Document { get; set; }

    /// <summary>
    /// Gets or sets the collection of direct document activity audit entries performed by users.
    /// </summary>
    /// <remarks>
    /// This collection contains audit entries for activities performed directly on the document,
    /// such as creation, saving, deletion, and restoration operations.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCollections() using comprehensive collection validation with detailed error contexts.
    /// </remarks>
    public IReadOnlyCollection<DocumentActivityUserMinimalDto> DocumentActivityUserAudit { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of source-side matter document transfer activity audit entries.
    /// </summary>
    /// <remarks>
    /// This collection contains audit entries for document transfer activities where this document
    /// was transferred FROM other matters to its current location.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCollections() using comprehensive collection validation with detailed error contexts.
    /// </remarks>
    public IReadOnlyCollection<MatterDocumentActivityUserMinimalDto> MatterDocumentActivityUserFromAudit { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of destination-side matter document transfer activity audit entries.
    /// </summary>
    /// <remarks>
    /// This collection contains audit entries for document transfer activities where this document
    /// was transferred TO its current or previous locations from other matters.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCollections() using comprehensive collection validation with detailed error contexts.
    /// </remarks>
    public IReadOnlyCollection<MatterDocumentActivityUserMinimalDto> MatterDocumentActivityUserToAudit { get; set; } = [];

    #endregion Core Properties

    #region Computed Properties

    /// <summary>
    /// Gets the total count of all audit activities across all activity types.
    /// </summary>
    public int TotalActivityCount =>
        DocumentActivityUserAudit.Count +
        MatterDocumentActivityUserFromAudit.Count +
        MatterDocumentActivityUserToAudit.Count;

    /// <summary>
    /// Gets a value indicating whether this document has any transfer history between matters.
    /// </summary>
    public bool HasTransferHistory =>
        MatterDocumentActivityUserFromAudit.Any() ||
        MatterDocumentActivityUserToAudit.Any();

    /// <summary>
    /// Gets the count of transfer activities (both from and to).
    /// </summary>
    public int TransferActivityCount =>
        MatterDocumentActivityUserFromAudit.Count +
        MatterDocumentActivityUserToAudit.Count;

    /// <summary>
    /// Gets a value indicating whether this document audit contains any activity data.
    /// </summary>
    public bool HasAnyActivity => TotalActivityCount > 0;

    /// <summary>
    /// Gets a value indicating whether this audit DTO is valid for system operations.
    /// </summary>
    public bool IsValid =>
        !(Document is IValidatableObject) ||
        !HasValidationErrors(Document.Validate(new ValidationContext(Document)));

    #endregion Computed Properties

    #region Standardized Validation Implementation

    /// <summary>
    /// Validates core properties such as the Document minimal DTO using ADMS validation helpers.
    /// </summary>
    /// <returns>A collection of validation results for core property validation.</returns>
    /// <remarks>
    /// This method implements the first step of the BaseValidationDto validation hierarchy,
    /// validating the essential document property that serves as the foundation for the audit trail.
    /// 
    /// <para><strong>Core Property Validation Steps:</strong></para>
    /// <list type="number">
    /// <item>Document DTO existence validation</item>
    /// <item>Document DTO validity validation using nested validation</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCoreProperties()
    {
        // Core validation: Document must exist

        // Additional basic validation for document completeness
        if (Document.Id == Guid.Empty)
        {
            yield return CreateValidationResult(
                "Document must have a valid ID for audit trail identification.",
                nameof(Document));
        }

        if (string.IsNullOrWhiteSpace(Document.FileName))
        {
            yield return CreateValidationResult(
                "Document must have a valid filename for audit trail context.",
                nameof(Document));
        }
    }

    /// <summary>
    /// Validates business rules specific to comprehensive document audit aggregation and professional standards.
    /// </summary>
    /// <returns>A collection of validation results for business rule validation.</returns>
    /// <remarks>
    /// This method implements the second step of the BaseValidationDto validation hierarchy,
    /// validating domain-specific business rules for comprehensive audit trail operations.
    /// 
    /// <para><strong>Business Rules Validated:</strong></para>
    /// <list type="bullet">
    /// <item>Audit trail completeness and integrity</item>
    /// <item>Transfer activity validation for professional standards</item>
    /// <item>Document activity sequence validation</item>
    /// <item>Professional standards for audit aggregation</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Business rule: If document has transfer activities, validate transfer types
        if (HasTransferHistory)
        {
            var invalidTransferTypes = MatterDocumentActivityUserFromAudit
                .Concat(MatterDocumentActivityUserToAudit)
                .Where(a => a?.MatterDocumentActivity?.Activity != null &&
                           !IsValidTransferActivity(a.MatterDocumentActivity.Activity))
                .ToList();

            if (invalidTransferTypes.Any())
            {
                yield return CreateValidationResult(
                    "Transfer activities must be of type MOVED or COPIED for audit trail integrity.",
                    nameof(MatterDocumentActivityUserFromAudit), nameof(MatterDocumentActivityUserToAudit));
            }
        }

        // Business rule: Document creation activity should be present if document exists
        if (!DocumentActivityUserAudit.Any())
        {
            var hasCreationActivity = DocumentActivityUserAudit
                .Any(a => a.DocumentActivity?.Activity?.ToUpperInvariant() == "CREATED");

            if (!hasCreationActivity)
            {
                yield return CreateValidationResult(
                    "Document audit trail should include creation activity for completeness.",
                    nameof(DocumentActivityUserAudit));
            }
        }

        // Business rule: Validate activity temporal consistency
        if (DocumentActivityUserAudit.Any())
        {
            var activities = DocumentActivityUserAudit.ToList();
            var firstActivity = activities.OrderBy(a => a.CreatedAt).FirstOrDefault();
            var lastActivity = activities.OrderByDescending(a => a.CreatedAt).FirstOrDefault();

            if (firstActivity != null && lastActivity != null)
            {
                var timeSpan = lastActivity.CreatedAt - firstActivity.CreatedAt;
                if (timeSpan.TotalDays > 3650) // More than 10 years
                {
                    yield return CreateValidationResult(
                        "Document activity timespan exceeds 10 years. Verify audit trail integrity for extended document lifecycle.",
                        nameof(DocumentActivityUserAudit));
                }
            }
        }

        // Business rule: Professional standards for audit completeness
        if (TotalActivityCount == 0)
        {
            yield return CreateValidationResult(
                "Document audit should contain at least one activity for professional audit trail standards.",
                nameof(DocumentActivityUserAudit), nameof(MatterDocumentActivityUserFromAudit), nameof(MatterDocumentActivityUserToAudit));
        }
    }

    /// <summary>
    /// Validates cross-property relationships and consistency including nested DTO validation.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    /// <remarks>
    /// This method implements the third step of the BaseValidationDto validation hierarchy,
    /// validating relationships between properties and nested DTO consistency.
    /// 
    /// <para><strong>Cross-Property Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Document DTO validation using nested validation</item>
    /// <item>Document ID consistency across all audit collections</item>
    /// <item>Temporal consistency between different activity types</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // Cross-property rule: Validate document DTO using nested validation
        // Validate the document DTO if it supports BaseValidationDto
        if (Document is IValidatableObject validatableDocument)
        {
            var documentContext = new ValidationContext(Document);
            var documentResults = validatableDocument.Validate(documentContext);
            foreach (var result in documentResults)
            {
                yield return CreateValidationResult(
                    $"Document validation failed: {result.ErrorMessage}",
                    $"{nameof(Document)}.{string.Join(",", result.MemberNames)}");
            }
        }

        // Cross-property rule: Document ID consistency across collections
        var documentId = Document.Id;

        // Validate document activity references
        var invalidDocumentActivities = DocumentActivityUserAudit
            .Where(a => a?.DocumentId != documentId)
            .ToList();

        if (invalidDocumentActivities.Any())
        {
            yield return CreateValidationResult(
                $"All document activities must reference the audit document (ID: {documentId}).",
                nameof(DocumentActivityUserAudit), nameof(Document));
        }

        // Validate matter document activity from references
        var invalidFromActivities = MatterDocumentActivityUserFromAudit
            .Where(a => a?.DocumentId != documentId)
            .ToList();

        if (invalidFromActivities.Any())
        {
            yield return CreateValidationResult(
                $"All matter document from activities must reference the audit document (ID: {documentId}).",
                nameof(MatterDocumentActivityUserFromAudit), nameof(Document));
        }

        // Validate matter document activity to references
        var invalidToActivities = MatterDocumentActivityUserToAudit
            .Where(a => a?.DocumentId != documentId)
            .ToList();

        if (invalidToActivities.Any())
        {
            yield return CreateValidationResult(
                $"All matter document to activities must reference the audit document (ID: {documentId}).",
                nameof(MatterDocumentActivityUserToAudit), nameof(Document));
        }

        // Cross-property rule: Temporal consistency validation
        if (!HasAnyActivity) yield break;
        var allActivities = new List<DateTime>();
        allActivities.AddRange(DocumentActivityUserAudit.Select(a => a.CreatedAt));
        allActivities.AddRange(MatterDocumentActivityUserFromAudit.Select(a => a.CreatedAt));
        allActivities.AddRange(MatterDocumentActivityUserToAudit.Select(a => a.CreatedAt));

        var orderedActivities = allActivities.OrderBy(d => d).ToList();
        if (!orderedActivities.Any()) yield break;
        var span = orderedActivities.Last() - orderedActivities.First();
        if (span.TotalDays > 3650) // More than 10 years
        {
            yield return CreateValidationResult(
                "Audit trail spans more than 10 years. Verify temporal consistency for professional standards.",
                nameof(DocumentActivityUserAudit), nameof(MatterDocumentActivityUserFromAudit), nameof(MatterDocumentActivityUserToAudit));
        }
    }

    /// <summary>
    /// Validates collections and nested objects using comprehensive collection validation.
    /// </summary>
    /// <returns>A collection of validation results for collection validation.</returns>
    /// <remarks>
    /// This method implements the fourth step of the BaseValidationDto validation hierarchy,
    /// validating all audit trail collections with detailed error reporting and context.
    /// 
    /// <para><strong>Collection Validation Features:</strong></para>
    /// <list type="bullet">
    /// <item>Deep validation of all collection items</item>
    /// <item>Detailed error context with collection item indices</item>
    /// <item>Null safety and collection integrity validation</item>
    /// <item>Professional standards compliance for audit collections</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCollections()
    {
        // Validate DocumentActivityUserAudit collection
        foreach (var result in ValidateAuditCollection(
            DocumentActivityUserAudit,
            nameof(DocumentActivityUserAudit),
            "Document activity audit"))
        {
            yield return result;
        }

        // Validate MatterDocumentActivityUserFromAudit collection
        foreach (var result in ValidateAuditCollection(
            MatterDocumentActivityUserFromAudit,
            nameof(MatterDocumentActivityUserFromAudit),
            "Matter document from activity audit"))
        {
            yield return result;
        }

        // Validate MatterDocumentActivityUserToAudit collection
        foreach (var result in ValidateAuditCollection(
            MatterDocumentActivityUserToAudit,
            nameof(MatterDocumentActivityUserToAudit),
            "Matter document to activity audit"))
        {
            yield return result;
        }
    }

    /// <summary>
    /// Validates custom rules specific to comprehensive document audit aggregation and legal compliance.
    /// </summary>
    /// <returns>A collection of validation results for custom validation.</returns>
    /// <remarks>
    /// This method implements custom validation logic specific to comprehensive audit trail aggregation,
    /// including legal compliance considerations and professional practice standards.
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCustomRules()
    {
        // Custom rule: Audit trail completeness for legal compliance
        if (Document.IsDeleted || HasAnyActivity)
        {
            yield return CreateValidationResult(
                "Active documents should have at least one audit trail entry for legal compliance and professional standards.",
                nameof(DocumentActivityUserAudit));
        }

        // Custom rule: Transfer audit consistency
        if (HasTransferHistory)
        {
            // Validate that bidirectional transfers make sense
            var fromTransferCount = MatterDocumentActivityUserFromAudit.Count;
            var toTransferCount = MatterDocumentActivityUserToAudit.Count;

            // If there are many more "from" than "to" transfers, it might indicate missing data
            if (fromTransferCount > toTransferCount + 5)
            {
                yield return CreateValidationResult(
                    $"Document has {fromTransferCount} outbound transfers but only {toTransferCount} inbound transfers. " +
                    "This may indicate incomplete audit trail data.",
                    nameof(MatterDocumentActivityUserFromAudit), nameof(MatterDocumentActivityUserToAudit));
            }
        }

        // Custom rule: Professional standards for comprehensive audit reporting
        if (!DocumentActivityUserAudit.Any())
        {
            // Check for reasonable user attribution across activities
            var uniqueUsers = DocumentActivityUserAudit
                .Select(a => a.User?.Id)
                .Where(id => id.HasValue && id != Guid.Empty)
                .Distinct()
                .Count();

            var totalActivities = DocumentActivityUserAudit.Count;

            // If all activities are by the same user and there are many activities, it might need review
            if (uniqueUsers == 1 && totalActivities > 10)
            {
                yield return CreateValidationResult(
                    $"Document has {totalActivities} activities all performed by the same user. " +
                    "Verify this represents normal professional practice patterns.",
                    nameof(DocumentActivityUserAudit));
            }
        }

        // Custom rule: Audit trail security validation
        if (HasAnyActivity)
        {
            // Check for suspicious patterns that might indicate automated or bulk operations
            var allActivityTimestamps = new List<DateTime>();
            allActivityTimestamps.AddRange(DocumentActivityUserAudit.Select(a => a.CreatedAt));
            allActivityTimestamps.AddRange(MatterDocumentActivityUserFromAudit.Select(a => a.CreatedAt));
            allActivityTimestamps.AddRange(MatterDocumentActivityUserToAudit.Select(a => a.CreatedAt));

            var orderedTimestamps = allActivityTimestamps.OrderBy(t => t).ToList();
            
            // Check for suspiciously rapid activity sequences
            for (var i = 1; i < orderedTimestamps.Count; i++)
            {
                var timeDiff = orderedTimestamps[i] - orderedTimestamps[i - 1];
                if (!(timeDiff.TotalSeconds < 1) || orderedTimestamps.Count <= 5) continue;
                yield return CreateValidationResult(
                    "Audit trail contains activities with sub-second intervals. Verify this represents genuine user activities.",
                    nameof(DocumentActivityUserAudit), nameof(MatterDocumentActivityUserFromAudit), nameof(MatterDocumentActivityUserToAudit));
                break; // Only report this once
            }
        }

        // Custom rule: Legal compliance validation for document lifecycle
        if (!DocumentActivityUserAudit.Any()) yield break;
        var hasCreation = DocumentActivityUserAudit.Any(a => a.IsCreationActivity);
        var hasDeletion = DocumentActivityUserAudit.Any(a => a.IsStateChangeActivity && 
                                                             a.DocumentActivity?.Activity?.ToUpperInvariant() == "DELETED");

        // If document is marked as deleted but has no deletion activity, flag inconsistency
        if (Document.IsDeleted || !!hasDeletion)
        {
            yield return CreateValidationResult(
                "Document is marked as deleted but audit trail contains no deletion activity. This may indicate data inconsistency.",
                nameof(Document), nameof(DocumentActivityUserAudit));
        }

        // If document has deletion activity but is not marked as deleted, flag inconsistency
        if (Document.IsDeleted || !hasDeletion) yield break;
        var hasRestoration = DocumentActivityUserAudit.Any(a => a.IsStateChangeActivity && 
                                                                a.DocumentActivity?.Activity?.ToUpperInvariant() == "RESTORED");
                
        if (!hasRestoration)
        {
            yield return CreateValidationResult(
                "Document has deletion activity but is not marked as deleted and has no restoration activity. Verify document state consistency.",
                nameof(Document), nameof(DocumentActivityUserAudit));
        }
    }

    #endregion Standardized Validation Implementation

    #region Collection Validation Helper

    /// <summary>
    /// Validates a collection of audit items using generic validation logic with comprehensive error context.
    /// </summary>
    /// <typeparam name="T">The type of the audit item, must implement IValidatableObject.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="propertyName">The property name for error reporting.</param>
    /// <param name="collectionDescription">Human-readable description of the collection for error messages.</param>
    /// <returns>A collection of validation results for the audit collection.</returns>
    /// <remarks>
    /// This comprehensive method provides detailed validation logic for all audit collections,
    /// ensuring null safety, individual item validation, and appropriate error reporting with
    /// precise error contexts for debugging and troubleshooting.
    /// </remarks>
    private IEnumerable<ValidationResult> ValidateAuditCollection<T>(
        IEnumerable<T> collection,
        string propertyName,
        string collectionDescription)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(collection);

        var collectionList = collection.ToList();
        var itemIndex = 0;

        foreach (var item in collectionList)
        {
            switch (item)
            {
                // Validate using BaseValidationDto if the item supports it
                case BaseValidationDto validatableItem:
                {
                    var itemResults = ValidateModel(validatableItem);
                    foreach (var result in itemResults)
                    {
                        yield return CreateContextualValidationResult(
                            $"{collectionDescription} validation failed: {result.ErrorMessage}",
                            $"{propertyName}[{itemIndex}].{string.Join(",", result.MemberNames)}");
                    }

                    break;
                }
                // Fallback to IValidatableObject if BaseValidationDto is not available
                case IValidatableObject validatable:
                {
                    var context = new ValidationContext(item);
                    var itemResults = validatable.Validate(context);
                    foreach (var result in itemResults)
                    {
                        yield return CreateContextualValidationResult(
                            $"{collectionDescription} validation failed: {result.ErrorMessage}",
                            $"{propertyName}[{itemIndex}].{string.Join(",", result.MemberNames)}");
                    }

                    break;
                }
            }

            itemIndex++;
        }

        // Additional collection-level validation
        if (collectionList.Count > 1000)
        {
            yield return CreateValidationResult(
                $"{collectionDescription} contains over 1000 items ({collectionList.Count}). " +
                "This may impact performance and should be reviewed for professional practice standards.",
                propertyName);
        }
    }

    #endregion Collection Validation Helper

    #region Static Methods

    /// <summary>
    /// Creates a DocumentAuditMinimalDto from an ADMS.API.Entities.Document entity with complete audit trail information and standardized validation.
    /// </summary>
    /// <param name="document">The Document entity to convert. Cannot be null.</param>
    /// <param name="includeActivityAudits">Whether to include document activity audit trails.</param>
    /// <param name="includeTransferAudits">Whether to include matter transfer audit trails.</param>
    /// <returns>A valid DocumentAuditMinimalDto instance with complete audit information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when document is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var document = await context.Documents
    ///     .Include(d => d.DocumentActivityUsers)
    ///         .ThenInclude(dau => dau.DocumentActivity)
    ///     .Include(d => d.DocumentActivityUsers)
    ///         .ThenInclude(dau => dau.User)
    ///     .FirstAsync(d => d.Id == documentId);
    /// 
    /// var auditDto = DocumentAuditMinimalDto.FromEntity(document, 
    ///     includeActivityAudits: true, includeTransferAudits: true);
    /// // DTO is guaranteed to be valid due to standardized validation
    /// </code>
    /// </example>
    public static DocumentAuditMinimalDto FromEntity(
        [NotNull] Entities.Document document,
        bool includeActivityAudits = true,
        bool includeTransferAudits = true)
    {
        ArgumentNullException.ThrowIfNull(document);

        var auditDto = new DocumentAuditMinimalDto
        {
            Document = DocumentMinimalDto.FromEntity(document),
            DocumentActivityUserAudit = includeActivityAudits
                ? document.DocumentActivityUsers
                    .Select(DocumentActivityUserMinimalDto.FromEntity)
                    .ToArray()
                : [],
            MatterDocumentActivityUserFromAudit = includeTransferAudits
                ? document.MatterDocumentActivityUsersFrom
                    .Select(mdauf => MatterDocumentActivityUserMinimalDto.FromEntity(mdauf, true))
                    .ToArray()
                : [],
            MatterDocumentActivityUserToAudit = includeTransferAudits
                ? document.MatterDocumentActivityUsersTo
                    .Select(mdaut => MatterDocumentActivityUserMinimalDto.FromEntity(mdaut, true))
                    .ToArray()
                : []
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(auditDto);
        if (!HasValidationErrors(validationResults)) return auditDto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Document audit validation failed: {summary}");
    }

    /// <summary>
    /// Creates multiple DocumentAuditMinimalDto instances from a collection of document entities with standardized validation.
    /// </summary>
    /// <param name="documents">The collection of Document entities to convert. Cannot be null.</param>
    /// <param name="includeActivityAudits">Whether to include document activity audit trails.</param>
    /// <param name="includeTransferAudits">Whether to include matter transfer audit trails.</param>
    /// <returns>A collection of valid DocumentAuditMinimalDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when documents collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method uses standardized validation and provides detailed error handling
    /// for invalid entities.
    /// </remarks>
    /// <example>
    /// <code>
    /// var documents = await context.Documents
    ///     .Include(d => d.DocumentActivityUsers)
    ///     .Include(d => d.MatterDocumentActivityUsersFrom)
    ///     .Include(d => d.MatterDocumentActivityUsersTo)
    ///     .ToListAsync();
    /// 
    /// var documentAudits = DocumentAuditMinimalDto.FromEntities(documents, 
    ///     includeActivityAudits: true, includeTransferAudits: true);
    /// // All DTOs are guaranteed to be valid
    /// </code>
    /// </example>
    public static IList<DocumentAuditMinimalDto> FromEntities(
        [NotNull] IEnumerable<Entities.Document> documents,
        bool includeActivityAudits = true,
        bool includeTransferAudits = true)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var result = new List<DocumentAuditMinimalDto>();
        var errors = new List<string>();

        foreach (var document in documents)
        {
            try
            {
                var dto = FromEntity(document, includeActivityAudits, includeTransferAudits);
                result.Add(dto);
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                // Collect errors for comprehensive error reporting
                errors.Add($"Document Audit {document.Id} ({document.FileName}): {ex.Message}");

                // In production, use proper logging framework
                Console.WriteLine($"Warning: Skipped invalid document audit entity: {ex.Message}");
            }
        }

        // Log summary if there were errors
        if (errors.Any())
        {
            Console.WriteLine($"Entity conversion completed with {errors.Count} errors out of {documents.Count()} entities processed.");
        }

        return result;
    }

    /// <summary>
    /// Creates a DocumentAuditMinimalDto with only document information (no activity audits) with standardized validation.
    /// </summary>
    /// <param name="document">The DocumentMinimalDto to use for audit creation.</param>
    /// <returns>A DocumentAuditMinimalDto with only document information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when document is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation
    /// while creating a document-only audit DTO.
    /// </remarks>
    /// <example>
    /// <code>
    /// var documentDto = DocumentMinimalDto.FromEntity(entity);
    /// var documentOnlyAudit = DocumentAuditMinimalDto.CreateDocumentOnly(documentDto);
    /// // Returns audit DTO with document information but no activity audits
    /// </code>
    /// </example>
    public static DocumentAuditMinimalDto CreateDocumentOnly([NotNull] DocumentMinimalDto document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var auditDto = new DocumentAuditMinimalDto
        {
            Document = document,
            DocumentActivityUserAudit = [],
            MatterDocumentActivityUserFromAudit = [],
            MatterDocumentActivityUserToAudit = []
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(auditDto);
        if (!HasValidationErrors(validationResults)) return auditDto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Document audit validation failed: {summary}");
    }

    /// <summary>
    /// Creates a comprehensive DocumentAuditMinimalDto with validated collections and standardized validation.
    /// </summary>
    /// <param name="document">The document minimal DTO.</param>
    /// <param name="documentActivities">The document activity audit collection.</param>
    /// <param name="fromTransfers">The from transfer audit collection.</param>
    /// <param name="toTransfers">The to transfer audit collection.</param>
    /// <returns>A DocumentAuditMinimalDto with complete audit information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when document is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation
    /// while allowing pre-validated collections to be provided.
    /// </remarks>
    /// <example>
    /// <code>
    /// var documentAudit = DocumentAuditMinimalDto.CreateComprehensive(
    ///     documentDto,
    ///     validatedDocumentActivities,
    ///     validatedFromTransfers,
    ///     validatedToTransfers);
    /// 
    /// // DTO is guaranteed to be valid with validated collections
    /// </code>
    /// </example>
    public static DocumentAuditMinimalDto CreateComprehensive(
        [NotNull] DocumentMinimalDto document,
        [NotNull] IEnumerable<DocumentActivityUserMinimalDto> documentActivities,
        [NotNull] IEnumerable<MatterDocumentActivityUserMinimalDto> fromTransfers,
        [NotNull] IEnumerable<MatterDocumentActivityUserMinimalDto> toTransfers)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(documentActivities);
        ArgumentNullException.ThrowIfNull(fromTransfers);
        ArgumentNullException.ThrowIfNull(toTransfers);

        var auditDto = new DocumentAuditMinimalDto
        {
            Document = document,
            DocumentActivityUserAudit = documentActivities.ToArray(),
            MatterDocumentActivityUserFromAudit = fromTransfers.ToArray(),
            MatterDocumentActivityUserToAudit = toTransfers.ToArray()
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(auditDto);
        if (!HasValidationErrors(validationResults)) return auditDto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Document audit validation failed: {summary}");
    }

    #endregion Static Methods

    #region Helper Methods

    /// <summary>
    /// Determines if an activity type is a valid transfer activity.
    /// </summary>
    /// <param name="activityType">The activity type to validate.</param>
    /// <returns>true if the activity is a valid transfer type; otherwise, false.</returns>
    private static bool IsValidTransferActivity(string activityType) =>
        activityType?.ToUpperInvariant() is "MOVED" or "COPIED";

    /// <summary>
    /// Gets all unique users involved in document activities with validation analysis.
    /// </summary>
    /// <returns>A collection of unique users who have performed activities on this document.</returns>
    /// <remarks>
    /// This method provides comprehensive insights including validation status for enhanced analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// var involvedUsers = documentAudit.GetInvolvedUsers();
    /// Console.WriteLine($"Document has been accessed by {involvedUsers.Count()} users");
    /// </code>
    /// </example>
    public IEnumerable<UserMinimalDto> GetInvolvedUsers()
    {
        var allUsers = new List<UserMinimalDto?>();

        // Add users from document activities
        allUsers.AddRange(DocumentActivityUserAudit.Select(a => a.User));

        // Add users from matter transfer activities
        allUsers.AddRange(MatterDocumentActivityUserFromAudit.Select(a => a.User));
        allUsers.AddRange(MatterDocumentActivityUserToAudit.Select(a => a.User));

        return allUsers
            .Where(u => u != null)
            .Cast<UserMinimalDto>()
            .DistinctBy(u => u.Id)
            .OrderBy(u => u.Name);
    }

    /// <summary>
    /// Gets all unique matters involved in document transfer activities.
    /// </summary>
    /// <returns>A collection of unique matters involved in document transfers.</returns>
    public IEnumerable<MatterMinimalDto> GetInvolvedMatters()
    {
        var allMatters = new List<MatterMinimalDto?>();

        // Add matters from transfer activities
        allMatters.AddRange(MatterDocumentActivityUserFromAudit.Select(a => a.Matter));
        allMatters.AddRange(MatterDocumentActivityUserToAudit.Select(a => a.Matter));

        return allMatters
            .Where(m => m != null)
            .Cast<MatterMinimalDto>()
            .DistinctBy(m => m.Id)
            .OrderBy(m => m.Description);
    }

    /// <summary>
    /// Gets activities of a specific type from all audit collections.
    /// </summary>
    /// <param name="activityType">The activity type to filter by (case-insensitive).</param>
    /// <returns>A collection of activities matching the specified type.</returns>
    public IEnumerable<object> GetActivitiesByType(string activityType)
    {
        if (string.IsNullOrWhiteSpace(activityType))
            return [];

        var normalizedActivityType = activityType.ToUpperInvariant();
        var results = new List<object>();

        // Search document activities
        results.AddRange(DocumentActivityUserAudit
            .Where(a => a.DocumentActivity?.Activity?.ToUpperInvariant() == normalizedActivityType));

        // Search matter transfer activities
        results.AddRange(MatterDocumentActivityUserFromAudit
            .Where(a => a.MatterDocumentActivity?.Activity?.ToUpperInvariant() == normalizedActivityType));

        results.AddRange(MatterDocumentActivityUserToAudit
            .Where(a => a.MatterDocumentActivity?.Activity?.ToUpperInvariant() == normalizedActivityType));

        return results;
    }

    /// <summary>
    /// Determines whether this document audit contains activities of the specified type.
    /// </summary>
    /// <param name="activityType">The activity type to check for (case-insensitive).</param>
    /// <returns>true if activities of the specified type exist; otherwise, false.</returns>
    public bool HasActivityType(string activityType) =>
        GetActivitiesByType(activityType).Any();

    /// <summary>
    /// Gets comprehensive audit information including validation analysis.
    /// </summary>
    /// <returns>A dictionary containing detailed audit information and validation status.</returns>
    /// <remarks>
    /// This method provides structured audit information including validation status,
    /// useful for debugging, reporting, and administrative operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var info = documentAudit.GetComprehensiveAuditInformation();
    /// foreach (var (key, value) in info)
    /// {
    ///     Console.WriteLine($"{key}: {value}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetComprehensiveAuditInformation()
    {
        // Perform validation to get current status
        var validationResults = ValidateModel(this);
        var validationStatus = HasValidationErrors(validationResults)
            ? GetValidationSummary(validationResults)
            : "Valid";

        return new Dictionary<string, object>
        {
            // Document Information
            ["DocumentId"] = Document?.Id ?? Guid.Empty,
            ["DocumentName"] = Document?.FileName ?? "Unknown",
            ["DocumentStatus"] = Document?.IsDeleted == true ? "Deleted" : 
                               Document?.IsCheckedOut == true ? "Checked Out" : "Active",

            // Activity Counts
            ["TotalActivityCount"] = TotalActivityCount,
            ["DocumentActivityCount"] = DocumentActivityUserAudit.Count,
            ["TransferActivityCount"] = TransferActivityCount,
            ["FromTransferCount"] = MatterDocumentActivityUserFromAudit.Count,
            ["ToTransferCount"] = MatterDocumentActivityUserToAudit.Count,

            // Analysis
            ["HasAnyActivity"] = HasAnyActivity,
            ["HasTransferHistory"] = HasTransferHistory,
            ["UniqueUserCount"] = GetInvolvedUsers().Count(),
            ["UniqueMattersCount"] = GetInvolvedMatters().Count(),

            // Temporal Information
            ["FirstActivityTime"] = GetFirstActivityTime(),
            ["LastActivityTime"] = GetLastActivityTime(),
            ["AuditTimeSpan"] = GetAuditTimeSpan(),

            // Validation Information
            ["ValidationStatus"] = validationStatus,
            ["IsValid"] = !HasValidationErrors(validationResults),

            // Audit Type
            ["AuditType"] = "Comprehensive",
            ["IncludesTransferAudits"] = HasTransferHistory
        };
    }

    /// <summary>
    /// Gets the timestamp of the first activity in the audit trail.
    /// </summary>
    /// <returns>The DateTime of the first activity, or null if no activities exist.</returns>
    private DateTime? GetFirstActivityTime()
    {
        var allTimes = new List<DateTime>();
        allTimes.AddRange(DocumentActivityUserAudit.Select(a => a.CreatedAt));
        allTimes.AddRange(MatterDocumentActivityUserFromAudit.Select(a => a.CreatedAt));
        allTimes.AddRange(MatterDocumentActivityUserToAudit.Select(a => a.CreatedAt));

        return allTimes.Any() ? allTimes.Min() : null;
    }

    /// <summary>
    /// Gets the timestamp of the last activity in the audit trail.
    /// </summary>
    /// <returns>The DateTime of the last activity, or null if no activities exist.</returns>
    private DateTime? GetLastActivityTime()
    {
        var allTimes = new List<DateTime>();
        allTimes.AddRange(DocumentActivityUserAudit.Select(a => a.CreatedAt));
        allTimes.AddRange(MatterDocumentActivityUserFromAudit.Select(a => a.CreatedAt));
        allTimes.AddRange(MatterDocumentActivityUserToAudit.Select(a => a.CreatedAt));

        return allTimes.Any() ? allTimes.Max() : null;
    }

    /// <summary>
    /// Gets the time span covered by the audit trail.
    /// </summary>
    /// <returns>The TimeSpan from first to last activity, or null if insufficient activities exist.</returns>
    private TimeSpan? GetAuditTimeSpan()
    {
        var first = GetFirstActivityTime();
        var last = GetLastActivityTime();

        return first.HasValue && last.HasValue ? last.Value - first.Value : null;
    }

    #endregion Helper Methods

    #region String Representation

    /// <summary>
    /// Returns a string representation of the DocumentAuditMinimalDto.
    /// </summary>
    public override string ToString()
    {
        var documentName = Document?.FileName ?? "Unknown Document";
        var transferInfo = HasTransferHistory ? $" ({TransferActivityCount} transfers)" : "";

        return $"Document Audit: '{documentName}' with {TotalActivityCount} activities{transferInfo}";
    }

    #endregion String Representation
}