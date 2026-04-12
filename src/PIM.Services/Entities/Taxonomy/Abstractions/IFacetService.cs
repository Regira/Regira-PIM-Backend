using PIM.Models.Taxonomy.Facets;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Regira.TreeList;

namespace PIM.Services.Entities.Taxonomy.Abstractions;

public interface IFacetService : IEntityService<Facet, FacetSearchObject, EntitySortBy, FacetIncludes>
{
    Task<TreeList<FacetTreeItem>> GetAncestors(IList<int> facetIds, IList<int>? groupIds = null, int maxLevel = 9);
    Task<TreeList<FacetTreeItem>> GetOffspring(IList<int> facetIds, IList<int>? groupIds = null, int maxLevel = 9);
    Task<TreeList<FacetTreeItem>> GetFamily(IList<int> facetIds, IList<int>? groupIds = null, int maxLevel = 9);
}
