using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryERP.Web.Models;

public class StockTransaction
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public StockTransactionType Type { get; set; }

    public int QuantityChange { get; set; }

    public int BalanceAfter { get; set; }

    [StringLength(100)]
    public string? Reference { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
