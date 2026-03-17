using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Stakeholders.Parties;

namespace Webshop.Services.Entities.Stakeholders.Parties;

public class PartyQueryBuilder : FilteredQueryBuilderBase<Party, int, PartySearchObject>
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

        return query;
    }
}
