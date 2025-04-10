using CRMS.Data;
using CRMS.Models;
using CRMS.Models.CreateModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CRMS.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly AppDbContext _context;

        public TeamController(AppDbContext context)
        {
            _context = context;
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
        // Show Team Details Page
        public async Task<IActionResult> Details(Guid id)
        {
            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
                return NotFound();

            return View(team);
        }
    }
}
