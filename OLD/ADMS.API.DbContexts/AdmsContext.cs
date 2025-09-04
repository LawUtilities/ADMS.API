using ADMS.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ADMS.API.DbContexts;

/// <summary>
///     ADMS Context for database access
/// </summary>
/// <remarks>
///     Adms constructor
/// </remarks>
/// <param name="options"></param>
public class AdmsContext(DbContextOptions<AdmsContext> options) : DbContext(options)
{
    /// <summary>
    ///     Documents database set for database access
    /// </summary>
    public DbSet<Document> Documents { get; set; } = null!;

    /// <summary>
    ///     DocumentActivities database set for database access
    /// </summary>
    public DbSet<DocumentActivity> DocumentActivities { get; set; } = null!;

    /// <summary>
    ///     Matters database set for database access
    /// </summary>
    public DbSet<Matter> Matters { get; set; } = null!;

    /// <summary>
    ///     MatterActivities database set for database access
    /// </summary>
    public DbSet<MatterActivity> MatterActivities { get; set; } = null!;

    /// <summary>
    ///     DocumentActivityUsers database set for database access
    /// </summary>
    public DbSet<DocumentActivityUser> DocumentActivityUsers { get; set; } = null!;

    /// <summary>
    ///     MatterDocumentActivities database set for database access
    /// </summary>
    public DbSet<MatterDocumentActivity> MatterDocumentActivities { get; set; } = null!;

    /// <summary>
    ///     MatterDocumentActivityUsersFrom database set for database access
    /// </summary>
    public DbSet<MatterDocumentActivityUserFrom> MatterDocumentActivityUsersFrom { get; set; } = null!;

    /// <summary>
    ///     MatterDocumentActivityUsersTo database set for database access
    /// </summary>
    public DbSet<MatterDocumentActivityUserTo> MatterDocumentActivityUsersTo { get; set; } = null!;

    /// <summary>
    ///     RevisionActivities database set for database access
    /// </summary>
    public DbSet<RevisionActivity> RevisionActivities { get; set; } = null!;

    /// <summary>
    ///     Revisions database set for database access
    /// </summary>
    public DbSet<Revision> Revisions { get; set; } = null!;

    /// <summary>
    ///     Users database set for database access
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    ///     MatterActivityUsers database set for database access
    /// </summary>
    public DbSet<MatterActivityUser> MatterActivityUsers { get; set; } = null!;

    /// <summary>
    ///     RevisionActivityUsers database set for database access
    /// </summary>
    public DbSet<RevisionActivityUser> RevisionActivityUsers { get; set; } = null!;

    /// <summary>
    /// Configures the entity framework model for the context, including relationships and seed data.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entity configuration
        ConfigureRevisionActivityUser(modelBuilder);
        ConfigureDocumentActivityUser(modelBuilder);
        ConfigureMatterActivityUser(modelBuilder);
        ConfigureMatterDocumentActivityUserFrom(modelBuilder);
        ConfigureMatterDocumentActivityUserTo(modelBuilder);

        // Seed data
        SeedInitialData(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Configures the entity relationships and keys for the <see cref="RevisionActivityUser"/> entity.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    private static void ConfigureRevisionActivityUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RevisionActivityUser>()
            .HasKey(rau => new
            {
                rau.RevisionId,
                rau.RevisionActivityId,
                rau.UserId,
                rau.CreatedAt
            });

        modelBuilder.Entity<RevisionActivityUser>()
            .HasOne(rau => rau.Revision)
            .WithMany(au => au.RevisionActivityUsers)
            .HasForeignKey(rau => rau.RevisionId)
            .IsRequired();

        modelBuilder.Entity<RevisionActivityUser>()
            .HasOne(rau => rau.RevisionActivity)
            .WithMany(au => au.RevisionActivityUsers)
            .HasForeignKey(rau => rau.RevisionActivityId)
            .IsRequired();

        modelBuilder.Entity<RevisionActivityUser>()
            .HasOne(rau => rau.User)
            .WithMany(au => au.RevisionActivityUsers)
            .HasForeignKey(rau => rau.UserId)
            .IsRequired();
    }

