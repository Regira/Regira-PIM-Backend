using PIM.Data;
using PIM.Models.Taxonomy.Facets;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Regira.TreeList;

namespace PIM.Services.Entities.Taxonomy.Abstractions;

public interface IFacetService : IEntityService<Facet, FacetSearchObject, EntitySortBy, FacetIncludes>
{
    Task<TreeList<FacetRelationResult>> GetAncestors(IList<int> facetIds, IList<int>? groupIds = null, int maxLevel = 9);
    Task<TreeList<FacetRelationResult>> GetOffspring(IList<int> facetIds, IList<int>? groupIds = null, int maxLevel = 9);
    Task<TreeList<FacetRelationResult>> GetFamily(IList<int> facetIds, IList<int>? groupIds = null, int maxLevel = 9);
}
