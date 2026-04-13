namespace Core.Application.DTOs.Stock;

public record UpdateStockMovementDto
{
    public decimal? Quantity { get; init; }
    public string? MovementType { get; init; }
}