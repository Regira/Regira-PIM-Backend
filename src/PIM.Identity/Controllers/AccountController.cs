using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PIM.Identity.Models;
using Regira.Security.Authentication.Jwt.Abstraction;
using Regira.Security.Authentication.Web.Controllers;

namespace PIM.Identity.Controllers;

public class AccountController(ITokenHelper tokenHelper, UserManager<PimIdentityUser> userManager, IUserClaimsPrincipalFactory<PimIdentityUser> claimsFactory, ILogger? logger = null)
    : AccountControllerBase<PimIdentityUser>(tokenHelper, userManager, claimsFactory, logger);
