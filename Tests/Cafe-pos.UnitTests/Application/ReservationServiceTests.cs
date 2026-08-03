using System.Linq.Expressions;
using Application.Common;
using Application.Interfaces.Logging;
using Application.Services;
using Core.Application.DTOs.Reservation;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Core.Domain.Models.Enums;
using FluentAssertions;
using Moq;

namespace Cafe_pos.UnitTests.Application;

public class ReservationServiceTests
{
    private readonly Mock<IRepositoryManager> _repositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ILoggerManager> _loggerMock = new();
    private readonly IReservationService _service;

    public ReservationServiceTests()
    {
        _currentUserServiceMock.Setup(m => m.BranchId).Returns(Guid.NewGuid());
        _repositoryMock.Setup(m => m.SaveAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _service = new ReservationService(_repositoryMock.Object, _currentUserServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenGuestCountIsInvalid_ReturnsFailure()
    {
        var dto = new CreateReservationDto
        {
            CustomerName = "Alice",
            GuestCount = 0,
            ReservationDate = DateTime.UtcNow.AddDays(1)
        };

        var result = await _service.CreateAsync(dto, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Guest count must be greater than zero.");
    }

    [Fact]
    public async Task CreateAsync_WhenValid_ReturnsSuccessAndSavesReservation()
    {
        var dto = new CreateReservationDto
        {
            CustomerName = "Alice",
            GuestCount = 4,
            ReservationDate = DateTime.UtcNow.AddDays(1),
            Notes = "Window seat"
        };

        var reservationRepository = new Mock<IRepository<Reservation>>();
        _repositoryMock.SetupGet(m => m.Reservation).Returns(reservationRepository.Object);
        reservationRepository
            .Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation reservation, CancellationToken _) => reservation);
        reservationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Reservation>, IQueryable<Reservation>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Reservation>, IQueryable<Reservation>>? include, CancellationToken _) => new Reservation { Id = id, CustomerName = dto.CustomerName, GuestCount = dto.GuestCount, Status = ReservationStatus.Pending });

        var result = await _service.CreateAsync(dto, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CustomerName.Should().Be("Alice");
        reservationRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenTableIsMissing_ReturnsSuccessWithoutTableAssignment()
    {
        var dto = new CreateReservationDto
        {
            CustomerName = "Alice",
            GuestCount = 2,
            ReservationDate = DateTime.UtcNow.AddDays(1),
            TableId = Guid.NewGuid()
        };

        var reservationRepository = new Mock<IRepository<Reservation>>();
        var tableRepository = new Mock<IRepository<Table>>();
        _repositoryMock.SetupGet(m => m.Reservation).Returns(reservationRepository.Object);
        _repositoryMock.SetupGet(m => m.Table).Returns(tableRepository.Object);
        reservationRepository
            .Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation reservation, CancellationToken _) => reservation);
        reservationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Reservation>, IQueryable<Reservation>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Reservation>, IQueryable<Reservation>>? include, CancellationToken _) => new Reservation { Id = id, CustomerName = dto.CustomerName, GuestCount = dto.GuestCount, TableId = null });
        tableRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Table>, IQueryable<Table>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Table>, IQueryable<Table>>? include, CancellationToken _) => null);

        var result = await _service.CreateAsync(dto, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.TableName.Should().BeNull();
        reservationRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenTableExists_MarksTableReserved()
    {
        var dto = new CreateReservationDto
        {
            CustomerName = "Alice",
            GuestCount = 3,
            ReservationDate = DateTime.UtcNow.AddDays(1),
            TableId = Guid.NewGuid()
        };

        var reservationRepository = new Mock<IRepository<Reservation>>();
        var tableRepository = new Mock<IRepository<Table>>();
        var table = new Table { Id = dto.TableId!.Value, Status = TableStatus.Available };
        _repositoryMock.SetupGet(m => m.Reservation).Returns(reservationRepository.Object);
        _repositoryMock.SetupGet(m => m.Table).Returns(tableRepository.Object);
        reservationRepository
            .Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation reservation, CancellationToken _) => reservation);
        reservationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Reservation>, IQueryable<Reservation>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Reservation>, IQueryable<Reservation>>? include, CancellationToken _) => new Reservation { Id = id, CustomerName = dto.CustomerName, GuestCount = dto.GuestCount, TableId = dto.TableId });
        tableRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Table>, IQueryable<Table>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Table>, IQueryable<Table>>? include, CancellationToken _) => table);

        var result = await _service.CreateAsync(dto, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        table.Status.Should().Be(TableStatus.Reserved);
    }

    [Fact]
    public async Task GetByIdAsync_WhenReservationMissing_ReturnsFailure()
    {
        var reservationRepository = new Mock<IRepository<Reservation>>();
        _repositoryMock.SetupGet(m => m.Reservation).Returns(reservationRepository.Object);
        reservationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Reservation>, IQueryable<Reservation>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Reservation>, IQueryable<Reservation>>? include, CancellationToken _) => null);

        var result = await _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Reservation not found.");
    }

    [Fact]
    public async Task GetByIdAsync_WhenReservationExists_ReturnsMappedReservation()
    {
        var reservationId = Guid.NewGuid();
        var reservation = new Reservation
        {
            Id = reservationId,
            CustomerName = "Bob",
            GuestCount = 5,
            Status = ReservationStatus.Confirmed,
            Table = new Table { Name = "T1" }
        };

        var reservationRepository = new Mock<IRepository<Reservation>>();
        _repositoryMock.SetupGet(m => m.Reservation).Returns(reservationRepository.Object);
        reservationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Reservation>, IQueryable<Reservation>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Reservation>, IQueryable<Reservation>>? include, CancellationToken _) => reservation);

        var result = await _service.GetByIdAsync(reservationId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.CustomerName.Should().Be("Bob");
        result.Data.TableName.Should().Be("T1");
        result.Data.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task GetAllAsync_WhenFiltersProvided_ReturnsPagedReservationsAndMetadata()
    {
        var reservationRepository = new Mock<IRepository<Reservation>>();
        var reservations = new List<Reservation>
        {
            new() { Id = Guid.NewGuid(), CustomerName = "Carol", Status = ReservationStatus.Pending, Table = new Table { Name = "T2" } }
        };

        _repositoryMock.SetupGet(m => m.Reservation).Returns(reservationRepository.Object);
        reservationRepository
            .Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Reservation, bool>>>(), It.IsAny<Func<IQueryable<Reservation>, IQueryable<Reservation>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((reservations, 1));

        var result = await _service.GetAllAsync(1, 10, "Pending", Guid.NewGuid(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.MetaData.Should().NotBeNull();
        result.MetaData!.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task UpdateAsync_WhenReservationMissing_ReturnsFailure()
    {
        var reservationRepository = new Mock<IRepository<Reservation>>();
        _repositoryMock.SetupGet(m => m.Reservation).Returns(reservationRepository.Object);
        reservationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Reservation>, IQueryable<Reservation>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Reservation>, IQueryable<Reservation>>? include, CancellationToken _) => null);

        var result = await _service.UpdateAsync(Guid.NewGuid(), new UpdateReservationDto(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Reservation not found.");
    }

    [Fact]
    public async Task UpdateAsync_WhenReservationAlreadyCompleted_ReturnsFailure()
    {
        var reservationRepository = new Mock<IRepository<Reservation>>();
        var existing = new Reservation { Id = Guid.NewGuid(), Status = ReservationStatus.Completed };
        _repositoryMock.SetupGet(m => m.Reservation).Returns(reservationRepository.Object);
        reservationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Reservation>, IQueryable<Reservation>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Reservation>, IQueryable<Reservation>>? include, CancellationToken _) => existing);

        var result = await _service.UpdateAsync(existing.Id, new UpdateReservationDto(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Cannot update reservation as it is already Completed.");
    }

    [Fact]
    public async Task UpdateAsync_WhenGuestCountIsInvalid_ReturnsFailure()
    {
        var reservationRepository = new Mock<IRepository<Reservation>>();
        var existing = new Reservation { Id = Guid.NewGuid(), Status = ReservationStatus.Pending };
        _repositoryMock.SetupGet(m => m.Reservation).Returns(reservationRepository.Object);
        reservationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Reservation>, IQueryable<Reservation>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Reservation>, IQueryable<Reservation>>? include, CancellationToken _) => existing);

        var result = await _service.UpdateAsync(existing.Id, new UpdateReservationDto { GuestCount = 0 }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Guest count must be greater than zero.");
    }

    [Fact]
    public async Task UpdateAsync_WhenNewTableMissing_ReturnsSuccessWithoutChangingTable()
    {
        var reservationRepository = new Mock<IRepository<Reservation>>();
        var tableRepository = new Mock<IRepository<Table>>();
        var existing = new Reservation { Id = Guid.NewGuid(), Status = ReservationStatus.Pending, TableId = Guid.NewGuid() };
        _repositoryMock.SetupGet(m => m.Reservation).Returns(reservationRepository.Object);
        _repositoryMock.SetupGet(m => m.Table).Returns(tableRepository.Object);
        reservationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Reservation>, IQueryable<Reservation>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Reservation>, IQueryable<Reservation>>? include, CancellationToken _) => existing);
        tableRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Table>, IQueryable<Table>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Table>, IQueryable<Table>>? include, CancellationToken _) => null);

        var result = await _service.UpdateAsync(existing.Id, new UpdateReservationDto { TableId = Guid.NewGuid() }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.TableName.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenNewTableIsAssigned_ReassignsTables()
    {
        var reservationRepository = new Mock<IRepository<Reservation>>();
        var tableRepository = new Mock<IRepository<Table>>();
        var existingTable = new Table { Id = Guid.NewGuid(), Status = TableStatus.Available };
        var newTable = new Table { Id = Guid.NewGuid(), Status = TableStatus.Available };
        var existing = new Reservation { Id = Guid.NewGuid(), Status = ReservationStatus.Pending, TableId = existingTable.Id };
        _repositoryMock.SetupGet(m => m.Reservation).Returns(reservationRepository.Object);
        _repositoryMock.SetupGet(m => m.Table).Returns(tableRepository.Object);
        reservationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Reservation>, IQueryable<Reservation>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Reservation>, IQueryable<Reservation>>? include, CancellationToken _) => existing);
        tableRepository
            .SetupSequence(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Table>, IQueryable<Table>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newTable)
            .ReturnsAsync(existingTable);

        var result = await _service.UpdateAsync(existing.Id, new UpdateReservationDto { TableId = newTable.Id }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existingTable.Status.Should().Be(TableStatus.Available);
        newTable.Status.Should().Be(TableStatus.Reserved);
    }

    [Fact]
    public async Task UpdateAsync_WhenReservationStatusChangesToCancelledAndTableAssigned_MakesTableAvailable()
    {
        var reservationRepository = new Mock<IRepository<Reservation>>();
        var tableRepository = new Mock<IRepository<Table>>();
        var table = new Table { Id = Guid.NewGuid(), Status = TableStatus.Reserved };
        var existing = new Reservation { Id = Guid.NewGuid(), Status = ReservationStatus.Pending, TableId = table.Id };
        _repositoryMock.SetupGet(m => m.Reservation).Returns(reservationRepository.Object);
        _repositoryMock.SetupGet(m => m.Table).Returns(tableRepository.Object);
        reservationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Reservation>, IQueryable<Reservation>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Reservation>, IQueryable<Reservation>>? include, CancellationToken _) => existing);
        tableRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Table>, IQueryable<Table>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Table>, IQueryable<Table>>? include, CancellationToken _) => table);

        var result = await _service.UpdateAsync(existing.Id, new UpdateReservationDto { Status = "Cancelled" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        table.Status.Should().Be(TableStatus.Available);
    }

    [Fact]
    public async Task DeleteAsync_WhenReservationMissing_ReturnsFailure()
    {
        var reservationRepository = new Mock<IRepository<Reservation>>();
        _repositoryMock.SetupGet(m => m.Reservation).Returns(reservationRepository.Object);
        reservationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Reservation>, IQueryable<Reservation>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Reservation>, IQueryable<Reservation>>? include, CancellationToken _) => null);

        var result = await _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Reservation not found.");
    }

    [Fact]
    public async Task DeleteAsync_WhenReservationExists_RemovesReservation()
    {
        var reservationRepository = new Mock<IRepository<Reservation>>();
        var reservation = new Reservation { Id = Guid.NewGuid() };
        _repositoryMock.SetupGet(m => m.Reservation).Returns(reservationRepository.Object);
        reservationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Func<IQueryable<Reservation>, IQueryable<Reservation>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Func<IQueryable<Reservation>, IQueryable<Reservation>>? include, CancellationToken _) => reservation);

        var result = await _service.DeleteAsync(reservation.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        reservationRepository.Verify(r => r.Remove(reservation), Times.Once);
        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
