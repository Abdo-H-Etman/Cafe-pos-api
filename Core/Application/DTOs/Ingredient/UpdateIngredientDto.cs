namespace Core.Application.DTOs.Ingredient;

public record UpdateIngredientDto
{
    public string? Name { get; init; }
    public decimal? MinStock { get; init; }
    public string? Unit { get; init; }
}