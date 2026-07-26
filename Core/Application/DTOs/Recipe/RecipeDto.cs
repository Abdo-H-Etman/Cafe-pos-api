namespace Core.Application.DTOs.Recipe;

public record RecipeDto
{
    public Guid Id { get; init; }
    public string Product { get; init; } = null!;
    public string Ingredient { get; init; } = null!;
    public decimal Quantity { get; init; }
}