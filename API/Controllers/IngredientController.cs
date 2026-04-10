using Application.Interfaces;
using Core.Application.DTOs.Ingredient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/ingredients")]
[Authorize(Roles = "Admin,Manager")]
public class IngredientController : ControllerBase
{
    private readonly IIngredientService _ingredientService;

    public IngredientController(IIngredientService ingredientService)
    {
        _ingredientService = ingredientService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateIngredient(CreateIngredientDto createIngredientDto, CancellationToken cancellationToken)
    {
        var result = await _ingredientService.CreateIngredientAsync(createIngredientDto, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result);
        return CreatedAtAction(nameof(GetIngredientById), new { id = result.Data!.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetIngredientById(Guid id, [FromQuery] Guid? adminSelectedBranchId, CancellationToken cancellationToken)
    {
        var result = await _ingredientService.GetIngredientByIdAsync(id, adminSelectedBranchId, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(result);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedIngredients(
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null, [FromQuery] Guid? adminSelectedBranchId = null,
            CancellationToken cancellationToken = default)
    {
        var result = await _ingredientService.GetPagedIngredientsAsync(pageNumber, pageSize, searchTerm, adminSelectedBranchId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateIngredient(Guid id, UpdateIngredientDto updateIngredientDto,
                    [FromQuery] Guid? adminSelectedBranchId, CancellationToken cancellationToken)
    {
        var result = await _ingredientService.UpdateIngredientAsync(id, adminSelectedBranchId, updateIngredientDto, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIngredient(Guid id, CancellationToken cancellationToken)
    {
        var result = await _ingredientService.DeleteIngredientAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(result);
        return Ok(result);
    }
}