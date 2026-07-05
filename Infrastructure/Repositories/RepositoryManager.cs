using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Repositories;

public class RepositoryManager : IRepositoryManager
{
    private readonly AppDbContext _context;
    private IServiceProvider _serviceProvider;

    public RepositoryManager(AppDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }
    public IRefreshTokenRepository RefreshToken => _serviceProvider.GetRequiredService<IRefreshTokenRepository>();
    public IRepository<Ingredient> Ingredient => _serviceProvider.GetRequiredService<IRepository<Ingredient>>();
    public IInventoryRepository Inventory => _serviceProvider.GetRequiredService<IInventoryRepository>();
    public IRepository<StockMovement> StockMovement => _serviceProvider.GetRequiredService<IRepository<StockMovement>>();
    public IUserRepository User => _serviceProvider.GetRequiredService<IUserRepository>();
    public IBranchRepository Branch => _serviceProvider.GetRequiredService<IBranchRepository>();

    public async Task SaveAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}