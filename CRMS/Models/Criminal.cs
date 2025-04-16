using System;
using System.Collections.Generic;

namespace CRMS.Models
{
    public class Criminal
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "At Large";
        public string GangAffiliation { get; set; } = string.Empty;
        public string Accomplices { get; set; } = string.Empty;
        public string WeaponUsed { get; set; } = string.Empty;
        public string PsychologicalProfile { get; set; } = string.Empty;
        public string LastKnownLocation { get; set; } = string.Empty;
        public decimal TotalBounty { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Address { get; set; } = string.Empty;
        public bool Caught { get; set; }
        public string ImageFilename { get; set; } = string.Empty;
        public string KnownHabits { get; set; } = string.Empty;

        // Navigation property for many-to-many relationship with Case
        public ICollection<CaseCriminal> CaseCriminals { get; set; } = new List<CaseCriminal>();
    }
}
