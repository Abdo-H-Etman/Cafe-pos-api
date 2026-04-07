using System.Linq.Expressions;
using Application.Common.Models;
using Application.DTOs.User;
using Application.Interfaces;
using Application.Interfaces.Logging;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;

    public UserService(IRepositoryManager repository, ILoggerManager logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<UserDetailsDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _repository.User.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result<UserDetailsDto>.Failure("User not found.");
            }

            var userDetails = MapToUserDetailsDto(user);

            return Result<UserDetailsDto>.Success(userDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching user by ID: {ex.Message}");
            return Result<UserDetailsDto>.Failure($"An error occurred while fetching the user. {ex.Message}");
        }
    }

    public async Task<Result<UserDetailsDto>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _repository.User.GetUserByUsernameAsync(username, cancellationToken);
            if (user == null)
            {
                return Result<UserDetailsDto>.Failure("User not found.");
            }

            var userDetails = MapToUserDetailsDto(user);

            return Result<UserDetailsDto>.Success(userDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching user by username: {ex.Message}");
            return Result<UserDetailsDto>.Failure("An error occurred while fetching the user.");
        }
    }

    public async Task<Result<UserDetailsDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _repository.User.GetUserByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                return Result<UserDetailsDto>.Failure("User not found.");
            }

            var userDetails = MapToUserDetailsDto(user);

            return Result<UserDetailsDto>.Success(userDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching user by email: {ex.Message}");
            return Result<UserDetailsDto>.Failure("An error occurred while fetching the user.");
        }
    }

    public async Task<Result<IEnumerable<UserDetailsDto>>> GetPagedUsersAsync(int pageNumber,
                int pageSize, string? searchTerm = null,
                Guid? branchId = null,
                string? roleName = null,
                CancellationToken cancellationToken = default)
    {
        try
        {
            var predicate = string.IsNullOrEmpty(searchTerm)
                ? null
                : (Expression<Func<User, bool>>)(u => u.Name.ToLower().Contains(searchTerm.ToLower()) ||
                        u.Email!.ToLower().Contains(searchTerm.ToLower()) ||
                        u.UserName!.ToLower().Contains(searchTerm.ToLower()));

            var users = await _repository.User.GetPagedUsersAsync(pageNumber,
                        pageSize, predicate, branchId, roleName, cancellationToken);
            var userDetailsList = users.Select(MapToUserDetailsDto);

            _logger.LogInfo($"Fetched page {pageNumber} of users with page size {pageSize} and search term '{searchTerm}'.");
            return Result<IEnumerable<UserDetailsDto>>.Success(userDetailsList);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching paged users: {ex.Message}");
            return Result<IEnumerable<UserDetailsDto>>.Failure($"An error occurred while fetching the users. {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserDetailsDto>>> GetUsersByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _repository.User.GetUsersByBranchIdAsync(branchId, cancellationToken);
            var userDetailsList = users.Select(user => new UserDetailsDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                UserName = user.UserName!,
                Branch = user.Branch.Name,
                Roles = user.Roles.Select(r => r.Role.Name)!
            });

            return Result<IEnumerable<UserDetailsDto>>.Success(userDetailsList);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching users by branch ID: {ex.Message}");
            return Result<IEnumerable<UserDetailsDto>>.Failure("An error occurred while fetching the users.");
        }
    }

    public async Task<Result<IEnumerable<UserDetailsDto>>> GetUsersByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _repository.User.GetUsersByRoleIdAsync(roleId, cancellationToken);
            var userDetailsList = users.Select(user => new UserDetailsDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                UserName = user.UserName!,
                Branch = user.Branch.Name,
                Roles = user.Roles.Select(r => r.Role.Name)!
            });

            return Result<IEnumerable<UserDetailsDto>>.Success(userDetailsList);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching users by role ID: {ex.Message}");
            return Result<IEnumerable<UserDetailsDto>>.Failure("An error occurred while fetching the users.");
        }
    }

    public async Task<Result<UserDetailsDto>> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _repository.User.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarn("User with ID {userId} not found or is deleted.", userId);
                return Result<UserDetailsDto>.Failure("User not found.");
            }

            if (!string.IsNullOrEmpty(updateUserDto.Name))
                user.Name = updateUserDto.Name;

            if (!string.IsNullOrEmpty(updateUserDto.Email))
                user.Email = updateUserDto.Email;

            if (!string.IsNullOrEmpty(updateUserDto.UserName))
                user.UserName = updateUserDto.UserName;

            await _repository.User.Update(user, cancellationToken);
            await _repository.SaveAsync(cancellationToken);
            
            var userDto = MapToUserDetailsDto(user);

            _logger.LogInfo("User with ID {UserId} updated successfully.", userId);
            return Result<UserDetailsDto>.Success(userDto, "User Updated Suuccefully");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in {UpdateUserAsync}: {message}", nameof(UpdateUserAsync), ex.Message);
            return Result<UserDetailsDto>.Failure($"An error occurred while updating the user. {ex.Message}");
        }
    }

    public async Task<Result> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _repository.User.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found.");
            }

            await _repository.User.Delete(user, cancellationToken);
            return Result.Success("User deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting user: {ex.Message}");
            return Result.Failure("An error occurred while deleting the user.");
        }
    }

    private UserDetailsDto MapToUserDetailsDto(User user)
    {
        return new UserDetailsDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            UserName = user.UserName!,
            BranchId = user.BranchId,
            Branch = user.Branch.Name,
            Roles = user.Roles.Select(r => r.Role.Name)!
        };
    }
    
}