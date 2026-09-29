using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryERP.Web.Authorization;
using InventoryERP.Web.Data;
using InventoryERP.Web.Models;

namespace InventoryERP.Web.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context) => _context = context;

    [Authorize(Policy = Permissions.Categories.View)]
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Select(c => new { c.Id, c.Name, c.Description, ProductCount = c.Products.Count })
            .OrderBy(c => c.Name)
            .ToListAsync();
        ViewBag.Categories = categories;
        return View();
    }

    [Authorize(Policy = Permissions.Categories.Create)]
    public IActionResult Create() => View(new Category());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Categories.Create)]
    public async Task<IActionResult> Create(Category category)
    {
        if (!ModelState.IsValid) return View(category);
        _context.Add(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Category created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = Permissions.Categories.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Categories.Edit)]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id) return NotFound();
        if (!ModelState.IsValid) return View(category);
        _context.Update(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Category updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Categories.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return NotFound();
        if (category.Products.Any())
        {
            TempData["Error"] = "Cannot delete category that has products assigned.";
            return RedirectToAction(nameof(Index));
        }
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Category deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
