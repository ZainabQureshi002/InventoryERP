using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryERP.Web.Authorization;
using InventoryERP.Web.Data;
using InventoryERP.Web.Models;

namespace InventoryERP.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _context;

    public CustomersController(ApplicationDbContext context) => _context = context;

    [Authorize(Policy = Permissions.Customers.View)]
    public async Task<IActionResult> Index()
    {
        return View(await _context.Customers.OrderBy(c => c.Name).ToListAsync());
    }

    [Authorize(Policy = Permissions.Customers.Create)]
    public IActionResult Create() => View(new Customer());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Customers.Create)]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (!ModelState.IsValid) return View(customer);
        _context.Add(customer);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Customer created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = Permissions.Customers.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Customers.Edit)]
    public async Task<IActionResult> Edit(int id, Customer customer)
    {
        if (id != customer.Id) return NotFound();
        if (!ModelState.IsValid) return View(customer);
        _context.Update(customer);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Customer updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Customers.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.Include(c => c.SalesOrders).FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null) return NotFound();
        if (customer.SalesOrders.Any())
        {
            TempData["Error"] = "Cannot delete customer that has sales orders.";
            return RedirectToAction(nameof(Index));
        }
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Customer deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
