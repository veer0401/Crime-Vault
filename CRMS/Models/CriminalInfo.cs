using System;

namespace CRMS.Models
{
    public class CriminalInfo
    {
        public Guid CriminalId { get; set; }
        public string Role { get; set; }
        public string Notes { get; set; }
    }
}