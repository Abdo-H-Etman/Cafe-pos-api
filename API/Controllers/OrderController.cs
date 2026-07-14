using Core.Application.DTOs.Order;
using Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Roles = "Cashier, Manager")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("paged")]
    [Authorize(Roles = "Manager, Admin")]
    public async Task<IActionResult> GetPaged([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate,
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
            [FromQuery] Guid? cashierId = null, [FromQuery] string? status = null,
            [FromQuery] Guid? adminSelectedBranchId = null, CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetPagedOrdersAsync(pageNumber, pageSize, fromDate, toDate,
                        cashierId, status, adminSelectedBranchId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto createOrderDto, CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateAsync(createOrderDto, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Consumes("application/json", "text/plain")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status, CancellationToken cancellationToken)
    {
        var result = await _orderService.UpdateStatusAsync(id, status, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderDto updateOrderDto, CancellationToken cancellationToken)
    {
        var result = await _orderService.UpdateAsync(id, updateOrderDto, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
