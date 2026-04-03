using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PIM.Identity.Models;
using Regira.Security.Authentication.Web.Controllers;
using Regira.Serializing.Abstractions;

namespace PIM.Identity.Controllers;

[AllowAnonymous]
public class PasswordController(UserManager<PimIdentityUser> userManager, ISerializer serializer)
    : PasswordControllerBase<PimIdentityUser>(userManager, serializer);