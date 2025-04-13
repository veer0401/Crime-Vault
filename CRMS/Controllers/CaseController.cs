using Microsoft.AspNetCore.Mvc;
using CRMS.Data;
using CRMS.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using CRMS.Models.CreateModel;

public class CaseController : Controller
{
    private readonly AppDbContext _context;

    public CaseController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Create()
    {
        try
        {
            // Ensure Teams are loaded
            var teams = _context.Teams.ToList();
            ViewBag.Teams = teams ?? new List<Team>();

            // Load criminals
            var criminals = _context.Criminal.ToList();
            ViewBag.Criminals = criminals ?? new List<Criminal>();

            var model = new CaseCreateViewModel
            {
                Title = string.Empty,
                Description = string.Empty,
                Status = "Open",
                Priority = "Medium",
                Location = string.Empty,
                SelectedTeams = new List<int>(),
                Criminals = criminals ?? new List<Criminal>(),
                Evidences = new List<EvidenceViewModel>(),
                Witnesses = new List<Witness>(),
                Victims = new List<Victim>(),
                Suspects = new List<Suspect>()
            };

            // Initialize the collections to prevent null reference exceptions
            if (model.Evidences == null) model.Evidences = new List<EvidenceViewModel>();
            if (model.Witnesses == null) model.Witnesses = new List<Witness>();
            if (model.Victims == null) model.Victims = new List<Victim>();
            if (model.Suspects == null) model.Suspects = new List<Suspect>();

            return View(model);
        }
        catch (Exception ex)
        {
            // Log the error
            ModelState.AddModelError("", "An error occurred while loading the form. Please try again.");
            return View(new CaseCreateViewModel
            {
                Title = string.Empty,
                Description = string.Empty,
                Status = "Open",
                Priority = "Medium",
                Location = string.Empty,
                SelectedTeams = new List<int>(),
                Criminals = new List<Criminal>(),
                Evidences = new List<EvidenceViewModel>(),
                Witnesses = new List<Witness>(),
                Victims = new List<Victim>(),
                Suspects = new List<Suspect>()
            });
        }
    }    // CASE DETAILS

    [HttpPost]
public async Task<IActionResult> CreateCaseDetails(CaseCreateViewModel model, List<string> selectedTeams)
{
    if (ModelState.IsValid)
    {
        var newCase = new Case
        {
            Title = model.Title,
            Description = model.Description,
            Status = model.Status,
            Priority = model.Priority,
            Location = model.Location,
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now
        };

        _context.Cases.Add(newCase);
        await _context.SaveChangesAsync();

        // Check if selectedTeams is not null or empty
        if (selectedTeams != null && selectedTeams.Any())
        {
            foreach (var teamIdString in selectedTeams)
            {
                // Try parsing the teamIdString (which is a string) into a Guid
                if (Guid.TryParse(teamIdString, out Guid teamId))
                {
                    _context.CaseTeams.Add(new CaseTeam
                    {
                        CaseId = newCase.Id,
                        TeamId = teamId
                    });
                }
                else
                {
                    // Handle invalid Guid and add an error to the ModelState
                    ModelState.AddModelError("selectedTeams", $"Invalid team ID: {teamIdString}");
                    return View("Create", model);
                }
            }
            await _context.SaveChangesAsync();
        }

        TempData["CaseId"] = newCase.Id;
        return RedirectToAction("Create"); // or show a success message
    }

    return View("Create", model);
}

    // CRIMINAL INFO
    [HttpPost]
    public async Task<IActionResult> CreateCriminalInfo(List<Guid> selectedCriminals, List<Criminal> criminals, Guid caseId)
    {
        // Add selected existing criminals to the case
        if (selectedCriminals != null && selectedCriminals.Any())
        {
            foreach (var criminalId in selectedCriminals)
            {
                var caseCriminal = new CaseCriminal
                {
                    CaseId = caseId,
                    CriminalId = criminalId
                };
                _context.CaseCriminals.Add(caseCriminal);
            }
        }

        // Add new criminals to the database and link them to the case
        if (criminals != null && criminals.Any())
        {
            foreach (var criminal in criminals)
            {
                // Save new criminal
                _context.Criminal.Add(criminal);
                await _context.SaveChangesAsync();

                // Create CaseCriminal link
                var caseCriminal = new CaseCriminal
                {
                    CaseId = caseId,
                    CriminalId = criminal.Id // Make sure Criminal.Id is populated after SaveChanges
                };
                _context.CaseCriminals.Add(caseCriminal);
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Create");
    }

    // EVIDENCE
    [HttpPost]
    public async Task<IActionResult> CreateEvidence(List<Evidence> evidences, Guid caseId)
    {
        foreach (var ev in evidences)
        {
            ev.CaseId = caseId;
            _context.Evidence.Add(ev);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Create");
    }

    // WITNESSES
    [HttpPost]
    public async Task<IActionResult> CreateWitness(List<Witness> witnesses, Guid caseId)
    {
        foreach (var witness in witnesses)
        {
            witness.CaseId = caseId;
            _context.Witnesses.Add(witness);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Create");
    }

    // VICTIMS
    [HttpPost]
    public async Task<IActionResult> CreateVictim(List<Victim> victims, Guid caseId)
    {
        foreach (var victim in victims)
        {
            victim.CaseId = caseId;
            _context.Victims.Add(victim);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Create");
    }

    // SUSPECTS
    [HttpPost]
    public async Task<IActionResult> CreateSuspect(List<Suspect> suspects, Guid caseId)
    {
        foreach (var suspect in suspects)
        {
            suspect.CaseId = caseId;
            _context.Suspect.Add(suspect);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Create");
    }
}
