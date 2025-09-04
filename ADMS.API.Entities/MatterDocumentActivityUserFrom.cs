using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Entities;

/// <summary>
/// Represents the linkage of a user to a "from" matter document activity in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// The MatterDocumentActivityUserFrom entity serves as a critical component of the directional audit trail system
/// for document transfer operations between matters. This entity specifically tracks the source side of document
/// transfers (moves and copies), working in conjunction with MatterDocumentActivityUserTo to provide complete
/// bidirectional audit trails essential for legal document management compliance.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Directional Audit Trail:</strong> Tracks source side of document transfers between matters</item>
/// <item><strong>Composite Primary Key:</strong> Ensures uniqueness while allowing temporal tracking</item>
/// <item><strong>User Attribution:</strong> Links document transfers to responsible source users</item>
/// <item><strong>Temporal Tracking:</strong> Maintains precise timestamps for all transfer operations</item>
/// <item><strong>Legal Compliance:</strong> Supports comprehensive audit requirements for document provenance</item>
/// </list>
/// 
/// <para><strong>Database Configuration:</strong></para>
/// <list type="bullet">
/// <item>Composite primary key: MatterId + DocumentId + MatterDocumentActivityId + UserId + CreatedAt</item>
/// <item>All foreign key relationships are required</item>
/// <item>NoAction cascade delete to preserve audit trail integrity</item>
/// <item>Indexed on CreatedAt for temporal queries</item>
/// </list>
/// 
/// <para><strong>Transfer Operations Supported:</strong></para>
/// <list type="bullet">
/// <item><strong>MOVED:</strong> Document moved from this source matter to destination matter</item>
/// <item><strong>COPIED:</strong> Document copied from this source matter to destination matter</item>
/// </list>
/// 
/// <para><strong>Audit Trail Functionality:</strong></para>
/// This entity enables comprehensive tracking of document transfer sources:
/// <list type="bullet">
/// <item><strong>Who:</strong> Which user initiated the document transfer</item>
/// <item><strong>What:</strong> What type of transfer operation occurred (MOVED/COPIED)</item>
/// <item><strong>Where:</strong> Which matter and document were involved in the transfer</item>
/// <item><strong>When:</strong> Precise timestamp of the transfer operation</item>
/// </list>
/// 
/// <para><strong>Legal Compliance Support:</strong></para>
/// <list type="bullet">
/// <item>Complete document custody chains for legal discovery</item>
/// <item>Bidirectional audit trails for document provenance</item>
/// <item>User accountability for document transfer initiation</item>
/// <item>Temporal tracking for legal timeline reconstruction</item>
/// </list>
/// 
/// <para><strong>Business Rules:</strong></para>
/// <list type="bullet">
/// <item>Every document transfer must be attributed to both source and destination users</item>
/// <item>Multiple transfers of the same document can occur with different timestamps</item>
/// <item>CreatedAt timestamp must be within reasonable bounds</item>
/// <item>All foreign key relationships must reference valid entities</item>
/// </list>
/// 
/// <para><strong>Entity Framework Configuration:</strong></para>
/// The entity is configured in AdmsContext.ConfigureMatterDocumentActivityUserFrom with:
/// <list type="bullet">
/// <item>Composite key including all five core properties</item>
/// <item>Required relationships to Matter, Document, MatterDocumentActivity, and User</item>
/// <item>NoAction cascade behaviors to maintain audit trail integrity</item>
/// <item>Performance indexes on commonly queried fields</item>
/// </list>
/// </remarks>
public class MatterDocumentActivityUserFrom : IEquatable<MatterDocumentActivityUserFrom>, IComparable<MatterDocumentActivityUserFrom>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier of the source matter.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the source matter where the
    /// document was transferred from. It forms part of the composite primary key and is required
    /// for all matter document activity user associations.
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to Matter entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>NoAction cascade delete to preserve audit trail integrity</item>
    /// </list>
    /// 
    /// <para><strong>Business Context:</strong></para>
    /// <list type="bullet">
    /// <item>Represents the source matter in document transfer operations</item>
    /// <item>Links transfer activity to specific legal case or project origin</item>
    /// <item>Enables matter-scoped document transfer queries</item>
    /// <item>Supports matter-level audit trail aggregation</item>
    /// </list>
    /// 
    /// <para><strong>Legal Significance:</strong></para>
    /// The source matter ID is crucial for legal document management as it establishes
    /// where documents originated from, supporting case management and legal compliance
    /// requirements for document custody and provenance tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// var transferFrom = new MatterDocumentActivityUserFrom
    /// {
    ///     MatterId = sourceMatter.Id,  // Must reference valid source matter
    ///     DocumentId = document.Id,
    ///     MatterDocumentActivityId = activityId,
    ///     UserId = initiatingUser.Id,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter ID is required for source matter association.")]
    public Guid MatterId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the document being transferred.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the document that was transferred
    /// from the source matter. It forms part of the composite primary key and identifies the
    /// specific document involved in the transfer operation.
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to Document entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>NoAction cascade delete to preserve audit trail integrity</item>
    /// </list>
    /// 
    /// <para><strong>Transfer Context:</strong></para>
    /// <list type="bullet">
    /// <item>Identifies the specific document being transferred</item>
    /// <item>Links transfer activity to document metadata and content</item>
    /// <item>Enables document-scoped transfer history queries</item>
    /// <item>Supports document provenance and custody tracking</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Integration:</strong></para>
    /// The document association enables comprehensive tracking of document movements
    /// across matters, supporting legal discovery, compliance audits, and case management
    /// requirements for maintaining accurate document custody records.
    /// </remarks>
    /// <example>
    /// <code>
    /// var transferFrom = new MatterDocumentActivityUserFrom
    /// {
    ///     DocumentId = legalDocument.Id,  // Must reference valid document
    ///     MatterId = sourceMatter.Id,
    ///     MatterDocumentActivityId = activityId,
    ///     UserId = initiatingUser.Id,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Document ID is required for document transfer association.")]
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the matter document activity.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the matter document activity type
    /// that was performed. It forms part of the composite primary key and must reference a valid
    /// matter document activity from the seeded data (MOVED or COPIED).
    /// 
    /// <para><strong>Activity Types:</strong></para>
    /// Must reference one of the seeded matter document activities:
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> Document moved from this source matter to destination matter</item>
    /// <item><strong>COPIED:</strong> Document copied from this source matter to destination matter</item>
    /// </list>
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to MatterDocumentActivity entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>NoAction cascade delete to preserve audit integrity</item>
    /// </list>
    /// 
    /// <para><strong>Validation Integration:</strong></para>
    /// The MatterDocumentActivityId is validated using MatterDocumentActivityValidationHelper
    /// to ensure it references a valid, allowed transfer activity type.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Linking to a COPIED activity (seeded ID: 40000000-0000-0000-0000-000000000001)
    /// var copiedActivityId = Guid.Parse("40000000-0000-0000-0000-000000000001");
    /// var transferFrom = new MatterDocumentActivityUserFrom
    /// {
    ///     MatterDocumentActivityId = copiedActivityId,  // Must be valid activity
    ///     // ... other properties
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Matter Document Activity ID is required for activity classification.")]
    public Guid MatterDocumentActivityId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who initiated the document transfer.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship to the user who initiated the
    /// document transfer from the source matter. It forms part of the composite primary key and
    /// provides essential user attribution for audit trail purposes in document transfer operations.
    /// 
    /// <para><strong>User Attribution:</strong></para>
    /// <list type="bullet">
    /// <item>Links transfer to responsible source user for accountability</item>
    /// <item>Enables user-scoped document transfer initiation reporting</item>
    /// <item>Supports legal compliance and audit requirements</item>
    /// <item>Facilitates user activity analytics and monitoring</item>
    /// </list>
    /// 
    /// <para><strong>Relationship Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key to User entity</item>
    /// <item>Part of composite primary key</item>
    /// <item>Required field - cannot be Guid.Empty</item>
    /// <item>NoAction cascade delete to preserve audit integrity</item>
    /// </list>
    /// 
    /// <para><strong>Legal Significance:</strong></para>
    /// Source user attribution is critical for:
    /// <list type="bullet">
    /// <item>Document custody chains and legal discovery</item>
    /// <item>Professional responsibility tracking for document transfers</item>
    /// <item>Accountability in legal document management</item>
    /// <item>Evidence of document transfer initiation and authorization</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Attributing document transfer to a specific user
    /// var transferFrom = new MatterDocumentActivityUserFrom
    /// {
    ///     UserId = initiatingUser.Id,  // Must reference valid source user
    ///     MatterId = sourceMatter.Id,
    ///     DocumentId = document.Id,
    ///     MatterDocumentActivityId = activityId,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User ID is required for source user attribution.")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this transfer entry was created (in UTC).
    /// </summary>
    /// <remarks>
    /// This property maintains the precise timestamp of when the document transfer from the source
    /// matter was initiated. It forms part of the composite primary key, enabling multiple transfers
    /// of the same document with different timestamps while maintaining uniqueness.
    /// 
    /// <para><strong>Temporal Tracking:</strong></para>
    /// <list type="bullet">
    /// <item>Provides precise timing for document transfer audit trails</item>
    /// <item>Enables temporal analysis of document movement patterns</item>
    /// <item>Supports legal timeline reconstruction for case management</item>
    /// <item>Facilitates workflow and process analysis</item>
    /// </list>
    /// 
    /// <para><strong>Date Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Must be stored in UTC format for consistency</item>
    /// <item>Cannot be earlier than system minimum date</item>
    /// <item>Cannot be in the future (with tolerance for clock skew)</item>
    /// <item>Forms part of composite primary key for uniqueness</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Automatically set to current UTC time when not specified</item>
    /// <item>Should reflect the actual time the document transfer was initiated</item>
    /// <item>Used for chronological ordering in audit reports</item>
    /// <item>Critical for legal compliance and document custody tracking</item>
    /// </list>
    /// 
    /// <para><strong>Validation:</strong></para>
    /// The CreatedAt timestamp is validated using RevisionValidationHelper date validation
    /// methods to ensure it falls within acceptable ranges for legal document management.
    /// </remarks>
    /// <example>
    /// <code>
    /// var transferFrom = new MatterDocumentActivityUserFrom
    /// {
    ///     CreatedAt = DateTime.UtcNow,  // Always use UTC
    ///     // ... other properties
    /// };
    /// 
    /// // For historical data import
    /// var historicalTransfer = new MatterDocumentActivityUserFrom
    /// {
    ///     CreatedAt = specificUtcDateTime,  // Specific historical timestamp
    ///     // ... other properties
    /// };
    /// </code>
    /// </example>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    #endregion Core Properties

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the source matter associated with this transfer activity.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the source matter where the document was
    /// transferred from. The relationship is established through the MatterId foreign key and
    /// enables rich querying and navigation within Entity Framework for source matter information
    /// and analysis.
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses MatterId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>NoAction cascade delete behavior preserves audit trail integrity</item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Accessing source matter metadata from transfer context</item>
    /// <item>Matter-level document transfer operations and reporting</item>
    /// <item>Audit trail reporting including source matter information</item>
    /// <item>Cross-matter document movement analysis</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// The virtual modifier enables lazy loading, but consider explicit loading or
    /// projections when working with multiple transfers to avoid N+1 query issues.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing source matter information through transfer
    /// var sourceMatterName = transferFrom.Matter?.Description;
    /// var matterCreationDate = transferFrom.Matter?.CreationDate;
    /// 
    /// // Using explicit loading to avoid N+1 queries
    /// await context.Entry(transferFrom)
    ///     .Reference(t => t.Matter)
    ///     .LoadAsync();
    /// </code>
    /// </example>
    [ForeignKey(nameof(MatterId))]
    public virtual required Matter Matter { get; set; }

    /// <summary>
    /// Gets or sets the document associated with this transfer activity.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the document that was transferred from the
    /// source matter. The relationship is established through the DocumentId foreign key and
    /// enables access to document metadata, content, and related information for audit trail
    /// and document management operations.
    /// 
    /// <para><strong>Document Information Access:</strong></para>
    /// Provides access to:
    /// <list type="bullet">
    /// <item>Document metadata including filename, size, and content type</item>
    /// <item>Document content and version information</item>
    /// <item>Related document operations and audit trails</item>
    /// <item>Cross-document transfer analysis and reporting</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses DocumentId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>NoAction cascade delete behavior preserves audit trail integrity</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Integration:</strong></para>
    /// <list type="bullet">
    /// <item>Document-centric audit trail analysis and reporting</item>
    /// <item>Document provenance and custody chain tracking</item>
    /// <item>Legal discovery and compliance support</item>
    /// <item>Cross-document transfer pattern analysis</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing document information through transfer
    /// var documentName = transferFrom.Document?.FileName;
    /// var documentSize = transferFrom.Document?.FileSize;
    /// 
    /// // Document transfer history analysis
    /// var allTransfers = transferFrom.Document?.MatterDocumentActivityUsersFrom
    ///     .OrderBy(t => t.CreatedAt);
    /// </code>
    /// </example>
    [ForeignKey(nameof(DocumentId))]
    public virtual required Document Document { get; set; }

    /// <summary>
    /// Gets or sets the matter document activity associated with this transfer operation.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the matter document activity type that was
    /// performed (MOVED or COPIED). The relationship is established through the
    /// MatterDocumentActivityId foreign key and enables access to activity metadata and
    /// classification for audit trail and business logic operations.
    /// 
    /// <para><strong>Activity Information Access:</strong></para>
    /// Provides access to:
    /// <list type="bullet">
    /// <item>Activity type name (MOVED, COPIED)</item>
    /// <item>Activity metadata and configuration</item>
    /// <item>Activity validation rules and constraints</item>
    /// <item>Cross-activity analysis and reporting</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses MatterDocumentActivityId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>NoAction cascade delete behavior preserves audit integrity</item>
    /// </list>
    /// 
    /// <para><strong>Business Logic Integration:</strong></para>
    /// <list type="bullet">
    /// <item>Activity-specific business rule enforcement</item>
    /// <item>Transfer operation categorization and reporting</item>
    /// <item>Workflow and process analysis</item>
    /// <item>Activity-based authorization and permissions</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing activity type information
    /// var activityType = transferFrom.MatterDocumentActivity?.Activity;
    /// var isMoveOperation = activityType == "MOVED";
    /// 
    /// // Activity-based filtering
    /// var copyOperations = transfers
    ///     .Where(t => t.MatterDocumentActivity?.Activity == "COPIED");
    /// </code>
    /// </example>
    [ForeignKey(nameof(MatterDocumentActivityId))]
    public virtual required MatterDocumentActivity MatterDocumentActivity { get; set; }

    /// <summary>
    /// Gets or sets the user associated with this document transfer operation.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the user who initiated the document transfer
    /// from the source matter. The relationship is established through the UserId foreign key and
    /// enables comprehensive user-based reporting and analysis of document transfer operations.
    /// 
    /// <para><strong>User Information Access:</strong></para>
    /// Provides access to:
    /// <list type="bullet">
    /// <item>User identification and contact information</item>
    /// <item>User document transfer patterns and behavior</item>
    /// <item>Professional attribution for legal compliance</item>
    /// <item>User-based reporting and analytics</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses UserId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>NoAction cascade delete behavior preserves audit integrity</item>
    /// </list>
    /// 
    /// <para><strong>Legal and Compliance Support:</strong></para>
    /// <list type="bullet">
    /// <item>User accountability for document transfer initiation</item>
    /// <item>Professional responsibility tracking for document transfers</item>
    /// <item>Legal discovery and audit trail support</item>
    /// <item>Compliance reporting and analysis</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing user information through transfer
    /// var initiatorName = transferFrom.User?.Name;
    /// var userTransfers = transferFrom.User?.MatterDocumentActivityUsersFrom;
    /// 
    /// // User-based transfer analysis
    /// var userInitiationCount = user.MatterDocumentActivityUsersFrom
    ///     .Count(t => t.MatterDocumentActivity?.Activity == "MOVED");
    /// </code>
    /// </example>
    [ForeignKey(nameof(UserId))]
    public virtual required User User { get; set; }

    #endregion Navigation Properties

    #region Computed Properties

    /// <summary>
    /// Gets a value indicating whether this transfer record has valid foreign key references.
    /// </summary>
    /// <remarks>
    /// This computed property validates that all required foreign key properties contain
    /// valid (non-empty) GUID values, ensuring referential integrity for document transfer
    /// operations.
    /// 
    /// <para><strong>Validation Checks:</strong></para>
    /// <list type="bullet">
    /// <item>MatterId is not Guid.Empty</item>
    /// <item>DocumentId is not Guid.Empty</item>
    /// <item>MatterDocumentActivityId is not Guid.Empty</item>
    /// <item>UserId is not Guid.Empty</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!transferFrom.HasValidReferences)
    /// {
    ///     throw new InvalidOperationException("Transfer record has invalid foreign key references");
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasValidReferences =>
        MatterId != Guid.Empty &&
        DocumentId != Guid.Empty &&
        MatterDocumentActivityId != Guid.Empty &&
        UserId != Guid.Empty;

    /// <summary>
    /// Gets a value indicating whether the CreatedAt timestamp is within reasonable bounds.
    /// </summary>
    /// <remarks>
    /// This computed property uses the RevisionValidationHelper to validate that the
    /// CreatedAt timestamp falls within acceptable date ranges for the legal document
    /// management system.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!transferFrom.HasValidTimestamp)
    /// {
    ///     logger.LogWarning($"Transfer record {transferFrom} has invalid timestamp: {transferFrom.CreatedAt}");
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasValidTimestamp =>
        Common.RevisionValidationHelper.IsValidDate(CreatedAt);

    /// <summary>
    /// Gets the age of this transfer record in days.
    /// </summary>
    /// <remarks>
    /// This computed property calculates the number of days between the transfer creation
    /// time and the current UTC time, providing insight into the age of audit trail records.
    /// </remarks>
    /// <example>
    /// <code>
    /// var recentTransfers = transfers.Where(t => t.AgeDays <= 30);
    /// Console.WriteLine($"Transfer is {transferFrom.AgeDays} days old");
    /// </code>
    /// </example>
    [NotMapped]
    public double AgeDays => (DateTime.UtcNow - CreatedAt).TotalDays;

    /// <summary>
    /// Gets the formatted transfer description for display purposes.
    /// </summary>
    /// <remarks>
    /// This computed property provides a human-readable description of the document transfer
    /// including user attribution, activity type, and timestamp information for audit trail
    /// display and reporting purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine(transferFrom.TransferDescription);
    /// // Output: "Document 'contract.pdf' MOVED from 'Corporate Matter' by rbrown at 2024-01-15 10:30 UTC"
    /// </code>
    /// </example>
    [NotMapped]
    public string TransferDescription =>
        $"Document '{Document?.FileName ?? "Unknown"}' {MatterDocumentActivity?.Activity ?? "TRANSFERRED"} " +
        $"from '{Matter?.Description ?? "Unknown Matter"}' by {User?.Name ?? "Unknown User"} at {CreatedAt:yyyy-MM-dd HH:mm} UTC";

    #endregion Computed Properties

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified MatterDocumentActivityUserFrom is equal to the current MatterDocumentActivityUserFrom.
    /// </summary>
    /// <param name="other">The MatterDocumentActivityUserFrom to compare with the current MatterDocumentActivityUserFrom.</param>
    /// <returns>true if the specified MatterDocumentActivityUserFrom is equal to the current MatterDocumentActivityUserFrom; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing all five components of the composite primary key:
    /// MatterId, DocumentId, MatterDocumentActivityId, UserId, and CreatedAt. This follows
    /// Entity Framework best practices for entities with composite keys.
    /// </remarks>
    public bool Equals(MatterDocumentActivityUserFrom? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return MatterId.Equals(other.MatterId) &&
               DocumentId.Equals(other.DocumentId) &&
               MatterDocumentActivityId.Equals(other.MatterDocumentActivityId) &&
               UserId.Equals(other.UserId) &&
               CreatedAt.Equals(other.CreatedAt);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current MatterDocumentActivityUserFrom.
    /// </summary>
    /// <param name="obj">The object to compare with the current MatterDocumentActivityUserFrom.</param>
    /// <returns>true if the specified object is equal to the current MatterDocumentActivityUserFrom; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as MatterDocumentActivityUserFrom);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current MatterDocumentActivityUserFrom.</returns>
    /// <remarks>
    /// The hash code is computed from all five components of the composite primary key
    /// to ensure consistent hashing behavior that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => HashCode.Combine(MatterId, DocumentId, MatterDocumentActivityId, UserId, CreatedAt);

    /// <summary>
    /// Compares the current MatterDocumentActivityUserFrom with another MatterDocumentActivityUserFrom for ordering purposes.
    /// </summary>
    /// <param name="other">The MatterDocumentActivityUserFrom to compare with the current MatterDocumentActivityUserFrom.</param>
    /// <returns>
    /// A value that indicates the relative order of the transfer records being compared.
    /// Less than zero: This transfer precedes the other transfer.
    /// Zero: This transfer occurs in the same position as the other transfer.
    /// Greater than zero: This transfer follows the other transfer.
    /// </returns>
    /// <remarks>
    /// Comparison is performed based on CreatedAt timestamp for chronological ordering,
    /// which is most useful for audit trail analysis and reporting. Secondary sorts
    /// provide consistency when timestamps are equal.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Sort transfers chronologically
    /// var sortedTransfers = transfers.OrderBy(t => t).ToList();
    /// 
    /// // Compare specific transfers
    /// if (transfer1.CompareTo(transfer2) < 0)
    /// {
    ///     Console.WriteLine($"Transfer1 occurred before Transfer2");
    /// }
    /// </code>
    /// </example>
    public int CompareTo(MatterDocumentActivityUserFrom? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // Primary sort by timestamp for chronological ordering
        var timeComparison = CreatedAt.CompareTo(other.CreatedAt);
        if (timeComparison != 0) return timeComparison;

        // Secondary sort by MatterId for consistency
        var matterComparison = MatterId.CompareTo(other.MatterId);
        if (matterComparison != 0) return matterComparison;

        // Tertiary sort by DocumentId
        var documentComparison = DocumentId.CompareTo(other.DocumentId);
        if (documentComparison != 0) return documentComparison;

        // Quaternary sort by ActivityId
        var activityComparison = MatterDocumentActivityId.CompareTo(other.MatterDocumentActivityId);
        return activityComparison != 0 ? activityComparison :
            // Final sort by UserId
            UserId.CompareTo(other.UserId);
    }

    /// <summary>
    /// Determines whether two MatterDocumentActivityUserFrom instances are equal.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivityUserFrom to compare.</param>
    /// <param name="right">The second MatterDocumentActivityUserFrom to compare.</param>
    /// <returns>true if the MatterDocumentActivityUserFroms are equal; otherwise, false.</returns>
    public static bool operator ==(MatterDocumentActivityUserFrom? left, MatterDocumentActivityUserFrom? right) =>
        EqualityComparer<MatterDocumentActivityUserFrom>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two MatterDocumentActivityUserFrom instances are not equal.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivityUserFrom to compare.</param>
    /// <param name="right">The second MatterDocumentActivityUserFrom to compare.</param>
    /// <returns>true if the MatterDocumentActivityUserFroms are not equal; otherwise, false.</returns>
    public static bool operator !=(MatterDocumentActivityUserFrom? left, MatterDocumentActivityUserFrom? right) => !(left == right);

    /// <summary>
    /// Determines whether one MatterDocumentActivityUserFrom precedes another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivityUserFrom to compare.</param>
    /// <param name="right">The second MatterDocumentActivityUserFrom to compare.</param>
    /// <returns>true if the left MatterDocumentActivityUserFrom precedes the right MatterDocumentActivityUserFrom; otherwise, false.</returns>
    public static bool operator <(MatterDocumentActivityUserFrom? left, MatterDocumentActivityUserFrom? right) =>
        left is not null && (right is null || left.CompareTo(right) < 0);

    /// <summary>
    /// Determines whether one MatterDocumentActivityUserFrom precedes or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivityUserFrom to compare.</param>
    /// <param name="right">The second MatterDocumentActivityUserFrom to compare.</param>
    /// <returns>true if the left MatterDocumentActivityUserFrom precedes or equals the right MatterDocumentActivityUserFrom; otherwise, false.</returns>
    public static bool operator <=(MatterDocumentActivityUserFrom? left, MatterDocumentActivityUserFrom? right) =>
        left is null || (right is not null && left.CompareTo(right) <= 0);

    /// <summary>
    /// Determines whether one MatterDocumentActivityUserFrom follows another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivityUserFrom to compare.</param>
    /// <param name="right">The second MatterDocumentActivityUserFrom to compare.</param>
    /// <returns>true if the left MatterDocumentActivityUserFrom follows the right MatterDocumentActivityUserFrom; otherwise, false.</returns>
    public static bool operator >(MatterDocumentActivityUserFrom? left, MatterDocumentActivityUserFrom? right) =>
        left is not null && (right is null || left.CompareTo(right) > 0);

    /// <summary>
    /// Determines whether one MatterDocumentActivityUserFrom follows or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first MatterDocumentActivityUserFrom to compare.</param>
    /// <param name="right">The second MatterDocumentActivityUserFrom to compare.</param>
    /// <returns>true if the left MatterDocumentActivityUserFrom follows or equals the right MatterDocumentActivityUserFrom; otherwise, false.</returns>
    public static bool operator >=(MatterDocumentActivityUserFrom? left, MatterDocumentActivityUserFrom? right) =>
        left is null || (right is not null && left.CompareTo(right) >= 0);

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the MatterDocumentActivityUserFrom.
    /// </summary>
    /// <returns>A string that represents the current MatterDocumentActivityUserFrom.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the document transfer,
    /// which is useful for debugging, logging, and audit trail display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var transferFrom = new MatterDocumentActivityUserFrom
    /// {
    ///     MatterId = matterGuid,
    ///     DocumentId = documentGuid,
    ///     MatterDocumentActivityId = activityGuid,
    ///     UserId = userGuid,
    ///     CreatedAt = DateTime.UtcNow
    /// };
    /// 
    /// Console.WriteLine(transferFrom);
    /// // Output: "Document MOVED from Matter 'Corporate Case' by rbrown at 2024-01-15 10:30:00 UTC"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Document {MatterDocumentActivity?.Activity ?? "TRANSFERRED"} from Matter '{Matter?.Description ?? MatterId.ToString()[..8]}' " +
        $"by {User?.Name ?? UserId.ToString()[..8]} at {CreatedAt:yyyy-MM-dd HH:mm:ss} UTC";

    #endregion String Representation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this transfer record represents a move operation.
    /// </summary>
    /// <returns>true if this is a move operation; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify move operations without
    /// directly accessing the navigation property, which can help avoid lazy loading
    /// in performance-sensitive scenarios.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (transferFrom.IsMoveOperation())
    /// {
    ///     // Handle move-specific logic
    ///     Console.WriteLine("Document was moved from: " + transferFrom.Matter?.Description);
    /// }
    /// </code>
    /// </example>
    public bool IsMoveOperation() => MatterDocumentActivity?.Activity == "MOVED";

    /// <summary>
    /// Determines whether this transfer record represents a copy operation.
    /// </summary>
    /// <returns>true if this is a copy operation; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to identify copy operations for
    /// audit trail analysis and business rule enforcement.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (transferFrom.IsCopyOperation())
    /// {
    ///     // Handle copy-specific logic
    ///     auditLogger.LogDocumentCopy(transferFrom);
    /// }
    /// </code>
    /// </example>
    public bool IsCopyOperation() => MatterDocumentActivity?.Activity == "COPIED";

    /// <summary>
    /// Determines whether this transfer occurred recently within the specified timeframe.
    /// </summary>
    /// <param name="withinHours">The number of hours to consider as "recent".</param>
    /// <returns>true if the transfer occurred within the specified timeframe; otherwise, false.</returns>
    /// <remarks>
    /// This method is useful for identifying recent document transfers for notifications,
    /// reporting, and real-time monitoring purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check for transfers in the last 24 hours
    /// if (transferFrom.IsRecent(24))
    /// {
    ///     notificationService.NotifyRecentTransfer(transferFrom);
    /// }
    /// </code>
    /// </example>
    public bool IsRecent(double withinHours = 24)
    {
        return (DateTime.UtcNow - CreatedAt).TotalHours <= withinHours;
    }

    /// <summary>
    /// Gets the seeded GUID for a specific matter document activity name.
    /// </summary>
    /// <param name="activityName">The activity name to get the GUID for.</param>
    /// <returns>The seeded GUID if found; otherwise, Guid.Empty.</returns>
    /// <remarks>
    /// This method returns the specific GUIDs used in database seeding for standard
    /// matter document activities, useful for business logic that needs to reference
    /// specific activity types.
    /// </remarks>
    /// <example>
    /// <code>
    /// var copiedActivityId = MatterDocumentActivityUserFrom.GetSeededActivityId("COPIED");
    /// // Returns: 40000000-0000-0000-0000-000000000001
    /// </code>
    /// </example>
    public static Guid GetSeededActivityId(string activityName)
    {
        if (string.IsNullOrWhiteSpace(activityName))
            return Guid.Empty;

        return activityName.Trim().ToUpperInvariant() switch
        {
            "COPIED" => Guid.Parse("40000000-0000-0000-0000-000000000001"),
            "MOVED" => Guid.Parse("40000000-0000-0000-0000-000000000002"),
            _ => Guid.Empty
        };
    }

    #endregion Business Logic Methods

    #region Validation Methods

    /// <summary>
    /// Validates the current MatterDocumentActivityUserFrom instance against business rules.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors found.
    /// Returns an empty enumerable if validation passes.
    /// </returns>
    /// <remarks>
    /// This method performs comprehensive validation using MatterDocumentActivityValidationHelper
    /// and additional entity-specific business rules including foreign key validation,
    /// timestamp validation, and directional transfer requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// var validationResults = transferFrom.Validate();
    /// if (validationResults.Any())
    /// {
    ///     foreach (var error in validationResults)
    ///     {
    ///         Console.WriteLine($"Validation Error: {error.ErrorMessage}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public IEnumerable<ValidationResult> Validate()
    {
        var results = new List<ValidationResult>();

        // Validate foreign key references
        if (MatterId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Matter ID cannot be empty.",
                [nameof(MatterId)]));
        }

        if (DocumentId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Document ID cannot be empty.",
                [nameof(DocumentId)]));
        }

        if (MatterDocumentActivityId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Matter Document Activity ID cannot be empty.",
                [nameof(MatterDocumentActivityId)]));
        }

        if (UserId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "User ID cannot be empty.",
                [nameof(UserId)]));
        }

        // Validate timestamp using RevisionValidationHelper
        if (!Common.RevisionValidationHelper.IsValidDate(CreatedAt))
        {
            results.Add(new ValidationResult(
                "Created At timestamp is not within valid date range.",
                [nameof(CreatedAt)]));
        }

        // Validate activity appropriateness (requires source user for complete audit trail)
        if (MatterDocumentActivity == null) return results;
        var hasFromUser = UserId != Guid.Empty;
        if (!Common.MatterDocumentActivityValidationHelper.IsActivityAppropriateForContext(
                MatterDocumentActivity.Activity, hasFromUser, false))
        {
            results.Add(new ValidationResult(
                $"Activity '{MatterDocumentActivity.Activity}' requires both source and destination users for complete audit trails.",
                [nameof(MatterDocumentActivityId), nameof(UserId)]));
        }

        return results;
    }

    #endregion Validation Methods
}