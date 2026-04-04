using API.Extensions;
using DotNetEnv;
using Infrastructure.Data;
using Infrastructure.SeedData;
using Microsoft.EntityFrameworkCore;

Env.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureAllServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<InitialDataSeeder>();
    await seeder.SeedAsync();
}

app.MapControllers();
app.Run();
