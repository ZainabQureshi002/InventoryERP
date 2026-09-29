namespace InventoryERP.Web.Services;

public interface IActivityLogService
{
    Task LogAsync(string action, string module, string actionName, int? entityId, string description, string httpMethod);
}
