using ADMS.API.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ADMS.API.DbContexts;

/// <summary>
/// Entity Framework Database Context for the ADMS (Advanced Document Management System) legal document management system.
/// </summary>
/// <remarks>
/// The AdmsContext serves as the primary data access layer for the ADMS legal document management system,
/// providing comprehensive entity configuration, relationship management, and seeded reference data essential
/// for professional legal practice document management.
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item><strong>Comprehensive Entity Management:</strong> Full support for documents, matters, users, and activities</item>
/// <item><strong>Advanced Relationship Configuration:</strong> Complex many-to-many relationships with composite keys</item>
/// <item><strong>Audit Trail Architecture:</strong> Complete audit trail support for all entity operations</item>
/// <item><strong>Seeded Reference Data:</strong> Pre-configured activities and test data for development/testing</item>
/// <item><strong>Legal Compliance:</strong> Database design supporting legal practice requirements</item>
/// <item><strong>Performance Optimization:</strong> Optimized for legal document management workloads</item>
/// </list>
/// 
/// <para><strong>Entity Architecture:</strong></para>
/// <list type="bullet">
/// <item><strong>Core Entities:</strong> Document, Matter, User for primary business objects</item>
/// <item><strong>Activity Entities:</strong> DocumentActivity, MatterActivity, RevisionActivity for operation classification</item>
/// <item><strong>Junction Entities:</strong> DocumentActivityUser, MatterActivityUser, RevisionActivityUser for audit trails</item>
/// <item><strong>Transfer Entities:</strong> MatterDocumentActivityUserFrom/To for directional document transfer tracking</item>
/// <item><strong>Version Control:</strong> Revision and RevisionActivityUser for document version management</item>
/// </list>
/// 
/// <para><strong>Audit Trail System:</strong></para>
/// The context implements a comprehensive audit trail system through junction entities that track:
/// <list type="bullet">
/// <item>Who performed each operation (User attribution)</item>
/// <item>What operation was performed (Activity classification)</item>
/// <item>When the operation occurred (Temporal tracking with UTC timestamps)</item>
/// <item>What entity was affected (Document, Matter, or Revision association)</item>
/// </list>
/// 
/// <para><strong>Database Design Principles:</strong></para>
/// <list type="bullet">
/// <item><strong>Referential Integrity:</strong> Comprehensive foreign key relationships with appropriate cascade behaviors</item>
/// <item><strong>Data Consistency:</strong> Business rules enforced at the database level where appropriate</item>
/// <item><strong>Audit Preservation:</strong> NoAction cascade deletes for audit trail entities to preserve historical data</item>
/// <item><strong>Performance:</strong> Optimized indexes and relationship configurations for legal document workflows</item>
/// </list>
/// 
/// <para><strong>Seeded Data Strategy:</strong></para>
/// <list type="bullet">
/// <item><strong>Activity Types:</strong> Predefined activities for documents, matters, and revisions</item>
/// <item><strong>Test Data:</strong> Sample matters and users for development and testing scenarios</item>
/// <item><strong>Audit Examples:</strong> Sample audit trail entries demonstrating system capabilities</item>
/// <item><strong>Consistent IDs:</strong> Predictable GUID patterns for reliable testing and development</item>
/// </list>
/// 
/// <para><strong>Professional Legal Practice Support:</strong></para>
/// <list type="bullet">
/// <item><strong>Document Management:</strong> Complete document lifecycle with version control and audit trails</item>
/// <item><strong>Matter Organization:</strong> Flexible matter-based document organization for legal cases</item>
/// <item><strong>User Attribution:</strong> Complete accountability for all system operations</item>
/// <item><strong>Compliance:</strong> Audit trail architecture supporting legal practice requirements</item>
/// </list>
/// </remarks>
/// <param name="options">The DbContext options including connection string and provider configuration.</param>
/// <example>
/// <code>
/// // Dependency injection registration
/// services.AddDbContext&lt;AdmsContext&gt;(options =&gt;
///     options.UseSqlServer(connectionString)
///            .EnableSensitiveDataLogging(isDevelopment)
///            .LogTo(Console.WriteLine, LogLevel.Information));
/// 
/// // Usage in services
/// public class DocumentService(AdmsContext context)
/// {
///     public async Task&lt;Document&gt; CreateDocumentAsync(DocumentForCreationDto dto)
///     {
///         var document = new Document { /* ... */ };
///         context.Documents.Add(document);
///         await context.SaveChangesAsync();
///         return document;
///     }
/// }
/// </code>
/// </example>
public class AdmsContext(DbContextOptions<AdmsContext> options) : DbContext(options)
{
    #region Entity Sets - Core Business Entities

