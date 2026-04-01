
namespace Application.Common;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Name { get; }
    Guid BranchId{ get; }
}
