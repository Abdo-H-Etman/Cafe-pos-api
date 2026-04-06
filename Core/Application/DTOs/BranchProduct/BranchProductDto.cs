namespace Core.Application.DTOs.BranchProduct;

public record BranchProductDto
{
    public Guid ProductId { get; init; }
    public Guid BranchId { get; init; }
    public string ProductName { get; init; } = null!;
    public decimal Price { get; init; }
}