    /// <summary>
    /// Gets or sets the Documents entity set for accessing document records in the database.
    /// </summary>
    /// <remarks>
    /// Provides access to document entities representing digital files stored in the ADMS system.
    /// Each document belongs to a specific matter and maintains comprehensive metadata including
    /// file information, checksums, and audit trails.
    /// 
    /// <para><strong>Key Features:</strong></para>
    /// <list type="bullet">
    /// <item>Digital file metadata storage with integrity verification</item>
    /// <item>Matter association for legal document organization</item>
    /// <item>Version control through revision tracking</item>
    /// <item>Comprehensive audit trails for all document operations</item>
    /// <item>Check-in/check-out support for collaborative editing</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Query active documents in a specific matter
    /// var matterDocuments = await context.Documents
    ///     .Where(d => d.MatterId == matterId && !d.IsDeleted)
    ///     .Include(d => d.DocumentActivityUsers)
    ///     .ToListAsync();
    /// </code>
    /// </example>
    public virtual DbSet<Document> Documents { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Matters entity set for accessing matter records in the database.
    /// </summary>
    /// <remarks>
    /// Provides access to matter entities representing legal cases, projects, or client-specific
    /// document collections within the ADMS system. Matters serve as the primary organizational
    /// unit for legal document management.
    /// 
    /// <para><strong>Key Features:</strong></para>
    /// <list type="bullet">
    /// <item>Legal case and project organization</item>
    /// <item>Client-based or matter-specific document grouping</item>
    /// <item>Lifecycle management with archival and deletion support</item>
    /// <item>Document transfer capabilities between matters</item>
    /// <item>Comprehensive activity audit trails</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Query active matters with document counts
    /// var activeMatters = await context.Matters
    ///     .Where(m => !m.IsDeleted && !m.IsArchived)
    ///     .Include(m => m.Documents.Where(d => !d.IsDeleted))
    ///     .ToListAsync();
    /// </code>
    /// </example>
    public virtual DbSet<Matter> Matters { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Users entity set for accessing user records in the database.
    /// </summary>
    /// <remarks>
    /// Provides access to user entities representing individuals who interact with the ADMS system.
    /// Users are central to the audit trail system, with all operations attributed to specific users
    /// for accountability and legal compliance purposes.
    /// 
    /// <para><strong>Key Features:</strong></para>
    /// <list type="bullet">
    /// <item>Professional user identity management</item>
    /// <item>Complete activity attribution across all system operations</item>
    /// <item>Audit trail participation for legal compliance</item>
    /// <item>User-based reporting and analytics</item>
    /// <item>Professional naming convention support</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Query user with recent activities
    /// var userWithActivities = await context.Users
    ///     .Include(u => u.DocumentActivityUsers.Take(10))
    ///     .Include(u => u.MatterActivityUsers.Take(10))
    ///     .FirstOrDefaultAsync(u => u.Id == userId);
    /// </code>
    /// </example>
    public virtual DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Revisions entity set for accessing document revision records in the database.
    /// </summary>
    /// <remarks>
    /// Provides access to revision entities representing specific versions of documents within
    /// the ADMS version control system. Revisions maintain sequential numbering and complete
    /// audit trails for legal document version management.
    /// 
    /// <para><strong>Key Features:</strong></para>
    /// <list type="bullet">
    /// <item>Sequential revision numbering for version control</item>
    /// <item>Temporal tracking of document changes</item>
    /// <item>User attribution for all revision operations</item>
    /// <item>Soft deletion with audit trail preservation</item>
    /// <item>Integration with document lifecycle management</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Query latest revision for a document
    /// var latestRevision = await context.Revisions
    ///     .Where(r => r.DocumentId == documentId && !r.IsDeleted)
    ///     .OrderByDescending(r => r.RevisionNumber)
    ///     .FirstOrDefaultAsync();
    /// </code>
    /// </example>
    public virtual DbSet<Revision> Revisions { get; set; } = null!;

    #endregion Entity Sets - Core Business Entities

    #region Entity Sets - Activity Classification

    /// <summary>
    /// Gets or sets the DocumentActivities entity set for accessing document activity type records.
    /// </summary>
    /// <remarks>
    /// Provides access to document activity entities representing standardized operations that can
    /// be performed on documents. These entities serve as lookup tables for consistent activity
    /// classification across the audit trail system.
    /// 
    /// <para><strong>Standard Activities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Document creation operations</item>
    /// <item><strong>SAVED:</strong> Document save operations</item>
    /// <item><strong>DELETED:</strong> Document deletion operations</item>
    /// <item><strong>RESTORED:</strong> Document restoration operations</item>
    /// <item><strong>CHECKED IN:</strong> Version control check-in operations</item>
    /// <item><strong>CHECKED OUT:</strong> Version control check-out operations</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get all document activities with usage statistics
    /// var activities = await context.DocumentActivities
    ///     .Include(da => da.DocumentActivityUsers)
    ///     .ToListAsync();
    /// </code>
    /// </example>
    public virtual DbSet<DocumentActivity> DocumentActivities { get; set; } = null!;

    /// <summary>
    /// Gets or sets the MatterActivities entity set for accessing matter activity type records.
    /// </summary>
    /// <remarks>
    /// Provides access to matter activity entities representing standardized operations that can
    /// be performed on legal matters. These activities support matter lifecycle management and
    /// comprehensive audit trail requirements.
    /// 
    /// <para><strong>Standard Activities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Matter creation operations</item>
    /// <item><strong>ARCHIVED:</strong> Matter archival operations</item>
    /// <item><strong>UNARCHIVED:</strong> Matter unarchival operations</item>
    /// <item><strong>DELETED:</strong> Matter deletion operations</item>
    /// <item><strong>RESTORED:</strong> Matter restoration operations</item>
    /// <item><strong>VIEWED:</strong> Matter access operations</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get creation activity for new matter audit trail
    /// var createdActivity = await context.MatterActivities
    ///     .FirstOrDefaultAsync(ma => ma.Activity == "CREATED");
    /// </code>
    /// </example>
    public virtual DbSet<MatterActivity> MatterActivities { get; set; } = null!;

    /// <summary>
    /// Gets or sets the RevisionActivities entity set for accessing revision activity type records.
    /// </summary>
    /// <remarks>
    /// Provides access to revision activity entities representing standardized operations that can
    /// be performed on document revisions. These activities support comprehensive version control
    /// audit trails and compliance requirements.
    /// 
    /// <para><strong>Standard Activities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED:</strong> Revision creation operations</item>
    /// <item><strong>SAVED:</strong> Revision save operations</item>
    /// <item><strong>DELETED:</strong> Revision deletion operations</item>
    /// <item><strong>RESTORED:</strong> Revision restoration operations</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get all revision activities for validation
    /// var validActivities = await context.RevisionActivities
    ///     .Select(ra => ra.Activity)
    ///     .ToListAsync();
    /// </code>
    /// </example>
    public virtual DbSet<RevisionActivity> RevisionActivities { get; set; } = null!;

    /// <summary>
    /// Gets or sets the MatterDocumentActivities entity set for accessing matter document activity type records.
    /// </summary>
    /// <remarks>
    /// Provides access to matter document activity entities representing standardized operations for
    /// document transfers between matters. These activities enable comprehensive audit trails for
    /// document movement and copy operations.
    /// 
    /// <para><strong>Standard Activities:</strong></para>
    /// <list type="bullet">
    /// <item><strong>MOVED:</strong> Document moved from one matter to another</item>
    /// <item><strong>COPIED:</strong> Document copied from one matter to another</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get move activity for document transfer
    /// var moveActivity = await context.MatterDocumentActivities
    ///     .FirstOrDefaultAsync(mda => mda.Activity == "MOVED");
    /// </code>
    /// </example>
    public virtual DbSet<MatterDocumentActivity> MatterDocumentActivities { get; set; } = null!;

    #endregion Entity Sets - Activity Classification

    #region Entity Sets - Audit Trail Junction Tables

    /// <summary>
    /// Gets or sets the DocumentActivityUsers entity set for accessing document activity audit records.
    /// </summary>
    /// <remarks>
    /// Provides access to document activity user entities that form the junction table between documents,
    /// document activities, and users. This entity set is central to the document audit trail system,
    /// maintaining comprehensive records of all document operations.
    /// 
    /// <para><strong>Audit Trail Features:</strong></para>
    /// <list type="bullet">
    /// <item>Complete user attribution for all document operations</item>
    /// <item>Temporal tracking with precise UTC timestamps</item>
    /// <item>Activity classification for operation categorization</item>
    /// <item>Composite primary key ensuring audit record uniqueness</item>
    /// <item>Immutable audit trail preservation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Query recent document activities
    /// var recentActivities = await context.DocumentActivityUsers
    ///     .Where(dau => dau.CreatedAt >= DateTime.UtcNow.AddDays(-7))
    ///     .Include(dau => dau.Document)
    ///     .Include(dau => dau.DocumentActivity)
    ///     .Include(dau => dau.User)
    ///     .OrderByDescending(dau => dau.CreatedAt)
    ///     .ToListAsync();
    /// </code>
    /// </example>
    public virtual DbSet<DocumentActivityUser> DocumentActivityUsers { get; set; } = null!;

    /// <summary>
    /// Gets or sets the MatterActivityUsers entity set for accessing matter activity audit records.
    /// </summary>
    /// <remarks>
    /// Provides access to matter activity user entities that form the junction table between matters,
    /// matter activities, and users. This entity set maintains comprehensive audit trails for all
    /// matter lifecycle operations and user interactions.
    /// 
    /// <para><strong>Audit Trail Features:</strong></para>
    /// <list type="bullet">
    /// <item>Complete matter operation tracking</item>
    /// <item>User accountability for matter management</item>
    /// <item>Temporal audit trail with precise timestamps</item>
    /// <item>Matter lifecycle event classification</item>
    /// <item>Legal compliance audit support</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Query matter creation activities
    /// var matterCreations = await context.MatterActivityUsers
    ///     .Where(mau => mau.MatterActivity.Activity == "CREATED")
    ///     .Include(mau => mau.Matter)
    ///     .Include(mau => mau.User)
    ///     .OrderByDescending(mau => mau.CreatedAt)
    ///     .ToListAsync();
    /// </code>
    /// </example>
    public virtual DbSet<MatterActivityUser> MatterActivityUsers { get; set; } = null!;

    /// <summary>
    /// Gets or sets the RevisionActivityUsers entity set for accessing revision activity audit records.
    /// </summary>
    /// <remarks>
    /// Provides access to revision activity user entities that form the junction table between revisions,
    /// revision activities, and users. This entity set maintains detailed audit trails for document
    /// version control operations essential for legal document management.
    /// 
    /// <para><strong>Version Control Audit Features:</strong></para>
    /// <list type="bullet">
    /// <item>Complete revision operation tracking</item>
    /// <item>User attribution for version control activities</item>
    /// <item>Temporal tracking for revision chronology</item>
    /// <item>Version control event classification</item>
    /// <item>Document change history preservation</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Query revision activities for a specific document
    /// var revisionActivities = await context.RevisionActivityUsers
    ///     .Where(rau => rau.Revision.DocumentId == documentId)
    ///     .Include(rau => rau.Revision)
    ///     .Include(rau => rau.RevisionActivity)
    ///     .Include(rau => rau.User)
    ///     .OrderByDescending(rau => rau.CreatedAt)
    ///     .ToListAsync();
    /// </code>
    /// </example>
    public virtual DbSet<RevisionActivityUser> RevisionActivityUsers { get; set; } = null!;

    #endregion Entity Sets - Audit Trail Junction Tables

    #region Entity Sets - Directional Transfer Audit Tables

    /// <summary>
    /// Gets or sets the MatterDocumentActivityUsersFrom entity set for tracking source-side document transfers.
    /// </summary>
    /// <remarks>
    /// Provides access to matter document activity user from entities that track the source side of
    /// document transfer operations between matters. This entity set works in conjunction with the
    /// "To" entity set to provide complete bidirectional audit trails for document transfers.
    /// 
    /// <para><strong>Directional Audit Features:</strong></para>
    /// <list type="bullet">
    /// <item>Source matter tracking for document transfers</item>
    /// <item>User attribution for transfer initiation</item>
    /// <item>Transfer operation classification (MOVED/COPIED)</item>
    /// <item>Temporal tracking with precise timestamps</item>
    /// <item>Document provenance chain maintenance</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Query documents moved from a specific matter
    /// var documentsMovedFrom = await context.MatterDocumentActivityUsersFrom
    ///     .Where(mdau => mdau.MatterId == sourceMatterId && 
    ///                    mdau.MatterDocumentActivity.Activity == "MOVED")
    ///     .Include(mdau => mdau.Document)
    ///     .Include(mdau => mdau.User)
    ///     .ToListAsync();
    /// </code>
    /// </example>
    public virtual DbSet<MatterDocumentActivityUserFrom> MatterDocumentActivityUsersFrom { get; set; } = null!;

    /// <summary>
    /// Gets or sets the MatterDocumentActivityUsersTo entity set for tracking destination-side document transfers.
    /// </summary>
    /// <remarks>
    /// Provides access to matter document activity user to entities that track the destination side of
    /// document transfer operations between matters. This entity set complements the "From" entity set
    /// to provide complete bidirectional audit trails essential for legal document management.
    /// 
    /// <para><strong>Directional Audit Features:</strong></para>
    /// <list type="bullet">
    /// <item>Destination matter tracking for document transfers</item>
    /// <item>User attribution for transfer receipt</item>
    /// <item>Transfer operation classification (MOVED/COPIED)</item>
    /// <item>Temporal tracking with precise timestamps</item>
    /// <item>Document custody chain maintenance</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Query documents received by a specific matter
    /// var documentsReceivedBy = await context.MatterDocumentActivityUsersTo
    ///     .Where(mdau => mdau.MatterId == destinationMatterId && 
    ///                    mdau.MatterDocumentActivity.Activity == "COPIED")
    ///     .Include(mdau => mdau.Document)
    ///     .Include(mdau => mdau.User)
    ///     .ToListAsync();
    /// </code>
    /// </example>
    public virtual DbSet<MatterDocumentActivityUserTo> MatterDocumentActivityUsersTo { get; set; } = null!;

    #endregion Entity Sets - Directional Transfer Audit Tables

    #region Entity Framework Model Configuration

    /// <summary>
    /// Configures the Entity Framework model for the ADMS context, including entity relationships,
    /// constraints, indexes, and comprehensive seed data for development and testing.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the Entity Framework model.</param>
    /// <remarks>
    /// This method performs comprehensive model configuration including:
    /// <list type="bullet">
    /// <item><strong>Entity Relationships:</strong> Complex many-to-many relationships with composite keys</item>
    /// <item><strong>Cascade Behaviors:</strong> Appropriate cascade settings for audit trail preservation</item>
    /// <item><strong>Constraints:</strong> Business rule enforcement at the database level</item>
    /// <item><strong>Indexes:</strong> Performance optimization for common query patterns</item>
    /// <item><strong>Seed Data:</strong> Comprehensive reference data and test scenarios</item>
    /// </list>
    /// 
    /// <para><strong>Configuration Strategy:</strong></para>
    /// The configuration is organized into logical sections for maintainability:
    /// <list type="bullet">
    /// <item>Entity relationship configuration for each junction table</item>
    /// <item>Constraint and index configuration for performance</item>
    /// <item>Comprehensive seed data for all reference entities</item>
    /// <item>Sample audit trail data for testing and demonstration</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // This method is automatically called by Entity Framework during context initialization
    /// // Configuration can be overridden in derived contexts if needed
    /// public class CustomAdmsContext : AdmsContext
    /// {
    ///     protected override void OnModelCreating(ModelBuilder modelBuilder)
    ///     {
    ///         base.OnModelCreating(modelBuilder);
    ///         // Additional custom configuration...
    ///     }
    /// }
    /// </code>
    /// </example>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entity relationships and constraints
        ConfigureEntityRelationships(modelBuilder);

        // Configure performance optimizations
        ConfigurePerformanceOptimizations(modelBuilder);

        // Seed comprehensive reference and test data
        SeedCompleteDataSet(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    #endregion Entity Framework Model Configuration

    #region Entity Relationship Configuration

    /// <summary>
    /// Configures all entity relationships and constraints for the ADMS model.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model.</param>
    /// <remarks>
    /// This method delegates to specific configuration methods for each junction entity,
    /// ensuring proper separation of concerns and maintainable configuration code.
    /// </remarks>
    private static void ConfigureEntityRelationships(ModelBuilder modelBuilder)
    {
        ConfigureRevisionActivityUser(modelBuilder);
        ConfigureDocumentActivityUser(modelBuilder);
        ConfigureMatterActivityUser(modelBuilder);
        ConfigureMatterDocumentActivityUserFrom(modelBuilder);
        ConfigureMatterDocumentActivityUserTo(modelBuilder);
    }

    /// <summary>
    /// Configures the entity relationships and composite primary key for the <see cref="ADMS.API.Entities.RevisionActivityUser"/> entity.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model for the context.</param>
    /// <remarks>
    /// Establishes a composite primary key consisting of RevisionId, RevisionActivityId, UserId, and CreatedAt,
    /// ensuring that the combination of all four fields must be unique while allowing multiple activities
    /// of the same type with different timestamps.
    /// 
    /// <para><strong>Key Design Decisions:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Key:</strong> Enables multiple activities per revision while maintaining uniqueness</item>
    /// <item><strong>Required Relationships:</strong> All foreign keys are required for complete audit attribution</item>
    /// <item><strong>Cascade Behavior:</strong> Default cascade from Revision, Restrict from Activity and User</item>
    /// <item><strong>Temporal Uniqueness:</strong> CreatedAt in key allows same activity multiple times</item>
    /// </list>
    /// </remarks>
    private static void ConfigureRevisionActivityUser(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<RevisionActivityUser>();

        // Configure composite primary key
        entity.HasKey(rau => new
        {
            rau.RevisionId,
            rau.RevisionActivityId,
            rau.UserId,
            rau.CreatedAt
        });

        // Configure required relationships
        entity.HasOne(rau => rau.Revision)
            .WithMany(r => r.RevisionActivityUsers)
            .HasForeignKey(rau => rau.RevisionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // Cascade when revision is deleted

        entity.HasOne(rau => rau.RevisionActivity)
            .WithMany(ra => ra.RevisionActivityUsers)
            .HasForeignKey(rau => rau.RevisionActivityId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict); // Preserve audit trail integrity

        entity.HasOne(rau => rau.User)
            .WithMany(u => u.RevisionActivityUsers)
            .HasForeignKey(rau => rau.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict); // Preserve audit trail integrity

        // Configure table name for clarity
        entity.ToTable("RevisionActivityUsers");
    }

    /// <summary>
    /// Configures the entity relationships and composite primary key for the <see cref="ADMS.API.Entities.DocumentActivityUser"/> entity.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model for the context.</param>
    /// <remarks>
    /// Establishes a composite primary key consisting of DocumentId, DocumentActivityId, UserId, and CreatedAt,
    /// supporting the comprehensive document audit trail system with complete user attribution and temporal tracking.
    /// 
    /// <para><strong>Key Design Decisions:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Key:</strong> Enables multiple activities per document while maintaining uniqueness</item>
    /// <item><strong>Required Relationships:</strong> All foreign keys are required for complete audit attribution</item>
    /// <item><strong>Standard Cascade:</strong> Default cascade behavior for referential integrity</item>
    /// <item><strong>Audit Trail Focus:</strong> Designed for comprehensive document operation tracking</item>
    /// </list>
    /// </remarks>
    private static void ConfigureDocumentActivityUser(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<DocumentActivityUser>();

        // Configure composite primary key
        entity.HasKey(dau => new
        {
            dau.DocumentId,
            dau.DocumentActivityId,
            dau.UserId,
            dau.CreatedAt
        });

        // Configure required relationships
        entity.HasOne(dau => dau.Document)
            .WithMany(d => d.DocumentActivityUsers)
            .HasForeignKey(dau => dau.DocumentId)
            .IsRequired();

        entity.HasOne(dau => dau.DocumentActivity)
            .WithMany(da => da.DocumentActivityUsers)
            .HasForeignKey(dau => dau.DocumentActivityId)
            .IsRequired();

        entity.HasOne(dau => dau.User)
            .WithMany(u => u.DocumentActivityUsers)
            .HasForeignKey(dau => dau.UserId)
            .IsRequired();

        // Configure table name for clarity
        entity.ToTable("DocumentActivityUsers");
    }

    /// <summary>
    /// Configures the entity relationships and composite primary key for the <see cref="ADMS.API.Entities.MatterActivityUser"/> entity.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model for the context.</param>
    /// <remarks>
    /// Establishes a composite primary key consisting of MatterId, MatterActivityId, UserId, and CreatedAt,
    /// supporting comprehensive matter audit trail functionality with complete user attribution.
    /// 
    /// <para><strong>Key Design Decisions:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Composite Key:</strong> Ensures unique audit records while allowing temporal tracking</item>
    /// <item><strong>Required Relationships:</strong> All foreign keys are required for audit completeness</item>
    /// <item><strong>NoAction Cascade:</strong> Preserves audit trail integrity by preventing cascade deletes</item>
    /// <item><strong>Matter Focus:</strong> Specialized for legal matter lifecycle tracking</item>
    /// </list>
    /// </remarks>
    private static void ConfigureMatterActivityUser(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<MatterActivityUser>();

        // Configure composite primary key
        entity.HasKey(mau => new
        {
            mau.MatterId,
            mau.MatterActivityId,
            mau.UserId,
            mau.CreatedAt
        });

        // Configure required relationships with NoAction to preserve audit trails
        entity.HasOne(mau => mau.Matter)
            .WithMany(m => m.MatterActivityUsers)
            .HasForeignKey(mau => mau.MatterId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction); // Preserve audit trail

        entity.HasOne(mau => mau.MatterActivity)
            .WithMany(ma => ma.MatterActivityUsers)
            .HasForeignKey(mau => mau.MatterActivityId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction); // Preserve audit trail

        entity.HasOne(mau => mau.User)
            .WithMany(u => u.MatterActivityUsers)
            .HasForeignKey(mau => mau.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction); // Preserve audit trail

        // Configure table name for clarity
        entity.ToTable("MatterActivityUsers");
    }

    /// <summary>
    /// Configures the entity relationships and composite primary key for the <see cref="ADMS.API.Entities.MatterDocumentActivityUserFrom"/> entity.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model for the context.</param>
    /// <remarks>
    /// Establishes a five-component composite primary key and relationships for tracking the source side of
    /// document transfer operations between matters. This configuration supports comprehensive bidirectional
    /// audit trails essential for legal document management compliance.
    /// 
    /// <para><strong>Key Design Decisions:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Extended Composite Key:</strong> Five components for complete transfer operation uniqueness</item>
    /// <item><strong>NoAction Cascade:</strong> Preserves audit trail integrity across all relationships</item>
    /// <item><strong>Directional Tracking:</strong> Specialized for source-side transfer operation audit trails</item>
    /// <item><strong>Legal Compliance:</strong> Designed for document custody and provenance tracking</item>
    /// </list>
    /// </remarks>
    private static void ConfigureMatterDocumentActivityUserFrom(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<MatterDocumentActivityUserFrom>();

        // Configure composite primary key with all identifying components
        entity.HasKey(mdau => new
        {
            mdau.MatterId,
            mdau.DocumentId,
            mdau.MatterDocumentActivityId,
            mdau.UserId,
            mdau.CreatedAt
        });

        // Configure required relationships with NoAction to preserve audit trails
        entity.HasOne(mdau => mdau.Matter)
            .WithMany(m => m.MatterDocumentActivityUsersFrom)
            .HasForeignKey(mdau => mdau.MatterId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction); // Preserve audit trail integrity

        entity.HasOne(mdau => mdau.Document)
            .WithMany(d => d.MatterDocumentActivityUsersFrom)
            .HasForeignKey(mdau => mdau.DocumentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction); // Preserve audit trail integrity

        entity.HasOne(mdau => mdau.MatterDocumentActivity)
            .WithMany(mda => mda.MatterDocumentActivityUsersFrom)
            .HasForeignKey(mdau => mdau.MatterDocumentActivityId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction); // Preserve audit trail integrity

        entity.HasOne(mdau => mdau.User)
            .WithMany(u => u.MatterDocumentActivityUsersFrom)
            .HasForeignKey(mdau => mdau.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction); // Preserve audit trail integrity

        // Configure table name for clarity
        entity.ToTable("MatterDocumentActivityUsersFrom");
    }

    /// <summary>
    /// Configures the entity relationships and composite primary key for the <see cref="ADMS.API.Entities.MatterDocumentActivityUserTo"/> entity.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model for the context.</param>
    /// <remarks>
    /// Establishes a five-component composite primary key and relationships for tracking the destination side of
    /// document transfer operations between matters. This configuration complements the "From" configuration to
    /// provide complete bidirectional audit trails for legal compliance.
    /// 
    /// <para><strong>Key Design Decisions:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Mirror Configuration:</strong> Matches "From" entity configuration for consistency</item>
    /// <item><strong>NoAction Cascade:</strong> Preserves audit trail integrity across all relationships</item>
    /// <item><strong>Directional Tracking:</strong> Specialized for destination-side transfer audit trails</item>
    /// <item><strong>Bidirectional Completeness:</strong> Works with "From" entity for complete transfer tracking</item>
    /// </list>
    /// </remarks>
    private static void ConfigureMatterDocumentActivityUserTo(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<MatterDocumentActivityUserTo>();

        // Configure composite primary key with all identifying components
        entity.HasKey(mdau => new
        {
            mdau.MatterId,
            mdau.DocumentId,
            mdau.MatterDocumentActivityId,
            mdau.UserId,
            mdau.CreatedAt
        });

        // Configure required relationships with NoAction to preserve audit trails
        entity.HasOne(mdau => mdau.Matter)
            .WithMany(m => m.MatterDocumentActivityUsersTo)
            .HasForeignKey(mdau => mdau.MatterId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction); // Preserve audit trail integrity

        entity.HasOne(mdau => mdau.Document)
            .WithMany(d => d.MatterDocumentActivityUsersTo)
            .HasForeignKey(mdau => mdau.DocumentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction); // Preserve audit trail integrity

        entity.HasOne(mdau => mdau.MatterDocumentActivity)
            .WithMany(mda => mda.MatterDocumentActivityUsersTo)
            .HasForeignKey(mdau => mdau.MatterDocumentActivityId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction); // Preserve audit trail integrity

        entity.HasOne(mdau => mdau.User)
            .WithMany(u => u.MatterDocumentActivityUsersTo)
            .HasForeignKey(mdau => mdau.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction); // Preserve audit trail integrity

        // Configure table name for clarity
        entity.ToTable("MatterDocumentActivityUsersTo");
    }

    #endregion Entity Relationship Configuration

    #region Performance Optimizations

    /// <summary>
    /// Configures performance optimizations including indexes and query optimizations for the ADMS model.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model.</param>
    /// <remarks>
    /// This method configures database indexes and other performance optimizations based on common
    /// query patterns in legal document management workflows.
    /// 
    /// <para><strong>Index Strategy:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Temporal Indexes:</strong> CreatedAt fields for chronological queries</item>
    /// <item><strong>Foreign Key Indexes:</strong> Optimized relationship traversal</item>
    /// <item><strong>Status Indexes:</strong> IsDeleted, IsArchived for filtering</item>
    /// <item><strong>Composite Indexes:</strong> Multi-column indexes for complex queries</item>
    /// </list>
    /// </remarks>
    private static void ConfigurePerformanceOptimizations(ModelBuilder modelBuilder)
    {
        // Configure indexes for common query patterns

        // Document indexes
        modelBuilder.Entity<Document>()
            .HasIndex(d => d.MatterId)
            .HasDatabaseName("IX_Documents_MatterId");

        modelBuilder.Entity<Document>()
            .HasIndex(d => new { d.MatterId, d.IsDeleted })
            .HasDatabaseName("IX_Documents_MatterId_IsDeleted");

        // Matter indexes
        modelBuilder.Entity<Matter>()
            .HasIndex(m => new { m.IsArchived, m.IsDeleted })
            .HasDatabaseName("IX_Matters_IsArchived_IsDeleted");

        modelBuilder.Entity<Matter>()
            .HasIndex(m => m.CreationDate)
            .HasDatabaseName("IX_Matters_CreationDate");

        // Audit trail indexes for temporal queries
        modelBuilder.Entity<DocumentActivityUser>()
            .HasIndex(dau => dau.CreatedAt)
            .HasDatabaseName("IX_DocumentActivityUsers_CreatedAt");

        modelBuilder.Entity<MatterActivityUser>()
            .HasIndex(mau => mau.CreatedAt)
            .HasDatabaseName("IX_MatterActivityUsers_CreatedAt");

        modelBuilder.Entity<RevisionActivityUser>()
            .HasIndex(rau => rau.CreatedAt)
            .HasDatabaseName("IX_RevisionActivityUsers_CreatedAt");

        // Transfer audit indexes
        modelBuilder.Entity<MatterDocumentActivityUserFrom>()
            .HasIndex(mdau => mdau.CreatedAt)
            .HasDatabaseName("IX_MatterDocumentActivityUsersFrom_CreatedAt");

        modelBuilder.Entity<MatterDocumentActivityUserTo>()
            .HasIndex(mdau => mdau.CreatedAt)
            .HasDatabaseName("IX_MatterDocumentActivityUsersTo_CreatedAt");
    }

    #endregion Performance Optimizations

    #region Comprehensive Seed Data

    /// <summary>
    /// Seeds comprehensive reference data and test scenarios into the ADMS model.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model.</param>
    /// <remarks>
    /// This method orchestrates the seeding of all reference data and test scenarios required for
    /// the ADMS system to function properly. The seeding is organized into logical categories:
    /// 
    /// <para><strong>Seed Data Categories:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Activity Types:</strong> All standardized activities for documents, matters, and revisions</item>
    /// <item><strong>Users:</strong> Test users for development and demonstration</item>
    /// <item><strong>Matters:</strong> Sample legal matters in various states</item>
    /// <item><strong>Audit Trails:</strong> Sample audit trail entries demonstrating system capabilities</item>
    /// </list>
    /// 
    /// <para><strong>Seeding Strategy:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Predictable IDs:</strong> GUID patterns for reliable testing (e.g., 10000000... for revision activities)</item>
    /// <item><strong>Realistic Data:</strong> Professional naming and realistic scenarios</item>
    /// <item><strong>State Variations:</strong> Different combinations of archived, deleted states</item>
    /// <item><strong>Temporal Consistency:</strong> Chronologically consistent timestamps</item>
    /// </list>
    /// </remarks>
    private static void SeedCompleteDataSet(ModelBuilder modelBuilder)
    {
        // Seed activity types (reference data)
        SeedRevisionActivities(modelBuilder);
        SeedDocumentActivities(modelBuilder);
        SeedMatterActivities(modelBuilder);
        SeedMatterDocumentActivities(modelBuilder);

        // Seed users and organizational data
        SeedUsers(modelBuilder);
        SeedMatters(modelBuilder);

        // Seed audit trail examples
        SeedMatterActivityUsers(modelBuilder);
    }

    /// <summary>
    /// Seeds the comprehensive set of revision activity types into the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model.</param>
    /// <remarks>
    /// Seeds the four standard revision activities that support document version control operations
    /// in the ADMS system. These activities form the foundation for revision audit trails and
    /// version control compliance.
    /// 
    /// <para><strong>Seeded Activities (ID Pattern: 10000000-0000-0000-0000-00000000000X):</strong></para>
    /// <list type="bullet">
    /// <item><strong>CREATED (ID: ...0001):</strong> Initial creation of a document revision</item>
    /// <item><strong>DELETED (ID: ...0002):</strong> Soft deletion of a revision</item>
    /// <item><strong>RESTORED (ID: ...0003):</strong> Restoration of a deleted revision</item>
    /// <item><strong>SAVED (ID: ...0004):</strong> Saving changes to an existing revision</item>
    /// </list>
    /// </remarks>
    private static void SeedRevisionActivities(ModelBuilder modelBuilder)
    {
        var revisionActivities = new[]
        {
            new RevisionActivity
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Activity = "CREATED"
            },
            new RevisionActivity
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Activity = "DELETED"
            },
            new RevisionActivity
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Activity = "RESTORED"
            },
            new RevisionActivity
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Activity = "SAVED"
            }
        };

        modelBuilder.Entity<RevisionActivity>().HasData(revisionActivities);
    }

