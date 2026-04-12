using Microsoft.AspNetCore.Mvc;
using PIM.Models.Stakeholders.Parties;
using PIM.Models.Stakeholders.Parties.DTO;
using Regira.Entities.Web.Controllers.Abstractions;

namespace PIM.Web.Controllers;

[ApiController, Route("relationship-types")]
public class RelationshipTypeController
    : EntityControllerBase<RelationshipType, RelationshipTypeDto, RelationshipTypeInputDto>;
