using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PIM.Identity.Models;
using Regira.Security.Authentication.Web.Controllers;

namespace PIM.Identity.Controllers;

[AllowAnonymous]
public class PasswordController(UserManager<PimIdentityUser> userManager)
    : PasswordControllerBase<PimIdentityUser>(userManager);