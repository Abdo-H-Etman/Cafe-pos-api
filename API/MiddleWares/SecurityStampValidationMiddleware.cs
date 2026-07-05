using System.Security.Claims;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;

public class SecurityStampValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityStampValidationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, UserManager<User> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tokenStamp = context.User.FindFirstValue("SecurityStamp");

            var user = await userManager.FindByIdAsync(userId!);
            if (user == null || user.SecurityStamp != tokenStamp)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await _next(context);
    }
}