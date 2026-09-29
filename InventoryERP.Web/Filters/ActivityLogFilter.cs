using InventoryERP.Web.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InventoryERP.Web.Filters;

/// <summary>
/// Global action filter that records every controller action a signed-in user reaches -
/// list pages, opening a create/edit form, and the resulting create/edit/delete submit -
/// as an ActivityLog row. Applies to all controllers automatically; no per-controller code needed.
/// </summary>
public class ActivityLogFilter : IAsyncActionFilter
{
    private readonly IActivityLogService _activityLog;

    public ActivityLogFilter(IActivityLogService activityLog) => _activityLog = activityLog;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();

        var controllerName = context.RouteData.Values["controller"] as string ?? "";
        var actionName = context.RouteData.Values["action"] as string ?? "";

        if (controllerName.Equals("ActivityLog", StringComparison.OrdinalIgnoreCase)) return;
        if (executedContext.HttpContext.User.Identity?.IsAuthenticated != true) return;
        if (executedContext.Exception != null && !executedContext.ExceptionHandled) return;

        var httpMethod = context.HttpContext.Request.Method;
        var entityId = TryGetEntityId(context);

        var (category, verb) = Classify(actionName, httpMethod);
        var description = $"{verb} {controllerName}" + (entityId.HasValue ? $" #{entityId}" : "");

        await _activityLog.LogAsync(category, controllerName, actionName, entityId, description, httpMethod);
    }

    private static (string Category, string Verb) Classify(string actionName, string httpMethod)
    {
        var isPost = httpMethod == "POST";
        return actionName.ToLowerInvariant() switch
        {
            "index" => ("View", "Viewed"),
            "details" => ("View", "Viewed details of"),
            "create" => isPost ? ("Create", "Created") : ("View", "Opened create form for"),
            "edit" => isPost ? ("Edit", "Updated") : ("View", "Opened edit form for"),
            "delete" => ("Delete", "Deleted"),
            "adjust" => isPost ? ("Adjust", "Adjusted") : ("View", "Opened adjust form for"),
            "ledger" or "lowstock" => ("View", "Viewed"),
            _ => ("Other", isPost ? $"Submitted {actionName} on" : $"Opened {actionName} on"),
        };
    }

    private static int? TryGetEntityId(ActionExecutingContext context)
    {
        if (context.RouteData.Values.TryGetValue("id", out var routeId) && int.TryParse(routeId?.ToString(), out var parsedRouteId))
            return parsedRouteId;

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;
            var idProperty = argument.GetType().GetProperty("Id");
            if (idProperty?.PropertyType == typeof(int) && idProperty.GetValue(argument) is int value && value > 0)
                return value;
        }

        return null;
    }
}
