
namespace Application.Common;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid BranchId{ get; }
    string? Name { get; } 
    string? Role { get; }
    bool IsAdmin();
    bool IsManager();
    bool IsAuthenticated { get; }
}
