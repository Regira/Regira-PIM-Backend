using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PIM.Models.Catalog.Products;
using PIM.Models.Catalog.Products.DTO;
using PIM.Services.Entities.Catalog.Abstractions;
using PIM.Web.Extensions;
using Regira.Entities.Web.Controllers.Abstractions;

namespace PIM.Web.Controllers;

[ApiController, Route("products")]
public class ProductController(IProductService service)
    : EntityControllerBase<Product, ProductSearchObject, ProductSortBy, ProductIncludes, ProductDto, ProductInputDto>
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