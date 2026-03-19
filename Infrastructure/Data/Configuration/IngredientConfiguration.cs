using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.ToTable("Ingredients");

        builder.Property(i => i.Name)
            .HasMaxLength(100);
        builder.Property(i => i.Unit)
            .HasConversion<string>();
        builder.Property(i => i.MinStock)
            .HasPrecision(10, 2);
        
        builder.HasIndex(i => i.Name)
            .IsUnique();
    }
}