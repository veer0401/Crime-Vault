using CRMS.Data;
using CRMS.Models;
using CRMS.Models.CreateModel;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class TeamApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<Users> _userManager;

    public TeamApiController(AppDbContext context, UserManager<Users> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ✅ API to get users
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
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
            return BadRequest(ModelState);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Generate a unique 8-character team code
        string teamCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            TeamCode = teamCode,
            TeamLeaderId = user.Id
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTeamDetails), new { id = team.Id }, team);
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
                return NotFound(new { message = "Team not found." });
            }

            var teamLeader = await _userManager.FindByIdAsync(team.TeamLeaderId);
            string teamLeaderName = teamLeader != null ? teamLeader.FullName : "Unknown";

            var result = new
            {
                team.Id,
                team.Name,
                TeamLeader = teamLeaderName,
                Members = team.TeamMembers.Select(m => new { m.User.Id, m.User.FullName }).ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetTeamDetails: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while fetching team details." });
        }
    }


    // ✅ API to join a team using team code
    [HttpPost("join")]
    public async Task<IActionResult> JoinTeam([FromBody] string teamCode)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var team = await _context.Teams
            .FirstOrDefaultAsync(t => t.TeamCode == teamCode);

        if (team == null)
            return NotFound("Invalid team code.");

        // Check if the user is already in the team
        bool isAlreadyMember = await _context.TeamMembers
            .AnyAsync(tm => tm.TeamId == team.Id && tm.UserId == user.Id);

        if (isAlreadyMember)
            return BadRequest("You are already a member of this team.");

        // Add user to the team
        var teamMember = new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = team.Id,
            UserId = user.Id
        };

        _context.TeamMembers.Add(teamMember);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Successfully joined the team." });
    }
}
