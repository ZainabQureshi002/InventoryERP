using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryERP.Web.Authorization;
using InventoryERP.Web.Data;
using InventoryERP.Web.Models;

namespace InventoryERP.Web.Controllers;

[Authorize]
public class SuppliersController : Controller
{
    private readonly ApplicationDbContext _context;

    public SuppliersController(ApplicationDbContext context) => _context = context;

    [Authorize(Policy = Permissions.Suppliers.View)]
    public async Task<IActionResult> Index()
    {
        return View(await _context.Suppliers.OrderBy(s => s.Name).ToListAsync());
    }

    [Authorize(Policy = Permissions.Suppliers.Create)]
    public IActionResult Create() => View(new Supplier());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Suppliers.Create)]
    public async Task<IActionResult> Create(Supplier supplier)
    {
        if (!ModelState.IsValid) return View(supplier);
        _context.Add(supplier);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Supplier created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = Permissions.Suppliers.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();
        return View(supplier);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Suppliers.Edit)]
    public async Task<IActionResult> Edit(int id, Supplier supplier)
    {
        if (id != supplier.Id) return NotFound();
        if (!ModelState.IsValid) return View(supplier);
        _context.Update(supplier);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Supplier updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Suppliers.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var supplier = await _context.Suppliers.Include(s => s.Products).FirstOrDefaultAsync(s => s.Id == id);
        if (supplier == null) return NotFound();
        if (supplier.Products.Any())
        {
            TempData["Error"] = "Cannot delete supplier that has products assigned.";
            return RedirectToAction(nameof(Index));
        }
        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Supplier deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
