namespace Core.Application.DTOs.Order;

public record UpdateOrderDto
{
    public decimal? Tax { get; init; }
    public string? DiscountType { get; init; }
    public decimal? DiscountValue { get; init; }
    public string? PaymentMethod { get; init; }
    public List<UpdateOrderItemDto> Items { get; init; } = [];
}
