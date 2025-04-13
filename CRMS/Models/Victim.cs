using System;

namespace CRMS.Models
{
    public class Victim
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactInfo => $"{ContactNumber} - {Address}"; // Combined contact information
        public string Description { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public string InjurySeverity { get; set; } = "Unknown";
        public string Status { get; set; } = "Active";
        public bool IsMinor { get; set; }
        public string? EmergencyContact { get; set; }
        public string? MedicalHistory { get; set; }
        public DateTime AddedDate { get; set; }
        public string? Statement { get; set; }
         public string? InjurySustained { get; set; }
    public string? PropertyDamage { get; set; }
    public string? CompensationClaimed { get; set; }
        // Foreign key for Case
        public Guid CaseId { get; set; }
        public Case Case { get; set; } = null!;
    }
}