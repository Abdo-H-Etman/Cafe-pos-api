using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

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
    IRepository<Order> Order { get; }
    IRepository<Recipe> Recipe { get; }
    IRepository<OrderItem> OrderItem { get; }
    IRepository<Product> Product { get; }
    IRepository<Reservation> Reservation { get; }

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
}