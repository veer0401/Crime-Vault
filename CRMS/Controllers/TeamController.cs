using CRMS.Data;
using CRMS.Models;
using CRMS.Models.CreateModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CRMS.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public TeamController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Show Team Creation Form
        public IActionResult Create()
        {
            return View();
        }
        public IActionResult Join()
        {
            return View();
        }

        // Show teams where current user is the leader
        public async Task<IActionResult> LeadingTeams()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var teams = await _context.Teams
                .Include(t => t.TeamMembers)
                .Where(t => t.TeamLeaderId == userId)
                .ToListAsync();

            return View(teams);
        }

        // Show teams where current user is a member
        public async Task<IActionResult> MemberTeams()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var teams = await _context.Teams
                .Include(t => t.TeamMembers)
                .Include(t => t.TeamLeader)
                .Where(t => t.TeamMembers.Any(tm => tm.UserId == userId))
                .ToListAsync();

            return View(teams);
        }

        // Show Team Details Page
        public async Task<IActionResult> Details(Guid id)
        {
            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
                .Include(t => t.TeamLeader)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
                return NotFound();

            return View(team);
        }

        // Show all teams with optional filtering
        public async Task<IActionResult> AllTeams(string searchName)
        {
            var query = _context.Teams
                .Include(t => t.TeamMembers)
                .Include(t => t.TeamLeader)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(t => t.Name.Contains(searchName));
            }

            var teams = await query.ToListAsync();
            ViewBag.SearchName = searchName;
            return View(teams);
        }

        // Show Team Edit Form
        public async Task<IActionResult> Edit(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
                .Include(t => t.TeamLeader)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
                return NotFound();

            if (team.TeamLeaderId != userId)
                return Forbid();

            // Get the IDs of users who are already team members
            var teamMemberIds = team.TeamMembers.Select(tm => tm.UserId).ToList();
            teamMemberIds.Add(team.TeamLeaderId); // Add team leader to excluded list

            // Get available users by excluding existing team members
            var availableUsers = await _userManager.Users
                .Where(u => !teamMemberIds.Contains(u.Id))
                .ToListAsync();

            ViewBag.AvailableUsers = availableUsers;
            return View(team);
        }

        // Handle Team Member Addition
        [HttpPost]
        public async Task<IActionResult> AddMember(Guid teamId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "User selection is required.";
                return RedirectToAction(nameof(Edit), new { id = teamId });
            }

            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
                return NotFound();

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (team.TeamLeaderId != currentUserId)
                return Forbid();

            // Check if user exists
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Selected user not found.";
                return RedirectToAction(nameof(Edit), new { id = teamId });
            }

            // Check if user is already a member
            var existingMember = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);
            if (existingMember != null)
            {
                TempData["Error"] = "User is already a member of this team.";
                return RedirectToAction(nameof(Edit), new { id = teamId });
            }

            try
            {
                var teamMember = new TeamMember
                {
                    TeamId = teamId,
                    UserId = userId
                };

                _context.TeamMembers.Add(teamMember);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Team member added successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while adding the team member.";
                // Log the exception details here if needed
            }

            return RedirectToAction(nameof(Edit), new { id = teamId });
        }

        // Handle Team Member Removal
        [HttpPost]
        public async Task<IActionResult> RemoveMember(Guid teamId, string userId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
                return NotFound();

            if (team.TeamLeaderId != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
                return Forbid();

            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

            if (teamMember != null)
            {
                _context.TeamMembers.Remove(teamMember);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = teamId });
        }
    }
}
