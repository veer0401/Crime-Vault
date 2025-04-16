using System;
using Microsoft.AspNetCore.Http;

namespace CRMS.Models.CreateModel
{
    public class EvidenceViewModel
    {
        public Guid Id { get; set; }
        public Guid CaseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string StorageLocation { get; set; }
        public DateTime CollectionDate { get; set; }
        public IFormFile FileUpload { get; set; }
        public string FilePath { get; set; }
    }
}