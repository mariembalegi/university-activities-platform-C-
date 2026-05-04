using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniHub.DAL;
using UniHub.BL.Entities;

namespace UniHub.UI.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly UniHubDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(UniHubDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard étudiant
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.Department.HasValue)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            // Activités du département de l'étudiant
            var myDepartmentActivities = _context.Activities != null 
                ? await _context.Activities
                    .Where(a => a.Department == user.Department && a.IsPublished)
                    .Include(a => a.CreatedByUser)
                    .Include(a => a.Inscriptions)
                    .OrderByDescending(a => a.StartDate)
                    .ToListAsync()
                : new List<Activity>();

            // Activités des autres départements
            var otherActivities = _context.Activities != null 
                ? await _context.Activities
                    .Where(a => a.Department != user.Department && a.IsPublished)
                    .Include(a => a.CreatedByUser)
                    .Include(a => a.Inscriptions)
                    .OrderByDescending(a => a.StartDate)
                    .ToListAsync()
                : new List<Activity>();

            // Récupérer les IDs des activités auxquelles l'étudiant est inscrit
            var userInscriptions = _context.Inscriptions != null
                ? await _context.Inscriptions
                    .Where(i => i.UserId == user.Id)
                    .Select(i => i.ActivityId)
                    .ToListAsync()
                : new List<int>();

            ViewBag.MyDepartmentActivities = myDepartmentActivities;
            ViewBag.OtherActivities = otherActivities;
            ViewBag.UserDepartment = user.Department;
            ViewBag.UserInscriptions = userInscriptions;

            return View();
        }

        // POST: S'inscrire à une activité
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(int activityId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            var activity = _context.Activities != null 
                ? await _context.Activities
                    .Include(a => a.Inscriptions)
                    .FirstOrDefaultAsync(a => a.Id == activityId)
                : null;

            if (activity == null)
            {
                TempData["Error"] = "Activité introuvable.";
                return RedirectToAction(nameof(Index));
            }

            // Vérifier si l'étudiant est déjà inscrit
            var existingInscription = _context.Inscriptions != null
                ? await _context.Inscriptions
                    .FirstOrDefaultAsync(i => i.ActivityId == activityId && i.UserId == user.Id)
                : null;

            if (existingInscription != null)
            {
                TempData["Error"] = "Vous êtes déjà inscrit à cette activité.";
                return RedirectToAction(nameof(Index));
            }

            // Vérifier si le nombre maximum de participants est atteint
            if (activity.MaxParticipants.HasValue && activity.Inscriptions.Count >= activity.MaxParticipants.Value)
            {
                TempData["Error"] = "Le nombre maximum de participants est atteint.";
                return RedirectToAction(nameof(Index));
            }

            // Créer l'inscription
            var inscription = new Inscription
            {
                ActivityId = activityId,
                UserId = user.Id,
                InscriptionDate = DateTime.Now,
                IsConfirmed = true
            };

            _context.Inscriptions?.Add(inscription);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Inscription réussie !";
            return RedirectToAction(nameof(Index));
        }

        // POST: Se désinscrire d'une activité
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unsubscribe(int activityId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            var inscription = _context.Inscriptions != null
                ? await _context.Inscriptions
                    .FirstOrDefaultAsync(i => i.ActivityId == activityId && i.UserId == user.Id)
                : null;

            if (inscription == null)
            {
                TempData["Error"] = "Inscription introuvable.";
                return RedirectToAction(nameof(Index));
            }

            _context.Inscriptions?.Remove(inscription);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Désinscription réussie !";
            return RedirectToAction(nameof(Index));
        }
    }
}
