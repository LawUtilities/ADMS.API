using ADMS.Domain.Entities;

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ADMS.Infrastructure.Persistence.Configurations;

public sealed class MatterConfiguration : IEntityTypeConfiguration<Matter>
{
    public void Configure(EntityTypeBuilder<Matter> builder)
    {
        builder.ToTable("Matters");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasConversion(
                id => id.Value,
                value => MatterId.From(value))
            .ValueGeneratedOnAdd();

        builder.Property(m => m.Description)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(m => m.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.CreationDate)
            .IsRequired();

        // Relationships
        builder.HasMany(m => m.Documents)
            .WithOne(d => d.Matter)
            .HasForeignKey(d => d.MatterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(m => m.Description)
            .IsUnique()
            .HasDatabaseName("IX_Matters_Description");

        builder.HasIndex(m => new { m.IsDeleted, m.IsArchived })
            .HasDatabaseName("IX_Matters_IsDeleted_IsArchived");

        // Ignore computed properties  
        builder.Ignore(m => m.HasDocuments);
        builder.Ignore(m => m.DocumentCount);
        builder.Ignore(m => m.ActiveDocumentCount);
        builder.Ignore(m => m.AgeDays);
        builder.Ignore(m => m.Status);
        builder.Ignore(m => m.DomainEvents);
    }
}