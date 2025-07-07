using AccountBalanceViewer.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AccountBalanceViewer.Persistence.Services
{
    public static class IdentitySeeder
    {
        public static async Task SeedUsersAndRolesAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (await userManager.FindByNameAsync("admin@test.com") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@test.com",
                    Email = "admin@test.com",
                    FirstName = "Admin",
                    LastName = "Test",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            if (await userManager.FindByNameAsync("user@test.com") == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "user@test.com",
                    Email = "user@test.com",
                    FirstName = "User",
                    LastName = "Test",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, "User123!");
                await userManager.AddToRoleAsync(user, "User");
            }
        }
    }
}
