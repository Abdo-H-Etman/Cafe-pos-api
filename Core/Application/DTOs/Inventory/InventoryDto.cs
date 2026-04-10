namespace Core.Application.DTOs.Inventory;

public record InventoryDto
{
    public decimal CurrentStock { get; init; }
}