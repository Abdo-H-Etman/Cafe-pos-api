using Application.Common.Models;
using Application.DTOs.User;

namespace Application.Interfaces;

public interface IUserService
{
    Task<Result<UserDetailsDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<UserDetailsDto>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Result<UserDetailsDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserDetailsDto>>> GetUsersByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserDetailsDto>>> GetUsersByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<Result<UserDetailsDto>> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default);
    Task<Result> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
}