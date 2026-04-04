using Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasData(
            new Role { Id = new("00000000-0000-0000-0000-000000000001"), Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "7c568c26-a532-4475-8e11-4bc7cfc3e3da" },
            new Role { Id = new("00000000-0000-0000-0000-000000000002"), Name = "Manager", NormalizedName = "MANAGER", ConcurrencyStamp = "812749b3-b93d-43aa-bf32-046412378de0" },
            new Role { Id = new("00000000-0000-0000-0000-000000000003"), Name = "Staff", NormalizedName = "STAFF", ConcurrencyStamp = "b01f0737-7958-43fc-a929-e3df60e8d371" },
            new Role { Id = new("00000000-0000-0000-0000-000000000004"), Name = "Cashier", NormalizedName = "CASHIER", ConcurrencyStamp = "80c9a557-1548-4fd7-b045-4fdb65a6d881" }
        );
    }
}