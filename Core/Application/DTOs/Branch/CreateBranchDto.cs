namespace Core.Application.DTOs.Branch;

public record CreateBranchDto
{
    public string Name { get; init; } = null!;
    public string Address { get; init; } = null!;
}