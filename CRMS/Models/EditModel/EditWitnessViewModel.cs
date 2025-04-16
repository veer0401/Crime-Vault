using System;
using System.ComponentModel.DataAnnotations;

namespace CRMS.Models.EditModel
{
    public class EditWitnessViewModel
    {
        public Guid? Id { get; set; }
        public Guid CaseId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        public string ContactNumber { get; set; }

        public string Contact { get; set; }
        public string Address { get; set; }

        [Required(ErrorMessage = "Statement is required")]
        public string Statement { get; set; }

        public string RelationToCase { get; set; }
        public string Credibility { get; set; }
        public int CredibilityRating { get; set; }
        public bool IsAnonymous { get; set; }
        public string Status { get; set; }
        public string ProtectionStatus { get; set; }
    }
} 