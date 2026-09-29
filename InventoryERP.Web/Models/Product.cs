using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryERP.Web.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CostPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SellingPrice { get; set; }

    public int Quantity { get; set; }

    public int ReorderLevel { get; set; } = 10;

    [StringLength(30)]
    public string Unit { get; set; } = "pcs";

    [StringLength(300)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
    public ICollection<SalesOrderItem> SalesOrderItems { get; set; } = new List<SalesOrderItem>();
    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();

    [NotMapped]
    public bool IsLowStock => Quantity <= ReorderLevel;

    [NotMapped]
    public decimal StockValue => Quantity * CostPrice;
}
