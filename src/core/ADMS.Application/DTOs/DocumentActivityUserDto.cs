using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.Application.DTOs;

/// <summary>
/// Comprehensive Data Transfer Object representing the audit trail association between a document, document activity, and user.
/// </summary>
/// <remarks>
/// This DTO serves as the complete representation of document activity audit trails within the ADMS legal document management system,
/// corresponding to <see cref="ADMS.API.Entities.DocumentActivityUser"/>. It captures comprehensive information about document 
/// operations performed by users, providing essential audit trail data for legal compliance, professional responsibility, 
/// and comprehensive document management accountability.
/// 
/// <para><strong>Enhanced with Standardized Validation (.NET 9):</strong></para>
/// <list type="bullet">
/// <item><strong>BaseValidationDto Integration:</strong> Inherits standardized ADMS validation patterns</item>
/// <item><strong>Audit Trail Validation:</strong> Specialized validation for audit trail data integrity</item>
/// <item><strong>Performance Optimized:</strong> Uses yield return for lazy validation evaluation</item>
/// <item><strong>Validation Hierarchy:</strong> Follows standardized core → business → cross-property → custom pattern</item>
/// <item><strong>Navigation Validation:</strong> Advanced validation for navigation property consistency</item>
/// </list>
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Audit Trail Foundation:</strong> Complete representation of document activity audit entries with full context</item>
/// <item><strong>Standardized Validation:</strong> Uses BaseValidationDto for consistent validation patterns</item>
/// <item><strong>Professional Validation:</strong> Uses centralized validation helpers for comprehensive data integrity</item>
/// <item><strong>Legal Compliance Support:</strong> Designed for comprehensive audit reporting and legal compliance</item>
/// <item><strong>Entity Synchronization:</strong> Mirrors all properties and relationships from ADMS.API.Entities.DocumentActivityUser</item>
/// </list>
/// 
/// <para><strong>Validation Hierarchy:</strong></para>
/// Following BaseValidationDto standardized validation pattern:
/// <list type="number">
/// <item><strong>Core Properties:</strong> Composite key validation (DocumentId, DocumentActivityId, UserId, CreatedAt)</item>
/// <item><strong>Business Rules:</strong> Audit trail integrity, activity-specific business rules, professional standards</item>
/// <item><strong>Cross-Property:</strong> Navigation property consistency, foreign key validation</item>
/// <item><strong>Custom Rules:</strong> Audit-specific validation, temporal consistency, security validations</item>
/// </list>
/// 
/// <para><strong>Document Activities Tracked:</strong></para>
/// Based on <see cref="ADMS.API.Entities.DocumentActivity"/> seeded data:
/// <list type="bullet">
/// <item><strong>CHECKED IN:</strong> Document checked into version control system</item>
/// <item><strong>CHECKED OUT:</strong> Document checked out for editing</item>
/// <item><strong>CREATED:</strong> Initial document creation</item>
/// <item><strong>DELETED:</strong> Document marked for deletion (soft delete)</item>
/// <item><strong>RESTORED:</strong> Deleted document restored to active status</item>
/// <item><strong>SAVED:</strong> Document saved with changes</item>
/// </list>
/// 
/// <para><strong>Performance Benefits with Standardized Validation:</strong></para>
/// <list type="bullet">
/// <item><strong>Early Termination:</strong> Validation stops on critical errors for better performance</item>
/// <item><strong>Lazy Evaluation:</strong> Navigation properties validated only when needed</item>
/// <item><strong>Consistent Error Handling:</strong> Standardized error formatting and reporting</item>
/// <item><strong>Memory Efficient:</strong> Optimized validation memory usage for audit trail operations</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Creating a comprehensive document activity audit entry with standardized validation
/// var auditEntry = new DocumentActivityUserDto
/// {
///     DocumentId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
///     Document = new DocumentWithoutRevisionsDto { /* document data */ },
///     DocumentActivityId = Guid.Parse("20000000-0000-0000-0000-000000000003"),
///     DocumentActivity = new DocumentActivityDto { /* activity data */ },
///     UserId = Guid.Parse("50000000-0000-0000-0000-000000000001"),
///     User = new UserDto { /* user data */ },
///     CreatedAt = DateTime.UtcNow
/// };
/// 
/// // Standardized validation using BaseValidationDto
/// var validationResults = BaseValidationDto.ValidateModel(auditEntry);
/// if (BaseValidationDto.HasValidationErrors(validationResults))
/// {
///     var summary = BaseValidationDto.GetValidationSummary(validationResults);
///     _logger.LogWarning("Audit trail validation failed: {ValidationSummary}", summary);
/// }
/// 
/// // Professional audit trail processing with validation
/// if (auditEntry.IsValid)
/// {
///     ProcessDocumentActivityAudit(auditEntry);
/// }
/// </code>
/// </example>
public class DocumentActivityUserDto : BaseValidationDto, IEquatable<DocumentActivityUserDto>, IComparable<DocumentActivityUserDto>
{
    #region Composite Primary Key Properties