    /// <summary>
    /// Configures the entity relationships and keys for the <see cref="DocumentActivityUser"/> entity.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    private static void ConfigureDocumentActivityUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentActivityUser>()
            .HasKey(dau => new
            {
                dau.DocumentId,
                dau.DocumentActivityId,
                dau.UserId,
                dau.CreatedAt
            });

        modelBuilder.Entity<DocumentActivityUser>()
            .HasOne(dau => dau.Document)
            .WithMany(au => au.DocumentActivityUsers)
            .HasForeignKey(dau => dau.DocumentId)
            .IsRequired();

        modelBuilder.Entity<DocumentActivityUser>()
            .HasOne(dau => dau.DocumentActivity)
            .WithMany(au => au.DocumentActivityUsers)
            .HasForeignKey(dau => dau.DocumentActivityId)
            .IsRequired();

        modelBuilder.Entity<DocumentActivityUser>()
            .HasOne(dau => dau.User)
            .WithMany(au => au.DocumentActivityUsers)
            .HasForeignKey(dau => dau.UserId)
            .IsRequired();
    }

    /// <summary>
    /// Configures the entity relationships and keys for the <see cref="MatterActivityUser"/> entity.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    private static void ConfigureMatterActivityUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MatterActivityUser>()
            .HasKey(mau => new
            {
                mau.MatterId,
                mau.MatterActivityId,
                mau.UserId,
                mau.CreatedAt
            });

        modelBuilder.Entity<MatterActivityUser>()
            .HasOne(mau => mau.Matter)
            .WithMany(au => au.MatterActivityUsers)
            .HasForeignKey(mau => mau.MatterId)
            .IsRequired();

        modelBuilder.Entity<MatterActivityUser>()
            .HasOne(mau => mau.MatterActivity)
            .WithMany(au => au.MatterActivityUsers)
            .HasForeignKey(mau => mau.MatterActivityId)
            .IsRequired();

        modelBuilder.Entity<MatterActivityUser>()
            .HasOne(mau => mau.User)
            .WithMany(au => au.MatterActivityUsers)
            .HasForeignKey(mau => mau.UserId)
            .IsRequired();
    }

    /// <summary>
    /// Configures the entity relationships and keys for the <see cref="MatterDocumentActivityUserFrom"/> entity.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    private static void ConfigureMatterDocumentActivityUserFrom(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MatterDocumentActivityUserFrom>()
            .HasKey(mdau => new
            {
                mdau.MatterId,
                mdau.DocumentId,
                mdau.MatterDocumentActivityId,
                mdau.UserId,
                mdau.CreatedAt
            });

        modelBuilder.Entity<MatterDocumentActivityUserFrom>()
            .HasOne(mdau => mdau.Matter)
            .WithMany(m => m.MatterDocumentActivityUsersFrom)
            .HasForeignKey(mdau => mdau.MatterId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MatterDocumentActivityUserFrom>()
            .HasOne(mdau => mdau.Document)
            .WithMany(d => d.MatterDocumentActivityUsersFrom)
            .HasForeignKey(mdau => mdau.DocumentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MatterDocumentActivityUserFrom>()
            .HasOne(mdau => mdau.MatterDocumentActivity)
            .WithMany(mda => mda.MatterDocumentActivityUsersFrom)
            .HasForeignKey(mdau => mdau.MatterDocumentActivityId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MatterDocumentActivityUserFrom>()
            .HasOne(mdau => mdau.User)
            .WithMany(u => u.MatterDocumentActivityUsersFrom)
            .HasForeignKey(mdau => mdau.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }

    /// <summary>
    /// Configures the entity relationships and keys for the <see cref="MatterDocumentActivityUserTo"/> entity.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    private static void ConfigureMatterDocumentActivityUserTo(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MatterDocumentActivityUserTo>()
            .HasKey(mdau => new
            {
                mdau.MatterId,
                mdau.DocumentId,
                mdau.MatterDocumentActivityId,
                mdau.UserId,
                mdau.CreatedAt
            });

        modelBuilder.Entity<MatterDocumentActivityUserTo>()
            .HasOne(mdau => mdau.Matter)
            .WithMany(m => m.MatterDocumentActivityUsersTo)
            .HasForeignKey(mdau => mdau.MatterId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MatterDocumentActivityUserTo>()
            .HasOne(mdau => mdau.Document)
            .WithMany(d => d.MatterDocumentActivityUsersTo)
            .HasForeignKey(mdau => mdau.DocumentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MatterDocumentActivityUserTo>()
            .HasOne(mdau => mdau.MatterDocumentActivity)
            .WithMany(mda => mda.MatterDocumentActivityUsersTo)
            .HasForeignKey(mdau => mdau.MatterDocumentActivityId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MatterDocumentActivityUserTo>()
            .HasOne(mdau => mdau.User)
            .WithMany(u => u.MatterDocumentActivityUsersTo)
            .HasForeignKey(mdau => mdau.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }

    /// <summary>
    /// Seeds the initial data for activities, users, matters, and audit entries into the model.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    private static void SeedInitialData(ModelBuilder modelBuilder)
    {
        SeedRevisionActivities(modelBuilder);
        SeedDocumentActivities(modelBuilder);
        SeedMatterActivities(modelBuilder);
        SeedMatterDocumentActivities(modelBuilder);
        SeedUsers(modelBuilder);
        SeedMatters(modelBuilder);
        SeedMatterActivityUsers(modelBuilder);
    }

    /// <summary>
    /// Seeds the initial revision activity data into the model.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    private static void SeedRevisionActivities(ModelBuilder modelBuilder)
    {
        var revisionActivity1 = new RevisionActivity
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            Activity = "CREATED"
        };
        var revisionActivity2 = new RevisionActivity
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
            Activity = "DELETED"
        };
        var revisionActivity3 = new RevisionActivity
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
            Activity = "RESTORED"
        };
        var revisionActivity4 = new RevisionActivity
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
            Activity = "SAVED"
        };

        modelBuilder.Entity<RevisionActivity>().HasData(
            revisionActivity1, revisionActivity2, revisionActivity3, revisionActivity4
        );
    }

    /// <summary>
    /// Seeds the initial document activity data into the model.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    private static void SeedDocumentActivities(ModelBuilder modelBuilder)
    {
        var documentActivity1 = new DocumentActivity
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
            Activity = "CHECKED IN"
        };
        var documentActivity2 = new DocumentActivity
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            Activity = "CHECKED OUT"
        };
        var documentActivity3 = new DocumentActivity
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
            Activity = "CREATED"
        };
        var documentActivity4 = new DocumentActivity
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000004"),
            Activity = "DELETED"
        };
        var documentActivity5 = new DocumentActivity
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000005"),
            Activity = "RESTORED"
        };
        var documentActivity6 = new DocumentActivity
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000006"),
            Activity = "SAVED"
        };

        modelBuilder.Entity<DocumentActivity>().HasData(
            documentActivity1, documentActivity2, documentActivity3, documentActivity4, documentActivity5, documentActivity6
        );
    }

    /// <summary>
    /// Seeds the initial matter activity data into the model.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    private static void SeedMatterActivities(ModelBuilder modelBuilder)
    {
        var matterActivity1 = new MatterActivity
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            Activity = "ARCHIVED"
        };
        var matterActivity2 = new MatterActivity
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
            Activity = "CREATED"
        };
        var matterActivity3 = new MatterActivity
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
            Activity = "DELETED"
        };
        var matterActivity4 = new MatterActivity
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000004"),
            Activity = "RESTORED"
        };
        var matterActivity5 = new MatterActivity
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000005"),
            Activity = "UNARCHIVED"
        };
        var matterActivity6 = new MatterActivity
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000006"),
            Activity = "VIEWED"
        };

        modelBuilder.Entity<MatterActivity>().HasData(
            matterActivity1, matterActivity2, matterActivity3, matterActivity4, matterActivity5, matterActivity6
        );
    }

    /// <summary>
    /// Seeds the initial matter document activity data into the model.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    private static void SeedMatterDocumentActivities(ModelBuilder modelBuilder)
    {
        var matterDocumentActivity1 = new MatterDocumentActivity
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
            Activity = "COPIED"
        };
        var matterDocumentActivity2 = new MatterDocumentActivity
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
            Activity = "MOVED"
        };

        modelBuilder.Entity<MatterDocumentActivity>().HasData(
            matterDocumentActivity1, matterDocumentActivity2
        );
    }

    /// <summary>
    /// Seeds the initial user data into the model.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        var user1 = new User
        {
            Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
            Name = "rbrown"
        };

        modelBuilder.Entity<User>().HasData(user1);
    }

    /// <summary>
    /// Seeds the initial matter data into the model.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    private static void SeedMatters(ModelBuilder modelBuilder)
    {
        var matter1 = new Matter
        {
            Id = Guid.Parse("60000000-0000-0000-0000-000000000001"),
            Description = "Test Matter #1",
            CreationDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsArchived = false,
            IsDeleted = false
        };
        var matter2 = new Matter
        {
            Id = Guid.Parse("60000000-0000-0000-0000-000000000002"),
            Description = "Test Matter #2",
            CreationDate = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            IsArchived = false,
            IsDeleted = false
        };
        var matter3 = new Matter
        {
            Id = Guid.Parse("60000000-0000-0000-0000-000000000003"),
            Description = "Test Matter #3",
            CreationDate = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc),
            IsArchived = true,
            IsDeleted = false
        };
        var matter4 = new Matter
        {
            Id = Guid.Parse("60000000-0000-0000-0000-000000000004"),
            Description = "Test Matter #4",
            CreationDate = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc),
            IsArchived = false,
            IsDeleted = true
        };
        var matter5 = new Matter
        {
            Id = Guid.Parse("60000000-0000-0000-0000-000000000005"),
            Description = "Test Matter #5",
            CreationDate = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc),
            IsArchived = true,
            IsDeleted = true
        };
        var matter6 = new Matter
        {
            Id = Guid.Parse("60000000-0000-0000-0000-000000000006"),
            Description = "Test Matter #6",
            CreationDate = new DateTime(2024, 1, 6, 0, 0, 0, DateTimeKind.Utc),
            IsArchived = true,
            IsDeleted = true
        };

        modelBuilder.Entity<Matter>().HasData(
            matter1, matter2, matter3, matter4, matter5, matter6
        );
    }

    /// <summary>
    /// Seeds the initial matter activity user audit entries into the model.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    private static void SeedMatterActivityUsers(ModelBuilder modelBuilder)
    {
        var createdAtBase = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc);

        var matterActivityUser1 = new MatterActivityUser
        {
            CreatedAt = createdAtBase.AddMinutes(1),
            MatterId = Guid.Parse("60000000-0000-0000-0000-000000000001"),
            MatterActivityId = Guid.Parse("30000000-0000-0000-0000-000000000002"), // CREATED
            UserId = Guid.Parse("50000000-0000-0000-0000-000000000001")
        };
        var matterActivityUser2 = new MatterActivityUser
        {
            CreatedAt = createdAtBase.AddMinutes(2),
            MatterId = Guid.Parse("60000000-0000-0000-0000-000000000002"),
            MatterActivityId = Guid.Parse("30000000-0000-0000-0000-000000000002"),
            UserId = Guid.Parse("50000000-0000-0000-0000-000000000001")
        };
        var matterActivityUser3 = new MatterActivityUser
        {
            CreatedAt = createdAtBase.AddMinutes(3),
            MatterId = Guid.Parse("60000000-0000-0000-0000-000000000003"),
            MatterActivityId = Guid.Parse("30000000-0000-0000-0000-000000000002"),
            UserId = Guid.Parse("50000000-0000-0000-0000-000000000001")
        };
        var matterActivityUser4 = new MatterActivityUser
        {
            CreatedAt = createdAtBase.AddMinutes(4),
            MatterId = Guid.Parse("60000000-0000-0000-0000-000000000004"),
            MatterActivityId = Guid.Parse("30000000-0000-0000-0000-000000000002"),
            UserId = Guid.Parse("50000000-0000-0000-0000-000000000001")
        };
        var matterActivityUser5 = new MatterActivityUser
        {
            CreatedAt = createdAtBase.AddMinutes(5),
            MatterId = Guid.Parse("60000000-0000-0000-0000-000000000005"),
            MatterActivityId = Guid.Parse("30000000-0000-0000-0000-000000000002"),
            UserId = Guid.Parse("50000000-0000-0000-0000-000000000001")
        };
        var matterActivityUser6 = new MatterActivityUser
        {
            CreatedAt = createdAtBase.AddMinutes(6),
            MatterId = Guid.Parse("60000000-0000-0000-0000-000000000006"),
            MatterActivityId = Guid.Parse("30000000-0000-0000-0000-000000000002"),
            UserId = Guid.Parse("50000000-0000-0000-0000-000000000001")
        };
        // Archival
        var matterActivityUser7 = new MatterActivityUser
        {
            CreatedAt = createdAtBase.AddMinutes(7),
            MatterId = Guid.Parse("60000000-0000-0000-0000-000000000003"),
            MatterActivityId = Guid.Parse("30000000-0000-0000-0000-000000000001"), // ARCHIVED
            UserId = Guid.Parse("50000000-0000-0000-0000-000000000001")
        };
        var matterActivityUser8 = new MatterActivityUser
        {
            CreatedAt = createdAtBase.AddMinutes(8),
            MatterId = Guid.Parse("60000000-0000-0000-0000-000000000005"),
            MatterActivityId = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            UserId = Guid.Parse("50000000-0000-0000-0000-000000000001")
        };
        var matterActivityUser9 = new MatterActivityUser
        {
            CreatedAt = createdAtBase.AddMinutes(9),
            MatterId = Guid.Parse("60000000-0000-0000-0000-000000000006"),
            MatterActivityId = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            UserId = Guid.Parse("50000000-0000-0000-0000-000000000001")
        };
        // Deletion
        var matterActivityUser10 = new MatterActivityUser
        {
            CreatedAt = createdAtBase.AddMinutes(10),
            MatterId = Guid.Parse("60000000-0000-0000-0000-000000000004"),
            MatterActivityId = Guid.Parse("30000000-0000-0000-0000-000000000003"), // DELETED
            UserId = Guid.Parse("50000000-0000-0000-0000-000000000001")
        };
        var matterActivityUser11 = new MatterActivityUser
        {
            CreatedAt = createdAtBase.AddMinutes(11),
            MatterId = Guid.Parse("60000000-0000-0000-0000-000000000005"),
            MatterActivityId = Guid.Parse("30000000-0000-0000-0000-000000000003"),
            UserId = Guid.Parse("50000000-0000-0000-0000-000000000001")
        };
        var matterActivityUser12 = new MatterActivityUser
        {
            CreatedAt = createdAtBase.AddMinutes(12),
            MatterId = Guid.Parse("60000000-0000-0000-0000-000000000006"),
            MatterActivityId = Guid.Parse("30000000-0000-0000-0000-000000000003"),
            UserId = Guid.Parse("50000000-0000-0000-0000-000000000001")
        };

        modelBuilder.Entity<MatterActivityUser>().HasData(
            matterActivityUser1, matterActivityUser2, matterActivityUser3, matterActivityUser4, matterActivityUser5, matterActivityUser6,
            matterActivityUser7, matterActivityUser8, matterActivityUser9, matterActivityUser10, matterActivityUser11, matterActivityUser12
        );
    }
}