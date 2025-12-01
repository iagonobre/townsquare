using Microsoft.AspNetCore.Identity;
using Townsquare.Models;

namespace Townsquare.Data;

public static class RoleSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Admin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var email = "admin@townsquare.com";

        if (await userManager.FindByEmailAsync(email) == null)
        {
            var admin = new ApplicationUser
            {
                FullName = "Administrator",
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}