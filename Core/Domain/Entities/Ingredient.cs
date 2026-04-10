using Core.Domain.Entities.Generics;
using Core.Domain.Models.Enums;

namespace Core.Domain.Entities;

public class Ingredient : IdModel
{
    public string Name { get; set; } = null!;
    public UnitType Unit { get; set; }
    public decimal MinStock { get; set; }
    public ICollection<StockMovement> StockMovements { get; set; } = [];
    public ICollection<Inventory> Inventories { get; set; } = [];
}