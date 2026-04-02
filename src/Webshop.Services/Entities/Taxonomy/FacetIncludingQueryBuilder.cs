using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Taxonomy.Facets;

namespace Webshop.Services.Entities.Taxonomy;

public class FacetIncludingQueryBuilder : IIncludableQueryBuilder<Facet, int, FacetIncludes>
{
    public IQueryable<Facet> AddIncludes(IQueryable<Facet> query, FacetIncludes? includes = null)
    {
        if (includes == null)
            return query;

        if (includes.Value.HasFlag(FacetIncludes.Parents))
            query = query.Include(x => x.ParentEntities!).ThenInclude(x => x.Parent);
        if (includes.Value.HasFlag(FacetIncludes.Children))
            query = query.Include(x => x.ChildEntities!).ThenInclude(x => x.Child);
        if (includes.Value.HasFlag(FacetIncludes.FacetGroups))
            query = query.Include(x => x.FacetParentGroups!).ThenInclude(x => x.FacetGroup);

        return query;
    }
}
