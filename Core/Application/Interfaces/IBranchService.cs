using Application.Common.Models;
using Core.Application.DTOs.Branch;

namespace Core.Application.Interfaces;

public interface IBranchService
{
    Task<Result<BranchDto>> CreateBranchAsync(CreateBranchDto createBranchDto, CancellationToken cancellationToken = default);
    Task<Result<BranchDto>> GetBranchByIdAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<Result<BranchDetailsDto>> GetBranchDetailsByIdAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<BranchDto>>> GetAllBranchesAsync(CancellationToken cancellationToken = default);
    Task<Result<BranchDto>> UpdateBranchAsync(Guid branchId, UpdateBranchDto updateBranchDto, CancellationToken cancellationToken = default);
    Task<Result> DeleteBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
}