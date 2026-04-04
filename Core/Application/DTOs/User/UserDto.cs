namespace Application.DTOs.User;

public record UserDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Email { get; init; }
    public string UserName { get; init; } = null!;
    public Guid BranchId { get; init; }
    public DateTime DateJoined { get; init; }
}
