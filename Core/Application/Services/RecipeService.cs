using Core.Application.Interfaces;
using Application.Interfaces.Logging;
using Core.Domain.Interfaces;
using Core.Application.DTOs.Recipe;
using Application.Common.Models;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class RecipeService : IRecipeService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly ILoggerManager _logger;

    public RecipeService(IRepositoryManager repositoryManager, ILoggerManager logger)
    {
        _repositoryManager = repositoryManager;
        _logger = logger;
    }

    public async Task<Result<RecipeDto>> CreateRecipeAsync(CreateRecipeDto createRecipeDto, CancellationToken cancellationToken = default)
    {
        var product = await _repositoryManager.Product.GetByIdAsync(createRecipeDto.ProductId, cancellationToken: cancellationToken);
        if (product is null)
        {
            _logger.LogError("Product with id {ProductId} not found.", createRecipeDto.ProductId);
            return Result<RecipeDto>.Failure($"Product with id {createRecipeDto.ProductId} not found.");
        }

        var ingredient = await _repositoryManager.Ingredient.GetByIdAsync(createRecipeDto.IngredientId, cancellationToken: cancellationToken);
        if (ingredient is null)
        {
            _logger.LogError("Ingredient with id {IngredientId} not found.", createRecipeDto.IngredientId);
            return Result<RecipeDto>.Failure($"Ingredient with id {createRecipeDto.IngredientId} not found.");
        }

        if (createRecipeDto.Quantity <= 0)
        {
            _logger.LogError("Quantity must be greater than zero. Provided quantity: {Quantity}.", createRecipeDto.Quantity);
            return Result<RecipeDto>.Failure("Quantity must be greater than zero.");
        }

        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            ProductId = createRecipeDto.ProductId,
            IngredientId = createRecipeDto.IngredientId,
            Quantity = createRecipeDto.Quantity
        };

        await _repositoryManager.Recipe.AddAsync(recipe, cancellationToken);
        await _repositoryManager.SaveAsync(cancellationToken);

        var recipeFromDb = await _repositoryManager.Recipe.GetByIdAsync(recipe.Id
                            , include: q => q.Include(r => r.Product)
                                        .Include(r => r.Ingredient)
                            , cancellationToken: cancellationToken);

        var recipeDto = MapToRecipeDto(recipeFromDb!);

        return Result<RecipeDto>.Success(recipeDto);
    }

    public async Task<Result<IEnumerable<RecipeDto>>> GetProductRecipesAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _repositoryManager.Product.GetByIdAsync(productId, cancellationToken: cancellationToken);
        if (product is null)
        {
            _logger.LogError("Product with id {ProductId} not found.", productId);
            return Result<IEnumerable<RecipeDto>>.Failure($"Product with id {productId} not found.");
        }

        var recipes = await _repositoryManager.Recipe.FindAsync(r => r.ProductId == productId
                    , include: q => q.Include(r => r.Ingredient)
                                    .Include(r => r.Product)
                    , cancellationToken: cancellationToken);
        var recipeDtos = recipes.Select(MapToRecipeDto);

        return Result<IEnumerable<RecipeDto>>.Success(recipeDtos);
    }

    public async Task<Result<IEnumerable<RecipeDto>>> GetIngredientRecipesAsync(Guid ingredientId, CancellationToken cancellationToken = default)
    {
        var ingredient = await _repositoryManager.Ingredient.GetByIdAsync(ingredientId, cancellationToken: cancellationToken);
        if (ingredient is null)
        {
            _logger.LogError("Ingredient with id {IngredientId} not found.", ingredientId);
            return Result<IEnumerable<RecipeDto>>.Failure($"Ingredient with id {ingredientId} not found.");
        }

        var recipes = await _repositoryManager.Recipe.FindAsync(r => r.IngredientId == ingredientId
                        , include: q => q.Include(r => r.Product)
                                        .Include(r => r.Ingredient)
                        , cancellationToken: cancellationToken);
        var recipeDtos = recipes.Select(MapToRecipeDto);

        return Result<IEnumerable<RecipeDto>>.Success(recipeDtos);
    }

    public async Task<Result<RecipeDto>> UpdateRecipeAsync(Guid id, UpdateRecipeDto updateRecipeDto,
                    CancellationToken cancellationToken = default)
    {
        var recipe = await _repositoryManager.Recipe.GetByIdAsync(id
                , include: q => q.Include(r => r.Product)
                                .Include(r => r.Ingredient)
                , cancellationToken: cancellationToken);
        if (recipe is null)
        {
            _logger.LogError("Recipe with id {RecipeId} not found.", id);
            return Result<RecipeDto>.Failure($"Recipe with id {id} not found.");
        }

        if (updateRecipeDto.ProductId != null)
        {
            var product = await _repositoryManager.Product.GetByIdAsync((Guid)updateRecipeDto.ProductId, cancellationToken: cancellationToken);
            if (product is null)
            {
                _logger.LogError("Product with id {ProductId} not found.", updateRecipeDto.ProductId);
                return Result<RecipeDto>.Failure($"Product with id {updateRecipeDto.ProductId} not found.");
            }
        }

        if (updateRecipeDto.IngredientId != null)
        {
            var ingredient = await _repositoryManager.Ingredient.GetByIdAsync((Guid)updateRecipeDto.IngredientId, cancellationToken: cancellationToken);
            if (ingredient is null)
            {
                _logger.LogError("Ingredient with id {IngredientId} not found.", updateRecipeDto.IngredientId);
                return Result<RecipeDto>.Failure($"Ingredient with id {updateRecipeDto.IngredientId} not found.");
            }
        }

        if (updateRecipeDto.Quantity <= 0)
        {
            _logger.LogError("Quantity must be greater than zero. Provided quantity: {Quantity}.", updateRecipeDto.Quantity);
            return Result<RecipeDto>.Failure("Quantity must be greater than zero.");
        }

        recipe.ProductId = updateRecipeDto.ProductId ?? recipe.ProductId;
        recipe.IngredientId = updateRecipeDto.IngredientId ?? recipe.IngredientId;
        recipe.Quantity = updateRecipeDto.Quantity ?? recipe.Quantity;

        await _repositoryManager.SaveAsync(cancellationToken);

        var recipeDto = MapToRecipeDto(recipe);

        return Result<RecipeDto>.Success(recipeDto);
    }

    public async Task<Result> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var recipe = await _repositoryManager.Recipe.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (recipe is null)
        {
            _logger.LogError("Recipe with id {RecipeId} not found.", id);
            return Result.Failure($"Recipe with id {id} not found.");
        }

        _repositoryManager.Recipe.Remove(recipe);
        await _repositoryManager.SaveAsync(cancellationToken);

        return Result.Success();
    }
    private RecipeDto MapToRecipeDto(Recipe recipe)
    {
        return new RecipeDto
        {
            Id = recipe.Id,
            Product = recipe.Product.Name,
            Ingredient = recipe.Ingredient.Name,
            Quantity = recipe.Quantity
        };
    }
}