    /// <summary>
    /// Seeds the comprehensive set of document activity types into the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model.</param>
    /// <remarks>
    /// Seeds the six standard document activities that support document lifecycle and version control
    /// operations in the ADMS system. These activities enable comprehensive audit trails for all
    /// document operations including version control workflows.
    /// 
    /// <para><strong>Seeded Activities (ID Pattern: 20000000-0000-0000-0000-00000000000X):</strong></para>
    /// <list type="bullet">
    /// <item><strong>CHECKED IN (ID: ...0001):</strong> Document checked into version control</item>
    /// <item><strong>CHECKED OUT (ID: ...0002):</strong> Document checked out for editing</item>
    /// <item><strong>CREATED (ID: ...0003):</strong> Initial document creation</item>
    /// <item><strong>DELETED (ID: ...0004):</strong> Document marked for deletion</item>
    /// <item><strong>RESTORED (ID: ...0005):</strong> Deleted document restored</item>
    /// <item><strong>SAVED (ID: ...0006):</strong> Document saved with changes</item>
    /// </list>
    /// </remarks>
    private static void SeedDocumentActivities(ModelBuilder modelBuilder)
    {
        var documentActivities = new[]
        {
            new DocumentActivity
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                Activity = "CHECKED IN"
            },
            new DocumentActivity
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                Activity = "CHECKED OUT"
            },
            new DocumentActivity
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                Activity = "CREATED"
            },
            new DocumentActivity
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000004"),
                Activity = "DELETED"
            },
            new DocumentActivity
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000005"),
                Activity = "RESTORED"
            },
            new DocumentActivity
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000006"),
                Activity = "SAVED"
            }
        };

        modelBuilder.Entity<DocumentActivity>().HasData(documentActivities);
    }

    /// <summary>
    /// Seeds the comprehensive set of matter activity types into the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model.</param>
    /// <remarks>
    /// Seeds the six standard matter activities that support legal matter lifecycle management
    /// in the ADMS system. These activities enable comprehensive audit trails for matter operations
    /// and support legal practice workflow requirements.
    /// 
    /// <para><strong>Seeded Activities (ID Pattern: 30000000-0000-0000-0000-00000000000X):</strong></para>
    /// <list type="bullet">
    /// <item><strong>ARCHIVED (ID: ...0001):</strong> Matter archived for long-term storage</item>
    /// <item><strong>CREATED (ID: ...0002):</strong> New matter creation</item>
    /// <item><strong>DELETED (ID: ...0003):</strong> Matter marked for deletion</item>
    /// <item><strong>RESTORED (ID: ...0004):</strong> Deleted matter restored</item>
    /// <item><strong>UNARCHIVED (ID: ...0005):</strong> Archived matter returned to active status</item>
    /// <item><strong>VIEWED (ID: ...0006):</strong> Matter accessed or viewed</item>
    /// </list>
    /// </remarks>
    private static void SeedMatterActivities(ModelBuilder modelBuilder)
    {
        var matterActivities = new[]
        {
            new MatterActivity
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                Activity = "ARCHIVED"
            },
            new MatterActivity
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                Activity = "CREATED"
            },
            new MatterActivity
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                Activity = "DELETED"
            },
            new MatterActivity
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000004"),
                Activity = "RESTORED"
            },
            new MatterActivity
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000005"),
                Activity = "UNARCHIVED"
            },
            new MatterActivity
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000006"),
                Activity = "VIEWED"
            }
        };

        modelBuilder.Entity<MatterActivity>().HasData(matterActivities);
    }

    /// <summary>
    /// Seeds the comprehensive set of matter document activity types into the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model.</param>
    /// <remarks>
    /// Seeds the two standard matter document activities that support document transfer operations
    /// between legal matters. These activities enable bidirectional audit trails for document
    /// movement and copying operations essential for legal case management.
    /// 
    /// <para><strong>Seeded Activities (ID Pattern: 40000000-0000-0000-0000-00000000000X):</strong></para>
    /// <list type="bullet">
    /// <item><strong>COPIED (ID: ...0001):</strong> Document copied from one matter to another</item>
    /// <item><strong>MOVED (ID: ...0002):</strong> Document moved from one matter to another</item>
    /// </list>
    /// </remarks>
    private static void SeedMatterDocumentActivities(ModelBuilder modelBuilder)
    {
        var matterDocumentActivities = new[]
        {
            new MatterDocumentActivity
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                Activity = "COPIED"
            },
            new MatterDocumentActivity
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
                Activity = "MOVED"
            }
        };

        modelBuilder.Entity<MatterDocumentActivity>().HasData(matterDocumentActivities);
    }

    /// <summary>
    /// Seeds comprehensive user data into the model for development and testing.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model.</param>
    /// <remarks>
    /// Seeds a set of professional users representing different roles and scenarios in legal practice.
    /// The seeded users provide realistic test scenarios and enable comprehensive system testing.
    /// 
    /// <para><strong>Seeded Users (ID Pattern: 50000000-0000-0000-0000-00000000000X):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Robert Brown (ID: ...0001):</strong> Primary test user for system operations</item>
    /// <item><strong>Jennifer Smith (ID: ...0002):</strong> Secondary user for collaboration testing</item>
    /// <item><strong>Michael Johnson (ID: ...0003):</strong> Third user for complex workflow testing</item>
    /// <item><strong>Admin User (ID: ...0004):</strong> Administrative user for system management</item>
    /// </list>
    /// </remarks>
    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        var users = new[]
        {
            new User
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                Name = "Robert Brown"
            },
            new User
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
                Name = "Jennifer Smith"
            },
            new User
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000003"),
                Name = "Michael Johnson"
            },
            new User
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000004"),
                Name = "Admin User"
            }
        };

        modelBuilder.Entity<User>().HasData(users);
    }

    /// <summary>
    /// Seeds comprehensive matter data into the model for development and testing.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model.</param>
    /// <remarks>
    /// Seeds a diverse set of legal matters representing different states and scenarios common in
    /// legal practice. The matters demonstrate various combinations of archived and deleted states
    /// to support comprehensive testing of matter lifecycle management.
    /// 
    /// <para><strong>Seeded Matters (ID Pattern: 60000000-0000-0000-0000-00000000000X):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Corporate Merger - ABC Corp (ID: ...0001):</strong> Active matter</item>
    /// <item><strong>Employment Dispute - Smith v. TechCorp (ID: ...0002):</strong> Active matter</item>
    /// <item><strong>Real Estate Transaction - Johnson Property (ID: ...0003):</strong> Archived matter</item>
    /// <item><strong>Contract Review - Vendor Agreement (ID: ...0004):</strong> Deleted matter</item>
    /// <item><strong>Intellectual Property - Patent Filing (ID: ...0005):</strong> Archived and deleted matter</item>
    /// <item><strong>Family Law - Estate Planning (ID: ...0006):</strong> Archived and deleted matter</item>
    /// </list>
    /// </remarks>
    private static void SeedMatters(ModelBuilder modelBuilder)
    {
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var matters = new[]
        {
            new Matter
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000001"),
                Description = "Corporate Merger - ABC Corp",
                CreationDate = baseDate,
                IsArchived = false,
                IsDeleted = false
            },
            new Matter
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000002"),
                Description = "Employment Dispute - Smith v. TechCorp",
                CreationDate = baseDate.AddDays(1),
                IsArchived = false,
                IsDeleted = false
            },
            new Matter
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000003"),
                Description = "Real Estate Transaction - Johnson Property",
                CreationDate = baseDate.AddDays(2),
                IsArchived = true,
                IsDeleted = false
            },
            new Matter
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000004"),
                Description = "Contract Review - Vendor Agreement",
                CreationDate = baseDate.AddDays(3),
                IsArchived = false,
                IsDeleted = true
            },
            new Matter
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000005"),
                Description = "Intellectual Property - Patent Filing",
                CreationDate = baseDate.AddDays(4),
                IsArchived = true,
                IsDeleted = true
            },
            new Matter
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000006"),
                Description = "Family Law - Estate Planning",
                CreationDate = baseDate.AddDays(5),
                IsArchived = true,
                IsDeleted = true
            }
        };

        modelBuilder.Entity<Matter>().HasData(matters);
    }

    /// <summary>
    /// Seeds comprehensive matter activity user audit trail entries into the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model.</param>
    /// <remarks>
    /// Seeds a complete set of audit trail entries demonstrating the matter lifecycle management
    /// system. The entries show creation, archival, deletion, and other activities for all seeded matters,
    /// providing realistic examples of the audit trail system in operation using proper Entity Framework seeding practices.
    /// 
    /// <para><strong>Audit Trail Scenarios (ADMS.API.Entities References):</strong></para>
    /// <list type="bullet">
    /// <item><strong>Matter Creation:</strong> All six matters created by rbrown with sequential timestamps</item>
    /// <item><strong>Matter Archival:</strong> Three matters archived (matters 3, 5, 6) after creation</item>
    /// <item><strong>Matter Deletion:</strong> Three matters deleted (matters 4, 5, 6) with proper sequencing</item>
    /// <item><strong>Multi-User Activities:</strong> Secondary users performing viewing and administrative actions</item>
    /// <item><strong>Administrative Actions:</strong> Admin user performing restoration and unarchival operations</item>
    /// <item><strong>Recent Activity Simulation:</strong> 30-day-later activities simulating ongoing system usage</item>
    /// </list>
    /// 
    /// <para><strong>Seeded Activity Pattern:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Minutes 1-6:</strong> Sequential creation by rbrown using ADMS.API.Entities.MatterActivity.CREATED</item>
    /// <item><strong>Minutes 7-9:</strong> Archival activities using ADMS.API.Entities.MatterActivity.ARCHIVED</item>
    /// <item><strong>Minutes 10-12:</strong> Deletion activities using ADMS.API.Entities.MatterActivity.DELETED</item>
    /// <item><strong>Minutes 13-14:</strong> Viewing activities by jsmith using ADMS.API.Entities.MatterActivity.VIEWED</item>
    /// <item><strong>Minute 15:</strong> Administrative restoration by admin user using RESTORED activity</item>
    /// <item><strong>Minute 16:</strong> Administrative unarchival by admin user using UNARCHIVED activity</item>
    /// <item><strong>Minutes 17-20:</strong> Recent multi-user viewing activities simulating active system usage</item>
    /// </list>
    /// 
    /// <para><strong>Entity References (ADMS.API.Entities Namespace):</strong></para>
    /// <list type="bullet">
    /// <item><strong>ADMS.API.Entities.User:</strong> rbrown, jsmith, mjohnson, admin (seeded users)</item>
    /// <item><strong>ADMS.API.Entities.Matter:</strong> Six seeded matters in various states (active, archived, deleted)</item>
    /// <item><strong>ADMS.API.Entities.MatterActivity.CREATED:</strong> (ID: 30000000-0000-0000-0000-000000000002)</item>
    /// <item><strong>ADMS.API.Entities.MatterActivity.ARCHIVED:</strong> (ID: 30000000-0000-0000-0000-000000000001)</item>
    /// <item><strong>ADMS.API.Entities.MatterActivity.DELETED:</strong> (ID: 30000000-0000-0000-0000-000000000003)</item>
    /// <item><strong>ADMS.API.Entities.MatterActivity.RESTORED:</strong> (ID: 30000000-0000-0000-0000-000000000004)</item>
    /// <item><strong>ADMS.API.Entities.MatterActivity.UNARCHIVED:</strong> (ID: 30000000-0000-0000-0000-000000000005)</item>
    /// <item><strong>ADMS.API.Entities.MatterActivity.VIEWED:</strong> (ID: 30000000-0000-0000-0000-000000000006)</item>
    /// </list>
    /// 
    /// <para><strong>Entity Framework Seeding Best Practices:</strong></para>
    /// <list type="bullet">
    /// <item><strong>Foreign Keys Only:</strong> Seeds only primitive properties and foreign keys (no navigation properties)</item>
    /// <item><strong>Referential Integrity:</strong> All foreign key values reference valid seeded entities</item>
    /// <item><strong>Validation Compliance:</strong> All seeded data passes entity validation rules</item>
    /// <item><strong>Batch Operations:</strong> Uses efficient HasData for bulk seeding operations</item>
    /// <item><strong>Error Handling:</strong> Includes validation to ensure data integrity</item>
    /// </list>
    /// 
    /// This comprehensive audit trail seeding supports development, testing, and demonstration
    /// of the ADMS legal document management system's audit trail capabilities while properly
    /// referencing ADMS.API.Entities namespace types and following Entity Framework seeding best practices.
    /// Navigation properties will be automatically populated by Entity Framework at runtime based on foreign key relationships.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example query to verify seeded audit trail using ADMS.API.Entities types
    /// var auditTrail = await context.MatterActivityUsers
    ///     .Include(mau => mau.Matter)
    ///     .Include(mau => mau.MatterActivity)
    ///     .Include(mau => mau.User)
    ///     .OrderBy(mau => mau.CreatedAt)
    ///     .ToListAsync();
    /// 
    /// // Analysis of seeded patterns
    /// var creationActivities = auditTrail.Where(a => a.MatterActivity.Activity == "CREATED");
    /// var archivalActivities = auditTrail.Where(a => a.MatterActivity.Activity == "ARCHIVED");
    /// var deletionActivities = auditTrail.Where(a => a.MatterActivity.Activity == "DELETED");
    /// 
    /// // Multi-user activity analysis
    /// var userActivityCounts = auditTrail
    ///     .GroupBy(a => a.User.Name)
    ///     .Select(g => new { User = g.Key, Count = g.Count() })
    ///     .OrderByDescending(x => x.Count);
    /// </code>
    /// </example>
    private static void SeedMatterActivityUsers(ModelBuilder modelBuilder)
    {
        // Define base timestamp and entity IDs for consistent seeding
        var createdAtBase = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc);

        // User IDs from ADMS.API.Entities.User seeded data
        var rbownUserId = Guid.Parse("50000000-0000-0000-0000-000000000001");        // rbrown
        var jsmithUserId = Guid.Parse("50000000-0000-0000-0000-000000000002");       // jsmith
        var mjohnsonUserId = Guid.Parse("50000000-0000-0000-0000-000000000003");     // mjohnson
        var adminUserId = Guid.Parse("50000000-0000-0000-0000-000000000004");        // admin

        // Activity IDs from ADMS.API.Entities.MatterActivity seeded data
        var createdActivityId = Guid.Parse("30000000-0000-0000-0000-000000000002");  // CREATED
        var archivedActivityId = Guid.Parse("30000000-0000-0000-0000-000000000001"); // ARCHIVED
        var deletedActivityId = Guid.Parse("30000000-0000-0000-0000-000000000003");  // DELETED
        var restoredActivityId = Guid.Parse("30000000-0000-0000-0000-000000000004"); // RESTORED
        var unarchivedActivityId = Guid.Parse("30000000-0000-0000-0000-000000000005"); // UNARCHIVED
        var viewedActivityId = Guid.Parse("30000000-0000-0000-0000-000000000006");   // VIEWED

        // Matter IDs from ADMS.API.Entities.Matter seeded data for comprehensive audit trail seeding
        var matterIds = new[]
        {
            Guid.Parse("60000000-0000-0000-0000-000000000001"), // Corporate Merger - ABC Corp
            Guid.Parse("60000000-0000-0000-0000-000000000002"), // Employment Dispute - Smith v. TechCorp
            Guid.Parse("60000000-0000-0000-0000-000000000003"), // Real Estate Transaction - Johnson Property
            Guid.Parse("60000000-0000-0000-0000-000000000004"), // Contract Review - Vendor Agreement
            Guid.Parse("60000000-0000-0000-0000-000000000005"), // Intellectual Property - Patent Filing
            Guid.Parse("60000000-0000-0000-0000-000000000006")  // Family Law - Estate Planning
        };

        // Collection to hold all MatterActivityUser entries for batch seeding
        var matterActivityUsers = new List<MatterActivityUser>();

        try
        {
            // Phase 1: Matter Creation Activities (Minutes 1-6)
            // All matters are initially created by rbrown user with sequential timestamps
            matterActivityUsers.AddRange(matterIds.Select((t, i) => new MatterActivityUser
            {
                CreatedAt = createdAtBase.AddMinutes(i + 1),
                MatterId = t,
                MatterActivityId = createdActivityId,
                UserId = rbownUserId,
                Matter = null!, // Required for C# 11 required members, but ignored by EF seeding
                MatterActivity = null!, // Required for C# 11 required members, but ignored by EF seeding
                User = null! // Required for C# 11 required members, but ignored by EF seeding
                // Note: Navigation properties (Matter, MatterActivity, User) are NOT set during seeding
                // Entity Framework will populate these automatically based on foreign key relationships
            }));

            // Phase 2: Matter Archival Activities (Minutes 7-9)
            // Matters 3, 5, and 6 are archived after initial creation
            var archivalMatters = new[]
            {
                matterIds[2], // Matter 3: Real Estate Transaction - Johnson Property
                matterIds[4], // Matter 5: Intellectual Property - Patent Filing
                matterIds[5]  // Matter 6: Family Law - Estate Planning
            };

            matterActivityUsers.AddRange(archivalMatters.Select((t, i) => new MatterActivityUser
            {
                CreatedAt = createdAtBase.AddMinutes(7 + i),
                MatterId = t,
                MatterActivityId = archivedActivityId,
                UserId = rbownUserId,
                Matter = null!, // Required for C# 11 required members, but ignored by EF seeding
                MatterActivity = null!, // Required for C# 11 required members, but ignored by EF seeding
                User = null! // Required for C# 11 required members, but ignored by EF seeding
            }));

            // Phase 3: Matter Deletion Activities (Minutes 10-12)
            // Matters 4, 5, and 6 are deleted (matters 5 and 6 were previously archived)
            var deletionMatters = new[]
            {
                matterIds[3], // Matter 4: Contract Review - Vendor Agreement
                matterIds[4], // Matter 5: Intellectual Property - Patent Filing
                matterIds[5]  // Matter 6: Family Law - Estate Planning
            };

            matterActivityUsers.AddRange(deletionMatters.Select((t, i) => new MatterActivityUser
            {
                CreatedAt = createdAtBase.AddMinutes(10 + i),
                MatterId = t,
                MatterActivityId = deletedActivityId,
                UserId = rbownUserId,
                Matter = null!, // Required for C# 11 required members, but ignored by EF seeding
                MatterActivity = null!, // Required for C# 11 required members, but ignored by EF seeding
                User = null! // Required for C# 11 required members, but ignored by EF seeding
            }));

            // Phase 4: Secondary User Activity Patterns (Minutes 13-14)
            // Add VIEWED activities by jsmith on active matters to demonstrate collaboration
            var activeMatters = new[] { matterIds[0], matterIds[1] }; // First two matters remain active

            matterActivityUsers.AddRange(activeMatters.Select((t, i) => new MatterActivityUser
            {
                CreatedAt = createdAtBase.AddMinutes(13 + i),
                MatterId = t,
                MatterActivityId = viewedActivityId,
                UserId = jsmithUserId,
                Matter = null!, // Required for C# 11 required members, but ignored by EF seeding
                MatterActivity = null!, // Required for C# 11 required members, but ignored by EF seeding
                User = null! // Required for C# 11 required members, but ignored by EF seeding
            }));

            // Phase 5: Administrative Restoration Activity (Minute 15)
            // Demonstrate restoration of previously deleted matter by admin user
            var restorationActivity = new MatterActivityUser
            {
                CreatedAt = createdAtBase.AddMinutes(15),
                MatterId = matterIds[3], // Matter 4: Contract Review - Vendor Agreement
                MatterActivityId = restoredActivityId,
                UserId = adminUserId,
                Matter = null!,           // Required for C# 11 required members, but ignored by EF seeding
                MatterActivity = null!,   // Required for C# 11 required members, but ignored by EF seeding
                User = null!              // Required for C# 11 required members, but ignored by EF seeding
            };

            matterActivityUsers.Add(restorationActivity);

            // Phase 6: Administrative Unarchival Activity (Minute 16)
            // Demonstrate unarchival of previously archived matter by admin user
            var unarchivalActivity = new MatterActivityUser
            {
                CreatedAt = createdAtBase.AddMinutes(16),
                MatterId = matterIds[2], // Matter 3: Real Estate Transaction - Johnson Property
                MatterActivityId = unarchivedActivityId,
                UserId = adminUserId,
                Matter = null!,           // Required for C# 11 required members, but ignored by EF seeding
                MatterActivity = null!,   // Required for C# 11 required members, but ignored by EF seeding
                User = null!              // Required for C# 11 required members, but ignored by EF seeding
            };

            matterActivityUsers.Add(unarchivalActivity);

            // Phase 7: Recent Multi-User Activity Simulation (Minutes 17-20)
            // Add recent activities to simulate active system usage by all users
            var recentBaseTime = createdAtBase.AddDays(30); // 30 days later for recent activity simulation

            var allUserIds = new[]
            {
                rbownUserId,     // rbrown - primary user
                jsmithUserId,    // jsmith - secondary user
                mjohnsonUserId,  // mjohnson - third user
                adminUserId      // admin - administrative user
            };

            // Simulate recent matter access patterns with round-robin user assignment
            matterActivityUsers.AddRange(allUserIds.Select((t, userIndex) => new MatterActivityUser
            {
                CreatedAt = recentBaseTime.AddMinutes(userIndex),
                MatterId = matterIds[userIndex % 2], // Alternate between first two active matters
                MatterActivityId = viewedActivityId,
                UserId = t,
                Matter = null!,
                MatterActivity = null!,
                User = null!
            }));

            // Phase 8: Additional Complex Scenarios (Minutes 21-24)
            // Add more complex interaction patterns for comprehensive testing

            // mjohnson performs additional viewing activities
            var mjohnsonViewingActivity = new MatterActivityUser
            {
                CreatedAt = recentBaseTime.AddMinutes(5),
                MatterId = matterIds[0], // Corporate Merger matter
                MatterActivityId = viewedActivityId,
                UserId = mjohnsonUserId,
                Matter = null!,
                MatterActivity = null!,
                User = null!
            };

            matterActivityUsers.Add(mjohnsonViewingActivity);

            // Additional admin viewing for oversight demonstration
            var adminOversightActivity = new MatterActivityUser
            {
                CreatedAt = recentBaseTime.AddMinutes(6),
                MatterId = matterIds[1], // Employment Dispute matter
                MatterActivityId = viewedActivityId,
                UserId = adminUserId,
                Matter = null!,
                MatterActivity = null!,
                User = null!
            };

            matterActivityUsers.Add(adminOversightActivity);

            // Validation: Ensure all entries have valid references
            foreach (var activityUser in matterActivityUsers.Where(activityUser => activityUser.MatterId == Guid.Empty ||
                         activityUser.MatterActivityId == Guid.Empty ||
                         activityUser.UserId == Guid.Empty ||
                         activityUser.CreatedAt == default))
            {
                throw new InvalidOperationException(
                    $"Invalid MatterActivityUser entry detected during seeding. " +
                    $"MatterId: {activityUser.MatterId}, " +
                    $"MatterActivityId: {activityUser.MatterActivityId}, " +
                    $"UserId: {activityUser.UserId}, " +
                    $"CreatedAt: {activityUser.CreatedAt}");
            }

            // Commit all seeded audit trail entries to the model using Entity Framework seeding
            // Note: Navigation properties cannot be set in seeding - Entity Framework will
            // automatically populate them based on foreign key relationships at runtime
            modelBuilder.Entity<MatterActivityUser>().HasData(matterActivityUsers.ToArray());
        }
        catch (Exception ex)
        {
            // Enhanced error handling for seeding operations
            throw new InvalidOperationException(
                "Failed to seed MatterActivityUser data. This may indicate issues with " +
                "referenced entity IDs or seeding configuration. Verify that all referenced " +
                "User, Matter, and MatterActivity entities have been properly seeded before " +
                "this method is called.", ex);
        }
    }

    #endregion Comprehensive Seed Data
}