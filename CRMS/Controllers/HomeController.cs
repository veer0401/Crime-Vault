using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using CRMS.Models;
using CRMS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CRMS.Services;

namespace CRMS.Controllers
{
    [Authorize]  // 🔒 This ensures only authenticated users can access this controller
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IActivityLogService _activityLogService;

        public HomeController(
            ILogger<HomeController> logger,
            AppDbContext context,
            UserManager<Users> userManager,
            IActivityLogService activityLogService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _activityLogService = activityLogService;
        }

        public IActionResult Index()
        {
            // Log the activity
            _activityLogService.LogActivityAsync(
                "View",
                "Home",
                "Anonymous",
                "Viewed home page"
            ).Wait();

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "View",
                "Dashboard",
                user.Id,
                "Viewed dashboard"
            );

            // Get user's teams
            var teams = await _context.Teams
                .Include(t => t.TeamMembers)
                .Include(t => t.CaseTeams)
                    .ThenInclude(ct => ct.Case)
                .Where(t => t.TeamLeaderId == user.Id || t.TeamMembers.Any(tm => tm.UserId == user.Id))
                .ToListAsync();

            // Get user's cases
            var cases = teams
                .SelectMany(t => t.CaseTeams)
                .Select(ct => ct.Case)
                .Distinct()
                .ToList();

            ViewBag.Teams = teams;
            ViewBag.Cases = cases;

            return View();
        }

        public IActionResult Privacy()
        {
            // Log the activity
            _activityLogService.LogActivityAsync(
                "View",
                "Privacy",
                "Anonymous",
                "Viewed privacy policy"
            ).Wait();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Log the activity
            _activityLogService.LogActivityAsync(
                "Error",
                "System",
                "Anonymous",
                "Encountered an error"
            ).Wait();

            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
