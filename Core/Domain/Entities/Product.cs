using Core.Domain.Entities.Generics;

namespace Core.Domain.Entities;

public class Product : IdModel
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<Recipe> Recipes { get; set; } = [];
}