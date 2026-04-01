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

    public Guid UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }

    public string Name => _httpContextAccessor.HttpContext?.User?.FindFirst("Name")?.Value ?? "User Name";
    public Guid BranchId => Guid.Parse(_httpContextAccessor.HttpContext?.User?.FindFirst("BranchId")?.Value ?? "0");
}
