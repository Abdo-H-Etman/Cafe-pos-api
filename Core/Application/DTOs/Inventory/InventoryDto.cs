namespace Core.Application.DTOs.Inventory;

public record InventoryDto
{
    public Guid Id { get; init; }
    public Guid IngredientId { get; init; }
    public string IngredientName { get; init; } = null!;
    public decimal CurrentStock { get; init; }
    public decimal MinStock { get; init; }
    public string Unit { get; init; } = null!;
    public bool IsLowStock { get; init; }
}
