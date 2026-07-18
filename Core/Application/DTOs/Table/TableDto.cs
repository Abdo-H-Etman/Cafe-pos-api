namespace Core.Application.DTOs.Table;

public record TableDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Capacity { get; init; }
    public string Status { get; init; } = string.Empty;
}