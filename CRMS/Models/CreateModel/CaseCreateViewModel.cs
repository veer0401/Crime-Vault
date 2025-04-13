using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CRMS.Models.CreateModel
{
    public class CaseCreateViewModel
    {
        // CASE DETAILS
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public string Priority { get; set; }

        public string Location { get; set; }

        public List<int> SelectedTeams { get; set; }

        // CRIMINALS
        public List<Criminal> Criminals { get; set; } = new List<Criminal>();

        // EVIDENCE
        public List<EvidenceViewModel> Evidences { get; set; } = new List<EvidenceViewModel>();

        // WITNESSES
        public List<Witness> Witnesses { get; set; } = new List<Witness>();

        // VICTIMS
        public List<Victim> Victims { get; set; } = new List<Victim>();

        // SUSPECTS
        public List<Suspect> Suspects { get; set; } = new List<Suspect>();
    }

    public class EvidenceViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string StorageLocation { get; set; }
        public DateTime? CollectionDate { get; set; }
        public IFormFile FileUpload { get; set; }
    }
}
