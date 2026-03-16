using Microsoft.AspNetCore.Identity;
using Regira.Security.Authentication.Web.Controllers;
using Regira.Serializing.Abstractions;
using Webshop.Identity.Models;

namespace Webshop.Identity.Controllers;

public class PasswordController(UserManager<WebshopUser> userManager, ISerializer serializer)
    : PasswordControllerBase<WebshopUser>(userManager, serializer);