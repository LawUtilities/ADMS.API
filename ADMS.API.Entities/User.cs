using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ADMS.API.Entities;

/// <summary>
/// Represents a user in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// The User entity serves as the central identity for all user-related operations within the ADMS system.
/// Users are associated with various activities across documents, matters, and revisions to maintain 
/// comprehensive audit trails and ensure proper attribution of all actions within the legal document 
/// management workflow.
/// 
/// <para><strong>Key Characteristics:</strong></para>
/// <list type="bullet">
/// <item><strong>Identity Management:</strong> Each user has a unique GUID identifier and display name</item>
/// <item><strong>Professional Names:</strong> Supports full professional names with proper formatting</item>
/// <item><strong>Activity Tracking:</strong> Central point for all user-related audit trail associations</item>
/// <item><strong>Legal Compliance:</strong> Maintains user attribution for legal document management</item>
/// </list>
/// 
/// <para><strong>Relationship Overview:</strong></para>
/// <list type="bullet">
/// <item><strong>Matter Activities:</strong> Tracks user involvement in matter lifecycle events</item>
/// <item><strong>Document Activities:</strong> Records user actions on documents (create, modify, delete)</item>
/// <item><strong>Revision Activities:</strong> Maintains user attribution for document version control</item>
/// <item><strong>Matter Document Activities:</strong> Tracks directional document transfers between matters</item>
/// </list>
/// 
/// <para><strong>Database Configuration:</strong></para>
/// <list type="bullet">
/// <item>Primary key: GUID with identity generation</item>
/// <item>Name constraint: StringLength(50) with required validation</item>
/// <item>Relationships: Multiple one-to-many associations for activity tracking</item>
/// <item>Seeded data: Initial user "rbrown" for system operations</item>
/// </list>
/// 
/// <para><strong>Audit Trail Integration:</strong></para>
/// Users are central to the ADMS audit trail system, with relationships to all major activity types
/// ensuring comprehensive tracking of who performed what actions when within the legal document
/// management system. This supports legal compliance and operational transparency requirements.
/// 
/// <para><strong>Professional Usage:</strong></para>
/// The User entity supports professional legal practice requirements including proper name handling,
/// professional attribution, and comprehensive activity tracking necessary for legal document
/// management and compliance with legal practice standards.
/// </remarks>
public class User : IEquatable<User>
{
    #region Core Properties

    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    /// <remarks>
    /// This GUID serves as the primary key and is automatically generated when the user is created.
    /// The ID is used throughout the system to establish relationships and maintain referential integrity
    /// across all user-related operations and audit trails.
    /// 
    /// <para><strong>Database Configuration:</strong></para>
    /// <list type="bullet">
    /// <item>Primary key with identity generation</item>
    /// <item>Non-nullable and required for all operations</item>
    /// <item>Used as foreign key in all user relationship tables</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var user = new User { Name = "John Doe" };
    /// // ID will be automatically generated when saved to database
    /// // Example: "12345678-1234-5678-9012-123456789012"
    /// </code>
    /// </example>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    /// <remarks>
    /// The user's name serves as the primary display identifier throughout the ADMS system.
    /// This field supports professional naming conventions and is used for user identification
    /// in audit trails, activity records, and system displays.
    /// 
    /// <para><strong>Validation Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Required field - cannot be null or empty</item>
    /// <item>Maximum length: 50 characters (database constraint)</item>
    /// <item>Minimum length: 2 characters (business rule)</item>
    /// <item>Supports professional names with spaces, periods, hyphens</item>
    /// <item>Must not conflict with reserved system names</item>
    /// </list>
    /// 
    /// <para><strong>Professional Support:</strong></para>
    /// <list type="bullet">
    /// <item>Full names: "John Doe", "Dr. Smith", "Mary Johnson-Brown"</item>
    /// <item>Professional titles: "J. Smith", "M.D. Johnson"</item>
    /// <item>International names: Supports various naming conventions</item>
    /// <item>Case preservation: Maintains professional formatting</item>
    /// </list>
    /// 
    /// <para><strong>Usage in System:</strong></para>
    /// The name appears in audit logs, activity records, user interfaces, and reports throughout
    /// the ADMS system. It should be professional, clearly identifiable, and appropriate for
    /// legal document management contexts.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Professional name examples
    /// user.Name = "John Doe";              // Standard full name
    /// user.Name = "Dr. Jane Smith";        // With professional title
    /// user.Name = "M. Johnson-Brown";      // Abbreviated with hyphen
    /// user.Name = "Robert Brown III";      // With suffix
    /// </code>
    /// </example>
    [Required(ErrorMessage = "User name is required and cannot be empty.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "User name must be between 2 and 50 characters.")]
    public required string Name { get; set; }

    #endregion Core Properties

    #region Activity Relationship Collections

    /// <summary>
    /// Gets or sets the collection of matter activity user associations for this user.
    /// </summary>
    /// <remarks>
    /// This collection maintains the many-to-many relationship between users and matter activities,
    /// providing a complete audit trail of user involvement in matter lifecycle events.
    /// 
    /// <para><strong>Activity Types Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> User created a new matter</item>
    /// <item><strong>ARCHIVED:</strong> User archived a matter</item>
    /// <item><strong>DELETED:</strong> User deleted a matter</item>
    /// <item><strong>RESTORED:</strong> User restored a deleted matter</item>
    /// <item><strong>UNARCHIVED:</strong> User unarchived a matter</item>
    /// <item><strong>VIEWED:</strong> User viewed a matter</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Significance:</strong></para>
    /// Each association includes a timestamp (CreatedAt) providing chronological tracking of
    /// user activities on matters, essential for legal compliance and operational oversight.
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// Configured in AdmsContext with composite primary key including MatterId, MatterActivityId,
    /// UserId, and CreatedAt to ensure unique activity records while allowing multiple activities
    /// of the same type over time.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Accessing user's matter activities
    /// foreach (var activity in user.MatterActivityUsers)
    /// {
    ///     Console.WriteLine($"User performed {activity.MatterActivity?.Activity} on matter {activity.Matter?.Description} at {activity.CreatedAt}");
    /// }
    /// </code>
    /// </example>
    public virtual ICollection<MatterActivityUser> MatterActivityUsers { get; set; } = new HashSet<MatterActivityUser>();

    /// <summary>
    /// Gets or sets the collection of document activity user associations for this user.
    /// </summary>
    /// <remarks>
    /// This collection tracks all document-related activities performed by the user, maintaining
    /// a comprehensive audit trail of document operations essential for legal document management.
    /// 
    /// <para><strong>Activity Types Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> User created a new document</item>
    /// <item><strong>SAVED:</strong> User saved changes to a document</item>
    /// <item><strong>DELETED:</strong> User deleted a document</item>
    /// <item><strong>RESTORED:</strong> User restored a deleted document</item>
    /// <item><strong>CHECKED IN:</strong> User checked in a document</item>
    /// <item><strong>CHECKED OUT:</strong> User checked out a document</item>
    /// </list>
    /// 
    /// <para><strong>Legal Compliance:</strong></para>
    /// Document activities are critical for legal compliance, providing evidence of who
    /// performed what actions on legal documents and when, supporting audit requirements
    /// and professional responsibility standards.
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// Configured with composite primary key including DocumentId, DocumentActivityId,
    /// UserId, and CreatedAt, ensuring precise tracking of all document operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Finding user's recent document activities
    /// var recentDocumentActivities = user.DocumentActivityUsers
    ///     .Where(da => da.CreatedAt >= DateTime.UtcNow.AddDays(-30))
    ///     .OrderByDescending(da => da.CreatedAt);
    /// </code>
    /// </example>
    public virtual ICollection<DocumentActivityUser> DocumentActivityUsers { get; set; } = new HashSet<DocumentActivityUser>();

    /// <summary>
    /// Gets or sets the collection of revision activity user associations for this user.
    /// </summary>
    /// <remarks>
    /// This collection maintains the relationship between users and document revision activities,
    /// providing detailed version control audit trails essential for legal document management
    /// and compliance with professional standards.
    /// 
    /// <para><strong>Activity Types Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> User created a new document revision</item>
    /// <item><strong>SAVED:</strong> User saved changes to a revision</item>
    /// <item><strong>DELETED:</strong> User deleted a revision</item>
    /// <item><strong>RESTORED:</strong> User restored a deleted revision</item>
    /// </list>
    /// 
    /// <para><strong>Version Control Integration:</strong></para>
    /// Revision activities provide granular tracking of document version changes, enabling
    /// comprehensive version control and change management for legal documents where
    /// maintaining accurate revision histories is critical.
    /// 
    /// <para><strong>Professional Practice Support:</strong></para>
    /// The detailed revision tracking supports professional practice requirements for
    /// maintaining accurate records of document changes, author attribution, and
    /// chronological development of legal documents.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Tracking user's revision activities for audit
    /// var userRevisionWork = user.RevisionActivityUsers
    ///     .GroupBy(ra => ra.Revision?.Document?.FileName)
    ///     .Select(g => new { Document = g.Key, RevisionCount = g.Count() });
    /// </code>
    /// </example>
    public virtual ICollection<RevisionActivityUser> RevisionActivityUsers { get; set; } = new HashSet<RevisionActivityUser>();

    /// <summary>
    /// Gets or sets the collection of "from" matter document activity user associations for this user.
    /// </summary>
    /// <remarks>
    /// This collection tracks instances where this user was the source (initiator) of document
    /// transfer operations between matters, such as moves or copies. This directional tracking
    /// is essential for maintaining comprehensive audit trails of document provenance.
    /// 
    /// <para><strong>Transfer Operations Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> User moved document from one matter to another</item>
    /// <item><strong>COPIED:</strong> User copied document from one matter to another</item>
    /// </list>
    /// 
    /// <para><strong>Audit Trail Significance:</strong></para>
    /// The "from" association identifies the user who initiated the transfer, providing
    /// accountability and traceability for document movements between matters, which is
    /// critical for legal document management and compliance.
    /// 
    /// <para><strong>Directional Relationships:</strong></para>
    /// Works in conjunction with MatterDocumentActivityUsersTo to provide complete
    /// bidirectional tracking of document transfers, ensuring full audit trail coverage
    /// of document movement operations.
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// Configured with NoAction cascade delete to prevent cascading deletions that could
    /// compromise audit trail integrity. Uses composite primary key for precise tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Finding documents this user has moved or copied from matters
    /// var documentsMovedByUser = user.MatterDocumentActivityUsersFrom
    ///     .Where(mdau => mdau.MatterDocumentActivity?.Activity == "MOVED")
    ///     .Select(mdau => new { 
    ///         Document = mdau.Document?.FileName, 
    ///         FromMatter = mdau.Matter?.Description,
    ///         When = mdau.CreatedAt 
    ///     });
    /// </code>
    /// </example>
    public virtual ICollection<MatterDocumentActivityUserFrom> MatterDocumentActivityUsersFrom { get; set; } = new HashSet<MatterDocumentActivityUserFrom>();

    /// <summary>
    /// Gets or sets the collection of "to" matter document activity user associations for this user.
    /// </summary>
    /// <remarks>
    /// This collection tracks instances where this user was the recipient (destination) of document
    /// transfer operations between matters. This complements the "from" tracking to provide
    /// complete bidirectional audit trails for document transfers.
    /// 
    /// <para><strong>Transfer Operations Tracked:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> User received document moved from another matter</item>
    /// <item><strong>COPIED:</strong> User received document copied from another matter</item>
    /// </list>
    /// 
    /// <para><strong>Bidirectional Tracking:</strong></para>
    /// While MatterDocumentActivityUsersFrom tracks who initiated transfers, this collection
    /// tracks who received them, providing complete visibility into document transfer chains
    /// and supporting comprehensive audit requirements.
    /// 
    /// <para><strong>Legal Compliance:</strong></para>
    /// The bidirectional tracking ensures complete accountability for document transfers,
    /// supporting legal practice requirements for maintaining accurate records of document
    /// custody and access chains.
    /// 
    /// <para><strong>Entity Framework Configuration:</strong></para>
    /// Mirrors the "from" configuration with NoAction cascade delete and composite primary key
    /// to maintain audit trail integrity while providing precise transfer tracking.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Finding documents received by this user from other matters
    /// var documentsReceivedByUser = user.MatterDocumentActivityUsersTo
    ///     .Where(mdau => mdau.MatterDocumentActivity?.Activity == "COPIED")
    ///     .Select(mdau => new { 
    ///         Document = mdau.Document?.FileName, 
    ///         ToMatter = mdau.Matter?.Description,
    ///         When = mdau.CreatedAt 
    ///     });
    /// </code>
    /// </example>
    public virtual ICollection<MatterDocumentActivityUserTo> MatterDocumentActivityUsersTo { get; set; } = new HashSet<MatterDocumentActivityUserTo>();

    #endregion Activity Relationship Collections

    #region Computed Properties

    /// <summary>
    /// Gets the total count of activities performed by this user across all activity types.
    /// </summary>
    /// <remarks>
    /// This computed property provides a quick overview of user activity levels within the system,
    /// useful for activity monitoring, user engagement tracking, and system usage analytics.
    /// 
    /// <para><strong>Activity Types Included:</strong></para>
    /// <list type="bullet">
    /// <item>Matter activities (create, archive, delete, etc.)</item>
    /// <item>Document activities (create, save, check in/out, etc.)</item>
    /// <item>Revision activities (create, save, delete, restore)</item>
    /// <item>Matter document transfer activities (move, copy)</item>
    /// </list>
    /// 
    /// <para><strong>Performance Note:</strong></para>
    /// This property triggers database queries to count related entities. Consider using
    /// explicit loading or projections when working with multiple users to avoid N+1 queries.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"User {user.Name} has performed {user.TotalActivityCount} activities");
    /// </code>
    /// </example>
    [NotMapped]
    public int TotalActivityCount =>
        MatterActivityUsers.Count +
        DocumentActivityUsers.Count +
        RevisionActivityUsers.Count +
        MatterDocumentActivityUsersFrom.Count +
        MatterDocumentActivityUsersTo.Count;

    /// <summary>
    /// Gets a value indicating whether this user has any recorded activities.
    /// </summary>
    /// <remarks>
    /// This property is useful for determining if a user can be safely removed from the system
    /// or if doing so would compromise audit trail integrity. Users with activities should
    /// typically be deactivated rather than deleted to preserve audit trails.
    /// 
    /// <para><strong>Audit Trail Preservation:</strong></para>
    /// Users with activities represent important audit trail data that should be preserved
    /// for legal compliance and operational transparency requirements.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (user.HasActivities)
    /// {
    ///     // Deactivate user rather than delete to preserve audit trail
    ///     user.IsActive = false;
    /// }
    /// else
    /// {
    ///     // Safe to delete - no audit trail impact
    ///     context.Users.Remove(user);
    /// }
    /// </code>
    /// </example>
    [NotMapped]
    public bool HasActivities => TotalActivityCount > 0;

    /// <summary>
    /// Gets the normalized version of the user's name for comparison and search operations.
    /// </summary>
    /// <remarks>
    /// This property provides a normalized version of the user name suitable for case-insensitive
    /// comparisons, search operations, and uniqueness validation. The normalization follows
    /// the patterns established in UserValidationHelper.
    /// 
    /// <para><strong>Normalization Rules:</strong></para>
    /// <list type="bullet">
    /// <item>Trims leading and trailing whitespace</item>
    /// <item>Collapses multiple consecutive spaces to single spaces</item>
    /// <item>Preserves case for professional appearance</item>
    /// <item>Returns null for invalid or empty names</item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
    /// <list type="bullet">
    /// <item>User search and filtering operations</item>
    /// <item>Uniqueness validation during user creation</item>
    /// <item>Name comparison operations</item>
    /// <item>Data cleanup and migration operations</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var user1 = new User { Name = "  John   Doe  " };
    /// var user2 = new User { Name = "John Doe" };
    /// 
    /// // Both will have the same normalized name: "John Doe"
    /// bool areEquivalent = user1.NormalizedName == user2.NormalizedName; // true
    /// </code>
    /// </example>
    [NotMapped]
    public string? NormalizedName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
                return null;

            var trimmed = Name.Trim();
            if (string.IsNullOrEmpty(trimmed))
                return null;

            // Collapse multiple spaces to single spaces (matches UserValidationHelper logic)
            return System.Text.RegularExpressions.Regex.Replace(trimmed, @"\s+", " ");
        }
    }

