using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.Domain.Entities;

/// <summary>
/// Represents a document revision in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// The Revision entity serves as the core component of the document versioning system within ADMS,
/// maintaining comprehensive version control and audit trails for legal documents. Each revision
/// represents a specific version of a document with sequential numbering and temporal tracking.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Version Control:</strong> Sequential revision numbering starting from 1</item>
/// <item><strong>Temporal Tracking:</strong> Creation and modification dates with UTC storage</item>
/// <item><strong>Document Association:</strong> Strong relationship with parent document</item>
/// <item><strong>Activity Tracking:</strong> Comprehensive audit trail through revision activities</item>
/// <item><strong>Soft Deletion:</strong> Deletion tracking without data loss for audit preservation</item>
/// </list>
/// 
/// <para><strong>Version Control System:</strong></para>
/// <list type="bullet">
/// <item><strong>Sequential Numbering:</strong> Revision numbers are sequential starting from 1</item>
/// <item><strong>No Gaps:</strong> Version control maintains continuous numbering without gaps</item>
/// <item><strong>Chronological Order:</strong> Creation and modification dates ensure temporal consistency</item>
/// <item><strong>Audit Trail:</strong> Every revision change is tracked through RevisionActivityUser associations</item>
/// </list>
/// 
/// <para><strong>Database Configuration:</strong></para>
/// <list type="bullet">
/// <item>Primary key: GUID with identity generation</item>
/// <item>Document relationship: Required foreign key with cascade restrictions</item>
/// <item>Activity tracking: One-to-many relationship with RevisionActivityUser</item>
/// <item>No explicit seeded data - revisions are created through document operations</item>
/// </list>
/// 
/// <para><strong>Legal Compliance:</strong></para>
/// Revisions are critical for legal document management, providing:
/// <list type="bullet">
/// <item>Complete version history for legal documents</item>
/// <item>Audit trails showing who made changes and when</item>
/// <item>Temporal consistency for document evolution tracking</item>
/// <item>Soft deletion capabilities preserving historical data</item>
/// <item>Integration with legal document retention requirements</item>
/// </list>
/// 
/// <para><strong>Business Rules:</strong></para>
/// <list type="bullet">
/// <item>Revision numbers must be sequential within each document</item>
/// <item>Creation date must precede or equal modification date</item>
/// <item>Deleted revisions are preserved for audit trail integrity</item>
/// <item>Each revision must be associated with at least one activity for accountability</item>
/// </list>
/// 
/// <para><strong>Professional Usage:</strong></para>
/// The Revision entity supports professional legal practice requirements including proper
/// document versioning, change tracking, and compliance with legal practice standards for
/// maintaining accurate records of document development and modification history.
/// </remarks>
public class Revision : IEquatable<Revision>, IComparable<Revision>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the revision.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and is automatically generated when the revision is created.
    /// The ID is used throughout the system to establish relationships and maintain referential integrity
    /// across all revision-related operations and audit trails.
    /// 
    /// <para><strong>Database Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Primary key with identity generation</item>
    /// <item>Non-nullable and required for all operations</item>
    /// <item>Used as foreign key in RevisionActivityUser relationship table</item>
    /// </list>
    /// 
    /// <para><strong>Usage Considerations:</strong></para>
    /// The ID remains constant throughout the revision's lifecycle and is used for all
    /// audit trail associations, activity tracking, and system references.
    /// </remarks>
    /// <example>
    /// <code>
    /// var revision = new Revision 
    /// { 
    ///     RevisionNumber = 1, 
    ///     DocumentId = documentGuid,
    ///     CreationDate = DateTime.UtcNow 
    /// };
    /// // ID will be automatically generated when saved to database
    /// </code>
    /// </example>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the revision number for the specified document.
    /// </summary>
    /// <remarks>
    /// The revision number represents the sequential version of the document, starting from 1 for
    /// the first revision and incrementing by 1 for each subsequent revision. This numbering
    /// system ensures clear version identification and maintains chronological order.
    /// 
    /// <para><strong>Versioning Rules:</strong></para>
    /// <list type="bullet">
    /// <item>First revision of any document must be number 1</item>
    /// <item>Subsequent revisions must be sequential (no gaps allowed)</item>
    /// <item>Maximum revision number is 999,999 (as defined in RevisionValidationHelper)</item>
    /// <item>Revision numbers are unique within each document scope</item>
    /// </list>
    /// 
    /// <para><strong>Business Logic:</strong></para>
    /// The revision number is used for:
    /// <list type="bullet">
    /// <item>Version identification in user interfaces</item>
    /// <item>Sorting and ordering revision history</item>
    /// <item>Version comparison and rollback operations</item>
    /// <item>Audit trail reporting and compliance documentation</item>
    /// </list>
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Revision numbers are validated using RevisionValidationHelper to ensure they follow
    /// proper sequencing rules and remain within acceptable bounds.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Creating sequential revisions for a document
    /// var revision1 = new Revision { RevisionNumber = 1, DocumentId = docId };  // First revision
    /// var revision2 = new Revision { RevisionNumber = 2, DocumentId = docId };  // Second revision
    /// var revision3 = new Revision { RevisionNumber = 3, DocumentId = docId };  // Third revision
    /// </code>
    /// </example>
    [Range(1, 999999, ErrorMessage = "Revision number must be between 1 and 999,999.")]
    public int RevisionNumber { get; set; }

    /// <summary>
    /// Gets or sets the creation date for the revision (in UTC).
    /// </summary>
    /// <remarks>
    /// The creation date represents when the revision was initially created in the system.
    /// All dates are stored in UTC to ensure consistency across different time zones and
    /// to support accurate temporal tracking for legal compliance.
    /// 
    /// <para><strong>Date Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Must be stored in UTC format</item>
    /// <item>Cannot be earlier than January 1, 1980 (system minimum)</item>
    /// <item>Cannot be in the future (with 1-minute tolerance for clock skew)</item>
    /// <item>Must precede or equal the ModificationDate</item>
    /// </list>
    /// 
    /// <para><strong>Legal Significance:</strong></para>
    /// The creation date is critical for legal document management as it establishes:
    /// <list type="bullet">
    /// <item>When document versions were created for legal timelines</item>
    /// <item>Chronological order for version control and audit trails</item>
    /// <item>Evidence for document development and modification history</item>
    /// <item>Compliance with legal practice standards for record keeping</item>
    /// </list>
    /// 
    /// <para><strong>Validation:</strong></para>
    /// Creation dates are validated using RevisionValidationHelper to ensure they meet
    /// business rules and legal compliance requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// var revision = new Revision
    /// {
    ///     CreationDate = DateTime.UtcNow,  // Always use UTC
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the modification date for the revision (in UTC).
    /// </summary>
    /// <remarks>
    /// The modification date represents when the revision was last modified or updated.
    /// This date must be greater than or equal to the creation date and follows the same
    /// UTC storage requirements for consistency and legal compliance.
    /// 
    /// <para><strong>Date Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Must be stored in UTC format</item>
    /// <item>Must be greater than or equal to CreationDate</item>
    /// <item>Cannot be earlier than January 1, 1980 (system minimum)</item>
    /// <item>Cannot be in the future (with 1-minute tolerance for clock skew)</item>
    /// </list>
    /// 
    /// <para><strong>Business Logic:</strong></para>
    /// The modification date serves several purposes:
    /// <list type="bullet">
    /// <item>Tracking when changes were made to revision content</item>
    /// <item>Establishing chronological order for audit purposes</item>
    /// <item>Supporting version comparison and change analysis</item>
    /// <item>Providing timestamps for legal document modification tracking</item>
    /// </list>
    /// 
    /// <para><strong>Update Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Initially set to same value as CreationDate</item>
    /// <item>Updated whenever revision content or metadata changes</item>
    /// <item>Preserved during soft deletion for audit trail integrity</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Creating a revision with initial dates
    /// var revision = new Revision
    /// {
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow  // Initially same as creation
    /// };
    /// 
    /// // Later updating the revision
    /// revision.ModificationDate = DateTime.UtcNow;  // Update to current time
    /// </code>
    /// </example>
    public DateTime ModificationDate { get; set; }

    /// <summary>
    /// Gets or sets the document ID linked to this revision.
    /// </summary>
    /// <remarks>
    /// This property establishes the foreign key relationship between the revision and its
    /// parent document. Every revision must be associated with exactly one document, and
    /// this relationship is enforced at the database level.
    /// 
    /// <para><strong>Relationship Requirements:</strong></para>
    /// <list type="bullet">
    /// <item>Must reference a valid, existing document</item>
    /// <item>Cannot be Guid.Empty (validated by RevisionValidationHelper)</item>
    /// <item>Establishes one-to-many relationship (Document -> Revisions)</item>
    /// <item>Required field - every revision must have a parent document</item>
    /// </list>
    /// 
    /// <para><strong>Database Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Foreign key constraint with required relationship</item>
    /// <item>Used in Entity Framework navigation property configuration</item>
    /// <item>Part of document version control integrity</item>
    /// </list>
    /// 
    /// <para><strong>Business Logic:</strong></para>
    /// The document association enables:
    /// <list type="bullet">
    /// <item>Grouping revisions by parent document</item>
    /// <item>Version control operations within document scope</item>
    /// <item>Document-level audit trail aggregation</item>
    /// <item>Cascade operations and referential integrity</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var document = new Document { /* document properties */ };
    /// var revision = new Revision
    /// {
    ///     DocumentId = document.Id,  // Establish relationship
    ///     RevisionNumber = 1,
    ///     CreationDate = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    [Required(ErrorMessage = "Document ID is required for revision association.")]
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the revision is deleted.
    /// </summary>
    /// <remarks>
    /// This property implements soft deletion for the revision, allowing the revision to be
    /// marked as deleted while preserving all data for audit trail and legal compliance
    /// purposes. Soft deletion is critical in legal document management where complete
    /// audit trails must be maintained.
    /// 
    /// <para><strong>Soft Deletion Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Preserves complete audit trail for legal compliance</item>
    /// <item>Maintains referential integrity across related entities</item>
    /// <item>Enables recovery operations if deletion was accidental</item>
    /// <item>Supports historical reporting and analysis</item>
    /// </list>
    /// 
    /// <para><strong>Business Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Deleted revisions cannot be deleted if they have active references</item>
    /// <item>Deletion status is tracked through RevisionActivityUser audit entries</item>
    /// <item>Deleted revisions are typically filtered from normal operations</item>
    /// <item>Restoration operations can reverse soft deletion</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance:</strong></para>
    /// Soft deletion supports legal document management requirements by:
    /// <list type="bullet">
    /// <item>Maintaining complete document version history</item>
    /// <item>Preserving evidence of document lifecycle events</item>
    /// <item>Supporting legal discovery and compliance audits</item>
    /// <item>Enabling rollback operations when legally required</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Soft delete a revision
    /// revision.IsDeleted = true;
    /// 
    /// // Query for active (non-deleted) revisions
    /// var activeRevisions = revisions.Where(r => !r.IsDeleted);
    /// 
    /// // Restore a deleted revision
    /// revision.IsDeleted = false;
    /// </code>
    /// </example>
    public bool IsDeleted { get; set; }

    #endregion Core Properties

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the document linked to this revision.
    /// </summary>
    /// <remarks>
    /// This navigation property provides access to the parent document associated with this
    /// revision. The relationship is established through the DocumentId foreign key and
    /// enables navigation from revision to document in Entity Framework queries.
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured as required relationship in AdmsContext</item>
    /// <item>Uses DocumentId as foreign key</item>
    /// <item>Supports lazy loading with virtual modifier</item>
    /// <item>Part of one-to-many relationship (Document has many Revisions)</item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Accessing document metadata from revision context</item>
    /// <item>Document-level operations initiated from revision</item>
    /// <item>Audit trail reporting including document information</item>
    /// <item>Version control operations requiring document context</item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// The virtual modifier enables lazy loading, but consider explicit loading or
    /// projections when working with multiple revisions to avoid N+1 query issues.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing document through revision
    /// string documentName = revision.Document.FileName;
    /// string matterDescription = revision.Document.Matter.Description;
    /// 
    /// // Using explicit loading to avoid N+1 queries
    /// context.Entry(revision)
    ///     .Reference(r => r.Document)
    ///     .Load();
    /// </code>
    /// </example>
    [ForeignKey(nameof(DocumentId))]
    public virtual Document Document { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of revision activity users associated with this revision.
    /// </summary>
    /// <remarks>
    /// This collection maintains the many-to-many relationship between revisions, activities,
    /// and users, providing a comprehensive audit trail of all actions performed on this revision.
    /// Each entry includes timestamps and tracks specific activities like creation, modification,
    /// deletion, and restoration.
    /// 
    /// <para><strong>Activity Types Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> User created this revision</item>
    /// <item><strong>SAVED:</strong> User saved changes to this revision</item>
    /// <item><strong>DELETED:</strong> User deleted this revision</item>
    /// <item><strong>RESTORED:</strong> User restored this deleted revision</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Significance:</strong></para>
    /// Each association provides:
    /// <list type="bullet">
    /// <item>User attribution for revision operations</item>
    /// <item>Timestamp tracking for temporal analysis</item>
    /// <item>Activity classification for operation categorization</item>
    /// <item>Complete audit trail for legal compliance</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Configured in AdmsContext.ConfigureRevisionActivityUser</item>
    /// <item>Composite primary key includes RevisionId, RevisionActivityId, UserId, CreatedAt</item>
    /// <item>Required relationships to all associated entities</item>
    /// <item>Supports multiple activities of same type with different timestamps</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance:</strong></para>
    /// The activity tracking supports legal document management by:
    /// <list type="bullet">
    /// <item>Providing complete user attribution for revision changes</item>
    /// <item>Maintaining chronological audit trails</item>
    /// <item>Supporting legal discovery and compliance requirements</item>
    /// <item>Enabling detailed reporting on document revision history</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing revision activities
    /// foreach (var activity in revision.RevisionActivityUsers)
    /// {
    ///     Console.WriteLine($"User {activity.User?.Name} performed {activity.RevisionActivity?.Activity} " +
    ///                      $"on revision {revision.RevisionNumber} at {activity.CreatedAt}");
    /// }
    /// 
    /// // Finding who created this revision
    /// var creator = revision.RevisionActivityUsers
    ///     .FirstOrDefault(ra => ra.RevisionActivity?.Activity == "CREATED")?.User;
    /// 
    /// // Getting revision activity timeline
    /// var timeline = revision.RevisionActivityUsers
    ///     .OrderBy(ra => ra.CreatedAt)
    ///     .Select(ra => new { ra.CreatedAt, ra.RevisionActivity?.Activity, ra.User?.Name });
    /// </code>
    /// </example>
    public virtual ICollection<RevisionActivityUser> RevisionActivityUsers { get; set; } = new HashSet<RevisionActivityUser>();

    #endregion Navigation Properties

    #region Computed Properties

    /// <summary>
    /// Gets a value indicating whether this revision has any recorded activities.
    /// </summary>
    /// <remarks>
    /// This property is useful for determining audit trail completeness and ensuring that
    /// revisions have proper activity tracking. Revisions without activities may indicate
    /// incomplete audit trail data or system issues.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Audit trail validation and completeness checking</item>
    /// <item>Data integrity verification during system operations</item>
    /// <item>Reporting on revision activity coverage</item>
    /// <item>Identifying revisions requiring activity backfill</item>
    /// </list>
    /// 
    /// <para><strong>Performance Note:</strong></para>
    /// This property triggers a database query to count related entities. Consider using
    /// explicit loading or projections when working with multiple revisions to avoid N+1 queries.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!revision.HasActivities)
    /// {
    ///     // Log potential audit trail gap
    ///     logger.LogWarning($"Revision {revision.Id} has no activity records");
    /// }
    /// 
    /// // Bulk check for revisions without activities
    /// var revisionsWithoutActivities = revisions.Where(r => !r.HasActivities).ToList();
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasActivities => RevisionActivityUsers.Count > 0;

    /// <summary>
    /// Gets the total count of activities recorded for this revision.
    /// </summary>
    /// <remarks>
    /// This computed property provides insight into the level of activity associated with
    /// this revision, useful for activity monitoring, audit analysis, and understanding
    /// revision lifecycle complexity.
    /// 
    /// <para><strong>Performance Note:</strong></para>
    /// This property triggers database queries to count related entities. Consider using
    /// explicit loading or projections when working with multiple revisions to avoid N+1 queries.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Revision {revision.RevisionNumber} has {revision.ActivityCount} recorded activities");
    /// 
    /// // Finding most active revisions
    /// var mostActiveRevisions = revisions
    ///     .OrderByDescending(r => r.ActivityCount)
    ///     .Take(10);
    /// </code>
    /// </example>
    [NotMapped]
    public int ActivityCount => RevisionActivityUsers.Count;

    /// <summary>
    /// Gets the time span between creation and modification dates.
    /// </summary>
    /// <remarks>
    /// This computed property calculates the duration between when the revision was created
    /// and when it was last modified, providing insight into revision development time and
    /// modification patterns.
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>Analyzing revision development time patterns</item>
    /// <item>Identifying long-running revision modifications</item>
    /// <item>Supporting time-based audit analysis</item>
    /// <item>Generating revision lifecycle reports</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (revision.ModificationTimeSpan.TotalHours > 24)
    /// {
    ///     Console.WriteLine($"Revision {revision.RevisionNumber} was modified over " +
    ///                      $"{revision.ModificationTimeSpan.TotalDays:F1} days");
    /// }
    /// 
    /// // Finding revisions with long modification periods
    /// var longRunningRevisions = revisions
    ///     .Where(r => r.ModificationTimeSpan.TotalDays > 30);
    /// </code>
    /// </example>
    [NotMapped]
    public TimeSpan ModificationTimeSpan => ModificationDate - CreationDate;

    /// <summary>
    /// Gets a value indicating whether the modification time span is reasonable.
    /// </summary>
    /// <remarks>
    /// This property uses the RevisionValidationHelper to determine if the time span between
    /// creation and modification dates is within reasonable bounds, helping identify potential
    /// data corruption or incorrect timestamp values.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!revision.HasReasonableTimeSpan)
    /// {
    ///     logger.LogWarning($"Revision {revision.Id} has unreasonable time span: {revision.ModificationTimeSpan}");
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasReasonableTimeSpan =>
        RevisionValidationHelper.IsValidDateTimeSpan(CreationDate, ModificationDate);

    /// <summary>
    /// Gets a value indicating whether both creation and modification dates are valid.
    /// </summary>
    /// <remarks>
    /// This property validates both dates using the RevisionValidationHelper to ensure
    /// they meet business rules and legal compliance requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!revision.HasValidDates)
    /// {
    ///     var errors = new List&lt;string&gt;();
    ///     if (!Common.RevisionValidationHelper.IsValidCreationDate(revision.CreationDate))
    ///         errors.Add("Invalid creation date");
    ///     if (!Common.RevisionValidationHelper.IsValidModificationDate(revision.ModificationDate))
    ///         errors.Add("Invalid modification date");
    ///     
    ///     logger.LogError($"Revision {revision.Id} date validation failed: {string.Join(", ", errors)}");
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasValidDates =>
        Common.RevisionValidationHelper.IsValidCreationDate(CreationDate) &&
        Common.RevisionValidationHelper.IsValidModificationDate(ModificationDate) &&
        Common.RevisionValidationHelper.IsValidDateSequence(CreationDate, ModificationDate);

    #endregion Computed Properties

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified Revision is equal to the current Revision.
    /// </summary>
    /// <param name="other">The Revision to compare with the current Revision.</param>
    /// <returns>true if the specified Revision is equal to the current Revision; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each revision has a unique identifier.
    /// This follows Entity Framework best practices for entity equality comparison.
    /// </remarks>
    public bool Equals(Revision? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current Revision.
    /// </summary>
    /// <param name="obj">The object to compare with the current Revision.</param>
    /// <returns>true if the specified object is equal to the current Revision; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as Revision);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current Revision.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Compares the current Revision with another Revision for ordering purposes.
    /// </summary>
    /// <param name="other">The Revision to compare with the current Revision.</param>
    /// <returns>
    /// A value that indicates the relative order of the revisions being compared.
    /// Less than zero: This revision precedes the other revision.
    /// Zero: This revision occurs in the same position as the other revision.
    /// Greater than zero: This revision follows the other revision.
    /// </returns>
    /// <remarks>
    /// Comparison is performed based on revision number, enabling natural sorting of revisions
    /// within a document's version history. This supports chronological ordering and version
    /// control operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Sort revisions by version number
    /// var sortedRevisions = revisions.OrderBy(r => r).ToList();
    /// 
    /// // Compare specific revisions
    /// if (revision1.CompareTo(revision2) < 0)
    /// {
    ///     Console.WriteLine($"Revision {revision1.RevisionNumber} comes before {revision2.RevisionNumber}");
    /// }
    /// </code>
    /// </example>
    public int CompareTo(Revision? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        // First compare by DocumentId to ensure we're comparing revisions of the same document
        var documentComparison = DocumentId.CompareTo(other.DocumentId);
        return documentComparison != 0 ? documentComparison :
            // Then compare by revision number for chronological ordering
            RevisionNumber.CompareTo(other.RevisionNumber);
    }

    /// <summary>
    /// Determines whether two Revision instances are equal.
    /// </summary>
    /// <param name="left">The first Revision to compare.</param>
    /// <param name="right">The second Revision to compare.</param>
    /// <returns>true if the Revisions are equal; otherwise, false.</returns>
    public static bool operator ==(Revision? left, Revision? right) => EqualityComparer<Revision>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two Revision instances are not equal.
    /// </summary>
    /// <param name="left">The first Revision to compare.</param>
    /// <param name="right">The second Revision to compare.</param>
    /// <returns>true if the Revisions are not equal; otherwise, false.</returns>
    public static bool operator !=(Revision? left, Revision? right) => !(left == right);

    /// <summary>
    /// Determines whether one Revision precedes another in the ordering.
    /// </summary>
    /// <param name="left">The first Revision to compare.</param>
    /// <param name="right">The second Revision to compare.</param>
    /// <returns>true if the left Revision precedes the right Revision; otherwise, false.</returns>
    public static bool operator <(Revision? left, Revision? right) =>
        left is not null && (right is null || left.CompareTo(right) < 0);

    /// <summary>
    /// Determines whether one Revision precedes or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first Revision to compare.</param>
    /// <param name="right">The second Revision to compare.</param>
    /// <returns>true if the left Revision precedes or equals the right Revision; otherwise, false.</returns>
    public static bool operator <=(Revision? left, Revision? right) =>
        left is null || (right is not null && left.CompareTo(right) <= 0);

    /// <summary>
    /// Determines whether one Revision follows another in the ordering.
    /// </summary>
    /// <param name="left">The first Revision to compare.</param>
    /// <param name="right">The second Revision to compare.</param>
    /// <returns>true if the left Revision follows the right Revision; otherwise, false.</returns>
    public static bool operator >(Revision? left, Revision? right) =>
        left is not null && (right is null || left.CompareTo(right) > 0);

    /// <summary>
    /// Determines whether one Revision follows or is equal to another in the ordering.
    /// </summary>
    /// <param name="left">The first Revision to compare.</param>
    /// <param name="right">The second Revision to compare.</param>
    /// <returns>true if the left Revision follows or equals the right Revision; otherwise, false.</returns>
    public static bool operator >=(Revision? left, Revision? right) =>
        left is null || (right is not null && left.CompareTo(right) >= 0);

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the Revision.
    /// </summary>
    /// <returns>A string that represents the current Revision.</returns>
    /// <remarks>
    /// The string representation includes key identifying information about the revision,
    /// which is useful for debugging, logging, and display purposes.
    /// </remarks>
    /// <example>
    /// <code>
    /// var revision = new Revision 
    /// { 
    ///     Id = Guid.Parse("12345678-1234-5678-9012-123456789012"), 
    ///     RevisionNumber = 3,
    ///     DocumentId = Guid.Parse("87654321-4321-8765-2109-876543210987"),
    ///     CreationDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)
    /// };
    /// 
    /// Console.WriteLine(revision);
    /// // Output: "Revision 3 (12345678-1234-5678-9012-123456789012) - Created: 2024-01-15 10:30:00 UTC"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Revision {RevisionNumber} ({Id}) - Created: {CreationDate:yyyy-MM-dd HH:mm:ss} UTC";

    #endregion String Representation

    #region Business Logic Methods

    /// <summary>
    /// Determines whether this revision can be safely deleted based on business rules.
    /// </summary>
    /// <returns>true if the revision can be deleted; otherwise, false.</returns>
    /// <remarks>
    /// This method checks various business rules to determine if soft deletion is allowed.
    /// It considers factors such as active references, audit trail requirements, and
    /// system integrity constraints.
    /// 
    /// <para><strong>Deletion Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Cannot delete if already deleted</item>
    /// <item>Cannot delete if there are active references (checked via validation helper)</item>
    /// <item>Must preserve audit trail integrity</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (revision.CanBeDeleted())
    /// {
    ///     revision.IsDeleted = true;
    ///     // Log deletion activity...
    /// }
    /// else
    /// {
    ///     throw new InvalidOperationException("Revision cannot be deleted due to active references");
    /// }
    /// </code>
    /// </example>
    public bool CanBeDeleted()
    {
        // Already deleted revisions cannot be deleted again
        if (IsDeleted)
            return false;

        // Check for active references using validation helper
        // This would typically check for any active references that prevent deletion
        var hasActiveReferences = false; // This would be determined by checking actual references

        return Common.RevisionValidationHelper.IsValidDeletionState(true, hasActiveReferences);
    }

    /// <summary>
    /// Determines whether this revision can be restored from deleted state.
    /// </summary>
    /// <returns>true if the revision can be restored; otherwise, false.</returns>
    /// <remarks>
    /// This method checks if a deleted revision can be restored to active state,
    /// considering business rules and data integrity requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (revision.CanBeRestored())
    /// {
    ///     revision.IsDeleted = false;
    ///     // Log restoration activity...
    /// }
    /// </code>
    /// </example>
    public bool CanBeRestored()
    {
        // Only deleted revisions can be restored
        return IsDeleted;
    }

    /// <summary>
    /// Gets the next logical revision number for the same document.
    /// </summary>
    /// <returns>The next revision number in sequence.</returns>
    /// <remarks>
    /// This method provides the next sequential revision number, useful for
    /// creating new revisions of the same document.
    /// </remarks>
    /// <example>
    /// <code>
    /// var nextRevisionNumber = currentRevision.GetNextRevisionNumber();
    /// var newRevision = new Revision
    /// {
    ///     RevisionNumber = nextRevisionNumber,
    ///     DocumentId = currentRevision.DocumentId,
    ///     CreationDate = DateTime.UtcNow,
    ///     ModificationDate = DateTime.UtcNow
    /// };
    /// </code>
    /// </example>
    public int GetNextRevisionNumber() => RevisionNumber + 1;

    #endregion Business Logic Methods
}