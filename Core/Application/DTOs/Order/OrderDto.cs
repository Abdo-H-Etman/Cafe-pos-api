namespace Core.Application.DTOs.Order;

public record OrderDto
{
    public Guid Id { get; init; }
    public string Cashier { get; init; } = null!;
    public decimal TotalPrice { get; init; }
    public DateTime Date { get; init; }
    public string Status { get; init; } = null!;
}