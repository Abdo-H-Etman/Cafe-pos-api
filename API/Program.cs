using API.Extensions;
using DotNetEnv;

Env.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureAllServices(builder.Configuration);
var app = builder.Build();

app.Run();
