using System.ComponentModel.DataAnnotations;

namespace InventoryERP.Web.Models;

public class Category
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
