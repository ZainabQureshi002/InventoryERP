using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventoryERP.Web.Authorization;
using InventoryERP.Web.Data;
using InventoryERP.Web.Models;

namespace InventoryERP.Web.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context) => _context = context;

    [Authorize(Policy = Permissions.Products.View)]
    public async Task<IActionResult> Index(string? search, int? categoryId, string? stockFilter)
    {
        var query = _context.Products.Include(p => p.Category).Include(p => p.Supplier).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.SKU.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        if (stockFilter == "low")
            query = query.Where(p => p.Quantity <= p.ReorderLevel);
        else if (stockFilter == "out")
            query = query.Where(p => p.Quantity == 0);

        ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.StockFilter = stockFilter;

        return View(await query.OrderBy(p => p.Name).ToListAsync());
    }

    [Authorize(Policy = Permissions.Products.View)]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category).Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();

        var transactions = await _context.StockTransactions
            .Where(t => t.ProductId == id)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .ToListAsync();
        ViewBag.Transactions = transactions;

        return View(product);
    }

    private async Task PopulateDropdowns(Product? product = null)
    {
        ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", product?.CategoryId);
        ViewBag.Suppliers = new SelectList(await _context.Suppliers.OrderBy(s => s.Name).ToListAsync(), "Id", "Name", product?.SupplierId);
    }

    [Authorize(Policy = Permissions.Products.Create)]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Products.Create)]
    public async Task<IActionResult> Create(Product product)
    {
        if (await _context.Products.AnyAsync(p => p.SKU == product.SKU))
            ModelState.AddModelError(nameof(product.SKU), "SKU already exists.");

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(product);
            return View(product);
        }

        product.CreatedAt = DateTime.UtcNow;
        _context.Add(product);
        await _context.SaveChangesAsync();

        if (product.Quantity > 0)
        {
            _context.StockTransactions.Add(new StockTransaction
            {
                ProductId = product.Id,
                Type = StockTransactionType.AdjustmentAdd,
                QuantityChange = product.Quantity,
                BalanceAfter = product.Quantity,
                Reference = "Opening Stock",
                Notes = "Stock set on product creation",
                CreatedBy = User.Identity?.Name,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        TempData["Success"] = "Product created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = Permissions.Products.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        await PopulateDropdowns(product);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Products.Edit)]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id) return NotFound();

        if (await _context.Products.AnyAsync(p => p.SKU == product.SKU && p.Id != id))
            ModelState.AddModelError(nameof(product.SKU), "SKU already exists.");

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(product);
            return View(product);
        }

        var existing = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (existing == null) return NotFound();

        product.CreatedAt = existing.CreatedAt;
        _context.Update(product);

        if (product.Quantity != existing.Quantity)
        {
            var diff = product.Quantity - existing.Quantity;
            _context.StockTransactions.Add(new StockTransaction
            {
                ProductId = product.Id,
                Type = diff > 0 ? StockTransactionType.AdjustmentAdd : StockTransactionType.AdjustmentRemove,
                QuantityChange = diff,
                BalanceAfter = product.Quantity,
                Reference = "Manual Edit",
                Notes = "Quantity changed via product edit form",
                CreatedBy = User.Identity?.Name,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Product updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Products.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products
            .Include(p => p.PurchaseOrderItems)
            .Include(p => p.SalesOrderItems)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();

        if (product.PurchaseOrderItems.Any() || product.SalesOrderItems.Any())
        {
            product.IsActive = false;
            TempData["Success"] = "Product has transaction history, so it was deactivated instead of deleted.";
        }
        else
        {
            _context.Products.Remove(product);
            TempData["Success"] = "Product deleted successfully.";
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
