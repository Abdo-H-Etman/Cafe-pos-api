namespace Core.Application.DTOs.Reservation;

public record CreateReservationDto
{
    public string CustomerName { get; init; } = null!;
    public int GuestCount { get; init; }
    public string ContactPhone { get; init; } = null!;
    public DateTime ReservationDate { get; init; }
    public string? Notes { get; init; }
    public Guid? TableId { get; init; }
}
