using System.Linq.Expressions;
using Application.Interfaces.Logging;
using Application.Services;
using Core.Application.DTOs.Branch;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using FluentAssertions;
using Infrastructure.Repositories;
using Moq;

namespace Cafe_pos.UnitTests.Application;

public class BranchServiceTests
{
    private readonly Mock<IRepositoryManager> _repositoryMock;
    private readonly Mock<ILoggerManager> _loggerMock;
    private readonly BranchService _service;

    public BranchServiceTests()
    {
        _repositoryMock = new Mock<IRepositoryManager>();
        _loggerMock = new Mock<ILoggerManager>();

        _repositoryMock.Setup(m => m.Branch).Returns(new Mock<IBranchRepository>().Object);

        _service = new BranchService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateBranchAsync_WhenBranchAlreadyExists_ReturnsFailure()
    {
        var createBranchDto = new CreateBranchDto
        {
            Name = "Existing Branch",
            Address = "123 Street"
        };

        var existingBranch = new Branch
        {
            Id = Guid.NewGuid(),
            Name = "Existing Branch",
            Address = "123 Street"
        };

        _repositoryMock.Setup(repo => repo.Branch.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Branch, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBranch);

        var result = await _service.CreateBranchAsync(createBranchDto);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Contains("A branch with the same name or address already exists.");

        _loggerMock.Verify(l => l.LogWarn(
            It.Is<string>(s => s.Contains("duplicate name")),
            It.IsAny<object[]>()), Times.Once);

        _repositoryMock.Verify(repo => repo.Branch.AddAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBranchAsync_WhenSuccess_ReturnSuccess()
    {
        var createBranchDto = new CreateBranchDto
        {
            Name = "Branch #10",
            Address = "Cairo 123 Street"
        };

        _repositoryMock.Setup(repo => repo.Branch.FirstOrDefaultAsync(
            e => e.Name == createBranchDto.Name || e.Address == createBranchDto.Address,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        var result = await _service.CreateBranchAsync(createBranchDto);

        result.IsSuccess.Should().BeTrue();
        result.Message.Contains("Branch created successfully.");

        _repositoryMock.Verify(repo => repo.Branch.AddAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBranchAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        var exceptionMessage = "Database error";
        var createBranchDto = new CreateBranchDto
        {
            Name = "Branch #10",
            Address = "Cairo 123 Street"
        };

        _repositoryMock.Setup(repo => repo.Branch.AddAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.CreateBranchAsync(createBranchDto);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(
            It.Is<string>(s => s.Contains("{ex}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString()!.Contains(exceptionMessage)))
        ), Times.Once);

        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetBranchByIdAsync_WhenBranchNotFound_ReturnFailure()
    {
        var branchId = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.Branch.GetByIdAsync(branchId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        var result = await _service.GetBranchByIdAsync(branchId);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Branch not found.");

        _loggerMock.Verify(l => l.LogWarn(
            It.Is<string>(s => s.Contains("{branchId}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString()!.Contains($"{branchId}")))
        ), Times.Once
        );
    }

    [Fact]
    public async Task GetBranchByIdAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        string exceptionMessage = "Db Error.";
        var branchId = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.Branch.GetByIdAsync(branchId, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.GetBranchByIdAsync(branchId);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(
            It.Is<string>(s => s.Contains("{ex}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString()!.Contains(exceptionMessage)))
        ), Times.Once);
    }

    [Fact]
    public async Task GetBranchByIdAsync_WhenBranchExists_ReturnSuccess()
    {
        var branchId = Guid.NewGuid();
        var branch = new Branch()
        {
            Id = branchId,
            Name = "Test Name",
            Address = "Test Address"
        };

        _repositoryMock.Setup(repo => repo.Branch.GetByIdAsync(branchId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);

        var result = await _service.GetBranchByIdAsync(branchId);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Branch fetched successfully.");

        _loggerMock.Verify(l => l.LogInfo(
            It.Is<string>(s => s.Contains("{branchId}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString()!.Contains($"{branchId}")))
        ), Times.Once
        );
    }

    [Fact]
    public async Task GetBranchDetailsByIdAsync_WhenBranchNotFound_ReturnFailure()
    {
        var branchId = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.Branch.GetBranchWithDetailsAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        var result = await _service.GetBranchDetailsByIdAsync(branchId);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Branch not found.");

        _loggerMock.Verify(l => l.LogWarn(
            It.Is<string>(s => s.Contains("{branchId}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString()!.Contains($"{branchId}")))
        ), Times.Once
        );
    }

    [Fact]
    public async Task GetBranchDetailsByIdAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        string exceptionMessage = "Db Error.";
        var branchId = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.Branch.GetBranchWithDetailsAsync(branchId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.GetBranchDetailsByIdAsync(branchId);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(
            It.Is<string>(s => s.Contains("{ex}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString()!.Contains(exceptionMessage)))
        ), Times.Once);
    }

    [Fact]
    public async Task GetBranchDetailsByIdAsync_WhenBranchExists_ReturnSuccess()
    {
        var branchId = Guid.NewGuid();
        var branch = new Branch()
        {
            Id = branchId,
            Name = "Test Name",
            Address = "Test Address",
            Users = [
                new() { Id = Guid.NewGuid(), BranchId = branchId, Name = "Abdo Othman", Email = "abdo@gmail.com", UserName = "abdo_hatem", Branch = new() { Id = branchId, Name = "Test Name", Address = "Test Address" }, Roles = [] }
            ],
            BranchProducts = [],
            Orders = [],
            StockMovements = []
        };

        _repositoryMock.Setup(repo => repo.Branch.GetBranchWithDetailsAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);

        var result = await _service.GetBranchDetailsByIdAsync(branchId);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Branch details fetched successfully.");
        result.Data!.Users.Should().NotBeNull();

        _loggerMock.Verify(l => l.LogInfo(
            It.Is<string>(s => s.Contains("{branchId}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString()!.Contains($"{branchId}")))
        ), Times.Once
        );
    }

    [Fact]
    public async Task GetAllBranchesAsync_WhenExceptionOccurs_ReturnFailureAndLogsError()
    {
        string exceptionMessage = "Db Error";

        _repositoryMock.Setup(repo => repo.Branch.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.GetAllBranchesAsync();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(
            It.Is<string>(s => s.Contains("{ex}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString()!.Contains(exceptionMessage)))
        ), Times.Once);
    }

    [Fact]
    public async Task GetAllBranchesAsync_WhenBranchesExist_ReturnSuccess()
    {
        var branches = new List<Branch>
        {
            new() {Id = Guid.NewGuid(), Name = "Branch #1", Address = "Address #1"},
            new() {Id = Guid.NewGuid(), Name = "Branch #2", Address = "Address #2"}
        };
        int count = 2;

        _repositoryMock.Setup(repo => repo.Branch.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(branches);

        var result = await _service.GetAllBranchesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("All branches fetched successfully.");

        _loggerMock.Verify(l => l.LogInfo(
            It.Is<string>(s => s.Contains("{count}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString()!.Contains(count.ToString())))
        ), Times.Once);
    }

    [Fact]
    public async Task UpdateBranchAsync_WhenBranchNotFound_ReturnFailure()
    {
        Guid branchId = Guid.NewGuid();
        UpdateBranchDto branchDto = new()
        {
            Name = "Branch",
            Address = "Address"
        };

        _repositoryMock.Setup(repo => repo.Branch.GetByIdAsync(branchId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        var result = await _service.UpdateBranchAsync(branchId, branchDto);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Branch not found.");

        _loggerMock.Verify(l => l.LogWarn(
            It.Is<string>(s => s.Contains("{branchId}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString()!.Contains($"{branchId}")))
        ), Times.Once
        );
    }

    [Fact]
    public async Task UpdateBranchAsync_WhenPartialUpdate_UpdatesChangedFieldsOnlyAndSaves()
    {
        var branchId = Guid.NewGuid();
        var existingBranch = new Branch { Name = "Old Name", Address = "Old Address" };
        var updateDto = new UpdateBranchDto { Name = "New Name", Address = null };

        _repositoryMock.Setup(repo => repo.Branch.GetByIdAsync(branchId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBranch);

        await _service.UpdateBranchAsync(branchId, updateDto);

        existingBranch.Name.Should().Be("New Name");
        existingBranch.Address.Should().Be("Old Address");

        _repositoryMock.Verify(repo => repo.Branch.Update(It.IsAny<Branch>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBranchAsync_WhenBranchExists_UpdateFiledsAndSave()
    {
        var branchId = Guid.NewGuid();
        var existingBranch = new Branch { Name = "Old Name", Address = "Old Address" };
        var updateDto = new UpdateBranchDto { Name = "New Name", Address = "New Address" };

        _repositoryMock.Setup(repo => repo.Branch.GetByIdAsync(branchId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBranch);

        var result = await _service.UpdateBranchAsync(branchId, updateDto);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("New Name");
        result.Data!.Address.Should().Be("New Address");

        _repositoryMock.Verify(repo => repo.Branch.Update(It.Is<Branch>(b => b.Name == "New Name")), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBranchAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        string exceptionMessage = "Db Error";
        Guid branchId = Guid.NewGuid();
        UpdateBranchDto branchDto = new()
        {
            Name = "Branch",
            Address = "Address"
        };

        _repositoryMock.Setup(repo => repo.Branch.GetByIdAsync(branchId, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.UpdateBranchAsync(branchId, branchDto);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));

        _loggerMock.Verify(l => l.LogError(
            It.Is<string>(s => s.Contains("{ex}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString()!.Contains(exceptionMessage)))
        ), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteBranchAsync_WhenBranchNotFound_ReturnFailure()
    {
        Guid branchId = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.Branch.GetByIdAsync(branchId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        var result = await _service.DeleteBranchAsync(branchId);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Branch not found.");

        _loggerMock.Verify(l => l.LogWarn(
            It.Is<string>(s => s.Contains("{branchId}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString()!.Contains($"{branchId}")))
        ), Times.Once
        );
    }

    [Fact]
    public async Task DeleteBranchAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        string exceptionMessage = "Db Error";
        Guid branchId = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.Branch.GetByIdAsync(branchId, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.DeleteBranchAsync(branchId);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));

        _loggerMock.Verify(l => l.LogError(
            It.Is<string>(s => s.Contains("{ex}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString()!.Contains(exceptionMessage)))
        ), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteBranchAsync_WhenBranchExists_DeleteAndSave()
    {
        var branchId = Guid.NewGuid();
        var existingBranch = new Branch
        {
            Id = branchId,
            Name = "Name",
            Address = "Address"
        };

        _repositoryMock.Setup(repo => repo.Branch.GetByIdAsync(branchId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBranch);

        await _service.DeleteBranchAsync(branchId);

        _repositoryMock.Verify(repo => repo.Branch.Remove(It.IsAny<Branch>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveAsync(), Times.Once);

        _loggerMock.Verify(l => l.LogInfo(
            It.Is<string>(s => s.Contains("{branchId}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString()!.Contains(branchId.ToString())))
            ), Times.Once);
    }
}
