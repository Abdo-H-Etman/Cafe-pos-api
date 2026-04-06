namespace Core.Application.DTOs.Branch;

public record UpdateBranchDto
{
    public string? Name { get; init; }
    public string? Address { get; init; }
}