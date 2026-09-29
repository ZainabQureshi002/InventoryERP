using System.ComponentModel.DataAnnotations;

namespace InventoryERP.Web.Models;

public class Supplier
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ContactPerson { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(150), EmailAddress]
    public string? Email { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
