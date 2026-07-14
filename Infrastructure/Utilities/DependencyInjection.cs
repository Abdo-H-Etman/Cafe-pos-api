using Application.Interfaces.Logging;
using Application.Services.Logging;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Utilities;

public static class DependencyInjection
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        services.AddSingleton<ILoggerManager, LoggerManager>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IRepository<Ingredient>, Repository<Ingredient>>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IRepository<StockMovement>, Repository<StockMovement>>();
        services.AddScoped<IRepository<Table>, Repository<Table>>();
        services.AddScoped<IRepository<Recipe>, Repository<Recipe>>();
        services.AddScoped<IRepository<Order>, Repository<Order>>();
        services.AddScoped<IRepository<OrderItem>, Repository<OrderItem>>();
        services.AddScoped<IRepository<Product>, Repository<Product>>();
    }
}
