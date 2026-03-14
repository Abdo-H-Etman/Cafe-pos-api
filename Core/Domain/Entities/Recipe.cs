namespace Core.Domain.Entities;

public class Recipe
{
    public Guid ProductId { get; set; }
    public Guid IngredientId { get; set; }
    public decimal Quantity { get; set; }

    public Product Product { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}