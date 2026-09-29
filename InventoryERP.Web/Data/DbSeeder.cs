using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InventoryERP.Web.Models;

namespace InventoryERP.Web.Data;

public static class DbSeeder
{
    public static readonly string[] Roles = { "Admin", "Manager", "Staff" };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        const string adminEmail = "admin@inventoryerp.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin@123");
        }
        if (!await userManager.IsInRoleAsync(admin, "Admin"))
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}
