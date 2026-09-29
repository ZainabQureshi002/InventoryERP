using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryERP.Web.Models;

public class PurchaseOrder
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string InvoiceNo { get; set; } = string.Empty;

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Completed;

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }

    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}

public class PurchaseOrderItem
{
    public int Id { get; set; }

    public int PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
}
