using System;

namespace CRMS.Models
{
    public class Evidence
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime CollectionDate { get; set; } = DateTime.Now;
        public string CollectedBy { get; set; } = string.Empty;
        public string StorageLocation { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string? FileUrl { get; set; }

        // Foreign key for Case
        public Guid CaseId { get; set; }
        public Case Case { get; set; } = null!;
    }
}