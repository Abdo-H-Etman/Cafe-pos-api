using Application.Common.Models;
using Core.Application.DTOs.Order;

namespace Core.Application.Interfaces;

public interface IOrderService
{
    Task<Result<OrderDto>> CreateAsync(CreateOrderDto createOrderDto, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<OrderDto>, MetaData>> GetPagedOrdersAsync(int pageNumber, int pageSize,
                DateOnly? fromDate, DateOnly? toDate, Guid? cashierId = null,
                string? status = null, Guid? adminSelectedBranchId = null, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> UpdateStatusAsync(Guid orderId, string status, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> UpdateAsync(Guid orderId, UpdateOrderDto updateOrderDto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid orderId, CancellationToken cancellationToken = default);
}
