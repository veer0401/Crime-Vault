using CRMS.Data;
using CRMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CRMS.Services;

namespace CRMS.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class TeamMessageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IActivityLogService _activityLogService;

        public TeamMessageController(
            AppDbContext context, 
            UserManager<Users> userManager,
            IActivityLogService activityLogService)
        {
            _context = context;
            _userManager = userManager;
            _activityLogService = activityLogService;
        }

        [HttpGet]
        [Route("Index/{teamId?}")]
        public async Task<IActionResult> Index(Guid? teamId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get teams where user is a member
            var memberTeams = await _context.TeamMembers
                .Include(tm => tm.Team)
                    .ThenInclude(t => t.TeamLeader)
                .Include(tm => tm.Team.TeamMembers)
                .Where(tm => tm.UserId == currentUser.Id)
                .Select(tm => tm.Team)
                .ToListAsync();

            // Get teams where user is a leader
            var leaderTeams = await _context.Teams
                .Include(t => t.TeamLeader)
                .Include(t => t.TeamMembers)
                .Where(t => t.TeamLeaderId == currentUser.Id)
                .ToListAsync();

            // Combine and deduplicate teams
            var allTeams = memberTeams.Union(leaderTeams).Distinct().ToList();
            ViewBag.UserTeams = allTeams;
            ViewBag.SelectedTeamId = teamId;

            if (teamId.HasValue)
            {
                // Check if user is a member or leader of the selected team
                var isTeamMember = memberTeams.Any(t => t.Id == teamId);
                var isTeamLeader = leaderTeams.Any(t => t.Id == teamId);

                if (!isTeamMember && !isTeamLeader)
                {
                    return RedirectToAction("Index");
                }

                var messages = await _context.TeamMessages
                    .Include(m => m.Sender)
                    .Where(m => m.TeamId == teamId)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                // Log the activity
                await _activityLogService.LogActivityAsync(
                    "View",
                    "TeamMessages",
                    teamId.ToString(),
                    $"Viewed messages for team"
                );

                return View(messages);
            }

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "View",
                "TeamMessages",
                "All",
                "Viewed team messages list"
            );

            return View(new List<TeamMessage>());
        }

        [HttpPost]
        [Route("SendMessage")]
        public async Task<IActionResult> SendMessage(Guid teamId, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return Json(new { success = false, message = "Message content cannot be empty" });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }
                
                // Check if user is a member or leader of the team
                var isTeamMember = await _context.TeamMembers
                    .AnyAsync(tm => tm.TeamId == teamId && tm.UserId == currentUser.Id);
                var isTeamLeader = await _context.Teams
                    .AnyAsync(t => t.Id == teamId && t.TeamLeaderId == currentUser.Id);

                if (!isTeamMember && !isTeamLeader)
                {
                    return Json(new { success = false, message = "You are not a member of this team" });
                }

                var message = new TeamMessage
                {
                    Id = Guid.NewGuid(),
                    TeamId = teamId,
                    SenderId = currentUser.Id,
                    Content = content,
                    SentAt = DateTime.UtcNow
                };

                _context.TeamMessages.Add(message);
                await _context.SaveChangesAsync();

                // Log the activity
                await _activityLogService.LogActivityAsync(
                    "Send",
                    "TeamMessage",
                    message.Id.ToString(),
                    $"Sent message to team"
                );

                return Json(new { success = true, messageId = message.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("DeleteMessage/{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var message = await _context.TeamMessages
                    .Include(m => m.Team)
                    .FirstOrDefaultAsync(m => m.Id == messageId);

                if (message == null)
                {
                    return Json(new { success = false, message = "Message not found" });
                }

                // Check if user is the sender or team leader
                var isTeamLeader = message.Team.TeamLeaderId == currentUser.Id;
                var isSender = message.SenderId == currentUser.Id;

                if (!isTeamLeader && !isSender)
                {
                    return Json(new { success = false, message = "You are not authorized to delete this message" });
                }

                // Log the activity before deletion
                await _activityLogService.LogActivityAsync(
                    "Delete",
                    "TeamMessage",
                    message.Id.ToString(),
                    $"Deleted message from team"
                );

                _context.TeamMessages.Remove(message);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 