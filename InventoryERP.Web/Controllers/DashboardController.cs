using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryERP.Web.Authorization;
using InventoryERP.Web.Data;
using InventoryERP.Web.Models.ViewModels;

namespace InventoryERP.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context) => _context = context;

    [Authorize(Policy = Permissions.Dashboard.View)]
    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var products = await _context.Products.Where(p => p.IsActive).ToListAsync();

        var vm = new DashboardViewModel
        {
            TotalProducts = products.Count,
            TotalStockValue = products.Sum(p => p.Quantity * p.CostPrice),
            LowStockCount = products.Count(p => p.Quantity <= p.ReorderLevel && p.Quantity > 0),
            OutOfStockCount = products.Count(p => p.Quantity == 0),
            TotalCategories = await _context.Categories.CountAsync(),
            TotalSuppliers = await _context.Suppliers.CountAsync(),
            TotalCustomers = await _context.Customers.CountAsync(),
            SalesToday = await _context.SalesOrders.Where(o => o.SaleDate >= today).SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
            SalesThisMonth = await _context.SalesOrders.Where(o => o.SaleDate >= monthStart).SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
            PurchasesThisMonth = await _context.PurchaseOrders.Where(o => o.PurchaseDate >= monthStart).SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
        };

        // 14-day sales vs purchase trend
        var start = today.AddDays(-13);
        var salesByDay = await _context.SalesOrders
            .Where(o => o.SaleDate >= start)
            .GroupBy(o => o.SaleDate.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(o => o.TotalAmount) })
            .ToListAsync();
        var purchasesByDay = await _context.PurchaseOrders
            .Where(o => o.PurchaseDate >= start)
            .GroupBy(o => o.PurchaseDate.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(o => o.TotalAmount) })
            .ToListAsync();

        for (var d = start; d <= today; d = d.AddDays(1))
        {
            vm.TrendLabels.Add(d.ToString("dd MMM"));
            vm.SalesTrend.Add(salesByDay.FirstOrDefault(x => x.Date == d)?.Total ?? 0);
            vm.PurchaseTrend.Add(purchasesByDay.FirstOrDefault(x => x.Date == d)?.Total ?? 0);
        }

        // Top 5 selling products (by quantity sold, all time)
        var topProducts = await _context.SalesOrderItems
            .Include(i => i.Product)
            .GroupBy(i => i.Product!.Name)
            .Select(g => new { Name = g.Key, Qty = g.Sum(i => i.Quantity) })
            .OrderByDescending(x => x.Qty)
            .Take(5)
            .ToListAsync();
        vm.TopProductNames = topProducts.Select(p => p.Name).ToList();
        vm.TopProductQuantities = topProducts.Select(p => p.Qty).ToList();

        // Category-wise stock distribution
        var categoryStock = await _context.Products
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .GroupBy(p => p.Category!.Name)
            .Select(g => new { Name = g.Key, Qty = g.Sum(p => p.Quantity) })
            .ToListAsync();
        vm.CategoryNames = categoryStock.Select(c => c.Name).ToList();
        vm.CategoryStockCounts = categoryStock.Select(c => c.Qty).ToList();

        vm.LowStockProducts = products.Where(p => p.Quantity <= p.ReorderLevel).OrderBy(p => p.Quantity).Take(8).ToList();
        vm.RecentSales = await _context.SalesOrders.Include(o => o.Customer).OrderByDescending(o => o.SaleDate).Take(8).ToListAsync();

        return View(vm);
    }
}