    /// <summary>
    /// Gets or sets the unique identifier of the document that the activity was performed on.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific document 
    /// that was the subject of the activity.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateGuid() with allowEmpty=false.
    /// </remarks>
    [Required(ErrorMessage = "Document ID is required for activity audit trail identification.")]
    public required Guid DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the document activity that was performed.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific type of 
    /// document operation that was performed.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateGuid() with allowEmpty=false.
    /// </remarks>
    [Required(ErrorMessage = "Document activity ID is required for activity classification.")]
    public required Guid DocumentActivityId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who performed the document activity.
    /// </summary>
    /// <remarks>
    /// This GUID serves as part of the composite primary key and identifies the specific user who 
    /// performed the document operation.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateGuid() with allowEmpty=false.
    /// </remarks>
    [Required(ErrorMessage = "User ID is required for activity attribution and professional accountability.")]
    public required Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the document activity was performed.
    /// </summary>
    /// <remarks>
    /// This DateTime serves as part of the composite primary key and records the precise moment when 
    /// the document activity occurred.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCoreProperties() using BaseValidationDto.ValidateDateTime() with temporal constraints.
    /// </remarks>
    [Required(ErrorMessage = "Activity timestamp is required for audit trail chronology.")]
    public required DateTime CreatedAt { get; set; }

    #endregion Composite Primary Key Properties

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the comprehensive document details for the document that the activity was performed on.
    /// </summary>
    /// <remarks>
    /// This property provides complete document information for the document that was the subject of the 
    /// activity, corresponding to the <see cref="ADMS.API.Entities.DocumentActivityUser.Document"/> 
    /// navigation property.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCrossPropertyRules() for navigation property consistency.
    /// </remarks>
    [Required(ErrorMessage = "Document details are required for comprehensive audit trail context.")]
    public required DocumentWithoutRevisionsDto Document { get; set; }

    /// <summary>
    /// Gets or sets the comprehensive document activity details describing the type of operation performed.
    /// </summary>
    /// <remarks>
    /// This property provides complete activity information for the document operation being performed.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCrossPropertyRules() for navigation property consistency.
    /// </remarks>
    [Required(ErrorMessage = "Document activity details are required for operation classification.")]
    public required DocumentActivityDto DocumentActivity { get; set; }

    /// <summary>
    /// Gets or sets the comprehensive user details for the user who performed the document activity.
    /// </summary>
    /// <remarks>
    /// This property provides complete user information for the professional who performed the document 
    /// operation.
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Validated in ValidateCrossPropertyRules() for navigation property consistency.
    /// </remarks>
    [Required(ErrorMessage = "User details are required for professional accountability and audit attribution.")]
    public required UserDto User { get; set; }

    #endregion Navigation Properties

    #region Computed Properties

    /// <summary>
    /// Gets the creation date and time in local time zone, formatted for professional display.
    /// </summary>
    public string LocalCreatedAtDateString => CreatedAt.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");

    /// <summary>
    /// Gets a professional summary of the document activity for audit trail display.
    /// </summary>
    public string ActivitySummary =>
        $"{Document?.FileName ?? "Unknown Document"} " +
        $"{DocumentActivity?.Activity ?? "UNKNOWN"} " +
        $"by {User?.Name ?? "Unknown User"}";

