using System;

namespace CRMS.Models
{
    public class Suspect
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Alias { get; set; }
        public int Age { get; set; }
        public string? Gender { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageFilename { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Suspect-specific properties
        public string SuspectStatus { get; set; } = "Under Investigation";
        public string? LastKnownLocation { get; set; }
        public string? PossibleMotives { get; set; }
        public string? PhysicalDescription { get; set; }
        public string? ContactInformation { get; set; }
        public string? Occupation { get; set; }
        public string? RelationshipToVictim { get; set; }
        public string? Alibi { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public bool IsPersonOfInterest { get; set; }

        // Navigation property for Case
        public Guid CaseId { get; set; }
        public Case Case { get; set; } = null!;
    }
}