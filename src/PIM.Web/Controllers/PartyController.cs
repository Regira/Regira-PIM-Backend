using Microsoft.AspNetCore.Mvc;
using PIM.Models.Stakeholders.Parties;
using PIM.Models.Stakeholders.Parties.DTO;
using PIM.Services.Entities.Stakeholders.Abstractions;
using PIM.Web.Extensions;
using Regira.Entities.Web.Controllers.Abstractions;
using System.Diagnostics;

namespace PIM.Web.Controllers;

[ApiController, Route("parties")]
public class PartyController(IPartyService service)
    : EntityControllerBase<Party, PartySearchObject, PartySortBy, PartyIncludes, PartyDto, PartyInputDto>
{
    [HttpGet("ancestors")]
    public async Task<IActionResult> GetAncestors([FromQuery] IList<int> ids, [FromQuery] int level = 9)
    {
        var sw = new Stopwatch();
        sw.Start();
        var tree = await service.GetAncestors(ids, level);
        sw.Stop();
        return Ok(tree.ToTreeViewListResult(sw.ElapsedMilliseconds));
    }

    [HttpGet("offspring")]
    public async Task<IActionResult> GetOffspring([FromQuery] IList<int> ids, [FromQuery] int level = 9)
    {
        var sw = new Stopwatch();
        sw.Start();
        var tree = await service.GetOffspring(ids, level);
        sw.Stop();
        return Ok(tree.ToTreeViewListResult(sw.ElapsedMilliseconds));
    }

    [HttpGet("family")]
    public async Task<IActionResult> GetFamily([FromQuery] IList<int> ids, [FromQuery] int level = 9)
    {
        var sw = new Stopwatch();
        sw.Start();
        var tree = await service.GetFamily(ids, level);
        sw.Stop();
        return Ok(tree.ToTreeViewListResult(sw.ElapsedMilliseconds));
    }
}
