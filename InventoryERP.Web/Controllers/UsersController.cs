using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using InventoryERP.Web.Authorization;
using InventoryERP.Web.Models;
using InventoryERP.Web.Models.ViewModels;

namespace InventoryERP.Web.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [Authorize(Policy = Permissions.Users.View)]
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.OrderBy(u => u.FullName).ToList();
        var vm = new List<(ApplicationUser User, IList<string> Roles)>();
        foreach (var user in users)
            vm.Add((user, await _userManager.GetRolesAsync(user)));
        return View(vm);
    }

    [Authorize(Policy = Permissions.Users.Create)]
    public IActionResult Create()
    {
        ViewBag.Roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
        return View(new UserCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Users.Create)]
    public async Task<IActionResult> Create(UserCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            return View(vm);
        }

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            FullName = vm.FullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            ViewBag.Roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            return View(vm);
        }

        if (vm.SelectedRoles.Any())
            await _userManager.AddToRolesAsync(user, vm.SelectedRoles);

        TempData["Success"] = $"User '{vm.FullName}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = Permissions.Users.Edit)]
    public async Task<IActionResult> Roles(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var vm = new UserRolesViewModel
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            SelectedRoles = (await _userManager.GetRolesAsync(user)).ToList()
        };
        ViewBag.Roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Users.Edit)]
    public async Task<IActionResult> Roles(string id, List<string> selectedRoles)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        selectedRoles ??= new List<string>();
        var currentRoles = await _userManager.GetRolesAsync(user);

        var toRemove = currentRoles.Except(selectedRoles).ToList();
        var toAdd = selectedRoles.Except(currentRoles).ToList();

        if (toRemove.Any()) await _userManager.RemoveFromRolesAsync(user, toRemove);
        if (toAdd.Any()) await _userManager.AddToRolesAsync(user, toAdd);

        TempData["Success"] = $"Roles updated for '{user.FullName}'.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Users.Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.DeleteAsync(user);
        TempData["Success"] = "User deleted.";
        return RedirectToAction(nameof(Index));
    }
}
