using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class InventoryRepository : Repository<Inventory>, IInventoryRepository
{
    public InventoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task UpdateStockAsync(Guid branchId, Guid ingredientId, decimal quantityDelta, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(i => i.BranchId == branchId && i.IngredientId == ingredientId)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.CurrentStock, i => i.CurrentStock + quantityDelta), cancellationToken);
    }

    public async Task<IEnumerable<Inventory>> GetLowStockByBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Ingredient)
            .Where(i => i.BranchId == branchId && i.CurrentStock <= i.Ingredient.MinStock)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Inventory>> GetInventoryByBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Ingredient)
            .Where(i => i.BranchId == branchId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
