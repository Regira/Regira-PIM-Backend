using Microsoft.AspNetCore.Identity;
using Regira.Security.Authentication.Web.Controllers;
using Regira.Serializing.Abstractions;
using Webshop.Identity.Models;

namespace Webshop.Admin.API.Controllers;

public class PasswordController(UserManager<WebshopUser> userManager, ISerializer serializer)
    : PasswordControllerBase<WebshopUser>(userManager, serializer);