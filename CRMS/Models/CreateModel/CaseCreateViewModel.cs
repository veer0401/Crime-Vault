using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CRMS.Models.CreateModel
{
    public class CaseCreateViewModel
    {
        public CaseCreateViewModel()
        {
            Case = new Case
            {
                Title = string.Empty,
                Description = string.Empty,
                Status = "Open",
                Priority = "Medium",
                Location = string.Empty
            };
            SelectedTeams = new List<int>();
            
            // Initialize collections with at least one item
            Criminals = new List<Criminal> { new Criminal() };
            Evidences = new List<EvidenceViewModel> { new EvidenceViewModel() };
            Witnesses = new List<Witness> { new Witness() };
            Victims = new List<Victim> { new Victim() };
            Suspects = new List<Suspect> { new Suspect() };
        }

        public Guid Id { get; set; }

        public Case Case { get; set; }

        // CASE DETAILS
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public string Priority { get; set; }

        public string Location { get; set; }

        public List<int> SelectedTeams { get; set; }

        // CRIMINALS
        public List<Criminal> Criminals { get; set; }

        // EVIDENCE
        public List<EvidenceViewModel> Evidences { get; set; }

        // WITNESSES
        public List<Witness> Witnesses { get; set; }

        // VICTIMS
        public List<Victim> Victims { get; set; }

        // SUSPECTS
        public List<Suspect> Suspects { get; set; }
    }
}
