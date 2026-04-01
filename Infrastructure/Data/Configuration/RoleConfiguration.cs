using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        builder.ToTable("Roles");

        builder.HasData(
            new IdentityRole<Guid> { Id = new("00000000-0000-0000-0000-000000000001"), Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole<Guid> { Id = new("00000000-0000-0000-0000-000000000002"), Name = "Manager", NormalizedName = "MANAGER" },
            new IdentityRole<Guid> { Id = new("00000000-0000-0000-0000-000000000003"), Name = "Staff", NormalizedName = "STAFF" },
            new IdentityRole<Guid> { Id = new("00000000-0000-0000-0000-000000000004"), Name = "Cashier", NormalizedName = "CASHIER" }
        );
    }
}