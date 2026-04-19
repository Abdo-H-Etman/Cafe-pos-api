using Application.Common.Models;
using Core.Application.DTOs.Inventory;

namespace Core.Application.Interfaces;

public interface IInventoryService
{
    Task<Result<IEnumerable<InventoryDto>>> GetBranchInventoryAsync(Guid? branchId = null, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<InventoryDto>>> GetLowStockAsync(Guid? branchId = null, CancellationToken cancellationToken = default);
}
