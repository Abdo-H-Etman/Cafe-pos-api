using System.Security.Claims;
using System.Text;
using Application.Common;
using Core.Application.Utilities;
using Core.Domain.Entities;
using DotNetEnv;
using Infrastructure.Data;
using Infrastructure.SeedData;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureAllServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureDatabase(configuration);
        services.ConfigureIdentity();
        services.ConfigureJwt(configuration);
        services.AddRepositories();
        services.AddApplicationServices();
        services.AddScoped<InitialDataSeeder>();
        services.AddControllers();
    }

    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var dbProvider = Env.GetString("DB_PROVIDER")?.ToLowerInvariant() ?? "postgres";

        if (dbProvider == "sqlite" || dbProvider == "sqlite3")
        {
            var dbName = Env.GetString("DB_NAME");
            if (string.IsNullOrWhiteSpace(dbName))
            {
                dbName = "cafe_pos.db";
            }

            services.AddDbContextPool<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbName}"));
            return;
        }

        string connectionString =
            $"Host={Env.GetString("DB_HOST")};"+
            $"Port={Env.GetString("DB_PORT")};"+
            $"Database={Env.GetString("DB_NAME")};"+
            $"Username={Env.GetString("DB_USER")};"+
            $"Password={Env.GetString("DB_PASSWORD")}";
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
    }

    public static void ConfigureIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
    }

    public static void ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecrestKey = Environment.GetEnvironmentVariable("JwtSettings_SECRET_KEY")
                ?? throw new InvalidOperationException("JWT Secret Key is not configured.");
        var jwtIssuer = Environment.GetEnvironmentVariable("JwtSettings_ISSUER")
                ?? throw new InvalidOperationException("JWT Issuer is not configured.");
        var jwtAudience = Environment.GetEnvironmentVariable("JwtSettings_AUDIENCE")
                ?? throw new InvalidOperationException("JWT Audience is not configured.");
        var jwtExpirationInMinutes = int.Parse(Environment.GetEnvironmentVariable("JwtSettings_EXPIRATION_MINUTES") ?? "60");
        var jwtRefreshTokenExpirationInDays = int.Parse(Environment.GetEnvironmentVariable("JwtSettings_REFRESH_TOKEN_EXPIRATION_DAYS") ?? "7");

        services.Configure<JwtSettings>(options =>
        {
            options.SecretKey = jwtSecrestKey;
            options.Issuer = jwtIssuer;
            options.Audience = jwtAudience;
            options.ExpirationInMinutes = jwtExpirationInMinutes;
            options.RefreshTokenExpirationInDays = jwtRefreshTokenExpirationInDays;
        });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecrestKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                    var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;

                    var securityStamp = claimsIdentity?.FindFirst("SecurityStamp")?.Value;
                    var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (string.IsNullOrEmpty(securityStamp) || string.IsNullOrEmpty(userId))
                    {
                        context.Fail("Token is missing required security claims.");
                        return;
                    }

                    var user = await userManager.FindByIdAsync(userId);

                    if (user == null)
                    {
                        context.Fail("User not found.");
                        return;
                    }

                    if (user.SecurityStamp != securityStamp)
                    {
                        context.Fail("Token is invalid (Security Stamp mismatch).");
                    }
                }
            };
        });

        services.AddAuthorization();
    }
}