using Application.DTOs.User;

namespace Application.DTOs.Auth;

public record AuthResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public UserDetailsDto User { get; init; } = null!;
}
