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
using CRMS.Services;
using System.Security.Claims;

namespace CRMS.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IActivityLogService _activityLogService;

        public TeamController(
            AppDbContext context, 
            UserManager<Users> userManager,
            IActivityLogService activityLogService)
        {
            _context = context;
            _userManager = userManager;
            _activityLogService = activityLogService;
        }

        // Show Team Creation Form
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            // Log the activity
            _activityLogService.LogActivityAsync(
                "View",
                "TeamCreateForm",
                "New",
                "Viewed team creation form"
            ).Wait();

            return View(new TeamCreateModel());
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
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var team = await _context.Teams
                    .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)
                    .Include(t => t.TeamLeader)
                    .Include(t => t.CaseTeams)
                    .ThenInclude(ct => ct.Case)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (team == null)
                    return NotFound();

                // Check if user is team member or leader
                bool isTeamMember = team.TeamMembers.Any(tm => tm.UserId == currentUser.Id) || 
                                   team.TeamLeaderId == currentUser.Id;

                if (!isTeamMember)
                    return Forbid();

                ViewBag.TeamId = id;
                ViewBag.IsTeamLeader = team.TeamLeaderId == currentUser.Id;
                ViewBag.CurrentUserId = currentUser.Id;

                // Get all cases assigned to this team
                var assignedCases = team.CaseTeams
                    .Select(ct => new
                    {
                        ct.Case.Id,
                        ct.Case.Title,
                        ct.Case.Status,
                        ct.Case.Priority,
                        AssignedDate = ct.AssignedDate
                    })
                    .ToList();

                ViewBag.AssignedCases = assignedCases;

                // Log the activity
                await _activityLogService.LogActivityAsync(
                    "View",
                    "Team",
                    id.ToString(),
                    $"Viewed details of team '{team.Name}'"
                );

                return View(team);
            }
            catch (Exception ex)
            {
                // Log the error
                return RedirectToAction("Error", "Home");
            }
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
                .Include(t => t.CaseTeams)
                .ThenInclude(ct => ct.Case)
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

            // Get all cases assigned to this team
            var assignedCases = team.CaseTeams
                .Select(ct => ct.Case)
                .ToList();

            ViewBag.AvailableUsers = availableUsers;
            ViewBag.AssignedCases = assignedCases;

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "View",
                "TeamEditForm",
                id.ToString(),
                $"Viewed edit form for team '{team.Name}'"
            );

            return View(team);
        }

        // Handle Team Member Addition
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(Guid teamId, string userId)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Edit), new { id = teamId });
            }

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "User selection is required.";
                return RedirectToAction(nameof(Edit), new { id = teamId });
            }

            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
            {
                TempData["Error"] = "Team not found.";
                return NotFound();
            }

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (team.TeamLeaderId != currentUserId)
            {
                TempData["Error"] = "You are not authorized to modify this team.";
                return Forbid();
            }

            // Check if user exists
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Selected user not found.";
                return RedirectToAction(nameof(Edit), new { id = teamId });
            }

            // Check if user is already a member
            if (team.TeamMembers.Any(tm => tm.UserId == userId))
            {
                TempData["Error"] = "User is already a member of this team.";
                return RedirectToAction(nameof(Edit), new { id = teamId });
            }

            // Check if user is the team leader
            if (team.TeamLeaderId == userId)
            {
                TempData["Error"] = "Team leader cannot be added as a team member.";
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

                // Log the activity
                await _activityLogService.LogActivityAsync(
                    "Add",
                    "TeamMember",
                    teamMember.Id.ToString(),
                    $"Added member to team '{team.Name}'"
                );
            }
            catch (DbUpdateException ex)
            {
                _context.Entry(team).State = EntityState.Detached;
                TempData["Error"] = "An error occurred while adding the team member. Please try again.";
                // Log the exception details here
            }
            catch (Exception ex)
            {
                _context.Entry(team).State = EntityState.Detached;
                TempData["Error"] = "An unexpected error occurred. Please try again.";
                // Log the exception details here
            }

            return RedirectToAction(nameof(Edit), new { id = teamId });
        }

        // Handle Team Member Removal
        [HttpPost]
        public async Task<IActionResult> RemoveMember(Guid teamId, string userId)
        {
            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
                return NotFound();

            if (team.TeamLeaderId != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
                return Forbid();

            var teamMember = await _context.TeamMembers
                .Include(tm => tm.User)
                .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

            if (teamMember != null)
            {
                _context.TeamMembers.Remove(teamMember);
                await _context.SaveChangesAsync();

                // Log the activity
                await _activityLogService.LogActivityAsync(
                    "Remove",
                    "TeamMember",
                    teamMember.Id.ToString(),
                    $"Removed member from team '{team.Name}'"
                );
            }

            return RedirectToAction(nameof(Edit), new { id = teamId });
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeamCreateModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Generate a unique 8-character team code
                string teamCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

                var team = new Team
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    TeamCode = teamCode,
                    TeamLeaderId = user.Id,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                // Log the activity
                await _activityLogService.LogActivityAsync(
                    "Create",
                    "Team",
                    team.Id.ToString(),
                    $"Created new team '{team.Name}' with team code '{teamCode}'"
                );

                TempData["ToastMessage"] = $"Successfully created team: {team.Name}";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Details), new { id = team.Id });
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, Team team)
        {
            if (id != team.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(team);
                    await _context.SaveChangesAsync();

                    // Log the activity
                    await _activityLogService.LogActivityAsync(
                        "Update",
                        "Team",
                        User.FindFirstValue(ClaimTypes.NameIdentifier),
                        $"Updated team: {team.Name}"
                    );

                    TempData["ToastMessage"] = $"Successfully updated team: {team.Name}";
                    TempData["ToastType"] = "success";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TeamExists(team.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        [HttpPost]
        public async Task<IActionResult> JoinTeam(string teamCode)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.TeamCode == teamCode);
            if (team == null)
            {
                TempData["ToastMessage"] = "Invalid team code";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingMember = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamId == team.Id && tm.UserId == userId);

            if (existingMember != null)
            {
                TempData["ToastMessage"] = "You are already a member of this team";
                TempData["ToastType"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            var teamMember = new TeamMember
            {
                TeamId = team.Id,
                UserId = userId,
                JoinDate = DateTime.UtcNow
            };

            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "Join",
                "Team",
                userId,
                $"Joined team: {team.Name}"
            );

            TempData["ToastMessage"] = $"Successfully joined team: {team.Name}";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> TeamExists(Guid id)
        {
            return await _context.Teams.AnyAsync(e => e.Id == id);
        }
    }
}
