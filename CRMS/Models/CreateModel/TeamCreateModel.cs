using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRMS.Models.CreateModel
{
    public class TeamCreateModel
    {
        [Required]
        public string Name { get; set; }

        // Removed TeamCode as it will be auto-generated
        //public List<String> TeamMembers { get; set; } = new List<String>(); // Selected user IDs
    }
}
