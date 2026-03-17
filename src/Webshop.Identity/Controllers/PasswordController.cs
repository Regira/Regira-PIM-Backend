using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Regira.Security.Authentication.Web.Controllers;
using Regira.Serializing.Abstractions;
using Webshop.Identity.Models;

namespace Webshop.Identity.Controllers;

[AllowAnonymous]
public class PasswordController(UserManager<WebshopIdentityUser> userManager, ISerializer serializer)
    : PasswordControllerBase<WebshopIdentityUser>(userManager, serializer);