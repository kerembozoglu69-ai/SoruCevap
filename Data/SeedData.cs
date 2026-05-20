using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoruCevap_forum_.Models;

namespace SoruCevap_forum_.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Create Roles
            string[] roleNames = { "Admin", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create Admin User
            var adminEmail = "admin@forum.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Forum Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Create Seed Categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Matematik", Icon = "fa-calculator" },
                    new Category { Name = "Fizik", Icon = "fa-atom" },
                    new Category { Name = "Yazılım", Icon = "fa-code" },
                    new Category { Name = "Sanat", Icon = "fa-palette" },
                    new Category { Name = "Spor", Icon = "fa-football-ball" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
