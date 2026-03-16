using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Admin.Entities;

namespace Webshop.Admin.API.Controllers;

[ApiController]
[Route("users")]
public class UserController : EntityControllerBase<WebshopUserEntity, string, WebshopUserSearchObject, EntitySortBy, WebshopUserIncludes, WebshopUserDto, WebshopUserInputDto>;
