using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.ToTable("Tables");

        builder.Property(t => t.Name)
            .HasMaxLength(100);

        builder.HasIndex(t => new { t.BranchId, t.Name })
            .IsUnique();

        builder.HasOne(t => t.Branch)
            .WithMany(b => b.Tables)
            .HasForeignKey(t => t.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
