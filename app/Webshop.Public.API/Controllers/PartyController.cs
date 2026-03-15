using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Entities.Stakeholders.Parties;

namespace Webshop.Public.API.Controllers;

[ApiController, Route("parties")]
public class PartyController
    : EntityControllerBase<Party, PartySearchObject, PartySortBy, PartyIncludes, PartyDto, PartyInputDto>;
