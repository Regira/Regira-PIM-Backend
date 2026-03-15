using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Webshop.Identity.Models;

namespace Webshop.Identity.Services;

public class WebshopUserClaimsPrincipalFactory(UserManager<WebshopUser> userManager, RoleManager<IdentityRole> roleManager ,IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<WebshopUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(WebshopUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        var displayName = $"{user.GivenName} {user.LastName}".Trim();
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            identity.AddClaim(new Claim(WebshopClaimTypes.DisplayName, $"{displayName}"));
        }

        var isSuperUser = identity.HasClaim(c => c.Type == identity.RoleClaimType && c.Value == WebshopClaimTypes.SuperUser);
        if (isSuperUser)
        {
            identity.AddClaim(new Claim(WebshopClaimTypes.Permission, WebshopClaimTypes.SuperUser));
        }


        return identity;
    }
}
public static class WebshopClaimTypes
{
    public const string SuperUser = "super_user";

    public const string Permission = "permissions";

    public const string DisplayName = "display_name";
}