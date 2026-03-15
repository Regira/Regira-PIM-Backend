using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Entities.Stakeholders.Parties.Relations;

namespace Webshop.Public.API.Controllers;

[ApiController, Route("relationship-types")]
public class RelationshipTypeController
    : EntityControllerBase<RelationshipType, RelationshipTypeDto, RelationshipTypeInputDto>;
