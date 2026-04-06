using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BranchRepository : Repository<Branch>, IBranchRepository
{
    public BranchRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Branch?> GetBranchWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbSet
            .Include(b => b.Users)
            .Include(b => b.BranchProducts)
                .ThenInclude(bp => bp.Product)
            .Include(b => b.Orders)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .Where(b => b.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
}