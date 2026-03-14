using Core.Domain.Entities.Generics;

namespace Core.Domain.Entities;

public class Inventory : IdModel
{
    public Guid IngredientId { get; set; }
    public Guid BranchId { get; set; }
    public decimal CurrentStock { get; set; }

    public Ingredient Ingredient { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}