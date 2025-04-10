using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace CRMS.Models
{
    public class Users : IdentityUser
    {
        [Required]
        public string FullName { get; set; } = " ";

        //public string NickName { get; set; } = " ";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Track first login status
        public bool FirstLogin { get; set; } = false;  // Default: false (force password reset)

        public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    }
}
