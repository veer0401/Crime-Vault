using System;
using System.ComponentModel.DataAnnotations;

namespace CRMS.Models.EditModel
{
    public class EditSuspectViewModel
    {
        public Guid? Id { get; set; }
        public Guid CaseId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public string Alias { get; set; }

        [Required(ErrorMessage = "Age is required")]
        [Range(0, 120, ErrorMessage = "Age must be between 0 and 120")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }

        public string LastKnownLocation { get; set; }
        public string PossibleMotives { get; set; }
        public string RelationshipToVictim { get; set; }
        public string Alibi { get; set; }
        public bool IsPersonOfInterest { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public string ThreatLevel { get; set; }
    }
} 