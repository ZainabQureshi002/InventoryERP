namespace InventoryERP.Web.Models;

public class ActivityLog
{
    public int Id { get; set; }

    public string? UserId { get; set; }
    public string UserName { get; set; } = "Unknown";

    /// <summary>Broad category: View, Create, Edit, Delete, Adjust, Other.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Controller name, e.g. "Categories".</summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>Controller action name, e.g. "Edit".</summary>
    public string ActionName { get; set; } = string.Empty;

    public int? EntityId { get; set; }

    /// <summary>Human-readable summary, e.g. "Updated Categories #5".</summary>
    public string Description { get; set; } = string.Empty;

    public string HttpMethod { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
