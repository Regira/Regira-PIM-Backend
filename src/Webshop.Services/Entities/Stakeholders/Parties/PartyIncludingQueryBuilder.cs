using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Webshop.Models.Stakeholders.Parties;

namespace Webshop.Services.Entities.Stakeholders.Parties;

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

        if (includes.Value.HasFlag(PartyIncludes.Relationships))
            query = query
                .Include(x => x.ChildRelationships!).ThenInclude(r => r.RelationshipType)
                .Include(x => x.ParentRelationships!).ThenInclude(r => r.RelationshipType);

        return query;
    }
}
