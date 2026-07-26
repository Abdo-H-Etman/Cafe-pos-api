using Application.Common;
using Application.Common.Models;
using Application.Interfaces.Logging;
using Core.Application.DTOs.Order;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Entities.Enums;
using Core.Domain.Interfaces;
using Core.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class OrderService : IOrderService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerManager _logger;

    public OrderService(IRepositoryManager repositoryManager, ICurrentUserService currentUserService, ILoggerManager logger)
    {
        _repositoryManager = repositoryManager;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> CreateAsync(CreateOrderDto createOrderDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (createOrderDto.Items == null || createOrderDto.Items.Count == 0)
            {
                return Result<OrderDto>.Failure("At least one order item is required.");
            }

            var branchId = _currentUserService.BranchId;
            var cashierId = _currentUserService.UserId;

            if (branchId == Guid.Empty || cashierId == Guid.Empty)
            {
                return Result<OrderDto>.Failure("Branch and cashier information is required.");
            }

            var subtotal = createOrderDto.Items.Sum(i => i.Quantity * i.UnitPrice);
            var total = subtotal + subtotal * (createOrderDto.Tax / 100);

            total -= createOrderDto.DiscountType switch
            {
                nameof(DiscountType.Percentage) when createOrderDto.DiscountValue.HasValue => total * (createOrderDto.DiscountValue.Value / 100),
                nameof(DiscountType.FixedAmount) when createOrderDto.DiscountValue.HasValue => createOrderDto.DiscountValue.Value,
                _ => 0
            };

            var order = new Order
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                CashierId = cashierId,
                TableId = createOrderDto.TableId,
                SubTotal = subtotal,
                Tax = createOrderDto.Tax,
                DiscountType = createOrderDto.DiscountType switch
                {
                    nameof(DiscountType.Percentage) => DiscountType.Percentage,
                    nameof(DiscountType.FixedAmount) => DiscountType.FixedAmount,
                    _ => null
                },
                DiscountValue = createOrderDto.DiscountValue,
                Total = total,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                OrderItems = createOrderDto.Items.Select(item => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice
                }).ToList()
            };

            await _repositoryManager.Order.AddAsync(order, cancellationToken);
            if (order.TableId != null)
            {
                var table = await _repositoryManager.Table.GetByIdAsync((Guid)order.TableId,
                        cancellationToken: cancellationToken);
                table!.Status = TableStatus.Occupied;
            }

            await _repositoryManager.SaveAsync(cancellationToken);

            foreach (var item in order.OrderItems)
            {
                var recipes = await _repositoryManager.Recipe.FindAsync(r => r.ProductId == item.ProductId, cancellationToken: cancellationToken);
                foreach (var recipe in recipes ?? Enumerable.Empty<Recipe>())
                {
                    var quantityToDeduct = recipe.Quantity * item.Quantity;
                    await _repositoryManager.Inventory.UpdateStockAsync(branchId, recipe.IngredientId, -quantityToDeduct, cancellationToken);

                    var stockMovement = new StockMovement
                    {
                        Id = Guid.NewGuid(),
                        BranchId = branchId,
                        IngredientId = recipe.IngredientId,
                        Quantity = quantityToDeduct,
                        Type = MovementType.Sale,
                        ReferenceId = order.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _repositoryManager.StockMovement.AddAsync(stockMovement, cancellationToken);
                }
            }

            await _repositoryManager.SaveAsync(cancellationToken);

            var result = MapToDto(order);
            return Result<OrderDto>.Success(result, "Order created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating order: {ex}", ex);
            return Result<OrderDto>.Failure("An error occurred while creating the order.");
        }
    }

    public async Task<Result<OrderDto>> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _repositoryManager.Order.GetByIdAsync(orderId,
                    include: q => q.Include(o => o.Table)
                        .Include(o => o.OrderItems).ThenInclude(oi => oi.Product),
                    cancellationToken: cancellationToken);
            if (order == null)
            {
                return Result<OrderDto>.Failure("Order not found.");
            }

            return Result<OrderDto>.Success(MapToDto(order), "Order fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching order: {ex}", ex);
            return Result<OrderDto>.Failure("An error occurred while fetching the order.");
        }
    }

    public async Task<Result<IEnumerable<OrderDto>, MetaData>> GetPagedOrdersAsync(int pageNumber, int pageSize,
                DateOnly? fromDate, DateOnly? toDate,
                Guid? cashierId = null, string? status = null, Guid? adminSelectedBranchId = null,
                CancellationToken cancellationToken = default)
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (adminSelectedBranchId.HasValue)
            {
                branchId = adminSelectedBranchId.Value;
            }

            var (orders, totalCount) = await _repositoryManager.Order.GetPagedAsync(pageNumber, pageSize,
                    predicate: o => (branchId == Guid.Empty || o.BranchId == branchId) &&
                                    (!cashierId.HasValue || o.CashierId == cashierId) &&
                                    (fromDate == null || DateOnly.FromDateTime(o.CreatedAt) >= fromDate) &&
                                    (toDate == null || DateOnly.FromDateTime(o.CreatedAt) <= toDate) &&
                                    (string.IsNullOrEmpty(status) || o.Status.ToString() == status),
                    include: q => q.Include(o => o.Table)
                        .Include(o => o.Cashier)
                        .Include(o => o.OrderItems).ThenInclude(oi => oi.Product),
                    cancellationToken: cancellationToken);

            var ordersDto = orders.Select(MapToDto);
            var metaData = new MetaData(pageNumber, pageSize, totalCount);
            return Result<IEnumerable<OrderDto>, MetaData>.Success(ordersDto, metaData, "Orders fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching paged orders: {ex}", ex);
            return Result<IEnumerable<OrderDto>, MetaData>.Failure("An error occurred while fetching the paged orders.");
        }
    }

    public async Task<Result<OrderDto>> UpdateStatusAsync(Guid orderId, string status, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _repositoryManager.Order.GetByIdAsync(orderId,
                            include: q => q.Include(o => o.Table)
                                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                            , cancellationToken: cancellationToken);
            if (order == null)
            {
                return Result<OrderDto>.Failure("Order not found.");
            }

            if (!Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                return Result<OrderDto>.Failure("Invalid order status.");
            }

            order.Status = parsedStatus;
            _repositoryManager.Order.Update(order);

            if (parsedStatus != OrderStatus.Pending && order.TableId != null)
            {
                var table = await _repositoryManager.Table.GetByIdAsync((Guid)order.TableId, cancellationToken: cancellationToken);
                if (table != null)
                {
                    table.Status = TableStatus.Available;
                }
            }
            await _repositoryManager.SaveAsync(cancellationToken);

            return Result<OrderDto>.Success(MapToDto(order), "Order status updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating order status: {ex}", ex);
            return Result<OrderDto>.Failure("An error occurred while updating the order status.");
        }
    }

    public async Task<Result<OrderDto>> UpdateAsync(Guid orderId, UpdateOrderDto updateOrderDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (updateOrderDto.Items.Any(i => i.Quantity <= 0))
            {
                return Result<OrderDto>.Failure("Item quantity must be greater than zero.");
            }

            if (updateOrderDto.Tax < 0)
            {
                return Result<OrderDto>.Failure("Tax cannot be negative.");
            }

            DiscountType? discountType = null;
            if (!string.IsNullOrEmpty(updateOrderDto.DiscountType))
            {
                if (!Enum.TryParse<DiscountType>(updateOrderDto.DiscountType, out var parsedType))
                {
                    return Result<OrderDto>.Failure($"Invalid discount type '{updateOrderDto.DiscountType}'.");
                }
                discountType = parsedType;

                if (!updateOrderDto.DiscountValue.HasValue || updateOrderDto.DiscountValue.Value < 0)
                {
                    return Result<OrderDto>.Failure("A non-negative discount value is required when a discount type is specified.");
                }

                if (discountType == DiscountType.Percentage && updateOrderDto.DiscountValue.Value > 100)
                {
                    return Result<OrderDto>.Failure("Percentage discount cannot exceed 100.");
                }
            }

            var order = await _repositoryManager.Order.GetByIdAsync(orderId,
                        include: q => q.Include(o => o.Table)
                            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product),
                        cancellationToken: cancellationToken);
            if (order == null)
            {
                return Result<OrderDto>.Failure("Order not found.");
            }

            if (order.Status is OrderStatus.Paid or OrderStatus.Cancelled or OrderStatus.Refunded)
            {
                return Result<OrderDto>.Failure($"Order cannot be modified while in status '{order.Status}'.");
            }


            if (updateOrderDto.Items != null && updateOrderDto.Items.Count > 0)
            {
                var productIds = updateOrderDto.Items.Select(i => i.ProductId).Distinct().ToList();
                var products = await _repositoryManager.Product.GetByIdsAsync(productIds, cancellationToken: cancellationToken);
                var productPriceMap = products.ToDictionary(p => p.Id, p => p.Price);

                var missingProductIds = productIds.Where(id => !productPriceMap.ContainsKey(id)).ToList();
                if (missingProductIds.Count > 0)
                {
                    return Result<OrderDto>.Failure($"Unknown product(s): {string.Join(", ", missingProductIds)}.");
                }
                var incomingByProductId = updateOrderDto.Items
                    .GroupBy(i => i.ProductId)
                    .ToDictionary(g => g.Key, g => g.First());

                var existingByProductId = order.OrderItems.ToDictionary(oi => oi.ProductId, oi => oi);

                foreach (var (productId, itemDto) in incomingByProductId)
                {
                    var unitPrice = productPriceMap[productId];

                    if (existingByProductId.TryGetValue(productId, out var existingItem))
                    {
                        existingItem.Quantity = itemDto.Quantity;
                        existingItem.UnitPrice = unitPrice;
                        existingItem.TotalPrice = itemDto.Quantity * unitPrice;
                    }
                    else
                    {
                        order.OrderItems.Add(new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = productId,
                            Quantity = itemDto.Quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = itemDto.Quantity * unitPrice
                        });
                    }
                }
            }
            var effectiveTax = updateOrderDto.Tax ?? order.Tax;
            var subtotal = order.OrderItems.Sum(i => i.TotalPrice);
            var total = subtotal + subtotal * (effectiveTax / 100);

            var discountAmount = discountType != null
                ? discountType switch
                {
                    DiscountType.Percentage => total * (updateOrderDto.DiscountValue!.Value / 100),
                    DiscountType.FixedAmount => updateOrderDto.DiscountValue!.Value,
                    _ => 0m
                }
                : order.DiscountType switch
                {
                    DiscountType.Percentage => total * (order.DiscountValue!.Value / 100),
                    DiscountType.FixedAmount => order.DiscountValue!.Value,
                    _ => 0m
                };


            total = Math.Max(0, total - discountAmount);

            if (!Enum.TryParse<PaymentMethod>(updateOrderDto.PaymentMethod, ignoreCase: true, out var paymentMethod))
            {
                return Result<OrderDto>.Failure("Invalid payment method.");
            }

            order.Tax = effectiveTax;
            order.DiscountType = discountType ?? order.DiscountType;
            order.DiscountValue = discountType.HasValue ? updateOrderDto.DiscountValue : order.DiscountValue;
            order.SubTotal = subtotal;
            order.Total = total;
            order.PaymentMethod = paymentMethod;

            _repositoryManager.Order.Update(order);

            try
            {
                await _repositoryManager.SaveAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarn("Concurrency conflict updating order {OrderId}: {ex}", orderId, ex);
                return Result<OrderDto>.Failure("This order was modified by another request. Please reload and try again.");
            }

            return Result<OrderDto>.Success(MapToDto(order), "Order updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating order {ex}", ex);
            return Result<OrderDto>.Failure($"An error occurred while updating the order. {ex.Message}");
        }
    }
    public async Task<Result> DeleteAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _repositoryManager.BeginTransactionAsync(cancellationToken);

        try
        {
            var order = await _repositoryManager.Order.GetByIdAsync(orderId,
                    include: q => q.Include(o => o.OrderItems),
                    cancellationToken: cancellationToken);
            if (order == null)
            {
                return Result.Failure("Order not found.");
            }

            if (order.Status != OrderStatus.Pending)
            {
                return Result.Failure("Only pending orders can be deleted.");
            }

            if (order.OrderItems is { Count: > 0 })
            {
                var productIds = order.OrderItems
                    .Select(i => i.ProductId)
                    .Distinct()
                    .ToList();

                var allRecipes = await _repositoryManager.Recipe.FindAsync(
                    r => productIds.Contains(r.ProductId),
                    cancellationToken: cancellationToken);

                var recipesByProduct = (allRecipes ?? Enumerable.Empty<Recipe>())
                    .GroupBy(r => r.ProductId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var stockMovements = new List<StockMovement>();

                foreach (var item in order.OrderItems)
                {
                    if (!recipesByProduct.TryGetValue(item.ProductId, out var recipes))
                    {
                        continue;
                    }

                    foreach (var recipe in recipes)
                    {
                        var quantityToRestore = recipe.Quantity * item.Quantity;

                        await _repositoryManager.Inventory.UpdateStockAsync(
                            order.BranchId,
                            recipe.IngredientId,
                            quantityToRestore,
                            cancellationToken);

                        stockMovements.Add(new StockMovement
                        {
                            Id = Guid.NewGuid(),
                            BranchId = order.BranchId,
                            IngredientId = recipe.IngredientId,
                            Quantity = quantityToRestore,
                            Type = MovementType.Restock,
                            ReferenceId = order.Id,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                if (stockMovements.Count > 0)
                {
                    await _repositoryManager.StockMovement.AddRangeAsync(stockMovements, cancellationToken);
                }

                _repositoryManager.OrderItem.RemoveRange(order.OrderItems);
            }

            _repositoryManager.Order.Remove(order);

            await _repositoryManager.SaveAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success("Order deleted successfully.");
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _logger.LogError("Error deleting order {OrderId}. {ex}", orderId, ex);
            return Result.Failure("An error occurred while deleting the order.");
        }
    }
    private static OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        Cashier = order.Cashier?.Name ?? string.Empty,
        Total = order.Total,
        Date = order.CreatedAt,
        Status = order.Status.ToString(),
        PaymentMethod = order.PaymentMethod.ToString(),
        Table = order.Table?.Name ?? "Takeaway",
        Tax = order.Tax / 100 * order.SubTotal,
        DiscountType = order.DiscountType?.ToString(),
        DiscountValue = order.DiscountValue ?? 0,
        Items = [.. order.OrderItems.Select(item => new OrderItemDto
        {
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? string.Empty,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.TotalPrice
        })]
    };
}
