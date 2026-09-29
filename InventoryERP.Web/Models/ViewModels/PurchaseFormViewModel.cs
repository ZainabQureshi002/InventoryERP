using System.ComponentModel.DataAnnotations;

namespace InventoryERP.Web.Models.ViewModels;

public class PurchaseFormViewModel
{
    public string InvoiceNo { get; set; } = string.Empty;

    [Required]
    public int SupplierId { get; set; }

    public DateTime PurchaseDate { get; set; } = DateTime.Now;

    public string? Notes { get; set; }

    public List<PurchaseLineItem> Items { get; set; } = new();
}

public class PurchaseLineItem
{
    [Required]
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
}
