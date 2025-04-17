using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMS.Models
{
    public class Bounty
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid CaseId { get; set; }
        public Case Case { get; set; }
        
        [Required]
        [Display(Name = "Criminal")]
        public Guid CriminalId { get; set; }
        
        [ForeignKey("CriminalId")]
        public Criminal Criminal { get; set; }
        
        [Required]
        public string Severity { get; set; } // Fatal, Severe, Minor
        public int SeverityScore { get; set; } // 5, 3, 1
        
        [Required]
        public string Priority { get; set; } // High, Medium, Low
        public int PriorityMultiplier { get; set; } // 3, 2, 1
        
        public int BountyPoints { get; set; }
        
        [Required]
        public DateTime CreatedDate { get; set; }
        
        [Required]
        public string CreatedById { get; set; }
        public Users CreatedBy { get; set; }
        
        [NotMapped]
        public bool CanEdit { get; set; }
        
        public int TotalBounty { get; set; } // Sum of all bounty points from all cases
    }
}