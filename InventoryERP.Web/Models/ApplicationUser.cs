using Microsoft.AspNetCore.Identity;

namespace InventoryERP.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
