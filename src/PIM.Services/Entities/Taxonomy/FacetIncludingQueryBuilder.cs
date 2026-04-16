using Microsoft.EntityFrameworkCore;
using PIM.Models.Taxonomy.Facets;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Taxonomy;

public class FacetIncludingQueryBuilder : IIncludableQueryBuilder<Facet, int, FacetIncludes>
{
    public IQueryable<Facet> AddIncludes(IQueryable<Facet> query, FacetIncludes? includes = null)
    {
        if (includes == null)
            return query;

        if (includes.Value.HasFlag(FacetIncludes.Parents))
            query = query.Include(x => x.ParentEntities!.Where(f => !f.Parent!.IsArchived)).ThenInclude(x => x.Parent);
        if (includes.Value.HasFlag(FacetIncludes.Children))
            query = query.Include(x => x.ChildEntities!.Where(f => !f.Child!.IsArchived)).ThenInclude(x => x.Child);
        if (includes.Value.HasFlag(FacetIncludes.FacetGroups))
            query = query.Include(x => x.FacetParentGroups!.Where(f => !f.FacetGroup!.IsArchived)).ThenInclude(x => x.FacetGroup);

        return query;
    }
}
