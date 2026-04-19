using Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/inventories")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBranchInventory([FromQuery] Guid? branchId = null)
    {
        var result = await _inventoryService.GetBranchInventoryAsync(branchId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock([FromQuery] Guid? branchId = null)
    {
        var result = await _inventoryService.GetLowStockAsync(branchId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}
