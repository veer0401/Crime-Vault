using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRMS.Models.CreateModel
{
    public class CaseCreateViewModel
    {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Location { get; set; }
        public List<Guid> SelectedTeams { get; set; }
public Criminal Criminal { get; set; }

        // Criminal Information
        public List<CriminalInfo> Criminals { get; set; }
        public NewCriminal NewCriminal { get; set; }
        public List<SuspectInfo> Suspects { get; set; }

        // Evidence Information
        public List<EvidenceInfo> Evidences { get; set; }

        // Witness Information
        public List<WitnessInfo> Witnesses { get; set; }

        // Victim Information
        public List<VictimInfo> Victims { get; set; }

        public CaseCreateViewModel()
        {
            Criminals = new List<CriminalInfo>();
            Suspects = new List<SuspectInfo>();
            Evidences = new List<EvidenceInfo>();
            Witnesses = new List<WitnessInfo>();
            Victims = new List<VictimInfo>();
            SelectedTeams = new List<Guid>();
        }
    }

    public class CriminalInfo
    {
        public Guid CriminalId { get; set; }
        public string Role { get; set; }
        public string Notes { get; set; }
    }

    public class NewCriminal
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
    }

    public class SuspectInfo
    {
        public string Name { get; set; }
        public string LastKnownLocation { get; set; }
        public string Description { get; set; }
        public string PossibleMotives { get; set; }
        public string RelationshipToVictim { get; set; }
    }

    public class EvidenceInfo
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string StorageLocation { get; set; }
    }

    public class WitnessInfo
    {
        public string Name { get; set; }
        public string ContactNumber { get; set; }
        public string Statement { get; set; }
        public bool IsAnonymous { get; set; }
    }

    public class VictimInfo
    {
        public string Name { get; set; }
        public string ContactNumber { get; set; }
        public string Description { get; set; }
        public string InjurySeverity { get; set; }
    }
}