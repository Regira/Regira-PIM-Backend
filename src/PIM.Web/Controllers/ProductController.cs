using Microsoft.AspNetCore.Mvc;
using PIM.Models.Catalog.Products;
using PIM.Models.Catalog.Products.DTO;
using PIM.Services.Entities.Catalog.Abstractions;
using PIM.Web.Extensions;
using Regira.Entities.Web.Controllers.Abstractions;
using System.Diagnostics;

namespace PIM.Web.Controllers;

[ApiController, Route("products")]
public class ProductController(IProductService service)
    : EntityControllerBase<Product, ProductSearchObject, ProductSortBy, ProductIncludes, ProductDto, ProductInputDto>
{
    [HttpGet("ancestors")]
    public async Task<IActionResult> GetAncestors(IList<int> ids, int level = 9)
    {
        var sw = new Stopwatch();
        sw.Start();
        var tree = await service.GetAncestors(ids, level);
        sw.Stop();
        return Ok(tree.ToTreeNodeListResult(sw.ElapsedMilliseconds));
    }

    [HttpGet("offspring")]
    public async Task<IActionResult> GetOffspring(IList<int> ids, int level = 9)
    {
        var sw = new Stopwatch();
        sw.Start();
        var tree = await service.GetOffspring(ids, level);
        sw.Stop();
        return Ok(tree.ToTreeNodeListResult(sw.ElapsedMilliseconds));
    }

    [HttpGet("family")]
    public async Task<IActionResult> GetFamily(IList<int> ids, int level = 9)
    {
        var sw = new Stopwatch();
        sw.Start();
        var tree = await service.GetFamily(ids, level);
        sw.Stop();
        return Ok(tree.ToTreeNodeListResult(sw.ElapsedMilliseconds));
    }
}