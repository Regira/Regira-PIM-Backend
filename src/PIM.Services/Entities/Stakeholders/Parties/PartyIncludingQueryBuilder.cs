using Microsoft.EntityFrameworkCore;
using PIM.Models.Stakeholders.Parties;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

namespace PIM.Services.Entities.Stakeholders.Parties;

public class PartyIncludingQueryBuilder : IIncludableQueryBuilder<Party, int, PartyIncludes>
{
    public IQueryable<Party> AddIncludes(IQueryable<Party> query, PartyIncludes? includes = null)
    {
        if (includes == null)
            return query;

        if (includes.Value.HasFlag(PartyIncludes.ContactData))
            query = query.Include(x => x.ContactData);

        if (includes.Value.HasFlag(PartyIncludes.Addresses))
            query = query.Include(x => x.Addresses);

        if (includes.Value.HasFlag(PartyIncludes.Parents))
            query = query
                .Include(x => x.ParentRelationships!).ThenInclude(r => r.Parent!).ThenInclude(p => p.ContactData)
                .Include(x => x.ParentRelationships!).ThenInclude(r => r.RelationshipType);

        if (includes.Value.HasFlag(PartyIncludes.Children))
            query = query
                .Include(x => x.ChildRelationships!.OrderBy(r => r.SortOrder)).ThenInclude(r => r.Child!).ThenInclude(p => p.ContactData)
                .Include(x => x.ChildRelationships!.OrderBy(r => r.SortOrder)).ThenInclude(r => r.RelationshipType);

        return query;
    }
}
