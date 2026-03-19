using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class BranchProductConfiguration : IEntityTypeConfiguration<BranchProduct>
{
    public void Configure(EntityTypeBuilder<BranchProduct> builder)
    {
        builder.ToTable("BranchProducts");

        builder.HasKey(bp => new{bp.BranchId, bp.ProductId});

        builder.Property(bp => bp.Price)
            .HasPrecision(10, 2);
        builder.Property(bp => bp.IsActive)
            .HasDefaultValue(true);
        
        builder.HasOne(bp => bp.Branch)
            .WithMany(b => b.BranchProducts)
            .HasForeignKey(bp => bp.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(bp => bp.Product)
            .WithMany()
            .HasForeignKey(bp => bp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}