using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryERP.Web.Authorization;
using InventoryERP.Web.Data;

namespace InventoryERP.Web.Controllers;

[Authorize]
public class ActivityLogController : Controller
{
    private const int PageSize = 25;

    private readonly ApplicationDbContext _context;

    public ActivityLogController(ApplicationDbContext context) => _context = context;

    [Authorize(Policy = Permissions.ActivityLog.View)]
    public async Task<IActionResult> Index(string? module, string? actionType, string? user, DateTime? from, DateTime? to, int page = 1)
    {
        var query = _context.ActivityLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(module)) query = query.Where(l => l.Module == module);
        if (!string.IsNullOrWhiteSpace(actionType)) query = query.Where(l => l.Action == actionType);
        if (!string.IsNullOrWhiteSpace(user)) query = query.Where(l => l.UserName.Contains(user));
        if (from.HasValue) query = query.Where(l => l.Timestamp >= from.Value);
        if (to.HasValue) query = query.Where(l => l.Timestamp < to.Value.AddDays(1));

        page = Math.Max(page, 1);
        var total = await query.CountAsync();
        var totalPages = Math.Max((int)Math.Ceiling(total / (double)PageSize), 1);
        page = Math.Min(page, totalPages);

        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.Modules = await _context.ActivityLogs.Select(l => l.Module).Distinct().OrderBy(m => m).ToListAsync();
        ViewBag.Actions = await _context.ActivityLogs.Select(l => l.Action).Distinct().OrderBy(a => a).ToListAsync();
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = total;
        ViewBag.Module = module;
        ViewBag.ActionType = actionType;
        ViewBag.User = user;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");

        return View(logs);
    }
}
