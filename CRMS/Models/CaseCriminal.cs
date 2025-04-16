using System;

namespace CRMS.Models
{
    public class CaseCriminal
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CaseId { get; set; }
        public Guid CriminalId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Case Case { get; set; }
        public Criminal Criminal { get; set; }
    }
}