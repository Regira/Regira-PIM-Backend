using PIM.Data;
using PIM.Models.Taxonomy.Facets;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Taxonomy;

public class FacetQueryFilter(PimDbContext dbContext) : FilteredQueryBuilderBase<Facet, int, FacetSearchObject>
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
        if (so.IsParent != null)
            query = query.Where(x => so.IsParent == (x.ChildEntities!.Any() || x.FacetChildGroups!.Any()));
        if (so.IsChild != null)
            query = query.Where(x => so.IsChild == (x.ParentEntities!.Any() || x.FacetParentGroups!.Any()));

        if (so.AncestorId?.Any() == true)
        {
            var offspring = dbContext.GetFacetOffspring(so.AncestorId);
            query = query.Where(x => offspring.Any(r => r.ChildId == x.Id && r.ChildType == nameof(Facet)));
        }
        if (so.OffspringId?.Any() == true)
        {
            var ancestors = dbContext.GetFacetAncestors(so.OffspringId);
            query = query.Where(x => ancestors.Any(r => r.ParentId == x.Id && r.ParentType == nameof(Facet)));
        }
        if (so.RootId?.Any() == true)
        {
            var offspring = dbContext.GetFacetOffspring(so.RootId);
            query = query.Where(x => so.RootId.Contains(x.Id) || offspring.Any(r => r.ChildId == x.Id && r.ChildType == nameof(Facet)));
        }

        if (so.AncestorGroupId?.Any() == true)
        {
            var offspring = dbContext.GetFacetOffspring(null, so.AncestorGroupId);
            query = query.Where(x => offspring.Any(r => r.ChildId == x.Id && r.ChildType == nameof(Facet)));
        }
        if (so.OffspringGroupId?.Any() == true)
        {
            var ancestors = dbContext.GetFacetAncestors(null, so.OffspringGroupId);
            query = query.Where(x => ancestors.Any(r => r.ParentId == x.Id && r.ParentType == nameof(Facet)));
        }
        if (so.RootGroupId?.Any() == true)
        {
            var offspring = dbContext.GetFacetOffspring(null, so.RootGroupId);
            query = query.Where(x => offspring.Any(r => r.ChildId == x.Id && r.ChildType == nameof(Facet)));
        }

        return query;
    }
}
