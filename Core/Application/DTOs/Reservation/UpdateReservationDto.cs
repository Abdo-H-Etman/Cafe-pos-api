namespace Core.Application.DTOs.Reservation;

public record UpdateReservationDto
{
    public string? CustomerName { get; init; }
    public int? GuestCount { get; init; }
    public string? ContactPhone { get; init; }
    public DateTime? ReservationDate { get; init; }
    public string? Notes { get; init; }
    public Guid? TableId { get; init; }
    public string? Status { get; init; }
}
