namespace Core.Application.DTOs.Ingredient;

public record CreateIngredientDto
{
    public string Name { get; init; } = null!;
    public decimal MinStock { get; init; }
    public string Unit { get; init; } = null!;
}