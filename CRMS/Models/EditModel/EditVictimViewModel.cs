using System;
using System.ComponentModel.DataAnnotations;

namespace CRMS.Models.EditModel
{
    public class EditVictimViewModel
    {
        public Guid? Id { get; set; }
        public Guid CaseId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        public string ContactNumber { get; set; }

        public string Contact { get; set; }
        public string ContactInfo { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "Incident date is required")]
        public DateTime IncidentDate { get; set; }

        public string InjurySeverity { get; set; }
        public string EmergencyContact { get; set; }
        public bool IsMinor { get; set; }
        public string MedicalHistory { get; set; }
        public string InjurySustained { get; set; }
        public string PropertyDamage { get; set; }
        public string CompensationClaimed { get; set; }
        public string Status { get; set; }
    }
} 