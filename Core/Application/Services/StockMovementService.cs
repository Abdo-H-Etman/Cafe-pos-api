using Application.Common;
using Application.Common.Models;
using Application.Interfaces;
using Application.Interfaces.Logging;
using Core.Application.DTOs.Stock;
using Core.Domain.Entities;
using Core.Domain.Entities.Enums;
using Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class StockMovementService : IStockMovementService
{
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;
    private readonly ICurrentUserService _currentUserService;

    public StockMovementService(IRepositoryManager repository, ILoggerManager logger, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<StockMovementDto>> CreateAsync(CreateStockMovementDto createStockMovementDto, Guid? branchId = null, CancellationToken cancellationToken = default)
    {
        var stockMovement = MapToStockMovement(createStockMovementDto, branchId);
        
        var createdStockMovement = await _repository.StockMovement.AddAsync(stockMovement, cancellationToken);
        await _repository.SaveAsync(cancellationToken);

        decimal delta = CalculateDelta(createdStockMovement.Type, createdStockMovement.Quantity);
        if (delta != 0)
        {
            await _repository.Inventory.UpdateStockAsync(createdStockMovement.BranchId, createdStockMovement.IngredientId, delta, cancellationToken);
        }

        var createdStockMovementWithIngredient = await _repository.StockMovement.GetByIdAsync(createdStockMovement.Id, 
            q => q.Include(sm => sm.Ingredient), cancellationToken);
        var stockMovementDto = MapToStockMovementDto(createdStockMovementWithIngredient!);

        _logger.LogInfo("Stock movement created successfully with ID: {stockMovementId}", stockMovementDto.Id);
        return Result<StockMovementDto>.Success(stockMovementDto);
    }

    public async Task<Result<IEnumerable<StockMovementDto>, MetaData>> GetAllAsync(int pageNumber, int pageSize, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var (stockMovements, totalCount) = await _repository.StockMovement.GetPagedAsync(pageNumber, pageSize,
                                predicate: sm => !branchId.HasValue || sm.BranchId == branchId.Value,
                                include: q => q.Include(sm => sm.Ingredient), cancellationToken: cancellationToken);

        var stockMovementDtos = stockMovements.Select(MapToStockMovementDto).ToList();
        var metaData = new MetaData(pageNumber, pageSize, totalCount);

        _logger.LogInfo("Retrieved paged stock movements page {pageNumber} of {pageSize} successfully.", pageNumber, pageSize);
        return Result<IEnumerable<StockMovementDto>, MetaData>.Success(stockMovementDtos, metaData);
    }

    public async Task<Result<IEnumerable<StockMovementDto>, MetaData>> GetByIngredientIdAsync(int pageNumber, int pageSize, Guid ingredientId, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var (stockMovements, totalCount) = await _repository.StockMovement.GetPagedAsync(pageNumber, pageSize,
                                predicate: sm => sm.IngredientId == ingredientId && (!branchId.HasValue || sm.BranchId == branchId.Value),
                                include: q => q.Include(sm => sm.Ingredient), cancellationToken: cancellationToken);

        var stockMovementDtos = stockMovements.Select(MapToStockMovementDto).ToList();
        var metaData = new MetaData(pageNumber, pageSize, totalCount);

        _logger.LogInfo("Retrieved stock movements for ingredient ID: {ingredientId} successfully Page {pageNumber} of {pageSize}.",
                ingredientId, pageNumber, pageSize);
        return Result<IEnumerable<StockMovementDto>, MetaData>.Success(stockMovementDtos, metaData);
    }

    public async Task<Result<StockMovementDto?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var stockMovement = await _repository.StockMovement.GetByIdAsync(id, include: q => q.Include(sm => sm.Ingredient),
                                            cancellationToken);
        if (stockMovement == null)
        {
            _logger.LogWarn("Stock movement with ID: {stockMovementId} not found.", id);
            return Result<StockMovementDto?>.Failure("Stock movement not found.");
        }

        var stockMovementDto = MapToStockMovementDto(stockMovement);

        _logger.LogInfo("Stock movement with ID: {stockMovementId} retrieved successfully.", id);
        return Result<StockMovementDto?>.Success(stockMovementDto);
    }

    public async Task<Result<StockMovementDto?>> UpdateAsync(Guid id, UpdateStockMovementDto updateStockMovementDto, CancellationToken cancellationToken = default)
    {
        var stockMovement = await _repository.StockMovement.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (stockMovement == null)
        {
            _logger.LogWarn("Stock movement with ID: {stockMovementId} not found for update.", id);
            return Result<StockMovementDto?>.Failure("Stock movement not found.");
        }

        if(stockMovement.Type == MovementType.Sale || stockMovement.Type == MovementType.Waste)
        {
            _logger.LogWarn("Stock movement with ID: {stockMovementId} is of type {movementType} and cannot be updated.", id, stockMovement.Type);
            return Result<StockMovementDto?>.Failure($"Stock movement of type {stockMovement.Type} cannot be updated.");
        }

        // 1. Undo old effect
        decimal oldDelta = CalculateDelta(stockMovement.Type, stockMovement.Quantity);
        if (oldDelta != 0)
        {
            await _repository.Inventory.UpdateStockAsync(stockMovement.BranchId, stockMovement.IngredientId, -oldDelta, cancellationToken);
        }

        // 2. Update stock movement entity
        if(updateStockMovementDto.Quantity != null)
        {
            stockMovement.Quantity = updateStockMovementDto.Quantity.Value;
        }

        if(updateStockMovementDto.MovementType != null)
        {
            if(stockMovement.Type == MovementType.Sale || stockMovement.Type == MovementType.Waste)
            {
                _logger.LogWarn("Stock movement with ID: {stockMovementId} is of type {movementType} and cannot have its type updated.", id, stockMovement.Type);

                await _repository.Inventory.UpdateStockAsync(stockMovement.BranchId, stockMovement.IngredientId, oldDelta, cancellationToken);
                return Result<StockMovementDto?>.Failure($"Stock movement of type {stockMovement.Type} cannot have its type updated.");
            }

            if(updateStockMovementDto.MovementType == MovementType.Sale.ToString())
            {
                _logger.LogWarn("Stock movement with ID: {stockMovementId} cannot be updated to type Sale.", id);

                await _repository.Inventory.UpdateStockAsync(stockMovement.BranchId, stockMovement.IngredientId, oldDelta, cancellationToken);
                return Result<StockMovementDto?>.Failure($"Stock movement cannot be updated to type Sale.");
            }
            
            if(Enum.TryParse(updateStockMovementDto.MovementType, out MovementType newMovementType))
            {
                stockMovement.Type = newMovementType;
            }
            else
            {
                _logger.LogWarn("Invalid movement type provided for stock movement with ID: {stockMovementId}.", id);

                await _repository.Inventory.UpdateStockAsync(stockMovement.BranchId, stockMovement.IngredientId, oldDelta, cancellationToken);
                return Result<StockMovementDto?>.Failure("Invalid movement type provided.");
            }
        }

        _repository.StockMovement.Update(stockMovement);
        await _repository.SaveAsync(cancellationToken);

        // 3. Apply new effect
        decimal newDelta = CalculateDelta(stockMovement.Type, stockMovement.Quantity);
        if (newDelta != 0)
        {
            await _repository.Inventory.UpdateStockAsync(stockMovement.BranchId, stockMovement.IngredientId, newDelta, cancellationToken);
        }

        var stockMovementWithIngredient = await _repository.StockMovement.GetByIdAsync(id, include: q => q.Include(sm => sm.Ingredient), cancellationToken);
        var stockMovementDto = MapToStockMovementDto(stockMovementWithIngredient!);

        _logger.LogInfo("Stock movement with ID: {stockMovementId} updated successfully.", id);
        return Result<StockMovementDto?>.Success(stockMovementDto);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var stockMovement = await _repository.StockMovement.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (stockMovement == null)
        {
            _logger.LogWarn("Stock movement with ID: {stockMovementId} not found for deletion.", id);
            return Result.Failure("Stock movement not found.");
        }

        if(stockMovement.Type == MovementType.Sale)
        {
            _logger.LogWarn("Stock movement with ID: {stockMovementId} is of type Sale and cannot be deleted.", id);
            return Result.Failure($"Stock movement of type Sale cannot be deleted.");
        }

        // Undo effect before deleting
        decimal delta = CalculateDelta(stockMovement.Type, stockMovement.Quantity);
        if (delta != 0)
        {
            await _repository.Inventory.UpdateStockAsync(stockMovement.BranchId, stockMovement.IngredientId, -delta, cancellationToken);
        }

        _repository.StockMovement.Remove(stockMovement);
        await _repository.SaveAsync(cancellationToken);

        _logger.LogInfo("Stock movement with ID: {stockMovementId} deleted successfully.", id);
        return Result.Success("Stock movement deleted successfully.");
    }

    private decimal CalculateDelta(MovementType type, decimal quantity)
    {
        return type switch
        {
            MovementType.Adjustment or MovementType.Purchase or MovementType.Refund => quantity,
            MovementType.Sale or MovementType.Waste => -quantity,
            _ => 0
        };
    }

    private StockMovementDto MapToStockMovementDto(StockMovement stockMovement)
    {
         return new StockMovementDto
        {
            Id = stockMovement.Id,
            IngredientName = stockMovement.Ingredient.Name,
            Quantity = stockMovement.Quantity,
            MovementType = stockMovement.Type.ToString(),
            Date = stockMovement.CreatedAt
        };
    }

    private StockMovement MapToStockMovement(CreateStockMovementDto createStockMovementDto, Guid? branchId)
    {
        return new StockMovement
        {
            Id = Guid.NewGuid(),
            BranchId = !_currentUserService.IsAdmin() ? _currentUserService.BranchId : (branchId ?? Guid.Empty),
            IngredientId = createStockMovementDto.IngredientId,
            Quantity = createStockMovementDto.Quantity,
            Type = Enum.TryParse(createStockMovementDto.MovementType, out MovementType movementType) ? movementType : MovementType.Adjustment
        };
    }
}
