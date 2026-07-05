using Core.Domain.Entities.Generics;

namespace Core.Domain.Entities;

public class Table : IdModel
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = null!;
    public int Capacity { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Branch Branch { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = [];
}
