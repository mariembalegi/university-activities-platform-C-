using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniHub.DAL;
using UniHub.BL.Entities;
using UniHub.UI.ViewModels;

namespace UniHub.UI.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UniHubDbContext _context;

        public SuperAdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            UniHubDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            var stats = new DashboardStatsViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalStudents = (await _userManager.GetUsersInRoleAsync("Student")).Count,
                TotalDepartmentAdmins = (await _userManager.GetUsersInRoleAsync("DepartmentAdmin")).Count,
                TotalActivities = _context.Activities != null ? await _context.Activities.CountAsync() : 0
            };

            return View(stats);
        }

        // Liste des utilisateurs
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Department = user.Department,
                    Roles = roles.ToList()
                });
            }

            return View(userViewModels);
        }

        // GET: Créer un utilisateur
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: Créer un utilisateur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Role))
                {
                    ModelState.AddModelError(string.Empty, "Les champs obligatoires doivent être renseignés.");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email.Split('@')[0],
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Department = model.Department,
                    EmailConfirmed = true
                };

                // Gérer l'upload de la photo de profil
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    // Vérifier la taille (max 2MB)
                    if (model.ProfilePicture.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ProfilePicture", "La taille de l'image ne doit pas dépasser 2MB.");
                        return View(model);
                    }

                    // Convertir l'image en byte array
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ProfilePicture.CopyToAsync(memoryStream);
                        user.ProfilePicture = memoryStream.ToArray();
                    }
                }

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assigner le rôle
                    await _userManager.AddToRoleAsync(user, model.Role);
                    TempData["Success"] = "Utilisateur créé avec succès.";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Modifier un utilisateur
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Department = user.Department,
                Role = roles.FirstOrDefault() ?? "Student",
                ExistingProfilePicture = user.ProfilePicture
            };

            return View(model);
        }

        // POST: Modifier un utilisateur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Id) || string.IsNullOrEmpty(model.Role))
                {
                    ModelState.AddModelError(string.Empty, "Les champs obligatoires doivent être renseignés.");
                    return View(model);
                }

                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Department = model.Department;
                user.Email = model.Email;

                // Gérer l'upload de la nouvelle photo de profil
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    // Vérifier la taille (max 2MB)
                    if (model.ProfilePicture.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ProfilePicture", "La taille de l'image ne doit pas dépasser 2MB.");
                        model.ExistingProfilePicture = user.ProfilePicture;
                        return View(model);
                    }

                    // Convertir l'image en byte array
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ProfilePicture.CopyToAsync(memoryStream);
                        user.ProfilePicture = memoryStream.ToArray();
                    }
                }

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Mettre à jour le rôle
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, model.Role);

                    TempData["Success"] = "Utilisateur modifié avec succès.";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // POST: Supprimer un utilisateur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Utilisateur supprimé avec succès.";
            }
            else
            {
                TempData["Error"] = "Erreur lors de la suppression de l'utilisateur.";
            }

            return RedirectToAction(nameof(Users));
        }
    }
}
