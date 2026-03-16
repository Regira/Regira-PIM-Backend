using Microsoft.AspNetCore.Identity;
using Regira.Security.Authentication.Jwt.Abstraction;
using Regira.Security.Authentication.Web.Controllers;
using Webshop.Identity.Models;

namespace Webshop.Admin.API.Controllers;

public class AccountController(ITokenHelper tokenHelper, UserManager<WebshopUser> userManager, IUserClaimsPrincipalFactory<WebshopUser> claimsFactory, ILogger? logger = null)
    : AccountControllerBase<WebshopUser>(tokenHelper, userManager, claimsFactory, logger);