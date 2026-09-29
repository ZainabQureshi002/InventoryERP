using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryERP.Web.Models;

public class SalesOrder
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string InvoiceNo { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Completed;

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }

    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
}

public class SalesOrderItem
{
    public int Id { get; set; }

    public int SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
}
