using Microsoft.AspNetCore.Identity;
using InventoryERP.Web.Authorization;

namespace InventoryERP.Web.Data;

/// <summary>
/// Syncs the master permission list (InventoryERP.Web.Authorization.Permissions) into the
/// database as role claims every time the application starts. Any permission that is new
/// (added to Permissions.Modules in code but not yet in the DB) gets created automatically -
/// nothing needs to be seeded by hand. Admin always receives every permission that exists.
/// Manager/Staff only receive the default set the FIRST time they are created (i.e. while
/// they still have zero permission claims), so an admin's later customization via the
/// Roles screen is never silently overwritten on the next restart.
/// </summary>
public static class PermissionSeeder
{
    private static readonly string[] ManagerDefaults =
    {
        Permissions.Dashboard.View,
        Permissions.Products.View, Permissions.Products.Create, Permissions.Products.Edit,
        Permissions.Categories.View, Permissions.Categories.Create, Permissions.Categories.Edit,
        Permissions.Suppliers.View, Permissions.Suppliers.Create, Permissions.Suppliers.Edit,
        Permissions.Customers.View, Permissions.Customers.Create, Permissions.Customers.Edit,
        Permissions.Purchases.View, Permissions.Purchases.Create,
        Permissions.Sales.View, Permissions.Sales.Create,
        Permissions.Stock.View, Permissions.Stock.Adjust,
        Permissions.Reports.View,
        Permissions.AiAssistant.View,
    };

    private static readonly string[] StaffDefaults =
    {
        Permissions.Dashboard.View,
        Permissions.Products.View,
        Permissions.Customers.View, Permissions.Customers.Create,
        Permissions.Sales.View, Permissions.Sales.Create,
        Permissions.Purchases.View,
        Permissions.Stock.View,
    };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var roleName in DbSeeder.Roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var existingClaims = (await roleManager.GetClaimsAsync(role))
                .Where(c => c.Type == Permissions.ClaimType)
                .Select(c => c.Value)
                .ToHashSet();

            if (roleName == "Admin")
            {
                // Admin always has every permission that currently exists in the app.
                foreach (var permission in Permissions.All())
                {
                    if (!existingClaims.Contains(permission))
                        await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(Permissions.ClaimType, permission));
                }
            }
            else if (existingClaims.Count == 0)
            {
                // First-time setup only - don't clobber an admin's later customization.
                var defaults = roleName == "Manager" ? ManagerDefaults : roleName == "Staff" ? StaffDefaults : Array.Empty<string>();
                foreach (var permission in defaults)
                {
                    await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(Permissions.ClaimType, permission));
                }
            }
        }
    }
}
