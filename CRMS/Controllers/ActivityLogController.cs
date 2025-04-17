using CRMS.Data;
using CRMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRMS.Controllers
{
    [Authorize]
    public class ActivityLogController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public ActivityLogController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Route("ActivityLog")]
        [Route("ActivityLog/Index")]
        public async Task<IActionResult> Index(
            string searchTerm = "", 
            string actionType = "", 
            string entityType = "", 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            bool viewAll = false,
            int page = 1,
            int pageSize = 10)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.ActivityLogs
                .Include(al => al.User)
                .AsQueryable();

            // By default, show only logs from the last hour unless viewAll is true
            if (!viewAll && !startDate.HasValue)
            {
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);
                query = query.Where(al => al.Timestamp >= oneHourAgo);
            }

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(al => 
                    al.User.UserName.Contains(searchTerm) ||
                    al.Details.Contains(searchTerm) ||
                    al.EntityId.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(actionType))
            {
                query = query.Where(al => al.Action == actionType);
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                query = query.Where(al => al.EntityType == entityType);
            }

            if (startDate.HasValue)
            {
                query = query.Where(al => al.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(al => al.Timestamp <= endDate.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, totalPages));

            // Apply pagination
            var logs = await query
                .OrderByDescending(al => al.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.ActionTypes = await _context.ActivityLogs
                .Select(al => al.Action)
                .Distinct()
                .ToListAsync();

            ViewBag.EntityTypes = await _context.ActivityLogs
                .Select(al => al.EntityType)
                .Distinct()
                .ToListAsync();

            ViewBag.ViewAll = viewAll;
            ViewBag.TotalLogs = totalCount;
            ViewBag.RecentLogs = logs.Count;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(logs);
        }

        [Route("ActivityLog/Details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var log = await _context.ActivityLogs
                .Include(al => al.User)
                .FirstOrDefaultAsync(al => al.Id == id);

            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }
    }
} 