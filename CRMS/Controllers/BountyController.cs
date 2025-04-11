using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using CRMS.Services;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CRMS.Data;

namespace CRMS.Controllers
{
    [Authorize]
    public class BountyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public BountyController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Sentinel Prime,Warden")]
        public IActionResult Create()
        {
            ViewBag.Criminals = _context.Criminal
                .Select(c => new SelectListItem 
                { 
                    Value = c.Id.ToString(),
                    Text = c.Name 
                })
                .ToList();
            
            return View(new Bounty 
            { 
                CreatedDate = DateTime.UtcNow,
                CreatedById = _userManager.GetUserId(User)
            });
        }

        [HttpPost]
        [Authorize(Roles = "Sentinel Prime,Warden")]
        public async Task<IActionResult> Create(Bounty model)
        {
            if (ModelState.IsValid)
            {
                try 
                {
                    // Calculate bounty points
                    model.BountyPoints = BountyService.CalculateBountyPoints(model.Severity, model.Priority);
                    
                    // Add to database
                    _context.Bounties.Add(model);
                    await _context.SaveChangesAsync();

                    // Update criminal's total bounty
                    await BountyService.UpdateCriminalBounty(_context, model.CriminalId);

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log error
                    ModelState.AddModelError("", $"Error creating bounty: {ex.Message}");
                }
            }

            // Reload criminals if validation fails
            ViewBag.Criminals = _context.Criminal
                .Select(c => new SelectListItem 
                { 
                    Value = c.Id.ToString(),
                    Text = c.Name 
                })
                .ToList();
            
            return View(model);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var bounty = await _context.Bounties
                .Include(b => b.Case)
                .Include(b => b.Criminal)
                .Include(b => b.CreatedBy)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bounty == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            bounty.CanEdit = await _userManager.IsInRoleAsync(user, "Sentinel Prime") || 
                           await _userManager.IsInRoleAsync(user, "Warden");

            return View(bounty);
        }

        public async Task<IActionResult> Index()
        {
            var bounties = await _context.Bounties
                .Include(b => b.Case)
                .Include(b => b.Criminal)
                .Include(b => b.CreatedBy)
                .OrderByDescending(b => b.BountyPoints)
                .ToListAsync();
            
            return View(bounties);
        }
    }
}