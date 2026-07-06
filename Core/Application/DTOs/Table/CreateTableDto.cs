namespace Core.Application.DTOs.Table;

public record CreateTableDto
{
    public string Name { get; init; } = null!;
    public int Capacity { get; init; }
}