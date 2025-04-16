using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRMS.Data;
using CRMS.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using CRMS.Models.CreateModel;
using System.Collections.Generic;
using CRMS.Models.EditModel;
using CreateEvidenceVM = CRMS.Models.CreateModel.EvidenceViewModel;
using EditEvidenceVM = CRMS.Models.EditModel.EvidenceViewModel;
using EditWitnessVM = CRMS.Models.EditModel.WitnessViewModel;
using EditVictimVM = CRMS.Models.EditModel.VictimViewModel;
using EditSuspectVM = CRMS.Models.EditModel.SuspectViewModel;
using CRMS.Services;

namespace CRMS.Controllers
{
    public class CaseController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IActivityLogService _activityLogService;

        public CaseController(AppDbContext context, IActivityLogService activityLogService)
        {
            _context = context;
            _activityLogService = activityLogService;
        }

        public async Task<IActionResult> Index()
        {
            // Log the activity
            await _activityLogService.LogActivityAsync(
                "View",
                "CaseList",
                "All",
                "Viewed list of all cases"
            );

            return View(await _context.Cases.ToListAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var caseItem = await _context.Cases.FirstOrDefaultAsync(c => c.Id == id.Value);
            if (caseItem == null) return NotFound();

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "View",
                "Case",
                caseItem.Id.ToString(),
                $"Viewed details of case '{caseItem.Title}'"
            );

            return View(caseItem);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Log the activity
            _activityLogService.LogActivityAsync(
                "View",
                "CaseCreateForm",
                "New",
                "Viewed case creation form"
            ).Wait();

            var viewModel = new CaseCreateViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CaseCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // Create new case
            var newCase = viewModel.Case;
            newCase.Id = Guid.NewGuid();
            newCase.CreatedDate = DateTime.Now;

            _context.Cases.Add(newCase);

            // Link related entries
            foreach (var criminal in viewModel.Criminals)
            {
                if (!string.IsNullOrEmpty(criminal.Name))
                {
                    criminal.Id = Guid.NewGuid();
                    _context.Criminal.Add(criminal);

                    var caseCriminal = new CaseCriminal
                    {
                        Id = Guid.NewGuid(),
                        CaseId = newCase.Id,
                        CriminalId = criminal.Id
                    };

                    _context.CaseCriminals.Add(caseCriminal);
                }
            }

            foreach (var evidenceViewModel in viewModel.Evidences)
            {
                if (!string.IsNullOrEmpty(evidenceViewModel.Description))
                {
                    var evidence = new Evidence
                    {
                        Id = Guid.NewGuid(),
                        CaseId = newCase.Id,
                        Description = evidenceViewModel.Description,
                    };

                    _context.Evidence.Add(evidence);
                }
            }

            foreach (var victim in viewModel.Victims)
            {
                if (!string.IsNullOrEmpty(victim.Name))
                {
                    victim.Id = Guid.NewGuid();
                    victim.CaseId = newCase.Id;
                    _context.Victim.Add(victim);
                }
            }

            foreach (var witness in viewModel.Witnesses)
            {
                if (!string.IsNullOrEmpty(witness.Name))
                {
                    witness.Id = Guid.NewGuid();
                    witness.CaseId = newCase.Id;
                    _context.Witness.Add(witness);
                }
            }

            await _context.SaveChangesAsync();

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "Create",
                "Case",
                newCase.Id.ToString(),
                $"Created new case '{newCase.Title}'"
            );

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult CreateCaseDetails()
        {
            var viewModel = new CaseDetailsViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCaseDetails(CaseDetailsViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // Create new case
            var newCase = new Case
            {
                Id = Guid.NewGuid(),
                Title = viewModel.Title,
                Description = viewModel.Description,
                Status = viewModel.Status,
                Priority = viewModel.Priority,
                Location = viewModel.Location,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            _context.Cases.Add(newCase);

            // Save the case ID in TempData for other forms
            TempData["CaseId"] = newCase.Id;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Create));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var caseEntity = await _context.Cases
                .Include(c => c.CaseTeams)
                .Include(c => c.CaseCriminals)
                    .ThenInclude(cc => cc.Criminal)
                .Include(c => c.Evidences)
                .Include(c => c.Witnesses)
                .Include(c => c.Victims)
                .Include(c => c.Suspects)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caseEntity == null)
            {
                return NotFound();
            }

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "View",
                "CaseEditForm",
                caseEntity.Id.ToString(),
                $"Viewed edit form for case '{caseEntity.Title}'"
            );

            var viewModel = new EditCaseDetailsViewModel
            {
                Id = caseEntity.Id,
                Title = caseEntity.Title,
                Description = caseEntity.Description,
                Status = caseEntity.Status,
                Priority = caseEntity.Priority,
                Location = caseEntity.Location,
                SelectedTeams = caseEntity.CaseTeams.Select(ct => ct.TeamId).ToList(),
                CreatedDate = caseEntity.CreatedDate,
                UpdatedDate = caseEntity.UpdatedDate,
                Criminals = caseEntity.CaseCriminals.Select(cc => new EditCriminalViewModel
                {
                    Id = cc.Criminal.Id,
                    CaseId = cc.CaseId,
                    Name = cc.Criminal.Name,
                    Role = cc.Role,
                    Notes = cc.Notes
                }).ToList(),
                Evidences = caseEntity.Evidences.Select(e => new EditEvidenceViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Type = e.Type,
                    StorageLocation = e.StorageLocation,
                    CollectionDate = e.CollectionDate,
                    FilePath = e.FilePath
                }).ToList(),
                Witnesses = caseEntity.Witnesses.Select(w => new EditWitnessViewModel
                {
                    Id = w.Id,
                    Name = w.Name,
                    ContactNumber = w.ContactNumber,
                    Address = w.Address,
                    Statement = w.Statement,
                    RelationToCase = w.RelationToCase,
                    Credibility = w.Credibility,
                    CredibilityRating = w.CredibilityRating,
                    IsAnonymous = w.IsAnonymous
                }).ToList(),
                Victims = caseEntity.Victims.Select(v => new EditVictimViewModel
                {
                    Id = v.Id,
                    Name = v.Name,
                    ContactNumber = v.ContactNumber,
                    Address = v.Address,
                    Description = v.Description,
                    IncidentDate = v.IncidentDate,
                    InjurySeverity = v.InjurySeverity,
                    EmergencyContact = v.EmergencyContact,
                    IsMinor = v.IsMinor,
                    MedicalHistory = v.MedicalHistory,
                    InjurySustained = v.InjurySustained,
                    PropertyDamage = v.PropertyDamage,
                    CompensationClaimed = v.CompensationClaimed
                }).ToList(),
                Suspects = caseEntity.Suspects.Select(s => new EditSuspectViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Alias = s.Alias,
                    Age = s.Age,
                    Gender = s.Gender,
                    Description = s.Description,
                    Status= s.SuspectStatus,
                    LastKnownLocation = s.LastKnownLocation,
                    PossibleMotives = s.PossibleMotives,
                    RelationshipToVictim = s.RelationshipToVictim,
                    Alibi = s.Alibi,
                    IsPersonOfInterest = s.IsPersonOfInterest
                }).ToList()
            };

            ViewBag.Teams = await _context.Teams.ToListAsync();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditCaseDetailsViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var caseEntity = await _context.Cases
                        .Include(c => c.CaseTeams)
                        .Include(c => c.CaseCriminals)
                        .Include(c => c.Evidences)
                        .Include(c => c.Witnesses)
                        .Include(c => c.Victims)
                        .Include(c => c.Suspects)
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (caseEntity == null)
                    {
                        return NotFound();
                    }

                    // Update case details
                    caseEntity.Title = viewModel.Title;
                    caseEntity.Description = viewModel.Description;
                    caseEntity.Status = viewModel.Status;
                    caseEntity.Priority = viewModel.Priority;
                    caseEntity.Location = viewModel.Location;
                    caseEntity.UpdatedDate = DateTime.UtcNow;

                    // Update teams
                    var existingTeamIds = caseEntity.CaseTeams.Select(ct => ct.TeamId).ToList();
                    var teamsToAdd = viewModel.SelectedTeams.Except(existingTeamIds).ToList();
                    var teamsToRemove = existingTeamIds.Except(viewModel.SelectedTeams).ToList();

                    foreach (var teamId in teamsToAdd)
                    {
                        caseEntity.CaseTeams.Add(new CaseTeam { CaseId = id, TeamId = teamId });
                    }

                    foreach (var teamId in teamsToRemove)
                    {
                        var caseTeam = caseEntity.CaseTeams.First(ct => ct.TeamId == teamId);
                        _context.CaseTeams.Remove(caseTeam);
                    }

                    // Update criminal
                    UpdateCriminals(caseEntity, viewModel.Criminals);

                    // Convert Edit view models to regular view models
                    var evidenceViewModels = viewModel.Evidences.Select(e => new EditEvidenceVM
                    {
                        Id = e.Id.Value,
                        Title = e.Title,
                        Description = e.Description,
                        Type = e.Type,
                        StorageLocation = e.StorageLocation,
                        CollectionDate = e.CollectionDate,
                        FilePath = e.FilePath
                    }).ToList();

                    var witnessViewModels = viewModel.Witnesses.Select(w => new EditWitnessVM
                    {
                        Id = w.Id.Value,
                        Name = w.Name,
                        ContactNumber = w.ContactNumber,
                        Address = w.Address,
                        Statement = w.Statement,
                        RelationToCase = w.RelationToCase,
                        Credibility = w.Credibility,
                        CredibilityRating = w.CredibilityRating,
                        IsAnonymous = w.IsAnonymous
                    }).ToList();

                    // Update the mapping to use EditVictimViewModel instead of VictimViewModel
                    var victimViewModels = viewModel.Victims.Select(v => new EditVictimVM
                    {
                        Id = v.Id.Value,
                        Name = v.Name,
                        ContactNumber = v.ContactNumber,
                        Address = v.Address,
                        Description = v.Description,
                        IncidentDate = v.IncidentDate,
                        InjurySeverity = v.InjurySeverity,
                        EmergencyContact = v.EmergencyContact,
                        IsMinor = v.IsMinor,
                        MedicalHistory = v.MedicalHistory,
                        InjurySustained = v.InjurySustained,
                        PropertyDamage = v.PropertyDamage,
                        CompensationClaimed = v.CompensationClaimed
                    }).ToList();

                    var suspectViewModels = viewModel.Suspects.Select(s => new EditSuspectVM
                    {
                        Id = s.Id.Value,
                        Name = s.Name,
                        Alias = s.Alias,
                        Age = s.Age,
                        Gender = s.Gender,
                        Description = s.Description,
                        SuspectStatus = s.Status,
                        LastKnownLocation = s.LastKnownLocation,
                        PossibleMotives = s.PossibleMotives,
                        RelationshipToVictim = s.RelationshipToVictim,
                        Alibi = s.Alibi,
                        IsPersonOfInterest = s.IsPersonOfInterest
                    }).ToList();

                    // Update evidence
                    await UpdateEvidence(caseEntity, evidenceViewModels);

                    // Update witnesses
                    await UpdateWitnesses(caseEntity, witnessViewModels);

                    // Pass the correctly typed list to UpdateVictims
                    var editVictimViewModels = victimViewModels.Select(v => new EditVictimViewModel
                    {
                        Id = v.Id,
                        Name = v.Name,
                        ContactNumber = v.ContactNumber,
                        Address = v.Address,
                        Description = v.Description,
                        IncidentDate = v.IncidentDate,
                        InjurySeverity = v.InjurySeverity,
                        EmergencyContact = v.EmergencyContact,
                        IsMinor = v.IsMinor,
                        MedicalHistory = v.MedicalHistory,
                        InjurySustained = v.InjurySustained,
                        PropertyDamage = v.PropertyDamage,
                        CompensationClaimed = v.CompensationClaimed
                    }).ToList();

                    UpdateVictims(caseEntity, editVictimViewModels); // ✅ Correct type


                    // Update suspects
                    await UpdateSuspects(caseEntity, suspectViewModels);

                    await _context.SaveChangesAsync();

                    // Log the activity
                    await _activityLogService.LogActivityAsync(
                        "Update",
                        "Case",
                        caseEntity.Id.ToString(),
                        $"Updated case '{caseEntity.Title}'"
                    );

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CaseExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Teams = await _context.Teams.ToListAsync();
            return View(viewModel);
        }

        private void UpdateCriminals(Case caseEntity, List<EditCriminalViewModel> criminals)
        {
            // Remove criminals that are no longer associated with the case
            var existingCriminals = _context.CaseCriminals
                .Where(cc => cc.CaseId == caseEntity.Id)
                .ToList();

            foreach (var existingCriminal in existingCriminals)
            {
                if (!criminals.Any(c => c.Id == existingCriminal.CriminalId))
                {
                    _context.CaseCriminals.Remove(existingCriminal);
                }
            }

            // Update or add new criminals
            foreach (var criminalVm in criminals)
            {
                var criminal = _context.Criminal.FirstOrDefault(c => c.Id == criminalVm.Id);
                if (criminal == null)
                {
                    criminal = new Criminal
                    {
                        Id = criminalVm.Id.Value,
                        Name = criminalVm.Name,
                    };
                    _context.Criminal.Add(criminal);
                }
                else
                {
                    criminal.Name = criminalVm.Name;
                }

                // Update or create the relationship
                var caseCriminal = _context.CaseCriminals
                    .FirstOrDefault(cc => cc.CaseId == caseEntity.Id && cc.CriminalId == criminal.Id);

                if (caseCriminal == null)
                {
                    caseCriminal = new CaseCriminal
                    {
                        CaseId = caseEntity.Id,
                        CriminalId = criminal.Id,
                        Role = criminalVm.Role,
                        Notes = criminalVm.Notes
                    };
                    _context.CaseCriminals.Add(caseCriminal);
                }
                else
                {
                    caseCriminal.Role = criminalVm.Role;
                    caseCriminal.Notes = criminalVm.Notes;
                }
            }
        }

        private async Task UpdateEvidence(Case caseEntity, List<EditEvidenceVM> evidences)
        {
            var existingEvidences = caseEntity.Evidences.ToList();
            var evidenceIds = evidences.Select(e => e.Id).ToList();

            // Remove evidence that is no longer in the view model
            foreach (var existingEvidence in existingEvidences)
            {
                if (!evidenceIds.Contains(existingEvidence.Id))
                {
                    _context.Evidence.Remove(existingEvidence);
                }
            }

            // Add or update evidence
            foreach (var evidence in evidences)
            {
                var existingEvidence = existingEvidences.FirstOrDefault(e => e.Id == evidence.Id);
                if (existingEvidence == null)
                {
                    var newEvidence = new Evidence
                    {
                        Id = evidence.Id,
                        CaseId = caseEntity.Id,
                        Title = evidence.Title,
                        Description = evidence.Description,
                        Type = evidence.Type,
                        StorageLocation = evidence.StorageLocation,
                        CollectionDate = evidence.CollectionDate,
                        FilePath = evidence.FilePath
                    };
                    _context.Evidence.Add(newEvidence);
                }
                else
                {
                    existingEvidence.Title = evidence.Title;
                    existingEvidence.Description = evidence.Description;
                    existingEvidence.Type = evidence.Type;
                    existingEvidence.StorageLocation = evidence.StorageLocation;
                    existingEvidence.CollectionDate = evidence.CollectionDate;
                    existingEvidence.FilePath = evidence.FilePath;
                }
            }
        }

        private async Task UpdateWitnesses(Case caseEntity, List<EditWitnessVM> witnesses)
        {
            var existingWitnesses = caseEntity.Witnesses.ToList();
            var witnessIds = witnesses.Select(w => w.Id).ToList();

            // Remove witnesses that are no longer in the view model
            foreach (var existingWitness in existingWitnesses)
            {
                if (!witnessIds.Contains(existingWitness.Id))
                {
                    _context.Witness.Remove(existingWitness);
                }
            }

            // Add or update witnesses
            foreach (var witness in witnesses)
            {
                var existingWitness = existingWitnesses.FirstOrDefault(w => w.Id == witness.Id);
                if (existingWitness == null)
                {
                    var newWitness = new Witness
                    {
                        Id = witness.Id,
                        CaseId = caseEntity.Id,
                        Name = witness.Name,
                        ContactNumber = witness.ContactNumber,
                        Address = witness.Address,
                        Statement = witness.Statement,
                        RelationToCase = witness.RelationToCase,
                        Credibility = witness.Credibility,
                        CredibilityRating = witness.CredibilityRating,
                        IsAnonymous = witness.IsAnonymous
                    };
                    _context.Witness.Add(newWitness);
                }
                else
                {
                    existingWitness.Name = witness.Name;
                    existingWitness.ContactNumber = witness.ContactNumber;
                    existingWitness.Address = witness.Address;
                    existingWitness.Statement = witness.Statement;
                    existingWitness.RelationToCase = witness.RelationToCase;
                    existingWitness.Credibility = witness.Credibility;
                    existingWitness.CredibilityRating = witness.CredibilityRating;
                    existingWitness.IsAnonymous = witness.IsAnonymous;
                }
            }
        }

        private void UpdateVictims(Case caseEntity, List<EditVictimViewModel> victims)
        {
            if (victims == null) return;

            // Remove victims that are no longer in the list
            var existingVictimIds = victims.Where(v => v.Id.HasValue).Select(v => v.Id.Value).ToList();
            var victimsToRemove = caseEntity.Victims.Where(v => !existingVictimIds.Contains(v.Id)).ToList();
            foreach (var victim in victimsToRemove)
            {
                caseEntity.Victims.Remove(victim);
            }

            // Update or add victims
            foreach (var victimVm in victims)
            {
                if (victimVm.Id.HasValue)
                {
                    // Update existing victim
                    var existingVictim = caseEntity.Victims.FirstOrDefault(v => v.Id == victimVm.Id.Value);
                    if (existingVictim != null)
                    {
                        existingVictim.Name = victimVm.Name;
                        existingVictim.ContactNumber = victimVm.ContactNumber;
                        existingVictim.Address = victimVm.Address;
                        existingVictim.Description = victimVm.Description;
                        existingVictim.IncidentDate = victimVm.IncidentDate;
                        existingVictim.InjurySeverity = victimVm.InjurySeverity;
                        existingVictim.EmergencyContact = victimVm.EmergencyContact;
                        existingVictim.IsMinor = victimVm.IsMinor;
                        existingVictim.MedicalHistory = victimVm.MedicalHistory;
                        existingVictim.InjurySustained = victimVm.InjurySustained;
                        existingVictim.PropertyDamage = victimVm.PropertyDamage;
                        existingVictim.CompensationClaimed = victimVm.CompensationClaimed;
                        existingVictim.Status = victimVm.Status;
                    }
                }
                else
                {
                    // Add new victim
                    var newVictim = new Victim
                    {
                        Id = Guid.NewGuid(),
                        CaseId = caseEntity.Id,
                        Name = victimVm.Name,
                        ContactNumber = victimVm.ContactNumber,
                        
                        //ContactInfo = victimVm.ContactInfo,
                        Address = victimVm.Address,
                        Description = victimVm.Description,
                        IncidentDate = victimVm.IncidentDate,
                        InjurySeverity = victimVm.InjurySeverity,
                        EmergencyContact = victimVm.EmergencyContact,
                        IsMinor = victimVm.IsMinor,
                        MedicalHistory = victimVm.MedicalHistory,
                        InjurySustained = victimVm.InjurySustained,
                        PropertyDamage = victimVm.PropertyDamage,
                        CompensationClaimed = victimVm.CompensationClaimed,
                        Status = victimVm.Status
                    };
                    caseEntity.Victims.Add(newVictim);
                }
            }
        }

        private async Task UpdateSuspects(Case caseEntity, List<EditSuspectVM> suspects)
        {
            var existingSuspects = caseEntity.Suspects.ToList();
            var suspectIds = suspects.Select(s => s.Id).ToList();

            // Remove suspects that are no longer in the view model
            foreach (var existingSuspect in existingSuspects)
            {
                if (!suspectIds.Contains(existingSuspect.Id))
                {
                    _context.Suspect.Remove(existingSuspect);
                }
            }

            // Add or update suspects
            foreach (var suspect in suspects)
            {
                var existingSuspect = existingSuspects.FirstOrDefault(s => s.Id == suspect.Id);
                if (existingSuspect == null)
                {
                    var newSuspect = new Suspect
                    {
                        Id = suspect.Id,
                        CaseId = caseEntity.Id,
                        Name = suspect.Name,
                        Alias = suspect.Alias,
                        Age = suspect.Age,
                        Gender = suspect.Gender,
                        Description = suspect.Description,
                        SuspectStatus = suspect.SuspectStatus,
                        LastKnownLocation = suspect.LastKnownLocation,
                        PossibleMotives = suspect.PossibleMotives,
                        RelationshipToVictim = suspect.RelationshipToVictim,
                        Alibi = suspect.Alibi,
                        IsPersonOfInterest = suspect.IsPersonOfInterest
                    };
                    _context.Suspect.Add(newSuspect);
                }
                else
                {
                    existingSuspect.Name = suspect.Name;
                    existingSuspect.Alias = suspect.Alias;
                    existingSuspect.Age = suspect.Age;
                    existingSuspect.Gender = suspect.Gender;
                    existingSuspect.Description = suspect.Description;
                    existingSuspect.SuspectStatus = suspect.SuspectStatus;
                    existingSuspect.LastKnownLocation = suspect.LastKnownLocation;
                    existingSuspect.PossibleMotives = suspect.PossibleMotives;
                    existingSuspect.RelationshipToVictim = suspect.RelationshipToVictim;
                    existingSuspect.Alibi = suspect.Alibi;
                    existingSuspect.IsPersonOfInterest = suspect.IsPersonOfInterest;
                }
            }
        }

        private async Task<bool> CaseExists(Guid id)
        {
            return await _context.Cases.AnyAsync(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> AddCriminal(EditCriminalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state" });
            }

            try
            {
                // Check if this is an existing criminal being added to the case
                if (model.Id != Guid.Empty)
                {
                    var existingCriminal = await _context.Criminal.FindAsync(model.Id);
                    if (existingCriminal == null)
                    {
                        return Json(new { success = false, message = "Criminal not found" });
                    }

                    // Check if the criminal is already associated with the case
                    var existingCaseCriminal = await _context.CaseCriminals
                        .FirstOrDefaultAsync(cc => cc.CaseId == model.CaseId && cc.CriminalId == model.Id);

                    if (existingCaseCriminal != null)
                    {
                        return Json(new { success = false, message = "Criminal is already associated with this case" });
                    }

                    // Create the relationship
                    var caseCriminal = new CaseCriminal
                    {
                        CaseId = model.CaseId,
                        CriminalId = model.Id.Value,
                        Role = model.Role,
                        Notes = model.Notes
                    };

                    _context.CaseCriminals.Add(caseCriminal);
                }
                else
                {
                    // Create a new criminal
                    var criminal = new Criminal
                    {
                        Id = Guid.NewGuid(),
                        Name = model.Name,
                    };

                    _context.Criminal.Add(criminal);

                    // Create the relationship
                    var caseCriminal = new CaseCriminal
                    {
                        CaseId = model.CaseId,
                        CriminalId = criminal.Id,
                        Role = model.Role,
                        Notes = model.Notes
                    };

                    _context.CaseCriminals.Add(caseCriminal);
                }

                await _context.SaveChangesAsync();

                // Log the activity
                await _activityLogService.LogActivityAsync(
                    "Add",
                    "CaseCriminal",
                    model.CaseId.ToString(),
                    $"Added criminal to case"
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCriminal(EditCriminalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state" });
            }

            try
            {
                var criminal = await _context.Criminal.FindAsync(model.Id);
                if (criminal == null)
                {
                    return Json(new { success = false, message = "Criminal not found" });
                }

                // Update criminal properties
                criminal.Name = model.Name;
       
                // Update the relationship if needed
                var caseCriminal = await _context.CaseCriminals
                    .FirstOrDefaultAsync(cc => cc.CaseId == model.CaseId && cc.CriminalId == model.Id);

                if (caseCriminal != null)
                {
                    caseCriminal.Role = model.Role;
                    caseCriminal.Notes = model.Notes;
                }

                await _context.SaveChangesAsync();

                // Log the activity
                await _activityLogService.LogActivityAsync(
                    "Update",
                    "CaseCriminal",
                    model.CaseId.ToString(),
                    $"Updated criminal in case"
                );

                return Json(new { success = true, criminal_id = model.Id, case_id = model.CaseId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCriminal(Guid id, Guid caseId)
        {
            try
            {
                // Find the case-criminal relationship
                var caseCriminal = await _context.CaseCriminals
                    .FirstOrDefaultAsync(cc => cc.CaseId == caseId && cc.CriminalId == id);

                if (caseCriminal == null)
                {
                    return Json(new { success = false, message = "Criminal not found in this case" });
                }

                // Remove the relationship
                _context.CaseCriminals.Remove(caseCriminal);
                await _context.SaveChangesAsync();

                // Log the activity
                await _activityLogService.LogActivityAsync(
                    "Delete",
                    "CaseCriminal",
                    caseId.ToString(),
                    $"Removed criminal from case"
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCriminal(Guid id)
        {
            var caseCriminal = await _context.CaseCriminals
                .Include(cc => cc.Criminal)
                .FirstOrDefaultAsync(cc => cc.CriminalId == id);

            if (caseCriminal == null || caseCriminal.Criminal == null)
            {
                return NotFound();
            }

            var viewModel = new EditCriminalViewModel
            {
                Id = caseCriminal.Criminal.Id,
                Name = caseCriminal.Criminal.Name,
                CaseId = caseCriminal.CaseId,
                Role = caseCriminal.Role,
                Notes = caseCriminal.Notes
            };

            return Json(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddEvidence([FromForm] EditEvidenceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state" });
            }

            try
            {
                var evidence = new Evidence
                {
                    Id = Guid.NewGuid(),
                    CaseId = model.CaseId,
                    Title = model.Title,
                    Description = model.Description,
                    Type = model.Type,
                    StorageLocation = model.StorageLocation,
                    CollectionDate = model.CollectionDate,
                    FilePath = model.FilePath
                };

                _context.Evidence.Add(evidence);
                await _context.SaveChangesAsync();

                // Log the activity
                await _activityLogService.LogActivityAsync(
                    "Add",
                    "Evidence",
                    model.CaseId.ToString(),
                    $"Added evidence to case"
                );

                return Json(new { success = true, evidence_id = evidence.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEvidence(Guid id)
        {
            var evidence = await _context.Evidence.FindAsync(id);
            if (evidence == null)
            {
                return NotFound();
            }

            var model = new EditEvidenceViewModel
            {
                Id = evidence.Id,
                CaseId = evidence.CaseId,
                Description = evidence.Description,
                CollectionDate = evidence.CollectionDate,
                Location = evidence.StorageLocation,
                Type = evidence.Type,
                Status = evidence.ContentType
            };

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvidence(Guid id)
        {
            var evidence = await _context.Evidence.FindAsync(id);
            if (evidence == null)
            {
                return Json(new { success = false, message = "Evidence not found" });
            }

            try
            {
                _context.Evidence.Remove(evidence);
                await _context.SaveChangesAsync();

                // Log the activity
                await _activityLogService.LogActivityAsync(
                    "Delete",
                    "Evidence",
                    id.ToString(),
                    $"Removed evidence from case"
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddWitness([FromForm] EditWitnessViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state" });
            }

            try
            {
                var witness = new Witness
                {
                    Id = model.Id ?? Guid.NewGuid(),
                    CaseId = model.CaseId,
                    Name = model.Name,
                    ContactNumber = model.Contact,
                    Statement = model.Statement,
                    RelationToCase = model.RelationToCase,
                    ProtectionStatus = model.Status
                };

                if (model.Id.HasValue)
                {
                    _context.Update(witness);
                }
                else
                {
                    await _context.Witness.AddAsync(witness);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetWitness(Guid id)
        {
            var witness = await _context.Witness.FindAsync(id);
            if (witness == null)
            {
                return NotFound();
            }

            var model = new EditWitnessViewModel
            {
                Id = witness.Id,
                CaseId = witness.CaseId,
                Name = witness.Name,
                Contact = witness.ContactNumber,
                Statement = witness.Statement,
                RelationToCase = witness.RelationToCase,
                Status = witness.ProtectionStatus
            };

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWitness(Guid id)
        {
            var witness = await _context.Witness.FindAsync(id);
            if (witness == null)
            {
                return Json(new { success = false, message = "Witness not found" });
            }

            try
            {
                _context.Witness.Remove(witness);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddVictim([FromForm] EditVictimViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state" });
            }

            try
            {
                var victim = new Victim
                {
                    Id = model.Id ?? Guid.NewGuid(),
                    CaseId = model.CaseId,
                    Name = model.Name,
                    ContactNumber = model.Contact,
                    IncidentDate = model.IncidentDate,
                    Description = model.Description,
                    InjurySeverity = model.InjurySeverity,
                    Status = model.Status
                };

                if (model.Id.HasValue)
                {
                    _context.Update(victim);
                }
                else
                {
                    await _context.Victim.AddAsync(victim);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVictim(Guid id)
        {
            var victim = await _context.Victim.FindAsync(id);
            if (victim == null)
            {
                return NotFound();
            }

            var model = new EditVictimViewModel
            {
                Id = victim.Id,
                CaseId = victim.CaseId,
                Name = victim.Name,
                Contact = victim.ContactInfo,
                IncidentDate = victim.IncidentDate,
                Description = victim.Description,
                InjurySeverity = victim.InjurySeverity,
                Status = victim.Status
            };

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVictim(Guid id)
        {
            var victim = await _context.Victim.FindAsync(id);
            if (victim == null)
            {
                return Json(new { success = false, message = "Victim not found" });
            }

            try
            {
                _context.Victim.Remove(victim);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddSuspect([FromForm] EditSuspectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state" });
            }

            try
            {
                var suspect = new Suspect
                {
                    Id = model.Id ?? Guid.NewGuid(),
                    CaseId = model.CaseId,
                    Name = model.Name,
                    Description = model.Description,
                    LastSeenDate = model.LastSeenDate,
                    LastKnownLocation = model.LastKnownLocation,
                    SuspectStatus = model.Status,
                    PossibleMotives= model.ThreatLevel
                };

                if (model.Id.HasValue)
                {
                    _context.Update(suspect);
                }
                else
                {
                    await _context.Suspect.AddAsync(suspect);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSuspect(Guid id)
        {
            var suspect = await _context.Suspect.FindAsync(id);
            if (suspect == null)
            {
                return NotFound();
            }

            var model = new EditSuspectViewModel
            {
                Id = suspect.Id,
                CaseId = suspect.CaseId,
                Name = suspect.Name,
                Description = suspect.Description,
                LastSeenDate = suspect.LastSeenDate,
                LastKnownLocation = suspect.LastKnownLocation,
                Status = suspect.SuspectStatus,
                ThreatLevel = suspect.PossibleMotives
            };

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSuspect(Guid id)
        {
            var suspect = await _context.Suspect.FindAsync(id);
            if (suspect == null)
            {
                return Json(new { success = false, message = "Suspect not found" });
            }

            try
            {
                _context.Suspect.Remove(suspect);
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
