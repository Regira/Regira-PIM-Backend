using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Taxonomy.Facets;

namespace Webshop.Services.Entities.Taxonomy;

public class FacetQueryFilter : FilteredQueryBuilderBase<Facet, int, FacetSearchObject>
{
    public override IQueryable<Facet> Build(IQueryable<Facet> query, FacetSearchObject? so)
    {
        if (so == null)
            return query;

        if (so.ParentId?.Any() == true)
            query = query.Where(x => x.ParentEntities!.Any(pe => so.ParentId.Contains(pe.ParentId)));
        if (so.ChildId?.Any() == true)
            query = query.Where(x => x.ChildEntities!.Any(ce => so.ChildId.Contains(ce.ChildId)));
        if (so.FacetGroupId?.Any() == true)
            query = query.Where(x => x.FacetParentGroups!.Any(fg => so.FacetGroupId.Contains(fg.FacetGroupId)));
        if (so.IsRoot != null)
            query = query.Where(x => so.IsRoot == !x.ParentEntities!.Any());

        return query;
    }
}
