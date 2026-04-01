using Application.Common;
using Application.Interfaces.Auth;
using Application.Services;
using Application.Services.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Utilities;

public static class ServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
    }
}