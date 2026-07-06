using Application.Common;
using Core.Domain.Entities;
using Core.Domain.Entities.Generics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    private readonly ICurrentUserService _currentUserService;
    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }
    public Guid currentBranchId => _currentUserService.BranchId;
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Ignore<IdModel>();

        base.OnModelCreating(builder);

        builder.Ignore<IdentityUserLogin<Guid>>();
        builder.Ignore<IdentityUserToken<Guid>>();
        builder.Ignore<IdentityUserClaim<Guid>>();
        builder.Ignore<IdentityRoleClaim<Guid>>();

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        builder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
        });

        builder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.Roles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Inventory>().HasQueryFilter(i => _currentUserService.IsAdmin() || i.BranchId == currentBranchId);
        builder.Entity<StockMovement>().HasQueryFilter(i => _currentUserService.IsAdmin() || i.BranchId == currentBranchId);
        builder.Entity<Table>().HasQueryFilter(t => _currentUserService.IsAdmin() || t.BranchId == currentBranchId);
        builder.Entity<User>().HasQueryFilter(u => !_currentUserService.IsAuthenticated || _currentUserService.IsAdmin() || (u.BranchId == currentBranchId && u.Roles.Any(ur => ur.Role.Name != "Admin")));
    }
}