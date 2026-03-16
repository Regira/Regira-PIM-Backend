using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Identity.Entities;

namespace Webshop.Admin.API.Controllers;

[ApiController]
[Route("users")]
public class WebshopUserController : EntityControllerBase<WebshopUserEntity, string, WebshopUserSearchObject, EntitySortBy, WebshopUserIncludes, WebshopUserDto, WebshopUserInputDto>;
