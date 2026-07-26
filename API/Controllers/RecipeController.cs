using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.Application.Interfaces;
using Core.Application.DTOs.Recipe;

namespace API.Controllers;

[ApiController]
[Route("api/recipes")]
[Authorize(Roles = "Manager, Admin")]
public class RecipeController : ControllerBase
{
    private readonly IRecipeService _recipeService;

    public RecipeController(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecipeDto createRecipeDto, CancellationToken cancellationToken)
    {
        var result = await _recipeService.CreateRecipeAsync(createRecipeDto, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(null, new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetProductRecipes(Guid productId, CancellationToken cancellationToken)
    {
        var result = await _recipeService.GetProductRecipesAsync(productId, cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpGet("ingredient/{ingredientId:guid}")]
    public async Task<IActionResult> GetIngredientRecipes(Guid ingredientId, CancellationToken cancellationToken)
    {
        var result = await _recipeService.GetIngredientRecipesAsync(ingredientId, cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRecipeDto updateRecipeDto, CancellationToken cancellationToken)
    {
        var result = await _recipeService.UpdateRecipeAsync(id, updateRecipeDto, cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _recipeService.DeleteRecipeAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
}