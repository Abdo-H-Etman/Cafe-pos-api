using Application.Common.Models;
using Application.DTOs.Auth;
using Application.DTOs.User;

namespace Application.Interfaces.Auth;

public interface IAuthenticationService
{
    Task<Result<UserDetailsDto>> RegisterUserAsync(
        CreateUserDto createUserDto,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> LoginAsync(
        LoginDto loginDto,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(
        TokenDto tokenDto,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
    
    Task<Result> ChangePasswordAsync(
        Guid userId,
        ChangePasswordDto changePasswordDto,
        CancellationToken cancellationToken = default);

    Task<Result> RevokeTokenAsync(
        string refreshToken,                
        string? ipAddress = null,
        CancellationToken cancellationToken = default);    
    Task<Result> LogoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
        
}
