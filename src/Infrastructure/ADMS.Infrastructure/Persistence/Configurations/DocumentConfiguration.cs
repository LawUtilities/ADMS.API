using ADMS.Domain.Entities;
using ADMS.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ADMS.Infrastructure.Persistence.Configurations;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        // Primary Key
        builder.HasKey(d => d.Id);

        // Configure DocumentId value object
        builder.Property(d => d.Id)
            .HasConversion(
                id => id.Value,
                value => DocumentId.From(value))
            .ValueGeneratedOnAdd();

        // Configure FileName value object  
        builder.Property(d => d.FileName)
            .HasConversion(
                fileName => fileName.Value,
                value => FileName.FromTrustedSource(value)) // Use trusted source for DB reads
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(d => d.Extension)
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(d => d.FileSize)
            .IsRequired();

        builder.Property(d => d.MimeType)
            .HasMaxLength(255)
            .IsRequired();

        // Configure FileChecksum value object
        builder.Property(d => d.Checksum)
            .HasConversion(
                checksum => checksum.Value,
                value => FileChecksum.FromTrustedSource(value)) // Use trusted source for DB reads
            .HasMaxLength(64)
            .IsRequired();

        // Configure MatterId value object
        builder.Property(d => d.MatterId)
            .HasConversion(
                id => id.Value,
                value => MatterId.From(value))
            .IsRequired();

        builder.Property(d => d.IsCheckedOut)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(d => d.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(d => d.CreationDate)
            .IsRequired();

        // Relationships
        builder.HasOne(d => d.Matter)
            .WithMany(m => m.Documents)
            .HasForeignKey(d => d.MatterId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent accidental matter deletion

        builder.HasMany(d => d.Revisions)
            .WithOne(r => r.Document)
            .HasForeignKey(r => r.DocumentId)
            .OnDelete(DeleteBehavior.Restrict); // Preserve audit trail

        // Indexes for performance
        builder.HasIndex(d => d.MatterId)
            .HasDatabaseName("IX_Documents_MatterId");

        builder.HasIndex(d => d.Checksum)
            .IsUnique()
            .HasDatabaseName("IX_Documents_Checksum");

        builder.HasIndex(d => new { d.IsDeleted, d.MatterId })
            .HasDatabaseName("IX_Documents_IsDeleted_MatterId");

        builder.HasIndex(d => d.CreationDate)
            .HasDatabaseName("IX_Documents_CreationDate");

        builder.HasIndex(d => new { d.MatterId, d.CreationDate })
            .HasDatabaseName("IX_Documents_MatterId_CreationDate");

        // Ignore computed properties
        builder.Ignore(d => d.FullFileName);
        builder.Ignore(d => d.FormattedFileSize);
        builder.Ignore(d => d.HasRevisions);
        builder.Ignore(d => d.RevisionCount);
        builder.Ignore(d => d.DomainEvents);
        builder.Ignore(d => d.LatestRevisionNumber);
        builder.Ignore(d => d.HasActivityHistory);
        builder.Ignore(d => d.TotalActivityCount);
        builder.Ignore(d => d.CanBeDeleted);
        builder.Ignore(d => d.CanBeCheckedOut);
        builder.Ignore(d => d.CanBeCheckedIn);
        builder.Ignore(d => d.IsPdf);
        builder.Ignore(d => d.IsImage);
        builder.Ignore(d => d.IsOfficeDocument);
    }
}