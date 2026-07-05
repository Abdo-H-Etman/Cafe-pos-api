using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Models;
using Application.Common;
using Application.DTOs.Auth;
using Application.DTOs.User;
using Application.Interfaces.Auth;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Application.Interfaces.Logging;

namespace Application.Services.Auth;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IRepositoryManager _repositoryManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILoggerManager _logger;

    public AuthenticationService(
        IRepositoryManager repositoryManager,
        IOptions<JwtSettings> jwtSettings,
        ILoggerManager logger,
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _repositoryManager = repositoryManager;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Result<UserDetailsDto>> RegisterUserAsync(
        CreateUserDto createUserDto,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check email uniqueness only if email is provided
            User? existingUser = null;
            if (!string.IsNullOrWhiteSpace(createUserDto.Email))
            {
                existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarn("Registration attempt failed: Email {email} is already registered", createUserDto.Email);
                    return Result<UserDetailsDto>.Failure("Email is already registered.");
                }
            }

            existingUser = await _userManager.FindByNameAsync(createUserDto.UserName);
            if (existingUser != null)
            {
                _logger.LogWarn("Registration attempt failed: Username {username} is already taken", createUserDto.UserName);
                return Result<UserDetailsDto>.Failure("Username is already taken.");
            }

            if (createUserDto.Password != createUserDto.ConfirmPassword)
            {
                _logger.LogError("Password and confirm password don't match for user {username}", createUserDto.UserName);
                return Result<UserDetailsDto>.Failure("Passwords do not match.");
            }

            var newUser = new User
            {
                UserName = createUserDto.UserName,
                Email = createUserDto.Email,
                Name = createUserDto.Name,
                DateJoined = DateTime.UtcNow,
                BranchId = createUserDto.BranchId
            };

            var result = await _userManager.CreateAsync(newUser, createUserDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                _logger.LogError("Failed to create user with email {email}: {errors}", createUserDto.Email!,
                                string.Join(';', errors));
                return Result<UserDetailsDto>.Failure($"User creation failed.", errors);
            }

            var createdUser = await _userManager.FindByNameAsync(createUserDto.UserName);
            if (createdUser == null)
            {
                _logger.LogError("User with username {username} was created but could not be retrieved from database.", createUserDto.UserName);
                return Result<UserDetailsDto>.Failure("User was created but could not be retrieved from database.");
            }

            if (createUserDto.Roles != null && createUserDto.Roles.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(createdUser, createUserDto.Roles);
                if (!roleResult.Succeeded)
                {
                    var errors = roleResult.Errors.Select(e => e.Description).ToList();
                    _logger.LogError("Failed to assign roles to user {username}: {errors}", createUserDto.UserName,
                                    string.Join(';', errors));
                    return Result<UserDetailsDto>.Failure("User was created but role assignment failed.", errors);
                }
            }

            // Fetch user with Branch and Roles properly loaded
            var userWithDetails = await _repositoryManager.User.GetUserByIdAsync(createdUser.Id, cancellationToken);
            if (userWithDetails == null)
            {
                _logger.LogError("User with username {username} was created but could not be retrieved with details.", createUserDto.UserName);
                return Result<UserDetailsDto>.Failure("User was created but could not retrieve user details.");
            }

            var userDetails = MapToUserDetailsDto(userWithDetails);

            _logger.LogInfo("User with Id {userId} registered successfully.", createdUser.Id);
            return Result<UserDetailsDto>.Success(userDetails, "User registered successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during user registration: {message}", ex.Message);
            return Result<UserDetailsDto>.Failure($"Registration failed. {ex.Message}");
        }
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(
        LoginDto loginDto,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _repositoryManager.User.GetUserByUsernameAsync(loginDto.Identifier, cancellationToken) ??
                       await _repositoryManager.User.GetUserByEmailAsync(loginDto.Identifier, cancellationToken);
            if (user == null)
            {
                _logger.LogWarn("Login attempt failed: User with identifier {identifier} not found", loginDto.Identifier);
                return Result<AuthResponseDto>.Failure("Invalid credentials.");
            }

            if (_signInManager.IsSignedIn(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) }))))
            {
                _logger.LogWarn("Login attempt failed: User with ID {userId} is already logged in", user.Id);
                return Result<AuthResponseDto>.Failure("User is already logged in.");
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);
            if (signInResult.IsLockedOut)
            {
                _logger.LogWarn("Login attempt failed: User account is locked out for user with Id {userId}", user.Id);
                return Result<AuthResponseDto>.Failure("User account is locked out. please try again later.");
            }
            if (!signInResult.Succeeded)
            {
                _logger.LogWarn("Login attempt failed: User with identifier {identifier} not found", loginDto.Identifier);
                return Result<AuthResponseDto>.Failure("Invalid credentials.");
            }

            var authResponse = await GenerateAuthResponseDtoAsync(user, ipAddress, userAgent, cancellationToken);
            user.LastLoginAt = DateTime.UtcNow;
            await _repositoryManager.User.Update(user, cancellationToken);
            await _repositoryManager.SaveAsync(cancellationToken);
            _logger.LogInfo("User with ID {userId} logged in successfully.", user.Id);
            return Result<AuthResponseDto>.Success(authResponse, "User logged in successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during user login: {message} ", ex.Message);
            return Result<AuthResponseDto>.Failure($"Login failed. {ex.Message}");
        }
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(
        TokenDto tokenDto,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var principal = GetPrincipalFromExpiredToken(tokenDto.AccessToken);
            if (principal == null)
            {
                _logger.LogWarn("Invalid access token during refresh attempt.");
                return Result<AuthResponseDto>.Failure("Invalid access token.");
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarn("Invalid token not claims for user ID: {userId}", userIdClaim?.Value!);
                return Result<AuthResponseDto>.Failure("Invalid token claims.");
            }

            var refreshToken = await _repositoryManager.RefreshToken.FirstOrDefaultAsync(
                rt => rt.Token == tokenDto.RefreshToken && rt.UserId == userId,
                cancellationToken);

            if (refreshToken == null)
            {
                _logger.LogWarn("Refresh token not found or expired for user ID: {userId}", userId);
                return Result<AuthResponseDto>.Failure("Invalid or expired refresh token.");
            }

            if (refreshToken.IsRevoked)
            {
                _logger.LogWarn("Reused/revoked refresh token detected for user {userId} — revoking all sessions.", userId);
                await RevokeAllUserTokensAsync(userId);
                return Result<AuthResponseDto>.Failure("Invalid or expired refresh token.");
            }

            if (refreshToken.ExpiresAt <= DateTime.UtcNow)
                return Result<AuthResponseDto>.Failure("Invalid or expired refresh token.");

            var rowsAffected = await _repositoryManager.RefreshToken
                .RevokeIfActiveAsync(tokenDto.RefreshToken, replacedByToken: null, cancellationToken);

            if (rowsAffected == 0)
            {
                var maybeStolen = await _repositoryManager.RefreshToken
                    .AnyAsync(rt => rt.Token == tokenDto.RefreshToken && rt.IsRevoked, cancellationToken);

                if (maybeStolen)
                {
                    _logger.LogWarn("Revoked/reused refresh token presented for user {userId} — revoking all sessions.", userId);
                    await _repositoryManager.RefreshToken.RevokeAllForUserAsync(userId, cancellationToken);
                }

                return Result<AuthResponseDto>.Failure("Invalid or expired refresh token.");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarn("User with ID {userId} not found or inactive during token refresh.", userId);
                return Result<AuthResponseDto>.Failure("User not found or inactive.");
            }

            var stampClaim = principal.FindFirst("SecurityStamp")?.Value;
            if (stampClaim != user.SecurityStamp)
            {
                _logger.LogWarn("Security stamp mismatch during token refresh for user ID: {userId}.", userId);
                return Result<AuthResponseDto>.Failure("Session is no longer valid. Please log in again.");
            }

            await _userManager.UpdateSecurityStampAsync(user);

            var authResponse = await GenerateAuthResponseDtoAsync(user, ipAddress, userAgent, cancellationToken);

            _logger.LogInfo("Token refreshed successfully for user: {userId}.", userId);
            return Result<AuthResponseDto>.Success(authResponse, "Token refreshed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during token refresh: {message}", ex.Message);
            return Result<AuthResponseDto>.Failure("Token refresh failed.");
        }
    }

    public async Task<Result> ChangePasswordAsync(
        Guid userId,
        ChangePasswordDto changePasswordDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = _userManager.FindByIdAsync(userId.ToString()).Result;
            if (user == null)
            {
                _logger.LogWarn("Change password attempt failed: User with ID {userId} not found.", userId);
                return Result.Failure("User not found.");
            }

            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
            {
                _logger.LogWarn("Change password attempt failed: New password and confirm new password do not match for user ID {userId}.", userId);
                return Result.Failure("New password and confirm new password do not match.");
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                _logger.LogWarn("Change password attempt failed for user ID {userId}: {errors}", userId, string.Join(';', errors));
                return Result.Failure("Password change failed.", errors);
            }

            _logger.LogInfo("Password changed successfully for user ID {userId}.", userId);
            return Result.Success("Password changed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during password change for user ID {userId}: {message}", userId, ex.Message);
            return Result.Failure("Password change failed.");
        }
    }
    public async Task<Result> RevokeTokenAsync(
        string refreshToken,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _repositoryManager.RefreshToken.FirstOrDefaultAsync(
                rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

            if (token == null)
            {
                return Result.Failure("Invalid or already revoked refresh token.");
            }

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;

            _repositoryManager.RefreshToken.Update(token);
            await _repositoryManager.SaveAsync(cancellationToken);

            _logger.LogInfo("Refresh token revoked successfully for user: {UserId}.", token.UserId);
            return Result.Success("Refresh token revoked successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during token revocation: {message}", ex.Message);
            return Result.Failure("Token revocation failed.");
        }
    }

    public async Task<Result> LogoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var activeTokens = await _repositoryManager.RefreshToken
                .FindAsync(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow, cancellationToken);

            if (!activeTokens.Any())
            {
                return Result.Failure("User is not currently logged in.");
            }

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                _repositoryManager.RefreshToken.Update(token);
            }

            await _repositoryManager.SaveAsync(cancellationToken);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                await _userManager.UpdateSecurityStampAsync(user);
            }

            _logger.LogInfo("User {userId} logged out successfully.", userId);
            return Result.Success("User logged out successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during user logout: {message}", ex.Message);
            return Result.Failure($"Logout failed. {ex.Message}");
        }
    }

    private async Task<AuthResponseDto> GenerateAuthResponseDtoAsync(
            User user,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken)
    {
        var accessToken = await GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress, userAgent, cancellationToken);


        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            User = MapToUserDetailsDto(user)
        };
    }

    private async Task<string> GenerateAccessToken(User user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""),
            new("BranchId", user.BranchId.ToString()),
            new("Name", user.Name),
            new("SecurityStamp", user.SecurityStamp ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(
        Guid userId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = GenerateRefreshTokenString(),
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays),
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            var previousTokens = await _repositoryManager.RefreshToken
                .FindAsync(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow, cancellationToken);
            foreach (var token in previousTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.ReplacedByToken = refreshToken.Token;
                _repositoryManager.RefreshToken.Update(token);
            }
            await _repositoryManager.RefreshToken.AddAsync(refreshToken, cancellationToken);
            await _repositoryManager.SaveAsync(cancellationToken);

            return refreshToken;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error generating refresh token: {message}", ex.Message);
            _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace!);
            throw;
        }
    }

    private string GenerateRefreshTokenString()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var tokens = await _repositoryManager.RefreshToken
            .FindAsync(rt => rt.UserId == userId && !rt.IsRevoked);

        foreach (var t in tokens) t.IsRevoked = true;
        await _repositoryManager.SaveAsync();
    }

    private UserDto MapToUserResponse(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            BranchId = user.BranchId,
            DateJoined = user.DateJoined
        };
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
            Branch = user.Branch?.Name!,
            Roles = user.Roles?.Select(r => r.Role?.Name)!
        };
    }
}