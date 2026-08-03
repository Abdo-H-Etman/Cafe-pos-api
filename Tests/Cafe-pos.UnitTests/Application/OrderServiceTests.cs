using System.Linq.Expressions;
using Application.Common;
using Application.Interfaces.Logging;
using Application.Services;
using Core.Application.DTOs.Order;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Entities.Enums;
using Core.Domain.Interfaces;
using Core.Domain.Models.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace Cafe_pos.UnitTests.Application;

public class OrderServiceTests
{
    private readonly Mock<IRepositoryManager> _repositoryMock;
    private readonly Mock<ILoggerManager> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _repositoryMock = new Mock<IRepositoryManager>();
        _loggerMock = new Mock<ILoggerManager>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _repositoryMock.Setup(m => m.Order).Returns(new Mock<IRepository<Order>>().Object);
        _repositoryMock.Setup(m => m.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IDbContextTransaction>());
        _repositoryMock.Setup(m => m.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _currentUserMock.Setup(s => s.BranchId).Returns(Guid.NewGuid());
        _currentUserMock.Setup(s => s.UserId).Returns(Guid.NewGuid());
        _service = new OrderService(_repositoryMock.Object, _currentUserMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenItemsMissing_ReturnsFailure()
    {
        var result = await _service.CreateAsync(new CreateOrderDto { Items = [] });

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("At least one order item is required.");
    }

    [Fact]
    public async Task CreateAsync_WhenSuccess_ReturnsSuccess()
    {
        var branchId = Guid.NewGuid();
        var cashierId = Guid.NewGuid();

        _currentUserMock.SetupGet(x => x.BranchId).Returns(branchId);
        _currentUserMock.SetupGet(x => x.UserId).Returns(cashierId);

        var orderRepository = new Mock<IRepository<Order>>();
        _repositoryMock.Setup(m => m.Order).Returns(orderRepository.Object);
        _repositoryMock.Setup(m => m.Recipe).Returns(new Mock<IRepository<Recipe>>().Object);
        _repositoryMock.Setup(m => m.Inventory).Returns(new Mock<IInventoryRepository>().Object);
        _repositoryMock.Setup(m => m.StockMovement).Returns(new Mock<IRepository<StockMovement>>().Object);

        orderRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken _) => order);

        var createDto = new CreateOrderDto
        {
            Tax = 5m,
            Items =
            [
                new CreateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 10m }
            ]
        };

        var result = await _service.CreateAsync(createDto);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(21m);
        orderRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateAsync_WhenOrderContainsRecipes_UpdatesInventoryAndCreatesStockMovement()
    {
        var branchId = Guid.NewGuid();
        var cashierId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        _currentUserMock.SetupGet(x => x.BranchId).Returns(branchId);
        _currentUserMock.SetupGet(x => x.UserId).Returns(cashierId);

        var orderRepository = new Mock<IRepository<Order>>();
        var recipeRepository = new Mock<IRepository<Recipe>>();
        var inventoryRepository = new Mock<IInventoryRepository>();
        var stockMovementRepository = new Mock<IRepository<StockMovement>>();

        _repositoryMock.Setup(m => m.Order).Returns(orderRepository.Object);
        _repositoryMock.Setup(m => m.Recipe).Returns(recipeRepository.Object);
        _repositoryMock.Setup(m => m.Inventory).Returns(inventoryRepository.Object);
        _repositoryMock.Setup(m => m.StockMovement).Returns(stockMovementRepository.Object);

        orderRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken _) => order);

        recipeRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Recipe, bool>>>(), It.IsAny<Func<IQueryable<Recipe>, IQueryable<Recipe>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Recipe { ProductId = productId, IngredientId = ingredientId, Quantity = 2m }]);

        inventoryRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Inventory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Inventory { BranchId = branchId, IngredientId = ingredientId, CurrentStock = 10m });

        stockMovementRepository
            .Setup(r => r.AddAsync(It.IsAny<StockMovement>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockMovement movement, CancellationToken _) => movement);

        var result = await _service.CreateAsync(new CreateOrderDto
        {
            Tax = 0m,
            Items = [new CreateOrderItemDto { ProductId = productId, Quantity = 2, UnitPrice = 10m }]
        });

        result.IsSuccess.Should().BeTrue();
        inventoryRepository.Verify(r => r.UpdateStockAsync(branchId, ingredientId, -4m, It.IsAny<CancellationToken>()), Times.Once);
        stockMovementRepository.Verify(r => r.AddAsync(It.Is<StockMovement>(m => m.Type == MovementType.Sale && m.ReferenceId == result.Data!.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenExceptionThrown_ReturnsFailure()
    {
        _repositoryMock
            .Setup(r => r.Order.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var createDto = new CreateOrderDto
        {
            Tax = 5m,
            Items = [new CreateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 10m }]
        };

        var result = await _service.CreateAsync(createDto);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("An error occurred while creating the order.");
    }

    [Fact]
    public async Task UpdateAsync_WhenOrderNotFound_ReturnsFailure()
    {
        var orderId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Order.GetByIdAsync(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var updateDto = new UpdateOrderDto
        {
            Tax = 5m,
            Items = [new UpdateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 10m }]
        };

        var result = await _service.UpdateAsync(orderId, updateDto);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Order not found.");
    }

    [Fact]
    public async Task UpdateAsync_WhenSuccess_ReturnsSuccess()
    {
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var existingOrder = new Order
        {
            BranchId = Guid.NewGuid(),
            CashierId = Guid.NewGuid(),
            Id = orderId,
            Status = OrderStatus.Pending,
            OrderItems = []
        };

        _repositoryMock.Setup(r => r.Order.GetByIdAsync(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        var productRepository = new Mock<IRepository<Product>>();
        _repositoryMock.Setup(r => r.Product).Returns(productRepository.Object);
        productRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Product { Id = productId, Price = 10m }]);

        var updateDto = new UpdateOrderDto
        {
            Tax = 5m,
            Items = [new UpdateOrderItemDto { ProductId = productId, Quantity = 2, UnitPrice = 10m }]
        };

        var result = await _service.UpdateAsync(orderId, updateDto);

        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.Order.Update(existingOrder), Times.Once);
        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DeleteAsync_WhenOrderExists_DeletesOrderAndReturnsSuccess()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, OrderItems = [] };

        _repositoryMock.Setup(r => r.Order.GetByIdAsync(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.DeleteAsync(orderId);

        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.Order.Remove(order), Times.Once);
        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DeleteAsync_WhenStatusIsNotPending_ReturnsFailure()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Paid, OrderItems = [] };

        _repositoryMock.Setup(r => r.Order.GetByIdAsync(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.DeleteAsync(orderId);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Only pending orders can be deleted.");
    }

    [Fact]
    public async Task DeleteAsync_WhenOrderNotFound_ReturnsFailure()
    {
        var orderId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Order.GetByIdAsync(orderId, It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _service.DeleteAsync(orderId);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Order not found.");
    }
}
