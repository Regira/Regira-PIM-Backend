using PIM.Models.Taxonomy.FacetGroups;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Taxonomy;

public class FacetGroupQueryFilter : FilteredQueryBuilderBase<FacetGroup, int, FacetGroupSearchObject>
{
    public override IQueryable<FacetGroup> Build(IQueryable<FacetGroup> query, FacetGroupSearchObject? so)
    {
        if (so == null)
            return query;

        if (so.IsRoot.HasValue)
            query = query.Where(x => so.IsRoot == !x.ParentFacets!.Any());

        if (so.FacetId?.Any() == true)
        {
            query = query.Where(x =>
                x.ChildFacets!.Any(f => so.FacetId.Contains(f.FacetId))
                || x.ParentFacets!.Any(f => so.FacetId.Contains(f.FacetId))
            );
        }
        else
        {
            if (so.ChildFacetId?.Any() == true)
                query = query.Where(x => x.ChildFacets!.Any(f => so.ChildFacetId.Contains(f.FacetId)));
            if (so.ParentFacetId?.Any() == true)
                query = query.Where(x => x.ParentFacets!.Any(f => so.ParentFacetId.Contains(f.FacetId)));
        }

        return query;
    }
}
