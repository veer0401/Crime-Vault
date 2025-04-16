using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CRMS.Models.EditModel
{
    public class EditCaseDetailsViewModel
    {
        public Guid Id { get; set; }

        // Case Details
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [Display(Name = "Case Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Case Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Case Status")]
        public string Status { get; set; } = "Open";

        [Required(ErrorMessage = "Priority is required")]
        [Display(Name = "Case Priority")]
        public string Priority { get; set; } = "Medium";

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        [Display(Name = "Case Location")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Assigned Teams")]
        public List<Guid> SelectedTeams { get; set; } = new List<Guid>();

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime UpdatedDate { get; set; }

        // Criminal Information
        public List<EditCriminalViewModel> Criminals { get; set; } = new List<EditCriminalViewModel>();

        // Evidence Information
        public List<EditEvidenceViewModel> Evidences { get; set; } = new List<EditEvidenceViewModel>();

        // Witness Information
        public List<EditWitnessViewModel> Witnesses { get; set; } = new List<EditWitnessViewModel>();

        // Victim Information
        public List<EditVictimViewModel> Victims { get; set; } = new List<EditVictimViewModel>();

        // Suspect Information
        public List<EditSuspectViewModel> Suspects { get; set; } = new List<EditSuspectViewModel>();
        public List<EditCriminalViewModel> Criminal { get; set; }
        //public List<Guid> SelectedTeams { get; set; }

        public EditCaseDetailsViewModel()
        {
            Status = "Open";
            Priority = "Medium";
            SelectedTeams = new List<Guid>();
        }
    }

    public class CriminalViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Status { get; set; }
        public string GangAffiliation { get; set; }
        public string Accomplices { get; set; }
        public string WeaponUsed { get; set; }
        public string PsychologicalProfile { get; set; }
        public string LastKnownLocation { get; set; }
        public decimal TotalBounty { get; set; }
    }

    public class EvidenceViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string StorageLocation { get; set; }
        public DateTime CollectionDate { get; set; }
        public IFormFile FileUpload { get; set; }
        public string FilePath { get; set; }
    }

    public class WitnessViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public string Statement { get; set; }
        public string RelationToCase { get; set; }
        public string Credibility { get; set; }
        public int CredibilityRating { get; set; }
        public bool IsAnonymous { get; set; }
    }

    public class VictimViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public DateTime IncidentDate { get; set; }
        public string InjurySeverity { get; set; }
        public string EmergencyContact { get; set; }
        public bool IsMinor { get; set; }
        public string MedicalHistory { get; set; }
        public string InjurySustained { get; set; }
        public string PropertyDamage { get; set; }
        public string CompensationClaimed { get; set; }
    }

    public class SuspectViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Description { get; set; }
        public string SuspectStatus { get; set; }
        public string LastKnownLocation { get; set; }
        public string PossibleMotives { get; set; }
        public string RelationshipToVictim { get; set; }
        public string Alibi { get; set; }
        public bool IsPersonOfInterest { get; set; }
    }
}