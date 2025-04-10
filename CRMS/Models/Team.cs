using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRMS.Models
{
    public class Team
    {
        [Key]
        [ForeignKey("Team")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public string TeamCode { get; set; } // Unique code for joining the team

        // Foreign key to the Identity user (team leader)
        [Required]
        public string TeamLeaderId { get; set; }

        [ForeignKey("TeamLeaderId")]
        public virtual Users TeamLeader { get; set; }

        // Alias property for view compatibility
        public Users Leader => TeamLeader;

        // Many-to-many with cases (teams working on multiple cases)
        public virtual ICollection<CaseTeam> CaseTeams { get; set; } = new List<CaseTeam>();

        // Navigation to team members (separate from leader)
        public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    }

    
}
