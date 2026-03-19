using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class OrderItemCOnfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.Property(oi => oi.Quantity)
            .HasDefaultValue(1);
        builder.Property(oi => oi.UnitPrice)
            .HasPrecision(10, 2);
        builder.Property(oi => oi.TotalPrice)
            .HasPrecision(10, 2);
        builder.Property(oi => oi.DiscountValue)
            .HasPrecision(10, 2);
        builder.Property(oi => oi.DiscountType)
            .HasConversion<string>();
        
        builder.HasIndex(oi => new{oi.ProductId, oi.OrderId})
            .IsUnique();

        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}