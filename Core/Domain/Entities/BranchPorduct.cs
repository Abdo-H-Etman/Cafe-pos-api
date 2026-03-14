namespace Core.Domain.Entities;

public class BranchProduct
{
    public Guid BranchId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }

    public Branch Branch { get; set;} = null!;
    public Product Product { get; set; } = null!;
}