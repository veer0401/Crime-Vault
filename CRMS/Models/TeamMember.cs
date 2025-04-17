using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMS.Models
{
    public class TeamMember
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; } // FK to Identity user

        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        [Required]
        public Guid TeamId { get; set; } // FK to Team

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }

        // Timestamp for when the user joined the team
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    }

}
