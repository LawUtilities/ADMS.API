using ADMS.API.Entities;

using Microsoft.EntityFrameworkCore;

namespace ADMS.API.DbContexts
{
    /// <summary>
    /// ADMS Context for database access
    /// </summary>
    /// <remarks>
    /// Adms constructor
    /// </remarks>
    /// <param name="options"></param>
    public class AdmsContext(DbContextOptions<AdmsContext> options) : DbContext(options)
    {
        /// <summary>
        /// Documents database set for database access
        /// </summary>
        public DbSet<Document> Documents { get; set; } = null!;

        /// <summary>
        /// DocumentActivities database set for database access
        /// </summary>
        public DbSet<DocumentActivity> DocumentActivities { get; set; } = null!;

        /// <summary>
        /// Matters database set for database access
        /// </summary>
        public DbSet<Matter> Matters { get; set; } = null!;

        /// <summary>
        /// MatterActivities database set for database access
        /// </summary>
        public DbSet<MatterActivity> MatterActivities { get; set; } = null!;

        /// <summary>
        /// DocumentActivityUsers database set for database access
        /// </summary>
        public DbSet<DocumentActivityUser> DocumentActivityUsers { get; set; } = null!;

        /// <summary>
        /// MatterDocumentActivities database set for database access
        /// </summary>
        public DbSet<MatterDocumentActivity> MatterDocumentActivities { get; set; } = null!;

        /// <summary>
        /// MatterDocumentActivityUsersFrom database set for database access
        /// </summary>
        public DbSet<MatterDocumentActivityUserFrom> MatterDocumentActivityUsersFrom { get; set; } = null!;

        /// <summary>
        /// MatterDocumentActivityUsersTo database set for database access
        /// </summary>
        public DbSet<MatterDocumentActivityUserTo> MatterDocumentActivityUsersTo { get; set; } = null!;

        /// <summary>
        /// RevisionActivities database set for database access
        /// </summary>
        public DbSet<RevisionActivity> RevisionActivities { get; set; } = null!;

        /// <summary>
        /// Revisions database set for database access
        /// </summary>
        public DbSet<Revision> Revisions { get; set; } = null!;

        /// <summary>
        /// Users database set for database access
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// MatterActivityUsers database set for database access
        /// </summary>
        public DbSet<MatterActivityUser> MatterActivityUsers { get; set; } = null!;

        /// <summary>
        /// RevisionActivityUsers database set for database access
        /// </summary>
        public DbSet<RevisionActivityUser> RevisionActivityUsers { get; set; } = null!;

        /// <summary>
        /// On Model Creating method
        /// </summary>
        /// <param name="modelBuilder">model builder in use</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region setup RevisionActivity

