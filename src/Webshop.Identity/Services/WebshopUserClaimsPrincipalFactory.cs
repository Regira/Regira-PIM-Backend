using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Webshop.Core.Constants;
using Webshop.Identity.Models;

namespace Webshop.Identity.Services;

public class WebshopUserClaimsPrincipalFactory(UserManager<WebshopIdentityUser> userManager, RoleManager<IdentityRole> roleManager ,IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<WebshopIdentityUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(WebshopIdentityUser user)
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