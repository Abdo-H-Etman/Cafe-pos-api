using Application.DTOs.User;
using Core.Application.DTOs.BranchProduct;
using Core.Application.DTOs.Order;
using Core.Application.DTOs.Stock;
using Core.Application.DTOs.Table;

namespace Core.Application.DTOs.Branch;

public record BranchDetailsDto : BranchDto
{
    public IEnumerable<UserDetailsDto> Users { get; init; } = [];
    public IEnumerable<BranchProductDto> BranchProducts { get; init; } = [];
    public IEnumerable<StockMovementDto> StockMovements { get; init; } = [];
    public IEnumerable<OrderDto> Orders { get; init; } = [];
    public IEnumerable<TableDto> Tables { get; init; } = [];
}