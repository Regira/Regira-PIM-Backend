using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PIM.Core.Abstractions;
using PIM.Core.Constants;
using PIM.Identity.Models;
using System.Security.Claims;

namespace PIM.Identity.Services;

public class PimUserClaimsPrincipalFactory(UserManager<PimIdentityUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> options, ICultureContext cultureContext)
    : UserClaimsPrincipalFactory<PimIdentityUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(PimIdentityUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        var displayName = $"{user.GivenName} {user.LastName}".Trim();
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            identity.AddClaim(new Claim(PimClaimTypes.DisplayName, $"{displayName}"));
        }

        var isSuperUser = identity.HasClaim(c => c.Type == identity.RoleClaimType && c.Value == PimClaimTypes.SuperUser);
        if (isSuperUser)
        {
            identity.AddClaim(new Claim(PimClaimTypes.Permission, PimClaimTypes.SuperUser));
        }

        identity.AddClaim(new Claim(PimClaimTypes.Culture, cultureContext.Culture.Name));


        return identity;
    }
}