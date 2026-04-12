using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models.Taxonomy.Facets;
using PIM.Services.Entities.Taxonomy.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Regira.TreeList;

namespace PIM.Services.Entities.Taxonomy;

public class FacetRepository(
    PimDbContext dbContext,
    IEntityReadService<Facet, int, FacetSearchObject, EntitySortBy, FacetIncludes> readService,
    IEntityWriteService<Facet, int> writeService)
    : EntityRepository<Facet, FacetSearchObject, EntitySortBy, FacetIncludes>(readService, writeService), IFacetRepository
{
    public async Task<TreeList<FacetTreeItem>> GetAncestors(IList<int> facetIds, IList<int>? groupIds = null, int maxLevel = 9)
    {
        var items = await dbContext.GetFacetAncestors(facetIds, groupIds, maxLevel).ToListAsync();
        return items.ToTreeList(x => items.FindAll(p => p.ChildId == x.ParentId));
    }
    public async Task<TreeList<FacetTreeItem>> GetOffspring(IList<int> facetIds, IList<int>? groupIds = null, int maxLevel = 9)
    {
        var items = await dbContext.GetFacetOffspring(facetIds, groupIds, maxLevel).ToListAsync();
        return items.ToTreeList(x => items.FindAll(p => p.ParentId == x.ChildId));
    }
    public async Task<TreeList<FacetTreeItem>> GetFamily(IList<int> facetIds, IList<int>? groupIds = null, int maxLevel = 9)
    {
        var items = await dbContext.GetFacetFamily(facetIds, groupIds, maxLevel).ToListAsync();
        return items.ToTreeList(x => items.FindAll(p => p.ChildId == x.ParentId));
    }
}
