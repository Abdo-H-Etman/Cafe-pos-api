namespace Application.DTOs.Auth;

public record LoginDto
{
    public string Identifier { get; init; } = null!;
    public string Password { get; init; } = null!;
}
