using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.Property(c => c.Name)
            .HasMaxLength(100);
        builder.Property(c => c.Description)
            .HasMaxLength(500);
        
        builder.HasIndex(c => c.Name)
            .IsUnique();
        
        builder.HasMany(c => c.Products)
            .WithOne(p => p.Category);
    }
}