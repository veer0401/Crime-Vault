using CRMS.Data;
using CRMS.Models;
using CRMS.Models.CreateModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using CRMS.Services;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<Users> _userManager;
    private readonly IActivityLogService _activityLogService;

    public TeamApiController(
        AppDbContext context, 
        UserManager<Users> userManager,
        IActivityLogService activityLogService)
    {
        _context = context;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    // ✅ API to get users
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        await _activityLogService.LogActivityAsync(
            "View",
            "UserList",
            "All",
            "Viewed list of all users"
        );

        var users = await _userManager.Users
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync();
        return Ok(users);
    }

    // ✅ API to create a new team
    [HttpPost("create")]
    public async Task<IActionResult> CreateTeam([FromBody] TeamCreateModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized(new { success = false, message = "User not authenticated." });

        // Generate a unique 8-character team code
        string teamCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            TeamCode = teamCode,
            TeamLeaderId = user.Id
        };

        try
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "Create",
                "Team",
                team.Id.ToString(),
                $"Created new team '{team.Name}' with team code '{teamCode}'"
            );

            return CreatedAtAction(nameof(GetTeamDetails), new { id = team.Id }, 
                new { success = true, data = new { team.Id, team.Name, team.TeamCode, TeamLeader = user.FullName } });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while creating the team." });            
        }
    }

    // ✅ API to get team details with members
    [HttpGet("details/{id}")]
    public async Task<IActionResult> GetTeamDetails(Guid id)
    {
        try
        {
            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound(new { success = false, message = "Team not found." });
            }

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "View",
                "Team",
                team.Id.ToString(),
                $"Viewed details of team '{team.Name}'"
            );

            var teamLeader = await _userManager.FindByIdAsync(team.TeamLeaderId);
            string teamLeaderName = teamLeader != null ? teamLeader.FullName : "Unknown";

            var result = new
            {
                success = true,
                data = new
                {
                    team.Id,
                    team.Name,
                    team.TeamCode,
                    TeamLeader = teamLeaderName,
                    TeamLeaderId = team.TeamLeaderId,
                    Members = team.TeamMembers.Select(m => new 
                    { 
                        m.User.Id, 
                        m.User.FullName,
                        JoinDate = m.Id
                    }).ToList()
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while fetching team details." });
        }
    }

    // ✅ API to join a team using team code
    [HttpPost("join")]
    public async Task<IActionResult> JoinTeam([FromBody] string teamCode)
    {
        if (string.IsNullOrEmpty(teamCode))
            return BadRequest(new { success = false, message = "Team code is required." });

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized(new { success = false, message = "User not authenticated." });

        var team = await _context.Teams
            .FirstOrDefaultAsync(t => t.TeamCode == teamCode);

        if (team == null)
            return NotFound(new { success = false, message = "Invalid team code." });

        // Check if the user is already in the team
        bool isAlreadyMember = await _context.TeamMembers
            .AnyAsync(tm => tm.TeamId == team.Id && tm.UserId == user.Id);

        if (isAlreadyMember)
            return BadRequest(new { success = false, message = "You are already a member of this team." });

        // Add user to the team
        var teamMember = new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = team.Id,
            UserId = user.Id
        };

        try
        {
            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "Join",
                "Team",
                team.Id.ToString(),
                $"Joined team '{team.Name}' using team code"
            );

            return Ok(new { 
                success = true, 
                message = "Successfully joined the team.",
                data = new { TeamId = team.Id, TeamName = team.Name }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while joining the team." });
        }
    }

    // Get all cases assigned to a team
    [HttpGet("{teamId}/cases")]
    public async Task<IActionResult> GetTeamCases(Guid teamId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized(new { success = false, message = "User not authenticated." });

        var team = await _context.Teams
            .Include(t => t.TeamMembers)
            .Include(t => t.CaseTeams)
            .ThenInclude(ct => ct.Case)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
            return NotFound(new { success = false, message = "Team not found." });

        // Check if the user is the team leader or a team member
        bool isTeamMember = team.TeamMembers.Any(tm => tm.UserId == user.Id) || team.TeamLeaderId == user.Id;
        if (!isTeamMember)
            return Forbid();

        try
        {
            // Log the activity
            await _activityLogService.LogActivityAsync(
                "View",
                "TeamCases",
                teamId.ToString(),
                $"Viewed cases assigned to team '{team.Name}'"
            );

            var cases = team.CaseTeams
                .Select(ct => new
                {
                    ct.Case.Id,
                    ct.Case.Title,
                    ct.Case.Description,
                    ct.Case.Status,
                    ct.Case.Priority,
                    ct.Case.CreatedDate,
                    ct.Case.UpdatedDate,
                    ct.AssignedDate,
                    ct.Role
                })
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            return Ok(new { 
                success = true, 
                data = cases 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while fetching team cases." });
        }
    }

    // Assign a case to a team
    [HttpPost("{teamId}/cases/{caseId}")]
    public async Task<IActionResult> AssignCase(Guid teamId, Guid caseId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized(new { success = false, message = "User not authenticated." });

        var team = await _context.Teams
            .Include(t => t.TeamMembers)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
            return NotFound(new { success = false, message = "Team not found." });

        // Check if the user is the team leader or a team member
        bool isTeamMember = team.TeamMembers.Any(tm => tm.UserId == user.Id) || team.TeamLeaderId == user.Id;
        if (!isTeamMember)
            return Forbid();

        var case_ = await _context.Cases.FindAsync(caseId);
        if (case_ == null)
            return NotFound(new { success = false, message = "Case not found." });

        // Check if the case is already assigned to the team
        var existingAssignment = await _context.CaseTeams
            .FirstOrDefaultAsync(ct => ct.TeamId == teamId && ct.CaseId == caseId);

        if (existingAssignment != null)
            return BadRequest(new { success = false, message = "Case is already assigned to this team." });

        var caseTeam = new CaseTeam
        {
            TeamId = teamId,
            CaseId = caseId,
            AssignedDate = DateTime.UtcNow,
            Role = "Primary"
        };

        try
        {
            _context.CaseTeams.Add(caseTeam);
            await _context.SaveChangesAsync();

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "Assign",
                "Case",
                caseId.ToString(),
                $"Assigned case to team '{team.Name}'"
            );

            return Ok(new { 
                success = true, 
                message = "Case assigned successfully.",
                data = new { 
                    CaseId = caseId,
                    TeamId = teamId,
                    AssignedDate = caseTeam.AssignedDate
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while assigning the case." });
        }
    }

    // Remove a case from a team
    [HttpDelete("{teamId}/cases/{caseId}")]
    public async Task<IActionResult> RemoveCase(Guid teamId, Guid caseId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized(new { success = false, message = "User not authenticated." });

        var team = await _context.Teams
            .Include(t => t.TeamMembers)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
            return NotFound(new { success = false, message = "Team not found." });

        // Only team leader can remove cases
        if (team.TeamLeaderId != user.Id)
            return Forbid();

        var caseTeam = await _context.CaseTeams
            .FirstOrDefaultAsync(ct => ct.TeamId == teamId && ct.CaseId == caseId);

        if (caseTeam == null)
            return NotFound(new { success = false, message = "Case assignment not found." });

        try
        {
            _context.CaseTeams.Remove(caseTeam);
            await _context.SaveChangesAsync();

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "Remove",
                "Case",
                caseId.ToString(),
                $"Removed case from team '{team.Name}'"
            );

            return Ok(new { 
                success = true, 
                message = "Case removed from team successfully.",
                data = new { CaseId = caseId, TeamId = teamId }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while removing the case from team." });
        }
    }

    // Get team statistics including case information
    [HttpGet("{teamId}/statistics")]
    public async Task<IActionResult> GetTeamStatistics(Guid teamId)
    {
        var team = await _context.Teams
            .Include(t => t.TeamMembers)
            .Include(t => t.CaseTeams)
            .ThenInclude(ct => ct.Case)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
            return NotFound(new { message = "Team not found." });

        // Log the activity
        await _activityLogService.LogActivityAsync(
            "View",
            "TeamStatistics",
            teamId.ToString(),
            $"Viewed statistics for team '{team.Name}'"
        );

        var statistics = new
        {
            TeamName = team.Name,
            MemberCount = team.TeamMembers.Count,
            TotalCases = team.CaseTeams.Count,
            OpenCases = team.CaseTeams.Count(ct => ct.Case.Status == "Open"),
            ClosedCases = team.CaseTeams.Count(ct => ct.Case.Status == "Closed"),
            HighPriorityCases = team.CaseTeams.Count(ct => ct.Case.Priority == "High"),
            RecentCases = team.CaseTeams
                .OrderByDescending(ct => ct.Case.CreatedDate)
                .Take(5)
                .Select(ct => new
                {
                    ct.Case.Id,
                    ct.Case.Title,
                    ct.Case.Status,
                    ct.Case.CreatedDate
                })
                .ToList()
        };

        return Ok(statistics);
    }
}
