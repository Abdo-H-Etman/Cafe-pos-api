using Core.Application.DTOs.Reservation;
using Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize(Roles = "Manager")]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
    [FromQuery] string? status = null, [FromQuery] Guid? tableId = null, CancellationToken cancellationToken = default)
    {
        var result = await _reservationService.GetAllAsync(pageNumber, pageSize, status, tableId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reservationService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto createReservationDto, CancellationToken cancellationToken)
    {
        var result = await _reservationService.CreateAsync(createReservationDto, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReservationDto updateReservationDto, CancellationToken cancellationToken)
    {
        var reservationToUpdate = await _reservationService.GetByIdAsync(id, cancellationToken);
        if (!reservationToUpdate.IsSuccess)
            return NotFound(reservationToUpdate);

        var result = await _reservationService.UpdateAsync(id, updateReservationDto, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reservationService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result);
    }
}
