using Core.Domain.Entities.Generics;

namespace Core.Domain.Models;

public class Product : IdModel
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsActive { get; set;}

    public Category Category { get; set; } = null!;
}