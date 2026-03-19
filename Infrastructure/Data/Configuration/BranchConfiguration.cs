using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        builder.Property(b => b.Name)
            .HasMaxLength(100);
        builder.Property(b => b.Address)
            .HasMaxLength(500);

        builder.HasIndex(b => b.Name)
            .IsUnique();
    }
}