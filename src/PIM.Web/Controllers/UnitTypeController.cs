using Microsoft.AspNetCore.Mvc;
using PIM.Models.Catalog.UnitTypes;
using Regira.Entities.Web.Controllers.Abstractions;

namespace PIM.Web.Controllers;

[ApiController, Route("unit-types")]
public class UnitTypeController : EntityControllerBase<UnitType, UnitTypeDto, UnitTypeInputDto>;
