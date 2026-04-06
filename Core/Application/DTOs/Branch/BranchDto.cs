namespace Core.Application.DTOs.Branch;

public record BranchDto : CreateBranchDto
{
    public Guid Id { get; init; }
}