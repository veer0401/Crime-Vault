using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRMS.Models.CreateModel
{
    public class CaseDetailsViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string Status { get; set; } = "Open";

        [Required]
        public string Priority { get; set; } = "Medium";

        public string Location { get; set; }

        public List<int> SelectedTeams { get; set; } = new List<int>();

        // Collections
        public List<CaseCriminal> CaseCriminals { get; set; } = new List<CaseCriminal>();
        public List<Evidence> Evidences { get; set; } = new List<Evidence>();
        public List<Suspect> Suspects { get; set; } = new List<Suspect>();
        public List<Witness> Witnesses { get; set; } = new List<Witness>();
        public List<Victim> Victims { get; set; } = new List<Victim>();
    }
} 