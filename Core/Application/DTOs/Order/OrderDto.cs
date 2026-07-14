namespace Core.Application.DTOs.Order;

public record OrderDto
{
    public Guid Id { get; init; }
    public string Cashier { get; init; } = null!;
    public string Table { get; init; } = null!;
    public decimal Total { get; init; }
    public DateTime Date { get; init; }
    public string Status { get; init; } = null!;
    public string PaymentMethod { get; init; } = null!;
    public decimal Tax { get; init; }
    public string? DiscountType { get; init; }
    public decimal? DiscountValue { get; init; }
    public List<OrderItemDto> Items { get; init; } = [];
}