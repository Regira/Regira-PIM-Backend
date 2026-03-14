using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Entities.Catalog.Parts;

namespace Webshop.Public.API.Controllers;

[ApiController, Route("parts")]
public class PartController
    : EntityControllerBase<Part, PartSearchObject, PartSortBy, PartIncludes, PartDto, PartInputDto>;
