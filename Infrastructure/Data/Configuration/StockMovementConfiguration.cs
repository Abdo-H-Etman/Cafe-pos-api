using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.Property(sm => sm.Quantity)
            .HasPrecision(10, 2);
        builder.Property(sm => sm.Type)
            .HasConversion<string>();
        
        builder.HasIndex(sm => new{sm.BranchId, sm.IngredientId});
        builder.HasIndex(sm => sm.CreatedAt);

        builder.HasOne(sm => sm.Branch)
            .WithMany(b => b.StockMovements)
            .HasForeignKey(sm => sm.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(sm => sm.Ingredient)
            .WithMany(i => i.StockMovements)
            .HasForeignKey(sm => sm.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}