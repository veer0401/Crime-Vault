using System.ComponentModel.DataAnnotations;

namespace CRMS.Models
{
    public class CriminalDTO
    {
        [Required]
        public string Name { get; set; } = " ";
       
        public string? Alias{ get; set; }
        
        public int? Age { get; set; }

        
        public string? Gender { get; set; } // Fixed Enum Declaration
        [Required]
        public string Description { get; set; } = " ";
        public IFormFile? ImageFile { get; set; }
        
        // Added properties
        public string? GangAffiliation { get; set; } // Nullable
        public string? Accomplices { get; set; } // Nullable (List of names)
        public string? WeaponUsed { get; set; } // Nullable
        public string? KnownHabits { get; set; } // Nullable
        public string? PsychologicalProfile { get; set; } // Nullable
        public string? Address { get; set; } // Nullable (Location)
        [Required]
        public bool Caught { get; set; }

    }
}
