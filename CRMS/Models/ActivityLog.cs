using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMS.Models
{
    public class ActivityLog
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public Users User { get; set; }

        [Required]
        public string Action { get; set; } // e.g., "View", "Create", "Update", "Delete"

        [Required]
        public string EntityType { get; set; } // e.g., "Team", "Message", "User"

        public string EntityId { get; set; } // ID of the entity being acted upon

        public string Details { get; set; } // Additional details about the action

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public string IPAddress { get; set; }

        public string UserAgent { get; set; } // Browser/device information
    }
} 