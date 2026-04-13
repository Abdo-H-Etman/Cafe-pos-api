namespace Core.Application.DTOs.Stock;

public record CreateStockMovementDto
{
    public Guid IngredientId { get; init; }
    public decimal Quantity { get; init; }
    public string MovementType { get; init; } = null!;
}