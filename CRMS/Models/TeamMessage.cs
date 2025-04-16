using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CRMS.Models
{
    public class TeamMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string SenderId { get; set; }

        [ForeignKey("SenderId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Users Sender { get; set; }

        [Required]
        public Guid TeamId { get; set; }

        [ForeignKey("TeamId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Team Team { get; set; }
    }
} 