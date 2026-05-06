using Application.Common;
using Application.Common.Models;
using Application.Interfaces;
using Application.Interfaces.Logging;
using Core.Application.DTOs.Ingredient;
using Core.Application.DTOs.Stock;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Core.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class IngredientService : IIngredientService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerManager _logger;

    public IngredientService(IRepositoryManager repositoryManager, ILoggerManager logger, ICurrentUserService currentUserService)
    {
        _repositoryManager = repositoryManager;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IngredientDto>> CreateIngredientAsync(CreateIngredientDto createIngredientDto, CancellationToken cancellationToken)
    {
        var existingIngredient = await _repositoryManager.Ingredient
            .FirstOrDefaultAsync(i => i.Name == createIngredientDto.Name, cancellationToken);
        if (existingIngredient != null)
        {
            _logger.LogWarn("Attempt to create duplicate ingredient: {ingredientName}", createIngredientDto.Name);
            return Result<IngredientDto>.Failure("An ingredient with the same name already exists.");
        }

        var ingredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = createIngredientDto.Name,
            MinStock = createIngredientDto.MinStock,
            Unit = Enum.TryParse(createIngredientDto.Unit, out UnitType unit) ? unit : UnitType.Gram
        };

        var createdIngredient = await _repositoryManager.Ingredient.AddAsync(ingredient, cancellationToken);

        var branches = await _repositoryManager.Branch.GetAllAsync(cancellationToken);
        foreach (var branch in branches)
        {
            var newInventory = new Inventory
            {
                Id = Guid.NewGuid(),
                BranchId = branch.Id,
                IngredientId = createdIngredient.Id,
                CurrentStock = 0
            };
            await _repositoryManager.Inventory.AddAsync(newInventory, cancellationToken);
        }
        await _repositoryManager.SaveAsync(cancellationToken);

        var ingredientDto = MapToIngredientDto(createdIngredient);

        _logger.LogInfo("Ingredient created successfully with ID: {ingredientId}", ingredientDto.Id);
        return Result<IngredientDto>.Success(ingredientDto);
    }

    public async Task<Result<IngredientDto>> GetIngredientByIdAsync(Guid id, Guid? adminSelectedBranchId, CancellationToken cancellationToken)
    {
        var ingredient = await _repositoryManager.Ingredient.GetByIdAsync(id,
            include: q =>
            {
                if (_currentUserService.IsAdmin() && adminSelectedBranchId.HasValue)
                {
                    return q.Include(i => i.StockMovements.Where(sm => sm.BranchId == adminSelectedBranchId.Value))
                            .Include(i => i.Inventories.Where(inv => inv.BranchId == adminSelectedBranchId.Value));
                }

                return q.Include(i => i.StockMovements)
                        .Include(i => i.Inventories);
            }, cancellationToken);
        if (ingredient == null)
        {
            return Result<IngredientDto>.Failure("Ingredient not found.");
        }

        var ingredientDto = MapToIngredientDto(ingredient);

        _logger.LogInfo("Ingredient retrieved successfully with ID: {ingredientId}", ingredientDto.Id);
        return Result<IngredientDto>.Success(ingredientDto);
    }

    public async Task<Result<IngredientDto>> UpdateIngredientAsync(Guid id, Guid? adminSelectedBranchId,
        UpdateIngredientDto updateIngredientDto, CancellationToken cancellationToken)
    {
        var ingredient = await _repositoryManager.Ingredient.GetByIdAsync(id,
            include: q =>
            {
                if (_currentUserService.IsAdmin() && adminSelectedBranchId.HasValue)
                {
                    return q.Include(i => i.StockMovements.Where(sm => sm.BranchId == adminSelectedBranchId.Value))
                            .Include(i => i.Inventories.Where(inv => inv.BranchId == adminSelectedBranchId.Value));
                }

                return q.Include(i => i.StockMovements)
                        .Include(i => i.Inventories);
            }, cancellationToken);
        if (ingredient == null)
        {
            return Result<IngredientDto>.Failure("Ingredient not found.");
        }

        var duplicateIngredient = await _repositoryManager.Ingredient
            .FirstOrDefaultAsync(i => i.Name == updateIngredientDto.Name && i.Id != id, cancellationToken);
        if (duplicateIngredient != null)
        {
            _logger.LogWarn("Attempt to update ingredient to a duplicate name: {ingredientName}", updateIngredientDto.Name!);
            return Result<IngredientDto>.Failure("An ingredient with the same name already exists.");
        }

        ingredient.Name = updateIngredientDto.Name ?? ingredient.Name;
        ingredient.MinStock = updateIngredientDto.MinStock ?? ingredient.MinStock;
        ingredient.Unit = Enum.TryParse(updateIngredientDto.Unit, out UnitType unit) ? unit : ingredient.Unit;

        await _repositoryManager.SaveAsync(cancellationToken);

        var ingredientDto = MapToIngredientDto(ingredient);

        _logger.LogInfo("Ingredient updated successfully with ID: {ingredientId}", ingredientDto.Id);
        return Result<IngredientDto>.Success(ingredientDto);
    }
    public async Task<Result<IEnumerable<IngredientDto>, MetaData>> GetPagedIngredientsAsync(
                int pageNumber, int pageSize, string? searchTerm,
                Guid? adminSelectedBranchId,
                CancellationToken cancellationToken)
    {
        var (ingredients, totalCount) = await _repositoryManager.Ingredient.GetPagedAsync(
            pageNumber,
            pageSize,
            i => string.IsNullOrEmpty(searchTerm) || i.Name.ToLower().Contains(searchTerm.ToLower()),
            include: q =>
            {
                if (_currentUserService.IsAdmin() && adminSelectedBranchId.HasValue)
                {
                    return q.Include(i => i.StockMovements.Where(sm => sm.BranchId == adminSelectedBranchId.Value))
                            .Include(i => i.Inventories.Where(inv => inv.BranchId == adminSelectedBranchId.Value));
                }

                return q.Include(i => i.StockMovements)
                        .Include(i => i.Inventories);
            },
            cancellationToken);

        var ingredientDtos = ingredients.Select(MapToIngredientDto).ToList();
        var metaData = new MetaData(pageNumber, pageSize, totalCount);

        _logger.LogInfo("Paged ingredients retrieved successfully. Page: {pageNumber}, Size: {pageSize}", pageNumber, pageSize);
        return Result<IEnumerable<IngredientDto>, MetaData>.Success(ingredientDtos, metaData);
    }

    public async Task<Result> DeleteIngredientAsync(Guid id, CancellationToken cancellationToken)
    {
        var ingredient = await _repositoryManager.Ingredient.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (ingredient == null)
        {
            return Result.Failure("Ingredient not found.");
        }

        _repositoryManager.Ingredient.Remove(ingredient);
        await _repositoryManager.SaveAsync(cancellationToken);

        _logger.LogInfo("Ingredient deleted successfully with ID: {ingredientId}", id);
        return Result.Success("Ingredient deleted successfully.");
    }

    private static IngredientDto MapToIngredientDto(Ingredient ingredient) =>
        new IngredientDto
        {
            Id = ingredient.Id,
            Name = ingredient.Name,
            MinStock = ingredient.MinStock,
            Unit = ingredient.Unit.ToString(),
            currentStocks = ingredient.Inventories.Select(i => i.CurrentStock),
            StockMovements = ingredient.StockMovements.Select(sm => new StockMovementDto
            {
                Id = sm.Id,
                IngredientName = ingredient.Name,
                Quantity = sm.Quantity,
                MovementType = sm.Type.ToString(),
                Date = sm.CreatedAt
            })
        };
}