using Core.Domain.Entities.Generics;
using Core.Domain.Models.Enums;

namespace Core.Domain.Entities;

public class Table : IdModel
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = null!;
    public int Capacity { get; set; }
    public TableStatus Status { get; set; } = TableStatus.Available;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Branch Branch { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = [];
}
