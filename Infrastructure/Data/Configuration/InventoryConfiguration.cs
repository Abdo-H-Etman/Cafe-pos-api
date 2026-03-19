using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventories");

        builder.Property(i => i.CurrentStock)
            .HasPrecision(10, 2);
        
        builder.HasOne(i => i.Branch)
            .WithMany()
            .HasForeignKey(i => i.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.Ingredient)
            .WithMany()
            .HasForeignKey(i => i.IngredientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}