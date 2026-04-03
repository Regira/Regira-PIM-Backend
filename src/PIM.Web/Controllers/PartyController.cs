using Microsoft.AspNetCore.Mvc;
using PIM.Models.Stakeholders.Parties;
using Regira.Entities.Web.Controllers.Abstractions;

namespace PIM.Web.Controllers;

[ApiController, Route("parties")]
public class PartyController
    : EntityControllerBase<Party, PartySearchObject, PartySortBy, PartyIncludes, PartyDto, PartyInputDto>;
