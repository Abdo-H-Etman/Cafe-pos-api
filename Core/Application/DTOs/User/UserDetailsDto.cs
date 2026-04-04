namespace Application.DTOs.User;

public record UserDetailsDto : UserDto
{
    public string Branch { get; init; } = null!;
    public IEnumerable<string> Roles { get; init; } = [];
}