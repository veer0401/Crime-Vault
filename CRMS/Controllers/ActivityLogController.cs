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
    [Route("[controller]")]
    public class ActivityLogController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public ActivityLogController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index(string searchTerm = "", string actionType = "", string entityType = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.ActivityLogs
                .Include(al => al.User)
                .AsQueryable();

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

            var logs = await query
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();

            ViewBag.ActionTypes = await _context.ActivityLogs
                .Select(al => al.Action)
                .Distinct()
                .ToListAsync();

            ViewBag.EntityTypes = await _context.ActivityLogs
                .Select(al => al.EntityType)
                .Distinct()
                .ToListAsync();

            return View(logs);
        }

        [HttpGet]
        [Route("Details/{id}")]
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