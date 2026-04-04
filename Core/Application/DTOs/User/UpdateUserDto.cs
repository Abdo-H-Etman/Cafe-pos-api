namespace Application.DTOs.User;

public record UpdateUserDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
}