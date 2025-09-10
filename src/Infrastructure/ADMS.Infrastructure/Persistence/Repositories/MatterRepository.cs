using ADMS.Application.Common.Interfaces;
using ADMS.Domain.Entities;
using ADMS.Domain.ValueObjects;
using ADMS.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ADMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Matter entities with comprehensive querying and persistence capabilities.
/// </summary>
public class MatterRepository(AdmsDbContext context) : Repository<Matter, MatterId>(context), IMatterRepository
{
    public async Task<bool> ExistsByDescriptionAsync(string description, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(m => !m.IsDeleted)
            .AnyAsync(m => m.Description.Value == description, cancellationToken);
    }

    public async Task<Matter?> GetByIdAsync(MatterId id, bool includeDocuments = false, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (includeDocuments)
        {
            query = query.Include(m => m.Documents.Where(d => !d.IsDeleted));
        }

        return await query.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Matter>> GetActiveMattersAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(m => m.IsActive())
            .OrderBy(m => m.Description.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Matter>> SearchByDescriptionAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(m => !m.IsDeleted && m.Description.Value.Contains(searchTerm))
            .OrderBy(m => m.Description.Value)
            .ToListAsync(cancellationToken);
    }
}