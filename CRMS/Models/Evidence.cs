using System;

namespace CRMS.Models
{
    public class Evidence
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string StorageLocation { get; set; }
        public DateTime CollectionDate { get; set; }
        public string FilePath { get; set; }
        
        // Foreign key for Case
        public Guid CaseId { get; set; }
        
        // Navigation property for Case
        public Case Case { get; set; }
    }
}