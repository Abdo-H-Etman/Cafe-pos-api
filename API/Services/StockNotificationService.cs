using Application.Interfaces;
using API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace API.Services;

public class StockNotificationService : IStockNotificationService
{
    private readonly IHubContext<StockHub> _hubContext;

    public StockNotificationService(IHubContext<StockHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyStockUpdatedAsync(Guid branchId, Guid ingredientId, decimal newStockLevel, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync("StockUpdated", new 
        { 
            BranchId = branchId, 
            IngredientId = ingredientId, 
            NewStockLevel = newStockLevel 
        }, cancellationToken);
    }

    public async Task NotifyLowStockAsync(Guid branchId, Guid ingredientId, string ingredientName, decimal currentStock, decimal minStock, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync("LowStockAlert", new 
        { 
            BranchId = branchId, 
            IngredientId = ingredientId, 
            IngredientName = ingredientName,
            CurrentStock = currentStock,
            MinStock = minStock
        }, cancellationToken);
    }
}
