using System.Security.Claims;
using InventoryERP.Web.Data;
using InventoryERP.Web.Models;
using Microsoft.AspNetCore.Http;

namespace InventoryERP.Web.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ActivityLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string module, string actionName, int? entityId, string description, string httpMethod)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        var log = new ActivityLog
        {
            UserId = user?.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName = user?.Identity?.Name ?? "Unknown",
            Action = action,
            Module = module,
            ActionName = actionName,
            EntityId = entityId,
            Description = description,
            HttpMethod = httpMethod,
            IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
            Timestamp = DateTime.UtcNow,
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
