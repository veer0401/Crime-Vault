using System;

namespace CRMS.Models
{
    public class CaseCriminal
    {
        public Guid CaseId { get; set; }
        public Case Case { get; set; } = null!;

        public Guid CriminalId { get; set; }
        public Criminal Criminal { get; set; } = null!;

        public string Role { get; set; } = "Suspect";
        public DateTime AddedDate { get; set; } = DateTime.Now;
        public string? Notes { get; set; }
    }
}