    #endregion Computed Properties

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified User is equal to the current User.
    /// </summary>
    /// <param name="other">The User to compare with the current User.</param>
    /// <returns>true if the specified User is equal to the current User; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the Id property, as each user has a unique identifier.
    /// This follows Entity Framework best practices for entity equality comparison.
    /// </remarks>
    public bool Equals(User? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Id != Guid.Empty;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current User.
    /// </summary>
    /// <param name="obj">The object to compare with the current User.</param>
    /// <returns>true if the specified object is equal to the current User; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as User);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current User.</returns>
    /// <remarks>
    /// The hash code is based on the Id property to ensure consistent hashing behavior
    /// that aligns with the equality implementation.
    /// </remarks>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Determines whether two User instances are equal.
    /// </summary>
    /// <param name="left">The first User to compare.</param>
    /// <param name="right">The second User to compare.</param>
    /// <returns>true if the Users are equal; otherwise, false.</returns>
    public static bool operator ==(User? left, User? right) => EqualityComparer<User>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two User instances are not equal.
    /// </summary>
    /// <param name="left">The first User to compare.</param>
    /// <param name="right">The second User to compare.</param>
    /// <returns>true if the Users are not equal; otherwise, false.</returns>
    public static bool operator !=(User? left, User? right) => !(left == right);

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a string representation of the User.
    /// </summary>
    /// <returns>A string that represents the current User.</returns>
    /// <remarks>
    /// The string representation includes both the user's name and ID for identification
    /// purposes, which is useful for debugging and logging operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var user = new User { Id = Guid.Parse("12345678-1234-5678-9012-123456789012"), Name = "John Doe" };
    /// Console.WriteLine(user); // Output: "John Doe (12345678-1234-5678-9012-123456789012)"
    /// </code>
    /// </example>
    public override string ToString() => $"{Name} ({Id})";

    #endregion String Representation
}