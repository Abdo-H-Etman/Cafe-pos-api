namespace Core.Application.DTOs.Stock;

public record StockMovementDto
{
    public Guid Id { get; init; }
    public string IngredientName { get; init; } = null!;
    public decimal Quantity { get; init; }
    public string MovementType { get; init; } = null!;
    public DateTime Date { get; init; }
}