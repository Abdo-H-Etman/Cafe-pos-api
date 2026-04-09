using System.Linq.Expressions;
using Core.Domain.Entities;

namespace Core.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedUsersAsync(int pageNumber,
                int pageSize,
                Expression<Func<User, bool>>? predicate = null,
                Guid? branchId = null,
                string? roleName = null,
                CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetUsersByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetUsersByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task Update(User user, CancellationToken cancellationToken = default);
    Task Delete(User user, CancellationToken cancellationToken = default);
}