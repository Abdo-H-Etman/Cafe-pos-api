using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Auth;
using Application.Services;
using Application.Services.Auth;
using Core.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Utilities;

public static class ServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IIngredientService, IngredientService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IStockMovementService, StockMovementService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IReservationService, ReservationService>();
    }
}