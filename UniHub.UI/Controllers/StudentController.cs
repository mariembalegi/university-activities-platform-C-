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
                    .OrderByDescending(a => a.StartDate)
                    .ToListAsync()
                : new List<Activity>();

            // Activités des autres départements
            var otherActivities = _context.Activities != null 
                ? await _context.Activities
                    .Where(a => a.Department != user.Department && a.IsPublished)
                    .Include(a => a.CreatedByUser)
                    .OrderByDescending(a => a.StartDate)
                    .ToListAsync()
                : new List<Activity>();

            ViewBag.MyDepartmentActivities = myDepartmentActivities;
            ViewBag.OtherActivities = otherActivities;
            ViewBag.UserDepartment = user.Department;

            return View();
        }
    }
}
