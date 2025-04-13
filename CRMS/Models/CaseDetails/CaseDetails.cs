using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMS.Models.CaseDetails
{
    public class CaseDetails
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CaseId { get; set; }

        [ForeignKey("CaseId")]
        public Case Case { get; set; }

        [StringLength(500)]
        public string Summary { get; set; }

        public string InvestigationDetails { get; set; }

        public string LegalNotes { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LastUpdated { get; set; }

        [StringLength(50)]
        public string LeadInvestigator { get; set; }

        public string AdditionalNotes { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Date)]
        public DateTime? ClosedDate { get; set; }

        public string Status { get; set; } = "Active";

        [StringLength(50)]
        public string CaseClassification { get; set; }
    }
}