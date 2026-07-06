namespace Core.Application.DTOs.Table;

public record UpdateTableDto
{
    public string? Name { get; init; }
    public int? Capacity { get; init; }
}