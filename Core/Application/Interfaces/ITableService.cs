using Application.Common.Models;
using Core.Application.DTOs.Table;

namespace Core.Application.Interfaces;

public interface ITableService
{
    Task<Result<IEnumerable<TableDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<TableDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<TableDto>> CreateAsync(CreateTableDto createTableDto, CancellationToken cancellationToken = default);
    Task<Result<TableDto>> UpdateAsync(Guid id, UpdateTableDto updateTableDto, CancellationToken cancellationToken = default);
    Task<Result<TableDto>> ReserveTableAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<TableDto>> UnreserveTableAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}