using Application.Common.Models;
using Application.DTOs.User;
using Application.Interfaces.Logging;
using Core.Application.DTOs.Branch;
using Core.Application.DTOs.BranchProduct;
using Core.Application.DTOs.Order;
using Core.Application.DTOs.Stock;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Interfaces;

namespace Application.Services;

public class BranchService : IBranchService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly ILoggerManager _logger;

    public BranchService(IRepositoryManager repositoryManager, ILoggerManager logger)
    {
        _repositoryManager = repositoryManager;
        _logger = logger;
    }

    public async Task<Result<BranchDto>> CreateBranchAsync(CreateBranchDto createBranchDto, CancellationToken cancellationToken = default)
    {
        try
        {
            Branch? existingBranch = null;
            existingBranch = await _repositoryManager.Branch
                    .FirstOrDefaultAsync(
                        b => b.Name == createBranchDto.Name || b.Address == createBranchDto.Address, cancellationToken);
            if (existingBranch != null)
            {
                _logger.LogWarn("Attempt to create branch with duplicate name or address: {branchName}", createBranchDto.Name);
                return Result<BranchDto>.Failure("A branch with the same name or address already exists.");
            }

            var branch = new Branch
            {
                Id = Guid.NewGuid(),
                Name = createBranchDto.Name,
                Address = createBranchDto.Address
            };

            await _repositoryManager.Branch.AddAsync(branch, cancellationToken);
            await _repositoryManager.SaveAsync(cancellationToken);

            var result = MapToBranchDto(branch);

            _logger.LogInfo("Branch created successfully with ID: {branchId}", result.Id);
            return Result<BranchDto>.Success(result, "Branch created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in {CreateBranchAsync}: {ex}", nameof(CreateBranchAsync), ex);
            return Result<BranchDto>.Failure($"An error occurred while creating the branch.");
        }
    }

    public async Task<Result<BranchDto>> GetBranchByIdAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        try
        {
            var branch = await _repositoryManager.Branch.GetByIdAsync(branchId, cancellationToken);
            if (branch == null)
            {
                _logger.LogWarn("Branch not found with ID: {branchId}", branchId);
                return Result<BranchDto>.Failure("Branch not found.");
            }

            var result = MapToBranchDto(branch);

            _logger.LogInfo("Branch fetched successfully with ID: {branchId}", result.Id);
            return Result<BranchDto>.Success(result, "Branch fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in {GetBranchByIdAsync}: {ex}", nameof(GetBranchByIdAsync), ex);
            return Result<BranchDto>.Failure($"An error occurred while fetching the branch.");
        }
    }

    public async Task<Result<BranchDetailsDto>> GetBranchDetailsByIdAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        try
        {
            var branch = await _repositoryManager.Branch.GetBranchWithDetailsAsync(branchId, cancellationToken);
            if (branch == null)
            {
                _logger.LogWarn("Branch not found with ID: {branchId}", branchId);
                return Result<BranchDetailsDto>.Failure("Branch not found.");
            }
            var result = MapToBranchDetailsDto(branch);

            _logger.LogInfo("Branch details fetched successfully with ID: {branchId}", result.Id);
            return Result<BranchDetailsDto>.Success(result, "Branch details fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in {GetBranchDetailsByIdAsync}: {ex}", nameof(GetBranchDetailsByIdAsync), ex);
            return Result<BranchDetailsDto>.Failure($"An error occurred while fetching the branch details.");
        }
    }
    
    public async Task<Result<IEnumerable<BranchDto>>> GetAllBranchesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var branches = await _repositoryManager.Branch.GetAllAsync(cancellationToken);
            var result = branches.Select(MapToBranchDto).ToList();

            _logger.LogInfo("All branches fetched successfully. Total branches: {count}", result.Count);
            return Result<IEnumerable<BranchDto>>.Success(result, "All branches fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in {GetAllBranchesAsync}: {ex}", nameof(GetAllBranchesAsync), ex);
            return Result<IEnumerable<BranchDto>>.Failure($"An error occurred while fetching all branches.");
        }
    }
    
    public async Task<Result<BranchDto>> UpdateBranchAsync(Guid branchId, UpdateBranchDto updateBranchDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var branch = await _repositoryManager.Branch.GetByIdAsync(branchId, cancellationToken);
            if (branch == null)
            {
                _logger.LogWarn("Branch not found with ID: {branchId}", branchId);
                return Result<BranchDto>.Failure("Branch not found.");
            }

            if(!string.IsNullOrEmpty(updateBranchDto.Name))
                branch.Name = updateBranchDto.Name;
            
            if(!string.IsNullOrEmpty(updateBranchDto.Address))
                branch.Address = updateBranchDto.Address;

            _repositoryManager.Branch.Update(branch);
            await _repositoryManager.SaveAsync(cancellationToken);

            var result = MapToBranchDto(branch);

            _logger.LogInfo("Branch updated successfully with ID: {branchId}", result.Id);
            return Result<BranchDto>.Success(result, "Branch updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in {UpdateBranchAsync}: {ex}", nameof(UpdateBranchAsync), ex);
            return Result<BranchDto>.Failure($"An error occurred while updating the branch.");
        }
    }

    public async Task<Result> DeleteBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        try
        {
            var branch = await _repositoryManager.Branch.GetByIdAsync(branchId, cancellationToken);
            if (branch == null)
            {
                _logger.LogWarn("Branch not found with ID: {branchId}", branchId);
                return Result.Failure("Branch not found.");
            }

            _repositoryManager.Branch.Remove(branch);
            await _repositoryManager.SaveAsync(cancellationToken);

            _logger.LogInfo("Branch deleted successfully with ID: {branchId}", branchId);
            return Result.Success("Branch deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in {DeleteBranchAsync}: {ex}", nameof(DeleteBranchAsync), ex);
            return Result.Failure($"An error occurred while deleting the branch.");
        }
    }



    private static BranchDto MapToBranchDto(Branch branch) =>
        new()
        {
            Id = branch.Id,
            Name = branch.Name,
            Address = branch.Address
        };
    
    private static BranchDetailsDto MapToBranchDetailsDto(Branch branch) =>
        new()
        {
            Id = branch.Id,
            Name = branch.Name,
            Address = branch.Address,
            Users = MapToUserDtos(branch.Users),
            BranchProducts = MapToBranchProductDtos(branch.BranchProducts),
            Orders = MapToOrderDtos(branch.Orders),
            StockMovements = MapToStockMovementDtos(branch.StockMovements)
        };
    
    private static IEnumerable<StockMovementDto> MapToStockMovementDtos(IEnumerable<StockMovement> stockMovements) =>
        [.. stockMovements.Select(sm => new StockMovementDto
        {
            Id = sm.Id,
            IngredientName = sm.Ingredient.Name,
            Quantity = sm.Quantity,
            MovementType = sm.Type.ToString(),
            Date = sm.CreatedAt
        })];
    
    private static IEnumerable<UserDetailsDto> MapToUserDtos(IEnumerable<User> users) =>
        [.. users.Select(u => new UserDetailsDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            UserName = u.UserName!,
            BranchId = u.BranchId,
            Branch = u.Branch.Name,
            Roles = u.Roles.Select(r => r.Role.Name)!,
            DateJoined = u.DateJoined
        })];

    private static IEnumerable<BranchProductDto> MapToBranchProductDtos(IEnumerable<BranchProduct> branchProducts) =>
        [.. branchProducts.Select(bp => new BranchProductDto
        {
            BranchId = bp.BranchId,
            ProductId = bp.ProductId,
            ProductName = bp.Product.Name,
            Price = bp.Price
        })];
    
    private static IEnumerable<OrderDto> MapToOrderDtos(IEnumerable<Order> orders) =>
        [.. orders.Select(o => new OrderDto
        {
            Id = o.Id,
            Cashier = o.Cashier.Name,
            TotalPrice = o.Total,
            Date = o.CreatedAt,
            Status = o.Status.ToString()
        })];
}