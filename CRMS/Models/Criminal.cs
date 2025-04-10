using System;

namespace CRMS.Models
{
    public class Criminal
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = " ";
        public string Alias { get; set; } = " ";
        public int Age { get; set; }

        public string? Gender { get; set; } // Fixed Enum Declaration

        public string Description { get; set; } = " ";
        public string ImageFilename { get; set; } = " ";
        public DateTime CreatedAt { get; set; }

        // Added properties
        public string? GangAffiliation { get; set; } // Nullable
        public string? Accomplices { get; set; } // Nullable (List of names)
        public string? WeaponUsed { get; set; } // Nullable
        public string? KnownHabits { get; set; } // Nullable
        public string? PsychologicalProfile { get; set; } // Nullable
        public string? Address { get; set; } // Nullable (Location)
        public string Status { get; set; } = "At Large"; // Criminal's current status
        public string LastKnownLocation { get; set; } = string.Empty; // Last known location of the criminal

        public bool Caught { get; set; }

    }

  
}
