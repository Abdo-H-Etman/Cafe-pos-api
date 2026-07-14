namespace Core.Application.DTOs.Order;

public record CreateOrderDto
{
    public Guid? TableId { get; init; }
    public decimal Tax { get; init; }
    public string? DiscountType { get; init; }
    public decimal? DiscountValue { get; init; }
    public List<CreateOrderItemDto> Items { get; init; } = [];
}

public record CreateOrderItemDto
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
