using Application.Common.Models;
using Core.Application.DTOs.Ingredient;

namespace Application.Interfaces;

public interface IIngredientService
{
    Task<Result<IngredientDto>> CreateIngredientAsync(CreateIngredientDto createIngredientDto, CancellationToken cancellationToken = default);
    Task<Result<IngredientDto>> GetIngredientByIdAsync(Guid id, Guid? adminSelectedBranchId, CancellationToken cancellationToken = default);
    Task<Result<IngredientDto>> UpdateIngredientAsync(Guid id, Guid? adminSelectedBranchId, UpdateIngredientDto updateIngredientDto, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<IngredientDto>, MetaData>> GetPagedIngredientsAsync(int pageNumber,
                int pageSize, string? searchTerm = null,
                Guid? adminSelectedBranchId = null,
                CancellationToken cancellationToken = default);
    Task<Result> DeleteIngredientAsync(Guid id, CancellationToken cancellationToken = default);
}