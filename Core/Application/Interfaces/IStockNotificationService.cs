namespace Application.Interfaces;

public interface IStockNotificationService
{
    Task NotifyStockUpdatedAsync(Guid branchId, Guid ingredientId, decimal newStockLevel, CancellationToken cancellationToken = default);
    Task NotifyLowStockAsync(Guid branchId, Guid ingredientId, string ingredientName, decimal currentStock, decimal minStock, CancellationToken cancellationToken = default);
}
