using Application.Interfaces;
using Core.Application.DTOs.Stock;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/stock-movements")]
[Authorize(Roles = "Manager")]
public class StockMovementController : ControllerBase
{
    private readonly IStockMovementService _stockMovementService;

    public StockMovementController(IStockMovementService stockMovementService)
    {
        _stockMovementService = stockMovementService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateStockMovement([FromBody] CreateStockMovementDto createStockMovementDto)
    {
        var result = await _stockMovementService.CreateAsync(dto: createStockMovementDto);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetStockMovementById(Guid id)
    {
        var result = await _stockMovementService.GetByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllStockMovements([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] Guid? branchId = null)
    {
        var result = await _stockMovementService.GetAllAsync(pageNumber, pageSize, branchId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("by-ingredient/{ingredientId:guid}")]
    public async Task<IActionResult> GetStockMovementsByIngredientId([FromRoute] Guid ingredientId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] Guid? branchId = null)
    {
        var result = await _stockMovementService.GetByIngredientIdAsync(pageNumber, pageSize, ingredientId, branchId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateStockMovement(Guid id, [FromBody] UpdateStockMovementDto updateStockMovementDto)
    {
        var result = await _stockMovementService.UpdateAsync(id, updateStockMovementDto);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
     }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteStockMovement(Guid id)
    {
        var result = await _stockMovementService.DeleteAsync(id);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }
}