using System;
using System.Collections.Generic;

namespace CRMS.Models
{
    public class Case
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CaseId => Id.ToString().Substring(0, 8).ToUpper(); // Short ID for display
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime OpenDate { get; set; } = DateTime.Now;
        public DateTime? CloseDate { get; set; }
        public string Status { get; set; } = "Open";
        public string Priority { get; set; } = "Medium";
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public string Location { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Evidence> Evidences { get; set; } = new List<Evidence>();
        public ICollection<Witness> Witnesses { get; set; } = new List<Witness>();
        public ICollection<Victim> Victims { get; set; } = new List<Victim>();
        public ICollection<CaseCriminal> CaseCriminals { get; set; } = new List<CaseCriminal>();
        public ICollection<CaseTeam> CaseTeams { get; set; } = new List<CaseTeam>();
        public ICollection<Suspect> Suspects { get; set; } = new List<Suspect>();
    }
}