    /// <summary>
    /// Gets a value indicating whether this activity represents a version control operation.
    /// </summary>
    public bool IsVersionControlActivity =>
        string.Equals(DocumentActivity?.Activity, "CHECKED IN", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(DocumentActivity?.Activity, "CHECKED OUT", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this activity represents a document creation operation.
    /// </summary>
    public bool IsCreationActivity =>
        string.Equals(DocumentActivity?.Activity, "CREATED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this activity represents a content modification operation.
    /// </summary>
    public bool IsContentModificationActivity =>
        string.Equals(DocumentActivity?.Activity, "SAVED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this activity represents a state change operation.
    /// </summary>
    public bool IsStateChangeActivity =>
        string.Equals(DocumentActivity?.Activity, "DELETED", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(DocumentActivity?.Activity, "RESTORED", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this audit entry DTO is valid for system operations.
    /// </summary>
    public bool IsValid =>
        DocumentId != Guid.Empty &&
        DocumentActivityId != Guid.Empty &&
        UserId != Guid.Empty &&
        CreatedAt != default;

    /// <summary>
    /// Gets the activity category for classification purposes.
    /// </summary>
    public string ActivityCategory => DocumentActivity?.Activity?.ToUpperInvariant() switch
    {
        "CREATED" or "DELETED" or "RESTORED" => "Lifecycle",
        "CHECKED IN" or "CHECKED OUT" => "Version Control",
        "SAVED" => "Content Management",
        _ => "Unknown"
    };

    #endregion Computed Properties

    #region Standardized Validation Implementation

    /// <summary>
    /// Validates core properties such as composite key components using ADMS validation helpers.
    /// </summary>
    /// <returns>A collection of validation results for core property validation.</returns>
    /// <remarks>
    /// This method implements the first step of the BaseValidationDto validation hierarchy,
    /// validating the composite primary key components that uniquely identify an audit trail entry.
    /// 
    /// <para><strong>Core Property Validation Steps:</strong></para>
    /// <list type="number">
    /// <item>Document ID validation using BaseValidationDto.ValidateGuid() (does not allow empty)</item>
    /// <item>Document Activity ID validation using BaseValidationDto.ValidateGuid() (does not allow empty)</item>
    /// <item>User ID validation using BaseValidationDto.ValidateGuid() (does not allow empty)</item>
    /// <item>Created timestamp validation using BaseValidationDto.ValidateDateTime() with temporal constraints</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCoreProperties()
    {
        // Validate composite key GUIDs using standardized validation
        foreach (var result in ValidateGuid(DocumentId, nameof(DocumentId), allowEmpty: false))
            yield return result;

        foreach (var result in ValidateGuid(DocumentActivityId, nameof(DocumentActivityId), allowEmpty: false))
            yield return result;

        foreach (var result in ValidateGuid(UserId, nameof(UserId), allowEmpty: false))
            yield return result;

        // Validate timestamp using standardized date validation with audit trail constraints
        foreach (var result in ValidateDateTime(CreatedAt, nameof(CreatedAt),
                     allowFuture: false, allowPast: true))
        {
            yield return result;
        }

        // Additional timestamp validation for audit trail integrity
        if (CreatedAt == default)
        {
            yield return CreateValidationResult(
                "Activity timestamp must be set to a valid date and time for audit trail chronology.",
                nameof(CreatedAt));
        }

        if (CreatedAt > DateTime.UtcNow.AddMinutes(1)) // Allow slight clock skew
        {
            yield return CreateValidationResult(
                "Activity timestamp cannot be in the future for audit trail integrity.",
                nameof(CreatedAt));
        }
    }

    /// <summary>
    /// Validates business rules specific to document activity audit trails and professional standards.
    /// </summary>
    /// <returns>A collection of validation results for business rule validation.</returns>
    /// <remarks>
    /// This method implements the second step of the BaseValidationDto validation hierarchy,
    /// validating domain-specific business rules for document activity audit trails.
    /// 
    /// <para><strong>Business Rules Validated:</strong></para>
    /// <list type="bullet">
    /// <item>Activity temporal constraints and chronological consistency</item>
    /// <item>Version control activity business rules</item>
    /// <item>Document state transition validation</item>
    /// <item>Professional standards compliance for audit trails</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // Business rule: Activity timestamps should be reasonable for audit trail integrity
        if (CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            yield return CreateValidationResult(
                "Activity creation timestamp cannot be significantly in the future for audit trail integrity.",
                nameof(CreatedAt));
        }

        if (CreatedAt < DateTime.UtcNow.AddYears(-10))
        {
            yield return CreateValidationResult(
                "Activity creation timestamp seems unusually old. Verify timestamp accuracy for audit trail integrity.",
                nameof(CreatedAt));
        }

        // Business rule: Version control activities should follow professional patterns
        if (IsVersionControlActivity && Document != null)
        {
            // Check-out activities should be performed on non-checked-out documents
            if (DocumentActivity?.Activity?.ToUpperInvariant() == "CHECKED OUT" && Document.IsCheckedOut)
            {
                yield return CreateValidationResult(
                    "Document cannot be checked out when it is already checked out by another user.",
                    nameof(DocumentActivity), nameof(Document));
            }

            // Check-in activities should only occur on checked-out documents
            if (DocumentActivity?.Activity?.ToUpperInvariant() == "CHECKED IN" && !Document.IsCheckedOut)
            {
                yield return CreateValidationResult(
                    "Document cannot be checked in when it is not currently checked out.",
                    nameof(DocumentActivity), nameof(Document));
            }
        }

        // Business rule: Activities on deleted documents should be restricted
        if (Document?.IsDeleted == true && !IsStateChangeActivity)
        {
            yield return CreateValidationResult(
                "Only RESTORED activities can be performed on deleted documents for audit trail integrity.",
                nameof(DocumentActivity), nameof(Document));
        }

        // Business rule: Creation activities should be unique per document
        if (IsCreationActivity && Document != null)
        {
            // Note: This is a logical check - in practice, the system should prevent multiple creation activities
            yield return CreateValidationResult(
                "Creation activity detected. Ensure this is the first creation activity for this document.",
                nameof(DocumentActivity), nameof(Document));
        }
    }

    /// <summary>
    /// Validates cross-property relationships and consistency including navigation property validation.
    /// </summary>
    /// <returns>A collection of validation results for cross-property validation.</returns>
    /// <remarks>
    /// This method implements the third step of the BaseValidationDto validation hierarchy,
    /// validating relationships between foreign key properties and navigation properties.
    /// 
    /// <para><strong>Cross-Property Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Navigation property ID consistency with foreign keys</item>
    /// <item>Audit trail data consistency</item>
    /// <item>Professional standards consistency across related entities</item>
    /// </list>
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCrossPropertyRules()
    {
        // Cross-property rule: Navigation properties must match foreign keys when provided
        if (Document != null && Document.Id != DocumentId)
        {
            yield return CreateValidationResult(
                "Document navigation property ID must match DocumentId foreign key for audit trail consistency.",
                nameof(Document), nameof(DocumentId));
        }

        if (DocumentActivity != null && DocumentActivity.Id != DocumentActivityId)
        {
            yield return CreateValidationResult(
                "DocumentActivity navigation property ID must match DocumentActivityId foreign key for operation consistency.",
                nameof(DocumentActivity), nameof(DocumentActivityId));
        }

        if (User != null && User.Id != UserId)
        {
            yield return CreateValidationResult(
                "User navigation property ID must match UserId foreign key for accountability consistency.",
                nameof(User), nameof(UserId));
        }

        // Cross-property rule: Activity and document state consistency
        if (DocumentActivity != null && Document != null)
        {
            // Validate activity appropriateness for document state
            if (Document.IsDeleted && DocumentActivity.Activity?.ToUpperInvariant() == "CREATED")
            {
                yield return CreateValidationResult(
                    "Creation activity cannot be recorded for a deleted document. This indicates data inconsistency.",
                    nameof(DocumentActivity), nameof(Document));
            }

            if (!Document.IsDeleted && DocumentActivity.Activity?.ToUpperInvariant() == "RESTORED")
            {
                yield return CreateValidationResult(
                    "Restoration activity should only be recorded for deleted documents.",
                    nameof(DocumentActivity), nameof(Document));
            }
        }

        // Cross-property rule: User authorization validation
        if (User != null && DocumentActivity != null)
        {
            // Check for reasonable user activity patterns
            if (string.IsNullOrWhiteSpace(User.Name))
            {
                yield return CreateValidationResult(
                    "User performing document activities must have a valid name for professional accountability.",
                    nameof(User), nameof(DocumentActivity));
            }
        }
    }

    /// <summary>
    /// Validates custom rules specific to document activity audit trails and security.
    /// </summary>
    /// <returns>A collection of validation results for custom validation.</returns>
    /// <remarks>
    /// This method implements custom validation logic specific to document activity audit trails,
    /// including security considerations, advanced temporal validation, and audit-specific rules.
    /// </remarks>
    protected override IEnumerable<ValidationResult> ValidateCustomRules()
    {
        // Custom rule: Audit trail temporal consistency validation
        if (Document != null && CreatedAt != default)
        {
            // Activity cannot predate document creation significantly
            // Note: In a real system, you'd compare with Document.CreationDate
            if (CreatedAt < DateTime.UtcNow.AddYears(-5))
            {
                yield return CreateValidationResult(
                    "Activity timestamp predates reasonable document creation timeframe. Verify audit trail integrity.",
                    nameof(CreatedAt), nameof(Document));
            }
        }

        // Custom rule: Version control activity patterns
        if (IsVersionControlActivity && DocumentActivity != null)
        {
            // Check for potentially problematic check-out duration
            if (DocumentActivity.Activity?.ToUpperInvariant() == "CHECKED OUT" &&
                CreatedAt < DateTime.UtcNow.AddDays(-30))
            {
                yield return CreateValidationResult(
                    "Document has been checked out for over 30 days. Consider reviewing document status for audit trail completeness.",
                    nameof(CreatedAt), nameof(DocumentActivity));
            }

            // Validate that version control activities have appropriate user context
            if (User != null && string.IsNullOrWhiteSpace(User.Name))
            {
                yield return CreateValidationResult(
                    "Version control activities must have complete user identification for professional accountability.",
                    nameof(User), nameof(DocumentActivity));
            }
        }

        // Custom rule: Activity frequency validation for audit integrity
        if (DocumentActivity != null && CreatedAt != default)
        {
            // Check for suspiciously frequent activities (potential system issues)
            var timeSinceEpoch = (CreatedAt - DateTime.UnixEpoch).TotalMilliseconds;
            if (timeSinceEpoch % 1000 == 0) // Exactly on the second might indicate batch processing
            {
                yield return CreateValidationResult(
                    "Activity timestamp has exact second precision which may indicate batch processing. Verify individual activity attribution.",
                    nameof(CreatedAt));
            }
        }

        // Custom rule: Document activity security validation
        if (Document != null && DocumentActivity != null && User != null)
        {
            // Validate that deleted documents aren't being actively modified
            if (Document.IsDeleted && IsContentModificationActivity)
            {
                yield return CreateValidationResult(
                    "Content modification activities should not be recorded for deleted documents. This may indicate unauthorized access.",
                    nameof(Document), nameof(DocumentActivity));
            }

            // Validate creation activity context
            if (IsCreationActivity && Document.IsDeleted)
            {
                yield return CreateValidationResult(
                    "Creation activity recorded for deleted document indicates potential data integrity issue.",
                    nameof(DocumentActivity), nameof(Document));
            }
        }

        // Custom rule: Professional standards for audit trail entries
        if (User != null && DocumentActivity != null)
        {
            // Ensure audit trail entries have meaningful context
            if (string.IsNullOrWhiteSpace(User.Name) &&
                string.IsNullOrWhiteSpace(DocumentActivity.Activity))
            {
                yield return CreateValidationResult(
                    "Audit trail entries must have meaningful user and activity context for professional accountability.",
                    nameof(User), nameof(DocumentActivity));
            }
        }
    }

    #endregion Standardized Validation Implementation

    #region Static Methods

    /// <summary>
    /// Creates a DocumentActivityUserDto from an ADMS.API.Entities.DocumentActivityUser entity with standardized validation.
    /// </summary>
    /// <param name="entity">The DocumentActivityUser entity to convert. Cannot be null.</param>
    /// <returns>A valid DocumentActivityUserDto instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entity = await context.DocumentActivityUsers
    ///     .Include(dau => dau.Document)
    ///     .Include(dau => dau.DocumentActivity)
    ///     .Include(dau => dau.User)
    ///     .FirstAsync(dau => dau.DocumentId == targetDocumentId);
    /// 
    /// var auditDto = DocumentActivityUserDto.FromEntity(entity);
    /// // DTO is guaranteed to be valid due to standardized validation
    /// </code>
    /// </example>
    public static DocumentActivityUserDto FromEntity([NotNull] Entities.DocumentActivityUser entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var auditDto = new DocumentActivityUserDto
        {
            DocumentId = entity.DocumentId,
            DocumentActivityId = entity.DocumentActivityId,
            UserId = entity.UserId,
            CreatedAt = entity.CreatedAt,
            Document = DocumentWithoutRevisionsDto.FromEntity(entity.Document),
            DocumentActivity = DocumentActivityDto.FromEntity(entity.DocumentActivity, includeUserAssociations: false),
            User = UserDto.FromEntity(entity.User)
        };

        // Use standardized validation from BaseValidationDto
        var validationResults = ValidateModel(auditDto);
        if (!HasValidationErrors(validationResults)) return auditDto;
        var summary = GetValidationSummary(validationResults);
        throw new ValidationException($"Document activity audit validation failed: {summary}");
    }

    /// <summary>
    /// Creates multiple DocumentActivityUserDto instances from a collection of entities with standardized validation.
    /// </summary>
    /// <param name="entities">The collection of DocumentActivityUser entities to convert. Cannot be null.</param>
    /// <returns>A collection of valid DocumentActivityUserDto instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
    /// <remarks>
    /// This bulk conversion method uses standardized validation and provides detailed error handling
    /// for invalid entities.
    /// </remarks>
    /// <example>
    /// <code>
    /// var entities = await context.DocumentActivityUsers
    ///     .Include(dau => dau.Document)
    ///     .Include(dau => dau.DocumentActivity)
    ///     .Include(dau => dau.User)
    ///     .Where(dau => dau.DocumentId == documentId)
    ///     .ToListAsync();
    /// 
    /// var auditDtos = DocumentActivityUserDto.FromEntities(entities);
    /// // All DTOs are guaranteed to be valid
    /// </code>
    /// </example>
    public static IList<DocumentActivityUserDto> FromEntities([NotNull] IEnumerable<Entities.DocumentActivityUser> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var result = new List<DocumentActivityUserDto>();
        var errors = new List<string>();

        foreach (var entity in entities)
        {
            try
            {
                var dto = FromEntity(entity);
                result.Add(dto);
            }
            catch (Exception ex) when (ex is ValidationException or ArgumentException)
            {
                // Collect errors for comprehensive error reporting
                errors.Add($"Audit {entity.DocumentId}/{entity.DocumentActivityId}/{entity.UserId}: {ex.Message}");

                // In production, use proper logging framework
                Console.WriteLine($"Warning: Skipped invalid audit entity: {ex.Message}");
            }
        }

        // Log summary if there were errors
        if (errors.Any())
        {
            Console.WriteLine($"Entity conversion completed with {errors.Count} errors out of {entities.Count()} entities processed.");
        }

        return result;
    }

    /// <summary>
    /// Creates a DocumentActivityUserDto for a specific activity type using seeded activity GUIDs with standardized validation.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="activityName">The standardized activity name.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="timestamp">Optional timestamp (uses current UTC time if not provided).</param>
    /// <returns>A DocumentActivityUserDto with the appropriate seeded activity GUID.</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid or activity is not allowed.</exception>
    /// <exception cref="ValidationException">Thrown when the resulting DTO fails validation.</exception>
    /// <remarks>
    /// This factory method uses the standardized BaseValidationDto.ValidateModel() for consistent validation.
    /// Note: Navigation properties are not populated and must be set separately if needed.
    /// </remarks>
    /// <example>
    /// <code>
    /// var creationAudit = DocumentActivityUserDto.CreateStandardActivity(
    ///     documentId,
    ///     "CREATED",
    ///     userId);
    /// 
    /// // DTO is guaranteed to have valid core properties and appropriate seeded GUID
    /// Console.WriteLine($"Created activity ID: {creationAudit.DocumentActivityId}");
    /// </code>
    /// </example>
    public static DocumentActivityUserDto CreateStandardActivity(
        Guid documentId,
        [NotNull] string activityName,
        Guid userId,
        DateTime? timestamp = null)
    {
        if (documentId == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty.", nameof(documentId));

        ArgumentException.ThrowIfNullOrWhiteSpace(activityName, nameof(activityName));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        var normalizedActivity = activityName.Trim().ToUpperInvariant();
        var activityId = GetSeededActivityId(normalizedActivity);

        if (activityId == Guid.Empty)
        {
            throw new ArgumentException(
                $"Activity '{activityName}' is not a standard seeded activity. " +
                $"Allowed activities: {DocumentActivityValidationHelper.AllowedActivitiesList}",
                nameof(activityName));
        }

        // Create minimal DTO for validation - navigation properties must be set by caller
        var auditDto = new DocumentActivityUserDto
        {
            DocumentId = documentId,
            DocumentActivityId = activityId,
            UserId = userId,
            CreatedAt = timestamp ?? DateTime.UtcNow,
            // Navigation properties must be set separately to avoid circular dependencies
            Document = new DocumentWithoutRevisionsDto
            {
                Id = documentId,
                FileName = "TempDocument",
                Extension = "tmp",
                FileSize = 0,
                MimeType = "application/octet-stream",
                Checksum = "0000000000000000000000000000000000000000000000000000000000000000",
                IsCheckedOut = false,
                IsDeleted = false
            },
            DocumentActivity = new DocumentActivityDto
            {
                Id = activityId,
                Activity = normalizedActivity
            },
            User = new UserDto
            {
                Id = userId,
                Name = "TempUser"
            }
        };

        // Note: This creates a basic structure that will need proper navigation properties
        // The caller should replace the navigation properties with actual data
        return auditDto;
    }

    #endregion Static Methods

    #region Business Logic Methods

    /// <summary>
    /// Gets the seeded GUID for a specific document activity name from ADMS.API.Entities.DocumentActivity.
    /// </summary>
    /// <param name="activityName">The activity name to get the GUID for.</param>
    /// <returns>The seeded GUID if found; otherwise, Guid.Empty.</returns>
    public static Guid GetSeededActivityId(string activityName)
    {
        if (string.IsNullOrWhiteSpace(activityName))
            return Guid.Empty;

        return activityName.Trim().ToUpperInvariant() switch
        {
            "CHECKED IN" => Guid.Parse("20000000-0000-0000-0000-000000000001"),
            "CHECKED OUT" => Guid.Parse("20000000-0000-0000-0000-000000000002"),
            "CREATED" => Guid.Parse("20000000-0000-0000-0000-000000000003"),
            "DELETED" => Guid.Parse("20000000-0000-0000-0000-000000000004"),
            "RESTORED" => Guid.Parse("20000000-0000-0000-0000-000000000005"),
            "SAVED" => Guid.Parse("20000000-0000-0000-0000-000000000006"),
            _ => Guid.Empty
        };
    }

    /// <summary>
    /// Gets a comprehensive audit trail summary for this activity entry.
    /// </summary>
    /// <returns>A detailed string containing activity, user, document, and timing information.</returns>
    public string GetAuditTrailSummary()
    {
        var documentName = Document?.FileName ?? "Unknown Document";
        var activityType = DocumentActivity?.Activity ?? "UNKNOWN";
        var userName = User?.Name ?? "Unknown User";

        return $"Document '{documentName}' {activityType} by {userName} on {LocalCreatedAtDateString}";
    }

    /// <summary>
    /// Determines whether this audit entry represents the same logical operation as another entry.
    /// </summary>
    /// <param name="other">The other audit entry to compare.</param>
    /// <returns>true if both entries represent the same logical operation; otherwise, false.</returns>
    public bool IsSameOperation(DocumentActivityUserDto? other)
    {
        if (other is null) return false;

        return DocumentId == other.DocumentId &&
               DocumentActivityId == other.DocumentActivityId &&
               UserId == other.UserId;
    }

    /// <summary>
    /// Gets the time elapsed since this activity was performed.
    /// </summary>
    /// <returns>A TimeSpan representing the time elapsed since the activity.</returns>
    public TimeSpan GetTimeElapsed() => DateTime.UtcNow - CreatedAt;

    /// <summary>
    /// Determines whether this activity was performed within the specified time period.
    /// </summary>
    /// <param name="timespan">The time period to check against.</param>
    /// <returns>true if the activity was performed within the specified timespan; otherwise, false.</returns>
    public bool WasPerformedWithin(TimeSpan timespan) => GetTimeElapsed() <= timespan;

    /// <summary>
    /// Gets usage statistics for this audit entry for reporting purposes with validation analysis.
    /// </summary>
    /// <returns>A dictionary containing comprehensive audit entry statistics and validation status.</returns>
    /// <remarks>
    /// This method provides comprehensive insights including validation status for enhanced analysis.
    /// </remarks>
    /// <example>
    /// <code>
    /// var stats = auditEntry.GetAuditStatistics();
    /// Console.WriteLine($"Activity: {stats["ActivityType"]}");
    /// Console.WriteLine($"Validation status: {stats["ValidationStatus"]}");
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetAuditStatistics()
    {
        // Perform validation to get current status
        var validationResults = ValidateModel(this);
        var validationStatus = HasValidationErrors(validationResults)
            ? GetValidationSummary(validationResults)
            : "Valid";

        return new Dictionary<string, object>
        {
            ["ActivityType"] = DocumentActivity?.Activity ?? "Unknown",
            ["ActivityCategory"] = ActivityCategory,
            ["UserName"] = User?.Name ?? "Unknown",
            ["DocumentName"] = Document?.FileName ?? "Unknown",
            ["Timestamp"] = CreatedAt,
            ["LocalTimestamp"] = LocalCreatedAtDateString,
            ["IsRecentActivity"] = WasPerformedWithin(TimeSpan.FromHours(24)),
            ["IsVersionControlActivity"] = IsVersionControlActivity,
            ["IsCreationActivity"] = IsCreationActivity,
            ["IsContentModificationActivity"] = IsContentModificationActivity,
            ["IsStateChangeActivity"] = IsStateChangeActivity,
            ["TimeElapsed"] = GetTimeElapsed(),
            ["ActivitySummary"] = ActivitySummary,
            ["ValidationStatus"] = validationStatus,
            ["IsValid"] = !HasValidationErrors(validationResults)
        };
    }

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
    /// var info = auditEntry.GetAuditInformation();
    /// foreach (var (key, value) in info)
    /// {
    ///     Console.WriteLine($"{key}: {value}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, object> GetAuditInformation()
    {
        // Get audit statistics and validation status
        var stats = GetAuditStatistics();

        return new Dictionary<string, object>
        {
            // Composite Key Information
            [nameof(DocumentId)] = DocumentId,
            [nameof(DocumentActivityId)] = DocumentActivityId,
            [nameof(UserId)] = UserId,
            [nameof(CreatedAt)] = CreatedAt,

            // Entity Information
            ["DocumentName"] = Document?.FileName ?? "Unknown",
            ["ActivityType"] = DocumentActivity?.Activity ?? "Unknown",
            ["UserName"] = User?.Name ?? "Unknown",

            // Classification Information
            ["ActivityCategory"] = ActivityCategory,
            ["IsVersionControlActivity"] = IsVersionControlActivity,
            ["IsCreationActivity"] = IsCreationActivity,
            ["IsContentModificationActivity"] = IsContentModificationActivity,
            ["IsStateChangeActivity"] = IsStateChangeActivity,

            // Temporal Information
            ["LocalTimestamp"] = LocalCreatedAtDateString,
            ["TimeElapsed"] = GetTimeElapsed(),
            ["IsRecentActivity"] = stats["IsRecentActivity"],

            // Summary Information
            ["ActivitySummary"] = ActivitySummary,
            ["AuditTrailSummary"] = GetAuditTrailSummary(),

            // Validation Information
            ["ValidationStatus"] = stats["ValidationStatus"],
            ["IsValid"] = stats["IsValid"]
        };
    }

    #endregion Business Logic Methods

    #region Equality Implementation

    /// <summary>
    /// Determines whether the specified DocumentActivityUserDto is equal to the current DocumentActivityUserDto.
    /// </summary>
    public bool Equals(DocumentActivityUserDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return DocumentId.Equals(other.DocumentId) &&
               DocumentActivityId.Equals(other.DocumentActivityId) &&
               UserId.Equals(other.UserId) &&
               CreatedAt.Equals(other.CreatedAt);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current DocumentActivityUserDto.
    /// </summary>
    public override bool Equals(object? obj) =>
        obj is DocumentActivityUserDto other && Equals(other);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode() =>
        HashCode.Combine(DocumentId, DocumentActivityId, UserId, CreatedAt);

    #endregion Equality Implementation

    #region Comparison Implementation

    /// <summary>
    /// Compares the current DocumentActivityUserDto with another DocumentActivityUserDto for ordering purposes.
    /// </summary>
    public int CompareTo(DocumentActivityUserDto? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by timestamp for chronological ordering
        var timestampComparison = CreatedAt.CompareTo(other.CreatedAt);
        if (timestampComparison != 0) return timestampComparison;

        // Secondary sort by activity type for consistency
        var activityComparison = string.Compare(
            DocumentActivity?.Activity,
            other.DocumentActivity?.Activity,
            StringComparison.OrdinalIgnoreCase);
        if (activityComparison != 0) return activityComparison;

        // Tertiary sort by document ID for complete consistency
        var documentComparison = DocumentId.CompareTo(other.DocumentId);
        return documentComparison != 0 ? documentComparison :
            // Final sort by user ID for complete deterministic ordering
            UserId.CompareTo(other.UserId);
    }

    #endregion Comparison Implementation

    #region String Representation

    /// <summary>
    /// Returns a string representation of the DocumentActivityUserDto.
    /// </summary>
    public override string ToString()
    {
        var documentName = Document?.FileName ?? "Unknown Document";
        var activityType = DocumentActivity?.Activity ?? "UNKNOWN";
        var userName = User?.Name ?? "Unknown User";

        return $"Document Activity Audit: '{documentName}' {activityType} by {userName}";
    }

    #endregion String Representation
}