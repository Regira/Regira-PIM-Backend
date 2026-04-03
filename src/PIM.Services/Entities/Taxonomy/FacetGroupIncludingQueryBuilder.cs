using Microsoft.EntityFrameworkCore;
using PIM.Models.Taxonomy.FacetGroups;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Taxonomy;

public class FacetGroupIncludingQueryBuilder : IIncludableQueryBuilder<FacetGroup, int, FacetGroupIncludes>
{
    public IQueryable<FacetGroup> AddIncludes(IQueryable<FacetGroup> query, FacetGroupIncludes? includes = null)
    {
        if (includes == null)
            return query;

        if (includes.Value.HasFlag(FacetGroupIncludes.ChildFacets))
            query = query.Include(x => x.ChildFacets!).ThenInclude(x => x.Facet);

        if (includes.Value.HasFlag(FacetGroupIncludes.ParentFacets))
            query = query.Include(x => x.ParentFacets!).ThenInclude(x => x.Facet);

        return query;
    }
}
