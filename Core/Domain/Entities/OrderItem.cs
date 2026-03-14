using Core.Domain.Entities.Generics;
using Core.Domain.Models.Enums;

namespace Core.Domain.Entities;

public class OrderItem : IdModel
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DiscountType? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal TotalPrice { get; set; }

    public Order Order { get; set; } = null!;
}