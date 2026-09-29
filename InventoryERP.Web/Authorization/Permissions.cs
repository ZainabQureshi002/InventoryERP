namespace InventoryERP.Web.Authorization;

public static class Permissions
{
    public const string ClaimType = "Permission";

    public static readonly string[] StandardActions = { "View", "Create", "Edit", "Delete" };

    /// <summary>
    /// Module name -> actions available for that module. Edit this list to add new
    /// modules/actions; PermissionSeeder syncs whatever is here into the database
    /// every time the application starts, so nothing needs to be added manually.
    /// </summary>
    public static readonly Dictionary<string, string[]> Modules = new()
    {
        ["Dashboard"] = new[] { "View" },
        ["Products"] = StandardActions,
        ["Categories"] = StandardActions,
        ["Suppliers"] = StandardActions,
        ["Customers"] = StandardActions,
        ["Purchases"] = new[] { "View", "Create" },
        ["Sales"] = new[] { "View", "Create" },
        ["Stock"] = new[] { "View", "Adjust" },
        ["Reports"] = new[] { "View" },
        ["Users"] = StandardActions,
        ["Roles"] = StandardActions,
        ["ActivityLog"] = new[] { "View" },
        ["AiAssistant"] = new[] { "View" },
    };

    public static string Code(string module, string action) => $"{module}.{action}";

    /// <summary>All permission codes that currently exist in the application, e.g. "Products.Create".</summary>
    public static IEnumerable<string> All()
    {
        foreach (var (module, actions) in Modules)
            foreach (var action in actions)
                yield return Code(module, action);
    }

    public static class Products
    {
        public const string View = "Products.View";
        public const string Create = "Products.Create";
        public const string Edit = "Products.Edit";
        public const string Delete = "Products.Delete";
    }

    public static class Categories
    {
        public const string View = "Categories.View";
        public const string Create = "Categories.Create";
        public const string Edit = "Categories.Edit";
        public const string Delete = "Categories.Delete";
    }

    public static class Suppliers
    {
        public const string View = "Suppliers.View";
        public const string Create = "Suppliers.Create";
        public const string Edit = "Suppliers.Edit";
        public const string Delete = "Suppliers.Delete";
    }

    public static class Customers
    {
        public const string View = "Customers.View";
        public const string Create = "Customers.Create";
        public const string Edit = "Customers.Edit";
        public const string Delete = "Customers.Delete";
    }

    public static class Purchases
    {
        public const string View = "Purchases.View";
        public const string Create = "Purchases.Create";
    }

    public static class Sales
    {
        public const string View = "Sales.View";
        public const string Create = "Sales.Create";
    }

    public static class Stock
    {
        public const string View = "Stock.View";
        public const string Adjust = "Stock.Adjust";
    }

    public static class Reports
    {
        public const string View = "Reports.View";
    }

    public static class Dashboard
    {
        public const string View = "Dashboard.View";
    }

    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Edit = "Users.Edit";
        public const string Delete = "Users.Delete";
    }

    public static class Roles
    {
        public const string View = "Roles.View";
        public const string Create = "Roles.Create";
        public const string Edit = "Roles.Edit";
        public const string Delete = "Roles.Delete";
    }

    public static class ActivityLog
    {
        public const string View = "ActivityLog.View";
    }

    public static class AiAssistant
    {
        public const string View = "AiAssistant.View";
    }
}
