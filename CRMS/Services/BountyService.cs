using CRMS.Models;

namespace CRMS.Services
{using CRMS.Data;
using Microsoft.EntityFrameworkCore;

    public class BountyService
    {
        public static int CalculateBountyPoints(string severity, string priority)
        {
            int severityScore = severity switch
            {
                "Fatal" => 5,
                "Severe" => 3,
                "Minor" => 1,
                _ => 0
            };

            int priorityMultiplier = priority switch
            {
                "High" => 3,
                "Medium" => 2,
                "Low" => 1,
                _ => 1
            };

            return (severityScore * 10) + (severityScore * priorityMultiplier * 5);
        }

        public static async Task UpdateCriminalBounty(AppDbContext context, Guid criminalId)
        {
            var totalBounty = await context.Bounties
                .Where(b => b.CriminalId == criminalId)
                .SumAsync(b => b.BountyPoints);

            var criminal = await context.Criminal.FindAsync(criminalId);
            if (criminal != null)
            {
                criminal.TotalBounty = totalBounty;
                await context.SaveChangesAsync();
            }
        }
    }
}