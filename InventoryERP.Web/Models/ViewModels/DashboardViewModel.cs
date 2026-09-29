namespace InventoryERP.Web.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public decimal TotalStockValue { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public decimal SalesToday { get; set; }
    public decimal SalesThisMonth { get; set; }
    public decimal PurchasesThisMonth { get; set; }
    public int TotalCategories { get; set; }
    public int TotalSuppliers { get; set; }
    public int TotalCustomers { get; set; }

    public List<string> TrendLabels { get; set; } = new();
    public List<decimal> SalesTrend { get; set; } = new();
    public List<decimal> PurchaseTrend { get; set; } = new();

    public List<string> TopProductNames { get; set; } = new();
    public List<int> TopProductQuantities { get; set; } = new();

    public List<string> CategoryNames { get; set; } = new();
    public List<int> CategoryStockCounts { get; set; } = new();

    public List<Product> LowStockProducts { get; set; } = new();
    public List<SalesOrder> RecentSales { get; set; } = new();
}
