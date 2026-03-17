using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Stakeholders.Parties;

namespace Webshop.Web.Controllers;

[ApiController, Route("parties")]
public class PartyController
    : EntityControllerBase<Party, PartySearchObject, PartySortBy, PartyIncludes, PartyDto, PartyInputDto>;
