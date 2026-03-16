using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Regira.Security.Authentication.Jwt.Abstraction;
using Regira.Security.Authentication.Web.Constants;
using Regira.Security.Authentication.Web.Controllers;
using Regira.Security.Authentication.Web.Models;
using Regira.Web.Utilities;
using Webshop.Identity.Models;

namespace Webshop.Admin.API.Controllers;

public class AccountController(ITokenHelper tokenHelper, UserManager<WebshopUser> userManager, IUserClaimsPrincipalFactory<WebshopUser> claimsFactory, ILogger? logger = null)
    : AccountControllerBase<WebshopUser>(tokenHelper, userManager, claimsFactory, logger)
{
    [AllowAnonymous]
    [HttpPost]
    [Route("", Name = RouteNames.Authenticate)]
    public override async Task<IActionResult> Authenticate([FromBody] AuthenticateInput model, [FromQuery] string clientApp)
    {
        bool? isLockedOut = null;
        DateTimeOffset? lockedOutEnd = null;

        var user = await userManager.FindByNameAsync(model.Username);
        if (user != null)
        {
            isLockedOut = await userManager.IsLockedOutAsync(user);
            if (isLockedOut == false)
            {
                bool isAuthenticated = await userManager.CheckPasswordAsync(user, model.Password);
                if (isAuthenticated)
                {
                    var principal = await claimsFactory.CreateAsync(user);
                    return Ok(CreateSuccessResponse(principal.Claims, clientApp));
                }
                // authentication failed
                await userManager.AccessFailedAsync(user);
            }
            else
            {
                lockedOutEnd = await userManager.GetLockoutEndDateAsync(user);
                logger?.LogWarning($"User {user.Id} {Request.GetIPAddress()} locked out until {lockedOutEnd:HH:mm:ss}");
            }
        }

        return StatusCode(StatusCodes.Status401Unauthorized, CreateFailedResponse(isLockedOut, lockedOutEnd));
    }
}