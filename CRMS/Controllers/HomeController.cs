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

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var isSentinelPrime = user != null && await _userManager.IsInRoleAsync(user, "Sentinel Prime");

            // Get cases from last 6 months
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-60);
            var cases = _context.Cases
                .Where(c => c.CreatedDate >= sixMonthsAgo)
                .Select(c => new { c.CreatedDate, c.Status })
                .ToList();

            // Process cases in memory
            var caseStats = cases
                .GroupBy(c => new { c.CreatedDate.Year, c.CreatedDate.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Get case status distribution
            var caseStatusStats = cases
                .GroupBy(c => c.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Get criminals from last 6 months
            var criminals = _context.Criminal
                .Where(c => c.CreatedAt >= sixMonthsAgo)
                .Select(c => new { c.CreatedAt })
                .ToList();

            // Process criminals in memory
            var criminalStats = criminals
                .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Get user's team statistics
            var userTeamStats = new
            {
                TeamMemberOf = _context.TeamMembers.Count(tm => tm.UserId == user.Id),
                TotalTeams = _context.Teams.Count(t => t.TeamLeaderId == user.Id),
                TotalTeamLeader = _context.Teams.Count(t => t.TeamLeaderId == user.Id)
            };

            // Get user's case statistics
            var userCaseStats = new
            {
                AssignedCases = _context.CaseTeams
                    .Count(ct => ct.Team.TeamMembers.Any(tm => tm.UserId == user.Id) || 
                                ct.Team.TeamLeaderId == user.Id),
                CompletedCases = _context.CaseTeams
                    .Count(ct => (ct.Team.TeamMembers.Any(tm => tm.UserId == user.Id) || 
                                 ct.Team.TeamLeaderId == user.Id) && 
                                ct.Case.Status == "Closed")
            };

            if (isSentinelPrime)
            {
                // Get team performance data only for Sentinel Prime
                var teamPerformance = _context.Teams
                    .Select(t => new
                    {
                        TeamName = t.Name,
                        CasesSolved = t.CaseTeams.Count(ct => ct.Case.Status == "Closed"),
                        TotalCases = t.CaseTeams.Count
                    })
                    .ToList();
                ViewBag.TeamPerformance = teamPerformance;
            }

            ViewBag.CaseStats = caseStats;
            ViewBag.CriminalStats = criminalStats;
            ViewBag.CaseStatusStats = caseStatusStats;
            ViewBag.IsSentinelPrime = isSentinelPrime;
            ViewBag.UserTeamStats = userTeamStats;
            ViewBag.UserCaseStats = userCaseStats;

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
