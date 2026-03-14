using Core.Domain.Entities.Generics;

namespace Core.Domain.Entities;

public class Category : IdModel
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public ICollection<Product> Products { get; set;} = [];
}