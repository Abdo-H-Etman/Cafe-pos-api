using Core.Domain.Entities.Generics;

namespace Core.Domain.Entities;

public class Branch : IdModel
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public ICollection<User> Users { get; set; } = [];
    public ICollection<BranchProduct> BranchProducts { get; set; } = [];
    public ICollection<StockMovement> StockMovements { get; set; } = [];
}