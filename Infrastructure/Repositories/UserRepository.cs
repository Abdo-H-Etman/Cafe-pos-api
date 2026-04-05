using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;

    public UserRepository(UserManager<User> userMangager, AppDbContext context)
    {
       _userManager = userMangager;
       _context = context;
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users
                .AsNoTracking()
                .Include(u => u.Branch)
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Users
                .AsNoTracking()
                .Include(u => u.Branch)
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public async Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        await _context.Users
                .AsNoTracking()
                .Include(u => u.Branch)
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == username, cancellationToken);

    public async Task<IReadOnlyList<User>> GetUsersByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default) =>
        await _context.Users
                .AsNoTrackingWithIdentityResolution()
                .Include(u => u.Branch)
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.BranchId == branchId)
                .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> GetUsersByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default) =>
        await _context.Users
                .AsNoTrackingWithIdentityResolution()
                .Include(u => u.Branch)
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.Roles.Any(ur => ur.RoleId == roleId))
                .ToListAsync(cancellationToken);

    public async Task Update(User user, CancellationToken cancellationToken = default)
    {
        _context.ChangeTracker.Clear();
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task Delete(User user, CancellationToken cancellationToken = default) =>
        await _userManager.DeleteAsync(user);
}