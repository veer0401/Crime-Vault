using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMS.Models
{
    public class FileModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string FilePath { get; set; }

        [Required]
        public string FileType { get; set; }

        [Required]
        public long FileSize { get; set; }

        [Required]
        public string UploadedByUserId { get; set; }

        [ForeignKey("UploadedByUserId")]
        public virtual Users UploadedByUser { get; set; }

        [Required]
        public DateTime UploadDate { get; set; }

        public string Description { get; set; }

        public virtual ICollection<FilePermission> Permissions { get; set; }
    }

    public class FilePermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FileId { get; set; }

        [Required]
        public string RequestedByUserId { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public bool IsApproved { get; set; }

        [ForeignKey("FileId")]
        public virtual FileModel File { get; set; }
    }
} 