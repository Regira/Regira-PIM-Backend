using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models.Catalog.Products;
using PIM.Services.Entities.Catalog.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Services.Abstractions;
using Regira.TreeList;

namespace PIM.Services.Entities.Catalog.Products;


public class ProductRepository(
    PimDbContext dbContext,
    IEntityReadService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes> readService,
    IEntityWriteService<Product, int> writeService)
    : EntityRepository<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(readService, writeService), IProductRepository
{
    public async Task<TreeList<ProductTreeItem>> GetAncestors(IList<int> ids, int maxLevel = 9)
    {
        var items = await dbContext.GetProductAncestors(ids, maxLevel).ToListAsync();
        return items.ToTreeList(x => items.FindAll(p => p.ChildId == x.ParentId));
    }
    public async Task<TreeList<ProductTreeItem>> GetOffspring(IList<int> ids, int maxLevel = 9)
    {
        var items = await dbContext.GetProductOffspring(ids, maxLevel).ToListAsync();
        return items.ToTreeList(x => items.FindAll(p => p.ParentId == x.ChildId));
    }
    public async Task<TreeList<ProductTreeItem>> GetFamily(IList<int> ids, int maxLevel = 9)
    {
        var items = await dbContext.GetProductFamily(ids, maxLevel).ToListAsync();
        return items.ToTreeList(x => items.FindAll(p => p.ChildId == x.ParentId));
    }
}