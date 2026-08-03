using Application.Common;
using Application.Common.Models;
using Application.Interfaces.Logging;
using Core.Application.DTOs.Reservation;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Core.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class ReservationService : IReservationService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerManager _logger;

    public ReservationService(IRepositoryManager repositoryManager, ICurrentUserService currentUserService, ILoggerManager logger)
    {
        _repositoryManager = repositoryManager;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ReservationDto>, MetaData>> GetAllAsync(int pageNumber, int pageSize,
    string? status = null, Guid? tableId = null, CancellationToken cancellationToken = default)
    {
        var (reservations, totalCount) = await _repositoryManager.Reservation.GetPagedAsync(pageNumber, pageSize,
            predicate: r => (!tableId.HasValue || r.TableId == tableId) &&
                            (string.IsNullOrEmpty(status) || r.Status.ToString() == status),
            include: r => r.Include(r => r.Table),
            cancellationToken: cancellationToken);

        var reservationsDto = reservations.Select(MapToReservationDto);
        var metaData = new MetaData(pageNumber, pageSize, totalCount);
        _logger.LogInfo($"Retrieved {reservations.Count()} reservations for branch {_currentUserService.BranchId}.");
        return Result<IEnumerable<ReservationDto>, MetaData>.Success(reservationsDto, metaData, "Reservations retrieved successfully.");
    }

    public async Task<Result<ReservationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reservation = await _repositoryManager.Reservation.GetByIdAsync(id,
            include: r => r.Include(r => r.Table),
            cancellationToken: cancellationToken);
        if (reservation == null)
        {
            _logger.LogError($"Reservation with ID {id} not found for branch {_currentUserService.BranchId}.");
            return Result<ReservationDto>.Failure("Reservation not found.");
        }

        _logger.LogInfo($"Retrieved reservation with ID {id} for branch {_currentUserService.BranchId}.");
        return Result<ReservationDto>.Success(MapToReservationDto(reservation), "Reservation retrieved successfully.");
    }

    public async Task<Result<ReservationDto>> CreateAsync(CreateReservationDto createReservationDto, CancellationToken cancellationToken = default)
    {
        if (createReservationDto.GuestCount <= 0)
        {
            return Result<ReservationDto>.Failure("Guest count must be greater than zero.");
        }

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            BranchId = _currentUserService.BranchId,
            CustomerName = createReservationDto.CustomerName,
            GuestCount = createReservationDto.GuestCount,
            ContactPhone = createReservationDto.ContactPhone,
            ReservationDate = createReservationDto.ReservationDate,
            Notes = createReservationDto.Notes,
            TableId = createReservationDto.TableId,
            Status = ReservationStatus.Pending
        };

        if (createReservationDto.TableId != null)
        {
            var table = await _repositoryManager.Table.GetByIdAsync((Guid)createReservationDto.TableId, cancellationToken: cancellationToken);
            if (table == null)
            {
                await _repositoryManager.Reservation.AddAsync(reservation, cancellationToken);
                await _repositoryManager.SaveAsync(cancellationToken);

                _logger.LogWarn("Table with ID {TableId} not found. Reservation will be created without a table assignment.", createReservationDto.TableId);
                return Result<ReservationDto>.Success(MapToReservationDto(reservation), $"Table with ID {createReservationDto.TableId} not found. Reservation will be created without a table assignment.");
            }
            table.Status = TableStatus.Reserved;
        }
        await _repositoryManager.Reservation.AddAsync(reservation, cancellationToken);
        await _repositoryManager.SaveAsync(cancellationToken);

        var reservationFromDb = await _repositoryManager.Reservation.GetByIdAsync(reservation.Id,
            include: r => r.Include(r => r.Table),
            cancellationToken: cancellationToken);
        var reservationDto = MapToReservationDto(reservationFromDb!);
        return Result<ReservationDto>.Success(reservationDto, "Reservation created successfully.");
    }

    public async Task<Result<ReservationDto>> UpdateAsync(Guid id, UpdateReservationDto updateReservationDto, CancellationToken cancellationToken = default)
    {
        var reservation = await _repositoryManager.Reservation.GetByIdAsync(id,
            include: r => r.Include(r => r.Table),
            cancellationToken: cancellationToken);
        if (reservation == null)
        {
            _logger.LogError($"Reservation with ID {id} not found for branch {_currentUserService.BranchId}.");
            return Result<ReservationDto>.Failure("Reservation not found.");
        }

        if (reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Completed)
        {
            _logger.LogError($"Cannot update reservation with ID {id} as it is already {reservation.Status}.");
            return Result<ReservationDto>.Failure($"Cannot update reservation as it is already {reservation.Status}.");
        }
        if (updateReservationDto.GuestCount is <= 0)
        {
            _logger.LogError($"Invalid guest count {updateReservationDto.GuestCount} for reservation with ID {id}.");
            return Result<ReservationDto>.Failure("Guest count must be greater than zero.");
        }

        reservation.CustomerName = updateReservationDto.CustomerName ?? reservation.CustomerName;
        reservation.GuestCount = updateReservationDto.GuestCount ?? reservation.GuestCount;
        reservation.ContactPhone = updateReservationDto.ContactPhone ?? reservation.ContactPhone;
        reservation.ReservationDate = updateReservationDto.ReservationDate ?? reservation.ReservationDate;
        reservation.Notes = updateReservationDto.Notes ?? reservation.Notes;


        if (!string.IsNullOrWhiteSpace(updateReservationDto.Status))
        {
            reservation.Status = Enum.Parse<ReservationStatus>(updateReservationDto.Status, ignoreCase: true);
        }
        if (reservation.TableId != updateReservationDto.TableId && updateReservationDto.TableId != null)
        {
            var newTable = await _repositoryManager.Table.GetByIdAsync((Guid)updateReservationDto.TableId, cancellationToken: cancellationToken);
            if (newTable == null)
            {
                _logger.LogWarn("Table with ID {TableId} not found. Reservation will be updated without a table assignment.", updateReservationDto.TableId);
                return Result<ReservationDto>.Success(MapToReservationDto(reservation), $"Table with ID {updateReservationDto.TableId} not found.");
            }
            newTable.Status = TableStatus.Reserved;

            if (reservation.TableId != null)
            {
                var oldTable = await _repositoryManager.Table.GetByIdAsync((Guid)reservation.TableId, cancellationToken: cancellationToken);
                if (oldTable != null)
                {
                    oldTable.Status = TableStatus.Available;
                }
            }
        }
        reservation.TableId = updateReservationDto.TableId ?? reservation.TableId;

        if ((reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Completed)
                && reservation.TableId != null)
        {
            var table = await _repositoryManager.Table.GetByIdAsync((Guid)reservation.TableId, cancellationToken: cancellationToken);
            if (table != null)
            {
                table.Status = TableStatus.Available;
            }
        }
        await _repositoryManager.SaveAsync(cancellationToken);

        _logger.LogInfo($"Reservation with ID {id} updated for branch {_currentUserService.BranchId}.");
        return Result<ReservationDto>.Success(MapToReservationDto(reservation), "Reservation updated successfully.");
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reservation = await _repositoryManager.Reservation.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (reservation == null)
        {
            _logger.LogError($"Reservation with ID {id} not found for branch {_currentUserService.BranchId}.");
            return Result.Failure("Reservation not found.");
        }

        _repositoryManager.Reservation.Remove(reservation);
        await _repositoryManager.SaveAsync(cancellationToken);

        _logger.LogInfo($"Reservation with ID {id} deleted for branch {_currentUserService.BranchId}.");
        return Result.Success("Reservation deleted successfully.");
    }

    private static ReservationDto MapToReservationDto(Reservation reservation) =>
        new()
        {
            Id = reservation.Id,
            CustomerName = reservation.CustomerName,
            GuestCount = reservation.GuestCount,
            ContactPhone = reservation.ContactPhone,
            ReservationDate = reservation.ReservationDate,
            Status = reservation.Status.ToString(),
            Notes = reservation.Notes,
            TableName = reservation.Table != null ? reservation.Table.Name : null
        };
}
