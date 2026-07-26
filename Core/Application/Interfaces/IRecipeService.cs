using Application.Common.Models;
using Core.Application.DTOs.Recipe;

namespace Core.Application.Interfaces;

public interface IRecipeService
{
    public Task<Result<RecipeDto>> CreateRecipeAsync(CreateRecipeDto createRecipeDto, CancellationToken cancellationToken = default);
    public Task<Result<IEnumerable<RecipeDto>>> GetProductRecipesAsync(Guid productId, CancellationToken cancellationToken = default);
    public Task<Result<IEnumerable<RecipeDto>>> GetIngredientRecipesAsync(Guid ingredientId, CancellationToken cancellationToken = default);
    public Task<Result<RecipeDto>> UpdateRecipeAsync(Guid id, UpdateRecipeDto updateRecipeDto, CancellationToken cancellationToken = default);
    public Task<Result> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken = default);
}