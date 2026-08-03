namespace Core.Application.DTOs.Reservation;

public record ReservationDto
{
    public Guid Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public int GuestCount { get; init; }
    public string ContactPhone { get; init; } = string.Empty;
    public DateTime ReservationDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public string? TableName { get; init; }
}
