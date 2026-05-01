using System.Linq.Expressions;
using Application.Common.Models;
using Application.DTOs.User;
using Application.Interfaces.Logging;
using Application.Services;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using FluentAssertions;
using Infrastructure.Repositories;
using Moq;

namespace Cafe_pos.UnitTests.Application;

public class UserServiceTests
{
    private readonly Mock<IRepositoryManager> _repositoryMock;
    private readonly Mock<ILoggerManager> _loggerMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _repositoryMock = new Mock<IRepositoryManager>();
        _loggerMock = new Mock<ILoggerManager>();
        _service = new UserService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "John Doe",
            Email = "john@example.com",
            UserName = "johndoe",
            BranchId = Guid.NewGuid(),
            Branch = new Branch { Name = "Main Branch" },
            Roles = new List<UserRole>
            {
                new UserRole { Role = new Role { Name = "Admin" } }
            }
        };

        _repositoryMock.Setup(repo => repo.User.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(userId);
        result.Data.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.User.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().BeEqualTo("User not found.");
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exceptionMessage = "Database error";
        _repositoryMock.Setup(repo => repo.User.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred") && e.Contains(exceptionMessage));
        _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains(exceptionMessage)), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_WhenUserExists_ReturnsSuccess()
    {
        // Arrange
        var username = "johndoe";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john@example.com",
            UserName = username,
            BranchId = Guid.NewGuid(),
            Branch = new Branch { Name = "Main Branch" },
            Roles = new List<UserRole>
            {
                new UserRole { Role = new Role { Name = "Admin" } }
            }
        };

        _repositoryMock.Setup(repo => repo.User.GetUserByUsernameAsync(username, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByUsernameAsync(username);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UserName.Should().Be(username);
    }

    [Fact]
    public async Task GetUserByUserNameAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        var userName = "johndoe";

        _repositoryMock.Setup(repo => repo.User.GetUserByUsernameAsync(userName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _service.GetUserByUsernameAsync(userName);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().BeEqualTo("User not found.");
    }

    [Fact]
    public async Task GetUserByUserNameAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        // Arrange
        var userName = "johndoe";
        var exceptionMessage = "Database error";
        _repositoryMock.Setup(repo => repo.User.GetUserByUsernameAsync(userName, It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _service.GetUserByUsernameAsync(userName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains(exceptionMessage)), It.IsAny<object[]>()), Times.Once);
    }


    [Fact]
    public async Task GetUserByEmailAsync_WhenUserExists_ReturnsSuccess()
    {
        // Arrange
        var email = "john@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = email,
            UserName = "johndoe",
            BranchId = Guid.NewGuid(),
            Branch = new Branch { Name = "Main Branch" },
            Roles = new List<UserRole>
            {
                new UserRole { Role = new Role { Name = "Admin" } }
            }
        };

        _repositoryMock.Setup(repo => repo.User.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByEmailAsync(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        var email = "john@example.com";

        _repositoryMock.Setup(repo => repo.User.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _service.GetUserByEmailAsync(email);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().BeEqualTo("User not found.");
    }

    [Fact]
    public async Task GetUserByEmailAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        // Arrange
        var email = "john@example.com";
        var exceptionMessage = "Database error";
        _repositoryMock.Setup(repo => repo.User.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _service.GetUserByEmailAsync(email);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains(exceptionMessage)), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedUsersAsync_WhenUsersExist_ReturnSuccess()
    {
        var usersFortests = GenerateUsersForTest();
        // Arrange
        var usersForTest = GenerateUsersForTest().ToList();
        int pageNumber = 1;
        int pageSize = 2;
        int totalCount = usersForTest.Count;
        var pagedUsers = usersForTest.Take(pageSize).ToList();

        _repositoryMock.Setup(repo => repo.User);
        _repositoryMock.Setup(repo => repo.User.GetPagedUsersAsync(
        pageNumber,
        pageSize,
        It.IsAny<Expression<Func<User, bool>>>(),
        null,
        null,
        It.IsAny<CancellationToken>()))
        .ReturnsAsync((pagedUsers, totalCount));

        // Act
        var result = await _service.GetPagedUsersAsync(pageNumber, pageSize);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(pageSize);
        result.MetaData.Should().NotBeNull();
        result.MetaData!.TotalCount.Should().Be(totalCount);
        result.MetaData.CurrentPage.Should().Be(pageNumber);
        result.MetaData.TotalPages.Should().Be((int)Math.Ceiling((double)totalCount / pageSize));
    }

    [Fact]
    public async Task GetPagedUsersAsync_WithBranchId_ReturnsFilteredResults()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var branchUsers = new List<User>
        {
            new User { Id = Guid.NewGuid(), Name = "Branch User 1", UserName = "bu", BranchId = branchId, Branch = new Branch {
            Name = "Branch 1" }
            },
            new User { Id = Guid.NewGuid(), Name = "Branch User 2", UserName = "branchuser", BranchId = branchId, Branch = new Branch {
            Name = "Branch 1" }
            }
        };
        int pageNumber = 1;
        int pageSize = 10;
        int totalCount = 2;

        _repositoryMock.Setup(repo => repo.User.GetPagedUsersAsync(
        pageNumber,
        pageSize,
        It.IsAny<Expression<Func<User, bool>>>(),
        branchId,
        null,
        It.IsAny<CancellationToken>()))
            .ReturnsAsync((branchUsers, totalCount));

        // Act
        var result = await _service.GetPagedUsersAsync(pageNumber, pageSize, branchId: branchId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(repo => repo.User.GetPagedUsersAsync(
        pageNumber,
        pageSize,
        It.IsAny<Expression<Func<User, bool>>>(),
        branchId,
        null,
        It.IsAny<CancellationToken>()), Times.Once);
        result.Data.Should().Match(u => u.All(u => u.BranchId == branchId));
    }

    [Fact]
    public async Task GetPagedUsersAsync_WhenExceptionOccures_ReturnFailureAndLogsError()
    {
        //Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string exceptionMessage = "Test Exception";


        _repositoryMock.Setup(repo => repo.User.GetPagedUsersAsync(pageNumber, pageSize))
            .ThrowsAsync(new Exception(exceptionMessage));
        //Act
        var result = await _service.GetPagedUsersAsync(pageNumber, pageSize);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred") && e.Contains(exceptionMessage));
        _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains(exceptionMessage)), It.IsAny<object[]>()), Times.Once);

    }

    [Fact]
    public async Task GetPagedUsersAsync_SearchPredicate_CorrectlyFiltersLocalData()
    {
        // Arrange
        var searchTerm = "Doe";
        Expression<Func<User, bool>> capturedPredicate = null!;

        _repositoryMock.Setup(repo => repo.User.GetPagedUsersAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<User, bool>>>(),
            null, null, It.IsAny<CancellationToken>()))
            .Callback<int, int, Expression<Func<User, bool>>, Guid?, string?, CancellationToken>(
                (p1, p2, pred, b, r, c) => capturedPredicate = pred)
            .ReturnsAsync((new List<User>(), 0));

        // Act
        await _service.GetPagedUsersAsync(1, 10, searchTerm: searchTerm);

        // Assert
        var testUser = new User { Name = "John Doe", Email = "test@test.com", UserName = "jd" };
        var compiled = capturedPredicate.Compile();

        compiled(testUser).Should().BeTrue();
    }

    [Fact]
    public async Task GetUsersByBranchIdAsync_WithValidBranchId_ReturnSuccess()
    {
        var branchId = Guid.NewGuid();
        var branch = new Branch { Name = "Branch #1" };
        var users = new List<User>
        {
            new() {
                Id = Guid.NewGuid(),
                Name = "Abdo Othman",
                UserName = "abo",
                BranchId = branchId,
                Branch = branch,
                Roles = [
                    new UserRole{
                        Role = new() { Name = "Admin" }
                    }
                ]
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Ady Othman",
                UserName = "ao",
                BranchId = branchId,
                Branch = branch,
                Roles = [
                    new UserRole{
                        Role = new() { Name = "Admin" }
                    }
                ]
            }
        };

        _repositoryMock.Setup(repo => repo.User.GetUsersByBranchIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _service.GetUsersByBranchIdAsync(branchId);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Match(us => us.All(u => u.BranchId == branchId));
    }

    [Fact]
    public async Task GetUsersByBranchIdAsync_WithBranchId_ReturnFailureAndLogsError()
    {
        string exceptionMessage = "Test exception";
        var branchId = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.User.GetUsersByBranchIdAsync(branchId))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.GetUsersByBranchIdAsync(branchId, It.IsAny<CancellationToken>());

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains(exceptionMessage)), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetUsersByRoleIdAsync_WithValidRoleId_ReturnSuccess()
    {
        var branchId = Guid.NewGuid();
        var branch = new Branch { Name = "Branch #1" };
        var roleId = Guid.NewGuid();
        var role = new Role { Id = roleId, Name = "Admin" };
        var users = new List<User>
        {
            new() {
                Id = Guid.NewGuid(),
                Name = "Abdo Othman",
                UserName = "abo",
                BranchId = branchId,
                Branch = branch,
                Roles = [
                    new UserRole{
                        RoleId = roleId,
                        Role = role
                    }
                ]
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Ady Othman",
                UserName = "ao",
                BranchId = branchId,
                Branch = branch,
                Roles = [
                    new UserRole{
                        RoleId = roleId,
                        Role = role
                    }
                ]
            }
        };

        _repositoryMock.Setup(repo => repo.User.GetUsersByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _service.GetUsersByRoleIdAsync(roleId);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Match(us => us.All(u => u.Roles.Contains("Admin")));
    }

    [Fact]
    public async Task GetUsersByRoleIdAsync_WithRoleId_ReturnFailureAndLogsError()
    {
        string exceptionMessage = "Test exception";
        var roleId = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.User.GetUsersByRoleIdAsync(roleId))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.GetUsersByRoleIdAsync(roleId, It.IsAny<CancellationToken>());

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains(exceptionMessage)), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserExists_UpdatesFieldsAndSaves()
    {
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            Name = "Old Name",
            Email = "old@test.com",
            UserName = "oldun",
            Branch = new Branch { Name = "B1" }
        };
        var updateDto = new UpdateUserDto
        {
            Name = "New Name",
            Email = "new@test.com",
            UserName = "newun"
        };

        _repositoryMock.Setup(repo => repo.User.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);

        var result = await _service.UpdateUserAsync(userId, updateDto);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("New Name");
        result.Data!.Email.Should().Be("new@test.com");

        _repositoryMock.Verify(repo => repo.User.Update(It.Is<User>(u => u.Name == "New Name"),
                    It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenPartialUpdate_UpdatesChangedFieldsOnlyAndSaves()
    {
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            Name = "Old Name",
            Email = "old@test.com",
            UserName = "oldun",
            Branch = new Branch { Name = "B1" }
        };
        var updateDto = new UpdateUserDto
        {
            Name = "Only New Name",
            Email = null,
            UserName = null
        };

        _repositoryMock.Setup(repo => repo.User.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);

        await _service.UpdateUserAsync(userId, updateDto);

        existingUser.Name.Should().Be("Only New Name");
        existingUser.Email.Should().Be("old@test.com");
        existingUser.UserName.Should().Be("oldun");
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.User.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _service.UpdateUserAsync(userId, new UpdateUserDto());

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User not found.");

        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        var userId = Guid.NewGuid();
        var exceptionMessage = "Database error";

        _repositoryMock.Setup(repo => repo.User.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.UpdateUserAsync(userId, new UpdateUserDto());

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(
            It.Is<string>(s => s.Contains("{message}")),
            It.Is<object[]>(objs => objs.Any(o => o.ToString() == exceptionMessage))
        ), Times.Once);

    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserExists_DeleteUserAndSaves()
    {
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            Name = "Old Name",
            Email = "old@test.com",
            Branch = new Branch { Name = "B1" }
        };
        _repositoryMock.Setup(repo => repo.User.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);

        var result = await _service.DeleteUserAsync(userId);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("User deleted successfully.");

        _repositoryMock.Verify(repo => repo.User.Delete(
            It.Is<User>(u => u.Name == "Old Name" && u.Email == "old@test.com"),
            It.IsAny<CancellationToken>()
        ), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.User.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

        var result = await _service.DeleteUserAsync(userId);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User not found.");

        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        var userId = Guid.NewGuid();
        var exceptionMessage = "Database error";

        _repositoryMock.Setup(repo => repo.User.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _service.DeleteUserAsync(userId);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("An error occurred"));
        _loggerMock.Verify(l => l.LogError(
            It.Is<string>(s => s.Contains(exceptionMessage)),
            It.IsAny<object[]>()
        ), Times.Once);

    }

    private IEnumerable<User> GenerateUsersForTest()
    {
        var branch1 = new Branch { Name = "Branch #1" };
        var branch2 = new Branch { Name = "Branch #2" };

        _repositoryMock.Setup(repo => repo.Branch.AddRangeAsync(new List<Branch> { branch1, branch2 }, It.IsAny<CancellationToken>()));

        var users = new List<User>
        {
            new() {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john@example.com",
                UserName = "johndoe",
                BranchId = branch1.Id,
                Branch = branch1,
                Roles =
                [
                    new UserRole { Role = new Role { Name = "Admin" } }
                ]
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Abdo Hatem",
                Email = "abdo@example.com",
                UserName = "abdohatem",
                BranchId = branch1.Id,
                Branch = branch1,
                Roles =
                [
                    new UserRole { Role = new Role { Name = "Manager" } }
                ]
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Mohamed Hatem",
                Email = "mohamed@example.com",
                UserName = "mohamedhatem",
                BranchId = branch1.Id,
                Branch = branch1,
                Roles =
                [
                    new UserRole { Role = new Role { Name = "Manager" } }
                ]
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Ady Hatem",
                Email = "ady@example.com",
                UserName = "adyhatem",
                BranchId = branch1.Id,
                Branch = branch1,
                Roles =
                [
                    new UserRole { Role = new Role { Name = "Manager" } }
                ]
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Ebraheem Ahmed",
                Email = "ebraheem@example.com",
                UserName = "ebraheemahmed",
                BranchId = branch2.Id,
                Branch = branch2,
                Roles =
                [
                    new UserRole { Role = new Role { Name = "Admin" } }
                ]
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Akrm Faheem",
                Email = "akrm@example.com",
                UserName = "akrmfaheem",
                BranchId = branch2.Id,
                Branch = branch2,
                Roles =
                [
                    new UserRole { Role = new Role { Name = "Manager" } }
                ]
            }
        };

        return users;
    }
}
