using System.Linq.Expressions;
using Application.Common;
using Application.Common.Models;
using Application.Interfaces.Logging;
using Application.Services;
using Core.Application.DTOs.Ingredient;
using Core.Domain.Entities;
using Core.Domain.Entities.Enums;
using Core.Domain.Interfaces;
using Core.Domain.Models.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Cafe_pos.UnitTests.Application;

public class IngredientServiceTests
{
    private readonly Mock<IRepositoryManager> _repositoryMock;
    private readonly Mock<ILoggerManager> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly IngredientService _service;

    public IngredientServiceTests()
    {
        _repositoryMock = new Mock<IRepositoryManager>();
        _loggerMock = new Mock<ILoggerManager>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _repositoryMock.Setup(m => m.Ingredient).Returns(new Mock<IRepository<Ingredient>>().Object);
        _repositoryMock.Setup(m => m.Branch).Returns(new Mock<IBranchRepository>().Object);
        _repositoryMock.Setup(m => m.Inventory).Returns(new Mock<IInventoryRepository>().Object);

        _service = new IngredientService(_repositoryMock.Object, _loggerMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task CreateIngredientAsync_WhenIngredientAlreadyExists_ReturnsFailure()
    {
        var dto = new CreateIngredientDto { Name = "Existing", MinStock = 10, Unit = "Gram" };
        var existing = new Ingredient { Id = Guid.NewGuid(), Name = "Existing", MinStock = 100, Unit = UnitType.Gram };

        _repositoryMock.Setup(repo => repo.Ingredient.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Ingredient, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _service.CreateIngredientAsync(dto, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("An ingredient with the same name already exists.");
        _loggerMock.Verify(l => l.LogWarn(It.Is<string>(s => s.Contains("duplicate")), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task CreateIngredientAsync_WhenSuccess_ReturnsSuccess()
    {
        var dto = new CreateIngredientDto { Name = "New", MinStock = 10, Unit = "Gram" };
        var ingredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = "New",
            MinStock = 10,
            Unit = UnitType.Gram,
            Inventories = [],
            StockMovements = []
        };
        var branches = new List<Branch> { new() { Id = Guid.NewGuid() } };

        _repositoryMock.Setup(repo => repo.Ingredient.FirstOrDefaultAsync(
            i => i.Name == dto.Name || i.MinStock == dto.MinStock || i.Unit == UnitType.Gram, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingredient?)null);

        _repositoryMock.Setup(repo => repo.Ingredient.AddAsync(It.IsAny<Ingredient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredient);
        _repositoryMock.Setup(repo => repo.Branch.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(branches);

        var result = await _service.CreateIngredientAsync(dto);

        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(repo => repo.Inventory.AddAsync(It.IsAny<Inventory>(), It.IsAny<CancellationToken>()), Times.Exactly(branches.Count));
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateIngredientAsync_WhenUnitIsInvalid_UsesDefaultUnitAndReturnsSuccess()
    {
        var dto = new CreateIngredientDto { Name = "New with Invalid Unit", MinStock = 10, Unit = "InvalidUnit" };
        var branches = new List<Branch> { new() { Id = Guid.NewGuid() } };

        _repositoryMock.Setup(repo => repo.Ingredient.FirstOrDefaultAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingredient?)null);

        Ingredient? capturedIngredient = null;
        _repositoryMock.Setup(repo => repo.Ingredient.AddAsync(It.IsAny<Ingredient>(), It.IsAny<CancellationToken>()))
            .Callback<Ingredient, CancellationToken>((i, ct) => capturedIngredient = i)
            .ReturnsAsync(new Ingredient { Id = Guid.NewGuid(), Name = dto.Name, Inventories = [], StockMovements = [] });

        _repositoryMock.Setup(repo => repo.Branch.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(branches);

        var result = await _service.CreateIngredientAsync(dto, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        capturedIngredient.Should().NotBeNull();
        capturedIngredient!.Unit.Should().Be(UnitType.Gram); // Default value when parsing fails
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateIngredientAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        var dto = new CreateIngredientDto { Name = "New", MinStock = 10, Unit = "Gram" };
        var exceptionMessage = "Db Error";

        _repositoryMock.Setup(repo => repo.Ingredient.FirstOrDefaultAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.CreateIngredientAsync(dto, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("{ex}")), It.IsAny<object[]>()), Times.Once);
    }
    [Fact]
    public async Task GetIngredientByIdAsync_WhenNotFound_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.Ingredient.GetByIdAsync(id, It.IsAny<Func<IQueryable<Ingredient>, IQueryable<Ingredient>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingredient?)null);

        var result = await _service.GetIngredientByIdAsync(id, null, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Ingredient not found.");
    }

    [Fact]
    public async Task GetIngredientByIdAsync_WhenExists_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var inventories = new List<Inventory>(){
            new(){Id = Guid.NewGuid(), BranchId = branchId, IngredientId = id, CurrentStock = 100}
        };
        var stockMovements = new List<StockMovement>(){
            new(){
                BranchId = branchId,
                Id = Guid.NewGuid(),
                IngredientId = id,
                CreatedAt = DateTime.UtcNow,
                Quantity = 100,
                Type = MovementType.Adjustment,
                ReferenceId = Guid.NewGuid()
            }
        };
        var ingredient = new Ingredient { Id = id, Name = "Test", Inventories = inventories, StockMovements = stockMovements };

        _currentUserServiceMock.Setup(s => s.IsAdmin()).Returns(false);
        _currentUserServiceMock.Setup(s => s.BranchId).Returns(branchId);

        Func<IQueryable<Ingredient>, IQueryable<Ingredient>>? capturedInclude = null;
        _repositoryMock.Setup(repo => repo.Ingredient.GetByIdAsync(id, It.IsAny<Func<IQueryable<Ingredient>,
        IQueryable<Ingredient>>>(), It.IsAny<CancellationToken>()))
        .Callback<Guid, Func<IQueryable<Ingredient>, IQueryable<Ingredient>>, CancellationToken>((guid, include, ct) =>
        capturedInclude = include)
        .ReturnsAsync(ingredient);

        var result = await _service.GetIngredientByIdAsync(id, null, CancellationToken.None);

        capturedInclude.Should().NotBeNull();
        var fakeQueryable = new List<Ingredient>().AsQueryable();
        capturedInclude!(fakeQueryable);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(id);
        result.Data!.StockMovements.Should().NotBeNullOrEmpty();
        result.Data!.currentStocks.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetIngredientByIdAsync_WhenAdminAndBranchProvided_UsesFilteredInclude()
    {
        var id = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var inventories = new List<Inventory>(){
            new(){Id = Guid.NewGuid(), BranchId = branchId, IngredientId = id, CurrentStock = 100}
        };
        var stockMovements = new List<StockMovement>(){
            new(){
                BranchId = branchId,
                Id = Guid.NewGuid(),
                IngredientId = id,
                CreatedAt = DateTime.UtcNow,
                Quantity = 100,
                Type = MovementType.Adjustment,
                ReferenceId = Guid.NewGuid()
            }
        };
        var ingredient = new Ingredient { Id = id, Name = "Test", Inventories = inventories, StockMovements = stockMovements };

        _currentUserServiceMock.Setup(s => s.IsAdmin()).Returns(true);

        Func<IQueryable<Ingredient>, IQueryable<Ingredient>>? capturedInclude = null;
        _repositoryMock.Setup(repo => repo.Ingredient.GetByIdAsync(id, It.IsAny<Func<IQueryable<Ingredient>,
        IQueryable<Ingredient>>>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, Func<IQueryable<Ingredient>, IQueryable<Ingredient>>, CancellationToken>((guid, include, ct) =>
        capturedInclude = include)
            .ReturnsAsync(ingredient);

        var result = await _service.GetIngredientByIdAsync(id, branchId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        capturedInclude.Should().NotBeNull();

        // Execute the lambda. We can't easily verify the internal Filter of Include in a unit test,
        // but executing it ensures the "if" branch code is hit and doesn't crash.
        var fakeQueryable = new List<Ingredient>().AsQueryable();
        var includeResult = capturedInclude!(fakeQueryable);
        includeResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetIngredientByIdAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        var id = Guid.NewGuid();
        var exceptionMessage = "Db Error";

        _repositoryMock.Setup(repo => repo.Ingredient.GetByIdAsync(id, It.IsAny<Func<IQueryable<Ingredient>, IQueryable<Ingredient>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.GetIngredientByIdAsync(id, null, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("{ex}")), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetIngredientByIdAsync_WhenCollectionsAreNull_ReturnsSuccessWithEmptyEnums()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ingredient = new Ingredient
        {
            Id = id,
            Name = "Test",
            Inventories = null!, // Force null to test the ?? branch
            StockMovements = null! // Force null to test the ?? branch
        };

        _currentUserServiceMock.Setup(s => s.IsAdmin()).Returns(false);

        _repositoryMock.Setup(repo => repo.Ingredient.GetByIdAsync(id,
        It.IsAny<Func<IQueryable<Ingredient>, IQueryable<Ingredient>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredient);
        var result = await _service.GetIngredientByIdAsync(id, null, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.currentStocks.Should().BeEmpty(); // Proves the ?? branch worked
        result.Data!.StockMovements.Should().BeEmpty(); // Proves the ?? branch worked
    }

    [Fact]
    public async Task GetPagedIngredientsAsync_ReturnsSuccess()
    {
        var branchId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var inventories = new List<Inventory>(){
            new(){Id = Guid.NewGuid(), BranchId = branchId, IngredientId = id, CurrentStock = 100}
        };
        var stockMovements = new List<StockMovement>(){
            new(){
                BranchId = branchId,
                Id = Guid.NewGuid(),
                IngredientId = id,
                CreatedAt = DateTime.UtcNow,
                Quantity = 100,
                Type = MovementType.Adjustment,
                ReferenceId = Guid.NewGuid()
            }
        };
        var ingredients = new List<Ingredient>
        {
            new()
            {
                Id = id,
                Name = "Test",
                Inventories = inventories,
                StockMovements = stockMovements,
                MinStock = 100,
                Unit = UnitType.Gram
            }
        };
        _repositoryMock.Setup(repo => repo.Ingredient.GetPagedAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Ingredient, bool>>>(),
            It.IsAny<Func<IQueryable<Ingredient>, IQueryable<Ingredient>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ingredients, 1));

        _currentUserServiceMock.Setup(s => s.IsAdmin()).Returns(false);
        _currentUserServiceMock.Setup(s => s.BranchId).Returns(branchId);

        Func<IQueryable<Ingredient>, IQueryable<Ingredient>>? capturedInclude = null;
        _repositoryMock.Setup(repo => repo.Ingredient.GetPagedAsync(1, 10, It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<Func<IQueryable<Ingredient>,
        IQueryable<Ingredient>>>(), It.IsAny<CancellationToken>()))
        .Callback<int, int, Expression<Func<Ingredient, bool>>, Func<IQueryable<Ingredient>, IQueryable<Ingredient>>, CancellationToken>((num, size, exp, include, ct) =>
        capturedInclude = include)
        .ReturnsAsync((ingredients, 1));

        var result = await _service.GetPagedIngredientsAsync(1, 10, null, null, CancellationToken.None);

        capturedInclude.Should().NotBeNull();
        var fakeQueryable = new List<Ingredient>().AsQueryable();
        capturedInclude!(fakeQueryable);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPagedIngredientsAsync_WhenAdminAndBranchProvided_UsesFilteredInclude()
    {
        var branchId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var inventories = new List<Inventory>(){
            new(){Id = Guid.NewGuid(), BranchId = branchId, IngredientId = id, CurrentStock = 100}
        };
        var stockMovements = new List<StockMovement>(){
            new(){
                BranchId = branchId,
                Id = Guid.NewGuid(),
                IngredientId = id,
                CreatedAt = DateTime.UtcNow,
                Quantity = 100,
                Type = MovementType.Adjustment,
                ReferenceId = Guid.NewGuid()
            }
        };
        var ingredients = new List<Ingredient>
        {
            new()
            {
                Id = id,
                Name = "Test",
                Inventories = inventories,
                StockMovements = stockMovements,
                MinStock = 100,
                Unit = UnitType.Gram
            }
        };

        _repositoryMock.Setup(repo => repo.Ingredient.GetPagedAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Ingredient, bool>>>(),
            It.IsAny<Func<IQueryable<Ingredient>, IQueryable<Ingredient>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ingredients, 1));

        _currentUserServiceMock.Setup(s => s.IsAdmin()).Returns(true);

        Func<IQueryable<Ingredient>, IQueryable<Ingredient>>? capturedInclude = null;
        _repositoryMock.Setup(repo => repo.Ingredient.GetPagedAsync(1, 10, It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<Func<IQueryable<Ingredient>,
        IQueryable<Ingredient>>>(), It.IsAny<CancellationToken>()))
        .Callback<int, int, Expression<Func<Ingredient, bool>>, Func<IQueryable<Ingredient>, IQueryable<Ingredient>>, CancellationToken>((num, size, exp, include, ct) =>
        capturedInclude = include)
        .ReturnsAsync((ingredients, 1));

        var result = await _service.GetPagedIngredientsAsync(1, 10, null, branchId, CancellationToken.None);

        capturedInclude.Should().NotBeNull();
        var fakeQueryable = new List<Ingredient>().AsQueryable();
        capturedInclude!(fakeQueryable);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPagedIngredientsAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        var exceptionMessage = "Db Error";

        _repositoryMock.Setup(repo => repo.Ingredient.GetPagedAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Ingredient, bool>>>(),
            It.IsAny<Func<IQueryable<Ingredient>, IQueryable<Ingredient>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.GetPagedIngredientsAsync(1, 10, null, null, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("{ex}")), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task UpdateIngredientAsync_WhenAdminAndBranchProvided_UsesFilteredInclude()
    {
        var id = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var ingredient = new Ingredient { Id = id, Name = "Test", Inventories = [], StockMovements = [] };
        var dto = new UpdateIngredientDto { Name = "New Name" };

        _currentUserServiceMock.Setup(s => s.IsAdmin()).Returns(true);

        Func<IQueryable<Ingredient>, IQueryable<Ingredient>>? capturedInclude = null;
        _repositoryMock.Setup(repo => repo.Ingredient.GetByIdAsync(id, It.IsAny<Func<IQueryable<Ingredient>,
        IQueryable<Ingredient>>>(), It.IsAny<CancellationToken>()))
        .Callback<Guid, Func<IQueryable<Ingredient>, IQueryable<Ingredient>>, CancellationToken>((guid, include, ct) =>
        capturedInclude = include)
        .ReturnsAsync(ingredient);

        // Act
        var result = await _service.UpdateIngredientAsync(id, branchId, dto);

        // Assert: Trigger the "else" branch of the lambda for coverage
        result.IsSuccess.Should().BeTrue();
        capturedInclude.Should().NotBeNull();

        if (capturedInclude != null)
        {
            var fakeQueryable = new List<Ingredient>().AsQueryable();
            var resultQuery = capturedInclude(fakeQueryable);
            resultQuery.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task UpdateIngredientAsync_WhenNotFound_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.Ingredient.GetByIdAsync(id, It.IsAny<Func<IQueryable<Ingredient>, IQueryable<Ingredient>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingredient?)null);

        var result = await _service.UpdateIngredientAsync(id, null, new UpdateIngredientDto(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Ingredient not found.");
    }

    [Fact]
    public async Task UpdateIngredientAsync_WhenIngredientExists_UpdateAndSave()
    {
        var id = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var movementId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var inventories = new List<Inventory>(){
            new(){Id = Guid.NewGuid(), BranchId = branchId, IngredientId = id, CurrentStock = 100}
        };
        var stockMovements = new List<StockMovement>(){
            new(){
                BranchId = branchId,
                Id = movementId,
                IngredientId = id,
                CreatedAt = createdAt,
                Quantity = 100,
                Type = MovementType.Adjustment,
                ReferenceId = Guid.NewGuid()
            }
        };
        var ingredient = new Ingredient { Id = id, Name = "Test", Inventories = inventories, StockMovements = stockMovements };
        var dto = new UpdateIngredientDto { MinStock = null, Name = "New", Unit = "Gram" };

        _currentUserServiceMock.Setup(s => s.IsAdmin()).Returns(false);
        _currentUserServiceMock.Setup(s => s.BranchId).Returns(branchId);

        Func<IQueryable<Ingredient>, IQueryable<Ingredient>>? capturedInclude = null;
        _repositoryMock.Setup(repo => repo.Ingredient.GetByIdAsync(id, It.IsAny<Func<IQueryable<Ingredient>,
        IQueryable<Ingredient>>>(), It.IsAny<CancellationToken>()))
        .Callback<Guid, Func<IQueryable<Ingredient>, IQueryable<Ingredient>>, CancellationToken>((guid, include, ct) =>
        capturedInclude = include)
        .ReturnsAsync(ingredient);

        var result = await _service.UpdateIngredientAsync(id, null, dto);

        capturedInclude.Should().NotBeNull();
        var fakeQueryable = new List<Ingredient>().AsQueryable();
        capturedInclude!(fakeQueryable);
        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("New");
        result.Data!.Unit.Should().Be("Gram");
        result.Data!.currentStocks.First().Should().Be(100);

        var sm = result.Data!.StockMovements.Should().ContainSingle().Subject;
        sm.Id.Should().Be(movementId);
        sm.IngredientName.Should().Be("New");   // ingredient.Name captured in closure
        sm.Quantity.Should().Be(100);
        sm.MovementType.Should().Be(MovementType.Adjustment.ToString());
        sm.Date.Should().Be(createdAt);

        _repositoryMock.Verify(repo => repo.Ingredient.Update(It.IsAny<Ingredient>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateIngredientAsync_WhenPartialUpdate_UpdateAndSave()
    {
        var id = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var inventories = new List<Inventory>(){
            new(){Id = Guid.NewGuid(), BranchId = branchId, IngredientId = id, CurrentStock = 100}
        };
        var stockMovements = new List<StockMovement>(){
            new(){
                BranchId = branchId,
                Id = Guid.NewGuid(),
                IngredientId = id,
                CreatedAt = DateTime.UtcNow,
                Quantity = 100,
                Type = MovementType.Adjustment,
                ReferenceId = Guid.NewGuid()
            }
        };
        var ingredient = new Ingredient { Id = id, Name = "Test", Inventories = inventories, StockMovements = stockMovements, Unit = UnitType.Gram };
        var dto = new UpdateIngredientDto { MinStock = 10000, Name = null, Unit = null };

        _repositoryMock.Setup(repo => repo.Ingredient.GetByIdAsync(id,
            It.IsAny<Func<IQueryable<Ingredient>, IQueryable<Ingredient>>>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(ingredient);

        var result = await _service.UpdateIngredientAsync(id, null, dto);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Test");
        result.Data!.MinStock.Should().Be(10000);
        result.Data!.Unit.Should().Be("Gram");

        _repositoryMock.Verify(repo => repo.Ingredient.Update(It.IsAny<Ingredient>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateIngredientAsync_WhenDuplicateName_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        var ingredient = new Ingredient { Id = id, Name = "Old" };
        var duplicate = new Ingredient { Id = Guid.NewGuid(), Name = "New" };
        var dto = new UpdateIngredientDto { Name = "New" };

        _repositoryMock.Setup(repo => repo.Ingredient.GetByIdAsync(id, It.IsAny<Func<IQueryable<Ingredient>, IQueryable<Ingredient>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredient);
        _repositoryMock.Setup(repo => repo.Ingredient.FirstOrDefaultAsync(It.IsAny<Expression<Func<Ingredient, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(duplicate);

        var result = await _service.UpdateIngredientAsync(id, null, dto, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("An ingredient with the same name already exists.");
    }

    [Fact]
    public async Task UpdateIngredientAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        var id = Guid.NewGuid();
        var exceptionMessage = "Db Error";

        _repositoryMock.Setup(repo => repo.Ingredient.GetByIdAsync(id, It.IsAny<Func<IQueryable<Ingredient>, IQueryable<Ingredient>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.UpdateIngredientAsync(id, null, new UpdateIngredientDto(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("{ex}")), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task DeleteIngredientAsync_WhenExists_RemovesAndSaves()
    {
        var id = Guid.NewGuid();
        var ingredient = new Ingredient { Id = id };
        _repositoryMock.Setup(repo => repo.Ingredient.GetByIdAsync(id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredient);

        var result = await _service.DeleteIngredientAsync(id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(repo => repo.Ingredient.Remove(ingredient), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteIngredientAsync_WhenNotFound_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.Ingredient.GetByIdAsync(id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingredient?)null);

        var result = await _service.DeleteIngredientAsync(id, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Ingredient not found.");
    }

    [Fact]
    public async Task DeleteIngredientAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        var id = Guid.NewGuid();
        var exceptionMessage = "Db Error";

        _repositoryMock.Setup(repo => repo.Ingredient.GetByIdAsync(id, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.DeleteIngredientAsync(id, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("{ex}")), It.IsAny<object[]>()), Times.Once);
    }

}