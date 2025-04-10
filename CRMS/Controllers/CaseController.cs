using CRMS.Data;
using CRMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CRMS.Controllers
{
    [Authorize]
    public class CaseController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public CaseController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Case
        public async Task<IActionResult> Index()
        {
            var cases = await _context.Cases
                .Include(c => c.CaseTeams)
                .ThenInclude(ct => ct.Team)
                .Include(c => c.CaseCriminals)
                .ThenInclude(cc => cc.Criminal)
                .Include(c => c.Evidences)
                .Include(c => c.Victims)
                .Include(c => c.Witnesses)
                .ToListAsync();

            return View(cases);
        }

        // GET: Case/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var @case = await _context.Cases
                .Include(c => c.CaseTeams)
                .ThenInclude(ct => ct.Team)
                .Include(c => c.CaseCriminals)
                .ThenInclude(cc => cc.Criminal)
                .Include(c => c.Evidences)
                .Include(c => c.Victims)
                .Include(c => c.Witnesses)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@case == null)
            {
                return NotFound();
            }

            return View(@case);
        }

        // GET: Case/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Teams = await _context.Teams.ToListAsync();
            ViewBag.Criminals = await _context.Criminal.ToListAsync();
            return View();
        }

        // POST: Case/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Case @case, Guid[] selectedTeams, Guid[] selectedCriminals)
        {
            if (@case == null)
            {
                return BadRequest("Case data is required.");
            }

            if (string.IsNullOrWhiteSpace(@case.Title))
            {
                ModelState.AddModelError(nameof(@case.Title), "Title is required.");
            }

            // Initialize collections to prevent null reference exceptions
            @case.CaseTeams = @case.CaseTeams ?? new List<CaseTeam>();
            @case.CaseCriminals = @case.CaseCriminals ?? new List<CaseCriminal>();

            if (ModelState.IsValid)
            {
                try
                {
                    // Set case properties
                    @case.OpenDate = DateTime.UtcNow;
                    @case.Status = "Open";
                    @case.Priority = @case.Priority ?? "Medium";

                    // Validate and add teams
                    if (selectedTeams != null && selectedTeams.Length > 0)
                    {
                        var existingTeams = await _context.Teams
                            .Where(t => selectedTeams.Contains(t.Id))
                            .ToDictionaryAsync(t => t.Id);

                        var invalidTeams = selectedTeams.Where(id => !existingTeams.ContainsKey(id)).ToList();
                        if (invalidTeams.Any())
                        {
                            ModelState.AddModelError("", $"Invalid team selections: {string.Join(", ", invalidTeams)}");
                            await PrepareViewBagForCase();
                            return View(@case);
                        }

                        foreach (var teamId in selectedTeams)
                        {
                            @case.CaseTeams.Add(new CaseTeam
                            {
                                TeamId = teamId,
                                AssignedDate = DateTime.UtcNow
                            });
                        }
                    }

                    // Validate and add criminals
                    if (selectedCriminals != null && selectedCriminals.Length > 0)
                    {
                        var existingCriminals = await _context.Criminal
                            .Where(c => selectedCriminals.Contains(c.Id))
                            .ToDictionaryAsync(c => c.Id);

                        var invalidCriminals = selectedCriminals.Where(id => !existingCriminals.ContainsKey(id)).ToList();
                        if (invalidCriminals.Any())
                        {
                            ModelState.AddModelError("", $"Invalid criminal selections: {string.Join(", ", invalidCriminals)}");
                            await PrepareViewBagForCase();
                            return View(@case);
                        }

                        foreach (var criminalId in selectedCriminals)
                        {
                            @case.CaseCriminals.Add(new CaseCriminal
                            {
                                CriminalId = criminalId,
                                AddedDate = DateTime.UtcNow
                            });
                        }
                    }

                    _context.Add(@case);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Case created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _context.Entry(@case).State = EntityState.Detached;
                    ModelState.AddModelError("", $"An error occurred while creating the case: {ex.Message}");
                }
            }

            await PrepareViewBagForCase();
            return View(@case);
        }

        private async Task PrepareViewBagForCase()
        {
            ViewBag.Teams = await _context.Teams.ToListAsync();
            ViewBag.Criminals = await _context.Criminal.ToListAsync();
        }


        // GET: Case/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var @case = await _context.Cases
                .Include(c => c.CaseTeams)
                .Include(c => c.CaseCriminals)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@case == null)
            {
                return NotFound();
            }

            ViewBag.Teams = await _context.Teams.ToListAsync();
            ViewBag.Criminals = await _context.Criminal.ToListAsync();
            ViewBag.SelectedTeams = @case.CaseTeams.Select(ct => ct.TeamId).ToList();
            ViewBag.SelectedCriminals = @case.CaseCriminals.Select(cc => cc.CriminalId).ToList();

            return View(@case);
        }

        // POST: Case/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Case @case, Guid[] selectedTeams, Guid[] selectedCriminals)
        {
            if (@case == null)
            {
                return BadRequest("Case data is required.");
            }

            if (id != @case.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(@case.Title))
            {
                ModelState.AddModelError(nameof(@case.Title), "Title is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCase = await _context.Cases
                        .Include(c => c.CaseTeams)
                        .Include(c => c.CaseCriminals)
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (existingCase == null)
                    {
                        return NotFound();
                    }

                    // Update case properties
                    existingCase.Title = @case.Title;
                    existingCase.Description = @case.Description;
                    existingCase.Status = @case.Status;
                    existingCase.Priority = @case.Priority ?? "Medium";
                    existingCase.Location = @case.Location;

                    // Update teams
                    _context.CaseTeams.RemoveRange(existingCase.CaseTeams);
                    if (selectedTeams != null && selectedTeams.Length > 0)
                    {
                        var existingTeams = await _context.Teams
                            .Where(t => selectedTeams.Contains(t.Id))
                            .ToDictionaryAsync(t => t.Id);

                        var invalidTeams = selectedTeams.Where(id => !existingTeams.ContainsKey(id)).ToList();
                        if (invalidTeams.Any())
                        {
                            ModelState.AddModelError("", $"Invalid team selections: {string.Join(", ", invalidTeams)}");
                            await PrepareViewBagForCase();
                            return View(@case);
                        }

                        foreach (var teamId in selectedTeams)
                        {
                            _context.CaseTeams.Add(new CaseTeam
                            {
                                CaseId = @case.Id,
                                TeamId = teamId,
                                AssignedDate = DateTime.UtcNow
                            });
                        }
                    }

                    // Update criminals
                    _context.CaseCriminals.RemoveRange(existingCase.CaseCriminals);
                    if (selectedCriminals != null && selectedCriminals.Length > 0)
                    {
                        var existingCriminals = await _context.Criminal
                            .Where(c => selectedCriminals.Contains(c.Id))
                            .ToDictionaryAsync(c => c.Id);

                        var invalidCriminals = selectedCriminals.Where(id => !existingCriminals.ContainsKey(id)).ToList();
                        if (invalidCriminals.Any())
                        {
                            ModelState.AddModelError("", $"Invalid criminal selections: {string.Join(", ", invalidCriminals)}");
                            await PrepareViewBagForCase();
                            return View(@case);
                        }

                        foreach (var criminalId in selectedCriminals)
                        {
                            _context.CaseCriminals.Add(new CaseCriminal
                            {
                                CaseId = @case.Id,
                                CriminalId = criminalId,
                                AddedDate = DateTime.UtcNow
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Case updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CaseExists(id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while updating the case: {ex.Message}");
                }
            }

            await PrepareViewBagForCase();
            ViewBag.SelectedTeams = selectedTeams;
            ViewBag.SelectedCriminals = selectedCriminals;
            return View(@case);
        }

        // GET: Case/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var @case = await _context.Cases
                .Include(c => c.CaseTeams)
                .ThenInclude(ct => ct.Team)
                .Include(c => c.CaseCriminals)
                .ThenInclude(cc => cc.Criminal)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@case == null)
            {
                return NotFound();
            }

            return View(@case);
        }

        // POST: Case/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var @case = await _context.Cases.FindAsync(id);
            if (@case != null)
            {
                _context.Cases.Remove(@case);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CaseExists(Guid id)
        {
            return _context.Cases.Any(e => e.Id == id);
        }
    }
}
