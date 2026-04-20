using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PIM.Core.Constants;
using PIM.Identity.Models;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;

namespace PIM.Admin.API.Controllers;

[ApiController]
[Route("user")]
[Authorize(Policy = PimPolicies.AdminOnly)]
public class PimUserController : EntityControllerBase<PimUserEntity, string, PimUserSearchObject, EntitySortBy, PimUserIncludes, PimUserDto, PimUserInputDto>;
