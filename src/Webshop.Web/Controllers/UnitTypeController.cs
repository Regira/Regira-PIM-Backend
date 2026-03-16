using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Entities.Catalog.UnitTypes;

namespace Webshop.Web.Controllers;

[ApiController, Route("unit-types")]
public class UnitTypeController : EntityControllerBase<UnitType, UnitTypeDto, UnitTypeInputDto>;
