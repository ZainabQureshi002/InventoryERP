using System.ComponentModel.DataAnnotations;

namespace InventoryERP.Web.Models;

public class Customer
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(150), EmailAddress]
    public string? Email { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
}
