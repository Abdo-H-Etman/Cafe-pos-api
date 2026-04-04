using Core.Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.SeedData;

public class InitialDataSeeder
{
    private readonly UserManager<User> _userManager;

    public InitialDataSeeder(
        UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        await SeedAdminUserAsync();
    }

    private async Task SeedAdminUserAsync()
    {
        // Check if admin user already exists
        var adminUser = await _userManager.FindByNameAsync("admin");
        if (adminUser == null)
        {
            // Get configuration or use defaults
            var adminEmail = Environment.GetEnvironmentVariable("DefaultAdminUser_Email");
            var adminPassword = Environment.GetEnvironmentVariable("DefaultAdminUser_Password");
            var adminUsername = Environment.GetEnvironmentVariable("DefaultAdminUser_Username");
            var branchId = new Guid("4acc688c-fec1-442a-9873-8d374079f097");

            // Create admin user
            var newAdmin = new User
            {
                Id = new Guid("00000000-0000-0000-0000-000000000001"),
                Name = "Admin User",
                UserName = adminUsername,
                Email = adminEmail,
                EmailConfirmed = true,
                BranchId = branchId,
                DateJoined = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(newAdmin, adminPassword!);
            
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            adminUser = newAdmin;
        }

        // Ensure the admin user has the Admin role
        if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await _userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
