using Core.Domain.Entities.Generics;

namespace Core.Domain.Models;

public class Category : IdModel
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}