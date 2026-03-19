using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("Recipes");

        builder.Property(r => r.Quantity)
            .HasPrecision(18, 4);

        builder.HasIndex(r => new{ r.ProductId, r.IngredientId })
            .IsUnique();
        
        builder.HasOne(r => r.Product)
            .WithMany(p => p.Recipes)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.Ingredient)
            .WithMany()
            .HasForeignKey(r => r.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}