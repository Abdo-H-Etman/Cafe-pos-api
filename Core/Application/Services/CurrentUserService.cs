using System.Security.Claims;
using Application.Common;
using Microsoft.AspNetCore.Http;

namespace Application.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
    public Guid UserId
    {
        get
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }

    public Guid BranchId 
    { 
        get
        {
            var value = User?.FindFirst("BranchId");
            return Guid.TryParse(value?.Value, out var id) ? id : Guid.Empty;   
        }
    }

    public string? Name => User?.FindFirstValue("Name");
    public string? Role => User?.FindFirstValue(ClaimTypes.Role);
    public bool IsAdmin() => User?.IsInRole("Admin") ?? false;

    public bool IsManager() => User?.IsInRole("Manager") ?? false;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
