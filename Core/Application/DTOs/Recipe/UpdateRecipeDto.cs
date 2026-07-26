namespace Core.Application.DTOs.Recipe;

public record UpdateRecipeDto
{
    public Guid? ProductId { get; init; }
    public Guid? IngredientId { get; init; }
    public decimal? Quantity { get; init; }
}