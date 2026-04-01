namespace Application.DTOs.User;

public record CreateUserDto
{
    public string Name { get; init; } = null!;
    public string? Email { get; init; }
    public string UserName { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string ConfirmPassword { get; init; } = null!;
}
