using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");

        builder.Property(r => r.CustomerName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(r => r.ContactPhone)
            .HasMaxLength(50);

        builder.Property(r => r.Status)
            .HasConversion<string>();

        builder.HasOne(r => r.Branch)
            .WithMany(b => b.Reservations)
            .HasForeignKey(r => r.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Table)
            .WithMany(t => t.Reservations)
            .HasForeignKey(r => r.TableId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
