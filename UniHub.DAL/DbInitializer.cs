using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniHub.BL.Entities;

namespace UniHub.DAL
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DbInitializer");

            using var context = new UniHubDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<UniHubDbContext>>());

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Ensure database is created
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migration completed.");

            // Seed Roles
            string[] roleNames = { "Student", "DepartmentAdmin", "SuperAdmin" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInformation($"Role '{roleName}' created.");
                }
            }

            // Create a super admin user if it doesn't exist
            var superAdminEmail = "admin@enit.utm.tn";
            var existingUser = await userManager.FindByEmailAsync(superAdminEmail);

            if (existingUser == null)
            {
                logger.LogInformation("Creating Super Admin user...");
                var superAdmin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = superAdminEmail,
                    EmailConfirmed = true,
                    FirstName = "Super",
                    LastName = "Admin"
                };

                var result = await userManager.CreateAsync(superAdmin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                    logger.LogInformation("Super Admin user created successfully.");
                }
                else
                {
                    logger.LogError($"Failed to create Super Admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation("Super Admin user already exists.");
            }

            // Create department admins for each department
            var departmentAdmins = new[]
            {
                new { Email = "gi-admin@enit.utm.tn", Username = "gi-admin", FirstName = "Admin", LastName = "GI", Department = Department.GI },
                new { Email = "gc-admin@enit.utm.tn", Username = "gc-admin", FirstName = "Admin", LastName = "GC", Department = Department.GC },
                new { Email = "ge-admin@enit.utm.tn", Username = "ge-admin", FirstName = "Admin", LastName = "GE", Department = Department.GE },
                new { Email = "gm-admin@enit.utm.tn", Username = "gm-admin", FirstName = "Admin", LastName = "GM", Department = Department.GM },
                new { Email = "gtic-admin@enit.utm.tn", Username = "gtic-admin", FirstName = "Admin", LastName = "TIC", Department = Department.GTIC }
            };

            foreach (var adminData in departmentAdmins)
            {
                var existingDeptUser = await userManager.FindByEmailAsync(adminData.Email);

                if (existingDeptUser == null)
                {
                    logger.LogInformation($"Creating Department Admin for {adminData.Department}...");
                    var admin = new ApplicationUser
                    {
                        UserName = adminData.Username,
                        Email = adminData.Email,
                        EmailConfirmed = true,
                        FirstName = adminData.FirstName,
                        LastName = adminData.LastName,
                        Department = adminData.Department
                    };

                    var result = await userManager.CreateAsync(admin, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(admin, "DepartmentAdmin");
                        logger.LogInformation($"Department Admin for {adminData.Department} created successfully.");
                    }
                    else
                    {
                        logger.LogError($"Failed to create Department Admin for {adminData.Department}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogInformation($"Department Admin for {adminData.Department} already exists.");
                }
            }

            logger.LogInformation("Database initialization completed.");
        }
    }
}
