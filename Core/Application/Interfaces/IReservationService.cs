using Application.Common.Models;
using Core.Application.DTOs.Reservation;

namespace Core.Application.Interfaces;

public interface IReservationService
{
    Task<Result<IEnumerable<ReservationDto>, MetaData>> GetAllAsync(int pageNumber, int pageSize, string? status = null,
                Guid? tableId = null, CancellationToken cancellationToken = default);
    Task<Result<ReservationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ReservationDto>> CreateAsync(CreateReservationDto createReservationDto, CancellationToken cancellationToken = default);
    Task<Result<ReservationDto>> UpdateAsync(Guid id, UpdateReservationDto updateReservationDto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
