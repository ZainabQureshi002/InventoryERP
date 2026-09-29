using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventoryERP.Web.Authorization;
using InventoryERP.Web.Data;
using InventoryERP.Web.Models;

namespace InventoryERP.Web.Controllers;

[Authorize]
public class StockController : Controller
{
    private readonly ApplicationDbContext _context;

    public StockController(ApplicationDbContext context) => _context = context;

    [Authorize(Policy = Permissions.Stock.View)]
    public async Task<IActionResult> Ledger(int? productId)
    {
        var query = _context.StockTransactions.Include(t => t.Product).AsQueryable();
        if (productId.HasValue)
            query = query.Where(t => t.ProductId == productId);

        ViewBag.Products = new SelectList(await _context.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", productId);

        var transactions = await query.OrderByDescending(t => t.CreatedAt).Take(300).ToListAsync();
        return View(transactions);
    }

    [Authorize(Policy = Permissions.Stock.View)]
    public async Task<IActionResult> LowStock()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.Quantity <= p.ReorderLevel)
            .OrderBy(p => p.Quantity)
            .ToListAsync();
        return View(products);
    }

    [Authorize(Policy = Permissions.Stock.Adjust)]
    public async Task<IActionResult> Adjust()
    {
        ViewBag.Products = new SelectList(await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Stock.Adjust)]
    public async Task<IActionResult> Adjust(int productId, string adjustmentType, int quantity, string? reason)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null || quantity <= 0)
        {
            TempData["Error"] = "Invalid product or quantity.";
            return RedirectToAction(nameof(Adjust));
        }

        int change;
        StockTransactionType type;
        if (adjustmentType == "add")
        {
            change = quantity;
            type = StockTransactionType.AdjustmentAdd;
        }
        else
        {
            if (quantity > product.Quantity)
            {
                TempData["Error"] = $"Cannot remove {quantity} units, only {product.Quantity} in stock.";
                return RedirectToAction(nameof(Adjust));
            }
            change = -quantity;
            type = StockTransactionType.AdjustmentRemove;
        }

        product.Quantity += change;

        _context.StockTransactions.Add(new StockTransaction
        {
            ProductId = product.Id,
            Type = type,
            QuantityChange = change,
            BalanceAfter = product.Quantity,
            Reference = "Manual Adjustment",
            Notes = reason,
            CreatedBy = User.Identity?.Name,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Stock adjusted for {product.Name}. New quantity: {product.Quantity}.";
        return RedirectToAction(nameof(Adjust));
    }
}
