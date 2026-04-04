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
    public IRepository<RefreshToken> RefreshToken => _serviceProvider.GetRequiredService<IRepository<RefreshToken>>();
    public IUserRepository User => _serviceProvider.GetRequiredService<IUserRepository>();
    public async Task SaveAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}