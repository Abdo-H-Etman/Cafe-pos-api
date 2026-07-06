using Core.Domain.Entities;

namespace Core.Domain.Interfaces;

public interface IRepositoryManager : IDisposable
{
    IRefreshTokenRepository RefreshToken { get; }
    IRepository<Ingredient> Ingredient { get; }
    IInventoryRepository Inventory { get; }
    IRepository<StockMovement> StockMovement { get; }
    IRepository<Table> Table { get; }
    IUserRepository User { get; }
    IBranchRepository Branch { get; }

    Task SaveAsync(CancellationToken cancellationToken = default);
}