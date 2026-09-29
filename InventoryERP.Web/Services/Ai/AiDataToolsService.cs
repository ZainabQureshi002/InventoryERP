using InventoryERP.Web.Data;
using InventoryERP.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryERP.Web.Services.Ai;

/// <summary>
/// The actual read-only database queries the AI assistant is allowed to run.
/// Each public method here is exposed to the LLM as a callable "tool" -
/// the model never sees raw SQL or the DbContext, only these fixed, safe queries.
/// </summary>
public class AiDataToolsService
{
    private readonly ApplicationDbContext _context;

    public AiDataToolsService(ApplicationDbContext context) => _context = context;

    public async Task<object> SalesSummaryAsync(DateTime from, DateTime to)
    {
        var (start, end) = (from.Date, to.Date);
        var orders = await _context.SalesOrders
            .Where(o => o.SaleDate.Date >= start && o.SaleDate.Date <= end && o.Status == OrderStatus.Completed)
            .ToListAsync();

        return new
        {
            fromDate = start.ToString("yyyy-MM-dd"),
            toDate = end.ToString("yyyy-MM-dd"),
            orderCount = orders.Count,
            totalAmount = orders.Sum(o => o.TotalAmount),
            distinctCustomers = orders.Select(o => o.CustomerId).Distinct().Count(),
        };
    }

    public async Task<object> PurchasesSummaryAsync(DateTime from, DateTime to)
    {
        var (start, end) = (from.Date, to.Date);
        var orders = await _context.PurchaseOrders
            .Where(o => o.PurchaseDate.Date >= start && o.PurchaseDate.Date <= end && o.Status == OrderStatus.Completed)
            .ToListAsync();

        return new
        {
            fromDate = start.ToString("yyyy-MM-dd"),
            toDate = end.ToString("yyyy-MM-dd"),
            orderCount = orders.Count,
            totalAmount = orders.Sum(o => o.TotalAmount),
            distinctSuppliers = orders.Select(o => o.SupplierId).Distinct().Count(),
        };
    }

    public async Task<object> TopSellingProductsAsync(DateTime from, DateTime to, int limit)
    {
        var (start, end) = (from.Date, to.Date);
        limit = Math.Clamp(limit, 1, 50);

        var items = await _context.SalesOrderItems
            .Where(i => i.SalesOrder!.SaleDate.Date >= start && i.SalesOrder.SaleDate.Date <= end
                        && i.SalesOrder.Status == OrderStatus.Completed)
            .GroupBy(i => new { i.ProductId, i.Product!.Name })
            .Select(g => new
            {
                product = g.Key.Name,
                quantitySold = g.Sum(i => i.Quantity),
                revenue = g.Sum(i => i.Subtotal),
            })
            .OrderByDescending(x => x.quantitySold)
            .Take(limit)
            .ToListAsync();

        return new { fromDate = start.ToString("yyyy-MM-dd"), toDate = end.ToString("yyyy-MM-dd"), topProducts = items };
    }

    public async Task<object> LowStockProductsAsync(int limit)
    {
        limit = Math.Clamp(limit, 1, 100);

        var products = await _context.Products
            .Where(p => p.IsActive && p.Quantity <= p.ReorderLevel)
            .OrderBy(p => p.Quantity)
            .Take(limit)
            .Select(p => new { p.Name, p.SKU, p.Quantity, p.ReorderLevel })
            .ToListAsync();

        return new { lowStockCount = products.Count, products };
    }

    public async Task<object> InventoryOverviewAsync()
    {
        return new
        {
            totalProducts = await _context.Products.CountAsync(p => p.IsActive),
            totalCategories = await _context.Categories.CountAsync(),
            totalSuppliers = await _context.Suppliers.CountAsync(),
            totalCustomers = await _context.Customers.CountAsync(),
            totalStockValue = await _context.Products.Where(p => p.IsActive).SumAsync(p => p.Quantity * p.CostPrice),
            lowStockCount = await _context.Products.CountAsync(p => p.IsActive && p.Quantity <= p.ReorderLevel),
        };
    }

    public async Task<object> RecentActivityAsync(int limit)
    {
        limit = Math.Clamp(limit, 1, 50);

        var logs = await _context.ActivityLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .Select(l => new { l.Timestamp, l.UserName, l.Action, l.Module, l.Description })
            .ToListAsync();

        return new { recentActivity = logs };
    }
}
