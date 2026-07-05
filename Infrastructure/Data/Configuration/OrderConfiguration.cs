using Core.Domain.Entities;
using Core.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.Property(o => o.SubTotal)
            .HasPrecision(10, 2);
        builder.Property(o => o.Total)
            .HasPrecision(10, 2);
        builder.Property(o => o.DiscountValue)
            .HasPrecision(10, 2);
        builder.Property(o => o.Tax)
            .HasPrecision(4, 2);
        builder.Property(o => o.DiscountType)
            .HasConversion<string>();
        builder.Property(o => o.Status)
            .HasConversion<string>();

        builder.HasIndex(o => new { o.BranchId, o.CreatedAt });
        builder.HasIndex(o => o.CashierId);

        builder.HasOne(o => o.Branch)
            .WithMany(b => b.Orders)
            .HasForeignKey(o => o.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Cashier)
            .WithMany()
            .HasForeignKey(o => o.CashierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Table)
            .WithMany(t => t.Orders)
            .HasForeignKey(o => o.TableId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}