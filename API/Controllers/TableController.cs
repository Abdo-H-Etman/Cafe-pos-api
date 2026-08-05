using Core.Application.DTOs.Table;
using Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/tables")]
[Authorize(Roles = "Manager")]
public class TableController : ControllerBase
{
    private readonly ITableService _tableService;

    public TableController(ITableService tableService)
    {
        _tableService = tableService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTable([FromBody] CreateTableDto createTableDto, CancellationToken cancellationToken)
    {
        var result = await _tableService.CreateAsync(createTableDto, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTableById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _tableService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTables(CancellationToken cancellationToken)
    {
        var result = await _tableService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateTable(Guid id, [FromBody] UpdateTableDto updateTableDto, CancellationToken cancellationToken)
    {
        var result = await _tableService.UpdateAsync(id, updateTableDto, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTable(Guid id, CancellationToken cancellationToken)
    {
        var result = await _tableService.DeleteAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }
}