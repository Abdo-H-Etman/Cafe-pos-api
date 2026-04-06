using Core.Domain.Entities;

namespace Core.Domain.Interfaces;

public interface IBranchRepository : IRepository<Branch>
{
    Task<Branch?> GetBranchWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}