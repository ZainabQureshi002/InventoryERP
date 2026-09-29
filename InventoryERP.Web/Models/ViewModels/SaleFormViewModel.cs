using System.ComponentModel.DataAnnotations;

namespace InventoryERP.Web.Models.ViewModels;

public class SaleFormViewModel
{
    public string InvoiceNo { get; set; } = string.Empty;

    [Required]
    public int CustomerId { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.Now;

    public string? Notes { get; set; }

    public List<SaleLineItem> Items { get; set; } = new();
}

public class SaleLineItem
{
    [Required]
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
