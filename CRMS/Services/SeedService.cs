using CRMS.Data;
using CRMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CRMS.Services
{
    public class SeedService
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                logger.LogInformation("Applying Migrations...");
                await context.Database.MigrateAsync();  // Ensures all migrations are applied

                logger.LogInformation("Seeding Roles...");
                await AddRoleAsync(roleManager, "Sentinel Prime");
                await AddRoleAsync(roleManager, "Warden");
                await AddRoleAsync(roleManager, "Vanguard");
                await AddRoleAsync(roleManager, "Ghost");
                await AddRoleAsync(roleManager, "Cipher");
                await AddRoleAsync(roleManager, "Tracker");
                await AddRoleAsync(roleManager, "Cadet");

                logger.LogInformation("Seeding Admin User...");
                var adminEmail = "admin@gmail.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var adminUser = new Users
                    {
                        FullName = "Admin",
                        UserName = adminEmail,
                        Email = adminEmail,
                        NormalizedEmail = adminEmail.ToUpper(),
                        NormalizedUserName = adminEmail.ToUpper(),
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };
                    var result = await userManager.CreateAsync(adminUser, "Admin@123");

                    if (result.Succeeded)
                    {
                        logger.LogInformation("Admin user created successfully.");
                        await userManager.AddToRoleAsync(adminUser, "Sentinel Prime");
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogInformation("Admin user already exists. Skipping creation.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task AddRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));

                if (result.Succeeded)
                {
                    Console.WriteLine($"Role '{roleName}' added successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to add role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
