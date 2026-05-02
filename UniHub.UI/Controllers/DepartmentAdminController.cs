using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniHub.DAL;
using UniHub.BL.Entities;
using UniHub.UI.ViewModels;

namespace UniHub.UI.Controllers
{
    [Authorize(Roles = "DepartmentAdmin")]
    public class DepartmentAdminController : Controller
    {
        private readonly UniHubDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DepartmentAdminController(UniHubDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard admin départemental
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.Department.HasValue)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            var myDepartmentActivities = _context.Activities != null 
                ? await _context.Activities
                    .Where(a => a.Department == user.Department)
                    .Include(a => a.CreatedByUser)
                    .OrderByDescending(a => a.StartDate)
                    .ToListAsync()
                : new List<Activity>();

            ViewBag.UserDepartment = user.Department;
            return View(myDepartmentActivities);
        }

        // GET: Créer une activité
        public IActionResult CreateActivity()
        {
            return View();
        }

        // POST: Créer une activité
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateActivity(ActivityViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.Department.HasValue)
            {
                ModelState.AddModelError("", "Vous devez être affecté à un département.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var activity = new Activity
                {
                    Title = model.Title,
                    Description = model.Description,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Location = model.Location,
                    MaxParticipants = model.MaxParticipants,
                    Department = user.Department.Value,
                    CreatedByUserId = user.Id,
                    CreatedDate = DateTime.Now,
                    IsPublished = true
                };

                _context.Activities?.Add(activity);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Activité créée avec succès.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Modifier une activité
        public async Task<IActionResult> EditActivity(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var activity = _context.Activities != null ? await _context.Activities.FindAsync(id) : null;

            if (activity == null || user == null || activity.Department != user.Department)
            {
                return NotFound();
            }

            var model = new ActivityViewModel
            {
                Id = activity.Id,
                Title = activity.Title,
                Description = activity.Description,
                StartDate = activity.StartDate,
                EndDate = activity.EndDate,
                Location = activity.Location,
                MaxParticipants = activity.MaxParticipants
            };

            return View(model);
        }

        // POST: Modifier une activité
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditActivity(ActivityViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var activity = _context.Activities != null ? await _context.Activities.FindAsync(model.Id) : null;

            if (activity == null || user == null || activity.Department != user.Department)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                activity.Title = model.Title;
                activity.Description = model.Description;
                activity.StartDate = model.StartDate;
                activity.EndDate = model.EndDate;
                activity.Location = model.Location;
                activity.MaxParticipants = model.MaxParticipants;
                activity.ModifiedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Activité modifiée avec succès.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: Supprimer une activité
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var activity = _context.Activities != null ? await _context.Activities.FindAsync(id) : null;

            if (activity == null || user == null || activity.Department != user.Department)
            {
                return NotFound();
            }

            _context.Activities?.Remove(activity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Activité supprimée avec succès.";
            return RedirectToAction(nameof(Index));
        }
    }
}
