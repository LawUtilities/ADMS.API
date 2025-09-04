using ADMS.Domain.Common;
using ADMS.Domain.Entities;
using ADMS.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ADMS.Infrastructure.Persistence.Context;

public sealed class AdmsDbContext : DbContext
{
    public AdmsDbContext(DbContextOptions<AdmsDbContext> options) : base(options) { }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Matter> Matters => Set<Matter>();
    public DbSet<Revision> Revisions => Set<Revision>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocumentConfiguration).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Enable sensitive data logging in development only
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        var domainEvents = ChangeTracker.Entries<Entity<object>>()
            .Where(x => x.Entity.DomainEvents.Any())
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear domain events after successful save
        foreach (var entry in ChangeTracker.Entries<Entity<object>>())
        {
            entry.Entity.ClearDomainEvents();
        }

        // Here you would dispatch domain events to handlers
        // This would typically be done through MediatR or a domain event dispatcher

        return result;
    }
}