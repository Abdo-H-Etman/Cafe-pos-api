using System.Security.Claims;
using Application.DTOs.Auth;
using Application.DTOs.User;
using Application.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationController(IAuthenticationService authenticationService, IHttpContextAccessor httpContextAccessor)
    {
        _authenticationService = authenticationService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();

        var result = await _authenticationService.LoginAsync(request, ipAddress, userAgent);

        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin, Manager")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto request)
    {
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();

        var result = await _authenticationService.RegisterUserAsync(request, ipAddress, userAgent);

        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = GetCurrentUserId();
        var result = await _authenticationService.LogoutAsync(userId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken([FromBody] TokenDto request)
    {
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();

        var result = await _authenticationService.RefreshTokenAsync(request, ipAddress, userAgent);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] string refreshToken)
    {
        var ipAddress = GetIpAddress();

        var result = await _authenticationService.RevokeTokenAsync(refreshToken, ipAddress);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
    private string? GetIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
    }
}