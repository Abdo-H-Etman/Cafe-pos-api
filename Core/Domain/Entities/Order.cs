using Core.Domain.Entities.Generics;
using Core.Domain.Models.Enums;

namespace Core.Domain.Entities;

public class Order : IdModel
{
    public Guid CashierId { get; set; }
    public Guid BranchId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public DiscountType? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    
}