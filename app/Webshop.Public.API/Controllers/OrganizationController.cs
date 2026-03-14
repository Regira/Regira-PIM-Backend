using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Entities.Clients.Organizations;

namespace Webshop.Public.API.Controllers;

[ApiController, Route("organizations")]
public class OrganizationController
    : EntityControllerBase<Organization, OrganizationDto, OrganizationInputDto>;
