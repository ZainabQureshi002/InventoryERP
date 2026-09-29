using System.ComponentModel.DataAnnotations;

namespace InventoryERP.Web.Models.ViewModels;

public class UserCreateViewModel
{
    [Required, StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    public List<string> SelectedRoles { get; set; } = new();
}

public class UserRolesViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> SelectedRoles { get; set; } = new();
}