            var revisionActivity1 = new RevisionActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "CREATED"
            };
            var revisionActivity2 = new RevisionActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "DELETED"
            };
            var revisionActivity3 = new RevisionActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "RESTORED"
            };
            var revisionActivity4 = new RevisionActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "SAVED"
            };

            #endregion setup RevisionActivity

            #region Setup DocumentActivities

            var documentActivity1 = new DocumentActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "CHECKED IN"
            };
            var documentActivity2 = new DocumentActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "CHECKED OUT"
            };
            var documentActivity3 = new DocumentActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "CREATED"
            };
            var documentActivity4 = new DocumentActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "DELETED"
            };
            var documentActivity5 = new DocumentActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "RESTORED"
            };
            var documentActivity6 = new DocumentActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "SAVED"
            };

            #endregion Setup DocumentActivities

            #region setup MatterActivity

            var matterActivity1 = new MatterActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "ARCHIVED"
            };
            var matterActivity2 = new MatterActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "CREATED"
            };
            var matterActivity3 = new MatterActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "DELETED"
            };
            var matterActivity4 = new MatterActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "RESTORED"
            };
            var matterActivity5 = new MatterActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "UNARCHIVED"
            };

            #endregion setup MatterActivity

            #region setup MatterDocumentActivity

            var matterDocumentActivity1 = new MatterDocumentActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "COPIED"
            };
            var matterDocumentActivity2 = new MatterDocumentActivity()
            {
                Id = Guid.NewGuid(),
                Activity = "MOVED"
            };

            #endregion setup MatterDocumentActivity

            #region setup User

            var user1 = new User()
            {
                Id = Guid.NewGuid(),
                Name = "rbrown"
            };

            #endregion setup User

            #region setup Matter

            var matter1 = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter #1",
                CreationDate = DateTime.Now.ToUniversalTime(),
                IsArchived = false,
                IsDeleted = false,
            };

            var matter2 = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter #2",
                CreationDate = DateTime.Now.ToUniversalTime(),
                IsArchived = false,
                IsDeleted = false
            };

            var matter3 = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter #3",
                CreationDate = DateTime.Now.ToUniversalTime(),
                IsArchived = true,
                IsDeleted = false
            };

            var matter4 = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter #4",
                CreationDate = DateTime.Now.ToUniversalTime(),
                IsArchived = false,
                IsDeleted = true
            };

            var matter5 = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter #5",
                CreationDate = DateTime.Now.ToUniversalTime(),
                IsArchived = true,
                IsDeleted = true
            };

            var matter6 = new Matter()
            {
                Id = Guid.NewGuid(),
                Description = "Test Matter #6",
                CreationDate = DateTime.Now.ToUniversalTime(),
                IsArchived = true,
                IsDeleted = true
            };

            #endregion setup Matter

            #region setup Creation MatterActivityUser

            var matterActivityUser1 = new MatterActivityUser()
            {
                CreatedAt = DateTime.Now.ToUniversalTime(),
                MatterId = matter1.Id,
                MatterActivityId = matterActivity2.Id,
                UserId = user1.Id
            };

            var matterActivityUser2 = new MatterActivityUser()
            {
                CreatedAt = DateTime.Now.ToUniversalTime(),
                MatterId = matter2.Id,
                MatterActivityId = matterActivity2.Id,
                UserId = user1.Id
            };

            var matterActivityUser3 = new MatterActivityUser()
            {
                CreatedAt = DateTime.Now.ToUniversalTime(),
                MatterId = matter3.Id,
                MatterActivityId = matterActivity2.Id,
                UserId = user1.Id
            };

            var matterActivityUser4 = new MatterActivityUser()
            {
                CreatedAt = DateTime.Now.ToUniversalTime(),
                MatterId = matter4.Id,
                MatterActivityId = matterActivity2.Id,
                UserId = user1.Id
            };

            var matterActivityUser5 = new MatterActivityUser()
            {
                CreatedAt = DateTime.Now.ToUniversalTime(),
                MatterId = matter5.Id,
                MatterActivityId = matterActivity2.Id,
                UserId = user1.Id
            };

            var matterActivityUser6 = new MatterActivityUser()
            {
                CreatedAt = DateTime.Now.ToUniversalTime(),
                MatterId = matter6.Id,
                MatterActivityId = matterActivity2.Id,
                UserId = user1.Id
            };

            #endregion setup Creation MatterActivityUser

            #region setup Archival MatterActivityUser

            var matterActivityUser7 = new MatterActivityUser()
            {
                CreatedAt = DateTime.Now.ToUniversalTime(),
                MatterId = matter3.Id,
                MatterActivityId = matterActivity1.Id,
                UserId = user1.Id
            };

            var matterActivityUser8 = new MatterActivityUser()
            {
                CreatedAt = DateTime.Now.ToUniversalTime(),
                MatterId = matter5.Id,
                MatterActivityId = matterActivity1.Id,
                UserId = user1.Id
            };

            var matterActivityUser9 = new MatterActivityUser()
            {
                CreatedAt = DateTime.Now.ToUniversalTime(),
                MatterId = matter6.Id,
                MatterActivityId = matterActivity1.Id,
                UserId = user1.Id
            };

            #endregion setup Archival MatterActivityUser

            #region setup Deletion MatterActivityUser

            var matterActivityUser10 = new MatterActivityUser()
            {
                CreatedAt = DateTime.Now.ToUniversalTime(),
                MatterId = matter4.Id,
                MatterActivityId = matterActivity3.Id,
                UserId = user1.Id
            };

            var matterActivityUser11 = new MatterActivityUser()
            {
                CreatedAt = DateTime.Now.ToUniversalTime(),
                MatterId = matter5.Id,
                MatterActivityId = matterActivity3.Id,
                UserId = user1.Id
            };

            var matterActivityUser12 = new MatterActivityUser()
            {
                CreatedAt = DateTime.Now.ToUniversalTime(),
                MatterId = matter6.Id,
                MatterActivityId = matterActivity3.Id,
                UserId = user1.Id
            };

            #endregion setup Deletion MatterActivityUser

            #region setup Revision Activity User linkage

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

            #endregion setup Revision Activity User linkage

            #region setup Document Activity User linkage

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

            #endregion setup Document Activity User linkage

            #region setup Matter Activity User linkage

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

            #endregion setup Matter Activity User linkage

            #region setup MatterDocumentActivityUserFrom linkage

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
                .WithMany(dau => dau.MatterDocumentActivityUsersFrom)
                .HasForeignKey(mdau => mdau.MatterId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MatterDocumentActivityUserFrom>()
                .HasOne(mdau => mdau.Document)
                .WithMany(dau => dau.MatterDocumentActivityUsersFrom)
                .HasForeignKey(mdau => mdau.DocumentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MatterDocumentActivityUserFrom>()
                .HasOne(mdau => mdau.MatterDocumentActivity)
                .WithMany(dau => dau.MatterDocumentActivityUsersFrom)
                .HasForeignKey(mdau => mdau.MatterDocumentActivityId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MatterDocumentActivityUserFrom>()
                .HasOne(mdau => mdau.User)
                .WithMany(dau => dau.MatterDocumentActivityUsersFrom)
                .HasForeignKey(mdau => mdau.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            #endregion setup MatterDocumentActivityUserFrom linkage

            #region setup MatterDocumentActivityUserTo linkage

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
                .WithMany(dau => dau.MatterDocumentActivityUsersTo)
                .HasForeignKey(mdau => mdau.MatterId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MatterDocumentActivityUserTo>()
                .HasOne(mdau => mdau.Document)
                .WithMany(dau => dau.MatterDocumentActivityUsersTo)
                .HasForeignKey(mdau => mdau.DocumentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MatterDocumentActivityUserTo>()
                .HasOne(mdau => mdau.MatterDocumentActivity)
                .WithMany(dau => dau.MatterDocumentActivityUsersTo)
                .HasForeignKey(mdau => mdau.MatterDocumentActivityId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MatterDocumentActivityUserTo>()
                .HasOne(mdau => mdau.User)
                .WithMany(dau => dau.MatterDocumentActivityUsersTo)
                .HasForeignKey(mdau => mdau.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            #endregion setup MatterDocumentActivityUserTo linkage

            #region Add New Entities

            modelBuilder.Entity<DocumentActivity>()
                .HasData(
                    documentActivity1,
                    documentActivity2,
                    documentActivity3,
                    documentActivity4,
                    documentActivity5,
                    documentActivity6
                );

            modelBuilder.Entity<MatterActivity>()
                .HasData(
                    matterActivity1,
                    matterActivity2,
                    matterActivity3,
                    matterActivity4,
                    matterActivity5
                );

            modelBuilder.Entity<MatterDocumentActivity>()
                .HasData(
                    matterDocumentActivity1,
                    matterDocumentActivity2
                );

            modelBuilder.Entity<RevisionActivity>()
                .HasData(
                    revisionActivity1,
                    revisionActivity2,
                    revisionActivity3,
                    revisionActivity4
                );

            modelBuilder.Entity<User>()
                .HasData(
                    user1
                );

            modelBuilder.Entity<Matter>()
                .HasData(
                    matter1,
                    matter2,
                    matter3,
                    matter4,
                    matter5,
                    matter6
                );

            modelBuilder.Entity<MatterActivityUser>()
                .HasData(
                    matterActivityUser1,
                    matterActivityUser2,
                    matterActivityUser3,
                    matterActivityUser4,
                    matterActivityUser5,
                    matterActivityUser6,
                    matterActivityUser7,
                    matterActivityUser8,
                    matterActivityUser9,
                    matterActivityUser10,
                    matterActivityUser11,
                    matterActivityUser12
                );

            #endregion Add New Entities

            base.OnModelCreating(modelBuilder);
        }
    }
}
