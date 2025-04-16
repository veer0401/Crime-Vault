using System;
using System.ComponentModel.DataAnnotations;

namespace CRMS.Models.EditModel
{
    public class EditCriminalViewModel
    {
        public Guid? Id { get; set; }
        public Guid CaseId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        public string Role { get; set; }
        public string Notes { get; set; }

    }
} 