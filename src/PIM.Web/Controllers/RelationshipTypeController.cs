using Microsoft.AspNetCore.Mvc;
using PIM.Models.Stakeholders.Parties.Relations;
using Regira.Entities.Web.Controllers.Abstractions;

namespace PIM.Web.Controllers;

[ApiController, Route("relationship-types")]
public class RelationshipTypeController
    : EntityControllerBase<RelationshipType, RelationshipTypeDto, RelationshipTypeInputDto>;
