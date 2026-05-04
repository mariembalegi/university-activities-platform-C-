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

            // Seed sample activities if none exist
            if (context.Activities != null && !context.Activities.Any())
            {
                logger.LogInformation("Creating sample activities...");

                var activities = new List<Activity>
                {
                    new Activity
                    {
                        Title = "Conférence sur l'Intelligence Artificielle",
                        Description = "Une conférence passionnante sur les dernières avancées en IA et machine learning. Intervenants de renommée internationale.",
                        StartDate = DateTime.Now.AddDays(7),
                        EndDate = DateTime.Now.AddDays(7).AddHours(3),
                        Location = "Amphithéâtre A",
                        MaxParticipants = 150,
                        Department = Department.GI,
                        CreatedByUserId = (await userManager.FindByEmailAsync("gi-admin@enit.utm.tn"))!.Id,
                        CreatedDate = DateTime.Now,
                        IsPublished = true
                    },
                    new Activity
                    {
                        Title = "Workshop Arduino & IoT",
                        Description = "Atelier pratique pour apprendre à créer des projets IoT avec Arduino. Matériel fourni sur place.",
                        StartDate = DateTime.Now.AddDays(10),
                        EndDate = DateTime.Now.AddDays(10).AddHours(4),
                        Location = "Laboratoire Électronique",
                        MaxParticipants = 30,
                        Department = Department.GE,
                        CreatedByUserId = (await userManager.FindByEmailAsync("ge-admin@enit.utm.tn"))!.Id,
                        CreatedDate = DateTime.Now,
                        IsPublished = true
                    },
                    new Activity
                    {
                        Title = "Visite de Chantier - Projet Urbain",
                        Description = "Visite guidée d'un grand chantier de construction avec les ingénieurs responsables du projet.",
                        StartDate = DateTime.Now.AddDays(5),
                        EndDate = DateTime.Now.AddDays(5).AddHours(3),
                        Location = "Centre-ville de Tunis",
                        MaxParticipants = 40,
                        Department = Department.GC,
                        CreatedByUserId = (await userManager.FindByEmailAsync("gc-admin@enit.utm.tn"))!.Id,
                        CreatedDate = DateTime.Now,
                        IsPublished = true
                    },
                    new Activity
                    {
                        Title = "Hackathon Innovation 2026",
                        Description = "24 heures de codage intense ! Formez vos équipes et relevez des défis technologiques avec des prix à la clé.",
                        StartDate = DateTime.Now.AddDays(14),
                        EndDate = DateTime.Now.AddDays(15),
                        Location = "Campus ENIT - Salle Polyvalente",
                        MaxParticipants = 100,
                        Department = Department.GTIC,
                        CreatedByUserId = (await userManager.FindByEmailAsync("gtic-admin@enit.utm.tn"))!.Id,
                        CreatedDate = DateTime.Now,
                        IsPublished = true
                    },
                    new Activity
                    {
                        Title = "Conférence Énergies Renouvelables",
                        Description = "Discussion sur l'avenir de l'énergie solaire et éolienne en Tunisie avec des experts du secteur.",
                        StartDate = DateTime.Now.AddDays(12),
                        EndDate = DateTime.Now.AddDays(12).AddHours(2),
                        Location = "Amphithéâtre B",
                        MaxParticipants = 120,
                        Department = Department.GE,
                        CreatedByUserId = (await userManager.FindByEmailAsync("ge-admin@enit.utm.tn"))!.Id,
                        CreatedDate = DateTime.Now,
                        IsPublished = true
                    },
                    new Activity
                    {
                        Title = "Atelier CAO/DAO - SolidWorks",
                        Description = "Formation pratique sur la conception assistée par ordinateur avec le logiciel SolidWorks.",
                        StartDate = DateTime.Now.AddDays(8),
                        EndDate = DateTime.Now.AddDays(8).AddHours(5),
                        Location = "Salle Informatique 102",
                        MaxParticipants = 25,
                        Department = Department.GM,
                        CreatedByUserId = (await userManager.FindByEmailAsync("gm-admin@enit.utm.tn"))!.Id,
                        CreatedDate = DateTime.Now,
                        IsPublished = true
                    },
                    new Activity
                    {
                        Title = "Journée Portes Ouvertes ENIT",
                        Description = "Découvrez les différents départements de l'ENIT, rencontrez les professeurs et visitez les laboratoires.",
                        StartDate = DateTime.Now.AddDays(3),
                        EndDate = DateTime.Now.AddDays(3).AddHours(6),
                        Location = "Campus ENIT",
                        MaxParticipants = null, // Pas de limite
                        Department = Department.GI,
                        CreatedByUserId = (await userManager.FindByEmailAsync("gi-admin@enit.utm.tn"))!.Id,
                        CreatedDate = DateTime.Now,
                        IsPublished = true
                    },
                    new Activity
                    {
                        Title = "Séminaire Gestion de Projet Agile",
                        Description = "Méthodologies Agile et Scrum pour la gestion de projets informatiques modernes.",
                        StartDate = DateTime.Now.AddDays(20),
                        EndDate = DateTime.Now.AddDays(20).AddHours(4),
                        Location = "Salle de Conférence",
                        MaxParticipants = 60,
                        Department = Department.GTIC,
                        CreatedByUserId = (await userManager.FindByEmailAsync("gtic-admin@enit.utm.tn"))!.Id,
                        CreatedDate = DateTime.Now,
                        IsPublished = true
                    },
                    new Activity
                    {
                        Title = "Concours de Robotique Mobile",
                        Description = "Compétition de robots autonomes. Inscrivez vos équipes pour participer au challenge !",
                        StartDate = DateTime.Now.AddDays(25),
                        EndDate = DateTime.Now.AddDays(25).AddHours(8),
                        Location = "Hall Principal",
                        MaxParticipants = 50,
                        Department = Department.GM,
                        CreatedByUserId = (await userManager.FindByEmailAsync("gm-admin@enit.utm.tn"))!.Id,
                        CreatedDate = DateTime.Now,
                        IsPublished = true
                    },
                    new Activity
                    {
                        Title = "Atelier Résistance des Matériaux",
                        Description = "TP pratique sur les essais de traction, compression et flexion des matériaux de construction.",
                        StartDate = DateTime.Now.AddDays(6),
                        EndDate = DateTime.Now.AddDays(6).AddHours(3),
                        Location = "Laboratoire Matériaux",
                        MaxParticipants = 20,
                        Department = Department.GC,
                        CreatedByUserId = (await userManager.FindByEmailAsync("gc-admin@enit.utm.tn"))!.Id,
                        CreatedDate = DateTime.Now,
                        IsPublished = true
                    }
                };

                context.Activities.AddRange(activities);
                await context.SaveChangesAsync();
                logger.LogInformation($"{activities.Count} sample activities created successfully.");
            }
            else
            {
                logger.LogInformation("Activities already exist, skipping seed.");
            }

            logger.LogInformation("Database initialization completed.");
        }
    }
}
