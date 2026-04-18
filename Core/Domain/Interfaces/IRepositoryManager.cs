using Core.Domain.Entities;

namespace Core.Domain.Interfaces;

public interface IRepositoryManager : IDisposable
{
    IRepository<RefreshToken> RefreshToken { get; }
    IRepository<Ingredient> Ingredient { get; }
    IInventoryRepository Inventory { get; }
    IRepository<StockMovement> StockMovement { get; }
    IUserRepository User { get; }
    IBranchRepository Branch { get; }

    Task SaveAsync(CancellationToken cancellationToken = default);
}