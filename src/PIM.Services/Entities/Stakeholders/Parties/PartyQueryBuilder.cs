using PIM.Data;
using PIM.Models.Catalog.Products;
using PIM.Models.Stakeholders.Parties;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Stakeholders.Parties;

public class PartyQueryBuilder(PimDbContext dbContext) : FilteredQueryBuilderBase<Party, int, PartySearchObject>
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

        if (so.IsRoot.HasValue)
            query = query.Where(x => so.IsRoot.Value == !x.ParentRelationships!.Any());
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
