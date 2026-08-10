using PIM.Data;
using PIM.Models.Catalog.Products;
using PIM.Models.Stakeholders.Parties;
using Regira.Entities.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Stakeholders.Parties;

public class PartyQueryFilter(PimDbContext dbContext) : FilteredQueryBuilderBase<Party, int, PartySearchObject>
{
    public override IQueryable<Party> Build(IQueryable<Party> query, PartySearchObject? so)
    {
        if (so == null)
            return query;

        if (!string.IsNullOrWhiteSpace(so.PartyType))
            query = query.Where(x => x.PartyType == so.PartyType);

        if (!string.IsNullOrWhiteSpace(so.Query))
            query = query.Where(x => x.NormalizedContent!.Contains(so.Query) || x.NormalizedTitle!.Contains(so.Query));

        if (!string.IsNullOrWhiteSpace(so.Code))
            query = query.Where(x => x.Code == so.Code);

        if (so.RelationshipId?.Any() == true)
            query = query.Where(x =>
                x.ChildRelationships!.Any(r => so.RelationshipId.Contains(r.RelationshipTypeId)) ||
                x.ParentRelationships!.Any(r => so.RelationshipId.Contains(r.RelationshipTypeId)));

        // Branch on the flag instead of comparing it to the expression: comparing emits
        // "@flag = CASE WHEN EXISTS (...) THEN 1 ELSE 0 END", which the parameter stops SQL Server
        // folding, so it evaluates the subquery per row rather than running an anti-semi-join.
        if (so.IsRoot == true)
            query = query.Where(x => !x.ParentRelationships!.Any());
        else if (so.IsRoot == false)
            query = query.Where(x => x.ParentRelationships!.Any());

        if (so.IsParent == true)
            query = query.Where(x => x.ChildRelationships!.Any());
        else if (so.IsParent == false)
            query = query.Where(x => !x.ChildRelationships!.Any());

        if (so.IsChild == true)
            query = query.Where(x => x.ParentRelationships!.Any());
        else if (so.IsChild == false)
            query = query.Where(x => !x.ParentRelationships!.Any());

        if (so.AncestorId?.Any() == true)
            query = query.Where(x => dbContext.GetPartyOffspring(so.AncestorId, 9).Any(o => o.ChildId == x.Id));
        if (so.OffspringId?.Any() == true)
            query = query.Where(x => dbContext.GetPartyAncestors(so.OffspringId, 9).Any(o => o.ParentId == x.Id));
        if (so.RootId?.Any() == true)
            query = query.Where(x => dbContext.GetPartyOffspring(so.RootId, 9).Any(o => o.RootId == x.Id));

        if (so.ProductIdSupplied?.Any() == true)
            query = query.Where(x => dbContext.Set<ProductSupplier>().Any(ps => ps.SupplierId == x.Id && so.ProductIdSupplied.Contains(ps.ProductId)));


        return query;
    }
}
