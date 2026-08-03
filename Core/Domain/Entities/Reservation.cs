using Core.Domain.Entities.Generics;
using Core.Domain.Models.Enums;

namespace Core.Domain.Entities;

public class Reservation : IdModel
{
    public Guid BranchId { get; set; }
    public string CustomerName { get; set; } = null!;
    public int GuestCount { get; set; }
    public string ContactPhone { get; set; } = null!;
    public DateTime ReservationDate { get; set; }
    public string? Notes { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? TableId { get; set; }

    public Branch Branch { get; set; } = null!;
    public Table? Table { get; set; }
}
