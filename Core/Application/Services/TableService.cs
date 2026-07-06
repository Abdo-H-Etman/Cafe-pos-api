using Application.Common;
using Application.Common.Models;
using Core.Application.DTOs.Table;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Interfaces;

namespace Application.Services;

public class TableService : ITableService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly ICurrentUserService _currentUserService;

    public TableService(IRepositoryManager repositoryManager, ICurrentUserService currentUserService)
    {
        _repositoryManager = repositoryManager;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IEnumerable<TableDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tables = await _repositoryManager.Table.GetAllAsync(cancellationToken);
        var tableDtos = tables.Select(t => new TableDto
        {
            Id = t.Id,
            Name = t.Name,
            Capacity = t.Capacity
        });

        return Result<IEnumerable<TableDto>>.Success(tableDtos);
    }

    public async Task<Result<TableDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var table = await _repositoryManager.Table.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (table == null)
        {
            return Result<TableDto>.Failure("Table not found.");
        }

        var tableDto = new TableDto
        {
            Id = table.Id,
            Name = table.Name,
            Capacity = table.Capacity
        };

        return Result<TableDto>.Success(tableDto);
    }

    public async Task<Result<TableDto>> CreateAsync(CreateTableDto createTableDto, CancellationToken cancellationToken = default)
    {
        var table = new Table
        {
            Id = Guid.NewGuid(),
            Name = createTableDto.Name,
            Capacity = createTableDto.Capacity,
            BranchId = _currentUserService.BranchId
        };

        await _repositoryManager.Table.AddAsync(table, cancellationToken);
        await _repositoryManager.SaveAsync(cancellationToken);

        var tableDto = new TableDto
        {
            Id = table.Id,
            Name = table.Name,
            Capacity = table.Capacity
        };

        return Result<TableDto>.Success(tableDto);
    }

    public async Task<Result<TableDto>> UpdateAsync(Guid id, UpdateTableDto updateTableDto, CancellationToken cancellationToken = default)
    {
        var table = await _repositoryManager.Table.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (table == null)
        {
            return Result<TableDto>.Failure("Table not found.");
        }

        table.Name = updateTableDto.Name ?? table.Name;
        table.Capacity = updateTableDto.Capacity ?? table.Capacity;

        await _repositoryManager.SaveAsync(cancellationToken);

        var tableDto = new TableDto
        {
            Id = table.Id,
            Name = table.Name,
            Capacity = table.Capacity
        };
        return Result<TableDto>.Success(tableDto);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var table = await _repositoryManager.Table.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (table == null)
        {
            return Result.Failure("Table not found.");
        }

        _repositoryManager.Table.Remove(table);
        await _repositoryManager.SaveAsync(cancellationToken);

        return Result.Success("Table deleted successfully.");
    }
}