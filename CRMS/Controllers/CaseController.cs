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
            await PrepareViewBagForCase();
            return View(new Models.CreateModel.CaseCreateViewModel());
        }



        // POST: Case/UpdateCaseDetails
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCaseDetails(Case model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareViewBagForCase();
                return PartialView("_CaseDetailsPartial", model);
            }

            var existingCase = await _context.Cases.FindAsync(model.Id);
            if (existingCase == null)
            {
                return NotFound();
            }

            existingCase.Title = model.Title;
            existingCase.Description = model.Description;
            existingCase.Status = model.Status;
            existingCase.Priority = model.Priority;
            existingCase.Location = model.Location;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CaseExists(model.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // POST: Case/UpdateCriminalInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCriminalInfo(Guid id, List<CriminalInfo> criminals)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CriminalInfoPartial", criminals);
            }

            var @case = await _context.Cases
                .Include(c => c.CaseCriminals)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (@case == null)
            {
                return NotFound();
            }

            // Remove existing criminals
            _context.CaseCriminals.RemoveRange(@case.CaseCriminals);

            // Add updated criminals
            foreach (var criminalInfo in criminals)
            {
                @case.CaseCriminals.Add(new CaseCriminal
                {
                    CaseId = id,
                    CriminalId = criminalInfo.CriminalId,
                    Role = criminalInfo.Role,
                    Notes = criminalInfo.Notes,
                    AddedDate = DateTime.UtcNow
                });
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Criminal information updated successfully.";
                return RedirectToAction(nameof(Edit), new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while updating criminal information: {ex.Message}");
                return PartialView("_CriminalInfoPartial", criminals);
            }
        }

        // POST: Case/UpdateEvidence
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEvidence(Guid id, List<Evidence> evidences)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EvidencePartial", evidences);
            }

            var @case = await _context.Cases
                .Include(c => c.Evidences)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (@case == null)
            {
                return NotFound();
            }

            // Remove existing evidence
            _context.Evidence.RemoveRange(@case.Evidences);

            // Add updated evidence
            foreach (var evidence in evidences)
            {
                evidence.CaseId = id;
                evidence.CollectionDate = DateTime.UtcNow;
                @case.Evidences.Add(evidence);
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Evidence information updated successfully.";
                return RedirectToAction(nameof(Edit), new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while updating evidence information: {ex.Message}");
                return PartialView("_EvidencePartial", evidences);
            }
        }

        // POST: Case/UpdateWitness
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWitness(Guid id, List<Witness> witnesses)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_WitnessPartial", witnesses);
            }

            var @case = await _context.Cases
                .Include(c => c.Witnesses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (@case == null)
            {
                return NotFound();
            }

            // Remove existing witnesses
            _context.Witnesses.RemoveRange(@case.Witnesses);

            // Add updated witnesses
            foreach (var witness in witnesses)
            {
                witness.CaseId = id;
                @case.Witnesses.Add(witness);
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Witness information updated successfully.";
                return RedirectToAction(nameof(Edit), new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while updating witness information: {ex.Message}");
                return PartialView("_WitnessPartial", witnesses);
            }
        }

        // POST: Case/UpdateVictim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVictim(Guid id, List<Victim> victims)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_VictimPartial", victims);
            }

            var @case = await _context.Cases
                .Include(c => c.Victims)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (@case == null)
            {
                return NotFound();
            }

            // Remove existing victims
            _context.Victims.RemoveRange(@case.Victims);

            // Add updated victims
            foreach (var victim in victims)
            {
                victim.CaseId = id;
                @case.Victims.Add(victim);
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Victim information updated successfully.";
                return RedirectToAction(nameof(Edit), new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while updating victim information: {ex.Message}");
                return PartialView("_VictimPartial", victims);
            }
        }



        // POST: Case/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.CreateModel.CaseCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareViewBagForCase();
                return View(model);
            }

            var @case = new Case
            {
                Title = model.Title,
                Description = model.Description,
                Status = "Open",
                Priority = model.Priority ?? "Medium",
                Location = model.Location,
                OpenDate = DateTime.UtcNow,
                CaseTeams = new List<CaseTeam>(),
                CaseCriminals = new List<CaseCriminal>(),
                Evidences = new List<Evidence>(),
                Witnesses = new List<Witness>(),
                Victims = new List<Victim>(),
                Suspects = new List<Suspect>()
            };

            try
            {
                // Add Teams
                if (model.SelectedTeams?.Any() == true)
                {
                    foreach (var teamId in model.SelectedTeams)
                    {
                        @case.CaseTeams.Add(new CaseTeam
                        {
                            TeamId = teamId,
                            AssignedDate = DateTime.UtcNow
                        });
                    }
                }

                // Add Criminals
                if (model.Criminals?.Any() == true)
                {
                    foreach (var criminalInfo in model.Criminals)
                    {
                        @case.CaseCriminals.Add(new CaseCriminal
                        {
                            CriminalId = criminalInfo.CriminalId,
                            Role = criminalInfo.Role,
                            Notes = criminalInfo.Notes,
                            AddedDate = DateTime.UtcNow
                        });
                    }
                }

                // Add New Criminal if provided
                if (model.NewCriminal != null && !string.IsNullOrWhiteSpace(model.NewCriminal.Name))
                {
                    var newCriminal = new Criminal
                    {
                        Name = model.NewCriminal.Name,
                        Alias = model.NewCriminal.Alias,
                        Description = model.NewCriminal.Description
                    };
                    _context.Criminal.Add(newCriminal);
                    await _context.SaveChangesAsync();

                    @case.CaseCriminals.Add(new CaseCriminal
                    {
                        CriminalId = newCriminal.Id,
                        Role = "Suspect",
                        AddedDate = DateTime.UtcNow
                    });
                }

                // Add Suspects
                if (model.Suspects?.Any() == true)
                {
                    foreach (var suspectInfo in model.Suspects)
                    {
                        @case.Suspects.Add(new Suspect
                        {
                            Name = suspectInfo.Name,
                            LastKnownLocation = suspectInfo.LastKnownLocation,
                            Description = suspectInfo.Description,
                            PossibleMotives = suspectInfo.PossibleMotives,
                            RelationshipToVictim = suspectInfo.RelationshipToVictim
                        });
                    }
                }

                // Add Evidence
                if (model.Evidences?.Any() == true)
                {
                    foreach (var evidenceInfo in model.Evidences)
                    {
                        @case.Evidences.Add(new Evidence
                        {
                            Title = evidenceInfo.Title,
                            Description = evidenceInfo.Description,
                            Type = evidenceInfo.Type,
                            StorageLocation = evidenceInfo.StorageLocation,
                            CollectionDate = DateTime.UtcNow
                        });
                    }
                }

                // Add Witnesses
                if (model.Witnesses?.Any() == true)
                {
                    foreach (var witnessInfo in model.Witnesses)
                    {
                        @case.Witnesses.Add(new Witness
                        {
                            Name = witnessInfo.Name,
                            ContactNumber = witnessInfo.ContactNumber,
                            Statement = witnessInfo.Statement,
                            IsAnonymous = witnessInfo.IsAnonymous
                        });
                    }
                }

                // Add Victims
                if (model.Victims?.Any() == true)
                {
                    foreach (var victimInfo in model.Victims)
                    {
                        @case.Victims.Add(new Victim
                        {
                            Name = victimInfo.Name,
                            ContactNumber = victimInfo.ContactNumber,
                            Description = victimInfo.Description,
                            InjurySeverity = victimInfo.InjurySeverity
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
                ModelState.AddModelError("", $"An error occurred while creating the case: {ex.Message}");
                await PrepareViewBagForCase();
                return View(model);
            }
        }
        private async Task PrepareViewBagForCase()
        {
            ViewBag.Teams = await _context.Teams.ToListAsync();
            ViewBag.Criminals = await _context.Criminal.ToListAsync();
            ViewBag.SelectedTeams = new List<Guid>();
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
