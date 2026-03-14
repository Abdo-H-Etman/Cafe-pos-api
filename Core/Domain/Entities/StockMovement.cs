using Core.Domain.Entities.Enums;
using Core.Domain.Entities.Generics;

namespace Core.Domain.Entities;

public class StockMovement : IdModel
{
    public Guid IngredientId { get; set; }
    public Guid BranchId { get; set; }
    public int Quantity { get; set; }
    public MovementType Type { get; set; }
    public Guid ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}