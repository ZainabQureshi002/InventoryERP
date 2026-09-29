using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryERP.Web.Authorization;
using InventoryERP.Web.Data;

namespace InventoryERP.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context) => _context = context;

    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate = (to ?? DateTime.UtcNow).Date.AddDays(1);
        ViewBag.From = fromDate.ToString("yyyy-MM-dd");
        ViewBag.To = (toDate.AddDays(-1)).ToString("yyyy-MM-dd");

        var salesByProduct = (await _context.SalesOrderItems
            .Include(i => i.Product)
            .Where(i => i.SalesOrder!.SaleDate >= fromDate && i.SalesOrder.SaleDate < toDate)
            .GroupBy(i => i.Product!.Name)
            .Select(g => new { Product = g.Key, QtySold = g.Sum(i => i.Quantity), Revenue = g.Sum(i => i.Subtotal) })
            .ToListAsync())
            .OrderByDescending(x => x.Revenue)
            .ToList();
        ViewBag.SalesByProduct = salesByProduct;

        var purchaseBySupplier = (await _context.PurchaseOrders
            .Include(o => o.Supplier)
            .Where(o => o.PurchaseDate >= fromDate && o.PurchaseDate < toDate)
            .GroupBy(o => o.Supplier!.Name)
            .Select(g => new { Supplier = g.Key, Orders = g.Count(), Amount = g.Sum(o => o.TotalAmount) })
            .ToListAsync())
            .OrderByDescending(x => x.Amount)
            .ToList();
        ViewBag.PurchaseBySupplier = purchaseBySupplier;

        var stockValuation = (await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .Select(p => new { p.Name, p.SKU, Category = p.Category!.Name, p.Quantity, p.CostPrice, Value = p.Quantity * p.CostPrice })
            .ToListAsync())
            .OrderByDescending(x => x.Value)
            .ToList();
        ViewBag.StockValuation = stockValuation;
        ViewBag.TotalStockValue = stockValuation.Sum(x => x.Value);

        var totalRevenue = await _context.SalesOrders.Where(o => o.SaleDate >= fromDate && o.SaleDate < toDate).SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
        var totalSpend = await _context.PurchaseOrders.Where(o => o.PurchaseDate >= fromDate && o.PurchaseDate < toDate).SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.TotalSpend = totalSpend;

        return View();
    }

    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> DailySales(DateTime? date)
    {
        var day = (date ?? DateTime.Today).Date;
        var nextDay = day.AddDays(1);

        var orders = await _context.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.SaleDate >= day && o.SaleDate < nextDay)
            .OrderBy(o => o.SaleDate)
            .ToListAsync();

        ViewBag.Date = day.ToString("yyyy-MM-dd");
        ViewBag.PrevDate = day.AddDays(-1).ToString("yyyy-MM-dd");
        ViewBag.NextDate = day.AddDays(1).ToString("yyyy-MM-dd");
        ViewBag.TotalInvoices = orders.Count;
        ViewBag.TotalRevenue = orders.Sum(o => o.TotalAmount);
        ViewBag.TotalItemsSold = orders.SelectMany(o => o.Items).Sum(i => i.Quantity);

        var byProduct = orders.SelectMany(o => o.Items)
            .GroupBy(i => i.Product!.Name)
            .Select(g => new { Product = g.Key, Qty = g.Sum(i => i.Quantity), Revenue = g.Sum(i => i.Subtotal) })
            .OrderByDescending(x => x.Revenue)
            .ToList();
        ViewBag.ByProduct = byProduct;

        return View(orders);
    }
}
