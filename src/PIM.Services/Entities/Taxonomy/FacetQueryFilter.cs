using PIM.Data;
using PIM.Models.Taxonomy.Facets;
using Regira.Entities.QueryBuilders.Abstractions;

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
        if (so.ParentGroupId?.Any() == true)
            query = query.Where(x => x.FacetParentGroups!.Any(fg => so.ParentGroupId.Contains(fg.FacetGroupId)));
        if (so.ChildGroupId?.Any() == true)
            query = query.Where(x => x.FacetChildGroups!.Any(fg => so.ChildGroupId.Contains(fg.FacetGroupId)));

        // Branch on the flag instead of comparing it to the expression: comparing emits
        // "@flag = CASE WHEN EXISTS (...) THEN 1 ELSE 0 END", which the parameter stops SQL Server
        // folding, so it evaluates the subquery per row rather than running an anti-semi-join.
        if (so.IsRoot == true)
            query = query.Where(x => !x.ParentEntities!.Any());
        else if (so.IsRoot == false)
            query = query.Where(x => x.ParentEntities!.Any());

        if (so.IsParent == true)
            query = query.Where(x => x.ChildEntities!.Any() || x.FacetChildGroups!.Any());
        else if (so.IsParent == false)
            query = query.Where(x => !x.ChildEntities!.Any() && !x.FacetChildGroups!.Any());

        if (so.IsChild == true)
            query = query.Where(x => x.ParentEntities!.Any() || x.FacetParentGroups!.Any());
        else if (so.IsChild == false)
            query = query.Where(x => !x.ParentEntities!.Any() && !x.FacetParentGroups!.Any());

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
