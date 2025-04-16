using System;
using System.ComponentModel.DataAnnotations;

namespace CRMS.Models.EditModel
{
    public class EditEvidenceViewModel
    {
        public Guid? Id { get; set; }
        public Guid CaseId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Storage location is required")]
        public string StorageLocation { get; set; }

        [Required(ErrorMessage = "Collection date is required")]
        public DateTime CollectionDate { get; set; }

        public string FilePath { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public string ContentType { get; set; }
    }
} 