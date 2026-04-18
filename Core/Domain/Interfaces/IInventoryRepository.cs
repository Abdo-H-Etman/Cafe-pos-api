using Core.Domain.Entities;

namespace Core.Domain.Interfaces;

public interface IInventoryRepository : IRepository<Inventory>
{
    Task UpdateStockAsync(Guid branchId, Guid ingredientId, decimal quantityDelta, CancellationToken cancellationToken = default);
}
