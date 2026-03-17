using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Regira.Security.Authentication.Jwt.Abstraction;
using Regira.Security.Authentication.Web.Controllers;
using Webshop.Identity.Models;

namespace Webshop.Identity.Controllers;

[AllowAnonymous]
public class AccountController(ITokenHelper tokenHelper, UserManager<WebshopIdentityUser> userManager, IUserClaimsPrincipalFactory<WebshopIdentityUser> claimsFactory, ILogger? logger = null)
    : AccountControllerBase<WebshopIdentityUser>(tokenHelper, userManager, claimsFactory, logger);
