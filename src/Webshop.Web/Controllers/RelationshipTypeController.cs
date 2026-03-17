using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Stakeholders.Parties.Relations;

namespace Webshop.Web.Controllers;

[ApiController, Route("relationship-types")]
public class RelationshipTypeController
    : EntityControllerBase<RelationshipType, RelationshipTypeDto, RelationshipTypeInputDto>;
