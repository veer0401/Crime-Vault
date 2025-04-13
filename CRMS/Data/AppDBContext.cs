using CRMS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;

namespace CRMS.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ✅ DbSet should be inside the class, not outside
        public DbSet<Criminal> Criminal { get; set; }
        public DbSet<Suspect> Suspect { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<CRMS.Models.Evidence> Evidence { get; set; }
        public DbSet<Victim> Victims { get; set; }
        public DbSet<Witness> Witnesses { get; set; }
        public DbSet<CaseCriminal> CaseCriminals { get; set; }
        public DbSet<CaseTeam> CaseTeams { get; set; }
        public DbSet<Bounty> Bounties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent cascading delete for TeamMember -> Team
            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.Team)
                .WithMany(t => t.TeamMembers)
                .HasForeignKey(tm => tm.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent cascading delete for TeamMember -> User
            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.User)
                .WithMany(u => u.TeamMembers)
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure many-to-many relationship between Case and Criminal
            modelBuilder.Entity<CaseCriminal>()
                .HasKey(cc => new { cc.CaseId, cc.CriminalId });

            modelBuilder.Entity<CaseCriminal>()
                .HasOne(cc => cc.Case)
                .WithMany(c => c.CaseCriminals)
                .HasForeignKey(cc => cc.CaseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CaseCriminal>()
                .HasOne(cc => cc.Criminal)
                .WithMany()
                .HasForeignKey(cc => cc.CriminalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure many-to-many relationship between Case and Team
            modelBuilder.Entity<CaseTeam>()
                .HasKey(ct => new { ct.CaseId, ct.TeamId });

            modelBuilder.Entity<CaseTeam>()
                .HasOne(ct => ct.Case)
                .WithMany(c => c.CaseTeams)
                .HasForeignKey(ct => ct.CaseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CaseTeam>()
                .HasOne(ct => ct.Team)
                .WithMany(t => t.CaseTeams)
                .HasForeignKey(ct => ct.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure one-to-many relationships for Case
            // Add this configuration for Evidence
            modelBuilder.Entity<CRMS.Models.Evidence>()
                .HasOne(e => e.Case)
                .WithMany(c => c.Evidences)
                .HasForeignKey(e => e.CaseId);
        }
    }

}

//public DbSet<Evidences> Evidence { get; set; }
// public DbSet<Suspect> Suspect { get; set; }
//public DbSet<Team> Teams { get; set; }
//public DbSet<Message> Messages { get; set; }

//public DbSet<Criminal> Criminals { get; set; }
//public DbSet<Bounty> Bounty { get; set; }
//public DbSet<ConfidentialFile> ConfidentialFiles { get; set; }
////public DbSet<Tasks> Tasks { get; set; }
//     protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            //    modelBuilder.Entity<Tasks>()
//            //.HasOne(t => t.AssignedBy)
//            //.WithMany()
//            //.HasForeignKey(t => t.AssignedById)
//            //.OnDelete(DeleteBehavior.NoAction);

//            //    modelBuilder.Entity<Tasks>()
//            //        .HasOne(t => t.AssignedTo)
//            //        .WithMany()
//            //        .HasForeignKey(t => t.AssignedToId)
//            //        .OnDelete(DeleteBehavior.NoAction);

//            modelBuilder.Entity<Bounty>()
//            .HasOne(b => b.Criminal)
//            .WithMany()
//            .HasForeignKey(b => b.CriminalId)
//            .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete

//            modelBuilder.Entity<Bounty>()
//                .HasOne(b => b.AssignedBy)
//                .WithMany()
//                .HasForeignKey(b => b.AssignedById)
//                .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete

//            modelBuilder.Entity<Bounty>()
//                .Property(b => b.Amount)
//                .HasPrecision(18, 4);

//            modelBuilder.Entity<Team>()
//                .HasOne(t => t.Leader)
//                .WithMany(u => u.TeamsLed)
//                .HasForeignKey(t => t.LeaderId)
//                .OnDelete(DeleteBehavior.Restrict);  // Prevent cascading delete

//            modelBuilder.Entity<Team>()
//                .HasMany(t => t.Members)
//                .WithMany(u => u.Teams)
//                .UsingEntity<Dictionary<string, object>>(
//                    "TeamMembers",
//                    j => j.HasOne<Users>().WithMany().HasForeignKey("UserId"),
//                    j => j.HasOne<Team>().WithMany().HasForeignKey("TeamId"),
//                    j => j.ToTable("TeamMembers"));

//            // **Define Relationships for Messages Table**
//            modelBuilder.Entity<Message>()
//                .HasOne(m => m.Sender)
//                .WithMany(u => u.SentMessages)
//                .HasForeignKey(m => m.SenderId)
//                .OnDelete(DeleteBehavior.Restrict);

//            modelBuilder.Entity<Message>()
//                .HasOne(m => m.Receiver)
//                .WithMany(u => u.ReceivedMessages)
//                .HasForeignKey(m => m.ReceiverId)
//                .OnDelete(DeleteBehavior.Restrict);

//            base.OnModelCreating(modelBuilder);
//        }
//    }
//}