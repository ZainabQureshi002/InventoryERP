using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InventoryERP.Web.Models;

namespace InventoryERP.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>()
            .HasIndex(p => p.SKU)
            .IsUnique();

        builder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Product>()
            .HasOne(p => p.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<PurchaseOrder>()
            .HasOne(o => o.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(o => o.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PurchaseOrderItem>()
            .HasOne(i => i.PurchaseOrder)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PurchaseOrderItem>()
            .HasOne(i => i.Product)
            .WithMany(p => p.PurchaseOrderItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SalesOrder>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.SalesOrders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SalesOrderItem>()
            .HasOne(i => i.SalesOrder)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SalesOrderItem>()
            .HasOne(i => i.Product)
            .WithMany(p => p.SalesOrderItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StockTransaction>()
            .HasOne(t => t.Product)
            .WithMany(p => p.StockTransactions)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
