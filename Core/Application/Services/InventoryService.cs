using Application.Common;
using Application.Common.Models;
using Core.Application.DTOs.Inventory;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Interfaces;

namespace Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IRepositoryManager _repository;
    private readonly ICurrentUserService _currentUserService;

    public InventoryService(IRepositoryManager repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IEnumerable<InventoryDto>>> GetBranchInventoryAsync(Guid? branchId = null, CancellationToken cancellationToken = default)
    {
        var id = !_currentUserService.IsAdmin() ? _currentUserService.BranchId : branchId;

        if (id == null)
            return Result<IEnumerable<InventoryDto>>.Failure("Branch ID is required for admins.");

        var inventory = await _repository.Inventory.GetInventoryByBranchAsync(id.Value, cancellationToken);
        var dtos = inventory.Select(MapToDto);

        return Result<IEnumerable<InventoryDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<InventoryDto>>> GetLowStockAsync(Guid? branchId = null, CancellationToken cancellationToken = default)
    {
        var id = !_currentUserService.IsAdmin() ? _currentUserService.BranchId : branchId;

        if (id == null)
            return Result<IEnumerable<InventoryDto>>.Failure("Branch ID is required for admins.");

        var inventory = await _repository.Inventory.GetLowStockByBranchAsync(id.Value, cancellationToken);
        var dtos = inventory.Select(MapToDto);

        return Result<IEnumerable<InventoryDto>>.Success(dtos);
    }

    private InventoryDto MapToDto(Inventory inventory)
    {
        return new InventoryDto
        {
            Id = inventory.Id,
            IngredientId = inventory.IngredientId,
            IngredientName = inventory.Ingredient.Name,
            CurrentStock = inventory.CurrentStock,
            MinStock = inventory.Ingredient.MinStock,
            Unit = inventory.Ingredient.Unit.ToString(),
            IsLowStock = inventory.CurrentStock <= inventory.Ingredient.MinStock
        };
    }
}
