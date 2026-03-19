using DotNetEnv;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureAllServices(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureDatabase(services, configuration);
    }

    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString =
            $"Host={Env.GetString("DB_HOST")};"+
            $"Port={Env.GetString("DB_PORT")};"+
            $"Database={Env.GetString("DB_NAME")};"+
            $"Username={Env.GetString("DB_USER")};"+
            $"Password={Env.GetString("DB_PASSWORD")}";
        services.AddDbContextPool<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
}