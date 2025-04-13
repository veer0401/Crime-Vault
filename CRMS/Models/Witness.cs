using System;

namespace CRMS.Models
{
    public class Witness
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactInfo => $"{ContactNumber} - {Address}"; // Combined contact information
        public string Statement { get; set; } = string.Empty;
        public DateTime StatementDate { get; set; } = DateTime.Now;
        public string RelationToCase { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; }
        public string? ProtectionStatus { get; set; }
        public string Credibility { get; set; } = "Unknown";
        public int CredibilityRating { get; set; }

        // public DateTime AddedDate { get; set; }

        // Foreign key for Case
        public Guid CaseId { get; set; }
        public Case Case { get; set; } = null!;
    }
}