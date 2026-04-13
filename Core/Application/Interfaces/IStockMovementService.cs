using Application.Common.Models;
using Core.Application.DTOs.Stock;

namespace Application.Interfaces;

public interface IStockMovementService
{
    Task<Result<IEnumerable<StockMovementDto>, MetaData>> GetAllAsync(int pageNumber, int pageSize, Guid? branchId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<StockMovementDto>, MetaData>> GetByIngredientIdAsync(int pageNumber, int pageSize, Guid ingredientId,
                                            Guid? branchId, CancellationToken cancellationToken = default);
    Task<Result<StockMovementDto?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<StockMovementDto>> CreateAsync(CreateStockMovementDto dto, Guid? branchId = null, CancellationToken cancellationToken = default);
    Task<Result<StockMovementDto?>> UpdateAsync(Guid id, UpdateStockMovementDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}