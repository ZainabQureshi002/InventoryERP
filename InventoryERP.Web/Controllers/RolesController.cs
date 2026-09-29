using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using InventoryERP.Web.Authorization;
using Perm = InventoryERP.Web.Authorization.Permissions;
using InventoryERP.Web.Data;

namespace InventoryERP.Web.Controllers;

[Authorize]
public class RolesController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesController(RoleManager<IdentityRole> roleManager) => _roleManager = roleManager;

    [Authorize(Policy = Perm.Roles.View)]
    public async Task<IActionResult> Index()
    {
        var roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
        var vm = new List<(string Id, string Name, int PermissionCount)>();
        foreach (var role in roles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            vm.Add((role.Id, role.Name!, claims.Count(c => c.Type == Perm.ClaimType)));
        }
        return View(vm);
    }

    [Authorize(Policy = Perm.Roles.Create)]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Perm.Roles.Create)]
    public async Task<IActionResult> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("", "Role name is required.");
            return View();
        }
        if (await _roleManager.RoleExistsAsync(name))
        {
            ModelState.AddModelError("", "A role with this name already exists.");
            return View();
        }

        await _roleManager.CreateAsync(new IdentityRole(name));
        TempData["Success"] = $"Role '{name}' created. Now assign its permissions.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = Perm.Roles.Edit)]
    public async Task<IActionResult> Permissions(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        var assigned = (await _roleManager.GetClaimsAsync(role))
            .Where(c => c.Type == Perm.ClaimType)
            .Select(c => c.Value)
            .ToHashSet();

        ViewBag.RoleName = role.Name;
        ViewBag.RoleId = role.Id;
        ViewBag.Modules = Perm.Modules;
        ViewBag.Assigned = assigned;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Perm.Roles.Edit)]
    public async Task<IActionResult> Permissions(string id, List<string> selectedPermissions)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        selectedPermissions ??= new List<string>();
        var validCodes = Perm.All().ToHashSet();
        selectedPermissions = selectedPermissions.Where(validCodes.Contains).ToList();

        var currentClaims = (await _roleManager.GetClaimsAsync(role))
            .Where(c => c.Type == Perm.ClaimType)
            .ToList();

        foreach (var claim in currentClaims.Where(c => !selectedPermissions.Contains(c.Value)))
            await _roleManager.RemoveClaimAsync(role, claim);

        var currentValues = currentClaims.Select(c => c.Value).ToHashSet();
        foreach (var code in selectedPermissions.Where(code => !currentValues.Contains(code)))
            await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(Perm.ClaimType, code));

        TempData["Success"] = $"Permissions updated for role '{role.Name}'.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Perm.Roles.Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        if (DbSeeder.Roles.Contains(role.Name))
        {
            TempData["Error"] = "Built-in roles (Admin/Manager/Staff) cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        await _roleManager.DeleteAsync(role);
        TempData["Success"] = "Role deleted.";
        return RedirectToAction(nameof(Index));
    }
}
