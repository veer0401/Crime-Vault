using CRMS.Data;
using CRMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CRMS.Services
{
    public interface IActivityLogService
    {
        Task LogActivityAsync(string action, string entityType, string entityId, string details);
    }

    public class ActivityLogService : IActivityLogService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<Users> _userManager;

        public ActivityLogService(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<Users> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task LogActivityAsync(string action, string entityType, string entityId, string details)
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (user == null) return;

            var log = new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                Timestamp = DateTime.UtcNow,
                IPAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString()
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
} 