using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Core.Constants;
using Webshop.Identity.Models;

namespace Webshop.Manager.API.Controllers;

[ApiController]
[Route("users")]
[Authorize(Policy = WebshopPolicies.AdminOnly)]
public class WebshopUserController : EntityControllerBase<WebshopUserEntity, string, WebshopUserSearchObject, EntitySortBy, WebshopUserIncludes, WebshopUserDto, WebshopUserInputDto>;
