using Microsoft.AspNetCore.Mvc;
using PIM.Models.Taxonomy.Facets;
using PIM.Services.Entities.Taxonomy.Abstractions;
using PIM.Web.Extensions;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using System.Diagnostics;

namespace PIM.Web.Controllers;

[ApiController, Route("facets")]
public class FacetController(IFacetService service)
    : EntityControllerBase<Facet, FacetSearchObject, EntitySortBy, FacetIncludes, FacetDto, FacetInputDto>
{
    [HttpGet("ancestors")]
    public async Task<IActionResult> GetAncestors([FromQuery] IList<int> facetIds, [FromQuery] IList<int>? groupIds = null, int level = 9)
    {
        var sw = new Stopwatch();
        sw.Start();
        var tree = await service.GetAncestors(facetIds, groupIds, level);
        sw.Stop();
        return Ok(tree.ToTreeViewListResult(sw.ElapsedMilliseconds));
    }

    [HttpGet("offspring")]
    public async Task<IActionResult> GetOffspring([FromQuery] IList<int> facetIds, [FromQuery] IList<int>? groupIds = null, int level = 9)
    {
        var sw = new Stopwatch();
        sw.Start();
        var tree = await service.GetOffspring(facetIds, groupIds, level);
        sw.Stop();
        return Ok(tree.ToTreeViewListResult(sw.ElapsedMilliseconds));
    }

    [HttpGet("family")]
    public async Task<IActionResult> GetFamily([FromQuery] IList<int> facetIds, [FromQuery] IList<int>? groupIds = null, int level = 9)
    {
        var sw = new Stopwatch();
        sw.Start();
        var tree = await service.GetFamily(facetIds, groupIds, level);
        sw.Stop();
        return Ok(tree.ToTreeViewListResult(sw.ElapsedMilliseconds));
    }
}
