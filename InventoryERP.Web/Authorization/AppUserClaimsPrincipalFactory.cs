using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using InventoryERP.Web.Models;

namespace InventoryERP.Web.Authorization;

/// <summary>
/// ASP.NET Identity only puts role NAMES into the sign-in cookie by default - claims attached
/// to a role (our permission claims) are not automatically included. This factory adds them,
/// so [Authorize(Policy = "Products.Create")] can check User.HasClaim(...) directly without an
/// extra database lookup on every request.
/// </summary>
public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public AppUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        var roleNames = await UserManager.GetRolesAsync(user);
        foreach (var roleName in roleNames)
        {
            var role = await RoleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var roleClaims = await RoleManager.GetClaimsAsync(role);
            foreach (var claim in roleClaims.Where(c => c.Type == Permissions.ClaimType))
            {
                if (!identity.HasClaim(claim.Type, claim.Value))
                    identity.AddClaim(claim);
            }
        }

        return identity;
    }
}
