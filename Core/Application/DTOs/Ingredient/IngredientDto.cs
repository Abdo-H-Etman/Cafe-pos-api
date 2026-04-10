using Core.Application.DTOs.Inventory;
using Core.Application.DTOs.Stock;

namespace Core.Application.DTOs.Ingredient;

public record IngredientDto : CreateIngredientDto
{
    public Guid Id { get; init; }
    public IEnumerable<decimal> currentStocks { get; init; } = null!;
    public IEnumerable<StockMovementDto> StockMovements { get; init; } = [];
}