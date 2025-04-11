using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;


namespace CRMS.Models
{
    public class Evidence
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Type { get; set; }

        public string StorageLocation { get; set; }

        public DateTime CollectionDate { get; set; }

        // Add these properties for file handling
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        
        // Navigation properties
        public Guid CaseId { get; set; }
        public Case Case { get; set; }
        
        // Helper property (not mapped to database)
        [NotMapped]
        public bool HasFile => !string.IsNullOrEmpty(FilePath);

        // Add this property for file upload handling
        [NotMapped]
        [Display(Name = "Evidence File")]
        public IFormFile FileUpload { get; set; }
    }
}