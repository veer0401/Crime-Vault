using System;

namespace CRMS.Models
{
    public class CaseTeam
    {
        public Guid CaseId { get; set; }
        public Case Case { get; set; } = null!;

        public Guid TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public DateTime AssignedDate { get; set; } = DateTime.Now;
        public string Role { get; set; } = "Primary";
        public string? Notes { get; set; }
    }
}